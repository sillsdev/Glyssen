using System.Linq;
using GlyssenEngine.Quote;
using NUnit.Framework;
using SIL.WritingSystems;

namespace GlyssenTests.Quote
{
	[TestFixture]
	public class QuoteUtilsTests
	{
		[Test]
		public void GetLevel2Possibilities_OnePossibility()
		{
			var level1 = new QuotationMark("»", "«", "»", 1, QuotationMarkingSystemType.Normal);
			var level2Possibilities = QuoteUtils.GetLevel2Possibilities(level1);
			Assert.AreEqual(1, level2Possibilities.Count());
			var level2 = level2Possibilities.First();
			Assert.AreEqual("›", level2.Open);
			Assert.AreEqual("‹", level2.Close);
		}

		[Test]
		public void GetLevel2Possibilities_MultiplePossibilities()
		{
			var level1 = new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal);
			var level2Possibilities = QuoteUtils.GetLevel2Possibilities(level1);
			Assert.AreEqual(6, level2Possibilities.Count());
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("“") && p.Close.Equals("”") && p.Continue.Equals("« “")));
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("‹") && p.Close.Equals("›") && p.Continue.Equals("« ‹")));
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("«") && p.Close.Equals("»") && p.Continue.Equals("« «")));
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("„") && p.Close.Equals("“") && p.Continue.Equals("« „")));
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("’") && p.Close.Equals("’") && p.Continue.Equals("« ’")));
			Assert.AreEqual(1, level2Possibilities.Count(p => p.Open.Equals("‘") && p.Close.Equals("’") && p.Continue.Equals("« ‘")));
		}

		[Test]
		public void GetLevel2Default_OnePossibility()
		{
			var level1 = new QuotationMark("»", "«", "»", 1, QuotationMarkingSystemType.Normal);
			var level2 = QuoteUtils.GetLevel2Default(level1);
			Assert.AreEqual("›", level2.Open);
			Assert.AreEqual("‹", level2.Close);
			Assert.AreEqual("›", level2.Continue);
			Assert.AreEqual(2, level2.Level);
			Assert.AreEqual(QuotationMarkingSystemType.Normal, level2.Type);
		}

		[Test]
		public void GetLevel2Default_MultiplePossibilities()
		{
			var level1 = new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal);
			var level2 = QuoteUtils.GetLevel2Default(level1);
			Assert.AreEqual("“", level2.Open);
			Assert.AreEqual("”", level2.Close);
			Assert.AreEqual("“", level2.Continue);
			Assert.AreEqual(2, level2.Level);
			Assert.AreEqual(QuotationMarkingSystemType.Normal, level2.Type);
		}
	}
}
