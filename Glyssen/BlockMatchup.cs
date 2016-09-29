using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using Paratext;
using SIL.Scripture;

namespace Glyssen
{
	public class BlockMatchup
	{
		private readonly BookScript m_vernacularBook;
		private readonly int m_iStartBlock;
		private readonly PortionScript m_portion;
		private int m_numberOfBlocksAddedBySplitting = 0;
		private readonly IReferenceLanguageInfo m_referenceLanguageInfo;

		public BlockMatchup(BookScript vernacularBook, int iBlock, Action<PortionScript> splitBlocks,
			Func<VerseRef, bool> isOkayToBreakAtVerse, IReferenceLanguageInfo heSaidProvider)
		{
			m_vernacularBook = vernacularBook;
			int bookNum = BCVRef.BookToNumber(m_vernacularBook.BookId);
			m_referenceLanguageInfo = heSaidProvider;
			var blocks = m_vernacularBook.GetScriptBlocks();
			var originalAnchorBlock = blocks[iBlock];
			var blocksForVersesCoveredByBlock =
				vernacularBook.GetBlocksForVerse(originalAnchorBlock.ChapterNumber, originalAnchorBlock.InitialStartVerseNumber).ToList();
			m_iStartBlock = iBlock - blocksForVersesCoveredByBlock.IndexOf(originalAnchorBlock);
			while (!blocksForVersesCoveredByBlock.First().StartsAtVerseStart)
			{
				var prepend = vernacularBook.GetBlocksForVerse(originalAnchorBlock.ChapterNumber, blocksForVersesCoveredByBlock.First().InitialStartVerseNumber).ToList();
				prepend.RemoveAt(prepend.Count - 1);
				m_iStartBlock -= prepend.Count;
				blocksForVersesCoveredByBlock.InsertRange(0, prepend);
			}
			int iLastBlock = m_iStartBlock + blocksForVersesCoveredByBlock.Count - 1;
			int i = iLastBlock;
			AdvanceToCleanVerseBreak(blocks, verseNum =>
			{
				return isOkayToBreakAtVerse(new VerseRef(bookNum, originalAnchorBlock.ChapterNumber, verseNum));
			}, ref i);
			if (i > iLastBlock)
				blocksForVersesCoveredByBlock.AddRange(blocks.Skip(iLastBlock + 1).Take(i - iLastBlock));
			while (CharacterVerseData.IsCharacterOfType(blocksForVersesCoveredByBlock.Last().CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical))
				blocksForVersesCoveredByBlock.RemoveAt(blocksForVersesCoveredByBlock.Count - 1);

			m_portion = new PortionScript(vernacularBook.BookId, blocksForVersesCoveredByBlock.Select(b => b.Clone()));
			CorrelatedAnchorBlock = m_portion.GetScriptBlocks()[iBlock - m_iStartBlock];
			if (splitBlocks != null)
			{
				int origCount = m_portion.GetScriptBlocks().Count;
				splitBlocks(m_portion);
				m_numberOfBlocksAddedBySplitting = m_portion.GetScriptBlocks().Count - origCount;
			}
		}

		public IEnumerable<Block> OriginalBlocks
		{
			get
			{
				return m_vernacularBook.GetScriptBlocks().Skip(m_iStartBlock).Take(CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting);
			}
		}

		public int CountOfBlocksAddedBySplitting { get { return m_numberOfBlocksAddedBySplitting; } }

		public IReadOnlyList<Block> CorrelatedBlocks { get { return m_portion.GetScriptBlocks(); } }

		public bool HasOutstandingChangesToApply
		{
			get
			{
				if (m_numberOfBlocksAddedBySplitting > 0)
					return true;
				int i = 0;
				foreach (var realBlock in OriginalBlocks)
				{
					var correlatedBlock = CorrelatedBlocks[i++];
					if (realBlock.CharacterId != correlatedBlock.CharacterId ||
						realBlock.Delivery != correlatedBlock.Delivery ||
						realBlock.PrimaryReferenceText != correlatedBlock.PrimaryReferenceText ||
						!realBlock.ReferenceBlocks.Select(r => r.PrimaryReferenceText).SequenceEqual(correlatedBlock.ReferenceBlocks.Select(r => r.PrimaryReferenceText)))
					return true;
				}
				return false;
			}
		}

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

		public Block CorrelatedAnchorBlock { get; private set; }

#if !DEBUG
		static
#endif
		private Block GetInvalidReferenceBlockAtAnyLevel(IEnumerable<Block> blocks)
		{
			var refBlocks = blocks.Where(b => b.MatchesReferenceText).Select(b => b.ReferenceBlocks.Single()).ToList();
			var bogusRefBlock = refBlocks.FirstOrDefault(r => r.BlockElements.Last() is Verse);
			if (bogusRefBlock != null)
				return bogusRefBlock;

			if (refBlocks.Any(r => r.MatchesReferenceText))
			{
#if DEBUG
				Debug.Assert(refBlocks.All(r => r.MatchesReferenceText || r.CharacterIs(m_vernacularBook.BookId, CharacterVerseData.StandardCharacter.ExtraBiblical)),
					"All reference blocks should have the same number of levels of underlying reference blocks.");
#endif
				return GetInvalidReferenceBlockAtAnyLevel(refBlocks);
			}
			return null;
		}

		public void Apply(Paratext.ScrVers versification)
		{
			if (!AllScriptureBlocksMatch)
				throw new InvalidOperationException("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.");

			var bogusRefBlock = GetInvalidReferenceBlockAtAnyLevel(CorrelatedBlocks);
			if (bogusRefBlock != null)
				throw new InvalidReferenceTextException(bogusRefBlock);

			if (m_numberOfBlocksAddedBySplitting > 0)
			{
				m_vernacularBook.ReplaceBlocks(m_iStartBlock, CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting,
					CorrelatedBlocks.Select(b => b.Clone()));
			}
			int bookNum = BCVRef.BookToNumber(m_vernacularBook.BookId);
			var origBlocks = m_vernacularBook.GetScriptBlocks();
			for (int i = 0; i < CorrelatedBlocks.Count; i++)
			{
				if (!CorrelatedBlocks[i].MatchesReferenceText) // e.g., section head
					continue;
				var vernBlock = origBlocks[m_iStartBlock + i];

				var refBlock = CorrelatedBlocks[i].ReferenceBlocks.Single();
				vernBlock.SetMatchedReferenceBlock(refBlock);
				vernBlock.SetCharacterAndDeliveryInfo(CorrelatedBlocks[i], bookNum, versification);

				if (CorrelatedBlocks[i].UserConfirmed)
				{
					if (vernBlock.CharacterIsUnclear())
						throw new InvalidOperationException("Character cannot be confirmed as ambigous or unknown.");
					vernBlock.UserConfirmed = true;
				}

				//if (vernBlock.CharacterId != refBlock.CharacterId)
				//{
				//	vernBlock.CharacterId = refBlock.CharacterId;
				//	if (refBlock.CharacterIdOverrideForScript != null)
				//		vernBlock.CharacterIdOverrideForScript = refBlock.CharacterIdOverrideForScript;
				//}
			}
			m_numberOfBlocksAddedBySplitting = 0;
		}

		public Block SetReferenceText(int blockIndex, string text, int level = 0)
		{
			var block = CorrelatedBlocks[blockIndex];
			for (int i = 0; i < level; i++)
			{
				var clone = block.ReferenceBlocks.Single().Clone();
				block.SetMatchedReferenceBlock(clone);
				block = clone;
			}
			// To avoid losing any deeper levels (or having them be cross-linked with the original block from which they might have been copied),
			// recursively clone all deeper levels.
			block.CloneReferenceBlocks();
			if (text == null)
				text = string.Empty;
			var newRefBlock = block.SetMatchedReferenceBlock(text, (blockIndex > 0) ? CorrelatedBlocks[blockIndex - 1].ReferenceBlocks.LastOrDefault() : null);
			if (blockIndex < CorrelatedBlocks.Count - 1)
			{
				var followingBlock = CorrelatedBlocks[blockIndex + 1];
				if (followingBlock.MatchesReferenceText)
				{
					var lastVerse = newRefBlock.LastVerse;
					var followingRefBlock = followingBlock.ReferenceBlocks.Single();
					followingRefBlock.InitialStartVerseNumber = lastVerse.StartVerse;
					followingRefBlock.InitialEndVerseNumber = lastVerse.LastVerseOfBridge;
				}
			}
			return newRefBlock;
		}

		public static void AdvanceToCleanVerseBreak(IReadOnlyList<Block> blockList, Func<int, bool> isOkayToBreakAtVerse, ref int i)
		{
			for (; i < blockList.Count - 1 && (!blockList[i + 1].StartsAtVerseStart || !isOkayToBreakAtVerse(blockList[i + 1].InitialStartVerseNumber)) &&
				!blockList[i + 1].IsChapterAnnouncement; i++)
			{
			}
		}

		public bool IncludesBlock(Block block)
		{
			// TODO: Write tests for the second part of this
			return OriginalBlocks.Contains(block); // || CorrelatedBlocks.Contains(block);
		}

		public void MatchAllBlocks(Paratext.ScrVers versification)
		{
			int bookNum = BCVRef.BookToNumber(m_vernacularBook.BookId);

			foreach (var block in CorrelatedBlocks)
			{
				if (block.MatchesReferenceText)
				{
					if (block.CharacterIsUnclear())
					{
						var refBlock = block.ReferenceBlocks.Single();
						block.SetCharacterAndDeliveryInfo(refBlock, bookNum, versification);
						if (block.CharacterIsUnclear())
							throw new InvalidOperationException("Character cannot be confirmed as ambigous or unknown.");
						block.UserConfirmed = true; // This does not affect original block until Apply is called
					}
				}
				else
				{
					var refBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialStartVerseNumber,
						block.InitialEndVerseNumber);
					refBlock.SetCharacterAndDeliveryInfo(block, bookNum, versification);
					if (block.ReferenceBlocks.Any())
						refBlock.AppendJoinedBlockElements(block.ReferenceBlocks, m_referenceLanguageInfo);
					else
						refBlock.BlockElements.Add(new ScriptText(""));
					block.SetMatchedReferenceBlock(refBlock);
				}
			}
		}

		internal void ChangeAnchor(Block block)
		{
			if (!CorrelatedBlocks.Contains(block))
			{
				return;
				//throw new ArgumentException("Specified block is not in the collection of correlated blocks: " + block, "block");
			}
			CorrelatedAnchorBlock = block;
			Debug.WriteLine("CorrelatedAnchorBlock changed to block " + CorrelatedBlocks.IndexOf(CorrelatedAnchorBlock) + " of " + CorrelatedBlocks.Count);
		}

		/// <summary>
		/// Inserts "he said." (and the equivalent for the primary reference language) into any null/blank reference text.
		/// </summary>
		/// <param name="i">The index of the (correlated) block to be changed, or -1 to do all blocks</param>
		/// <param name="handleHeSaidInserted">Callback to inform caller of any insertions made.</param>
		public void InsertHeSaidText(int i, Action<int, int, string> handleHeSaidInserted)
		{
			if (i == -1)
			{
				for (int iBlock = 0; iBlock < CorrelatedBlocks.Count; iBlock++)
					InsertHeSaidText(m_referenceLanguageInfo, iBlock, handleHeSaidInserted);
			}
			else
				InsertHeSaidText(m_referenceLanguageInfo, i, handleHeSaidInserted);
		}

		private void InsertHeSaidText(IReferenceLanguageInfo referenceLanguageInfo, int i, Action<int, int, string> handleHeSaidInserted, int level = 0)
		{
			if (CorrelatedBlocks[i].CharacterIs(m_vernacularBook.BookId, CharacterVerseData.StandardCharacter.Narrator) ||
				CorrelatedBlocks[i].CharacterId == CharacterVerseData.kUnknownCharacter)
			{
				if (CorrelatedBlocks[i].GetReferenceTextAtDepth(level) == "")
				{
					if (CorrelatedBlocks[i].CharacterId == CharacterVerseData.kUnknownCharacter)
						CorrelatedBlocks[i].CharacterId = CharacterVerseData.GetStandardCharacterId(m_vernacularBook.BookId, CharacterVerseData.StandardCharacter.Narrator);
					var text = referenceLanguageInfo.HeSaidText;
					if (i < CorrelatedBlocks.Count - 1 && !CorrelatedBlocks[i + 1].IsParagraphStart)
						text += referenceLanguageInfo.WordSeparator;
					SetReferenceText(i, text, level);
					handleHeSaidInserted(i, level, text);
				}
				if (referenceLanguageInfo.HasSecondaryReferenceText)
					InsertHeSaidText(referenceLanguageInfo.BackingReferenceLanguage, i, handleHeSaidInserted, level + 1);
			}
		}

		public Block GetCorrespondingOriginalBlock(Block block)
		{
			if (!CorrelatedBlocks.Contains(block))
				return null;
			var i = CorrelatedBlocks.Where(b => b.GetText(true).Contains(block.GetText(true))).IndexOf(block);
			var matches = OriginalBlocks.Where(b => b.GetText(true).Contains(block.GetText(true))).ToList();
			if (matches.Count > i)
				return matches[i];
			Debug.Fail("Properly corresponding match not found in for block " + block.GetText(true));
			return matches.FirstOrDefault();
		}

		public bool CanChangeCharacterAndDeliveryInfo(params int[] blockIndices)
		{
			if (!blockIndices.Any())
				throw new ArgumentException();
			return !blockIndices.Any(i => CorrelatedBlocks[i].CharacterIsStandard);
		}
	}

	public class InvalidReferenceTextException :  Exception
	{
		public InvalidReferenceTextException(Block referenceTextBlock) : base(referenceTextBlock.GetText(true, true))
		{
		}
	}
}
