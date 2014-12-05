using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Palaso.Xml;
using ProtoScript.Properties;

namespace ProtoScript
{
	[XmlRoot("stylesheet")]
	public class UsxStylesheet : IStylesheet
	{
		private Dictionary<string, Style> m_styleLookup;

		public static UsxStylesheet Load(string filename, out Exception exception)
		{
			return XmlSerializationHelper.DeserializeFromFile<UsxStylesheet>(filename, out exception);
		}

		[XmlElement(ElementName = "property")]
		public List<StylesheetProperty> Properties { get; set; }

		public IStyle GetStyle(string styleId)
		{
			if (m_styleLookup == null)
				m_styleLookup = Styles.ToDictionary(s => s.Id, s => s);

			Style style;
			if (m_styleLookup.TryGetValue(styleId, out style))
				return style;

			Debug.Fail("Should never get here. Either we encountered an unknown style or dictionary got created prematurely.");

			return Styles.FirstOrDefault(s => s.Id == styleId);
		}

		public string FontFamily
		{
			get
			{
				var fontFamilyProperty = Properties.FirstOrDefault(p => p.name == "font-family");
				return fontFamilyProperty != null ? fontFamilyProperty.Value : null;
			}
		}

		public int FontSizeInPoints
		{
			get
			{
				var fontSizeProperty = Properties.FirstOrDefault(p => p.name == "font-size");
				int val;
				if (fontSizeProperty == null || !Int32.TryParse(fontSizeProperty.Value, out val))
					return Settings.Default.DefaultFontSize;

				if (fontSizeProperty.unit == "pt")
					return val;

				// REVIEW: Are any other units possible?
				return Settings.Default.DefaultFontSize;
			}
		}

		[XmlElement(ElementName = "style")]
		public List<Style> Styles { get; set; }
	}

	public class StylesheetProperty
	{
		[XmlAttribute]
		public string name { get; set; }
		[XmlAttribute]
		public string unit { get; set; }

		[XmlText]
		public string Value { get; set; }
	}
}
