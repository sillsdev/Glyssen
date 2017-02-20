using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen;
using Glyssen.Character;
using Glyssen.Quote;
using OfficeOpenXml;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.Xml;

namespace DevTools
{
	static class ReferenceTextUtility
	{
		private const string kOutputFileForAnnotations = @"..\..\Glyssen\Resources\Annotations.txt";
		private const string kDirectorGuideInput = @"..\..\DevTools\Resources\DIRECTOR_GUIDES.xlsx";

		private const string kBookHeader = "Book";
		private const string kChapterHeader = "Cp";
		private const string kVerseHeader = "Vs";
		private const string kCharacterHeader = "CHARACTER";
		private const string kEnglishHeader = "ENGLISH";
		private const string kNewEnglishTempName = "NewEnglish";

		private static readonly ReferenceText s_existingEnglish;
		private static readonly Regex s_verseNumberMarkupRegex = new Regex("(\\{\\d*?\\}\u00A0)", RegexOptions.Compiled);
		private static readonly Regex s_extractVerseNumberRegex = new Regex("\\{(\\d*?)\\}\u00A0", RegexOptions.Compiled);
		private static readonly Regex s_removeQuotes = new Regex("[“”\"'\u2018\u2019«»]|(--$)", RegexOptions.Compiled);
		private static readonly Regex s_verseNumberInExcelRegex = new Regex("\\{(\\d*?)\\} ?", RegexOptions.Compiled);
		private static readonly Regex s_doubleSingleOpenQuote = new Regex("(<< <)", RegexOptions.Compiled);
		private static readonly Regex s_singleDoubleOpenQuote = new Regex("(< <<)", RegexOptions.Compiled);
		private static readonly Regex s_doubleSingleCloseQuote = new Regex("(>> >)", RegexOptions.Compiled);
		private static readonly Regex s_singleDoubleCloseQuote = new Regex("((> >>)|(>>>))", RegexOptions.Compiled);
		private static readonly Regex s_doubleOpenQuote = new Regex("(<<)", RegexOptions.Compiled);
		private static readonly Regex s_doubleCloseQuote = new Regex("(>>)", RegexOptions.Compiled);
		private static readonly Regex s_singleOpenQuote = new Regex("(<)", RegexOptions.Compiled);
		private static readonly Regex s_singleCloseQuote = new Regex("(>)", RegexOptions.Compiled);

		private static Regex s_regexStartQuoteMarks;
		private static Regex s_regexEndQuoteMarks;
		private static Regex s_regexStartEnglishDoubleQuoteMarks;
		private static Regex s_regexEndEnglishDoubleQuoteMarks;

		private static void InitializeQuoteDetectionRegexes()
		{
			var allQuoteChars = new HashSet<string>();
			foreach (char c in from string quoteMark in QuoteUtils.AllDefaultSymbols().Where(s => (string)s != QuoteUtils.None) from c in quoteMark select c)
				allQuoteChars.Add("(" + Regex.Escape(c.ToString()) + ")");
			allQuoteChars.Add("(" + Regex.Escape(@"""") + ")");
			allQuoteChars.Add("(" + Regex.Escape("-") + ")");
			allQuoteChars.Add("(" + Regex.Escape("\u2012") + ")");
			allQuoteChars.Add("(" + Regex.Escape("\u2013") + ")");
			allQuoteChars.Add("(" + Regex.Escape("\u2014") + ")");
			allQuoteChars.Add("(" + Regex.Escape("\u2015") + ")");
			allQuoteChars.Add("(" + Regex.Escape("&gt;") + ")");
			allQuoteChars.Add("(" + Regex.Escape("&gt;") + ")");
			allQuoteChars.Add("(" + Regex.Escape("&lt;") + ")");

			s_regexStartQuoteMarks = new Regex(@"^\s*" + String.Join("|", allQuoteChars), RegexOptions.Compiled);
			s_regexEndQuoteMarks = new Regex("(" + String.Join("|", allQuoteChars) + @")\s*[.,?!]*\s*$", RegexOptions.Compiled);
			s_regexStartEnglishDoubleQuoteMarks = new Regex(@"^\s*“|""", RegexOptions.Compiled);
			s_regexEndEnglishDoubleQuoteMarks = new Regex(@"”|""\s*$", RegexOptions.Compiled);
		}

		static ReferenceTextUtility()
		{
			InitializeQuoteDetectionRegexes();
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

		private static string GetColumnFromCellAddress(string address)
		{
			return Regex.Match(address, @"[A-Z]+").Value;
		}

		private static string NormalizeLanguageColumnHeaderName(string nameFromExcelFile)
		{
			// Not sure why FCBH spells it "PISON", but we'll just fix it up here.
			var nameParts = nameFromExcelFile.Replace("PISON", "PISIN").Split(' ', '\n');
			var sb = new StringBuilder();
			foreach (var part in nameParts.Where(p => !string.IsNullOrEmpty(p)))
			{
				if (part.StartsWith("("))
					break;
				sb.Append(Char.ToUpper(part[0]));
				foreach (var letter in part.Skip(1))
					sb.Append(Char.ToLower(letter));
			}

			return sb.ToString();
		}

		public static bool GenerateReferenceTexts(
			bool onlyRunToFindDifferencesBetweenCurrentVersionAndExcel,
			bool onlyCreateCharacterMapping,
			ReferenceTextIdentifier refTextId = null,
			bool ignoreWhitespaceAndPunctuationDifferences = false)
		{
			Debug.Assert(!onlyRunToFindDifferencesBetweenCurrentVersionAndExcel || !onlyCreateCharacterMapping);

			var characterMappings = new List<CharacterMapping>();
			var glyssenToFcbhIds = new SortedDictionary<string, SortedSet<string>>();
			var fcbhToGlyssenIds = new SortedDictionary<string, SortedSet<string>>();

			if (!File.Exists(kDirectorGuideInput))
			{
				Console.WriteLine("File does not exist: " + kDirectorGuideInput);
				return false;
			}

			IEnumerable<string> languageNames;
			var referenceTextRowsFromExcelSpreadsheet = GetDataFromExcelFile(out languageNames);

			bool errorsOccurred = !referenceTextRowsFromExcelSpreadsheet.Any() || !languageNames.Any();
			var resultSummary = new List<BookTitleAndChapterLabelInfo>(66); // Though all we have currently is the NT

			var annotationsToOutput = new List<string>();
			var languagesToProcess = (refTextId == null || refTextId.Type == ReferenceTextType.Unknown) ? languageNames :
				languageNames.Where(l => l == refTextId.Name);

			// This must match logic in Glyssen.Program.BaseDataFolder (but Program is an internal class)
			var proprietaryRefTextTemBaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				"FCBH-SIL", "Glyssen", "Newly Generated Reference Texts");

			ReferenceText existingReferenceTextForLanguage = null;

			foreach (var language in languagesToProcess)
			{
				Console.WriteLine("Processing " + language + "...");

				if (onlyRunToFindDifferencesBetweenCurrentVersionAndExcel)
				{
					try
					{
						existingReferenceTextForLanguage = refTextId != null ? ReferenceText.GetReferenceText(refTextId) : GetReferenceTextFromString(language);
					}
					catch (ArgumentException)
					{
						Console.WriteLine($"No existing reference text for language {language}");
						continue;
					}
				}

				const string kOutputDirDistfiles = @"..\..\DistFiles\reference_texts";
				ReferenceTextType refTextType;
				string outputDir = Enum.TryParse(language, out refTextType) ? kOutputDirDistfiles : proprietaryRefTextTemBaseFolder;

				bool doingEnglish = language == ReferenceTextType.English.ToString();
				string languageFolderName = doingEnglish ? kNewEnglishTempName : language;
				string languageOutputDir = Path.Combine(outputDir, languageFolderName);
				Directory.CreateDirectory(languageOutputDir);

				string prevBook = null;
				int iBook = 0;
				int iBlock = 0;
				BookScript existingEnglishRefBook = null;
				string chapterLabel = null;
				string chapterLabelForPrevBook = null;
				string justTheWordForChapter = null;
				int startBook = BCVRef.BookToNumber("MAT");
				List<BookScript> existingEnglishRefBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= startBook).ToList();
				List<BookScript> newBooks = new List<BookScript>();
				List<Block> newBlocks = new List<Block>();
				TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo = null;
				IReadOnlyList<Block> existingRefBlocksForLanguage = null;

				string openDoubleQuote;
				string closeDoubleQuote;
				string openQuoteSingle;
				string closeQuoteSingle;
				GetQuoteMarksForLanguage(language, out openDoubleQuote, out closeDoubleQuote, out openQuoteSingle, out closeQuoteSingle);

				foreach (var referenceTextRow in referenceTextRowsFromExcelSpreadsheet.Where(r => BCVRef.BookToNumber(ConvertFcbhBookCodeToSilBookCode(r.Book)) >= startBook))
				{
					if (prevBook != referenceTextRow.Book)
					{
						if (existingEnglishRefBook != null)
						{
							newBooks.Add(new BookScript(existingEnglishRefBook.BookId, newBlocks) {PageHeader = chapterLabel});
							newBlocks.Clear();
						}
						existingEnglishRefBook = existingEnglishRefBooks[iBook++];
						if (onlyRunToFindDifferencesBetweenCurrentVersionAndExcel)
							existingRefBlocksForLanguage = existingReferenceTextForLanguage.Books.Single(b => b.BookId == existingEnglishRefBook.BookId).GetScriptBlocks();
						iBlock = 0;
						chapterLabelForPrevBook = chapterLabel;
						chapterLabel = null;

						var newBlock = new Block("mt")
						{
							CharacterId =
								CharacterVerseData.GetStandardCharacterId(existingEnglishRefBook.BookId,
									CharacterVerseData.StandardCharacter.BookOrChapter),
						};
						var bookTitleAndchapter1Announcement = referenceTextRow.GetText(language);
						var summaryForBook = resultSummary.SingleOrDefault(b => b.BookId == existingEnglishRefBook.BookId);
						if (summaryForBook == null)
						{
							summaryForBook = new BookTitleAndChapterLabelInfo {BookId = existingEnglishRefBook.BookId};
							resultSummary.Add(summaryForBook);
						}
						currentTitleAndChapterLabelInfo = new TitleAndChapterLabelInfo
						{
							Language = language,
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
					var existingEnglishRefBlock = existingEnglishRefBook.GetScriptBlocks()[iBlock++];

					while (CharacterVerseData.IsCharacterExtraBiblical(existingEnglishRefBlock.CharacterId))
					{
						existingEnglishRefBlock = existingEnglishRefBook.GetScriptBlocks()[iBlock++];
					}

					if (referenceTextRow.Verse == "<<")
					{
						int chapter = int.Parse(referenceTextRow.Chapter);
						if (chapter == 2)
						{
							currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls = referenceTextRow.GetText(language);
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

							var mainTitleElement = (ScriptText)newBlocks.First().BlockElements[0];
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
								CharacterVerseData.GetStandardCharacterId(existingEnglishRefBook.BookId,
									CharacterVerseData.StandardCharacter.BookOrChapter),
							IsParagraphStart = true,
							BookCode = existingEnglishRefBook.BookId
						};
						newBlock.BlockElements.Add(new ScriptText(referenceTextRow.Chapter));
						newBlocks.Add(newBlock);

						iBlock--;
					}
					else
					{
						if (onlyCreateCharacterMapping)
						{
							if (!CharacterVerseData.IsCharacterOfType(existingEnglishRefBlock.CharacterId, CharacterVerseData.StandardCharacter.Narrator) || !referenceTextRow.CharacterId.StartsWith("Narr_0"))
							{
								var verse = new BCVRef(BCVRef.BookToNumber(existingEnglishRefBook.BookId), int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse));
								characterMappings.Add(new CharacterMapping(existingEnglishRefBlock.CharacterId, referenceTextRow.CharacterId, verse));

								SortedSet<string> fcbhIds;
								if (glyssenToFcbhIds.TryGetValue(existingEnglishRefBlock.CharacterId, out fcbhIds))
									fcbhIds.Add(referenceTextRow.CharacterId);
								else
									glyssenToFcbhIds.Add(existingEnglishRefBlock.CharacterId, new SortedSet<string> {referenceTextRow.CharacterId});

								SortedSet<string> glyssenIds;
								if (fcbhToGlyssenIds.TryGetValue(referenceTextRow.CharacterId, out glyssenIds))
									glyssenIds.Add(existingEnglishRefBlock.CharacterId);
								else
									fcbhToGlyssenIds.Add(referenceTextRow.CharacterId, new SortedSet<string> {existingEnglishRefBlock.CharacterId});
							}
							continue;
						}

						string originalText = referenceTextRow.GetText(language);
						var verseNumberFixedText = s_verseNumberInExcelRegex.Replace(originalText, "{$1}\u00A0");
						var modifiedText = s_doubleSingleOpenQuote.Replace(verseNumberFixedText, openDoubleQuote + "\u202F" + openQuoteSingle);
						modifiedText = s_singleDoubleOpenQuote.Replace(modifiedText, openQuoteSingle + "\u202F" + openDoubleQuote);
						modifiedText = s_doubleSingleCloseQuote.Replace(modifiedText, closeDoubleQuote + "\u202F" + closeQuoteSingle);
						modifiedText = s_singleDoubleCloseQuote.Replace(modifiedText, closeQuoteSingle + "\u202F" + closeDoubleQuote);
						modifiedText = s_doubleOpenQuote.Replace(modifiedText, openDoubleQuote);
						modifiedText = s_doubleCloseQuote.Replace(modifiedText, closeDoubleQuote);
						modifiedText = s_singleOpenQuote.Replace(modifiedText, openQuoteSingle);
						modifiedText = s_singleCloseQuote.Replace(modifiedText, closeQuoteSingle);
						if (verseNumberFixedText != modifiedText)
							Debug.WriteLine($"{verseNumberFixedText} != {modifiedText}");

						if (!onlyRunToFindDifferencesBetweenCurrentVersionAndExcel ||
							CompareIgnoringQuoteMarkDifferences(modifiedText, existingRefBlocksForLanguage[iBlock - 1], referenceTextRow.Book, ignoreWhitespaceAndPunctuationDifferences))
						{
							if (int.Parse(referenceTextRow.Chapter) != existingEnglishRefBlock.ChapterNumber)
							{
								Console.WriteLine($"Chapters do not match. Book: {referenceTextRow.Book}, Excel: {referenceTextRow.Chapter}, Existing: {existingEnglishRefBlock.ChapterNumber}");
								errorsOccurred = true;
							}
							if (int.Parse(referenceTextRow.Verse) != existingEnglishRefBlock.InitialStartVerseNumber)
							{
								Console.WriteLine($"Verse numbers do not match. Book: {referenceTextRow.Book}, Ch: {referenceTextRow.Chapter}, " +
									$"Excell: {referenceTextRow.Verse}, Existing: {existingEnglishRefBlock.InitialStartVerseNumber}");
								errorsOccurred = true;
							}

							var newBlock = new Block(existingEnglishRefBlock.StyleTag, int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse))
							{
								CharacterId = existingEnglishRefBlock.CharacterId,
								Delivery = existingEnglishRefBlock.Delivery,
								IsParagraphStart = existingEnglishRefBlock.IsParagraphStart,
								MultiBlockQuote = existingEnglishRefBlock.MultiBlockQuote
							};
							BlockElement lastElementInBlock = null;
							var splits = s_verseNumberMarkupRegex.Split(modifiedText);
							foreach (var split in splits)
							{
								if (string.IsNullOrWhiteSpace(split))
								{
									if (splits.Length == 1)
										Debug.Fail("");
									continue;
								}
								var match = s_extractVerseNumberRegex.Match(split);
								if (match.Success)
								{
									var verseNum = match.Groups[1].Value;
									var nonAnnotations = newBlock.BlockElements.Where(be => be.GetType() == typeof(Verse) || be.GetType() == typeof(ScriptText));
									var processingFirstElement = !nonAnnotations.Any();
									newBlock.BlockElements.Add(lastElementInBlock = new Verse(verseNum));
									if (processingFirstElement)
										newBlock.InitialStartVerseNumber = int.Parse(verseNum);
									else if (newBlock.InitialStartVerseNumber == int.Parse(verseNum))
									{
										//Console.WriteLine();
										//Console.WriteLine("Verse number incorrect. Language: {3}, Bk: {0}, Ch: {1}, Vrs: {2}", existingBook.BookId, newBlock.ChapterNumber, newBlock.InitialStartVerseNumber, language);
										//Console.WriteLine(newBlock.GetText(true));
										newBlock.InitialStartVerseNumber = newBlocks[newBlocks.Count - 1].LastVerseNum;
										//Console.WriteLine("Corrected verse number to {0}", newBlock.InitialStartVerseNumber);
									}
								}
								else
								{
									var splits2 = Regex.Split(split, "(" + RegexEscapedDoNotCombine + "{[^0-9]+.*?}|{[^0-9]+.*?}| \\|\\|\\|.*?\\|\\|\\| )");
									foreach (var s in splits2)
									{
										if (string.IsNullOrWhiteSpace(s))
											continue;
										var match2 = Regex.Match(s, RegexEscapedDoNotCombine + "{[^0-9]+.*?}|{[^0-9]+.*?}| \\|\\|\\|.*?\\|\\|\\| ");
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
												if (doingEnglish)
												{
													var pause = annotation as Pause;
													var serializedAnnotation = pause != null ? XmlSerializationHelper.SerializeToString(pause, true) :
														XmlSerializationHelper.SerializeToString((Sound)annotation, true);

													var formattedAnnotationForDisplay = annotation.ToDisplay();

													if (string.IsNullOrWhiteSpace(formattedAnnotationForDisplay) || string.IsNullOrWhiteSpace(serializedAnnotation))
													{
														Console.WriteLine("Annotation not formatted correctly (is null or whitespace): {0}", referenceTextRow.English);
														Console.WriteLine();
														errorsOccurred = true;
													}
													var trimmedEnglish = referenceTextRow.English.TrimEnd();
													if ((annotation is Pause && !trimmedEnglish.EndsWith(formattedAnnotationForDisplay)) ||
														(annotation is Sound && !trimmedEnglish.StartsWith(formattedAnnotationForDisplay)))
													{
														var bcv = new BCVRef(BCVRef.BookToNumber(existingEnglishRefBook.BookId), existingEnglishRefBlock.ChapterNumber, existingEnglishRefBlock.InitialStartVerseNumber);
														Console.WriteLine("(warning) Annotation not formatted the same as FCBH: ({0}) {1} => {2}", bcv.AsString, referenceTextRow.English, formattedAnnotationForDisplay);
														Console.WriteLine();
														// This is a good check to run for sanity. But we can't fail as
														// a few of the annotations are actually displayed slightly differently by FCBH
														// (due to what are insignificant differences like 'before' vs. '@')
														//errorsOccurred = true;
													}
													int offset = 0;
													if ((existingEnglishRefBook.BookId == "MRK" && existingEnglishRefBlock.ChapterNumber == 4 && existingEnglishRefBlock.InitialVerseNumberOrBridge == "39") ||
														(existingEnglishRefBook.BookId == "ACT" && existingEnglishRefBlock.ChapterNumber == 10 && existingEnglishRefBlock.InitialVerseNumberOrBridge == "23"))
													{
														offset = -1;
													}
													annotationsToOutput.Add(existingEnglishRefBook.BookId + "\t" + existingEnglishRefBlock.ChapterNumber + "\t" +
														existingEnglishRefBlock.InitialVerseNumberOrBridge + "\t" + offset + "\t" + serializedAnnotation);
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
							var lastScriptText = newBlock.BlockElements.OfType<ScriptText>().Last();
							lastScriptText.Content = lastScriptText.Content.Trim();
							newBlocks.Add(newBlock);
						}
					}
					prevBook = referenceTextRow.Book;
				}

				if (onlyCreateCharacterMapping)
				{
					WriteCharacterMappingFiles(characterMappings, glyssenToFcbhIds, fcbhToGlyssenIds);
					return true;
				}

				if (!onlyRunToFindDifferencesBetweenCurrentVersionAndExcel)
				{
					newBooks.Add(new BookScript(existingEnglishRefBook.BookId, newBlocks) {PageHeader = chapterLabel});

					foreach (var bookScript in newBooks)
					{
						if (!doingEnglish)
							LinkBlockByBlockInOrder(bookScript);
						XmlSerializationHelper.SerializeToFile(Path.Combine(languageOutputDir, bookScript.BookId + ".xml"), bookScript);
					}
				}
			}

			if (onlyRunToFindDifferencesBetweenCurrentVersionAndExcel)
				return true;

			if (languagesToProcess.Any(l => l != ReferenceTextType.English.ToString()))
				Console.WriteLine($"Reference texts (other than English) have been aligned to the EXISTING English reference text. If that version " +
					"is replaced by \"{kNewEnglishTempName}\", then the tool needs to re re-run to link all reference texts to the new English version.");

			WriteAnnotationsFile(annotationsToOutput);

			if (!errorsOccurred)
			{
				Console.WriteLine("Write book title and chapter label summary to file? Y/N");
				var answer = Console.ReadLine();
				if (answer == "Y" || answer == "y")
					WriteTitleAndChapterSummaryResults(resultSummary);

				return true;
			}
			return !errorsOccurred;
		}

		private static void WriteTitleAndChapterSummaryResults(List<BookTitleAndChapterLabelInfo> resultSummary)
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"Protoscript Generator", "book title and chapter label summary.txt");
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

		private static List<ReferenceTextRow> GetDataFromExcelFile(out IEnumerable<string> languageNames)
		{
			var allLanguages = new Dictionary<string, string>();

			List<ReferenceTextRow> referenceTextRowsFromExcelSpreadsheet = new List<ReferenceTextRow>();
			int attempt = 1;
			while (attempt < 3)
			{
				if (attempt > 1)
				{
					Console.WriteLine("You probably have the Excel File open. If so, close it and press any key to try again...");
					Console.ReadLine();
				}

				try
				{
					using (var xls = new ExcelPackage(new FileInfo(kDirectorGuideInput)))
					{
						var worksheet = xls.Workbook.Worksheets["Sheet1"];

						string bookCol = null, chapterCol = null, verseCol = null, characterCol = null;

						var cells = worksheet.Cells;
						var rowData = cells.GroupBy(c => c.Start.Row).ToList();
						int rowsToSkip = -1;
						for (int iRow = 0; iRow <= rowData.Count; iRow++)
						{
							var nonNullCellAddresses = rowData[iRow].Where(er => !String.IsNullOrWhiteSpace(cells[er.Address].Value?.ToString())).ToList();
							var bookAddress = nonNullCellAddresses.FirstOrDefault(a => cells[a.Address].Value.Equals(kBookHeader))?.Address;
							if (bookAddress != null)
							{
								bookCol = GetColumnFromCellAddress(bookAddress);
								foreach (var excelRange in nonNullCellAddresses.Skip(1))
								{
									var value = cells[excelRange.Address].Value.ToString();
									var col = GetColumnFromCellAddress(excelRange.Address);
									switch (value)
									{
										case kChapterHeader:
											chapterCol = col;
											break;
										case kVerseHeader:
											verseCol = col;
											break;
										case kCharacterHeader:
											characterCol = col;
											break;
										case kEnglishHeader: // English is required and must come first!
											allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
											break;
										default:
											if (allLanguages.Any()) // Any other columns before English will be ignored
												allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
											break;
									}
								}
								rowsToSkip = iRow + 1;
								break;
							}
						}
						if (bookCol == null)
							throw new MissingFieldException($"Book column not found!", kBookHeader);
						if (chapterCol == null)
							throw new MissingFieldException($"Chapter number column not found!", kChapterHeader);
						if (verseCol == null)
							throw new MissingFieldException($"Verse number column not found!", kChapterHeader);
						if (characterCol == null)
							throw new MissingFieldException($"Character column not found!", kCharacterHeader);
						if (!allLanguages.Any())
							throw new MissingFieldException($"No language columns found! (English must exist and must be first)");

						foreach (var textRow in rowData.Skip(rowsToSkip))
						{
							var row = textRow.Key;
							var verseValue = cells[verseCol + row].Value;
							if (verseValue == null)
							{
								Trace.WriteLine("Stopping at row " + row + " because verse column contained a null value.");
								break;
							}
							var verseStr = verseValue as string ?? ((double)verseValue).ToString(CultureInfo.InvariantCulture);
							referenceTextRowsFromExcelSpreadsheet.Add(new ReferenceTextRow(
								(string)cells[bookCol + row].Value,
								((double)cells[chapterCol + row].Value).ToString(CultureInfo.InvariantCulture),
								verseStr,
								(string)cells[characterCol + row].Value,
								allLanguages.ToDictionary(kvp => kvp.Key, kvp => cells[kvp.Value + row].Value.ToString())));
						}
					}
					break;
				}
				catch (IOException ex)
				{
					Console.WriteLine(ex.Message);
					attempt++;
				}
			}

			languageNames = allLanguages.Keys;
			return referenceTextRowsFromExcelSpreadsheet;
		}

		private static ReferenceText GetReferenceTextFromString(string language)
		{
			ReferenceTextType type;
			if (Enum.TryParse<ReferenceTextType>(language, out type))
			{
				if (type == ReferenceTextType.Custom || type == ReferenceTextType.Unknown)
					throw new ArgumentException("unknown language", nameof(language));
				return ReferenceText.GetStandardReferenceText(type);
			}

			var refTextId = ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, language);
			if (refTextId.Missing)
				throw new ArgumentException("Unvailable custom language", nameof(language));

			return ReferenceText.GetReferenceText(refTextId);
		}

		private static void WriteCharacterMappingFiles(List<CharacterMapping> characterMappings, SortedDictionary<string, SortedSet<string>> glyssenToFcbhIds, SortedDictionary<string, SortedSet<string>> fcbhToGlyssenIds)
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

		private static bool CompareIgnoringQuoteMarkDifferences(string excelStr, Block existingBlock, string bookId, bool ignoreWhitespaceAndPunctuationDifferences)
		{
			var existingStr = existingBlock.GetText(true);
			var excelStrWithoutAnnotations = Regex.Replace(excelStr, " \\|\\|\\|.*?\\|\\|\\| ", "");
			excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "{[^0-9]+.*?}", "");

			if (ignoreWhitespaceAndPunctuationDifferences)
			{
				if (existingStr.Where(c => !Char.IsWhiteSpace(c) && !Char.IsPunctuation(c) && !Char.IsSymbol(c)).SequenceEqual(
					excelStrWithoutAnnotations.Where(c => !Char.IsWhiteSpace(c) && !Char.IsPunctuation(c) && !Char.IsSymbol(c))))
					return true;
			}
			else
			{
				excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "  ", " ");
				excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "\u00A0 ", "\u00A0");

				var existingStrModified = Regex.Replace(s_removeQuotes.Replace(existingStr, ""), "\u00A0 ", "\u00A0").Trim();
				excelStrWithoutAnnotations = s_removeQuotes.Replace(excelStrWithoutAnnotations, "").Trim();

				if (excelStrWithoutAnnotations == existingStrModified)
					return true;
			}
			string lengthComparison;
			int lengthChange = existingStr.Length - excelStr.Length;
			if (lengthChange == 0)
				lengthComparison = "String lengths identical";
			else if (lengthChange < 0)
				lengthComparison = $"String length grew by {Math.Abs(lengthChange)} characters";
			else
				lengthComparison = $"String length shrunk by {lengthChange} characters";

			Console.WriteLine($"Difference found in text at {bookId} {existingBlock.ChapterNumber}:{existingBlock.InitialStartVerseNumber} - {lengthComparison}");
			Console.WriteLine($"   Existing: {existingStr}");
			Console.WriteLine($"   New:      {excelStr}");
			// ------------------------------------------
			// Put a breakpoint here to investigate differences
			// | | | |
			// V V V V
			return false;
		}

		private static void GetQuoteMarksForLanguage(string languageName, out string doubleOpen, out string doubleClose, out string singleOpen, out string singleClose)
		{
			doubleOpen = "“";
			doubleClose = "”";
			singleOpen = "‘";
			singleClose = "’";
			switch (languageName)
			{
				case "Azeri":
					doubleOpen = "\"";
					doubleClose = "\"";
					singleOpen = "“";
					singleClose = "”";
					break;
				case "French":
				case "Spanish":
					doubleOpen = "«";
					doubleClose = "»";
					break;
				case "TokPisin":
					doubleOpen = "\"";
					doubleClose = "\"";
					singleOpen = "'";
					singleClose = "'";
					break;
			}
		}

		public static bool LinkToEnglish()
		{
			bool errorOccurred = false;
			foreach (var referenceTextId in ReferenceTextIdentifier.AllAvailable.Where(r => r.Type != ReferenceTextType.English))
			{
				Console.WriteLine("Processing " +
					(referenceTextId.Type == ReferenceTextType.Custom ? referenceTextId.CustomIdentifier : referenceTextId.Type.ToString()) +
					"...");

				string openQuote;
				string closeQuote;
				string openQuoteSingle;
				string closeQuoteSingle;
				GetQuoteMarksForLanguage(referenceTextId.Name, out openQuote, out closeQuote, out openQuoteSingle, out closeQuoteSingle);

				Console.Write("   ");

				var refText = ReferenceText.GetReferenceText(referenceTextId);

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

						Exception error;

						try
						{
							LinkBlockByBlockInOrder(book);
							//s_existingEnglish.ApplyTo(book, s_existingEnglish.Versification, true);
							string folder = (string) ReflectionHelper.GetProperty(refText, "ProjectFolder");
							string bookXmlFile = Path.Combine(folder, Path.ChangeExtension(book.BookId, "xml"));
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

		private static void LinkBlockByBlockInOrder(BookScript book)
		{
			var blocks = book.GetScriptBlocks();
			var englishBlocks = s_existingEnglish.Books.Single(b => b.BookId == book.BookId).GetScriptBlocks();
			Debug.Assert(blocks.Count == englishBlocks.Count);
			for (int i = 0; i < blocks.Count; i++)
			{
				var block = blocks[i];
				if (block.IsChapterAnnouncement)
				{
					var refChapterBlock = new Block(block.StyleTag, block.ChapterNumber);
					refChapterBlock.BlockElements.Add(
						new ScriptText(s_existingEnglish.GetFormattedChapterAnnouncement(book.BookId, block.ChapterNumber)));
					block.SetMatchedReferenceBlock(refChapterBlock);
				}
				else
				{
					block.SetMatchedReferenceBlock(englishBlocks[i]);
					if (!englishBlocks[i].CharacterIsStandard)
					{
						if (englishBlocks[i].StartsWithQuoteMarks(s_regexStartEnglishDoubleQuoteMarks) && !block.StartsWithQuoteMarks(s_regexStartQuoteMarks))
						{
							var firstScriptText = block.BlockElements.OfType<ScriptText>().First();
							firstScriptText.Content = "“" + firstScriptText.Content;
						}
						if (englishBlocks[i].EndsWithQuoteMarks(s_regexEndEnglishDoubleQuoteMarks) && !block.EndsWithQuoteMarks(s_regexEndQuoteMarks))
						{
							block.BlockElements.OfType<ScriptText>().Last().Content += "”";
						}
					}
				}
			}
		}

		private static bool StartsWithQuoteMarks(this Block block, Regex regex)
		{
			return regex.IsMatch(block.BlockElements.OfType<ScriptText>().First().Content);
		}

		private static bool EndsWithQuoteMarks(this Block block, Regex regex)
		{
			return regex.IsMatch(block.BlockElements.OfType<ScriptText>().Last().Content);
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

		private static string RegexEscapedDoNotCombine
		{
			get { return Regex.Escape(Sound.kDoNotCombine) + " "; }
		}

		private static readonly Regex s_doNotCombineRegex = new Regex(RegexEscapedDoNotCombine, RegexOptions.Compiled);
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

		public static void ObfuscateProprietaryReferenceTextsToMakeTestingResources()
		{
			var baseResourcesDir = Path.Combine("..", "..", "GlyssenTests", "Resources");
			var outputDir = Path.Combine(baseResourcesDir, "temporary");

			if (!Directory.Exists(outputDir))
				Directory.CreateDirectory(outputDir);

			foreach (var rt in ReferenceTextIdentifier.AllAvailable.Where(r => r.Type == ReferenceTextType.Custom))
			{
				var refText = ReferenceText.GetReferenceText(rt);
				foreach (var book in refText.Books)
				{
					var fileName = String.Format("{0}{1}RefText.xml", rt.CustomIdentifier, book.BookId);
					var existingTestResourcePath = Path.Combine(baseResourcesDir, fileName);
					var outputPath = Path.Combine(outputDir, fileName);
					if (File.Exists(existingTestResourcePath) || File.Exists(outputPath))
						continue;

					foreach (var block in book.GetScriptBlocks())
					{
						foreach (var scriptText in block.BlockElements.OfType<ScriptText>())
						{
							var content = scriptText.Content;
							content = Regex.Replace(content, "([^ ]* )([^ ]* )*([^ ]*.*)", "$1... $3");
							scriptText.Content = content;
						}
					}

					XmlSerializationHelper.SerializeToFile(outputPath, book);
				}
			}
		}
	}

	class ReferenceTextRow
	{
		private readonly Dictionary<string, string> m_text;

		public ReferenceTextRow(string book, string chapter, string verse, string characterId, Dictionary<string, string> text)
		{
			m_text = text;
			Book = book;
			Chapter = chapter;
			Verse = verse;
			CharacterId = characterId;
			m_text = text;
			if (!m_text.ContainsKey("English"))
				throw new ArgumentException("English is required", nameof(text));
		}

		public string Book { get; set; }
		public string Chapter { get; set; }
		public string Verse { get; set; }
		public string CharacterId { get; set; }
		public string English => m_text["English"];

		public string GetText(string language)
		{
			return m_text[language];
		}

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
