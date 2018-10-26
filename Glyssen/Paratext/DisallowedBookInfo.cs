using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;

namespace Glyssen.Paratext
{
	internal class DisallowedBookInfo
	{
		public enum Reason
		{
			NonCanonical,
			FailedCheck
		}

		private readonly Dictionary<string, BookProblem> m_exclusions = new Dictionary<string, BookProblem>();

		public IEnumerable<BookProblem> Exclusions => m_exclusions.Values;

		public class BookProblem
		{
			public string BookCode { get; set; }
			public Reason ProblemType { get; set; }
			public object Details { get; set; }

			public override string ToString()
			{
				string sFmt;
				switch (ProblemType)
				{
					case Reason.NonCanonical:
						sFmt = LocalizationManager.GetString("ExcludedBookExplanation.NonCanonical",
							"Excluded {0} because {1} supports only books in the universally accepted canon of Scripture.");
						break;
					case Reason.FailedCheck:
						sFmt = LocalizationManager.GetString("ExcludedBookExplanation.FailedChecks",
								"Excluded {0} because it did not pass one or more checks required by {1}:") +
							String.Join(", ", FailedChecks);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return String.Format(sFmt, BookCode, GlyssenInfo.kProduct);
			}

			public IEnumerable<string> FailedChecks => LocalizedCheckNames((IEnumerable<string>)((object[])Details)[0]);

		}

		public void Add(string bookCode, Reason reason, params object[] details)
		{
			m_exclusions[bookCode] = new BookProblem {BookCode = bookCode, ProblemType = reason, Details = details};
		}

		public override string ToString()
		{
			if (!Exclusions.Any())
				return String.Empty;
			return LocalizationManager.GetString("ExcludedBookExplanation.IntroductoryExplanation",
				"The following books were not included in this project:") + Environment.NewLine +
				String.Join(Environment.NewLine, Exclusions);
		}

		public static IEnumerable<string> LocalizedCheckNames(IEnumerable<string> paratextCheckIds)
		{
			foreach (var check in paratextCheckIds)
			{
				switch (check)
				{
					case "Marker":
						yield return LocalizationManager.GetString("ParatextCheck.Marker", "Markers",
							"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
						break;
					case "Quotation":
						yield return LocalizationManager.GetString("ParatextCheck.Quotation", "Quoted Text",
							"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
						break;
					case "ChapterVerse":
						yield return LocalizationManager.GetString("ParatextCheck.ChapterVerse", "Chapter/Verse Numbers",
							"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
						break;
					default:
						yield return LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "ParatextCheck." + check, check,
							"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
						break;
				}
			}
		}
	}
}
