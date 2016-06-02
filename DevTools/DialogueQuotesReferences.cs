using System;
using System.Collections.Generic;
using System.Diagnostics;
using SIL.Scripture;

namespace DevTools
{
	public class DialogueQuotesReferences
	{
		private static readonly HashSet<string> s_references = new HashSet<string>();

		static DialogueQuotesReferences()
		{
			foreach (var line in ControlFiles.DialogQuotes.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				var reference = line.Trim();
				if (reference.Length < 7 || reference[3] != ' ' || !reference.Substring(4).Contains(":"))
					Debug.Fail("Line was not a valid scripture reference: " + line);
				BCVRef start = new BCVRef();
				BCVRef end = new BCVRef();
				if (!BCVRef.ParseRefRange(reference , ref start, ref end) || start != end)
					Debug.Fail("Line was not a valid scripture reference: " + line);

				s_references.Add(reference);
			}
		}

		public static bool Contains(string book, string chapter, string verse)
		{
			return s_references.Contains(String.Format("{0} {1}:{2}", book, chapter, verse));
		}
	}
}
