using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using NUnit.Framework;
using Paratext.Data;
using SIL.DblBundle;
using SIL.DblBundle.Tests.Usx;
using SIL.DblBundle.Usx;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
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

		/// <summary>
		/// PG-1434
		/// </summary>
		[Test]
		public void Parse_NoScriptureTextFollowingFinalChapterMarker_FinalChapterOmitted()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"mt1\">Markus</para>" +
				"<chapter number=\"15\" style=\"c\" />" +
				"<para style=\"p\"><verse number=\"1\" />This is Scripture text.</para>" +
				"<chapter number=\"16\" style=\"c\" />" +
				"<para style=\"s\">A chapter to end all chapters</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			var lastBlock = blocks.Last();
			Assert.IsTrue(lastBlock.IsScripture);
			Assert.AreEqual("{1}\u00A0This is Scripture text.", lastBlock.GetText(true));
			Assert.AreEqual(1, lastBlock.InitialStartVerseNumber);
			Assert.AreEqual(15, lastBlock.ChapterNumber);
		}

		[TestCase(". ", "*")]
		[TestCase(".", "?")]
		[TestCase(". [", "...")]
		public void Parse_MissingVerseWithOnlyPunctuation_VerseAndPunctuationOmitted(string v10Ending, string punctInMissingVerse)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart.Replace("MRK", "MAT") +
				"<para style=\"mt1\">Mateo</para>" + Environment.NewLine +
				"<chapter number=\"18\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"p\">" + Environment.NewLine +
				$"<verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin{v10Ending}<verse number=\"11\" style=\"v\"/>{punctInMissingVerse} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char><char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note> " +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc, "MAT");
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

		#region PG-1419 - Quote milestones
		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = CharacterVerseData.kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		[TestCase(null, "Enoc", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesWithOnlyTextBetweenThem_TextBetweenMilestonesAddedAsQuoteBlock(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				"séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", character, qtId,  level) +
				"He aquí, vino el Señor con sus santas decenas de millares." +
				GetQtMilestoneElement("end", character, qtId,  level) +
				" <verse number=\"15\" style=\"v\" />" +
				"The quote should continue in this verse but it does not." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count, "Should have a chapter block, plus 3 Scripture blocks.");
			Assert.AreEqual(14, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].StartsAtVerseStart);
			Assert.IsTrue(blocks[1].GetText(true).EndsWith("diciendo: "));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual(14, blocks[2].InitialStartVerseNumber);
			Assert.IsFalse(blocks[2].StartsAtVerseStart);
			Assert.IsTrue(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			if (qtId == null)
				Assert.AreEqual("He aquí, vino el Señor con sus santas decenas de millares. ", blocks[2].GetText(true, true));
			else
			{
				Assert.AreEqual("He aquí, vino el Señor con sus santas decenas de millares. ", blocks[2].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				Assert.IsFalse(quoteIdAnnotation.IsNarrator);
			}
			if (blocks[2].CharacterId == CharacterVerseData.kNeedsReview)
				Assert.AreEqual(character, blocks[2].CharacterIdInScript);
			Assert.AreEqual(MultiBlockQuote.None, blocks[2].MultiBlockQuote);

			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[3].InitialStartVerseNumber);
			Assert.IsNull(blocks[3].CharacterId);
			Assert.IsTrue(blocks[3].IsPredeterminedFirstLevelQuoteEnd);
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.Last();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
				Assert.IsFalse(quoteIdAnnotation.IsNarrator);
			}
			Assert.AreEqual(MultiBlockQuote.None, blocks[3].MultiBlockQuote);

			return blocks[2].CharacterId;
		}

		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = CharacterVerseData.kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesInsideEnclosingQuotes_AdjacentPunctuationIncludedInBlockWithQuotedText(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				"séptimo desde Adán, diciendo: «" +
				GetQtMilestoneElement("start", character, qtId,  level) +
				"He aquí, vino el Señor con sus santas decenas de millares." +
				GetQtMilestoneElement("end", character, qtId,  level) +
				"» <verse number=\"15\" style=\"v\" />" +
				"The quote should continue in this verse but it does not." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count, "Should have a chapter block, plus 3 Scripture blocks.");
			Assert.AreEqual(14, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].StartsAtVerseStart);
			Assert.That(blocks[1].GetText(true), Does.EndWith("diciendo: "));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual(14, blocks[2].InitialStartVerseNumber);
			Assert.IsFalse(blocks[2].StartsAtVerseStart);
			Assert.IsTrue(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			if (qtId == null)
				Assert.AreEqual("«He aquí, vino el Señor con sus santas decenas de millares.» ", blocks[2].GetText(true, true));
			else
			{
				Assert.AreEqual("«He aquí, vino el Señor con sus santas decenas de millares.» ", blocks[2].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
			}
			if (blocks[2].CharacterId == CharacterVerseData.kNeedsReview)
				Assert.AreEqual(character, blocks[2].CharacterIdInScript);
			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[3].InitialStartVerseNumber);
			Assert.IsNull(blocks[3].CharacterId);
			Assert.IsTrue(blocks[3].IsPredeterminedFirstLevelQuoteEnd);
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.Last();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
			}

			return blocks[2].CharacterId;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_QtMilestoneStartAtStartOfVerse_QuoteIdAnnotationInsertedAfterVerseNumber(
			bool milestoneBeforeVerseNumber)
		{
			const string qtId = "yeah";
			const string character = "Jesus";
			var usx = "<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				"séptimo desde Adán, diciendo que el Señor venía con sus santas decenas de millares.";
			if (milestoneBeforeVerseNumber)
				usx += GetQtMilestoneElement("start", character, qtId,  1);
			usx += "<verse number=\"15\" style=\"v\" />";
			if (!milestoneBeforeVerseNumber)
				usx += GetQtMilestoneElement("start", character, qtId,  1);
			usx += "“Haré juicio contra todos para convencer a todos los impíos de entre ellos tocante a todas sus obras de impiedad.”" +
				GetQtMilestoneElement("end", character, qtId,  1) +
				"</para>";

			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				usx));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count, "Should have a chapter block, plus 2 Scripture blocks.");
			Assert.AreEqual(14, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].StartsAtVerseStart);
			Assert.AreEqual("{14}\u00A0De éstos también profetizó Enoc, séptimo desde Adán, " +
				"diciendo que el Señor venía con sus santas decenas de millares.",
				blocks[1].GetText(true));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual(15, blocks[2].InitialStartVerseNumber);
			Assert.IsTrue(blocks[2].StartsAtVerseStart);
			Assert.IsTrue(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{15}\u00A0“Haré juicio contra todos para convencer a todos los " +
				"impíos de entre ellos tocante a todas sus obras de impiedad.”",
				blocks[2].GetText(true));
			var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
			Assert.AreEqual(qtId, quoteIdAnnotation.Id);
			Assert.IsTrue(quoteIdAnnotation.Start);
			Assert.AreEqual(CharacterVerseData.kNeedsReview, blocks[2].CharacterId,
				$"Because {character} is not expected to speak in JUD 15.");
			Assert.AreEqual(character, blocks[2].CharacterIdInScript);
			quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.Last();
			Assert.AreEqual(qtId, quoteIdAnnotation.Id);
			Assert.IsFalse(quoteIdAnnotation.Start);
		}

		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = CharacterVerseData.kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesCoveringMultipleVersesAndParagraphs_MultiBlockQuoteBlocksCreated(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				"séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", character, qtId,  level) +
				"«He aquí, vino el Señor con sus santas decenas de millares;" +
				"</para>" +
				"<para style=\"q1\">" +
				"<verse number=\"15\" style=\"v\" />" +
				"para hacer juicio contra todos." +
				"</para>" +
				"<para style=\"q2\">" +
				"Dejará convictos a todos los impíos de sus obras impías y de todas las cosas duras que han hablado contra él" +
				GetQtMilestoneElement("end", character, qtId,  level) +
				",» dijo Enoc." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(6, blocks.Count, "Should have a chapter block, a leading narrator" +
				" block, 3 quote blocks and a trailing narrator block.");
			Assert.AreEqual(14, blocks[1].InitialStartVerseNumber);
			Assert.IsTrue(blocks[1].StartsAtVerseStart);
			Assert.IsTrue(blocks[1].GetText(true, true).EndsWith("diciendo: "));
			Assert.IsNull(blocks[1].CharacterId);
			Assert.AreEqual(14, blocks[2].InitialStartVerseNumber);
			Assert.IsFalse(blocks[2].StartsAtVerseStart);
			Assert.IsTrue(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("«He aquí, vino el Señor con sus santas decenas de millares;",
				blocks[2].GetText(true).Trim());
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
			}
			if (blocks[2].CharacterId == CharacterVerseData.kNeedsReview)
				Assert.AreEqual(character, blocks[2].CharacterIdInScript);
			Assert.AreEqual(MultiBlockQuote.Start, blocks[2].MultiBlockQuote);
			
			Assert.IsTrue(blocks[3].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(blocks[2].CharacterId, blocks[3].CharacterId);
			Assert.AreEqual(blocks[2].CharacterIdInScript, blocks[3].CharacterIdInScript);
			Assert.IsFalse(blocks[3].IsPredeterminedFirstLevelQuoteEnd);
			Assert.AreEqual("q1", blocks[3].StyleTag);
			Assert.AreEqual("{15}\u00A0para hacer juicio contra todos.", blocks[3].GetText(true, true));
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[3].MultiBlockQuote);

			Assert.IsFalse(blocks[4].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[4].InitialStartVerseNumber);
			Assert.AreEqual(blocks[2].CharacterId, blocks[4].CharacterId);
			Assert.AreEqual(blocks[2].CharacterIdInScript, blocks[4].CharacterIdInScript);
			Assert.IsFalse(blocks[4].IsPredeterminedFirstLevelQuoteEnd);
			Assert.AreEqual("q2", blocks[4].StyleTag);
			Assert.AreEqual("Dejará convictos a todos los impíos de sus obras impías y de todas las cosas duras que han hablado contra él,» ",
				blocks[4].GetText(true));
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[4].BlockElements.Last();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
			}
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[4].MultiBlockQuote);

			Assert.IsFalse(blocks[5].StartsAtVerseStart);
			Assert.AreEqual(15, blocks[5].InitialStartVerseNumber);
			Assert.IsNull(blocks[5].CharacterId);
			Assert.IsTrue(blocks[5].IsPredeterminedFirstLevelQuoteEnd);
			Assert.AreEqual("dijo Enoc.", blocks[5].GetText(true, true));
			Assert.AreEqual(MultiBlockQuote.None, blocks[5].MultiBlockQuote);

			return blocks[2].CharacterId;
		}

		[Test]
		public void Parse_QtMilestonesInVersesWithMultipleCharactersAndImpliedDeliveries_DeliveriesSetBasedOnCharacter()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"9\""),
				"<para style=\"p\">" +
				"<verse number=\"4\" style=\"v\" />" +
				"He fell to the ground and heard a voice say to him, " +
				GetQtMilestoneElement("start", "Jesus") +
				"“Saul, Saul, why do you persecute me?”" +
				GetQtMilestoneElement("end", "Jesus") +
				"</para>"+
				"<para style=\"p\">" +
				"<verse number=\"5\" style=\"v\" />" +
				GetQtMilestoneElement("start", "Paul") +
				"“Who are you, Lord?”" +
				GetQtMilestoneElement("end", "Paul") +
				"Saul asked." +
				"</para>"+
				"<para style=\"p\">" +
				GetQtMilestoneElement("start", "Jesus") +
				"“I am Jesus, whom you are persecuting,”" +
				GetQtMilestoneElement("end", "Jesus") +
				"he replied." +
				"<verse number=\"6\" style=\"v\" />" +
				GetQtMilestoneElement("start", "Jesus") +
				"“Now get up and go into the city, and you will be told what you must do.”" +
				GetQtMilestoneElement("end", "Jesus") +
				"</para>"));
			var parser = GetUsxParser(doc, "ACT");
			var blocks = parser.Parse().ToList();
			int i = 0;
			Assert.AreEqual(9, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(4, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{4}\u00A0He fell to the ground and heard a voice say to him, ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(4, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Saul, Saul, why do you persecute me?”",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual("questioning", blocks[i].Delivery);
			
			Assert.AreEqual(5, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{5}\u00A0“Who are you, Lord?”", blocks[i].GetText(true, true));
			Assert.AreEqual("Paul", blocks[i].CharacterId);
			Assert.AreEqual("awe", blocks[i].Delivery);

			Assert.AreEqual(5, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteEnd);
			Assert.AreEqual("Saul asked.", blocks[i].GetText(true, true));

			Assert.AreEqual(5, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“I am Jesus, whom you are persecuting,”",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.IsNull(blocks[i].Delivery);

			Assert.AreEqual(5, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteEnd);
			Assert.AreEqual("he replied.", blocks[i].GetText(true, true));
			
			Assert.AreEqual(6, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{6}\u00A0“Now get up and go into the city, and you will be told what " +
				"you must do.”", blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.IsNull(blocks[i].Delivery);

			Assert.AreEqual(++i, blocks.Count);
		}

		// Note: In production, we have tried to clean up this kind of C-V data so this can't
		// happen because it seldom makes sense.
		[Test]
		public void Parse_QtMilestonesForTwoVersesWithSameGroupCharactersButDifferentDefaults_DefaultFromFirstVerseUsed()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"4\""),
				"<para style=\"p\">" +
				"<verse number=\"19\" style=\"v\" />" +
				"But Peter and John replied, " +
				GetQtMilestoneElement("start", "Peter (Simon)/John") +
				"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! " +
				"<verse number=\"20\" style=\"v\" />" +
				"As for us, we cannot help speaking about what we have seen and heard.”" +
				GetQtMilestoneElement("end") +
				"</para>"));
			var parser = GetUsxParser(doc, "ACT");
			var blocks = parser.Parse().ToList();
			int i = 0;
			Assert.AreEqual(4, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{19}\u00A0But Peter and John replied, ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Which is right in God’s eyes: to listen to you, or to him? You be " +
				"the judges! {20}\u00A0As for us, we cannot help speaking about what we have seen " +
				"and heard.”",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Peter (Simon)/John", blocks[i].CharacterId);
			Assert.AreEqual("Peter (Simon)", blocks[i].CharacterIdInScript);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("Peter (Simon)", "John")]
		[TestCase("Peter", "John")]
		[TestCase("John", "Peter")]
		[TestCase("John", "Peter (Simon)")]
		public void Parse_QtMilestonesForExplicitMemberOfGroup_CharacterInUseSetAsSpecified(
			string characterForV19, string characterForV20)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"4\""),
					"<para style=\"p\">" +
					"<verse number=\"19\" style=\"v\" />" +
					"But Peter and John replied, " +
					GetQtMilestoneElement("start", characterForV19) +
					"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! " +
					GetQtMilestoneElement("end") +
					GetQtMilestoneElement("start", characterForV20) +
					"<verse number=\"20\" style=\"v\" />" +
					"As for us, we cannot help speaking about what we have seen and heard.”" +
					GetQtMilestoneElement("end") +
					"</para>"));
			var parser = GetUsxParser(doc, "ACT");
			var blocks = parser.Parse().ToList();
			
			if (characterForV19 == "Peter")
				characterForV19 = "Peter (Simon)";
			if (characterForV20 == "Peter")
				characterForV20 = "Peter (Simon)";

			int i = 0;
			Assert.AreEqual(4, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{19}\u00A0But Peter and John replied, ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Which is right in God’s eyes: to listen to you, or to him? You be " +
				"the judges! ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Peter (Simon)/John", blocks[i].CharacterId);
			Assert.AreEqual(characterForV19, blocks[i].CharacterIdInScript);

			Assert.AreEqual(20, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{20}\u00A0As for us, we cannot help speaking about what we have seen " +
				"and heard.”",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Peter (Simon)/John", blocks[i].CharacterId);
			Assert.AreEqual(characterForV20, blocks[i].CharacterIdInScript);

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_SeparateQtMilestonesForTwoVersesWithSameGroupCharactersButDifferentDefaults_DefaultFromEachVerseUsed()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"4\""),
					"<para style=\"p\">" +
					"<verse number=\"19\" style=\"v\" />" +
					"But Peter and John replied, " +
					GetQtMilestoneElement("start", "Peter (Simon)/John") +
					"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! " +
					GetQtMilestoneElement("end") +
					GetQtMilestoneElement("start", "Peter (Simon)/John") +
					"<verse number=\"20\" style=\"v\" />" +
					"As for us, we cannot help speaking about what we have seen and heard.”" +
					GetQtMilestoneElement("end") +
					"</para>"));
			var parser = GetUsxParser(doc, "ACT");
			var blocks = parser.Parse().ToList();
			
			int i = 0;
			Assert.AreEqual(4, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{19}\u00A0But Peter and John replied, ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(19, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Which is right in God’s eyes: to listen to you, or to him? You be " +
				"the judges! ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Peter (Simon)/John", blocks[i].CharacterId);
			Assert.AreEqual("Peter (Simon)", blocks[i].CharacterIdInScript);

			Assert.AreEqual(20, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{20}\u00A0As for us, we cannot help speaking about what we have seen " +
				"and heard.”",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Peter (Simon)/John", blocks[i].CharacterId);
			Assert.AreEqual("John", blocks[i].CharacterIdInScript);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("one", "quotation")]
		[TestCase("one", "quotation", false)]
		[TestCase("one")]
		[TestCase]
		[TestCase(null, null, false)]
		public void Parse_NestedQuotesWithExplicitMilestones_OnlyFirstLevelQuotesBrokenOut(
			string qtId1 = null, string qtId2 = null, bool includeCharacterInEndMilestones = true)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JER\""),
					"<para style=\"p\">" +
					"<verse number=\"7\" style=\"v\" />" +
					"But the Lord said to me, " +
					GetQtMilestoneElement("start", "God", qtId1, 1) +
					"“Do not say, " +
					GetQtMilestoneElement("start", "Jeremiah", qtId2, 2) +
					"‘I am too young.’" +
					GetQtMilestoneElement("end", includeCharacterInEndMilestones ? "Jeremiah" : null, qtId2, 2) +
					" You must go to everyone I send you to and say whatever I command you. " +
					"<verse number=\"8\" style=\"v\" />" +
					"Do not be afraid of them, for I am with you and will rescue you,”" +
					GetQtMilestoneElement("end", includeCharacterInEndMilestones ? "God" : null, qtId1, 1) +
					" declares the Lord." +
					"</para>"));
			var parser = GetUsxParser(doc, "JER");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(7, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{7}\u00A0But the Lord said to me, ", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(7, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Do not say, ‘I am too young.’ You must go to everyone I send you to and say whatever I command you. " +
				"{8}\u00A0Do not be afraid of them, for I am with you and will rescue you,”",
				blocks[i].GetText(true));
			if (qtId1 == null)
			{
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(3));
				Assert.False(blocks[i].BlockElements.Any(be => be is QuoteId));
			}
			else
			{
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(5));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.AreEqual(qtId1, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtId1, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
				Assert.That(blocks[i].CharacterId, Is.EqualTo("God"));
			}

			Assert.AreEqual(8, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("declares the Lord.", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("Interruption", "En Gedi info")]
		[TestCase("interruption-2CH")]
		[TestCase("narr-2CH")]
		[TestCase("narrator")]
		public void Parse_ExplicitlyMarkedInterruption_InterruptionSetAsNarrator(
			string interruptionCharacter, string qtId = null)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"2CH\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"20\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"After this, the Moabites and others came to war against Jehoshaphat. " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"Some people came and told Jehoshaphat, " +
					GetQtMilestoneElement("start", "men, some", level: 1) +
					"“A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar " +
					GetQtMilestoneElement("start", interruptionCharacter, qtId, 2) +
					"(that is, En Gedi) " +
					GetQtMilestoneElement("end", qtId: qtId, level: 2) +
					"is where they are currently camped.” " +
					GetQtMilestoneElement("end", level: 1) +
					"<verse number=\"3\" style=\"v\" />" +
					"Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: " +
					GetQtMilestoneElement("start", "King Jehoshaphat") +
					"All Judah must fast. " +
					GetQtMilestoneElement("end") +
					"</para>"));
			var parser = GetUsxParser(doc, "2CH");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(20, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{2}\u00A0Some people came and told Jehoshaphat, ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("men, some", blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			if (qtId == null)
				Assert.AreEqual("(that is, En Gedi) ", blocks[i].GetText(true, true));
			else
			{
				Assert.AreEqual("(that is, En Gedi) ", blocks[i].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
			}
			Assert.IsTrue(blocks[i].CharacterIs("2CH", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			Assert.AreEqual("is where they are currently camped.” ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("men, some", blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("All Judah must fast. ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jehoshaphat, king of Judah", blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("interruption", "En Gedi info")]
		[TestCase("Interruption-2CH", null)]
		[TestCase("interruption-2CH", "i1", "q1")]
		[TestCase("NARRATOR-2CH", null, "m234")]
		[TestCase("Narrator", null)]
		public void Parse_ExplicitlyMarkedInterruptionWithNoAlphaTextFollowing_InterruptionSetAsNarrator(
			string interruptionCharacter, string qtInterruptionId, string qtMenId = null)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"2CH\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"20\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"After this, the Moabites and others came to war against Jehoshaphat. " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"Some people came and told Jehoshaphat, " +
					GetQtMilestoneElement("start", "men, some", qtMenId, 1) +
					"“A vast army is coming against you from Edom, from the other side of the Dead Sea. They have already reached Hazezon Tamar " +
					GetQtMilestoneElement("start", interruptionCharacter, qtInterruptionId, 2) +
					"(that is, En Gedi)" +
					GetQtMilestoneElement("end", qtId: qtInterruptionId, level: 2) +
					"”. " +
					GetQtMilestoneElement("end", qtId: qtMenId, level: 1) +
					"<verse number=\"3\" style=\"v\" />" +
					"Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: " +
					GetQtMilestoneElement("start", "King Jehoshaphat") +
					"All Judah must fast. " +
					GetQtMilestoneElement("end") +
					"</para>"));
			var parser = GetUsxParser(doc, "2CH");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(20, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{2}\u00A0Some people came and told Jehoshaphat, ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			if (qtMenId == null)
			{
				Assert.AreEqual("“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar ",
					blocks[i].GetText(true, true));
			}
			else
			{
				Assert.AreEqual("“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar ",
					blocks[i].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.AreEqual(qtMenId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				Assert.AreEqual(2, blocks[i].BlockElements.Count);
			}
			Assert.AreEqual("men, some", blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("(that is, En Gedi)”. ", blocks[i].GetText(true, qtInterruptionId == null && qtMenId == null));
			var expectedBlockElementCount = 1;
			if (qtInterruptionId != null)
			{
				expectedBlockElementCount = 4;
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.AreEqual(qtInterruptionId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				Assert.That(((ScriptText)blocks[i].BlockElements[1]).Content, Is.EqualTo("(that is, En Gedi)"));
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[2];
				Assert.AreEqual(qtInterruptionId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
				Assert.That(((ScriptText)blocks[i].BlockElements[3]).Content, Is.EqualTo("”. "));
			}
			if (qtMenId != null)
			{
				expectedBlockElementCount++;
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtMenId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
			}
			Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(expectedBlockElementCount));
			Assert.IsTrue(blocks[i].CharacterIs("2CH", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("All Judah must fast. ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jehoshaphat, king of Judah", blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("Interruption", "En Gedi info")]
		[TestCase("interruption-2CH")]
		[TestCase("narrator-2CH")]
		[TestCase("narrator")]
		public void Parse_ExplicitlyMarkedInterruptionWithoutExplicitlyMarkedQuote_InterruptionSetAsNarrator(
			string interruptionCharacter, string qtId = null)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"2CH\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"20\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"After this, the Moabites and others came to war against Jehoshaphat. " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"Some people came and told Jehoshaphat, " +
					"“A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar " +
					GetQtMilestoneElement("start", interruptionCharacter, qtId) +
					"(that is, En Gedi) " +
					GetQtMilestoneElement("end", qtId: qtId) +
					"is where they are currently camped.” " +
					"<verse number=\"3\" style=\"v\" />" +
					"Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed a fast for all Judah. " +
					"</para>"));
			var parser = GetUsxParser(doc, "2CH");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(20, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{2}\u00A0Some people came and told Jehoshaphat, “A vast army is coming" +
				" against you from Edom, from the other side of the Dead Sea. Hazezon Tamar ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			if (qtId == null)
				Assert.AreEqual("(that is, En Gedi) ", blocks[i].GetText(true, true));
			else
			{
				Assert.AreEqual("(that is, En Gedi) ", blocks[i].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
			}
			Assert.IsTrue(blocks[i].CharacterIs("2CH", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			Assert.AreEqual("is where they are currently camped.” {3}\u00A0Alarmed, Jehoshaphat " +
				"resolved to inquire of the Lord, and he proclaimed a fast for all Judah. ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("narrator-2CH")]
		[TestCase("narrator-2CH", true)]
		[TestCase("narr", true, "no one", "no one")]
		[TestCase("narrator", true, "no one can curse Jesus", "no one can say Jesus is Lord")]
		[TestCase("narrator", false, "no one can curse Jesus", "no one can say Jesus is Lord")]
		public void Parse_ExplicitlyMarkedNarratorQuote_NarratorQuoteIdAnnotationsAdded(
			string narrator, bool explicitEndCharacter = false, string qtId1 = null, string qtId2 = null)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"1CO\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"12\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"Now about Spirit-given abilities, brothers, I do not want you to be confused. " +
					"<verse number=\"2\" style=\"v\" />" +
					"You know that as pagans, you were led astray to mute idols. " +
					"<verse number=\"3\" style=\"v\" />" +
					"So I want you to clearly understand that no one who is speaking by the Holy Spirit could ever say, " +
					GetQtMilestoneElement("start", narrator, qtId1) +
					"“Jesus be cursed,”" +
					GetQtMilestoneElement("end", explicitEndCharacter ? narrator : null, qtId1) +
					" and no one can confess, " +
					GetQtMilestoneElement("start", narrator, qtId2) +
					"“Jesus is Lord,”" +
					GetQtMilestoneElement("end", explicitEndCharacter ? narrator : null, qtId2) +
					" except under the leading of the Holy Spirit." +
					"</para>"));
			var parser = GetUsxParser(doc, "1CO");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(12, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("{1}\u00A0Now about Spirit-given abilities, brothers, I do not want you to be confused. " +
				"{2}\u00A0You know that as pagans, you were led astray to mute idols. " +
				"{3}\u00A0So I want you to clearly understand that no one who is speaking by the Holy Spirit could ever say, “Jesus be cursed,” and no one can confess, “Jesus is Lord,” except under the leading of the Holy Spirit." ,
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[6];
			Assert.AreEqual(qtId1, quoteIdAnnotation.Id);
			Assert.IsTrue(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[8];
			Assert.AreEqual(qtId1, quoteIdAnnotation.Id);
			Assert.IsFalse(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);

			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[10];
			Assert.AreEqual(qtId2, quoteIdAnnotation.Id);
			Assert.IsTrue(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[12];
			Assert.AreEqual(qtId2, quoteIdAnnotation.Id);
			Assert.IsFalse(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("interruption")]
		[TestCase("narrator-MAT")]
		public void Parse_ExplicitlyMarkedInterruptionInVerseWithPotentialNarratorQuote_QuoteAndInterruptionBrokenOut(
			string interruptionCharacter)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"1\""),
					"<para style=\"p\">" +
					"<verse number=\"22\" style=\"v\" />" +
					"All this took place to fulfill what the Lord had said through the prophet: " +
					"<verse number=\"23\" style=\"v\" />" +
					GetQtMilestoneElement("start", "scripture", level:1) +
					"“The virgin will conceive and give birth to a son, and Immanuel " +
					GetQtMilestoneElement("start", interruptionCharacter, level:2) +
					"(which means ‘God with us’)" +
					GetQtMilestoneElement("end", level:2) +
					" will be the name by which they will call him.”" +
					GetQtMilestoneElement("end", level:1) +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(22, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("{22}\u00A0All this took place to fulfill what the Lord had said through the prophet: " ,
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(23, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("{23}\u00A0“The virgin will conceive and give birth to a son, and Immanuel " ,
				blocks[i].GetText(true, true));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));
			
			Assert.AreEqual(23, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("(which means ‘God with us’) " , blocks[i].GetText(true, true));
			Assert.IsTrue(blocks[i].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual(23, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("will be the name by which they will call him.”" ,
				blocks[i].GetText(true, true));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));

			Assert.AreEqual(++i, blocks.Count);
		}

		/// <summary>
		/// This test (in contrast to the following one) helps to illustrate the distinction
		/// between the explicit use of an "interruption" character (which will always be
		/// treated as an interruption) as opposed to "narrator".
		/// </summary>
		[TestCase(1)]
		[TestCase(2)]
		public void Parse_ExplicitlyMarkedInterruptionButNotQuoteInVerseWithPotentialNarratorQuote_InterruptionBrokenOut(
			int level)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"1\""),
					"<para style=\"p\">" +
					"<verse number=\"22\" style=\"v\" />" +
					"All this took place to fulfill what the Lord had said through the prophet: " +
					"<verse number=\"23\" style=\"v\" />" +
					"“The virgin will conceive and give birth to a son, and Immanuel " +
					GetQtMilestoneElement("start", "interruption", level:level) +
					"(which means ‘God with us’)" +
					GetQtMilestoneElement("end", level:level) +
					" will be the name by which they will call him.”" +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(22, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("{22}\u00A0All this took place to fulfill what the Lord had said through the prophet: " +
				"{23}\u00A0“The virgin will conceive and give birth to a son, and Immanuel " ,
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);
			
			Assert.AreEqual(23, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("(which means ‘God with us’) " , blocks[i].GetText(true, true));
			Assert.IsTrue(blocks[i].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual(23, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("will be the name by which they will call him.”" ,
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		/// <summary>
		/// This test (in contrast to the previous one) helps to illustrate the distinction between
		/// the explicit use of an "narrator" character (which will only be treated as an
		/// interruption if it is inside a quote, when it occurs in a verse that is known to have
		/// normal narrator quotes) as opposed to "interruption".
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void Parse_ExplicitlyMarkedNarratorQuoteInVerseWithPotentialInterruption_NarratorQuoteIdAnnotationsAdded(
			bool includeCharacterInEndMilestone)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"1\""),
					"<para style=\"p\">" +
					"<verse number=\"22\" style=\"v\" />" +
					"All this happened to fulfill God’s prophetic word: " +
					"<verse number=\"23\" style=\"v\" />" +
					"“The virgin will give birth to a son called ‘Immanuel’” (meaning " +
					GetQtMilestoneElement("start", "narr-MAT") +
					"“God with us”" +
					GetQtMilestoneElement("end", includeCharacterInEndMilestone ? "narrator" : null) +
					")." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(22, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsFalse(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.AreEqual("{22}\u00A0All this happened to fulfill God’s prophetic word: " +
				"{23}\u00A0“The virgin will give birth to a son called ‘Immanuel’” (meaning “God with us”).",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[4];
			Assert.IsNull(quoteIdAnnotation.Id);
			Assert.IsTrue(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[6];
			Assert.IsNull(quoteIdAnnotation.Id);
			Assert.IsFalse(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("interruption", "En Gedi info")]
		[TestCase("Interruption-2CH", null)]
		[TestCase("NARRATOR-2CH", null, "m234")]
		[TestCase("Narrator", null)]
		public void Parse_ExplicitlyMarkedInterruptionWithMissingEnd_InterruptionSetAsNarrator(
			string interruptionCharacter, string qtInterruptionId, string qtMenId = null)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"2CH\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"20\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"After this, the Moabites and others came to war against Jehoshaphat. " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"Some people came and told Jehoshaphat, " +
					GetQtMilestoneElement("start", "men, some", qtMenId, 1) +
					"“A vast army is coming against you from Edom, from the other side of the Dead Sea. They have already reached Hazezon Tamar " +
					GetQtMilestoneElement("start", interruptionCharacter, qtInterruptionId, 2) + // Should have closed quotes above and not marked as interruption.
					"(that is, En Gedi)" + // Missing end-interruption here.
					"”. " +
					GetQtMilestoneElement("end", qtId: qtMenId, level: 1) +
					"<verse number=\"3\" style=\"v\" />" +
					"Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: " +
					GetQtMilestoneElement("start", "King Jehoshaphat") +
					"All Judah must fast. " +
					GetQtMilestoneElement("end") +
					"</para>"));
			var parser = GetUsxParser(doc, "2CH");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.AreEqual(20, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. ",
				blocks[i].GetText(true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{2}\u00A0Some people came and told Jehoshaphat, ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			if (qtMenId == null)
			{
				Assert.AreEqual("“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar ",
					blocks[i].GetText(true, true));
			}
			else
			{
				Assert.AreEqual("“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar ",
					blocks[i].GetText(true));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.AreEqual(qtMenId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				Assert.IsFalse(quoteIdAnnotation.IsNarrator);
				Assert.AreEqual(2, blocks[i].BlockElements.Count);
			}
			Assert.AreEqual("men, some", blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("(that is, En Gedi)”. ", blocks[i].GetText(true, qtInterruptionId == null && qtMenId == null));
			if (qtInterruptionId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.AreEqual(qtInterruptionId, quoteIdAnnotation.Id);
				Assert.IsTrue(quoteIdAnnotation.Start);
				Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			}
			if (qtMenId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtMenId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
				Assert.IsFalse(quoteIdAnnotation.IsNarrator);
			}
			Assert.IsTrue(blocks[i].CharacterIs("2CH", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: ",
				blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("All Judah must fast. ",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jehoshaphat, king of Judah", blocks[i].CharacterId);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase("reader")]
		[TestCase(null)]
		public void Parse_ExplicitlyMarkedInterruptionInMultiParaQuoteWithSpaceAfterInterruptionEnd_InterruptionSetAsNarrator(
			string qtEndInterruptionId)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"24\""),
					"<para style=\"p\">" +
					"<verse number=\"10\" style=\"v\" />" +
					GetQtMilestoneElement("start", "Jesus", level:1) +
					"“At that time many will lose faith, and they will betray and hate one another. " +
					"<verse number=\"11\" style=\"v\" />" +
					"Many people will falsely claim to speak for God in order to deceive others. " +
					"<verse number=\"12\" style=\"v\" />" +
					"As lawlessness increases more and more people will lose their will to love. " +
					"<verse number=\"13\" style=\"v\" />" +
					"But the overcomers will be saved. " +
					"<verse number=\"14\" style=\"v\" />" +
					"Before the end of time, this good news of the kingdom of God will be announced everywhere so all the nations will hear it." +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"15-16\" style=\"v\" />" +
					"“The Judeans must run to hide in the hills when in the temple the you see the abomination of desolation, " +
					GetQtMilestoneElement("start", "interruption", "reader", 2) +
					"with which a reader of Daniel should be familiar." +
					GetQtMilestoneElement("end", "interruption", qtEndInterruptionId, 2) +
					" <verse number=\"17\" style=\"v\" />" +
					"Whoever is on the roof must not go get things out of his house. " +
					"<verse number=\"18\" style=\"v\" />" +
					"Anyone in the field must not go get his robe. " +
					"<verse number=\"19\" style=\"v\" />" +
					"It will really stink for women who are pregnant or nursing in those days! " +
					"<verse number=\"20\" style=\"v\" />" +
					"Also, pray that when you flee the weather will be nice and it will not be on a day of rest.”" +
					GetQtMilestoneElement("end", "Jesus", level:1) +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			int i = 0;
			Assert.AreEqual(24, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);

			Assert.AreEqual(10, blocks[++i].InitialStartVerseNumber);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.AreEqual("{10}\u00A0“At that time many will lose faith, and they will betray and hate one another. " +
				"{11}\u00A0Many people will falsely claim to speak for God in order to deceive others. " +
				"{12}\u00A0As lawlessness increases more and more people will lose their will to love. " +
				"{13}\u00A0But the overcomers will be saved. " +
				"{14}\u00A0Before the end of time, this good news of the kingdom of God will be announced everywhere so all the nations will hear it.",
				blocks[i].GetText(true, true));

			Assert.AreEqual(15, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(16, blocks[i].InitialEndVerseNumber);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsContinuationOfPreviousBlockQuote);
			Assert.AreEqual("{15-16}\u00A0“The Judeans must run to hide in the hills when in the temple the you see the abomination of desolation, ",
				blocks[i].GetText(true, true));

			Assert.AreEqual(15, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(16, blocks[i].InitialEndVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.IsTrue(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.AreEqual("with which a reader of Daniel should be familiar. ",
				blocks[i].GetText(true));
			Assert.IsTrue(blocks[i].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
			Assert.AreEqual("reader", quoteIdAnnotation.Id);
			Assert.IsTrue(quoteIdAnnotation.Start);
			Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			if (qtEndInterruptionId == null)
				Assert.AreEqual(2, blocks[i].BlockElements.Count);
			else
			{
				// REVIEW: do we want the space added before or after the annotation?
				Assert.AreEqual(3, blocks[i].BlockElements.Count);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.AreEqual(qtEndInterruptionId, quoteIdAnnotation.Id);
				Assert.IsFalse(quoteIdAnnotation.Start);
				Assert.IsTrue(quoteIdAnnotation.IsNarrator);
			}

			Assert.AreEqual(17, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsParagraphStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{17}\u00A0Whoever is on the roof must not go get things out of his house. " +
				"{18}\u00A0Anyone in the field must not go get his robe. " +
				"{19}\u00A0It will really stink for women who are pregnant or nursing in those days! " +
				"{20}\u00A0Also, pray that when you flee the weather will be nice and it will not be on a day of rest.”"));

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_QtMilestonesOpenAndCloseInDifferentChapters_MultiBlockQuoteStoppedAndRestartedAndCharactersSetCorrectly()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"5\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"When Jesus perceived the crowds, he ascended a mountain and sat down. His disciples came to him, " +
					"<verse number=\"2\" style=\"v\" />" +
					"and he commenced teaching them, saying: " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"3\" style=\"v\" />" +
					GetQtMilestoneElement("start", "Jesus", "SOTM") +
					"Blessed are the poor in spirit," +
					"</para>" +
					"<para style=\"q2\">" +
					"for they own the kingdom of heaven." +
					"</para>" +
					"<para style=\"s\">Love Your Enemies</para>" +
					"<para style=\"r\">Luke 6:32</para>" +
					"<para style=\"p\">" +
					"<verse number=\"48\" style=\"v\" />" +
					"Be perfect, then, as your Father is perfect." +
					"</para>" +
					"<chapter number=\"6\" style=\"c\" />" +
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"Practice your righteousness in front of others and you forfeit your heavenly reward." +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"So when you give to the poor, do not make a big deal of it. " +
					"<verse number=\"3\" style=\"v\" />" +
					"Do not even let your one hand know what the other is doing." +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"34\" style=\"v\" />" +
					"So do not worry about tomorrow; it will worry about itself. Each day is enough of a problem." +
					"</para>" +
					"<chapter number=\"7\" style=\"c\" />" +
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"Do not judge unless you want to be judged. " +
					"<verse number=\"27\" style=\"v\" />" +
					"The rain came down, the streams rose, and the winds blew and beat against that house, and it fell with a great crash." +
					GetQtMilestoneElement("end") +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"28-29\" style=\"v\" />" +
					"When Jesus had finished, having taught as one who had authority, the crowds were amazed at how his teaching was not like that of their own teachers." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical);

			Assert.AreEqual(5, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{3}\u00A0Blessed are the poor in spirit,", blocks[i].GetText(true));
			// Not sure we actually care whether the QuoteId annotation comes
			// before or after the verse number.
			Assert.AreEqual("SOTM", blocks[i].BlockElements.Take(2).OfType<QuoteId>().Single().Id);
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, blocks[i].MultiBlockQuote);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("q2", blocks[i].StyleTag);
			Assert.AreEqual("for they own the kingdom of heaven.", blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual("s", blocks[++i].StyleTag);
			Assert.AreEqual(sectionHeadCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			// Parallel passage references do not get included in script.

			Assert.AreEqual(48, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(6, blocks[++i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{1}\u00A0Practice your righteousness in front of others and you " +
				"forfeit your heavenly reward.", blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{2}\u00A0So when you give to the poor, do not make a big deal of it. " +
				"{3}\u00A0Do not even let your one hand know what the other is doing.",
				blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(34, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{34}\u00A0So do not worry about tomorrow; it will worry about itself. " +
				"Each day is enough of a problem.", blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(7, blocks[++i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{1}\u00A0Do not judge unless you want to be judged. " +
				"{27}\u00A0The rain came down, the streams rose, and the winds blew and beat " +
				"against that house, and it fell with a great crash.",
				blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(28, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(29, blocks[i].InitialEndVerseNumber);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_QtMilestonesLeftOpenFollowedByWordsOfJesusStyle_WjClosesOpenMilestoneQuote()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"8\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"When Jesus came back from the hill, big crowds chased Him. " +
					"<verse number=\"2\" style=\"v\" />" +
					"And a guy with leprosy came and bowed before Him, saying, " +
					GetQtMilestoneElement("start", "leper") +
					"“Sir, if You would, you can cleanse me.” " +
					// Quote should have been closed here, but there was a mistake in the data.
					"<verse number=\"3\" style=\"v\" />" +
					"Jesus reached out with His hand and touched him, saying, " +
					"<char style=\"wj\">“Absolutely.” </char>" +
					"And immediately his leprosy was cleansed." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);

			Assert.AreEqual(8, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[i].LastVerseNum);
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“Sir, if You would, you can cleanse me.” " +
				"{3}\u00A0Jesus reached out with His hand and touched him, saying, ",
				blocks[i].GetText(true, true));
			Assert.AreEqual(CharacterVerseData.kNeedsReview, blocks[i].CharacterId);
			Assert.AreEqual("leper", blocks[i].CharacterIdInScript);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("wj", blocks[i].StyleTag);
			Assert.AreEqual("“Absolutely.” ", blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("And immediately his leprosy was cleansed.", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_QtMilestonesForOtherCharacterFollowedByBreakThenWjStyle_WjClosesOpenMilestoneQuote(
			bool includeFigureInSectionHead)
		{
			var usxText = string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
					.Replace("<chapter number=\"1\"", "<chapter number=\"8\""),
				string.Format("<para style=\"p\">" +
				"<verse number=\"17\" style=\"v\" />" +
				"This happened so that what was spoken through Isaiah the prophet would be fulfilled: " +
				GetQtMilestoneElement("start", "scripture") +
				"“He Himself took our illnesses and carried away our diseases.”" +
				"</para>" +
				"<para style=\"s\">Discipleship Tested{0}</para>" +
				"<para style=\"p\">" +
				"<verse number=\"18\" style=\"v\" />" +
				"<char style=\"wj\">“Let us go over to the other side of the sea,” </char>" +
				"said Jesus, wishing to ditch the crowd." +
				"</para>", includeFigureInSectionHead ?
					"<figure style=\"fig\" desc=\"Lake with fishing boats\" file=\"galilee.png\" " +
					"size=\"col\" loc=\"\" copy=\"\" ref=\"8.18\">Sea of Galilee</figure>" : ""));

			var doc = UsxDocumentTests.CreateDocFromString(usxText);

			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical);

			Assert.AreEqual(8, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(17, blocks[++i].InitialStartVerseNumber);
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(17, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“He Himself took our illnesses and carried away our diseases.”", blocks[i].GetText(true, true));
			Assert.AreEqual("scripture", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual("s", blocks[++i].StyleTag);
			Assert.AreEqual(sectionHeadCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(18, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{18}\u00A0“Let us go over to the other side of the sea,” ", blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(18, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("said Jesus, wishing to ditch the crowd.", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_QtMilestonesForOtherCharacterFollowedBySectionHeadWithKeyword_EntireSectionHeadTreatedAsSingleExtraBlock()
		{			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"8\""),
					"<para style=\"p\">" +
					"<verse number=\"17\" style=\"v\" />" +
					"This happened so that what was spoken through Isaiah the prophet would be fulfilled: " +
					GetQtMilestoneElement("start", "scripture") +
					"“He Himself took our illnesses and carried away our diseases.”" +
					"</para>" +
					"<para style=\"s\">Discipleship <char style=\"k\">Tested</char>"+
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"18\" style=\"v\" />" +
					"<char style=\"wj\">“Let us go over to the other side of the sea,” </char>" +
					"said Jesus, wishing to ditch the crowd." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical);

			Assert.AreEqual(8, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(17, blocks[++i].InitialStartVerseNumber);
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(17, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("“He Himself took our illnesses and carried away our diseases.”", blocks[i].GetText(true, true));
			Assert.AreEqual("scripture", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual("s", blocks[++i].StyleTag);
			Assert.AreEqual(sectionHeadCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			Assert.AreEqual("Discipleship Tested", blocks[i].GetText(true));

			Assert.AreEqual(18, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{18}\u00A0“Let us go over to the other side of the sea,” ", blocks[i].GetText(true, true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(18, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("said Jesus, wishing to ditch the crowd.", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_QtMilestonesIntermingledWithWjStyle_WjDoesNotCloseOpenMilestoneQuote(bool includeSectionHead)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"MAT\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"5\""),
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"When Jesus perceived the crowds, he ascended a slope and sat. His followers came to him, " +
					"<verse number=\"2\" style=\"v\" />" +
					"and he commenced teaching them, saying: " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"3\" style=\"v\" />" +
					GetQtMilestoneElement("start", "Jesus") +
					"<char style=\"wj\">Blessed are the poor in spirit.</char>" +
					"</para>" +
					(includeSectionHead ? "<para style=\"s\">Love Your Enemies</para>" : "") +
					"<para style=\"p\">" +
					"<verse number=\"48\" style=\"v\" />" +
					// If user really wanted the wj's, then "is perfect" should have been included:
					"<char style=\"wj\">Be perfect, then, as your Father</char> is perfect." +
					"</para>" +
					"<chapter number=\"6\" style=\"c\" />" +
					"<para style=\"p\">" +
					"<verse number=\"1\" style=\"v\" />" +
					// If user really wanted the wj's, this next snippet should have been included:
					"Practice your righteousness " +
					"<char style=\"wj\">in front of others and you forfeit your heavenly reward.</char>" +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					// No wj around this, but the milestone quote should still be in effect since
					// it's for the same character.
					"So when you give to the poor, do not make a big deal of it." +
					GetQtMilestoneElement("end", "Jesus") +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical);

			Assert.AreEqual(5, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(3, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{3}\u00A0Blessed are the poor in spirit.", blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);

			if (includeSectionHead)
			{
				Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

				Assert.AreEqual("s", blocks[++i].StyleTag);
				Assert.AreEqual(sectionHeadCharacter, blocks[i].CharacterId);

				Assert.AreEqual(MultiBlockQuote.None, blocks[++i].MultiBlockQuote);
			}
			else
			{
				Assert.AreEqual(MultiBlockQuote.Start, blocks[i].MultiBlockQuote);

				Assert.AreEqual(MultiBlockQuote.Continuation, blocks[++i].MultiBlockQuote);
			}

			Assert.AreEqual(48, blocks[i].InitialStartVerseNumber);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{48}\u00A0Be perfect, then, as your Father is perfect.", blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);

			Assert.AreEqual(6, blocks[++i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{1}\u00A0Practice your righteousness in front of others and you " +
				"forfeit your heavenly reward.", blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, blocks[i].MultiBlockQuote);
			
			Assert.AreEqual(2, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("p", blocks[i].StyleTag);
			Assert.AreEqual("{2}\u00A0So when you give to the poor, do not make a big deal of it.",
				blocks[i].GetText(true));
			Assert.AreEqual("Jesus", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_QtMilestonesInIntroMaterial_Ignored()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame
						.Replace("<chapter number=\"1\" style=\"c\" />", ""),
					"<para style=\"ip\">" +
					"As you study the book of Mark, it is important to remember Jesus' words: " +
					GetQtMilestoneElement("start", "Jesus") +
					"“what shall it profit a man, if he shall gain the whole world, and lose his own soul?” " +
					GetQtMilestoneElement("end", "Jesus") +
					"So be sure not just to read it, but to put your faith in Christ." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Intro),
				blocks.Single().CharacterId);
		}

		[Test]
		public void Parse_DuplicateStartQtMilestone_Ignored()
		{
			var usx = "<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", "Enoch") +
				"El Señor viene con sus santas decenas de millares. " +
				"<verse number=\"15\" style=\"v\" />" +
				GetQtMilestoneElement("start", "Enoch") +
				"“Hará juicio contra todos para convencer a todos los impíos " +
				GetQtMilestoneElement("start", "Enoch") +
				"de entre ellos tocante a todas sus obras de impiedad.”" +
				GetQtMilestoneElement("end") +
				"</para>";

			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				usx));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();

			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter);

			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(14, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.AreEqual("{14}\u00A0De éstos también profetizó Enoc, séptimo desde Adán, " +
				"diciendo: ", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(14, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("El Señor viene con sus santas decenas de millares. " +
				"{15}\u00A0“Hará juicio contra todos para convencer a todos los impíos " +
				"de entre ellos tocante a todas sus obras de impiedad.”",
				blocks[i].GetText(true));
			Assert.AreEqual("Enoch", blocks[i].CharacterId);
		}

		[Test]
		public void Parse_ExtraneousEndQtMilestone_Ignored()
		{
			var usx = "<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", "Enoch") +
				"El Señor viene con sus santas decenas de millares. " +
				"<verse number=\"15\" style=\"v\" />" +
				"“Hará juicio contra todos para convencer a todos los impíos " +
				"de entre ellos tocante a todas sus obras de impiedad.” " +
				GetQtMilestoneElement("end") +
				"<verse number=\"16\" style=\"v\" />" +
				"These are grumble-bunnies, following their own sinful desires." +
				// Oops. This doesn't belong here:
				GetQtMilestoneElement("end") +
				"</para>";

			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
					usx));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();

			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter);

			Assert.AreEqual(1, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(14, blocks[++i].InitialStartVerseNumber);
			Assert.IsTrue(blocks[i].StartsAtVerseStart);
			Assert.AreEqual("{14}\u00A0De éstos también profetizó Enoc, séptimo desde Adán, " +
				"diciendo: ", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);

			Assert.AreEqual(14, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].StartsAtVerseStart);
			Assert.IsTrue(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("El Señor viene con sus santas decenas de millares. " +
				"{15}\u00A0“Hará juicio contra todos para convencer a todos los impíos " +
				"de entre ellos tocante a todas sus obras de impiedad.” ",
				blocks[i].GetText(true));
			Assert.AreEqual("Enoch", blocks[i].CharacterId);

			Assert.AreEqual(16, blocks[++i].InitialStartVerseNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("{16}\u00A0These are grumble-bunnies, following their own sinful " +
				"desires.", blocks[i].GetText(true, true));
			Assert.IsNull(blocks[i].CharacterId);
		}

		[Test]
		public void Parse_StartQtMilestoneAtEndOfParagraph_IgnoredOrProcessed()
		{
			Assert.Inconclusive("REVIEW: if we encounter a start milestone at the end of a paragraph, " +
				"should we assume that it is a mistake and ignore it, or should we handle it, as " +
				"an opener (perhaps only in the case where the paragraph does not end with " +
				"sentence-ending punctuation)?");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_QtMilestonesLeftOpenFollowedByHebrewSubtitle_WjClosesOpenMilestoneQuote(bool includeSectionHead)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"PSA\"")
						.Replace("<chapter number=\"1\"", "<chapter number=\"39\""),
					"<para style=\"q1\">" +
					GetQtMilestoneElement("start", "David", "Prayer in Psalm 39") +
					"<verse number=\"12\" style=\"v\" />" +
					"“Hear my prayer, Lord, and listen to my cry for help;" +
					"</para>" +
					"<para style=\"q1\">" +
					"Do not be silent to my tears;" +
					"</para>" +
					"<para style=\"q1\">" +
					"For I am a stranger with You," +
					"</para>" +
					"<para style=\"q1\">" +
					"One who lives abroad, like all my fathers." +
					"</para>" +
					"<para style=\"q1\">" +
					"<verse number=\"13\" style=\"v\" />" +
					"Turn Your eyes away from me, that I may become cheerful again" +
					"</para>" +
					"<para style=\"q1\">" +
					"Before I depart and am no more.”" +
					"</para>" +
					// Quote should have been closed here, but there was a mistake in the data.
					"<chapter number=\"40\" style=\"c\" />" +
					(includeSectionHead ? "<para style=\"s\">God Sustains His Servant</para>" : "") +
					"<para style=\"d\">" +
					"For the music director. A Psalm of David." +
					"</para>" +
					"<para style=\"q1\">" +
					"<verse number=\"1\" style=\"v\" />" +
					"I waited patiently for the Lord;" +
					"</para>" +
					"<para style=\"q1\">" +
					"And He reached down to me and heard my cry." +
					"</para>"));
			var parser = GetUsxParser(doc, "PSA");
			var blocks = parser.Parse().ToList();
			int i = 0;

			var chapterCharacter = CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var narrator = CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.Narrator);

			Assert.AreEqual(39, blocks[i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			Assert.AreEqual(12, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, blocks[i].MultiBlockQuote);

			Assert.AreEqual(12, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(12, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(12, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(13, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);

			Assert.AreEqual(13, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual("David", blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, blocks[i].MultiBlockQuote);
			Assert.AreEqual(39, blocks[i].ChapterNumber);

			Assert.AreEqual(40, blocks[++i].ChapterNumber);
			Assert.IsTrue(blocks[i].IsChapterAnnouncement);
			Assert.AreEqual(chapterCharacter, blocks[i].CharacterId);

			if (includeSectionHead)
			{
				Assert.AreEqual("s", blocks[++i].StyleTag);
				Assert.AreEqual(sectionHeadCharacter, blocks[i].CharacterId);
				Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
				Assert.AreEqual("God Sustains His Servant", blocks[i].GetText(true, true));
			}

			Assert.AreEqual("d", blocks[++i].StyleTag);
			Assert.AreEqual(narrator, blocks[i].CharacterId);
			Assert.AreEqual(40, blocks[i].ChapterNumber);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);
			Assert.AreEqual("For the music director. A Psalm of David.", blocks[i].GetText(true, true));

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(40, blocks[i].ChapterNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("q1", blocks[i].StyleTag);
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(1, blocks[++i].InitialStartVerseNumber);
			Assert.AreEqual(40, blocks[i].ChapterNumber);
			Assert.IsFalse(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.AreEqual("q1", blocks[i].StyleTag);
			Assert.IsNull(blocks[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, blocks[i].MultiBlockQuote);

			Assert.AreEqual(++i, blocks.Count);
		}

		[Test]
		public void Parse_QtMilestonesQuoteOpenPastLastVerseWhereCharacterIsExpected_LastBlockForExpectedVerseAndBlockPrecedingCloseNeedsReview()
		{
			Assert.Ignore("Write this test (unless we determine that the QuoteParser can handle this case.");
		}

		[Test]
		public void Parse_QtMilestonesNotClosed_LastBlockForExpectedVerseNeedsReview()
		{
			Assert.Ignore("Write this test (unless we determine that the QuoteParser can handle this case.");
		}

		private string GetQtMilestoneElement(string startOrEnd, string character = null, string qtId = null, int level = 0)
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
		#endregion PG-1419 - Quote milestones

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
		public void Parse_MultiParagraphTitleFollowedByChapter_TitleIsSimplified()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<para style=\"h\">header</para>" +
				"<para style=\"mt2\">The Gospel According to</para>" +
				"<para style=\"mt1\">Mark</para>" +
				"<chapter number=\"1\" style=\"c\" />" +
				"<para style=\"p\">" +
				"<verse number=\"1\" style=\"v\" />" +
				"Acakki me lok me kwena maber i kom Yecu Kricito</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Mark", blocks[0].GetText(true));
			Assert.AreEqual("header", parser.PageHeader);
			Assert.AreEqual("Mark", parser.MainTitle);
		}

		[Test]
		public void Parse_MultiParagraphTitleNotFollowedByChapter_TitleIsSimplified()
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
				// These chapters will get pruned because you can't end a book with an
				// empty chapter.
				"<chapter number=\"1\" style=\"c\" />" +
				"<chapter number=\"2\" style=\"c\" />" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(1, blocks.Count);
			Assert.AreEqual("mt", blocks[0].StyleTag);
			Assert.AreEqual("The Gospel According to Markus", blocks[0].GetText(false));
			Assert.AreEqual("The Gospel According to Markus", blocks[0].GetText(true));
			Assert.AreEqual("Marco", parser.PageHeader);
			Assert.AreEqual("Markus", parser.MainTitle);
		}

		[Test]
		public void ParseBooks_OnlyEmptyChapters_EmptyBookNotAdded()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart +
				"<chapter number=\"1\" style=\"c\" />" +
				"<chapter number=\"2\" style=\"c\" />" +
				UsxDocumentTests.kUsxFrameEnd);

			var books = UsxParser.ParseBooks(new[] {new UsxDocument(doc)}, new TestStylesheet(),
				null, null);
			Assert.AreEqual(0, books.Count);
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
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), null, new UsxDocument(doc).GetChaptersAndParas());
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
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), null, new UsxDocument(doc).GetChaptersAndParas());
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

		[Test]
		public void Parse_AcrosticHeading_BlockAddedWithQaTagAndBCCharacterId()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				UsxDocumentTests.kUsxFrameStart.Replace("MRK", "PSA") +
				"<para style=\"mt1\">Salmos</para>" + Environment.NewLine +
				"<chapter number=\"119\" style=\"c\" />" + Environment.NewLine +
				"<para style=\"qa\">Alef</para>" + Environment.NewLine +
				"<para style=\"q\">" + Environment.NewLine +
				"<verse number=\"1\" style=\"v\"/>Bienaventurados los perfectos de camino.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc, "PSA");
			var blocks = parser.Parse().ToList();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("Alef", blocks[2].GetText(true));
			Assert.AreEqual("qa", blocks[2].StyleTag);
			Assert.AreEqual(0, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.BookOrChapter), blocks[2].CharacterId);
		}

		private UsxParser GetUsxParser(XmlDocument doc, string bookId = "MRK", ICharacterUsageStore characterUsageStore = null)
		{
			if (characterUsageStore == null)
			{
				characterUsageStore = new CharacterUsageStore(ScrVers.English, 
					ControlCharacterVerseData.Singleton, null);
			}

			return new UsxParser(bookId, new TestStylesheet(), characterUsageStore,
				new UsxDocument(doc).GetChaptersAndParas());
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
