using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Scripture;
using Waxuquerque.Character;

namespace DevTools
{
	class CharacterDetailProcessing
	{
		private const char kTab = '\t';
		private static readonly Regex s_findTabsRegex = new Regex("\t", RegexOptions.Compiled);

		public static void GenerateReferences()
		{
			var data = ReadFile();
//			data.Sort((x, y) => String.Compare(x.CharacterId, y.CharacterId, StringComparison.Ordinal));
			PopulateReferences(data);
			WriteFile(Stringify(data));
		}

		private static void PopulateReferences(List<CharacterDetailLine> lines)
		{
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = false;
			Dictionary<string, List<BCVRef>> dictionaryWithHypotheticals = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().GroupBy(c => c.Character).ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());
			ControlCharacterVerseData.ReadHypotheticalAsNarrator = true;
			Dictionary<string, List<BCVRef>> dictionaryWithoutHypotheticals = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().GroupBy(c => c.Character).ToDictionary(c => c.Key, cv => cv.Select(c => c.BcvRef).ToList());
			Dictionary<string, List<BCVRef>> dictionaryOfDefaultCharacters = ControlCharacterVerseData.Singleton.GetAllQuoteInfo().GroupBy(c => c.DefaultCharacter).ToDictionary(dc => dc.Key, cv => cv.Select(c => c.BcvRef).ToList());

			foreach (var line in lines)
			{
				if (dictionaryWithHypotheticals.TryGetValue(line.CharacterId, out var bcvRefs))
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

					if (!dictionaryWithoutHypotheticals.TryGetValue(line.CharacterId, out bcvRefs) &&
						!dictionaryOfDefaultCharacters.TryGetValue(line.CharacterId, out bcvRefs))
					{
						line.HypotheticalOnly = true;
					}
				}
			}
		}

		private static string Stringify(List<CharacterDetailLine> lines)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var line in lines)
				sb.Append(GetNewLine(line)).Append(Environment.NewLine);
			return sb.ToString();
		}

		private static string GetNewLine(CharacterDetailLine line)
		{
			// Just making sure...
			if (s_findTabsRegex.Matches(line.CurrentLine).Count != 7)
				throw new ArgumentException();

			var match = Regex.Match(line.CurrentLine, @"\t[^\t]*\t", RegexOptions.RightToLeft);
			var lineWithReferenceComment = line.CurrentLine.Substring(0, match.Index + 1) + line.ReferenceComment;

			return $"{lineWithReferenceComment}\t{line.HypotheticalOnly}";
		}

		private static void WriteFile(string fileText)
		{
			fileText = "#Character ID\tMax Speakers\tGender\tAge\tStatus\tComment\tReference Comment\tHypothetical Only" + Environment.NewLine + fileText;
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
			public bool HypotheticalOnly { get; set; }
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
