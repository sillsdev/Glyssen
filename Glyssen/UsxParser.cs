using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Glyssen.Character;
using Glyssen.Shared;
using Glyssen.Utilities;
using SIL.DblBundle;
using SIL.DblBundle.Usx;
using SIL.Reporting;
using SIL.Scripture;
using static System.String;

namespace Glyssen
{
	public class UsxParser
	{
		public static List<BookScript> ParseBooks(IEnumerable<UsxDocument> books, IStylesheet stylesheet, Action<int> reportProgressAsPercent)
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
				var bookScript = new UsxParser(bookId, stylesheet, book.Value).CreateBookScript();
				lock(bookScripts)
					bookScripts.Add(bookScript);
				Logger.WriteEvent("Added bookScript ({0}, {1})", bookId, bookScript.BookId);
				completedBlocks += numBlocksPerBook[bookId];
				reportProgressAsPercent?.Invoke(MathUtilities.Percent(completedBlocks, allBlocks, 99));
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
			var bookScript = new BookScript(m_bookId, Parse(), null)
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
		private readonly XmlNodeList m_nodeList;

		private string m_bookLevelChapterLabel;
		private int m_currentChapter;
		private int m_currentStartVerse;
		private int m_currentEndVerse;

		public UsxParser(string bookId, IStylesheet stylesheet, XmlNodeList nodeList)
		{
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(m_bookId);
			m_stylesheet = stylesheet;
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
					case "chapter":
						AddMainTitleIfApplicable(blocks, titleBuilder);
						block = ProcessChapterNode(node);
						if (block == null)
							continue;
						break;
					case "para":
						if (!node.HasChildNodes)
							continue;

						var usxPara = new UsxNode(node);
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
							block.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.ExtraBiblical);

						var sb = new StringBuilder();
						// <verse number="1" style="v" />
						// Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,
						// <verse number="2" style="v" />
						// <note caller="-" style="x"><char style="xo" closed="false">1.2: </char><char style="xt" closed="false">Mal 3.1</char></note>
						// kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>
						foreach (XmlNode childNode in usxPara.ChildNodes)
						{
							switch (childNode.Name)
							{
								case "verse":
									if (sb.Length > 0)
									{
										sb.TrimStart();
										block.BlockElements.Add(new ScriptText(sb.ToString()));
										sb.Clear();
									}
									var verseNumStr = childNode.Attributes.GetNamedItem("number").Value;
									if (!HandleVerseBridgeInSeparateVerseFields(block, ref verseNumStr))
									{
										RemoveEmptyTrailingVerse(block);
										m_currentStartVerse = BCVRef.VerseToIntStart(verseNumStr);
										m_currentEndVerse = BCVRef.VerseToIntEnd(verseNumStr);
									}
									if (!block.BlockElements.Any() ||
										(block.BlockElements.Count == 1 && block.StartsWithScriptTextElementContainingOnlyPunctuation))
									{
										block.InitialStartVerseNumber = m_currentStartVerse;
										block.InitialEndVerseNumber = m_currentEndVerse;
									}

									block.BlockElements.Add(new Verse(verseNumStr));
									break;
								case "char":
									var charTag = (new UsxNode(childNode)).StyleTag;
									IStyle charStyle = m_stylesheet.GetStyle(charTag);
									if (!charStyle.IsInlineQuotationReference && charStyle.IsPublishable)
									{
										// Starting with USFM 3.0, char styles can have attributes separated by |
										var tokens = childNode.InnerText.Split('|');
										if (tokens.Any())
										{
											if (ControlCharacterVerseData.TryGetCharacterForCharStyle(charTag, out var character) && block.StyleTag != charTag)
											{
												FinalizeCharacterStyleBlock(sb, ref block, blocks, charTag);
												block.CharacterId = character;
												//ControlCharacterVerseData.Singleton.GetCharacters(m_bookNum, block.ChapterNumber,
												//block.InitialStartVerseNumber, block.InitialEndVerseNumber, block.LastVerseNum, m_versification, true)
												//.FirstOrDefault(cv => cv.Character == character)?.Character ?? CharacterVerseData.kNeedsReview;
											}
											sb.Append(tokens[0]);
										}
									}

									break;
								case "#text":
									if (ControlCharacterVerseData.IsCharStyleThatMapsToSpecificCharacter(block.StyleTag))
										FinalizeCharacterStyleBlock(sb, ref block, blocks, usxPara.StyleTag);
									sb.Append(childNode.InnerText);
									break;
								case "#whitespace":
									if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
										sb.Append(" ");
									break;
							}
						}
						FlushStringBuilderToBlockElement(sb, block);
						if (RemoveEmptyTrailingVerse(block))
						{
							var lastVerse = block.LastVerse;
							m_currentStartVerse = lastVerse.StartVerse;
							m_currentEndVerse = lastVerse.EndVerse;
						}
						break;
				}
				if (block != null && block.BlockElements.Count > 0)
					blocks.Add(block);
			}
			return blocks;
		}

		private void FinalizeCharacterStyleBlock(StringBuilder sb, ref Block block, IList<Block> blocks, string newBlockTag)
		{
			FlushStringBuilderToBlockElement(sb, block);
			if (block.BlockElements.OfType<ScriptText>().Any())
			{
				Verse finalVerse = block.BlockElements.Last() as Verse;
				if (finalVerse != null)
					block.BlockElements.RemoveAt(block.BlockElements.Count - 1);
				blocks.Add(block);
				block = new Block(newBlockTag, m_currentChapter, m_currentStartVerse, m_currentEndVerse);
				if (finalVerse != null)
					block.BlockElements.Add(finalVerse);
			}
			else
			{
				block.StyleTag = newBlockTag;
			}
		}

		private void FlushStringBuilderToBlockElement(StringBuilder sb, Block block)
		{
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
				// Originally, the last part of this condition was:
				// text.Content.All(c => char.IsPunctuation(c) || char.IsWhiteSpace(c))
				// because char.IsLetter doesn't know what to do with PUA characters and
				// I didn't want to run the risk of accidentally deleting a verse that
				// might have all PUA characters, but upon further consideration, I decided
				// that was extremely unlikely, and there was probably a greater risk of
				// some other symbol, number, separator, etc. being the only thing in the
				// text. And it would be slow and unwieldy to check all the other possibilities
				// and something might still fall through the cracks.
				if (lastBlockElement is ScriptText text && block.BlockElements.Count > 1 &&
					!text.Content.Any(char.IsLetter))
				{
					if (!(block.BlockElements[block.BlockElements.Count - 2] is Verse))
					{
						Debug.Fail("This should be impossible. The only block elements the UsxParser can add are Verse and ScriptText, and they must alternate.");
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
				if (finalScriptText.Content.All(char.IsWhiteSpace))
				{
					block.BlockElements.Remove(finalScriptText);
				}
			}
		}

		private void AddMainTitleIfApplicable(ICollection<Block> blocks, StringBuilder titleBuilder)
		{
			if (titleBuilder.Length < 1)
				return;
			var titleBlock = new Block("mt");
			titleBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.BookOrChapter);
			titleBlock.BlockElements.Add(new ScriptText(titleBuilder.ToString().Trim()));
			blocks.Add(titleBlock);
			titleBuilder.Clear();
		}

		private Block ProcessChapterNode(XmlNode node)
		{
			var usxChapter = new UsxChapter(node);
			string chapterText;
			if (m_bookLevelChapterLabel != null)
			{
				// If this isn't the right order, the user would have had to enter a specific chapter label
				// for each chapter to format it correctly.
				chapterText = m_bookLevelChapterLabel + " " + usxChapter.ChapterNumber;
			}
			else
				chapterText = usxChapter.ChapterNumber;

			if (Int32.TryParse(usxChapter.ChapterNumber, out var chapterNum))
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
