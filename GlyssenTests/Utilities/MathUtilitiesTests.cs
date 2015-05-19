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
	}
}
