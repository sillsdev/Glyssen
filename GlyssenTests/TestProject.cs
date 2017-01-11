using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Quote;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.Scripture;
using SIL.WritingSystems;

namespace GlyssenTests
{
	static class TestProject
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum TestBook
		{
			JOS,
			RUT,
			MRK,
			LUK,
			JHN,
			ACT,
			ICO,
			GAL,
			EPH,
			PHM,
			HEB,
			IJN,
			IIJN,
			IIIJN,
			JUD,
			REV,
		}

		private const string kTest = "test~~";

		public static void DeleteTestProjectFolder()
		{
			var testProjFolder = Path.Combine(Program.BaseDataFolder, kTest);
			if (Directory.Exists(testProjFolder))
				DirectoryUtilities.DeleteDirectoryRobust(testProjFolder);
		}

		public static Project CreateTestProject(params TestBook[] booksToInclude)
		{
			DeleteTestProjectFolder();
			var sampleMetadata = new GlyssenDblTextMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			var books = new List<UsxDocument>();

			foreach (var testBook in booksToInclude)
				AddBook(testBook, sampleMetadata, books);

			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.Id = kTest;
			sampleMetadata.Language = new GlyssenDblMetadataLanguage { Iso = kTest };
			sampleMetadata.Identification = new DblMetadataIdentification { Name = kTest };
			sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;

			var sampleWs = new WritingSystemDefinition();
			sampleWs.QuotationMarks.AddRange(GetTestQuoteSystem(booksToInclude.Contains(TestBook.JOS) || booksToInclude.Contains(TestBook.RUT)).AllLevels);

			var project = new Project(sampleMetadata, books, SfmLoader.GetUsfmStylesheet(), sampleWs);

			// Wait for quote parse to finish
			while (project.ProjectState != ProjectState.FullyInitialized)
				Thread.Sleep(100);

			return LoadExistingTestProject();
		}

		public static Project LoadExistingTestProject()
		{
			return Project.Load(Project.GetProjectFilePath(kTest, kTest, Project.GetDefaultRecordingProjectName(kTest)));
		}

		public static Project CreateBasicTestProject()
		{
			return CreateTestProject(TestBook.JUD);
		}

		public static List<BookScript> BooksBeforeQuoteParse(params TestBook[] booksToInclude)
		{
			var sampleMetadata = new GlyssenDblTextMetadata {AvailableBooks = new List<Book>()};
			var books = new List<UsxDocument>();

			foreach (var testBook in booksToInclude)
				AddBook(testBook, sampleMetadata, books);

			return UsxParser.ParseProject(books, SfmLoader.GetUsfmStylesheet(), new BackgroundWorker { WorkerReportsProgress = true });
		}

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
				case TestBook.ICO:
					book.Code = "1CO";
					book.LongName = "The First Epistle of Paul to the Church of Corinth";
					book.ShortName = "1 Corinthians";
					xmlDocument.LoadXml(Properties.Resources.Test1CO);
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
			metadata.AvailableBooks.Add(book);

			usxDocuments.Add(new UsxDocument(xmlDocument));
		}

		public static void SimulateDisambiguationForAllBooks(Project testProject)
		{
			var cvData = new CombinedCharacterVerseData(testProject);

			foreach (var book in testProject.IncludedBooks)
			{
				var bookNum = BCVRef.BookToNumber(book.BookId);
				foreach (var block in book.GetScriptBlocks())
				{
					if (block.CharacterId == CharacterVerseData.kUnknownCharacter)
					{
						block.SetCharacterAndCharacterIdInScript(CharacterVerseData.GetStandardCharacterId(book.BookId, CharacterVerseData.StandardCharacter.Narrator), bookNum, testProject.Versification);
						block.UserConfirmed = true;
					}
					else if (block.CharacterId == CharacterVerseData.kAmbiguousCharacter)
					{
						var cvEntry =
							cvData.GetCharacters(bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber,
								versification: testProject.Versification).First();
						block.SetCharacterAndCharacterIdInScript(cvEntry.Character, bookNum, testProject.Versification);
						block.Delivery = cvEntry.Delivery;
						block.UserConfirmed = true;
					}
				}
			}
		}

		public static List<CharacterDetail> GetIncludedCharacterDetails(Project project)
		{
			return project.AllCharacterDetailDictionary.Values.Where(c => project.KeyStrokesByCharacterId.Select(e => e.Key).Contains(c.CharacterId)).ToList();
		}
	}
}
