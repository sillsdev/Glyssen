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
using SIL.Extensions;
using SIL.Scripture;
using SIL.Unicode;
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

		public static List<BookScript> TestQuoteSystem(Project project, QuoteSystem altQuoteSystem)
		{
			var cvInfo = new CombinedCharacterVerseData(project);

			var unparsedBlocks = Unparse(project.Books);

			var blocksInBook = unparsedBlocks.ToDictionary(bookidBlocksPair => bookidBlocksPair.Key.BookId, bookidBlocksPair => bookidBlocksPair.Value);

			var parsedBlocksByBook = new ConcurrentDictionary<string, BookScript>();
			Parallel.ForEach(blocksInBook, bookidBlocksPair =>
			{
				var bookId = bookidBlocksPair.Key;
				var blocks =
					new QuoteParser(cvInfo, bookId, bookidBlocksPair.Value, altQuoteSystem, project.Versification).Parse().ToList();
				var parsedBook = new BookScript(bookId, blocks);
				parsedBlocksByBook.AddOrUpdate(bookId, parsedBook, (s, script) => parsedBook);
			});

			// sort the list
			var bookScripts = parsedBlocksByBook.Values.ToList();
			bookScripts.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));
			return bookScripts;
		}

		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteSystem m_quoteSystem;
		private readonly ScrVers m_versification;
		private readonly List<Regex> m_regexes = new List<Regex>();
		private readonly Regex m_regexStartsWithSpecialOpeningPunctuation = new Regex(@"^(\(|\[|\{)", RegexOptions.Compiled);
		private Regex m_regexStartsWithFirstLevelOpener;

		#region working (state) members
		// These members are used by several methods. Making them class-level avoids having to pass them repeatedly.
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		private int m_quoteLevel;
		private bool m_nextBlockContinuesQuote;
		private readonly List<Block> m_currentMultiBlockQuote = new List<Block>();
		private List<string> m_possibleCharactersForCurrentQuote = new List<string>();
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
			IList<string> splitters;
			var quoteChars = new SortedSet<char>(new QuoteCharComparer());
			var regexExpressions = new List<string>(m_quoteSystem.NormalLevels.Count);

			if (m_quoteSystem.NormalLevels.Count > 0)
			{
				m_regexStartsWithFirstLevelOpener = new Regex(Regex.Escape(m_quoteSystem.NormalLevels[0].Open), RegexOptions.Compiled);

				// At level x, we need continuer x, closer x, opener x+1.  Continuer must be first.
				for (int level = 0; level < m_quoteSystem.NormalLevels.Count; level++)
				{
					splitters = new List<string>();
					if (level > 0)
					{
						var quoteSystemLevelMinusOne = m_quoteSystem.NormalLevels[level - 1];
						if (!string.IsNullOrWhiteSpace(quoteSystemLevelMinusOne.Continue))
							splitters.Add(quoteSystemLevelMinusOne.Continue);
						splitters.Add(quoteSystemLevelMinusOne.Close);
					}
					else if (level == 0 && m_quoteSystem.NormalLevels[0].Continue != m_quoteSystem.NormalLevels[0].Open)
						splitters.Add(m_quoteSystem.NormalLevels[0].Continue);
					splitters.Add(m_quoteSystem.NormalLevels[level].Open);
					if (level == 0)
						AddQuotationDashes(splitters);

					regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
				}

				// Final regex handles when we are inside the innermost level
				splitters = new List<string>();
				splitters.Add(m_quoteSystem.NormalLevels.Last().Close);
				splitters.Add(m_quoteSystem.NormalLevels.Last().Continue);
				if (m_quoteSystem.NormalLevels.Count == 1)
					AddQuotationDashes(splitters);

				regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
			}
			else
			{
				splitters = new List<string>();
				AddQuotationDashes(splitters);
				regexExpressions.Add(BuildQuoteMatcherRegex(splitters));
			}

			// Get all unique characters which make up all quote marks
			StringBuilder sbAllCharacters = new StringBuilder();
			foreach (var quoteSystemLevel in m_quoteSystem.AllLevels)
			{
				sbAllCharacters.Append(quoteSystemLevel.Open);
				sbAllCharacters.Append(quoteSystemLevel.Close);
				if (!String.IsNullOrWhiteSpace(quoteSystemLevel.Continue))
					sbAllCharacters.Append(quoteSystemLevel.Continue);
			}
			quoteChars.AddRange(sbAllCharacters.ToString().Where(c => !char.IsWhiteSpace(c)));

			foreach (var expr in regexExpressions)
			{
				m_regexes.Add(new Regex(String.Format(expr,
					Regex.Escape(string.Join(string.Empty, quoteChars)),
					Regex.Escape(@"(\[\{")), RegexOptions.Compiled));
			}
		}

		private void AddQuotationDashes(IList<string> splitters)
		{
			if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashMarker))
			{
				splitters.Add(m_quoteSystem.QuotationDashMarker);
				if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker))
				{
					splitters.Add(m_quoteSystem.QuotationDashEndMarker);
				}
			}
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
			return String.Format(@"((?:(?:{0})(?:[^\w{1}])*))", quoteMatcher, "{0}{1}");
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
					!IsFollowOnParagraphStyle(block.StyleTag)))
				{
					DecrementQuoteLevel();
					blockInWhichDialogueQuoteStarted = null;
					m_nextBlockContinuesQuote = potentialDialogueContinuer = !string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker) ||
						(m_quoteSystem.NormalLevels.Count > 0 && m_quoteSystem.NormalLevels[0].Continue != m_quoteSystem.NormalLevels[0].Open);
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

						var verseElement = element as Verse;
						if (verseElement != null)
						{
							if (!m_workingBlock.BlockElements.Any())
								SetBlockInitialVerseFromVerseElement(verseElement);

							if (m_possibleCharactersForCurrentQuote.Any())
							{
								m_possibleCharactersForCurrentQuote = m_possibleCharactersForCurrentQuote.Intersect(
									m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, verseElement.StartVerse, verseElement.EndVerse, versification: m_versification).Select(cv => cv.Character)).ToList();

								if (!m_possibleCharactersForCurrentQuote.Any())
								{
									foreach (var multiBlock in m_currentMultiBlockQuote)
									{
										multiBlock.MultiBlockQuote = MultiBlockQuote.None;
										multiBlock.CharacterId = CharacterVerseData.UnknownCharacter;
										multiBlock.Delivery = null;
									}
									m_currentMultiBlockQuote.Clear();
									FlushStringBuilderAndBlock(sb, block.StyleTag, m_quoteLevel > 0, true);
									SetBlockInitialVerseFromVerseElement(verseElement);
									m_quoteLevel = 0;
									m_nextBlockContinuesQuote = false;
								}
							}
						}

						m_workingBlock.BlockElements.Add(element);
						continue;
					}
					sb.Clear();

					var content = scriptText.Content;
					int pos = 0;
					while (pos < content.Length)
					{
						if (pendingColon)
						{
							if (pos > 0 && m_regexStartsWithFirstLevelOpener.Match(content).Index == pos)
								DecrementQuoteLevel();
							else
								blockInWhichDialogueQuoteStarted = block;
							pendingColon = false;
						}

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
								if ((m_quoteLevel == 0) && (m_quoteSystem.NormalLevels.Count > 0))
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

							if ((m_quoteLevel > 0) && (m_quoteSystem.NormalLevels.Count > 0) &&
								token.StartsWith(CloserForCurrentLevel) && blockInWhichDialogueQuoteStarted == null)
							{
								sb.Append(token);
								DecrementQuoteLevel();
								if (m_quoteLevel == 0)
									FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							}
							else if (m_quoteSystem.NormalLevels.Count > m_quoteLevel && token.StartsWith(OpenerForNextLevel) && blockInWhichDialogueQuoteStarted == null)
							{
								if (m_quoteLevel == 0)
									FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								sb.Append(token);
								IncrementQuoteLevel();
							}
							else if (m_quoteLevel == 0 && m_quoteSystem.QuotationDashMarker != null && token.StartsWith(m_quoteSystem.QuotationDashMarker))
							{
								blockEndedWithSentenceEndingPunctuation = false;
								pendingColon = token.StartsWith(":");
								if (pendingColon)
									sb.Append(token);
								FlushStringBuilderAndBlock(sb, block.StyleTag, false);
								if (!pendingColon)
								{
									blockInWhichDialogueQuoteStarted = block;
									sb.Append(token);
								}
								IncrementQuoteLevel();
							}
							else if (potentialDialogueContinuer || (m_quoteLevel == 1 && blockInWhichDialogueQuoteStarted != null))
							{
								if (!string.IsNullOrEmpty(m_quoteSystem.QuotationDashEndMarker) && token.StartsWith(m_quoteSystem.QuotationDashEndMarker, StringComparison.Ordinal))
								{
									DecrementQuoteLevel();
									potentialDialogueContinuer = false;
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
					if (sb.Length > 0)
						m_outputBlocks.Last().BlockElements.OfType<ScriptText>().Last().Content += sb.ToString();
				}
				FlushBlock(block.StyleTag, m_quoteLevel > 0);
			}
			if (blockInWhichDialogueQuoteStarted != null)
			{
				m_nextBlockContinuesQuote = false;
			}
			if (m_nextBlockContinuesQuote)
			{
				foreach (var multiBlock in m_currentMultiBlockQuote)
				{
					multiBlock.MultiBlockQuote = MultiBlockQuote.None;
					multiBlock.CharacterId = CharacterVerseData.UnknownCharacter;
					multiBlock.Delivery = null;
				}
			}
			else
			{
				// In case the last set of blocks were a multi-block quote
				ProcessMultiBlock();
			}
			return m_outputBlocks;
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
			if (--m_quoteLevel == 0)
				m_possibleCharactersForCurrentQuote.Clear();
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
					if (CharacterUtils.IsSentenceFinalPunctuation(c))
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
			if (characterUnknown)
			{
				m_workingBlock.CharacterId = CharacterVerseData.UnknownCharacter;
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

					m_workingBlock.SetCharacterAndDelivery(
						m_cvInfo.GetCharacters(m_bookNum, m_workingBlock.ChapterNumber, m_workingBlock.InitialStartVerseNumber, m_workingBlock.InitialEndVerseNumber, m_workingBlock.LastVerse, m_versification));
				}
				else
				{
					m_nextBlockContinuesQuote = false;
					m_workingBlock.SetStandardCharacter(m_bookId, CharacterVerseData.StandardCharacter.Narrator);
				}
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

			if (m_currentMultiBlockQuote.Count == 1)
			{
				m_currentMultiBlockQuote[0].MultiBlockQuote = MultiBlockQuote.None;
				m_currentMultiBlockQuote.Clear();
				return;
			}

			var uniqueCharacters = m_currentMultiBlockQuote.Select(b => b.CharacterId).Distinct().ToList();
			int numUniqueCharacters = uniqueCharacters.Count;
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
				block.SetCharacterAndCharacterIdInScript(character, m_bookNum, m_versification);
				block.Delivery = delivery;
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

		public static ConcurrentDictionary<BookScript, IReadOnlyList<Block>> Unparse(IEnumerable<BookScript> books)
		{
			var blocksInBook = new ConcurrentDictionary<BookScript, IReadOnlyList<Block>>();

			Parallel.ForEach(books, book =>
			{
				var oldBlocks = book.GetScriptBlocks();
				var newBlocks = new List<Block>();
				Block currentBlock = null;

				foreach (var oldBlock in oldBlocks)
				{
					// is this a new chapter?
					if (oldBlock.IsParagraphStart || (currentBlock == null))
					{
						if (currentBlock != null) newBlocks.Add(currentBlock);

						if (CharacterVerseData.IsCharacterStandard(oldBlock.CharacterId, false) && !oldBlock.UserConfirmed)
						{
							newBlocks.Add(oldBlock.Clone());
							currentBlock = null;
							continue;
						}
						else
						{
							currentBlock = new Block(oldBlock.StyleTag, oldBlock.ChapterNumber, oldBlock.InitialStartVerseNumber,
								oldBlock.InitialEndVerseNumber);
							currentBlock.IsParagraphStart = oldBlock.IsParagraphStart;
						}
					}

					foreach (var element in oldBlock.BlockElements)
					{
						if (element is Verse)
						{
							currentBlock.BlockElements.Add(element.Clone());
							continue;
						}

						// element is Glyssen.ScriptText
						// check if this text should be appended to the previous element
						var lastElement = currentBlock.BlockElements.LastOrDefault() as ScriptText;
						if (lastElement != null)
						{
							lastElement.Content += ((ScriptText) element).Content;
						}
						else
						{
							currentBlock.BlockElements.Add(element.Clone());
						}
					}
				}

				// add the last block now
				if (currentBlock != null)
					newBlocks.Add(currentBlock);

				blocksInBook.AddOrUpdate(book, newBlocks, (script, list) => newBlocks);
			});

			return blocksInBook;
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

		private class QuoteCharComparer : IComparer<char>
		{
			public int Compare(char x, char y)
			{
				// Putting regular dash at the beginning makes the regex not try to treat it as a range operator
				if (x.Equals('-'))
					return -1;
				return x.CompareTo(y);
			}
		}
	}
}
