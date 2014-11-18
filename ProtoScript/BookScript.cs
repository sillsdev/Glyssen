using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ProtoScript
{
	[XmlRoot("Book")]
	public class BookScript
	{
		public BookScript()
		{
			// Needed for deserialization
		}

		public BookScript(string BookScript, IEnumerable<Block> blocks)
		{
			BookId = BookScript;
			Blocks = blocks.ToList();
		}

		[XmlAttribute("id")]
		public string BookId { get; set; }

		[XmlElement(ElementName = "Block")]
		public List<Block> Blocks { get; set; }
	}
}
