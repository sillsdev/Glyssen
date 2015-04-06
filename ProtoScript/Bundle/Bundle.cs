using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using L10NSharp;
using ProtoScript.Character;
using ProtoScript.Properties;
using ProtoScript.Utilities;
using SIL.Reporting;
using SIL.Xml;

namespace ProtoScript.Bundle
{
	public class Bundle : IDisposable
	{
		private readonly DblMetadata m_dblMetadata;
		private readonly UsxStylesheet m_stylesheet;
		private readonly IDictionary<string, UsxDocument> m_books = new Dictionary<string, UsxDocument>();
		private readonly string m_pathToZippedBundle;
		private string m_pathToUnzippedDirectory;

		public Bundle(string pathToZippedBundle)
		{
			m_pathToZippedBundle = pathToZippedBundle;
			try
			{
				m_pathToUnzippedDirectory = Zip.ExtractToTempDirectory(m_pathToZippedBundle);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(LocalizationManager.GetString("File.UnableToExtractBundle",
						"Unable to read contents of Text Release Bundle:") +
					Environment.NewLine + m_pathToZippedBundle, ex);
			}

			m_dblMetadata = LoadMetadata();
			m_stylesheet = LoadStylesheet();

			ExtractBooks();
		}

		private DblMetadata LoadMetadata()
		{
			const string filename = "metadata.xml";
			string metadataPath = Path.Combine(m_pathToUnzippedDirectory, filename);

			if (!File.Exists(metadataPath))
			{
				bool sourceBundle = filename.Contains("source") || Directory.Exists(Path.Combine(m_pathToUnzippedDirectory, "gather"));
				if (sourceBundle)
				{
					throw new ApplicationException(
						string.Format(LocalizationManager.GetString("File.SourceReleaseBundle",
							"This bundle appears to be a source bundle. Only Text Release Bundles are currently supported."), filename) +
						Environment.NewLine + m_pathToZippedBundle);

				}
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("File.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			Exception exception;
			var dblMetadata = DblMetadata.Load(metadataPath, out exception);
			if (exception != null)
			{
				Exception metadataBaseDeserializationError;
				DblMetadataBase metadataBase = XmlSerializationHelper.DeserializeFromFile<DblMetadataBase>(metadataPath,
					out metadataBaseDeserializationError);
				if (metadataBaseDeserializationError != null)
				{
					throw new ApplicationException(
						LocalizationManager.GetString("File.MetadataInvalid",
							"Unable to read metadata. File is not a valid Text Release Bundle:") +
						Environment.NewLine + m_pathToZippedBundle, metadataBaseDeserializationError);
				}

				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("File.MetadataInvalidVersion",
						"Unable to read metadata. Type: {0}. Version: {1}. {2} does not recognize this file as a valid Text Release Bundle:"),
						metadataBase.type, metadataBase.typeVersion, Program.kProduct) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			if (!dblMetadata.IsTextReleaseBundle)
			{
				throw new ApplicationException(
					String.Format(LocalizationManager.GetString("File.NotTextReleaseBundle",
						"This metadata in this bundle indicates that it is of type \"{0}\". Only Text Release Bundles are currently supported."),
						dblMetadata.type));
			}

			dblMetadata.OriginalPathOfDblFile = m_pathToZippedBundle;
			dblMetadata.PgUsxParserVersion = Settings.Default.PgUsxParserVersion;
			dblMetadata.ControlFileVersion = ControlCharacterVerseData.Singleton.ControlFileVersion;

			return dblMetadata;
		}

		private UsxStylesheet LoadStylesheet()
		{
			const string filename = "styles.xml";
			string stylesheetPath = Path.Combine(m_pathToUnzippedDirectory, filename);

			if (!File.Exists(stylesheetPath))
			{
				throw new ApplicationException(
					string.Format(LocalizationManager.GetString("File.FileMissingFromBundle",
						"Required {0} file not found. File is not a valid Text Release Bundle:"), filename) +
					Environment.NewLine + m_pathToZippedBundle);
			}

			Exception exception;
			var stylesheet = UsxStylesheet.Load(stylesheetPath, out exception);
			if (exception != null)
			{
				throw new ApplicationException(
					LocalizationManager.GetString("File.StylesheetInvalid",
						"Unable to read stylesheet. File is not a valid Text Release Bundle:") +
					Environment.NewLine + m_pathToZippedBundle, exception);
			}

			m_dblMetadata.FontFamily = stylesheet.FontFamily;
			m_dblMetadata.FontSizeInPoints = stylesheet.FontSizeInPoints;

			return stylesheet;
		}

		public void CopyVersificationFile(string destinationPath)
		{
			string versificationPath = Path.Combine(m_pathToUnzippedDirectory, Project.kVersificationFileName);

			if (!File.Exists(versificationPath))
				return; // REVIEW (PG-117): Waiting to hear back from Eric whether this is possible. If so, what to do?

			File.Copy(versificationPath, destinationPath);
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
			get { return m_dblMetadata.language.iso; }
		}

		public UsxStylesheet Stylesheet
		{
			get { return m_stylesheet; }
		}

		public bool TryGetBook(string bookId, out UsxDocument book)
		{
			return m_books.TryGetValue(bookId, out book);
		}

		private void ExtractBooks()
		{
			DblMetadataCanon defaultCanon = Metadata.Canons.FirstOrDefault(c => c.Default);
			if (defaultCanon != null)
				ExtractBooksInCanon(GetPathToCanon(defaultCanon.CanonId));
			foreach (DblMetadataCanon canon in Metadata.Canons.Where(c => !c.Default).OrderBy(c => c.CanonId))
				ExtractBooksInCanon(GetPathToCanon(canon.CanonId));
		}

		private string GetPathToCanon(string canonId)
		{
			return Path.Combine(m_pathToUnzippedDirectory, "USX_" + canonId);
		}

		private void ExtractBooksInCanon(string pathToCanon)
		{
			foreach (string filePath in Directory.GetFiles(pathToCanon, "*.usx"))
			{
				var fi = new FileInfo(filePath);
				string bookId = Path.GetFileNameWithoutExtension(fi.Name);
				if (bookId.Length == 3 && !m_books.ContainsKey(bookId))
					m_books.Add(bookId, new UsxDocument(filePath));
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (m_pathToUnzippedDirectory != null && Directory.Exists(m_pathToUnzippedDirectory))
			{
				try
				{
					Directory.Delete(m_pathToUnzippedDirectory, true);
				}
				catch (Exception e)
				{
					ErrorReport.ReportNonFatalExceptionWithMessage(e,
						string.Format("Failed to clean up temporary folder where bundle was unzipped: {0}.", m_pathToUnzippedDirectory));
				}
				m_pathToUnzippedDirectory = null;
			}
		}

		#endregion
	}
}
