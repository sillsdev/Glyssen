using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glyssen.Character;

namespace Glyssen.Rules
{
	public class Proximity
	{
		public const int kDefaultMinimumProximity = 30;
		private readonly Project m_project;

		public Proximity(Project project)
		{
			m_project = project;
		}

		/// <summary>
		/// Calculate the minimum number of blocks between two character ids in given collection
		/// </summary>
		public MinimumProximity CalculateMinimumProximity(ISet<string> characterIdsToCalculate, bool handleEachBookSeparately = true)
		{
			RelatedCharactersData relChar = RelatedCharactersData.Singleton;
			bool foundFirst = false;
			int currentBlockCount = 0;
			int minProximity = Int32.MaxValue;
			bool prevIsSingleCharacterId = true;
			string prevCharacterId = null;
			ISet<string> prevMatchingCharacterIds = new HashSet<string>();
			BookScript firstBook = null;
			Block firstBlock = null;
			BookScript secondBook = null;
			Block secondBlock = null;
			BookScript prevBook = null;
			Block prevBlock = null;
			bool breakOutOfBothLoops = false;

			ISet<string> characterIdsWithAgeVariations =
				relChar.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges);

			foreach (var book in m_project.IncludedBooks)
			{
				if (breakOutOfBothLoops)
					break;

				if (handleEachBookSeparately)
					currentBlockCount += 50;

				foreach (var block in book.Blocks)
				{
					var characterId = block.CharacterIdInScript;

					// The original logic here was NOT split out for the single character vs. multiple character scenarios.
					// This made the code much more readable, but the performance was atrocious since we were creating
					// extra hashsets and doing extra intersects. Please consider the performance implications of any
					// changes to this code.  (I'm sure it could be optimized further, too...)

					bool isSingleCharacterId = true;
					ISet<string> matchingCharacterIds = new HashSet<string>();
					if (characterIdsWithAgeVariations.Contains(characterId))
					{
						matchingCharacterIds = relChar.GetMatchingCharacterIds(characterId, CharacterRelationshipType.SameCharacterWithMultipleAges);
						if (matchingCharacterIds.Count == 1)
							characterId = matchingCharacterIds.First();
						else
							isSingleCharacterId = false;
					}

					if (isSingleCharacterId)
					{
						if (prevIsSingleCharacterId && prevCharacterId == characterId || prevMatchingCharacterIds.Contains(characterId))
						{
							currentBlockCount = 0;
							prevBook = book;
							prevBlock = block;
						}
						else if (characterIdsToCalculate.Contains(characterId))
						{
							if (ProcessDifferentCharacter(book, block, isSingleCharacterId, characterId, matchingCharacterIds, ref foundFirst, ref currentBlockCount, ref minProximity, ref firstBook, ref prevBook, ref firstBlock, ref prevBlock, ref secondBook, ref secondBlock, ref breakOutOfBothLoops, ref prevIsSingleCharacterId, ref prevCharacterId, ref prevMatchingCharacterIds))
								break;
						}
						else
							currentBlockCount++;
					}
					else if (matchingCharacterIds.Intersect(prevMatchingCharacterIds).Any())
					{
						currentBlockCount = 0;
						prevBook = book;
						prevBlock = block;
					}
					else if (characterIdsToCalculate.Intersect(matchingCharacterIds).Any())
					{
						if (ProcessDifferentCharacter(book, block, isSingleCharacterId, characterId, matchingCharacterIds, ref foundFirst, ref currentBlockCount, ref minProximity, ref firstBook, ref prevBook, ref firstBlock, ref prevBlock, ref secondBook, ref secondBlock, ref breakOutOfBothLoops, ref prevIsSingleCharacterId, ref prevCharacterId, ref prevMatchingCharacterIds))
							break;
					}
					else
					{
						currentBlockCount++;
					}
				}
			}

			return new MinimumProximity
			{
				NumberOfBlocks = minProximity,
				FirstBook = firstBook,
				SecondBook = secondBook,
				FirstBlock = firstBlock,
				SecondBlock = secondBlock
			};
		}

		private static bool ProcessDifferentCharacter(BookScript book, Block block, bool isSingleCharacterId, string characterId, ISet<string> matchingCharacterIds, ref bool foundFirst, ref int currentBlockCount, ref int minProximity, ref BookScript firstBook, ref BookScript prevBook, ref Block firstBlock, ref Block prevBlock, ref BookScript secondBook, ref Block secondBlock, ref bool breakOutOfBothLoops, ref bool prevIsSingleCharacterId, ref string prevCharacterId, ref ISet<string> prevMatchingCharacterIds)
		{
			if (foundFirst)
			{
				if (currentBlockCount < minProximity)
				{
					minProximity = currentBlockCount;
					firstBook = prevBook;
					firstBlock = prevBlock;
					secondBook = book;
					secondBlock = block;

					if (minProximity == 0)
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
			prevIsSingleCharacterId = isSingleCharacterId;
			prevCharacterId = characterId;
			prevMatchingCharacterIds = matchingCharacterIds;

			prevBook = book;
			prevBlock = block;
			return false;
		}
	}

	public class MinimumProximity
	{
		public int NumberOfBlocks { get; set; }
		public BookScript FirstBook { get; set; }
		public BookScript SecondBook { get; set; }
		public Block FirstBlock { get; set; }
		public Block SecondBlock { get; set; }

		public override string ToString()
		{
			if (FirstBlock == null || SecondBlock == null)
				return "[no characters in group]";

			var sb = new StringBuilder();
			sb.Append(NumberOfBlocks == Int32.MaxValue ? "MAX" : NumberOfBlocks.ToString()).Append("  |  ")
				.Append(FirstBook.BookId).Append(" ").Append(FirstBlock.ChapterNumber).Append(":").Append(FirstBlock.InitialStartVerseNumber)
				.Append(" (").Append(FirstBlock.CharacterIdInScript).Append(")")
				.Append(" - ")
				.Append(SecondBook.BookId).Append(" ").Append(SecondBlock.ChapterNumber).Append(":").Append(SecondBlock.InitialStartVerseNumber)
				.Append(" (").Append(SecondBlock.CharacterIdInScript).Append(")");
			return sb.ToString();
		}
	}

	public class WeightedMinimumProximity : MinimumProximity
	{
		public double WeightingPower { get; set; }

		public int WeightedNumberOfBlocks
		{
			get { return (int)Math.Round(Math.Pow(NumberOfBlocks, WeightingPower)); }
		}

		public WeightedMinimumProximity(MinimumProximity minimumProximity)
		{
			NumberOfBlocks = minimumProximity.NumberOfBlocks;
			FirstBook = minimumProximity.FirstBook;
			SecondBook = minimumProximity.SecondBook;
			FirstBlock = minimumProximity.FirstBlock;
			SecondBlock = minimumProximity.SecondBlock;
			WeightingPower = 1;
		}
	}
}
