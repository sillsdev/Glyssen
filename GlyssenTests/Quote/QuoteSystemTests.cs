using NUnit.Framework;
using SIL.WritingSystems;
using SIL.Xml;
using Waxuquerque.Quote;

namespace WaxuquerqueTests.Quote
{
	[TestFixture]
	class QuoteSystemTests
	{
		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_SameSystem()
		//{
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	Assert.AreEqual(french, french.GetCorrespondingFirstLevelQuoteSystem());
		//	new QuotationMark("", "", "", 1, QuotationMarkingSystemType.Normal);
		//}

		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_WithQuoteDash()
		//{
		//	var turkish = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), "—", null); //Tırnak işareti (with 2014 Quotation dash) -- Turkish/Vietnamese
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	var english = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null); //Quotation marks, double -- English, US/Canada
		//	Assert.AreEqual(french, turkish.GetCorrespondingFirstLevelQuoteSystem());
		//	Assert.AreNotEqual(english, turkish.GetCorrespondingFirstLevelQuoteSystem());
		//}

		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_AllAdditionalFields()
		//{
		//	var madeup = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", "―"); //Made-up
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	var english = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null); //Quotation marks, double -- English, US/Canada
		//	Assert.AreEqual(english, madeup.GetCorrespondingFirstLevelQuoteSystem());
		//	Assert.AreNotEqual(french, madeup.GetCorrespondingFirstLevelQuoteSystem());
		//}

		[Test]
		public void Deserialize()
		{
			const string input = @"<QuoteSystem>
					<Name>Virgolette (with opening and closing 2014 Quotation dash)</Name>
					<MajorLanguage>Italian</MajorLanguage>
					<StartQuoteMarker>“</StartQuoteMarker>
					<EndQuoteMarker>”</EndQuoteMarker>
					<QuotationDashMarker>—</QuotationDashMarker>
					<QuotationDashEndMarker>—</QuotationDashEndMarker>
				</QuoteSystem>";
			var quoteSystem = XmlSerializationHelper.DeserializeFromString<QuoteSystem>(input);
			Assert.AreEqual("Virgolette (with opening and closing 2014 Quotation dash)", quoteSystem.Name);
			Assert.AreEqual("Italian", quoteSystem.MajorLanguage);
			Assert.AreEqual(2, quoteSystem.AllLevels.Count);
			Assert.AreEqual(1, quoteSystem.NormalLevels.Count);
			Assert.AreEqual("“", quoteSystem.FirstLevel.Open);
			Assert.AreEqual("”", quoteSystem.FirstLevel.Close);
			Assert.AreEqual("“", quoteSystem.FirstLevel.Continue);
			Assert.AreEqual(1, quoteSystem.FirstLevel.Level);
			Assert.AreEqual(QuotationMarkingSystemType.Normal, quoteSystem.FirstLevel.Type);
			Assert.AreEqual("—", quoteSystem.QuotationDashMarker);
			Assert.AreEqual("—", quoteSystem.QuotationDashEndMarker);
		}
	}
}
