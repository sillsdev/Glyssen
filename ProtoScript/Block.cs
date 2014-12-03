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
		/// <summary>Blocks represents a quote whose character has not been set (usually represents an unexpected quote)</summary>
		public const string UnknownCharacter = "Unknown";
		/// <summary>
		/// Blocks represents a quote whose character has not been set.
		/// Used when the user needs to disambiguate between multiple potential characters.
		/// </summary>
		public const string AmbiguousCharacter = "Ambiguous";
		/// <summary>Blocks which has not yet been parsed to identify contents/character</summary>
		public static readonly string NotSet = null;

		public enum StandardCharacter
		{
			Narrator,
			BookOrChapter,
			ExtraBiblical,
			Intro,
		}

		/// <summary>Character ID prefix for material to be read by narrator</summary>
		private const string kNarratorPrefix = "narrator-";
		/// <summary>Character ID prefix for book titles or chapter breaks</summary>
		private const string kBookOrChapterPrefix = "BC-";
		/// <summary>Character ID prefix for extra-biblical material (section heads, etc.)</summary>
		private const string kExtraBiblicalPrefix = "extra-";
		/// <summary>Character ID prefix for intro material</summary>
		private const string kIntroPrefix = "intro-";

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
		public string CharacterId { get; set; }

		[XmlAttribute("userConfirmed")]
		public bool UserConfirmed { get; set; }

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

		public bool CharacterIs(string prefixOrName)
		{
			if (CharacterId == prefixOrName)
				return true;

			return (prefixOrName.EndsWith("-") && CharacterId.StartsWith(prefixOrName));
		}

		public bool CharacterIs(StandardCharacter standardCharacterType)
		{
			return CharacterIs(GetCharacterPrefix(standardCharacterType));
		}

		public bool CharacterIs(string bookId, StandardCharacter standardCharacterType)
		{
			return CharacterId == GetCharacterPrefix(standardCharacterType) + bookId;
		}

		public void SetStandardCharacter(string bookId, StandardCharacter standardCharacterType)
		{
			CharacterId = GetCharacterPrefix(standardCharacterType) + bookId;
		}

		private string GetCharacterPrefix(StandardCharacter standardCharacterType)
		{
			switch (standardCharacterType)
			{
				case StandardCharacter.Narrator: return kNarratorPrefix;
				case StandardCharacter.BookOrChapter: return kBookOrChapterPrefix;
				case StandardCharacter.ExtraBiblical: return kExtraBiblicalPrefix;
				case StandardCharacter.Intro: return kIntroPrefix;
				default: throw new ArgumentException("Unexpected standard character type.");
			}
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
			builder.Append('\t');
			builder.Append(GetText(false).Length);
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
