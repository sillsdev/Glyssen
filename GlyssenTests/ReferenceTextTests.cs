using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen;
using Glyssen.Bundle;
using Glyssen.Character;
using GlyssenTests.Properties;
using NUnit.Framework;
using Paratext;
using SIL.IO;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Windows.Forms;
using SIL.Xml;
using static System.String;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests
{
	[TestFixture]
	class ReferenceTextTests
	{
		private ScrVers m_vernVersification;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_vernVersification = Versification.Table.Load(tempFile.Path);
			}
		}

		[TearDown]
		public void Teardown()
		{
			TestReferenceText.DeleteTempCustomReferenceProjectFolder();
		}

		[TestCase(ReferenceTextType.English)]
		//[TestCase(ReferenceTextType.Azeri)]
		//[TestCase(ReferenceTextType.French)]
		//[TestCase(ReferenceTextType.Indonesian)]
		//[TestCase(ReferenceTextType.Portuguese)]
		[TestCase(ReferenceTextType.Russian)]
		//[TestCase(ReferenceTextType.Spanish)]
		//[TestCase(ReferenceTextType.TokPisin)]
		public void GetStandardReferenceText_AllStandardReferenceTextsAreLoadedCorrectly(ReferenceTextType referenceTextType)
		{
			var referenceText = ReferenceText.GetStandardReferenceText(referenceTextType);
			Assert.AreEqual(27, referenceText.Books.Count); //Only NT so far. Hopefully soon, it will include the OT also.
			Assert.AreEqual(ScrVers.English, referenceText.Versification);
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
			var vernBook = new BookScript("MAT", vernacularBlocks);
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

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(3, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result.All(b => b.CharacterId == "Peter/James/John"));
			Assert.IsTrue(result.All(b => b.CharacterIdInScript == "John"));
			Assert.IsTrue(result.All(b => b.Delivery == "annoyed beyond belief"));
			Assert.AreEqual(expectedResultForFirstBlock, result.First().MultiBlockQuote);
			Assert.IsTrue(result.Skip(1).All(b => b.MultiBlockQuote == expectedResultForSubsequentBlocks));
			Assert.IsTrue(result.All(b => b.UserConfirmed));
			Assert.IsTrue(result.All(b => b.SplitId == -1));
			Assert.IsTrue(result.Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))));
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
			var vernBook = new BookScript("MAT", vernacularBlocks);
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

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(3, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result.All(b => b.CharacterId == "Peter/James/John"));
			Assert.IsTrue(result.All(b => b.CharacterIdInScript == "John"));
			Assert.IsTrue(result.All(b => b.Delivery == "annoyed beyond belief"));
			Assert.AreEqual(expectedResultForFirstBlock, result.First().MultiBlockQuote);
			Assert.IsTrue(result.Skip(1).All(b => b.MultiBlockQuote == expectedResultForSubsequentBlocks));
			Assert.IsTrue(result.All(b => b.UserConfirmed));
			Assert.IsTrue(result.All(b => b.SplitId == -1));
			Assert.IsTrue(result.Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseSplitIntoMoreBlocksThanReference_ReferenceTextBrokenByVerse_SubsequentBlocksInVernacularGetBrokenByVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Fred. ");
			var block = CreateNarratorBlockForVerse(2, "Blah blah. ");
			block.AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "I don't know if Fred told you this or not, but he's crazy. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "This is your narrator speaking. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "I hope you enjoy your flight. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "The end. "));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(4, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("{1}\u00A0Cosas que Fred dice, ", result[0].GetText(true));
			Assert.AreEqual(0, result[0].ReferenceBlocks.Count);

			Assert.AreEqual("dijo Fred. ", result[1].GetText(true));
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[1].ReferenceBlocks.Single().GetText(true));

			Assert.AreEqual("{2}\u00A0Blah blah. ", result[2].GetText(true));
			Assert.AreEqual("{3}\u00A0More blah blah. ", result[3].GetText(true));
			Assert.AreEqual("{4}\u00A0The final blah blah.", result[4].GetText(true));
			Assert.IsTrue(result.Skip(2).Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))));
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceTextHaveSectionHeadsInDifferentPlaces_SectionHeadReferenceTextsAreNotMatched()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true));
			var block = new Block("s", 1, 1)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Section cabeza text"));
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 2)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
			};
			block.AddVerse(2, "Blah blah. ").AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "I don't know if Fred told you this or not, but he's crazy. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "This is your narrator speaking. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "I hope you enjoy your flight. "));
			block = new Block("s", 1, 3)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Section head text (the English version)"));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "The end."));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(5, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("{1}\u00A0Cosas que Fred dice, ", result[0].GetText(true));
			Assert.AreEqual("Section cabeza text", result[1].GetText(true));
			Assert.AreEqual("{2}\u00A0Blah blah. ", result[2].GetText(true));
			Assert.AreEqual("{3}\u00A0More blah blah. ", result[3].GetText(true));
			Assert.AreEqual("{4}\u00A0The final blah blah.", result[4].GetText(true));
			Assert.AreEqual("{1}\u00A0I don't know if Fred told you this or not, but he's crazy. ", result[0].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual(0, result[1].ReferenceBlocks.Count);
			Assert.AreEqual("{2}\u00A0This is your narrator speaking. ", result[2].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual("{3}\u00A0I hope you enjoy your flight. ", result[3].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual("{4}\u00A0The end.", result[4].ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void ApplyTo_VernacularHasSectionHeadInTheMiddleOfAVerse_ReferenceTextHasNoSectionHead_NotMatchedAndReferenceTextLinkedToFirstVernacularBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(31, "But eagerly desire the greater gifts.", false, 12, "1CO"));
			var block = new Block("s", 12, 31)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("1CO", CharacterVerseData.StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("Love"));
			vernacularBlocks.Add(block);
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "And now I will show you...", "1CO").AddVerse(32, "This isn't here.");
			var vernBook = new BookScript("1CO", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(31, "In this version, there is no section head.", false, 12, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(32, "The verse that was never supposed to exist.", false, 12, "1CO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(2, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual(referenceBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual("{31}\u00A0But eagerly desire the greater gifts.", result[0].GetText(true));
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);

			Assert.AreEqual("Love", result[1].GetText(true));
			Assert.AreEqual(0, result[1].ReferenceBlocks.Count);
			Assert.IsFalse(result[1].MatchesReferenceText);
			Assert.IsNull(result[1].PrimaryReferenceText);

			Assert.AreEqual("And now I will show you...", result[2].GetText(true));
			Assert.AreEqual(0, result[2].ReferenceBlocks.Count);
			Assert.IsFalse(result[2].MatchesReferenceText);
			Assert.IsNull(result[2].PrimaryReferenceText);

			Assert.AreEqual("{32}\u00A0This isn't here.", result[3].GetText(true));
			Assert.AreEqual("{32}\u00A0The verse that was never supposed to exist.", result[3].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[3].MatchesReferenceText);
			Assert.AreEqual("{32}\u00A0The verse that was never supposed to exist.", result[3].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasMidVerseParagraphBreakFollowedByMoreVerses_ReferenceHasBlocksSplitAtVerseBreaks_AdditionalVerseSplitsHappen()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(31, "But eagerly desire the greater gifts.", false, 12, "1CO"));
			var blockForNewParagraph = AddNarratorBlockForVerseInProgress(vernacularBlocks, "And now I will show you...", "1CO").AddVerse(32, "This isn't here.");
			blockForNewParagraph.IsParagraphStart = true;
			var vernBook = new BookScript("1CO", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(31, "In this version, there is no paragraph break.", false, 12, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(32, "The verse that was never supposed to exist.", false, 12, "1CO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(2, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("{31}\u00A0But eagerly desire the greater gifts.", result[0].GetText(true));
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);

			Assert.AreEqual("And now I will show you...", result[1].GetText(true));
			Assert.AreEqual(0, result[1].ReferenceBlocks.Count);
			Assert.IsFalse(result[1].MatchesReferenceText);
			Assert.IsNull(result[1].PrimaryReferenceText);

			Assert.AreEqual("{32}\u00A0This isn't here.", result[2].GetText(true));
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_MultipleBlocksOfVernacularWithTwoCharacters_ReferenceTextHasSameCharacters_PrimaryReferenceTextSetCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
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

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(2, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator), result[0].CharacterId);
			Assert.AreEqual("Peter/James/John", result[1].CharacterId);
			Assert.AreEqual("John", result[1].CharacterIdInScript);
			Assert.IsTrue(result.All(b => !b.UserConfirmed));
			Assert.IsTrue(result.All(b => b.SplitId == -1));
			Assert.IsTrue(result.Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))));
		}

		[Test]
		public void ApplyTo_MultipleBlocksOfVernacular_NoConflictsInReferenceText_ReferenceTextAppliedCorrectly()
		{
			Block.FormatChapterAnnouncement = (s, i) => s + i;
			var vernacularBlocks = new List<Block>();
			var block = new Block("mt", 1)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("El Evangelio Segun San Mateo"));
			vernacularBlocks.Add(block);
			block = new Block("c", 1)
			{
				BookCode = "MAT",
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("1"));
			vernacularBlocks.Add(block);
			vernacularBlocks.Add(CreateBlockForVerse("Paul", 1, "This is versiculo uno.", true)
				.AddVerse(2, "This is versiculo dos.")
				.AddVerse(3, "This is versiculo tres."));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(4, "Now the narrator butts in."));
			block = new Block("c", 2)
			{
				BookCode = "MAT",
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("2"));
			vernacularBlocks.Add(block);
			block = new Block("s", 2)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("This is una historia about a scruffy robot jugando volleybol"));
			vernacularBlocks.Add(block);
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "El robot agarro la pelota.", true, 2, "MAT", "q"));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			block = new Block("mt", 1)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("The Gospel According to Saint Thomas"));
			referenceBlocks.Add(block);
			block = new Block("c", 1)
			{
				BookCode = "MAT",
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			referenceBlocks.Add(block);
			block.BlockElements.Add(new ScriptText("1"));
			referenceBlocks.Add(CreateBlockForVerse("Paul", 1, "This is verse one.", true));
			referenceBlocks.Add(CreateBlockForVerse("Paul", 2, "This is verse two.", true));
			referenceBlocks.Add(CreateBlockForVerse("Paul", 3, "This is verse three.", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Now the narrator butts in."));
			block = new Block("c", 2)
			{
				BookCode = "MAT",
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter),
			};
			block.BlockElements.Add(new ScriptText("2"));
			referenceBlocks.Add(block);
			block = new Block("s", 2)
			{
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical),
			};
			block.BlockElements.Add(new ScriptText("This is a story about a scruffy robot playing volleyball"));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "The robot grabbed the ball.", true, 2, "MAT", "q"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(referenceBlocks.Count, result.Count);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter), result[0].CharacterId);
			Assert.AreEqual("The Gospel According to Saint Thomas", result[0].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter), result[1].CharacterId);
			Assert.AreEqual("The Gospel According to Thomas 1", result[1].PrimaryReferenceText);
			Assert.AreEqual("Paul", result[2].CharacterId);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[2].PrimaryReferenceText);
			Assert.AreEqual("Paul", result[3].CharacterId);
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[3].PrimaryReferenceText);
			Assert.AreEqual("Paul", result[4].CharacterId);
			Assert.AreEqual(referenceBlocks[4].GetText(true), result[4].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator), result[5].CharacterId);
			Assert.AreEqual(referenceBlocks[5].GetText(true), result[5].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.BookOrChapter), result[6].CharacterId);
			Assert.AreEqual("The Gospel According to Thomas 2", result[6].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical), result[7].CharacterId);
			Assert.AreEqual(referenceBlocks[7].GetText(true), result[7].PrimaryReferenceText);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator), result[8].CharacterId);
			Assert.AreEqual(referenceBlocks[8].GetText(true), result[8].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasExtraTrailingNarratorBlock_AllBlocksMatchExceptTrailingNarratorBlock()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo uno, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo. Pero Paul replico: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "Asi pense, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo.");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“This is verse one.” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But Paul replied, ");
			AddBlockForVerseInProgress(referenceBlocks, "Paul", "“That's what I thought.”");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			for (int i = 0; i < referenceBlocks.Count; i++)
			{
				var desc = "Block " + i + ": " + result[i].GetText(true);
				Assert.AreEqual(vernacularBlocks[i].CharacterId, result[i].CharacterId, desc);
				Assert.AreEqual(referenceBlocks[i].GetText(true), result[i].ReferenceBlocks.Single().GetText(true), desc);
				Assert.AreEqual(referenceBlocks[i].GetText(true), result[i].PrimaryReferenceText, desc);
				Assert.IsTrue(result[i].MatchesReferenceText, desc);
			}
			
			Assert.AreEqual(vernacularBlocks.Last().CharacterId, result.Last().CharacterId);
			Assert.IsFalse(result.Last().ReferenceBlocks.Any());
			Assert.IsFalse(result.Last().MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_NoConflictsInReferenceText_ReferenceTextAppliedCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
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
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new ScriptText("Y Pablo respondio diciendo, "));
			vernacularBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Paul"
			};
			block.BlockElements.Add(new ScriptText("Quien eres Senor? Pedro?"));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
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
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new ScriptText("And Paul responded, "));
			referenceBlocks.Add(block);
			block = new Block("p", 1, 1)
			{
				CharacterId = "Paul"
			};
			block.BlockElements.Add(new ScriptText("Who are you? PETA?"));
			referenceBlocks.Add(block);
			Assert.AreEqual(referenceBlocks.Count, vernacularBlocks.Count); // Sanity check

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(referenceBlocks.Count, result.Count);
			Assert.IsTrue(result.Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Select(r => r.GetText(true))));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridge_ReferenceBrokenAtVerses_ReferenceTextCombinedToMatch()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 3)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new Verse("1-3"));
			block.BlockElements.Add(new ScriptText("Entonces Jesús dijo que los reducirían un burro. El número de ellos dónde encontrarlo. Y todo salió bien."));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "It did."));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsTrue(result.Single().MatchesReferenceText);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(Join("", referenceBlocks.Select(r => r.GetText(true))), result[0].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerseBridgeCorrespondingToTwoVernVerses_Mismatched()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator);

			var vernacularBlocks = new List<Block>
				{
					new Block("p", 1, 1) {CharacterId = narrator }.AddVerse("1", "A"),
					new Block("p", 1, 2) {CharacterId = "Enoch" }.AddVerse("2", "B"),
					new Block("p", 1, 3) {CharacterId = narrator }.AddVerse("3", "C"),
					new Block("p", 1, 4) {CharacterId = "Michael" }.AddVerse("4", "D")
				};
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>
			{
				new Block("p", 1, 1) { CharacterId = narrator }.AddVerse("1", "Ayy"),
				new Block("p", 1, 2, 3) {CharacterId = narrator}.AddVerse("2-3", "Bee Cee"),
				new Block("p", 1, 4) { CharacterId = "Michael" }.AddVerse(4, "Dee, "),
			};

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));

			// We don't especially care how the vern blocks for verses 2 and 3 align to the ref block for 2-3 as long as neither one
			// is considered a match and exactly one of them is hooked up to it.
			Assert.IsFalse(result[1].MatchesReferenceText);
			Assert.IsFalse(result[2].MatchesReferenceText);
			Assert.IsTrue(result.Skip(1).Take(2).SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true))
				.SequenceEqual(referenceBlocks.Skip(1).Take(1).Select(r => r.GetText(true))));

			Assert.AreEqual(referenceBlocks[2].GetText(true), result[3].ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void GetExportData_VernVerseHasMorePartsThanReference_FinalBlockOfVersContainsStartOfFollowingVerseInBothVernAndRef_BeginningAndEndMatch()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);

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
			var vernBook = new BookScript("MAT", vernacularBlocks);

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

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks.Single().GetText(true));
			Assert.IsFalse(result[2].ReferenceBlocks.Any());
			Assert.IsFalse(result[3].ReferenceBlocks.Any());
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[4].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[5].ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeNotAtStartOfBlock_ReferenceBrokenAtVerses_VernacularSplitForNonBridgedVerses()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.AddVerse(1, "Entonces Jesús dijo que los reducirían un burro. ")
				.AddVerse("2-3", "El número de ellos dónde encontrarlo. Y todo salió bien. ")
				.AddVerse(4, "El cuarto versiculo.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "It did. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Fourth verse."));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Jesus told them where to find a donkey. ", result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true), result[1].PrimaryReferenceText);
			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.AreEqual("{4}\u00A0Fourth verse.", result[2].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeAtStartOfBlockFollowedByOtherVerses_ReferenceBrokenAtVerses_VernacularSplitAtEndOfBridgeAndSubsequentVerses()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Jesús les dijo donde encontrar un burro. ", true));
			var block = new Block("p", 1, 2, 3)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.AddVerse("2-3", "El número de ellos dónde encontrarlo. Y todo salió bien. ")
				.AddVerse(4, "El cuarto versiculo.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "It did. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Fourth verse."));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Jesus told them where to find a donkey. ", result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true), result[1].PrimaryReferenceText);
			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.AreEqual("{4}\u00A0Fourth verse.", result[2].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeNotAtStartOfBlock_ReferenceNotBrokenAtStartOfVernacularBridge_NoSplitAndNoErrorReport()
		{
			// PG-746 Chikunda
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(17, "Paadasiya gunyenye ndiye adapita munyumba, wakufundila wake adamubvunza kuti alewe dzvadzvikalewa dzvaalewa. ", true, 7 ,"MRK");
			block.AddVerse("18-19", "Jesu adati kwa iwo, ");
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MRK", vernacularBlocks);

			// Indonesian
			var referenceBlocks = new List<Block>();
			block = CreateNarratorBlockForVerse(17, "Sesudah Ia masuk ke sebuah rumah untuk menyingkir dari orang banyak, murid-murid-Nya bertanya kepada-Nya tentang arti perumpamaan itu. ", true, 7, "MRK");
			block.AddVerse(18, "Maka jawab-Nya:");
			referenceBlocks.Add(block);
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "<<Apakah kamu juga tidak dapat memahaminya? Tidak tahukah kamu bahwa segala sesuatu dari luar yang masuk " +
				"ke dalam seseorang tidak dapat menajiskannya, ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.IsTrue(result.Single().MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true) + " " + referenceBlocks[1].GetText(true),
				result[0].ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void ApplyTo_VerseBridgeInVernacularPreventsSplitToCorrespondToBreakInReferenceText_MatchTheLeadingAndTrailingBlocksAndMismatchMiddleOnes()
		{
			// PG-746 Chikunda (Acts 8:26-29)
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(26, "Ngilozi ... Filipi, ", true, 8, "ACT"));
			AddBlockForVerseInProgress(vernacularBlocks, "angel", "“Konzekela ... Gaza.” ");
			var block = AddNarratorBlockForVerseInProgress(vernacularBlocks, "(Njila ... zino.) ", "ACT");
			block.AddVerse("27-28", "Saka ... Ayizaya. ");
			block.AddVerse(29, "Mzimu ... Filipi, ");
			AddBlockForVerseInProgress(vernacularBlocks, "Holy Spirit, the", "“Yenda ... iyo.” ");
			var vernBook = new BookScript("ACT", vernacularBlocks);

			// Indonesian
			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(26, "Kemudian ..., katanya:", true, 8, "ACT"));
			AddBlockForVerseInProgress(referenceBlocks, "angel", "<<Bangunlah ... Gaza.>>");
			block = AddNarratorBlockForVerseInProgress(referenceBlocks, "Jalan ... sunyi. ", "ACT");
			block.AddVerse(27, "Lalu ... beribadah.");
			referenceBlocks.Add(block = CreateNarratorBlockForVerse(28, "Sekarang ... Yesaya. ", true, 8, "ACT"));
			block.AddVerse(29, "Lalu ... Filipus:");
			AddBlockForVerseInProgress(referenceBlocks, "Holy Spirit, the", "<<Pergilah ... itu!>>");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.True(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));

			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.True(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks[0].GetText(true));

			Assert.AreEqual(2, result[2].ReferenceBlocks.Count);
			Assert.False(result[2].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[2].ReferenceBlocks[1].GetText(true));

			Assert.AreEqual(1, result[3].ReferenceBlocks.Count);
			Assert.True(result[3].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[4].GetText(true), result[3].ReferenceBlocks[0].GetText(true));
		}

		[Test]
		public void ApplyTo_SingleSpeakerVerseBridgeInVernacularCorrespondsToTwoBlocksInReferenceText_CombineReferenceTextBlocksAndMatchToVernBlock()
		{
			// PG-764 Chikunda (I Corinthians 5:3-4)
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId("1CO", CharacterVerseData.StandardCharacter.Narrator), 3,
				"Ndiye, kana dzvangu ndilikutali na imwepo pathupi, ndichilikumweko na imwepo pamzimu. Ndiye ngatindilipo, ndatotonga kale mudzina la Mfumu Jesu padzulu pa mamuna ayita dzvimwedzvi. Apo pamunizagumana, ndinizagumana na imwepo pamzimu, na mphamvu ya Mfumu yathu Jesu alipo na ifepo, ",
				false, 5, "p" , 4));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(5,
				"munifanila kuyikha mamuna umweyu kwa Satana kuti thupi lake liwonongewe, kuti mzimu wake upulumusiwe pa Nsiku ya Mfumu. ",
				false, 5, "1CO"));
			var vernBook = new BookScript("1CO", vernacularBlocks);

			// Indonesian & English reference blocks
			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "Sebab aku, ... itu.", true, 5, "1CO"));
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(3, "For ... thing.", true, 5, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Bilamana ... kita,", true, 5, "1CO"));
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(4, "In ... Christ.", true, 5, "1CO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(5, "orang ... Tuhan.", true, 5, "1CO"));
			referenceBlocks.Last().SetMatchedReferenceBlock(CreateNarratorBlockForVerse(5, "Are to ... Jesus.", true, 5, "1CO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			//using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(vernacularBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.True(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true) + " " + referenceBlocks[1].GetText(true), result[0].ReferenceBlocks[0].GetText(true));

			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.True(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
		}

		/// <summary>
		/// PG-742
		/// </summary>
		[Test]
		public void ApplyTo_MissingVerseNumberInVernacular_DoesNotFail()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1,
				"E mbaŋako iyako, Herod, iye Galili gharambarombaro i loŋweya Jisas le vakatha utuutuniye. ", true, 14)
				.AddVerse(2, "I dage weŋgiya le rakakaiwo e raberabe iŋa, "));
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

#if DEBUG
			using (new ErrorReport.NonFatalErrorReportExpected())
#else
			using (new ErrorReport.NoNonFatalErrorReportExpected())
#endif
				refText.ApplyTo(vernBook, m_vernVersification);
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

			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(50,
				"A particular one of them dudes whacked the slave of the main priest, severing his ear.", true, 22, "LUK"));
			vernacularBlocks.Add(CreateBlockForVerse("Jesus", 51,
				"“What in the world was that all about? They're supposed to get away with this,” ", true, 22));
			AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"reprimanded Jesus. Then He put his hand on his ear and fixed him right up.", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result[0].ReferenceBlocks.Single().BlockElements.OfType<Sound>().Any());
			Assert.IsTrue(result[0].MatchesReferenceText);

			// We don't especially care how the remaining vern blocks align to the ref blocks as long as the first one
			// is not a match, and all of the reference blocks are hooked up (in the correct order) with exactly one vern block.
			var referenceBlocks = refText.Books.Single(b => b.BookId == vernBook.BookId).GetBlocksForVerse(22, 51).ToList();
			Assert.IsTrue(result.Skip(1).SelectMany(v => v.ReferenceBlocks).Select(r => r.GetText(true))
				.SequenceEqual(referenceBlocks.Select(r => r.GetText(true))));
			Assert.IsFalse(result[1].MatchesReferenceText);
		}

		/// <summary>
		/// PG-744
		/// </summary>
		[Test]
		public void ApplyTo_VerseBreakInVernThatIsNotInReferenceText_SoundEffectAtEndOfVerseInReferenceText_ReferenceTextAppliedCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(50,
				"A particular one of them dudes whacked the slave of the main priest, severing his ear.", true, 22, "LUK"));
			vernacularBlocks.Add(CreateBlockForVerse("Jesus", 51,
				"“What in the world was that all about? They're supposed to get away with this,” ", true, 22));
			AddNarratorBlockForVerseInProgress(vernacularBlocks,
				"reprimanded Jesus. Then He put his hand on his ear and fixed him right up.", "LUK");
			var vernBook = new BookScript("LUK", vernacularBlocks);

			var referenceBlocks = new List<Block>();

			var block = CreateNarratorBlockForVerse(50,
				"A certain one of them struck the servant of the high priest, and cut off his right ear. ", true, 22, "LUK");
			block.BlockElements.Add(new Sound { SoundType = SoundType.Sfx, EffectName = "Man crying out", UserSpecifiesLocation = true });
			block.AddVerse(51, "But Jesus answered, ");
			referenceBlocks.Add(block);
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Permit them to seize me.”");
			AddNarratorBlockForVerseInProgress(referenceBlocks,
				"and he touched his ear, and healed him.", "LUK");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			using (new ErrorReport.NoNonFatalErrorReportExpected())
				refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);

			Assert.IsInstanceOf<Sound>(result[0].ReferenceBlocks.Single().BlockElements.Last());
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{50}\u00A0A certain one of them struck the servant of the high priest, and cut off his right ear. {F8 SFX--Man crying out} ", result[0].PrimaryReferenceText);

			// We don't especially care how the remaining vern blocks align to the ref blocks as long as the first one
			// is not a match, and each of the remaining reference blocks is hooked up (in the correct order) to exactly
			// one vern block in the correct order.
			Assert.AreEqual(3, result.Skip(1).SelectMany(v => v.ReferenceBlocks).Count());
			Assert.AreEqual("{51}\u00A0But Jesus answered, ", result.Skip(1).SelectMany(v => v.ReferenceBlocks).First().GetText(true));
			Assert.IsTrue(result.Skip(1).SelectMany(v => v.ReferenceBlocks).Skip(1).Select(r => r.GetText(true))
				.SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))));
			Assert.IsFalse(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasVerseBridgeWithSubVerseLetter_ReferenceBrokenAtVerses_VernacularSplitAtEndOfLastSubVerseChunk()
		{
			var vernacularBlocks = new List<Block>();
			var block = new Block("p", 1, 1, 1)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Jesus told them where to find a donkey. ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "He said that they should bring it, and it would all work out. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "It did. "));
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Fourth verse."));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("{1}\u00A0Entonces Jesús dijo que los reducirían un burro. ", result[0].GetText(true));
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Jesus told them where to find a donkey. ", result[0].PrimaryReferenceText);

			Assert.AreEqual("{2-3a}\u00A0El número de ellos dónde encontrarlo. Y todo salió bien. {3f}\u00A0La segunda parte del versiculo. ",
				result[1].GetText(true));
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true) + referenceBlocks[2].GetText(true), result[1].PrimaryReferenceText);

			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.AreEqual("{4}\u00A0Fourth verse.", result[2].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerseBridge_VernacularBrokenAtEndOfBridge()
		{
			var vernacularBlocks = new List<Block>();
			var block = CreateNarratorBlockForVerse(1, "I gotta go. ");
			block.AddVerse(2, "More blah blah. ").AddVerse(3, "More more blah blah. ").AddVerse(4, "Jesús les dijo dónde encontrar un burro.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			block = new Block("p", 1, 1, 3)
			{
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator)
			};
			block.BlockElements.Add(new Verse("1-3"));
			block.BlockElements.Add(new ScriptText("Then Jesus said would reduce a donkey. The number of them where to find it. And all went well."));
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Jesus told them where to find a donkey.", true));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[0].InitialStartVerseNumber);
			Assert.AreEqual(0, result[0].InitialEndVerseNumber);
			Assert.AreEqual(3, result[0].LastVerseNum);

			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].PrimaryReferenceText);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(4, result[1].InitialStartVerseNumber);
			Assert.AreEqual(0, result[1].InitialEndVerseNumber);
			Assert.AreEqual(4, result[1].LastVerseNum);
		}

		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_SpeakersDoNotCorrespond_ReferenceTextCopiedIntoFirstBlockForVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Y Pablo respondio diciendo, ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "“Quien eres Senor? Pedro?”");
			vernacularBlocks.Add(CreateBlockForVerse("Paul", 2, "“Vamos a Asia!”"));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 1, "Why do you kick the cat? ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "asked Jesus. ");
			AddBlockForVerseInProgress(referenceBlocks, "Martha", "“Couldn't you have come sooner?” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "muttered Martha.");
			referenceBlocks.Add(CreateBlockForVerse("Timothy", 2, "“Let's go to Asia!”"));

			Assert.AreEqual(referenceBlocks.Count, vernacularBlocks.Count); // Sanity check

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual(4, result[0].ReferenceBlocks.Count);
			// Verse 1
			Assert.IsTrue(result[0].ReferenceBlocks.Select(r => r.GetText(true)).SequenceEqual(referenceBlocks.Take(4).Select(r => r.GetText(true))));
			Assert.AreEqual(0, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(0, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(0, result[3].ReferenceBlocks.Count);
			Assert.IsTrue(result.Take(4).All(b => b.PrimaryReferenceText == null));

			// Verse 2 (different character IDs but we match them anyway because it is the entirety of the verse)
			Assert.AreEqual(1, result[4].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[4].GetText(true), result[4].ReferenceBlocks[0].GetText(true));
			Assert.AreEqual(referenceBlocks[4].CharacterId, result[4].ReferenceBlocks[0].CharacterId);
			Assert.AreEqual(referenceBlocks[4].GetText(true), result[4].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_MultipleSpeakersInVerse_SpeakersBeginCorrespondingThenDoNotCorrespond_ReferenceTextCopiedIntoFirstBlockForVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "thus he spake. ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual(referenceBlocks[0], result[0].ReferenceBlocks.Single());
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);

			Assert.IsFalse(result[1].MatchesReferenceText);
			Assert.IsTrue(result[1].ReferenceBlocks.Select(r => r.GetText(true)).SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))));
			Assert.IsNull(result[1].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernHasVerse1ButReferenceDoesNot_OtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 1 but those are not present in the reference text translation.
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Hola!", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus, "));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsNull(result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerse1ButVernDoesNot_OtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 1 but those are not present in the vernacular translation.
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus, "));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Hello!", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernHasVerse0ButReferenceDoesNot_Verse0NotMatchedAndOtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 0 but those are not present in the reference text translation.
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsNull(result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasVerse0ButVernDoesNot_Verse0IgnoredAndOtherVersesMatchedCorrectly()
		{
			// The only scenario we can think of at this point is if the versification makes
			// the Hebrew titles in Psalms vs. 0 but those are not present in the vernacular translation.
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernHasTwoParagraphsOfHebrewSubtitleButReferenceHasOne_TreatedLikeNormalVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(0, "A psalm of David", true, 1, "PSA", "d"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(referenceBlocks.Count, result.SelectMany(v => v.ReferenceBlocks).Count());

			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);

			Assert.AreEqual(0, result[1].ReferenceBlocks.Count);
			Assert.IsFalse(result[1].MatchesReferenceText);
			Assert.IsNull(result[1].PrimaryReferenceText);

			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);

			Assert.AreEqual(1, result[3].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[3].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[3].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasTwoParagraphsOfHebrewSubtitleButVernHasOne_TreatedLikeNormalVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(0, "Hola!", true, 1, "PSA", "d"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Porque pateas al gato?” ");
			var vernBook = new BookScript("PSA", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(0, "Hello!", true, 1, "PSA", "d"));
			var block = new Block("d", 1) {
				IsParagraphStart = true,
				CharacterId = CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.Narrator),
				BlockElements = new List<BlockElement> { new ScriptText("A psalm of David") }
			};
			referenceBlocks.Add(block);
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true, 1, "PSA", "q"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Why do you kick the cat? ");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);

			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(Join(" ", referenceBlocks.Take(2).Select(b => b.GetText(true))), result[0].PrimaryReferenceText);

			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);

			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_ReferenceHasMoreVersesThanVernacular_ExtraVersesIgnored()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(8, "Con gran temblor...", true, 16, "MRK"));
			var vernBook = new BookScript("MRK", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(8, "Trembling and bewildered...", true, 16, "MRK"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(9, "Hey, who added these verse to the Bible? ", true, 16, "MRK"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(10, "remember what God said about that!", false, 16, "MRK"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularHasIntroAndReferenceDoesnt_IntroIgnored()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Intro), 0, "Intro uno", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Con gran temblor...", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Trembling and bewildered...", true));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsNull(result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceHaveIntros_IntroNotMatched()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Intro), 0, "Intro uno", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Con gran temblor...", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Intro), 0, "Introduction", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Trembling and bewildered...", true));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsNull(result[0].PrimaryReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_MultipleVersesInSingleReferenceBlock_VernacularNotSplitAtVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Verse uno.", true).AddVerse(2, "Verse dos."));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Verse 1.", true).AddVerse(2, "Verse 2."));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularNeedsToBeBrokenByReference_FirstReferenceVerseHasTwoBlocks_VernacularBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(7, "Verse seven. ", true).AddVerse(8, "Verse eight. "));
			AddBlockForVerseInProgress(vernacularBlocks, "Herod", "What Herod says in verse eight.");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(7, "Verse 7. ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Herod", "What Herod says in verse 7. ");
			referenceBlocks.Add(CreateNarratorBlockForVerse(8, "Verse 8. ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Herod", "What Herod says in verse 8.");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(3, result.Count);

			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true) + referenceBlocks[1].GetText(true), result[0].PrimaryReferenceText);

			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[2].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);

			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
		}

		/// <summary>
		/// The following is no longer true...
		/// PG-699: Note that this test expects results that are actually less than ideal, since in this case it just so happens
		/// that we would actually prefer for everything to just match up (as it used to).
		/// </summary>
		[Test]
		public void ApplyTo_ReferenceTextNeedsToBeBrokenAtVerseToMatchVernacular_NarratorIntroducesQuoteAtStartOfVerseTwo_ReferenceTextIsBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1,
				"Ka doŋ ginywalo Yecu i Beterekem ma i Judaya i kare me loc pa kabaka Kerode, nen, luryeko mogo ma gua yo tuŋ wokceŋ " +
				"gubino i Jerucalem, kun gipenyo ni, ", true, 2));
			vernacularBlocks.Add(CreateBlockForVerse("magi (wise men from East)", 2,
				"“En latin ma ginywalo me bedo kabaka pa Lujudaya-ni tye kwene? Pien onoŋo waneno lakalatwene yo tuŋ wokceŋ, " +
				"ci man wabino ka wore.”", true, 2));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Now when Jesus was born in Bethlehem of Judea in the days of King Herod, " +
				"behold, wise men from the east came to Jerusalem, ", true, 2).AddVerse(2, "saying, "));
			AddBlockForVerseInProgress(referenceBlocks, "magi (wise men from East)", "“Where is the one who is born King of the Jews? For we saw his star in the east, " +
				"and have come to worship him.”");

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(2, result.Count);

			Assert.AreEqual("{1}\u00A0Now when Jesus was born in Bethlehem of Judea in the days of King Herod, " +
				"behold, wise men from the east came to Jerusalem, ", result[0].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);

			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual("{2}\u00A0saying, " + referenceBlocks[1].GetText(true), result[1].ReferenceBlocks.Single().GetText(true));
		}

		/// <summary>
		/// PG-699: This test illustrates the real reason we want to be able to break the reference text at the end of
		/// any verses that have breaks in the vernacular.
		/// PG-761: Added secondary reference text to show that it needs to be split also.
		/// </summary>
		[Test]
		public void ApplyTo_ReferenceTextNeedsToBeBrokenAtVerseToMatchVernacular_GoodIllustration_ReferenceTextIsBrokenCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(16, "diciendo: ", true, 18, "REV"));
			AddBlockForVerseInProgress(vernacularBlocks, "merchants of the earth", "«¡Ay, ay, la gran ciudad, que estaba vestida de " +
				"lino fino, púrpura y escarlata, y adornada de oro, piedras preciosas y perlas!»");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(17,
				"Porque en una hora ha sido arrasada tanta riqueza. Y todos los capitanes, pasajeros y marineros, y todos los que viven " +
				"del mar, se pararon a lo lejos, ", false, 18, "REV"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(18, "y al ver el humo de su incendio gritaban, diciendo: ", false, 18, "REV"));
			AddBlockForVerseInProgress(vernacularBlocks, "merchants of the earth", "«¿Qué ciudad es semejante a la gran ciudad?»");
			var vernBook = new BookScript("REV", vernacularBlocks);

			var frenchReferenceBlocks = new List<Block>();
			frenchReferenceBlocks.Add(CreateNarratorBlockForVerse(16, "en disant, ", true, 18, "REV"));
			AddBlockForVerseInProgress(frenchReferenceBlocks, "merchants of the earth", "«Malheur, malheur, la grande ville, elle qui était " +
				"vêtue de fin lin, de pourpre et d'écarlate, et parée d' or et de pierres précieuses et de perles!");
			frenchReferenceBlocks.Add(CreateBlockForVerse("merchants of the earth", 17, "Pour en une heure tant de richesses sont en souffrance.»", false, 18));
			AddNarratorBlockForVerseInProgress(frenchReferenceBlocks, "Chaque capitaine, et tout le monde qui navigue partout, et les marins, et " +
				"autant que gagner leur vie en mer, se tenaient loin, ", "REV")
				.AddVerse(18, "et pleuré comme ils ont regardé la fumée de son embrasement: ");
			AddBlockForVerseInProgress(frenchReferenceBlocks, "merchants of the earth", "«Qu'est-ce que la grande ville?»");

			var englishReferenceBlocks = new List<Block>();
			englishReferenceBlocks.Add(CreateNarratorBlockForVerse(16, "saying, ", true, 18, "REV"));
			AddBlockForVerseInProgress(englishReferenceBlocks, "merchants of the earth", "“Woe, woe, the great city, she who was dressed in " +
				"fine linen, purple, and scarlet, and decked with gold and precious stones and pearls!");
			englishReferenceBlocks.Add(CreateBlockForVerse("merchants of the earth", 17, "For in an hour such great riches are made desolate.”", false, 18));
			AddNarratorBlockForVerseInProgress(englishReferenceBlocks, "Every shipmaster, and everyone who sails anywhere, and mariners, and " +
				"as many as gain their living by sea, stood far away, ", "REV")
				.AddVerse(18, "and cried out as they looked at the smoke of her burning, saying, ");
			AddBlockForVerseInProgress(englishReferenceBlocks, "merchants of the earth", "“What is like the great city?”");

			for (int i = 0; i < englishReferenceBlocks.Count; i++)
				frenchReferenceBlocks[i].SetMatchedReferenceBlock(englishReferenceBlocks[i]);

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, frenchReferenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(5, result.Count);

			Assert.AreEqual(frenchReferenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual("{16}\u00A0saying, ", result[0].ReferenceBlocks.Single().PrimaryReferenceText);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(englishReferenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().PrimaryReferenceText);

			Assert.AreEqual(frenchReferenceBlocks[1].GetText(true), result[1].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(englishReferenceBlocks[1].GetText(true), result[1].ReferenceBlocks.Single().PrimaryReferenceText);

			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.AreEqual(frenchReferenceBlocks[2].GetText(true) + " " + frenchReferenceBlocks[3].GetText(true),
				result[2].ReferenceBlocks.Single().GetText(true));
			Assert.AreEqual(englishReferenceBlocks[2].GetText(true) + " " + englishReferenceBlocks[3].GetText(true),
				result[2].ReferenceBlocks[0].ReferenceBlocks.Single().GetText(true));

			Assert.AreEqual("{18}\u00A0et pleuré comme ils ont regardé la fumée de son embrasement: ", result[3].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[3].MatchesReferenceText);
			Assert.AreEqual("{18}\u00A0and cried out as they looked at the smoke of her burning, saying, ", result[3].ReferenceBlocks.Single().PrimaryReferenceText);

			Assert.AreEqual(frenchReferenceBlocks.Last().GetText(true), result[4].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[4].MatchesReferenceText);
			Assert.AreEqual(englishReferenceBlocks.Last().GetText(true), result[4].ReferenceBlocks.Single().PrimaryReferenceText);
		}

		[TestCase(null)]
		[TestCase("[")] // PG-760
		[TestCase("[ ")] // PG-760
		public void ApplyTo_VernacularHasBlockThatStartsWithVerseNotInReferenceText_ReferenceTextIsBrokenCorrectly(string openingBracket)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(36, "And as they went on the way, they came unto a certain water; and the eunuch saith, ", true, 8, "ACT"));
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
			var vernBook = new BookScript("ACT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(36, "Yendo por el camino, llegaron a un lugar donde había agua; y el eunuco dijo: ", true, 8, "ACT"));
			AddBlockForVerseInProgress(referenceBlocks, "Ethiopian officer of Queen Candace", "«Mira, agua. ¿Qué impide que yo sea bautizado? ").AddVerse(38, "¡Para el carruaje!» ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Ambos descendieron al agua, Felipe y el eunuco, y lo bautizó.", "ACT");
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);

			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);

			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks.Single().GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);

			Assert.AreEqual(0, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(0, result[3].ReferenceBlocks.Count);
			Assert.AreEqual(0, result[4].ReferenceBlocks.Count);
			Assert.AreEqual(0, result[5].ReferenceBlocks.Count);

			Assert.IsTrue(result[6].MatchesReferenceText);
			Assert.AreEqual("{38}\u00A0¡Para el carruaje!» " + referenceBlocks.Last().GetText(true), result[6].ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void ApplyTo_StandardReferenceTextSplitToMatchVernacular_ModifiedBooksGetReloadedWhenStandardReferenceTextIsGottenAgain()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1,
				"Ka doŋ ginywalo Yecu i Beterekem ma i Judaya i kare me loc pa kabaka Kerode, nen, luryeko mogo ma gua yo tuŋ wokceŋ " +
				"gubino i Jerucalem, kun gipenyo ni, ", true, 2));
			vernacularBlocks.Add(CreateBlockForVerse("magi (wise men from East)", 2,
				"“En latin ma ginywalo me bedo kabaka pa Lujudaya-ni tye kwene? Pien onoŋo waneno lakalatwene yo tuŋ wokceŋ, " +
				"ci man wabino ka wore.”", true, 2));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var origBlockCountForMatthew = refText.Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count;

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.IsTrue(result[1].MatchesReferenceText);

			Assert.AreNotEqual(origBlockCountForMatthew, refText.Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count);
			
			Assert.AreEqual(origBlockCountForMatthew,
				ReferenceText.GetStandardReferenceText(ReferenceTextType.English).Books.Single(b => b.BookId == "MAT").GetScriptBlocks().Count);
		}

		[Test]
		public void ApplyTo_FirstTwoVersesLinkedNtoM_RemainingVersesMatchedCorrectly()
		{
			//Acholi MAT 9:23-26
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(23, "Ka Yecu obino i ot pa laloc, oneno lukut nyamulere, kun lwak tye ka koko, ", false, 9)
				.AddVerse(24, "ci owacci, "));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "“Wua woko; nyakoni pe oto ento onino anina.” ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Ci gin gunyere anyera. ")
				.AddVerse(25, "Ento i kare ma doŋ oryemo lwak gukato woko, ci en odonyo i ot omako latin nyako-nu ki ciŋe, nyako-nu oa malo. ")
				.AddVerse(26, "Lok meno oywek owinnye oromo lobo meno ducu.");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(23, "When Jesus came into the ruler’s house and saw the flute players and the noisy crowd, ", false, 9)
				.AddVerse(24, "he said to them,"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“Go away. The girl isn’t dead, but sleeping.”");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "They mocked him, saying:");
			AddBlockForVerseInProgress(referenceBlocks, "people at Jairus' house", "“The girl is dead!”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(25, "But when the crowd was put out, he entered in, took her by the hand, and the girl arose.", false, 9));
			referenceBlocks.Add(CreateNarratorBlockForVerse(26, "The report of this went out into all that land.", false, 9));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(5, result.Count);

			Assert.AreEqual(4, result[0].ReferenceBlocks.Count + result[1].ReferenceBlocks.Count + result[2].ReferenceBlocks.Count);

			Assert.AreEqual(1, result[3].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[4].GetText(true), result[3].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[3].MatchesReferenceText);

			Assert.AreEqual(1, result[4].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[5].GetText(true), result[4].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[4].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_SomeBlocksPrematched_PrematchedVersesUnchangedAndOthersMatchedCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "John", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Juan el");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“amado”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "This is what happened.", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "said Jesus. In response, John spake thusly: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "Cool! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "which translated, means “thank you.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			// Now pre-match the vernacular blocks for verse 2:
			var narrator = CharacterVerseData.GetStandardCharacterId(vernBook.BookId, CharacterVerseData.StandardCharacter.Narrator);
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

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(7, result.Count);
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));

			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);
			Assert.AreEqual("{2}\u00A0Then Jesus said:", result[1].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(false), result[2].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[3].PrimaryReferenceText);
			Assert.AreEqual("responded John the ", result[4].PrimaryReferenceText);
			Assert.AreEqual("beloved.", result[5].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[5].GetText(true), result[6].PrimaryReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_SimpleVerseMapping_VersificationDifferencesResolvedBeforeMatching()
		{
			//GEN 32:1-32 = GEN 32:2-33
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(7, "Verse siete. ", true, 32, "GEN"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(8, "Verse ocho.", false, 32, "GEN"));
			var vernBook = new BookScript("GEN", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(6, "Verse 6. ", true, 32, "GEN"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(7, "Verse 7.", false, 32, "GEN"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
		}

		/// <summary>
		/// PG-786: Prevent skipping chunks of reference text when versification pulls stuff into an earlier chapter
		/// </summary>
		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_VersesMappedToEarlierChapter_InterveningMaterialMatches()
		{
			var chapterAnnouncementCharacter = CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.BookOrChapter);
			//ROM 14:24-26 = ROM 16:25-27
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(23, "Fourteen Twenty-three. ", true, 14, "ROM"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(24, "Fourteen Twenty-four.", true, 14, "ROM"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(25, "Fourteen Twenty-five.", false, 14, "ROM"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(26, "Fourteen Twenty-six.", false, 14, "ROM"));
			vernacularBlocks.Add(CreateBlockForVerse(chapterAnnouncementCharacter, 0, "15", true, 15, "c"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Fifteen One.", true, 15, "ROM"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Fifteen Two.", false, 15, "ROM"));
			vernacularBlocks.Add(CreateBlockForVerse(chapterAnnouncementCharacter, 0, "16", true, 16, "c"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Sixteen One.", true, 16, "ROM"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(23, "Gaius, whose hospitality...", true, 16, "ROM"));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Erastus sayas hi.", "ROM");
			var vernBook = new BookScript("ROM", vernacularBlocks);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.IsTrue(vernacularBlocks.Take(9).All(b => b.MatchesReferenceText)); // This could be 10, but that's not the point of this test

			Assert.IsTrue(result[0].PrimaryReferenceText.StartsWith("{23}\u00A0"));
			Assert.AreEqual(14, result[0].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[1].PrimaryReferenceText.StartsWith("{25}\u00A0"));
			Assert.AreEqual(16, result[1].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[2].PrimaryReferenceText.StartsWith("{26}\u00A0"));
			Assert.AreEqual(16, result[2].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[3].PrimaryReferenceText.StartsWith("{27}\u00A0"));
			Assert.AreEqual(16, result[3].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[4].ReferenceBlocks.Single().IsChapterAnnouncement);
			Assert.AreEqual(15, result[4].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[5].PrimaryReferenceText.StartsWith("{1}\u00A0"));
			Assert.AreEqual(15, result[5].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[6].PrimaryReferenceText.StartsWith("{2}\u00A0"));
			Assert.AreEqual(15, result[6].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[7].ReferenceBlocks.Single().IsChapterAnnouncement);
			Assert.AreEqual(16, result[7].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[8].PrimaryReferenceText.StartsWith("{1}\u00A0"));
			Assert.AreEqual(16, result[8].ReferenceBlocks.Single().ChapterNumber);
			Assert.AreEqual(16, result[9].ReferenceBlocks.Single().ChapterNumber);
			Assert.IsTrue(result[9].PrimaryReferenceText.StartsWith("{23}\u00A0"));
			Assert.IsFalse(result[10].ReferenceBlocks.Any());
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_CrossChapterMapping_VersificationDifferencesResolvedBeforeMatching()
		{
			//EXO 8:1-4 = EXO 7:26-29
			//EXO 8:5-32 = EXO 8:1-28
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(29, "Versículo 29. ", true, 7, "EXO"));
			vernacularBlocks.Add(CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId("EXO", CharacterVerseData.StandardCharacter.BookOrChapter), 0, "Chapter 8", false, 8, "c"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Versículo 1.", false, 8, "EXO"));
			var vernBook = new BookScript("EXO", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(4, "Verse 4. ", true, 8, "EXO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(5, "Verse 5.", false, 8, "EXO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(vernacularBlocks.Count, result.Count);
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("The Gospel According to Thomas 8", result[1].PrimaryReferenceText);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(1, result[2].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[2].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[2].MatchesReferenceText);
		}

		[Test]
		public void ApplyTo_VernacularAndReferenceVersificationsDoNotMatch_SplitVernacularBasedOnReference()
		{
			//EXO 8:5-32 = EXO 8:1-28
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse("Fred", 1, "Cosas que Fred dice, ", true, 8));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "dijo Fred. ", "EXO");
			var block = CreateNarratorBlockForVerse(2, "Blah blah. ", false, 8, "EXO");
			block.AddVerse(3, "More blah blah. ").AddVerse(4, "The final blah blah.");
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("EXO", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(5, "I don't know if Fred told you this or not, but he's crazy. ", true, 8, "EXO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(6, "This is your narrator speaking. ", false, 8, "EXO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(7, "I hope you enjoy your flight. ", false, 8, "EXO"));
			referenceBlocks.Add(CreateNarratorBlockForVerse(8, "The end. ", false, 8, "EXO"));

			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			refText.ApplyTo(vernBook, m_vernVersification);

			Assert.AreEqual(4, referenceBlocks.Count);
			var result = vernBook.GetScriptBlocks();
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("{1}\u00A0Cosas que Fred dice, ", result[0].GetText(true));
			Assert.AreEqual(0, result[0].ReferenceBlocks.Count);

			Assert.AreEqual("dijo Fred. ", result[1].GetText(true));
			Assert.AreEqual("{5}\u00A0I don't know if Fred told you this or not, but he's crazy. ", result[1].ReferenceBlocks.Single().GetText(true));

			Assert.AreEqual("{2}\u00A0Blah blah. ", result[2].GetText(true));
			Assert.AreEqual("{3}\u00A0More blah blah. ", result[3].GetText(true));
			Assert.AreEqual("{4}\u00A0The final blah blah.", result[4].GetText(true));
			Assert.IsTrue(result.Skip(2).Select(v => v.PrimaryReferenceText).SequenceEqual(referenceBlocks.Skip(1).Select(r => r.GetText(true))));
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

			var jude = TestReferenceText.CreateTestReferenceText(Resources.TestReferenceTextJUD).GetBooksWithBlocksConnectedToReferenceText(TestProject.CreateBasicTestProject()).Single();
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
					if (expectedReferenceResults[i] != block.PrimaryReferenceText)
						sbForReferenceTextResults.Append("Expected: ").Append(expectedReferenceResults[i]).AppendLine()
							.Append("Actual: ").Append(block.PrimaryReferenceText).AppendLine().AppendLine();
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

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_ReferenceTextDoesNotContainBook_NoChangeToVernacular()
		{
			var refTextForJude = TestReferenceText.CreateTestReferenceText(Resources.TestReferenceTextJUD);
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.RUT);
			var blocksBeforeCall = testProject.IncludedBooks[0].GetScriptBlocks();
			var result = refTextForJude.GetBooksWithBlocksConnectedToReferenceText(testProject);
			var resultBlocks = result.Single().GetScriptBlocks();
			Assert.IsTrue(blocksBeforeCall.Select(b => b.GetText(true)).SequenceEqual(resultBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(resultBlocks.All(b => (b.ReferenceBlocks == null || !b.ReferenceBlocks.Any()) && !b.MatchesReferenceText));
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_VernacularHasSubsequentBlocksWithSameCharacter_BlocksAreJoinedThenSplitBasedOnReference()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "El cual significa, ", true));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“Dios con nosotros.” ").AddVerse(2, "Blah blah. ");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Which means, God with us.", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "This is your narrator speaking. "));
			var primaryReferenceText = TestReferenceText.CreateTestReferenceText("JUD", referenceBlocks);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject).Single().GetScriptBlocks();

			Assert.AreEqual(2, result.Count);

			Assert.AreEqual("{1}\u00A0El cual significa, “Dios con nosotros.” ", result[0].GetText(true));
			Assert.AreEqual(1, result[0].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);

			Assert.AreEqual("{2}\u00A0Blah blah. ", result[1].GetText(true));
			Assert.AreEqual(1, result[1].ReferenceBlocks.Count);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].ReferenceBlocks[0].GetText(true));
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].PrimaryReferenceText);
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_SomeBlocksPrematched_PrematchedVersesUnchangedAndOthersMatchedCorrectly()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "John", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Juan el");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "“amado”");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "This is what happened.", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "said Jesus. In response, John spake thusly: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "Cool! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "which translated, means “thank you.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));

			// Now pre-match the vern blocks for verse 2:
			var narrator = CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator);
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

			var metadata = new GlyssenDblTextMetadata();
			metadata.Language = new GlyssenDblMetadataLanguage { Name = "Doublespeak" };
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			var doublespeakFolder = Path.Combine(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation, "Doublespeak");
			Directory.CreateDirectory(doublespeakFolder);
			var glyssenFilePath = Path.Combine(doublespeakFolder, "doublespeak.glyssen");
			XmlSerializationHelper.SerializeToFile(glyssenFilePath, metadata);
			var primaryReferenceText = ReferenceText.GetReferenceText(ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, "Doublespeak"));
			var books = (List<BookScript>)primaryReferenceText.Books;
			var refBook = new BookScript(testProject.Books[0].BookId, referenceBlocks);
			books.Add(refBook);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject).Single().GetScriptBlocks();

			Assert.AreEqual(6, result.Count);
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));

			Assert.AreEqual(referenceBlocks[0].GetText(true), result[0].PrimaryReferenceText);
			Assert.AreEqual("{2}\u00A0Then Jesus said:", result[1].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[1].GetText(false), result[2].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[3].GetText(true), result[3].PrimaryReferenceText);
			Assert.AreEqual("responded John the beloved.", result[4].PrimaryReferenceText);
			Assert.AreEqual(referenceBlocks[5].GetText(true), result[5].PrimaryReferenceText);
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_SomeBlocksPrematchedAndJoined_PrematchedVersesDoNotGetResplit()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(new Block("c", 3) {BookCode = "MAT"});
			vernacularBlocks.Last().BlockElements.Add(new ScriptText("3"));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Een dat time, John wa Bactize come ta de wildaness een Judea, an e staat fa preach dey. ", true, 3));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "E tell um say, "));
			AddBlockForVerseInProgress(vernacularBlocks, "John the Baptist", "“Oona mus change oona sinful way an dohn do um no mo. Cause de time mos yah wen God gwine rule oba we!” ");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "John been de man wa de prophet Isaiah beena taak bout wen e say,"));
			AddBlockForVerseInProgress(vernacularBlocks, "scripture", "“Somebody da holla een de wildaness say, ‘Oona mus cleah de road weh de Lawd gwine come shru.", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, "scripture", "Mek de pat scraight fa um fa waak!’ ”", "q2");

			var testProject = TestProject.CreateTestProject(TestProject.TestBook.MAT);
			testProject.Books[0].Blocks = vernacularBlocks;

			// Now pre-match the vern blocks for verses 1-2:
			vernacularBlocks[1].SetMatchedReferenceBlock("In those days, John the Baptizer came, preaching in the wilderness of Judea,");
			vernacularBlocks[2].SetMatchedReferenceBlock("...{2} saying,");
			vernacularBlocks[3].SetMatchedReferenceBlock("“Repent, for the Kingdom of Heaven is at hand!”");

			var result = ReferenceText.GetStandardReferenceText(ReferenceTextType.English).GetBooksWithBlocksConnectedToReferenceText(testProject).Single().GetScriptBlocks();

			Assert.AreEqual(6, result.Count);
			Assert.IsFalse(result[0].MatchesReferenceText);
			Assert.IsTrue(result[1].MatchesReferenceText);
			Assert.IsTrue(result[2].MatchesReferenceText);
			Assert.IsFalse(result[3].MatchesReferenceText);
			Assert.IsFalse(result[4].MatchesReferenceText);
			Assert.IsFalse(result[5].MatchesReferenceText);

			Assert.AreEqual("In those days, John the Baptizer came, preaching in the wilderness of Judea, {2}\u00A0saying,", result[1].PrimaryReferenceText);
			Assert.AreEqual("“Repent, for the Kingdom of Heaven is at hand!”", result[2].PrimaryReferenceText);
		}

		[Test]
		public void GetBooksWithBlocksConnectedToReferenceText_VernacularContainsQBlocks_ReferenceTextSingleBlock_VernacularBlocksCombined()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateBlockForVerse("Peter", 1, "Juan dijo, ", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "'This es estrofa 1, ", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 2, ", "q2");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 3, ", "q1");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "This es estrofa 4.'", "q2");
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.ACT);
			testProject.Books[0].Blocks = vernacularBlocks;

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateBlockForVerse("Peter", 1, "John said, 'This is line 1, This is line 2, This is line 3, This is line 4.'", true));

			var metadata = new GlyssenDblTextMetadata();
			metadata.Language = new GlyssenDblMetadataLanguage { Name = "Poetian" };
			TestReferenceText.OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			var doublespeakFolder = Path.Combine(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation, "Poetian");
			Directory.CreateDirectory(doublespeakFolder);
			var glyssenFilePath = Path.Combine(doublespeakFolder, "poetian.glyssen");
			XmlSerializationHelper.SerializeToFile(glyssenFilePath, metadata);
			var primaryReferenceText = ReferenceText.GetReferenceText(ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, "Poetian"));

			ReflectionHelper.SetField(primaryReferenceText, "m_vers", ScrVers.English);
			var books = (List<BookScript>)primaryReferenceText.Books;
			var refBook = new BookScript(testProject.Books[0].BookId, referenceBlocks);
			books.Add(refBook);

			var result = primaryReferenceText.GetBooksWithBlocksConnectedToReferenceText(testProject).Single().GetScriptBlocks().Single();

			Assert.AreEqual("{1}\u00A0Juan dijo, 'This es estrofa 1, This es estrofa 2, This es estrofa 3, This es estrofa 4.'",
				result.GetText(true));
			Assert.AreEqual(referenceBlocks.Single().GetText(true), result.ReferenceBlocks.Single().GetText(true));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_BadIndex_ThrowsArgumentOutOfRangeException()
		{
			var vernBook = new BookScript("MAT", new List<Block>(0));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, new List<Block>(0));

			Assert.Throws<ArgumentOutOfRangeException>(() => refText.GetBlocksForVerseMatchedToReferenceText(vernBook, -1, m_vernVersification));
			Assert.Throws<ArgumentOutOfRangeException>(() => refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, m_vernVersification));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_NoCorrespondingBookInReferenceText_ReturnsNull()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);
			var refText = TestReferenceText.CreateTestReferenceText("LUK", new List<Block>(0));

			Assert.IsNull(refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, m_vernVersification));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlocksAreForSingleVerse_ReturnedBlocksAreMatchedClonesOfOriginals(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Este es versiculo uno, ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "asi dijo. Pero Paul replico: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Paul", "Asi pense, ");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "“This is verse one.” ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But Paul replied, ");
			AddBlockForVerseInProgress(referenceBlocks, "Paul", "“That's what I thought.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "Extra stuff that is not used. ", true).AddVerse(3));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.IsTrue(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Select(b => b.ToString())));
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void GetBlocksForVerseMatchedToReferenceText_VernAndRefHaveMultipleSingleBlockVerses_ReturnsSingleClonedAndMatchedBlock(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(CreateBlockForVerse("Jesus", 2, "Nunca os dejare! ", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Entonces fue con sus discipulos al jardin, donde Pedro dijo: ", true));
			vernacularBlocks.Add(CreateBlockForVerse("Peter", 4, "Me gusta este lugar.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "Then he went with his disciples to the garden, whereupon Peter said: ", true));
			referenceBlocks.Add(CreateBlockForVerse("Peter", 4, "“I like this place.”", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.AreEqual(vernacularBlocks[iBlock].ToString(), result.Single().ToString());
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsTrue(result.Single().MatchesReferenceText);
			Assert.AreEqual(referenceBlocks[iBlock].GetText(true), result.Single().PrimaryReferenceText);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocks_ReturnsMultipleClonedAndMatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(CreateBlockForVerse("Jesus", 2, "Nunca os dejare! ", true));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "En ese momento, Pedro entro exclamando: ");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "Nos quieren hundir con los impuestos exagerados! ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Claro que estaba exagerando.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, Peter burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "Peter", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.IsTrue(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())));
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));
			Assert.IsTrue(referenceBlocks.Skip(1).Take(4).Select(r => r.GetText(true)).SequenceEqual(result.Select(v => v.PrimaryReferenceText)));
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocksWithCharactersAssignedThatDoNotMatchReference_ReturnsMultipleClonedAndMismatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Pedro.");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, John burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "John", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.IsTrue(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())));
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsFalse(result.Take(3).All(b => b.MatchesReferenceText));
			Assert.IsTrue(referenceBlocks.Skip(1).Take(3).Select(r => r.GetText(true)).SequenceEqual(result[0].ReferenceBlocks.Select(b => b.GetText(true))));
			Assert.IsTrue(result[3].MatchesReferenceText);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_VerseHasMultipleBlocksWithoutCharactersAssignedThatCouldMatchReference_ReturnsMultipleClonedAndMatchedBlocks(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Entonces dijo Jesus: ", true));
			vernacularBlocks.Add(CreateBlockForVerse(CharacterVerseData.kAmbiguousCharacter, 2, "Nunca os dejare! ", true));
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "En ese momento, Pedro entro exclamando: ");
			AddBlockForVerseInProgress(vernacularBlocks, CharacterVerseData.kAmbiguousCharacter, "Nos quieren hundir con los impuestos exagerados! ");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "Claro que estaba exagerando.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Eso es todo.", true));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
			AddNarratorBlockForVerseInProgress(referenceBlocks, "But just then, Peter burst in and exclaimed: ");
			AddBlockForVerseInProgress(referenceBlocks, "Peter", "They want to tax us to death! ");
			AddNarratorBlockForVerseInProgress(referenceBlocks, "Of course, he was exagerating.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "That's all.", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.IsTrue(result.Select(b => b.ToString()).SequenceEqual(vernacularBlocks.Skip(1).Take(4).Select(b => b.ToString())));
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));
			Assert.IsTrue(referenceBlocks.Skip(1).Take(4).Select(r => r.GetText(true)).SequenceEqual(result.Select(v => v.PrimaryReferenceText)));
		}

		[TestCase(0)]
		[TestCase(1)]
		public void GetBlocksForVerseMatchedToReferenceText_VernVerseStartsMidBlock_ReturnedBlocksAreMatchedClonesOfOriginals(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Partieron de alli para Jerico. ", true).AddVerse(2, "Entonces dijo Jesus: "));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "Come to me you who are weary.");
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "Another verse."));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "They journeyed on from there to Jericho.", true));
			referenceBlocks.Add(CreateNarratorBlockForVerse(2, "Then Jesus said,"));
			AddBlockForVerseInProgress(referenceBlocks, "Jesus", "Come to me you who are weary.");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "Extra stuff that is not used. ", true));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].BlockElements.OfType<ScriptText>().First().Content,
				matchup.CorrelatedAnchorBlock.BlockElements.OfType<ScriptText>().First().Content);
			var result = matchup.CorrelatedBlocks;
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));

			Assert.AreEqual("{1}\u00A0Partieron de alli para Jerico. ", result[0].GetText(true));
			Assert.AreEqual("{2}\u00A0Entonces dijo Jesus: ", result[1].GetText(true));
			Assert.AreEqual("Come to me you who are weary.", result[2].GetText(true));

			Assert.IsTrue(result.Select(b => b.PrimaryReferenceText).SequenceEqual(referenceBlocks.Take(3).Select(b => b.GetText(true))));
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_EnglishRefVerseStartsMidBlock_MatchupIncludesPrecedingVerse()
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Maria i karim Jisas long taun Betlehem long distrik Judia long taim Herot i stap king. Em i karim Jisas pinis, na bihain sampela saveman bilong hap sankamap i kam long Jerusalem na ol i askim nabaut olsem, ", true, 2));
			vernacularBlocks.Add(CreateBlockForVerse("magi", 2, "\"Nupela pikinini em king bilong ol Juda, em i stap we ? Mipela i lukim sta bilong en long hap sankamap, na mipela i kam bilong lotu long em.\"", false, 2));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(3, "King Herot i... ", true, 2));
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Now when Jesus was born in Bethlehem of Judea in the days of King Herod, behold, wise men from the east came to Jerusalem,", true, 2).AddVerse(2, "saying, "));
			AddBlockForVerseInProgress(referenceBlocks, "magi", " “Where is the one who is born King of the Jews? For we saw his star in the east, and have come to worship him.”");
			referenceBlocks.Add(CreateNarratorBlockForVerse(3, "When King Herod... ", true, 2));
			var refText = TestReferenceText.CreateTestReferenceText(vernBook.BookId, referenceBlocks);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[1].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			var result = matchup.CorrelatedBlocks;
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(0, vernacularBlocks.Intersect(result).Count());
			Assert.IsFalse(result[0].MatchesReferenceText);
			Assert.IsTrue(result[1].MatchesReferenceText);

			Assert.AreEqual("{1}\u00A0Maria i karim Jisas long taun Betlehem long distrik Judia long taim Herot i stap king. Em i karim Jisas pinis, na bihain sampela saveman bilong hap sankamap i kam long Jerusalem na ol i askim nabaut olsem, ", result[0].GetText(true));
			Assert.AreEqual("{2}\u00A0\"Nupela pikinini em king bilong ol Juda, em i stap we ? Mipela i lukim sta bilong en long hap sankamap, na mipela i kam bilong lotu long em.\"", result[1].GetText(true));

			Assert.IsTrue(result.Select(b => b.ReferenceBlocks.Single().GetText(true)).SequenceEqual(referenceBlocks.Take(2).Select(b => b.GetText(true))));
			Assert.AreEqual(referenceBlocks[1].GetText(true), result[1].PrimaryReferenceText);
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void GetBlocksForVerseMatchedToReferenceText_Prematched_SavedMatchesAreNotOverwritten(int iBlock)
		{
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(CreateNarratorBlockForVerse(1, "Esto es lo que paso.", true));
			vernacularBlocks.Add(CreateNarratorBlockForVerse(2, "Entonces dijo Jesus:", true));
			AddBlockForVerseInProgress(vernacularBlocks, "Jesus", "—Nunca os dejare!");
			AddBlockForVerseInProgress(vernacularBlocks, "Peter", "—Gracias.");
			AddNarratorBlockForVerseInProgress(vernacularBlocks, "respondio Pedro.");
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var referenceBlocks = new List<Block>();
			referenceBlocks.Add(CreateNarratorBlockForVerse(1, "Then Jesus said, ", true));
			referenceBlocks.Add(CreateBlockForVerse("Jesus", 2, "“I will never leave you!” ", true));
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
				CharacterId = CharacterVerseData.GetStandardCharacterId(vernBook.BookId, CharacterVerseData.StandardCharacter.Narrator),
				BlockElements = new List<BlockElement> { new ScriptText("answered Peter.") }
			});

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, iBlock, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[iBlock].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			Assert.IsFalse(matchup.HasOutstandingChangesToApply);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_EndOfBookWhereReferenceTextCombinesTwoVerses_DoesNotCrashOrMatch()
		{
			var vernacularBlocks = new List<Block> { CreateNarratorBlockForVerse(25, "Grace be unto you all.", true, 13, "HEB") };
			var vernBook = new BookScript("HEB", vernacularBlocks);

			// The last block of the standard reference text for Hebrews combines verse 24 and 25.
			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[0].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			Assert.AreEqual(1, matchup.CorrelatedBlocks.Count);
			Assert.IsFalse(matchup.CorrelatedBlocks[0].MatchesReferenceText);
		}

		[Test]
		public void GetBlocksForVerseMatchedToReferenceText_VernBlockIsFirstVerseOfCombinedReferenceTextBlock_MatchupIncludesBothVerses()
		{
			var vernacularBlocks = new List<Block> {
				CreateNarratorBlockForVerse(24, "Köszöntsétek minden elõljárótokat és a szenteket mind. Köszöntenek titeket az Olaszországból valók. ", true, 13, "HEB"),
				CreateNarratorBlockForVerse(25, "Kegyelem mindnyájatokkal! Ámen!", true, 13, "HEB")
			};
			var vernBook = new BookScript("HEB", vernacularBlocks);

			// The last block of the standard reference text for Hebrews combines verse 24 and 25.
			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 0, m_vernVersification);
			Assert.AreEqual(vernacularBlocks[0].GetText(true), matchup.CorrelatedAnchorBlock.GetText(true));
			Assert.AreEqual(2, matchup.CorrelatedBlocks.Count);
			Assert.IsFalse(matchup.CorrelatedBlocks[0].MatchesReferenceText);
			Assert.AreEqual(1, matchup.CorrelatedBlocks[0].ReferenceBlocks.Count);
			Assert.IsFalse(matchup.CorrelatedBlocks[1].MatchesReferenceText);
			Assert.AreEqual(0, matchup.CorrelatedBlocks[1].ReferenceBlocks.Count);
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
			var vernBook = new BookScript("MRK", vernacularBlocks);

			var refText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = refText.GetBlocksForVerseMatchedToReferenceText(vernBook, 1, m_vernVersification);
			Assert.AreEqual(3, matchup.CorrelatedBlocks.Count);
			var firstCorrelatedBlock = matchup.CorrelatedBlocks.First();
			Assert.AreEqual(3, firstCorrelatedBlock.BlockElements.Count);
			Assert.AreEqual("[", ((ScriptText)firstCorrelatedBlock.BlockElements[0]).Content);
			Assert.AreEqual("9", ((Verse)firstCorrelatedBlock.BlockElements[1]).Number);
			Assert.IsTrue(((ScriptText)firstCorrelatedBlock.BlockElements[2]).Content.StartsWith("Ka en doŋ"));
		}

		#region private helper methods
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
				initialStartVerse = ScrReference.VerseToIntStart(lastVerseElement.Number);
				initialEndVerse = ScrReference.VerseToIntEnd(lastVerseElement.Number);
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
			return AddBlockForVerseInProgress(list, CharacterVerseData.GetStandardCharacterId(book, CharacterVerseData.StandardCharacter.Narrator), text);
		}

		internal static Block CreateNarratorBlockForVerse(int verseNumber, string text, bool paraStart = false, int chapter = 1, string book = "MAT", string styleTag = "p")
		{
			return CreateBlockForVerse(CharacterVerseData.GetStandardCharacterId(book, CharacterVerseData.StandardCharacter.Narrator),
				verseNumber, text, paraStart, chapter, styleTag);
		}
		#endregion
	}

	public class TestReferenceText : ReferenceText
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum TestReferenceTextResource
		{
			EnglishJUD,
			AzeriJUD,
			AzeriREV,
			FrenchMAT,
			FrenchMRK,
			SpanishMAT,
		}

		private TestReferenceText(GlyssenDblTextMetadata metadata, BookScript book)
			: base(metadata, ReferenceTextType.Custom, null)
		{
			m_books.Add(book);
		}

		protected override void SetVersification()
		{
			Debug.Assert(m_referenceTextType == ReferenceTextType.Custom);
			m_vers = ScrVers.English;
		}

		private static GlyssenDblTextMetadata NewMetadata
		{
			get
			{
				var metadata = new GlyssenDblTextMetadata();
				metadata.Language = new GlyssenDblMetadataLanguage { Iso = "~tst~", Name = "Test Language" };
				return metadata;
			}
		}
			
		public static TestReferenceText CreateTestReferenceText(string bookId, IList<Block> blocks)
		{
			return new TestReferenceText(NewMetadata, new BookScript(bookId, blocks)) { GetBookName = b => "The Gospel According to Thomas" };
		}

		public static TestReferenceText CreateTestReferenceText(string bookScriptXml)
		{
			return new TestReferenceText(NewMetadata, XmlSerializationHelper.DeserializeFromString<BookScript>(bookScriptXml));
		}

		private static bool IsProprietaryReferenceTextLocationOveridden
		{
			get
			{
				return !ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation.EndsWith(
					ReferenceTextIdentifier.kLocalReferenceTextDirectoryName);
			}
		}

		public static void DeleteTempCustomReferenceProjectFolder()
		{
			if (!IsProprietaryReferenceTextLocationOveridden)
				return;
			
			if (Directory.Exists(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation))
				DirectoryUtilities.DeleteDirectoryRobust(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation);

			ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation = null;
		}

		public static void OverrideProprietaryReferenceTextProjectFileLocationToTempLocation()
		{
			if (IsProprietaryReferenceTextLocationOveridden)
				return;
			ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation = Path.GetTempFileName();
			File.Delete(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation);
			Directory.CreateDirectory(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation);
		}

		public static ReferenceText CreateCustomReferenceText(params TestReferenceTextResource[] booksToInclude)
		{
			OverrideProprietaryReferenceTextProjectFileLocationToTempLocation();
			//var sampleMetadata = new GlyssenDblTextMetadata();
			//sampleMetadata.AvailableBooks = new List<Book>();
			//var books = new List<UsxDocument>();

			string customFolderId = null;

			foreach (var testBook in booksToInclude)
				AddBook(testBook, ref customFolderId);

			return GetReferenceText(ReferenceTextIdentifier.GetOrCreate(ReferenceTextType.Custom, customFolderId));
		}

		private static void AddBook(TestReferenceTextResource testResource, ref string customFolderId)
		{
			string folder;
			string fileName;
			string fileContents;
			switch (testResource)
			{
				case TestReferenceTextResource.EnglishJUD:
					folder = "English";
					fileName = "JUD.xml";
					fileContents = Resources.TestReferenceTextJUD;
					break;
				case TestReferenceTextResource.AzeriJUD:
					folder = "Azeri";
					fileName = "JUD.xml";
					fileContents = Resources.AzeriJUDRefText;
					break;
				case TestReferenceTextResource.AzeriREV:
					folder = "Azeri";
					fileName = "REV.xml";
					fileContents = Resources.AzeriREVRefText;
					break;
				case TestReferenceTextResource.FrenchMAT:
					folder = "French";
					fileName = "MAT.xml";
					fileContents = Resources.FrenchMATRefText;
					break;
				case TestReferenceTextResource.FrenchMRK:
					folder = "French";
					fileName = "MRK.xml";
					fileContents = Resources.FrenchMRKRefText;
					break;
				case TestReferenceTextResource.SpanishMAT:
					folder = "Spanish";
					fileName = "MAT.xml";
					fileContents = Resources.SpanishMATRefText;
					break;
				default:
					throw new ArgumentOutOfRangeException("testResource", testResource, null);
			}
			var rtFolder = Path.Combine(ReferenceTextIdentifier.ProprietaryReferenceTextProjectFileLocation, folder);
			if (customFolderId == null)
			{
				customFolderId = folder;
				Directory.CreateDirectory(rtFolder);
				var lowercase = folder.ToLowerInvariant();
				File.WriteAllBytes(Path.Combine(rtFolder, lowercase + kProjectFileExtension), (byte[])Resources.ResourceManager.GetObject(lowercase));
				File.WriteAllText(Path.Combine(rtFolder, "versification.vrs"), Resources.EnglishVersification);
			}
			else if (customFolderId != folder)
			{
				throw new ArgumentException("Attempt to compbine resources for different languages into a single reference text.",
					"testResource");
			}
			File.WriteAllText(Path.Combine(rtFolder, fileName), fileContents);
		}
	}
}
