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
using Glyssen.Character;
using Glyssen.Shared;
using Glyssen.Utilities;
using SIL.Extensions;
using SIL.Scripture;
using SIL.Unicode;
using static System.Char;

namespace Glyssen.Quote
{
	public class QuoteParser
	{
		public static void ParseProject(Project project, BackgroundWorker projectWorker)
		{
			var cvInfo = new ParserCharacterRepository(new CombinedCharacterVerseData(project), project.ReferenceText);

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
			Block.InitializeInterruptionRegEx(quoteSystem.QuotationDashMarker != null && quoteSystem.QuotationDashMarker.Any(c => c == '\u2014' || c == '\u2015'));
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
			var sbQuoteMatcher = new StringBuilder();

			foreach (var qm in splitters.Where(s => !string.IsNullOrEmpty(s)).Distinct())
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
					throw new InvalidOperationException("Should not be parsing blocks that already have user-decisions applied.");

				bool thisBlockStartsWithAContinuer = false;

				if (block.CharacterIsStandard && !block.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
				{
					m_nextBlockContinuesQuote = false;

					m_outputBlocks.Add(block);
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

				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber) { IsParagraphStart = block.IsParagraphStart };

				bool atBeginningOfBlock = true;
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						// Add the element to our working list in case we need to move it to the next block (see MoveTrailingElementsIfNecessary)
						m_nonScriptTextBlockElements.Add(element);

						var verseElement = element as Verse;
						if (verseElement != null)
						{
							if (!m_workingBlock.BlockElements.Any())
								SetBlockInitialVerseFromVerseElement(verseElement);

							if (m_possibleCharactersForCurrentQuote.Any())
							{
								m_possibleCharactersForCurrentQuote = m_possibleCharactersForCurrentQuote.Intersect(
									m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, verseElement.StartVerse, verseElement.EndVerse, versification: m_versification, includeAlternates:true).Select(cv => cv.Character)).ToList();

								if (!m_possibleCharactersForCurrentQuote.Any())
								{
									foreach (var multiBlock in m_currentMultiBlockQuote)
									{
										multiBlock.MultiBlockQuote = MultiBlockQuote.None;
										multiBlock.CharacterId = CharacterVerseData.kUnexpectedCharacter;
										multiBlock.Delivery = null;
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
									var characters = m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.LastVerse.StartVerse, m_workingBlock.LastVerse.EndVerse, versification: m_versification).ToList();
									// PG-814: If the only character for this verse is a narrator "Quotation", then do not treat it as speech.
									if (characters.Count == 1 && characters[0].QuoteType == QuoteType.Quotation &&
										CharacterVerseData.IsCharacterOfType(characters[0].Character, CharacterVerseData.StandardCharacter.Narrator))
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
			SetImplicitCharacters();
			return m_outputBlocks;
		}

		private void SetImplicitCharacters()
		{
			var comparer = new CharacterDeliveryEqualityComparer();
			for (int i = 0; i < m_outputBlocks.Count; i++)
			{
				var block = m_outputBlocks[i];
				if (block.CharacterIs(m_bookId, CharacterVerseData.StandardCharacter.Narrator))
				{
					var initialImplicitCv = m_cvInfo.GetImplicitCharacter(m_bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber,
						 m_versification);
					var subsequentImplicitCv = initialImplicitCv;

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
						var newBlock = new Block(block.StyleTag, block.ChapterNumber,
							block.InitialStartVerseNumber, block.InitialEndVerseNumber)
						{
							BlockElements = block.BlockElements.Take(iElem).ToList(),
						};
						m_outputBlocks.Insert(i, newBlock);
						CharacterVerse leadInCharacter;
						if (subsequentImplicitCv != null && (leadInCharacter = m_cvInfo.GetCharacters(m_bookNum, block.ChapterNumber, newBlock.LastVerseNum, versification: m_versification)
							.SingleOrDefault(cv => cv.Character == subsequentImplicitCv.Character)) != null)
						{
							if (newBlock.StartsAtVerseStart)
								newBlock.CharacterId = CharacterVerseData.kNeedsReview;
							else
							{
								newBlock.SetCharacterIdAndCharacterIdInScript(leadInCharacter.Character, m_bookNum, m_versification);
								newBlock.Delivery = leadInCharacter.Delivery;
							}
						}
						else
						{
							if (initialImplicitCv != null)
							{
								newBlock.SetCharacterIdAndCharacterIdInScript(initialImplicitCv.Character, m_bookNum, m_versification);
								newBlock.Delivery = initialImplicitCv.Delivery;
							}
							else
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
						block.SetNonDramaticCharacterId(initialImplicitCv.Character);
						block.UseDefaultForMultipleChoiceCharacter(() => initialImplicitCv);
						block.Delivery = initialImplicitCv.Delivery;
					}
				}
			}
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
			if (m_quoteLevel++ == 0)
				m_possibleCharactersForCurrentQuote = m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber, m_workingBlock.InitialEndVerseNumber, versification: m_versification).Select(cv => cv.Character).ToList();
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
					var verse = m_nonScriptTextBlockElements.First() as Verse;
					if (verse != null)
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
		/// <param name="sb"></param>
		/// <param name="styleTag"></param>
		/// <param name="nonNarrator"></param>
		/// <param name="characterUnknown"></param>
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
		/// <param name="styleTag"></param>
		/// <param name="nonNarrator"></param>
		/// <param name="characterUnknown"></param>
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

					var characterVerseDetails = m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber,
						m_workingBlock.InitialEndVerseNumber, m_workingBlock.LastVerseNum, m_versification,
						m_workingBlock.MultiBlockQuote == MultiBlockQuote.Continuation).ToList();
					if (characterVerseDetails.Any(cv => cv.QuoteType == QuoteType.Interruption))
					{
						blockFollowingInterruption = BreakOutInterruptionsFromWorkingBlock(m_bookId, characterVerseDetails);
					}
					if (m_workingBlock.MultiBlockQuote == MultiBlockQuote.Continuation &&
						characterVerseDetails.Any(cv => m_outputBlocks.Last().CharacterId == cv.Character))
					{
						// Generally, we should be able to pretty much assume that since this is a continuation
						// of the previous block's quote, we have the same character and delivery. But there's a
						// slight chance the delivery could change. And an even slighter chance we could have two
						// possible deliveries left after removing any other characters from this list. So we'll
						// be conservative and just prune the list down by character.
						characterVerseDetails.RemoveAll(cv => cv.Character != m_outputBlocks.Last().CharacterId);
					}
#if DEBUG
					else
					{
						Debug.Fail("We are in the middle of a quote and we're about to change speakers. The logic " +
							"for m_possibleCharactersForCurrentQuote should have made this impossible.");
					}
#endif
					m_workingBlock.SetCharacterAndDelivery(characterVerseDetails);
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
				// Only combine following poetry pragraphs, etc. (i.e., prevent joining two "normal" paragraphs).
				m_workingBlock.IsFollowOnParagraphStyle &&
				// PG-1121: Since indentation (without quotes) is often used to indicate a Scripture quotation, prevent combining following
				// poetry paragraphs with the preceding "normal" paragraph if a Scripture quote is expected in this verse.
				(prevBlock.IsFollowOnParagraphStyle ||
				!m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber,
				m_workingBlock.InitialEndVerseNumber, m_workingBlock.LastVerseNum, m_versification).Any(cv => cv.IsScriptureQuotation)))
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
		/// <param name="characterVerseDetails"></param>
		/// <returns>Any portion of the block following the (last) interruption we detect</returns>
		private Block BreakOutInterruptionsFromWorkingBlock(string bookId, List<CharacterVerse> characterVerseDetails)
		{
			var nextInterruption = m_workingBlock.GetNextInterruption();
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
					originalQuoteBlock.SetCharacterAndDelivery(characterVerseDetails);
				var startCharIndex = nextInterruption.Item1.Length;
				if (blocks.GetScriptBlocks().Last().GetText(true).Substring(nextInterruption.Item1.Length).Any(IsLetter))
				{
					blockFollowingLastInterruption = blocks.SplitBlock(blocks.GetScriptBlocks().Last(), nextInterruption.Item2, nextInterruption.Item1.Length, false);
					startCharIndex = 1;
					nextInterruption = blocks.GetScriptBlocks().Last().GetNextInterruption(startCharIndex);
					blockFollowingLastInterruption.MultiBlockQuote = m_nextBlockContinuesQuote && nextInterruption == null ? MultiBlockQuote.Start : MultiBlockQuote.None;
					blockFollowingLastInterruption.SetCharacterInfo(originalQuoteBlock);
					blockFollowingLastInterruption.Delivery = originalQuoteBlock.Delivery;
				}
				else
				{
					blockFollowingLastInterruption = null;
					nextInterruption = blocks.GetScriptBlocks().Last().GetNextInterruption(startCharIndex);
				}
				if (nextInterruption == null)
					break;
				m_workingBlock.SetCharacterAndDelivery(characterVerseDetails);
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

			private static readonly IEqualityComparer<CharacterDelivery> s_characterDeliveryComparerInstance = new CharacterDeliveryEqualityComparer();

			public static IEqualityComparer<CharacterDelivery> CharacterDeliveryComparer
			{
				get { return s_characterDeliveryComparerInstance; }
			}
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
