using Glyssen.Shared;
using GlyssenEngine.Analysis;
using GlyssenEngine.Bundle;
using GlyssenEngine.Casting;
using GlyssenEngine.ErrorHandling;
using GlyssenEngine.Paratext;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;
using JetBrains.Annotations;
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GlyssenCharacters;
using GlyssenEngine.Character;
using SIL.Xml;
using static System.String;
using CharacterGroup = GlyssenEngine.Character.CharacterGroup;
using CharacterGroupList = GlyssenEngine.Character.CharacterGroupList;

namespace GlyssenEngine
{
	public class Project : ProjectBase, IUserProject
	{
		public const int kParserVersion = 54;

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
		private ReferenceText m_referenceText;

		private Dictionary<string, int> m_speechDistributionScore;
		private Dictionary<string, int> m_keyStrokesByCharacterId;

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public event EventHandler<ProjectStateChangedEventArgs> ProjectStateChanged;
		public event EventHandler QuoteParseCompleted;
		public event EventHandler AnalysisCompleted;
		public event EventHandler CharacterGroupCollectionChanged;
		public event EventHandler CharacterStatisticsCleared;
		public static EventHandler UpgradingProjectToNewParserVersion;

		public delegate BadLdmlRecoveryAction BadLdmlHandler(Project sender, string error, bool attemptToUseBackup);
		public static BadLdmlHandler GetBadLdmlRecoveryAction;

		public static Func<string, IEnumerable<string>> GetDynamicLocalizedCharacterIdVariants;

		public static IFontRepository FontRepository { get; set; }
		public static IProjectPersistenceWriter Writer { get; set; }
		public static string EnglishVersification => Resources.EnglishVersification;

		public Func<bool> IsOkayToClearExistingRefBlocksWhenChangingReferenceText { get; set; }

		/// <exception cref="ProjectNotFoundException">Paratext was unable to access the project (only pertains to
		/// Glyssen projects that are associated with a live Paratext project)</exception>
		public Project(GlyssenDblTextMetadata metadata, string recordingProjectName = null, bool installFonts = false,
			WritingSystemDefinition ws = null, bool loadVersification = true)
			: base(metadata, recordingProjectName ?? GetDefaultRecordingProjectName(metadata.Identification.Name, metadata.Language.Iso))
		{
			m_projectMetadata = metadata;
			SetBlockGetChapterAnnouncement(ChapterAnnouncementStyle);
			m_wsDefinition = ws;

			using (var reader = Reader.Load(this, ProjectResource.CharacterDetailData))
				ProjectCharacterDetail = ProjectCharacterDetailData.Load(reader);

			if (loadVersification)
			{
				var versification = LoadVersification(GlyssenVersificationTable.InvalidVersificationLineExceptionHandling.ReportIndividually);
				if (versification != null)
					SetVersification(versification);
				else if (IsLiveParatextProject)
				{
					try
					{
						SetVersification(GetSourceParatextProject().Settings.Versification);
					}
					catch (ProjectNotFoundException e)
					{
						Logger.WriteError("Paratext project not found. Falling back to get usable versification.", e);
						SetVersification(GetFallbackVersificationOrEnglish());
					}
				}
			}

			if (installFonts)
				InstallFontsIfNecessary();
		}

		private ScrVers GetFallbackVersificationOrEnglish()
		{
			string msgPart1;
			var fallbackVersification = LoadVersification(GlyssenVersificationTable.InvalidVersificationLineExceptionHandling.BatchErrors, true);
			if (fallbackVersification == null)
			{
				msgPart1 = Format(Localizer.GetString("Project.ParatextProjectNoFallbackVersification",
					"{0} project {1} is not available and project {2} does not have a fallback versification file. ",
					"Param 0: \"Paratext\" (product name); " +
					"Param 1: Paratext project short name (unique project identifier); " +
					"Param 2: Glyssen recording project name"),
					ParatextScrTextWrapper.kParatextProgramName,
					ParatextProjectName,
					Name);
				if (!char.IsWhiteSpace(msgPart1.Last()))
					msgPart1 += " ";
			}
			else
			{
				var tableImpl = (GlyssenVersificationTable)SIL.Scripture.Versification.Table.Implementation;
				var errors = tableImpl.GetBatchedErrorString(ProjectResource.FallbackVersification.ToString());
				if (errors == null)
				{
					return fallbackVersification;
				}

				msgPart1 = Format(Localizer.GetString("Project.ParatextProjectFallbackVersificationInvalid",
					"{0} project {1} is not available and the fallback versification in project {2} is invalid:\r\n {3}\r\n",
					"Param 0: \"Paratext\" (product name); " +
					"Param 1: Paratext project short name (unique project identifier); " +
					"Param 2: Glyssen recording project name; " +
					"Param 3: Error details"),
					ParatextScrTextWrapper.kParatextProgramName,
					ParatextProjectName,
					Name,
					errors);

			}

			MessageModal.Show(msgPart1 + Format(Localizer.GetString("Project.FallingBackToDefaultEnglishVersification",
				"Therefore, the {0} versification is being used by default. If this is not the correct versification " +
				"for this project, some things will not work as expected.",
				"Param: “English” (versification name)"),
				ScrVers.English.Name));

			return ScrVers.English;
		}

		public Project(GlyssenBundle bundle, string recordingProjectName = null, Project projectBeingUpdated = null) :
			this(bundle.Metadata, recordingProjectName, false, bundle.WritingSystemDefinition ?? projectBeingUpdated?.WritingSystem)
		{
			Writer.SetUpProjectPersistence(this, bundle);
			InstallFontsIfNecessary();

			try
			{
				SetVersification(LoadVersification(GlyssenVersificationTable.InvalidVersificationLineExceptionHandling.Throw));
			}
			catch (InvalidVersificationLineException ex)
			{
				Logger.WriteError(ex);
				Writer.DeleteProject(this);
				throw;
			}

			if (bundle.WritingSystemDefinition?.QuotationMarks != null && bundle.WritingSystemDefinition.QuotationMarks.Any())
			{
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				SetWsQuotationMarksUsingFullySpecifiedContinuers(bundle.WritingSystemDefinition.QuotationMarks);
			}

			UserDecisionsProject = projectBeingUpdated;
			PopulateAndParseBooks(bundle);
		}

		public Project(ParatextScrTextWrapper paratextProject) :
			this(paratextProject.GlyssenDblTextMetadata, null, false, paratextProject.WritingSystem)
		{
			Writer.SetUpProjectPersistence(this);
			if (paratextProject.HasQuotationRulesSet)
			{
				QuoteSystemStatus = QuoteSystemStatus.Obtained;
				SetWsQuotationMarksUsingFullySpecifiedContinuers(paratextProject.QuotationMarks);
				if (paratextProject.ReportingClauseStartDelimiter != null)
				{
					QuoteSystem.SetReportingClauseDelimiters(paratextProject.ReportingClauseStartDelimiter,
						paratextProject.ReportingClauseEndDelimiter);
				}
			}

			ParseAndSetBooks(paratextProject.UsxDocumentsForIncludedBooks, paratextProject.Stylesheet);
		}

		/// <summary>
		/// Used only for sample project and in tests.
		/// </summary>
		internal Project(GlyssenDblTextMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet,
			WritingSystemDefinition ws, string versificationInfo = null) : this(metadata, ws: ws)
		{
			Writer.SetUpProjectPersistence(this);
			using (var writer = Writer.GetTextWriter(this, ProjectResource.Versification))
				writer.Write(versificationInfo ?? EnglishVersification);
			SetVersification(LoadVersification(GlyssenVersificationTable.InvalidVersificationLineExceptionHandling.Throw));

			ParseAndSetBooks(books, stylesheet);
		}

		public string AudioStockNumber => m_projectMetadata.AudioStockNumber;

		public string Id => m_metadata.Id;

		public override string Name => m_recordingProjectName;

		public override string ToString() => $"{LanguageIsoCode}|{Id}|{Name}";

		public string PublicationName => m_metadata.Identification?.Name;

		public static string DefaultRecordingProjectNameSuffix { get; set; } = " Audio"; // This default can be overriden (and localized)

		public bool IsLiveParatextProject => m_projectMetadata.Type == ParatextScrTextWrapper.kLiveParatextProjectType;

		public bool IsBundleBasedProject => !IsNullOrEmpty(OriginalBundlePath);

		public string ParatextProjectName => m_projectMetadata.ParatextProjectId;
		public int ParserVersionWhenLastParsed => m_projectMetadata.ParserVersion;

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
			// As of Paratext 9.1: To get a project, we to try to "Find" it using the GUID (because
			// in P9.1 it is possible to have multiple projects with the same name). If we have not
			// saved a GUID for the project in the metadata (this is probably never the case), then
			// as a fallback we try to "Get" it by name, though I kind of doubt this would ever
			// succeed. If that fails, Paratext throws a ProjectNotFoundException, and the caller
			// needs to deal with it.
			// Note: If there is more than one local project with the same name, attempting to load
			// by name will not work - they can only be loaded by ID.
			var id = m_projectMetadata.ParatextProjectUniqueId;
			if (!IsNullOrEmpty(id))
			{
				// The following falls back to looking for the project by name if
				// the id is null or looks to be an invalid ID.
				var scrText = ScrTextCollection.FindById(HexId.FromStrSafe(id), ParatextProjectName);
				if (scrText != null)
					return scrText;
			}
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

		public QuoteSystem QuoteSystem => m_quoteSystem != null ? m_quoteSystem : (m_quoteSystem = QuoteSystem.TryCreateFromWritingSystem(WritingSystem));

		/// <summary>
		/// Setting the quote system has a number of potential side-effects. Most notably, if the project is awaiting quote system
		/// confirmation and the quote system status is ready for parsing, this kicks off the quote parse. In the event of a later
		/// change, this can also trigger an automatic backup of the project (because the process of re-applying user decisions when
		/// the parse changes dramatically can be kind of messy).
		/// </summary>
		public void SetQuoteSystem(QuoteSystemStatus status, QuoteSystem system)
		{
			Debug.Assert(status != QuoteSystemStatus.Unknown);

			if (ProjectState == ProjectState.Parsing)
				throw new InvalidOperationException("The quote system cannot be changed while quote parsing is in progress!");

			// Changing the status sets the date, but if the user had previously set thus and is now changing the quote
			// system, we want to force the date to update.
			if (QuoteSystemStatus == QuoteSystemStatus.UserSet && status == QuoteSystemStatus.UserSet && QuoteSystem != system)
				Status.QuoteSystemDate = DateTime.Now;
			QuoteSystemStatus = status;
			bool quoteSystemBeingSetForFirstTime = QuoteSystem == null;
			bool quoteSystemChanged = m_quoteSystem != system;

			if (IsQuoteSystemReadyForParse && ProjectState == ProjectState.NeedsQuoteSystemConfirmation)
			{
				m_quoteSystem = system;
				DoQuoteParse();
			}
			else if ((quoteSystemChanged && !quoteSystemBeingSetForFirstTime) ||
				(QuoteSystemStatus == QuoteSystemStatus.Reviewed &&
					ProjectState == (ProjectState.NeedsQuoteSystemConfirmation | ProjectState.WritingSystemRecoveryInProcess)))
			{
				// These need to happen in this order
				Save();
				CreateBackup("Backup before quote system change");
				m_quoteSystem = system;
				HandleQuoteSystemChanged();
			}
			else
			{
				m_quoteSystem = system;
			}

			WritingSystem.QuotationMarks.Clear();
			WritingSystem.QuotationMarks.AddRange(m_quoteSystem.AllLevels);
		}

		public QuoteSystemStatus QuoteSystemStatus
		{
			get => Status.QuoteSystemStatus;
			private set => Status.QuoteSystemStatus = value;
		}

		public bool IsQuoteSystemReadyForParse => (QuoteSystemStatus & QuoteSystemStatus.ParseReady) != 0;

		public DateTime QuoteSystemDate => Status.QuoteSystemDate;

		public BookSelectionStatus BookSelectionStatus
		{
			get
			{
				// Don't make the user open the select books dialog if there is only 1 book.
				if (Status.BookSelectionStatus == BookSelectionStatus.UnReviewed && AvailableBooks.Count == 1 && IncludedBooks.Count == 1)
					Status.BookSelectionStatus = BookSelectionStatus.Reviewed;

				return Status.BookSelectionStatus;
			}
			set => Status.BookSelectionStatus = value;
		}

		public ProjectSettingsStatus ProjectSettingsStatus
		{
			get => Status.ProjectSettingsStatus;
			set => Status.ProjectSettingsStatus = value;
		}

		public CharacterGroupGenerationPreferences CharacterGroupGenerationPreferences
		{
			get => m_projectMetadata.CharacterGroupGenerationPreferences;
			set => m_projectMetadata.CharacterGroupGenerationPreferences = value;
		}

		// TODO: Implement this feature. Currently, this setting is not exposed in the UI and it is
		// only used for estimating cast size.
		public ProjectDramatizationPreferences DramatizationPreferences
		{
			get => m_projectMetadata.DramatizationPreferences;
			set => m_projectMetadata.DramatizationPreferences = value;
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

		public void InstallFontsIfNecessary()
		{
			Debug.Assert(FontRepository != null, "Font repository implementation should be set before attempting to load a project.");

			string fontFamily = m_projectMetadata.FontFamily;
			if (m_fontInstallationAttempted || IsNullOrEmpty(fontFamily) || FontRepository.IsFontInstalled(fontFamily))
				return;

			m_fontInstallationAttempted = true;

			if (!Reader.TryInstallFonts(this, FontRepository))
				FontRepository.ReportMissingFontFamily(fontFamily);
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

		public IReadOnlyCollection<string> ReportingClauses => m_metadata.Language.ReportingClauses;

		public bool AddNewReportingClauses(IEnumerable<string> reportingClauses)
		{
			var retVal = false;
			if (m_metadata.Language.ReportingClauses == null)
				m_metadata.Language.ReportingClauses = new HashSet<string>();
			foreach (var reportingClause in reportingClauses)
				retVal |= m_metadata.Language.ReportingClauses.Add(reportingClause);
			return retVal;
		}

		public void UpdateSettings(ProjectSettingsViewModel model, string defaultFontFamily, int defaultFontSizeInPoints, bool rightToLeftScript)
		{
			Debug.Assert(!IsNullOrEmpty(model.RecordingProjectName));

			if (m_recordingProjectName != model.RecordingProjectName)
			{
				Writer.ChangeProjectName(this, model.RecordingProjectName);
				m_recordingProjectName = model.RecordingProjectName;
			}
			
			m_projectMetadata.AudioStockNumber = model.AudioStockNumber;
			m_projectMetadata.FontFamily = defaultFontFamily;
			m_projectMetadata.FontSizeInPoints = defaultFontSizeInPoints;
			m_metadata.Language.ScriptDirection = rightToLeftScript ? "RTL" : "LTR";
			ChapterAnnouncementStyle = model.ChapterAnnouncementStyle;
			m_projectMetadata.IncludeChapterAnnouncementForFirstChapter = !model.SkipChapterAnnouncementForFirstChapter;
			m_projectMetadata.IncludeChapterAnnouncementForSingleChapterBooks = !model.SkipChapterAnnouncementForSingleChapterBooks;
		}

		public Project UpdateProjectFromBundleData(GlyssenBundle bundle)
		{
			if ((ProjectState & ProjectState.ReadyForUserInteraction) == 0)
				throw new InvalidOperationException($"Project not in a valid state to update from text release bundle. ProjectState = {ProjectState}");

			// If we're updating the project in place, we need to make a backup. Otherwise, if it's moving to a new
			// location, just mark the existing one as inactive.
			bool moving = LanguageIsoCode != bundle.LanguageIso || MetadataId != bundle.Id;
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

		public ProjectState ProjectState
		{
			get => m_projectState;
			private set
			{
				if (m_projectState == value)
					return;
				m_projectState = value;
				ProjectStateChanged?.Invoke(this, new ProjectStateChangedEventArgs {ProjectState = m_projectState});
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
						if (Enum.TryParse(ReferenceTextProxy.CustomIdentifier, out ReferenceTextType type))
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
			get => ReferenceTextProxy.GetOrCreate(m_projectMetadata.ReferenceTextType, m_projectMetadata.ProprietaryReferenceTextIdentifier);
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
									candidate => m_referenceText.IsOkayToSplitBeforeBlock(book, candidate, refTextVerseSplitLocations),
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

		public void ClearAssignCharacterStatus()
		{
			Status.AssignCharacterMode = BlocksToDisplay.NotAlignedToReferenceText;
			Status.AssignCharacterBlock = new BookBlockIndices();
		}

		public static Project Load(Project existingProject, Func<Project, bool> handleMissingBundleNeededForUpgrade, IParatextProjectLoadingAssistant loadingAssistant)
		{
			if (existingProject == null)
				return null;
			
			InitializeExistingProject(existingProject);

			if (!existingProject.IsSampleProject && existingProject.NeedsQuoteReparse)
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
					AttemptToUpgradeByReparsingBundleData(existingProject, handleMissingBundleNeededForUpgrade) :
					AttemptToUpgradeByReparsingParatextData(existingProject, loadingAssistant);

				if (upgradedProject != null)
					return upgradedProject;
			}

			existingProject.InitializeLoadedProject();
			return existingProject;
		}

		/// <summary>
		/// Returns true if the project needs to be reparsed, typically because it was parsed with an older version of the quote
		/// parser, but also for other more obscure reasons. (Can have non-readonly side-effects - see remarks.)
		/// </summary>
		/// <remarks>Note that in some circumstances, if we know that the new parser version would not result in improved parsing,
		/// this property has the side-effect of just updating the project's parser version to the current version number and
		/// returning false.
		/// </remarks>
		private bool NeedsQuoteReparse
		{
			get
			{
				// PG-1315: If the Versification was changed in Paratext, we should redo the quote parsing because guesses about
				// appropriate character assignments are heavily tied to verse references. However, versification mappings in the
				// New Testament are rare and very limited even when they do occur. So if we're including only NT books, we can
				// skip this.
				if (IsLiveParatextProject && m_projectMetadata.Versification != Versification.Name && IncludedBooks.Any() && IncludedBooks.First().BookNumber < 40)
					return true;

				if (m_projectMetadata.ParserVersion >= kParserVersion)
					return false;
				// Examples of past versions that did not necessarily require a re-parse. Good idea to comment these out to keep track
				// of the history, but once the setting (which is now a const) is incremented, there is no need to continue to execute
				// the code to check for these versions.
				//if (kParserVersion == 43)
				//{
				//	if (m_projectMetadata.ParserVersion < 42 || QuoteSystem.QuotationDashMarker?.FirstOrDefault() == ':')
				//		return true;
				//	m_projectMetadata.ParserVersion = kParserVersion;
				//	return false;
				// }
				//if (kParserVersion == 46)
				//{
				//	// This change only affects the way the \qa marker is handled, and that marker is most likely only used in Psalm 119
				//	if (m_projectMetadata.ParserVersion < 45 || AvailableBooks.Any(b => b.Code == "PSA"))
				//		return true;
				//	m_projectMetadata.ParserVersion = kParserVersion;
				//	return false;
				// }
				return true;
			}
	}

		private static Project AttemptToUpgradeByReparsingBundleData(Project existingProject, [CanBeNull] Func<Project, bool> handleMissingBundleNeededForUpgrade)
		{
			if (!RobustFile.Exists(existingProject.OriginalBundlePath))
			{
				bool upgradeProject = false;
				if (kParserVersion > existingProject.m_projectMetadata.ParserUpgradeOptOutVersion && handleMissingBundleNeededForUpgrade != null)
				{
					upgradeProject = handleMissingBundleNeededForUpgrade(existingProject);
					if (!upgradeProject)
						existingProject.m_projectMetadata.ParserUpgradeOptOutVersion = kParserVersion;
				}

				if (!upgradeProject)
					return null;
			}

			using (var bundle = new GlyssenBundle(existingProject.OriginalBundlePath))
			{
				var upgradedProject = new Project(existingProject.m_projectMetadata, existingProject.m_recordingProjectName,
					ws: existingProject.WritingSystem);

				UpgradingProjectToNewParserVersion(existingProject, new EventArgs());

				return UpgradeProject(existingProject, upgradedProject, () => upgradedProject.PopulateAndParseBooks(bundle));
			}
		}

		private static Project AttemptToUpgradeByReparsingParatextData(Project existingProject, IParatextProjectLoadingAssistant loadingAssistant)
		{
			if (loadingAssistant != null)
			{
				loadingAssistant.Project = existingProject;
				loadingAssistant.ParatextProjectName = existingProject.ParatextProjectName;
				loadingAssistant.SilentMode = kParserVersion <= existingProject.m_projectMetadata.ParserUpgradeOptOutVersion;
			}

			var scrTextWrapper = existingProject.GetLiveParatextDataIfCompatible(loadingAssistant);
			if (scrTextWrapper == null)
			{
				existingProject.m_projectMetadata.ParserUpgradeOptOutVersion = kParserVersion;
				return null;
			}

			UpgradingProjectToNewParserVersion?.Invoke(existingProject, new EventArgs());

			return existingProject.UpdateProjectFromParatextData(scrTextWrapper);
		}

		public ParatextScrTextWrapper GetParatextScrTextWrapper()
		{
			return GetLiveParatextDataIfCompatible(null, false);
		}

		public ParatextScrTextWrapper GetLiveParatextDataIfCompatible(IParatextProjectLoadingAssistant loadingAssistant, bool checkForChangesInAvailableBooks = true)
		{
			if (loadingAssistant != null)
			{
				loadingAssistant.Project = this;
				loadingAssistant.ParatextProjectName = ParatextProjectName;
			}

			ParatextScrTextWrapper scrTextWrapper;
			ScrText sourceScrText = null;
			do
			{
				try
				{
					sourceScrText = GetSourceParatextProject();
				}
				catch (ProjectNotFoundException)
				{
					if (loadingAssistant != null && loadingAssistant.RetryWhenProjectNotFound())
						continue;

					return null;
				}
			} while (sourceScrText == null); // retry

			if (loadingAssistant != null && loadingAssistant.ForceReload)
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
						if (loadingAssistant.RetryWhenReloadFails(e.Message))
							continue;
						return null;
					}
				} while (!reloadSucceeded);
			}

			try
			{
				scrTextWrapper = new ParatextScrTextWrapper(sourceScrText, QuoteSystemStatus != QuoteSystemStatus.Obtained);
				bool compatible = scrTextWrapper.GlyssenDblTextMetadata.Language.Iso == Metadata.Language.Iso;
				if (compatible)
				{
					compatible = scrTextWrapper.GlyssenDblTextMetadata.Id == Metadata.Id;
					if (!compatible && loadingAssistant != null &&
						!Reader.ProjectExistsHaving(LanguageIsoCode, scrTextWrapper.GlyssenDblTextMetadata.Id, Name))
					{
						if (loadingAssistant.ConfirmUpdateGlyssenProjectMetadataIdToMatchParatextProject())
						{
							ChangePublicationId(scrTextWrapper.GlyssenDblTextMetadata.Id);
							loadingAssistant.HandleProjectPathChanged();
							compatible = true;
						}
						else
							return null;
					}
				}
				if (!compatible)
				{
					throw new ApplicationException(Format(Localizer.GetString("Project.ParatextProjectMetadataChangedMsg",
						"The settings of the {0} project {1} no longer appear to correspond to the {2} project. " +
						"This is an unusual situation. If you do not understand how this happened, please contact support.",
						"Param 0: \"Paratext\" (product name); " +
						"Param 1: Paratext project short name (unique project identifier); " +
						"Param 2: Product name (e.g., \"Glyssen\")"),
						ParatextScrTextWrapper.kParatextProgramName,
						ParatextProjectName,
						GlyssenInfo.Product));
				}
			}
			catch (ApplicationException e)
			{
				loadingAssistant?.ReportApplicationError(e);
				return null;
			}

			return !checkForChangesInAvailableBooks ||
				!FoundUnacceptableChangesInAvailableBooks(scrTextWrapper, loadingAssistant) ?
				scrTextWrapper : null;
		}

		private bool FoundUnacceptableChangesInAvailableBooks(ParatextScrTextWrapper scrTextWrapper, IParatextProjectLoadingAssistant loadingAssistant)
		{
			// Any of the following scenarios is possible:
			// 1) Exactly the same books are available and passing checks (or were previously added by overriding the
			//    check status). This is the happy path!
			// 2) Some of the "Available", but not "Included" books in the existing project are no longer available.
			//    => We can just remove them from the list of available books and delete the corresponding data (if any)
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
			if (loadingAssistant != null && !loadingAssistant.SilentMode)
			{
				var noLongerAvailableBookIds = new List<string>();
				var noLongerPassingListBookIds = new List<string>();

				HandleDifferencesInAvailableBooks(scrTextWrapper, null,
					bookCode => noLongerAvailableBookIds.Add(bookCode),
					bookCode => noLongerPassingListBookIds.Add(bookCode));

				if (!noLongerAvailableBookIds.Any() && !noLongerPassingListBookIds.Any())
					return false;

				return !loadingAssistant.ConfirmUpdateThatWouldExcludeExistingBooks(noLongerAvailableBookIds, noLongerPassingListBookIds);
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
			// ENHANCE: The previous version of this method attempted a more efficient
			// implementation, taking advantage of both lists of books being in canonical order.
			// However, the logic was flawed. I think this new approach is easier to understand,
			// but creation of a dictionary and the final loop (which will rarely be executed)
			// is probably less efficient. Anyway, this is code that is not likely to be a
			// performance bottleneck, but I added this comment to explain the reason for the
			// change and so that no one would judge me later for writing less than optimally
			// efficient code.
			var existingAvailable = m_projectMetadata.AvailableBooks
				.ToDictionary(b => b.Code, b => b.IncludeInScript);
			var nowAvailable = scrTextWrapper.AvailableBooks;
			foreach (Book nowAvailableBook in nowAvailable)
			{
				var bookCode = nowAvailableBook.Code;
				var bookNum = Canon.BookIdToNumber(bookCode);

				if (existingAvailable.TryGetValue(nowAvailableBook.Code, out var includeExistingInScript))
				{
					var book = GetBook(bookCode);
					if (book != null)
					{
						if (includeExistingInScript &&
							!book.CheckStatusOverridden &&
							!scrTextWrapper.DoesBookPassChecks(bookNum))
						{
							noLongerPassChecksPreviouslyIncludedWithoutCheckStatusOverride?.Invoke(bookCode);
						}
						else
							foundInBoth?.Invoke(bookCode);
					}

					existingAvailable.Remove(bookCode);
				}
				else if (newlyAvailableChecksPass != null || newlyAvailableChecksFail != null)
				{
					// New available book.
					if (scrTextWrapper.DoesBookPassChecks(bookNum))
						newlyAvailableChecksPass?.Invoke(bookCode);
					else
						newlyAvailableChecksFail?.Invoke(bookCode);
				}
			}

			while (existingAvailable.Any())
			{
				foreach (var bookCode in StandardCanon.AllBookCodes)
				{
					if (existingAvailable.TryGetValue(bookCode, out var includeExistingInScript))
					{
						if (includeExistingInScript)
							nowMissingPreviouslyIncluded?.Invoke(bookCode);
						else
							nowMissingPreviouslyExcluded.Invoke(bookCode);
						existingAvailable.Remove(bookCode);
					}
				}
			}
		}

		public Project UpdateProjectFromParatextData(IParatextScrTextWrapper scrTextWrapper)
		{
			var existingAvailable = m_projectMetadata.AvailableBooks;
			var upgradedProject = new Project(m_projectMetadata, Name);
			// Initially we want to include anything that is included in Paratext so we don't lose user decisions.
			scrTextWrapper.IncludeBooks(upgradedProject.AvailableBooks.Select(b => b.Code));

			// Add metadata for any books that are available in scrTextWrapper but not in the
			// existing project. Remove metadata for any books formerly available that are not now.
			bool foundDataChange = false;

			void NowMissing(string bookCode)
			{
				Writer.ArchiveBookThatIsNoLongerAvailable(this, bookCode);
				foundDataChange = true;
			}

			var booksToExcludeFromProject = new List<string>();
			void Exclude(string bookCode)
			{
				var books = upgradedProject.m_projectMetadata.AvailableBooks;
				// Although this book is being excluded (because it does not pass checks)
				// we do need to check to see if it is a newly available book for this
				// project, so we can include it in the list of available books in the
				// dialog in case the user wants to override and include it anyway.
				if (!books.Any(b => b.Code == bookCode))
				{
					foundDataChange = true;
					int insertAt = 0;
					var newBookNum = BCVRef.BookToNumber(bookCode);
					while (insertAt < books.Count)
					{
						var existingAvailBookNum = BCVRef.BookToNumber(books[insertAt].Code);
						if (existingAvailBookNum > newBookNum)
							break;
						insertAt++;
					}
					books.Insert(insertAt, scrTextWrapper.AvailableBooks.Single(b => b.Code == bookCode));
				}
				booksToExcludeFromProject.Add(bookCode);
			}

			// For any newly available book that passes checks, if all existing books are included,
			// we assume we want to include anything new as well.
			var excludeNewBooks = existingAvailable.Any(b => b.IncludeInScript == false);

			void HandleNewPassingBook(string bookCode)
			{
				if (excludeNewBooks)
					booksToExcludeFromProject.Add(bookCode);
				foundDataChange = true;
			}

			if (upgradedProject.QuoteSystemStatus == QuoteSystemStatus.Obtained && scrTextWrapper.HasQuotationRulesSet)
			{
				upgradedProject.SetWsQuotationMarksUsingFullySpecifiedContinuers(scrTextWrapper.QuotationMarks);
				foundDataChange |= upgradedProject.QuoteSystem != QuoteSystem;
			}
			else
				CopyQuoteMarksIfAppropriate(upgradedProject.WritingSystem, upgradedProject.m_projectMetadata);

			// ENHANCE: Rather than unconditionally excluding new books that do not pass checks, we might
			// want to ask the user.
			HandleDifferencesInAvailableBooks(scrTextWrapper, NowMissing, NowMissing,
				Exclude, HandleNewPassingBook, Exclude);

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

			void ParseAndSetBooksForUpgradedProject() => upgradedProject.ParseAndSetBooks(scrTextWrapper.UsxDocumentsForIncludedBooks, scrTextWrapper.Stylesheet);

			UpgradeProject(this, upgradedProject, ParseAndSetBooksForUpgradedProject);

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

		public static void SetHiddenFlag(GlyssenDblTextMetadata metadata, string projectName, bool hidden)
		{
			metadata.Inactive = hidden;
			new Project(metadata, projectName).SaveProjectMetadata(out var error);
			if (error != null)
				throw error;
			// TODO: preserve WritingSystemRecoveryInProcess flag
		}

		private int PercentInitialized => (int)(m_usxPercentComplete * kUsxPercent + m_guessPercentComplete * kGuessPercent + m_quotePercentComplete * kQuotePercent);

		private static void InitializeExistingProject(Project project)
		{
			project.LoadExistingBooks();

			project.RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();

			// For legacy projects
			if (project.CharacterGroupList.CharacterGroups.Any() &&
				project.CharacterGroupGenerationPreferences.CastSizeOption == CastSizeOption.NotSet)
				project.CharacterGroupGenerationPreferences.CastSizeOption = CastSizeOption.MatchVoiceActorList;
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

		/// <summary>
		/// Adds an available book to the metadata for any book in the m_books list that does
		/// not already correspond to an existing available book. The newly added book will be
		/// marked for inclusion in the script.
		/// </summary>
		/// <remarks>Probably only relevant to Paratext-based projects. If this is called for
		/// a bundle-based project and m_books contains a book that is not in the list
		/// of available books in the metadata, this will throw an exception.</remarks>
		private void AddMissingAvailableBooks()
		{
			BookNames nameInfo = null;

			int Compare(Book book, BookScript bookScript) => BCVRef.BookToNumber(book.Code).CompareTo(bookScript.BookNumber);
			Book GetBook(BookScript bookScript)
			{
				if (!IsLiveParatextProject)
					throw new InvalidOperationException($"Book {bookScript.BookId} parsed and added to project but not in AvailableBooks in metadata!");
				if (nameInfo == null)
					nameInfo = GetSourceParatextProject().BookNames;
				return ParatextScrTextWrapper.GetBook(nameInfo, bookScript.BookNumber, true);
			}

			// Merge m_books into m_projectMetadata.AvailableBooks
			m_projectMetadata.AvailableBooks.MergeOrderedList(m_books, Compare, GetBook);
		}

		protected override void SetVersification(ScrVers versification)
		{
			base.SetVersification(versification);

			using (var reader = Reader.Load(this, ProjectResource.CharacterVerseData))
				ProjectCharacterVerseData = new ProjectCharacterVerseData(reader, Versification);
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
				ProjectState = ProjectState.NeedsQuoteSystemConfirmation | (ProjectState & ProjectState.WritingSystemRecoveryInProcess);
				return;
			}

			m_quotePercentComplete = 100;
			if (m_projectMetadata.ControlFileVersion != ControlCharacterVerseData.Singleton.ControlFileVersion)
			{
				const int kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan = 78;
				new CharacterAssigner(new CombinedCharacterVerseData(this), QuoteSystem).AssignAll(m_books,
					m_projectMetadata.ControlFileVersion < kControlFileVersionWhenOnTheFlyAssignmentOfCharacterIdInScriptBegan);

				UpdateControlFileVersion();
			}

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
					targetBookScript.ApplyUserDecisions(sourceBookScript, ReferenceText, ReportingClauses);
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
		/// <param name="book">The <see cref="BookScript"/>, either from a prior call to LoadExistingBookIfPossible or as newly created by
		/// the USX parser</param>
		/// <exception cref="InvalidOperationException">The project is not in a valid state for the book to be included.</exception>
		public void IncludeExistingBook(BookScript book)
		{
			Debug.Assert(IsLiveParatextProject, "Heads up! This might not be a problem, but we really only anticipated this method being used for Paratext-based projects.");
			var bookMetadata = AvailableBooks.SingleOrDefault(b => b.Code == book.BookId);
			if (bookMetadata == null)
				throw new InvalidOperationException($"Attempt to include the {nameof(BookScript)} for {book.BookId}, but the project contains no metadata for the book.");
			if (!bookMetadata.IncludeInScript)
				throw new InvalidOperationException($"Attempt to include the {nameof(BookScript)} for {book.BookId}, but the metadata for the book indicates that it should not be included.");

			if (!book.GetScriptBlocks().Any())
				return;

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

		public void IncludeBooksFromParatext(ParatextScrTextWrapper wrapper, ISet<int> bookNumbers, Action<BookScript> postParseAction)
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
			var parameters = (object[])e.Argument;
			var books = (IEnumerable<UsxDocument>)parameters[0];
			var stylesheet = (IStylesheet)parameters[1];
			var postParseAction = parameters.Length > 2 ? (Action<BookScript>)parameters[2] : null;

			var backgroundWorker = (BackgroundWorker)sender;

			var parsedBooks = UsxParser.ParseBooks(books, stylesheet,
				new CharacterUsageStore(Versification, new CombinedCharacterVerseData(this),
					GetDynamicLocalizedCharacterIdVariants),
				i => backgroundWorker.ReportProgress(i));

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

			var bookScripts = (List<BookScript>)e.Result;

			foreach (var bookScript in bookScripts)
			{
				// This code is an attempt to figure out how we are getting null reference exceptions when using the objects in the list (See PG-275 & PG-287)
				if (bookScript?.BookId == null)
				{
					var nonNullBookScripts = bookScripts.Where(b => b != null).Select(b => b.BookId);
					var nonNullBookScriptsStr = Join(";", nonNullBookScripts);
					var initialMessage = bookScript == null ? "BookScript is null." : "BookScript has null BookId.";
					throw new ApplicationException($"{initialMessage} Number of BookScripts: {bookScripts.Count}. " +
						$"BookScripts which are NOT null: {nonNullBookScriptsStr}");
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
				m_projectMetadata.ParserVersion = kParserVersion;
				if (m_books.All(b => IsNullOrEmpty(b.PageHeader)))
					ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
				UpdateControlFileVersion();
				RemoveAvailableBooksThatDoNotCorrespondToExistingBooks();
				AddMissingAvailableBooks();
			}

			if (!bookScripts.Any() || bookScripts.All(b => !b.GetScriptBlocks().Any())) // This is really unlikely for a real user (second part is probably impossible)
				Save();
			else if (QuoteSystem == null)
				GuessAtQuoteSystem();
			else if (IsQuoteSystemReadyForParse)
				DoQuoteParse(bookScripts.Select(b => b.BookId));
		}

		private void UsxWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_usxPercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(PercentInitialized, null);
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
			e.Result = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, m_books, Versification, out _,
				sender as BackgroundWorker);
		}

		private void GuessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			SetQuoteSystem(QuoteSystemStatus.Guessed, (QuoteSystem)e.Result);

			Save();
		}

		private void GuessWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_guessPercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(PercentInitialized, null);
			OnReport(pe);
		}

		private void DoQuoteParse(IEnumerable<string> booksToParse = null)
		{
			m_projectMetadata.ParserVersion = kParserVersion;
			ProjectState = ProjectState.Parsing;
			var quoteWorker = new BackgroundWorker {WorkerReportsProgress = true};
			quoteWorker.DoWork += QuoteWorker_DoWork;
			quoteWorker.RunWorkerCompleted += QuoteWorker_RunWorkerCompleted;
			quoteWorker.ProgressChanged += QuoteWorker_ProgressChanged;
			object[] parameters = {booksToParse};
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
#if DEBUG
				Exception innerException;
				if ((innerException = e.Error?.InnerException) != null)
					Debug.WriteLine(innerException.Message + innerException.StackTrace);
#endif
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
			var pe = new ProgressChangedEventArgs(PercentInitialized, null);
			OnReport(pe);
		}

		public int MaxProjectNameLength => Writer.GetMaxProjectNameLength(this);

		public BookScript LoadExistingBookIfPossible(string bookId)
		{
			try
			{
				return BookScript.Deserialize(Reader.LoadBook(this, bookId), Versification);
			}
			catch (Exception e)
			{
				ErrorReport.ReportNonFatalException(e);
				return null;
			}
		}

		public override string ValidLanguageIsoCode => IetfLanguageTag.IsValid(LanguageIsoCode) ?
			LanguageIsoCode : WellKnownSubtags.UnlistedLanguage;

		public void Analyze()
		{
			ProjectAnalysis.AnalyzeQuoteParse();
			AnalysisCompleted?.Invoke(this, new EventArgs());
		}

		private void ChangePublicationId(string newPubId)
		{
			Debug.Assert(newPubId != m_metadata.Id);

			var restoreId = m_metadata.Id;

			void SetInternalId()
			{
				m_metadata.Id = newPubId;
				m_metadata.LastModified = DateTime.Now;
			}

			void SaveMetadata(TextWriter w)
			{
				XmlSerializationHelper.Serialize(w, m_projectMetadata, out var error);
				if (error != null)
					throw error;
			}

			try
			{
				Writer.ChangePublicationId(this, SetInternalId, SaveMetadata);
			}
			catch (Exception inner)
			{
				m_metadata.Id = restoreId;
				throw new ApplicationException(Localizer.GetString("Project.ChangeMetadataIdFailure",
					"An error occurred attempting to change the publication ID for this project:"), inner);
			}
		}

		public void Save(bool saveCharacterGroups = false)
		{
			if (!ProjectIsWritable)
				return;

			Writer.SetUpProjectPersistence(this);

			SaveProjectMetadata(out var error);
			if (error != null)
			{
				MessageModal.Show(error.Message, true);
				return;
			}

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

		private void SaveProjectMetadata(out Exception error)
		{
			m_projectMetadata.LastModified = DateTime.Now;
			XmlSerializationHelper.Serialize(Writer.GetTextWriter(this, ProjectResource.Metadata), m_projectMetadata, out error);
		}

		public void SaveBook(BookScript book)
		{
			if (!XmlSerializationHelper.Serialize(Writer.GetTextWriter(this, book), book, out var error))
				MessageModal.Show(error.Message, true);
		}

		public void SaveProjectCharacterVerseData()
		{
			using (var textWriter = Writer.GetTextWriter(this, ProjectResource.CharacterVerseData))
				ProjectCharacterVerseData.Write(textWriter);
		}

		public void SaveProjectCharacterDetailData()
		{
			using (var textWriter = Writer.GetTextWriter(this, ProjectResource.CharacterDetailData))
				ProjectCharacterDetailData.Write(ProjectCharacterDetail, textWriter);
		}

		public void SaveCharacterGroupData()
		{
			XmlSerializationHelper.Serialize(Writer.GetTextWriter(this, ProjectResource.CharacterGroups),
				m_characterGroupList, out _);
		}

		public void SaveVoiceActorInformationData()
		{
			using (var textWriter = Writer.GetTextWriter(this, ProjectResource.VoiceActorInformation))
				m_voiceActorList.Save(textWriter);
		}

		private void LoadCharacterGroupData()
		{
			// ENHANCE: Add try-catch in case there is an error loading this data. This should probably be treated as non-fatal,
			// but it should warn the user that if they scrap their existing groups, any customizations or actor assignments
			// will be lost.
			m_characterGroupList = CharacterGroupList.LoadCharacterGroupList(Reader.Load(this, ProjectResource.CharacterGroups), this);
			m_characterGroupList.CharacterGroups.CollectionChanged += (o, args) => CharacterGroupCollectionChanged?.Invoke(this, new EventArgs());
			if (m_voiceActorList != null)
				EnsureCastSizeOptionValid();
		}

		private void LoadVoiceActorInformationData()
		{
			// ENHANCE: Add try-catch in case there is an error loading this data. This should probably be treated as non-fatal,
			// but it should warn the user that their actor information will be lost if they proceed.
			m_voiceActorList = VoiceActorList.LoadVoiceActorList(Reader.Load(this, ProjectResource.VoiceActorInformation));

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
				bool attemptToUseBackup = Reader.BackupResourceExists(this, ProjectResource.Ldml);

				do
				{
					if (!Reader.ResourceExists(this, ProjectResource.Ldml))
					{
						if (attemptToUseBackup)
						{
							try
							{
								Writer.RestoreResourceFromBackup(this, ProjectResource.Ldml);
								attemptToUseBackup = false;
							}
							catch (Exception exRestoreBackup)
							{
								NonFatalErrorHandler.LogAndHandleException(exRestoreBackup, "Failed to replace LDML with backup.", new Dictionary<string, string>
								{
									{"Project", ToString()},
								});
							}
						}
						if (!Reader.ResourceExists(this, ProjectResource.Ldml))
							break;
					}
					try
					{
						return LoadWritingSystemFromLdml(m_wsDefinition);
					}
					catch (XmlException e)
					{
						switch (GetBadLdmlRecoveryAction?.Invoke(this, e.Message, attemptToUseBackup))
						{
							case BadLdmlRecoveryAction.Retry:
								if (attemptToUseBackup)
								{
									try
									{
										Writer.RestoreResourceFromBackup(this, ProjectResource.Ldml);
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
							case BadLdmlRecoveryAction.Repair:
								ProjectState |= ProjectState.WritingSystemRecoveryInProcess;
								retry = false;
								break;
							default:
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

		private WritingSystemDefinition LoadWritingSystemFromLdml(WritingSystemDefinition wsDefinition)
		{
			using (var ldmlReader = Reader.Load(this, ProjectResource.Ldml))
			{
				var xmlReader = XmlReader.Create(ldmlReader);
				new LdmlDataMapper(new WritingSystemFactory()).Read(xmlReader, wsDefinition);
				return wsDefinition;
			}
		}

		private void SaveWritingSystem()
		{
			bool backupCreated = false;

			try
			{
				backupCreated = Writer.SaveBackupResource(this, ProjectResource.Ldml);
			}
			catch (Exception exMakeBackup)
			{
				// Oh, well. Hope for the best...
				NonFatalErrorHandler.LogAndHandleException(exMakeBackup, "Failed to create LDML backup", new Dictionary<string, string>
				{
					{"Project", ToString()},
				});
			}

			try
			{
				using (var ldmlWriter = Writer.GetTextWriter(this, ProjectResource.Ldml))
				{
					var settings = new XmlWriterSettings
					{
						Indent = true,
						IndentChars = "\t",
						CheckCharacters = true,
						OmitXmlDeclaration = false
					};
					using (var xmlWriter = XmlWriter.Create(ldmlWriter, settings))
					{
						new LdmlDataMapper(new WritingSystemFactory()).Write(xmlWriter, WritingSystem, null);
						xmlWriter.Flush();
						xmlWriter.Close();
					}
				}

				// Now test to see if what we wrote is actually readable...
				LoadWritingSystemFromLdml(new WritingSystemDefinition());
			}
			catch (FileLoadException)
			{
				throw; // Don't want to ignore this error - should never happen on valid installation.
			}
			catch (Exception exSave)
			{
				NonFatalErrorHandler.LogAndHandleException(exSave, "Writing System Save Failure", new Dictionary<string, string>
				{
					{"Project", ToString()},
				});
				if (backupCreated)
				{
					// If we ended up with unreadable data, revert to backup???
					try
					{
						Writer.RestoreResourceFromBackup(this, ProjectResource.Ldml);
						var wsFromBackup = new WritingSystemDefinition();
						LoadWritingSystemFromLdml(wsFromBackup);
						if (!wsFromBackup.QuotationMarks.SequenceEqual(WritingSystem.QuotationMarks) ||
							wsFromBackup.DefaultFont.Name != WritingSystem.DefaultFont.Name ||
							wsFromBackup.RightToLeftScript != WritingSystem.RightToLeftScript)
						{
							// There were significant changes that we couldn't save. Something bad is probably going to happen, so we need to tell the user.
							ErrorReport.ReportNonFatalExceptionWithMessage(exSave, Localizer.GetString("Project.RevertedToOutdatedBackupWs",
									"The current writing system settings could not be saved. Fortunately, {0} was able to recover from the backup, but " +
									"since the old settings were different, some things might not work correctly next time you open this project."),
								GlyssenInfo.Product);
						}
					}
					catch (Exception exRecover)
					{
						NonFatalErrorHandler.LogAndHandleException(exRecover, "Recovery from backup Writing System Failed.", new Dictionary<string, string>
						{
							{"Project", ToString()},
						});
						throw exSave;
					}
				}
			}
		}

		private void HandleQuoteSystemChanged()
		{
			if (QuoteSystem == null)
			{
				// This should be prevented by logic elsewhere
				throw new InvalidOperationException("HandleQuoteSystemChanged called with a null QuoteSystem.");
			}
			Project copyOfExistingProject = new Project(m_projectMetadata, Name, ws: WritingSystem);
			copyOfExistingProject.m_books.AddRange(m_books);

			m_books.Clear();

			if (IsBundleBasedProject)
			{
				if (!RobustFile.Exists(OriginalBundlePath))
				{
					// This should be prevented by logic elsewhere
					throw new InvalidOperationException("Attempt to change quote system for a bundle-based project, but the text release bundle is not present.");
				}
				UserDecisionsProject = copyOfExistingProject;
				using (var bundle = new GlyssenBundle(OriginalBundlePath))
					PopulateAndParseBooks(bundle);
			}
			else
			{
				var scrTextWrapper = GetParatextScrTextWrapper();
				if (scrTextWrapper == null)
				{
					// This should be prevented by logic elsewhere
					throw new InvalidOperationException("Attempt to change quote system for a Paratext-based project, but the project is unavailable.");
				}
				scrTextWrapper.IncludeOverriddenBooksFromProject(copyOfExistingProject);
				UserDecisionsProject = copyOfExistingProject;
				ParseAndSetBooks(scrTextWrapper.UsxDocumentsForIncludedBooks, scrTextWrapper.Stylesheet);
			}
		}

		public void CreateBackup(string textToAppendToRecordingProjectName, bool hidden = true)
		{
			if (!m_books.Any(b => b.GetScriptBlocks().Any(sb => sb.UserConfirmed)))
				return;
			Writer.CreateBackup(this, textToAppendToRecordingProjectName, hidden);
		}

		// Note: For live Paratext projects, the only proper way to change the quote system is via Paratext.
		public bool IsOkayToChangeQuoteSystem => QuoteSystem != null && IsBundleBasedProject &&
			RobustFile.Exists(OriginalBundlePath);

		private void OnReport(ProgressChangedEventArgs e)
		{
			ProgressChanged?.Invoke(this, e);
		}

		public bool IsSampleProject => Id.Equals(SampleProject.kSample, StringComparison.OrdinalIgnoreCase) && LanguageIsoCode == SampleProject.kSample;

		public static string GetDefaultRecordingProjectName(string publicationName, string languageIsoCode)
		{
			var defaultProjectNameSuffix = DefaultRecordingProjectNameSuffix;
			publicationName = FileSystemUtils.RemoveDangerousCharacters(publicationName,
				Writer.GetMaxProjectNameLength(languageIsoCode) - defaultProjectNameSuffix.Length);
			return $"{publicationName}{defaultProjectNameSuffix}";
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
			internal int FirstChapter { get; }
			internal int LastChapter { get; set; }
		}

		public void ClearCharacterStatistics()
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
						// The "Needs Review" character should never be added to a group.
						// REVIEW: By the time we get here (in production code), it shouldn't be possible
						// to have any characters that have not been disambiguated. Should we throw an
						// exception if this happens?
						if (block.CharacterIsUnclear || block.CharacterId == CharacterVerseData.kNeedsReview)
							continue;
						character = book.GetCharacterIdInScript(block);

						if (character == null)
						{
							throw new Exception("Block has character set to null. This should never happen! " +
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

					if (!bookDistributionScoreStats.TryGetValue(character, out var stats))
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

					if (!m_speechDistributionScore.TryGetValue(characterStatsInfo.Key, out var resultInMaxBook) || (resultInBook > resultInMaxBook))
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
						// we ever were to process LDML data that had already been "hacked" by Glyssen, we wouldn't want to add the
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
			// set to *none*). But the LDML specification allows all 4 options. Depending on the origin of the LDML data, this
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

		public bool ProjectIsWritable { get; set; } = true;

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
				var scrTextWrapper = GetLiveParatextDataIfCompatible(null);
				if (scrTextWrapper == null)
				{
					Logger.WriteEvent("Paratext project is unavailable or is no longer compatible! Cannot test quote system.");
					return new BookScript[] { };
				}
				usxDocsForBooksToInclude = scrTextWrapper.UsxDocumentsForIncludedBooks;
				stylesheet = scrTextWrapper.Stylesheet;
			}
			var books = UsxParser.ParseBooks(usxDocsForBooksToInclude, stylesheet,
				new CharacterUsageStore(Versification, cvInfo, GetDynamicLocalizedCharacterIdVariants), null);

			var blocksInBook = books.ToDictionary(b => b.BookId, b => b.GetScriptBlocks());

			var parsedBlocksByBook = new ConcurrentDictionary<string, BookScript>();
			Parallel.ForEach(blocksInBook, bookIdBlocksPair =>
			{
				var bookId = bookIdBlocksPair.Key;
				var blocks = new QuoteParser(cvInfo, bookId, bookIdBlocksPair.Value,
					altQuoteSystem, Versification).Parse().ToList();
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

