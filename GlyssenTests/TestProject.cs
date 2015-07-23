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
		private const string kTest = "test~~";

		public static void DeleteTestProjectFolder()
		{
			var testProjFolder = Path.Combine(Program.BaseDataFolder, kTest);
			if (Directory.Exists(testProjFolder))
				DirectoryUtilities.DeleteDirectoryRobust(testProjFolder);
		}

		public static Project CreateTestProject()
		{
			DeleteTestProjectFolder();
			var sampleMetadata = new GlyssenDblTextMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			var bookOfMark = new Book();
			bookOfMark.Code = "MRK";
			bookOfMark.IncludeInScript = true;
			bookOfMark.LongName = "Gospel of Mark";
			bookOfMark.ShortName = "Mark";
			sampleMetadata.AvailableBooks.Add(bookOfMark);
			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.Id = kTest;
			sampleMetadata.Language = new GlyssenDblMetadataLanguage { Iso = kTest };
			sampleMetadata.Identification = new DblMetadataIdentification { Name = "test~~" };
			sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
			sampleMetadata.QuoteSystem = GetTestQuoteSystem();

			XmlDocument sampleMark = new XmlDocument();
			sampleMark.LoadXml(Properties.Resources.TestMRK);
			UsxDocument mark = new UsxDocument(sampleMark);

			var project = new Project(sampleMetadata, new[] { mark }, SfmLoader.GetUsfmStylesheet());

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
	}
}
