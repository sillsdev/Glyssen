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

		public static Project Load(string projectFilePath)
		{
			var project = new Project(XmlSerializationHelper.DeserializeFromFile<DblMetadata>(projectFilePath));
			var projectDir = Path.GetDirectoryName(projectFilePath);
			Debug.Assert(projectDir != null);
			foreach (var file in Directory.GetFiles(projectDir, "???.xml"))
			{
				project.m_books[Path.GetFileNameWithoutExtension(file)] =
					XmlSerializationHelper.DeserializeFromFile<BookScript>(file);
			}
			return project;
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
	}
}
