using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen;
using Glyssen.Character;
using OfficeOpenXml;
using SIL.Scripture;

namespace DevTools
{
	class ReferenceTextUtility
	{
		public static void Go()
		{
			var existing = ReferenceText.English;
			var pathToFCBHDirGuide = @"C:\Users\Polk\Documents\Protoscript Generator\DGNTEnglishSimplified.xlsx";

			using (var xls = new ExcelPackage(new FileInfo(pathToFCBHDirGuide)))
			{
				var worksheet = xls.Workbook.Worksheets["Sheet1"];

				//Cells only contains references to cells with actual data
				var cells = worksheet.Cells;
				List<ReferenceTextRow> list = cells.GroupBy(c => c.Start.Row).Select(r =>
				{
					var row = r.Key;
					var verseStr = cells[row, 3].Value as string ?? ((double)cells[row, 3].Value).ToString();
					return new ReferenceTextRow(
						(string)cells[row, 1].Value,
						((double)cells[row, 2].Value).ToString(),
						verseStr,
						(string)cells[row, 4].Value,
						(string)cells[row, 5].Value);
				}).ToList();

				string prevBook = null;
				int iBook = 0;
				int iBlock = 0;
				BookScript existingBook = null;
				int startBook = BCVRef.BookToNumber("REV");
				List<BookScript> refBooks = existing.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= startBook).ToList();
				foreach (var referenceTextRow in list.Where(r => BCVRef.BookToNumber(r.Book) >= startBook))
				{
					if (prevBook != referenceTextRow.Book)
					{
						existingBook = refBooks[iBook++];
						iBlock = 0;
					}
					var block = existingBook.GetScriptBlocks(false)[iBlock++];

					while (CharacterVerseData.IsCharacterStandard(block.CharacterId, false))
					{
//						block = existingBook.Blocks[iBlock++];
						block = existingBook.GetScriptBlocks(false)[iBlock++];
					}

					if (referenceTextRow.Verse != "<<")
					{
						var newText = referenceTextRow.Text;
						var filteredText = Regex.Replace(newText, "[\u007c][\u007c][\u007c].*?[\u007c][\u007c][\u007c]", "");
						filteredText = Regex.Replace(filteredText, "{.*?}", "");
						filteredText = Regex.Replace(filteredText, "\r?\n", "");
//						filteredText = Regex.Replace(filteredText, "\"([A-Z])", "“$1");
//						filteredText = Regex.Replace(filteredText, "([\\w\\.\\?\\!])\"", "$1”");
						filteredText = Regex.Replace(filteredText, "  ", " ");

//						var blockText = Regex.Replace(block.GetText(false), "’", "'").Trim();
						var blockText = block.GetText(false);
						if (Compare(filteredText, blockText))
							Debug.WriteLine(referenceTextRow);
						else
							Debug.WriteLine(referenceTextRow);
					}
					else
						iBlock--;
					prevBook = referenceTextRow.Book;
				}
			}
		}

		private static bool Compare(string str1, string str2)
		{
			return Regex.Replace(str1, "[’‘'“”\"]", "").Trim() == Regex.Replace(Regex.Replace(str2, "  ", " "), "[’‘'“”\"]", "").Trim();
		}
	}

	class ReferenceTextRow
	{
		public ReferenceTextRow(string book, string chapter, string verse, string characterId, string text)
		{
			Book = book;
			Chapter = chapter;
			Verse = verse;
			CharacterId = characterId;
			Text = text;
		}

		public string Book { get; set; }
		public string Chapter { get; set; }
		public string Verse { get; set; }
		public string CharacterId { get; set; }
		public string Text { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4}", Book, Chapter, Verse, CharacterId, Text);
		}
	}
}
