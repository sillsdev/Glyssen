using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Dialogs;
using SIL.DblBundle.Text;
using SIL.Xml;

namespace Glyssen.Bundle
{
	[XmlRoot("DBLMetadata")]
	public class GlyssenDblTextMetadata : DblTextMetadata<GlyssenDblMetadataLanguage>
	{
		private int m_fontSizeInPointsTemp;
		private string m_fontFamilyTemp;

		#region public Properties
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

		///// <summary>This is only needed as a temporary place to store the recording project name when the user metadata
		///// is created for a project that does not have a bundle - this can go away if we stop supporting those.</summary>
		//[XmlIgnore]
		//public string PgRecordingProjectName;

		/// <summary>We add this when we parse the USX to create
		/// a script. If significant changes to the parser are made and the parser version in the program does
		/// not match the stored parser version, then we know to re-parse the original USX data.</summary>
		[XmlAttribute("usxparserversion")]
		[DefaultValue(0)]
		public int ParserVersion;

		/// <summary>
		/// If we attempt to upgrade the project, and the user decides to opt out,
		/// we need to store the version he opted out of so we don't ask again.</summary>
		[XmlAttribute("parserupgradeoptoutversion")]
		[DefaultValue(0)]
		public int ParserUpgradeOptOutVersion;

		/// <summary>
		/// We add this when we parse the USX to create a script.
		/// This tells us the original (local) path of the Text Release Bundle file used to create this project.
		/// </summary>
		[XmlAttribute("origdblpath")]
		public string OriginalPathBundlePath;

        /// <summary>
        /// Optional ID for tracking a project (used by FCBH's internal database)
        /// </summary>
        [XmlAttribute("audiostocknumber")]
        public string AudioStockNumber { get; set; }

		/// <summary>
		/// We use this to know if character assignments should be reprocessed.
		/// </summary>
		[XmlAttribute("controlfileversion")]
		public int ControlFileVersion;

		/// <summary>
		/// If true, the project is hidden from the user unless he chooses to view inactive projects
		/// </summary>
		[XmlAttribute("hiddenbydefault")]
		[DefaultValue(false)]
		public bool Inactive { get; set; }

		/// <summary>
		/// Last modified date to project - updated when project is saved
		/// </summary>
		[XmlAttribute("modifieddate")]
		public DateTime LastModified { get; set; }

		[XmlElement("projectStatus")]
		public ProjectStatus ProjectStatus = new ProjectStatus();

		[XmlElement("characterGroupGenerationPreferences")]
		public CharacterGroupGenerationPreferences CharacterGroupGenerationPreferences = new CharacterGroupGenerationPreferences();

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

		/// <summary>
		/// If a project does not come with a versification file, this is the name of the standard versification to be used.
		/// </summary>
		[XmlAttribute("versification")]
		[DefaultValue("English")]
		public string Versification;

		/// <summary>
		/// If a project does not come with a versification file, this is the name of the standard versification to be used.
		/// </summary>
		[XmlAttribute("chapterannouncement")]
		[DefaultValue(ChapterAnnouncement.PageHeader)]
		public ChapterAnnouncement ChapterAnnouncementStyle;

		[XmlAttribute("firstchapterannouncement")]
		[DefaultValue(false)]
		public bool IncludeChapterAnnouncementForFirstChapter { get; set; }

		[XmlAttribute("singlechapterannouncement")]
		[DefaultValue(false)]
		public bool IncludeChapterAnnouncementForSingleChapterBooks { get; set; }

		/// <summary>
		/// Gets the revision number from a standard DBL bundle. If this bundle is an ad-hoc bundle created by Paratext,
		/// this will instead be the (Mercurial) changeset id (which is a GUID)
		/// </summary>
		public string RevisionOrChangesetId
		{
			get
			{
				if (Revision == 0 && Identification != null)
				{
					var paratext = Identification.SystemIds.FirstOrDefault(si => si.Type == "paratext");
					if (paratext != null && !String.IsNullOrEmpty(paratext.ChangeSetId))
						return paratext.ChangeSetId;
				}
				return Revision.ToString(CultureInfo.InvariantCulture);
			}
		}
		#endregion

		public new string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

		protected override void InitializeMetadata()
		{
			base.InitializeMetadata();
			if (Language == null && (m_fontFamilyTemp != default(string) || m_fontSizeInPointsTemp != default(int)))
				Language = new GlyssenDblMetadataLanguage();
		}

		public static string GetRevisionOrChangesetId(string filename)
		{
			Exception exception;
			var metadata = XmlSerializationHelper.DeserializeFromFile<GlyssenDblTextMetadata>(filename, out exception);
			return metadata != null ? metadata.RevisionOrChangesetId : null;
		}

		#region Deprecated properties used only for deserialization of projects with an old version of the data
		[XmlElement("isBookSelectionUserConfirmed")]
		[DefaultValue(false)]
		public bool IsBookSelectionUserConfirmed_DeprecatedXml
		{
			get { return false; }
			set
			{
				if (value)
					ProjectStatus.BookSelectionStatus = BookSelectionStatus.Reviewed;
			}
		}

		/// <summary>
		/// This is not part of the original DBL metadata. This data is now stored as part of the "language" data.
		/// </summary>
		[XmlElement("fontFamily")]
		public string FontFamily_DeprecatedXml
		{
			get { return null; }
			set { FontFamily = value; }
		}

		/// <summary>
		/// This is not part of the original DBL metadata. This data is now stored as part of the "langauge" data.
		/// </summary>
		[XmlElement("fontSizeInPoints")]
		[DefaultValue(default(int))]
		public int FontSizeInPoints_DeprecatedXml
		{
			get { return default(int); }
			set { FontSizeInPoints = value; }
		}
	    #endregion

		/// <summary>
		/// "Clone" only the bits of metadata that the user can modify in Glyssen.
		/// </summary>
		public void CopyGlyssenModifiableSettings(GlyssenDblTextMetadata source)
		{
			ProjectStatus = source.ProjectStatus;
			FontFamily = source.FontFamily;
			FontSizeInPoints = source.FontSizeInPoints;
			Language.ScriptDirection = source.Language.ScriptDirection;
			CharacterGroupGenerationPreferences = source.CharacterGroupGenerationPreferences;
			foreach (var book in AvailableBooks)
			{
				var sourceProjectBook = source.AvailableBooks.FirstOrDefault(b => book.Code == b.Code);
				if (sourceProjectBook != null)
					book.IncludeInScript = sourceProjectBook.IncludeInScript;
			}
		}
	}

	public class ProjectStatus
	{
		private QuoteSystemStatus m_quoteSystemStatus = QuoteSystemStatus.Unknown;

		[XmlElement("assignCharacterBlock")]
		public BookBlockIndices AssignCharacterBlock;

		[XmlElement("assignCharacterMode")]
		public BlocksToDisplay AssignCharacterMode;

		[XmlElement("quoteSystemStatus")]
		[DefaultValue(QuoteSystemStatus.Unknown)]
		public QuoteSystemStatus QuoteSystemStatus
		{
			get { return m_quoteSystemStatus; }
			set
			{
				if (m_quoteSystemStatus == value)
					return;
				m_quoteSystemStatus = value;
				QuoteSystemDate = DateTime.Now;
			}
		}

		[XmlElement("quoteSystemDate")]
		public DateTime QuoteSystemDate { get; set; }

		[XmlElement("bookSelectionStatus")]
		[DefaultValue(BookSelectionStatus.UnReviewed)]
		public BookSelectionStatus BookSelectionStatus { get; set; }

		[XmlElement("projectSettingsStatus")]
		[DefaultValue(ProjectSettingsStatus.UnReviewed)]
		public ProjectSettingsStatus ProjectSettingsStatus { get; set; }

		[XmlElement("voiceActorStatus")]
		[DefaultValue(VoiceActorStatus.UnProvided)]
		public VoiceActorStatus VoiceActorStatus { get; set; }

		[XmlElement("lastExportLocation")]
		[DefaultValue("")]
		public string LastExportLocation { get; set; }
	}

	public class CharacterGroupGenerationPreferences
	{
		[XmlElement("isSetByUser")]
		[DefaultValue(false)]
		public bool IsSetByUser { get; set; }

		/// <summary>
		/// Number of male narrators set by user
		/// </summary>
		[XmlElement("numberOfMaleNarrators")]
		[DefaultValue(0)]
		public int NumberOfMaleNarrators { get; set; }

		/// <summary>
		/// Number of female narrators set by user
		/// </summary>
		[XmlElement("numberOfFemaleNarrators")]
		[DefaultValue(0)]
		public int NumberOfFemaleNarrators { get; set; }

		/// <summary>
		/// How the number of narrators was set:
		/// 0 = not set
		/// 1 = single narrator
		/// 2 = narrator per author
		/// 3 = custom
		/// </summary>
		[XmlElement("narratorsOption")]
		[DefaultValue(0)]
		public int NarratorsOption { get; set; }

		/// <summary>
		/// How the number of voice actors was set:
		/// 0 = not set
		/// 1 = small cast size
		/// 2 = recommended cast size
		/// 3 = large cast size
		/// 4 = custom cast size
		/// 5 = match voice actor list
		/// </summary>
		[XmlElement("castSizeOption")]
		[DefaultValue(0)]
		public int CastSizeOption { get; set; }

		/// <summary>
		/// Number of male actors set by user
		/// </summary>
		[XmlElement("numberOfMaleActors")]
		[DefaultValue(0)]
		public int NumberOfMaleActors { get; set; }

		/// <summary>
		/// Number of female actors set by user
		/// </summary>
		[XmlElement("numberOfFemaleActors")]
		[DefaultValue(0)]
		public int NumberOfFemaleActors { get; set; }

		/// <summary>
		/// Number of child actors set by user
		/// </summary>
		[XmlElement("numberOfChildActors")]
		[DefaultValue(0)]
		public int NumberOfChildActors { get; set; }
	}

	[Flags]
	public enum QuoteSystemStatus
	{
		Unknown = 1,
		Guessed = 2,
		Reviewed = 4,
		Obtained = 8,
		UserSet = 16,
		NotParseReady = Unknown | Guessed,
		ParseReady = Reviewed | Obtained | UserSet
	}

	public enum BookSelectionStatus
	{
		UnReviewed,
		Reviewed
	}

	public enum ProjectSettingsStatus
	{
		UnReviewed,
		Reviewed
	}

	public enum VoiceActorStatus
	{
		UnProvided,
		Provided
	}

	public enum ChapterAnnouncement
	{
		// These "unused" items are used dynamically. The number of items before ChapterLabel must match the
		// number (and order) of items in ProjectSettingsDlg.m_cboBookMarker.Items.
		PageHeader,
		MainTitle1,
		ShortNameFromMetadata,
		LongNameFromMetadata,
		ChapterLabel, // Keep this one last for easier logic in ProjectSettingsDlg
	}
}
