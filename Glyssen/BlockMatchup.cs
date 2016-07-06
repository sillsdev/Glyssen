using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace Glyssen
{
	public class BlockMatchup
	{
		public enum MatchState
		{
			MatchedWithAllAssignmentsComplete,
			MatchedWithUnassignedCharacters,
			MismatchedWithAllAssignmentsComplete,
			MismatchedWithUnassignedCharacters,
		}

		private readonly BookScript m_vernacularBook;
		private readonly int m_iStartBlock;
		private PortionScript m_portion;

		public BlockMatchup(BookScript vernacularBook, int iBlock)
		{
			m_vernacularBook = vernacularBook;
			var blocks = vernacularBook.GetScriptBlocks();
			var block = blocks[iBlock];
			var blocksForVersesCoveredByBlock =
				vernacularBook.GetBlocksForVerse(block.ChapterNumber, block.InitialStartVerseNumber).ToList();
			m_iStartBlock = iBlock - blocksForVersesCoveredByBlock.IndexOf(block);
			while (!blocksForVersesCoveredByBlock.First().StartsAtVerseStart)
			{
				var prepend = vernacularBook.GetBlocksForVerse(block.ChapterNumber, blocksForVersesCoveredByBlock.First().InitialStartVerseNumber).ToList();
				prepend.RemoveAt(prepend.Count - 1);
				m_iStartBlock -= prepend.Count;
				blocksForVersesCoveredByBlock.InsertRange(0, prepend);
			}
			int iLastBlock = m_iStartBlock + blocksForVersesCoveredByBlock.Count - 1;
			int i = iLastBlock;
			AdvanceToCleanVerseBreak(blocks, ref i);
			if (i > iLastBlock)
				blocksForVersesCoveredByBlock.AddRange(blocks.Skip(iLastBlock + 1).Take(i - iLastBlock));
			m_portion = new PortionScript(vernacularBook.BookId, blocksForVersesCoveredByBlock.Select(b => b.Clone()));
		}
 
		public IReadOnlyList<Block> CorrelatedBlocks { get { return m_portion.GetScriptBlocks(); } }

		public MatchState ReferenceTextMatchState
		{
			get
			{
				bool assignmentsIncomplete = CorrelatedBlocks.Any(b => b.CharacterIsUnclear());
				if (assignmentsIncomplete)
				{
					return AllBlocksMatch ? MatchState.MatchedWithUnassignedCharacters :
						MatchState.MismatchedWithUnassignedCharacters;
				}
				return AllBlocksMatch ? MatchState.MatchedWithAllAssignmentsComplete :
					MatchState.MismatchedWithAllAssignmentsComplete;
			}
		}

		public bool AllBlocksMatch { get { return CorrelatedBlocks.All(b => b.MatchesReferenceText); } }

		public void Apply()
		{
			var origBlocks = m_vernacularBook.GetScriptBlocks();
			for (int i = 0; i < CorrelatedBlocks.Count; i++)
			{
				origBlocks[m_iStartBlock + i].SetMatchedReferenceBlock(CorrelatedBlocks[i].ReferenceBlocks.Single());
			}
		}

		public static void AdvanceToCleanVerseBreak(IReadOnlyList<Block> blockList, ref int i)
		{
			for (; i < blockList.Count - 1 && !blockList[i + 1].StartsAtVerseStart && !blockList[i + 1].IsChapterAnnouncement; i++)
			{
			}
		}
	}
}
