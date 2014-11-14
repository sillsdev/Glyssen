using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxParaParserTests
	{
		const string usxFrame = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>" +
			"<chapter number=\"1\" style=\"c\" />" +
			"{0}" +
			"</usx>";

		private XmlDocument CreateMarkOneDoc(string paraXmlNodes)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(string.Format(usxFrame, paraXmlNodes));
			return xmlDoc;
		}

		[Test]
		public void Parse_SingleNarratorParagraphWithVerseNumbers_GeneratesSingleNarratorBlock()
		{
			var doc = CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
										"<verse number=\"2\" style=\"v\" />" +
										"kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>");
			var parser = new UsxParaParser(doc.GetElementsByTagName("para"));
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.IsTrue(blocks[0].IsNarrator);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(false));
			Assert.AreEqual("[1]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, [2]kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(true));
		}
	}
}
