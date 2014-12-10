using System.Linq;
using System.Xml;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Bundle;

namespace ProtoScriptTests
{
	[TestFixture]
	class UsxParserTests
	{
		const string usxFrameWithGlobalChapterLabel = UsxDocumentTests.usxFrameStart +
			"<para style=\"cl\">Global-Chapter</para>" +
			UsxDocumentTests.usxChapter1AndContentPlaceholder +
			UsxDocumentTests.usxFrameEnd;

		[Test]
		public void Parse_SingleNarratorParagraphWithVerseNumbers_GeneratesSingleNarratorBlock()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
										"<verse number=\"2\" style=\"v\" />" +
										"kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.IsTrue(blocks[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.IsTrue(blocks[1].CharacterId == Block.NotSet);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(false));
			Assert.AreEqual("[1]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, [2]kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithNote_NoteIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
										"<verse number=\"3\" style=\"v\" />" +
										"<note caller=\"-\" style=\"x\">" +
										"<char style=\"xo\" closed=\"false\">1.3: </char>" +
										"<char style=\"xt\" closed=\"false\">Ic 40.3</char>" +
										"</note>dwan dano mo ma daŋŋe ki i tim ni,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("dwan dano mo ma daŋŋe ki i tim ni,", blocks[1].GetText(false));
			Assert.AreEqual("[3]dwan dano mo ma daŋŋe ki i tim ni,", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithFigure_FigureIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"><verse number=\"18\" style=\"v\" />" +
										"Ci cutcut gutugi weko obwogi, gulubo kore." +
										"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
										"Cutcut gutugi weko obwugi</figure></para >");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("Ci cutcut gutugi weko obwogi, gulubo kore.", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_ParagraphWithFigureInMiddle_FigureIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"This text is before the figure, " +
										"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
										"Cutcut gutugi weko obwugi</figure>" +
										"and this text is after.</para >");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[1].BlockElements.Count);
			Assert.AreEqual("This text is before the figure, and this text is after.", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_SpaceAfterFigureBeforeVerseMaintained()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"Text before figure." +
										"<figure /> <verse number=\"2\" />Text after figure.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(3, blocks[1].BlockElements.Count);
			Assert.AreEqual("Text before figure. Text after figure.", blocks[1].GetText(false));
			Assert.AreEqual("Text before figure. [2]Text after figure.", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithCharacterStyle_DataIsIncluded()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"If you don't always remember things, you will " +
										"<char style=\"b\">" +
										"sometimes</char>" +
										" forget!</para >");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("If you don't always remember things, you will sometimes forget!", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_WhitespaceAtBeginningOfParaNotPreserved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"> <verse number=\"2\" />Text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("Text", blocks[1].GetText(false));
			Assert.AreEqual("[2]Text", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerse()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">ma bigero yoni;</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("ma bigero yoni;", blocks[1].GetText(false));
			Assert.AreEqual("ma bigero yoni;", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerseAndHasAnotherVerse()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. " +
										"<verse number=\"13\" style=\"v\" />Ci obedo i tim nino pyeraŋwen; Catan ocako bite, " +
										"ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[1].GetText(false));
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. [13]Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ChapterAndPara_BecomeTwoBlocks()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(false));
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_GlobalChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>", usxFrameWithGlobalChapterLabel);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialVerseNumber);
			Assert.AreEqual("Global-Chapter 1", blocks[0].GetText(false));
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_SpecificChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"cl\">Specific-Chapter One</para><para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialVerseNumber);
			Assert.AreEqual("Specific-Chapter One", blocks[0].GetText(false));
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_ProcessChaptersAndVerses_BlocksGetCorrectChapterAndVerseNumbers()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
										"<verse number=\"2\" style=\"v\" />" +
										"kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>" +
										"<chapter number=\"2\" style=\"c\" />" +
										"<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>" +
										"<para style=\"q1\">" +
										"This is poetry, dude.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual("c", blocks[0].StyleTag);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(true));
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialVerseNumber);
			Assert.AreEqual("[1]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, [2]kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));

			Assert.AreEqual("c", blocks[2].StyleTag);
			Assert.AreEqual(2, blocks[2].ChapterNumber);
			Assert.AreEqual(0, blocks[2].InitialVerseNumber);
			Assert.AreEqual("2", blocks[2].GetText(true));
			Assert.AreEqual("p", blocks[3].StyleTag);
			Assert.AreEqual(2, blocks[3].ChapterNumber);
			Assert.AreEqual(1, blocks[3].InitialVerseNumber);
			Assert.AreEqual("[1]Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[3].GetText(true));
			Assert.AreEqual("q1", blocks[4].StyleTag);
			Assert.AreEqual(2, blocks[4].ChapterNumber);
			Assert.AreEqual(1, blocks[4].InitialVerseNumber);
			Assert.AreEqual("This is poetry, dude.", blocks[4].GetText(true));
		}

		[Test]
		public void Parse_ParaStartsWithVerseNumber_BlocksGetCorrectChapterAndVerseNumbers()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"12\" style=\"v\" />" +
										"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,</para>" +
										"<para style=\"p\">" +
										"<verse number=\"13\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(12, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(false));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(13, blocks[2].InitialVerseNumber);
			Assert.AreEqual("Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(false));
		}

		[Test]
		public void Parse_VerseRange_BlocksGetCorrectStartingVerseNumber()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"12-14\" style=\"v\" />" +
										"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,</para>" +
										"<para style=\"p\">" +
										"<verse number=\"15-18\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(12, blocks[1].InitialVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(false));
			Assert.AreEqual("[12-14]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(true));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(15, blocks[2].InitialVerseNumber);
			Assert.AreEqual("Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(false));
			Assert.AreEqual("[15-18]Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(true));
		}

		[Test]
		public void Parse_Intro_BlocksGetIntroCharacter()
		{
			var doc = new XmlDocument { PreserveWhitespace = true };
			doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<usx version=\"2.0\">" +
			"<book code=\"MRK\" style=\"id\">Acholi Bible 1985 Digitised by Africa Typesetting Network for DBL April 2013</book>" +
			"<para style=\"is\">About the Author</para>" +
			"<para style=\"ip\">Mark was a great guy.</para>" +
			"<chapter number=\"1\" style=\"c\" />" +
			"<para style=\"p\">" +
			"<verse number=\"1\" style=\"v\" />" +
			"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, </para>" +
			"</usx>");

			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(0, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialVerseNumber);
			Assert.IsTrue(blocks[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual(0, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialVerseNumber);
			Assert.IsTrue(blocks[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(0, blocks[2].InitialVerseNumber);
			Assert.IsTrue(blocks[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual(1, blocks[3].ChapterNumber);
			Assert.AreEqual(1, blocks[3].InitialVerseNumber);
			Assert.AreEqual(Block.NotSet, blocks[3].CharacterId);
		}

		[Test]
		public void Parse_SectionHeads_SectionHeadBlocksGetExtraBiblicalCharacterAndParallelPassageReferencesAreOmitted()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s\">" +
										"John the Baptist prepares the way</para>" +
										"<para style=\"r\">" +
										"Matthew 3:1-12; Luke 3:1-20</para>" +
										"<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialVerseNumber);
			Assert.AreEqual("s", blocks[1].StyleTag);
			Assert.IsTrue(blocks[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("John the Baptist prepares the way", blocks[1].GetText(false));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(1, blocks[2].InitialVerseNumber);
			Assert.AreEqual("p", blocks[2].StyleTag);
			Assert.AreEqual(Block.NotSet, blocks[2].CharacterId);
			Assert.AreEqual("Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(false));
		}

		[Test]
		public void Parse_UnpublishableText_NonpublishableDatExcluded()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
							"<verse number=\"1-2\" style=\"v\" />" +
							"Acakki me lok me kwena maber i kom Yecu Kricito" +
							"<char style=\"pro\">Crissitu</char>" +
							", Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>" +
							"<para style=\"rem\">" +
							"Tom was here!</para>" +
							"<para style=\"q1\">" +
							"“Nen, acwalo lakwenana otelo nyimi,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialVerseNumber);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(Block.NotSet, blocks[1].CharacterId);
			Assert.AreEqual("[1-2]Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(1, blocks[2].InitialVerseNumber);
			Assert.AreEqual("q1", blocks[2].StyleTag);
			Assert.AreEqual(Block.NotSet, blocks[2].CharacterId);
			Assert.AreEqual("“Nen, acwalo lakwenana otelo nyimi,", blocks[2].GetText(true));
		}

		[Test]
		public void Parse_TitleFollowedByChapter_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.usxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<chapter number=\"1\" style=\"c\" />" +
				UsxDocumentTests.usxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_TitleNotFollowedByChapter_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.usxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<para style=\"s1\">section</para>" +
				UsxDocumentTests.usxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_TwoChapters_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.usxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<chapter number=\"1\" style=\"c\" />" +
				"<chapter number=\"2\" style=\"c\" />" +
				UsxDocumentTests.usxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
		}

		[Test]
		public void Parse_IsParagraphStart()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
							"<verse number=\"1\" style=\"v\" />" +
							"Verse 1 text</para>" +
							"<para style=\"p\">" +
			                "Verse 2 text" +
							"<verse number=\"2\" style=\"v\" />" +
							"more Verse 2 text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.IsTrue(blocks[0].IsParagraphStart); //chapter
			Assert.IsTrue(blocks[1].IsParagraphStart);
			Assert.IsTrue(blocks[2].IsParagraphStart);
		}

		private UsxParser GetUsxParser(XmlDocument doc)
		{
			return new UsxParser("MRK", new TestStylesheet(), new UsxDocument(doc).GetChaptersAndParas());
		}
	}

	public class TestStylesheet : IStylesheet
	{
		public IStyle GetStyle(string styleId)
		{
			Style style = new Style
			{
				Id = styleId,
				IsPublishable = true,
				IsVerseText = true,
			};

			switch (styleId)
			{
				case "s":
				case "h":
				case "h1":
				case "toc1":
				case "mt":
				case "mt1":
				case "mt2":
					style.IsVerseText = false;
					break;
				case "rem":
				case "restore":
				case "sts":
				case "pro":
				case "pubinfo":
					style.IsPublishable = false;
					style.IsVerseText = false;
					break;
			}

			return style;
		}

		public string FontFamily { get; private set; }
		public int FontSizeInPoints { get; private set; }
	}
}
