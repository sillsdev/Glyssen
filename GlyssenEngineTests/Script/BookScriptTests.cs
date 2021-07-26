using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using NUnit.Framework;
using SIL.Extensions;
using SIL.IO;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;
using Resources = GlyssenEngineTests.Properties.Resources;

namespace GlyssenEngineTests.Script
{
	[TestFixture]
	class BookScriptTests
	{
		private int m_curSetupChapter;
		private int m_curSetupVerse;
		private int m_curSetupVerseEnd;
		private string m_curStyleTag;

		private ScrVers m_testVersification;
		private IQuoteInterruptionFinder m_interruptionFinder;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			NarratorOverrides.NarratorOverridesXmlData = Resources.TestNarratorOverrides;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Implementation.Load(tempFile.Path);
			}

			m_interruptionFinder = new QuoteSystem(new QuotationMark("—", "—", null, 1, QuotationMarkingSystemType.Narrative));
		}

		#region GetVerseText Tests
		[Test]
		public void GetVerseText_NoBlocks_ReturnsEmptyString()
		{
			var bookScript = new BookScript("MRK", new List<Block>(), ScrVers.English);
			Assert.AreEqual(String.Empty, bookScript.GetVerseText(1, 1));
		}

		[Test]
		public void GetVerseText_BlocksConsistEntirelyOfRequestedVerse_ReturnsBlockContentsWithoutVerseNumber()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3, "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(4));
			mrkBlocks.Add(NewSingleVersePara(5));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 3));
		}

		[Test]
		public void GetVerseText_FirstVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2, "This is it!").AddVerse(3).AddVerse(4).AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 2));
		}

		[Test]
		public void GetVerseText_SubsequentVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 4));
		}

		[Test]
		public void GetVerseText_VerseInLastBlockInChapterIsRequested_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 4));
		}

		[Test]
		public void GetVerseText_RequestedVerseInChapterTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5, "This is it!").AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(2, 5));
		}

		[Test]
		public void GetVerseText_RequestedLastVerseInChapterOneOfTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7, "This is it!"));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5).AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 7));
		}

		[Test]
		public void GetVerseText_RequestedVerseIsPartOfVerseBridge_ReturnsVerseBridgeContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse("4-6", "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(7));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 5));
		}

		[Test]
		public void GetVerseText_RequestedVerseSpansBlocks_ContentsJoinedToGetAllVerseText()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3, "This is it!"));
			m_curSetupVerse = 3;
			mrkBlocks.Add(NewPara("q1", "This is more of it."));
			mrkBlocks.Add(NewPara("q2", "This is the rest of it."));
			mrkBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual("This is it!" + Environment.NewLine + "This is more of it." + Environment.NewLine + "This is the rest of it.", bookScript.GetVerseText(1, 3));
		}
		#endregion

		#region GetFirstBlockForVerse Tests
		[Test]
		public void GetFirstBlockForVerse_NoBlocks_ReturnsNull()
		{
			var bookScript = new BookScript("MRK", new List<Block>(), ScrVers.English);
			Assert.IsNull(bookScript.GetFirstBlockForVerse(1, 1));
		}

		[Test]
		public void GetFirstBlockForVerse_BlocksConsistEntirelyOfRequestedVerse_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3, "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(4));
			mrkBlocks.Add(NewSingleVersePara(5));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(1, 3);
			Assert.AreEqual("{3}\u00A0This is it!", block.GetText(true));
		}

		[Test]
		public void GetFirstBlockForVerse_FirstVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2, "This is it!").AddVerse(3).AddVerse(4).AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(1, 2);
			Assert.IsTrue(block.GetText(true).StartsWith("{2}\u00A0This is it!{3}\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_SubsequentVerseInBlockIsRequestedVerse_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			mrkBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(1, 4);
			Assert.IsTrue(block.GetText(true).Contains("{4}\u00A0This is it!{5}\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_VerseInLastBlockInChapterIsRequested_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(1, 4);
			Assert.AreEqual(mrkBlocks.Last(), block);
		}

		[TestCase(4)]
		[TestCase(5)]
		public void GetFirstBlockForVerse_VerseBridgeFollowsSectionHead_ReturnsActualVerseNotSectionHead(int verseToFind)
		{
			var matBlocks = new List<Block>();
			matBlocks.Add(NewChapterBlock(7));
			matBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3));
			m_curSetupVerse = 3;
			matBlocks.Add(NewPara("s", "This is a section head", "MAT"));
			var blockToFind = NewVerseBridgePara(4, 5, "This is it.");
			matBlocks.Add(blockToFind);
			matBlocks.Add(NewSingleVersePara(6));
			var bookScript = new BookScript("MAT", matBlocks, ScrVers.English);
			Assert.AreEqual(blockToFind.GetText(true), bookScript.GetFirstBlockForVerse(7, verseToFind).GetText(true));
		}

		[TestCase(4)]
		[TestCase(5)]
		[TestCase(6)]
		public void GetFirstBlockForVerse_VerseIsInVerseBridgeAtEndOfBlock_ReturnsCorrectBlock(int verseToFind)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockToFind = NewSingleVersePara(2, "This is it!").AddVerse(3).AddVerse("4-6");
			mrkBlocks.Add(blockToFind);
			m_curSetupVerse = 4;
			m_curSetupVerseEnd = 6;
			mrkBlocks.Add(NewBlock("This is another paragraph for the bridge"));
			mrkBlocks.Add(NewSingleVersePara(7));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(blockToFind.GetText(true), bookScript.GetFirstBlockForVerse(1, verseToFind).GetText(true));
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseInChapterTwo_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1).AddVerse(2).AddVerse(3).AddVerse(4));
			mrkBlocks.Add(NewSingleVersePara(5).AddVerse(6).AddVerse(7));
			mrkBlocks.Add(NewChapterBlock(2));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3));
			mrkBlocks.Add(NewSingleVersePara(4).AddVerse(5, "This is it!").AddVerse(6));
			mrkBlocks.Add(NewSingleVersePara(7).AddVerse(8));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(2, 5);
			Assert.IsTrue(block.GetText(true).Contains("{5}\u00A0This is it!{6}\u00A0"));
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseIsPartOfVerseBridge_ReturnsVerseBridgeContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse("4-6", "This is it!"));
			mrkBlocks.Add(NewSingleVersePara(7));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var block = bookScript.GetFirstBlockForVerse(1, 5);
			Assert.IsTrue(block.GetText(true).EndsWith("{4-6}\u00A0This is it!"));
		}

		[Test]
		public void GetFirstBlockForVerse_RequestedVerseSpansBlocks_ContentsJoinedToGetAllVerseText()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3, "This is it!"));
			var block = new Block("q1", m_curSetupChapter, 3);
			block.BlockElements.Add(new ScriptText("This is more of it."));
			mrkBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, 3);
			block.BlockElements.Add(new ScriptText("This is the rest of it."));
			mrkBlocks.Add(block);
			mrkBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var firstBlockForVerse1_3 = bookScript.GetFirstBlockForVerse(1, 3);
			Assert.IsTrue(firstBlockForVerse1_3.GetText(true).EndsWith("{3}\u00A0This is it!"));
		}
		#endregion

		#region GetBlocksForVerse Tests
		[Test]
		public void GetBlocksForVerse_SecondVerseInBridge_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(73));
			Block expected;
			mrkBlocks.Add(expected = NewVerseBridgePara(74, 75));
			mrkBlocks.Add(NewSingleVersePara(76));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var list = bookScript.GetBlocksForVerse(1, 75).ToList();
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_SecondVerseInBridgeWithFollowingParasForSameVerseBridge_ReturnsBlockWithVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(73));
			Block expected;
			mrkBlocks.Add(expected = NewVerseBridgePara(74, 75));
			mrkBlocks.Add(NewPara("q1", "more"));
			mrkBlocks.Add(NewPara("q2", "even more"));
			mrkBlocks.Add(NewSingleVersePara(76));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var list = bookScript.GetBlocksForVerse(1, 75).ToList();
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_MultipleBlocksForVerseButSecondBlockDoesNotHaveAnotherVerseNumber_ReturnsAllBlocksForVerse()
		{
			var revBlocks = new List<Block>();
			revBlocks.Add(NewChapterBlock(15));
			revBlocks.Add(NewSingleVersePara(2));
			Block expected;
			revBlocks.Add(expected = NewSingleVersePara(3));
			revBlocks.Add(NewPara("q1", "more"));
			revBlocks.Add(NewPara("q2", "even more"));
			revBlocks.Add(NewSingleVersePara(4));
			var bookScript = new BookScript("REV", revBlocks, ScrVers.English);
			var list = bookScript.GetBlocksForVerse(15, 3).ToList();
			Assert.AreEqual(3, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_VerseNumberPrecededBySquareBracket_ReturnsBlocksForVerse()
		{
			var blocks = new List<Block>();
			blocks.Add(NewChapterBlock(8));
			blocks.Add(NewSingleVersePara(36, "Ka guwoto kacel i yo, ci guo ka ma pii tye iye. Laco ma gikolo owacci, "));
			blocks.Add(NewBlock("“Nen pii doŋ ene! Gin aŋo ma geŋa limo batija?” "));
			Block expected;
			blocks.Add(expected = NewSingleVersePara(37, "Ci Pilipo owacce ni, "));
			expected.BlockElements.Insert(0, new ScriptText("[ "));
			blocks.Add(NewBlock("“Ka iye ki cwinyi ducu ci itwero.” "));
			blocks.Add(NewBlock("En odok iye ni, "));
			blocks.Add(NewBlock("“An aye Yecu Kricito, ni en Wod pa Lubaŋa.”] "));
			var bookScript = new BookScript("ACT", blocks, ScrVers.English);
			var list = bookScript.GetBlocksForVerse(8, 37).ToList();
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual(expected, list[0]);
		}

		[Test]
		public void GetBlocksForVerse_VerseZero_ReturnsBlocksForVerse()
		{
			var blocks = new List<Block>();
			blocks.Add(NewChapterBlock(4, "ROM"));
			blocks.Add(NewSingleVersePara(1, "Verse one."));
			blocks.Add(NewSingleVersePara(20, "Verse twenty"));
			blocks.Add(NewChapterBlock(5, "ROM"));
			blocks.Add(new Block("d", 5) {BlockElements = new List<BlockElement>(new BlockElement[] {new ScriptText("(Taema nota pebëxëpanayo.)")})});
			blocks.Add(NewSingleVersePara(1, "Ka guwoto kacel i yo, ci guo ka ma pii tye iye. Laco ma gikolo owacci, "));
			blocks.Add(NewBlock("“Nen pii doŋ ene! Gin aŋo ma geŋa limo batija?” "));
			var bookScript = new BookScript("ROM", blocks, ScrVers.English);
			var list = bookScript.GetBlocksForVerse(5, 0).ToList();
			Assert.AreEqual("(Taema nota pebëxëpanayo.)", list.Single().GetText(true));
		}
		#endregion

		#region GetScriptBlocks Tests
		[Test]
		public void GetScriptBlocks_NoJoining_GetsEntireBlocksList()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, null);
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks)); // Same as above but proves that false is the default
		}

		[Test]
		public void GetCloneWithJoinedBlocks_ConsecutiveNarratorBlocksInTheSameParagraph_ResultsIncludeJoinedBlocks()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1, "MRK"));

			var block = NewSingleVersePara(1).AddVerse(2);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);
			m_curSetupVerse = 2;

			block = NewBlock(" «Sons of Thunder»");
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			block = NewBlock(" the rest of verse 2. ");
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.Delivery = ""; // There was a bug in which null and blank delivery were not considered the same, thus causing blocks not to combine
			mrkBlocks.Add(block.AddVerse(3));

			block = NewSingleVersePara(4).AddVerse(5);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var result = bookScript.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.IsTrue(result[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(result[1].IsParagraphStart);
			var textOfFirstVerseBlock = result[1].GetText(true);
			Assert.IsTrue(textOfFirstVerseBlock.StartsWith("{1}\u00A0"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains("{2}\u00A0"));
			Assert.IsTrue(textOfFirstVerseBlock.Contains(" «Sons of Thunder» the rest of verse 2. {3}\u00A0"));
			Assert.IsTrue(result[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(result[2].IsParagraphStart);
			Assert.IsTrue(result[2].GetText(true).StartsWith("{4}\u00A0"));
		}

		[Test]
		public void GetCloneWithJoinedBlocks_ConsecutiveBcBlocksInDifferentParagraphs_ResultsNotJoined()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewTitleBlock("The Gospel According to Mark", "MRK"));
			mrkBlocks.Add(NewChapterBlock(1, "MRK"));

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var result = bookScript.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
		}

		[TestCase(ScrVersType.Original)]
		[TestCase(ScrVersType.RussianOrthodox)]
		public void GetCloneWithJoinedBlocks_VariousVersifications_VersificationCloned(ScrVersType versType)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewTitleBlock("The Gospel According to Mark", "MRK"));
			mrkBlocks.Add(NewChapterBlock(1, "MRK"));

			var bookScript = new BookScript("MRK", mrkBlocks, new ScrVers(versType));
			var clone = bookScript.GetCloneWithJoinedBlocks(false);
			Assert.AreEqual(versType, clone.Versification.Type);
		}

		[TestCase(" ")]
		[TestCase("")]
		public void GetCloneWithJoinedBlocks_VernacularContainsQBlocks_VernacularBlocksBySameSpeakerCombined(string trailingWhitespace)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Peter said, ");
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("«This is line 1," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("This is line 2," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("This is line 3," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("This is line 4.»"));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("«This is line 1, This is line 2, This is line 3, This is line 4.»",
				result[1].GetText(true));
		}

		[TestCase(" ")]
		[TestCase("")]
		public void GetCloneWithJoinedBlocks_VernacularContainsQBlocks_VernacularBlocksForDifferentVersesNotCombined(string trailingWhitespace)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Peter said, ");
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("«This is line 1," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("This is line 2," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("This is line 3," + trailingWhitespace));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("This is line 4.»"));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("«This is line 1, This is line 2,", result[1].GetText(true).TrimEnd());
			Assert.AreEqual("{2}\u00A0This is line 3, This is line 4.»", result[2].GetText(true));
		}

		[Test]
		public void GetCloneWithJoinedBlocks_VernacularContainsQBlocksIntroducedByPBlock_AllVernacularBlocksCombined()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Peter said, ");
			block.CharacterId = narrator;
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = narrator };
			block.BlockElements.Add(new ScriptText("'This is line 1, "));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = narrator };
			block.BlockElements.Add(new ScriptText("'This is line 2, "));
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = narrator };
			block.BlockElements.Add(new ScriptText("'This is line 3, "));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = narrator };
			block.BlockElements.Add(new ScriptText("'This is line 4, "));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("{1}\u00A0Peter said, 'This is line 1, 'This is line 2, 'This is line 3, 'This is line 4, ",
				result[0].GetText(true));
		}

		[Test]
		public void GetCloneWithJoinedBlocks_VernacularContainsQBlocksWithReferenceTextBlocks_VernacularBlocksCombined()
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "John said, ");
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(NewBlock("rt0"));
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("'This is line 1, "));
			block.SetMatchedReferenceBlock(NewBlock("rt1"));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("'This is line 2, "));
			block.SetMatchedReferenceBlock(NewBlock("rt2"));
			vernacularBlocks.Add(block);
			block = new Block("q1", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("'This is line 3, "));
			block.SetMatchedReferenceBlock(NewBlock("rt3"));
			vernacularBlocks.Add(block);
			block = new Block("q2", m_curSetupChapter, m_curSetupVerse) { CharacterId = "Peter" };
			block.BlockElements.Add(new ScriptText("'This is line 4, "));
			block.SetMatchedReferenceBlock(NewBlock("rt4"));
			vernacularBlocks.Add(block);
			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));
			Assert.AreEqual("rt0", result[0].GetPrimaryReferenceText());
			Assert.AreEqual("rt1 rt2 rt3 rt4", result[1].GetPrimaryReferenceText());
		}

		[TestCase(".", "...")]
		[TestCase(".", "\u2026")]
		[TestCase(",", "...")]
		[TestCase(",", "\u2026")]
		public void GetCloneWithJoinedBlocks_PrimaryReferenceTextStartsWithElipsesBeforeVerse_BlocksCombined(string vernBlock1EndingPunctuation, string ellipsis)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock("{1} This is the genealogy of Jesus.");
			vernacularBlocks.Add(block);
			block = NewSingleVersePara(2, "Abraham fue el primero.");
			block.BlockElements.Insert(0, new ScriptText("starting with the first patriarch: "));
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(ellipsis + "{2} Abraham was first.");
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation +
				" starting with the first patriarch: {2}\u00A0Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual("{1}\u00A0This is the genealogy of Jesus. {2}\u00A0Abraham was first.", result[0].GetPrimaryReferenceText());
		}

		[TestCase(ScrVersType.English, "PSA", 82, 1, TextExplicitOverrides.All, -1, ExpectedResult = "narrator-PSA")]
		[TestCase(ScrVersType.RussianOrthodox, "PSA", 81, 1, TextExplicitOverrides.All, -1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.English, "PSA", 82, 1, TextExplicitOverrides.All, 1, ExpectedResult = "narrator-PSA")]
		[TestCase(ScrVersType.RussianOrthodox, "PSA", 81, 1, TextExplicitOverrides.All, 1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.English, "PSA", 82, 1, TextExplicitOverrides.None, -1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.English, "PSA", 82, 1, TextExplicitOverrides.Some, -1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.RussianOrthodox, "PSA", 81, 1, TextExplicitOverrides.None, -1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.RussianOrthodox, "PSA", 81, 1, TextExplicitOverrides.Some, -1, ExpectedResult = "Asaph")]
		[TestCase(ScrVersType.English, "PRO", 17, 14, TextExplicitOverrides.All, -1, ExpectedResult = "Solomon, king")]
		[TestCase(ScrVersType.English, "PRO", 30, 2, TextExplicitOverrides.All, -1, ExpectedResult = "narrator-PRO")]
		[TestCase(ScrVersType.English, "PRO", 30, 3, TextExplicitOverrides.All, 0, ExpectedResult = "narrator-PRO")]
		[TestCase(ScrVersType.Vulgate, "SNG", 1, 2, TextExplicitOverrides.All, -1, ExpectedResult = "narrator-SNG")]
		[TestCase(ScrVersType.English, "PRO", 17, 14, TextExplicitOverrides.All, 1, ExpectedResult = "Solomon, king")]
		[TestCase(ScrVersType.Vulgate, "SNG", 1, 2, TextExplicitOverrides.All, 1, ExpectedResult = "narrator-SNG")]
		[TestCase(ScrVersType.English, "PRO", 17, 14, TextExplicitOverrides.None, -1, ExpectedResult = "Solomon, king")]
		[TestCase(ScrVersType.English, "PRO", 17, 14, TextExplicitOverrides.Some, -1, ExpectedResult = "Solomon, king")]
		[TestCase(ScrVersType.Vulgate, "SNG", 1, 2, TextExplicitOverrides.None, -1, ExpectedResult = "beloved")]
		[TestCase(ScrVersType.Vulgate, "SNG", 1, 2, TextExplicitOverrides.Some, -1, ExpectedResult = "beloved")]
		[TestCase(ScrVersType.English, "NEH", 13, 6, TextExplicitOverrides.All, -1, ExpectedResult = "narrator-NEH")]
		[TestCase(ScrVersType.English, "NEH", 13, 31, TextExplicitOverrides.None, -1, ExpectedResult = "Nehemiah")]
		public string GetCharacterIdInScript_ResultDependsOnWhetherBlocksAlreadyHaveCharacterSpeakingExplicitlyInAllVersesInOverride(
			ScrVersType versType, string bookId, int chapter, int verse, TextExplicitOverrides option, int missingVerseOffset)
		{
			var testBook = GetTestBook(versType, bookId, option, missingVerseOffset);
			var bookNum = testBook.BookNumber;
			var block = testBook.GetFirstBlockForVerse(chapter, verse);
			if (option == TextExplicitOverrides.None || option == TextExplicitOverrides.Some)
			{
				var vref = new VerseRef(bookNum, chapter, verse, testBook.Versification);
				vref.ChangeVersification(ScrVers.English);
				var overrideInfo = NarratorOverrides.GetCharacterOverrideDetailsForRefRange(vref, vref.VerseNum).Single();
				var overrideStartRef = new VerseRef(bookNum, overrideInfo.StartChapter, overrideInfo.StartVerse);
				overrideStartRef.ChangeVersification(testBook.Versification);
				var overrideEndRef = new VerseRef(bookNum, overrideInfo.EndChapter, overrideInfo.EndVerse);
				overrideEndRef.ChangeVersification(testBook.Versification);
				for (var c = overrideStartRef.ChapterNum; c <= overrideEndRef.ChapterNum; c++)
				{
					var startVerse = (c == overrideStartRef.ChapterNum) ? overrideStartRef.VerseNum : 1;
					var endVerse = (c == overrideEndRef.ChapterNum) ? overrideEndRef.VerseNum : testBook.Versification.GetLastVerse(bookNum, c);
					var expectedCharacter = overrideInfo.Character;
					if (startVerse == 1 && bookId == "PSA" && versType == ScrVersType.RussianOrthodox)
					{
						// If this verse 1 is actually the Hebrew subtitle (verse "0" in the English numbering system),
						// then it's a special case and we don't want it to get overridden.
						var psalmVerse1Ref = new VerseRef(bookNum, c, 1, testBook.Versification);
						psalmVerse1Ref.ChangeVersification(ScrVers.English);
						if (psalmVerse1Ref.VerseNum == 0)
							expectedCharacter = testBook.NarratorCharacterId;
					}
					for (int v = startVerse; v <= endVerse; v++)
					{
						Assert.True(testBook.GetBlocksForVerse(c, v).All(b => testBook.GetCharacterIdInScript(b) == expectedCharacter),
							$"Unexpected character in {bookId} {c}:{v} (expected: {expectedCharacter})");
					}
				}
			}
			return testBook.GetCharacterIdInScript(block);
		}

		[Test]
		public void GetCharacterIdInScript_OverrideRangeStartsInBlock2InPreviousChapter_GetsOverrideCharacter()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(2, "NEH"),
				new Block("p", 2, 1) {CharacterId = narrator}
					.AddVerse(1, "During Nisan, in the 20th year of Artaxerxes, I gave him his wine. I'd not been sad in his presence. ")
					.AddVerse(2, "he said,"),
				new Block("p", 2, 2) {CharacterId = "Artaxerxes, king of Persia"}.AddText("«What's wrong? This is sorrow of heart.»"),
				new Block("p", 2, 2) {CharacterId = narrator}.AddText("So I freaked.").AddVerse(3, "I replied:"),
				new Block("p", 2, 3) {CharacterId = "Nehemiah"}.AddText("«Don't die! Why shouldn't I be sad, when my fathers' city is a hot mess?»"),
				new Block("p", 2, 4) {CharacterId = narrator}.AddVerse(4, "So the king's like,"),
				new Block("p", 2, 4) {CharacterId = "Artaxerxes, king of Persia"}.AddText("«What do you want me to do about it?»"),
				new Block("p", 2, 4) {CharacterId = narrator}.AddText("So I prayed.").AddVerse(5, "I retorted,"),
				new Block("p", 2, 5) {CharacterId = "Nehemiah"}.AddText("«If it ain't too much trouble, let me go fix it.»"),
				new Block("p", 2, 6) {CharacterId = narrator}.AddVerse(6, "What with the queeny-poo being right there, the king's goes,"),
				new Block("p", 2, 6) {CharacterId = "Artaxerxes, king of Persia"}.AddText("«Are you going to be gone like forever?»"),
				new Block("p", 2, 6) {CharacterId = narrator}.AddText("So he was cool with my plan.").AddVerse(7, "And then I go:"),
				new Block("p", 2, 7) {CharacterId = "Nehemiah"}.AddText("«Don't forget to write!")
					.AddVerse(8, "And tell Asaph to make sure I can get my hands on some decent timber.»"),
				new Block("p", 2, 8) {CharacterId = narrator}.AddText("And that's basically how it went down, thanks to God's help."),
				new Block("p", 2, 9) {CharacterId = narrator}
					.AddVerse(9, "I came to the guys beyond the river and gave them the letters. The king had sent along a whole army.")
					.AddVerse(10, "But, oh boy, were Sanballat and Tobiah ticked!")
			};
			var bookOfNehemiah = new BookScript("NEH", blocks, ScrVers.English);
			foreach (var block in bookOfNehemiah.GetScriptBlocks().Where(b => b.CharacterId == narrator))
			{
				Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(block), block.ToString(true, "NEH"));
			}
		}

		[Test]
		public void GetCharacterIdInScript_NarratorBlockInOverrideRangeCoversMultipleVerses_GetsOverrideCharacter()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(2, "NEH"),
				new Block("p", 2, 2) {CharacterId = "Artaxerxes, king of Persia"}.AddVerse(2, "«What's wrong? This is sorrow of heart.»"),
				new Block("p", 2, 2) {CharacterId = narrator}.AddText("So I freaked.").AddVerse(3, "I replied:"),
				new Block("p", 2, 3) {CharacterId = "Nehemiah"}.AddText("«Don't die! Why shouldn't I be sad, when my fathers' city is a hot mess?»"),
				new Block("p", 2, 4) {CharacterId = narrator}.AddVerse(4, "So the king's like,"),
				new Block("p", 2, 4) {CharacterId = "Artaxerxes, king of Persia"}.AddText("«What do you want me to do about it?»"),
				new Block("p", 2, 4) {CharacterId = narrator}.AddText("So I prayed.").AddVerse(5, "I retorted,"),
				new Block("p", 2, 5) {CharacterId = "Nehemiah"}.AddText("«If it ain't too much trouble, let me go fix it.»"),
				new Block("p", 2, 6) {CharacterId = narrator}.AddVerse(6, "What with the queeny-poo being right there, the king's goes,"),
				new Block("p", 2, 6) {CharacterId = "Artaxerxes, king of Persia"}.AddText("«Are you going to be gone like forever?»"),
				new Block("p", 2, 6) {CharacterId = narrator}.AddText("So he was cool with my plan.").AddVerse(7, "And then I go:"),
				new Block("p", 2, 7) {CharacterId = "Nehemiah"}.AddText("«Don't forget to write!")
					.AddVerse(8, "And tell Asaph to make sure I can get my hands on some decent timber.»"),
				new Block("p", 2, 8) {CharacterId = narrator}.AddText("And that's basically how it went down, thanks to God's help.")
					.AddVerse(9, "I came to the guys beyond the river and gave them the letters. The king had sent along a whole army.")
					.AddVerse(10, "But, oh boy, were Sanballat and Tobiah ticked!")
			};
			var bookOfNehemiah = new BookScript("NEH", blocks, ScrVers.English);
			foreach (var block in bookOfNehemiah.GetScriptBlocks().Where(b => b.CharacterId == narrator))
			{
				Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(block), block.ToString(true, "NEH"));
			}
		}

		[Test]
		public void GetCharacterIdInScript_OverrideRangeStartsInBlock2OfVerseNoVersesWhollyAssignedToNarrator_OverrideNotApplied()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(1, "NEH"),
				new Block("p", 1, 1) {CharacterId = narrator}.AddVerse(1, "The writings of Nehemiah bar Hacaliah."),
				new Block("p", 1, 1) {CharacterId = narrator}.AddText("It was the month Chislev, in the twentieth year of King A."),
				new Block("p", 1, 1) {CharacterId = "Nehemiah"}.AddText("I was in Shushan the palace, when along came,"),
				new Block("p", 1, 2) {CharacterId = "Nehemiah"}.AddVerse(2, "Hanani, one of my brothers. I asked him,"),
				new Block("p", 1, 2) {CharacterId = "Nehemiah"}.AddText("«How are the Jews who avoided capture in Jerusalem?»"),
				new Block("p", 1, 3) {CharacterId = narrator}.AddVerse(3, "They replied to Nehemiah,"),
				new Block("p", 1, 3) {CharacterId = "Hanani"}.AddText("«The remnant are in great affliction. Jerusalem is a hot mess!»"),
				new Block("p", 1, 4) {CharacterId = "Nehemiah"}.AddVerse(4, "When I got the news, I cried for days on end, fasting and praying."),

			};
			var bookOfNehemiah = new BookScript("NEH", blocks, ScrVers.English);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.BookOrChapter),
				bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[0]));
			Assert.AreEqual(narrator, bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[1]));
			Assert.AreEqual(narrator, bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[2]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[3]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[4]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[5]));
			Assert.AreEqual(narrator, bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[6]));
			Assert.AreEqual("Hanani", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[7]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[8]));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetCharacterIdInScript_OverrideRangeStartsInBlock2OfVerseWhichIsWhollyAssignedToNarrator_GetsOverrideCharacterButNotForBlock1(bool extraWeirdSectionHead)
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(1, "NEH"),
				new Block("p", 1, 1) {CharacterId = narrator}.AddVerse(1, "The writings of Nehemiah bar Hacaliah."),
				new Block("p", 1, 1) {CharacterId = narrator}.AddText("It was Chislev 20 and I was in Shushan the palace.")
					.AddVerse(2, "Hanani came. I asked him,"),
				new Block("p", 1, 2) {CharacterId = "Nehemiah"}.AddText("«How are the Jews who avoided capture in Jerusalem?»"),
				new Block("p", 1, 3) {CharacterId = narrator}.AddVerse(3, "And he's like,"),
				new Block("p", 1, 3) {CharacterId = "Hanani"}.AddText("«The remnant are in great affliction. Jerusalem is a hot mess!»")

			};
			if (extraWeirdSectionHead)
			{
				blocks.Insert(2, new Block("s", 2, 10)
						{ CharacterId = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.ExtraBiblical) }
					.AddText("Why is there a section head here?"));
			}
			int i = 0;
			var bookOfNehemiah = new BookScript("NEH", blocks, ScrVers.English);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.BookOrChapter),
				bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			Assert.AreEqual(narrator, bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			if (extraWeirdSectionHead)
			{
				Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.ExtraBiblical),
					bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			}
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			Assert.AreEqual("Nehemiah", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
			Assert.AreEqual("Hanani", bookOfNehemiah.GetCharacterIdInScript(bookOfNehemiah.GetScriptBlocks()[i++]));
		}

		[Test]
		public void GetCharacterIdInScript_OverrideRangeFromCh1V2ThruEndOfCh2_GetsOverrideCharacterForNarratorBlockCh2V1And2()
		{
			// <Override startChapter="1" startVerse="2" endChapter="2" character="Habakkuk"/>
			var bookId = "HAB";
			// Ensure that the current overrides are still what they were when this test was written:
			var habOverrides = NarratorOverrides.GetNarratorOverridesForBook(bookId).ToList();
			var overrideCharacter = habOverrides.Single(o => o.StartChapter == 1 && o.StartVerse == 2 &&
				o.EndChapter == 2 && o.EndVerse >= 3 && o.EndBlock == 0).Character;

			var narrator = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(2, bookId),
				new Block("p", 2, 1) {CharacterId = narrator}.AddVerse(1, "I will stand watch on the ramparts to see what he will tell me and what I will answer concerning my complaint. ")
					.AddVerse(2, "God answered me,"),
				new Block("p", 2, 2) {CharacterId = "God"}.AddText("«Write the vision on tablets, that he who runs may read it."),
				new Block("p", 2, 3) {CharacterId = "God"}.AddVerse(3, "Wait for the vision's fulfillment! It will come without delay.»")

			};
			var bookOfHabakkuk = new BookScript(bookId, blocks, ScrVers.English);
			var habScriptBlocks = bookOfHabakkuk.GetScriptBlocks();
			var chapterBlock = habScriptBlocks[0];
			Assert.AreEqual(chapterBlock.CharacterId, bookOfHabakkuk.GetCharacterIdInScript(chapterBlock));
			Assert.AreEqual(overrideCharacter, bookOfHabakkuk.GetCharacterIdInScript(habScriptBlocks[1]));
			Assert.AreEqual("God", bookOfHabakkuk.GetCharacterIdInScript(habScriptBlocks[2]));
			Assert.AreEqual("God", bookOfHabakkuk.GetCharacterIdInScript(habScriptBlocks[3]));
		}

		[TestCase("Agur")]
		[TestCase("Some other character that can't really happen")]
		public void GetCharacterIdInScript_BlockHasCharacterIdInScriptSet_GetsExistingOverrideCharacter(string overrideCharacter)
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("PRO", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(30, "PRO"),
				new Block("p", 30, 8) {CharacterId = narrator, CharacterIdOverrideForScript = overrideCharacter}
					.AddVerse(1, "Don't let me lie or steal or do something stupid."),
			};
			var bookOfProverbs = new BookScript("PRO", blocks, ScrVers.English);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("PRO", CharacterVerseData.StandardCharacter.BookOrChapter),
				bookOfProverbs.GetCharacterIdInScript(bookOfProverbs.GetScriptBlocks()[0]));
			Assert.AreEqual(overrideCharacter, bookOfProverbs.GetCharacterIdInScript(bookOfProverbs.GetScriptBlocks()[1]));
		}

		[Test]
		public void GetCharacterIdInScript_AcrosticHeading_HeadingNotOverridden()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(119, "PSA"),
				new Block("q1", 119, 8) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(8, "I will keep thy statutes;"),
				new Block("q2", 119, 8) {IsParagraphStart = true, CharacterId = narrator}.AddText("This language doesn't use periods in poetry"),
				new Block("qa", 119, 8) {IsParagraphStart = true, CharacterId = narrator}.AddText("Beth"),
				new Block("q1", 119, 9) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(9, "How shall a pipsqueak cleanse his way?"),
			};

			var bookOfPsalms = new BookScript("PSA", blocks, ScrVers.English);

			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("PSA", CharacterVerseData.StandardCharacter.BookOrChapter),
				bookOfPsalms.GetCharacterIdInScript(bookOfPsalms.GetScriptBlocks()[0]));
			Assert.AreEqual("psalmist (Aleph)", bookOfPsalms.GetCharacterIdInScript(bookOfPsalms.GetScriptBlocks()[1]));
			Assert.AreEqual("psalmist (Aleph)", bookOfPsalms.GetCharacterIdInScript(bookOfPsalms.GetScriptBlocks()[2]));
			Assert.AreEqual(narrator, bookOfPsalms.GetCharacterIdInScript(bookOfPsalms.GetScriptBlocks()[3]));
			Assert.AreEqual("psalmist (Beth)", bookOfPsalms.GetCharacterIdInScript(bookOfPsalms.GetScriptBlocks()[4]));
		}

		[Test]
		public void GetCharacterIdInScript_NoBlocksExistForVersesInStartChapterButBlocksDoExistForSubsequentChapter_OverrideApplied()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("PRO", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(1, "PRO"),
				new Block("q1", 1, 1) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(1, "The proverbs of Solomon bar David, king of Israel:"),
				new Block("q1", 1, 7) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(7, "The fear of the Lord is the start of knowledge,"),
				new Block("q2", 1, 7) {IsParagraphStart = true, CharacterId = narrator}.AddText("but fools loathe wisdom and instruction."),
				NewChapterBlock(2, "PRO"),
				new Block("q1", 2, 1) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(1, "My son, if you adopt my words"),
				new Block("q2", 2, 1) {IsParagraphStart = true, CharacterId = narrator}.AddText("and store up my commands within you,"),
				new Block("q1", 2, 2) {IsParagraphStart = true, CharacterId = narrator}.AddVerse(2, "tuning your auditory faculties to wisdom"),
				new Block("q2", 2, 2) {IsParagraphStart = true, CharacterId = narrator}.AddText("and sticking your heart to understanding—"),
			};

			var bookOfPsalms = new BookScript("PRO", blocks, ScrVers.English);

			Assert.IsTrue(bookOfPsalms.GetScriptBlocks().Where(b => !b.IsChapterAnnouncement && b.ChapterNumber == 1).All(b => bookOfPsalms.GetCharacterIdInScript(b) == narrator));
			Assert.IsTrue(bookOfPsalms.GetScriptBlocks().Where(b => !b.IsChapterAnnouncement && b.ChapterNumber == 2).All(b => bookOfPsalms.GetCharacterIdInScript(b) == "Solomon, king"));
		}

		#region Test GetCharacterIdInScript for override covering two blocks in a single verse
		/// <summary>
		/// Helper method for the tests in this region
		/// </summary>
		private BookScript GetSngChapter1Verse4(string characterC1V4B3, string characterC1V4B4)
		{
			// <Override startChapter="1" startVerse="4" startBlock="3" endVerse="4" endBlock="4" character="maidens"/>
			const string bookId = "SNG";
			// Ensure that the current overrides are still what they were when this test was written:
			var sngOverrides = NarratorOverrides.GetNarratorOverridesForBook(bookId).ToList();
			var overrideCharacter = sngOverrides.Single(o => o.StartChapter == 1 && o.StartVerse == 4 &&
				o.StartBlock == 3 && o.EndVerse == 4 && o.EndBlock == 4).Character;
			Assert.AreEqual("maidens", overrideCharacter);

			var narrator = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(1, bookId),
				new Block("q1", 1, 4) {CharacterId = "Solomon, king"}.AddVerse(4, "You are sweet!"),
				new Block("q1", 1, 4) {CharacterId = narrator}.AddText("Then the maidens said:"),
				new Block("q1", 1, 4) {CharacterId = characterC1V4B3}.AddText("You are one amazing guy."),
				new Block("q1", 1, 4) {CharacterId = characterC1V4B4}.AddText("And that ain't no lie!"),
				new Block("q1", 1, 4) {CharacterId = narrator}.AddText("So spake they."),
			};
			var bookOfSongs = new BookScript(bookId, blocks, ScrVers.English);
			return bookOfSongs;
		}

		[TestCase("narrator-SNG", "narrator-SNG")]
		[TestCase("beloved", "maidens")]
		[TestCase("maidens", "Solomon, king")]
		[TestCase("maidens", "maidens")]
		[TestCase("beloved", "Solomon, king")]
		public void GetCharacterIdInScript_OverrideTwoBlocksInVerse_NeitherBlockExplicitlyAssignedToOverrideCharacter_GetsOverrideCharacterForNarratorBlocksInCoveredRange(
			string characterC1V4B3, string characterC1V4B4)
		{
			// SETUP
			// <Override startChapter="1" startVerse="4" startBlock="3" endVerse="4" endBlock="4" character="maidens"/>
			var bookOfSongs = GetSngChapter1Verse4(characterC1V4B3, characterC1V4B4);
			var narrator = bookOfSongs.NarratorCharacterId;
			var sngScriptBlocks = bookOfSongs.GetScriptBlocks();
			Assert.AreEqual(characterC1V4B3, sngScriptBlocks[3].CharacterId, "Ensure test setup was correct");
			Assert.AreEqual(characterC1V4B4, sngScriptBlocks[4].CharacterId, "Ensure test setup was correct");

			// Verify preceding blocks
			Assert.AreEqual("Solomon, king", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[1]));
			Assert.AreNotEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[2]));

			// SUT: Covered narrator block(s) should get overridden
			if (sngScriptBlocks[3].CharacterId == narrator)
				Assert.AreEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[3]));
			if (sngScriptBlocks[4].CharacterId == narrator)
				Assert.AreEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[4]));
			
			// Verify following block
			Assert.AreNotEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[5]));
		}

		[TestCase("narrator-SNG", "maidens")]
		[TestCase("maidens", "narrator-SNG")]
		public void GetCharacterIdInScript_OverrideCoversTwoBlocksInVerse_OneBlockExplicitlyAssignedToOverrideCharacter_OverrideNotApplied(
			string characterC1V4B3, string characterC1V4B4)
		{
			// SETUP
			// <Override startChapter="1" startVerse="4" startBlock="3" endVerse="4" endBlock="4" character="maidens"/>
			var bookOfSongs = GetSngChapter1Verse4(characterC1V4B3, characterC1V4B4);
			var sngScriptBlocks = bookOfSongs.GetScriptBlocks();
			Assert.AreEqual(characterC1V4B3, sngScriptBlocks[3].CharacterId, "Ensure test setup was correct");
			Assert.AreEqual(characterC1V4B4, sngScriptBlocks[4].CharacterId, "Ensure test setup was correct");

			// Verify preceding blocks
			Assert.AreEqual("Solomon, king", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[1]));
			Assert.AreNotEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[2]));

			// SUT: Neither of these two covered blocks should get overridden
			Assert.AreEqual(sngScriptBlocks[3].CharacterId, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[3]));
			Assert.AreEqual(sngScriptBlocks[4].CharacterId, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[4]));

			// Verify following block
			Assert.AreNotEqual("maidens", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[5]));
		}
		#endregion

		[Test]
		public void GetCharacterIdInScript_OverrideChangeFromBlock1ToBlock2_GetsCorrectOverrideCharacterForEachBlock()
		{
			// <Override startChapter="2" startVerse="3" endVerse="10" endBlock="1" character="beloved"/>
			// <Override startChapter="2" startVerse="10" startBlock="2" endVerse="15" character="Solomon, king"/>
			const string bookId = "SNG";
			// Ensure that the current overrides are still what they were when this test was written:
			var sngOverrides = NarratorOverrides.GetNarratorOverridesForBook(bookId).ToList();
			var overrideCharacter1 = sngOverrides.Single(o => o.EndChapter == 2 && o.EndVerse == 10 && o.EndBlock == 1).Character;
			var overrideCharacter2 = sngOverrides.Single(o => o.StartChapter == 2 && o.StartVerse == 10 && o.StartBlock == 2).Character;
			Assert.AreNotEqual(overrideCharacter1, overrideCharacter2);
			Assert.IsFalse(sngOverrides.Any(o => o.StartChapter == 7 && o.StartVerse == 9));

			var narrator = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(2, bookId),
				new Block("p", 2, 3) {CharacterId = narrator}.AddVerse(3, "I am a beloved person, because my king says: "),
				new Block("p", 2, 3) {CharacterId = "Solomon, king"}.AddText("You are sweet!").AddVerse(4).AddVerse(5).AddVerse(6),
				new Block("p", 2, 6) {CharacterId = narrator}.AddText("Thus he spoke.").AddVerse(7, "He also said:"),
				new Block("p", 2, 7) {CharacterId = "Solomon, king"}.AddText("Start of God's speech in v. 7...").AddVerse(8),
				new Block("p", 2, 9) {CharacterId = narrator}.AddVerse(9).AddVerse(10),
				new Block("p", 2, 10) {CharacterId = narrator}.AddText("Part II of v. 10")
			};
			var bookOfSongs = new BookScript(bookId, blocks, ScrVers.English);
			var sngScriptBlocks = bookOfSongs.GetScriptBlocks();
			Assert.AreEqual(blocks[0].CharacterId, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[0]));
			Assert.AreEqual(overrideCharacter1, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[1]));
			Assert.AreEqual("Solomon, king", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[2]));
			Assert.AreEqual(overrideCharacter1, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[3]));
			Assert.AreEqual("Solomon, king", bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[4]));
			Assert.AreEqual(overrideCharacter1, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[5]));
			Assert.AreEqual(overrideCharacter2, bookOfSongs.GetCharacterIdInScript(sngScriptBlocks[6]));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetCharacterIdInScript_OverrideRangeEndsInBlock1OfVerse_AllVersesHaveExplicitAssignment_NoOverrideApplied(bool extraWeirdSectionHead)
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(2, "HAB"),
				new Block("p", 2, 10) {CharacterId = narrator}.AddVerse(10, "God said:"),
				new Block("p", 2, 10) {CharacterId = "God"}.AddText("This is important").AddVerse(11).AddVerse(12).AddVerse(13).AddVerse(14),
				new Block("p", 2, 14) {CharacterId = narrator}.AddText("Thus he spoke.").AddVerse(15, "Again he said:"),
				new Block("p", 2, 15) {CharacterId = "God"}.AddText("Start of God's speech in v. 15...").AddVerse(16),
				new Block("p", 2, 17) {CharacterId = "God"}.AddVerse(17).AddVerse(18).AddVerse(19),
				new Block("p", 2, 19) {CharacterId = narrator}.AddText("Part II of v. 19")
			};
			if (extraWeirdSectionHead)
			{
				blocks.Insert(2, new Block("s", 2, 10)
					{ CharacterId = CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.ExtraBiblical)}
					.AddText("Why is there a section head here?"));
			}

			int i = 0;
			var bookOfHabakkuk = new BookScript("HAB", blocks, ScrVers.English);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.BookOrChapter),
				bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			Assert.AreEqual(narrator, bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			if (extraWeirdSectionHead)
			{
				Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("HAB", CharacterVerseData.StandardCharacter.ExtraBiblical),
					bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			}
			Assert.AreEqual("God", bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			Assert.AreEqual(narrator, bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			Assert.AreEqual("God", bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			Assert.AreEqual("God", bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
			Assert.AreEqual(narrator, bookOfHabakkuk.GetCharacterIdInScript(bookOfHabakkuk.GetScriptBlocks()[i++]));
		}

		[Test]
		public void GetCharacterIdInScript_OverrideSpecifiesStartAndEndBlock_ReturnValueDependsOnBlock()
		{
			// <Override startChapter="6" startVerse="13" endVerse="13" endBlock="2" character="maidens"/>
			// <Override startChapter="6" startVerse="13" startBlock="3" endChapter="7" endVerse="9" endBlock="1" character="Solomon, king"/>
			// Note: No override for 7:9b or 7:9c
			// <Override startChapter="7" startVerse="10" endChapter="8" endVerse="4" character="beloved"/>
			const string bookId = "SNG";
			// Ensure that the current overrides are still what they were when this test was written:
			var sngOverrides = NarratorOverrides.GetNarratorOverridesForBook(bookId).ToList();
			var overrideCharacter = sngOverrides.Single(o => o.StartChapter == 6 && o.StartVerse == 13 && o.StartBlock == 3 &&
				o.EndChapter == 7 && o.EndVerse == 9 && o.EndBlock == 1).Character;
			var precedingOverrideCharacter = sngOverrides.Single(o => o.EndChapter == 6 && o.EndVerse == 13 && o.EndBlock == 2).Character;
			Assert.AreNotEqual(overrideCharacter, precedingOverrideCharacter);
			Assert.IsFalse(sngOverrides.Any(o => o.StartChapter == 7 && o.StartVerse == 9));

			var testBlocks = GetTestBlocks(ScrVers.English, bookId, TextExplicitOverrides.None);
			var iBlockFor6_13 = testBlocks.IndexOf(b => b.ChapterNumber == 6 && b.BlockElements.OfType<Verse>().LastOrDefault()?.Number == "13");
			Assert.AreEqual(7, testBlocks[iBlockFor6_13 + 1].ChapterNumber, "Test set-up sanity check");
			testBlocks.Insert(iBlockFor6_13 + 1, new Block("q2", 6, 13, 13)
			{
				CharacterId = testBlocks[iBlockFor6_13].CharacterId,
				BlockElements = new List<BlockElement>(new [] {new ScriptText("return, return, that we may look upon you.") })
			});
			testBlocks.Insert(iBlockFor6_13 + 2, new Block("q1", 6, 13, 13)
			{
				CharacterId = testBlocks[iBlockFor6_13].CharacterId,
				BlockElements = new List<BlockElement>(new[] { new ScriptText("Why should you look upon the Shulammite,") })
			});
			testBlocks.Insert(iBlockFor6_13 + 3, new Block("q2", 6, 13, 13)
			{
				CharacterId = testBlocks[iBlockFor6_13].CharacterId,
				BlockElements = new List<BlockElement>(new[] { new ScriptText("as upon a dance before two armies?") })
			});
			var iBlockFor7_9 = testBlocks.IndexOf(b => b.ChapterNumber == 7 && b.BlockElements.OfType<Verse>().FirstOrDefault()?.Number == "9");
			Assert.AreEqual(10, testBlocks[iBlockFor7_9 + 1].InitialStartVerseNumber, "Test set-up sanity check");
			testBlocks.Insert(iBlockFor7_9 + 1, new Block("q1", 7, 9, 9)
			{
				CharacterId = testBlocks[iBlockFor7_9].CharacterId,
				BlockElements = new List<BlockElement>(new[] { new ScriptText("It goes down smoothly for my beloved,") })
			});
			testBlocks.Insert(iBlockFor7_9 + 2, new Block("q2", 7, 9, 9)
			{
				CharacterId = testBlocks[iBlockFor7_9].CharacterId,
				BlockElements = new List<BlockElement>(new[] { new ScriptText("gliding over lips and teeth.") })
			});
			var testBook = new BookScript(bookId, testBlocks, ScrVers.English);
			Assert.AreEqual(precedingOverrideCharacter, testBook.GetCharacterIdInScript(testBook[iBlockFor6_13]));
			Assert.AreEqual(precedingOverrideCharacter, testBook.GetCharacterIdInScript(testBook[iBlockFor6_13 + 1]));
			Assert.AreEqual(overrideCharacter, testBook.GetCharacterIdInScript(testBook[iBlockFor6_13 + 2]));
			Assert.AreEqual(overrideCharacter, testBook.GetCharacterIdInScript(testBook[iBlockFor6_13 + 3]));
			Assert.AreEqual(overrideCharacter, testBook.GetCharacterIdInScript(testBook[iBlockFor7_9]));
			Assert.AreEqual(testBook.NarratorCharacterId, testBook.GetCharacterIdInScript(testBook[iBlockFor7_9 + 1]));
			Assert.AreEqual(testBook.NarratorCharacterId, testBook.GetCharacterIdInScript(testBook[iBlockFor7_9 + 2]));
		}

		[Test]
		public void GetCloneWithJoinedBlocks_ApplyingNarratorOverrides_AdjacentSentencesInSameParagraphAndVerse_BlocksCombinedWithOverrideCharacter()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block>
			{
				NewChapterBlock(1, "NEH"),
				new Block("p", 1, 1) {CharacterId = narrator}.AddVerse(1, "The writings of Nehemiah bar Hacaliah:"),
				new Block("p", 1, 1) {CharacterId = narrator}.AddText("It was the month Chislev, in the twentieth year of King Arty;"),
				new Block("q", 1, 1) {CharacterId = narrator}.AddText("I was in Shushan the palace, when along came"),
				new Block("p", 1, 2) {CharacterId = narrator}.AddVerse(2, "Hanani, one of my brothers. I asked him,"),
				new Block("q", 1, 2) {CharacterId = "Nehemiah"}.AddText("«How are the Jews who avoided capture in Jerusalem?»"),
				new Block("p", 1, 3) {CharacterId = narrator}.AddVerse(3, "He said,"),
				new Block("p", 1, 3) {CharacterId = "Hanani"}.AddText("«The remnant are in great affliction. Jerusalem is a hot mess!»"),
				new Block("p", 1, 4) {CharacterId = narrator}.AddVerse(4, "When I got the news, I cried for days on end, fasting and praying."),

			};
			var bookOfNehemiah = new BookScript("NEH", blocks, ScrVers.English);

			var result = bookOfNehemiah.GetCloneWithJoinedBlocks(true).GetScriptBlocks();
			Assert.AreEqual(7, result.Count);

			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("NEH", CharacterVerseData.StandardCharacter.BookOrChapter), result[0].CharacterIdInScript);
			Assert.AreEqual(narrator, result[1].CharacterIdInScript);
			Assert.AreEqual("Nehemiah", result[2].CharacterIdInScript);
			Assert.AreEqual("It was the month Chislev, in the twentieth year of King Arty; I was in Shushan the palace, when along came", result[2].GetText(true));
			Assert.AreEqual("Nehemiah", result[3].CharacterIdInScript);
			Assert.AreEqual("{2}\u00A0Hanani, one of my brothers. I asked him, «How are the Jews who avoided capture in Jerusalem?»", result[3].GetText(true));
			Assert.AreEqual("Nehemiah", result[4].CharacterIdInScript);
			Assert.AreEqual("{3}\u00A0He said,", result[4].GetText(true));
			Assert.AreEqual("Hanani", result[5].CharacterIdInScript);
			Assert.AreEqual("Nehemiah", result[6].CharacterIdInScript);
		}

		[TestCase(".", "p", "{1} This is not empty.", " ", "{1}\u00A0This is not empty.")]
		[TestCase("?", "li", " ", "{1} Neither is this.", "{1}\u00A0Neither is this.")]
		[TestCase("!", "q1", "{1}", "The other block's ref text was just a verse number.", "{1}\u00A0The other block's ref text was just a verse number.")]
		public void GetCloneWithJoinedBlocks_WithBlankReferenceTexts_AdjacentSentencesInSameParagraphAndVerse_BlocksCombined(string vernBlock1EndingPunctuation,
			string styleTag, string block1RefText, string block2RefText, string expectedCombinedRefText)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.StyleTag = styleTag;
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block1RefText);
			vernacularBlocks.Add(block);
			block = new Block(styleTag, m_curSetupChapter, 1); // IsParaStart == false by default.
			block.BlockElements.Add(new ScriptText("Abraham fue el primero."));
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block2RefText);
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation.TrimEnd() +
				" Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual(expectedCombinedRefText, result[0].GetPrimaryReferenceText());
		}

		[TestCase(";", "p", "{1} This is not empty, ", "okay?", "{1}\u00A0This is not empty, okay?")]
		[TestCase(",", "pi2", "This is just another way", " to say that.", "This is just another way to say that.")]
		[TestCase("", "q1", "{1}My soul will rejoice. Yes, it will sing;", "Because it is happy!", "{1}\u00A0My soul will rejoice. Yes, it will sing; Because it is happy!")]
		public void GetCloneWithJoinedBlocks_WithReferenceTextsInSameVerseAndSentence_BlocksCombined(string vernBlock1EndingPunctuation,
			string block2StyleTag, string block1RefText, string block2RefText, string expectedCombinedRefText)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block1RefText);
			vernacularBlocks.Add(block);
			block = new Block(block2StyleTag, m_curSetupChapter, 1);
			if (block2StyleTag != "p")
				block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Abraham fue el primero."));
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block2RefText);
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation.TrimEnd() +
				" Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual(expectedCombinedRefText, result[0].GetPrimaryReferenceText());
		}

		[TestCase(".", "q", " ", "This is not empty.")]
		[TestCase(".", "q", "{1}", "This is not empty.")]
		[TestCase("!", "q1", "{F8 SFX--Whatever}", "This is not empty...")]
		[TestCase("!", "pi1", "This is fine", "and\u2026dandy.")]
		[TestCase("?", "q2", "{1}\u00A0Freaky,", "isn't it?")]
		public void GetCloneWithJoinedBlocks_WithReferenceTexts_VernBlockEndsInSentenceEndingPunctuation_BlocksNotCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
			string block1RefText, string block2RefText)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block1RefText);
			var origRefText1 = block.GetPrimaryReferenceText();
			vernacularBlocks.Add(block);
			block = new Block(block2StyleTag, m_curSetupChapter, 1);
			if (block2StyleTag != "p")
				block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Abraham fue el primero."));
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block2RefText);
			var origRefText2 = block.GetPrimaryReferenceText();
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(origRefText1, result[0].GetPrimaryReferenceText());
			Assert.AreEqual(origRefText2, result[1].GetPrimaryReferenceText());
		}

		[TestCase(",", "q", " ", "This is not empty.")]
		[TestCase(",", "q", "{1}", "This is not empty.")]
		[TestCase(";", "q1", "{F8 SFX--Whatever}", "This is not empty...")]
		[TestCase("", "p", "This is not empty.", " ")]
		[TestCase(";", "pi1", "This is fine", "and\u2026dandy.")]
		[TestCase(" - ", "q2", "{1}\u00A0Freaky,", "isn't it?")]
		public void GetCloneWithJoinedBlocks_WithReferenceTexts_BlockBHasVerseNumber_BlocksNotCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
			string block1RefText, string block2RefText)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block1RefText);
			var origRefText1 = block.GetPrimaryReferenceText();
			vernacularBlocks.Add(block);
			block = NewSingleVersePara(2, "Abraham fue el primero.");
			if (block2StyleTag == "p")
				block.IsParagraphStart = false;
			else
				block.StyleTag = block2StyleTag;
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block2RefText);
			var origRefText2 = block.GetPrimaryReferenceText();
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(origRefText1, result[0].GetPrimaryReferenceText());
			Assert.AreEqual(origRefText2, result[1].GetPrimaryReferenceText());
		}

		[Test]
		public void GetCloneWithJoinedBlocks_SingleVoiceWithReferenceTexts_BlocksCombinedByOrigParagraphAndReferenceTextIgnored()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			var jude = testProject.IncludedBooks.Single();
			var countOfOrigParagraphs = jude.GetScriptBlocks().Count(b => b.IsParagraphStart || CharacterVerseData.IsCharacterOfType(b.CharacterId, CharacterVerseData.StandardCharacter.BookOrChapter));
			foreach (var block in jude.GetScriptBlocks())
				block.SetMatchedReferenceBlock("blah");
			jude.SingleVoice = true;

			var result = jude.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(countOfOrigParagraphs, result.Count);
			Assert.IsTrue(result.All(b => b.CharacterIsStandard));
			Assert.IsFalse(result.Any(b => b.MatchesReferenceText));

			// Verify that the original book was not modified
			Assert.IsTrue(jude.GetScriptBlocks().All(b => b.MatchesReferenceText));
			Assert.IsTrue(jude.GetScriptBlocks().All(b => b.GetPrimaryReferenceText() == "blah"));
		}

		// PG-1334
		[TestCase(true)]
		[TestCase(false)]
		public void GetCloneWithJoinedBlocks_SingleVoiceStandardCharacterBlocks_DifferentStandardCharacterBlocksNotCombined(bool paragraphStart)
		{
			var sectionHead = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var intro = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Intro);
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var vernacularBlocks = new List<Block>();
			vernacularBlocks.Add(NewTitleBlock("The Gospel According to Saint Matthew", "MAT"));
			// The case where paragraphStart is false is somewhat contrived. Any intro paragraph should have IsParagraphStart true
			// (and would most likely end in sentence-ending punctuation).
			vernacularBlocks.Add(new Block("ip", 1) { IsParagraphStart = paragraphStart, CharacterId = intro });
			vernacularBlocks.Last().BlockElements.Add(new ScriptText("Matthew's gospel: Jesus the Messianic King"));
			vernacularBlocks.Add(NewChapterBlock(1, "MAT"));
			vernacularBlocks.Add(new Block("ms", 1) { IsParagraphStart = true, CharacterId = sectionHead });
			vernacularBlocks.Last().BlockElements.Add(new ScriptText("INTRODUCTION"));
			// Again the case where paragraphStart is false is somewhat contrived. Should have IsParagraphStart true.
			vernacularBlocks.Add(new Block("s", 1) { IsParagraphStart = paragraphStart, CharacterId = sectionHead });
			vernacularBlocks.Last().BlockElements.Add(new ScriptText("Royal Genealogy of Jesus"));
			vernacularBlocks.Add(NewSingleVersePara(1, "Este es la genealogia de Jesus:"));
			vernacularBlocks.Last().CharacterId = narrator;
			// This one is NOT contrived. This should be true, but the bug in this case is that some paragraph-start
			// blocks are incorrectly missing this flag.
			vernacularBlocks.Last().IsParagraphStart = paragraphStart;
			vernacularBlocks.Add(NewPara("q", "Abraham fue el primero."));
			vernacularBlocks.Last().CharacterId = narrator;
			vernacularBlocks.Last().IsParagraphStart = true;
			var countOfOrigParagraphs = vernacularBlocks.Count;

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);
			vernBook.SingleVoice = true;

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			int iBlock = 0;
			Assert.AreEqual(result[iBlock], result.Single(b => b.CharacterIs("MAT", CharacterVerseData.StandardCharacter.BookOrChapter) &&
				!b.IsChapterAnnouncement));
			Assert.AreEqual("The Gospel According to Saint Matthew", ((ScriptText)result[iBlock].BlockElements.Single()).Content);
			Assert.AreEqual("Matthew's gospel: Jesus the Messianic King", ((ScriptText)result[++iBlock].BlockElements.Single()).Content);
			Assert.AreEqual("ip", result[iBlock].StyleTag);
			Assert.IsTrue(result[++iBlock].IsChapterAnnouncement);
			if (paragraphStart)
			{
				Assert.AreEqual("INTRODUCTION", ((ScriptText)result[++iBlock].BlockElements.Single()).Content);
				Assert.AreEqual("Royal Genealogy of Jesus", ((ScriptText)result[++iBlock].BlockElements.Single()).Content);
			}
			else
			{
				Assert.AreEqual("INTRODUCTION Royal Genealogy of Jesus", ((ScriptText)result[++iBlock].BlockElements.Single()).Content);
			}

			var firstVerse = result[++iBlock];
			Assert.IsTrue(firstVerse.StartsAtVerseStart);
			Assert.AreEqual(1, firstVerse.InitialStartVerseNumber);
			Assert.AreEqual("p", firstVerse.StyleTag);
			Assert.AreEqual("Este es la genealogia de Jesus: Abraham fue el primero.", ((ScriptText)firstVerse.BlockElements[1]).Content);
			Assert.IsTrue(result.All(b => b.CharacterIsStandard));
			Assert.AreEqual(iBlock + 1, result.Count);
		}

		[TestCase(";", "q1", "", "Part 2.", "Part 2.")]
		[TestCase(" - ", "p", "{1}   ", "Part 2", "{1}\u00A0Part 2")]
		[TestCase(",", "pi1", "{1} Part 1.", " ", "{1}\u00A0Part 1.")]
		[TestCase(",", "m", "Part 1", "", "Part 1")]
		public void GetCloneWithJoinedBlocks_PrimaryReferenceTextIsBlank_BlocksCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
			string block1RefText, string block2RefText, string expectedCombinedRefText)
		{
			var vernacularBlocks = new List<Block>();
			var block = NewSingleVersePara(1, "Este es la genealogia de Jesus" + vernBlock1EndingPunctuation);
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block1RefText);
			vernacularBlocks.Add(block);
			block = NewPara(block2StyleTag, "Abraham fue el primero.");
			if (block2StyleTag == "p")
				block.IsParagraphStart = false;
			block.CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			block.SetMatchedReferenceBlock(block2RefText);
			vernacularBlocks.Add(block);

			var vernBook = new BookScript("MAT", vernacularBlocks, ScrVers.English);

			var origblock1RefText = vernBook.GetScriptBlocks()[0].GetPrimaryReferenceText();
			var origblock2RefText = vernBook.GetScriptBlocks()[1].GetPrimaryReferenceText();

			var result = vernBook.GetCloneWithJoinedBlocks(false).GetScriptBlocks();
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation.TrimEnd() +
				" Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual(expectedCombinedRefText, result[0].GetPrimaryReferenceText());
			// Finally, ensure that joining of reference texts did not alter the reference text of the original vernacular blocks
			// because if it did, then when we save, that will get stored and can lead to the bug PG-1291.
			Assert.AreEqual(origblock1RefText, vernBook.GetScriptBlocks()[0].GetPrimaryReferenceText());
			Assert.AreEqual(origblock2RefText, vernBook.GetScriptBlocks()[1].GetPrimaryReferenceText());
		}
		#endregion

		#region ApplyUserDecisions Character Assignments Tests
		[Test]
		public void ApplyUserDecisions_NoUserDecisionsOrSplitsInSource_NoChangesInTarget()
		{
			var source = CreateStandardMarkScript();
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);
			Assert.IsFalse(target.GetScriptBlocks().Any(b => b.UserConfirmed));
			Assert.IsTrue(target.GetScriptBlocks().All(b => b.CharacterIsStandard || b.CharacterId == CharacterVerseData.kUnexpectedCharacter));
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_UserDecisionsInSourceVerseNumbersAndTextMatchExactly_DecisionsGetCopiedToTarget()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Fred the Frog", target[i].CharacterId);
					Assert.AreEqual("with certitude", target[i].Delivery);
					Assert.IsTrue(target[i].UserConfirmed);
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserDecisionsInSourceSomeTextDoesNotMatchExactly_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					if (i % 2 == 0)
					{
						int iText = source[i].BlockElements.FindIndex(e => e is ScriptText);
						source[i].BlockElements[iText] = new ScriptText("This is not a match.");
					}
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					if (i % 2 == 0)
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter);
						Assert.IsNull(target[i].Delivery);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInTarget_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript(true);
			var quoteBlockIndices = new List<int>();
			int iBlockAtVerse7 = -1;
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
					quoteBlockIndices.Add(i);

				if (target[i].InitialStartVerseNumber == 7 && iBlockAtVerse7 < 0)
					iBlockAtVerse7 = i;  // this block has extra verses
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(iBlockAtVerse7 > 0);
			int iLastBlock = target.GetScriptBlocks().Count - 1; // this block has extra verses
			Assert.IsTrue(iLastBlock > iBlockAtVerse7);
			var indicesOfQuoteBlocksWithExtraVerses = new[] { iBlockAtVerse7, iLastBlock };
			Assert.IsFalse(indicesOfQuoteBlocksWithExtraVerses.Except(quoteBlockIndices).Any());

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (indicesOfQuoteBlocksWithExtraVerses.Contains(i))
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInTarget_BookTitleIncluded_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript(false, true);
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript(true, true);
			var quoteBlockIndices = new List<int>();
			int iBlockAtVerse7 = -1;
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
					quoteBlockIndices.Add(i);

				if (target[i].InitialStartVerseNumber == 7 && iBlockAtVerse7 < 0)
					iBlockAtVerse7 = i;  // this block has extra verses
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(iBlockAtVerse7 > 0);
			int iLastBlock = target.GetScriptBlocks().Count - 1; // this block has extra verses
			Assert.IsTrue(iLastBlock > iBlockAtVerse7);
			var indicesOfQuoteBlocksWithExtraVerses = new[] { iBlockAtVerse7, iLastBlock };
			Assert.IsFalse(indicesOfQuoteBlocksWithExtraVerses.Except(quoteBlockIndices).Any());

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (indicesOfQuoteBlocksWithExtraVerses.Contains(i))
					{
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_ExtraVersesInSource_DecisionsGetCopiedToTargetOnlyForExactMatches()
		{
			var source = CreateStandardMarkScript(true);
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Fred the Frog";
					source[i].Delivery = "with certitude";
					source[i].UserConfirmed = true;
				}
			}

			var target = CreateStandardMarkScript();
			var quoteBlockIndices = new List<int>();
			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (target[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					quoteBlockIndices.Add(i);
					if (i % 2 != 0)
					{
						int iText = target[i].BlockElements.FindIndex(e => e is ScriptText);
						target[i].BlockElements[iText] = new ScriptText("This is not a match.");
					}
				}
			}
			Assert.IsTrue(quoteBlockIndices.Count > 0);
			Assert.IsTrue(quoteBlockIndices.Count < source.GetScriptBlocks().Count(b => b.CharacterId == "Fred the Frog"));

			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (quoteBlockIndices.Contains(i))
				{
					if (i % 2 != 0)
					{
						Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, target[i].CharacterId);
						Assert.Null(target[i].Delivery);
						Assert.IsFalse(target[i].UserConfirmed);
					}
					else
					{
						Assert.AreEqual("Fred the Frog", target[i].CharacterId);
						Assert.AreEqual("with certitude", target[i].Delivery);
						Assert.IsTrue(target[i].UserConfirmed);
					}
				}
				else
				{
					Assert.IsTrue(target[i].CharacterIsStandard);
					Assert.IsNull(target[i].Delivery);
					Assert.IsFalse(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserConfirmedMultiCharacterIdWithoutCharacterInScriptSet_CharacterInScriptGetsSetToCurrentDefault()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Thomas/James";
					Assert.AreEqual(source[i].CharacterId, source[i].CharacterIdInScript, "CharacterId setter should not set CharacterInScript to default character");
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 0);
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Thomas/James", target[i].CharacterId);
					Assert.AreEqual("Thomas", target[i].CharacterIdInScript);
					Assert.IsTrue(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_UserConfirmedMultiCharacterIdWithCharacterInScriptSet_CharacterInScriptGetsCopied()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].CharacterId == CharacterVerseData.kUnexpectedCharacter)
				{
					source[i].CharacterId = "Thomas/James";
					source[i].CharacterIdInScript = "James";
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Any());
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.AreEqual("Thomas/James", target[i].CharacterId);
					Assert.AreEqual("James", target[i].CharacterIdInScript);
					Assert.IsTrue(target[i].UserConfirmed);
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_AlignedToReferenceText_ReferenceTextBlocksCloned()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count; i++)
			{
				if (source[i].InitialStartVerseNumber % 2 == 1)
				{
					if (source[i].CharacterIsUnclear)
						source[i].CharacterId = "Jesus";
					var refBlock = source[i].SetMatchedReferenceBlock(new String(source[i].BlockElements.OfType<ScriptText>().First().Content.Reverse().ToArray()));
					if (i < 20)
						refBlock.SetMatchedReferenceBlock("{19} Whatever");
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Any());
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.IsFalse(target[i].CharacterIsUnclear);
					Assert.IsTrue(target[i].UserConfirmed);
					Assert.IsTrue(target[i].MatchesReferenceText);
					Assert.AreNotEqual(target[i].ReferenceBlocks.Single(), source[i].ReferenceBlocks.Single());
					Assert.AreEqual(new String(source[i].BlockElements.OfType<ScriptText>().First().Content.TrimEnd().Reverse().ToArray()), target[i].GetPrimaryReferenceText());
					if (i < 20)
					{
						var refBlock = target[i].ReferenceBlocks.Single();
						Assert.IsTrue(refBlock.MatchesReferenceText);
						Assert.AreEqual("{19}\u00A0Whatever", refBlock.GetPrimaryReferenceText());
						Assert.AreNotEqual(refBlock.ReferenceBlocks.Single(), source[i].ReferenceBlocks.Single().ReferenceBlocks.Single());
					}
				}
			}
		}

		[Test]
		public void ApplyUserDecisions_MatchesReferenceTextTrueButNoReferenceBlock_IgnoredWithoutCrashing()
		{
			var source = CreateStandardMarkScript();
			var userConfirmedCharacterBlockIndices = new List<int>();
			for (int i = 0; i < source.GetScriptBlocks().Count && userConfirmedCharacterBlockIndices.Count < 6; i++)
			{
				if (source[i].InitialStartVerseNumber % 2 == 1)
				{
					if (source[i].CharacterIsUnclear)
						source[i].CharacterId = "Jesus";
					var refBlock = source[i].SetMatchedReferenceBlock(new String(source[i].BlockElements.OfType<ScriptText>().First().Content.Reverse().ToArray()));
					if (userConfirmedCharacterBlockIndices.Count >= 2)
						refBlock.SetMatchedReferenceBlock("{19} Whatever");
					source[i].UserConfirmed = true;
					userConfirmedCharacterBlockIndices.Add(i);
				}
			}
			Assert.IsTrue(userConfirmedCharacterBlockIndices.Count > 3, "oops - need to adjust test setup");
			// These next two lines set up the bad data condition that we want to be able to tolerate without crashing:
			source[userConfirmedCharacterBlockIndices[0]].ReferenceBlocks.Clear();
			source[userConfirmedCharacterBlockIndices[2]].ReferenceBlocks[0].ReferenceBlocks.Clear();
			var target = CreateStandardMarkScript();
			target.ApplyUserDecisions(source);

			for (int i = 0; i < target.GetScriptBlocks().Count; i++)
			{
				if (userConfirmedCharacterBlockIndices.Contains(i))
				{
					Assert.IsFalse(target[i].CharacterIsUnclear);
					Assert.IsTrue(target[i].UserConfirmed);
					if (i == userConfirmedCharacterBlockIndices[0])
					{
						Assert.IsFalse(target[i].MatchesReferenceText);
					}
					else
					{
						Assert.IsTrue(target[i].MatchesReferenceText);
						Assert.AreNotEqual(target[i].ReferenceBlocks.Single(), source[i].ReferenceBlocks.Single());
						Assert.AreEqual(new String(source[i].BlockElements.OfType<ScriptText>().First().Content.TrimEnd().Reverse().ToArray()), target[i].GetPrimaryReferenceText());
					}
					if (i >= userConfirmedCharacterBlockIndices[2])
					{
						var refBlock = target[i].ReferenceBlocks.Single();
						if (i == userConfirmedCharacterBlockIndices[2])
						{
							Assert.IsFalse(refBlock.MatchesReferenceText);
							Assert.AreEqual(0, refBlock.ReferenceBlocks.Count);
						}
						else
						{
							Assert.IsTrue(refBlock.MatchesReferenceText);
							Assert.AreEqual("{19}\u00A0Whatever", refBlock.GetPrimaryReferenceText());
							Assert.AreNotEqual(refBlock.ReferenceBlocks.Single(), source[i].ReferenceBlocks.Single().ReferenceBlocks.Single());
						}
					}
				}
			}
		}
		#endregion

		#region ApplyUserDecisions Block Splits Tests
		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, "1", 5);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		#region PG-1168: Splits and reference-text alignments in rainbow mode.
		[Test]
		public void ApplyUserDecisions_SplitBlockInChunkAlignedToReferenceText_TextChanged_SplitNotAppliedAndNoException()
		{
			var source = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, 26));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var blockToSplit = source.GetIndexOfFirstBlockForVerse(15, 26);
			source.SplitBlock(source.GetScriptBlocks()[blockToSplit], "26", 14);

			var target = CreateStandard1CorinthiansScript(ScrVers.English);
			var scriptTextForV26 = target.GetScriptBlocks().First(b => b.ChapterNumber == 15 && b.InitialStartVerseNumber <= 26 && b.LastVerseNum >= 26).BlockElements
				.SkipWhile(e => !(e is Verse) || ((Verse)e).Number != "26").OfType<ScriptText>().First();
			scriptTextForV26.Content = scriptTextForV26.Content.Replace('a', 'q');
			var countOfTargetBlocksBeforeApplyingSplits = target.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingUserDecisions = target.GetScriptBlocks();
			Assert.AreEqual(countOfTargetBlocksBeforeApplyingSplits, targetBlocksAfterApplyingUserDecisions.Count);
			Assert.IsFalse(source.GetScriptBlocks().SequenceEqual(targetBlocksAfterApplyingUserDecisions, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
		}

		// 1CO 15
		// Verse 1: "Alai naeng ma pabotohononku tu. "
		// Verse 2: "Laos i do parhiteanmuna gabe malua. "
		// Verse 26: "Hamatean i do musu na parpudi sipasohotonna i. "
		// Verse 27: "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi. "
		[TestCase(1, 14, k1Co15V1Text)]
		[TestCase(2, 5, k1Co15V2Text)]
		[TestCase(26, 14, k1Co15V26Text)]
		[TestCase(27, 44, k1Co15V27Text)]
		public void ApplyUserDecisions_UnalignedSplitBlockInChunkPreviouslyAlignedToReferenceText_TextUnchanged_ManualSplitAndRefTextAlignmentsApplied(int verseNum,
			int splitPos, string fullVerseText)
		{
			var source = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			
			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, verseNum));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;

			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, verseNum);
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], verseNum.ToString(), splitPos);

			var target = CreateStandard1CorinthiansScript(ScrVers.English);

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();

			var comparer = new BlockComparer();
			Assert.IsTrue(source.GetScriptBlocks().SequenceEqual(targetBlocksAfterApplyingSplit, comparer));
			var firstBlockOfSplit = targetBlocksAfterApplyingSplit.First(b => b.ChapterNumber == 15 && b.LastVerseNum == verseNum);
			Assert.AreEqual(fullVerseText.Substring(0, splitPos), ((ScriptText)firstBlockOfSplit.BlockElements.Last()).Content);
			var lastBlockOfSplit = targetBlocksAfterApplyingSplit.Last(b => b.ChapterNumber == 15 && b.InitialStartVerseNumber == verseNum);
			Assert.AreEqual(fullVerseText.Substring(splitPos), ((ScriptText)lastBlockOfSplit.BlockElements.First()).Content);

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// 1CO 15
		[TestCase(3)]
		[TestCase(4)]
		public void ApplyUserDecisions_UnalignedSplitBlockInChunkPreviouslyAlignedToReferenceText_TextChangedInLaterVerse_ManualSplitAndRefTextAlignmentsInUnchangedVersesApplied(int verseToChange)
		{
			// Verse 2: "Laos i do parhiteanmuna gabe malua. "
			const int kVerseNumToSplit = 2;
			const int kSplitPos = 7;
			var source = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			
			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, kVerseNumToSplit));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;

			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, kVerseNumToSplit);
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], kVerseNumToSplit.ToString(), kSplitPos);

			var target = CreateStandard1CorinthiansScript(ScrVers.English);

			var iChangedBlock = source.GetIndexOfFirstBlockForVerse(15, verseToChange);
			var textToChange = (ScriptText)target.GetFirstBlockForVerse(15, verseToChange).BlockElements
				.SkipWhile(be => !(be is Verse) || ((Verse)be).StartVerse != verseToChange).Skip(1).First();
			textToChange.Content = "Then this wobbly frog bounced under her chimney and correspondingly fried a spoon to my top ear molecule.";

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();

			var comparer = new BlockComparer();
			Assert.IsTrue(source.GetScriptBlocks().Take(iBlockToSplit).SequenceEqual(targetBlocksAfterApplyingSplit.Take(iBlockToSplit), comparer));
			Assert.IsTrue(source.GetScriptBlocks().SkipWhile(b => b.ChapterNumber < 16)
				.SequenceEqual(targetBlocksAfterApplyingSplit.SkipWhile(b => b.ChapterNumber < 16), comparer));
			var firstBlockOfSplit = targetBlocksAfterApplyingSplit.First(b => b.ChapterNumber == 15 && b.LastVerseNum == kVerseNumToSplit);
			Assert.AreEqual(k1Co15V2Text.Substring(0, kSplitPos), ((ScriptText)firstBlockOfSplit.BlockElements.Last()).Content);
			var lastBlockOfSplit = targetBlocksAfterApplyingSplit.Last(b => b.ChapterNumber == 15 && b.InitialStartVerseNumber == kVerseNumToSplit);
			Assert.AreEqual(k1Co15V2Text.Substring(kSplitPos), ((ScriptText)lastBlockOfSplit.BlockElements.First()).Content);

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// 1CO 15
		// Verse 1: "Alai naeng ma pabotohononku tu. "
		// Verse 2: "Laos i do parhiteanmuna gabe malua. "
		// Verse 26: "Hamatean i do musu na parpudi sipasohotonna i. "
		// Verse 27: "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi. "
		[TestCase(1, 14, k1Co15V1Text)]
		[TestCase(2, 5, k1Co15V2Text)]
		[TestCase(26, 14, k1Co15V26Text)]
		[TestCase(27, 44, k1Co15V27Text)]
		public void ApplyUserDecisions_MatchupWithUserSplitSubsequentlyAlignedToReferenceText_TextUnchanged_SplitAndAlignmentsApplied(int verseNum,
			int splitPos, string fullVerseText)
		{
			var source = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, verseNum);
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], verseNum.ToString(), splitPos);

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, verseNum));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			foreach (var block in matchup.CorrelatedBlocks.SkipWhile(b => b.ChapterNumber < 15 || b.InitialStartVerseNumber < verseNum).Skip(1).TakeWhile(b => b.SplitId == 0 || b.IsContinuationOfPreviousBlockQuote))
			{
				block.CharacterId = "Walter";
				block.UserConfirmed = true;
				block.MultiBlockQuote = MultiBlockQuote.None;
			}
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText + 1, source.GetScriptBlocks().Count);

			var target = CreateStandard1CorinthiansScript(ScrVers.English);

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.IsTrue(source.GetScriptBlocks().SequenceEqual(targetBlocksAfterApplyingSplit, new BlockComparer()));

			foreach (var block in targetBlocksAfterApplyingSplit.Where(b => b.ChapterNumber == 15 && b.IsScripture))
				Assert.IsTrue(block.MatchesReferenceText, $"Target block {block} does not match ref text.");

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// 1CO 15:26
		// Verse 26: "Hamatean i do musu na parpudi sipasohotonna i. "
		// Offset:              1         2         3         4         5         6         7         8          9        10        11        12        13        14        15
		// Offset:    012345678901234567890123456789012345678901234567890123456789012345678901234567890012345678901234567890123456789012345678901234567890123456789012345678901234567890
		// Split:                                                 |
		// Verse 27: "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi. "
		[Test]
		public void ApplyUserDecisions_AllSourceBlocksAlignedToReferenceTextWithManualSplitInMiddleOfVerse_TextUnchanged_SplitAppliedAndAllBlocksAligned()
		{
			var origSource = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = origSource.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(origSource);
			var sourceBlocksChunkedOut = origSource.GetScriptBlocks();
			var source = new BookScript(origSource.BookId, sourceBlocksChunkedOut, origSource.Versification);
			Assert.IsTrue(origBlockCount < sourceBlocksChunkedOut.Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, 27);
			var firstBlockforV27 = source.GetScriptBlocks()[iBlockToSplit];
			var v27SplitPos = 44;
			source.SplitBlock(firstBlockforV27, "27", v27SplitPos, true, origSource.NarratorCharacterId);

			firstBlockforV27.SetNonDramaticCharacterId(CharacterVerse.kScriptureCharacter);
			firstBlockforV27.UserConfirmed = true;

			var blockForV26 = source.GetScriptBlocks()[iBlockToSplit];
			Assert.AreEqual("26", ((Verse)blockForV26.BlockElements.First()).Number);
			source.SplitBlock(blockForV26, "26", k1Co15V26Text.Length, true, CharacterVerse.kScriptureCharacter).UserConfirmed = true;

			foreach (var sb in source.GetScriptBlocks().Where(b => b.CharacterIsUnclear))
			{
				sb.MultiBlockQuote = MultiBlockQuote.None;
				sb.SetNonDramaticCharacterId(origSource.NarratorCharacterId);
			}

			var target = CreateStandard1CorinthiansScript(origSource.Versification);
			var countOfSourceBlocks = source.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(countOfSourceBlocks, source.GetScriptBlocks().Count, "Sanity check");
			Assert.AreEqual(countOfSourceBlocks, targetBlocksAfterApplyingSplit.Count);
			var blockWithVerse26 = targetBlocksAfterApplyingSplit.Single(b => b.ChapterNumber == 15 && b.LastVerseNum == 26);
			Assert.AreEqual(k1Co15V26Text, ((ScriptText)blockWithVerse26.BlockElements.Last()).Content);
			var iBlockEndingInV26 = targetBlocksAfterApplyingSplit.IndexOf(blockWithVerse26);
			Assert.AreEqual("{27}\u00A0" + k1Co15V27Text.Substring(0, v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].CharacterId);
			Assert.AreEqual(k1Co15V27Text.Substring(v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2].GetText(true));
			Assert.IsTrue(targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2].CharacterId == origSource.NarratorCharacterId);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// 1CO 15:26
		// Verse 26: "Hamatean i do musu na parpudi sipasohotonna i. "
		// Offset:              1         2         3         4         5         6         7         8          9        10        11        12        13        14        15
		// Offset:    012345678901234567890123456789012345678901234567890123456789012345678901234567890012345678901234567890123456789012345678901234567890123456789012345678901234567890
		// Split:                                                 |
		// Verse 27: "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi. "
		[Test]
		public void ApplyUserDecisions_SplitBetweenTwoVersesChunkedOutByReferenceTextAndInMiddleOfSecondVerse_TextUnchanged_SplitsApplied()
		{
			var source = CreateStandard1CorinthiansScript(ScrVers.English);
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var blockForV26 = source.GetScriptBlocks()[source.GetIndexOfFirstBlockForVerse(15, 26)];
			source.SplitBlock(blockForV26, "26", k1Co15V26Text.Length, true, CharacterVerse.kScriptureCharacter)
				.UserConfirmed = true;

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, 26));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText + 1, source.GetScriptBlocks().Count);

			Assert.IsTrue(origBlockCount < source.GetScriptBlocks().Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, 27);
			var v27SplitPos = 44;
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], "27", v27SplitPos, true,
				source.NarratorCharacterId);

			var target = CreateStandard1CorinthiansScript(ScrVers.English);
			var countOfSourceBlocks = source.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(countOfSourceBlocks, targetBlocksAfterApplyingSplit.Count);
			var blockWithVerse26 = targetBlocksAfterApplyingSplit.Single(b => b.ChapterNumber == 15 && b.LastVerseNum == 26);
			Assert.AreEqual(k1Co15V26Text, ((ScriptText)blockWithVerse26.BlockElements.Last()).Content);
			var iBlockEndingInV26 = targetBlocksAfterApplyingSplit.IndexOf(blockWithVerse26);
			Assert.AreEqual("{27}\u00A0" + k1Co15V27Text.Substring(0, v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].CharacterId);
			Assert.AreEqual(k1Co15V27Text.Substring(v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2].GetText(true));
			Assert.IsTrue(targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2]
				.CharacterIs(source.BookId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// MAT 21:31
		[Test]
		public void ApplyUserDecisions_SplitBetweenTwoVersesChunkedOutByReferenceTextAndMultipleSplitsInFollowingVerse_TextUnchanged_MatchesAndSplitsApplied()
		{
			const int kChapter = 21;
			var blocks = new List<Block> {
				NewChapterBlock(kChapter, "MAT"),
				new Block("p", kChapter, 30)
					.AddVerse(30, "Ia amaama i ditopot ma sianggian i, laos songon i do hatana, gabe didok i ma mangalusi: Olo, tuan! Hape, ndang saut laho. ")
					// Offset:               1         2         3         4         5         6         7         8          9        10        11        12        13        14        15
					// Offset:     012345678901234567890123456789012345678901234567890123456789012345678901234567890012345678901234567890123456789012345678901234567890123456789012345678901234567890
					// Splits:                                                                 |                              |              |                                |
					.AddVerse(31, "Ise sian nasida na dua i mangulahon pinangido ni amanasida? Jadi didok nasida ma tu Ibana: Sihahaan i do! Dung i didok Jesus ma tu nasida: Situtu do na hudok on tu hamu: Tagonan do halak sijalobeo dohot boru na jahat bongot tu harajaon ni Debata, unang hamu! ")
					.AddVerse(32, "Ai na ro do si Johannes tu hamu di dalan hatigoran, ndang dihaporseai hamu ibana; alai anggo angka sijalobeo dohot angka boru na jahat porsea do. Diida hamu do i nian; laos tong so disolsoli hamu rohamuna, laho mangkaporseai ibana. ") };
			blocks.Last().CharacterId = CharacterVerseData.kAmbiguousCharacter;
			const int kSplitPos1 = 60;
			const int kSplitPos2 = 90;
			const int kSplitPos3 = 105;
			const int kSplitPos4 = 138;
			var origLengthOfVerse31 = blocks.Last().BlockElements.OfType<ScriptText>().ElementAt(1).Content.Length;

			var source = new BookScript("MAT", blocks, ScrVers.English);
			var target = source.Clone();
			Assert.AreEqual(ScrVers.English, target.Versification);

			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(source);
			var sourceBlocksChunkedOut = source.GetScriptBlocks();
			Assert.IsTrue(origBlockCount < sourceBlocksChunkedOut.Count);

			var iBlockV30 = source.GetIndexOfFirstBlockForVerse(kChapter, 30);
			Assert.AreEqual(1, iBlockV30);
			var blockV30 = source.GetScriptBlocks()[iBlockV30];
			Assert.AreEqual("30", blockV30.BlockElements.OfType<Verse>().Single().Number);
			blockV30.CharacterId = blockV30.ReferenceBlocks.Single().CharacterId;

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(kChapter, 31);
			Assert.AreEqual(2, iBlockToSplit);
			var blockToSplit = source.GetScriptBlocks()[iBlockToSplit];
			var narrator = source.NarratorCharacterId;

			var split4 = source.SplitBlock(blockToSplit, "31", kSplitPos4, true, "Jesus");
			split4.SetMatchedReferenceBlock("“Most certainly I tell you that the tax collectors and the prostitutes are entering into the Kingdom of God before you.");
			split4.UserConfirmed = true;

			var split3 = source.SplitBlock(blockToSplit, "31", kSplitPos3, true, narrator);
			split3.SetMatchedReferenceBlock("Jesus said to them,");
			split3.UserConfirmed = true;

			var split2 = source.SplitBlock(blockToSplit, "31", kSplitPos2, true, "chief priests/elders");
			split2.SetMatchedReferenceBlock("“The first.”");
			split2.UserConfirmed = true;

			var split1 = source.SplitBlock(blockToSplit, "31", kSplitPos1, true, narrator);
			split1.SetMatchedReferenceBlock("They said to him,");
			split1.UserConfirmed = true;

			blockToSplit.SetCharacterIdAndCharacterIdInScript("Jesus", source.BookNumber, ScrVers.English);
			blockToSplit.SetMatchedReferenceBlock("{31}Which one of the two did the will of his father?”");
			// In the actual data that this test is based on, neither this block, nor the ones for vv. 30 or 32 had
			// UserConfirmed set to true. (By contrast, in the above test for 1CO 15:26-26, all three blocks involved in
			// the split DID have UserConfirmed set to true in the actual data.) I'm not sure the implementation is really
			// 100% logivcal, but if blocks in the vernacular match automatically to the reference text and already have
			// the correct character ID (because there was only one possiblity in the control file), then we don't mark
			// them as user-confirmed when the user clicks apply.

			var blockV32 = source.GetScriptBlocks()[source.GetIndexOfFirstBlockForVerse(kChapter, 32)];
			Assert.AreEqual("32", blockV32.BlockElements.OfType<Verse>().Single().Number);
			blockV32.CharacterId = blockV32.ReferenceBlocks.Single().CharacterId;

			Assert.AreEqual(split4.SplitId, blockToSplit.SplitId);
			Assert.AreEqual(split3.SplitId, blockToSplit.SplitId);
			Assert.AreEqual(split2.SplitId, blockToSplit.SplitId);
			Assert.AreEqual(split1.SplitId, blockToSplit.SplitId);

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(source.GetScriptBlocks().Count, targetBlocksAfterApplyingSplit.Count,
				"There were 4 explicit user splits, dividing v. 31 into 5 parts, plus the preceding verse and the following verse " +
				"were split by and matched to the reference text. Therefore, what started as one block should now be seven blocks.");
			var blockVerse31Start = targetBlocksAfterApplyingSplit.Single(b => b.BlockElements.OfType<Verse>().Any(v => v.Number == "31"));
			Assert.AreEqual(kSplitPos1, blockVerse31Start.BlockElements.OfType<ScriptText>().Last().Content.Length);
			Assert.IsTrue(blockVerse31Start.MatchesReferenceText);
			Assert.AreEqual("{31}\u00A0Which one of the two did the will of his father?”", blockVerse31Start.GetPrimaryReferenceText());

			var iBlock = targetBlocksAfterApplyingSplit.IndexOf(blockVerse31Start);

			var block = targetBlocksAfterApplyingSplit[iBlock - 1];
			Assert.AreEqual("{30}\u00A0Ia amaama i ditopot ma sianggian i, laos songon i do hatana, gabe didok i ma mangalusi: Olo, tuan! Hape, ndang saut laho. ",
				block.GetText(true));
			Assert.AreEqual("Jesus", block.CharacterId);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("{30}\u00A0He came to the second, and said the same thing. He answered, ‹I go, sir,› but he didn’t go.",
				block.ReferenceBlocks.Single().GetText(true));

			block = targetBlocksAfterApplyingSplit[++iBlock];
			Assert.AreEqual(kSplitPos2 - kSplitPos1, ((ScriptText)block.BlockElements.Single()).Content.Length);
			Assert.IsTrue(block.CharacterIs(source.BookId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("They said to him,", block.ReferenceBlocks.Single().GetText(true));

			block = targetBlocksAfterApplyingSplit[++iBlock];
			Assert.AreEqual(kSplitPos3 - kSplitPos2, ((ScriptText)block.BlockElements.Single()).Content.Length);
			Assert.AreEqual("chief priests/elders", block.CharacterId);
			Assert.AreEqual("Good Priest", block.CharacterIdInScript);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("“The first.”", block.ReferenceBlocks.Single().GetText(true));

			block = targetBlocksAfterApplyingSplit[++iBlock];
			Assert.AreEqual(kSplitPos4 - kSplitPos3, ((ScriptText)block.BlockElements.Single()).Content.Length);
			Assert.IsTrue(block.CharacterIs(source.BookId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("Jesus said to them,", block.ReferenceBlocks.Single().GetText(true));

			block = targetBlocksAfterApplyingSplit[++iBlock];
			Assert.AreEqual(origLengthOfVerse31 - kSplitPos4, block.BlockElements.OfType<ScriptText>().First().Content.Length);
			Assert.AreEqual("Jesus", block.CharacterId);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("“Most certainly I tell you that the tax collectors and the prostitutes are entering into the Kingdom of God before you.",
				block.ReferenceBlocks.Single().GetText(true));

			block = targetBlocksAfterApplyingSplit[++iBlock];
			Assert.AreEqual("{32}\u00A0Ai na ro do si Johannes tu hamu di dalan hatigoran, ndang dihaporseai hamu ibana; alai anggo angka sijalobeo dohot angka boru na jahat porsea do. Diida hamu do i nian; laos tong so disolsoli hamu rohamuna, laho mangkaporseai ibana. ",
				block.GetText(true));
			Assert.AreEqual("{32}\u00A0For John came to you in the way of righteousness, and you didn’t believe him, but the tax collectors and the prostitutes believed him. When you saw it, you didn’t even repent afterward, that you might believe him.",
				block.ReferenceBlocks.Single().GetText(true));

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// MRK 8:26
		[Test]
		public void ApplyUserDecisions_SplitInMiddleAndEndOfLastVerseInParaChunkedOutByReferenceText_FirstSplitApplied()
		{
			var blocks = new List<Block> {
				NewChapterBlock(8, "MRK"),
				new Block("p", 8, 25)
					.AddVerse(25, "Torang do saluhutna diida. ")
					// Offset:               1         2         3         4         5         6
					// Offset:     0123456789012345678901234567890123456789012345678901234567890123456789
					// Splits:                                                    |
					.AddVerse(26, "Laos disuru ma ibana muli tu jabuna, didok ma: Unang bongoti huta i! "),
				NewSingleVersePara(27, "Dung i borhat ma Jesus rap dohot siseanna i tu angka huta mardonokkon Kaesarea Pilippi...", "MRK")
				};
			const int kSplitPos = 47;
			var origLengthOfVerse26 = blocks[1].BlockElements.OfType<ScriptText>().ElementAt(1).Content.Length;
			blocks[1].CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);

			var source = new BookScript("MRK", blocks, ScrVers.English);
			var target = source.Clone();

			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(source);
			var sourceBlocksChunkedOut = source.GetScriptBlocks();
			Assert.IsTrue(origBlockCount < sourceBlocksChunkedOut.Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(8, 26);
			Assert.AreEqual(2, iBlockToSplit);
			var blockToSplit = source.GetScriptBlocks()[iBlockToSplit];

			var split = source.SplitBlock(blockToSplit, "26", kSplitPos, true, "Jesus");
			split.SetMatchedReferenceBlock("“Don't enter into the village, nor tell anyone in the village.”");
			split.Delivery = "giving orders";
			split.UserConfirmed = true;

			// This next line is really unnecessary, but it matches the real data. We won't check that this gets
			// reset because we won't expect v. 26 to get broken out. Glyssen will be able to re-apply this
			// on the fly (I think).
			blockToSplit.SetMatchedReferenceBlock("{26}He sent him away to his house, saying,");
			blockToSplit.UserConfirmed = true;

			Assert.AreEqual(split.SplitId, blockToSplit.SplitId);

			var countOfTargetBlocksBeforeApplyingSplits = target.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(countOfTargetBlocksBeforeApplyingSplits + 2, targetBlocksAfterApplyingSplit.Count,
				"One explicit break in verse 26, plus one implicit one between v.25 and v. 26");
			var blockVerse26Start = targetBlocksAfterApplyingSplit.Single(b => b.BlockElements.OfType<Verse>().Any(v => v.Number == "26"));
			Assert.AreEqual(kSplitPos, blockVerse26Start.BlockElements.OfType<ScriptText>().Last().Content.Length);

			var iBlock = targetBlocksAfterApplyingSplit.IndexOf(blockVerse26Start) + 1;

			var block = targetBlocksAfterApplyingSplit[iBlock];
			Assert.AreEqual(origLengthOfVerse26 - kSplitPos, block.BlockElements.OfType<ScriptText>().First().Content.Length);
			Assert.AreEqual("Jesus", block.CharacterId);
			Assert.IsTrue(block.MatchesReferenceText);
			Assert.AreEqual("“Don't enter into the village, nor tell anyone in the village.”",
				block.ReferenceBlocks.Single().GetText(true));

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		public void ApplyUserDecisions_ReferenceTextAligned_VerseBridgingChanged_AlignmentNotApplied(int verseToMatchUp)
		{
			const int kChapter = 1;
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var sourceBlocks = new List<Block> {
				NewChapterBlock(kChapter, "MAT"),
				new Block("p", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse(1, "Blah")
					.AddVerse("2-3", "This is the lead-in prose. "),
				new Block("q", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddText("This is the first bit of poetic stuff. ")
					.AddVerse("4", "And this is some more text, which is also poetic. "),
				new Block("m", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddText("Now we're back to prose. ")
					.AddVerse("5", "That was a nice thing to say.") };

			var origSourceBlockCount = sourceBlocks.Count;
			var source = new BookScript("MAT", sourceBlocks, ScrVers.English);
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(1, verseToMatchUp));
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origSourceBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var targetBlocks = new List<Block> {
				NewChapterBlock(kChapter, "MAT"),
				new Block("p", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse(1, "Blah")
					.AddVerse("2", "This is the lead-in prose. "),
				new Block("q", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse("3-4", "This is the first bit of poetic stuff. And this is some more text,")
					.AddVerse("5", " which is also poetic. "),
				new Block("m", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddText("Now we're back to prose. That was a nice thing to say.") };

			var target = new BookScript("MAT", targetBlocks.Select(b => b.Clone()), ScrVers.English);

			target.ApplyUserDecisions(source, englishRefText);
			Assert.IsTrue(targetBlocks.SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		/// <summary>
		/// Match up all blocks to corresponding blocks in the reference text. Anything left over gets "blindly" matched
		/// to an empty reference block and assigned to narrator.
		/// </summary>
		/// <param name="matchup"></param>
		private static void MatchUpBlocksAndApplyToSource(BlockMatchup matchup)
		{
			matchup.MatchAllBlocks();
			var narrator = CharacterVerseData.GetStandardCharacterId(matchup.BookId, CharacterVerseData.StandardCharacter.Narrator);
			foreach (var block in matchup.CorrelatedBlocks.Where(b => b.CharacterIsUnclear ||
				(b.MultiBlockQuote != MultiBlockQuote.None && b.CharacterIsStandard)))
			{
				block.SetNonDramaticCharacterId(narrator);
				block.MultiBlockQuote = MultiBlockQuote.None;
			}
			matchup.Apply();
		}
		#endregion PG-1168

		/// <summary>
		/// PG-1231
		/// </summary>
		[Test]
		public void ApplyUserDecisions_SplitBlockBeforeMissingVerseNumber_TargetContainsBridgeWithMissingVerseNumber_SplitNotApplied()
		{
			const int kChapter = 1;
			const string kMat1_20a = "Gang hapëko opët le, imbalingum gang ɗakëko gokëɗ, aɗaŋal ar Urün ale komaŝaŋëta gonëkëɗ laŋ. Komare: ";
			const string kMat1_20b = "<Ŝosef, wuj ar ŝalëk ŝan gañamb an Dafid, koyéŋ imamaɗ Mari ogé asówar arój ɓawo ñaŋëso ñan ɗüɓko Mari ñaŋ fangar Genjëm en Urün le rik haŋ ŝotëko gacëɗ aŋ, mage hal. ";
			const string kMat1_21a = "Mari omadëme ñaŋëso ñacan. Ñungum imatap Ŝésü. Koto ër uyat or Ŝésü ówum oɗ le gon e oŋ iyi Afeyën ar ɓal e. Gungum ejëmatapal ñaŋëso ñungum Ŝésü ɓawo oɓepeyëne ɓal ɓüróm ɓële oméƴ or ñüdëkëɓe kongol ër ɓiñüŋüla ɓindóɓün laŋ. ";
			const string kMat1_21b = "Ngër kak reɗëkoma aɗaŋal ar Urün ale Ŝosef. Gang hëñëlakoma mëŝësün, koƴe. ";
			const string kMat1_23a = "Gen oŝot or gacëɗ or ŝungutuŋ ër mamalange aŝan eŋ, Urün oɗ acasëɗ kabiriŋ alëka iyi ogéye le. Adeɗ: ";
			const string kMat1_23b = "<Ŝungutuŋ ër mamalange aŝan ocote gacëɗ, omadëme ñaŋëso ñacan ñungum ënmayó Emanuwel. ";
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block> {
				NewChapterBlock(kChapter, "MAT"),
				new Block("p", kChapter, 20) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse(20, kMat1_20a),
				// Block break triggered by opening quote
				new Block("p", kChapter, 20) { IsParagraphStart = false, CharacterId = CharacterVerseData.kUnexpectedCharacter }
					.AddText(kMat1_20b)
					.AddVerse(21, kMat1_21a + kMat1_21b),
				// The closing quote mark is missing in the text, but the parser would force this block break (and
				// mark the previous one unknown because no known character from v. 20-21 should still be speaking
				// in v. 23 (or 22 for that matter).
				new Block("p", kChapter, 23) { IsParagraphStart = false, CharacterId = narrator }
					.AddVerse(23, kMat1_23a),
				// Block break triggered by opening quote. Again, this is unknown because of the missing closer.
				new Block("p", kChapter, 23) { IsParagraphStart = false, CharacterId = CharacterVerseData.kUnexpectedCharacter }
					.AddVerse(23, kMat1_23b + "Koto ër Emanuwel le gon e oŋ iyi Urün oɗ ëng ɓé eene. Gon reɗëko Urün oŋ aŝükélün ar Urün ale añugëɗ. Gang ŝotëko Mari gacëɗ hara mamalangëɗelaŋ aŝan, agé gon reɗëko Urün kabiriŋ alëka ogéye oŋ. ")
					.AddVerse(24, "Ŝosef gang lügütéko, adiɗ ngër gang reɗëkoma aɗaŋal ar Urün ale le. Komamaɗ Mari. Kogé asówar aróm. \v 25 Kono mënɓarɗelaŋ gañar ëng Ŝosef haŋ fo rëmkoma Mari ñaŋëso ñaŋ le. Gang rëmkoma, Ŝosef amatapëɗ ñaŋëso ñungum Ŝésü."),
			};

			var target = new BookScript("MAT", blocks.Select(b => b.Clone()), ScrVers.English);
			// In the bug report we're simulating here, the user had fixed the missing v. 22 by making
			// the existing v. 23 into verse bridge for 22-23.
			target.GetScriptBlocks()[3].InitialStartVerseNumber = 22;
			target.GetScriptBlocks()[3].InitialEndVerseNumber = 23;
			target.GetScriptBlocks()[3].BlockElements[0] = new Verse("22-23");
			target.GetScriptBlocks()[4].InitialStartVerseNumber = 22;
			target.GetScriptBlocks()[4].InitialEndVerseNumber = 23;

			var source = new BookScript("MAT", blocks, ScrVers.English);

			int i = 1;
			var firstBlockForV20 = source.GetScriptBlocks()[i++];
			Assert.IsTrue(firstBlockForV20.StartsAtVerseStart);
			Assert.AreEqual("20", ((Verse)firstBlockForV20.BlockElements[0]).Number);
			var secondBlockForV20 = source.GetScriptBlocks()[i++];

			var secondBlockForV21 = source.SplitBlock(secondBlockForV20, "21", kMat1_21a.Length, true, narrator);
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var origBlockCount = source.GetScriptBlocks().Count;
			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(kChapter, 20));
			matchup.CorrelatedBlocks.Last(b => b.InitialStartVerseNumber == 21).SetMatchedReferenceBlock("he said.");
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);
			// The setup steps don't exactly mimic real life, so we need to reset this to match actual data condition:
			source.GetScriptBlocks()[2].SplitId = -1;

			var v23Thru24Block = source.GetScriptBlocks().Last();
			var newBlock = source.SplitBlock(v23Thru24Block, "23", kMat1_23b.Length, true, narrator);
			newBlock.UserConfirmed = true;
			v23Thru24Block.CharacterId = CharacterVerseData.kAmbiguousCharacter;

			var v21Blocks = source.GetBlocksForVerse(1, 21).ToList();
			Assert.AreEqual(2, v21Blocks.Count);
			Assert.AreEqual(secondBlockForV21.GetText(true), v21Blocks.Last().GetText(true));
			Assert.IsNull(source.GetFirstBlockForVerse(1, 22));
			var v23Blocks = source.GetBlocksForVerse(1, 23).ToList();
			Assert.AreEqual(3, v23Blocks.Count);
			Assert.IsTrue(v21Blocks.All(b => b.SplitId == 0));
			Assert.IsTrue(v23Blocks.Skip(1).All(b => b.SplitId == 1));

			target.ApplyUserDecisions(source, englishRefText);

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
		}

		/// <summary>
		/// PG-1207
		/// </summary>
		[Test]
		public void ApplyUserDecisions_SourceMatchupNotAllMatchedToReferenceText_AlignmentNotApplied()
		{
			const int kChapter = 2;
			var narrator = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator);
			var blocks = new List<Block> {
				NewChapterBlock(kChapter, "MAT"),
				new Block("p", kChapter, 1) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse(1, "Jésus naît à Bethléem, en Judée, au moment où Hérode le Grand est roi. Alors, des sages viennent de l'est et arrivent à Jérusalem."),
				new Block("p", kChapter, 2) { IsParagraphStart = true, CharacterId = narrator }
					.AddVerse(2, "Ils demandent: "),
				new Block("p", kChapter, 2) { IsParagraphStart = false, CharacterId = "magi (wise men from East)" }
					.AddText(" <<Où est le roi des Juifs qui vient de naître? Nous avons vu son étoile se lever à l'est, et nous sommes venus l'adorer.>>")
			};

			var source = new BookScript("MAT", blocks, ScrVers.English);
			source[1].SetMatchedReferenceBlock("When Jesus was born in Bethlehem, Judea in the days of Herod, eastern wise men came to Jerusalem.");

			var target = new BookScript("MAT", blocks.Select(b => b.Clone()), source.Versification);

			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			target.ApplyUserDecisions(source, englishRefText);
			Assert.IsTrue(blocks.SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		#region PG-1179 unit tests for re-applying insertion of annotations to indicate where a sound effect goes.
		//[Test]
		//public void ApplyUserDecisions_AnnotationAddedAtStartOfVerseText_NoTextChange_AnnotationPreserved()
		//{
		//	// TODO: This test is not even close to complete!
		//	var source = CreateStandardMarkScript(true, true);
		//	var blockWhereV26Starts = source.GetFirstBlockForVerse(1, 26);

		//	blockWhereV26Starts.BlockElements.Insert(1, new Sound
		//	{
		//		StartVerse = 26,
		//		EffectName = "Demons screaming",
		//		SoundType = SoundType.Sfx,
		//		UserSpecifiesLocation = true
		//	});

		//	for (int i = 0; i < source.GetScriptBlocks().Count; i++)
		//	{
		//		if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
		//		{
		//			source[i].CharacterId = "Fred the Frog";
		//			source[i].Delivery = "with certitude";
		//			source[i].UserConfirmed = true;
		//		}
		//	}

		//	var target = CreateStandardMarkScript(true, true);
		//	var quoteBlockIndices = new List<int>();
		//	int iBlockAtVerse7 = -1;
		//	for (int i = 0; i < target.GetScriptBlocks().Count; i++)
		//	{
		//		if (target[i].CharacterId == CharacterVerseData.kUnknownCharacter)
		//			quoteBlockIndices.Add(i);

		//		if (target[i].InitialStartVerseNumber == 7 && iBlockAtVerse7 < 0)
		//			iBlockAtVerse7 = i;  // this block has extra verses
		//	}
		//	Assert.IsTrue(quoteBlockIndices.Count > 0);
		//	Assert.IsTrue(iBlockAtVerse7 > 0);
		//	int iLastBlock = target.GetScriptBlocks().Count - 1; // this block has extra verses
		//	Assert.IsTrue(iLastBlock > iBlockAtVerse7);
		//	var indicesOfQuoteBlocksWithExtraVerses = new[] { iBlockAtVerse7, iLastBlock };
		//	Assert.IsFalse(indicesOfQuoteBlocksWithExtraVerses.Except(quoteBlockIndices).Any());

		//	target.ApplyUserDecisions(source);

		//	for (int i = 0; i < target.GetScriptBlocks().Count; i++)
		//	{
		//		if (quoteBlockIndices.Contains(i))
		//		{
		//			if (indicesOfQuoteBlocksWithExtraVerses.Contains(i))
		//			{
		//				Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnknownCharacter);
		//				Assert.IsFalse(target[i].UserConfirmed);
		//			}
		//			else
		//			{
		//				Assert.AreEqual("Fred the Frog", target[i].CharacterId);
		//				Assert.AreEqual("with certitude", target[i].Delivery);
		//				Assert.IsTrue(target[i].UserConfirmed);
		//			}
		//		}
		//		else
		//		{
		//			Assert.IsTrue(target[i].CharacterIsStandard);
		//			Assert.IsNull(target[i].Delivery);
		//			Assert.IsFalse(target[i].UserConfirmed);
		//		}
		//	}

		//}

		//[Test]
		//public void ApplyUserDecisions_AnnotationAddedInMiddleOfVerseText_NoTextChange_AnnotationPreserved()
		//{
		//	Assert.Fail("Write this test");
		//}

		//[Test]
		//public void ApplyUserDecisions_AnnotationAddedAtEndOfVerseText_NoTextChange_AnnotationPreserved()
		//{
		//	Assert.Fail("Write this test");
		//}

		//[TestCase(0)]
		//[TestCase(10)]
		//[TestCase(46)]
		//public void ApplyUserDecisions_AnnotationAddedInText_TextChanged_AnnotationNotPreserved(int offset)
		//{
		//	Assert.Fail("Write this test");
		//}
		#endregion PG-1179

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, "1", 5);

			var target = CreateStandardMarkScript();
			var targetBlockToSplit = target.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newTargetBlock = target.SplitBlock(targetBlockToSplit, "1", 5);
			targetBlockToSplit.SplitId = Block.kNotSplit;
			newTargetBlock.SplitId = Block.kNotSplit;

			var expected = CreateStandardMarkScript();
			var expectedBlockToSplit = expected.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newExpectedBlock = expected.SplitBlock(expectedBlockToSplit, "1", 5);
			expectedBlockToSplit.SplitId = Block.kNotSplit;
			newExpectedBlock.SplitId = Block.kNotSplit;

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_TargetBlockOnlyMatchesFirstSplitBlock_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.LastVerseNum.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(12, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(12, blockToModify.LastVerseNum);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInTwo_ChangeInEarlierVerseInTarget_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			source.SplitBlock(blockToSplit, blockToSplit.LastVerseNum.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.First(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(6, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(6, blockToModify.LastVerseNum);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
			Assert.AreEqual(source.Blocks.Count, target.Blocks.Count);
			var comparer = new BlockComparer();
			for (int i = 0; i < target.Blocks.Count; i++)
			{
				if (target.Blocks[i] != blockToModify)
					Assert.IsTrue(comparer.Equals(source.Blocks[i], target.Blocks[i]));
			}
		}

		[Test]
		public void ApplyUserDecisions_SplitOneBlockInThree_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newBlockToSplit = source.SplitBlock(blockToSplit, "1", 10);
			source.SplitBlock(newBlockToSplit, "1", 5);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();
			Block targetBlockToSplit = GetSecondBlockInVerse(target);
			var newTargetBlock = target.SplitBlock(targetBlockToSplit, targetBlockToSplit.InitialStartVerseNumber.ToString(), 40);
			targetBlockToSplit.SplitId = Block.kNotSplit;
			newTargetBlock.SplitId = Block.kNotSplit;

			var expected = CreateStandardMarkScript();
			var expectedBlockToSplit = GetSecondBlockInVerse(expected);
			var newExpectedBlock = expected.SplitBlock(expectedBlockToSplit, expectedBlockToSplit.InitialStartVerseNumber.ToString(), 40);
			expectedBlockToSplit.SplitId = Block.kNotSplit;
			newExpectedBlock.SplitId = Block.kNotSplit;

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitSecondBlockInVerse_TargetBlockDoesNotMatch_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = GetSecondBlockInVerse(source);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), 40);

			var target = CreateStandardMarkScript();
			Block blockToModify = GetSecondBlockInVerse(target);
			var firstScriptTextInSecondVerse = blockToModify.BlockElements.OfType<ScriptText>().First();
			firstScriptTextInSecondVerse.Content = firstScriptTextInSecondVerse.Content.Insert(5, "blah");

			target.ApplyUserDecisions(source);
			Assert.IsTrue(blockToModify.BlockElements.OfType<ScriptText>().First().Content.Contains("blah"));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));
		}

		private Block GetSecondBlockInVerse(BookScript bookScript)
		{
			Block blockToSplit = null;
			int currentVerse = -1;
			foreach (var block in bookScript.Blocks.Where(b => b.InitialStartVerseNumber > 0))
			{
				if (block.InitialStartVerseNumber == currentVerse)
				{
					blockToSplit = block;
					break;
				}
				currentVerse = block.LastVerseNum;
			}
			Assert.IsNotNull(blockToSplit);
			return blockToSplit;
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockAlreadyHasSplitsBecauseParserHasBeenImproved_SplitsIgnored()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);

			var target = CreateStandardMarkScript();
			blockToSplit = target.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				var newBlock = target.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}

			var expected = CreateStandardMarkScript();
			blockToSplit = expected.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				var newBlock = expected.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}

			target.ApplyUserDecisions(source);
			Assert.True(expected.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitMultipleVersesInBlock_TargetBlockDoesNotMatchLastPieceOfSplit_SplitAddedToUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			List<Block> splits = new List<Block>();
			Block blockToSplit = source.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			splits.Add(blockToSplit);
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				blockToSplit = source.SplitBlock(blockToSplit, verse.ToString(), 4);
				splits.Add(blockToSplit);
			}

			var target = CreateStandardMarkScript();
			blockToSplit = target.Blocks.First(b => b.InitialEndVerseNumber != b.LastVerseNum);
			Block newBlock = null;
			foreach (var verse in blockToSplit.BlockElements.OfType<Verse>().Select(v => v.StartVerse).ToList())
			{
				newBlock = target.SplitBlock(blockToSplit, verse.ToString(), 4);
				blockToSplit.SplitId = Block.kNotSplit;
				newBlock.SplitId = Block.kNotSplit;
				blockToSplit = newBlock;
			}
			var last = newBlock.BlockElements.OfType<ScriptText>().Last();
			last.Content = last.Content.Insert(6, "blah");

			target.ApplyUserDecisions(source);
			Assert.IsTrue(newBlock.BlockElements.OfType<ScriptText>().Last().Content.Contains("blah"));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(splits));
		}

		[Test]
		public void ApplyUserDecisions_SplitImmediatelyBeforeVerseNumber_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0 && b.BlockElements.First() is ScriptText && b.ContainsVerseNumber);
			source.SplitBlock(blockToSplit, blockToSplit.InitialStartVerseNumber.ToString(), BookScript.kSplitAtEndOfVerse);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitImmediatelyBeforeThirdVerseNumberInBlock_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0 && b.BlockElements.OfType<Verse>().Count() >= 3);
			source.SplitBlock(blockToSplit, blockToSplit.BlockElements.OfType<Verse>().Skip(1).First().Number, BookScript.kSplitAtEndOfVerse);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitInVerseWithVerseBridge_TargetBlockCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit = source.Blocks.Last();
			blockToSplit.AddVerse("12-14", "The magic frog ate a monkey for breakfast.");
			source.SplitBlock(blockToSplit, "12-14", 9);

			var target = CreateStandardMarkScript();
			Block lastBlockInTarget = target.Blocks.Last();
			lastBlockInTarget.AddVerse("12-14", "The magic frog ate a monkey for breakfast.");

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitTwoUnrelatedBlocks_TargetBlocksCorrespondsToSplitSourceBlocks_SplitReapplied()
		{
			var source = CreateStandardMarkScript();
			Block blockToSplit1 = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			Block blockToSplit2 = source.Blocks.Last();
			source.SplitBlock(blockToSplit1, blockToSplit1.InitialVerseNumberOrBridge, 3);
			source.SplitBlock(blockToSplit2, blockToSplit2.InitialVerseNumberOrBridge, 3);

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.Blocks.SequenceEqual(target.Blocks, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		[Test]
		public void ApplyUserDecisions_SplitVersesHaveMatchedReferenceText_ReferenceTextSetAfterSplitReapplied()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newBlock = source.SplitBlock(blockToSplit, "1", 5);
			blockToSplit.SetMatchedReferenceBlock("{" + blockToSplit.BlockElements.OfType<Verse>().First().Number + "} First part. ");
			newBlock.SetMatchedReferenceBlock("Second part.");

			var target = CreateStandardMarkScript();

			target.ApplyUserDecisions(source);
			Assert.True(source.GetScriptBlocks().SequenceEqual(target.GetScriptBlocks(), new BlockComparerIncludingReferenceText()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		private class BlockComparerIncludingReferenceText : IEqualityComparer<Block>
		{
			private BlockComparer m_baseComparer = new BlockComparer();

			public bool Equals(Block x, Block y)
			{
				return m_baseComparer.Equals(x, y) && x.MatchesReferenceText == y.MatchesReferenceText &&
					x.ReferenceBlocks.Select(rx => rx.GetText(true)).SequenceEqual(y.ReferenceBlocks.Select(ry => ry.GetText(true)));
			}

			public int GetHashCode(Block obj)
			{
				return m_baseComparer.GetHashCode(obj);
			}
		}
		#endregion

		#region ApplyUserDecisions Other Tests
		[Test]
		public void ApplyUserDecisions_UnappliedSplitsExist_MaintainsOriginalUnappliedSplits()
		{
			var source = CreateStandardMarkScript();
			var blockToSplit = source.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock = source.SplitBlock(blockToSplit, blockToSplit.LastVerseNum.ToString(), 5);

			var target = CreateStandardMarkScript();
			var blockToModify = target.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			blockToModify.AddVerse(12, "This is another verse that was added to the new bundle.");

			target.ApplyUserDecisions(source);
			Assert.AreEqual(12, blockToModify.LastVerseNum);
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
			Assert.IsTrue(target.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }));

			var newSource = target;
			var blockToSplit2 = newSource.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			var newBlock2 = newSource.SplitBlock(blockToSplit2, blockToSplit2.LastVerseNum.ToString(), 5);

			var newTarget = CreateStandardMarkScript();
			var newBlockToModify = newTarget.Blocks.Last(b => b.InitialStartVerseNumber > 0);
			newBlockToModify.AddVerse(12, "This is another verse that was added to the new bundle, but now the text is different.");

			newTarget.ApplyUserDecisions(newSource);
			Assert.AreEqual(12, newBlockToModify.LastVerseNum);
			Assert.IsNotNull(newTarget.UnappliedSplits);
			Assert.AreEqual(2, newTarget.UnappliedSplits.Count);
			Assert.IsTrue(newTarget.UnappliedSplits[0].SequenceEqual(new[] { blockToSplit, newBlock }, new BlockComparer()));
			Assert.IsTrue(newTarget.UnappliedSplits[1].SequenceEqual(new[] { blockToSplit2, newBlock2 }, new BlockComparer()));
		}
		#endregion

		#region SplitBlock Tests
		[Test]
		public void SplitBlock_BlockNotInList_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(NewSingleVersePara(5, "Split here, dude."), "5", 11));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_VerseNotInBlock_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockForVerse2 = NewSingleVersePara(2, "It doesn't matter what this is.");
			mrkBlocks.Add(blockForVerse2);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(blockForVerse2, "3", 11));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_CharacterOffsetNotInVerse_ThrowsArgumentOutOfRangeException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockForVerse2 = NewSingleVersePara(2, "Short.");
			mrkBlocks.Add(blockForVerse2);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.Throws<ArgumentOutOfRangeException>(() => bookScript.SplitBlock(blockForVerse2, "2", 400));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_SingleVerseBlock_SplitsBlockAtSpecifiedOffset()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe.", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_AssignCharacterId_CharacterAssignedAndUserConfirmed()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57, true, "Jesus");
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe.", newBlock.GetText(true));
			Assert.AreEqual("Jesus", newBlock.CharacterId);
			Assert.IsNull(newBlock.CharacterIdOverrideForScript);
			Assert.IsTrue(newBlock.UserConfirmed);
		}

		[Test]
		public void SplitBlock_AssignMultiCharacterId_CharacterAndCharacterInScriptAssignedAndUserConfirmed()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, they told that synagogue ruler: Don't be afraid; just believe.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57, true, "James/John");
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignoring what they said, they told that synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe.", newBlock.GetText(true));
			Assert.AreEqual("James/John", newBlock.CharacterId);
			Assert.AreEqual("James", newBlock.CharacterIdInScript);
			Assert.IsTrue(newBlock.UserConfirmed);
		}

		[Test]
		public void SplitBlock_AssignMultiCharacterIdWithDefaultOverriddenInControlFile_VersificationShift_CharacterAndCharacterInScriptAssignedAndUserConfirmed()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(9));
			var blockToSplit = NewSingleVersePara(9, "And as they were coming down the mountain, he charged them to tell no one what they had seen, until the Son of Man had risen from the dead. ")
				//            0         1         2         3         4         5         6         7         8
				//            012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
				.AddVerse(10, "So they kept the matter to themselves, questioning: What does rising from the dead mean?");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, m_testVersification);
			var newBlock = bookScript.SplitBlock(blockToSplit, "10", 52, true, "Peter (Simon)/James/John");
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{9}\u00A0And as they were coming down the mountain, he charged them to tell no one what they had seen, " +
				"until the Son of Man had risen from the dead. {10}\u00A0So they kept the matter to themselves, questioning: ", blocks[1].GetText(true));
			Assert.AreEqual(10, newBlock.InitialStartVerseNumber);
			Assert.AreEqual("What does rising from the dead mean?", newBlock.GetText(true));
			Assert.AreEqual("Peter (Simon)/James/John", newBlock.CharacterId);
			Assert.AreEqual("John", newBlock.CharacterIdInScript);
			Assert.IsTrue(newBlock.UserConfirmed);
		}

		[Test]
		public void SplitBlock_MultiVerseBlock_SubsequentTextAndVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37-38", "This is the text of following verses. ");
			blockToSplit.AddVerse("39", "This is the text of final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. {37-38}\u00A0This is the text of following verses. {39}\u00A0This is the text of final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_SubsequentVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37-38", "This is the text of following verses. ");
			blockToSplit.AddVerse("39", "This is the text of final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 88);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[1].GetText(true));
			Assert.AreEqual(37, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(38, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("{37-38}\u00A0This is the text of following verses. {39}\u00A0This is the text of final verse. ", newBlock.GetText(true));
			Assert.AreEqual(2, newBlock.BlockElements.OfType<ScriptText>().Count());
		}

		[Test]
		public void SplitBlock_SplitInSecondVerseInBlock_SubsequentVersesMovedToNewBlock()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			var blockToSplit = NewSingleVersePara(35, "This is the first verse. ");
			//                          0         1         2         3         4         5         6         7         8
			//                          012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			blockToSplit.AddVerse("36", "Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("37", "This is the final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. {37}\u00A0This is the final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_SplitInVerseBridge_NewBlockHasCorrectStartAndEndVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			var blockToSplit = NewSingleVersePara(35, "This is the first verse. ");
			//                             0                                                                                                   1
			//                             0         1         2         3         4         5         6         7         8         9         0         1         2         3         4         5         6
			//                             0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
			blockToSplit.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplit.AddVerse("38", "This is the final verse. ");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36-37", 128);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(37, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("Don't be afraid; just believe. {38}\u00A0This is the final verse. ", newBlock.GetText(true));
		}

		[Test]
		public void SplitBlock_AttemptToSplitAtBeginningOfBlockThatIsNotPartOfMultiBlockQuote_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var splitBeforeBlock = NewSingleVersePara(2);
			mrkBlocks.Add(splitBeforeBlock);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(splitBeforeBlock, null, 0));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_AttemptToSplitAtEndOfBlockThatIsNotPartOfMultiBlockQuote_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var splitAfterBlock = NewSingleVersePara(2, "Verse text");
			mrkBlocks.Add(splitAfterBlock);
			mrkBlocks.Add(NewSingleVersePara(3));
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.Throws<ArgumentException>(() => bookScript.SplitBlock(splitAfterBlock, "2", "Verse text".Length));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks));
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_OffsetIsSpecialValue_NewBlockHasCorrectInitialVerseNumber()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockToSplit = NewSingleVersePara(2, "Verse text").AddVerse("3-4").AddVerse(5);
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", BookScript.kSplitAtEndOfVerse);
			Assert.AreEqual(3, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(4, newBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, blockToSplit.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)blockToSplit.BlockElements.First()).Number);
			Assert.AreEqual("Verse text", ((ScriptText)blockToSplit.BlockElements.Last()).Content);
			Assert.AreEqual(4, newBlock.BlockElements.Count);
			Assert.AreEqual("3-4", ((Verse)newBlock.BlockElements.First()).Number);
		}

		[Test]
		public void SplitBlock_SplitAtEndOfVerse_OffsetIsLengthOfText_NewBlockHasCorrectInitialVerseNumber()
		{
			// This tests it with the offset passed in the way it makes logical sense, even though the split dialog never calculates it this way.
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			var blockToSplit = NewSingleVersePara(2, "Verse text").AddVerse("3-4").AddVerse(5);
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", "Verse text".Length);
			Assert.AreEqual(3, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(4, newBlock.InitialEndVerseNumber);
			Assert.AreEqual(2, blockToSplit.BlockElements.Count);
			Assert.AreEqual("2", ((Verse)blockToSplit.BlockElements.First()).Number);
			Assert.AreEqual("Verse text", ((ScriptText)blockToSplit.BlockElements.Last()).Content);
			Assert.AreEqual(4, newBlock.BlockElements.Count);
			Assert.AreEqual("3-4", ((Verse)newBlock.BlockElements.First()).Number);
		}

		[Test]
		public void SplitBlock_SplitAfterFirstBlockInMultiBlockQuote_UsesSpecialNullParameterCase_FirstBlockChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-34");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block4 = NewSingleVersePara(38).AddVerse(39);
			block4.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block4);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block4.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitAfterFirstBlockInMultiBlockQuote_UsesActualOffset_FirstBlockChangedToNone()
		{
			// This tests it with the offset passed in the way it makes logical sense, even though the split dialog never calculates it this way.
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplitAfter = NewSingleVersePara(1).AddVerse("2-34");
			blockToSplitAfter.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplitAfter);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block4 = NewSingleVersePara(38).AddVerse(39);
			block4.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block4);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitAfter, "2-34", blockToSplitAfter.BlockElements.OfType<ScriptText>().Last().Content.Length));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitAfter.MultiBlockQuote);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block4.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocksInMultiBlockQuote_BlockAtStartOfSplitChangedToStart()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var block3 = NewSingleVersePara(1).AddVerse("3-34");
			block3.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block3);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var block5 = NewSingleVersePara(38).AddVerse(39);
			block5.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block5);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(5, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(5, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[3].GetText(true));
			Assert.AreEqual(MultiBlockQuote.Start, blockToSplitBefore.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block5.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBeforeLastBlockInMultiBlockQuote_BlockChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-32");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var block3 = NewSingleVersePara(33).AddVerse(34);
			block3.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(block3);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.Start, block2.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, block3.MultiBlockQuote);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[3].GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitBefore.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_SplitBetweenBlocksInTwoBlockQuote_BothBlocksChangedToNone()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var block2 = NewSingleVersePara(1).AddVerse("2-34");
			block2.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(block2);
			var blockToSplitBefore = NewSingleVersePara(35, "This is the first verse. ");
			blockToSplitBefore.AddVerse("36-37", "Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
			Assert.AreEqual(blockToSplitBefore, bookScript.SplitBlock(blockToSplitBefore, null, 0));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(MultiBlockQuote.None, block2.MultiBlockQuote);
			Assert.AreEqual("{35}\u00A0This is the first verse. {36-37}\u00A0Ignoring what they said and prohibiting anyone except Peter, James and John from following him, Jesus told the synagogue ruler: Don't be afraid; just believe. ", blocks[2].GetText(true));
			Assert.AreEqual(MultiBlockQuote.None, blockToSplitBefore.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void SplitBlock_SplitInMiddleOfMultiBlockStart_FirstPartIsNoneAndSecondPartIsStart(MultiBlockQuote continuingStatus)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var blockToSplit = NewSingleVersePara(1, "Original start block. ");
			blockToSplit.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplit);

			var continuationBlock = NewSingleVersePara(2, "Original continuation block. ");
			continuationBlock.MultiBlockQuote = continuingStatus;
			mrkBlocks.Add(continuationBlock);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "1", 8);

			Assert.AreEqual(MultiBlockQuote.None, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, newBlock.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.ChangeOfDelivery)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.ChangeOfDelivery)]
		[TestCase(MultiBlockQuote.Continuation, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery, MultiBlockQuote.Continuation)]
		public void SplitBlock_SplitInMiddleOfMultiBlockContinuationFollowedByContinuationBlock_FirstPartIsContinuationAndSecondPartIsStart(MultiBlockQuote continuingStatus1, MultiBlockQuote continuingStatus2)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var startBlock = NewSingleVersePara(1, "Original start block. ");
			startBlock.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(startBlock);

			var blockToSplit = NewSingleVersePara(2, "Original continuation block (first). ");
			blockToSplit.MultiBlockQuote = continuingStatus1;
			mrkBlocks.Add(blockToSplit);

			var nextContinuationBlock = NewSingleVersePara(3, "Original continuation block (second). ");
			nextContinuationBlock.MultiBlockQuote = continuingStatus2;
			mrkBlocks.Add(nextContinuationBlock);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 8);

			Assert.AreEqual(continuingStatus1, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, newBlock.MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.ChangeOfDelivery)]
		public void SplitBlock_SplitInMiddleOfMultiBlockContinuationFollowedByNoneBlock_FirstPartIsContinuationAndSecondPartIsNone(MultiBlockQuote continuingStatus)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));

			var startBlock = NewSingleVersePara(1, "Original start block. ");
			startBlock.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(startBlock);

			var blockToSplit = NewSingleVersePara(2, "Original continuation block. ");
			blockToSplit.MultiBlockQuote = continuingStatus;
			mrkBlocks.Add(blockToSplit);

			var noneBlock = NewSingleVersePara(3, "Original none block. ");
			noneBlock.MultiBlockQuote = MultiBlockQuote.None;
			mrkBlocks.Add(noneBlock);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 8);

			Assert.AreEqual(continuingStatus, blockToSplit.MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, newBlock.MultiBlockQuote);
		}

		[Test]
		public void SplitBlock_FirstBlockHasOriginalCharacterIdAndUserConfirmedTrueAndSecondBlockHasUnknownCharacterIdAndUserConfirmedFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = NewSingleVersePara(1).AddVerse("2", "Some text");
			blockToSplit.CharacterId = "Bill";
			blockToSplit.UserConfirmed = true;
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 5);
			Assert.AreEqual("Bill", blockToSplit.CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnexpectedCharacter, newBlock.CharacterId);
			Assert.IsTrue(blockToSplit.UserConfirmed);
			Assert.IsFalse(newBlock.UserConfirmed);
		}

		[Test]
		public void SplitBlock_GetFirstBlockForVerse_SplitBlockInPreviousChapterAfterCallingGetFirstBlockForVerseInSubsequentChapter()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = NewSingleVersePara(1).AddVerse("1", "Block to split");
			mrkBlocks.Add(blockToSplit);
			mrkBlocks.Add(NewChapterBlock(2));
			var block2 = NewSingleVersePara(1).AddVerse("1", "A verse in a subsequent chapter");
			mrkBlocks.Add(block2);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count);

			// Call GetFirstBlockForVerse on a chapter after the block we are splitting.
			// There was a bug in the code which updates an index in the split code.
			// That index is not set up until we call this.
			var blockResultBeforeSplit = bookScript.GetFirstBlockForVerse(2, 1);
			bookScript.SplitBlock(blockToSplit, "1", 5);

			var blockResultAfterSplit = bookScript.GetFirstBlockForVerse(2, 1);
			Assert.AreEqual(blockResultBeforeSplit, blockResultAfterSplit);
		}

		[Test]
		public void SplitBlock_NoBlockInBookHasBeenSplitPreviously_ResultBlocksHaveSplitIdOfZero()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(0, blocks[2].SplitId);
			Assert.AreEqual(0, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_BlockInBookHasBeenSplitPreviously_ResultBlocksHaveMaxSplitIdPlusOne()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			Block previouslySplitBlock1 = NewSingleVersePara(35, "This block was split previously");
			previouslySplitBlock1.SplitId = 5;
			mrkBlocks.Add(previouslySplitBlock1);
			Block previouslySplitBlock2 = NewSingleVersePara(35, "This block was split previously");
			previouslySplitBlock2.SplitId = 5;
			mrkBlocks.Add(previouslySplitBlock2);
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(5, blocks[1].SplitId);
			Assert.AreEqual(5, blocks[2].SplitId);
			Assert.AreEqual(6, blocks[3].SplitId);
			Assert.AreEqual(6, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_BlockBeingSplitHasBeenSplitPreviously_ResultBlocksHaveSameSplitIdAsBlockToSplit()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			//                                        0         1         2
			//                                        01234567890123456789012345
			var blockToSplit = NewSingleVersePara(36, "Before split: After Split");
			blockToSplit.SplitId = 3;
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.SplitBlock(blockToSplit, "36", 15);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(3, blocks[2].SplitId);
			Assert.AreEqual(3, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_MultiBlock_NoBlockInBookHasBeenSplitPreviously_ResultBlocksHaveSplitIdOfZero()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			mrkBlocks.Add(NewSingleVersePara(35, "This block is not split"));
			var blockToSplitAfter = NewSingleVersePara(36, "Before split: ");
			blockToSplitAfter.MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(blockToSplitAfter);
			var blockToSplitBefore = NewSingleVersePara(36, "After Split");
			blockToSplitBefore.MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(blockToSplitBefore);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.SplitBlock(blockToSplitBefore, null, 0);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(Block.kNotSplit, blocks[0].SplitId); // chapter number
			Assert.AreEqual(Block.kNotSplit, blocks[1].SplitId);
			Assert.AreEqual(0, blocks[2].SplitId);
			Assert.AreEqual(0, blocks[3].SplitId);
		}

		[Test]
		public void SplitBlock_BlockHasReferenceText_RefererenceTextCleared()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(5));
			//                                        0         1         2         3         4         5         6         7         8
			//                                        012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			var blockToSplit = NewSingleVersePara(36, "Ignorando lo que dijeron, Jesus le dijo al jefe de la sinagoga: No temas; solo cree.");
			mrkBlocks.Add(blockToSplit);
			blockToSplit.SetMatchedReferenceBlock("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe.");
			Assert.AreEqual("{36}\u00A0Ignoring what they said, Jesus told the synagogue ruler: Don't be afraid; just believe.", blockToSplit.GetPrimaryReferenceText());
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 64);
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual(blocks[2], newBlock);
			Assert.AreEqual("{36}\u00A0Ignorando lo que dijeron, Jesus le dijo al jefe de la sinagoga: ", blocks[1].GetText(true));
			Assert.AreEqual(36, newBlock.InitialStartVerseNumber);
			Assert.AreEqual(0, newBlock.InitialEndVerseNumber);
			Assert.AreEqual("No temas; solo cree.", newBlock.GetText(true));
			Assert.IsFalse(blockToSplit.MatchesReferenceText);
			Assert.IsFalse(blockToSplit.ReferenceBlocks.Any());
			Assert.IsFalse(newBlock.MatchesReferenceText);
			Assert.IsFalse(newBlock.ReferenceBlocks.Any());
		}

		[Test]
		public void SplitBlock_InitialPunctuationBeforeVerseNumber_SplitsCorrectlyIgnoringLeadingPunctuation()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", 1, 2)
			{
				BlockElements =
				{
					new ScriptText("("),
					new Verse("2"),
					new ScriptText("This verse has initial punctuation).")
				}
			};
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			// SUT
			bookScript.SplitBlock(blockToSplit, "2", 5);

			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual("({2}\u00A0This ", blocks[1].GetText(true));
			Assert.AreEqual("verse has initial punctuation).", blocks[2].GetText(true));
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_NoVerseElementsInBlock_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1) { IsParagraphStart = true };
			blockToSplit.BlockElements.Add(new ScriptText("Blah."));
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 1));
			Assert.AreEqual(2, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_NoSubsequentVersesInBlockAndNoSubsequentBlocks_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1) { IsParagraphStart = true };
			blockToSplit.AddVerse(1, "Blah blah blah.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 1));
			Assert.AreEqual(2, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_NoSubsequentVersesInBlockSubsequentBlockIsNotContinuationOfQuote_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1) { IsParagraphStart = true };
			blockToSplit.AddVerse(1, "Blah blah blah.");
			mrkBlocks.Add(blockToSplit);
			var followingBlock = new Block("p", m_curSetupChapter, 2) { IsParagraphStart = true };
			followingBlock.AddVerse(2, "Whatever.");
			mrkBlocks.Add(followingBlock);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 1));
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_BlockStartsInTheMiddleOfAVerseBridgeThatEndsWithTheVerseNumberToSplitAfter_SplitsAtEndOfVerseBridge()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var precedingBlock = NewVerseBridgePara(1, 2,
				"This is the part of the bridged verse that occurs in the preceding paragraph.");
			mrkBlocks.Add(precedingBlock);
			var blockToSplit = NewPara("p", "This is the remainder of the verse bridge in the block to split. ");
			blockToSplit.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("This is the remainder of the verse bridge in the block to split. ", blocks[2].GetText(true));
			Assert.AreEqual(1, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[2].InitialEndVerseNumber);
			Assert.AreEqual("{3}\u00A0This is the text of the following verse.", blocks[3].GetText(true));
			Assert.AreEqual(3, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[3].InitialEndVerseNumber);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitAfterHasABridgeAndASubVerseLetter_SplitsAtEndOfLastPartOfVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }.AddVerse(
				"1-2a", "This is the bridge that has the first part of verse 2. ")
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(3, blocks.Count);
			Assert.AreEqual("{1-2a}\u00A0This is the bridge that has the first part of verse 2. {2b}\u00A0This is the rest of verse two. ",
				blocks[1].GetText(true));
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("{3}\u00A0This is the text of the following verse.", blocks[2].GetText(true));
			Assert.AreEqual(3, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[2].InitialEndVerseNumber);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitAfterHasABridgeAndASubVerseLetterButSecondOPartOfVerseIsInFollowingBlock_ReturnsFalse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }
				.AddVerse("1-2a", "This is the bridge that has the first part of verse 2. ");
			mrkBlocks.Add(blockToSplit);
			var followingBlock = new Block("p", m_curSetupChapter, 2) { CharacterId = "Jesus" }
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(followingBlock);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsFalse(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_VerseToSplitBeginsWithSecondOPartOfVerseToSplit_SplitsAtEndOfLastPartOfVerse()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			var precedingBlock = new Block("p", m_curSetupChapter, 1, 2) { IsParagraphStart = true }
				.AddVerse("1-2a", "This is the bridge that has the first part of verse 2. ");
			mrkBlocks.Add(precedingBlock);
			var blockToSplit = new Block("p", m_curSetupChapter, 2) { CharacterId = "Jesus" }
				.AddVerse("2b", "This is the rest of verse two. ")
				.AddVerse("3", "This is the text of the following verse.");
			mrkBlocks.Add(blockToSplit);
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 2));
			var blocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(4, blocks.Count);
			Assert.AreEqual("{1-2a}\u00A0This is the bridge that has the first part of verse 2. ",
				blocks[1].GetText(true));
			Assert.AreEqual(1, blocks[1].InitialStartVerseNumber);
			Assert.AreEqual(2, blocks[1].InitialEndVerseNumber);
			Assert.AreEqual("{2b}\u00A0This is the rest of verse two. ", blocks[2].GetText(true));
			Assert.AreEqual(2, blocks[2].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[2].InitialEndVerseNumber);
			Assert.AreEqual("{3}\u00A0This is the text of the following verse.", blocks[3].GetText(true));
			Assert.AreEqual(3, blocks[3].InitialStartVerseNumber);
			Assert.AreEqual(0, blocks[3].InitialEndVerseNumber);
		}

		[Test]
		public void TrySplitBlockAtEndOfVerse_ReferenceBlockHasNoCorrespondingVerse_NewBlockMatchesToEmptyRefBlock()
		{
			//Assert.Fail("Write this test - existing logic causes new block to match to a null ref block.");
			var blocks = new List<Block>();
			blocks.Add(NewChapterBlock(1));
			var blockToSplit = new Block("p", m_curSetupChapter, 10) { IsParagraphStart = true };
			blockToSplit.CharacterId = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator);
			blockToSplit.AddVerse(10, "Ich hörte hinter mir eine Stimme, die durchdringend wie eine Posaune klang ").AddVerse(11, "und die mir befahl: ");
			blocks.Add(blockToSplit);
			var spanishRefBlock = new Block("p", m_curSetupChapter, 10).AddVerse(10, "Oí detrás de mí una gran voz, como sonido de trompeta, que decía: ");
			spanishRefBlock.CharacterId = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator);
			blockToSplit.SetMatchedReferenceBlock(spanishRefBlock);
			var englishRefBlock = new Block("p", m_curSetupChapter, 10).AddVerse(10, "I heard behind me a loud voice like a trumpet ").AddVerse(11, "saying, ");
			englishRefBlock.CharacterId = CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator);
			spanishRefBlock.SetMatchedReferenceBlock(englishRefBlock);

			var bookScript = new BookScript("REV", blocks, ScrVers.English);
			Assert.IsTrue(bookScript.TrySplitBlockAtEndOfVerse(blockToSplit, 10));
			Assert.AreEqual(3, bookScript.GetScriptBlocks().Count);
			var origSplitBlock = bookScript.GetScriptBlocks()[1];
			Assert.AreEqual("{10}\u00A0Ich hörte hinter mir eine Stimme, die durchdringend wie eine Posaune klang ", origSplitBlock.GetText(true));
			Assert.AreEqual("{10}\u00A0Oí detrás de mí una gran voz, como sonido de trompeta, que decía: ", origSplitBlock.GetPrimaryReferenceText());
			var newSplitBlock = bookScript.GetScriptBlocks()[2];
			Assert.AreEqual("{11}\u00A0und die mir befahl: ", newSplitBlock.GetText(true));
			Assert.AreEqual("", newSplitBlock.GetPrimaryReferenceText());
			var spanishRefBlockForOrigVernBlock = origSplitBlock.ReferenceBlocks.Single();
			Assert.AreEqual("{10}\u00A0I heard behind me a loud voice like a trumpet {11}\u00A0saying, ", spanishRefBlockForOrigVernBlock.GetPrimaryReferenceText(),
				"This might seem wrong, since we're splitting the vern at 11, but this is the English ref text that corresponded to the Spanish " +
				"referenece text, and since that hasn't changed, this shouldn't either.");
			Assert.IsTrue(spanishRefBlockForOrigVernBlock.MatchesReferenceText);
			var spanishRefBlockForNewVernBlock = newSplitBlock.ReferenceBlocks.Single();
			Assert.AreEqual("", spanishRefBlockForNewVernBlock.GetPrimaryReferenceText());
			Assert.IsTrue(spanishRefBlockForNewVernBlock.MatchesReferenceText);
		}
		#endregion

		#region CleanUpMultiBlockQuotes Tests
		[Test]
		public void CleanUpMultiBlockQuotes_MultiBlockHasSingleCharacter_DataUnchanged()
		{
			var mrkBlocks = new List<Block>();

			var block = NewSingleVersePara(1);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Start;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Continuation;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.MultiBlockQuote = MultiBlockQuote.None;
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.CleanUpMultiBlockQuotes();
			var result = bookScript.GetScriptBlocks();

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("Andrew", result[0].CharacterId);
			Assert.AreEqual("Andrew", result[0].CharacterIdInScript);
			Assert.True(result[0].UserConfirmed);
			Assert.AreEqual("Andrew", result[1].CharacterId);
			Assert.AreEqual("Andrew", result[1].CharacterIdInScript);
			Assert.True(result[1].UserConfirmed);
			Assert.IsTrue(result[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void CleanUpMultiBlockQuotes_MultiBlockHasMultipleCharacters_CharacterIsAmbiguousAndUserConfirmedIsFalse()
		{
			var mrkBlocks = new List<Block>();

			var block = NewSingleVersePara(1);
			block.CharacterId = "Andrew";
			block.CharacterIdInScript = "Andrew";
			block.MultiBlockQuote = MultiBlockQuote.Start;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Peter";
			block.CharacterIdInScript = "Peter";
			block.MultiBlockQuote = MultiBlockQuote.Continuation;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			block = NewSingleVersePara(2);
			block.CharacterId = "Peter";
			block.CharacterIdInScript = "Peter";
			block.MultiBlockQuote = MultiBlockQuote.None;
			block.UserConfirmed = true;
			mrkBlocks.Add(block);

			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			bookScript.CleanUpMultiBlockQuotes();
			var result = bookScript.GetScriptBlocks();

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, result[0].CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, result[0].CharacterIdInScript);
			Assert.False(result[0].UserConfirmed);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, result[1].CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, result[1].CharacterIdInScript);
			Assert.False(result[1].UserConfirmed);
			Assert.AreEqual("Peter", result[2].CharacterId);
			Assert.AreEqual("Peter", result[2].CharacterIdInScript);
			Assert.True(result[2].UserConfirmed);
		}
		#endregion

		#region ReplaceBlocks Tests
		[TestCase(MultiBlockQuote.None, MultiBlockQuote.None, MultiBlockQuote.None)]
		[TestCase(MultiBlockQuote.Start, MultiBlockQuote.None, MultiBlockQuote.None)]
		[TestCase(MultiBlockQuote.None, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Start, MultiBlockQuote.Start, MultiBlockQuote.Continuation)]
		public void ReplaceBlocks_WithQuoteBlocks_FollowingBlockIsNotContinuationOfQuote_NoChangesToFollowingBlock(MultiBlockQuote followingBlockMultiBlockQuoteType,
			MultiBlockQuote firstReplacementBlockMultiBlockQuoteType, MultiBlockQuote subsequentReplacementBlockMultiBlockQuoteType)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Add(NewSingleVersePara(3));
			var origLastBlockText = mrkBlocks.Last().GetText(true);
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = followingBlockMultiBlockQuoteType;
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			var replacementBlocks = new List<Block>();
			replacementBlocks.Add(NewSingleVersePara(1, "This is the new text."));
			replacementBlocks[0].MultiBlockQuote = firstReplacementBlockMultiBlockQuoteType;
			replacementBlocks[0].CharacterId = "neighbors";
			replacementBlocks[0].SetMatchedReferenceBlock("{1} Reference text for v1.");
			replacementBlocks.Add(NewSingleVersePara(2, "This is the new text."));
			replacementBlocks[1].MultiBlockQuote = subsequentReplacementBlockMultiBlockQuoteType;
			replacementBlocks[1].CharacterId = "neighbors";
			replacementBlocks[1].SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks.Add(NewBlock("Again, I say, this is the new text."));
			replacementBlocks[2].MultiBlockQuote = subsequentReplacementBlockMultiBlockQuoteType;
			replacementBlocks[2].CharacterId = "neighbors";
			replacementBlocks[2].SetMatchedReferenceBlock("More reference text for v2.");

			bookScript.ReplaceBlocks(1, 2, replacementBlocks);

			var newBlocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(5, newBlocks.Count);
			Assert.AreEqual("{1}\u00A0This is the new text.", newBlocks[1].GetText(true));
			Assert.AreEqual("{1}\u00A0Reference text for v1.", newBlocks[1].GetPrimaryReferenceText());
			Assert.AreEqual(firstReplacementBlockMultiBlockQuoteType, newBlocks[1].MultiBlockQuote);
			Assert.AreEqual("neighbors", newBlocks[1].CharacterId);

			Assert.AreEqual("{2}\u00A0This is the new text.", newBlocks[2].GetText(true));
			Assert.AreEqual("{2}\u00A0Reference text for v2.", newBlocks[2].GetPrimaryReferenceText());
			Assert.AreEqual(subsequentReplacementBlockMultiBlockQuoteType, newBlocks[2].MultiBlockQuote);
			Assert.AreEqual("neighbors", newBlocks[2].CharacterId);

			Assert.AreEqual("Again, I say, this is the new text.", newBlocks[3].GetText(true));
			Assert.AreEqual("More reference text for v2.", newBlocks[3].GetPrimaryReferenceText());
			Assert.AreEqual(subsequentReplacementBlockMultiBlockQuoteType, newBlocks[3].MultiBlockQuote);
			Assert.AreEqual("neighbors", newBlocks[3].CharacterId);

			Assert.AreEqual(origLastBlockText, newBlocks[4].GetText(true));
			Assert.IsFalse(newBlocks[4].MatchesReferenceText);
			Assert.AreEqual(followingBlockMultiBlockQuoteType, newBlocks[4].MultiBlockQuote);
		}

		[TestCase(MultiBlockQuote.Start)]
		[TestCase(MultiBlockQuote.Continuation)]
		public void ReplaceBlocks_LastReplacementBlockIsNoneFollowingBlockIsContinuationOfQuote_ThrowsInvalidOperationException(MultiBlockQuote lastBlockMultiBlockQuote)
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			var verse3Block = NewSingleVersePara(3);
			mrkBlocks.Add(verse3Block);
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			var replacementBlocks = new List<Block>();
			replacementBlocks.Add(NewSingleVersePara(1, "This is the new text."));
			replacementBlocks[0].CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			replacementBlocks[0].SetMatchedReferenceBlock("{1} Reference text for v1.");
			replacementBlocks.Add(NewSingleVersePara(2, "This is the new text."));
			replacementBlocks[1].CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			replacementBlocks[1].SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks[1].MultiBlockQuote = lastBlockMultiBlockQuote;

			var exception = Assert.Throws<InvalidOperationException>(() => bookScript.ReplaceBlocks(1, 2, replacementBlocks));
			Assert.AreEqual("Caller is responsible for setting preceding block(s)' MultiBlockQuote property set to None\r\n" +
				$"{replacementBlocks[1].ToString(true, "MRK")}", exception.Message);
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Start)]
		public void ReplaceBlocks_WithQuoteBlocks_FollowingBlocksAreContinuationOfQuote_CharacterIdUpdatedForFollowingBlocks(MultiBlockQuote finalReplacementBlockMultiBlockQuoteType)
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Add(NewSingleVersePara(3, "What the blind man says in verse 3."));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(NewSingleVersePara(4, "What the blind man says in verse 4, "));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(NewBlock("thus spake he forthwith."));
			mrkBlocks.Last().CharacterId = narrator;
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			var replacementBlocks = new List<Block>();
			MultiBlockQuote firstReplacementBlockMultiBlockQuoteType;
			string characterOfFirstReplacementBlock;
			if (finalReplacementBlockMultiBlockQuoteType == MultiBlockQuote.Continuation)
			{
				firstReplacementBlockMultiBlockQuoteType = MultiBlockQuote.Start;
				characterOfFirstReplacementBlock = "neighbors";
			}
			else
			{
				firstReplacementBlockMultiBlockQuoteType = MultiBlockQuote.None;
				characterOfFirstReplacementBlock = narrator;
			}
			replacementBlocks.Add(NewSingleVersePara(2, "This is the new text."));
			replacementBlocks.Last().MultiBlockQuote = firstReplacementBlockMultiBlockQuoteType;
			replacementBlocks.Last().CharacterId = characterOfFirstReplacementBlock;
			if (finalReplacementBlockMultiBlockQuoteType == MultiBlockQuote.Continuation)
				replacementBlocks.Last().CharacterIdInScript = "Walter";
			replacementBlocks.Last().SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks.Add(NewBlock("This is the new text, spoken by the neighbors."));
			replacementBlocks.Last().MultiBlockQuote = finalReplacementBlockMultiBlockQuoteType;
			replacementBlocks.Last().CharacterId = "neighbors";
			replacementBlocks.Last().CharacterIdInScript = "Walter";
			replacementBlocks.Last().SetMatchedReferenceBlock("More reference text for v2.");

			bookScript.ReplaceBlocks(2, 1, replacementBlocks);

			var newBlocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(7, newBlocks.Count);
			int i = 2;
			Assert.AreEqual("{2}\u00A0This is the new text.", newBlocks[i].GetText(true));
			Assert.AreEqual("{2}\u00A0Reference text for v2.", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(firstReplacementBlockMultiBlockQuoteType, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(characterOfFirstReplacementBlock, newBlocks[i].CharacterId);

			Assert.AreEqual("This is the new text, spoken by the neighbors.", newBlocks[++i].GetText(true));
			Assert.AreEqual("More reference text for v2.", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(finalReplacementBlockMultiBlockQuoteType, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("neighbors", newBlocks[i].CharacterId);
			Assert.AreEqual("Walter", newBlocks[i].CharacterIdInScript);

			Assert.AreEqual("{3}\u00A0What the blind man says in verse 3.", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.Continuation, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(newBlocks[3].CharacterId, newBlocks[i].CharacterId);
			Assert.AreEqual(newBlocks[3].CharacterIdInScript, newBlocks[i].CharacterIdInScript);

			Assert.AreEqual("{4}\u00A0What the blind man says in verse 4, ", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.Continuation, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(newBlocks[3].CharacterId, newBlocks[i].CharacterId);
			Assert.AreEqual(newBlocks[3].CharacterIdInScript, newBlocks[i].CharacterIdInScript);

			Assert.AreEqual("thus spake he forthwith.", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(narrator, newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);
		}

		[Test]
		public void ReplaceBlocks_WithNarratorBlock_MultipleFollowingBlocksAreContinuationOfQuote_MultiBlockQuoteChainBroken()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Add(NewSingleVersePara(3, "What the blind man says in verse 3."));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(NewSingleVersePara(4, "What the blind man says in verse 4, "));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(NewBlock("thus spake he forthwith."));
			mrkBlocks.Last().CharacterId = narrator;
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			var replacementBlocks = new List<Block>();
			replacementBlocks.Add(NewSingleVersePara(2, "I can't see anything!"));
			replacementBlocks.Last().CharacterId = "blind man";
			replacementBlocks.Last().CharacterIdOverrideForScript = "Walter";
			replacementBlocks.Last().SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks.Add(NewBlock("Then Walter said: "));
			replacementBlocks.Last().MultiBlockQuote = MultiBlockQuote.None;
			replacementBlocks.Last().CharacterId = narrator;
			replacementBlocks.Last().SetMatchedReferenceBlock("he said");

			bookScript.ReplaceBlocks(2, 1, replacementBlocks);

			var newBlocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(7, newBlocks.Count);
			int i = 2;
			Assert.AreEqual("{2}\u00A0I can't see anything!", newBlocks[i].GetText(true));
			Assert.AreEqual("{2}\u00A0Reference text for v2.", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("blind man", newBlocks[i].CharacterId);
			Assert.AreEqual("Walter", newBlocks[i].CharacterIdInScript);

			Assert.AreEqual("Then Walter said: ", newBlocks[++i].GetText(true));
			Assert.AreEqual("he said", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(narrator, newBlocks[i].CharacterId);

			Assert.AreEqual("{3}\u00A0What the blind man says in verse 3.", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.Start, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("blind man", newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);

			Assert.AreEqual("{4}\u00A0What the blind man says in verse 4, ", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.Continuation, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("blind man", newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);

			Assert.AreEqual("thus spake he forthwith.", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(narrator, newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);
		}

		[Test]
		public void ReplaceBlocks_WithNarratorBlock_SingleFollowingBlockIsContinuationOfQuote_MultiBlockQuoteChainBroken()
		{
			var narrator = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Add(NewSingleVersePara(3, "What the blind man says in verse 3. "));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			mrkBlocks.Add(NewSingleVersePara(4, "thus spake he forthwith."));
			mrkBlocks.Last().CharacterId = narrator;
			var bookScript = new BookScript("MRK", mrkBlocks, ScrVers.English);

			var replacementBlocks = new List<Block>();
			replacementBlocks.Add(NewSingleVersePara(2, "I can't see anything!"));
			replacementBlocks.Last().CharacterId = "blind man";
			replacementBlocks.Last().CharacterIdOverrideForScript = "Walter";
			replacementBlocks.Last().SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks.Add(NewBlock("Then Walter said: "));
			replacementBlocks.Last().MultiBlockQuote = MultiBlockQuote.None;
			replacementBlocks.Last().CharacterId = narrator;
			replacementBlocks.Last().SetMatchedReferenceBlock("he said");

			bookScript.ReplaceBlocks(2, 1, replacementBlocks);

			var newBlocks = bookScript.GetScriptBlocks();
			Assert.AreEqual(6, newBlocks.Count);
			int i = 2;
			Assert.AreEqual("{2}\u00A0I can't see anything!", newBlocks[i].GetText(true));
			Assert.AreEqual("{2}\u00A0Reference text for v2.", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("blind man", newBlocks[i].CharacterId);
			Assert.AreEqual("Walter", newBlocks[i].CharacterIdInScript);

			Assert.AreEqual("Then Walter said: ", newBlocks[++i].GetText(true));
			Assert.AreEqual("he said", newBlocks[i].GetPrimaryReferenceText());
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(narrator, newBlocks[i].CharacterId);

			Assert.AreEqual("{3}\u00A0What the blind man says in verse 3. ", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual("blind man", newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);

			Assert.AreEqual("{4}\u00A0thus spake he forthwith.", newBlocks[++i].GetText(true));
			Assert.IsFalse(newBlocks[i].MatchesReferenceText);
			Assert.AreEqual(MultiBlockQuote.None, newBlocks[i].MultiBlockQuote);
			Assert.AreEqual(narrator, newBlocks[i].CharacterId);
			Assert.IsNull(newBlocks[i].CharacterIdOverrideForScript);
		}
		#endregion

		#region Private Helper methods
		/// <summary>
		/// Options to tell GetTestBlocks when/whether to explicitly assign the known narrator
		/// override character to direct-speech block(s) for the verses where he might speak.
		/// </summary>
		public enum TextExplicitOverrides
		{
			/// <summary>
			/// All known verses in each narrator-override range should have a block explicitly
			/// spoken by the override character. (This effectively suppresses the override behavior.)
			/// </summary>
			All,
			/// <summary>
			/// Some (roughly half) of the known verses in each narrator-override range should have
			/// a block explicitly spoken by the override character.
			/// </summary>
			Some,
			/// <summary>
			/// No blocks in any narrator-override range should have their character IDs explicitly
			/// assigned to the override character.
			/// </summary>
			None,
		}

		private BookScript GetTestBook(ScrVersType versType, string bookId, TextExplicitOverrides option, int missingVerseOffset = -1)
		{
			var scrVers = new ScrVers(versType);
			return new BookScript(bookId, GetTestBlocks(scrVers, bookId, option, missingVerseOffset), scrVers);
		}

		/// <summary>
		/// Gets a "dummy" book with blocks covering all (or all but one, if <paramref name="missingVerseOffset"/> is specified)
		/// verses in all chapters. Some may be (very big) verse bridges. Details of what the blocks will look like and which
		/// character will be assigned depends on the chapter, what narrator overrides exist for the book, and the
		/// <paramref name="option"/> specified. This doesn't make for the most readable test setup, but it does make it easy to
		/// run lots of different iterations of tests over a variety of scenarios. To really understand it, the easiest thing is
		/// probably to set a break point and examine the blocks that get returned.
		/// </summary>
		private List<Block> GetTestBlocks(ScrVers scrVers, string bookId, TextExplicitOverrides option, int missingVerseOffset = -1)
		{
			var bookNum = BCVRef.BookToNumber(bookId);

			List<Block> blocks;
			var narrator = CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);
			blocks = new List<Block>();
			for (int c = 1; c <= scrVers.GetLastChapter(bookNum); c++)
			{
				var refOfFirstVerse = scrVers.FirstIncludedVerse(bookNum, c);
				if (refOfFirstVerse == null)
					continue;
				blocks.Add(NewChapterBlock(c, bookId));
				var lastVerseInChapter = scrVers.GetLastVerse(bookNum, c);
				var firstVerse = ((VerseRef)refOfFirstVerse).VerseNum;
				for (int v = firstVerse; v <= lastVerseInChapter; v++)
				{
					var block = new Block("p", c, v, v);
					if (option == TextExplicitOverrides.None || option == TextExplicitOverrides.Some && (v % 2 == 0))
					{
						block.CharacterId = narrator;
					}
					else
					{
						var verseRef = new VerseRef(bookNum, c, v, scrVers);
						var overrideInfo = NarratorOverrides.GetCharacterOverrideDetailsForRefRange(verseRef, v).FirstOrDefault();
						if (overrideInfo != null)
						{
							if (c == overrideInfo.StartChapter && v == overrideInfo.StartVerse + (missingVerseOffset == 0 ? 1 :0) &&
								(option == TextExplicitOverrides.All))
							{
								block.IsParagraphStart = true;
								block.CharacterId = narrator;
								block.BlockElements.Add(new Verse(v.ToString()));
								block.BlockElements.Add(new ScriptText("Then the override character said: "));
								blocks.Add(block);
								block = new Block("p", c, v, v) { CharacterId = overrideInfo.Character };
								block.BlockElements.Add(new ScriptText("The first thing I want to say is... "));
								blocks.Add(block);
								continue;
							}
							if (missingVerseOffset >= 0 && (c == overrideInfo.StartChapter && v == overrideInfo.StartVerse + missingVerseOffset ||
								c > overrideInfo.StartChapter && v == 1 + missingVerseOffset))
							{
								continue; // This is the one verse in this chapter for this override that we'll omit
							}
							if (c % 3 != 1 && v > 2)
							{
								var lastVerseToCombine = lastVerseInChapter - 2;
								if (overrideInfo.EndChapter == c && overrideInfo.EndVerse - 1 < lastVerseToCombine)
									lastVerseToCombine = overrideInfo.EndVerse - 1;
								while (v < lastVerseToCombine)
								{
									block.BlockElements.Add(new Verse(v++.ToString()));
									block.BlockElements.Add(new ScriptText("Multi-verse para."));
								}
							}
							block.CharacterId = overrideInfo.Character ?? narrator;
						}
						else
							block.CharacterId = narrator;
					}
					block.BlockElements.Add(new Verse(v.ToString()));
					block.BlockElements.Add(new ScriptText("Blah."));
					blocks.Add(block);
				}
			}
			return blocks;
		}

		private Block NewTitleBlock(string text, string bookCodeToSetCharacterId)
		{
			var block = new Block("mt");
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText(text));
			block.SetStandardCharacter(bookCodeToSetCharacterId, CharacterVerseData.StandardCharacter.BookOrChapter);
			return block;
		}

		private Block NewChapterBlock(int chapterNum, string bookCodeToSetCharacterId = null)
		{
			var block = new Block("c", chapterNum);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText(chapterNum.ToString()));
			m_curSetupChapter = chapterNum;
			m_curSetupVerse = 0;
			m_curSetupVerseEnd = 0;
			m_curStyleTag = null;
			if (bookCodeToSetCharacterId != null)
				block.SetStandardCharacter(bookCodeToSetCharacterId, CharacterVerseData.StandardCharacter.BookOrChapter);
			return block;
		}

		private Block NewSingleVersePara(int verseNum, string text = null, string bookCodeToSetNarrator = null)
		{
			m_curSetupVerse = verseNum;
			m_curSetupVerseEnd = 0;
			var block = new Block("p", m_curSetupChapter, verseNum).AddVerse(verseNum, text);
			block.IsParagraphStart = true;
			if (bookCodeToSetNarrator != null)
				block.SetStandardCharacter(bookCodeToSetNarrator, CharacterVerseData.StandardCharacter.Narrator);
			m_curStyleTag = "p";
			return block;
		}

		private Block NewVerseBridgePara(int verseNumStart, int verseNumEnd, string text = null)
		{
			var block = new Block("p", m_curSetupChapter, verseNumStart, verseNumEnd).AddVerse(
				string.Format("{0}-{1}", verseNumStart, verseNumEnd), text);
			block.IsParagraphStart = true;
			m_curStyleTag = "p";
			m_curSetupVerse = verseNumStart;
			m_curSetupVerseEnd = verseNumEnd;
			return block;
		}

		private Block NewPara(string styleTag, string text, string bookCodeToSetCharacterId = null)
		{
			m_curStyleTag = styleTag;
			var block = NewBlock(text);
			block.IsParagraphStart = true;
			if (styleTag == "s" && bookCodeToSetCharacterId != null)
				block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			return block;
		}

		private Block NewBlock(string text, string simpleCharacterId = null)
		{
			Debug.Assert(m_curStyleTag != null);
			var block = new Block(m_curStyleTag, m_curSetupChapter, m_curSetupVerse, m_curSetupVerseEnd);
			block.BlockElements.Add(new ScriptText(text));
			block.CharacterId = simpleCharacterId;
			return block;
		}

		private BookScript CreateStandardMarkScript(bool includeExtraVersesInChapter1 = false, bool includeBookTitle = false)
		{
			return new BookScript("MRK", GetStandardMarkScriptBlocks(includeExtraVersesInChapter1, includeBookTitle), m_testVersification);
		}

		private IList<Block> GetStandardMarkScriptBlocks(bool includeExtraVersesInChapter1 = false, bool includeBookTitle = false)
		{
			m_curSetupChapter = 1;
			m_curSetupVerse = 0;
			m_curSetupVerseEnd = 0;
			m_curStyleTag = null;

			var mrkBlocks = new List<Block>();

			if (includeBookTitle)
				mrkBlocks.Add(NewTitleBlock("Gospel According to Mark", "MRK"));

			mrkBlocks.Add(NewChapterBlock(1, "MRK"));
			mrkBlocks.Add(NewPara("s", "Predicación de Juan el Bautista", "MRK"));
			mrkBlocks.Add(NewSingleVersePara(1, "Principio del evangelio de Jesucristo, el Hijo de Dios. ", "MRK")
				.AddVerse(2, "Como está escrito en el profeta Isaías:"));
			m_curSetupVerse = 2;
			mrkBlocks.Add(NewPara("q1", "«Yo envío a mi mensajero delante de ti, El cual preparará tu camino.")
				.AddVerse(3, "Una voz clama en el desierto: “Preparen el camino del Señor; Enderecen sus sendas.”»"));
			m_curSetupVerse = 3;
			mrkBlocks.Last().SetCharacterAndDelivery(m_interruptionFinder, new CharacterVerse[0]);
			mrkBlocks.Add(NewSingleVersePara(4, "Juan se presentó en el desierto, y bautizaba y proclamaba el bautismo de arrepentimiento para el perdón de pecados. ", "MRK")
				.AddVerse(5, "Toda la gente de la provincia de Judea y de Jerusalén acudía a él, y allí en el río Jordán confesaban sus pecados, y Juan los bautizaba."));
			m_curSetupVerse = 5;

			if (includeExtraVersesInChapter1)
			{
				mrkBlocks.Last().AddVerse(6, "La ropa de Juan era de pelo de camello, alrededor de la cintura llevaba un cinto de cuero, y se alimentaba de langostas y miel silvestre. ")
					.AddVerse(7, "Al predicar, Juan decía: ");

				m_curSetupVerse = 7;

				mrkBlocks.Add(NewBlock(
						"«Después de mí viene uno más poderoso que yo. ¡Yo no soy digno de inclinarme ante él para desatarle la correa de su calzado! ")
						.AddVerse(8, "A ustedes yo los he bautizado con agua, pero él los bautizará con el Espíritu Santo.»"));
				m_curSetupVerse = 8;
				mrkBlocks.Last().SetCharacterAndDelivery(m_interruptionFinder, new CharacterVerse[0]);
			}

			mrkBlocks.Add(NewPara("s", "El bautismo de Jesús", "MRK"));
			mrkBlocks.Add(NewSingleVersePara(9, "Por esos días llegó Jesús desde Nazaret de Galilea, y fue bautizado por Juan en el Jordán. ", "MRK")
				.AddVerse(10, "En cuanto Jesús salió del agua, vio que los cielos se abrían y que el Espíritu descendía sobre él como una paloma. ")
				.AddVerse(11, "Y desde los cielos se oyó una voz que decía: "));
			m_curSetupVerse = 11;
			mrkBlocks.Add(NewBlock("«Tú eres mi Hijo amado, en quien me complazco.»"));
			mrkBlocks.Last().SetCharacterAndDelivery(m_interruptionFinder, new CharacterVerse[0]);

			if (includeExtraVersesInChapter1)
			{
				mrkBlocks.Add(NewSingleVersePara(14,
					"Después de que Juan fue encarcelado, Jesús fue a Galilea para proclamar el evangelio del reino de Dios.", "MRK")
					.AddVerse(15, "Decía: "));
				m_curSetupVerse = 15;
				mrkBlocks.Add(NewBlock("«El tiempo se ha cumplido, y el reino de Dios se ha acercado. ¡Arrepiéntanse, y crean en el evangelio!»"));
				mrkBlocks.Last().SetCharacterAndDelivery(m_interruptionFinder, new CharacterVerse[0]);
			}

			return mrkBlocks;
		}

		private BookScript CreateStandard1CorinthiansScript(ScrVers versification)
		{
			return new BookScript("1CO", GetStandard1CorinthiansScriptBlocks(), versification);
		}

		private const string k1Co15V1Text = "Alai naeng ma pabotohononku tu. ";
		private const string k1Co15V2Text = "Laos i do parhiteanmuna gabe malua. ";
		private const string k1Co15V26Text = "Hamatean i do musu na parpudi sipasohotonna i. ";
		private const string k1Co15V27Text = "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi.";

		private IList<Block> GetStandard1CorinthiansScriptBlocks()
		{
			m_curSetupChapter = 1;
			m_curSetupVerse = 0;
			m_curSetupVerseEnd = 0;
			m_curStyleTag = null;

			var blocks = new List<Block>();

			blocks.Add(NewTitleBlock("Paul's First Letter to the Church at Corinth", "1CO"));

			blocks.Add(NewChapterBlock(14, "1CO"));
			blocks.Add(NewSingleVersePara(1, "Alai naeng ma pabotohononku tu. ", "1CO")
				.AddVerse(2, "Laos i dos parhiteanmuna gabe malua. ")
				.AddVerse(3, "Laos i three parhiteanmuna gabe malua. ")
				.AddVerse(4, "Laos i cuatro parhiteanmuna gabe malua. ")
				.AddVerse(5, "Laos i five parhiteanmuna gabe malua. ")
				.AddVerse(6, "Laos i seis parhiteanmuna gabe malua. ")
				.AddVerse(7, "Laos i seven parhiteanmuna gabe malua. "));

			blocks.Add(NewChapterBlock(15, "1CO"));
			blocks.Add(NewPara("s", "Taringot tu naung mate", "1CO"));
			blocks.Add(NewSingleVersePara(1, k1Co15V1Text, "1CO")
				.AddVerse(2, k1Co15V2Text)
				.AddVerse(3, "Ai sian mulana hujalo; on do:"));
			m_curSetupVerse = 3;
			blocks.Add(NewBlock("Naung dosanta, hombar tu na tarsurat i. ", CharacterVerseData.kAmbiguousCharacter)
				.AddVerse(4, "Naung tartanom do Ibana. ")
				.AddVerse(5, "Jadi dipataridahon do dirina. ")
				.AddVerse(6, "Dung i dipataridahon dirina sahali tu. ")
				.AddVerse(7, "Dung i dipataridahon dirina tu si Jakobus. ")
				.AddVerse(8, "Dung i dipataridahon dirina tu si Jakobus. ")
				.AddVerse(9, "Ai ahu do na ummetmet sian angka apostel. ")
				.AddVerse(10, "Alai na di ahu nuaeng. ")
				.AddVerse(25, "Ai ingkon mangarajai Ibana, rasirasa dipatunduk saluhut musu i tutoru ni patna. ")
				.AddVerse(26, k1Co15V26Text)
				.AddVerse(27, k1Co15V27Text));
			blocks.Last().IsParagraphStart = false;

			blocks.Add(NewChapterBlock(16, "1CO"));
			blocks.Add(NewPara("s", "Tumpak tu huria na di Jerusalem", "1CO"));
			blocks.Add(NewSingleVersePara(1, "Alai naeng ma pabotohononku tu. ", "1CO")
				.AddVerse(2, "Verse two Laos i do parhiteanmuna gabe malua. ")
				.AddVerse(3, "Third verse of chapter 16 i do parhiteanmuna gabe malua. ")
				.AddVerse(4, "Four i do parhiteanmuna gabe malua. ")
				.AddVerse(5, "Cinco i do parhiteanmuna gabe malua. ")
				.AddVerse(6, "This is chapter sixteen, verse six i do parhiteanmuna gabe malua. ")
				.AddVerse(7, "Seven i do parhiteanmuna gabe malua. ")
				.AddVerse(8, "Ocho i do parhiteanmuna gabe malua. ")
				.AddVerse(9, "Last verse of chapter 16 is nine i do parhiteanmuna gabe malua. "));

			return blocks;
		}
		#endregion

		#region Serialization/Deserialization Tests
		[Test]
		public void Roundtrip_HasUnappliedSplits_DataIsTheSame()
		{
			var bookScript = CreateStandardMarkScript();
			var blockToSplit = bookScript.Blocks.First(b => b.InitialStartVerseNumber > 0);
			var newBlock = bookScript.SplitBlock(blockToSplit, "1", 5);
			var newBookScript = CreateStandardMarkScript();
			newBookScript.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplit, newBlock });
			Assert.AreEqual(1, newBookScript.UnappliedSplits.Count);

			var serializedBookScript = XmlSerializationHelper.SerializeToString(newBookScript);

			var deserializedBookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(serializedBookScript);
			deserializedBookScript.Initialize(newBookScript.Versification); // Not really necessary for this test, but to avoid possible future errors or confusion.
			Assert.IsTrue(newBookScript.UnappliedSplits.SequenceEqual(deserializedBookScript.UnappliedSplits, new BlockListComparer()));
		}

		public class BlockListComparer : IEqualityComparer<IEnumerable<Block>>
		{
			public bool Equals(IEnumerable<Block> x, IEnumerable<Block> y)
			{
				if (x == null && y == null)
					return true;
				if (x == null || y == null)
					return false;

				return x.SequenceEqual(y, new BlockComparer());
			}

			public int GetHashCode(IEnumerable<Block> obj)
			{
				return obj.GetHashCode();
			}
		}
		#endregion

		[TestCase(true)]
		[TestCase(false)]
		public void Clone_AllMembersAndAutoPropertiesDeepCopied(bool singleVoice)
		{
			var blocks = GetStandardMarkScriptBlocks(true, true);
			var i = blocks.Count;
			blocks.Add(NewChapterBlock(2));
			blocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			blocks.Add(NewPara("s", "Predicación de Juan el Bautista (chapter 2)"));
			blocks[i++].SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			blocks.Add(NewSingleVersePara(1, "Principio del evangelio de Jesucristo, el Hijo de Dios. (chapter 2) ")
				.AddVerse(2, "Como está escrito en el profeta Isaías: (chapter 2) "));
			var orig = new BookScript("MRK", blocks, ScrVers.English);

			orig.MainTitle = "Main Title";
			orig.PageHeader = "Page Header";
			orig.SingleVoice = singleVoice;

			var block1 = new Block("m", 3, 2).AddVerse("2", "Verse 2 text.");
			var block2 = new Block("m", 3, 3).AddVerse("3", "Verse 3 text.");
			orig.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { block1, block2 });
			var origMark1_4Blocks = orig.GetBlocksForVerse(1, 4).ToList(); // Populates orig.m_chapterStartBlockIndices
			var origMark2_1Blocks = orig.GetBlocksForVerse(2, 1).ToList();

			var clone = orig.Clone(); 
			Assert.AreEqual("Main Title", clone.MainTitle);
			Assert.AreEqual("Page Header", clone.PageHeader);
			Assert.AreEqual("MRK", clone.BookId);
			Assert.AreEqual(singleVoice, clone.SingleVoice);
			Assert.IsTrue(orig.Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.Blocks.Select(b => b.GetText(true))));
			Assert.IsFalse(orig.Blocks.SequenceEqual(clone.Blocks));
			Assert.AreEqual(1, clone.UnappliedSplits.Count);
			Assert.IsTrue(orig.UnappliedSplits[0].Select(b => b.GetText(true)).SequenceEqual(clone.UnappliedSplits[0].Select(b => b.GetText(true))));
			Assert.IsFalse(orig.UnappliedSplits.SequenceEqual(clone.UnappliedSplits));
			Assert.IsFalse(orig.UnappliedSplits[0].SequenceEqual(clone.UnappliedSplits[0]));

			var clonedMark1_4Blocks = clone.GetBlocksForVerse(1, 4);
			Assert.IsTrue(origMark1_4Blocks.Select(b => b.GetText(true)).SequenceEqual(orig.GetBlocksForVerse(1, 4).Select(b => b.GetText(true))));
			clone.SplitBlock(clonedMark1_4Blocks.First(), "4", BookScript.kSplitAtEndOfVerse, false);
			Assert.IsTrue(origMark1_4Blocks.SequenceEqual(orig.GetBlocksForVerse(1, 5)));
			Assert.IsTrue(origMark2_1Blocks.SequenceEqual(orig.GetBlocksForVerse(2, 1)));
			Assert.IsFalse(origMark1_4Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.GetBlocksForVerse(1, 4).Select(b => b.GetText(true))));
			Assert.IsTrue(origMark2_1Blocks.Select(b => b.GetText(true)).SequenceEqual(clone.GetBlocksForVerse(2, 1).Select(b => b.GetText(true))));
		}
	}

	internal static class BlockTestExtensions
	{
		static readonly Random s_random = new Random(42);

		internal static Block AddVerse(this Block block, int verseNum, string text = null)
		{
			return block.AddVerse(verseNum.ToString(), text);
		}

		internal static Block AddVerse(this Block block, string verse, string text = null)
		{
			if (text == null)
				text = RandomString();
			block.BlockElements.Add(new Verse(verse));
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		internal static Block AddText(this Block block, string text = null)
		{
			if (text == null)
				text = RandomString();
			block.BlockElements.Add(new ScriptText(text));
			return block;
		}

		internal static string RandomString()
		{
			var chars = " AAAAABB CCDDD EEEEFF GGHHIIJK LLMMNNN OOPPP QRRRS SSTTTTU VWWXYYZ aaaaaabb cccddd eeeeefff gggghhh iiiiijjk llll mmmnnnn ooooo pppp qqrrrr sssss tttttuu vvwwwxyyz ,,,.... !?? AAAAABB CCDDD EEEEFF GGHHIIJK LLMMNNN OOPPP QRRRS SSTTTTU VWWXYYZ aaaaaabb cccddd eeeeefff gggghhh iiiiijjk llll mmmnnnn ooooo pppp qqrrrr sssss tttttuu vvwwwxyyz ,,,.... !??\u2014";
			var randomString = new StringBuilder();
			var length = 4 + s_random.Next(80);

			for (int i = 0; i < length; i++)
				randomString.Append(chars[s_random.Next(chars.Length)]);

			return randomString.ToString();
		}
	}
}
