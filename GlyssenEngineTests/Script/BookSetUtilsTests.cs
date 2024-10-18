using System.Collections.Generic;
using System.Linq;
using GlyssenEngine.Script;
using NUnit.Framework;
using SIL.DblBundle.Text;
using SIL.Scripture;

namespace GlyssenEngineTests.Script
{
	[TestFixture]
	public class BookSetUtilsTests
	{
		[Test]
		public void OldTestament()
		{
			BookSet oldTestament = BookSetUtils.OldTestament;
			IEnumerable<string> otBookIds = oldTestament.SelectedBookIds.ToList();
			Assert.That(otBookIds.Count(), Is.EqualTo(39));
			Assert.That(otBookIds, Does.Contain("GEN"));
			Assert.That(otBookIds, Does.Contain("PSA"));
			Assert.That(otBookIds, Does.Contain("MAL"));
		}
		[Test]
		public void NewTestament()
		{
			BookSet newTestament = BookSetUtils.NewTestament;
			IEnumerable<string> ntBookIds = newTestament.SelectedBookIds.ToList();
			Assert.That(ntBookIds.Count(), Is.EqualTo(27));
			Assert.That(ntBookIds, Does.Contain("MAT"));
			Assert.That(ntBookIds, Does.Contain("TIT"));
			Assert.That(ntBookIds, Does.Contain("REV"));
		}

		[Test]
		public void ToBookSet_FromBookScripts()
		{
			var books = new List<string> { "MRK", "LUK", "1TH" };
			IEnumerable<BookScript> bookScripts = books.ToBookScriptEnumerable();
			BookSet bookSet = bookScripts.ToBookSet();
			IEnumerable<string> bookIds = bookSet.SelectedBookIds.ToList();
			Assert.That(bookIds.Count(), Is.EqualTo(3));
			Assert.That(bookIds, Does.Contain("MRK"));
			Assert.That(bookIds, Does.Contain("LUK"));
			Assert.That(bookIds, Does.Contain("1TH"));
		}

		[Test]
		public void ToBookSet_FromBooks()
		{
			var books = new List<string> { "MRK", "LUK", "1TH" };
			IEnumerable<Book> bookScripts = books.ToBookEnumerable();
			BookSet bookSet = bookScripts.ToBookSet();
			IEnumerable<string> bookIds = bookSet.SelectedBookIds.ToList();
			Assert.That(bookIds.Count(), Is.EqualTo(3));
			Assert.That(bookIds, Does.Contain("MRK"));
			Assert.That(bookIds, Does.Contain("LUK"));
			Assert.That(bookIds, Does.Contain("1TH"));
		}

		[Test]
		public void BookSummary_OneBook()
		{
			var books = new List<string> { "MRK" };
			Assert.That(books.BookSummary(), Is.EqualTo("MRK"));
		}

		[Test]
		public void BookSummary_TwoAdjacentBooks()
		{
			var books = new List<string> { "MRK", "LUK" };
			Assert.That(books.BookSummary(), Is.EqualTo("MRK-LUK"));
		}

		[Test]
		public void BookSummary_TwoNonAdjacentBooks()
		{
			var books = new List<string> { "MRK", "JHN" };
			Assert.That(books.BookSummary(), Is.EqualTo("MRK, JHN"));
		}

		[Test]
		public void BookSummary_ThreeConsecutiveBooks()
		{
			var books = new List<string> { "MRK", "LUK", "JHN" };
			Assert.That(books.BookSummary(), Is.EqualTo("MRK-JHN"));
		}

		[Test]
		public void BookSummary_TwoSetsOfConsecutiveBooks()
		{
			var books = new List<string> { "MRK", "LUK", "JHN", "1TH", "2TH", "1TI" };
			Assert.That(books.BookSummary(), Is.EqualTo("MRK-JHN, 1TH-1TI"));
		}

		[Test]
		public void BookSummary_ConsecutiveBooksSpanningTestaments()
		{
			var books = new List<string> { "MAL", "MAT", "MRK" };
			Assert.That(books.BookSummary(), Is.EqualTo("MAL, MAT-MRK"));
		}

		[Test]
		public void BookSummary_OldTestament()
		{
			var books = BookSetUtils.OldTestament.SelectedBookIds;
			Assert.That(books.BookSummary(), Is.EqualTo("Old Testament"));
		}

		[Test]
		public void BookSummary_NewTestament()
		{
			var books = BookSetUtils.NewTestament.SelectedBookIds;
			Assert.That(books.BookSummary(), Is.EqualTo("New Testament"));
		}

		[Test]
		public void BookSummary_OldTestamentAndOtherBooks()
		{
			var books = new List<string> { "MRK", "JHN" };
			books.AddRange(BookSetUtils.OldTestament.SelectedBookIds);
			Assert.That(books.BookSummary(), Is.EqualTo("Old Testament, MRK, JHN"));
		}

		[Test]
		public void BookSummary_NewTestamentAndOtherBooks()
		{
			var books = new List<string> { "PSA", "PRO" };
			books.AddRange(BookSetUtils.NewTestament.SelectedBookIds);
			Assert.That(books.BookSummary(), Is.EqualTo("PSA-PRO, New Testament"));
		}

		[Test]
		public void BookSummary_AllBooks()
		{
			var books = BookSetUtils.OldTestament.SelectedBookIds.Union(BookSetUtils.NewTestament.SelectedBookIds);
			Assert.That(books.BookSummary(), Is.EqualTo("All Books"));
		}

		[Test]
		public void BookSummary_AllBooksButOne()
		{
			var books = new List<string>();
			books.AddRange(BookSetUtils.OldTestament.SelectedBookIds);
			books.AddRange(BookSetUtils.NewTestament.SelectedBookIds);
			books.Remove("GAL");
			Assert.That(books.BookSummary(), Is.EqualTo("Old Testament, MAT-2CO, EPH-REV"));
		}
	}

	public static class BookSetUtilsTestsExtensions
	{
		public static string BookSummary(this IEnumerable<string> bookIds) =>
			bookIds.Select(b => new BookScript(b, Enumerable.Empty<Block>(), null)).BookSummary();

		public static IEnumerable<BookScript> ToBookScriptEnumerable(this IEnumerable<string> bookIds) =>
			bookIds.Select(b => new BookScript(b, Enumerable.Empty<Block>(), null));

		public static IEnumerable<Book> ToBookEnumerable(this IEnumerable<string> bookIds) =>
			bookIds.Select(b => new Book{Code = b});
	}
}
