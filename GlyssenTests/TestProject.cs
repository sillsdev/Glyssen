using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Quote;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.WritingSystems;

namespace GlyssenTests
{
	class TestProject
	{
		public enum TestBook
		{
			MRK,
			LUK,
			ACT,
			JUD
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
			sampleMetadata.Identification = new DblMetadataIdentification { Name = "test~~" };
			sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
			sampleMetadata.QuoteSystem = GetTestQuoteSystem();

			var project = new Project(sampleMetadata, books, SfmLoader.GetUsfmStylesheet());

			// Wait for quote parse to finish
			while (project.ProjectState != ProjectState.FullyInitialized)
				Thread.Sleep(100);

			return Project.Load(Project.GetProjectFilePath(kTest, kTest, Project.GetDefaultRecordingProjectName(kTest)));
		}

		private static QuoteSystem GetTestQuoteSystem()
		{
			QuoteSystem testQuoteSystem = new QuoteSystem();
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“‘“", 3, QuotationMarkingSystemType.Normal));
			return testQuoteSystem;
		}

		private static void AddBook(TestBook testBook, GlyssenDblTextMetadata metadata, List<UsxDocument> usxDocuments)
		{
			var book = new Book();
			book.IncludeInScript = true;
			
			XmlDocument xmlDocument = new XmlDocument();

			switch (testBook)
			{
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
				case TestBook.ACT:
					book.Code = "ACT";
					book.LongName = "The Acts of the Apostles";
					book.ShortName = "Acts";
					xmlDocument.LoadXml(Properties.Resources.TestACT);
					break;
				case TestBook.JUD:
					book.Code = "JUD";
					book.LongName = "Jude";
					book.ShortName = "Jude";
					xmlDocument.LoadXml(Properties.Resources.TestJUD);
					break;
				default:
					throw new ArgumentOutOfRangeException("testBook", testBook, null);
			}
			metadata.AvailableBooks.Add(book);

			usxDocuments.Add(new UsxDocument(xmlDocument));
		}
	}
}
