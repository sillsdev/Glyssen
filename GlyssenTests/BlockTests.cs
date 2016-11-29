using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Scripture;
using SIL.Xml;
using GlyssenTests.Properties;
using Paratext;
using Rhino.Mocks;
using SIL.IO;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests
{
	[TestFixture]
	class BlockTests
	{
		private ScrVers m_testVersification;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Load(tempFile.Path);
			}
		}

		[SetUp]
		public void Setup()
		{
			Block.FormatChapterAnnouncement = null;
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
		}

		[Test]
		public void GetText_GetBookNameNull_ChapterBlockTextBasedOnStoredText()
		{
			var block = new Block("c", 4);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.AreEqual("Chapter 4", block.GetText(true));
			Assert.AreEqual("Chapter 4", block.GetText(false));
		}

		[TestCase("c")]
		[TestCase("cl")]
		public void GetText_FormatChapterAnnouncementSet_ChapterBlockTextBasedOnOverride(string chapterStyleTag)
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => chapterNum + (bookId == "MRK" ? " Marky" : " Unknown");
			var block = new Block(chapterStyleTag, 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.AreEqual("4 Marky", block.GetText(true));
			Assert.AreEqual("4 Marky", block.GetText(false));

			block = new Block(chapterStyleTag, 1) { BookCode = "LUK" };
			block.SetStandardCharacter("LUK", CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 1"));

			Assert.AreEqual("1 Unknown", block.GetText(true));
			Assert.AreEqual("1 Unknown", block.GetText(false));
		}

		[TestCase("c")]
		[TestCase("cl")]
		public void GetText_FormatChapterAnnouncementSetButBookCodeNotSet_ChapterBlockTextBasedOnStoredText(string chapterStyleTag)
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => (bookId == null) ? "ARGHHHH!" : "Marky " + chapterNum;
			var block = new Block(chapterStyleTag, 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.AreEqual("Marky 4", block.GetText(false));
			block.BookCode = null;
			Assert.AreEqual("Chapter 4", block.GetText(false));
		}

		[Test]
		public void GetText_FormatChapterAnnouncementReturnsNull_ChapterBlockTextBasedOnStoredText()
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => null;
			var block = new Block("c", 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.AreEqual("Chapter 4", block.GetText(false));
		}

		[Test]
		public void GetText_BlockContainsAnnotation_IncludeAnnotationsFalse_ReturnsAllText()
		{
			const string text1 = "text1 ";
			const string text2 = "text2 ";
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText(text1));
			block.BlockElements.Add(new Sound { EffectName = "effect name", UserSpecifiesLocation = true });
			block.BlockElements.Add(new ScriptText(text2));

			Assert.AreEqual(text1 + text2, block.GetText(true));
		}

		[Test]
		public void GetText_BlockContainsAnnotation_IncludeAnnotationsTrue_ReturnsAllTextIncludingAnnotation()
		{
			const string text1 = "text1 ";
			const string text2 = "text2 ";
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText(text1));
			block.BlockElements.Add(new Sound { SoundType = SoundType.Sfx, EffectName = "effect name", UserSpecifiesLocation = true });
			block.BlockElements.Add(new ScriptText(text2));

			Assert.AreEqual(text1 + "{F8 SFX--effect name} " + text2, block.GetText(true, true));
		}

		[Test]
		public void GetAsXml_VerseAndTextElements_XmlHasCorrectAttributesAndAlternatingVerseAndTextElements()
		{
			var block = new Block("p", 4);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" chapter=\"4\" initialStartVerse=\"1\" characterId=\"narrator-MRK\">" +
				"<verse num=\"1\"/>" +
				"<text>Text of verse one. </text>" +
				"<verse num=\"2\"/>" +
				"<text>Text of verse two.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetAsXml_TextBeginsMidVerse_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" paragraphStart=\"true\" chapter=\"4\" initialStartVerse=\"3\">" +
				"<text>Text of verse three, part two. </text>" +
				"<verse num=\"4\"/>" +
				"<text>Text of verse four. </text>" +
				"<verse num=\"5\"/>" +
				"<text>Text of verse five.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetAsXml_VerseBridge_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3, 5);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4-5"));
			block.BlockElements.Add(new ScriptText("Text of verse four and five."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" paragraphStart=\"true\" chapter=\"4\" initialStartVerse=\"3\" initialEndVerse=\"5\">" +
				"<text>Text of verse three, part two. </text>" +
				"<verse num=\"4-5\"/>" +
				"<text>Text of verse four and five.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetTextAsHtml_ContainsCharactersWhichNeedToBeEscaped()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText(@"The dog'cat says, <<Woof!>> & ""Meow."""));

			const string expected = "<div id=\"3\" class=\"scripttext\">The dog&#39;cat says, &lt;&lt;Woof!&gt;&gt; &amp; &quot;Meow.&quot;</div>";
			var actual = block.GetTextAsHtml(true, false);

			Assert.AreEqual(expected, actual);
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		public void GetTextAsHtml_TextContainsSquareBrackets_OnlyVerseNumbersAreSuperscripted(string open, string close)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two " + open + "2" + close + ". "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers " + open + "sic" + close + " four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			string expect1 = ">Text of verse three, part two " + open + "2" + close + ". <";
			const string expect2 = "<sup>4&#160;</sup>";
			string expect3 = ">Text of vers " + open + "sic" + close + " four. <";
			const string expect4 = "<sup>5&#160;</sup>";
			const string expect5 = ">Text of verse five.<";
			var actual = block.GetTextAsHtml(true, false);

			Assert.IsTrue(actual.Contains(expect1), string.Format("The output string did not contain: {0}", expect1));
			Assert.IsTrue(actual.Contains(expect2), string.Format("The output string did not contain: {0}", expect2));
			Assert.IsTrue(actual.Contains(expect3), string.Format("The output string did not contain: {0}", expect3));
			Assert.IsTrue(actual.Contains(expect4), string.Format("The output string did not contain: {0}", expect4));
			Assert.IsTrue(actual.Contains(expect5), string.Format("The output string did not contain: {0}", expect5));
		}

		[Test]
		public void GetTextAsHtml_RightToLeftScript_RtlMarkersAddedCorrectly()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));

			const string expected = "<sup>&rlm;4&#160;&rlm;</sup>";
			var actual = block.GetTextAsHtml(true, true);

			Assert.IsTrue(actual.Contains(expected), string.Format("The output string did not contain: {0}", expected));
		}

		[Test]
		public void GetSplitTextAsHtml_OffsetTooHigh_ThrowsArgumentOutOfRangeException()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text"));
			Assert.Throws<ArgumentOutOfRangeException>(
				() => block.GetSplitTextAsHtml(0, false, new[] {new BlockSplitData(1, block, "3", 5)}, false));
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		public void GetSplitTextAsHtml_BlockSplitProvided_InsertsBlockSplit(string open, string close)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two " + open + "2" + close + ". "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers " + open + "sic" + close + " four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			var expected = Block.BuildSplitLineHtml(1);
			var actual = block.GetSplitTextAsHtml(0, false, new[] { new BlockSplitData(1, block, "4", 5) }, false);

			Assert.IsTrue(actual.Contains(expected), string.Format("The output string did not contain: {0}", expected));
		}

		[TestCase("[", "]")]
		[TestCase("{", "}")]
		public void GetSplitTextAsHtml_MultipleBlockSplitsProvided_InsertsBlockSplits(string open, string close)
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two " + open + "2" + close + ". "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers " + open + "sic" + close + " four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Text of verse three, part two " + open + "2" + close + ". </div>" + 
				            "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Text </div>" +
							Block.BuildSplitLineHtml(1) + 
							"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">of </div>" +
							Block.BuildSplitLineHtml(2) +
							"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">vers " + open + "sic" + close + " </div>" +
							Block.BuildSplitLineHtml(3) + 
							"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">four. </div>" + 
							"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"5\"><sup>5&#160;</sup>Text</div>" +
							Block.BuildSplitLineHtml(4) + 
							"<div class=\"splittext\" data-blockid=\"0\" data-verse=\"5\"> of verse five.</div>";

			var actual = block.GetSplitTextAsHtml(0, false, new[]
			{
				new BlockSplitData(1, block, "4", 5),
				new BlockSplitData(2, block, "4", 8),
				new BlockSplitData(3, block, "4", 19),
				new BlockSplitData(4, block, "5", 4)
			}, false);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_MultipleBlockSplitsProvided_InsertsBlockSplits_TODO_Name_realdata()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("—Ananías, ¿Ibiga nia-saila-Satanás burba-isgana begi oubononiki? Emide, Bab-Dummad-Burba-Isligwaledga be gakansanonigu, mani-abala be susgu. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Yoo be nainu-ukegu, ¿nainu begadinsursi? Nainu be uksagu, ¿a-manide begadinsursi? ¿Ibiga be-gwagegi anmarga gakansaedgi be binsanoniki? Dulemarga be gakan-imaksasulid, Bab-Dummadga be gakan-imaksad."));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">—Anan&#237;as, &#191;Ibiga nia-saila-Satan&#225;s burba-isgana begi oubononiki? Emide, Bab-Dummad-Burba-Isligwaledga be gakans</div>"+
						   Block.BuildSplitLineHtml(1) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">anonigu, mani-abala be susgu. </div>" +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Yoo be nainu-ukegu, &#191;nainu begadin</div>" +
						   Block.BuildSplitLineHtml(2) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">sursi? </div>" +
						   Block.BuildSplitLineHtml(4) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">Nainu be uksagu, &#191;a-manide begadinsursi? &#191;Ibiga be-gwagegi anmarga gakan</div>" +
						   Block.BuildSplitLineHtml(3) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\">saedgi be binsanoniki? Dulemarga be gakan-imaksasulid, Bab-Dummadga be gakan-imaksad.</div>";

			var actual = block.GetSplitTextAsHtml(0, false, new[]
			{
				new BlockSplitData(1, block, "3", 111),
				new BlockSplitData(2, block, "4", 34),
				new BlockSplitData(3, block, "4", 113),
				new BlockSplitData(4, block, "4", 41)
			}, false);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_SpecialCharactersInText_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Нылыс эз кув, сiй&#246; </div>" +
						   Block.BuildSplitLineHtml(1) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">узь&#246;</div>";

			var actual = block.GetSplitTextAsHtml(0, false, new[] { new BlockSplitData(1, block, "3", 19) }, false);

			Assert.AreEqual( expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_SpecialCharactersInTextWithSplitJustBeforeVerseNumber_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));

			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">Ны</div>" +
						   Block.BuildSplitLineHtml(1) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">лыс эз кув, сiй&#246; узь&#246;</div>" +
						   Block.BuildSplitLineHtml(2) +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\"></div>" +
						   "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"4\"><sup>4&#160;</sup>Нылыс эз кув, сiй&#246; узь&#246;</div>";

			var actual = block.GetSplitTextAsHtml(0, false, new[]
			{
				new BlockSplitData(2, block, "3", BookScript.kSplitAtEndOfVerse),
				new BlockSplitData(1, block, "3", 2),
			}, false);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSplitTextAsHtml_ExpectedSpecialCharacters_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("A & <<B>> C"));
			var expected = "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">A &amp; &lt;&lt;B&gt;&gt; </div>" + Block.BuildSplitLineHtml(1) + "<div class=\"splittext\" data-blockid=\"0\" data-verse=\"3\">C</div>";
			var actual = block.GetSplitTextAsHtml(0, false, new[] {new BlockSplitData(1, block, "3", 10)}, false);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SetCharacterAndDelivery_SingleCharacter_SetsCharacterAndDelivery()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { JesusQuestioning });
			Assert.AreEqual("Jesus", block.CharacterId);
			Assert.AreEqual("Questioning", block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_NoCharacters_SetsCharacterToUnknown()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(new CharacterVerse[0]);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, block.CharacterId);
			Assert.IsNull(block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_MultipleCharacters_SetsCharacterToAmbiguous()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(new [] { JesusCommanding, JesusQuestioning, Andrew });
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, block.CharacterId);
			Assert.IsNull(block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_SingleMultipleChoiceCharacter_SetsCharacterAndCharacterIdInScript()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Mary", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SettingToSameMultiCharacterWithNonDefaultCharacterIdInScriptSet_CharacterIdInScriptNotChanged()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Mary/Martha";
			block.CharacterIdInScript = "Martha";
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Martha", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SettingToSameMultiCharacterWithNoCharacterIdInScriptSet_CharacterIdInScriptGetsSet()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Mary/Martha";
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Mary", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SameMultipleChoiceCharacterWithDifferentDefaults_CharacterIdInScriptSetBasedOnFirstCvEntry()
		{
			var block = new Block("p", 4, 4, 6);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verses four through seven. "));
			block.SetCharacterAndDelivery(new[]
			{
				new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Martha"),
				new CharacterVerse(new BCVRef(41, 4, 5), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Jews"),
				new CharacterVerse(new BCVRef(41, 4, 6), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Mary")
			});
			Assert.AreEqual("Mary/Martha/Jews", block.CharacterId);
			Assert.AreEqual("Martha", block.CharacterIdInScript);
		}

		[Test]
		public void IsStandardCharacter_BiblicalCharacter_ReturnsFalse()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { JesusQuestioning });
			Assert.IsFalse(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_Narrator_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_ExtraBiblical_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("GEN", CharacterVerseData.StandardCharacter.ExtraBiblical);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_BookOrChapter_ReturnsTrue()
		{
			var block = new Block("c", 4);
			block.BlockElements.Add(new ScriptText("4"));
			block.SetStandardCharacter("REV", CharacterVerseData.StandardCharacter.BookOrChapter);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_Intro_ReturnsTrue()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.SetStandardCharacter("ROM", CharacterVerseData.StandardCharacter.Intro);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void LastVerseNum_Intro_ReturnsZero()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(0, block.LastVerseNum);
		}

		[Test]
		public void LastVerseNum_ScriptureBlockWithSingleStartVerseAndNoVerseElements_ReturnsInitialStartVerse()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(15, block.LastVerseNum);
		}

		[Test]
		public void LastVerseNum_ScriptureBlockStartingWithVerseBridgeAndNoVerseElements_ReturnsInitialEndVerse()
		{
			var block = new Block("ip", 3, 15, 17);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(17, block.LastVerseNum);
		}

		[Test]
		public void LastVerseNum_ScriptureBlockWithVerseElements_ReturnsEndVerseFromBlockElement()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("16"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("17"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(17, block.LastVerseNum);
		}

		[Test]
		public void LastVerseNum_ScriptureBlockWithVerseElementContainingBridge_ReturnsEndVerseFromBlockElement()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("16"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("17-19"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(19, block.LastVerseNum);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NotMultipleChoice_DoNothing()
		{
			var block = new Block("p", 4, 38);
			block.CharacterId = "disciples";
			block.CharacterIdInScript = "Peter (Simon)";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.AreEqual("Peter (Simon)", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NoExplicitDefault_UseFirst()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_ExplicitDefault_UseDefault()
		{
			var block = new Block("p", 9, 11);
			block.CharacterId = "Peter (Simon)/James/John";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.AreEqual("John", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_AlreadySetToAnotherValue_OverwriteWithDefault()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "chief baker";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer", block.CharacterIdInScript);
		}

		[TestCase(CharacterVerseData.kAmbiguousCharacter)]
		[TestCase(CharacterVerseData.kUnknownCharacter)]
		public void SetCharacterAndCharacterIdInScript_CharacterIdSetUnclear_CharacterIdInScriptSetToNull(string unclearCharacterId)
		{
			var block = new Block("p", 40, 8);
			block.CharacterIdInScript = "chief monkey";
			block.CharacterId = "chief monkey";
			Assert.AreEqual("chief monkey", block.CharacterId);
			Assert.AreEqual("chief monkey", block.CharacterIdInScript);
			Assert.AreEqual("chief monkey", block.CharacterIdOverrideForScript);
			// end setup

			block.SetCharacterAndCharacterIdInScript(unclearCharacterId, BCVRef.BookToNumber("EXO"));
			Assert.AreEqual(unclearCharacterId, block.CharacterId);
			Assert.AreEqual(unclearCharacterId, block.CharacterIdInScript);
			Assert.IsNull(block.CharacterIdOverrideForScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoice_CharacterIdInScriptRemainsNull()
		{
			var block = new Block("p", 40, 8);
			block.SetCharacterAndCharacterIdInScript("chief monkey", BCVRef.BookToNumber("EXO"));
			Assert.AreEqual("chief monkey", block.CharacterId);
			Assert.AreEqual("chief monkey", block.CharacterIdInScript);
			Assert.IsNull(block.CharacterIdOverrideForScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoiceCharacterIdInScriptAlreadySet_CharacterIdInScriUnchanged()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "live frog";
			block.CharacterIdInScript = "dead frog";
			block.SetCharacterAndCharacterIdInScript("subordinate monkey", BCVRef.BookToNumber("REV"));
			Assert.AreEqual("subordinate monkey", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NoChangeToCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_NoChange()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "dead frog";
			block.SetCharacterAndCharacterIdInScript("chief cupbearer/chief baker", BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer/chief baker", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ControlFileHasOverriddenDefault_VersificationShift_CharacterIdInScriptBasedOnOverride()
		{
			// MRK 9:10 in the Vulgate should translate to 9:11 in the "original"
			// The control file overrides the default speaker in MRK 9:11 to be John.
			var block = new Block("p", 9, 10);
			block.SetCharacterAndCharacterIdInScript("Peter (Simon)/James/John", BCVRef.BookToNumber("MRK"), m_testVersification);
			Assert.AreEqual("Peter (Simon)/James/John", block.CharacterId);
			Assert.AreEqual("John", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ChangeCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_CharacterIdInScriptChanged()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "chief cupbearer";
			block.SetCharacterAndCharacterIdInScript("David/Goliath", BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("David/Goliath", block.CharacterId);
			Assert.AreEqual("David", block.CharacterIdInScript);
		}

		[Test]
		public void SerializeDeserialize_ContainsScriptAnnotations_RoundtripDataRemainsTheSame()
		{
			var block = new Block();
			block.BlockElements = new List<BlockElement>
			{
				new ScriptText("script text"),
				new Sound { SoundType = SoundType.Sfx, EffectName = "effect name", StartVerse = 2 },
				new Verse("2"),
				new ScriptText("script text 2"),
			};

			var blockBefore = block.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(block);
			AssertThatXmlIn.String(xmlString).HasSpecifiedNumberOfMatchesForXpath("/block/sound", 1);
			var blockAfter = XmlSerializationHelper.DeserializeFromString<Block>(xmlString);
			Assert.AreEqual(blockBefore.GetText(true, true), blockAfter.GetText(true, true));
		}

		/// <summary>
		/// Note that this tests more deeply recursive nesting than we actually expect to have in Glyssen:
		/// Vernacular (Spanish) backed by French, backed by Portuguese, backed by English.
		/// </summary>
		[Test]
		public void SetMatchedReferenceBlock_NestedBackingReferenceTextBlocks_EmptyRefBlockCreatedAtAllNestingLevels()
		{
			var vernBlock = ReferenceTextTests.CreateNarratorBlockForVerse(2, "dijo.");
			var rtFrench = MockRepository.GenerateMock<IReferenceLanguageInfo>();
			var rtPortuguese = MockRepository.GenerateMock<IReferenceLanguageInfo>();
			var rtEnglish = MockRepository.GenerateMock<IReferenceLanguageInfo>();
			rtFrench.Expect(r => r.BackingReferenceLanguage).Return(rtPortuguese);
			rtFrench.Expect(r => r.HasSecondaryReferenceText).Return(true);
			rtPortuguese.Expect(r => r.BackingReferenceLanguage).Return(rtEnglish);
			rtPortuguese.Expect(r => r.HasSecondaryReferenceText).Return(true);
			rtEnglish.Expect(r => r.BackingReferenceLanguage).Return(null);
			rtEnglish.Expect(r => r.HasSecondaryReferenceText).Return(false);
			vernBlock.SetMatchedReferenceBlock(40, m_testVersification, rtFrench);

			Assert.AreEqual("", vernBlock.PrimaryReferenceText);
			var refBlockFrench = vernBlock.ReferenceBlocks.Single();
			Assert.AreEqual("", refBlockFrench.PrimaryReferenceText);
			var refBlockPortuguese = refBlockFrench.ReferenceBlocks.Single();
			Assert.AreEqual("", refBlockPortuguese.PrimaryReferenceText);
			var refBlockEnglish = refBlockPortuguese.ReferenceBlocks.Single();
			Assert.IsFalse(refBlockEnglish.MatchesReferenceText);
			Assert.IsFalse(refBlockEnglish.ReferenceBlocks.Any());
		}

		/// <summary>
		/// Note that this tests more deeply recursive nesting than we actually expect to have in Glyssen:
		/// Vernacular (Spanish) backed by French, backed by Portuguese, backed by English.
		/// For normal usage of this method, see the MatchAllBlocks tests in BlockMatchupTests
		/// </summary>
		[Test]
		public void AppendJoinedBlockElements_MultipleReferenceTextBlocksWithNestedBackingReferenceTextBlocks_ElementsOfRefBlocksCombinedAtAllNestingLevels()
		{
			var refBlockNarratorFrench = ReferenceTextTests.CreateNarratorBlockForVerse(2, "Jésus a dit. Pour que Matthieu a répondu, ");
			var narrator = refBlockNarratorFrench.CharacterId;
			var refBlockNarratorPortuguese = ReferenceTextTests.CreateNarratorBlockForVerse(2, "disse Jesus. Para que Matthew respondeu: ");
			refBlockNarratorFrench.SetMatchedReferenceBlock(refBlockNarratorPortuguese);
			var refBlockNarratorEnglish = ReferenceTextTests.CreateNarratorBlockForVerse(2, "said Jesus. To which Matthew replied, ");
			refBlockNarratorPortuguese.SetMatchedReferenceBlock(refBlockNarratorEnglish);

			var refBlockMatthewFrench = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthewFrench.BlockElements.Add(new ScriptText("«Nous savions que.»"));
			var refBlockMatthewPortuguese = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthewPortuguese.BlockElements.Add(new ScriptText("“Sabíamos que isso.”"));
			refBlockMatthewFrench.SetMatchedReferenceBlock(refBlockMatthewPortuguese);
			var refBlockMatthewEnglish = new Block("p", 1, 2) { CharacterId = "Matthew" };
			refBlockMatthewEnglish.BlockElements.Add(new ScriptText("“We knew that.”"));
			refBlockMatthewPortuguese.SetMatchedReferenceBlock(refBlockMatthewEnglish);

			var joinedFrenchRefBlock = new Block(refBlockNarratorFrench.StyleTag, refBlockNarratorFrench.ChapterNumber, refBlockNarratorFrench.InitialStartVerseNumber);
			joinedFrenchRefBlock.CharacterId = narrator;
			joinedFrenchRefBlock.Delivery = "raspy";
			ReferenceText rt = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMAT);
			joinedFrenchRefBlock.AppendJoinedBlockElements(new List<Block> { refBlockNarratorFrench, refBlockMatthewFrench }, rt);
			Assert.AreEqual("{2}\u00A0Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»", joinedFrenchRefBlock.GetText(true));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.AreEqual(2, joinedFrenchRefBlock.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)joinedFrenchRefBlock.BlockElements[0]).Number);
			Assert.AreEqual("Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»", ((ScriptText)joinedFrenchRefBlock.BlockElements[1]).Content);

			Assert.IsTrue(joinedFrenchRefBlock.MatchesReferenceText);
			var portugueseRefBlock = joinedFrenchRefBlock.ReferenceBlocks.Single();

			Assert.AreEqual("{2}\u00A0disse Jesus. Para que Matthew respondeu: “Sabíamos que isso.”", portugueseRefBlock.GetText(true));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.AreEqual(narrator, portugueseRefBlock.CharacterId);
			Assert.AreEqual("raspy", portugueseRefBlock.Delivery);
			Assert.AreEqual(2, portugueseRefBlock.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)portugueseRefBlock.BlockElements[0]).Number);
			Assert.AreEqual("disse Jesus. Para que Matthew respondeu: “Sabíamos que isso.”", ((ScriptText)portugueseRefBlock.BlockElements[1]).Content);

			Assert.IsTrue(portugueseRefBlock.MatchesReferenceText);
			var englishRefBlock = portugueseRefBlock.ReferenceBlocks.Single();

			Assert.AreEqual("{2}\u00A0said Jesus. To which Matthew replied, “We knew that.”", englishRefBlock.GetText(true));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.AreEqual(narrator, englishRefBlock.CharacterId);
			Assert.AreEqual("raspy", englishRefBlock.Delivery);
			Assert.AreEqual(2, englishRefBlock.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)englishRefBlock.BlockElements[0]).Number);
			Assert.AreEqual("said Jesus. To which Matthew replied, “We knew that.”", ((ScriptText)englishRefBlock.BlockElements[1]).Content);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_VerseBridgeAtStart_RefBlockGetsStartingAndEndingVerseNumbersFromBridgeInText(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var refBlock = block.SetMatchedReferenceBlock("{3-6}" + separator + "Text of verses three through six.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(3, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(6, refBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_PreviousReferenceBlockWithVerses_RefBlockGetsStartingAndEndingVerseNumbersFromPreviousReferenceBlock(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var prevRefBlock = new Block("p", 3, 42, 45).AddVerse("42-45", "Initial stuff").AddVerse(46, "Later stuff").AddVerse("47-48", "Final stuff. ");
			var refBlock = block.SetMatchedReferenceBlock("Rest of forty-seven and forty-eight. {49-50}" + separator + "Contents of verses forty-nine through fifty.", prevRefBlock);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(47, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(48, refBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_PreviousReferenceBlockWithoutVerses_RefBlockGetsStartingAndEndingVerseNumbersFromPreviousReferenceBlock(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var prevRefBlock = new Block("p", 3, 47, 48);
			prevRefBlock.BlockElements.Add(new ScriptText("This is some nice text in the middle of a verse bridge"));
			var refBlock = block.SetMatchedReferenceBlock("Rest of forty-seven and forty-eight. {49-50}" + separator + "Contents of verses forty-nine through fifty.", prevRefBlock);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(47, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(48, refBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_RefBlockHasVerseNumberInTheMiddleOfBridgeForPreviousReferenceBlock_RefBlockGetsStartingAndEndingVerseNumbersFromPreviousReferenceBlock(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var prevRefBlock = new Block("p", 3, 47, 48);
			prevRefBlock.BlockElements.Add(new ScriptText("This is some nice text in the middle of a verse bridge"));
			var refBlock = block.SetMatchedReferenceBlock("Rest of forty-seven and forty-eight. {49-50}" + separator + "Contents of verses forty-nine through fifty.", prevRefBlock);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(47, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(48, refBlock.InitialEndVerseNumber);
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_StartAndEndVerseNumbersSeparatedByComma_CommaReplacedByDash(string separator)
		{
			var block = new Block("p", 3, 1).AddVerse(1, "This is verse one. ").AddVerse(2, "This is verse two.");
			var refBlock = block.SetMatchedReferenceBlock("he said. {2}" + separator + "Verse two. {3,6}" + separator + "Text of verses three through six.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(1, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("3-6", refBlock.BlockElements.OfType<Verse>().Skip(1).Single().Number);
		}

		/// <summary>
		/// This is probably not a valid final state, but the user may be in the process of editing and about to cut some text from elsewhere
		/// and paste it after the verse number, so we don't want to just prune the verse number.
		/// </summary>
		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_VerseNumberAtEnd_RefBlockEndsWithVerseElement(string trailingWhitespace)
		{
			var block = new Block("p", 3, 2, 3).AddVerse("2-3", "This is verses two and three. ");
			var refBlock = block.SetMatchedReferenceBlock("{2} Text of verse two. {3}" + trailingWhitespace);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(2, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("3", ((Verse)refBlock.BlockElements.Last()).Number);
		}

		[Test]
		public void SetMatchedReferenceBlock_ContainsInitialEndVerse_RefBlockInitialEndVerseSetBackToZero()
		{
			var block = new Block("p", 3, 2, 3).AddVerse("2-3", "This is verses two and three. ");
			var refBlock = block.SetMatchedReferenceBlock("Text of verse two. {3}Text of verse three.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(2, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
		}

		[TestCase("", "\u00A0")]
		[TestCase("\u00A0", "\u00A0")]
		[TestCase("", " ")]
		[TestCase(" ", " ")]
		[TestCase("", "")]
		public void SetMatchedReferenceBlock_TwoContiguousVerseNmbers_OnlyRetainLastVerseNumber(string separatorBetweenVerses, string separatorAfterSecondVerse)
		{
			var block = new Block("p", 3, 2, 3).AddVerse("2-3", "This is verses two and three. ");
			var refBlock = block.SetMatchedReferenceBlock("{2}" + separatorBetweenVerses + "{3}" + separatorAfterSecondVerse + "Text of verse three.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(3, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("3", refBlock.BlockElements.OfType<Verse>().Single().Number);
		}

		[TestCase("", " ")]
		[TestCase(" ", " ")]
		[TestCase(" ", "")]
		[TestCase("", "")]
		[TestCase("\u00A0", "\u00A0")] // Not very likely, but for good measure
		public void SetMatchedReferenceBlock_ContainsNamedSoundEffect_AnnotationParsedAndIncludedAsBlockElement(string separatorBeforeEffect, string separatorAfterEffect)
		{
			var block = new Block("p", 3, 2).AddVerse("2", "This is verse two.");
			var soundEffect = new Sound {SoundType = SoundType.Sfx, EffectName = "Sneezing", UserSpecifiesLocation = true};
			var refBlock = block.SetMatchedReferenceBlock("{2} Text of verse" + separatorBeforeEffect + soundEffect.ToDisplay() + separatorAfterEffect + "three.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(2, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("2", refBlock.BlockElements.OfType<Verse>().Single().Number);
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.AreEqual("Sneezing", effect.EffectName);
			Assert.AreEqual("Text of verse ", refBlock.BlockElements.OfType<ScriptText>().First().Content);
			Assert.AreEqual(" three.", refBlock.BlockElements.OfType<ScriptText>().Last().Content);
		}

		[TestCase(Sound.kNonSpecificStartOrStop)]
		[TestCase(0)]
		public void SetMatchedReferenceBlock_ContainsMusicStart_AnnotationParsedAndIncludedAsBlockElement(int startVerse)
		{
			var block = new Block("p", 3, 2).AddVerse("2", "This is verse two.");
			var music = new Sound {SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = startVerse, EndVerse = 0};
			var refBlock = block.SetMatchedReferenceBlock("{2} Text of verse " + music.ToDisplay() + "three.");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(2, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("2", refBlock.BlockElements.OfType<Verse>().Single().Number);
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.AreEqual(SoundType.Music, effect.SoundType);
			Assert.AreEqual(startVerse, effect.StartVerse);
			Assert.AreEqual(0, effect.EndVerse);
			Assert.IsTrue(effect.UserSpecifiesLocation);
			Assert.IsNull(effect.EffectName);
			Assert.AreEqual("Text of verse ", refBlock.BlockElements.OfType<ScriptText>().First().Content);
			Assert.AreEqual(" three.", refBlock.BlockElements.OfType<ScriptText>().Last().Content);
		}

		[Test]
		public void SetMatchedReferenceBlock_VernBlockHasCharacter_AnnotationParsedAndIncludedAsBlockElement()
		{
			var block = new Block("p", 8, 29).AddVerse("29", "“¡No te metas con nosotros, Hijo de Dios! ¿Viniste acá para atormentarnos antes de tiempo?”");
			block.SetCharacterAndCharacterIdInScript(@"demons (Legion)/man delivered from Legion of demons", 40, m_testVersification);
			Assert.AreEqual(@"demons (Legion)", block.CharacterIdOverrideForScript);
			var refBlock = block.SetMatchedReferenceBlock("{29} “What do we have to do with you, Jesus, Son of God? Have you come here to torment us before the time?”");
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(29, refBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, refBlock.InitialEndVerseNumber);
			Assert.AreEqual("29", refBlock.BlockElements.OfType<Verse>().Single().Number);
			Assert.AreEqual(@"demons (Legion)/man delivered from Legion of demons", refBlock.CharacterId);
			Assert.AreEqual(@"demons (Legion)", refBlock.CharacterIdInScript);
			Assert.AreEqual(@"demons (Legion)", refBlock.CharacterIdOverrideForScript);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNoVerseNumber_LeadingVerseStaysWithRowA(string separator)
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText("{19}"+  separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out newRowAValue, out newRowBValue);
			Assert.AreEqual("{19}" + separator + "This is another chunk of some verse.", newRowAValue);
			Assert.AreEqual("Cool. {20}" + separator + "Fine", newRowBValue);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasNonLeadingVerseNumber_RowBHasNoVerseNumber_EntireContentsSwap(string separator)
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText("Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out newRowAValue, out newRowBValue);
			Assert.AreEqual("This is another chunk of some verse.", newRowAValue);
			Assert.AreEqual("Cool. {20}" + separator + "Fine", newRowBValue);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNonLeadingVerseNumber_LeadingVerseStaysWithRowA(string separator)
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText("{19}" + separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse. {21}" + separator + "Verse twenty-one.",
				out newRowAValue, out newRowBValue);
			Assert.AreEqual("{19}" + separator + "This is another chunk of some verse. {21}" + separator + "Verse twenty-one.", newRowAValue);
			Assert.AreEqual("Cool. {20}" + separator + "Fine", newRowBValue);
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasLeadingVerseNumber_EntireContentsSwap(string separator)
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText("{19}" + separator + "Cool. {20}" + separator + "Fine", "{21}" + separator + "Verse twenty-one.",
				out newRowAValue, out newRowBValue);
			Assert.AreEqual("{21}" + separator + "Verse twenty-one.", newRowAValue);
			Assert.AreEqual("{19}" + separator + "Cool. {20}" + separator + "Fine", newRowBValue);
		}

		[Test]
		public void GetSwappedReferenceText_RowAIsNull_EntireContentsSwap()
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText(null, "{21} Verse twenty-one.", out newRowAValue, out newRowBValue);
			Assert.AreEqual("{21} Verse twenty-one.", newRowAValue);
			Assert.IsTrue(String.IsNullOrEmpty(newRowBValue));
		}

		[Test]
		public void GetSwappedReferenceText_RowBIsNull_EntireContentsSwap()
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText("{21} Verse twenty-one.", null, out newRowAValue, out newRowBValue);
			Assert.IsTrue(String.IsNullOrEmpty(newRowAValue));
			Assert.AreEqual("{21} Verse twenty-one.", newRowBValue);
		}

		private CharacterVerse JesusQuestioning
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Jesus", "Questioning", null, false);
			}
		}

		private CharacterVerse JesusCommanding
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Jesus", "Commanding", null, false);
			}
		}

		private CharacterVerse Andrew
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Andrew", null, null, false);
			}
		}
	}
}
