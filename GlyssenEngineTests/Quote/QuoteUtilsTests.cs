using System.Linq;
using GlyssenEngine.Quote;
using NUnit.Framework;
using SIL.WritingSystems;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	public class QuoteUtilsTests
	{
		[Test]
		public void GetLevel2Possibilities_OnePossibility()
		{
			var level1 = new QuotationMark("»", "«", "»", 1, QuotationMarkingSystemType.Normal);
			var level2Possibilities = QuoteUtils.GetLevel2Possibilities(level1);
			Assert.That(level2Possibilities.Count(), Is.EqualTo(1));
			var level2 = level2Possibilities.First();
			Assert.That(level2.Open, Is.EqualTo("›"));
			Assert.That(level2.Close, Is.EqualTo("‹"));
		}

		[Test]
		public void GetLevel2Possibilities_MultiplePossibilities()
		{
			var level1 = new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal);
			var level2Possibilities = QuoteUtils.GetLevel2Possibilities(level1);
			Assert.That(level2Possibilities.Count(), Is.EqualTo(6));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("“") && p.Close.Equals("”") && p.Continue.Equals("« “")), Is.EqualTo(1));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("‹") && p.Close.Equals("›") && p.Continue.Equals("« ‹")), Is.EqualTo(1));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("«") && p.Close.Equals("»") && p.Continue.Equals("« «")), Is.EqualTo(1));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("„") && p.Close.Equals("“") && p.Continue.Equals("« „")), Is.EqualTo(1));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("’") && p.Close.Equals("’") && p.Continue.Equals("« ’")), Is.EqualTo(1));
			Assert.That(level2Possibilities.Count(p => p.Open.Equals("‘") && p.Close.Equals("’") && p.Continue.Equals("« ‘")), Is.EqualTo(1));
		}

		[Test]
		public void GetLevel2Default_OnePossibility()
		{
			var level1 = new QuotationMark("»", "«", "»", 1, QuotationMarkingSystemType.Normal);
			var level2 = QuoteUtils.GetLevel2Default(level1);
			Assert.That(level2.Open, Is.EqualTo("›"));
			Assert.That(level2.Close, Is.EqualTo("‹"));
			Assert.That(level2.Continue, Is.EqualTo("›"));
			Assert.That(level2.Level, Is.EqualTo(2));
			Assert.That(QuotationMarkingSystemType.Normal, Is.EqualTo(level2.Type));
		}

		[Test]
		public void GetLevel2Default_MultiplePossibilities()
		{
			var level1 = new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal);
			var level2 = QuoteUtils.GetLevel2Default(level1);
			Assert.That(level2.Open, Is.EqualTo("“"));
			Assert.That(level2.Close, Is.EqualTo("”"));
			Assert.That(level2.Continue, Is.EqualTo("“"));
			Assert.That(level2.Level, Is.EqualTo(2));
			Assert.That(QuotationMarkingSystemType.Normal, Is.EqualTo(level2.Type));
		}
	}
}
