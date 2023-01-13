using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using InMemoryTestPersistence;
using SIL.Scripture;
using static System.String;

namespace DevTools
{
	internal class CharacterVerseUpdater
	{
		private static Regex sRegexSkipMultiVerseLine = new Regex(@"\t\d+-\d+\t");
		private const string kTab = "\t";

		public static void MakeNormalQuotesImplicit()
		{
			var persistenceImpl = new PersistenceImplementation();
			ProjectBase.Reader = ReferenceTextProxy.Reader = persistenceImpl;
			Project.Writer = persistenceImpl;

			int versionNumber = ControlCharacterVerseData.Singleton.ControlFileVersion + 1;

			const int numberOfStdTexts = 3;

			var standardTexts = new Project[numberOfStdTexts];
			int i = 0;

			foreach (var text in new[] { "CEVUS06", "NVI-S", "ESVUS16" })
			{
				var wrapper = new ParatextUnzippedResourceWrapper(text);
				standardTexts[i++] = new Project(wrapper);
			}

			do
			{
				Thread.Sleep(500);
			} while (standardTexts.Any(p => p.ProjectState != ProjectState.FullyInitialized));

			var currentBook = new BookScript[numberOfStdTexts];

			var sb = new StringBuilder("Control File Version\t");
			sb.Append(versionNumber).Append(Environment.NewLine);
			foreach (var line in ControlCharacterVerseData.TabDelimitedCharacterVerseData
				         .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Skip(1)
				         .Select((contents, number) => new { Contents = contents, Number = number + 1 }))
			{
				if (line.Contents.Length == 0)
				{
					sb.Append(Environment.NewLine);
					continue;
				}

				if (line.Contents.StartsWith("#"))
				{
					sb.Append(line.Contents).Append(Environment.NewLine);
					continue;
				}

				string[] items = line.Contents.Split(new[] { kTab }, StringSplitOptions.None);

				if (sRegexSkipMultiVerseLine.IsMatch(line.Contents))
				{
					InsertEmptyQuotePositionIfNeeded(items, sb, line);
					continue;
				}

				var cv = ControlCharacterVerseData.Singleton.ProcessLine(items, line.Number).Single();
				if (cv.QuoteType == QuoteType.Indirect || cv.QuoteType == QuoteType.Interruption ||
				    cv.QuoteType == QuoteType.Implicit || cv.QuoteType == QuoteType.ImplicitWithPotentialSelfQuote ||
				    cv.QuoteType == QuoteType.Rare ||
				    NarratorOverrides.GetCharacterOverrideDetailsForRefRange(new VerseRef(cv.BcvRef.BBCCCVVV, ScrVers.English),
					    cv.Verse).Any() ||
				    ControlCharacterVerseData.Singleton.GetCharacters(cv.Book, cv.Chapter, cv.Verse)
					    .OfType<GlyssenCharacters.CharacterVerse>().Any(c => !c.Equals(cv) &&
						    ((cv.QuoteType == QuoteType.Normal && c.QuoteType != QuoteType.Alternate) ||
							    (cv.QuoteType == QuoteType.Alternate && !(c.QuoteType == QuoteType.Alternate ||
								    c.QuoteType == QuoteType.Alternate)))))

				{
					InsertEmptyQuotePositionIfNeeded(items, sb, line);
					continue;
				}

				if (currentBook[0] == null || currentBook[0].BookNumber != cv.Book)
				{
					for (i = 0; i < standardTexts.Length; i++)
					{
						var stdText = standardTexts[i];
						if (currentBook[i] == null || currentBook[i].BookNumber != cv.Book)
							currentBook[i] = stdText.GetBook(cv.Book);
					}
				}
				SetQuotePosition(items, sb, line,
					currentBook.Select(b => b.GetProposedQuotePosition(cv.Chapter, cv.Verse))
						.Distinct().OnlyOrDefault());
			}

			WriteFile(sb.ToString());
		}

		private static void InsertEmptyQuotePositionIfNeeded(string[] items, StringBuilder sb, object line)
		{
			if (items.Length >= CharacterVerseData.kiQuotePosition && items.Length < CharacterVerseData.kMaxItems)
			{
				var itemList = new List<string>(items);
				itemList.Insert(CharacterVerseData.kiQuotePosition, Empty);
				sb.Append(Join(kTab, itemList));
			}
			else
				sb.Append(line);

			sb.Append(Environment.NewLine);
		}

		private static void SetQuotePosition(string[] items, StringBuilder sb, object line, QuotePosition position)
		{
			if (position == QuotePosition.EntireVerse &&
			    items[CharacterVerseData.kiQuoteType] != QuoteType.Implicit.ToString() &&
			    items[CharacterVerseData.kiQuoteType] != QuoteType.ImplicitWithPotentialSelfQuote.ToString())
			{
				items[CharacterVerseData.kiQuoteType] = QuoteType.Implicit.ToString();
			}
			var sPos = position.ToString();
			if (items.Length > CharacterVerseData.kiQuotePosition)
			{
				items[CharacterVerseData.kiQuotePosition] = sPos;
				sb.Append(Join(kTab, items));
			}
			else if (items.Length == CharacterVerseData.kiQuotePosition)
			{
				var itemList = new List<string>(items) { sPos };
				sb.Append(Join(kTab, itemList));
			}
			else if (position != QuotePosition.Unspecified)
			{
				var itemList = new List<string>(items);
				while (itemList.Count < CharacterVerseData.kiQuotePosition - 1)
					itemList.Add(Empty);
				itemList.Add(sPos);
				sb.Append(Join(kTab, itemList));
			}
			else
			{
				sb.Append(line);
			}

			sb.Append(Environment.NewLine);
		}

		private static void WriteFile(string fileText)
		{
			File.WriteAllText(Path.Combine(CharacterListProcessing.kBaseDirForRealOutput, "CharacterVerse.txt"), fileText);
		}
	}
}
