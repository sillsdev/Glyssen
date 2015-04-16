using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Bundle;
using ProtoScript.Character;
using ProtoScript.Dialogs;
using ProtoScript.Quote;
using SIL.WritingSystems;

namespace ProtoScriptTests.Dialogs
{
	[TestFixture]
	class BlockNavigatorViewModelTests
	{
		private const string kTest = "test~~";

		private Project m_testProject;
		private BlockNavigatorViewModel m_model;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			m_testProject = CreateTestProject();
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
			DeleteTestProjectFolder();
		}

		public static void DeleteTestProjectFolder()
		{
			var testProjFolder = Path.Combine(Project.ProjectsBaseFolder, kTest);
			if (Directory.Exists(testProjFolder))
				Directory.Delete(testProjFolder, true);
		}

		public static Project CreateTestProject()
		{
			DeleteTestProjectFolder();
			var sampleMetadata = new DblMetadata();
			sampleMetadata.AvailableBooks = new List<Book>();
			var bookOfMark = new Book();
			bookOfMark.Code = "MRK";
			bookOfMark.IncludeInScript = true;
			bookOfMark.LongName = "Gospel of Mark";
			bookOfMark.ShortName = "Mark";
			sampleMetadata.AvailableBooks.Add(bookOfMark);
			sampleMetadata.FontFamily = "Times New Roman";
			sampleMetadata.FontSizeInPoints = 12;
			sampleMetadata.id = kTest;
			sampleMetadata.language = new DblMetadataLanguage { iso = kTest };
			sampleMetadata.identification = new DblMetadataIdentification { name = "test~~" };
			sampleMetadata.ProjectStatus.QuoteSystemStatus = QuoteSystemStatus.Obtained;
			sampleMetadata.QuoteSystem = GetTestQuoteSystem();

			XmlDocument sampleMark = new XmlDocument();
			sampleMark.LoadXml(Properties.Resources.TestMRK);
			UsxDocument mark = new UsxDocument(sampleMark);

			var project = new Project(sampleMetadata, new[] { mark }, SfmLoader.GetUsfmStylesheet());

			// Wait for quote parse to finish
			while (project.ProjectState != ProjectState.FullyInitialized)
				Thread.Sleep(100);

			return Project.Load(Project.GetProjectFilePath(kTest, kTest, Project.GetDefaultRecordingProjectName(kTest)));
		}

		private static QuoteSystem GetTestQuoteSystem()
		{
			QuoteSystem testQuoteSystem = new QuoteSystem();
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			testQuoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“‘“", 3, QuotationMarkingSystemType.Normal));
			return testQuoteSystem;
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
		public void SetMode_AllExpectedQuotes_UserConfirmedBlockSkipped()
		{
			// TODO (PG-70, part 1): When Jim has finished his part of PG-42, we need to add the new IsExpected field into the data.
			// When we do that, we should also update the TestCharacterVerse.txt to add this new column of data. The easiest way would
			// be to do a reg-ex Replace, as follows:
			// Find:True
			// Replace:True\t\True
			// This will make all the Dialogue quotes also be expected quotes (and it will allow this test to continue to pass)
			m_model.Mode = BlocksToDisplay.AllScripture;
			FindRefInMark(1, 17);
			var block1 = m_model.CurrentBlock;
			m_model.LoadNextRelevantBlock();
			//var block2 = m_model.CurrentBlock;

			m_model.Mode = BlocksToDisplay.AllExpectedQuotes;

			Assert.IsTrue(m_model.RelevantBlockCount > 0);
			Assert.AreEqual("MRK 1:16-17", m_model.GetBlockReferenceString());
			m_model.LoadNextRelevantBlock();
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
			Assert.IsTrue(m_model.GetIsBlockScripture(m_model.CurrentBlock));
			Assert.IsTrue(m_model.GetIsBlockScripture(m_model.CurrentBlockIndexInBook));
		}

		[Test]
		public void GetIsBlockScripture_TitleBlock_ReturnsFalse()
		{
			Assert.IsFalse(m_model.GetIsBlockScripture(m_testProject.IncludedBooks[0].Blocks[0]));
			Assert.IsFalse(m_model.GetIsBlockScripture(0));
		}

		private void FindRefInMark(int chapter, int verse)
		{
			while (m_model.CurrentBlock.ChapterNumber <= chapter && m_model.CurrentBlock.InitialStartVerseNumber != verse)
				m_model.LoadNextRelevantBlock();
			Assert.AreEqual("MRK", m_model.CurrentBookId);
			Assert.AreEqual(chapter, m_model.CurrentBlock.ChapterNumber);
			Assert.AreEqual(verse, m_model.CurrentBlock.InitialStartVerseNumber);			
		}
	}
}
