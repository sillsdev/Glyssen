using System;
using Glyssen.Shared;

namespace GlyssenSharedTests
{
	public class BookStub : IScrBook
	{
		public string BookId { get; set; }
		public string GetVerseText(int chapter, int verse)
		{
			throw new NotImplementedException();
		}
	}

}
