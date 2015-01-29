using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProtoScript.Character;
using SIL.ScriptureUtils;

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
			sb.Append("#Book").Append(TAB).Append("Chapter").Append(TAB).Append("Verse").Append(TAB)
				.Append("Control Character").Append(TAB).Append("FCBH Character").Append(TAB).Append("Delivery").Append(TAB)
				.Append("Alias").Append(TAB).Append("Dialog").Append(TAB).Append("Default Char").Append(TAB)
				.Append("Parallel").Append(TAB).Append(Environment.NewLine);

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

			var controlDictionary = new Dictionary<BCVRef, List<ProtoScript.Character.CharacterVerse>>();
			foreach (ProtoScript.Character.CharacterVerse cv in control)
			{
				List<ProtoScript.Character.CharacterVerse> list;
				if (controlDictionary.TryGetValue(cv.BcvRef, out list))
				{
					list.Add(cv);
				}
				else
				{
					list = new List<ProtoScript.Character.CharacterVerse> { cv };
					controlDictionary.Add(cv.BcvRef, list);
				}
			}

			foreach (BCVRef bcvRef in new SortedSet<BCVRef>(fcbhDictionary.Keys.Union(controlDictionary.Keys)))
			{
				List<TemplateDatum> fcbhList;
				List<ProtoScript.Character.CharacterVerse> controlList;
				bool hasFcbh = fcbhDictionary.TryGetValue(bcvRef, out fcbhList);
				bool hasControl = controlDictionary.TryGetValue(bcvRef, out controlList);
				if (hasFcbh && fcbhList.Count == 1 && hasControl && controlList.Count == 1)
				{
					var cv = controlList[0];
					var fcbhDatum = fcbhList[0];
					sb.Append(cv.BookCode).Append(TAB)
					.Append(cv.Chapter).Append(TAB).Append(cv.Verse).Append(TAB).Append(cv.Character).Append(TAB)
					.Append(fcbhDatum.CharacterId).Append(TAB).Append(cv.Delivery).Append(TAB).Append(cv.Alias).Append(TAB)
					.Append(cv.IsDialogue).Append(TAB).Append(cv.DefaultCharacter).Append(TAB).Append(cv.ParallelPassageReferences)
					.Append(Environment.NewLine);
				}
				else
				{
					if (hasControl)
						foreach (ProtoScript.Character.CharacterVerse cv in controlList)
						{
							sb.Append(cv.BookCode).Append(TAB)
							.Append(cv.Chapter).Append(TAB).Append(cv.Verse).Append(TAB).Append(cv.Character).Append(TAB)
							.Append(TAB).Append(cv.Delivery).Append(TAB).Append(cv.Alias).Append(TAB)
							.Append(cv.IsDialogue).Append(TAB).Append(cv.DefaultCharacter).Append(TAB).Append(cv.ParallelPassageReferences)
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
