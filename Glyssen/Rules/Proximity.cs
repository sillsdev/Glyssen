using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glyssen.Bundle;
using Glyssen.Character;
using SIL.Scripture;

namespace Glyssen.Rules
{
	public class Proximity
	{
		public const int kDefaultMinimumBlocks = 15;
		//public const int kDefaultMinimumKeystrokes = 110 * kDefaultMinimumBlocks;

		private readonly IReadOnlyList<BookScript> m_booksToConsider;
		private readonly bool m_narrationByAuthor;
		private readonly ReferenceText m_referenceText;
		private readonly ProjectDramatizationPreferences m_dramatizationPreferences;
		private readonly Dictionary<BookScript, HashSet<string>> m_considerSameExtrabiblicalCharacter;

		public Proximity(Project project)
		{
			m_referenceText = project.ReferenceText;
			m_booksToConsider = m_referenceText.GetBooksWithBlocksConnectedToReferenceText(project).ToList();
			m_dramatizationPreferences = project.DramatizationPreferences;
			m_narrationByAuthor = project.CharacterGroupGenerationPreferences.NarratorsOption == NarratorsOption.NarrationByAuthor;
			m_considerSameExtrabiblicalCharacter = new Dictionary<BookScript, HashSet<string>>();
		}

		private HashSet<string> GetStandardCharactersAndAuthorsToTreatAsOne(BookScript book)
		{
			HashSet<string> treatAsSameCharacter;
			if (!m_considerSameExtrabiblicalCharacter.TryGetValue(book, out treatAsSameCharacter))
			{
				treatAsSameCharacter = new HashSet<string>();

				var bookId = book.BookId;
				if (m_dramatizationPreferences.BookTitleAndChapterDramatization != ExtraBiblicalMaterialSpeakerOption.Omitted)
					treatAsSameCharacter.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.BookOrChapter));
				if (m_dramatizationPreferences.SectionHeadDramatization != ExtraBiblicalMaterialSpeakerOption.Omitted)
					treatAsSameCharacter.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical));
				if (m_dramatizationPreferences.BookIntroductionsDramatization != ExtraBiblicalMaterialSpeakerOption.Omitted)
					treatAsSameCharacter.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Intro));

				if (m_narrationByAuthor)
				{
					var author = BiblicalAuthors.GetAuthorOfBook(book.BookId);
					if (author.CombineAuthorAndNarrator)
						treatAsSameCharacter.Add(author.Name);
				}

				// For performance reasons, in the unlikely event that nothing was added to the set, then we can return null.
				// Otherwise, we also need to include the narrator.
				if (treatAsSameCharacter.Any())
					treatAsSameCharacter.Add(CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator));
				else
					treatAsSameCharacter = null;
				m_considerSameExtrabiblicalCharacter[book] = treatAsSameCharacter;
			}
			return treatAsSameCharacter;
		}

		/// <summary>Calculate the minimum number of blocks between two character ids in given collection</summary>
		public MinimumProximity CalculateMinimumProximity(ISet<string> characterIdsToCalculate)
		{
			if (!characterIdsToCalculate.Any())
				return new MinimumProximity(Int32.MaxValue, null, null, null, null);

			RelatedCharactersData relChar = RelatedCharactersData.Singleton;
			bool foundFirst = false;
			int currentBlockCount = 0;
			int minBlockProximity = Int32.MaxValue;
			string prevCharacterId = null;
			ISet<string> prevMatchingCharacterIds = null;
			BookScript firstBook = null;
			Block firstBlock = null;
			BookScript secondBook = null;
			Block secondBlock = null;
			BookScript prevBook = null;
			Block prevBlock = null;
			bool breakOutOfBothLoops = false;

			bool calculateAnyRelatedCharacters = characterIdsToCalculate.Any(c => relChar.HasMatchingCharacterIdsOfADifferentAge(c));
			foreach (var book in m_booksToConsider)
			{
				if (breakOutOfBothLoops)
					break;

				var countVersesRatherThanBlocks = !m_referenceText.HasContentForBook(book.BookId);

				var treatAsSameCharacter = GetStandardCharactersAndAuthorsToTreatAsOne(book);

				// We don't want to treat book ends as being directly adjacent but not infinitely distant, either.
				currentBlockCount += kDefaultMinimumBlocks * 5 / 3; // The amount of padding is somewhat arbitrary.

				foreach (var block in book.Blocks)
				{
					var characterId = block.CharacterIdInScript;

					// The original logic here was NOT split out for the single character vs. multiple character scenarios.
					// This made the code much more readable, but the performance was atrocious since we were creating
					// extra hashsets and doing extra intersects. Please consider the performance implications of any
					// changes to this code.  (I'm sure it could be optimized further, too...)

					ISet<string> matchingCharacterIds = null;
					if (calculateAnyRelatedCharacters &&
						relChar.TryGetMatchingCharacterIdsOfADifferentAge(characterId, out matchingCharacterIds))
					{
						if (matchingCharacterIds.Count == 1)
							matchingCharacterIds = null;
					}
					else if (treatAsSameCharacter != null && treatAsSameCharacter.Contains(characterId))
					{
						matchingCharacterIds = treatAsSameCharacter;
					}

					if (matchingCharacterIds == null)
					{
						if ((prevMatchingCharacterIds == null && prevCharacterId == characterId) ||
							(prevMatchingCharacterIds != null && prevMatchingCharacterIds.Contains(characterId)))
						{
							currentBlockCount = 0;
							prevBook = book;
							prevBlock = block;
						}
						else if (characterIdsToCalculate.Contains(characterId) &&
								(!CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator) ||
								prevCharacterId == null ||
								!CharacterVerseData.IsCharacterOfType(prevCharacterId, CharacterVerseData.StandardCharacter.Narrator)))
						{
							if (ProcessDifferentCharacter(book, block, characterId, matchingCharacterIds, ref foundFirst,
								ref currentBlockCount, ref minBlockProximity,
								ref firstBook, ref prevBook, ref firstBlock, ref prevBlock,
								ref secondBook, ref secondBlock, ref breakOutOfBothLoops, ref prevCharacterId, ref prevMatchingCharacterIds))
								break;
						}
						else
						{
							IncrementCount(countVersesRatherThanBlocks, block, ref currentBlockCount);
						}
					}
					else if (prevMatchingCharacterIds != null && matchingCharacterIds.Intersect(prevMatchingCharacterIds).Any())
					{
						currentBlockCount = 0;
						prevBook = book;
						prevBlock = block;
						prevMatchingCharacterIds = matchingCharacterIds;
					}
					else if (characterIdsToCalculate.Intersect(matchingCharacterIds).Any())
					{
						if (ProcessDifferentCharacter(book, block, characterId, matchingCharacterIds, ref foundFirst,
							ref currentBlockCount, ref minBlockProximity,
							ref firstBook, ref prevBook, ref firstBlock, ref prevBlock,
							ref secondBook, ref secondBlock, ref breakOutOfBothLoops, ref prevCharacterId, ref prevMatchingCharacterIds))
							break;
					}
					else
					{
						IncrementCount(countVersesRatherThanBlocks, block, ref currentBlockCount);
					}
				}
			}

			return new MinimumProximity(minBlockProximity, firstBook, secondBook, firstBlock, secondBlock);
		}

		private void IncrementCount(bool countVersesRatherThanBlocks, Block block, ref int count)
		{
			if (countVersesRatherThanBlocks)
				count += block.ScriptTextCount;
			else
				count++;
			// TODO we think we want this eventually, but it is not performant, and we are not ready to use it.
			//currentKeystrokeCount += block.Length;
		}
		
		private static bool ProcessDifferentCharacter(
			BookScript book, Block block, string characterId, ISet<string> matchingCharacterIds,
			ref bool foundFirst,
			ref int currentBlockCount, ref int minBlockProximity,
			ref BookScript firstBook, ref BookScript prevBook, ref Block firstBlock, ref Block prevBlock,
			ref BookScript secondBook, ref Block secondBlock,
			ref bool breakOutOfBothLoops,
			ref string prevCharacterId, ref ISet<string> prevMatchingCharacterIds)
		{
			if (foundFirst)
			{
				if (currentBlockCount < minBlockProximity)
				{
					minBlockProximity = currentBlockCount;
					firstBook = prevBook;
					firstBlock = prevBlock;
					secondBook = book;
					secondBlock = block;

					if (minBlockProximity == 0)
					{
						breakOutOfBothLoops = true;
						return true;
					}
				}
			}
			else
			{
				firstBook = book;
				firstBlock = block;
				secondBook = book;
				secondBlock = block;
			}
			foundFirst = true;
			currentBlockCount = 0;
			prevCharacterId = characterId;
			prevMatchingCharacterIds = matchingCharacterIds;

			prevBook = book;
			prevBlock = block;
			return false;
		}
	}

	public class MinimumProximity
	{
		private readonly BookScript m_firstBook;
		private readonly BookScript m_secondBook;
		private readonly Block m_firstBlock;
		private readonly Block m_secondBlock;

		/// <summary>
		/// Number of blocks <em>between</em> characters
		/// (so the minimum is 0)
		/// </summary>
		public int NumberOfBlocks { get; }

		/// <summary>
		/// Number of "keystrokes" (writing system characters) <em>between</em> characters
		/// </summary>
		//public int NumberOfKeystrokes { get; }

		public string FirstReference => new BCVRef(BCVRef.BookToNumber(m_firstBook.BookId), m_firstBlock.ChapterNumber,
			m_firstBlock.InitialStartVerseNumber).ToString();

		public string SecondReference => new BCVRef(BCVRef.BookToNumber(m_secondBook.BookId), m_secondBlock.ChapterNumber,
			m_secondBlock.InitialStartVerseNumber).ToString();

		public string FirstCharacterId => m_firstBlock.CharacterIdInScript;

		public string SecondCharacterId => m_secondBlock.CharacterIdInScript;

		public MinimumProximity(int numberOfBlocks, BookScript firstBook, BookScript secondBook, Block firstBlock, Block secondBlock)
		{
			NumberOfBlocks = numberOfBlocks;
			m_firstBook = firstBook;
			m_secondBook = secondBook;
			m_firstBlock = firstBlock;
			m_secondBlock = secondBlock;
		}

		public MinimumProximity(MinimumProximity copyFrom)
		{
			NumberOfBlocks = copyFrom.NumberOfBlocks;
			m_firstBook = copyFrom.m_firstBook;
			m_secondBook = copyFrom.m_secondBook;
			m_firstBlock = copyFrom.m_firstBlock;
			m_secondBlock = copyFrom.m_secondBlock;
		}

		private const string kProximityHeader = "Proximity";
		public static string ReportHeader => kProximityHeader + "  | Characters & Locations";
		public override string ToString()
		{
			if (m_firstBlock == null || m_secondBlock == null)
				return "[no characters in group]";

			var sb = new StringBuilder();
			sb.Append((NumberOfBlocks == Int32.MaxValue ? "MAX" : NumberOfBlocks.ToString()).PadLeft(kProximityHeader.Length)).Append("  |  ")
				.Append(FirstReference)
				.Append(" (").Append(m_firstBlock.CharacterIdInScript).Append(")")
				.Append(" - ")
				.Append(SecondReference)
				.Append(" (").Append(m_secondBlock.CharacterIdInScript).Append(")");
			return sb.ToString();
		}
	}

	public class WeightedMinimumProximity : MinimumProximity
	{
		public double WeightingPower { get; set; }

		public int WeightedNumberOfBlocks => (int)Math.Round(Math.Pow(NumberOfBlocks, WeightingPower));

		public WeightedMinimumProximity(MinimumProximity minimumProximity) : base(minimumProximity)
		{
			WeightingPower = 1;
		}
	}

	public static class MinimumProximityExtensions
	{
		public static bool IsAcceptable(this MinimumProximity minimumProximity)
		{
			return minimumProximity == null || minimumProximity.NumberOfBlocks >= Proximity.kDefaultMinimumBlocks;
		}

		public static bool IsFinite(this MinimumProximity minimumProximity)
		{
			return minimumProximity != null && minimumProximity.NumberOfBlocks < Int32.MaxValue;
		}

		public static bool IsBetterThan(this MinimumProximity a, MinimumProximity b)
		{
			return (a?.NumberOfBlocks ?? Int32.MaxValue) > (b?.NumberOfBlocks ?? Int32.MaxValue);
		}

		public static bool IsBetterThanOrEqualTo(this MinimumProximity a, MinimumProximity b)
		{
			return (a?.NumberOfBlocks ?? Int32.MaxValue) >= (b?.NumberOfBlocks ?? Int32.MaxValue);
		}

		public static bool IsAcceptable(this WeightedMinimumProximity weightedMinimumProximity)
		{
			return weightedMinimumProximity == null || weightedMinimumProximity.NumberOfBlocks >= Proximity.kDefaultMinimumBlocks;
		}

		public static bool IsBetterThan(this WeightedMinimumProximity a, WeightedMinimumProximity b)
		{
			return (a?.WeightedNumberOfBlocks ?? Int32.MaxValue) > (b?.WeightedNumberOfBlocks ?? Int32.MaxValue);
		}

		public static bool IsBetterThanOrEqualTo(this WeightedMinimumProximity a, WeightedMinimumProximity b)
		{
			return (a?.WeightedNumberOfBlocks ?? Int32.MaxValue) >= (b?.WeightedNumberOfBlocks ?? Int32.MaxValue);
		}
	}
}
