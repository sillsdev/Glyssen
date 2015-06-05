using System.Collections.Generic;
using System.Linq;
using Glyssen;
using NUnit.Framework;
using Paratext;
using SIL.Scripture;

namespace GlyssenTests
{
	[TestFixture]
	class BlockNavigatorTests
	{
		private List<BookScript> m_books;
		private BlockNavigator m_navigator;

		[SetUp]
		public void SetUp()
		{
			var blockA = new Block("ip");
			var blockB = new Block("p", 1, 1);
			var blockC = new Block("p", 2, 7);
			var bookScriptA = new BookScript("LUK", new List<Block> { blockA, blockB, blockC });
			var blockD = new Block("ip");
			var blockE = new Block("p", 1, 1);
			var blockF = new Block("p", 5, 7);
			var blockG = new Block("p", 5, 7);
			var bookScriptB = new BookScript("ROM", new List<Block> { blockD, blockE, blockF, blockG });
			m_books = new List<BookScript> { bookScriptA, bookScriptB };

			m_navigator = new BlockNavigator(m_books);
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
			Assert.AreEqual(firstBook, m_navigator.GetBookScriptContainingBlock(firstBlock));
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockIsThere_MiddleBook_MiddleBlock_ReturnsCorrectBook()
		{
			var middleBook = m_books[1];
			var middleBlock = middleBook[1];
			Assert.AreEqual(middleBook, m_navigator.GetBookScriptContainingBlock(middleBlock));
		}

		[Test]
		public void GetBookScriptContainingBlock_BlockIsThere_LastBook_LastBlock_ReturnsCorrectBook()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			Assert.AreEqual(lastBook, m_navigator.GetBookScriptContainingBlock(lastBlock));
		}

		[Test]
		public void IsLastBlockInBook_True()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			Assert.AreEqual(true, m_navigator.IsLastBlockInBook(firstBook, lastBlock));
		}

		[Test]
		public void IsLastBlockInBook_False()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.AreEqual(false, m_navigator.IsLastBlockInBook(firstBook, firstBlock));
		}

		[Test]
		public void IsLastBlock_True()
		{
			var lastBook = m_books.Last();
			m_navigator.CurrentBlock = lastBook.GetScriptBlocks().Last();
			Assert.AreEqual(true, m_navigator.IsLastBlock());
		}

		[Test]
		public void IsLastBlock_False()
		{
			var firstBook = m_books.First();
			m_navigator.CurrentBlock = firstBook[0];
			Assert.AreEqual(false, m_navigator.IsLastBlock());
		}

		[Test]
		public void IsLastBlock_LastBlockInOtherBook_False()
		{
			var firstBook = m_books.First();
			m_navigator.CurrentBlock = firstBook.GetScriptBlocks().Last();
			Assert.AreEqual(false, m_navigator.IsLastBlock());
		}

		[Test]
		public void IsLastBook_True()
		{
			var lastBook = m_books.Last();
			Assert.AreEqual(true, m_navigator.IsLastBook(lastBook));
		}

		[Test]
		public void IsFirstBlockInBook_True()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.AreEqual(true, m_navigator.IsFirstBlockInBook(firstBook, firstBlock));
		}

		[Test]
		public void IsFirstBlockInBookFalse()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			Assert.AreEqual(false, m_navigator.IsFirstBlockInBook(firstBook, lastBlock));
		}

		[Test]
		public void IsFirstBlock_True()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			Assert.AreEqual(true, m_navigator.IsFirstBlock(firstBlock));
		}

		[Test]
		public void IsFirstBlock_False()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			Assert.AreEqual(false, m_navigator.IsFirstBlock(lastBlock));
		}

		[Test]
		public void IsFirstBlock_FirstBlockInOtherBook_False()
		{
			var lastBook = m_books.Last();
			var firstBlock = lastBook[0];
			Assert.AreEqual(false, m_navigator.IsFirstBlock(firstBlock));
		}

		[Test]
		public void IsFirstBook_True()
		{
			var firstBook = m_books.First();
			Assert.AreEqual(true, m_navigator.IsFirstBook(firstBook));
		}

		[Test]
		public void GetNextBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = firstBlock;
			Assert.AreEqual(secondBlock, m_navigator.NextBlock());
		}

		[Test]
		public void GetNextBlock_NextBook()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			m_navigator.CurrentBlock = lastBlock;
			Assert.AreEqual(firstBlock, m_navigator.NextBlock());
		}

		[Test]
		public void PeekNextBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = firstBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.AreEqual(secondBlock, m_navigator.PeekNextBlock());
			Assert.AreEqual(currentBlock, m_navigator.CurrentBlock);
			Assert.AreEqual(currentBook, m_navigator.CurrentBook);
		}

		[Test]
		public void PeekNextBlock_NextBook()
		{
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			m_navigator.CurrentBlock = lastBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.AreEqual(firstBlock, m_navigator.PeekNextBlock());
			Assert.AreEqual(currentBlock, m_navigator.CurrentBlock);
			Assert.AreEqual(currentBook, m_navigator.CurrentBook);
		}

		[Test]
		public void GetNextBlock_LastReturnsNull()
		{
			var lastBook = m_books.Last();
			var lastBlock = lastBook.GetScriptBlocks().Last();
			m_navigator.CurrentBlock = lastBlock;
			Assert.IsNull(m_navigator.NextBlock());
		}

		[Test]
		public void GetPreviousBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = secondBlock;
			Assert.AreEqual(firstBlock, m_navigator.PreviousBlock());
		}

		[Test]
		public void GetPreviousBlock_PreviousBook()
		{
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			m_navigator.CurrentBlock = firstBlock;
			Assert.AreEqual(lastBlock, m_navigator.PreviousBlock());
		}

		[Test]
		public void PeekPreviousBlock_SameBook()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			var secondBlock = firstBook[1];
			m_navigator.CurrentBlock = secondBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.AreEqual(firstBlock, m_navigator.PeekPreviousBlock());
			Assert.AreEqual(currentBlock, m_navigator.CurrentBlock);
			Assert.AreEqual(currentBook, m_navigator.CurrentBook);
		}

		[Test]
		public void PeekPreviousBlock_PreviousBook()
		{
			var secondBook = m_books[1];
			var firstBlock = secondBook[0];
			var firstBook = m_books.First();
			var lastBlock = firstBook.GetScriptBlocks().Last();
			m_navigator.CurrentBlock = firstBlock;
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			Assert.AreEqual(lastBlock, m_navigator.PeekPreviousBlock());
			Assert.AreEqual(currentBlock, m_navigator.CurrentBlock);
			Assert.AreEqual(currentBook, m_navigator.CurrentBook);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_NEquals1()
		{
			m_navigator.NavigateToFirstBlock();
			Block result = m_navigator.PeekNthNextBlockWithinBook(1);
			Assert.AreEqual(m_books.First()[1], result);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_NGreaterThan1()
		{
			m_navigator.NavigateToFirstBlock();
			Block result = m_navigator.PeekNthNextBlockWithinBook(2);
			Assert.AreEqual(m_books.First()[2], result);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_BeyondBook_ReturnsNull()
		{
			m_navigator.NavigateToFirstBlock();
			Block result = m_navigator.PeekNthNextBlockWithinBook(3);
			Assert.IsNull(result);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_FromSpecificBook_NEquals1()
		{
			Block result = m_navigator.PeekNthNextBlockWithinBook(1, m_books[0].Blocks[0]);
			Assert.AreEqual(m_books.First()[1], result);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_FromSpecificBook_NGreaterThan1()
		{
			Block result = m_navigator.PeekNthNextBlockWithinBook(1, m_books[0].Blocks[1]);
			Assert.AreEqual(m_books.First()[2], result);
		}

		[Test]
		public void PeekNthNextBlockWithinBook_FromSpecificBook_BeyondBook_ReturnsNull()
		{
			Block result = m_navigator.PeekNthNextBlockWithinBook(2, m_books[0].Blocks[1]);
			Assert.IsNull(result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_NEquals1()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(1);
			Assert.AreEqual(m_books[1].Blocks[1], result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_NGreaterThan1()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(2);
			Assert.AreEqual(m_books[1].Blocks[0], result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_BeyondBook_ReturnsNull()
		{
			m_navigator.CurrentBlock = m_books[1].Blocks[2];
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(3);
			Assert.IsNull(result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_FromSpecificBook_NEquals1()
		{
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(1, m_books[1].Blocks[2]);
			Assert.AreEqual(m_books[1].Blocks[1], result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_FromSpecificBook_NGreaterThan1()
		{
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(2, m_books[1].Blocks[2]);
			Assert.AreEqual(m_books[1].Blocks[0], result);
		}

		[Test]
		public void PeekNthPreviousBlockWithinBook_FromSpecificBook_BeyondBook_ReturnsNull()
		{
			Block result = m_navigator.PeekNthPreviousBlockWithinBook(3, m_books[1].Blocks[2]);
			Assert.IsNull(result);
		}

		[Test]
		public void PeekForwardWithinBook_OneBlock_WithinBook()
		{
			var secondBlock = m_books.First()[1];
			IEnumerable<Block> result = m_navigator.PeekForwardWithinBook(1);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(secondBlock, result.First());
		}

		[Test]
		public void PeekForwardWithinBook_OneBlock_StartAtBookEnd_ReturnsEmpty()
		{
			m_navigator.CurrentBlock = m_books.First().Blocks.Last();
			IEnumerable<Block> result = m_navigator.PeekForwardWithinBook(1);
			Assert.AreEqual(0, result.Count());
		}

		[Test]
		public void PeekForwardWithinBook_MultipleBlocks_WithinBook()
		{
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			IEnumerable<Block> result = m_navigator.PeekForwardWithinBook(2);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(secondBlock, result.First());
			Assert.AreEqual(thirdBlock, result.Last());
		}

		[Test]
		public void PeekForwardWithinBook_MultipleBlocks_StopsAtBookEnd()
		{
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			IEnumerable<Block> result = m_navigator.PeekForwardWithinBook(3);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(secondBlock, result.First());
			Assert.AreEqual(thirdBlock, result.Last());
		}

		[Test]
		public void PeekBackwardWithinBook_OneBlock_WithinBook()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			m_navigator.CurrentBlock = secondBlock;
			IEnumerable<Block> result = m_navigator.PeekBackwardWithinBook(1);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(firstBlock, result.First());
		}

		[Test]
		public void PeekBackwardWithinBook_OneBlock_StartAtBookBegin_ReturnsEmpty()
		{
			IEnumerable<Block> result = m_navigator.PeekBackwardWithinBook(1);
			Assert.AreEqual(0, result.Count());
		}

		[Test]
		public void PeekBackwardWithinBook_MultipleBlocks_WithinBook()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			m_navigator.CurrentBlock = thirdBlock;
			IEnumerable<Block> result = m_navigator.PeekBackwardWithinBook(2);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(firstBlock, result.First());
			Assert.AreEqual(secondBlock, result.Last());
		}

		[Test]
		public void PeekBackwardWithinBook_MultipleBlocks_StopsAtBookBegin()
		{
			var firstBlock = m_books.First()[0];
			var secondBlock = m_books.First()[1];
			var thirdBlock = m_books.First()[2];
			m_navigator.CurrentBlock = thirdBlock;
			IEnumerable<Block> result = m_navigator.PeekBackwardWithinBook(3);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(firstBlock, result.First());
			Assert.AreEqual(secondBlock, result.Last());
		}

		[Test]
		public void GetPreviousBlock_FirstReturnsNull()
		{
			var firstBook = m_books.First();
			var firstBlock = firstBook[0];
			m_navigator.CurrentBlock = firstBlock;
			Assert.IsNull(m_navigator.PreviousBlock());
		}

		[Test]
		public void CurrentBlock_StartsAtFirstBlock()
		{
			Assert.AreEqual(m_books.First()[0], m_navigator.CurrentBlock);
		}

		[Test]
		public void CurrentBlock_StartsAtFirstBook()
		{
			Assert.AreEqual(m_books.First(), m_navigator.CurrentBook);
		}

		[Test]
		public void GetNextBlock_GetPreviousBlock_BackToFirst()
		{
			m_navigator.NextBlock();
			Assert.AreEqual(m_books.First()[0], m_navigator.PreviousBlock());
		}

		[Test]
		public void GetPreviousBlock_GetNextBlock_BackToLast()
		{
			m_navigator.CurrentBlock = m_books.Last().GetScriptBlocks().Last();
			m_navigator.PreviousBlock();
			Assert.AreEqual(m_books.Last().GetScriptBlocks().Last(), m_navigator.NextBlock());
		}

		[Test]
		public void GetIndices_FirstBlock()
		{
			Assert.AreEqual(new BookBlockIndices(0, 0), m_navigator.GetIndices());
		}

		[Test]
		public void GetIndices_SecondBlock()
		{
			m_navigator.CurrentBlock = m_books.First().GetScriptBlocks()[1];
			Assert.AreEqual(new BookBlockIndices(0, 1), m_navigator.GetIndices());
		}

		[Test]
		public void GetIndices_LastBlock()
		{
			m_navigator.CurrentBlock = m_books.Last().GetScriptBlocks()[3];
			Assert.AreEqual(new BookBlockIndices(1, 3), m_navigator.GetIndices());
		}

		[Test]
		public void SetIndices_FirstBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(0, 0));
			Assert.AreEqual(m_books.First().GetScriptBlocks()[0], m_navigator.CurrentBlock);
		}

		[Test]
		public void SetIndices_SecondBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(0, 1));
			Assert.AreEqual(m_books.First().GetScriptBlocks()[1], m_navigator.CurrentBlock);
		}

		[Test]
		public void SetIndices_LastBlock()
		{
			m_navigator.SetIndices(new BookBlockIndices(1, 3));
			Assert.AreEqual(m_books.Last().GetScriptBlocks()[3], m_navigator.CurrentBlock);
		}

		[Test]
		public void GetIndicesOfSpecificBlock_FirstBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.First().GetScriptBlocks()[0]);
			Assert.AreEqual(new BookBlockIndices(0, 0), result);
		}

		[Test]
		public void GetIndicesOfSpecificBlock_SecondBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.First().GetScriptBlocks()[1]);
			Assert.AreEqual(new BookBlockIndices(0, 1), result);
		}

		[Test]
		public void GetIndicesOfSpecificBlock_LastBlock()
		{
			var result = m_navigator.GetIndicesOfSpecificBlock(m_books.Last().GetScriptBlocks()[3]);
			Assert.AreEqual(new BookBlockIndices(1, 3), result);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_BookNotIncluded_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(65, 1, 2));
			Assert.IsNull(result);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_ChapterNotFound_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 3, 7));
			Assert.IsNull(result);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_VerseNotFound_ReturnsNull()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 2, 17));
			Assert.IsNull(result);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_InFirstBook_ReturnsIndices()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("LUK"), 1, 1));
			Assert.AreEqual(0, result.BookIndex);
			Assert.AreEqual(1, result.BlockIndex);
		}

		[Test]
		public void GetIndicesOfFirstBlockAtReference_TwoBlockAtSameReference_ReturnsIndicesForFirstMatch()
		{
			var result = m_navigator.GetIndicesOfFirstBlockAtReference(new VerseRef(BCVRef.BookToNumber("ROM"), 5, 7));
			Assert.AreEqual(1, result.BookIndex);
			Assert.AreEqual(2, result.BlockIndex);
		}

	}
}
