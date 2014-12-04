using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SIL.ScriptureUtils;

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

		/// <summary>
		/// Don't use this getter in production code. It is intended ONLY for use by the XML serializer!
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

		public IReadOnlyList<Block> ScriptBlocks
		{
			get { return m_blocks; }
		}

		public string GetVerseText(int chapter, int verse)
		{
			// Admittedly, this isn't the best way to prevent changes, but it is easier than doing custom serialization or trying to encapsulate the
			// class to allow XML serialization but not expose the Blocks getter.
			if (m_blockCount == 0)
				m_blockCount = m_blocks.Count;
			else if (m_blockCount != m_blocks.Count)
				throw new InvalidOperationException("Blocks collection changed. Blocks getter should not be used to add or remove blocks to the list. Use setter instead.");
			if (m_blockCount == 0)
				return string.Empty;
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
				if (block.InitialVerseNumber < verse)
					continue;
				iFirstBlockToExamine = index;
				if (block.InitialVerseNumber > verse || !(block.BlockElements.First() is Verse))
					iFirstBlockToExamine--;
				break;
			}

			if (iFirstBlockToExamine < 0)
			{
				if (!chapterStartFound)
					return string.Empty;
				iFirstBlockToExamine = m_blockCount - 1;
			}
			StringBuilder bldr = new StringBuilder();
			bool foundVerseStart = false;
			for (int index = iFirstBlockToExamine; index < m_blockCount; index++)
			{
				var block = m_blocks[index];
				foreach (var element in block.BlockElements)
				{
					Verse verseElement = element as Verse;
					if (verseElement != null)
					{
						var startVerse = ScrReference.VerseToIntStart(verseElement.Number);
						var endVerse = ScrReference.VerseToIntEnd(verseElement.Number);
						if (verse > endVerse)
							continue;
						if (verse >= startVerse && verse <= endVerse)
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
	}
}
