using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;

namespace Glyssen.Paratext
{
	internal class ParatextProjectBookInfo
	{
		public enum BookState
		{
			NoProblem,
			FailedCheck,
			/// <summary>
			/// "Canonical" here refers to Glyssen's notion of "canonical", not Paratext's (i.e., deuterocanonical books
			/// are excluded and are NOT considered canonical.
			/// </summary>
			ExcludedNonCanonical,
		}

		private readonly Dictionary<int, BookStatus> m_books = new Dictionary<int, BookStatus>();

		public IEnumerable<string> FailedChecksBooks => m_books.Values.Where(b => b.State == BookState.FailedCheck).Select(b => b.BookCode);
		public int SupportedBookCount => m_books.Values.Count(b => b.State != BookState.ExcludedNonCanonical);
		public bool HasBooksWithoutProblems => m_books.Values.Any(b => b.State == BookState.NoProblem);

		public class BookStatus
		{
			public string BookCode { get; set; }
			public BookState State { get; set; }
			public object Details { get; set; }

			public override string ToString()
			{
				string sFmt;
				switch (State)
				{
					case BookState.NoProblem:
						sFmt = LocalizationManager.GetString("ExcludedBookExplanation.NoProblem", "{0} has no problems.");
						break;
					case BookState.FailedCheck:
						sFmt = LocalizationManager.GetString("ExcludedBookExplanation.FailedChecks",
								"{0} did not pass the following checks required by {1}: ") +
							String.Join(", ", ((IEnumerable<string>)((object[])Details)[0]).Select(LocalizedCheckName));
						break;
					case BookState.ExcludedNonCanonical:
						sFmt = LocalizationManager.GetString("ExcludedBookExplanation.NonCanonical",
							"Excluded {0} because {1} supports only books in the universally accepted canon of Scripture.");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return String.Format(sFmt, BookCode, GlyssenInfo.kProduct);
			}
		}

		public void Add(int bookNum, string bookCode, BookState state, params object[] details)
		{
			m_books[bookNum] = new BookStatus {BookCode = bookCode, State = state, Details = details};
		}

		//public override string ToString()
		//{
		//	return String.Join(Environment.NewLine, Exclusions);
		//}

		//public string ToString(bool omitExplanations)
		//{
		//	return omitExplanations ? String.Join(Environment.NewLine, Exclusions.Select(e => e.BookCode)) : ToString();
		//}

		public static string LocalizedCheckName(string paratextCheckId)
		{
			switch (paratextCheckId)
			{
				case "Marker":
					return LocalizationManager.GetString("ParatextCheck.Marker", "Markers",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				case "Quotation":
					return LocalizationManager.GetString("ParatextCheck.Quotation", "Quotations",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				case "ChapterVerse":
					return LocalizationManager.GetString("ParatextCheck.ChapterVerse", "Chapter/Verse Numbers",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				default:
					return LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "ParatextCheck." + paratextCheckId, paratextCheckId,
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
			}
		}

		public BookState GetState(int bookNum)
		{
			return m_books[bookNum].State;
		}

		public string GetStatusInfo(int bookNum)
		{
			return m_books[bookNum].ToString();
		}
	}
}
