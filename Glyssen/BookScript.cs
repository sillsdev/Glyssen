using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Quote;
using Glyssen.Shared;
using SIL.Extensions;
using SIL.Scripture;
using SIL.Unicode;
using static System.Char;
using static System.String;

namespace Glyssen
{
	[XmlRoot("book")]
	public class BookScript : PortionScript, IScrBook
	{
		private Dictionary<int, int> m_chapterStartBlockIndices;
		private List<List<Block>> m_unappliedSplitBlocks = new List<List<Block>>();
		private ScrStylesheetAdapter m_styleSheet;
		private int m_blockCount;

		private BookScript() : base(null, null)
		{
			// Needed for deserialization
		}

		public BookScript(string bookId, IEnumerable<Block> blocks) : base(bookId, blocks)
		{
			OnBlocksReset();
			BookNumber = BCVRef.BookToNumber(bookId);
		}

		[XmlAttribute("id")]
		public string BookId
		{
			get { return Id; }
			set
			{
				m_id = value;
				BookNumber = BCVRef.BookToNumber(m_id);
			}
		}

		[XmlIgnore]
		public int BookNumber { get; private set; }

		[XmlAttribute("singlevoice")]
		public bool SingleVoice { get; set; }

		[XmlAttribute("pageheader")]
		public string PageHeader { get; set; }

		[XmlAttribute("maintitle")]
		public string MainTitle { get; set; }

		[XmlAttribute("checkstatusoverridden")]
		public bool CheckStatusOverridden { get; set; }

		[XmlAttribute("ptchecksum")]
		public string ParatextChecksum { get; set; }

		/// <summary>
		/// Don't use this getter in production code. It is intended ONLY for use by the XML serializer!
		/// This is to prevent accidentally leaking the actual list and risking modification by calling code.
		/// </summary>
		[XmlElement(ElementName = "block")]
		public List<Block> Blocks
		{
			get { return m_blocks; }
			set
			{
				m_blocks = value;
				OnBlocksReset();
			}
		}

		public Block this[int i]
		{
			get { return m_blocks[i]; }
		}

		public bool HasScriptBlocks
		{
			get { return m_blocks.Any(); }
		}

		/// <summary>
		/// Don't use this getter in production code. It is intended ONLY for use by the XML serializer!
		/// This is to prevent accidentally leaking the actual list and risking modification by calling code.
		/// </summary>
		[XmlArray("UnappliedSplits")]
		[XmlArrayItem("Split")]
		public List<List<Block>> UnappliedBlockSplits_DoNotUse
		{
			get { return m_unappliedSplitBlocks; }
			set { m_unappliedSplitBlocks = value; }
		}

		public System.Collections.Generic.IReadOnlyList<IEnumerable<Block>> UnappliedSplits
		{
			get { return m_unappliedSplitBlocks; }
		}

		public BookScript Clone(bool join)
		{
			BookScript newBook = (BookScript) MemberwiseClone();
			newBook.Blocks = new List<Block>(GetScriptBlocks(join).Select(b => b.Clone()));
			newBook.m_unappliedSplitBlocks = new List<List<Block>>(m_unappliedSplitBlocks.Select(l => l.Select(b => b.Clone()).ToList()));
			return newBook;
		}

		public override System.Collections.Generic.IReadOnlyList<Block> GetScriptBlocks()
		{
			EnsureBlockCount();
			return base.GetScriptBlocks();
		}

		public System.Collections.Generic.IReadOnlyList<Block> GetScriptBlocks(bool join)
		{
			if (!join)
				return GetScriptBlocks();

			EnsureBlockCount();

			if (!join || m_blockCount == 0)
				return m_blocks;

			var list = new List<Block>(m_blockCount);

			if (SingleVoice)
			{
				list.Add(m_blocks[0].Clone());
				var prevBlock = list.Single();
				prevBlock.MatchesReferenceText = false;
				var narrator = CharacterVerseData.GetStandardCharacterId(BookId, CharacterVerseData.StandardCharacter.Narrator);
				for (var i = 1; i < m_blockCount; i++)
				{
					var clonedBlock = m_blocks[i].Clone();
					clonedBlock.MatchesReferenceText = false;
					if (!clonedBlock.CharacterIsStandard)
						clonedBlock.CharacterId = narrator;

					if (!clonedBlock.IsParagraphStart || (clonedBlock.IsFollowOnParagraphStyle && !CharacterUtils.EndsWithSentenceFinalPunctuation(prevBlock.GetText(false)))) // && clonedBlock.CharacterId == prevBlock.CharacterId)
						prevBlock.CombineWith(clonedBlock);
					else
					{
						list.Add(clonedBlock);
						prevBlock = clonedBlock;
					}
				}
			}
			else
			{
				list.Add(m_blocks[0]);
				if (m_styleSheet == null)
					m_styleSheet = SfmLoader.GetUsfmStylesheet();

				for (var i = 1; i < m_blockCount; i++)
				{
					var block = m_blocks[i];
					var prevBlock = list.Last();

					if (block.MatchesReferenceText == prevBlock.MatchesReferenceText &&
						block.CharacterIdInScript == prevBlock.CharacterIdInScript && (block.Delivery ?? Empty) == (prevBlock.Delivery ?? Empty))
					{
						bool combine = false;
						if (block.MatchesReferenceText)
						{
							combine = block.ReferenceBlocks.Single().StartsWithEllipsis ||
							((!block.IsParagraphStart || (block.IsFollowOnParagraphStyle && !CharacterUtils.EndsWithSentenceFinalPunctuation(prevBlock.GetText(false)))) &&
								!block.ContainsVerseNumber &&
								((!block.ReferenceBlocks.Single().BlockElements.OfType<Verse>().Any() &&
										!CharacterUtils.EndsWithSentenceFinalPunctuation(prevBlock.GetText(false))) ||
									block.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().All(t => t.Content.All(IsWhiteSpace)) ||
									prevBlock.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().All(t => t.Content.All(IsWhiteSpace))));
						}
						else if (!block.StartsAtVerseStart)
						{
							var style = (StyleAdapter)m_styleSheet.GetStyle(block.StyleTag);
							combine = !block.IsParagraphStart || (style.IsPoetic && !CharacterUtils.EndsWithSentenceFinalPunctuation(prevBlock.GetText(false)));
						}
						if (combine)
						{
							list[list.Count - 1] = Block.CombineBlocks(prevBlock, block);
							continue;
						}
					}
					list.Add(block);
				}
			}
			return list;
		}

		public string GetVerseText(int chapter, int verse)
		{
			var iFirstBlockToExamine = GetIndexOfFirstBlockForVerse(chapter, verse);
			if (iFirstBlockToExamine < 0)
				return Empty;
			StringBuilder bldr = new StringBuilder();
			bool foundVerseStart = false;
			for (int index = iFirstBlockToExamine; index < m_blockCount; index++)
			{
				var block = m_blocks[index];
				if (block.ChapterNumber != chapter)
					break;
				foreach (var element in block.BlockElements)
				{
					Verse verseElement = element as Verse;
					if (verseElement != null)
					{
						var endVerse = verseElement.EndVerse;
						if (verse > endVerse)
							continue;
						if (verse >= verseElement.StartVerse && verse <= endVerse)
							foundVerseStart = true;
						else if (foundVerseStart)
							return bldr.ToString();
					}
					else if (foundVerseStart)
					{
						if (index > iFirstBlockToExamine)
							bldr.Append(Environment.NewLine);
						var textElement = (ScriptText) element;
						bldr.Append(textElement.Content);
					}
				}
			}
			return bldr.ToString();
		}

		private void OnBlocksReset()
		{
			m_chapterStartBlockIndices = new Dictionary<int, int>();
			m_blockCount = m_blocks.Count;
		}

		protected override void OnBlocksInserted(int insertionIndex, int countOfInsertedBlocks = 1)
		{
			base.OnBlocksInserted(insertionIndex);
			Debug.Assert(insertionIndex > 0);
			var chapterNumbersToIncrement = m_chapterStartBlockIndices.Keys.Where(chapterNum =>
				chapterNum > m_blocks[insertionIndex - 1].ChapterNumber).ToList();
			foreach (var chapterNum in  chapterNumbersToIncrement)
				m_chapterStartBlockIndices[chapterNum] += countOfInsertedBlocks;

			m_blockCount += countOfInsertedBlocks;
		}

		public IEnumerable<Block> GetBlocksForVerse(int chapter, int startVerse, int endVerse = -1)
		{
			if (endVerse == -1)
				endVerse = startVerse;
			var iFirstBlockToExamine = GetIndexOfFirstBlockForVerse(chapter, startVerse);
			if (iFirstBlockToExamine >= 0)
			{
				for (int index = iFirstBlockToExamine; index < m_blockCount; index++)
				{
					var block = m_blocks[index];
					if (block.ChapterNumber != chapter)
						break;
					if (block.InitialStartVerseNumber <= endVerse && block.LastVerseNum >= startVerse)
						yield return block;
					else
						break;
				}
			}
		}

		public Block GetFirstBlockForVerse(int chapter, int verse)
		{
			var iFirstBlockToExamine = GetIndexOfFirstBlockForVerse(chapter, verse);
			if (iFirstBlockToExamine < 0)
				return null;

			var block = m_blocks[iFirstBlockToExamine];
			foreach (var verseElement in block.BlockElements.OfType<Verse>().SkipWhile(v => verse > v.EndVerse))
			{
				if (verse >= verseElement.StartVerse && verse <= verseElement.EndVerse)
					return block;
				break;
			}
			return null;
		}

		public int GetIndexOfFirstBlockForVerse(int chapter, int verse)
		{
			EnsureBlockCount();
			if (m_blockCount == 0)
				return -1;
			int chapterStartBlock;
			bool chapterStartFound = m_chapterStartBlockIndices.TryGetValue(chapter, out chapterStartBlock);

			if (!chapterStartFound && m_chapterStartBlockIndices.Any())
			{
				int fallBackChapter = chapter;
				while (fallBackChapter > 1)
				{
					if (m_chapterStartBlockIndices.TryGetValue(--fallBackChapter, out chapterStartBlock))
						break;
				}
			}
			int iFirstBlockToExamine = -1;
			for (int index = chapterStartBlock; index < m_blockCount; index++)
			{
				var block = m_blocks[index];
				if (block.ChapterNumber < chapter)
					continue;
				if (block.ChapterNumber > chapter)
				{
					if (chapterStartFound)
						iFirstBlockToExamine = index - 1;
					break;
				}
				if (!chapterStartFound)
				{
					m_chapterStartBlockIndices[chapter] = index;
					chapterStartFound = true;
				}
				if (block.InitialStartVerseNumber < verse && block.InitialEndVerseNumber < verse)
					continue;
				iFirstBlockToExamine = index;
				if (block.InitialStartVerseNumber > verse ||
					(iFirstBlockToExamine > 0 && !(block.BlockElements.First() is Verse) && m_blocks[iFirstBlockToExamine - 1].LastVerseNum == verse))
				{
					iFirstBlockToExamine--;
				}
				break;
			}

			if (iFirstBlockToExamine < 0)
			{
				if (!chapterStartFound)
					return -1;
				iFirstBlockToExamine = m_blockCount - 1;
			}
			return iFirstBlockToExamine;
		}

		private int GetIndexOfFirstBlockThatStartsWithVerse(int chapter, int verse)
		{
			var i = GetIndexOfFirstBlockForVerse(chapter, verse);
			while (i < m_blocks.Count && m_blocks[i].InitialStartVerseNumber < verse)
				i++;
			return i < m_blocks.Count ? i : -1;
		}

		/// <summary>
		/// Admittedly, this isn't the best way to prevent changes, but it is easier than doing custom
		/// serialization or trying to encapsulate the class to allow XML serialization but not expose
		/// the Blocks getter.
		/// </summary>
		private void EnsureBlockCount()
		{
			if (m_blockCount == 0)
				m_blockCount = m_blocks.Count;
			else if (m_blockCount != m_blocks.Count)
				throw new InvalidOperationException(
					"Blocks collection changed. Blocks getter should not be used to add or remove blocks to the list. Use setter instead.");
		}

		public void ApplyUserDecisions(BookScript sourceBookScript, ScrVers versification = null, ReferenceText referenceTextToReapply = null)
		{
			var blockComparer = new SplitBlockComparer();

			foreach (var sourceUnappliedSplit in sourceBookScript.UnappliedSplits)
			{
				List<Block> targetUnappliedSplit = sourceUnappliedSplit.Select(splitPart => splitPart.Clone()).ToList();
				m_unappliedSplitBlocks.Add(targetUnappliedSplit);
			}

			ApplyUserSplits(sourceBookScript, blockComparer);
			if (referenceTextToReapply != null)
				ApplyReferenceBlockMatches(sourceBookScript, versification, referenceTextToReapply, blockComparer);
			ApplyUserAssignments(sourceBookScript, versification);
			CleanUpMultiBlockQuotes(versification);
		}

		private void ApplyReferenceBlockMatches(BookScript sourceBookScript, ScrVers versification,
			ReferenceText referenceTextToReapply, SplitBlockComparer blockComparer)
		{
			//var clone = Clone(true);
			//referenceTextToReapply.ApplyTo(clone, versification);
			//var clonedBlocks = clone.GetScriptBlocks(false);
			var sourceBlocks = sourceBookScript.GetScriptBlocks(false);
			var iPrevFirstTargetBlock = 0;
			for (int iSrc = 0; iSrc < sourceBlocks.Count; iSrc++)
			{
				var sourceBlock = sourceBlocks[iSrc];
				if (!sourceBlock.MatchesReferenceText)
					continue;
				int iTargetBlock = GetIndexOfFirstBlockForVerse(sourceBlock.ChapterNumber, sourceBlock.InitialStartVerseNumber);
				if (iTargetBlock < iPrevFirstTargetBlock)
					continue;
				iPrevFirstTargetBlock = iTargetBlock;
				var targetMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(this, iTargetBlock,
					versification);
				if (targetMatchup.CorrelatedBlocks[0].InitialStartVerseNumber < m_blocks[iTargetBlock].InitialStartVerseNumber)
					continue; // Oops, we ended up going backwards into the target 
				//var lastTargetBlock = targetMatchup.CorrelatedBlocks.Last();
				//var lastVerseOfTargetMatchup = lastTargetBlock.LastVerse.EndVerse;
				//var cSourceBlocksToMatchup = sourceBlocks.Skip(iSrc).Count(sb => sb.ChapterNumber == lastTargetBlock.ChapterNumber &&
				//	sb.InitialStartVerseNumber <= lastVerseOfTargetMatchup || sb.LastVerseNum < lastVerseOfTargetMatchup);
				var sourceMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(sourceBookScript, iSrc,
					versification, (uint)targetMatchup.CorrelatedBlocks.Count, false);
				Debug.Assert(sourceMatchup.CountOfBlocksAddedBySplitting == 0);
				iSrc += sourceMatchup.OriginalBlockCount - 1; // Need to subtract 1 because this gets incremented in for loop.
				if (sourceMatchup.CorrelatedBlocks.Count != targetMatchup.CorrelatedBlocks.Count)
				{
					Debug.Fail("Source matchup did not get the requested number of blocks. Did we run off the end of the source?");
					continue;
				}
				bool everythingMatched = true;
				for (int i = 0; i < sourceMatchup.CorrelatedBlocks.Count && everythingMatched; i++)
				{
					sourceBlock = sourceMatchup.CorrelatedBlocks[i];
					var targetBlock = targetMatchup.CorrelatedBlocks[i];
					if (sourceBlock.MatchesReferenceText && blockComparer.Equals(sourceBlock, targetBlock))
					{
						if (targetBlock.IsContinuationOfPreviousBlockQuote)
						{
							Block prevBlock;
							if (i > 0)
							{
								prevBlock = targetMatchup.CorrelatedBlocks[i - 1];
							}
							else
							{
								Debug.Assert(targetMatchup.IndexOfStartBlockInBook > 0);
								prevBlock = m_blocks[targetMatchup.IndexOfStartBlockInBook - 1];
							}
							if (prevBlock.CharacterId != sourceBlock.CharacterId)
							{
								everythingMatched = false;
								break;
							}
						}
						targetBlock.SetMatchedReferenceBlock(sourceBlock.ReferenceBlocks.Single());
						targetBlock.CloneReferenceBlocks();
						targetBlock.SetCharacterAndDeliveryInfo(sourceBlock, BookNumber, versification);
						targetBlock.SplitId = sourceBlock.SplitId;
						targetBlock.UserConfirmed = sourceBlock.UserConfirmed;
					}
					else
						everythingMatched = false;
				}
				if (everythingMatched)
					targetMatchup.Apply(versification);
			}

			//foreach (var sourceBlock in sourceBlocks.Where(b => b.MatchesReferenceText))
			//{
			//	int iClonedBlock = clone.GetIndexOfFirstBlockForVerse(sourceBlock.ChapterNumber, sourceBlock.InitialStartVerseNumber);
			//	if (iClonedBlock < 0)
			//		continue;
			//	var clonedBlock = clonedBlocks[iClonedBlock];
			//	do
			//	{
			//		if (blockComparer.Equals(clonedBlock, sourceBlock))
			//		{
			//			clonedBlock.MatchesReferenceText = true;
			//			clonedBlock.ReferenceBlocks = new List<Block> { sourceBlock.ReferenceBlocks.Single().Clone(true) };
			//		}
			//	} while ();
		}

		private void ApplyUserAssignments(BookScript sourceBookScript, ScrVers versification)
		{
			var comparer = new BlockElementContentsComparer();
			int iTarget = 0;
			var bookNum = BCVRef.BookToNumber(sourceBookScript.BookId);
			foreach (var sourceBlock in sourceBookScript.m_blocks.Where(b => b.UserConfirmed))
			{
				if (iTarget == m_blocks.Count)
					return;

				if (m_blocks[iTarget].ChapterNumber < sourceBlock.ChapterNumber)
					iTarget = GetIndexOfFirstBlockForVerse(sourceBlock.ChapterNumber, sourceBlock.InitialStartVerseNumber);
				else
				{
					while (m_blocks[iTarget].InitialStartVerseNumber < sourceBlock.InitialStartVerseNumber)
					{
						iTarget++;
						if (iTarget == m_blocks.Count)
							return;
					}
				}
				do
				{
					if (m_blocks[iTarget].StyleTag == sourceBlock.StyleTag &&
						m_blocks[iTarget].IsParagraphStart == sourceBlock.IsParagraphStart &&
						m_blocks[iTarget].BlockElements.SequenceEqual(sourceBlock.BlockElements, comparer))
					{
						if (sourceBlock.CharacterIdOverrideForScript == null)
							m_blocks[iTarget].SetCharacterIdAndCharacterIdInScript(sourceBlock.CharacterId, bookNum, versification);
						else
						{
							m_blocks[iTarget].CharacterId = sourceBlock.CharacterId;
							m_blocks[iTarget].CharacterIdOverrideForScript = sourceBlock.CharacterIdOverrideForScript;
						}
						m_blocks[iTarget].Delivery = sourceBlock.Delivery;
						if (sourceBlock.MatchesReferenceText && !m_blocks[iTarget].MatchesReferenceText)
						{
							m_blocks[iTarget].SetMatchedReferenceBlock(sourceBlock.ReferenceBlocks.Single());
							m_blocks[iTarget].CloneReferenceBlocks();
						}
						m_blocks[iTarget].UserConfirmed = true;
						iTarget++;
						if (iTarget == m_blocks.Count)
							return;
						break;
					}
				} while (++iTarget < m_blocks.Count &&
					m_blocks[iTarget].ChapterNumber == sourceBlock.ChapterNumber &&
					m_blocks[iTarget].InitialStartVerseNumber == sourceBlock.InitialStartVerseNumber);
			}
		}

		private void ApplyUserSplits(BookScript sourceBookScript, IEqualityComparer<Block> blockComparer)
		{
			int splitId = Block.kNotSplit;
			List<Block> split = null;
			foreach (var block in sourceBookScript.Blocks.Where(b => b.SplitId != Block.kNotSplit))
			{
				if (block.SplitId != splitId)
				{
					if (split != null)
						m_unappliedSplitBlocks.Add(split);
					split = new List<Block>();
					splitId = block.SplitId;
				}
				split.Add(block);
			}
			if (split != null)
				m_unappliedSplitBlocks.Add(split);

			var elementComparer = new BlockElementContentsComparer();

			for (int index = 0; index < m_unappliedSplitBlocks.Count; index++)
			{
				var unappliedSplit = m_unappliedSplitBlocks[index];
				var firstBlockOfSplit = unappliedSplit.First();
				var i = GetIndexOfFirstBlockThatStartsWithVerse(firstBlockOfSplit.ChapterNumber, firstBlockOfSplit.InitialStartVerseNumber);
				if (i < 0)
				{
					// The parse was different enough that we can't find a block that starts with that verse number at all.
					if (ApplySplitAgainstUnchunkedBlock(unappliedSplit, elementComparer))
						m_unappliedSplitBlocks.RemoveAt(index--);
					continue;
				}
				var iFirstMatchingBlock = i;
				var iUnapplied = 0;
				bool blocksMatch;
				do
				{
					var splitBlock = unappliedSplit[iUnapplied];
					var parsedBlock = m_blocks[i++];
					blocksMatch = blockComparer.Equals(splitBlock, parsedBlock);
					if (iUnapplied > 0 || blocksMatch)
					{
						if (!blocksMatch)
							break;
						if (iUnapplied == 0)
							iFirstMatchingBlock = i;
						iUnapplied++;
					}
				} while (i < m_blocks.Count && iUnapplied < unappliedSplit.Count);
				if (blocksMatch ||
					ApplySplitAgainstCombinedBlocks(unappliedSplit, iFirstMatchingBlock, blockComparer) ||
					ApplySplitAgainstUnchunkedBlock(unappliedSplit, elementComparer))
				{
					m_unappliedSplitBlocks.RemoveAt(index--);
				}
			}
		}

		private bool ApplySplitAgainstCombinedBlocks(List<Block> unappliedSplit, int iFirstMatchingBlock, IEqualityComparer<Block> comparer)
		{
			var combinedBlock = CombineBlocks(unappliedSplit);
			for (int iBlock = iFirstMatchingBlock; iBlock < m_blocks.Count && m_blocks[iBlock].InitialStartVerseNumber == combinedBlock.InitialStartVerseNumber; iBlock++)
			{
				if (comparer.Equals(combinedBlock, m_blocks[iBlock]))
				{
					var i = iBlock;
					int iUnapplied;
					for (iUnapplied = 1; iUnapplied < unappliedSplit.Count; iUnapplied++)
					{
						var elementsOfBlockPrecedingSplit = unappliedSplit[iUnapplied - 1].BlockElements;
						var textElementAtEndOfBlockPrecedingSplit = elementsOfBlockPrecedingSplit.Last() as ScriptText;
						int offset = textElementAtEndOfBlockPrecedingSplit?.Content.Length ?? 0;
						string verse;
						if (unappliedSplit[iUnapplied].BlockElements.First() is Verse)
						{
							var lastVerseInPrecedingBlock = elementsOfBlockPrecedingSplit.OfType<Verse>().LastOrDefault();
							if (lastVerseInPrecedingBlock != null)
								verse = lastVerseInPrecedingBlock.Number;
							else
								verse = m_blocks[i].InitialVerseNumberOrBridge;
						}
						else
						{
							verse = unappliedSplit[iUnapplied].InitialVerseNumberOrBridge;
						}
						SplitBlock(m_blocks[i++], verse, offset);
						if (unappliedSplit[iUnapplied - 1].MatchesReferenceText)
							m_blocks[i - 1].SetMatchedReferenceBlock(unappliedSplit[iUnapplied - 1].ReferenceBlocks.Single().Clone());
					}
					if (unappliedSplit[iUnapplied - 1].MatchesReferenceText)
						m_blocks[i].SetMatchedReferenceBlock(unappliedSplit[iUnapplied - 1].ReferenceBlocks.Single().Clone());

					return true;
				}
			}
			return false;
		}

		private bool ApplySplitAgainstUnchunkedBlock(List<Block> unappliedSplit, IEqualityComparer<BlockElement> comparer)
		{
			var firstBlockOfSplit = unappliedSplit.First();
			var combinedBlockElements = CombineBlocks(unappliedSplit).BlockElements;

			var firstVerseOfSplit = firstBlockOfSplit.BlockElements.First() as Verse;
			if (firstVerseOfSplit == null)
				return false;

			// Very likely, the split was done on a block that was part of a larger parsed block that was chunked
			// up according to the reference text, though it may also have been split manually. If we can find that
			// larger block with matching verse text on either side of the splits, we can still apply them (though
			// we won't attempt to fully or partially connect it up with the reference text).
			var blockToSplit = GetFirstBlockForVerse(firstBlockOfSplit.ChapterNumber, firstBlockOfSplit.InitialStartVerseNumber);
			var indexOfFirstCorrespondingElement = blockToSplit.BlockElements.IndexOf(firstVerseOfSplit, comparer);
			var indexOfLastCorrespondingElement = indexOfFirstCorrespondingElement + combinedBlockElements.Count - 1;
			if (indexOfLastCorrespondingElement >= blockToSplit.BlockElements.Count)
				return false;
			var textOfLastVerseInBlockToSplit = ((ScriptText)blockToSplit.BlockElements[indexOfLastCorrespondingElement]).Content;
			if (!combinedBlockElements.Take(combinedBlockElements.Count - 1).SequenceEqual(
					blockToSplit.BlockElements.Skip(indexOfFirstCorrespondingElement).Take(combinedBlockElements.Count - 1), comparer) ||
				!textOfLastVerseInBlockToSplit.StartsWith(((ScriptText)combinedBlockElements.Last()).Content))
			{
				return false;
			}

			// The relevant block elements match. We can re-apply the split.

			//if (indexOfFirstCorrespondingElement > 0 && firstBlockOfSplit.UserConfirmed)
			//{
			//	// The first block of the "split" is a special case because when Glyssen chunked up the text
			//	// to match the reference text, it made a temporary split right before this block, but since
			//	// it is UserConfirmed, it needs to be regarded as having been done by the user so that
			//	// reference text alignment and character assignments can be re-applied in ApplyUserAssignments.
			//	var prevVerse = blockToSplit.BlockElements.Take(indexOfFirstCorrespondingElement).OfType<Verse>().LastOrDefault()?.Number;
			//	if (prevVerse != null)
			//		blockToSplit = SplitBlock(blockToSplit, prevVerse, kSplitAtEndOfVerse);
			//}

			foreach (var currentSplit in unappliedSplit)
			{
				var offset = currentSplit.BlockElements.OfType<ScriptText>().First().Content.Length;
				//	||
				//(currentSplit.MultiBlockQuote == MultiBlockQuote.Start && currentSplit == unappliedSplit.Last())
				if ((!currentSplit.UserConfirmed)
					&& offset == blockToSplit.BlockElements.OfType<ScriptText>().First().Content.Length)
				{
					// This is not really a user-split. Glyssen will redo this split as needed when re-aligning to reference text.
					continue;
				}
				try
				{
					blockToSplit = SplitBlock(blockToSplit, currentSplit.InitialVerseNumberOrBridge, offset);
				}
				catch
				{
					// Probably an attempt (unnecessarily) to split between two verses that are already in separate blocks.
					return offset == blockToSplit.BlockElements.OfType<ScriptText>().First().Content.Length &&
						currentSplit == unappliedSplit.Last();
				}
			}
			return true;
		}

		public void CleanUpMultiBlockQuotes(ScrVers versification)
		{
			var model = new BlockNavigatorViewModel(new[] { this }.ToReadOnlyList(), versification);
			foreach (IEnumerable<Block> multiBlock in GetScriptBlocks()
				.Where(b => b.MultiBlockQuote == MultiBlockQuote.Start)
				.Select(block => model.GetAllBlocksWhichContinueTheQuoteStartedByBlock(block)))
			{
				ProcessAssignmentForMultiBlockQuote(BCVRef.BookToNumber(BookId), multiBlock.ToList(), versification);
			}
		}

		private Block CombineBlocks(List<Block> blocks)
		{
			Block combinedBlock = blocks.First().Clone();
			for (int i = 1; i < blocks.Count; i++)
			{
				int skip = 0;
				if ((combinedBlock.BlockElements.Last() is ScriptText) && (blocks[i].BlockElements.First() is ScriptText))
				{
					((ScriptText)combinedBlock.BlockElements.Last()).Content += ((ScriptText)blocks[i].BlockElements.First()).Content;
					skip = 1;
				}
				foreach (var blockElement in blocks[i].BlockElements.Skip(skip))
					combinedBlock.BlockElements.Add(blockElement.Clone());
			}
			return combinedBlock;
		}

		public void ClearUnappliedSplits()
		{
			m_unappliedSplitBlocks.Clear();
		}

		public static void ProcessAssignmentForMultiBlockQuote(int bookNum, List<Block> multiBlockQuote, ScrVers versification)
		{
			var uniqueCharacters = multiBlockQuote.Select(b => b.CharacterId).Distinct().ToList();
			int numUniqueCharacters = uniqueCharacters.Count;
			var uniqueCharacterDeliveries = multiBlockQuote.Select(b => new QuoteParser.CharacterDelivery(b.CharacterId, b.Delivery)).Distinct(QuoteParser.CharacterDelivery.CharacterDeliveryComparer).ToList();
			int numUniqueCharacterDeliveries = uniqueCharacterDeliveries.Count;
			if (numUniqueCharacterDeliveries > 1)
			{
				var unclearCharacters = new[] { CharacterVerseData.kAmbiguousCharacter, CharacterVerseData.kUnknownCharacter };
				if (numUniqueCharacters > unclearCharacters.Count(uniqueCharacters.Contains) + 1)
				{
					// More than one real character. Set to Ambiguous.
					SetCharacterAndDeliveryForMultipleBlocks(bookNum, multiBlockQuote, CharacterVerseData.kAmbiguousCharacter, null, versification);
				}
				else if (numUniqueCharacters == 2 && unclearCharacters.All(uniqueCharacters.Contains))
				{
					// Only values are Ambiguous and Unique. Set to Ambiguous.
					SetCharacterAndDeliveryForMultipleBlocks(bookNum, multiBlockQuote, CharacterVerseData.kAmbiguousCharacter, null, versification);
				}
				else if (numUniqueCharacterDeliveries <= numUniqueCharacters)
				{
					// Only one real character (and delivery). Set to that character (and delivery).
					var realCharacter = uniqueCharacterDeliveries.Single(c => c.Character != CharacterVerseData.kAmbiguousCharacter && c.Character != CharacterVerseData.kUnknownCharacter);
					SetCharacterAndDeliveryForMultipleBlocks(bookNum, multiBlockQuote, realCharacter.Character, realCharacter.Delivery, versification);
				}
			}
		}

		private static void SetCharacterAndDeliveryForMultipleBlocks(int bookNum, IEnumerable<Block> blocks, string character, string delivery, ScrVers versification)
		{
			foreach (Block block in blocks)
			{
				block.SetCharacterIdAndCharacterIdInScript(character, bookNum, versification);
				block.Delivery = delivery;

				if (character == CharacterVerseData.kAmbiguousCharacter || character == CharacterVerseData.kUnknownCharacter)
					block.UserConfirmed = false;
			}
		}

		public void ReplaceBlocks(int iStartBlock, int count, IReadOnlyCollection<Block> replacementBlocks)
		{
			var blockIndexFollowingReplacement = iStartBlock + count;
			if (m_blocks.Count > blockIndexFollowingReplacement)
			{
				if (m_blocks[blockIndexFollowingReplacement].IsContinuationOfPreviousBlockQuote)
				{
					var lastReplacementBlock = replacementBlocks.Last();
					if (lastReplacementBlock.MultiBlockQuote == MultiBlockQuote.None)
					{
						throw new ArgumentException("Last replacement block must have a MultiBlockQuote value of Start or Continuation, since the first " +
							"block following the replacement range is a Continuation block.");
					}
					if (lastReplacementBlock.CharacterIsStandard)
						throw new InvalidOperationException("Following blocks are continuations of a \"quote\" that is now assigned to " +
							$"{lastReplacementBlock.CharacterId}. We need to look at this data condition to see what the desired beahvior is. ***Final block in " +
							$"matchup: {lastReplacementBlock} ***First following block: {m_blocks[blockIndexFollowingReplacement]}");
					do
					{
						m_blocks[blockIndexFollowingReplacement].CharacterId = lastReplacementBlock.CharacterId;
						// REVIEW: We need to think about whether the delivery should automatically flow through the continuation blocks
						// outside the matchup (probably not).
						// m_blocks[blockIndexFollowingReplacement].Delivery = lastReplacementBlock.Delivery;
						m_blocks[blockIndexFollowingReplacement].CharacterIdOverrideForScript = lastReplacementBlock.CharacterIdOverrideForScript;
					} while (++blockIndexFollowingReplacement < m_blocks.Count && m_blocks[blockIndexFollowingReplacement].IsContinuationOfPreviousBlockQuote);
				}
			}
			m_blocks.RemoveRange(iStartBlock, count);
			m_blocks.InsertRange(iStartBlock, replacementBlocks);
			OnBlocksInserted(iStartBlock, replacementBlocks.Count - count);
		}
	}
}
