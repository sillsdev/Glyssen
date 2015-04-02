using System.Xml;
using NUnit.Framework;
using ProtoScript.Bundle;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxDocumentTests
	{
		internal const string usxFrameStart = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>";
		internal const string usxChapter1AndContentPlaceholder = "<chapter number=\"1\" style=\"c\" />{0}";
		internal const string usxFrameEnd = "</usx>";
		internal const string usxFrame =
			usxFrameStart +
			usxChapter1AndContentPlaceholder +
			usxFrameEnd;

		internal static XmlDocument CreateMarkOneDoc(string paraXmlNodes, string usxFrame = usxFrame)
		{
			return CreateDocFromString(string.Format(usxFrame, paraXmlNodes));
		}

		internal static XmlDocument CreateDocFromString(string xml)
		{
			var xmlDoc = new XmlDocument { PreserveWhitespace = true };
			xmlDoc.LoadXml(xml);
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
