using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.ScriptureUtils;

namespace DevTools.FCBH
{
	public class TemplateData
	{
		public static IEnumerable<TemplateDatum> All(bool includeNarrator)
		{
			var result = new List<TemplateDatum>();
			var allLines = ControlFiles.FCBH_OT_Template_simplified.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
				allLines.AddRange(ControlFiles.FCBH_NT_Template_simplified.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList());
			foreach (var line in allLines)
			{
				try
				{
					string[] items = line.Split(new[] { '\t' });

					if (!includeNarrator && items[3].StartsWith("Narr_"))
						continue;

					int verse = items[2] == "<<" ? 0 : Int32.Parse(items[2]);
					string book = items[0];
					switch (book)
					{
						case "1SM":
							book = "1SA";
							break;
						case "2SM":
							book = "2SA";
							break;
						case "PSM":
							book = "PSA";
							break;
						case "PRV":
							book = "PRO";
							break;
						case "SOS":
							book = "SNG";
							break;
						case "EZE":
							book = "EZK";
							break;
						case "JOEL":
							book = "JOL";
							break;
						case "NAH":
							book = "NAM";
							break;
						case "TTS":
							book = "TIT";
							break;
						case "JMS":
							book = "JAS";
							break;
					}
					var bcvRef = new BCVRef(BCVRef.BookToNumber(book), Int32.Parse(items[1]), verse);
					if (!bcvRef.BookIsValid)
						Debug.Fail("Invalid book: " + line);
					result.Add(new TemplateDatum(bcvRef, items[3]));
				}
				catch
				{
					Debug.Fail(line);
				}
			}
			return result;
		} 
	}
}
