using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen;
using L10NSharp;
using Paratext;
using Glyssen.Bundle;
using Glyssen.Dialogs;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.IO;
using SIL.Windows.Forms;
using SIL.Windows.Forms.WritingSystems;
using SIL.WritingSystems;

namespace Glyssen
{
	public static class SfmLoader
	{
		private static ScrStylesheet s_usfmStyleSheet;

		public static UsxDocument LoadSfmBook(string sfmFilePath)
		{
			var book = new UsxDocument(UsfmToUsx.ConvertToXmlDocument(GetUsfmScrStylesheet(), File.ReadAllText(sfmFilePath)));
			var bookId = book.BookId;
			if (bookId.Length != 3)
				throw new Exception(string.Format(LocalizationManager.GetString("Project.StandardFormat.InvalidBookId",
					"Invalid Book ID: {0}", "Parameter is the bookId"), bookId));
			return book;
		}

		public static Project LoadSfmBookAndMetadata(string sfmFilePath)
		{
			var books = new[] { LoadSfmBook(sfmFilePath) };
			var stylesheet = GetUsfmStylesheet();
			var metadata = GenerateMetadataForSfmProject(books, stylesheet);
			if (metadata == null)
				return null;
			metadata.OriginalPathOfSfmFile = sfmFilePath;
			return new Project(metadata, books, stylesheet);
		}

		public static List<UsxDocument> LoadSfmFolder(string sfmFolderPath)
		{
			var books = new List<UsxDocument>();
			Exception firstFailure = null;
			foreach (var sfmFilePath in Directory.GetFiles(sfmFolderPath))
			{
				try
				{
					var book = new UsxDocument(UsfmToUsx.ConvertToXmlDocument(GetUsfmScrStylesheet(), File.ReadAllText(sfmFilePath)));
					var bookId = book.BookId;
					if (bookId.Length != 3)
						throw new Exception(string.Format(LocalizationManager.GetString("Project.StandardFormat.InvalidBookId",
							"Invalid Book ID: {0}", "Parameter is the bookId"), bookId));
					books.Add(book);
				}
				catch (Exception e)
				{
					if (firstFailure == null)
						firstFailure = e;
				}
			}
			if (books.Count == 0)
			{
				if (firstFailure != null)
					throw firstFailure;
				throw new Exception(string.Format(LocalizationManager.GetString("Project.StandardFormat.NoValidSfBooksInFolder",
					"No valid Standard Format Scripture books were found in: {0}", "Parameter is the path to the folder of standard format files"), sfmFolderPath));
			}
			return books;
		}

		public static Project LoadSfmFolderAndMetadata(string sfmFolderPath)
		{
			List<UsxDocument> books = LoadSfmFolder(sfmFolderPath);
			var stylesheet = GetUsfmStylesheet();
			var metadata = GenerateMetadataForSfmProject(books, stylesheet, Path.GetFileName(sfmFolderPath));
			if (metadata == null)
				return null;
			metadata.OriginalPathOfSfmDirectory = sfmFolderPath;
			return new Project(metadata, books, stylesheet);
		}

		public static ScrStylesheet GetUsfmScrStylesheet()
		{
			if (s_usfmStyleSheet != null)
				return s_usfmStyleSheet;
			string usfmStylesheetPath = FileLocator.GetFileDistributedWithApplication("sfm", "usfm.sty");
			return s_usfmStyleSheet = new ScrStylesheet(usfmStylesheetPath);
		}

		public static ScrStylesheetAdapter GetUsfmStylesheet()
		{
			return new ScrStylesheetAdapter(GetUsfmScrStylesheet()); 
		}

		private static PgDblTextMetadata GenerateMetadataForSfmProject(IEnumerable<UsxDocument> books, ScrStylesheetAdapter stylesheet, string defaultLanguageName = null)
		{
			string projectId;
			string isoCode;
			string languageName;
			string publicationName;
			string recordingProjectName;
			var wsDefinition = new WritingSystemDefinition();
			var wsModel = new WritingSystemSetupModel(wsDefinition);
			stylesheet.FontFamily = FontHelper.GetSupportsRegular(Project.kDefaultFontPrimary) ? Project.kDefaultFontPrimary : Project.kDefaultFontSecondary;
			wsModel.CurrentDefaultFontName = stylesheet.FontFamily;
			wsModel.CurrentDefaultFontSize = stylesheet.FontSizeInPoints = Project.kDefaultFontSize;
			var model = new ProjectMetadataViewModel(wsModel) { LanguageName = defaultLanguageName };

			using (var dlg = new ProjectMetadataDlg(model, true))
			{
				if (dlg.ShowDialog() == DialogResult.Cancel)
					return null;

				recordingProjectName = model.RecordingProjectName;
				projectId = model.PublicationId;
				isoCode = model.IsoCode;
				languageName = model.LanguageName;
				publicationName = model.PublicationName;
				stylesheet.FontFamily = model.WsModel.CurrentDefaultFontName;
				stylesheet.FontSizeInPoints = (int)model.WsModel.CurrentDefaultFontSize;
			}

			var availableBooks = books.Select(b => new Book { Code = b.BookId }).ToList();
			var metadata = new PgDblTextMetadata
			{
				Id = projectId,
				//PgRecordingProjectName = recordingProjectName,
				Identification = new DblMetadataIdentification { Name = publicationName },
				Language = new PgDblMetadataLanguage { Iso = isoCode, Name = languageName, ScriptDirection = wsModel.CurrentRightToLeftScript ? "RTL" : "LTR" },
				AvailableBooks = availableBooks,
				FontFamily = stylesheet.FontFamily,
				FontSizeInPoints = stylesheet.FontSizeInPoints
			};
			return metadata;
		}
	}
}
