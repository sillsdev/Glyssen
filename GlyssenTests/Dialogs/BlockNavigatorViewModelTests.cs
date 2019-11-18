using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Shared;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Reflection;
using SIL.Scripture;

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
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
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
			Assert.IsTrue(m_testProject.IncludedBooks.SelectMany(b => b.Blocks).Any(b => b.CharacterIsUnclear && b.MultiBlockQuote == MultiBlockQuote.Continuation),
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
			m_model.SetMode(m_model.Mode, true);
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
			m_model.SetMode(m_model.Mode, true);
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
			} while (m_model.CanNavigateToPreviousRelevantBlock);
		}

		[Test]
		public void SetMode_CurrentBlockIsRelevantInNewMode_KeepCurrentBlock()
		{
			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			m_model.LoadNextRelevantBlock();
			var block = m_model.CurrentBlock;

			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically);
			Assert.AreEqual(block, m_model.CurrentBlock);

			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.AreEqual(block, m_model.CurrentBlock);
		}

		[Test]
		public void SetMode_MissingExpectedQuote_LoadsBlocksWithMissingExpectedQuotes()
		{
			m_model.SetMode(BlocksToDisplay.MissingExpectedQuote);

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
			m_model.SetMode(BlocksToDisplay.MoreQuotesThanExpectedSpeakers);

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

			m_model.SetMode(m_model.Mode | BlocksToDisplay.ExcludeUserConfirmed);

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
			m_model.SetMode(BlocksToDisplay.AllQuotes);
			FindRefInMark(6, 2);
			Assert.AreEqual("MRK 6:2-3", m_model.GetBlockReferenceString());
			m_model.CurrentBlockIndexInBook = 0;

			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			FindRefInMark(5, 41);
			// The above line gets us to the first block for Mark 5:41. This verse has four blocks in it, so we advance 3 times to get to the
			// end of the verse.
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK 5:41", m_model.GetBlockReferenceString());
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK 5:41", m_model.GetBlockReferenceString());
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
			m_model.SetMode(BlocksToDisplay.AllQuotes);
			FindRefInMark(1, 17);
			var block1 = m_model.CurrentBlock;
			Assert.AreEqual("Jesus", block1.CharacterId);
			m_model.LoadNextRelevantBlock();

			Assert.AreEqual("MRK 1:24", m_model.GetBlockReferenceString());
			m_model.LoadPreviousRelevantBlock();
			Assert.AreEqual(block1, m_model.CurrentBlock);
		}

		/// <summary>
		/// PG-909
		/// </summary>
		[Test]
		public void SetMode_SwitchFromNotAssignedAutomaticallyToNotAlignedWithReferenceText_CurrentMatchupIsRelevant_RelevantBlockIndicesSelected()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			FindRefInMark(8, 5);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			var origMatchup = m_model.CurrentReferenceTextMatchup;
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			Assert.AreEqual(origMatchup.IndexOfStartBlockInBook, m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook);
			Assert.AreEqual(origMatchup.OriginalBlockCount, m_model.CurrentReferenceTextMatchup.OriginalBlockCount);
		}

		[Test]
		public void SetMode_SwitchFromNotAlignedWithReferenceTextToNotAssignedAutomatically_MatchupIsTheSame()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			FindRefInMark(8, 20);
			var origBlock = m_model.CurrentBlock;
			var origMatchup = m_model.CurrentReferenceTextMatchup;
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			var newMatchup = m_model.CurrentReferenceTextMatchup;
			Assert.AreEqual(origBlock.GetText(true), m_model.CurrentBlock.GetText(true));
			Assert.AreEqual(origMatchup.IndexOfStartBlockInBook, newMatchup.IndexOfStartBlockInBook);
			Assert.AreEqual(origMatchup.OriginalBlockCount, newMatchup.OriginalBlockCount);
			Assert.AreEqual(origMatchup.CorrelatedAnchorBlock.GetText(true), newMatchup.CorrelatedAnchorBlock.GetText(true));
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
			m_model.SetMode(m_model.Mode, false);
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(m_model.RelevantBlockCount - 1, m_model.CurrentDisplayIndex);

			m_model.SetMode(m_model.Mode, true);

			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(m_model.RelevantBlockCount, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void FindStartOfQuote_BlockIsQuoteContinuation_ReturnsQuoteStartBlock()
		{
			var blocks = m_testProject.IncludedBooks[0].GetScriptBlocks();
			int quoteStart = -1;
			int i = 1;
			for (; i < blocks.Count; i++)
			{
				if (blocks[i].MultiBlockQuote == MultiBlockQuote.Continuation)
				{
					quoteStart = i - 1;
					while (i + 1 < blocks.Count && blocks[i + 1].MultiBlockQuote == MultiBlockQuote.Continuation)
						i++;
					break;
				}
			}
			Assert.True(quoteStart >= 0, "Couldn't find data suitable to test");

			var startBlock = m_model.FindStartOfQuote(ref i);
			Assert.AreEqual(MultiBlockQuote.Start, startBlock.MultiBlockQuote);
			Assert.AreEqual(blocks[quoteStart], startBlock);
			Assert.AreEqual(quoteStart, i);
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
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsNotRelevantSingleBlockQuote_StateReflectsRelevantBlock()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.AreEqual(MultiBlockQuote.None, m_model.CurrentBlock.MultiBlockQuote, "If this fails, choose a different block.");
			Assert.IsFalse(m_model.IsCurrentBlockRelevant);
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(0, m_model.CurrentDisplayIndex);
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
					if (blocks[quoteStart].CharacterIsUnclear)
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
			Assert.IsTrue(m_model.CurrentDisplayIndex > 0);
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
					if (!blocks[quoteStart].CharacterIsUnclear)
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
			Assert.AreEqual(0, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockPrecedesCurrentMatchup_StateReflectsCorrectBlock()
		{
			m_model.SetMode(m_model.Mode, true);
			FindRefInMark(9, 21);

			var origIndexAfterFindingMark9_21 = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook;
			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook - 1;
			var expectedBlock = m_testProject.IncludedBooks.Single(b => b.BookId == "MRK").GetScriptBlocks()[indexOfBlockToSelect];

			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.IsTrue(m_model.CurrentBlockIndexInBook >= m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook);
			Assert.IsTrue(m_model.CurrentBlockIndexInBook < origIndexAfterFindingMark9_21);
			Assert.AreEqual(expectedBlock.GetText(true), m_model.CurrentBlock.GetText(true));
		}


		/// <summary>
		/// PG-924
		/// </summary>
		[Test]
		public void SetCurrentBlockIndexInBook_CurrentBlockIsNotTheFirstBlockOfRelevantMatchup_CurrentBlockRelevantAndDisplayIndexIsCorrect()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			var followingMatchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(followingMatchup);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			var origDisplayIndex = m_model.CurrentDisplayIndex;
			m_model.CurrentBlockIndexInBook = followingMatchup.IndexOfStartBlockInBook + followingMatchup.OriginalBlockCount + followingMatchup.CountOfBlocksAddedBySplitting;
			Assert.AreNotEqual(followingMatchup, m_model.CurrentReferenceTextMatchup);

			var precedingMatchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(precedingMatchup);

			m_model.CurrentBlockIndexInBook = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook - 1;

			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			Assert.AreEqual(origDisplayIndex, m_model.CurrentDisplayIndex);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(origDisplayIndex + 1, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockFollowsCurrentMatchup_StateReflectsCorrectBlock()
		{
			m_model.SetMode(m_model.Mode, true);
			FindRefInMark(9, 21);

			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook + m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count + 1;
			var expectedBlock = m_testProject.IncludedBooks.Single(b => b.BookId == "MRK")
				.GetScriptBlocks()[indexOfBlockToSelect - m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting];

			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.AreEqual(expectedBlock.GetText(true), m_model.CurrentBlock.GetText(true));
		}

		/// <summary>
		/// PG-956
		/// </summary>
		[Test]
		public void SetCurrentBlockIndexInBook_SectionHeadBlockInCurrentMatchup_FollowingScriptureBlockSelected()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture, true);
			FindRefInMark(3, 19);

			var iSectionHeadBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.IndexOf(b => b.CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook + iSectionHeadBlock;

			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.AreNotEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical),
				m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId);
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iSectionHeadBlock + 1],
				m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock);
		}

		[Test]
		public void GetIsBlockScripture_ScriptureBlock_ReturnsTrue()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
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
			m_model.SetMode(BlocksToDisplay.AllScripture);
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
			m_model.SetMode(BlocksToDisplay.AllScripture);
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
			m_model.SetMode(BlocksToDisplay.AllScripture);
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);

			//Create bad data
			BlockNavigator navigator = (BlockNavigator)ReflectionHelper.GetField(m_model, "m_navigator");
			Block nextBlock = navigator.GetNextBlock();
			var originalStatus = nextBlock.MultiBlockQuote;
			nextBlock.MultiBlockQuote = followingStatus;

			Assert.AreEqual(MultiBlockQuote.Start, m_model.CurrentBlock.MultiBlockQuote);

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.AreEqual(m_model.CurrentBlock, lastBlock);

			//Reset data for other tests
			nextBlock = navigator.GoToNextBlock();
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
			var bookScriptJud = new BookScript("JUD", new List<Block> { blockA, blockB, blockC, blockD }, ScrVers.English);
			var bookList = new List<BookScript> { bookScriptJud };

			var versesWithPotentialMissingQuote = new List<BCVRef> {new BCVRef(65, 1, 3), new BCVRef(65, 1, 4)};

			var model = new BlockNavigatorViewModel(bookList, ScrVers.English);
			var navigator = (BlockNavigator)ReflectionHelper.GetField(model, "m_navigator");
			navigator.GoToFirstBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();

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
			var bookScriptJud = new BookScript("JUD", new List<Block> {blockA, blockB, blockC}, ScrVers.English);
			var bookList = new List<BookScript> {bookScriptJud};

			var versesWithPotentialMissingQuote = new List<BCVRef> {new BCVRef(65, 1, 1), new BCVRef(65, 1, 2)};

			var model = new BlockNavigatorViewModel(bookList, ScrVers.English);
			var navigator = (BlockNavigator) ReflectionHelper.GetField(model, "m_navigator");
			navigator.GoToFirstBlock();
			navigator.GoToNextBlock();

			var found = model.CurrentBlockHasMissingExpectedQuote(versesWithPotentialMissingQuote);
			Assert.False(found);

			navigator.GoToNextBlock();
			found = model.CurrentBlockHasMissingExpectedQuote(versesWithPotentialMissingQuote);
			Assert.False(found);
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToTrue_SetsCurrentReferenceTextMatchup()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(8, 5);
			m_model.SetMode(m_model.Mode, true);
			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToFalse_ClearsCurrentReferenceTextMatchup()
		{
			m_model.SetMode(m_model.Mode, true);
			FindRefInMark(8, 5);
			m_model.SetMode(m_model.Mode, false);
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_AttemptRefBlockMatchupIsFalse_DoesNothing()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(8, 5);
			m_model.SetBlockMatchupForCurrentVerse();
			Assert.IsNull(m_model.CurrentReferenceTextMatchup);
		}

		[Test]
		public void LoadNextRelevantBlock_NoSplits_NoChangeToNumberOfBlocksCurrentBlockOrRelevantBlocks()
		{
			// Setup
			m_model.SetMode(m_model.Mode, true);
			while (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0)
			{
				m_model.LoadNextRelevantBlock();
			}
			Assert.True(m_model.CanNavigateToNextRelevantBlock);

			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origRelevantBlock = m_model.RelevantBlockCount;

			do
			{
				m_model.LoadNextRelevantBlock();
				if (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0)
					continue;

				// Verify
				Assert.AreEqual(origBlockCount, m_model.BlockCountForCurrentBook);
				Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount);
			} while (m_model.CanNavigateToNextRelevantBlock);
		}

		#region These tests test the process of creating a block matchup (as performed by SetBlockMatchupForCurrentVerse), but that method is called indirectly.
		[Test]
		public void SetBlockMatchupForCurrentVerse_Indirect_BlockWithTwoVersesSplitByReferenceText_CurrentReferenceTextMatchupHasOneOriginalBlockAndTwoCorrelatedBlocks()
		{
			// Setup
			m_model.SetMode(BlocksToDisplay.AllScripture, true);
			while (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0)
			{
				m_model.LoadNextRelevantBlock();
			}
			var origBlockCount = m_model.BlockCountForCurrentBook;
			FindRefInMark(m_model, 1, 12);

			Assert.IsNotNull(m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(2, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count);
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Single().GetText(true).StartsWith(m_model.CurrentBlock.GetText(true)));
			Assert.AreEqual(origBlockCount + 1, m_model.BlockCountForCurrentBook);
		}

		[Test]
		public void SetBlockMatchupForCurrentVerse_Indirect_BlockIsExactlyOneWholeVerse_CurrentReferenceTextMatchupHasOneBlockWithNoSplits()
		{
			// Setup
			m_model.SetMode(BlocksToDisplay.AllScripture, true);
			while (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0)
			{
				m_model.LoadNextRelevantBlock();
			}
			var origBlockCount = m_model.BlockCountForCurrentBook;

			while (m_model.CanNavigateToNextRelevantBlock)
			{
				m_model.LoadNextRelevantBlock();
				if (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting == 0 && m_model.CurrentReferenceTextMatchup.OriginalBlockCount == 1)
					break;
			}

			// If either of these calls to Single() fails, then we didn't find a suitable block to test (in the second while loop above).
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Single().GetText(true), m_model.CurrentBlock.GetText(true));
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Single().GetText(true), m_model.CurrentBlock.GetText(true));
			Assert.AreEqual(origBlockCount, m_model.BlockCountForCurrentBook);
		}
		#endregion

		[Test]
		public void TryLoadBlock_ReferenceTextCausesSplitInVernacular_SplitBlocksAddedToNumberOfBlocksAndRelevantBlocks()
		{
			var origBlockCount = m_model.BlockCountForCurrentBook;
			m_model.SetMode(m_model.Mode, true); // This causes filter to be reset and re-computes the "relevant block count" based on matchups, not individual blocks
			var origRelevantBlock = m_model.RelevantBlockCount;

			m_model.TryLoadBlock(new VerseRef(041009021, m_testProject.Versification));

			// Verify the matchup caused the block to be split as expected
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

			// Verify other state information
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock, m_model.CurrentBlock);
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));

			// Verify block count and relevant block count
			Assert.AreEqual(origBlockCount + 1, m_model.BlockCountForCurrentBook);
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount,
				"The relevant block count should not go up unless/until the existing blocks are replaced by the Correlated Blocks.");
		}

		[TestCase(20)]
		[TestCase(21)]
		[TestCase(22)]
		public void TryLoadBlock_ReferenceTextMatchupCoversMultipleVerses_AnchorBlockSetToFirstBlockForVerse(int verseNum)
		{
			m_model.SetMode(m_model.Mode, true);
			m_model.TryLoadBlock(new VerseRef(41, 9, verseNum, m_testProject.Versification));

			// Verify the matchup caused the block to be split as expected
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

			// Verify Anchor block and current block
			Assert.AreEqual(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock, m_model.CurrentBlock);
			Assert.IsTrue(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= verseNum && v.EndVerse >= verseNum));
		}

		[Test]
		public void TryLoadBlock_LoadDifferentVersesInExistingMatchup_AnchorBlockSetToFirstBlockForVerse()
		{
			m_model.SetMode(m_model.Mode, true);
			m_model.TryLoadBlock(new VerseRef(41, 9, 20, m_testProject.Versification));

			// Verify the matchup caused the block to be split as expected
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

			Assert.IsTrue(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 20 && v.EndVerse >= 20),
				"Current (anchor) block should contain start of verse we tried to load.");

			m_model.TryLoadBlock(new VerseRef(41, 9, 22, m_testProject.Versification));

			Assert.IsTrue(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 22 && v.EndVerse >= 22),
				"Current (anchor) block should contain start of verse we tried to load.");

			m_model.TryLoadBlock(new VerseRef(41, 9, 21, m_testProject.Versification));

			Assert.IsTrue(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 21 && v.EndVerse >= 21),
				"Current (anchor) block should contain start of verse we tried to load.");
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NoBlockMatchup_ThrowsInvalidOperationException()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(9, 21);
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.AreEqual("No current reference text block matchup!", e.Message);
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

			m_model.SetMode(m_model.Mode, true);
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
			//var restore = model.AttemptRefBlockMatchup;
			//model.AttemptRefBlockMatchup = false;
			while (model.CurrentBlock.ChapterNumber < chapter || model.CurrentEndBlock.LastVerseNum < verse)
			{
				if (model.IsCurrentBlockRelevant && model.CurrentDisplayIndex == model.RelevantBlockCount)
					throw new Exception("Could not find Mark " + chapter + ":" + verse);
				model.LoadNextRelevantBlock();
			}
			Assert.AreEqual("MRK", model.CurrentBookId);
			Assert.AreEqual(chapter, model.CurrentBlock.ChapterNumber);
			Assert.IsTrue(verse >= model.CurrentBlock.InitialStartVerseNumber && verse <= model.CurrentEndBlock.LastVerseNum);
			//model.AttemptRefBlockMatchup = restore;
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
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		[Test]
		public void TryLoadBlock_FromRelevantBlockToRelevantBlockInMatchup_RelevantBlockLoaded()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);

			m_model.TryLoadBlock(m_model.GetBlockVerseRef());
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void TryLoadBlock_FromIrrelevantBlockToRelevantBlockInMatchup_RelevantBlockLoaded()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			var targetRef = m_model.GetBlockVerseRef();

			int v = 1;
			while (m_model.IsCurrentBlockRelevant)
			{
				m_model.TryLoadBlock(new VerseRef(41, 1, v++));
			}

			m_model.TryLoadBlock(targetRef);
			Assert.IsTrue(m_model.CurrentDisplayIndex > 0);
		}

		[TestCase(41001000)] // MRK 1:0
		[TestCase(41000001)] // MRK 0:1
		public void TryLoadBlock_FromRelevantBlockToNonScriptureBlock_NotLoaded(int bbbcccvvv)
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);

			Assert.IsFalse(m_model.TryLoadBlock(new VerseRef(bbbcccvvv, ScrVers.English)));
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void LoadNextRelevantBlock_FollowingInsertionOfBlockByApplyingMatchupWithSplits_LoadsARelevantBlock()
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			//int displayIndex = m_model.CurrentBlockDisplayIndex;
			m_model.SetMode(m_model.Mode, true);
			//Assert.AreEqual(displayIndex, m_model.CurrentBlockDisplayIndex);
			int displayIndex = m_model.CurrentDisplayIndex;
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			m_model.ApplyCurrentReferenceTextMatchup();
			Assert.AreEqual(displayIndex, m_model.CurrentDisplayIndex);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			// Need to switch to "Match Character" mode because in rainbow mode, we occasionally get a block matchup that covers
			// more than one relevant block, so advancing to the next relevant place can increment the display index by more than one.
			//		m_model.SetMode(m_model.Mode, false);
			do
			{
				m_model.LoadNextRelevantBlock();
				Assert.AreEqual(++displayIndex, m_model.CurrentDisplayIndex);
				Assert.IsTrue(m_model.IsCurrentBlockRelevant);
			} while (m_model.CurrentBookId == "MRK");
			Assert.AreEqual("ACT", m_model.CurrentBookId);
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
		}

		[TestCase(BlocksToDisplay.NotAssignedAutomatically)]
		[TestCase(BlocksToDisplay.NotYetAssigned)]
		public void ApplyCurrentReferenceTextMatchup_Simple_SetsReferenceTextsForBlocksInGroupAndUpdatesState(BlocksToDisplay filterMode)
		{
			m_model.SetMode(filterMode, true);

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 8, 5);
			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origCurrentBlock = m_model.CurrentBlock;
			var origRelevantBlock = m_model.RelevantBlockCount;
			var origBlockDisplayIndex = m_model.CurrentDisplayIndex;
			var matchupForMark85 = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchupForMark85);
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			matchupForMark85.SetReferenceText(0, "Some random text: " + Guid.NewGuid());

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(matchupForMark85, m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(origCurrentBlock, m_model.CurrentBlock);
			Assert.AreEqual(0, matchupForMark85.CountOfBlocksAddedBySplitting);
			Assert.IsFalse(matchupForMark85.HasOutstandingChangesToApply);
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentDisplayIndex);
			Assert.AreEqual(origRelevantBlock, m_model.RelevantBlockCount);
			Assert.AreEqual(origBlockCount, m_model.BlockCountForCurrentBook);
			Assert.IsTrue(m_model.CanNavigateToNextRelevantBlock);
		}

		[TestCase(BlocksToDisplay.NotAssignedAutomatically)]
		[TestCase(BlocksToDisplay.NotYetAssigned)]
		public void ApplyCurrentReferenceTextMatchup_Splits_BlocksInBookReplacedAndCurrentBlockNoLongerComesFromMatchupAndSubsequentRelevantBlockIndicesIncremented(BlocksToDisplay filterMode)
		{
			var origBlockCount = m_model.BlockCountForCurrentBook;
			m_model.SetMode(filterMode, true);
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			var origRelevantBlock = m_model.RelevantBlockCount;
			var origBlockDisplayIndex = m_model.CurrentDisplayIndex;
			var matchupForMark921 = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchupForMark921);
			Assert.AreEqual(5, matchupForMark921.CorrelatedBlocks.Count,
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentDisplayIndex);
			var origTextOfFirstBlockInVerse = m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup).GetText(true);
			var origCurrentBlockText = m_model.CurrentBlock.GetText(true);
			Assert.IsFalse(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock));

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(matchupForMark921, m_model.CurrentReferenceTextMatchup);
			Assert.AreEqual(0, matchupForMark921.CountOfBlocksAddedBySplitting);
			Assert.IsFalse(matchupForMark921.HasOutstandingChangesToApply);
			Assert.AreEqual(origBlockDisplayIndex, m_model.CurrentDisplayIndex);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, false);
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			var blockIndex = m_model.CurrentBlockIndexInBook;
			m_model.CurrentBlock.SetCharacterIdAndCharacterIdInScript("Jesus", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			m_model.CurrentBlock.SetCharacterIdAndCharacterIdInScript("father of demon-possessed boy", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			var origNextRelevantBlock = m_model.CurrentBlock;
			m_model.SetMode(BlocksToDisplay.NotYetAssigned, true);
			
			m_model.TryLoadBlock(new VerseRef(41, 9, 21, m_testProject.Versification));
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
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Contains(origNextRelevantBlock));
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_CurrentBlockIsPastLastBlockThatMatchesFilter_ChangesAppliedAndSubsequentIndicesIncremented()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(BCVRef.BookToNumber("ACT"), 28, 23, m_testProject.Versification)));
			Assert.AreEqual(23, m_model.CurrentBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, m_model.CurrentDisplayIndex);
			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(1, m_model.CurrentDisplayIndex, "Trying to go to the \"next\" block from a position beyond the end of the list should take us back to the beginning.");
			var origNextRelevantBlockText = m_model.CurrentBlock.GetText(true);
			var origNextRelevantBlockIndexInBook = m_model.CurrentBlockIndexInBook;
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 23), m_testProject.Versification)));
			var origRelevantBlockCount = m_model.RelevantBlockCount;

			Assert.IsFalse(m_model.IsCurrentBlockRelevant);

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);
			matchup.MatchAllBlocks(m_testProject.Versification);

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(origRelevantBlockCount, m_model.RelevantBlockCount);
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);

			m_model.LoadNextRelevantBlock();
			Assert.AreEqual(origNextRelevantBlockText, m_model.CurrentBlock.GetText(true));
			Assert.AreEqual(origNextRelevantBlockIndexInBook, m_model.CurrentBlockIndexInBook);
			Assert.AreEqual(1, m_model.CurrentDisplayIndex);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_BlockMatchupAlreadyApplied_ThrowsInvalidOperationException()
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 8, 5);
			m_model.SetMode(m_model.Mode, true);
			m_model.CurrentReferenceTextMatchup.SetReferenceText(0, "Some random text: " + Guid.NewGuid());
			m_model.ApplyCurrentReferenceTextMatchup();
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.AreEqual("Current reference text block matchup has no outstanding changes!", e.Message);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsInLastBlockMatchup_ReturnsFalse()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			Assert.IsTrue(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 27), m_testProject.Versification)));
			m_model.LoadPreviousRelevantBlock();
			var startIndex = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook;
			do
			{
				Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
				m_model.CurrentBlockIndexInBook++;
			} while (startIndex == m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook);
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
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
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
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);

			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();

			var matchup = m_model.CurrentReferenceTextMatchup;

			Assert.IsTrue(m_model.CurrentDisplayIndex >= m_model.RelevantBlockCount);
			matchup.MatchAllBlocks(m_testProject.Versification);
			foreach (var block in matchup.CorrelatedBlocks.Where(b => b.CharacterIsUnclear))
				block.SetCharacterIdAndCharacterIdInScript("Paul", m_model.CurrentBookNumber, m_testProject.Versification);
			Assert.IsTrue(matchup.HasOutstandingChangesToApply);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.IsFalse(m_model.CanNavigateToNextRelevantBlock);
			Assert.AreEqual(matchup, m_model.CurrentReferenceTextMatchup);
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}
	}

	/// <summary>
	/// JHN has data that allows us to test the special case of a quote being split by the reference text in a verse (12:27) that has
	/// ambiguity (There are two speakers in John 12:28). Also, tests in this fixture can modify the test project data, so the folder
	/// is cleaned up each time.
	/// </summary>
	[TestFixture]
	class BlockNavigatorViewModelTestsForLukeAndJohn
	{
		private Project m_testProject;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK, TestProject.TestBook.JHN);
		}

		[TearDown]
		public void TearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		/// <summary>
		/// PG-879:
		/// </summary>
		[Test]
		public void ApplyCurrentReferenceTextMatchup_ApplyCausesSplittingOfUserConfirmedBlockThatDoesNotMatchFilter_NewBlocksAreNotAddedToRelevantBlocks()
		{
			// Set up initial data state
			TestProject.SimulateDisambiguationForAllBooks(m_testProject);
			var blockToMatchFilter = m_testProject.IncludedBooks.First().GetScriptBlocks().First(b => b.UserConfirmed);
			blockToMatchFilter.UserConfirmed = false;
			blockToMatchFilter.CharacterId = CharacterVerseData.kUnexpectedCharacter;

			// Create model and initialize state
			var model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotYetAssigned);

			// Get into correct state for this test and ensure initial conditions
			Assert.AreEqual(1, model.RelevantBlockCount);
			model.SetMode(model.Mode, true);
			Assert.IsTrue(model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("JHN"), 12, 27), m_testProject.Versification)));
			Assert.AreEqual(27, model.CurrentBlock.InitialStartVerseNumber);
			Assert.IsFalse(model.IsCurrentBlockRelevant);
			var matchup = model.CurrentReferenceTextMatchup;
			Assert.IsNotNull(matchup);
			matchup.MatchAllBlocks(m_testProject.Versification);

			model.ApplyCurrentReferenceTextMatchup();

			Assert.AreEqual(1, model.RelevantBlockCount);
			Assert.IsTrue(model.CanNavigateToPreviousRelevantBlock);
			Assert.IsFalse(model.CanNavigateToNextRelevantBlock);
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
			m_model.SetMode(m_model.Mode, false);
			while (m_model.CanNavigateToPreviousRelevantBlock)
				m_model.LoadPreviousRelevantBlock();
			m_model.LoadNextRelevantBlock();
			Assert.IsTrue(m_model.CanNavigateToPreviousRelevantBlock);
			Assert.AreEqual(2, m_model.CurrentDisplayIndex);
			var origCurrentBlock = m_model.CurrentBlock;

			m_model.SetMode(m_model.Mode, true);

			Assert.IsFalse(m_model.CanNavigateToPreviousRelevantBlock, m_model.CurrentBlock.ToString());
			Assert.IsTrue(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Contains(origCurrentBlock));
			Assert.AreEqual(1, m_model.CurrentDisplayIndex, "Note: Setting AttemptRefBlockMatchup = true should cause the filter to be" +
				"reset and the \"block\" display index");
		}
	}

	/// <summary>
	/// PG-1261: This test fixture doctors up the data to test for a very specific case.
	/// The "real" place where the bug occurred was in Rev 12:18 (due to a textual variant), but since
	/// we can now fix the reference text to deal with cases where the vernacular follows that variant,
	/// this fixture sets up a "dummy" scenario at the end of MRK 15.
	/// </summary>
	[TestFixture]
	class BlockNavigatorViewModelTestsWithExtraVerseInVernacular
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;
		private VerseRef m_targetReference;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			var mark = m_testProject.IncludedBooks.Single();
			var bookNum = mark.BookNumber;
			var lastBlockInChapter = m_testProject.IncludedBooks.Single().Blocks.First(b =>
				b.ChapterNumber == 15 &&
				b.LastVerseNum == ScrVers.English.GetLastVerse(bookNum, b.ChapterNumber) &&
				b.StartsAtVerseStart &&
				b.AllVerses.Skip(1).Any() && 
				b.CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			// We have to manually split the block to separate the initial verses which already don't match
			// the reference text from the last two verse, which are just plain narrator verses in both the
			// Vernacular and the English reference text.
			lastBlockInChapter = mark.SplitBlock(lastBlockInChapter,
				((Verse)(lastBlockInChapter.AllVerses.Reverse().ElementAt(2))).Number,
				PortionScript.kSplitAtEndOfVerse, true,
				lastBlockInChapter.CharacterId);
			m_targetReference = new VerseRef(bookNum, lastBlockInChapter.ChapterNumber, lastBlockInChapter.InitialStartVerseNumber);
			var tempModel = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAlignedToReferenceText);
			Assert.IsTrue(tempModel.TryLoadBlock(m_targetReference));
			Assert.IsFalse(tempModel.IsCurrentBlockRelevant, "Before adding the extra verse, this block should not be relevant.");

			// Finally, we add the extra verse (not in the reference text)
			var extraVerseNum = lastBlockInChapter.LastVerseNum + 1;
			lastBlockInChapter.AddVerse(extraVerseNum, "This verse will not have a corresponding verse in the reference text.");
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAlignedToReferenceText);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TestProject.DeleteTestProjectFolder();
		}

		/// <summary>
		/// PG-1261: This test indirectly tests the IsRelevant method, which should determine that the vernacular
		/// block at the end of Mark 15 IS relevant because it has an additional verse not in the reference text.
		/// </summary>
		[Test]
		public void IsRelevant_VernBlockHasUnexpectedVerseNotInReferenceText_ReturnsFalse()
		{
			m_model.SetMode(m_model.Mode, true);
			Assert.IsTrue(m_model.TryLoadBlock(m_targetReference));
			Assert.IsTrue(m_model.IsCurrentBlockRelevant);
		}
	}
}
