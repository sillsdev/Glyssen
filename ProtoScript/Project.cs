using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Palaso.Xml;
using ProtoScript.Bundle;
using ProtoScript.Properties;

namespace ProtoScript
{
	internal class Project
	{
		public const string kProjectFileExtension = ".pgproj";
		private readonly DblMetadata m_metadata;
		private readonly Dictionary<string, BookScript> m_books = new Dictionary<string, BookScript>();

		public Project(DblMetadata metadata)
		{
			m_metadata = metadata;
		}

		public string Id
		{
			get { return m_metadata.id; }
		}

		public string Language
		{
			get { return m_metadata.language.ToString(); }
		}

		public QuoteSystem QuoteSystem
		{
			get { return m_metadata.QuoteSystem; }
			set { m_metadata.QuoteSystem = value; }
		}

		public static Project Load(string projectFilePath)
		{
			Project project;
			var metadata = XmlSerializationHelper.DeserializeFromFile<DblMetadata>(projectFilePath);
			if (metadata.PgUsxParserVersion != Settings.Default.PgUsxParserVersion &&
				File.Exists(metadata.OriginalPathOfDblFile))
			{
				// ENHANCE: For now, just create a new bundle and re-parse
				var bundle = new Bundle.Bundle(metadata.OriginalPathOfDblFile);
				// See if we already have a project for this bundle and open it instead.
				project = new Project(bundle.Metadata);
				project.PopulateAndParseBooks(bundle);
				return project;
			}
			project = new Project(metadata);
			var projectDir = Path.GetDirectoryName(projectFilePath);
			Debug.Assert(projectDir != null);
			foreach (var file in Directory.GetFiles(projectDir, "???.xml"))
			{
				project.m_books[Path.GetFileNameWithoutExtension(file)] =
					XmlSerializationHelper.DeserializeFromFile<BookScript>(file);
			}
			return project;
		}

		public void PopulateAndParseBooks(Bundle.Bundle bundle)
		{
			Canon canon;
			if (bundle.TryGetCanon(1, out canon))
			{
				UsxDocument book;
				if (canon.TryGetBook("MRK", out book))
				{
					AddBook("MRK", new QuoteParser(new UsxParser(book.GetChaptersAndParas()).Parse()).Parse());
				}
			}
		}

		public void AddBook(string bookId, IEnumerable<Block> blocks)
		{
			m_books[bookId] = new BookScript(bookId, blocks);
		}

		public IEnumerable<Block> GetBlocksForBook(string bookId)
		{
			BookScript bookScript;
			if (m_books.TryGetValue(bookId, out bookScript))
			{
				return bookScript.Blocks;
			}
			return null;
		}

		public static string GetProjectFilePath(string basePath, string langId, string bundleId)
		{
			return Path.Combine(basePath, langId, bundleId, langId + kProjectFileExtension);
		}

		public void Save(string path)
		{
			var projectPath = GetProjectFilePath(path, m_metadata.language.ToString(), m_metadata.id);
			Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
			Exception error;
			XmlSerializationHelper.SerializeToFile(projectPath, m_metadata, out error);
			if (error != null)
			{
				MessageBox.Show(error.Message);
				return;
			}
			Settings.Default.CurrentProject = projectPath;
			var projectFolder = Path.GetDirectoryName(projectPath);
			foreach (var book in m_books)
			{
				var filePath = Path.ChangeExtension(Path.Combine(projectFolder, book.Key), "xml");
				XmlSerializationHelper.SerializeToFile(filePath, book.Value, out error);
				if (error != null)
					MessageBox.Show(error.Message);
			}
		}

		public void ExportTabDelimited(string fileName)
		{
			int blockNumber = 1;
			using (StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8))
			{
				foreach (var book in m_books.Values)
				{
					foreach (var block in book.Blocks)
					{
						stream.WriteLine((blockNumber++) + "\t" + block.GetAsTabDelimited(book.BookId));
					}
				}
			}
		}
	}
}
