using System;
using System.Collections.Generic;
using System.Linq;

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
			m_currentBook = m_books.First(b => b.HasScriptBlocks);
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
				int i = 0;
				foreach (BookScript book in m_books)
				{
					if (book == m_currentBook)
					{
						m_currentBookIndex = i;
						break;
					}
					i++;
				}
			}
		}

		public Block CurrentBlock
		{
			get { return m_currentBlock; }
			set
			{
				m_currentBlock = value;
				if (!CurrentBook.GetScriptBlocks().Contains(m_currentBlock))
					CurrentBook = GetBookScriptContainingBlock(m_currentBlock);
				int i = 0;
				foreach (Block block in CurrentBook.GetScriptBlocks())
				{
					if (block == m_currentBlock)
					{
						m_currentBlockIndex = i;
						break;
					}
					i++;
				}
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

		public bool IsLastBlock(Block block)
		{
			BookScript book = GetBookScriptContainingBlock(block);
			if (book == null)
				return false;
			return IsLastBlockInBook(book, block) && IsLastBook(book);
		}

		public bool IsLastBlockInBook(BookScript book, Block block)
		{
			return block == book.GetScriptBlocks().LastOrDefault();
		}

		private bool IsLastBlockInBook(BookScript book, int blockIndex)
		{
			return blockIndex == book.Blocks.Count - 1;
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
			if (IsLastBlock(m_currentBlock))
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

		public Block NextBlock()
		{
			if (IsLastBlock(m_currentBlock))
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
}
