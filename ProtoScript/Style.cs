using System.Xml.Serialization;

namespace ProtoScript
{
	public class Style : IStyle
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("versetext")]
		public bool IsVerseText { get; set; }

		[XmlAttribute("publishable")]
		public bool IsPublishable { get; set; }

		public bool IsChapterLabel
		{
			get { return Id == "cl"; }
		}

		public bool IsParallelPassageReference
		{
			get { return Id == "r"; }
		}

		public bool HoldsBookNameOrAbbreviation
		{
			get
			{
				if (!IsPublishable || IsVerseText)
					return false;
				if (Id.StartsWith("h") || Id.StartsWith("toc") || Id.StartsWith("mt")) 
					return true;
				return false;
			}
		}
	}
}
