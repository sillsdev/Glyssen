using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Shared;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Scripture;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class AssignCharacterViewModelTests
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private bool m_fullProjectRefreshRequired;
		private int m_assigned;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.ClearAssignCharacterStatus(); //Otherwise tests interfere with each other in an undesirable way
			m_model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, m_testProject.Status.AssignCharacterBlock);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
			m_model.BackwardContextBlockCount = 10;
			m_model.ForwardContextBlockCount = 10;
			m_model.AssignedBlocksIncremented += HandleAssignedBlocksIncremented;
		}

		private void HandleAssignedBlocksIncremented(AssignCharacterViewModel sender, int increment)
		{
			m_assigned += increment;
		}

		[TearDown]
		public void Teardown()
		{
			if (m_fullProjectRefreshRequired)
			{
				TestFixtureTearDown();
				TestFixtureSetUp();
				m_fullProjectRefreshRequired = false;
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void Constructor_FirstQuoteIsUnexpected_FirstUnexpectedBlockLoaded()
		{
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(1, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(2, m_model.CurrentBlock.InitialStartVerseNumber);
		}

		[Test]
		public void Narrator_CurrentBookIsMark_ToStringIncludesBookName()
		{
			Assert.AreEqual("narrator (MRK)", AssignCharacterViewModel.Character.Narrator.ToString());
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWithNoContext_GetsNarratorOnly()
		{
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.AreEqual(1, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReference_NonQuoteBlockContainingVerseWitExpectedQuote_GetsNarratorFollowedByOtherSpeakersInVersesInBlock()
		{
			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;
			Assert.AreEqual("MRK 1:16-17", m_model.GetBlockReferenceString());
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(2, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWithContext_GetsNarratorOnly()
		{
			// Note: Default forward/backward context is 10 blocks.
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(1, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWith20BlockForwardContext_GetsNarratorAndJesus()
		{
			m_model.ForwardContextBlockCount = 20;
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(2, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
			Assert.IsTrue(characters[1].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReference_AmbiguousQuote_GetsBothCharactersPlusNarrator()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(3, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
			Assert.IsFalse(characters[1].ProjectSpecific);
			Assert.AreEqual("demons (Legion)/man delivered from Legion of demons", characters[2].CharacterId);
			Assert.IsFalse(characters[2].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReferenceTextMatchup_Works()
		{
			FindRefInMark(9, 21);
			m_model.AttemptRefBlockMatchup = true;

			var result = m_model.GetCharactersForCurrentReferenceTextMatchup().ToList();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].IsNarrator);
			Assert.AreEqual("Jesus", result[1].CharacterId);
			Assert.AreEqual("father of demon-possessed boy", result[2].CharacterId);
		}

		[Test]
		public void GetDeliverieForCurrentReferenceTextMatchup_CannedDeliveries_GetsNormalPlusDeliveriesForCoveredVerses()
		{
			FindRefInMark(9, 21);
			m_model.AttemptRefBlockMatchup = true;

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].IsNormal);
			Assert.AreEqual(1, result.Count(d => d.Text == "questioning"));
			Assert.AreEqual(1, result.Count(d => d.Text == "distraught"));
		}

		[Test]
		public void GetDeliverieForCurrentReferenceTextMatchup_BlockSetToProjectSpecificDelivery_ResultIncludesProjectSpecificDelivery()
		{
			FindRefInMark(10, 49);
			m_model.SetCharacterAndDelivery(m_model.GetUniqueCharactersForCurrentReference().First(c => c.CharacterId == "Jesus"),
				new AssignCharacterViewModel.Delivery("ordering"));
			m_model.AttemptRefBlockMatchup = true;

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].IsNormal);
			Assert.AreEqual(1, result.Count(d => d.Text == "encouraging"));
			Assert.AreEqual(1, result.Count(d => d.Text == "ordering"));
		}

		[Test]
		public void GetUniqueCharacters_AmbiguousQuoteNoFilter_GetsAllCharactersInMark()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharacters().ToList();
			Assert.AreEqual(80, characters.Count);
			Assert.IsTrue(characters.Any(c => c.IsNarrator));
			Assert.IsTrue(characters.Any(c => c.CharacterId == "Jesus"));
			Assert.IsTrue(characters.Any(c => c.CharacterId == "demons (Legion)/man delivered from Legion of demons"));
		}

		[Test]
		public void GetUniqueCharacters_AmbiguousQuoteFilter_GetsFilterCharactersAndReferenceCharacters()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharacters("zeru").ToList();
			Assert.AreEqual(4, characters.Count);
			Assert.AreEqual("Zerubbabel/Jeshua/rest of heads of families", characters[0].CharacterId);
			Assert.IsTrue(characters.Any(c => c.CharacterId == "Jesus"));
			Assert.IsTrue(characters.Any(c => c.CharacterId == "demons (Legion)/man delivered from Legion of demons"));
			Assert.IsTrue(characters[3].IsNarrator);
		}

		[Test]
		public void GetCharactersForCurrentReference_AmbiguousQuote_SortByAlias()
		{
			FindRefInMark(6, 24);
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(3, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.AreEqual("Herodias' daughter", characters[1].CharacterId);
			Assert.AreEqual("alias", characters[1].Alias);
			Assert.AreEqual("Herodias", characters[2].CharacterId);
			Assert.IsNull(characters[2].Alias);
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsTrue(string delivery)
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = delivery};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsFalse()
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = "encouraging"};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsTrue()
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = "mumbling"};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsTrue(string delivery)
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = delivery};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[TestCase("exasperated")]
		[TestCase("giving orders")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = delivery};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsTrue()
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = "mumbling"};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasOnlyUnspecifiedDelivery_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 9, 23) {CharacterId = "Jesus", Delivery = delivery};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasOnlyUnspecifiedDelivery_ReturnsTrue()
		{
			var block = new Block("p", 9, 23) {CharacterId = "Jesus", Delivery = "artful"};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		// Note that a line had to be added to TestCharacterVerse for the following three test cases because there was no example of it.
		// As of now, the production version of the file doesn't have anything like this either, but it is a theoretical possibility,
		// so I felt I should text for it.
		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = delivery};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasHasMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsFalse()
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = "suspicious"};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasHasNonMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsTrue()
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = "violent"};
			Assert.IsTrue(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[TestCase(10, 49, null)]
		[TestCase(9, 19, "")]
		[TestCase(9, 23, null)]
		[TestCase(11, 5, "")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_NarratorBlock_ReturnsFalse(int chapter, int verse, string delivery)
		{
			var block = new Block("p", chapter, verse)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator),
				Delivery = delivery
			};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void SetMode_AlternateBetweenModes_AssignedBlockCountDoesNotGrowContinuously()
		{
			Assert.AreEqual(0, m_model.CompletedBlockCount);

			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Jesus"), AssignCharacterViewModel.Delivery.Normal);

			Assert.AreEqual(1, m_model.CompletedBlockCount);

			m_model.Mode = BlocksToDisplay.NotYetAssigned;
			Assert.AreEqual(0, m_model.CompletedBlockCount);

			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			Assert.AreEqual(1, m_model.CompletedBlockCount);

			// The assignment call above actually affects 5 blocks because they are all in the same quote.
			m_model.Mode = BlocksToDisplay.AllScripture;
			Assert.AreEqual(1, m_model.CompletedBlockCount);

			m_model.Mode = BlocksToDisplay.HotSpots | BlocksToDisplay.ExcludeUserConfirmed;
			Assert.AreEqual(0, m_model.CompletedBlockCount);
		}

		[Test]
		public void GetDeliveriesForCharacter_NullCharacter_GetsEmptyEnumeration()
		{
			Assert.False(m_model.GetDeliveriesForCharacter(null).Any());
		}

		[Test]
		public void GetDeliveriesForCharacter_CharacterWithNoDeliveries_GetsOnlyNormalDelivery()
		{
			FindRefInMark(5, 9);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("man with evil spirit")).ToArray();
			Assert.AreEqual(1, deliveries.Count());
			Assert.AreEqual(AssignCharacterViewModel.Delivery.Normal, deliveries.First());
		}

		[Test]
		public void GetDeliveriesForCharacter_CharacterWithOneDelivery_GetsDeliveryAndNormal()
		{
			FindRefInMark(5, 9);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("Jesus")).ToArray();
			Assert.AreEqual(2, deliveries.Count());
			Assert.Contains(new AssignCharacterViewModel.Delivery("questioning"), deliveries.ToList());
			Assert.Contains(AssignCharacterViewModel.Delivery.Normal, deliveries.ToList());
		}

		[Test]
		public void GetUniqueDeliveries_NoFilterText_ReturnsAll()
		{
			var uniqueDeliveries = m_model.GetUniqueDeliveries();
			Assert.AreEqual(257, uniqueDeliveries.Count());
		}

		[Test]
		public void GetUniqueDeliveries_FilterText_ReturnsDeliveriesWithFilterText()
		{
			var uniqueDeliveries = m_model.GetUniqueDeliveries("amazed");
			Assert.AreEqual(2, uniqueDeliveries.Count());
		}

		[Test]
		public void GetUniqueDeliveries_HasCurrentDeliveries_NoFilterText_ReturnsAll()
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			FindRefInMark(5, 7);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("demons (Legion)/man delivered from Legion of demons"));
			Assert.AreEqual(2, deliveries.Count());
			var uniqueDeliveries = m_model.GetUniqueDeliveries();
			Assert.AreEqual(258, uniqueDeliveries.Count());
		}

		[Test]
		public void GetUniqueDeliveries_HasCurrentDeliveries_FilterText_ReturnsDeliveriesWithFilterText()
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			FindRefInMark(5, 7);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("demons (Legion)/man delivered from Legion of demons"));
			Assert.AreEqual(2, deliveries.Count());
			var uniqueDeliveries = m_model.GetUniqueDeliveries("shrieking");
			Assert.AreEqual(2, uniqueDeliveries.Count());
		}

		[Test]
		public void IsModified_NormalDeliveryNoChange_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = null;
			Assert.IsFalse(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal));
			m_model.CurrentBlock.Delivery = string.Empty;
			Assert.IsFalse(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal));
		}

		[Test]
		public void IsModified_CharacterChanged_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.Delivery = null;
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character("Ferdinand"), AssignCharacterViewModel.Delivery.Normal));
		}

		[Test]
		public void IsModified_DeliveryChangedToNormal_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = "annoyed";
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal));
		}

		[Test]
		public void IsModified_DeliveryChangedFromNormal_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = null;
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), new AssignCharacterViewModel.Delivery("peeved")));
		}

		[Test]
		public void IsModified_BlockCharacterAndDeliveryNotSetCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = null;
			block1.Delivery = null;
			Assert.IsFalse(m_model.IsModified(null, null));
		}

		[Test]
		public void IsModified_BlockCharacterAmbiguousCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = CharacterVerseData.kAmbiguousCharacter;
			Assert.IsFalse(m_model.IsModified(null, null));
		}

		[Test]
		public void IsModified_BlockCharacterUnknownCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = CharacterVerseData.kUnexpectedCharacter;
			Assert.IsFalse(m_model.IsModified(null, null));
		}

		[Test]
		public void IsModified_BlockCharacterAndDeliverySetCharacterNull_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "Abram";
			block1.Delivery = "frenetic";
			Assert.IsTrue(m_model.IsModified(null, null));
		}

		[Test]
		public void IsModified_CharacterUnchangedDeliveryNull_ReturnsTrue()
		{
			// NOTE: This scenario should not be possible via the UI.
			var block1 = m_model.CurrentBlock;
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), null));
		}

		[Test]
		public void IsModified_CharacterChangedDeliveryNull_ReturnsTrue()
		{
			m_model.CurrentBlock.Delivery = null;
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character("Ralph W Emerson"), null));
		}

		[Test]
		public void IsModified_StandardCharacter_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "extra-MRK";
			block1.Delivery = "foo";
			Assert.IsFalse(m_model.IsModified(new AssignCharacterViewModel.Character("extra-MRK"), null));
		}

		[Test]
		public void IsModified_ChangeToNarrator_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "Theodore";
			block1.Delivery = null;
			Assert.IsTrue(m_model.IsModified(new AssignCharacterViewModel.Character("narrator-MRK"), null));
		}

		[Test]
		public void IsModified_SameNarrator_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "narrator-MRK";
			block1.Delivery = null;
			Assert.IsFalse(m_model.IsModified(new AssignCharacterViewModel.Character("narrator-MRK"), AssignCharacterViewModel.Delivery.Normal));
		}

		[Test]
		public void SetCurrentBookSingleVoice_SetTrueFromTrue_IsTrue()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.IsCurrentBookSingleVoice);
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.IsCurrentBookSingleVoice);
		}

		[Test]
		public void SetCurrentBookSingleVoice_SetFalseFromTrue_IsFalse()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.IsCurrentBookSingleVoice);
			m_model.SetCurrentBookSingleVoice(false);
			Assert.IsFalse(m_model.IsCurrentBookSingleVoice);
		}

		[Test]
		public void SetCurrentBookSingleVoice_TrueNoSubsequentBooks_CurrentBlockIsUnchanged()
		{
			m_fullProjectRefreshRequired = true;
			var currentBlock = m_model.CurrentBlock;
			m_model.SetCurrentBookSingleVoice(true);

			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			m_model.Mode = BlocksToDisplay.NotYetAssigned;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void AreAllAssignmentsComplete_OnlyBookSetToSingleVoice_ValueChangesFromFalseToTrue()
		{
			m_fullProjectRefreshRequired = true;
			Assert.IsFalse(m_model.IsCurrentTaskComplete);
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.IsCurrentTaskComplete);
		}

		[Test]
		public void SplitBlock_SingleSplit_RelevantBlocksUpdated()
		{
			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			var currentBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextNextBlock = m_model.CurrentBlock;
			m_model.LoadPreviousRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			var preSplit = currentBlock.Clone();

			// List<KeyValuePair<int, string>> characters, Block currentBlock

			m_model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, "2", 6)}, GetListOfCharacters(2, new string[0]));
			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			var splitPartA = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartB = m_model.CurrentBlock;
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.AreEqual(6, partALength);
			Assert.AreEqual(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				partALength + splitPartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length));
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextBlock, m_model.CurrentBlock);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextNextBlock, m_model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_SingleSplitWithMultipleBooksInProject_RelevantBlocksUpdated()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);

			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			var currentBlock = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var nextBlock = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var nextNextBlock = model.CurrentBlock;
			model.LoadPreviousRelevantBlock();
			model.LoadPreviousRelevantBlock();
			Assert.AreEqual(currentBlock, model.CurrentBlock);
			var preSplit = currentBlock.Clone();

			model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, "2", 6)}, GetListOfCharacters(2, new string[0]));
			var splitPartA = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var splitPartB = model.CurrentBlock;
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.AreEqual(6, partALength);
			Assert.AreEqual(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				partALength + splitPartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length));
			model.LoadNextRelevantBlock();
			Assert.AreEqual(nextBlock, model.CurrentBlock);
			model.LoadNextRelevantBlock();
			Assert.AreEqual(nextNextBlock, model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_MultipleSplitsInOneBlock_RelevantBlocksUpdated()
		{
			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			while (m_model.CurrentBlock.InitialStartVerseNumber == m_model.CurrentBlock.LastVerseNum)
				m_model.LoadNextRelevantBlock();
			var currentBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextNextBlock = m_model.CurrentBlock;
			m_model.LoadPreviousRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			var preSplit = currentBlock.Clone();

			Assert.AreEqual(7, currentBlock.InitialStartVerseNumber, "If this fails, update the test to reflect the test data.");

			m_model.SplitBlock(new[]
			{
				// The order here is significant as we need to be able to handle them "out of order" like this
				new BlockSplitData(1, currentBlock, "7", 6),
				new BlockSplitData(5, currentBlock, "8", 3),
				new BlockSplitData(4, currentBlock, "7", BookScript.kSplitAtEndOfVerse),
				new BlockSplitData(2, currentBlock, "7", 11),
				new BlockSplitData(3, currentBlock, "7", 2)
			}, GetListOfCharacters(6, new string[0]));

			var splitPartA = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartB = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartC = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartD = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartE = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartF = m_model.CurrentBlock;
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var partBLength = splitPartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var partCLength = splitPartC.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var partDLength = splitPartD.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var partELength = splitPartE.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var partFLength = splitPartF.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.AreEqual(2, partALength);
			Assert.AreEqual(4, partBLength);
			Assert.AreEqual(5, partCLength);
			Assert.AreEqual(preSplit.BlockElements.OfType<ScriptText>().First().Content.Length - 11, partDLength);
			Assert.AreEqual(3, partELength);
			Assert.AreEqual(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				partALength + partBLength + partCLength + partDLength + partELength + partFLength);

			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextBlock, m_model.CurrentBlock);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextNextBlock, m_model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_SplitsInMultipleBlocks_RelevantBlocksUpdated()
		{
			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			var currentBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextBlock = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var nextNextBlock = m_model.CurrentBlock;
			m_model.LoadPreviousRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			var preSplit1 = currentBlock.Clone();
			var preSplit2 = nextBlock.Clone();

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, currentBlock, "2", 6),
				new BlockSplitData(2, nextBlock, "4", 8),
			}, GetListOfCharacters(3, new string[0]));

			var split1PartA = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var split1PartB = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var split2PartA = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var split2PartB = m_model.CurrentBlock;
			var part1ALength = split1PartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var part1BLength = split1PartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var part2ALength = split2PartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			var part2BLength = split2PartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.AreEqual(6, part1ALength);
			Assert.AreEqual(8, part2ALength);
			Assert.AreEqual(preSplit1.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				part1ALength + part1BLength);
			Assert.AreEqual(preSplit2.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				part2ALength + part2BLength);

			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextNextBlock, m_model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_DoubleSplitInBlockWhichIsNotRelevant_NewBlocksAllNeedAssignments()
		{
			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			FindRefInMark(1, 7);
			Block firstRelevantBlockAfterTheBlockToSplit = m_model.CurrentBlock;
			m_model.CurrentBlockIndexInBook = m_model.CurrentBlockIndexInBook - 2;
			var blockToSplit = m_model.CurrentBlock;
			var currentBlockCharacterId = blockToSplit.CharacterId;
			Assert.True(blockToSplit.ChapterNumber == 1 && blockToSplit.InitialStartVerseNumber == 5);
			Assert.False(m_model.IsCurrentBlockRelevant);

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "5", 6),
				new BlockSplitData(2, blockToSplit, "5", 9)
			}, GetListOfCharacters(3, new[] {currentBlockCharacterId}));

			Assert.False(m_model.IsCurrentBlockRelevant);
			Assert.AreEqual(currentBlockCharacterId, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.True(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.True(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(firstRelevantBlockAfterTheBlockToSplit, m_model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_DoesNotThrow()
		{
			m_fullProjectRefreshRequired = true;

			m_model.CurrentBlockIndexInBook = 4;
			var block1 = m_testProject.Books[0].Blocks[4];

			Assert.AreEqual(block1, m_model.CurrentBlock);
			var block2 = m_testProject.Books[0].Blocks[5];

			Assert.AreEqual(2, block1.InitialStartVerseNumber, "If this fails, update the test to reflect the test data.");

			Assert.DoesNotThrow(() =>
					m_model.SplitBlock(new[]
					{
						new BlockSplitData(1, block1, "2", 13),
						new BlockSplitData(2, block2, null, 0),
						new BlockSplitData(3, block2, "3", 10)
					}, GetListOfCharacters(4, new string[0]))
			);
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_SplitCorrectly()
		{
			m_fullProjectRefreshRequired = true;

			// Find some place where we have a long run of continuation blocks, where the first two are in the
			// same vere but the next one starts a new verse.
			int i = 0;
			for (; i < m_testProject.Books[0].Blocks.Count - 4; i++)
			{
				if (m_testProject.Books[0].Blocks[i].MultiBlockQuote == MultiBlockQuote.Start &&
					m_testProject.Books[0].Blocks[i + 1].MultiBlockQuote == MultiBlockQuote.Continuation &&
					!m_testProject.Books[0].Blocks[i + 1].StartsAtVerseStart &&
					m_testProject.Books[0].Blocks[i + 2].MultiBlockQuote == MultiBlockQuote.Continuation &&
					m_testProject.Books[0].Blocks[i + 3].MultiBlockQuote == MultiBlockQuote.Continuation &&
					m_testProject.Books[0].Blocks[i + 2].StartsAtVerseStart)
				{
					break;
				}
			}

			m_model.CurrentBlockIndexInBook = i;
			var block1 = m_model.CurrentBlock;
			var block2 = m_testProject.Books[0].Blocks[i + 1];

			var text1 = block1.GetText(false);
			var text2 = block2.GetText(false);

			// make sure the blocks we found have enough text to accommodate our (arbitrary) character offsets where we split the text.
			var block1StartVerse = block1.InitialStartVerseNumber;
			Assert.True(block1.BlockElements.OfType<ScriptText>().First().Content.Length > 13);
			Assert.True(block2.BlockElements.OfType<ScriptText>().First().Content.Length > 10);

			// Now run the SUT

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, block1, block1StartVerse.ToString(), 13),
				new BlockSplitData(2, block2, null, 0),
				new BlockSplitData(3, block2, block1StartVerse.ToString(), 10)
			}, GetListOfCharacters(4, new string[0]));

			// check the text
			Assert.AreEqual(text1.Substring(0, 13), m_testProject.Books[0].Blocks[i].GetText(false));
			Assert.AreEqual(text1.Substring(13), m_testProject.Books[0].Blocks[i+1].GetText(false));
			Assert.AreEqual(text2.Substring(0, 10), m_testProject.Books[0].Blocks[i + 2].GetText(false));
			Assert.AreEqual(text2.Substring(10), m_testProject.Books[0].Blocks[i + 3].GetText(false));

			// check the multi-block quote
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[i].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[i + 1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[i + 2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, m_testProject.Books[0].Blocks[i + 3].MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_CurrentBlockDoesNotMatchFilter_SubsequentRelevantBlockIndicesUpdated()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			var model = new AssignCharacterViewModel(project);

			// Find a block that we can split that will not be "relevant" when we filter to show only blocks with missing expected quotes.
			while (!model.CurrentBlock.BlockElements.OfType<ScriptText>().First().Content.TrimEnd().Contains(" "))
				model.LoadNextRelevantBlock();
			var currentBlock = model.CurrentBlock;
			Assert.IsTrue(model.IsCurrentBlockRelevant, "Couldn't find a block to use for this test.");
			Assert.AreEqual("MRK", model.CurrentBookId);

			var verse = currentBlock.InitialVerseNumberOrBridge;
			var splitIndex = currentBlock.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal);
			var indexOfBlockToSplit = model.CurrentBlockIndexInBook;
			Assert.IsTrue(splitIndex > 0);
			var blockTextBeforeSplit = currentBlock.GetText(true);

			model.Mode = BlocksToDisplay.MissingExpectedQuote;
			Assert.AreEqual("MRK", model.CurrentBookId, "Changing the filter should not have caused us to go to a different book.");
			model.CurrentBlockIndexInBook = indexOfBlockToSplit;
			Assert.AreEqual(currentBlock, model.CurrentBlock, "Setting the CurrentBlockIndexInBook should have moved us back to the block we intend to split.");
			Assert.IsFalse(model.IsCurrentBlockRelevant, "The block we intend to split must not be condidered \"relevant\" with the \"NeedAssignments\" filter.");

			// Now go to the next relevant block in this same book and rememeber which block it is. After splitting, going to the next block
			// should still take us to this same block.
			model.LoadNextRelevantBlock();
			var nextBlock = model.CurrentBlock;
			Assert.AreEqual("MRK", model.CurrentBookId);
			var indexOfNextRelevantBlock = model.CurrentBlockIndexInBook;
			Assert.IsTrue(indexOfBlockToSplit < model.CurrentBlockIndexInBook);

			// Now go back to the block we intend to split.
			model.CurrentBlockIndexInBook = indexOfBlockToSplit;
			Assert.AreEqual(currentBlock, model.CurrentBlock);

			model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, verse, splitIndex)},
				GetListOfCharacters(2, new[] {currentBlock.CharacterId, currentBlock.CharacterId}));

			// Verify split
			var splitPartA = model.CurrentBlock;
			model.CurrentBlockIndexInBook = indexOfBlockToSplit + 1;
			var splitPartB = model.CurrentBlock;
			Assert.IsFalse(model.IsCurrentBlockRelevant, "The second part of the split block should not be condidered \"relevant\" with the \"NeedAssignments\" filter.");
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.AreEqual(splitIndex, partALength);
			Assert.IsTrue(blockTextBeforeSplit.StartsWith(splitPartA.GetText(true)));
			Assert.AreEqual(blockTextBeforeSplit.Length, splitPartA.GetText(true).Length + splitPartB.GetText(true).Length);
			Assert.IsTrue(blockTextBeforeSplit.EndsWith(splitPartB.GetText(true)));

			// Now make sure that LoadNextRelevantBlock still takes us to the same next relevant block as before.
			model.LoadNextRelevantBlock();
			Assert.AreEqual(nextBlock, model.CurrentBlock);
			Assert.AreEqual(indexOfNextRelevantBlock + 1, model.CurrentBlockIndexInBook);
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocks_IndicesNotChanged()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.NotYetAssigned;
			model.AttemptRefBlockMatchup = false;

			while (model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				model.LoadNextRelevantBlock();

			Assert.IsTrue(model.CanNavigateToNextRelevantBlock, "Did not find a block sufficient for testing this scenario - no subsequent relevant blocks");
			var blockToSplit = model.CurrentBlock;
			Assert.IsTrue(blockToSplit.MultiBlockQuote == MultiBlockQuote.Start, "Did not find a block sufficient for testing this scenario");

			var originalNextBlock = model.BlockAccessor.GetNextBlock();

			model.LoadNextRelevantBlock();
			var indexOfNextRelevantBlock = model.BlockAccessor.GetIndices().BlockIndex;
			model.LoadPreviousRelevantBlock();

			Assert.AreEqual(blockToSplit, model.CurrentBlock, "setup problem!");

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, blockToSplit.LastVerseNum.ToString(), BookScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new string[] { null, null }));

			// Validates our test was set up correctly
			Assert.AreEqual(originalNextBlock, model.BlockAccessor.GetNthNextBlockWithinBook(1, blockToSplit));

			model.LoadNextRelevantBlock();
			Assert.AreEqual(originalNextBlock, model.CurrentBlock);
			model.LoadNextRelevantBlock();
			Assert.AreEqual(indexOfNextRelevantBlock, model.BlockAccessor.GetIndices().BlockIndex,
				"Index of next relevant block should not have been incremented.");
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocksAndHasReferenceMatchup_IndicesNotChangedAndNoBlockAddedToBlockMatchup()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			model.AttemptRefBlockMatchup = true;

			Block blockToSplit;
			BlockMatchup matchup;
			do
			{
				model.LoadNextRelevantBlock();
				matchup = model.CurrentReferenceTextMatchup;
				blockToSplit = matchup.CountOfBlocksAddedBySplitting == 0 ? model.CurrentBlock : null;
			} while (blockToSplit == null || blockToSplit.MultiBlockQuote != MultiBlockQuote.Start);

			Assert.IsTrue(model.CanNavigateToNextRelevantBlock, "Did not find a block sufficient for testing this scenario - no subsequent relevant blocks");
			Assert.IsTrue(blockToSplit.MultiBlockQuote == MultiBlockQuote.Start, "Did not find a block sufficient for testing this scenario");

			var originalNextBlock = model.BlockAccessor.GetNextBlock();
			var origOriginalBlockCount = matchup.OriginalBlockCount;
			var origCorrelatedBlocks = matchup.CorrelatedBlocks.ToList();

			model.LoadNextRelevantBlock();
			var indexOfNextRelevantBlock = model.BlockAccessor.GetIndices().BlockIndex;
			model.LoadPreviousRelevantBlock();

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, blockToSplit.LastVerseNum.ToString(), BookScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new string[] { "", "" }));

			// Validates our test was set up correctly
			Assert.AreEqual(originalNextBlock, model.BlockAccessor.GetNthNextBlockWithinBook(1, blockToSplit));

			Assert.AreEqual(origOriginalBlockCount, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.IsTrue(model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(origCorrelatedBlocks.Select(b => b.GetText(true))));

			model.LoadNextRelevantBlock();

			Assert.AreEqual(indexOfNextRelevantBlock, model.BlockAccessor.GetIndices().BlockIndex,
				"Index of next relevant block should not have been incremented.");
		}

		[Test]
		public void SplitBlock_SplitWhereMatchupHasCreatedPseudoSplit_ResultingBlockShouldBeRelevant()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			model.AttemptRefBlockMatchup = true;

			// Go to MAT 5:18
			while (model.CurrentBlock.ChapterNumber != 5 || model.CurrentBlock.InitialStartVerseNumber != 18)
				model.LoadNextRelevantBlock();

			var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.First();

			// Validate our setup
			Assert.True(model.IsCurrentBlockRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum, "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.AreEqual(1, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.Greater(blockToSplit.LastVerseNum, 18);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.True(model.CanNavigateToNextRelevantBlock);

			model.LoadNextRelevantBlock();

			Assert.AreEqual(19, model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(19, model.CurrentReferenceTextMatchup.OriginalBlocks.First().InitialStartVerseNumber);
		}

		[Test]
		public void SplitBlock_SplitMidReferenceTextBlock_BlockMatchupShouldContainBothBlocksAndStillBeRelevant()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			model.AttemptRefBlockMatchup = true;

			// Go to MAT 5:18
			while (model.CurrentBlock.ChapterNumber != 5 || model.CurrentBlock.InitialStartVerseNumber != 18)
				model.LoadNextRelevantBlock();

			var origRelevantBlockCount = model.RelevantBlockCount;
			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.First();

			model.LoadNextRelevantBlock();
			var origNextRelevantBlockText = model.CurrentBlock.GetText(true);
			model.LoadPreviousRelevantBlock();

			// Validate our setup
			Assert.True(model.IsCurrentBlockRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum, "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.Greater(blockToSplit.LastVerseNum, 18);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", 10) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.AreEqual(origRelevantBlockCount, model.RelevantBlockCount);
			Assert.True(model.IsCurrentBlockRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum);
			Assert.AreEqual(2, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.AreEqual(origBlock, model.CurrentReferenceTextMatchup.OriginalBlocks.First());
			Assert.AreEqual(String.Join("", model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))), origBlockText);

			model.LoadNextRelevantBlock();
			Assert.AreEqual(origNextRelevantBlockText, model.CurrentBlock.GetText(true));
		}

		// PG-1075
		[Test]
		public void SplitBlock_MakeTwoDifferentSplitsAtVerseBoundariesAndNavigateToLaterBlock_IndicesKeptInSync()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			var model = new AssignCharacterViewModel(project);

			model.Mode = BlocksToDisplay.AllQuotes;
			model.AttemptRefBlockMatchup = true;

			// LUK 21:20
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(042021020)));

		    var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "20", PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			// LUK 21:21
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(042021021)));

			blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "21", PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			// LUK 21:25
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(042021025)));

			// Before our fix, this was throwing an IndexOutOfRangeException
			Assert.IsTrue(model.CanNavigateToNextRelevantBlock);
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseBridgeAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001012)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "12-16", 3),
				new BlockSplitData(2, blockToSplit, "17", 8)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual("Verse 1 text. Verses 2-6a text. Verses 6b-11 text. Ver", project.Books[0].Blocks[blockIndex].GetText(false));
			Assert.AreEqual("ses 12-16 text. Verse 17", project.Books[0].Blocks[blockIndex + 1].GetText(false));
			Assert.AreEqual(" text. ", project.Books[0].Blocks[blockIndex + 2].GetText(false));
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001018)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "18a", 4),
				new BlockSplitData(2, blockToSplit, "18b", 5)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual("Vers", project.Books[0].Blocks[blockIndex].GetText(false));
			Assert.AreEqual("e 18a text. Verse", project.Books[0].Blocks[blockIndex + 1].GetText(false));
			Assert.AreEqual(" 18b text. ", project.Books[0].Blocks[blockIndex + 2].GetText(false));
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseBridgeWithSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001001)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "1", 4),
				new BlockSplitData(2, blockToSplit, "2-6a", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual("Vers", project.Books[0].Blocks[blockIndex].GetText(false));
			Assert.AreEqual("e 1 text. Verses ", project.Books[0].Blocks[blockIndex + 1].GetText(false));
			Assert.AreEqual("2-6a text. Verses 6b-11 text. Verses 12-16 text. Verse 17 text. ", project.Books[0].Blocks[blockIndex + 2].GetText(false));
		}

		//PG-1081
		// Not sure if we will ever get data in this form, but testing just in case
		[Test]
		public void SplitBlock_VerseWhichContinuesIntoVerseBridgeAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001019)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "19", 4),
				new BlockSplitData(2, blockToSplit, "19-20", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual("Vers", project.Books[0].Blocks[blockIndex].GetText(false));
			Assert.AreEqual("e 19 text. Verses ", project.Books[0].Blocks[blockIndex + 1].GetText(false));
			Assert.AreEqual("19-20 text. ", project.Books[0].Blocks[blockIndex + 2].GetText(false));
		}

		//PG-1081
		// Not sure if we will ever get data in this form, but testing just in case
		[Test]
		public void SplitBlock_VerseWithAndWithoutSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001021)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "21", 4),
				new BlockSplitData(2, blockToSplit, "21b", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual("Vers", project.Books[0].Blocks[blockIndex].GetText(false));
			Assert.AreEqual("e 21 text. Verse 2", project.Books[0].Blocks[blockIndex + 1].GetText(false));
			Assert.AreEqual("1b text. ", project.Books[0].Blocks[blockIndex + 2].GetText(false));
		}

		// PG-1089
		[Test]
		public void SplitBlock_CharacterIdsAreEmptyString_CharactersSetToUnknown()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.IsTrue(model.TryLoadBlock(new VerseRef(031001021)));

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "21", 4),
				new BlockSplitData(2, blockToSplit, "21b", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, project.Books[0].Blocks[blockIndex].CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, project.Books[0].Blocks[blockIndex + 1].CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, project.Books[0].Blocks[blockIndex + 2].CharacterId);
		}

		[Test]
		public void SetCurrentBookSingleVoice_UnassignedQuotesInOtherBooks_CurrentBlockInNextBook()
		{
			var project = TestProject.CreateTestProject(
				TestProject.TestBook.JOS,
				TestProject.TestBook.MRK,
				TestProject.TestBook.LUK,
				TestProject.TestBook.ACT,
				TestProject.TestBook.GAL,
				TestProject.TestBook.EPH,
				TestProject.TestBook.PHM,
				TestProject.TestBook.HEB,
				TestProject.TestBook.IJN,
				TestProject.TestBook.IIJN,
				TestProject.TestBook.IIIJN,
				TestProject.TestBook.JUD,
				TestProject.TestBook.REV
			);

			var model = new AssignCharacterViewModel(project);
			model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
			model.BackwardContextBlockCount = 10;
			model.ForwardContextBlockCount = 10;

			Assert.True(model.TryLoadBlock(new VerseRef(042001001)));
			model.LoadNextRelevantBlock();
			Assert.AreEqual("LUK", model.CurrentBookId);

			model.SetCurrentBookSingleVoice(true);
			Assert.AreEqual("ACT", model.CurrentBookId);
		}

		[Test]
		public void SetCurrentBookSingleVoice_CurrentBlockIsResultOfBlockMatchupSplit_MatchupResetAndOriginalBlocksRestored()
		{
			FindRefInMark(9, 21);
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0);
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
			Assert.IsTrue(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_Narrator_ReturnsGenericNarratorCharacter()
		{
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003010)));
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.AreEqual(1, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.AreEqual(AssignCharacterViewModel.Character.Narrator, m_model.GetCharacterToSelectForCurrentBlock(null));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacter_ReturnsAssignedCharacter()
		{
			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003009)));
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.GetUniqueCharactersForCurrentReference(false).Any(c => c.CharacterId == "Jesus"));
			Assert.IsTrue(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)).CharacterId == "Jesus");
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacterThatIsNotInEnumeration_ThisShouldNeverHappenInRealLife_ReturnsNull()
		{
			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003009)));
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(new List<AssignCharacterViewModel.Character>()));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockIsOneOfTwoAmbiguousQuotesInVerse_ReturnsNull()
		{
			FindRefInMark(5, 9);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
			Assert.AreEqual(3, m_model.GetUniqueCharactersForCurrentReference(false).Count()); // Includes narrator
			Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfTwoDialogueQuotesInVerse_ReturnsOtherExpectedCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 9);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
			var posibleCharactersForMark59 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var posibleSpeakingCharactersForMark59 = posibleCharactersForMark59.Where(c => !c.IsNarrator).ToList();
			Assert.AreEqual(2, posibleSpeakingCharactersForMark59.Count);
			m_model.SetCharacterAndDelivery(posibleSpeakingCharactersForMark59[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			posibleSpeakingCharactersForMark59.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnknown);
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
			Assert.AreEqual(5, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(9, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(posibleSpeakingCharactersForMark59.Single(), m_model.GetCharacterToSelectForCurrentBlock(posibleCharactersForMark59));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneDialogueQuoteAndOneNarratorQuotationInVerse_ReturnsOtherCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 41);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
			var posibleCharactersForMark541 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var unusedCharactersForMark541 = posibleCharactersForMark541.ToList();
			Assert.AreEqual(2, unusedCharactersForMark541.Count);
			m_model.SetCharacterAndDelivery(unusedCharactersForMark541[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			unusedCharactersForMark541.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnknown);
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
			Assert.AreEqual(5, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(41, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(unusedCharactersForMark541.Single(), m_model.GetCharacterToSelectForCurrentBlock(posibleCharactersForMark541));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneIndirectAndOnePotentialInVerse_ReturnsNull(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			try
			{
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
				m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
				SetUp();

				while (m_model.CurrentBlock.ChapterNumber != 8 || m_model.CurrentBlock.InitialStartVerseNumber != 37)
					m_model.LoadNextRelevantBlock();
				Assert.AreEqual("ACT", m_model.CurrentBookId);
				Assert.AreEqual(8, m_model.CurrentBlock.ChapterNumber);
				Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
				var posibleCharactersForActs837 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
				Assert.AreEqual(3, posibleCharactersForActs837.Count);
				m_model.SetCharacterAndDelivery(posibleCharactersForActs837.Where(c => !c.IsNarrator).ElementAt(indexOfCharacterToAssignToFirstBlock),
					AssignCharacterViewModel.Delivery.Normal);
				Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnknown);
				m_model.LoadNextRelevantBlock();
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnknown);
				Assert.AreEqual(8, m_model.CurrentBlock.ChapterNumber);
				Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
				Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(posibleCharactersForActs837));
			}
			finally
			{
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			}
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_Null_CharacterSetToAmbiguousAndNotUserConfirmed()
		{
			m_model.AttemptRefBlockMatchup = true;
			// To most closely simulate the real situation where this can occur, find a place where the matchup results in a correlated block with an
			// unknown character ID. Then set an adjacent block's character id to "null", as would happen if the user atempted to swap the values between
			// these two blocks.
			while ((m_model.CurrentReferenceTextMatchup == null || !m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => b.CharacterIsUnknown)) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var blockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First(b => b.CharacterIsUnknown);
			var indexOfBlockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.IndexOf(blockWithoutCharacterId);
			var indexOfAdjacentBlock = (indexOfBlockWithoutCharacterId > 0) ? indexOfBlockWithoutCharacterId - 1 : indexOfBlockWithoutCharacterId + 1;
			var adjacentBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[indexOfAdjacentBlock];

			m_model.SetReferenceTextMatchupCharacter(indexOfAdjacentBlock, null);

			Assert.IsTrue(adjacentBlock.CharacterIsUnknown);
			Assert.IsFalse(adjacentBlock.UserConfirmed);
		}

		[Test]
		public void SetCharacterAndDelivery_BlockMatchupIsSet_OriginalAndCorrelatedAnchorBlocksAreModified()
		{
			m_fullProjectRefreshRequired = true;
			FindRefInMark(9, 21);
			var originalAnchorBlock = m_model.CurrentBlock;
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsTrue(originalAnchorBlock.CharacterIsUnknown);
			var charactersForVerse = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var deliveriesForVerse = m_model.GetUniqueDeliveries().ToList();
			var characterJesus = charactersForVerse.Single(c => c.CharacterId == "Jesus");
			var deliveryQuestioning = deliveriesForVerse.Single(d => d.Text == "questioning");
			var characterFather = charactersForVerse.Single(c => c.CharacterId == "father of demon-possessed boy");
			var deliveryDistraught = deliveriesForVerse.Single(d => d.Text == "distraught");

			// Part I: Assign to Jesus/questioning (which is the automatic matchup)
			Assert.IsTrue(m_model.IsModified(characterJesus, deliveryQuestioning),
				"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.");
			Assert.AreNotEqual("Jesus", originalAnchorBlock.CharacterId);
			Assert.AreEqual("Jesus", m_model.CurrentBlock.CharacterId);

			m_model.SetCharacterAndDelivery(characterJesus, deliveryQuestioning);
			Assert.AreEqual("Jesus", m_model.CurrentBlock.CharacterId);
			Assert.AreEqual("questioning", m_model.CurrentBlock.Delivery);
			Assert.AreEqual("Jesus", originalAnchorBlock.CharacterId);
			Assert.AreEqual("questioning", originalAnchorBlock.Delivery);
			Assert.AreEqual("Jesus", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId);
			Assert.AreEqual("questioning", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery);
			Assert.IsFalse(m_model.IsModified(characterJesus, deliveryQuestioning),
				"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.");

			// Part II: Assign to father of demon-possessed boy/distraught
			Assert.IsTrue(m_model.IsModified(characterFather, deliveryDistraught),
				"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.");
			Assert.AreNotEqual(characterFather.CharacterId, originalAnchorBlock.CharacterId);
			Assert.AreNotEqual(characterFather.CharacterId, m_model.CurrentBlock.CharacterId);
			m_model.SetCharacterAndDelivery(characterFather, deliveryDistraught);
			Assert.AreEqual(characterFather.CharacterId, m_model.CurrentBlock.CharacterId);
			Assert.AreEqual("distraught", m_model.CurrentBlock.Delivery);
			Assert.AreEqual(characterFather.CharacterId, originalAnchorBlock.CharacterId);
			Assert.AreEqual("distraught", originalAnchorBlock.Delivery);
			Assert.AreEqual(characterFather.CharacterId, m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId);
			Assert.AreEqual("distraught", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery);
			Assert.IsFalse(m_model.IsModified(characterFather, deliveryDistraught),
				"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.");

			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnknown));
			Assert.IsTrue(m_model.CurrentBlock.ChapterNumber > 9);
		}

		[Test]
		public void StoreCharacterDetail_CallTwiceWithSameCharacter_NotSavedInProject()
		{
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);
			Assert.IsFalse(reloadedProject.AllCharacterDetailDictionary.ContainsKey("Larry"));
		}

		[Test]
		public void GetUniqueCharacters_AfterAddingProjectSpecificCharacter_ListIncludesCharacter()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Larry"),
				AssignCharacterViewModel.Delivery.Normal);
			Assert.IsTrue(m_model.GetUniqueCharacters("Larry").Any(c => c.CharacterId == "Larry"));
		}

		[Test]
		public void StoreCharacterDetail_CallWithExistingCharacter_Throws()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Larry"),
				AssignCharacterViewModel.Delivery.Normal);
			Assert.Throws<ArgumentException>(() =>
			{
				m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			});
		}

		[Test]
		public void StoreCharacterDetail_CallWithExistingFactoryCharacter_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				m_model.StoreCharacterDetail("Jesus", CharacterGender.Male, CharacterAge.Adult);
			});
		}

		[Test]
		public void SetCharacterAndDelivery_ProjectSpecificCharacter_CharacterDetailSavedInProject()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Larry"),
				AssignCharacterViewModel.Delivery.Normal);
			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);
			Assert.IsTrue(reloadedProject.AllCharacterDetailDictionary.ContainsKey("Larry"));
		}

		[Test]
		public void StoreCharacterDetail_CharacterDetailChangedBeforeSavingInProject_ChangedDetailsSaved()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.YoungAdult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Larry"),
				AssignCharacterViewModel.Delivery.Normal);
			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);
			Assert.AreEqual(CharacterAge.YoungAdult, reloadedProject.AllCharacterDetailDictionary["Larry"].Age);
		}

		// PG-1104
		[Test]
		public void StoreCharacterDetail_CharacterDetailOnlyUsedForHypotheticalSpeech_DoesNotThrow()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("sluggard", CharacterGender.Male, CharacterAge.Adult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("sluggard"),
				AssignCharacterViewModel.Delivery.Normal);
			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);
			Assert.IsTrue(reloadedProject.AllCharacterDetailDictionary.ContainsKey("sluggard"));
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_TwoAddedCharacters_AddsBothToProject()
		{
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey("Christ"));
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey("Thaddeus' wife"));

			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;

			FindRefInMark(8, 5);
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			m_model.StoreCharacterDetail("Christ", CharacterGender.Male, CharacterAge.Adult);
			var newCharacterChrist = new AssignCharacterViewModel.Character("Christ");
			var block = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1];
			m_model.AddPendingProjectCharacterVerseData(block, newCharacterChrist, new AssignCharacterViewModel.Delivery(block.Delivery));
			m_model.SetReferenceTextMatchupDelivery(3, AssignCharacterViewModel.Delivery.Normal);
			var newCharacterThadsWife = new AssignCharacterViewModel.Character("Thaddeus' wife");
			m_model.AddPendingProjectCharacterVerseData(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[3], newCharacterThadsWife, null);
			m_model.StoreCharacterDetail("Thaddeus' wife", CharacterGender.Female, CharacterAge.YoungAdult);
			m_model.SetReferenceTextMatchupCharacter(1, newCharacterChrist);
			m_model.SetReferenceTextMatchupCharacter(3, newCharacterThadsWife);

			m_model.ApplyCurrentReferenceTextMatchup();

			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);

			var christ = reloadedProject.AllCharacterDetailDictionary["Christ"];
			Assert.AreEqual(CharacterAge.Adult, christ.Age);
			Assert.AreEqual(CharacterGender.Male, christ.Gender);

			var wife = reloadedProject.AllCharacterDetailDictionary["Thaddeus' wife"];
			Assert.AreEqual(CharacterAge.YoungAdult, wife.Age);
			Assert.AreEqual(CharacterGender.Female, wife.Gender);
		}

		[TestCase(null)]
		[TestCase("passionate")]
		public void ApplyCurrentReferenceTextMatchup_AddedCharacterWithFollowingContinuationBlocks_AssignedAndAddedToProjectForAllBlocksInQuote(string delivery)
		{
			const string charChrist = "Christ";
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey(charChrist));

			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.AllScripture;

			const int chapter = 13;
			const int startVerse = 5;
			FindRefInMark(chapter, startVerse);
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			var block = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1];
			Assert.AreEqual(5, block.InitialStartVerseNumber);
			Assert.AreEqual(5, block.LastVerseNum, "Sanity check: In this test, we expected the reference text to" +
				"split off individual verses such that verse 6 would be in a separate block from verse 5.");
			var firstVerseNumFollowingSetBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[2].InitialStartVerseNumber;
			Assert.AreEqual(6, firstVerseNumFollowingSetBlock, "Sanity check: In this test, we expected the reference text to" +
				"split off individual verses such that verse 6 would be in a separate block from verse 5.");

			Assert.AreEqual(8, m_model.CurrentReferenceTextMatchup.OriginalBlocks.Last().LastVerseNum,
				"Sanity check: in this test, we were expecting the matchup to cover verses 5-8.");
			int iFollowingBlock = m_model.CurrentBlockIndexInBook + m_model.CurrentReferenceTextMatchup.OriginalBlockCount;
			var followingBlock = m_testProject.Books[0].GetScriptBlocks()[iFollowingBlock];
			Assert.IsTrue(followingBlock.IsContinuationOfPreviousBlockQuote,
				"Sanity check: in this test, we were expecting the block after the matchup to be a continuation.");
			var lastVerseNumOfBlockFollowingMatchup = followingBlock.LastVerseNum;
			Assert.IsFalse(m_testProject.Books[0].GetScriptBlocks()[iFollowingBlock + 1].IsContinuationOfPreviousBlockQuote,
				"Sanity check: in this test, we were expecting only one block after the matchup to be a continuation.");
			Assert.AreEqual(13, lastVerseNumOfBlockFollowingMatchup,
				"Sanity check: in this test, we were expecting the block after the matchup to end at verse 13.");
			m_model.StoreCharacterDetail(charChrist, CharacterGender.Male, CharacterAge.Adult);
			var newCharacterChrist = new AssignCharacterViewModel.Character(charChrist);
			var newDelivery = delivery == null ? null : new AssignCharacterViewModel.Delivery(delivery);
			m_model.AddPendingProjectCharacterVerseData(block, newCharacterChrist, newDelivery);
			m_model.SetReferenceTextMatchupCharacter(1, newCharacterChrist);
			if (newDelivery != null)
			{
				// REVIEW: Currently, Delivery only gets set on the requested block and does not automatically flow through
				// the continuation blocks. This is almost certainly the desired behavior for blocks following the matchup
				// (which the user is not necessarily looking at and which may have already had a different delivery set).
				// However, we might need to think about whether the delivery should flow through the continuation blocks
				// within the matchup if they were previously all part of the same block and were merely split off by the
				// reference text (as is the case with verses 6-8 in this test).
				// For now, the verification logic in this test is based on the current behavior.
				m_model.SetReferenceTextMatchupDelivery(1, newDelivery);
			}

			m_model.ApplyCurrentReferenceTextMatchup();

			var reloadedProject = Project.Load(m_testProject.ProjectFilePath);

			Assert.AreEqual(1, reloadedProject.ProjectCharacterVerseData.GetCharacters(41, chapter, startVerse, startVerse)
				.Count(cv => cv.Character == charChrist && cv.Delivery == (delivery?? "")),
				$"Character ID \"{charChrist}\"{(delivery != null ? " (" + delivery + ")" : "")} missing from " +
				$"ProjectCharacterVerseData for verse {startVerse}");

			for (int verse = firstVerseNumFollowingSetBlock; verse < lastVerseNumOfBlockFollowingMatchup; verse++)
				Assert.AreEqual(1, reloadedProject.ProjectCharacterVerseData.GetCharacters(41, chapter, verse, verse)
						.Count(cv => cv.Character == charChrist && cv.Delivery == ""),
					$"Character ID \"{charChrist}\" missing from ProjectCharacterVerseData for verse {verse}");

			do
			{
				block = reloadedProject.Books[0].GetScriptBlocks()[iFollowingBlock++];
				Assert.IsTrue(block.IsContinuationOfPreviousBlockQuote);
				Assert.AreEqual("Christ", block.CharacterId, "Following block is not assigned to \"{charChrist}\"");
				Assert.IsTrue(string.IsNullOrEmpty(block.Delivery), "Following block should not have Delivery assigned.");
			} while (block.IsScripture && block.LastVerseNum < lastVerseNumOfBlockFollowingMatchup);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NeedAssignmentsTask_ReferenceTextSetButNoAssignmentsMade_NoChange()
		{
			m_model.Mode = BlocksToDisplay.AllQuotes;
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 7, 6);
			var iBlock = m_model.CurrentBlockIndexInBook;
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			m_model.CurrentBlockIndexInBook = iBlock;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(0, matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnknown));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(0, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NotAlignedToReferenceText_ReferenceTextSetButNoAssignmentsMade_NoChangeToMaximumAssignmentsIncremented()
		{
			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 34);
			m_model.AttemptRefBlockMatchup = true;
			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchup.CorrelatedBlocks.Count);
			Assert.IsTrue(matchup.OriginalBlocks.All(b => !b.CharacterIsUnknown));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(1, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NeedAssignmentsTask_ApplyCurrentReferenceTextMatchupMakesTwoAssignments_NoChangeToMaximumAssignmentsIncremented()
		{
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 8, 5);
			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnknown));
			matchupForMark85.SetReferenceText(0, "Some random text: " + Guid.NewGuid());

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(2, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NotAlignedToReferenceText_CharacterAssignmentApplied_NoChangeToMaximumAssignmentsIncremented()
		{
			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 10, 48);
			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
 			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchupForMark85.CorrelatedBlocks.Count);
			Assert.IsTrue(matchupForMark85.CorrelatedBlocks[1].CharacterIsUnknown);
			m_model.SetReferenceTextMatchupCharacter(1, new AssignCharacterViewModel.Character("Bartimaeus (a blind man)"));
			m_model.SetReferenceTextMatchupDelivery(1, new AssignCharacterViewModel.Delivery("shouting", false));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(1, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NeedAssignmentsTask_VernacularSplitByReferenceText_NoChangeToMaximumAssignmentsIncremented()
		{
			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 12, 15);
			m_model.Mode = BlocksToDisplay.NotAssignedAutomatically;
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark1215 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(1, matchupForMark1215.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(1, matchupForMark1215.OriginalBlocks.Count(b => b.CharacterIsUnknown));
			Assert.IsTrue(matchupForMark1215.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			m_model.SetReferenceTextMatchupCharacter(4, new AssignCharacterViewModel.Character("Jesus"));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(1, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NotAlignedToReferenceText_VernacularSplitByReferenceText_MaximumAndAssignmentsIncremented()
		{
			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 3, 19);
			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark319 = m_model.CurrentReferenceTextMatchup;
			var countOfBlocksAddedBySplitting = matchupForMark319.CountOfBlocksAddedBySplitting;
			Assert.IsTrue(countOfBlocksAddedBySplitting > 0);
			Assert.AreEqual(3, matchupForMark319.OriginalBlocks.Count(b => !CharacterVerseData.IsCharacterExtraBiblical(b.CharacterId)));
			var numberOfCorrelatedBlocks = matchupForMark319.CorrelatedBlocks.Count;
			for (int i = 0; i < numberOfCorrelatedBlocks; i++)
			{
				var block = matchupForMark319.CorrelatedBlocks[i];
				if (!block.MatchesReferenceText)
					matchupForMark319.SetReferenceText(i, "Some random text: " + Guid.NewGuid());
				Assert.IsFalse(block.CharacterIsUnknown);
			}

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
				Assert.AreEqual(1, m_assigned);
			}
			finally
			{
				ResetAlignedBlocks(m_model.CurrentReferenceTextMatchup);
			}
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_RelevantMatchup_RemainsRelevant()
		{
			m_assigned = 0;
			m_fullProjectRefreshRequired = true;
			m_model.AttemptRefBlockMatchup = true;
			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			while (m_model.CurrentReferenceTextMatchup == null || m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting == 0 ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.All(
				b => m_model.GetCharactersForCurrentReferenceTextMatchup().Any(c => c.CharacterId == b.CharacterId)))
			{
				m_model.LoadNextRelevantBlock();
			}

			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
			Assert.AreEqual(1, m_assigned);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
		}

		private void FindRefInMark(int chapter, int verse)
		{
			while (m_model.CurrentBlock.ChapterNumber != chapter || m_model.CurrentBlock.InitialStartVerseNumber != verse)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(chapter, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(verse, m_model.CurrentBlock.InitialStartVerseNumber);
		}

		private List<KeyValuePair<int, string>> GetListOfCharacters(int numberOfCharacters, IReadOnlyList<string> characterIds )
		{
			var returnVal = new List<KeyValuePair<int, string>>();

			for (var i = 0; i < numberOfCharacters; i++)
			{
				var kvp = new KeyValuePair<int, string>(i, i < characterIds.Count ? characterIds[i] : "");
				returnVal.Add(kvp);
			}

			return returnVal;
		}

		private void ResetAlignedBlocks(BlockMatchup matchup)
		{
			foreach (var block in matchup.OriginalBlocks)
			{
				if (block.MatchesReferenceText)
					block.MatchesReferenceText = false;
				if (block.UserConfirmed)
				{
					block.UserConfirmed = false;
					block.CharacterId = CharacterVerseData.kAmbiguousCharacter;
				}
			}
		}
	}

	[TestFixture]
	internal class AssignCharacterViewModelConstructorTests
	{
		private Project m_testProject;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		/// <summary>
		/// PG-845
		/// </summary>
		[Test]
		public void Constructor_StartingIndexIsExtraBiblicalBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0,
				m_testProject.IncludedBooks[0].GetScriptBlocks().IndexOf(b => b.CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical) &&
				b.InitialStartVerseNumber > 0)));
			model.AttemptRefBlockMatchup = true;
			Assert.IsFalse(model.CurrentBlock.CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsChapterBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0,
				m_testProject.IncludedBooks[0].GetScriptBlocks().IndexOf(b => b.CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter) &&
				b.ChapterNumber > 0)));
			model.AttemptRefBlockMatchup = true;
			Assert.IsFalse(model.CurrentBlock.CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBook_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(1, 3));
			model.AttemptRefBlockMatchup = true;
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0, Int32.MaxValue));
			model.AttemptRefBlockMatchup = true;
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}
	}

	[TestFixture]
	internal class AssignCharacterViewModelTests_Acts
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			m_model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotYetAssigned,
			m_testProject.Status.AssignCharacterBlock);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_BlockMatchupAlreadyApplied_ThrowsInvalidOperationException()
		{
			m_model.Mode = BlocksToDisplay.NotYetAssigned;
			var verseRefActs837 = new VerseRef(44, 8, 37);
			m_model.TryLoadBlock(verseRefActs837);
			if (!m_model.IsCurrentBlockRelevant)
				m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber == 37);

			m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Philip the evangelist").First().CharacterId;
			m_model.CurrentBlock.UserConfirmed = true;
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
			m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Ethiop").First().CharacterId;
			m_model.CurrentBlock.UserConfirmed = true;
			m_model.LoadNextRelevantBlock();

			m_model.Mode = BlocksToDisplay.NotAlignedToReferenceText;

			Assert.IsFalse(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber <= 37);
			while (m_model.CanNavigateToPreviousRelevantBlock && m_model.CurrentBlock.ChapterNumber >= 8)
			{
				m_model.LoadPreviousRelevantBlock();
				Assert.IsFalse(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber == 37);
			}
		}
	}

	[TestFixture]
	internal class AssignCharacterViewModelMatKunaTests
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private readonly List<int> m_indicesOfChangedBlocks = new List<int>();

		private void CorrelatedBlockCharacterAssignmentChanged(AssignCharacterViewModel sender, int index)
		{
			m_indicesOfChangedBlocks.Add(index);
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			m_model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAlignedToReferenceText, m_testProject.Status.AssignCharacterBlock);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
			m_model.CorrelatedBlockCharacterAssignmentChanged += CorrelatedBlockCharacterAssignmentChanged;
		}

		[TearDown]
		public void TearDown()
		{
			m_model.CorrelatedBlockCharacterAssignmentChanged -= CorrelatedBlockCharacterAssignmentChanged;
			m_testProject = null;
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiblockQuote_CharacterSetForAllContinuationBlocks()
		{
			Func<Block, bool> isAmbiguousStartBlock = block => block.MultiBlockQuote == MultiBlockQuote.Start && block.CharacterId == CharacterVerseData.kAmbiguousCharacter;
			m_model.AttemptRefBlockMatchup = true;
			// Find a matchup that has a multi-block quote that is ambiguous.
			while ((m_model.CurrentReferenceTextMatchup == null ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => isAmbiguousStartBlock(b))) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.IndexOf(b => isAmbiguousStartBlock(b));
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			var continuationBlocks = matchup.CorrelatedBlocks.Skip(indexOfQuoteStartBlock + 1).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote).ToList();
			Assert.IsTrue(continuationBlocks.Any());
			m_indicesOfChangedBlocks.Clear();
			var character = m_model.GetCharactersForCurrentReferenceTextMatchup().Last();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, character);

			Assert.AreEqual(character.CharacterId, startBlock.CharacterId);
			Assert.IsTrue(continuationBlocks.TrueForAll(b => b.CharacterId == character.CharacterId));
			foreach (var iBlock in m_indicesOfChangedBlocks)
			{
				var changedContBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iBlock];
				var i = continuationBlocks.IndexOf(changedContBlock);
				Assert.IsTrue(i >= 0);
				continuationBlocks.RemoveAt(i);
			}
			Assert.IsFalse(continuationBlocks.Any());
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiblockQuote_SetToNarrator_QuoteChainBrokenIntoIndividual()
		{
			Func<Block, bool> isAmbiguousStartBlock = block => block.MultiBlockQuote == MultiBlockQuote.Start && block.CharacterId == CharacterVerseData.kAmbiguousCharacter;
			m_model.AttemptRefBlockMatchup = true;
			// Find a matchup that has a multi-block quote that is ambiguous.
			while ((m_model.CurrentReferenceTextMatchup == null ||
					!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => isAmbiguousStartBlock(b))) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.IndexOf(b => isAmbiguousStartBlock(b));
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			var continuationBlocks = matchup.CorrelatedBlocks.Skip(indexOfQuoteStartBlock + 1).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote).ToList();
			Assert.IsTrue(continuationBlocks.Any());
			m_indicesOfChangedBlocks.Clear();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, AssignCharacterViewModel.Character.Narrator);

			Assert.IsTrue(startBlock.CharacterIs(m_model.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, startBlock.MultiBlockQuote);
			Assert.IsTrue(continuationBlocks.TrueForAll(b => b.CharacterIs(m_model.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(continuationBlocks.TrueForAll(b => !b.IsContinuationOfPreviousBlockQuote));
			foreach (var iBlock in m_indicesOfChangedBlocks)
			{
				var changedContBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iBlock];
				var i = continuationBlocks.IndexOf(changedContBlock);
				Assert.IsTrue(i >= 0);
				continuationBlocks.RemoveAt(i);
			}
			Assert.IsFalse(continuationBlocks.Any());
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiblockQuoteButMatchupContainsNoContinuationBlocks_DoesNotCrash()
		{
			Func<Block, bool> isStartBlockAtEndOfMatchup = block => block.MultiBlockQuote == MultiBlockQuote.Start && block == m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last();
			m_model.AttemptRefBlockMatchup = true;
			// Find a matchup that ends with the first block of a multi-block quote.
			while ((m_model.CurrentReferenceTextMatchup == null ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => isStartBlockAtEndOfMatchup(b))) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);
			Assert.IsTrue(isStartBlockAtEndOfMatchup(matchup.CorrelatedBlocks.Last()));
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.Count - 1;
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			m_indicesOfChangedBlocks.Clear();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, new AssignCharacterViewModel.Character("Martin"));

			Assert.AreEqual("Martin", startBlock.CharacterId);
			Assert.IsFalse(m_indicesOfChangedBlocks.Any());
		}
	}
}
