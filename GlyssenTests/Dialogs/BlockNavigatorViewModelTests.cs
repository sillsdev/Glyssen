using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenTests.Properties;
using NUnit.Framework;
using Paratext;
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
		public void LoadNextRelevantBlock_OnFirstRelevantBlockInRainbowGroupThatCoversMultipleRelevantBlocks_LoadsNextBlockThatIsNotPartOfCurrentRainbowGroup()
		{
			m_model.AttemptRefBlockMatchup = true;
			HashSet<Block> blocksInPreviousRainbowGroup = null;
			do
			{
				if (m_model.CurrentReferenceTextMatchup != null)
				{
					if (blocksInPreviousRainbowGroup != null)
						Assert.IsFalse(blocksInPreviousRainbowGroup.Intersect(m_model.CurrentReferenceTextMatchup.OriginalBlocks).Any());
					blocksInPreviousRainbowGroup = new HashSet<Block>(m_model.CurrentReferenceTextMatchup.OriginalBlocks);
				}
				else
					blocksInPreviousRainbowGroup = null;
				m_model.LoadNextRelevantBlock();
				// It's annoying and unfortunate that we have to call this explicitly, but for the timing of the notifications to work
				// out in the actual production code, the AssignCharacterViewModel, rather than the BlockNavigatorViewModel itself, is
				// responsible for calling this after the block is loaded.
				m_model.SetBlockMatchupForCurrentVerse();
			} while (m_model.CanNavigateToNextRelevantBlock);
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
		public void LoadPreviousRelevantBlock_OnLastRelevantBlockInRainbowGroupThatCoversMultipleRelevantBlocks_LoadsPreviousBlockThatIsNotPartOfCurrentRainbowGroup()
		{
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			m_model.AttemptRefBlockMatchup = true;
			HashSet<Block> blocksInFollowingRainbowGroup = null;
			do
			{
				if (m_model.CurrentReferenceTextMatchup != null)
				{
					if (blocksInFollowingRainbowGroup != null)
						Assert.IsFalse(blocksInFollowingRainbowGroup.Intersect(m_model.CurrentReferenceTextMatchup.OriginalBlocks).Any());
					blocksInFollowingRainbowGroup = new HashSet<Block>(m_model.CurrentReferenceTextMatchup.OriginalBlocks);
				}
				else
					blocksInFollowingRainbowGroup = null;
				m_model.LoadPreviousRelevantBlock();
				// It's annoying and unfortunate that we have to call this explicitly, but for the timing of the notifications to work
				// out in the actual production code, the AssignCharacterViewModel, rather than the BlockNavigatorViewModel itself, is
				// responsible for calling this after the block is loaded.
				m_model.SetBlockMatchupForCurrentVerse();
			} while (m_model.CanNavigateToPreviousRelevantBlock);
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
			Assert.AreEqual(20, m_model.CurrentBlock.LastVerseNum);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 30
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(1, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(29, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(31, m_model.CurrentBlock.LastVerseNum);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 15
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(2, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(15, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(16, m_model.CurrentBlock.LastVerseNum);

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 9
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(3, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(7, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(11, m_model.CurrentBlock.LastVerseNum);
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
		public void CanNavigateToNextRelevantBlock_OnBlockWhoseRainbowGroupCoversLastBlock_ReturnsFalse()
		{
			m_model.AttemptRefBlockMatchup = false;
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(m_model.RelevantBlockCount - 1, m_model.CurrentBlockDisplayIndex);

			m_model.AttemptRefBlockMatchup = true;

			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(m_model.RelevantBlockCount, m_model.CurrentBlockDisplayIndex);
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
		public void SetCurrentBlockIndexInBook_BlockPrecedesCurrentMatchup_StateReflectsCorrectBlock()
		{
			m_model.AttemptRefBlockMatchup = true;
			FindRefInMark(9, 21);

			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook - 1;
			var expectedBlock = m_testProject.IncludedBooks.Single(b => b.BookId == "MRK").GetScriptBlocks()[indexOfBlockToSelect];
			
			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.AreEqual(expectedBlock, m_model.CurrentBlock);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockFollowsCurrentMatchup_StateReflectsCorrectBlock()
		{
			m_model.AttemptRefBlockMatchup = true;
			FindRefInMark(9, 21);

			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook + m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count + 1;
			var expectedBlock = m_testProject.IncludedBooks.Single(b => b.BookId == "MRK")
				.GetScriptBlocks()[indexOfBlockToSelect - m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting];

			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.AreEqual(expectedBlock, m_model.CurrentBlock);
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

		[Test]
		public void AttemptRefBlockMatchup_SetToTrue_SetsCurrentReferenceTextMatchup()
		{
			m_model.AttemptRefBlockMatchup = false;
			FindRefInMark(8, 5);
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToFalse_ClearsCurrentReferenceTextMatchup()
		{
			m_model.AttemptRefBlockMatchup = true;
			FindRefInMark(8, 5);
			m_model.AttemptRefBlockMatchup = false;
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_AttemptRefBlockMatchupIsFalse_DoesNothing()
		{
			m_model.AttemptRefBlockMatchup = false;
			FindRefInMark(8, 5);
			m_model.SetBlockMatchupForCurrentVerse();
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_NoSplits_NoChangeToNumberOfBlocksCurrentBlockOrRelevantBlocks()
		{
			m_model.AttemptRefBlockMatchup = true;
			FindRefInMark(8, 5);
			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origCurrentBlock = m_model.CurrentBlock;
			var origRelevantBlock = m_model.RelevantBlockCount;
			m_model.SetBlockMatchupForCurrentVerse();
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(0, m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting);
			Assert.AreEqual(4, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count);
			Assert.AreEqual(5, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum);
			Assert.AreEqual(origBlockCount, m_model.BlockCountForCurrentBook);
			Assert.AreEqual(origCurrentBlock, m_model.CurrentBlock);
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount);
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_VerseHasOnlyOneBlock_CurrentReferenceTextMatchupHasOneBlock()
		{
			m_model.AttemptRefBlockMatchup = false;
			// Find Mark 1:12
			int i;
			for (i = 0; m_testProject.Books[0].GetScriptBlocks()[i].LastVerseNum < 12; i++) { }
			m_model.CurrentBlockIndexInBook = i;

			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origAnchorBlock = m_model.CurrentBlock;
			Assert.AreEqual(13, origAnchorBlock.LastVerseNum);

			m_model.AttemptRefBlockMatchup = true;
			m_model.SetBlockMatchupForCurrentVerse();
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(2, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count);
			Assert.AreEqual(1, m_model.CurrentReferenceTextMatchup.OriginalBlocks.Count());
			Assert.AreEqual(origBlockCount + 1, m_model.BlockCountForCurrentBook);
			Assert.AreNotEqual(origAnchorBlock, m_model.CurrentBlock);
			Assert.IsTrue(origAnchorBlock.GetText(true).StartsWith(m_model.CurrentBlock.GetText(true)));
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_ReferenceTextCausesSplitInVernacular_SplitBlocksAddedToNumberOfBlocksAndRelevantBlocks()
		{
			m_model.AttemptRefBlockMatchup = false;
			FindRefInMark(9, 21);
			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origRelevantBlock = m_model.RelevantBlockCount;
			var origAnchorBlock = m_model.CurrentBlock;
			m_model.AttemptRefBlockMatchup = true;
			m_model.SetBlockMatchupForCurrentVerse();
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(5, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count,
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.AreEqual(20, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[0].LastVerseNum,
				"Narrator text at start of verse 21 begins in verse 20.");
			Assert.AreEqual(21, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1].InitialStartVerseNumber,
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.AreEqual(22, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum,
				"Final quote in vernacular spills over into verse 22");
			Assert.AreEqual(22, m_model.GetLastVerseInCurrentQuote());
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last(), m_model.GetLastBlockInCurrentQuote());
			Assert.AreEqual(origBlockCount + 1, m_model.BlockCountForCurrentBook);
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock, m_model.CurrentBlock);
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.ElementAt(2), m_model.CurrentBlock);
			Assert.AreEqual(origAnchorBlock.GetText(true), m_model.CurrentBlock.GetText(true));
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount,
				"The relevant block count should not go up unless/until the existing blocks are replaced by the Correlated Blocks.");
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NoBlockMatchup_ThrowsInvalidOperationException()
		{
			m_model.AttemptRefBlockMatchup = false;
			FindRefInMark(9, 21);
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.AreEqual("No current reference text block matchup!", e.Message);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_BlockMatchupAlreadyApplied_ThrowsInvalidOperationException()
		{
			FindRefInMark(8, 5);
			m_model.AttemptRefBlockMatchup = true;
			m_model.CurrentReferenceTextMatchup.SetReferenceText(0, "Some random text: " + Guid.NewGuid());
			m_model.ApplyCurrentReferenceTextMatchup();
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.AreEqual("Current reference text block matchup has no outstanding changes!", e.Message);
		}

		[Test]
		public void GetNthBlockInCurrentBook_VernacularBlocksSplitByMatchupWithReferenceText_BlocksComeFromMatchupAndSubsequentBlocksUseAdjustedIndex()
		{
			FindRefInMark(9, 21);

			var indexOfPrecedingBlock = m_model.CurrentBlockIndexInBook;
			while (m_model.GetNthBlockInCurrentBook(indexOfPrecedingBlock).LastVerseNum == 21)
				indexOfPrecedingBlock--;
			var origPrecedingBlock = m_model.GetNthBlockInCurrentBook(indexOfPrecedingBlock);

			var origIndexOfFollowingBlock = m_model.CurrentBlockIndexInBook;
			while (m_model.GetNthBlockInCurrentBook(origIndexOfFollowingBlock).LastVerseNum <= 22)
				origIndexOfFollowingBlock++;
			var origFollowingBlock = m_model.GetNthBlockInCurrentBook(origIndexOfFollowingBlock);

			m_model.AttemptRefBlockMatchup = true;
			Assert.AreEqual(origPrecedingBlock, m_model.GetNthBlockInCurrentBook(indexOfPrecedingBlock));
			int c = 0;
			foreach (var correlatedBlock in m_model.CurrentReferenceTextMatchup.CorrelatedBlocks)
			{
				Assert.AreEqual(correlatedBlock, m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup + c), c.ToString());
				c++;
			}
			Assert.AreEqual(origFollowingBlock, m_model.GetNthBlockInCurrentBook(origIndexOfFollowingBlock + m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting));
		}

		private void FindRefInMark(int chapter, int verse)
		{
			FindRefInMark(m_model, chapter, verse);
		}

		internal static void FindRefInMark(BlockNavigatorViewModel model, int chapter, int verse)
		{
			var restore = model.AttemptRefBlockMatchup;
			model.AttemptRefBlockMatchup = false;
			while (model.CurrentBlock.ChapterNumber < chapter || model.CurrentBlock.InitialStartVerseNumber != verse)
			{
				if (model.IsCurrentBlockRelevant && model.CurrentBlockDisplayIndex == model.RelevantBlockCount)
					throw new Exception("Could not find Mark " + chapter + ":" + verse);
				model.LoadNextRelevantBlock();
			}
			Assert.AreEqual("MRK", model.CurrentBookId);
			Assert.AreEqual(chapter, model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(verse, model.CurrentBlock.InitialStartVerseNumber);
			model.AttemptRefBlockMatchup = restore;
		}
	}

	/// <summary>
	/// JUD doesn't have any ambiguous/unexpected blocks, so we use Acts in this fixture in order to have more than
	/// one book with relevant blocks. Also, tests in this fixture can modify the test project data, so the folder
	/// is cleaned up each time.
	/// </summary>
	[TestFixture]
	class BlockNavigatorViewModelTestsForMarkAndActs
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.ACT);
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NeedAssignments);
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void LoadNextRelevantBlock_FollowingInsertionOfBlockByApplyingMatchupWithSplits_LoadsARelevantBlock()
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			int displayIndex = m_model.CurrentBlockDisplayIndex;
			m_model.AttemptRefBlockMatchup = true;
			Assert.AreEqual(displayIndex, m_model.CurrentBlockDisplayIndex);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			m_model.ApplyCurrentReferenceTextMatchup();
			Assert.AreEqual(displayIndex, m_model.CurrentBlockDisplayIndex);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			do
			{
				m_model.LoadNextRelevantBlock();
				Assert.AreEqual(++displayIndex, m_model.CurrentBlockDisplayIndex);
				Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			} while (m_model.CurrentBookId == "MRK");
			Assert.AreEqual("ACT", m_model.CurrentBookId);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
		}

		[TestCase(BlocksToDisplay.NeedAssignments)]
		[TestCase(BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed)]
		public void ApplyCurrentReferenceTextMatchup_Simple_SetsReferenceTextsForBlocksInGroupAndUpdatesState(BlocksToDisplay filterMode)
		{
			m_model.Mode = filterMode;

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 8, 5);
			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origCurrentBlock = m_model.CurrentBlock;
			var origRelevantBlock = m_model.RelevantBlockCount;
			var origBlockDisplayIndex = m_model.CurrentBlockDisplayIndex;
			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchupForMark85);
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			matchupForMark85.SetReferenceText(0, "Some random text: " + Guid.NewGuid());

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(matchupForMark85, m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(origCurrentBlock, m_model.CurrentBlock);
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.IsFalse(matchupForMark85.HasOutstandingChangesToApply);
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentBlockDisplayIndex);
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount);
			Assert.AreEqual(origBlockCount, m_model.BlockCountForCurrentBook);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
		}

		[TestCase(BlocksToDisplay.NeedAssignments)]
		[TestCase(BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed)]
		public void ApplyCurrentReferenceTextMatchup_Splits_BlocksInBookReplacedAndCurrentBlockNoLongerComesFromMatchupAndSubsequentRelevantBlockIndicesIncremented(BlocksToDisplay filterMode)
		{
			m_model.Mode = filterMode;
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origRelevantBlock = m_model.RelevantBlockCount;
			var origBlockDisplayIndex = m_model.CurrentBlockDisplayIndex;
			m_model.AttemptRefBlockMatchup = true;
			var matchupForMark921 = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchupForMark921);
			Assert.AreEqual(5, matchupForMark921.CorrelatedBlocks.Count,
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentBlockDisplayIndex);
			var origTextOfFirstBlockInVerse = m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup).GetText(true);
			var origCurrentBlockText = m_model.CurrentBlock.GetText(true);
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
			
			m_model.ApplyCurrentReferenceTextMatchup();
			
			Assert.AreEqual(matchupForMark921, m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(0, matchupForMark921.CountOfBlocksAddedBySplitting);
			Assert.IsFalse(matchupForMark921.HasOutstandingChangesToApply);
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentBlockDisplayIndex);
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount,
				"Since the block that was split did not match the filter (i.e., was not relevant), the relevant block count should not change.");
			Assert.AreEqual(origBlockCount + 1, m_model.BlockCountForCurrentBook);
			Assert.AreEqual(origCurrentBlockText, m_model.CurrentBlock.GetText(true));
			Assert.IsTrue(origTextOfFirstBlockInVerse.StartsWith(m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup).GetText(true)));
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_Splits_CurrentBlockDoesNotMatchFilter_ChangesAppliedAndSubsequentIndicesIncremented()
		{
			m_model.AttemptRefBlockMatchup = false;
			m_model.Mode = BlocksToDisplay.NeedAssignments;
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			var blockIndex = m_model.CurrentBlockIndexInBook;
			m_model.CurrentBlock.SetCharacterAndCharacterIdInScript("Jesus", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			m_model.CurrentBlock.SetCharacterAndCharacterIdInScript("father of demon-possessed boy", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			var origNextRelevantBlock = m_model.CurrentBlock;
			m_model.Mode = BlocksToDisplay.NeedAssignments | BlocksToDisplay.ExcludeUserConfirmed;
			m_model.CurrentBlockIndexInBook = blockIndex;
			m_model.AttemptRefBlockMatchup = true;

			Assert.IsFalse(m_model.IsCurrentBlockRelevant);

			var matchupForMark921 = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchupForMark921);
			Assert.AreEqual(5, matchupForMark921.CorrelatedBlocks.Count,
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));
			matchupForMark921.SetReferenceText(2, "Blah");

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);

			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(origNextRelevantBlock, m_model.CurrentBlock);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_CurrentBlockIsPastLastBlockThatMatchesFilter_ChangesAppliedAndSubsequentIndicesIncremented()
		{
			m_model.AttemptRefBlockMatchup = false;
			m_model.Mode = BlocksToDisplay.NeedAssignments;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 27), m_testProject.Versification)));
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(1, m_model.CurrentBlockDisplayIndex, "Trying to go to the \"next\" block from a position beyond the end of the list should take us back to the beginning.");
			var origNextRelevantBlock = m_model.CurrentBlock;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 27), m_testProject.Versification)));
			m_model.AttemptRefBlockMatchup = true;

			Assert.IsFalse(m_model.IsCurrentBlockRelevant);

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);

			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(origNextRelevantBlock, m_model.CurrentBlock);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsNonRelevantBlockInLastBlockMatchup_ReturnsFalse()
		{
			m_model.AttemptRefBlockMatchup = false;
			m_model.Mode = BlocksToDisplay.NeedAssignments;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 27), m_testProject.Versification)));
			m_model.LoadPreviousRelevantBlock();
			m_model.CurrentBlockIndexInBook--;
			m_model.AttemptRefBlockMatchup = true;
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
		}
	}

	/// <summary>
	/// 1 Corinthians has some especially useful data for testing multi-block matchups that have ambiguities.
	/// </summary>
	[TestFixture]
	class BlockNavigatorViewModelTestsFor1Corinthians
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.ICO);
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NeedAssignments);
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		// PG-823: Prevent out of range index
		[Test]
		public void ApplyCurrentReferenceTextMatchup_CurrentBlockIsLastRelevantBlockInLastMatchup_DoesNotCrashAndStaysOnCurrentMatchup()
		{
			m_model.Mode = BlocksToDisplay.NeedAssignments;
			m_model.AttemptRefBlockMatchup = true;

			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();

			m_model.SetBlockMatchupForCurrentVerse();
			var matchup = m_model.CurrentReferenceTextMatchup;

			Assert.IsTrue(m_model.CurrentBlockDisplayIndex >= m_model.RelevantBlockCount);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(matchup, m_model.CurrentReferenceTextMatchup);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}
	}

	[TestFixture]
	class BlockNavigatorViewModelTestsWhereFirstRelevantBlockIsAmbiguous
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.Ambiguous);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_OnBlockWhoseRainbowGroupCoversFirstBlock_ReturnsFalse()
		{
			m_model.AttemptRefBlockMatchup = false;
			while (m_model.CanNavigateToPreviousRelevantBlock)
				m_model.LoadPreviousRelevantBlock();
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.AreEqual(2, m_model.CurrentBlockDisplayIndex);

			m_model.AttemptRefBlockMatchup = true;

			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock, m_model.CurrentBlock.ToString());
			Assert.AreEqual(2, m_model.CurrentBlockDisplayIndex);
		}
	}
}
