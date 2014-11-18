using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private readonly DblMetadata m_metadata;
		private readonly Dictionary<string, List<Block>> m_books = new Dictionary<string, List<Block>>();

		public Project(DblMetadata metadata)
		{
			m_metadata = metadata;
		}

		public Project Load(string metaDataFilePath)
		{
			var project = new Project(XmlSerializationHelper.DeserializeFromFile<DblMetadata>(metaDataFilePath));
			var projectDir = Path.GetDirectoryName(metaDataFilePath);
			foreach (var file in Directory.GetFiles(projectDir, "???.xml"))
			{
				project.m_books[Path.GetFileNameWithoutExtension(file)] =
					XmlSerializationHelper.DeserializeFromFile<List<Block>>(file);
			}
			return project;
		}

		public void AddBook(string bookId, IEnumerable<Block> blocks)
		{
			m_books[bookId] = blocks.ToList();
		}

		public IEnumerable<Block> GetBlocksForBook(string bookId)
		{
			List<Block> blocks;
			if (m_books.TryGetValue(bookId, out blocks))
			{
				return blocks;
			}
			return null;
		}

		public void Save(string path)
		{
			path = Path.Combine(path, m_metadata.language.ToString(), m_metadata.id);
			Directory.CreateDirectory(path);
			Exception error;
			var metadataFilename = Path.Combine(path, "metadata.xml");
			XmlSerializationHelper.SerializeToFile(metadataFilename, m_metadata, out error);
			if (error != null)
			{
				MessageBox.Show(error.Message);
				return;
			}
			Settings.Default.CurrentProject = metadataFilename;
			foreach (var book in m_books)
			{
				var filePath = Path.ChangeExtension(Path.Combine(path, book.Key), "xml");
				XmlSerializationHelper.SerializeToFile(filePath, book.Value, out error);
				if (error != null)
					MessageBox.Show(error.Message);
			}
		}
	}
}
