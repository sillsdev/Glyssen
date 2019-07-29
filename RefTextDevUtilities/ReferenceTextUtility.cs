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
		private const string kChapterHeader = "Cp";
		private const string kVerseHeader = "Vs";
		private const string kCharacterHeader = "CHARACTER";
		private const string kCharacterHeaderAlt = "Char";
		private const string kEnglishHeader = "ENGLISH";
		private const string kSporadicAddlCharNamesPrefixIgnore = "Sporadic";
		private const string kPrompts = "Prompts";
		public const string kTempFolderPrefix = "New";

		private static readonly ReferenceText s_existingEnglish;
		private static readonly Regex s_verseNumberMarkupRegex = new Regex("(\\{\\d*?\\}\u00A0)", RegexOptions.Compiled);
		private static readonly Regex s_extractVerseNumberRegex = new Regex("\\{(\\d*?)\\}\u00A0", RegexOptions.Compiled);
		private static Regex s_charactersToExcludeWhenComparing;
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
			DifferencesToIgnore = Ignore.Default;
		}

		public static void WriteOutput(string msg = "", bool error = false)
		{
			ErrorsOccurred |= error;
			OnMessageRaised?.Invoke(msg, error);
			//System.Threading.Thread.Sleep(150);
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
			 */
			GenerateEnglish
		}

		[Flags]
		public enum Ignore
		{
			Nothing = 0,
			WhitespaceDifferences = 1,
			QuotationMarkDifferences = 2,
			Default = QuotationMarkDifferences | WhitespaceDifferences,
			Punctuation = 4,
			Symbols = 8,
			AllDifferencesExceptAlphaNumericText = Punctuation | Symbols | Default,
		}

		public static Ignore DifferencesToIgnore
		{
			get => s_differencesToIgnore;
			set
			{
				s_differencesToIgnore = value;

				if (s_differencesToIgnore == Ignore.Nothing)
				{
					s_charactersToExcludeWhenComparing = null;
					return;
				}

				var sb = new StringBuilder("[]");

				if ((DifferencesToIgnore & Ignore.Symbols) > 0)
					sb.Insert(1, @"\p{S}");
				if ((DifferencesToIgnore & Ignore.Punctuation) > 0)
					sb.Insert(1, @"\p{P}");
				else if ((DifferencesToIgnore & Ignore.QuotationMarkDifferences) > 0)
				{
					sb.Insert(1, "“”\"'\u2018\u2019«»");
					// Not all dashes in all contexts are necessarily quotation dashes, but a double dash at the
					// end of the string apparently is.
					sb.Append("|(--$)");
				}
				if ((DifferencesToIgnore & Ignore.WhitespaceDifferences) > 0)
					sb.Insert(1, @"\s");

				s_charactersToExcludeWhenComparing = new Regex(sb.ToString(), RegexOptions.Compiled);
			}
		}

		public static void ProcessReferenceTextDataFromFile(Mode mode, ReferenceTextProxy refTextId = null)
		{
			var hasNtData = ProcessExcelFile(kDirectorGuideInput, out var ntData);
			var hasOtData = ProcessExcelFile(kDirectorGuideOTInput, out var otData);
			if (!hasNtData || !hasOtData)
				return;

			if (refTextId != null)
			{
				ntData?.FilterBy(refTextId.Name);
				otData?.FilterBy(refTextId.Name);
			}

			try
			{
				ProcessReferenceTextData(mode, ntData, otData, () => refTextId != null ? ReferenceText.GetReferenceText(refTextId) : null);
			}
			catch (Exception e)
			{
				WriteOutput(e.Message);
				//throw;
			}
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
			try
			{
				ProcessReferenceTextData(mode, data, otData, null);
			}
			catch (Exception e)
			{
				WriteOutput(e.Message, true);
				//throw;
			}
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

			ErrorsOccurred = !ntData.IsValid || (otData != null && !otData.IsValid);
			var resultSummary = new List<BookTitleAndChapterLabelInfo>(66);

			var annotationsToOutput = new List<string>();

			List<ReferenceTextLanguageInfo> languagesToProcess;
			if (otData != null)
				languagesToProcess = ntData.LanguagesToProcess.Union(otData.LanguagesToProcess).ToList();
			else
				languagesToProcess = ntData.LanguagesToProcess.ToList();

			for (int iData = 0 + (otData == null ? 1 : 0); iData < 2; iData++)
			{
				languagesToProcess = iData == 0 ? otData.LanguagesToProcess.ToList() : ntData.LanguagesToProcess.ToList();

				foreach (var languageInfo in languagesToProcess)
				{
					if (mode == Mode.GenerateEnglish && !languageInfo.IsEnglish)
						continue;

					var language = languageInfo.Name;
					WriteOutput("Processing " + language + "...");

					ReferenceText existingReferenceTextForLanguage = null;
					if (languageInfo.IsEnglish)
						existingReferenceTextForLanguage = s_existingEnglish;
					else
					{
						try
						{
							existingReferenceTextForLanguage = getReferenceText?.Invoke() ?? GetReferenceTextFromString(language);
						}
						catch (ArgumentException)
						{
							if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
							{
								WriteOutput($"No existing reference text for language {language}", languagesToProcess.Count == 1);
								continue;
							}

							existingReferenceTextForLanguage = null;
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
					bool isFirstBookInTestament = true;
					int iBlockInExistingEnglishRefBook = 0;
					int iBlockInExistingRefBookForLanguage = 0;
					BookScript existingEnglishRefBook = null;
					string chapterLabelForPrevBook = null;
					string justTheWordForChapter = null;
					List<BookScript> existingEnglishRefBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) >= 40).ToList();
					if (iData == 0)
						existingEnglishRefBooks = s_existingEnglish.Books.Where(b => BCVRef.BookToNumber(b.BookId) < 40).ToList();
					List<BookScript> newBooks = new List<BookScript>();
					List<Block> newBlocks = new List<Block>();
					TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo = null;
					IReadOnlyList<Block> existingRefBlocksForLanguage = null;
					int skippingBook = 0;

					GetQuoteMarksForLanguage(language, out var openDoubleQuote, out var closeDoubleQuote, out var openQuoteSingle, out var closeQuoteSingle);

					HashSet<int> existingBookNumbersRequired = null;
					if (mode != Mode.GenerateEnglish && (mode != Mode.Generate || !languageInfo.IsEnglish))
					{
						existingBookNumbersRequired = new HashSet<int>((mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText ?
								existingReferenceTextForLanguage.Books : existingEnglishRefBooks)
							.Select(e => BCVRef.BookToNumber(e.BookId)));
					}

					var referenceTextRowsToProcess = iData == 0 ? otData.ReferenceTextRows : ntData.ReferenceTextRows;

					foreach (var referenceTextRow in referenceTextRowsToProcess.Where(r => !r.CharacterId.StartsWith("Section Head_")))
					{
						var currBookId = GetBookIdFromFcbhBookCode(referenceTextRow.Book, out int currBookNum);

						if (skippingBook == currBookNum)
							continue;

						if (prevBook != currBookId)
						{
							if (prevBook != null)
							{
								isFirstBookInTestament = false;
								newBooks.Add(new BookScript(prevBook, newBlocks) {PageHeader = currentTitleAndChapterLabelInfo.ChapterLabel});
								newBlocks.Clear();
							}

							if (existingBookNumbersRequired != null && !existingBookNumbersRequired.Contains(currBookNum))
							{
								skippingBook = currBookNum;
								if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
									WriteOutput($"Book {currBookId} cannot be compared because it does not exist in {languageInfo.Name} reference text.", true);
								continue;
							}

							skippingBook = 0;

							if (mode != Mode.GenerateEnglish)
								existingEnglishRefBook = existingEnglishRefBooks.FirstOrDefault(b => b.BookId == currBookId);

							if (existingReferenceTextForLanguage != null)
							{
								existingRefBlocksForLanguage = existingReferenceTextForLanguage.Books
									.SingleOrDefault(b => b.BookId == currBookId)?.GetScriptBlocks();
								
								if (existingRefBlocksForLanguage?.Count != existingEnglishRefBook?.GetScriptBlocks().Count)
									WriteOutput($"Existing book of {currBookId} for {language} has different number of blocks than " +
										"the existing English reference text.", true);
							}

							iBlockInExistingEnglishRefBook = 0;
							iBlockInExistingRefBookForLanguage = 0;

							chapterLabelForPrevBook = currentTitleAndChapterLabelInfo?.ChapterLabel;
							prevBook = currBookId;

							currentTitleAndChapterLabelInfo = ProcessTitleAndInitialChapterLabel(currBookId,
								referenceTextRow, language, resultSummary, justTheWordForChapter, newBlocks);
						}

						if (referenceTextRow.Verse == "<<")
						{
							ProcessChapterBreak(referenceTextRow, currentTitleAndChapterLabelInfo, language, ref justTheWordForChapter, chapterLabelForPrevBook, isFirstBookInTestament, newBlocks, currBookId);
							continue;
						}

						if (mode == Mode.CreateBookTitleAndChapterLabelSummary)
							continue;

						int currChapter, currVerse;
						if (!Int32.TryParse(referenceTextRow.Chapter, out currChapter))
							WriteOutput($"Invalid chapter number in {currBookId}: {referenceTextRow.Chapter}", true);
						if (!Int32.TryParse(referenceTextRow.Verse, out currVerse))
							WriteOutput($"Invalid verse number in {currBookId}: {referenceTextRow.Verse}", true);

						Block existingEnglishRefBlock = null;
						if (mode != Mode.GenerateEnglish)
						{
							var blocks = existingEnglishRefBook.GetScriptBlocks();
							if (blocks.Count <= iBlockInExistingEnglishRefBook)
							{
								if (mode == Mode.Generate && !languageInfo.IsEnglish)
								{
									WriteOutput($"Went past end of existing English reference blocks. Skipping {referenceTextRow}");
									continue;
								}
								existingEnglishRefBlock = null;
							}
							else
								existingEnglishRefBlock = blocks[iBlockInExistingEnglishRefBook++];

							while (CharacterVerseData.IsCharacterExtraBiblical(existingEnglishRefBlock.CharacterId))
								existingEnglishRefBlock = blocks[iBlockInExistingEnglishRefBook++];

							// When generating a new English reference text, blocks can be inserted or removed, so we don't
							// necessarily expect it to align verse-by-verse to the old version.
							if (mode != Mode.Generate || !languageInfo.IsEnglish)
							{
								EnsureAlignmentToExistingReferenceText(currBookId, currChapter, currVerse,
									ref existingEnglishRefBlock, blocks, "English", ref iBlockInExistingEnglishRefBook);
							}
						}

						if (mode == Mode.CreateCharacterMapping)
						{
							AddCharacterMappingInfo(existingEnglishRefBlock, referenceTextRow, currBookId, currBookNum,
								characterMappings, glyssenToFcbhIds, fcbhToGlyssenIds);
							continue;
						}

						string originalText = referenceTextRow.GetText(language);

						if (originalText == null)
						{
							WriteOutput($"No {language} text present for {currBookId} {referenceTextRow.Chapter}:{referenceTextRow.Verse} " +
								$"corresponding to the English text: {referenceTextRow.English}", true);
							originalText = string.Empty;
						}

						var verseNumberFixedText = s_verseNumberInExcelRegex.Replace(originalText, "{$1}\u00A0");

						var modifiedText = verseNumberFixedText.Replace("\n ", " ").Replace('\n', ' ');
						modifiedText = s_doubleSingleOpenQuote.Replace(modifiedText, openDoubleQuote + "\u202F" + openQuoteSingle);
						modifiedText = s_singleDoubleOpenQuote.Replace(modifiedText, openQuoteSingle + "\u202F" + openDoubleQuote);
						modifiedText = s_doubleSingleCloseQuote.Replace(modifiedText, closeDoubleQuote + "\u202F" + closeQuoteSingle);
						modifiedText = s_singleDoubleCloseQuote.Replace(modifiedText, closeQuoteSingle + "\u202F" + closeDoubleQuote);
						modifiedText = s_doubleOpenQuote.Replace(modifiedText, openDoubleQuote);
						modifiedText = s_doubleCloseQuote.Replace(modifiedText, closeDoubleQuote);
						modifiedText = s_singleOpenQuote.Replace(modifiedText, openQuoteSingle);
						modifiedText = s_singleCloseQuote.Replace(modifiedText, closeQuoteSingle);
						if (verseNumberFixedText != modifiedText)
							Debug.WriteLine($"{verseNumberFixedText} != {modifiedText}");

						Block existingRefBlockForLanguage;
						if (languageInfo.IsEnglish)
						{
							existingRefBlockForLanguage = existingEnglishRefBlock;
						}
						else
						{
							if (existingRefBlocksForLanguage?.Count <= iBlockInExistingRefBookForLanguage)
							{
								if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
								{
									WriteOutput($"Went past end of existing blocks for {languageInfo.Name}. Skipping {referenceTextRow}");
									continue;
								}
								existingRefBlockForLanguage = null;
							}
							else
							{
								existingRefBlockForLanguage = existingRefBlocksForLanguage[iBlockInExistingRefBookForLanguage++];
								while (CharacterVerseData.IsCharacterExtraBiblical(existingRefBlockForLanguage.CharacterId))
									existingRefBlockForLanguage = existingRefBlocksForLanguage[iBlockInExistingRefBookForLanguage++];
							}

							// If we've gotten out of C/V alignment with the existing reference text, we need to try to
							// get back in sync.
							if (mode == Mode.FindDifferencesBetweenCurrentVersionAndNewText)
							{
								Debug.Assert(existingRefBlockForLanguage != null);
								EnsureAlignmentToExistingReferenceText(currBookId, currChapter, currVerse,
									ref existingRefBlockForLanguage, existingRefBlocksForLanguage,
									languageInfo.Name, ref iBlockInExistingRefBookForLanguage);
							}
						}

						var noChangesWeCareAbout = IsUnchanged(modifiedText, existingRefBlockForLanguage,
							currBookId);

						if (mode != Mode.Generate && mode != Mode.GenerateEnglish)
							continue;

						if (noChangesWeCareAbout)
						{
							newBlocks.Add(existingRefBlockForLanguage);
							foreach (var split in s_verseNumberMarkupRegex.Split(modifiedText)
								.Where(s => !string.IsNullOrWhiteSpace(s) && !s_extractVerseNumberRegex.IsMatch(s)))
							{
								foreach (var s in s_anyAnnotationForSplittingRegex.Split(split).Where(s => !string.IsNullOrWhiteSpace(s)))
								{
									if (s_anyAnnotationRegex.Match(s).Success)
									{
										if (!ConvertTextToUserSpecifiedScriptAnnotationElement(s, out _))
											AttemptParseOfControlFileAnnotation(mode, s, languageInfo, currBookId, referenceTextRow, existingEnglishRefBlock, annotationsToOutput);
									}
									else if (string.IsNullOrWhiteSpace(s.TrimStart()))
										WriteOutput("No text found between annotations:" + referenceTextRow, true);
								}
							}

							continue;
						}

						Block newBlock;
						if (mode != Mode.GenerateEnglish)
						{
							newBlock = new Block(existingEnglishRefBlock.StyleTag, currChapter,
								currVerse)
							{
								CharacterId = existingEnglishRefBlock.CharacterId,
								Delivery = existingEnglishRefBlock.Delivery,
								IsParagraphStart = existingEnglishRefBlock.IsParagraphStart,
								MultiBlockQuote = existingEnglishRefBlock.MultiBlockQuote
							};
						}
						else
						{
							newBlock = new Block("p", currChapter, currVerse);
							newBlock.CharacterId =
								GetCharacterIdFromFCBHCharacterLabel(referenceTextRow.CharacterId, currBookId, newBlock);
						}

						BlockElement lastElementInBlock = null;
						var splits = s_verseNumberMarkupRegex.Split(modifiedText);
						foreach (var split in splits)
						{
							if (string.IsNullOrWhiteSpace(split))
							{
								if (splits.Length == 1)
									newBlock.BlockElements.Add(new ScriptText(" ")); // Blank row should have already be reported as error.
								continue;
							}

							var match = s_extractVerseNumberRegex.Match(split);
							if (match.Success)
							{
								var verseNum = match.Groups[1].Value;
								var processingFirstScrElement = newBlock.BlockElements.Select(be => be.GetType()).All(t => t != typeof(Verse) && t != typeof(ScriptText));
								newBlock.BlockElements.Add(lastElementInBlock = new Verse(verseNum));
								if (processingFirstScrElement)
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
								foreach (var s in s_anyAnnotationForSplittingRegex.Split(split).Where(s => !string.IsNullOrWhiteSpace(s)))
								{
									if (s_anyAnnotationRegex.Match(s).Success)
									{
										if (ConvertTextToUserSpecifiedScriptAnnotationElement(s, out var annotation))
										{
											newBlock.BlockElements.Add(lastElementInBlock = annotation);
											//Debug.WriteLine(newBlock.ToString(true, existingBook.BookId) + " (" + annotation.ToDisplay + ")");
										}
										else
											AttemptParseOfControlFileAnnotation(mode, s, languageInfo, currBookId, referenceTextRow, existingEnglishRefBlock, annotationsToOutput);
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

					if (mode == Mode.CreateCharacterMapping)
					{
						WriteCharacterMappingFiles(characterMappings, glyssenToFcbhIds, fcbhToGlyssenIds);
						return;
					}

					if (mode == Mode.Generate || mode == Mode.GenerateEnglish)
					{
						newBooks.Add(new BookScript(newBlocks[0].BookCode, newBlocks) {PageHeader = currentTitleAndChapterLabelInfo.ChapterLabel});

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
			}

			if (!ErrorsOccurred && mode == Mode.CreateBookTitleAndChapterLabelSummary)
			{
				WriteTitleAndChapterSummaryResults(resultSummary);
			}

			WriteOutput("Done!");
		}

		private static void EnsureAlignmentToExistingReferenceText(string currBookId, int currChapter, int currVerse, ref Block existingRefBlock, IReadOnlyList<Block> blocks, string languageName, ref int iBlock)
		{
			if (currChapter != existingRefBlock.ChapterNumber)
			{
				WriteOutput($"Chapter does not align with existing {languageName} reference text. Book: {currBookId}, " +
					$"Excel: {currChapter}, Existing: {existingRefBlock.ChapterNumber}", true);

				if (existingRefBlock.ChapterNumber > currChapter)
				{
					do
					{
						WriteOutput("   Backing up to earlier existing ref text block.");
						iBlock -= 2; // We already incremented this, so we have to go back 2 to get to the previous one.
						existingRefBlock = blocks[iBlock++];
					} while (existingRefBlock.ChapterNumber > currChapter ||
						CharacterVerseData.IsCharacterExtraBiblical(existingRefBlock.CharacterId));
				}
				else
				{
					do
					{
						WriteOutput($"   Skipping past existing ref text block: {existingRefBlock}");
						existingRefBlock = blocks[iBlock++];
					} while (existingRefBlock.ChapterNumber < currChapter ||
						CharacterVerseData.IsCharacterExtraBiblical(existingRefBlock.CharacterId));
				}

				WriteOutput($"   Aligned to existing {languageName} ref text block: {existingRefBlock}");
			}

			if (currVerse != existingRefBlock.InitialStartVerseNumber)
			{
				WriteOutput($"Verse number does not align with existing {languageName} reference text. Book: {currBookId}, Ch: {currChapter}, " +
					$"Excel: {currVerse}, Existing: {existingRefBlock.InitialStartVerseNumber}", true);

				if (existingRefBlock.InitialStartVerseNumber > currVerse)
				{
					do
					{
						WriteOutput("   Backing up to earlier existing ref text block.");
						iBlock -= 2; // We already incremented this, so we have to go back 2 to get to the previous one.
						existingRefBlock = blocks[iBlock++];
					} while (existingRefBlock.InitialStartVerseNumber > currVerse ||
						CharacterVerseData.IsCharacterExtraBiblical(existingRefBlock.CharacterId));
				}
				else
				{
					do
					{
						WriteOutput($"   Skipping past existing ref text block: {existingRefBlock}");
						existingRefBlock = blocks[iBlock++];
					} while (existingRefBlock.InitialStartVerseNumber < currVerse ||
						CharacterVerseData.IsCharacterExtraBiblical(existingRefBlock.CharacterId));
				}

				WriteOutput($"   Aligned to existing {languageName} ref text block: {existingRefBlock}");
			}
		}

		private static void AddCharacterMappingInfo(Block existingEnglishRefBlock, ReferenceTextRow referenceTextRow, string currBookId, int currBookNum, List<CharacterMapping> characterMappings, SortedDictionary<string, SortedSet<string>> glyssenToFcbhIds, SortedDictionary<string, SortedSet<string>> fcbhToGlyssenIds)
		{
			if (!CharacterVerseData.IsCharacterOfType(existingEnglishRefBlock.CharacterId,
				CharacterVerseData.StandardCharacter.Narrator) || !referenceTextRow.CharacterId.StartsWith("Narr_0"))
			{
				var verse = new BCVRef(currBookNum, int.Parse(referenceTextRow.Chapter),
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
		}

		private static void ProcessChapterBreak(ReferenceTextRow referenceTextRow, TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo, string language, ref string justTheWordForChapter, string chapterLabelForPrevBook, bool isFirstBookInTestament, List<Block> newBlocks, string currBookId)
		{
			int chapter = int.Parse(referenceTextRow.Chapter);
			if (chapter == 2)
			{
				currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls = referenceTextRow.GetText(language);
				var chapterLabelForCurrentBook = currentTitleAndChapterLabelInfo.ChapterTwoInfoFromXls.TrimEnd(' ', '2');
				if (justTheWordForChapter == null && chapterLabelForPrevBook != null && !isFirstBookInTestament)
				{
					// We're going to try to find just the word for chapter in case we later hit a single-chapter book that doesn't have it.
					int iStartOfWord = chapterLabelForPrevBook.Length;
					int i = iStartOfWord;
					int j = chapterLabelForCurrentBook.Length;
					while (--i > 0 && --j > 0)
					{
						if (chapterLabelForPrevBook[i] != chapterLabelForCurrentBook[j])
							break;
						if (chapterLabelForPrevBook[i] == ' ')
							iStartOfWord = i + 1;
					}

					if (iStartOfWord > 0 && iStartOfWord < chapterLabelForPrevBook.Length - 2)
						justTheWordForChapter = chapterLabelForPrevBook.Substring(iStartOfWord);
				}

				if (justTheWordForChapter != null)
					JustGetHalfOfRepeated(ref chapterLabelForCurrentBook, justTheWordForChapter);

				string chapterLabel = chapterLabelForCurrentBook;

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

			var chapterBlock = new Block("c", chapter)
			{
				CharacterId =
					CharacterVerseData.GetStandardCharacterId(currBookId,
						CharacterVerseData.StandardCharacter.BookOrChapter),
				IsParagraphStart = true,
				BookCode = currBookId
			};
			chapterBlock.BlockElements.Add(new ScriptText(referenceTextRow.Chapter));
			newBlocks.Add(chapterBlock);
		}

		private static TitleAndChapterLabelInfo ProcessTitleAndInitialChapterLabel(string currBookId, ReferenceTextRow referenceTextRow, string language, List<BookTitleAndChapterLabelInfo> resultSummary, string justTheWordForChapter, List<Block> newBlocks)
		{
			TitleAndChapterLabelInfo currentTitleAndChapterLabelInfo;
			string chapterLabel = null;

			var newBlock = new Block("mt")
			{
				BookCode = currBookId,
				CharacterId = CharacterVerseData.GetStandardCharacterId(currBookId,
					CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			var bookTitleAndChapter1Announcement = referenceTextRow.GetText(language);
			var summaryForBook = resultSummary.SingleOrDefault(b => b.BookId == currBookId);
			if (summaryForBook == null)
			{
				summaryForBook = new BookTitleAndChapterLabelInfo {BookId = currBookId};
				resultSummary.Add(summaryForBook);
			}

			currentTitleAndChapterLabelInfo = new TitleAndChapterLabelInfo
			{
				Language = language,
				TitleAndChapterOneInfoFromXls = bookTitleAndChapter1Announcement
			};
			summaryForBook.Details.Add(currentTitleAndChapterLabelInfo);

			var bookName = bookTitleAndChapter1Announcement.TrimEnd(' ', '1').TrimStart(' ');

			if (!JustGetHalfOfRepeated(ref bookName) && IsSingleChapterBook(currBookId))
			{
				var iFirstSpace = bookTitleAndChapter1Announcement.IndexOf(" ", StringComparison.Ordinal);
				if (iFirstSpace > 0)
				{
					var firstWord = bookTitleAndChapter1Announcement.Substring(0, iFirstSpace);
					var iStartOfChapterAnnouncement = bookTitleAndChapter1Announcement.IndexOf(firstWord,
						iFirstSpace, StringComparison.Ordinal);
					if (iStartOfChapterAnnouncement > 0)
					{
						bookName = bookTitleAndChapter1Announcement.Substring(0, iStartOfChapterAnnouncement).TrimEnd();
						chapterLabel = bookTitleAndChapter1Announcement.Substring(iStartOfChapterAnnouncement);
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
			return currentTitleAndChapterLabelInfo;
		}

		private static void AttemptParseOfControlFileAnnotation(Mode mode, string s, ReferenceTextLanguageInfo languageInfo, string bookId, ReferenceTextRow referenceTextRow, Block existingEnglishRefBlock, List<string> annotationsToOutput)
		{
			if (ConvertTextToControlScriptAnnotationElement(s, out var annotation))
			{
				if (mode != Mode.GenerateEnglish && languageInfo.IsEnglish)
				{
					var pause = annotation as Pause;
					var serializedAnnotation = pause != null
						? XmlSerializationHelper.SerializeToString(pause, true)
						: XmlSerializationHelper.SerializeToString((Sound)annotation, true);

					var formattedAnnotationForDisplay = annotation.ToDisplay();

					if (string.IsNullOrWhiteSpace(formattedAnnotationForDisplay) ||
						string.IsNullOrWhiteSpace(serializedAnnotation))
					{
						WriteOutput($"Annotation not formatted correctly (is null or whitespace): {referenceTextRow.English}",
							true);
						WriteOutput();
					}

					var trimmedEnglish = referenceTextRow.English.Replace("\n ", " ").Replace('\n', ' ').TrimEnd();
					if ((annotation is Pause && !trimmedEnglish.EndsWith(formattedAnnotationForDisplay)) ||
						(annotation is Sound && !trimmedEnglish.StartsWith(formattedAnnotationForDisplay)))
					{
						// Although this is a good check to run for sanity, we can't treat it as an error
						// because a few of the annotations are actually displayed slightly differently by
						// FCBH (due to what are insignificant differences like 'before' vs. '@')
						var bcv = new BCVRef(BCVRef.BookToNumber(bookId),
							existingEnglishRefBlock.ChapterNumber, existingEnglishRefBlock.InitialStartVerseNumber);
						WriteOutput(
							$"(warning) Annotation not formatted the same as FCBH: ({bcv.AsString}) {trimmedEnglish} \r\n=> {formattedAnnotationForDisplay}");
						WriteOutput();
					}

					int offset = 0;
					if ((bookId == "MRK" && existingEnglishRefBlock?.ChapterNumber == 4 &&
							existingEnglishRefBlock.InitialVerseNumberOrBridge == "39") ||
						(bookId == "ACT" && existingEnglishRefBlock?.ChapterNumber == 10 &&
							existingEnglishRefBlock.InitialVerseNumberOrBridge == "23"))
					{
						offset = -1;
					}

					annotationsToOutput.Add(bookId + "\t" + existingEnglishRefBlock.ChapterNumber +
						"\t" +
						existingEnglishRefBlock.InitialVerseNumberOrBridge + "\t" + offset + "\t" + serializedAnnotation);
				}
			}
			else
			{
				WriteOutput("Could not parse annotation: " + referenceTextRow, true);
			}
		}

		private static string GetBookIdFromFcbhBookCode(string fcbhBookCode, out int bookNum)
		{
			bookNum = BCVRef.BookToNumber(fcbhBookCode);
			if (bookNum > 0)
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
				default:
					WriteOutput($"Unexpected Book code: {fcbhBookCode}", true);
					return fcbhBookCode;
			}

			bookNum = BCVRef.BookToNumber(silBookCode);
			return silBookCode;
		}

		private static string GetCharacterIdFromFCBHCharacterLabel(string fcbhCharacterLabel, string bookId, Block block)
		{
			if (s_FcbhNarrator.IsMatch(fcbhCharacterLabel))
				return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

			fcbhCharacterLabel = Regex.Replace(fcbhCharacterLabel, "(.*)-FX\\d+", "$1");
			fcbhCharacterLabel = Regex.Replace(fcbhCharacterLabel, "(.*) \\(female\\)", "$1");
			fcbhCharacterLabel = Regex.Replace(fcbhCharacterLabel, "(.*) #\\d+", "$1");
			fcbhCharacterLabel = Regex.Replace(fcbhCharacterLabel, "(.*)_\\d+", "$1");

			//if (CharacterDetailData.Singleton.GetAllCharacterIdsAsLowerInvariant().Contains(characterId.ToLowerInvariant()))
			//	return characterId;
			//if (TryGetKnownNameMatch(characterId, out var knownMatch))
			//	return knownMatch;

			var bookNum = BCVRef.BookToNumber(bookId);
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(bookNum, block.ChapterNumber, block.InitialStartVerseNumber, block.InitialEndVerseNumber, block.LastVerseNum);
			var charactersCount = characters.Count();
			var bcvRef = new BCVRef(bookNum, block.ChapterNumber, block.InitialStartVerseNumber);
			if (charactersCount == 1)
			{
				// These three lines ensure we are handling characterIds with slashes correctly.
				block.CharacterId = characters.Single().Character;
				block.UseDefaultForMultipleChoiceCharacter(bookNum);
				var characterIdToUse = block.CharacterIdInScript;

				if (characterIdToUse.ToLowerInvariant().StartsWith(fcbhCharacterLabel.ToLowerInvariant()))
				{
					// Don't bother reporting; these are not interesting
				}
				else
				{
					s_characterIdsMatchedByControlFile.Add(new Tuple<string, string, BCVRef, string>(fcbhCharacterLabel, characterIdToUse, bcvRef, block.CharacterId));
					s_characterIdsMatchedByControlFileStr.Add($"{fcbhCharacterLabel} => {characterIdToUse} -- {bcvRef}");
				}

				return characterIdToUse;
			}
			else if (charactersCount == 0)
			{
				//if (CharacterDetailData.Singleton.GetAllCharacterIdsAsLowerInvariant().Contains(characterId.ToLowerInvariant()))
				//	return characterId;
				//if (TryGetKnownNameMatch(characterId, out var knownMatch))
				//	return knownMatch;
				if (TryGetDocumentedUnknownCharacter(fcbhCharacterLabel, bookId, block.ChapterNumber, block.InitialStartVerseNumber, out var documentedCharacterId))
					return documentedCharacterId;
				s_unmatchedCharacterIds.Add(new Tuple<string, BCVRef, string>(CharacterVerseData.kUnknownCharacter, bcvRef, fcbhCharacterLabel));
				//return CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
				return CharacterVerseData.kUnknownCharacter;
			}
			else
			{
				s_unmatchedCharacterIds.Add(new Tuple<string, BCVRef, string>(CharacterVerseData.kAmbiguousCharacter, bcvRef, fcbhCharacterLabel));
				return CharacterVerseData.kAmbiguousCharacter;
			}
		}

		private static bool TryGetKnownNameMatch(string characterId, out string knownMatch)
		{
			switch (characterId)
			{
				case "Solomon":
					knownMatch = "Solomon, king";
					return true;
				case "Ezra":
					knownMatch = "Ezra, priest and teacher";
					return true;
				case "Zechariah (son of Berekiah)":
					knownMatch = "Zechariah";
					return true;
			}

			knownMatch = null;
			return false;
		}

		private static bool TryGetDocumentedUnknownCharacter(string characterId, string bookId, int chapter, int verse, out string documentedCharacterId)
		{
			if (characterId == "Ambassador" && bookId == "ISA" && chapter == 18 && verse == 2)
			{
				// It really doesn't make sense for this to be a quote. Just make it narrator.
				documentedCharacterId = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
				return true;
			}

			documentedCharacterId = null;
			return false;
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
				var worksheet = xls.Workbook.Worksheets["Main DG"] ?? xls.Workbook.Worksheets.First();

				string bookCol = null, chapterCol = null, verseCol = null, characterCol = null;

				var cells = worksheet.Cells;
				var rowData = cells.GroupBy(c => c.Start.Row).ToList();
				int rowsToSkip = -1;
				int languageHeaderRow = -1;
				for (int iRow = 0; iRow < rowData.Count && (bookCol == null || languageHeaderRow == -1); iRow++)
				{
					var nonNullCellAddresses = rowData[iRow].Where(er => !String.IsNullOrWhiteSpace(cells[er.Address].Value?.ToString())).ToList();
					var iCol = nonNullCellAddresses.FindIndex(a => cells[a.Address].Value.Equals(kBookHeader) || cells[a.Address].Value.Equals(kBookHeaderAlt));
					if (iCol >= 0)
					{
						var bookAddress = nonNullCellAddresses[iCol].Address;
						bookCol = GetColumnFromCellAddress(bookAddress);
					}
					else
					{
						// Language headers can now be in a row BEFORE the other column headers
						// English is required and must come first!
						iCol = nonNullCellAddresses.FindIndex(a => cells[a.Address].Value.Equals(kEnglishHeader));
						if (iCol >= 0)
						{
							var englishAddress = nonNullCellAddresses[iCol].Address;
							allLanguages[NormalizeLanguageColumnHeaderName(kEnglishHeader)] = GetColumnFromCellAddress(englishAddress);
							languageHeaderRow = iRow;
						}
						else
							continue;
					}

					bool doneProcessingColumns = false;
					foreach (var excelRange in nonNullCellAddresses.Skip(iCol + 1))
					{
						var value = cells[excelRange.Address].Value?.ToString();
						if (value == null)
							continue;
						var iBreak = value.IndexOfAny(new[] {'\n', '\r'});
						if (iBreak > 0)
							value = value.Substring(0, iBreak);
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
							case kCharacterHeaderAlt:
								characterCol = col;
								break;
							case kEnglishHeader: // English is required and must come first!
								allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
								languageHeaderRow = iRow;
								break;
							case kPrompts:
								if (languageHeaderRow != iRow || allLanguages.Any())
									doneProcessingColumns = true;
								break;
							case "":
								if (allLanguages.Any() && languageHeaderRow == iRow)
									doneProcessingColumns = true;
								break;
							default:
								if (!value.Any(Char.IsLetter) || value.StartsWith(kSporadicAddlCharNamesPrefixIgnore))
								{
									if (allLanguages.Any() && languageHeaderRow == iRow)
										doneProcessingColumns = true;
								}
								else
								{
									if (allLanguages.Any() && languageHeaderRow == iRow) // Any other columns before English will be ignored
										allLanguages[NormalizeLanguageColumnHeaderName(value)] = col;
								}
								break;
						}

						if (doneProcessingColumns)
							break;
					}

					rowsToSkip = iRow + 1;
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
					var verseValue = cells[verseCol + row].Value;
					if (verseValue == null)
					{
						WriteOutput("Stopping at row " + row + " because verse column contained a null value.");
						break;
					}
					var verseStr = verseValue as string ?? ((double)verseValue).ToString(CultureInfo.InvariantCulture);
					data.ReferenceTextRows.Add(new ReferenceTextRow(
						ConvertFcbhBookCodeToSilBookCode((string)cells[bookCol + row].Value),
						((double)cells[chapterCol + row].Value).ToString(CultureInfo.InvariantCulture),
						verseStr,
						(string)cells[characterCol + row].Value,
						allLanguages.ToDictionary(kvp => kvp.Key, kvp => cells[kvp.Value + row].Value?.ToString())));
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
			if (!Directory.Exists(Path.GetDirectoryName(kOutputFileForAnnotations)))
			{
				WriteOutput($"Could not write file {kOutputFileForAnnotations}. If annotations have changed, this needs to be run by a developer. ");
				return;
			}
			var sb = new StringBuilder();
			foreach (string annotation in annotationsToOutput)
				sb.Append(annotation).Append(Environment.NewLine);
			File.WriteAllText(kOutputFileForAnnotations, sb.ToString());
		}

		private static bool IsSingleChapterBook(string bookId)
		{
			return s_existingEnglish.Versification.GetLastChapter(BCVRef.BookToNumber(bookId)) == 1;
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

		private static bool IsUnchanged(string excelStr, Block existingBlock, string bookId)
		{
			if (existingBlock == null)
				return false; // This will have already been reported.
			// Even when not ignoring (internal) whitespace differences, we probably want to ignore leading
			// and trailing whitespace differences
			var existingStr = existingBlock.GetText(true).Trim();
			var excelStrWithoutAnnotations = s_annotationDelimitedWith3VerticalBarsRegex.Replace(excelStr, "");
			excelStrWithoutAnnotations = s_annotationInCurlyBracesRegex.Replace(excelStrWithoutAnnotations, "").Trim();
			var excelStrToCompare = excelStrWithoutAnnotations;

			var existingStrToCompare = existingStr;
			if (s_charactersToExcludeWhenComparing != null)
			{
				existingStrToCompare = s_charactersToExcludeWhenComparing.Replace(existingStr, "");
				excelStrToCompare = s_charactersToExcludeWhenComparing.Replace(excelStrToCompare, "");
			}

			var indexOfFirstDifference = DiffersAtIndex(excelStrToCompare, existingStrToCompare);
			if (indexOfFirstDifference == -1)
				return true;

			string lengthComparison;
			int lengthChange = existingStr.Length - excelStrWithoutAnnotations.Length;
			if (lengthChange == 0)
				lengthComparison = "String lengths identical";
			else if (lengthChange < 0)
				lengthComparison = $"String length grew by {Math.Abs(lengthChange)} characters";
			else
				lengthComparison = $"String length shrunk by {lengthChange} characters";

			WriteOutput($"Difference found in text at {bookId} {existingBlock.ChapterNumber}:{existingBlock.InitialStartVerseNumber} - {lengthComparison}");
			WriteOutput($"   Existing:   {existingStr.Trim()}");
			WriteOutput($"   New:        {excelStrWithoutAnnotations.Trim()}");
#if DEBUG
			WriteOutput($"   Difference: {new string(' ', indexOfFirstDifference)}^"); // This will be off!
#endif
			// ------------------------------------------
			// Put a breakpoint here to investigate differences
			// | | | |
			// V V V V
			return false;
		}

		/// <summary>
		/// Compare two strings and return the index of the first difference.  Return -1 if the strings are equal.
		/// </summary>
		private static int DiffersAtIndex(string s1, string s2)
		{
			int index = 0;
			int min = Math.Min(s1.Length, s2.Length);
			while (index < min && s1[index] == s2[index])
				index++;

			return (index == min && s1.Length == s2.Length) ? -1 : index;
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

		private static string RegexEscapedDoNotCombine => Regex.Escape(Sound.kDoNotCombine) + " ";

		private const string k3Bars = @"\|\|\|";
		private static readonly Regex s_annotationInCurlyBracesRegex = new Regex("{[^0-9]+.*?}", RegexOptions.Compiled);
		private static readonly Regex s_annotationDelimitedWith3VerticalBarsRegex = new Regex($" {k3Bars}.*?{k3Bars} ", RegexOptions.Compiled);
		private static readonly Regex s_anyAnnotationRegex = new Regex($"{RegexEscapedDoNotCombine}{s_annotationInCurlyBracesRegex}|{s_annotationInCurlyBracesRegex}|{s_annotationDelimitedWith3VerticalBarsRegex}", RegexOptions.Compiled);
		// For splitting, the regular expression is wrapped in parentheses to tell Split method to include the capturing group as one of the strings in the resulting array.
		// See explanation here: https://stackoverflow.com/questions/27999449/c-sharp-regex-match-vs-split-for-same-string
		private static readonly Regex s_anyAnnotationForSplittingRegex = new Regex($"({s_anyAnnotationRegex})", RegexOptions.Compiled);

		private static readonly Regex s_doNotCombineRegex = new Regex(RegexEscapedDoNotCombine, RegexOptions.Compiled);
		private static readonly Regex s_pauseRegex = new Regex($"{k3Bars} \\+ ([\\d\\.]*?) SECs {k3Bars}", RegexOptions.Compiled);
		private static readonly Regex s_pauseMinuteRegex = new Regex($"{k3Bars} \\+ ([\\d\\.]*?) MINUTES? {k3Bars}", RegexOptions.Compiled);
		private static readonly Regex s_musicEndRegex = new Regex("{Music--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicStartRegex = new Regex("{Music--Starts @ v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicStopAndStartRegex = new Regex("{Music--Ends & New Music--Starts @ v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxStartRegex = new Regex("{SFX--(.*?)(?:--Starts)? (?:@|before) v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxEndRegex = new Regex("{SFX--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxEndRegex2 = new Regex("{SFX--(.*?)--Ends before v(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_sfxRangeRegex = new Regex("{SFX--(.*?) @ v(\\d*?)-(\\d*?)}", RegexOptions.Compiled);
		private static readonly Regex s_musicSfxRegex = new Regex("{Music \\+ SFX--(.*?) Starts? @ v(\\d*?)}", RegexOptions.Compiled);
		private static Ignore s_differencesToIgnore;

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
