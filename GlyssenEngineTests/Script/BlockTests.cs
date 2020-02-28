﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.IO;
using SIL.Scripture;
using SIL.TestUtilities;
using SIL.WritingSystems;
using SIL.Xml;
using static GlyssenEngine.Character.CharacterVerseData;
using Resources = GlyssenEngineTests.Properties.Resources;

namespace GlyssenEngineTests.Script
{
	[TestFixture]
	class BlockTests
	{
		private ScrVers m_testVersification;
		private IQuoteInterruptionFinder m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Implementation.Load(tempFile.Path);
			}

			m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes = new QuoteSystem(QuoteSystem.Default);
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
		public void ChangeReferenceText_EnglishToFrenchHeSaid_ReferenceTextChangesToIlADit()
		{
			var rtEnglish = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var block = new Block("p", 1, 10);
			block.BlockElements.Add(new ScriptText("dijo."));
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(rtEnglish.HeSaidText);
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English));
			Assert.AreEqual(rtFrench.HeSaidText, block.GetPrimaryReferenceText());
			Assert.AreEqual(rtEnglish.HeSaidText, block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchHeSaidWithVerseNumber_ReferenceTextChangesToIlADitWithVerseNumber()
		{
			var rtEnglish = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var block = new Block("p", 1, 10).AddVerse(10, "dijo."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0" + rtEnglish.HeSaidText);
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English));
			Assert.AreEqual("{10}\u00A0" + rtFrench.HeSaidText, block.GetPrimaryReferenceText());
			Assert.AreEqual("{10}\u00A0" + rtEnglish.HeSaidText, block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_FrenchToEnglish_EnglishMovedFromSecondaryToPrimary()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			var refBlock = block.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary French reference text.");
			refBlock.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary English reference text.");
			Assert.IsTrue(block.ChangeReferenceText("MRK", ReferenceText.GetStandardReferenceText(ReferenceTextType.English),
				ScrVers.English));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{10}\u00A0This is some arbitrary English reference text.", block.GetPrimaryReferenceText());
		}

		[Test] public void ChangeReferenceText_FrenchToSpanish_MultipleMatchingCombinedRefBlocks_ReferenceTextChanged()
		{
			var block = new Block("p", 2, 1)
				.AddVerse(1, "Now when Jesus was born in Bethlehem, Judea during King Herod's reign of terror, oriental magi came to Zion, ")
				.AddVerse(2, "wondering where the King of the Jews was supposed to be born because they had seen his star in the sky and come to worship."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var frenchRefText = block.SetMatchedReferenceBlock("{1}\u00A0Jésus ... {2}\u00A0Ils ... demandent: <<Où ... l'adorer.>>");
			frenchRefText.SetMatchedReferenceBlock("{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem, " +
				"{2}\u00A0saying, “Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.”");
			ReferenceText rtSpanish = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.SpanishMAT);
			Assert.IsTrue(block.ChangeReferenceText("MAT", rtSpanish, ScrVers.English));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Jesús ... {2}\u00A0y ... preguntaron: <<¿Dónde ... adorarlo.>>",
				block.GetPrimaryReferenceText());
			Assert.AreEqual("{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem, " +
				"{2}\u00A0saying, “Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.”",
				block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchArbitraryEditing_ReturnsFalse()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary English reference text.");
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsFalse(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English));
			// Caller will be responsible for clearing the alignment (for this and other related blocks)
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{10}\u00A0This is some arbitrary English reference text.", block.GetPrimaryReferenceText());
			Assert.IsFalse(block.ReferenceBlocks.Single().MatchesReferenceText);
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchWhiteSpaceOnlyAfterVerseNumber_VerseNumberKeptAsReferenceText()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0     ");
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{10}\u00A0", block.GetPrimaryReferenceText());
			Assert.IsTrue(block.ReferenceBlocks.Single().MatchesReferenceText);
			Assert.AreEqual("{10}\u00A0", block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchWhiteSpaceOnlyNoVerseNumber_BlankReferenceText()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("     ");
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("", block.GetPrimaryReferenceText());
			Assert.IsTrue(block.ReferenceBlocks.Single().MatchesReferenceText);
			Assert.AreEqual("", block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToAzeriDifferentNumberOfBlockElements_DoesNotMatch_ReturnsFalse()
		{
			var block = new Block("p", 12, 17).AddVerse(17, "blah blah blah.").AddVerse(18, "More blah blah."); // vernacular
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{17} Stuff that doesn't match...");
			ReferenceText rtAzeri = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.AzeriREV);
			Assert.IsFalse(block.ChangeReferenceText("REV", rtAzeri, ScrVers.English));
			// Caller will be responsible for clearing the alignment (for this and other related blocks)
			Assert.AreEqual("{17}\u00A0Stuff that doesn't match...", block.GetPrimaryReferenceText());
			Assert.IsFalse(block.ReferenceBlocks.Single().MatchesReferenceText);
			Assert.IsNull(block.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchDifferentVersification_ReferenceTextChanged()
		{
			ScrVers vernVers;
			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				vernVers = Versification.Table.Implementation.Load(tempFile.Path);
			}

			var block = new Block("p", 9, 20);
			block.BlockElements.Add(new ScriptText("<<Desde cuando le llega asi?>>"));
			block.CharacterId = "Jesus";
			block.SetMatchedReferenceBlock("“How long has it been since this has come to him?”");
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, vernVers));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("<<Cela lui arrive depuis quand?>>", block.GetPrimaryReferenceText());
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchDifferentVersificationAcrossChapter_ReferenceTextChanged()
		{
			ScrVers vernVers;
			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				vernVers = Versification.Table.Implementation.Load(tempFile.Path);
			}

			var block = new Block("p", 5, 43).AddVerse(43, "Whatever. ").AddVerse(44, "Cool.");
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{43} He strictly ordered them, saying: “Tell no one about this!” Then he said: “Give her something to eat.” " +
				"{1} He went out from there. He came into his own country, and his disciples followed him.");
			ReferenceText rtFrench = TestReferenceText.CreateCustomReferenceText(TestReferenceText.TestReferenceTextResource.FrenchMRK);
			Assert.IsTrue(block.ChangeReferenceText("MRK", rtFrench, vernVers));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{43}\u00A0mais Jésus leur demandeforce: <<Ne dites rien à personne.>> Ensuite il leur dit: " +
				"<<Donnez-lui quelque chose à manger.>> {1}\u00A0J... l'accompagnent.",
				block.GetPrimaryReferenceText());
		}

		[TestCase("lf")] // List footer: https://ubsicap.github.io/usfm/lists/index.html#lf
		[TestCase("li")] // List entry: https://ubsicap.github.io/usfm/lists/index.html#li
		[TestCase("li2")]
		[TestCase("lim")] // Embedded list entry: https://ubsicap.github.io/usfm/lists/index.html#lim
		[TestCase("lim1")]
		[TestCase("q")] // Poetic line: https://ubsicap.github.io/usfm/poetry/index.html#q
		[TestCase("q1")]
		[TestCase("qm")] // Embedded text poetic line: https://ubsicap.github.io/usfm/poetry/index.html#qm
		[TestCase("qm3")]
		[TestCase("m")] // Continuation (margin) paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#m
		[TestCase("mi")] // Indented flush left paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#mi
		[TestCase("pi")] // Indented paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#pi
		[TestCase("pi2")]
		[TestCase("qr")] // Right-aligned poetic line: https://ubsicap.github.io/usfm/poetry/index.html#qr
		[TestCase("qc")] // Centered poetic line: https://ubsicap.github.io/usfm/poetry/index.html#qc
		public void IsFollowOnParagraphStyle_LineBreakingUsfmTag_ReturnsTrue(string tag)
		{
			var block = new Block(tag);
			Assert.IsTrue(block.IsFollowOnParagraphStyle);
		}

		[TestCase("p")] // Normal paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#p
		[TestCase("po")] // Opening of an epistle/letter: https://ubsicap.github.io/usfm/paragraphs/index.html#po
		[TestCase("pr")] // Right-aligned paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#pr
		[TestCase("cls")] // Closure of an epistle/letter: https://ubsicap.github.io/usfm/paragraphs/index.html#cls
		[TestCase("pmo")] // Embedded text opening: https://ubsicap.github.io/usfm/paragraphs/index.html#pmo
		[TestCase("pmc")] // Embedded text closing: https://ubsicap.github.io/usfm/paragraphs/index.html#pmc
		[TestCase("pmr")] // Embedded text refrain: https://ubsicap.github.io/usfm/paragraphs/index.html#pmr
		[TestCase("pc")] // Centered paragraph: https://ubsicap.github.io/usfm/paragraphs/index.html#pc
		[TestCase("qd")] // Hebrew note: https://ubsicap.github.io/usfm/poetry/index.html#qd
		[TestCase("qa")] // Acrostic heading: https://ubsicap.github.io/usfm/poetry/index.html#qa
		public void IsFollowOnParagraphStyle_RealParagraphUsfmTag_ReturnsFalse(string tag)
		{
			var block = new Block(tag);
			Assert.IsFalse(block.IsFollowOnParagraphStyle);
		}

		[TestCase("lf6")]
		[TestCase("li22")]
		[TestCase("lim10")]
		[TestCase("nb2")]
		[TestCase(" q1")]
		[TestCase("p3")]
		[TestCase("qm13")]
		[TestCase("m6")]
		[TestCase("ma")]
		[TestCase("mi1")]
		[TestCase("z")]
		[TestCase("pig")]
		[TestCase("qr7")]
		[TestCase("qc ")]
		public void IsFollowOnParagraphStyle_UnknownTag_ReturnsFalse(string tag)
		{
			var block = new Block(tag);
			Assert.IsFalse(block.IsFollowOnParagraphStyle);
		}

		[Test]
		public void CombineBlocks_DoesNotModifyBlocks()
		{
			var blockA = new Block("p", 1, 4).AddVerse(4);
			var origBlockAElements = blockA.BlockElements.ToList();
			var origBlockAText = blockA.GetText(true);
			blockA.SetMatchedReferenceBlock("Espanol A, ");
			blockA.ReferenceBlocks.Single().SetMatchedReferenceBlock("English A, ");
			var origSpanishRefTextA = blockA.GetPrimaryReferenceText();
			var origEnglishRefTextA = blockA.ReferenceBlocks.Single().GetPrimaryReferenceText();
			var blockB = new Block("q", 1, 4);
			blockB.BlockElements.Add(new ScriptText("Whatever"));
			var origBlockBElements = blockB.BlockElements.ToList();
			var origBlockBText = blockB.GetText(true);
			blockB.SetMatchedReferenceBlock("espanol B.");
			blockB.ReferenceBlocks.Single().SetMatchedReferenceBlock("English B.");
			var origSpanishRefTextB = blockB.GetPrimaryReferenceText();
			var origEnglishRefTextB = blockB.ReferenceBlocks.Single().GetPrimaryReferenceText();
			var newBlock = Block.CombineBlocks(blockA, blockB);
			Assert.AreNotEqual(newBlock, blockA);
			Assert.AreNotEqual(newBlock, blockB);
			Assert.AreEqual(origBlockAText, blockA.GetText(true));
			Assert.AreEqual(origBlockBText, blockB.GetText(true));
			Assert.IsTrue(origBlockAElements.SequenceEqual(blockA.BlockElements));
			Assert.IsTrue(origBlockBElements.SequenceEqual(blockB.BlockElements));
			Assert.IsFalse(newBlock.BlockElements.Any(e => origBlockAElements.Contains(e)));
			Assert.IsFalse(newBlock.BlockElements.Any(e => origBlockBElements.Contains(e)));
			Assert.AreNotEqual(newBlock.ReferenceBlocks.Single(), blockA.ReferenceBlocks.Single());
			Assert.AreNotEqual(newBlock.ReferenceBlocks.Single(), blockB.ReferenceBlocks.Single());
			Assert.AreEqual(origSpanishRefTextA, blockA.GetPrimaryReferenceText());
			Assert.AreEqual(origEnglishRefTextA, blockA.ReferenceBlocks.Single().GetPrimaryReferenceText());
			Assert.AreEqual(origSpanishRefTextB, blockB.GetPrimaryReferenceText());
			Assert.AreEqual(origEnglishRefTextB, blockB.ReferenceBlocks.Single().GetPrimaryReferenceText());
		}

		[Test]
		public void CombineWith_ReturnsThisBlock()
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4);
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			Assert.AreEqual(thisBlock, thisBlock.CombineWith(otherBlock));
		}

		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void CombineWith_BothBlocksAreNotkUserConfirmed_CombinedBlockIsNotUserConfirmed(bool thisBlockUserConfirmed, bool otherBlockUserConfirmed)
		{
			var thisBlock = new Block("p", 1, 4) { UserConfirmed = thisBlockUserConfirmed }.AddVerse(4);
			var otherBlock = new Block("q", 1, 4) { UserConfirmed = otherBlockUserConfirmed };
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			thisBlock.CombineWith(otherBlock);
			Assert.IsFalse(thisBlock.UserConfirmed);
		}

		[Test]
		public void CombineWith_BothBlocksUserConfirmed_CombinedBlockIsUserConfirmed()
		{
			var thisBlock = new Block("p", 1, 4) { UserConfirmed = true }.AddVerse(4);
			var otherBlock = new Block("q", 1, 4) { UserConfirmed = true };
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			thisBlock.CombineWith(otherBlock);
			Assert.IsTrue(thisBlock.UserConfirmed);
		}

		[TestCase("", "")]
		[TestCase(" ", "")]
		[TestCase("", " ")]
		public void CombineWith_TwoBlocksSingleVerse_CombinedBlockTextCombinedWithSpaceAddedAsNeeded(string trailingSpace, string leadingSpace)
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First" + trailingSpace);
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText(leadingSpace + "Second"));
			thisBlock.CombineWith(otherBlock);
			Assert.AreEqual("{4}\u00A0First Second", thisBlock.GetText(true));
		}

		[TestCase("")]
		[TestCase(" ")]
		public void CombineWith_SecondSBlockStartsWithVerseNumber_ScriptTextElementsNotCombined(string trailingSpace)
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First" + trailingSpace);
			var otherBlock = new Block("q", 1, 5).AddVerse(5, "Second");
			thisBlock.CombineWith(otherBlock);
			Assert.AreEqual(4, thisBlock.BlockElements.Count);
			Assert.AreEqual("{4}\u00A0First {5}\u00A0Second", thisBlock.GetText(true));
		}

		[TestCase("", "")]
		[TestCase(" ", "")]
		[TestCase("", " ")]
		public void CombineWith_BothBlocksAreAlignedToEnglishReferenceText_ReferenceTextsAreCombined(string trailingSpace, string leadingSpace)
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First");
			thisBlock.SetMatchedReferenceBlock("{4} First English." + trailingSpace);
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Second"));
			otherBlock.SetMatchedReferenceBlock(leadingSpace + "Second English.");
			thisBlock.CombineWith(otherBlock);
			Assert.AreEqual("{4}\u00A0First Second", thisBlock.GetText(true));
			Assert.AreEqual("{4}\u00A0First English. Second English.", thisBlock.GetPrimaryReferenceText());
		}

		[Test]
		public void CombineWith_OnlyThisBlockHasRT_ThrowsInvalidOperationException()
		{
			// So far, we have no known need for this. If we ever come up with such a need, we can adjust the expected results accordingly.
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First");
			thisBlock.SetMatchedReferenceBlock("{4} First English.");
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Second"));
			Assert.Throws<InvalidOperationException>(() => thisBlock.CombineWith(otherBlock));
		}

		[Test]
		public void CombineWith_OnlyOtherBlockHasRT_ThrowsInvalidOperationException()
		{
			// So far, we have no known need for this. If we ever come up with such a need, we can adjust the expected results accordingly.
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First");
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Second"));
			otherBlock.SetMatchedReferenceBlock("{4} Second English.");
			Assert.Throws<InvalidOperationException>(() => thisBlock.CombineWith(otherBlock));
		}

		[Test]
		public void CombineWith_ThisBlockHasRefTextThatEndsWithAnnotation_ReferenceTextContainsTheAnnotation()
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First");
			thisBlock.SetMatchedReferenceBlock("{4} First English. {F8 SFX--Whatever}");
			Assert.AreEqual("Whatever", ((Sound)thisBlock.ReferenceBlocks.Single().BlockElements.Last()).EffectName);
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Second"));
			otherBlock.SetMatchedReferenceBlock("Second English.");
			thisBlock.CombineWith(otherBlock);
			Assert.AreEqual("{4}\u00A0First Second", thisBlock.GetText(true));
			Assert.AreEqual("{4}\u00A0First English. {F8 SFX--Whatever} Second English.", thisBlock.GetPrimaryReferenceText());
			Assert.AreEqual("Whatever", ((Sound)thisBlock.ReferenceBlocks.Single().BlockElements[2]).EffectName);
		}

		[Test]
		public void CombineWith_MultipleLevelsOfReferenceText_ReferenceTextCombinedAtEachLevel()
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "Eins");
			thisBlock.SetMatchedReferenceBlock("{4} Primer espanol, {F8 SFX--Whatever}");
			thisBlock.ReferenceBlocks.Single().SetMatchedReferenceBlock("{4} First {F8 SFX--Whatever} English,");
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Zwei."));
			otherBlock.SetMatchedReferenceBlock("segundo.");
			otherBlock.ReferenceBlocks.Single().SetMatchedReferenceBlock("second.");
			thisBlock.CombineWith(otherBlock);
			Assert.AreEqual("{4}\u00A0Eins Zwei.", thisBlock.GetText(true));
			Assert.AreEqual("{4}\u00A0Primer espanol, {F8 SFX--Whatever} segundo.", thisBlock.GetPrimaryReferenceText());
			Assert.AreEqual("{4}\u00A0First {F8 SFX--Whatever} English, second.", thisBlock.ReferenceBlocks.Single().GetPrimaryReferenceText().Replace("  ", " "));
			Assert.AreEqual(4, thisBlock.ReferenceBlocks.Single().ReferenceBlocks.Single().BlockElements.Count);
			Assert.AreEqual("Whatever", ((Sound)thisBlock.ReferenceBlocks.Single().ReferenceBlocks.Single().BlockElements[2]).EffectName);
		}

		[TestCase(StandardCharacter.BookOrChapter, "mt")]
		[TestCase(StandardCharacter.Intro, "ip")]
		[TestCase(StandardCharacter.ExtraBiblical, "s")]
		public void AllVerses_ExtraBiblical_Empty(StandardCharacter type, string styleTag)
		{
			var block = new Block(styleTag) {BookCode = "MAT", CharacterId = GetStandardCharacterId("MAT", type)};
			Assert.Throws<InvalidOperationException>(() =>
			{
				if (block.AllVerses.Any()) throw new Exception("Shouldn't have found any verses in a non-Scripture block - should have thrown InvalidOperationException.");
			});
		}

		[TestCase(1, 3, "1-3")]
		[TestCase(5, 0, "5")]
		public void AllVerses_VerseAtStart_ReturnsStartVerse(int start, int end, string verseNumString)
		{
			var block = new Block("p", 1, start, end) { BookCode = "MAT", CharacterId = "Jesus" }.AddVerse(verseNumString);
			var result = block.AllVerses.Single();
			Assert.AreEqual(start, result.StartVerse);
			Assert.AreEqual(end, result.LastVerseOfBridge);
		}

		[TestCase(2, 3)]
		[TestCase(100, 0)]
		public void AllVerses_NoVerse_ReturnsVerseRepresentingInitialVerseStartAndEnd(int start, int end)
		{
			var block = new Block("p", 1, start, end)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			}.AddText();
			var result = block.AllVerses.Single();
			Assert.AreEqual(start, result.StartVerse);
			Assert.AreEqual(end, result.LastVerseOfBridge);
		}

		[Test]
		public void AllVerses_MultipleVersesIncludingStart_ReturnsAllVerses()
		{
			var block = new Block("p", 1, 3)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			}.AddVerse(3).AddVerse(4).AddVerse("5-6").AddVerse("7-9");
			var result = block.AllVerses.ToList();
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual(3, result[0].StartVerse);
			Assert.AreEqual(0, result[0].LastVerseOfBridge);
			Assert.AreEqual(4, result[1].StartVerse);
			Assert.AreEqual(0, result[1].LastVerseOfBridge);
			Assert.AreEqual(5, result[2].StartVerse);
			Assert.AreEqual(6, result[2].LastVerseOfBridge);
			Assert.AreEqual(7, result[3].StartVerse);
			Assert.AreEqual(9, result[3].LastVerseOfBridge);
		}

		[Test]
		public void AllVerses_DoesNotStartWithVerseNum_HasMidBlockVerse_ReturnsStartAndMidBlockVerses()
		{
			var block = new Block("p", 1, 3)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			}.AddText("Second half of verse 3").AddVerse(4);
			var result = block.AllVerses.ToList();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(3, result[0].StartVerse);
			Assert.AreEqual(0, result[0].LastVerseOfBridge);
			Assert.AreEqual(4, result[1].StartVerse);
			Assert.AreEqual(0, result[1].LastVerseOfBridge);
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
		public void SetCharacterAndDelivery_SingleCharacter_SetsCharacterAndDelivery()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { JesusQuestioning });
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
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new CharacterSpeakingMode[0]);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, block.CharacterId);
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
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new [] { JesusCommanding, JesusQuestioning, Andrew });
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, block.CharacterId);
			Assert.IsNull(block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_SingleMultipleChoiceCharacter_SetsCharacterAndCharacterIdInScript()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { new CharacterSpeakingMode("Mary/Martha", null, null, false),  });
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
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { new CharacterSpeakingMode("Mary/Martha", null, null, false) });
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
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { new CharacterSpeakingMode("Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Mary", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SameMultipleChoiceCharacterWithDifferentDefaults_CharacterIdInScriptSetBasedOnFirstCvEntry()
		{
			var block = new Block("p", 4, 4, 6);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verses four through seven. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[]
			{
				new CharacterSpeakingMode("Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Martha"),
				new CharacterSpeakingMode("Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Jews"),
				new CharacterSpeakingMode("Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Mary")
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
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { JesusQuestioning });
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
		[TestCase(CharacterVerseData.kUnexpectedCharacter)]
		public void SetCharacterAndCharacterIdInScript_CharacterIdSetUnclear_CharacterIdInScriptSetToNull(string unclearCharacterId)
		{
			var block = new Block("p", 40, 8);
			block.CharacterIdInScript = "chief monkey";
			block.CharacterId = "chief monkey";
			Assert.AreEqual("chief monkey", block.CharacterId);
			Assert.AreEqual("chief monkey", block.CharacterIdInScript);
			Assert.AreEqual("chief monkey", block.CharacterIdOverrideForScript);
			// end setup

			block.SetCharacterIdAndCharacterIdInScript(unclearCharacterId, BCVRef.BookToNumber("EXO"));
			Assert.AreEqual(unclearCharacterId, block.CharacterId);
			Assert.AreEqual(unclearCharacterId, block.CharacterIdInScript);
			Assert.IsNull(block.CharacterIdOverrideForScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoice_CharacterIdInScriptRemainsNull()
		{
			var block = new Block("p", 40, 8);
			block.SetCharacterIdAndCharacterIdInScript("chief monkey", BCVRef.BookToNumber("EXO"));
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
			block.SetCharacterIdAndCharacterIdInScript("subordinate monkey", BCVRef.BookToNumber("REV"));
			Assert.AreEqual("subordinate monkey", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NoChangeToCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_NoChange()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "dead frog";
			block.SetCharacterIdAndCharacterIdInScript("chief cupbearer/chief baker", BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer/chief baker", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ControlFileHasOverriddenDefault_VersificationShift_CharacterIdInScriptBasedOnOverride()
		{
			// MRK 9:10 in the Vulgate should translate to 9:11 in the "original"
			// The control file overrides the default speaker in MRK 9:11 to be John.
			var block = new Block("p", 9, 10);
			block.SetCharacterIdAndCharacterIdInScript("Peter (Simon)/James/John", BCVRef.BookToNumber("MRK"), m_testVersification);
			Assert.AreEqual("Peter (Simon)/James/John", block.CharacterId);
			Assert.AreEqual("John", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ChangeCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_CharacterIdInScriptChanged()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "chief cupbearer";
			block.SetCharacterIdAndCharacterIdInScript("David/Goliath", BCVRef.BookToNumber("GEN"));
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

		[Test]
		public void SerializeDeserialize_ContainsPrimaryReferenceText_RoundtripDataRemainsTheSame()
		{
			var block = new Block();
			block.BlockElements = new List<BlockElement>
			{
				new ScriptText("script text"),
			};
			var primaryReferenceTextBlock = new Block();
			primaryReferenceTextBlock.BlockElements = new List<BlockElement>
			{
				new ScriptText("primary reference text")
			};
			block.SetMatchedReferenceBlock(primaryReferenceTextBlock);

			var blockBefore = block.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(block);
			AssertThatXmlIn.String(xmlString).HasSpecifiedNumberOfMatchesForXpath("/block/ReferenceBlocks/text[text()='primary reference text']", 1);
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

			Assert.AreEqual("", vernBlock.GetPrimaryReferenceText());
			var refBlockFrench = vernBlock.ReferenceBlocks.Single();
			Assert.AreEqual("", refBlockFrench.GetPrimaryReferenceText());
			var refBlockPortuguese = refBlockFrench.ReferenceBlocks.Single();
			Assert.AreEqual("", refBlockPortuguese.GetPrimaryReferenceText());
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
			var music = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = startVerse, EndVerse = 0 };
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

		[TestCase(Sound.kNonSpecificStartOrStop)]
		[TestCase(0)]
		public void SetMatchedReferenceBlock_OnlyAnnotation_AnnotationParsed(int startVerse)
		{
			var block = new Block("p", 3, 2).AddVerse("2", "This is verse two.");
			var music = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = startVerse, EndVerse = 0 };
			var refBlock = block.SetMatchedReferenceBlock(music.ToDisplay());
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual(refBlock, block.ReferenceBlocks.Single());
			Assert.AreEqual(2, refBlock.InitialStartVerseNumber);
			Assert.IsFalse(refBlock.BlockElements.OfType<Verse>().Any());
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.AreEqual(SoundType.Music, effect.SoundType);
			Assert.AreEqual(startVerse, effect.StartVerse);
			Assert.IsTrue(effect.UserSpecifiesLocation);
			Assert.IsNull(effect.EffectName);
			Assert.IsFalse(refBlock.BlockElements.OfType<ScriptText>().Any());
		}

		[Test]
		public void SetMatchedReferenceBlock_VernBlockHasCharacter_AnnotationParsedAndIncludedAsBlockElement()
		{
			var block = new Block("p", 8, 29).AddVerse("29", "“¡No te metas con nosotros, Hijo de Dios! ¿Viniste acá para atormentarnos antes de tiempo?”");
			block.SetCharacterIdAndCharacterIdInScript(@"demons (Legion)/man delivered from Legion of demons", 40, m_testVersification);
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

		[TestCase("1234567")]
		[TestCase("This is a pretty basic test")]
		public void Length_JustText_LengthIsTextLength(string text)
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new ScriptText(text)
				}
			};
			Assert.AreEqual(text.Length, block.Length);
		}

		[TestCase("1234567")]
		[TestCase("This is a pretty basic test")]
		public void Length_VerseAndText_LengthIsTextLength(string text)
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new Verse("2"),
					new ScriptText(text)
				}
			};
			Assert.AreEqual(text.Length, block.Length);
		}

		[TestCase("1234567", "123")]
		[TestCase("This is a slightly more", "complicated test")]
		public void Length_TextVerseText_LengthIsCombinedTextLength(string text1, string text2)
		{
			var block = new Block("p", 1, 2, 3)
			{
				BlockElements =
				{
					new ScriptText(text1),
					new Verse("3"),
					new ScriptText(text2)
				}
			};
			Assert.AreEqual(text1.Length + text2.Length, block.Length);
		}

		[Test]
		public void Length_IsChapterAnnouncementWithFormat_LengthIsCorrect()
		{
			var text = "My Chapter Announcement";
			var chapterAnnouncementBlock = new Block("c", 1)
			{
				BlockElements = { new ScriptText(text) },
				BookCode = "MAT"
			};

			var originalChapterFormat = Block.FormatChapterAnnouncement;

			Block.FormatChapterAnnouncement = (bookCode, chapterNumber) => $"{bookCode} {chapterNumber}";
			Assert.AreEqual("MAT 1".Length, chapterAnnouncementBlock.Length);

			// If the formating Func returns null, we get the text from the ScriptText element
			Block.FormatChapterAnnouncement = (s, i) => null;
			Assert.AreEqual(text.Length, chapterAnnouncementBlock.Length);

			// Set it back to what it was so we don't mess up any other tests.
			Block.FormatChapterAnnouncement = originalChapterFormat;
		}

		[Test]
		public void ScriptTextCount_JustText_1Verse()
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new ScriptText("abc")
				}
			};
			Assert.AreEqual(1, block.ScriptTextCount);
		}

		[Test]
		public void ScriptTextCount_VerseAndText_1Verse()
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new Verse("2"),
					new ScriptText("abc")
				}
			};
			Assert.AreEqual(1, block.ScriptTextCount);
		}

		[Test]
		public void ScriptTextCount_TextVerseText_2Verses()
		{
			var block = new Block("p", 1, 2, 3)
			{
				BlockElements =
				{
					new ScriptText("abc"),
					new Verse("3"),
					new ScriptText("xyz")
				}
			};
			Assert.AreEqual(2, block.ScriptTextCount);
		}

		[Test]
		public void ScriptTextCount_VerseTextVerseText_2Verses()
		{
			var block = new Block("p", 1, 2, 3)
			{
				BlockElements =
				{
					new Verse("2"),
					new ScriptText("abc"),
					new Verse("3"),
					new ScriptText("xyz")
				}
			};
			Assert.AreEqual(2, block.ScriptTextCount);
		}

		[Test]
		public void SplitBlock_VerseBridge_SplitsCorrectly()
		{
			var block = new Block("p", 1, 1)
			{
				BlockElements =
				{
					new Verse("1b"),
					new ScriptText("abcdef ghi")
				}
			};
			var newBlock = block.SplitBlock("1b", 3);

			Assert.AreEqual("{1b}\u00A0abc", block.GetText(true));
			Assert.AreEqual("def ghi", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_VerseSegment_SplitsCorrectly()
		{
			var block = new Block("p", 1, 1)
			{
				BlockElements =
				{
					new Verse("1"),
					new ScriptText("abcdef ghi "),
					new Verse("2-3"),
					new ScriptText("jk lmno p")
				}
			};
			var newBlock = block.SplitBlock("2-3", 4);

			Assert.AreEqual("{1}\u00A0abcdef ghi {2-3}\u00A0jk l", block.GetText(true));
			Assert.AreEqual("mno p", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_HasLeadingPunctuation_SplitsCorrectly()
		{
			var block = new Block("p", 1, 1)
			{
				BlockElements =
				{
					new ScriptText("("),
					new Verse("1"),
					new ScriptText("abcdef ghi) "),
					new Verse("2"),
					new ScriptText("jk lmno p")
				}
			};
			var newBlock = block.SplitBlock("1", 3);

			Assert.AreEqual("({1}\u00A0abc", block.GetText(true));
			Assert.AreEqual("def ghi) {2}\u00A0jk lmno p", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_HasLeadingPunctuation_SplitOneCharacterAwayFromVerseNumber_SplitsCorrectly()
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new ScriptText("("),
					new Verse("2"),
					new ScriptText("abcdef ghi) "),
					new Verse("3"),
					new ScriptText("jk lmno p")
				}
			};

			var newBlock = block.SplitBlock("2", 1);

			Assert.AreEqual("({2}\u00A0a", block.GetText(true));
			Assert.AreEqual(2, block.InitialStartVerseNumber);
			Assert.AreEqual("bcdef ghi) {3}\u00A0jk lmno p", newBlock.GetText(true));
			Assert.AreEqual(2, newBlock.InitialStartVerseNumber);
		}

		/// <summary>
		/// PG-1311: Test that the initial start verse number for the new block is set correctly (based on first actual verse element)
		/// </summary>
		[TestCase("[")] // This is the likely case
		[TestCase(".")] // This would probably be a mistake (see ENHANCE comment in SplitBlockDlg.DetermineSplitLocatino),
						// but if the user did it, we would also need this same behavior.
		public void SplitBlock_SplitBeforeTrailingPunctuation_NewBlockInitialVerseNumberBasedOnFirstVerseNumber(string trailingPunctuation)
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new Verse("2"),
					new ScriptText($"abcdef ghi{trailingPunctuation} "),
					new Verse("3"),
					new ScriptText("jk lmno p")
				}
			};

			var newBlock = block.SplitBlock("2", 10);

			Assert.AreEqual("{2}\u00A0abcdef ghi", block.GetText(true));
			Assert.AreEqual(2, block.InitialStartVerseNumber);
			Assert.AreEqual(trailingPunctuation + " {3}\u00A0jk lmno p", newBlock.GetText(true));
			Assert.AreEqual(3, newBlock.InitialStartVerseNumber);
		}

		[Test]
		public void SplitBlock_HasLeadingPunctuation_SplitAtVerseEnd_SplitsCorrectly()
		{
			var block = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new ScriptText("("),
					new Verse("2"),
					new ScriptText("abcdef ghi) "),
					new Verse("3"),
					new ScriptText("jk lmno p")
				}
			};

			var newBlock = block.SplitBlock("2", PortionScript.kSplitAtEndOfVerse);

			Assert.AreEqual("({2}\u00A0abcdef ghi) ", block.GetText(true));
			Assert.AreEqual("{3}\u00A0jk lmno p", newBlock.GetText(true));
		}

		[TestCase("-a-")]
		[TestCase("—a—")]
		[TestCase("(a)")]
		[TestCase("[a]")]
		[TestCase("[a] b c")]
		public void ProbablyDoesNotContainInterruption_ApparentInterruptions_ReturnsFalse(string text)
		{
			var block = GetBlockWithText(text);
			Assert.False(block.ProbablyDoesNotContainInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes));
		}

		[Test]
		public void ProbablyDoesNotContainInterruption_PossibleInterruptionUsesLongDashesWhichAreAlsoUsedForDialgueQuotes_ReturnsTrue()
		{
			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			var block = GetBlockWithText("—a—");
			Assert.True(block.ProbablyDoesNotContainInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes));
		}

		[TestCase("a b-c d-e")]
		[TestCase("a -c d e")]
		[TestCase("a c- d e")]
		[TestCase("a c - d e")]
		public void ProbablyDoesNotContainInterruption_WordMedialOrUnmatchedDashes_ReturnsTrue(string text)
		{
			var block = GetBlockWithText(text);
			Assert.True(block.ProbablyDoesNotContainInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes));

			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			Assert.True(block.ProbablyDoesNotContainInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes));
		}

		[TestCase("a (bcd) e", ExpectedResult = "(bcd) ")]
		[TestCase("a -b- c", ExpectedResult = "-b- ")]
		[TestCase("a - b - c", ExpectedResult = "- b - ")]
		[TestCase("a -- b -- c", ExpectedResult = "-- b -- ")]
		[TestCase("a —b— c", ExpectedResult = null)]
		[TestCase("a - b-c - d", ExpectedResult = "- b-c - ")]
		[TestCase("a -- b-c -- d", ExpectedResult = "-- b-c -- ")]
		public string GetNextInterruption_QuoteSystemWithLongDashDialogueQuotes_InterruptionFoundCorrectly(string text)
		{
			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			var block = GetBlockWithText(text);
			return block.GetNextInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes)?.Item1.Value;
		}

		[TestCase("a (bcd) e", ExpectedResult = "(bcd) ")]
		[TestCase("a -b- c", ExpectedResult = "-b- ")]
		[TestCase("a - b - c", ExpectedResult = "- b - ")]
		[TestCase("a -- b -- c", ExpectedResult = "-- b -- ")]
		[TestCase("a —b— c", ExpectedResult = "—b— ")]
		[TestCase("a - b-c - d", ExpectedResult = "- b-c - ")]
		[TestCase("a -- b-c -- d", ExpectedResult = "-- b-c -- ")]
		public string GetNextInterruption_QuoteSystemWithoutLongDashDialogueQuotes_InterruptionFoundCorrectly(string text)
		{
			var block = GetBlockWithText(text);
			return block.GetNextInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes)?.Item1.Value;
		}

		[TestCase("a b-c-d e")]
		[TestCase("a b-c d-e")]
		public void GetNextInterruption_WordMedialDashes_NoInterruptionFound(string text)
		{
			var block = GetBlockWithText(text);
			Assert.Null(block.GetNextInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes));

			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			Assert.Null(block.GetNextInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes));
		}

		private Block GetBlockWithText(string text)
		{
			return new Block("p", 1, 1)
			{
				BlockElements =
				{
					new ScriptText(text),
				}
			};
		}

		private CharacterSpeakingMode JesusQuestioning => new CharacterSpeakingMode("Jesus", "Questioning", null, false);

		private CharacterSpeakingMode JesusCommanding => new CharacterSpeakingMode("Jesus", "Commanding", null, false);

		private CharacterSpeakingMode Andrew => new CharacterSpeakingMode("Andrew", null, null, false);
	}
}
