using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen;
using Glyssen.Character;
using OfficeOpenXml;
using SIL.IO;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.Windows.Forms.FolderBrowserControl;
using SIL.Xml;

namespace DevTools
{
	static class ReferenceTextUtility
	{
		private const string kOutputDir = @"C:\projects\_Glyssen\DistFiles\reference_texts";

		private static readonly Dictionary<string, string> s_allLanguages = new Dictionary<string, string>
		{
			{"English", "NewEnglish"},
			{"Azeri", "Azeri"},
			{"French", "French"},
			{"Indonesian", "Indonesian"},
			{"Portuguese", "Portuguese"},
			{"Russian", "Russian"},
			{"Spanish", "Spanish"},
			{"TokPisin", "TokPisin"},
		};

		private static readonly ReferenceText s_existingEnglish;

		static ReferenceTextUtility()
		{
			s_existingEnglish = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
		}

		public static bool Go()
		{
			//var pathToFCBHDirGuide = @"C:\Users\Polk\Documents\Protoscript Generator\DGNTEnglishSimplified.xlsx";
			var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var pathToFCBHDirGuide = Path.Combine(myDocuments, @"Protoscript Generator\DGNTAllSimplified.xlsx");

			if (!File.Exists(pathToFCBHDirGuide))
			{
				Console.WriteLine("File does not exist: " + pathToFCBHDirGuide);
				return true;
			}

			List<ReferenceTextRow> list;
			using (var xls = new ExcelPackage(new FileInfo(pathToFCBHDirGuide)))
			{
				var worksheet = xls.Workbook.Worksheets["Sheet1"];

				//Cells only contains references to cells with actual data
				var cells = worksheet.Cells;
				list = cells.GroupBy(c => c.Start.Row).Select(r =>
				{
					var row = r.Key;
					var verseStr = cells[row, 3].Value as string ?? ((double) cells[row, 3].Value).ToString();
					return new ReferenceTextRow(
						(string) cells[row, 1].Value,
						((double) cells[row, 2].Value).ToString(),
						verseStr,
						(string) cells[row, 4].Value,
						(string) cells[row, 5].Value,
						(string) cells[row, 6].Value,
						(string) cells[row, 7].Value,
						(string) cells[row, 8].Value,
						(string) cells[row, 9].Value,
						(string) cells[row, 10].Value,
						(string) cells[row, 11].Value,
						(string) cells[row, 12].Value);
				}).ToList();
			}

			foreach (var language in s_allLanguages)
			{
				string languageOutputDir = Path.Combine(kOutputDir, language.Value);
				Directory.CreateDirectory(languageOutputDir);

				string prevBook = null;
				int iBook = 0;
				int iBlock = 0;
				BookScript existingBook = null;
				int startBook = BCVRef.BookToNumber("MAT");
				List<BookScript> refBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= startBook).ToList();
				List<BookScript> newBooks = new List<BookScript>();
				List<Block> newBlocks = new List<Block>();
				foreach (var referenceTextRow in list.Where(r => BCVRef.BookToNumber(r.Book) >= startBook))
				{
					if (prevBook != referenceTextRow.Book)
					{
						if (existingBook != null)
						{
							newBooks.Add(new BookScript(existingBook.BookId, newBlocks) {PageHeader = existingBook.PageHeader});
							newBlocks.Clear();
						}
						existingBook = refBooks[iBook++];
						iBlock = 0;

						var newBlock = new Block("mt")
						{
							CharacterId =
								CharacterVerseData.GetStandardCharacterId(existingBook.BookId,
									CharacterVerseData.StandardCharacter.BookOrChapter),
						};
						newBlock.BlockElements.Add(new ScriptText(existingBook.PageHeader));
						newBlocks.Add(newBlock);
					}
					var block = existingBook.GetScriptBlocks(false)[iBlock++];

					while (CharacterVerseData.IsCharacterStandard(block.CharacterId, false))
					{
						block = existingBook.GetScriptBlocks(false)[iBlock++];
					}

					if (referenceTextRow.Verse == "<<")
					{
						int chapter = int.Parse(referenceTextRow.Chapter);
						var newBlock = new Block("c", chapter)
						{
							CharacterId =
								CharacterVerseData.GetStandardCharacterId(existingBook.BookId,
									CharacterVerseData.StandardCharacter.BookOrChapter),
							IsParagraphStart = true,
							BookCode = existingBook.BookId
						};
						newBlock.BlockElements.Add(new ScriptText(referenceTextRow.Chapter));
						newBlocks.Add(newBlock);

						iBlock--;
					}
					else
					{
//						var newText = referenceTextRow.EnglishText;
						string newText = (string) ReflectionHelper.GetProperty(referenceTextRow, language.Key);
						var filteredText = Regex.Replace(newText, "[\u007c][\u007c][\u007c].*?[\u007c][\u007c][\u007c]", "");
						filteredText = Regex.Replace(filteredText, "{(\\d*?)} ", "[$1]\u00A0");
						filteredText = Regex.Replace(filteredText, "{.*?}", "");
						filteredText = Regex.Replace(filteredText, "\r?\n", "");
						//filteredText = Regex.Replace(filteredText, "(\\w)'(\\w)", "$1\u2019$2");
//					filteredText = Regex.Replace(filteredText, "\"([A-Z])", "“$1");
//					filteredText = Regex.Replace(filteredText, "([\\w\\.\\?\\!])\"", "$1”");
						filteredText = Regex.Replace(filteredText, "  ", " ");

						var blockText = block.GetText(true);
						if (true || Compare(filteredText, blockText))
						{
							Debug.WriteLine(referenceTextRow);
							var newBlock = new Block(block.StyleTag, int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse))
							{
								CharacterId = block.CharacterId,
								Delivery = block.Delivery,
								IsParagraphStart = block.IsParagraphStart,
								MultiBlockQuote = block.MultiBlockQuote
							};
							var splits = Regex.Split(filteredText, "(\\[\\d*?\\]\u00A0)");
							foreach (var split in splits)
							{
								if (string.IsNullOrWhiteSpace(split))
								{
									if (splits.Length == 1)
										newBlock.BlockElements.Add(new ScriptText(" "));
									continue;
								}
								var match = Regex.Match(split, "\\[(\\d*?)\\]\u00A0");
								if (match.Success)
									newBlock.BlockElements.Add(new Verse(match.Groups[1].Value));
								else
								{
									string text = split.TrimStart();
									newBlock.BlockElements.Add(new ScriptText(text));
								}
							}
							newBlocks.Add(newBlock);
						}
						else
							Debug.WriteLine(referenceTextRow);
					}
					prevBook = referenceTextRow.Book;
				}
				newBooks.Add(new BookScript(existingBook.BookId, newBlocks));

				foreach (var bookScript in newBooks)
				{
					XmlSerializationHelper.SerializeToFile(Path.Combine(languageOutputDir, bookScript.BookId + ".xml"), bookScript);
				}
			}
			return false;
		}

		private static bool Compare(string str1, string str2)
		{
			return Regex.Replace(str1, "[’‘'“”\"]", "").Trim() ==
					Regex.Replace(Regex.Replace(str2, "  ", " "), "[’‘'“”\"]", "").Trim();
		}

		public static bool LinkToEnglish()
		{
			bool errorOcurred = false;
			foreach (ReferenceTextType language in Enum.GetValues(typeof(ReferenceTextType)))
			{
				if (language == ReferenceTextType.English || language == ReferenceTextType.Custom)
					continue;

				var refText = ReferenceText.GetStandardReferenceText(language);

				//string languageOutputDir = Path.Combine(kOutputDir, language.Value);
				if (refText == null)
				{
					errorOcurred = true;
					Console.Error.WriteLine("No data available to create " + language + " reference text.");
					continue;
				}
				Console.WriteLine("Processing " + language + "...");
				Console.Write("   ");

				foreach (var book in refText.Books)
				{
					var referenceBook = s_existingEnglish.Books.SingleOrDefault(b => b.BookId == book.BookId);
					if (referenceBook == null)
					{
						errorOcurred = true;
						Console.Error.WriteLine("English reference text does not contain book: " + book.BookId + ".");
					}
					else
					{
						Console.Write(book.BookId + "...");

						Exception error = null;

						try
						{
							ReferenceText.ApplyTo(book, referenceBook.GetScriptBlocks(), s_existingEnglish.GetFormattedChapterAnnouncement, refText.Versification, s_existingEnglish.Versification, true);
							var bookXmlFile = FileLocator.GetFileDistributedWithApplication(ReferenceText.kDistFilesReferenceTextDirectoryName, language.ToString(), Path.ChangeExtension(book.BookId, "xml"));
							XmlSerializationHelper.SerializeToFile(bookXmlFile, book, out error);
						}
						catch (Exception e)
						{
							error = e;
						}
						if (error != null)
						{
							errorOcurred = true;
							Console.Error.WriteLine(error.Message);
						}
					}
				}
			}
			return errorOcurred;
		}
	}

	class ReferenceTextRow
	{
		public ReferenceTextRow(string book, string chapter, string verse, string characterId, string englishText,
			string azeri, string french, string indonesian, string portuguese, string russian, string spanish, string tokPisin)
		{
			Book = book;
			Chapter = chapter;
			Verse = verse;
			CharacterId = characterId;
			English = englishText;
			Azeri = azeri;
			French = french;
			Indonesian = indonesian;
			Portuguese = portuguese;
			Russian = russian;
			Spanish = spanish;
			TokPisin = tokPisin;
		}

		public string Book { get; set; }
		public string Chapter { get; set; }
		public string Verse { get; set; }
		public string CharacterId { get; set; }
		public string English { get; set; }
		public string Azeri { get; set; }
		public string French { get; set; }
		public string Indonesian { get; set; }
		public string Portuguese { get; set; }
		public string Russian { get; set; }
		public string Spanish { get; set; }
		public string TokPisin { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4}", Book, Chapter, Verse, CharacterId, English);
		}
	}
}
