using System;
using System.Xml.Serialization;
using SIL.DblBundle.Text;

namespace Glyssen.Shared.Bundle
{
	public interface IReadOnlyGlyssenDblTextMetadata
	{
		DblMetadataCopyright Copyright { get; }
		string Id { get; }
		DblMetadataIdentification Identification { get; }
		GlyssenDblMetadataLanguage Language { get; }
		DateTime LastModified { get; }
		int Revision { get; }
	}

	public abstract class GlyssenDblTextMetadataBase : DblTextMetadata<GlyssenDblMetadataLanguage>, IReadOnlyGlyssenDblTextMetadata
	{
		protected int m_fontSizeInPointsTemp;
		protected string m_fontFamilyTemp;

		[XmlElement("language")]
		public override GlyssenDblMetadataLanguage Language
		{
			set
			{
				base.Language = value;
				if (m_fontFamilyTemp != default(string))
				{
					Language.FontFamily = m_fontFamilyTemp;
					m_fontFamilyTemp = default(string);
				}
				if (m_fontSizeInPointsTemp != default(int))
				{
					Language.FontSizeInPoints = m_fontSizeInPointsTemp;
					m_fontSizeInPointsTemp = default(int);
				}
			}
		}

		/// <summary>
		/// Last modified date to project - updated when project is saved
		/// </summary>
		[XmlAttribute("modifieddate")]
		public DateTime LastModified { get; set; }

		/// <summary>
		/// The font family for the language associated with this project.
		/// This is pulled in from the stylesheet or set via the ProjectSettingsDlg.
		/// </summary>
		[XmlIgnore]
		public string FontFamily
		{
			get { return Language == null ? m_fontFamilyTemp : Language.FontFamily; }
			set
			{
				if (Language == null)
					m_fontFamilyTemp = value;
				else
					Language.FontFamily = value;
			}
		}

		/// <summary>
		/// The font size for the language associated with this project.
		/// This is pulled in from the stylesheet or set via the ProjectSettingsDlg.
		/// </summary>
		[XmlIgnore]
		public int FontSizeInPoints
		{
			get { return Language == null ? m_fontSizeInPointsTemp : Language.FontSizeInPoints; }
			set
			{
				if (Language == null)
					m_fontSizeInPointsTemp = value;
				else
					Language.FontSizeInPoints = value;
			}
		}

		[XmlIgnore]
		public int FontSizeUiAdjustment
		{
			get { return Language == null ? 0 : Language.FontSizeUiAdjustment; }
			set { Language.FontSizeUiAdjustment = value; }
		}
	}
}