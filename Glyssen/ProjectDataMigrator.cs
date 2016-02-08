using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;

namespace Glyssen
{
	internal class ProjectDataMigrator
	{
		public static void MigrateProjectData(Project project, int fromControlFileVersion)
		{
			if (project.ProjectState == ProjectState.FullyInitialized)
			{
				if (fromControlFileVersion < 88)
					MigrateInvalidMultiBlockQuoteDataToVersion88(project.Books);
				if (fromControlFileVersion < 89)
					CleanUpOrphanedMultiBlockQuoteStati(project.Books);
				if (fromControlFileVersion < 88)
					MigrateInvalidCharacterIdForScriptDataToVersion88(project.Books);
			}
		}

		// internal for testing
		internal static void MigrateInvalidMultiBlockQuoteDataToVersion88(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				ISet<int> processedSplitIds = new HashSet<int>();
				var blocks = book.GetScriptBlocks();
				foreach (var firstBlockOfSplit in blocks.Where(b => b.SplitId != -1))
				{
					int splitId = firstBlockOfSplit.SplitId;
					if (processedSplitIds.Contains(splitId))
						continue;
					if (firstBlockOfSplit.MultiBlockQuote == MultiBlockQuote.None)
					{
						processedSplitIds.Add(splitId);
						continue;
					}

					List<Block> subsequentBlocksInSplit = new List<Block>(5);
					Block firstBlockAfterSplit = null;
					int iBlock = blocks.IndexOf(firstBlockOfSplit);
					while (++iBlock < blocks.Count)
					{
						if (blocks[iBlock].SplitId == splitId)
							subsequentBlocksInSplit.Add(blocks[iBlock]);
						else
						{
							firstBlockAfterSplit = blocks[iBlock];
							break;
						}
					}

					if (!subsequentBlocksInSplit.Any())
					{
						// This should never happen
						Debug.Fail("Splits should always have more than one block and those blocks should be sequential.");
						processedSplitIds.Add(splitId);
						continue;
					}

					if (subsequentBlocksInSplit.All(b => b.MultiBlockQuote == MultiBlockQuote.None) &&
						(firstBlockAfterSplit == null ||
						firstBlockAfterSplit.MultiBlockQuote == MultiBlockQuote.Continuation ||
						firstBlockAfterSplit.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery))
					{
						if (firstBlockOfSplit.MultiBlockQuote == MultiBlockQuote.Start)
							firstBlockOfSplit.MultiBlockQuote = MultiBlockQuote.None;
						subsequentBlocksInSplit.Last().MultiBlockQuote = MultiBlockQuote.Start;
					}

					processedSplitIds.Add(splitId);
				}
			}
		}

		//internal for testing
		internal static void CleanUpOrphanedMultiBlockQuoteStati(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				var blocks = book.GetScriptBlocks();
				if (!blocks.Any())
					continue;

				Block previousBlock = blocks[0];
				if (blocks[0].MultiBlockQuote != MultiBlockQuote.None && blocks[0].MultiBlockQuote != MultiBlockQuote.Start)
					previousBlock.MultiBlockQuote = MultiBlockQuote.None;
				MultiBlockQuote previousBlockMultiBlockStatus = previousBlock.MultiBlockQuote;
				foreach (var block in blocks.Skip(1))
				{
					if (block.MultiBlockQuote == MultiBlockQuote.None)
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.Start)
							previousBlock.MultiBlockQuote = MultiBlockQuote.None;
					}
					else if (block.MultiBlockQuote == MultiBlockQuote.Start)
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.Start)
							previousBlock.MultiBlockQuote = MultiBlockQuote.None;
					}
					else
					{
						if (previousBlockMultiBlockStatus == MultiBlockQuote.None)
							block.MultiBlockQuote = MultiBlockQuote.None;
					}

					previousBlock = block;
					previousBlockMultiBlockStatus = block.MultiBlockQuote;
				}
			}
		}

		public static void MigrateInvalidCharacterIdForScriptDataToVersion88(IReadOnlyList<BookScript> books)
		{
			foreach (var book in books)
			{
				var blocks = book.GetScriptBlocks();
				foreach (var block in blocks)
				{
					if ((block.CharacterId == CharacterVerseData.AmbiguousCharacter || block.CharacterId == CharacterVerseData.UnknownCharacter) &&
						block.CharacterIdOverrideForScript != null)
						block.CharacterIdInScript = null;
				}
			}
		}
	}
}
