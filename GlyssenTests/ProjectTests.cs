using System.IO;
using System.Linq;
using System.Threading;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Quote;
using GlyssenTests.Bundle;
using NUnit.Framework;
using SIL.IO;
using SIL.ObjectModel;
using SIL.WritingSystems;

namespace GlyssenTests
{
	class ProjectTests
	{
		public const string kTest = "test~~ProjectTests";

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			DirectoryUtilities.DeleteDirectoryRobust(Path.Combine(Program.BaseDataFolder, kTest));
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			DirectoryUtilities.DeleteDirectoryRobust(Path.Combine(Program.BaseDataFolder, kTest));
		}

		[Test]
		public void CreateFromBundle_BundleContainsQuoteInformation_LoadsQuoteSystemFromBundle()
		{
			var bundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			var bogusQuoteSystem = new QuoteSystem(new QuotationMark("^", "^^", "^^^", 1, QuotationMarkingSystemType.Normal));
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			bundle.WritingSystemDefinition.QuotationMarks.AddRange(bogusQuoteSystem.AllLevels);
			var project = new Project(bundle);

			// Wait for project initialization to finish
			while (project.ProjectState != ProjectState.FullyInitialized)
				Thread.Sleep(100);

			Assert.AreEqual(bogusQuoteSystem, project.QuoteSystem);
			Assert.AreEqual(QuoteSystemStatus.Obtained, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void CreateFromBundle_BundleDoesNotContainQuoteInformation_GuessesQuoteSystem()
		{
			var bundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var project = new Project(bundle);

			// Wait for project initialization to finish
			while (project.ProjectState != ProjectState.NeedsQuoteSystemConfirmation)
				Thread.Sleep(100);

			Assert.IsTrue(project.QuoteSystem.AllLevels.Any());
			Assert.AreEqual(QuoteSystemStatus.Guessed, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void UpdateProjectFromBundleData()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			originalBundle.Metadata.FontSizeInPoints = 10;
			var project = new Project(originalBundle);

			Assert.AreEqual(10, project.FontSizeInPoints);

			var newBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			originalBundle.Metadata.FontSizeInPoints = 12;
			project.UpdateProjectFromBundleData(newBundle);

			Assert.AreEqual(12, project.FontSizeInPoints);
		}

		[Test]
		public void UpdateProjectFromBundleData_BundleDoesNotContainLdmlFile_MaintainsOriginalQuoteSystem()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			originalBundle.WritingSystemDefinition.QuotationMarks[0] = new QuotationMark("open", "close", "cont", 1, QuotationMarkingSystemType.Normal);
			var project = new Project(originalBundle);

			Assert.AreEqual("open", project.QuoteSystem.FirstLevel.Open);

			var newBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest(false);
			Assert.IsNull(newBundle.WritingSystemDefinition);
			project.UpdateProjectFromBundleData(newBundle);

			Assert.AreEqual("open", project.QuoteSystem.FirstLevel.Open);
		}

		[Test]
		public void CopyQuoteMarksIfAppropriate_TargetWsHasNoQuotes_TargetReceivesQuotes()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			var project = new Project(originalBundle);
			project.Status.QuoteSystemStatus = QuoteSystemStatus.UserSet;

			WritingSystemDefinition targetWs = new WritingSystemDefinition();
			GlyssenDblTextMetadata targetMetadata = new GlyssenDblTextMetadata();
			project.CopyQuoteMarksIfAppropriate(targetWs, targetMetadata);

			Assert.AreEqual(project.QuoteSystem.AllLevels, targetWs.QuotationMarks);
			Assert.AreEqual(QuoteSystemStatus.UserSet, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void CopyQuoteMarksIfAppropriate_TargetWsHasQuotes_TargetQuotesObtained_TargetDoesNotReceiveQuotes()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			var project = new Project(originalBundle);
			project.Status.QuoteSystemStatus = QuoteSystemStatus.Obtained;

			WritingSystemDefinition targetWs = new WritingSystemDefinition();
			var bogusQuoteSystem = new QuoteSystem(
				new QuotationMark("^", "^^", "^^^", 1, QuotationMarkingSystemType.Normal)
			);
			targetWs.QuotationMarks.AddRange(bogusQuoteSystem.AllLevels);
			GlyssenDblTextMetadata targetMetadata = new GlyssenDblTextMetadata();
			targetMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
			project.CopyQuoteMarksIfAppropriate(targetWs, targetMetadata);

			Assert.AreEqual(bogusQuoteSystem.AllLevels, targetWs.QuotationMarks);
			Assert.AreEqual(QuoteSystemStatus.Obtained, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void CopyQuoteMarksIfAppropriate_TargetWsHasLessQuoteLevelsThanOriginal_CommonLevelsSame_TargetReceivesQuotes()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			var bogusQuoteSystem = new QuoteSystem(new BulkObservableList<QuotationMark>
			{
				new QuotationMark("^", "^^", "^^^", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("$", "^^$", "$$$", 1, QuotationMarkingSystemType.Normal)
			});
			originalBundle.WritingSystemDefinition.QuotationMarks.Clear();
			originalBundle.WritingSystemDefinition.QuotationMarks.AddRange(bogusQuoteSystem.AllLevels);
			var project = new Project(originalBundle);
			project.Status.QuoteSystemStatus = QuoteSystemStatus.UserSet;

			WritingSystemDefinition targetWs = new WritingSystemDefinition();
			var bogusQuoteSystem2 = new QuoteSystem(
				new QuotationMark("^", "^^", "^^^", 1, QuotationMarkingSystemType.Normal)
			);
			targetWs.QuotationMarks.AddRange(bogusQuoteSystem2.AllLevels);

			GlyssenDblTextMetadata targetMetadata = new GlyssenDblTextMetadata();
			project.CopyQuoteMarksIfAppropriate(targetWs, targetMetadata);

			Assert.AreEqual(bogusQuoteSystem.AllLevels, targetWs.QuotationMarks);
			Assert.AreEqual(QuoteSystemStatus.UserSet, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void CopyQuoteMarksIfAppropriate_TargetWsHasLessQuoteLevelsThanOriginal_CommonLevelsDifferent_TargetDoesNotReceiveQuotes()
		{
			var originalBundle = GlyssenBundleTests.GetNewGlyssenBundleForTest();
			var bogusQuoteSystem = new QuoteSystem(new BulkObservableList<QuotationMark>
			{
				new QuotationMark("^", "^^", "^^^", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("$", "^^$", "$$$", 1, QuotationMarkingSystemType.Normal)
			});
			originalBundle.WritingSystemDefinition.QuotationMarks.Clear();
			originalBundle.WritingSystemDefinition.QuotationMarks.AddRange(bogusQuoteSystem.AllLevels);
			var project = new Project(originalBundle);
			project.Status.QuoteSystemStatus = QuoteSystemStatus.UserSet;

			WritingSystemDefinition targetWs = new WritingSystemDefinition();
			var bogusQuoteSystem2 = new QuoteSystem(
				new QuotationMark("$", "$$", "$$$", 1, QuotationMarkingSystemType.Normal)
			);
			targetWs.QuotationMarks.AddRange(bogusQuoteSystem2.AllLevels);

			GlyssenDblTextMetadata targetMetadata = new GlyssenDblTextMetadata();
			project.CopyQuoteMarksIfAppropriate(targetWs, targetMetadata);

			Assert.AreEqual(bogusQuoteSystem2.AllLevels, targetWs.QuotationMarks);
			Assert.AreEqual(QuoteSystemStatus.UserSet, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void QuoteSystem_Changed()
		{
			var originalBundleAndFile = GlyssenBundleTests.GetNewGlyssenBundleAndFile();
			var originalBundle = originalBundleAndFile.Item1;
			var originalBundleFile = originalBundleAndFile.Item2;
			var project = new Project(originalBundle);

			// Wait for project initialization to finish
			while (project.ProjectState != ProjectState.FullyInitialized)
				Thread.Sleep(100);

			project.QuoteSystem = QuoteSystem.Default;

			// Must dispose after because changing the quote system needs access to original bundle file
			originalBundleFile.Dispose();
		}
	}
}
