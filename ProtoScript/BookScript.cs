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
		private Dictionary<int, int> m_chapterStartBlockIndices = new Dictionary<int, int>();
 
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

		[XmlElement(ElementName = "block")]
		public List<Block> Blocks { get; set; }

		public string GetVerseText(int chapter, int verse)
		{
			int chapterStartBlock;
			bool chapterStartFound = m_chapterStartBlockIndices.TryGetValue(chapter, out chapterStartBlock);

			int iFirstBlockToExamine = -1;
			for (int index = chapterStartBlock; index < Blocks.Count; index++)
			{
				var block = Blocks[index];
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
				iFirstBlockToExamine = Blocks.Count - 1;
			}
			StringBuilder bldr = new StringBuilder();
			bool foundVerseStart = false;
			for (int index = iFirstBlockToExamine; index < Blocks.Count; index++)
			{
				var block = Blocks[index];
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
