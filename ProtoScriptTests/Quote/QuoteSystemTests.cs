using NUnit.Framework;
using Palaso.TestUtilities;
using ProtoScript.Quote;
using SIL.Xml;

namespace ProtoScriptTests.Quote
{
	[TestFixture]
	class QuoteSystemTests
	{
		[Test]
		public void GetCorrespondingFirstLevelQuoteSystem_SameSystem()
		{
			var french = QuoteSystem.GetOrCreateQuoteSystem("«", "»", null, null, false); //Guillemets -- French
			Assert.AreEqual(french, french.GetCorrespondingFirstLevelQuoteSystem());
		}

		[Test]
		public void GetCorrespondingFirstLevelQuoteSystem_WithQuoteDash()
		{
			var turkish = QuoteSystem.GetOrCreateQuoteSystem("«", "»", "—", null, false); //Tırnak işareti (with 2014 Quotation dash) -- Turkish/Vietnamese
			var french = QuoteSystem.GetOrCreateQuoteSystem("«", "»", null, null, false); //Guillemets -- French
			var english = QuoteSystem.GetOrCreateQuoteSystem("“", "”", null, null, false); //Quotation marks, double -- English, US/Canada
			Assert.AreEqual(french, turkish.GetCorrespondingFirstLevelQuoteSystem());
			Assert.AreNotEqual(english, turkish.GetCorrespondingFirstLevelQuoteSystem());
		}

		[Test]
		public void GetCorrespondingFirstLevelQuoteSystem_AllAdditionalFields()
		{
			var madeup = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "―", "―", true); //Made-up
			var french = QuoteSystem.GetOrCreateQuoteSystem("«", "»", null, null, false); //Guillemets -- French
			var english = QuoteSystem.GetOrCreateQuoteSystem("“", "”", null, null, false); //Quotation marks, double -- English, US/Canada
			Assert.AreEqual(english, madeup.GetCorrespondingFirstLevelQuoteSystem());
			Assert.AreNotEqual(french, madeup.GetCorrespondingFirstLevelQuoteSystem());
		}

		[Test]
		public void Serialize()
		{
			var qs = new QuoteSystem { Name = "Guillemets", StartQuoteMarker = "«", EndQuoteMarker = "»", MajorLanguage = "French" };
			string xml = XmlSerializationHelper.SerializeToString(qs);
			const string expectedResult = @"<QuoteSystem>
	<Name>Guillemets</Name>
	<MajorLanguage>French</MajorLanguage>
	<StartQuoteMarker>«</StartQuoteMarker>
	<EndQuoteMarker>»</EndQuoteMarker>
</QuoteSystem>";
			AssertThatXmlIn.String(expectedResult).EqualsIgnoreWhitespace(xml);
		}
	}
}
