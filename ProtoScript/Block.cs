using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Palaso.Xml;

namespace ProtoScript
{
	[XmlRoot("block")]
	public class Block
	{
		public Block()
		{
			// Needed for deserialization
		}

		public Block(string styleTag)
		{
			StyleTag = styleTag;
			BlockElements = new List<BlockElement>();
		}

		[XmlAttribute("style")]
		public string StyleTag { get; set; }

		[XmlAttribute("characterId")]
		public int CharacterId { get; set; }

		[XmlElement(Type = typeof(ScriptText), ElementName = "text")]
		[XmlElement(Type = typeof(Verse), ElementName = "verse")]
		public List<BlockElement> BlockElements { get; set; }

		public string GetText(bool includeVerseNumbers)
		{
			StringBuilder bldr = new StringBuilder();
			foreach (var blockElement in BlockElements)
			{
				Verse verse = blockElement as Verse;
				if (verse != null)
				{
					if (includeVerseNumbers)
					{
						bldr.Append("[");
						bldr.Append(verse.Number);
						bldr.Append("]");
					}
				}
				else
				{
					ScriptText text = blockElement as ScriptText;
					if (text != null)
						bldr.Append(text.Content);
				}
			}
			return bldr.ToString();
		}

		[XmlIgnore]
		public bool IsNarrator
		{
			get { return CharacterId == 0; }
		}

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}
	}

	[XmlInclude(typeof(ScriptText))]
	[XmlInclude(typeof(Verse))]
	public abstract class BlockElement
	{
	}

	public class ScriptText : BlockElement
	{
		public ScriptText()
		{
			// Needed for deserialization
		}

		public ScriptText(string content)
		{
			Content = content;
		}

		[XmlText]
		public string Content { get; set; }
	}

	public class Verse : BlockElement
	{
		public Verse()
		{
			// Needed for deserialization
		}

		public Verse(string number)
		{
			Number = number;
		}

		[XmlAttribute("num")]
		public string Number{ get; set; }
	}
}
