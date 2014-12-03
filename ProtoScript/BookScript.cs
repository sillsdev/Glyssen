using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ProtoScript
{
	[XmlRoot("book")]
	public class BookScript : IScrBook
	{
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
			throw new System.NotImplementedException();
		}
	}
}
