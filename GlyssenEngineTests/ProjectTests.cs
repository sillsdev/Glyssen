using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Paratext;
using GlyssenEngine.Quote;
using GlyssenEngineTests.Bundle;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.Extensions;
using SIL.IO;
using SIL.ObjectModel;
using SIL.Reflection;
using SIL.Scripture;
using SIL.WritingSystems;

namespace GlyssenEngineTests
{
	[TestFixture, Timeout(70000)]
	class ProjectTests
	{
		private readonly HashSet<string> m_tempProjectFolders = new HashSet<string>();
		private GlyssenBundle GetGlyssenBundleToBeUsedForProject(bool includeLdml = true)
		{
			var bundle = GlyssenBundleTests.GetNewGlyssenBundleForTest(includeLdml);
			m_tempProjectFolders.Add(Path.Combine(GlyssenInfo.BaseDataFolder, bundle.Metadata.Id));
			return bundle;
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
		}

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = Properties.Resources.TestCharacterDetail;

			// Clean up anything from previously aborted tests
			foreach (var directory in Directory.GetDirectories(GlyssenInfo.BaseDataFolder, GlyssenBundleTests.kTestBundleIdPrefix + "*"))
				RobustIO.DeleteDirectoryAndContents(directory);
		}

		[TestFixtureTearDown]
		public void TestFixtureTeardown()
		{
			foreach (var folder in m_tempProjectFolders)
				RobustIO.DeleteDirectoryAndContents(folder);
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
		[Timeout(9000)]
		public void SetQuoteSystem_QuoteParseCompletedCalledWithNewQuoteSystem()
		{
			var originalBundleAndFile = GlyssenBundleTests.GetNewGlyssenBundleAndFile();
			try
			{
				m_tempProjectFolders.Add(Path.Combine(GlyssenInfo.BaseDataFolder, originalBundleAndFile.Item1.Metadata.Id));
				var originalBundle = originalBundleAndFile.Item1;
				var project = new Project(originalBundle);

				WaitForProjectInitializationToFinish(project, ProjectState.FullyInitialized);

				QuoteSystem quoteSystemAfterQuoteParserCompletes = null;

				project.QuoteParseCompleted += delegate(object sender, EventArgs args)
				{
					quoteSystemAfterQuoteParserCompletes = ((Project)sender).QuoteSystem;
				};

				Assert.AreNotEqual(QuoteSystem.Default, project.QuoteSystem);
				project.QuoteSystem = QuoteSystem.Default;

				do
				{
					Thread.Sleep(100);
				} while (quoteSystemAfterQuoteParserCompletes == null);

				Assert.AreEqual(QuoteSystem.Default, quoteSystemAfterQuoteParserCompletes);
				Assert.AreEqual(project.QuoteSystem, quoteSystemAfterQuoteParserCompletes);
			}
			finally
			{
				// Must dispose after because changing the quote system needs access to original bundle file
				originalBundleAndFile.Item2.Dispose();
			}
		}

		[TestCase("Boaz")]
		[TestCase("Mr. Rogers")]
		[Timeout(8000)]
		public void SetQuoteSystem_ProjectHasCustomCharacterVerseDecisions_UserDecisionsReapplied(string character)
		{
			var originalBundleAndFile = GlyssenBundleTests.GetNewGlyssenBundleAndFile();
			try
			{
				m_tempProjectFolders.Add(Path.Combine(GlyssenInfo.BaseDataFolder, originalBundleAndFile.Item1.Metadata.Id));
				var originalBundle = originalBundleAndFile.Item1;
				var testProject = new Project(originalBundle);

				WaitForProjectInitializationToFinish(testProject, ProjectState.FullyInitialized);

				var book = testProject.IncludedBooks.First();
				var matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(book,
					book.GetScriptBlocks().Count - 1);

				SetAndConfirmCharacterAndDeliveryForAllCorrelatedBlocks(matchup, book.BookNumber, testProject, character);

				Assert.IsTrue(testProject.ProjectCharacterVerseData.Any());
				if (!CharacterDetailData.Singleton.GetDictionary().ContainsKey(character))
					testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = character, Age = CharacterAge.Elder, Gender = CharacterGender.Male });

				matchup.Apply(testProject.Versification);

				var newQuoteSystem = new QuoteSystem(testProject.QuoteSystem);
				newQuoteSystem.AllLevels.Add(new QuotationMark("=+", "#$", "^&", newQuoteSystem.FirstLevel.Level, QuotationMarkingSystemType.Narrative));

				bool complete = false;

				testProject.QuoteParseCompleted += delegate
				{
					complete = true;
				};

				var origCountOfUserConfirmedBlocks = book.GetScriptBlocks().Count(b => b.UserConfirmed);

				testProject.QuoteSystem = newQuoteSystem;

				do
				{
					Thread.Sleep(100);
				} while (!complete);

				var userConfirmedBlocksAfterReapplying = testProject.IncludedBooks.First().GetScriptBlocks().Where(b => b.UserConfirmed).ToList();
				Assert.AreEqual(origCountOfUserConfirmedBlocks, userConfirmedBlocksAfterReapplying.Count);
				foreach (var blockWithReappliedUserDecision in userConfirmedBlocksAfterReapplying)
				{
					Assert.AreEqual(character, blockWithReappliedUserDecision.CharacterId);
					Assert.AreEqual("foamy", blockWithReappliedUserDecision.Delivery);
				}
			}
			finally
			{
				// Must dispose after because changing the quote system needs access to original bundle file
				originalBundleAndFile.Item2.Dispose();
			}
		}

		[TestCase("Boaz")]
		[TestCase("Mr. Rogers")]
		[Timeout(8000)]
		public void UpdateProjectFromBundleData_ProjectHasCustomCharacterVerseDecisions_UserDecisionsReapplied(string character)
		{
			var originalBundleAndFile = GlyssenBundleTests.GetNewGlyssenBundleAndFile();
			try
			{
				m_tempProjectFolders.Add(Path.Combine(GlyssenInfo.BaseDataFolder, originalBundleAndFile.Item1.Metadata.Id));
				var originalBundle = originalBundleAndFile.Item1;
				var testProject = new Project(originalBundle);

				WaitForProjectInitializationToFinish(testProject, ProjectState.FullyInitialized);

				var book = testProject.IncludedBooks.First();
				var matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(book,
					book.GetScriptBlocks().Count - 1);

				SetAndConfirmCharacterAndDeliveryForAllCorrelatedBlocks(matchup, book.BookNumber, testProject, character);

				Assert.IsTrue(testProject.ProjectCharacterVerseData.Any());
				if (!CharacterDetailData.Singleton.GetDictionary().ContainsKey(character))
					testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = character, Age = CharacterAge.Elder, Gender = CharacterGender.Male });

				matchup.Apply(testProject.Versification);

				bool complete = false;

				var origCountOfUserConfirmedBlocks = book.GetScriptBlocks().Count(b => b.UserConfirmed);

				var updatedProject = testProject.UpdateProjectFromBundleData(originalBundle);

				updatedProject.QuoteParseCompleted += delegate
				{
					complete = true;
				};

				do
				{
					Thread.Sleep(100);
				} while (!complete);

				var userConfirmedBlocksAfterReapplying = updatedProject.IncludedBooks.First().GetScriptBlocks().Where(b => b.UserConfirmed).ToList();
				Assert.AreEqual(origCountOfUserConfirmedBlocks, userConfirmedBlocksAfterReapplying.Count);
				foreach (var blockWithReappliedUserDecision in userConfirmedBlocksAfterReapplying)
				{
					Assert.AreEqual(character, blockWithReappliedUserDecision.CharacterId);
					Assert.AreEqual("foamy", blockWithReappliedUserDecision.Delivery);
				}
			}
			finally
			{
				originalBundleAndFile.Item2.Dispose();
			}
		}

		[TestCase("Boaz")]
		[TestCase("Mr. Rogers")]
		[Timeout(10000)]
		public void UpdateFromParatextData_ProjectHasCustomCharacterVerseDecisions_UserDecisionsReapplied(string character)
		{
			var originalBundleAndFile = GlyssenBundleTests.GetNewGlyssenBundleAndFile();
			try
			{
				m_tempProjectFolders.Add(Path.Combine(GlyssenInfo.BaseDataFolder, originalBundleAndFile.Item1.Metadata.Id));
				var originalBundle = originalBundleAndFile.Item1;
				var testProject = new Project(originalBundle);

				WaitForProjectInitializationToFinish(testProject, ProjectState.FullyInitialized);

				var book = testProject.IncludedBooks.First();
				var matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(book,
					book.GetScriptBlocks().Count - 1);

				SetAndConfirmCharacterAndDeliveryForAllCorrelatedBlocks(matchup, book.BookNumber, testProject, character);

				Assert.IsTrue(testProject.ProjectCharacterVerseData.Any());
				if (!CharacterDetailData.Singleton.GetDictionary().ContainsKey(character))
					testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = character, Age = CharacterAge.Elder, Gender = CharacterGender.Male });

				matchup.Apply(testProject.Versification);

				bool complete = false;

				var origCountOfUserConfirmedBlocks = book.GetScriptBlocks().Count(b => b.UserConfirmed);

				var scrTextWrapper = MockRepository.GenerateMock<IParatextScrTextWrapper>();
				scrTextWrapper.Stub(w => w.AvailableBooks).Return(testProject.AvailableBooks);
				scrTextWrapper.Stub(w => w.HasQuotationRulesSet).Return(false);
				scrTextWrapper.Stub(w => w.DoesBookPassChecks(book.BookNumber)).Return(true);
				var list = new ParatextUsxBookList {{book.BookNumber, originalBundle.UsxBooksToInclude.Single(), "checksum", true}};
				scrTextWrapper.Stub(w => w.UsxDocumentsForIncludedBooks).Return(list);
				scrTextWrapper.Stub(w => w.Stylesheet).Return(new TestStylesheet());

				var updatedProject = testProject.UpdateProjectFromParatextData(scrTextWrapper);

				updatedProject.QuoteParseCompleted += delegate
				{
					complete = true;
				};

				do
				{
					Thread.Sleep(100);
				} while (!complete);

				var userConfirmedBlocksAfterReapplying = updatedProject.IncludedBooks.First().GetScriptBlocks().Where(b => b.UserConfirmed).ToList();
				Assert.AreEqual(origCountOfUserConfirmedBlocks, userConfirmedBlocksAfterReapplying.Count);
				foreach (var blockWithReappliedUserDecision in userConfirmedBlocksAfterReapplying)
				{
					Assert.AreEqual(character, blockWithReappliedUserDecision.CharacterId);
					Assert.AreEqual("foamy", blockWithReappliedUserDecision.Delivery);
				}
			}
			finally
			{
				originalBundleAndFile.Item2.Dispose();
			}
		}

		[Test]
		public void IsVoiceActorAssignmentsComplete_Complete_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new GlyssenEngine.VoiceActor.VoiceActor();
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

			var actor = new GlyssenEngine.VoiceActor.VoiceActor();
			project.VoiceActorList.AllActors.Add(actor);
			var group = new CharacterGroup(project);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.False(project.IsVoiceActorAssignmentsComplete);
		}

		[Test]
		public void EveryAssignedGroupHasACharacter_EveryAssignedGroupHasACharacter_ReturnsTrue()
		{
			var project = TestProject.CreateBasicTestProject();

			var actor = new GlyssenEngine.VoiceActor.VoiceActor();
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

			var actor = new GlyssenEngine.VoiceActor.VoiceActor();
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

			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor();
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

			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1 };
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

			var actor1 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 0 };
			project.VoiceActorList.AllActors.Add(actor1);
			var actor2 = new GlyssenEngine.VoiceActor.VoiceActor { Id = 1, IsInactive = true };
			project.VoiceActorList.AllActors.Add(actor2);
			var group = new CharacterGroup(project);
			group.AssignVoiceActor(actor1.Id);
			project.CharacterGroupList.CharacterGroups.Add(group);

			Assert.True(project.CharacterGroupList.AnyVoiceActorAssigned());
			Assert.False(project.UnusedActors.Any());
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_Level1Only_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_2Levels_NoContinuer_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", null, 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", null, 2, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_2Levels_Continuer_ModifiesLevel2Continuer()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_3Levels_Continuer_ModifiesLevel2AndLevel3Continuers()
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

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_3Levels_AlreadyFullySpecified_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", "<< <", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< < <<", 3, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[TestCase(QuotationParagraphContinueType.Innermost, "<", "<<")]
		[TestCase(QuotationParagraphContinueType.Outermost, "<<", "<<")]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_3Levels_NonCompundingContinuers_NoChange(QuotationParagraphContinueType type,
			string level2Cont, string level3Cont)
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<", ">", level2Cont, 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", level3Cont, 3, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationParagraphContinueType = type;
			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_3Levels_AllTheSame_ModifiesLevel2AndLevel3Continuers()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<<", 3, QuotationMarkingSystemType.Normal)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			var expected = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< <<", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("<<", ">>", "<< << <<", 3, QuotationMarkingSystemType.Normal)
			};

			Assert.True(expected.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[Test]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_Level1NormalAndNarrative_NoChange()
		{
			var project = TestProject.CreateBasicTestProject();
			var quotationMarks = new List<QuotationMark>
			{
				new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("--", null, null, 1, QuotationMarkingSystemType.Narrative)
			};

			project.WritingSystem.QuotationMarks.Clear();
			project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

			project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);

			Assert.True(quotationMarks.SequenceEqual(project.WritingSystem.QuotationMarks));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void SetWsQuotationMarksUsingFullySpecifiedContinuers_3LevelsPlusNarrative_Continuer_ModifiesLevel2AndLevel3Continuers(bool fromExternalList)
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
			if (fromExternalList)
			{
				project.SetWsQuotationMarksUsingFullySpecifiedContinuers(quotationMarks);
			}
			else
			{
				project.WritingSystem.QuotationMarks.AddRange(quotationMarks);

				project.SetWsQuotationMarksUsingFullySpecifiedContinuers(project.WritingSystem.QuotationMarks);
			}

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
		[Timeout(9000)]
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
					"If \"ach\" is not found in the global cache, the language subtag will be considered \"private-use\" and the " +
					"ISO code will be null");
				Assert.AreEqual("ach", project.WritingSystem.Language.Code);
			}
			finally
			{
				Sldr.Cleanup();
			}
		}

		[Test]
		[Timeout(8000)]
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

		[Test, Timeout(10000)]
		public void Constructor_CreateNewProjectFromBundle_BundleHasNoLdmlFile_WsLdmlHasCountrySpecified_ProjectIsCreatedSuccessfully()
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

		[Test, Timeout(10000)]
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
		[Timeout(8000)]
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
		[Timeout(8000)]
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
				var testProjFolder = Path.Combine(GlyssenInfo.BaseDataFolder, "~~funkyFrogLipsAndStuff");
				if (Directory.Exists(testProjFolder))
					RobustIO.DeleteDirectoryAndContents(testProjFolder);
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

		[Test]
		public void SetCharacterGroupGenerationPreferencesToValidValues_NarrationByAuthorValueIsZero_SetToDefault()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = 0;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(1, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(0, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[TestCase(1, 1)]
		[TestCase(2, 2)]
		[TestCase(3, 2)]
		public void SetCharacterGroupGenerationPreferencesToValidValues_NarrationByAuthor_CappedAtActualNumberOfAuthors(int numMaleNarrators, int expected)
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT);
			testProject.CharacterGroupGenerationPreferences.NarratorsOption = NarratorsOption.NarrationByAuthor;
			testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators = numMaleNarrators;
			testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators = 0;

			testProject.SetCharacterGroupGenerationPreferencesToValidValues();
			Assert.AreEqual(expected, testProject.CharacterGroupGenerationPreferences.NumberOfMaleNarrators);
			Assert.AreEqual(0, testProject.CharacterGroupGenerationPreferences.NumberOfFemaleNarrators);
		}

		[Test]
		public void SetReferenceText_ChangeFromEnglishToFrench_MatchedBlocksGetMigrated()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			testProject.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var mark = testProject.IncludedBooks[0];
			var blocks = mark.GetScriptBlocks();

			// Case where the vern blocks match 1-for-1 to the English reference text
			var mark8V5 = blocks.IndexOf(b => b.ChapterNumber == 8 && b.InitialStartVerseNumber == 5);
			var matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(mark, mark8V5);
			Assert.AreEqual(4, matchup.CorrelatedBlocks.Count);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.ReferenceBlocks.Count == 1));
			matchup.MatchAllBlocks(null);
			matchup.Apply();
			var matchedVernBlocks = blocks.Skip(mark8V5).Take(4).ToList();
			Assert.IsTrue(matchedVernBlocks.All(b => b.MatchesReferenceText));
			Assert.IsTrue(matchedVernBlocks.All(b => b.ReferenceBlocks.Single().ReferenceBlocks.Count == 0));
			Assert.IsFalse(matchedVernBlocks.Any(b => string.IsNullOrEmpty(b.GetPrimaryReferenceText())));

			// Case where two of the English reference text blocks get combined to match a vern block
			var mark9V9 = blocks.IndexOf(b => b.ChapterNumber == 9 && b.InitialStartVerseNumber == 9);
			var englishRefBlocks = testProject.ReferenceText.Books.Single(b => b.BookId == "MRK").GetScriptBlocks();
			var mark9V9EnglishRefText = englishRefBlocks.IndexOf(b => b.ChapterNumber == 9 && b.InitialStartVerseNumber == 9);
			Assert.AreEqual(9, englishRefBlocks[mark9V9EnglishRefText + 1].InitialStartVerseNumber);
			matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(mark, mark9V9);
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			Assert.IsTrue(matchup.CorrelatedBlocks.All(b => b.ReferenceBlocks.Count == 1));
			var expectedEnglishRefTextForMark9V9 = englishRefBlocks[mark9V9EnglishRefText].GetText(true) + " " +
				englishRefBlocks[mark9V9EnglishRefText + 1].GetText(true);
			Assert.AreEqual(expectedEnglishRefTextForMark9V9, matchup.CorrelatedBlocks[0].GetPrimaryReferenceText());
			matchup.MatchAllBlocks(null);
			matchup.Apply();
			matchedVernBlocks = blocks.Skip(mark9V9).Take(3).ToList();
			Assert.IsTrue(matchedVernBlocks.All(b => b.MatchesReferenceText));
			Assert.IsTrue(matchedVernBlocks.All(b => b.ReferenceBlocks.Single().ReferenceBlocks.Count == 0));
			Assert.IsFalse(matchedVernBlocks.Any(b => string.IsNullOrEmpty(b.GetPrimaryReferenceText())));

			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			testProject.ReferenceText = rtFrench;

			var frenchRefBlocks = rtFrench.Books.Single(b => b.BookId == "MRK").GetScriptBlocks();

			// Verify results for case where the vern blocks match 1-for-1 to the English reference text
			matchedVernBlocks = blocks.Skip(mark8V5).Take(4).ToList();
			Assert.IsTrue(matchedVernBlocks.All(b => b.MatchesReferenceText));
			Assert.IsFalse(matchedVernBlocks.Any(b => string.IsNullOrEmpty(b.GetPrimaryReferenceText())));
			Assert.IsTrue(matchedVernBlocks.All(b => b.ReferenceBlocks.Single().ReferenceBlocks.Count == 1));
			Assert.IsFalse(matchedVernBlocks.All(b => string.IsNullOrEmpty(b.ReferenceBlocks.Single().GetPrimaryReferenceText())));
			Assert.IsTrue(matchedVernBlocks.All(b => frenchRefBlocks.Any(fb => fb.GetText(true) == b.GetPrimaryReferenceText() &&
			fb.ChapterNumber == b.ChapterNumber && fb.InitialVerseNumberOrBridge == b.InitialVerseNumberOrBridge &&
			b.ReferenceBlocks.Single().GetPrimaryReferenceText() == fb.GetPrimaryReferenceText())));

			// Verify results for case where two of the English reference text blocks get combined to match a vern block
			matchedVernBlocks = blocks.Skip(mark9V9).Take(3).ToList();
			Assert.IsTrue(matchedVernBlocks.All(b => b.MatchesReferenceText));
			Assert.IsFalse(matchedVernBlocks.Any(b => string.IsNullOrEmpty(b.GetPrimaryReferenceText())));
			Assert.IsTrue(matchedVernBlocks.All(b => b.ReferenceBlocks.Single().ReferenceBlocks.Count == 1));
			Assert.IsFalse(matchedVernBlocks.All(b => string.IsNullOrEmpty(b.ReferenceBlocks.Single().GetPrimaryReferenceText())));
			var mark9V9FrenchRefText = frenchRefBlocks.IndexOf(b => b.ChapterNumber == 9 && b.InitialStartVerseNumber == 9);
			Assert.AreEqual(frenchRefBlocks[mark9V9FrenchRefText].GetText(true) + " " + frenchRefBlocks[mark9V9FrenchRefText + 1].GetText(true),
				matchedVernBlocks[0].GetPrimaryReferenceText());
			Assert.AreEqual(expectedEnglishRefTextForMark9V9, matchedVernBlocks[0].ReferenceBlocks.Single().GetPrimaryReferenceText());
			Assert.IsTrue(matchedVernBlocks.Skip(1).All(b => frenchRefBlocks.Any(fb => fb.GetText(true) == b.GetPrimaryReferenceText() &&
			fb.ChapterNumber == b.ChapterNumber && fb.InitialVerseNumberOrBridge == b.InitialVerseNumberOrBridge &&
			b.ReferenceBlocks.Single().GetPrimaryReferenceText() == fb.GetPrimaryReferenceText())));
		}

		[Test]
		[Timeout(8000)]
		public void SetReferenceText_ChangeFromEnglishToFrenchWithOneBlockMismatched_ReferenceTextClearedForAllRelatedBlocks()
		{
			// Setup
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			testProject.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			testProject.IsOkayToClearExistingRefBlocksWhenChangingReferenceText = () => true;
			var mark = testProject.IncludedBooks[0];
			var blocks = mark.GetScriptBlocks();

			var mark5V41 = blocks.IndexOf(b => b.ChapterNumber == 5 && b.InitialStartVerseNumber == 41);
			var matchup = testProject.ReferenceText.GetBlocksForVerseMatchedToReferenceText(mark, mark5V41);
			Assert.AreEqual(5, matchup.CorrelatedBlocks.Count, "Setup problem");
			Assert.AreEqual(40, matchup.CorrelatedBlocks[0].InitialStartVerseNumber, "Setup problem");
			Assert.AreEqual(41, matchup.CorrelatedBlocks[1].InitialStartVerseNumber, "Setup problem");
			Assert.IsTrue(matchup.CorrelatedBlocks.Take(4).All(b => b.MatchesReferenceText),
				"Setup problem: GetBlocksForVerseMatchedToReferenceText expected to match all except the last " +
				"vern block to exactly one ref block.");
			Assert.IsFalse(matchup.CorrelatedBlocks.Last().MatchesReferenceText);
			var englishTextOfLastNarrtorBlock = ((ScriptText)matchup.CorrelatedBlocks[3].ReferenceBlocks.Single().BlockElements.Single()).Content;
			var iQuoteMark = englishTextOfLastNarrtorBlock.IndexOf("“");
			matchup.SetReferenceText(3, "This is not going to match the corresponding English text in the French test reference text.");
			matchup.SetReferenceText(4, englishTextOfLastNarrtorBlock.Substring(iQuoteMark));
			matchup.CorrelatedBlocks.Last().SetNonDramaticCharacterId(mark.NarratorCharacterId);
			matchup.MatchAllBlocks(null);
			matchup.Apply();
			var matchedVernBlocks = blocks.Skip(mark5V41).Take(4).ToList();
			Assert.IsTrue(matchedVernBlocks.All(b => b.MatchesReferenceText));
			Assert.IsTrue(matchedVernBlocks.All(b => b.ReferenceBlocks.Single().ReferenceBlocks.Count == 0));
			Assert.IsFalse(matchedVernBlocks.Any(b => string.IsNullOrEmpty(b.GetPrimaryReferenceText())));

			// SUT
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			testProject.ReferenceText = rtFrench;

			// Verification
			Assert.IsTrue(blocks.Single(b => b.ChapterNumber == 5 && b.InitialStartVerseNumber == 40).MatchesReferenceText);
			mark5V41 = blocks.IndexOf(b => b.ChapterNumber == 5 && b.InitialStartVerseNumber == 41);
			var vernBlocksForMark5V41 = blocks.Skip(mark5V41).Take(4).ToList();
			Assert.IsFalse(vernBlocksForMark5V41.Any(b => b.MatchesReferenceText));
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
		public void Name_GetDefaultRecordingProjectName_SetCorrectly()
		{
			var project = TestProject.CreateBasicTestProject();
			Assert.AreEqual(project.Id + " Audio", project.Name);
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
		[Timeout(8000)]
		public void CalculateSpeechDistributionScore_BoazInProjectThatOnlyIncludesRuth_ReturnsResultFromMaxBook()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);
			TestProject.SimulateDisambiguationForAllBooks(testProject);
			Assert.IsTrue(testProject.SpeechDistributionScoreByCharacterId["Boaz"] >= 7);
		}

		private static void SetAndConfirmCharacterAndDeliveryForAllCorrelatedBlocks(BlockMatchup matchup, int bookNumber, Project testProject, string character, string delivery = "foamy")
		{
			foreach (var block in matchup.CorrelatedBlocks)
			{
				block.SetCharacterIdAndCharacterIdInScript(character, bookNumber, testProject.Versification);
				block.Delivery = delivery;
				if (!ControlCharacterVerseData.Singleton.GetCharacters(bookNumber, block.ChapterNumber, block.AllVerses, testProject.Versification)
					.Any(c => c.Character == character))
				{
					testProject.ProjectCharacterVerseData.AddEntriesFor(bookNumber, block);
				}

				block.UserConfirmed = true;
			}
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
