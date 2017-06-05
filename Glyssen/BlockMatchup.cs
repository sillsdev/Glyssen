using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using SIL.Extensions;
using SIL.Reporting;
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
			Func<VerseRef, bool> isOkayToBreakAtVerse, IReferenceLanguageInfo heSaidProvider, uint predeterminedBlockCount = 0)
		{
			m_vernacularBook = vernacularBook;
			int bookNum = BCVRef.BookToNumber(m_vernacularBook.BookId);
			m_referenceLanguageInfo = heSaidProvider;
			var blocks = m_vernacularBook.GetScriptBlocks();
			var originalAnchorBlock = blocks[iBlock];
			if (predeterminedBlockCount == 0)
			{
				var blocksForVersesCoveredByBlock =
					vernacularBook.GetBlocksForVerse(originalAnchorBlock.ChapterNumber, originalAnchorBlock.InitialStartVerseNumber).ToList();
				var indexOfAnchorBlockInVerse = blocksForVersesCoveredByBlock.IndexOf(originalAnchorBlock);
				if (indexOfAnchorBlockInVerse < 0)
				{
					Logger.WriteEvent($"Anchor block not found in verse: {m_vernacularBook.BookId} {originalAnchorBlock.ChapterNumber}:" +
						$"{originalAnchorBlock.InitialStartVerseNumber} Verse apparently occurs more than once in the Scripture text.");
					// REVIEW: This logic assumes that the repeated verse is wholly contained in this onwe block.
					blocksForVersesCoveredByBlock = new List<Block>() {originalAnchorBlock};
					indexOfAnchorBlockInVerse = 0;
				}
				m_iStartBlock = iBlock - indexOfAnchorBlockInVerse;
				while (m_iStartBlock > 0)
				{
					if (blocksForVersesCoveredByBlock.First().InitialStartVerseNumber < originalAnchorBlock.InitialStartVerseNumber &&
						!blocksForVersesCoveredByBlock.First().StartsAtVerseStart)
					{
						var prepend = vernacularBook.GetBlocksForVerse(originalAnchorBlock.ChapterNumber,
							blocksForVersesCoveredByBlock.First().InitialStartVerseNumber).ToList();
						prepend.RemoveAt(prepend.Count - 1);
						m_iStartBlock -= prepend.Count;
						blocksForVersesCoveredByBlock.InsertRange(0, prepend);
					}
					if (m_iStartBlock == 0 || isOkayToBreakAtVerse(new VerseRef(bookNum, originalAnchorBlock.ChapterNumber,
						blocksForVersesCoveredByBlock.First().InitialStartVerseNumber)))
					{
						break;
					}

					m_iStartBlock--;
					blocksForVersesCoveredByBlock.Insert(0, blocks[m_iStartBlock]);
				}
				int iLastBlock = m_iStartBlock + blocksForVersesCoveredByBlock.Count - 1;
				int i = iLastBlock;
				AdvanceToCleanVerseBreak(blocks,
					verseNum => isOkayToBreakAtVerse(new VerseRef(bookNum, originalAnchorBlock.ChapterNumber, verseNum)),
					ref i);
				if (i > iLastBlock)
					blocksForVersesCoveredByBlock.AddRange(blocks.Skip(iLastBlock + 1).Take(i - iLastBlock));
				while (CharacterVerseData.IsCharacterOfType(blocksForVersesCoveredByBlock.Last().CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical))
					blocksForVersesCoveredByBlock.RemoveAt(blocksForVersesCoveredByBlock.Count - 1);
				m_portion = new PortionScript(vernacularBook.BookId, blocksForVersesCoveredByBlock.Select(b => b.Clone()));

				try
				{
					CorrelatedAnchorBlock = m_portion.GetScriptBlocks()[iBlock - m_iStartBlock];
				}
				catch (Exception ex)
				{
					Logger.WriteEvent(ex.Message);
					Logger.WriteEvent($"iBlock = {iBlock}; m_iStartBlock = {m_iStartBlock}");
					foreach (var block in m_portion.GetScriptBlocks())
						Logger.WriteEvent($"block = {block}");
					throw;
				}
			}
			else
			{
				m_iStartBlock = iBlock;
				m_portion = new PortionScript(vernacularBook.BookId, vernacularBook.GetScriptBlocks().Skip(iBlock).Take((int)predeterminedBlockCount).Select(b => b.Clone()));
				CorrelatedAnchorBlock = m_portion.GetScriptBlocks().First();
			}

			if (splitBlocks != null)
			{
				int origCount = m_portion.GetScriptBlocks().Count;
				splitBlocks(m_portion);
				m_numberOfBlocksAddedBySplitting = m_portion.GetScriptBlocks().Count - origCount;
			}
		}

		public int OriginalBlockCount => CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting;

		public IEnumerable<Block> OriginalBlocks
		{
			get
			{
				return m_vernacularBook.GetScriptBlocks().Skip(m_iStartBlock).Take(OriginalBlockCount);
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

		public bool AllScriptureBlocksMatch => CorrelatedBlocks.All(b => b.MatchesReferenceText);

		public int IndexOfStartBlockInBook
		{
			get { return m_iStartBlock; }
		}

		public Block CorrelatedAnchorBlock { get; private set; }

		public IEnumerable<Tuple<int, int>> GetInvalidReferenceBlocksAtAnyLevel()
		{
			return GetInvalidReferenceBlocksAtAnyLevel(CorrelatedBlocks, 1, m_vernacularBook.BookId);
		}

		private static IEnumerable<Tuple<int, int>> GetInvalidReferenceBlocksAtAnyLevel(IReadOnlyList<Block> blocks, int level, string bookId)
		{
			var refBlocks = new List<Block>();
			for (int i = 0; i < blocks.Count; i++)
			{
				var block = blocks[i];
				if (block.MatchesReferenceText)
				{
					var refBlock = block.ReferenceBlocks.Single();
					if (refBlock.BlockElements.Last() is Verse)
						yield return new Tuple<int, int>(i, level);
					refBlocks.Add(refBlock);
				}
			}

			if (refBlocks.Any(r => r.MatchesReferenceText))
			{
				Debug.Assert(refBlocks.All(r => r.MatchesReferenceText || r.CharacterIs(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical)),
					"All reference blocks should have the same number of levels of underlying reference blocks.");
				foreach (var bogusRefBlock in GetInvalidReferenceBlocksAtAnyLevel(refBlocks, level + 1, bookId))
					yield return bogusRefBlock;
			}
		}

		public void Apply(ScrVers versification)
		{
			if (!AllScriptureBlocksMatch)
				throw new InvalidOperationException("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.");

			//var bogusRefBlock = GetInvalidReferenceBlockAtAnyLevel(CorrelatedBlocks);
			//if (bogusRefBlock != null)
			//	throw new InvalidReferenceTextException(bogusRefBlock);

			if (m_numberOfBlocksAddedBySplitting > 0)
			{
				m_vernacularBook.ReplaceBlocks(m_iStartBlock, CorrelatedBlocks.Count - m_numberOfBlocksAddedBySplitting,
					CorrelatedBlocks.Select(b => b.Clone()).ToList());
			}
			int bookNum = BCVRef.BookToNumber(m_vernacularBook.BookId);
			var origBlocks = m_vernacularBook.GetScriptBlocks();
			for (int i = 0; i < CorrelatedBlocks.Count; i++)
			{
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
			}
			if (m_numberOfBlocksAddedBySplitting == 0)
			{
				var lastBlockInMatchup = CorrelatedBlocks.Last();
				foreach (var block in origBlocks.Skip(m_iStartBlock + OriginalBlockCount).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote))
				{
					block.CharacterId = lastBlockInMatchup.CharacterId;
					block.CharacterIdOverrideForScript = lastBlockInMatchup.CharacterIdOverrideForScript;
				}
			}
			else
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
			bool foundFollowingPrimaryRefTextVerse = false;
			bool foundFollowingSecondaryRefTextVerse = false;
			while (++blockIndex < CorrelatedBlocks.Count)
			{
				var followingBlock = CorrelatedBlocks[blockIndex];
				var followingRefBlock = followingBlock.ReferenceBlocks.FirstOrDefault();
				// Keep going as long as we have a ref block at either level that doesn't
				if (followingRefBlock == null)
					break;
				foundFollowingPrimaryRefTextVerse |= followingRefBlock.StartsAtVerseStart;
				foundFollowingSecondaryRefTextVerse |= !followingRefBlock.ReferenceBlocks.Any() || followingRefBlock.ReferenceBlocks.Single().StartsAtVerseStart;
				if (foundFollowingPrimaryRefTextVerse && foundFollowingSecondaryRefTextVerse)
					break;

				var lastVerse = newRefBlock.LastVerse;
				followingBlock.ReferenceBlocks[0] = followingRefBlock = followingRefBlock.Clone();
				if (!foundFollowingPrimaryRefTextVerse)
				{
					followingRefBlock.InitialStartVerseNumber = lastVerse.StartVerse;
					followingRefBlock.InitialEndVerseNumber = lastVerse.LastVerseOfBridge;
					foundFollowingPrimaryRefTextVerse = followingRefBlock.BlockElements.OfType<Verse>().Any();
				}
				if (!foundFollowingSecondaryRefTextVerse)
				{
					followingRefBlock.CloneReferenceBlocks();
					var secondaryRefBlock = followingRefBlock.ReferenceBlocks.Single();
					secondaryRefBlock.InitialStartVerseNumber = lastVerse.StartVerse;
					secondaryRefBlock.InitialEndVerseNumber = lastVerse.LastVerseOfBridge;
					foundFollowingSecondaryRefTextVerse = secondaryRefBlock.BlockElements.OfType<Verse>().Any();
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
			return OriginalBlocks.Contains(block) || CorrelatedBlocks.Contains(block);
		}

		public void MatchAllBlocks(ScrVers versification)
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
						if (!block.CharacterIsUnclear())
							block.UserConfirmed = true; // This does not affect original block until Apply is called
					}
				}
				else
				{
					block.SetMatchedReferenceBlock(bookNum, versification, m_referenceLanguageInfo);
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
				var existintgEmptyVerseRefText = CorrelatedBlocks[i].GetEmptyVerseReferenceTextAtDepth(level);
				if (existintgEmptyVerseRefText != null)
				{
					if (CorrelatedBlocks[i].CharacterId == CharacterVerseData.kUnknownCharacter)
						CorrelatedBlocks[i].CharacterId = CharacterVerseData.GetStandardCharacterId(m_vernacularBook.BookId, CharacterVerseData.StandardCharacter.Narrator);
					var text = existintgEmptyVerseRefText + referenceLanguageInfo.HeSaidText;
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
			//var matches = OriginalBlocks.Where(b => b.GetText(true).IndexOf(block.GetText(true), StringComparison.Ordinal) >= 0).ToList();
			var matches = OriginalBlocks.Where(b => b.GetText(true).Contains(block.GetText(true))).ToList();
			if (matches.Count > i)
				return matches[i];
			// If we're in the middle of doing a split, there can be a few moments where the block cannot be found. Let's hope
			// this gets called again after the split is complete.
			Debug.WriteLine("Properly corresponding match not found in for block " + block.GetText(true));
			return matches.FirstOrDefault();
		}

		public bool CanChangeCharacterAndDeliveryInfo(params int[] blockIndices)
		{
			if (!blockIndices.Any())
				throw new ArgumentException();
			return !blockIndices.Any(i => CorrelatedBlocks[i].CharacterIsStandard);
		}
	}
}
