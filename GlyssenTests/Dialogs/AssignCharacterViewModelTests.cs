using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;
using Paratext;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class AssignCharacterViewModelTests
	{
		private Project m_testProject;
		private AssignCharacterViewModel m_model;
		private bool m_fullProjectRefreshRequired;

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
			m_model = new AssignCharacterViewModel(m_testProject);
			m_model.SetUiStrings("narrator ({0})",
				"book title or chapter ({0})",
				"introduction ({0})",
				"section head ({0})",
				"normal");
			m_model.BackwardContextBlockCount = 10;
			m_model.ForwardContextBlockCount = 10;
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

		[Test]
		public void SetMode_AlternateBetweenModes_AssignedBlockCountDoesNotGrowContinuously()
		{
			Assert.AreEqual(0, m_model.AssignedBlockCount);

			m_model.SetCharacterAndDelivery(new AssignCharacterViewModel.Character("Jesus"), AssignCharacterViewModel.Delivery.Normal);

			Assert.AreEqual(1, m_model.AssignedBlockCount);

			m_model.Mode = BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed;
			Assert.AreEqual(0, m_model.AssignedBlockCount);

			m_model.Mode = BlocksToDisplay.NeedAssignments;
			Assert.AreEqual(1, m_model.AssignedBlockCount);

			// The assignment call above actually affects 5 blocks because they are all in the same quote.
			m_model.Mode = BlocksToDisplay.AllScripture;
			Assert.AreEqual(1, m_model.AssignedBlockCount);

			m_model.Mode = BlocksToDisplay.HotSpots | BlocksToDisplay.ExcludeUserConfirmed;
			Assert.AreEqual(0, m_model.AssignedBlockCount);
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
			block1.CharacterId = CharacterVerseData.kUnknownCharacter;
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
			m_model.Mode = BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void AreAllAssignmentsComplete_FalseBeforeTrueAfterSettingSingleVoice()
		{
			m_fullProjectRefreshRequired = true;
			Assert.IsFalse(m_model.AreAllAssignmentsComplete);
			m_model.SetCurrentBookSingleVoice(true);
			Assert.IsTrue(m_model.AreAllAssignmentsComplete);
		}

		[Test]
		public void SplitBlock_SingleSplit_RelevantBlocksUpdated()
		{
			m_fullProjectRefreshRequired = true;
			m_model.Mode = BlocksToDisplay.NeedAssignments;
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

			m_model.SplitBlock(new[] { new BlockSplitData(1, currentBlock, "2", 6) }, GetListOfCharacters(2, new string[0]));
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

			model.Mode = BlocksToDisplay.NeedAssignments;
			var currentBlock = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var nextBlock = model.CurrentBlock;
			model.LoadNextRelevantBlock();
			var nextNextBlock = model.CurrentBlock;
			model.LoadPreviousRelevantBlock();
			model.LoadPreviousRelevantBlock();
			Assert.AreEqual(currentBlock, model.CurrentBlock);
			var preSplit = currentBlock.Clone();

			model.SplitBlock(new[] { new BlockSplitData(1, currentBlock, "2", 6) }, GetListOfCharacters(2, new string[0]));
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
			m_model.Mode = BlocksToDisplay.NeedAssignments;
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
			m_model.Mode = BlocksToDisplay.NeedAssignments;
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
			m_model.Mode = BlocksToDisplay.NeedAssignments;
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
			}, GetListOfCharacters(3, new[] { currentBlockCharacterId }));

			Assert.False(m_model.IsCurrentBlockRelevant);
			Assert.AreEqual(currentBlockCharacterId, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.True(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.True(m_model.CurrentBlock.ChapterNumber == 1 && m_model.CurrentBlock.InitialStartVerseNumber == 5);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, m_model.CurrentBlock.CharacterId);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(firstRelevantBlockAfterTheBlockToSplit, m_model.CurrentBlock);
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_DoesNotThrow()
		{
			m_fullProjectRefreshRequired = true;

			m_model.CurrentBlockIndexInBook = 4;
			var block1 = m_testProject.Books[0].Blocks[4]; ;
			Assert.AreEqual(block1, m_model.CurrentBlock);
			var block2 = m_testProject.Books[0].Blocks[5];

			Assert.AreEqual(2, block1.InitialStartVerseNumber, "If this fails, update the test to reflect the test data.");

			Assert.DoesNotThrow(() =>
				m_model.SplitBlock(new[]
				{
					new BlockSplitData(1, block1, "2", 13),
					new BlockSplitData(2, block2, null, 0),
					new BlockSplitData(3, block2, "2", 10)
				}, GetListOfCharacters(4, new string[0]))
			);
		}

		[Test]
		public void SplitBlock_MultiBlockQuoteMultipleSplitsSplitAtParagraph_SplitCorrectly()
		{
			m_fullProjectRefreshRequired = true;

			m_model.CurrentBlockIndexInBook = 4;
			var block1 = m_testProject.Books[0].Blocks[4]; ;
			Assert.AreEqual(block1, m_model.CurrentBlock);
			var block2 = m_testProject.Books[0].Blocks[5];

			var text1 = block1.GetText(false);
			var text2 = block2.GetText(false);

			// make sure the data is good to start with
			Assert.AreEqual(2, block1.InitialStartVerseNumber, "If this fails, update the test to reflect the test data.");
			Assert.AreEqual(MultiBlockQuote.Start, block1.MultiBlockQuote, "If this fails, update the test to reflect the test data.");
			Assert.AreEqual(MultiBlockQuote.Continuation, block2.MultiBlockQuote, "If this fails, update the test to reflect the test data.");

			m_model.SplitBlock(new[]
			{
				new BlockSplitData(1, block1, "2", 13),
				new BlockSplitData(2, block2, null, 0),
				new BlockSplitData(3, block2, "2", 10)
			}, GetListOfCharacters(4, new string[0]));

			// check the text
			Assert.AreEqual(text1.Substring(0, 13), m_testProject.Books[0].Blocks[4].GetText(false));
			Assert.AreEqual(text1.Substring(13), m_testProject.Books[0].Blocks[5].GetText(false));
			Assert.AreEqual(text2.Substring(0, 10), m_testProject.Books[0].Blocks[6].GetText(false));
			Assert.AreEqual(text2.Substring(10), m_testProject.Books[0].Blocks[7].GetText(false));

			// check the multi-block quote
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[4].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[5].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, m_testProject.Books[0].Blocks[6].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, m_testProject.Books[0].Blocks[7].MultiBlockQuote);
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
			
			model.SplitBlock(new[] { new BlockSplitData(1, currentBlock, verse, splitIndex) },
				GetListOfCharacters(2, new [] {currentBlock.CharacterId, currentBlock.CharacterId}));

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
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
			Assert.AreEqual(3, m_model.GetUniqueCharactersForCurrentReference(false).Count()); // Includes narrator
			Assert.IsNull(m_model.GetCharacterToSelectForCurrentBlock(m_model.GetUniqueCharactersForCurrentReference(false)));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetCharacterToSelectForCurrentBlock_BlockIsRemainingAmbiguousBlockOfTwoDialogueQuotesInVerse_ReturnsOtherExpectedCharacter(int indexOfCharacterToAssignToFirstBlock)
		{
			m_fullProjectRefreshRequired = true;

			FindRefInMark(5, 9);
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
			var posibleCharactersForMark59 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var posibleSpeakingCharactersForMark59 = posibleCharactersForMark59.Where(c => !c.IsNarrator).ToList();
			Assert.AreEqual(2, posibleSpeakingCharactersForMark59.Count);
			m_model.SetCharacterAndDelivery(posibleSpeakingCharactersForMark59[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			posibleSpeakingCharactersForMark59.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear());
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
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
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
			var posibleCharactersForMark541 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
			var unusedCharactersForMark541 = posibleCharactersForMark541.ToList();
			Assert.AreEqual(2, unusedCharactersForMark541.Count);
			m_model.SetCharacterAndDelivery(unusedCharactersForMark541[indexOfCharacterToAssignToFirstBlock], AssignCharacterViewModel.Delivery.Normal);
			unusedCharactersForMark541.RemoveAt(indexOfCharacterToAssignToFirstBlock);
			Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear());
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
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
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
				var posibleCharactersForActs837 = m_model.GetUniqueCharactersForCurrentReference(false).ToList();
				Assert.AreEqual(3, posibleCharactersForActs837.Count);
				m_model.SetCharacterAndDelivery(posibleCharactersForActs837.Where(c => !c.IsNarrator).ElementAt(indexOfCharacterToAssignToFirstBlock),
					AssignCharacterViewModel.Delivery.Normal);
				Assert.IsFalse(m_model.CurrentBlock.CharacterIsUnclear());
				m_model.LoadNextRelevantBlock();
				Assert.IsTrue(m_model.CurrentBlock.CharacterIsUnclear());
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
		public void SetCharacterAndDelivery_BlockMatchupIsSet_OriginalAndCorrelatedAnchorBlocksAreModified()
		{
			m_fullProjectRefreshRequired = true;
			FindRefInMark(9, 21);
			var originalAnchorBlock = m_model.CurrentBlock;
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsTrue(originalAnchorBlock.CharacterIsUnclear());
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
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear()));
			Assert.IsTrue(m_model.CurrentBlock.ChapterNumber > 9);
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
	}
}
