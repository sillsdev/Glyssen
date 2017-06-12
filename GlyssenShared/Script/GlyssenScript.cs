using System.Collections.Generic;
using System.Xml.Serialization;
using Glyssen.Shared.Bundle;

namespace Glyssen.Shared.Script
{
	[XmlRoot("glyssenscript")]
	public class GlyssenScript : GlyssenDblTextMetadataBase
	{
		public GlyssenScript()
		{
			// Needed for serialization/deserialization
		}

		public GlyssenScript(IReadOnlyGlyssenDblTextMetadata source)
		{
			Copyright = source.Copyright;
			Id = source.Id;
			Identification = source.Identification;
			Language = source.Language;
			LastModified = source.LastModified;
			Revision = source.Revision;
		}

		[XmlElement("script")]
		public Script Script { get; set; }
	}

	public class Script
	{
		[XmlElement(ElementName = "book")]
		public List<ScriptBook> Books { get; set; }
	}

	public class ScriptBook
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlElement(ElementName = "chapter")]
		public List<ScriptChapter> Chapters { get; set; }
	}

	public class ScriptChapter
	{
		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlElement(ElementName = "block")]
		public List<ScriptBlock> Blocks { get; set; }
	}

	public class ScriptBlock
	{
		[XmlAttribute("id")]
		public int Id { get; set; }
		[XmlAttribute("actor")]
		public string Actor { get; set; }
		[XmlAttribute("tag")]
		public string Tag { get; set; }
		[XmlAttribute("verse")]
		public string Verse { get; set; }
		[XmlAttribute("character")]
		public string Character { get; set; }
		[XmlAttribute("delivery")]
		public string Delivery { get; set; }
		[XmlAttribute("file")]
		public string File { get; set; }

		[XmlElement("vern")]
		public Vernacular Vernacular { get; set; }

		[XmlElement("primaryref")]
		public Reference Primary { get; set; }

		[XmlElement("secondaryref")]
		public Reference Secondary { get; set; }
	}

	public class Vernacular
	{
		[XmlText]
		public string Text { get; set; }
	}

	public class Reference
	{
		[XmlAttribute("xml:lang", DataType = "language")]
		public string LanguageCode { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
