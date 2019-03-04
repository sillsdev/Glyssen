using System.ComponentModel;
using System.Xml.Serialization;
using Glyssen.Shared.Properties;
using SIL.DblBundle.Text;

namespace Glyssen.Shared.Bundle
{
	/// <summary>
	/// Glyssen-specific language info.
	/// This is in addition to the Digital Bible Library-provided info.
	/// </summary>
	[XmlRoot("DBLMetadata")]
	public class GlyssenDblMetadataLanguage : DblMetadataLanguage
	{
		private int m_fontSizeInPoints;

		/// <summary>
		/// This is pulled in from the stylesheet or set via the ProjectSettingsDlg.
		/// </summary>
		[XmlElement("fontFamily")]
		public string FontFamily { get; set; }

		/// <summary>
		/// This is pulled in from the stylesheet or set via the ProjectSettingsDlg.
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

		/// <summary>
		/// Translation equivalent for "he said." in this language. (Only needed for reference texts - currently
		/// not exposed via the UI.)
		/// </summary>
		[XmlElement("heSaidText")]
		[DefaultValue(null)]
		public string HeSaidText { get; set; }
	}
}
