using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using L10NSharp;
using Palaso.Xml;
using ProtoScript.Properties;
using ProtoScript.Utilities;

namespace ProtoScript.Bundle
{
	public class Bundle
	{
		private readonly DblMetadata m_dblMetadata;
		private readonly UsxStylesheet m_stylesheet;
		private readonly IDictionary<int, Canon> m_canons = new Dictionary<int, Canon>();

		public Bundle(string pathToZippedBundle)
		{
			string pathToUnzippedDirectory;
			try
			{
				pathToUnzippedDirectory = Zip.ExtractToTempDirectory(pathToZippedBundle);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(LocalizationManager.GetString("File.UnableToExtractBundle",
						"Unable to read contents of Text Release Bundle:") +
					Environment.NewLine + pathToZippedBundle, ex);
			}

			m_stylesheet = LoadStylesheet(pathToZippedBundle, pathToUnzippedDirectory);
			m_dblMetadata = LoadMetadata(pathToZippedBundle, pathToUnzippedDirectory);

			ExtractCanons(pathToUnzippedDirectory);
		}

		private DblMetadata LoadMetadata(string pathToZippedBundle, string pathToUnzippedDirectory)
		{
			const string filename = "metadata.xml";
			string metadataPath = Path.Combine(pathToUnzippedDirectory, filename);

			if (!File.Exists(metadataPath))
			{
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("File.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + pathToZippedBundle);
			}

			Exception exception;
			var dblMetadata = DblMetadata.Load(metadataPath, out exception);
			if (exception != null)
			{
				DblMetadataBase metadataBase;
				Exception metadataBaseDeserializationError;
				metadataBase = XmlSerializationHelper.DeserializeFromFile<DblMetadataBase>(metadataPath,
					out metadataBaseDeserializationError);
				if (metadataBaseDeserializationError != null)
				{
					throw new ApplicationException(
						LocalizationManager.GetString("File.MetadataInvalid",
							"Unable to read metadata. File is not a valid Text Release Bundle:") +
						Environment.NewLine + pathToZippedBundle, metadataBaseDeserializationError);
				}

				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("File.MetadataInvalidVersion",
						"Unable to read metadata. Type: {0}. Version: {1}. {2} does not recognize this file as a valid Text Release Bundle:"),
						metadataBase.type, metadataBase.typeVersion, Program.kProduct) +
					Environment.NewLine + pathToZippedBundle);
			}

			if (!dblMetadata.IsTextReleaseBundle)
			{
				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("File.NotTextReleaseBundle",
						"This metadata in this bundle indicates that it is of type \"{0}\". Only Text Release Bundles are currently supported."),
						dblMetadata.type));
			}

			dblMetadata.OriginalPathOfDblFile = pathToZippedBundle;
			dblMetadata.PgUsxParserVersion = Settings.Default.PgUsxParserVersion;
			dblMetadata.ControlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;

			dblMetadata.FontFamily = Stylesheet.FontFamily;
			dblMetadata.FontSizeInPoints = Stylesheet.FontSizeInPoints;

			return dblMetadata;
		}

		private UsxStylesheet LoadStylesheet(string pathToZippedBundle, string pathToUnzippedDirectory)
		{
			const string filename = "styles.xml";
			string stylesheetPath = Path.Combine(pathToUnzippedDirectory, filename);

			if (!File.Exists(stylesheetPath))
			{
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("File.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + pathToZippedBundle);
			}

			Exception exception;
			var stylesheet = UsxStylesheet.Load(stylesheetPath, out exception);
			if (exception != null)
			{
				throw new ApplicationException(
					LocalizationManager.GetString("File.StylesheetInvalid",
						"Unable to read stylesheet. File is not a valid Text Release Bundle:") +
					Environment.NewLine + pathToZippedBundle, exception);
			}
			return stylesheet;
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
		public UsxStylesheet Stylesheet
		{
			get { return m_stylesheet; }
		}

		public bool TryGetCanon(int bookId, out Canon canon)
		{
			return m_canons.TryGetValue(bookId, out canon);
		}

		//TODO (PG-36) This method either needs to be greatly improved or replaced
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
