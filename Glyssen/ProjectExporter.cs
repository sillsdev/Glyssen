using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopAnalytics;
using Glyssen.Character;
using L10NSharp;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SIL.Scripture;

namespace Glyssen
{
	public enum ExportFileType
	{
		Excel,
		TabSeparated
	}

	public class ProjectExporter
	{
		public const string kExcelFileExtension = ".xlsx";
		public const string kTabDelimitedFileExtension = ".txt";
		private const string Separator = "\t";

		private List<Tuple<int, string, List<object>>> m_data;

		public ProjectExporter(Project project)
		{
			Project = project;
			IncludeVoiceActors = Project.CharacterGroupList.AnyVoiceActorAssigned();
		}

		public Project Project { get; private set; }

		public bool IncludeVoiceActors { get; private set; }

		public void GenerateFile(string path, ExportFileType fileType)
		{
			switch (fileType)
			{
				case ExportFileType.TabSeparated:
					GenerateTabSeparatedFile(path, GetExportData().Select(t => t.Item3));
					break;
				default:
					GenerateExcelFile(path, GetExportData().Select(t => t.Item3));
					break;
			}

			Analytics.Track("Export",
				new Dictionary<string, string>
				{
					{ "exportType", fileType.ToString() },
					{ "includeVoiceActors", IncludeVoiceActors.ToString() }
				});
		}

		public void GenerateActorFiles(string directoryPath, ExportFileType fileType)
		{
			if (!IncludeVoiceActors)
				return;

			switch (fileType)
			{
				case ExportFileType.TabSeparated:
					foreach (var actor in Project.VoiceActorList.AllActors.Where(a => Project.CharacterGroupList.HasVoiceActorAssigned(a.Id)))
						GenerateTabSeparatedFile(Path.Combine(directoryPath, actor.Name), GetExportData().Where(t => t.Item1 == actor.Id).Select(t => t.Item3));
					break;
				default:
					foreach (var actor in Project.VoiceActorList.AllActors.Where(a => Project.CharacterGroupList.HasVoiceActorAssigned(a.Id)))
						GenerateExcelFile(Path.Combine(directoryPath, actor.Name), GetExportData().Where(t => t.Item1 == actor.Id).Select(t => t.Item3));
					break;
			}
		}

		public void GenerateBookFiles(string directoryPath, ExportFileType fileType)
		{
			switch (fileType)
			{
				case ExportFileType.TabSeparated:
					foreach (var book in Project.IncludedBooks)
						GenerateTabSeparatedFile(Path.Combine(directoryPath, book.BookId), GetExportData().Where(t => t.Item2 == book.BookId).Select(t => t.Item3));
					break;
				default:
					foreach (var book in Project.IncludedBooks)
						GenerateExcelFile(Path.Combine(directoryPath, book.BookId), GetExportData().Where(t => t.Item2 == book.BookId).Select(t => t.Item3));
					break;
			}
		}

		private void GenerateTabSeparatedFile(string path, IEnumerable<List<object>> data)
		{
			if (Path.GetExtension(path) != kTabDelimitedFileExtension)
				path += kTabDelimitedFileExtension;

			using (var stream = new StreamWriter(path, false, Encoding.UTF8))
			{
				stream.WriteLine(GetTabSeparatedLine(GetHeaders()));
				foreach (var line in data)
					stream.WriteLine(GetTabSeparatedLine(line));
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

				// this is the last column, no need to increment columNum
				sheet.Column(columnNum).AutoFit(2d, sheet.DefaultColWidth); // block length

				sheet.View.FreezePanes(2, 1);

				xls.Save();
			}
		}

		// internal for testing
		internal List<Tuple<int, string, List<object>>> GetExportData()
		{
			if (m_data == null)
			{
				int blockNumber = 1;
				var data = new List<Tuple<int, string, List<object>>>();

				foreach (var book in Project.IncludedBooks)
				{
					string singleVoiceNarratorOverride = null;
					if (book.SingleVoice)
						singleVoiceNarratorOverride = CharacterVerseData.GetStandardCharacterId(book.BookId, CharacterVerseData.StandardCharacter.Narrator);
					foreach (var block in book.GetScriptBlocks(true))
					{
						if (block.IsChapterAnnouncement && block.ChapterNumber == 1)
						{
							if (Project.SkipChapterAnnouncementForFirstChapter)
								continue;
							if (Project.SkipChapterAnnouncementForSingleChapterBooks && Project.Versification.LastChapter(BCVRef.BookToNumber(book.BookId)) == 1)
								continue;
						}
						if (IncludeVoiceActors)
						{
							VoiceActor.VoiceActor voiceActor = Project.GetVoiceActorForCharacter(singleVoiceNarratorOverride ?? block.CharacterIdInScript) ?? GetDummyActor();
							data.Add(GetExportDataForBlock(block, blockNumber++, book.BookId, voiceActor, singleVoiceNarratorOverride, IncludeVoiceActors));
						}
						else
							data.Add(GetExportDataForBlock(block, blockNumber++, book.BookId, null, singleVoiceNarratorOverride, IncludeVoiceActors));
					}
				}
				m_data = data;
			}
			return m_data;
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
			headers.Add("Size");
			return headers;
		}

		internal static Tuple<int, string, List<object>> GetExportDataForBlock(Block block, int blockNumber, string bookId, VoiceActor.VoiceActor voiceActor = null, string singleVoiceNarratorOverride = null, bool useCharacterIdInScript = true)
		{
			// NOTE: if the order here changes, there may be changes needed in GenerateExcelFile
			List<object> list = new List<object>();
			list.Add(blockNumber);
			if (voiceActor != null)
				list.Add(voiceActor.Name);
			list.Add(block.StyleTag);
			list.Add(bookId);
			list.Add(block.ChapterNumber);
			list.Add(block.InitialStartVerseNumber);
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
			list.Add(block.GetText(false).Length);
			return new Tuple<int, string, List<object>>(voiceActor == null ? -1 : voiceActor.Id, bookId, list);
		}

		internal static string GetTabSeparatedLine(List<object> items)
		{
			return string.Join(Separator, items);
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
				const int maxHeight = 65;

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
						sheet.Row(i).Height = maxHeight;

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
				group.Add(characterGroup.GroupId);
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
