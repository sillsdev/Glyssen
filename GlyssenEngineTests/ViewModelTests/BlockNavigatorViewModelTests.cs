using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using SIL.Extensions;
using SIL.Reflection;
using SIL.Scripture;
using static System.String;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenEngineTests.TestProject;
using BlockNavigatorViewModel = GlyssenEngine.ViewModels.BlockNavigatorViewModel<Rhino.Mocks.Interfaces.IMockedObject>;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class BlockNavigatorViewModelTests
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
			m_testProject = CreateTestProject(TestBook.MRK, TestBook.JUD);
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
			m_model.BackwardContextBlockCount = 10;
			m_model.ForwardContextBlockCount = 10;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			DeleteTestProjects();
		}

		[Test]
		public void Constructor_FirstQuoteIsUnexpected_FirstUnexpectedBlockLoaded()
		{
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(1));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(2));
		}

		[Test]
		public void LoadNextRelevantBlock_DataHasSomeContinuationBlocksNeedingAssignment_ContinuationBlocksNeverGetLoaded()
		{
			Assert.That(m_testProject.IncludedBooks.SelectMany(b => b.Blocks).Any(b => b.CharacterIsUnclear && b.MultiBlockQuote == MultiBlockQuote.Continuation), Is.True,
				"Test data does not have the required characteristics. Need at least one unassigned block that is a continuation of a quote from earlier paragraph.");

			do
			{
				Assert.That(m_model.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation, Is.False);
				m_model.LoadNextRelevantBlock();
			} while (!m_model.CanNavigateToNextRelevantBlock);

			Assert.That(m_model.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation, Is.False);
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
						Assert.That(blocksInPreviousRainbowGroup.Intersect(m_model.CurrentReferenceTextMatchup.OriginalBlocks).Any(), Is.False);
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
			Assert.That(m_testProject.IncludedBooks.Count, Is.EqualTo(2));
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_testProject.IncludedBooks[0].BookId, Is.EqualTo(m_model.CurrentBookId));

			// Run test
			var currentBlockBeforeCall = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlockInSubsequentBook();
			Assert.That(currentBlockBeforeCall, Is.EqualTo(m_model.CurrentBlock));
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
						Assert.That(blocksInFollowingRainbowGroup.Intersect(m_model.CurrentReferenceTextMatchup.OriginalBlocks).Any(), Is.False);
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
			Assert.That(block, Is.EqualTo(m_model.CurrentBlock));

			m_model.SetMode(BlocksToDisplay.NotYetAssigned);
			Assert.That(block, Is.EqualTo(m_model.CurrentBlock));
		}

		[Test]
		public void SetMode_MissingExpectedQuote_LoadsBlocksWithMissingExpectedQuotes()
		{
			m_model.SetMode(BlocksToDisplay.MissingExpectedQuote);

			// Missing quote is for verse 20
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(1));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(m_model.CurrentBlock.LastVerseNum, Is.EqualTo(20));

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 30
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(1));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(29));
			Assert.That(m_model.CurrentBlock.LastVerseNum, Is.EqualTo(31));

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 15
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(2));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(m_model.CurrentBlock.LastVerseNum, Is.EqualTo(16));

			m_model.LoadNextRelevantBlock();

			// Missing quote is for verse 9
			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(3));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(m_model.CurrentBlock.LastVerseNum, Is.EqualTo(11));
		}

		[Test]
		public void SetMode_MoreQuotesThanExpectedSpeakers_LoadsBlocksWithMoreQuotesThanExpectedSpeakers()
		{
			m_model.SetMode(BlocksToDisplay.MoreQuotesThanExpectedSpeakers);

			Assert.That(m_model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(m_model.CurrentBlock.ChapterNumber, Is.EqualTo(1));
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(2));
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

			Assert.That(block2, Is.EqualTo(m_model.CurrentBlock));
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
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 6:2-3"));
			m_model.CurrentBlockIndexInBook = 0;

			m_model.SetMode(BlocksToDisplay.AllExpectedQuotes);
			FindRefInMark(5, 41);
			// The above line gets us to the first block for Mark 5:41. This verse has four blocks in it, so we advance 3 times to get to the
			// end of the verse.
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 5:41"));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 5:41"));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 5:41"));
			//End validate setup

			m_model.LoadNextRelevantBlock();
			// The expected quote is actually in verse 4, but unfortunately, the filter is
			// actually "verses with expected quotes" so we stop on this block
			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 6:3-4"));
		}

		[Test]
		public void SetMode_AllQuotes_NonQuoteBlocksSkipped()
		{
			m_model.SetMode(BlocksToDisplay.AllQuotes);
			FindRefInMark(1, 17);
			var block1 = m_model.CurrentBlock;
			Assert.That(block1.CharacterId, Is.EqualTo("Jesus"));
			m_model.LoadNextRelevantBlock();

			Assert.That(m_model.GetBlockReferenceString(), Is.EqualTo("MRK 1:24"));
			m_model.LoadPreviousRelevantBlock();
			Assert.That(block1, Is.EqualTo(m_model.CurrentBlock));
		}

		/// <summary>
		/// PG-909
		/// </summary>
		[Test]
		public void SetMode_SwitchFromNotAssignedAutomaticallyToNotAlignedWithReferenceText_CurrentMatchupIsRelevant_RelevantBlockIndicesSelected()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			FindRefInMark(8, 5);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			var origMatchup = m_model.CurrentReferenceTextMatchup;
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			Assert.That(origMatchup.IndexOfStartBlockInBook, Is.EqualTo(m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook));
			Assert.That(origMatchup.OriginalBlockCount, Is.EqualTo(m_model.CurrentReferenceTextMatchup.OriginalBlockCount));
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
			Assert.That(origBlock.GetText(true), Is.EqualTo(m_model.CurrentBlock.GetText(true)));
			Assert.That(origMatchup.IndexOfStartBlockInBook, Is.EqualTo(newMatchup.IndexOfStartBlockInBook));
			Assert.That(origMatchup.OriginalBlockCount, Is.EqualTo(newMatchup.OriginalBlockCount));
			Assert.That(origMatchup.CorrelatedAnchorBlock.GetText(true), Is.EqualTo(newMatchup.CorrelatedAnchorBlock.GetText(true)));
		}

		[Test]
		public void BlockCountForCurrentBook_TestMrk_ReturnsTrue()
		{
			int expectedCount = m_testProject.IncludedBooks[0].Blocks.Count;
			Assert.That(expectedCount, Is.EqualTo(m_model.BlockCountForCurrentBook), "Test data may have been changed");
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_MinGreaterThanMax_ReturnsNegative1()
		{
			Assert.That(BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				new List<BookBlockIndices>(), new BookBlockIndices(1, 2), true, 1, 0),
				Is.EqualTo(-1));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_PreviousBlockIsRelevant_ReturnsClosestPreviousRelevantBlock()
		{
			var relevantBlocks = new List<BookBlockIndices>
			{
				new BookBlockIndices(1, 2),
				new BookBlockIndices(1, 20),
				new BookBlockIndices(2, 1),
				new BookBlockIndices(2, 7),
				new BookBlockIndices(2, 8),
				new BookBlockIndices(2, 14),
				new BookBlockIndices(3, 2)
			};
			Assert.That(BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(2, 10), true, 0, relevantBlocks.Count - 1),
				Is.EqualTo(4));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_NoPreviousBlockIsRelevant_ReturnsNegative1()
		{
			var relevantBlocks = new List<BookBlockIndices>
			{
				new BookBlockIndices(2, 14),
				new BookBlockIndices(3, 2)
			};
			Assert.That(BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(1, 3), true, 0, relevantBlocks.Count - 1),
				Is.EqualTo(-1));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_FollowingBlockIsRelevant_ReturnsClosestFollowingRelevantBlock()
		{
			var relevantBlocks = new List<BookBlockIndices>
			{
				new BookBlockIndices(1, 2),
				new BookBlockIndices(1, 20),
				new BookBlockIndices(2, 1),
				new BookBlockIndices(2, 7),
				new BookBlockIndices(2, 8),
				new BookBlockIndices(2, 14),
				new BookBlockIndices(3, 2)
			};
			Assert.That(BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(1, 21), false, 0, relevantBlocks.Count - 1),
				Is.EqualTo(2));
		}

		[Test]
		public void GetIndexOfClosestRelevantBlock_NoFollowingBlockIsRelevant_ReturnsNegative1()
		{
			var relevantBlocks = new List<BookBlockIndices>
			{
				new BookBlockIndices(1, 2),
				new BookBlockIndices(1, 20),
				new BookBlockIndices(2, 1),
				new BookBlockIndices(2, 7),
				new BookBlockIndices(2, 8),
				new BookBlockIndices(2, 14),
				new BookBlockIndices(3, 2)
			};
			Assert.That(BlockNavigatorViewModel.GetIndexOfClosestRelevantBlock(
				relevantBlocks, new BookBlockIndices(3, 3), false, 0, relevantBlocks.Count - 1),
				Is.EqualTo(-1));
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsFirstRelevantBlock_ReturnsFalse()
		{
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.False);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsFirstRelevantBlock_ReturnsTrue()
		{
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsSecondRelevantBlock_ReturnsTrue()
		{
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsLastRelevantBlock_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = m_testProject.IncludedBooks[0].GetScriptBlocks().Count - 1;
			m_model.LoadPreviousRelevantBlock();
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsVeryFirstBlock_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = 0;
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.False);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsVeryLastBlockInBook_ReturnsFalse()
		{
			m_model.CurrentBlockIndexInBook = m_testProject.IncludedBooks[0].GetScriptBlocks().Count - 1;
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_CurrentBlockIsAdHocLocationInMiddleOfBook_ReturnsTrue()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False, "If this fails, we chose a relevant block index by accident.");
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsAdHocLocationInMiddleOfBook_ReturnsTrue()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False, "If this fails, we chose a relevant block index by accident.");
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_OnBlockWhoseRainbowGroupCoversLastBlock_ReturnsFalse()
		{
			m_model.SetMode(m_model.Mode, false);
			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
			Assert.That(m_model.RelevantBlockCount - 1, Is.EqualTo(m_model.CurrentDisplayIndex));

			m_model.SetMode(m_model.Mode, true);

			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
			Assert.That(m_model.RelevantBlockCount, Is.EqualTo(m_model.CurrentDisplayIndex));
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

			Assert.That(quoteStart, Is.GreaterThanOrEqualTo(0), "Couldn't find data suitable to test");

			var startBlock = m_model.FindStartOfQuote(ref i);
			Assert.That(startBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(blocks[quoteStart], Is.EqualTo(startBlock));
			Assert.That(quoteStart, Is.EqualTo(i));
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsRelevantSingleBlockQuote_StateReflectsRelevantBlock()
		{
			m_model.LoadNextRelevantBlock();
			var index = m_model.CurrentBlockIndexInBook;
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None),
				"If this fails, choose a different block.");
			m_model.LoadNextRelevantBlock();
			m_model.LoadNextRelevantBlock();
			m_model.CurrentBlockIndexInBook = index;
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));
		}

		[Test]
		public void SetCurrentBlockIndexInBook_BlockIsNotRelevantSingleBlockQuote_StateReflectsRelevantBlock()
		{
			m_model.CurrentBlockIndexInBook = 400;
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None),
				"If this fails, choose a different block.");
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(0));
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
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(m_model.GetIndicesOfQuoteContinuationBlocks(m_model.CurrentBlock).Any(), Is.True);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			Assert.That(m_model.CurrentDisplayIndex > 0, Is.True);
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
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(m_model.GetIndicesOfQuoteContinuationBlocks(m_model.CurrentBlock).Any(), Is.True);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(0));
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
			Assert.That(m_model.CurrentBlockIndexInBook >= m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook, Is.True);
			Assert.That(m_model.CurrentBlockIndexInBook < origIndexAfterFindingMark9_21, Is.True);
			Assert.That(expectedBlock.GetText(true), Is.EqualTo(m_model.CurrentBlock.GetText(true)));
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
			Assert.That(followingMatchup, Is.Not.Null);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			var origDisplayIndex = m_model.CurrentDisplayIndex;
			m_model.CurrentBlockIndexInBook = followingMatchup.IndexOfStartBlockInBook + followingMatchup.OriginalBlockCount + followingMatchup.CountOfBlocksAddedBySplitting;
			Assert.That(followingMatchup, Is.Not.EqualTo(m_model.CurrentReferenceTextMatchup));

			var precedingMatchup = m_model.CurrentReferenceTextMatchup;
			Assert.That(precedingMatchup, Is.Not.Null);

			m_model.CurrentBlockIndexInBook = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook - 1;

			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			Assert.That(origDisplayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
			m_model.LoadNextRelevantBlock();
			Assert.That(origDisplayIndex + 1, Is.EqualTo(m_model.CurrentDisplayIndex));
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
			Assert.That(expectedBlock.GetText(true), Is.EqualTo(m_model.CurrentBlock.GetText(true)));
		}

		/// <summary>
		/// PG-956
		/// </summary>
		[Test]
		public void SetCurrentBlockIndexInBook_SectionHeadBlockInCurrentMatchup_FollowingScriptureBlockSelected()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture, true);
			FindRefInMark(3, 19);

			var iSectionHeadBlock = m_model.CurrentReferenceTextMatchup.CorrelatedBlocks
				.IndexOf(b => b.CharacterIs("MRK", StandardCharacter.ExtraBiblical));
			var indexOfBlockToSelect = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook + iSectionHeadBlock;

			m_model.CurrentBlockIndexInBook = indexOfBlockToSelect;
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock.CharacterId,
				Is.Not.EqualTo(GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical)));
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[iSectionHeadBlock + 1],
				Is.EqualTo(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock));
		}

		[Test]
		public void GetIsBlockScripture_ScriptureBlock_ReturnsTrue()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			FindRefInMark(1, 12);
			Assert.That(m_model.CurrentBlock.IsScripture, Is.True);
			Assert.That(m_model.GetIsBlockScripture(m_model.CurrentBlockIndexInBook), Is.True);
		}

		[Test]
		public void GetIsBlockScripture_TitleBlock_ReturnsFalse()
		{
			Assert.That(m_testProject.IncludedBooks[0].Blocks[0].IsScripture, Is.False);
			Assert.That(m_model.GetIsBlockScripture(0), Is.False);
		}

		[Test]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsStart_FindsLastContinuationBlock()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.That(lastBlock, Is.Not.EqualTo(m_model.CurrentBlock));
			Assert.That(lastBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			m_model.LoadNextRelevantBlock(); // Goes to next block not in this multi-block quote

			Assert.That(m_model.CurrentBlockIndexInBook - 1, Is.EqualTo(m_model.GetBlockIndices(lastBlock).BlockIndex));
		}

		[Test]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsNone_ReturnsCurrentBlock()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.None)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.That(m_model.CurrentBlock, Is.EqualTo(lastBlock));
			Assert.That(lastBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[TestCase(MultiBlockQuote.None)]
		[TestCase(MultiBlockQuote.Start)]
		public void GetLastBlockInCurrentQuote_CurrentBlockIsStartButNextIsNoneOrStartRepresentingBadData_ReturnsCurrentBlock(MultiBlockQuote followingStatus)
		{
			m_model.SetMode(BlocksToDisplay.AllScripture);
			while (m_model.CurrentBlock.MultiBlockQuote != MultiBlockQuote.Start)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			//Create bad data
			var navigator = (BlockNavigator)ReflectionHelper.GetField(m_model, "m_navigator");
			Block nextBlock = navigator.GetNextBlock();
			var originalStatus = nextBlock.MultiBlockQuote;
			nextBlock.MultiBlockQuote = followingStatus;

			Assert.That(m_model.CurrentBlock.MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			var lastBlock = m_model.GetLastBlockInCurrentQuote();
			Assert.That(m_model.CurrentBlock, Is.EqualTo(lastBlock));

			//Reset data for other tests
			nextBlock = navigator.GoToNextBlock();
			nextBlock.MultiBlockQuote = originalStatus;
		}

		[Test]
		public void CurrentBlockHasMissingExpectedQuote_QuoteContainsVerseBridgeNonQuoteAtEnd_NoMissingExpectedQuotes()
		{
			var blockA = new Block("ip") {MultiBlockQuote = MultiBlockQuote.None};
			var blockB = new Block("p", 1, 1) {MultiBlockQuote = MultiBlockQuote.Start};
			blockB.BlockElements.Add(new Verse("1"));
			blockB.BlockElements.Add(new ScriptText("Verse 1"));
			var blockC = new Block("p", 1, 2) {MultiBlockQuote = MultiBlockQuote.Continuation};
			blockC.BlockElements.Add(new Verse("2"));
			blockC.BlockElements.Add(new ScriptText("Verse 2"));
			blockC.BlockElements.Add(new Verse("3-4"));
			blockC.BlockElements.Add(new ScriptText("Verse 3-4"));
			var blockD = new Block("p", 1, 3, 4) {MultiBlockQuote = MultiBlockQuote.None};
			blockD.BlockElements.Add(new ScriptText("Jesus said"));
			var bookScriptJud = new BookScript("JUD", new List<Block> {blockA, blockB, blockC, blockD}, ScrVers.English);
			var bookList = new List<BookScript> {bookScriptJud};

			var versesWithPotentialMissingQuote = new List<BCVRef> {new BCVRef(65, 1, 3), new BCVRef(65, 1, 4)};

			var model = new BlockNavigatorViewModel(bookList, ScrVers.English);
			var navigator = (BlockNavigator)ReflectionHelper.GetField(model, "m_navigator");
			navigator.GoToFirstBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();
			navigator.GoToNextBlock();

			var found = model.BlockHasMissingExpectedQuote(navigator.CurrentBlock, false, versesWithPotentialMissingQuote);

			Assert.That(found, Is.False);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CurrentBlockHasMissingExpectedQuote_QuoteContainsVerseBridgeNonQuoteAtBeginning_NoMissingExpectedQuotes(bool searchForwardOnlyFromFirstVerse)
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
			var navigator = (BlockNavigator)ReflectionHelper.GetField(model, "m_navigator");
			navigator.GoToFirstBlock();
			navigator.GoToNextBlock();

			var found = model.BlockHasMissingExpectedQuote(navigator.CurrentBlock, searchForwardOnlyFromFirstVerse, versesWithPotentialMissingQuote);
			Assert.That(found, Is.False);

			navigator.GoToNextBlock();
			found = model.BlockHasMissingExpectedQuote(navigator.CurrentBlock, false, versesWithPotentialMissingQuote);
			Assert.That(found, Is.False);
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToTrue_SetsCurrentReferenceTextMatchup()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(8, 5);
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToFalseWithMatchupHavingSubsequentBlockRelevant_ClearsCurrentReferenceTextMatchupAndSetsFirstRelevantBlockInMatchupAsCurrent()
		{
			m_model.SetMode(m_model.Mode, true);
			FindRefInMark(8, 5);
			Block expectedBlock = null;
			var markBlocks = m_testProject.IncludedBooks.Single(b => b.BookId == m_model.CurrentBookId).GetScriptBlocks();
			for (int i = m_model.IndexOfFirstBlockInCurrentGroup; i < m_model.IndexOfLastBlockInCurrentGroup; i++)
			{
				if (markBlocks[i].CharacterIsUnclear)
				{
					expectedBlock = markBlocks[i];
					break;
				}
			}
			Assert.That(expectedBlock, Is.Not.Null);
			m_model.SetMode(m_model.Mode, false);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Null);
			Assert.That(expectedBlock, Is.EqualTo(m_model.CurrentBlock));
		}

		[Test]
		public void AttemptRefBlockMatchup_SetToFalseWithMatchupHavingFirstBlockRelevant_ClearsCurrentReferenceTextMatchupAndSetsFirstBlockInMatchupAsCurrent()
		{
			m_model.SetMode(BlocksToDisplay.AllScripture, true);
			while (m_model.CurrentReferenceTextMatchup.OriginalBlockCount == 1 && m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlockCount > 1,
				"SETUP error: No suitable block found.");

			var expectedBlock = m_model.IndexOfFirstBlockInCurrentGroup;
			m_model.SetMode(BlocksToDisplay.AllScripture, false);
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Null);
			Assert.That(expectedBlock, Is.EqualTo(m_model.CurrentBlockIndexInBook));
		}

		[Test]
		public void SetBlockMatchupForCurrentLocation_AttemptRefBlockMatchupIsFalse_DoesNothing()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(8, 5);
			m_model.SetBlockMatchupForCurrentLocation();
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Null);
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

			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);

			var origBlockCount = m_model.BlockCountForCurrentBook;
			var origRelevantBlock = m_model.RelevantBlockCount;

			do
			{
				m_model.LoadNextRelevantBlock();
				if (m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting > 0)
					continue;

				// Verify
				Assert.That(origBlockCount, Is.EqualTo(m_model.BlockCountForCurrentBook));
				Assert.That(origRelevantBlock, Is.EqualTo(m_model.RelevantBlockCount));
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

			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Single().GetText(true), Does.StartWith(m_model.CurrentBlock.GetText(true)));
			Assert.That(origBlockCount + 1, Is.EqualTo(m_model.BlockCountForCurrentBook));
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
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Single().GetText(true), Is.EqualTo(m_model.CurrentBlock.GetText(true)));
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks.Single().GetText(true), Is.EqualTo(m_model.CurrentBlock.GetText(true)));
			Assert.That(origBlockCount, Is.EqualTo(m_model.BlockCountForCurrentBook));
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
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[0].LastVerseNum, Is.EqualTo(20),
				"Narrator text at start of verse 21 begins in verse 20.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1].InitialStartVerseNumber, Is.EqualTo(21),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(22),
				"Final quote in vernacular spills over into verse 22");
			Assert.That(m_model.GetLastVerseInCurrentQuote(), Is.EqualTo(22));
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last(), Is.EqualTo(m_model.GetLastBlockInCurrentQuote()));

			// Verify other state information
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock, Is.EqualTo(m_model.CurrentBlock));
			Assert.That(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock), Is.False);

			// Verify block count and relevant block count
			Assert.That(origBlockCount + 1, Is.EqualTo(m_model.BlockCountForCurrentBook));
			Assert.That(origRelevantBlock, Is.EqualTo(m_model.RelevantBlockCount),
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
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[0].LastVerseNum, Is.EqualTo(20),
				"Narrator text at start of verse 21 begins in verse 20.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1].InitialStartVerseNumber, Is.EqualTo(21),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(22),
				"Final quote in vernacular spills over into verse 22");
			Assert.That(m_model.GetLastVerseInCurrentQuote(), Is.EqualTo(22));
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last(), Is.EqualTo(m_model.GetLastBlockInCurrentQuote()));

			// Verify Anchor block and current block
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedAnchorBlock, Is.EqualTo(m_model.CurrentBlock));
			Assert.That(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= verseNum && v.EndVerse >= verseNum), Is.True);
		}

		[Test]
		public void TryLoadBlock_LoadDifferentVersesInExistingMatchup_AnchorBlockSetToFirstBlockForVerse()
		{
			m_model.SetMode(m_model.Mode, true);
			m_model.TryLoadBlock(new VerseRef(41, 9, 20, m_testProject.Versification));

			// Verify the matchup caused the block to be split as expected
			Assert.That(m_model.CurrentReferenceTextMatchup, Is.Not.Null);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Count, Is.EqualTo(5),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[0].LastVerseNum, Is.EqualTo(20),
				"Narrator text at start of verse 21 begins in verse 20.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks[1].InitialStartVerseNumber, Is.EqualTo(21),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(22),
				"Final quote in vernacular spills over into verse 22");
			Assert.That(m_model.GetLastVerseInCurrentQuote(), Is.EqualTo(22));
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last(), Is.EqualTo(m_model.GetLastBlockInCurrentQuote()));

			Assert.That(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 20 && v.EndVerse >= 20), Is.True,
				"Current (anchor) block should contain start of verse we tried to load.");

			m_model.TryLoadBlock(new VerseRef(41, 9, 22, m_testProject.Versification));

			Assert.That(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 22 && v.EndVerse >= 22), Is.True,
				"Current (anchor) block should contain start of verse we tried to load.");

			m_model.TryLoadBlock(new VerseRef(41, 9, 21, m_testProject.Versification));

			Assert.That(m_model.CurrentBlock.AllVerses.Any(v => v.StartVerse <= 21 && v.EndVerse >= 21), Is.True,
				"Current (anchor) block should contain start of verse we tried to load.");
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_NoBlockMatchup_ThrowsInvalidOperationException()
		{
			m_model.SetMode(m_model.Mode, false);
			FindRefInMark(9, 21);
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.That(e.Message, Is.EqualTo("No current reference text block matchup!"));
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
			Assert.That(origPrecedingBlock, Is.EqualTo(m_model.GetNthBlockInCurrentBook(indexOfPrecedingBlock)));
			int c = 0;
			foreach (var correlatedBlock in m_model.CurrentReferenceTextMatchup.CorrelatedBlocks)
			{
				Assert.That(correlatedBlock, Is.EqualTo(m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup + c)), c.ToString());
				c++;
			}

			Assert.That(origFollowingBlock, Is.EqualTo(m_model.GetNthBlockInCurrentBook(origIndexOfFollowingBlock + m_model.CurrentReferenceTextMatchup.CountOfBlocksAddedBySplitting)));
		}

		private void FindRefInMark(int chapter, int verse)
		{
			FindRefInMark(m_model, chapter, verse);
		}

		internal static void FindRefInMark(BlockNavigatorViewModel model, int chapter, int verse)
		{
			while (model.CurrentBlock.ChapterNumber < chapter || model.CurrentEndBlock.LastVerseNum < verse)
			{
				if (model.IsCurrentLocationRelevant && model.CurrentDisplayIndex == model.RelevantBlockCount)
					throw new Exception("Could not find Mark " + chapter + ":" + verse);
				model.LoadNextRelevantBlock();
			}

			Assert.That(model.CurrentBookId, Is.EqualTo("MRK"));
			Assert.That(chapter, Is.EqualTo(model.CurrentBlock.ChapterNumber));
			Assert.That(verse >= model.CurrentBlock.InitialStartVerseNumber && verse <= model.CurrentEndBlock.LastVerseNum, Is.True);
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = CreateTestProject(TestBook.MRK, TestBook.ACT);
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
		}

		[TearDown]
		public void TearDown()
		{
			DeleteTestProjects();
		}

		[Test]
		public void TryLoadBlock_FromRelevantBlockToRelevantBlockInMatchup_RelevantBlockLoaded()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));

			m_model.TryLoadBlock(m_model.GetBlockVerseRef());
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));
		}

		[Test]
		public void TryLoadBlock_FromIrrelevantBlockToRelevantBlockInMatchup_RelevantBlockLoaded()
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			var targetRef = m_model.GetBlockVerseRef();

			int v = 1;
			while (m_model.IsCurrentLocationRelevant)
			{
				m_model.TryLoadBlock(new VerseRef(41, 1, v++));
			}

			m_model.TryLoadBlock(targetRef);
			Assert.That(m_model.CurrentDisplayIndex > 0, Is.True);
		}

		[TestCase(41001000)] // MRK 1:0
		[TestCase(41000001)] // MRK 0:1
		public void TryLoadBlock_FromRelevantBlockToNonScriptureBlock_NotLoaded(int bbbcccvvv)
		{
			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));

			Assert.That(m_model.TryLoadBlock(new VerseRef(bbbcccvvv, ScrVers.English)), Is.False);
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));
		}

		[Test]
		public void LoadNextRelevantBlock_FollowingInsertionOfBlockByApplyingMatchupWithSplits_LoadsARelevantBlock()
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			//int displayIndex = m_model.CurrentBlockDisplayIndex;
			m_model.SetMode(m_model.Mode, true);
			//Assert.That(displayIndex, Is.EqualTo(m_model.CurrentBlockDisplayIndex));
			int displayIndex = m_model.CurrentDisplayIndex;
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			m_model.ApplyCurrentReferenceTextMatchup();
			Assert.That(displayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			do
			{
				m_model.LoadNextRelevantBlock();
				Assert.That(++displayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
				Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			} while (m_model.CurrentBookId == "MRK");

			Assert.That(m_model.CurrentBookId, Is.EqualTo("ACT"));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
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
			Assert.That(matchupForMark85, Is.Not.Null);
			Assert.That(matchupForMark85.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			matchupForMark85.SetReferenceText(0, "Some random text: " + Guid.NewGuid());

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(matchupForMark85, Is.EqualTo(m_model.CurrentReferenceTextMatchup));
			Assert.That(origCurrentBlock, Is.EqualTo(m_model.CurrentBlock));
			Assert.That(matchupForMark85.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchupForMark85.HasOutstandingChangesToApply, Is.False);
			Assert.That(origBlockDisplayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
			Assert.That(origRelevantBlock, Is.EqualTo(m_model.RelevantBlockCount));
			Assert.That(origBlockCount, Is.EqualTo(m_model.BlockCountForCurrentBook));
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
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
			Assert.That(matchupForMark921, Is.Not.Null);
			Assert.That(matchupForMark921.CorrelatedBlocks.Count, Is.EqualTo(5),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(origBlockDisplayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
			var origTextOfFirstBlockInVerse = m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup).GetText(true);
			var origCurrentBlockText = m_model.CurrentBlock.GetText(true);
			Assert.That(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock), Is.False);

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(matchupForMark921, Is.EqualTo(m_model.CurrentReferenceTextMatchup));
			Assert.That(matchupForMark921.CountOfBlocksAddedBySplitting, Is.EqualTo(0));
			Assert.That(matchupForMark921.HasOutstandingChangesToApply, Is.False);
			Assert.That(origBlockDisplayIndex, Is.EqualTo(m_model.CurrentDisplayIndex));
			Assert.That(origRelevantBlock, Is.EqualTo(m_model.RelevantBlockCount),
				"Since the block that was split did not match the filter (i.e., was not relevant), the relevant block count should not change.");
			Assert.That(origBlockCount + 1, Is.EqualTo(m_model.BlockCountForCurrentBook));
			Assert.That(origCurrentBlockText, Is.EqualTo(m_model.CurrentBlock.GetText(true)));
			Assert.That(origTextOfFirstBlockInVerse, Does.StartWith(m_model.GetNthBlockInCurrentBook(m_model.IndexOfFirstBlockInCurrentGroup).GetText(true)));
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_Splits_CurrentBlockDoesNotMatchFilter_ChangesAppliedAndSubsequentIndicesIncremented()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, false);
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 9, 21);
			m_model.CurrentBlock.SetCharacterIdAndCharacterIdInScript("Jesus", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			m_model.CurrentBlock.SetCharacterIdAndCharacterIdInScript("father of demon-possessed boy", 0, m_testProject.Versification);
			m_model.LoadNextRelevantBlock();
			var origNextRelevantBlock = m_model.CurrentBlock;
			m_model.SetMode(BlocksToDisplay.NotYetAssigned, true);

			m_model.TryLoadBlock(new VerseRef(41, 9, 21, m_testProject.Versification));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);

			var matchupForMark921 = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchupForMark921, Is.Not.Null);
			Assert.That(matchupForMark921.CorrelatedBlocks.Count, Is.EqualTo(5),
				"Original first block (narrator) should have been split at start of verse 21.");
			Assert.That(m_testProject.Books[0].Blocks.Contains(m_model.CurrentBlock), Is.False);
			matchupForMark921.SetReferenceText(2, "Blah");

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);

			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks,
				Does.Contain(origNextRelevantBlock));
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_CurrentBlockIsPastLastBlockThatMatchesFilter_ChangesAppliedAndSubsequentIndicesIncremented()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			Assert.That(m_model.TryLoadBlock(new VerseRef(BCVRef.BookToNumber("ACT"), 28, 23, m_testProject.Versification)), Is.True);
			Assert.That(m_model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(0));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(1),
				"Trying to go to the \"next\" block from a position beyond the end of the list should take us back to the beginning.");
			var origNextRelevantBlockText = m_model.CurrentBlock.GetText(true);
			var origNextRelevantBlockIndexInBook = m_model.CurrentBlockIndexInBook;
			Assert.That(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 23), m_testProject.Versification)), Is.True);
			var origRelevantBlockCount = m_model.RelevantBlockCount;

			Assert.That(m_model.IsCurrentLocationRelevant, Is.False);

			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.That(matchup, Is.Not.Null);
			matchup.MatchAllBlocks();

			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(origRelevantBlockCount, Is.EqualTo(m_model.RelevantBlockCount));
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);

			m_model.LoadNextRelevantBlock();
			Assert.That(origNextRelevantBlockText, Is.EqualTo(m_model.CurrentBlock.GetText(true)));
			Assert.That(origNextRelevantBlockIndexInBook, Is.EqualTo(m_model.CurrentBlockIndexInBook));
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(1));
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_BlockMatchupAlreadyApplied_ThrowsInvalidOperationException()
		{
			BlockNavigatorViewModelTests.FindRefInMark(m_model, 8, 5);
			m_model.SetMode(m_model.Mode, true);
			m_model.CurrentReferenceTextMatchup.SetReferenceText(0, "Some random text: " + Guid.NewGuid());
			m_model.ApplyCurrentReferenceTextMatchup();
			var e = Assert.Throws<InvalidOperationException>(() => m_model.ApplyCurrentReferenceTextMatchup());
			Assert.That(e.Message, Does.StartWith("Current reference text block matchup has no outstanding changes!"));
		}

		[Test]
		public void CanNavigateToNextRelevantBlock_CurrentBlockIsInLastBlockMatchup_ReturnsFalse()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);
			Assert.That(m_model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("ACT"), 28, 27), m_testProject.Versification)), Is.True);
			m_model.LoadPreviousRelevantBlock();
			var startIndex = m_model.CurrentReferenceTextMatchup.IndexOfStartBlockInBook;
			do
			{
				Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
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
			m_testProject = CreateTestProject(TestBook.ICO);
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);
		}

		[TearDown]
		public void TearDown()
		{
			DeleteTestProjects();
		}

		// PG-823: Prevent out of range index
		[Test]
		public void ApplyCurrentReferenceTextMatchup_CurrentBlockIsLastRelevantBlockInLastMatchup_DoesNotCrashAndStaysOnCurrentMatchup()
		{
			m_model.SetMode(BlocksToDisplay.NotAssignedAutomatically, true);

			while (m_model.CanNavigateToNextRelevantBlock)
				m_model.LoadNextRelevantBlock();

			var matchup = m_model.CurrentReferenceTextMatchup;

			Assert.That(m_model.CurrentDisplayIndex >= m_model.RelevantBlockCount, Is.True);
			matchup.MatchAllBlocks();
			foreach (var block in matchup.CorrelatedBlocks.Where(b => b.CharacterIsUnclear))
				block.SetCharacterIdAndCharacterIdInScript("Paul", m_model.CurrentBookNumber, m_testProject.Versification);
			Assert.That(matchup.HasOutstandingChangesToApply, Is.True);
			m_model.ApplyCurrentReferenceTextMatchup();

			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.False);
			Assert.That(matchup, Is.EqualTo(m_model.CurrentReferenceTextMatchup));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject = CreateTestProject(TestBook.LUK, TestBook.JHN);
		}

		[TearDown]
		public void TearDown()
		{
			DeleteTestProjects();
		}

		/// <summary>
		/// PG-879:
		/// </summary>
		[Test]
		public void ApplyCurrentReferenceTextMatchup_ApplyCausesSplittingOfUserConfirmedBlockThatDoesNotMatchFilter_NewBlocksAreNotAddedToRelevantBlocks()
		{
			// Set up initial data state
			SimulateDisambiguationForAllBooks(m_testProject);
			var blockToMatchFilter = m_testProject.IncludedBooks.First().GetScriptBlocks().First(b => b.UserConfirmed);
			blockToMatchFilter.UserConfirmed = false;
			blockToMatchFilter.CharacterId = kUnexpectedCharacter;

			// Create model and initialize state
			var model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotYetAssigned);

			// Get into correct state for this test and ensure initial conditions
			Assert.That(model.RelevantBlockCount, Is.EqualTo(1));
			model.SetMode(model.Mode, true);
			Assert.That(model.TryLoadBlock(new VerseRef(new BCVRef(BCVRef.BookToNumber("JHN"), 12, 27), m_testProject.Versification)), Is.True);
			Assert.That(model.CurrentBlock.InitialStartVerseNumber, Is.EqualTo(27));
			Assert.That(model.IsCurrentLocationRelevant, Is.False);
			var matchup = model.CurrentReferenceTextMatchup;
			Assert.That(matchup, Is.Not.Null);
			matchup.MatchAllBlocks();

			model.ApplyCurrentReferenceTextMatchup();

			Assert.That(model.RelevantBlockCount, Is.EqualTo(1));
			Assert.That(model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(model.CanNavigateToNextRelevantBlock, Is.False);
		}

		[Test]
		public void SetMode_BlockHasPreConfirmedCharacterThatWasUserConfirmedWithSameCharacter_IsNotRelevant()
		{
			var charactersForLukC3V14 = new []
			{
				"soldiers",
				"John the Baptist"
			};

			// Set up initial data state
			var i = 0;
			foreach (var block in m_testProject.Books.Single(b => b.BookId == "LUK").GetBlocksForVerse(3, 14).Where(b => b.CharacterIsUnclear))
			{
				block.StyleTag = "qt-s";
				block.CharacterId = charactersForLukC3V14[i++ % charactersForLukC3V14.Length];
				block.UserConfirmed = true;
			}
			Assert.That(i >= charactersForLukC3V14.Length,
				"Test Setup problem: Did not find as many unclear blocks in Luk 3:14 as we thought there would be.");

			// Create model and initialize state
			var model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAssignedAutomatically);

			while (model.CurrentBlock.ChapterNumber <= 3 && model.CanNavigateToNextRelevantBlock)
			{
				Assert.That(model.CurrentBlock.ChapterNumber != 3 || model.CurrentBlock.InitialStartVerseNumber != 14, Is.True);
				model.LoadNextRelevantBlock();
			}
		}
	}

	[TestFixture]
	class BlockNavigatorViewModelTestsWhereFirstRelevantBlockIsAmbiguous
	{
		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			m_testProject = CreateTestProject(TestBook.MRK);
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.Ambiguous);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			DeleteTestProjects();
		}

		[Test]
		public void CanNavigateToPreviousRelevantBlock_OnBlockWhoseRainbowGroupCoversFirstBlock_ReturnsFalse()
		{
			m_model.SetMode(BlocksToDisplay.Ambiguous);
			while (m_model.CanNavigateToPreviousRelevantBlock)
				m_model.LoadPreviousRelevantBlock();
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.True);
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(2));
			var origCurrentBlock = m_model.CurrentBlock;

			m_model.SetMode(m_model.Mode, true);

			Assert.That(m_model.CanNavigateToPreviousRelevantBlock, Is.False,
				m_model.CurrentBlock.ToString());
			Assert.That(m_model.CurrentReferenceTextMatchup.OriginalBlocks,
				Does.Contain(origCurrentBlock));
			Assert.That(m_model.CurrentDisplayIndex, Is.EqualTo(1),
				"Note: Setting AttemptRefBlockMatchup = true should cause the filter to be " +
				"reset and the \"block\" display index");
		}

		[Test]
		public void ApplyCurrentReferenceTextMatchup_HeSaid_SetsReferenceTextsForBlocksInGroupAndUpdatesState()
		{
			// SETUP
			var markNarrator = GetStandardCharacterId("MRK", StandardCharacter.Narrator);
			AddHeSaidAfterQuoteAtEndOfVerseInMark(1, 8, markNarrator);
			var addedHeSaidBlockToMatchExplicitly = AddHeSaidAfterQuoteAtEndOfVerseInMark(1, 11, markNarrator);
			AddHeSaidAfterQuoteAtEndOfVerseInMark(1, 15, markNarrator);
			AddHeSaidAfterQuoteAtEndOfVerseInMark(1, 17, markNarrator);

			m_model.SetMode(BlocksToDisplay.NotAlignedToReferenceText, true);
			while (m_model.CanNavigateToPreviousRelevantBlock)
				m_model.LoadPreviousRelevantBlock();

			BlockNavigatorViewModelTests.FindRefInMark(m_model, 1, 8);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First().InitialStartVerseNumber <= 11, Is.True);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(11));
			const string separator = "***";
			var Mark1V11MatchupBlocks = Join(separator, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true)));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First().InitialStartVerseNumber <= 15, Is.True);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(15));
			m_model.LoadNextRelevantBlock();
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.First().InitialStartVerseNumber <= 17, Is.True);
			Assert.That(m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Last().LastVerseNum, Is.EqualTo(17));
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True, "Setup condition not met");
			m_model.LoadNextRelevantBlock();
			var firstRelevantMatchupPastMark1V17Blocks = Join(separator, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true)));
			m_model.LoadPreviousRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			m_model.LoadPreviousRelevantBlock();
			var matchup = m_model.CurrentReferenceTextMatchup;
			Assert.That(Mark1V11MatchupBlocks, Is.EqualTo(Join(separator, matchup.CorrelatedBlocks.Select(b => b.GetText(true)))));
			Assert.That(addedHeSaidBlockToMatchExplicitly, Is.EqualTo(matchup.OriginalBlocks.Last()));
			bool callbackCalled = false;
			var iHeSaidRow = matchup.CorrelatedBlocks.Count - 1;
			matchup.InsertHeSaidText(iHeSaidRow, (iRow, level, text) =>
			{
				Assert.That(iHeSaidRow, Is.EqualTo(iRow));
				Assert.That(level, Is.EqualTo(0));
				Assert.That(text, Is.EqualTo("he said."));
				Assert.That(callbackCalled, Is.False);
				callbackCalled = true;
			});
			Assert.That(callbackCalled, Is.True);

			var origRelevantBlock = m_model.RelevantBlockCount;
			var origBlockDisplayIndex = m_model.CurrentDisplayIndex;

			// SUT
			m_model.ApplyCurrentReferenceTextMatchup();

			// VERIFY
			Assert.That(matchup, Is.EqualTo(m_model.CurrentReferenceTextMatchup));
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True,
				"We leave the matchup that was explicitly applied in the collection so the user can use \"Prev\" to get back to it");

			Assert.That(origBlockDisplayIndex - 1, Is.EqualTo(m_model.CurrentDisplayIndex),
				"One of the previous matchups (for Mark 1:8) is no longer relevant.");
			Assert.That(origRelevantBlock - 3, Is.EqualTo(m_model.RelevantBlockCount),
				"The three other matchups with the \"he said\" text should no longer be considered relevant.");
			Assert.That(m_model.CanNavigateToNextRelevantBlock, Is.True);
			m_model.LoadNextRelevantBlock();
			Assert.That(Join(separator, m_model.CurrentReferenceTextMatchup.CorrelatedBlocks.Select(b => b.GetText(true))),
				Is.EqualTo(firstRelevantMatchupPastMark1V17Blocks));
		}

		private Block AddHeSaidAfterQuoteAtEndOfVerseInMark(int chapterNum, int verseNum, string markNarrator)
		{
			var mark = m_testProject.IncludedBooks.Single(b => b.BookId == "MRK");
			var markBlocks = mark.GetScriptBlocks();
			var iBlock = markBlocks.TakeWhile(b => b.ChapterNumber <= chapterNum)
				.IndexOf(b => b.LastVerseNum == verseNum);
			Assert.That(iBlock, Is.GreaterThan(0), "Setup condition not met.");
			while (iBlock + 1 < markBlocks.Count && markBlocks[iBlock + 1].IsScripture && markBlocks[iBlock + 1].LastVerseNum == verseNum)
				iBlock++;
			var block = markBlocks[iBlock];
			var lastScriptText = (ScriptText)block.BlockElements.Last();
			var lastTextContent = lastScriptText.Content;
			if (lastTextContent.Last() != ' ')
				lastTextContent = lastScriptText.Content += " ";
			Assert.That(lastTextContent, Does.EndWith("” "), "Setup condition not met.");

			var characterToReapply = block.CharacterId;
			lastScriptText.Content += "wacci kum.";
			var newBlock = mark.SplitBlock(block, verseNum.ToString(), lastTextContent.Length, false);
			block.CharacterId = characterToReapply;
			newBlock.CharacterId = markNarrator;
			return newBlock;
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			m_testProject = CreateTestProject(TestBook.MRK);
			var mark = m_testProject.IncludedBooks.Single();
			var bookNum = mark.BookNumber;
			var lastBlockInChapter = m_testProject.IncludedBooks.Single().Blocks.First(b =>
				b.ChapterNumber == 15 &&
				b.LastVerseNum == ScrVers.English.GetLastVerse(bookNum, b.ChapterNumber) &&
				b.StartsAtVerseStart &&
				b.AllVerses.Skip(1).Any() &&
				b.CharacterIs("MRK", StandardCharacter.Narrator));
			// We have to manually split the block to separate the initial verses which already don't match
			// the reference text from the last two verses, which are just plain narrator verses in both the
			// Vernacular and the English reference text.
			lastBlockInChapter = mark.SplitBlock(lastBlockInChapter,
				((Verse)(lastBlockInChapter.AllVerses.Reverse().ElementAt(2))).Number,
				PortionScript.kSplitAtEndOfVerse, true,
				lastBlockInChapter.CharacterId);
			m_targetReference = new VerseRef(bookNum, lastBlockInChapter.ChapterNumber, lastBlockInChapter.InitialStartVerseNumber);
			var tempModel = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAlignedToReferenceText);
			Assert.That(tempModel.TryLoadBlock(m_targetReference), Is.True);
			Assert.That(tempModel.IsCurrentLocationRelevant, Is.False, "Before adding the extra verse, this block should not be relevant.");

			// Finally, we add the extra verse (not in the reference text)
			var extraVerseNum = lastBlockInChapter.LastVerseNum + 1;
			lastBlockInChapter.AddVerse(extraVerseNum, "This verse will not have a corresponding verse in the reference text.");
		}

		[SetUp]
		public void SetUp()
		{
			m_model = new BlockNavigatorViewModel(m_testProject, BlocksToDisplay.NotAlignedToReferenceText);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			DeleteTestProjects();
		}

		/// <summary>
		/// PG-1261: This test indirectly tests the IsRelevant method, which should determine that the vernacular
		/// block at the end of Mark 15 IS relevant because it has an additional verse not in the reference text.
		/// </summary>
		[Test]
		public void IsRelevant_VernBlockHasUnexpectedVerseNotInReferenceText_ReturnsFalse()
		{
			m_model.SetMode(m_model.Mode, true);
			Assert.That(m_model.TryLoadBlock(m_targetReference), Is.True);
			Assert.That(m_model.IsCurrentLocationRelevant, Is.True);
		}
	}

	[TestFixture]
	class BlockNavigatorViewModelTestsForMatCuk
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			DeleteTestProjects();
		}

		[Test]
		public void IsRelevant_MissingExpectedQuote_BlockWithMultiple_BlocksWithMissingExpectedQuotes()
		{
			Project testProject = CreateTestProject(TestBook.MAT);
			BlockNavigatorViewModel model = new BlockNavigatorViewModel(testProject, BlocksToDisplay.MissingExpectedQuote);
			model.SetMode(model.Mode, true);

			model.TryLoadBlock(new VerseRef(40, 3, 14));
			Assert.That(model.CurrentReferenceTextMatchup.OriginalBlocks.Last().LastVerseNum, Is.EqualTo(14));
			model.LoadNextRelevantBlock();

			// MAT 3:15-17 should not be relevant
			Assert.That(model.CurrentBookId, Is.EqualTo("MAT"));
			Assert.That(model.CurrentBlock.ChapterNumber == 3 && model.CurrentBlock.InitialStartVerseNumber <= 17, Is.False);
		}
	}
}
