using System;
using System.IO;

namespace Glyssen.Shared
{
	public class BookReader : IDisposable
	{
		public string BookId { get; }
		private TextReader m_readerImpl;

		public BookReader(string bookId, TextReader readerImpl)
		{
			BookId = bookId;
			m_readerImpl = readerImpl;
		}

		public static implicit operator TextReader(BookReader reader)
		{
			return reader.m_readerImpl;
		}

		public void Dispose()
		{
			if (m_readerImpl != null)
			{
				m_readerImpl.Dispose();
				m_readerImpl = null;
			}
		}
	}
}
