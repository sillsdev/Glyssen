using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
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
		private string m_heSaidText;

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
			get { return m_fontSizeInPoints == 0 ? 10 : m_fontSizeInPoints; }
			set { m_fontSizeInPoints = value; }
		}

		[XmlElement("fontSizeUiAdjustment")]
		[DefaultValue(0)]
		public int FontSizeUiAdjustment { get; set; }

		/// <summary>
		/// Translation equivalent for "he said." in this language.
		/// This is needed only for reference texts and is not exposed via the UI.
		/// </summary>
		[XmlElement("heSaidText")]
		[DefaultValue(null)]
		public string HeSaidText
		{
			get => m_heSaidText;
			set
			{
				m_heSaidText = value;
				if (ReportingClauses != null && !ReportingClauses.Contains(value))
					ReportingClauses.Add(value);
			}
		}

		/// <summary>
		/// Translation equivalent for "he said", "they said", "she says", "you will say", etc. in
		/// this language.
		/// </summary>
		/// <remarks>If we ever implement the ability to create a custom reference text from a
		/// Glyssen project, there will need a way to select the desired "he said" from this list
		/// to set <see cref="HeSaidText"/>.</remarks>
		[XmlElement("reportingClause")]
		public HashSet<string> ReportingClauses { get; set; }

		public GlyssenDblMetadataLanguage Clone()
		{
			return (GlyssenDblMetadataLanguage)MemberwiseClone();
		}
	}
}
