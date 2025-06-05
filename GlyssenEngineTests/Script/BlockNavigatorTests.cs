using System.Collections.Generic;
using System.Linq;
using GlyssenEngine.Script;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenEngineTests.Script
{
	[TestFixture]
	class BlockNavigatorTests
	{
		private List<BookScript> m_books;
		private BlockNavigator m_navigator;
		private BlockNavigator m_navigatorForMultiBlockTests;

		[SetUp]
		public void SetUp()
		{
			var blockA = new Block("ip");
			var blockB = new Block("p", 1, 1);
			var blockC = new Block("p", 2, 7);
			var bookScriptA = new BookScript("LUK", new List<Block> { blockA, blockB, blockC }, ScrVers.English);
			var blockD = new Block("ip");
			var blockE = new Block("p", 1, 1);
			var blockF = new Block("p", 5, 7);
			var blockG = new Block("p", 5, 7);
			var bookScriptB = new BookScript("ROM", new List<Block> { blockD, blockE, blockF, blockG }, ScrVers.English);
			m_books = new List<BookScript> { bookScriptA, bookScriptB };

			m_navigator = new BlockNavigator(m_books);

			var blockH = new Block("ip") { MultiBlockQuote = MultiBlockQuote.None };
			var blockI = new Block("p", 1, 1) { MultiBlockQuote = MultiBlockQuote.Start };
			var blockJ = new Block("p", 1, 1) { MultiBlockQuote = MultiBlockQuote.Continuation };
			var blockK = new Block("p", 1, 2) { MultiBlockQuote = MultiBlockQuote.Continuation };
			var blockL = new Block("p", 1, 2) { MultiBlockQuote = MultiBlockQuote.None };
			var bookScriptJud = new BookScript("JUD", new List<Block> { blockH, blockI, blockJ, blockK, blockL }, ScrVers.English);
			var bookList = new List<BookScript> { bookScriptJud };

			m_navigatorForMultiBlockTests = new BlockNavigator(bookList);
		}

		[Test]
		public void GetBookScriptContainingBlock_EmptyList_ReturnsNull()
		{
			m_navigator.GetBookScriptContainingBlock(new Block());
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockNotThere_ReturnsNull()
		{
			m_navigator.GetBookScriptContainingBlock(new Block());
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockIsThere_FirstBook_FirstBlock_ReturnsCorrectBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.That(firstBook, Is.EqualTo(m_navigator.GetBookScriptContainingBlock(firstBlock)));
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockIsThere_MiddleBook_MiddleBlock_ReturnsCorrectBook()
		{
			var middleBook = m_books[1];
			var middleBlock = middleBook[1];
			Assert.That(middleBook, Is.EqualTo(m_navigator.GetBookScriptContainingBlock(middleBlock)));
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockIsThere_LastBook_LastBlock_ReturnsCorrectBook()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			Assert.That(lastBook, Is.EqualTo(m_navigator.GetBookScriptContainingBlock(lastBlock)));
		}

		[Test]
		public void IsLastBlockInBook_True()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			Assert.That(m_navigator.IsLastBlockInBook(firstBook, lastBlock));
		}

		[Test]
		public void IsLastBlockInBook_False()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.That(m_navigator.IsLastBlockInBook(firstBook, firstBlock), Is.False);
		}

		[Test]
		public void IsLastBlock_True()
		{
			var lastBook = m_books.Last();
			m_navigator.CurrentBlock = lastBook.GetScriptBlocks().Last();
			Assert.That(m_navigator.IsLastBlock());
		}

		[Test]
		public void IsLastBlock_False()
		{
			var firstBook = m_books.First();
			m_navigator.CurrentBlock = firstBook[0];
			Assert.That(m_navigator.IsLastBlock(), Is.False);
		}

		[Test]
		public void IsLastBlock_LastBlockInOtherBook_False()
		{
			var firstBook = m_books.First();
			m_navigator.CurrentBlock = firstBook.GetScriptBlocks().Last();
			Assert.That(m_navigator.IsLastBlock(), Is.False);
		}

		[Test]
		public void IsLastBook_True()
		{
			var lastBook = m_books.Last();
			Assert.That(m_navigator.IsLastBook(lastBook));
		}

		[Test]
		public void IsFirstBlockInBook_True()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.That(m_navigator.IsFirstBlockInBook(firstBook, firstBlock));
		}

		[Test]
		public void IsFirstBlockInBookFalse()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			Assert.That(m_navigator.IsFirstBlockInBook(firstBook, lastBlock), Is.False);
		}

		[Test]
		public void IsFirstBlock_True()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.That(m_navigator.IsFirstBlock(firstBlock));
		}

		[Test]
		public void IsFirstBlock_False()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			Assert.That(m_navigator.IsFirstBlock(lastBlock), Is.False);
		}

		[Test]
		public void IsFirstBlock_FirstBlockInOtherBook_False()
		{
			var lastBook = m_books.Last();
			var firstBlock = lastBook[0];
			Assert.That(m_navigator.IsFirstBlock(firstBlock), Is.False);
		}

		[Test]
		public void IsFirstBook_True()
		{
			var firstBook = m_books.First();
			Assert.That(m_navigator.IsFirstBook(firstBook));
		}

		[Test]
		public void GoToNextBlock_FromFirst_AdvancesToNextBlockInSameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = firstBlock;
			Assert.That(m_navigator.GoToNextBlock(), Is.EqualTo(secondBlock));
		}

		[Test]
		public void GoToNextBlock_FromMultiBlockIndexRepresentingFirstTwoBlocksInBook_AdvancesToThirdBlock()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var thirdBlock = firstBook[2];
			m_navigator.CurrentBlock = firstBlock;
			m_navigator.ExtendCurrentBlockGroup(1);
			Assert.That(m_navigator.GoToNextBlock(), Is.EqualTo(thirdBlock));
		}

		[Test]
		public void GoToNextBlock_FromLastBlockInBook_AdvancesToFirstBlockInNextBook()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			m_navigator.CurrentBlock = lastBlock;
			Assert.That(m_navigator.GoToNextBlock(), Is.EqualTo(firstBlock));
		}

		[TestCase(3)]
		[TestCase(2)]
		public void GoToNextBlock_FromMultiBlockIndexRepresentingLastThreeBlocksInBook_AdvancesToFirstBlockInNextBook(int numberOfBlocksToCover)
		{
			var firstBook = m_books.First();
			var blocks = firstBook.GetScriptBlocks();
			var thirdBlockFromEnd = blocks[blocks.Count - numberOfBlocksToCover];
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			m_navigator.CurrentBlock = thirdBlockFromEnd;
			m_navigator.ExtendCurrentBlockGroup((uint)numberOfBlocksToCover - 1);
			Assert.That(m_navigator.GoToNextBlock(), Is.EqualTo(firstBlock));
		}

		[Test]
		public void GetNextBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = firstBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.That(secondBlock, Is.EqualTo(m_navigator.GetNextBlock()));
			Assert.That(currentBlock, Is.EqualTo(m_navigator.CurrentBlock));
			Assert.That(currentBook, Is.EqualTo(m_navigator.CurrentBook));
		}

		[Test]
		public void GetNextBlock_NextBook()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			m_navigator.CurrentBlock = lastBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.That(firstBlock, Is.EqualTo(m_navigator.GetNextBlock()));
			Assert.That(currentBlock, Is.EqualTo(m_navigator.CurrentBlock));
			Assert.That(currentBook, Is.EqualTo(m_navigator.CurrentBook));
		}

		[Test]
		public void GoToNextBlock_LastReturnsNull()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			m_navigator.CurrentBlock = lastBlock;
			Assert.That(m_navigator.GoToNextBlock(), Is.Null);
		}

		//[Test]
		//public void GoToPreviousBlock_SameBook()
		//{
		//	var firstBook = m_books.First();
		//	var firstBlock = firstBook[0];
		//	var secondBlock = firstBook[1];
		//	m_navigator.CurrentBlock = secondBlock;
		//	Assert.That(firstBlock, Is.EqualTo(m_navigator.GoToPreviousBlock()));
		//}

		//[Test]
		//public void GoToPreviousBlock_PreviousBook()
		//{
		//	var secondBook = m_books[1];
		//	var firstBlock = secondBook[0];
		//	var firstBook = m_books.First();
		//	var lastBlock = firstBook.GetScriptBlocks().Last();
		//	m_navigator.CurrentBlock = firstBlock;
		//	Assert.That(lastBlock, Is.EqualTo(m_navigator.GoToPreviousBlock()));
		//}

		[Test]
		public void GetPreviousBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = secondBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.That(firstBlock, Is.EqualTo(m_navigator.GetPreviousBlock()));
			Assert.That(currentBlock, Is.EqualTo(m_navigator.CurrentBlock));
			Assert.That(currentBook, Is.EqualTo(m_navigator.CurrentBook));
		}

		[Test]
		public void GetPreviousBlock_PreviousBook()
		{
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			m_navigator.CurrentBlock = firstBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.That(lastBlock, Is.EqualTo(m_navigator.GetPreviousBlock()));
			Assert.That(currentBlock, Is.EqualTo(m_navigator.CurrentBlock));
			Assert.That(currentBook, Is.EqualTo(m_navigator.CurrentBook));
		}

		[Test]
		public void GetNthNextBlockWithinBook_NEquals1()
		{
			m_navigator.GoToFirstBlock();
			Block result = m_navigator.GetNthNextBlockWithinBook(1);
			Assert.That(m_books.First()[1], Is.EqualTo(result));
		}

		[Test]
		public void GetNthNextBlockWithinBook_NGreaterThan1()
		{
			m_navigator.GoToFirstBlock();
			Block result = m_navigator.GetNthNextBlockWithinBook(2);
			Assert.That(m_books.First()[2], Is.EqualTo(result));
		}

		[Test]
		public void GetNthNextBlockWithinBook_BeyondBook_ReturnsNull()
		{
			m_navigator.GoToFirstBlock();
			Block result = m_navigator.GetNthNextBlockWithinBook(3);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetNthNextBlockWithinBook_FromSpecificBook_NEquals1()
		{
			Block result = m_navigator.GetNthNextBlockWithinBook(1, m_books[0].Blocks[0]);
			Assert.That(m_books.First()[1], Is.EqualTo(result));
		}

		[Test]
		public void GetNthNextBlockWithinBook_FromSpecificBook_NGreaterThan1()
		{
			Block result = m_navigator.GetNthNextBlockWithinBook(1, m_books[0].Blocks[1]);
			Assert.That(m_books.First()[2], Is.EqualTo(result));
		}

		[Test]
		public void GetNthNextBlockWithinBook_FromSpecificBook_BeyondBook_ReturnsNull()
		{
			Block result = m_navigator.GetNthNextBlockWithinBook(2, m_books[0].Blocks[1]);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_NEquals1()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.GetNthPreviousBlockWithinBook(1);
			Assert.That(m_books[1].Blocks[1], Is.EqualTo(result));
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_NGreaterThan1()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.GetNthPreviousBlockWithinBook(2);
			Assert.That(m_books[1].Blocks[0], Is.EqualTo(result));
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_BeyondBook_ReturnsNull()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.GetNthPreviousBlockWithinBook(3);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_FromSpecificBook_NEquals1()
		{
			Block result = m_navigator.GetNthPreviousBlockWithinBook(1, m_books[1].Blocks[2]);
			Assert.That(m_books[1].Blocks[1], Is.EqualTo(result));
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_FromSpecificBook_NGreaterThan1()
		{
			Block result = m_navigator.GetNthPreviousBlockWithinBook(2, m_books[1].Blocks[2]);
			Assert.That(m_books[1].Blocks[0], Is.EqualTo(result));
		}

		[Test]
		public void GetNthPreviousBlockWithinBook_FromSpecificBook_BeyondBook_ReturnsNull()
		{
			Block result = m_navigator.GetNthPreviousBlockWithinBook(3, m_books[1].Blocks[2]);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetNextNBlocksWithinBook_OneBlock_WithinBook()
		{
			var secondBlock = m_books.First()[1];
			IEnumerable<Block> result = m_navigator.GetNextNBlocksWithinBook(1);
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(secondBlock, Is.EqualTo(result.First()));
		}

		[Test]
		public void GetNextNBlocksWithinBook_OneBlock_StartAtBookEnd_ReturnsEmpty()
		{
			m_navigator.CurrentBlock = m_books.First().Blocks.Last();
			IEnumerable<Block> result = m_navigator.GetNextNBlocksWithinBook(1);
			Assert.That(result.Count(), Is.EqualTo(0));
		}

		[Test]
		public void GetNextNBlocksWithinBook_MultipleBlocks_WithinBook()
		{
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			IEnumerable<Block> result = m_navigator.GetNextNBlocksWithinBook(2);
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(secondBlock, Is.EqualTo(result.First()));
			Assert.That(thirdBlock, Is.EqualTo(result.Last()));
		}

		[Test]
		public void GetNextNBlocksWithinBook_MultipleBlocks_StopsAtBookEnd()
		{
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			IEnumerable<Block> result = m_navigator.GetNextNBlocksWithinBook(3);
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(secondBlock, Is.EqualTo(result.First()));
			Assert.That(thirdBlock, Is.EqualTo(result.Last()));
		}

		[Test]
		public void GetPreviousNBlocksWithinBook_OneBlock_WithinBook()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			m_navigator.CurrentBlock = secondBlock;
			IEnumerable<Block> result = m_navigator.GetPreviousNBlocksWithinBook(1);
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(firstBlock, Is.EqualTo(result.First()));
		}

		[Test]
		public void GetPreviousNBlocksWithinBook_OneBlock_StartAtBookBegin_ReturnsEmpty()
		{
			IEnumerable<Block> result = m_navigator.GetPreviousNBlocksWithinBook(1);
			Assert.That(result.Count(), Is.EqualTo(0));
		}

		[Test]
		public void GetPreviousNBlocksWithinBook_MultipleBlocks_WithinBook()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			m_navigator.CurrentBlock = thirdBlock;
			IEnumerable<Block> result = m_navigator.GetPreviousNBlocksWithinBook(2);
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(firstBlock, Is.EqualTo(result.First()));
			Assert.That(secondBlock, Is.EqualTo(result.Last()));
		}

		[Test]
		public void GetPreviousNBlocksWithinBook_MultipleBlocks_StopsAtBookBegin()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			m_navigator.CurrentBlock = thirdBlock;
			IEnumerable<Block> result = m_navigator.GetPreviousNBlocksWithinBook(3);
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(firstBlock, Is.EqualTo(result.First()));
			Assert.That(secondBlock, Is.EqualTo(result.Last()));
		}

		//[Test]
		//public void GoToPreviousBlock_FirstReturnsNull()
		//{
		//	var firstBook = m_books.First();
		//	var firstBlock = firstBook[0];
		//	m_navigator.CurrentBlock = firstBlock;
		//	Assert.That(m_navigator.GoToPreviousBlock(), Is.Null);
		//}

		[Test]
		public void CurrentBlock_StartsAtFirstBlock()
		{
			Assert.That(m_books.First()[0], Is.EqualTo(m_navigator.CurrentBlock));
		}

		[Test]
		public void CurrentBlock_StartsAtFirstBook()
		{
			Assert.That(m_books.First(), Is.EqualTo(m_navigator.CurrentBook));
		}

		//[Test]
		//public void GoToNextBlock_GoToPreviousBlock_BackToFirst()
		//{
		//	m_navigator.GoToNextBlock();
		//	Assert.That(m_books.First()[0], Is.EqualTo(m_navigator.GoToPreviousBlock()));
		//}

		//[Test]
		//public void GoToPreviousBlock_GoToNextBlock_BackToLast()
		//{
		//	m_navigator.CurrentBlock = m_books.Last().GetScriptBlocks().Last();
		//	m_navigator.GoToPreviousBlock();
		//	Assert.That(m_books.Last().GetScriptBlocks().Last(), Is.EqualTo(m_navigator.GoToNextBlock()));
		//}

		[Test]
		public void GetIndices_FirstBlock()
		{
			Assert.That(new BookBlockIndices(0, 0), Is.EqualTo(m_navigator.GetIndices()));
		}

		[Test]
		public void GetIndices_SecondBlock()
		{
			m_navigator.CurrentBlock = m_books.First().GetScriptBlocks()[1];
			Assert.That(new BookBlockIndices(0, 1), Is.EqualTo(m_navigator.GetIndices()));
		}

		[Test]
		public void GetIndices_LastBlock()
		{
			m_navigator.CurrentBlock = m_books.Last().GetScriptBlocks()[3];
			Assert.That(new BookBlockIndices(1, 3), Is.EqualTo(m_navigator.GetIndices()));
		}

		[Test]
		public void SetIndices_FirstBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(0, 0));
			Assert.That(m_books.First().GetScriptBlocks()[0], Is.EqualTo(m_navigator.CurrentBlock));
		}

		[Test]
		public void SetIndices_SecondBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(0, 1));
			Assert.That(m_books.First().GetScriptBlocks()[1], Is.EqualTo(m_navigator.CurrentBlock));
		}

		[Test]
		public void SetIndices_LastBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(1, 3));
			Assert.That(m_books.Last().GetScriptBlocks()[3], Is.EqualTo(m_navigator.CurrentBlock));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_InvalidRef_ReturnsNull()
		{
			VerseRef.TryParse("REV 5:5-3", out var verseRef);
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(verseRef);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetIndicesOfSpecificBlock_FirstBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.First().GetScriptBlocks()[0]);
			Assert.That(new BookBlockIndices(0, 0), Is.EqualTo(result));
		}

		[Test]
		public void GetIndicesOfSpecificBlock_SecondBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.First().GetScriptBlocks()[1]);
			Assert.That(new BookBlockIndices(0, 1), Is.EqualTo(result));
		}

		[Test]
		public void GetIndicesOfSpecificBlock_LastBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.Last().GetScriptBlocks()[3]);
			Assert.That(new BookBlockIndices(1, 3), Is.EqualTo(result));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_BookNotIncluded_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(65, 1, 2));
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_ChapterNotFound_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 3, 7));
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_VerseNotFound_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 2, 17));
			Assert.That(result, Is.Null);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_InFirstBook_ReturnsIndices()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 1, 1));
			Assert.That(result.BookIndex, Is.EqualTo(0));
			Assert.That(result.BlockIndex, Is.EqualTo(1));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_TwoBlockAtSameReference_ReturnsIndicesForFirstMatch()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("ROM"), 5, 7));
			Assert.That(result.BookIndex, Is.EqualTo(1));
			Assert.That(result.BlockIndex, Is.EqualTo(2));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_MultiBlockAtSameReference_ReturnsIndicesForFirstStartBlock()
		{
			var result = m_navigatorForMultiBlockTests.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("JUD"), 1, 1));
			Assert.That(result.BookIndex, Is.EqualTo(0));
			Assert.That(result.BlockIndex, Is.EqualTo(1));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_MultiBlockAtDifferentReference_ReturnsIndicesForFirstStartBlock()
		{
			var result = m_navigatorForMultiBlockTests.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("JUD"), 1, 2));
			Assert.That(result.BookIndex, Is.EqualTo(0));
			Assert.That(result.BlockIndex, Is.EqualTo(1));
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_MultiBlockAtDifferentReference_AllowMidQuoteBlock_ReturnsIndicesForMidQuoteBlock()
		{
			var result = m_navigatorForMultiBlockTests.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("JUD"), 1, 2), true);
			Assert.That(result.BookIndex, Is.EqualTo(0));
			Assert.That(result.BlockIndex, Is.EqualTo(3));
		}
	}
}
