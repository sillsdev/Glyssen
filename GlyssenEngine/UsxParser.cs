using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using SIL.DblBundle;
using SIL.DblBundle.Usx;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Xml;
using static System.Char;
using static System.String;

namespace GlyssenEngine
{
	public class UsxParser
	{
		public static List<BookScript> ParseBooks(IEnumerable<UsxDocument> books, IStylesheet stylesheet,
			ICharacterUsageStore characterUsageStore, Action<int> reportProgressAsPercent)
		{
			var numBlocksPerBook = new ConcurrentDictionary<string, int>();
			var blocksInBook = new ConcurrentDictionary<string, XmlNodeList>();
			Parallel.ForEach(books, usxDoc =>
			{
				var nodeList = usxDoc.GetChaptersAndParas();
				blocksInBook.AddOrUpdate(usxDoc.BookId, nodeList, (s, list) => nodeList);
				numBlocksPerBook.AddOrUpdate(usxDoc.BookId, nodeList.Count, (s, i) => nodeList.Count);
			});
			int allBlocks = numBlocksPerBook.Values.Sum();

			int completedBlocks = 0;
			var bookScripts = new List<BookScript>(blocksInBook.Count);
			Parallel.ForEach(blocksInBook, book =>
			{
				var bookId = book.Key;
				var bookScript = new UsxParser(bookId, stylesheet, characterUsageStore, book.Value).CreateBookScript();
				if (bookScript != null)
				{
					lock (bookScripts)
						bookScripts.Add(bookScript);
					Logger.WriteEvent("Added bookScript ({0}, {1})", bookId, bookScript.BookId);
					completedBlocks += numBlocksPerBook[bookId];
					reportProgressAsPercent?.Invoke(MathUtilities.Percent(completedBlocks, allBlocks, 99));
				}
			});

			// This code is an attempt to figure out how we are getting null reference exceptions on the Sort call (See PG-275 & PG-287)
			// The above call to lock bookScripts probably fixes the problem!!! :-) We hope...
			foreach (var bookScript in bookScripts)
				if (bookScript?.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException($"{initialMessage} Number of BookScripts: {bookScripts.Count}. " +
						$"BookScripts which are NOT null: {nonNullBookScriptsStr}");
				}

			try
			{
				bookScripts.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));
			}
			catch (NullReferenceException n)
			{
				// This code is an attempt to figure out how we are getting null reference exceptions on the Sort call (See PG-275 & PG-287)
				StringBuilder sb = new StringBuilder();
				foreach (var bookScript in bookScripts)
					sb.Append(Environment.NewLine).Append(bookScript.BookId).Append("(").Append(BCVRef.BookToNumber(bookScript.BookId)).Append(")");
				throw new NullReferenceException("Null reference exception while sorting books." + sb, n);
			}

			reportProgressAsPercent?.Invoke(100);
			return bookScripts;
		}

		private BookScript CreateBookScript()
		{
			Logger.WriteEvent("Creating bookScript ({0})", m_bookId);
			var blocks = Parse();
			if (!blocks.Any())
				return null;
			var bookScript = new BookScript(m_bookId, blocks, null)
			{
				PageHeader = PageHeader,
				MainTitle = MainTitle
			};
			Logger.WriteEvent("Created bookScript ({0}, {1})", m_bookId, bookScript.BookId);
			return bookScript;
		}

		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IStylesheet m_stylesheet;
		private readonly ICharacterUsageStore m_characterUsageStore;
		private readonly XmlNodeList m_nodeList;

		private string m_bookLevelChapterLabel;
		private int m_currentChapter;
		private int m_currentStartVerse;
		private int m_currentEndVerse;
		private ExplicitQuoteInfo m_currentExplicitQuote;
		private ExplicitInterruptionInfo m_currentExplicitInterruption;

		/// <summary>
		/// Information about an explicit quote that is marked up in the USFM (using
		/// a qt-s marker) that allows the parser to keep track of state.
		/// </summary>
		private class ExplicitQuoteInfo
		{
			/// <summary>
			/// The first block of the explicit quote
			/// </summary>
			protected internal Block StartBlock { get; set; }
			/// <summary>
			/// The explicit quote ID in the USFM data, if any (may be null)
			/// </summary>
			protected internal string Id { get; }
			/// <summary>
			/// The character name/label/description specified in the USFM data, if any (may be
			/// null)
			/// </summary>
			protected internal string SpecifiedCharacter { get; }
			/// <summary>
			/// Flag indicating whether the specified character, if supplied, has been resolved to
			/// either a known ("official") character ID or a "Needs Review". (If no character was
			/// specified in the data, this is always false.)
			/// </summary>
			protected internal bool Resolved { get; set; }
			/// <summary>
			/// Quote nesting level. Since the parser only cares about first-level quotes, this is
			/// usually 1. However, in the case of a nested interruption, it can be greater than 1.
			/// </summary>
			protected internal virtual uint Level => 1;
			/// <summary>
			/// If a quote (or more typically an interruption) is opened without the current open
			/// quote being explicitly closed, this is the quote that was open when this quote or
			/// interruption starts.
			/// </summary>
			protected internal ExplicitQuoteInfo PreviouslyOpenQuote { get; }

			internal ExplicitQuoteInfo(Block startBlock, string quoteId, string character, ExplicitQuoteInfo previouslyOpenQuote)
			{
				StartBlock = startBlock;
				PreviouslyOpenQuote = previouslyOpenQuote;
				Id = quoteId;
				SpecifiedCharacter = character;
				Resolved = false;
			}

			public string ToString(string bookId)
			{
				var sb = new StringBuilder("Level ");
				sb.Append(Level);
				sb.Append(" quote ");
				if (Id != null)
				{
					sb.Append("with ID ");
					sb.Append(Id);
					sb.Append(" ");
				}
				if (SpecifiedCharacter != null)
				{
					sb.Append(" and character ");
					sb.Append(SpecifiedCharacter);
					sb.Append(" ");
				}

				sb.Append(" starting in block ");
				sb.Append(StartBlock.ToString(true, bookId));
				return sb.ToString();
			}
		}

		/// <summary>
		/// A class for the special case where the "quote" is actually an
		/// interruption (by the narrator) of a quote.
		/// </summary>
		private class ExplicitInterruptionInfo : ExplicitQuoteInfo
		{
			protected internal override uint Level { get; }
			internal string OriginalStyleTag => PreviouslyOpenQuote?.StartBlock?.StyleTag;

			internal ExplicitInterruptionInfo(Block startBlock, string quoteId, string bookId,
				ExplicitQuoteInfo previouslyOpenQuote, uint level) :
				base(startBlock, quoteId, CharacterVerseData.GetStandardCharacterId(bookId,
					CharacterVerseData.StandardCharacter.Narrator), previouslyOpenQuote)
			{
				Level = level;
			}

		}

		private static readonly Regex s_regexLeadingNonInitialPunctuationWithAdjacentWhitespace = new Regex(@"^\s*(\p{Pd}|\p{Pe}|\p{Pf}|\p{Po})*\s+", RegexOptions.Compiled);
		private static readonly Regex s_regexEndsWithOpeningQuote = new Regex("(\\p{Pi}\\s?)*(\\p{Pi}|\")+$", RegexOptions.Compiled);
		private const string kOptionalBookRegex = "(-(?<book>[1-3]?[A-Z]{2,3}))?$";
		private static readonly Regex s_regexInterruptionCharacter = new Regex($"^interruption{kOptionalBookRegex}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex s_regexNarratorCharacter = new Regex($"^narr(ator)?{kOptionalBookRegex}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public UsxParser(string bookId, IStylesheet stylesheet, ICharacterUsageStore characterUsageStore, XmlNodeList nodeList)
		{
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(m_bookId);
			m_stylesheet = stylesheet;
			m_characterUsageStore = characterUsageStore;
			m_nodeList = nodeList;
		}

		public string PageHeader { get; private set; }
		public string MainTitle { get; private set; }

		public IEnumerable<Block> Parse()
		{
			var titleBuilder = new StringBuilder();
			IList<Block> blocks = new List<Block>();
			foreach (XmlNode node in m_nodeList)
			{
				Block block = null;
				switch (node.Name)
				{
					case UsxNode.kChapterNodeName:
						AddMainTitleIfApplicable(blocks, titleBuilder);
						block = ProcessChapterNode(node);
						if (block == null)
							continue;
						break;
					case UsxNode.kParaNodeName:
						if (!node.HasChildNodes)
							continue;

						var usxPara = new UsxPara(node);
						IStyle style = m_stylesheet.GetStyle(usxPara.StyleTag);
						if (style.IsChapterLabel)
						{
							block = ProcessChapterLabelNode(node.InnerText, usxPara, blocks);
							break;
						}

						if (style.IsParallelPassageReference || !style.IsPublishable)
							continue;

						if (style.HoldsBookNameOrAbbreviation)
						{
							if (style.Id.StartsWith("mt"))
							{
								titleBuilder.Append(node.InnerText).Append(" ");
								if (style.Id == "mt1")
									MainTitle = node.InnerText;
							}
							else if (style.Id == "h")
								PageHeader = node.InnerText;
							continue;
						}
						AddMainTitleIfApplicable(blocks, titleBuilder);

						block = new Block(usxPara.StyleTag, m_currentChapter, m_currentStartVerse, m_currentEndVerse) { IsParagraphStart = true };
						if (m_currentChapter == 0)
							block.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Intro);
						else if (style.IsPublishable && !style.IsVerseText)
						{
							if (m_currentExplicitQuote?.StartBlock != null)
							{
								if (m_currentExplicitQuote.StartBlock != null && blocks.Last() != m_currentExplicitQuote.StartBlock)
									m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
								m_currentExplicitQuote.StartBlock = null;
							}

							block.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.ExtraBiblical);
						}
						else if (StyleToCharacterMappings.TryGetCharacterForParaStyle(style.Id, m_bookId, out var character))
						{
							// ENHANCE (PG-1322): If we ever decide to make it so section heads do
							// not interrupt multi-block quotes, this is one place that will need
							// to change.
							if (m_currentExplicitQuote != null)
							{
								// Break off any unclosed explicit quote initiated by a milestone quote marker.
								if (m_currentExplicitQuote.StartBlock != null &&
									blocks.Last() != m_currentExplicitQuote.StartBlock)
								{
									m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
								}
								m_currentExplicitQuote = null;
							}

							block.SetNonDramaticCharacterId(character);
						}
						else if (m_currentExplicitQuote != null && m_currentExplicitQuote.StartBlock == null)
						{
							Debug.Assert(blocks.Last().CharacterIsStandard, "This should only " +
								"happen when restarting a long quote that carries over past chapter" +
								"and/or section head breaks");
							block.SetCharacterAndDeliveryInfo(blocks.Last(b => !b.CharacterIsStandard),
								m_bookNum, m_characterUsageStore.Versification);
							m_currentExplicitQuote.StartBlock = block;
						}

						var sb = new StringBuilder();
						// <verse number="1" style="v" />
						// Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,
						// <verse number="2" style="v" />
						// <note caller="-" style="x"><char style="xo" closed="false">1.2: </char><char style="xt" closed="false">Mal 3.1</char></note>
						// kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>
						foreach (XmlNode childNode in usxPara.ChildNodes)
							ProcessParaChildNode(childNode, sb, ref block, blocks, usxPara.StyleTag);

						FlushStringBuilderToBlockElement(sb, block, trim: !(block.BlockElements.LastOrDefault() is QuoteId));
						if (RemoveEmptyTrailingVerse(block))
						{
							var lastVerse = block.LastVerse;
							m_currentStartVerse = lastVerse.StartVerse;
							m_currentEndVerse = lastVerse.EndVerse;
						}
						break;
				}
				if (block != null && block.BlockElements.Count > 0)
					AddBlock(blocks, block);
			}

			var lastBlock = blocks.LastOrDefault();
			while (lastBlock != null && lastBlock.ChapterNumber > 0 && (!lastBlock.IsScripture || lastBlock.IsChapterAnnouncement))
			{
				blocks.RemoveAt(blocks.Count - 1);
				lastBlock = blocks.LastOrDefault();
			}

			return blocks;
		}

		private void ProcessParaChildNode(XmlNode childNode, StringBuilder sb, ref Block block, IList<Block> blocks, string paraStyleTag)
		{
			switch (childNode.Name)
			{
				case UsxNode.kVerseNodeName:
					var verseNumAttr = childNode.Attributes.GetNamedItem("number");
					if (verseNumAttr == null)
						return;
					if (sb.Length > 0)
					{
						sb.TrimStart();
						block.BlockElements.Add(new ScriptText(sb.ToString()));
						sb.Clear();
					}

					var verseNumStr = verseNumAttr.Value;
					if (!HandleVerseBridgeInSeparateVerseFields(block, ref verseNumStr))
					{
						RemoveEmptyTrailingVerse(block);
						m_currentStartVerse = BCVRef.VerseToIntStart(verseNumStr);
						m_currentEndVerse = BCVRef.VerseToIntEnd(verseNumStr);
					}

					if (!block.BlockElements.Any(e => e is ScriptText) ||
					    ((block.BlockElements.OnlyOrDefault() as ScriptText)?.ContainsOnlyWhitespaceAndPunctuation ?? false))
					{
						block.InitialStartVerseNumber = m_currentStartVerse;
						block.InitialEndVerseNumber = m_currentEndVerse;
					}

					block.BlockElements.Add(new Verse(verseNumStr));
					break;
				case UsxNode.kCharNodeName:
					var charTag = (new UsxChar(childNode)).StyleTag;
					IStyle charStyle = m_stylesheet.GetStyle(charTag);
					if (!charStyle.IsInlineQuotationReference && charStyle.IsPublishable)
					{
						// Starting with USFM 3.0, char styles can have attributes separated by |
						var tokens = childNode.InnerText.Split('|');
						if (tokens.Any())
						{
							if (StyleToCharacterMappings.TryGetCharacterForCharStyle(charTag, out var character) && block.StyleTag != charTag)
							{
								// If m_currentExplicitQuote is not null, it will almost always
								// have its StartBlock set, but if this is a character style inside
								// an "interrupting" paragraph (e.g., a section head) or an explicit interruption,
								// then it won't and none of this logic applies.
								if (m_currentExplicitQuote?.StartBlock != null)
								{
									ResolveQuoteCharacter();

									if (m_currentExplicitQuote.StartBlock.CharacterIdInScript != character)
									{
										// Break off any unclosed explicit quote initiated by a milestone quote marker.
										if (block != m_currentExplicitQuote.StartBlock)
											m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
										m_currentExplicitQuote = null;
									}
								}

								if (m_currentExplicitQuote == null)
								{
									FinalizeCharacterStyleBlockWithoutTrailingOpener(sb, ref block, blocks, charTag);
									block.CharacterId = character;
								}
							}

							sb.Append(tokens[0]);
						}
					}

					break;
				case UsxNode.kMilestoneNodeName: // Milestone (PG-1419)
					if (m_currentChapter == 0)
					{
						Logger.WriteEvent($"Ignoring milestone node {childNode} in intro material.");
						break;
					}

					// Note: Technically, the style attribute is required for ms elements,
					// but for greater robustness, if it's missing, we'll just ignore it.
					var styleTag = childNode.GetOptionalStringAttribute("style", default);
					if (Block.TryGetQuoteStartMilestoneLevel(styleTag, out var level))
					{
						var character = childNode.GetOptionalStringAttribute("who", default);
						var id = childNode.GetOptionalStringAttribute("sid", default);
						if (IsQuoteInterruption(character, inExplicitQuote: m_currentExplicitQuote != null))
						{
							ProcessInterruptionStart(sb, id, ref block, blocks, styleTag, level);
						}
						else if (level == 1)
						{
							if (!AddQuoteIdIfNarrator(character, sb, block, id))
							{
								if (m_currentExplicitQuote != null)
								{
									if ((character != null || id != null) &&
									    m_currentExplicitQuote.SpecifiedCharacter == character &&
									    m_currentExplicitQuote.Id == id)
									{
										Logger.WriteEvent($"Duplicate milestone node {childNode}.");
									}
									else
									{
										Logger.WriteEvent("Found level-1 milestone node " +
											$"{childNode} without closing previous milestone " +
											$"{m_currentExplicitQuote.ToString(m_bookId)}.");
									}
									block.CharacterId = CharacterVerseData.kNeedsReview;
									block.CharacterIdInScript = m_currentExplicitQuote.SpecifiedCharacter;
								}

								FinalizeCharacterStyleBlockWithoutTrailingOpener(sb, ref block, blocks, styleTag);
								m_currentExplicitQuote = new ExplicitQuoteInfo(block, id, character, m_currentExplicitQuote);
							}
						}
					}
					else if (Block.TryGetQuoteEndMilestoneLevel(styleTag, out level))
					{
						Debug.Assert(childNode.ParentNode != null);
						var nextNode = childNode.NextSibling;
						var quoteId = childNode.GetOptionalStringAttribute("eid", default);

						if (m_currentExplicitInterruption != null && level <= m_currentExplicitInterruption.Level)
						{
							ProcessInterruptionEnd(sb, quoteId, ref block, blocks, paraStyleTag, level, nextNode);
							break;
						}

						if (level > 1)
							break;

						if (m_currentExplicitQuote == null)
						{
							var character = childNode.GetOptionalStringAttribute("who", default);
							if (!AddQuoteIdIfNarrator(character, sb, block, quoteId, start: false))
								Logger.WriteEvent($"End quote milestone {childNode} does not correspond to a start milestone.");
							break;
						}

						Debug.Assert(m_currentExplicitQuote.Level == 1);

						if (nextNode is XmlWhitespace)
						{
							AppendSpaceIfNeeded(sb);
							childNode.ParentNode.RemoveChild(nextNode);
						}
						else if (TryStripLeadingNonInitialPunctuationWithAdjacentWhitespace(
					         nextNode, out var punctuation))
						{
							sb.Append(punctuation);
						}

						if (block != m_currentExplicitQuote.StartBlock && m_currentExplicitQuote.StartBlock != null)
							m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
						FinalizeCharacterStyleBlock(sb, ref block, blocks,
							m_currentExplicitQuote.PreviouslyOpenQuote?.StartBlock.StyleTag ?? paraStyleTag);
						if (block == m_currentExplicitQuote.StartBlock)
						{
							// The block where we were planning to write the rest of the quote
							// (after an interruption) didn't end up having any contents. So
							// instead of creating a new block, FinalizeCharacterStyleBlock
							// just reused it for the next thing that we're about to parse.
							block.CharacterId = block.CharacterIdOverrideForScript = block.Delivery = null;
						}
						else if (m_currentExplicitQuote.PreviouslyOpenQuote != null)
						{
							for (int i = blocks.Count - 1; i >= 0; i--)
							{
								blocks[i].CharacterId = CharacterVerseData.kNeedsReview;
								blocks[i].CharacterIdInScript = m_currentExplicitQuote.SpecifiedCharacter;
								if (blocks[i] == m_currentExplicitQuote.StartBlock)
									break;
							}

						}

						// Except in the case of bad data, this will always be null.
						m_currentExplicitQuote = m_currentExplicitQuote.PreviouslyOpenQuote;
						if (m_currentExplicitQuote != null)
							m_currentExplicitQuote.StartBlock = null;
						
						blocks.Last().BlockElements.Add(new QuoteId { Id = quoteId, Start = false });
					}

					break;
				case "#text":
					var textToAppend = childNode.InnerText;
					if (m_currentExplicitQuote?.StartBlock != null && !m_currentExplicitQuote.Resolved)
						ResolveQuoteCharacter();
					if (m_currentExplicitInterruption != null)
						InsertQuoteIdAnnotationIfNeeded(m_currentExplicitInterruption);
					if (StyleToCharacterMappings.IncludesCharStyle(block.StyleTag) && textToAppend.Any(IsLetter))
					{
						if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
						{
							if (textToAppend.StartsWith(" "))
							{
								// Not terribly important (in fact, we don't even need the trailing space either), but if blocks have
								// leading spaces, it might look funny in some places in the display.
								sb.Append(" ");
								textToAppend = textToAppend.TrimStart();
							}
							else
							{
								// This logic implements PG-1298
								var match = s_regexLeadingNonInitialPunctuationWithAdjacentWhitespace.Match(textToAppend);
								if (match.Success)
								{
									sb.Append(match.Value);
									textToAppend = textToAppend.Remove(0, match.Length);
								}
							}
						}

						FinalizeCharacterStyleBlock(sb, ref block, blocks, paraStyleTag);
					}

					sb.Append(textToAppend);
					break;
				case "#whitespace":
					AppendSpaceIfNeeded(sb);
					break;
			}
		}

		private bool AddQuoteIdIfNarrator(string character, StringBuilder sb, Block block, string id, bool start = true)
		{
			var lastElement = block.BlockElements.LastOrDefault();
			if ((character == null || !s_regexNarratorCharacter.IsMatch(character)) &&
			    (!(lastElement is QuoteId quid) || !quid.IsNarrator))
				return false;

			FlushStringBuilderToBlockElement(sb, block, trim: !block.BlockElements.Any());
			block.BlockElements.Add(new QuoteId
			{
				Id = id,
				Start = start,
				IsNarrator = true
			});
			return true;
		}

		private class Verses : IReadOnlyCollection<IVerse>
		{
			private int StartVerse { get; }
			private int EndVerse { get; }

			public Verses(int start, int end)
			{
				Debug.Assert(start > 0);
				Debug.Assert(end == 0 || end >= start);
				StartVerse = start;
				EndVerse = end == 0 ? start : end;
			}

			public IEnumerator<IVerse> GetEnumerator()
			{
				for (int i = StartVerse; i <= EndVerse; i++)
					yield return new SingleVerse(i);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int Count => EndVerse - StartVerse + 1;
		}

		private bool IsQuoteInterruption(string character, bool inExplicitQuote)
		{
			if (character == null)
				return false;

			// We distinguish between an "interruption" character (which will always be
			// treated as an interruption) and a "narrator" character (which will only
			// be treated as an interruption if it is inside an explicitly marked quote
			// or it occurs in a verse that is known to have an interruption and
			// does not also have a normal narrator quote).
			var match = s_regexInterruptionCharacter.Match(character);
			bool isExplicitInterruption = match.Success;
			if (!isExplicitInterruption)
				match = s_regexNarratorCharacter.Match(character);
			if (match.Success)
			{
				// Sanity check (but even if they copied an interruption from another book, we
				// will still treat it as an interruption.
				var book = match.Groups["book"].Value;
				if (book != Empty && book != m_bookId)
				{
					Logger.WriteEvent($"Processing explicit interruption in {m_bookId} with " +
						$"book code that does not match current book: {book}");
				}

				if (inExplicitQuote || isExplicitInterruption)
					return true;

				var characters = ControlCharacterVerseData.Singleton.GetCharacters(m_bookNum, m_currentChapter,
					new Verses(m_currentStartVerse, m_currentEndVerse), m_characterUsageStore.Versification);
				return characters.Any(c => c.QuoteType == QuoteType.Interruption) &&
					!characters.Any(c => CharacterVerseData.IsCharacterOfType(c.Character, CharacterVerseData.StandardCharacter.Narrator) &&
						c.QuoteType != QuoteType.Interruption);
			}

			return false;
		}

		private void ProcessInterruptionStart(StringBuilder sb, string sid, ref Block block,
			IList<Block> blocks, string styleTag, uint level)
		{
			// Break off any unclosed explicit quote initiated by a milestone quote marker.
			FinalizeCharacterStyleBlock(sb, ref block, blocks, styleTag);

			if (m_currentExplicitQuote != null)
			{
				Debug.Assert(m_currentExplicitQuote.Resolved);

				if (m_currentExplicitQuote.StartBlock != null &&
				    blocks.Last() != m_currentExplicitQuote.StartBlock)
				{
					m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
				}
			}

			m_currentExplicitInterruption = new ExplicitInterruptionInfo(block, sid, m_bookId, m_currentExplicitQuote, level);
			m_currentExplicitQuote = null;
			block.SetNonDramaticCharacterId(m_currentExplicitInterruption.SpecifiedCharacter);
			m_currentExplicitInterruption.Resolved = true;
		}

		private void ProcessInterruptionEnd(StringBuilder sb, string eid, ref Block block,
			IList<Block> blocks, string paraStyleTag, uint level, XmlNode nextSibling)
		{
			FinalizeCharacterStyleBlock(sb, ref block, blocks, level > 1 ?
				(m_currentExplicitInterruption.OriginalStyleTag ?? paraStyleTag) : paraStyleTag);

			m_currentExplicitQuote = level > 1 ?
				m_currentExplicitInterruption.PreviouslyOpenQuote : null;
			if (m_currentExplicitQuote != null)
			{
				block.CharacterId = m_currentExplicitQuote.StartBlock.CharacterId;
				block.Delivery = m_currentExplicitQuote.StartBlock.Delivery;
				block.CharacterIdInScript = m_currentExplicitQuote.StartBlock.CharacterIdInScript;
				m_currentExplicitQuote.StartBlock = block;
			}
			m_currentExplicitInterruption = null;

			if (eid != null)
				blocks.Last().BlockElements.Add(new QuoteId { Id = eid, Start = false, IsNarrator = level > 1});

			if (nextSibling is XmlWhitespace)
			{
				var lastText = blocks.Last().BlockElements.OfType<ScriptText>().Last();
				if (!IsWhiteSpace(lastText.Content.Last()))
					lastText.Content += " ";
				nextSibling.ParentNode.RemoveChild(nextSibling);
			}
			else
			{
				string stripped;
				if (nextSibling is XmlText text && IsWhiteSpace(text.InnerText[0]))
				{
					// Not terribly important (in fact, we don't even need the trailing space either), but if blocks have
					// leading spaces, it might look funny in some places in the display.
					stripped = " ";
					var remainingText = text.InnerText.TrimStart();
					if (remainingText.Any())
						text.InnerText = remainingText;
					else
						nextSibling.ParentNode.RemoveChild(nextSibling);
				}
				else
					TryStripLeadingNonInitialPunctuationWithAdjacentWhitespace(nextSibling, out stripped);

				if (stripped != null)
				{
					if (blocks.Last().BlockElements.Last() is ScriptText last)
						last.Content += stripped;
					else
						blocks.Last().BlockElements.Add(new ScriptText(stripped));
				}
			}
		}

		private bool TryStripLeadingNonInitialPunctuationWithAdjacentWhitespace(XmlNode node, out string punctuation)
		{
			if (node is XmlText text)
			{
				var match = s_regexLeadingNonInitialPunctuationWithAdjacentWhitespace.Match(text.InnerText);
				if (match.Success)
				{
					punctuation = match.Value;

					var toRemove = match.Length;
					var remainingText = text.InnerText.Remove(0, toRemove).TrimStart();
					if (remainingText.Any())
						text.InnerText = remainingText;
					else
						node.ParentNode.RemoveChild(node);

					return true;
				}
			}

			punctuation = null;
			return false;
		}

		private void AddBlock(IList<Block> blocks, Block block)
		{
			if (m_currentExplicitQuote?.StartBlock != null && m_currentExplicitQuote.StartBlock != block)
			{
				if (block.CharacterIsStandard)
				{
					if (m_currentExplicitQuote.StartBlock != null)
					{
						if (blocks.Last() != m_currentExplicitQuote.StartBlock)
							m_currentExplicitQuote.StartBlock.MultiBlockQuote = MultiBlockQuote.Start;
						m_currentExplicitQuote.StartBlock = null;
					}
				}
				else
				{
					block.MultiBlockQuote = MultiBlockQuote.Continuation;
					block.CharacterId = m_currentExplicitQuote.StartBlock.CharacterId;
					block.CharacterIdInScript = m_currentExplicitQuote.StartBlock.CharacterIdInScript;
					// REVIEW: What about the delivery? Currently there is no standard field to set this
					// defined in USFM. Should there be?
				}
			}

			blocks.Add(block);
		}

		private static void AppendSpaceIfNeeded(StringBuilder sb)
		{
			if (sb.Length > 0 && !IsWhiteSpace(sb[sb.Length - 1]))
				sb.Append(" ");
		}

		private void ResolveQuoteCharacter()
		{
			InsertQuoteIdAnnotationIfNeeded(m_currentExplicitQuote);
			var block = m_currentExplicitQuote.StartBlock;

			if (m_currentExplicitQuote.SpecifiedCharacter != null)
			{
				var standardCharacter = m_characterUsageStore.GetKnownCharacterName(
					m_currentExplicitQuote.SpecifiedCharacter, m_bookNum, m_currentChapter, block.AllVerses,
					out var delivery, out var defaultCharacter);
				if (standardCharacter != null)
				{
					block.CharacterId = standardCharacter;
					block.Delivery = delivery;
					block.CharacterIdInScript = defaultCharacter;
				}
				else
				{
					block.CharacterId = CharacterVerseData.kNeedsReview;
					block.CharacterIdInScript = m_currentExplicitQuote.SpecifiedCharacter;
				}

				m_currentExplicitQuote.Resolved = true;
			}
		}

		private static void InsertQuoteIdAnnotationIfNeeded(ExplicitQuoteInfo explicitQuoteInfo)
		{
			var block = explicitQuoteInfo.StartBlock;
			if (explicitQuoteInfo.Id != null)
			{
				var quoteIdAnnotation = new QuoteId
				{
					Id = explicitQuoteInfo.Id,
					Start = true,
					IsNarrator = explicitQuoteInfo.SpecifiedCharacter != null &&
						CharacterVerseData.IsCharacterOfType(explicitQuoteInfo.SpecifiedCharacter,
							CharacterVerseData.StandardCharacter.Narrator)
				};
				if (block.BlockElements.LastOrDefault() is Verse)
					block.BlockElements.Insert(block.BlockElements.Count - 1, quoteIdAnnotation);
				else
					block.BlockElements.Add(quoteIdAnnotation);
			}
		}

		private void FinalizeCharacterStyleBlockWithoutTrailingOpener(StringBuilder sb, ref Block block, IList<Block> blocks, string newBlockTag)
		{
			string trailingOpener = null;
			var matchOpeningQuoteAtEnd = s_regexEndsWithOpeningQuote.Match(sb.ToString());
			if (matchOpeningQuoteAtEnd.Success)
			{
				sb.Remove(matchOpeningQuoteAtEnd.Index, matchOpeningQuoteAtEnd.Length);
				trailingOpener = matchOpeningQuoteAtEnd.Value;
			}
			FinalizeCharacterStyleBlock(sb, ref block, blocks, newBlockTag);
			if (trailingOpener != null)
				sb.Append(trailingOpener);
		}

		private void FinalizeCharacterStyleBlock(StringBuilder sb, ref Block block, IList<Block> blocks, string newBlockTag)
		{
			FlushStringBuilderToBlockElement(sb, block);
			if (block.BlockElements.OfType<ScriptText>().Any())
			{
				Verse finalVerse = block.BlockElements.Last() as Verse;
				if (finalVerse != null)
					block.BlockElements.RemoveAt(block.BlockElements.Count - 1);
				AddBlock(blocks, block);
				block = new Block(newBlockTag, m_currentChapter, m_currentStartVerse, m_currentEndVerse);
				if (finalVerse != null)
					block.BlockElements.Add(finalVerse);
			}
			else
			{
				block.StyleTag = newBlockTag;
			}
		}

		private void FlushStringBuilderToBlockElement(StringBuilder sb, Block block, bool trim = true)
		{
			if (trim)
				sb.TrimStart();
			if (sb.Length > 0)
			{
				block.BlockElements.Add(new ScriptText(sb.ToString()));
				sb.Clear();
			}
		}

		/// <summary>
		/// If the existing block begins with a verse number, followed by "verse text" that consists
		/// merely of a single dash, and we are now processing another (higher) verse number, the
		/// block is doctored up to interpret this as verse bridge. Although it is not clear whether
		/// this has ever been considered valid USFM, it has been done in real projects. Since Paratext
		/// does not currently (as of 9.0 beta) flag this as a missing verse, handling the data this
		/// way allows Glyssen to interpret it as it was probably intended and not treat it as a missing
		/// verse.
		/// </summary>
		/// <example>For example, if USFM looks like this: \v 1 - \v 2 Text of the two verses.
		/// Then in the USXParser, we will have a block with {1} - and a subsequent verse number 2.
		/// This method will produce a block with the verse bridge {1-2} (and, as yet, no verse text
		/// element).</example>
		/// <returns><c>true</c> if a mal-formed verse range is turned into a valid verse bridge,
		/// </returns>
		private bool HandleVerseBridgeInSeparateVerseFields(Block block, ref string verseNumStr)
		{
			var iLastElem = block.BlockElements.Count - 1;
			if (iLastElem >= 1 && m_currentStartVerse == m_currentEndVerse)
			{
				if ((block.BlockElements[iLastElem] as ScriptText)?.Content?.Trim() == "-")
				{
					var existingVerseNum = (block.BlockElements[iLastElem - 1] as Verse)?.Number;
					if (existingVerseNum == m_currentStartVerse.ToString() &&
						Compare(verseNumStr, existingVerseNum, StringComparison.Ordinal) > 0)
					{
						block.BlockElements.RemoveRange(iLastElem - 1, 2);
						m_currentEndVerse = BCVRef.VerseToIntEnd(verseNumStr);
						verseNumStr = existingVerseNum + "-" + verseNumStr;
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// In addition to cleaning up any completely *empty* Verse (i.e., a verse number not followed by
		/// a ScriptText), this method also checks for a Verse followed by a ScriptText that doesn't have any
		/// useful text (i.e., at least one word-forming character). In that case, the bogus ScriptText and
		/// the preceding Verse element are both removed, and as a special case we also check for and remove
		/// any matching opening bracket/brace from the preceding ScriptText, since some translations will
		/// just put a verse number in brackets along with a footnote (which Glyssen discards already) to
		/// indicate that the verse was omitted because it is not contained in the most reliable manuscripts.
		/// </summary>
		/// <returns><c>true</c> if an empty verse is removed; <c>false</c> otherwise.</returns>
		private static bool RemoveEmptyTrailingVerse(Block block)
		{
			if (block.BlockElements.Count > 0)
			{
				var lastBlockElement = block.BlockElements.Last();
				if (lastBlockElement is Verse)
				{
					block.BlockElements.Remove(lastBlockElement);
					return true;
				}
				if (lastBlockElement is ScriptText text && block.BlockElements.Count > 1 && text.ContainsNoWords)
				{
					if (!(block.BlockElements[block.BlockElements.Count - 2] is Verse))
					{
						Debug.Assert(!(block.BlockElements[block.BlockElements.Count - 2] is ScriptText),
							"This should be impossible. The USX parser should never add two adjacent ScriptText elements!");
						return false;
					}
					var potentialClosingBracketPunct = text.Content.Trim();
					// Remove both the bogus ScriptText and the preceding Verse.
					block.BlockElements.RemoveRange(block.BlockElements.Count - 2, 2);
					RemoveMatchingOpeningPunctuation(block, potentialClosingBracketPunct);
					return true;
				}
			}
			return false;
		}

		private static void RemoveMatchingOpeningPunctuation(Block block, string punct)
		{
			char punctToTrim;
			switch (punct)
			{
				case "]": punctToTrim = '['; break;
				case "}": punctToTrim = '{'; break;
				case ")": punctToTrim = '('; break;
				case "\u300d": punctToTrim = '\u300c'; break;
				case "\uff63": punctToTrim = '\uff62'; break;
				case "\u3011": punctToTrim = '\u3010'; break;
				case "\u3015": punctToTrim = '\u3014'; break;
				default: return;
			}

			if (block.BlockElements.LastOrDefault() is ScriptText finalScriptText &&
				finalScriptText.Content.TrimEnd().Last() == punctToTrim)
			{
				finalScriptText.Content = finalScriptText.Content.TrimEnd().TrimEnd(punctToTrim);
				if (finalScriptText.Content.All(IsWhiteSpace))
				{
					block.BlockElements.Remove(finalScriptText);
				}
			}
		}

		private void AddMainTitleIfApplicable(ICollection<Block> blocks, StringBuilder titleBuilder)
		{
			if (titleBuilder.Length < 1)
				return;
			var titleBlock = new Block("mt") { IsParagraphStart = true };
			titleBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.BookOrChapter);
			titleBlock.BlockElements.Add(new ScriptText(titleBuilder.ToString().Trim()));
			blocks.Add(titleBlock);
			titleBuilder.Clear();
		}

		private Block ProcessChapterNode(XmlNode node)
		{
			var usxChapter = new UsxChapter(node);
			if (!usxChapter.IsChapterStart)
				return null; // We can ignore chapter end milestones
			string chapterText;
			if (m_bookLevelChapterLabel != null)
			{
				// If this isn't the right order, the user would have had to enter a specific chapter label
				// for each chapter to format it correctly.
				chapterText = m_bookLevelChapterLabel + " " + usxChapter.ChapterNumber;
			}
			else
				chapterText = usxChapter.ChapterNumber;

			if (int.TryParse(usxChapter.ChapterNumber, out var chapterNum))
				m_currentChapter = chapterNum;
			else
				Debug.Fail("TODO: Deal with bogus chapter number in USX data!");
			m_currentStartVerse = 0;
			m_currentEndVerse = 0;
			var block = new Block(usxChapter.StyleTag, m_currentChapter) { IsParagraphStart = true, BookCode = m_bookId};
			block.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText(chapterText));
			return block;
		}

		private Block ProcessChapterLabelNode(string nodeText, UsxNode usxNode, IList<Block> blocks)
		{
			var lastChapterAnnouncementBlock = blocks.LastOrDefault(b => b.IsChapterAnnouncement);

			// Chapter label before the first chapter means we have a chapter label which applies to all chapters
			if (lastChapterAnnouncementBlock == null)
				m_bookLevelChapterLabel = nodeText;
			else if (m_bookLevelChapterLabel == null && blocks.Last() == lastChapterAnnouncementBlock)
			{
				// The node before this was the chapter. We already added it, then found this label.
				// Remove that block so it will be replaced with this one.
				blocks.RemoveAt(blocks.Count - 1);
				var block = new Block(usxNode.StyleTag, m_currentChapter) { IsParagraphStart = true, BookCode = m_bookId };
				block.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.BookOrChapter);
				block.BlockElements.Add(new ScriptText(nodeText));
				m_currentStartVerse = 0;
				m_currentEndVerse = 0;
				return block;
			}
			// Note: In PG-1140, it was reported that errant/misplaced \cl markers (at the end of the Pauline epistles) were causing
			// the preceding block to go AWOL. We aren't sure what the text in the \cl field was supposed to be (it was the same in
			// all the books and ended with a comma, which seemed pretty weird), but we decided to just ignore any \cl marker coming
			// in an unexpected place. Ideally, the Markers check in Paratext should catch this as an error (and I have requested this),
			// but at least as of now, it does not.
			return null;
		}
	}

	static class StringBuilderExtensions
	{
		public static void TrimStart(this StringBuilder sb)
		{
			while (sb.Length > 0 && sb[0] == ' ')
				sb.Remove(0, 1);
		}
	}
}
