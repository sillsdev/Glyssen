using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Paratext;
using GlyssenEngine.Quote;
using SIL.Extensions;
using SIL.Scripture;
using SIL.Unicode;
using SIL.Xml;
using static System.Char;
using static System.String;

namespace GlyssenEngine.Script
{
	[XmlRoot("book")]
	public class BookScript : PortionScript, IScrBook
	{
		private Dictionary<int, int> m_chapterStartBlockIndices;
		private Dictionary<Block, string> m_narratorOverrides;
		private List<List<Block>> m_unappliedSplitBlocks = new List<List<Block>>();
		private ScrStylesheetAdapter m_styleSheet;
		private int m_blockCount;

		private BookScript() : base(null, null, null)
		{
			// Needed for deserialization
		}

		public BookScript(string bookId, IEnumerable<Block> blocks, ScrVers versification) : base(bookId, blocks, versification)
		{
			OnBlocksReset();
		}

		public static BookScript Deserialize(string fileName, ScrVers versification, out Exception error)
		{
			var newBook = XmlSerializationHelper.DeserializeFromFile<BookScript>(fileName, out error);
			newBook.Initialize(versification);
			return newBook;
		}

		public static BookScript Deserialize(string fileName, ScrVers versification)
		{
			var newBook = XmlSerializationHelper.DeserializeFromFile<BookScript>(fileName);
			newBook.Initialize(versification);
			return newBook;
		}

		[XmlAttribute("singlevoice")]
		public bool SingleVoice { get; set; }

		[XmlAttribute("pageheader")]
		public string PageHeader { get; set; }

		[XmlAttribute("maintitle")]
		public string MainTitle { get; set; }

		[DefaultValue(false)]
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

		public Block this[int i] => m_blocks[i];

		public bool HasScriptBlocks => m_blocks.Any();

		/// <summary>
		/// Don't use this getter in production code. It is intended ONLY for use by the XML serializer!
		/// This is to prevent accidentally leaking the actual list and risking modification by calling code.
		/// </summary>
		[XmlArray("UnappliedSplits")]
		[XmlArrayItem("Split")]
		public List<List<Block>> UnappliedBlockSplits_DoNotUse
		{
			get => m_unappliedSplitBlocks;
			set => m_unappliedSplitBlocks = value;
		}

		public System.Collections.Generic.IReadOnlyList<IEnumerable<Block>> UnappliedSplits => m_unappliedSplitBlocks;

		// This method is currently only used for testing
		internal BookScript Clone()
		{
			BookScript newBook = (BookScript)MemberwiseClone();
			newBook.Blocks = new List<Block>(GetScriptBlocks().Select(b => b.Clone()));
			newBook.m_unappliedSplitBlocks = new List<List<Block>>(m_unappliedSplitBlocks.Select(l => l.Select(b => b.Clone()).ToList()));
			return newBook;
		}

		public override IReadOnlyList<Block> GetScriptBlocks()
		{
			EnsureBlockCount();
			return base.GetScriptBlocks();
		}

		public BookScript GetCloneWithJoinedBlocks(bool applyNarratorOverrides)
		{
			BookScript clonedBook = (BookScript)MemberwiseClone();

			EnsureBlockCount();

			var list = clonedBook.Blocks = new List<Block>(m_blockCount);

			if (m_blockCount == 0)
				return clonedBook;

			Block.ReferenceBlockCloningBehavior refBlockCloning;
			Action<Block, Block> modifyClonedBlockAsNeeded = null;
			Func<Block, Block, bool> shouldCombine;

			if (SingleVoice)
			{
				refBlockCloning = Block.ReferenceBlockCloningBehavior.SetToNewEmptyList;
				var narrator = CharacterVerseData.GetStandardCharacterId(BookId, CharacterVerseData.StandardCharacter.Narrator);
				modifyClonedBlockAsNeeded = (orig, clone) =>
				{
					if (!clone.CharacterIsStandard)
						clone.CharacterIdInScript = narrator;
				};
				shouldCombine = (block1, block2) => !block2.IsParagraphStart || (block2.IsFollowOnParagraphStyle && !CharacterUtils.EndsWithSentenceFinalPunctuation(block1.GetText(false)));
			}
			else
			{
				refBlockCloning = Block.ReferenceBlockCloningBehavior.CrossLinkToOriginalReferenceBlockList;
				if (m_styleSheet == null)
					m_styleSheet = SfmLoader.GetUsfmStylesheet();

				if (applyNarratorOverrides)
				{
					modifyClonedBlockAsNeeded =
						(orig, clone) =>
						{
							clone.CharacterIdInScript = GetCharacterIdInScript(orig);
						};
				}

				shouldCombine = ShouldCombineBlocksInMultiVoiceBook;
			}

			var currBlock = m_blocks[0].Clone(refBlockCloning);
			modifyClonedBlockAsNeeded?.Invoke(m_blocks[0], currBlock);
			list.Add(currBlock);
			for (var i = 1; i < m_blockCount; i++)
			{
				var prevBlock = list.Last();
				currBlock = m_blocks[i].Clone(refBlockCloning);
				modifyClonedBlockAsNeeded?.Invoke(m_blocks[i], currBlock);
				if (shouldCombine(prevBlock, currBlock))
				{
					if (refBlockCloning == Block.ReferenceBlockCloningBehavior.CrossLinkToOriginalReferenceBlockList)
						prevBlock.CloneReferenceBlocks();
					prevBlock.CombineWith(currBlock);
				}
				else
					list.Add(currBlock);
			}

			return clonedBook;
		}

		internal bool ShouldCombineBlocksInMultiVoiceBook(Block block1, Block block2)
		{
			if (block2.MatchesReferenceText == block1.MatchesReferenceText &&
				block2.CharacterIdInScript == block1.CharacterIdInScript && (block2.Delivery ?? Empty) == (block1.Delivery ?? Empty))
			{
				if (block2.MatchesReferenceText)
				{
					return block2.ReferenceBlocks.Single().StartsWithEllipsis ||
						((!block2.IsParagraphStart || (block2.IsFollowOnParagraphStyle && !CharacterUtils.EndsWithSentenceFinalPunctuation(block1.GetText(false)))) &&
							!block2.ContainsVerseNumber &&
							((!block2.ReferenceBlocks.Single().BlockElements.OfType<Verse>().Any() &&
									!CharacterUtils.EndsWithSentenceFinalPunctuation(block1.GetText(false))) ||
								block2.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().All(t => t.Content.All(IsWhiteSpace)) ||
								block1.ReferenceBlocks.Single().BlockElements.OfType<ScriptText>().All(t => t.Content.All(IsWhiteSpace))));
				}

				if (!block2.StartsAtVerseStart)
				{
					var style = (StyleAdapter)m_styleSheet.GetStyle(block2.StyleTag);
					return !block2.IsParagraphStart || (style.IsPoetic && !CharacterUtils.EndsWithSentenceFinalPunctuation(block1.GetText(false)));
				}
			}

			return false;
		}

		internal string GetCharacterIdInScript(Block block)
		{
			if (block.CharacterIdOverrideForScript != null)
				return block.CharacterIdOverrideForScript;

			if (m_narratorOverrides == null)
				PopulateEffectiveNarratorOverrides();
			return m_narratorOverrides.TryGetValue(block, out string characterOverride) ?
				characterOverride : block.CharacterId;
		}

		private void AddOverrideInfo(NarratorOverrides.NarratorOverrideDetail info)
		{
			// STEP 1: Find the first block (if any) to which this override applies
			var firstChapter = info.StartChapter;
			int iBlock = GetIndexOfFirstBlockForVerse(firstChapter, info.StartVerse);
			while (iBlock == -1 && ++firstChapter <= info.EndChapter)
				iBlock = GetIndexOfFirstBlockForVerse(firstChapter, 1);
			if (iBlock == -1)
				return; // No existing blocks are covered by this override

			if (m_blocks[iBlock].ChapterNumber == info.StartChapter)
			{
				if (m_blocks[iBlock].LastVerseNum < info.StartVerse)
				{
					var offsetToNextChapter = m_blocks.Skip(iBlock).IndexOf(b => b.ChapterNumber > info.StartChapter);
					if (offsetToNextChapter == -1)
						return; // All the blocks in this chapter are before the override starts and there are no more blocks beyond this chapter.
					iBlock += offsetToNextChapter;
				}
				else if (info.StartBlock > 1)
				{
					var lastVerseNumInBlock = m_blocks[iBlock].LastVerseNum;
					if (lastVerseNumInBlock == info.StartVerse || (lastVerseNumInBlock < info.StartVerse && lastVerseNumInBlock >= info.StartVerse))
					{
						// Skip ahead to get to correct start block.
						for (int i = 1; i < info.StartBlock; i++)
						{
							var block = m_blocks[++iBlock];
							if (!block.IsScripture)
							{
								// Unlikely, but if this happens, we don't want to count it as one of the blocks to skip.
								i--;
								continue;
							}
							if (block.InitialStartVerseNumber > info.StartVerse && block.InitialEndVerseNumber > info.StartVerse)
								break;
						}
					}
				}
			}

			// STEP 2: Based on the characters used in the block(s) that pertain to this override, determine whether to apply it.
			// Details: There are three kinds of blocks that can be found within a range covered by an override:
			// 1) narrator block
			// 2) block explicitly attributed to the override character
			// 3) block explicitly attributed to some other character
			// In a really simple world, we would just override all the narrator blocks and leave the others alone. But
			// that's not the kind of world we live in. We need to correctly deal with the special case where a
			// vernacular translation uses explicit quotes around the author's "self-quoted" material and has an actual
			// narrator chime in with an occasional he-said. In that scenario, we wouldn't want those he-saids to get
			// automatically assigned to the implicit speaking character. To attempt to prevent this, the logic below
			// says that if any verse (present in the text) of the entire passage/chapter to override is entirely
			// assigned to the narrator (regardless of how many blocks represent the verse), then the override is
			// applied (to any narrator blocks in the range). Two special cases:
			// A) We never look at more than one chapter at a time, so if an override covers multiple chapters, this
			//    logic will treat each chapter (or portion thereof) as if it were a distinct override.
			// B) For overrides that apply to only part of a verse, if any included block is explicitly assigned to
			//    the override character, then the override will not be applied to any block.
			bool applyOverride = false;
			var narratorBlocksInRange = new List<Block>();
			int blocksFoundAtEndVerse = 0;
			int currentLastVerseNum = -1;
			bool currentVerseBlocksAllNarrator = false;

			for (int i = iBlock; i < m_blockCount; i++)
			{
				var block = m_blocks[i];

				if (block.ChapterNumber > info.EndChapter)
					break;
				if (block.ChapterNumber == info.EndChapter)
				{
					var lastVerseNum = block.LastVerseNum;
					if (lastVerseNum > info.EndVerse)
						break;
					if (lastVerseNum == info.EndVerse)
					{
						blocksFoundAtEndVerse++;
						if (info.EndBlock > 0 && blocksFoundAtEndVerse > info.NumberOfBlocksIncludedInEndVerse)
							break;
					}
				}

				if (block.InitialStartVerseNumber > 0 && block.CharacterId == NarratorCharacterId)
				{
					// A block at verse 0 (in the Psalms) is a Hebrew Title line. We do not support overriding those.
					// Style tag "qa" is an acrostic header. These also do not get overridden.
					if (block.StyleTag != "qa")
						narratorBlocksInRange.Add(block);
				}
				else
				{
					if (!applyOverride)
					{
						if (currentLastVerseNum < block.InitialStartVerseNumber && currentVerseBlocksAllNarrator)
						{
							// All blocks for the previous verse -- the last verse in the previous (i.e., "current") block --
							// were assigned to the narrator.
							applyOverride = true;
							continue;
						}
						currentLastVerseNum = block.LastVerseNum;
						currentVerseBlocksAllNarrator = false;
					}
					continue;
				}
				if (applyOverride) // Once this is true, all we need to do is collect the rest of the narrator blocks in range.
					continue; // Remaining logic in loop just checks additional edge-case conditions that indicate we found a narrator-only verse.

				if (currentLastVerseNum != block.LastVerseNum)
				{
					// If this (narrator) block represents the final block for the previous (i.e., "current") verse AND
					if (currentVerseBlocksAllNarrator || // all the previous blocks for the verse were also narrator blocks; OR
						(currentLastVerseNum == -1 &&    // this is the 1st block in range (i.e., no previous blocks for this
						block.CoversMoreThanOneVerse))   // verse need be considered)
					{
						applyOverride = true;
						continue;
					}
					currentLastVerseNum = block.LastVerseNum;
					currentVerseBlocksAllNarrator = true;
				}

				// If this (narrator) block contains a whole verse, then we have a narrator-only verse.
				if (block.BlockElements.OfType<Verse>().Skip(1).Any()) // This is a presumably more efficient equivalent to ...Count() >= 2
					applyOverride = true;
			}
			applyOverride |= currentVerseBlocksAllNarrator;
			if (applyOverride)
				m_narratorOverrides.AddRange(narratorBlocksInRange.Select(b => new KeyValuePair<Block, string>(b, info.Character)));
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
			m_narratorOverrides = null;
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
					if (block.InitialStartVerseNumber == verse || (block.InitialStartVerseNumber < verse && block.InitialEndVerseNumber >= verse))
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
			return i < m_blocks.Count && m_blocks[i].InitialStartVerseNumber <= verse ? i : -1;
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

		private void PopulateEffectiveNarratorOverrides()
		{
			m_narratorOverrides = new Dictionary<Block, string>();

			foreach (var info in NarratorOverrides.GetNarratorOverridesForBook(BookId, Versification))
				AddOverrideInfo(info);
		}

		public void ApplyUserDecisions(BookScript sourceBookScript, ReferenceText referenceTextToReapply = null)
		{
			var blockComparer = new SplitBlockComparer();

			foreach (var sourceUnappliedSplit in sourceBookScript.UnappliedSplits)
			{
				List<Block> targetUnappliedSplit = sourceUnappliedSplit.Select(splitPart => splitPart.Clone()).ToList();
				m_unappliedSplitBlocks.Add(targetUnappliedSplit);
			}

			ApplyUserSplits(sourceBookScript, blockComparer);
			if (referenceTextToReapply != null)
				ApplyReferenceBlockMatches(sourceBookScript, referenceTextToReapply, blockComparer);
			ApplyUserAssignments(sourceBookScript);
			CleanUpMultiBlockQuotes();
		}

		private void ApplyReferenceBlockMatches(BookScript sourceBookScript, ReferenceText referenceTextToReapply,
			SplitBlockComparer blockComparer)
		{
			var sourceBlocks = sourceBookScript.GetScriptBlocks();
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
								targetBlock.SetMatchedReferenceBlockFrom(sourceBlock);
							break;
						}
						iTargetBlock++;
					}
					continue;
				}
				var targetMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(this, iTargetBlock);
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
						SplitBlock(m_blocks[iTargetBlock++], verseToSplitAfter, kSplitAtEndOfVerse, false);
						targetMatchup = referenceTextToReapply.GetBlocksForVerseMatchedToReferenceText(this, iTargetBlock);
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
					(uint)targetMatchup.CorrelatedBlocks.Count, false);
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
						targetBlock.SetMatchedReferenceBlockFrom(sourceBlock);
						targetBlock.SetCharacterAndDeliveryInfo(sourceBlock, BookNumber, Versification);
						targetBlock.SplitId = sourceBlock.SplitId;
						targetBlock.MultiBlockQuote = sourceBlock.MultiBlockQuote;
						targetBlock.UserConfirmed = sourceBlock.UserConfirmed;
					}
				}
				targetMatchup.Apply(Versification);
			}
		}

		private void ApplyUserAssignments(BookScript sourceBookScript)
		{
			var comparer = new BlockElementContentsComparer();
			int iTarget = 0;
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
							m_blocks[iTarget].SetCharacterIdAndCharacterIdInScript(sourceBlock.CharacterId, sourceBookScript.BookNumber, Versification);
						else
							m_blocks[iTarget].SetCharacterInfo(sourceBlock);
						m_blocks[iTarget].Delivery = sourceBlock.Delivery;
						if (sourceBlock.MatchesReferenceText && !m_blocks[iTarget].MatchesReferenceText)
							m_blocks[iTarget].SetMatchedReferenceBlockFrom(sourceBlock);
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
			if (blockToSplit == null)
			{
				Debug.Fail("If this happens, examine data condition to see whether there is a logic error and/or a better way to handle it.");
				return false; // Not sure this can ever happen, but if it does, we can't apply this split.
			}

			var indexOfFirstCorrespondingElement = -1;
			for (int iElem = 0; iElem < blockToSplit.BlockElements.Count; iElem++)
			{
				if (blockToSplit.BlockElements[iElem] is Verse v && v.Number == verseToSplit)
				{
					indexOfFirstCorrespondingElement = iElem;
					break;
				}
			}

			if (indexOfFirstCorrespondingElement == -1)
				return false; // There may have been a verse numbering change.
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
					SplitBeforeBlock(iBlock, currentSplit.SplitId, true, currentSplit.CharacterId, true);
					chipOffTheOldBlock = blockToSplit;
				}
				else
				{
					chipOffTheOldBlock = helper.SplitBlockBasedOn(currentSplit);
					chipOffTheOldBlock.CharacterId = currentSplit.CharacterId;
				}
				chipOffTheOldBlock.CharacterIdInScript = currentSplit.CharacterIdOverrideForScript;
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

		public void CleanUpMultiBlockQuotes()
		{
			var navigator = new BlockNavigator(new[] { this }.ToReadOnlyList());
			foreach (IEnumerable<Block> multiBlock in GetScriptBlocks()
				.Where(b => b.MultiBlockQuote == MultiBlockQuote.Start)
				.Select(block => navigator.GetAllBlocksWhichContinueTheQuoteStartedByBlock(block)))
			{
				ProcessAssignmentForMultiBlockQuote(BCVRef.BookToNumber(BookId), multiBlock.ToList(), Versification);
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
				// First check to see if we need to set all these blocks to Ambiguous.
				var unclearCharacters = new[] { CharacterVerseData.kAmbiguousCharacter, CharacterVerseData.kUnexpectedCharacter /* "Unknown" */};
				if (numUniqueCharacters > unclearCharacters.Count(uniqueCharacters.Contains) + 1 || // More than one "real" character
					(numUniqueCharacters == 2 && unclearCharacters.All(uniqueCharacters.Contains))) // Only values are Ambiguous and Unknown
				{
					foreach (Block block in multiBlockQuote)
					{
						block.SetNonDramaticCharacterId(CharacterVerseData.kAmbiguousCharacter);
						block.UserConfirmed = false;
					}
				}
				else if (numUniqueCharacterDeliveries <= numUniqueCharacters)
				{
					// Only one real character (and delivery). Set to that character (and delivery).
					var realCharacter = uniqueCharacterDeliveries.Single(c => c.Character != CharacterVerseData.kAmbiguousCharacter && c.Character != CharacterVerseData.kUnexpectedCharacter);
					foreach (Block block in multiBlockQuote)
					{
						block.SetCharacterIdAndCharacterIdInScript(realCharacter.Character, bookNum, versification);
						block.Delivery = realCharacter.Delivery;
					}
				}
			}
		}

		public void ReplaceBlocks(int iStartBlock, int count, IReadOnlyCollection<Block> replacementBlocks)
		{
			m_blocks.RemoveRange(iStartBlock, count);
			m_blocks.InsertRange(iStartBlock, replacementBlocks);
			if (iStartBlock > 0 && m_blocks[iStartBlock].MultiBlockQuote != MultiBlockQuote.Continuation &&
				m_blocks[iStartBlock - 1].MultiBlockQuote == MultiBlockQuote.Start)
			{
				m_blocks[iStartBlock - 1].MultiBlockQuote = MultiBlockQuote.None;
			}

			var iLastInserted = iStartBlock + replacementBlocks.Count - 1;
			UpdateFollowingContinuationBlocks(iLastInserted);
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
						m_blocks[iNextBlock].SetCharacterInfo(baseBlock);
						// REVIEW: We need to think about whether the delivery should automatically flow through the continuation blocks.
						// m_blocks[iNextBlock].Delivery = baseBlock.Delivery;
					} while (++iNextBlock < m_blocks.Count && m_blocks[iNextBlock].IsContinuationOfPreviousBlockQuote);
				}
			}
		}
	}
}
