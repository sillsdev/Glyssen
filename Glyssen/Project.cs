using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
using static System.String;
using static Glyssen.ReferenceTextType;

namespace Glyssen
{
	public class Project : ProjectBase
	{
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";
		private const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
		private const string kCharacterGroupFileName = "CharacterGroups.xml";

		private const double kUsxPercent = 0.25;
		private const double kGuessPercent = 0.10;
		private const double kQuotePercent = 0.65;

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
		private ReferenceText m_referenceText;

		private Dictionary<string, int> m_speechDistributionScore;
		private Dictionary<string, int> m_keyStrokesByCharacterId;

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public event EventHandler<ProjectStateChangedEventArgs> ProjectStateChanged;
		public event EventHandler QuoteParseCompleted;
		public event EventHandler AnalysisCompleted;
		public event EventHandler CharacterGroupCollectionChanged;
		public event EventHandler CharacterStatisticsCleared;

		public Func<bool> IsOkayToClearExistingRefBlocksWhenChangingReferenceText { get; set; }

		private Project(GlyssenDblTextMetadata metadata, string recordingProjectName = null, bool installFonts = false,
			WritingSystemDefinition ws = null)
			: base(metadata, recordingProjectName ?? GetDefaultRecordingProjectName(metadata.Identification.Name))
		{
			SetBlockGetChapterAnnouncement(ChapterAnnouncementStyle);
			m_wsDefinition = ws;
			ProjectCharacterVerseData = new ProjectCharacterVerseData(ProjectCharacterVerseDataPath);
			m_projectCharacterDetailData = ProjectCharacterDetailData.Load(ProjectCharacterDetailDataPath);
			if (SIL.IO.RobustFile.Exists(VersificationFilePath))
				m_vers = LoadVersification(VersificationFilePath);
			if (installFonts)
				InstallFontsIfNecessary();
		}

		public Project(GlyssenBundle bundle, string recordingProjectName = null, Project projectBeingUpdated = null) :
			this(bundle.Metadata, recordingProjectName, false, bundle.WritingSystemDefinition ?? projectBeingUpdated?.WritingSystem)
		{
			Directory.CreateDirectory(ProjectFolder);
			if (bundle.WritingSystemDefinition != null && bundle.WritingSystemDefinition.QuotationMarks != null &&
				bundle.WritingSystemDefinition.QuotationMarks.Any())
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
			catch (InvalidVersificationLineException ex)
			{
				Logger.WriteError(ex);
				DeleteProjectFolderAndEmptyContainingFolders(ProjectFolder);
				throw;
			}
			UserDecisionsProject = projectBeingUpdated;
			PopulateAndParseBooks(bundle);
		}

		/// <summary>
		/// Used only for sample project and in tests.
		/// </summary>
		internal Project(GlyssenDblTextMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet,
			WritingSystemDefinition ws)
			: this(metadata, ws: ws)
		{
			AddAndParseBooks(books, stylesheet);

			Directory.CreateDirectory(ProjectFolder);
			SIL.IO.RobustFile.WriteAllText(VersificationFilePath, Resources.EnglishVersification);
			m_vers = LoadVersification(VersificationFilePath);
		}

		public static IEnumerable<string> AllPublicationFolders
		{
			get { return Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories); }
		}

		public static IEnumerable<string> AllRecordingProjectFolders
		{
			get { return AllPublicationFolders.SelectMany(Directory.GetDirectories); }
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
				else if ((quoteSystemChanged && !quoteSystemBeingSetForFirstTime) ||
						(QuoteSystemStatus == QuoteSystemStatus.Reviewed &&
						ProjectState == (ProjectState.NeedsQuoteSystemConfirmation | ProjectState.WritingSystemRecoveryInProcess)))
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

		public bool IsQuoteSystemReadyForParse
		{
			get { return (QuoteSystemStatus & QuoteSystemStatus.ParseReady) != 0; }
		}

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
			get { return m_metadata.CharacterGroupGenerationPreferences; }
			set { m_metadata.CharacterGroupGenerationPreferences = value; }
		}

		// TODO: Implement this feature. Currently, this setting is not exposed in the UI and it is
		// only used for estimating cast size.
		public ProjectDramatizationPreferences DramatizationPreferences
		{
			get { return m_metadata.DramatizationPreferences; }
			set { m_metadata.DramatizationPreferences = value; }
		}

		public void SetDefaultCharacterGroupGenerationPreferences()
		{
			if (!CharacterGroupGenerationPreferences.IsSetByUser)
			{
				CharacterGroupGenerationPreferences.NumberOfMaleNarrators = BiblicalAuthors.GetAuthorCount(IncludedBooks.Select(b => b.BookId));
				CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
			}
			if (CharacterGroupGenerationPreferences.CastSizeOption == CastSizeOption.NotSet)
			{
				CharacterGroupGenerationPreferences.CastSizeOption = VoiceActorList.ActiveActors.Any()
					? CastSizeOption.MatchVoiceActorList
					: CastSizeOption.Recommended;
			}
		}

		public void SetCharacterGroupGenerationPreferencesToValidValues()
		{
			// We validate the values when the user can change them directly (in the Narration Preferences dialog),
			// but this handles when other factors are changed which could invalidate the user's choices.
			//
			// For example, the project might be a whole NT, and the user chooses to use 27 authors.
			// Later, the user may remove a book, but the requested number of authors is still 27 (which is now invalid).

			if (CharacterGroupGenerationPreferences.NarratorsOption == NarratorsOption.NarrationByAuthor)
			{
				// Force values to snap to the number of authors, even if this means increasing or decreasing the count.
				Debug.Assert(CharacterGroupGenerationPreferences.NumberOfFemaleNarrators == 0);
				CharacterGroupGenerationPreferences.NumberOfMaleNarrators = AuthorCount;
				return;
			}

			int includedBooksCount = IncludedBooks.Count;
			int numMale = CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			int numFemale = CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;

			if (numMale + numFemale > includedBooksCount)
			{
				int numNarratorsToDecrement = (numMale + numFemale) - includedBooksCount;
				if (numFemale >= numNarratorsToDecrement)
					CharacterGroupGenerationPreferences.NumberOfFemaleNarrators -= numNarratorsToDecrement;
				else
				{
					CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;
					CharacterGroupGenerationPreferences.NumberOfMaleNarrators -= numNarratorsToDecrement - numFemale;
				}
			}
		}

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
				if (!m_projectCharacterDetailData.Any())
					return CharacterDetailData.Singleton.GetDictionary();
				Dictionary<string, CharacterDetail> characterDetails =
					new Dictionary<string, CharacterDetail>(CharacterDetailData.Singleton.GetDictionary());
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
			Debug.Assert(!IsNullOrEmpty(model.RecordingProjectName));
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
				throw new InvalidOperationException("Project not in a valid state to update from text release bundle. ProjectState = " +
													ProjectState);

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
					ProjectStateChanged(this, new ProjectStateChangedEventArgs {ProjectState = m_projectState});
			}
		}

		public string ProjectSummary
		{
			get
			{
				var sb = new StringBuilder(Name);
				if (!IsNullOrEmpty(m_metadata.Language.Name))
					sb.Append(", ").Append(m_metadata.Language.Name);
				if (!IsNullOrEmpty(LanguageIsoCode))
					sb.Append(" (").Append(LanguageIsoCode).Append(")");
				if (!IsNullOrEmpty(PublicationName))
					sb.Append(", ").Append(PublicationName);
				if (!IsNullOrEmpty(Id))
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
				sb.Append(", ")
					.Append(FontSizeInPoints)
					.Append(LocalizationManager.GetString("WritingSystem.Points", "pt", "Units appended to font size to represent points"));
				sb.Append(", ")
					.Append(RightToLeftScript
						? LocalizationManager.GetString("WritingSystem.RightToLeft", "Right-to-left", "Describes a writing system")
						: LocalizationManager.GetString("WritingSystem.LeftToRight", "Left-to-right", "Describes a writing system"));
				return sb.ToString();
			}
		}

		public string BookSelectionSummary
		{
			get { return IncludedBooks.BookSummary(); }
		}

		/// <summary>
		/// If this is set, the user decisions in it will be applied when the quote parser is done
		/// </summary>
		private Project UserDecisionsProject { get; set; }

		public VoiceActorList VoiceActorList
		{
			get
			{
				if (m_voiceActorList == null)
					LoadVoiceActorInformationData();
				return m_voiceActorList;
			}
		}

		public CharacterGroupList CharacterGroupList
		{
			get
			{
				if (m_characterGroupList == null)
					LoadCharacterGroupData();
				return m_characterGroupList;
			}
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

		public ReferenceText ReferenceText
		{
			get
			{
				if (m_referenceText == null)
				{
					if (!ReferenceTextIdentifier.Missing)
						m_referenceText = ReferenceText.GetReferenceText(ReferenceTextIdentifier);
					else
					{
						// If this is a custom reference text whose identifier happens to match a standard one
						// just use it. The custom one has likely been removed because now it has become a
						// standard one.
						Debug.Assert(ReferenceTextIdentifier.Type == Custom);
						ReferenceTextType type;
						if (Enum.TryParse(ReferenceTextIdentifier.CustomIdentifier, out type))
						{
							ChangeReferenceTextIdentifier(ReferenceTextIdentifier.GetOrCreate(type));
						}
					}
				}
				return m_referenceText;
			}
			set
			{
				bool changing = m_referenceText != null;
				m_referenceText = value;
				if (changing)
					ChangeReferenceText();
			}
		}

		public ReferenceTextIdentifier ReferenceTextIdentifier
		{
			get
			{
				return ReferenceTextIdentifier.GetOrCreate(m_metadata.ReferenceTextType, m_metadata.ProprietaryReferenceTextIdentifier);
			}
			set
			{
				if (value.Type == m_metadata.ReferenceTextType && value.CustomIdentifier == m_metadata.ProprietaryReferenceTextIdentifier)
					return;

				ChangeReferenceTextIdentifier(value);
				ChangeReferenceText();
			}
		}

		private void ChangeReferenceTextIdentifier(ReferenceTextIdentifier value)
		{
			m_metadata.ReferenceTextType = value.Type;
			m_metadata.ProprietaryReferenceTextIdentifier = value.CustomIdentifier;
			m_referenceText = ReferenceText.GetReferenceText(ReferenceTextIdentifier);
		}

		private void ChangeReferenceText()
		{
			foreach (var book in m_books)
			{
				List<ReferenceText.VerseSplitLocation> refTextVerseSplitLocations = null;
				var bookNum = BCVRef.BookToNumber(book.BookId);
				var scriptBlocks = book.GetScriptBlocks();
				for (var i = 0; i < scriptBlocks.Count; i++)
				{
					var block = scriptBlocks[i];
					if (block.MatchesReferenceText)
					{
						if (!block.ChangeReferenceText(book.BookId, m_referenceText, Versification))
						{
							if (IsOkayToClearExistingRefBlocksWhenChangingReferenceText())
							{
								if (refTextVerseSplitLocations == null)
									refTextVerseSplitLocations = m_referenceText.GetVerseSplitLocations(book.BookId);
								var matchup = new BlockMatchup(book, i, null,
									nextVerse => m_referenceText.IsOkayToSplitAtVerse(nextVerse, Versification, refTextVerseSplitLocations),
									m_referenceText);
								foreach (var blockToClear in matchup.OriginalBlocks)
									blockToClear.MatchesReferenceText = false;
							}
						}
					}
				}
			}
		}

		public string UiReferenceTextName
		{
			get { return ReferenceTextIdentifier.Missing ? m_metadata.ProprietaryReferenceTextIdentifier : ReferenceText.LanguageName; }
		}

		public bool HasUnappliedSplits()
		{
			return IncludedBooks.Any(b => b.UnappliedSplits.Any());
		}

		internal void ClearAssignCharacterStatus()
		{
			Status.AssignCharacterMode = BlocksToDisplay.NotAlignedToReferenceText;
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
				if (!SIL.IO.RobustFile.Exists(existingProject.OriginalBundlePath))
				{
					upgradeProject = false;
					if (Settings.Default.ParserVersion > existingProject.m_metadata.ParserUpgradeOptOutVersion)
					{
						string msg =
							Format(
								LocalizationManager.GetString("Project.ParserUpgradeBundleMissingMsg",
									"The splitting engine has been upgraded. To make use of the new engine, the original text bundle must be available, but it is not in the original location ({0})."),
								existingProject.OriginalBundlePath) +
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
						var upgradedProject = new Project(existingProject.m_metadata, existingProject.m_recordingProjectName,
							ws: existingProject.WritingSystem);

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
						upgradedProject.InitializeLoadedProject();
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
			// TODO: preserve WritingSystemRecoveryInProcess flag
		}

		public static void DeleteProjectFolderAndEmptyContainingFolders(string projectFolder, bool confirmAndRecycle = false)
		{
			if (confirmAndRecycle)
			{
				if (!ConfirmRecycleDialog.ConfirmThenRecycle(Format("Standard format project \"{0}\"", projectFolder), projectFolder))
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
			return
				PercentInitialized =
					(int) (m_usxPercentComplete * kUsxPercent + m_guessPercentComplete * kGuessPercent + m_quotePercentComplete * kQuotePercent);
		}

		private static Project LoadExistingProject(string projectFilePath)
		{
			// PG-433, 04 JAN 2015, PH: Let the user know if the project file is not writable
			var isWritable = !FileUtils.IsFileLocked(projectFilePath);
			if (!isWritable)
			{
				MessageBox.Show(LocalizationManager.GetString("Project.NotWritableMsg",
					"The project file is not writable. No changes will be saved."));
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
			ForEachBookFileInProject(projectDir,
				(bookId, fileName) => project.m_books.Add(XmlSerializationHelper.DeserializeFromFile<BookScript>(fileName)));
			project.RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();

			// For legacy projects
			if (project.CharacterGroupList.CharacterGroups.Any() &&
				project.CharacterGroupGenerationPreferences.CastSizeOption == CastSizeOption.NotSet)
				project.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;

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
				ProjectState = ProjectState.NeedsQuoteSystemConfirmation | (ProjectState & ProjectState.WritingSystemRecoveryInProcess);
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
			ProjectState = ProjectState.Initial | (ProjectState & ProjectState.WritingSystemRecoveryInProcess);
			var usxWorker = new BackgroundWorker {WorkerReportsProgress = true};
			usxWorker.DoWork += UsxWorker_DoWork;
			usxWorker.RunWorkerCompleted += UsxWorker_RunWorkerCompleted;
			usxWorker.ProgressChanged += UsxWorker_ProgressChanged;

			object[] parameters = {books, stylesheet};
			usxWorker.RunWorkerAsync(parameters);
		}

		private void UsxWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var parameters = (object[]) e.Argument;
			var books = (IEnumerable<UsxDocument>) parameters[0];
			var stylesheet = (IStylesheet) parameters[1];

			var backgroundWorker = (BackgroundWorker)sender;

			e.Result = UsxParser.ParseProject(books, stylesheet, i => backgroundWorker.ReportProgress(i));
		}

		private void UsxWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			var bookScripts = (List<BookScript>) e.Result;

			// This code is an attempt to figure out how we are getting null reference exceptions when using the objects in the list (See PG-275 & PG-287)
			foreach (var bookScript in bookScripts)
				if (bookScript == null || bookScript.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException(Format("{0} Number of BookScripts: {1}. BookScripts which are NOT null: {2}", initialMessage,
						bookScripts.Count, nonNullBookScriptsStr));
				}

			m_books.AddRange(bookScripts);
			m_metadata.ParserVersion = Settings.Default.ParserVersion;
			if (m_books.All(b => IsNullOrEmpty(b.PageHeader)))
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
			ProjectState = ProjectState.UsxComplete | (ProjectState & ProjectState.WritingSystemRecoveryInProcess);
			var guessWorker = new BackgroundWorker {WorkerReportsProgress = true};
			guessWorker.DoWork += GuessWorker_DoWork;
			guessWorker.RunWorkerCompleted += GuessWorker_RunWorkerCompleted;
			guessWorker.ProgressChanged += GuessWorker_ProgressChanged;
			guessWorker.RunWorkerAsync();
		}

		private void GuessWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			bool certain;
			e.Result = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, m_books, Versification, out certain,
				sender as BackgroundWorker);
		}

		private void GuessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			QuoteSystemStatus = QuoteSystemStatus.Guessed;
			QuoteSystem = (QuoteSystem) e.Result;

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
			var quoteWorker = new BackgroundWorker {WorkerReportsProgress = true};
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

			ClearCharacterStatistics();
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

		private string LdmlBackupFilePath => Path.ChangeExtension(LdmlFilePath, DblBundleFileUtils.kUnzippedLdmlFileExtension + "bak");

		protected override string ProjectFolder
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
				{"language", LanguageIsoCode},
				{"ID", Id},
				{"recordingProjectName", Name},
				{"TotalBlocks", ProjectAnalysis.TotalBlocks.ToString(CultureInfo.InvariantCulture)},
				{"UserPercentAssigned", ProjectAnalysis.UserPercentAssigned.ToString(CultureInfo.InvariantCulture)},
				{"TotalPercentAssigned", ProjectAnalysis.TotalPercentAssigned.ToString(CultureInfo.InvariantCulture)},
				{"PercentUnknown", ProjectAnalysis.PercentUnknown.ToString(CultureInfo.InvariantCulture)}
			});

			if (AnalysisCompleted != null)
				AnalysisCompleted(this, new EventArgs());
		}

		public void Save(bool saveCharacterGroups = false)
		{
			if (!m_projectFileIsWritable)
				return;

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
			ProjectState = !IsQuoteSystemReadyForParse
				? ProjectState.NeedsQuoteSystemConfirmation | (ProjectState & ProjectState.WritingSystemRecoveryInProcess)
				: ProjectState.FullyInitialized;
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

		private void LoadCharacterGroupData()
		{
			string path = Path.Combine(ProjectFolder, kCharacterGroupFileName);
			m_characterGroupList = SIL.IO.RobustFile.Exists(path)
				? CharacterGroupList.LoadCharacterGroupListFromFile(path, this)
				: new CharacterGroupList();
			m_characterGroupList.CharacterGroups.CollectionChanged += CharacterGroups_CollectionChanged;
			if (m_voiceActorList != null)
				EnsureCastSizeOptionValid();
		}

		void CharacterGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (CharacterGroupCollectionChanged != null)
				CharacterGroupCollectionChanged(this, new EventArgs());
		}

		private void LoadVoiceActorInformationData()
		{
			string path = Path.Combine(ProjectFolder, kVoiceActorInformationFileName);
			m_voiceActorList = (SIL.IO.RobustFile.Exists(path)) ? VoiceActorList.LoadVoiceActorListFromFile(path) : new VoiceActorList();
			if (m_characterGroupList != null)
				EnsureCastSizeOptionValid();
		}

		public void EnsureCastSizeOptionValid()
		{
			if (CharacterGroupGenerationPreferences.CastSizeOption == CastSizeOption.MatchVoiceActorList &&
				!m_voiceActorList.AllActors.Any())
			{
				var groups = CharacterGroupList.CharacterGroups;
				if (groups.Count == 0)
					CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.NotSet;
				else
				{
					CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.Custom;
					CharacterGroupGenerationPreferences.NumberOfMaleActors = 0;
					CharacterGroupGenerationPreferences.NumberOfFemaleActors = 0;
					CharacterGroupGenerationPreferences.NumberOfChildActors = 0;
					foreach (var characterGroup in groups)
					{
						switch (characterGroup.GroupIdLabel)
						{
							case CharacterGroup.Label.Male:
								CharacterGroupGenerationPreferences.NumberOfMaleActors++;
								break;
							case CharacterGroup.Label.Female:
								CharacterGroupGenerationPreferences.NumberOfFemaleActors++;
								break;
							case CharacterGroup.Label.Child:
								CharacterGroupGenerationPreferences.NumberOfChildActors++;
								break;
						}
					}
					CharacterGroupGenerationPreferences.NumberOfMaleActors += CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
					CharacterGroupGenerationPreferences.NumberOfFemaleActors += CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;
				}
			}
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
				bool retry;
				string backupPath = LdmlBackupFilePath;
				bool attemptToUseBackup = SIL.IO.RobustFile.Exists(backupPath);

				do
				{
					if (!SIL.IO.RobustFile.Exists(LdmlFilePath))
					{
						if (attemptToUseBackup)
						{
							try
							{
								SIL.IO.RobustFile.Move(backupPath, LdmlFilePath);
								attemptToUseBackup = false;
							}
							catch (Exception exRestoreBackup)
							{
								Logger.WriteError(exRestoreBackup);
								Analytics.Track("Failed to rename LDML backup", new Dictionary<string, string>
								{
									{"exceptionMessage", exRestoreBackup.Message},
									{"LdmlFilePath", LdmlFilePath},
								});
							}
						}
						if (!SIL.IO.RobustFile.Exists(LdmlFilePath))
							break;
					}
					try
					{
						new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, m_wsDefinition);
						return m_wsDefinition;
					}
					catch (XmlException e)
					{
						var msg1 = String.Format(LocalizationManager.GetString("Project.LdmlFileLoadError",
								"The writing system definition file for project {0} could not be read:\n{1}\nError: {2}",
								"Param 0: project name; Param 1: LDML filename; Param 2: XML Error message"),
							Name, LdmlFilePath, e.Message);
						var msg2 = attemptToUseBackup
							? LocalizationManager.GetString("Project.UseBackupLdmlFile",
								"To use the automatically created backup (which might be out-of-date), click Retry.",
								"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
								"created backup file exists.")
							: LocalizationManager.GetString("Project.AdvancedUserLdmlRepairInstructions",
								"If you can replace it with a valid backup or know how to repair it yourself, do so and then click Retry.",
								"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
								"created backup file does not exist.");
						var msg3 = String.Format(LocalizationManager.GetString("Project.IgnoreToRepairLdmlFile",
							"Otherwise, click Ignore and {0} will repair the file for you. Some information might not be recoverable, " +
							"so check the quote system and font settings carefully.", "Param 0: \"Glyssen\""), GlyssenInfo.kProduct);
						var msg = msg1 + "\n\n" + msg2 + msg3;
						Logger.WriteError(msg, e);
						switch (
							MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning,
								MessageBoxDefaultButton.Button2))
						{
							default:
								ProjectState |= ProjectState.WritingSystemRecoveryInProcess;
								retry = false;
								break;
							case DialogResult.Retry:
								if (attemptToUseBackup)
								{
									try
									{
										string corruptedLdmlFilePath =
											Path.ChangeExtension(LdmlFilePath, DblBundleFileUtils.kUnzippedLdmlFileExtension + "corrupted");
										SIL.IO.RobustFile.Delete(corruptedLdmlFilePath);
										SIL.IO.RobustFile.Move(LdmlFilePath, corruptedLdmlFilePath);
										SIL.IO.RobustFile.Move(backupPath, LdmlFilePath);
									}
									catch (Exception exReplaceCorruptedLdmlWithBackup)
									{
										Logger.WriteError(exReplaceCorruptedLdmlWithBackup);
										// We'll come back around and display the "self-repair" version of the message.
									}
									attemptToUseBackup = false;
								}
								retry = true;
								break;
							case DialogResult.Abort:
								throw;
						}
					}
				} while (retry);

				if (IetfLanguageTag.IsValid(m_metadata.Language.Ldml))
					m_wsDefinition.Id = IetfLanguageTag.GetLanguageSubtag(m_metadata.Language.Ldml);
				if (IsNullOrWhiteSpace(m_wsDefinition.Id) && IetfLanguageTag.IsValid(m_metadata.Language.Iso))
					m_wsDefinition.Id = IetfLanguageTag.GetLanguageSubtag(m_metadata.Language.Iso);
				if (m_wsDefinition.Id == null)
				{
					m_wsDefinition.Id = IsNullOrEmpty(m_metadata.Language.Ldml) ? m_metadata.Language.Iso : m_metadata.Language.Ldml;
				}
				else
				{
					try
					{
						m_wsDefinition.Language = m_wsDefinition.Id;
					}
					catch (ArgumentException)
					{
						try
						{
							if (m_metadata.Language.Ldml != m_metadata.Language.Iso && IetfLanguageTag.IsValid(m_metadata.Language.Iso))
							{
								m_wsDefinition.Id = IetfLanguageTag.GetLanguageSubtag(m_metadata.Language.Iso);
								m_wsDefinition.Language = m_wsDefinition.Id;
							}
						}
						catch (ArgumentException)
						{
							// Ignore. Following code should try to patch things up.
						}
					}
				}
				if ((m_wsDefinition.Language == null || m_wsDefinition.Language.IsPrivateUse) && !IsNullOrEmpty(m_metadata.Language.Name))
				{
					// TODO: Strip off the first dash and anything following???
					// Couldn't find the language in the official repo. Create a better "private-use" one using the name from the metadata.
					m_wsDefinition.Language = new LanguageSubtag(m_wsDefinition.Id, m_metadata.Language.Name);
				}

				return m_wsDefinition;
			}
		}

		private void SaveWritingSystem()
		{
			string backupPath = null;
			try
			{
				if (SIL.IO.RobustFile.Exists(LdmlFilePath))
				{
					backupPath = LdmlBackupFilePath;
					if (SIL.IO.RobustFile.Exists(backupPath))
						SIL.IO.RobustFile.Delete(backupPath);
					SIL.IO.RobustFile.Move(LdmlFilePath, backupPath);
				}
			}
			catch (Exception exMakeBackup)
			{
				// Oh, well. Hope for the best...
				Logger.WriteError("Failed to create LDML backup", exMakeBackup);
				Analytics.Track("Failed to create LDML backup", new Dictionary<string, string>
				{
					{"exceptionMessage", exMakeBackup.Message},
					{"LdmlFilePath", LdmlFilePath},
				});
				backupPath = null;
			}
			try
			{
				new LdmlDataMapper(new WritingSystemFactory()).Write(LdmlFilePath, WritingSystem, null);
				// Now test to see if what we wrote is actually readable...
				new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, new WritingSystemDefinition());
			}
			catch(FileLoadException loadException)
			{
				throw; // Don't want to ignore this error - should never happen on valid installation.
			}
			catch (Exception exSave)
			{
				Logger.WriteError("Writing System Save Failure", exSave);
				Analytics.Track("Writing System Save Failure", new Dictionary<string, string>
				{
					{"exceptionMessage", exSave.Message},
					{"CurrentProjectPath", Settings.Default.CurrentProject},
				});
				if (backupPath != null)
				{
					// If we ended up with an unreadable file, revert to backup???
					try
					{
						var wsFromBackup = new WritingSystemDefinition();
						SIL.IO.RobustFile.Delete(LdmlFilePath);
						SIL.IO.RobustFile.Move(backupPath, LdmlFilePath);
						new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, wsFromBackup);
						if (!wsFromBackup.QuotationMarks.SequenceEqual(WritingSystem.QuotationMarks) ||
							wsFromBackup.DefaultFont.Name != WritingSystem.DefaultFont.Name ||
							wsFromBackup.RightToLeftScript != WritingSystem.RightToLeftScript)
						{
							// There were significant changes that we couldn't save. Something bad is probably going to happen, so we need to tell the user.
							ErrorReport.ReportNonFatalExceptionWithMessage(exSave, LocalizationManager.GetString("Project.RevertedToOutdatedBackupWs",
									"The current writing system settings could not be saved. Fortunately, {0} was able to recover from the backup, but " +
									"since the old settings were different, some things might not work correctly next time you open this project."),
								GlyssenInfo.kProduct);
						}
					}
					catch (Exception exRecover)
					{
						Logger.WriteError("Recovery from backup Writing System File Failed.", exRecover);
						Analytics.Track("Recovery from backup Writing System File Failed.", new Dictionary<string, string>
						{
							{"exceptionMessage", exRecover.Message},
							{"CurrentProjectPath", Settings.Default.CurrentProject},
						});
						throw exSave;
					}
				}
			}
		}

		private void HandleQuoteSystemChanged()
		{
			Project copyOfExistingProject = new Project(m_metadata, Name, ws: WritingSystem);
			copyOfExistingProject.m_books.AddRange(m_books);

			m_books.Clear();

			if (SIL.IO.RobustFile.Exists(OriginalBundlePath) && QuoteSystem != null)
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
					newDirectoryPath = Format(fmt, n++);
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
			if (SIL.IO.RobustFile.Exists(OriginalBundlePath))
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
			get
			{
				return Id.Equals(SampleProject.kSample, StringComparison.OrdinalIgnoreCase) && LanguageIsoCode == SampleProject.kSample;
			}
		}

		internal static string GetDefaultRecordingProjectName(string publicationName)
		{
			return Format("{0} {1}", publicationName, LocalizationManager.GetString("Project.RecordingProjectDefaultSuffix", "Audio"));
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
					MessageBox.Show(
						Format(
							LocalizationManager.GetString("Font.InstallInstructionsMultipleStyles",
								"The font ({0}) used by this project has not been installed on this machine. We will now launch multiple font preview windows, one for each font style. In the top left of each window, click Install. After installing each font style, you will need to restart {1} to make use of the font."),
							m_metadata.FontFamily, GlyssenInfo.kProduct));
				else
					MessageBox.Show(
						Format(
							LocalizationManager.GetString("Font.InstallInstructions",
								"The font used by this project ({0}) has not been installed on this machine. We will now launch a font preview window. In the top left, click Install. After installing the font, you will need to restart {1} to make use of it."),
							m_metadata.FontFamily, GlyssenInfo.kProduct));

				foreach (var ttfFile in ttfFilesToInstall)
				{
					try
					{
						Process.Start(ttfFile);
					}
					catch (Exception ex)
					{
						Logger.WriteError("There was a problem launching the font preview.Please install the font manually:" + ttfFile, ex);
						MessageBox.Show(
							Format(
								LocalizationManager.GetString("Font.UnableToLaunchFontPreview",
									"There was a problem launching the font preview. Please install the font manually. {0}"), ttfFile));
					}
				}
			}
			else
				MessageBox.Show(
					Format(
						LocalizationManager.GetString("Font.FontFilesNotFound",
							"The font ({0}) used by this project has not been installed on this machine, and {1} could not find the relevant font files. Either they were not copied from the bundle correctly, or they have been moved. You will need to install {0} yourself. After installing the font, you will need to restart {1} to make use of it."),
						m_metadata.FontFamily, GlyssenInfo.kProduct));
		}

		public void UseDefaultForUnresolvedMultipleChoiceCharacters()
		{
			foreach (var book in IncludedBooks)
			{
				foreach (var block in book.GetScriptBlocks())
					block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber(book.BookId), Versification);
			}
		}

		public Dictionary<string, int> SpeechDistributionScoreByCharacterId
		{
			get
			{
				if (m_speechDistributionScore == null)
					CalculateCharacterStatistics();
				return m_speechDistributionScore;
			}
		}

		public Dictionary<string, int> KeyStrokesByCharacterId
		{
			get
			{
				if (m_keyStrokesByCharacterId == null)
					CalculateCharacterStatistics();
				return m_keyStrokesByCharacterId;
			}
		}

		public Dictionary<string, int>.KeyCollection AllCharacterIds
		{
			get { return KeyStrokesByCharacterId.Keys; }
		}

		public int TotalCharacterCount
		{
			get { return KeyStrokesByCharacterId.Count; }
		}

		private class DistributionScoreBookStats
		{
			internal DistributionScoreBookStats(int chapterNumber)
			{
				FirstChapter = chapterNumber;
				LastChapter = chapterNumber;
				NumberOfChapters = 1;
				NonContiguousBlocksInCurrentChapter = 1;
			}

			internal int NonContiguousBlocksInMaxChapter { get; set; }
			internal int NonContiguousBlocksInCurrentChapter { get; set; }
			internal int NumberOfChapters { get; set; }
			internal int FirstChapter { get; set; }
			internal int LastChapter { get; set; }
		}

		internal void ClearCharacterStatistics()
		{
			m_keyStrokesByCharacterId = null;
			m_speechDistributionScore = null;
			CharacterStatisticsCleared?.Invoke(this, new EventArgs());
		}

		private void CalculateCharacterStatistics()
		{
			m_keyStrokesByCharacterId = new Dictionary<string, int>();
			m_speechDistributionScore = new Dictionary<string, int>();
			foreach (var book in IncludedBooks)
			{
				var bookDistributionScoreStats = new Dictionary<string, DistributionScoreBookStats>();
				var narratorToUseForSingleVoiceBook = (book.SingleVoice) ?
					CharacterVerseData.GetStandardCharacterId(book.BookId, CharacterVerseData.StandardCharacter.Narrator) :
					null;

				string prevCharacter = null;
				foreach (var block in book.GetScriptBlocks( /*true*/))
					// The logic for calculating keystrokes had join = true, but this seems likely to be less efficient and should not be needed.
				{
					string character;
					if (narratorToUseForSingleVoiceBook != null)
						character = narratorToUseForSingleVoiceBook;
					else
					{
						character = block.CharacterIdInScript;

						// REVIEW: It's possible that we should throw an exception if this happens (in production code).
						if (character == CharacterVerseData.kAmbiguousCharacter || character == CharacterVerseData.kUnknownCharacter)
							continue;

						if (character == null)
						{
							throw new Exception($"Block has character set to null. This should never happen! " +
								$"Block ({book.BookId} {block.ChapterNumber}:{block.InitialStartVerseNumber}): {block}");
						}

						switch (CharacterVerseData.GetStandardCharacterType(character))
						{
							case CharacterVerseData.StandardCharacter.Intro:
								if (DramatizationPreferences.BookIntroductionsDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
									continue;
								break;
							case CharacterVerseData.StandardCharacter.ExtraBiblical:
								if (DramatizationPreferences.SectionHeadDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
									continue;
								break;
							case CharacterVerseData.StandardCharacter.BookOrChapter:
								if (DramatizationPreferences.BookTitleAndChapterDramatization == ExtraBiblicalMaterialSpeakerOption.Omitted)
									continue;
								break;
						}
					}

					if (!m_keyStrokesByCharacterId.ContainsKey(character))
					{
						m_keyStrokesByCharacterId.Add(character, 0);
						m_speechDistributionScore.Add(character, 0);
					}
					m_keyStrokesByCharacterId[character] += block.GetText(false).Length;

					DistributionScoreBookStats stats;
					if (!bookDistributionScoreStats.TryGetValue(character, out stats))
					{
						bookDistributionScoreStats.Add(character, new DistributionScoreBookStats(block.ChapterNumber));
					}
					else
					{
						if (stats.LastChapter != block.ChapterNumber)
						{
							if (stats.NonContiguousBlocksInCurrentChapter > stats.NonContiguousBlocksInMaxChapter)
								stats.NonContiguousBlocksInMaxChapter = stats.NonContiguousBlocksInCurrentChapter;
							stats.LastChapter = block.ChapterNumber;
							stats.NonContiguousBlocksInCurrentChapter = 1;
							stats.NumberOfChapters++;
						}
						else if (prevCharacter != character)
						{
							stats.NonContiguousBlocksInCurrentChapter++;
						}
					}
					prevCharacter = character;
				}
				foreach (var characterStatsInfo in bookDistributionScoreStats)
				{
					var stats = characterStatsInfo.Value;

					if (stats.NonContiguousBlocksInCurrentChapter > stats.NonContiguousBlocksInMaxChapter)
						stats.NonContiguousBlocksInMaxChapter = stats.NonContiguousBlocksInCurrentChapter;

					var resultInBook = (stats.NumberOfChapters <= 1)
						? stats.NonContiguousBlocksInMaxChapter
						: (int)
						Math.Round(
							stats.NonContiguousBlocksInMaxChapter + (Math.Pow(stats.NumberOfChapters, 3) + stats.LastChapter - stats.FirstChapter) / 2,
							MidpointRounding.AwayFromZero);

					int resultInMaxBook;
					if (!m_speechDistributionScore.TryGetValue(characterStatsInfo.Key, out resultInMaxBook) || (resultInBook > resultInMaxBook))
						m_speechDistributionScore[characterStatsInfo.Key] = resultInBook;
				}
			}
			Debug.Assert(m_keyStrokesByCharacterId.Values.All(v => v != 0));
		}

		public double GetEstimatedRecordingTime()
		{
			long keyStrokes = KeyStrokesByCharacterId.Values.Sum();
			return keyStrokes / GlyssenInfo.kKeyStrokesPerHour;
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
				if (level.Type == QuotationMarkingSystemType.Normal && level.Level > 1 && !IsNullOrWhiteSpace(level.Continue))
				{
					var oneLevelUp =
						replacementQuotationMarks.SingleOrDefault(q => q.Level == level.Level - 1 && q.Type == QuotationMarkingSystemType.Normal);
					if (oneLevelUp == null)
						continue;
					string oneLevelUpContinuer = oneLevelUp.Continue;
					if (IsNullOrWhiteSpace(oneLevelUpContinuer))
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
			get { return m_projectFileIsWritable; }
			set { m_projectFileIsWritable = value; }
		}

		public int AuthorCount
		{
			get { return BiblicalAuthors.GetAuthorCount(IncludedBooks.Select(b => b.BookId)); }
		}

		public string LastExportLocation => Directory.Exists(Status.LastExportLocation) ? Status.LastExportLocation : Empty;

		public IReadOnlyList<BookScript> TestQuoteSystem(QuoteSystem altQuoteSystem)
		{
			var cvInfo = new CombinedCharacterVerseData(this);

			var bundle = new GlyssenBundle(OriginalBundlePath);
			var books = UsxParser.ParseProject(bundle.UsxBooksToInclude, bundle.Stylesheet, null);

			var blocksInBook = books.ToDictionary(b => b.BookId, b => b.GetScriptBlocks());

			var parsedBlocksByBook = new ConcurrentDictionary<string, BookScript>();
			QuoteParser.SetQuoteSystem(altQuoteSystem);
			Parallel.ForEach(blocksInBook, bookidBlocksPair =>
			{
				var bookId = bookidBlocksPair.Key;
				var blocks =
					new QuoteParser(cvInfo, bookId, bookidBlocksPair.Value, Versification).Parse().ToList();
				var parsedBook = new BookScript(bookId, blocks);
				parsedBlocksByBook.AddOrUpdate(bookId, parsedBook, (s, script) => parsedBook);
			});

			// sort the list
			var bookScripts = parsedBlocksByBook.Values.ToList();
			bookScripts.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));
			return bookScripts;
		}

		public class ProjectStateChangedEventArgs : EventArgs
		{
			public ProjectState ProjectState { get; set; }
		}
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
		WritingSystemRecoveryInProcess = 64,
		ReadyForUserInteraction = NeedsQuoteSystemConfirmation | FullyInitialized
	}
}
