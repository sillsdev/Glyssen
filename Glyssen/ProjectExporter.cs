using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Properties;
using L10NSharp;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;

namespace Glyssen
{
	public enum ExportFileType
	{
		Excel,
		TabSeparated
	}

	public enum ExportColumn
	{
		BlockId = 0,
		Actor = 1,
		ParaTag = 2,
		BookId = 3,
		Chapter = 4,
		Verse = 5,
		CharacterId = 6,
		Delivery = 7,
		VernacularText = 8,
		PrimaryReferenceText = 9,
		SecondaryReferenceText = 10,
		VernacularTextLength = 11,
	}

	public class ProjectExporter
	{
		private const string kExcelLineBreak = "\r\n"; ///????????????????????
		private const string kTabFileAnnotationElementSeparator = " ";
		public const string kExcelFileExtension = ".xlsx";
		public const string kTabDelimitedFileExtension = ".txt";

		private string m_customFileName;
		private int m_numberOfFileSuccessfullyExported;
		private readonly bool m_includeVoiceActors;
		private readonly IReadOnlyList<BookScript> m_booksToExport;
		private List<int> m_annotatedRowIndexes;
		private readonly Regex m_splitRegex = new Regex(@"(\|\|\|[^\|]+\|\|\|)|(\{(?:Music|F8|SFX).*?\})");

		public ProjectExporter(Project project)
		{
			Project = project;
			m_includeVoiceActors = Project.CharacterGroupList.AnyVoiceActorAssigned();
			m_booksToExport = new List<BookScript>(project.ReferenceText.GetBooksWithBlocksConnectedToReferenceText(project));
		}

		public Project Project { get; private set; }
		public bool IncludeVoiceActors { get { return m_includeVoiceActors; } }
		public bool IncludeActorBreakdown { get; set; }
		public bool IncludeBookBreakdown { get; set; }
		public bool IncludeCreateClips { get; set; }
		public bool ExportAnnotationsInSeparateRows { get; set; }
		
		internal ExportFileType SelectedFileType { get; set; }

		internal string RecordingScriptFileNameSuffix
		{
			get
			{
				return LocalizationManager.GetString("DialogBoxes.ExportDlg.RecordingScriptFileNameDefaultSuffix", "Recording Script");
			}
		}

		private string DefaultDirectory
		{
			get
			{
				var defaultDirectory = Settings.Default.DefaultExportDirectory;
				if (string.IsNullOrWhiteSpace(defaultDirectory))
				{
					defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Program.kProduct);
					if (!Directory.Exists(defaultDirectory))
						Directory.CreateDirectory(defaultDirectory);
				}

				return defaultDirectory;
			}
		}

		internal string CurrentBaseFolder
		{
			get { return Path.GetDirectoryName(FullFileName); }
		}

		internal string FullFileName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_customFileName))
					return m_customFileName;

				var defaultFileName = Project.Name + " " +
					RecordingScriptFileNameSuffix + GetFileExtension(SelectedFileType);

				return Path.Combine(DefaultDirectory, defaultFileName.Trim());
			}
			set
			{
				m_customFileName = value;
			}
		}

		private string FileNameWithoutExtension
		{
			get { return Path.GetFileNameWithoutExtension(FullFileName); }
		}

		internal string ActorDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ActorDirectoryNameSuffix", "Voice Actors");
				return Path.Combine(CurrentBaseFolder, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal string BookDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.BookDirectoryNameSuffix", "Books");
				return Path.Combine(CurrentBaseFolder, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal string ClipDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ClipDirectoryNameSuffix", "Clips");
				return Path.Combine(CurrentBaseFolder, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		public string AnnotationElementSeparator
		{
			get { return SelectedFileType == ExportFileType.Excel ? kExcelLineBreak : kTabFileAnnotationElementSeparator; }
		}

		//internal  GetListOfFilesInUse()
		//{
		//	try
		//	{
		//		lockedFiles.AddRange(ProcessMasterScriptFile(FullFileName));
		//		if (IncludeActorBreakdown && Directory.Exists(ActorDirectory))
		//			lockedFiles.AddRange(ProcessActorFiles(ActorDirectory));
		//		if (IncludeBookBreakdown && Directory.Exists(BookDirectory))
		//			lockedFiles.AddRange(ProcessBookFiles(BookDirectory));
		//	}
		//	return lockedFiles;
		//}

		internal IReadOnlyDictionary<string, List<string>> ExportNow(bool openForMe)
		{
			var lockedFiles = new List<Tuple<string, string>>();

			m_numberOfFileSuccessfullyExported = 0;
			
			lockedFiles.AddRange(GenerateMasterScriptFile(FullFileName));

			// remember the location (at least for this project and possible as new default)
			Project.Status.LastExportLocation = CurrentBaseFolder;
			if (!string.IsNullOrEmpty(m_customFileName))
			{
				// if the directory is not the stored default directory, make the new directory the default
				if (!DirectoryUtilities.AreDirectoriesEquivalent(Project.Status.LastExportLocation, DefaultDirectory))
					Settings.Default.DefaultExportDirectory = Project.Status.LastExportLocation;
			}

			if (IncludeActorBreakdown)
			{
				try
				{
					Directory.CreateDirectory(ActorDirectory);
					lockedFiles.AddRange(GenerateActorFiles(ActorDirectory));
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.NotifyUserOfProblem(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportActors",
						"Could not create destination folder for voice actor script files: {0}", "{0} is a directory name."), ActorDirectory));
				}
			}
			if (IncludeBookBreakdown)
			{
				try
				{
					Directory.CreateDirectory(BookDirectory);
					lockedFiles.AddRange(GenerateBookFiles(BookDirectory));
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.NotifyUserOfProblem(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportBooks",
						"Could not create destination folder for book script files: {0}", "{0} is a directory name."), BookDirectory));
				}
			}

			if (IncludeCreateClips)
			{
				try
				{
					Directory.CreateDirectory(ClipDirectory);
					GenerateClipFiles(ClipDirectory);
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.NotifyUserOfProblem(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportClips",
						"Could not create destination folder for clip files: {0}", "{0} is a directory name."), ClipDirectory));
				}
			}

			if (openForMe && !lockedFiles.Any())
				OpenExportFileOrLocation();

			var result = new Dictionary<string, List<string>>();
			foreach (var lockedFile in lockedFiles)
			{
				List<string> list;
				if (!result.TryGetValue(lockedFile.Item1, out list))
					result[lockedFile.Item1] = list = new List<string>();
				list.Add(lockedFile.Item2);
			}
			return result;
		}

		public void OpenExportFileOrLocation()
		{
			try
			{
				if (m_numberOfFileSuccessfullyExported > 0 && Project.Status.LastExportLocation != null)
				{
					if (IncludeActorBreakdown || IncludeBookBreakdown)
						PathUtilities.OpenDirectoryInExplorer(Project.Status.LastExportLocation);
					else
						PathUtilities.OpenFileInApplication(FullFileName);
				}
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				// Oh well, probably not worth bugging the user with a yellow screen.
			}
		}

		internal DataTable GeneratePreviewTable()
		{
			var dt = new DataTable();

			foreach (string columnName in GetHeaders())
				dt.Columns.Add(columnName);

			foreach (var line in GetExportData())
				dt.Rows.Add(line.ToArray());
		
			return dt;
		}

		private IEnumerable<Tuple<string, string>> GenerateMasterScriptFile(string path)
		{
			Analytics.Track("Export", new Dictionary<string, string>
				{
					{ "exportType", SelectedFileType.ToString() },
					{ "includeVoiceActors", IncludeVoiceActors.ToString() }
				});

			return GenerateFile(path, () => GetExportData());
		}

		private IEnumerable<Tuple<string, string>> GenerateFile(string path, Func<IEnumerable<List<object>>> getData)
		{
			Action<string, IEnumerable<List<object>>> generateFile = (SelectedFileType == ExportFileType.TabSeparated)
				? (Action<string, IEnumerable<List<object>>>) GenerateTabSeparatedFile
				: GenerateExcelFile;

			Exception caughtException = null;
			try
			{
				generateFile(path, getData());
				m_numberOfFileSuccessfullyExported++;
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				caughtException = ex;
			}
			if (caughtException != null)
				yield return new Tuple<string, string>(caughtException.Message, path);
		}

		private IEnumerable<Tuple<string, string>> GenerateActorFiles(string directoryPath)
		{
			if (IncludeVoiceActors)
			{
				foreach (var actor in Project.VoiceActorList.AllActors.Where(a => Project.CharacterGroupList.HasVoiceActorAssigned(a.Id)))
				{
					var actorId = actor.Id;
					foreach (var lockedFile in GenerateFile(Path.Combine(directoryPath, actor.Name), () => GetExportData(voiceActorId: actorId)))
					{
						yield return lockedFile;
					}
				}
			}
		}

		private IEnumerable<Tuple<string, string>> GenerateBookFiles(string directoryPath)
		{
			foreach (var book in m_booksToExport)
			{
				var bookId = book.BookId;
				foreach (var lockedFile in GenerateFile(Path.Combine(directoryPath, book.BookId), () => GetExportData(bookId)))
				{
					yield return lockedFile;
				}
			}
		}

		private void GenerateClipFiles(string directoryPath)
		{
			const int kLineNumIndex = 0;
			const int kBookIndex = 2;
			const int kChapterIndex = 3;
			const int kVerseIndex = 4;

			var data = GetExportData();
			var bookId = string.Empty;
			var currentOutputDirectory = string.Empty;
			var fileNameTemplate = Project.Name.Replace(" ", "_") + "_{0:D5}_{1}_{2:D3}_{3:D3}.wav";

			foreach (var row in data)
			{
				// if this is a different book, create the output directory
				var tempBookId = (string) row[kBookIndex];
				if (!string.IsNullOrEmpty(tempBookId) && (bookId != tempBookId))
				{
					bookId = tempBookId;
					currentOutputDirectory = Path.Combine(directoryPath, bookId);
					Directory.CreateDirectory(currentOutputDirectory);
				}

				// if the line number is not empty, create the wave file
				if (row[kLineNumIndex] != null)
				{
					var lineNumber = (int) row[kLineNumIndex];
					var verseNumber = int.Parse(row[kVerseIndex].ToString().Split('-', ',', ' ')[0]);
					var fileName = Path.Combine(currentOutputDirectory, 
						string.Format(fileNameTemplate, lineNumber, bookId, row[kChapterIndex], verseNumber));

					// do not overwrite existing files
					if (File.Exists(fileName))
						continue;

					using (var ms = new MemoryStream())
					{
						Resources.Silent.CopyTo(ms);
						using (var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
						{
							ms.WriteTo(fs);
						}
					}
				}
			}
		}

		private void GenerateTabSeparatedFile(string path, IEnumerable<List<object>> data)
		{
			if (Path.GetExtension(path) != kTabDelimitedFileExtension)
				path += kTabDelimitedFileExtension;

			using (var stream = new StreamWriter(path, false, Encoding.UTF8))
			{
				stream.WriteLine(GetTabSeparatedLine(GetHeaders()));
				foreach (var line in data.Select(GetTabSeparatedLine))
					stream.WriteLine(line);
			}
		}

		private void GenerateExcelFile(string path, IEnumerable<List<object>> data)
		{
			if (Path.GetExtension(path) != kExcelFileExtension)
				path += kExcelFileExtension;

			// If we got this far with a path to an existing file, the user has (in theory)
			// confirmed he wants to overwrite it.
			// We need to delete it first or the code will attempt to modify it instead.
			File.Delete(path);

			var dataArray = data.Select(d => d.ToArray()).ToList();
			dataArray.Insert(0, GetHeaders().ToArray());
			using (var xls = new ExcelPackage(new FileInfo(path)))
			{
				var sheet = xls.Workbook.Worksheets.Add("Script");
				sheet.Cells["A1"].LoadFromArrays(dataArray);

				ColorizeAnnotations(sheet);

				sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
				sheet.Row(1).Style.Font.Bold = true;

				var columnNum = 1;
				sheet.Column(columnNum++).AutoFit(2d, sheet.DefaultColWidth); // line number

				if (IncludeVoiceActors)
					sheet.Column(columnNum++).AutoFit(2d, 20d); // voice actor
				
				sheet.Column(columnNum++).AutoFit(2d, sheet.DefaultColWidth); // style tag
				sheet.Column(columnNum++).AutoFit(2d, sheet.DefaultColWidth); // book
				sheet.Column(columnNum++).AutoFit(2d, sheet.DefaultColWidth); // chapter
				sheet.Column(columnNum++).AutoFit(2d, sheet.DefaultColWidth); // verse
				sheet.Column(columnNum++).AutoFit(2d, 20d); // character ID

				// add a column for the localized character id
				if (LocalizationManager.UILanguageId != "en")
					sheet.Column(columnNum++).AutoFit(2d, 20d); // localized character ID

				// skip the delivery column
				columnNum++;

				// for script text set both width and text wrapping
				sheet.Column(columnNum).Style.WrapText = true; // script text
				sheet.Column(columnNum++).Width = 50d;

				sheet.Column(columnNum).Style.WrapText = true; // primaryReferenceText text
				sheet.Column(columnNum++).Width = 50d;

				if (Project.ReferenceText.HasSecondaryReferenceText)
				{
					sheet.Column(columnNum).Style.WrapText = true; // secondaryReferenceText text
					sheet.Column(columnNum++).Width = 50d;
				}

				// this is the last column, no need to increment columNum
				sheet.Column(columnNum).AutoFit(2d, sheet.DefaultColWidth); // block length

				sheet.View.FreezePanes(2, 1);

				xls.Save();
			}
		}

		private void ColorizeAnnotations(ExcelWorksheet sheet)
		{
			foreach (var rowIndex in m_annotatedRowIndexes)
			{
				// Excel row index is 1-based, and a header row has been inserted, so add 2.
				// Excel column indexes are also 1-based, so add 1.
				var excelRowIndex = rowIndex + 2;
				var cell = sheet.Cells[excelRowIndex, GetColumnIndex(ExportColumn.PrimaryReferenceText) + 1].First();
				SetAnnotationColors(cell);

				if (Project.ReferenceText.HasSecondaryReferenceText)
				{
					cell = sheet.Cells[excelRowIndex, GetColumnIndex(ExportColumn.SecondaryReferenceText) + 1].First();
					SetAnnotationColors(cell);
				}
			}
		}

		private void SetAnnotationColors(ExcelRangeBase cell)
		{
			// do nothing if the cell is empty
			var cellValue = cell.Value as string;
			if (string.IsNullOrEmpty(cellValue)) return;

			var splits = m_splitRegex.Split(cellValue);

			// list of string beginnings of strings to color blue
			var blueBeginnings = new[] { "|||" };

			// list of string beginnings of strings to color red
			var redBeginnings = new[] { "{Music", "{F8", "{SFX" };

			cell.RichText.Clear();
			foreach (var split in splits)
			{
				var r = cell.RichText.Add(split);

				if (blueBeginnings.Any(split.StartsWith))
				{
					r.Color = Color.Blue;
				}
				else if (redBeginnings.Any(split.StartsWith))
				{
					r.Color = Color.Red;
				}
				else
				{
					r.Color = Color.Black;
				}
			}
		}

		// internal for testing
		internal int GetColumnIndex(ExportColumn column)
		{
			int columnNumber = (int) column;
			if (column != ExportColumn.BlockId && !IncludeVoiceActors)
				columnNumber--;
			if (column == ExportColumn.VernacularTextLength)
			{
				if (!Project.ReferenceText.HasSecondaryReferenceText)
					columnNumber--;
			}
			if (LocalizationManager.UILanguageId != "en")
				columnNumber++;
			return columnNumber;
		}

		// internal for testing
		internal List<List<object>> GetExportData(string bookId = null, int voiceActorId = -1)
		{
			var result = new List<List<object>>();

			int blockNumber = 1;

			IEnumerable<BookScript> booksToInclude = bookId == null
				? m_booksToExport
				: m_booksToExport.Where(b => b.BookId == bookId);

			foreach (var book in booksToInclude)
			{
				string singleVoiceNarratorOverride = null;
				if (book.SingleVoice)
					singleVoiceNarratorOverride = CharacterVerseData.GetStandardCharacterId(book.BookId,
						CharacterVerseData.StandardCharacter.Narrator);
				List<Block> pendingMismatchedReferenceBlocks = null;
				foreach (var block in book.GetScriptBlocks())
				{
					if (block.IsChapterAnnouncement && block.ChapterNumber == 1)
					{
						if (Project.SkipChapterAnnouncementForFirstChapter)
							continue;
						if (Project.SkipChapterAnnouncementForSingleChapterBooks &&
							Project.Versification.LastChapter(BCVRef.BookToNumber(book.BookId)) == 1)
							continue;
					}
					VoiceActor.VoiceActor voiceActor = null;
					bool includeInOutput = true;
					if (IncludeVoiceActors)
					{
						voiceActor = Project.GetVoiceActorForCharacter(singleVoiceNarratorOverride ?? block.CharacterIdInScript) ?? GetDummyActor();
						includeInOutput = voiceActorId == -1 || voiceActor.Id == voiceActorId;
					}

					if (includeInOutput)
					{
						if (pendingMismatchedReferenceBlocks != null && block.ReferenceBlocks.Any())
						{
							foreach (var refBlock in pendingMismatchedReferenceBlocks)
								result.Add(GetExportDataForReferenceBlock(refBlock, book.BookId));
							pendingMismatchedReferenceBlocks = null;
						}
						result.Add(GetExportDataForBlock(block, blockNumber++, book.BookId, voiceActor, singleVoiceNarratorOverride, IncludeVoiceActors,
							Project.ReferenceText.HasSecondaryReferenceText));
						if (!block.MatchesReferenceText && block.ReferenceBlocks.Any())
							pendingMismatchedReferenceBlocks = block.ReferenceBlocks;
					}
				}
				if (pendingMismatchedReferenceBlocks != null)
				{
					foreach (var refBlock in pendingMismatchedReferenceBlocks)
						result.Add(GetExportDataForReferenceBlock(refBlock, book.BookId));
				}
			}
			if (bookId == null && voiceActorId == -1)
				AddAnnotations(result);
			return result;
		}

		private void AddAnnotations(List<List<object>> data)
		{
			var annotationRowIndexes = new List<int>();

			int lastIndexOfPreviousVerse = 0;
			BCVRef previousReferenceTextVerse = null;
			IEnumerable<VerseAnnotation> annotationsForPreviousVerse = null;
			for (int i = 0; i < data.Count; i++)
			{
				var row = data[i];
				if (HasReferenceText(row))
				{
					var referenceTextVerse = GetBcvRefForRow(row);
					if (referenceTextVerse != previousReferenceTextVerse)
					{
						if (previousReferenceTextVerse != null && annotationsForPreviousVerse != null)
						{
							foreach (var verseAnnotation in annotationsForPreviousVerse.Where(va => va.Annotation is Pause))
							{
								if (ExportAnnotationsInSeparateRows)
								{
									var rowIndex = lastIndexOfPreviousVerse + 1 + verseAnnotation.Offset;
									data.Insert(rowIndex,
										GetExportDataForAnnotation(verseAnnotation, BCVRef.NumberToBookCode(previousReferenceTextVerse.Book),
											previousReferenceTextVerse.Chapter, previousReferenceTextVerse.Verse.ToString()));
									i++;
									annotationRowIndexes.Add(rowIndex);
								}
								else
									annotationRowIndexes.Add(AddAnnotationData(data, lastIndexOfPreviousVerse, verseAnnotation));
							}
						}
						if (referenceTextVerse != previousReferenceTextVerse)
						{
							var annotationsForVerse = ControlAnnotations.Singleton.GetAnnotationsForVerse(referenceTextVerse);
							foreach (var verseAnnotation in annotationsForVerse.Where(va => va.Annotation is Sound))
							{
								if (ExportAnnotationsInSeparateRows)
								{
									var rowIndex = i++ + verseAnnotation.Offset;
									data.Insert(rowIndex,
										GetExportDataForAnnotation(verseAnnotation, BCVRef.NumberToBookCode(referenceTextVerse.Book),
											referenceTextVerse.Chapter, referenceTextVerse.Verse.ToString()));
									annotationRowIndexes.Add(rowIndex);
								}
								else
									annotationRowIndexes.Add(AddAnnotationData(data, i, verseAnnotation));
							}

							annotationsForPreviousVerse = annotationsForVerse;
							previousReferenceTextVerse = referenceTextVerse;
						}
					}
					lastIndexOfPreviousVerse = i;
				}
			}

			m_annotatedRowIndexes = annotationRowIndexes;
		}

		private int AddAnnotationData(List<List<object>> data, int relativeIndex, VerseAnnotation verseAnnotation)
		{
			Func<string, string, string> modify = (verseAnnotation.Annotation is Sound)
				? (Func<string, string, string>)PrependAnnotationInfo : (Func<string, string, string>)AppendAnnotationInfo;

			var col = GetColumnIndex(ExportColumn.PrimaryReferenceText);
			var annotationInfo = verseAnnotation.Annotation.ToDisplay(AnnotationElementSeparator);

			List<object> rowToModify;
			string text;
			int rowIndex = relativeIndex + verseAnnotation.Offset;
			do
			{
				rowToModify = data[rowIndex];
				text = (string) rowToModify[col];
			} while (text == null && --rowIndex >= 0);
			Debug.Assert(text != null, "We should have been able to find a preceding row with a non-empty reference text");

			rowToModify[col] = modify(text, annotationInfo);
			if (Project.ReferenceText.HasSecondaryReferenceText)
			{
				col = GetColumnIndex(ExportColumn.SecondaryReferenceText);
				rowToModify[col] = modify((string)rowToModify[col], annotationInfo);
			}

			return rowIndex;
		}

		private string AppendAnnotationInfo(string text, string annotationInfo)
		{
			var separator = char.IsWhiteSpace(text.LastOrDefault()) || char.IsWhiteSpace(annotationInfo.FirstOrDefault()) ?
				string.Empty : " ";
			return text + separator + annotationInfo;
		}

		private string PrependAnnotationInfo(string text, string annotationInfo)
		{
			var separator = char.IsWhiteSpace(annotationInfo.LastOrDefault()) || char.IsWhiteSpace(text.FirstOrDefault()) ?
				string.Empty : " ";
			return annotationInfo + separator + text;
		}
		private bool HasReferenceText(List<object> dataRow)
		{
			int offset = IncludeVoiceActors ? 1 : 0;
			offset += LocalizationManager.UILanguageId != "en" ? 1 : 0;
			return !string.IsNullOrEmpty((string)dataRow[8 + offset]);
		}

		private BCVRef GetBcvRefForRow(List<object> row)
		{
			int offset = IncludeVoiceActors ? 1 : 0;
			return new BCVRef(BCVRef.BookToNumber((string)row[2 + offset]), (int)row[3 + offset], ScrReference.VerseToIntStart((string)row[4 + offset]));
		}

		private List<object> GetExportDataForAnnotation(VerseAnnotation verseAnnotation, string bookId, int chapter, string verse)
		{
			var row = new List<object>();
			row.Add(null);
			if (IncludeVoiceActors)
				row.Add(null);
			row.Add(null);
			row.Add(bookId);
			row.Add(chapter);
			row.Add(verse);
			row.Add(null);
			if (LocalizationManager.UILanguageId != "en")
				row.Add(null);
			row.Add(null);
			row.Add(null);
			var annotationInfo = verseAnnotation.Annotation.ToDisplay(AnnotationElementSeparator);
			row.Add(annotationInfo);
			if (Project.ReferenceText.HasSecondaryReferenceText)
				row.Add(annotationInfo);
			row.Add(null);
			return row;
		}

		private List<object> GetExportDataForReferenceBlock(Block refBlock, string bookId)
		{
			var row = new List<object>();
			row.Add(null);
			if (IncludeVoiceActors)
				row.Add(null);
			row.Add(refBlock.StyleTag);
			row.Add(bookId);
			row.Add(refBlock.ChapterNumber);
			row.Add(refBlock.InitialVerseNumberOrBridge);
			row.Add((CharacterVerseData.IsCharacterStandard(refBlock.CharacterId) ?
				CharacterVerseData.GetStandardCharacterIdAsEnglish(refBlock.CharacterId) : refBlock.CharacterId));
			if (LocalizationManager.UILanguageId != "en")
				row.Add(null);
			row.Add(refBlock.Delivery);
			row.Add(null);
			row.Add(refBlock.GetText(true, true));
			if (Project.ReferenceText.HasSecondaryReferenceText)
				row.Add(refBlock.PrimaryReferenceText);
			row.Add(0);
			return row;
		}

		private List<object> GetHeaders()
		{
			List<object> headers = new List<object>(10);
			headers.Add("#");
			if (IncludeVoiceActors)
				headers.Add("Actor");
			headers.Add("Tag");
			headers.Add("Book");
			headers.Add("Chapter");
			headers.Add("Verse");

			// add a column for the localized character id
			if (LocalizationManager.UILanguageId == "en")
			{
				headers.Add("Character");
			}
			else
			{
				headers.Add("Character (English)");
				headers.Add("Character");
			}

			headers.Add("Delivery");
			headers.Add("Text");
			AddDirectorsGuideHeader(headers, Project.ReferenceText.LanguageName);
			if (Project.ReferenceText.HasSecondaryReferenceText)
				AddDirectorsGuideHeader(headers, Project.ReferenceText.SecondaryReferenceTextLanguageName);
			headers.Add("Size");
			return headers;
		}

		private void AddDirectorsGuideHeader(List<object> headers, string languageName)
		{
			headers.Add(String.Format("{0} Director's Guide", languageName));
		}

		internal static List<object> GetExportDataForBlock(Block block, int blockNumber, string bookId,
			VoiceActor.VoiceActor voiceActor, string singleVoiceNarratorOverride, bool useCharacterIdInScript, bool includeSecondaryDirectorsGuide)
		{
			// NOTE: if the order here changes, there may be changes needed in GenerateExcelFile
			List<object> list = new List<object>();
			list.Add(blockNumber);
			if (voiceActor != null)
				list.Add(voiceActor.Name);
			list.Add(block.StyleTag);
			list.Add(bookId);
			list.Add(block.ChapterNumber);
			list.Add(block.InitialVerseNumberOrBridge);
			string characterId;
			if (singleVoiceNarratorOverride != null)
				characterId = singleVoiceNarratorOverride;
			else
				characterId = useCharacterIdInScript ? block.CharacterIdInScript : block.CharacterId;
			list.Add(CharacterVerseData.IsCharacterStandard(characterId) ? CharacterVerseData.GetStandardCharacterIdAsEnglish(characterId) : characterId);
			
			// add a column for the localized character id
			if (LocalizationManager.UILanguageId != "en")
				list.Add(CharacterVerseData.GetCharacterNameForUi(characterId));
				
			list.Add(block.Delivery);
			list.Add(block.GetText(true));
			list.Add(block.PrimaryReferenceText);
			if (includeSecondaryDirectorsGuide)
			{
				var primaryRefBlock = (block.MatchesReferenceText) ? block.ReferenceBlocks.Single() : null;
				if (primaryRefBlock != null && primaryRefBlock.MatchesReferenceText)
					list.Add(primaryRefBlock.PrimaryReferenceText);
				else
					list.Add(null);
			}
			list.Add(block.GetText(false).Length);
			return list;
		}

		internal static string GetTabSeparatedLine(List<object> items)
		{
			const string kTabSeparator = "\t";
			return string.Join(kTabSeparator, items);
		}

		public static string GetFileExtension(ExportFileType fileType)
		{
			switch (fileType)
			{
				case ExportFileType.Excel:
					return kExcelFileExtension;
				case ExportFileType.TabSeparated:
					return kTabDelimitedFileExtension;
			}
			return null;
		}

		private VoiceActor.VoiceActor GetDummyActor()
		{
			return new VoiceActor.VoiceActor{Id = -1};
		}

		#region Roles for Voice Actors
		public void ExportRolesForVoiceActors(string path)
		{
			GenerateRolesForVoiceActorsExport(path, GetRolesForVoiceActorsData());
		}

		private void GenerateRolesForVoiceActorsExport(string path, List<List<object>> data)
		{
			if (Path.GetExtension(path) != kExcelFileExtension)
				path += kExcelFileExtension;

			// If we got this far with a path to an existing file, the user has (in theory)
			// confirmed he wants to overwrite it.
			// We need to delete it first or the code will attempt to modify it instead.
			File.Delete(path);

			var dataArray = data.Select(d => d.ToArray()).ToList();
			dataArray.Insert(0, GetRolesHeaders().ToArray());
			using (var xls = new ExcelPackage(new FileInfo(path)))
			{
				const int kMaxHeight = 65;

				var sheet = xls.Workbook.Worksheets.Add("Roles for Voice Actors");
				sheet.Cells["A1"].LoadFromArrays(dataArray);

				// Counter-intuitively, these two lines together set "Fit All Columns on One Page" to true
				sheet.PrinterSettings.FitToPage = true;
				sheet.PrinterSettings.FitToHeight = 0;

				sheet.PrinterSettings.RepeatRows = sheet.Cells["1:1"];
				sheet.PrinterSettings.ShowGridLines = true;

				sheet.Cells.Style.Font.Size = 10;
				sheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
				sheet.Row(1).Style.Font.Bold = true;

				sheet.Column(1).Width = 8; // Group ID
				sheet.Column(2).Style.WrapText = true; // Character Roles
				sheet.Column(2).Width = 50;
				sheet.Column(3).Style.WrapText = true; // Attributes
				sheet.Column(3).Width = 17;
				sheet.Column(4).Width = 7; // Hours
				sheet.Column(5).AutoFit(10); // Voice Actor

				for (int i = 1; i <= data.Count + 1; i++)
					if (sheet.Cells[i, 2].Value.ToString().Length > 300)
						sheet.Row(i).Height = kMaxHeight;

				sheet.View.FreezePanes(2, 1);

				xls.Save();
			}
		}

		private List<object> GetRolesHeaders()
		{
			List<object> headers = new List<object>(5);
			headers.Add("Group ID");
			headers.Add("Character Roles");
			headers.Add("Attributes");
			headers.Add("Hours");
			headers.Add("Voice Actor");
			return headers;
		}

		private List<List<object>> GetRolesForVoiceActorsData()
		{
			var groups = new List<List<object>>();
			foreach (var characterGroup in Project.CharacterGroupList.CharacterGroups)
			{
				var group = new List<object>();
				group.Add(characterGroup.GroupIdForUiDisplay);
				group.Add(characterGroup.CharacterIds);
				group.Add(characterGroup.AttributesDisplay);
				group.Add(string.Format("{0:N2}", characterGroup.EstimatedHours));
				group.Add(characterGroup.IsVoiceActorAssigned ? characterGroup.VoiceActor.Name : string.Empty);
				groups.Add(group);
			}
			return groups;
		}
		#endregion
	}
}
