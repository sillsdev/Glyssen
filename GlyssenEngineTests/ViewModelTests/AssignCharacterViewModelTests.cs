using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Scripture;
using AssignCharacterViewModel = GlyssenEngine.ViewModels.AssignCharacterViewModel<Rhino.Mocks.Interfaces.IMockedObject>;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	internal class AssignCharacterViewModelTests
	{
		private Project m_testProject;
		//private string m_testProjectFilePath;
		private AssignCharacterViewModel m_model;
		private bool m_fullProjectRefreshRequired;
		private int m_assigned;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			//m_testProjectFilePath = ProjectRepository.GetProjectFilePath(m_testProject);
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
				OneTimeTearDown();
				OneTimeSetUp();
				m_fullProjectRefreshRequired = false;
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjects();
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
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
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
		public void GetCharactersForCurrentReference_UnexpectedQuoteWith20BlockForwardContext_GetsNarratorPlusCharactersFromMark1V1Thru24()
		{
			m_model.ForwardContextBlockCount = 20;
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(3, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
			Assert.IsTrue(characters[1].ProjectSpecific);
			// Note: This test used to omit this character because he speaks in v. 24, which is not the first verse in any of the blocks
			// we look at. But we now look at each verse in each block individually, and I think in this case we would probably
			// actually want to include this character since we're trying to come up with a potential list from the surrounding 20 blocks.
			Assert.AreEqual("man possessed by evil spirit", characters[2].CharacterId);
			Assert.IsTrue(characters[2].ProjectSpecific);
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
			m_model.SetMode(m_model.Mode, true);

			var result = m_model.GetCharactersForCurrentReferenceTextMatchup().ToList();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].IsNarrator);
			Assert.AreEqual("Jesus", result[1].CharacterId);
			Assert.AreEqual("father of demon-possessed boy", result[2].CharacterId);
		}

		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_CannedDeliveries_GetsNormalPlusDeliveriesForCoveredVerses()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].IsNormal);
			Assert.AreEqual(1, result.Count(d => d.Text == "questioning"));
			Assert.AreEqual(1, result.Count(d => d.Text == "distraught"));
		}

		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_BlockSetToProjectSpecificDelivery_ResultIncludesProjectSpecificDelivery()
		{
			FindRefInMark(10, 49);
			m_model.SetCharacterAndDelivery(m_model.GetUniqueCharactersForCurrentReference().First(c => c.CharacterId == "Jesus"),
				new AssignCharacterViewModel.Delivery("ordering"));
			m_model.SetMode(m_model.Mode, true);

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
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsFalse()
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = "suspicious"};
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsTrue()
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
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithoutMatchAndSubsequentVerseWithMatch_ReturnsFalse()
		{
			Assert.IsNull(ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(1), 
				m_testProject.Versification).SingleOrDefault(), "Test setup conditions not met");
			var cvMrk14V2 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(2),
				m_testProject.Versification).Single();
			Assert.AreEqual("chief priests/teachers of religious law/elders", cvMrk14V2.Character,
				"Test setup conditions not met");

			var block = new Block("p", 14, 1) { CharacterId = cvMrk14V2.Character }
				.AddText("“Let us arrest Jesus secretly and kill him, ")
				.AddVerse(2, "but not during the festival,” ");
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithNormalMatchAndSubsequentVerseWithIndirectMatch_ReturnsFalse()
		{
			var cvMrk14V32 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(32),
				m_testProject.Versification).Single();
			Assert.AreEqual("Jesus", cvMrk14V32.Character, "Test setup conditions not met");
			Assert.AreEqual(QuoteType.Normal, cvMrk14V32.QuoteType, "Test setup conditions not met");
			Assert.AreEqual(QuoteType.Indirect, ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(33),
					m_testProject.Versification).Single(cv => cv.Character == cvMrk14V32.Character).QuoteType,
				"Test setup conditions not met");

			var block = new Block("p", 14, 32) { CharacterId = cvMrk14V32.Character }
				.AddText("“Sit here while I pray. ")
				.AddVerse(33, "Peter, James and John, You three come along with me.” ");
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithMatchAndSubsequentVerseWithoutMatch_ReturnsFalse()
		{
			var cvMrk14V25 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(25),
				m_testProject.Versification).Single();
			Assert.AreEqual("Jesus", cvMrk14V25.Character, "Test setup conditions not met");
			Assert.IsNull(ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(26),
				m_testProject.Versification).SingleOrDefault(), "Test setup conditions not met");

			var block = new Block("p", 14, 25) { CharacterId = cvMrk14V25.Character }
				.AddText("“This is all I can say, ")
				.AddVerse(26, "because I am not supposed to talk in this verse.” ");
			Assert.IsFalse(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block));
		}

		[Test]
		public void SetMode_AlternateBetweenModes_AssignedBlockCountDoesNotGrowContinuously()
		{
			Assert.AreEqual(0, m_model.CompletedBlockCount);

			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Jesus"), AssignCharacterViewModel.Delivery.Normal);

			Assert.AreEqual(1, m_model.CompletedBlockCount);

			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.AreEqual(0, m_model.CompletedBlockCount);

			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
			Assert.AreEqual(1, m_model.CompletedBlockCount);

			// The assignment call above actually affects 5 blocks because they are all in the same quote.
			m_model.SetMode(BlocksToDisplay.AllScripture);
			Assert.AreEqual(1, m_model.CompletedBlockCount);

			m_model.SetMode(BlocksToDisplay.HotSpots | BlocksToDisplay.ExcludeUserConfirmed);
			Assert.AreEqual(0, m_model.CompletedBlockCount);
		}

		// PG-1344
		[Test]
		public void SetMode_MultipleConfirmedBlocksInMatchupForSingleVoiceBook_CompletedBlockCountDoesNotCountMatchupMoreThanOnce()
		{
			m_fullProjectRefreshRequired = true;

			foreach (var block in m_model.BlockAccessor.CurrentBook.GetScriptBlocks())
			{
				block.CharacterId = CharacterVerseData.kNeedsReview;
				block.UserConfirmed = true;
			}

			m_model.BlockAccessor.CurrentBook.SingleVoice = true;
			m_model.SetMode(BlocksToDisplay.NeedsReview, true);
			Assert.IsTrue(m_model.RelevantBlockCount > 1);
			Assert.AreEqual(m_model.RelevantBlockCount, m_model.CompletedBlockCount);
		}

		// PG-1211
		[Test]
		public void SetMode_SwitchToFilterWithNothingRelevantAfterApplyingMatchupThatCausedCurrentBlockToBeReplaced_ReplacedBlockIsCurrent()
		{
			m_fullProjectRefreshRequired = true;

			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			while (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0 &&
				m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.GetText(true) == m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.GetText(true)) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			Assert.That(!m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.GetText(true) == m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.GetText(true)),
				"Could not find any relevant block in project whose alignment to the reference text would result in splitting the anchor block.");

			foreach (var block in m_model.CurrentReferenceTextMatchup.CorrelatedBlocks)
			{
				block.MultiBlockQuote = MultiBlockQuote.None;
				block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
				block.Delivery = null;
			}

			var origCorrelatedBlocksText = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true)).ToList();

			m_model.ApplyCurrentReferenceTextMatchup();
			m_model.SetMode(BlocksToDisplay.KnownTroubleSpots);
			Assert.AreEqual(0, m_model.RelevantBlockCount, "IMPORTANT NOTE: We're using the KnownTroubleSpots for convenience because it isn't " +
				"implemented. Therefore, nothing matches it. If we ever implement it, this test may need to be adjusted to ensure that when we " +
				"switch to the new filter, nothing matches.");

			Assert.IsTrue(origCorrelatedBlocksText.SequenceEqual(m_testProject.IncludedBooks.Single().GetScriptBlocks()
				.Skip(m_model.CurrentBlockIndexInBook).Take(origCorrelatedBlocksText.Count).Select(b => b.GetText(true))));
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
			Assert.AreEqual(258, uniqueDeliveries.Count());
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
			m_model.SetMode(BlocksToDisplay.AllScripture);
			FindRefInMark(5, 7);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("demons (Legion)/man delivered from Legion of demons"));
			Assert.AreEqual(2, deliveries.Count());
			var uniqueDeliveries = m_model.GetUniqueDeliveries();
			Assert.AreEqual(259, uniqueDeliveries.Count());
		}

		[Test]
		public void GetUniqueDeliveries_HasCurrentDeliveries_FilterText_ReturnsDeliveriesWithFilterText()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
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
			m_fullProjectRefreshRequired = true;

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
			Assert.IsFalse(m_model.SetCurrentBookSingleVoice(true));
			Assert.IsTrue(m_model.IsCurrentBookSingleVoice);
		}

		[Test]
		public void SetCurrentBookSingleVoice_SetFalseFromTrue_IsFalse()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.IsCurrentBookSingleVoice);
			Assert.IsTrue(m_model.SetCurrentBookSingleVoice(false));
			Assert.IsFalse(m_model.IsCurrentBookSingleVoice);
		}

		[Test]
		public void SetCurrentBookSingleVoice_TrueNoSubsequentBooks_CurrentBlockIsUnchanged()
		{
			m_fullProjectRefreshRequired = true;
			var currentBlock = m_model.CurrentBlock;
			m_model.SetCurrentBookSingleVoice(true);

			Assert.AreEqual(currentBlock, m_model.CurrentBlock);
			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.IsFalse(m_model.IsCurrentLocationRelevant);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
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

			model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
			FindRefInMark(1, 7);
			Block firstRelevantBlockAfterTheBlockToSplit = m_model.CurrentBlock;
			m_model.CurrentBlockIndexInBook = m_model.CurrentBlockIndexInBook - 2;
			var blockToSplit = m_model.CurrentBlock;
			var currentBlockCharacterId = blockToSplit.CharacterId;
			Assert.True(blockToSplit.ChapterNumber == 1 && blockToSplit.InitialStartVerseNumber == 5);
			Assert.False(m_model.IsCurrentLocationRelevant);

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "5", 6),
				new BlockSplitData(2, blockToSplit, "5", 9)
			}, GetListOfCharacters(3, new[] {currentBlockCharacterId}));

			Assert.False(m_model.IsCurrentLocationRelevant);
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
			Assert.IsTrue(model.IsCurrentLocationRelevant, "Couldn't find a block to use for this test.");
			Assert.AreEqual("MRK", model.CurrentBookId);

			var verse = currentBlock.InitialVerseNumberOrBridge;
			var splitIndex = currentBlock.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal);
			var indexOfBlockToSplit = model.CurrentBlockIndexInBook;
			Assert.IsTrue(splitIndex > 0);
			var blockTextBeforeSplit = currentBlock.GetText(true);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote);
			Assert.AreEqual("MRK", model.CurrentBookId, "Changing the filter should not have caused us to go to a different book.");
			model.CurrentBlockIndexInBook = indexOfBlockToSplit;
			Assert.AreEqual(currentBlock, model.CurrentBlock, "Setting the CurrentBlockIndexInBook should have moved us back to the block we intend to split.");
			Assert.IsFalse(model.IsCurrentLocationRelevant, "The block we intend to split must not be condidered \"relevant\" with the \"NeedAssignments\" filter.");

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
			Assert.IsFalse(model.IsCurrentLocationRelevant, "The second part of the split block should not be condidered \"relevant\" with the \"NeedAssignments\" filter.");
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
			var model = new AssignCharacterViewModel(project, BlocksToDisplay.NotYetAssigned, null);

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
			var model = new AssignCharacterViewModel(project, BlocksToDisplay.NotAlignedToReferenceText, null);

			Block blockToSplit;
			BlockMatchup matchup;
			do
			{
				model.LoadNextRelevantBlock();
				matchup = model.CurrentReferenceTextMatchup;
				blockToSplit = matchup.CountOfBlocksAddedBySplitting == 0 && matchup.OriginalBlocks.Count() > 1 ?
					matchup.OriginalBlocks.First() : null;
			} while ((blockToSplit == null || blockToSplit.MultiBlockQuote != MultiBlockQuote.Start) && model.CanNavigateToNextRelevantBlock);

			Assert.NotNull(blockToSplit, "Did not find a block sufficient for testing this scenario - no subsequent relevant blocks");
			Assert.IsTrue(blockToSplit.MultiBlockQuote == MultiBlockQuote.Start, "Did not find a block sufficient for testing this scenario");

			var origOriginalBlockCount = matchup.OriginalBlockCount;
			var origCorrelatedBlocks = matchup.CorrelatedBlocks.ToList();
			var originalNextBlock = matchup.OriginalBlocks.ElementAt(1);

			model.LoadNextRelevantBlock();
			var origIndexOfNextRelevantBlock = model.BlockAccessor.GetIndices().BlockIndex;
			model.LoadPreviousRelevantBlock();

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, blockToSplit.LastVerseNum.ToString(), PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new [] { "", "" }));

			// Validates our test was set up correctly
			Assert.AreEqual(originalNextBlock, model.BlockAccessor.GetNthNextBlockWithinBook(1, blockToSplit));

			Assert.AreEqual(origOriginalBlockCount, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.IsTrue(model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(origCorrelatedBlocks.Select(b => b.GetText(true))));

			model.LoadNextRelevantBlock();

			Assert.AreEqual(origIndexOfNextRelevantBlock, model.BlockAccessor.GetIndices().BlockIndex,
				"Index of next relevant block should not have been incremented.");
		}

		[Test]
		public void SplitBlock_SplitWhereMatchupHasCreatedPseudoSplit_MatchupAdjustedToContainBothBlocks()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project, BlocksToDisplay.NotAlignedToReferenceText, null);

			// Go to MAT 5:18
			while (model.CurrentBlock.ChapterNumber != 5 || model.CurrentBlock.InitialStartVerseNumber != 18)
				model.LoadNextRelevantBlock();

			var origSubsequentRelevantBlocksExist = model.CanNavigateToNextRelevantBlock;
			var origCorrelatedBlockCount = model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count;
			var origOriginalBlockCount = model.CurrentReferenceTextMatchup.OriginalBlockCount;

			var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();

			// Validate our setup
			Assert.True(model.IsCurrentLocationRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum, "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.AreEqual(1, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.Greater(blockToSplit.LastVerseNum, 18);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.AreEqual(18, model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum);
			Assert.AreEqual(origCorrelatedBlockCount, model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count);
			Assert.AreEqual(origOriginalBlockCount + 1, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.AreEqual(18, model.CurrentReferenceTextMatchup.OriginalBlocks.First().InitialStartVerseNumber);
			Assert.AreEqual(19, model.CurrentReferenceTextMatchup.OriginalBlocks.ElementAt(1).InitialStartVerseNumber);
			Assert.AreEqual(origSubsequentRelevantBlocksExist, model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void SplitBlock_SplitMidReferenceTextBlock_BlockMatchupShouldContainBothBlocksAndStillBeRelevant()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);

			// Go to MAT 5:18
			while (model.CurrentBlock.ChapterNumber != 5 || model.CurrentBlock.InitialStartVerseNumber != 18)
				model.LoadNextRelevantBlock();

			var origRelevantBlockCount = model.RelevantBlockCount;
			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = origBlock;

			model.LoadNextRelevantBlock();
			var origNextRelevantBlockText = model.CurrentBlock.GetText(true);
			model.LoadPreviousRelevantBlock();

			// Validate our setup
			Assert.True(model.IsCurrentLocationRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum, "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.Greater(blockToSplit.LastVerseNum, 18);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", 10) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.AreEqual(origRelevantBlockCount, model.RelevantBlockCount);
			Assert.True(model.IsCurrentLocationRelevant);
			Assert.AreEqual(18, model.CurrentBlock.LastVerseNum);
			Assert.AreEqual(2, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.AreEqual(origBlock, model.CurrentReferenceTextMatchup.OriginalBlocks.First());
			Assert.AreEqual(String.Join("", model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))), origBlockText);

			model.LoadNextRelevantBlock();
			Assert.AreEqual(origNextRelevantBlockText, model.CurrentBlock.GetText(true));
		}

		// PG-1204
		[Test]
		public void SplitBlock_RelevantTextBlockIsOnlyOriginalBlockInIndex_BlockMatchupExtendedToContainBothBlocks()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote, true);
			Assert.IsTrue(model.IsCurrentLocationRelevant);
			while (model.CurrentReferenceTextMatchup.OriginalBlockCount != 1 && model.CanNavigateToNextRelevantBlock)
			{
				model.LoadNextRelevantBlock();
			}
			Assert.IsTrue(model.IsCurrentLocationRelevant, $"Setup problem: no block in {project.IncludedBooks.Single().BookId} " +
				$"matches the filter for {model.Mode} and results in a matchup with a single original block.");

			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = origBlock;

			var indexOfFirstVerseElement = blockToSplit.BlockElements.IndexOf(be => be is Verse);
			var verseToSplit = ((Verse)blockToSplit.BlockElements[indexOfFirstVerseElement]).Number;
			var splitPosInVerse = ((ScriptText)blockToSplit.BlockElements[indexOfFirstVerseElement + 1]).Content.IndexOf(" ");

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, verseToSplit, splitPosInVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.AreEqual(2, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.AreEqual(origBlock, model.CurrentReferenceTextMatchup.OriginalBlocks.First());
			Assert.AreEqual(String.Join("", model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))), origBlockText);

			var indexOfBlockThatWasSplitOff = model.IndexOfLastBlockInCurrentGroup;
			model.SetMode(BlocksToDisplay.AllScripture, true);
			model.LoadNextRelevantBlock();

			Assert.True(model.IndexOfFirstBlockInCurrentGroup > indexOfBlockThatWasSplitOff);
		}

		// PG-1208
		[Test]
		public void SplitBlock_AdHocTextBlockIsOnlyOriginalBlockInIndex_BlockMatchupExtendedToContainBothBlocksAndStillBeAdHoc()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote, true);
			var verseRef = new VerseRef(BCVRef.BookToNumber("MAT"), 1, 1, ScrVers.English);
			Assert.IsTrue(model.TryLoadBlock(verseRef));
			while (model.CurrentReferenceTextMatchup.OriginalBlockCount != 1 || model.IsCurrentLocationRelevant)
			{
				if (!verseRef.NextVerse())
					Assert.Fail("Could not find any block in MAT to test this case.");
				Assert.IsTrue(model.TryLoadBlock(verseRef));
			}

			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = origBlock;

			var indexOfFirstVerseElement = blockToSplit.BlockElements.IndexOf(be => be is Verse);
			var verseToSplit = ((Verse)blockToSplit.BlockElements[indexOfFirstVerseElement]).Number;
			var splitPosInVerse = ((ScriptText)blockToSplit.BlockElements[indexOfFirstVerseElement + 1]).Content.IndexOf(" ");

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, verseToSplit, splitPosInVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.AreEqual(2, model.CurrentReferenceTextMatchup.OriginalBlockCount);
			Assert.AreEqual(origBlock, model.CurrentReferenceTextMatchup.OriginalBlocks.First());
			Assert.AreEqual(String.Join("", model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))), origBlockText);
			Assert.IsFalse(model.IsCurrentLocationRelevant);

			var indexOfBlockThatWasSplitOff = model.IndexOfLastBlockInCurrentGroup;
			model.SetMode(BlocksToDisplay.AllScripture, true);
			model.LoadNextRelevantBlock();

			Assert.True(model.IndexOfFirstBlockInCurrentGroup > indexOfBlockThatWasSplitOff);
		}

		// PG-1075
		[Test]
		public void SplitBlock_MakeTwoDifferentSplitsAtVerseBoundariesAndNavigateToLaterBlock_IndicesKeptInSync()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.AllQuotes, true);

			// LUK 21:20
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(042021020)));

		    var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "20", PortionScript.kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			// LUK 21:21
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(042021021)));

			blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Last();
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

			Assert.IsTrue(model.SetCurrentBookSingleVoice(true));
			Assert.AreEqual("ACT", model.CurrentBookId);
		}

		[Test]
		public void SetCurrentBookSingleVoice_CurrentBlockIsResultOfBlockMatchupSplit_MatchupResetAndOriginalBlocksRestored()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0);
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
			Assert.IsTrue(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_NarratorBlockWithVersesThatHaveOtherExpectedCharacters_ReturnsNarrator()
		{
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003010))); // This block has verses 7-11
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.IsTrue(characters.Count > 1, "Test setup: expected conditions not met");
			Assert.AreEqual(AssignCharacterViewModel.Character.Narrator, m_model.GetCharacterToSelectForCurrentBlock(null));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_Narrator_ReturnsGenericNarratorCharacter()
		{
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041001039)));
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.AreEqual(1, characters.Count, "Test setup: expected conditions not met");
			Assert.IsTrue(characters[0].IsNarrator, "Test setup: expected conditions not met");
			Assert.AreEqual(AssignCharacterViewModel.Character.Narrator, m_model.GetCharacterToSelectForCurrentBlock(null));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacter_ReturnsAssignedCharacter()
		{
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003009)));
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.GetUniqueCharactersForCurrentReference(false).Any(c => c.CharacterId == "Jesus"));
			Assert.IsTrue(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)).CharacterId == "Jesus");
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacterThatIsNotInEnumeration_ThisShouldNeverHappenInRealLife_ReturnsNull()
		{
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(041003009)));
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(new List<AssignCharacterViewModel.Character>()));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockIsOneOfTwoAmbiguousQuotesInVerse_ReturnsNull()
		{
			FindRefInMark(5, 9);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
			Assert.AreEqual(3, m_model.GetUniqueCharactersForCurrentReference(false).Count()); // Includes narrator
			Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfTwoDialogueQuotesInVerse_ReturnsOtherExpectedCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 9);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
			var possibleCharactersForMark59 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var possibleSpeakingCharactersForMark59 = possibleCharactersForMark59.Where(c => !c.IsNarrator).ToList();
			Assert.AreEqual(2, possibleSpeakingCharactersForMark59.Count);
			m_model.SetCharacterAndDelivery(possibleSpeakingCharactersForMark59[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			possibleSpeakingCharactersForMark59.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear);
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
			Assert.AreEqual(5, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(9, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(possibleSpeakingCharactersForMark59.Single(), m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForMark59));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneDialogueQuoteAndOneNarratorQuotationInVerse_ReturnsOtherCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 41);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
			var possibleCharactersForMark541 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var unusedCharactersForMark541 = possibleCharactersForMark541.ToList();
			Assert.AreEqual(2, unusedCharactersForMark541.Count);
			m_model.SetCharacterAndDelivery(unusedCharactersForMark541[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			unusedCharactersForMark541.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear);
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
			Assert.AreEqual(5, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(41, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(unusedCharactersForMark541.Single(), m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForMark541));
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_Null_CharacterSetToAmbiguousAndNotUserConfirmed()
		{
			m_model.SetMode(m_model.Mode, true);
			// To most closely simulate the real situation where this can occur, find a place where the matchup results in a correlated block with an
			// unknown character ID. Then set an adjacent block's character id to "null", as would happen if the user atempted to swap the values between
			// these two blocks.
			while ((m_model.CurrentReferenceTextMatchup == null || !m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => b.CharacterIsUnclear)) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var blockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First(b => b.CharacterIsUnclear);
			var indexOfBlockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.IndexOf(blockWithoutCharacterId);
			var indexOfAdjacentBlock = (indexOfBlockWithoutCharacterId > 0) ? indexOfBlockWithoutCharacterId - 1 : indexOfBlockWithoutCharacterId + 1;
			var adjacentBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[indexOfAdjacentBlock];

			m_model.SetReferenceTextMatchupCharacter(indexOfAdjacentBlock, null);

			Assert.IsTrue(adjacentBlock.CharacterIsUnclear);
			Assert.IsFalse(adjacentBlock.UserConfirmed);
		}

		// This test for SetCharacterAndDelivery is pointless because that method is never intended to be called
		// when there is a block matchup. (Maybe it was at some time in the past?) It no longer passes because
		// setting m_model.SetMode(m_model.Mode, true) causes refiltering, which changes the state, and
		// FindRefInMark is implemented in such a way that it cannot find the verse when block matches are set.
		// It didn't seem worthwhile to do further debugging of this test, especially since it relied on code
		// which was only ever used for this test. I wasn't include to just delete it yet because I thought its
		// deletion without a comment would raise eyebrows during the review and because it is long and complicated
		// enough that someone might want to try to reinstate it if it can ever be determined that it is useful
		// beyond what it appears to be trying to test.
		//[Test]
		//public void SetCharacterAndDelivery_BlockMatchupIsSet_OriginalAndCorrelatedAnchorBlocksAreModified()
		//{
		//m_fullProjectRefreshRequired = true;
		//FindRefInMark(9, 21);
		//var originalAnchorBlock = m_model.CurrentBlock;
		//m_model.SetMode(m_model.Mode, true);
		//Assert.IsTrue(originalAnchorBlock.CharacterIsUnclear);
		//var charactersForVerse = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
		//var deliveriesForVerse = m_model.GetUniqueDeliveries().ToList();
		//var characterJesus = charactersForVerse.Single(c => c.CharacterId == "Jesus");
		//var deliveryQuestioning = deliveriesForVerse.Single(d => d.Text == "questioning");
		//var characterFather = charactersForVerse.Single(c => c.CharacterId == "father of demon-possessed boy");
		//var deliveryDistraught = deliveriesForVerse.Single(d => d.Text == "distraught");

		//	// Part I: Assign to Jesus/questioning (which is the automatic matchup)
		//	Assert.IsTrue(m_model.IsModified(characterJesus, deliveryQuestioning),
		//		"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.");
		//	Assert.AreNotEqual("Jesus", originalAnchorBlock.CharacterId);
		//	Assert.AreEqual("Jesus", m_model.CurrentBlock.CharacterId);

		//	m_model.SetCharacterAndDelivery(characterJesus, deliveryQuestioning);
		//	Assert.AreEqual("Jesus", m_model.CurrentBlock.CharacterId);
		//	Assert.AreEqual("questioning", m_model.CurrentBlock.Delivery);
		//	Assert.AreEqual("Jesus", originalAnchorBlock.CharacterId);
		//	Assert.AreEqual("questioning", originalAnchorBlock.Delivery);
		//	Assert.AreEqual("Jesus", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId);
		//	Assert.AreEqual("questioning", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery);
		//	Assert.IsFalse(m_model.IsModified(characterJesus, deliveryQuestioning),
		//		"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.");

		//	// Part II: Assign to father of demon-possessed boy/distraught
		//	Assert.IsTrue(m_model.IsModified(characterFather, deliveryDistraught),
		//		"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.");
		//	Assert.AreNotEqual(characterFather.CharacterId, originalAnchorBlock.CharacterId);
		//	Assert.AreNotEqual(characterFather.CharacterId, m_model.CurrentBlock.CharacterId);
		//	m_model.SetCharacterAndDelivery(characterFather, deliveryDistraught);
		//	Assert.AreEqual(characterFather.CharacterId, m_model.CurrentBlock.CharacterId);
		//	Assert.AreEqual("distraught", m_model.CurrentBlock.Delivery);
		//	Assert.AreEqual(characterFather.CharacterId, originalAnchorBlock.CharacterId);
		//	Assert.AreEqual("distraught", originalAnchorBlock.Delivery);
		//	Assert.AreEqual(characterFather.CharacterId, m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId);
		//	Assert.AreEqual("distraught", m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery);
		//	Assert.IsFalse(m_model.IsModified(characterFather, deliveryDistraught),
		//		"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.");

		//	m_model.LoadNextRelevantBlock();
		//	Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear));
		//	Assert.IsTrue(m_model.CurrentBlock.ChapterNumber > 9);
		//}

		/// <summary>
		/// Storing character detail info in the model does not actually add it to the project
		/// unless/until it is actually used for a character assignment.
		/// </summary>
		[Test]
		public void StoreCharacterDetail_CallTwiceWithSameCharacter_NotSavedInProject()
		{
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			var reloadedProject = ReloadTestProject();
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
			var reloadedProject = ReloadTestProject();
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
			var reloadedProject = ReloadTestProject();
			Assert.AreEqual(CharacterAge.YoungAdult, reloadedProject.AllCharacterDetailDictionary["Larry"].Age);
		}

		// PG-1104 - Note: We now always include factory hypothetical characters in our list
		// (though they might not be considered by the quote parser), so they should be treated
		// like any other character - it is an exception to attempt to add one that already exists.
		[Test]
		public void StoreCharacterDetail_CharacterDetailOnlyUsedForHypotheticalSpeech_Throws()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				m_model.StoreCharacterDetail("sluggard", CharacterGender.Male, CharacterAge.Adult);
			});
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_TwoAddedCharacters_AddsBothToProject()
		{
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey("Christ"));
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey("Thaddeus' wife"));

			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);

			FindRefInMark(8, 5);
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

			var reloadedProject = ReloadTestProject();

			var christ = reloadedProject.AllCharacterDetailDictionary["Christ"];
			Assert.AreEqual(CharacterAge.Adult, christ.Age);
			Assert.AreEqual(CharacterGender.Male, christ.Gender);

			var wife = reloadedProject.AllCharacterDetailDictionary["Thaddeus' wife"];
			Assert.AreEqual(CharacterAge.YoungAdult, wife.Age);
			Assert.AreEqual(CharacterGender.Female, wife.Gender);
		}

		private Project ReloadTestProject() =>
			Project.Load(new Project((GlyssenDblTextMetadata)m_testProject.Metadata), null, null);


		[TestCase(null)]
		[TestCase("passionate")]
		public void ApplyCurrentReferenceTextMatchup_AddedCharacterWithFollowingContinuationBlocks_AssignedAndAddedToProjectForAllBlocksInQuote(string delivery)
		{
			const string charChrist = "Christ";
			Assert.IsFalse(m_testProject.AllCharacterDetailDictionary.ContainsKey(charChrist));

			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.AllScripture, true);

			const int chapter = 13;
			const int startVerse = 5;
			FindRefInMark(chapter, startVerse);
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

			var reloadedProject = ReloadTestProject();

			var cvData = new CombinedCharacterVerseData(reloadedProject);

			Assert.AreEqual(1, cvData.GetCharacters(41, chapter, new SingleVerse(startVerse))
				.Count(cv => cv.Character == charChrist && cv.Delivery == (delivery?? "")),
				$"Character ID \"{charChrist}\"{(delivery != null ? " (" + delivery + ")" : "")} missing from " +
				$"ProjectCharacterVerseData for verse {startVerse}");

			do
			{
				block = reloadedProject.Books[0].GetScriptBlocks()[iFollowingBlock++];
				
				Assert.AreEqual(1, cvData.GetCharacters(41, chapter, block.AllVerses)
						.Count(cv => cv.Character == charChrist && cv.Delivery == ""),
					$"Character ID \"{charChrist}\" missing from ProjectCharacterVerseData for block {block}");

				Assert.IsTrue(block.IsContinuationOfPreviousBlockQuote);
				Assert.AreEqual("Christ", block.CharacterId, $"Following block is not assigned to \"{charChrist}\"");
				Assert.IsTrue(string.IsNullOrEmpty(block.Delivery), "Following block should not have Delivery assigned.");
			} while (block.IsScripture && block.LastVerseNum < lastVerseNumOfBlockFollowingMatchup);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NeedAssignmentsTask_ReferenceTextSetButNoAssignmentsMade_NoChange()
		{
			m_fullProjectRefreshRequired = true;

			m_model.SetMode(BlocksToDisplay.AllQuotes, true);
			FindRefInMark(7, 6);
			var iBlock = m_model.CurrentBlockIndexInBook;
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			m_model.CurrentBlockIndexInBook = iBlock;
			Assert.IsFalse(m_model.IsCurrentLocationRelevant);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(0, matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnclear));

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
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			FindRefInMark(9, 34);
			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchup.CorrelatedBlocks.Count);
			Assert.IsTrue(matchup.OriginalBlocks.All(b => !b.CharacterIsUnclear));

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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			m_assigned = 0;

			FindRefInMark(8, 5);
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnclear));
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
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			FindRefInMark(10, 48);
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
 			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(2, matchupForMark85.CorrelatedBlocks.Count);
			Assert.IsTrue(matchupForMark85.CorrelatedBlocks[1].CharacterIsUnclear);
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
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			FindRefInMark(12, 15);
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			Assert.IsTrue(m_model.IsCurrentLocationRelevant);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			var matchupForMark1215 = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(1, matchupForMark1215.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(1, matchupForMark1215.OriginalBlocks.Count(b => b.CharacterIsUnclear));
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
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			FindRefInMark(3, 19);
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
				Assert.IsFalse(block.CharacterIsUnclear);
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

		// ENHANCE: Add a similar test for a relevant matchup consisting of multiple original blocks but that
		// also inserts blocks due to splits from aligning to reference text. (None of the existing test data
		// used in the fixtures in this file (as of 5/23/2019) have such data.)
		[Test]
		public void ApplyCurrentReferenceTextMatchup_NotAlignedToReferenceText_RelevantMatchupConsistingOfSingleBlockThatIsSplitByRefText_RemainsRelevant()
		{
			// It might seem counter-intuitive that we want the block to remain relevant, but we want the user
			// to be able to navigate back to it and not have the total count keep going down.
			m_assigned = 0;
			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			var origRelevantBlockCount = m_model.RelevantBlockCount;
			while (m_model.CurrentReferenceTextMatchup == null || m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting == 0 ||
				m_model.CurrentReferenceTextMatchup.OriginalBlockCount != 1 ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.All(
				b => m_model.GetCharactersForCurrentReferenceTextMatchup().Any(c => c.CharacterId == b.CharacterId)))
			{
				m_model.LoadNextRelevantBlock();
			}

			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
			Assert.AreEqual(1, m_assigned);
			Assert.IsTrue(m_model.IsCurrentLocationRelevant);
		}

		private void FindRefInMark(int chapter, int verse)
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, chapter, verse);
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
					block.ClearReferenceText();
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TestProject.DeleteTestProjects();
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
			model.SetMode(model.Mode, true);
			Assert.IsFalse(model.CurrentBlock.CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsChapterBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0,
				m_testProject.IncludedBooks[0].GetScriptBlocks().IndexOf(b => b.CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter) &&
				b.ChapterNumber > 0)));
			model.SetMode(model.Mode, true);
			Assert.IsFalse(model.CurrentBlock.CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBook_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(1, 3));
			model.SetMode(model.Mode, true);
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0, Int32.MaxValue));
			model.SetMode(model.Mode, true);
			Assert.IsFalse(model.CanNavigateToPreviousRelevantBlock);
		}
	}

	[TestFixture]
	internal class AssignCharacterViewModelTests_Acts
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
		}

		private void CreateModel(BlocksToDisplay mode)
		{
			m_model = new AssignCharacterViewModel(m_testProject, mode, null);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_BlockMatchupAlreadyApplied_ThrowsInvalidOperationException()
		{
			try
			{
				CreateModel(BlocksToDisplay.NotYetAssigned);
				var verseRefActs837 = new VerseRef(44, 8, 37);
				m_model.TryLoadBlock(verseRefActs837);
				if (!m_model.IsCurrentLocationRelevant)
					m_model.LoadNextRelevantBlock();
				Assert.IsTrue(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber == 37);

				m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Philip the evangelist").First().CharacterId;
				m_model.CurrentBlock.UserConfirmed = true;
				m_model.LoadNextRelevantBlock();
				Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
				m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Ethiop").First().CharacterId;
				m_model.CurrentBlock.UserConfirmed = true;
				m_model.LoadNextRelevantBlock();

				m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);

				Assert.IsFalse(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber <= 37);
				while (m_model.CanNavigateToPreviousRelevantBlock && m_model.CurrentBlock.ChapterNumber >= 8)
				{
					m_model.LoadPreviousRelevantBlock();
					Assert.IsFalse(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber == 37);
				}
			}
			finally
			{
				m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			}
		}

		[Test]
		public void GetCharactersForCurrentReference_AlternatesPresent_GetsAllIncludingAlternates()
		{
			CreateModel(BlocksToDisplay.AllQuotes);
			while (m_model.CurrentBlock.ChapterNumber != 10 || m_model.CurrentBlock.InitialStartVerseNumber != 13)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual("ACT", m_model.CurrentBookId);
			Assert.AreEqual(10, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(13, m_model.CurrentBlock.InitialStartVerseNumber);
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.AreEqual(4, characters.Count);
			Assert.IsFalse(characters.Any(c => c.ProjectSpecific));
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.AreEqual(1, characters.Count(c => c.CharacterId == "Jesus"));
			Assert.AreEqual(1, characters.Count(c => c.CharacterId == "God"));
			Assert.AreEqual(1, characters.Count(c => c.CharacterId == "Holy Spirit, the"));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneIndirectAndOnePotentialInVerse_ReturnsNull(int indexOfCharacterToAssignToFirstBlock)
		{
			CreateModel(BlocksToDisplay.NotAssignedAutomatically);
			try
			{
				while (m_model.CurrentBlock.ChapterNumber != 8 || m_model.CurrentBlock.InitialStartVerseNumber != 37)
					m_model.LoadNextRelevantBlock();
				Assert.AreEqual("ACT", m_model.CurrentBookId);
				Assert.AreEqual(8, m_model.CurrentBlock.ChapterNumber);
				Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
				var possibleCharactersForActs837 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
				Assert.AreEqual(3, possibleCharactersForActs837.Count);
				m_model.SetCharacterAndDelivery(possibleCharactersForActs837.Where(c => !c.IsNarrator).ElementAt(indexOfCharacterToAssignToFirstBlock),
					AssignCharacterViewModel.Delivery.Normal);
				Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear);
				m_model.LoadNextRelevantBlock();
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear);
				Assert.AreEqual(8, m_model.CurrentBlock.ChapterNumber);
				Assert.AreEqual(37, m_model.CurrentBlock.InitialStartVerseNumber);
				Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForActs837));
			}
			finally
			{
				m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
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
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiblockQuote_CharacterSetForAllContinuationBlocks()
		{
			Func<Block, bool> isAmbiguousStartBlock = block => block.MultiBlockQuote == MultiBlockQuote.Start && block.CharacterId == CharacterVerseData.kAmbiguousCharacter;
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

	[TestFixture]
	internal class AssignCharacterViewModelObaTests
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private int m_bookNum;
		const string kObadiahTheProphet = "Obadiah, prophet";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			m_model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.AllScripture, m_testProject.Status.AssignCharacterBlock);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");

			var bookScript = m_testProject.IncludedBooks.Single();
			m_bookNum = bookScript.BookNumber;
			Assert.AreEqual("God", ControlCharacterVerseData.Singleton.GetCharacters(m_bookNum, 1, 18).Single().Character,
				"Test setup conditions not met!");

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var overrideOba = NarratorOverrides.Singleton.Books.Single(b => b.Id == bookScript.BookId).Overrides.Single();
			Assert.AreEqual(1, overrideOba.StartChapter, "Test setup conditions not met!");
			Assert.AreEqual(1, overrideOba.EndChapter, "Test setup conditions not met!");
			Assert.AreEqual(kObadiahTheProphet, overrideOba.Character, "Test setup conditions not met!");
			Assert.AreEqual(1, overrideOba.StartVerse, "Test setup conditions not met!");
			Assert.AreEqual(2, overrideOba.StartBlock, "Test setup conditions not met!");
			Assert.AreEqual(21, overrideOba.EndVerse, "Test setup conditions not met!");
		}

		[TearDown]
		public void TearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void GetUniqueCharacters_InBlockWithNarratorOverride_ResultIncludesOverrideCharacter()
		{
			// Setup
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 18)), "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters().ToList();

			// Verify
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("God", result[0].CharacterId, "God is first because the list is alphabetical (with narrator last)");
			Assert.AreEqual(kObadiahTheProphet, result[1].CharacterId);
			Assert.AreEqual("narrator (OBA)", result[2].LocalizedDisplay);
		}

		[Test]
		public void GetUniqueCharactersForCurrentReference_InBlockWithNarratorOverride_ResultIncludesOverrideCharacter()
		{
			// Setup
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 18)), "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharactersForCurrentReference().ToList();

			// Verify
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("narrator (OBA)", result[0].LocalizedDisplay, "GetUniqueCharactersForCurrentReference should always put the narrator first.");
			Assert.AreEqual("God", result[1].CharacterId, "God is second because remaining items in the list are alphabetical");
			Assert.AreEqual(kObadiahTheProphet, result[2].CharacterId);
		}

		[Test]
		public void GetUniqueCharacters_MatchingFilterInVerseWithNarratorOverride_ResultStartsWithMatchingCharactersIncludingOverrideCharacter()
		{
			// Setup
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 1)), "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters("Obad").ToList();

			// Verify
			Assert.AreEqual(4, result.Count);
			Assert.IsTrue(result[0].CharacterId.StartsWith("Obadiah"), "The first match happens to be from a different book. " +
				"List of matches is alphabetic");
			Assert.AreEqual(kObadiahTheProphet, result[1].CharacterId);
			Assert.AreEqual("God", result[2].CharacterId, "God is second because he is a Normal character in OBA 1:1");
			Assert.AreEqual("narrator (OBA)", result[3].LocalizedDisplay, "GetUniqueCharacters should always include" +
				"the narrator at the end, if not already in the list.");
		}

		[Test]
		public void GetUniqueCharacters_MatchingCharacterHasAliasedAndNonAliasedMatch_ResultIncludesNonAliasedRatherThanAliasedCharacter()
		{
			// SUT
			var result = m_model.GetUniqueCharacters("script").ToList();

			// Verify
			Assert.IsTrue(result.Any(c => c.CharacterId == "scripture" && c.LocalizedAlias == null));
			Assert.IsFalse(result.Any(c => c.CharacterId == "scripture" && c.LocalizedAlias != null));
		}

		[Test]
		public void GetUniqueCharacters_NonMatchingFilterInBlockWithNarratorOverride_ResultIncludesOverrideCharacterAtEnd()
		{
			// Setup
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 1)), "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters("Holy S").ToList();

			// Verify
			Assert.IsTrue(result.Count >= 4);
			var iStartOfNonMatching = result.Count -3;
			Assert.AreEqual("God", result[iStartOfNonMatching].CharacterId, "God should be first non-match because this part " +
				"of list is alphabetical.");
			Assert.AreEqual(kObadiahTheProphet, result[iStartOfNonMatching + 1].CharacterId, $"{kObadiahTheProphet} should be " +
				$"second non-match because this part of list is alphabetical.");
			Assert.AreEqual("narrator (OBA)", result[iStartOfNonMatching + 2].LocalizedDisplay, "GetUniqueCharacters should always include" +
				"the narrator at the end, if not already in the list.");
		}
	}
}
