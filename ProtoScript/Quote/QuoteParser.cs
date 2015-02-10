using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProtoScript.Character;
using SIL.ScriptureUtils;

namespace ProtoScript.Quote
{
	public class QuoteParser
	{
		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteSystem m_quoteSystem;

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
		}

		/// <summary>
		/// Parse through the given blocks character by character to determine where we need to break based on quotes 
		/// </summary>
		/// <param name="progress">If provided, reports how many blocks have been processed</param>
		/// <returns>A new enumerable of blocks broken up for quotes</returns>
		public IEnumerable<Block> Parse(IProgress<int> progress = null)
		{
			if (m_quoteSystem == null)
				return m_inputBlocks;
			m_outputBlocks = new List<Block>();
			var sb = new StringBuilder();
			var possibleStartQuoteMarker = new StringBuilder();
			var possibleEndQuoteMarker = new StringBuilder();
			bool quoteStartMulticharacter = m_quoteSystem.StartQuoteMarker.Length > 1;
			bool quoteEndMulticharacter = m_quoteSystem.EndQuoteMarker.Length > 1;
			m_quoteLevel = 0;
			bool quoteEndPending = false;
			bool quoteStartPending = false;
			bool dialogueQuoteEndPending = false;
			bool blockEndedWithSentenceEndingPunctuation = false;
			Block blockInWhichDialogueQuoteStarted = null;
			int blocksCompleted = 0;
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

					if (progress != null)
						progress.Report(blocksCompleted++);

					continue;
				}

				if (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null && (!IsNormalParagraphStyle(blockInWhichDialogueQuoteStarted.StyleTag) || blockEndedWithSentenceEndingPunctuation || !IsFollowOnParagraphStyle(block.StyleTag)))
					m_quoteLevel--;

				possibleStartQuoteMarker.Clear();
				possibleEndQuoteMarker.Clear();
				bool beyondLeadingPunctuation = false;
				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber) { IsParagraphStart = block.IsParagraphStart };
				
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						if (quoteEndPending)
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							quoteEndPending = false;
						}

						// Add the element to our working list in case we need to move it to the next block (see MoveTrailingElementsIfNecessary)
						m_nonScriptTextBlockElements.Add(element);

						m_workingBlock.BlockElements.Add(element);
						continue;
					}
					sb.Clear();
					foreach (char c in scriptText.Content)
					{
						string ch = Char.ToString(c);

						if (possibleStartQuoteMarker.Length > 0)
						{
							possibleStartQuoteMarker.Append(ch);
							if (IsStartOfRegularQuote(possibleStartQuoteMarker.ToString()))
							{
								quoteStartPending = true;
							}
							else if (IsPossibleStartOfRegularQuote(possibleStartQuoteMarker.ToString()))
							{
								continue;
							}
							else
							{
								sb.Append(possibleStartQuoteMarker);
								possibleStartQuoteMarker.Clear();
							}
						}
						if (!quoteEndPending && possibleEndQuoteMarker.Length > 0)
						{
							possibleEndQuoteMarker.Append(ch);
							if (IsEndOfRegularQuote(possibleEndQuoteMarker.ToString()))
							{
								//Do nothing
							}
							else if (IsPossibleEndOfRegularQuote(possibleEndQuoteMarker.ToString()))
							{
								continue;
							}
							else
							{
								sb.Append(possibleEndQuoteMarker);
								possibleEndQuoteMarker.Clear();
							}
						}

						if (Char.IsPunctuation(c))
						{
							if (!beyondLeadingPunctuation && m_quoteLevel > 0 && m_workingBlock.IsParagraphStart)
							{
								// If we are in a quote, skip any paragraph-leading punctuation for proper processing of continuation quotes
								sb.Append(c);
								continue;
							}
						}
						else
							beyondLeadingPunctuation = true;

						if (quoteStartPending)
						{
							if (Char.IsWhiteSpace(c))
							{
								sb.Append(c);
								continue;
							}
							if (!IsStartOfQuote(ch))
							{
								FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								m_quoteLevel++;
							}
							quoteStartPending = false;
						}
						if (quoteEndPending)
						{
							if (!IsStartOfQuote(ch) && IsAddOnCharacter(c))
							{
								sb.Append(c);
								continue;
							}
							FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							quoteEndPending = false;
						}
						if (dialogueQuoteEndPending)
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							dialogueQuoteEndPending = false;
							sb.Append(m_quoteSystem.QuotationDashEndMarker);
						}

						if (m_quoteLevel > 0 && IsStartOfRegularQuote(ch) && !IsStartAndEndMarkersSame())
							m_quoteLevel++;
						else if (m_quoteLevel > 1 && IsEndOfRegularQuote(ch) && !IsStartAndEndMarkersSame())
							m_quoteLevel--;
						else if (m_quoteLevel == 0 && (IsStartOfRegularQuote(quoteStartMulticharacter, ch, possibleStartQuoteMarker.ToString()) || IsStartOfDialogQuote(ch)))
						{
							blockInWhichDialogueQuoteStarted = IsStartOfRegularQuote(quoteStartMulticharacter, ch, possibleStartQuoteMarker.ToString()) ? null : block;
							if (c == ':') // For quotes introduced using a colon, the colon belongs with the preceding block.
								quoteStartPending = true;
							else
							{
								FlushStringBuilderAndBlock(sb, block.StyleTag, m_quoteLevel > 0);
								m_quoteLevel++;
							}
							blockEndedWithSentenceEndingPunctuation = false;
						}
						else if (quoteStartMulticharacter && m_quoteLevel == 0 && (IsPossibleStartOfRegularQuote(ch) || IsStartOfDialogQuote(ch)))
						{
							possibleStartQuoteMarker.Append(ch);
							continue;
						}
						else if (blockInWhichDialogueQuoteStarted == null && IsEndOfRegularQuote(quoteEndMulticharacter, ch, possibleEndQuoteMarker.ToString()))
						{
							quoteEndPending = true;
							m_quoteLevel--;
						}
						else if (quoteEndMulticharacter && blockInWhichDialogueQuoteStarted == null && IsPossibleEndOfRegularQuote(ch))
						{
							possibleEndQuoteMarker.Append(ch);
							continue;
						}
						else if (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null)
						{
							if (m_quoteSystem.QuotationDashEndMarker == QuoteSystem.AnyPunctuation && Char.IsPunctuation(c))
							{
								quoteEndPending = true;
								m_quoteLevel--;
							}
							else if (ch.Equals(m_quoteSystem.QuotationDashEndMarker))
							{
								dialogueQuoteEndPending = true;
								m_quoteLevel--;
								continue;
							}
							else if (blockInWhichDialogueQuoteStarted == block)
							{
								if (char.IsPunctuation(c))
								{
									if (IsSentenceEnding(c))
										blockEndedWithSentenceEndingPunctuation = true;
								}
								else if (!char.IsWhiteSpace(c))
								{
									blockEndedWithSentenceEndingPunctuation = false;
								}
							}
						}
						if (m_quoteLevel < 0)
							Debug.Fail("Quote level should never be less than 0");
						if (possibleStartQuoteMarker.Length > 0)
						{
							sb.Append(possibleStartQuoteMarker);
							possibleStartQuoteMarker.Clear();
						}
						else if (quoteEndPending && possibleEndQuoteMarker.Length > 0)
						{
							sb.Append(possibleEndQuoteMarker);
							possibleEndQuoteMarker.Clear();
						}
						else
							sb.Append(c);
					}
					FlushStringBuilderToBlockElement(sb);
				}
				FlushBlock(block.StyleTag, quoteEndPending || m_quoteLevel > 0);

				if (progress != null)
					progress.Report(blocksCompleted++);
			}
			return m_outputBlocks;
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

		/// <summary>
		/// The given character should be attached to whatever text precedes it.
		/// For example, given this text:
		///		«Go»! he said.
		/// The first block should contain the trailing punctuation and whitespace:
		///		«Go»! 
		///		he said.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsAddOnCharacter(char character)
		{
			return Char.IsWhiteSpace(character) || Char.IsPunctuation(character);
		}

		/// <summary>
		/// The given string represents the beginning of a first-level quote (indicated by a regular start/end pair)
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsStartOfRegularQuote(string character)
		{
			return character.Equals(m_quoteSystem.StartQuoteMarker);
		}

		private bool IsStartOfRegularQuote(bool multicharacter, string character, string possibleMulticharacterQuoteMarker)
		{
			if (multicharacter)
				return possibleMulticharacterQuoteMarker.Equals(m_quoteSystem.StartQuoteMarker);
			return character.Equals(m_quoteSystem.StartQuoteMarker);
		}

		/// <summary>
		/// The given string represents the beginning of a dialog quote
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsStartOfDialogQuote(string character)
		{
			return character.Equals(m_quoteSystem.QuotationDashMarker);
		}

		/// <summary>
		/// The given string represents the beginning of a quote
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsStartOfQuote(string character)
		{
			return character.Equals(m_quoteSystem.StartQuoteMarker) || character.Equals(m_quoteSystem.QuotationDashMarker);
		}

		/// <summary>
		/// The given string represents the start of a beginning quote marker
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsPossibleStartOfRegularQuote(string character)
		{
			return m_quoteSystem.StartQuoteMarker.StartsWith(character);
		}

		/// <summary>
		/// The given string represents the start of a ending quote marker
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsPossibleEndOfRegularQuote(string character)
		{
			return m_quoteSystem.EndQuoteMarker.StartsWith(character);
		}

		/// <summary>
		/// The given string represents the end of a first-level quote (indicated by a regular start/end pair)
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsEndOfRegularQuote(string character)
		{
			return character.Equals(m_quoteSystem.EndQuoteMarker);
		}

		private bool IsEndOfRegularQuote(bool multicharacter, string character, string possibleMulticharacterQuoteMarker)
		{
			if (multicharacter)
				return possibleMulticharacterQuoteMarker.Equals(m_quoteSystem.EndQuoteMarker);
			return character.Equals(m_quoteSystem.EndQuoteMarker);
		}

		private bool IsStartAndEndMarkersSame()
		{
			return m_quoteSystem.StartQuoteMarker.Equals(m_quoteSystem.EndQuoteMarker);
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
