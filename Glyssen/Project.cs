using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Glyssen.Analysis;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Paratext;
using Glyssen.Properties;
using Glyssen.Quote;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngine.Utilities;
using GlyssenEngine.VoiceActor;
using Paratext.Data;
using SIL;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.Extensions;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;
using static System.String;
using Resources = Glyssen.Properties.Resources;

namespace Glyssen
{
	public class Project : ProjectBase
	{
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kProjectCharacterDetailFileName = "ProjectCharacterDetail.txt";
		private const string kVoiceActorInformationFileName = "VoiceActorInformation.xml";
		private const string kCharacterGroupFileName = "CharacterGroups.xml";
		// Total path limit is 260 characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\xxx\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\xxx\xxxxxxxxxxxxxxxx\<Recording Project Name>\xxx*.glyssen
		private const int kMaxDefaultProjectNameLength = 150;
		private const int kMaxPath = 260;

		private const double kUsxPercent = 0.25;
		private const double kGuessPercent = 0.10;
		private const double kQuotePercent = 0.65;

		// Enhance: these should be settings, eventually in the UI
		public const double kKeyStrokesPerHour = 6000;
		public const double kCameoCharacterEstimatedHoursLimit = 0.2;

		private readonly GlyssenDblTextMetadata m_projectMetadata;
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
		private ISet<CharacterDetail> ProjectCharacterDetail { get; }
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

		/// <exception cref="ProjectNotFoundException">Paratext was unable to access the project (only pertains to
		/// Glyssen projects that are associated with a live Paratext project)</exception>
		private Project(GlyssenDblTextMetadata metadata, string recordingProjectName = null, bool installFonts = false,
			WritingSystemDefinition ws = null, bool loadVersification = true)
			: base(metadata, recordingProjectName ?? GetDefaultRecordingProjectName(metadata.Identification.Name))
		{
			m_projectMetadata = metadata;
			SetBlockGetChapterAnnouncement(ChapterAnnouncementStyle);
			m_wsDefinition = ws;

			ProjectCharacterDetail = ProjectCharacterDetailData.Load(ProjectCharacterDetailDataPath);

			if (loadVersification)
			{
				if (HasVersificationFile)
					SetVersification(LoadVersification(VersificationFilePath));
				else if (IsLiveParatextProject)
				{
					try
					{
						SetVersification(GetSourceParatextProject().Settings.Versification);
					}
					catch (ProjectNotFoundException e)
					{
						Logger.WriteError("Paratext project not found. Falling back to get usable versification file.", e);
						if (RobustFile.Exists(FallbackVersificationFilePath))
							SetVersification(LoadVersification(FallbackVersificationFilePath));
						else
						{
							MessageModal.Show(Format(Localizer.GetString("Project.ParatextProjectMissingNoFallbackVersificationFile",
								"{0} project {1} is not available and project {2} does not have a fallback versification file; " +
								"therefore, the {3} versification is being used by default. If this is not the correct versification " +
								"for this project, some things will not work as expected.",
								"Param 0: \"Paratext\" (product name); " +
								"Param 1: Paratext project short name (unique project identifier); " +
								"Param 2: Glyssen recording project name; " +
								"Param 3: “English” (versification mame)"),
								ParatextScrTextWrapper.kParatextProgramName,
								ParatextProjectName,
								Name,
								"“English”"), GlyssenInfo.kProduct);
							SetVersification(ScrVers.English);
						}
					}
				}
			}

			if (installFonts)
				Fonts.InstallIfNecessary(m_projectMetadata.FontFamily, LanguageFolder, ref m_fontInstallationAttempted);
		}

		public Project(GlyssenBundle bundle, string recordingProjectName = null, Project projectBeingUpdated = null) :
			this(bundle.Metadata, recordingProjectName, false, bundle.WritingSystemDefinition ?? projectBeingUpdated?.WritingSystem)
		{
			Directory.CreateDirectory(ProjectFolder);
			if (bundle.WritingSystemDefinition != null && bundle.WritingSystemDefinition.QuotationMarks != null &&
				bundle.WritingSystemDefinition.QuotationMarks.Any())
			{
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				SetWsQuotationMarksUsingFullySpecifiedContinuers(bundle.WritingSystemDefinition.QuotationMarks);
			}

			if (!bundle.CopyFontFiles(LanguageFolder, out var filesWhichFailedToCopy) && filesWhichFailedToCopy.Any())
				ErrorReport.ReportNonFatalMessageWithStackTrace(
					Localizer.GetString("DblBundle.FontFileCopyFailed", "An attempt to copy font file {0} from the bundle to {1} failed."),
					Path.GetFileName(filesWhichFailedToCopy.First()), LanguageFolder);

			Fonts.InstallIfNecessary(m_projectMetadata.FontFamily, LanguageFolder, ref m_fontInstallationAttempted);
			bundle.CopyVersificationFile(VersificationFilePath);
			try
			{
				SetVersification(LoadVersification(VersificationFilePath));
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

		internal Project(ParatextScrTextWrapper paratextProject) :
			this(paratextProject.GlyssenDblTextMetadata, null, false, paratextProject.WritingSystem)
		{
			Directory.CreateDirectory(ProjectFolder);
			if (paratextProject.HasQuotationRulesSet)
			{
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				SetWsQuotationMarksUsingFullySpecifiedContinuers(paratextProject.QuotationMarks);
			}

			ParseAndSetBooks(paratextProject.UsxDocumentsForIncludedBooks, paratextProject.Stylesheet);
		}

		/// <summary>
		/// Used only for sample project and in tests.
		/// </summary>
		internal Project(GlyssenDblTextMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet,
			WritingSystemDefinition ws, string versificationInfo = null)
			: this(metadata, ws: ws)
		{
			Directory.CreateDirectory(ProjectFolder);
			RobustFile.WriteAllText(VersificationFilePath, versificationInfo ?? Resources.EnglishVersification);
			SetVersification(LoadVersification(VersificationFilePath));

			ParseAndSetBooks(books, stylesheet);
		}

		private bool HasVersificationFile => RobustFile.Exists(VersificationFilePath);

		public static IEnumerable<string> AllPublicationFolders => Directory.GetDirectories(ProjectsBaseFolder).SelectMany(Directory.GetDirectories);

		public static IEnumerable<string> AllRecordingProjectFolders => AllPublicationFolders.SelectMany(Directory.GetDirectories);

		public string AudioStockNumber => m_projectMetadata.AudioStockNumber;

		public string Id => m_metadata.Id;

		public string Name => m_recordingProjectName;

		public string PublicationName => m_metadata.Identification?.Name;

		internal static string DefaultRecordingProjectNameSuffix => " " +
			Localizer.GetString("Project.RecordingProjectDefaultSuffix", "Audio",
				"This must not contain any illegal file path characters!").Trim(FileSystemUtils.TrimCharacters);

		private static int MaxBaseRecordingNameLength => kMaxDefaultProjectNameLength - DefaultRecordingProjectNameSuffix.Length;

		public IReadOnlyGlyssenDblTextMetadata Metadata => m_metadata;

		internal bool IsLiveParatextProject => m_projectMetadata.Type == ParatextScrTextWrapper.kLiveParatextProjectType;

		internal bool IsBundleBasedProject => !IsNullOrEmpty(OriginalBundlePath);

		internal string ParatextProjectName => m_projectMetadata.ParatextProjectId;

		/// <summary>
		/// Gets the live Paratext project associated with this Glyssen project
		/// </summary>
		/// <exception cref="InvalidOperationException">An attempt was made to call this method for a project not
		/// associated with a live Paratext project</exception>
		/// <exception cref="ProjectNotFoundException">Paratext was unable to access the project</exception>
		private ScrText GetSourceParatextProject()
		{
			if (m_projectMetadata.Type != ParatextScrTextWrapper.kLiveParatextProjectType)
				throw new InvalidOperationException("GetSourceParatextProject should only be used for projects based on live Paratext projects.");
			return ScrTextCollection.Get(ParatextProjectName);
		}

		public ChapterAnnouncement ChapterAnnouncementStyle
		{
			get => m_projectMetadata.ChapterAnnouncementStyle;
			private set
			{
				m_projectMetadata.ChapterAnnouncementStyle = value;
				SetBlockGetChapterAnnouncement(value);
			}
		}

		public bool SkipChapterAnnouncementForFirstChapter => !m_projectMetadata.IncludeChapterAnnouncementForFirstChapter;

		public bool SkipChapterAnnouncementForSingleChapterBooks => !m_projectMetadata.IncludeChapterAnnouncementForSingleChapterBooks;

		public void SetBlockGetChapterAnnouncement(ChapterAnnouncement announcementStyle)
		{
			switch (announcementStyle)
			{
				case ChapterAnnouncement.ChapterLabel:
					GetBookName = null;
					Block.FormatChapterAnnouncement = null;
					return;
				case ChapterAnnouncement.PageHeader:
					GetBookName = bookId => GetBook(bookId)?.PageHeader;
					break;
				case ChapterAnnouncement.MainTitle1:
					GetBookName = bookId => GetBook(bookId)?.MainTitle;
					break;
				case ChapterAnnouncement.ShortNameFromMetadata:
					GetBookName = bookId => m_metadata.AvailableBooks.FirstOrDefault(b => b.Code == bookId)?.ShortName;
					break;
				case ChapterAnnouncement.LongNameFromMetadata:
					GetBookName = bookId => m_metadata.AvailableBooks.FirstOrDefault(b => b.Code == bookId)?.LongName;
					break;
			}
			Block.FormatChapterAnnouncement = GetFormattedChapterAnnouncement;
		}

		public ProjectStatus Status => m_projectMetadata.ProjectStatus;

		public ProjectAnalysis ProjectAnalysis => m_analysis ?? (m_analysis = new ProjectAnalysis(this));

		public QuoteSystem QuoteSystem
		{
			get => m_quoteSystem != null ? m_quoteSystem : (m_quoteSystem = QuoteSystem.TryCreateFromWritingSystem(WritingSystem));
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
			get { return m_projectMetadata.ProjectStatus.QuoteSystemStatus; }
			set { m_projectMetadata.ProjectStatus.QuoteSystemStatus = value; }
		}

		public bool IsQuoteSystemReadyForParse => (QuoteSystemStatus & QuoteSystemStatus.ParseReady) != 0;

		public DateTime QuoteSystemDate => m_projectMetadata.ProjectStatus.QuoteSystemDate;

		public BookSelectionStatus BookSelectionStatus
		{
			get
			{
				// don't make the user open the select books dialog if there is only 1 book
				if ((m_projectMetadata.ProjectStatus.BookSelectionStatus == BookSelectionStatus.UnReviewed) &&
					(AvailableBooks.Count == 1) && (IncludedBooks.Count == 1))
				{
					m_projectMetadata.ProjectStatus.BookSelectionStatus = BookSelectionStatus.Reviewed;
				}

				return m_projectMetadata.ProjectStatus.BookSelectionStatus;
			}
			set { m_projectMetadata.ProjectStatus.BookSelectionStatus = value; }
		}

		public ProjectSettingsStatus ProjectSettingsStatus
		{
			get { return m_projectMetadata.ProjectStatus.ProjectSettingsStatus; }
			set { m_projectMetadata.ProjectStatus.ProjectSettingsStatus = value; }
		}

		public CharacterGroupGenerationPreferences CharacterGroupGenerationPreferences
		{
			get { return m_projectMetadata.CharacterGroupGenerationPreferences; }
			set { m_projectMetadata.CharacterGroupGenerationPreferences = value; }
		}

		// TODO: Implement this feature. Currently, this setting is not exposed in the UI and it is
		// only used for estimating cast size.
		public ProjectDramatizationPreferences DramatizationPreferences
		{
			get { return m_projectMetadata.DramatizationPreferences; }
			set { m_projectMetadata.DramatizationPreferences = value; }
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
			// For example, the project might be a whole NT, and the user chooses to use 27 narrators.
			// Later, the user may remove a book, but the requested number of narrators is still 27 (which is now invalid).
			EnsureNarratorPreferencesAreValid(CharacterGroupGenerationPreferences.NarratorsOption,
				(v) => CharacterGroupGenerationPreferences.NumberOfMaleNarrators = v,
				(v) => CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = v);
		}

		/// <summary>
		/// This implementation method allows CastSizePlanningViewModel to pass in a different
		/// <paramref name="desiredOption"/> and setter methods so that the underlying preferences
		/// aren't changed directly.
		/// </summary>
		public void EnsureNarratorPreferencesAreValid(NarratorsOption desiredOption,
			Action<int> setNewMaleNarratorCount, Action<int> setNewFemaleNarratorCount)
		{
			int numMale = CharacterGroupGenerationPreferences.NumberOfMaleNarrators;
			int numFemale = CharacterGroupGenerationPreferences.NumberOfFemaleNarrators;

			if (desiredOption == NarratorsOption.NarrationByAuthor)
			{
				Debug.Assert(numFemale == 0);
				if (numMale > AuthorCount)
					setNewMaleNarratorCount(AuthorCount);
				else if (numMale == 0)
					setNewMaleNarratorCount(DefaultNarratorCountForNarrationByAuthor);
				return;
			}

			int includedBooksCount = IncludedBooks.Count;

			if (numMale + numFemale > includedBooksCount)
			{
				int numNarratorsToDecrement = (numMale + numFemale) - includedBooksCount;
				if (numFemale >= numNarratorsToDecrement)
					setNewFemaleNarratorCount(numFemale - numNarratorsToDecrement);
				else
				{
					setNewFemaleNarratorCount(0);
					setNewMaleNarratorCount(numMale - (numNarratorsToDecrement - numFemale));
				}
			}
		}

		public IEnumerable<string> IncludedBookIds => IncludedBooks.Select(b => b.BookId);

		public IReadOnlyList<BookScript> IncludedBooks
		{
			get
			{
				return (from book in Books
					where AvailableBooks.Where(ab => ab.IncludeInScript).Select(ab => ab.Code).Contains(book.BookId)
					select book).ToList();
			}
		}

		public IReadOnlyList<Book> AvailableBooks => m_metadata.AvailableBibleBooks;

		public string OriginalBundlePath
		{
			get => m_projectMetadata.OriginalReleaseBundlePath;
			set => m_projectMetadata.OriginalReleaseBundlePath = value;
		}

		public ProjectCharacterVerseData ProjectCharacterVerseData { get; private set; }

		public IReadOnlyDictionary<string, CharacterDetail> AllCharacterDetailDictionary
		{
			get
			{
				if (!ProjectCharacterDetail.Any())
					return CharacterDetailData.Singleton.GetDictionary();
				Dictionary<string, CharacterDetail> characterDetails =
					new Dictionary<string, CharacterDetail>(CharacterDetailData.Singleton.GetDictionary());
				// If the following line throws an exception because the key is already present, there is
				// either a bug in ProjectDataMigrator.MigrateDeprecatedCharacterIds, or this property is
				// being accessed prematurely, before the migration has happened.
				characterDetails.AddRange(ProjectCharacterDetail.ToDictionary(k => k.CharacterId));
				return characterDetails;
			}
		}

		public bool IsProjectSpecificCharacter(string character)
		{
			return ProjectCharacterDetail.Any(d => d.CharacterId == character);
		}

		public void AddProjectCharacterDetail(CharacterDetail characterDetail)
		{
			if (CharacterDetailData.Singleton.GetDictionary().ContainsKey(characterDetail.CharacterId))
				throw new ArgumentException($"The built-in character detail collection already contains character {characterDetail.CharacterId}.");
			ProjectCharacterDetail.Add(characterDetail);
		}

		public bool RemoveProjectCharacterDetail(string character)
		{
			var itemToRemove = ProjectCharacterDetail.SingleOrDefault(d => d.CharacterId == character);
			return itemToRemove != null && ProjectCharacterDetail.Remove(itemToRemove);
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
			m_projectMetadata.AudioStockNumber = model.AudioStockNumber;
			m_projectMetadata.FontFamily = model.WsModel.CurrentDefaultFontName;
			m_projectMetadata.FontSizeInPoints = (int) model.WsModel.CurrentDefaultFontSize;
			m_metadata.Language.ScriptDirection = model.WsModel.CurrentRightToLeftScript ? "RTL" : "LTR";
			ChapterAnnouncementStyle = model.ChapterAnnouncementStyle;
			m_projectMetadata.IncludeChapterAnnouncementForFirstChapter = !model.SkipChapterAnnouncementForFirstChapter;
			m_projectMetadata.IncludeChapterAnnouncementForSingleChapterBooks = !model.SkipChapterAnnouncementForSingleChapterBooks;
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
				m_projectMetadata.Inactive = true;
			Save();
			if (!moving)
				CreateBackup("Backup before updating from new bundle");

			bundle.Metadata.CopyGlyssenModifiableSettings(m_projectMetadata);

			// PG-612: renaming bundle causes loss of assignments
			bundle.Metadata.ControlFileVersion = m_projectMetadata.ControlFileVersion;

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
					.Append(Localizer.GetString("WritingSystem.Points", "pt", "Units appended to font size to represent points"));
				sb.Append(", ")
					.Append(RightToLeftScript
						? Localizer.GetString("WritingSystem.RightToLeft", "Right-to-left", "Describes a writing system")
						: Localizer.GetString("WritingSystem.LeftToRight", "Left-to-right", "Describes a writing system"));
				return sb.ToString();
			}
		}

		public string BookSelectionSummary => IncludedBooks.BookSummary();

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

		public bool CharacterGroupListPreviouslyGenerated => CharacterGroupList.CharacterGroups.Any();

		public bool IsVoiceActorScriptReady => IsVoiceActorAssignmentsComplete && EveryAssignedGroupHasACharacter;

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

		public IEnumerable<VoiceActor> UnusedActors
		{
			get { return VoiceActorList.ActiveActors.Where(actor => !CharacterGroupList.HasVoiceActorAssigned(actor.Id)); }
		}

		public ReferenceText ReferenceText
		{
			get
			{
				if (m_referenceText == null)
				{
					if (!ReferenceTextProxy.Missing)
						m_referenceText = ReferenceText.GetReferenceText(ReferenceTextProxy);
					else
					{
						// If this is a custom reference text whose identifier happens to match a standard one
						// just use it. The custom one has likely been removed because now it has become a
						// standard one.
						Debug.Assert(ReferenceTextProxy.Type == ReferenceTextType.Custom);
						ReferenceTextType type;
						if (Enum.TryParse(ReferenceTextProxy.CustomIdentifier, out type))
						{
							ChangeReferenceTextIdentifier(ReferenceTextProxy.GetOrCreate(type));
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

		public ReferenceTextProxy ReferenceTextProxy
		{
			get
			{
				return ReferenceTextProxy.GetOrCreate(m_projectMetadata.ReferenceTextType, m_projectMetadata.ProprietaryReferenceTextIdentifier);
			}
			set
			{
				if (value.Type == m_projectMetadata.ReferenceTextType && value.CustomIdentifier == m_projectMetadata.ProprietaryReferenceTextIdentifier)
					return;

				ChangeReferenceTextIdentifier(value);
				ChangeReferenceText();
			}
		}

		private void ChangeReferenceTextIdentifier(ReferenceTextProxy value)
		{
			m_projectMetadata.ReferenceTextType = value.Type;
			m_projectMetadata.ProprietaryReferenceTextIdentifier = value.CustomIdentifier;
			m_referenceText = ReferenceText.GetReferenceText(ReferenceTextProxy);
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
									blockToClear.ClearReferenceText();
							}
						}
					}
				}
			}
		}

		public string UiReferenceTextName => ReferenceTextProxy.Missing ? m_projectMetadata.ProprietaryReferenceTextIdentifier : ReferenceText.LanguageName;

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

			if (!existingProject.IsSampleProject && existingProject.NeedsQuoteParserUpgrade)
			{
				Debug.Assert(existingProject.Books.Any());
				if (existingProject.ProjectState != ProjectState.FullyInitialized)
				{
					// We need to acknowledge that the quote parse has already happened (albeit with
					// some older version of the parser). This not only might (hypothetically) tell
					// us not to do it again (although there's currently no logic that would attempt
					// that), but more importantly, it tells the data migration code that this project
					// can be migrated. And that's important because when we go to apply existing user
					// decisions, we want to do that from a project that is free of any old/weird data
					// that doesn't abide by the current expectations.
					existingProject.ProjectState = ProjectState.QuoteParseComplete;
				}
				Project upgradedProject = existingProject.IsBundleBasedProject ?
					AttemptToUpgradeByReparsingBundleData(existingProject) :
					AttemptToUpgradeByReparsingParatextData(existingProject);

				if (upgradedProject != null)
					return upgradedProject;
			}

			existingProject.InitializeLoadedProject();
			return existingProject;
		}

		/// <summary>
		/// Returns true if the project was parsed with a quote parser that is older than the current parser version.
		/// However, in some circumstances, if we know that the new version would not result in improved parsing, this
		/// property has the side-effect of just updating the project's parser version to the current version number
		/// and returning false.
		/// </summary>
		private bool NeedsQuoteParserUpgrade
		{
			get
			{
				if (m_projectMetadata.ParserVersion >= Settings.Default.ParserVersion)
					return false;
				switch (Settings.Default.ParserVersion)
				{
					case 43:
						if (m_projectMetadata.ParserVersion < 42 || QuoteSystem.QuotationDashMarker?.FirstOrDefault() == ':')
							return true;
						m_projectMetadata.ParserVersion = Settings.Default.ParserVersion;
						return false;
					case 46:
						// This change only affects the way the \qa marker is handled, and that marker is most likely only used in Psalm 119
						if (m_projectMetadata.ParserVersion < 45 || AvailableBooks.Any(b => b.Code == "PSA"))
							return true;
						m_projectMetadata.ParserVersion = Settings.Default.ParserVersion;
						return false;
					default:
						return true;
				}
			}
		}

		private static string ParserUpgradeMessage => Localizer.GetString("Project.ParserVersionUpgraded",
			"The splitting engine has been upgraded.") + " ";

		private static Project AttemptToUpgradeByReparsingBundleData(Project existingProject)
		{
			if (!RobustFile.Exists(existingProject.OriginalBundlePath))
			{
				bool upgradeProject = false;
				if (Settings.Default.ParserVersion > existingProject.m_projectMetadata.ParserUpgradeOptOutVersion)
				{
					string msg = ParserUpgradeMessage + " " + Format(
							Localizer.GetString("Project.ParserUpgradeBundleMissingMsg",
								"To make use of the new engine, the original text release bundle must be available, but it is not in the original location ({0})."),
							existingProject.OriginalBundlePath) +
						Environment.NewLine + Environment.NewLine +
						Localizer.GetString("Project.LocateBundleYourself", "Would you like to locate the text release bundle yourself?");
					string caption = Localizer.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
					if (MessageResult.Yes == MessageModal.Show(msg, caption, Buttons.YesNo))
						upgradeProject = SelectBundleForProjectDlg.GiveUserChanceToFindOriginalBundle(existingProject);
					if (!upgradeProject)
						existingProject.m_projectMetadata.ParserUpgradeOptOutVersion = Settings.Default.ParserVersion;
				}
				if (!upgradeProject)
					return null;
			}
			using (var bundle = new GlyssenBundle(existingProject.OriginalBundlePath))
			{
				var upgradedProject = new Project(existingProject.m_projectMetadata, existingProject.m_recordingProjectName,
					ws: existingProject.WritingSystem);

				Analytics.Track("UpgradeBundleProject", new Dictionary<string, string>
				{
					{"language", existingProject.LanguageIsoCode},
					{"ID", existingProject.Id},
					{"recordingProjectName", existingProject.Name},
					{"oldParserVersion", existingProject.m_projectMetadata.ParserVersion.ToString(CultureInfo.InvariantCulture)},
					{"newParserVersion", Settings.Default.ParserVersion.ToString(CultureInfo.InvariantCulture)}
				});

				return UpgradeProject(existingProject, upgradedProject, () => upgradedProject.PopulateAndParseBooks(bundle));
			}
		}

		public static Project AttemptToUpgradeByReparsingParatextData(Project existingProject)
		{
			var scrTextWrapper = existingProject.GetLiveParatextDataIfCompatible(
				Settings.Default.ParserVersion > existingProject.m_projectMetadata.ParserUpgradeOptOutVersion, ParserUpgradeMessage);
			if (scrTextWrapper == null)
			{
				existingProject.m_projectMetadata.ParserUpgradeOptOutVersion = Settings.Default.ParserVersion;
				return null;
			}
			Analytics.Track("UpgradeParatextProject", new Dictionary<string, string>
			{
				{"language", existingProject.LanguageIsoCode},
				{"ID", existingProject.Id},
				{"ParatextProjectName", existingProject.ParatextProjectName},
				{"recordingProjectName", existingProject.Name},
				{"oldParserVersion", existingProject.m_projectMetadata.ParserVersion.ToString(CultureInfo.InvariantCulture)},
				{"newParserVersion", Settings.Default.ParserVersion.ToString(CultureInfo.InvariantCulture)}
			});
			return existingProject.UpdateProjectFromParatextData(scrTextWrapper);
		}

		internal ParatextScrTextWrapper GetLiveParatextDataIfCompatible(bool canInteractWithUser = true,
			string contextMessage = "", bool checkForChangesInAvailableBooks = true, bool forceReload = false)
		{
			ParatextScrTextWrapper scrTextWrapper = null;
			ScrText sourceScrText = null;
			do
			{
				try
				{
					sourceScrText = GetSourceParatextProject();
				}
				catch (ProjectNotFoundException e)
				{
					if (canInteractWithUser)
					{
						string msg = contextMessage + Format(
								Localizer.GetString("Project.ParatextProjectMissingMsg",
									"To update the {0} project, the {1} project {2} must be available, but it is not.",
									"Param 0: Glyssen recording project name; " +
									"Param 1: \"Paratext\" (product name); " +
									"Param 2: Paratext project short name (unique project identifier)"),
								Name, ParatextScrTextWrapper.kParatextProgramName, e.ProjectName) +
							Environment.NewLine + Environment.NewLine +
							Format(Localizer.GetString("Project.RestoreParatextProject",
									"If possible, you can restore the {0} project and retry; otherwise, you can cancel and {1} will continue to work with the existing project data.",
									"Param 0: \"Paratext\" (product name); Param 1: \"Glyssen\" (product name)"),
								ParatextScrTextWrapper.kParatextProgramName, GlyssenInfo.kProduct);

						string caption = Format(Localizer.GetString("Project.ParatextProjectUnavailable", "{0} Project Unavailable",
								"Param is \"Paratext\" (product name)"),
							ParatextScrTextWrapper.kParatextProgramName);
						if (MessageResult.Retry == MessageModal.Show(msg, caption, Buttons.RetryCancel))
							continue;
					}
					return null;
				}
			} while (sourceScrText == null); // retry

			if (forceReload)
			{
				bool reloadSucceeded = false;
				do
				{
					try
					{
						sourceScrText.Reload();
						reloadSucceeded = true;
					}
					catch (Exception e)
					{
						if (canInteractWithUser)
						{
							string msg = contextMessage + Format(
								Localizer.GetString("Project.ParatextProjectReloadFailure",
									"An error occurred reloading the {0} project {1}:\r\n{2}\r\n\r\n" +
									"If you cannot fix the problem, you can cancel and continue to work with the existing project data.",
									"Param 0: \"Paratext\" (product name); " +
									"Param 1: Paratext project short name (unique project identifier); " +
									"Param 2: Specific error message"),
								ParatextScrTextWrapper.kParatextProgramName, ParatextProjectName, e.Message);
							if (MessageResult.Retry == MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.RetryCancel))
								continue;
						}
						return null;
					}
				} while (!reloadSucceeded);
			}

			try
			{
				scrTextWrapper = new ParatextScrTextWrapper(sourceScrText, QuoteSystemStatus != QuoteSystemStatus.Obtained);
				if (!scrTextWrapper.IsMetadataCompatible(Metadata))
				{
					throw new ApplicationException(Format(Localizer.GetString("Project.ParatextProjectMetadataChangedMsg",
						"The settings of the {0} project {1} no longer appear to correspond to the {2} project. " +
						"This is an unusual situation. If you do not understand how this happened, please contact support.",
						"Param 0: \"Paratext\" (product name); " +
						"Param 1: Paratext project short name (unique project identifier); " +
						"Param 2: \"Glyssen\" (product name)"),
						ParatextScrTextWrapper.kParatextProgramName,
						ParatextProjectName,
						GlyssenInfo.kProduct));
				}
			}
			catch (ApplicationException e)
			{
				if (canInteractWithUser)
				{
					string msg = contextMessage + Format(Localizer.GetString("Project.ParatextProjectUpdateErrorMsg",
						"To update the {0} project, {1} attempted to get the current text of the books from the {2} project {3}, but there was a problem:",
						"Param 0: Glyssen recording project name; " +
						"Param 1: \"Glyssen\" (product name); " +
						"Param 2: \"Paratext\" (product name); " +
						"Param 3: Paratext project short name (unique project identifier)"),
						Name,
						GlyssenInfo.kProduct,
						ParatextScrTextWrapper.kParatextProgramName,
						sourceScrText.Name) +
						Environment.NewLine + e.Message;

					MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.OK, Icon.Exclamation);
				}
				return null;
			}

			return !checkForChangesInAvailableBooks ||
				!FoundUnacceptableChangesInAvailableBooks(scrTextWrapper, contextMessage, canInteractWithUser) ?
				scrTextWrapper : null;
		}

		private bool FoundUnacceptableChangesInAvailableBooks(ParatextScrTextWrapper scrTextWrapper, string contextMessage, bool canInteractWithUser)
		{
			// Any of the following scenarios is possible:
			// 1) Exactly the same books are available and passing checks (or were previously added by overiding the
			//    check status). This is the happy path!
			// 2) Some of the "Available", but not "Included" books in the existing project are no longer available.
			//    => We can just remove them from the list of available books and delete the corresponding file (if any)
			//       stored in the project.
			// 3) Some of the "Included" books in the existing project are no longer available.
			//    => If we're interacting with the user, we can ask them whether to proceed and remove the deleted books.
			//       If not, return true (i.e., continue to opt out of this version of the parser for now)
			// 4) Some of the "Included" books in the existing project that previously passed the recommended checks no
			//       longer do.
			//    => If we're interacting with the user, we can ask them whether to proceed and exclude them for now
			//       (they can re-add them later in the Selected Books dialog).
			//       If not, return true (i.e., continue to opt out of this version of the parser for now)
			// 5) Some additional books are now available that were not previously.
			//    a) Checks pass
			//       => We will add the new books as available. We will only parse them and automatically
			//          include them in the project if we were previously including all available books.
			//    b) Checks fail
			//       => We will add the new books as available but not parse them or include them in the project.
			if (canInteractWithUser)
			{
				var noLongerAvailableBookIds = new List<string>();
				var noLongerPassingListBookIds = new List<string>();

				HandleDifferencesInAvailableBooks(scrTextWrapper, null,
					bookCode => noLongerAvailableBookIds.Add(bookCode),
					bookCode => noLongerPassingListBookIds.Add(bookCode));

				if (!noLongerAvailableBookIds.Any() && !noLongerPassingListBookIds.Any())
					return false;

				string msg = contextMessage + Format(Localizer.GetString("Project.ParatextProjectUpdateExcludedBooksWarning",
						"{1} detected changes in the {2} project {3} that would result in the exclusion " +
						"of books from the {0} project that were previously included:",
						"Param 0: \"Glyssen\" (product name); " +
						"Param 1: \"Paratext\" (product name); " +
						"Param 2: Paratext project short name (unique project identifier); " +
						"Param 3: Glyssen recording project name"),
					GlyssenInfo.kProduct,
					ParatextScrTextWrapper.kParatextProgramName,
					ParatextProjectName,
					Name);

				if (noLongerAvailableBookIds.Any())
				{
					msg += Environment.NewLine + Localizer.GetString("Project.ParatextBooksNoLongerAvailable",
							"The following books are no longer available:") + Environment.NewLine + "   " +
						Join(Localizer.GetString("Common.SimpleListSeparator", ", "), noLongerAvailableBookIds);
				}
				if (noLongerPassingListBookIds.Any())
				{
					var scriptureRangeSelectionDlgName = Localizer.GetString(
						"DialogBoxes.ScriptureRangeSelectionDlg.WindowTitle", "Select Books - {0}", "{0} is the project name");
					Debug.Assert(Localizer.UILanguageId != "en" || scriptureRangeSelectionDlgName == "Select Books - {0}",
						"Dev alert: this localized string and ID MUST be kept in sync with the version in ScriptureRangeSelectionDlg.Designer.cs!");
					scriptureRangeSelectionDlgName = Format(scriptureRangeSelectionDlgName, "");
					while (!Char.IsLetter(scriptureRangeSelectionDlgName.Last()))
						scriptureRangeSelectionDlgName = scriptureRangeSelectionDlgName.Remove(scriptureRangeSelectionDlgName.Length - 1);
					msg += Environment.NewLine + Format(Localizer.GetString("Project.ParatextBooksNoLongerPassChecks",
							"The following books no longer appear to pass the basic checks:\r\n   {0}\r\n" +
							"(If needed, you can include these books again later in the {1} dialog box.)",
							"Param 0: list of 3-letter book IDs; " +
							"Param 1: Name of the \"Select Books\" dialog box."),
						Join(Localizer.GetString("Common.SimpleListSeparator", ", "), noLongerPassingListBookIds),
						scriptureRangeSelectionDlgName);
				}

				msg += Environment.NewLine + Environment.NewLine +
					Localizer.GetString("Project.ParatextProjectUpdateConfirmExcludeBooks",
						"Would you like to proceed with the update?");

				return MessageResult.No == MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.YesNo, Icon.Exclamation);
			}

			try
			{
				HandleDifferencesInAvailableBooks(scrTextWrapper, null,
					bookCode => throw new ApplicationException($"Book {bookCode} is " +
						$"included in the project but is no longer available from Paratext project {ParatextProjectName}."),
					bookCode => throw new ApplicationException($"Book {bookCode} is included in the project but " +
						"Paratext reports that it does not currently pass basic checks."));
				return false;
			}
			catch (ApplicationException e)
			{
				Logger.WriteError(e);
				return true;
			}
		}

		private void HandleDifferencesInAvailableBooks(IParatextScrTextWrapper scrTextWrapper,
			Action<string> nowMissingPreviouslyExcluded,
			Action<string> nowMissingPreviouslyIncluded,
			Action<string> noLongerPassChecksPreviouslyIncludedWithoutCheckStatusOverride,
			Action<string> newlyAvailableChecksPass = null,
			Action<string> newlyAvailableChecksFail = null,
			Action<string> foundInBoth = null)
		{
			var existingAvailable = (IReadOnlyList<Book>)m_projectMetadata.AvailableBooks;
			var nowAvailable = scrTextWrapper.AvailableBooks;
			var x = 0;
			foreach (Book nowAvailableBook in nowAvailable)
			{
				var nowAvailableBookNum = Canon.BookIdToNumber(nowAvailableBook.Code);
				if (x < existingAvailable.Count)
				{
					var existingBookNum = Canon.BookIdToNumber(existingAvailable[x].Code);
					if (existingAvailable[x].Code == nowAvailableBook.Code)
					{
						if (existingAvailable[x].IncludeInScript &&
							!GetBook(existingAvailable[x].Code).CheckStatusOverridden &&
							!scrTextWrapper.DoesBookPassChecks(existingBookNum))
						{
							noLongerPassChecksPreviouslyIncludedWithoutCheckStatusOverride?.Invoke(existingAvailable[x].Code);
						}
						else
							foundInBoth?.Invoke(existingAvailable[x].Code);
						x++;
						continue;
					}
					if (existingBookNum < nowAvailableBookNum)
					{
						if (existingAvailable[x].IncludeInScript)
							nowMissingPreviouslyIncluded?.Invoke(existingAvailable[x].Code);
						else
							nowMissingPreviouslyExcluded.Invoke(existingAvailable[x].Code);
						continue;
					}
				}

				// New available book.
				if (scrTextWrapper.DoesBookPassChecks(nowAvailableBookNum))
					newlyAvailableChecksFail?.Invoke(nowAvailableBook.Code);
				else
					newlyAvailableChecksPass?.Invoke(nowAvailableBook.Code);
			}
		}

		internal Project UpdateProjectFromParatextData(IParatextScrTextWrapper scrTextWrapper)
		{
			var existingAvailable = m_projectMetadata.AvailableBooks;
			var upgradedProject = new Project(m_projectMetadata, Name);
			// Initially we want to include anything that is included in Paratext so we don't lose user decisions.
			scrTextWrapper.IncludeBooks(upgradedProject.AvailableBooks.Select(b => b.Code));

			// Add metadata for any books that are available in scrTextWrapper but not in the
			// existing project. Remove metadata for any books formerly available that are not now.
			bool foundDataChange = false;
			Action<string> nowMissing = bookCode =>
			{
				var origPath = GetBookDataFilePath(bookCode);
				RobustFile.Move(origPath, origPath + ".nolongeravailable");
				foundDataChange = true;
			};

			List<String> booksToExcludeFromProject = new List<string>();
			Action<string> exclude = bookCode => booksToExcludeFromProject.Add(bookCode);

			// For any newly available book that passes checks, if all existing books are included,
			// we assume we want to include anything new as well.
			var excludeNewBooks = existingAvailable.Any(b => b.IncludeInScript == false);
			Action<string> handleNewPassingBook = (bookCode) =>
				{
					if (excludeNewBooks)
						booksToExcludeFromProject.Add(bookCode);
					foundDataChange = true;
				};

			if (upgradedProject.QuoteSystemStatus == QuoteSystemStatus.Obtained && scrTextWrapper.HasQuotationRulesSet)
			{
				upgradedProject.SetWsQuotationMarksUsingFullySpecifiedContinuers(scrTextWrapper.QuotationMarks);
				foundDataChange |= upgradedProject.QuoteSystem != QuoteSystem;
			}
			else
				CopyQuoteMarksIfAppropriate(upgradedProject.WritingSystem, upgradedProject.m_projectMetadata);

			HandleDifferencesInAvailableBooks(scrTextWrapper, nowMissing, nowMissing,
				exclude, handleNewPassingBook, exclude);

			void OnUpgradedProjectOnQuoteParseCompleted(object sender, EventArgs e)
			{
				upgradedProject.QuoteParseCompleted -= OnUpgradedProjectOnQuoteParseCompleted;

				foreach (var book in upgradedProject.AvailableBooks.Where(b => b.IncludeInScript && booksToExcludeFromProject.Contains(b.Code)))
					book.IncludeInScript = false;

				foreach (var book in upgradedProject.IncludedBooks)
				{
					book.ParatextChecksum = scrTextWrapper.GetBookChecksum(book.BookNumber);

					if (!foundDataChange)
					{
						var existingBook = GetBook(book.BookNumber);
						if (existingBook == null || book.ParatextChecksum != existingBook.ParatextChecksum)
							foundDataChange = true;
					}
				}
				if (foundDataChange)
					upgradedProject.m_projectMetadata.Revision++; // See note on GlyssenDblTextMetadata.RevisionOrChangesetId

				upgradedProject.ProjectStateChanged?.Invoke(upgradedProject, new ProjectStateChangedEventArgs());
			}

			upgradedProject.QuoteParseCompleted += OnUpgradedProjectOnQuoteParseCompleted;

			UpgradeProject(this, upgradedProject, () =>
			{
				upgradedProject.ParseAndSetBooks(scrTextWrapper.UsxDocumentsForIncludedBooks, scrTextWrapper.Stylesheet);
			});

			return upgradedProject;
		}

		private static Project UpgradeProject(Project existingProject, Project upgradedProject, Action populateAndParseBooks)
		{
			upgradedProject.UserDecisionsProject = existingProject;
			populateAndParseBooks();
			// While that's happening, we can migrate the existing project if necessary
			existingProject.UpdateControlFileVersion();
			return upgradedProject;
		}

		public static void SetHiddenFlag(string projectFilePath, bool hidden)
		{
			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					Localizer.GetString("File.ProjectCouldNotBeModified", "Project could not be modified: {0}"), projectFilePath);
				return;
			}
			metadata.Inactive = hidden;
			new Project(metadata, GetRecordingProjectNameFromProjectFilePath(projectFilePath)).Save();
			// TODO: preserve WritingSystemRecoveryInProcess flag
		}

		private static void DeleteProjectFolderAndEmptyContainingFolders(string projectFolder)
		{
			if (Directory.Exists(projectFolder))
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
			return path.GetContainingFolderName();
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
			var isWritable = !FileHelper.IsLocked(projectFilePath);
			if (!isWritable)
			{
				MessageModal.Show(Localizer.GetString("Project.NotWritableMsg",
					"The project file is not writable. No changes will be saved."));
			}

			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					Localizer.GetString("File.ProjectMetadataInvalid", "Project could not be loaded: {0}"), projectFilePath);
				return null;
			}

			Project project;
			try
			{
				project = new Project(metadata, GetRecordingProjectNameFromProjectFilePath(projectFilePath), true);
			}
			catch(ProjectNotFoundException e)
			{
				throw new ApplicationException(Format(Localizer.GetString("Project.ParatextProjectNotFound",
					"Unable to access the {0} project {1}, which is needed to load the {2} project {3}.\r\n\r\nTechnical details:",
					"Param 0: \"Paratext\" (product name); " +
					"Param 1: Paratext project short name (unique project identifier); " +
					"Param 2: \"Glyssen\" (product name); " +
					"Param 3: Glyssen recording project name"),
					ParatextScrTextWrapper.kParatextProgramName,
					metadata.ParatextProjectId,
					GlyssenInfo.kProduct,
					metadata.Name), e);
			}
			project.ProjectFileIsWritable = isWritable;

			var projectDir = Path.GetDirectoryName(projectFilePath);
			Debug.Assert(projectDir != null);
			ProjectUtilities.ForEachBookFileInProject(projectDir,
				(bookId, fileName) => project.m_books.Add(BookScript.Deserialize(fileName, project.Versification)));
			project.RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();

			// For legacy projects
			if (project.CharacterGroupList.CharacterGroups.Any() &&
				project.CharacterGroupGenerationPreferences.CastSizeOption == CastSizeOption.NotSet)
				project.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;

			return project;
		}

		private void RemoveAvailableBooksThatDoNotCorrespondToExistingBooks()
		{
			ScrText sourceParatextProject = null;

			for (int i = 0; i < m_metadata.AvailableBooks.Count; i++)
			{
				if (!m_books.Any(b => b.BookId == m_metadata.AvailableBooks[i].Code))
				{
					if (IsLiveParatextProject)
					{
						// For Paratext-based projects, there are three possible situations here:
						// 1) The book is missing because (as of the last time we updated from Paratext), it
						//    did not pass the required checks, so it was not included in the script. In this
						//    case, we just assume it's still in the project and is available if the user
						//    decides to include it (we'll check at that time to see if it passes the checks).
						// 2) We need to remove the book because it is no longer available from the Paratext project.
						// 3) The book is still available from the Paratext project, but Glyssen's copy of it was
						//    (manually?) deleted. We can continue to keep it in the available list and just mark it
						//    as no longer included.
						if (m_metadata.AvailableBooks[i].IncludeInScript)
						{
							if (sourceParatextProject == null)
							{
								try
								{
									sourceParatextProject = GetSourceParatextProject();
								}
								catch (ProjectNotFoundException e)
								{
									Logger.WriteError($"Paratext project not found. Removing {m_metadata.AvailableBooks[i].Code}.", e);
								}
							}
							if (sourceParatextProject != null && sourceParatextProject.BookPresent(Canon.BookIdToNumber(m_metadata.AvailableBooks[i].Code)))
								m_metadata.AvailableBooks[i].IncludeInScript = false;
							else
								m_metadata.AvailableBooks.RemoveAt(i--);
						}
					}
					else
						m_metadata.AvailableBooks.RemoveAt(i--);
				}
			}
		}

		protected override void SetVersification(ScrVers versification)
		{
			base.SetVersification(versification);

			ProjectCharacterVerseData = new ProjectCharacterVerseData(ProjectCharacterVerseDataPath, Versification);
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
			if (m_projectMetadata.ControlFileVersion != ControlCharacterVerseData.Singleton.ControlFileVersion)
			{
				const int kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan = 78;
				new CharacterAssigner(new CombinedCharacterVerseData(this)).AssignAll(m_books,
					m_projectMetadata.ControlFileVersion < kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan);

				UpdateControlFileVersion();
			}
			UpdatePercentInitialized();
			ProjectState = ProjectState.FullyInitialized;
			UpdateControlFileVersion();
			Analyze();
		}

		private void UpdateControlFileVersion()
		{
			if (m_projectMetadata.ControlFileVersion == 0)
				m_projectMetadata.ControlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;
			else
			{
				if (ProjectDataMigrator.MigrateProjectData(this, m_projectMetadata.ControlFileVersion) == ProjectDataMigrator.MigrationResult.Complete)
					m_projectMetadata.ControlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;
			}
		}

		private void ApplyUserDecisions(Project sourceProject)
		{
			var referenceTextToReapply = (sourceProject.ReferenceText == ReferenceText) ? ReferenceText : null;
			Debug.Assert(referenceTextToReapply != null, "Reference texts should be the same.");
			foreach (var targetBookScript in m_books)
			{
				var sourceBookScript = sourceProject.m_books.SingleOrDefault(b => b.BookId == targetBookScript.BookId);
				if (sourceBookScript != null)
				{
					targetBookScript.ApplyUserDecisions(sourceBookScript, ReferenceText);
				}
			}
			Analyze();
		}

		private void PopulateAndParseBooks(ITextBundle bundle)
		{
			ParseAndSetBooks(bundle.UsxBooksToInclude, bundle.Stylesheet);
		}

		/// <summary>
		/// Inserts the specified book into its proper location in the list of existing included books
		/// </summary>
		/// <param name="book">The bookscript, either from a prior call to FluffUpBookFromFileIfPossible or as newly created by
		/// the USX parser</param>
		/// <exception cref="InvalidOperationException">The project is not in a valid state for the book to be included.</exception>
		public void IncludeExistingBook(BookScript book)
		{
			Debug.Assert(IsLiveParatextProject, "Heads up! This might not be a problem, but we really only anticipated this method being used for Paratext-based projects.");
			var bookMetadata = AvailableBooks.SingleOrDefault(b => b.Code == book.BookId);
			if (bookMetadata == null)
				throw new InvalidOperationException($"Attempt to include the bookscript for {book.BookId}, but the project contains no metadata for the book.");
			if (!bookMetadata.IncludeInScript)
				throw new InvalidOperationException($"Attempt to include the bookscript for {book.BookId}, but the metadata for the book indicates that it should not be included.");

			int i;
			for (i = 0; i < m_books.Count; i++)
			{
				if (m_books[i].BookNumber > book.BookNumber)
					break;
				if (m_books[i].BookNumber == book.BookNumber)
				{
					m_books[i] = book;
					return;
				}
			}
			m_books.Insert(i, book);
		}

		internal void IncludeBooksFromParatext(ParatextScrTextWrapper wrapper, ISet<int> bookNumbers,
			Action<BookScript> postParseAction)
		{
			wrapper.IncludeBooks(bookNumbers.Select(BCVRef.NumberToBookCode));
			var usxBookInfoList = wrapper.GetUsxDocumentsForIncludedParatextBooks(bookNumbers);

			void EnhancedPostParseAction(BookScript book)
			{
				book.ParatextChecksum = usxBookInfoList.GetCheckum(book.BookNumber);
				if (!usxBookInfoList.GetPassesChecks(book.BookNumber))
					book.CheckStatusOverridden = true;
				postParseAction?.Invoke(book);
			}

			ParseAndIncludeBooks(usxBookInfoList, wrapper.Stylesheet, EnhancedPostParseAction);
		}

		private void ParseAndSetBooks(IEnumerable<UsxDocument> books, IStylesheet stylesheet)
		{
			if (m_books.Any())
				throw new InvalidOperationException("Project already contains books. If the intention is to replace the existing ones, let's clear the list first. Otherwise, call ParseAndIncludeBooks.");
			ParseAndIncludeBooks(books, stylesheet);
		}

		private void ParseAndIncludeBooks(IEnumerable<UsxDocument> books, IStylesheet stylesheet, Action<BookScript> postParseAction = null)
		{
			if (Versification == null)
				throw new NullReferenceException("What!!!");
			ProjectState = ProjectState.Initial | (ProjectState & ProjectState.WritingSystemRecoveryInProcess);
			var usxWorker = new BackgroundWorker {WorkerReportsProgress = true};
			usxWorker.DoWork += UsxWorker_DoWork;
			usxWorker.RunWorkerCompleted += UsxWorker_RunWorkerCompleted;
			usxWorker.ProgressChanged += UsxWorker_ProgressChanged;

			object[] parameters = {books, stylesheet, postParseAction};
			usxWorker.RunWorkerAsync(parameters);
		}

		private void UsxWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var parameters = (object[]) e.Argument;
			var books = (IEnumerable<UsxDocument>) parameters[0];
			var stylesheet = (IStylesheet) parameters[1];
			var postParseAction = parameters.Length > 2 ? (Action <BookScript> )parameters[2] : null;

			var backgroundWorker = (BackgroundWorker)sender;

			var parsedBooks = UsxParser.ParseBooks(books, stylesheet, i => backgroundWorker.ReportProgress(i));

			if (postParseAction != null)
			{
				foreach (var book in parsedBooks)
					postParseAction(book);
			}
			e.Result = parsedBooks;
		}

		private void UsxWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			var bookScripts = (List<BookScript>) e.Result;

			foreach (var bookScript in bookScripts)
			{
				// This code is an attempt to figure out how we are getting null reference exceptions when using the objects in the list (See PG-275 & PG-287)
				if (bookScript == null || bookScript.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException(Format("{0} Number of BookScripts: {1}. BookScripts which are NOT null: {2}", initialMessage,
						bookScripts.Count, nonNullBookScriptsStr));
				}

				bookScript.Initialize(Versification);
			}

			if (m_books.Any())
			{
				foreach (var book in bookScripts)
					IncludeExistingBook(book);
			}
			else
			{
				m_books.AddRange(bookScripts);
				m_projectMetadata.ParserVersion = Settings.Default.ParserVersion;
				if (m_books.All(b => IsNullOrEmpty(b.PageHeader)))
					ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
				UpdateControlFileVersion();
				RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();
			}

			if (QuoteSystem == null)
				GuessAtQuoteSystem();
			else if (IsQuoteSystemReadyForParse)
				DoQuoteParse(bookScripts.Select(b => b.BookId));
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

		private void DoQuoteParse(IEnumerable<string> booksToParse = null)
		{
			m_projectMetadata.ParserVersion = Settings.Default.ParserVersion;
			ProjectState = ProjectState.Parsing;
			var quoteWorker = new BackgroundWorker {WorkerReportsProgress = true};
			quoteWorker.DoWork += QuoteWorker_DoWork;
			quoteWorker.RunWorkerCompleted += QuoteWorker_RunWorkerCompleted;
			quoteWorker.ProgressChanged += QuoteWorker_ProgressChanged;
			object[] parameters = { booksToParse };
			quoteWorker.RunWorkerAsync(parameters);
		}

		private void QuoteWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var bookIds = (IEnumerable<string>)((object[])e.Argument)[0];
			QuoteParser.ParseProject(this, sender as BackgroundWorker, bookIds);
		}

		private void QuoteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Debug.WriteLine(e.Error.InnerException.Message + e.Error.InnerException.StackTrace);
				throw e.Error;
			}

			ProjectState = ProjectState.QuoteParseComplete;

			UpdateControlFileVersion();
			if (UserDecisionsProject != null)
			{
				Debug.Assert(UserDecisionsProject.m_projectMetadata.ControlFileVersion == m_projectMetadata.ControlFileVersion);
				ApplyUserDecisions(UserDecisionsProject);
				UserDecisionsProject = null;
			}
			else
				Analyze();

			ClearCharacterStatistics();
			Save();

			QuoteParseCompleted?.Invoke(this, new EventArgs());
		}

		private void QuoteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_quotePercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(UpdatePercentInitialized(), null);
			OnReport(pe);
		}

		public static string GetProjectFilePath(string langId, string publicationId, string recordingProjectId)
		{
			return Path.Combine(GetProjectFolderPath(langId, publicationId, recordingProjectId), langId + Constants.kProjectFileExtension);
		}

		public string ProjectFileName => LanguageIsoCode + Constants.kProjectFileExtension;

		public static string GetDefaultProjectFilePath(IBundle bundle)
		{
			return GetProjectFilePath(bundle.LanguageIso, bundle.Id, GetDefaultRecordingProjectName(bundle.Name));
		}

		internal static string GetDefaultProjectFilePath(ParatextScrTextWrapper textWrapper)
		{
			return GetProjectFilePath(textWrapper.LanguageIso3Code, textWrapper.GlyssenDblTextMetadata.Id, GetDefaultRecordingProjectName(textWrapper.ProjectFullName));
		}

		// Total path limit is 260 (MAX_PATH) characters. Need to allow for:
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\ProjectCharacterDetail.txt
		// C:\ProgramData\FCBH-SIL\Glyssen\<ISO>\xxxxxxxxxxxxxxxx\<Recording Project Name>\<ISO>.glyssen
		public int MaxProjectNameLength => kMaxPath - Path.Combine(ProjectsBaseFolder, LanguageIsoCode, m_metadata.Id).Length -
			Math.Max(ProjectFileName.Length, kProjectCharacterDetailFileName.Length) - 3; // the magic 3 allows for three Path.DirectorySeparatorChar's

		public static string GetProjectFolderPath(string langId, string publicationId, string recordingProjectId)
		{
			return Path.Combine(ProjectsBaseFolder, langId, publicationId, recordingProjectId);
		}

		public static string GetPublicationFolderPath(IBundle bundle)
		{
			return Path.Combine(ProjectsBaseFolder, bundle.LanguageIso, bundle.Id);
		}

		public static string GetPublicationFolderPath(ScrText scrText)
		{
			return Path.Combine(ProjectsBaseFolder, scrText.Language.LanguageId.Iso6393Code, scrText.Name);
		}

		public static string GetLanguageFolderPath(IBundle bundle)
		{
			return Path.Combine(ProjectsBaseFolder, bundle.LanguageIso);
		}

		public static string GetLanguageFolderPath(string langId)
		{
			return Path.Combine(ProjectsBaseFolder, langId);
		}

		public string ProjectFilePath => GetProjectFilePath(LanguageIsoCode, m_metadata.Id, m_recordingProjectName);

		private string ProjectCharacterVerseDataPath => Path.Combine(ProjectFolder, kProjectCharacterVerseFileName);

		private string ProjectCharacterDetailDataPath => Path.Combine(ProjectFolder, kProjectCharacterDetailFileName);

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

		protected override string ProjectFolder => GetProjectFolderPath(m_metadata.Language.Iso, m_metadata.Id, m_recordingProjectName);

		private string LanguageFolder => GetLanguageFolderPath(m_metadata.Language.Iso);

		private string GetBookDataFilePath(string bookId)
		{
			return Path.ChangeExtension(Path.Combine(ProjectFolder, bookId), "xml");
		}

		public bool DoesBookScriptFileExist(string bookId)
		{
			return RobustFile.Exists(GetBookDataFilePath(bookId));
		}

		public BookScript FluffUpBookFromFileIfPossible(string bookId)
		{
			var path = GetBookDataFilePath(bookId);
			if (RobustFile.Exists(path))
			{
				Exception error;
				var bookScript = BookScript.Deserialize(GetBookDataFilePath(bookId), Versification, out error);
				if (error != null)
					ErrorReport.ReportNonFatalException(error);
				return bookScript;
			}
			return null;
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

		internal void PrepareForExport()
		{
			if (!HasVersificationFile)
			{
				try
				{
					Versification.Save(FallbackVersificationFilePath);
				}
				catch (Exception e)
				{
					Logger.WriteError($"Failed to save fallback versification file to {FallbackVersificationFilePath}", e);
				}
			}
		}

		internal void ExportCompleted()
		{
			RobustFile.Delete(FallbackVersificationFilePath);
		}

		public void Save(bool saveCharacterGroups = false)
		{
			if (!m_projectFileIsWritable)
				return;

			Directory.CreateDirectory(ProjectFolder);

			m_metadata.LastModified = DateTime.Now;
			var projectPath = ProjectFilePath;
			Exception error;
			XmlSerializationHelper.SerializeToFile(projectPath, m_projectMetadata, out error);
			if (error != null)
			{
				MessageModal.Show(error.Message);
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
			XmlSerializationHelper.SerializeToFile(GetBookDataFilePath(book.BookId), book, out error);
			if (error != null)
			{
				MessageModal.Show(error.Message);
			}
		}

		public void SaveProjectCharacterVerseData()
		{
			ProjectCharacterVerseData.WriteToFile(ProjectCharacterVerseDataPath);
		}

		public void SaveProjectCharacterDetailData()
		{
			ProjectCharacterDetailData.WriteToFile(ProjectCharacterDetail, ProjectCharacterDetailDataPath);
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
			m_characterGroupList = RobustFile.Exists(path)
				? CharacterGroupList.LoadCharacterGroupListFromFile(path, this)
				: new CharacterGroupList();
			m_characterGroupList.CharacterGroups.CollectionChanged += CharacterGroups_CollectionChanged;
			if (m_voiceActorList != null)
				EnsureCastSizeOptionValid();
		}

		void CharacterGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CharacterGroupCollectionChanged != null)
				CharacterGroupCollectionChanged(this, new EventArgs());
		}

		private void LoadVoiceActorInformationData()
		{
			string path = Path.Combine(ProjectFolder, kVoiceActorInformationFileName);
			m_voiceActorList = (RobustFile.Exists(path)) ? VoiceActorList.LoadVoiceActorListFromFile(path) : new VoiceActorList();
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

		public VoiceActor GetVoiceActorForCharacter(string characterId)
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
				bool attemptToUseBackup = RobustFile.Exists(backupPath);

				do
				{
					if (!RobustFile.Exists(LdmlFilePath))
					{
						if (attemptToUseBackup)
						{
							try
							{
								RobustFile.Move(backupPath, LdmlFilePath);
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
						if (!RobustFile.Exists(LdmlFilePath))
							break;
					}
					try
					{
						new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, m_wsDefinition);
						return m_wsDefinition;
					}
					catch (XmlException e)
					{
						var msg1 = Format(Localizer.GetString("Project.LdmlFileLoadError",
								"The writing system definition file for project {0} could not be read:\n{1}\nError: {2}",
								"Param 0: project name; Param 1: LDML filename; Param 2: XML Error message"),
							Name, LdmlFilePath, e.Message);
						var msg2 = Format(attemptToUseBackup
							? Localizer.GetString("Project.UseBackupLdmlFile",
								"To use the automatically created backup (which might be out-of-date), click {0}.",
								"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
								"created backup file exists. Param is \"Retry\" button label.")
							: Localizer.GetString("Project.AdvancedUserLdmlRepairInstructions",
								"If you can replace it with a valid backup or know how to repair it yourself, do so and then click {0}.",
								"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
								"created backup file does not exist. Param is \"Retry\" button label."), MessageBoxStrings.RetryButton);
						var msg3 = Format(Localizer.GetString("Project.IgnoreToRepairLdmlFile",
							"Otherwise, click {0} and {1} will repair the file for you. Some information might not be recoverable, " +
							"so check the quote system and font settings carefully.",
							"Param 0: \"Ignore\" button label; " +
							"Param 1: \"Glyssen\" (product name)"),
							MessageBoxStrings.IgnoreButton, GlyssenInfo.kProduct);
						var msg = msg1 + "\n\n" + msg2 + msg3;
						Logger.WriteError(msg, e);

						switch (MessageModal.Show(msg, GlyssenInfo.kProduct, Buttons.AbortRetryIgnore, Icon.Warning, DefaultButton.Button2))
						{
							default:
								ProjectState |= ProjectState.WritingSystemRecoveryInProcess;
								retry = false;
								break;
							case MessageResult.Retry:
								if (attemptToUseBackup)
								{
									try
									{
										string corruptedLdmlFilePath =
											Path.ChangeExtension(LdmlFilePath, DblBundleFileUtils.kUnzippedLdmlFileExtension + "corrupted");
										RobustFile.Delete(corruptedLdmlFilePath);
										RobustFile.Move(LdmlFilePath, corruptedLdmlFilePath);
										RobustFile.Move(backupPath, LdmlFilePath);
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
							case MessageResult.Abort:
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
				if (RobustFile.Exists(LdmlFilePath))
				{
					backupPath = LdmlBackupFilePath;
					if (RobustFile.Exists(backupPath))
						RobustFile.Delete(backupPath);
					RobustFile.Move(LdmlFilePath, backupPath);
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
			catch(FileLoadException)
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
						RobustFile.Delete(LdmlFilePath);
						RobustFile.Move(backupPath, LdmlFilePath);
						new LdmlDataMapper(new WritingSystemFactory()).Read(LdmlFilePath, wsFromBackup);
						if (!wsFromBackup.QuotationMarks.SequenceEqual(WritingSystem.QuotationMarks) ||
							wsFromBackup.DefaultFont.Name != WritingSystem.DefaultFont.Name ||
							wsFromBackup.RightToLeftScript != WritingSystem.RightToLeftScript)
						{
							// There were significant changes that we couldn't save. Something bad is probably going to happen, so we need to tell the user.
							ErrorReport.ReportNonFatalExceptionWithMessage(exSave, Localizer.GetString("Project.RevertedToOutdatedBackupWs",
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
			Project copyOfExistingProject = new Project(m_projectMetadata, Name, ws: WritingSystem);
			copyOfExistingProject.m_books.AddRange(m_books);

			m_books.Clear();

			if (IsBundleBasedProject)
			{
				if (RobustFile.Exists(OriginalBundlePath) && QuoteSystem != null)
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
			else
			{
				var scrTextWrapper = GetLiveParatextDataIfCompatible(false, "", false);
				if (scrTextWrapper != null && QuoteSystem != null)
				{
					scrTextWrapper.IncludeOverriddenBooksFromProject(copyOfExistingProject);
					UserDecisionsProject = copyOfExistingProject;
					ParseAndSetBooks(scrTextWrapper.UsxDocumentsForIncludedBooks, scrTextWrapper.Stylesheet);
				}
			}
		}

		internal void CreateBackup(string textToAppendToRecordingProjectName, bool hidden = true)
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
				var newFilePath = Directory.GetFiles(newDirectoryPath, "*" + Constants.kProjectFileExtension).FirstOrDefault();
				if (newFilePath != null)
					SetHiddenFlag(newFilePath, true);
			}
		}

		// Note: For live Paratext projects, the only proper way to change the quote system is via Paratext.
		public bool IsOkayToChangeQuoteSystem => QuoteSystem != null && IsBundleBasedProject &&
			RobustFile.Exists(OriginalBundlePath);

		private void OnReport(ProgressChangedEventArgs e)
		{
			ProgressChanged?.Invoke(this, e);
		}

		public bool IsSampleProject => Id.Equals(SampleProject.kSample, StringComparison.OrdinalIgnoreCase) && LanguageIsoCode == SampleProject.kSample;

		internal static string GetDefaultRecordingProjectName(string publicationName)
		{
			publicationName = FileSystemUtils.RemoveDangerousCharacters(publicationName, MaxBaseRecordingNameLength);
			return $"{publicationName}{DefaultRecordingProjectNameSuffix}";
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

		public Dictionary<string, int>.KeyCollection AllCharacterIds => KeyStrokesByCharacterId.Keys;

		public int TotalCharacterCount => KeyStrokesByCharacterId.Count;

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
				var narratorToUseForSingleVoiceBook = book.SingleVoice ? book.NarratorCharacterId : null;

				string prevCharacter = null;
				foreach (var block in book.GetScriptBlocks())
				{
					string character;
					if (narratorToUseForSingleVoiceBook != null)
						character = narratorToUseForSingleVoiceBook;
					else
					{
						if (block.CharacterIsUnclear)
							continue; // REVIEW: Should we throw an exception if this happens (in production code)?
						character = book.GetCharacterIdInScript(block);

						if (character == CharacterVerseData.kNeedsReview)
							continue; // The "Needs Review" character should never be added to a group.

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
					m_keyStrokesByCharacterId[character] += block.Length;

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
			return keyStrokes / kKeyStrokesPerHour;
		}

		public CharacterGroup GetGroupById(string id)
		{
			var grp = CharacterGroupList.GetGroupById(id);
			return grp ?? CharacterGroupList.GroupContainingCharacterId(id);
		}

		public IEnumerable<QuotationMark> GetQuotationMarksWithFullySpecifiedContinuers(IEnumerable<QuotationMark> quotationMarks)
		{
			if (quotationMarks != null)
			{
				QuotationMark oneLevelUp = null;
				foreach (var level in quotationMarks.Where(q => !IsNullOrWhiteSpace(q.Open))
					.OrderBy(q => q, QuoteSystem.QuotationMarkTypeAndLevelComparer))
				{
					if (level.Type == QuotationMarkingSystemType.Normal && level.Level > 1 && !IsNullOrWhiteSpace(level.Continue))
					{
						// I'm adding the final part of this check for sanity's sake. I've never seen data come in like this, but if
						// we ever were to process an LDML file that had already been "hacked" by Glyssen, we wouldn't want to add the
						// preceding level's continuer again.
						if (!IsNullOrWhiteSpace(oneLevelUp?.Continue) &&
							(level.Continue == oneLevelUp.Continue || !level.Continue.StartsWith(oneLevelUp.Continue)))
						{
							string newContinuer = oneLevelUp.Continue + " " + level.Continue;
							var replacementLevel = new QuotationMark(level.Open, level.Close, newContinuer, level.Level, level.Type);
							yield return replacementLevel;
							oneLevelUp = replacementLevel;
							continue;
						}
					}
					yield return level;
					oneLevelUp = level;
				}
			}
		}

		internal void SetWsQuotationMarksUsingFullySpecifiedContinuers(IEnumerable<QuotationMark> quotationMarks)
		{
			// The Paratext UI and check only support QuotationParagraphContinueType All or None (implicitly, if all levels are
			// set to *none*). But the LDML specification allows all 4 options. Depending on the origin of the LDML file, this
			// attribute may or may not be set. If not, the default is None, but if any levels have continuers specified, we
			// interpret this as All (since None doesn't make sense in this case).
			if (quotationMarks == null ||
				WritingSystem.QuotationParagraphContinueType == QuotationParagraphContinueType.Innermost ||
				WritingSystem.QuotationParagraphContinueType == QuotationParagraphContinueType.Outermost)
				return;

			var replacementQuotationMarks = GetQuotationMarksWithFullySpecifiedContinuers(quotationMarks).ToList();
			WritingSystem.QuotationMarks.Clear();
			WritingSystem.QuotationMarks.AddRange(replacementQuotationMarks);
		}

		public bool ProjectFileIsWritable
		{
			get { return m_projectFileIsWritable; }
			set { m_projectFileIsWritable = value; }
		}

		public int DefaultNarratorCountForNarrationByAuthor
		{
			get
			{
				// For narration by author
				var includedBookIds = IncludedBooks.Select(b => b.BookId).ToList();
				var authorsToCombine = BiblicalAuthors.All().Where(a => a.CombineAuthorAndNarrator &&
					!a.DoNotCombineByDefault && a.Books.Any(b => includedBookIds.Contains(b))).ToList();
				return authorsToCombine.Count + (IncludedBooks.All(b => authorsToCombine.SelectMany(a => a.Books).Contains(b.BookId)) ? 0 : 1);
			}
		}

		public int AuthorCount
		{
			get { return BiblicalAuthors.GetAuthorCount(IncludedBooks.Select(b => b.BookId)); }
		}

		public string LastExportLocation => Directory.Exists(Status.LastExportLocation) ? Status.LastExportLocation : Empty;

		public IReadOnlyList<BookScript> TestQuoteSystem(QuoteSystem altQuoteSystem)
		{
			var cvInfo = new CombinedCharacterVerseData(this);

			IEnumerable<UsxDocument> usxDocsForBooksToInclude;
			IStylesheet stylesheet;
			if (IsBundleBasedProject)
			{
				var bundle = new GlyssenBundle(OriginalBundlePath);
				usxDocsForBooksToInclude = bundle.UsxBooksToInclude;
				stylesheet = bundle.Stylesheet;
			}
			else
			{
				var scrTextWrapper = GetLiveParatextDataIfCompatible(false);
				if (scrTextWrapper == null)
				{
					Logger.WriteEvent("Paratext project is unavailable or is no longer compatible! Cannot test quote system.");
					return new BookScript[] { };
				}
				usxDocsForBooksToInclude = scrTextWrapper.UsxDocumentsForIncludedBooks;
				stylesheet = scrTextWrapper.Stylesheet;
			}
			var books = UsxParser.ParseBooks(usxDocsForBooksToInclude, stylesheet, null);

			var blocksInBook = books.ToDictionary(b => b.BookId, b => b.GetScriptBlocks());

			var parsedBlocksByBook = new ConcurrentDictionary<string, BookScript>();
			QuoteParser.SetQuoteSystem(altQuoteSystem);
			Parallel.ForEach(blocksInBook, bookidBlocksPair =>
			{
				var bookId = bookidBlocksPair.Key;
				var blocks = new QuoteParser(cvInfo, bookId, bookidBlocksPair.Value, Versification).Parse().ToList();
				var parsedBook = new BookScript(bookId, blocks, Versification);
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

		/// <summary>
		/// </summary>
		/// <param name="runningTests">The logic when this is false is superior.
		/// However, unfortunately, we have tests whose results depend on the inferior logic.
		/// When I combined the TestProject method with this one, I was too lazy to bother reworking
		/// those tests, so we have this optional parameter for now.</param>
		public void DoDemoDisambiguation(bool runningTests = false)
		{
			Logger.WriteEvent("Doing demo disambiguation");
			var cvData = new CombinedCharacterVerseData(this);

			foreach (var book in IncludedBooks)
			{
				var bookNum = BCVRef.BookToNumber(book.BookId);
				List<Block> blocksForMultiBlockQuote = null;
				CharacterSpeakingMode characterForMultiBlockQuote = null;
				int iCharacter = 0;
				List<CharacterSpeakingMode> charactersForVerse = null;
				foreach (var block in book.GetScriptBlocks())
				{
					if (block.StartsAtVerseStart)
					{
						iCharacter = 0;
						charactersForVerse = null;
					}
					if (block.MultiBlockQuote == MultiBlockQuote.Start)
						blocksForMultiBlockQuote = new List<Block>();
					else if (block.MultiBlockQuote == MultiBlockQuote.None)
					{
						ProcessMultiBlock(bookNum, ref blocksForMultiBlockQuote, ref characterForMultiBlockQuote);
					}
					if (block.CharacterId == CharacterVerseData.kUnexpectedCharacter)
					{
						block.SetNonDramaticCharacterId(book.NarratorCharacterId);
						block.UserConfirmed = true;
					}
					else if (block.CharacterId == CharacterVerseData.kAmbiguousCharacter)
					{
						blocksForMultiBlockQuote?.Add(block);

						if (runningTests || charactersForVerse == null)
						{
							charactersForVerse = cvData.GetCharacters(bookNum, block.ChapterNumber,
								(Block.InitialVerseNumberBridgeFromBlock)block, Versification).ToList();
						}

						if (!charactersForVerse.Any())
							continue;

						var cvEntry = runningTests ? charactersForVerse[0] : charactersForVerse[iCharacter++];
						if (iCharacter == charactersForVerse.Count)
							iCharacter = 0;
						if (blocksForMultiBlockQuote != null)
						{
							characterForMultiBlockQuote = cvEntry;
							continue;
						}
						block.SetCharacterIdAndCharacterIdInScript(cvEntry.Character, bookNum, Versification);
						block.Delivery = cvEntry.Delivery;
						block.UserConfirmed = true;
					}
				}
				ProcessMultiBlock(bookNum, ref blocksForMultiBlockQuote, ref characterForMultiBlockQuote);

#if DEBUG
				if (book.GetScriptBlocks().Any(block => block.CharacterIsUnclear))
					Debug.Fail("Failed to disambiguate");
#endif
			}
		}

		private void ProcessMultiBlock(int bookNum, ref List<Block> blocksForMultiBlockQuote, ref CharacterSpeakingMode characterForMultiBlockQuote)
		{
			if (blocksForMultiBlockQuote != null)
			{
				foreach (var blockForMultiBlockQuote in blocksForMultiBlockQuote)
				{
					blockForMultiBlockQuote.SetCharacterIdAndCharacterIdInScript(characterForMultiBlockQuote.Character, bookNum,
						Versification);
					blockForMultiBlockQuote.Delivery = characterForMultiBlockQuote.Delivery;
					blockForMultiBlockQuote.UserConfirmed = true;
				}
				blocksForMultiBlockQuote = null;
				characterForMultiBlockQuote = null;
			}
		}

		public void DoTestDisambiguation()
		{
			DoDemoDisambiguation(true);
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
		ReadyForUserInteraction = NeedsQuoteSystemConfirmation | FullyInitialized,
		ReadyForDataMigration = QuoteParseComplete | FullyInitialized
	}
}

