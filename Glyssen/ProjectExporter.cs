using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopAnalytics;
using Glyssen.Character;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
					foreach (var actor in Project.VoiceActorList.Actors.Where(a => Project.CharacterGroupList.HasVoiceActorAssigned(a.Id)))
						GenerateTabSeparatedFile(Path.Combine(directoryPath, actor.Name), GetExportData().Where(t => t.Item1 == actor.Id).Select(t => t.Item3));
					break;
				default:
					foreach (var actor in Project.VoiceActorList.Actors.Where(a => Project.CharacterGroupList.HasVoiceActorAssigned(a.Id)))
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
				stream.WriteLine(GetHeaders());
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
				sheet.Column(1).AutoFit(2d, sheet.DefaultColWidth); // line number
				int offset = 0;
				if (IncludeVoiceActors)
				{
					offset = 1;
					sheet.Column(2).AutoFit(2d, 20d); // voice actor
				}
				sheet.Column(2 + offset).AutoFit(2d, sheet.DefaultColWidth); // style tag
				sheet.Column(3 + offset).AutoFit(2d, sheet.DefaultColWidth); // book
				sheet.Column(4 + offset).AutoFit(2d, sheet.DefaultColWidth); // chapter
				sheet.Column(5 + offset).AutoFit(2d, sheet.DefaultColWidth); // verse
				sheet.Column(6 + offset).AutoFit(2d, 20d); // character ID
				sheet.Column(8 + offset).Style.WrapText = true; // script text
				sheet.Column(8 + offset).Width = 50d;
				sheet.Column(9 + offset).AutoFit(2d, sheet.DefaultColWidth); // block length

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
			headers.Add("Character");
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
	}
}
