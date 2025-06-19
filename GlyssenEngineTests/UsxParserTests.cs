using Glyssen.Shared;
using GlyssenEngine;
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
using GlyssenCharacters;
using static GlyssenCharacters.CharacterVerseData;
using Resources = GlyssenCharactersTests.Properties.Resources;
using static GlyssenSharedTests.CustomConstraints;

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
										@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
										"<verse number=\"2\" style=\"v\" />" +
										@"kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[0].CharacterIs("MRK", StandardCharacter.BookOrChapter), Is.True);
			Assert.That(blocks[1].CharacterId, Is.EqualTo(Block.kNotSet));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].GetText(false), Is.EqualTo("Acakki me lok me kwena maber i " +
				"kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1}\u00A0Acakki me lok me kwena " +
				"maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
				"{2}\u00A0kit ma gicoyo kwede i buk pa lanebi Icaya ni,"));
		}

		[Test]
		public void Parse_ParagraphWithNote_NoteIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
				"<verse number=\"3\" style=\"v\" />" +
				"<note caller=\"-\" style=\"x\">" +
				"<char style=\"xo\" closed=\"false\">1.3: </char>" +
				"<char style=\"xt\" closed=\"false\">Ic 40.3</char>" +
				@"</note>dwan dano mo ma daŋŋe ki i tim ni,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false),
				Is.EqualTo(@"dwan dano mo ma daŋŋe ki i tim ni,"));
			Assert.That(blocks[1].GetText(true),
				Is.EqualTo("{3}\u00A0dwan dano mo ma daŋŋe ki i tim ni,"));
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerseAndNote_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
				"“pe, kadi ki acel.” " +
				"<verse number=\"3\" /><note /> " +
				"“Guŋamo doggi calo lyel ma twolo,”" +
				"</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo("“pe, kadi ki acel.” “Guŋamo doggi calo lyel ma twolo,”"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("“pe, kadi ki acel.” " +
				"{3}\u00A0“Guŋamo doggi calo lyel ma twolo,”"));
			Assert.That(((ScriptText)blocks[1].BlockElements[0]).Content, Is.EqualTo("“pe, kadi ki acel.” "));
			Assert.That(((ScriptText)blocks[1].BlockElements[2]).Content, Is.EqualTo(
				"“Guŋamo doggi calo lyel ma twolo,”"));
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
			Assert.That(blocks.Count, Is.EqualTo(4));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("{8}\u00A0Trembling, the women fled because they were afraid."));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("[{9}\u00A0When Jesus rose, he first appeared to Mary. " +
							"{10}\u00A0She told those who were weeping. " +
							"{11}\u00A0They didn't believe it.]"));
			Assert.That(blocks[2].StartsAtVerseStart, Is.True);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(8));
			Assert.That(blocks[3].StartsAtVerseStart, Is.True);
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(9));
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
			Assert.That(blocks.Count, Is.EqualTo(3));
			var lastBlock = blocks.Last();
			Assert.That(lastBlock.IsScripture, Is.True);
			Assert.That(lastBlock.GetText(true), Is.EqualTo("{1}\u00A0This is Scripture text."));
			Assert.That(lastBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(lastBlock.ChapterNumber, Is.EqualTo(15));
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
				"<verse number=\"10\" style=\"v\"/>A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na" +
				$", ‑deliin{v10Ending}<verse number=\"11\" style=\"v\"/>{punctInMissingVerse} " +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">18.11 </char>" +
				"<char style=\"ft\" closed=\"false\">‑Godogodofluwia ‑pla: " +
				"«Ka Nclɔɔa 'Cʋa ci 'lɛ le ɔ 'ka maa ‑ɔ mlɔa 'lɛ na gbʋʋnsa na.» </char></note> " +
				"<verse number=\"12\" style=\"v\"/>'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka " +
				"'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"<verse number=\"13\" style=\"v\"/>N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[2].GetText(true), Is.EqualTo(
				"{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin" + v10Ending +
				"{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ " +
				"'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ."));
			Assert.That(blocks[2].StartsAtVerseStart, Is.True);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(10));
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
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. " +
				"{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ."));
			Assert.That(blocks[2].StartsAtVerseStart, Is.True);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(10));
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
			Assert.That(blocks.Count, Is.EqualTo(5));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("The coolest section head ever"));
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(10));
			Assert.That(blocks[4].GetText(true), Is.EqualTo("{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa " +
				"bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ."));
			Assert.That(blocks[4].StartsAtVerseStart, Is.True);
			Assert.That(blocks[4].InitialStartVerseNumber, Is.EqualTo(12));
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
			Assert.That(blocks.Count, Is.EqualTo(5));
			Assert.That(blocks[2].GetText(true), Is.EqualTo(
				"{10}\u00A0A zʋlʋ pɔlɛ 'kʋ ɩya. N solu 'nylugo ‑laagɔɔn na, ‑deliin. "));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("The coolest section head ever"));
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(10));
			Assert.That(blocks[4].GetText(true), Is.EqualTo("{12}\u00A0'Nsasa a 'lɛ wɔlɩ ‑naa " +
				"bha? Gbazɩ nclɔɔ ‑ka 'nyɩ ‑ka mlɔ na, 'ɔ cɩ 'ta 'ka ‑ɛ mlɔ 'lɛ na, mʋ bha? " +
				"{13}\u00A0N solu anyɩ ɩ ‑glɩ ‑nʋawlɛ."));
			Assert.That(blocks[4].StartsAtVerseStart, Is.True);
			Assert.That(blocks[4].InitialStartVerseNumber, Is.EqualTo(12));
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerseAndNoteWithFollowingVerse_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"> <verse number=\"1\" /> <note /> Pi <verse number=\"2\" />Wan </para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo("Pi Wan "));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1}\u00A0Pi {2}\u00A0Wan "));
			Assert.That(((ScriptText)blocks[1].BlockElements[1]).Content, Is.EqualTo("Pi "));
			Assert.That(((ScriptText)blocks[1].BlockElements[3]).Content, Is.EqualTo("Wan "));
		}

		[Test]
		public void Parse_ParagraphWithSpaceAfterVerse_ExtraSpaceIsRemoved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">" +
				"“pe, kadi ki acel.” " +
				"<verse number=\"3\" /> " +
				"“Guŋamo doggi calo lyel ma twolo,”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"“pe, kadi ki acel.” “Guŋamo doggi calo lyel ma twolo,”"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"“pe, kadi ki acel.” {3}\u00A0“Guŋamo doggi calo lyel ma twolo,”"));
			Assert.That(((ScriptText)blocks[1].BlockElements[0]).Content, Is.EqualTo(
				"“pe, kadi ki acel.” "));
			Assert.That(((ScriptText)blocks[1].BlockElements[2]).Content, Is.EqualTo(
				"“Guŋamo doggi calo lyel ma twolo,”"));
		}

		[Test]
		public void Parse_ParagraphWithFigure_FigureIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"><verse number=\"18\" style=\"v\" />" +
				@"Ci cutcut gutugi weko obwogi, gulubo kore." +
				"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
				@"Cutcut gutugi weko obwugi</figure></para >");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"Ci cutcut gutugi weko obwogi, gulubo kore."));
		}

		[Test]
		public void Parse_ParagraphWithFigureInMiddle_FigureIsIgnored()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"This text is before the figure, " +
				"<figure style=\"fig\" desc=\"\" file=\"4200118.TIF\" size=\"col\" loc=\"\" copy=\"\" ref=\"1.18\">" +
				@"Cutcut gutugi weko obwugi</figure>" +
				"and this text is after.</para >");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(1));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"This text is before the figure, and this text is after."));
		}

		[Test]
		public void Parse_SpaceAfterFigureBeforeVerseMaintained()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"Text before figure." +
				"<figure /> <verse number=\"2\" />Text after figure.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(3));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"Text before figure. Text after figure."));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"Text before figure. {2}\u00A0Text after figure."));
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
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"If you don't always remember things, you will sometimes forget!"));
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
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"If you don't always remember things, you will sometimes forget!"));
		}

		[Test]
		public void Parse_WhitespaceAtBeginningOfParaNotPreserved()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\"> <verse number=\"2\" />Text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo("Text"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{2}\u00A0Text"));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerse()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"q1\">ma bigero yoni;</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(@"ma bigero yoni;"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(@"ma bigero yoni;"));
		}

		[Test]
		public void Parse_ParagraphStartsMidVerseAndHasAnotherVerse()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. " +
				"<verse number=\"13\" style=\"v\" />" +
				"Ci obedo i tim nino pyeraŋwen; Catan ocako bite, " +
				@"ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. Ci obedo i tim nino pyeraŋwen; " +
				"Catan ocako bite, ma onoŋo en tye kacel ki lee tim, kun lumalaika gikonye."));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"Cutcut Cwiny Maleŋ otero Yecu woko wa i tim. " +
				"{13}\u00A0Ci obedo i tim nino pyeraŋwen; Catan ocako bite, ma onoŋo en tye " +
				"kacel ki lee tim, kun lumalaika gikonye."));
		}

		[Test]
		public void Parse_ChapterAndPara_BecomeTwoBlocks()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			VerifyChapterBlock(blocks[0], 1);
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(@"Lok ma Jon Labatija otito"));
		}

		[Test]
		public void Parse_GlobalChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"s1\">" +
				@"Lok ma Jon Labatija otito</para>", kUsxFrameWithGlobalChapterLabel);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			VerifyChapterBlock(blocks[0], 1, text:"Global-Chapter 1");
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(@"Lok ma Jon Labatija otito"));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[1].IsParagraphStart, Is.True);
		}

		[Test]
		public void Parse_SpecificChapterLabel()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"<para style=\"cl\">Specific-Chapter One</para><para style=\"s1\">" +
				@"Lok ma Jon Labatija otito</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			VerifyChapterBlock(blocks[0], 1, text:"Specific-Chapter One", tag:"cl");
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(@"Lok ma Jon Labatija otito"));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[1].IsParagraphStart, Is.True);
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
			Assert.That(blocks.Count, Is.EqualTo(9));
			VerifyChapterBlock(blocks[1], 1, "EPH");
			VerifyChapterBlock(blocks[5], 6, "EPH");
			var lastBlock = blocks.Last();
			Assert.That(lastBlock.GetText(true), Is.EqualTo(
				"{23}\u00A0Mbolimo i Pue Ala Papa, pai i Yesu Kerisitu da mawai jaya ri pura - " +
				"pura anggota dompu kasamba'a-mba'a pai pombepotowe pai todo ri peaya ri Kerisitu. " +
				"{24}\u00A0Mbolimo i Pue Ala da madonco komi pura - pura anu mampotowe Pueta i " +
				"Yesu Kerisitu pai towe ndaya anu bare'e da re'e kabalinya."));
			Assert.That(blocks.Any(b => b.GetText(false).Contains(@"Petubunaka")), Is.False);
		}

		[Test]
		public void Parse_ProcessChaptersAndVerses_BlocksGetCorrectChapterAndVerseNumbers()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
										"<verse number=\"2\" style=\"v\" />" +
										@"kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>" +
										"<chapter number=\"2\" style=\"c\" />" +
										"<para style=\"p\">" +
										"<verse number=\"1\" style=\"v\" />" +
										@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>" +
										"<para style=\"q1\">" +
										"This is poetry, dude.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(5));
			VerifyChapterBlock(blocks[0], 1);
			Assert.That(blocks[1].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1}\u00A0" +
				@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, " +
				"{2}\u00A0" +
				@"kit ma gicoyo kwede i buk pa lanebi Icaya ni,"));

			Assert.That(blocks[2].StyleTag, Is.EqualTo("c"));
			VerifyChapterBlock(blocks[2], 2);
			Assert.That(blocks[3].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[3].ChapterNumber, Is.EqualTo(2));
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("{1}\u00A0" +
				@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(blocks[4].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[4].ChapterNumber, Is.EqualTo(2));
			Assert.That(blocks[4].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[4].GetText(true), Is.EqualTo("This is poetry, dude."));
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
				@"Ba Yusufu wi keni ye ngonimo ne be.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(7),
				"Should have a chapter block, 4 \"scripture\" blocks, and one regular \\q block.");
			Assert.That(blocks.Skip(1).Take(4), ForEvery<Block>(b => b.CharacterId,
				Is.EqualTo("scripture")));
			Assert.That(blocks[5].GetText(true), Does.StartWith(@"Emanuweli."),
				"Period should get pulled into the \"scripture\" block with \"Emanuweli.\" " +
				"We probably don't really care if the trailing space is retained or not.");
			Assert.That(blocks.Last().GetText(true), Is.EqualTo(
				"Kire wi jo “Kulocelie ye ne we ni.” " +
				"{24}\u00A0Ba Yusufu wi keni ye ngonimo ne be."));
		}

		[TestCase("wj")]
		[TestCase("qt")]
		public void Parse_MappedMarkerInsideQuotationMarks_AdjacentPunctuationIncludedInBlockWithQuotedText(string sfMarker)
		{
			Assert.That(StyleToCharacterMappings.TryGetCharacterForCharStyle(sfMarker, out var character), Is.True,
				$"Setup condition not met: marker \"{sfMarker}\" in TestCase should be included " +
				$"in {nameof(StyleToCharacterMappings)}.");
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"18\" style=\"v\" />" +
				@"Kulocɛliɛ céki 'juu mɛ́ nɛ́ jo: «" +
				$"<char style=\"{sfMarker}\">Siaga nī muɔ bésimɛ ta bè</char>" +
				"<note caller=\"+\" style=\"f\"><char style=\"fr\" closed=\"false\">11.18 </char><char style=\"ft\" closed=\"false\">" +
				"<char style=\"xt\" closed=\"true\">Sél 21.12</char>" +
				@"tire ti 'juu náʔa gè.</char></note>" +
				".»" +
				"<verse number=\"19\" style=\"v\" />" +
				@"Nɛ̀ kiyaʔa Birayoma yéki sɔ̀ngi nɛ̀ tɛ́ngɛ ki nɛ̄ dí Kucɛliɛ bèle. Kire nɛ̄ wire kiyɛ́nì kpíʔile." +
				"</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(4),
				"Should have a chapter block, plus 3 Scripture blocks.");
			Assert.That(blocks[2].CharacterId, Is.EqualTo(character));
			Assert.That(blocks[1].GetText(true).TrimEnd(), Does.EndWith("jo:"));
			Assert.That(blocks[2].GetText(true).Trim(), Is.EqualTo(
				"«Siaga nī muɔ bésimɛ ta bè.»"));
			Assert.That(blocks[3].StartsAtVerseStart, Is.True);
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(19));
		}

		#region PG-1419 - Quote milestones
		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		[TestCase(null, @"Enoc", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesWithOnlyTextBetweenThem_TextBetweenMilestonesAddedAsQuoteBlock(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				@"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				@"séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", character, qtId,  level) +
				@"He aquí, vino el Señor con sus santas decenas de millares." +
				GetQtMilestoneElement("end", character, qtId,  level) +
				" <verse number=\"15\" style=\"v\" />" +
				"The quote should continue in this verse but it does not." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(4),
				"Should have a chapter block, plus 3 Scripture blocks.");
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[1].StartsAtVerseStart, Is.True);
			Assert.That(blocks[1].GetText(true), Does.EndWith(@"diciendo: "));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[2].StartsAtVerseStart, Is.False);
			Assert.That(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[2].GetText(true), Is.EqualTo(@"He aquí, vino el Señor con sus santas decenas de millares. "));
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(quoteIdAnnotation.IsNarrator, Is.False);
			}
			VerifyQuoteEnd(blocks[2], qtId);
			if (blocks[2].CharacterId == kNeedsReview)
				Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(character));
			Assert.That(blocks[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[3].StartsAtVerseStart, Is.True);
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[3].CharacterId, Is.Null);
			Assert.That(blocks[3].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			return blocks[2].CharacterId;
		}

		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesInsideEnclosingQuotes_AdjacentPunctuationIncludedInBlockWithQuotedText(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				@"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				@"séptimo desde Adán, diciendo: «" +
				GetQtMilestoneElement("start", character, qtId,  level) +
				@"He aquí, vino el Señor con sus santas decenas de millares." +
				GetQtMilestoneElement("end", character, qtId,  level) +
				"» <verse number=\"15\" style=\"v\" />" +
				"The quote should continue in this verse but it does not." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(4),
				"Should have a chapter block, plus 3 Scripture blocks.");
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[1].StartsAtVerseStart, Is.True);
			Assert.That(blocks[1].GetText(true), Does.EndWith(@"diciendo: "));
			Assert.That(blocks[1].CharacterId, Is.Null);

			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[2].StartsAtVerseStart, Is.False);
			Assert.That(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[2].GetText(true), Is.EqualTo(@"«He aquí, vino el Señor con sus santas decenas de millares.» "));
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
			}
			VerifyQuoteEnd(blocks[2], qtId);
			if (blocks[2].CharacterId == kNeedsReview)
				Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(character));
			VerifyQuoteEnd(blocks[2], qtId);

			Assert.That(blocks[3].StartsAtVerseStart, Is.True);
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[3].CharacterId, Is.Null);
			Assert.That(blocks[3].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[3].IsPredeterminedFirstLevelQuoteEnd, Is.False);

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
				@"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				@"séptimo desde Adán, diciendo que el Señor venía con sus santas decenas de millares.";
			if (milestoneBeforeVerseNumber)
				usx += GetQtMilestoneElement("start", character, qtId,  1);
			usx += "<verse number=\"15\" style=\"v\" />";
			if (!milestoneBeforeVerseNumber)
				usx += GetQtMilestoneElement("start", character, qtId,  1);
			usx += @"“Haré juicio contra todos para convencer a todos los impíos de entre ellos tocante a todas sus obras de impiedad.”" +
				GetQtMilestoneElement("end", character, qtId,  1) +
				"</para>";

			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				usx));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3),
				"Should have a chapter block, plus 2 Scripture blocks.");
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[1].StartsAtVerseStart, Is.True);
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{14}\u00A0" +
				@"De éstos también profetizó Enoc, séptimo desde Adán, " +
				@"diciendo que el Señor venía con sus santas decenas de millares."));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[2].StartsAtVerseStart, Is.True);
			Assert.That(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[2].GetText(true), Is.EqualTo("{15}\u00A0" +
				@"“Haré juicio contra todos para convencer a todos los " +
				@"impíos de entre ellos tocante a todas sus obras de impiedad.”"));
			var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
			Assert.That(quoteIdAnnotation.Start, Is.True);
			Assert.That(blocks[2].CharacterId, Is.EqualTo(kNeedsReview),
				$"Because {character} is not expected to speak in JUD 15.");
			Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(character));
			quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.Last();
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
			Assert.That(quoteIdAnnotation.Start, Is.False);
		}

		[TestCase(ExpectedResult = null)]
		[TestCase("qt_123", ExpectedResult = null)]
		[TestCase("qt_123", null, 1, ExpectedResult = null)]
		[TestCase("qt_123", "Some random name that doesn't look anything like a name we expect", 1, ExpectedResult = kNeedsReview)]
		[TestCase("qt_123", "Enoch", ExpectedResult = "Enoch")]
		[TestCase(null, "Enoch", 1, ExpectedResult = "Enoch")]
		public string Parse_QtMilestonesCoveringMultipleVersesAndParagraphs_MultiBlockQuoteBlocksCreated(
			string qtId = null, string character = null, int level = 0)
		{
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				"<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				@"De éstos también profetizó Enoc," +
				"<note caller=\"-\" style=\"x\"><char style=\"xo\" closed=\"false\">1:14 </char><char style=\"xt\" closed=\"false\">Gn. 5.21-24.</char></note> " +
				@"séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", character, qtId,  level) +
				@"«He aquí, vino el Señor con sus santas decenas de millares;" +
				"</para>" +
				"<para style=\"q1\">" +
				"<verse number=\"15\" style=\"v\" />" +
				@"para hacer juicio contra todos." +
				"</para>" +
				"<para style=\"q2\">" +
				@"Dejará convictos a todos los impíos de sus obras impías y de todas las cosas duras que han hablado contra él" +
				GetQtMilestoneElement("end", character, qtId,  level) +
				@",» dijo Enoc." +
				"</para>"));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(6),
				"Should have a chapter block, a leading narrator" +
				" block, 3 quote blocks and a trailing narrator block.");
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[1].StartsAtVerseStart);
			Assert.That(blocks[1].GetText(true, true), Does.EndWith(@"diciendo: "));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[2].StartsAtVerseStart, Is.False);
			Assert.That(blocks[2].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[2].GetText(true).Trim(),
				Is.EqualTo(@"«He aquí, vino el Señor con sus santas decenas de millares;"));
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[2].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
			}
			if (blocks[2].CharacterId == kNeedsReview)
				Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(character));
			Assert.That(blocks[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			
			Assert.That(blocks[3].StartsAtVerseStart, Is.True);
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[2].CharacterId, Is.EqualTo(blocks[3].CharacterId));
			Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(blocks[3].CharacterIdInScript));
			Assert.That(blocks[3].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[3].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[3].GetText(true, true), Is.EqualTo("{15}\u00A0" +
				@"para hacer juicio contra todos."));
			Assert.That(blocks[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[4].StartsAtVerseStart, Is.False);
			Assert.That(blocks[4].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[2].CharacterId, Is.EqualTo(blocks[4].CharacterId));
			Assert.That(blocks[2].CharacterIdInScript, Is.EqualTo(blocks[4].CharacterIdInScript));
			Assert.That(blocks[4].IsPredeterminedFirstLevelQuoteEnd);
			Assert.That(blocks[4].StyleTag, Is.EqualTo("q2"));
			Assert.That(blocks[4].GetText(true), Is.EqualTo(
				"Dejará convictos a todos los impíos de sus obras impías y de todas las cosas " +
				"duras que han hablado contra él,» "));
			if (qtId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[4].BlockElements.Last();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
			}
			Assert.That(blocks[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[5].StartsAtVerseStart, Is.False);
			Assert.That(blocks[5].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[5].CharacterId, Is.Null);
			Assert.That(blocks[5].StyleTag, Is.EqualTo("q2"));
			Assert.That(blocks[5].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[5].GetText(true, true), Is.EqualTo(@"dijo Enoc."));
			Assert.That(blocks[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(9));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("{4}\u00A0He fell to the ground and heard a voice say to him, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd);
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("“Saul, Saul, why do you persecute me?”"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].Delivery, Is.EqualTo("questioning"));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{5}\u00A0“Who are you, Lord?”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Paul"));
			Assert.That(blocks[i].Delivery, Is.EqualTo("awe"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("Saul asked."));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd);
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("“I am Jesus, whom you are persecuting,”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].Delivery, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("he replied."));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{6}\u00A0“" +
				"Now get up and go into the city, and you will be told what you must do.”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].Delivery, Is.Null);

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		bool HasAPeterAndJohnCharacter(ICharacterDeliveryInfo cv)
		{
			var individuals = cv.Character.Split(new[] { CharacterSpeakingMode.kMultiCharacterIdSeparator }, StringSplitOptions.None);
			return individuals.Length > 1 && individuals.Any(c => c == "John") && individuals.Any(c => c == "Peter (Simon)");
		}

		// Note: In production, we have tried to clean up this kind of C-V data so this can't
		// happen because it seldom makes sense.
		[Test]
		public void Parse_QtMilestonesForTwoVersesWithSameGroupCharactersButDifferentDefaults_DefaultFromFirstVerseUsed()
		{
			var bookNbrActs = BCVRef.BookToNumber("ACT");
			var chapter = 4;

			// Confirm pre-conditions in CV file
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 19)
				.Single(HasAPeterAndJohnCharacter).DefaultCharacter, Is.Not.EqualTo(
				ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 20)
				.Single(HasAPeterAndJohnCharacter).DefaultCharacter));

			// Setup
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
					.Replace("<chapter number=\"1\"", $"<chapter number=\"{chapter}\""),
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

			// SUT
			var blocks = parser.Parse().ToList();

			// Verify
			int i = 0;
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(chapter));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{19}\u00A0But Peter and John replied, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! " +
				"{20}\u00A0As for us, we cannot help speaking about what we have seen and heard.”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Peter (Simon)/John"));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Peter (Simon)"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("Peter (Simon)", "John")]
		[TestCase("Peter", "John")]
		[TestCase("John", "Peter")]
		[TestCase("John", "Peter (Simon)")]
		public void Parse_QtMilestonesForExplicitMemberOfGroup_CharacterInUseSetAsSpecified(
			string characterForV19, string characterForV20)
		{
			var bookNbrActs = BCVRef.BookToNumber("ACT");
			var chapter = 4;

			// Confirm pre-conditions in CV file
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 19)
				.Any(HasAPeterAndJohnCharacter), Is.True);
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 20)
				.Any(HasAPeterAndJohnCharacter), Is.True);

			// Setup
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
					.Replace("<chapter number=\"1\"", $"<chapter number=\"{chapter}\""),
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

			// SUT
			var blocks = parser.Parse().ToList();

			// Verify
			if (characterForV19 == "Peter")
				characterForV19 = "Peter (Simon)";
			if (characterForV20 == "Peter")
				characterForV20 = "Peter (Simon)";

			int i = 0;
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(chapter));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{19}\u00A0But Peter and John replied, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! "));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Peter (Simon)/John"));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo(characterForV19));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(20));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{20}\u00A0As for us, we cannot help speaking about what we have seen and heard.”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Peter (Simon)/John"));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo(characterForV20));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		// Note: In production, we have tried to clean up this kind of C-V data so this can't
		// happen because it seldom makes sense.
		[Test]
		public void Parse_SeparateQtMilestonesForTwoVersesWithSameGroupCharactersButDifferentDefaults_DefaultFromEachVerseUsed()
		{
			var bookNbrActs = BCVRef.BookToNumber("ACT");
			var chapter = 4;

			// Confirm pre-conditions in CV file
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 19)
				.Single(HasAPeterAndJohnCharacter).DefaultCharacter, Is.Not.EqualTo(
				ControlCharacterVerseData.Singleton.GetCharacters(bookNbrActs, chapter, 20)
					.Single(HasAPeterAndJohnCharacter).DefaultCharacter));

			// Setup
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"ACT\"")
					.Replace("<chapter number=\"1\"", $"<chapter number=\"{chapter}\""),
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

			// SUT
			var blocks = parser.Parse().ToList();

			// Verify
			int i = 0;
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(chapter));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("{19}\u00A0But Peter and John replied, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(19));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"“Which is right in God’s eyes: to listen to you, or to him? You be the judges! "));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Peter (Simon)/John"));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Peter (Simon)"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(20));
			Assert.That(blocks[i].StartsAtVerseStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{20}\u00A0As for us, we cannot help speaking about what we have seen and heard.”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Peter (Simon)/John"));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("John"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{7}\u00A0But the Lord said to me, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"“Do not say, ‘I am too young.’ You must go to everyone I send you to and say " +
				"whatever I command you. " +
				"{8}\u00A0Do not be afraid of them, for I am with you and will rescue you,” "));
			VerifyQuoteEnd(blocks[i], qtId1);
			if (qtId1 == null)
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(4));
			else
			{
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(5));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId1));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(blocks[i].CharacterId, Is.EqualTo("God"));
			}

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(8));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("declares the Lord."));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("one", "quotation")]
		[TestCase("one", "quotation", false)]
		[TestCase("one")]
		[TestCase]
		[TestCase(null, null, false)]
		public void Parse_NestedQuoteWithExplicitMilestonesUsesLevel1_NeedsReview(
			string qtId1 = null, string qtId2 = null, bool includeCharacterInEndMilestones = true)
		{
			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JER\""),
					"<para style=\"p\">" +
					"<verse number=\"7\" style=\"v\" />" +
					"But the Lord said to me, " +
					GetQtMilestoneElement("start", "God", qtId1, 1) +
					"“Do not say, " +
					GetQtMilestoneElement("start", "Jeremiah", qtId2, 1) +
					"‘I am too young.’" +
					GetQtMilestoneElement("end", includeCharacterInEndMilestones ? "Jeremiah" : null, qtId2, 1) +
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{7}\u00A0But the Lord said to me, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("“Do not say, "));
			if (qtId1 != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId1));
				Assert.That(quoteIdAnnotation.Start, Is.True);
			}
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("God"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("‘I am too young.’ "));
			VerifyQuoteEnd(blocks[i], qtId2);
			if (qtId2 == null)
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(2));
			else
			{
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(3));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId2));
				Assert.That(quoteIdAnnotation.Start, Is.True);
			}
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Jeremiah"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"You must go to everyone I send you to and say whatever I command you. " +
				"{8}\u00A0Do not be afraid of them, for I am with you and will rescue you,” "));
			VerifyQuoteEnd(blocks[i], qtId1);
			Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(4));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(8));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("declares the Lord."));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("Interruption", @"En Gedi info")]
		[TestCase("interruption-2CH")]
		[TestCase(@"narr-2CH")]
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
					@"“A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar " +
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(20));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("{2}\u00A0Some people came and told Jehoshaphat, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"“A vast army is coming against you from Edom, from the other side of the Dead " +
				"Sea. Hazezon Tamar "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("men, some"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			if (qtId == null)
				Assert.That(blocks[i].GetText(true, true), Is.EqualTo("(that is, En Gedi) "));
			else
			{
				Assert.That(blocks[i].GetText(true), Is.EqualTo("(that is, En Gedi) "));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
			}
			Assert.That(blocks[i].CharacterIs("2CH", StandardCharacter.Narrator));
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("is where they are currently camped.” "));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("men, some"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("All Judah must fast. "));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jehoshaphat, king of Judah"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("interruption", @"En Gedi info")]
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
					@"After this, the Moabites and others came to war against Jehoshaphat. " +
					"</para>" +
					"<para style=\"p\">" +
					"<verse number=\"2\" style=\"v\" />" +
					"Some people came and told Jehoshaphat, " +
					GetQtMilestoneElement("start", "men, some", qtMenId, 1) +
					@"“A vast army is coming against you from Edom, from the other side of the Dead Sea. They have already reached Hazezon Tamar " +
					GetQtMilestoneElement("start", interruptionCharacter, qtInterruptionId, 2) +
					@"(that is, En Gedi)" +
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(20));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("{2}\u00A0Some people came and told Jehoshaphat, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			if (qtMenId == null)
			{
				Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
					"“A vast army is coming against you from Edom, from the other " +
					"side of the Dead Sea. They have already reached Hazezon Tamar "));
			}
			else
			{
				Assert.That(blocks[i].GetText(true), Is.EqualTo(
					"“A vast army is coming against you from Edom, from the other " +
					"side of the Dead Sea. They have already reached Hazezon Tamar "));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtMenId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(2));
			}
			Assert.That(blocks[i].CharacterId, Is.EqualTo("men, some"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("(that is, En Gedi)”. "));
			var expectedBlockElementCount = 2;
			if (qtInterruptionId != null)
			{
				expectedBlockElementCount += 3;
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[0];
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtInterruptionId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(((ScriptText)blocks[i].BlockElements[1]).Content, Is.EqualTo(@"(that is, En Gedi)"));
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[2];
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtInterruptionId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
				Assert.That(((ScriptText)blocks[i].BlockElements[3]).Content, Is.EqualTo("”. "));
			}
			VerifyQuoteEnd(blocks[i], qtMenId);
			Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(expectedBlockElementCount));
			Assert.That(blocks[i].CharacterIs("2CH", StandardCharacter.Narrator), Is.True);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.True);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{3}\u00A0Alarmed, Jehoshaphat" +
				" resolved to inquire of the Lord, and he proclaimed: "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteEnd);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("All Judah must fast. "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jehoshaphat, king of Judah"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("Interruption", @"En Gedi info")]
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
					@"After this, the Moabites and others came to war against Jehoshaphat. " +
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(20));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{2}\u00A0Some people came and told Jehoshaphat, “A vast army is coming" +
				@" against you from Edom, from the other side of the Dead Sea. Hazezon Tamar "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			if (qtId == null)
				Assert.That(blocks[i].GetText(true, true), Is.EqualTo(@"(that is, En Gedi) "));
			else
			{
				Assert.That(blocks[i].GetText(true), Is.EqualTo(@"(that is, En Gedi) "));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
			}
			Assert.That(blocks[i].CharacterIs("2CH", StandardCharacter.Narrator));
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.True);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"is where they are currently camped.” {3}\u00A0Alarmed, Jehoshaphat " +
				"resolved to inquire of the Lord, and he proclaimed a fast for all Judah. "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[TestCase("narrator-2CH")]
		[TestCase("narrator-2CH", true)]
		[TestCase(@"narr", true, "no one", "no one")]
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(12));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{1}\u00A0Now about Spirit-given " +
				"abilities, brothers, I do not want you to be confused. " +
				"{2}\u00A0You know that as pagans, you were led astray to mute idols. " +
				"{3}\u00A0So I want you to clearly understand that no one who is speaking by " +
				"the Holy Spirit could ever say, “Jesus be cursed,” and no one can confess, " +
				"“Jesus is Lord,” except under the leading of the Holy Spirit."));
			Assert.That(blocks[i].CharacterId, Is.Null);

			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[6];
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId1));
			Assert.That(quoteIdAnnotation.Start, Is.True);
			Assert.That(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[8];
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId1));
			Assert.That(quoteIdAnnotation.Start, Is.False);
			Assert.That(quoteIdAnnotation.IsNarrator);

			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[10];
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId2));
			Assert.That(quoteIdAnnotation.Start, Is.True);
			Assert.That(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[12];
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtId2));
			Assert.That(quoteIdAnnotation.Start, Is.False);
			Assert.That(quoteIdAnnotation.IsNarrator);

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(22));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{22}\u00A0All this took place to fulfill what the Lord had said through the " +
				"prophet: "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{23}\u00A0“The virgin will conceive and give birth to a son, and Immanuel "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.True);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("(which means ‘God with us’) "));
			Assert.That(blocks[i].CharacterIs("MAT", StandardCharacter.Narrator));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("will be the name by which they will call him.”"));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		/// <summary>
		/// This test (in contrast to the following one) helps to illustrate the distinction
		/// between the explicit use of an "interruption" character (which will always be
		/// treated as an interruption) as opposed to "narrator".
		/// </summary>
		[TestCase(1)]
		[TestCase(2)]
		public void Parse_ExplicitlyMarkedInterruptionButNoQuoteInVerseWithPotentialNarratorQuote_InterruptionBrokenOut(
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(22));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{22}\u00A0All this took place to fulfill what the Lord had said through the " +
				"prophet: " +
				"{23}\u00A0“The virgin will conceive and give birth to a son, and Immanuel "));
			Assert.That(blocks[i].CharacterId, Is.Null);
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("(which means ‘God with us’) "));
			Assert.That(blocks[i].CharacterIs("MAT", StandardCharacter.Narrator));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(23));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("will be the name by which they will call him.”"));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
					GetQtMilestoneElement("start", @"narr-MAT") +
					"“God with us”" +
					GetQtMilestoneElement("end", includeCharacterInEndMilestone ? "narrator" : null) +
					")." +
					"</para>"));
			var parser = GetUsxParser(doc, "MAT");
			var blocks = parser.Parse().ToList();

			Assert.That(blocks.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			int i = 0;
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(22));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{22}\u00A0All this happened to fulfill God’s prophetic word: " +
				"{23}\u00A0“The virgin will give birth to a son called ‘Immanuel’” " +
				"(meaning “God with us”)."));
			Assert.That(blocks[i].CharacterId, Is.Null);

			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[4];
			Assert.That(quoteIdAnnotation.Id, Is.Null);
			Assert.That(quoteIdAnnotation.Start, Is.True);
			Assert.That(quoteIdAnnotation.IsNarrator);
			quoteIdAnnotation = (QuoteId)blocks[i].BlockElements[6];
			Assert.That(quoteIdAnnotation.Id, Is.Null);
			Assert.That(quoteIdAnnotation.Start, Is.False);
			Assert.That(quoteIdAnnotation.IsNarrator);

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(20));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0After this, the Moabites and others came to war against Jehoshaphat. "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("{2}\u00A0Some people came and told Jehoshaphat, "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			if (qtMenId == null)
			{
				Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
					"“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar "));
			}
			else
			{
				Assert.That(blocks[i].GetText(true), Is.EqualTo(
					"“A vast army is coming against you from Edom, from the other side of " +
					"the Dead Sea. They have already reached Hazezon Tamar "));
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtMenId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(quoteIdAnnotation.IsNarrator, Is.False);
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(2));
			}
			Assert.That(blocks[i].CharacterId, Is.EqualTo("men, some"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, qtInterruptionId == null && qtMenId == null),
				Is.EqualTo(@"(that is, En Gedi)”. "));
			if (qtInterruptionId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtInterruptionId));
				Assert.That(quoteIdAnnotation.Start, Is.True);
				Assert.That(quoteIdAnnotation.IsNarrator);
			}
			if (qtMenId != null)
			{
				var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtMenId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
				Assert.That(quoteIdAnnotation.IsNarrator, Is.False);
			}
			Assert.That(blocks[i].CharacterIs("2CH", StandardCharacter.Narrator), Is.True);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("All Judah must fast. "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jehoshaphat, king of Judah"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(24));
			Assert.That(blocks[i].IsChapterAnnouncement);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(10));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"{10}\u00A0“At that time many will lose faith, and they will betray and hate one another. " +
				"{11}\u00A0Many people will falsely claim to speak for God in order to deceive others. " +
				"{12}\u00A0As lawlessness increases more and more people will lose their will to love. " +
				"{13}\u00A0But the overcomers will be saved. " +
				"{14}\u00A0Before the end of time, this good news of the kingdom of God will be " +
				"announced everywhere so all the nations will hear it."));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[i].InitialEndVerseNumber, Is.EqualTo(16));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsContinuationOfPreviousBlockQuote, Is.True);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{15-16}\u00A0" +
				"“The Judeans must run to hide in the hills when in the temple the you see the " +
				"abomination of desolation, "));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[i].InitialEndVerseNumber, Is.EqualTo(16));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedQuoteInterruption, Is.True);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true),
				Is.EqualTo("with which a reader of Daniel should be familiar. "));
			Assert.That(blocks[i].CharacterIs("MAT", StandardCharacter.Narrator));
			var quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.First();
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo("reader"));
			Assert.That(quoteIdAnnotation.Start, Is.True);
			Assert.That(quoteIdAnnotation.IsNarrator);
			if (qtEndInterruptionId == null)
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(2));
			else
			{
				// REVIEW: do we want the space added before or after the annotation?
				Assert.That(blocks[i].BlockElements.Count, Is.EqualTo(3));
				quoteIdAnnotation = (QuoteId)blocks[i].BlockElements.Last();
				Assert.That(quoteIdAnnotation.Id, Is.EqualTo(qtEndInterruptionId));
				Assert.That(quoteIdAnnotation.Start, Is.False);
				Assert.That(quoteIdAnnotation.IsNarrator);
			}

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsParagraphStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{17}\u00A0Whoever is on the roof must not go get things out of his house. " +
				"{18}\u00A0Anyone in the field must not go get his robe. " +
				"{19}\u00A0It will really stink for women who are pregnant or nursing in those days! " +
				"{20}\u00A0Also, pray that when you flee the weather will be nice and it will " +
				"not be on a day of rest.”"));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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
					GetQtMilestoneElement("start", "Jesus", @"SOTM") +
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

			var chapterCharacter = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(5));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{3}\u00A0Blessed are the poor in spirit,"));
			// Not sure that we actually care whether the QuoteId annotation comes
			// before or after the verse number.
			Assert.That(blocks[i].BlockElements.Take(2).OfType<QuoteId>().Single().Id,
				Is.EqualTo("SOTM"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StyleTag, Is.EqualTo("q2"));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("for they own the kingdom of heaven."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			
			Assert.That(blocks[++i].StyleTag, Is.EqualTo("s"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(sectionHeadCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			// Parallel passage references do not get included in script.

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(48));
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].ChapterNumber, Is.EqualTo(6));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{1}\u00A0Practice your " +
				"righteousness in front of others and you forfeit your heavenly reward."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{2}\u00A0So when you give to the poor, do not make a big deal of it. " +
				"{3}\u00A0Do not even let your one hand know what the other is doing."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(34));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{34}\u00A0So do not worry about tomorrow; it will worry about itself. " +
				"Each day is enough of a problem."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[++i].ChapterNumber, Is.EqualTo(7));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0Do not judge unless you want to be judged. " +
				"{27}\u00A0The rain came down, the streams rose, and the winds blew and beat " +
				"against that house, and it fell with a great crash."));
			VerifyQuoteEnd(blocks[i]);
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(28));
			Assert.That(blocks[i].InitialEndVerseNumber, Is.EqualTo(29));
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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

			var chapterCharacter = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(8));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].LastVerseNum, Is.EqualTo(2));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"“Sir, if You would, you can cleanse me.” " +
				"{3}\u00A0Jesus reached out with His hand and touched him, saying, "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("leper"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("wj"));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("“Absolutely.” "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"And immediately his leprosy was cleansed."));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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

			var chapterCharacter = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(8));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"“He Himself took our illnesses and carried away our diseases.”"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			Assert.That(blocks[++i].StyleTag, Is.EqualTo("s"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(sectionHeadCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("{18}\u00A0“Let us go over to the other side of the sea,” "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"said Jesus, wishing to ditch the crowd."));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(++i, Is.EqualTo(blocks.Count));
		}

		[Test]
		public void Parse_QtMilestonesForOtherCharacterFollowedBySectionHeadWithKeyword_EntireSectionHeadTreatedAsSingleExtraBlock()
		{
			var doc = UsxDocumentTests.CreateDocFromString(
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

			var chapterCharacter = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(8));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"“He Himself took our illnesses and carried away our diseases.”"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("scripture"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			Assert.That(blocks[++i].StyleTag, Is.EqualTo("s"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(sectionHeadCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true), Is.EqualTo("Discipleship Tested"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true),
				Is.EqualTo("{18}\u00A0“Let us go over to the other side of the sea,” "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo(
				"said Jesus, wishing to ditch the crowd."));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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

			var chapterCharacter = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(5));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{3}\u00A0Blessed are the poor in spirit."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));

			if (includeSectionHead)
			{
				Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

				Assert.That(blocks[++i].StyleTag, Is.EqualTo("s"));
				Assert.That(blocks[i].CharacterId, Is.EqualTo(sectionHeadCharacter));

				Assert.That(blocks[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			}
			else
			{
				Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

				Assert.That(blocks[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			}

			Assert.That(blocks[i].InitialStartVerseNumber, Is.EqualTo(48));
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{48}\u00A0Be perfect, then, as your Father is perfect."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(blocks[++i].ChapterNumber, Is.EqualTo(6));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{1}\u00A0Practice your righteousness in front of others and you " +
				"forfeit your heavenly reward."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"{2}\u00A0So when you give to the poor, do not make a big deal of it."));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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

			Assert.That(blocks.Single().CharacterId,
				Is.EqualTo(GetStandardCharacterId("MAT", StandardCharacter.Intro)));
		}

		[Test]
		public void Parse_DuplicateStartQtMilestone_BlockBreakInsertedAndMarkedAsNeedsReview()
		{
			var usx = "<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				@"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", "Enoch") +
				@"El Señor viene con sus santas decenas de millares. " +
				"<verse number=\"15\" style=\"v\" />" +
				GetQtMilestoneElement("start", "Enoch") +
				@"“Hará juicio contra todos para convencer a todos los impíos " +
				GetQtMilestoneElement("start", "Enoch") +
				@"de entre ellos tocante a todas sus obras de impiedad.”" +
				GetQtMilestoneElement("end") +
				"</para>";

			var doc = UsxDocumentTests.CreateDocFromString(
				string.Format(UsxDocumentTests.kUsxFrame.Replace("\"MRK\"", "\"JUD\""),
				usx));
			var parser = GetUsxParser(doc, "JUD");
			var blocks = parser.Parse().ToList();

			int i = 0;

			var chapterCharacter = GetStandardCharacterId("JUD", StandardCharacter.BookOrChapter);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{14}\u00A0" +
				"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: "));
			Assert.That(blocks[i].CharacterId, Is.Null);
			
			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"El Señor viene con sus santas decenas de millares. "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Enoch"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo("{15}\u00A0" +
				"“Hará juicio contra todos para convencer a todos los impíos "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Enoch"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"de entre ellos tocante a todas sus obras de impiedad.”"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(blocks[i].CharacterIdInScript, Is.EqualTo("Enoch"));
		}

		[Test]
		public void Parse_ExtraneousEndQtMilestone_Ignored()
		{
			var usx = "<para style=\"p\">" +
				"<verse number=\"14\" style=\"v\" />" +
				@"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: " +
				GetQtMilestoneElement("start", "Enoch") +
				@"El Señor viene con sus santas decenas de millares. " +
				"<verse number=\"15\" style=\"v\" />" +
				@"“Hará juicio contra todos para convencer a todos los impíos " +
				@"de entre ellos tocante a todas sus obras de impiedad.” " +
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

			var chapterCharacter = GetStandardCharacterId("JUD", StandardCharacter.BookOrChapter);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[i].StartsAtVerseStart, Is.True);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{14}\u00A0" +
				"De éstos también profetizó Enoc, séptimo desde Adán, diciendo: "));
			Assert.That(blocks[i].CharacterId, Is.Null);

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(14));
			Assert.That(blocks[i].StartsAtVerseStart, Is.False);
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart);
			Assert.That(blocks[i].GetText(true), Is.EqualTo(
				"El Señor viene con sus santas decenas de millares. " +
				"{15}\u00A0“Hará juicio contra todos para convencer a todos los impíos " +
				"de entre ellos tocante a todas sus obras de impiedad.” "));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("Enoch"));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(16));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("{16}\u00A0These " +
				"are grumble-bunnies, following their own sinful desires."));
			Assert.That(blocks[i].CharacterId, Is.Null);
		}

		[Test]
		public void Parse_StartQtMilestoneAtEndOfParagraph_IgnoredOrProcessed()
		{
			Assert.Inconclusive("REVIEW: if we encounter a start milestone at the end of a " +
				"paragraph, should we assume that it is a mistake and ignore it, or should we " +
				"handle it as an opener (perhaps only in the case where the paragraph does not " +
				"end with sentence-ending punctuation)? We should probably wait to resolve this " +
				"question until we actually encounter it on some real data and see what the " +
				"user intended.");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_QtMilestonesLeftOpenFollowedByHebrewSubtitle_HebrewSubtitleClosesOpenMilestoneQuote(bool includeSectionHead)
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

			var chapterCharacter = GetStandardCharacterId("PSA", StandardCharacter.BookOrChapter);
			var sectionHeadCharacter = GetStandardCharacterId("PSA", StandardCharacter.ExtraBiblical);
			var narrator = GetStandardCharacterId("PSA", StandardCharacter.Narrator);

			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(39));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(13));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(13));
			Assert.That(blocks[i].CharacterId, Is.EqualTo("David"));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(39));

			Assert.That(blocks[++i].ChapterNumber, Is.EqualTo(40));
			Assert.That(blocks[i].IsChapterAnnouncement);
			Assert.That(blocks[i].CharacterId, Is.EqualTo(chapterCharacter));

			if (includeSectionHead)
			{
				Assert.That(blocks[++i].StyleTag, Is.EqualTo("s"));
				Assert.That(blocks[i].CharacterId, Is.EqualTo(sectionHeadCharacter));
				Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
				Assert.That(blocks[i].GetText(true, true), Is.EqualTo("God Sustains His Servant"));
			}

			Assert.That(blocks[++i].StyleTag, Is.EqualTo("d"));
			Assert.That(blocks[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(40));
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(blocks[i].GetText(true, true), Is.EqualTo("For the music director. A Psalm of David."));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(40));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(blocks[++i].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[i].ChapterNumber, Is.EqualTo(40));
			Assert.That(blocks[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(blocks[i].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[i].CharacterId, Is.Null);
			Assert.That(blocks[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(++i, Is.EqualTo(blocks.Count));
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

		private static void VerifyQuoteEnd(Block block, string id = null)
		{
			Assert.That(block.IsPredeterminedFirstLevelQuoteEnd);
			var quoteIdAnnotation = (QuoteId)block.BlockElements.Last();
			Assert.That(quoteIdAnnotation.Id, Is.EqualTo(id));
			Assert.That(quoteIdAnnotation.Start, Is.False);
			Assert.That(quoteIdAnnotation.IsNarrator, Is.False);
		}
		#endregion PG-1419 - Quote milestones

		[Test]
		public void Parse_DescriptiveTitleUsedOutsidePsalms_CharacterSetToNarrator()
		{
			Assert.That(StyleToCharacterMappings.TryGetCharacterForParaStyle("d", "MRK", out var character), Is.True,
				$"Setup condition not met: marker \"d\" should be included in {nameof(StyleToCharacterMappings)}.");
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"d\">INTRODUCTION TO MARK</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2),
				"Should have a chapter block, plus the descriptive title block.");
			Assert.That(blocks[1].CharacterId, Is.EqualTo(character));
		}

		[Test]
		public void Parse_ParaStartsWithVerseNumber_BlocksGetCorrectChapterAndVerseNumbers()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"12\" style=\"v\" />" +
				@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,</para>" +
				"<para style=\"p\">" +
				"<verse number=\"13\" style=\"v\" />" +
				@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,"));

			Assert.That(blocks[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(13));
			Assert.That(blocks[2].GetText(false), Is.EqualTo("Ka nino okato manok, Yecu dok " +
				"odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
		}

		[Test]
		public void Parse_VerseRange_BlocksGetCorrectStartingVerseNumber()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"12-14\" style=\"v\" />" +
				@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,</para>" +
				"<para style=\"p\">" +
				"<verse number=\"15-18\" style=\"v\" />" +
				@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(12));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,"));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{12-14}\u00A0" +
				@"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa,"));

			Assert.That(blocks[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(blocks[2].GetText(false), Is.EqualTo(
				"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("{15-18}\u00A0" +
				@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
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
			Assert.That(blocks.Count, Is.EqualTo(4));
			Assert.That(blocks[0].ChapterNumber, Is.EqualTo(0));
			Assert.That(blocks[0].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[0].CharacterIs("MRK", StandardCharacter.Intro), Is.True);
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(0));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].CharacterIs("MRK", StandardCharacter.Intro), Is.True);
			VerifyChapterBlock(blocks[2], 1);
			Assert.That(blocks[3].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[3].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[3].CharacterId, Is.EqualTo(Block.kNotSet));
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
				@"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].StyleTag, Is.EqualTo("s"));
			Assert.That(blocks[1].CharacterIs("MRK", StandardCharacter.ExtraBiblical), Is.True);
			Assert.That(blocks[1].GetText(false), Is.EqualTo("John the Baptist prepares the way"));

			Assert.That(blocks[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[2].CharacterId, Is.EqualTo(Block.kNotSet));
			Assert.That(blocks[2].GetText(false), Is.EqualTo(
				"Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
		}

		[Test]
		public void Parse_UnpublishableText_NonpublishableDataExcluded()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1-2\" style=\"v\" />" +
				@"Acakki me lok me kwena maber i kom Yecu Kricito" +
				"<char style=\"pro\">Crissitu</char>" +
				@", Wod pa Lubaŋa, kit ma gicoyo kwede i buk pa lanebi Icaya ni,</para>" +
				"<para style=\"rem\">" +
				"Tom was here!</para>" +
				"<para style=\"q1\">" +
				@"“Nen, acwalo lakwenana otelo nyimi,</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[1].CharacterId, Is.EqualTo(Block.kNotSet));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1-2}\u00A0" +
				"Acakki me lok me kwena maber i kom Yecu Kricito, Wod pa Lubaŋa, kit ma gicoyo " +
				"kwede i buk pa lanebi Icaya ni,"));

			Assert.That(blocks[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[2].CharacterId, Is.EqualTo(Block.kNotSet));
			Assert.That(blocks[2].GetText(true),
				Is.EqualTo(@"“Nen, acwalo lakwenana otelo nyimi,"));
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
				@"Acakki me lok me kwena maber i kom Yecu Kricito</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[0].StyleTag, Is.EqualTo("mt"));
			Assert.That(blocks[0].GetText(false), Is.EqualTo("The Gospel According to Mark"));
			Assert.That(blocks[0].GetText(true), Is.EqualTo("The Gospel According to Mark"));
			Assert.That(parser.PageHeader, Is.EqualTo("header"));
			Assert.That(parser.MainTitle, Is.EqualTo("Mark"));
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
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[0].StyleTag, Is.EqualTo("mt"));
			Assert.That(blocks[0].GetText(false), Is.EqualTo("The Gospel According to Mark"));
			Assert.That(blocks[0].GetText(true), Is.EqualTo("The Gospel According to Mark"));
			Assert.That(parser.PageHeader, Is.EqualTo("header"));
			Assert.That(parser.MainTitle, Is.EqualTo("Mark"));
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
			Assert.That(blocks.Count, Is.EqualTo(1));
			Assert.That(blocks[0].StyleTag, Is.EqualTo("mt"));
			Assert.That(blocks[0].GetText(false), Is.EqualTo(@"The Gospel According to Markus"));
			Assert.That(blocks[0].GetText(true), Is.EqualTo(@"The Gospel According to Markus"));
			Assert.That(parser.PageHeader, Is.EqualTo("Marco"));
			Assert.That(parser.MainTitle, Is.EqualTo("Markus"));
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
			Assert.That(books.Count, Is.EqualTo(0));
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
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[0].IsParagraphStart, Is.True); //chapter
			Assert.That(blocks[1].IsParagraphStart, Is.True);
			Assert.That(blocks[2].IsParagraphStart, Is.True);
		}

		[Test]
		public void Parse_VerseBridge_SingleParagraph()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1-3\" style=\"v\" />" +
				"Verse 1-3 text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialEndVerseNumber, Is.EqualTo(3));
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
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[2].InitialEndVerseNumber, Is.EqualTo(3));
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
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1-3}\u00A0Verse 1-3 text"));
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
			Assert.That(blocks.Count, Is.EqualTo(2),
				"Should only have chapter number block and v. 4 block. " +
				"Empty verse bridge block should have been discarded.");
			Assert.That(blocks[0].CharacterIs("MRK", StandardCharacter.BookOrChapter), Is.True);
			Assert.That(blocks[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(blocks[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{4}\u00A0Verse 4 text"));
		}

		[TestCase("-", 3)]
		[TestCase("-", 2)]
		public void Parse_VerseConsistsOfDashButFollowingVerseLessThanOrEqualToPrevNumber_DiscardedAsEmptyVerse(
			string dash, int followingVerseNum)
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"3\" style=\"v\" />" + dash + // This one should get discarded
				$"<verse number=\"{followingVerseNum}\" style=\"v\" />" +
				"Verse text</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].InitialStartVerseNumber, Is.EqualTo(followingVerseNum));
			Assert.That(blocks[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"{" + followingVerseNum + "}\u00A0Verse text"));
		}

		[Test]
		public void Parse_NodeWithNoChildren_IgnoresNode()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc("<para style=\"p\">" +
				"<verse number=\"1\" style=\"v\" />এই হল যীশু খ্রীষ্টের বংশ তালিকা৷ ইনি ছিলেন রাজা " +
				"দায়ূদের বংশধর, দায়ূদ ছিলেন অব্রাহামের বংশধর৷</para>" +
				"<para style=\"b\" />" +
				"<para style=\"li\">" +
				"<verse number=\"2\" style=\"v\" />অব্রাহামের ছেলে ইসহাক৷</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[0].StyleTag, Is.EqualTo("c"));
			Assert.That(blocks[1].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("li"));
		}

		[Test]
		public void Parse_WhitespaceBetweenCharAndNoteElements_WhitespaceIsIgnored()
		{
			// World English Bible, MAT 5:27, PG-593
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"27\" style=\"v\" />\r\n" +
				"	<char style=\"wj\">“You have heard that it was said, </char>\r\n" +
				"	<note caller=\"+\" style=\"f\">TR adds “to the ancients”.</note> " +
				"<char style=\"wj\">‘You shall not commit adultery;’</char><note caller=\"+\" " +
				"style=\"x\">Exodus 20:14</note> <verse number=\"28\" style=\"v\" />" +
				"<char style=\"wj\">but I tell you that everyone who gazes at a woman to lust " +
				"after her has committed adultery with her already in his heart. </char> " +
				"<verse number=\"29\" style=\"v\" /><char style=\"wj\">If your right eye causes " +
				"you to stumble, pluck it out and throw it away from you. For it is more " +
				"profitable for you that one of your members should perish, than for your " +
				"whole body to be cast into Gehenna.</char><note caller=\"+\" style=\"f\">or, " +
				"Hell</note> <verse number=\"30\" style=\"v\" /><char style=\"wj\">If your " +
				"right hand causes you to stumble, cut it off, and throw it away from you. " +
				"For it is more profitable for you that one of your members should perish, " +
				"than for your whole body to be cast into Gehenna.</char><note caller=\"+\" " +
				"style=\"f\">or, Hell</note></para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(8));
			Assert.That(blocks[1].GetText(false), Is.EqualTo(
				"“You have heard that it was said, ‘You shall not commit adultery;’ but I " +
				"tell you that everyone who gazes at a woman to lust after her has committed " +
				"adultery with her already in his heart. If your right eye causes you to " +
				"stumble, pluck it out and throw it away from you. For it is more profitable " +
				"for you that one of your members should perish, than for your whole body to be " +
				"cast into Gehenna. If your right hand causes you to stumble, cut it off, and " +
				"throw it away from you. For it is more profitable for you that one of your " +
				"members should perish, than for your whole body to be cast into Gehenna."));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"{27}\u00A0“You have heard that it was said, ‘You shall not commit adultery;’ " +
				"{28}\u00A0but I tell you that everyone who gazes at a woman to lust after her " +
				"has committed adultery with her already in his heart. " +
				"{29}\u00A0If your right eye causes you to stumble, pluck it out and throw it " +
				"away from you. For it is more profitable for you that one of your members " +
				"should perish, than for your whole body to be cast into Gehenna. " +
				"{30}\u00A0If your right hand causes you to stumble, cut it off, and throw it " +
				"away from you. For it is more profitable for you that one of your members " +
				"should perish, than for your whole body to be cast into Gehenna."));
		}

		private List<Block> ParseLuke17Data(string paragraphData)
		{
			var usxFrame = "<?xml version=\"1.0\" encoding=\"utf-8\"?><usx version=\"2.0\"><book code=\"LUK\" style=\"id\">Some Bible</book><chapter number=\"17\" style=\"c\" />{0}</usx>";
			var doc = UsxDocumentTests.CreateDocFromString(string.Format(usxFrame, paragraphData));
			var parser = GetUsxParser(doc, "LUK");
			return parser.Parse().ToList();
		}

		[Test]
		public void Parse_VerseAtEndOfParagraphConsistsEntirelyOfNote_DoNotIncludeVerseNumber()
		{
			// World English Bible, LUK 17:36
			const string data = "  <para style=\"p\">\r\n" +
				"	<verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.”</char> <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left.” "));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("{37}\u00A0They, answering, asked him, “Where, Lord?”"));
		}

		// This test is an attempt to future-proof Glyssen when the DBL spec changes to support USX 3 (without us getting a heads-up).
		[Test]
		public void Parse_VerseElementWithEid_EndVerseElementIgnored()
		{
			var data = "  <para style=\"p\">\r\n" +
				"	<verse number=\"35\" style=\"v\" sid=\"LUK 17:35\" />There will be two grinding grain together. One will be taken and the other will be left. <verse eid=\"LUK 17:35\" /><verse number=\"36\" style=\"v\" sid=\"LUK 17:36\" />Two will be in the field: the one taken, and the other left, Jesus concluded.<verse eid=\"LUK 17:36\" /></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"37\" style=\"v\" sid=\"LUK 17:37\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(4));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{35}\u00A0There will be two " +
				"grinding grain together. One will be taken and the other will be left. " +
				"{36}\u00A0Two will be in the field: the one taken, and the other left, " +
				"Jesus concluded."));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[2].GetText(true), Is.EqualTo(
				"{37}\u00A0They, answering, asked him, “Where, Lord?”"));
			Assert.That(blocks[2].CharacterId, Is.Null);
			Assert.That(blocks[2].StyleTag, Is.EqualTo("p"));
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
				@"\v 25 Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì.\f + \fr 16:25 \ft Ɓalli gbal aɓa: \xt Mat 10:39; Luk 17:33; Yoh 12:25\xt*.\f*";

			// This uses the "real" stylesheet (now USFM v. 3)
			var doc = UsfmToUsx.ConvertToXmlDocument(null, SfmLoader.GetUsfmScrStylesheet(), usfmData);
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), null, new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(6));
			Assert.That(blocks[1].GetText(true), Is.EqualTo(
				"{23}\u00A0Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, Nyaram anggo, Shetan! " +
				"{24}\u00A0Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga. " +
				"{25}\u00A0Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì."));
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
			var doc = UsfmToUsx.ConvertToXmlDocument(null, SfmLoader.GetUsfmScrStylesheet(), usfmData);
			var parser = new UsxParser("MAT", SfmLoader.GetUsfmStylesheet(), null, new UsxDocument(doc).GetChaptersAndParas());
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(true),
				Is.EqualTo("{23}\u00A0Yesu pǝlǝa arǝ Bitǝrus sǝ ne wi ama, "));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(5));
			Assert.That(blocks[2].GetText(true), Is.EqualTo(
				"Nyaram anggo, Shetan! " +
				"{24}\u00A0Ɓwa mǝnana kat earce ama nǝ̀ yiu atam nǝ̀ duk mǝkpatam ngga. " +
				"{25}\u00A0Ɓwa mana kat kǝ sǝni nǝ̀ amsǝ yilǝmi ka nǝ̀ ngga ɗwanyi banì."));
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
				$"	<verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.{trailingSpaceInsideChar}</char>{trailingSpaceOutsideChar}<verse number=\"36\" style=\"v\" sid=\"LUK 17:36\" /><char style=\"wj\">Two will be in the field: the one taken, and the other left,{trailingSpaceInsideChar}</char>{trailingSpaceOutsideChar}Jesus concluded.</para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>";

			var blocks = ParseLuke17Data(data);
			Assert.That(blocks.Count, Is.EqualTo(4));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(4));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{35}\u00A0There will be two grinding grain together. One will be taken and the other will be left. {36}\u00A0Two will be in the field: the one taken, and the other left, "));
			Assert.That(blocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[1].StyleTag, Is.EqualTo("wj"));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(1));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("Jesus concluded."));
			Assert.That(blocks[2].CharacterId, Is.Null);
			Assert.That(blocks[2].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[3].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("{37}\u00A0They, answering, asked him, “Where, Lord?”"));
			Assert.That(blocks[3].CharacterId, Is.Null);
			Assert.That(blocks[3].StyleTag, Is.EqualTo("p"));
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
				"	<verse number=\"7\" style=\"v\" />And he preached: <char style=\"wj\">Someone is coming who is > I, the thong of whose sandals I am unworthy to untie.</char></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"8\" style=\"v\" /><char style=\"wj\">I immerse you in H2O, but he will plunge you into life with God's 'Holy Spirit.</char></para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(4));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{7}\u00A0And he preached: "));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[1].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(1));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("Someone is coming who is > I, the thong of whose sandals I am unworthy to untie."));
			Assert.That(blocks[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("wj"));
			Assert.That(blocks[3].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("{8}\u00A0I immerse you in H2O, but he will plunge you into life with God's 'Holy Spirit."));
			Assert.That(blocks[3].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(blocks[3].StyleTag, Is.EqualTo("wj"));
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
			Assert.That(blocks.Count, Is.EqualTo(5));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(4));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{1}\u00A0The start of the gospel of Jesus Christ, God's Son'. {2}\u00A0In the words of Isaiah: "));
			Assert.That(blocks[1].CharacterId, Is.Null);
			Assert.That(blocks[1].StyleTag, Is.EqualTo("p"));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(3));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("I send my messenger ahead of you to prepare the way, {3}\u00A0shouting in the wild:"));
			Assert.That(blocks[2].CharacterId, Is.EqualTo("scripture"));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("qt"));
			Assert.That(blocks[3].BlockElements.Count, Is.EqualTo(1));
			Assert.That(blocks[3].GetText(true), Is.EqualTo("“Prepare the way for the Lord,"));
			Assert.That(blocks[3].CharacterId, Is.Null);
			Assert.That(blocks[3].StyleTag, Is.EqualTo("q1"));
			Assert.That(blocks[4].BlockElements.Count, Is.EqualTo(1));
			Assert.That(blocks[4].GetText(true), Is.EqualTo("make straight paths for him.”"));
			Assert.That(blocks[4].CharacterId, Is.Null);
			Assert.That(blocks[4].StyleTag, Is.EqualTo("q2"));
		}
		#endregion // PG-1272 Tests

		[Test]
		public void Parse_OnlyVerseInParagraphConsistsEntirelyOfNote_DoNotIncludeParagraph()
		{
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note></para>\r\n" +
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(2));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{37}\u00A0They, answering, asked him, “Where, Lord?”"));
		}

		[Test]
		public void Parse_VerseMidParagraphConsistsEntirelyOfNote_DoNotIncludeVerseNumber()
		{
			// World English Bible, LUK 17:36, PG-594
			var doc = UsxDocumentTests.CreateMarkOneDoc(
				"  <para style=\"p\">\r\n" +
				"	<verse number=\"35\" style=\"v\" /><char style=\"wj\">There will be two grinding grain together. One will be taken and the other will be left.”</char> <verse number=\"36\" style=\"v\" /><note caller=\"+\" style=\"f\">Some Greek manuscripts add: “Two will be in the field: the one taken, and the other left.”</note> <verse number=\"37\" style=\"v\" />They, answering, asked him, “Where, Lord?”</para>");
			var parser = GetUsxParser(doc);
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(3));
			Assert.That(blocks[1].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[1].GetText(true), Is.EqualTo("{35}\u00A0There will be two " +
				"grinding grain together. One will be taken and the other will be left.” "));
			Assert.That(blocks[2].BlockElements.Count, Is.EqualTo(2));
			Assert.That(blocks[2].GetText(true), Is.EqualTo(
				"{37}\u00A0They, answering, asked him, “Where, Lord?”"));
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
				"<verse number=\"1\" style=\"v\"/>" +
				@"Bienaventurados los perfectos de camino.</para>" +
				UsxDocumentTests.kUsxFrameEnd);
			var parser = GetUsxParser(doc, "PSA");
			var blocks = parser.Parse().ToList();
			Assert.That(blocks.Count, Is.EqualTo(4));
			Assert.That(blocks[2].GetText(true), Is.EqualTo("Alef"));
			Assert.That(blocks[2].StyleTag, Is.EqualTo("qa"));
			Assert.That(blocks[2].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(blocks[2].CharacterId,
				Is.EqualTo(GetStandardCharacterId("PSA", StandardCharacter.BookOrChapter)));
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
			Assert.That(block.StyleTag, Is.EqualTo(tag));
			Assert.That(block.IsChapterAnnouncement);
			Assert.That(block.BookCode, Is.EqualTo(bookId));
			Assert.That(block.ChapterNumber, Is.EqualTo(number));
			Assert.That(block.InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(block.GetText(true), Is.EqualTo(text ?? number.ToString()));
			Assert.That(block.CharacterId, Is.EqualTo($"BC-{bookId}"));
			Assert.That(block.IsParagraphStart, Is.True);
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

		public string FontFamily => null;
		public int FontSizeInPoints => 10;
	}
}
