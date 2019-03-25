using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
				if (bookScript == null || bookScript.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = string.Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException(string.Format("{0} Number of BookScripts: {1}. BookScripts which are NOT null: {2}", initialMessage, bookScripts.Count, nonNullBookScriptsStr));
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
			var bookScript = new BookScript(m_bookId, Parse())
			{
				SingleVoice = BookMetadata.DefaultToSingleVoice(m_bookId, out SingleVoiceReason reason),
				PageHeader = PageHeader,
				MainTitle = MainTitle
			};
			Logger.WriteEvent("Created bookScript ({0}, {1})", m_bookId, bookScript.BookId);
			return bookScript;
		}

		private readonly string m_bookId;
		private readonly IStylesheet m_stylesheet;
		private readonly XmlNodeList m_nodeList;

		private string m_bookLevelChapterLabel;
		private int m_currentChapter;
		private int m_currentStartVerse;
		private int m_currentEndVerse;

		public UsxParser(string bookId, IStylesheet stylesheet, XmlNodeList nodeList)
		{
			m_bookId = bookId;
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
									RemoveLastElementIfVerse(block);
									var verseNumStr = childNode.Attributes.GetNamedItem("number").Value;
									m_currentStartVerse = BCVRef.VerseToIntStart(verseNumStr);
									m_currentEndVerse = BCVRef.VerseToIntEnd(verseNumStr);
									if (!block.BlockElements.Any() ||
										(block.BlockElements.Count == 1 && block.StartsWithScriptTextElementContainingOnlyPunctuation))
									{
										block.InitialStartVerseNumber = m_currentStartVerse;
										block.InitialEndVerseNumber = m_currentEndVerse;
									}

									block.BlockElements.Add(new Verse(verseNumStr));
									break;
								case "char":
									IStyle charStyle = m_stylesheet.GetStyle((new UsxNode(childNode)).StyleTag);
									if (!charStyle.IsInlineQuotationReference && charStyle.IsPublishable)
									{
										// Starting with USFM 3.0, char styles can have attributes separated by |
										var tokens = childNode.InnerText.Split('|');
										if (tokens.Any())
											sb.Append(tokens[0]);
									}

									break;
								case "#text":
									sb.Append(childNode.InnerText);
									break;
								case "#whitespace":
									if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
										sb.Append(" ");
									break;
							}
						}
						sb.TrimStart();
						if (sb.Length > 0)
						{
							block.BlockElements.Add(new ScriptText(sb.ToString()));
							sb.Clear();
						}
						RemoveLastElementIfVerse(block);
						break;
				}
				if (block != null && block.BlockElements.Count > 0)
					blocks.Add(block);
			}
			return blocks;
		}

		private static void RemoveLastElementIfVerse(Block block)
		{
			if (block.BlockElements.Count > 0)
			{
				var lastBlockElement = block.BlockElements.Last();
				if (lastBlockElement is Verse)
					block.BlockElements.Remove(block.BlockElements.Last());
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

			int chapterNum;
			if (Int32.TryParse(usxChapter.ChapterNumber, out chapterNum))
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
			var lastChapter = blocks.LastOrDefault(b => b.IsChapterAnnouncement);

			// Chapter label before the first chapter means we have a chapter label which applies to all chapters
			if (lastChapter == null)
				m_bookLevelChapterLabel = nodeText;
			else if (m_bookLevelChapterLabel == null && blocks.Last() == lastChapter)
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
