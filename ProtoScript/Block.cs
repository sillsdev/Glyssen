using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Palaso.Xml;

namespace ProtoScript
{
	[XmlRoot("block")]
	public class Block
	{
		private int m_initialVerseNumber;
		private int m_chapterNumber;

		public Block()
		{
			// Needed for deserialization
		}

		public Block(string styleTag, int chapterNum = 0, int initialVerseNum = 0)
		{
			StyleTag = styleTag;
			BlockElements = new List<BlockElement>();
			ChapterNumber = chapterNum;
			InitialVerseNumber = initialVerseNum;
		}

		[XmlAttribute("style")]
		public string StyleTag { get; set; }

		[XmlAttribute("chapter")]
		public int ChapterNumber
		{
			get
			{
				if (m_chapterNumber == 0)
				{
					if (InitialVerseNumber > 0 || BlockElements.Any(b => b is Verse))
						m_chapterNumber = 1;
				}
				return m_chapterNumber;
			}
			set { m_chapterNumber = value; }
		}

		[XmlAttribute("verse")]
		public int InitialVerseNumber
		{
			get
			{
				if (m_initialVerseNumber == 0)
				{
					var leadingVerseElement = BlockElements.FirstOrDefault() as Verse;
					int verseNum;
					if (leadingVerseElement != null)
					{
						if (Int32.TryParse(leadingVerseElement.Number, out verseNum))
							m_initialVerseNumber = verseNum;
						else
							Debug.Fail("TODO: Deal with bogus verse number in data!");
					}
					else if (BlockElements.Any(b => b is Verse))
					{
						m_initialVerseNumber = 1;
					}
				}
				return m_initialVerseNumber; 
			}
			set { m_initialVerseNumber = value; }
		}

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

		public string GetAsXml(bool includeXmlDeclaration = true)
		{
			return XmlSerializationHelper.SerializeToString(this, !includeXmlDeclaration);
		}

		public string GetAsTabDelimited(string bookId)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(StyleTag);
			builder.Append('\t');
			builder.Append(bookId);
			builder.Append('\t');
			builder.Append(ChapterNumber);
			builder.Append('\t');
			builder.Append(InitialVerseNumber);
			builder.Append('\t');
			builder.Append(CharacterId);
			builder.Append('\t');
			builder.Append(GetText(true));
			return builder.ToString();
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
