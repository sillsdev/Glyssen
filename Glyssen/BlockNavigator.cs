using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared.Bundle;
using SIL.Extensions;
using SIL.Scripture;

namespace Glyssen
{
	public class BlockNavigator
	{
		private readonly IReadOnlyList<BookScript> m_books;
		private BookScript m_currentBook;
		private BookBlockIndices m_currentIndices = new BookBlockIndices();
		private Block m_currentBlock;

		public BlockNavigator(IReadOnlyList<BookScript> books)
		{
			m_books = books;
			m_currentBook = m_books.FirstOrDefault(b => b.HasScriptBlocks);
			if (m_currentBook == null)
				throw new ArgumentException("The list of books must contain at least one block.");
			m_currentBlock = m_currentBook[0];
			m_currentIndices = new BookBlockIndices(0, 0);
		}

		public BookScript CurrentBook
		{
			get { return m_currentBook; }
			set
			{
				m_currentBook = value;
				m_currentIndices.BookIndex = GetBookIndex(m_currentBook);
			}
		}

		public Block CurrentBlock
		{
			get { return m_currentBlock; }
			set
			{
				m_currentIndices = GetIndicesOfSpecificBlock(value);
				m_currentBlock = value;
				m_currentBook = m_books[m_currentIndices.BookIndex];
			}
		}

		public Block CurrentEndBlock => m_currentIndices.IsMultiBlock ?
			m_books[m_currentIndices.BookIndex].GetScriptBlocks()[m_currentIndices.EffectiveFinalBlockIndex] :
			CurrentBlock;

		public void NavigateToFirstBlock()
		{
			SetIndices(new BookBlockIndices(0, 0));
		}

		internal BookBlockIndices GetIndices()
		{
			return new BookBlockIndices(m_currentIndices);
		}

		internal void SetIndices(BookBlockIndices indices)
		{
			m_currentIndices = new BookBlockIndices(indices);
			m_currentBook = m_books[m_currentIndices.BookIndex];
			m_currentBlock = m_currentBook.GetScriptBlocks()[m_currentIndices.BlockIndex];
		}

		internal BookBlockIndices GetIndicesOfSpecificBlock(Block block)
		{
			if (block == m_currentBlock)
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
			return IsLastBlockInBook(m_currentBook, m_currentBlock) && IsLastBook(m_currentBook);
		}

		public bool IsLastBlockInBook(BookScript book, Block block)
		{
			return block == book.GetScriptBlocks().LastOrDefault();
		}

		private int GetBookIndex(BookScript bookToFind)
		{
			int i = 0;
			foreach (BookScript book in m_books)
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
			BookScript book = GetBookScriptContainingBlock(block);
			if (book == null)
				return false;
			return IsFirstBlockInBook(book, block) && IsFirstBook(book);
		}

		public bool IsFirstBlockInBook(BookScript book, Block block)
		{
			return block == book.GetScriptBlocks().FirstOrDefault();
		}

		private BookScript PeekNextBook()
		{
			if (IsLastBook(m_currentBook))
				return null;
			return m_books[m_currentIndices.BookIndex + 1];
		}

		private BookScript NextBook()
		{
			if (IsLastBook(m_currentBook))
				return null;
			return m_currentBook = m_books[++m_currentIndices.BookIndex];
		}

		public Block PeekNextBlock()
		{
			if (IsLastBlock())
				return null;
			if (IsLastBlockInBook(m_currentBook, m_currentBlock))
			{
				BookScript nextBook = PeekNextBook();
				if (!nextBook.HasScriptBlocks)
					return null;
				return nextBook[0];
			}

			return m_currentBook[m_currentIndices.BlockIndex + 1];
		}

		public IEnumerable<Block> PeekForwardWithinBook(int numberOfBlocks)
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

		public Block PeekNthNextBlockWithinBook(int n)
		{
			return PeekNthNextBlockWithinBook(n, m_currentIndices.BookIndex, m_currentIndices.BlockIndex);
		}

		public Block PeekNthNextBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == m_currentBlock)
				return PeekNthNextBlockWithinBook(n);
			BookBlockIndices indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return PeekNthNextBlockWithinBook(n, indices.BookIndex, indices.BlockIndex);
		}

		private Block PeekNthNextBlockWithinBook(int n, int bookIndex, int blockIndex)
		{
			BookScript book = m_books[bookIndex];
			if (book.GetScriptBlocks().Count < blockIndex + n + 1)
				return null;
			return book[blockIndex + n];
		}

		public IEnumerable<Block> PeekBackwardWithinBook(int numberOfBlocks)
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

		public IEnumerable<Block> PeekBackwardWithinBookWhile(Func<Block, bool> predicate)
		{
			int tempCurrentBlockIndex = m_currentIndices.BlockIndex;
			while (tempCurrentBlockIndex > 0 && predicate(m_currentBook[--tempCurrentBlockIndex]))
				yield return m_currentBook[tempCurrentBlockIndex];
		}

		public IEnumerable<Block> PeekForwardWithinBookWhile(Func<Block, bool> predicate)
		{
			int tempCurrentBlockIndex = m_currentIndices.BlockIndex;
			while (!IsLastBlockInBook(m_currentBook, tempCurrentBlockIndex) && predicate(m_currentBook[++tempCurrentBlockIndex]))
				yield return m_currentBook[tempCurrentBlockIndex];
		}

		public Block PeekNthPreviousBlockWithinBook(int n)
		{
			return PeekNthPreviousBlockWithinBook(n, m_currentIndices.BookIndex, m_currentIndices.BlockIndex);
		}

		public Block PeekNthPreviousBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == m_currentBlock)
				return PeekNthPreviousBlockWithinBook(n);
			BookBlockIndices indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return PeekNthPreviousBlockWithinBook(n, indices.BookIndex, indices.BlockIndex);
		}

		private Block PeekNthPreviousBlockWithinBook(int n, int bookIndex, int blockIndex)
		{
			if (blockIndex - n < 0)
				return null;
			return m_books[bookIndex][blockIndex - n];
		}

		public Block NextBlock()
		{
			if (IsLastBlock())
				return null;
			if (IsLastBlockInBook(m_currentBook, m_currentBlock))
			{
				BookScript nextBook = NextBook();
				if (!nextBook.HasScriptBlocks)
					return null;
				m_currentIndices.BlockIndex = 0;
				return nextBook[m_currentIndices.BlockIndex];
			}

			return m_currentBlock = m_currentBook[++m_currentIndices.BlockIndex];
		}

		private BookScript PeekPreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			return m_books[m_currentIndices.BookIndex - 1];
		}

		private BookScript PreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			return m_currentBook = m_books[--m_currentIndices.BookIndex];
		}

		public Block PeekPreviousBlock()
		{
			if (IsFirstBlock(m_currentBlock))
				return null;
			if (IsFirstBlockInBook(m_currentBook, m_currentBlock))
			{
				BookScript previousBook = PeekPreviousBook();
				if (!previousBook.HasScriptBlocks)
					return null;
				return previousBook[previousBook.GetScriptBlocks().Count - 1];
			}

			return m_currentBook[m_currentIndices.BlockIndex - 1];
		}

		public Block PreviousBlock()
		{
			if (IsFirstBlock(m_currentBlock))
				return null;
			if (IsFirstBlockInBook(m_currentBook, m_currentBlock))
			{
				BookScript previousBook = PreviousBook();
				if (!previousBook.HasScriptBlocks)
					return null;
				m_currentIndices.BlockIndex = m_currentBook.GetScriptBlocks().Count - 1;
				return previousBook[m_currentIndices.BlockIndex];
			}

			return m_currentBlock = m_currentBook[--m_currentIndices.BlockIndex];
		}

		public void ExtendCurrentBlockGroup(uint additionalBlocks)
		{
			m_currentIndices.MultiBlockCount += additionalBlocks;
		}
	}
}
