using GlyssenEngine.Utilities;
using NUnit.Framework;

namespace GlyssenEngineTests.Utilities
{
	[TestFixture]
	public class MathUtilitiesTests
	{
		[Test]
		public void Percent_ResultCalculatesCorrectly()
		{
			Assert.That(MathUtilities.Percent(1, 2), Is.EqualTo(50));
		}

		[Test]
		public void PercentAsDouble_ResultCalculatesCorrectly()
		{
			Assert.That(50d, Is.EqualTo(MathUtilities.PercentAsDouble(1, 2)));
		}

		[Test]
		public void PercentAsDouble_ResultNotMoreThanMax()
		{
			int max = 99;
			Assert.That((double)max, Is.EqualTo(MathUtilities.PercentAsDouble(1, 1, max)));
		}

		[Test]
		public void PercentAsDouble_AllowsResultGreaterThan100()
		{
			Assert.That(200d, Is.EqualTo(MathUtilities.PercentAsDouble(2, 1, 0)));
		}

		[Test]
		public void PercentAsDouble_DenominatorIs0_InsteadOfDivideBy0ReturnsMaxOr100()
		{
			Assert.That(99d, Is.EqualTo(MathUtilities.PercentAsDouble(1, 0, 99)));
			Assert.That(100d, Is.EqualTo(MathUtilities.PercentAsDouble(1, 0, 0)));
			Assert.That(100d, Is.EqualTo(MathUtilities.PercentAsDouble(1, 0)));
		}

		[Test]
		public void PercentAsDouble_NumeratorAndDenominatorBoth0_Returns0()
		{
			Assert.That(0d, Is.EqualTo(MathUtilities.PercentAsDouble(0, 0, 99)));
			Assert.That(0d, Is.EqualTo(MathUtilities.PercentAsDouble(0, 0, 0)));
			Assert.That(0d, Is.EqualTo(MathUtilities.PercentAsDouble(0, 0)));
		}

		[Test]
		public void FormattedPercent_IntegralPercent_MinimumDecimalPlaces()
		{
			Assert.That(MathUtilities.FormattedPercent(99, 0, 13), Is.EqualTo("99%"));
			Assert.That(MathUtilities.FormattedPercent(99, 2, 13), Is.EqualTo("99.00%"));
			Assert.That(MathUtilities.FormattedPercent(100, 0, 1), Is.EqualTo("100%"));
			Assert.That(MathUtilities.FormattedPercent(100, 1, 5, false), Is.EqualTo("100.0%"));
			Assert.That(MathUtilities.FormattedPercent(100, 1, 5), Is.EqualTo("100%"));
		}

		[Test]
		public void FormattedPercent_NumbersThatDoNotRoundTo100Percent_NormalRounding()
		{
			Assert.That(MathUtilities.FormattedPercent(99.9, 1, 3), Is.EqualTo("99.9%"));
			Assert.That(MathUtilities.FormattedPercent(99.949, 1, 3), Is.EqualTo("99.9%"));
			Assert.That(MathUtilities.FormattedPercent(97.999, 2, 13), Is.EqualTo("98.00%"));
			Assert.That(MathUtilities.FormattedPercent(97.999, 3, 6), Is.EqualTo("97.999%"));
			Assert.That(MathUtilities.FormattedPercent(99.9994, 3, 5), Is.EqualTo("99.999%"));
		}

		[Test]
		public void FormattedPercent_NumbersThatRequireAdditionalPrecisionToNotRoundTo100Percent_ExtraPrecisionUsed()
		{
			Assert.That(MathUtilities.FormattedPercent(99.9, 0, 3), Is.EqualTo("99.9%"));
			Assert.That(MathUtilities.FormattedPercent(99.949, 0, 3), Is.EqualTo("99.9%"));
			Assert.That(MathUtilities.FormattedPercent(99.999, 1, 13), Is.EqualTo("99.999%"));
			Assert.That(MathUtilities.FormattedPercent(99.9994, 1, 13), Is.EqualTo("99.999%"));
			Assert.That(MathUtilities.FormattedPercent(99.9999994, 1, 6), Is.EqualTo("99.999999%"));
		}

		[Test]
		public void FormattedPercent_NumbersThatRoundTo100Percent_Truncate()
		{
			Assert.That(MathUtilities.FormattedPercent(99.9, 0, 0), Is.EqualTo("99%"));
			Assert.That(MathUtilities.FormattedPercent(99.95, 0, 1), Is.EqualTo("99.9%"));
			Assert.That(MathUtilities.FormattedPercent(99.999, 1, 2), Is.EqualTo("99.99%"));
			Assert.That(MathUtilities.FormattedPercent(99.99999, 1, 4), Is.EqualTo("99.9999%"));
			Assert.That(MathUtilities.FormattedPercent(99.9999995, 1, 6), Is.EqualTo("99.999999%"));
		}
	}
}
