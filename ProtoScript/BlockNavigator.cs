using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

namespace ProtoScript
{
	public class BlockNavigator
	{
		private readonly IReadOnlyList<BookScript> m_books;
		private BookScript m_currentBook;
		private int m_currentBookIndex;
		private Block m_currentBlock;
		private int m_currentBlockIndex;

		public BlockNavigator(IReadOnlyList<BookScript> books)
		{
			m_books = books;
			m_currentBook = m_books.FirstOrDefault(b => b.HasScriptBlocks);
			if (m_currentBook == null)
				throw new ArgumentException("The list of books must contain at least one block.");
			m_currentBlock = m_currentBook[0];
		}

		public BookScript CurrentBook
		{
			get { return m_currentBook; }
			set
			{
				m_currentBook = value;
				m_currentBookIndex = GetBookIndex(m_currentBook);
			}
		}

		public Block CurrentBlock
		{
			get { return m_currentBlock; }
			set
			{
				var indices = GetIndicesOfSpecificBlock(value);
				m_currentBlock = value;
				m_currentBookIndex = indices.Item1;
				m_currentBlockIndex = indices.Item2;
				m_currentBook = m_books[m_currentBookIndex];
			}
		}

		public void NavigateToFirstBlock()
		{
			SetIndices(new Tuple<int, int>(0, 0));
		}

		internal Tuple<int, int> GetIndices()
		{
			return new Tuple<int, int>(m_currentBookIndex, m_currentBlockIndex);
		}

		internal void SetIndices(Tuple<int, int> indices)
		{
			m_currentBookIndex = indices.Item1;
			m_currentBook = m_books[m_currentBookIndex];
			m_currentBlockIndex = indices.Item2;
			m_currentBlock = m_currentBook.GetScriptBlocks()[m_currentBlockIndex];
		}

		internal Tuple<int, int> GetIndicesOfSpecificBlock(Block block)
		{
			if (block == m_currentBlock)
				return GetIndices();
			// In production code, I think this will always only be called for the current book, so we try that
			// first before looking at all the rest of the books for this block.

			var indexInCurrentBook = m_currentBook.GetScriptBlocks().IndexOf(block);
			if (indexInCurrentBook >= 0)
				return new Tuple<int, int>(m_currentBookIndex, indexInCurrentBook);

			for (int iBook = 0; iBook < m_books.Count; iBook++)
			{
				if (iBook == m_currentBookIndex)
					continue;
				var book = m_books[iBook];
				var iBlock = book.GetScriptBlocks().IndexOf(block);
				if (iBlock >= 0)
					return new Tuple<int, int>(iBook, iBlock);
			}
			throw new ArgumentOutOfRangeException("block", block.ToString(), "Block not found in any book!");
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
			return m_books[m_currentBookIndex + 1];
		}

		private BookScript NextBook()
		{
			if (IsLastBook(m_currentBook))
				return null;
			return m_currentBook = m_books[++m_currentBookIndex];
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

			return m_currentBook[m_currentBlockIndex + 1];
		}

		public IEnumerable<Block> PeekForwardWithinBook(int numberOfBlocks)
		{
			var blocks = new List<Block>();
			int tempCurrentBlockIndex = m_currentBlockIndex;
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
			return PeekNthNextBlockWithinBook(n, m_currentBookIndex, m_currentBlockIndex);
		}

		public Block PeekNthNextBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == m_currentBlock)
				return PeekNthNextBlockWithinBook(n);
			Tuple<int, int> indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return PeekNthNextBlockWithinBook(n, indices.Item1, indices.Item2);
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
			int tempCurrentBlockIndex = m_currentBlockIndex;
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
			int tempCurrentBlockIndex = m_currentBlockIndex;
			while (tempCurrentBlockIndex > 0 && predicate(m_currentBook[--tempCurrentBlockIndex]))
				yield return m_currentBook[tempCurrentBlockIndex];
		}

		public IEnumerable<Block> PeekForwardWithinBookWhile(Func<Block, bool> predicate)
		{
			int tempCurrentBlockIndex = m_currentBlockIndex;
			while (!IsLastBlockInBook(m_currentBook, tempCurrentBlockIndex) && predicate(m_currentBook[++tempCurrentBlockIndex]))
				yield return m_currentBook[tempCurrentBlockIndex];
		}

		public Block PeekNthPreviousBlockWithinBook(int n)
		{
			return PeekNthPreviousBlockWithinBook(n, m_currentBookIndex, m_currentBlockIndex);
		}

		public Block PeekNthPreviousBlockWithinBook(int n, Block baseLineBlock)
		{
			if (baseLineBlock == m_currentBlock)
				return PeekNthPreviousBlockWithinBook(n);
			Tuple<int, int> indices = GetIndicesOfSpecificBlock(baseLineBlock);
			return PeekNthPreviousBlockWithinBook(n, indices.Item1, indices.Item2);
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
				m_currentBlockIndex = 0;
				return nextBook[m_currentBlockIndex];
			}

			return m_currentBlock = m_currentBook[++m_currentBlockIndex];
		}

		private BookScript PeekPreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			return m_books[m_currentBookIndex - 1];
		}

		private BookScript PreviousBook()
		{
			if (IsFirstBook(m_currentBook))
				return null;
			return m_currentBook = m_books[--m_currentBookIndex];
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

			return m_currentBook[m_currentBlockIndex - 1];
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
				m_currentBlockIndex = m_currentBook.GetScriptBlocks().Count - 1;
				return previousBook[m_currentBlockIndex];
			}

			return m_currentBlock = m_currentBook[--m_currentBlockIndex];
		}
	}

	public static class IEnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> list, T item)
		{
			for (int i = 0; i < list.Count(); i++)
			{
				if (item.Equals(list.ElementAt(i)))
					return i;
			}
			return -1;
		}
	}
}
