using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Palaso.Xml;

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

		public string FontFamily { get; private set; }
		public int FontSizeInPoints { get; private set; }

		[XmlElement(ElementName = "style")]
		public List<Style> Styles { get; set; }
	}

	public class StylesheetProperty
	{
		public string name { get; set; }
		public string unit { get; set; }

		[XmlText]
		public string Value { get; set; }
	}
}
