using System;
using System.Collections.Generic;
using System.Diagnostics;
using SIL.ScriptureUtils;

namespace DevTools.FCBH
{
	public class TemplateData
	{
		public static IEnumerable<TemplateDatum> All()
		{
			var result = new List<TemplateDatum>();
			foreach (var line in ControlFiles.FCBH_OT_Template_simplified.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				try
				{
					string[] items = line.Split(new[] { '\t' });
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
			foreach (var line in ControlFiles.FCBH_NT_Template_simplified.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				try
				{
					string[] items = line.Split(new[] { '\t' });
					int verse = items[2] == "<<" ? 0 : Int32.Parse(items[2]);
					string book = items[0];
					switch (book)
					{
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
