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

		public bool TryGetBook(string bookId, out UsxDocument book)
		{
			return m_books.TryGetValue(bookId, out book);
		}

		//TODO This method either needs to be greatly improved or replaced
		internal void ExtractBooks(string pathToCanon)
		{
			foreach (string filePath in Directory.GetFiles(pathToCanon, "*.usx"))
			{
				var fi = new FileInfo(filePath);
				string bookId = Path.GetFileNameWithoutExtension(fi.Name);
				if (bookId.Length == 3)
					m_books.Add(bookId, new UsxDocument(filePath));
			}
		}
	}
}
