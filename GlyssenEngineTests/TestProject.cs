using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Xml;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using InMemoryTestPersistence;
using NUnit.Framework;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.WritingSystems;

namespace GlyssenEngineTests
{
	static class TestProject
	{
		private static readonly Random s_random = new Random(DateTime.Now.Millisecond);

		public class TestFontRepository : IFontRepository
		{
			public bool IsFontInstalled(string fontFamilyIdentifier)
			{
				return true;
			}

			public bool DoesTrueTypeFontFileContainFontFamily(string ttfFile, string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}

			public void TryToInstall(string fontFamilyIdentifier, IReadOnlyCollection<string> ttfFile)
			{
				throw new NotImplementedException();
			}

			public void ReportMissingFontFamily(string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}
		}

		static TestProject()
		{
			GlyssenInfo.Product = "GlyssenTests";
			Project.FontRepository = new TestFontRepository();
		}

		private static Exception m_errorDuringProjectCreation;
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum TestBook
		{
			NUM,
			JOS,
			RUT,
			PSA_NoData,
			OBA, // This is a playground book, currently useful for verse bridges and verse segments
			MAT, // This is derived from Kuna (cuk), not Acholi
			MRK,
			LUK,
			JHN,
			ACT,
			ROM_NoData,
			ICO,
			IICO_NoData,
			GAL,
			EPH,
			PHP_NoData,
			COL_NoData,
			ITH_NoData,
			IITH_NoData,
			ITI_NoData,
			IITI_NoData,
			TIT_NoData,
			PHM,
			HEB,
			JAS_NoData,
			IPE_NoData,
			IIPE_NoData,
			IJN,
			IIJN,
			IIIJN,
			JUD,
			REV,
		}

		private const string kTest = "test~~";
		private const string kTestFontFamily = "Times New Roman";

		public static void DeleteTestProjects()
		{
			((PersistenceImplementation)Project.Writer).ClearAllUserProjects();
		}

		public static Project CreateTestProject(params TestBook[] booksToInclude)
		{
			return CreateTestProject(null, booksToInclude);
		}

		public static Project CreateTestProject(string versificationInfo, params TestBook[] booksToInclude)
		{
			m_errorDuringProjectCreation = null;

			AppDomain.CurrentDomain.UnhandledException += HandleErrorDuringProjectCreation;
			var metadataId = s_random.Next(16000).ToString();
			try
			{
				var sampleMetadata = new GlyssenDblTextMetadata();
				sampleMetadata.AvailableBooks = new List<Book>();
				var books = new List<UsxDocument>();

				foreach (var testBook in booksToInclude)
					AddBook(testBook, sampleMetadata, books);

				sampleMetadata.FontFamily = kTestFontFamily;
				sampleMetadata.FontSizeInPoints = 12;
				sampleMetadata.Id = metadataId;
				sampleMetadata.Language = new GlyssenDblMetadataLanguage {Iso = kTest};
				sampleMetadata.Identification = new DblMetadataIdentification {Name = kTest};
				sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;

				var sampleWs = new WritingSystemDefinition();
				sampleWs.QuotationMarks.AddRange(GetTestQuoteSystem(booksToInclude.Contains(TestBook.JOS) || booksToInclude.Contains(TestBook.RUT) || booksToInclude.Contains(TestBook.MAT)).AllLevels);

				var project = new Project(sampleMetadata, books, SfmLoader.GetUsfmStylesheet(), sampleWs, versificationInfo);

				// Wait for quote parse to finish
				while (project.ProjectState != ProjectState.FullyInitialized && m_errorDuringProjectCreation == null)
					Thread.Sleep(30);
			}
			finally
			{
				AppDomain.CurrentDomain.UnhandledException -= HandleErrorDuringProjectCreation;
			}

			if (m_errorDuringProjectCreation != null)
				throw m_errorDuringProjectCreation;

			return LoadExistingTestProject(metadataId);
		}

		private static void HandleErrorDuringProjectCreation(object sender, UnhandledExceptionEventArgs e)
		{
			m_errorDuringProjectCreation = (e.ExceptionObject as Exception) ?? new Exception("Something went wrong on background thread trying to create test project.");
		} 

		private class TestProjectStub: IUserProject
		{
			public string Name => Project.GetDefaultRecordingProjectName(kTest, kTest);
			public string LanguageIsoCode => kTest;
			public string ValidLanguageIsoCode => WellKnownSubtags.UnlistedLanguage;
			public string MetadataId { get; }
			public string FontFamily => kTestFontFamily;

			public TestProjectStub(string metadataId)
			{
				MetadataId = metadataId;
			}
		}

		public static Project LoadExistingTestProject(string metadataId)
		{
			using (var metadataReader = ProjectBase.Reader.Load(new TestProjectStub(metadataId), ProjectResource.Metadata))
			{
				var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(metadataReader, ProjectResource.Metadata.ToString(), out var exception);
				if (exception != null)
					throw exception;
				var project = Project.Load(new Project(metadata), null, null);
				Assert.IsTrue(project.ProjectIsWritable);
				return project;
			}
		}

		public static Project CreateBasicTestProject()
		{
			return CreateTestProject(TestBook.JUD);
		}

		//public static List<BookScript> BooksBeforeQuoteParse(params TestBook[] booksToInclude)
		//{
		//	var sampleMetadata = new GlyssenDblTextMetadata {AvailableBooks = new List<Book>()};
		//	var books = new List<UsxDocument>();

		//	foreach (var testBook in booksToInclude)
		//		AddBook(testBook, sampleMetadata, books);

		//	int previousPercentageValue = 0;
		//	var reportProgress = new Action<int>(i => Assert.IsTrue(previousPercentageValue <= i));

		//	return UsxParser.ParseBooks(books, SfmLoader.GetUsfmStylesheet(), reportProgress);
		//}

		private static QuoteSystem GetTestQuoteSystem(bool includeDialogueDash)
		{
			QuoteSystem testQuoteSystem = new QuoteSystem();
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“‘“", 3, QuotationMarkingSystemType.Normal));
			if (includeDialogueDash)
				testQuoteSystem.AllLevels.Add(new QuotationMark("—", null, null, 1, QuotationMarkingSystemType.Narrative));
			return testQuoteSystem;
		}

		private static void AddBook(TestBook testBook, GlyssenDblTextMetadata metadata, List<UsxDocument> usxDocuments)
		{
			var book = new Book();
			book.IncludeInScript = true;

			XmlDocument xmlDocument = new XmlDocument();

			switch (testBook)
			{
				case TestBook.NUM:
					book.Code = "NUM";
					book.LongName = "Numbers";
					book.ShortName = "Numbers";
					xmlDocument.LoadXml(Properties.Resources.TestNUM);
					break;
				case TestBook.JOS:
					book.Code = "JOS";
					book.LongName = "Joshua";
					book.ShortName = "Joshua";
					xmlDocument.LoadXml(Properties.Resources.TestJOS);
					break;
				case TestBook.RUT:
					book.Code = "RUT";
					book.LongName = "Ruth";
					book.ShortName = "Ruth";
					xmlDocument.LoadXml(Properties.Resources.TestRUT);
					break;
				case TestBook.PSA_NoData:
					book.Code = "PSA";
					book.LongName = "The Book of Psalms";
					book.ShortName = "Psalms";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.OBA:
					book.Code = "OBA";
					book.LongName = "Obadiah";
					book.ShortName = "Obadiah";
					xmlDocument.LoadXml(Properties.Resources.TestOBAwithInterestingVerseNums);
					break;
				case TestBook.MAT:
					book.Code = "MAT";
					book.LongName = "Gospel of Matthew";
					book.ShortName = "Matthew";
					xmlDocument.LoadXml(Properties.Resources.TestMATcuk);
					break;
				case TestBook.MRK:
					book.Code = "MRK";
					book.LongName = "Gospel of Mark";
					book.ShortName = "Mark";
					xmlDocument.LoadXml(Properties.Resources.TestMRK);
					break;
				case TestBook.LUK:
					book.Code = "LUK";
					book.LongName = "Gospel of Luke";
					book.ShortName = "Luke";
					xmlDocument.LoadXml(Properties.Resources.TestLUK);
					break;
				case TestBook.JHN:
					book.Code = "JHN";
					book.LongName = "Gospel of John";
					book.ShortName = "John";
					xmlDocument.LoadXml(Properties.Resources.TestJHN);
					break;
				case TestBook.ACT:
					book.Code = "ACT";
					book.LongName = "The Acts of the Apostles";
					book.ShortName = "Acts";
					xmlDocument.LoadXml(Properties.Resources.TestACT);
					break;
				case TestBook.ROM_NoData:
					book.Code = "ROM";
					book.LongName = "The Epistle of Paul to the Romans";
					book.ShortName = "Romans";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.ICO:
					book.Code = "1CO";
					book.LongName = "The First Epistle of Paul to the Church of Corinth";
					book.ShortName = "1 Corinthians";
					xmlDocument.LoadXml(Properties.Resources.Test1CO);
					break;
				case TestBook.IICO_NoData:
					book.Code = "2CO";
					book.LongName = "The First Epistle of Paul to the Church of Corinth";
					book.ShortName = "2 Corinthians";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.GAL:
					book.Code = "GAL";
					book.LongName = "The Epistle of Paul to the Church of Galatia";
					book.ShortName = "Galatians";
					xmlDocument.LoadXml(Properties.Resources.TestGAL);
					break;
				case TestBook.EPH:
					book.Code = "EPH";
					book.LongName = "The Epistle of Paul to the Church of Ephesus";
					book.ShortName = "Ephesians";
					xmlDocument.LoadXml(Properties.Resources.TestEPH);
					break;
				case TestBook.PHP_NoData:
					book.Code = "PHP";
					book.LongName = "The Epistle of Paul to the Philippians";
					book.ShortName = "Philippians";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.COL_NoData:
					book.Code = "COL";
					book.LongName = "The Epistle of Paul to the Colossians";
					book.ShortName = "Colossians";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.ITH_NoData:
					book.Code = "1TH";
					book.LongName = "The First Epistle of Paul to the Thessalonians";
					book.ShortName = "1 Thessalonians";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.IITH_NoData:
					book.Code = "2TH";
					book.LongName = "The Second Epistle of Paul to the Thessalonians";
					book.ShortName = "2 Thessalonians";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.ITI_NoData:
					book.Code = "1TI";
					book.LongName = "The First Epistle of Paul to Timothy";
					book.ShortName = "1 Timothy";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.IITI_NoData:
					book.Code = "2TI";
					book.LongName = "The Second Epistle of Paul to Timothy";
					book.ShortName = "2 Timothy";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.TIT_NoData:
					book.Code = "TIT";
					book.LongName = "The Epistle of Paul to Titus";
					book.ShortName = "Titus";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.PHM:
					book.Code = "PHM";
					book.LongName = "Paul's Letter to Philemon";
					book.ShortName = "Philemon";
					xmlDocument.LoadXml(Properties.Resources.TestPHM);
					break;
				case TestBook.HEB:
					book.Code = "HEB";
					book.LongName = "Hebrews";
					book.ShortName = "Hebrews";
					xmlDocument.LoadXml(Properties.Resources.TestHEB);
					break;
				case TestBook.JAS_NoData:
					book.Code = "JAS";
					book.LongName = "The Epistle of James";
					book.ShortName = "James";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.IPE_NoData:
					book.Code = "1PE";
					book.LongName = "The First Epistle of Peter";
					book.ShortName = "1 Peter";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.IIPE_NoData:
					book.Code = "2PE";
					book.LongName = "The Second Epistle of Peter";
					book.ShortName = "2 Peter";
					xmlDocument.LoadXml(String.Format(Properties.Resources.TestEmptyBook, book.Code));
					break;
				case TestBook.IJN:
					book.Code = "1JN";
					book.LongName = "The First Epistle of John";
					book.ShortName = "1 John";
					xmlDocument.LoadXml(Properties.Resources.Test1JN);
					break;
				case TestBook.IIJN:
					book.Code = "2JN";
					book.LongName = "The Second Epistle of John";
					book.ShortName = "2 John";
					xmlDocument.LoadXml(Properties.Resources.Test2JN);
					break;
				case TestBook.IIIJN:
					book.Code = "3JN";
					book.LongName = "The Third Epistle of John";
					book.ShortName = "3 John";
					xmlDocument.LoadXml(Properties.Resources.Test3JN);
					break;
				case TestBook.JUD:
					book.Code = "JUD";
					book.LongName = "Jude";
					book.ShortName = "Jude";
					xmlDocument.LoadXml(Properties.Resources.TestJUD);
					break;
				case TestBook.REV:
					book.Code = "REV";
					book.LongName = "The Book of the Revelation of Saint John";
					book.ShortName = "Revelation";
					xmlDocument.LoadXml(Properties.Resources.TestREV);
					break;
				default:
					throw new ArgumentOutOfRangeException("testBook", testBook, null);
			}

			var insertAt = 0;
			foreach (var bookCode in StandardCanon.AllBookCodes)
			{
				if (bookCode == book.Code)
					break;
				if (metadata.AvailableBooks.Any(b => b.Code == bookCode))
					insertAt++;
			}
			metadata.AvailableBooks.Insert(insertAt, book);

			usxDocuments.Add(new UsxDocument(xmlDocument));
		}

		public static void SimulateDisambiguationForAllBooks(Project testProject)
		{
			testProject.DoTestDisambiguation();
		}

		public static List<CharacterDetail> GetIncludedCharacterDetails(Project project)
		{
			return project.AllCharacterDetailDictionary.Values.Where(c => project.KeyStrokesByCharacterId.Select(e => e.Key).Contains(c.CharacterId)).ToList();
		}
	}
}
