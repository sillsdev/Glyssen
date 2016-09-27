﻿using System.Collections.Generic;
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
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Properties.Resources.TestCharacterDetail;

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
			project.VoiceActorList.AllActors.Add(actor);
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
			project.VoiceActorList.AllActors.Add(actor);
			var group = new CharacterGroup(project);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.False(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_EveryAssignedGroupHasACharacter_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.AllActors.Add(actor);
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
			project.VoiceActorList.AllActors.Add(actor);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.EveryAssignedGroupHasACharacter);
		}

		[Test]
		public void UnusedActors_NoUnusedActor_ReturnsEmptyEnumeration()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor();
			project.VoiceActorList.AllActors.Add(actor1);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.UnusedActors.Any());
		}

		[Test]
		public void UnusedActors_UnusedActor_ReturnsCorrectActor()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			project.VoiceActorList.AllActors.Add(actor2);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.AreEqual(actor2, project.UnusedActors.Single());
		}

		[Test]
		public void UnusedActors_UsedActorAndInactiveActor_ReturnsEmptyEnumeration()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor1 = new Glyssen.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new Glyssen.VoiceActor.VoiceActor { Id = 1, IsInactive = true };
			project.VoiceActorList.AllActors.Add(actor2);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.UnusedActors.Any());
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
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsIsoIsSet_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Ldml = "";
				bundle.Metadata.Language.Iso = "ach";
				bundle.Metadata.Language.Name = "Acholi"; // see messages in Assert.AreEqual lines below
				var project = new Project(bundle);
				m_tempProjectFolders.Add(Path.GetDirectoryName(Path.GetDirectoryName(project.ProjectFilePath)));
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
				Assert.AreEqual("Acoli", project.WritingSystem.Language.Name,
					"This name should be coming from the \"global\" cache, not from the metadata above - note spelling difference.");
				Assert.AreEqual("ach", project.WritingSystem.Language.Iso3Code,
					"If \"ach\" is not found in the global cache, the lnaguage subtag will be considered \"private-use\" and the " +
					"ISO code will be null");
				Assert.AreEqual("ach", project.WritingSystem.Language.Code);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsLdmlIsSet_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Ldml = "ach";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsLdmlHasCountySpecified_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Iso = "ach";
				bundle.Metadata.Language.Ldml = "ach-CM";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsLdmlAndIsoCodesHaveCountySpecified_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Iso = "ach-CM";
				bundle.Metadata.Language.Ldml = "ach-CM";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsLdmlCodeIsInvalid_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Iso = "ach-CM";
				bundle.Metadata.Language.Ldml = "ach%CM***-blah___ickypoo!";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsIsoCodeIsInvalid_ProjectIsCreatedSuccessfully()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Iso = "ach%CM-blah___ickypoo!";
				bundle.Metadata.Language.Ldml = "ach-CM";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("ach", project.WritingSystem.Id);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsIsoCodeNotInLanguageRepo_ProjectIsCreatedUsingPrivateUseWritingSystem()
		{
			Sldr.Initialize();
			try
			{
				var bundle = GetGlyssenBundleToBeUsedForProject(false);
				bundle.Metadata.Language.Iso = "zyt";
				bundle.Metadata.Language.Ldml = "";
				var project = new Project(bundle);
				WaitForProjectInitializationToFinish(project, ProjectState.ReadyForUserInteraction);
				Assert.IsNotNull(project);
				Assert.IsNotEmpty(project.QuoteSystem.AllLevels);
				Assert.AreEqual("zyt", project.WritingSystem.Id);
				Assert.IsTrue(project.WritingSystem.Language.IsPrivateUse);
			}
			finally
			{
				Sldr.Cleanup();
			}
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
			testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.Custom;
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
			testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.Custom;
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMaleNarrators;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemaleNarrators;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(resultMale, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(resultFemale, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[TestCase(1, 0)]
		[TestCase(2, 0)]
		[TestCase(3, 0)]
		public void SetCharacterGroupGenerationPreferencesToValidValues_NarrationByAuthor_NumbersSnapToActualNumberOfAuthors(int numMaleNarrators, int numFemaleNarrators)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMaleNarrators;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = numFemaleNarrators;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(2, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(0, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[Test]
		public void GetFormattedChapterAnnouncement_ChapterLabel_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);

			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.ChapterLabel);
			// In reality, we wouldn't expect GetFormattedChapterAnnouncement to get called at all in this
			// case, but safer to just have it return null.
			Assert.IsNull(testProject.GetFormattedChapterAnnouncement("1JN", 4));
		}

		[Test]
		public void GetFormattedChapterAnnouncement_PageHeader_BookNameComesFromPageHeader()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);

			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.PageHeader);
			Assert.AreEqual("1 JON 4", testProject.GetFormattedChapterAnnouncement("1JN", 4));
		}

		[Test]
		public void GetFormattedChapterAnnouncement_MainTitle1_BookNameComesFromMainTitle1()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);

			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.MainTitle1);
			Assert.AreEqual("JON 4", testProject.GetFormattedChapterAnnouncement("1JN", 4));
		}

		[Test]
		public void GetFormattedChapterAnnouncement_LongName_BookNameComesFromLongName()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);

			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.LongNameFromMetadata);
			Assert.AreEqual("The First Epistle of John 4", testProject.GetFormattedChapterAnnouncement("1JN", 4));
		}

		[Test]
		public void GetFormattedChapterAnnouncement_ShortName_BookNameComesFromShortName()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);

			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.ShortNameFromMetadata);
			Assert.AreEqual("1 John 4", testProject.GetFormattedChapterAnnouncement("1JN", 4));
		}

		[Test]
		public void GetFormattedChapterAnnouncement_NoBookNameAvailable_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN);
			testProject.SetBlockGetChapterAnnouncement(ChapterAnnouncement.ShortNameFromMetadata);
			var metadata = (GlyssenDblTextMetadata)ReflectionHelper.GetField(testProject, "m_metadata");
			metadata.AvailableBooks[0].ShortName = "   ";
			Assert.IsNull(testProject.GetFormattedChapterAnnouncement("1JN", 4));

			metadata.AvailableBooks[0].ShortName = null;
			Assert.IsNull(testProject.GetFormattedChapterAnnouncement("1JN", 4));
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

		[Test]
		public void CalculateSpeechDistributionScore_CharacterWhoDoesNotSpeak_ReturnsZero()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			Assert.IsFalse(testProject.SpeechDistributionScoreByCharacterId.ContainsKey("Jesus"));
		}

		[Test]
		public void CalculateSpeechDistributionScore_CharacterWhoSpeaksOnlyOnce_ReturnsOne()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			Assert.AreEqual(1, testProject.SpeechDistributionScoreByCharacterId["apostles"]);
			Assert.AreEqual(1, testProject.SpeechDistributionScoreByCharacterId["Enoch"]);
		}

		[Test]
		public void CalculateSpeechDistributionScore_CharacterWhoSpeaksFourTimesInOnlyOneChapter_ReturnsFour()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			Assert.AreEqual(4, testProject.SpeechDistributionScoreByCharacterId["Stephen"]);
		}

		[Test]
		public void CalculateSpeechDistributionScore_CharacterWhoSpeaksThriceInOneChapterTwiceInAnotherChapterAndOnceInEachOfTwoOtherChapterAcrossRangeOfSevenChapters_ReturnsThirtyNine()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			Assert.AreEqual(39, testProject.SpeechDistributionScoreByCharacterId["angel of the LORD, an"]);
		}

		[Test]
		public void CalculateSpeechDistributionScore_CharacterWhoSpeaksALotInOneBookAndALittleInAnother_ReturnsResultFromMaxBook()
		{
			var testProjectA = TestProject.CreateTestProject(TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProjectA);
			var resultFromRev = testProjectA.SpeechDistributionScoreByCharacterId["God"];

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.REV);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			Assert.AreEqual(resultFromRev, testProject.SpeechDistributionScoreByCharacterId["God"]);
		}

		[Test]
		public void CalculateSpeechDistributionScore_BoazInProjectThatOnlyIncludesRuth_ReturnsResultFromMaxBook()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			Assert.IsTrue(testProject.SpeechDistributionScoreByCharacterId["Boaz"] >= 7);
		}

		private void WaitForProjectInitializationToFinish(Project project, ProjectState projectState)
		{
			const int maxCyclesAllowed = 1000000;
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
