using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Glyssen.Character;
using Glyssen.Quote;
using Glyssen.Shared;
using OfficeOpenXml;
using SIL.Reflection;
using SIL.Scripture;
using SIL.Xml;

namespace Glyssen.RefTextDevUtilities
{
	public static class ReferenceTextUtility
	{
		private const string kOutputFileForAnnotations = @"..\..\Glyssen\Resources\Annotations.txt";
		public const string kDirectorGuideInput = @"..\..\DevTools\Resources\DIRECTOR_GUIDES.xlsx";
		public const string kDirectorGuideOTInput = @"..\..\DevTools\Resources\DIRECTOR_GUIDE_OT.xlsx";
		public const string kOutputDirDistfiles = @"..\..\DistFiles\reference_texts";

		public static string ProprietaryRefTextTempBaseFolder => Path.Combine(GlyssenInfo.BaseDataFolder, "Newly Generated Reference Texts");

		private const string kBookHeader = "Book";
		private const string kBookHeaderAlt = "Bk";
		private const string kChapterHeader = "Chapter";
		private const string kChapterHeaderAlt = "Cp";
		private const string kVerseHeader = "Verse";
		private const string kVerseHeaderAlt = "Vs";
		private const string kCharacterHeader = "Character";
		private const string kCharacterHeaderAlt1 = "CHARACTER";
		private const string kCharacterHeaderAlt2 = "Char";
		private const string kEnglishHeader = "ENGLISH";
		private const string kEnglishHeaderAltEnglishOnly = "Text";
		public const string kTempFolderPrefix = "New";

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
		private static readonly Regex s_FcbhNarrator = new Regex("Narr_\\d+: ", RegexOptions.Compiled);

		private static Regex s_regexStartQuoteMarks;
		private static Regex s_regexEndQuoteMarks;
		private static Regex s_regexStartEnglishDoubleQuoteMarks;
		private static Regex s_regexEndEnglishDoubleQuoteMarks;

		private static ISet<Tuple<string, BCVRef, string>> s_unmatchedCharacterIds = new HashSet<Tuple<string, BCVRef, string>>();
		private static ISet<Tuple<string, string, BCVRef, string>> s_characterIdsMatchedByControlFile = new HashSet<Tuple<string, string, BCVRef, string>>();
		private static ISet<string> s_characterIdsMatchedByControlFileStr = new HashSet<string>();
		private static Dictionary<string, CharacterDetail> s_characterDetailsWithUnmatchedFCBHCharacterLabel;

		public static bool ErrorsOccurred { get; private set; }

		public delegate void MessageEventHandler(string message, bool isError);

		public static event MessageEventHandler OnMessageRaised;

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

		public static void WriteOutput(string msg = "", bool error = false)
		{
			ErrorsOccurred |= error;
			OnMessageRaised?.Invoke(msg, error);
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
			public string BookId { get; set; }
			public List<TitleAndChapterLabelInfo> Details { get; } = new List<TitleAndChapterLabelInfo>(8);
		}

		private class CharacterMapping
		{
			public CharacterMapping(string glyssenId, string fcbhId, BCVRef verse)
			{
				GlyssenId = glyssenId;
				FcbhId = fcbhId;
				Verse = verse;
			}

			private string GlyssenId { get; }
			private string FcbhId { get; }
			private BCVRef Verse { get; }

			public override string ToString()
			{
				return $"{Verse.AsString}\t{GlyssenId}\t{FcbhId}";
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

		public enum Mode
		{
			// This will also generate the annotations.txt file.
			Generate,
			FindDifferencesBetweenCurrentVersionAndNewText,
			CreateCharacterMapping,
			CreateBookTitleAndChapterLabelSummary,
			/*
			 * This will create book (.xml) files for each book in DistFiles/reference_texts/NewEnglish.
			 * My process, after running in this mode, was to
			 * 1) Do a bunch of checking, including making use of the output files in DevTools/Resources/temporary.
			 * 2) Copy the book files files into a real Glyssen project which would not run a quote parse (version matched control file).
			 * 3) Run the books through Identify Speaking Parts.
			 * 4) Copy the files to DistFiles/reference_texts/English.
			 * 5) Remove all userConfirmed="true"> from the book files.
			 */
			GenerateEnglish
		}

		public enum Ignore
		{
			QuotationMarkDifferences,
			AllDifferences,
			WhitespaceDifferences,
			AllDifferencesExceptAlphaNumericText,
		}

		public enum Testament
		{
			WholeBible,
			OT,
			NT,
		}

		public enum MatchLikelihood
		{
			Reliable,
			Possible,
			Mismatch,
		}

		public static Ignore ComparisonSensitivity { get; set; }

		public static void ProcessReferenceTextDataFromFile(Mode mode, ReferenceTextProxy refTextId = null, Testament testament = Testament.WholeBible)
		{
			ReferenceTextData ntData = null, otData = null;
			var hasNtData = testament != Testament.OT && ProcessExcelFile(kDirectorGuideInput, out ntData);
			var hasOtData = testament != Testament.NT && ProcessExcelFile(kDirectorGuideOTInput, out otData);
			if (!hasNtData && !hasOtData)
				return;

			if (refTextId != null)
			{
				ntData?.FilterBy(refTextId.Name);
				otData?.FilterBy(refTextId.Name);
			}

			ProcessReferenceTextData(mode, ntData, otData, () => refTextId != null ? ReferenceText.GetReferenceText(refTextId) : null);
		}

		private static bool ProcessExcelFile(string excelPath, out ReferenceTextData data)
		{
			data = null;
			// This had better be in console mode!!!
			if (!File.Exists(excelPath))
			{
				WriteOutput("File does not exist: " + excelPath, true);
				return false;
			}
			int attempt = 1;
			do
			{
				if (attempt > 1)
				{
					Console.WriteLine("You probably have the Excel File open. If so, close it and press any key to try again...");
					Console.ReadLine();
				}

				try
				{
					data = GetDataFromExcelFile(excelPath);
				}
				catch (IOException ex)
				{
					Console.WriteLine(ex.Message);
					if (++attempt > 3)
					{
						WriteOutput("Giving up...", true);
						return false;
					}
				}
			} while (data == null);
			return true;
		}

		/// <summary>
		/// We can't just roll this into the method below with an optional parameter because
		/// ReferenceTextUtility doesn't have access to the definition of a 'ReferenceText'
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="data"></param>
		public static void ProcessReferenceTextData(Mode mode, ReferenceTextData data, ReferenceTextData otData)
		{
			ProcessReferenceTextData(mode, data, otData, null);
		}

		public static void ProcessReferenceTextData(
			Mode mode,
			ReferenceTextData ntData,
			ReferenceTextData otData,
			Func<ReferenceText> getReferenceText)
		{
			ErrorsOccurred = false;

			var characterMappings = new List<CharacterMapping>();
			var glyssenToFcbhIds = new SortedDictionary<string, SortedSet<string>>();
			var fcbhToGlyssenIds = new SortedDictionary<string, SortedSet<string>>();
			s_characterDetailsWithUnmatchedFCBHCharacterLabel = CharacterDetailData.Singleton.GetDictionary()
				.Where(kvp =>
				{
					if (kvp.Value.DefaultFCBHCharacter == null)
						return false;
					if (otData == null && kvp.Value.LastReference.BBCCCVVV < 40000000)
						return false;
					if (ntData == null && kvp.Value.FirstReference.BBCCCVVV > 39999999)
						return false;
					return true;
				}).ToDictionary(e => e.Key, e => e.Value);

			ErrorsOccurred = (ntData != null && !ntData.IsValid) || (otData != null && !otData.IsValid);
			var resultSummary = new List<BookTitleAndChapterLabelInfo>(66);

			var annotationsToOutput = new List<string>();

			ReferenceText existingReferenceTextForLanguage = null;
			List<ReferenceTextLanguageInfo> languagesToProcess = null;

			for (int iData = 0 + (otData == null ? 1 : 0); iData < (ntData == null ? 1 : 2); iData++)
			{
				languagesToProcess = iData == 0 ? otData.LanguagesToProcess.ToList() : ntData.LanguagesToProcess.ToList();

				foreach (var languageInfo in languagesToProcess)
				{
					if (mode == Mode.GenerateEnglish && !languageInfo.IsEnglish)
						continue;

					var language = languageInfo.Name;
					WriteOutput("Processing " + language + "...");

					if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
					{
						try
						{
							existingReferenceTextForLanguage = getReferenceText?.Invoke() ?? GetReferenceTextFromString(language);
						}
						catch (ArgumentException)
						{
							WriteOutput($"No existing reference text for language {language}", languagesToProcess.Count == 1);
							continue;
						}
					}

					if (String.IsNullOrEmpty(languageInfo.OutputFolder))
					{
						ReferenceTextType refTextType;
						languageInfo.OutputFolder = Enum.TryParse(language, out refTextType)
							? kOutputDirDistfiles
							: ProprietaryRefTextTempBaseFolder;
						languageInfo.OutputFolder = Path.Combine(languageInfo.OutputFolder,
							(languageInfo.IsEnglish ? kTempFolderPrefix : "") + language);
					}

					Directory.CreateDirectory(languageInfo.OutputFolder);

					string prevBook = null;
					int iBook = 0;
					int iBlock = 0;
					BookScript existingEnglishRefBook = null;
					string chapterLabel = null;
					string chapterLabelForPrevBook = null;
					string justTheWordForChapter = null;
					List<BookScript> existingEnglishRefBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= 40).ToList();
					if (iData == 0)
						existingEnglishRefBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) < 40).ToList();
					List<BookScript> newBooks = new List<BookScript>();
					List<Block> newBlocks = new List<Block>();
					TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo = null;
					IReadOnlyList<Block> existingRefBlocksForLanguage = null;

					GetQuoteMarksForLanguage(language, out var openDoubleQuote, out var closeDoubleQuote, out var openQuoteSingle, out var closeQuoteSingle);

					var referenceTextRowsToProcess = iData == 0
						? otData.ReferenceTextRows.Where(delegate(ReferenceTextRow r)
						{
							return mode == Mode.GenerateEnglish ||
									existingEnglishRefBooks.Select(b => BCVRef.BookToNumber(b.BookId)).Contains(BCVRef.BookToNumber(GetBookIdFromFcbhBookCode(r.Book)));
						})
						: ntData.ReferenceTextRows.Where(delegate(ReferenceTextRow r)
						{
							return mode == Mode.GenerateEnglish ||
									existingEnglishRefBooks.Select(b => BCVRef.BookToNumber(b.BookId)).Contains(BCVRef.BookToNumber(GetBookIdFromFcbhBookCode(r.Book)));
						});

					foreach (var referenceTextRow in referenceTextRowsToProcess)
					{
						var referenceTextBookId = GetBookIdFromFcbhBookCode(referenceTextRow.Book);

						if (prevBook != referenceTextBookId)
						{
							if (existingEnglishRefBook != null)
							{
								newBooks.Add(new BookScript(existingEnglishRefBook.BookId, newBlocks, existingEnglishRefBook.Versification) {PageHeader = chapterLabel});
								newBlocks.Clear();
							}

							if (mode == Mode.GenerateEnglish)
								existingEnglishRefBook = new BookScript(referenceTextBookId, new Block[10000], ScrVers.English);
							else
								existingEnglishRefBook = existingEnglishRefBooks[iBook++];

							if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
								existingRefBlocksForLanguage = existingReferenceTextForLanguage.Books
									.Single(b => b.BookId == existingEnglishRefBook.BookId).GetScriptBlocks();
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
										//WriteOutput("Guessing at chapter label: " + chapterLabel);
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

						if (existingEnglishRefBlock != null)
						{
							while (CharacterVerseData.IsCharacterExtraBiblical(existingEnglishRefBlock.CharacterId))
							{
								existingEnglishRefBlock = existingEnglishRefBook.GetScriptBlocks()[iBlock++];
							}
						}

						if (referenceTextRow.Verse == "<<")
						{
							int chapter = int.Parse(referenceTextRow.Chapter);
							if (chapter == 2)
							{
								currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls = referenceTextRow.GetText(language);
								var chapterLabelForCurrentBook = currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls.TrimEnd(' ', '2').TrimStart();
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
											bookName = chapterLabel.Substring(0, chapterLabel.IndexOf(justTheWordForChapter, StringComparison.Ordinal))
												.TrimEnd();
											mainTitleElement.Content = bookName;
										}
										else
										{
											chapterLabel = bookName + " " + justTheWordForChapter;
											//WriteOutput("Book title being left as \"" + bookName + "\" and chapter label set to: " + chapterLabel);
										}
									}
									else if (bookName != chapterLabel)
										WriteOutput("Could not figure out book title: " + bookName, true);
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
							if (mode == Mode.CreateCharacterMapping)
							{
								if (!CharacterVerseData.IsCharacterOfType(existingEnglishRefBlock.CharacterId,
										CharacterVerseData.StandardCharacter.Narrator) || !referenceTextRow.CharacterId.StartsWith("Narr_0"))
								{
									var verse = new BCVRef(BCVRef.BookToNumber(existingEnglishRefBook.BookId), int.Parse(referenceTextRow.Chapter),
										int.Parse(referenceTextRow.Verse));
									characterMappings.Add(new CharacterMapping(existingEnglishRefBlock.CharacterId, referenceTextRow.CharacterId,
										verse));

									SortedSet<string> fcbhIds;
									if (glyssenToFcbhIds.TryGetValue(existingEnglishRefBlock.CharacterId, out fcbhIds))
										fcbhIds.Add(referenceTextRow.CharacterId);
									else
										glyssenToFcbhIds.Add(existingEnglishRefBlock.CharacterId,
											new SortedSet<string> {referenceTextRow.CharacterId});

									SortedSet<string> glyssenIds;
									if (fcbhToGlyssenIds.TryGetValue(referenceTextRow.CharacterId, out glyssenIds))
										glyssenIds.Add(existingEnglishRefBlock.CharacterId);
									else
										fcbhToGlyssenIds.Add(referenceTextRow.CharacterId,
											new SortedSet<string> {existingEnglishRefBlock.CharacterId});
								}

								continue;
							}

							string originalText = referenceTextRow.GetText(language);
							var verseNumberFixedText = s_verseNumberInExcelRegex.Replace(originalText, "{$1}\u00A0");
							var modifiedText =
								s_doubleSingleOpenQuote.Replace(verseNumberFixedText, openDoubleQuote + "\u202F" + openQuoteSingle);
							modifiedText = s_singleDoubleOpenQuote.Replace(modifiedText, openQuoteSingle + "\u202F" + openDoubleQuote);
							modifiedText = s_doubleSingleCloseQuote.Replace(modifiedText, closeDoubleQuote + "\u202F" + closeQuoteSingle);
							modifiedText = s_singleDoubleCloseQuote.Replace(modifiedText, closeQuoteSingle + "\u202F" + closeDoubleQuote);
							modifiedText = s_doubleOpenQuote.Replace(modifiedText, openDoubleQuote);
							modifiedText = s_doubleCloseQuote.Replace(modifiedText, closeDoubleQuote);
							modifiedText = s_singleOpenQuote.Replace(modifiedText, openQuoteSingle);
							modifiedText = s_singleCloseQuote.Replace(modifiedText, closeQuoteSingle);
							if (verseNumberFixedText != modifiedText)
								Debug.WriteLine($"{verseNumberFixedText} != {modifiedText}");

							if (mode != Mode.FindDifferencesBetweenCurrentVersionAndNewText ||
								CompareVersions(modifiedText, existingRefBlocksForLanguage[iBlock - 1], referenceTextBookId))
							{
								if (mode != Mode.GenerateEnglish &&
									int.Parse(referenceTextRow.Chapter) != existingEnglishRefBlock.ChapterNumber)
								{
									WriteOutput($"Chapters do not match. Book: {referenceTextBookId}, Excel: {referenceTextRow.Chapter}, Existing: {existingEnglishRefBlock.ChapterNumber}", true);
								}

								if (mode != Mode.GenerateEnglish &&
									int.Parse(referenceTextRow.Verse) != existingEnglishRefBlock.InitialStartVerseNumber)
								{
									WriteOutput($"Verse numbers do not match. Book: {referenceTextBookId}, Ch: {referenceTextRow.Chapter}, " +
												$"Excel: {referenceTextRow.Verse}, Existing: {existingEnglishRefBlock.InitialStartVerseNumber}", true);
								}

								Block newBlock;
								if (mode != Mode.GenerateEnglish)
								{
									newBlock = new Block(existingEnglishRefBlock.StyleTag, int.Parse(referenceTextRow.Chapter),
										int.Parse(referenceTextRow.Verse))
									{
										CharacterId = existingEnglishRefBlock.CharacterId,
										Delivery = existingEnglishRefBlock.Delivery,
										IsParagraphStart = existingEnglishRefBlock.IsParagraphStart,
										MultiBlockQuote = existingEnglishRefBlock.MultiBlockQuote
									};
								}
								else
								{
									newBlock = new Block("p", int.Parse(referenceTextRow.Chapter), int.Parse(referenceTextRow.Verse));
									newBlock.CharacterId =
										GetCharacterIdFromFCBHCharacterLabel(referenceTextRow.CharacterId, referenceTextBookId, newBlock);
								}

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
										var nonAnnotations =
											newBlock.BlockElements.Where(be => be.GetType() == typeof(Verse) || be.GetType() == typeof(ScriptText));
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
										var splits2 = Regex.Split(split,
											"(" + RegexEscapedDoNotCombine + "{[^0-9]+.*?}|{[^0-9]+.*?}| \\|\\|\\|.*?\\|\\|\\| )");
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
													if (mode != Mode.GenerateEnglish && languageInfo.IsEnglish)
													{
														var pause = annotation as Pause;
														var serializedAnnotation = pause != null
															? XmlSerializationHelper.SerializeToString(pause, true)
															: XmlSerializationHelper.SerializeToString((Sound) annotation, true);

														var formattedAnnotationForDisplay = annotation.ToDisplay();

														if (string.IsNullOrWhiteSpace(formattedAnnotationForDisplay) ||
															string.IsNullOrWhiteSpace(serializedAnnotation))
														{
															WriteOutput($"Annotation not formatted correctly (is null or whitespace): {referenceTextRow.English}",
																true);
															WriteOutput();
														}

														var trimmedEnglish = referenceTextRow.English.TrimEnd();
														if ((annotation is Pause && !trimmedEnglish.EndsWith(formattedAnnotationForDisplay)) ||
															(annotation is Sound && !trimmedEnglish.StartsWith(formattedAnnotationForDisplay)))
														{
															// Although this is a good check to run for sanity, we can't treat it as an error
															// because a few of the annotations are actually displayed slightly differently by
															// FCBH (due to what are insignificant differences like 'before' vs. '@')
															var bcv = new BCVRef(BCVRef.BookToNumber(existingEnglishRefBook.BookId),
																existingEnglishRefBlock.ChapterNumber, existingEnglishRefBlock.InitialStartVerseNumber);
															WriteOutput(
																$"(warning) Annotation not formatted the same as FCBH: ({bcv.AsString}) {referenceTextRow.English} => {formattedAnnotationForDisplay}");
															WriteOutput();
														}

														int offset = 0;
														if ((existingEnglishRefBook.BookId == "MRK" && existingEnglishRefBlock.ChapterNumber == 4 &&
															existingEnglishRefBlock.InitialVerseNumberOrBridge == "39") ||
															(existingEnglishRefBook.BookId == "ACT" && existingEnglishRefBlock.ChapterNumber == 10 &&
															existingEnglishRefBlock.InitialVerseNumberOrBridge == "23"))
														{
															offset = -1;
														}

														annotationsToOutput.Add(existingEnglishRefBook.BookId + "\t" + existingEnglishRefBlock.ChapterNumber +
																				"\t" +
																				existingEnglishRefBlock.InitialVerseNumberOrBridge + "\t" + offset + "\t" + serializedAnnotation);
													}
												}
												else
												{
													WriteOutput("Could not parse annotation: " + referenceTextRow, true);
												}
											}
											else
											{
												string text = s.TrimStart();
												if (string.IsNullOrWhiteSpace(text))
													WriteOutput("No text found between annotations:" + referenceTextRow, true);
												else
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

						prevBook = referenceTextBookId;
					}

					if (mode == Mode.CreateCharacterMapping)
					{
						WriteCharacterMappingFiles(characterMappings, glyssenToFcbhIds, fcbhToGlyssenIds);
						return;
					}

					if (mode == Mode.Generate || mode == Mode.GenerateEnglish)
					{
						newBooks.Add(new BookScript(existingEnglishRefBook.BookId, newBlocks, existingEnglishRefBook.Versification) {PageHeader = chapterLabel});

						foreach (var bookScript in newBooks)
						{
							if (!languageInfo.IsEnglish)
								LinkBlockByBlockInOrder(bookScript);
							XmlSerializationHelper.SerializeToFile(Path.Combine(languageInfo.OutputFolder, bookScript.BookId + ".xml"),
								bookScript);
						}
					}
				}
			}

			if (mode == Mode.Generate)
			{
				if (languagesToProcess.Any(l => !l.IsEnglish))
					WriteOutput("Reference texts (other than English) have been aligned to the EXISTING English reference text. If that version is " +
						"replaced by a new version of the English text, then the tool needs to re re-run to link all reference texts to the new English version.");

				WriteAnnotationsFile(annotationsToOutput);
			}

			if (mode == Mode.GenerateEnglish)
			{
				var temporaryPathRoot = @"..\..\DevTools\Resources\temporary";
				Directory.CreateDirectory(temporaryPathRoot);

				var sortedUnmatchedStr = s_unmatchedCharacterIds.OrderByDescending(t => t.Item1).ThenBy(t => t.Item2).Select(t => $"{t.Item1}\t{t.Item2}\t{t.Item3}");
				File.WriteAllLines(Path.Combine(temporaryPathRoot, "unmatched.txt"), sortedUnmatchedStr);

				var sortedMatched = s_characterIdsMatchedByControlFile.Select(t => $"{t.Item1} => {t.Item2}").ToList();
				sortedMatched.Sort();
				File.WriteAllLines(Path.Combine(temporaryPathRoot, "matchedByControlFileWOref.txt"), sortedMatched);

				var matchedSortedByRefStr = s_characterIdsMatchedByControlFile.OrderBy(t => t.Item3).ThenBy(t => t.Item1).Select(t =>
				{
					var extra = t.Item4.Contains("/") ? $"\t\t({t.Item4})" : "";
					return $"{t.Item3}\t-- {t.Item1} => {t.Item2}{extra}";
				});
				File.WriteAllLines(Path.Combine(temporaryPathRoot, "matchedByControlFileSortedByRef.txt"), matchedSortedByRefStr);

				var sortedMatchedStr = s_characterIdsMatchedByControlFileStr.ToList();
				sortedMatchedStr.Sort();
				File.WriteAllLines(Path.Combine(temporaryPathRoot, "matchedByControlFile.txt"), sortedMatchedStr);

				if (s_characterDetailsWithUnmatchedFCBHCharacterLabel.Any())
				{
					var filename = Path.Combine(temporaryPathRoot, "FCBHCharacterLabelsNotMatched.txt");
					WriteOutput($"CharacterDetail.txt contains some FCBH character labels that were not found in the " +
						$"source Director's Guide. See {filename} for details.", true);
					File.WriteAllLines(filename, s_characterDetailsWithUnmatchedFCBHCharacterLabel.Values
						.Select(d => $"{d.CharacterId} => {d.DefaultFCBHCharacter}"));
				}
			}

			if (!ErrorsOccurred && mode == Mode.CreateBookTitleAndChapterLabelSummary)
			{
				WriteTitleAndChapterSummaryResults(resultSummary);
			}

			WriteOutput("Done!");
		}

		private static string GetBookIdFromFcbhBookCode(string fcbhBookCode)
		{
			var bookId = BCVRef.BookToNumber(fcbhBookCode);
			if (bookId > 0)
				return fcbhBookCode;

			string silBookCode = null;
			switch (fcbhBookCode)
			{
				case "1SM":
					silBookCode = "1SA";
					break;
				case "2SM":
					silBookCode = "2SA";
					break;
				case "PSM":
					silBookCode = "PSA";
					break;
				case "PRV":
					silBookCode = "PRO";
					break;
				case "SOS":
					silBookCode = "SNG";
					break;
				case "EZE":
					silBookCode = "EZK";
					break;
				case "JOE":
					silBookCode = "JOL";
					break;
				case "NAH":
					silBookCode = "NAM";
					break;
			}

			return silBookCode;
		}

		static readonly Regex s_stripNumericSuffixes = new Regex(@"(.*?)((( #)|(-FX)|(_))\d+)+", RegexOptions.Compiled);
		static readonly Regex s_stripFemaleSuffix = new Regex(@"(.*) \(female\)", RegexOptions.Compiled);
		static readonly Regex s_matchWithoutParentheses = new Regex("[^(]*", RegexOptions.Compiled);
		static readonly Regex s_matchGlyssenKing = new Regex(@"(?<name>(\w|-)+),? king of .+", RegexOptions.Compiled);
		static readonly Regex s_matchFcbhKing = new Regex(@"^(King )?(?<name>[A-Z](\w|-)+)( I{1,3})?", RegexOptions.Compiled);
		static readonly Regex s_matchPossessiveWithApostropheS = new Regex(@"(?<possessor>.+)? of (?<possessee>.+)", RegexOptions.Compiled);
		static readonly Regex s_matchPossessiveWithOf = new Regex(@"((?<possessee>.+)'s (?<possessor>.+))", RegexOptions.Compiled);
		static readonly Regex s_matchGlyssenProperNameWithQualifiers = new Regex(@"^(?<name>([A-Z](\w|-)+))((, )|( \()).+", RegexOptions.Compiled);
		static readonly Regex s_matchFcbhProperNameWithLabel = new Regex(@"\w+: (?<name>([A-Z](\w|-)+))", RegexOptions.Compiled);
		static readonly Regex s_matchGlyssenFirstWordCapitalized = new Regex(@"^(?<name>([A-Z](\w|-)+))", RegexOptions.Compiled);

		private static string GetCharacterIdFromFCBHCharacterLabel(string fcbhCharacterLabel, string bookId, Block block)
		{
			//if (bookId == "EZR" && block.ChapterNumber == 9)
			//	Debug.WriteLine("Here");

			if (s_FcbhNarrator.IsMatch(fcbhCharacterLabel))
				return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

			var fcbhCharacterLabelOrig = fcbhCharacterLabel;
			var fcbhCharacterLabelSansNumber = fcbhCharacterLabel = s_stripNumericSuffixes.Replace(fcbhCharacterLabel, "$1");
			fcbhCharacterLabel = s_stripFemaleSuffix.Replace(fcbhCharacterLabel, "$1");

			//if (CharacterDetailData.Singleton.GetAllCharacterIdsAsLowerInvariant().Contains(characterId.ToLowerInvariant()))
			//	return characterId;
			//if (TryGetKnownNameMatch(characterId, out var knownMatch))
			//	return knownMatch;

			var bookNum = BCVRef.BookToNumber(bookId);
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber, block.LastVerseNum, includeAlternates:true).ToList();
			var bcvRef = new BCVRef(bookNum, block.ChapterNumber, block.InitialStartVerseNumber);
			switch (characters.Count)
			{
				case 1:
					var character = characters.Single();
					// If the character Id has slashes, the following line gets the default one.
					var characterIdToUse = character.ResolvedDefaultCharacter;

					switch (IsReliableMatch(fcbhCharacterLabel, fcbhCharacterLabelSansNumber, characterIdToUse, character.Alias))
					{
						case MatchLikelihood.Reliable:
							// Don't bother reporting; these are not interesting
							break;
						case MatchLikelihood.Mismatch:
							if (character.Character.Contains("/" + fcbhCharacterLabel) || character.Character.Contains(fcbhCharacterLabel + "/"))
							{
								s_unmatchedCharacterIds.Add(new Tuple<string, BCVRef, string>(CharacterVerseData.kAmbiguousCharacter, bcvRef, fcbhCharacterLabel));
								return CharacterVerseData.kAmbiguousCharacter;
							}
							characterIdToUse = null;
							break;
						default:

							if (IsNarratorOverride(bookNum, block, fcbhCharacterLabel, fcbhCharacterLabelSansNumber))
								return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

							if (characterIdToUse != character.Character)
							{
								// This is a multi-character ID. If FCBH's guide happens to prefer a different default one and it's
								// an exact (case-insensitive) match, we will map it to their desired default, but we will report it for
								// further research and evaluation.
								characterIdToUse = character.Character.SplitCharacterId().Skip(1)
									.SingleOrDefault(alt => alt.Equals(fcbhCharacterLabel, StringComparison.OrdinalIgnoreCase)) ?? characterIdToUse;
							}
							s_characterIdsMatchedByControlFile.Add(new Tuple<string, string, BCVRef, string>(fcbhCharacterLabelOrig, characterIdToUse, bcvRef, character.Character));
							s_characterIdsMatchedByControlFileStr.Add($"{fcbhCharacterLabelOrig} => {characterIdToUse} -- {bcvRef}");
							break;
					}
					if (characterIdToUse == null)
						goto case 0;
					return characterIdToUse;
				case 0:
					if (IsNarratorOverride(bookNum, block, fcbhCharacterLabel, fcbhCharacterLabelSansNumber))
						return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

					//if (CharacterDetailData.Singleton.GetAllCharacterIdsAsLowerInvariant().Contains(characterId.ToLowerInvariant()))
					//	return characterId;
					s_unmatchedCharacterIds.Add(new Tuple<string, BCVRef, string>(CharacterVerseData.kUnexpectedCharacter, bcvRef, fcbhCharacterLabel));
					//return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
					return CharacterVerseData.kUnexpectedCharacter;
				default:
					if (characters.Any(c => c.Character == CharacterVerseData.kNeedsReview) && ForceRefTextToNeedsReview(fcbhCharacterLabel, bookId, block.ChapterNumber, block.InitialStartVerseNumber))
						return CharacterVerseData.kNeedsReview;
					var defaultCharactersAndFullCharacterIds = characters.Select(c => new Tuple<string, string>(c.ResolvedDefaultCharacter, c.Character)).ToList();
					try
					{
						return defaultCharactersAndFullCharacterIds.Select(c => c.Item1).Single(glyssenCharId => IsReliableMatch(fcbhCharacterLabel, fcbhCharacterLabelSansNumber, glyssenCharId) == MatchLikelihood.Reliable);
					}
					catch
					{
						var altMatch = defaultCharactersAndFullCharacterIds.Where(t => t.Item1 != t.Item2).Select(c =>
							new Tuple<string, string>(c.Item2.SplitCharacterId().Skip(1).FirstOrDefault(alt =>
							alt.Equals(fcbhCharacterLabel, StringComparison.OrdinalIgnoreCase)), c.Item2)).FirstOrDefault();
						if (altMatch?.Item1 != null)
						{
							s_characterIdsMatchedByControlFile.Add(new Tuple<string, string, BCVRef, string>(fcbhCharacterLabel, altMatch.Item1, bcvRef, altMatch.Item2));
							s_characterIdsMatchedByControlFileStr.Add($"{fcbhCharacterLabel} => {altMatch.Item1} -- {bcvRef}");
							return altMatch.Item1;
						}
						else
						{
							s_unmatchedCharacterIds.Add(new Tuple<string, BCVRef, string>(CharacterVerseData.kAmbiguousCharacter, bcvRef, fcbhCharacterLabel));
							return CharacterVerseData.kAmbiguousCharacter;
						}
					}
			}
		}

		private static bool ForceRefTextToNeedsReview(string fcbhCharacterLabel, string bookId, int chapter, int verse)
		{
			switch (fcbhCharacterLabel)
			{
				case "Servant": return (bookId == "2KI" && chapter == 5 && verse == 4);
				default: return false;
			}
		}

		private static bool IsNarratorOverride(int bookNum, Block block, string fcbhCharacterLabel, string fcbhCharacterLabelSansNumber)
		{
			return NarratorOverrides.GetCharacterOverrideForBlock(bookNum, block, ScrVers.English)
				.Any(oc => IsReliableMatch(fcbhCharacterLabel, fcbhCharacterLabelSansNumber, oc) == MatchLikelihood.Reliable);
		}

		private static MatchLikelihood IsReliableMatch(string fcbhCharacterLabel, string fcbhCharacterLabelSansNumber, string glyssenCharacterId, string alias = null)
		{
			if (CharacterVerseData.IsCharacterStandard(glyssenCharacterId) || glyssenCharacterId == CharacterVerseData.kNeedsReview)
				return MatchLikelihood.Mismatch; // Before we call this, we've already checked to see if the FCBH character is the narrator. Can't auto-map any other character to that.

			if (glyssenCharacterId == fcbhCharacterLabel)
				return MatchLikelihood.Reliable; // Exact match

			var details = CharacterDetailData.Singleton.GetDictionary();
			// REVIEW: Do we want to store in our control files the exact character name from FCBH (e.g., Man #1)
			// or the stripped down version (e.g., Man)
			if (details[glyssenCharacterId].DefaultFCBHCharacter == fcbhCharacterLabelSansNumber)
			{
				s_characterDetailsWithUnmatchedFCBHCharacterLabel.Remove(glyssenCharacterId);
				return MatchLikelihood.Reliable;
			}

			if (details.ContainsKey(fcbhCharacterLabel))
			{
				// Never match to some other existing character ID, except one that just differs by age or one that has an exact match on the alias.
				if (glyssenCharacterId == fcbhCharacterLabel + " (old)" || glyssenCharacterId == fcbhCharacterLabel + " (dead)" ||
					alias == fcbhCharacterLabel)
					return MatchLikelihood.Reliable;

				return MatchLikelihood.Mismatch;
			}

			var characterIdToUseToLower = glyssenCharacterId.ToLowerInvariant();
			var fcbhCharacterToLower = fcbhCharacterLabel.ToLowerInvariant();
			if (characterIdToUseToLower.StartsWith(fcbhCharacterToLower) ||
				fcbhCharacterToLower.StartsWith(characterIdToUseToLower) ||
				s_matchWithoutParentheses.Match(characterIdToUseToLower).Value == s_matchWithoutParentheses.Match(fcbhCharacterToLower).Value ||
				characterIdToUseToLower.Replace("the ", string.Empty) == fcbhCharacterToLower)
			{
				return MatchLikelihood.Reliable;
			}

			if ((glyssenCharacterId == "God" && !fcbhCharacterLabel.Contains("God")) ||
				(!glyssenCharacterId.Contains("God") && fcbhCharacterLabel == "God"))
				return MatchLikelihood.Mismatch; // "God" can't map to some other character

			switch (fcbhCharacterLabel)
			{
				case "David":
					if (glyssenCharacterId.StartsWith("David's") || glyssenCharacterId == "fool")
						return MatchLikelihood.Mismatch; // "David" can't map to an enemy, servant, etc. of David or to "fool"
					break;
				case "Psalmist":
					return MatchLikelihood.Mismatch; // "Psalmist" can only match psalmist
				case "Ezra":
					if (glyssenCharacterId != "Ezra, priest and teacher")
						return MatchLikelihood.Mismatch; // See Neh 8:10. But this is also needed to prevent bogus match to "leaders" in EZR 9:1
					break;
			}

			var glyssenKing = s_matchGlyssenKing.Match(glyssenCharacterId);
			if (glyssenKing.Success)
			{
				var fcbhKing = s_matchFcbhKing.Match(fcbhCharacterLabel);
				if (fcbhKing.Success && glyssenKing.Result("${name}") == fcbhKing.Result("${name}"))
					return MatchLikelihood.Reliable;
			}

			var glyssenPossessive = s_matchPossessiveWithApostropheS.Match(characterIdToUseToLower);
			if (glyssenPossessive.Success)
			{
				var fcbhPossessive = s_matchPossessiveWithOf.Match(fcbhCharacterToLower);
				if (fcbhPossessive.Success &&
					glyssenPossessive.Result("${possessor}").StartsWith(fcbhPossessive.Result("${possessor}")) &&
					glyssenPossessive.Result("${possessee}").StartsWith(fcbhPossessive.Result("${possessee}")))
					return MatchLikelihood.Reliable;
			}
			else
			{
				var fcbhPossessive = s_matchPossessiveWithApostropheS.Match(fcbhCharacterToLower);
				if (fcbhPossessive.Success)
				{
					glyssenPossessive = s_matchPossessiveWithOf.Match(characterIdToUseToLower);
					if (glyssenPossessive.Success &&
						glyssenPossessive.Result("${possessor}").StartsWith(fcbhPossessive.Result("${possessor}")) &&
						glyssenPossessive.Result("${possessee}").StartsWith(fcbhPossessive.Result("${possessee}")))
						return MatchLikelihood.Reliable;
				}
			}

			var matchGProperName = s_matchGlyssenProperNameWithQualifiers.Match(glyssenCharacterId);
			if (matchGProperName.Success)
			{
				if (matchGProperName.Result("${name}") == fcbhCharacterLabel)
					return MatchLikelihood.Reliable;
			}

			var matchFcbhProperName = s_matchFcbhProperNameWithLabel.Match(fcbhCharacterLabel);
			if (!matchFcbhProperName.Success)
				matchFcbhProperName = s_matchFcbhKing.Match(fcbhCharacterLabel);
			if (matchFcbhProperName.Success)
				{
				matchGProperName = s_matchGlyssenFirstWordCapitalized.Match(glyssenCharacterId);
				if (matchGProperName.Success &&
					matchGProperName.Result("${name}") == matchFcbhProperName.Result("${name}"))
					return MatchLikelihood.Reliable;
			}

			if (fcbhCharacterToLower.Replace("man", "men") == characterIdToUseToLower)
				return MatchLikelihood.Reliable;

			return IsKnownNameMatch(fcbhCharacterLabel, glyssenCharacterId) ? MatchLikelihood.Reliable : MatchLikelihood.Possible;
		}

		private static bool IsKnownNameMatch(string fcbhCharacterLabel, string glyssenCharacterId)
		{
			switch (fcbhCharacterLabel)
			{
				case "Angel": return glyssenCharacterId.StartsWith("angel", StringComparison.OrdinalIgnoreCase) ||
					glyssenCharacterId == "horses (or their angelic riders) (in vision)";
				case "Son of Jacob": return glyssenCharacterId == "Joseph's brothers";
				case "Rehab": return glyssenCharacterId == "Rahab";
				case "Judean": return glyssenCharacterId == "Judah, men of";
				case "Woman": return glyssenCharacterId == "Babylon (personified as adulteress)";
				case "Spirit": return glyssenCharacterId == "Holy Spirit, the";
				case "Queen of Babylon": return glyssenCharacterId == "Babylon (personified as adulteress)";
				case "Gehazi": return glyssenCharacterId == "Elisha's messenger";
				// FCBH seems insistent on using the same character label for these two different Zechariah's.
				case "Zechariah": return glyssenCharacterId == "Zechariah, son of Jehoiada the priest" ||
					glyssenCharacterId == "Zechariah the prophet, son of Berechiah";
				case "Israelite in Egypt": return glyssenCharacterId.StartsWith("idolaters from Judah");
				case "Hebrew": return glyssenCharacterId.StartsWith("Israelite");
				case "Man": return glyssenCharacterId.StartsWith("men");
				case "Gilead": return glyssenCharacterId.StartsWith("family heads of Gilead");
				case "Leader": return glyssenCharacterId.EndsWith(", leaders of");
				case "Watchman": return glyssenCharacterId == "lookout";
				case "Soldier":
					return glyssenCharacterId.IndexOf("soldier", StringComparison.OrdinalIgnoreCase) >= 0 ||
						glyssenCharacterId.IndexOf("men", StringComparison.OrdinalIgnoreCase) >= 0 ||
						glyssenCharacterId.IndexOf("messenger", StringComparison.OrdinalIgnoreCase) >= 0 ||
						glyssenCharacterId == "Judah, men of";
				case "Heavenly Man":
					return glyssenCharacterId == "man like bronze with measuring rod (in vision)" ||
						glyssenCharacterId == "man's voice from the Ulai (in vision)" ||
						glyssenCharacterId == "one who looked like a man" ||
						glyssenCharacterId == "man in linen above river";
				default: return false;
			}
		}

		private static void WriteTitleAndChapterSummaryResults(List<BookTitleAndChapterLabelInfo> resultSummary)
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				GlyssenInfo.kProduct, "book title and chapter label summary.txt");
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

		public static ReferenceTextData GetDataFromExcelFile(string path)
		{
			var allLanguages = new Dictionary<string, string>();

			var data = new ReferenceTextData();

			using (var xls = new ExcelPackage(new FileInfo(path)))
			{
				var worksheet = xls.Workbook.Worksheets["Sheet1"];

				string bookCol = null, chapterCol = null, verseCol = null, characterCol = null;

				var cells = worksheet.Cells;
				var rowData = cells.GroupBy(c => c.Start.Row).ToList();
				int rowsToSkip = -1;
				for (int iRow = 0; iRow <= rowData.Count; iRow++)
				{
					var nonNullCellAddresses = rowData[iRow].Where(er => !String.IsNullOrWhiteSpace(cells[er.Address].Value?.ToString())).ToList();
					var bookAddress = (nonNullCellAddresses.FirstOrDefault(a => cells[a.Address].Value.Equals(kBookHeader)) ??
						nonNullCellAddresses.FirstOrDefault(a => cells[a.Address].Value.Equals(kBookHeaderAlt)))?.Address;
					if (bookAddress != null)
					{
						bookCol = GetColumnFromCellAddress(bookAddress);
						bool doneProcessingColumns = false;
						foreach (var excelRange in nonNullCellAddresses.Skip(1))
						{
							var value = cells[excelRange.Address].Value.ToString();
							var col = GetColumnFromCellAddress(excelRange.Address);
							switch (value)
							{
								case kChapterHeader:
								case kChapterHeaderAlt:
									chapterCol = col;
									break;
								case kVerseHeader:
								case kVerseHeaderAlt:
									verseCol = col;
									break;
								case kCharacterHeader:
								case kCharacterHeaderAlt1:
								case kCharacterHeaderAlt2:
									characterCol = col;
									break;
								case kEnglishHeaderAltEnglishOnly: // If re-generating English, the English column can be labeled "Text"
									doneProcessingColumns = true;
									value = "English";
									goto case kEnglishHeader;
								case kEnglishHeader: // English is required and must come first!
									allLanguages.Clear();
									allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
									break;
								default:
									if (allLanguages.Any()) // Any other columns before English will be ignored
										allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
									break;
							}
							if (doneProcessingColumns)
								break;
						}
						rowsToSkip = iRow + 1;
						break;
					}
				}
				if (bookCol == null)
					WriteOutput($"Book column {kBookHeader} not found!", true);
				if (chapterCol == null)
					WriteOutput($"Chapter number column {kChapterHeader} not found!", true);
				if (verseCol == null)
					WriteOutput($"Verse number column {kChapterHeader} not found!", true);
				if (characterCol == null)
					WriteOutput($"Character column {kCharacterHeader} not found!", true);
				if (!allLanguages.Any())
					WriteOutput("No language columns found! (English must exist and must be first)", true);

				if (ErrorsOccurred)
					return null;

				foreach (var textRow in rowData.Skip(rowsToSkip))
				{
					var row = textRow.Key;

					var bookValue = cells[bookCol + row].Value;
					if ("End".Equals(bookValue?.ToString().Trim(), StringComparison.OrdinalIgnoreCase))
					{
						WriteOutput($"Stopping at \"End\" row: {row}");
						break;
					}
					var verseValue = cells[verseCol + row].Value;
					if (verseValue == null)
					{
						WriteOutput($"Stopping at row {row} because verse column contained a null value.");
						break;
					}
					var verseStr = verseValue as string ?? ((double)verseValue).ToString(CultureInfo.InvariantCulture);
					data.ReferenceTextRows.Add(new ReferenceTextRow(
						ConvertFcbhBookCodeToSilBookCode((string)bookValue),
						((double)cells[chapterCol + row].Value).ToString(CultureInfo.InvariantCulture),
						verseStr,
						(string)cells[characterCol + row].Value,
						allLanguages.ToDictionary(kvp => kvp.Key, kvp => cells[kvp.Value + row].Value.ToString())));
				}
			}

			data.AddLanguages(allLanguages.Keys);
			return data;
		}

		public static IReferenceTextProxy GetReferenceTextIdFromString(string language)
		{
			ReferenceTextType type;
			if (Enum.TryParse(language, out type))
			{
				if (type == ReferenceTextType.Custom || type == ReferenceTextType.Unknown)
					throw new ArgumentException("unknown language", nameof(language));
				return ReferenceTextProxy.GetOrCreate(type);
			}

			return ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, language);
		}

		private static ReferenceText GetReferenceTextFromString(string language)
		{
			var refTextId = GetReferenceTextIdFromString(language);
			if (refTextId.Missing)
				throw new ArgumentException("Unvailable custom language", nameof(language));

			return ReferenceText.GetReferenceText(refTextId);
		}

		private static void WriteCharacterMappingFiles(List<CharacterMapping> characterMappings, SortedDictionary<string, SortedSet<string>> glyssenToFcbhIds, SortedDictionary<string, SortedSet<string>> fcbhToGlyssenIds)
		{
			WriteOutput("Writing character mapping files");

			const string kOutputDirForCharacterMapping = @"..\..\DevTools\Resources\temporary";
			const string kOutputFileForCharacterMapping = @"CharacterMappingToFcbh.txt";
			const string kOutputFileForGlyssenToFcbhMultiMap = @"GlyssenToFcbhMultiMap.txt";
			const string kOutputFileForFcbhToGlyssenMultiMap = @"FcbhToGlyssenMultiMap.txt";

			Directory.CreateDirectory(kOutputDirForCharacterMapping);
			var sb = new StringBuilder();
			foreach (CharacterMapping characterMapping in characterMappings)
				sb.Append(characterMapping).Append(Environment.NewLine);
			var path = Path.Combine(kOutputDirForCharacterMapping, kOutputFileForCharacterMapping);
			WriteOutput($"Writing {path}");
			File.WriteAllText(path, sb.ToString());

			sb.Clear();
			foreach (var glyssenToFcbhIdsEntry in glyssenToFcbhIds)
				if (glyssenToFcbhIdsEntry.Value.Count > 1 ||
					CharacterVerseData.IsCharacterOfType(glyssenToFcbhIdsEntry.Key, CharacterVerseData.StandardCharacter.Narrator) ||
					glyssenToFcbhIdsEntry.Value.Any(c => c.StartsWith("Narr_0")))
				{
					sb.Append(string.Format("{0}\t{1}", glyssenToFcbhIdsEntry.Key, glyssenToFcbhIdsEntry.Value.TabSeparated())).Append(Environment.NewLine);
				}
			path = Path.Combine(kOutputDirForCharacterMapping, kOutputFileForGlyssenToFcbhMultiMap);
			WriteOutput($"Writing {path}");
			File.WriteAllText(path, sb.ToString());

			sb.Clear();
			foreach (var fcbhToGlyssenIdsEntry in fcbhToGlyssenIds)
				if (fcbhToGlyssenIdsEntry.Value.Count > 1 ||
					CharacterVerseData.IsCharacterOfType(fcbhToGlyssenIdsEntry.Key, CharacterVerseData.StandardCharacter.Narrator) ||
					fcbhToGlyssenIdsEntry.Value.Any(c => c.StartsWith("Narr_0")))
					sb.Append(string.Format("{0}\t{1}", fcbhToGlyssenIdsEntry.Key, fcbhToGlyssenIdsEntry.Value.TabSeparated())).Append(Environment.NewLine);
			path = Path.Combine(kOutputDirForCharacterMapping, kOutputFileForFcbhToGlyssenMultiMap);
			WriteOutput($"Writing {path}");
			File.WriteAllText(path, sb.ToString());
			WriteOutput("Finished writing character mapping files!");
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
			return s_existingEnglish.Versification.GetLastChapter(BCVRef.BookToNumber(referenceTextRow.Book)) == 1;
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

		private static bool CompareVersions(string excelStr, Block existingBlock, string bookId)
		{
			var existingStr = existingBlock.GetText(true);
			var excelStrWithoutAnnotations = Regex.Replace(excelStr, " \\|\\|\\|.*?\\|\\|\\| ", "");
			excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "{[^0-9]+.*?}", "");

			if (ComparisonSensitivity == Ignore.AllDifferencesExceptAlphaNumericText)
			{
				if (existingStr.Where(c => !Char.IsWhiteSpace(c) && !Char.IsPunctuation(c) && !Char.IsSymbol(c)).SequenceEqual(
					excelStrWithoutAnnotations.Where(c => !Char.IsWhiteSpace(c) && !Char.IsPunctuation(c) && !Char.IsSymbol(c))))
					return true;
			}
			else if (ComparisonSensitivity == Ignore.WhitespaceDifferences)
			{
				if (existingStr.Where(c => !Char.IsWhiteSpace(c)).SequenceEqual(excelStrWithoutAnnotations.Where(c => !Char.IsWhiteSpace(c))))
					return true;
			}
			else
			{
				excelStrWithoutAnnotations = excelStrWithoutAnnotations.Trim();
				excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "  ", " ");
				excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "\u00A0 ", "\u00A0");
				excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "\u00A0\u00A0", "\u00A0");
				//excelStrWithoutAnnotations = Regex.Replace(excelStrWithoutAnnotations, "^ ({\\d+})", "$1");

				string existingStrModified;
				if (ComparisonSensitivity == Ignore.QuotationMarkDifferences)
				{
					existingStrModified = Regex.Replace(s_removeQuotes.Replace(existingStr, ""), "\u00A0 ", "\u00A0").Trim();
					excelStrWithoutAnnotations = s_removeQuotes.Replace(excelStrWithoutAnnotations, "").Trim();
				}
				else
				{
					existingStrModified = existingStr;
				}

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

			WriteOutput($"Difference found in text at {bookId} {existingBlock.ChapterNumber}:{existingBlock.InitialStartVerseNumber} - {lengthComparison}");
			WriteOutput($"   Existing: {existingStr}");
			WriteOutput($"   New:      {excelStr}");
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
			foreach (var referenceTextId in ReferenceTextProxy.AllAvailable.Where(r => r.Type != ReferenceTextType.English))
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

			foreach (var rt in ReferenceTextProxy.AllAvailable.Where(r => r.Type == ReferenceTextType.Custom))
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
