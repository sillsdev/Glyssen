﻿using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using GlyssenSharedTests;
using InMemoryTestPersistence;
using NUnit.Framework;
using SIL.Reflection;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GlyssenCharacters;
using SIL.Extensions;
using static System.String;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenEngine.ReferenceText;
using static GlyssenSharedTests.CustomConstraints;

namespace GlyssenEngineTests
{
	[TestFixture]
	class ReferenceTextTests
	{
		private ScrVers m_vernVersification;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = GlyssenCharactersTests.Properties.Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = GlyssenCharactersTests.Properties.Resources.TestCharacterDetailOct2015;

			m_vernVersification = Versification.Table.Implementation.Load(new StringReader(GlyssenCharactersTests.Properties.Resources.TestVersification), "Resources.TestVersification", "Test");
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.ForgetCustomReferenceTexts();
		}

		[TestCase(ReferenceTextType.English, 66)] // Whole Bible
		//[TestCase(ReferenceTextType.Azeri)]
		//[TestCase(ReferenceTextType.French)]
		//[TestCase(ReferenceTextType.Indonesian)]
		//[TestCase(ReferenceTextType.Portuguese)]
		[TestCase(ReferenceTextType.Russian, 27)] // NT only
		//[TestCase(ReferenceTextType.Spanish)]
		//[TestCase(ReferenceTextType.TokPisin)]
		public void GetStandardReferenceText_AllStandardReferenceTextsAreLoadedCorrectly(ReferenceTextType referenceTextType, int numberOfBooks)
		{
			var referenceText = GetStandardReferenceText(referenceTextType);
			Assert.That(referenceText.Books.Count, Is.EqualTo(numberOfBooks));
			Assert.That(referenceText.Versification, Is.EqualTo(ScrVers.English));
		}

		[TestCase(MultiBlockQuote.None, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Start, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.Continuation, MultiBlockQuote.Continuation)]
		public void ApplyTo_SingleBlockOfVernacular_ReferenceTextBrokenByVerse_VernacularGetsBrokenByVerse(
			MultiBlockQuote vernMultiBlockQuote, MultiBlockQuote expectedResultForFirstBlock, MultiBlockQuote expectedResultForSubsequentBlocks)
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
				MultiBlockQuote = vernMultiBlockQuote,
				UserConfirmed = true,
			};
			block.AddVerse(1, "This is versiculo uno.").AddVerse(2, "This is versiculo dos.").AddVerse(3, "This is versiculo tres.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);
			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(1, "This is verse one.");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(2, "This is verse two.");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 3)
			{
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(3, "This is verse three.");
			referenceBlocks.Add(block);
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(3));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result, ForEvery<Block>(b => b.CharacterId, Is.EqualTo("Peter/James/John")));
			Assert.That(result, ForEvery<Block>(b => b.CharacterIdInScript, Is.EqualTo("John")));
			Assert.That(result, ForEvery<Block>(b => b.Delivery, Is.EqualTo("annoyed beyond belief")));
			Assert.That(result.First().MultiBlockQuote, Is.EqualTo(expectedResultForFirstBlock));
			Assert.That(result.Skip(1), ForEvery<Block>(b => b.MultiBlockQuote, Is.EqualTo(expectedResultForSubsequentBlocks)));
			Assert.That(result, ForEvery<Block>(b => b.UserConfirmed, Is.True));
			Assert.That(result, ForEvery<Block>(b => b.SplitId, Is.EqualTo(-1)));
			Assert.That(result.Select(v => v.GetPrimaryReferenceText()),
				Is.EqualTo(referenceBlocks.Select(r => r.GetText(true))));
		}

		[TestCase(MultiBlockQuote.None, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Start, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.Continuation, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.Continuation)]
		public void ApplyTo_SingleBlockOfVernacular_ReferenceTextHasVersePrecededBySquareBracket_VernacularGetsBrokenByVerse(
			MultiBlockQuote vernMultiBlockQuote, MultiBlockQuote expectedResultForFirstBlock, MultiBlockQuote expectedResultForSubsequentBlocks)
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
				MultiBlockQuote = vernMultiBlockQuote,
				UserConfirmed = true,
			};
			block.AddVerse(1, "This is versiculo uno.").AddVerse(2, "This is versiculo dos.").AddVerse(3, "This is versiculo tres.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);
			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(1, "This is verse one.");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(2, "This is verse two.");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 3)
			{
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.AddVerse(3, "This is verse three.");
			referenceBlocks.Add(block);
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(3));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result, ForEvery<Block>(b => b.CharacterId, Is.EqualTo("Peter/James/John")));
			Assert.That(result, ForEvery<Block>(b => b.CharacterIdInScript, Is.EqualTo("John")));
			Assert.That(result, ForEvery<Block>(b => b.Delivery, Is.EqualTo("annoyed beyond belief")));
			Assert.That(result.First().MultiBlockQuote, Is.EqualTo(expectedResultForFirstBlock));
			Assert.That(result.Skip(1), ForEvery<Block>(b => b.MultiBlockQuote, Is.EqualTo(expectedResultForSubsequentBlocks)));
			Assert.That(result, ForEvery<Block>(b => b.UserConfirmed, Is.True));
			Assert.That(result, ForEvery<Block>(b => b.SplitId, Is.EqualTo(-1)));
			Assert.That(result.Select(v => v.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))), Is.True);
		}

		[Test]
		public void ApplyTo_VernacularHasVerseSplitIntoMoreBlocksThanReference_ReferenceTextBrokenByVerse_SubsequentBlocksInVernacularGetBrokenByVerse()
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Fred. ");
			var block = CreateNarratorBlockForVerse(2, "Blah blah. ");
			block.AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "I don't know if Fred told you this or not, but he's crazy. ", true),
				CreateNarratorBlockForVerse(2, "This is your narrator speaking. "),
				CreateNarratorBlockForVerse(3, "I hope you enjoy your flight. "),
				CreateNarratorBlockForVerse(4, "The end. ")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(4));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(result[0].GetText(true), Is.EqualTo("{1}\u00A0Cosas que Fred dice, "));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(0));

			Assert.That(result[1].GetText(true), Is.EqualTo("dijo Fred. "));
			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[2].GetText(true), Is.EqualTo("{2}\u00A0Blah blah. "));
			Assert.That(result[3].GetText(true), Is.EqualTo("{3}\u00A0More blah blah. "));
			Assert.That(result[4].GetText(true), Is.EqualTo("{4}\u00A0The final blah blah."));
			Assert.That(result.Skip(2).Select(v => v.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))), Is.True);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceTextHaveSectionHeadsInDifferentPlaces_SectionHeadReferenceTextsAreNotMatched()
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true) };
			var block = new Block("s", 1, 1)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Section cabeza text"));
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator),
			};
			block.AddVerse(2, "Blah blah. ").AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "I don't know if Fred told you this or not, but he's crazy. ", true),
				CreateNarratorBlockForVerse(2, "This is your narrator speaking. "),
				CreateNarratorBlockForVerse(3, "I hope you enjoy your flight. ")
			};
			block = new Block("s", 1, 3)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Section head text (the English version)"));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "The end."));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(5));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(result[0].GetText(true), Is.EqualTo("{1}\u00A0Cosas que Fred dice, "));
			Assert.That(result[1].GetText(true), Is.EqualTo("Section cabeza text"));
			Assert.That(result[2].GetText(true), Is.EqualTo("{2}\u00A0Blah blah. "));
			Assert.That(result[3].GetText(true), Is.EqualTo("{3}\u00A0More blah blah. "));
			Assert.That(result[4].GetText(true), Is.EqualTo("{4}\u00A0The final blah blah."));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{1}\u00A0I don't know if Fred told you this or not, but he's crazy. "));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[2].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{2}\u00A0This is your narrator speaking. "));
			Assert.That(result[3].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{3}\u00A0I hope you enjoy your flight. "));
			Assert.That(result[4].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{4}\u00A0The end."));
		}

		[Test]
		public void ApplyTo_VernacularHasSectionHeadInTheMiddleOfAVerse_ReferenceTextHasNoSectionHead_NotMatchedAndReferenceTextLinkedToFirstVernacularBlock()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(31, "But eagerly desire the greater gifts.", false, 12, "1CO") };
			var block = new Block("s", 12, 31)
			{
				CharacterId = GetStandardCharacterId("1CO", StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Love"));
			vernacularBlocks.Add(block);
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "And now I will show you...", "1CO").AddVerse(32, "This isn't here.");
			var vernBook = new BookScript("1CO", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(31, "In this version, there is no section head.", false, 12, "1CO"),
				CreateNarratorBlockForVerse(32, "The verse that was never supposed to exist.", false, 12, "1CO")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(2));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].GetText(true), Is.EqualTo("{31}\u00A0But eagerly desire the greater gifts."));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].GetText(true), Is.EqualTo("Love"));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[2].GetText(true), Is.EqualTo("And now I will show you..."));
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[2].MatchesReferenceText, Is.False);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[3].GetText(true), Is.EqualTo("{32}\u00A0This isn't here."));
			Assert.That(result[3].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{32}\u00A0The verse that was never supposed to exist."));
			Assert.That(result[3].MatchesReferenceText);
			Assert.That(result[3].GetPrimaryReferenceText(), Is.EqualTo("{32}\u00A0The verse that was never supposed to exist."));
		}

		[Test]
		public void ApplyTo_VernacularHasMidVerseParagraphBreakFollowedByMoreVerses_ReferenceHasBlocksSplitAtVerseBreaks_AdditionalVerseSplitsHappen()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(31, "But eagerly desire the greater gifts.", false, 12, "1CO") };
			var blockForNewParagraph = AddNarratorBlockForVerseInProgress(vernacularBlocks, "And now I will show you...", "1CO").AddVerse(32, "This isn't here.");
			blockForNewParagraph.IsParagraphStart = true;
			var vernBook = new BookScript("1CO", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(31, "In this version, there is no paragraph break.", false, 12, "1CO"),
				CreateNarratorBlockForVerse(32, "The verse that was never supposed to exist.", false, 12, "1CO")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(2));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].GetText(true), Is.EqualTo("{31}\u00A0But eagerly desire the greater gifts."));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].GetText(true), Is.EqualTo("And now I will show you..."));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[2].GetText(true), Is.EqualTo("{32}\u00A0This isn't here."));
			Assert.That(result[2].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));
		}

		[Test]
		public void ApplyTo_MultipleBlocksOfVernacularWithTwoCharacters_ReferenceTextHasSameCharacters_PrimaryReferenceTextSetCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator),
			};
			block.AddVerse(1, "This is versiculo uno.").AddVerse(2, "Pedro, Jacobo y Juan respondieron, ");
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				IsParagraphStart = true,
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.BlockElements.Add(new ScriptText("“Estamos irritados mas que lo que puedes creer! "));
			block.AddVerse(3, "Entiede?”");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator),
			};
			block.AddVerse(1, "This is verse one.").AddVerse(2, "Peter, James, and John said, ");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				CharacterId = "Peter/James/John",
				CharacterIdInScript = "John",
				Delivery = "annoyed beyond belief",
			};
			block.BlockElements.Add(new ScriptText("“We are irritated beyond belief! "));
			block.AddVerse(3, "Got it?”");
			referenceBlocks.Add(block);

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(2));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", StandardCharacter.Narrator)));
			Assert.That(result[1].CharacterId, Is.EqualTo("Peter/James/John"));
			Assert.That(result[1].CharacterIdInScript, Is.EqualTo("John"));
			Assert.That(result, ForEvery<Block>(b => b.UserConfirmed, Is.False));
			Assert.That(result, ForEvery<Block>(b => b.SplitId, Is.EqualTo(-1)));
			Assert.That(result.Select(v => v.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))), Is.True);
		}

		[Test]
		public void ApplyTo_MultipleBlocksOfVernacular_NoConflictsInReferenceText_ReferenceTextAppliedCorrectly()
		{
			Block.FormatChapterAnnouncement = (s, i) => s + i;
			var vernacularBlocks = new List<Block>();
			var block = new Block("mt", 1)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("El Evangelio Segun San Mateo"));
			vernacularBlocks.Add(block);
			block = NewChapterBlock("MAT", 1);
			vernacularBlocks.Add(block);
			vernacularBlocks.Add(CreateBlockForVerse("Paul", 1, "This is versiculo uno.", true)
				.AddVerse(2, "This is versiculo dos.")
				.AddVerse(3, "This is versiculo tres."));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(4, "Now the narrator butts in."));
			block = NewChapterBlock("MAT", 2);
			vernacularBlocks.Add(block);
			block = new Block("s", 2)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("This is una historia about a scruffy robot jugando volleybol"));
			vernacularBlocks.Add(block);
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "El robot agarro la pelota.", true, 2, "MAT", "q"));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			block = new Block("mt", 1)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("The Gospel According to Saint Thomas"));
			referenceBlocks.Add(block);
			block = NewChapterBlock("MAT", 1);
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateBlockForVerse("Paul", 1, "This is verse one.", true));
			referenceBlocks.Add(CreateBlockForVerse("Paul", 2, "This is verse two.", true));
			referenceBlocks.Add(CreateBlockForVerse("Paul", 3, "This is verse three.", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Now the narrator butts in."));
			block = NewChapterBlock("MAT", 2);
			referenceBlocks.Add(block);
			block = new Block("s", 2)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("This is a story about a scruffy robot playing volleyball"));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "The robot grabbed the ball.", true, 2, "MAT", "q"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(referenceBlocks.Count));
			Assert.That(result[0].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter)));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(
				"The Gospel According to Saint Thomas"));
			Assert.That(result[1].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter)));
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(
				"The Gospel According to Thomas 1"));
			Assert.That(result[2].CharacterId, Is.EqualTo("Paul"));
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[2].GetText(true)));
			Assert.That(result[3].CharacterId, Is.EqualTo("Paul"));
			Assert.That(result[3].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[3].GetText(true)));
			Assert.That(result[4].CharacterId, Is.EqualTo("Paul"));
			Assert.That(result[4].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[4].GetText(true)));
			Assert.That(result[5].CharacterId,
				Is.EqualTo(GetStandardCharacterId("MAT", StandardCharacter.Narrator)));
			Assert.That(result[5].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[5].GetText(true)));
			Assert.That(result[6].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", StandardCharacter.BookOrChapter)));
			Assert.That(result[6].GetPrimaryReferenceText(), Is.EqualTo(
				"The Gospel According to Thomas 2"));
			Assert.That(result[7].CharacterId, Is.EqualTo(
				GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical)));
			Assert.That(result[7].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[7].GetText(true)));
			Assert.That(result[8].CharacterId,
				Is.EqualTo(GetStandardCharacterId("MAT", StandardCharacter.Narrator)));
			Assert.That(result[8].GetPrimaryReferenceText(), Is.EqualTo(
				referenceBlocks[8].GetText(true)));
		}

		[Test]
		public void ApplyTo_VernacularHasExtraTrailingNarratorBlock_AllBlocksMatchExceptTrailingNarratorBlock()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo uno, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo. Pero Paul replico: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "Asi pense, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Then Jesus said, ", true) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“This is verse one.” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But Paul replied, ");
			AddBlockForVerseInProgress(referenceBlocks, "Paul", "“That's what I thought.”");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			for (int i = 0; i < referenceBlocks.Count; i++)
			{
				var desc = $"Block {i}: {result[i].GetText(true)}";

				Assert.That(result[i].CharacterId,
					Is.EqualTo(vernacularBlocks[i].CharacterId), desc);
				Assert.That(result[i].ReferenceBlocks.Single().GetText(true),
					Is.EqualTo(referenceBlocks[i].GetText(true)), desc);
				Assert.That(result[i].GetPrimaryReferenceText(),
					Is.EqualTo(referenceBlocks[i].GetText(true)), desc);
				Assert.That(result[i].MatchesReferenceText, Is.True, desc);
			}


			Assert.That(result.Last().CharacterId, Is.EqualTo(vernacularBlocks.Last().CharacterId));
			Assert.That(result.Last().ReferenceBlocks, Is.Empty);
			Assert.That(result.Last().MatchesReferenceText, Is.False);
		}

		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_NoConflictsInReferenceText_ReferenceTextAppliedCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.AddVerse(1, "Then Jesus said, ");
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Jesus"
			};
			block.BlockElements.Add(new ScriptText("Porque pateas al gato? "));
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new ScriptText("Y Pablo respondio diciendo, "));
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Paul"
			};
			block.BlockElements.Add(new ScriptText("Quien eres Senor? Pedro?"));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.AddVerse(1, "Then Jesus said, ");
			referenceBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Jesus"
			};
			block.BlockElements.Add(new ScriptText("Why do you kick the cat? "));
			referenceBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new ScriptText("And Paul responded, "));
			referenceBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Paul"
			};
			block.BlockElements.Add(new ScriptText("Who are you? PETA?"));
			referenceBlocks.Add(block);
			Assert.That(referenceBlocks.Count, Is.EqualTo(vernacularBlocks.Count)); // Sanity check

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(referenceBlocks.Count));
			Assert.That(result.Select(v => v.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))), Is.True);
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridge_ReferenceBrokenAtVerses_ReferenceTextCombinedToMatch()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 3)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new Verse("1-3"));
			block.BlockElements.Add(new ScriptText("Entonces Jesús dijo que los reducirían un burro. El número de ellos dónde encontrarlo. Y todo salió bien."));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true),
				CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "),
				CreateNarratorBlockForVerse(3, "It did.")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.Single().MatchesReferenceText, Is.True);
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].GetPrimaryReferenceText(),
				Is.EqualTo(string.Concat(referenceBlocks.Select(r => r.GetText(true)))));
		}

		[Test]
		public void ApplyTo_ReferenceHasVerseBridgeCorrespondingToTwoVernVerses_Mismatched()
		{
			var narrator = GetStandardCharacterId("JUD", StandardCharacter.Narrator);

			var vernacularBlocks = new List<Block>
				{
					new Block("p", 1, 1) {CharacterId = narrator }.AddVerse("1", "A"),
					new Block("p", 1, 2) {CharacterId = "Enoch" }.AddVerse("2", "B"),
					new Block("p", 1, 3) {CharacterId = narrator }.AddVerse("3", "C"),
					new Block("p", 1, 4) {CharacterId = "Michael" }.AddVerse("4", "D")
				};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				new Block("p", 1, 1) { CharacterId = narrator }.AddVerse("1", "Ayy"),
				new Block("p", 1, 2, 3) {CharacterId = narrator}.AddVerse("2-3", "Bee Cee"),
				new Block("p", 1, 4) { CharacterId = "Michael" }.AddVerse(4, "Dee, "),
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));

			// We don't especially care how the vern blocks for verses 2 and 3 align to the ref block for 2–3 as long as neither one
			// is considered a match and exactly one of them is hooked up to it.
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[2].MatchesReferenceText, Is.False);
			Assert.That(
				referenceBlocks.Skip(1).Take(1).Select(r => r.GetText(true)),
				Is.EqualTo(result.Skip(1).Take(2).SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true)))
			);

			Assert.That(result[3].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
		}

		[Test]
		public void GetExportData_VernVerseHasMorePartsThanReference_FinalBlockOfVerseContainsStartOfFollowingVerseInBothVernAndRef_BeginningAndEndMatch()
		{
			var narrator = GetStandardCharacterId("MRK", StandardCharacter.Narrator);

			var vernacularBlocks = new List<Block>
				{
					new Block("p", 4, 39) {IsParagraphStart = true, CharacterId = narrator }.AddVerse(39, "Jedus stanop, taak scrong ta de big breeze say, "),
					new Block("p", 4, 39) {CharacterId = "Jesus", Delivery = "forcefully", BlockElements = new List<BlockElement> { new ScriptText("“Hush, stop blow.” ") } },
					new Block("p", 4, 39) {CharacterId = narrator, BlockElements = new List<BlockElement> { new ScriptText("An e say ta de swellin wata, ") } },
					new Block("p", 4, 39) {CharacterId = "Jesus", Delivery = "forcefully", BlockElements = new List<BlockElement> { new ScriptText("“Go down.” ") } },
					new Block("p", 4, 39) {CharacterId = narrator, BlockElements = new List<BlockElement>
					{
						new ScriptText("De big breeze done hush an stop fa blow, an de swellin wata gone down an been peaceable an steady. "),
						new Verse("40"),
						new ScriptText("Den Jedus ton roun ta e ciple dem an e say, ")
					} },
					new Block("p", 4, 40) {CharacterId = "Jesus", Delivery = "questioning", BlockElements = new List<BlockElement>
					{
						new ScriptText("“Hoccome oona so scaid? Stillyet oona ain bleebe pon God, ainty?”")
					} }
				};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
				{
					new Block("p", 4, 39) {IsParagraphStart = true, CharacterId = narrator }.AddVerse(39, "He awoke, and rebuked the wind, and said to the sea, "),
					new Block("p", 4, 39) {CharacterId = "Jesus", BlockElements = new List<BlockElement> { new ScriptText("“Peace! Be still!” ") } },
					new Block("p", 4, 39) {CharacterId = narrator, BlockElements = new List<BlockElement>
					{
						new ScriptText("The wind ceased, and there was a great calm. "),
						new Verse("40"),
						new ScriptText("He said to them, ")
					} },
					new Block("p", 4, 40) {CharacterId = "Jesus", Delivery = "questioning", BlockElements = new List<BlockElement>
					{
						new ScriptText("“Why are you so afraid? How is it that you have no faith?”")
					} }
				};
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[1].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[2].ReferenceBlocks, Is.Empty);
			Assert.That(result[3].ReferenceBlocks, Is.Empty);
			Assert.That(result[4].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(referenceBlocks[2].GetText(true)));
			Assert.That(result[5].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(referenceBlocks[3].GetText(true)));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeNotAtStartOfBlock_ReferenceBrokenAtVerses_VernacularSplitForNonBridgedVerses()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.AddVerse(1, "Entonces Jesús dijo que los reducirían un burro. ")
				.AddVerse("2-3", "El número de ellos dónde encontrarlo. Y todo salió bien. ")
				.AddVerse(4, "El cuarto versiculo.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true),
				CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "),
				CreateNarratorBlockForVerse(3, "It did. "),
				CreateNarratorBlockForVerse(4, "Fourth verse.")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo("{1}\u00A0Jesus told them where to find a donkey. "));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true)));
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0Fourth verse."));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeAtStartOfBlockFollowedByOtherVerses_ReferenceBrokenAtVerses_VernacularSplitAtEndOfBridgeAndSubsequentVerses()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Jesús les dijo donde encontrar un burro. ", true) };
			var block = new Block("p", 1, 2, 3)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.AddVerse("2-3", "El número de ellos dónde encontrarlo. Y todo salió bien. ")
				.AddVerse(4, "El cuarto versiculo.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true),
				CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "),
				CreateNarratorBlockForVerse(3, "It did. "),
				CreateNarratorBlockForVerse(4, "Fourth verse.")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo("{1}\u00A0Jesus told them where to find a donkey. "));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true)));
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0Fourth verse."));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeNotAtStartOfBlock_ReferenceNotBrokenAtStartOfVernacularBridge_NoSplitAndNoErrorReport()
		{
			// PG-746 Chikunda
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(17, "Paadasiya gunyenye ndiye adapita munyumba, wakufundila wake adamubvunza kuti alewe dzvadzvikalewa dzvaalewa. ", true, 7 ,"MRK");
			block.AddVerse("18-19", "Jesu adati kwa iwo, ");
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MRK", vernacularBlocks, m_vernVersification);

			// Indonesian
			var referenceBlocks = new List<Block>();
			block = CreateNarratorBlockForVerse(17, "Sesudah Ia masuk ke sebuah rumah untuk menyingkir dari orang banyak, murid-murid-Nya bertanya kepada-Nya tentang arti perumpamaan itu. ", true, 7, "MRK");
			block.AddVerse(18, "Maka jawab-Nya:");
			referenceBlocks.Add(block);
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "<<Apakah kamu juga tidak dapat memahaminya? Tidak tahukah kamu bahwa segala sesuatu dari luar yang masuk " +
				"ke dalam seseorang tidak dapat menajiskannya, ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Single().MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true) + " " + referenceBlocks[1].GetText(true)));
		}

		[Test]
		public void ApplyTo_VerseBridgeInVernacularPreventsSplitToCorrespondToBreakInReferenceText_MatchTheLeadingAndTrailingBlocksAndMismatchMiddleOnes()
		{
			// PG-746 Chikunda (Acts 8:26-29)
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(26, "Ngilozi ... Filipi, ", true, 8, "ACT") };
			AddBlockForVerseInProgress(vernacularBlocks, "angel", "“Konzekela ... Gaza.” ");
			var block = AddNarratorBlockForVerseInProgress(vernacularBlocks, "(Njila ... zino.) ", "ACT");
			block.AddVerse("27-28", "Saka ... Ayizaya. ");
			block.AddVerse(29, "Mzimu ... Filipi, ");
			AddBlockForVerseInProgress(vernacularBlocks, "Holy Spirit, the", "“Yenda ... iyo.” ");
			var vernBook = new BookScript("ACT", vernacularBlocks, m_vernVersification);

			// Indonesian
			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(26, "Kemudian ..., katanya:", true, 8, "ACT") };
			AddBlockForVerseInProgress(referenceBlocks, "angel", "<<Bangunlah ... Gaza.>>");
			block = AddNarratorBlockForVerseInProgress(referenceBlocks, "Jalan ... sunyi. ", "ACT");
			block.AddVerse(27, "Lalu ... beribadah.");
			referenceBlocks.Add(block = CreateNarratorBlockForVerse(28, "Sekarang ... Yesaya. ", true, 8, "ACT"));
			block.AddVerse(29, "Lalu ... Filipus:");
			AddBlockForVerseInProgress(referenceBlocks, "Holy Spirit, the", "<<Pergilah ... itu!>>");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].MatchesReferenceText, Is.True);
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText, Is.True);
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(2));
			Assert.That(result[2].MatchesReferenceText, Is.False);
			Assert.That(result[2].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
			Assert.That(result[2].ReferenceBlocks[1].GetText(true), Is.EqualTo(referenceBlocks[3].GetText(true)));

			Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[3].MatchesReferenceText, Is.True);
			Assert.That(result[3].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[4].GetText(true)));
		}

		[Test]
		public void ApplyTo_SingleSpeakerVerseBridgeInVernacularCorrespondsToTwoBlocksInReferenceText_CombineReferenceTextBlocksAndMatchToVernBlock()
		{
			// PG-764 Chikunda (I Corinthians 5:3-4)
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(GetStandardCharacterId("1CO", StandardCharacter.Narrator), 3,
					"Ndiye, kana dzvangu ndilikutali na imwepo pathupi, ndichilikumweko na imwepo pamzimu. Ndiye ngatindilipo, ndatotonga kale mudzina la Mfumu Jesu padzulu pa mamuna ayita dzvimwedzvi. Apo pamunizagumana, ndinizagumana na imwepo pamzimu, na mphamvu ya Mfumu yathu Jesu alipo na ifepo, ",
					false, 5, "p" , 4),
				CreateNarratorBlockForVerse(5,
					"munifanila kuyikha mamuna umweyu kwa Satana kuti thupi lake liwonongewe, kuti mzimu wake upulumusiwe pa Nsiku ya Mfumu. ",
					false, 5, "1CO")
			};
			var vernBook = new BookScript("1CO", vernacularBlocks, m_vernVersification);

			// Indonesian & English reference blocks
			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(3, "Sebab aku, ... itu.", true, 5, "1CO") };
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(3, "For ... thing.", true, 5, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Bilamana ... kita,", true, 5, "1CO"));
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(4, "In ... Christ.", true, 5, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(5, "orang ... Tuhan.", true, 5, "1CO"));
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(5, "Are to ... Jesus.", true, 5, "1CO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(
				referenceBlocks[0].GetText(true) + " " + referenceBlocks[1].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks[0].GetText(true),
				Is.EqualTo(referenceBlocks[2].GetText(true)));
		}

		/// <summary>
		/// PG-742
		/// </summary>
		[Test]
		public void ApplyTo_MissingVerseNumberInVernacular_DoesNotFail()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1,
						"E mbaŋako iyako, Herod, iye Galili gharambarombaro i loŋweya Jisas le vakatha utuutuniye. ", true, 14)
					.AddVerse(2, "I dage weŋgiya le rakakaiwo e raberabe iŋa, ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Herod Antipas (the tetrarch)",
				"“Loloko iyako mbema emunjoru Jon Rabapɨtaiso, i thuweiru na tembe e yawayawaliyeva. Iya kaiwae valɨkaiwae i vakathaŋgiya vakatha ghamba rotaele ŋgoranjiyako.”");
			var block = new Block("p", 14, 3);
			block.BlockElements.Add(new Verse("3,"));
			block.BlockElements.Add(
				new ScriptText(
					"4 Kaiwae Herod va i viwe ghagha Pilip levo Herodiyas na i ghe weiye, Jon vambe i vathivalaŋa wevara, iŋa, "));
			vernacularBlocks.Add(block);
			AddBlockForVerseInProgress(vernacularBlocks, "John the Baptist",
				"“Ghanda Mbaro ma i vatomwe e ghen na u vaŋgwa Herodiyas!” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"Iyako kaiwae, Herod va iŋa na thɨ yalawe Jon, thɨ ŋgarɨ na thɨ woruwo e thiyo. ")
				.AddVerse(5,
					"Herod va nuwaiya iŋa na Jon i mare, ko va i mararuŋgiya Jiu kaiwae va thɨŋa Jon iye Loi ghalɨŋae gharautu.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

#if DEBUG
			using (new ErrorReport.NonFatalErrorReportExpected())
#else
			using (new ErrorReport.NoNonFatalErrorReportExpected())
#endif
				refText.ApplyTo(vernBook);
		}

		/// <summary>
		/// PG-744
		/// </summary>
		[Test]
		public void ApplyTo_VerseBreakInVernThatIsNotInReferenceText_SoundEffectInReferenceText_ReferenceTextAppliedCorrectly()
		{
			// This test assumes the English reference text has the following:
			// <block style="p" paragraphStart="true" chapter="22" initialStartVerse="50" characterId="narrator-LUK">
			//   <verse num="50" />
			//   <text>A certain one of them struck the servant of the high priest, and </text>
			//   <sound soundType="Sfx" effectName="Man crying out" userSpecifiesLocation="true" />
			//   <text>cut off his right ear. </text>
			//   <verse num="51" />
			//   <text>But Jesus answered,</text>
			// </block>
			// <block style="p" paragraphStart="true" chapter="22" initialStartVerse="51" characterId="Jesus" delivery="forcefully">
			//   <text>“Permit them to seize me.”</text>
			// </block>
			// <block style="p" paragraphStart="true" chapter="22" initialStartVerse="51" characterId="narrator-LUK">
			//   <text>and he touched his ear, and healed him.</text>
			// </block>

			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(50,
					"A particular one of them dudes whacked the slave of the main priest, severing his ear.", true, 22, "LUK"),
				CreateBlockForVerse("Jesus", 51,
					"“What in the world was that all about? They're supposed to get away with this,” ", true, 22)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"reprimanded Jesus. Then He put his hand on his ear and fixed him right up.", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].ReferenceBlocks[0].BlockElements.OfType<Sound>().Any(), Is.True);
			// Even though the verse break in the vernacular forced the reference block for vv. 50-51 to get split,
			// there is now refined logic that should ensure that the "But Jesus answered, " block that is unaccounted for
			// in the vernacular will attach to the preceding narrator block. But we still want the user to review it, so
			// it should not be treated as automatically aligning perfectly, even though in this case it works out.
			Assert.That(result[0].MatchesReferenceText, Is.False);
			var referenceBlocks = refText.Books.Single(b => b.BookId == vernBook.BookId).GetBlocksForVerse(22, 50, 51).ToList();
			// Ensure all reference blocks are accounted for
			Assert.That(string.Concat(result.SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true))),
				Is.EqualTo(string.Concat(referenceBlocks.Select(r => r.GetText(true)))));
			Assert.That(result.Skip(1), ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}

		/// <summary>
		/// PG-744
		/// </summary>
		[Test]
		public void ApplyTo_VerseBreakInVernThatIsNotInReferenceText_SoundEffectAtEndOfVerseInReferenceText_ReferenceTextAppliedCorrectly()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(50,
					"A particular one of them dudes whacked the slave of the main priest, severing his ear.", true, 22, "LUK"),
				CreateBlockForVerse("Jesus", 51,
					"“What in the world was that all about? They're supposed to get away with this,” ", true, 22)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"reprimanded Jesus. Then He put his hand on his ear and fixed him right up.", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();

			var block = CreateNarratorBlockForVerse(50,
				"A certain one of them struck the servant of the high priest, and cut off his right ear. ", true, 22, "LUK");
			block.BlockElements.Add(new Sound { SoundType = SoundType.Sfx, EffectName = "Man crying out", UserSpecifiesLocation = true });
			block.AddVerse(51, "But Jesus answered, ");
			referenceBlocks.Add(block);
			var origRefBlock0 = block.GetText(true, true);
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Permit them to seize me.”");
			AddNarratorBlockForVerseInProgress(referenceBlocks,
				"and he touched his ear, and healed him.", "LUK");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.English);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].ReferenceBlocks[0].BlockElements.Last(), Is.InstanceOf<Sound>());
			// Even though the verse break in the vernacular forced the reference block for vv. 50-51 to get split,
			// there is now refined logic that should ensure that the "But Jesus answered, " block that is unaccounted for
			// in the vernacular will attach to the preceding narrator block. But we still want the user to review it, so
			// it should not be treated as automatically aligning perfectly, even though in this case it works out.
			Assert.That(result[0].MatchesReferenceText, Is.False);
			Assert.That(string.Concat(result[0].ReferenceBlocks.Select(rb => rb.GetText(true, true))),
				Is.EqualTo(origRefBlock0));
			// Ensure remaining reference blocks align 1-to-1
			Assert.That(result.Skip(1).SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count - 1));
			Assert.That(result.Skip(1).SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true))
				.SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))), Is.True);
			Assert.That(result.Skip(1), ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeWithSubVerseLetter_ReferenceBrokenAtVerses_VernacularSplitAtEndOfLastSubVerseChunk()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Entonces Jesús dijo que los reducirían un burro. "));
			block.BlockElements.Add(new Verse("2-3a"));
			block.BlockElements.Add(new ScriptText("El número de ellos dónde encontrarlo. Y todo salió bien. "));
			block.BlockElements.Add(new Verse("3f")); // Using "f" instead of "b" just to demonstrate that we aren't hardcoding "b"
			block.BlockElements.Add(new ScriptText("La segunda parte del versiculo. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("El cuarto versiculo."));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true),
				CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "),
				CreateNarratorBlockForVerse(3, "It did. "),
				CreateNarratorBlockForVerse(4, "Fourth verse.")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].GetText(true),
				Is.EqualTo("{1}\u00A0Entonces Jesús dijo que los reducirían un burro. "));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(),
				Is.EqualTo("{1}\u00A0Jesus told them where to find a donkey. "));

			Assert.That(result[1].GetText(true), Is.EqualTo(
				"{2-3a}\u00A0El número de ellos dónde encontrarlo. Y todo salió bien. {3f}\u00A0La segunda parte del versiculo. "));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true)));

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo("{4}\u00A0Fourth verse."));
		}

		[Test]
		public void ApplyTo_ReferenceHasVerseBridge_VernacularBrokenAtEndOfBridge()
		{
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(1, "I gotta go. ");
			block.AddVerse(2, "More blah blah. ").AddVerse(3, "More more blah blah. ").AddVerse(4, "Jesús les dijo dónde encontrar un burro.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1, 3)
			{
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new Verse("1-3"));
			block.BlockElements.Add(new ScriptText("Then Jesus said would reduce a donkey. The number of them where to find it. And all went well."));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Jesus told them where to find a donkey.", true));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(result[0].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(result[0].LastVerseNum, Is.EqualTo(3));

			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(result[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(result[1].LastVerseNum, Is.EqualTo(4));
		}

		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_SpeakersDoNotCorrespond_UnmatchedReferenceTextCopiedIntoFirstBlockForVerse()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Then Jesus said, ", true) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Y Pablo respondio diciendo, ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "“Quien eres Senor? Pedro?”");
			vernacularBlocks.Add(CreateBlockForVerse("Paul", 2, "“Vamos a Asia!”"));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateBlockForVerse("Jesus", 1, "Why do you kick the cat? ", true) };
			AddNarratorBlockForVerseInProgress(referenceBlocks, "asked Jesus. ");
			AddBlockForVerseInProgress(referenceBlocks, "Martha", "“Couldn't you have come sooner?” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "muttered Martha.");
			referenceBlocks.Add(CreateBlockForVerse("Timothy", 2, "“Let's go to Asia!”"));

			Assert.That(referenceBlocks.Count, Is.EqualTo(vernacularBlocks.Count)); // Sanity check

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			// Verse 1
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(4));
			Assert.That(result[0].ReferenceBlocks.Select(r => r.GetText(true))
				.SequenceEqual(referenceBlocks.Take(4).Select(r => r.GetText(true))), Is.True);
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result.Take(4), ForEvery<Block>(b => b.GetPrimaryReferenceText(), Is.EqualTo(null)));

			// Verse 2 (different character IDs but we match them anyway because it is the entirety of the verse)
			Assert.That(result[4].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[4].MatchesReferenceText, Is.False);
			Assert.That(result[4].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[4].GetText(true)));
			Assert.That(result[4].ReferenceBlocks[0].CharacterId, Is.EqualTo(referenceBlocks[4].CharacterId));
		}

		[Test]
		public void ApplyTo_SingleSpeakerInVerse_SpeakersBeginCorrespondingThenDoNotCorrespond_ReferenceTextCopiedIntoBestMatchedVerseBlocks()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "Entonces dijo Jesus, ", true, 9) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "Then Jesus said, ", true, 9) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "thus he spake. ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].ReferenceBlocks[0], Is.EqualTo(referenceBlocks[0]));
			Assert.That(result[0].ReferenceBlocks[1], Is.EqualTo(referenceBlocks[2]));
			Assert.That(result[0].MatchesReferenceText, Is.False);

			Assert.That(result[1].MatchesReferenceText, Is.True);
			Assert.That(result[1].ReferenceBlocks.Single(), Is.EqualTo(referenceBlocks[1]));
		}

		[Test]
		public void ApplyTo_SingleVoice_SingleSpeakerInVerse_SpeakersBeginCorrespondingThenDoNotCorrespond_ReferenceTextOrderNotChanged()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "Entonces dijo Jesus, ", true, 9) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification)
			{
				SingleVoice = true
			};

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "Then Jesus said, ", true, 9) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "thus he spake. ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result, No<Block>(v => v.ReferenceBlocks, Is.Empty));
			var refBlocksConnectedToVernBlocks = result.SelectMany(v => v.ReferenceBlocks).ToList();
			Assert.That(string.Concat(refBlocksConnectedToVernBlocks.Select(r => r.GetText(true))),
				Is.EqualTo(string.Concat(referenceBlocks.Select(r => r.GetText(true)))));
		}

		// PG-1133 (part 2: preventing re-ordering of ref blocks in a way that would combine texts for two different speakers)
		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_IndirectSpeechInVernDoesNotMatchDirectSpeechInRef_ReferenceTextForSecondSpeakerNotAppendedToFirstSpeaker()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(23, "Ndè, Yesu 'ǒ rò. ", true, 9).AddVerse(24, "Fǿrò, ke pòngʉ́e rɨ: ") };
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“'Ánɨ̀tù 'ɨ̀gǎrò, ddidhò, fǿ rɨ, ngbá kpalí.” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Ndrǔ ʉ̀ ke ʉ̌. ")
				.AddVerse(25, "Bíròbírò kʉ̀ ndrǔ-kpàrì rɨ gòtɨ́, Yesu tsùngʉ́e gì nà, le dángʉ́enɨ̀ rønà théchʉ́. ")
				.AddVerse(26, "Fǿ lò-yɨ̌ ngø̀ lú gblè.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(23, "When Jesus saw the crowd, ", true, 9).AddVerse(24, "he said,") };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Scram. The girl is sleeping.”");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "They mocked him, saying:");
			AddBlockForVerseInProgress(referenceBlocks, "people at Jairus' house", "“The girl is dead!”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(25, "But when the crowd was put out, he made her alive.", true, 9));
			referenceBlocks.Add(CreateNarratorBlockForVerse(26, "The report of this went out into all that land.", true, 9));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Single(), Is.EqualTo(referenceBlocks[0]));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks.Single(), Is.EqualTo(referenceBlocks[1]));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(result[2].ReferenceBlocks[0], Is.EqualTo(referenceBlocks[2]));
			Assert.That(result[2].ReferenceBlocks[1], Is.EqualTo(referenceBlocks[3]));
			Assert.That(result[3].ReferenceBlocks.Single(), Is.EqualTo(referenceBlocks[4]));
			Assert.That(result[3].MatchesReferenceText);
			Assert.That(result[4].ReferenceBlocks.Single(), Is.EqualTo(referenceBlocks[5]));
			Assert.That(result[4].MatchesReferenceText);
		}

		// PG-1133 (part 3: handling missing lead-in "Saying,")
		[Test]
		public void ApplyTo_ReferenceTextHasLeadingNarratorBlockNotPresentInVernacular_AddedAsUnmatchedBlockToReferenceBlocksOfPrecedingBlock()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(41, "Ddichʉ́, Farisayó ndɨ̀mà-tsò rò, Yesu tsó 'ɨ̌: ", true, 22),
				CreateBlockForVerse(kAmbiguousCharacter, 42, "“Nɨ̀ ká ddɨ́ Ndrǔ ná ke djò? Ke ká nǎrò sɨ̀ ná gø̀?” ", false, 22)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Fǿrò, kpa ngʉ̀ngʉ́e lò-gòtɨ ke dhò: ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“Ke kʉ̀ Dawudi nǎrò sɨ̀ ná gø̀.” ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(41, "While the Pharisees were together, Jesus asked them, ", true, 22)
					.AddVerse(42, "saying,")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Whose son is the Christ?”");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "They said to him,");
			AddBlockForVerseInProgress(referenceBlocks, "Pharisees", "“Of David.”");

			var orig = string.Concat(referenceBlocks.Select(r => r.GetText(true)));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(),
				Is.EqualTo(referenceBlocks.Count + 1),
				"There were 4 original reference blocks, but the one that contained the start of v. 42 " +
				"gets split up because the vernacular has a block break aligned to that verse break.");
			Assert.That(string.Concat(result.SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true))),
				Is.EqualTo(orig));

			Assert.That(result[0].ReferenceBlocks[0].GetText(true),
				Is.EqualTo("{41}\u00A0While the Pharisees were together, Jesus asked them, "));
			Assert.That(result[0].ReferenceBlocks[1].GetText(true),
				Is.EqualTo("{42}\u00A0saying,"));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText);
		}

		// PG-1133 (part 4: When attempting to align ref block to preceding block, don't assume that it has reference blocks.)
		[Test] public void ApplyTo_VernacularPoeticBlocksNotBrokenOutInReferenceTextWithFollowingUnmatchedReferenceBlock_DoesNotCrash()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(7, "ଆୟୁମେପେ, ମାସିଦ ରିମିଲ୍‌ତେଃ ହୁଜୁଃତେନେ; ଅଣ୍ଡଃ ହୁଜୁଃତେନେଃକ ନେଲି'ୟେ।",
					true, 1, "REV", "q1")
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "ଆଏଃ ସବଃକେନ୍‌କ ଜାକେଡ୍‌ ଆଏଃକ ନେଲ୍‌କିଃତେକ ରାଃ-ଗିରାଙ୍ଗିୟା।")
				.IsParagraphStart = true;
			AddBlockForVerseInProgress(vernacularBlocks, vernacularBlocks.Last().CharacterId,
					"ସାରିତେ ନିନାଦ ନେଲେକାଗେ ହବାୱା, ଆମେନ୍‌!", "m");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(8, "ପ୍ରବୁ ଇସର୍‌ଦଏ କାଜି'ତେନେ, ", true, 1, "REV"));
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"“ଆଞ୍‌ଗେ ସବେନାଃଞ୍‌ ଉଙ୍କୁଳୁକିଡା, ହୁଜୁଃତିନିଃଦ, ଅଣ୍ଡଃ ଆଞ୍‌ଗେ ସବେନ୍‌କୟେତେ ପେଃୟାନ୍‌ ଇସର୍‌ଦ।”");
			var vernBook = new BookScript("REV", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(7, "All eyes will see him come. All will mourn. Amen.", true, 1, "REV"),
				CreateBlockForVerse("God", 8, "“I am Alpha and Omega, beginning and end,”", true, 1, "REV")
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "says the Lord God,", "REV");
			AddBlockForVerseInProgress(referenceBlocks, "God", "“who is and who was and who is to come, the Almighty.”");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(referenceBlocks[0].GetText(true),
				Is.EqualTo(result[0].GetPrimaryReferenceText()));

			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));

			Assert.That(result[2].MatchesReferenceText, Is.False);
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(0));

			Assert.That(result[3].MatchesReferenceText);
			Assert.That(referenceBlocks[2].GetText(true),
				Is.EqualTo(result[3].GetPrimaryReferenceText()));

			Assert.That(result[4].ReferenceBlocks.Count, Is.EqualTo(2));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[4].ReferenceBlocks[0].GetText(true)));
			Assert.That(referenceBlocks[3].GetText(true),
				Is.EqualTo(result[4].ReferenceBlocks[1].GetText(true)));
		}

		[Test]
		public void ApplyTo_VernHasVerse1ButReferenceDoesNot_OtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 1 but those are not present in the reference text translation.
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Hola!", true),
				CreateNarratorBlockForVerse(2, "Entonces dijo Jesus, ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(2, "Then Jesus said, ", true) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[0].GetText(true),
				Is.EqualTo(result[1].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[2].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerse1ButVernDoesNot_OtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 1 but those are not present in the vernacular translation.
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(2, "Entonces dijo Jesus, ") };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Hello!", true),
				CreateNarratorBlockForVerse(2, "Then Jesus said, ", true)
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[0].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[2].GetText(true),
				Is.EqualTo(result[1].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernHasVerse0ButReferenceDoesNot_Verse0NotMatchedAndOtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 0 but those are not present in the reference text translation.
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q") };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[0].GetText(true),
				Is.EqualTo(result[1].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[2].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerse0ButVernDoesNot_Verse0IgnoredAndOtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 0 but those are not present in the vernacular translation.
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q") };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[0].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[2].GetText(true),
				Is.EqualTo(result[1].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernHasTwoParagraphsOfHebrewSubtitleButReferenceHasOne_TreatedLikeNormalVerse()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(0, "A psalm of David", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result.SelectMany(v => v.ReferenceBlocks).Count(), Is.EqualTo(referenceBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[0].GetText(true),
				Is.EqualTo(result[0].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(referenceBlocks[0].GetText(true),
				Is.EqualTo(result[0].GetPrimaryReferenceText()));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.Null);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[1].GetText(true),
				Is.EqualTo(result[2].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);

			Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[2].GetText(true),
				Is.EqualTo(result[3].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[3].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasTwoParagraphsOfHebrewSubtitleButVernHasOne_TreatedLikeNormalVerse()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"),
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d") };
			var block = new Block("d", 1) {
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId("PSA", StandardCharacter.Narrator),
				BlockElements = new List<BlockElement> { new ScriptText("A psalm of David") }
			};
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(
				Join(" ", referenceBlocks.Take(2).Select(b => b.GetText(true)))));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[2].GetText(true),
				Is.EqualTo(result[1].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(referenceBlocks[3].GetText(true),
				Is.EqualTo(result[2].ReferenceBlocks[0].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasMoreVersesThanVernacular_ExtraVersesIgnored()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(8, "Con gran temblor...", true, 16, "MRK") };
			var vernBook = new BookScript("MRK", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(8, "Trembling and bewildered...", true, 16, "MRK"),
				CreateNarratorBlockForVerse(9, "Hey, who added these verse to the Bible? ", true, 16, "MRK"),
				CreateNarratorBlockForVerse(10, "remember what God said about that!", false, 16, "MRK")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasIntroAndReferenceDoesnt_IntroIgnored()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(GetStandardCharacterId("MAT", StandardCharacter.Intro), 0, "Intro uno", true),
				CreateNarratorBlockForVerse(1, "Con gran temblor...", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Trembling and bewildered...", true) };

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.Null);
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceHaveIntros_IntroNotMatched()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(GetStandardCharacterId("MAT", StandardCharacter.Intro), 0, "Intro uno", true),
				CreateNarratorBlockForVerse(1, "Con gran temblor...", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateBlockForVerse(GetStandardCharacterId("MAT", StandardCharacter.Intro), 0, "Introduction", true),
				CreateNarratorBlockForVerse(1, "Trembling and bewildered...", true)
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.Null);
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_MultipleVersesInSingleReferenceBlock_VernacularNotSplitAtVerse()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Verse uno.", true).AddVerse(2, "Verse dos.") };
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Verse 1.", true).AddVerse(2, "Verse 2.") };

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularNeedsToBeBrokenByReference_FirstReferenceVerseHasTwoBlocks_VernacularBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(7, "Verse seven. ", true).AddVerse(8, "Verse eight. ") };
			AddBlockForVerseInProgress(vernacularBlocks, "Herod", "What Herod says in verse eight.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(7, "Verse 7. ", true) };
			AddBlockForVerseInProgress(referenceBlocks, "Herod", "What Herod says in verse 7. ");
			referenceBlocks.Add(CreateNarratorBlockForVerse(8, "Verse 8. ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Herod", "What Herod says in verse 8.");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true) + referenceBlocks[1].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[2].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[3].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
		}

		/// <summary>
		/// The following is no longer true...
		/// PG-699: Note that this test expects results that are actually less than ideal, since in this case it just so happens
		/// that we would actually prefer for everything to just match up (as it used to).
		/// </summary>
		[Test]
		public void ApplyTo_ReferenceTextNeedsToBeBrokenAtVerseToMatchVernacular_NarratorIntroducesQuoteAtStartOfVerseTwo_ReferenceTextIsBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1,
					"Ka doŋ ginywalo Yecu i Beterekem ma i Judaya i kare me loc pa kabaka Kerode, nen, luryeko mogo ma gua yo tuŋ wokceŋ " +
					"gubino i Jerucalem, kun gipenyo ni, ", true, 2),
				CreateBlockForVerse("magi (wise men from East)", 2,
					"“En latin ma ginywalo me bedo kabaka pa Lujudaya-ni tye kwene? Pien onoŋo waneno lakalatwene yo tuŋ wokceŋ, " +
					"ci man wabino ka wore.”", true, 2)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Now when Jesus was born in Bethlehem of Judea in the days of King Herod, " +
					"behold, wise men from the east came to Jerusalem, ", true, 2).AddVerse(2, "saying, ")
			};
			AddBlockForVerseInProgress(referenceBlocks, "magi (wise men from East)", "“Where is the one who is born King of the Jews? For we saw his star in the east, " +
				"and have come to worship him.”");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(2));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo("{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days " +
					"of King Herod, behold, wise men from the east came to Jerusalem, "));
			Assert.That(result[0].MatchesReferenceText);

			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo("{2}\u00A0saying, " + referenceBlocks[1].GetText(true)));
		}

		/// <summary>
		/// PG-699: This test illustrates the real reason we want to be able to break the reference text at the end of
		/// any verses that have breaks in the vernacular.
		/// PG-761: Added secondary reference text to show that it needs to be split also.
		/// </summary>
		[TestCase(true)] // This is the more likely real-life scenario
		[TestCase(false)]
		public void ApplyTo_ReferenceTextNeedsToBeBrokenAtVerseToMatchVernacular_GoodIllustration_ReferenceTextIsBrokenCorrectly(
			bool useAmbiguousInVern)
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "diciendo: ", true, 18, "REV") };
			AddBlockForVerseInProgress(vernacularBlocks, useAmbiguousInVern ?
					kAmbiguousCharacter : "merchants of the earth",
				"«¡Ay, ay, la gran ciudad, que estaba vestida de " +
				"lino fino, púrpura y escarlata, y adornada de oro, piedras preciosas y perlas!»");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(17,
				"Porque en una hora ha sido arrasada tanta riqueza. Y todos los capitanes, pasajeros y marineros, y todos los que viven " +
				"del mar, se pararon a lo lejos, ", false, 18, "REV"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(18, "y al ver el humo de su incendio gritaban, diciendo: ", false, 18, "REV"));
			AddBlockForVerseInProgress(vernacularBlocks, useAmbiguousInVern ?
				kAmbiguousCharacter : "sea captains, seafarers, sailors",
				"«¿Qué ciudad es semejante a la gran ciudad?»");
			var vernBook = new BookScript("REV", vernacularBlocks, m_vernVersification);

			var frenchReferenceBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "en disant, ", true, 18, "REV") };
			AddBlockForVerseInProgress(frenchReferenceBlocks, "merchants of the earth", "«Malheur, malheur, la grande ville, elle qui était " +
				"vêtue de fin lin, de pourpre et d'écarlate, et parée d' or et de pierres précieuses et de perles!");
			frenchReferenceBlocks.Add(CreateBlockForVerse("merchants of the earth", 17, "Pour en une heure tant de richesses sont en souffrance.»", false, 18));
			AddNarratorBlockForVerseInProgress(frenchReferenceBlocks, "Chaque capitaine, et tout le monde qui navigue partout, et les marins, et " +
				"autant que gagner leur vie en mer, se tenaient loin, ", "REV")
				.AddVerse(18, "et pleuré comme ils ont regardé la fumée de son embrasement: ");
			AddBlockForVerseInProgress(frenchReferenceBlocks, "sea captains, seafarers, sailors", "«Qu'est-ce que la grande ville?»");

			var englishReferenceBlocks = new List<Block> { CreateNarratorBlockForVerse(16, "saying, ", true, 18, "REV") };
			AddBlockForVerseInProgress(englishReferenceBlocks, "merchants of the earth", "“Woe, woe, the great city, she who was dressed in " +
				"fine linen, purple, and scarlet, and decked with gold and precious stones and pearls!");
			englishReferenceBlocks.Add(CreateBlockForVerse("merchants of the earth", 17, "For in an hour such great riches are made desolate.”", false, 18));
			AddNarratorBlockForVerseInProgress(englishReferenceBlocks, "Every shipmaster, and everyone who sails anywhere, and mariners, and " +
				"as many as gain their living by sea, stood far away, ", "REV")
				.AddVerse(18, "and cried out as they looked at the smoke of her burning, saying, ");
			AddBlockForVerseInProgress(englishReferenceBlocks, "sea captains, seafarers, sailors", "“What is like the great city?”");

			for (int i = 0; i < englishReferenceBlocks.Count; i++)
				frenchReferenceBlocks[i].SetMatchedReferenceBlock(englishReferenceBlocks[i]);

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, frenchReferenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(frenchReferenceBlocks[0].GetText(true)));
			Assert.That(result[0].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo("{16}\u00A0saying, "));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(englishReferenceBlocks[0].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(frenchReferenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(englishReferenceBlocks[1].GetText(true)));

			// REVIEW: We might not care whether this is a match (with the two blocks joined into one) or
			// a mismatch (with them separate)
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(Join(" ", result[2].ReferenceBlocks.Select(b => b.GetText(true))),
				Is.EqualTo(Join(" ", frenchReferenceBlocks.Skip(2).Take(2).Select(b => b.GetText(true)))));
			Assert.That(Join(" ", result[2].ReferenceBlocks.SelectMany(b => b.ReferenceBlocks).Select(b => b.GetText(true))),
				Is.EqualTo(Join(" ", englishReferenceBlocks.Skip(2).Take(2).Select(b => b.GetText(true)))));

			Assert.That(result[3].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{18}\u00A0et pleuré comme ils ont regardé la fumée de son embrasement: "));
			Assert.That(result[3].MatchesReferenceText);
			Assert.That(result[3].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo("{18}\u00A0and cried out as they looked at the smoke of her burning, saying, "));

			Assert.That(result[4].ReferenceBlocks.Single().GetText(true), Is.EqualTo(frenchReferenceBlocks.Last().GetText(true)));
			Assert.That(result[4].MatchesReferenceText);
			Assert.That(result[4].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo(englishReferenceBlocks.Last().GetText(true)));
		}

		/// <summary>
		/// PG-943
		/// </summary>
		[Test]
		public void ApplyTo_ChapterLabelFollowsMatchedBlock_SecondaryReferenceTextIsCorrect()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(CharacterVerse.kScriptureCharacter, 23, "Jadi omátám. Dená’ pun antang diam di kampong Nasaret di da’erah Galilea nyén...", false, 2),
				NewChapterBlock("MAT", 3),
				CreateNarratorBlockForVerse(1, "Vern-This is just needed because it is illegal to end with a chapter number.", true, 3)
			};

			var indonesianReferenceBlocks = new List<Block>
			{
				CreateBlockForVerse(CharacterVerse.kScriptureCharacter, 23, "<<Orang Nazaret.>>", false, 2),
				NewChapterBlock("MAT", 3),
				CreateNarratorBlockForVerse(1, "Indo-This is just needed because it is illegal to end with a chapter number.", true, 3)
			};

			var englishReferenceBlocks = new List<Block>
			{
				CreateBlockForVerse(CharacterVerse.kScriptureCharacter, 23, "“He will be called a Nazarene.”", false, 2),
				NewChapterBlock("MAT", 3, "Matthew 3"),
				CreateNarratorBlockForVerse(1, "Eng-This is just needed because it is illegal to end with a chapter number.", true, 3)
			};

			for (int i = 0; i < englishReferenceBlocks.Count; i++)
				indonesianReferenceBlocks[i].SetMatchedReferenceBlock(englishReferenceBlocks[i]);

			vernacularBlocks[0].SetMatchedReferenceBlock(indonesianReferenceBlocks[0]);

			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, indonesianReferenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(result[1].ReferenceBlocks.Single().GetPrimaryReferenceText(), Is.EqualTo("Matthew 3"));
		}

		[TestCase(null)]
		[TestCase("[")] // PG-760
		[TestCase("[ ")] // PG-760
		public void ApplyTo_VernacularHasBlockThatStartsWithVerseNotInReferenceText_ReferenceTextIsBrokenCorrectly(string openingBracket)
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(36, "And as they went on the way, they came unto a certain water; and the eunuch saith, ", true, 8, "ACT") };
			AddBlockForVerseInProgress(vernacularBlocks, "Ethiopian officer of Queen Candace",
				"«Behold, here is water; what doth hinder me to be baptized?»");
			var block = CreateNarratorBlockForVerse(37, "And Philip said, ");
			if (openingBracket != null)
				block.BlockElements.Insert(0, new ScriptText(openingBracket));
			vernacularBlocks.Add(block);
			AddBlockForVerseInProgress(vernacularBlocks, "Philip the evangelist", "«If you believe with all your heart, you may.» ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "And he answered and said, ", "ACT");
			AddBlockForVerseInProgress(vernacularBlocks, "Ethiopian officer of Queen Candace", "«I believe that Jesus Christ is the Son of God.»" +
				(openingBracket == null ? "" : "]"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(38, "And he gave orders to stop the chariot. Then both Philip and the eunuch went down " +
				"into the water and Philip baptized him.", false, 8, "ACT"));
			var vernBook = new BookScript("ACT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(36, "Yendo por el camino, llegaron a un lugar donde había agua; y el eunuco dijo: ", true, 8, "ACT") };
			AddBlockForVerseInProgress(referenceBlocks, "Ethiopian officer of Queen Candace", "«Mira, agua. ¿Qué impide que yo sea bautizado? ").AddVerse(38, "¡Para el carruaje!» ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Ambos descendieron al agua, Felipe y el eunuco, y lo bautizó.", "ACT");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);

			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[4].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[5].ReferenceBlocks.Count, Is.EqualTo(0));

			Assert.That(result[6].MatchesReferenceText);
			Assert.That(result[6].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo("{38}\u00A0¡Para el carruaje!» " + referenceBlocks.Last().GetText(true)));
		}

		[Test]
		public void ApplyTo_StandardReferenceTextSplitToMatchVernacular_ModifiedBooksGetReloadedWhenStandardReferenceTextIsGottenAgain()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1,
					"Ka doŋ ginywalo Yecu i Beterekem ma i Judaya i kare me loc pa kabaka Kerode, nen, luryeko mogo ma gua yo tuŋ wokceŋ " +
					"gubino i Jerucalem, kun gipenyo ni, ", true, 2),
				CreateBlockForVerse("magi (wise men from East)", 2,
					"“En latin ma ginywalo me bedo kabaka pa Lujudaya-ni tye kwene? Pien onoŋo waneno lakalatwene yo tuŋ wokceŋ, " +
					"ci man wabino ka wore.”", true, 2)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var origBlockCountForMatthew = refText.Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count;

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(refText.Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count,
				Is.Not.EqualTo(origBlockCountForMatthew));

			Assert.That(GetStandardReferenceText(ReferenceTextType.English)
				.Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count,
				Is.EqualTo(origBlockCountForMatthew));
		}

		[Test]
		public void ApplyTo_FirstTwoVersesLinkedNtoM_RemainingVersesMatchedCorrectly()
		{
			//Acholi MAT 9:23-26
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(23, "Ka Yecu obino i ot pa laloc, oneno lukut nyamulere, kun lwak tye ka koko, ", false, 9)
					.AddVerse(24, "ci owacci, ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Wua woko; nyakoni pe oto ento onino anina.” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Ci gin gunyere anyera. ")
				.AddVerse(25, "Ento i kare ma doŋ oryemo lwak gukato woko, ci en odonyo i ot omako latin nyako-nu ki ciŋe, nyako-nu oa malo. ")
				.AddVerse(26, "Lok meno oywek owinnye oromo lobo meno ducu.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(23, "When Jesus came into the ruler’s house and saw the flute players and the noisy crowd, ", false, 9)
					.AddVerse(24, "he said to them,")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Go away. The girl isn’t dead, but sleeping.”");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "They mocked him, saying:");
			AddBlockForVerseInProgress(referenceBlocks, "people at Jairus' house", "“The girl is dead!”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(25, "But when the crowd was put out, he entered in, took her by the hand, and the girl arose.", false, 9));
			referenceBlocks.Add(CreateNarratorBlockForVerse(26, "The report of this went out into all that land.", false, 9));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));

			Assert.That(result[0].ReferenceBlocks.Count + result[1].ReferenceBlocks.Count + result[2].ReferenceBlocks.Count, Is.EqualTo(4));

			Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[3].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[4].GetText(true)));
			Assert.That(result[3].MatchesReferenceText);

			Assert.That(result[4].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[4].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[5].GetText(true)));
			Assert.That(result[4].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_SomeBlocksPrematched_PrematchedVersesUnchangedAndOthersMatchedCorrectly()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true),
				CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "John", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Juan el");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“amado”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "This is what happened.", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "said Jesus. In response, John spake thusly: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "Cool! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "which translated, means “thank you.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			// Now pre-match the vernacular blocks for verse 2:
			vernacularBlocks[1].SetMatchedReferenceBlock(CreateNarratorBlockForVerse(2, "Then Jesus said:", true));
			vernacularBlocks[2].SetMatchedReferenceBlock(referenceBlocks[1].Clone());
			vernacularBlocks[2].ReferenceBlocks.Single().BlockElements.RemoveAt(0); // Get rid of verse number
			vernacularBlocks[3].SetMatchedReferenceBlock(referenceBlocks[3].Clone());
			vernacularBlocks[4].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = vernBook.NarratorCharacterId,
				BlockElements = new List<BlockElement> { new ScriptText("responded John the ") }
			});
			vernacularBlocks[5].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = vernBook.NarratorCharacterId,
				BlockElements = new List<BlockElement> { new ScriptText("beloved.") }
			});

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(7));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0Then Jesus said:"));
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(false)));
			Assert.That(result[3].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[3].GetText(true)));
			Assert.That(result[4].GetPrimaryReferenceText(), Is.EqualTo("responded John the "));
			Assert.That(result[5].GetPrimaryReferenceText(), Is.EqualTo("beloved."));
			Assert.That(result[6].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[5].GetText(true)));
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_SimpleVerseMapping_VersificationDifferencesResolvedBeforeMatching()
		{
			//GEN 32:1-32 = GEN 32:2-33
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(7, "Verse siete. ", true, 32, "GEN"),
				CreateNarratorBlockForVerse(8, "Verse ocho.", false, 32, "GEN")
			};
			var vernBook = new BookScript("GEN", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(6, "Verse 6. ", true, 32, "GEN"),
				CreateNarratorBlockForVerse(7, "Verse 7.", false, 32, "GEN")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
		}

		/// <summary>
		/// PG-786: Prevent skipping chunks of reference text when versification pulls stuff into an earlier chapter
		/// </summary>
		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_VersesMappedToEarlierChapter_InterveningMaterialMatches()
		{
			var chapterAnnouncementCharacter = GetStandardCharacterId("ROM", StandardCharacter.BookOrChapter);
			//ROM 14:24-26 = ROM 16:25-27
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(23, "Fourteen Twenty-three. ", true, 14, "ROM"),
				CreateNarratorBlockForVerse(24, "Fourteen Twenty-four.", true, 14, "ROM"),
				CreateNarratorBlockForVerse(25, "Fourteen Twenty-five.", false, 14, "ROM"),
				CreateNarratorBlockForVerse(26, "Fourteen Twenty-six.", false, 14, "ROM"),
				NewChapterBlock("ROM", 15),
				CreateNarratorBlockForVerse(1, "Fifteen One.", true, 15, "ROM"),
				CreateNarratorBlockForVerse(2, "Fifteen Two.", false, 15, "ROM"),
				NewChapterBlock("ROM", 16),
				CreateNarratorBlockForVerse(1, "Sixteen One.", true, 16, "ROM"),
				CreateNarratorBlockForVerse(23, "Gaius, whose hospitality...", true, 16, "ROM")
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Erastus sayas hi.", "ROM");
			var vernBook = new BookScript("ROM", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(vernacularBlocks.Take(9), ForEvery<Block>(b => b.MatchesReferenceText, Is.True)); // This could be 10, but that's not the point of this test

			Assert.That(result[0].GetPrimaryReferenceText(), Does.StartWith("{23}\u00A0"));
			Assert.That(result[0].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(14));
			Assert.That(result[1].GetPrimaryReferenceText(), Does.StartWith("{25}\u00A0"));
			Assert.That(result[1].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[2].GetPrimaryReferenceText(), Does.StartWith("{26}\u00A0"));
			Assert.That(result[2].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[3].GetPrimaryReferenceText(), Does.StartWith("{27}\u00A0"));
			Assert.That(result[3].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[4].ReferenceBlocks.Single().IsChapterAnnouncement);
			Assert.That(result[4].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(15));
			Assert.That(result[5].GetPrimaryReferenceText(), Does.StartWith("{1}\u00A0"));
			Assert.That(result[5].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(15));
			Assert.That(result[6].GetPrimaryReferenceText(), Does.StartWith("{2}\u00A0"));
			Assert.That(result[6].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(15));
			Assert.That(result[7].ReferenceBlocks.Single().IsChapterAnnouncement);
			Assert.That(result[7].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[8].GetPrimaryReferenceText(), Does.StartWith("{1}\u00A0"));
			Assert.That(result[8].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[9].ReferenceBlocks.Single().ChapterNumber, Is.EqualTo(16));
			Assert.That(result[9].GetPrimaryReferenceText(), Does.StartWith("{23}\u00A0"));
			Assert.That(result[10].ReferenceBlocks, Is.Empty);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_CrossChapterMapping_VersificationDifferencesResolvedBeforeMatching()
		{
			//EXO 8:1-4 = EXO 7:26-29
			//EXO 8:5-32 = EXO 8:1-28
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(29, "Versículo 29. ", true, 7, "EXO"),
				NewChapterBlock("EXO", 8, "Chapter 8"),
				CreateNarratorBlockForVerse(1, "Versículo 1.", false, 8, "EXO")
			};
			var vernBook = new BookScript("EXO", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				NewChapterBlock("EXO", 8, "Chapter 8"),
				CreateNarratorBlockForVerse(4, "Verse 4. ", true, 8, "EXO"),
				CreateNarratorBlockForVerse(5, "Verse 5.", false, 8, "EXO")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(vernacularBlocks.Count));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo("The Gospel According to Thomas 8"));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[2].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
			Assert.That(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_SplitVernacularBasedOnReference()
		{
			//EXO 8:5-32 = EXO 8:1-28
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true, 8) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Fred. ", "EXO");
			var block = CreateNarratorBlockForVerse(2, "Blah blah. ", false, 8, "EXO");
			block.AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("EXO", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(5, "I don't know if Fred told you this or not, but he's crazy. ", true, 8, "EXO"),
				CreateNarratorBlockForVerse(6, "This is your narrator speaking. ", false, 8, "EXO"),
				CreateNarratorBlockForVerse(7, "I hope you enjoy your flight. ", false, 8, "EXO"),
				CreateNarratorBlockForVerse(8, "The end. ", false, 8, "EXO")
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			Assert.That(referenceBlocks.Count, Is.EqualTo(4));
			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(result[0].GetText(true), Is.EqualTo("{1}\u00A0Cosas que Fred dice, "));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(0));

			Assert.That(result[1].GetText(true), Is.EqualTo("dijo Fred. "));
			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo("{5}\u00A0I don't know if Fred told you this or not, but he's crazy. "));

			Assert.That(result[2].GetText(true), Is.EqualTo("{2}\u00A0Blah blah. "));
			Assert.That(result[3].GetText(true), Is.EqualTo("{3}\u00A0More blah blah. "));
			Assert.That(result[4].GetText(true), Is.EqualTo("{4}\u00A0The final blah blah."));
			Assert.That(result.Skip(2).Select(v => v.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))), Is.True);
		}

		// PG-1085
		[Test]
		public void ApplyTo_SingleVoice_HasSectionHeadingMidVerse_AppliesCorrectly()
		{
			// Acts 10:23
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(23, "Narrator before SH", true, 10, "ACT") };
			AddBlockForVerseInProgress(vernacularBlocks, GetStandardCharacterId("ACT", StandardCharacter.ExtraBiblical), "SH", "s");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Narrator after SH", "ACT");
			var vernBook = new BookScript("ACT", vernacularBlocks, m_vernVersification)
			{
				SingleVoice = true
			};

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(23, "verse 23a in reftext - has no SH", true, 10, "ACT") };
			AddNarratorBlockForVerseInProgress(referenceBlocks, "verse 23b in reftext - has no SH", "ACT");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].GetPrimaryReferenceText(true), Is.EqualTo("verse 23a in reftext - has no SH"));
			Assert.That(result[1].GetPrimaryReferenceText(true), Is.Null);
			Assert.That(result[2].GetPrimaryReferenceText(true), Is.EqualTo("verse 23b in reftext - has no SH"));
		}

		// PG-1327
		[Test]
		public void ApplyTo_SingleVoice_MoreBlocksInVernacularThanInReferenceText_AppliesCorrectly()
		{
			// Mark 6:24
			var vernacularBlocks = new List<Block> {
				// This first block simulates the joining of a narrator block and the subsequent (ambiguous) block, which is
				// what happens in GetBooksWithBlocksConnectedToReferenceText when called from Project Exporter. The second
				// block is not joined (per current logic) because of the sentence-ending punctuation (?).
				CreateNarratorBlockForVerse(24, "Sara wɨdake' walyuna yaka ganaangevɨ wɨdɨna “Nɨmɨ berɨ'na wɨdɨjɨwana?”", true, 6, "MRK") };
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“ ‘Jonɨ Bavɨtaazɨa mɨnyagɨnya' dyaama!’ sara duthana!”");
			var vernBook = new BookScript("MRK", vernacularBlocks, m_vernVersification)
			{
				SingleVoice = true
			};

			var refText = GetStandardReferenceText(ReferenceTextType.Russian);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(2));

			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var russianRefTextBlock = result[0].ReferenceBlocks.Single();
			Assert.That(((Verse)russianRefTextBlock.BlockElements.First()).StartVerse, Is.EqualTo(24));
			var englishRefTextBlock = russianRefTextBlock.ReferenceBlocks.Single();
			Assert.That(((Verse)englishRefTextBlock.BlockElements.First()).StartVerse, Is.EqualTo(24));
			russianRefTextBlock = result[1].ReferenceBlocks.Single();
			englishRefTextBlock = russianRefTextBlock.ReferenceBlocks.Single();
			// Not entirely sure this is the best way, but rather than creating a dummy reference text or hard-coding the exact text as it is today,
			// I'm writing this test using a regex so that it should survive any minor wording changes. Both the Russian and the English reference
			// texts have the general pattern: Quote by Herod. Narrator introducing the reply. Quote by Herodias' daughter.
			const string kOpenQuote = "(“|\u00AB)";
			const string kCloseQuote = "(”|\u00BB)";
			var combinedQuoteNarratorQuoteRefTextMatcher = new Regex($"^{kOpenQuote}.+\\?{kCloseQuote} .+(:|,) {kOpenQuote}.+\\.{kCloseQuote}$", RegexOptions.Compiled);
			var russianRefTextPart2 = ((ScriptText)russianRefTextBlock.BlockElements.Single()).Content;
			var englishRefTextPart2 = ((ScriptText)englishRefTextBlock.BlockElements.Single()).Content;
			Assert.That(combinedQuoteNarratorQuoteRefTextMatcher.IsMatch(russianRefTextPart2), Is.True);
			Assert.That(combinedQuoteNarratorQuoteRefTextMatcher.IsMatch(englishRefTextPart2), Is.True);
		}

		// PG-1086
		[TestCase(true)]
		[TestCase(false)]
		public void ApplyTo_SectionHeadingMidVerse_ReferenceHasMoreThanOneBlockForVerse_FirstBlockMatchedAndRemainingBlocksNotMatched(bool singleVoice)
		{
			// Luke 9:42-43
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(42, "Mi tana bona na gari", true, 9, "LUK");
			block.AddVerse(43, "Mana vure subo tara");
			vernacularBlocks.Add(block);
			AddBlockForVerseInProgress(vernacularBlocks, GetStandardCharacterId("LUK", StandardCharacter.ExtraBiblical), "Jesus te ghoi bosaa na mateana", "s");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Tana bona na vure tara", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification)
			{
				SingleVoice = singleVoice
			};

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(42, "While he was still coming,", true, 9, "LUK"),
				CreateNarratorBlockForVerse(43, "They were all astonished and said:", true, 9, "LUK")
			};
			AddBlockForVerseInProgress(referenceBlocks, "everyone who witnessed healing of boy", "“Behold the majesty of God!”", "LUK");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But while all were marveling", "LUK");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(4));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));

			Assert.That(result[2].GetText(true), Is.EqualTo("Jesus te ghoi bosaa na mateana"));
			Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(0));
			Assert.That(result[2].MatchesReferenceText, Is.False);
			Assert.That(result[2].GetPrimaryReferenceText(), Is.Null);

			if (!singleVoice)
			{
				Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(2));
				Assert.That(result[3].MatchesReferenceText, Is.False);
				Assert.That(result[3].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
				Assert.That(result[3].ReferenceBlocks[1].GetText(true), Is.EqualTo(referenceBlocks[3].GetText(true)));
				Assert.That(result[3].GetPrimaryReferenceText(), Is.Null);
			}
			else
			{
				Assert.That(result[3].ReferenceBlocks.Count, Is.EqualTo(1));
				Assert.That(result[3].MatchesReferenceText);
				var expected = referenceBlocks[2].GetText(true) + " " + referenceBlocks[3].GetText(true);
				Assert.That(result[3].ReferenceBlocks[0].GetText(true), Is.EqualTo(expected));
				Assert.That(result[3].GetPrimaryReferenceText(), Is.EqualTo(expected));
			}
		}

		// PG-1086
		// I created this test to prove to myself that the one above had the correct expected results. I decided to leave it here rather than delete it.
		// So I named it the same as the one above but with _Control at the end. So the name doesn't match the condition but rather it is the same test
		// without a section heading.
		[TestCase(true)]
		[TestCase(false)]
		public void ApplyTo_SectionHeadingMidVerse_ReferenceHasMoreThanOneBlockForVerse_FirstBlockMatchedAndRemainingBlocksNotMatched_Control(bool singleVoice)
		{
			// Luke 9:42-43
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(42, "Mi tana bona na gari", true, 9, "LUK");
			block.AddVerse(43, "Mana vure subo tara");
			vernacularBlocks.Add(block);
			//AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.ExtraBiblical), "Jesus te ghoi bosaa na mateana", "s");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Tana bona na vure tara", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification)
			{
				SingleVoice = singleVoice
			};

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(42, "While he was still coming,", true, 9, "LUK"),
				CreateNarratorBlockForVerse(43, "They were all astonished and said:", true, 9, "LUK")
			};
			AddBlockForVerseInProgress(referenceBlocks, "everyone who witnessed healing of boy", "“Behold the majesty of God!”",
				"LUK");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But while all were marveling", "LUK");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook);

			var result = vernBook.GetScriptBlocks();
			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));

			if (!singleVoice)
			{
				Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(2));
				Assert.That(result[2].MatchesReferenceText, Is.False);
				Assert.That(result[2].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[2].GetText(true)));
				Assert.That(result[2].ReferenceBlocks[1].GetText(true), Is.EqualTo(referenceBlocks[3].GetText(true)));
				Assert.That(result[2].GetPrimaryReferenceText(), Is.Null);
			}
			else
			{
				Assert.That(result[2].ReferenceBlocks.Count, Is.EqualTo(1));
				Assert.That(result[2].MatchesReferenceText);
				Assert.That(result[2].ReferenceBlocks[0].GetText(true), Is.EqualTo("“Behold the majesty of God!” But while all were marveling"));
				Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo("“Behold the majesty of God!” But while all were marveling"));
			}
		}

		// PG-1374 Note: Skipping a verse (v. 1 in this case) is of course really unlikely, and it
		// would be the "perfect storm" if it happened to occur at a place where the versifications
		// mismatched and the reference text had two verses combined in a single block. Although I
		// have not seen this case in the wild, I noticed (in the course of tracking down the real
		// problem reported in this issue) that the code failed to account for versification in
		// this case, so I have added the test cases for it and fixed the logic accordingly.
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void ApplyTo_VernVerseAtStartOfChapterMapsToRefBlockWithTwoVersesInPrevChapter_RefBlockSplitToMatch(bool includeV1, bool includeSectionHead)
		{
			var customVersification = Versification.Table.Implementation.Load(new StringReader(
					"# Versification  \"Custom\"\r\n" +
					"JOB 1:22 2:13 3:26 4:21 5:27 6:30 7:21 8:22 9:35 10:22 11:20 12:25 13:28 14:22 15:35 16:22 17:16 18:21 19:29 20:29 21:34 22:30 23:17 24:25 25:6 26:14 27:23 28:28 29:25 30:31 31:40 32:22 33:33 34:37 35:16 36:33 37:24 38:38 39:38 40:28 41:25 42:17\r\n" +
					"JOB 39:1-3 = JOB 38:39-41"),
				"IndonesianExample", "Custom");

			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse("God", 36, "Siapagah yang hikmat di batin? Atau, siapagah pangertian pepada pikiren? ", true, 38)
					.AddVerse(37, "Siapekah yang menghitung awan hikmat? Siepaka fapat isi kirbot langit, ")
					.AddVerse(38, "ketika tarcedak jodi keras gumpalan tanah berhekatan?"),
				NewChapterBlock("JOB", 39)
			};
			if (includeSectionHead)
			{ 
				vernacularBlocks.Add(new Block("s", 39)
					{
						CharacterId = GetStandardCharacterId("JOB", StandardCharacter.ExtraBiblical),
						BlockElements = new List<BlockElement>(new [] {new ScriptText("Habi Ayob Mituntanj")})
					}
				);
			}
			if (includeV1)
				vernacularBlocks.Add(CreateBlockForVerse("God", 1, "Dalatah engkau memburu, ateu memoaskan naxsu makan singas cuda ", true, 39));
			vernacularBlocks.Add(CreateBlockForVerse("God", 2, "ketika mereka meringkuk semak untuk menyergap? ", false, 39));
			vernacularBlocks.Add(CreateBlockForVerse("God", 3, "Siarakah yong menyediakan, ketika anoknya barteriak kepada Allah tidak aza makanan? ", false, 39));

			var vernBook = new BookScript("JOB", vernacularBlocks, customVersification);
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			Assert.That(vernBook.GetScriptBlocks()
				.Where(b => !b.CharacterIs("JOB", StandardCharacter.ExtraBiblical)),
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}
		
		// PG-1374
		[Test]
		public void ApplyTo_MappingOfHebrewSubtitlesWhenChaptersDifferInVersification_RefBlockSplitToMatch()
		{
			// The following is excerpted from Russian Orthodox (rso.vrs)
			var customVersification = Versification.Table.Implementation.Load(new StringReader(
					"# Versification  \"Custom\"\r\n" +
					"PSA 1:6 2:12 3:9 4:9 5:13 6:11 7:18 8:10 9:39 10:7 11:9 12:6 13:7 14:5 15:11\r\n" +
					"PSA 9:22 = PSA 10:0\r\n" + // Note: Psalm 10 has no Hebrew subtitle, so there really is no verse 0.
					"PSA 9:22-39 = PSA 10:1-18\r\n" +
					"PSA 10:0-7 = PSA 11:0-7\r\n" +
					"PSA 11:0-9 = PSA 12:0-9"),
				"IndonesianExample", "Custom");

			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(22, "Salmo 9-22 (10:1 in English). ", true, 9, "PSA")
					.AddVerse(23, "Salmo 9-23 (10:2 in English), ")
					.AddVerse(24, "Salmo 9-24 (10:3 in English), ")
					.AddVerse(25, "Salmo 9-25 (10:4 in English), ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "man, wicked", "“God is not even on my radar.”", "q1");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(26, "Salmo 9-26 (10:5 in English), ", true, 9, "PSA")
				.AddVerse(27, "Salmo 9-27 (10:6 in English), "));
			AddBlockForVerseInProgress(vernacularBlocks, "man, wicked", "“Can't mess with me. I'm on top and my great grandkids have got it made.”", "q1");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(28, "Salmo 9-28 (10:7 in English). ", true, 9, "PSA")
				.AddVerse(29, "Salmo 9-29 (10:8 in English), ")
				.AddVerse(30, "Salmo 9-30 (10:9 in English), ")
				.AddVerse(31, "Salmo 9-31 (10:10 in English), ")
				.AddVerse(32, "Salmo 9-32 (10:11 in English), "));
			AddBlockForVerseInProgress(vernacularBlocks, "man, wicked", "“God can't even see me. I can get away with murder.”", "q1");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(33, "Salmo 9-33 (10:12 in English), ", true, 9, "PSA")
				.AddVerse(34, "Salmo 9-34 (10:13 in English), "));
			AddBlockForVerseInProgress(vernacularBlocks, "man, wicked", "“God will never judge me.”", "q1");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(35, "Salmo 9-35 (10:14 in English), ", true, 9, "PSA")
				.AddVerse(36, "Salmo 9-36 (10:15 in English), ")
				.AddVerse(37, "Salmo 9-37 (10:16 in English), ")
				.AddVerse(38, "Salmo 9-38 (10:17 in English), ")
				.AddVerse(39, "Salmo 9-39 (10:18 in English)."));
			vernacularBlocks.Add(NewChapterBlock("PSA", 10));
			// For Psalm 11 in Russian (Psalm 10 in original), the Hebrew subtitle text is included
			// together with the text that appears as verse 1 in English.
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "For the director of music. Of David. Also, Salmo 10-1, ... how can you say: (11:1 in Original, 11:0-1 in English)", true, 10, "PSA"));
			AddBlockForVerseInProgress(vernacularBlocks, "someone (hypothetical argument)", "“Fly like a fowl to the high hill”?", "q1");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Salmo 10-2 (11:2 in Original, 11:2 in English)", true, 10, "PSA"));

			var vernBook = new BookScript("PSA", vernacularBlocks, customVersification);
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			Assert.That(vernBook.GetScriptBlocks()
				.Where(b => !b.CharacterIs("PSA", StandardCharacter.ExtraBiblical) && b.InitialStartVerseNumber != 1),
				ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}
		
		// PG-1386
		[Test]
		public void ApplyTo_MappingSplitsVerseAcrossChapterBreakWhereRefHasFirstTwoVersesInSingleBlock_RefBlockAppliedButNotConsideredAsMatch()
		{
			// Note: Standard English vrs has mapping: ISA 64:2-12 = ISA 64:1-11 (i.e., v. 2 in English is actually part of v. 1 in Original)

			// Vernacular	-> Original		-> English
			// ISA 63:19	-> ISA 63:19	-> ISA 63:19 (no mapping for this verse in English)
			// ISA 64:1		-> ISA 63:19	-> ISA 64:2
			// ISA 64:2		-> ISA 64:1		-> ISA 64:2
			// So this basically means that there is NO VERSE in English that corresponds to ISA 64:1! Both 64:1 and 64:2 map to 64:2

			// Going the other direction:
			// English		-> Original		-> Vernacular
			// ISA 63:19	-> ISA 63:19	-> AMBIGUOUS: ISA 63:19 or ISA 64:1!
			// ISA 64:1		-> ISA 64:1		-> ISA 64:2
			// ISA 64:2		-> ISA 64:1		-> ISA 64:2 (both go to the same verse!)

			var customVersification = Versification.Table.Implementation.Load(new StringReader(
					"# Versification  \"Custom\"\r\n" +
					"ISA 1:31 2:22 3:26 4:6 5:30 6:13 7:25 8:23 9:20 10:34 11:16 12:6 13:22 14:32 15:9 16:14 17:14 18:7 19:25 20:6 21:17 22:25 23:18 24:23 25:12 26:21 27:13 28:29 29:24 30:33 31:9 32:20 33:24 34:17 35:10 36:22 37:38 38:22 39:8 40:31 41:29 42:25 43:28 44:28 45:25 46:13 47:15 48:22 49:26 50:11 51:23 52:15 53:12 54:17 55:13 56:12 57:21 58:14 59:21 60:22 61:11 62:12 63:19 64:12 65:25 66:24\r\n" +
					"ISA 63:19 = ISA 63:19\r\n" +
					"ISA 64:1 = ISA 63:19\r\n" +
					"ISA 64:2-12 = ISA 64:1-11"),
				"pg1386", "Custom");

			var vernacularBlocks = new List<Block> { NewChapterBlock("ISA", 63) };
			for (var i = 1; i <= 19; i++)
				vernacularBlocks.Add(CreateNarratorBlockForVerse(i, $"Isaiah 39:{i}. ", true, 63, "ISA"));
			vernacularBlocks.Add(NewChapterBlock("ISA", 64));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Koyakkanlah langit lalo terun, sehingga gunung laluh de halapan-Mu — ", true, 64, "ISA")
				.AddVerse(2, "seperti agi menpalatan semakapi didihkan wir. Bwatlah dikenal dole lawan-Mu, sehinga bangso gametir hadapan-Mu. ")
				.AddVerse(3, "Ketika elakukan hal yong dasyat, yang sangka, Engkau tirun, dax gungunung leluh hadapan-Mu."));

			var vernBook = new BookScript("ISA", vernacularBlocks, customVersification);
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook);

			var firstScrBlockInCh64 = vernBook.GetScriptBlocks().Single(b => b.ChapterNumber == 64 && b.InitialStartVerseNumber == 1);
			Assert.That(firstScrBlockInCh64.MatchesReferenceText, Is.False);
			Assert.That(firstScrBlockInCh64.ReferenceBlocks.First().InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(firstScrBlockInCh64.ReferenceBlocks.Last().LastVerseNum, Is.EqualTo(2));
		}

		[Test]
		public void ApplyTo_SingleVoice_MappingSplitsVerseAcrossChapterBreakWhereRefHasFirstTwoVersesInSingleBlock_RefBlockMatched()
		{
			// Note: Standard English vrs has mapping: ISA 64:2-12 = ISA 64:1-11 (i.e., v. 2 in English is actually part of v. 1 in Original)

			// Vernacular	-> Original		-> English
			// ISA 63:19	-> ISA 63:19	-> ISA 63:19 (no mapping for this verse in English)
			// ISA 64:1		-> ISA 63:19	-> ISA 64:2
			// ISA 64:2		-> ISA 64:1		-> ISA 64:2
			// So this basically means that there is NO VERSE in English that corresponds to ISA 64:1! Both 64:1 and 64:2 map to 64:2

			// Going the other direction:
			// English		-> Original		-> Vernacular
			// ISA 63:19	-> ISA 63:19	-> AMBIGUOUS: ISA 63:19 or ISA 64:1!
			// ISA 64:1		-> ISA 64:1		-> ISA 64:2
			// ISA 64:2		-> ISA 64:1		-> ISA 64:2 (both go to the same verse!)

			var customVersification = Versification.Table.Implementation.Load(new StringReader(
					"# Versification  \"Custom\"\r\n" +
					"ISA 1:31 2:22 3:26 4:6 5:30 6:13 7:25 8:23 9:20 10:34 11:16 12:6 13:22 14:32 15:9 16:14 17:14 18:7 19:25 20:6 21:17 22:25 23:18 24:23 25:12 26:21 27:13 28:29 29:24 30:33 31:9 32:20 33:24 34:17 35:10 36:22 37:38 38:22 39:8 40:31 41:29 42:25 43:28 44:28 45:25 46:13 47:15 48:22 49:26 50:11 51:23 52:15 53:12 54:17 55:13 56:12 57:21 58:14 59:21 60:22 61:11 62:12 63:19 64:12 65:25 66:24\r\n" +
					"ISA 63:19 = ISA 63:19\r\n" +
					"ISA 64:1 = ISA 63:19\r\n" +
					"ISA 64:2-12 = ISA 64:1-11"),
				"pg1386", "Custom");

			var vernacularBlocks = new List<Block> { NewChapterBlock("ISA", 63) };
			for (var i = 1; i <= 19; i++)
				vernacularBlocks.Add(CreateNarratorBlockForVerse(i, $"Isaiah 39:{i}. ", true, 63, "ISA"));
			vernacularBlocks.Add(NewChapterBlock("ISA", 64));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Koyakkanlah langit lalo terun, sehingga gunung laluh de halapan-Mu — ", true, 64, "ISA")
				.AddVerse(2, "seperti agi menpalatan semakapi didihkan wir. Bwatlah dikenal dole lawan-Mu, sehinga bangso gametir hadapan-Mu. ")
				.AddVerse(3, "Ketika elakukan hal yong dasyat, yang sangka, Engkau tirun, dax gungunung leluh hadapan-Mu."));

			var vernBook = new BookScript("ISA", vernacularBlocks, customVersification)
			{
				SingleVoice = true
			};
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refBookIsa = refText.Books[BCVRef.BookToNumber("ISA") - 1];

			refText.ApplyTo(vernBook);

			var firstScrBlockInCh64 = vernBook.GetScriptBlocks().Single(b => b.ChapterNumber == 64 && b.InitialStartVerseNumber == 1);
			Assert.That(firstScrBlockInCh64.MatchesReferenceText);
			var matchedRefBlock = firstScrBlockInCh64.ReferenceBlocks.Single();
			Assert.That(matchedRefBlock.InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(matchedRefBlock.LastVerseNum, Is.EqualTo(2));
			// The following not only ensures we got the correct content, it also ensures that the matched
			// block was cloned so that the reference text was not modified.
			Assert.That(((ScriptText)matchedRefBlock.BlockElements[1]).Content,
				Is.EqualTo(refBookIsa.GetBlocksForVerse(64, 1).Single().BlockElements.OfType<ScriptText>().Single().Content + " "));
			Assert.That(((ScriptText)matchedRefBlock.BlockElements[3]).Content,
				Is.EqualTo(refBookIsa.GetBlocksForVerse(64, 2).Single().BlockElements.OfType<ScriptText>().Single().Content));
		}


		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_WholeBookOfJude_AppliedCorrectly()
		{
			var expectedVernacularResults = new []
			{
				"WARAGA ma JUDA ocoyo",
				"JUDA 1",
				"{1}\u00A0Waraga man oa ki bot Juda ma latic pa Yecu Kricito, ma omin Yakobo.",
				"Bot jo ma Lubaŋa olwoŋogi ma gibedo i mar pa Lubaŋa Won, ma Yecu Kricito tye ka gwokogi.",
				"{2}\u00A0Kica, kuc ki mar myero omedde botwu.",
				"Lupwony ma lugoba",
				"{3}\u00A0Lurema, onoŋo atemo ki tekka ducu me coyo botwu waraga pi lok i kom larre ma wan ducu waribbe iye, ci aneno ni myero aco acuk kwede cwinywu wek wulweny matek pi niye ma yam doŋ gimiyo bot jo pa Lubaŋa kicel ma doŋ otyeko lok ducu. ",
				"{4}\u00A0Pien jo mogo ma pe gilworo Lubaŋa gilibbe aliba me donyo i kinwa, gin jo ma kop ma yam giŋolo i komgi doŋ ginyuto woko con i coc ma yam gicoyo. Gin guloko kica ma Lubaŋa omiyowa odoko tim me coco, gukwero Laditwa acel keken-ni ma en aye Rwotwa Yecu Kricito.",
				"{5}\u00A0Wun jo ma yam doŋ gimiyo wuŋeyo lok man con, koŋ amito dok apo wiwu ni, Rwot ma yam olaro jo Icrael ki i lobo Ejipt, lacenne en dok otyeko jo ma pe guye en woko. ",
				"{6}\u00A0Lumalaika ma yam pe gigwoko ka locgi ma gimiyo botgi, ento guweko kabedogi woko, en ogwokogi ka macol ki nyor ma ri nakanaka, nio wa i kare me ŋolo kop i nino madit; ",
				"{7}\u00A0kit macalo yam jo Codom ki Gomora ki gaŋi madoŋo ma i ŋetgi, gudonyo i tim me coco ki par maraco ma pe obedo me anywalli, macalo gin, ci giŋolo botgi can me mac ma pe to, gin mubedo macalo lanen bot jo ducu.",
				"{8}\u00A0Nen kadi kit meno, jo-nu bene i kit acel-lu, i lek ma gin leko, gubalo komgi woko, gukwero ker pa Lubaŋa woko, dok guyeto wegi deyo-gu bene ayeta. ",
				"{9}\u00A0Ento i kare ma Mikael, lamalaika madit, yam olwenyo ki Catan kun pyem kwede pi kom Moses, en yam pe oŋwette pi ŋolo kop mo me yet, ento oloko ni, ",
				"“Rwot myero ojuki.” ",
				"{10}\u00A0Ento jo magi giyeto gin ma pe giniaŋ iye, ento gin ma giŋeyo pi lubo kit ma giketogi kwede, macalo lee ma pe ryek, tye ka tyekogi woko. ",
				"{11}\u00A0Gibineno can ma rom man! Pien gilubo kit pa Kain, giketo tek mada i tam mogo ma pe atir pi mito lim kit macalo yam Balaam otimo, dok bene gito woko pi pyem kit macalo yam Kora ojemo kwede, ",
				"{12}\u00A0Gikelo lewic i karamawu me mar ka gicamo matek mukato kare laboŋo lworo, kun giparo pi komgi keken. Gubedo calo pol ma pii pe iye ma yamo kolo; girom ki yadi ma nyiggi pe nen i kare me cekgi, ma giputo lwitgi woko, yam guto kiryo. ",
				"{13}\u00A0Gical bene ki nam ma twagge mager ki bwoyo me lewicgi; gin lakalatwe ma wirre atata ma Lubaŋa otyeko yubo kakagi woko i kabedo macol licuc pi kare ma pe gik.",
				"{14}\u00A0Magi gin aye gin ma yam Enoka, ma obedo dano me abiro nia i kom Adam, yam otito pire macalo lanebi ni, ",
				"“Wunen, Rwot bibino ki lwak jone maleŋ mapol ata, ",
				"{15}\u00A0ka ŋolo kop i kom jo ducu, ki ka miyo kop wek olo jo ducu ma pe gilworo Lubaŋa pi timgi ma pe gilworo kwede Lubaŋa, ki pi lok ducu me gero ma lubalo ma pe gilworo Lubaŋa guloko i kome.”",
				"{16}\u00A0Meno gin jo ma bedo kar ŋur aŋura, ma gipyem apyema, kun gilubo mitgi keken, doggi opoŋ ki loko lok me wakke keken, gidworo dano wek ginoŋ gin ma cwinygi mito.",
				"Lok me ciko dano ki pwony",
				"{17}\u00A0Ento wun lurema ma amaro, myero wupo i lok ma yam lukwena pa Rwotwa Yecu Kricito gutito pire con. ",
				"{18}\u00A0Gin yam gutito botwu ni, ",
				"“I kare me agikki luŋala bibedo tye, ma gibilubo mitigi keken, ma pe gilworo Lubaŋa.” ",
				"{19}\u00A0Jo ma kit meno gin aye lukel apokapoka, gin jo me lobo man, Cwiny pa Lubaŋa pe botgi. ",
				"{20}\u00A0Ento wun luwota, wudoŋ matek i niyewu maleŋ twatwal-li, kun wulego Lubaŋa i Cwiny Maleŋ. ",
				"{21}\u00A0Wugwokke kenwu i mar pa Lubaŋa, wukur kica pa Rwotwa Yecu Kricito nio wa i kwo ma pe tum. ",
				"{22}\u00A0Wubed ki kica i kom jo mogo ma gitye ka cabbe acaba, ",
				"{23}\u00A0wular jo mogo ma kit meno kun wuceyogi woko ki i mac. Wubed ki kica i kom jo mogo kun wulworo bene, dok wukwer bene ruk ma bal me kom tye iye.",
				"Miyo deyo",
				"{24}\u00A0Deyo obed bot Ŋat ma twero gwokowu miyo pe wupoto, dok ma twero miyo wucuŋ laboŋo roc mo i nyim deyone ki yomcwiny. ",
				"{25}\u00A0Deyo, dit, loc ki twer ducu obed bot Lubaŋa acel keken, ma Lalarwa, pi Yecu Kricito Rwotwa, cakke ma peya giketo lobo, nio koni, ki kare ma pe gik. Amen.",
			};
			var expectedReferenceResults = new []
			{
				"JUDE",
				"JUDE 1",
				"{1}\u00A0Jude, a servant of Jesus Christ, and brother of James, to those who are called, sanctified by God the Father, and kept for Jesus Christ:",
				null,
				"{2}\u00A0Mercy to you and peace and love be multiplied.",
				null,
				"{3}\u00A0Beloved, while I was very eager to write to you about our common salvation, I was constrained to write to you exhorting you to contend earnestly for the faith which was once for all delivered to the saints.",
				"{4}\u00A0For there are certain men who crept in secretly, even those who were long ago written about for this condemnation: ungodly men, turning the grace of our God into indecency, and denying our only Master, God, and Lord, Jesus Christ.",
				"{5}\u00A0Now I desire to remind you, though you already know this, that the Lord, having saved a people out of the land of Egypt, afterward destroyed those who didn’t believe.",
				"{6}\u00A0Angels who didn’t keep their first domain, but deserted their own dwelling place, he has kept in everlasting bonds under darkness for the judgment of the great day.",
				"{7}\u00A0Even as Sodom and Gomorrah, and the cities around them, having, in the same way as these, given themselves over to sexual immorality and gone after strange flesh, are set forth as an example, suffering the punishment of eternal fire.",
				"{8}\u00A0Yet in the same way, these also in their dreaming defile the flesh, despise authority, and slander celestial beings.",
				"{9}\u00A0But Michael, the archangel, when contending with the devil and arguing about the body of Moses, dared not bring against him an abusive condemnation, but said, ",
				"“May the Lord rebuke you!”",
				"{10}\u00A0But these speak evil of whatever things they don’t know. What they understand naturally, like the creatures without reason, they are destroyed in these things.",
				"{11}\u00A0Woe to them! For they went in the way of Cain, and ran riotously in the error of Balaam for hire, and perished in Korah’s rebellion.",
				"{12}\u00A0These men are hidden rocky reefs in your love feasts when they feast with you, shepherds who without fear feed themselves; clouds without water, carried along by winds. They are like autumn leaves without fruit, twice dead, plucked up by the roots.",
				"{13}\u00A0They are like wild waves of the sea, foaming out their own shame; wandering stars, for whom the blackness of darkness has been reserved forever.",
				"{14}\u00A0About these also Enoch, the seventh from Adam, prophesied, saying,",
				"“Behold, the Lord came with ten thousands of his holy ones,[1]",
				"{15}\u00A0to execute judgment on all, and to convict all the ungodly of all their works of ungodliness which they have done in an ungodly way, and of all the hard things which ungodly sinners have spoken against him. [1]”",
				"{16}\u00A0These are murmurers and complainers, walking after their lusts (and their mouth speaks proud things), showing respect of persons to gain advantage.",
				null,
				"{17}\u00A0But you, beloved, remember the words which have been spoken before by the apostles of our Lord Jesus Christ.",
				"{18}\u00A0They said to you that ",
				"“In the last time there will be mockers, walking after their own ungodly lusts.”",
				"{19}\u00A0These are they who cause divisions, and are sensual, not having the Spirit.",
				"{20}\u00A0But you, beloved, keep building up yourselves on your most holy faith, praying in the Holy Spirit.",
				"{21}\u00A0Keep yourselves in the love of God, looking for the mercy of our Lord Jesus Christ to eternal life.",
				"{22}\u00A0Be merciful to those who doubt.",
				"{23}\u00A0Snatch others from the fire and save them. To others show mercy mixed with fear, hating even the clothing stained by the flesh.",
				null,
				"{24}\u00A0Now to him who is able to keep them from stumbling, and to present you faultless before the presence of his glory in great joy --",
				"{25}\u00A0to God our Savior, who alone is wise, be glory and majesty, dominion and power, both now and forever. Amen.",
			};

			var jude = TestReferenceText.CreateTestReferenceText(ReferenceTextTestUtils.GetBookContents(TestReferenceTextResource.EnglishJUD))
				.GetBooksWithBlocksConnectedToReferenceText(TestProject.CreateBasicTestProject(), false).Single();
			StringBuilder sbForVernacularResults = new StringBuilder();
			StringBuilder sbForReferenceTextResults = new StringBuilder();
			for (int i = 0; i < jude.Blocks.Count; i++)
			{
				var block = jude.Blocks[i];
				if (expectedVernacularResults[i] != block.GetText(true))
					sbForVernacularResults.Append("Expected: ").Append(expectedVernacularResults[i]).AppendLine()
						.Append("Actual: ").Append(block.GetText(true)).AppendLine().AppendLine();
				if (block.MatchesReferenceText || expectedReferenceResults[i] == null)
				{
					if (expectedReferenceResults[i] != block.GetPrimaryReferenceText())
						sbForReferenceTextResults.Append("Expected: ").Append(expectedReferenceResults[i]).AppendLine()
							.Append("Actual: ").Append(block.GetPrimaryReferenceText()).AppendLine().AppendLine();
				}
				else
				{
					var splits = expectedReferenceResults[i].Split('~');
					if (block.ReferenceBlocks.Count != splits.Length)
						sbForReferenceTextResults.Append("Expected: ").Append(splits.Length).Append(" reference blocks in verse ").Append(block.InitialVerseNumberOrBridge).AppendLine()
							.Append("Actual: ").Append(block.ReferenceBlocks.Count).Append(" reference blocks in verse ").Append(block.InitialVerseNumberOrBridge).AppendLine().AppendLine();
				}
			}

			bool failed = false;
			if (sbForVernacularResults.Length > 0)
			{
				sbForVernacularResults.Insert(0, "*****VERNACULAR TEXT FAILURES:*****" + Environment.NewLine);
				failed = true;
				Debug.WriteLine(sbForVernacularResults.ToString());
			}
			if (sbForReferenceTextResults.Length > 0)
			{
				sbForReferenceTextResults.Insert(0, "*****REFERENCE TEXT FAILURES:*****" + Environment.NewLine);
				failed = true;
				Debug.WriteLine(sbForReferenceTextResults.ToString());
			}

			if (failed)
				Assert.Fail();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetBooksWithBlocksConnectedToReferenceText_ReferenceTextDoesNotContainBook_NoChangeToVernacular(bool applyNarratorOverrides)
		{
			var refTextForJude = TestReferenceText.CreateTestReferenceText(ReferenceTextTestUtils.GetBookContents(TestReferenceTextResource.EnglishJUD));
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);
			var blocksBeforeCall = testProject.IncludedBooks[0].GetScriptBlocks();
			var result = refTextForJude.GetBooksWithBlocksConnectedToReferenceText(testProject, applyNarratorOverrides);
			var resultBlocks = result.Single().GetScriptBlocks();
			Assert.That(blocksBeforeCall.Select(b => b.GetText(true)).SequenceEqual(resultBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(resultBlocks, ForEvery<Block>(b =>
				(b.ReferenceBlocks == null || !b.ReferenceBlocks.Any()) && !b.MatchesReferenceText, Is.True));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_VernacularHasSubsequentBlocksWithSameCharacter_BlocksAreJoinedThenSplitBasedOnReference()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "El cual significa, ", true) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“Dios con nosotros.” ").AddVerse(2, "Blah blah. ");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Which means, God with us.", true),
				CreateNarratorBlockForVerse(2, "This is your narrator speaking. ")
			};
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("JUD", referenceBlocks);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(2));

			Assert.That(result[0].GetText(true), Is.EqualTo("{1}\u00A0El cual significa, “Dios con nosotros.” "));
			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[0].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));

			Assert.That(result[1].GetText(true), Is.EqualTo("{2}\u00A0Blah blah. "));
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true)));
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_SomeBlocksPrematched_PrematchedVersesUnchangedAndOthersMatchedCorrectly()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true),
				CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "John", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Juan el");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“amado”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "This is what happened.", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "said Jesus. In response, John spake thusly: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "Cool! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "which translated, means “thank you.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));

			// Now pre-match the vern blocks for verse 2:
			var narrator = GetStandardCharacterId("JUD", StandardCharacter.Narrator);
			vernacularBlocks[1].SetMatchedReferenceBlock(CreateNarratorBlockForVerse(2, "Then Jesus said:", true));
			vernacularBlocks[2].SetMatchedReferenceBlock(referenceBlocks[1].Clone());
			vernacularBlocks[2].ReferenceBlocks.Single().BlockElements.RemoveAt(0); // Get rid of verse number
			vernacularBlocks[3].SetMatchedReferenceBlock(referenceBlocks[3].Clone());
			vernacularBlocks[4].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = narrator,
				BlockElements = new List<BlockElement> { new ScriptText("responded John the ") }
			});
			vernacularBlocks[5].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = narrator,
				BlockElements = new List<BlockElement> { new ScriptText("beloved.") }
			});

			var metadata = new GlyssenDblTextMetadata {Language = new GlyssenDblMetadataLanguage {Name = "Doublespeak"}};
			TestReferenceText.ForgetCustomReferenceTexts();
			var persistenceImpl = (IProjectPersistenceWriter)ReferenceTextProxy.Reader;
			var customDoubleSpeakReferenceTextId = new ReferenceTextId(ReferenceTextType.Custom, "Doublespeak");
			persistenceImpl.SetUpProjectPersistence(customDoubleSpeakReferenceTextId);
			using (var writer = persistenceImpl.GetTextWriter(customDoubleSpeakReferenceTextId, ProjectResource.Metadata))
			{
				XmlSerializationHelper.Serialize(writer, metadata, out var e);
				Assert.That(e, Is.Null);
			}
			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, "Doublespeak"));
			var books = (List<BookScript>)primaryReferenceText.Books;
			var refBook = new BookScript(testProject.Books[0].BookId, referenceBlocks, primaryReferenceText.Versification);
			books.Add(refBook);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(6));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(result[0].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo("{2}\u00A0Then Jesus said:"));
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(false)));
			Assert.That(result[3].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[3].GetText(true)));
			Assert.That(result[4].GetPrimaryReferenceText(), Is.EqualTo("responded John the beloved."));
			Assert.That(result[5].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[5].GetText(true)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetBooksWithBlocksConnectedToReferenceText_SomeBlocksPrematchedAndJoined_PrematchedVersesDoNotGetResplit(bool applyNarratorOverrides)
		{
			var vernacularBlocks = new List<Block>
			{
				NewChapterBlock("MAT", 3),
				CreateNarratorBlockForVerse(1, "Een dat time, John wa Bactize come ta de wildaness een Judea, an e staat fa preach dey. ", true, 3),
				CreateNarratorBlockForVerse(2, "E tell um say, ", chapter:3)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "John the Baptist", "“Oona mus change oona sinful way an dohn do um no mo. Cause de time mos yah wen God gwine rule oba we!” ");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "John been de man wa de prophet Isaiah beena taak bout wen e say,", chapter: 3));
			AddBlockForVerseInProgress(vernacularBlocks, CharacterVerse.kScriptureCharacter, "“Somebody da holla een de wildaness say, ‘Oona mus cleah de road weh de Lawd gwine come shru.", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, CharacterVerse.kScriptureCharacter, "Mek de pat scraight fa um fa waak!’ ”", "q2");

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			testProject.Books[0].Blocks = vernacularBlocks;

			// Now pre-match the vern blocks for verses 1-2:
			vernacularBlocks[1].SetMatchedReferenceBlock("In those days, John the Baptizer came, preaching in the wilderness of Judea,");
			vernacularBlocks[2].SetMatchedReferenceBlock("...{2} saying,");
			vernacularBlocks[3].SetMatchedReferenceBlock("“Repent, for the Kingdom of Heaven is at hand!”");

			var result = GetStandardReferenceText(ReferenceTextType.English)
				.GetBooksWithBlocksConnectedToReferenceText(testProject, applyNarratorOverrides).Single().GetScriptBlocks();

			Assert.That(result.Count, Is.EqualTo(6));
			Assert.That(result[0].MatchesReferenceText && result[0].ReferenceBlocks.Single().IsChapterAnnouncement);
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].MatchesReferenceText);
			// These final three blocks aren't of major importance in this test, but here's how they shake out:
			Assert.That(result[3].MatchesReferenceText);
			Assert.That(result[4].MatchesReferenceText);
			Assert.That(result[5].MatchesReferenceText, Is.False);

			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo("In those days, John the Baptizer came, preaching in the wilderness of Judea, {2}\u00A0saying,"));
			Assert.That(result[2].GetPrimaryReferenceText(), Is.EqualTo("“Repent, for the Kingdom of Heaven is at hand!”"));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_VernacularContainsQBlocks_ReferenceTextSingleBlock_VernacularBlocksCombined()
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Peter", 1, "Juan dijo, ", true) };
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "'This es estrofa 1, ", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 2, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 3, ", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 4.'", "q2");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block> { CreateBlockForVerse("Peter", 1, "John said, 'This is line 1, This is line 2, This is line 3, This is line 4.'", true) };

			const string kPoetian = "Poetian";
			var metadata = new GlyssenDblTextMetadata
			{
				Language = new GlyssenDblMetadataLanguage { Name = kPoetian }
			};
			TestReferenceText.ForgetCustomReferenceTexts();
			var persistenceImpl = (IProjectPersistenceWriter)ReferenceTextProxy.Reader;
			var customPoetianReferenceTextId = new ReferenceTextId(ReferenceTextType.Custom, kPoetian);
			persistenceImpl.SetUpProjectPersistence(customPoetianReferenceTextId);
			using (var writer = persistenceImpl.GetTextWriter(customPoetianReferenceTextId, ProjectResource.Metadata))
			{
				XmlSerializationHelper.Serialize(writer, metadata, out var e);
				Assert.That(e, Is.Null);
			}
			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, kPoetian));

			var books = (List<BookScript>)primaryReferenceText.Books;
			var refBook = new BookScript(testProject.Books[0].BookId, referenceBlocks, primaryReferenceText.Versification);
			books.Add(refBook);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks().Single();

			Assert.That(result.GetText(true), Is.EqualTo(
				"{1}\u00A0Juan dijo, 'This es estrofa 1, This es estrofa 2, This es estrofa 3, This es estrofa 4.'"));
			Assert.That(result.ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks.Single().GetText(true)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetBooksWithBlocksConnectedToReferenceText_EnglishVersification_NarratorOverridesAppliedIfRequested(bool applyNarratorOverrides)
		{
			// This test is based on the NarratorOverrides.xml control file having the following contents:
			//< Override startChapter = "68" endChapter = "70" character = "David" />
			//<Override startChapter="71" character="psalmist"/>
			//< Override startChapter = "72" character = "Solomon, king" />
			//< Override startChapter = "73" endChapter = "82" endVerse = "1" character = "Asaph" />
			//< Override startChapter = "82" startVerse = "2" endVerse = "7" character = "God" />
			//< Override startChapter = "82" startVerse = "8" endChapter = "83" character = "Asaph" />
			//< Override startChapter = "84" endChapter = "85" character = "sons of Korah" />

			// SETUP
			// =====
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.PSA_NoData);
			var psalms = testProject.Books[0];
			var vernacularBlocks = new List<Block>();

			var chapter = 71;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1 , "Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(24, "Езикът ми, тъй също, ще приказва за правдата Ти всеки ден, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Защото се посрамиха - защото се смутиха - ония, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "които искат зло за мене.", "q2");

			chapter = 72;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Псалом за Соломона", "d");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(20, "Свършиха се молитвите на Иесевия син Давида.", true, chapter, psalms.BookId, "q1"));
			chapter = 73;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Асафов псалом.", "d");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateBlockForVerse("Mike/Fred", 5, "Hi, this is Fred!", true, chapter, "q1"));
			vernacularBlocks.Last().CharacterIdOverrideForScript = "Fred";
			vernacularBlocks.Add(CreateNarratorBlockForVerse(28, "Но за мене е добре да се приближа при Бога; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Тебе, Господи Иеова, направих прибежището си, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "За да възгласявам всичките Твои дела.", "q1");
			chapter = 74;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Асафов псалом.", "d");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(23, "Не забравяй гласа на противниците Си; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Размирството на ония, които се повдигат против Тебе, постоянно се умножава.", "q2");
			chapter = 82;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Асафов псалом.", "d");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Бог стои в Божия събор, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Седи всред боговете.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateBlockForVerse("Mike/Fred", 5, "Hi, this is Mike!", true, chapter, "q1"));
			vernacularBlocks.Last().CharacterIdOverrideForScript = "Mike";
			vernacularBlocks.Add(CreateNarratorBlockForVerse(7, "А при все това вие ще умрете като човеци, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И ще паднете като един от князете.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(8, "Стани, Боже, съди земята; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Защото Ти имаш наследство всред всичките народи.", "q2");

			chapter = 84;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "За първия певец, на гетския инструмент, псалом на Кореевите потомци.", "d");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Колко са мили Твоите обиталища Господи на силите!", true, chapter, psalms.BookId, "q1"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Копнее и даже примира душата ми за дворовете Господни; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Сърцето ми и плътта ми викат към живия Бог.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(12, "Господи на Силите, Блажен оня човек, който уповава на Тебе.", true, chapter, psalms.BookId, "q1"));

			psalms.Blocks = vernacularBlocks;

			// SUT
			// ===
			var primaryReferenceText = GetStandardReferenceText(ReferenceTextType.English);
			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, applyNarratorOverrides).Single().GetScriptBlocks();

			// VERIFY BLOCKS WITH EXPLICITLY SET OVERRIDES
			// ===========================================
			var theBlockSpokenByMike = result.Single(b => b.CharacterIdInScript == "Mike");
			var theBlockSpokenByFred = result.Single(b => b.CharacterIdInScript == "Fred");
			Assert.That(theBlockSpokenByMike.CharacterId, Is.EqualTo(theBlockSpokenByFred.CharacterId));
			Assert.That(theBlockSpokenByMike.InitialStartVerseNumber, Is.EqualTo(theBlockSpokenByFred.InitialStartVerseNumber));

			// VERIFY OVERRIDES (OR NOT) FOR NARRATOR BLOCKS
			// =============================================
			var resultBlocksExcludingDirectSpeech = result.Where(b => b != theBlockSpokenByMike && b != theBlockSpokenByFred).ToList();

			Assert.That(resultBlocksExcludingDirectSpeech,
				CustomConstraints.ForEvery<Block>(
					b => b.IsChapterAnnouncement ^
					     (b.CharacterId == psalms.NarratorCharacterId || b.CharacterId == "Mike/Fred"),
					Is.True,
					"All blocks that are not direct speech should be chapter announcements, " +
					"narrator blocks or have the overridden CharacterId"
				));

			var narratorBlocks = resultBlocksExcludingDirectSpeech.Where(b => !b.IsChapterAnnouncement);

			// Setup is identical for both test cases - expected results are a bit different.
			if (applyNarratorOverrides)
			{
				foreach (var b in narratorBlocks)
				{
					if (b.InitialStartVerseNumber == 0)
					{
						Assert.That(b.CharacterIdInScript, Is.EqualTo(psalms.NarratorCharacterId));
						Assert.That(b.StyleTag, Is.EqualTo("d"));
						continue;
					}
					switch (b.ChapterNumber)
					{
						case 71:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("psalmist"));
							break;
						case 72:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("Solomon, king"));
							break;
						case 73:
						case 74:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("Asaph"));
							break;
						case 82:
							if (b.LastVerseNum < 2 || b.InitialStartVerseNumber > 7)
								Assert.That(b.CharacterIdInScript, Is.EqualTo("Asaph"));
							else
								Assert.That(b.CharacterIdInScript, Is.EqualTo("God"));
							break;
						case 84:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("sons of Korah"));
							break;
						default:
							Assert.Fail("Unexpected chapter number in result blocks");
							break;
					}
				}
			}
			else
			{
				Assert.That(narratorBlocks, ForEvery<Block>(b => b.CharacterIdInScript, Is.EqualTo(psalms.NarratorCharacterId)));
			}

			// VERIFY BLOCK DETAILS (CH 71)
			// ============================
			var i = 0;
			var block = resultBlocksExcludingDirectSpeech[i++];

			chapter = 71;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{24}\u00A0Езикът ми, тъй също, ще " +
				"приказва за правдата Ти всеки ден, Защото се посрамиха - защото се смутиха - " +
				"ония, които искат зло за мене."));

			// VERIFY BLOCK DETAILS (CH 72)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 72;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("d"));
			Assert.That(block.GetText(false), Is.EqualTo("Псалом за Соломона"));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{20}\u00A0Свършиха се молитвите на Иесевия син Давида."));

			// VERIFY BLOCK DETAILS (CH 73)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 73;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("d"));
			Assert.That(block.GetText(false), Is.EqualTo("Асафов псалом."));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{28}\u00A0Но за мене е добре да се " +
				"приближа при Бога; Тебе, Господи Иеова, направих прибежището си, За да " +
				"възгласявам всичките Твои дела."));

			// VERIFY BLOCK DETAILS (CH 74)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 74;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("d"));
			Assert.That(block.GetText(false), Is.EqualTo("Асафов псалом."));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{23}\u00A0Не забравяй гласа на " +
				"противниците Си; Размирството на ония, които се повдигат против Тебе, " +
				"постоянно се умножава."));

			// VERIFY BLOCK DETAILS (CH 82)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 82;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("d"));
			Assert.That(block.GetText(false), Is.EqualTo("Асафов псалом."));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Бог стои в Божия събор, Седи всред боговете."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{7}\u00A0А при все това вие ще умрете като човеци, И ще паднете като един от князете."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{8}\u00A0Стани, Боже, съди земята; " +
				"Защото Ти имаш наследство всред всичките народи."));

			// VERIFY BLOCK DETAILS (CH 84)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 84;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("d"));
			Assert.That(block.GetText(false), Is.EqualTo(
				"За първия певец, на гетския инструмент, псалом на Кореевите потомци."));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Колко са мили Твоите обиталища Господи на силите!"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{2}\u00A0Копнее и даже примира душата " +
				"ми за дворовете Господни; Сърцето ми и плътта ми викат към живия Бог."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{12}\u00A0Господи на Силите, " +
				"Блажен оня човек, който уповава на Тебе."));

			Assert.That(resultBlocksExcludingDirectSpeech.Count, Is.EqualTo(i),
				"Oops. we got more blocks than expected.");
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetBooksWithBlocksConnectedToReferenceText_RussianOrthoVersification_NarratorOverridesAppliedIfRequested(bool applyNarratorOverrides)
		{
			// This test is based on the NarratorOverrides.xml control file having the following contents:
			//<Override startChapter = "68" endChapter = "70" character = "David" />
			//<Override startChapter="71" character="psalmist"/>
			//<Override startChapter = "72" character = "Solomon, king" />
			//<Override startChapter = "73" endChapter = "82" endVerse = "1" character = "Asaph" />
			//<Override startChapter="81" startVerse="6" character="God"/>
			//<Override startChapter = "82" startVerse = "2" endVerse = "7" character = "God" />
			//<Override startChapter = "82" startVerse = "8" endChapter = "83" character = "Asaph" />
			//<Override startChapter = "84" endChapter = "85" character = "sons of Korah" />

			// SETUP
			// =====
			var testProject = TestProject.CreateTestProject(GlyssenEngineTests.Properties.Resources.RussianOrthodoxVersification, TestProject.TestBook.PSA_NoData);
			var psalms = testProject.Books[0];
			var vernacularBlocks = new List<Block>();

			// Also, all the chapter numbers in this range are one less than their english counterparts (which is what the overrides are based on)!
			var chapterNumAdjustment = -1;

			var chapter = 71 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(24, "Езикът ми, тъй също, ще приказва за правдата Ти всеки ден, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Защото се посрамиха - защото се смутиха - ония, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "които искат зло за мене.", "q2");

			chapter = 72 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Псалом за Соломона: Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(20, "Свършиха се молитвите на Иесевия син Давида.", true, chapter, psalms.BookId, "q1"));
			chapter = 73 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Асафов псалом. Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateBlockForVerse("Mike/Fred", 5, "Hi, this is Fred!", true, chapter, "q1"));
			vernacularBlocks.Last().CharacterIdOverrideForScript = "Fred";
			vernacularBlocks.Add(CreateNarratorBlockForVerse(28, "Но за мене е добре да се приближа при Бога; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Тебе, Господи Иеова, направих прибежището си, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "За да възгласявам всичките Твои дела.", "q1");
			chapter = 74 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Асафов псалом. Боже, дай твоето правосъдие на царя, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И правдата си на царския син,", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(23, "Не забравяй гласа на противниците Си; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Размирството на ония, които се повдигат против Тебе, постоянно се умножава.", "q2");
			chapter = 82 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Асафов псалом. Бог стои в Божия събор, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Седи всред боговете.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "За да съди Твоите люде с правда, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И угнетените Ти с правосъдие.", "q2");
			vernacularBlocks.Add(CreateBlockForVerse("Mike/Fred", 5, "Hi, this is Mike!", true, chapter, "q1"));
			vernacularBlocks.Last().CharacterIdOverrideForScript = "Mike";
			vernacularBlocks.Add(CreateNarratorBlockForVerse(7, "А при все това вие ще умрете като човеци, ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "И ще паднете като един от князете.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(8, "Стани, Боже, съди земята; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Защото Ти имаш наследство всред всичките народи.", "q2");

			// Note: In the following chapter, in English, the Hebrew subtitle is "verse 0" (i.e., not numbered), but in Russian Orthodox,
			// it is marked as verse 1 and any subsequent verse is bumped up by 1.
			chapter = 84 + chapterNumAdjustment;
			vernacularBlocks.Add(NewChapterBlock(psalms.BookId, chapter, $"Псалми {chapter}"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "За първия певец, на гетския инструмент, псалом на Кореевите потомци.", true, chapter, psalms.BookId));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Колко са мили Твоите обиталища Господи на силите!", true, chapter, psalms.BookId, "q1"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Копнее и даже примира душата ми за дворовете Господни; ", true, chapter, psalms.BookId, "q1"));
			AddBlockForVerseInProgress(vernacularBlocks, psalms.NarratorCharacterId, "Сърцето ми и плътта ми викат към живия Бог.", "q2");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(13, "Господи на Силите, Блажен оня човек, който уповава на Тебе.", true, chapter, psalms.BookId, "q1"));

			psalms.Blocks = vernacularBlocks;

			// SUT
			// ===
			var primaryReferenceText = GetStandardReferenceText(ReferenceTextType.English);
			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, applyNarratorOverrides).Single().GetScriptBlocks();

			// VERIFY BLOCKS WITH EXPLICITLY SET OVERRIDES
			// ===========================================
			var theBlockSpokenByMike = result.Single(b => b.CharacterIdInScript == "Mike");
			var theBlockSpokenByFred = result.Single(b => b.CharacterIdInScript == "Fred");
			Assert.That(theBlockSpokenByMike.CharacterId, Is.EqualTo(theBlockSpokenByFred.CharacterId));
			Assert.That(theBlockSpokenByMike.InitialStartVerseNumber, Is.EqualTo(theBlockSpokenByFred.InitialStartVerseNumber));

			// VERIFY OVERRIDES (OR NOT) FOR NARRATOR BLOCKS
			// =============================================
			var resultBlocksExcludingDirectSpeech = result.Where(b => b != theBlockSpokenByMike && b != theBlockSpokenByFred).ToList();

			Assert.That(resultBlocksExcludingDirectSpeech,
				CustomConstraints.ForEvery<Block>(
					b => b.IsChapterAnnouncement ^
					     (b.CharacterId == psalms.NarratorCharacterId || b.CharacterId == "Mike/Fred"),
					Is.True,
					"All blocks that are not direct speech should be chapter announcements, " +
					"narrator blocks or have the overridden CharacterId"
				));

			var narratorBlocks = resultBlocksExcludingDirectSpeech.Where(b => !b.IsChapterAnnouncement);

			// Setup is identical for both test cases - expected results are a bit different.
			if (applyNarratorOverrides)
			{
				foreach (var b in narratorBlocks)
				{
					switch (b.ChapterNumber)
					{
						case 70:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("psalmist"));
							break;
						case 71:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("Solomon, king"));
							break;
						case 72:
						case 73:
							Assert.That(b.CharacterIdInScript, Is.EqualTo("Asaph"));
							break;
						case 81:
							if (b.LastVerseNum < 2 || b.InitialStartVerseNumber > 7)
								Assert.That(b.CharacterIdInScript, Is.EqualTo("Asaph"));
							else
								Assert.That(b.CharacterIdInScript, Is.EqualTo("God"));
							break;
						case 83:
							if (b.InitialStartVerseNumber == 1)
								Assert.That(b.CharacterIdInScript, Is.EqualTo(psalms.NarratorCharacterId));
							else
								Assert.That(b.CharacterIdInScript, Is.EqualTo("sons of Korah"));
							break;
						default:
							Assert.Fail("Unexpected chapter number in result blocks");
							break;
					}
				}
			}
			else
			{
				Assert.That(narratorBlocks, ForEvery<Block>(b => b.CharacterIdInScript, Is.EqualTo(psalms.NarratorCharacterId)));
			}

			// VERIFY BLOCK DETAILS (CH 71)
			// ============================
			var i = 0;
			var block = resultBlocksExcludingDirectSpeech[i++];

			chapter = 71 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{1}\u00A0Боже, дай твоето " +
				"правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{2}\u00A0За да съди Твоите люде " +
				"с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{24}\u00A0Езикът ми, тъй също, ще " +
				"приказва за правдата Ти всеки ден, Защото се посрамиха - защото се смутиха - " +
				"ония, които искат зло за мене."));

			// VERIFY BLOCK DETAILS (CH 72)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 72 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{1}\u00A0Псалом за Соломона: Боже, " +
				"дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{2}\u00A0За да съди Твоите люде с " +
				"правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{20}\u00A0Свършиха се молитвите на Иесевия син Давида."));

			// VERIFY BLOCK DETAILS (CH 73)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 73 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Асафов псалом. Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{28}\u00A0Но за мене е добре да се приближа при Бога; Тебе, Господи Иеова, " +
				"направих прибежището си, За да възгласявам всичките Твои дела."));

			// VERIFY BLOCK DETAILS (CH 74)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 74 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Асафов псалом. Боже, дай твоето правосъдие на царя, И правдата си на царския син,"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{23}\u00A0Не забравяй гласа на противниците Си; Размирството на ония, които се повдигат против Тебе, постоянно се умножава."));

			// VERIFY BLOCK DETAILS (CH 82)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 82 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0Асафов псалом. Бог стои в Божия събор, Седи всред боговете."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0За да съди Твоите люде с правда, И угнетените Ти с правосъдие."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{7}\u00A0А при все това вие ще умрете като човеци, И ще паднете като един от князете."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{8}\u00A0Стани, Боже, съди земята; Защото Ти имаш наследство всред всичките народи."));

			// VERIFY BLOCK DETAILS (CH 84)
			// ============================
			block = resultBlocksExcludingDirectSpeech[i++];
			chapter = 84 + chapterNumAdjustment;
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.IsChapterAnnouncement);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("p"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{1}\u00A0За първия певец, на гетския инструмент, псалом на Кореевите потомци."));
			Assert.That(block.MatchesReferenceText);

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{2}\u00A0Колко са мили Твоите обиталища Господи на силите!"));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo("{3}\u00A0Копнее и даже примира душата " +
				"ми за дворовете Господни; Сърцето ми и плътта ми викат към живия Бог."));

			block = resultBlocksExcludingDirectSpeech[i++];
			Assert.That(block.ChapterNumber, Is.EqualTo(chapter));
			Assert.That(block.StyleTag, Is.EqualTo("q1"));
			Assert.That(block.GetText(true), Is.EqualTo(
				"{13}\u00A0Господи на Силите, Блажен оня човек, който уповава на Тебе."));

			Assert.That(resultBlocksExcludingDirectSpeech.Count, Is.EqualTo(i),
				"Oops. we got more blocks than expected.");
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_AlternateEndings_AllBlocksInBothEndingsAreAligned()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(8, "Hanən dyamdyamb.", true, 16, "MRK"),
				new Block("ms", 16, 8)
				{
					CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
					BlockElements = new List<BlockElement> { new ScriptText("Nyaa pɔ́g echě Kálag e Makɔsɛ ésógé") }
				},
				new Block("mr", 16, 8)
				{
					CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
					BlockElements = new List<BlockElement> { new ScriptText("16.9-20") }
				},
				new Block("s", 16, 8)
				{
					CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
					BlockElements = new List<BlockElement> { new ScriptText("Yesuɛ alúmté Maria Magdalɛnɛ yə̌l") }
				},
				CreateNarratorBlockForVerse(9, "Áde Yesuɛ bé mbéb saámbé áte. ", true, 16, "MRK")
					.AddVerse(10, "Hɛ́ɛ ane mmwaád ákíí boŋ álāŋgē bad ábe bɔ́ɔbɛ Yesuɛ bébágéʼáá. Antán nɛ́ɛ béchyɛʼɛ́ kwééd eche Yesuɛ. ")
					.AddVerse(11, "Boŋ áde béwógé bán Yesuɛ adé á aloŋgé, bán ane mmwaád anyíné-ʼɛ mɔ́, bénkêndúbɛ́ɛ́."),
				new Block("s", 16, 11)
				{
					CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
					BlockElements = new List<BlockElement> { new ScriptText("Yesuɛ alúmté ábɛ̄ bad bébɛ yə̌l") }
				},
				CreateNarratorBlockForVerse(12, "Ámbīd e póndé Yesuɛ anlúméd. Boŋ enɛ́n ngen anlúméd yə̌l nyaa émpēe. ", true, 16, "MRK")
					.AddVerse(13, "Hɛ́ɛ bad bétímé ámbīd, békɛ béláá baáb ábíníí."),
				CreateNarratorBlockForVerse(14, "Ámbīd e póndé Yesuɛ anlúméd yə̌l wɛ́ɛ ábē bembapɛɛ dyôm-ne-nhɔ́g áde bédyágkē ndyééd. ", true, 16, "MRK")
					.AddVerse(15, "Anláá bɔ́ aá, ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Nyékag nkǒŋsé áte ńsyə̄ə̄l, nyékanle bad bésyə̄ə̄l nkalaŋ ḿ bwâm. ")
				.AddVerse(16, "Kénzɛ́ɛ́ awě adúbpé boŋ bédusɛ́n-nɛ mɔ́ ǎkǔd eʼchoóchoŋ, boŋ kénzɛ́ɛ́ awě eedúbpe ǎkūd nkɔ́gsɛn. ")
				.AddVerse(17, "Bad ábe bédúbpé bɛ́wōŋ ngíne âbɛl mam mé menyáké: bɛ́bīdtē bad eʼdəə́dəŋ bé mbéb áte á dǐn ádêm; bɛ́hɔ̄bē-ʼɛ mehɔ́b mékɔ̄ɔ̄lē. ")
				.AddVerse(18, "Bɛ́kōbɛ̄n nyə̌ mekáá, éebɛnlé bɔ́ dyamdyam. Ké bémmwɛ̄-ʼɛ ké eʼwɛ, ábê eʼwɛ béebɛnlé bɔ́ dyamdyam. Bɛ́bān bad bé nkole mekáá á yə̌l, bad bé nkole bɛ̂ bédyɛ̄ɛ̄-ʼɛ bwâm.”");
			vernacularBlocks.Add(new Block("s", 16, 18)
			{
				CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
				BlockElements = new List<BlockElement> { new ScriptText("Yesuɛ apǔmmé") }
			});
			vernacularBlocks.Add(CreateNarratorBlockForVerse(19, "Áde Sáŋgú Yesuɛ ámáá bɔ́ ḿmɛ́n mekan aláa, dɔ́ɔ Dyǒb ábɛ́lé boŋ ápūmē ámīn. ", true, 16, "MRK")
				.AddVerse(20, "Hɛ́ɛ bembapɛɛ bébídé, boŋ békɛ békalé Eyale é Dyǒb hǒm tɛ́ɛ́."));
			vernacularBlocks.Add(new Block("ms", 16, 20)
			{
				CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
				BlockElements = new List<BlockElement> { new ScriptText("Nyaa empée echě kálag e Makɔsɛ ésógé") }
			});
			vernacularBlocks.Add(new Block("mr", 16, 20)
			{
				CharacterId = GetStandardCharacterId("MRK", StandardCharacter.ExtraBiblical),
				BlockElements = new List<BlockElement> { new ScriptText("16.9-10") }
			});
			var altEndingBlock = CreateNarratorBlockForVerse(9, "Hɛ́ɛ ábê bebaád békíí, boŋ mésyə̄ə̄l ḿme bénlyə̄gtɛ̄nnē bɔ́, bésɛ̄lē mɔ́ esóŋ-esóŋ. ", true, 16, "MRK")
				.AddVerse(10, "Nɛ́ɛ nɛ̂ átómé, Yesuɛ mwěn anlóm bad nkǒŋsé áte ńsyə̄ə̄l âkal nkalaŋ ḿ bwâm.]");
			altEndingBlock.BlockElements.Insert(0, new ScriptText("["));
			vernacularBlocks.Add(altEndingBlock);

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			testProject.Books[0].Blocks = vernacularBlocks;

			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

			Assert.That(result.Where(b => b.IsScripture), ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_ExtraBlockAtEndOfBook_ExtraBlocksNotAlignedToReferenceText()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse("Jesus", 19,
					"“Tell everybody everywhere about me and get them to follow me,” ", true, 28, "p", 21)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "said Jesus.");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			testProject.Books[0].Blocks = vernacularBlocks;

			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

			Assert.That(result[0].ReferenceBlocks.Count, Is.EqualTo(3));
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[1].ReferenceBlocks, Is.Empty);
		}

		/// <summary>
		/// PG-1277: Test to ensure that no part of the reference text needed to align to the vernacular goes missing when a verse is missing
		/// in the vernacular (because of a manuscript variant) and in the reference text that verse is combined with (part of) the following
		/// verse. In the case of LUK 23:17, when we try to skip over the reference block for v. 17, we don't want to 'inadvertently also skip
		/// over the first part of v. 18, which FCBH has included in the same block.
		/// </summary>
		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_MissingVerseInVernIsInRefBlockAlongWithStartOfFollowingVerse_VernVerseFollowingHoleAlignsToPartOfRefBlockWithVerse()
		{
			const string kBookId = "LUK";
			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));

			Action cleanup = null;
			try
			{
				var v17And18Block = GetLuke23v17And18Block(primaryReferenceText, out var lastBlockForV18, out cleanup);
				var expectedRefTextForVerse18Part1 = "{18}\u00A0" + ((ScriptText)v17And18Block.BlockElements[3]).Content;
				var expectedRefTextForVerse18Part2 = lastBlockForV18.GetText(true);

				var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(14, "Pilate said, ", true, 23, kBookId) };
				AddBlockForVerseInProgress(vernacularBlocks, "Pilate", "“You say this man was inciting rebellion, but I have found no basis for your charges. ")
					.AddVerse(15, "Herod came up empty, too, so he sent him back. How could we kill him? ")
					.AddVerse(16, "I'll just rough him up a bit and let him go.”");
				vernacularBlocks.Add(CreateNarratorBlockForVerse(18, "But the whole crowd is screaming like, ", true, 23, kBookId));
				AddBlockForVerseInProgress(vernacularBlocks, "crowd", "“Eliminate this man! Give us Barabbas!” ");

				var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
				testProject.Books[0].Blocks = vernacularBlocks;

				var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

				Assert.That(result.Count, Is.EqualTo(6));
				Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
				Assert.That(result[4].ReferenceBlocks.Single().GetText(true), Is.EqualTo(expectedRefTextForVerse18Part1));
				Assert.That(result[5].ReferenceBlocks.Single().GetText(true), Is.EqualTo(expectedRefTextForVerse18Part2));
			}
			finally
			{
				cleanup?.Invoke();
			}
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_MissingVerseInVernIsInRefBlockAlongWithStartOfFollowingVerse_WholeVerseVernBlockFollowingHoleAlignsToPartOfRefBlockWithVerse()
		{
			const string kBookId = "LUK";

			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(14, "Pilate said, ", true, 23, kBookId) };
			AddBlockForVerseInProgress(vernacularBlocks, "Pilate", "“You say this man was inciting rebellion, but I have found no basis for your charges. ")
				.AddVerse(15, "Herod came up empty, too, so he sent him back. How could we kill him? ")
				.AddVerse(16, "I'll just rough him up a bit and let him go.”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(18, "But the whole crowd screamed at Pilate to kill Jesus and release Barabbas.", true, 23, kBookId));

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
			testProject.Books[0].Blocks = vernacularBlocks;

			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));
			Action cleanup = null;
			try
			{
				var v17And18Block = GetLuke23v17And18Block(primaryReferenceText, out var lastBlockForV18, out cleanup);
				var expectedRefTextForVerse18Part1 = "{18}\u00A0" + ((ScriptText)v17And18Block.BlockElements[3]).Content;
				var expectedRefTextForVerse18Part2 = lastBlockForV18.GetText(true);

				var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject, false).Single().GetScriptBlocks();

				Assert.That(result.Count, Is.EqualTo(5));
				Assert.That(result.Take(3), ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
				Assert.That(result[4].MatchesReferenceText, Is.False);
				Assert.That(result[4].ReferenceBlocks.Count, Is.EqualTo(2));
				Assert.That(result[4].ReferenceBlocks[0].GetText(true),
					Is.EqualTo(expectedRefTextForVerse18Part1));
				Assert.That(result[4].ReferenceBlocks[1].GetText(true),
					Is.EqualTo(expectedRefTextForVerse18Part2));
			}
			finally
			{
				cleanup?.Invoke();
			}
		}

		[Test]
		public void ApplyTo_MissingVerseInSingleVoiceVernIsInRefBlockAlongWithStartOfFollowingVerse_WholeVerseVernBlockFollowingHoleAlignsToPartOfRefBlockWithVerse()
		{
			const string kBookId = "LUK";
			var primaryReferenceText = GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.English));
			Action cleanup = null;
			try
			{
				var v17And18Block = GetLuke23v17And18Block(primaryReferenceText, out var lastBlockForV18, out cleanup);
				var expectedRefTextForVerse18Part1 = "{18}\u00A0" + ((ScriptText)v17And18Block.BlockElements[3]).Content;
				var expectedRefTextForVerse18Part2 = lastBlockForV18.GetText(true);

				var vernacularBlocks = new List<Block>
				{
					CreateNarratorBlockForVerse(14, "Pilate said, “You say this man was inciting rebellion, but I have found no basis for your charges. ", true, 23, kBookId)
						.AddVerse(15, "Herod came up empty, too, so he sent him back. How could we kill him? ")
						.AddVerse(16, "I'll just rough him up a bit and let him go.”")
						.AddVerse(18, "But the whole crowd screamed at Pilate: “Eliminate this man! Give us Barabbas!”")
				};

				var testProject = TestProject.CreateTestProject(TestProject.TestBook.LUK);
				testProject.Books[0].Blocks = vernacularBlocks;
				testProject.Books[0].SingleVoice = true;

				primaryReferenceText.ApplyTo(testProject.Books[0]);
				var result = testProject.Books[0].Blocks;

				Assert.That(result.Count, Is.EqualTo(4));
				Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
				Assert.That(result[3].ReferenceBlocks.Single().GetText(true),
					Is.EqualTo(expectedRefTextForVerse18Part1 + " " + expectedRefTextForVerse18Part2));
			}
			finally
			{
				cleanup?.Invoke();
			}
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_BadIndex_ThrowsArgumentOutOfRangeException()
		{
			var vernBook = new BookScript("MAT", new List<Block>(0), m_vernVersification);
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, new List<Block>(0));

			Assert.Throws<ArgumentOutOfRangeException>(() => refText.GetBlocksForVerseMatchedToReferenceText(vernBook, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_NoCorrespondingBookInReferenceText_ReturnsNull()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true) };
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);
			var refText = TestReferenceText.CreateTestReferenceText("LUK", new List<Block>(0));

			Assert.That(refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0), Is.Null);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlocksAreForSingleVerse_ReturnedBlocksAreMatchedClonesOfOriginals(int iBlock)
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo uno, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo. Pero Paul replico: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "Asi pense, ");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Then Jesus said, ", true) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“This is verse one.” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But Paul replied, ");
			AddBlockForVerseInProgress(referenceBlocks, "Paul", "“That's what I thought.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "Extra stuff that is not used. ", true).AddVerse(3));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Select(b => b.ToString())), Is.True);
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void GetBlocksForVerseMatchedToReferenceText_VernAndRefHaveMultipleSingleBlockVerses_ReturnsSingleClonedAndMatchedBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				CreateBlockForVerse("Jesus", 2, "Nunca os dejare! ", true),
				CreateNarratorBlockForVerse(3, "Entonces fue con sus discipulos al jardin, donde Pedro dijo: ", true),
				CreateBlockForVerse("Peter", 4, "Me gusta este lugar.", true)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true),
				CreateNarratorBlockForVerse(3, "Then he went with his disciples to the garden, whereupon Peter said: ", true),
				CreateBlockForVerse("Peter", 4, "“I like this place.”", true)
			};
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Single().ToString(), Is.EqualTo(vernacularBlocks[iBlock].ToString()));
			Assert.That(result.Intersect(vernacularBlocks).Count(), Is.EqualTo(0));
			Assert.That(result.Single().MatchesReferenceText);
			Assert.That(result.Single().GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[iBlock].GetText(true)));
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocks_ReturnsMultipleClonedAndMatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				CreateBlockForVerse("Jesus", 2, "Nunca os dejare! ", true)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "En ese momento, Pedro entro exclamando: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nos quieren hundir con los impuestos exagerados! ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Claro que estaba exagerando.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, Peter burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "Peter", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())), Is.True);
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(referenceBlocks.Skip(1).Take(4).Select(r => r.GetText(true)).SequenceEqual(result.Select(v => v.GetPrimaryReferenceText())), Is.True);
		}

		/// <summary>
		/// PG-1311: Block has multiple verses. Last verse bleeds into subsequent block, but a verse is missing so the first verse number
		/// present in the subsequent block is > the initial verse number + 1.
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_MultipleVersesInVernBlockNextBlockStartsWith_ReturnsMatchupCoveringInitialBlockOnly()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse("Jesus", 31, "«Dœ olonœ asœmœ, uzu á nœ ye sœ ɓa sœnda, tshe ye ga mangba ye nene. ", false, 17)
					.AddVerse(32, "'E gbetshelœ 'e tœ upu nœ awo Lota kane. ")
					.AddVerse(33, "Uzu neke á tshe para awa ndœ kœgbɔndœ soro tshu; kashe tsheneke nœ nene dá she. ")
					.AddVerse(34, "Mœ sœ 'e, lœ butshɔnœ asœmœ, ayakoshe: endje za anga bale, yé anga œ sœpe. ")
					.AddVerse(35, "Ayashe bisha œ sœ kœtɔ œrœ tœ œsœnœ bale: endje za anga bale, yé anga œ sœpe. "),
				CreateBlockForVerse(kUnexpectedCharacter, 37, "[ ", false, 17)
					.AddVerse(37, "Ayambarœ nœ Yisu yu she adœke:")
			};
			AddBlockForVerseInProgress(vernacularBlocks, kUnexpectedCharacter, " «Œrœnœ atamœ œ mbœrœtœ endje kpœta Gbozu?» ", "p");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "é tshe kœgi fœ endje adœke: ", "LUK");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Osho á oko sœ tœnœ, œ kœngbɔtœ endje ɓa zœ.»", "p");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			Assert.That(matchup.OriginalBlockCount, Is.EqualTo(1));
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocksWithCharactersAssignedThatDoNotMatchReference_ReturnsMultipleClonedAndMismatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true),
				CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Pedro.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, John burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())), Is.True);
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result.Take(3), ForEvery<Block>(b => b.MatchesReferenceText, Is.False));
			Assert.That(referenceBlocks.Skip(1).Take(3).Select(r => r.GetText(true)).SequenceEqual(result[0].ReferenceBlocks.Select(b => b.GetText(true))), Is.True);
			Assert.That(result[3].MatchesReferenceText);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocksWithoutCharactersAssignedThatCouldMatchReference_ReturnsMultipleClonedAndMatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true),
				CreateBlockForVerse(kAmbiguousCharacter, 2, "Nunca os dejare! ", true)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "En ese momento, Pedro entro exclamando: ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "Nos quieren hundir con los impuestos exagerados! ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Claro que estaba exagerando.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, Peter burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "Peter", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())), Is.True);
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(referenceBlocks.Skip(1).Take(4).Select(r => r.GetText(true)).SequenceEqual(result.Select(v => v.GetPrimaryReferenceText())), Is.True);
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetBlocksForVerseMatchedToReferenceText_VernVerseStartsMidBlock_ReturnedBlocksAreMatchedClonesOfOriginals(int iBlock)
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Partieron de alli para Jerico. ", true).AddVerse(2, "Entonces dijo Jesus: ") };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Another verse."));
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "They journeyed on from there to Jericho.", true),
				CreateNarratorBlockForVerse(2, "Then Jesus said,")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Come to me you who are weary.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "Extra stuff that is not used. ", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(matchup.CorrelatedAnchorBlock.BlockElements.OfType<ScriptText>().First().Content,
				Is.EqualTo(vernacularBlocks[iBlock].BlockElements.OfType<ScriptText>().First().Content));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(result[0].GetText(true), Is.EqualTo("{1}\u00A0Partieron de alli para Jerico. "));
			Assert.That(result[1].GetText(true), Is.EqualTo("{2}\u00A0Entonces dijo Jesus: "));
			Assert.That(result[2].GetText(true), Is.EqualTo("Come to me you who are weary."));

			Assert.That(result.Select(b => b.GetPrimaryReferenceText()).SequenceEqual(referenceBlocks.Take(3).Select(b => b.GetText(true))), Is.True);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_EnglishRefVerseStartsMidBlock_MatchupIncludesPrecedingVerse()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Maria i karim Jisas long taun Betlehem long distrik Judia long taim Herot i stap king. Em i karim Jisas pinis, na bihain sampela saveman bilong hap sankamap i kam long Jerusalem na ol i askim nabaut olsem, ", true, 2),
				CreateBlockForVerse("magi", 2, "\"Nupela pikinini em king bilong ol Juda, em i stap we ? Mipela i lukim sta bilong en long hap sankamap, na mipela i kam bilong lotu long em.\"", false, 2),
				CreateNarratorBlockForVerse(3, "King Herot i... ", true, 2)
			};
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(1, "Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem,", true, 2).AddVerse(2, "saying, ") };
			AddBlockForVerseInProgress(referenceBlocks, "magi", " “Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "When King Herod... ", true, 2));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedAnchorBlock.GetText(true),
				Is.EqualTo(vernacularBlocks[1].GetText(true)));
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(vernacularBlocks.Intersect(result).Count(), Is.EqualTo(0));
			Assert.That(result[0].MatchesReferenceText, Is.False);
			Assert.That(result[1].MatchesReferenceText);

			Assert.That(result[0].GetText(true), Is.EqualTo(
				"{1}\u00A0Maria i karim Jisas long taun Betlehem long distrik Judia long taim " +
				"Herot i stap king. Em i karim Jisas pinis, na bihain sampela saveman bilong " +
				"hap sankamap i kam long Jerusalem na ol i askim nabaut olsem, "));
			Assert.That(result[1].GetText(true), Is.EqualTo(
				"{2}\u00A0\"Nupela pikinini em king bilong ol Juda, em i stap we ? Mipela i " +
				"lukim sta bilong en long hap sankamap, na mipela i kam bilong lotu long em.\""));

			Assert.That(result.Select(b => b.ReferenceBlocks.Single().GetText(true)).SequenceEqual(referenceBlocks.Take(2).Select(b => b.GetText(true))), Is.True);
			Assert.That(result[1].GetPrimaryReferenceText(), Is.EqualTo(referenceBlocks[1].GetText(true)));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_Prematched_SavedMatchesAreNotOverwritten(int iBlock)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true),
				CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Pedro.");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Then Jesus said, ", true),
				CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true)
			};
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, John burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			// Now pre-match them:
			vernacularBlocks[0].SetMatchedReferenceBlock(CreateNarratorBlockForVerse(1, "This is what happened.", true));
			vernacularBlocks[1].SetMatchedReferenceBlock(CreateNarratorBlockForVerse(2, "Then Jesus said:", true));
			vernacularBlocks[2].SetMatchedReferenceBlock(referenceBlocks[1].Clone());
			vernacularBlocks[2].ReferenceBlocks.Single().BlockElements.RemoveAt(0); // Get rid of verse number
			vernacularBlocks[3].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = "Peter",
				BlockElements = new List<BlockElement> { new ScriptText("“Thanks,”") }
			});
			vernacularBlocks[4].SetMatchedReferenceBlock(new Block("p", 1, 2)
			{
				CharacterId = vernBook.NarratorCharacterId,
				BlockElements = new List<BlockElement> { new ScriptText("answered Peter.") }
			});

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock);
			Assert.That(vernacularBlocks[iBlock].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.That(matchup.HasOutstandingChangesToApply, Is.False);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_EndOfBookWhereReferenceTextCombinesTwoVerses_MatchesToRelevantPartOfRefBlock()
		{
			// The last block of the standard reference text for Hebrews combines verse 24 and 25.
			const string kBookId = "HEB";
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var v25RefTextBlock = refText.Books.Single(b => b.BookId == kBookId).GetBlocksForVerse(13, 25).Single();
			Assert.That(v25RefTextBlock.InitialStartVerseNumber, Is.EqualTo(24),
				"Test setup conditions not met");
			Assert.That(((Verse)v25RefTextBlock.BlockElements[2]).Number, Is.EqualTo("25"),
				"Test setup conditions not met");
			var expectedRefTextForVerse25 = "{25}\u00A0" + ((ScriptText)v25RefTextBlock.BlockElements[3]).Content;

			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(25, "Grace be unto you all.", true, 13, kBookId) };
			var vernBook = new BookScript(kBookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			Assert.That(vernacularBlocks[0].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(1));
			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.That(matchup.CorrelatedBlocks[0].GetPrimaryReferenceText(),
				Is.EqualTo(expectedRefTextForVerse25));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_AllowSplitting_VernBlockIsFirstVerseOfCombinedReferenceTextBlock_MatchupIncludesBothVerses()
		{
			var vernacularBlocks = new List<Block> {
				CreateNarratorBlockForVerse(24, "Köszöntsétek minden elõljárótokat és a szenteket mind. Köszöntenek titeket az Olaszországból valók. ", true, 13, "HEB"),
				CreateNarratorBlockForVerse(25, "Kegyelem mindnyájatokkal! Ámen!", true, 13, "HEB")
			};
			var vernBook = new BookScript("HEB", vernacularBlocks, m_vernVersification);

			// The last block of the standard reference text for Hebrews combines verse 24 and 25.
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0); // allowing splitting is the default
			Assert.That(vernacularBlocks[0].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.False);
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().InitialStartVerseNumber, Is.EqualTo(24));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().LastVerseNum, Is.EqualTo(24));
			Assert.That(matchup.CorrelatedBlocks[1].MatchesReferenceText);
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().InitialStartVerseNumber, Is.EqualTo(25));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().LastVerseNum, Is.EqualTo(25));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_DisallowSplitting_VernBlockIsFirstVerseOfCombinedReferenceTextBlock_MatchupIncludesBothVerses()
		{
			var vernacularBlocks = new List<Block> {
				CreateNarratorBlockForVerse(24, "Köszöntsétek minden elõljárótokat és a szenteket mind. Köszöntenek titeket az Olaszországból valók. ", true, 13, "HEB"),
				CreateNarratorBlockForVerse(25, "Kegyelem mindnyájatokkal! Ámen!", true, 13, "HEB")
			};
			var vernBook = new BookScript("HEB", vernacularBlocks, m_vernVersification);

			// The last block of the standard reference text for Hebrews combines verse 24 and 25.
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, allowSplitting: false);
			Assert.That(vernacularBlocks[0].GetText(true), Is.EqualTo(matchup.CorrelatedAnchorBlock.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.False);
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(matchup.CorrelatedBlocks[1].MatchesReferenceText, Is.False);
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Count, Is.EqualTo(1));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetText(true)),
				"Since we're not allowing the reference block to be split, we should match the block with v. 24 and v. 25 to both vern blocks.");
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasExtraChapterBeyondLastChapterOfRefText_VernVersesInExtraChapterRemainsUnmatched()
		{
			var vernacularBlocks = new List<Block> {
				CreateNarratorBlockForVerse(24, "Köszöntsétek minden elõljárótokat és a szenteket mind. Köszöntenek titeket az Olaszországból valók. ", true, 13, "HEB")
					.AddVerse(25, "Kegyelem mindnyájatokkal! Ámen!"),
				NewChapterBlock("HEB", 14),
				CreateNarratorBlockForVerse(1, "Alright, who's the wise guy?", true, 14, "HEB"),
				CreateNarratorBlockForVerse(2, "This is not supposed to be here", true, 14, "HEB")
			};
			var vernBook = new BookScript("HEB", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 2);
			Assert.That(matchup.CorrelatedBlocks.Last().MatchesReferenceText, Is.False);
			matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 3);
			Assert.That(matchup.CorrelatedBlocks.Last().MatchesReferenceText, Is.False);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasLeadingSquareBracket_BlockIsNotSplitBetweenBracketAndVerseNumber()
		{
			var vernacularBlocks = new List<Block> {
				CreateNarratorBlockForVerse(8, "Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo. ", true, 16, "MRK"),
				CreateNarratorBlockForVerse(9, "Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", true, 16, "MRK")
					.AddVerse(10, "En otugi tero lok bot jo ma yam gibedo kwede, i kare ma gitye ki cola ki kumu-gu. ").AddVerse(11, "Ka guwinyo ni en tye gire makwo, dok ni otyeko nyutte, kome onen bot Maliam, pe guye lokke. "),
			};
			vernacularBlocks.Last().BlockElements.Insert(0, new ScriptText("["));
			var vernBook = new BookScript("MRK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(3));
			var firstCorrelatedBlock = matchup.CorrelatedBlocks.First();
			Assert.That(firstCorrelatedBlock.BlockElements.Count, Is.EqualTo(3));
			Assert.That(((ScriptText)firstCorrelatedBlock.BlockElements[0]).Content, Is.EqualTo("["));
			Assert.That(((Verse)firstCorrelatedBlock.BlockElements[1]).Number, Is.EqualTo("9"));
			Assert.That(((ScriptText)firstCorrelatedBlock.BlockElements[2]).Content, Does.StartWith("Ka en doŋ"));
		}

		#region PG-794 (misalignment between vern and ref)
		/// <summary>
		/// This is the text for the specific scenario detailed in PG-794.
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasPreAnnouncedQuoteAsSingleBlockThatIsSplitInRefText_AllTextFromRefTextIsIncluded()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextRev = refText.GetBook("REV");
			var refBlockRev14V12 = refTextRev.GetBlocksForVerse(14, 12).Single();
			var refBlocksRev14V13 = refTextRev.GetBlocksForVerse(14, 13).ToList();
			VerifyCharacterVerseEntriesForRev4V12To13(refBlockRev14V12, refBlocksRev14V13);

			var vernacularBlocks = new List<Block>
			{
				new Block("c", 14)
				{
					BookCode = refTextRev.BookId,
					CharacterId = GetStandardCharacterId(refTextRev.BookId, StandardCharacter.BookOrChapter),
				},
				CreateNarratorBlockForVerse(12, "'Ŋwacɩa ya ŋeni, ɩya Laagɔ 'la ncɛlɩa ‑ɔ 'plɩlɩ 'lɛ ɔla ‑jlɩmaa 'kʋ 'nyɩ ‑ɔ ‑ka 'lɛ Zozii 'la pɔɔtɛtɛ na, 'kanɩ pɔlɛ kla. ", true, 14, refTextRev.BookId)
					.AddVerse(13, "'Nyɩ n ‑ya ‑blɩɩzɔn bhlo 'nu, ʋ 'wlʋlʋa ‑laagɔɔn 'nyɩ ʋ claa lebhe: ")
			};

			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Zie lebhe: Ma ‑kʋnɩ ‑pɔtɔnʋ 'nanɩ, maa ‑ɔ 'kulu 'lɛ ‑Kwlenyɔ nɩkplaan na!» "); // Write: Blessed...
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "‑Ghɛɛ, 'Wugoa nabhe: ", refTextRev.BookId); // The Holy Spirit said:
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«'Naa ma 'lɩnɩ lobholia ‑tɛtɛa ma nʋ 'lɛ na 'la nyɔkwɛa, sa mala nʋnʋgbɩa gɩlɩ maa na.»"); // Yes, that they...
			var vernBook = new BookScript(refTextRev.BookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(5));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refBlockRev14V12.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[0].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[1].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[2].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[1].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[2].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[3].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[3].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[3].GetText(true)}");
			Assert.That(string.Concat(matchup.CorrelatedBlocks[4].ReferenceBlocks.Select(r => r.GetText(true))),
				Is.EqualTo(refBlocksRev14V13[2].GetText(true) + refBlocksRev14V13[4].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[4].GetText(true)}");
		}

		/// <summary>
		/// This scenario has the two lines by the Holy Spirit combined, but with the "he said" at the end.
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasPostAnnouncedQuoteAsSingleBlockThatIsSplitInRefText_AllTextFromRefTextMatchesBlockForCorrectSpeaker()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextRev = refText.GetBook("REV");
			var refBlockRev14V12 = refTextRev.GetBlocksForVerse(14, 12).Single();
			var refBlocksRev14V13 = refTextRev.GetBlocksForVerse(14, 13).ToList();
			VerifyCharacterVerseEntriesForRev4V12To13(refBlockRev14V12, refBlocksRev14V13);

			var vernacularBlocks = new List<Block>
			{
				new Block("c", 14)
				{
					BookCode = refTextRev.BookId,
					CharacterId = GetStandardCharacterId(refTextRev.BookId, StandardCharacter.BookOrChapter),
				},
				CreateNarratorBlockForVerse(12, "'Ŋwacɩa ya ŋeni, ɩya Laagɔ 'la ncɛlɩa ‑ɔ 'plɩlɩ 'lɛ ɔla ‑jlɩmaa 'kʋ 'nyɩ ‑ɔ ‑ka 'lɛ Zozii 'la pɔɔtɛtɛ na, 'kanɩ pɔlɛ kla. ", true, 14, refTextRev.BookId)
					.AddVerse(13, "Then I heard a heavenly voice articulating: ")
			};

			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Escriba: Blessed are the muertos from here on out!» ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Yes, that they can get some sleep; their labores les siguen.»");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "said the Holy Spirit.", refTextRev.BookId);
			var vernBook = new BookScript(refTextRev.BookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(5));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refBlockRev14V12.GetText(true)));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[0].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[1].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[2].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[1].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[2].GetText(true)}");
			Assert.That(string.Concat(matchup.CorrelatedBlocks[3].ReferenceBlocks.Select(r => r.GetText(true))),
				Is.EqualTo(refBlocksRev14V13[2].GetText(true) + refBlocksRev14V13[4].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[3].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[4].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksRev14V13[3].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[4].GetText(true)}");
		}

		/// <summary>
		/// This scenario has the two lines by the Holy Spirit combined, but with both a preceeding announcement and a closing "he said".
		/// We could possibly hope for better results, but it's pretty tricky to figure out what's best in this case and probably harder
		/// to write intelligible code to do it. So we'll be content if everything in the reference text is just preserved in the corretc order.
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasPreAndPostAnnouncedQuoteAsSingleBlockThatIsSplitInRefText_AllTextFromRefTextIncluded()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextRev = refText.GetBook("REV");
			var refBlockRev14V12 = refTextRev.GetBlocksForVerse(14, 12).Single();
			var refBlocksRev14V13 = refTextRev.GetBlocksForVerse(14, 13).ToList();
			VerifyCharacterVerseEntriesForRev4V12To13(refBlockRev14V12, refBlocksRev14V13);

			var vernacularBlocks = new List<Block>
			{
				new Block("c", 14)
				{
					BookCode = refTextRev.BookId,
					CharacterId = GetStandardCharacterId(refTextRev.BookId, StandardCharacter.BookOrChapter),
				},
				CreateNarratorBlockForVerse(12, "'Ŋwacɩa ya ŋeni, ɩya Laagɔ 'la ncɛlɩa ‑ɔ 'plɩlɩ 'lɛ ɔla ‑jlɩmaa 'kʋ 'nyɩ ‑ɔ ‑ka 'lɛ Zozii 'la pɔɔtɛtɛ na, 'kanɩ pɔlɛ kla. ", true, 14, refTextRev.BookId)
					.AddVerse(13, "Then I heard a heavenly voice articulating: ")
			};

			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Escriba: Blessed are the muertos from here on out!» ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "The Holy Spirit responded: ", refTextRev.BookId);
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Yes, that they can get some sleep; their labores les siguen,» ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "end quote.", refTextRev.BookId);
			var vernBook = new BookScript(refTextRev.BookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(6));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refBlockRev14V12.GetText(true)));
			Assert.That(refBlocksRev14V13.Select(r => r.GetText(true)).SequenceEqual(matchup.CorrelatedBlocks.Skip(1).SelectMany(c =>
				c.ReferenceBlocks).Select(rb => rb.GetText(true))), Is.True);
		}

		/// <summary>
		/// This scenario has the two lines by the Holy Spirit split as in the reference text, with an extra closing "he said".
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasPostAnnouncedQuote_AllTextFromRefTextMatchesWithExtraVernLineUnmatched()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextRev = refText.GetBook("REV");
			var refBlockRev14V12 = refTextRev.GetBlocksForVerse(14, 12).Single();
			var refBlocksRev14V13 = refTextRev.GetBlocksForVerse(14, 13).ToList();
			VerifyCharacterVerseEntriesForRev4V12To13(refBlockRev14V12, refBlocksRev14V13);

			var vernacularBlocks = new List<Block>
			{
				new Block("c", 14)
				{
					BookCode = refTextRev.BookId,
					CharacterId = GetStandardCharacterId(refTextRev.BookId, StandardCharacter.BookOrChapter),
				},
				CreateNarratorBlockForVerse(12, "'Ŋwacɩa ya ŋeni, ɩya Laagɔ 'la ncɛlɩa ‑ɔ 'plɩlɩ 'lɛ ɔla ‑jlɩmaa 'kʋ 'nyɩ ‑ɔ ‑ka 'lɛ Zozii 'la pɔɔtɛtɛ na, 'kanɩ pɔlɛ kla. ", true, 14, refTextRev.BookId)
					.AddVerse(13, "Then I heard a heavenly voice articulating: ")
			};

			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Escriba: Blessed are the muertos from here on out!» ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Yes,» ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "the Holy Spirit responded, ", refTextRev.BookId);
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«that they can get some sleep; their labores les siguen,» ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "end quote.", refTextRev.BookId);
			var vernBook = new BookScript(refTextRev.BookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(7));
			Assert.That(matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refBlockRev14V12.GetText(true)));
			for (int i = 1; i < matchup.CorrelatedBlocks.Count - 1; i++)
			{
				Assert.That(matchup.CorrelatedBlocks[i].ReferenceBlocks.Single().GetText(true),
					Is.EqualTo(refBlocksRev14V13[i - 1].GetText(true)),
					$"Vern: {matchup.CorrelatedBlocks[i].GetText(true)}");
			}
			Assert.That(matchup.CorrelatedBlocks[6].MatchesReferenceText, Is.False,
				$"Vern: {matchup.CorrelatedBlocks[6].GetText(true)}");
		}

		private static void VerifyCharacterVerseEntriesForRev4V12To13(Block refBlockRev14V12, List<Block> refBlocksRev14V13)
		{
			var narrator = GetStandardCharacterId("REV", StandardCharacter.Narrator);
			Assert.That(refBlockRev14V12.CharacterId,
				Is.EqualTo("angel flying directly overhead, third"),
				"Expected pre-condition for test not met. Reference text for REV 14:12 should be a single blockspoken by \"angel flying directly overhead, third\".");
			Assert.That(refBlocksRev14V13.Count, Is.EqualTo(5),
				"Expected pre-condition for test not met. Reference text for REV 14:13 should be 5 blocks.");
			Assert.That(refBlocksRev14V13[0].CharacterId,
				Is.EqualTo(narrator), 
				$"Expected pre-condition for test not met. Reference text block 0 for REV 14:13 should be spoken by \"{narrator}\".");
			Assert.That(refBlocksRev14V13[1].CharacterId, Is.EqualTo("voice from heaven (God?)"),
				"Expected pre-condition for test not met. Reference text block 1 for REV 14:13 should be spoken by \"voice from heaven (God?)\".");
			Assert.That(refBlocksRev14V13[2].CharacterId, Is.EqualTo("Holy Spirit, the"),
				"Expected pre-condition for test not met. Reference text block 2 for REV 14:13 should be spoken by \"Holy Spirit, the\".");
			Assert.That(refBlocksRev14V13[3].CharacterId, Is.EqualTo(narrator),
				$"Expected pre-condition for test not met. Reference text block 3 for REV 14:13 should be spoken by \"{narrator}\".");
			Assert.That(refBlocksRev14V13[4].CharacterId,
				Is.EqualTo("Holy Spirit, the"),
				"Expected pre-condition for test not met. Reference text block 4 for REV 14:13 should be spoken by \"Holy Spirit, the\".");
		}
		#endregion

		#region PG-1133 (More misalignment between vern and ref)
		/// <summary>
		/// This is the text for the MRK 14:70 scenario detailed in PG-1133.
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockHasQuoteRenderedAsIndirectSpeech_AllTextFromRefTextIsIncluded()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextMrk = refText.GetBook("MRK");
			var refBlocksMrk14V70 = refTextMrk.GetBlocksForVerse(14, 70).ToList();
			var narrator = GetStandardCharacterId(refTextMrk.BookId, StandardCharacter.Narrator);
			Assert.That(refBlocksMrk14V70.Count, Is.EqualTo(4),
				"Expected pre-condition for test not met. Reference text for MRK 14:70 should be 4 blocks.");
			Assert.That(refBlocksMrk14V70[0].CharacterId, Is.EqualTo(narrator),
				$"Expected pre-condition for test not met. Reference text block 0 for MRK 14:70 should be spoken by \"{narrator}\".");
			Assert.That(refBlocksMrk14V70[1].CharacterId, Is.EqualTo("Peter (Simon)"),
				"Expected pre-condition for test not met. Reference text block 1 for MRK 14:70 should be spoken by \"Peter (Simon)\".");
			Assert.That(refBlocksMrk14V70[2].CharacterId, Is.EqualTo(narrator),
				$"Expected pre-condition for test not met. Reference text block 2 for MRK 14:70 should be spoken by \"{narrator}\".");
			Assert.That(refBlocksMrk14V70[3].CharacterId, Is.EqualTo("high priest's servant (relative of the man whose ear Peter cut off)/those standing near"),
				"Expected pre-condition for test not met. Reference text block 3 for MRK 14:70 should be spoken by \"high priest's servant (relative of the man whose ear Peter cut off)/those standing near\".");

			var vernacularBlocks = new List<Block>
			{
				new Block("c", 14)
				{
					BookCode = refTextMrk.BookId,
					CharacterId = GetStandardCharacterId(refTextMrk.BookId, StandardCharacter.BookOrChapter),
				},
				CreateNarratorBlockForVerse(70, "Navuzwa Petero yambakana kandi.", false, 14, refTextMrk.BookId)
			};

			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Lwinyima hakekeke, avandu vaali ni vasingiyi ho vavoolera Petero, ", refTextMrk.BookId).IsParagraphStart = true;
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Agiligali oveeye mulala kuvo kigira uturaa Galilaya.»");
			var vernBook = new BookScript(refTextMrk.BookId, vernacularBlocks, m_vernVersification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1);
			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(3));
			Assert.That(string.Concat(matchup.CorrelatedBlocks[0].ReferenceBlocks.Select(r => r.GetText(true))),
				Is.EqualTo(refBlocksMrk14V70[0].GetText(true) + refBlocksMrk14V70[1].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[0].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksMrk14V70[2].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[1].GetText(true)}");
			Assert.That(matchup.CorrelatedBlocks[2].ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refBlocksMrk14V70[3].GetText(true)),
				$"Vern: {matchup.CorrelatedBlocks[2].GetText(true)}");
		}
		#endregion

		/// <summary>
		/// PG-1264
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_RefTextVerseStartsWithDirectSpeechButIsIndirectInVern_MatchedByAligningVerseNumbers()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Sarvia-machi-Joab, Rey-David Absalónʼgi-nue-binsaed magar daksad. ", true, 14, "2SA")
					.AddVerse(2, "Degisoggu, Joab ome-emar-binsaed-gaed imaksad. We-ome, Tecoa-neggweburginedid. Joab a-omega sogded:")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Joab", "—An bega, dule-mor-yoleged yoo. Dikasursur dule, be san-sao.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Agi, geb Joab, Rey-Davidʼse barmisad. A-iduale sunmaknai, Rey-Davidʼga sunmakdapoe. ", true, 14, "2SA")
				.AddVerse(4, "Ome-Tecoa-neggweburgined Rey-David-asabin gwisgunonigua, dulluu-napase imaksad. Davidʼga sogded:"));
			AddBlockForVerseInProgress(vernacularBlocks, "woman from Tekoa", "—¡Rey, be an-bendake!");
			var vernBook = new BookScript("2SA", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(1, "Now Joab perceived that the king's heart was toward Absalom.", true, 14, "2SA"),
				CreateNarratorBlockForVerse(2, "Joab sent to Tekoa, fetched a woman, and said,", false, 14, "2SA")
			};
			AddBlockForVerseInProgress(referenceBlocks, "Joab", "«Put on mourning clothes, and be as a woman who has mourned a long time.");
			referenceBlocks.Add(CreateBlockForVerse("Joab", 3, "Go in to the king, and speak thus.»", false, 14));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "So Joab put the words in her mouth. ", "2SA")
				.AddVerse(4, "When the woman spoke to the king, she fell on her face to the ground, saying,");
			referenceBlocks.Last().BookCode = "2SA";
			AddBlockForVerseInProgress(referenceBlocks, "woman from Tekoa", "«Help, O king!»");

			var expected = string.Concat(referenceBlocks.Skip(3).Take(2).Select(r => r.GetText(true)));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 2);

			Assert.That(matchup.CorrelatedBlocks.Count, Is.EqualTo(2));
			Assert.That(matchup.CorrelatedBlocks[0].MatchesReferenceText, Is.False);
			Assert.That(matchup.CorrelatedBlocks[1].MatchesReferenceText);
			Assert.That(string.Concat(matchup.CorrelatedBlocks[0].ReferenceBlocks.Select(r => r.GetText(true))),
				Is.EqualTo(expected));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks.Last().GetText(true)));
		}

		/// <summary>
		/// PG-1297
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernacularHasTextBeforeVerse1InChapter_MatchupDoesNotCrossChapterBoundary()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(25, "He had no relations with her until she had her firstborn son: and he called him Jesus.", true),
				NewChapterBlock("MAT", 2),
				new Block("s", 2) {CharacterId = GetStandardCharacterId("MAT", StandardCharacter.ExtraBiblical)}
					.AddText("The next thing"),
				new Block("p", 2) { CharacterId = GetStandardCharacterId("MAT", StandardCharacter.Narrator) }.AddText("The text before verse one:"),
				CreateBlockForVerse(kAmbiguousCharacter, 1, "“Who am I?”", false, 2)
			};
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			testProject.Books[0].Blocks = vernacularBlocks;

			var primaryReferenceText = GetStandardReferenceText(ReferenceTextType.English);
			for (var i = 0; i < vernacularBlocks.Count; i++)
			{
				var block = vernacularBlocks[i];
				if (!block.IsScripture)
					continue;
				var matchup = primaryReferenceText.GetBlocksForVerseMatchedToReferenceText(testProject.Books.First(), i);
				Assert.That(matchup.OriginalBlocks.Last().ChapterNumber, Is.EqualTo(matchup.OriginalBlocks.First().ChapterNumber));
			}
		}

		/// <summary>
		/// PG-1315
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernacularVerseMapsIntoMultiVerseRefBlockInPreviousChapter_MatchupCrossesChapterBoundary()
		{
			var testProject = TestProject.CreateTestProject(Properties.Resources.OriginalVersification,
				TestProject.TestBook.NUM);

			var numbersVern = testProject.GetBook("NUM");
			var iNum16V35VernBlock = numbersVern.GetIndexOfFirstBlockForVerse(16, 35);
			var iNum17V1VernBlock = numbersVern.GetIndexOfFirstBlockForVerse(17, 1);
			Assert.That(iNum16V35VernBlock < iNum17V1VernBlock, Is.True);
			var englishRefText = GetStandardReferenceText(ReferenceTextType.English);
			var numbersEng = englishRefText.GetBook("NUM");
			var num16V36Block = numbersEng.GetBlocksForVerse(16, 36).Single();
			if (num16V36Block.InitialStartVerseNumber != 35)
			{
				// This is a semi-dangerous hack. Since we can't get the lock, this isn't thread-safe. But most tests don't access NUM, so we're probably okay.
				var modifiedBooks = (HashSet<string>)ReflectionHelper.GetField(englishRefText, "m_modifiedBooks");
				modifiedBooks.Add("NUM");
				var num16V35Block = numbersEng.GetBlocksForVerse(16, 35).Single();
				num16V36Block.BlockElements.InsertRange(0, num16V35Block.BlockElements);
				num16V36Block.InitialStartVerseNumber = 35;
				num16V35Block.BlockElements.Clear();
				num16V35Block.BlockElements.Add(new ScriptText("This is the new ending for verse thirty-four."));
				num16V35Block.InitialStartVerseNumber = 34;
			}
			var matchup16V35 = englishRefText.GetBlocksForVerseMatchedToReferenceText(numbersVern, iNum16V35VernBlock);
			var matchup17V1 = englishRefText.GetBlocksForVerseMatchedToReferenceText(numbersVern, iNum17V1VernBlock);
			Assert.That(matchup16V35.OriginalBlocks.Select(b => b.GetText(true)).SequenceEqual(matchup17V1.OriginalBlocks.Select(b => b.GetText(true))), Is.True);
		}

		/// <summary>
		/// PG-1020/PG-1032: Handle case of well-aligned blocks with single quote where vern has verse bridge
		/// </summary>
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBridgeWithSingleQuoteThatMatchesQuoteInRefText_RefBlocksCombineToMatch()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(20, "Haxuya ba, ", true, 17, initialEndVerseNumber:21) };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Hatumingaim haringindi sanga te. Ahatumia: Mastat. Bila balau, ahatum ba longgalo! Bila na bimbia ila.”");
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block> { CreateNarratorBlockForVerse(20, "He said to them, ", true, 17) };
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Because of your unbelief. If you have faith, nothing will be impossible. ");
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 21, "But this kind leaves only by prayer and fasting.”", chapter:17));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(vernacularBlocks.Select(b => b.GetText(true)).SequenceEqual(result.Select(b => b.GetText(true))), Is.True);
			Assert.That(result.First().ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[0].GetText(true)));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(result.Last().ReferenceBlocks.Single().GetText(true), Is.EqualTo(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true)));
		}
		
		#region PG-1393
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VerseBridgeAtEndOfChapterWithTwoNarratorBlocks_OnlyOneVernBlockAlignsToCombinedRefBlocks()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(20, "Timoteo, cuida bien de no escuchar palabrerías, falsamente llamado " +
					"“conocimiento de la verdad”; pues algunos se han desviado de la que se te ha confiado fe por creer esa clase de " +
					"“conocimiento”.", true, 6, "1TI", "p", 21)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Que el Señor derrame su gracia sobre ustedes.", "1TI");
			var vernBook = new BookScript("1TI", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.False));
			var refTextVerses = result.SelectMany(b => b.ReferenceBlocks).SelectMany(r => r.BlockElements.OfType<Verse>()).ToList();
			Assert.That(refTextVerses.Count, Is.EqualTo(2));
			Assert.That(refTextVerses[0].StartVerse, Is.EqualTo(20));
			Assert.That(refTextVerses[1].StartVerse, Is.EqualTo(21));
		}
		#endregion

		#region PG-1394
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_HeSaidMatchAtStartOfVerse_AlignedAutomaticallyWithVerseNumberPrepended()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var mat = project.GetBook("MAT");

			var iMat21v33 = mat.GetIndexOfFirstBlockForVerse(21, 33);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(mat, iMat21v33, new[] {"Jesús Judío-dummaganga sogdebalid:"});

			// VERIFY
			Assert.That(matchup.CorrelatedBlocks, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var v33 = ((Verse)matchup.CorrelatedBlocks.First().BlockElements.First()).Number;
			Assert.That(((Verse)matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().BlockElements.First()).Number,
				Is.EqualTo(v33));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().BlockElements.OfType<Verse>().Any(v => v.Number == v33), Is.False);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_HeSaidMatchAtStartOfVerseWithDifferentVersification_AlignedAutomaticallyWithCorrespondingVerseNumberPrepended()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(6, "Continuó: ", false, 32, "GEN") };
			AddBlockForVerseInProgress(vernacularBlocks, "Jacob (Israel)", "“Tengo animales y gente para tirar para arriba. Vengo en paz.””");
			var vernBook = new BookScript("GEN", vernacularBlocks, ScrVers.Original);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, new[] {"Continuó:"});

			// VERIFY
			Assert.That(matchup.CorrelatedBlocks, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(((Verse)matchup.CorrelatedBlocks[0].ReferenceBlocks.Single().BlockElements.First()).Number,
				Is.EqualTo("5"));
			Assert.That(matchup.CorrelatedBlocks[1].ReferenceBlocks.Single().BlockElements.OfType<Verse>(), Is.Empty);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_HeSaidMatchAtEndOfVerse_AlignedAutomatically()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var mat = project.GetBook("MAT");

			var iMat8v7 = mat.GetIndexOfFirstBlockForVerse(8, 7);
			var blocks = mat.GetScriptBlocks();
			Assert.That(blocks[iMat8v7].InitialStartVerseNumber, Is.EqualTo(blocks[iMat8v7 + 1].InitialStartVerseNumber),
				"SETUP check - test data not as expected");
			Assert.That(IsCharacterOfType(blocks[iMat8v7].CharacterId, StandardCharacter.Narrator), Is.True,
				"SETUP check - test data not as expected");
			Assert.That(blocks[iMat8v7].InitialStartVerseNumber < blocks[iMat8v7 + 2].InitialStartVerseNumber, Is.True,
				"SETUP check - test data not as expected");
			var prevBlock = blocks[iMat8v7 + 1];
			var heSaidBlock = new Block(prevBlock.StyleTag, prevBlock.ChapterNumber,
				prevBlock.InitialStartVerseNumber, prevBlock.InitialEndVerseNumber)
			{
				CharacterId = blocks[iMat8v7].CharacterId,
				IsParagraphStart = false,
				BlockElements = new List<BlockElement>(1) { new ScriptText(", pregunto-sogde.") }
			};
			mat.Blocks.Insert(iMat8v7 + 2, heSaidBlock);
			ReflectionHelper.SetField(mat, "m_blockCount", 0); // It's illegal to change block collection, so we have to do this cheat to tell it it's okay.
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(mat, iMat8v7, new[] {", pregunto-sogde."});

			// VERIFY
			Assert.That(matchup.CorrelatedBlocks, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			Assert.That(matchup.CorrelatedBlocks.Last().ReferenceBlocks.Single().GetText(true),
				Is.EqualTo(refText.HeSaidText));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_HeSaidMatchAtStartOfVerse_AlignedAutomaticallyWithVerseNumberPrepended()
		{
			var project = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			project.AddNewReportingClauses(new[] {"Jesús Judío-dummaganga sogdebalid:"});
			var refText = GetStandardReferenceText(ReferenceTextType.English);

			var matchedUpMat = refText.GetBooksWithBlocksConnectedToReferenceText(project).Single();
			var iMat21v33 = matchedUpMat.GetIndexOfFirstBlockForVerse(21, 33);
			var blocks = matchedUpMat.GetScriptBlocks();

			// VERIFY
			Assert.That(blocks[iMat21v33].MatchesReferenceText);
			Assert.That(blocks[iMat21v33 + 1].MatchesReferenceText);
			Assert.That(((Verse)blocks[iMat21v33].ReferenceBlocks.Single().BlockElements.First()).Number,
				Is.EqualTo(((Verse)blocks[iMat21v33].BlockElements.First()).Number));
			Assert.That(blocks[iMat21v33 + 1].ReferenceBlocks.Single().ContainsVerseNumber, Is.False);
		}
		#endregion

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockSplitAtVerseWhichIsNotSplitInRefText_RefTextSplitToMatchVern()
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(7, "Dadi, pigupokatan nog glupaꞌ nog konaꞌ Hudyu. ", false, 27)
					.AddVerse(8, "Saꞌan ituꞌ nog Piktolongan nog Duguꞌ. ")
					.AddVerse(9, "Sog bianbian koni, mituman og inulat ni Dyerimaya nog,")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "scripture", "“Inalap nilan og puluꞌ buk solapiꞌ gotow Israꞌel.", "q1");
			vernacularBlocks.Add(CreateBlockForVerse("scripture", 10, "Bu piksaluy nilan og dondagan nog Mikpongon.”", false, 27, "q1"));
			vernacularBlocks.Add(new Block("s", 27, 10)
			{
				CharacterId = "extra-MAT",
				BlockElements = new List<BlockElement> (new BlockElement[] {new ScriptText("Piksakan si Isus ni Pilatu") }),
			});
			vernacularBlocks.Add(CreateNarratorBlockForVerse(11, "Dangan nilan si Isus diani Pilatu, sinakan non si Isus nog,"));
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“Ika taꞌ og datuꞌ kituꞌ nog bansa Hudyu?” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Tinumabal si Isus nog, ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“Ika na og miktaluꞌ dun.” ");

			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextBlocks = refText.GetBook("MAT").GetBlocksForVerse(27, 9).ToList();
			Assert.That(refTextBlocks.Last().LastVerseNum, Is.EqualTo(10),
				"SETUP check - expected English reference text to have verse 10 included with the last block for MAT 27:9.");

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			var scriptureBlocks = result.Where(b => b.CharacterId == "scripture").ToList();
			Assert.That(scriptureBlocks, ForEvery<Block>(
				(b => b.MatchesReferenceText, Is.True, nameof(Block.MatchesReferenceText)),
				(b => b.InitialStartVerseNumber == b.ReferenceBlocks.Single().InitialStartVerseNumber, Is.True,
					$"{nameof(Block.InitialStartVerseNumber)} == ReferenceBlocks.Single().InitialStartVerseNumber")));

			Assert.That(((Verse)scriptureBlocks.Last().ReferenceBlocks.Single().BlockElements.First()).Number, Is.EqualTo("10"));
		}

		#region PG-1395 (modified for PG-1396 and PG-1403)
		[TestCase(false)]
		[TestCase(true)]
		public void GetBlocksForVerseMatchedToReferenceText_ReportingClausesComeAfterSpeechLinesInVerseWithMultipleSpeakers_AllBlocksMatchedWithReportingClausesMatchedToModifiedReportingClauses(
			bool blockIsNeedsReview)
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Jesus", 31, "Кьве хцикай бубадин тӀалабун ни кьилиз акъудна?» ", false, 21) };
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Сад лагьайда», ");
			var heSaidBlock = AddNarratorBlockForVerseInProgress(vernacularBlocks, "– жаваб гана абуру. ");
			var narrator = heSaidBlock.CharacterId;
			if (blockIsNeedsReview)
				heSaidBlock.CharacterId = kNeedsReview;
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«За квез рикӀивай лугьузва: харж кӀватӀдайбурни ява папар Аллагьдин Пачагьлугъдиз квелай вилик акъатда», ");
			heSaidBlock = AddNarratorBlockForVerseInProgress(vernacularBlocks, "– лагьана Исади. – ");
			if (blockIsNeedsReview)
				heSaidBlock.CharacterId = kNeedsReview;
			var vernBook = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextBlocks = refText.GetBook("MAT").GetBlocksForVerse(21, 31).ToList();
			Assert.That(refTextBlocks.Count, Is.EqualTo(5),
				"SETUP check - expected English reference text to have five blocks for Matthew 21:31.");
			Assert.That(refTextBlocks[0].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in first block for Luke 7:40.");
			Assert.That(refTextBlocks[1].GetText(false), Does.Contain("They said"),
				"SETUP check - expected English reference text to have reporting clause introducing chief priests/elders");
			Assert.That(refTextBlocks[2].CharacterId, Is.EqualTo("chief priests/elders"),
				"SETUP check - expected English reference text to have chief priests/elders speak in response to Jesus' question in Matthew 21:31.");
			Assert.That(refTextBlocks[3].GetText(false), Does.Contain("Jesus said"),
				"SETUP check - expected English reference text to have reporting clause introducing Jesus");
			Assert.That(refTextBlocks.Last().CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in final block for Matthew 21:31.");

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, new []{"– лагьана ада.", "– лагьана Исади. –", "– минетдалди лагьана ада. –", "– жаваб гана абуру."});
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			var textOfMatchedRefTextBlocks = result.SelectMany(b => b.ReferenceBlocks).Select(r => r.GetText(true)).ToList();

			Assert.That(textOfMatchedRefTextBlocks[0], Is.EqualTo(refTextBlocks[0].GetText(true)));
			Assert.That(textOfMatchedRefTextBlocks[1], Is.EqualTo(refTextBlocks[2].GetText(true)));
			Assert.That(textOfMatchedRefTextBlocks[2].ToLower(), Is.EqualTo(
				refTextBlocks[1].GetText(true).ToLower().Replace(",", ".")));
			Assert.That(result[2].CharacterId, Is.EqualTo(narrator));
			Assert.That(textOfMatchedRefTextBlocks[3], Is.EqualTo(refTextBlocks[4].GetText(true)));
			Assert.That(textOfMatchedRefTextBlocks[4].ToLower(), Is.EqualTo(
				refTextBlocks[3].GetText(true).ToLower().Replace(",", ".")));
			Assert.That(result[4].CharacterId, Is.EqualTo(narrator));
		}
		#endregion

		#region PG-1396
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_SimpleReportingClauseComesAfterDialogueInsteadOfBetweenSpeakers_InterveningReportingClauseOmitted()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(40, "Амма Исади, адахъ элкъвена, лагьана: ", false, 7, "LUK") };
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Симун! Захъ ваз лугьудай са гаф ава». ");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "«Лагь, Муаллим», ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "– лагьана ада. ");
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextBlocks = refText.GetBook("LUK").GetBlocksForVerse(7, 40).ToList();
			Assert.That(refTextBlocks.Count, Is.EqualTo(4), "SETUP check - expected English reference text to have four blocks for Luke 7:40.");
			Assert.That(refTextBlocks[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for Luke 7:40.");
			Assert.That(((ScriptText)refTextBlocks[1].BlockElements.Single()).Content.TrimEnd(), Does.EndWith("»"),
				"SETUP check - expected English reference text to have Jesus' words in quotes for Luke 7:40.");
			Assert.That(refTextBlocks[2].GetText(false).ToLower().Replace(",", "."),
				Is.EqualTo(refText.HeSaidText),
				"SETUP check - expected English reference text to have leading reporting clause between two speakers");
			Assert.That(refTextBlocks.Last().CharacterId, Is.EqualTo("Pharisee (Simon)"),
				"SETUP check - expected English reference text to have Pharisee (Simon) speak in final block for Luke 7:40.");

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, new []{"– лагьана ада.", "– лагьана Исади. –", "– жаваб гана абуру."});
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlocks[0].GetText(true)),
				"Expected the first narrator block in the English reference text to be matched to the first block of matchup for Luke 7:40.");

			Assert.That(result[1].CharacterId, Is.EqualTo(kAmbiguousCharacter),
				"Speaker not expected to be automatically assigned in Jesus' block of matchup for Luke 7:40.");
			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlocks[1].GetText(true)),
				"Expected the block with Jesus speaking in the English reference text to be matched to the second block of matchup for Luke 7:40.");

			Assert.That(result[2].CharacterId, Is.EqualTo(kAmbiguousCharacter),
				"Speaker not expected to be automatically assigned in Pharisee (Simon)'s block of matchup for Luke 7:40.");
			Assert.That(result[2].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlocks.Last().GetText(true)),
				"Expected the block with Pharisee (Simon) speaking in the English reference text to be matched to the third block of matchup for Luke 7:40.");

			Assert.That(result.Last().ReferenceBlocks.Single().GetText(true), Is.EqualTo(refText.HeSaidText),
				"Expected last block of matchup for Luke 7:40 to match up to a generated \"he said\" block.");
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_ReportingClauseContainingHeSaidComesAfterSpeechInsteadOfAtStartOfVerse_InterveningReportingClauseOmitted()
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Jesus", 18, "«Заз цӀайлапан хьиз цаварай аватай иблис акуна», ", false, 10) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "– лагьана Исади. – ", "LUK");
			vernacularBlocks.Add(CreateBlockForVerse("Jesus", 19, "«Ингье, За квез гъуьлягърални... ", false, 10));
			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextBlocks = refText.GetBook("LUK").GetBlocksForVerse(10, 18).ToList();
			Assert.That(refTextBlocks.Count, Is.EqualTo(2), "SETUP check - expected English reference text to have two blocks for Luke 10:18.");
			Assert.That(refTextBlocks[0].StartsAtVerseStart, Is.True,
				"SETUP check - expected the first English reference text for Luke 10:18 to start at the beginning of v 18.");
			var refTextHeSaidLowercase = refText.HeSaidText.ToLower().Trim('.');
			var refTextActualReportingClauseLowercase = refTextBlocks[0].GetText(false).ToLower();
			Assert.That(refTextHeSaidLowercase, Is.Not.EqualTo(refTextActualReportingClauseLowercase),
				"SETUP check - expected the initial reporting clause in the English reference text to have additional words besides \"he said\".");
			Assert.That(refTextActualReportingClauseLowercase, Does.Contain(refTextHeSaidLowercase),
				"SETUP check - expected the initial reporting clause in the English reference text to contain \"he said\".");
			Assert.That(refTextBlocks[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for Luke 10:18.");

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, new []{"– лагьана ада.", "– лагьана Исади. –", "– жаваб гана абуру."});
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var firstMatchedRefBlock = result[0].ReferenceBlocks.Single();
			Assert.That(firstMatchedRefBlock.StartsAtVerseStart, Is.True,
				"Expected the matched reference text of the first block to start with v 18.");
			Assert.That(firstMatchedRefBlock.InitialStartVerseNumber, Is.EqualTo(18),
				"Expected the matched reference text of the first block to start with v 18.");
			Assert.That(result[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(firstMatchedRefBlock.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(firstMatchedRefBlock.GetText(false),
				Is.EqualTo(refTextBlocks[1].GetText(false)),
				"Expected the block with Jesus speaking in the English reference text to be matched to the first block of matchup for Luke 10:18.");
			var lastMatchedRefBlock = result.Last().ReferenceBlocks.Single();
			Assert.That(lastMatchedRefBlock.CharacterId, Is.EqualTo(refTextBlocks[0].CharacterId),
				"Expected last block of matchup for Luke 10:18 to be spoken by the narrator.");
			Assert.That(lastMatchedRefBlock.GetText(true),
				Is.EqualTo(refTextActualReportingClauseLowercase.Replace(",", ".")),
				"Expected last block of matchup for Luke 10:18 to match up to the modified omitted \"he said\" block.");
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_ComplexReportingClauseComesAfterSpeechInsteadOfAtStartOfVerse_InterveningReportingClauseOmitted()
		{
			var vernacularBlocks = new List<Block> { CreateBlockForVerse("Jesus", 19, "«Чам чпихъ галамай кьван гагьда мехъерин мугьманривай сив хвена акъвазиз жедани?» ", false, 2) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "– лагьана Исади. – ", "MRK");
			var vernBook = new BookScript("MRK", vernacularBlocks, m_vernVersification);

			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextBlocks = refText.GetBook("MRK").GetBlocksForVerse(2, 19).ToList();
			Assert.That(refTextBlocks.Count, Is.EqualTo(2), "SETUP check - expected English reference text to have two blocks for Mark 2:19.");
			Assert.That(refTextBlocks[0].StartsAtVerseStart, Is.True,
				"SETUP check - expected the first English reference text for Mark 2:19 to start at the beginning of v 19.");
			Assert.That(refTextBlocks[0].GetText(false).Trim(),
				Is.EqualTo("Jesus said to them,"),
				"SETUP check - unexpected initial reporting clause in the English reference text.");
			Assert.That(refTextBlocks[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for Mark 2:19.");

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, new []{"– лагьана ада.", "– лагьана Исади. –", "– жаваб гана абуру."});
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
			var firstMatchedRefBlock = result[0].ReferenceBlocks.Single();
			Assert.That(firstMatchedRefBlock.StartsAtVerseStart, Is.True,
				"Expected the matched reference text of the first block to start with v 19.");
			Assert.That(firstMatchedRefBlock.InitialStartVerseNumber, Is.EqualTo(19),
				"Expected the matched reference text of the first block to start with v 19.");
			Assert.That(result[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(firstMatchedRefBlock.CharacterId, Is.EqualTo("Jesus"));
			Assert.That(firstMatchedRefBlock.GetText(false),
				Is.EqualTo(refTextBlocks[1].GetText(false)),
				"Expected the block with Jesus speaking in the English reference text to be matched to the first block of matchup for Mark 2:19.");
			var lastMatchedRefBlock = result.Last().ReferenceBlocks.Single();
			Assert.That(lastMatchedRefBlock.CharacterId,
				Is.EqualTo(refTextBlocks[0].CharacterId),
				"Expected last block of matchup for Mark 2:19 to be spoken by the narrator.");
			Assert.That(lastMatchedRefBlock.GetText(true),
				Is.EqualTo("Jesus said to them."),
				"Expected last block of matchup for Mark 2:19 to match up to a generated \"he said\" block.");
		}
		#endregion

		#region PG-1403
		// There was a bug that caused the reference text to get changed as a side-effect of
		// applying the "he said" so that a subsequent retrieval of split locations using
		// GetVerseSplitLocations could give different results, so that a call to
		// IsOkayToSplitBeforeBlock that had previously returned true would return false.
		[Test]
		public void IsOkayToSplitBeforeBlock_CalledAfterGetBlocksForVerseMatchedToReferenceTextThatAutoMatchesHeSaidAtStartOfVerse_ReturnsTrue()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.Russian);
			var firstRussianBlockForMat6v5 = refText.Books.Single(b => b.BookId == "MAT").GetFirstBlockForVerse(6, 5);
			var russianTextOfFirstBlockForMat6v5 = firstRussianBlockForMat6v5.GetText(true);
			var englishTextOfFirstBlockForMat6v5 = firstRussianBlockForMat6v5.ReferenceBlocks.Single().GetText(true);

			var vernacularBlocks = new List<Block>
			{
				NewChapterBlock("MAT", 6),
				CreateBlockForVerse("Jesus", 4, "“Thus your beneficial acts will be executed privately and God, who sees all that is hidden, will give you your trophy.”", true, 6),
				new Block("s", 6, 4)
				{
					CharacterId = "extra-MAT",
					BlockElements = new List<BlockElement> (new BlockElement[] {new ScriptText("What next?") }),
				},
				CreateNarratorBlockForVerse(5, "Jesus continued: ", true, 6)
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus",
					"“When you pray, don't babble like a pagan, thinking that you can overwhelm God by sheer volume of words.”")
				.AddVerse(6, "Instead try praying along these lines: ")
				.AddVerse(7, "Dearest Holy Father, your name is above all others.");
			var mat = new BookScript("MAT", vernacularBlocks, m_vernVersification);

			var iMat6v5 = mat.GetIndexOfFirstBlockForVerse(6, 5);
			Assert.That(refText.IsOkayToSplitBeforeBlock(mat,
				mat.GetScriptBlocks()[iMat6v5], refText.GetVerseSplitLocations("MAT")), Is.True);

			refText.GetBlocksForVerseMatchedToReferenceText(mat, iMat6v5, new[] {"Jesus continued:"});

			// VERIFY
			Assert.That(refText.IsOkayToSplitBeforeBlock(mat,
				mat.GetScriptBlocks()[iMat6v5], refText.GetVerseSplitLocations("MAT")), Is.True);
			// Further verify that text of blocks in reference text have not changed:
			firstRussianBlockForMat6v5 = refText.Books.Single(b => b.BookId == "MAT").GetFirstBlockForVerse(6, 5);
			Assert.That(firstRussianBlockForMat6v5.GetText(true), Is.EqualTo(russianTextOfFirstBlockForMat6v5));
			// This proves that the clone was a deep clone:
			Assert.That(firstRussianBlockForMat6v5.ReferenceBlocks.Single().GetText(true), Is.EqualTo(englishTextOfFirstBlockForMat6v5));
		}
		#endregion

		#region PG-1408
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_ClosingDashAtStartOfLongHeSaidParagraphNotIdentifiedAsDialogueCloser_NoPartOfReferenceTextDuplicatedNorOmitted()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextJhn = refText.GetBook("JHN");
			var refTextBlocksForJhn12V35 = refTextJhn.GetBlocksForVerse(12, 35).ToList();
			Assert.That(refTextBlocksForJhn12V35.Count, Is.EqualTo(2),
				"SETUP check - expected English reference text to have two blocks for John 12:35.");
			Assert.That(refTextBlocksForJhn12V35[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for John 12:35.");
			Assert.That(refTextBlocksForJhn12V35[1].LastVerseNum, Is.EqualTo(35),
				"SETUP check - expected English reference text to have have a block break between John 12:35 and v. 36.");
			var refTextBlocksForJhn12V36 = refTextJhn.GetBlocksForVerse(12, 36).ToList();
			Assert.That(refTextBlocksForJhn12V36.Count, Is.EqualTo(2),
				"SETUP check - expected English reference text to have two blocks for John 12:36.");
			Assert.That(refTextBlocksForJhn12V36[0].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in the first block for John 12:36.");
			Assert.That(IsCharacterOfType(refTextBlocksForJhn12V36[1].CharacterId,
				StandardCharacter.Narrator), Is.True,
				"SETUP check - expected English reference text to have the narrator speak in the second block for John 12:36.");

			var matchup = GetBlockMatchupForJohn12V35And36ForPg1408();
			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(4));

			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText, Is.False);

			Assert.That(refTextBlocksForJhn12V35[0].GetText(true), Is.EqualTo(result[0].ReferenceBlocks.Single().GetText(true)),
				"Expected the first narrator block in the English reference text for John 12:35 to be matched to the first block of matchup.");
			Assert.That(refTextBlocksForJhn12V35[1].GetText(true), Is.EqualTo(result[1].ReferenceBlocks.Single().GetText(true)),
				"Expected the second block (Jesus) in the English reference text for John 12:35 to be matched to the second block of matchup.");
			Assert.That(refTextBlocksForJhn12V36[0].GetText(true), Is.EqualTo(result[2].ReferenceBlocks.Single().GetText(true)),
				"Expected the first block (Jesus) in the English reference text for John 12:36 to be matched to the third block of matchup.");
			Assert.That(refTextBlocksForJhn12V36[1].GetText(true), Is.EqualTo(result[3].ReferenceBlocks.Single().GetText(true)),
				"Expected the second block (narrator) in the English reference text for John 12:36 to be correlated (but not matched) to the last block of matchup.");
		}

		internal static BlockMatchup GetBlockMatchupForJohn12V35And36ForPg1408()
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(35, "ꞌBa Jesús kitsure:", true, 12, "JHN") };
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Tu ngichuba nixtjin kꞌuejñajin ngisanu ndiꞌu. Ngatꞌa xi ña jñu tsuꞌba bi be ñá fi. ")
				.AddVerse(36, "Ngatjamakjainnu ngatꞌare ndiꞌu yejerañu tjin ngisanu, tuxi tseꞌe ndiꞌu ku̱anñu");
			// The following is supposed to be spoken by the narrator, but the "closing" dash at the start of the
			// paragraph is treated as an opener.
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—kitsu Jesús, ꞌba ꞌbatsaꞌen ꞌetju ꞌba tsikꞌejñaꞌmore xutankjiun.")
				.IsParagraphStart = true;
			var vernBook = new BookScript("JHN", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			return matchup;
		}

		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetBlocksForVerseMatchedToReferenceText_ClosingDashAtStartOfShortHeSaidParagraphNotIdentifiedAsDialogueCloser_NoPartOfReferenceTextDuplicatedNorOmitted(
			ReferenceTextType type)
		{
			var narrator = GetStandardCharacterId("MRK", StandardCharacter.Narrator);
			var refText = GetStandardReferenceText(type);
			var refTextMrk = refText.GetBook("MRK");
			var refTextBlocksForMrk15V14 = refTextMrk.GetBlocksForVerse(15, 14).ToList();
			Assert.That(refTextBlocksForMrk15V14.Count, Is.EqualTo(4), "SETUP check - expected reference text to have four blocks for Mark 15:14.");
			Assert.That(refTextBlocksForMrk15V14[0].CharacterId, Is.EqualTo(narrator),
				"SETUP check - expected reference text to have narrator speak in first block for Mark 15:14.");
			Assert.That(refTextBlocksForMrk15V14[1].CharacterId, Is.EqualTo("Pilate"),
				"SETUP check - expected reference text to have Pilate speak in second block for Mark 15:14.");
			Assert.That(refTextBlocksForMrk15V14[2].CharacterId, Is.EqualTo(narrator),
				"SETUP check - expected reference text to have narrator speak in third block for Mark 15:14.");
			Assert.That(refTextBlocksForMrk15V14[3].CharacterId, Is.EqualTo("crowd before Pilate"),
				"SETUP check - expected reference text to have the crowd before Pilate speak in last block for Mark 15:14.");
			Assert.That(refTextBlocksForMrk15V14.Last().LastVerseNum, Is.EqualTo(14),
				"SETUP check - expected reference text to have have a block break between Mark 15:14 and v. 15.");

			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(kAmbiguousCharacter, 14,
					"—Tunga ¿mé= kjua xi chꞌotjin xi kitsaꞌen kui?", true, 15)
			};
			// The following is supposed to be spoken by the narrator, but the "closing" dash at the start of the
			// paragraph is treated as an opener.
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"—kitsingojo Pilato.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Tunga xutankjiun ngisa ꞌñu kiskiꞌndaya ꞌba kitsu:", "MRK");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"—¡Tjatꞌai kru!");
			var vernBook = new BookScript("MRK", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(((Verse)result[0].BlockElements[0]).Number, Is.EqualTo("14"));
			Assert.That(result.Take(2).All(b => b.MatchesReferenceText), Is.False,
				"One or both of the first two blocks should not match the reference text.");
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText);

			var allReferenceBlocks = result.SelectMany(b => b.ReferenceBlocks).ToList();
			Assert.That(allReferenceBlocks.Count, Is.EqualTo(4),
				"There should be no extra or missing reference blocks");

			Assert.That(refTextBlocksForMrk15V14.Select(b => b.CharacterId),
				Is.EquivalentTo(allReferenceBlocks.Select(b => b.CharacterId)),
				"The resulting reference blocks should have the same characters as the original reference blocks");

			var blockComparer = new BlockComparer();

			Assert.That(allReferenceBlocks.Distinct(blockComparer).Count(), Is.EqualTo(4),
				"There should be no duplicate reference blocks");

			Assert.That(allReferenceBlocks.Single(b => b.StartsAtVerseStart).BlockElements.OfType<Verse>().Single().AllVerseNumbers.Single(), Is.EqualTo(14));

			var indexOfPrevRefBlockInOriginalList = -1;
			foreach (var nonPilateSaidRefBlock in refTextBlocksForMrk15V14.Skip(1))
			{
				var indexOfThisRefBlockInResult = allReferenceBlocks.FindIndex(b => blockComparer.Equals(b, nonPilateSaidRefBlock));
				Assert.That(indexOfThisRefBlockInResult >= 0, Is.True,
					$"Reference block {nonPilateSaidRefBlock} missing. With the possible exception of" +
					$" the \"he said\" block for Pilate, all reference blocks should be in the" +
					$" list of blocks corresponding to a vernacular block.");
				Assert.That(indexOfThisRefBlockInResult >= indexOfPrevRefBlockInOriginalList, Is.True,
					$"Reference block {nonPilateSaidRefBlock} out of order. With the possible exception" +
					$" of the \"he said\" block for Pilate, all reference blocks should be in the" +
					$" same relative order as their occur in the reference text.");
				indexOfPrevRefBlockInOriginalList = indexOfThisRefBlockInResult;

				if (refText.HasSecondaryReferenceText)
				{
					var refBlockInOrigList = allReferenceBlocks[indexOfThisRefBlockInResult].ReferenceBlocks.Single();
					var refBlockInResults = nonPilateSaidRefBlock.ReferenceBlocks.Single();
					Assert.That(blockComparer.Equals(refBlockInOrigList, refBlockInResults), Is.True,
						"Secondary (English) reference text got hooked up differently from the primary!");
				}
			}

			if (refText.HasSecondaryReferenceText)
			{
				Assert.That(allReferenceBlocks.Select(b => b.ReferenceBlocks.Single()).Single(b => b.StartsAtVerseStart)
					.BlockElements.OfType<Verse>().Single().AllVerseNumbers.Single(),
					Is.EqualTo(14));

				var firstRefBlock = allReferenceBlocks[0];
				Assert.That(firstRefBlock.MatchesReferenceText);
				var secondaryRefBlock = firstRefBlock.ReferenceBlocks.Single();
				Assert.That(firstRefBlock.StartsAtVerseStart,
					Is.EqualTo(secondaryRefBlock.StartsAtVerseStart));
				Assert.That(firstRefBlock.InitialStartVerseNumber,
					Is.EqualTo(secondaryRefBlock.InitialStartVerseNumber));
				Assert.That(firstRefBlock.LastVerseNum,
					Is.EqualTo(secondaryRefBlock.LastVerseNum));
			}
		}

		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetBlocksForVerseMatchedToReferenceText_ClosingDashAtStartOfShortHeSaidParagraphNotIdentifiedAsDialogueCloserInSameOrderAsRefText_UnmodifiedRefBlocksAlignedWithOneUnmatched(
			ReferenceTextType type)
		{
			var narrator = GetStandardCharacterId("MRK", StandardCharacter.Narrator);
			var refText = GetStandardReferenceText(type);
			var refTextMrk = refText.GetBook("MRK");
			var refTextBlocksForMrk10V39And40 = refTextMrk.GetBlocksForVerse(10, 39).ToList();
			Assert.That(refTextBlocksForMrk10V39And40.Count, Is.EqualTo(4),
				"SETUP check - expected reference text to have four blocks for Mark 10:39.");
			Assert.That(refTextBlocksForMrk10V39And40[0].CharacterId, Is.EqualTo(narrator),
				"SETUP check - expected reference text to have narrator speak in first block for Mark 10:39.");
			Assert.That(refTextBlocksForMrk10V39And40[1].CharacterId,
				Is.EqualTo("James, the disciple/John"),
				"SETUP check - expected reference text to have James and John speak in second block for Mark 10:39.");
			Assert.That(refTextBlocksForMrk10V39And40[2].CharacterId, Is.EqualTo(narrator),
				"SETUP check - expected reference text to have narrator speak in third block for Mark 10:39.");
			Assert.That(refTextBlocksForMrk10V39And40[3].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected reference text to have Jesus speak in last block for Mark 10:39.");
			Assert.That(refTextBlocksForMrk10V39And40.Last().LastVerseNum, Is.EqualTo(39),
				"SETUP check - expected reference text to have have a block break between Mark 10:39 and v. 40.");
			var refTextBlocksForMrk10V40 = refTextMrk.GetBlocksForVerse(10, 40).Single();
			Assert.That(refTextBlocksForMrk10V40.CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected reference text to have Jesus speak in block for Mark 10:40.");
			Assert.That(refTextBlocksForMrk10V40.BlockElements.OfType<Verse>().Single().Number, Is.EqualTo("40"),
				"SETUP check - expected reference text to have have a block break between Mark 10:40 and v. 41.");
			refTextBlocksForMrk10V39And40.Add(refTextBlocksForMrk10V40);

			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(kAmbiguousCharacter, 39,
					"—Ku̱an=ni", true, 10)
			};
			// The following is supposed to be spoken by the narrator, but the "closing" dash at the start of the
			// paragraph is treated as an opener.
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"—kitsingojo ngojó.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "ꞌBa Jesús kitsure:", "MRK");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"—Sꞌiu nda sja xi kꞌuia̱ an, ꞌba sa̱tendo ko kjuanima jotsaꞌen sa̱tendaa̱; ")
				.AddVerse(40, "tunga tsa ngate kixina̱ asa ngate skjunna̱ kuetsubo, tu kui= xuta xi je kisꞌendare ngaꞌndebiu kuaꞌere.");
			var vernBook = new BookScript("MRK", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(5));
			Assert.That(((Verse)result[0].BlockElements[0]).Number, Is.EqualTo("39"));
			Assert.That(result.Take(2).All(b => b.MatchesReferenceText), Is.False,
				"One or both of the first two blocks should not match the reference text.");
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText);
			Assert.That(result[4].MatchesReferenceText);

			var allReferenceBlocks = result.SelectMany(b => b.ReferenceBlocks).ToList();
			Assert.That(allReferenceBlocks.Count, Is.EqualTo(5),
				"There should be no extra or missing reference blocks");

			Assert.That(refTextBlocksForMrk10V39And40.Select(b => b.CharacterId),
				Is.EquivalentTo(allReferenceBlocks.Select(b => b.CharacterId)),
				"The resulting reference blocks should have the same characters as the original reference blocks");

			var blockComparer = new BlockComparer();

			Assert.That(allReferenceBlocks.Distinct(blockComparer).Count(), Is.EqualTo(5),
				"There should be no duplicate reference blocks");

			Assert.That(allReferenceBlocks.SelectMany(b => b.BlockElements).OfType<Verse>()
				.Select(v => v.Number),
				Is.EquivalentTo(new[] { "39", "40" }));

			Assert.That(allReferenceBlocks.SequenceEqual(refTextBlocksForMrk10V39And40, blockComparer), Is.True);

			if (refText.HasSecondaryReferenceText)
			{
				Assert.That(allReferenceBlocks, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
				var englishRefBlocks = allReferenceBlocks.Select(b => b.ReferenceBlocks.Single()).ToList();

				Assert.That(englishRefBlocks.Distinct(blockComparer).Count(), Is.EqualTo(5),
					"There should be no duplicate English reference blocks");

				Assert.That(englishRefBlocks.SelectMany(b => b.BlockElements).OfType<Verse>()
					.Select(v => v.Number),
					Is.EquivalentTo(new [] {"39", "40"}));

				Assert.That(englishRefBlocks.SequenceEqual(refTextBlocksForMrk10V39And40.Select(b => b.ReferenceBlocks.Single()), blockComparer), Is.True);
			}
		}

		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetBlocksForVerseMatchedToReferenceText_InterruptingDashForHeSaidParagraphNotIdentifiedAsDialogueCloserInVerseWithSingleSpeaker_HeSaidNotOmitted(
			ReferenceTextType type)
		{
			var narrator = GetStandardCharacterId("MAT", StandardCharacter.Narrator);
			var refText = GetStandardReferenceText(type);
			var refTextMat = refText.GetBook("MAT");
			var refTextBlocksForMat19V20 = refTextMat.GetBlocksForVerse(19, 20).ToList();
			Assert.That(refTextBlocksForMat19V20.Count, Is.EqualTo(2), "SETUP check - expected reference text to have two blocks for Matthew 19:20.");
			Assert.That(refTextBlocksForMat19V20[0].CharacterId, Is.EqualTo(narrator),
				"SETUP check - expected reference text to have narrator speak in first block for Matthew 19:20.");
			Assert.That(refTextBlocksForMat19V20[1].CharacterId, Is.EqualTo("rich young ruler"),
				"SETUP check - expected reference text to have the rich young ruler speak in second block for Matthew 19:20.");
			Assert.That(refTextBlocksForMat19V20.Last().LastVerseNum, Is.EqualTo(20),
				"SETUP check - expected reference text to have have a block break between Matthew 19:20 and v. 21.");

			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(refTextBlocksForMat19V20[1].CharacterId, 20,
					"—Ngayéjebiu je kitsikꞌetjusaa̱n", true, 19)
			};
			// The following is supposed to be spoken by the narrator, but the "closing" (interrupting) dash at the
			// start of the paragraph is treated as an opener.
			AddBlockForVerseInProgress(vernacularBlocks, refTextBlocksForMat19V20[1].CharacterId,
				"—kitsu chanaꞌenbiu");
			AddBlockForVerseInProgress(vernacularBlocks, refTextBlocksForMat19V20[1].CharacterId,
				"—. ¿Mé= xi chaja ngisana̱?");
			var vernBook = new BookScript("MAT", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(((Verse)result[0].BlockElements[0]).Number, Is.EqualTo("20"));
			Assert.That(result.All(b => b.MatchesReferenceText), Is.False,
				"At least one of the blocks should not match the reference text.");

			var allReferenceBlocks = result.SelectMany(b => b.ReferenceBlocks).ToList();

			Assert.That(allReferenceBlocks.Any(b => b.CharacterId == narrator), Is.True,
				"The narrator block with the reporting clause should not be omitted.");

			var blockComparer = new BlockComparer();

			Assert.That(allReferenceBlocks.Contains(refTextBlocksForMat19V20[1], blockComparer), Is.True,
				"The reference block spoken by the rich young ruler should not be omitted.");

			Assert.That(((Verse)allReferenceBlocks[0].BlockElements.First()).Number, Is.EqualTo("20"));

			if (refText.HasSecondaryReferenceText)
			{
				Assert.That(allReferenceBlocks, ForEvery<Block>(b => b.MatchesReferenceText, Is.True));
				Assert.That(((Verse)allReferenceBlocks.Select(b => b.ReferenceBlocks.Single()).First().BlockElements.First()).Number, Is.EqualTo("20"));
			}
		}
		#endregion

		#region PG-1423
		[TestCase(true)]
		[TestCase(false)]
		public void GetBlocksForVerseMatchedToReferenceText_OpeningAndClosingReportingClausesInVerseThatIsAllQuotationInEnglishReferenceText_EntireReferenceTextAssociatedWithQuoteBlock(
			bool includeClosingHeSaidInReportingClauses)
		{
			ReferenceText refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextMat = refText.GetBook("MAT");
			var refTextBlockForMat2V19 = refTextMat.GetBlocksForVerse(2, 19).Single();
			Assert.That(IsCharacterOfType(refTextBlockForMat2V19.CharacterId, StandardCharacter.Narrator), Is.True,
				"SETUP check - expected English reference text to have narrator speak in block for Mat 2:19.");
			Assert.That(refTextBlockForMat2V19.LastVerseNum, Is.EqualTo(19),
				"SETUP check - expected English reference text to have have a block break between Mat 2:19 and v. 20.");

			var refTextBlockForMat2V20 = refTextMat.GetBlocksForVerse(2, 20).Last();
			Assert.That(refTextBlockForMat2V20.CharacterId, Is.EqualTo("angel"),
				"SETUP check - expected English reference text to have angel speak in block for Mat 2:20.");
			Assert.That(refTextBlockForMat2V20.LastVerseNum, Is.EqualTo(20),
				"SETUP check - expected English reference text to have have a block break between Mat 2:20 and v. 21.");

			var matchup = GetBlockMatchupForMat2V20(includeClosingHeSaidInReportingClauses, refTextBlockForMat2V20, refText);
			
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText, Is.EqualTo(includeClosingHeSaidInReportingClauses));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlockForMat2V19.GetText(true)),
				"Expected the narrator block in the English reference text for Mat 2:19 to be matched to the first block of matchup.");

			Assert.That(result[2].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlockForMat2V20.GetText(true)),
				"Expected the block (angel) in the English reference text for Mat 2:20 to be matched to the third block of matchup.");
		}

		private static BlockMatchup GetBlockMatchupForMat2V20(bool includeClosingHeSaidInReportingClauses, Block refTextBlockForMat2V20, ReferenceText refText)
		{
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(19,
						"Apnengkek mokhom Jose m'a iokhalhma Egipto, apveske Herodes. Neksa apteianma lhnak Jose m'a,mokhom angel apkapaskama Apveske,", true, 2)
					.AddVerse(20, "lhna aptemak:")
			};
			// The following is supposed to be spoken by the narrator, but the "closing" (interrupting) dash at the
			// start of the paragraph is treated as an opener.
			AddBlockForVerseInProgress(vernacularBlocks, refTextBlockForMat2V20.CharacterId,
				"—¡Elhatakha, eiantemekha nematka nak kakpota nhan ngken akieto Israel, apkenmaskengvakme apkenmahai'a lhta ennapok nematka nak! ")
				.Delivery = refTextBlockForMat2V20.Delivery;
			var closingHeSaid = AddNarratorBlockForVerseInProgress(vernacularBlocks, "—lhna aptemak.");

			var vernBook = new BookScript("MAT", vernacularBlocks, refText.Versification);

			var reportingClauses = new List<string>(new[] {"— lhna aptemak ma'a.", "lhna aptemak ma'a.", "lhna aptemak."});
			if (includeClosingHeSaidInReportingClauses)
				reportingClauses.Insert(2, "—lhna aptemak.");
			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, reportingClauses);
			return matchup;
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetBlocksForVerseMatchedToReferenceText_OpeningAndClosingReportingClausesInVerseThatIsAllQuotationInReferenceText_EntireReferenceTextAssociatedWithQuoteBlock(
			bool includeClosingHeSaidInReportingClauses)
		{
			ReferenceText refText = GetStandardReferenceText(ReferenceTextType.Russian);
			var refTextMat = refText.GetBook("MAT");
			var refTextBlockForMat2V19 = refTextMat.GetBlocksForVerse(2, 19).Single();
			Assert.That(IsCharacterOfType(refTextBlockForMat2V19.CharacterId, StandardCharacter.Narrator), Is.True,
				"SETUP check - expected Russian reference text to have narrator speak in block for Mat 2:19.");
			Assert.That(refTextBlockForMat2V19.LastVerseNum, Is.EqualTo(20),
				"SETUP check - expected Russian reference text to have have the start of Mat 2:20 included in block for v. 19.");

			var refTextBlockForMat2V20 = refTextMat.GetBlocksForVerse(2, 20).Last();
			Assert.That(refTextBlockForMat2V20.CharacterId, Is.EqualTo("angel"),
				"SETUP check - expected Russian reference text to have angel speak in block for Mat 2:20.");
			Assert.That(refTextBlockForMat2V20.LastVerseNum, Is.EqualTo(20),
				"SETUP check - expected Russian reference text to have have a block break between Mat 2:20 and v. 21.");

			var matchup = GetBlockMatchupForMat2V20(includeClosingHeSaidInReportingClauses, refTextBlockForMat2V20, refText);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].MatchesReferenceText, Is.EqualTo(includeClosingHeSaidInReportingClauses));

			Assert.That(result[0].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlockForMat2V19.GetText(true)),
				"Expected the narrator block in the reference text for Mat 2:19 to be matched to the first block of matchup.");

			Assert.That(result[1].ReferenceBlocks.Single().GetText(true), Is.EqualTo(refTextBlockForMat2V20.GetText(true)),
				"Expected the block (angel) in the reference text for Mat 2:20 to be matched to the third block of matchup.");
		}
		#endregion

		#region PG-1431
		[TestCase(ReferenceTextType.English)]
		[TestCase(ReferenceTextType.Russian)]
		public void GetBlocksForVerseMatchedToReferenceText_ReportingClausesInRefTextAppearBetweenSpeakingPartsInVern_RefTextReportingClausesCombinedToMatchOnlyNarratorBlock(
			ReferenceTextType refTextType)
		{
			const string adulteress = "woman, caught in adultery";
			var refText = GetStandardReferenceText(refTextType);
			var refTextJhn = refText.GetBook("JHN");
			var refTextBlocksJhn8V11 = refTextJhn.GetBlocksForVerse(8, 11).ToList();
			Assert.That(refTextBlocksJhn8V11.Count, Is.EqualTo(4),
				"SETUP check - unexpected number of reference blocks for John 8:11.");
			Assert.That(refTextBlocksJhn8V11[1].CharacterId,
				Is.EqualTo(adulteress),
				$"SETUP check - expected {refTextType} reference text to have woman speak in second block for John 8:11.");
			Assert.That(refTextBlocksJhn8V11[3].CharacterId, Is.EqualTo("Jesus"),
				$"SETUP check - expected {refTextType} reference text to have Jesus speak in last block for John 8:11.");

			var vernacularBlocks = new List<Block> { CreateBlockForVerse(kAmbiguousCharacter, 11, "“Ondaꞌidun, Polopanad,” ", true, 8) };
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "tinumabal og glibun koyon. Miktaluꞌ si Isus, ", "JHN");
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter, "“Akon siꞌoy ondiꞌu ika ukumon. Uliꞌa na bu naꞌa na mokpuliꞌ moginang nog dusa.”");
			var vernBook = new BookScript("JHN", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);

			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText, Is.False);
			Assert.That(result[2].MatchesReferenceText);
			var womanRefBlock = result[0].ReferenceBlocks.Single();
			Assert.That(womanRefBlock.StartsAtVerseStart, Is.True);
			Assert.That(result[0].InitialStartVerseNumber, Is.EqualTo(womanRefBlock.InitialStartVerseNumber));
			Assert.That(womanRefBlock.GetText(false), Is.EqualTo(
				refTextBlocksJhn8V11.Single(b => b.CharacterId == adulteress).GetText(false)),
				$"Expected the second block (woman in adultery) in the {refTextType} reference text for" +
				" John 8:11 to be matched to the first block of matchup.");
			if (refText.HasSecondaryReferenceText)
			{
				womanRefBlock = womanRefBlock.ReferenceBlocks.Single();
				Assert.That(womanRefBlock.StartsAtVerseStart, Is.True);
				Assert.That(result[0].InitialStartVerseNumber, Is.EqualTo(womanRefBlock.InitialStartVerseNumber));
				Assert.That(womanRefBlock.GetText(false), Is.EqualTo(
					refTextBlocksJhn8V11.Single(b => b.CharacterId == adulteress).GetPrimaryReferenceText(true)),
					$"Expected the second block (woman in adultery) in the {refTextType} reference text for" +
					" John 8:11 to be matched to the first block of matchup.");
			}
			Assert.That(result[1].ReferenceBlocks.Count, Is.EqualTo(2));
			Assert.That(result[1].ReferenceBlocks[0].GetText(true),
				Is.EqualTo(refTextBlocksJhn8V11.First(b => !b.IsQuote).GetText(false)),
				"Expected the first reporting clause in the reference text for John 8:11 to be the " +
				"first reference block for the combined reporting clause block in the vernacular.");
			Assert.That(result[1].ReferenceBlocks[1].GetText(true),
				Is.EqualTo(refTextBlocksJhn8V11.Last(b => !b.IsQuote).GetText(false)),
				"Expected the last reporting clause in the reference text for John 8:11 to be the " +
				"last reference block for the combined reporting clause block in the vernacular.");
			Assert.That(result[2].ReferenceBlocks.Single().GetText(false),
				Is.EqualTo(refTextBlocksJhn8V11.Single(b => b.CharacterId == "Jesus").GetText(true)),
				$"Expected the \"Jesus\" block in the {refTextType} reference text for" +
				" John 8:11 to be matched to the last block of matchup.");
		}
		#endregion

		#region PG-1408
		[TestCase()]
		[TestCase("– диэн көрдөспүтэ")]
		public void GetBlocksForVerseMatchedToReferenceText_MultipleReportingClausesInOppositeOrder_NoReferenceTextIsLost(
			params string[] knownHeSaids)
		{
			var refText = GetStandardReferenceText(ReferenceTextType.English);
			var refTextMat = refText.GetBook("MAT");
			var refTextBlocksMat15V34 = refTextMat.GetBlocksForVerse(15, 34).ToList();
			Assert.That(refTextBlocksMat15V34.Count, Is.EqualTo(4),
				"SETUP check - unexpected number of reference blocks for Matthew 15:34.");
			Assert.That(refTextBlocksMat15V34[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for Matthew 15:34.");
			Assert.That(refTextBlocksMat15V34[3].CharacterId, Is.EqualTo("disciples"),
				"SETUP check - expected English reference text to have disciples speak in last block for Matthew 15:34.");
			
			var refTextBlocksMat15V35 = refTextMat.GetBlocksForVerse(15, 35).ToList();
			Assert.That(refTextBlocksMat15V35.Count, Is.EqualTo(2),
				"SETUP check - unexpected number of reference blocks for Matthew 15:35.");
			Assert.That(refTextBlocksMat15V35[1].CharacterId, Is.EqualTo("Jesus"),
				"SETUP check - expected English reference text to have Jesus speak in second block for Matthew 15:35.");

			for (int v = 36; v <= 39; v++)
			{
				Assert.That(refTextMat.GetBlocksForVerse(15, v).Single()
						.CharacterIs("MAT", StandardCharacter.Narrator), Is.True,
					$"SETUP check - expected single narrator block in English reference text for Matthew 15:{v}");
			}

			var vernacularBlocks = new List<Block>
			{
				CreateBlockForVerse(kAmbiguousCharacter, 34,
					"«Эһиги төһө килиэптээххитий?»", true, 15)
			};
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "– Иисус ыйыппыта.");
			vernacularBlocks.Last().IsParagraphStart = true;
			AddBlockForVerseInProgress(vernacularBlocks, kAmbiguousCharacter,
				"«Сэттэ килиэби кытта аҕыйах кыра балык баар»,");
			vernacularBlocks.Last().IsParagraphStart = true;
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "– диэбиттэрэ. ")
				.AddVerse(35, "Кини дьону сиргэ олордубута. ")
				.AddVerse(36, "Онтон сэттэ килиэби уонна балыгы ылан, үөрэнээччилэрэ дьоҥҥо үллэрбиттэрэ. ")
				.AddVerse(37, "Бары тото аһаабыттара; хаалбыт астара сэттэ толору корзина буолбута. ")
				.AddVerse(38, "Манна оҕону-дьахтары таһынан барыта түөрт тыһыынча эр киһи аһаабыта. ")
				.AddVerse(39, "Онтон дьону ыыталаан баран, Иисус оҥочоҕо олорон, Магдала сиригэр устубута.");
			vernacularBlocks.Last().IsParagraphStart = true;
			var vernBook = new BookScript("MAT", vernacularBlocks, refText.Versification);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, knownHeSaids);

			var result = matchup.CorrelatedBlocks;
			Assert.That(result.Count, Is.EqualTo(9));

			var refBlocksAttachedToVern = result.SelectMany(vb => vb.ReferenceBlocks)
				.Select(rb => rb.GetText(false)).ToList();

			for (int v = 34; v <= 39; v++)
			{
				foreach (var rb in refTextMat.GetBlocksForVerse(15, v))
				{
					var text = rb.GetText(false);
					Assert.That(refBlocksAttachedToVern.Where(t => t.Contains(text)), Is.Not.Empty,
						$"Reference text from verse {v} is missing: {text}");
				}
			}
		}
		#endregion

		#region PG-1458
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_ErrantRefTextBlockMatchesBlockForPrecedingVerseWithUnexpecteSpeaker_TreatedAsMismatch()
		{
			// This is a test for a scenario where a reference text (not English) has a block whose speaker is
			// expected to speak in a preceding verse but (according to the CV file) NOT for the following verse.
			// This is actually an error in the reference text but because Glyssen has no control over custom
			// reference texts, the logic in ReferenceText cannot assume that the reference text will be correct.
			// Unfortunately, there is a slight discrepancy in the logic when matching up "BlockMatchup" objects
			// for ISP vs. hooking up everything for export. So we need to ensure that in this scenario the
			// link to the reference block is not actually considered a "match" so the scripter will have a chance
			// to look at it and figure out what to do.
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(29, "De o ngo Maria duru mataoko itiai ma malaekata ai gea. ", book:"LUK")
					.AddVerse(30, "Masara o Gabiriele wotemoli munangika wato, ")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "Gabriel", "“Maria, ngaro ufa nohawana ngonaika hiadono ngona. ")
				.AddVerse(31, "Abeika ahi demo tonihingahu ngonaika! Abe ani ngofaka o nauru de Una nihiromanga o Yesusu. ")
				.AddVerse("32-33", "De ani ngofaka gea Una ai Ngofaka. De nanga Baluhu Ma Jou wihidadi nia koano" +
					"Una ai tofora o Dautu o Isiraele ma nyawa. Ho gea wodadi ti ngini o Yakubu. De ka Unangohi kuahaka!”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(34, "De o ngo Maria mopaluhu o malaekatika mato, ", book: "LUK"));
			AddBlockForVerseInProgress(vernacularBlocks, "Mary, Jesus' mother", "“Masara ngohi ko ma moiuahi tamake o ngofaka gea?” ");

			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			var refBlock = CreateNarratorBlockForVerse(29, "Maria terkejut mendengar perkataan itu, apakah arti salam itu.", book:"LUK")
				.AddVerse(30, "Kata malaikat itu kepadanya:");
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateNarratorBlockForVerse(29, "But she was troubled, wondering what kind of greeting this was.", book:"LUK")
				.AddVerse(30, "The angel said to her,"));
			refBlock = AddBlockForVerseInProgress(referenceBlocks, "Gabriel", "«Jangan takut, sebab engkau di hadapan Allah.");
			refBlock.SetMatchedReferenceBlock("«Don’t be afraid, Mary, for you have found favor with God.");
			refBlock = CreateBlockForVerse("Gabriel", 31, "Sesungguhnya engkau akan mengandung dan hendaklah engkau menamai Dia Yesus.");
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateBlockForVerse("Gabriel", 31, "You will have a son. Name him ‹Jesus.›"));
			refBlock = CreateBlockForVerse("Gabriel", 32, "Ia akan menjadi Anak Allah Yang Mahatinggi. Dan Tuhan Allah akan takhta Daud, leluhur-Nya.");
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateBlockForVerse("Gabriel", 32, "He will be the Son of God and will reign in place of David."));
			refBlock = CreateBlockForVerse("Gabriel", 33, "La akan menjadi raja atas kaum Kerajaan-Nya tidak akan berkesudahan.")
				.AddVerse(34, "Bagaimana hal itu mungkin terjadi,» "); // Note: this is the part that is incorrect. This should be Mary.
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateBlockForVerse("Gabriel", 33, "He will rule Jacob forever with no end to his Kingdom.» "));
			refBlock = AddNarratorBlockForVerseInProgress(referenceBlocks, "Kata Maria kepada malaikat itu:", "LUK");
			refBlock.SetMatchedReferenceBlock(CreateNarratorBlockForVerse(34, "Mary said to the angel,", book:"LUK"));
			refBlock = AddBlockForVerseInProgress(referenceBlocks, "Mary, Jesus' mother", "«karena aku belum bersuami?» ");
			refBlock.SetMatchedReferenceBlock("«How can this be, since I am a virgin?»");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.Custom);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(6));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(result[1].MatchesReferenceText);
			Assert.That(result[2].MatchesReferenceText);
			Assert.That(result[3].MatchesReferenceText, Is.False);
			Assert.That(result[4].MatchesReferenceText);
			Assert.That(result[5].MatchesReferenceText);
			Assert.That(result, ForEvery<Block>(b =>
				b.ReferenceBlocks.All(ind => ind.MatchesReferenceText), Is.True));
		}
		#endregion

		#region PG-1459
		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_ErrantRefTextBlockWithMisspelledSpeaker_TextIsNotLost()
		{
			// This is a test for a scenario where a reference text (not English) has an outdated biblical character
			// no longer in the control file. Because Glyssen has no control over custom reference texts, this
			// should not result in lost text.
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(8, "Zach stands and declares: ", false, 19, "LUK") };
			AddBlockForVerseInProgress(vernacularBlocks, "Zacchaeus", "“Lord Jesus, I am done with my life of crime!”");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "he said.");

			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			var refBlock = CreateNarratorBlockForVerse(8,
				"Tetapi Zakheus berdiri dan berkata kepada Tuhan:", true, 19, "LUK");
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateNarratorBlockForVerse(29,
					"Zacchaeus stood and said to the Lord,", true, 19, "LUK"));
			refBlock = AddBlockForVerseInProgress(referenceBlocks, "Zachary",
				"“Tuhan, setengah dari seseorang akan empat kali lipat.”");
			refBlock.SetMatchedReferenceBlock("“Lord, I give half to the poor. I will repay stolen stuff fourfold.”");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks, ReferenceTextType.Custom);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(3));
			Assert.That(result[0].MatchesReferenceText);
			Assert.That(!result[1].MatchesReferenceText || !result[2].MatchesReferenceText);
			Assert.That(result.Skip(1).SelectMany(s => s.ReferenceBlocks).Single().GetText(true),
				Is.EqualTo(referenceBlocks.Last().GetText(true)));
			Assert.That(result, ForEvery<Block>(b => b.ReferenceBlocks.All(ind => ind.MatchesReferenceText), Is.True));
		}

		[TestCase()]
		[TestCase("—jai.")]
		[TestCase("—jai fariseowi.", "—jai.", "—jaibeje.")]
		[TestCase("—jai fariseowi.", "—jaibeje.")]
		public void GetBlocksForVerseMatchedToReferenceText_KnownTrailingHeSaidAfterMiddleMismatchedBlock_TextOfRefTextIsNotLost(params string[] knownHeSaids)
		{
			// This test was written for a scenario where a reference text (not English) has an
			// outdated biblical character ID (different from the control file). This was actually
			// an error in the reference text but because Glyssen has no control over custom
			// reference texts, this kind of error should not result in lost text. For this
			// scenario, two of the four test cases failed.
			// However, also note that the fix for this also fixed several other real-life
			// scenarios that were not caused by problems in the reference text.
			var vernacularBlocks = new List<Block>
			{
				CreateNarratorBlockForVerse(5,
					"Itsamatacabi, Jesús pija-apóstolewi jumaitsi Jesús-jawabelia:", false, 17, "LUK")
			};
			AddBlockForVerseInProgress(vernacularBlocks, "disciples",
				"—¡Patajatuxanenë, paneyawenonare patajamatabëcuenewitsabinexa pata-itaxutuajamatejemajawa Diosojawabelia! ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "—jai.");

			var vernBook = new BookScript("LUK", vernacularBlocks, m_vernVersification);

			var referenceBlocks = new List<Block>();
			var refBlock = CreateNarratorBlockForVerse(5,
				"Los apóstoles le pidieron al Señor:", true, 17, "LUK");
			referenceBlocks.Add(refBlock);
			refBlock.SetMatchedReferenceBlock(CreateNarratorBlockForVerse(5,
				"The apostles said to the Lord,", true, 17, "LUK"));
			refBlock = AddBlockForVerseInProgress(referenceBlocks, "apostles",
				"“Aumente nuestro fe.”");
			refBlock.SetMatchedReferenceBlock("“Increase our faith.”");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, knownHeSaids);
			var result = matchup.CorrelatedBlocks;

			Assert.That(result.Count, Is.EqualTo(3));
			var attachedRefTexts = result.SelectMany(r => r.ReferenceBlocks).Select(b => b.GetText(false)).ToList();
			Assert.That(referenceBlocks,
				ForEvery<Block>(b => attachedRefTexts.Contains(b.GetText(false)), Is.True,
				"text found in attached reference texts"));
		}
		#endregion

		#region private helper methods
		private Block NewChapterBlock(string bookId, int chapterNum, string text = null)
		{
			var block = new Block("c", chapterNum)
			{
				BookCode = bookId,
				IsParagraphStart = true,
				CharacterId = GetStandardCharacterId(bookId, StandardCharacter.BookOrChapter)
			};
			block.BlockElements.Add(new ScriptText(text ?? chapterNum.ToString()));
			return block;
		}

		internal static Block CreateBlockForVerse(string characterId, int initialStartVerseNumber, string text, bool paraStart = false,
			int chapter = 1, string styleTag = "p", int initialEndVerseNumber = 0)
		{
			var block = new Block(styleTag, chapter, initialStartVerseNumber, initialEndVerseNumber)
			{
				IsParagraphStart = paraStart,
				CharacterId = characterId,
			};
			if (initialEndVerseNumber > 0)
				block.AddVerse(Format("{0}-{1}", initialStartVerseNumber, initialEndVerseNumber), text);
			else
				block.AddVerse(initialStartVerseNumber, text);
			return block;
		}

		internal static Block AddBlockForVerseInProgress(IList<Block> list, string characterId, string text, string styleTag = "")
		{
			var lastBlock = list.Last();
			var initialStartVerse = lastBlock.InitialStartVerseNumber;
			var initialEndVerse = lastBlock.InitialEndVerseNumber;
			var lastVerseElement = lastBlock.BlockElements.OfType<Verse>().LastOrDefault();
			if (lastVerseElement != null)
			{
				initialStartVerse = BCVRef.VerseToIntStart(lastVerseElement.Number);
				initialEndVerse = BCVRef.VerseToIntEnd(lastVerseElement.Number);
			}

			if (IsNullOrEmpty(styleTag))
				styleTag = lastBlock.StyleTag;

			var block = new Block(styleTag, lastBlock.ChapterNumber, initialStartVerse, initialEndVerse)
			{
				CharacterId = characterId,
				IsParagraphStart = styleTag != lastBlock.StyleTag,
			};
			block.BlockElements.Add(new ScriptText(text));
			list.Add(block);
			return block;
		}

		internal static Block AddNarratorBlockForVerseInProgress(IList<Block> list, string text, string book = "MAT")
		{
			return AddBlockForVerseInProgress(list, GetStandardCharacterId(book, StandardCharacter.Narrator), text);
		}

		internal static Block CreateNarratorBlockForVerse(int verseNumber, string text, bool paraStart = false, int chapter = 1,
			string book = "MAT", string styleTag = "p", int initialEndVerseNumber = 0)
		{
			return CreateBlockForVerse(GetStandardCharacterId(book, StandardCharacter.Narrator),
				verseNumber, text, paraStart, chapter, styleTag, initialEndVerseNumber);
		}

		/// <summary>
		/// Gets a block that has both LuUK 23:17 and the start of v. 18, and also (as an output param) the
		/// block containing the end of v. 18.
		/// In v. 112 of the DG, these two verses were split out into separate blocks. Not sure
		/// if there are other places where this could reasonably occur, but as least for now, I'm
		/// going to leave the tests cases (and the production code) for this. So this method modifies
		/// the built-in reference text to be the way it was in previous versions of the DG.
		/// </summary>
		/// <returns></returns>
		private Block GetLuke23v17And18Block(ReferenceText englishReferenceText, out Block lastBlockForV18, out Action cleanup)
		{
			const string kLukBookId = "LUK";
			var luk = englishReferenceText.Books.Single(b => b.BookId == kLukBookId);
			var iLuk23v17 = luk.GetIndexOfFirstBlockForVerse(23, 17);
			var v17And18Block = luk[iLuk23v17];
			var v18RefTextBlocks = luk.GetBlocksForVerse(23, 18).ToList();
			lastBlockForV18 = v18RefTextBlocks.Last();
			Assert.That(v18RefTextBlocks.Count, Is.EqualTo(2), "Test setup conditions not met");
			if (v17And18Block != v18RefTextBlocks.First())
			{
				// This is a semi-dangerous hack. Since we can't get the lock, this isn't thread-safe.
				cleanup = () =>
				{
					var modifiedBooks = (HashSet<string>)ReflectionHelper.GetField(englishReferenceText, "m_modifiedBooks");
					modifiedBooks.Add(kLukBookId);
					ReflectionHelper.CallMethod(englishReferenceText, "ReloadModifiedBooks");
				};
				v17And18Block = v18RefTextBlocks.First(); 
				v17And18Block.BlockElements.InsertRange(0, luk[iLuk23v17].BlockElements);
				v17And18Block.InitialStartVerseNumber = 17;
				luk.Blocks.RemoveAt(iLuk23v17);
				ReflectionHelper.CallMethod(luk, "OnBlocksReset");
			}
			else
			{
				cleanup = null;
			}

			Assert.That(v17And18Block.InitialStartVerseNumber, Is.EqualTo(17),
				"Test setup conditions not met");
			Assert.That(((Verse)v17And18Block.BlockElements[2]).Number, Is.EqualTo("18"),
				"Test setup conditions not met");

			return v17And18Block;
		}
		#endregion
	}

	public class TestReferenceText : ReferenceText
	{
		private TestReferenceText(GlyssenDblTextMetadata metadata, BookScript book, ReferenceTextType type = ReferenceTextType.Custom)
			: base(metadata, type)
		{
			if (Versification != null && book.Versification == null)
				book.Initialize(Versification);

			m_books.Add(book);
		}

		protected override void SetVersification()
		{
			Debug.Assert(m_referenceTextType == ReferenceTextType.Custom);
			SetVersification(ScrVers.English);
			foreach (var book in m_books.Where(b => b.Versification == null))
				book.Initialize(Versification);
		}

		private static GlyssenDblTextMetadata NewMetadata
		{
			get
			{
				var metadata = new GlyssenDblTextMetadata
				{
					Language = new GlyssenDblMetadataLanguage { Iso = "~tst~", Name = "Test Language" }
				};
				return metadata;
			}
		}

		public static TestReferenceText CreateTestReferenceText(string bookId, IList<Block> blocks, ReferenceTextType type = ReferenceTextType.Custom)
		{
			return new TestReferenceText(NewMetadata, new BookScript(bookId, blocks, ScrVers.English), type) { GetBookName = b => "The Gospel According to Thomas" };
		}

		public static TestReferenceText CreateTestReferenceText(string bookScriptXml)
		{
			return new TestReferenceText(NewMetadata, XmlSerializationHelper.DeserializeFromString<BookScript>(bookScriptXml));
		}

		public static void ForgetCustomReferenceTexts()
		{
			var impl = Project.Writer as PersistenceImplementation;
			impl?.ForgetCustomReferenceTexts();
		}

		public static ReferenceText CreateCustomReferenceText(params TestReferenceTextResource[] booksToInclude)
		{
			Assert.That(Project.Writer is PersistenceImplementation);
			var customLanguageId = ReferenceTextTestUtils.CreateCustomReferenceText((IProjectPersistenceWriter)ReferenceTextProxy.Reader, booksToInclude);
			return GetReferenceText(ReferenceTextProxy.GetOrCreate(ReferenceTextType.Custom, customLanguageId));
		}
	}
}
