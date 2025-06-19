using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Scripture;
using static System.String;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenEngine.Script.PortionScript;
using AssignCharacterViewModel = GlyssenEngine.ViewModels.AssignCharacterViewModel<Rhino.Mocks.Interfaces.IMockedObject>;
using Resources = GlyssenCharactersTests.Properties.Resources;
using static GlyssenSharedTests.CustomConstraints;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	internal class AssignCharacterViewModelTests
	{
		private const string kLegionCharacter = "demons (Legion)/man delivered from Legion of demons";
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private bool m_fullProjectRefreshRequired;
		private int m_assigned;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			CharacterDetailData.TabDelimitedCharacterDetailData = null;
			CreateTestProjectForMark();
		}

		private void CreateTestProjectForMark()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{			
			if (m_fullProjectRefreshRequired)
			{
				TestProject.DeleteTestProjects();
				CreateTestProjectForMark();
				m_fullProjectRefreshRequired = false;
			}
			else
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

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			m_testProject = null;
			TestProject.DeleteTestProjects();
		}

		[Test]
		public void Constructor_FirstQuoteIsUnexpected_FirstUnexpectedBlockLoaded()
		{
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(1));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(2));
		}

		[Test]
		public void Constructor_Match_FirstUnexpectedBlockLoaded()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.TryLoadBlock(new VerseRef(41001001, ScrVers.English));
			m_model.LoadNextRelevantBlock();
			while (m_model.CurrentReferenceTextMatchup.OriginalBlockCount < 3 && m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();

			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlockCount > 2, Is.True, "Setup condition not met!");

			var expectedBook = m_model.CurrentBookId;
			var expectedBlock = m_model.CurrentBlockIndexInBook;
			var expectedNumberOfBlocks = m_model.CurrentReferenceTextMatchup.OriginalBlockCount;

			m_model = new AssignCharacterViewModel(m_testProject, m_model.Mode, new BookBlockIndices(m_testProject.AvailableBooks.IndexOf(b => b.Code == expectedBook), expectedBlock, 2));
			Assert.That(m_model.CurrentBookId, Is.EqualTo(expectedBook));
			Assert.That(m_model.CurrentBlockIndexInBook, Is.EqualTo(expectedBlock));
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(expectedNumberOfBlocks));
			Assert.That(m_model.BlockAccessor.GetIndices().MultiBlockCount, Is.EqualTo(expectedNumberOfBlocks));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
		}

		[Test]
		public void Narrator_CurrentBookIsMark_ToStringIncludesBookName()
		{
			Assert.That(AssignCharacterViewModel.Character.Narrator.ToString(), Is.EqualTo("narrator (MRK)"));
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWithNoContext_GetsNarratorOnly()
		{
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.That(characters.Count, Is.EqualTo(1));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[0].ProjectSpecific, Is.False);
		}

		[Test]
		public void GetCharactersForCurrentReference_NonQuoteBlockContainingVerseWitExpectedQuote_GetsNarratorFollowedByOtherSpeakersInVersesInBlock()
		{
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 1:16-17"));
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(2));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[1].CharacterId, Is.EqualTo("Jesus"));
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWithContext_GetsNarratorOnly()
		{
			// Note: Default forward/backward context is 10 blocks.
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(1));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[0].ProjectSpecific, Is.False);
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWith20BlockForwardContext_GetsNarratorPlusCharactersFromMark1V1Thru24()
		{
			m_model.ForwardContextBlockCount = 20;
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(3));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[0].ProjectSpecific, Is.False);
			Assert.That(characters[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(characters[1].ProjectSpecific, Is.True);
			// Note: This test used to omit this character because he speaks in v. 24, which is not the first verse in any of the blocks
			// we look at. But we now look at each verse in each block individually, and I think in this case we would probably
			// actually want to include this character since we're trying to come up with a potential list from the surrounding 20 blocks.
			Assert.That(characters[2].CharacterId, Is.EqualTo("man possessed by evil spirit"));
			Assert.That(characters[2].ProjectSpecific, Is.True);
		}

		[Test]
		public void GetCharactersForCurrentReference_AmbiguousQuote_GetsBothCharactersPlusNarrator()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(3));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[0].ProjectSpecific, Is.False);
			Assert.That(characters[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(characters[1].ProjectSpecific, Is.False);
			Assert.That(characters[2].CharacterId, Is.EqualTo(kLegionCharacter));
			Assert.That(characters[2].ProjectSpecific, Is.False);
		}

		[Test]
		public void GetCharactersForCurrentReferenceTextMatchup_Works()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);

			var result = m_model.GetCharactersForCurrentReferenceTextMatchup().ToList();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].IsNarrator, Is.True);
			Assert.That(result[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(result[2].CharacterId, Is.EqualTo("father of demon-possessed boy"));
		}

		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_CannedDeliveries_GetsNormalPlusDeliveriesForCoveredVerses()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].IsNormal, Is.True);
			Assert.That(result.Count(d => d.Text == "questioning"), Is.EqualTo(1));
			Assert.That(result.Count(d => d.Text == "distraught"), Is.EqualTo(1));
		}

		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_BlockSetToProjectSpecificDelivery_ResultIncludesProjectSpecificDelivery()
		{
			m_fullProjectRefreshRequired = true;
			FindRefInMark(10, 49);
			m_model.SetCharacterAndDelivery(m_model.GetUniqueCharactersForCurrentReference().First(c => c.CharacterId == "Jesus"),
				new AssignCharacterViewModel.Delivery("ordering"));
			m_model.SetMode(m_model.Mode, true);

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].IsNormal, Is.True);
			Assert.That(result.Count(d => d.Text == "encouraging"), Is.EqualTo(1));
			Assert.That(result.Count(d => d.Text == "ordering"), Is.EqualTo(1));
		}


		#region PG-1401
		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_BlockReferenceTextSetToNonstandardDelivery_ResultIncludesNonstandardDelivery()
		{
			FindRefInMark(10, 49);
			m_model.SetMode(m_model.Mode, true);
			var baseList = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			var block = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First(b => b.MatchesReferenceText &&
				b.CharacterId != GetStandardCharacterId("MRK", StandardCharacter.Narrator));
			block.ReferenceBlocks.Single().Delivery = "slurred";

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.That(baseList.Count + 1, Is.EqualTo(result.Count));
			Assert.That(result[0].IsNormal, Is.True);
			Assert.That(baseList,
				ForEvery<AssignCharacterViewModel.Delivery>(d => result.Contains(d), Is.True));
			Assert.That(result.Any(d => d.Text == "slurred"), Is.True);
		}

		[Test]
		public void GetDeliveriesForCurrentReferenceTextMatchup_SecondLevelBlockReferenceTextSetToNonstandardDelivery_ResultIncludesNonstandardDelivery()
		{
			m_fullProjectRefreshRequired = true;
			m_testProject.ReferenceText = ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian);
			FindRefInMark(10, 49);
			m_model.SetMode(m_model.Mode, true);
			var baseList = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			var block = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First(b => b.MatchesReferenceText &&
				b.CharacterId != GetStandardCharacterId("MRK", StandardCharacter.Narrator));
			var refBlock = block.ReferenceBlocks.Single();
			refBlock.Delivery = null;
			refBlock.ReferenceBlocks.Single().Delivery = "squealing";

			var result = m_model.GetDeliveriesForCurrentReferenceTextMatchup().ToList();
			Assert.That(baseList.Count + 1, Is.EqualTo(result.Count));
			Assert.That(result[0].IsNormal, Is.True);
			Assert.That(baseList, ForEvery<AssignCharacterViewModel.Delivery>(d => result.Contains(d), Is.True));
			Assert.That(result.Any(d => d.Text == "squealing"), Is.True);
		}
		#endregion

		[Test]
		public void GetUniqueCharacters_AmbiguousQuoteNoFilter_GetsAllCharactersInMark()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharacters().ToList();
			Assert.That(characters.Count, Is.EqualTo(80));
			Assert.That(characters.Any(c => c.IsNarrator), Is.True);
			Assert.That(characters.Select(c => c.CharacterId), Does.Contain("Jesus"));
			Assert.That(characters.Select(c => c.CharacterId), Does.Contain(kLegionCharacter));
		}

		[Test]
		public void GetUniqueCharacters_AmbiguousQuoteFilter_GetsFilterCharactersAndReferenceCharacters()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharacters("zeru").ToList();
			Assert.That(characters.Count, Is.EqualTo(5));
			Assert.That(characters[0].CharacterId, Is.EqualTo("Zerubbabel"));
			Assert.That(characters[1].CharacterId,
				Is.EqualTo("Zerubbabel/Jeshua/rest of heads of families"));
			Assert.That(characters.Select(c => c.CharacterId), Does.Contain("Jesus"));
			Assert.That(characters.Select(c => c.CharacterId), Does.Contain(kLegionCharacter));
			Assert.That(characters[4].IsNarrator, Is.True);
		}

		[TestCase("father of cured man, blind from birth")]
		public void GetUniqueCharacters_MatchingFilterInCharacterDetailButNotInCharacterVerse_ResultIncludesMatchingDetailCharacter(
			string character)
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Any(c => c.Character == character), Is.False,
				"Setup precondition not met");
			Assert.That(CharacterDetailData.Singleton.GetDictionary()[character].FirstReference, Is.Null,
				"Setup precondition not met");
			Assert.That(character, Is.EqualTo(m_model.GetUniqueCharacters(character).First().CharacterId));
		}

		[Test]
		public void GetCharactersForCurrentReference_AmbiguousQuote_SortByAlias()
		{
			FindRefInMark(6, 24);
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(3));
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters[1].CharacterId, Is.EqualTo("Herodias' daughter"));
			Assert.That(characters[1].Alias, Is.EqualTo("alias"));
			Assert.That(characters[2].CharacterId, Is.EqualTo("Herodias"));
			Assert.That(characters[2].Alias, Is.Null);
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsTrue(
			string delivery)
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = delivery};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsFalse()
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = "encouraging"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasOnlySpecificDeliveryForThisCharacter_ReturnsTrue()
		{
			var block = new Block("p", 10, 49) {CharacterId = "crowd at Jericho", Delivery = "mumbling"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsTrue(string delivery)
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = delivery};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		[TestCase("exasperated")]
		[TestCase("giving orders")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = delivery};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasTwoSpecificDeliveries_ReturnsTrue()
		{
			var block = new Block("p", 9, 19) {CharacterId = "Jesus", Delivery = "mumbling"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasOnlyUnspecifiedDelivery_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 9, 23) {CharacterId = "Jesus", Delivery = delivery};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasOnlyUnspecifiedDelivery_ReturnsTrue()
		{
			var block = new Block("p", 9, 23) {CharacterId = "Jesus", Delivery = "artful"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		// Note that a line had to be added to TestCharacterVerse for the following three test cases because there was no example of it.
		// As of now, the production version of the file doesn't have anything like this either, but it is a theoretical possibility,
		// so I felt I should text for it.
		[TestCase(null)]
		[TestCase("")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNormalDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsFalse(string delivery)
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = delivery};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsFalse()
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = "suspicious"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasNonMatchingDelivery_ControlFileHasBothUnspecifiedAndSpecificDelivery_ReturnsTrue()
		{
			var block = new Block("p", 11, 5) {CharacterId = "owners of colt", Delivery = "violent"};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.True);
		}

		[TestCase(10, 49, null)]
		[TestCase(9, 19, "")]
		[TestCase(9, 23, null)]
		[TestCase(11, 5, "")]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_NarratorBlock_ReturnsFalse(int chapter, int verse, string delivery)
		{
			var block = new Block("p", chapter, verse)
			{
				CharacterId = GetStandardCharacterId("MRK", StandardCharacter.Narrator),
				Delivery = delivery
			};
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithoutMatchAndSubsequentVerseWithMatch_ReturnsFalse()
		{
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(1), 
				m_testProject.Versification).SingleOrDefault(), Is.Null,
				"Test setup conditions not met");
			var cvMrk14V2 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(2),
				m_testProject.Versification).Single();
			Assert.That(cvMrk14V2.Character, Is.EqualTo("chief priests/teachers of religious law/elders"),
				"Test setup conditions not met");

			var block = new Block("p", 14, 1) { CharacterId = cvMrk14V2.Character }
				.AddText("“Let us arrest Jesus secretly and kill him, ")
				.AddVerse(2, "but not during the festival,” ");
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithNormalMatchAndSubsequentVerseWithIndirectMatch_ReturnsFalse()
		{
			var cvMrk14V32 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(32),
				m_testProject.Versification).Single();
			Assert.That(cvMrk14V32.Character, Is.EqualTo("Jesus"), "Test setup conditions not met");
			Assert.That(QuoteType.Normal, Is.EqualTo(cvMrk14V32.QuoteType), "Test setup conditions not met");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(33),
					m_testProject.Versification).Single(cv => cv.Character == cvMrk14V32.Character).QuoteType,
				Is.EqualTo(QuoteType.Indirect),
				"Test setup conditions not met");

			var block = new Block("p", 14, 32) { CharacterId = cvMrk14V32.Character }
				.AddText("“Sit here while I pray. ")
				.AddVerse(33, "Peter, James and John, You three come along with me.” ");
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void IsBlockAssignedToUnknownCharacterDeliveryPair_BlockHasFirstVerseWithMatchAndSubsequentVerseWithoutMatch_ReturnsFalse()
		{
			var cvMrk14V25 = ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(25),
				m_testProject.Versification).Single();
			Assert.That(cvMrk14V25.Character, Is.EqualTo("Jesus"), "Test setup conditions not met");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(m_model.CurrentBookNumber, 14, new SingleVerse(26),
				m_testProject.Versification).SingleOrDefault(), Is.Null,
				"Test setup conditions not met");

			var block = new Block("p", 14, 25) { CharacterId = cvMrk14V25.Character }
				.AddText("“This is all I can say, ")
				.AddVerse(26, "because I am not supposed to talk in this verse.” ");
			Assert.That(m_model.IsBlockAssignedToUnknownCharacterDeliveryPair(block), Is.False);
		}

		[Test]
		public void SetMode_AlternateBetweenModes_AssignedBlockCountDoesNotGrowContinuously()
		{
			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(0));

			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Jesus"), AssignCharacterViewModel.Delivery.Normal);

			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(1));

			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(0));

			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(1));

			// The assignment call above actually affects 5 blocks because they are all in the same quote.
			m_model.SetMode(BlocksToDisplay.AllScripture);
			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(1));

			m_model.SetMode(BlocksToDisplay.HotSpots | BlocksToDisplay.ExcludeUserConfirmed);
			Assert.That(m_model.CompletedBlockCount, Is.EqualTo(0));
		}

		// PG-1344
		[Test]
		public void SetMode_MultipleConfirmedBlocksInMatchupForSingleVoiceBook_CompletedBlockCountDoesNotCountMatchupMoreThanOnce()
		{
			m_fullProjectRefreshRequired = true;

			foreach (var block in m_model.BlockAccessor.CurrentBook.GetScriptBlocks())
			{
				block.CharacterId = kNeedsReview;
				block.UserConfirmed = true;
			}

			m_model.BlockAccessor.CurrentBook.SingleVoice = true;
			m_model.SetMode(BlocksToDisplay.NeedsReview, true);
			Assert.That(m_model.RelevantBlockCount > 1, Is.True);
			Assert.That(m_model.RelevantBlockCount, Is.EqualTo(m_model.CompletedBlockCount));
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

			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Where(b => b.GetText(true) == m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.GetText(true)),
				Is.Empty,
				"Could not find any relevant block in project whose alignment to the reference text would result in splitting the anchor block.");

			foreach (var block in m_model.CurrentReferenceTextMatchup.CorrelatedBlocks)
			{
				block.MultiBlockQuote = MultiBlockQuote.None;
				block.CharacterId = GetStandardCharacterId("MRK", StandardCharacter.Narrator);
				block.Delivery = null;
			}

			var origCorrelatedBlocksText = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true)).ToList();

			m_model.ApplyCurrentReferenceTextMatchup();
			m_model.SetMode(BlocksToDisplay.KnownTroubleSpots);
			Assert.That(m_model.RelevantBlockCount, Is.EqualTo(0),
				"IMPORTANT NOTE: We're using the KnownTroubleSpots for convenience because it isn't " +
				"implemented. Therefore, nothing matches it. If we ever implement it, this test may need to be adjusted to ensure that when we " +
				"switch to the new filter, nothing matches.");

			Assert.That(origCorrelatedBlocksText.SequenceEqual(m_testProject.IncludedBooks.Single().GetScriptBlocks()
				.Skip(m_model.CurrentBlockIndexInBook).Take(origCorrelatedBlocksText.Count).Select(b => b.GetText(true))), Is.True);
		}

		[Test]
		public void GetDeliveriesForCharacter_NullCharacter_GetsEmptyEnumeration()
		{
			Assert.That(m_model.GetDeliveriesForCharacter(null), Is.Empty);
		}

		[Test]
		public void GetDeliveriesForCharacter_CharacterWithNoDeliveries_GetsOnlyNormalDelivery()
		{
			FindRefInMark(5, 9);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("man with evil spirit")).ToArray();
			Assert.That(deliveries.Length, Is.EqualTo(1));
			Assert.That(AssignCharacterViewModel.Delivery.Normal, Is.EqualTo(deliveries.First()));
		}

		[Test]
		public void GetDeliveriesForCharacter_CharacterWithOneDelivery_GetsDeliveryAndNormal()
		{
			FindRefInMark(5, 9);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("Jesus")).ToArray();
			Assert.That(deliveries.Length, Is.EqualTo(2));
			Assert.That(deliveries, Does.Contain(new AssignCharacterViewModel.Delivery("questioning")));
			Assert.That(deliveries, Does.Contain(AssignCharacterViewModel.Delivery.Normal));
		}

		[Test]
		public void GetUniqueDeliveries_NoFilterText_ReturnsAll()
		{
			var uniqueDeliveries = m_model.GetUniqueDeliveries();
			Assert.That(uniqueDeliveries.Count(), Is.EqualTo(258));
		}

		[Test]
		public void GetUniqueDeliveries_FilterText_ReturnsDeliveriesWithFilterText()
		{
			var uniqueDeliveries = m_model.GetUniqueDeliveries("amazed");
			Assert.That(uniqueDeliveries.Count(), Is.EqualTo(2));
		}

		[Test]
		public void GetUniqueDeliveries_HasCurrentDeliveries_NoFilterText_ReturnsAll()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			FindRefInMark(5, 7);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character(kLegionCharacter));
			Assert.That(deliveries.Count(), Is.EqualTo(2));
			var uniqueDeliveries = m_model.GetUniqueDeliveries();
			Assert.That(uniqueDeliveries.Count(), Is.EqualTo(259));
		}

		[Test]
		public void GetUniqueDeliveries_HasCurrentDeliveries_FilterText_ReturnsDeliveriesWithFilterText()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			FindRefInMark(5, 7);
			m_model.GetUniqueCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character(kLegionCharacter));
			Assert.That(deliveries.Count(), Is.EqualTo(2));
			var uniqueDeliveries = m_model.GetUniqueDeliveries("shrieking");
			Assert.That(uniqueDeliveries.Count(), Is.EqualTo(2));
		}

		[Test]
		public void IsModified_NormalDeliveryNoChange_ReturnsFalse()
		{
			m_fullProjectRefreshRequired = true;

			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal), Is.False);
			m_model.CurrentBlock.Delivery = Empty;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal), Is.False);
		}

		[Test]
		public void IsModified_CharacterChanged_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character("Ferdinand"), AssignCharacterViewModel.Delivery.Normal), Is.True);
		}

		[Test]
		public void IsModified_DeliveryChangedToNormal_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = "annoyed";
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), AssignCharacterViewModel.Delivery.Normal), Is.True);
		}

		[Test]
		public void IsModified_DeliveryChangedFromNormal_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			m_model.CurrentBlock.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), new AssignCharacterViewModel.Delivery("peeved")), Is.True);
		}

		[Test]
		public void IsModified_BlockCharacterAndDeliveryNotSetCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = null;
			block1.Delivery = null;
			Assert.That(m_model.IsModified(null, null), Is.False);
		}

		[Test]
		public void IsModified_BlockCharacterAmbiguousCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = kAmbiguousCharacter;
			Assert.That(m_model.IsModified(null, null), Is.False);
		}

		[Test]
		public void IsModified_BlockCharacterUnknownCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = kUnexpectedCharacter;
			Assert.That(m_model.IsModified(null, null), Is.False);
		}

		[Test]
		public void IsModified_BlockCharacterAndDeliverySetCharacterNull_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "Abram";
			block1.Delivery = "frenetic";
			Assert.That(m_model.IsModified(null, null), Is.True);
		}

		[Test]
		public void IsModified_CharacterUnchangedDeliveryNull_ReturnsTrue()
		{
			// NOTE: This scenario should not be possible via the UI.
			var block1 = m_model.CurrentBlock;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character(block1.CharacterId), null), Is.True);
		}

		[Test]
		public void IsModified_CharacterChangedDeliveryNull_ReturnsTrue()
		{
			m_model.CurrentBlock.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character("Ralph W Emerson"), null), Is.True);
		}

		[Test]
		public void IsModified_StandardCharacter_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "extra-MRK";
			block1.Delivery = "foo";
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character("extra-MRK"), null), Is.False);
		}

		[Test]
		public void IsModified_ChangeToNarrator_ReturnsTrue()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "Theodore";
			block1.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character("narrator-MRK"), null), Is.True);
		}

		[Test]
		public void IsModified_SameNarrator_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = "narrator-MRK";
			block1.Delivery = null;
			Assert.That(m_model.IsModified(new AssignCharacterViewModel.Character("narrator-MRK"), AssignCharacterViewModel.Delivery.Normal), Is.False);
		}

		[Test]
		public void SetCurrentBookSingleVoice_SetTrueFromTrue_IsTrue()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetCurrentBookSingleVoice(true);
			Assert.That(m_model.IsCurrentBookSingleVoice, Is.True);
			Assert.That(m_model.SetCurrentBookSingleVoice(true), Is.False);
			Assert.That(m_model.IsCurrentBookSingleVoice, Is.True);
		}

		[Test]
		public void SetCurrentBookSingleVoice_SetFalseFromTrue_IsFalse()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetCurrentBookSingleVoice(true);
			Assert.That(m_model.IsCurrentBookSingleVoice, Is.True);
			Assert.That(m_model.SetCurrentBookSingleVoice(false), Is.True);
			Assert.That(m_model.IsCurrentBookSingleVoice, Is.False);
		}

		[Test]
		public void SetCurrentBookSingleVoice_TrueNoSubsequentBooks_CurrentBlockIsUnchanged()
		{
			m_fullProjectRefreshRequired = true;
			var currentBlock = m_model.CurrentBlock;
			m_model.SetCurrentBookSingleVoice(true);

			Assert.That(currentBlock, Is.EqualTo(m_model.CurrentBlock));
			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.False);
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
		}

		[Test]
		public void AreAllAssignmentsComplete_OnlyBookSetToSingleVoice_ValueChangesFromFalseToTrue()
		{
			m_fullProjectRefreshRequired = true;
			Assert.That(m_model.IsCurrentTaskComplete, Is.False);
			m_model.SetCurrentBookSingleVoice(true);
			Assert.That(m_model.IsCurrentTaskComplete, Is.True);
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
			Assert.That(currentBlock, Is.EqualTo(m_model.CurrentBlock));
			var preSplit = currentBlock.Clone();

			// List<KeyValuePair<int, string>> characters, Block currentBlock

			m_model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, "2", 6)}, GetListOfCharacters(2, Array.Empty<string>()));
			Assert.That(currentBlock, Is.EqualTo(m_model.CurrentBlock));
			var splitPartA = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var splitPartB = m_model.CurrentBlock;
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.That(partALength, Is.EqualTo(6));
			Assert.That(partALength + splitPartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				Is.EqualTo(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length)));
			m_model.LoadNextRelevantBlock();
			Assert.That(nextBlock, Is.EqualTo(m_model.CurrentBlock));
			m_model.LoadNextRelevantBlock();
			Assert.That(nextNextBlock, Is.EqualTo(m_model.CurrentBlock));
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
			Assert.That(currentBlock, Is.EqualTo(model.CurrentBlock));
			var preSplit = currentBlock.Clone();

			model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, "2", 6)}, GetListOfCharacters(2, Array.Empty<string>()));
			var splitPartA = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var splitPartB = model.CurrentBlock;
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.That(partALength, Is.EqualTo(6));
			Assert.That(partALength + splitPartB.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length),
				Is.EqualTo(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length)));
			model.LoadNextRelevantBlock();
			Assert.That(model.CurrentBlock, Is.EqualTo(nextBlock));
			model.LoadNextRelevantBlock();
			Assert.That(nextNextBlock, Is.EqualTo(model.CurrentBlock));
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
			Assert.That(currentBlock, Is.EqualTo(m_model.CurrentBlock));
			var preSplit = currentBlock.Clone();

			Assert.That(currentBlock.InitialStartVerseNumber, Is.EqualTo(7),
				"If this fails, update the test to reflect the test data.");

			m_model.SplitBlock(new[]
			{
				// The order here is significant as we need to be able to handle them "out of order" like this
				new BlockSplitData(1, currentBlock, "7", 6),
				new BlockSplitData(5, currentBlock, "8", 3),
				new BlockSplitData(4, currentBlock, "7", kSplitAtEndOfVerse),
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
			Assert.That(partALength, Is.EqualTo(2));
			Assert.That(partBLength, Is.EqualTo(4));
			Assert.That(partCLength, Is.EqualTo(5));
			Assert.That(preSplit.BlockElements.OfType<ScriptText>().First().Content.Length - 11, Is.EqualTo(partDLength));
			Assert.That(partELength, Is.EqualTo(3));
			Assert.That(partALength + partBLength + partCLength + partDLength + partELength + partFLength,
				Is.EqualTo(preSplit.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length)));

			m_model.LoadNextRelevantBlock();
			Assert.That(nextBlock, Is.EqualTo(m_model.CurrentBlock));
			m_model.LoadNextRelevantBlock();
			Assert.That(nextNextBlock, Is.EqualTo(m_model.CurrentBlock));
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
			Assert.That(currentBlock, Is.EqualTo(m_model.CurrentBlock));
			var preSplit1 = currentBlock.Clone();
			var preSplit2 = nextBlock.Clone();

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, currentBlock, "2", 6),
				new BlockSplitData(2, nextBlock, "4", 8),
			}, GetListOfCharacters(3, Array.Empty<string>()));

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
			Assert.That(part1ALength, Is.EqualTo(6));
			Assert.That(part2ALength, Is.EqualTo(8));
			Assert.That(part1ALength + part1BLength, Is.EqualTo(
				preSplit1.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length)));
			Assert.That(part2ALength + part2BLength, Is.EqualTo(
				preSplit2.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length)));

			m_model.LoadNextRelevantBlock();
			Assert.That(nextNextBlock, Is.EqualTo(m_model.CurrentBlock));
		}

		[Test]
		public void SplitBlock_DoubleSplitInBlockWhichIsNotRelevant_NewBlocksAllNeedAssignments()
		{
			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
			FindRefInMark(1, 7);
			Block firstRelevantBlockAfterTheBlockToSplit = m_model.CurrentBlock;
			m_model.CurrentBlockIndexInBook -= 2;
			var blockToSplit = m_model.CurrentBlock;
			var currentBlockCharacterId = blockToSplit.CharacterId;
			Assert.That(blockToSplit.ChapterNumber, Is.EqualTo(1));
			Assert.That(blockToSplit.InitialStartVerseNumber, Is.EqualTo(5));

			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "5", 6),
				new BlockSplitData(2, blockToSplit, "5", 9)
			}, GetListOfCharacters(3, new[] {currentBlockCharacterId}));

			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);
			Assert.That(currentBlockCharacterId, Is.EqualTo(m_model.CurrentBlock.CharacterId));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5, Is.True);
			Assert.That(m_model.CurrentBlock.CharacterId, Is.EqualTo(kUnexpectedCharacter));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5, Is.True);
			Assert.That(m_model.CurrentBlock.CharacterId, Is.EqualTo(kUnexpectedCharacter));
			m_model.LoadNextRelevantBlock();
			Assert.That(firstRelevantBlockAfterTheBlockToSplit, Is.EqualTo(m_model.CurrentBlock));
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_DoesNotThrow()
		{
			m_fullProjectRefreshRequired = true;

			m_model.CurrentBlockIndexInBook = 4;
			var block1 = m_testProject.Books[0].Blocks[4];

			Assert.That(block1, Is.EqualTo(m_model.CurrentBlock));
			var block2 = m_testProject.Books[0].Blocks[5];

			Assert.That(block1.InitialStartVerseNumber, Is.EqualTo(2),
				"If this fails, update the test to reflect the test data.");

			Assert.DoesNotThrow(() =>
					m_model.SplitBlock(new[]
					{
						new BlockSplitData(1, block1, "2", 13),
						new BlockSplitData(2, block2, null, 0),
						new BlockSplitData(3, block2, "3", 10)
					}, GetListOfCharacters(4, Array.Empty<string>()))
			);
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_SplitCorrectly()
		{
			m_fullProjectRefreshRequired = true;

			// Find some place where we have a long run of continuation blocks, where the first two are in the
			// same verse but the next one starts a new verse.
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
			Assert.That(block1.BlockElements.OfType<ScriptText>().First().Content.Length > 13, Is.True);
			Assert.That(block2.BlockElements.OfType<ScriptText>().First().Content.Length > 10, Is.True);

			// Now run the SUT

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, block1, block1StartVerse.ToString(), 13),
				new BlockSplitData(2, block2, null, 0),
				new BlockSplitData(3, block2, block1StartVerse.ToString(), 10)
			}, GetListOfCharacters(4, Array.Empty<string>()));

			// check the text
			Assert.That(text1.Substring(0, 13), Is.EqualTo(m_testProject.Books[0].Blocks[i].GetText(false)));
			Assert.That(text1.Substring(13), Is.EqualTo(m_testProject.Books[0].Blocks[i+1].GetText(false)));
			Assert.That(text2.Substring(0, 10), Is.EqualTo(m_testProject.Books[0].Blocks[i + 2].GetText(false)));
			Assert.That(text2.Substring(10), Is.EqualTo(m_testProject.Books[0].Blocks[i + 3].GetText(false)));

			// check the multi-block quote
			Assert.That(m_testProject.Books[0].Blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(m_testProject.Books[0].Blocks[i + 1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(m_testProject.Books[0].Blocks[i + 2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(m_testProject.Books[0].Blocks[i + 3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
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
			Assert.That(model.IsCurrentLocationRelevant, Is.True, "Couldn't find a block to use for this test.");
			Assert.That(model.CurrentBookId, Is.EqualTo("MRK"));

			var verse = currentBlock.InitialVerseNumberOrBridge;
			var splitIndex = currentBlock.BlockElements.OfType<ScriptText>().First().Content.IndexOf(" ", StringComparison.Ordinal);
			var indexOfBlockToSplit = model.CurrentBlockIndexInBook;
			Assert.That(splitIndex > 0, Is.True);
			var blockTextBeforeSplit = currentBlock.GetText(true);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote);
			Assert.That(model.CurrentBookId, Is.EqualTo("MRK"),
				"Changing the filter should not have caused us to go to a different book.");
			model.CurrentBlockIndexInBook = indexOfBlockToSplit;
			Assert.That(currentBlock, Is.EqualTo(model.CurrentBlock),
				"Setting the CurrentBlockIndexInBook should have moved us back to the block we intend to split.");
			Assert.That(model.IsCurrentLocationRelevant, Is.False,
				"The block we intend to split must not be considered \"relevant\" with the \"NeedAssignments\" filter.");

			// Now go to the next relevant block in this same book and remember which block it is. After splitting, going to the next block
			// should still take us to this same block.
			model.LoadNextRelevantBlock();
			var nextBlock = model.CurrentBlock;
			Assert.That(model.CurrentBookId, Is.EqualTo("MRK"));
			var indexOfNextRelevantBlock = model.CurrentBlockIndexInBook;
			Assert.That(indexOfBlockToSplit < model.CurrentBlockIndexInBook, Is.True);

			// Now go back to the block we intend to split.
			model.CurrentBlockIndexInBook = indexOfBlockToSplit;
			Assert.That(currentBlock, Is.EqualTo(model.CurrentBlock));

			model.SplitBlock(new[] {new BlockSplitData(1, currentBlock, verse, splitIndex)},
				GetListOfCharacters(2, new[] {currentBlock.CharacterId, currentBlock.CharacterId}));

			// Verify split
			var splitPartA = model.CurrentBlock;
			model.CurrentBlockIndexInBook = indexOfBlockToSplit + 1;
			var splitPartB = model.CurrentBlock;
			Assert.That(model.IsCurrentLocationRelevant, Is.False,
				"The second part of the split block should not be considered \"relevant\" with the \"NeedAssignments\" filter.");
			var partALength = splitPartA.BlockElements.OfType<ScriptText>().Sum(t => t.Content.Length);
			Assert.That(splitIndex, Is.EqualTo(partALength));
			Assert.That(blockTextBeforeSplit, Does.StartWith(splitPartA.GetText(true)));
			Assert.That(blockTextBeforeSplit.Length, Is.EqualTo(splitPartA.GetText(true).Length + splitPartB.GetText(true).Length));
			Assert.That(blockTextBeforeSplit, Does.EndWith(splitPartB.GetText(true)));

			// Now make sure that LoadNextRelevantBlock still takes us to the same next relevant block as before.
			model.LoadNextRelevantBlock();
			Assert.That(model.CurrentBlock, Is.EqualTo(nextBlock));
			Assert.That(model.CurrentBlockIndexInBook, Is.EqualTo(indexOfNextRelevantBlock + 1));
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocks_IndicesNotChanged()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			var model = new AssignCharacterViewModel(project, BlocksToDisplay.NotYetAssigned, null);

			while (model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				model.LoadNextRelevantBlock();

			Assert.That(model.CanNavigateToNextRelevantBlock, Is.True,
				"Did not find a block sufficient for testing this scenario - no subsequent relevant blocks");
			var blockToSplit = model.CurrentBlock;
			Assert.That(blockToSplit.MultiBlockQuote == MultiBlockQuote.Start, Is.True,
				"Did not find a block sufficient for testing this scenario");

			var originalNextBlock = model.BlockAccessor.GetNextBlock();

			model.LoadNextRelevantBlock();
			var indexOfNextRelevantBlock = model.BlockAccessor.GetIndices().BlockIndex;
			model.LoadPreviousRelevantBlock();

			Assert.That(model.CurrentBlock, Is.EqualTo(blockToSplit), "setup problem!");

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, blockToSplit.LastVerseNum.ToString(), kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new string[] { null, null }));

			// Validates our test was set up correctly
			Assert.That(originalNextBlock, Is.EqualTo(model.BlockAccessor.GetNthNextBlockWithinBook(1, blockToSplit)));

			model.LoadNextRelevantBlock();
			Assert.That(model.CurrentBlock, Is.EqualTo(originalNextBlock));
			model.LoadNextRelevantBlock();
			Assert.That(model.BlockAccessor.GetIndices().BlockIndex, Is.EqualTo(indexOfNextRelevantBlock),
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

			Assert.That(blockToSplit, Is.Not.Null,
				"Did not find a block sufficient for testing this scenario - no subsequent relevant blocks");
			Assert.That(blockToSplit.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start),
				"Did not find a block sufficient for testing this scenario");

			var origOriginalBlockCount = matchup.OriginalBlockCount;
			var origCorrelatedBlocks = matchup.CorrelatedBlocks.ToList();
			var originalNextBlock = matchup.OriginalBlocks.ElementAt(1);

			model.LoadNextRelevantBlock();
			var origIndexOfNextRelevantBlock = model.BlockAccessor.GetIndices().BlockIndex;
			model.LoadPreviousRelevantBlock();

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, blockToSplit.LastVerseNum.ToString(), kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new [] { "", "" }));

			// Validates our test was set up correctly
			Assert.That(originalNextBlock, Is.EqualTo(model.BlockAccessor.GetNthNextBlockWithinBook(1, blockToSplit)));

			Assert.That(origOriginalBlockCount, Is.EqualTo(model.CurrentReferenceTextMatchup.OriginalBlockCount));
			Assert.That(model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true))
				.SequenceEqual(origCorrelatedBlocks.Select(b => b.GetText(true))), Is.True);

			model.LoadNextRelevantBlock();

			Assert.That(model.BlockAccessor.GetIndices().BlockIndex, Is.EqualTo(origIndexOfNextRelevantBlock),
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
			Assert.That(model.IsCurrentLocationRelevant, Is.True);
			Assert.That(model.CurrentBlock.LastVerseNum, Is.EqualTo(18), "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(1));
			Assert.That(blockToSplit.LastVerseNum, Is.GreaterThan(18));

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.That(model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(model.CurrentBlock.LastVerseNum, Is.EqualTo(18));
			Assert.That(model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(origCorrelatedBlockCount));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(origOriginalBlockCount + 1));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlocks.First().InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlocks.ElementAt(1).InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(model.CanNavigateToNextRelevantBlock, Is.EqualTo(origSubsequentRelevantBlocksExist));
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
			Assert.That(model.IsCurrentLocationRelevant, Is.True);
			Assert.That(model.CurrentBlock.LastVerseNum, Is.EqualTo(18), "CurrentBlock is not an original block, so it should end with v. 18");
			Assert.That(blockToSplit.LastVerseNum, Is.GreaterThan(18));

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "18", 10) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.That(origRelevantBlockCount, Is.EqualTo(model.RelevantBlockCount));
			Assert.That(model.IsCurrentLocationRelevant, Is.True);
			Assert.That(model.CurrentBlock.LastVerseNum, Is.EqualTo(18));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(2));
			Assert.That(origBlock, Is.EqualTo(model.CurrentReferenceTextMatchup.OriginalBlocks.First()));
			Assert.That(Concat(model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))),
				Is.EqualTo(origBlockText));

			model.LoadNextRelevantBlock();
			Assert.That(origNextRelevantBlockText, Is.EqualTo(model.CurrentBlock.GetText(true)));
		}

		// PG-1204
		[Test]
		public void SplitBlock_RelevantTextBlockIsOnlyOriginalBlockInIndex_BlockMatchupExtendedToContainBothBlocks()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote, true);
			Assert.That(model.IsCurrentLocationRelevant, Is.True);
			while (model.CurrentReferenceTextMatchup.OriginalBlockCount != 1 && model.CanNavigateToNextRelevantBlock)
			{
				model.LoadNextRelevantBlock();
			}
			Assert.That(model.IsCurrentLocationRelevant, Is.True,
				$"Setup problem: no block in {project.IncludedBooks.Single().BookId} " +
				$"matches the filter for {model.Mode} and results in a matchup with a single original block.");

			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = origBlock;

			var indexOfFirstVerseElement = blockToSplit.BlockElements.IndexOf(be => be is Verse);
			var verseToSplit = ((Verse)blockToSplit.BlockElements[indexOfFirstVerseElement]).Number;
			var splitPosInVerse =
				((ScriptText)blockToSplit.BlockElements[indexOfFirstVerseElement + 1]).Content
				.IndexOf(" ", StringComparison.Ordinal);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, verseToSplit, splitPosInVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(2));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlocks.First(),
				Is.EqualTo(origBlock));
			Assert.That(Concat(model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))),
				Is.EqualTo(origBlockText));

			var indexOfBlockThatWasSplitOff = model.IndexOfLastBlockInCurrentGroup;
			model.SetMode(BlocksToDisplay.AllScripture, true);
			model.LoadNextRelevantBlock();

			Assert.That(model.IndexOfFirstBlockInCurrentGroup > indexOfBlockThatWasSplitOff, Is.True);
		}

		// PG-1208
		[Test]
		public void SplitBlock_AdHocTextBlockIsOnlyOriginalBlockInIndex_BlockMatchupExtendedToContainBothBlocksAndStillBeAdHoc()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.MissingExpectedQuote, true);
			var verseRef = new VerseRef(BCVRef.BookToNumber("MAT"), 1, 1, ScrVers.English);
			Assert.That(model.TryLoadBlock(verseRef), Is.True);
			while (model.CurrentReferenceTextMatchup.OriginalBlockCount != 1 || model.IsCurrentLocationRelevant)
			{
				if (!verseRef.NextVerse())
					Assert.Fail("Could not find any block in MAT to test this case.");
				Assert.That(model.TryLoadBlock(verseRef), Is.True);
			}

			var origBlock = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			var origBlockText = origBlock.GetText(true);
			var blockToSplit = origBlock;

			var indexOfFirstVerseElement = blockToSplit.BlockElements.IndexOf(be => be is Verse);
			var verseToSplit = ((Verse)blockToSplit.BlockElements[indexOfFirstVerseElement]).Number;
			var splitPosInVerse =
				((ScriptText)blockToSplit.BlockElements[indexOfFirstVerseElement + 1]).Content
				.IndexOf(" ", StringComparison.Ordinal);

			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, verseToSplit, splitPosInVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlockCount, Is.EqualTo(2));
			Assert.That(origBlock, Is.EqualTo(model.CurrentReferenceTextMatchup.OriginalBlocks.First()));
			Assert.That(Concat(model.CurrentReferenceTextMatchup.OriginalBlocks.Select(b => b.GetText(true))),
				Is.EqualTo(origBlockText));
			Assert.That(model.IsCurrentLocationRelevant, Is.False);

			var indexOfBlockThatWasSplitOff = model.IndexOfLastBlockInCurrentGroup;
			model.SetMode(BlocksToDisplay.AllScripture, true);
			model.LoadNextRelevantBlock();

			Assert.That(model.IndexOfFirstBlockInCurrentGroup,
				Is.GreaterThan(indexOfBlockThatWasSplitOff));
		}

		// PG-1075
		[Test]
		public void SplitBlock_MakeTwoDifferentSplitsAtVerseBoundariesAndNavigateToLaterBlock_IndicesKeptInSync()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			var model = new AssignCharacterViewModel(project);

			model.SetMode(BlocksToDisplay.AllQuotes, true);

			// LUK 21:20
			Assert.That(model.TryLoadBlock(new VerseRef(042021020)), Is.True);

		    var blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Single();
			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "20", kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			// LUK 21:21
			Assert.That(model.TryLoadBlock(new VerseRef(042021021)), Is.True);

			blockToSplit = model.CurrentReferenceTextMatchup.OriginalBlocks.Last();
			model.SplitBlock(new[] { new BlockSplitData(1, blockToSplit, "21", kSplitAtEndOfVerse) },
				GetListOfCharacters(2, new[] { "", "" }));

			// LUK 21:25
			Assert.That(model.TryLoadBlock(new VerseRef(042021025)), Is.True);

			// Before our fix, this was throwing an IndexOutOfRangeException
			Assert.That(model.CanNavigateToNextRelevantBlock, Is.True);
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseBridgeAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001012)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "12-16", 3),
				new BlockSplitData(2, blockToSplit, "17", 8)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].GetText(false),
				Is.EqualTo("Verse 1 text. Verses 2-6a text. Verses 6b-11 text. Ver"));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].GetText(false),
				Is.EqualTo("ses 12-16 text. Verse 17"));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].GetText(false),
				Is.EqualTo(" text. "));
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001018)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "18a", 4),
				new BlockSplitData(2, blockToSplit, "18b", 5)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].GetText(false),
				Is.EqualTo("Vers"));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].GetText(false),
				Is.EqualTo("e 18a text. Verse"));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].GetText(false),
				Is.EqualTo(" 18b text. "));
		}

		//PG-1081
		[Test]
		public void SplitBlock_VerseBridgeWithSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001001)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "1", 4),
				new BlockSplitData(2, blockToSplit, "2-6a", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].GetText(false),
				Is.EqualTo("Vers"));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].GetText(false),
				Is.EqualTo("e 1 text. Verses "));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].GetText(false),
				Is.EqualTo("2-6a text. Verses 6b-11 text. Verses 12-16 text. Verse 17 text. "));
		}

		//PG-1081
		// Not sure if we will ever get data in this form, but testing just in case
		[Test]
		public void SplitBlock_VerseWhichContinuesIntoVerseBridgeAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001019)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "19", 4),
				new BlockSplitData(2, blockToSplit, "19-20", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].GetText(false),
				Is.EqualTo("Vers"));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].GetText(false),
				Is.EqualTo("e 19 text. Verses "));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].GetText(false),
				Is.EqualTo("19-20 text. "));
		}

		//PG-1081
		// Not sure if we will ever get data in this form, but testing just in case
		[Test]
		public void SplitBlock_VerseWithAndWithoutSegmentAndMoreThanOneSplit_SplitsCorrectly()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001021)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "21", 4),
				new BlockSplitData(2, blockToSplit, "21b", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].GetText(false),
				Is.EqualTo("Vers"));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].GetText(false),
				Is.EqualTo("e 21 text. Verse 2"));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].GetText(false),
				Is.EqualTo("1b text. "));
		}

		// PG-1089
		[Test]
		public void SplitBlock_CharacterIdsAreEmptyString_CharactersSetToUnknown()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			var model = new AssignCharacterViewModel(project);

			Assert.That(model.TryLoadBlock(new VerseRef(031001021)), Is.True);

			var blockToSplit = model.CurrentBlock;
			var blockIndex = model.CurrentBlockIndexInBook;

			model.SplitBlock(new[]
			{
				new BlockSplitData(1, blockToSplit, "21", 4),
				new BlockSplitData(2, blockToSplit, "21b", 7)
			}, GetListOfCharacters(3, new[] { "", "", "" }));

			Assert.That(project.Books[0].Blocks[blockIndex].CharacterId,
				Is.EqualTo(kUnexpectedCharacter));
			Assert.That(project.Books[0].Blocks[blockIndex + 1].CharacterId,
				Is.EqualTo(kUnexpectedCharacter));
			Assert.That(project.Books[0].Blocks[blockIndex + 2].CharacterId,
				Is.EqualTo(kUnexpectedCharacter));
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

			Assert.That(model.TryLoadBlock(new VerseRef(042001001)), Is.True);
			model.LoadNextRelevantBlock();
			Assert.That(model.CurrentBookId, Is.EqualTo("LUK"));

			Assert.That(model.SetCurrentBookSingleVoice(true), Is.True);
			Assert.That(model.CurrentBookId, Is.EqualTo("ACT"));
		}

		[Test]
		public void SetCurrentBookSingleVoice_CurrentBlockIsResultOfBlockMatchupSplit_MatchupResetAndOriginalBlocksRestored()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			Assert.That(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting,
				Is.GreaterThan(0));
			Assert.That(m_testProject.Books[0].Blocks, Does.Not.Contain(m_model.CurrentBlock));
			m_model.SetCurrentBookSingleVoice(true);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Null);
			Assert.That(m_testProject.Books[0].Blocks, Does.Contain(m_model.CurrentBlock));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_NarratorBlockWithVersesThatHaveOtherExpectedCharacters_ReturnsNarrator()
		{
			Assert.That(m_model.TryLoadBlock(new VerseRef(041003010)), Is.True); // This block has verses 7-11
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.That(characters.Count, Is.GreaterThan(1),
				"Test setup: expected conditions not met");
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(null),
				Is.EqualTo(AssignCharacterViewModel.Character.Narrator));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_Narrator_ReturnsGenericNarratorCharacter()
		{
			Assert.That(m_model.TryLoadBlock(new VerseRef(041001039)), Is.True);
			var characters = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.That(characters.Count, Is.EqualTo(1), "Test setup: expected conditions not met");
			Assert.That(characters[0].IsNarrator, Is.True, "Test setup: expected conditions not met");
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(null),
				Is.EqualTo(AssignCharacterViewModel.Character.Narrator));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacter_ReturnsAssignedCharacter()
		{
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			Assert.That(m_model.TryLoadBlock(new VerseRef(041003009)), Is.True);
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.GetUniqueCharactersForCurrentReference(false)
				.Where(c => c.CharacterId == "Jesus"), Is.Not.Empty);
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)).CharacterId,
				Is.EqualTo("Jesus"));
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockHasAssignedCharacterThatIsNotInEnumeration_ThisShouldNeverHappenInRealLife_ReturnsNull()
		{
			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			Assert.That(m_model.TryLoadBlock(new VerseRef(041003009)), Is.True);
			while (m_model.CurrentBlock.CharacterId != "Jesus")
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(new List<AssignCharacterViewModel.Character>()), Is.Null);
		}

		[Test]
		public void GetCharacterToSelectForCurrentBlock_BlockIsOneOfTwoAmbiguousQuotesInVerse_ReturnsNull()
		{
			FindRefInMark(5, 9);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			Assert.That(m_model.GetUniqueCharactersForCurrentReference(false).Count(),
				Is.EqualTo(3)); // Includes narrator
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)), Is.Null);
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfTwoDialogueQuotesInVerse_ReturnsOtherExpectedCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 9);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			var possibleCharactersForMark59 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var possibleSpeakingCharactersForMark59 = possibleCharactersForMark59.Where(c => !c.IsNarrator).ToList();
			Assert.That(possibleSpeakingCharactersForMark59.Count, Is.EqualTo(2));
			m_model.SetCharacterAndDelivery(possibleSpeakingCharactersForMark59[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			possibleSpeakingCharactersForMark59.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.False);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(5));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(9));
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForMark59),
				Is.EqualTo(possibleSpeakingCharactersForMark59.Single()));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneDialogueQuoteAndOneNarratorQuotationInVerse_ReturnsOtherCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 41);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			var possibleCharactersForMark541 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var unusedCharactersForMark541 = possibleCharactersForMark541.ToList();
			Assert.That(unusedCharactersForMark541.Count, Is.EqualTo(2));
			m_model.SetCharacterAndDelivery(unusedCharactersForMark541[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			unusedCharactersForMark541.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.False);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(5));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(41));
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForMark541),
				Is.EqualTo(unusedCharactersForMark541.Single()));
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_Null_CharacterSetToAmbiguousAndNotUserConfirmed()
		{
			m_model.SetMode(m_model.Mode, true);
			// To most closely simulate the real situation where this can occur, find a place where
			// the matchup results in a correlated block with an unknown character ID. Then set an
			// adjacent block's character id to "null", as would happen if the user attempted to
			// swap the values between these two blocks.
			while ((m_model.CurrentReferenceTextMatchup == null ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => b.CharacterIsUnclear)) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var blockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First(b => b.CharacterIsUnclear);
			var indexOfBlockWithoutCharacterId = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.IndexOf(blockWithoutCharacterId);
			var indexOfAdjacentBlock = (indexOfBlockWithoutCharacterId > 0) ? indexOfBlockWithoutCharacterId - 1 : indexOfBlockWithoutCharacterId + 1;
			var adjacentBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[indexOfAdjacentBlock];

			m_model.SetReferenceTextMatchupCharacter(indexOfAdjacentBlock, null);

			Assert.That(adjacentBlock.CharacterIsUnclear, Is.True);
			Assert.That(adjacentBlock.UserConfirmed, Is.False);
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
		//Assert.That(originalAnchorBlock.CharacterIsUnclear, Is.True);
		//var charactersForVerse = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
		//var deliveriesForVerse = m_model.GetUniqueDeliveries().ToList();
		//var characterJesus = charactersForVerse.Single(c => c.CharacterId == "Jesus");
		//var deliveryQuestioning = deliveriesForVerse.Single(d => d.Text == "questioning");
		//var characterFather = charactersForVerse.Single(c => c.CharacterId == "father of demon-possessed boy");
		//var deliveryDistraught = deliveriesForVerse.Single(d => d.Text == "distraught");

		//	// Part I: Assign to Jesus/questioning (which is the automatic matchup)
		//	Assert.That(m_model.IsModified(characterJesus, deliveryQuestioning),
		//		"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.", Is.True);
		//	Assert.That(originalAnchorBlock.CharacterId, Is.Not.EqualTo("Jesus"));
		//	Assert.That(m_model.CurrentBlock.CharacterId, Is.EqualTo("Jesus"));

		//	m_model.SetCharacterAndDelivery(characterJesus, deliveryQuestioning);
		//	Assert.That(m_model.CurrentBlock.CharacterId, Is.EqualTo("Jesus"));
		//	Assert.That(m_model.CurrentBlock.Delivery, Is.EqualTo("questioning"));
		//	Assert.That(originalAnchorBlock.CharacterId, Is.EqualTo("Jesus"));
		//	Assert.That(originalAnchorBlock.Delivery, Is.EqualTo("questioning"));
		//	Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId, Is.EqualTo("Jesus"));
		//	Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery, Is.EqualTo("questioning"));
		//	Assert.That(m_model.IsModified(characterJesus, deliveryQuestioning),
		//		"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.", Is.False);

		//	// Part II: Assign to father of demon-possessed boy/distraught
		//	Assert.That(m_model.IsModified(characterFather, deliveryDistraught),
		//		"If this isn't true, it is not considered \"dirty\" and the Assign button will not be enabled.", Is.True);
		//	Assert.That(characterFather.CharacterId, Is.Not.EqualTo(originalAnchorBlock.CharacterId));
		//	Assert.That(characterFather.CharacterId, Is.Not.EqualTo(m_model.CurrentBlock.CharacterId));
		//	m_model.SetCharacterAndDelivery(characterFather, deliveryDistraught);
		//	Assert.That(characterFather.CharacterId, Is.EqualTo(m_model.CurrentBlock.CharacterId));
		//	Assert.That(m_model.CurrentBlock.Delivery, Is.EqualTo("distraught"));
		//	Assert.That(characterFather.CharacterId, Is.EqualTo(originalAnchorBlock.CharacterId));
		//	Assert.That(originalAnchorBlock.Delivery, Is.EqualTo("distraught"));
		//	Assert.That(characterFather.CharacterId, Is.EqualTo(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId));
		//	Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.Delivery, Is.EqualTo("distraught"));
		//	Assert.That(m_model.IsModified(characterFather, deliveryDistraught),
		//		"If this isn't false, it is considered \"dirty\" and the Assign button will be enabled.", Is.False);

		//	m_model.LoadNextRelevantBlock();
		//	Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear), Is.True);
		//	Assert.That(m_model.CurrentBlock.ChapterNumber > 9, Is.True);
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
			Assert.That(reloadedProject.AllCharacterDetailDictionary.ContainsKey("Larry"), Is.False);
		}

		[Test]
		public void GetUniqueCharacters_AfterAddingProjectSpecificCharacter_ListIncludesCharacter()
		{
			m_fullProjectRefreshRequired = true;
			m_model.StoreCharacterDetail("Larry", CharacterGender.Male, CharacterAge.Adult);
			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Larry"),
				AssignCharacterViewModel.Delivery.Normal);
			Assert.That(m_model.GetUniqueCharacters("Larry").Any(c => c.CharacterId == "Larry"), Is.True);
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
			Assert.That(reloadedProject.AllCharacterDetailDictionary.ContainsKey("Larry"), Is.True);
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
			Assert.That(CharacterAge.YoungAdult, Is.EqualTo(reloadedProject.AllCharacterDetailDictionary["Larry"].Age));
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
			Assert.That(m_testProject.AllCharacterDetailDictionary.ContainsKey("Christ"), Is.False);
			Assert.That(m_testProject.AllCharacterDetailDictionary.ContainsKey("Thaddeus' wife"), Is.False);

			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);

			FindRefInMark(8, 5);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
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
			Assert.That(CharacterAge.Adult, Is.EqualTo(christ.Age));
			Assert.That(CharacterGender.Male, Is.EqualTo(christ.Gender));

			var wife = reloadedProject.AllCharacterDetailDictionary["Thaddeus' wife"];
			Assert.That(CharacterAge.YoungAdult, Is.EqualTo(wife.Age));
			Assert.That(CharacterGender.Female, Is.EqualTo(wife.Gender));
		}

		private Project ReloadTestProject() =>
			Project.Load(new Project((GlyssenDblTextMetadata)m_testProject.Metadata), null, null);


		[TestCase(null)]
		[TestCase("passionate")]
		public void ApplyCurrentReferenceTextMatchup_AddedCharacterWithFollowingContinuationBlocks_AssignedAndAddedToProjectForAllBlocksInQuote(string delivery)
		{
			const string charChrist = "Christ";
			Assert.That(m_testProject.AllCharacterDetailDictionary.ContainsKey(charChrist), Is.False);

			m_fullProjectRefreshRequired = true;
			m_model.SetMode(BlocksToDisplay.AllScripture, true);

			const int chapter = 13;
			const int startVerse = 5;
			FindRefInMark(chapter, startVerse);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			var block = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1];
			Assert.That(block.InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(block.LastVerseNum, Is.EqualTo(5),
				"Sanity check: In this test, we expected the reference text to split off " +
				"individual verses such that verse 6 would be in a separate block from verse 5.");
			var firstVerseNumFollowingSetBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[2].InitialStartVerseNumber;
			Assert.That(firstVerseNumFollowingSetBlock, Is.EqualTo(6),
				"Sanity check: In this test, we expected the reference text to split off " +
				"individual verses such that verse 6 would be in a separate block from verse 5.");

			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Last().LastVerseNum, Is.EqualTo(8),
				"Sanity check: in this test, we were expecting the matchup to cover verses 5-8.");
			int iFollowingBlock = m_model.CurrentBlockIndexInBook + m_model.CurrentReferenceTextMatchup.OriginalBlockCount;
			var followingBlock = m_testProject.Books[0].GetScriptBlocks()[iFollowingBlock];
			Assert.That(followingBlock.IsContinuationOfPreviousBlockQuote, Is.True,
				"Sanity check: in this test, we were expecting the block after the matchup to be a continuation.");
			var lastVerseNumOfBlockFollowingMatchup = followingBlock.LastVerseNum;
			Assert.That(m_testProject.Books[0].GetScriptBlocks()[iFollowingBlock + 1].IsContinuationOfPreviousBlockQuote, Is.False,
				"Sanity check: in this test, we were expecting only one block after the matchup to be a continuation.");
			Assert.That(lastVerseNumOfBlockFollowingMatchup, Is.EqualTo(13),
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

			Assert.That(cvData.GetCharacters(41, chapter, new SingleVerse(startVerse))
				.Count(cv => cv.Character == charChrist && cv.Delivery == (delivery?? "")),
				Is.EqualTo(1),
				$"Character ID \"{charChrist}\"{(delivery != null ? " (" + delivery + ")" : "")} missing from " +
				$"ProjectCharacterVerseData for verse {startVerse}");

			do
			{
				block = reloadedProject.Books[0].GetScriptBlocks()[iFollowingBlock++];
				
				Assert.That(cvData.GetCharacters(41, chapter, block.AllVerses)
						.Count(cv => cv.Character == charChrist && cv.Delivery == ""),
					Is.EqualTo(1),
					$"Character ID \"{charChrist}\" missing from ProjectCharacterVerseData for block {block}");

				Assert.That(block.IsContinuationOfPreviousBlockQuote, Is.True);
				Assert.That(block.CharacterId, Is.EqualTo("Christ"), $"Following block is not assigned to \"{charChrist}\"");
				Assert.That(block.Delivery, Is.Null.Or.Empty, "Following block should not have Delivery assigned.");
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
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchupForMark85.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnclear), Is.EqualTo(0));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(m_model.RelevantBlockCount, Is.EqualTo(origRelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(0));
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
			Assert.That(matchup.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(matchup.OriginalBlocks, ForEvery<Block>(b => b.CharacterIsUnclear, Is.False));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(1));
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
			Assert.That(matchupForMark85.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchupForMark85.OriginalBlocks.Count(b => b.CharacterIsUnclear), Is.EqualTo(2));
			matchupForMark85.SetReferenceText(0, "Some random text: " + Guid.NewGuid());

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(2));
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
 			Assert.That(matchupForMark85.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchupForMark85.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(matchupForMark85.CorrelatedBlocks[1].CharacterIsUnclear, Is.True);
			m_model.SetReferenceTextMatchupCharacter(1, new AssignCharacterViewModel.Character("Bartimaeus (a blind man)"));
			m_model.SetReferenceTextMatchupDelivery(1, new AssignCharacterViewModel.Delivery("shouting", false));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(1));
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
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);

			var origRelevantBlockCount = m_model.RelevantBlockCount;
			m_assigned = 0;

			var matchupForMark1215 = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchupForMark1215.CountOfBlocksAddedBySplitting, Is.EqualTo(1));
			Assert.That(matchupForMark1215.OriginalBlocks.Count(b => b.CharacterIsUnclear),
				Is.EqualTo(1));
			Assert.That(matchupForMark1215.CorrelatedBlocks,
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			m_model.SetReferenceTextMatchupCharacter(4, new AssignCharacterViewModel.Character("Jesus"));

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(1));
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
			Assert.That(countOfBlocksAddedBySplitting > 0, Is.True);
			Assert.That(matchupForMark319.OriginalBlocks.Count(b => !IsCharacterExtraBiblical(b.CharacterId)), Is.EqualTo(3));
			var numberOfCorrelatedBlocks = matchupForMark319.CorrelatedBlocks.Count;
			for (int i = 0; i < numberOfCorrelatedBlocks; i++)
			{
				var block = matchupForMark319.CorrelatedBlocks[i];
				if (!block.MatchesReferenceText)
					matchupForMark319.SetReferenceText(i, "Some random text: " + Guid.NewGuid());
				Assert.That(block.CharacterIsUnclear, Is.False);
			}

			try
			{
				m_model.ApplyCurrentReferenceTextMatchup();

				Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
				Assert.That(m_assigned, Is.EqualTo(1));
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

			Assert.That(m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0, Is.True);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
			Assert.That(m_assigned, Is.EqualTo(1));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
		}

		[TestCase(true, -1)]
		[TestCase(false, -1)]
		[TestCase(true, 5)]
		[TestCase(false, 5)]
		public void GetRowIndicesForMovingReferenceText_CurrentRowOutOfRange_ThrowsArgumentOutOfRangeException(bool down, int currentRow)
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");

			Assert.Throws<ArgumentOutOfRangeException>(() =>
				m_model.GetRowIndicesForMovingReferenceText(down, currentRow, out _, out int _));
		}

		[TestCase(false, 0)]
		[TestCase(true, 4)]
		public void GetRowIndicesForMovingReferenceText_CurrentRowOutOfRange_ThrowsArgumentException(bool down, int currentRow)
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");

			Assert.Throws<ArgumentException>(() =>
				m_model.GetRowIndicesForMovingReferenceText(down, currentRow, out _, out int _));
		}

		[Test]
		public void GetRowIndicesForMovingReferenceText_ImpossibleCaseWhereMatchupStartsWithSectionHead_ThrowsInvalidOperationException()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[0].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			Assert.Throws<InvalidOperationException>(() =>
				m_model.GetRowIndicesForMovingReferenceText(false, 1, out _, out int _));
		}

		[Test]
		public void GetRowIndicesForMovingReferenceText_ImpossibleCaseWhereMatchupEndsWithSectionHead_ThrowsInvalidOperationException()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			var lastBlockIndex = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count - 1;
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[lastBlockIndex].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			Assert.Throws<InvalidOperationException>(() =>
				m_model.GetRowIndicesForMovingReferenceText(true, lastBlockIndex - 1, out _, out int _));
		}

		[TestCase(true, 0, 0, 1)]
		[TestCase(false, 1, 0, 1)]
		[TestCase(true, 3, 3, 4)]
		[TestCase(false, 4, 3, 4)]
		public void GetRowIndicesForMovingReferenceText_CurrentMatchupDoesNotHaveSectionHead_GetsContiguousRows(
			bool down, int currentRow, int expectedPreceding, int expectedFollowing)
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");

			m_model.GetRowIndicesForMovingReferenceText(down, currentRow, out int iPreceding, out int iFollowing);
			Assert.That(iPreceding, Is.EqualTo(expectedPreceding));
			Assert.That(iFollowing, Is.EqualTo(expectedFollowing));
		}

		[TestCase(true, 0, 1, 0, 2)]
		[TestCase(false, 2, 1, 0, 2)]
		[TestCase(true, 2, 3, 2, 4)]
		[TestCase(false, 4, 3, 2, 4)]
		public void GetRowIndicesForMovingReferenceText_CurrentMatchupHasSectionHead_GetsScriptureRow(
			bool down, int currentRow, int iRowToMakeSectionHead, int expectedPreceding, int expectedFollowing)
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iRowToMakeSectionHead].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			m_model.GetRowIndicesForMovingReferenceText(down, currentRow, out int iPreceding, out int iFollowing);
			Assert.That(iPreceding, Is.EqualTo(expectedPreceding));
			Assert.That(iFollowing, Is.EqualTo(expectedFollowing));
		}

		[TestCase(1)]
		[TestCase(3)]
		public void TryFindScriptureRowAtOrBelow_PassedRowIsSectionHead_GetsNextScripture(int row)
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			int expected = row + 1;
			Assert.That(m_model.TryFindScriptureRowAtOrBelow(ref row), Is.True);
			Assert.That(row, Is.EqualTo(expected));
		}

		[Test]
		public void TryFindScriptureRowAtOrBelow_PassedRowAndFollowingRowAreSectionHead_GetsNextScripture()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			int row = 1;
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row + 1].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			Assert.That(m_model.TryFindScriptureRowAtOrBelow(ref row), Is.True);
			Assert.That(row, Is.EqualTo(3));
		}

		[Test]
		public void TryFindScriptureRowAtOrAbove_PassedRowAndPrecedingRowAreSectionHead_GetsPrevScripture()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			int row = 3;
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row - 1].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			Assert.That(m_model.TryFindScriptureRowAtOrAbove(ref row), Is.True);
			Assert.That(row, Is.EqualTo(1));
		}
		
		[Test]
		public void GetVerseRefForRow_NoCurrentMatchup_GetsRefForCurrentBlock()
		{
			// In real life, this probably should not be called when not in block matchup mode,
			// but even in that mode, there are transitional moments where a current block matchup
			// is not set. This is just an easy way to test that:
			m_model.SetMode(m_model.Mode, false);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Null, "SETUP conditions not met");
			Assert.That(m_model.GetBlockVerseRef(), Is.EqualTo(m_model.GetVerseRefForRow(1)));
		}

		[Test]
		public void GetVerseRefForRow_ScriptureRow_GetsFirstRefForRow()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First().InitialStartVerseNumber, Is.EqualTo(20), "SETUP conditions not met");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(22), "SETUP conditions not met");

			var row = 0;
			var vRef = m_model.GetVerseRefForRow(row);
			Assert.That(vRef.ChapterNum, Is.EqualTo(9));
			Assert.That(vRef.VerseNum, Is.EqualTo(20));

			for (++row; row < 5; row++)
			{
				vRef = m_model.GetVerseRefForRow(row);
				Assert.That(vRef.ChapterNum, Is.EqualTo(9));
				Assert.That(vRef.VerseNum, Is.EqualTo(21));
			}
		}
		
		[Test]
		public void GetVerseRefForRow_PassedRowAndFollowingRowAreSectionHead_GetsRefOfNextScriptureRow()
		{
			FindRefInMark(9, 21);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5), "SETUP conditions not met");
			int row = 2;
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);
			m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[row + 1].CharacterId =
				GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical);

			Assert.That(m_model.GetVerseRefForRow(row).VerseNum, Is.EqualTo(21));
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
					block.CharacterId = kAmbiguousCharacter;
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
				m_testProject.IncludedBooks[0].GetScriptBlocks().IndexOf(b => b.CharacterIs("MRK", StandardCharacter.ExtraBiblical) &&
				b.InitialStartVerseNumber > 0)));
			model.SetMode(model.Mode, true);
			Assert.That(model.CurrentBlock.CharacterIs("MRK", StandardCharacter.ExtraBiblical), Is.False);
			Assert.That(model.CanNavigateToPreviousRelevantBlock, Is.False);
		}

		[Test]
		public void Constructor_StartingIndexIsChapterBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0,
				m_testProject.IncludedBooks[0].GetScriptBlocks().IndexOf(b => b.CharacterIs("MRK", StandardCharacter.BookOrChapter) &&
				b.ChapterNumber > 0)));
			model.SetMode(model.Mode, true);
			Assert.That(model.CurrentBlock.CharacterIs("MRK", StandardCharacter.BookOrChapter), Is.False);
			Assert.That(model.CanNavigateToPreviousRelevantBlock, Is.False);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBook_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(1, 3));
			model.SetMode(model.Mode, true);
			Assert.That(model.CanNavigateToPreviousRelevantBlock, Is.False);
		}

		[Test]
		public void Constructor_StartingIndexIsOutOfRangeBlock_StartingIndexIgnored()
		{
			var model = new AssignCharacterViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically, new BookBlockIndices(0, Int32.MaxValue));
			model.SetMode(model.Mode, true);
			Assert.That(model.CanNavigateToPreviousRelevantBlock, Is.False);
		}
	}

	[TestFixture]
	internal class AssignCharacterViewModelTests_Acts
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private bool m_fullProjectRefreshRequired;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			CreateTestProjectForActs();
		}

		private void CreateTestProjectForActs()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
		}

		[SetUp]
		public void SetUp()
		{			
			if (m_fullProjectRefreshRequired)
			{
				TestProject.DeleteTestProjects();
				CreateTestProjectForActs();
				m_fullProjectRefreshRequired = false;
			}
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
		public void SetMode_NotAlignedToReferenceText_MatchupWithCharactersAssignedIsExcluded()
		{
			m_fullProjectRefreshRequired = true;
			CreateModel(BlocksToDisplay.NotYetAssigned);
			var verseRefActs837 = new VerseRef(44, 8, 37);
			m_model.TryLoadBlock(verseRefActs837);
			if (!m_model.IsCurrentLocationRelevant)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(8));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(37));

			m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Philip the evangelist").First().CharacterId;
			m_model.CurrentBlock.UserConfirmed = true;
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(37));
			m_model.CurrentBlock.CharacterId = m_model.GetUniqueCharacters("Ethiop").First().CharacterId;
			m_model.CurrentBlock.UserConfirmed = true;
			m_model.LoadNextRelevantBlock();

			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);

			Assert.That(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber <= 37, Is.False);
			while (m_model.CanNavigateToPreviousRelevantBlock && m_model.CurrentBlock.ChapterNumber >= 8)
			{
				m_model.LoadPreviousRelevantBlock();
				Assert.That(m_model.CurrentBlock.ChapterNumber == 8 && m_model.CurrentBlock.InitialStartVerseNumber == 37, Is.False);
			}
		}

		[Test]
		public void GetCharactersForCurrentReference_AlternatesPresent_GetsAllIncludingAlternates()
		{
			CreateModel(BlocksToDisplay.AllQuotes);
			while (m_model.CurrentBlock.ChapterNumber != 10 || m_model.CurrentBlock.InitialStartVerseNumber != 13)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBookId, Is.EqualTo("ACT"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(10));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(13));
			var characters = m_model.GetUniqueCharactersForCurrentReference().ToList();
			Assert.That(characters.Count, Is.EqualTo(4));
			Assert.That(characters.Any(c => c.ProjectSpecific), Is.False);
			Assert.That(characters[0].IsNarrator, Is.True);
			Assert.That(characters.Count(c => c.CharacterId == "Jesus"), Is.EqualTo(1));
			Assert.That(characters.Count(c => c.CharacterId == "God"), Is.EqualTo(1));
			Assert.That(characters.Count(c => c.CharacterId == "Holy Spirit, the"), Is.EqualTo(1));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfOneIndirectAndOnePotentialInVerse_ReturnsNull(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;
			CreateModel(BlocksToDisplay.NotAssignedAutomatically);
			while (m_model.CurrentBlock.ChapterNumber != 8 || m_model.CurrentBlock.InitialStartVerseNumber != 37)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBookId, Is.EqualTo("ACT"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(8));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			var possibleCharactersForActs837 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			Assert.That(possibleCharactersForActs837.Count, Is.EqualTo(3));
			m_model.SetCharacterAndDelivery(possibleCharactersForActs837.Where(c => !c.IsNarrator).ElementAt(indexOfCharacterToAssignToFirstBlock),
				AssignCharacterViewModel.Delivery.Normal);
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.False);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.CharacterIsUnclear, Is.True);
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(8));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(m_model.GetCharacterToSelectForCurrentBlock(possibleCharactersForActs837), Is.Null);
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
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
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
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiBlockQuote_CharacterSetForAllContinuationBlocks()
		{
			Func<Block, bool> isAmbiguousStartBlock = block => block.MultiBlockQuote == MultiBlockQuote.Start && block.CharacterId == kAmbiguousCharacter;
			// Find a matchup that has a multi-block quote that is ambiguous.
			while ((m_model.CurrentReferenceTextMatchup == null ||
				!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => isAmbiguousStartBlock(b))) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchup, Is.Not.Null);
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.IndexOf(b => isAmbiguousStartBlock(b));
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			var continuationBlocks = matchup.CorrelatedBlocks.Skip(indexOfQuoteStartBlock + 1).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote).ToList();
			Assert.That(continuationBlocks.Any(), Is.True);
			m_indicesOfChangedBlocks.Clear();
			var character = m_model.GetCharactersForCurrentReferenceTextMatchup().Last();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, character);

			Assert.That(character.CharacterId, Is.EqualTo(startBlock.CharacterId));
			Assert.That(continuationBlocks.TrueForAll(b => b.CharacterId == character.CharacterId), Is.True);
			foreach (var iBlock in m_indicesOfChangedBlocks)
			{
				var changedContBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iBlock];
				var i = continuationBlocks.IndexOf(changedContBlock);
				Assert.That(i >= 0, Is.True);
				continuationBlocks.RemoveAt(i);
			}
			Assert.That(continuationBlocks, Is.Empty);
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiBlockQuote_SetToNarrator_QuoteChainBrokenIntoIndividual()
		{
			Func<Block, bool> isAmbiguousStartBlock = block => block.MultiBlockQuote == MultiBlockQuote.Start && block.CharacterId == kAmbiguousCharacter;
			// Find a matchup that has a multi-block quote that is ambiguous.
			while ((m_model.CurrentReferenceTextMatchup == null ||
					!m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Any(b => isAmbiguousStartBlock(b))) &&
				m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
			}

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchup, Is.Not.Null);
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.IndexOf(b => isAmbiguousStartBlock(b));
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			var continuationBlocks = matchup.CorrelatedBlocks.Skip(indexOfQuoteStartBlock + 1).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote).ToList();
			Assert.That(continuationBlocks.Any(), Is.True);
			m_indicesOfChangedBlocks.Clear();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, AssignCharacterViewModel.Character.Narrator);

			Assert.That(startBlock.CharacterIs(m_model.CurrentBookId, StandardCharacter.Narrator), Is.True);
			Assert.That(startBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(continuationBlocks.TrueForAll(b => b.CharacterIs(m_model.CurrentBookId, StandardCharacter.Narrator)), Is.True);
			Assert.That(continuationBlocks.TrueForAll(b => !b.IsContinuationOfPreviousBlockQuote), Is.True);
			foreach (var iBlock in m_indicesOfChangedBlocks)
			{
				var changedContBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iBlock];
				var i = continuationBlocks.IndexOf(changedContBlock);
				Assert.That(i >= 0, Is.True);
				continuationBlocks.RemoveAt(i);
			}
			Assert.That(continuationBlocks, Is.Empty);
		}

		[Test]
		public void SetReferenceTextMatchupCharacter_BlockIsStartOfMultiBlockQuoteButMatchupContainsNoContinuationBlocks_DoesNotCrash()
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
			Assert.That(matchup, Is.Not.Null);
			Assert.That(isStartBlockAtEndOfMatchup(matchup.CorrelatedBlocks.Last()), Is.True);
			var indexOfQuoteStartBlock = matchup.CorrelatedBlocks.Count - 1;
			var startBlock = matchup.CorrelatedBlocks[indexOfQuoteStartBlock];
			m_indicesOfChangedBlocks.Clear();

			m_model.SetReferenceTextMatchupCharacter(indexOfQuoteStartBlock, new AssignCharacterViewModel.Character("Martin"));

			Assert.That(startBlock.CharacterId, Is.EqualTo("Martin"));
			Assert.That(m_indicesOfChangedBlocks, Is.Empty);
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
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
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
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(m_bookNum, 1, 18).Single().Character,
				Is.EqualTo("God"),
				"Test setup conditions not met!");

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.OBA);
			TestProject.SimulateDisambiguationForAllBooks(testProject);

			var overrideOba = NarratorOverrides.Singleton.Books.Single(b => b.Id == bookScript.BookId).Overrides.Single();
			Assert.That(overrideOba.StartChapter, Is.EqualTo(1), "Test setup conditions not met!");
			Assert.That(overrideOba.EndChapter, Is.EqualTo(1), "Test setup conditions not met!");
			Assert.That(overrideOba.Character, Is.EqualTo(kObadiahTheProphet),
				"Test setup conditions not met!");
			Assert.That(overrideOba.StartVerse, Is.EqualTo(1), "Test setup conditions not met!");
			Assert.That(overrideOba.StartBlock, Is.EqualTo(2), "Test setup conditions not met!");
			Assert.That(overrideOba.EndVerse, Is.EqualTo(21), "Test setup conditions not met!");
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
			Assert.That(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 18)), Is.True, "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters().ToList();

			// Verify
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].CharacterId, Is.EqualTo("God"), "God is first because the list is alphabetical (with narrator last)");
			Assert.That(result[1].CharacterId, Is.EqualTo(kObadiahTheProphet));
			Assert.That(result[2].LocalizedDisplay, Is.EqualTo("narrator (OBA)"));
		}

		[Test]
		public void GetUniqueCharactersForCurrentReference_InBlockWithNarratorOverride_ResultIncludesOverrideCharacter()
		{
			// Setup
			Assert.That(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 18)), Is.True, "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharactersForCurrentReference().ToList();

			// Verify
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].LocalizedDisplay, Is.EqualTo("narrator (OBA)"),
				"GetUniqueCharactersForCurrentReference should always put the narrator first.");
			Assert.That(result[1].CharacterId, Is.EqualTo("God"), "God is second because remaining items in the list are alphabetical");
			Assert.That(result[2].CharacterId, Is.EqualTo(kObadiahTheProphet));
		}

		[Test]
		public void GetUniqueCharacters_MatchingFilterInVerseWithNarratorOverride_ResultStartsWithMatchingCharactersIncludingOverrideCharacter()
		{
			// Setup
			Assert.That(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 1)), Is.True,
				"Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters("Obad").ToList();

			// Verify
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result[0].CharacterId.StartsWith("Obadiah"), Is.True,
				"The first match happens to be from a different book. List of matches is alphabetic");
			Assert.That(result[1].CharacterId, Is.EqualTo(kObadiahTheProphet));
			Assert.That(result[2].CharacterId, Is.EqualTo("God"),
				"God is second because he is a Normal character in OBA 1:1");
			Assert.That(result[3].LocalizedDisplay, Is.EqualTo("narrator (OBA)"),
				"GetUniqueCharacters should always include the narrator at the end, if not " +
				"already in the list.");
		}

		[Test]
		public void GetUniqueCharacters_MatchingCharacterHasAliasedAndNonAliasedMatch_ResultIncludesNonAliasedRatherThanAliasedCharacter()
		{
			// SUT
			var result = m_model.GetUniqueCharacters("script").ToList();

			// Verify
			Assert.That(result.Any(c => c.CharacterId == "scripture" && c.LocalizedAlias == null), Is.True);
			Assert.That(result.Any(c => c.CharacterId == "scripture" && c.LocalizedAlias != null), Is.False);
		}

		[Test]
		public void GetUniqueCharacters_NonMatchingFilterInBlockWithNarratorOverride_ResultIncludesOverrideCharacterAtEnd()
		{
			// Setup
			Assert.That(m_model.TryLoadBlock(new VerseRef(m_bookNum, 1, 1)), Is.True, "Test setup conditions not met!");

			// SUT
			var result = m_model.GetUniqueCharacters("Holy S").ToList();

			// Verify
			Assert.That(result.Count >= 4, Is.True);
			var iStartOfNonMatching = result.Count -3;
			Assert.That(result[iStartOfNonMatching].CharacterId, Is.EqualTo("God"),
				"God should be first non-match because this part of list is alphabetical.");
			Assert.That(result[iStartOfNonMatching + 1].CharacterId, Is.EqualTo(kObadiahTheProphet),
				$"{kObadiahTheProphet} should be " +
				"second non-match because this part of list is alphabetical.");
			Assert.That(result[iStartOfNonMatching + 2].LocalizedDisplay,
				Is.EqualTo("narrator (OBA)"),
				"GetUniqueCharacters should always include the narrator at the end, if not " +
				"already in the list.");
		}
	}
}
