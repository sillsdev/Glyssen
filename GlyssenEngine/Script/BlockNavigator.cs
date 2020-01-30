using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.Extensions;
using SIL.Scripture;

namespace GlyssenEngine.Script
{
	public class BlockNavigator : IBlockAccessor
	{
		private readonly IReadOnlyList<BookScript> m_books;
		private BookScript m_currentBook;
		private BookBlockIndices m_currentIndices = new BookBlockIndices();

		public BlockNavigator(IReadOnlyList<BookScript> books)
		{
			m_books = books;
			m_currentBook = m_books.FirstOrDefault(b => b.HasScriptBlocks);
			if (m_currentBook == null)
				throw new ArgumentException("The list of books must contain at least one block.");
			m_currentIndices = new BookBlockIndices(0, 0);
		}

		public BookScript CurrentBook
		{
			get => m_currentBook;
			set
			{
				m_currentBook = value;
				m_currentIndices.BookIndex = GetBookIndex(m_currentBook);
			}
		}

		public Block CurrentBlock
		{
			get => m_currentBook[m_currentIndices.BlockIndex];
			set
			{
				m_currentIndices = GetIndicesOfSpecificBlock(value);
				m_currentBook = m_books[m_currentIndices.BookIndex];
			}
		}

		public Block CurrentEndBlock => m_currentIndices.IsMultiBlock ?
			m_books[m_currentIndices.BookIndex].GetScriptBlocks()[m_currentIndices.EffectiveFinalBlockIndex] :
			CurrentBlock;

		public void GoToFirstBlock()
		{
			SetIndices(new BookBlockIndices(0, 0));
		}

		public BookBlockIndices GetIndices()
		{
			return new BookBlockIndices(m_currentIndices);
		}

		internal void SetIndices(BookBlockIndices indices)
		{
			m_currentIndices = new BookBlockIndices(indices);
			m_currentBook = m_books[m_currentIndices.BookIndex];
		}

		public BookBlockIndices GetIndicesOfSpecificBlock(Block block)
		{
			if (block == CurrentBlock)
				return GetIndices();
			// In production code, I think this will always only be called for the current book, so we try that
			// first before looking at all the rest of the books for this block.

			var indexInCurrentBook = m_currentBook.GetScriptBlocks().IndexOf(block);
			if (indexInCurrentBook >= 0)
				return new BookBlockIndices(m_currentIndices.BookIndex, indexInCurrentBook);

			for (int iBook = 0; iBook < m_books.Count; iBook++)
			{
				if (iBook == m_currentIndices.BookIndex)
					continue;
				var book = m_books[iBook];
				var iBlock = book.GetScriptBlocks().IndexOf(block);
				if (iBlock >= 0)
					return new BookBlockIndices(iBook, iBlock);
			}
			throw new ArgumentOutOfRangeException(nameof(block), block.ToString(), "Block not found in any book!");
		}

		public BookBlockIndices GetIndicesOfFirstBlockAtReference(VerseRef verseRef, bool allowMidQuoteBlock = false)
		{
			if (!verseRef.Valid)
				return null;
			var bookId = verseRef.Book;
			int bookIndex = -1;
			BookScript book = null;
			for (int i = 0; i < m_books.Count; i++)
			{
				book = m_books[i];
				if (book.BookId == bookId)
				{
					bookIndex = i;
					break;
				}
			}

			if (bookIndex == -1 || book == null)
				return null;

			var block = book.Blocks.FirstOrDefault(
				a => a.ChapterNumber == verseRef.ChapterNum && a.InitialStartVerseNumber <= verseRef.VerseNum && a.LastVerseNum >= verseRef.VerseNum);
			if (block == null)
				return null;

			int blockIndex = book.Blocks.IndexOf(block);
			if (!allowMidQuoteBlock)
			{
				if (block.IsContinuationOfPreviousBlockQuote)
					blockIndex = book.Blocks.FindLastIndex(blockIndex, b => b.MultiBlockQuote == MultiBlockQuote.Start);
			}

			return blockIndex == -1 ? null : new BookBlockIndices(bookIndex, blockIndex);
		}

		public BookScript GetBookScriptContainingBlock(Block block)
		{
			return m_books.FirstOrDefault(script => script.GetScriptBlocks() != null && script.GetScriptBlocks().Contains(block));
		}

		public bool IsLastBook(BookScript book)
		{
			if (!m_books.Any())
				return false;
			return book == m_books.Last();
		}

		public bool IsLastBlock()
		{
			return IsLastBlockInBook(m_currentBook, CurrentEndBlock) && IsLastBook(m_currentBook);
		}

		public bool IsLastBlockInBook(BookScript book, Block block)
		{
			return block == book.GetScriptBlocks().LastOrDefault();
		}

		private int GetBookIndex(BookScript bookToFind)
		{
			int i = 0;
			foreach (var book in m_books)
			{
				if (book == bookToFind)
					return i;
				i++;
			}
			throw new ArgumentException("Book is not part of book list");
		}

		private bool IsLastBlockInBook(BookScript book, int blockIndex)
		{
			return blockIndex == book.GetScriptBlocks().Count - 1;
		}

		public bool IsFirstBook(BookScript book)
		{
			if (!m_books.Any())
				return false;
			return book == m_books.First();
		}

		public bool IsFirstBlock(Block block)
		{
			var book = GetBookScriptContainingBlock(block);
			if (book == null)
				return false;
			return IsFirstBlockInBook(book, block) && IsFirstBook(book);
		}

		public bool IsFirstBlockInBook(BookScript book, Block block)
		{
			return block == book.GetScriptBlocks().FirstOrDefault();
		}

		private BookScript GetNextBook()
		{
			if (IsLastBook(m_currentBook))
				return null;
			return m_books[m_currentIndices.BookIndex + 1];
		}

		private BookScript GoToNextBook()
		{
			if (IsLastBook(m_currentBook))
				return null;
			m_currentIndices = new BookBlockIndices(m_currentIndices.BookIndex + 1, 0);
			return m_currentBook = m_books[m_currentIndices.BookIndex];
		}

		public Block GetNextBlock()
		{
			if (IsLastBlock())
				return null;
			if (IsLastBlockInBook(m_currentBook, CurrentEndBlock))
			{
				var nextBook = GetNextBook();
				if (!nextBook.HasScriptBlocks)
					return null;
				return nextBook[0];
			}

			return m_currentBook[m_currentIndices.BlockIndex + (int)m_currentIndices.BlockCount];
		}

		/// <summary>
		/// Gets a collection representing the requested number of blocks following the current
		/// block (not including the current block). If the current indices represents a "MultiBlock"
		/// position, the start block -- not the final block -- is used as a starting reference point.
		/// </summary>
		public IEnumerable<Block> GetNextNBlocksWithinBook(int numberOfBlocks)
		{
			var blocks = new List<Block>();
			int tempCurrentBlockIndex = m_currentIndices.BlockIndex;
			for (int i = 0; i < numberOfBlocks; i++)
			{
				if (IsLastBlockInBook(m_currentBook, tempCurrentBlockIndex))
					break;
				blocks.Add(m_currentBook[++tempCurrentBlockIndex]);
			}
			return blocks;
		}

		/// <summary>
		/// Gets the block "n" blocks beyond the current block. If the current indices represents a "MultiBlock"
		/// position, the start block -- not the final block -- is used as a starting reference point.
		/// </summary>
		public Block GetNthNextBlockWithinBook(int n)
		{
			return GetNthNextBlockWithinBook(n, m_currentIndices.BookIndex, m_currentIndices.BlockIndex);
		}

		/// <summary>
		/// Gets the block "n" blocks beyond the specified block.
		/// </summary>
		public Block GetNthNextBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == CurrentBlock)
				return GetNthNextBlockWithinBook(n);
			BookBlockIndices indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return GetNthNextBlockWithinBook(n, indices.BookIndex, indices.BlockIndex);
		}

		private Block GetNthNextBlockWithinBook(int n, int bookIndex, int blockIndex)
		{
			var book = m_books[bookIndex];
			if (book.GetScriptBlocks().Count < blockIndex + n + 1)
				return null;
			return book[blockIndex + n];
		}

		public IEnumerable<Block> GetPreviousNBlocksWithinBook(int numberOfBlocks)
		{
			var blocks = new List<Block>();
			int tempCurrentBlockIndex = m_currentIndices.BlockIndex;
			for (int i = 0; i < numberOfBlocks; i++)
			{
				if (tempCurrentBlockIndex == 0)
					break;
				blocks.Add(m_currentBook[--tempCurrentBlockIndex]);
			}
			blocks.Reverse();
			return blocks;
		}

		public IEnumerable<Block> GetSurroundingBlocksWithinBookWhile(Func<Block, bool> predicate, bool forwardOnly, Block startBlock = null)
		{
			int tempCurrentBlockIndex = m_currentIndices.BlockIndex;
			if (startBlock != null && startBlock != CurrentBlock)
			{
				var currentBookBlocks = m_currentBook.GetScriptBlocks();
				// By far the most common case will be a start block within the current multi-block index, so we'll try that first
				do
				{
					if (++tempCurrentBlockIndex > m_currentIndices.EffectiveFinalBlockIndex)
					{
						tempCurrentBlockIndex = currentBookBlocks.IndexOf(startBlock);
						break;
					}
				} while (currentBookBlocks[tempCurrentBlockIndex] != startBlock);
			}

			if (!forwardOnly)
			{
				var tempTempIndex = tempCurrentBlockIndex;
				while (tempTempIndex > 0 && predicate(m_currentBook[--tempTempIndex]))
					yield return m_currentBook[tempTempIndex];
			}
			while (!IsLastBlockInBook(m_currentBook, tempCurrentBlockIndex) && predicate(m_currentBook[++tempCurrentBlockIndex]))
				yield return m_currentBook[tempCurrentBlockIndex];
		}

		public Block GetNthPreviousBlockWithinBook(int n)
		{
			return GetNthPreviousBlockWithinBook(n, m_currentIndices.BookIndex, m_currentIndices.BlockIndex);
		}

		public Block GetNthPreviousBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == CurrentBlock)
				return GetNthPreviousBlockWithinBook(n);
			BookBlockIndices indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return GetNthPreviousBlockWithinBook(n, indices.BookIndex, indices.BlockIndex);
		}

		private Block GetNthPreviousBlockWithinBook(int n, int bookIndex, int blockIndex)
		{
			if (blockIndex - n < 0)
				return null;
			return m_books[bookIndex][blockIndex - n];
		}

		public Block GoToNextBlock()
		{
			if (IsLastBlock())
				return null;
			if (IsLastBlockInBook(m_currentBook, CurrentEndBlock))
			{
				var nextBook = GoToNextBook();
				return !nextBook.HasScriptBlocks ? null : nextBook[m_currentIndices.BlockIndex];
			}

			m_currentIndices.AdvanceToNextBlock();
			return CurrentBlock;
		}

		private BookScript GetPreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			return m_books[m_currentIndices.BookIndex - 1];
		}

		private BookScript GoToPreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			var newBookIndex = m_currentIndices.BookIndex - 1;
			m_currentIndices = new BookBlockIndices(newBookIndex, m_books[newBookIndex].GetScriptBlocks().Count - 1);
			return m_currentBook = m_books[newBookIndex];
		}

		public Block GetPreviousBlock()
		{
			if (IsFirstBlock(CurrentBlock))
				return null;
			if (IsFirstBlockInBook(m_currentBook, CurrentBlock))
			{
				var previousBook = GetPreviousBook();
				if (!previousBook.HasScriptBlocks)
					return null;
				return previousBook[previousBook.GetScriptBlocks().Count - 1];
			}

			return m_currentBook[m_currentIndices.BlockIndex - 1];
		}

		//public Block GoToPreviousBlock()
		//{
		//	if (IsFirstBlock(CurrentBlock))
		//		return null;
		//	if (IsFirstBlockInBook(m_currentBook, CurrentBlock))
		//	{
		//		var previousBook = GoToPreviousBook();
		//		return !previousBook.HasScriptBlocks ? null : previousBook[m_currentIndices.BlockIndex];
		//	}

		//	m_currentIndices.BlockIndex--;
		//	m_currentIndices.MultiBlockCount = 0;
		//	return CurrentBlock;
		//}

		#region Methods for dealing with multi-block groups/quotes
		public IEnumerable<Block> GetAllBlocksWhichContinueTheQuoteStartedByBlock(Block firstBlock, int adjustmentToBlockCount = 0)
		{
			switch (firstBlock.MultiBlockQuote)
			{
				case MultiBlockQuote.Start:
					yield return firstBlock;
					foreach (var i in GetIndicesOfQuoteContinuationBlocks(firstBlock, adjustmentToBlockCount))
						yield return CurrentBook[i];
					break;
				case MultiBlockQuote.Continuation:
					// These should all be brought in through a Start block, so don't do anything with them here.
					break;
				default:
					// Not part of a multi-block quote. Just return the base-line block
					yield return firstBlock;
					break;
			}
		}

		public IEnumerable<int> GetIndicesOfQuoteContinuationBlocks(Block startQuoteBlock, int adjustmentToBlockCount = 0)
		{
			// Note this method assumes the startQuoteBlock is in the navigator's current book.
			Debug.Assert(startQuoteBlock.MultiBlockQuote == MultiBlockQuote.Start);

			for (int j = GetIndicesOfSpecificBlock(startQuoteBlock).BlockIndex + 1; j < CurrentBook.GetScriptBlocks().Count + adjustmentToBlockCount; j++)
			{
				Block block = CurrentBook[j];
				if (block == null || !block.IsContinuationOfPreviousBlockQuote)
					break;
				yield return j;
			}
		}
		#endregion

		public void ExtendCurrentBlockGroup(uint additionalBlocks)
		{
			m_currentIndices.ExtendToIncludeMoreBlocks(additionalBlocks);
		}
	}
}
