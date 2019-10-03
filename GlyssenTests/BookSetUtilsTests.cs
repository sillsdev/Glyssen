using System.Collections.Generic;
using System.Linq;
using Glyssen;
using NUnit.Framework;
using SIL.DblBundle.Text;
using SIL.Scripture;

namespace GlyssenTests
{
	[TestFixture]
	public class BookSetUtilsTests
	{
		[Test]
		public void OldTestament()
		{
			BookSet oldTestament = BookSetUtils.OldTestament;
			IEnumerable<string> otBookIds = oldTestament.SelectedBookIds;
			Assert.AreEqual(39, otBookIds.Count());
			Assert.IsTrue(otBookIds.Contains("GEN"));
			Assert.IsTrue(otBookIds.Contains("PSA"));
			Assert.IsTrue(otBookIds.Contains("MAL"));
		}
		[Test]
		public void NewTestament()
		{
			BookSet newTestament = BookSetUtils.NewTestament;
			IEnumerable<string> ntBookIds = newTestament.SelectedBookIds;
			Assert.AreEqual(27, ntBookIds.Count());
			Assert.IsTrue(ntBookIds.Contains("MAT"));
			Assert.IsTrue(ntBookIds.Contains("TIT"));
			Assert.IsTrue(ntBookIds.Contains("REV"));
		}

		[Test]
		public void ToBookSet_FromBookScripts()
		{
			var books = new List<string> { "MRK", "LUK", "1TH" };
			IEnumerable<BookScript> bookScripts = books.ToBookScriptEnumerable();
			BookSet bookSet = bookScripts.ToBookSet();
			IEnumerable<string> bookIds = bookSet.SelectedBookIds;
			Assert.AreEqual(3, bookIds.Count());
			Assert.IsTrue(bookIds.Contains("MRK"));
			Assert.IsTrue(bookIds.Contains("LUK"));
			Assert.IsTrue(bookIds.Contains("1TH"));
		}

		[Test]
		public void ToBookSet_FromBooks()
		{
			var books = new List<string> { "MRK", "LUK", "1TH" };
			IEnumerable<Book> bookScripts = books.ToBookEnumerable();
			BookSet bookSet = bookScripts.ToBookSet();
			IEnumerable<string> bookIds = bookSet.SelectedBookIds;
			Assert.AreEqual(3, bookIds.Count());
			Assert.IsTrue(bookIds.Contains("MRK"));
			Assert.IsTrue(bookIds.Contains("LUK"));
			Assert.IsTrue(bookIds.Contains("1TH"));
		}

		[Test]
		public void BookSummary_OneBook()
		{
			var books = new List<string> { "MRK" };
			Assert.AreEqual("MRK", books.BookSummary());
		}

		[Test]
		public void BookSummary_TwoAdjacentBooks()
		{
			var books = new List<string> { "MRK", "LUK" };
			Assert.AreEqual("MRK-LUK", books.BookSummary());
		}

		[Test]
		public void BookSummary_TwoNonAdjacentBooks()
		{
			var books = new List<string> { "MRK", "JHN" };
			Assert.AreEqual("MRK, JHN", books.BookSummary());
		}

		[Test]
		public void BookSummary_ThreeConsecutiveBooks()
		{
			var books = new List<string> { "MRK", "LUK", "JHN" };
			Assert.AreEqual("MRK-JHN", books.BookSummary());
		}

		[Test]
		public void BookSummary_TwoSetsOfConsecutiveBooks()
		{
			var books = new List<string> { "MRK", "LUK", "JHN", "1TH", "2TH", "1TI" };
			Assert.AreEqual("MRK-JHN, 1TH-1TI", books.BookSummary());
		}

		[Test]
		public void BookSummary_ConsecutiveBooksSpanningTestaments()
		{
			var books = new List<string> { "MAL", "MAT", "MRK" };
			Assert.AreEqual("MAL, MAT-MRK", books.BookSummary());
		}

		[Test]
		public void BookSummary_OldTestament()
		{
			var books = BookSetUtils.OldTestament.SelectedBookIds;
			Assert.AreEqual("Old Testament", books.BookSummary());
		}

		[Test]
		public void BookSummary_NewTestament()
		{
			var books = BookSetUtils.NewTestament.SelectedBookIds;
			Assert.AreEqual("New Testament", books.BookSummary());
		}

		[Test]
		public void BookSummary_OldTestamentAndOtherBooks()
		{
			var books = new List<string> { "MRK", "JHN" };
			books.AddRange(BookSetUtils.OldTestament.SelectedBookIds);
			Assert.AreEqual("Old Testament, MRK, JHN", books.BookSummary());
		}

		[Test]
		public void BookSummary_NewTestamentAndOtherBooks()
		{
			var books = new List<string> { "PSA", "PRO" };
			books.AddRange(BookSetUtils.NewTestament.SelectedBookIds);
			Assert.AreEqual("PSA-PRO, New Testament", books.BookSummary());
		}

		[Test]
		public void BookSummary_AllBooks()
		{
			var books = BookSetUtils.OldTestament.SelectedBookIds.Union(BookSetUtils.NewTestament.SelectedBookIds);
			Assert.AreEqual("All Books", books.BookSummary());
		}

		[Test]
		public void BookSummary_AllBooksButOne()
		{
			var books = new List<string>();
			books.AddRange(BookSetUtils.OldTestament.SelectedBookIds);
			books.AddRange(BookSetUtils.NewTestament.SelectedBookIds);
			books.Remove("GAL");
			Assert.AreEqual("Old Testament, MAT-2CO, EPH-REV", books.BookSummary());
		}
	}

	public static class BookSetUtilsTestsExtensions
	{
		public static string BookSummary(this IEnumerable<string> bookStrs)
		{
			return bookStrs.Select(b => new BookScript(b, Enumerable.Empty<Block>(), null)).BookSummary();
		}

		public static IEnumerable<BookScript> ToBookScriptEnumerable(this IEnumerable<string> bookStrs)
		{
			return bookStrs.Select(b => new BookScript(b, Enumerable.Empty<Block>(), null));
		}

		public static IEnumerable<Book> ToBookEnumerable(this IEnumerable<string> bookStrs)
		{
			return bookStrs.Select(b => new Book{Code = b});
		}
	}
}
