using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Serialization;

namespace ProtoScript
{
	[XmlRoot("book")]
	public class BookScript : IScrBook
	{
		private Dictionary<int, int> m_chapterStartBlockIndices;
		private int m_blockCount;
		private List<Block> m_blocks;

		public BookScript()
		{
			// Needed for deserialization
		}

		public BookScript(string bookId, IEnumerable<Block> blocks)
		{
			BookId = bookId;
			Blocks = blocks.ToList();
		}

		[XmlAttribute("id")]
		public string BookId { get; set; }

		[XmlAttribute("singlevoice")]
		public bool SingleVoice { get; set; }

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
				m_chapterStartBlockIndices = new Dictionary<int, int>();
				m_blockCount = m_blocks.Count;
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

		public IReadOnlyList<Block> GetScriptBlocks(bool join = false)
		{
			EnsureBlockCount();

			if (!join || m_blockCount == 0)
				return m_blocks;

			var list = new List<Block>(m_blockCount);
			list.Add(m_blocks[0]);
			for (int i = 1; i < m_blockCount; i++)
			{
				var block = m_blocks[i];
				if (!block.IsParagraphStart)
				{
					var prevBlock = list.Last();
					if (block.CharacterId == prevBlock.CharacterId && block.Delivery == prevBlock.Delivery)
					{
						var newBlock = prevBlock.Clone();
						newBlock.BlockElements = new List<BlockElement>(prevBlock.BlockElements.Count + block.BlockElements.Count);
						foreach (var blockElement in prevBlock.BlockElements.Concat(block.BlockElements))
							newBlock.BlockElements.Add(blockElement.Clone());
						newBlock.UserConfirmed &= block.UserConfirmed;
						list[list.Count - 1] = newBlock;
						continue;
					}
				}
				list.Add(block);
			}
			return list;
		}

		public string GetVerseText(int chapter, int verse)
		{
			var iFirstBlockToExamine = GetIndexOfFirstBlockForVerse(chapter, verse);
			if (iFirstBlockToExamine < 0)
				return String.Empty;
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

		private int GetIndexOfFirstBlockForVerse(int chapter, int verse)
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
				if (block.InitialStartVerseNumber < verse)
					continue;
				iFirstBlockToExamine = index;
				if (block.InitialStartVerseNumber > verse || !(block.BlockElements.First() is Verse))
					iFirstBlockToExamine--;
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

		public void ApplyUserDecisions(BookScript sourceBookScript)
		{
			var comparer = new BlockElementContentsComparer();
			int iTarget = 0;
			foreach (var sourceBlock in sourceBookScript.m_blocks.Where(b => b.UserConfirmed))
			{
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
						m_blocks[iTarget].CharacterId = sourceBlock.CharacterId;
						m_blocks[iTarget].Delivery = sourceBlock.Delivery;
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

		public Block SplitBlock(Block blockToSplit, string verseToSplit, int characterOffsetToSplit)
		{
			var iBlock = m_blocks.IndexOf(blockToSplit);

			if (iBlock < 0)
				throw new ArgumentException("Block not found in the list for " + BookId, "blockToSplit");

			if (verseToSplit == null && characterOffsetToSplit == 0)
			{
				SplitBeforeBlock(iBlock);
				return blockToSplit;
			}

			var currVerse = blockToSplit.InitialEndVerseNumber == 0
				? blockToSplit.InitialStartVerseNumber.ToString(CultureInfo.InvariantCulture)
				: blockToSplit.InitialStartVerseNumber + "-" + blockToSplit.InitialEndVerseNumber;

			Block newBlock = null;
			int indexOfFirstElementToRemove = -1;

			for (int i = 0; i < blockToSplit.BlockElements.Count; i++)
			{
				var blockElement = blockToSplit.BlockElements[i];

				if (newBlock != null)
				{
					if (indexOfFirstElementToRemove < 0)
						indexOfFirstElementToRemove = i;
					newBlock.BlockElements.Add(blockElement);
					continue;
				}

				Verse verse = blockElement as Verse;
				if (verse != null)
					currVerse = verse.Number;
				else if (verseToSplit == currVerse)
				{
					ScriptText text = blockElement as ScriptText;

					if (text == null)
						continue;

					var content = text.Content;

					if (characterOffsetToSplit <= 0 || characterOffsetToSplit >= content.Length)
					{
						throw new ArgumentOutOfRangeException("characterOffsetToSplit", characterOffsetToSplit,
							"Value must be greater than 0 and less than or equal to the length (" + content.Length +
							") of the text of verse " + currVerse + ".");
					}

					var verseNumParts = verseToSplit.Split(new []{'-'}, 2, StringSplitOptions.None);
					int initialStartVerse = int.Parse(verseNumParts[0]);
					int initialEndVerse = verseNumParts.Length == 2 ? int.Parse(verseNumParts[1]) : 0;
					newBlock = new Block(blockToSplit.StyleTag, blockToSplit.ChapterNumber,
						initialStartVerse, initialEndVerse);
					newBlock.BlockElements.Add(new ScriptText(content.Substring(characterOffsetToSplit)));
					text.Content = content.Substring(0, characterOffsetToSplit);
					m_blocks.Insert(iBlock + 1, newBlock);
					foreach (
						var chapterNum in m_chapterStartBlockIndices.Keys.Where(chapterNum => chapterNum > blockToSplit.ChapterNumber))
						m_chapterStartBlockIndices[chapterNum]++;

					m_blockCount++;
				}
			}

			if (newBlock == null)
				throw new ArgumentException("Verse not found in given block.", "verseToSplit");

			if (indexOfFirstElementToRemove >= 0)
			{
				while (indexOfFirstElementToRemove < blockToSplit.BlockElements.Count)
					blockToSplit.BlockElements.RemoveAt(indexOfFirstElementToRemove);
			}

			return newBlock;
		}

		private void SplitBeforeBlock(int indexOfBlockToSplit)
		{
			if (indexOfBlockToSplit == 0 || m_blocks[indexOfBlockToSplit].MultiBlockQuote == MultiBlockQuote.None ||
				m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote == MultiBlockQuote.None)
			{
				throw new InvalidOperationException("Split allowed only between blocks that are part of a multi-block quote");
			}

			if (m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote == MultiBlockQuote.Start)
				m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote = MultiBlockQuote.None;

			if (indexOfBlockToSplit < m_blockCount - 1 && m_blocks[indexOfBlockToSplit + 1].MultiBlockQuote == MultiBlockQuote.Continuation)
				m_blocks[indexOfBlockToSplit].MultiBlockQuote = MultiBlockQuote.Start;
			else
				m_blocks[indexOfBlockToSplit].MultiBlockQuote = MultiBlockQuote.None;
		}
	}
}
