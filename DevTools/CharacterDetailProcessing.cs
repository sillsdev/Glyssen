using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Glyssen.Character;
using GlyssenEngine.Character;
using SIL.Scripture;
using static System.String;

namespace DevTools
{
	class CharacterDetailProcessing
	{
		private const char kTab = '\t';
		private static readonly Regex s_regexContentBeforeAndAfterReference = new Regex(@"(?<precedingPart>([^\t]*\t){6})[^\t]*(?<followingPart>.*)", RegexOptions.Compiled);

		public static void GenerateReferences()
		{
			var data = ReadFile();
//			data.Sort((x, y) => String.Compare(x.CharacterId, y.CharacterId, StringComparison.Ordinal));
			PopulateReferences(data);
			WriteFile(Stringify(data));
		}

		private static void PopulateReferences(List<CharacterDetailLine> lines)
		{
			// ENHANCE: Get info from NarratorOverrides as well. The information there
			// would not currently allow us to indicate exactly how many occurrences
			// there are because it is done using ranges and we do not necessarily
			// expect that every verse in the range could/would be assigned to that
			// character. Perhaps we could tack on something like:
			// ; Also used as narrator override in PSA, JER, EZK.
			// But how useful would that information really be?

			Dictionary<string, List<BCVRef>> dictionaryOfCharactersToReferences =
				ControlCharacterVerseData.Singleton.GetAllQuoteInfo().SelectMany(cv => cv.Character.Split('/')
				.Select(c => new Tuple<string, GlyssenEngine.Character.CharacterVerse>(c, cv)))
				.GroupBy(t => t.Item1, t => t.Item2).ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());

			foreach (var line in lines)
			{
				if (dictionaryOfCharactersToReferences.TryGetValue(line.CharacterId, out var bcvRefs))
				{
					switch (bcvRefs.Count)
					{
						case 0:
							continue;
						case 1:
							line.ReferenceComment = bcvRefs.Min().ToString();
							break;
						case 2:
							line.ReferenceComment = bcvRefs.Min() + " & " + bcvRefs.Max();
							break;
						default:
							line.ReferenceComment = bcvRefs.Min() + " <-(" + (bcvRefs.Count - 2) + " more)-> " + bcvRefs.Max();
							break;
					}
				}
			}
		}

		private static string Stringify(List<CharacterDetailLine> lines)
		{
			return Join(Environment.NewLine, lines.Select(GetNewLine));
		}

		private static string GetNewLine(CharacterDetailLine line)
		{
			var match = s_regexContentBeforeAndAfterReference.Match(line.CurrentLine);
			if (!match.Success)
				throw new ArgumentException($"Invalid input: {line.CurrentLine}", nameof(line));

			var lineWithReferenceComment = match.Result("${precedingPart}") + line.ReferenceComment + match.Result("${followingPart}");
			return lineWithReferenceComment;
		}

		private static void WriteFile(string fileText)
		{
			// Note: Keeping the "Status" column around in case we want to bring it back, but it is currently always empty and
			// always ignored in Glyssen.
			fileText = "#Character ID\tMax Speakers\tGender\tAge\tStatus\tComment\tReference\tFCBH Character" + Environment.NewLine + fileText;
			File.WriteAllText("..\\..\\Glyssen\\Resources\\CharacterDetail.txt", fileText);
		}

		private static List<CharacterDetailLine> ReadFile()
		{
			return File.ReadAllLines("..\\..\\Glyssen\\Resources\\CharacterDetail.txt").Select(ReadLine).Where(l => l != null).ToList();
		}

		private static CharacterDetailLine ReadLine(string line)
		{
			if (line.StartsWith("#"))
				return null;

			string characterId = line.Substring(0, line.IndexOf(kTab));
			return new CharacterDetailLine
			{
				CharacterId = characterId,
				CurrentLine = line
			};
		}

		private class CharacterDetailLine
		{
			public string CharacterId { get; set; }
			public string CurrentLine { get; set; }
			public string ReferenceComment { get; set; }
		}

		public static void GetAllRangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacterNotMarkedAsImplicit()
		{
			string prevBook = "";
			int prevChapter = 0;
			int prevVerse = -1;
			int firstVerseOfRange = -1;
			string prevCharacter = "";
			string prevDelivery = "";
			string prevDefaultCharacter = "";
			string prevAlias = "";
			string prevParallelPassageReferences = "";

			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SoftDev", "Glyssen",
				"RangesOfThreeOrMoreConsecutiveVersesWithTheSameSingleCharacter.txt");

			using (StreamWriter writer = new StreamWriter(filePath))
			{
				foreach (var quote in ControlCharacterVerseData.Singleton.GetAllQuoteInfo().Where(q => q.QuoteType == QuoteType.Normal || q.QuoteType == QuoteType.Dialogue))
				{
					bool sameBookAndChapter = prevBook == quote.BookCode && prevChapter == quote.Chapter;
					if (sameBookAndChapter && prevVerse + 1 == quote.Verse &&
						prevCharacter == quote.Character && prevDelivery == quote.Delivery && prevDefaultCharacter == quote.DefaultCharacter)
					{
						prevVerse = quote.Verse;
					}
					else
					{
						bool thisVerseHasMultipleSpeakersOrDeliveries = sameBookAndChapter && prevVerse == quote.Verse;
						if (thisVerseHasMultipleSpeakersOrDeliveries)
							prevVerse--;

						if (prevVerse - firstVerseOfRange >= 2)
						{
							for (int i = firstVerseOfRange; i <= prevVerse; i++)
							{
								writer.Write(prevBook);
								writer.Write("\t");
								writer.Write(prevChapter);
								writer.Write("\t");
								writer.Write(i);
								writer.Write("\t");
								writer.Write(prevCharacter);
								writer.Write("\t");
								writer.Write(prevDelivery);
								writer.Write("\t");
								writer.Write(prevAlias);
								writer.Write("\tImplicit\t");
								writer.Write(prevDefaultCharacter);
								writer.Write("\t");
								writer.WriteLine(prevParallelPassageReferences);
							}
						}
						if (thisVerseHasMultipleSpeakersOrDeliveries)
						{
							firstVerseOfRange = prevVerse = quote.Verse + 1;
							prevCharacter = "";
							prevDelivery = "";
							prevDefaultCharacter = "";
							prevAlias = "";
							prevParallelPassageReferences = "";
						}
						else if (!sameBookAndChapter || quote.Verse >= prevVerse)
						{
							if (!sameBookAndChapter)
							{
								prevBook = quote.BookCode;
								prevChapter = quote.Chapter;
							}
							firstVerseOfRange = prevVerse = quote.Verse;
							prevCharacter = quote.Character;
							prevDelivery = quote.Delivery;
							prevDefaultCharacter = quote.DefaultCharacter;
							prevAlias = quote.Alias;
							prevParallelPassageReferences = quote.ParallelPassageReferences;
						}
					}
				}
			}
		}
	}
}
