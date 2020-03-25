using System;
using GlyssenEngine.Utilities;
using NUnit.Framework;

namespace GlyssenEngineTests.Utilities
{
	[TestFixture]
	public class StringExtensionsTests
	{
		[Test]
		public void Truncate_NullString_ThrowsArgumentNullException()
		{
			string nullStr = null;
			Assert.Throws<ArgumentNullException>(() => nullStr.Truncate(1));
		}

		[Test]
		public void Truncate_NullEllipses_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => "Blah".Truncate(100, null));
		}

		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(1)]
		public void Truncate_TruncateToNonPositiveLength_ThrowsArgumentOutOfRangeException(int truncateAt)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => "Blah".Truncate(truncateAt));
		}

		[TestCase("S")]
		[TestCase("Short string")]
		[TestCase("This is a super-duper very long string, my friend.")]
		public void Truncate_ShorterOrEqualToLengthSpecified_NoChange(string input)
		{
			var maxLen = Math.Max(2, input.Length);
			Assert.AreEqual(input, input.Truncate(maxLen));
			Assert.AreEqual(input, input.Truncate(maxLen + 1));
		}

		[TestCase("Short string", "...")]
		[TestCase("This is a super-duper very long string, my friend.", "\u2026")]
		public void Truncate_TruncatingAndAddingEllipsesWouldMakeSameOrLonger_NoChange(string input, string ellipses)
		{
			var maxLen = input.Length - 1;
			Assert.AreEqual(input, input.Truncate(maxLen, ellipses));
			for (int i = ellipses.Length - 1; i > 0; i--)
				Assert.AreEqual(input, input.Truncate(maxLen - i, ellipses));
		}

		[TestCase("Short string", 7, "...", ExpectedResult = "Short s...")]
		[TestCase("Short string", 6, "...", ExpectedResult = "Short...")]
		[TestCase("Short string", 5, "...", ExpectedResult = "Short...")]
		[TestCase("Short string", 8, "...", ExpectedResult = "Short st...")]
		[TestCase("This is a super-duper very long string, my friend.     ", 9, "\u2026", ExpectedResult = "This is a\u2026")]
		[TestCase("This is a super-duper very long string, my friend.     ", 48, "\u2026", ExpectedResult = "This is a super-duper very long string, my frien\u2026")]
		public string Truncate_LongerThanMaxPlusEllipsesLength_Truncated(string input, int truncateAt, string ellipses)
		{
			return input.Truncate(truncateAt, ellipses);
		}

		[TestCase("This is a super-duper very long string, my friend.     ", 49, "\u2026")]
		[TestCase("This is a super-duper very long string, my friend.     ", 49, "...")]
		[TestCase("This is a super-duper very long string, my friend.     ", 50, "\u2026")]
		[TestCase("This is a super-duper very long string, my friend.     ", 51, "\u2026")]
		[TestCase("This is a super-duper very long string, my friend.     ", 52, "\u2026")]
		[TestCase("This is a super-duper very long string, my friend.     ", 53, "\u2026")]
		public void Truncate_TrailingWhitespaceMakesItLongerThanMaxPlusEllipsesLength_Trimmed(string input, int truncateAt, string ellipses)
		{
			Assert.AreEqual("This is a super-duper very long string, my friend.", input.Truncate(truncateAt, ellipses));
		}
	}
}
