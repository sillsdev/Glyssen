using NUnit.Framework;
using System.Xml.Linq;

namespace GlyssenEngineTests
{
	internal class XmlComparisonTestUtils
	{
		public static void AssertXmlEqual(string expectedXml, string actualXml)
		{
			var expected = XDocument.Parse(expectedXml);
			var actual = XDocument.Parse(actualXml);
			Assert.That(XNode.DeepEquals(expected, actual), Is.True,
				"XML did not match expected XML (ignoring whitespace).");
		}

	}
}
