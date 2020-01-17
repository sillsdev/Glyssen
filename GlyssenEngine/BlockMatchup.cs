using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Character;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;

namespace GlyssenEngine
{
	public class BlockMatchup
	{
		private readonly BookScript m_vernacularBook;
		private readonly int m_iStartBlock;
		private readonly PortionScript m_portion;
		private int m_numberOfBlocksAddedBySplitting = 0;
		private readonly IReferenceLanguageInfo m_referenceLanguageInfo;
		private ScrVers m_versification;

		public BlockMatchup(BookScript vernacularBook, int iBlock, Action<PortionScript> splitBlocks,
			Func<VerseRef, bool> isOkayToBreakAtVerse, IReferenceLanguageInfo heSaidProvider, uint predeterminedBlockCount = 0)
		{
			m_vernacularBook = vernacularBook;
			int bookNum = m_vernacularBook.BookNumber;
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
					Logger.WriteEvent($"Anchor block not found in verse: {BookId} {originalAnchorBlock.ChapterNumber}:" +
						$"{originalAnchorBlock.InitialStartVerseNumber} Verse apparently occurs more than once in the Scripture text.");
					// REVIEW: This logic assumes that the repeated verse is wholly contained in this one block.
					blocksForVersesCoveredByBlock = new List<Block>() {originalAnchorBlock};
					indexOfAnchorBlockInVerse = 0;
				}
				m_iStartBlock = iBlock - indexOfAnchorBlockInVerse;
				while (m_iStartBlock > 0)
				{
					var firstIncludedBlock = blocksForVersesCoveredByBlock.First();
					if (firstIncludedBlock.InitialStartVerseNumber < originalAnchorBlock.InitialStartVerseNumber &&
						!firstIncludedBlock.StartsAtVerseStart && !firstIncludedBlock.IsChapterAnnouncement)
					{
						var prepend = vernacularBook.GetBlocksForVerse(originalAnchorBlock.ChapterNumber,
							firstIncludedBlock.InitialStartVerseNumber).ToList();
						if (prepend.Count > 1)
						{
							prepend.RemoveAt(prepend.Count - 1);
							m_iStartBlock -= prepend.Count;
							blocksForVersesCoveredByBlock.InsertRange(0, prepend);
						}
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
				m_portion = new PortionScript(vernacularBook, blocksForVersesCoveredByBlock.Select(b => b.Clone()));

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
				m_portion = new PortionScript(vernacularBook, vernacularBook.GetScriptBlocks().Skip(iBlock).Take((int)predeterminedBlockCount).Select(b => b.Clone()));
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

		public string BookId => m_vernacularBook.BookId;

		public IEnumerable<Block> OriginalBlocks => m_vernacularBook.GetScriptBlocks().Skip(m_iStartBlock).Take(OriginalBlockCount);

		public int CountOfBlocksAddedBySplitting => m_numberOfBlocksAddedBySplitting;

		public IReadOnlyList<Block> CorrelatedBlocks => m_portion.GetScriptBlocks();

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
						realBlock.GetPrimaryReferenceText() != correlatedBlock.GetPrimaryReferenceText() ||
						!realBlock.ReferenceBlocks.Select(r => r.GetPrimaryReferenceText()).SequenceEqual(correlatedBlock.ReferenceBlocks.Select(r => r.GetPrimaryReferenceText())))
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
			return GetInvalidReferenceBlocksAtAnyLevel(CorrelatedBlocks, 1, BookId);
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

		public void Apply(ScrVers versification = null)
		{
			if (!AllScriptureBlocksMatch)
				throw new InvalidOperationException("Cannot apply reference blocks unless all Scripture blocks have corresponding reference blocks.");

			if (versification != null)
			{
				if (m_versification != null && m_versification != versification)
					throw new ArgumentException("Apply called with unexpected versification!", nameof(versification));
				m_versification = versification;
			}

			if (m_numberOfBlocksAddedBySplitting > 0)
			{
				m_vernacularBook.ReplaceBlocks(m_iStartBlock, OriginalBlockCount, CorrelatedBlocks.Select(b => b.Clone()).ToList());
			}
			int bookNum = BCVRef.BookToNumber(BookId);
			var origBlocks = m_vernacularBook.GetScriptBlocks();
			for (int i = 0; i < CorrelatedBlocks.Count; i++)
			{
				var vernBlock = origBlocks[m_iStartBlock + i];

				var refBlock = CorrelatedBlocks[i].ReferenceBlocks.Single();
				vernBlock.SetMatchedReferenceBlock(refBlock);
				var basedOnBlock = CorrelatedBlocks[i].CharacterIsUnclear ? refBlock : CorrelatedBlocks[i];
				vernBlock.SetCharacterAndDeliveryInfo(basedOnBlock, bookNum, m_versification);
				if (vernBlock.CharacterIsStandard)
					vernBlock.MultiBlockQuote = MultiBlockQuote.None;

				if (vernBlock.CharacterIsUnclear)
					throw new InvalidOperationException("Vernacular block matched to reference block must have a CharacterId that is not ambiguous or unknown.");

				if (CorrelatedBlocks[i].UserConfirmed)
					vernBlock.UserConfirmed = true;
				vernBlock.SplitId = CorrelatedBlocks[i].SplitId;
			}
			// No need to update following continuation blocks here if m_numberOfBlocksAddedBySplitting > 0 because the call to
			// ReplaceBlocks (above) already did it.
			if (m_numberOfBlocksAddedBySplitting == 0)
				m_vernacularBook.UpdateFollowingContinuationBlocks(m_iStartBlock + OriginalBlockCount - 1);
			else
				m_numberOfBlocksAddedBySplitting = 0;

			Debug.Assert(origBlocks.Skip(m_iStartBlock).Take(CorrelatedBlocks.Count)
				.All(b => !b.CharacterIsStandard || b.MultiBlockQuote == MultiBlockQuote.None),
				"Applying block matchup resulted in an illegal multi-block quote chain.");
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
			var prevRefBlock = (blockIndex > 0) ? CorrelatedBlocks[blockIndex - 1].ReferenceBlocks.LastOrDefault() : null;
			for (int i = 0; i < level && prevRefBlock != null; i++)
				prevRefBlock = prevRefBlock.ReferenceBlocks.LastOrDefault();
			var newRefBlock = block.SetMatchedReferenceBlock(text, prevRefBlock);
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
			int initialValue = i;
			for (; i < blockList.Count - 1 && (!blockList[i + 1].StartsAtVerseStart || !isOkayToBreakAtVerse(blockList[i + 1].InitialStartVerseNumber)) &&
				!blockList[i + 1].IsChapterAnnouncement; i++)
			{
			}
			// We don't want to include a TRAILING section head.
			while (CharacterVerseData.IsCharacterOfType(blockList[i].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical) && i > initialValue)
				i--;
		}

		public bool IncludesBlock(Block block)
		{
			return OriginalBlocks.Contains(block) || CorrelatedBlocks.Contains(block);
		}

		public void MatchAllBlocks(ScrVers versification)
		{
			m_versification = versification;
			int bookNum = BCVRef.BookToNumber(BookId);
			Block prevBlock = null;
			foreach (var block in CorrelatedBlocks)
			{
				if (block.MatchesReferenceText)
				{
					if (block.CharacterIsUnclear)
					{
						var refBlock = block.ReferenceBlocks.Single();
						block.SetCharacterAndDeliveryInfo(refBlock, bookNum, versification);
						if (!block.CharacterIsUnclear)
						{
							block.UserConfirmed = true; // This does not affect original block until Apply is called
						}
					}
				}
				else
				{
					block.SetMatchedReferenceBlock(bookNum, versification, m_referenceLanguageInfo);
					if (block.CharacterIsUnclear)
						block.SetCharacterAndDeliveryInfo(block.ReferenceBlocks.Single(), bookNum, m_versification);
				}

				if (block.CharacterIsStandard && block.MultiBlockQuote != MultiBlockQuote.None)
				{
					block.MultiBlockQuote = MultiBlockQuote.None;
					if (prevBlock?.MultiBlockQuote == MultiBlockQuote.Start)
						prevBlock.MultiBlockQuote = MultiBlockQuote.None;
				}

				prevBlock = block;
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
			var block = CorrelatedBlocks[i];
			if (block.CharacterIs(BookId, CharacterVerseData.StandardCharacter.Narrator) ||
				block.CharacterIsUnclear)
			{
				var existingEmptyVerseRefText = block.GetEmptyVerseReferenceTextAtDepth(level);
				if (existingEmptyVerseRefText != null)
				{
					var narrator = CharacterVerseData.GetStandardCharacterId(BookId, CharacterVerseData.StandardCharacter.Narrator);
					if (block.CharacterIsUnclear)
						block.SetNonDramaticCharacterId(narrator);
					// Deal with following blocks in quote block chain (and mark this one None).
					if (block.MultiBlockQuote == MultiBlockQuote.Start)
					{
						for (int iCont = i + 1; iCont < CorrelatedBlocks.Count; iCont++)
						{
							var contBlock = CorrelatedBlocks[iCont];
							if (contBlock.MultiBlockQuote != MultiBlockQuote.Continuation)
								break;
							contBlock.MultiBlockQuote = MultiBlockQuote.None;
							// It's probably impossible in practice, but if this block has a character other than narrator already set,
							// let's leave it as is. And, of course, if it's already explicitly set to narrator, then there's nothing to
							// do.
							if (contBlock.CharacterIsUnclear)
							{
								// By far the common case will be that this block will be associated with a reference
								// block that has a real character ID. If so, we'll use that ID, even if it's not
								// narrator because it's better not to have weird UI changes (that might be scrolled off
								// the screen) that the user won't be able to account for. But in the unusual case where
								// the vernacular block was unknown/ambiguous and it didn't align to a real ref block (so
								// we just made up an empty one on the fly), it's probably best to go ahead and assume
								// that any such continuation blocks are to be assigned to the narrator. In that case,
								// we need to fire the handler to alert the client.
								var dataChange = false;
								var newCharacterId = contBlock.ReferenceBlocks.SingleOrDefault()?.CharacterId;
								if (CharacterVerseData.IsCharacterUnclear(newCharacterId))
								{
									newCharacterId = narrator;
									dataChange = true;
								}
								contBlock.SetNonDramaticCharacterId(newCharacterId);
								if (dataChange)
									handleHeSaidInserted(iCont, level, null);
							}
						}
						block.MultiBlockQuote = MultiBlockQuote.None;
					}
					var text = existingEmptyVerseRefText + referenceLanguageInfo.HeSaidText;
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
