using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using NUnit.Framework;
using Paratext.Data;
using SIL.DblBundle;
using SIL.DblBundle.Tests.Usx;
using SIL.DblBundle.Usx;
using Resources = GlyssenEngineTests.Properties.Resources;

namespace GlyssenEngineTests
{
	[TestFixture]
	class UsxParserTests
	{
		const string kUsxFrameWithGlobalChapterLabel = UsxDocumentTests.kUsxFrameStart +
			"<para style=\"cl\">Global-Chapter</para>" +
			UsxDocumentTests.kUsxChapter1AndContentPlaceholder +
			UsxDocumentTests.kUsxFrameEnd;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
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

		[TestCase(". ", "*")]
		[TestCase(".", "?")]
		[TestCase(". [", "...")]
		public void Parse_MissingVerseWithOnlyPunctuation_VerseAndPunctuationOmitted(string v10Ending, string punctInMissingVerse)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Mateo</para>" + Environment.NewLine +
				"<chapter number=\"18\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				$"<verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin{v10Ending}<verse number=\"11\" style=\"v\"/>{punctInMissingVerse} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char><char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note> " +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin" + v10Ending +
				"{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.", blocks[2].GetText(true));
			Assert.IsTrue(blocks[2].StartsAtVerseStart);
			Assert.AreEqual(10, blocks[2].InitialStartVerseNumber);
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		[TestCase("(", ")")]
		[TestCase("\u300c", "\u300d")]
		[TestCase("\uff62", "\uff63")]
		[TestCase("\u3010", "\u3011")]
		[TestCase("\u3014", "\u3015")]
		public void Parse_MissingVerseEnclosedInBrackets_VerseAndPunctuationOmitted(string open, string close)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Mateo</para>" + Environment.NewLine +
				"<chapter number=\"18\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				$"<verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. {open} <verse number=\"11\" style=\"v\"/>{close} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char><char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note> " +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. " +
				"{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.", blocks[2].GetText(true));
			Assert.IsTrue(blocks[2].StartsAtVerseStart);
			Assert.AreEqual(10, blocks[2].InitialStartVerseNumber);
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		[TestCase("(", ")")]
		[TestCase("\u300c", "\u300d")]
		[TestCase("\uff62", "\uff63")]
		[TestCase("\u3010", "\u3011")]
		[TestCase("\u3014", "\u3015")]
		public void Parse_MissingVerseAtStartOfParaEnclosedInBrackets_VerseAndPunctuationOmitted(string open, string close)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Mateo</para>" + Environment.NewLine +
				"<chapter number=\"18\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"p\"><verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. </para>" + Environment.NewLine +
				"<para style=\"s\">The coolest section head ever</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				$"{open} <verse number=\"11\" style=\"v\"/>{close} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char><char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note> " +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para> " +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual("The coolest section head ever", blocks[3].GetText(true));
			Assert.AreEqual(10, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual("{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.", blocks[4].GetText(true));
			Assert.IsTrue(blocks[4].StartsAtVerseStart);
			Assert.AreEqual(12, blocks[4].InitialStartVerseNumber);
		}

		// Technically, we barely care about the references on section heads (they actually get the reference from the preceding verse),
		// but it really doesn't make sense for them to have the reference of a verse that doesn't even exist.
		[TestCase("[", "]")]
		[TestCase("{", "}")]
		[TestCase("(", ")")]
		[TestCase("\u300c", "\u300d")]
		[TestCase("\uff62", "\uff63")]
		[TestCase("\u3010", "\u3011")]
		[TestCase("\u3014", "\u3015")]
		public void Parse_MissingVerseAtEndOfParaEnclosedInBrackets_VerseAndPunctuationOmitted_FollowingSectionHeadUsesPrevReference(string open, string close)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Mateo</para>" + Environment.NewLine +
				"<chapter number=\"18\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"p\"><verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. " +
				$"{open} <verse number=\"11\" style=\"v\"/>{close} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char><char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note></para>" +
				"<para style=\"s\">The coolest section head ever</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para>" + Environment.NewLine +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual("{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. ", blocks[2].GetText(true));
			Assert.AreEqual("The coolest section head ever", blocks[3].GetText(true));
			Assert.AreEqual(10, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual("{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.", blocks[4].GetText(true));
			Assert.IsTrue(blocks[4].StartsAtVerseStart);
			Assert.AreEqual(12, blocks[4].InitialStartVerseNumber);
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

		// PG-1084
		[Test]
		public void Parse_ParagraphWithCharacterStyleAndAttributes_AttributesNotIncluded()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
														"If you don't always remember things, you will " +
														"<char style=\"b\">" +
														"sometimes|strong=\"H01234,G05485\"</char>" +
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
			VerifyChapterBlock(blocks[0], 1);
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
			VerifyChapterBlock(blocks[0], 1, text:"Global-Chapter 1");
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
			VerifyChapterBlock(blocks[0], 1, text:"Specific-Chapter One", tag:"cl");
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(0, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("Lok ma Jon Labatija otito", blocks[1].GetText(false));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.True(blocks[1].IsParagraphStart);
		}

		/// <summary>
		///  PG-1140: Since the Paratext Markers check unfortunately allows the \cl marker to occur later in the chapter,
		/// </summary>
		[Test]
		public void Parse_SpecificChapterLabelLaterInChapter()
		{
			var usxHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?><usx version=\"2.5\"><book code=\"EPH\" style=\"id\">Test for PG-1140</book>";
			var doc = UsxDocumentTests.CreateDocFromString(
				usxHeader + Environment.NewLine +
				"<para style=\"h\">EPESU</para>" + Environment.NewLine +
				"<para style=\"mt\">RI EPESU</para>" + Environment.NewLine +
				"<chapter number=\"1\" style=\"c\"/>" + Environment.NewLine +
				"<para style=\"s\">Petabea i mPaulu</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				"<verse number=\"1\" style=\"v\"/>Tau i mPue Ala ri Epesu, anu mepo'inaya ri Kerisitu Yesu!</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				"<verse number=\"2\" style=\"v\"/>Yaku, i Paulu, suro i ngKerisitu Yesu ua pepokono i mPue Ala, mampomata - mata tuarapa i Pue Ala Papata pai i Yesu Kerisitu da madonco pai damawai palindo ndaya ri komi.</para>" + Environment.NewLine +
				"<chapter number=\"6\" style=\"c\"/>" + Environment.NewLine +
				"<para style=\"s\">Ana pai ine papanya</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				"<verse number=\"1\" style=\"v\"/>Ee wa'a ana! Tandanya tau anu meaya ri Kerisitu, komi da metubunaka ri tau tu'ami, ince'e anu sintinaja da ndiwianaka. " + Environment.NewLine +
				"<verse number=\"2\" style=\"v\"/>&lt;&lt;Tubunaka inemu pai papamu&gt;&gt; ince'emo songka anu ka'isa ungka ri Pue Ala pai pojanji, " + Environment.NewLine +
				"<verse number=\"3\" style=\"v\"/>ewase'i, &lt; &lt; Da naka dago ngkatuwumi pai marate inosami ri lino se'i&gt;&gt;.</para>" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				"<verse number=\"23\" syle=\"v\"/>Mbolimo i Pue Ala Papa, pai i Yesu Kerisitu da mawai jaya ri pura - pura anggota dompu kasamba'a-mba'a pai pombepotowe pai todo ri peaya ri Kerisitu. " +
				"<verse number=\"24\" style=\"v\"/>Mbolimo i Pue Ala da madonco komi pura - pura anu mampotowe Pueta i Yesu Kerisitu pai towe ndaya anu bare'e da re'e kabalinya.</para>" + Environment.NewLine +
				// Note: As decided in the discussion for PG-1140, the dtaa in this is errant \cl field will just be ignored. To be interpreted as valid, Chapter label data must either
				// precede the first chapter in the book or immediately follow the \c field to which it pertains.
				"<para style=\"cl\">Petubunaka ungka ri kami,</para>" + Environment.NewLine +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc, "EPH");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(9, blocks.Count);
			VerifyChapterBlock(blocks[1], 1, "EPH");
			VerifyChapterBlock(blocks[5], 6, "EPH");
			var lastBlock = blocks.Last();
			Assert.AreEqual("{23}\u00A0Mbolimo i Pue Ala Papa, pai i Yesu Kerisitu da mawai jaya ri pura - pura anggota dompu kasamba'a-mba'a pai pombepotowe pai todo ri peaya ri Kerisitu. " +
				"{24}\u00A0Mbolimo i Pue Ala da madonco komi pura - pura anu mampotowe Pueta i Yesu Kerisitu pai towe ndaya anu bare'e da re'e kabalinya.", lastBlock.GetText(true));
			Assert.IsFalse(blocks.Any(b => b.GetText(false).Contains("Petubunaka")));
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
			VerifyChapterBlock(blocks[0], 1);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, {2}\u00A0kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));

			Assert.AreEqual("c", blocks[2].StyleTag);
			VerifyChapterBlock(blocks[2], 2);
			Assert.AreEqual("p", blocks[3].StyleTag);
			Assert.AreEqual(2, blocks[3].ChapterNumber);
			Assert.AreEqual(1, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", blocks[3].GetText(true));
			Assert.AreEqual("q1", blocks[4].StyleTag);
			Assert.AreEqual(2, blocks[4].ChapterNumber);
			Assert.AreEqual(1, blocks[4].InitialStartVerseNumber);
			Assert.AreEqual("This is poetry, dude.", blocks[4].GetText(true));
		}

		[Test]
		public void Parse_PoetryParagraphsWithQuotedText_QtMarkersgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
				"<verse number=\"23\" style=\"v\" />" +
				"<char style=\"qt\">N'qe, sinboriwo</char></para>" +
				"<para style =\"q1\">" +
				"<char style=\"qt\">wi baga laala taa</char></para>" +
				"<para style =\"q2\">" +
				"<char style=\"qt\">be nagabile sii</char>,</para>" +
				"<para style =\"q1\">" +
				"<char style=\"qt\">pe beri wi yeri</char></para>" +
				"<para style =\"q\">" +
				"<char style=\"qt\">Emanuweli</char>. Kire wi jo “Kulocelie ye ne we ni.” " +
				"<verse number=\"24\" style=\"v\" />" +
				"Ba Yusufu wi keni ye ngonimo ne be.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(7, blocks.Count, "Should have a chapter block, 4 \"scripture\" blocks, and one regular \\q block.");
			Assert.IsTrue(blocks.Skip(1).Take(4).All(b => b.CharacterId == "scripture"));
			Assert.IsTrue(blocks[5].GetText(true).StartsWith("Emanuweli."), "Period should get pulled into the \"scripture\" block with \"Emanuweli.\" " +
				"We probably don't really care if the trailing space is retained or not.");
			Assert.AreEqual("Kire wi jo “Kulocelie ye ne we ni.” {24}\u00A0Ba Yusufu wi keni ye ngonimo ne be.", blocks.Last().GetText(true));
		}

		[TestCase("wj")]
		[TestCase("qt")]
		public void Parse_MappedMarkerInsideQuotationMarks_AdjacentPunctuationIncludedInBlockWithQuotedText(string sfMarker)
		{
			Assert.IsTrue(StyleToCharacterMappings.TryGetCharacterForCharStyle(sfMarker, out var character),
				$"Setup condition not met: marker \"{sfMarker}\" in TestCase should be included in {nameof(StyleToCharacterMappings)}.");
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"18\" style=\"v\" />" +
				"Kulocɛliɛ céki 'juu mɛ́ nɛ́ jo: «" +
				$"<char style=\"{sfMarker}\">Siaga nī muɔ bésimɛ ta bè</char>" +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">11.18 </char><char style=\"ft\" closed=\"false\">" +
				"<char style=\"xt\" closed=\"true\">Sél 21.12</char>" +
				"tire ti 'juu náʔa gè.</char></note>" +
				".»" +
				"<verse number=\"19\" style=\"v\" />" +
				"Nɛ̀ kiyaʔa Birayoma yéki sɔ̀ngi nɛ̀ tɛ́ngɛ ki nɛ̄ dí Kucɛliɛ bèle. Kire nɛ̄ wire kiyɛ́nì kpíʔile." +
				"</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count, "Should have a chapter block, plus 3 Scripture blocks.");
			Assert.AreEqual(character, blocks[2].CharacterId);
			Assert.IsTrue(blocks[1].GetText(true).TrimEnd().EndsWith("jo:"));
			Assert.AreEqual("«Siaga nī muɔ bésimɛ ta bè.»", blocks[2].GetText(true).Trim());
			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(19, blocks[3].InitialStartVerseNumber);
		}

		[TestCase(ExpectedResult = CharacterVerseData.kUnexpectedCharacter)]
		[TestCase("qt_123", ExpectedResult = CharacterVerseData.kUnexpectedCharacter)]
		[TestCase("qt_123", null, 1, ExpectedResult = CharacterVerseData.kUnexpectedCharacter)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase("qt_123", "Enoch", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesWithOnlyTextBetweenThem_AdjacentPunctuationIncludedInBlockWithQuotedText(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				" séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", qtId, character, level) +
				"He aquí, vino el Señor con sus santas decenas de millares." +
				GetQtMilestoneElement("end", qtId, character, level) +
				"<verse number=\"15\" style=\"v\" />" +
				"The quote should continue in this verse but it does not." +
				"</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count, "Should have a chapter block, plus 3 Scripture blocks.");
			Assert.AreEqual(14, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].GetText(true).TrimEnd().EndsWith("diciendo:"));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual(14, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("He aquí, vino el Señor con sus santas decenas de millares.", blocks[2].GetText(true).Trim());
			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[3].InitialStartVerseNumber);
			Assert.IsNull(blocks[3].CharacterId);
			return blocks[2].CharacterId;
		}

		private string GetQtMilestoneElement(string startOrEnd, string qtId = null, string character = null, int level = 0)
		{
			Debug.Assert(startOrEnd == "start" || startOrEnd == "end");
			var sb = new StringBuilder("<ms style=\"qt");
			if (level >= 1)
				sb.Append(level);
			sb.Append("-");
			sb.Append(startOrEnd[0]);
			sb.Append("\" status=\"");
			sb.Append(startOrEnd);
			sb.Append("\" ");
			if (qtId != null)
			{
				sb.Append(startOrEnd[0]);
				sb.Append("id=\"");
				sb.Append(qtId);
				sb.Append("\"");
				sb.Append(" ");
			}
			if (character != null)
			{
				sb.Append("who=\"");
				sb.Append(character);
				sb.Append("\"");
				sb.Append(" ");
			}
			sb.Append("/>");
			return sb.ToString();
		}

		[Test]
		public void Parse_DescriptiveTitleUsedOutsidePsalms_CharacterSetToNarrator()
		{
			Assert.IsTrue(StyleToCharacterMappings.TryGetCharacterForParaStyle("d", "MRK", out var character),
				$"Setup condition not met: marker \"d\" should be included in {nameof(StyleToCharacterMappings)}.");
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"d\">INTRODUCTION TO MARK</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count, "Should have a chapter block, plus the descriptive title block.");
			Assert.AreEqual(character, blocks[1].CharacterId);
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
			VerifyChapterBlock(blocks[2], 1);
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
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(1, blocks[1].ChapterNumber);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(Block.kNotSet, blocks[1].CharacterId);
			Assert.AreEqual("{1-2}\u00A0Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,", blocks[1].GetText(true));

			Assert.AreEqual(1, blocks[2].ChapterNumber);
			Assert.AreEqual(1, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual("q1", blocks[2].StyleTag);
			Assert.AreEqual(Block.kNotSet, blocks[2].CharacterId);
			Assert.AreEqual("“Nen, acwalo lakwenana otelo nyimi,", blocks[2].GetText(true));
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

		[TestCase("-")]
		[TestCase("- ")]
		[TestCase(" -")]
		[TestCase(" - ")]
		public void Parse_OddlyFormedVerseBridgeInAdjacentVFields_CorrectlyInterpretsAsVerseBridge(string dash)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1\" style=\"v\" />" +
				dash +
				"<verse number=\"3\" style=\"v\" />" +
				"Verse 1-3 text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(3, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("{1-3}\u00A0Verse 1-3 text", blocks[1].GetText(true));
		}

		[TestCase("-")]
		[TestCase(" - ")]
		public void Parse_VerseConsistsOfDashButPreviousVerseIsABridge_DiscardedAsEmptyVerse(string dash)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1-3\" style=\"v\" />" + dash + // This one should get discarded
				"<verse number=\"4\" style=\"v\" />" +
				"Verse 4 text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count, "Should only have chapter number block and v. 4 block. " +
				"Empty verse bridge block should have been discarded.");
			Assert.IsTrue(blocks[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual(1, blocks[0].ChapterNumber);
			Assert.AreEqual(4, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("{4}\u00A0Verse 4 text", blocks[1].GetText(true));
		}

		[TestCase("-", 3)]
		[TestCase("-", 2)]
		public void Parse_VerseConsistsOfDashButFollowingVerseLessThanOrEqualToPrevNumber_DiscardedAsEmptyVerse(string dash, int followingVerseNum)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"3\" style=\"v\" />" + dash + // This one should get discarded
				$"<verse number=\"{followingVerseNum}\" style=\"v\" />" +
				"Verse text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(followingVerseNum, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("{" + followingVerseNum + "}\u00A0Verse text", blocks[1].GetText(true));
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

		private List<Block> ParseLuke17Data(string paragraphData)
		{
			string usxFrame = "<?xml version=\"1.0\" encoding=\"utf-8\"?><usx version=\"2.0\"><book code=\"LUK\" style=\"id\">Some Bible</book><chapter number=\"17\" style=\"c\" />{0}</usx>";
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(usxFrame, paragraphData));
			var parser = GetUsxParser(doc, "LUK");
			return parser.Parse().ToList();
		}

		[Test]
		public void Parse_VerseAtEndOfParagraphConsistsEntirelyOfNote_DoNotIncludeVerseNumber()
		{
			// World English Bible, LUK 17:36
			var data = "  <para style=\"p\">\r\n" +
				"    <verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.”</char> <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left.” ", blocks[1].GetText(true));
			Assert.AreEqual(2, blocks[2].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[2].GetText(true));
		}

		// This test is an attempt to future-proof Glyssen when the DBL spec changes to support USX 3 (without us getting a heads-up).
		[Test]
		public void Parse_VerseElementWithEid_EndVerseElementIgnored()
		{
			var data = "  <para style=\"p\">\r\n" +
				$"    <verse number=\"35\" style=\"v\" sid=\"LUK 17:35\" />There will be two grinding grain together. One will be taken and the other will be left. <verse eid=\"LUK 17:35\" /><verse number=\"36\" style=\"v\" sid=\"LUK 17:36\" />Two will be in the field: the one taken, and the other left, Jesus concluded.<verse eid=\"LUK 17:36\" /></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"37\" style=\"v\" sid=\"LUK 17:37\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(4, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left. {36}\u00A0Two will be in the field: the one taken, and the other left, Jesus concluded.", blocks[1].GetText(true));
			Assert.AreEqual(2, blocks[2].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[2].GetText(true));
			Assert.IsNull(blocks[2].CharacterId);
			Assert.AreEqual("p", blocks[2].StyleTag);
		}

		// Since most of the tests in this fixture rely on UsxDocumentTests.CreateDocFromString, this
		// test ensures that the XML doc we get from ParatextData isn't somehow significantly different.
		[TestCase("\r\n")]
		[TestCase(" ")]
		public void Parse_FromParatextUsfm_VersesInSingleParagraphStayAsOneBlock(string whitespace)
		{
			var usfmData = $@"\id MAT{whitespace}" +
				$@"\c 16{whitespace}" +
				$@"\p{whitespace}" +
				$@"\v 23 Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, Nyaram anggo, Shetan!{whitespace}" +
				$@"\v 24 Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga.\f + \fr 16:24 \ft Ɓalli gbal aɓa: \xt Mat 10:38; Luk 14:27\xt*.\f*{whitespace}" +
				$@"\v 25 Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì.\f + \fr 16:25 \ft Ɓalli gbal aɓa: \xt Mat 10:39; Luk 17:33; Yoh 12:25\xt*.\f*";

			// This uses the "real" stylesheet (now USFM v. 3)
			var doc = UsfmToUsx.ConvertToXmlDocument(SfmLoader.GetUsfmScrStylesheet(), usfmData);
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(2, blocks.Count);
			Assert.AreEqual(6, blocks[1].BlockElements.Count);
			Assert.AreEqual("{23}\u00A0Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, Nyaram anggo, Shetan! " +
				"{24}\u00A0Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga. " +
				"{25}\u00A0Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì.", blocks[1].GetText(true));
		}

		#region PG-1272 Tests
		[TestCase("\r\n")]
		[TestCase(" ")]
		public void Parse_WordsOfJesusFromUsfm_BreakIntoSeparateBlockAssignedToJesus(string whitespace)
		{
			var usfmData = $@"\id MAT{whitespace}" +
				$@"\c 16{whitespace}" +
				$@"\p{whitespace}" +
				$@"\v 23 Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, \wj Nyaram anggo, Shetan!\wj*{whitespace}" +
				$@"\v 24 \wj Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga.\wj*\f + \fr 16:24 \ft Ɓalli gbal aɓa: \xt Mat 10:38; Luk 14:27\xt*.\f*{whitespace}" +
				$@"\v 25 \wj Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì.\wj*\f + \fr 16:25 \ft Ɓalli gbal aɓa: \xt Mat 10:39; Luk 17:33; Yoh 12:25\xt*.\f*";

			// This uses the "real" stylesheet (now USFM v. 3)
			var doc = UsfmToUsx.ConvertToXmlDocument(SfmLoader.GetUsfmScrStylesheet(), usfmData);
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{23}\u00A0Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, ", blocks[1].GetText(true));
			Assert.AreEqual(5, blocks[2].BlockElements.Count);
			Assert.AreEqual("Nyaram anggo, Shetan! " +
				"{24}\u00A0Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga. " +
				"{25}\u00A0Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì.", blocks[2].GetText(true));
		}

		// Note: Although it might feel like the USX should logically have the trailing space after a run of "wj" characters
		// included inside the run, that appears not to be what Paratext generates. Paratext requires a space following the
		// closing \wj* marker, and when this gets turned into USX, that space is retained as data inside the para element,
		// but not inside the char. These three test cases ensure that whether the spaces are inside, outside, or both, we
		// do not break the blocks just because of a space that Jesus doesn't speak.
		[TestCase(" ", "")]
		[TestCase("", " ")]
		[TestCase(" ", " ")]
		public void Parse_WordsOfJesus_BreakIntoSeparateBlockAssignedToJesus(string trailingSpaceInsideChar, string trailingSpaceOutsideChar)
		{
			var data = "  <para style=\"p\">\r\n" +
				$"    <verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.{trailingSpaceInsideChar}</char>{trailingSpaceOutsideChar}<verse number=\"36\" style=\"v\" sid=\"LUK 17:36\" /><char style=\"wj\">Two will be in the field: the one taken, and the other left,{trailingSpaceInsideChar}</char>{trailingSpaceOutsideChar}Jesus concluded.</para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(4, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left. {36}\u00A0Two will be in the field: the one taken, and the other left, ", blocks[1].GetText(true));
			Assert.AreEqual("Jesus", blocks[1].CharacterId);
			Assert.AreEqual("wj", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[2].BlockElements.Count);
			Assert.AreEqual("Jesus concluded.", blocks[2].GetText(true));
			Assert.IsNull(blocks[2].CharacterId);
			Assert.AreEqual("p", blocks[2].StyleTag);
			Assert.AreEqual(2, blocks[3].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[3].GetText(true));
			Assert.IsNull(blocks[3].CharacterId);
			Assert.AreEqual("p", blocks[3].StyleTag);
		}

		// Note: this test was originally written with the expectation that the USXParser would be responsible for
		// determining whether the character (e.g, Jesus) is expected to speak in the verse, but for efficiency and
		// clarity, that is now the responsibility of the QuoteParser, but I'm keeping this (modified) test here
		// to make the new expectation explicit.
		[Test]
		public void Parse_WordsOfJesusInVerseWhereJesusIsNotExpected_BreakIntoSeparateBlockAssignedToJesus()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"7\" style=\"v\" />And he preached: <char style=\"wj\">Someone is coming who is > I, the thong of whose sandals I am unworthy to untie.</char></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"    <verse number=\"8\" style=\"v\" /><char style=\"wj\">I immerse you in H2O, but he will plunge you into life with God's 'Holy Spirit.</char></para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{7}\u00A0And he preached: ", blocks[1].GetText(true));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(1, blocks[2].BlockElements.Count);
			Assert.AreEqual("Someone is coming who is > I, the thong of whose sandals I am unworthy to untie.", blocks[2].GetText(true));
			Assert.AreEqual("Jesus", blocks[2].CharacterId);
			Assert.AreEqual("wj", blocks[2].StyleTag);
			Assert.AreEqual(2, blocks[3].BlockElements.Count);
			Assert.AreEqual("{8}\u00A0I immerse you in H2O, but he will plunge you into life with God's 'Holy Spirit.", blocks[3].GetText(true));
			Assert.AreEqual("Jesus", blocks[3].CharacterId);
			Assert.AreEqual("wj", blocks[3].StyleTag);
		}

		[Test]
		public void Parse_CharacterStyleScriptureQuotes_BreakIntoSeparateBlockAssignedToScripture()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"<para style=\"p\">\r\n" +
				"<verse number=\"1\" style=\"v\" />The start of the gospel of Jesus Christ, God's Son'. " +
				"<verse number=\"2\" style=\"v\" />In the words of Isaiah: " +
				"<char style=\"qt\">I send my messenger ahead of you to prepare the way, </char>" +
				"<verse number=\"3\" style=\"v\" /><char style=\"qt\">shouting in the wild:</char></para>\r\n" +
				"<para style=\"q1\">“Prepare the way for the Lord,</para>\r\n" +
				"<para style=\"q2\">make straight paths for him.”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual(4, blocks[1].BlockElements.Count);
			Assert.AreEqual("{1}\u00A0The start of the gospel of Jesus Christ, God's Son'. {2}\u00A0In the words of Isaiah: ", blocks[1].GetText(true));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual("p", blocks[1].StyleTag);
			Assert.AreEqual(3, blocks[2].BlockElements.Count);
			Assert.AreEqual("I send my messenger ahead of you to prepare the way, {3}\u00A0shouting in the wild:", blocks[2].GetText(true));
			Assert.AreEqual("scripture", blocks[2].CharacterId);
			Assert.AreEqual("qt", blocks[2].StyleTag);
			Assert.AreEqual(1, blocks[3].BlockElements.Count);
			Assert.AreEqual("“Prepare the way for the Lord,", blocks[3].GetText(true));
			Assert.IsNull(blocks[3].CharacterId);
			Assert.AreEqual("q1", blocks[3].StyleTag);
			Assert.AreEqual(1, blocks[4].BlockElements.Count);
			Assert.AreEqual("make straight paths for him.”", blocks[4].GetText(true));
			Assert.IsNull(blocks[4].CharacterId);
			Assert.AreEqual("q2", blocks[4].StyleTag);
		}
		#endregion // PG-1272 Tests

		[Test]
		public void Parse_OnlyVerseInParagraphConsistsEntirelyOfNote_DoNotIncludeParagraph()
		{
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
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(2, blocks[1].BlockElements.Count);
			Assert.AreEqual("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left.” ", blocks[1].GetText(true));
			Assert.AreEqual(2, blocks[2].BlockElements.Count);
			Assert.AreEqual("{37}\u00A0They, answering, asked him, “Where, Lord?”", blocks[2].GetText(true));
		}

		private UsxParser GetUsxParser(XmlDocument doc, string bookId = "MRK")
		{
			return new UsxParser(bookId, new TestStylesheet(), new UsxDocument(doc).GetChaptersAndParas());
		}

		private void VerifyChapterBlock(Block block, int number, string bookId = "MRK", string text = null, string tag = "c")
		{
			Assert.AreEqual(tag, block.StyleTag);
			Assert.IsTrue(block.IsChapterAnnouncement);
			Assert.AreEqual(bookId, block.BookCode);
			Assert.AreEqual(number, block.ChapterNumber);
			Assert.AreEqual(0, block.InitialStartVerseNumber);
			Assert.AreEqual(text ?? number.ToString(), block.GetText(true));
			Assert.AreEqual($"BC-{bookId}", block.CharacterId);
			Assert.True(block.IsParagraphStart);
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
