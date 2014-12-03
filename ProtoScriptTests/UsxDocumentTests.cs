using System.Xml;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Bundle;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxDocumentTests
	{
		const string usxFrame = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>" +
			"<chapter number=\"1\" style=\"c\" />" +
			"{0}" +
			"</usx>";

		internal static XmlDocument CreateMarkOneDoc(string paraXmlNodes, string usxFrame = usxFrame)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml(string.Format(usxFrame, paraXmlNodes));
			return xmlDoc;
		}

		[Test]
		public void BookId_GetsBookIdFromBookNode()
		{
			var usxDocument = new UsxDocument(CreateMarkOneDoc(string.Empty));
			Assert.AreEqual("MRK", usxDocument.BookId);
		}
	}
}
