using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using Palaso.IO;
using Palaso.UI.WindowsForms;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Paratext;
using ProtoScript.Bundle;
using ProtoScript.Dialogs;

namespace ProtoScript
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
			string usfmStylesheetPath = Path.Combine(FileLocator.GetDirectoryDistributedWithApplication("sfm"), "usfm.sty");
			return s_usfmStyleSheet = new ScrStylesheet(usfmStylesheetPath);
		}

		public static ScrStylesheetAdapter GetUsfmStylesheet()
		{
			return new ScrStylesheetAdapter(GetUsfmScrStylesheet()); 
		}

		private static DblMetadata GenerateMetadataForSfmProject(IEnumerable<UsxDocument> books, ScrStylesheetAdapter stylesheet, string defaultLanguageName = null)
		{
			string projectId;
			string isoCode;
			string languageName;
			string projectName;
			var wsDefinition = new WritingSystemDefinition();
			var model = new WritingSystemSetupModel(wsDefinition);
			stylesheet.FontFamily = FontHelper.GetSupportsRegular(Project.kDefaultFontPrimary) ? Project.kDefaultFontPrimary : Project.kDefaultFontSecondary;
			model.CurrentDefaultFontName = stylesheet.FontFamily;
			model.CurrentDefaultFontSize = stylesheet.FontSizeInPoints = Project.kDefaultFontSize;

			using (var dlg = new SfmProjectMetadataDlg(model))
			{
				dlg.LanguageName = defaultLanguageName;

				if (dlg.ShowDialog() == DialogResult.Cancel)
					return null;

				projectId = dlg.ProjectId;
				isoCode = dlg.IsoCode;
				projectName = dlg.ProjectName;
				languageName = dlg.LanguageName;
				stylesheet.FontFamily = model.CurrentDefaultFontName;
				stylesheet.FontSizeInPoints = (int)model.CurrentDefaultFontSize;
			}

			var availableBooks = books.Select(b => new Book { Code = b.BookId }).ToList();
			var metadata = new DblMetadata
			{
				id = projectId,
				identification = new DblMetadataIdentification { name = projectName },
				language = new DblMetadataLanguage { iso = isoCode, name = languageName, scriptDirection = model.CurrentRightToLeftScript ? "RTL" : "LTR" },
				AvailableBooks = availableBooks,
				FontFamily = stylesheet.FontFamily,
				FontSizeInPoints = stylesheet.FontSizeInPoints
			};
			return metadata;
		}
	}
}
