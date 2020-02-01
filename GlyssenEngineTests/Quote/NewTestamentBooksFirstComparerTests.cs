using System;
using GlyssenEngine.Quote;
using NUnit.Framework;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	class NewTestamentBooksFirstComparerTests
	{
		[TestCase(1, 1, ExpectedResult = 0)]
		[TestCase(39, 39, ExpectedResult = 0)]
		[TestCase(40, 40, ExpectedResult = 0)]
		[TestCase(66, 66, ExpectedResult = 0)]
		[TestCase(39, 40, ExpectedResult = 1)]
		[TestCase(1, 40, ExpectedResult = 1)]
		[TestCase(39, 66, ExpectedResult = 1)]
		[TestCase(1, 66, ExpectedResult = 1)]
		[TestCase(40, 39, ExpectedResult = -1)]
		[TestCase(66, 39, ExpectedResult = -1)]
		[TestCase(66, 1, ExpectedResult = -1)]
		[TestCase(40, 1, ExpectedResult = -1)]
		public int NewTestamentBooksFirstComparer_Tests(int book1, int book2)
		{
			var comparer = new NewTestamentBooksFirstComparer();
			var result = comparer.Compare(book1, book2);
			if (result != 0)
				result = result / Math.Abs(result);
			return result;
		}
	}
}
