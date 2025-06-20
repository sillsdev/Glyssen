using System.Xml;
using NUnit.Framework;

namespace GlyssenSharedTests
{
	public static class XmlAssertExtensions
	{
		public static void AssertHasXPathMatchCount(this string xml, string xpath, int expectedCount)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			var nodes = doc.SelectNodes(xpath);
			Assert.That(nodes?.Count ?? 0, Is.EqualTo(expectedCount), $"Expected {expectedCount} match(es) for XPath '{xpath}'.");
		}
	}
}
