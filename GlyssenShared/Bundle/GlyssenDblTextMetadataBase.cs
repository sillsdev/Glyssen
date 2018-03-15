using System;
using System.Xml.Serialization;
using SIL.DblBundle.Text;

namespace Glyssen.Shared.Bundle
{
	/// <summary>
	/// Lets us read the metadata without exposing it to possible revision
	/// </summary>
	public interface IReadOnlyGlyssenDblTextMetadata
	{
		DblMetadataCopyright Copyright { get; }
		Guid UniqueRecordingProjectId { get; }
		string Id { get; }
		DblMetadataIdentification Identification { get; }
		GlyssenDblMetadataLanguage Language { get; }
		DateTime LastModified { get; }
		string AudioStockNumber { get; }
		int Revision { get; }
	}

	/// <summary>
	/// This class essentially exists to allow us to share serialization for certain elements across classes
	/// (e.g. GlyssenScript and GlyssenDblTextMetadata)
	///
	/// It adds metadata to what we received from the Digital Bible Library (DBL)
	/// </summary>
	public abstract class GlyssenDblTextMetadataBase : DblTextMetadata<GlyssenDblMetadataLanguage>, IReadOnlyGlyssenDblTextMetadata
	{
		protected int m_fontSizeInPointsTemp;
		protected string m_fontFamilyTemp;
		private Guid m_uniqueRecordingProjectId;

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
		/// The idea is that every project has a unique identifier which
		/// informs us if this is the exact project or a similar one.
		/// This is useful when another application such as HearThis goes to load
		/// a script and needs to know if it already knows about that recording project.
		/// </summary>
		[XmlAttribute("uniqueProjectId")]
		public Guid UniqueRecordingProjectId
		{
			get
			{
				if (m_uniqueRecordingProjectId == Guid.Empty)
					m_uniqueRecordingProjectId = Guid.NewGuid();
				return m_uniqueRecordingProjectId;
			}
			set { m_uniqueRecordingProjectId = value; }
		}

		/// <summary>
		/// Optional ID for tracking a project (used by FCBH's internal database)
		/// </summary>
		[XmlAttribute("audiostocknumber")]
		public string AudioStockNumber { get; set; }

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