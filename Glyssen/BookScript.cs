using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Glyssen.Character;
using Glyssen.Quote;
using Glyssen.Shared;
using Glyssen.ViewModel;
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
		}

		[XmlAttribute("id")]
		public string BookId
		{
			get { return Id; }
			set { m_id = value; }
		}

		[XmlAttribute("singlevoice")]
		public bool SingleVoice { get; set; }

		[XmlAttribute("pageheader")]
		public string PageHeader { get; set; }

		[XmlAttribute("maintitle")]
		public string MainTitle { get; set; }

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
			while (m_blocks[i].InitialStartVerseNumber < verse)
				i++;
			return i;
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

		public void ApplyUserDecisions(BookScript sourceBookScript, ScrVers versification = null)
		{
			foreach (var sourceUnappliedSplit in sourceBookScript.UnappliedSplits)
			{
				List<Block> targetUnappliedSplit = sourceUnappliedSplit.Select(splitPart => splitPart.Clone()).ToList();
				m_unappliedSplitBlocks.Add(targetUnappliedSplit);
			}
			ApplyUserSplits(sourceBookScript);
			ApplyUserAssignments(sourceBookScript, versification);
			CleanUpMultiBlockQuotes(versification);
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
							m_blocks[iTarget].SetCharacterAndCharacterIdInScript(sourceBlock.CharacterId, bookNum, versification);
						else
						{
							m_blocks[iTarget].CharacterId = sourceBlock.CharacterId;
							m_blocks[iTarget].CharacterIdOverrideForScript = sourceBlock.CharacterIdOverrideForScript;
						}
						m_blocks[iTarget].Delivery = sourceBlock.Delivery;
						if (sourceBlock.MatchesReferenceText)
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

		private void ApplyUserSplits(BookScript sourceBookScript)
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

			var comparer = new SplitBlockComparer();

			for (int index = 0; index < m_unappliedSplitBlocks.Count; index++)
			{
				var unappliedSplit = m_unappliedSplitBlocks[index];
				var firstBlockOfSplit = unappliedSplit.First();
				var i = GetIndexOfFirstBlockThatStartsWithVerse(firstBlockOfSplit.ChapterNumber, firstBlockOfSplit.InitialStartVerseNumber);
				var iFirstMatchingBlock = i;
				var iUnapplied = 0;
				bool blocksMatch;
				do
				{
					var splitBlock = unappliedSplit[iUnapplied];
					var parsedBlock = m_blocks[i++];
					blocksMatch = comparer.Equals(splitBlock, parsedBlock);
					if (iUnapplied > 0 || blocksMatch)
					{
						if (!blocksMatch)
							break;
						if (iUnapplied == 0)
							iFirstMatchingBlock = i;
						iUnapplied++;
					}
				} while (i < m_blocks.Count && iUnapplied < unappliedSplit.Count);
				if (blocksMatch)
				{
					m_unappliedSplitBlocks.RemoveAt(index--);
				}
				else
				{
					var combinedBlock = CombineBlocks(unappliedSplit);
					for (int iBlock = iFirstMatchingBlock; iBlock < m_blocks.Count && m_blocks[iBlock].InitialStartVerseNumber == combinedBlock.InitialStartVerseNumber; iBlock++)
					{
						if (comparer.Equals(combinedBlock, m_blocks[iBlock]))
						{
							i = iBlock;
							for (iUnapplied = 1; iUnapplied < unappliedSplit.Count; iUnapplied++)
							{
								var elementsOfBlockPrecedingSplit = unappliedSplit[iUnapplied - 1].BlockElements;
								var textElementAtEndOfBlockPrecedingSplit = elementsOfBlockPrecedingSplit.Last() as ScriptText;
								int offset = textElementAtEndOfBlockPrecedingSplit != null ? textElementAtEndOfBlockPrecedingSplit.Content.Length : 0;
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

							m_unappliedSplitBlocks.RemoveAt(index--);
							break;
						}
					}
				}
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
				block.SetCharacterAndCharacterIdInScript(character, bookNum, versification);
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
					do
					{
						m_blocks[blockIndexFollowingReplacement].CharacterId = lastReplacementBlock.CharacterId;
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
