using System.Collections.Generic;
using Paratext;
using ProtoScript.Bundle;

namespace ProtoScript
{
	public static class BookSetUtils
	{
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
	}
}
