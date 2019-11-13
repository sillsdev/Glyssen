using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using L10NSharp;
using SIL;

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
			public List<String> FailedChecks { get; set; }
		}

		public void Add(int bookNum, string bookCode, BookState state, IEnumerable<string> failedChecks = null)
		{
			m_books[bookNum] = new BookStatus {BookCode = bookCode, State = state, FailedChecks = failedChecks?.ToList()};
		}

		public static string LocalizedCheckName(string paratextCheckId)
		{
			switch (paratextCheckId)
			{
				case "Marker":
					return Localizer.GetString("ParatextCheck.Marker", "Markers",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				case "Quotation":
					return Localizer.GetString("ParatextCheck.Quotation", "Quotations",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				case "ChapterVerse":
					return Localizer.GetString("ParatextCheck.ChapterVerse", "Chapter/Verse Numbers",
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
				default:
					return Localizer.GetDynamicString(GlyssenInfo.kApplicationId, "ParatextCheck." + paratextCheckId, paratextCheckId,
						"This should exactly match the localized name of the check in Paratext if it is localized into the target language. Otherwise, probably best not to localize it at all (or put the English name in parentheses).");
			}
		}

		public BookState GetState(int bookNum)
		{
			return m_books[bookNum].State;
		}

		public IEnumerable<string> GetFailedChecks(int bookNum)
		{
			return m_books[bookNum].FailedChecks;
		}
	}
}
