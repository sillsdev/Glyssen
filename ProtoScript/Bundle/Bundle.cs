using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using ProtoScript.Properties;
using ProtoScript.Utilities;

namespace ProtoScript.Bundle
{
	public class Bundle
	{
		private readonly DblMetadata m_dblMetadata;
		private readonly IDictionary<int, Canon> m_canons = new Dictionary<int, Canon>();

		public Bundle(string pathToZippedBundle)
		{
			string pathToUnzippedDirectory = Zip.ExtractToTempDirectory(pathToZippedBundle);

			string metadataPath = Path.Combine(pathToUnzippedDirectory, "metadata.xml");

			var xs = new XmlSerializer(typeof(DblMetadata));
			m_dblMetadata = (DblMetadata)xs.Deserialize(new FileStream(metadataPath, FileMode.Open));
			m_dblMetadata.OriginalPathOfDblFile = pathToZippedBundle;
			m_dblMetadata.PgUsxParserVersion = Settings.Default.PgUsxParserVersion;

			ExtractCanons(pathToUnzippedDirectory);
		}

		public DblMetadata Metadata
		{
			get { return m_dblMetadata; }
		}

		public string Id
		{
			get { return m_dblMetadata.id; }
		}

		public string Language
		{
			get { return m_dblMetadata.language.ToString(); }
		}

		public IDictionary<int, Canon> Canons
		{
			get { return m_canons; }
		}

		public bool TryGetCanon(int bookId, out Canon canon)
		{
			return m_canons.TryGetValue(bookId, out canon);
		}

		//TODO This method either needs to be greatly improved or replaced
		private void ExtractCanons(string pathToUnzippedDirectory)
		{
			foreach (string dir in Directory.GetDirectories(pathToUnzippedDirectory, "USX_*"))
			{
				int canonId;
				if (Int32.TryParse(dir[dir.Length - 1].ToString(CultureInfo.InvariantCulture), out canonId))
				{
					var canon = new Canon(canonId);
					canon.ExtractBooks(dir);
					m_canons.Add(canonId, canon);
				}
			}
		}
	}
}
