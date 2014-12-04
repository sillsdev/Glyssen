using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Paratext;

namespace ProtoScript
{
	class ScrStylesheetAdapter : IStylesheet
	{
		private readonly ScrStylesheet m_scrStylesheet;
		private readonly Dictionary<string, Style> m_styleLookup;

		public ScrStylesheetAdapter(ScrStylesheet scrStylesheet)
		{
			m_scrStylesheet = scrStylesheet;
			m_styleLookup = new Dictionary<string, Style>(scrStylesheet.Tags.Count());
		}

		public IStyle GetStyle(string styleId)
		{
			Style style;
			if (m_styleLookup.TryGetValue(styleId, out style))
				return style;
			var tag = m_scrStylesheet.GetTag(styleId);
			if (tag == null)
			{
				style = new Style
				{
					Id = styleId,
				};
				Debug.Fail("Unexpected style found in data");
			}
			else
			{
				style = new Style
				{
					Id = styleId,
					IsPublishable = ((tag.TextProperties & TextProperties.scPublishable) > 0),
					IsVerseText = ((tag.TextType & ScrTextType.scVerseText) > 0),
				};
			}
			m_styleLookup[styleId] = style;
			return style;
		}

		public string FontFamily { get; private set; }
		public int FontSizeInPoints { get; private set; }
	}
}
