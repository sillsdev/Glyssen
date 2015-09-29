using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;

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
			var characters = m_model.GetCharactersForCurrentReference(false).ToList();
			Assert.AreEqual(1, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReference_NonQuoteBlockContainingVerseWitExpectedQuote_GetsNarratorFollowedByOtherSpeakersInVersesInBlock()
		{
			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;
			Assert.AreEqual("MRK 1:16-17", m_model.GetBlockReferenceString());
			var characters = m_model.GetCharactersForCurrentReference().ToList();
			Assert.AreEqual(2, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWithContext_GetsNarratorOnly()
		{
			// Note: Default forward/backward context is 10 blocks.
			var characters = m_model.GetCharactersForCurrentReference().ToList();
			Assert.AreEqual(1, characters.Count);
			Assert.IsTrue(characters[0].IsNarrator);
			Assert.IsFalse(characters[0].ProjectSpecific);
		}

		[Test]
		public void GetCharactersForCurrentReference_UnexpectedQuoteWith20BlockForwardContext_GetsNarratorAndJesus()
		{
			m_model.ForwardContextBlockCount = 20;
			var characters = m_model.GetCharactersForCurrentReference().ToList();
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
			var characters = m_model.GetCharactersForCurrentReference().ToList();
			Assert.AreEqual(3, characters.Count);
			Assert.AreEqual("demons (Legion)/man delivered from Legion of demons", characters[0].CharacterId);
			Assert.IsFalse(characters[0].ProjectSpecific);
			Assert.AreEqual("Jesus", characters[1].CharacterId);
			Assert.IsFalse(characters[1].ProjectSpecific);
			Assert.IsTrue(characters[2].IsNarrator);
			Assert.IsFalse(characters[2].ProjectSpecific);
		}

		[Test]
		public void GetUniqueCharacters_AmbiguousQuoteNoFilter_GetsAllCharactersInMark()
		{
			FindRefInMark(5, 9);
			var characters = m_model.GetUniqueCharacters().ToList();
			Assert.AreEqual(79, characters.Count);
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
			var characters = m_model.GetCharactersForCurrentReference().ToList();
			Assert.AreEqual(3, characters.Count);
			Assert.AreEqual("Herodias' daughter", characters[0].CharacterId);
			Assert.AreEqual("alias", characters[0].Alias);
			Assert.AreEqual("Herodias", characters[1].CharacterId);
			Assert.IsNull(characters[1].Alias);
			Assert.IsTrue(characters[2].IsNarrator);
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
			m_model.GetCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("man with evil spirit"));
			Assert.AreEqual(1, deliveries.Count());
			Assert.AreEqual(AssignCharacterViewModel.Delivery.Normal, deliveries.First());
		}

		[Test]
		public void GetDeliveriesForCharacter_CharacterWithOneDelivery_GetsDeliveryAndNormal()
		{
			FindRefInMark(5, 9);
			m_model.GetCharactersForCurrentReference();
			var deliveries = m_model.GetDeliveriesForCharacter(new AssignCharacterViewModel.Character("Jesus"));
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
			m_model.GetCharactersForCurrentReference();
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
			m_model.GetCharactersForCurrentReference();
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
			block1.CharacterId = CharacterVerseData.AmbiguousCharacter;
			Assert.IsFalse(m_model.IsModified(null, null));
		}

		[Test]
		public void IsModified_BlockCharacterUnknownCharacterNull_ReturnsFalse()
		{
			var block1 = m_model.CurrentBlock;
			block1.CharacterId = CharacterVerseData.UnknownCharacter;
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
		public void SetCurrentBookSingleVoice_TrueNoSubsequentBooks_RemainingBlocksAssignedToNarratorAndCurrentBlockIsUnchanged()
		{
			m_fullProjectRefreshRequired = true;
			var currentBlock = m_model.CurrentBlock;
			m_model.SetCurrentBookSingleVoice(true);
			foreach (var block in m_testProject.IncludedBooks[0].GetScriptBlocks())
			{
				Assert.IsFalse(block.CharacterIsUnclear(), block.GetText(true));
				if (block.UserConfirmed)
					Assert.IsTrue(block.CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator), block.GetText(true));
			}
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
		public void SplitBlock_RelevantBlocksUpdated()
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

			var newBlock = m_model.SplitBlock(currentBlock, "2", 6);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(newBlock, m_model.CurrentBlock);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextBlock, m_model.CurrentBlock);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(nextNextBlock, m_model.CurrentBlock);
		}

		private void FindRefInMark(int chapter, int verse)
		{
			while (m_model.CurrentBlock.ChapterNumber != chapter || m_model.CurrentBlock.InitialStartVerseNumber != verse)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(chapter, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(verse, m_model.CurrentBlock.InitialStartVerseNumber);			
		}
	}
}
