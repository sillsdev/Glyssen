using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Glyssen.Character;
using Glyssen.Utilities;
using SIL.Scripture;
using ScrVers = Paratext.ScrVers;

namespace Glyssen.Quote
{
	public class QuoteParser
	{
		public static void ParseProject(Project project, BackgroundWorker projectWorker)
		{
			var cvInfo = new CombinedCharacterVerseData(project);

			var numBlocksPerBook = new ConcurrentDictionary<string, int>();
			var blocksInBook = new ConcurrentDictionary<BookScript, IReadOnlyList<Block>>();
			Parallel.ForEach(project.Books, book =>
			{
				var nodeList = book.GetScriptBlocks();
				blocksInBook.AddOrUpdate(book, nodeList, (script, list) => nodeList);
				numBlocksPerBook.AddOrUpdate(book.BookId, nodeList.Count, (s, i) => nodeList.Count);
			});
			int allProjectBlocks = numBlocksPerBook.Values.Sum();

			int completedProjectBlocks = 0;
			Parallel.ForEach(blocksInBook.Keys, book =>
			{
				book.Blocks = new QuoteParser(cvInfo, book.BookId, blocksInBook[book], project.QuoteSystem, project.Versification).Parse().ToList();
				completedProjectBlocks += numBlocksPerBook[book.BookId];
				projectWorker.ReportProgress(MathUtilities.Percent(completedProjectBlocks, allProjectBlocks, 99));
			});

			projectWorker.ReportProgress(100);
		}

		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteSystem m_quoteSystem;
		private readonly ScrVers m_versification;
		private readonly List<Regex> m_regexes = new List<Regex>();
		private readonly Regex m_regexStartsWithSpecialOpeningPunctuation = new Regex(@"^(\(|\\\[|\\\{)", RegexOptions.Compiled);

		#region working members
		// These members are used by several methods. Making them class-level prevents passing them repeatedly
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		private int m_quoteLevel;
		private bool m_nextBlockContinuesQuote;
		private readonly List<Block> m_currentMultiBlockQuote = new List<Block>();
		#endregion
		
		public QuoteParser(ICharacterVerseInfo cvInfo, string bookId, IEnumerable<Block> blocks, QuoteSystem quoteSystem = null, ScrVers versification = null)
		{
			m_cvInfo = cvInfo;
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(bookId);
			m_inputBlocks = blocks;
			m_quoteSystem = quoteSystem ?? QuoteSystem.Default;
			m_versification = versification ?? ScrVers.English;
			GetRegExesForSplittingQuotes();
		}

		private void GetRegExesForSplittingQuotes()
		{
			var splitters = new SortedSet<string>(new SplitterComparer());
			var quoteChars = new HashSet<char>();
			var regexExpressions = new List<string>(m_quoteSystem.NormalLevels.Count);

			for (int level = 0; level < m_quoteSystem.NormalLevels.Count; level++)
			{
				var quoteSystemLevel = m_quoteSystem.NormalLevels[level];
				splitters.Add(quoteSystemLevel.Open);
				splitters.Add(quoteSystemLevel.Close);
				if (!string.IsNullOrWhiteSpace(quoteSystemLevel.Continue))
					splitters.Add(quoteSystemLevel.Continue);


				if (level == 0 && !string.IsNullOrEmpty(m_quoteSystem.QuotationDashMarker))
				{
					splitters.Add(m_quoteSystem.QuotationDashMarker);
					if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker))
						splitters.Add(m_quoteSystem.QuotationDashEndMarker);
				}
				
				var sbQuoteMatcher = new StringBuilder();

				foreach (var qm in splitters)
				{
					sbQuoteMatcher.Append("(?:");
					sbQuoteMatcher.Append(Regex.Escape(qm));
					sbQuoteMatcher.Append(")|");
				}
				sbQuoteMatcher.Length--;

				var quoteMatcher = sbQuoteMatcher.ToString();
				// quoteMatcher includes all the possible markers; e.g. (?:«)|(?:‹)|(?:›)|(?:»).
				// Need to group because they could be more than one character each.
				// ?: => non-matching group
				// \w => word-forming character
				regexExpressions.Add(String.Format(@"((?:(?:{0})(?:[^\w{1}])*))", quoteMatcher, "{0}{1}"));
			}

			foreach (var ch in splitters.SelectMany(qm => qm.Where(c => !Char.IsWhiteSpace(c))))
				quoteChars.Add(ch);

			foreach (var expr in regexExpressions)
			{
				m_regexes.Add(new Regex(String.Format(expr,
					Regex.Escape(string.Join(string.Empty, quoteChars)),
					Regex.Escape(@"(\[\{")), RegexOptions.Compiled));
			}
		}

		/// <summary>
		/// Parse through the given blocks character by character to determine where we need to break based on quotes 
		/// </summary>
		/// <returns>A new enumerable of blocks broken up for quotes</returns>
		public IEnumerable<Block> Parse()
		{
			if (m_quoteSystem == null)
				return m_inputBlocks;
			m_outputBlocks = new List<Block>();
			var sb = new StringBuilder();
			m_quoteLevel = 0;
			bool blockEndedWithSentenceEndingPunctuation = false;
			Block blockInWhichDialogueQuoteStarted = null;
			bool potentialDialogueContinuer = false;
			foreach (Block block in m_inputBlocks)
			{
				if (block.UserConfirmed)
					throw new InvalidOperationException("Should not be parsing blocks that already have user-decisions applied.");

				if (block.CharacterIsStandard && !block.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
				{
					// The following handles the case where an open quote is interrupted by a section head or chapter break
					var lastBlockAdded = m_outputBlocks.LastOrDefault();
					if (lastBlockAdded != null && lastBlockAdded.MultiBlockQuote == MultiBlockQuote.Start)
					{
						lastBlockAdded.MultiBlockQuote = MultiBlockQuote.None;
						ProcessMultiBlock();
					}
					m_nextBlockContinuesQuote = false;

					m_outputBlocks.Add(block);
					continue;
				}

				if (m_quoteLevel == 1 && 
					blockInWhichDialogueQuoteStarted != null && 
					(!IsNormalParagraphStyle(blockInWhichDialogueQuoteStarted.StyleTag) || blockEndedWithSentenceEndingPunctuation || !IsFollowOnParagraphStyle(block.StyleTag)))
				{
					m_quoteLevel--;
					blockInWhichDialogueQuoteStarted = null;
					potentialDialogueContinuer = m_quoteSystem.QuotationDashEndMarker != null;
				}

				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber) { IsParagraphStart = block.IsParagraphStart };

				bool atBeginningOfBlock = true;
				bool specialOpeningPunctuation = false;
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						// Add the element to our working list in case we need to move it to the next block (see MoveTrailingElementsIfNecessary)
						m_nonScriptTextBlockElements.Add(element);

						if (!m_workingBlock.BlockElements.Any() && element is Verse)
							m_workingBlock.InitialStartVerseNumber = (element as Verse).StartVerse;

						m_workingBlock.BlockElements.Add(element);
						continue;
					}
					sb.Clear();

					var content = scriptText.Content;
					int pos = 0;
					while (pos < content.Length)
					{
						var regex = m_regexes[m_quoteLevel >= m_regexes.Count ? m_regexes.Count - 1 : m_quoteLevel];
						var match = regex.Match(content, pos);
						if (match.Success)
						{
							if (match.Index > pos)
							{
								specialOpeningPunctuation = m_regexStartsWithSpecialOpeningPunctuation.Match(content).Success;
								sb.Append(content.Substring(pos, match.Index - pos));
							}

							pos = match.Index + match.Length;

							var token = match.Value;

							if (!specialOpeningPunctuation)
								atBeginningOfBlock &= match.Index == 0;

							if (atBeginningOfBlock)
							{
								if (!specialOpeningPunctuation)
									atBeginningOfBlock = false;

								if (m_quoteLevel > 0 && token.StartsWith(ContinuerForCurrentLevel))
								{
									int i = ContinuerForCurrentLevel.Length;
									while (i < token.Length && Char.IsWhiteSpace(token[i]))
										i++;
									sb.Append(token.Substring(0, i));
									if (token.Length == i)
										continue;
									token = token.Substring(i);
								}
								if (m_quoteLevel == 0)
								{
									string continuerForNextLevel = ContinuerForNextLevel;
									if (string.IsNullOrEmpty(continuerForNextLevel) || !token.StartsWith(continuerForNextLevel))
										potentialDialogueContinuer = false;
								}
								else
								{
									potentialDialogueContinuer = false;
								}
							}

							if (m_quoteLevel > 0 && token.StartsWith(CloserForCurrentLevel) && blockInWhichDialogueQuoteStarted == null)
							{
								sb.Append(token);
								if (--m_quoteLevel == 0)
									FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							}
							else if (m_quoteSystem.NormalLevels.Count > m_quoteLevel && token.StartsWith(OpenerForNextLevel) && blockInWhichDialogueQuoteStarted == null)
							{
								if (m_quoteLevel == 0)
									FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								sb.Append(token);
								m_quoteLevel++;
							}
							else if (m_quoteLevel == 0 && m_quoteSystem.QuotationDashMarker != null && token.StartsWith(m_quoteSystem.QuotationDashMarker))
							{
								blockInWhichDialogueQuoteStarted = block;
								blockEndedWithSentenceEndingPunctuation = false;
								bool specialCaseWithColon = token.StartsWith(":");
								if (specialCaseWithColon)
									sb.Append(token);
								FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								if (!specialCaseWithColon)
									sb.Append(token);
								m_quoteLevel++;
							}
							else if (potentialDialogueContinuer || (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null))
							{
								if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker) && token.StartsWith(m_quoteSystem.QuotationDashEndMarker, StringComparison.Ordinal))
								{
									m_quoteLevel--;
									blockInWhichDialogueQuoteStarted = null;
									FlushStringBuilderAndBlock(sb, block.StyleTag, true);
								}
								else
								{
									blockEndedWithSentenceEndingPunctuation = !IsFollowOnParagraphStyle(m_workingBlock.StyleTag) && EndsWithSentenceEndingPunctuation(token);
								}
								sb.Append(token);
							}
							else
							{
								sb.Append(token);
							}
						}
						else
						{
							var remainingText = content.Substring(pos);
							if (m_quoteLevel == 1 && block == blockInWhichDialogueQuoteStarted && EndsWithSentenceEndingPunctuation(remainingText))
								blockEndedWithSentenceEndingPunctuation = true;
							sb.Append(remainingText);
							break;
						}
					}
					FlushStringBuilderToBlockElement(sb);
				}
				FlushBlock(block.StyleTag, m_quoteLevel > 0);
			}
			// In case the last set of blocks were a multi-block quote
			ProcessMultiBlock();
			return m_outputBlocks;
		}

		public string ContinuerForCurrentLevel { get { return m_quoteSystem.NormalLevels[m_quoteLevel - 1].Continue; } }
		public string ContinuerForNextLevel { get { return m_quoteSystem.NormalLevels[m_quoteLevel].Continue; } }
		public string CloserForCurrentLevel { get { return m_quoteSystem.NormalLevels[m_quoteLevel - 1].Close; } }
		public string OpenerForNextLevel { get { return m_quoteSystem.NormalLevels[m_quoteLevel].Open; } }

		private bool EndsWithSentenceEndingPunctuation(string text)
		{
			int i = text.Length - 1;
			while (i >= 0)
			{
				char c = text[i];
				if (char.IsPunctuation(c))
				{
					if (IsSentenceEnding(c))
					{
						return true;
					}
				}
				else if (!char.IsWhiteSpace(c))
				{
					return false;
				}
				i--;
			}
			return false;
		}

		/// <summary>
		/// Flush the current string builder to a block element
		/// </summary>
		/// <param name="sb"></param>
		private void FlushStringBuilderToBlockElement(StringBuilder sb)
		{
			if (sb.Length > 0 && string.IsNullOrWhiteSpace(sb.ToString()))
				sb.Clear();
			else
			{
				var text = sb.ToString();
				if (text.Any(Char.IsLetterOrDigit)) // If not, just keep anything (probably opening punctuation) in the builder to be included with the next bit of text.
				{
					MoveTrailingElementsIfNecessary();
					m_workingBlock.BlockElements.Add(new ScriptText(text));
					sb.Clear();
				}
			}
		}

		/// <summary>
		/// Block elements which are not scriptText must not be the last elements in their block.
		/// Move them from the end of one block to the beginning of the next.
		/// </summary>
		private void MoveTrailingElementsIfNecessary()
		{
			if (m_outputBlocks.Any())
			{
				Block lastBlock = m_outputBlocks.Last();
				int numRemoved = lastBlock.BlockElements.RemoveAll(m_nonScriptTextBlockElements.Contains);
				if (numRemoved > 0)
				{
					var verse = m_nonScriptTextBlockElements.First() as Verse;
					if (verse != null)
						m_workingBlock.InitialStartVerseNumber = ScrReference.VerseToIntStart(verse.Number);
					m_workingBlock.BlockElements.InsertRange(0, m_nonScriptTextBlockElements);

					// If we removed all block elements, remove the block
					if (!lastBlock.BlockElements.Any())
					{
						m_workingBlock.IsParagraphStart = lastBlock.IsParagraphStart;
						m_outputBlocks.Remove(lastBlock);
					}
				}
			}
			m_nonScriptTextBlockElements.Clear();
		}

		/// <summary>
		/// Flush the current string builder to a block element,
		/// and flush the current block elements to a block
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="styleTag"></param>
		/// <param name="nonNarrator"></param>
		private void FlushStringBuilderAndBlock(StringBuilder sb, string styleTag, bool nonNarrator)
		{
			FlushStringBuilderToBlockElement(sb);
			if (m_workingBlock.BlockElements.Count > 0)
			{
				FlushBlock(styleTag, nonNarrator);
			}
		}

		/// <summary>
		/// Add the working block to the new list and create a new working block
		/// </summary>
		/// <param name="styleTag"></param>
		/// <param name="nonNarrator"></param>
		private void FlushBlock(string styleTag, bool nonNarrator)
		{
			if (!m_workingBlock.BlockElements.Any())
			{
				m_workingBlock.StyleTag = styleTag;
				return;
			}
			if (nonNarrator)
			{
				if (m_nextBlockContinuesQuote)
					m_workingBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
				m_nextBlockContinuesQuote = m_quoteLevel > 0;
				if (m_nextBlockContinuesQuote && m_workingBlock.MultiBlockQuote != MultiBlockQuote.Continuation)
					m_workingBlock.MultiBlockQuote = MultiBlockQuote.Start;

				m_workingBlock.SetCharacterAndDelivery(
					m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber, m_workingBlock.InitialEndVerseNumber, m_workingBlock.LastVerse, m_versification));
			}
			else
			{
				m_nextBlockContinuesQuote = false;
				m_workingBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Narrator);
			}

			switch (m_workingBlock.MultiBlockQuote)
			{
				case MultiBlockQuote.Start:
					ProcessMultiBlock();
					m_currentMultiBlockQuote.Add(m_workingBlock);
					break;
				case MultiBlockQuote.Continuation:
					m_currentMultiBlockQuote.Add(m_workingBlock);
					break;
				case MultiBlockQuote.None:
					ProcessMultiBlock();
					break;
			}

			m_outputBlocks.Add(m_workingBlock);
			var lastVerse = m_workingBlock.BlockElements.OfType<Verse>().LastOrDefault();
			int verseStartNum = m_workingBlock.InitialStartVerseNumber;
			int verseEndNum = m_workingBlock.InitialEndVerseNumber;
			if (lastVerse != null)
			{
				verseStartNum = ScrReference.VerseToIntStart(lastVerse.Number);
				verseEndNum = ScrReference.VerseToIntEnd(lastVerse.Number);
			}
			m_workingBlock = new Block(styleTag, m_workingBlock.ChapterNumber, verseStartNum, verseEndNum);
		}

		private void ProcessMultiBlock()
		{
			if (!m_currentMultiBlockQuote.Any())
				return;

			var uniqueCharacters = m_currentMultiBlockQuote.Select(b => b.CharacterId).Distinct().ToList();
			int numUniqueCharacters = uniqueCharacters.Count();
			var uniqueCharacterDeliveries = m_currentMultiBlockQuote.Select(b => new CharacterDelivery(b.CharacterId, b.Delivery)).Distinct(CharacterDelivery.CharacterDeliveryComparer).ToList();
			int numUniqueCharacterDeliveries = uniqueCharacterDeliveries.Count();
			if (numUniqueCharacterDeliveries > 1)
			{
				var unclearCharacters = new [] { CharacterVerseData.AmbiguousCharacter, CharacterVerseData.UnknownCharacter };
				if (numUniqueCharacters > unclearCharacters.Count(uniqueCharacters.Contains) + 1)
				{
					// More than one real character. Set to Ambiguous.
					SetCharacterAndDeliveryForMultipleBlocks(m_currentMultiBlockQuote, CharacterVerseData.AmbiguousCharacter, null);
				}
				else if (numUniqueCharacters == 2 && unclearCharacters.All(uniqueCharacters.Contains))
				{
					// Only values are Ambiguous and Unique. Set to Ambiguous.
					SetCharacterAndDeliveryForMultipleBlocks(m_currentMultiBlockQuote, CharacterVerseData.AmbiguousCharacter, null);
				}
				else if (numUniqueCharacterDeliveries > numUniqueCharacters)
				{
					// Multiple deliveries for the same character
					string delivery = "";
					bool first = true;
					foreach (Block block in m_currentMultiBlockQuote)
					{
						if (first)
							first = false;
						else if (block.Delivery != delivery)
							block.MultiBlockQuote = MultiBlockQuote.ChangeOfDelivery;
						delivery = block.Delivery;
					}
				}
				else
				{
					// Only one real character (and delivery). Set to that character (and delivery).
					var realCharacter = uniqueCharacterDeliveries.Single(c => c.Character != CharacterVerseData.AmbiguousCharacter && c.Character != CharacterVerseData.UnknownCharacter);
					SetCharacterAndDeliveryForMultipleBlocks(m_currentMultiBlockQuote, realCharacter.Character, realCharacter.Delivery);
				}
				
			}
				
			m_currentMultiBlockQuote.Clear();
		}

		private void SetCharacterAndDeliveryForMultipleBlocks(IEnumerable<Block> blocks, string character, string delivery)
		{
			foreach (Block block in blocks)
			{
				block.CharacterId = character;
				block.Delivery = delivery;
			}
		}

		private bool IsSentenceEnding(char c)
		{
			// Note... while one might think that char.GetUnicodeCategory could tell you if a character was a sentence separator, this is not the case. 
			// This is because, for example, '.' can be used for various things (abbreviation, decimal point, as well as sentence terminator).
			// This should be a complete list of code points with the \p{Sentence_Break=STerm} or \p{Sentence_Break=ATerm} properties that also
			// have the \p{Terminal_Punctuation} property. This list is up-to-date as of Unicode v6.1.
			// ENHANCE: Ideally this should be dynamic, or at least moved into Palaso (this list was copied from HearThis code).
			switch (c)
			{
				case '.':
				case '?':
				case '!':
				case '\u0589': // ARMENIAN FULL STOP
				case '\u061F': // ARABIC QUESTION MARK
				case '\u06D4': // ARABIC FULL STOP
				case '\u0700': // SYRIAC END OF PARAGRAPH
				case '\u0701': // SYRIAC SUPRALINEAR FULL STOP
				case '\u0702': // SYRIAC SUBLINEAR FULL STOP
				case '\u07F9': // NKO EXCLAMATION MARK
				case '\u0964': // DEVANAGARI DANDA
				case '\u0965': // DEVANAGARI DOUBLE DANDA
				case '\u104A': // MYANMAR SIGN LITTLE SECTION
				case '\u104B': // MYANMAR SIGN SECTION
				case '\u1362': // ETHIOPIC FULL STOP
				case '\u1367': // ETHIOPIC QUESTION MARK
				case '\u1368': // ETHIOPIC PARAGRAPH SEPARATOR
				case '\u166E': // CANADIAN SYLLABICS FULL STOP
				case '\u1803': // MONGOLIAN FULL STOP
				case '\u1809': // MONGOLIAN MANCHU FULL STOP
				case '\u1944': // LIMBU EXCLAMATION MARK
				case '\u1945': // LIMBU QUESTION MARK
				case '\u1AA8': // TAI THAM SIGN KAAN
				case '\u1AA9': // TAI THAM SIGN KAANKUU
				case '\u1AAA': // TAI THAM SIGN SATKAAN
				case '\u1AAB': // TAI THAM SIGN SATKAANKUU
				case '\u1B5A': // BALINESE PANTI
				case '\u1B5B': // BALINESE PAMADA
				case '\u1B5E': // BALINESE CARIK SIKI
				case '\u1B5F': // BALINESE CARIK PAREREN
				case '\u1C3B': // LEPCHA PUNCTUATION TA-ROL
				case '\u1C3C': // LEPCHA PUNCTUATION NYET THYOOM TA-ROL
				case '\u1C7E': // OL CHIKI PUNCTUATION MUCAAD
				case '\u1C7F': // OL CHIKI PUNCTUATION DOUBLE MUCAAD
				case '\u203C': // DOUBLE EXCLAMATION MARK
				case '\u203D': // INTERROBANG
				case '\u2047': // DOUBLE QUESTION MARK
				case '\u2048': // QUESTION EXCLAMATION MARK
				case '\u2049': // EXCLAMATION QUESTION MARK
				case '\u2E2E': // REVERSED QUESTION MARK
				case '\u3002': // IDEOGRAPHIC FULL STOP
				case '\uA4FF': // LISU PUNCTUATION FULL STOP
				case '\uA60E': // VAI FULL STOP
				case '\uA60F': // VAI QUESTION MARK
				case '\uA6F3': // BAMUM FULL STOP
				case '\uA6F7': // BAMUM QUESTION MARK
				case '\uA876': // PHAGS-PA MARK SHAD
				case '\uA877': // PHAGS-PA MARK DOUBLE SHAD
				case '\uA8CE': // SAURASHTRA DANDA
				case '\uA8CF': // SAURASHTRA DOUBLE DANDA
				case '\uA92F': // KAYAH LI SIGN SHYA
				case '\uA9C8': // JAVANESE PADA LINGSA
				case '\uA9C9': // JAVANESE PADA LUNGSI
				case '\uAA5D': // CHAM PUNCTUATION DANDA
				case '\uAA5E': // CHAM PUNCTUATION DOUBLE DANDA
				case '\uAA5F': // CHAM PUNCTUATION TRIPLE DANDA
				case '\uAAF0': // MEETEI MAYEK CHEIKHAN
				case '\uAAF1': // MEETEI MAYEK AHANG KHUDAM
				case '\uABEB': // MEETEI MAYEK CHEIKHEI
				case '\uFE52': // SMALL FULL STOP
				case '\uFE56': // SMALL QUESTION MARK
				case '\uFE57': // SMALL EXCLAMATION MARK
				case '\uFF01': // FULLWIDTH EXCLAMATION MARK
				case '\uFF0E': // FULLWIDTH FULL STOP
				case '\uFF1F': // FULLWIDTH QUESTION MARK
				case '\uFF61': // HALFWIDTH IDEOGRAPHIC FULL STOP
				// These would require surrogate pairs
				//'\u11047', // BRAHMI DANDA
				//'\u11048', // BRAHMI DOUBLE DANDA
				//'\u110BE', // KAITHI SECTION MARK
				//'\u110BF', // KAITHI DOUBLE SECTION MARK
				//'\u110C0', // KAITHI DANDA
				//'\u110C1', // KAITHI DOUBLE DANDA
				//'\u11141', // CHAKMA DANDA
				//'\u11142', // CHAKMA DOUBLE DANDA
				//'\u11143', // CHAKMA QUESTION MARK
				//'\u111C5', // SHARADA DANDA
				//'\u111C6', // SHARADA DOUBLE DANDA
					return true;
				default:
					return false;
			}
		}

		private bool IsNormalParagraphStyle(string styleTag)
		{
			return styleTag == "p";
		}

		private bool IsFollowOnParagraphStyle(string styleTag)
		{
			return styleTag.StartsWith("q") || styleTag == "m";
		}

		#region CharacterDelivery utility class
		private class CharacterDelivery
		{
			public readonly string Character;
			public readonly string Delivery;
			
			public CharacterDelivery(string character, string delivery)
			{
				Character = character;
				Delivery = delivery;
			}

			private sealed class CharacterDeliveryEqualityComparer : IEqualityComparer<CharacterDelivery>
			{
				public bool Equals(CharacterDelivery x, CharacterDelivery y)
				{
					if (ReferenceEquals(x, y))
						return true;
					if (ReferenceEquals(x, null))
						return false;
					if (ReferenceEquals(y, null))
						return false;
					if (x.GetType() != y.GetType())
						return false;
					return string.Equals(x.Character, y.Character) && string.Equals(x.Delivery, y.Delivery);
				}

				public int GetHashCode(CharacterDelivery obj)
				{
					unchecked
					{
						return ((obj.Character != null ? obj.Character.GetHashCode() : 0) * 397) ^ (obj.Delivery != null ? obj.Delivery.GetHashCode() : 0);
					}
				}
			}

			private static readonly IEqualityComparer<CharacterDelivery> CharacterDeliveryComparerInstance = new CharacterDeliveryEqualityComparer();

			public static IEqualityComparer<CharacterDelivery> CharacterDeliveryComparer
			{
				get { return CharacterDeliveryComparerInstance; }
			}
		}
		#endregion

		public class SplitterComparer : IComparer<string>
		{
			int IComparer<string>.Compare(string x, string y)
			{
				if (x.Contains(" ") && !y.Contains(" "))
					return -1;
				if (!x.Contains(" ") && y.Contains(" "))
					return 1;
				int result;
				if (x.Contains(" ") && y.Contains(" "))
				{
					result = -x.Length.CompareTo(y.Length);
					if (result != 0)
						return result;
					return String.Compare(x, y, StringComparison.InvariantCulture);
				}
				result = x.Length.CompareTo(y.Length);
				if (result != 0)
					return result;
				return String.Compare(x, y, StringComparison.InvariantCulture);
			}
		}
	}
}
