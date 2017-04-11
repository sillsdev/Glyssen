using Glyssen.Utilities;
using NUnit.Framework;

namespace GlyssenTests.Utilities
{
	[TestFixture]
	public class MathUtilitiesTests
	{
		[Test]
		public void Percent_ResultCalculatesCorrectly()
		{
			Assert.AreEqual(50, MathUtilities.Percent(1, 2));
		}

		[Test]
		public void PercentAsDouble_ResultCalculatesCorrectly()
		{
			Assert.AreEqual(50d, MathUtilities.PercentAsDouble(1, 2));
		}

		[Test]
		public void PercentAsDouble_ResultNotMoreThanMax()
		{
			int max = 99;
			Assert.AreEqual((double)max, MathUtilities.PercentAsDouble(1, 1, max));
		}

		[Test]
		public void PercentAsDouble_AllowsResultGreaterThan100()
		{
			Assert.AreEqual(200d, MathUtilities.PercentAsDouble(2, 1, 0));
		}

		[Test]
		public void PercentAsDouble_DivideBy0ReturnsMaxOr100()
		{
			Assert.AreEqual(99d, MathUtilities.PercentAsDouble(1, 0, 99));
			Assert.AreEqual(100d, MathUtilities.PercentAsDouble(1, 0, 0));
			Assert.AreEqual(100d, MathUtilities.PercentAsDouble(1, 0));
		}

		[Test]
		public void FormattedPercent_IntegralPercent_MinimumDecimalPlaces()
		{
			Assert.AreEqual("99%", MathUtilities.FormattedPercent(99, 0, 13));
			Assert.AreEqual("99.00%", MathUtilities.FormattedPercent(99, 2, 13));
			Assert.AreEqual("100%", MathUtilities.FormattedPercent(100, 0, 1));
			Assert.AreEqual("100.0%", MathUtilities.FormattedPercent(100, 1, 5, false));
			Assert.AreEqual("100%", MathUtilities.FormattedPercent(100, 1, 5));
		}

		[Test]
		public void FormattedPercent_NumbersThatDoNotRoundTo100Percent_NormalRounding()
		{
			Assert.AreEqual("99.9%", MathUtilities.FormattedPercent(99.9, 1, 3));
			Assert.AreEqual("99.9%", MathUtilities.FormattedPercent(99.949, 1, 3));
			Assert.AreEqual("98.00%", MathUtilities.FormattedPercent(97.999, 2, 13));
			Assert.AreEqual("97.999%", MathUtilities.FormattedPercent(97.999, 3, 6));
			Assert.AreEqual("99.999%", MathUtilities.FormattedPercent(99.9994, 3, 5));
		}

		[Test]
		public void FormattedPercent_NumbersThatRequireAdditionalPrecisionToNotRoundTo100Percent_ExtraPrecisionUsed()
		{
			Assert.AreEqual("99.9%", MathUtilities.FormattedPercent(99.9, 0, 3));
			Assert.AreEqual("99.9%", MathUtilities.FormattedPercent(99.949, 0, 3));
			Assert.AreEqual("99.999%", MathUtilities.FormattedPercent(99.999, 1, 13));
			Assert.AreEqual("99.999%", MathUtilities.FormattedPercent(99.9994, 1, 13));
			Assert.AreEqual("99.999999%", MathUtilities.FormattedPercent(99.9999994, 1, 6));
		}

		[Test]
		public void FormattedPercent_NumbersThatRoundTo100Percent_Truncate()
		{
			Assert.AreEqual("99%", MathUtilities.FormattedPercent(99.9, 0, 0));
			Assert.AreEqual("99.9%", MathUtilities.FormattedPercent(99.95, 0, 1));
			Assert.AreEqual("99.99%", MathUtilities.FormattedPercent(99.999, 1, 2));
			Assert.AreEqual("99.9999%", MathUtilities.FormattedPercent(99.99999, 1, 4));
			Assert.AreEqual("99.999999%", MathUtilities.FormattedPercent(99.9999995, 1, 6));
		}
	}
}
