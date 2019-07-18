using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
		[DefaultValue(false)]
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
				prevBlock.ClearReferenceText();
				var narrator = CharacterVerseData.GetStandardCharacterId(BookId, CharacterVerseData.StandardCharacter.Narrator);
				for (var i = 1; i < m_blockCount; i++)
				{
					var clonedBlock = m_blocks[i].Clone();
					clonedBlock.ClearReferenceText();
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

				// Note: block.InitialEndVerseNumber will be non-0 only for a verse bridge.
				// But in the case of a verse bridge, it is not sufficient to consider only
				// the InitialStartVerseNumber when checking to see if we've found the correct
				// block.

				if (block.InitialStartVerseNumber < verse && block.InitialEndVerseNumber < verse)
					continue;
				iFirstBlockToExamine = index;
				if (block.BlockElements.First() is Verse)
				{
					if (block.InitialStartVerseNumber == verse || block.InitialEndVerseNumber >= verse)
						return iFirstBlockToExamine;
				}
				if (iFirstBlockToExamine > 0 && m_blocks[iFirstBlockToExamine - 1].LastVerseNum >= verse)
					return iFirstBlockToExamine - 1;
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
			var sourceBlocks = sourceBookScript.GetScriptBlocks(false);
			for (int iSrc = 0; iSrc < sourceBlocks.Count; iSrc++)
			{
				var sourceBlock = sourceBlocks[iSrc];
				if (!sourceBlock.MatchesReferenceText)
					continue;
				int iTargetBlock = GetIndexOfFirstBlockForVerse(sourceBlock.ChapterNumber, sourceBlock.InitialStartVerseNumber);
				if (!sourceBlock.IsScripture)
				{
					while (!m_blocks[iTargetBlock].IsScripture)
					{
						var targetBlock = m_blocks[iTargetBlock];
						if (((ScriptText)targetBlock.BlockElements.Single()).Content == ((ScriptText)sourceBlock.BlockElements.Single()).Content)
						{
							if (!targetBlock.MatchesReferenceText)
							{
								targetBlock.SetMatchedReferenceBlock(sourceBlock.ReferenceBlocks.Single());
								targetBlock.CloneReferenceBlocks();
							}
							break;
						}
						iTargetBlock++;
					}
					continue;
				}
				var targetMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(this, iTargetBlock,
					versification);
				var targetMatchupInitialVerse = targetMatchup.CorrelatedBlocks[0].InitialStartVerseNumber;
				if (targetMatchupInitialVerse < m_blocks[iTargetBlock].InitialStartVerseNumber)
					continue; // Oops, we ended up going backwards into the target
				if (targetMatchupInitialVerse < sourceBlock.InitialStartVerseNumber)
				{
					if (sourceBlock.BlockElements.First() is Verse verseToSplitBefore)
					{
						var iCorrespondingTargetMatchupBlock = targetMatchup.CorrelatedBlocks.IndexOf(b => b.BlockElements.OfType<Verse>().FirstOrDefault()?.Number == verseToSplitBefore.Number);
						if (!sourceBlocks.Skip(iSrc).Take(targetMatchup.CorrelatedBlocks.Count - iCorrespondingTargetMatchupBlock)
							.SequenceEqual(targetMatchup.CorrelatedBlocks.Skip(iCorrespondingTargetMatchupBlock), blockComparer))
						{
							continue;
						}
						var verseToSplitAfter = m_blocks[iTargetBlock].BlockElements.OfType<Verse>()
							.TakeWhile(v => v.Number != verseToSplitBefore.Number).LastOrDefault()?.Number ??
							m_blocks[iTargetBlock].InitialVerseNumberOrBridge;
						SplitBlock(m_blocks[iTargetBlock++], verseToSplitAfter, kSplitAtEndOfVerse, false, null, versification);
						targetMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(this, iTargetBlock,
							versification);
						targetMatchupInitialVerse = targetMatchup.CorrelatedBlocks[0].InitialStartVerseNumber;
						Debug.Assert(targetMatchupInitialVerse == m_blocks[iTargetBlock].InitialStartVerseNumber &&
							targetMatchupInitialVerse == sourceBlock.InitialStartVerseNumber);
					}
					else
						continue;
				}
				else if (!sourceBlocks.Skip(iSrc).Take(targetMatchup.CorrelatedBlocks.Count).SequenceEqual(targetMatchup.CorrelatedBlocks, blockComparer))
					continue;
				var sourceMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(sourceBookScript, iSrc,
					versification, (uint)targetMatchup.CorrelatedBlocks.Count, false);
				if (sourceMatchup.CountOfBlocksAddedBySplitting != 0)
				{
					Debug.Fail("Something unexpected happened. Logic above should guarantee that unsplit source matched split target.");
					continue;
				}
				if (!sourceMatchup.AllScriptureBlocksMatch)
				{
					// Something has apparently changed (in the reference text or in the parser?) that causes the target matchup
					// to now contain some blocks than were not orginally matched up when the source blocks were aligned.
					continue;
				}

				iSrc += sourceMatchup.OriginalBlockCount - 1; // Need to subtract 1 because this gets incremented in for loop.

				for (int i = 0; i < sourceMatchup.CorrelatedBlocks.Count; i++)
				{
					sourceBlock = sourceMatchup.CorrelatedBlocks[i];
					var targetBlock = targetMatchup.CorrelatedBlocks[i];
					{
						targetBlock.SetMatchedReferenceBlock(sourceBlock.ReferenceBlocks.Single());
						targetBlock.CloneReferenceBlocks();
						targetBlock.SetCharacterAndDeliveryInfo(sourceBlock, BookNumber, versification);
						targetBlock.SplitId = sourceBlock.SplitId;
						targetBlock.MultiBlockQuote = sourceBlock.MultiBlockQuote;
						targetBlock.UserConfirmed = sourceBlock.UserConfirmed;
					}
				}
				targetMatchup.Apply(versification);
			}
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

			var verseToSplit = firstBlockOfSplit.InitialVerseNumberOrBridge;

			// Very likely, the split was done on a block that was part of a larger parsed block that was chunked
			// up according to the reference text, though it may also have been split manually. If we can find that
			// larger block with matching verse text on either side of the splits, we can still apply them (though
			// we won't attempt to fully or partially connect it up with the reference text).
			var blockToSplit = GetFirstBlockForVerse(firstBlockOfSplit.ChapterNumber, firstBlockOfSplit.InitialStartVerseNumber);
			var indexOfFirstCorrespondingElement = -1;
			for (int iElem = 0; iElem < blockToSplit.BlockElements.Count; iElem++)
			{
				if (blockToSplit.BlockElements[iElem] is Verse v && v.Number == verseToSplit)
				{
					indexOfFirstCorrespondingElement = iElem;
					break;
				}
			}
			Debug.Assert(indexOfFirstCorrespondingElement != -1);
			var indexOfLastCorrespondingElement = indexOfFirstCorrespondingElement + combinedBlockElements.Count -
				(firstBlockOfSplit.BlockElements[0] is Verse ? 1 : 0);
			if (indexOfLastCorrespondingElement >= blockToSplit.BlockElements.Count)
				return false;
			var textOfLastVerseInBlockToSplit = ((ScriptText)blockToSplit.BlockElements[indexOfLastCorrespondingElement]).Content;
			var textOfLastUnappliedSplitVerse = unappliedSplit.Last().BlockElements.OfType<ScriptText>().Last().Content;
			if (!combinedBlockElements.Take(combinedBlockElements.Count - 1).SequenceEqual(
				blockToSplit.BlockElements.Skip(indexOfFirstCorrespondingElement).Take(combinedBlockElements.Count - 1), comparer) ||
				!textOfLastVerseInBlockToSplit.EndsWith(textOfLastUnappliedSplitVerse))
			{
				return false;
			}

			var helper = new SplitBlockHelper(this, blockToSplit, indexOfLastCorrespondingElement);

			bool restoreFirstBlockSplitId = (unappliedSplit.Count > 1 && unappliedSplit[0].StartsAtVerseStart && blockToSplit.SplitId == Block.kNotSplit);
			blockToSplit.SplitId = firstBlockOfSplit.SplitId;

			for (int iSplit = unappliedSplit.Count - 1; iSplit >= 0; iSplit--)
			{
				var currentSplit = unappliedSplit[iSplit];
				Block chipOffTheOldBlock;
				if (currentSplit.StartsAtVerseStart && verseToSplit == blockToSplit.InitialVerseNumberOrBridge)
				{
					// This is a split right at a verse break. It is likely (though not absolutely certain) that this split
					// originated as a non-user break, when Glyssen aligned the text to the reference text. But since the
					// user then did a manual break, the preceding block break also got converted to a user split. 
					var iBlock = m_blocks.IndexOf(blockToSplit);
					if (iSplit == 0 && restoreFirstBlockSplitId)
						return true;
					// The "normal" rules for a user break were thus not enforced. In order to be able to re-apply this split,
					// tell it we're reapplying splits, so it skips that check.
					SplitBeforeBlock(iBlock, currentSplit.SplitId, true, currentSplit.CharacterId, null, true);
					chipOffTheOldBlock = blockToSplit;
				}
				else
				{
					chipOffTheOldBlock = helper.SplitBlockBasedOn(currentSplit);
					chipOffTheOldBlock.CharacterId = currentSplit.CharacterId;
				}
				chipOffTheOldBlock.CharacterIdOverrideForScript = currentSplit.CharacterIdOverrideForScript;
				chipOffTheOldBlock.Delivery = currentSplit.Delivery;
			}

			if (restoreFirstBlockSplitId)
				blockToSplit.SplitId = Block.kNotSplit;
			return true;
		}

		private class SplitBlockHelper
		{
			private readonly BookScript m_bookScript;
			private readonly Block m_blockToSplit;
			private string m_verseNumber;
			private int m_elementIndex;
			private int m_remainingLength;
			private Block m_currentSplit;

			internal SplitBlockHelper(BookScript bookScript, Block blockToSplit, int elementStartIndex)
			{
				if (blockToSplit.BlockElements.Count <= elementStartIndex)
					throw new IndexOutOfRangeException();
				if (blockToSplit.BlockElements[elementStartIndex] is Verse)
					throw new ArgumentException("Starting index should not be a verse number element.");

				m_bookScript = bookScript;
				m_blockToSplit = blockToSplit;
				m_elementIndex = elementStartIndex;
			}

			private void SetVerseNumber(string verseNumber)
			{
				if (verseNumber == m_verseNumber)
					return;

				m_verseNumber = verseNumber;
				for (bool foundCorrectVerse = false; m_elementIndex >= 0 && !foundCorrectVerse; m_elementIndex--)
				{
					var elem = m_blockToSplit.BlockElements[m_elementIndex];

					if (elem is ScriptText text)
						m_remainingLength = text.Content.Length;
					else if (elem is Verse verse)
						foundCorrectVerse = (verse.Number == verseNumber);
				}
			}

			internal Block SplitBlockBasedOn(Block currentSplit)
			{
				AdjustFor(currentSplit);
				return m_bookScript.SplitBlock(m_blockToSplit, m_verseNumber, m_remainingLength);
			}

			private void AdjustFor(Block currentSplit)
			{
				if (m_currentSplit == currentSplit)
					return;
				SetVerseNumber(currentSplit.InitialVerseNumberOrBridge);
				m_remainingLength -= currentSplit.BlockElements.OfType<ScriptText>().First().Content.Length;
				if (m_remainingLength == 0)
				{
					// This is a split at the start of a verse. We need to re-interpret that as a split at the end of the preceding verse.
					SetVerseNumber(m_blockToSplit.BlockElements.Take(m_elementIndex).OfType<Verse>().LastOrDefault()?.Number ??
						m_blockToSplit.InitialVerseNumberOrBridge);
				}
				m_currentSplit = currentSplit;
			}
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
			m_blocks.RemoveRange(iStartBlock, count);
			m_blocks.InsertRange(iStartBlock, replacementBlocks);
			UpdateFollowingContinuationBlocks(iStartBlock + replacementBlocks.Count - 1);
			OnBlocksInserted(iStartBlock, replacementBlocks.Count - count);
		}

		/// <summary>
		/// "Fixes" any quote continuation blocks immediately following the given block index. If the given index
		/// is for a quote block (i.e., the character ID is not narrator or -- and this is probably impossible in
		/// practice -- some other standard character type), then the following continuation blocks are set to have
		/// the same character speak. Otherwise, the multi-block quote chain is broken and the following block(s)
		/// form a new chain, beginning with the first following block. (Of course, if there is only one following
		/// block, then it's no longer a multi-block chain at all.)
		/// </summary>
		/// <param name="iBlock"></param>
		public void UpdateFollowingContinuationBlocks(int iBlock)
		{
			var iNextBlock = iBlock + 1;
			if (m_blocks.Count > iNextBlock && m_blocks[iNextBlock].IsContinuationOfPreviousBlockQuote)
			{
				var baseBlock = m_blocks[iBlock];
				if (baseBlock.MultiBlockQuote == MultiBlockQuote.None)
				{
					if (m_blocks.Count > iNextBlock + 1 && m_blocks[iNextBlock + 1].IsContinuationOfPreviousBlockQuote)
					{
						m_blocks[iNextBlock++].MultiBlockQuote = MultiBlockQuote.Start;
						do
						{
							m_blocks[iNextBlock].MultiBlockQuote = MultiBlockQuote.Continuation;
						} while (++iNextBlock < m_blocks.Count && m_blocks[iNextBlock].IsContinuationOfPreviousBlockQuote);
					}
					else
					{
						m_blocks[iNextBlock].MultiBlockQuote = MultiBlockQuote.None;
					}
				}
				else
				{
					if (baseBlock.CharacterIsStandard)
						throw new InvalidOperationException("Caller is responsible for setting preceding block(s)' MultiBlockQuote property set to None\r\n" +
							$"{baseBlock.ToString(true, BookId)}");
					do
					{
						m_blocks[iNextBlock].CharacterId = baseBlock.CharacterId;
						// REVIEW: We need to think about whether the delivery should automatically flow through the continuation blocks
						// outside the matchup (probably not).
						// m_blocks[blockIndexFollowingReplacement].Delivery = lastReplacementBlock.Delivery;
						m_blocks[iNextBlock].CharacterIdOverrideForScript = baseBlock.CharacterIdOverrideForScript;
					} while (++iNextBlock < m_blocks.Count && m_blocks[iNextBlock].IsContinuationOfPreviousBlockQuote);
				}
			}
		}
	}
}
