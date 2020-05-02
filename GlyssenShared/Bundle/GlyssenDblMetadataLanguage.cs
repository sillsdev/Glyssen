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
		private string m_heSaidTextTemp;

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
		/// Historically, the "heSaidText" element held the (on-and-only) rendering of "he said" in
		/// the language. This was needed only for reference texts and was not exposed via the UI.
		/// However, now that we are detecting and applying reporting clauses, there is the
		/// possibility of multiple ones. We don't actually care what the specific meaning of each
		/// one is. For data migration purposes, we don't ever expect to have both the "heSaidText"
		/// element and a list of reporting clauses, but the set code should handle it correctly if
		/// it ever happened. For most reference texts, the list of reporting clauses will just
		/// contain a single entry (which used to be the "heSaidText"). 
		/// </summary>
		[XmlElement("heSaidText")]
		[DefaultValue(null)]
		public string HeSaidText_DeprecatedXml
		{
			get => null;
			set => HeSaidText = value;

		}

		/// <summary>
		/// Translation equivalent for "he said." in this language.
		/// </summary>
		public string HeSaidText
		{
			get => ReportingClauses?.FirstOrDefault();
			set
			{
				m_heSaidTextTemp = value;
				if (ReportingClauses == null)
					ReportingClauses = new List<string>(new [] { value });
				else if (ReportingClauses.Count == 0)
					ReportingClauses.Add(value);
				else if (ReportingClauses[0] != value)
				{
					if (ReportingClauses.Contains(value))
						ReportingClauses.Remove(value);
					ReportingClauses.Insert(0, value);
				}
			}
		}

		/// <summary>
		/// Translation equivalent for "he said", "they said", "she says", "you will say", etc. in
		/// this language. The first one will be treated as the default "he said" text. (The
		/// current UI doesn't actually give the user a way to order them to ensure this, so if we
		/// ever implement the ability to create a custom reference text from a Glyssen project,
		/// they will need a way to re-order them so that the desired "he said" is first.)
		/// </summary>
		[XmlElement("reportingClause")]
		public List<string> ReportingClauses { get; set; }

		public GlyssenDblMetadataLanguage Clone()
		{
			return (GlyssenDblMetadataLanguage)MemberwiseClone();
		}
	}
}
