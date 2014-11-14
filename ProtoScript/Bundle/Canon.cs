using System;
using System.Collections.Generic;
using System.IO;

namespace ProtoScript.Bundle
{
	public class Canon
	{
		public int Id { get; private set; }
		private IDictionary<string, UsxDocument> m_books = new Dictionary<string, UsxDocument>();

		public Canon(int id)
		{
			Id = id;
		}

		public IDictionary<string, UsxDocument> Books
		{
			get { return m_books; }
			set { m_books = value; }
		}

		//TODO This method either needs to be greatly improved or replaced
		internal void ExtractBooks(string pathToCanon)
		{
			foreach (string filePath in Directory.GetFiles(pathToCanon, "*.usx"))
			{
				var fi = new FileInfo(filePath);
				string bookId = fi.Name.Substring(0, fi.Name.LastIndexOf(".", StringComparison.InvariantCulture));
				m_books.Add(bookId, new UsxDocument(filePath));
			}
		}
	}
}
