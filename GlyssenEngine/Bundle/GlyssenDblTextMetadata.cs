using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Script;
using SIL.Xml;

namespace GlyssenEngine.Bundle
{
	[XmlRoot("DBLMetadata")]
	public class GlyssenDblTextMetadata : GlyssenDblTextMetadataBase
	{
		private ReferenceTextType m_referenceTextType = ReferenceTextType.English;

		#region public Properties

		///// <summary>This is only needed as a temporary place to store the recording project name when the user metadata
		///// is created for a project that does not have a bundle - this can go away if we stop supporting those.</summary>
		//[XmlIgnore]
		//public string PgRecordingProjectName;

		/// <summary>We add this when we parse the USX to create
		/// a script. If significant changes to the parser are made and the parser version in the program does
		/// not match the stored parser version, then we know to re-parse the original USX data.</summary>
		[XmlAttribute("usxparserversion")]
		[DefaultValue(0)]
		public int ParserVersion { get; set; }

		/// <summary>
		/// If we attempt to upgrade the project, and the user decides to opt out,
		/// we need to store the version he opted out of so we don't ask again.</summary>
		[XmlAttribute("parserupgradeoptoutversion")]
		[DefaultValue(0)]
		public int ParserUpgradeOptOutVersion { get; set; }

		/// <summary>
		/// We add this when we parse the USX to create a script.
		/// This tells us the original (local) path of the Text Release Bundle file used to create this project.
		/// </summary>
		[XmlAttribute("origdblpath")]
		public string OriginalReleaseBundlePath
		{
			get => m_originalReleaseBundlePath;
			set
			{
				if (!String.IsNullOrEmpty(ParatextProjectId))
					throw new InvalidOperationException("A Glyssen project cannot have both a Text Release Bundle and a Paratext project as its source.");
				m_originalReleaseBundlePath = value;
			}
		}

		/// <summary>
		/// We add this when we parse the USX to create a script.
		/// This tells us the Paratext Project (short name) used to create this project.
		/// </summary>
		[XmlAttribute("sourceparatextproj")]
		public string ParatextProjectId
		{
			get => m_paratextProjectId;
			set
			{
				if (!String.IsNullOrEmpty(OriginalReleaseBundlePath))
					throw new InvalidOperationException("A Glyssen project cannot have both a Text Release Bundle and a Paratext project as its source.");
				m_paratextProjectId = value;
			}
		}

		/// <summary>
		/// We use this to know if character assignments should be reprocessed.
		/// </summary>
		[XmlAttribute("controlfileversion")]
		public int ControlFileVersion { get; set; }

		/// <summary>
		/// If true, the project is hidden from the user unless he chooses to view inactive projects
		/// </summary>
		[XmlAttribute("hiddenbydefault")]
		[DefaultValue(false)]
		public bool Inactive { get; set; }

		[XmlElement("projectStatus")]
		public ProjectStatus ProjectStatus = new ProjectStatus();

		[XmlElement("characterGroupGenerationPreferences")]
		public CharacterGroupGenerationPreferences CharacterGroupGenerationPreferences = new CharacterGroupGenerationPreferences();

		[XmlElement("projectDramatizationPreferences")]
		public ProjectDramatizationPreferences DramatizationPreferences = new ProjectDramatizationPreferences();

		private string m_originalReleaseBundlePath;
		private string m_paratextProjectId;

		// Needed for deserialization
		public GlyssenDblTextMetadata()
		{
		}

		/// <summary>
		/// If a project does not come with a versification file, this is the name of the standard versification to be used.
		/// </summary>
		[XmlAttribute("versification")]
		[DefaultValue("English")]
		public string Versification { get; set; }

		[XmlAttribute("chapterannouncement")]
		[DefaultValue(ChapterAnnouncement.PageHeader)]
		public ChapterAnnouncement ChapterAnnouncementStyle { get; set; }

		[XmlAttribute("firstchapterannouncement")]
		[DefaultValue(false)]
		public bool IncludeChapterAnnouncementForFirstChapter { get; set; }

		[XmlAttribute("singlechapterannouncement")]
		[DefaultValue(false)]
		public bool IncludeChapterAnnouncementForSingleChapterBooks { get; set; }

		/// <summary>
		/// Gets the revision number from a standard DBL bundle. If this bundle is an ad-hoc bundle created by Paratext,
		/// this will instead be the (Mercurial) changeset id (which is a GUID). If this is metadata created from a live
		/// Paratext project, then it's just a sequence number telling how many times we've pulled in new data from Paratext
		/// (probably not terribly useful).
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

		/// <summary>
		/// The reference text type for this project. Note that this is poorly named in the XML. Historically,
		/// a reference text could be identified by type alone. With the added support for proprietary reference
		/// texts, we now also need the ProprietaryReferenceTextIdentifier.
		/// </summary>
		[XmlAttribute("referenceTextType")]
		[DefaultValue(ReferenceTextType.English)]
		public ReferenceTextType ReferenceTextType
		{
			get { return m_referenceTextType; }
			set { m_referenceTextType = value; }
		}

		/// <summary>
		/// The reference text for this project.
		/// </summary>
		[XmlAttribute("proprietaryReferenceTextIdentifier")]
		public string ProprietaryReferenceTextIdentifier { get; set; }
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
		/// <summary>
		/// The reference text for this project. Historically, Glyssen shipped with a bunch of reference text
		/// languages, but we had to remove most of them because of copyright issues. These can now be accessed as
		/// custom reference texts if the user has them installed. To keep from "breaking" existing projects, the
		/// setter here will attempt to determine if the reference text is available, and if so will migrate the
		/// settings to point to the custom reference text.
		/// </summary>
		[XmlAttribute("referenceText")]
		[DefaultValue(null)]
		public string ReferenceText_DeprecatedXml
		{
			get { return null; }
			set
			{
				if (value == null || ReferenceTextType != ReferenceTextType.English)
					return;

				if (value == "English")
					ReferenceTextType = ReferenceTextType.English;
				if (value == "Russian")
					ReferenceTextType = ReferenceTextType.Russian;
				else
				{
					ReferenceTextType = ReferenceTextType.Custom;
					ProprietaryReferenceTextIdentifier = value;
				}
			}
		}

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
		public BookBlockIndices AssignCharacterBlock { get; set; }

		[XmlElement("assignCharacterMode")]
		public BlocksToDisplay AssignCharacterMode { get; set; }

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

		[XmlElement("lastExportLocation")]
		[DefaultValue("")]
		public string LastExportLocation { get; set; }
	}
}
