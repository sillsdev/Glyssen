using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using GlyssenSharedTests;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.IO;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;
using static System.String;
using static Glyssen.Shared.ReferenceTextType;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenCharacters.CharacterVerseData.StandardCharacter;
using static GlyssenEngineTests.TestReferenceText;
using static GlyssenEngineTests.XmlComparisonTestUtils;
using Resources = GlyssenCharactersTests.Properties.Resources;

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
			ForgetCustomReferenceTexts();
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchHeSaid_ReferenceTextChangesToIlADit()
		{
			var rtEnglish = ReferenceText.GetStandardReferenceText(English);
			var block = new Block("p", 1, 10);
			block.BlockElements.Add(new ScriptText("dijo."));
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock(rtEnglish.HeSaidText);
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English), Is.True);
			Assert.That(rtFrench.HeSaidText, Is.EqualTo(block.GetPrimaryReferenceText()));
			Assert.That(rtEnglish.HeSaidText, Is.EqualTo(block.ReferenceBlocks.Single().GetPrimaryReferenceText()));
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchHeSaidWithVerseNumber_ReferenceTextChangesToIlADitWithVerseNumber()
		{
			var rtEnglish = ReferenceText.GetStandardReferenceText(English);
			var block = new Block("p", 1, 10).AddVerse(10, "dijo."); // vernacular
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0" + rtEnglish.HeSaidText);
			var rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English), Is.True);
			Assert.That(block.GetPrimaryReferenceText(),
				Is.EqualTo("{10}\u00A0" + rtFrench.HeSaidText));
			Assert.That(block.ReferenceBlocks.Single().GetPrimaryReferenceText(),
				Is.EqualTo("{10}\u00A0" + rtEnglish.HeSaidText));
		}

		[Test]
		public void ChangeReferenceText_FrenchToEnglish_EnglishMovedFromSecondaryToPrimary()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			var refBlock = block.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary French reference text.");
			refBlock.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary English reference text.");
			Assert.That(block.ChangeReferenceText("MRK", ReferenceText.GetStandardReferenceText(English),
				ScrVers.English), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo("{10}\u00A0This is some arbitrary English reference text."));
		}

		[Test] public void ChangeReferenceText_FrenchToSpanish_MultipleMatchingCombinedRefBlocks_ReferenceTextChanged()
		{
			var block = new Block("p", 2, 1)
				.AddVerse(1, "Now when Jesus was born in Bethlehem, Judea during King Herod's reign of terror, oriental magi came to Zion, ")
				.AddVerse(2, "wondering where the King of the Jews was supposed to be born because they had seen his star in the sky and come to worship."); // vernacular
			block.CharacterId = GetStandardCharacterId("MAT", Narrator);
			var frenchRefText = block.SetMatchedReferenceBlock("{1}\u00A0Jésus ... {2}\u00A0Ils ... demandent: <<Où ... l'adorer.>>");
			frenchRefText.SetMatchedReferenceBlock("{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem, " +
				"{2}\u00A0saying, «Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.»");
			ReferenceText rtSpanish = CreateCustomReferenceText(TestReferenceTextResource.SpanishMAT);
			Assert.That(block.ChangeReferenceText("MAT", rtSpanish, ScrVers.English), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo(
				"{1}\u00A0Jesús ... {2}\u00A0y ... preguntaron: <<¿Dónde ... adorarlo.>>"));
			Assert.That(block.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(
				"{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem, " +
				"{2}\u00A0saying, «Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.»"));
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchArbitraryEditing_ReturnsFalse()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0This is some arbitrary English reference text.");
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English), Is.False);
			// Caller will be responsible for clearing the alignment (for this and other related blocks)
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo("{10}\u00A0This is some arbitrary English reference text."));
			Assert.That(block.ReferenceBlocks.Single().MatchesReferenceText, Is.False);
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchWhiteSpaceOnlyAfterVerseNumber_VerseNumberKeptAsReferenceText()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock("{10}\u00A0     ");
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo("{10}\u00A0"));
			Assert.That(block.ReferenceBlocks.Single().MatchesReferenceText, Is.True);
			Assert.That(block.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo("{10}\u00A0"));
		}

		[Test]
		public void ChangeReferenceText_EnglishToFrenchWhiteSpaceOnlyNoVerseNumber_BlankReferenceText()
		{
			var block = new Block("p", 1, 10).AddVerse(10, "blah blah blah."); // vernacular
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock("     ");
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, ScrVers.English), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo(""));
			Assert.That(block.ReferenceBlocks.Single().MatchesReferenceText, Is.True);
			Assert.That(block.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(""));
		}

		[Test]
		public void ChangeReferenceText_EnglishToAzeriDifferentNumberOfBlockElements_DoesNotMatch_ReturnsFalse()
		{
			var block = new Block("p", 12, 17).AddVerse(17, "blah blah blah.").AddVerse(18, "More blah blah."); // vernacular
			block.CharacterId = GetStandardCharacterId("REV", Narrator);
			block.SetMatchedReferenceBlock("{17} Stuff that doesn't match...");
			ReferenceText rtAzeri = CreateCustomReferenceText(TestReferenceTextResource.AzeriREV);
			Assert.That(block.ChangeReferenceText("REV", rtAzeri, ScrVers.English), Is.False);
			// Caller will be responsible for clearing the alignment (for this and other related blocks)
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo("{17}\u00A0Stuff that doesn't match..."));
			Assert.That(block.ReferenceBlocks.Single().MatchesReferenceText, Is.False);
			Assert.That(block.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.Null);
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
			block.SetMatchedReferenceBlock("«How long has it been since this has come to him?»");
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, vernVers), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo("<<Cela lui arrive depuis quand?>>"));
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
			block.CharacterId = GetStandardCharacterId("MRK", Narrator);
			block.SetMatchedReferenceBlock("{43} He strictly ordered them, saying: «Tell no one about this!» Then he said: «Give her something to eat.» " +
				"{1} He went out from there. He came into his own country, and his disciples followed him.");
			ReferenceText rtFrench = CreateCustomReferenceText(TestReferenceTextResource.FrenchMRK);
			Assert.That(block.ChangeReferenceText("MRK", rtFrench, vernVers), Is.True);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(block.GetPrimaryReferenceText(), Is.EqualTo(
				"{43}\u00A0mais Jésus leur demandeforce: <<Ne dites rien à personne.>> Ensuite il leur dit: " +
				"<<Donnez-lui quelque chose à manger.>> {1}\u00A0J... l'accompagnent."));
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
			Assert.That(block.IsFollowOnParagraphStyle, Is.True);
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
			Assert.That(block.IsFollowOnParagraphStyle, Is.False);
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
			Assert.That(block.IsFollowOnParagraphStyle, Is.False);
		}

		[TestCase(3, 4)]
		[TestCase(6, 10)]
		public void IsSimpleBridge_StartsWithBridgeAndHasNoOtherVerses_ReturnsTrue(int bridgeStartVerse, int bridgeEndVerse)
		{
			var block = new Block("p", 1, bridgeStartVerse, bridgeEndVerse)
				{ BlockElements = new List<BlockElement>
					{
						new Verse($"{bridgeStartVerse}-{bridgeEndVerse}"),
						new ScriptText("This is the text of the only verse "),
						new Sound {UserSpecifiesLocation = true, EffectName = "Warbling noise"},
						new ScriptText("in this block.")
					}
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.False);
			// SUT
			Assert.That(block.IsSimpleBridge, Is.True);
		}

		[TestCase(3, 4)]
		[TestCase(6, 10)]
		public void IsSimpleBridge_ContinuesVerseBridgeStartedInPreviousBlockAndHasNoOtherVerses_ReturnsTrue(int bridgeStartVerse, int bridgeEndVerse)
		{
			var block = new Block("p", 1, bridgeStartVerse, bridgeEndVerse)
				{ BlockElements = new List<BlockElement> { new ScriptText("“This is the thing spoken by the guy whose reporting clause was in the previous block.”") }
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.False);
			// SUT
			Assert.That(block.IsSimpleBridge, Is.True);
		}

		[TestCase(3)]
		[TestCase(6)]
		public void IsSimpleBridge_StartsWithSingleVerseAndHasNoOtherVerses_ReturnsFalse(int verse)
		{
			var block = new Block("p", 1, verse)
				{ BlockElements = new List<BlockElement>
					{
						new Verse($"{verse}"),
						new ScriptText("This is the text of the only verse in this block.")
					}
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.False,
				"Note: Even if the block covers only a single verse, it is not a simple bridge if that verse is not a verse bridge.");
			// SUT
			Assert.That(block.IsSimpleBridge, Is.False);
		}

		[TestCase(3, 4)]
		[TestCase(6, 10)]
		public void IsSimpleBridge_StartsWithBridgeButHasAnotherVerse_ReturnsFalse(int bridgeStartVerse, int bridgeEndVerse)
		{
			var block = new Block("p", 1, bridgeStartVerse, bridgeEndVerse)
				{ BlockElements = new List<BlockElement>
					{
						new Verse($"{bridgeStartVerse}-{bridgeEndVerse}"),
						new ScriptText("This is the text of the verse bridge."),
						new Verse($"{bridgeEndVerse + 1}"),
						new ScriptText("This is the next verse.")
					}
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.True,
				"If block covers more than one verse, then it is not a simple bridge.");
			// SUT
			Assert.That(block.IsSimpleBridge, Is.False);
		}

		[TestCase(3, 4)]
		[TestCase(6, 10)]
		public void IsSimpleBridge_ContinuesVerseBridgeStartedInPreviousBlockButHasAnotherVerse_ReturnsFalse(int bridgeStartVerse, int bridgeEndVerse)
		{
			var block = new Block("p", 1, bridgeStartVerse, bridgeEndVerse)
				{ BlockElements = new List<BlockElement>
					{
						new ScriptText("“This is the thing spoken by the guy whose reporting clause was in the previous block.”"),
						new Verse($"{bridgeEndVerse + 1}"),
						new ScriptText("This is the next verse.")
					}
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.True, "If block covers more than one verse, then it is not a simple bridge.");
			// SUT
			Assert.That(block.IsSimpleBridge, Is.False);
		}

		[TestCase(3)]
		[TestCase(6)]
		public void CoversMoreThanOneVerse_StartsWithTextOfPreviousVerseButHasAnotherVerses_ReturnsTrue(int prevVerse)
		{
			var block = new Block("p", 1, prevVerse)
				{ BlockElements = new List<BlockElement>
					{
						new ScriptText("rest of the text of the verse started in the previous block. "),
						new Verse($"{prevVerse + 1}"),
						new ScriptText("This is the text of the verse started in this block.")
					}
				};
			Assert.That(block.CoversMoreThanOneVerse, Is.True);
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
			Assert.That(newBlock, Is.Not.EqualTo(blockA));
			Assert.That(newBlock, Is.Not.EqualTo(blockB));
			Assert.That(blockA.GetText(true), Is.EqualTo(origBlockAText));
			Assert.That(blockB.GetText(true), Is.EqualTo(origBlockBText));
			Assert.That(blockA.BlockElements, Is.EqualTo(origBlockAElements));
			Assert.That(blockB.BlockElements, Is.EqualTo(origBlockBElements));
			Assert.That(newBlock.BlockElements.Any(e => origBlockAElements.Contains(e)), Is.False);
			Assert.That(newBlock.BlockElements.Any(e => origBlockBElements.Contains(e)), Is.False);
			Assert.That(newBlock.ReferenceBlocks.Single(), Is.Not.EqualTo(blockA.ReferenceBlocks.Single()));
			Assert.That(newBlock.ReferenceBlocks.Single(), Is.Not.EqualTo(blockB.ReferenceBlocks.Single()));
			Assert.That(blockA.GetPrimaryReferenceText(), Is.EqualTo(origSpanishRefTextA));
			Assert.That(blockA.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(origEnglishRefTextA));
			Assert.That(blockB.GetPrimaryReferenceText(), Is.EqualTo(origSpanishRefTextB));
			Assert.That(blockB.ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(origEnglishRefTextB));
		}

		[Test]
		public void CombineWith_ReturnsThisBlock()
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4);
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			Assert.That(thisBlock, Is.EqualTo(thisBlock.CombineWith(otherBlock)));
		}

		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void CombineWith_BothBlocksAreNotUserConfirmed_CombinedBlockIsNotUserConfirmed(bool thisBlockUserConfirmed, bool otherBlockUserConfirmed)
		{
			var thisBlock = new Block("p", 1, 4) { UserConfirmed = thisBlockUserConfirmed }.AddVerse(4);
			var otherBlock = new Block("q", 1, 4) { UserConfirmed = otherBlockUserConfirmed };
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			thisBlock.CombineWith(otherBlock);
			Assert.That(thisBlock.UserConfirmed, Is.False);
		}

		[Test]
		public void CombineWith_BothBlocksUserConfirmed_CombinedBlockIsUserConfirmed()
		{
			var thisBlock = new Block("p", 1, 4) { UserConfirmed = true }.AddVerse(4);
			var otherBlock = new Block("q", 1, 4) { UserConfirmed = true };
			otherBlock.BlockElements.Add(new ScriptText("Whatever"));
			thisBlock.CombineWith(otherBlock);
			Assert.That(thisBlock.UserConfirmed, Is.True);
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
			Assert.That(thisBlock.GetText(true), Is.EqualTo("{4}\u00A0First Second"));
		}

		[TestCase("")]
		[TestCase(" ")]
		public void CombineWith_SecondSBlockStartsWithVerseNumber_ScriptTextElementsNotCombined(string trailingSpace)
		{
			var thisBlock = new Block("p", 1, 4).AddVerse(4, "First" + trailingSpace);
			var otherBlock = new Block("q", 1, 5).AddVerse(5, "Second");
			thisBlock.CombineWith(otherBlock);
			Assert.That(thisBlock.BlockElements.Count, Is.EqualTo(4));
			Assert.That(thisBlock.GetText(true), Is.EqualTo("{4}\u00A0First {5}\u00A0Second"));
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
			Assert.That(thisBlock.GetText(true), Is.EqualTo("{4}\u00A0First Second"));
			Assert.That(thisBlock.GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0First English. Second English."));
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
			Assert.That(((Sound)thisBlock.ReferenceBlocks.Single().BlockElements.Last()).EffectName, Is.EqualTo("Whatever"));
			var otherBlock = new Block("q", 1, 4);
			otherBlock.BlockElements.Add(new ScriptText("Second"));
			otherBlock.SetMatchedReferenceBlock("Second English.");
			thisBlock.CombineWith(otherBlock);
			Assert.That(thisBlock.GetText(true), Is.EqualTo("{4}\u00A0First Second"));
			Assert.That(thisBlock.GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0First English. {F8 SFX--Whatever} Second English."));
			Assert.That(((Sound)thisBlock.ReferenceBlocks.Single().BlockElements[2]).EffectName, Is.EqualTo("Whatever"));
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
			Assert.That(thisBlock.GetText(true), Is.EqualTo("{4}\u00A0Eins Zwei."));
			Assert.That(thisBlock.GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0Primer espanol, {F8 SFX--Whatever} segundo."));
			Assert.That(thisBlock.ReferenceBlocks.Single().GetPrimaryReferenceText().Replace("  ", " "), Is.EqualTo("{4}\u00A0First {F8 SFX--Whatever} English, second."));
			Assert.That(thisBlock.ReferenceBlocks.Single().ReferenceBlocks.Single().BlockElements.Count, Is.EqualTo(4));
			Assert.That(((Sound)thisBlock.ReferenceBlocks.Single().ReferenceBlocks.Single().BlockElements[2]).EffectName, Is.EqualTo("Whatever"));
		}

		[TestCase(BookOrChapter, "mt")]
		[TestCase(Intro, "ip")]
		[TestCase(ExtraBiblical, "s")]
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
			Assert.That(start, Is.EqualTo(result.StartVerse));
			Assert.That(end, Is.EqualTo(result.LastVerseOfBridge));
		}

		[TestCase(2, 3)]
		[TestCase(100, 0)]
		public void AllVerses_NoVerse_ReturnsVerseRepresentingInitialVerseStartAndEnd(int start, int end)
		{
			var block = new Block("p", 1, start, end)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", Narrator)
			}.AddText();
			var result = block.AllVerses.Single();
			Assert.That(start, Is.EqualTo(result.StartVerse));
			Assert.That(end, Is.EqualTo(result.LastVerseOfBridge));
		}

		[Test]
		public void AllVerses_MultipleVersesIncludingStart_ReturnsAllVerses()
		{
			var block = new Block("p", 1, 3)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", Narrator)
			}.AddVerse(3).AddVerse(4).AddVerse("5-6").AddVerse("7-9");
			var result = block.AllVerses.ToList();
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result[0].StartVerse, Is.EqualTo(3));
			Assert.That(result[0].LastVerseOfBridge, Is.EqualTo(0));
			Assert.That(result[1].StartVerse, Is.EqualTo(4));
			Assert.That(result[1].LastVerseOfBridge, Is.EqualTo(0));
			Assert.That(result[2].StartVerse, Is.EqualTo(5));
			Assert.That(result[2].LastVerseOfBridge, Is.EqualTo(6));
			Assert.That(result[3].StartVerse, Is.EqualTo(7));
			Assert.That(result[3].LastVerseOfBridge, Is.EqualTo(9));
		}

		[Test]
		public void AllVerses_DoesNotStartWithVerseNum_HasMidBlockVerse_ReturnsStartAndMidBlockVerses()
		{
			var block = new Block("p", 1, 3)
			{
				BookCode = "MAT",
				CharacterId = GetStandardCharacterId("MAT", Narrator)
			}.AddText("Second half of verse 3").AddVerse(4);
			var result = block.AllVerses.ToList();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0].StartVerse, Is.EqualTo(3));
			Assert.That(result[0].LastVerseOfBridge, Is.EqualTo(0));
			Assert.That(result[1].StartVerse, Is.EqualTo(4));
			Assert.That(result[1].LastVerseOfBridge, Is.EqualTo(0));
		}

		[Test]
		public void GetText_GetBookNameNull_ChapterBlockTextBasedOnStoredText()
		{
			var block = new Block("c", 4);
			block.SetStandardCharacter("MRK", BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.That(block.GetText(true), Is.EqualTo("Chapter 4"));
			Assert.That(block.GetText(false), Is.EqualTo("Chapter 4"));
		}

		[TestCase("c")]
		[TestCase("cl")]
		public void GetText_FormatChapterAnnouncementSet_ChapterBlockTextBasedOnOverride(string chapterStyleTag)
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => chapterNum + (bookId == "MRK" ? " Marky" : " Unknown");
			var block = new Block(chapterStyleTag, 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.That(block.GetText(true), Is.EqualTo("4 Marky"));
			Assert.That(block.GetText(false), Is.EqualTo("4 Marky"));

			block = new Block(chapterStyleTag, 1) { BookCode = "LUK" };
			block.SetStandardCharacter("LUK", BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 1"));

			Assert.That(block.GetText(true), Is.EqualTo("1 Unknown"));
			Assert.That(block.GetText(false), Is.EqualTo("1 Unknown"));
		}

		[TestCase("c")]
		[TestCase("cl")]
		public void GetText_FormatChapterAnnouncementSetButBookCodeNotSet_ChapterBlockTextBasedOnStoredText(string chapterStyleTag)
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => (bookId == null) ? "ARGHHHH!" : "Marky " + chapterNum;
			var block = new Block(chapterStyleTag, 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.That(block.GetText(false), Is.EqualTo("Marky 4"));
			block.BookCode = null;
			Assert.That(block.GetText(false), Is.EqualTo("Chapter 4"));
		}

		[Test]
		public void GetText_FormatChapterAnnouncementReturnsNull_ChapterBlockTextBasedOnStoredText()
		{
			Block.FormatChapterAnnouncement = (bookId, chapterNum) => null;
			var block = new Block("c", 4) { BookCode = "MRK" };
			block.SetStandardCharacter("MRK", BookOrChapter);
			block.BlockElements.Add(new ScriptText("Chapter 4"));

			Assert.That(block.GetText(false), Is.EqualTo("Chapter 4"));
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

			Assert.That(text1 + text2, Is.EqualTo(block.GetText(true)));
		}

		[Test]
		public void GetText_BlockContainsAnnotation_IncludeAnnotationsTrue_ReturnsAllTextIncludingAnnotation()
		{
			const string text1 = "text1 ";
			const string text2 = "text2 ";
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText(text1));
			block.BlockElements.Add(new Sound
			{
				SoundType = SoundType.Sfx,
				EffectName = "effect name",
				UserSpecifiesLocation = true
			});
			block.BlockElements.Add(new ScriptText(text2));

			Assert.That(block.GetText(true, true), Is.EqualTo(
				text1 + "{F8 SFX--effect name} " + text2));
		}

		[Test]
		public void GetAsXml_VerseAndTextElements_XmlHasCorrectAttributesAndAlternatingVerseAndTextElements()
		{
			var block = new Block("p", 4);
			block.SetStandardCharacter("MRK", Narrator);
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			const string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<block style=""p"" chapter=""4"" initialStartVerse=""1"" characterId=""narrator-MRK"">
    <verse num=""1""/>
    <text>Text of verse one. </text>
    <verse num=""2""/>
    <text>Text of verse two.</text>
</block>";

			AssertXmlEqual(expectedXml, block.GetAsXml());
		}

		[Test]
		public void GetAsXml_TextBeginsMidVerse_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3)
			{
				IsParagraphStart = true
			};
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			const string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<block style=""p"" paragraphStart=""true"" chapter=""4"" initialStartVerse=""3"">
    <text>Text of verse three, part two. </text>
    <verse num=""4""/>
    <text>Text of verse four. </text>
    <verse num=""5""/>
    <text>Text of verse five.</text>
</block>";

			AssertXmlEqual(expectedXml, block.GetAsXml());
		}

		[Test]
		public void GetAsXml_VerseBridge_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3, 5)
			{
				IsParagraphStart = true
			};
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4-5"));
			block.BlockElements.Add(new ScriptText("Text of verse four and five."));

			const string expectedXml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<block style=""p"" paragraphStart=""true"" chapter=""4"" initialStartVerse=""3"" initialEndVerse=""5"">
    <text>Text of verse three, part two. </text>
    <verse num=""4-5""/>
    <text>Text of verse four and five.</text>
</block>";

			AssertXmlEqual(expectedXml, block.GetAsXml());
		}

		[Test]
		public void GetTextAsHtml_ContainsCharactersWhichNeedToBeEscaped()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText(@"The dog'cat says, <<Woof!>> & ""Meow."""));

			const string expected = "<div id=\"3\" class=\"scripttext\">The dog&#39;cat says, &lt;&lt;Woof!&gt;&gt; &amp; &quot;Meow.&quot;</div>";
			var actual = block.GetTextAsHtml(true, false);

			Assert.That(actual, Is.EqualTo(expected));
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

			Assert.That(actual, Does.Contain(expect1), $"The output string did not contain: {expect1}");
			Assert.That(actual, Does.Contain(expect2), $"The output string did not contain: {expect2}");
			Assert.That(actual, Does.Contain(expect3), $"The output string did not contain: {expect3}");
			Assert.That(actual, Does.Contain(expect4), $"The output string did not contain: {expect4}");
			Assert.That(actual, Does.Contain(expect5), $"The output string did not contain: {expect5}");
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

			Assert.That(actual, Does.Contain(expected), Format("The output string did not contain: {0}", expected));
		}

		[Test]
		public void SetCharacterAndDelivery_SingleCharacter_SetsCharacterAndDelivery()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { JesusQuestioning });
			Assert.That(block.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(block.Delivery, Is.EqualTo("Questioning"));
		}

		[Test]
		public void SetCharacterAndDelivery_NoCharacters_SetsCharacterToUnknown()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, Array.Empty<CharacterSpeakingMode>());
			Assert.That(block.CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(block.Delivery, Is.Null);
		}

		[Test]
		public void SetCharacterAndDelivery_MultipleCharacters_SetsCharacterToAmbiguous()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes,
				new [] { JesusCommanding, JesusQuestioning, Andrew });
			Assert.That(block.CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(block.Delivery, Is.Null);
		}

		[Test]
		public void SetCharacterAndDelivery_SingleMultipleChoiceCharacter_SetsCharacterAndCharacterIdInScript()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes,
				new[] { new CharacterSpeakingMode("Mary/Martha", null, null, false),  });
			Assert.That(block.CharacterId, Is.EqualTo("Mary/Martha"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("Mary"));
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
			Assert.That(block.CharacterId, Is.EqualTo("Mary/Martha"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("Martha"));
		}

		[Test]
		public void SetCharacterAndDelivery_SettingToSameMultiCharacterWithNoCharacterIdInScriptSet_CharacterIdInScriptGetsSet()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Mary/Martha";
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { new CharacterSpeakingMode("Mary/Martha", null, null, false) });
			Assert.That(block.CharacterId, Is.EqualTo("Mary/Martha"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("Mary"));
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
			Assert.That(block.CharacterId, Is.EqualTo("Mary/Martha/Jews"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("Martha"));
		}

		[Test]
		public void IsStandardCharacter_BiblicalCharacter_ReturnsFalse()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes, new[] { JesusQuestioning });
			Assert.That(block.CharacterIsStandard, Is.False);
		}

		[Test]
		public void IsStandardCharacter_Narrator_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("MRK", Narrator);
			Assert.That(block.CharacterIsStandard, Is.True);
		}

		[Test]
		public void IsStandardCharacter_ExtraBiblical_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("GEN", ExtraBiblical);
			Assert.That(block.CharacterIsStandard, Is.True);
		}

		[Test]
		public void IsStandardCharacter_BookOrChapter_ReturnsTrue()
		{
			var block = new Block("c", 4);
			block.BlockElements.Add(new ScriptText("4"));
			block.SetStandardCharacter("REV", BookOrChapter);
			Assert.That(block.CharacterIsStandard, Is.True);
		}

		[Test]
		public void IsStandardCharacter_Intro_ReturnsTrue()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.SetStandardCharacter("ROM", Intro);
			Assert.That(block.CharacterIsStandard, Is.True);
		}

		[Test]
		public void LastVerseNum_Intro_ReturnsZero()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.That(block.LastVerseNum, Is.EqualTo(0));
		}

		[Test]
		public void LastVerseNum_ScriptureBlockWithSingleStartVerseAndNoVerseElements_ReturnsInitialStartVerse()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.That(block.LastVerseNum, Is.EqualTo(15));
		}

		[Test]
		public void LastVerseNum_ScriptureBlockStartingWithVerseBridgeAndNoVerseElements_ReturnsInitialEndVerse()
		{
			var block = new Block("ip", 3, 15, 17);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.That(block.LastVerseNum, Is.EqualTo(17));
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
			Assert.That(block.LastVerseNum, Is.EqualTo(17));
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
			Assert.That(block.LastVerseNum, Is.EqualTo(19));
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NotMultipleChoice_DoNothing()
		{
			var block = new Block("p", 4, 38)
			{
				CharacterId = "disciples",
				CharacterIdInScript = "Peter (Simon)"
			};
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("Peter (Simon)"));
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NoExplicitDefault_UseFirst()
		{
			var block = new Block("p", 40, 8) { CharacterId = "chief cupbearer/chief baker" };
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("chief cupbearer"));
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_ExplicitDefault_UseDefault()
		{
			var block = new Block("p", 9, 11) { CharacterId = "Peter (Simon)/James/John" };
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("John"));
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_AlreadySetToAnotherValue_OverwriteWithDefault()
		{
			var block = new Block("p", 40, 8)
			{
				CharacterId = "chief cupbearer/chief baker",
				CharacterIdInScript = "chief baker"
			};
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("chief cupbearer"));
		}

		[TestCase(kAmbiguousCharacter)]
		[TestCase(kUnexpectedCharacter)]
		public void SetCharacterAndCharacterIdInScript_CharacterIdSetUnclear_CharacterIdInScriptSetToNull(string unclearCharacterId)
		{
			var block = new Block("p", 40, 8)
			{
				CharacterIdInScript = "chief monkey",
				CharacterId = "chief monkey"
			};
			Assert.That(block.CharacterId, Is.EqualTo("chief monkey"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("chief monkey"));
			Assert.That(block.CharacterIdOverrideForScript, Is.EqualTo("chief monkey"));
			// end setup

			block.SetCharacterIdAndCharacterIdInScript(unclearCharacterId, BCVRef.BookToNumber("EXO"));
			Assert.That(unclearCharacterId, Is.EqualTo(block.CharacterId));
			Assert.That(unclearCharacterId, Is.EqualTo(block.CharacterIdInScript));
			Assert.That(block.CharacterIdOverrideForScript, Is.Null);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoice_CharacterIdInScriptRemainsNull()
		{
			var block = new Block("p", 40, 8);
			block.SetCharacterIdAndCharacterIdInScript("chief monkey", BCVRef.BookToNumber("EXO"));
			Assert.That(block.CharacterId, Is.EqualTo("chief monkey"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("chief monkey"));
			Assert.That(block.CharacterIdOverrideForScript, Is.Null);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoiceCharacterIdInScriptAlreadySet_CharacterIdInScriUnchanged()
		{
			var block = new Block("p", 40, 8)
			{
				CharacterId = "live frog",
				CharacterIdInScript = "dead frog"
			};
			block.SetCharacterIdAndCharacterIdInScript("subordinate monkey", BCVRef.BookToNumber("REV"));
			Assert.That(block.CharacterId, Is.EqualTo("subordinate monkey"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("dead frog"));
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NoChangeToCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_NoChange()
		{
			var block = new Block("p", 40, 8)
			{
				CharacterId = "chief cupbearer/chief baker",
				CharacterIdInScript = "dead frog"
			};
			block.SetCharacterIdAndCharacterIdInScript("chief cupbearer/chief baker", BCVRef.BookToNumber("GEN"));
			Assert.That(block.CharacterId, Is.EqualTo("chief cupbearer/chief baker"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("dead frog"));
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ControlFileHasOverriddenDefault_VersificationShift_CharacterIdInScriptBasedOnOverride()
		{
			// MRK 9:10 in the Vulgate should translate to 9:11 in the "original"
			// The control file overrides the default speaker in MRK 9:11 to be John.
			var block = new Block("p", 9, 10);
			block.SetCharacterIdAndCharacterIdInScript("Peter (Simon)/James/John", BCVRef.BookToNumber("MRK"), m_testVersification);
			Assert.That(block.CharacterId, Is.EqualTo("Peter (Simon)/James/John"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("John"));
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ChangeCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_CharacterIdInScriptChanged()
		{
			var block = new Block("p", 40, 8)
			{
				CharacterId = "chief cupbearer/chief baker",
				CharacterIdInScript = "chief cupbearer"
			};
			block.SetCharacterIdAndCharacterIdInScript("David/Goliath", BCVRef.BookToNumber("GEN"));
			Assert.That(block.CharacterId, Is.EqualTo("David/Goliath"));
			Assert.That(block.CharacterIdInScript, Is.EqualTo("David"));
		}

		[Test]
		public void SerializeDeserialize_ContainsScriptAnnotations_RoundtripDataRemainsTheSame()
		{
			var block = new Block
			{
				BlockElements = new List<BlockElement>
				{
					new ScriptText("script text"),
					new Sound { SoundType = SoundType.Sfx, EffectName = "effect name", StartVerse = 2 },
					new Verse("2"),
					new ScriptText("script text 2"),
				}
			};

			var blockBefore = block.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(block);
			xmlString.AssertHasXPathMatchCount("/block/sound", 1);
			var blockAfter = XmlSerializationHelper.DeserializeFromString<Block>(xmlString);
			Assert.That(blockBefore.GetText(true, true), Is.EqualTo(blockAfter.GetText(true, true)));
		}

		[Test]
		public void SerializeDeserialize_ContainsPrimaryReferenceText_RoundtripDataRemainsTheSame()
		{
			var block = new Block
			{
				BlockElements = new List<BlockElement> { new ScriptText("script text"), }
			};
			var primaryReferenceTextBlock = new Block
			{
				BlockElements = new List<BlockElement> { new ScriptText("primary reference text") }
			};
			block.SetMatchedReferenceBlock(primaryReferenceTextBlock);

			var blockBefore = block.Clone();
			var xmlString = XmlSerializationHelper.SerializeToString(block);
			xmlString.AssertHasXPathMatchCount("/block/ReferenceBlocks/text[text()='primary reference text']", 1);
			var blockAfter = XmlSerializationHelper.DeserializeFromString<Block>(xmlString);
			Assert.That(blockBefore.GetText(true, true), Is.EqualTo(blockAfter.GetText(true, true)));
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

			Assert.That(vernBlock.GetPrimaryReferenceText(), Is.EqualTo(""));
			var refBlockFrench = vernBlock.ReferenceBlocks.Single();
			Assert.That(refBlockFrench.GetPrimaryReferenceText(), Is.EqualTo(""));
			var refBlockPortuguese = refBlockFrench.ReferenceBlocks.Single();
			Assert.That(refBlockPortuguese.GetPrimaryReferenceText(), Is.EqualTo(""));
			var refBlockEnglish = refBlockPortuguese.ReferenceBlocks.Single();
			Assert.That(refBlockEnglish.MatchesReferenceText, Is.False);
			Assert.That(refBlockEnglish.ReferenceBlocks, Is.Empty);
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

			var joinedFrenchRefBlock = new Block(refBlockNarratorFrench.StyleTag,
				refBlockNarratorFrench.ChapterNumber, refBlockNarratorFrench.InitialStartVerseNumber)
			{
				CharacterId = narrator,
				Delivery = "raspy"
			};
			ReferenceText rt = CreateCustomReferenceText(TestReferenceTextResource.FrenchMAT);
			joinedFrenchRefBlock.AppendJoinedBlockElements(new List<Block> { refBlockNarratorFrench, refBlockMatthewFrench }, rt);
			Assert.That(joinedFrenchRefBlock.GetText(true),
				Is.EqualTo("{2}\u00A0Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»"));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.That(joinedFrenchRefBlock.BlockElements.Count, Is.EqualTo(2));
			Assert.That(((Verse)joinedFrenchRefBlock.BlockElements[0]).Number, Is.EqualTo("2"));
			Assert.That(((ScriptText)joinedFrenchRefBlock.BlockElements[1]).Content,
				Is.EqualTo("Jésus a dit. Pour que Matthieu a répondu, «Nous savions que.»"));

			Assert.That(joinedFrenchRefBlock.MatchesReferenceText, Is.True);
			var portugueseRefBlock = joinedFrenchRefBlock.ReferenceBlocks.Single();

			Assert.That(portugueseRefBlock.GetText(true),
				Is.EqualTo("{2}\u00A0disse Jesus. Para que Matthew respondeu: “Sabíamos que isso.”"));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.That(portugueseRefBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(portugueseRefBlock.Delivery, Is.EqualTo("raspy"));
			Assert.That(portugueseRefBlock.BlockElements.Count, Is.EqualTo(2));
			Assert.That(((Verse)portugueseRefBlock.BlockElements[0]).Number, Is.EqualTo("2"));
			Assert.That(((ScriptText)portugueseRefBlock.BlockElements[1]).Content,
				Is.EqualTo("disse Jesus. Para que Matthew respondeu: “Sabíamos que isso.”"));

			Assert.That(portugueseRefBlock.MatchesReferenceText, Is.True);
			var englishRefBlock = portugueseRefBlock.ReferenceBlocks.Single();

			Assert.That(englishRefBlock.GetText(true), Is.EqualTo("{2}\u00A0said Jesus. To which Matthew replied, “We knew that.”"));
			// We may not technically really care too much about the next four lines (at least right now),
			// but this is how we expect the reference block to be built.
			Assert.That(englishRefBlock.CharacterId, Is.EqualTo(narrator));
			Assert.That(englishRefBlock.Delivery, Is.EqualTo("raspy"));
			Assert.That(englishRefBlock.BlockElements.Count, Is.EqualTo(2));
			Assert.That(((Verse)englishRefBlock.BlockElements[0]).Number, Is.EqualTo("2"));
			Assert.That(((ScriptText)englishRefBlock.BlockElements[1]).Content,
				Is.EqualTo("said Jesus. To which Matthew replied, “We knew that.”"));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_VerseBridgeAtStart_RefBlockGetsStartingAndEndingVerseNumbersFromBridgeInText(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var refBlock = block.SetMatchedReferenceBlock("{3-6}" + separator + "Text of verses three through six.");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(6));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_PreviousReferenceBlockWithVerses_RefBlockGetsStartingAndEndingVerseNumbersFromPreviousReferenceBlock(string separator)
		{
			var block = new Block("p", 3, 42, 45);
			var prevRefBlock = new Block("p", 3, 42, 45).AddVerse("42-45", "Initial stuff").AddVerse(46, "Later stuff").AddVerse("47-48", "Final stuff. ");
			var refBlock = block.SetMatchedReferenceBlock("Rest of forty-seven and forty-eight. {49-50}" + separator + "Contents of verses forty-nine through fifty.", prevRefBlock);
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(47));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(48));
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
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(47));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(48));
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
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(47));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(48));
		}

		[TestCase("\u00A0")]
		[TestCase(" ")]
		[TestCase("")]
		public void SetMatchedReferenceBlock_StartAndEndVerseNumbersSeparatedByComma_CommaReplacedByDash(string separator)
		{
			var block = new Block("p", 3, 1).AddVerse(1, "This is verse one. ").AddVerse(2, "This is verse two.");
			var refBlock = block.SetMatchedReferenceBlock("he said. {2}" + separator + "Verse two. {3,6}" + separator + "Text of verses three through six.");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(refBlock.BlockElements.OfType<Verse>().Skip(1).Single().Number, Is.EqualTo("3-6"));
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
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(((Verse)refBlock.BlockElements.Last()).Number, Is.EqualTo("3"));
		}

		[Test]
		public void SetMatchedReferenceBlock_ContainsInitialEndVerse_RefBlockInitialEndVerseSetBackToZero()
		{
			var block = new Block("p", 3, 2, 3).AddVerse("2-3", "This is verses two and three. ");
			var refBlock = block.SetMatchedReferenceBlock("Text of verse two. {3}Text of verse three.");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
		}

		[TestCase("", "\u00A0")]
		[TestCase("\u00A0", "\u00A0")]
		[TestCase("", " ")]
		[TestCase(" ", " ")]
		[TestCase("", "")]
		public void SetMatchedReferenceBlock_TwoContiguousVerseNumbers_OnlyRetainLastVerseNumber(string separatorBetweenVerses, string separatorAfterSecondVerse)
		{
			var block = new Block("p", 3, 2, 3).AddVerse("2-3", "This is verses two and three. ");
			var refBlock = block.SetMatchedReferenceBlock("{2}" + separatorBetweenVerses + "{3}" + separatorAfterSecondVerse + "Text of verse three.");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(refBlock.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("3"));
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
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(refBlock.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("2"));
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.That(effect.EffectName, Is.EqualTo("Sneezing"));
			Assert.That(refBlock.BlockElements.OfType<ScriptText>().First().Content, Is.EqualTo("Text of verse "));
			Assert.That(refBlock.BlockElements.OfType<ScriptText>().Last().Content, Is.EqualTo(" three."));
		}

		[TestCase(Sound.kNonSpecificStartOrStop)]
		[TestCase(0)]
		public void SetMatchedReferenceBlock_ContainsMusicStart_AnnotationParsedAndIncludedAsBlockElement(int startVerse)
		{
			var block = new Block("p", 3, 2).AddVerse("2", "This is verse two.");
			var music = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = startVerse, EndVerse = 0 };
			var refBlock = block.SetMatchedReferenceBlock("{2} Text of verse " + music.ToDisplay() + "three.");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(refBlock.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("2"));
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.That(SoundType.Music, Is.EqualTo(effect.SoundType));
			Assert.That(startVerse, Is.EqualTo(effect.StartVerse));
			Assert.That(effect.EndVerse, Is.EqualTo(0));
			Assert.That(effect.UserSpecifiesLocation, Is.True);
			Assert.That(effect.EffectName, Is.Null);
			Assert.That(refBlock.BlockElements.OfType<ScriptText>().First().Content, Is.EqualTo("Text of verse "));
			Assert.That(refBlock.BlockElements.OfType<ScriptText>().Last().Content, Is.EqualTo(" three."));
		}

		[TestCase(Sound.kNonSpecificStartOrStop)]
		[TestCase(0)]
		public void SetMatchedReferenceBlock_OnlyAnnotation_AnnotationParsed(int startVerse)
		{
			var block = new Block("p", 3, 2).AddVerse("2", "This is verse two.");
			var music = new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true, StartVerse = startVerse, EndVerse = 0 };
			var refBlock = block.SetMatchedReferenceBlock(music.ToDisplay());
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(refBlock.BlockElements.OfType<Verse>(), Is.Empty);
			var effect = refBlock.BlockElements.OfType<Sound>().Single();
			Assert.That(SoundType.Music, Is.EqualTo(effect.SoundType));
			Assert.That(startVerse, Is.EqualTo(effect.StartVerse));
			Assert.That(effect.UserSpecifiesLocation, Is.True);
			Assert.That(effect.EffectName, Is.Null);
			Assert.That(refBlock.BlockElements.OfType<ScriptText>(), Is.Empty);
		}

		[Test]
		public void SetMatchedReferenceBlock_VernBlockHasCharacter_AnnotationParsedAndIncludedAsBlockElement()
		{
			var block = new Block("p", 8, 29).AddVerse("29",
				"“¡No te metas con nosotros, Hijo de Dios! ¿Viniste acá para atormentarnos antes de tiempo?”");
			block.SetCharacterIdAndCharacterIdInScript(
				"demons (Legion)/man delivered from Legion of demons", 40, m_testVersification);
			Assert.That(block.CharacterIdOverrideForScript, Is.EqualTo("demons (Legion)"));
			var refBlock = block.SetMatchedReferenceBlock("{29} “What do we have to do with " +
				"you, Jesus, Son of God? Have you come here to torment us before the time?”");
			Assert.That(block.MatchesReferenceText, Is.True);
			Assert.That(refBlock, Is.EqualTo(block.ReferenceBlocks.Single()));
			Assert.That(refBlock.InitialStartVerseNumber, Is.EqualTo(29));
			Assert.That(refBlock.InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(refBlock.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("29"));
			Assert.That(refBlock.CharacterId, Is.EqualTo(
				"demons (Legion)/man delivered from Legion of demons"));
			Assert.That(refBlock.CharacterIdInScript, Is.EqualTo("demons (Legion)"));
			Assert.That(refBlock.CharacterIdOverrideForScript, Is.EqualTo("demons (Legion)"));
		}

		[Test]
		public void RemoveVerseNumbers_EmptyCollection_NoChange()
		{
			var block = new Block("p", 8, 29)
				.AddVerse("29", "Verse 29 text. ")
				.AddVerse("30", "Verse 30 text.");
			block.BlockElements.Add(new Sound {EndVerse = 31});
			var expected = block.GetText(true, true);

			block.RemoveVerseNumbers(Array.Empty<Verse>());
			Assert.That(block.GetText(true, true), Is.EqualTo(expected));
		}

		[Test]
		public void RemoveVerseNumbers_CollectionContainsVersesNotInBlock_NoChange()
		{
			var block = new Block("p", 8, 29)
				.AddVerse("29", "Verse 29 text. ")
				.AddVerse("30", "Verse 30 text.");
			block.BlockElements.Add(new Sound {EndVerse = 31});
			var expected = block.GetText(true, true);

			block.RemoveVerseNumbers(new [] {new Verse("42"), new Verse("43")});
			Assert.That(block.GetText(true, true), Is.EqualTo(expected));
		}

		[Test]
		public void RemoveVerseNumbers_CollectionContainsOneVerseInBlock_VerseRemovedAndAdjacentTextJoinedIntoSingleElement()
		{
			var block = new Block("p", 8, 29)
				.AddVerse("29", "Verse 29 text. ")
				.AddVerse("30", "Verse 30 text.");

			block.RemoveVerseNumbers(new [] {new Verse("30")});
			Assert.That(block.GetText(true, true), Is.EqualTo("{29}\u00A0Verse 29 text. Verse 30 text."));
		}

		[Test]
		public void RemoveVerseNumbers_CollectionContainsAllVersesInBlock_VersesRemovedAndAdjacentTextJoinedIntoSingleElement()
		{
			var block = new Block("p", 8, 29)
				.AddVerse("29", "Verse 29 text. ")
				.AddVerse("30", "Verse 30 text.");

			block.RemoveVerseNumbers(new [] {new Verse("29"), new Verse("30")});
			Assert.That(block.GetText(true, true), Is.EqualTo("Verse 29 text. Verse 30 text."));
		}

		[Test]
		public void RemoveVerseNumbers_CollectionContainsSingleStartingVerse_VerseRemoved()
		{
			var block = new Block("p", 8, 29)
				.AddVerse("29", "Verse 29 text. ");

			block.RemoveVerseNumbers(new [] {new Verse("29"), new Verse("30")});
			Assert.That(block.GetText(true, true), Is.EqualTo("Verse 29 text. "));
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNoVerseNumber_VernHasLaterVerse_LeadingVerseStaysWithRowA(string separator)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("q", 8, 20).AddText("More verse 20 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, 0, ScrVers.English, 
				"{19}"+  separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{19}" + separator + "This is another chunk of some verse."));
			Assert.That(newRowBValue, Is.EqualTo("Cool. {20}" + separator + "Fine"));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(1, true)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBIsEmpty_VernHasCorrespondingVerse_LeadingVerseStaysWithRowA(
			int vernRowCorrespondingToA, bool includeSectionHead = false)
		{
			var vernBlocks = new List<Block>();
			if (includeSectionHead)
			{
				vernBlocks.Add(new Block("s", 8, 18)
				{
					CharacterId = GetStandardCharacterId("MAT", ExtraBiblical),
					BlockElements = new List<BlockElement>(new [] { new ScriptText("Section head text") })
				});
			}
			vernBlocks.Add(new Block("p", 8, 19).AddVerse("19", "Start of verse 19 text."));
			vernBlocks.Add(new Block("p", 8, 19).AddText("Rest of verse 19.")
				.AddVerse("20", "Verse 20 text.").AddVerse("21", "Verse 21 text."));
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, vernRowCorrespondingToA, ScrVers.English,
				"{19}\u00A0Cool and fine", "",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{19}\u00A0"));
			Assert.That(newRowBValue, Is.EqualTo("Cool and fine"));
		}

		[TestCase("NUM", 13, 3, 4 /* 2-3 in English */, "2-3", ScrVersType.RussianOrthodox)]
		[TestCase("NUM", 13, 2, 3 /* 1-2 in English */, "2-3", ScrVersType.RussianOrthodox)]
		[TestCase("NUM", 13, 3, 4 /* 2-3 in English */, "2", ScrVersType.RussianOrthodox)]
		[TestCase("NUM", 13, 3, 5 /* 2-4 in English */, "4-5", ScrVersType.RussianProtestant)]
		[TestCase("NUM", 13, 4, 5 /* 3-4 in English */, "4", ScrVersType.RussianProtestant)]
		[TestCase("NUM", 13, 3, 5 /* 2-4 in English */, "3-4", ScrVersType.RussianProtestant)]
		[TestCase("NUM", 13, 3, 7 /* 2-6 in English */, "3-4", ScrVersType.RussianOrthodox)]
		public void GetSwappedReferenceText_VerseRangesWithPartialOverlapBetweenRefAndVern_LeadingVerseStaysWithRowA(
			string bookId, int chapter, int vernStartVerse, int vernEndVerse, string refVerseRange, ScrVersType vernVersification)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", chapter, vernStartVerse, vernEndVerse) {BookCode = bookId}
					.AddVerse($"{vernStartVerse}-{vernEndVerse}", "Gobbledygook and monkey soup."),
				new Block("p", chapter, vernStartVerse, vernEndVerse) {BookCode = bookId}
					.AddText("Zubbety lubble fmoodic'ka.")
					.AddVerse(vernEndVerse + 1, "Some more text for the next verse.")
			};
			for (int vernRowCorrespondingToA = 0; vernRowCorrespondingToA < 2; vernRowCorrespondingToA++)
			{
				Block.GetSwappedReferenceText(vernBlocks, bookId, chapter,
					vernRowCorrespondingToA, new ScrVers(vernVersification),
					$"{{{refVerseRange}}} Cool and fine", "",
					out var newRowAValue, out var newRowBValue);
				Assert.That($"{{{refVerseRange}}}\u00A0", Is.EqualTo(newRowAValue));
				Assert.That(newRowBValue, Is.EqualTo("Cool and fine"));
			}
		}

		[TestCase("NUM", 13, 12, 1 /* 12:16 in English */, 16, ScrVersType.RussianOrthodox)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBIsEmpty_VernHasCorrespondingVerseInDifferentChapterInDifferentVersification_LeadingVerseStaysWithRowA(
			string bookId, int vernChapter, int refChapter, int vernVerse, int refVerse, ScrVersType vernVersification)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", vernChapter, vernVerse) {BookCode = bookId}
					.AddVerse($"{vernVerse}", "Gobbledygook and monkey soup."),
				new Block("p", vernChapter, vernVerse) {BookCode = bookId}
					.AddText("Zubbety lubble fmoodic'ka.")
			};
			for (int vernRowCorrespondingToA = 0; vernRowCorrespondingToA < 2; vernRowCorrespondingToA++)
			{
				Block.GetSwappedReferenceText(vernBlocks, bookId, refChapter,
					vernRowCorrespondingToA, new ScrVers(vernVersification),
					$"{{{refVerse}}} Cool and fine", "",
					out var newRowAValue, out var newRowBValue);
				Assert.That($"{{{refVerse}}}\u00A0", Is.EqualTo(newRowAValue));
				Assert.That(newRowBValue, Is.EqualTo("Cool and fine"));
			}
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBIsEmpty_VernHasCorrespondingVerseInDifferentVersification_LeadingVerseStaysWithRowA(
			int vernRowCorrespondingToA)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 2, 6).AddVerse("6", "Start of verse 6 (verse 2 in English) text."),
				new Block("p", 2, 6).AddText("Rest of verse 6 (2 in en).")
					.AddVerse("7", "Verse 7(3) text.").AddVerse("8", "Verse 8(4) text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "ZEC", 2, vernRowCorrespondingToA, ScrVers.Original,
				"{2}\u00A0Cool and fine", "",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{2}\u00A0"));
			Assert.That(newRowBValue, Is.EqualTo("Cool and fine"));
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNoVerseNumber_VernHasCorrespondingVerseAbove_LeadingVerseStaysWithRowA(string separator)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, 1, ScrVers.English,
				"{19}"+  separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{19}" + separator + "This is another chunk of some verse."));
			Assert.That(newRowBValue, Is.EqualTo("Cool. {20}" + separator + "Fine"));
		}

		[TestCase("")]
		[TestCase(" ")]
		[TestCase("\u00A0")]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNoVerseNumber_VernHasCorrespondingVerseAtSameRow_LeadingVerseStaysWithRowA(string separator)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, 0, ScrVers.English,
				"{19}"+  separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{19}" + separator + "This is another chunk of some verse."));
			Assert.That(newRowBValue, Is.EqualTo("Cool. {20}" + separator + "Fine"));
		}

		[TestCase("", 0)]
		[TestCase(" ", 1)]
		[TestCase("\u00A0", 1)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNoVerseNumber_VernHasCorrespondingVerseSubsequentRow_LeadingVerseMovesWithRowAContents(
			string separator, int currentRow)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 17).AddVerse("17", "Verse 17 text."),
				new Block("p", 8, 18).AddVerse("18", "Verse 18 text."),
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, currentRow, ScrVers.English,
				"{19}"+  separator + "Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("This is another chunk of some verse."));
			Assert.That(newRowBValue, Is.EqualTo("{19}" + separator + "Cool. {20}" + separator + "Fine"));
		}

		[TestCase("", 0)]
		[TestCase(" ", 2)]
		[TestCase("\u00A0", 3)]
		public void GetSwappedReferenceText_RowAHasNonLeadingVerseNumber_RowBHasNoVerseNumber_EntireContentsSwap(
			string separator, int currentRow)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 17).AddVerse("17", "Verse 17 text."),
				new Block("p", 8, 18).AddVerse("18", "Verse 18 text."),
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, currentRow, ScrVers.English,
				"Cool. {20}" + separator + "Fine", "This is another chunk of some verse.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("This is another chunk of some verse."));
			Assert.That(newRowBValue, Is.EqualTo("Cool. {20}" + separator + "Fine"));
		}

		[TestCase("", 0)]
		[TestCase(" ", 2)]
		[TestCase("\u00A0", 3)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasNonLeadingVerseNumber_LeadingVerseStaysWithRowA(
			string separator, int currentRow)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 17).AddVerse("17", "Verse 17 text."),
				new Block("p", 8, 18).AddVerse("18", "Verse 18 text."),
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, currentRow, ScrVers.English,
				"Cool. {20}" + separator + "Fine", "This is another chunk of some verse. {21}" +
				separator + "Verse twenty-one.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo(
				"This is another chunk of some verse. {21}" + separator + "Verse twenty-one."));
			Assert.That(newRowBValue, Is.EqualTo("Cool. {20}" + separator + "Fine"));
		}

		[TestCase("", 0)]
		[TestCase(" ", 2)]
		[TestCase("\u00A0", 3)]
		public void GetSwappedReferenceText_RowAHasLeadingVerseNumber_RowBHasLeadingVerseNumber_EntireContentsSwap(
			string separator, int currentRow)
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 17).AddVerse("17", "Verse 17 text."),
				new Block("p", 8, 18).AddVerse("18", "Verse 18 text."),
				new Block("p", 8, 19).AddVerse("19", "Verse 19 text."),
				new Block("p", 8, 20).AddVerse("20", "Verse 20 text."),
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, currentRow, ScrVers.English,
				"{19}" + separator + "Cool. {20}" + separator + "Fine", "{21}" + separator + "Verse twenty-one.",
				out var newRowAValue, out var newRowBValue);
			Assert.That(newRowAValue, Is.EqualTo("{21}" + separator + "Verse twenty-one."));
			Assert.That(newRowBValue, Is.EqualTo("{19}" + separator + "Cool. {20}" + separator + "Fine"));
		}

		[Test]
		public void GetSwappedReferenceText_RowAIsNull_EntireContentsSwap()
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text. "),
				new Block("p", 8, 21).AddText("Blah.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, 0, ScrVers.English, null, "{21} Verse twenty-one.",
				out string newRowAValue, out string newRowBValue);
			
			Assert.That(newRowAValue, Is.EqualTo("{21} Verse twenty-one."));
			Assert.That(IsNullOrEmpty(newRowBValue), Is.True);
		}

		[Test]
		public void GetSwappedReferenceText_RowBIsNull_EntireContentsSwap()
		{
			var vernBlocks = new List<Block>
			{
				new Block("p", 8, 21).AddVerse("21", "Verse 21 text. "),
				new Block("p", 8, 21).AddText("Blah.")
			};
			Block.GetSwappedReferenceText(vernBlocks, "MAT", 8, 0, ScrVers.English, "{21} Verse twenty-one.", null,
				out string newRowAValue, out string newRowBValue);

			Assert.That(IsNullOrEmpty(newRowAValue), Is.True);
			Assert.That(newRowBValue, Is.EqualTo("{21} Verse twenty-one."));
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
			Assert.That(text.Length, Is.EqualTo(block.Length));
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
			Assert.That(text.Length, Is.EqualTo(block.Length));
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
			Assert.That(text1.Length + text2.Length, Is.EqualTo(block.Length));
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
			Assert.That(chapterAnnouncementBlock.Length, Is.EqualTo("MAT 1".Length));

			// If the formatting Func returns null, we get the text from the ScriptText element
			Block.FormatChapterAnnouncement = (s, i) => null;
			Assert.That(text.Length, Is.EqualTo(chapterAnnouncementBlock.Length));

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
			Assert.That(block.ScriptTextCount, Is.EqualTo(1));
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
			Assert.That(block.ScriptTextCount, Is.EqualTo(1));
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
			Assert.That(block.ScriptTextCount, Is.EqualTo(2));
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
			Assert.That(block.ScriptTextCount, Is.EqualTo(2));
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

			Assert.That(block.GetText(true), Is.EqualTo("{1b}\u00A0abc"));
			Assert.That(newBlock.GetText(true), Is.EqualTo("def ghi"));
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

			Assert.That(block.GetText(true), Is.EqualTo("{1}\u00A0abcdef ghi {2-3}\u00A0jk l"));
			Assert.That(newBlock.GetText(true), Is.EqualTo("mno p"));
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

			Assert.That(block.GetText(true), Is.EqualTo("({1}\u00A0abc"));
			Assert.That(newBlock.GetText(true), Is.EqualTo("def ghi) {2}\u00A0jk lmno p"));
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

			Assert.That(block.GetText(true), Is.EqualTo("({2}\u00A0a"));
			Assert.That(block.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(newBlock.GetText(true), Is.EqualTo("bcdef ghi) {3}\u00A0jk lmno p"));
			Assert.That(newBlock.InitialStartVerseNumber, Is.EqualTo(2));
		}

		/// <summary>
		/// PG-1311: Test that the initial start verse number for the new block is set correctly (based on first actual verse element)
		/// </summary>
		[TestCase("[")] // This is the likely case
		[TestCase(".")] // This would probably be a mistake (see ENHANCE comment in SplitBlockDlg.DetermineSplitLocation),
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

			Assert.That(block.GetText(true), Is.EqualTo("{2}\u00A0abcdef ghi"));
			Assert.That(block.InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(newBlock.GetText(true), Is.EqualTo(
				trailingPunctuation + " {3}\u00A0jk lmno p"));
			Assert.That(newBlock.InitialStartVerseNumber, Is.EqualTo(3));
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

			Assert.That(block.GetText(true), Is.EqualTo("({2}\u00A0abcdef ghi) "));
			Assert.That(newBlock.GetText(true), Is.EqualTo("{3}\u00A0jk lmno p"));
		}

		[TestCase("-a-")]
		[TestCase(" -a- ")]
		[TestCase("—a—")]
		[TestCase("—a— ")]
		[TestCase("(a)")]
		[TestCase(" (a)")]
		[TestCase("[a]")]
		[TestCase(" [a]")]
		[TestCase("[a]»")]
		[TestCase("(a)»")]
		public void ProbablyIsNotAnInterruption_LikelyInterruptions_ReturnsFalse(string text)
		{
			var block = GetBlockWithText(text);
			Assert.That(block.ProbablyIsNotAnInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes), Is.False);
		}

		[TestCase("(a) starts with parentheses; possibly CONTAINS an interruption.")]
		[TestCase(" (a) starts with parentheses; possibly CONTAINS an interruption.")]
		[TestCase("“(a) starts with parentheses; possibly CONTAINS an interruption.”")]
		[TestCase(" [a] b c")]
		[TestCase("«[a] b c»")]
		[TestCase("Somebody talking and then -a-")]
		[TestCase("Somebody talking and then—a—")]
		[TestCase("Somebody talking and then(a)")]
		[TestCase("‘Somebody talking and then (a)")]
		[TestCase("Somebody talking and then (a) there was an interruption.")]
		[TestCase("Somebody talking and then[a]")]
		[TestCase("Somebody talking and then [a] there was an interruption")]
		[TestCase("«Somebody talking and then[a] there was an interruption»")]
		[TestCase(" -a- there was an interruption.")]
		[TestCase("—a—there was an interruption.")]
		[TestCase(" -a- there was an interruption.")]
		[TestCase(" —a— there was an interruption.")]
		[TestCase("‘(a)")]
		[TestCase("“(a)”")]
		[TestCase("“[a]”")]
		public void ProbablyIsNotAnInterruption_ApparentNonInterruptions_ReturnsTrue(string text)
		{
			var block = GetBlockWithText(text);
			Assert.That(block.ProbablyIsNotAnInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes), Is.True);
		}

		[Test]
		public void ProbablyIsNotAnInterruption_PossibleInterruptionUsesLongDashesWhichAreAlsoUsedForDialogueQuotes_ReturnsTrue()
		{
			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			var block = GetBlockWithText("—a—");
			Assert.That(block.ProbablyIsNotAnInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes), Is.True);
		}

		[TestCase("(", ")")]
		[TestCase("[", "]")]
		// REVIEW: Currently, we do treat the following as a potential interruption, but that might not
		// be correct. Probably need to wait until some real data comes along to decide for sure.
		//[TestCase("-", "-")]
		public void GetNextInterruption_OnlyOpeningQuoteMarkBeforeInterruptionStart_NoInterruptionFound(string interruptionStart, string interruptionEnd)
		{
			var block = GetBlockWithText($"“{interruptionStart}plus some text{interruptionEnd} is not an interruption.”");
			Assert.That(block.GetNextInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes), Is.Null);

			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			Assert.That(block.GetNextInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes), Is.Null);
		}

		[TestCase("(", ")")]
		[TestCase("[", "]")]
		public void GetNextInterruption_OnlyOpeningQuoteMarkBeforeInterruptionStart_RealInterruptionFound(string interruptionStart, string interruptionEnd)
		{
			var block = GetBlockWithText($"“{interruptionStart}plus some text{interruptionEnd} is not an interruption {interruptionStart}but this is{interruptionEnd}.”");
			Assert.That(block.GetNextInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes).Item1.Value,
				Is.EqualTo($"{interruptionStart}but this is{interruptionEnd}.”"));

			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			Assert.That(block.GetNextInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes).Item1.Value,
				Is.EqualTo($"{interruptionStart}but this is{interruptionEnd}.”"));
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

		[TestCase("a -c d e")]
		[TestCase("a c- d e")]
		[TestCase("a c - d e")]
		[TestCase("a b-c-d e")]
		[TestCase("a b-c d-e")]
		public void GetNextInterruption_WordMedialOrUnmatchedDashes_NoInterruptionFound(string text)
		{
			var block = GetBlockWithText(text);
			Assert.That(block.GetNextInterruption(m_interruptionFinderForQuoteSystemWithoutLongDashDialogueQuotes), Is.Null);

			IQuoteInterruptionFinder interruptionFinderForQuoteSystemWithLongDashDialogueQuotes =
				new QuoteSystem(new QuotationMark("\u2014", "\u2014", null, 1, QuotationMarkingSystemType.Narrative));

			Assert.That(block.GetNextInterruption(interruptionFinderForQuoteSystemWithLongDashDialogueQuotes), Is.Null);
		}

		[TestCase(kAmbiguousCharacter)]
		[TestCase(kUnexpectedCharacter)]
		public void TryMatchToReportingClause_BlockCharacterIsUnclear_ReturnsFalse(string character)
		{
			var block = new Block("p", 1, 2)
			{
				CharacterId = character,
				BookCode = "MAT",
				BlockElements =
				{
					new Verse("2"),
					new ScriptText("el dijo"),
				}
			};
			Assert.That(block.TryMatchToReportingClause(new []{"el dijo"}, ReferenceText.GetStandardReferenceText(English),
				40, ScrVers.English), Is.False);
		}

		[Test]
		public void TryMatchToReportingClause_BlockCharacterIsNarrator_ReturnsTrue()
		{
			var block = new Block("p", 1, 2)
			{
				CharacterId = GetStandardCharacterId("MAT", Narrator),
				BookCode = "MAT",
				BlockElements =
				{
					new Verse("2"),
					new ScriptText("el dijo"),
				}
			};
			Assert.That(block.TryMatchToReportingClause(new []{"el dijo"}, ReferenceText.GetStandardReferenceText(English),
				40, ScrVers.English), Is.True);
		}

		[Test]
		public void TryMatchToReportingClause_BlockCharacterIsNeedsReview_ReturnsTrue()
		{
			var block = new Block("p", 1, 2)
			{
				CharacterId = kNeedsReview,
				BookCode = "MAT",
				BlockElements =
				{
					new Verse("2"),
					new ScriptText("el dijo"),
				}
			};
			Assert.That(block.TryMatchToReportingClause(new []{"el dijo"}, ReferenceText.GetStandardReferenceText(English),
				40, ScrVers.English), Is.True);
			Assert.That(block.CharacterId,
				Is.EqualTo(GetStandardCharacterId("MAT", Narrator)));
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
