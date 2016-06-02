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
		private const string kOutputFileForAnnotations = @"..\..\Glyssen\Resources\Annotations.txt";

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

		// When running with this true, I have simply been putting a breakpoint on 'return false'
		// statement in CompareIgnoringQuoteMarkDifferences and looking at each case
		// (this is not a const to prevent compiler warnings)
		private static readonly bool s_onlyRunToFindDifferencesBetweenCurrentEnglishAndExcelSpreadsheetEnglish = false;

		private static readonly bool s_onlyCreateCharacterMapping = false;

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
			public List<TitleAndChapterLabelInfo> Details { get { return m_details; } }
		}

		private class CharacterMapping
		{
			public CharacterMapping(string glyssenId, string fcbhId, BCVRef verse)
			{
				GlyssenId = glyssenId;
				FcbhId = fcbhId;
				Verse = verse;
			}

			private string GlyssenId { get; set; }
			private string FcbhId { get; set; }
			private BCVRef Verse { get; set; }

			public override string ToString()
			{
				return string.Format("{0}\t{1}\t{2}", Verse.AsString, GlyssenId, FcbhId);
			}
		}

		public static bool GenerateReferenceTexts()
		{
			var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var pathToFCBHDirGuide = Path.Combine(myDocuments, "Protoscript Generator", "DGNTAllSimplified_71.xlsx");
			var characterMappings = new List<CharacterMapping>();
			var glyssenToFcbhIds = new SortedDictionary<string, SortedSet<string>>();
			var fcbhToGlyssenIds = new SortedDictionary<string, SortedSet<string>>();

			if (!File.Exists(pathToFCBHDirGuide))
			{
				Console.WriteLine("File does not exist: " + pathToFCBHDirGuide);
				return false;
			}

			List<ReferenceTextRow> referenceTextRowsFromExcelSpreadsheet;
			using (var xls = new ExcelPackage(new FileInfo(pathToFCBHDirGuide)))
			{
				var worksheet = xls.Workbook.Worksheets["Sheet1"];

				//Cells only contains references to cells with actual data
				var cells = worksheet.Cells;
				referenceTextRowsFromExcelSpreadsheet = cells.GroupBy(c => c.Start.Row).Select(r =>
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

			var annotationsToOutput = new List<string>();
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
				foreach (var referenceTextRow in referenceTextRowsFromExcelSpreadsheet.Where(r => BCVRef.BookToNumber(ConvertFcbhBookCodeToSilBookCode(r.Book)) >= startBook))
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
								else
								{
									var iLastSpace = bookName.LastIndexOf(' ');
									if (iLastSpace > 0)
									{
										var lastWord = bookName.Substring(iLastSpace + 1);
										if (bookName.StartsWith(lastWord, StringComparison.Ordinal))
										{
											chapterLabel = lastWord;
											bookName = bookName.Substring(0, iLastSpace);
										}
									}
									if (chapterLabel == null)
										chapterLabel = bookName;
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
						if (s_onlyCreateCharacterMapping)
						{
							if (!CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator) || !referenceTextRow.CharacterId.StartsWith("Narr_0"))
							{
								var verse = new BCVRef(BCVRef.BookToNumber(existingBook.BookId), int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse));
								characterMappings.Add(new CharacterMapping(block.CharacterId, referenceTextRow.CharacterId, verse));

								SortedSet<string> fcbhIds;
								if (glyssenToFcbhIds.TryGetValue(block.CharacterId, out fcbhIds))
									fcbhIds.Add(referenceTextRow.CharacterId);
								else
									glyssenToFcbhIds.Add(block.CharacterId, new SortedSet<string> { referenceTextRow.CharacterId });

								SortedSet<string> glyssenIds;
								if (fcbhToGlyssenIds.TryGetValue(referenceTextRow.CharacterId, out glyssenIds))
									glyssenIds.Add(block.CharacterId);
								else
									fcbhToGlyssenIds.Add(referenceTextRow.CharacterId, new SortedSet<string> { block.CharacterId });
							}
							continue;
						}

						string originalText = (string) ReflectionHelper.GetProperty(referenceTextRow, language.Key);
						var modifiedText = Regex.Replace(originalText, "{(\\d*?)} ?", "[$1]\u00A0");
						var modifiedTextWithoutAnnotations = Regex.Replace(modifiedText, " \\|\\|\\|.*?\\|\\|\\| ", "");
						modifiedTextWithoutAnnotations = Regex.Replace(modifiedTextWithoutAnnotations, "{.*?}", "");
						modifiedTextWithoutAnnotations = Regex.Replace(modifiedTextWithoutAnnotations, "  ", " ");

						var blockText = block.GetText(true);
						if (!s_onlyRunToFindDifferencesBetweenCurrentEnglishAndExcelSpreadsheetEnglish ||
							CompareIgnoringQuoteMarkDifferences(modifiedTextWithoutAnnotations, blockText))
						{
							//Debug.WriteLine(referenceTextRow);
							var newBlock = new Block(block.StyleTag, int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse))
							{
								CharacterId = block.CharacterId,
								Delivery = block.Delivery,
								IsParagraphStart = block.IsParagraphStart,
								MultiBlockQuote = block.MultiBlockQuote
							};
							BlockElement lastElementInBlock = null;
							var splits = Regex.Split(modifiedText, "(\\[\\d*?\\]\u00A0)");
							foreach (var split in splits)
							{
								if (string.IsNullOrWhiteSpace(split))
								{
									if (splits.Length == 1)
										Debug.Fail("");
									continue;
								}
								var match = Regex.Match(split, "\\[(\\d*?)\\]\u00A0");
								if (match.Success)
									newBlock.BlockElements.Add(lastElementInBlock = new Verse(match.Groups[1].Value));
								else
								{
									var splits2 = Regex.Split(split, "( \\|\\|\\| DO NOT COMBINE \\|\\|\\| {.*?}|{.*?}| \\|\\|\\|.*?\\|\\|\\| )");
									foreach (var s in splits2)
									{
										if (string.IsNullOrWhiteSpace(s))
											continue;
										var match2 = Regex.Match(s, " \\|\\|\\| DO NOT COMBINE \\|\\|\\| {.*?}|{.*?}| \\|\\|\\|.*?\\|\\|\\| ");
										if (match2.Success)
										{
											ScriptAnnotation annotation;
											if (ConvertTextToUserSpecifiedScriptAnnotationElement(s, out annotation))
											{
												newBlock.BlockElements.Add(lastElementInBlock = annotation);
												//Debug.WriteLine(newBlock.ToString(true, existingBook.BookId) + " (" + annotation.ToDisplay + ")");
											}
											else if (ConvertTextToControlScriptAnnotationElement(s, out annotation))
											{
												if (language.Key == "English")
												{
													string serializedAnnotation;
													if (annotation is Pause)
														serializedAnnotation = XmlSerializationHelper.SerializeToString((Pause)annotation, true);
													else
														serializedAnnotation = XmlSerializationHelper.SerializeToString((Sound)annotation, true);

													if (string.IsNullOrWhiteSpace(annotation.ToDisplay) || string.IsNullOrWhiteSpace(serializedAnnotation))
													{
														Console.WriteLine("Annotation not formatted correctly (is null or whitespace): {0}", referenceTextRow.English);
														Console.WriteLine();
														errorsOccurred = true;
													}
													var trimmedEnglish = referenceTextRow.English.TrimEnd();
													if ((annotation is Pause && !trimmedEnglish.EndsWith(annotation.ToDisplay)) ||
														(annotation is Sound && !trimmedEnglish.StartsWith(annotation.ToDisplay)))
													{
														var bcv = new BCVRef(BCVRef.BookToNumber(existingBook.BookId), block.ChapterNumber, block.InitialStartVerseNumber);
														Console.WriteLine("(warning) Annotation not formatted the same as FCBH: ({0}) {1} => {2}", bcv.AsString, referenceTextRow.English, annotation.ToDisplay);
														Console.WriteLine();
														// This is a good check to run for sanity. But we can't fail as
														// a few of the annotations are actually displayed slightly differently by FCBH
														// (due to what we are assuming are insignificant differences like 'before' vs. '@')
														//errorsOccurred = true;
													}
													int offset = 0;
													if ((existingBook.BookId == "MRK" && block.ChapterNumber == 4 && block.InitialVerseNumberOrBridge == "39") ||
														(existingBook.BookId == "ACT" && block.ChapterNumber == 10 && block.InitialVerseNumberOrBridge == "23"))
													{
														offset = -1;
													}
													annotationsToOutput.Add(existingBook.BookId + "\t" + block.ChapterNumber + "\t" +
														block.InitialVerseNumberOrBridge + "\t" + offset + "\t" + serializedAnnotation);
												}
											}
											else
											{
												Console.WriteLine("Could not parse annotation: " + referenceTextRow);
												errorsOccurred = true;
											}
										}
										else
										{
											string text = s.TrimStart();
											if (string.IsNullOrWhiteSpace(text))
												Debug.Fail("");
											newBlock.BlockElements.Add(lastElementInBlock = new ScriptText(text));
										}
									}
								}
							}
							if (lastElementInBlock is Verse)
								newBlock.BlockElements.Add(new ScriptText("…"));
							newBlocks.Add(newBlock);
						}
					}
					prevBook = referenceTextRow.Book;
				}

				if (s_onlyCreateCharacterMapping)
				{
					WriteCharacterMappingFile(characterMappings, glyssenToFcbhIds, fcbhToGlyssenIds);
					return true;
				}

				if (s_onlyRunToFindDifferencesBetweenCurrentEnglishAndExcelSpreadsheetEnglish)
					return true;
				newBooks.Add(new BookScript(existingBook.BookId, newBlocks) { PageHeader = chapterLabel });

				foreach (var bookScript in newBooks)
				{
					XmlSerializationHelper.SerializeToFile(Path.Combine(languageOutputDir, bookScript.BookId + ".xml"), bookScript);
				}
			}

			WriteAnnotationsFile(annotationsToOutput);

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
			return !errorsOccurred;
		}

		private static void WriteCharacterMappingFile(List<CharacterMapping> characterMappings, SortedDictionary<string, SortedSet<string>> glyssenToFcbhIds, SortedDictionary<string, SortedSet<string>> fcbhToGlyssenIds)
		{
			const string kOutputDirForCharacterMapping = @"..\..\DevTools\Resources\temporary";
			const string kOutputFileForCharacterMapping = @"CharacterMappingToFcbh.txt";
			const string kOutputFileForGlyssenToFcbhMultiMap = @"GlyssenToFcbhMultiMap.txt";
			const string kOutputFileForFcbhToGlyssenMultiMap = @"FcbhToGlyssenMultiMap.txt";

			Directory.CreateDirectory(kOutputDirForCharacterMapping);
			var sb = new StringBuilder();
			foreach (CharacterMapping characterMapping in characterMappings)
				sb.Append(characterMapping).Append(Environment.NewLine);
			File.WriteAllText(Path.Combine(kOutputDirForCharacterMapping, kOutputFileForCharacterMapping), sb.ToString());

			sb.Clear();
			foreach (var glyssenToFcbhIdsEntry in glyssenToFcbhIds)
				if (glyssenToFcbhIdsEntry.Value.Count > 1 ||
					CharacterVerseData.IsCharacterOfType(glyssenToFcbhIdsEntry.Key, CharacterVerseData.StandardCharacter.Narrator) ||
					glyssenToFcbhIdsEntry.Value.Any(c => c.StartsWith("Narr_0")))
				{
					sb.Append(string.Format("{0}\t{1}", glyssenToFcbhIdsEntry.Key, glyssenToFcbhIdsEntry.Value.TabSeparated())).Append(Environment.NewLine);
				}
			File.WriteAllText(Path.Combine(kOutputDirForCharacterMapping, kOutputFileForGlyssenToFcbhMultiMap), sb.ToString());

			sb.Clear();
			foreach (var fcbhToGlyssenIdsEntry in fcbhToGlyssenIds)
				if (fcbhToGlyssenIdsEntry.Value.Count > 1 ||
					CharacterVerseData.IsCharacterOfType(fcbhToGlyssenIdsEntry.Key, CharacterVerseData.StandardCharacter.Narrator) ||
					fcbhToGlyssenIdsEntry.Value.Any(c => c.StartsWith("Narr_0")))
					sb.Append(string.Format("{0}\t{1}", fcbhToGlyssenIdsEntry.Key, fcbhToGlyssenIdsEntry.Value.TabSeparated())).Append(Environment.NewLine);
			File.WriteAllText(Path.Combine(kOutputDirForCharacterMapping, kOutputFileForFcbhToGlyssenMultiMap), sb.ToString());
		}

		private static void WriteAnnotationsFile(List<string> annotationsToOutput)
		{
			var sb = new StringBuilder();
			foreach (string annotation in annotationsToOutput)
				sb.Append(annotation).Append(Environment.NewLine);
			File.WriteAllText(kOutputFileForAnnotations, sb.ToString());
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

		private static bool CompareIgnoringQuoteMarkDifferences(string str1, string str2)
		{
			if (Regex.Replace(str1, "[“”\"]", "").Trim() == Regex.Replace(str2, "[“”\"]", "").Trim())
				return true;
			// When onlyRunToFindDifferencesBetweenCurrentEnglishAndExcelSpreadsheetEnglish is true, put a breakpoint here to look at diffs
			return false;
		}

		public static bool LinkToEnglish()
		{
			bool errorOccurred = false;
			foreach (ReferenceTextType language in Enum.GetValues(typeof(ReferenceTextType)))
			{
				if (language == ReferenceTextType.English || language == ReferenceTextType.Custom)
					continue;

				var refText = ReferenceText.GetStandardReferenceText(language);

				if (refText == null)
				{
					errorOccurred = true;
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
						errorOccurred = true;
						Console.Error.WriteLine("English reference text does not contain book: " + book.BookId + ".");
					}
					else
					{
						Console.Write(book.BookId + "...");

						Exception error = null;

						try
						{
							s_existingEnglish.ApplyTo(book, s_existingEnglish.Versification, true);
							var bookXmlFile = FileLocator.GetFileDistributedWithApplication(ReferenceText.kDistFilesReferenceTextDirectoryName, language.ToString(), Path.ChangeExtension(book.BookId, "xml"));
							XmlSerializationHelper.SerializeToFile(bookXmlFile, book, out error);
						}
						catch (Exception e)
						{
							error = e;
						}
						if (error != null)
						{
							errorOccurred = true;
							Console.Error.WriteLine(error.Message);
						}
					}
				}
			}
			return !errorOccurred;
		}

		private static readonly Regex s_userSfxRegex = new Regex("{F8 SFX ?-?- ?(.*)}", RegexOptions.Compiled);
		private static readonly Regex s_userMusicStartsRegex = new Regex("{F8 Music--Starts}", RegexOptions.Compiled);
		private static readonly Regex s_userMusicEndsRegex = new Regex("{F8 Music--Ends}", RegexOptions.Compiled);

		public static bool ConvertTextToUserSpecifiedScriptAnnotationElement(string text, out ScriptAnnotation annotation)
		{
			var match = s_userSfxRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Sfx, EffectName = match.Groups[1].Value, UserSpecifiesLocation = true };
				return true;
			}

			match = s_userMusicStartsRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = Sound.kNonSpecificStartOrStop };
				return true;
			}

			match = s_userMusicEndsRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true };
				return true;
			}

			annotation = null;
			return false;
		}

		private static readonly Regex s_doNotCombineRegex = new Regex(" \\|\\|\\| DO NOT COMBINE \\|\\|\\| ", RegexOptions.Compiled);
		private static readonly Regex s_pauseRegex = new Regex("\\|\\|\\| \\+ ([\\d\\.]*?) SECs \\|\\|\\|", RegexOptions.Compiled);
		private static readonly Regex s_pauseMinuteRegex = new Regex("\\|\\|\\| \\+ ([\\d\\.]*?) MINUTES? \\|\\|\\|", RegexOptions.Compiled);
		private static readonly Regex s_musicEndRegex = new Regex("{Music--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicStartRegex = new Regex("{Music--Starts @ v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicStopAndStartRegex = new Regex("{Music--Ends & New Music--Starts @ v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxStartRegex = new Regex("{SFX--(.*?)(?:--Starts)? (?:@|before) v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxEndRegex = new Regex("{SFX--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxEndRegex2 = new Regex("{SFX--(.*?)--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxRangeRegex = new Regex("{SFX--(.*?) @ v(\\d*?)-(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicSfxRegex = new Regex("{Music \\+ SFX--(.*?) Starts? @ v(\\d*?)}", RegexOptions.Compiled);

		public static bool ConvertTextToControlScriptAnnotationElement(string text, out ScriptAnnotation annotation)
		{
			if (string.IsNullOrWhiteSpace(text))
				throw new ArgumentException("text must contain non-whitespace", "text");

			var match = s_doNotCombineRegex.Match(text);
			if (match.Success)
				return ConvertTextToControlScriptAnnotationElement(text.Substring(match.Length), out annotation);

			match = s_pauseRegex.Match(text);
			if (match.Success)
			{
				annotation = new Pause { TimeUnits = TimeUnits.Seconds, Time = double.Parse(match.Groups[1].Value) };
				return true;
			}
			match = s_pauseMinuteRegex.Match(text);
			if (match.Success)
			{
				annotation = new Pause { TimeUnits = TimeUnits.Minutes, Time = double.Parse(match.Groups[1].Value) };
				return true;
			}

			match = s_musicEndRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Music, EndVerse = int.Parse(match.Groups[1].Value) };
				return true;
			}

			match = s_musicStartRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Music, StartVerse = int.Parse(match.Groups[1].Value) };
				return true;
			}

			match = s_musicStopAndStartRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Music, StartVerse = int.Parse(match.Groups[1].Value), EndVerse = Sound.kNonSpecificStartOrStop};
				return true;
			}

			match = s_sfxEndRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Sfx, EndVerse = int.Parse(match.Groups[1].Value) };
				return true;
			}
			match = s_sfxEndRegex2.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Sfx, EffectName = match.Groups[1].Value, EndVerse = int.Parse(match.Groups[2].Value) };
				return true;
			}

			match = s_sfxStartRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Sfx, EffectName = match.Groups[1].Value, StartVerse = int.Parse(match.Groups[2].Value) };
				return true;
			}

			match = s_sfxRangeRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.Sfx, EffectName = match.Groups[1].Value, StartVerse = int.Parse(match.Groups[2].Value), EndVerse = int.Parse(match.Groups[3].Value) };
				return true;
			}

			match = s_musicSfxRegex.Match(text);
			if (match.Success)
			{
				annotation = new Sound { SoundType = SoundType.MusicSfx, EffectName = match.Groups[1].Value, StartVerse = int.Parse(match.Groups[2].Value) };
				return true;
			}

			annotation = null;
			return false;
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

	static class Extensions
	{
		public static string TabSeparated(this SortedSet<string> strings)
		{
			var sb = new StringBuilder();
			foreach (var str in strings)
				sb.Append(str).Append("\t");
			sb.Length--;
			return sb.ToString();
		}
	}
}
