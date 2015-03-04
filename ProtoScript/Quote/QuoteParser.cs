using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Palaso.Extensions;
using ProtoScript.Character;
using SIL.ScriptureUtils;
using Utilities;

namespace ProtoScript.Quote
{
	public class QuoteParser
	{
		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteSystem m_quoteSystem;
		private readonly Regex m_regexSplitQuoteTokens;
		private string m_quoteCharacters;

		#region working members
		// These members are used by several methods. Making them class-level prevents passing them repeatedly
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		private int m_quoteLevel;
		private bool m_nextBlockContinuesQuote;
		#endregion

		/// <summary>
		/// Create a QuoteParser using the default QuoteSystem
		/// </summary>
		/// <param name="cvInfo"></param>
		/// <param name="bookId"></param>
		/// <param name="blocks"></param>
		public QuoteParser(ICharacterVerseInfo cvInfo, string bookId, IEnumerable<Block> blocks) : this(cvInfo, bookId, blocks, QuoteSystem.Default)
		{
		}

		public QuoteParser(ICharacterVerseInfo cvInfo, string bookId, IEnumerable<Block> blocks, QuoteSystem quoteSystem)
		{
			m_cvInfo = cvInfo;
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(bookId);
			m_inputBlocks = blocks;
			m_quoteSystem = quoteSystem;
			m_regexSplitQuoteTokens = GetRegExForSplittingQuotes();
		}

		private Regex GetRegExForSplittingQuotes()
		{
			var splitters = new HashSet<string>();
			var quoteChars = new HashSet<char>();

			foreach (var level in m_quoteSystem.Levels)
			{
				splitters.Add(level.Open);
				splitters.Add(level.Close);
				if (!string.IsNullOrWhiteSpace(level.Continue))
					splitters.Add(level.Continue);	
			}

			foreach (var ch in splitters.SelectMany(qm => qm.Where(c => !Char.IsWhiteSpace(c))))
			{
				quoteChars.Add(ch);
			}
			m_quoteCharacters = quoteChars.Concat(string.Empty);

			if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashMarker))
			{
				splitters.Add(m_quoteSystem.QuotationDashMarker);
				foreach (char ch in m_quoteSystem.QuotationDashMarker)
					quoteChars.Add(ch);
				if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker) &&
					m_quoteSystem.QuotationDashEndMarker != QuoteSystem.AnyPunctuation)
				{
					splitters.Add(m_quoteSystem.QuotationDashEndMarker);
					foreach (char ch in m_quoteSystem.QuotationDashEndMarker)
						quoteChars.Add(ch);
				}
			}

			var sbQuoteMatcher = new StringBuilder();

			foreach (var qm in splitters)
			{
				sbQuoteMatcher.Append("(?:");
				sbQuoteMatcher.Append(Regex.Escape(qm));
				sbQuoteMatcher.Append(")|");
			}
			sbQuoteMatcher.Length--;

			string punctuation = "";
			if (m_quoteSystem.QuotationDashEndMarker == QuoteSystem.AnyPunctuation)
				// (?!\w) is to ensure we only get word-final punctuation
				punctuation = string.Format(@"|(?:[^\w\s\d{0}](?!\w)\s*)", Regex.Escape(m_quoteCharacters));

			var quoteMatcher = sbQuoteMatcher.ToString();
			// quoteMatcher includes all the possible markers; e.g. (?:«)|(?:‹)|(?:›)|(?:»).
			// Need to group because they could be more than one character each.
			// The outer () means we want to include the delimiter in the results
			// ?: => non-matching group
			// \s => whitespace
			// \w => word-forming character
			return new Regex(String.Format(@"((?:(?:{0})(?:[^\w{1}])*){2})", quoteMatcher, Regex.Escape(quoteChars.Concat(string.Empty)), punctuation), RegexOptions.Compiled);
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
			foreach (Block block in m_inputBlocks)
			{
				if (block.UserConfirmed)
					throw new InvalidOperationException("Should not be parsing blocks that already have user-decisions applied.");

				if (block.CharacterIsStandard && !block.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
				{
					// The following handles the case where an open quote is interrupted by a section head or chapter break
					var lastBlockAdded = m_outputBlocks.LastOrDefault();
					if (lastBlockAdded != null && lastBlockAdded.MultiBlockQuote == MultiBlockQuote.Start)
						lastBlockAdded.MultiBlockQuote = MultiBlockQuote.None;
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
				}

				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber) { IsParagraphStart = block.IsParagraphStart };

				bool atBeginningOfBlock = true;
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

					foreach (var token in m_regexSplitQuoteTokens.Split(scriptText.Content).Where(t => t.Length > 0))
					{
						if (atBeginningOfBlock)
						{
							atBeginningOfBlock = false;

							if (m_quoteLevel > 0 && token.StartsWith(ContinuerForCurrentLevel))
							{
								sb.Append(token);
								continue;
							}
						}

						if (m_quoteLevel > 0 && token.StartsWith(CloserForCurrentLevel) && blockInWhichDialogueQuoteStarted == null)
						{
							sb.Append(token);
							if (--m_quoteLevel == 0)
								FlushStringBuilderAndBlock(sb, block.StyleTag, true);
						}
						else if (m_quoteSystem.Levels.Count > m_quoteLevel && token.StartsWith(OpenerForNextLevel) && blockInWhichDialogueQuoteStarted == null)
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
						else if (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null)
						{
							if (m_quoteSystem.QuotationDashEndMarker == QuoteSystem.AnyPunctuation && IsNonQuotePunctuation(token[0]))
							{
								m_quoteLevel--;
								blockInWhichDialogueQuoteStarted = null;
								sb.Append(token);
								FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							}
							else
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
						}
						else
						{
							sb.Append(token);
						}
					}
					FlushStringBuilderToBlockElement(sb);
				}
				FlushBlock(block.StyleTag, m_quoteLevel > 0);
			}
			return m_outputBlocks;
		}

		public string ContinuerForCurrentLevel { get { return m_quoteSystem.Levels[m_quoteLevel - 1].Continue; } }
		public string CloserForCurrentLevel { get { return m_quoteSystem.Levels[m_quoteLevel - 1].Close; } }
		public string OpenerForNextLevel { get { return m_quoteSystem.Levels[m_quoteLevel].Open; } }

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

		private bool IsNonQuotePunctuation(char c)
		{
			return char.IsPunctuation(c) && !m_quoteCharacters.Contains(c);
		}

		/// <summary>
		/// Flush the current string builder to a block element
		/// </summary>
		/// <param name="sb"></param>
		private void FlushStringBuilderToBlockElement(StringBuilder sb)
		{
			if (sb.Length > 0 && string.IsNullOrWhiteSpace(sb.ToString()))
				sb.Clear();
			else if (sb.Length > 0)
			{
				MoveTrailingElementsIfNecessary();
				m_workingBlock.BlockElements.Add(new ScriptText(sb.ToString()));
				sb.Clear();
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
					m_cvInfo.GetCharacters(m_bookId, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber, m_workingBlock.InitialEndVerseNumber));
			}
			else
			{
				m_nextBlockContinuesQuote = false;
				m_workingBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Narrator);
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
	}
}
