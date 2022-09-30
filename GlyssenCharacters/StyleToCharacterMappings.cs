using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using GlyssenCharacters.Properties;
using SIL.Xml;

namespace GlyssenCharacters
{
	[XmlRoot(ElementName="StyleToCharacterMappings")]
	public class StyleToCharacterMappings
	{
		[XmlElement(ElementName="StyleMapping")]
		public List<StyleMapping> StyleMappings { get; set; }

		private static Dictionary<string, string> s_characterStylesDictionary;
		private static Dictionary<string, CharacterVerseData.StandardCharacter> s_paragraphStylesDictionary;

		private static Dictionary<string, string> CharacterStyles
		{
			get
			{
				if (s_characterStylesDictionary == null)
					LoadDictionaries();
				return s_characterStylesDictionary;
			}
		}

		private static Dictionary<string, CharacterVerseData.StandardCharacter> ParagraphStyles
		{
			get
			{
				if (s_paragraphStylesDictionary == null)
					LoadDictionaries();
				return s_paragraphStylesDictionary;
			}
		}

		private static void LoadDictionaries()
		{
			var mappings = XmlSerializationHelper.DeserializeFromString<StyleToCharacterMappings>(
				Resources.StyleToCharacterMappings).StyleMappings;
			s_characterStylesDictionary = mappings.Where(s => !s.IsParagraphStyle)
				.ToDictionary(m => m.StandardFormatMarker, m => m.Character);
			s_paragraphStylesDictionary = mappings.Where(s => s.IsParagraphStyle)
				.ToDictionary(m => m.StandardFormatMarker,
				m => (CharacterVerseData.StandardCharacter)Enum.Parse(
					typeof(CharacterVerseData.StandardCharacter), m.Character));
		}

		public static bool TryGetCharacterForCharStyle(string charTag, out string character)
		{
			return CharacterStyles.TryGetValue(charTag, out character);
		}

		public static bool TryGetStandardCharacterForParaStyle(string charTag,
			out CharacterVerseData.StandardCharacter standardCharacter)
		{
			return ParagraphStyles.TryGetValue(charTag, out standardCharacter);
		}

		public static bool TryGetCharacterForParaStyle(string charTag, string bookId,
			out string character)
		{
			if (TryGetStandardCharacterForParaStyle(charTag, out var standardCharacterType))
			{
				character = CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType);
				return true;
			}

			character = null;
			return false;
		}

		public static bool IncludesCharStyle(string charTag)
		{
			return CharacterStyles.ContainsKey(charTag);
		}

		internal static IEnumerable<string> AllSfMarkers => CharacterStyles.Keys.Union(ParagraphStyles.Keys);
	}

	[XmlRoot(ElementName="StyleMapping")]
	public class StyleMapping
	{
		[XmlAttribute(AttributeName="sf")]
		public string StandardFormatMarker { get; set; }

		[XmlAttribute(AttributeName="paragraph")]
		[DefaultValue(false)]
		public bool IsParagraphStyle { get; set; }

		[XmlAttribute(AttributeName="character")]
		public string Character { get; set; }
	}
}
