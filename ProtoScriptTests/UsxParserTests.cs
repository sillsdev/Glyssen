using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Bundle;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxParserTests
	{
		const string usxFrame = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>" +
			"<chapter number=\"1\" style=\"c\" />" +
			"{0}" +
			"</usx>";
		const string usxFrameWithGlobalChapterLabel = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>" +
			"<para style=\"cl\">Global-Chapter</para>" +
			"<chapter number=\"1\" style=\"c\" />" +
			"{0}" +
			"</usx>";

		private XmlDocument CreateMarkOneDoc(string paraXmlNodes, string usxFrame = usxFrame)
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
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.IsTrue(blocks[0].IsNarrator);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(false));
			Assert.AreEqual("[1]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, [2]kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithNote_NoteIsIgnored()
		{
			var doc = CreateMarkOneDoc("<para style=\"q1\">" +
										"<verse number=\"3\" style=\"v\" />" +
										"<note caller=\"-\" style=\"x\">" +
										"<char style=\"xo\" closed=\"false\">1.3: </char>" +
										"<char style=\"xt\" closed=\"false\">Ic 40.3</char>" +
										"</note>dwan dano mo ma daŋŋe ki i tim ni,</para>");
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("dwan dano mo ma daŋŋe ki i tim ni,", blocks[0].GetText(false));
			Assert.AreEqual("[3]dwan dano mo ma daŋŋe ki i tim ni,", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithFigure_FigureIsIgnored()
		{
			var doc = CreateMarkOneDoc("<para style=\"p\"><verse number=\"18\" style=\"v\" />" +
										"Ci cutcut gutugi weko obwogi, gulubo kore." +
										"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
										"Cutcut gutugi weko obwugi</figure></para >");
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual(2, blocks[0].BlockElements.Count);
			Assert.AreEqual("Ci cutcut gutugi weko obwogi, gulubo kore.", blocks[0].GetText(false));
		}

		[Test]
		public void Parse_ParagraphWithFigureInMiddle_FigureIsIgnored()
		{
			var doc = CreateMarkOneDoc("<para style=\"p\">" +
										"This text is before the figure, " +
										"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
										"Cutcut gutugi weko obwugi</figure>" +
										"and this text is after.</para >");
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual(1, blocks[0].BlockElements.Count);
			Assert.AreEqual("This text is before the figure, and this text is after.", blocks[0].GetText(false));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerse()
		{
			var doc = CreateMarkOneDoc("<para style=\"q1\">ma bigero yoni;</para>");
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
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
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[0].GetText(false));
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. [13]Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_ChapterAndPara_BecomeTwoBlocks()
		{
			var doc = CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = new UsxParser(new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("1", blocks[0].GetText(false));
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_GlobalChapterLabel()
		{
			var doc = CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>", usxFrameWithGlobalChapterLabel);
			var parser = new UsxParser(new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("Global-Chapter 1", blocks[0].GetText(false));
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_SpecificChapterLabel()
		{
			var doc = CreateMarkOneDoc("<para style=\"cl\">Specific-Chapter One</para><para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = new UsxParser(new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("Specific-Chapter One", blocks[0].GetText(false));
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test, Ignore("TODO")]
		public void Parse_ParaWithQuotes_BecomesTwoBlocks()
		{
			var doc = CreateMarkOneDoc("<verse number=\"19\" style=\"v\" />Yecu ocako gamo ni, “Wun yalwak ma niyewu lam, abibedo kwedwu nio wa awene? Dok abidiyo cwinya i komwu nio wa awene ba? Wukel en bota kany.” ");
			var parser = new UsxParser(new UsxDocument(doc).GetParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("Yecu ocako gamo ni, ", blocks[0].GetText(false));
			Assert.AreEqual("[19]Yecu ocako gamo ni, ", blocks[0].GetText(true));
			Assert.AreEqual("“Wun yalwak ma niyewu lam, abibedo kwedwu nio wa awene? Dok abidiyo cwinya i komwu nio wa awene ba? Wukel en bota kany.”", blocks[1].GetText(false));
		}
	}
}
