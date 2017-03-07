using System.Linq;
using System.Xml;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using SIL.DblBundle;
using SIL.DblBundle.Tests.Usx;
using SIL.DblBundle.Usx;

namespace GlyssenTests
{
	[TestFixture]
	class UsxParserTests
	{
		const string kUsxFrameWithGlobalChapterLabel = UsxDocumentTests.kUsxFrameStart +
			"<para style=\"cl\">Global-Chapter</para>" +
			UsxDocumentTests.kUsxChapter1AndContentPlaceholder +
			UsxDocumentTests.kUsxFrameEnd;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			Block.FormatChapterAnnouncement = null;
		}

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
			Assert.IsTrue(blocks[1].CharacterId == Block.kNotSet);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(false));
			Assert.AreEqual("{1}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, {2}\u00A0kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));
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
			Assert.AreEqual("{3}\u00A0dwan dano mo ma daŋŋe ki i tim ni,", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerseAndNote_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
										"“pe, kadi ki acel.” <verse number=\"3\" /><note /> “Guŋamo doggi calo lyel ma twolo,”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("“pe, kadi ki acel.” “Guŋamo doggi calo lyel ma twolo,”", blocks[1].GetText(false));
			Assert.AreEqual("“pe, kadi ki acel.” {3}\u00A0“Guŋamo doggi calo lyel ma twolo,”", blocks[1].GetText(true));
			Assert.AreEqual("“pe, kadi ki acel.” ", ((ScriptText)blocks[1].BlockElements[0]).Content);
			Assert.AreEqual("“Guŋamo doggi calo lyel ma twolo,”", ((ScriptText)blocks[1].BlockElements[2]).Content);
		}

		[Test]
		public void Parse_ParagraphStartsWithOpeningSquareBracketBeforeVerseNumber_InitialStartVerseNumberIsBasedOnVerseNumberFollowingBracket()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Markus</para>" +
				"<chapter number=\"16\" style=\"c\" />" +
				"<para style=\"p\"><verse number=\"8\" />Trembling, the women fled because they were afraid.</para>" +
				"<para style=\"p\">[<verse number=\"9\" />When Jesus rose, he first appeared to Mary. <verse number=\"10\" />" +
				"She told those who were weeping. <verse number=\"11\" />They didn't believe it.]</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("{8}\u00A0Trembling, the women fled because they were afraid.", blocks[2].GetText(true));
			Assert.AreEqual("[{9}\u00A0When Jesus rose, he first appeared to Mary. " +
							"{10}\u00A0She told those who were weeping. " +
							"{11}\u00A0They didn't believe it.]", blocks[3].GetText(true));
			Assert.IsTrue(blocks[2].StartsAtVerseStart);
			Assert.AreEqual(8, blocks[2].InitialStartVerseNumber);
			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(9, blocks[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerseAndNoteWithFollowingVerse_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"> <verse number=\"1\" /> <note /> Pi <verse number=\"2\" />Wan </para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("Pi Wan ", blocks[1].GetText(false));
			Assert.AreEqual("{1}\u00A0Pi {2}\u00A0Wan ", blocks[1].GetText(true));
			Assert.AreEqual("Pi ", ((ScriptText)blocks[1].BlockElements[1]).Content);
			Assert.AreEqual("Wan ", ((ScriptText)blocks[1].BlockElements[3]).Content);
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerse_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
										"“pe, kadi ki acel.” <verse number=\"3\" /> “Guŋamo doggi calo lyel ma twolo,”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("“pe, kadi ki acel.” “Guŋamo doggi calo lyel ma twolo,”", blocks[1].GetText(false));
			Assert.AreEqual("“pe, kadi ki acel.” {3}\u00A0“Guŋamo doggi calo lyel ma twolo,”", blocks[1].GetText(true));
			Assert.AreEqual("“pe, kadi ki acel.” ", ((ScriptText)blocks[1].BlockElements[0]).Content);
			Assert.AreEqual("“Guŋamo doggi calo lyel ma twolo,”", ((ScriptText)blocks[1].BlockElements[2]).Content);
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
			Assert.AreEqual("Text before figure. {2}\u00A0Text after figure.", blocks[1].GetText(true));
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
			Assert.AreEqual("{2}\u00A0Text", blocks[1].GetText(true));
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
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[1].GetText(false));
			Assert.AreEqual("Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. {13}\u00A0Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_ChapterAndPara_BecomeTwoBlocks()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(false));
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
		}

		[Test]
		public void Parse_GlobalChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>", kUsxFrameWithGlobalChapterLabel);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("Global-Chapter 1", blocks[0].GetText(false));
			Assert.AreEqual("BC-MRK", blocks[0].CharacterId);
			Assert.True(blocks[0].IsParagraphStart);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.True(blocks[1].IsParagraphStart);
		}

		[Test]
		public void Parse_SpecificChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"cl\">Specific-Chapter One</para><para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("Specific-Chapter One", blocks[0].GetText(false));
			Assert.AreEqual("BC-MRK", blocks[0].CharacterId);
			Assert.True(blocks[0].IsParagraphStart);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.True(blocks[1].IsParagraphStart);
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
			Assert.IsTrue(blocks[0].IsChapterAnnouncement);
			Assert.AreEqual("MRK", blocks[0].BookCode);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(true));
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, {2}\u00A0kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));

			Assert.AreEqual("c", blocks[2].StyleTag);
			Assert.IsTrue(blocks[2].IsChapterAnnouncement);
			Assert.AreEqual("MRK", blocks[2].BookCode);
			Assert.AreEqual(2, blocks[2].ChapterNumber);
			Assert.AreEqual(0, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("2", blocks[2].GetText(true));
			Assert.AreEqual("p", blocks[3].StyleTag);
			Assert.AreEqual(2, blocks[3].ChapterNumber);
			Assert.AreEqual(1, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[3].GetText(true));
			Assert.AreEqual("q1", blocks[4].StyleTag);
			Assert.AreEqual(2, blocks[4].ChapterNumber);
			Assert.AreEqual(1, blocks[4].InitialStartVerseNumber);
			Assert.AreEqual("This is poetry, dude.", blocks[4].GetText(true));
		}

		[TestCase("p", "q1", "q2")]
		[TestCase("p", "q", "m")]
		[TestCase("p", "pi1", "pi2")]
		[TestCase("q1", "q2", "m")]
		public void Parse_TwoPoetryLinesInSingleVerseWithNoInterveningSentenceEndingPunctuation_DoNotSplitPoetryLinesIntoSeparateBlocks(string style1, string style2, string style3)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc($"<para style=\"{style1}\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>" +
										$"<para style=\"{style2}\">" +
										"<verse number=\"2\" style=\"v\" />" +
										"This is a poem, </para>" +
										$"<para style=\"{style3}\">" +
										"about something good.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("c", blocks[0].StyleTag);
			Assert.IsTrue(blocks[0].IsChapterAnnouncement);
			Assert.AreEqual("MRK", blocks[0].BookCode);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(true));
			Assert.AreEqual(style1, blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[1].GetText(true));
			Assert.AreEqual(style2, blocks[2].StyleTag);
			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(2, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("{2}\u00A0This is a poem, about something good.", blocks[2].GetText(true));
		}

		[Test]
		public void Parse_MultiplePoetryLinesInSingleVerseWithNoInterveningSentenceEndingPunctuation_DoNotSplitPoetryLinesIntoSeparateBlocks()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"This is what the LORD says\u2014</para>" +
										"<para style=\"q2\">" +
										"the Redeemer and Holy One of Israel\u2014</para>" +
										"<para style=\"q1\">" +
										"to him who was despised and abhorred by the nation,</para>" +
										"<para style=\"q2\">" +
										"to the servant of rulers:</para>" +
										"<para style=\"q1\">" +
										"“Kings will see you and rise up,</para>" +
										"<para style=\"q2\">" +
										"princes will see and bow down,</para>" +
										"<para style=\"q1\">" +
										"because of the LORD, who is faithful,</para>" +
										"<para style=\"q2\">" +
										"the Holy One of Israel, who has chosen you.”</para>" +
										"<para style=\"q1\">" +
										"<verse number=\"2\" style=\"v\" />" +
										"This is what the LORD says:</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("c", blocks[0].StyleTag);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0This is what the LORD says\u2014 the Redeemer and Holy One of Israel\u2014 to him who " +
							"was despised and abhorred by the nation, to the servant of rulers: “Kings will see you and rise up, " +
							"princes will see and bow down, because of the LORD, who is faithful, the Holy One of Israel, who " +
							"has chosen you.”", blocks[1].GetText(true));
			Assert.AreEqual("q1", blocks[2].StyleTag);
			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(2, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("{2}\u00A0This is what the LORD says:", blocks[2].GetText(true));
		}

		[TestCase("p", "q1", "q2")]
		[TestCase("p", "q", "m")]
		[TestCase("p", "pi1", "pi2")]
		[TestCase("q1", "q2", "m")]
		public void Parse_PoetryLinesInDifferentVersesWithNoInterveningSentenceEndingPunctuation_VersesAreNotCombined(string style1, string style2, string style3)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc($"<para style=\"{style1}\">" +
										"<verse number=\"1\" style=\"v\" />" +
										"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>" +
										$"<para style=\"{style2}\">" +
										"<verse number=\"2\" style=\"v\" />" +
										"This is a poem, </para>" +
										$"<para style=\"{style3}\">" +
										"about something good;</para>" +
										$"<para style=\"{style2}\">" +
										"<verse number=\"3\" style=\"v\" />" +
										"So you can see that</para>" +
										$"<para style=\"{style3}\">" +
										"it's not about something wood.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("c", blocks[0].StyleTag);
			Assert.IsTrue(blocks[0].IsChapterAnnouncement);
			Assert.AreEqual("MRK", blocks[0].BookCode);
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.AreEqual("1", blocks[0].GetText(true));
			Assert.AreEqual(style1, blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[1].GetText(true));
			Assert.AreEqual(style2, blocks[2].StyleTag);
			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(2, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("{2}\u00A0This is a poem, about something good;", blocks[2].GetText(true));
			Assert.AreEqual(style2, blocks[3].StyleTag);
			Assert.AreEqual(1, blocks[3].ChapterNumber);
			Assert.AreEqual(3, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual("{3}\u00A0So you can see that it's not about something wood.", blocks[3].GetText(true));
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
			Assert.AreEqual(12, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(false));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(13, blocks[2].InitialStartVerseNumber);
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
			Assert.AreEqual(12, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(false));
			Assert.AreEqual("{12-14}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,", blocks[1].GetText(true));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(15, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(false));
			Assert.AreEqual("{15-18}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(true));
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
			Assert.AreEqual(0, blocks[0].InitialStartVerseNumber);
			Assert.IsTrue(blocks[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual(0, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(0, blocks[2].InitialStartVerseNumber);
			Assert.IsTrue(blocks[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual(1, blocks[3].ChapterNumber);
			Assert.AreEqual(1, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(Block.kNotSet, blocks[3].CharacterId);
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
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("s", blocks[1].StyleTag);
			Assert.IsTrue(blocks[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("John the Baptist prepares the way", blocks[1].GetText(false));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(1, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("p", blocks[2].StyleTag);
			Assert.AreEqual(Block.kNotSet, blocks[2].CharacterId);
			Assert.AreEqual("Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[2].GetText(false));
		}

		[Test]
		public void Parse_UnpublishableText_NonpublishableDataExcluded()
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
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(Block.kNotSet, blocks[1].CharacterId);
			Assert.AreEqual("{1-2}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni, “Nen, acwalo lakwenana otelo nyimi,", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_TitleFollowedByChapter_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<chapter number=\"1\" style=\"c\" />" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
			Assert.AreEqual("header", parser.PageHeader);
			Assert.AreEqual("Mark", parser.MainTitle);
		}

		[Test]
		public void Parse_TitleNotFollowedByChapter_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<para style=\"s1\">section</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
			Assert.AreEqual("header", parser.PageHeader);
			Assert.AreEqual("Mark", parser.MainTitle);
		}

		[Test]
		public void Parse_TwoChapters_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"h\">Marco</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Markus</para>" +
				"<chapter number=\"1\" style=\"c\" />" +
				"<chapter number=\"2\" style=\"c\" />" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Markus", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Markus", blocks[0].GetText(true));
			Assert.AreEqual("Marco", parser.PageHeader);
			Assert.AreEqual("Markus", parser.MainTitle);
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

		[Test]
		public void Parse_VerseBridge_SingleParagraph()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
							"<verse number=\"1-3\" style=\"v\" />" +
							"Verse 1-3 text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(3, blocks[1].InitialEndVerseNumber);
		}

		[Test]
		public void Parse_VerseBridge_AcrossMultipleParagraphs()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
							"<verse number=\"1-3\" style=\"v\" />" +
							"Verse 1-3 beginning text" +
							"</para><para style=\"p\">" +
							"Verse 1-3 continuing text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(3, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual(1, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(3, blocks[2].InitialEndVerseNumber);
		}

		[Test]
		public void Parse_NodeWithNoChildren_IgnoresNode()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1\" style=\"v\" />এই হল যীশু খ্রীষ্টের বংশ তালিকা৷ ইনি ছিলেন রাজা দায়ূদের বংশধর, দায়ূদ ছিলেন অব্রাহামের বংশধর৷</para>" +
				"<para style=\"b\" />" +
				"<para style=\"li\">" +
				"<verse number=\"2\" style=\"v\" />অব্রাহামের ছেলে ইসহাক৷</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("c", blocks[0].StyleTag);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual("li", blocks[2].StyleTag);
		}

		[Test]
		public void Parse_WhitespaceBetweenCharAndNoteElements_WhitespaceIsIgnored()
		{
			// World English Bible, MAT 5:27, PG-593
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"27\" style=\"v\" />\r\n" +
				"    <char style=\"wj\">“You have heard that it was said, </char>\r\n" +
				"    <note caller=\"+\" style=\"f\">TR adds “to the ancients”.</note> <char style=\"wj\">‘You shall not commit adultery;’</char><note caller=\"+\" style=\"x\">Exodus 20:14</note> <verse number=\"28\" style=\"v\" /><char style=\"wj\">but I tell you that everyone who gazes at a woman to lust after her has committed adultery with her already in his heart. </char> <verse number=\"29\" style=\"v\" /><char style=\"wj\">If your right eye causes you to stumble, pluck it out and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna.</char><note caller=\"+\" style=\"f\">or, Hell</note> <verse number=\"30\" style=\"v\" /><char style=\"wj\">If your right hand causes you to stumble, cut it off, and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna.</char><note caller=\"+\" style=\"f\">or, Hell</note></para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(8, blocks[1].BlockElements.Count);
			Assert.AreEqual("“You have heard that it was said, ‘You shall not commit adultery;’ but I tell you that everyone who gazes at a woman to lust after her has committed adultery with her already in his heart. If your right eye causes you to stumble, pluck it out and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna. If your right hand causes you to stumble, cut it off, and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna.", blocks[1].GetText(false));
			Assert.AreEqual("{27}\u00A0“You have heard that it was said, ‘You shall not commit adultery;’ {28}\u00A0but I tell you that everyone who gazes at a woman to lust after her has committed adultery with her already in his heart. {29}\u00A0If your right eye causes you to stumble, pluck it out and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna. {30}\u00A0If your right hand causes you to stumble, cut it off, and throw it away from you. For it is more profitable for you that one of your members should perish, than for your whole body to be cast into Gehenna.", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_VerseAtEndOfParagraphConsistsEntirelyOfNote_DoNotIncludeVerseNumber()
		{
			// World English Bible, LUK 17:36, PG-594
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.”</char> <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left.” ", blocks[1].GetText(true));
			Assert.AreEqual(2, blocks[2].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[2].GetText(true));
		}

		[Test]
		public void Parse_OnlyVerseInParagraphConsistsEntirelyOfNote_DoNotIncludeParagraph()
		{
			// World English Bible, LUK 17:36, PG-594
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[1].GetText(true));
		}

		[Test]
		public void Parse_VerseMidParagraphConsistsEntirelyOfNote_DoNotIncludeVerseNumber()
		{
			// World English Bible, LUK 17:36, PG-594
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.”</char> <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note> <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(4, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left.” {37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[1].GetText(true));
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
