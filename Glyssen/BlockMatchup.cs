using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;

namespace Glyssen
{
	public class BlockMatchup
	{
		private readonly BookScript m_vernacularBook;
		private readonly int m_iStartBlock;
		private readonly PortionScript m_portion;
		private readonly int m_numberOfBlocksAddedBySplitting = 0;

		public BlockMatchup(BookScript vernacularBook, int iBlock, Action<PortionScript> splitBlocks)
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
			while (CharacterVerseData.IsCharacterOfType(blocksForVersesCoveredByBlock.Last().CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical))
				blocksForVersesCoveredByBlock.RemoveAt(blocksForVersesCoveredByBlock.Count - 1);

			m_portion = new PortionScript(vernacularBook.BookId, blocksForVersesCoveredByBlock.Select(b => b.Clone()));
			if (splitBlocks != null)
			{
				int origCount = m_portion.GetScriptBlocks().Count;
				splitBlocks(m_portion);
				m_numberOfBlocksAddedBySplitting = m_portion.GetScriptBlocks().Count - origCount;
				HasOutstandingChangesToApply = m_numberOfBlocksAddedBySplitting > 0;
			}
		}

		public IEnumerable<Block> OriginalBlocks
		{
			get
			{
				return m_vernacularBook.GetScriptBlocks().Skip(m_iStartBlock).Take(CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting);
			}
		}

		public IReadOnlyList<Block> CorrelatedBlocks { get { return m_portion.GetScriptBlocks(); } }

		public bool HasOutstandingChangesToApply { get; private set; }

		public bool AllScriptureBlocksMatch
		{
			get
			{
				return CorrelatedBlocks.All(b => b.MatchesReferenceText ||
					b.CharacterIs(m_vernacularBook.BookId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			}
		}
		public int IndexOfStartBlockInBook
		{
			get { return m_iStartBlock; }
		}

		public int Apply()
		{
			if (!AllScriptureBlocksMatch)
				throw new InvalidOperationException("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.");

			if (m_numberOfBlocksAddedBySplitting > 0)
			{
				m_vernacularBook.ReplaceBlocks(m_iStartBlock, CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting,
					CorrelatedBlocks);
			}
			var origBlocks = m_vernacularBook.GetScriptBlocks();
			for (int i = 0; i < CorrelatedBlocks.Count; i++)
			{
				if (!CorrelatedBlocks[i].MatchesReferenceText)
					continue;
				var vernBlock = origBlocks[m_iStartBlock + i];
				var refBlock = CorrelatedBlocks[i].ReferenceBlocks.Single();
				vernBlock.SetMatchedReferenceBlock(refBlock);
				if (vernBlock.CharacterId != refBlock.CharacterId)
				{
					vernBlock.CharacterId = refBlock.CharacterId;
					if (refBlock.CharacterIdOverrideForScript != null)
						vernBlock.CharacterIdOverrideForScript = refBlock.CharacterIdOverrideForScript;
				}
			}
			return m_numberOfBlocksAddedBySplitting;
		}

		public Block SetReferenceText(int blockIndex, string text, int level = 0)
		{
			var block = CorrelatedBlocks[blockIndex];
			return block.SetMatchedReferenceBlock(text, (blockIndex > 0) ? CorrelatedBlocks[blockIndex - 1].ReferenceBlocks.LastOrDefault() : null);
		}

		public void SetCharacter(int blockIndex, string character)
		{
			throw new NotImplementedException();
		}

		public void SetDelivery(int blockIndex, string delivery)
		{
			throw new NotImplementedException();
		}

		public static void AdvanceToCleanVerseBreak(IReadOnlyList<Block> blockList, ref int i)
		{
			for (; i < blockList.Count - 1 && !blockList[i + 1].StartsAtVerseStart && !blockList[i + 1].IsChapterAnnouncement; i++)
			{
			}
		}

		public bool IncludesBlock(Block block)
		{
			return OriginalBlocks.Contains(block);
		}

		public void MatchAllBlocks()
		{
			foreach (var block in CorrelatedBlocks)
			{
				if (block.MatchesReferenceText)
				{
					if (block.CharacterIsUnclear())
					{
						var refBlock = block.ReferenceBlocks.Single();
						block.SetCharacterAndDeliveryInfo(refBlock);
						HasOutstandingChangesToApply = true;
					}
				}
				else
				{
					var refBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber,
						block.InitialEndVerseNumber);
					refBlock.SetCharacterAndDeliveryInfo(block);
					if (block.ReferenceBlocks.Any())
						refBlock.AppendJoinedBlockElements(block.ReferenceBlocks);
					else
						refBlock.BlockElements.Add(new ScriptText(""));
					block.SetMatchedReferenceBlock(refBlock);
					HasOutstandingChangesToApply = true;
				}
			}
		}
	}
}
