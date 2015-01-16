using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using L10NSharp;
using Palaso.Reporting;
using Palaso.Xml;
using ProtoScript.Analysis;
using ProtoScript.Bundle;
using ProtoScript.Character;
using ProtoScript.Properties;
using ProtoScript.Quote;
using SIL.ScriptureUtils;
using Canon = ProtoScript.Bundle.Canon;

namespace ProtoScript
{
	public class Project
	{
		public const string kProjectFileExtension = ".pgproj";
		public const string kBookScriptFileExtension = ".xml";
		public const string kProjectCharacterVerseFileName = "ProjectCharacterVerse.txt";
		public const string kDefaultFontPrimary = "Charis SIL";
		public const string kDefaultFontSecondary = "Times New Roman";
		public const int kDefaultFontSize = 14;
		private readonly DblMetadata m_metadata;
		private QuoteSystem m_defaultQuoteSystem = QuoteSystem.Default;
		private readonly List<BookScript> m_books = new List<BookScript>();

		public Project(DblMetadata metadata)
		{
			m_metadata = metadata;
			ProjectCharacterVerseData = new ProjectCharacterVerseData(ProjectCharacterVerseDataPath);
		}

		public Project(Bundle.Bundle bundle) : this(bundle.Metadata)
		{
			PopulateAndParseBooks(bundle);
		}

		public Project(DblMetadata metadata, IEnumerable<UsxDocument> books, IStylesheet stylesheet) : this(metadata)
		{
			AddAndParseBooks(books, stylesheet);
		}

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

		public string Language
		{
			get { return m_metadata.language.ToString(); }
		}

		public string FontFamily
		{
			get { return m_metadata.FontFamily; }
		}

		public int FontSizeInPoints
		{
			get { return m_metadata.FontSizeInPoints; }
		}

		public bool RightToLeftScript
		{
			get { return m_metadata.language.scriptDirection == "RTL"; }
		}

		public QuoteSystem QuoteSystem
		{
			get { return m_metadata.QuoteSystem ?? m_defaultQuoteSystem; }
			set
			{
				bool quoteSystemBeingSetForFirstTime = ConfirmedQuoteSystem == null;
				bool quoteSystemChanged = ConfirmedQuoteSystem != value;
				m_metadata.QuoteSystem = value;
				if (quoteSystemChanged)
				{
					if (quoteSystemBeingSetForFirstTime)
						DoQuoteParse();
					else
						HandleQuoteSystemChanged();
				}
			}
		}

		public QuoteSystem ConfirmedQuoteSystem
		{
			get { return m_metadata.QuoteSystem; }
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

		public ProjectCharacterVerseData ProjectCharacterVerseData;

		public static Project Load(string projectFilePath)
		{
			Project existingProject = LoadExistingProject(projectFilePath);

			if (existingProject.m_metadata.PgUsxParserVersion != Settings.Default.PgUsxParserVersion &&
				File.Exists(existingProject.m_metadata.OriginalPathOfDblFile))
			{
				var bundle = new Bundle.Bundle(existingProject.m_metadata.OriginalPathOfDblFile);
				// See if we already have a project for this bundle and open it instead.
				var upgradedProject = new Project(bundle.Metadata);
				upgradedProject.QuoteSystem = existingProject.m_metadata.QuoteSystem;
				// Prior to Parser version 17, project metadata didn't keep the Books collection.
				if (existingProject.m_metadata.AvailableBooks != null && existingProject.m_metadata.AvailableBooks.Any())
					upgradedProject.m_metadata.AvailableBooks = existingProject.m_metadata.AvailableBooks;
				upgradedProject.PopulateAndParseBooks(bundle);
				upgradedProject.ApplyUserDecisions(existingProject);
				return upgradedProject;
			}
			
			existingProject.InitializeLoadedProject();
			return existingProject;
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
			int controlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;
			if (ConfirmedQuoteSystem == null)
			{
				GuessAtQuoteSystem();
				DoQuoteParse();
				m_metadata.ControlFileVersion = controlFileVersion;
			}
			else if (m_metadata.ControlFileVersion != controlFileVersion)
			{
				new CharacterAssigner(new CombinedCharacterVerseData(this)).AssignAll(m_books);
				m_metadata.ControlFileVersion = controlFileVersion;
			}
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
			Canon canon;
			if (bundle.TryGetCanon(1, out canon))
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
			foreach (var book in books)
			{
				var bookId = book.BookId;
				m_books.Add(new BookScript(bookId, new UsxParser(bookId, stylesheet, book.GetChaptersAndParas()).Parse()));
			}

			if (ConfirmedQuoteSystem == null)
				GuessAtQuoteSystem();

			DoQuoteParse();
		}

		private void GuessAtQuoteSystem()
		{
			bool certain;
			m_defaultQuoteSystem = QuoteSystemGuesser.Guess(ControlCharacterVerseData.Singleton, m_books, out certain);
			if (certain)
				m_metadata.QuoteSystem = m_defaultQuoteSystem;
		}

		private void DoQuoteParse()
		{
			var cvInfo = ProjectCharacterVerseData;
			foreach (var bookScript in m_books)
				bookScript.Blocks = new QuoteParser(cvInfo, bookScript.BookId, bookScript.GetScriptBlocks(), ConfirmedQuoteSystem).Parse().ToList();
#if DEBUG
			new ProjectAnalysis(this).AnalyzeQuoteParse();
#endif
		}

		public static string GetProjectFilePath(string langId, string bundleId)
		{
			return Path.Combine(ProjectsBaseFolder, langId, bundleId, langId + kProjectFileExtension);
		}

		public void Save()
		{
			var projectPath = GetProjectFilePath(m_metadata.language.ToString(), m_metadata.id);
			Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
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
			if (File.Exists(m_metadata.OriginalPathOfDblFile) && QuoteSystem != null)
			{
				var bundle = new Bundle.Bundle(m_metadata.OriginalPathOfDblFile);
				PopulateAndParseBooks(bundle);
			}
			else if (File.Exists(m_metadata.OriginalPathOfSfmFile) && QuoteSystem != null)
			{
				AddAndParseBooks(new [] { SfmLoader.LoadSfmBook(m_metadata.OriginalPathOfSfmFile) }, SfmLoader.GetUsfmStylesheet());
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
			get { return Path.Combine(ProjectsBaseFolder, m_metadata.language.ToString(), m_metadata.id, kProjectCharacterVerseFileName); }
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

		public static void CreateSampleProjectIfNeeded()
		{
			var samplePath = GetProjectFilePath("sample", "sample");
			if (File.Exists(samplePath))
				return;
			var sampleMetadata = new DblMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			var bookOfMarkMetadata = new Book();
			bookOfMarkMetadata.Code = "MRK";
			bookOfMarkMetadata.IncludeInScript = true;
			bookOfMarkMetadata.LongName = "Gospel of Mark";
			bookOfMarkMetadata.ShortName = "Mark";
			sampleMetadata.AvailableBooks.Add(bookOfMarkMetadata);
			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.id = "Sample";
			sampleMetadata.language = new DblMetadataLanguage {iso = "sample"};

			XmlDocument sampleMark = new XmlDocument();
			sampleMark.LoadXml(Resources.SampleMRK);
			UsxDocument mark = new UsxDocument(sampleMark);

			(new Project(sampleMetadata, new[] { mark }, SfmLoader.GetUsfmStylesheet())).Save();
		}
	}
}
