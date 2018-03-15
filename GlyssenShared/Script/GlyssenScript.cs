using System.Collections.Generic;
using System.Xml.Serialization;
using Glyssen.Shared.Bundle;

namespace Glyssen.Shared.Script
{
	/// <summary>
	/// Defines a Glyssen script which can be exported and used by other applications, e.g. HearThis.
	/// It seems odd that it extends something called Metadata, but that is basically done for serialization purposes.
	/// Some of the DBL (Digital Bible Library) metadata is purposefully maintained from the bundle through the
	/// project to the script (e.g. language info).
	/// </summary>
	[XmlRoot("glyssenScript")]
	public class GlyssenScript : GlyssenDblTextMetadataBase
	{
		// Needed for serialization/deserialization
		public GlyssenScript()
		{
			Version = "1.0";
		}

		public GlyssenScript(string recordingProjectName, IReadOnlyGlyssenDblTextMetadata source) : this()
		{
			RecordingProjectName = recordingProjectName;
			Copyright = source.Copyright;
			UniqueRecordingProjectId = source.UniqueRecordingProjectId;
			Id = source.Id;
			Identification = source.Identification;
			Language = source.Language;
			LastModified = source.LastModified;
			AudioStockNumber = source.AudioStockNumber;
			Revision = source.Revision;

			Script = new Script
			{
				Books = new List<ScriptBook>(),
				LanguageCode = source.Language.Ldml
			};
		}

		/// <summary>
		/// This is used by a consuming application to know which versions it is capable of opening.
		/// We will use semver, so upgrade the minor for non-breaking changes and the major for
		/// any change which could cause a consumer a problem when reading.
		/// </summary>
		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlAttribute("projectName")]
		public string RecordingProjectName { get; set; }

		[XmlElement("script")]
		public Script Script { get; set; }
	}

	public class Script
	{
		[XmlElement(ElementName = "book")]
		public List<ScriptBook> Books { get; set; }

		[XmlAttribute("xml:lang", DataType = "language")]
		public string LanguageCode { get; set; }
	}

	/// <summary>
	/// Book data in the script
	/// </summary>
	public class ScriptBook
	{
		/// <summary>
		/// The book ID, e.g. 'MAT'
		/// </summary>
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlElement(ElementName = "chapter")]
		public List<ScriptChapter> Chapters { get; set; }
	}

	/// <summary>
	/// Chapter data in the script
	/// </summary>
	public class ScriptChapter
	{
		/// <summary>
		/// The chapter number.
		/// Extra-biblical material (intro, book title, etc.) which comes before chapter 1 will be 0.
		/// </summary>
		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlElement(ElementName = "block")]
		public List<ScriptBlock> Blocks { get; set; }
	}

	/// <summary>
	/// Block data in the script (essentially a line in the script)
	/// </summary>
	public class ScriptBlock
	{
		/// <summary>
		/// A sequential number starting with 1 at the beginning of each chapter
		/// </summary>
		[XmlAttribute("id")]
		public int Id { get; set; }

		/// <summary>
		/// The actor who speaks the line
		/// </summary>
		[XmlAttribute("actor")]
		public string Actor { get; set; }

		/// <summary>
		/// The original sfm marker which applies to this text, e.g. 'm'
		/// </summary>
		[XmlAttribute("tag")]
		public string Tag { get; set; }

		/// <summary>
		/// The applicable verse number or bridge
		/// </summary>
		[XmlAttribute("verse")]
		public string Verse { get; set; }

		/// <summary>
		/// The Biblical character who speaks the line
		/// </summary>
		[XmlAttribute("character")]
		public string Character { get; set; }

		/// <summary>
		/// How the line should be delivered, e.g. 'questioning'
		/// </summary>
		[XmlAttribute("delivery")]
		public string Delivery { get; set; }

		[XmlElement("text")]
		public TextWithLanguage VernacularText { get; set; }

		[XmlElement(ElementName = "referenceText")]
		public List<TextWithLanguage> ReferenceTexts { get; set; }
	}

	public class TextWithLanguage
	{
		[XmlAttribute("xml:lang", DataType = "language")]
		public string LanguageCode { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
