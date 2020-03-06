using System;
using System.IO;

namespace Glyssen.Shared
{
	public class ResourceReader<IdType> : IDisposable
	{
		public IdType Id { get; }
		private TextReader m_readerImpl;

		public ResourceReader(IdType id, TextReader readerImpl)
		{
			Id = id;
			m_readerImpl = readerImpl;
		}

		public static implicit operator TextReader(ResourceReader<IdType> reader)
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
