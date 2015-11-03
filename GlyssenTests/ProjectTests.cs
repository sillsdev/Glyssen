using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
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
		private readonly HashSet<string> m_tempProjectFolders = new HashSet<string>();
		private GlyssenBundle GetGlyssenBundleToBeUsedForProject(bool includeLdml = true)
		{
			var bundle = GlyssenBundleTests.GetNewGlyssenBundleForTest(includeLdml);
			m_tempProjectFolders.Add(Path.Combine(Program.BaseDataFolder, bundle.Metadata.Id));
			return bundle;
		}

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			// Clean up anything from previously aborted tests
			foreach (var directory in Directory.GetDirectories(Program.BaseDataFolder, GlyssenBundleTests.kTestBundleIdPrefix + "*"))
				DirectoryUtilities.DeleteDirectoryRobust(directory);				
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			foreach (var folder in m_tempProjectFolders)
				DirectoryUtilities.DeleteDirectoryRobust(folder);				
		}

		[Test]
		public void CreateFromBundle_BundleContainsQuoteInformation_LoadsQuoteSystemFromBundle()
		{
			var bundle = GetGlyssenBundleToBeUsedForProject();
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
			var bundle = GetGlyssenBundleToBeUsedForProject();
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
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.Metadata.FontSizeInPoints = 10;
			var project = new Project(originalBundle);

			Assert.AreEqual(10, project.FontSizeInPoints);

			var newBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.Metadata.FontSizeInPoints = 12;
			project.UpdateProjectFromBundleData(newBundle);

			Assert.AreEqual(12, project.FontSizeInPoints);
		}

		[Test]
		public void UpdateProjectFromBundleData_BundleDoesNotContainLdmlFile_MaintainsOriginalQuoteSystem()
		{
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.WritingSystemDefinition.QuotationMarks[0] = new QuotationMark("open", "close", "cont", 1, QuotationMarkingSystemType.Normal);
			var project = new Project(originalBundle);

			Assert.AreEqual("open", project.QuoteSystem.FirstLevel.Open);

			var newBundle = GetGlyssenBundleToBeUsedForProject(false);
			Assert.IsNull(newBundle.WritingSystemDefinition);
			project.UpdateProjectFromBundleData(newBundle);

			Assert.AreEqual("open", project.QuoteSystem.FirstLevel.Open);
		}

		[Test]
		public void CopyQuoteMarksIfAppropriate_TargetWsHasNoQuotes_TargetReceivesQuotes()
		{
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
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
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
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
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
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
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
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
			try
			{
				m_tempProjectFolders.Add(Path.Combine(Program.BaseDataFolder, originalBundleAndFile.Item1.Metadata.Id));
				var originalBundle = originalBundleAndFile.Item1;
				var project = new Project(originalBundle);

				// Wait for project initialization to finish
				while (project.ProjectState != ProjectState.FullyInitialized)
					Thread.Sleep(100);

				project.QuoteSystem = QuoteSystem.Default;
			}
			finally
			{
				// Must dispose after because changing the quote system needs access to original bundle file
				originalBundleAndFile.Item2.Dispose();
			}
		}

		[Test]
		public void IsVoiceActorAssignmentsComplete_Complete_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void IsVoiceActorAssignmentsComplete_GroupWithNoActor_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.False(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_EveryAssignedGroupHasACharacter_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.CharacterIds.Add("Bob");
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.True(project.EveryAssignedGroupHasACharacter);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_AssignedGroupWithNoCharacter_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.EveryAssignedGroupHasACharacter);
		}

		[Test]
		public void HasUnusedActor_NoUnusedActor_ReturnsFalse()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.Actors.Add(actor1);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.HasUnusedActor);
		}

		[Test]
		public void HasUnusedActor_UnusedActor_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.Actors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			project.VoiceActorList.Actors.Add(actor2);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.True(project.HasUnusedActor);
		}
	}
}
