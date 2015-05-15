using System.ComponentModel;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.DblBundle.Text;

namespace Glyssen.Bundle
{
	[XmlRoot("DBLMetadata")]
	public class PgDblMetadataLanguage : DblMetadataLanguage
	{
		private int m_fontSizeInPoints;

		/// <summary>
		/// This is pulled in from the stylesheet or set via the ProjectMetadataDlg.
		/// </summary>
		[XmlElement("fontFamily")]
		public string FontFamily { get; set; }

		/// <summary>
		/// This is pulled in from the stylesheet or set via the ProjectMetadataDlg.
		/// </summary>
		[XmlElement("fontSizeInPoints")]
		public int FontSizeInPoints
		{
			get { return m_fontSizeInPoints == 0 ? Settings.Default.DefaultFontSize : m_fontSizeInPoints; }
			set { m_fontSizeInPoints = value; }
		}

		[XmlElement("fontSizeUiAdjustment")]
		[DefaultValue(0)]
		public int FontSizeUiAdjustment { get; set; }
	}
}
