using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen;
using Glyssen.Character;
using OfficeOpenXml;
using SIL.IO;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.Xml;

namespace DevTools
{
	static class ReferenceTextUtility
	{
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

		private class TitleAndChapterLabelInfo
		{
			public string Language { get; set; }
			public string TitleAndChapterOneInfoFromXls { get; set; }
			public string ChapterTwoInfoFromXls { get; set; }
			public string BookTitle { get; set; }
			public string ChapterLabel { get; set; }

		}

		private class BookTitleAndChapterLabelInfo
		{
			private readonly List<TitleAndChapterLabelInfo> m_details = new List<TitleAndChapterLabelInfo>(8);
			public string BookId { get; set; }
			public List<TitleAndChapterLabelInfo> Details
			{
				get { return m_details; }
			}
		}

		public static bool Go()
		{
			var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var pathToFCBHDirGuide = Path.Combine(myDocuments, "Protoscript Generator", "DGNTAllSimplified_71.xlsx");

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

			bool errorsOccurred = false;
			var resultSummary = new List<BookTitleAndChapterLabelInfo>(66); // Though all we have currently is the NT

			foreach (var language in s_allLanguages)
			{
				Console.WriteLine("Processing " + language + "...");

				const string kOutputDir = @"..\..\DistFiles\reference_texts";

				string languageOutputDir = Path.Combine(kOutputDir, language.Value);
				Directory.CreateDirectory(languageOutputDir);

				string prevBook = null;
				int iBook = 0;
				int iBlock = 0;
				BookScript existingBook = null;
				string chapterLabel = null;
				string chapterLabelForPrevBook = null;
				string justTheWordForChapter = null;
				int startBook = BCVRef.BookToNumber("MAT");
				List<BookScript> refBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= startBook).ToList();
				List<BookScript> newBooks = new List<BookScript>();
				List<Block> newBlocks = new List<Block>();
				TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo = null;
				foreach (var referenceTextRow in list.Where(r => BCVRef.BookToNumber(ConvertFcbhBookCodeToSilBookCode(r.Book)) >= startBook))
				{
					if (prevBook != referenceTextRow.Book)
					{
						if (existingBook != null)
						{
							newBooks.Add(new BookScript(existingBook.BookId, newBlocks) {PageHeader = chapterLabel});
							newBlocks.Clear();
						}
						existingBook = refBooks[iBook++];
						iBlock = 0;
						chapterLabelForPrevBook = chapterLabel;
						chapterLabel = null;

						var newBlock = new Block("mt")
						{
							CharacterId =
								CharacterVerseData.GetStandardCharacterId(existingBook.BookId,
									CharacterVerseData.StandardCharacter.BookOrChapter),
						};
						var bookTitleAndchapter1Announcement = (string)ReflectionHelper.GetProperty(referenceTextRow, language.Key);
						var summaryForBook = resultSummary.SingleOrDefault(b => b.BookId == existingBook.BookId);
						if (summaryForBook == null)
						{
							summaryForBook = new BookTitleAndChapterLabelInfo {BookId = existingBook.BookId};
							resultSummary.Add(summaryForBook);
						}
						currentTitleAndChapterLabelInfo = new TitleAndChapterLabelInfo
						{
							Language = language.Key,
							TitleAndChapterOneInfoFromXls = bookTitleAndchapter1Announcement
						};
						summaryForBook.Details.Add(currentTitleAndChapterLabelInfo);

						var bookName = bookTitleAndchapter1Announcement.TrimEnd(' ', '1').TrimStart(' ');

						if (!JustGetHalfOfRepeated(ref bookName) && IsSingleChapterBook(referenceTextRow))
						{
							var iFirstSpace = bookTitleAndchapter1Announcement.IndexOf(" ", StringComparison.Ordinal);
							if (iFirstSpace > 0)
							{
								var firstWord = bookTitleAndchapter1Announcement.Substring(0, iFirstSpace);
								var iStartOfChapterAnnouncement = bookTitleAndchapter1Announcement.IndexOf(firstWord,
									iFirstSpace, StringComparison.Ordinal);
								if (iStartOfChapterAnnouncement > 0)
								{
									bookName = bookTitleAndchapter1Announcement.Substring(0, iStartOfChapterAnnouncement).TrimEnd();
									chapterLabel = bookTitleAndchapter1Announcement.Substring(iStartOfChapterAnnouncement);
								}
							}
							if (chapterLabel == null)
							{
								if (justTheWordForChapter != null)
								{
									chapterLabel = bookName + " " + justTheWordForChapter;
									//Console.WriteLine("Guessing at chapter label: " + chapterLabel);
								}
								else if (IsSingleChapterBook(referenceTextRow))
								{
									chapterLabel = bookName;
								}
								else
								{
									Console.WriteLine("Could not distinguish book title and chapter label: " + bookTitleAndchapter1Announcement);
									errorsOccurred = true;
								}
							}
							currentTitleAndChapterLabelInfo.ChapterLabel = chapterLabel;
						}
						newBlock.BlockElements.Add(new ScriptText(bookName));
						currentTitleAndChapterLabelInfo.BookTitle = bookName;
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
						if (chapter ==  2)
						{
							currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls = (string) ReflectionHelper.GetProperty(referenceTextRow, language.Key);
							var chapterLabelForCurrentBook = currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls.TrimEnd(' ', '2');
							if (justTheWordForChapter == null && chapterLabelForPrevBook != null && iBook == 2)
							{
								// We're going to try to find just the word for chapter in case we later hit a single-chapter book that doesn't have it.
								int istartOfWord = chapterLabelForPrevBook.Length;
								int i = istartOfWord;
								int j = chapterLabelForCurrentBook.Length;
								while (--i > 0 && --j > 0)
								{
									if (chapterLabelForPrevBook[i] != chapterLabelForCurrentBook[j])
										break;
									if (chapterLabelForPrevBook[i] == ' ')
										istartOfWord = i + 1;
								}
								if (istartOfWord > 0 && istartOfWord < chapterLabelForPrevBook.Length - 2)
									justTheWordForChapter = chapterLabelForPrevBook.Substring(istartOfWord);
							}
							
							if (justTheWordForChapter != null)
								JustGetHalfOfRepeated(ref chapterLabelForCurrentBook, justTheWordForChapter);
							
							chapterLabel = chapterLabelForCurrentBook;

							var mainTitleElement = (ScriptText) newBlocks.First().BlockElements[0];
							var bookName = mainTitleElement.Content;
							int startOfChapterLabel = bookName.LastIndexOf(chapterLabel, StringComparison.Ordinal);
							if (startOfChapterLabel == -1)
							{
								if (chapterLabel.StartsWith("1 "))
								{
									var sb = new StringBuilder(chapterLabel);
									sb[0] = 'I';
									startOfChapterLabel = bookName.LastIndexOf(sb.ToString(), StringComparison.Ordinal);
									if (startOfChapterLabel == -1)
									{
										sb.Remove(0, 2);
										startOfChapterLabel = bookName.LastIndexOf(sb.ToString(), StringComparison.Ordinal);
									}
								}
								else if (chapterLabel.StartsWith("2 "))
								{
									var sb = new StringBuilder(chapterLabel);
									sb.Insert(1, "nd");
									startOfChapterLabel = bookName.LastIndexOf(sb.ToString(), StringComparison.Ordinal);
									if (startOfChapterLabel == -1)
									{
										sb = new StringBuilder(chapterLabel);
										sb[0] = 'I';
										sb.Insert(1, "I");
										startOfChapterLabel = bookName.LastIndexOf(sb.ToString(), StringComparison.Ordinal);
									}
								}
							}
							if (startOfChapterLabel > 0)
							{
								bookName = bookName.Substring(0, startOfChapterLabel).Trim();
								mainTitleElement.Content = bookName;
							}
							else
							{
								if (justTheWordForChapter != null)
								{
									if (bookName.StartsWith(chapterLabel, StringComparison.Ordinal) && chapterLabel != null &&
										chapterLabel.Contains(justTheWordForChapter))
									{
										bookName = chapterLabel.Substring(0, chapterLabel.IndexOf(justTheWordForChapter, StringComparison.Ordinal)).TrimEnd();
										mainTitleElement.Content = bookName;
									}
									else
									{
										chapterLabel = bookName + " " + justTheWordForChapter;
										//Console.WriteLine("Book title being left as \"" + bookName + "\" and chapter label set to: " + chapterLabel);
									}
								}
								else if (bookName != chapterLabel)
								{
									Console.WriteLine("Could not figure out book title: " + bookName);
									errorsOccurred = true;
								}
							}

							currentTitleAndChapterLabelInfo.BookTitle = bookName;
							currentTitleAndChapterLabelInfo.ChapterLabel = chapterLabel;
						}
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
						string newText = (string) ReflectionHelper.GetProperty(referenceTextRow, language.Key);
						var filteredText = Regex.Replace(newText, " [|][|][|].*?[|][|][|] ", "");
						filteredText = Regex.Replace(filteredText, "{(\\d*?)} ", "[$1]\u00A0");
						//var filteredText = Regex.Replace(newText, "{(\\d*?)} ", "[$1]\u00A0");
						//filteredText = Regex.Replace(filteredText, "\r?\n", "");
						//filteredText = Regex.Replace(filteredText, "(\\w)'(\\w)", "$1\u2019$2");
//					filteredText = Regex.Replace(filteredText, "\"([A-Z])", "“$1");
//					filteredText = Regex.Replace(filteredText, "([\\w\\.\\?\\!])\"", "$1”");
						filteredText = Regex.Replace(filteredText, "  ", " ");
						var filteredTextWithoutInlineAnnotations = Regex.Replace(filteredText, "{.*?}", "");
						filteredTextWithoutInlineAnnotations = Regex.Replace(filteredTextWithoutInlineAnnotations, "  ", " ");

						var blockText = block.GetText(true);
						if (true || Compare(filteredTextWithoutInlineAnnotations, blockText))
						{
							//Debug.WriteLine(referenceTextRow);
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
										Debug.Fail("");//newBlock.BlockElements.Add(new ScriptText(" ")));
									continue;
								}
								var match = Regex.Match(split, "\\[(\\d*?)\\]\u00A0");
								if (match.Success)
									newBlock.BlockElements.Add(new Verse(match.Groups[1].Value));
								else
								{
									var splits2 = Regex.Split(split, "({.*?})");
									foreach (var s in splits2)
									{
										if (string.IsNullOrWhiteSpace(s))
											continue;
										var match2 = Regex.Match(s, "{.*?}");
										if (match2.Success)
										{
											if (splits2.IndexOf(s) == 0 || splits2.IndexOf(s) == splits2.Length - 1)
												continue;
											ScriptAnnotation annotation;
											if (AnnotationExtractor.ConvertTextToScriptAnnotationElement(s, out annotation))
											{
												newBlock.BlockElements.Add(annotation);
												Debug.WriteLine(newBlock.ToString(true, existingBook.BookId) + " (" + annotation.ToDisplay + ")");
											}
										}
										else
										{
											string text = s.TrimStart();
											if (string.IsNullOrWhiteSpace(text))
												Debug.Fail("");
											newBlock.BlockElements.Add(new ScriptText(text));
										}
									}
								}
							}
							//if (!newBlock.BlockElements.Any())
							//	Debug.Fail("");
							newBlocks.Add(newBlock);
						}
						else
							Debug.WriteLine(referenceTextRow);
					}
					prevBook = referenceTextRow.Book;
				}
				newBooks.Add(new BookScript(existingBook.BookId, newBlocks) { PageHeader = chapterLabel });

				foreach (var bookScript in newBooks)
				{
					XmlSerializationHelper.SerializeToFile(Path.Combine(languageOutputDir, bookScript.BookId + ".xml"), bookScript);
				}
			}
			if (!errorsOccurred)
			{
					Console.WriteLine("Write book title and chapter label summary to file? Y/N");
					var answer = Console.ReadLine();
				if (answer == "Y" || answer == "y")
				{
					var path = Path.Combine(myDocuments, "Protoscript Generator", "book title and chapter label summary.txt");
					using (var w = new StreamWriter(path))
					{
						foreach (var info in resultSummary)
						{
							w.WriteLine(info.BookId);
							w.WriteLine(String.Join("\t", info.Details.Select(d => d.Language)));
							w.WriteLine(String.Join("\t", info.Details.Select(d => d.TitleAndChapterOneInfoFromXls)));
							w.WriteLine(String.Join("\t", info.Details.Select(d => d.BookTitle)));
							if (info.Details.Any(d => d.ChapterTwoInfoFromXls != null))
								w.WriteLine(String.Join("\t", info.Details.Select(d => d.ChapterTwoInfoFromXls)));
							w.WriteLine(String.Join("\t", info.Details.Select(d => d.ChapterLabel)));
						}
					}
					Process.Start(path);
				}

				return LinkToEnglish();
			}
			return errorsOccurred;
		}

		private static bool IsSingleChapterBook(ReferenceTextRow referenceTextRow)
		{
			return s_existingEnglish.Versification.LastChapter(BCVRef.BookToNumber(referenceTextRow.Book)) == 1;
		}

		private static bool JustGetHalfOfRepeated(ref string stringWithPossibleReduplication, string suffix = null)
		{
			string temp;
			if (suffix == null)
				temp = stringWithPossibleReduplication;
			else
			{
				int iWordForChapter = stringWithPossibleReduplication.IndexOf(suffix, StringComparison.Ordinal);
				if (iWordForChapter > 0)
					temp = stringWithPossibleReduplication.Substring(0, iWordForChapter).TrimEnd();
				else
					return false;
			}
			if (temp.Length % 2 == 1)
			{
				if (temp.Substring(0, temp.Length / 2) == temp.Substring(1 + temp.Length / 2))
				{
					stringWithPossibleReduplication = temp.Substring(1 + temp.Length / 2);
					if (suffix != null)
						stringWithPossibleReduplication += " " + suffix;
					return true;
				}
			}
			return false;
		}

		private static string ConvertFcbhBookCodeToSilBookCode(string bookCode)
		{
			switch (bookCode)
			{
				case "TTS":
					return "TIT";
				case "JMS":
					return "JAS";
				default:
					return bookCode;
			}
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
