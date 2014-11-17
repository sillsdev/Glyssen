using System.Linq;
using System.Xml;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Bundle;

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
			var parser = new UsxParaParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.IsTrue(blocks[0].IsNarrator);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(false));
			Assert.AreEqual("[1]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, [2]kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithNote()
		{
			var doc = CreateMarkOneDoc("<para style=\"q1\">" +
										"<verse number=\"3\" style=\"v\" />" +
										"<note caller=\"-\" style=\"x\">" +
										"<char style=\"xo\" closed=\"false\">1.3: </char>" +
										"<char style=\"xt\" closed=\"false\">Ic 40.3</char>" +
										"</note>dwan dano mo ma daŋŋe ki i tim ni,</para>");
			var parser = new UsxParaParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("dwan dano mo ma daŋŋe ki i tim ni,", blocks[0].GetText(false));
			Assert.AreEqual("[3]dwan dano mo ma daŋŋe ki i tim ni,", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerse()
		{
			var doc = CreateMarkOneDoc("<para style=\"q1\">ma bigero yoni;</para>");
			var parser = new UsxParaParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("ma bigero yoni;", blocks[0].GetText(false));
			Assert.AreEqual("ma bigero yoni;", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerseAndHasAnotherVerse()
		{
			var doc = CreateMarkOneDoc("<para style=\"p\">" +
										"Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. " +
										"<verse number=\"13\" style=\"v\" />Ci obedo i tim nino pyeraŋwen; Catan ocako bite, " +
										"ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.</para>");
			var parser = new UsxParaParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[0].GetText(false));
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. [13]Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ChapterAndPara_BecomeTwoBlocks()
		{
			var doc = CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = new UsxParaParser(new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("1", blocks[0].GetText(false));
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}
	}
}
