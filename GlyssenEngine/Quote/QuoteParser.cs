using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using SIL.Extensions;
using SIL.Scripture;
using SIL.Unicode;
using static System.Char;

namespace GlyssenEngine.Quote
{
	public class QuoteParser
	{
		public static void ParseProject(Project project, BackgroundWorker projectWorker, IEnumerable<string> bookIdsToParse)
		{
			var cvInfo = new ParserCharacterRepository(new CombinedCharacterVerseData(project), project.ReferenceText);

			var numBlocksPerBook = new ConcurrentDictionary<string, int>();
			var blocksInBook = new ConcurrentDictionary<BookScript, IReadOnlyList<Block>>();
			IEnumerable<BookScript> books = bookIdsToParse == null ? project.Books :
				project.Books.Where(b => bookIdsToParse.Contains(b.BookId));
			Parallel.ForEach(books, book =>
			{
				var nodeList = book.GetScriptBlocks();
				blocksInBook.AddOrUpdate(book, nodeList, (script, list) => nodeList);
				numBlocksPerBook.AddOrUpdate(book.BookId, nodeList.Count, (s, i) => nodeList.Count);
			});
			int allProjectBlocks = numBlocksPerBook.Values.Sum();

			int completedProjectBlocks = 0;
			SetQuoteSystem(project.QuoteSystem);
			Parallel.ForEach(blocksInBook.Keys, book =>
			{
				book.Blocks = new QuoteParser(cvInfo, book.BookId, blocksInBook[book], project.Versification).Parse().ToList();
				completedProjectBlocks += numBlocksPerBook[book.BookId];
				projectWorker.ReportProgress(MathUtilities.Percent(completedProjectBlocks, allProjectBlocks, 99));
			});

			projectWorker.ReportProgress(100);
		}

		public static void SetQuoteSystem(QuoteSystem quoteSystem)
		{
			s_quoteSystem = quoteSystem;
		}

		private readonly ICharacterVerseRepository m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private static QuoteSystem s_quoteSystem = QuoteSystem.Default;
		private readonly ScrVers m_versification;
		private readonly List<Regex> m_regexes = new List<Regex>();
		private Regex m_regexStartsWithFirstLevelOpener;
		private Regex m_regexHasFirstLevelClose;

		#region working (state) members
		// These members are used by several methods. Making them class-level avoids having to pass them repeatedly.
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		private int m_quoteLevel;
		private bool m_nextBlockContinuesQuote;
		private readonly List<Block> m_currentMultiBlockQuote = new List<Block>();
		private List<string> m_possibleCharactersForCurrentQuote = new List<string>();
		private bool m_ignoringNarratorQuotation;
		private int m_outputBlockInWhichPoetryStarted = -1;
		#endregion

		public QuoteParser(ICharacterVerseRepository cvInfo, string bookId, IEnumerable<Block> blocks, ScrVers versification = null)
		{
			m_cvInfo = cvInfo;
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(bookId);
			m_inputBlocks = blocks;
			m_versification = versification ?? ScrVers.English;
			GetRegExesForSplittingQuotes();
		}

		private void GetRegExesForSplittingQuotes()
		{
			IList<string> splitters;
			var quoteChars = new SortedSet<char>(new QuoteCharComparer());
			var regexExpressions = new List<string>(s_quoteSystem.NormalLevels.Count);

			if (s_quoteSystem.NormalLevels.Count > 0)
			{
				m_regexStartsWithFirstLevelOpener = new Regex(Regex.Escape(s_quoteSystem.NormalLevels[0].Open), RegexOptions.Compiled);
				m_regexHasFirstLevelClose = new Regex(Regex.Escape(s_quoteSystem.NormalLevels[0].Close), RegexOptions.Compiled);

				// At level x, we need continuer x, closer x, opener x+1.  Continuer must be first.
				for (int level = 0; level < s_quoteSystem.NormalLevels.Count; level++)
				{
					splitters = new List<string>();
					if (level > 0)
					{
						var quoteSystemLevelMinusOne = s_quoteSystem.NormalLevels[level - 1];
						if (!string.IsNullOrWhiteSpace(quoteSystemLevelMinusOne.Continue))
							splitters.Add(quoteSystemLevelMinusOne.Continue);
						splitters.Add(quoteSystemLevelMinusOne.Close);
					}
					else if (level == 0 && s_quoteSystem.NormalLevels[0].Continue != s_quoteSystem.NormalLevels[0].Open)
						splitters.Add(s_quoteSystem.NormalLevels[0].Continue);
					splitters.Add(s_quoteSystem.NormalLevels[level].Open);
					if (level == 0)
						AddQuotationDashStart(splitters);
					else if (level == 1)
						AddQuotationDashEnd(splitters);

					regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
				}

				// Final regex handles when we are inside the innermost level
				splitters = new List<string>();
				splitters.Add(s_quoteSystem.NormalLevels.Last().Close);
				splitters.Add(s_quoteSystem.NormalLevels.Last().Continue);
				if (s_quoteSystem.NormalLevels.Count == 1)
					AddQuotationDashEnd(splitters);

				regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
			}
			else
			{
				splitters = new List<string>();
				AddQuotationDashStart(splitters);
				AddQuotationDashEnd(splitters);
				regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
			}

			// Get all unique characters which make up all quote marks
			StringBuilder sbAllCharacters = new StringBuilder();
			foreach (var quoteSystemLevel in s_quoteSystem.AllLevels)
			{
				sbAllCharacters.Append(quoteSystemLevel.Open);
				sbAllCharacters.Append(quoteSystemLevel.Close);
				if (!String.IsNullOrWhiteSpace(quoteSystemLevel.Continue))
					sbAllCharacters.Append(quoteSystemLevel.Continue);
			}
			quoteChars.AddRange(sbAllCharacters.ToString().Where(c => !IsWhiteSpace(c)));

			foreach (var expr in regexExpressions)
			{
				m_regexes.Add(new Regex(String.Format(expr,
					Regex.Escape(string.Join(string.Empty, quoteChars)),
					Regex.Escape(GetOpeningPunctuationAsSingleString())), RegexOptions.Compiled));
			}
		}

		private void AddQuotationDashStart(IList<string> splitters)
		{
			if (!string.IsNullOrEmpty(s_quoteSystem.QuotationDashMarker))
				splitters.Add(s_quoteSystem.QuotationDashMarker);
		}

		private void AddQuotationDashEnd(IList<string> splitters)
		{
			if (!string.IsNullOrEmpty(s_quoteSystem.QuotationDashMarker) && !string.IsNullOrEmpty(s_quoteSystem.QuotationDashEndMarker))
				splitters.Add(s_quoteSystem.QuotationDashEndMarker);
		}

		private static string BuildQuoteMatcherRegex(IList<string> splitters)
		{
			if (splitters.All(String.IsNullOrEmpty))
				return @"\b\B"; // Guaranteed to match nothing!

			var sbQuoteMatcher = new StringBuilder();

			foreach (var qm in splitters.Where(s => !string.IsNullOrEmpty(s)).Distinct())
			{
				sbQuoteMatcher.Append("(?:");
				sbQuoteMatcher.Append(Regex.Escape(qm));
				// It's extremely unlikely that a quote would ever start with a numeral,
				// so we could probably safely add this look-ahead for any quote marker.
				// But the main purpose here is to prevent breaking a number range with
				// a dash (or any kind) by accidentally interpreting the dash as the
				// start of dialogue.
				if (qm == "-" || qm == "\u2014" || qm == "\u2015")
					sbQuoteMatcher.Append(@"(?!\d)");
				sbQuoteMatcher.Append(")|");
			}
			sbQuoteMatcher.Length--;

			var quoteMatcher = sbQuoteMatcher.ToString();
			// quoteMatcher includes all the possible markers; e.g. (?:«)|(?:‹)|(?:›)|(?:»).
			// Need to group because they could be more than one character each.
			// ?: => non-matching group
			// \w => word-forming character
			return String.Format(@"((?:(?:{0})(?:[^ \w{1}])* *))", quoteMatcher, "{0}{1}");
		}

		/// <summary>
		/// Parse through the given blocks character by character to determine where we need to break based on quotes
		/// </summary>
		/// <returns>A new enumerable of blocks broken up for quotes</returns>
		public IEnumerable<Block> Parse()
		{
			m_outputBlocks = new List<Block>();
			var sb = new StringBuilder();
			m_quoteLevel = 0;
			bool blockEndedWithSentenceEndingPunctuation = false;
			Block blockInWhichDialogueQuoteStarted = null;
			bool potentialDialogueContinuer = false;
			bool inPairedFirstLevelQuote = false;
			bool pendingColon = false;
			foreach (Block block in m_inputBlocks)
			{
				if (block.UserConfirmed)
					throw new InvalidOperationException($"Should not be parsing blocks that already have user-decisions applied. ({m_bookId} {block.ChapterNumber}:{block.InitialStartVerseNumber}");

				if (!block.IsScripture)
				{
					m_nextBlockContinuesQuote = false;
					m_outputBlocks.Add(block);
					continue;
				}

				if (block.IsPredeterminedFirstLevelQuoteStart)
				{
					if (block.CharacterId == null)
					{
						m_quoteLevel = 0;
						IncrementQuoteLevel(block);
					}
					else
					{
						m_outputBlocks.Add(block);
						m_quoteLevel = 1;
						m_possibleCharactersForCurrentQuote = new List<string>(new [] {block.CharacterId});
						continue;
					}
				}
				else if (block.IsPredeterminedFirstLevelQuoteEnd)
				{
					m_quoteLevel = 1;
					DecrementQuoteLevel();
				}

				bool thisBlockStartsWithAContinuer = false;

				if (StyleToCharacterMappings.IncludesCharStyle(block.StyleTag))
				{
					var cvInfo = GetMatchingCharacter(m_cvInfo.GetCharacters(m_bookNum, block.ChapterNumber, block.AllVerses, m_versification),
						new CharacterVerseData.SimpleCharacterInfoWithoutDelivery(block.CharacterId));
					if (cvInfo ==  null)
					{
						block.CharacterId = CharacterVerseData.kNeedsReview;
					}
					else
					{
						block.CharacterId = cvInfo.Character;
						block.Delivery = cvInfo.Delivery;
					}

					if (sb.Length > 0)
					{
						var pendingText = sb.ToString();
						Debug.Assert(!pendingText.Any(IsLetterOrDigit));
						if (!(block.BlockElements.First() is ScriptText textElement))
							throw new Exception($"Pending text \"{pendingText}\" was left from " +
								$"previous block, but following block does not start with text: {block.ToString(true, m_bookId)}");
						textElement.Content = pendingText + textElement.Content;
						sb.Clear();
					}
					m_nextBlockContinuesQuote = false;
					m_workingBlock = block;
					MoveTrailingElementsIfNecessary();
					m_outputBlocks.Add(m_workingBlock);
					continue;
				}

				if (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null &&
					(!IsNormalParagraphStyle(blockInWhichDialogueQuoteStarted.StyleTag) || blockEndedWithSentenceEndingPunctuation ||
					!block.IsFollowOnParagraphStyle))
				{
					DecrementQuoteLevel();
					inPairedFirstLevelQuote = false;
					blockInWhichDialogueQuoteStarted = null;
					m_nextBlockContinuesQuote = potentialDialogueContinuer = !string.IsNullOrEmpty(s_quoteSystem.QuotationDashEndMarker) ||
						(s_quoteSystem.NormalLevels.Count > 0 && s_quoteSystem.NormalLevels[0].Continue != s_quoteSystem.NormalLevels[0].Open);
				}

				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber)
					{ IsParagraphStart = block.IsParagraphStart, MultiBlockQuote = block.MultiBlockQuote, CharacterId = block.CharacterId };
				if (!m_workingBlock.IsContinuationParagraphStyle && m_workingBlock.IsFollowOnParagraphStyle)
				{
					if (m_outputBlockInWhichPoetryStarted < 0)
						m_outputBlockInWhichPoetryStarted = m_outputBlocks.Count;
				}
				else
				{
					ProcessPossibleRunOfPoetryBlocksAsScripture();
				}

				bool atBeginningOfBlock = true;
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						if (!element.CanBeLastElementInBlock)
						{
							// Add the element to our working list in case we need to move it to the next block (see MoveTrailingElementsIfNecessary)
							m_nonScriptTextBlockElements.Add(element);
						}

						var verseElement = element as Verse;
						if (verseElement != null)
						{
							if (!m_workingBlock.BlockElements.Any())
								SetBlockInitialVerseFromVerseElement(verseElement);

							if (m_possibleCharactersForCurrentQuote.Any())
							{
								m_possibleCharactersForCurrentQuote = m_possibleCharactersForCurrentQuote.Intersect(
									m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, verseElement, m_versification, true)
									.Select(cv => cv.Character)).ToList();

								if (!m_possibleCharactersForCurrentQuote.Any())
								{
									foreach (var multiBlock in m_currentMultiBlockQuote)
									{
										multiBlock.MultiBlockQuote = MultiBlockQuote.None;
										multiBlock.SetNonDramaticCharacterId(CharacterVerseData.kUnexpectedCharacter);
									}
									m_currentMultiBlockQuote.Clear();
									FlushStringBuilderAndBlock(sb, block.StyleTag, m_quoteLevel > 0, true);
									SetBlockInitialVerseFromVerseElement(verseElement);
									if (!pendingColon)
										m_quoteLevel = 0;
									m_nextBlockContinuesQuote = false;
								}
							}
						}

						if (sb.Length > 0)
						{
							// Paragraph starts with non-word-forming characters followed by verse number
							m_workingBlock.BlockElements.Add(new ScriptText(sb.ToString()));
						}
						m_workingBlock.BlockElements.Add(element);
						continue;
					}
					sb.Clear();

					var content = scriptText.Content;
					var pos = 0;
					while (pos < content.Length)
					{
						if (pendingColon)
						{
							if (s_quoteSystem.NormalLevels.Count > 0)
							{
								if (AtOpeningFirstLevelQuoteThatSeemsToBeMoreThanJustAnExpression(content, pos))
								{
									DecrementQuoteLevel();
									Debug.Assert(m_quoteLevel > 0 || blockInWhichDialogueQuoteStarted == null);
								}
								else
									blockInWhichDialogueQuoteStarted = block;
							}
							else
							{
								blockInWhichDialogueQuoteStarted = block;
							}
							pendingColon = false;
						}

						var regex = m_regexes[m_quoteLevel >= m_regexes.Count ? m_regexes.Count - 1 : m_quoteLevel];
						var match = regex.Match(content, pos);
						if (match.Success)
						{
							var specialOpeningPunctuationLen = 0;
							if (match.Index > pos)
							{
								specialOpeningPunctuationLen = GetSpecialOpeningPunctuation(content).Length;
								sb.Append(content.Substring(pos, match.Index - pos));
							}

							pos = match.Index + match.Length;

							var token = match.Value;

							if (specialOpeningPunctuationLen > 0)
							{
								// this is only the beginning of a block if there are no characters between the special opening punctuation and the quotation mark
								atBeginningOfBlock &= string.IsNullOrWhiteSpace(content.Substring(specialOpeningPunctuationLen, match.Index - specialOpeningPunctuationLen));
							}
							else
								atBeginningOfBlock &= match.Index == 0;

							if (atBeginningOfBlock)
							{
								if (specialOpeningPunctuationLen == 0)
									atBeginningOfBlock = false;

								if (m_quoteLevel > 0 && s_quoteSystem.NormalLevels.Count > m_quoteLevel - 1 && token.StartsWith(ContinuerForCurrentLevel))
								{
									thisBlockStartsWithAContinuer = true;
									int i = ContinuerForCurrentLevel.Length;
									while (i < token.Length && IsWhiteSpace(token[i]))
										i++;
									sb.Append(token.Substring(0, i));
									if (token.Length == i)
										continue;
									token = token.Substring(i);
								}
								if (m_quoteLevel == 0 && s_quoteSystem.NormalLevels.Count > 0)
								{
									string continuerForNextLevel = ContinuerForNextLevel;
									if (string.IsNullOrEmpty(continuerForNextLevel) || !token.StartsWith(continuerForNextLevel))
										potentialDialogueContinuer = false;
									else
									{
										thisBlockStartsWithAContinuer = true;
										if (continuerForNextLevel != OpenerForNextLevel)
										{
											IncrementQuoteLevel();
											sb.Append(token);
											continue;
										}
									}
								}
								else
								{
									potentialDialogueContinuer = false;
								}
							}
							if (!thisBlockStartsWithAContinuer)
								potentialDialogueContinuer = false;

							if ((m_quoteLevel > 0) && (s_quoteSystem.NormalLevels.Count > 0) &&
								token.StartsWith(CloserForCurrentLevel) && blockInWhichDialogueQuoteStarted == null &&
								!ProbablyAnApostrophe(content, match.Index) && inPairedFirstLevelQuote)
							{
								sb.Append(token);
								DecrementQuoteLevel();
								if (m_quoteLevel == 0)
								{
									if (potentialDialogueContinuer && OpenerForNextLevel == ContinuerForNextLevel)
									{
										foreach (var multiBlock in m_currentMultiBlockQuote)
											multiBlock.MultiBlockQuote = MultiBlockQuote.None;
										m_currentMultiBlockQuote.Clear();
										m_nextBlockContinuesQuote = false;
									}
									if (m_ignoringNarratorQuotation)
										m_ignoringNarratorQuotation = false;
									else
										FlushStringBuilderAndBlock(sb, block.StyleTag, true);
									potentialDialogueContinuer = false;
									inPairedFirstLevelQuote = false;
								}
							}
							else if (s_quoteSystem.NormalLevels.Count > m_quoteLevel && token.StartsWith(OpenerForNextLevel) && blockInWhichDialogueQuoteStarted == null)
							{
								if (m_quoteLevel == 0 && (sb.Length > 0 || m_workingBlock.BlockElements.OfType<ScriptText>().Any(e => !e.Content.All(IsPunctuation))))
								{
									var characters = m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.LastVerse, m_versification);
									// PG-814: If the only character for this verse is a narrator "Quotation", then do not treat it as speech.
									// Also, if the verse has an implicit character with the possibility of a self-quote, ignore it.
									// (There is a slight chance a stray "he said" could mess us up here, but that's unlikely.)
									CharacterSpeakingMode onlyChar = characters.OnlyOrDefault();
									if (onlyChar?.QuoteType == QuoteType.Quotation &&
										CharacterVerseData.IsCharacterOfType(onlyChar.Character, CharacterVerseData.StandardCharacter.Narrator) ||
										onlyChar?.QuoteType == QuoteType.ImplicitWithPotentialSelfQuote)
									{
										m_ignoringNarratorQuotation = true;
									}
									else
										FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								}
								sb.Append(token);
								IncrementQuoteLevel();
								inPairedFirstLevelQuote = true;
							}
							else if (m_quoteLevel == 0 && s_quoteSystem.QuotationDashMarker != null && token.StartsWith(s_quoteSystem.QuotationDashMarker))
							{
								blockEndedWithSentenceEndingPunctuation = false;
								pendingColon = token.StartsWith(":");
								if (pendingColon)
								{
									blockInWhichDialogueQuoteStarted = null;
									sb.Append(token);
								}
								FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								if (!pendingColon)
								{
									blockInWhichDialogueQuoteStarted = block;
									sb.Append(token);
								}
								IncrementQuoteLevel();
								inPairedFirstLevelQuote = false;
							}
							else if (potentialDialogueContinuer || (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null))
							{
								if (!string.IsNullOrEmpty(s_quoteSystem.QuotationDashEndMarker) && token.StartsWith(s_quoteSystem.QuotationDashEndMarker, StringComparison.Ordinal))
								{
									DecrementQuoteLevel();
									potentialDialogueContinuer = false;
									blockInWhichDialogueQuoteStarted = null;
									FlushStringBuilderAndBlock(sb, block.StyleTag, true);
								}
								else
								{
									blockEndedWithSentenceEndingPunctuation = !m_workingBlock.IsFollowOnParagraphStyle && token.EndsWithSentenceEndingPunctuation();
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
							if (m_quoteLevel == 1 && block == blockInWhichDialogueQuoteStarted && remainingText.EndsWithSentenceEndingPunctuation())
								blockEndedWithSentenceEndingPunctuation = true;
							sb.Append(remainingText);
							break;
						}
					}
					FlushStringBuilderToBlockElement(sb);
					if (sb.Length > 0)
					{
						if (!block.IsParagraphStart)
							m_outputBlocks.Last().BlockElements.OfType<ScriptText>().Last().Content += sb.ToString();
					}
				}
				FlushBlock(block.StyleTag, m_quoteLevel > 0);
			}
			if (blockInWhichDialogueQuoteStarted != null || !inPairedFirstLevelQuote)
			{
				m_nextBlockContinuesQuote = false;
			}
			if (m_nextBlockContinuesQuote)
			{
				foreach (var multiBlock in m_currentMultiBlockQuote)
				{
					multiBlock.MultiBlockQuote = MultiBlockQuote.None;
					multiBlock.CharacterId = CharacterVerseData.kUnexpectedCharacter;
					multiBlock.Delivery = null;
				}
			}
			else
			{
				// In case the last set of blocks were a multi-block quote
				ProcessMultiBlock();
			}
			ProcessPossibleRunOfPoetryBlocksAsScripture();
			SetImplicitCharacters();
			return m_outputBlocks;
		}

		private void ProcessPossibleRunOfPoetryBlocksAsScripture()
		{
			if (InRunOfPoetryBlocksThatAreProbablyScripture)
			{
				m_outputBlocks[m_outputBlockInWhichPoetryStarted].CharacterId = CharacterSpeakingMode.kScriptureCharacter;
				for (var iPoetry = m_outputBlockInWhichPoetryStarted + 1; iPoetry < m_outputBlocks.Count; iPoetry++)
				{
					// Perhaps slightly inefficient to do this inside the loop, but there will seldom be more than
					// a few paragraphs, and it's probably just as inefficient to set a flag or repeat the check.
					m_outputBlocks[m_outputBlockInWhichPoetryStarted].MultiBlockQuote = MultiBlockQuote.Start;
					m_outputBlocks[iPoetry].CharacterId = CharacterSpeakingMode.kScriptureCharacter;
					m_outputBlocks[iPoetry].MultiBlockQuote = MultiBlockQuote.Continuation;
				}
			}
			m_outputBlockInWhichPoetryStarted = -1;
		}

		private bool InRunOfPoetryBlocksThatAreProbablyScripture
		{
			get
			{
				return m_outputBlockInWhichPoetryStarted >= 0 && m_outputBlockInWhichPoetryStarted < m_outputBlocks.Count &&
					m_outputBlocks.Skip(m_outputBlockInWhichPoetryStarted).All(b =>
					{
						if (!b.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
							return false;

						var characters = m_cvInfo.GetCharacters(m_bookNum, b.ChapterNumber, b.AllVerses, m_versification, true);
						if (!characters.Any(c => c.Character == CharacterSpeakingMode.kScriptureCharacter))
							return false;

						return characters.Count == 1 || (characters.Count == 2 &&
							characters.Any(c => CharacterVerseData.IsCharacterOfType(c.Character, CharacterVerseData.StandardCharacter.Narrator)));
					});
			}
		}

		/// <summary>
		/// After doing all the "normal" parsing logic, this methods takes a final pass to see if there are any
		/// narrator blocks (i.e., no explicit quote markup was detected) for verses that are known to be spoken
		/// by a particular character.
		/// </summary>
		/// <remarks>
		/// If the logic here seems unnecessarily complicated, it's because it has special handling to deal with
		/// blocks that contain leading and/or trailing verses or partial verses that are not part of the passage
		/// known implicitly to be spoken entirely by the specified character. In that case:
		/// * If the leading or trailing verse is already split such that part of the verse is included in the
		/// paragraph(s) with the implicit verses, we assume that the paragraph break(s) correspond to start/end
		/// of the quote, and mark the paragraph(s) containing the implicit verse and partial verse(s) as being
		/// spoken by the expected character.
		/// * Otherwise, regardless of the number of leading or trailing verses in the paragraph, we split the
		/// block to isolate the verses known to contain the implicit speech, so that block can be marked as
		/// spoken by the known character. If the control file indicates that the same character also speaks in
		/// the preceding or following verse, we mark those split-off blocks as ambiguous.
		/// Note: The control file needs to take a fairly conservative approach to marking a verse as
		/// Implicit. Verses that are not part of an ongoing discourse and may reasonably be re-worded to turn
		/// direct speech into indirect speech should not be marked as implicit.
		/// </remarks>
		private void SetImplicitCharacters()
		{
			var prevBlockWasOriginallyNarratorCharacter = false;
			var comparer = new CharacterDeliveryEqualityComparer();
			for (int i = 0; i < m_outputBlocks.Count; i++)
			{
				var block = m_outputBlocks[i];
				if (block.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
				{
					var initialImplicitCv = m_cvInfo.GetImplicitCharacter(m_bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber,
						 m_versification);
					var subsequentImplicitCv = initialImplicitCv; // These can be (and very often will be) null

					// See if there is a verse/bridge in the block where we have a change in implicit character (which
					// could be going from one implicit character to another or, more likely, going from having one to
					// not having one, or vice versa).
					int iElem;
					for (iElem = 1; iElem < block.BlockElements.Count; iElem++)
					{
						if (block.BlockElements[iElem] is Verse verse)
						{
							subsequentImplicitCv = m_cvInfo.GetImplicitCharacter(m_bookNum, block.ChapterNumber, verse.StartVerse,
								verse.EndVerse, m_versification);
							if (!comparer.Equals(initialImplicitCv, subsequentImplicitCv))
								break;
						}
					}
					if (!comparer.Equals(initialImplicitCv, subsequentImplicitCv))
					{
						// Split this block
						var newBlock = new Block(block.StyleTag, block.ChapterNumber,
							block.InitialStartVerseNumber, block.InitialEndVerseNumber)
						{
							BlockElements = block.BlockElements.Take(iElem).ToList(),
							IsParagraphStart = block.IsParagraphStart
						};
						block.IsParagraphStart = false;
						m_outputBlocks.Insert(i, newBlock);
						// Decide how to assign the newly split-off (preceding) block. There are four possibilities.
						ICharacterDeliveryInfo leadInCharacter;
						if (subsequentImplicitCv != null &&
							(leadInCharacter = GetMatchingCharacter(m_cvInfo.GetCharacters(m_bookNum, newBlock.ChapterNumber, new SingleVerse(newBlock.LastVerseNum),
								versification: m_versification), subsequentImplicitCv)) != null)
						{
							if (newBlock.ContainsVerseNumber)
							{
								// 1) Whole verse(s). Not a likely lead-in, though still possible. User should have a look.
								newBlock.CharacterId = CharacterVerseData.kNeedsReview;
							}
							else
							{
								// 2) Most interesting case: this is a partial-verse block, that could
								// reasonably be the same character. (Long discourses often begin with a verse
								// that starts off with an intro statement by the narrator.)
								newBlock.SetCharacterIdAndCharacterIdInScript(leadInCharacter.Character, m_bookNum, m_versification);
								newBlock.Delivery = leadInCharacter.Delivery;
							}
						}
						else
						{
							if (initialImplicitCv != null)
							{
								// 3) Two different back-to-back implicit blocks. Not common outside of prophecy/poetry.
								newBlock.SetCharacterIdAndCharacterIdInScript(initialImplicitCv.Character, m_bookNum, m_versification);
								newBlock.Delivery = initialImplicitCv.Delivery;
							}
							else // 4) Just leave it as narrator.
								newBlock.CharacterId = block.CharacterId; // i.e., Narrator
						}
						var startVerse = (Verse)block.BlockElements[iElem];
						block.InitialStartVerseNumber = startVerse.StartVerse;
						block.InitialEndVerseNumber = startVerse.EndVerse;
						block.BlockElements = block.BlockElements.Skip(iElem).ToList();
						// Since we inserted a block into the list, the remainder of the original block is now at 1 + 1, so
						// it will get processed next time around.
					}
					else if (initialImplicitCv != null)
					{
						// Deal with the "tail" (anything in the block following the verses known to be spoken
						// by the implicit character).

						// In case you're wondering about the final check in these two expressions, in the C-V
						// control file, if we have a self-quote (i.e., same character) in a verse that is implicitly
						// expected to be spoken 100% by a particular character, then we ignore the fact that the
						// adjacent block got assigned as an explicit quote. This keeps us from unnecessarily marking
						// a block for review just because there were no first-level quotes around it (in which case
						// the first-level quotes will be around the embedded self-quote). See ISA 51:16, for an example.
						var prevBlockForThisSameVerseWasSetToCorrectCharacterDueToExplicitlyMarkedQuotes =
							!prevBlockWasOriginallyNarratorCharacter && i > 0 &&
							m_outputBlocks[i - 1].LastVerse.StartVerse == block.InitialStartVerseNumber &&
							m_outputBlocks[i - 1].CharacterId == initialImplicitCv.Character &&
							!m_cvInfo.GetCharacters(m_bookNum, block.ChapterNumber, (Block.InitialVerseNumberBridgeFromBlock)block, m_versification)
								.Any(cv => cv.QuoteType == QuoteType.Quotation);
						var nextBlockForThisSameVerseIsSetToCorrectCharacterDueToExplicitlyMarkedQuotes =
							i + 1 < m_outputBlocks.Count &&
							m_outputBlocks[i + 1].InitialStartVerseNumber == block.LastVerse.StartVerse &&
							m_outputBlocks[i + 1].CharacterId == initialImplicitCv.Character &&
							!m_cvInfo.GetCharacters(m_bookNum, block.ChapterNumber, (Block.InitialVerseNumberBridgeFromBlock)m_outputBlocks[i + 1], m_versification)
								.Any(cv => cv.QuoteType == QuoteType.Quotation);

						bool needsReview;
						if (block.BlockElements.OfType<Verse>().Skip(1).Any())
						{
							// This is similar to the "he said" check below but deals with the more extreme/weird case where this
							// block has multiple intervening verses that were all supposed to be assigned implicitly. If *both*
							// the preceding block and following block cover the start/end verses for this block, something fishy
							// is probably going on (e.g., maybe something we expected to be rendered as direct speech is being
							// rendered indirectly).
							needsReview = prevBlockForThisSameVerseWasSetToCorrectCharacterDueToExplicitlyMarkedQuotes &&
								nextBlockForThisSameVerseIsSetToCorrectCharacterDueToExplicitlyMarkedQuotes;
						}
						else
						{
							// Check for a possible unexpected "He said" block and mark it as Needs Review.
							needsReview = prevBlockForThisSameVerseWasSetToCorrectCharacterDueToExplicitlyMarkedQuotes ||
								nextBlockForThisSameVerseIsSetToCorrectCharacterDueToExplicitlyMarkedQuotes;
						}

						if (needsReview)
						{
							block.CharacterId = CharacterVerseData.kNeedsReview;
						}
						else
						{
							block.SetNonDramaticCharacterId(initialImplicitCv.Character);
							block.UseDefaultForMultipleChoiceCharacter(() => initialImplicitCv);
							block.Delivery = initialImplicitCv.Delivery;
						}
					}

					prevBlockWasOriginallyNarratorCharacter = true;
				}
				else
				{
					prevBlockWasOriginallyNarratorCharacter = false;
				}
			}
		}

		private ICharacterDeliveryInfo GetMatchingCharacter(IEnumerable<CharacterSpeakingMode> possibilities, ICharacterDeliveryInfo cvToMatch)
		{
			ICharacterDeliveryInfo leadInCharacter = null;
			// There will almost always be only one entry with a matching character name. But if there are two, we
			// want the one whose delivery matches, if any.
			foreach (var character in possibilities.Where(cv => cv.Character == cvToMatch.Character))
			{
				if (leadInCharacter == null)
					leadInCharacter = character;
				else if (leadInCharacter.Delivery == cvToMatch.Delivery)
					break;
				else if (character.Delivery == cvToMatch.Delivery)
				{
					return character;
				}
				else
				{
					// Technically, there could be more than two, so there might yet be one whose
					// delivery matches, but that's extremely unlikely and not worth further complexity.
					// What we really want to do is return an object representing the correct character
					// but an ambiguous delivery, but we have no such notion.
					return CharacterVerseData.NeedsReviewCharacter.Singleton;
				}
			}

			return leadInCharacter;
		}

		private bool AtOpeningFirstLevelQuoteThatSeemsToBeMoreThanJustAnExpression(string content, int pos)
		{
			// Note: this method is used to try to guess whether an opening quote (that immediately follows
			// a "dialogue" colon) is just a word or short phrase or a whole quote that happens to have a
			// preceding colon. This is needed because languages that use a colon (without quotes) to introduce
			// a bit of dialogue often also use a colon to introduce quoted speech. But we don't want to confuse
			// a bit of dialogue that happens to begin with an exclamation or some other word or phrase that
			// merits quotation marks with a full quote.
			// See PG-487 for an idea that might ultimately do a better job of helping us figure this out.
			var matchFirstLevelOpen = m_regexStartsWithFirstLevelOpener.Match(content, pos);
			if (matchFirstLevelOpen.Success && matchFirstLevelOpen.Index == pos)
			{
				if (pos > 0)
					return true;
				int quotationStartPos = pos + matchFirstLevelOpen.Length;
				var match = m_regexHasFirstLevelClose.Match(content, quotationStartPos);
				if (!match.Success)
					return true;
				// The remaining logic here is pretty fuzzy. Basically, it feels like a quote that covers more than
				// half of the remaining text or which consists of 3 or more words is likely to be a full quote.
				// In any case, it gets some existing (somewhat contrived) tests to pass and also allows a test based
				// on more realistic data to pass.
				int lengthOfQuotation = match.Index - quotationStartPos;
				if (lengthOfQuotation > (content.Length - pos) / 2)
					return true;
				if (content.Skip(quotationStartPos).Take(lengthOfQuotation).Count(IsWhiteSpace) > 2)
					return true;
				return false;
			}
			return false;
		}

		private string GetOpeningPunctuationAsSingleString()
		{
			return string.Join("", CharacterUtils.GetAllCharactersInUnicodeCategory(UnicodeCategory.OpenPunctuation)) + GetOtherPunctuationTreatedAsOpeningPunctuationAsSingleString();
		}

		private string GetOtherPunctuationTreatedAsOpeningPunctuationAsSingleString()
		{
			return "¡¿";
		}

		private string GetSpecialOpeningPunctuation(string text)
		{
			if (text.Length == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var testIndex = 0;
			var testCharacter = text[testIndex];

			while (CharUnicodeInfo.GetUnicodeCategory(testCharacter) == UnicodeCategory.OpenPunctuation ||
				GetOtherPunctuationTreatedAsOpeningPunctuationAsSingleString().Contains(testCharacter))
			{
				sb.Append(testCharacter);
				testIndex++;

				if (testIndex >= text.Length)
					break;
				testCharacter = text[testIndex];
			}

			return sb.ToString();
		}

		private bool ProbablyAnApostrophe(string content, int pos)
		{
			if (CloserForCurrentLevel != "’" ||
				pos == 0 || pos >= content.Length - 1)
				return false;

			if (IsPunctuation(content[pos - 1]) || IsWhiteSpace(content[pos - 1]) ||
				IsPunctuation(content[pos + 1]))
				return false;

			if (IsLetter(content[pos + 1]))
				return true;

			if (!IsWhiteSpace(content[pos + 1]))
				return false;

			var regex = m_regexes[m_quoteLevel >= m_regexes.Count ? m_regexes.Count - 1 : m_quoteLevel];
			var match = regex.Match(content, pos + 1);
			return (match.Success &&
					(match.Value == CloserForCurrentLevel ||
					((m_quoteLevel < s_quoteSystem.NormalLevels.Count && match.Value == OpenerForNextLevel) &&
					(m_quoteLevel == 0 || !m_regexes[m_quoteLevel - 1].Match(content, pos + 1, match.Index - (pos + 1)).Success)) ||
					(m_quoteLevel > 0 && match.Value == s_quoteSystem.NormalLevels[m_quoteLevel - 1].Open)));
		}

		private void SetBlockInitialVerseFromVerseElement(Verse verseElement)
		{
			m_workingBlock.InitialStartVerseNumber = verseElement.StartVerse;
			m_workingBlock.InitialEndVerseNumber = verseElement.StartVerse != verseElement.EndVerse ? verseElement.EndVerse : 0;
		}

		private void IncrementQuoteLevel()
		{
			IncrementQuoteLevel(m_workingBlock);
		}

		private void IncrementQuoteLevel(Block currentBlock)
		{
			if (m_quoteLevel++ == 0)
				m_possibleCharactersForCurrentQuote = m_cvInfo.GetCharacters(m_bookNum, currentBlock.ChapterNumber,
					(Block.InitialVerseNumberBridgeFromBlock)currentBlock, m_versification, true).Select(cv => cv.Character).ToList();
		}

		private void DecrementQuoteLevel()
		{
			Debug.Assert(m_quoteLevel > 0);
			if (--m_quoteLevel == 0)
				m_possibleCharactersForCurrentQuote.Clear();
		}

		public string ContinuerForCurrentLevel { get { return s_quoteSystem.NormalLevels[m_quoteLevel - 1].Continue; } }
		public string ContinuerForNextLevel { get { return s_quoteSystem.NormalLevels[m_quoteLevel].Continue; } }
		public string CloserForCurrentLevel { get { return s_quoteSystem.NormalLevels[m_quoteLevel - 1].Close; } }
		public string OpenerForNextLevel { get { return s_quoteSystem.NormalLevels[m_quoteLevel].Open; } }

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
				if (text.Any(IsLetterOrDigit)) // If not, just keep anything (probably opening punctuation) in the builder to be included with the next bit of text.
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
					if (m_workingBlock.BlockElements.FirstOrDefault() is Verse && m_nonScriptTextBlockElements.Last() is Verse)
						throw new Exception($"Pending verse \"{m_nonScriptTextBlockElements.Last()}\" was left " +
							"from previous block, but following block starts with a verse: " +
							m_workingBlock.ToString(true, m_bookId));

					if (m_nonScriptTextBlockElements.First() is Verse verse)
						m_workingBlock.InitialStartVerseNumber = BCVRef.VerseToIntStart(verse.Number);

					m_workingBlock.BlockElements.InsertRange(0, m_nonScriptTextBlockElements);
					m_workingBlock.MultiBlockQuote = (lastBlock.MultiBlockQuote == MultiBlockQuote.Start)
						? MultiBlockQuote.Continuation : lastBlock.MultiBlockQuote;

					// If we removed all block elements, remove the block
					if (!lastBlock.BlockElements.Any())
					{
						m_workingBlock.IsParagraphStart = lastBlock.IsParagraphStart;
						m_outputBlocks.Remove(lastBlock);
						if (m_currentMultiBlockQuote.LastOrDefault() == lastBlock)
							m_currentMultiBlockQuote.RemoveAt(m_currentMultiBlockQuote.Count - 1);
					}
					//if (m_workingBlock.MultiBlockQuote != MultiBlockQuote.None)
					//	m_currentMultiBlockQuote.Add(m_workingBlock);
				}
			}
			m_nonScriptTextBlockElements.Clear();
		}

		/// <summary>
		/// Flush the current string builder to a block element,
		/// and flush the current block elements to a block
		/// </summary>
		private void FlushStringBuilderAndBlock(StringBuilder sb, string styleTag, bool nonNarrator, bool characterUnknown = false)
		{
			Debug.Assert(!m_ignoringNarratorQuotation,
				"This should only happen if the data is bad or the settings have an ending quote mark that is not properly paired with the starting quote mark.");
			// reset this flag just to be safe.
			m_ignoringNarratorQuotation = false;

			FlushStringBuilderToBlockElement(sb);
			if (m_workingBlock.BlockElements.Count > 0)
			{
				FlushBlock(styleTag, nonNarrator, characterUnknown);
			}
		}

		/// <summary>
		/// Add the working block to the new list and create a new working block
		/// </summary>
		private void FlushBlock(string styleTag, bool nonNarrator, bool characterUnknown = false)
		{
			if (!m_workingBlock.BlockElements.Any())
			{
				m_workingBlock.StyleTag = styleTag;
				return;
			}
			Block blockFollowingInterruption = null;
			if (characterUnknown)
			{
				m_workingBlock.CharacterId = CharacterVerseData.kUnexpectedCharacter;
				m_workingBlock.Delivery = null;
			}
			else
			{
				if (!m_workingBlock.BlockElements.OfType<ScriptText>().Any())
				{
					if (m_nextBlockContinuesQuote)
						m_workingBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
					m_nextBlockContinuesQuote = m_quoteLevel > 0;
					//m_workingBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Narrator);
				}
				else if (nonNarrator)
				{
					if (m_nextBlockContinuesQuote)
						m_workingBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
					m_nextBlockContinuesQuote = m_quoteLevel > 0;
					if (m_nextBlockContinuesQuote && m_workingBlock.MultiBlockQuote != MultiBlockQuote.Continuation)
						m_workingBlock.MultiBlockQuote = MultiBlockQuote.Start;

					var characterSpeakingDetails = m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.AllVerses, m_versification,
						// The quote parser generally ignores alternate characters, but if it is trying to resolve
						// a multi-block quote, we want to include them in case an alternate is the one being
						// continued from a previous block. (This can happen if the translators used explicit
						// quotes that disagree with FCBH's idea of who is speaking, especially in poetic or
						// prophetic material.)
						m_workingBlock.MultiBlockQuote == MultiBlockQuote.Continuation);
					if (characterSpeakingDetails.Any(cv => cv.QuoteType == QuoteType.Interruption))
					{
						blockFollowingInterruption = BreakOutInterruptionsFromWorkingBlock(m_bookId, characterSpeakingDetails);
					}
					if (m_workingBlock.MultiBlockQuote == MultiBlockQuote.Continuation)
					{
						var prevQuoteBlock = m_outputBlocks.Last();
						if (!prevQuoteBlock.CharacterIsUnclear)
						{
							// Generally, we should be able to pretty much assume that since this is a continuation
							// of the previous block's quote, we have the same character and delivery. But there's a
							// slight chance the delivery could change. And an even slighter chance we could have two
							// possible deliveries left after removing any other characters from this list. So we'll
							// be conservative and just prune the list down by character.
							// PG-1321: there's also the special case where we need to backtrack and consider an
							// alternate character who was passed over in the preceding blocks.
							if (characterSpeakingDetails.All(cv => cv.Character != prevQuoteBlock.CharacterId))
							{
								characterSpeakingDetails = new HashSet<CharacterSpeakingMode>(
									characterSpeakingDetails.Where(newCv =>
									m_currentMultiBlockQuote.All(earlierBlock => m_cvInfo.GetCharacters(m_bookNum,
										earlierBlock.ChapterNumber, earlierBlock.AllVerses, m_versification, true).Any(cv =>
										cv.Character == newCv.Character))));
								Debug.Assert(characterSpeakingDetails.Any(),
									$"(See PG-1321) TODO: Try to write a unit test. We are in the middle of a quote in {m_bookId} {m_workingBlock.ChapterNumber}:{m_workingBlock.InitialStartVerseNumber} " +
									"and we have no speakers left who were possible when this quote opened. Unless we're missing some useful entries in the CharacterVerse " +
									$"control file, the logic for {nameof(m_possibleCharactersForCurrentQuote)} should have kept us from running off the rails like this.");
								foreach (var earlierBlock in m_currentMultiBlockQuote)
									earlierBlock.SetCharacterAndDelivery(s_quoteSystem, characterSpeakingDetails);
							}
							else
							{
								characterSpeakingDetails.RemoveAll(cv => cv.Character != prevQuoteBlock.CharacterId);
							}
						}
					}
					m_workingBlock.SetCharacterAndDelivery(s_quoteSystem, characterSpeakingDetails);
				}
				else
				{
					m_nextBlockContinuesQuote = false;
					m_workingBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Narrator);
				}
			}

			var prevBlock = m_outputBlocks.LastOrDefault();
			if (prevBlock != null &&
				// Prevent combining blocks spoken by different characters (e.g., Parse_QuoteInNewParagraphWithinVerseBridge_NarratorAndOther)
				m_workingBlock.CharacterId == prevBlock.CharacterId &&
				// Prevent combining a new multi-block quote with a previous block (e.g., Parse_TwoAdjacentQuotesBySameCharacter_NotCombined) - this is unlikely
				m_workingBlock.MultiBlockQuote != MultiBlockQuote.Start &&
				// Prevent combining blocks within a multi-block quote (e.g., Parse_MultiBlockQuote_BlocksDoNotGetCombined)
				(prevBlock.MultiBlockQuote == MultiBlockQuote.None ||
				m_workingBlock.MultiBlockQuote != MultiBlockQuote.None) &&
				// Prevent combining verses (e.g., Parse_PoetryLinesInDifferentVersesWithNoInterveningSentenceEndingPunctuation_VersesAreNotCombined)
				!m_workingBlock.BlockElements.OfType<Verse>().Any() &&
				// Prevent combining sentences
				!prevBlock.BlockElements.OfType<ScriptText>().Last().Content.EndsWithSentenceEndingPunctuation() &&
				// Only combine following poetry paragraphs, etc. (i.e., prevent joining two "normal" paragraphs).
				m_workingBlock.IsFollowOnParagraphStyle &&
				// PG-1121: Since indentation (without quotes) is often used to indicate a Scripture quotation, prevent combining following
				// poetry paragraphs with the preceding "normal" paragraph if a Scripture quote is expected in this verse.
				(prevBlock.IsFollowOnParagraphStyle ||
				!m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.AllVerses, m_versification).Any(cv => cv.IsScriptureQuotation)) &&
				// If we seem to be in a run of poetry blocks that can get set to be an implicit Scripture quote,
				// we don't want to combine a trailing continuation paragraph.
				(!m_workingBlock.IsContinuationParagraphStyle || !InRunOfPoetryBlocksThatAreProbablyScripture))
			{
				prevBlock.CombineWith(m_workingBlock);
			}
			else
			{
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
			}

			if (blockFollowingInterruption != null)
			{
				m_outputBlocks.Add(blockFollowingInterruption);
				m_currentMultiBlockQuote.Add(blockFollowingInterruption);
			}

			var lastVerse = m_workingBlock.BlockElements.OfType<Verse>().LastOrDefault();
			int verseStartNum = m_workingBlock.InitialStartVerseNumber;
			int verseEndNum = m_workingBlock.InitialEndVerseNumber;
			if (lastVerse != null)
			{
				verseStartNum = lastVerse.StartVerse;
				verseEndNum = lastVerse.EndVerse;
			}
			m_workingBlock = new Block(styleTag, m_workingBlock.ChapterNumber, verseStartNum, verseEndNum);
		}

		/// <summary>
		/// This deals with parenthetical interruptions in a quote, like (let the reader understand).
		/// Coming out of this method, m_workingBlock will always be the last interruption found.
		/// </summary>
		/// <param name="bookId"></param>
		/// <param name="characterSpeakingDetails"></param>
		/// <returns>Any portion of the block following the (last) interruption we detect</returns>
		private Block BreakOutInterruptionsFromWorkingBlock(string bookId, IReadOnlyCollection<CharacterSpeakingMode> characterSpeakingDetails)
		{
			var nextInterruption = m_workingBlock.GetNextInterruption(s_quoteSystem);
			if (nextInterruption == null)
				return null;

			Block blockFollowingLastInterruption = null;

			var blocks = new PortionScript(bookId, new[] {m_workingBlock}, m_versification);
			Block originalQuoteBlock = blocks.GetScriptBlocks().Last();
			if (originalQuoteBlock.MultiBlockQuote != MultiBlockQuote.Continuation)
				ProcessMultiBlock();
			m_currentMultiBlockQuote.Add(originalQuoteBlock);

			while (true)
			{
				m_workingBlock = blocks.SplitBlock(blocks.GetScriptBlocks().Last(), nextInterruption.Item2, nextInterruption.Item1.Index, false);
				if (originalQuoteBlock.CharacterId == null)
					originalQuoteBlock.SetCharacterAndDelivery(s_quoteSystem, characterSpeakingDetails);
				var startCharIndex = nextInterruption.Item1.Length;
				if (blocks.GetScriptBlocks().Last().GetText(true).Substring(nextInterruption.Item1.Length).Any(IsLetter))
				{
					blockFollowingLastInterruption = blocks.SplitBlock(blocks.GetScriptBlocks().Last(), nextInterruption.Item2, nextInterruption.Item1.Length, false);
					startCharIndex = 1;
					nextInterruption = blocks.GetScriptBlocks().Last().GetNextInterruption(s_quoteSystem, startCharIndex);
					blockFollowingLastInterruption.MultiBlockQuote = m_nextBlockContinuesQuote && nextInterruption == null ? MultiBlockQuote.Start : MultiBlockQuote.None;
					blockFollowingLastInterruption.SetCharacterInfo(originalQuoteBlock);
					blockFollowingLastInterruption.Delivery = originalQuoteBlock.Delivery;
				}
				else
				{
					blockFollowingLastInterruption = null;
					nextInterruption = blocks.GetScriptBlocks().Last().GetNextInterruption(s_quoteSystem, startCharIndex);
				}
				if (nextInterruption == null)
					break;
				m_workingBlock.SetCharacterAndDelivery(s_quoteSystem, characterSpeakingDetails);
				m_workingBlock = blocks.GetScriptBlocks().Last();
			}

			foreach (var b in blocks.GetScriptBlocks().TakeWhile(b => b != m_workingBlock))
			{
				m_outputBlocks.Add(b);
			}

			if (blockFollowingLastInterruption == null)
				m_nextBlockContinuesQuote = false;

			return blockFollowingLastInterruption;
		}

		private void ProcessMultiBlock()
		{
			if (!m_currentMultiBlockQuote.Any())
				return;

			if (m_currentMultiBlockQuote.Count == 1)
			{
				m_currentMultiBlockQuote[0].MultiBlockQuote = MultiBlockQuote.None;
				m_currentMultiBlockQuote.Clear();
				return;
			}

			BookScript.ProcessAssignmentForMultiBlockQuote(m_bookNum, m_currentMultiBlockQuote, m_versification);

			m_currentMultiBlockQuote.Clear();
		}

		private bool IsNormalParagraphStyle(string styleTag)
		{
			return styleTag == "p";
		}

		//public static ConcurrentDictionary<BookScript, IReadOnlyList<Block>> Unparse(IEnumerable<BookScript> books)
		//{
		//	var blocksInBook = new ConcurrentDictionary<BookScript, IReadOnlyList<Block>>();

		//	Parallel.ForEach(books, book =>
		//	{
		//		var oldBlocks = book.GetScriptBlocks();
		//		var newBlocks = new List<Block>();
		//		Block currentBlock = null;

		//		foreach (var oldBlock in oldBlocks)
		//		{
		//			// is this a new chapter?
		//			if (oldBlock.IsParagraphStart || (currentBlock == null))
		//			{
		//				if (currentBlock != null) newBlocks.Add(currentBlock);

		//				if (CharacterVerseData.IsCharacterExtraBiblical(oldBlock.CharacterId) && !oldBlock.UserConfirmed)
		//				{
		//					newBlocks.Add(oldBlock.Clone());
		//					currentBlock = null;
		//					continue;
		//				}
		//				else
		//				{
		//					currentBlock = new Block(oldBlock.StyleTag, oldBlock.ChapterNumber, oldBlock.InitialStartVerseNumber,
		//						oldBlock.InitialEndVerseNumber);
		//					currentBlock.IsParagraphStart = oldBlock.IsParagraphStart;
		//				}
		//			}

		//			foreach (var element in oldBlock.BlockElements)
		//			{
		//				if (element is Verse)
		//				{
		//					currentBlock.BlockElements.Add(element.Clone());
		//					continue;
		//				}

		//				// element is Glyssen.ScriptText
		//				// check if this text should be appended to the previous element
		//				var lastElement = currentBlock.BlockElements.LastOrDefault() as ScriptText;
		//				if (lastElement != null)
		//				{
		//					lastElement.Content += ((ScriptText) element).Content;
		//				}
		//				else
		//				{
		//					currentBlock.BlockElements.Add(element.Clone());
		//				}
		//			}
		//		}

		//		// add the last block now
		//		if (currentBlock != null)
		//			newBlocks.Add(currentBlock);

		//		blocksInBook.AddOrUpdate(book, newBlocks, (script, list) => newBlocks);
		//	});

		//	return blocksInBook;
		//}

		#region CharacterDelivery utility class
		public class CharacterDelivery
		{
			public string Character { get; private set; }
			public string Delivery { get; private set; }

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

			public static IEqualityComparer<CharacterDelivery> CharacterDeliveryComparer { get; } = new CharacterDeliveryEqualityComparer();
		}
		#endregion

		private class QuoteCharComparer : IComparer<char>
		{
			public int Compare(char x, char y)
			{
				// Putting regular dash at the beginning makes the regex not try to treat it as a range operator
				if (x.Equals('-') && !y.Equals('-'))
					return -1;
				return x.CompareTo(y);
			}
		}
	}
}
