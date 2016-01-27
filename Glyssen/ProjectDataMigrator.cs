using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Glyssen
{
	internal class ProjectDataMigrator
	{
		public static void MigrateProjectData(Project project, int fromControlFileVersion)
		{
			if (fromControlFileVersion < 87)
				MigrateToVersion87(project.Books);
		}

		// internal for testing
		internal static void MigrateToVersion87(IReadOnlyList<BookScript> books)
		{
			ISet<int> processedSplitIds = new HashSet<int>();
			foreach (var book in books)
			{
				var blocks = book.GetScriptBlocks();
				foreach (var firstBlockOfSplit in blocks.Where(b => b.SplitId != -1))
				{
					int splitId = firstBlockOfSplit.SplitId;
					if (processedSplitIds.Contains(splitId))
						continue;
					if (firstBlockOfSplit.MultiBlockQuote != MultiBlockQuote.Start)
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
						(firstBlockAfterSplit == null || firstBlockAfterSplit.MultiBlockQuote == MultiBlockQuote.Continuation))
					{
						firstBlockOfSplit.MultiBlockQuote = MultiBlockQuote.None;
						subsequentBlocksInSplit.Last().MultiBlockQuote = MultiBlockQuote.Start;
					}

					processedSplitIds.Add(splitId);
				}
			}
		}
	}
}
