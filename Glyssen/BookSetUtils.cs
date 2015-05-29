using System.Collections.Generic;
using System.Linq;
using L10NSharp;
using Paratext;
using SIL.DblBundle.Text;
using SIL.ScriptureUtils;

namespace Glyssen
{
	public static class BookSetUtils
	{
		public static BookSet OldTestament = new BookSet("111111111111111111111111111111111111111");
		public static BookSet NewTestament = new BookSet("000000000000000000000000000000000000000111111111111111111111111111");

		public static BookSet ToBookSet(this IEnumerable<Book> books)
		{
			var bookSet = new BookSet();
			foreach (Book book in books)
				bookSet.Add(book.Code);
			return bookSet;
		}

		public static BookSet ToBookSet(this IEnumerable<BookScript> books)
		{
			var bookSet = new BookSet();
			foreach (BookScript book in books)
				bookSet.Add(book.BookId);
			return bookSet;
		}

		public static string BookSummary(this IEnumerable<BookScript> books)
		{
			var bookSet = new BookSet();
			foreach (BookScript book in books)
				bookSet.Add(book.BookId);
			return bookSet.CustomSummary();
		}

		public static string BookSummary(this IEnumerable<string> books)
		{
			var bookSet = new BookSet();
			foreach (string bookId in books)
				bookSet.Add(bookId);
			return bookSet.CustomSummary();
		}

		public static string CustomSummary(this BookSet bookSet)
		{
			var components = new List<string>();

			if (bookSet.Count == OldTestament.Count + NewTestament.Count)
				return LocalizationManager.GetString("BookSelection.AllBooks", "All Books");
			BookSet includedOtBooks = bookSet.Intersect(OldTestament);
			if (includedOtBooks.Count == OldTestament.Count)
				components.Add(LocalizationManager.GetString("BookSelection.OldTestament", "Old Testament"));
			else
				components.AddRange(PartialTestamentBookSummary(includedOtBooks, OldTestament));

			BookSet includedNtBooks = bookSet.Intersect(NewTestament);
			if (includedNtBooks.Count == NewTestament.Count)
				components.Add(LocalizationManager.GetString("BookSelection.NewTestament", "New Testament"));
			else
				components.AddRange(PartialTestamentBookSummary(includedNtBooks, NewTestament));
			return string.Join(", ", components);
		}

		private static IEnumerable<string> PartialTestamentBookSummary(BookSet bookSet, BookSet setToCompare)
		{
			if (bookSet.Count == 0)
				return Enumerable.Empty<string>();

			var result = new List<string>();
			string start = null;
			string previous = null;
			foreach (int bookNum in setToCompare.SelectedBookNumbers)
			{
				if (bookSet.IsSelected(bookNum))
				{
					string bookCode = BCVRef.NumberToBookCode(bookNum);
					if (start == null)
						start = bookCode;
					previous = bookCode;
				}
				else
				{
					if (start == null && previous != null)
						result.Add(previous);
					else if (start != null)
					{
						if (start == previous)
							result.Add(start);
						else
							result.Add(start + "-" + previous);
						start = null;
						previous = null;
					}
				}
			}
			if (previous != null)
			{
				if (start == previous)
					result.Add(start);
				else
					result.Add(start + "-" + previous);
			}
			return result;
		}
	}
}
