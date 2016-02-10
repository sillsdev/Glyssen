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
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.WritingSystems;

namespace GlyssenTests
{
	[TestFixture, Timeout(60000)]
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

			WaitForProjectInitializationToFinish(project, ProjectState.FullyInitialized);

			Assert.AreEqual(bogusQuoteSystem, project.QuoteSystem);
			Assert.AreEqual(QuoteSystemStatus.Obtained, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void CreateFromBundle_BundleDoesNotContainQuoteInformation_GuessesQuoteSystem()
		{
			var bundle = GetGlyssenBundleToBeUsedForProject();
			bundle.WritingSystemDefinition.QuotationMarks.Clear();
			var project = new Project(bundle);

			WaitForProjectInitializationToFinish(project, ProjectState.NeedsQuoteSystemConfirmation);

			Assert.IsTrue(project.QuoteSystem.AllLevels.Any());
			Assert.AreEqual(QuoteSystemStatus.Guessed, project.Status.QuoteSystemStatus);
		}

		[Test]
		public void UpdateProjectFromBundleData()
		{
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.Metadata.FontSizeInPoints = 10;
			var project = new Project(originalBundle);

			WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);

			Assert.AreEqual(10, project.FontSizeInPoints);

			var newBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.Metadata.FontSizeInPoints = 12;
			var updatedProject = project.UpdateProjectFromBundleData(newBundle);

			WaitForProjectInitializationToFinish(updatedProject, ProjectState.ReadyForUserInteraction);

			Assert.AreEqual(12, updatedProject.FontSizeInPoints);
		}

		[Test]
		public void UpdateProjectFromBundleData_BundleDoesNotContainLdmlFile_MaintainsOriginalQuoteSystem()
		{
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.WritingSystemDefinition.QuotationMarks[0] = new QuotationMark("open", "close", "cont", 1, QuotationMarkingSystemType.Normal);
			var project = new Project(originalBundle);

			WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);

			Assert.AreEqual("open", project.QuoteSystem.FirstLevel.Open);

			var newBundle = GetGlyssenBundleToBeUsedForProject(false);
			Assert.IsNull(newBundle.WritingSystemDefinition);
			var updatedProject = project.UpdateProjectFromBundleData(newBundle);

			Assert.AreEqual("open", updatedProject.QuoteSystem.FirstLevel.Open);
		}

		[Test]
		public void UpdateProjectFromBundleData_ExistingProjectHasUserDecisions_UserDecisionsApplied()
		{
			var originalBundle = GetGlyssenBundleToBeUsedForProject();
			originalBundle.WritingSystemDefinition.QuotationMarks[0] = new QuotationMark("open", "close", "cont", 1, QuotationMarkingSystemType.Normal);
			var project = new Project(originalBundle);

			WaitForProjectInitializationToFinish(project, ProjectState.FullyInitialized);

			var firstBook = project.Books[0];
			var block = firstBook.GetScriptBlocks().Last();
			var verseRef = new BCVRef(BCVRef.BookToNumber(firstBook.BookId), block.ChapterNumber, block.InitialStartVerseNumber);
			block.SetCharacterAndDelivery(new List<CharacterVerse>(
				new [] { new CharacterVerse(verseRef, "Wilma", "agitated beyond belief", null, true) }));
			block.UserConfirmed = true;

			var newBundle = GetGlyssenBundleToBeUsedForProject();
			var updatedProject = project.UpdateProjectFromBundleData(newBundle);

			WaitForProjectInitializationToFinish(updatedProject, ProjectState.FullyInitialized);

			Assert.AreEqual(verseRef.Verse, updatedProject.Books[0].GetScriptBlocks().First(b => b.CharacterId == "Wilma").InitialStartVerseNumber);
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

				WaitForProjectInitializationToFinish(project, ProjectState.FullyInitialized);

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

		[Test]
		public void ConvertContinuersToParatextAssumptions_Level1Only_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_2Levels_NoContinuer_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", null, 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", null, 2, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_2Levels_Continuer_ModifiesLevel2Continuer()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_3Levels_Continuer_ModifiesLevel2AndLevel3Continuers()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 3, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_Level1NormalAndNarrative_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void ConvertContinuersToParatextAssumptions_3LevelsPlusNarrative_Continuer_ModifiesLevel2AndLevel3Continuers()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 3, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.ConvertContinuersToParatextAssumptions();

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void Load_MetadataContainsAvailableBookThatDoesNotExist_SpuriousBookRemovedFromMetadata()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.AvailableBooks.Insert(0, new Book { Code = "GEN" });
			metadata.AvailableBooks.Insert(0, new Book { Code = "PSA" });
			metadata.AvailableBooks.Insert(0, new Book { Code = "MAT" });
			project.Save();

			project = TestProject.LoadExistingTestProject();

			Assert.AreEqual("JUD", project.AvailableBooks.Single().Code);
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_ProjectIsCreatedSuccessfully()
		{
			var bundle = GetGlyssenBundleToBeUsedForProject(false);
			var project = new Project(bundle);
			WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
			Assert.IsNotNull(project);
			Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
		}

		[Test]
		public void Constructor_MetadataContainsAvailableBookThatDoesNotExist_SpuriousBookRemovedFromMetadata()
		{
			var sampleMetadata = new GlyssenDblTextMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			sampleMetadata.AvailableBooks.Insert(0, new Book { Code = "GEN" });
			sampleMetadata.AvailableBooks.Insert(0, new Book { Code = "PSA" });
			sampleMetadata.AvailableBooks.Insert(0, new Book { Code = "MAT" });

			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.Id = "~~funkyFrogLipsAndStuff";
			sampleMetadata.Language = new GlyssenDblMetadataLanguage { Iso = "~~funkyFrogLipsAndStuff" };
			sampleMetadata.Identification = new DblMetadataIdentification { Name = "~~funkyFrogLipsAndStuff" };

			var sampleWs = new WritingSystemDefinition();

			try
			{
				var project = new Project(sampleMetadata, new List<UsxDocument>(), SfmLoader.GetUsfmStylesheet(), sampleWs);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.False(project.AvailableBooks.Any());
			}
			finally
			{
				var testProjFolder = Path.Combine(Program.BaseDataFolder, "~~funkyFrogLipsAndStuff");
				if (Directory.Exists(testProjFolder))
					DirectoryUtilities.DeleteDirectoryRobust(testProjFolder);
			}
		}

		[TestCase(2, 1, 1, 0)]
		[TestCase(1, 1, 1, 0)]
		[TestCase(1, 0, 1, 0)]
		[TestCase(0, 1, 0, 1)]
		[TestCase(0, 2, 0, 1)]
		[TestCase(1, 2, 1, 0)]
		public void SetCharacterGroupGenerationPreferencesToValidValues_OneBook(int numMaleNarrators, int numFemaleNarrators, int resultMale, int resultFemale)
		{
			var testProject = TestProject.CreateBasicTestProject();
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMaleNarrators;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemaleNarrators;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(resultMale, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(resultFemale, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[TestCase(1, 1, 1, 1)]
		[TestCase(2, 1, 2, 1)]
		[TestCase(3, 1, 3, 0)]
		[TestCase(4, 1, 3, 0)]
		[TestCase(1, 2, 1, 2)]
		[TestCase(1, 3, 1, 2)]
		public void SetCharacterGroupGenerationPreferencesToValidValues_ThreeBooks(int numMaleNarrators, int numFemaleNarrators, int resultMale, int resultFemale)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMaleNarrators;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemaleNarrators;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(resultMale, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(resultFemale, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[Test]
		public void Load_JustOneBookAvailableAndOneIncluded_AutomaticallyFlagAsReviewed()
		{
			var project = TestProject.CreateBasicTestProject();
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(project, "m_metadata");
			metadata.AvailableBooks.Insert(0, new Book { Code = "GEN" });
			project.Save();

			project = TestProject.LoadExistingTestProject();

			Assert.AreEqual(BookSelectionStatus.Reviewed, project.BookSelectionStatus);
		}

		private void WaitForProjectInitializationToFinish(Project project, ProjectState projectState)
		{
			const int maxCyclesAllowed = 100;
			int iCycle = 1;
			while ((project.ProjectState & projectState) == 0)
			{
				if (iCycle++ < maxCyclesAllowed)
					Thread.Sleep(100);
				else
					Assert.Fail("Timed out waiting for project initialization. Expected ProjectState = " + projectState + "; current ProjectState = " + project.ProjectState);
			}
		}
	}
}
