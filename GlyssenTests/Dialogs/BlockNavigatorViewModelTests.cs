﻿using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Scripture;
using SIL.Windows.Forms;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	class BlockNavigatorViewModelTests
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.JUD);
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NeedAssignments);
			m_model.BackwardContextBlockCount = 10;
			m_model.ForwardContextBlockCount = 10;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
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
		public void LoadNextRelevantBlock_DataHasSomeContinuationBlocksNeedingAssignment_ContinuationBlocksNeverGetLoaded()
		{
			Assert.IsTrue(m_testProject.IncludedBooks.SelectMany(b => b.Blocks).Any(b => b.CharacterIsUnclear() && b.MultiBlockQuote == MultiBlockQuote.Continuation),
				"Test data does not have the required characteristics. Need at least one unassigned block that is a continuation of a quote from earlier paragraph.");

			do
			{
				Assert.IsFalse(m_model.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation);
				m_model.LoadNextRelevantBlock();
			} while (!m_model.CanNavigateToNextRelevantBlock);
			Assert.IsFalse(m_model.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation);
		}

		[Test]
		public void LoadNextRelevantBlockInSubsequentBook_HasFurtherBooks_HasNoFurtherRelevantBlocks_CallingMethodDoesNothing()
		{
			// Validate our setup
			Assert.AreEqual(2, m_testProject.IncludedBooks.Count);
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual(m_testProject.IncludedBooks[0].BookId, m_model.CurrentBookId);

			// Run test
			var currentBlockBeforeCall = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlockInSubsequentBook();
			Assert.AreEqual(currentBlockBeforeCall, m_model.CurrentBlock);
		}

		[Test]
		public void SetMode_CurrentBlockIsRelevantInNewMode_KeepCurrentBlock()
		{
			m_model.Mode = BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed;
			m_model.LoadNextRelevantBlock();
			var block = m_model.CurrentBlock;

			m_model.Mode = BlocksToDisplay.NeedAssignments;
			Assert.AreEqual(block, m_model.CurrentBlock);

			m_model.Mode = BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed;
			Assert.AreEqual(block, m_model.CurrentBlock);
		}

		[Test]
		public void SetMode_MissingExpectedQuote_LoadsBlocksWithMissingExpectedQuotes()
		{
			m_model.Mode = BlocksToDisplay.MissingExpectedQuote;

			// Missing quote is for verse 20
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(1, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(18, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(20, m_model.CurrentBlock.LastVerse);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 30
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(1, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(29, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(31, m_model.CurrentBlock.LastVerse);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 15
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(2, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(15, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(16, m_model.CurrentBlock.LastVerse);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 9
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(3, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(7, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(11, m_model.CurrentBlock.LastVerse);
		}

		[Test]
		public void SetMode_MoreQuotesThanExpectedSpeakers_LoadsBlocksWithMoreQuotesThanExpectedSpeakers()
		{
			m_model.Mode = BlocksToDisplay.MoreQuotesThanExpectedSpeakers;

			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(1, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(2, m_model.CurrentBlock.InitialStartVerseNumber);
			FindRefInMark(2, 5);
		}

		[Test]
		public void SetMode_ExcludeUserConfirmed_UserConfirmedBlockSkipped()
		{
			var block1 = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			var block2 = m_model.CurrentBlock;
			m_model.LoadPreviousRelevantBlock();

			block1.CharacterId = "Sigmund";
			block1.UserConfirmed = true;

			m_model.Mode |= BlocksToDisplay.ExcludeUserConfirmed;

			Assert.AreEqual(block2, m_model.CurrentBlock);
		}

		[Test]
		public void SetMode_AllExpectedQuotes_StandardCharactersAndIndirectBlocksSkipped()
		{
			//MRK	5	41	Jesus	giving orders		Dialogue
			//MRK	5	41	narrator-MRK			Quotation
			//MRK	5	43	Jesus			Indirect
			//MRK	6	2	men in Nazareth synagogue	amazed		Indirect
			//MRK	6	3	men in Nazareth synagogue	amazed		Indirect
			//MRK	6	4	Jesus			Dialogue

			//Validate setup
			m_model.Mode = BlocksToDisplay.AllQuotes;
			FindRefInMark(6, 2);
			Assert.AreEqual("MRK 6:2-3", m_model.GetBlockReferenceString());
			m_model.CurrentBlockIndexInBook = 0;

			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;
			FindRefInMark(5, 41);
			m_model.LoadNextRelevantBlock();
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK 5:41", m_model.GetBlockReferenceString());
			//End validate setup

			m_model.LoadNextRelevantBlock();
			// The expected quote is actually in verse 4, but unfortunately, the filter is
			// actually "verses with expected quotes" so we stop on this block
			Assert.AreEqual("MRK 6:3-4", m_model.GetBlockReferenceString());
		}

		[Test]
		public void SetMode_AllQuotes_NonQuoteBlocksSkipped()
		{
			m_model.Mode = BlocksToDisplay.AllQuotes;
			FindRefInMark(1, 17);
			var block1 = m_model.CurrentBlock;
			Assert.AreEqual("Jesus", block1.CharacterId);
			m_model.LoadNextRelevantBlock();

			Assert.AreEqual("MRK 1:24", m_model.GetBlockReferenceString());
			m_model.LoadPreviousRelevantBlock();
			Assert.AreEqual(block1, m_model.CurrentBlock);
		}

		[Test]
		public void BlockCountForCurrentBook_TestMrk_ReturnsTrue()
		{
			int expectedCount = m_testProject.IncludedBooks[0].Blocks.Count;
			Assert.AreEqual(expectedCount, m_model.BlockCountForCurrentBook, "Test data may have been changed");
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_MinGreaterThanMax_ReturnsNegative1()
		{
			Assert.AreEqual(-1, BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				new List<BookBlockIndices>(), new BookBlockIndices(1, 2), true, 1, 0));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_PreviousBlockIsRelevant_ReturnsClosestPreviousRelevantBlock()
		{
			var relevantBlocks = new List<BookBlockIndices>();
			relevantBlocks.Add(new BookBlockIndices(1, 2));
			relevantBlocks.Add(new BookBlockIndices(1, 20));
			relevantBlocks.Add(new BookBlockIndices(2, 1));
			relevantBlocks.Add(new BookBlockIndices(2, 7));
			relevantBlocks.Add(new BookBlockIndices(2, 8));
			relevantBlocks.Add(new BookBlockIndices(2, 14));
			relevantBlocks.Add(new BookBlockIndices(3, 2));
			Assert.AreEqual(4, BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(2, 10), true, 0, relevantBlocks.Count - 1));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_NoPreviousBlockIsRelevant_ReturnsNegative1()
		{
			var relevantBlocks = new List<BookBlockIndices>();
			relevantBlocks.Add(new BookBlockIndices(2, 14));
			relevantBlocks.Add(new BookBlockIndices(3, 2));
			Assert.AreEqual(-1, BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(1, 3), true, 0, relevantBlocks.Count - 1));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_FollowingBlockIsRelevant_ReturnsClosestFollowingRelevantBlock()
		{
			var relevantBlocks = new List<BookBlockIndices>();
			relevantBlocks.Add(new BookBlockIndices(1, 2));
			relevantBlocks.Add(new BookBlockIndices(1, 20));
			relevantBlocks.Add(new BookBlockIndices(2, 1));
			relevantBlocks.Add(new BookBlockIndices(2, 7));
			relevantBlocks.Add(new BookBlockIndices(2, 8));
			relevantBlocks.Add(new BookBlockIndices(2, 14));
			relevantBlocks.Add(new BookBlockIndices(3, 2));
			Assert.AreEqual(2, BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(1, 21), false, 0, relevantBlocks.Count - 1));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_NoFollowingBlockIsRelevant_ReturnsNegative1()
		{
			var relevantBlocks = new List<BookBlockIndices>();
			relevantBlocks.Add(new BookBlockIndices(1, 2));
			relevantBlocks.Add(new BookBlockIndices(1, 20));
			relevantBlocks.Add(new BookBlockIndices(2, 1));
			relevantBlocks.Add(new BookBlockIndices(2, 7));
			relevantBlocks.Add(new BookBlockIndices(2, 8));
			relevantBlocks.Add(new BookBlockIndices(2, 14));
			relevantBlocks.Add(new BookBlockIndices(3, 2));
			Assert.AreEqual(-1, BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(3, 3), false, 0, relevantBlocks.Count - 1));
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsFirstRelevantBlock_ReturnsFalse()
		{
			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsFirstRelevantBlock_ReturnsTrue()
		{
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsSecondRelevantBlock_ReturnsTrue()
		{
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsLastRelevantBlock_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = m_testProject.IncludedBooks[0].GetScriptBlocks().Count - 1;
			m_model.LoadPreviousRelevantBlock();
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsVeryFirstBlock_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = 0;
			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsVeryLastBlockInBook_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = m_testProject.IncludedBooks[0].GetScriptBlocks().Count - 1;
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsAdHocLocationInMIddleOfBook_ReturnsTrue()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant, "If this fails, we chose a relevant block index by accident.");
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsAdHocLocationInMIddleOfBook_ReturnsTrue()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant, "If this fails, we chose a relevant block index by accident.");
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsRelevantSingleBlockQuote_StateReflectsRelevantBlock()
		{
			m_model.LoadNextRelevantBlock();
			var index = m_model.CurrentBlockIndexInBook;
			Assert.AreEqual(MultiBlockQuote.None, m_model.CurrentBlock.MultiBlockQuote, "If this fails, choose a different block.");
			m_model.LoadNextRelevantBlock();
			m_model.LoadNextRelevantBlock();
			m_model.CurrentBlockIndexInBook = index;
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(2, m_model.CurrentBlockDisplayIndex);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsNotRelevantSingleBlockQuote_StateReflectsRelevantBlock()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.AreEqual(MultiBlockQuote.None, m_model.CurrentBlock.MultiBlockQuote, "If this fails, choose a different block.");
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(0, m_model.CurrentBlockDisplayIndex);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsQuoteContinuationBlockForRelevantQuote_StateReflectsQuoteStartBlock()
		{
			var blocks = m_testProject.IncludedBooks[0].GetScriptBlocks();
			int i = 1;
			for (; i < blocks.Count; i++)
			{
				if (blocks[i].MultiBlockQuote == MultiBlockQuote.Continuation)
				{
					var quoteStart = i - 1;
					if (blocks[quoteStart].CharacterIsUnclear())
						break;
					do
					{
						i++;
					} while (blocks[i].MultiBlockQuote == MultiBlockQuote.Continuation);
				}
			}
			m_model.CurrentBlockIndexInBook = i;
			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);
			Assert.IsTrue(m_model.GetIndicesOfQuoteContinuationBlocks(m_model.CurrentBlock).Any());
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			Assert.IsTrue(m_model.CurrentBlockDisplayIndex > 0);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsNotRelevantQuoteContinuationBlock_StateReflectsQuoteStartBlock()
		{
			var blocks = m_testProject.IncludedBooks[0].GetScriptBlocks();
			int i = 1;
			for (; i < blocks.Count; i++)
			{
				if (blocks[i].MultiBlockQuote == MultiBlockQuote.Continuation)
				{
					var quoteStart = i - 1;
					if (!blocks[quoteStart].CharacterIsUnclear())
						break;
					do
					{
						i++;
					} while (blocks[i].MultiBlockQuote == MultiBlockQuote.Continuation);
				}
			}
			m_model.CurrentBlockIndexInBook = i;
			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);
			Assert.IsTrue(m_model.GetIndicesOfQuoteContinuationBlocks(m_model.CurrentBlock).Any());
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.AreEqual(0, m_model.CurrentBlockDisplayIndex);
		}

		[Test]
		public void GetIsBlockScripture_ScriptureBlock_ReturnsTrue()
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			FindRefInMark(1, 12);
			Assert.IsTrue(m_model.CurrentBlock.IsScripture);
			Assert.IsTrue(m_model.GetIsBlockScripture(m_model.CurrentBlockIndexInBook));
		}

		[Test]
		public void GetIsBlockScripture_TitleBlock_ReturnsFalse()
		{
			Assert.IsFalse(m_testProject.IncludedBooks[0].Blocks[0].IsScripture);
			Assert.IsFalse(m_model.GetIsBlockScripture(0));
		}

		[Test]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsStart_FindsLastContinuationBlock()
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.AreNotEqual(lastBlock, m_model.CurrentBlock);
			Assert.AreEqual(MultiBlockQuote.Continuation, lastBlock.MultiBlockQuote);

			m_model.LoadNextRelevantBlock(); // Goes to next block not in this multiblock quote

			Assert.AreEqual(m_model.CurrentBlockIndexInBook - 1, m_model.GetBlockIndices(lastBlock).BlockIndex);
		}

		[Test]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsNone_ReturnsCurrentBlock()
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.None)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual(MultiBlockQuote.None, m_model.CurrentBlock.MultiBlockQuote);

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.AreEqual(m_model.CurrentBlock, lastBlock);
			Assert.AreEqual(MultiBlockQuote.None, lastBlock.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.None)]
		[TestCase(MultiBlockQuote.Start)]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsStartButNextIsNoneOrStartRepresentingBadData_ReturnsCurrentBlock(MultiBlockQuote followingStatus)
		{
			m_model.Mode = BlocksToDisplay.AllScripture;
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);

			//Create bad data
			BlockNavigator navigator = (BlockNavigator)ReflectionHelper.GetField(m_model, "m_navigator");
			Block nextBlock = navigator.NextBlock();
			var originalStatus = nextBlock.MultiBlockQuote;
			nextBlock.MultiBlockQuote = followingStatus;
			navigator.PreviousBlock();

			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.AreEqual(m_model.CurrentBlock, lastBlock);

			//Reset data for other tests
			nextBlock = navigator.NextBlock();
			nextBlock.MultiBlockQuote = originalStatus;
		}

		[Test]
		public void CurrentBlockHasMissingExpectedQuote_QuoteContainsVerseBridgeNonQuoteAtEnd_NoMissingExpectedQuotes()
		{
			var blockA = new Block("ip") { MultiBlockQuote = MultiBlockQuote.None };
			var blockB = new Block("p", 1, 1) { MultiBlockQuote = MultiBlockQuote.Start };
			blockB.BlockElements.Add(new Verse("1"));
			blockB.BlockElements.Add(new ScriptText("Verse 1"));
			var blockC = new Block("p", 1, 2) { MultiBlockQuote = MultiBlockQuote.Continuation };
			blockC.BlockElements.Add(new Verse("2"));
			blockC.BlockElements.Add(new ScriptText("Verse 2"));
			blockC.BlockElements.Add(new Verse("3-4"));
			blockC.BlockElements.Add(new ScriptText("Verse 3-4"));
			var blockD = new Block("p", 1, 3, 4) { MultiBlockQuote = MultiBlockQuote.None };
			blockD.BlockElements.Add(new ScriptText("Jesus said"));
			var bookScriptJud = new BookScript("JUD", new List<Block> { blockA, blockB, blockC, blockD });
			var bookList = new List<BookScript> { bookScriptJud };

			var versesWithPotentialMissingQuote = new List<BCVRef> {new BCVRef(65, 1, 3), new BCVRef(65, 1, 4)};

			var model = new BlockNavigatorViewModel(bookList, ScrVers.English.BaseVersification);
			var navigator = (BlockNavigator)ReflectionHelper.GetField(model, "m_navigator");
			navigator.NavigateToFirstBlock();
			navigator.NextBlock();
			navigator.NextBlock();
			navigator.NextBlock();
			navigator.NextBlock();

			var found = model.CurrentBlockHasMissingExpectedQuote(versesWithPotentialMissingQuote);

			Assert.False(found);
		}

		[Test]
		public void CurrentBlockHasMissingExpectedQuote_QuoteContainsVerseBridgeNonQuoteAtBeginning_NoMissingExpectedQuotes()
		{
			var blockA = new Block("ip") {MultiBlockQuote = MultiBlockQuote.None};
			var blockB = new Block("p", 1, 1, 2) {MultiBlockQuote = MultiBlockQuote.None};
			blockB.BlockElements.Add(new Verse("1-2"));
			blockB.BlockElements.Add(new ScriptText("Jesus said,"));

			var blockC = new Block("p", 1, 1, 2) {MultiBlockQuote = MultiBlockQuote.Start};
			blockC.BlockElements.Add(new ScriptText("'This is the quote'"));
			var bookScriptJud = new BookScript("JUD", new List<Block> {blockA, blockB, blockC});
			var bookList = new List<BookScript> {bookScriptJud};

			var versesWithPotentialMissingQuote = new List<BCVRef> {new BCVRef(65, 1, 1), new BCVRef(65, 1, 2)};

			var model = new BlockNavigatorViewModel(bookList, ScrVers.English.BaseVersification);
			var navigator = (BlockNavigator) ReflectionHelper.GetField(model, "m_navigator");
			navigator.NavigateToFirstBlock();
			navigator.NextBlock();

			var found = model.CurrentBlockHasMissingExpectedQuote(versesWithPotentialMissingQuote);
			Assert.False(found);

			navigator.NextBlock();
			found = model.CurrentBlockHasMissingExpectedQuote(versesWithPotentialMissingQuote);
			Assert.False(found);
		}

		private void FindRefInMark(int chapter, int verse)
		{
			while (m_model.CurrentBlock.ChapterNumber < chapter || m_model.CurrentBlock.InitialStartVerseNumber != verse)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(chapter, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(verse, m_model.CurrentBlock.InitialStartVerseNumber);
		}
	}
}
