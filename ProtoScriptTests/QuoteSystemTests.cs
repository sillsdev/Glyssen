using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.Xml;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class QuoteSystemTests
	{
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
