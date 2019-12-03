using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Character;
using SIL.Scripture;

namespace DevTools.FCBH
{
	public class Processor
	{
		public const string kTab = "\t";
		public static void Process()
		{
			Directory.CreateDirectory("..\\..\\Resources\\temporary");
			var sb = new StringBuilder();

//			CompareAndCombineLists(true, sb);
//			File.WriteAllText("..\\..\\Resources\\temporary\\control_plus_FCBH.txt", sb.ToString());

			sb.Clear();
			CompareAndCombineLists(false, sb);
			File.WriteAllText("..\\..\\Resources\\temporary\\control_plus_FCBH_without_FCBH_narrator.txt", sb.ToString());
		}

		private static void CompareAndCombineLists(bool includeFcbhNarrator, StringBuilder sb)
		{
			sb.Append("Control File Version").Append(kTab).Append(Environment.NewLine);
			sb.Append("#").Append(kTab).Append("C").Append(kTab).Append("V").Append(kTab)
				.Append("Character ID").Append(kTab).Append("FCBH Character ID").Append(kTab).Append("Delivery").Append(kTab)
				.Append("Alias").Append(kTab).Append("Quote Type").Append(kTab).Append("Default Character").Append(kTab)
				.Append("Parallel Passage").Append(Environment.NewLine);

			var allFcbh = TemplateData.All(includeFcbhNarrator);
			var control = ControlCharacterVerseData.Singleton.GetAllQuoteInfo();

			var fcbhDictionary = new Dictionary<BCVRef, List<TemplateDatum>>();
			foreach (TemplateDatum d in allFcbh)
			{
				List<TemplateDatum> list;
				if (fcbhDictionary.TryGetValue(d.BcvRef, out list))
				{
					if (!list.Select(f => f.CharacterId).Contains(d.CharacterId))
						list.Add(d);
				}
				else
				{
					list = new List<TemplateDatum> { d };
					fcbhDictionary.Add(d.BcvRef, list);
				}
			}

			var controlDictionary = new Dictionary<BCVRef, List<GlyssenEngine.Character.CharacterVerse>>();
			foreach (GlyssenEngine.Character.CharacterVerse cv in control)
			{
				List<GlyssenEngine.Character.CharacterVerse> list;
				if (controlDictionary.TryGetValue(cv.BcvRef, out list))
				{
					list.Add(cv);
				}
				else
				{
					list = new List<GlyssenEngine.Character.CharacterVerse> { cv };
					controlDictionary.Add(cv.BcvRef, list);
				}
			}

			foreach (BCVRef bcvRef in new SortedSet<BCVRef>(fcbhDictionary.Keys.Union(controlDictionary.Keys)))
			{
				List<TemplateDatum> fcbhList;
				List<GlyssenEngine.Character.CharacterVerse> controlList;
				bool hasFcbh = fcbhDictionary.TryGetValue(bcvRef, out fcbhList);
				bool hasControl = controlDictionary.TryGetValue(bcvRef, out controlList);
				if (hasFcbh && fcbhList.Count == 1 && hasControl && controlList.Count == 1)
				{
					var cv = controlList[0];
					var fcbhDatum = fcbhList[0];
					sb.Append(cv.BookCode).Append(kTab)
					.Append(cv.Chapter).Append(kTab).Append(cv.Verse).Append(kTab).Append(cv.Character).Append(kTab)
					.Append(fcbhDatum.CharacterId).Append(kTab).Append(cv.Delivery).Append(kTab).Append(cv.Alias).Append(kTab)
					.Append(cv.QuoteType).Append(kTab).Append(cv.DefaultCharacter).Append(kTab).Append(cv.ParallelPassageReferences)
					.Append(Environment.NewLine);
				}
				else
				{
					if (hasControl)
						foreach (GlyssenEngine.Character.CharacterVerse cv in controlList)
						{
							if (hasFcbh && fcbhList.Select(f => f.CharacterId).Contains(cv.Character))
							{
								sb.Append(cv.BookCode).Append(kTab)
									.Append(cv.Chapter).Append(kTab).Append(cv.Verse).Append(kTab).Append(cv.Character).Append(kTab)
									.Append(cv.Character).Append(kTab).Append(cv.Delivery).Append(kTab).Append(cv.Alias).Append(kTab)
									.Append(cv.QuoteType).Append(kTab).Append(cv.DefaultCharacter).Append(kTab).Append(cv.ParallelPassageReferences)
									.Append(Environment.NewLine);
								foreach (TemplateDatum d in fcbhList.Where(f => f.CharacterId == cv.Character))
									d.IsProcessed = true;
							}
							else
							{
								sb.Append(cv.BookCode).Append(kTab)
									.Append(cv.Chapter).Append(kTab).Append(cv.Verse).Append(kTab).Append(cv.Character).Append(kTab)
									.Append(kTab).Append(cv.Delivery).Append(kTab).Append(cv.Alias).Append(kTab)
									.Append(cv.QuoteType).Append(kTab).Append(cv.DefaultCharacter).Append(kTab).Append(cv.ParallelPassageReferences)
									.Append(Environment.NewLine);
							}
						}
					if (hasFcbh)
						foreach (TemplateDatum d in fcbhList.Where(f => !f.IsProcessed))
						{
							if (!d.BcvRef.BookIsValid)
								Debug.Fail("Invalid book: " + d.BcvRef);
							var bookCode = BCVRef.NumberToBookCode(d.BcvRef.Book);
							if (string.IsNullOrWhiteSpace(bookCode))
								Debug.Fail("Invalid book code: " + bookCode);
							sb.Append(bookCode).Append(kTab)
							.Append(d.BcvRef.Chapter).Append(kTab).Append(d.BcvRef.Verse).Append(kTab).Append(kTab)
							.Append(d.CharacterId).Append(kTab).Append(kTab).Append(kTab)
							.Append(kTab).Append(kTab)
							.Append(Environment.NewLine);
						}
				}
			}
		}
	}
}
