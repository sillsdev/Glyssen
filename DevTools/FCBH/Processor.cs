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
		public const string TAB = "\t";
		public static void Process()
		{
			Directory.CreateDirectory("..\\..\\Resources\\temporary");
			var sb = new StringBuilder();

			CompareAndCombineLists(true, sb);
			File.WriteAllText("..\\..\\Resources\\temporary\\control_plus_FCBH.txt", sb.ToString());

			sb.Clear();
			CompareAndCombineLists(false, sb);
			File.WriteAllText("..\\..\\Resources\\temporary\\control_plus_FCBH_without_FCBH_narrator.txt", sb.ToString());
		}

		private static void CompareAndCombineLists(bool includeFcbhNarrator, StringBuilder sb)
		{
			sb.Append("Control File Version").Append(TAB).Append(Environment.NewLine);
			sb.Append("#").Append(TAB).Append("C").Append(TAB).Append("V").Append(TAB)
				.Append("Character ID").Append(TAB).Append("FCBH Character ID").Append(TAB).Append("Delivery").Append(TAB)
				.Append("Alias").Append(TAB).Append("Quote Type").Append(TAB).Append("Default Character").Append(TAB)
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

			var controlDictionary = new Dictionary<BCVRef, List<Glyssen.Character.CharacterVerse>>();
			foreach (Glyssen.Character.CharacterVerse cv in control)
			{
				List<Glyssen.Character.CharacterVerse> list;
				if (controlDictionary.TryGetValue(cv.BcvRef, out list))
				{
					list.Add(cv);
				}
				else
				{
					list = new List<Glyssen.Character.CharacterVerse> { cv };
					controlDictionary.Add(cv.BcvRef, list);
				}
			}

			foreach (BCVRef bcvRef in new SortedSet<BCVRef>(fcbhDictionary.Keys.Union(controlDictionary.Keys)))
			{
				List<TemplateDatum> fcbhList;
				List<Glyssen.Character.CharacterVerse> controlList;
				bool hasFcbh = fcbhDictionary.TryGetValue(bcvRef, out fcbhList);
				bool hasControl = controlDictionary.TryGetValue(bcvRef, out controlList);
				if (hasFcbh && fcbhList.Count == 1 && hasControl && controlList.Count == 1)
				{
					var cv = controlList[0];
					var fcbhDatum = fcbhList[0];
					sb.Append(cv.BookCode).Append(TAB)
					.Append(cv.Chapter).Append(TAB).Append(cv.Verse).Append(TAB).Append(cv.Character).Append(TAB)
					.Append(fcbhDatum.CharacterId).Append(TAB).Append(cv.Delivery).Append(TAB).Append(cv.Alias).Append(TAB)
					.Append(cv.QuoteType).Append(TAB).Append(cv.DefaultCharacter).Append(TAB).Append(cv.ParallelPassageReferences)
					.Append(Environment.NewLine);
				}
				else
				{
					if (hasControl)
						foreach (Glyssen.Character.CharacterVerse cv in controlList)
						{
							sb.Append(cv.BookCode).Append(TAB)
							.Append(cv.Chapter).Append(TAB).Append(cv.Verse).Append(TAB).Append(cv.Character).Append(TAB)
							.Append(TAB).Append(cv.Delivery).Append(TAB).Append(cv.Alias).Append(TAB)
							.Append(cv.QuoteType).Append(TAB).Append(cv.DefaultCharacter).Append(TAB).Append(cv.ParallelPassageReferences)
							.Append(Environment.NewLine);
						}
					if (hasFcbh)
						foreach (TemplateDatum d in fcbhList)
						{
							if (!d.BcvRef.BookIsValid)
								Debug.Fail("Invalid book: " + d.BcvRef);
							var bookCode = BCVRef.NumberToBookCode(d.BcvRef.Book);
							if (string.IsNullOrWhiteSpace(bookCode))
								Debug.Fail("Invalid book code: " + bookCode);
							sb.Append(bookCode).Append(TAB)
							.Append(d.BcvRef.Chapter).Append(TAB).Append(d.BcvRef.Verse).Append(TAB).Append(TAB)
							.Append(d.CharacterId).Append(TAB).Append(TAB).Append(TAB)
							.Append(TAB).Append(TAB)
							.Append(Environment.NewLine);
						}
				}
			}
		}
	}
}
