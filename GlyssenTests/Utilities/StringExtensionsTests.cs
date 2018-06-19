using Glyssen.Utilities;
using NUnit.Framework;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	class StringExtensionsTests
	{
		[Test]
		public void CombineGuaranteeingWhitespaceBetween_NoWhiteSpace_AddsSpace()
		{
			string goat = "goat";
			string pig = "pig";
			Assert.AreEqual($"{goat} {pig}", goat.CombineGuaranteeingWhitespaceBetween(pig));
		}

		[TestCase("goat ", "pig")]
		[TestCase("goat", " pig")]
		[TestCase("goat ", " pig")]
		[TestCase("goat  ", " pig")]
		[TestCase(" goat ", "pig ")]
		[TestCase("goat\u00A0", "pig ")]
		[TestCase("goat", "\u00A0pig ")]
		public void CombineGuaranteeingWhitespaceBetween_AlreadyHasWhiteSpace_DoesNotAddSpace(string first, string second)
		{
			Assert.AreEqual($"{first}{second}", first.CombineGuaranteeingWhitespaceBetween(second));
		}
	}
}
