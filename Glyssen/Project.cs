using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Analysis;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Quote;
using Glyssen.VoiceActor;
using L10NSharp;
using Paratext;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.Extensions;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.Windows.Forms.FileSystem;
using SIL.WritingSystems;
using SIL.Xml;
using ScrVers = Paratext.ScrVers;

namespace Glyssen
{
	public class Project
	{
		public const string kProjectFileExtension = ".glyssen";
		private const string kBookScriptFileExtension = ".xml";
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";
		private const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
		private const string kCharacterGroupFileName = "CharacterGroups.xml";

		private const double kUsxPercent = 0.25;
		private const double kGuessPercent = 0.10;
		private const double kQuotePercent = 0.65;

		private readonly GlyssenDblTextMetadata m_metadata;
		private readonly List<BookScript> m_books = new List<BookScript>();
		private readonly ScrVers m_vers;
		private string m_recordingProjectName;
		private int m_usxPercentComplete;
		private int m_guessPercentComplete;
		private int m_quotePercentComplete;
		private ProjectState m_projectState;
		private ProjectAnalysis m_analysis;
		private WritingSystemDefinition m_wsDefinition;
		private QuoteSystem m_quoteSystem;
		// Don't want to hound the user more than once per launch per project
		private bool m_fontInstallationAttempted;
		private VoiceActorList m_voiceActorList;
		private CharacterGroupList m_characterGroupList;
		private readonly ISet<CharacterDetail> m_projectCharacterDetailData;
		private bool m_projectFileIsWritable = true;

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public event EventHandler<ProjectStateChangedEventArgs> ProjectStateChanged;
		public event EventHandler QuoteParseCompleted;
		public event EventHandler AnalysisCompleted;
		public event EventHandler CharacterGroupCollectionChanged;

		private Func<string, string> GetBookName { get; set; }

		private Project(GlyssenDblTextMetadata metadata, string recordingProjectName = null, bool installFonts = false, WritingSystemDefinition ws = null)
		{
			m_metadata = metadata;
			SetBlockGetChapterAnnouncement(ChapterAnnouncementStyle);
			m_wsDefinition = ws;
			m_recordingProjectName = recordingProjectName ?? GetDefaultRecordingProjectName(m_metadata.Identification.Name);
			ProjectCharacterVerseData = new ProjectCharacterVerseData(ProjectCharacterVerseDataPath);
			m_projectCharacterDetailData = ProjectCharacterDetailData.Load(ProjectCharacterDetailDataPath);
			if (File.Exists(VersificationFilePath))
				m_vers = LoadVersification(VersificationFilePath);
			if (installFonts)
				InstallFontsIfNecessary();
		}

		public Project(GlyssenBundle bundle, string recordingProjectName = null, Project projectBeingUpdated = null) :
			this(bundle.Metadata, recordingProjectName, false, bundle.WritingSystemDefinition ?? (projectBeingUpdated != null ? projectBeingUpdated.WritingSystem : null))
		{
			Directory.CreateDirectory(ProjectFolder);
			if (bundle.WritingSystemDefinition != null && bundle.WritingSystemDefinition.QuotationMarks != null && bundle.WritingSystemDefinition.QuotationMarks.Any())
			{
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				ConvertContinuersToParatextAssumptions();
			}
			bundle.CopyFontFiles(LanguageFolder);
			InstallFontsIfNecessary();
			bundle.CopyVersificationFile(VersificationFilePath);
			try
			{
				m_vers = LoadVersification(VersificationFilePath);
			}
			catch (InvalidVersificationLineException)
			{
				DeleteProjectFolderAndEmptyContainingFolders(ProjectFolder);
				throw;
			}
			UserDecisionsProject = projectBeingUpdated;
			PopulateAndParseBooks(bundle);
		}

		/// <summary>
		/// Used only for sample project and in tests.
		/// </summary>
		internal Project(GlyssenDblTextMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet, WritingSystemDefinition ws)
			: this(metadata, ws: ws)
		{
			AddAndParseBooks(books, stylesheet);

			Directory.CreateDirectory(ProjectFolder);
			File.WriteAllText(VersificationFilePath, Resources.EnglishVersification);
			m_vers = LoadVersification(VersificationFilePath);
		}

		private static string ProjectsBaseFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Program.kCompany, Program.kProduct);
			}
		}

		public static IEnumerable<string> AllPublicationFolders
		{
			get
			{
				return Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories);
			}
		}

		public static IEnumerable<string> AllRecordingProjectFolders
		{
			get
			{
				return AllPublicationFolders.SelectMany(Directory.GetDirectories);
			}
		}

        public string AudioStockNumber
        {
            get { return m_metadata.AudioStockNumber; }
        }

        public string Id
        {
            get { return m_metadata.Id; }
        }

		public string Name
		{
			get { return m_recordingProjectName; }
		}

		public string PublicationName
		{
			get { return m_metadata.Identification == null ? null : m_metadata.Identification.Name; }
		}

		public string LanguageIsoCode
		{
			get { return m_metadata.Language.Iso; }
		}

		public string LanguageName
		{
			get { return m_metadata.Language.Name; }
		}

		public string FontFamily
		{
			get { return m_metadata.FontFamily; }
		}

		public int FontSizeInPoints
		{
			get { return m_metadata.FontSizeInPoints; }
		}

		public int FontSizeUiAdjustment
		{
			get { return m_metadata.FontSizeUiAdjustment; }
			set { m_metadata.FontSizeUiAdjustment = value; }
		}

		public bool RightToLeftScript
		{
			get { return m_metadata.Language.ScriptDirection == "RTL"; }
		}

		public ScrVers Versification
		{
			get  { return m_vers; }
		}

		public ChapterAnnouncement ChapterAnnouncementStyle
		{
			get { return m_metadata.ChapterAnnouncementStyle; }
			private set
			{
				m_metadata.ChapterAnnouncementStyle = value;
				SetBlockGetChapterAnnouncement(value);
			}
		}

		public bool SkipChapterAnnouncementForFirstChapter
		{
			get { return !m_metadata.IncludeChapterAnnouncementForFirstChapter; }
		}

		public bool SkipChapterAnnouncementForSingleChapterBooks
		{
			get { return !m_metadata.IncludeChapterAnnouncementForSingleChapterBooks; }
		}

		public void SetBlockGetChapterAnnouncement(ChapterAnnouncement announcementStyle)
		{
			switch (announcementStyle)
			{
				case ChapterAnnouncement.ChapterLabel:
					GetBookName = null;
					Block.FormatChapterAnnouncement = null;
					return;
				case ChapterAnnouncement.PageHeader:
					GetBookName = (bookId) =>
					{
						var book = Books.FirstOrDefault(b => b.BookId == bookId);
						return (book == null) ? null : book.PageHeader;
					};
					break;
				case ChapterAnnouncement.MainTitle1:
					GetBookName = (bookId) =>
					{
						var book = Books.FirstOrDefault(b => b.BookId == bookId);
						return (book == null) ? null : book.MainTitle;
					};
					break;
				case ChapterAnnouncement.ShortNameFromMetadata:
					GetBookName = (bookId) =>
					{
						var book = m_metadata.AvailableBooks.FirstOrDefault(b => b.Code == bookId);
						return (book == null) ? null : book.ShortName;
					};
					break;
				case ChapterAnnouncement.LongNameFromMetadata:
					GetBookName = (bookId) =>
					{
						var book = m_metadata.AvailableBooks.FirstOrDefault(b => b.Code == bookId);
						return (book == null) ? null : book.LongName;
					};
					break;
			}
			Block.FormatChapterAnnouncement = GetFormattedChapterAnnouncement;
		}

		public string GetFormattedChapterAnnouncement(string bookCode, int chapterNumber)
		{
			if (GetBookName == null)
				return null;
			var bookName = GetBookName(bookCode);
			if (string.IsNullOrWhiteSpace(bookName))
				return null;

			StringBuilder bldr = new StringBuilder();
			bldr.Append(bookName);
			bldr.Append(" ");
			bldr.Append(chapterNumber);
			return bldr.ToString();
		}

		public static ScrVers LoadVersification(string vrsPath)
		{
			return Paratext.Versification.Table.Load(vrsPath, LocalizationManager.GetString("Project.DefaultCustomVersificationName",
				"custom", "Used as the versification name when a the versification file does not contain a name."));
		}

		public ProjectStatus Status
		{
			get { return m_metadata.ProjectStatus; }
		}

		public ProjectAnalysis ProjectAnalysis
		{
			get { return m_analysis ?? (m_analysis = new ProjectAnalysis(this)); }
		}

		public QuoteSystem QuoteSystem
		{
			get
			{
				if (m_quoteSystem != null)
					return m_quoteSystem;
				if (WritingSystem != null && WritingSystem.QuotationMarks != null && WritingSystem.QuotationMarks.Any())
					return m_quoteSystem = new QuoteSystem(WritingSystem.QuotationMarks);
				return null;
			}
			set
			{
				bool quoteSystemBeingSetForFirstTime = QuoteSystem == null;
				bool quoteSystemChanged = m_quoteSystem != value;

				if (IsQuoteSystemReadyForParse && ProjectState == ProjectState.NeedsQuoteSystemConfirmation)
				{
					m_quoteSystem = value;
					DoQuoteParse();
				}
				else if (quoteSystemChanged && !quoteSystemBeingSetForFirstTime)
				{
					// These need to happen in this order
					Save();
					CreateBackup("Backup before quote system change");
					m_quoteSystem = value;
					HandleQuoteSystemChanged();
				}
				else
				{
					m_quoteSystem = value;
				}

				WritingSystem.QuotationMarks.Clear();
				WritingSystem.QuotationMarks.AddRange(m_quoteSystem.AllLevels);
			}
		}

		public QuoteSystemStatus QuoteSystemStatus
		{
			get { return m_metadata.ProjectStatus.QuoteSystemStatus; }
			set { m_metadata.ProjectStatus.QuoteSystemStatus = value; }
		}

		public bool IsQuoteSystemReadyForParse { get { return (QuoteSystemStatus & QuoteSystemStatus.ParseReady) != 0; } }

		public DateTime QuoteSystemDate
		{
			get { return m_metadata.ProjectStatus.QuoteSystemDate; }
		}

		public BookSelectionStatus BookSelectionStatus
		{
			get
			{
				// don't make the user open the select books dialog if there is only 1 book
				if ((m_metadata.ProjectStatus.BookSelectionStatus == BookSelectionStatus.UnReviewed) &&
					(AvailableBooks.Count == 1) && (IncludedBooks.Count == 1))
				{
					m_metadata.ProjectStatus.BookSelectionStatus = BookSelectionStatus.Reviewed;
				}

				return m_metadata.ProjectStatus.BookSelectionStatus;
			}
			set { m_metadata.ProjectStatus.BookSelectionStatus = value; }
		}

		public ProjectSettingsStatus ProjectSettingsStatus
		{
			get { return m_metadata.ProjectStatus.ProjectSettingsStatus; }
			set { m_metadata.ProjectStatus.ProjectSettingsStatus = value; }
		}

		public CharacterGroupGenerationPreferences CharacterGroupGenerationPreferences
		{
			get { return m_metadata.CharacterGroupGenerationPreferences;  }
			set { m_metadata.CharacterGroupGenerationPreferences = value; }
		}

		public void SetDefaultCharacterGroupGenerationPreferences()
		{
			if (!CharacterGroupGenerationPreferences.IsSetByUser)
			{
				CharacterGroupGenerationPreferences.NumberOfMaleNarrators = BiblicalAuthors.GetAuthorCount(IncludedBooks.Select(b => b.BookId));
				CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			}
			if (CharacterGroupGenerationPreferences.CastSizeOption == CastSizeRow.NotSet)
			{
				CharacterGroupGenerationPreferences.CastSizeOption = VoiceActorList.ActiveActors.Any() ? CastSizeRow.MatchVoiceActorList : CastSizeRow.Recommended;
			}
		}

		public void SetCharacterGroupGenerationPreferencesToValidValues()
		{
			// We validate the values when the user can change them directly (in the Narration Preferences dialog),
			// but this handles when other factors are changed which could invalidate the user's choices.
			//
			// For example, the project might be a whole NT, and the user chooses to use 27 authors.
			// Later, the user may remove a book, but the requested number of authors is still 27 (which is now invalid).

			int includedBooksCount = IncludedBooks.Count;
			int numMale = CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			int numFemale = CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;

			if (numMale + numFemale > includedBooksCount)
			{
				int numNarratorsToDecriment = (numMale + numFemale) - includedBooksCount;
				if (numFemale >= numNarratorsToDecriment)
					CharacterGroupGenerationPreferences.NumberOfFemaleNarrators -= numNarratorsToDecriment;
				else
				{
					CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
					CharacterGroupGenerationPreferences.NumberOfMaleNarrators -= numNarratorsToDecriment - numFemale;
				}
			}
		}

		public IReadOnlyList<BookScript> Books { get { return m_books; } }

		public IReadOnlyList<BookScript> IncludedBooks
		{
			get
			{
				return (from book in Books
						where AvailableBooks.Where(ab => ab.IncludeInScript).Select(ab => ab.Code).Contains(book.BookId)
						select book).ToList();
			}
		}

		public SIL.ObjectModel.IReadOnlyList<Book> AvailableBooks
		{
			get { return m_metadata.AvailableBibleBooks; }
		}

		public string OriginalBundlePath
		{
			get { return m_metadata.OriginalPathBundlePath; }
			set { m_metadata.OriginalPathBundlePath = value; }
		}

		public readonly ProjectCharacterVerseData ProjectCharacterVerseData;

		public IReadOnlyDictionary<string, CharacterDetail> AllCharacterDetailDictionary
		{
			get
			{
				Dictionary<string, CharacterDetail> characterDetails = new Dictionary<string, CharacterDetail>(CharacterDetailData.Singleton.GetDictionary());
				characterDetails.AddRange(m_projectCharacterDetailData.ToDictionary(k => k.CharacterId));
				return characterDetails;
			}
		}

		public void AddProjectCharacterDetail(CharacterDetail characterDetail)
		{
			m_projectCharacterDetailData.Add(characterDetail);
		}

		public void UpdateSettings(ProjectSettingsViewModel model)
		{
			Debug.Assert(!String.IsNullOrEmpty(model.RecordingProjectName));
			var newPath = GetProjectFolderPath(model.IsoCode, model.PublicationId, model.RecordingProjectName);
			if (newPath != ProjectFolder)
			{
				Directory.Move(ProjectFolder, newPath);
				m_recordingProjectName = model.RecordingProjectName;
			}
		    m_metadata.AudioStockNumber = model.AudioStockNumber;
			m_metadata.FontFamily = model.WsModel.CurrentDefaultFontName;
			m_metadata.FontSizeInPoints = (int) model.WsModel.CurrentDefaultFontSize;
			m_metadata.Language.ScriptDirection = model.WsModel.CurrentRightToLeftScript ? "RTL" : "LTR";
			ChapterAnnouncementStyle = model.ChapterAnnouncementStyle;
			m_metadata.IncludeChapterAnnouncementForFirstChapter = !model.SkipChapterAnnouncementForFirstChapter;
			m_metadata.IncludeChapterAnnouncementForSingleChapterBooks = !model.SkipChapterAnnouncementForSingleChapterBooks;
		}

		public Project UpdateProjectFromBundleData(GlyssenBundle bundle)
		{
			if ((ProjectState & ProjectState.ReadyForUserInteraction) == 0)
				throw new InvalidOperationException("Project not in a valid state to update from text release bundle. ProjectState = " + ProjectState);

			// If we're updating the project in place, we need to make a backup. Otherwise, if it's moving to a new
			// location, just mark the existing one as inactive.
			bool moving = (ProjectFilePath != GetProjectFilePath(bundle.LanguageIso, bundle.Id, m_recordingProjectName));
			if (moving)
				m_metadata.Inactive = true;
			Save();
			if (!moving)
				CreateBackup("Backup before updating from new bundle");

			bundle.Metadata.CopyGlyssenModifiableSettings(m_metadata);

			// PG-612: renaming bundle causes loss of assignments
			bundle.Metadata.ControlFileVersion = m_metadata.ControlFileVersion;

			CopyQuoteMarksIfAppropriate(bundle.WritingSystemDefinition, bundle.Metadata);

			return new Project(bundle, m_recordingProjectName, this);
		}

		// internal for testing
		internal void CopyQuoteMarksIfAppropriate(WritingSystemDefinition targetWs, GlyssenDblTextMetadata targetMetadata)
		{
			if (targetWs == null)
				return;

			// If the target has no quote information, add it.
			if (!targetWs.QuotationMarks.Any())
			{
				targetWs.QuotationMarks.AddRange(WritingSystem.QuotationMarks);
			}
			// Assumes QuoteSystemStatus has already been set from source metadata.
			// If the user hasn't changed the QuoteSystemStatus, keep what is in the target.
			else if ((targetMetadata.ProjectStatus.QuoteSystemStatus & QuoteSystemStatus.Obtained) > 0)
			{
			}
			// Copy if source has more detail
			else
			{
				bool copy = false;
				var sourceLevelsCount = WritingSystem.QuotationMarks.Count;
				var targetLevelsCount = targetWs.QuotationMarks.Count;

				if (sourceLevelsCount > targetLevelsCount)
				{
					copy = true;
					for (int i = 0; i < targetLevelsCount; i++)
					{
						if (!targetWs.QuotationMarks[i].Equals(WritingSystem.QuotationMarks[i]))
						{
							copy = false;
							break;
						}
					}
				}
				if (copy)
				{
					targetWs.QuotationMarks.Clear();
					targetWs.QuotationMarks.AddRange(WritingSystem.QuotationMarks);
				}
				else
				{
					targetMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
				}
			}
		}

		private int PercentInitialized { get; set; }

		public ProjectState ProjectState
		{
			get { return m_projectState; }
			private set
			{
				if (m_projectState == value)
					return;
				m_projectState = value;
				if (ProjectStateChanged != null)
					ProjectStateChanged(this, new ProjectStateChangedEventArgs { ProjectState = m_projectState });
			}
		}

		public string ProjectSummary
		{
			get
			{
				var sb = new StringBuilder(Name);
				if (!string.IsNullOrEmpty(m_metadata.Language.Name))
					sb.Append(", ").Append(m_metadata.Language.Name);
				if (!string.IsNullOrEmpty(LanguageIsoCode))
					sb.Append(" (").Append(LanguageIsoCode).Append(")");
				if (!string.IsNullOrEmpty(PublicationName))
					sb.Append(", ").Append(PublicationName);
				if (!string.IsNullOrEmpty(Id))
					sb.Append(" (").Append(Id).Append(")");
				return sb.ToString();
			}
		}

		public string SettingsSummary
		{
			get
			{
				var sb = new StringBuilder(QuoteSystem.ShortSummary);
				sb.Append(", ").Append(FontFamily);
				sb.Append(", ").Append(FontSizeInPoints).Append(LocalizationManager.GetString("WritingSystem.Points", "pt", "Units appended to font size to represent points"));
				sb.Append(", ").Append(RightToLeftScript ? LocalizationManager.GetString("WritingSystem.RightToLeft", "Right-to-left", "Describes a writing system") :
					LocalizationManager.GetString("WritingSystem.LeftToRight", "Left-to-right", "Describes a writing system"));
				return sb.ToString();
			}
		}

		public string BookSelectionSummary { get { return IncludedBooks.BookSummary(); } }

		/// <summary>
		/// If this is set, the user decisions in it will be applied when the quote parser is done
		/// </summary>
		private Project UserDecisionsProject { get; set; }

		public VoiceActorList VoiceActorList
		{
			get { return m_voiceActorList ?? (m_voiceActorList = LoadVoiceActorInformationData()); }
		}

		public CharacterGroupList CharacterGroupList
		{
			get { return m_characterGroupList ?? (m_characterGroupList = LoadCharacterGroupData()); }
		}

		public bool CharacterGroupListPreviouslyGenerated
		{
			get { return CharacterGroupList.CharacterGroups.Any(); }
		}

		public bool IsVoiceActorScriptReady
		{
			get { return IsVoiceActorAssignmentsComplete && EveryAssignedGroupHasACharacter; }
		}

		public bool IsVoiceActorAssignmentsComplete
		{
			get
			{
				var groups = CharacterGroupList.CharacterGroups;
				return groups.Count > 0 && groups.All(t => t.IsVoiceActorAssigned);
			}
		}

		public bool EveryAssignedGroupHasACharacter
		{
			get { return CharacterGroupList.AssignedGroups.All(g => g.CharacterIds.Count != 0); }
		}

		public IEnumerable<VoiceActor.VoiceActor> UnusedActors
		{
			get { return VoiceActorList.ActiveActors.Where(actor => !CharacterGroupList.HasVoiceActorAssigned(actor.Id)); }
		}

		public bool HasUnappliedSplits()
		{
			return IncludedBooks.Any(b => b.UnappliedSplits.Any());
		}

		internal void ClearAssignCharacterStatus()
		{
			Status.AssignCharacterMode = BlocksToDisplay.NeedAssignments;
			Status.AssignCharacterBlock = new BookBlockIndices();
		}

		public static Project Load(string projectFilePath)
		{
			Project existingProject = LoadExistingProject(projectFilePath);

			if (existingProject == null)
				return null;

			if (!existingProject.IsSampleProject && existingProject.m_metadata.ParserVersion != Settings.Default.ParserVersion)
			{
				bool upgradeProject = true;
				if (!File.Exists(existingProject.OriginalBundlePath))
				{
					upgradeProject = false;
					if (Settings.Default.ParserVersion > existingProject.m_metadata.ParserUpgradeOptOutVersion)
					{
						string msg = string.Format(LocalizationManager.GetString("Project.ParserUpgradeBundleMissingMsg", "The splitting engine has been upgraded. To make use of the new engine, the original text bundle must be available, but it is not in the original location ({0})."), existingProject.OriginalBundlePath) +
							Environment.NewLine + Environment.NewLine +
							LocalizationManager.GetString("Project.LocateBundleYourself", "Would you like to locate the text bundle yourself?");
						string caption = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
						if (DialogResult.Yes == MessageBox.Show(msg, caption, MessageBoxButtons.YesNo))
							upgradeProject = SelectProjectDlg.GiveUserChanceToFindOriginalBundle(existingProject);
						if (!upgradeProject)
							existingProject.m_metadata.ParserUpgradeOptOutVersion = Settings.Default.ParserVersion;
					}
				}
				if (upgradeProject)
				{
					using (var bundle = new GlyssenBundle(existingProject.OriginalBundlePath))
					{
						var upgradedProject = new Project(existingProject.m_metadata, existingProject.m_recordingProjectName, ws: existingProject.WritingSystem);

						Analytics.Track("UpgradeProject", new Dictionary<string, string>
						{
							{"language", existingProject.LanguageIsoCode},
							{"ID", existingProject.Id},
							{"recordingProjectName", existingProject.Name},
							{"oldParserVersion", existingProject.m_metadata.ParserVersion.ToString(CultureInfo.InvariantCulture)},
							{"newParserVersion", Settings.Default.ParserVersion.ToString(CultureInfo.InvariantCulture)}
						});

						upgradedProject.UserDecisionsProject = existingProject;
						upgradedProject.PopulateAndParseBooks(bundle);
						upgradedProject.m_metadata.ParserVersion = Settings.Default.ParserVersion;
						return upgradedProject;
					}
				}
			}

			existingProject.InitializeLoadedProject();
			return existingProject;
		}

		public static void SetHiddenFlag(string projectFilePath, bool hidden)
		{
			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					LocalizationManager.GetString("File.ProjectCouldNotBeModified", "Project could not be modified: {0}"), projectFilePath);
				return;
			}
			metadata.Inactive = hidden;
			new Project(metadata, GetRecordingProjectNameFromProjectFilePath(projectFilePath)).Save();
		}

		public static void DeleteProjectFolderAndEmptyContainingFolders(string projectFolder, bool confirmAndRecycle = false)
		{
			if (confirmAndRecycle)
			{
				if (!ConfirmRecycleDialog.ConfirmThenRecycle(string.Format("Standard format project \"{0}\"", projectFolder), projectFolder))
					return;
			}
			else if (Directory.Exists(projectFolder))
				Directory.Delete(projectFolder, true);
			var parent = Path.GetDirectoryName(projectFolder);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
			parent = Path.GetDirectoryName(parent);
			if (Directory.Exists(parent) && !Directory.GetFileSystemEntries(parent).Any())
				Directory.Delete(parent);
		}

		private static string GetRecordingProjectNameFromProjectFilePath(string path)
		{
			return Path.GetFileName(Path.GetDirectoryName(path));
		}

		private int UpdatePercentInitialized()
		{
			return PercentInitialized = (int)(m_usxPercentComplete * kUsxPercent + m_guessPercentComplete * kGuessPercent + m_quotePercentComplete * kQuotePercent);
		}

		private static Project LoadExistingProject(string projectFilePath)
		{
			// PG-433, 04 JAN 2015, PH: Let the user know if the project file is not writable
			var isWritable = !FileUtils.IsFileLocked(projectFilePath);
			if (!isWritable)
			{
				MessageBox.Show(LocalizationManager.GetString("Project.NotWritableMsg", "The project file is not writable. No changes will be saved."));
			}

			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					LocalizationManager.GetString("File.ProjectMetadataInvalid", "Project could not be loaded: {0}"), projectFilePath);
				return null;
			}

			var project = new Project(metadata, GetRecordingProjectNameFromProjectFilePath(projectFilePath), true)
			{
				ProjectFileIsWritable = isWritable
			};

			var projectDir = Path.GetDirectoryName(projectFilePath);
			Debug.Assert(projectDir != null);
			string[] files = Directory.GetFiles(projectDir, "???" + kBookScriptFileExtension);
			for (int i = 1; i <= BCVRef.LastBook; i++)
			{
				string bookCode = BCVRef.NumberToBookCode(i);
				string possibleFileName = Path.Combine(projectDir, bookCode + kBookScriptFileExtension);
				if (files.Contains(possibleFileName))
					project.m_books.Add(XmlSerializationHelper.DeserializeFromFile<BookScript>(possibleFileName));
			}
			project.RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();
			return project;
		}

		private void RemoveAvailableBooksThatDoNotCorrespondToExistingBooks()
		{
			for (int i = 0; i < m_metadata.AvailableBooks.Count; i++)
			{
				if (!m_books.Any(b => b.BookId == m_metadata.AvailableBooks[i].Code))
					m_metadata.AvailableBooks.RemoveAt(i--);
			}
		}

		private void InitializeLoadedProject()
		{
			m_usxPercentComplete = 100;
			if (QuoteSystem == null)
			{
				GuessAtQuoteSystem();
				UpdateControlFileVersion();
				return;
			}
			m_guessPercentComplete = 100;

			if (IsSampleProject)
			{
				ProjectSettingsStatus = ProjectSettingsStatus.Reviewed;
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				BookSelectionStatus = BookSelectionStatus.Reviewed;
			}

			if (!IsQuoteSystemReadyForParse)
			{
				m_quotePercentComplete = 0;
				UpdatePercentInitialized();
				ProjectState = ProjectState.NeedsQuoteSystemConfirmation;
				return;
			}
			m_quotePercentComplete = 100;
			if (m_metadata.ControlFileVersion != ControlCharacterVerseData.Singleton.ControlFileVersion)
			{
				const int kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan = 78;
				new CharacterAssigner(new CombinedCharacterVerseData(this)).AssignAll(m_books, Versification,
					m_metadata.ControlFileVersion < kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan);

				UpdateControlFileVersion();
			}
			UpdatePercentInitialized();
			ProjectState = ProjectState.FullyInitialized;
			UpdateControlFileVersion();
			Analyze();
		}

		private void UpdateControlFileVersion()
		{
			if (m_metadata.ControlFileVersion != 0)
				ProjectDataMigrator.MigrateProjectData(this, m_metadata.ControlFileVersion);
			if (m_metadata.ControlFileVersion == 0 || ProjectState == ProjectState.FullyInitialized)
				m_metadata.ControlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;
		}

		private void ApplyUserDecisions(Project sourceProject)
		{
			foreach (var targetBookScript in m_books)
			{
				var sourceBookScript = sourceProject.m_books.SingleOrDefault(b => b.BookId == targetBookScript.BookId);
				if (sourceBookScript != null)
					targetBookScript.ApplyUserDecisions(sourceBookScript, Versification);
			}
			Analyze();
		}

		private void PopulateAndParseBooks(ITextBundle bundle)
		{
			AddAndParseBooks(bundle.UsxBooksToInclude, bundle.Stylesheet);
		}

		private void AddAndParseBooks(IEnumerable<UsxDocument> books, IStylesheet stylesheet)
		{
			ProjectState = ProjectState.Initial;
			var usxWorker = new BackgroundWorker { WorkerReportsProgress = true };
			usxWorker.DoWork += UsxWorker_DoWork;
			usxWorker.RunWorkerCompleted += UsxWorker_RunWorkerCompleted;
			usxWorker.ProgressChanged += UsxWorker_ProgressChanged;

			object[] parameters = { books, stylesheet };
			usxWorker.RunWorkerAsync(parameters);
		}

		private void UsxWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var parameters = (object[])e.Argument;
			var books = (IEnumerable<UsxDocument>)parameters[0];
			var stylesheet = (IStylesheet)parameters[1];

			e.Result = UsxParser.ParseProject(books, stylesheet, sender as BackgroundWorker);
		}

		private void UsxWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			var bookScripts = (List<BookScript>)e.Result;

			// This code is an attempt to figure out how we are getting null reference exceptions when using the objects in the list (See PG-275 & PG-287)
			foreach (var bookScript in bookScripts)
				if (bookScript == null || bookScript.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = string.Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException(string.Format("{0} Number of BookScripts: {1}. BookScripts which are NOT null: {2}", initialMessage, bookScripts.Count, nonNullBookScriptsStr));
				}

			m_books.AddRange(bookScripts);
			m_metadata.ParserVersion = Settings.Default.ParserVersion;
			if (m_books.All(b => String.IsNullOrEmpty(b.PageHeader)))
				ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			UpdateControlFileVersion();
			RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();

			if (QuoteSystem == null)
				GuessAtQuoteSystem();
			else if (IsQuoteSystemReadyForParse)
				DoQuoteParse();
		}

		private void UsxWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_usxPercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(UpdatePercentInitialized(), null);
			OnReport(pe);
		}

		private void GuessAtQuoteSystem()
		{
			ProjectState = ProjectState.UsxComplete;
			var guessWorker = new BackgroundWorker { WorkerReportsProgress = true };
			guessWorker.DoWork += GuessWorker_DoWork;
			guessWorker.RunWorkerCompleted += GuessWorker_RunWorkerCompleted;
			guessWorker.ProgressChanged += GuessWorker_ProgressChanged;
			guessWorker.RunWorkerAsync();
		}

		private void GuessWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool certain;
			e.Result = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, m_books, Versification, out certain, sender as BackgroundWorker);
		}

		private void GuessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			QuoteSystemStatus = QuoteSystemStatus.Guessed;
			QuoteSystem = (QuoteSystem)e.Result;

			Save();
		}

		private void GuessWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_guessPercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(UpdatePercentInitialized(), null);
			OnReport(pe);
		}

		private void DoQuoteParse()
		{
			ProjectState = ProjectState.Parsing;
			var quoteWorker = new BackgroundWorker { WorkerReportsProgress = true };
			quoteWorker.DoWork += QuoteWorker_DoWork;
			quoteWorker.RunWorkerCompleted += QuoteWorker_RunWorkerCompleted;
			quoteWorker.ProgressChanged += QuoteWorker_ProgressChanged;
			quoteWorker.RunWorkerAsync();
		}

		private void QuoteWorker_DoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			QuoteParser.ParseProject(this, sender as BackgroundWorker);
		}

		private void QuoteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Debug.WriteLine(e.Error.InnerException.Message + e.Error.InnerException.StackTrace);
				throw e.Error;
			}

			UpdateControlFileVersion();
			if (UserDecisionsProject != null)
			{
				ApplyUserDecisions(UserDecisionsProject);
				UserDecisionsProject = null;
			}
			else
				Analyze();

			Save();

			if (QuoteParseCompleted != null)
				QuoteParseCompleted(this, new EventArgs());
		}

		private void QuoteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_quotePercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(UpdatePercentInitialized(), null);
			OnReport(pe);
		}

		public static string GetProjectFilePath(string langId, string publicationId, string recordingProjectId)
		{
			return Path.Combine(GetProjectFolderPath(langId, publicationId, recordingProjectId), langId + kProjectFileExtension);
		}

		public static string GetDefaultProjectFilePath(IBundle bundle)
		{
			return GetProjectFilePath(bundle.LanguageIso, bundle.Id, GetDefaultRecordingProjectName(bundle));
		}

		public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectId)
		{
			return Path.Combine(ProjectsBaseFolder, langId, publicationId, recordingProjectId);
		}

		public static string GetPublicationFolderPath(IBundle bundle)
		{
			return Path.Combine(ProjectsBaseFolder, bundle.LanguageIso, bundle.Id);
		}

		public static string GetLanguageFolderPath(IBundle bundle)
		{
			return Path.Combine(ProjectsBaseFolder, bundle.LanguageIso);
		}

		public static string GetLanguageFolderPath(string langId)
		{
			return Path.Combine(ProjectsBaseFolder, langId);
		}

		public string ProjectFilePath
		{
			get { return GetProjectFilePath(m_metadata.Language.Iso, m_metadata.Id, m_recordingProjectName); }
		}

		private string ProjectCharacterVerseDataPath
		{
			get { return Path.Combine(ProjectFolder, kProjectCharacterVerseFileName); }
		}

		private string ProjectCharacterDetailDataPath
		{
			get { return Path.Combine(ProjectFolder, kProjectCharacterDetailFileName); }
		}

		private string VersificationFilePath
		{
			get { return Path.Combine(ProjectFolder, DblBundleFileUtils.kVersificationFileName); }
		}

		private string LdmlFilePath
		{
			get
			{
				string languagecode = LanguageIsoCode;
				if (!IetfLanguageTag.IsValid(languagecode))
					languagecode = WellKnownSubtags.UnlistedLanguage;
				return Path.Combine(ProjectFolder, languagecode + DblBundleFileUtils.kUnzippedLdmlFileExtension);
			}
		}

		private string ProjectFolder
		{
			get { return GetProjectFolderPath(m_metadata.Language.Iso, m_metadata.Id, m_recordingProjectName); }
		}

		private string LanguageFolder
		{
			get { return GetLanguageFolderPath(m_metadata.Language.Iso); }
		}

		public void Analyze()
		{
			ProjectAnalysis.AnalyzeQuoteParse();

			Analytics.Track("ProjectAnalysis", new Dictionary<string, string>
			{
				{ "language", LanguageIsoCode },
				{ "ID", Id },
				{ "recordingProjectName", Name },
				{ "TotalBlocks", ProjectAnalysis.TotalBlocks.ToString(CultureInfo.InvariantCulture) },
				{ "UserPercentAssigned", ProjectAnalysis.UserPercentAssigned.ToString(CultureInfo.InvariantCulture) },
				{ "TotalPercentAssigned", ProjectAnalysis.TotalPercentAssigned.ToString(CultureInfo.InvariantCulture) },
				{ "PercentUnknown", ProjectAnalysis.PercentUnknown.ToString(CultureInfo.InvariantCulture) }
			});

			if (AnalysisCompleted != null)
				AnalysisCompleted(this, new EventArgs());
		}

		public void Save(bool saveCharacterGroups = false)
		{
			if (!m_projectFileIsWritable) return;

			Directory.CreateDirectory(ProjectFolder);

			m_metadata.LastModified = DateTime.Now;
			var projectPath = ProjectFilePath;
			Exception error;
			XmlSerializationHelper.SerializeToFile(projectPath, m_metadata, out error);
			if (error != null)
			{
				MessageBox.Show(error.Message);
				return;
			}
			Settings.Default.CurrentProject = projectPath;
			foreach (var book in m_books)
				SaveBook(book);
			SaveProjectCharacterVerseData();
			SaveProjectCharacterDetailData();
			SaveWritingSystem();
			if (saveCharacterGroups)
				SaveCharacterGroupData();
			ProjectState = !IsQuoteSystemReadyForParse ? ProjectState.NeedsQuoteSystemConfirmation : ProjectState.FullyInitialized;
		}

		public void SaveBook(BookScript book)
		{
			Exception error;
			XmlSerializationHelper.SerializeToFile(Path.ChangeExtension(Path.Combine(ProjectFolder, book.BookId), "xml"), book, out error);
			if (error != null)
				MessageBox.Show(error.Message);
		}

		public void SaveProjectCharacterVerseData()
		{
			ProjectCharacterVerseData.WriteToFile(ProjectCharacterVerseDataPath);
		}

		public void SaveProjectCharacterDetailData()
		{
			ProjectCharacterDetailData.WriteToFile(m_projectCharacterDetailData, ProjectCharacterDetailDataPath);
		}

		public void SaveCharacterGroupData()
		{
			m_characterGroupList.SaveToFile(Path.Combine(ProjectFolder, kCharacterGroupFileName));
		}

		public void SaveVoiceActorInformationData()
		{
			m_voiceActorList.SaveToFile(Path.Combine(ProjectFolder, kVoiceActorInformationFileName));
		}

		private CharacterGroupList LoadCharacterGroupData()
		{
			string path = Path.Combine(ProjectFolder, kCharacterGroupFileName);
			CharacterGroupList list;
			list = File.Exists(path) ? CharacterGroupList.LoadCharacterGroupListFromFile(path, this) : new CharacterGroupList();
			list.CharacterGroups.CollectionChanged += CharacterGroups_CollectionChanged;
			return list;
		}

		void CharacterGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (CharacterGroupCollectionChanged != null)
				CharacterGroupCollectionChanged(this, new EventArgs());
		}

		private VoiceActorList LoadVoiceActorInformationData()
		{
			string path = Path.Combine(ProjectFolder, kVoiceActorInformationFileName);
			if (File.Exists(path))
				return VoiceActorList.LoadVoiceActorListFromFile(path);
			return new VoiceActorList();
		}

		public VoiceActor.VoiceActor GetVoiceActorForCharacter(string characterId)
		{
			var charGroup = CharacterGroupList.CharacterGroups.FirstOrDefault(cg => cg.CharacterIds.Contains(characterId));
			if (charGroup == null)
				return null;
			return VoiceActorList.AllActors.FirstOrDefault(a => a.Id == charGroup.VoiceActorId);
		}

		public WritingSystemDefinition WritingSystem
		{
			get
			{
				if (m_wsDefinition != null)
					return m_wsDefinition;

				m_wsDefinition = new WritingSystemDefinition();
				if (File.Exists(LdmlFilePath))
					new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, m_wsDefinition);
				else
				{
					m_wsDefinition.Id = m_metadata.Language.Ldml;
					if (string.IsNullOrWhiteSpace(m_wsDefinition.Id))
						m_wsDefinition.Id = m_metadata.Language.Iso;
					m_wsDefinition.Language = m_wsDefinition.Id;
					if (m_wsDefinition.Language.IsPrivateUse && !string.IsNullOrEmpty(m_metadata.Language.Name))
					{
						// Couldn't find the language in the official repo. Create a better "private-use" one using the name from the metadata.
						m_wsDefinition.Language = new LanguageSubtag(m_wsDefinition.Id, m_metadata.Language.Name);
					}
				}

				return m_wsDefinition;
			}
		}

		private void SaveWritingSystem()
		{
			new LdmlDataMapper(new WritingSystemFactory()).Write(LdmlFilePath, WritingSystem, null);
		}

		private void HandleQuoteSystemChanged()
		{
			Project copyOfExistingProject = new Project(m_metadata, Name, ws: WritingSystem);
			copyOfExistingProject.m_books.AddRange(m_books);

			m_books.Clear();

			if (File.Exists(OriginalBundlePath) && QuoteSystem != null)
			{
				UserDecisionsProject = copyOfExistingProject;
				using (var bundle = new GlyssenBundle(OriginalBundlePath))
					PopulateAndParseBooks(bundle);
			}
			else
			{
				// This is prevented by logic elsewhere
				throw new ApplicationException();
			}
		}

		private void CreateBackup(string textToAppendToRecordingProjectName, bool hidden = true)
		{
			if (!m_books.Any(b => b.GetScriptBlocks().Any(sb => sb.UserConfirmed)))
				return;
			string newDirectoryPath = GetProjectFolderPath(LanguageIsoCode, Id, Name + " - " + textToAppendToRecordingProjectName);
			if (Directory.Exists(newDirectoryPath))
			{
				string fmt = newDirectoryPath + " ({0})";
				int n = 1;
				do
				{
					newDirectoryPath = String.Format(fmt, n++);
				} while (Directory.Exists(newDirectoryPath));
			}
			DirectoryUtilities.CopyDirectoryContents(ProjectFolder, newDirectoryPath);
			if (hidden)
			{
				var newFilePath = Directory.GetFiles(newDirectoryPath, "*" + kProjectFileExtension).FirstOrDefault();
				if (newFilePath != null)
					SetHiddenFlag(newFilePath, true);
			}
		}

		public bool IsReparseOkay()
		{
			if (QuoteSystem == null)
				return false;
			if (File.Exists(OriginalBundlePath))
				return true;
			return false;
		}

		private void OnReport(ProgressChangedEventArgs e)
		{
			if (ProgressChanged != null)
				ProgressChanged(this, e);
		}

		public bool IsSampleProject
		{
			get { return Id.Equals(SampleProject.kSample, StringComparison.OrdinalIgnoreCase) && LanguageIsoCode == SampleProject.kSample; }
		}

		internal static string GetDefaultRecordingProjectName(string publicationName)
		{
			return String.Format("{0} {1}", publicationName, LocalizationManager.GetString("Project.RecordingProjectDefaultSuffix", "Audio"));
		}

		internal static string GetDefaultRecordingProjectName(IBundle bundle)
		{
			return GetDefaultRecordingProjectName(bundle.Name);
		}

		private void InstallFontsIfNecessary()
		{
			if (m_fontInstallationAttempted || FontHelper.FontInstalled(m_metadata.FontFamily))
				return;

			List<string> ttfFilesToInstall = new List<string>();
			// There could be more than one if different styles (Regular, Italics, etc.) are in different files
			foreach (var ttfFile in Directory.GetFiles(LanguageFolder, "*.ttf"))
			{
				using (PrivateFontCollection fontCol = new PrivateFontCollection())
				{
					fontCol.AddFontFile(ttfFile);
					if (fontCol.Families[0].Name == m_metadata.FontFamily)
						ttfFilesToInstall.Add(ttfFile);
				}
			}
			int count = ttfFilesToInstall.Count;
			if (count > 0)
			{
				m_fontInstallationAttempted = true;

				if (count > 1)
					MessageBox.Show(string.Format(LocalizationManager.GetString("Font.InstallInstructionsMultipleStyles", "The font ({0}) used by this project has not been installed on this machine. We will now launch multiple font preview windows, one for each font style. In the top left of each window, click Install. After installing each font style, you will need to restart {1} to make use of the font."), m_metadata.FontFamily, Program.kProduct));
				else
					MessageBox.Show(string.Format(LocalizationManager.GetString("Font.InstallInstructions", "The font used by this project ({0}) has not been installed on this machine. We will now launch a font preview window. In the top left, click Install. After installing the font, you will need to restart {1} to make use of it."), m_metadata.FontFamily, Program.kProduct));

				foreach (var ttfFile in ttfFilesToInstall)
				{
					try
					{
						Process.Start(ttfFile);
					}
					catch (Exception)
					{
						MessageBox.Show(string.Format(LocalizationManager.GetString("Font.UnableToLaunchFontPreview", "There was a problem launching the font preview. Please install the font manually. {0}"), ttfFile));
					}
				}
			}
			else
				MessageBox.Show(string.Format(LocalizationManager.GetString("Font.FontFilesNotFound", "The font ({0}) used by this project has not been installed on this machine, and {1} could not find the relevant font files. Either they were not copied from the bundle correctly, or they have been moved. You will need to install {0} yourself. After installing the font, you will need to restart {1} to make use of it."), m_metadata.FontFamily, Program.kProduct));
		}

		public void UseDefaultForUnresolvedMultipleChoiceCharacters()
		{
			foreach (var book in IncludedBooks)
			{
				foreach (var block in book.GetScriptBlocks())
					block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber(book.BookId), Versification);
			}
		}

		public Dictionary<string, int> GetKeyStrokesByCharacterId()
		{
			Dictionary<string, int> keyStrokesByCharacterId = new Dictionary<string, int>();
			foreach (var book in IncludedBooks)
			{
				bool singleVoice = book.SingleVoice;
				foreach (var block in book.GetScriptBlocks(true))
				{
					var character = singleVoice ? CharacterVerseData.GetStandardCharacterId(book.BookId, CharacterVerseData.StandardCharacter.Narrator) : block.CharacterIdInScript;
					if (!keyStrokesByCharacterId.ContainsKey(character))
						keyStrokesByCharacterId.Add(character, 0);
					keyStrokesByCharacterId[character] += block.GetText(false).Length;
				}
			}
			return keyStrokesByCharacterId;
		}

		public double GetEstimatedRecordingTime()
		{
			long keyStrokes = GetKeyStrokesByCharacterId().Values.Sum();
			return keyStrokes / (double)Program.kKeyStrokesPerHour;
		}

		public CharacterGroup GetGroupById(string id)
		{
			var grp = CharacterGroupList.GetGroupById(id);
			return grp ?? CharacterGroupList.GroupContainingCharacterId(id);
		}

		public void ConvertContinuersToParatextAssumptions()
		{
			if (m_wsDefinition == null || m_wsDefinition.QuotationMarks == null)
				return;

			List<QuotationMark> replacementQuotationMarks = new List<QuotationMark>();
			foreach (var level in m_wsDefinition.QuotationMarks.OrderBy(q => q, QuoteSystem.QuotationMarkTypeAndLevelComparer))
			{
				if (level.Type == QuotationMarkingSystemType.Normal && level.Level > 1 && !string.IsNullOrWhiteSpace(level.Continue))
				{
					var oneLevelUp = replacementQuotationMarks.SingleOrDefault(q => q.Level == level.Level - 1 && q.Type == QuotationMarkingSystemType.Normal);
					if (oneLevelUp == null)
						continue;
					string oneLevelUpContinuer = oneLevelUp.Continue;
					if (string.IsNullOrWhiteSpace(oneLevelUpContinuer))
						continue;
					string newContinuer = oneLevelUpContinuer + " " + level.Continue;
					replacementQuotationMarks.Add(new QuotationMark(level.Open, level.Close, newContinuer, level.Level, level.Type));
					continue;
				}
				replacementQuotationMarks.Add(level);
			}

			m_wsDefinition.QuotationMarks.Clear();
			m_wsDefinition.QuotationMarks.AddRange(replacementQuotationMarks);
		}

		public bool ProjectFileIsWritable
		{
			get { return m_projectFileIsWritable;  }
			set { m_projectFileIsWritable = value; }
		}
	}

	public class ProjectStateChangedEventArgs : EventArgs
	{
		public ProjectState ProjectState { get; set; }
	}

	[Flags]
	public enum ProjectState
	{
		Initial = 1,
		UsxComplete = 2,
		Parsing = 4,
		NeedsQuoteSystemConfirmation = 8,
		QuoteParseComplete = 16,
		FullyInitialized = 32,
		ReadyForUserInteraction = NeedsQuoteSystemConfirmation | FullyInitialized
	}
}
