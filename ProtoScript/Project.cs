using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using L10NSharp;
using ProtoScript.Analysis;
using ProtoScript.Bundle;
using ProtoScript.Character;
using ProtoScript.Properties;
using ProtoScript.Quote;
using SIL.Reporting;
using SIL.ScriptureUtils;
using SIL.Windows.Forms.WritingSystems;
using SIL.WritingSystems;
using SIL.Xml;
using Canon = ProtoScript.Bundle.Canon;

namespace ProtoScript
{
	public class Project
	{
		public const string kProjectFileExtension = ".pgproj";
		public const string kBookScriptFileExtension = ".xml";
		public const string kLdmlFileExtension = ".ldml";
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kVersificationFileName = "versification.vrs";
		public const string kDefaultFontPrimary = "Charis SIL";
		public const string kDefaultFontSecondary = "Times New Roman";
		public const int kDefaultFontSize = 14;

		private const double kUsxPercent = 0.25;
		private const double kGuessPercent = 0.10;
		private const double kQuotePercent = 0.65;

		private readonly DblMetadata m_metadata;
		private QuoteSystem m_defaultQuoteSystem = QuoteSystem.Default;
		private readonly List<BookScript> m_books = new List<BookScript>();
		private int m_usxPercentComplete;
		private int m_guessPercentComplete;
		private int m_quotePercentComplete;
		private ProjectState m_projectState;
		private ProjectAnalysis m_analysis;
		private WritingSystemDefinition m_wsDefinition;
		private IWritingSystemRepository m_wsRepository;

		public Project(DblMetadata metadata)
		{
			m_metadata = metadata;
			ProjectCharacterVerseData = new ProjectCharacterVerseData(ProjectCharacterVerseDataPath);
		}

		public Project(Bundle.Bundle bundle) : this(bundle.Metadata)
		{
			Directory.CreateDirectory(ProjectFolder);
			bundle.CopyVersificationFile(VersificationFilePath);
			PopulateAndParseBooks(bundle);
		}

		public Project(DblMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet) : this(metadata)
		{
			// TODO (PG-117): Allow caller to pass in a versification.
			//File.Copy(FileLocator.GetFileDistributedWithApplication("eng.vrs"), VersificationFilePath);
			AddAndParseBooks(books, stylesheet);
		}

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public event EventHandler<ProjectStateChangedEventArgs> ProjectStateChanged;

		public static string ProjectsBaseFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Program.kCompany, Program.kProduct);
			}
		}

		public string Id
		{
			get { return m_metadata.id; }
		}

		public string Name
		{
			get
			{
				if (m_metadata.identification == null) //Some old Sample projects will fall into this category
					return Id;
				return m_metadata.identification.name;
			}
		}

		public string LanguageIsoCode
		{
			get { return m_metadata.language.iso; }
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
			get { return m_metadata.language.scriptDirection == "RTL"; }
		}

		public Paratext.ScrVers Versification
		{
			get
			{
				if (VersificationFilePath != null)
				{
					// TODO (PG-117): return custom versification.
				}
				return Paratext.ScrVers.English;
			}
		}

		public ProjectStatus Status
		{
			get { return m_metadata.ProjectStatus; }
			private set { m_metadata.ProjectStatus = value; }
		}

		public ProjectAnalysis ProjectAnalysis
		{
			get { return m_analysis ?? (m_analysis = new ProjectAnalysis(this)); }
		}

		public QuoteSystem QuoteSystem
		{
			get { return m_metadata.QuoteSystem ?? m_defaultQuoteSystem; }
			set
			{
				bool quoteSystemBeingSetForFirstTime = ConfirmedQuoteSystem == null;
				bool quoteSystemChanged = ConfirmedQuoteSystem != value;
				m_metadata.QuoteSystem = value;

				if (IsQuoteSystemUserConfirmed && ProjectState == ProjectState.NeedsQuoteSystemConfirmation)
					DoQuoteParse();
				else if (quoteSystemChanged && !quoteSystemBeingSetForFirstTime)
					HandleQuoteSystemChanged();
			}
		}

		public QuoteSystem ConfirmedQuoteSystem
		{
			get { return m_metadata.QuoteSystem; }
		}

		public bool IsQuoteSystemUserConfirmed
		{
			get { return m_metadata.IsQuoteSystemUserConfirmed; }
			set { m_metadata.IsQuoteSystemUserConfirmed = value; }
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

		public IReadOnlyList<Book> AvailableBooks { get { return m_metadata.AvailableBooks; } }

		public string OriginalPathOfDblFile { get { return m_metadata.OriginalPathOfDblFile; } }

		public ProjectCharacterVerseData ProjectCharacterVerseData;

		public ProjectMetadataViewModel ProjectMetadataViewModel
		{
			get
			{
				var wsModel = new WritingSystemSetupModel(WritingSystem)
				{
					CurrentDefaultFontName = FontFamily,
					CurrentDefaultFontSize = FontSizeInPoints,
					CurrentRightToLeftScript = RightToLeftScript
				};
				var model = new ProjectMetadataViewModel(wsModel)
				{
					LanguageName = m_metadata.language.name,
					IsoCode = m_metadata.language.iso,
					ProjectId = Id,
					ProjectName = m_metadata.identification == null ? null : m_metadata.identification.name
				};
				return model;
			}
			set
			{
				ProjectMetadataViewModel model = value;
				m_metadata.id = model.ProjectId;
				m_metadata.language.iso = model.IsoCode;
				m_metadata.identification.name = model.ProjectName;
				m_metadata.language.name = model.LanguageName;
				m_metadata.FontFamily = model.WsModel.CurrentDefaultFontName;
				m_metadata.FontSizeInPoints = (int)model.WsModel.CurrentDefaultFontSize;
				m_metadata.language.scriptDirection = model.WsModel.CurrentRightToLeftScript ? "RTL" : "LTR";
			}
		}

		public int PercentInitialized { get; private set; }

		public ProjectState ProjectState
		{
			get { return m_projectState; }
			private set
			{
				if (m_projectState == value)
					return;
				m_projectState = value;
				OnStateChanged(new ProjectStateChangedEventArgs { ProjectState = m_projectState });
			}
		}

		internal void ClearProjectStatus()
		{
			Status = new ProjectStatus();
		}

		public static Project Load(string projectFilePath)
		{
			Project existingProject = LoadExistingProject(projectFilePath);

			if (existingProject.m_metadata.PgUsxParserVersion != Settings.Default.PgUsxParserVersion &&
				File.Exists(existingProject.m_metadata.OriginalPathOfDblFile))
			{
				using (var bundle = new Bundle.Bundle(existingProject.m_metadata.OriginalPathOfDblFile))
				{
					var upgradedProject = new Project(bundle.Metadata);
					upgradedProject.QuoteSystem = existingProject.m_metadata.QuoteSystem;
					// Prior to Parser version 17, project metadata didn't keep the Books collection.
					if (existingProject.m_metadata.AvailableBooks != null && existingProject.m_metadata.AvailableBooks.Any())
						upgradedProject.m_metadata.AvailableBooks = existingProject.m_metadata.AvailableBooks;
					upgradedProject.PopulateAndParseBooks(bundle);
					upgradedProject.ApplyUserDecisions(existingProject);
					return upgradedProject;
				}
			}
			
			existingProject.InitializeLoadedProject();
			return existingProject;
		}

		public static void SetHiddenFlag(string projectFilePath, bool hidden)
		{
			Exception exception;
			var metadata = DblMetadata.Load(projectFilePath, out exception);
			if (exception != null)
			{
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					LocalizationManager.GetString("File.ProjectCouldNotBeModified", "Project could not be modified: {0}"), projectFilePath);
				return;
			}
			metadata.HiddenByDefault = hidden;
			new Project(metadata).Save();
		}

		private int UpdatePercentInitialized()
		{
			return PercentInitialized = (int)(m_usxPercentComplete * kUsxPercent + m_guessPercentComplete * kGuessPercent + m_quotePercentComplete * kQuotePercent);
		}

		private static Project LoadExistingProject(string projectFilePath)
		{
			Exception exception;
			var metadata = DblMetadata.Load(projectFilePath, out exception);
			if (exception != null)
			{
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					LocalizationManager.GetString("File.ProjectMetadataInvalid", "Project could not be loaded: {0}"), projectFilePath);
				return null;
			}
			Project project = new Project(metadata);
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
			return project;
		}

		private void InitializeLoadedProject()
		{
			LoadWritingSystem();

			m_usxPercentComplete = 100;
			int controlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;
			if (ConfirmedQuoteSystem == null)
			{
				GuessAtQuoteSystem();
				m_metadata.ControlFileVersion = controlFileVersion;
				return;
			}
			m_guessPercentComplete = 100;
			if (!IsQuoteSystemUserConfirmed)
			{
				m_quotePercentComplete = 0;
				UpdatePercentInitialized();
				ProjectState = ProjectState.NeedsQuoteSystemConfirmation;
				return;
			}
			m_quotePercentComplete = 100;
			if (m_metadata.ControlFileVersion != controlFileVersion)
			{
				new CharacterAssigner(new CombinedCharacterVerseData(this)).AssignAll(m_books);
				m_metadata.ControlFileVersion = controlFileVersion;
			}
			UpdatePercentInitialized();
			ProjectState = ProjectState.FullyInitialized;
			Analyze();
		}

		private void ApplyUserDecisions(Project sourceProject)
		{
			for (int iBook = 0; iBook < m_books.Count; iBook++)
			{
				var targetBookScript = m_books[iBook];
				var sourceBookScript = sourceProject.m_books.SingleOrDefault(b => b.BookId == targetBookScript.BookId);
				if (sourceBookScript != null)
					targetBookScript.ApplyUserDecisions(sourceBookScript);
			}
		}

		private void PopulateAndParseBooks(Bundle.Bundle bundle)
		{
			Canon canon = null;
			// TODO PG-36
			// This is a hack until we fully implement canons
			for (int i = 0; i < 100; i++)
				if (bundle.TryGetCanon(i, out canon))
					break;
			if (canon == null)
				throw new ArgumentException("The bundle must contain at least one canon with an ID between 0 and 99.");
			AddAndParseBooks(GetUsxBooksToInclude(canon), bundle.Stylesheet);
		}

		private IEnumerable<UsxDocument> GetUsxBooksToInclude(Canon canon)
		{
			foreach (var book in m_metadata.AvailableBooks.Where(b => b.IncludeInScript))
			{
				UsxDocument usxBook;
				if (canon.TryGetBook(book.Code, out usxBook))
					yield return usxBook;
			}
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
			var parameters = e.Argument as object[];
			var books = (IEnumerable<UsxDocument>)parameters[0];
			var stylesheet = (IStylesheet)parameters[1];

			e.Result = new ProjectUsxParser().ParseProject(books, stylesheet, sender as BackgroundWorker);
		}

		private void UsxWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;
			
			var result = (IEnumerable<BookScript>)e.Result;
			//REVIEW: more efficient to sort after the fact like this?  Or just don't run them in parallel (in ProjectUsxParser) in the first place?
			var resultList = result.ToList();
			resultList.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));
			m_books.AddRange(resultList);

			if (ConfirmedQuoteSystem == null)
				GuessAtQuoteSystem();
			else if (IsQuoteSystemUserConfirmed)
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
			m_defaultQuoteSystem = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, m_books, out certain, sender as BackgroundWorker);
			e.Result = certain;
		}

		private void GuessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			if ((bool)e.Result) //certain
			{
				IsQuoteSystemUserConfirmed = false;
				QuoteSystem = m_defaultQuoteSystem;
			}
			
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
			ProjectState = ProjectState.GuessComplete;
			var quoteWorker = new BackgroundWorker { WorkerReportsProgress = true };
			quoteWorker.DoWork += QuoteWorker_DoWork;
			quoteWorker.RunWorkerCompleted += QuoteWorker_RunWorkerCompleted;
			quoteWorker.ProgressChanged += QuoteWorker_ProgressChanged;
			quoteWorker.RunWorkerAsync();
		}

		private void QuoteWorker_DoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			new ProjectQuoteParser().ParseProject(this, sender as BackgroundWorker);
		}

		private void QuoteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
				throw e.Error;

			Analyze();
			Save();
		}

		private void QuoteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_quotePercentComplete = e.ProgressPercentage;
			var pe = new ProgressChangedEventArgs(UpdatePercentInitialized(), null);
			OnReport(pe);
		}

		public static string GetProjectFilePath(string langId, string bundleId)
		{
			return Path.Combine(ProjectsBaseFolder, langId, bundleId, langId + kProjectFileExtension);
		}

		public void Analyze()
		{
			ProjectAnalysis.AnalyzeQuoteParse();
		}

		public void Save()
		{
			Directory.CreateDirectory(ProjectFolder);

			var projectPath = GetProjectFilePath(m_metadata.language.ToString(), m_metadata.id);
			Exception error;
			XmlSerializationHelper.SerializeToFile(projectPath, m_metadata, out error);
			if (error != null)
			{
				MessageBox.Show(error.Message);
				return;
			}
			Settings.Default.CurrentProject = projectPath;
			var projectFolder = Path.GetDirectoryName(projectPath);
			foreach (var book in m_books)
			{
				var filePath = Path.ChangeExtension(Path.Combine(projectFolder, book.BookId), "xml");
				XmlSerializationHelper.SerializeToFile(filePath, book, out error);
				if (error != null)
					MessageBox.Show(error.Message);
			}
			ProjectCharacterVerseData.WriteToFile(ProjectCharacterVerseDataPath);
			SaveWritingSystem();
			ProjectState = !IsQuoteSystemUserConfirmed ? ProjectState.NeedsQuoteSystemConfirmation : ProjectState.FullyInitialized;
		}

		private WritingSystemDefinition WritingSystem
		{
			get
			{
				if (m_wsDefinition != null)
					return m_wsDefinition;

				string languagecode = LanguageIsoCode;
				if (!IetfLanguageTagHelper.IsValid(languagecode))
					languagecode = WellKnownSubtags.UnlistedLanguage;

				if (!WritingSystemRepository.TryGet(languagecode, out m_wsDefinition))
					m_wsDefinition = new WritingSystemDefinition(languagecode);

				WritingSystemRepository.Set(m_wsDefinition);
				return m_wsDefinition;
			}
		}

		private void SaveWritingSystem()
		{
			WritingSystemDefinition ws = WritingSystem;
			ws.QuotationMarks.Clear();
			if (ConfirmedQuoteSystem != null)
				ws.QuotationMarks.AddRange(ConfirmedQuoteSystem.AllLevels);

			WritingSystemRepository.Save();
		}

		private void LoadWritingSystem()
		{
			WritingSystemDefinition ws = WritingSystem;
			if (m_metadata.QuoteSystem == null)
				m_metadata.QuoteSystem = new QuoteSystem();

			// If we read in the quote data from the metadata (where it used to be stored)
			// and haven't created the LDML file yet, don't blow away the old data yet
			if (ws.QuotationMarks.Any())
			{
				m_metadata.QuoteSystem.AllLevels.Clear();
				m_metadata.QuoteSystem.AllLevels.AddRange(ws.QuotationMarks);
			}
		}

		public void ExportTabDelimited(string fileName)
		{
			int blockNumber = 1;
			using (var stream = new StreamWriter(fileName, false, Encoding.UTF8))
			{
				foreach (var book in IncludedBooks)
				{
					foreach (var block in book.GetScriptBlocks(true))
					{
						stream.WriteLine((blockNumber++) + "\t" + block.GetAsTabDelimited(book.BookId));
					}
				}
			}
		}

		private void HandleQuoteSystemChanged()
		{
			m_books.Clear();

			if (File.Exists(m_metadata.OriginalPathOfDblFile) && QuoteSystem != null)
			{
				using (var bundle = new Bundle.Bundle(m_metadata.OriginalPathOfDblFile))
					PopulateAndParseBooks(bundle);
			}
			else if (File.Exists(m_metadata.OriginalPathOfSfmFile) && QuoteSystem != null)
			{
				AddAndParseBooks(new[] { SfmLoader.LoadSfmBook(m_metadata.OriginalPathOfSfmFile) }, SfmLoader.GetUsfmStylesheet());
			}
			else if (Directory.Exists(m_metadata.OriginalPathOfSfmDirectory) && QuoteSystem != null)
			{
				AddAndParseBooks(SfmLoader.LoadSfmFolder(m_metadata.OriginalPathOfSfmDirectory), SfmLoader.GetUsfmStylesheet());
			}
			else
			{
				//TODO
				throw new ApplicationException();
			}
		}

		private string ProjectCharacterVerseDataPath
		{
			get { return Path.Combine(ProjectFolder, kProjectCharacterVerseFileName); }
		}

		private string VersificationFilePath
		{
			get { return Path.Combine(ProjectFolder, kVersificationFileName); }
		}

		private string ProjectFolder
		{
			get { return Path.Combine(ProjectsBaseFolder, m_metadata.language.ToString(), m_metadata.id); }
		}

		private IWritingSystemRepository WritingSystemRepository
		{
			get
			{
				if (m_wsRepository != null)
					return m_wsRepository;
				return m_wsRepository = LdmlInFolderWritingSystemRepository.Initialize(ProjectFolder);
			}
		}

		public bool IsReparseOkay()
		{
			if (QuoteSystem == null)
				return false;
			if (File.Exists(m_metadata.OriginalPathOfDblFile))
				return true;
			if (File.Exists(m_metadata.OriginalPathOfSfmFile))
				return true;
			if (Directory.Exists(m_metadata.OriginalPathOfSfmDirectory))
			{
				// Ensure the books present originally are the same as those present now
				List<UsxDocument> booksInFolder = SfmLoader.LoadSfmFolder(m_metadata.OriginalPathOfSfmDirectory);
				if (booksInFolder.Count != m_metadata.AvailableBooks.Count)
					return false;
				List<string> bookIdsInFolder = booksInFolder.Select(b => b.BookId).ToList();
				return m_metadata.AvailableBooks.All(book => bookIdsInFolder.Contains(book.Code));
			}
			return false;
		}

		private void OnReport(ProgressChangedEventArgs e)
		{
			EventHandler<ProgressChangedEventArgs> handler = ProgressChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		private void OnStateChanged(ProjectStateChangedEventArgs e)
		{
			EventHandler<ProjectStateChangedEventArgs> handler = ProjectStateChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public static void CreateSampleProjectIfNeeded()
		{
			const string kSample = "sample";
			var samplePath = GetProjectFilePath(kSample, kSample);
			if (File.Exists(samplePath))
				return;
			var sampleMetadata = new DblMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			var bookOfMark = new Book();
			bookOfMark.Code = "MRK";
			bookOfMark.IncludeInScript = true;
			bookOfMark.LongName = "Gospel of Mark";
			bookOfMark.ShortName = "Mark";
			sampleMetadata.AvailableBooks.Add(bookOfMark);
			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.id = "Sample";
			sampleMetadata.language = new DblMetadataLanguage {iso = kSample};
			sampleMetadata.identification = new DblMetadataIdentification { name = "Sample Project", nameLocal = "Sample Project"};

			XmlDocument sampleMark = new XmlDocument();
			sampleMark.LoadXml(Resources.SampleMRK);
			UsxDocument mark = new UsxDocument(sampleMark);

			new Project(sampleMetadata, new[] { mark }, SfmLoader.GetUsfmStylesheet());
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
		GuessComplete = 4,
		NeedsQuoteSystemConfirmation = 8,
		QuoteParseComplete = 16,
		FullyInitialized = 32,
		ReadyForUserInteraction = NeedsQuoteSystemConfirmation | FullyInitialized
	}
}
