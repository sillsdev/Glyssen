using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using SIL.Xml;

namespace GlyssenEngine.Character
{
	[XmlRoot(ElementName="StyleToCharacaterMappings")]
	public class StyleToCharacterMappings
	{
		[XmlElement(ElementName="StyleMapping")]
		public List<StyleMapping> StyleMappings { get; set; }

		private static Dictionary<string, string> s_dictionary;

		private static Dictionary<string, string> All
		{
			get
			{
				if (s_dictionary == null)
				{
					var me = XmlSerializationHelper.DeserializeFromString<StyleToCharacterMappings>(Resources.StyleToCharacterMappings);
					s_dictionary = me.StyleMappings.ToDictionary(m => m.StandardFormatMarker, m => m.Character);
				}
				return s_dictionary;
			}
		}

		public static bool TryGetCharacterForCharStyle(string charTag, out string character)
		{
			return All.TryGetValue(charTag, out character);
		}

		public static bool Includes(string charTag)
		{
			return All.ContainsKey(charTag);
		}

		internal static IEnumerable<string> AllSfMarkers => All.Keys;
	}

	[XmlRoot(ElementName="StyleMapping")]
	public class StyleMapping
	{
		[XmlAttribute(AttributeName="sf")]
		public string StandardFormatMarker { get; set; }

		[XmlAttribute(AttributeName="character")]
		public string Character { get; set; }
	}
}
