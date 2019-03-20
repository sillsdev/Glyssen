using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen;
using Glyssen.Character;
using Glyssen.Shared;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Extensions;
using SIL.IO;
using SIL.Scripture;
using SIL.Xml;

namespace GlyssenTests
{
	[TestFixture]
	class BookScriptTests
	{
		private int m_curSetupChapter;
		private int m_curSetupVerse;
		private int m_curSetupVerseEnd;
		private string m_curStyleTag;

		private ScrVers m_testVersification;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Implementation.Load(tempFile.Path);
			}
		}

		#region GetVerseText Tests
		[Test]
		public void GetVerseText_NoBlocks_ReturnsEmptyString()
		{
			var bookScript = new BookScript("MRK", new List<Block>());
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!", bookScript.GetVerseText(1, 4));
		}

		[Test]
		public void GetVerseText_VerseInLastBlockInChapterIsRequested_ReturnsVerseContents()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2).AddVerse(3).AddVerse(4, "This is it!").AddVerse(5));
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.AreEqual("This is it!" + Environment.NewLine + "This is more of it." + Environment.NewLine + "This is the rest of it.", bookScript.GetVerseText(1, 3));
		}
		#endregion

		#region GetFirstBlockForVerse Tests
		[Test]
		public void GetFirstBlockForVerse_NoBlocks_ReturnsNull()
		{
			var bookScript = new BookScript("MRK", new List<Block>());
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			var block = bookScript.GetFirstBlockForVerse(1, 4);
			Assert.AreEqual(mrkBlocks.Last(), block);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("REV", revBlocks);
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
			var bookScript = new BookScript("ACT", blocks);
			var list = bookScript.GetBlocksForVerse(8, 37).ToList();
			Assert.AreEqual(4, list.Count);
			Assert.AreEqual(expected, list[0]);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			Assert.IsTrue(bookScript.GetScriptBlocks(false).SequenceEqual(mrkBlocks));
			Assert.IsTrue(bookScript.GetScriptBlocks().SequenceEqual(mrkBlocks)); // Same as above but proves that false is the default
		}

		[Test]
		public void GetScriptBlocks_Joining_ConsecutiveNarratorBlocksInTheSameParagraph_ResultsIncludeJoinedBlocks()
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

			var bookScript = new BookScript("MRK", mrkBlocks);
			var result = bookScript.GetScriptBlocks(true);
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
		public void GetScriptBlocks_Joining_ConsecutiveBcBlocksInDifferentParagraphs_ResultsNotJoined()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewTitleBlock("The Gospel According to Mark", "MRK"));
			mrkBlocks.Add(NewChapterBlock(1));

			var bookScript = new BookScript("MRK", mrkBlocks);
			var result = bookScript.GetScriptBlocks(true);
			Assert.AreEqual(2, result.Count);
		}

		[TestCase(" ")]
		[TestCase("")]
		public void GetScriptBlocks_Joining_VernacularContainsQBlocks_VernacularBlocksBySameSpeakerCombined(string trailingWhitespace)
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("«This is line 1, This is line 2, This is line 3, This is line 4.»",
				result[1].GetText(true));
		}

		[TestCase(" ")]
		[TestCase("")]
		public void GetScriptBlocks_Joining_VernacularContainsQBlocks_VernacularBlocksForDifferentVersesNotCombined(string trailingWhitespace)
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("«This is line 1, This is line 2,", result[1].GetText(true).TrimEnd());
			Assert.AreEqual("{2}\u00A0This is line 3, This is line 4.»", result[2].GetText(true));
		}

		[Test]
		public void GetScriptBlocks_Joining_VernacularContainsQBlocksIntroducedByPBlock_AllVernacularBlocksCombined()
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("{1}\u00A0Peter said, 'This is line 1, 'This is line 2, 'This is line 3, 'This is line 4, ",
				result[0].GetText(true));
		}

		[Test]
		public void GetScriptBlocks_Joining_VernacularContainsQBlocksWithReferenceTextBlocks_VernacularBlocksCombined()
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
			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.All(b => b.MatchesReferenceText));
			Assert.AreEqual("rt0", result[0].GetPrimaryReferenceText());
			Assert.AreEqual("rt1 rt2 rt3 rt4", result[1].GetPrimaryReferenceText());
		}

		[TestCase(".", "...")]
		[TestCase(".", "\u2026")]
		[TestCase(",", "...")]
		[TestCase(",", "\u2026")]
		public void GetScriptBlocks_Joining_PrimaryReferenceTextStartsWithElipsesBeforeVerse_BlocksCombined(string vernBlock1EndingPunctuation, string ellipsis)
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation +
				" starting with the first patriarch: {2}\u00A0Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual("{1}\u00A0This is the genealogy of Jesus. {2}\u00A0Abraham was first.", result[0].GetPrimaryReferenceText());
		}

		[TestCase(".", "p", "{1} This is not empty.", " ", "{1}\u00A0This is not empty.")]
		[TestCase("?", "li", " ", "{1} Neither is this.", "{1}\u00A0Neither is this.")]
		[TestCase("!", "q1", "{1}", "The other block's ref text was just a verse number.", "{1}\u00A0The other block's ref text was just a verse number.")]
		public void GetScriptBlocks_JoiningWithBlankReferenceTexts_AdjacentSentencesInSameParagraphAndVerse_BlocksCombined(string vernBlock1EndingPunctuation,
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation.TrimEnd() +
				" Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual(expectedCombinedRefText, result[0].GetPrimaryReferenceText());
		}

		[TestCase(";", "p", "{1} This is not empty, ", "okay?", "{1}\u00A0This is not empty, okay?")]
		[TestCase(",", "pi2", "This is just another way", " to say that.", "This is just another way to say that.")]
		[TestCase("", "q1", "{1}My soul will rejoice. Yes, it will sing;", "Because it is happy!", "{1}\u00A0My soul will rejoice. Yes, it will sing; Because it is happy!")]
		public void GetScriptBlocks_JoiningWithReferenceTextsInSameVerseAndSentence_BlocksCombined(string vernBlock1EndingPunctuation,
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
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
		public void GetScriptBlocks_JoiningWithReferenceTexts_VernBlockEndsInSentenceEndingPunctuation_BlocksNotCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
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
		public void GetScriptBlocks_JoiningWithReferenceTexts_BlockBHasVerseNumber_BlocksNotCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(origRefText1, result[0].GetPrimaryReferenceText());
			Assert.AreEqual(origRefText2, result[1].GetPrimaryReferenceText());
		}

		[Test]
		public void GetScriptBlocks_JoiningWithReferenceTexts_SingleVoice_BlocksCombinedByOrigParagraphAndReferenceTextIgnored()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.JUD);
			var jude = testProject.IncludedBooks.Single();
			var countOfOrigParagraphs = jude.GetScriptBlocks().Count(b => b.IsParagraphStart || CharacterVerseData.IsCharacterOfType(b.CharacterId, CharacterVerseData.StandardCharacter.BookOrChapter));
			foreach (var block in jude.GetScriptBlocks())
			{
				if (block.CharacterIsUnclear())
					block.CharacterId = "Paul";
				block.SetMatchedReferenceBlock("blah");
			}
			jude.SingleVoice = true;

			var result = jude.GetScriptBlocks(true);
			Assert.AreEqual(countOfOrigParagraphs, result.Count);
			Assert.IsTrue(result.All(b => b.CharacterIsStandard));
			Assert.IsFalse(result.Any(b => b.MatchesReferenceText));
		}

		[TestCase(";", "q1", "", "Part 2.", "Part 2.")]
		[TestCase(" - ", "p", "{1}   ", "Part 2", "{1}\u00A0Part 2")]
		[TestCase(",", "pi1", "{1} Part 1.", " ", "{1}\u00A0Part 1.")]
		[TestCase(",", "m", "Part 1", "", "Part 1")]
		public void GetScriptBlocks_Joining_PrimaryReferenceTextIsBlank_BlocksCombined(string vernBlock1EndingPunctuation, string block2StyleTag,
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

			var vernBook = new BookScript("MAT", vernacularBlocks);

			var result = vernBook.GetScriptBlocks(true);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result[0].MatchesReferenceText);
			Assert.AreEqual("{1}\u00A0Este es la genealogia de Jesus" + vernBlock1EndingPunctuation.TrimEnd() +
				" Abraham fue el primero.", result[0].GetText(true));
			Assert.AreEqual(expectedCombinedRefText, result[0].GetPrimaryReferenceText());
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
			Assert.IsTrue(target.GetScriptBlocks().All(b => b.CharacterIsStandard || b.CharacterId == CharacterVerseData.kUnknownCharacter));
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnknownCharacter);
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
				if (target[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnknownCharacter);
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
				if (target[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
						Assert.IsTrue(target[i].CharacterId == CharacterVerseData.kUnknownCharacter);
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
				if (target[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
						Assert.AreEqual(CharacterVerseData.kUnknownCharacter, target[i].CharacterId);
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
				if (source[i].CharacterId == CharacterVerseData.kUnknownCharacter)
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
					if (source[i].CharacterIsUnclear())
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
					Assert.IsFalse(target[i].CharacterIsUnclear());
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
					if (source[i].CharacterIsUnclear())
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
					Assert.IsFalse(target[i].CharacterIsUnclear());
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
			var source = CreateStandard1CorinthiansScript();
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, 26), ScrVers.English);
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var blockToSplit = source.GetIndexOfFirstBlockForVerse(15, 26);
			source.SplitBlock(source.GetScriptBlocks()[blockToSplit], "26", 14);

			var target = CreateStandard1CorinthiansScript();
			var scriptTextForV26 = target.GetScriptBlocks().First(b => b.ChapterNumber == 15 && b.InitialStartVerseNumber <= 26 && b.LastVerseNum >= 26).BlockElements
				.SkipWhile(e => !(e is Verse) || ((Verse)e).Number != "26").OfType<ScriptText>().First();
			scriptTextForV26.Content = scriptTextForV26.Content.Replace('a', 'q');
			var countOfTargetBlocksBeforeApplyingSplits = target.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
			var targetBlocksAfterApplyingUserDecisions = target.GetScriptBlocks();
			Assert.AreEqual(countOfTargetBlocksBeforeApplyingSplits, targetBlocksAfterApplyingUserDecisions.Count);
			Assert.IsFalse(source.GetScriptBlocks().SequenceEqual(targetBlocksAfterApplyingUserDecisions, new BlockComparer()));
			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(1, target.UnappliedSplits.Count);
		}

		// 1CO 15:26
		// Verse 1: "Alai naeng ma pabotohononku tu. "
		// Verse 2: "Laos i do parhiteanmuna gabe malua. "
		// Verse 26: "Hamatean i do musu na parpudi sipasohotonna i. "
		// Verse 27: "Ai saluhutna do dipatunduk tutoru ni patna. Alai molo saluhut dipatunduk didok, tandap ma disi. "
		[TestCase(1, 14, k1Co15V1Text)]
		[TestCase(2, 5, k1Co15V2Text)]
		[TestCase(26, 14, k1Co15V26Text)]
		[TestCase(27, 44, k1Co15V27Text)]
		public void ApplyUserDecisions_UnalignedSplitBlockInChunkPreviouslyAlignedToReferenceText_TextUnchanged_ManualSplitAndPrecedingRefTextSplitApplied(int verseNum,
			int splitPos, string fullVerseText)
		{
			var source = CreateStandard1CorinthiansScript();
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			
			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, verseNum), ScrVers.English);
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			// We expect one reapplied split at the start of each preceding verse number (except v. 1) in the chapter
			// (because all those verses were matched up to the reference text and still match the corresponding target blocks.
			var countOfExpectedReappliedSplits = matchup.CorrelatedBlocks.Count(b => b.StartsAtVerseStart && b.InitialStartVerseNumber < verseNum);

			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText, source.GetScriptBlocks().Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, verseNum);
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], verseNum.ToString(), splitPos);
			// Plus the manual split in the verse itself:
			countOfExpectedReappliedSplits++;

			var expectedRematchedBlocks = source.GetScriptBlocks().Where(b => b.IsScripture && b.ChapterNumber == 15 &&
				b.InitialStartVerseNumber < verseNum).ToList();

			var target = CreateStandard1CorinthiansScript();
			var countOfTargetBlocksBeforeApplyingSplits = target.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();

			Assert.AreEqual(countOfTargetBlocksBeforeApplyingSplits + countOfExpectedReappliedSplits, targetBlocksAfterApplyingSplit.Count);
			var comparer = new BlockComparer();
			if (targetBlocksAfterApplyingSplit.Count == source.GetScriptBlocks().Count)
			{
				// This happens to be a special case where we can do a better job of preserving alignments.
				Assert.IsTrue(source.GetScriptBlocks().SequenceEqual(targetBlocksAfterApplyingSplit, comparer));
			}
			else
			{
				Assert.IsTrue(expectedRematchedBlocks.SequenceEqual(
					targetBlocksAfterApplyingSplit.SkipWhile(b => !b.IsScripture || b.ChapterNumber != 15).TakeWhile(b => b.MatchesReferenceText),
					comparer));
			}
			var firstBlockOfSplit = targetBlocksAfterApplyingSplit.First(b => b.ChapterNumber == 15 && b.LastVerseNum == verseNum);
			Assert.AreEqual(fullVerseText.Substring(0, splitPos), ((ScriptText)firstBlockOfSplit.BlockElements.Last()).Content);
			var lastBlockOfSplit = targetBlocksAfterApplyingSplit.Last(b => b.ChapterNumber == 15 && b.InitialStartVerseNumber == verseNum);
			Assert.AreEqual(fullVerseText.Substring(splitPos), ((ScriptText)lastBlockOfSplit.BlockElements.First()).Content);

			Assert.IsNotNull(target.UnappliedSplits);
			Assert.AreEqual(0, target.UnappliedSplits.Count);
		}

		// 1CO 15:26
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
			var source = CreateStandard1CorinthiansScript();
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, verseNum);
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], verseNum.ToString(), splitPos);

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, verseNum), ScrVers.English);
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			foreach (var block in matchup.CorrelatedBlocks.SkipWhile(b => b.ChapterNumber < 15 || b.InitialStartVerseNumber < verseNum).Skip(1).TakeWhile(b => b.SplitId == 0 || b.IsContinuationOfPreviousBlockQuote))
			{
				block.CharacterId = "Walter";
				block.UserConfirmed = true;
				block.MultiBlockQuote = MultiBlockQuote.None;
			}
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText + 1, source.GetScriptBlocks().Count);

			var target = CreateStandard1CorinthiansScript();

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
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
			var origSource = CreateStandard1CorinthiansScript();
			var origBlockCount = origSource.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(origSource, ScrVers.English);
			var sourceBlocksChunkedOut = origSource.GetScriptBlocks();
			// ICO 15:3 has a paragraph break, so we need to match up the second half of the verse
			sourceBlocksChunkedOut.Single(b => b.IsScripture && !b.MatchesReferenceText).SetMatchedReferenceBlock("Blah blah");
			var narrator = CharacterVerseData.GetStandardCharacterId(origSource.BookId, CharacterVerseData.StandardCharacter.Narrator);
			var source = new BookScript(origSource.BookId, sourceBlocksChunkedOut);
			Assert.IsTrue(origBlockCount < sourceBlocksChunkedOut.Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, 27);
			var firstBlockforV27 = source.GetScriptBlocks()[iBlockToSplit];
			var v27SplitPos = 44;
			source.SplitBlock(firstBlockforV27, "27", v27SplitPos, true, narrator, ScrVers.English);

			firstBlockforV27.SetNonDramaticCharacterId("scripture");
			firstBlockforV27.UserConfirmed = true;

			var blockForV26 = source.GetScriptBlocks()[iBlockToSplit - 1];
			Assert.AreEqual("26", ((Verse)blockForV26.BlockElements.First()).Number);
			source.SplitBlock(blockForV26, "26", k1Co15V26Text.Length, true, "scripture", ScrVers.English).UserConfirmed = true;

			foreach (var sb in source.GetScriptBlocks().Where(b => b.CharacterIsUnclear()))
			{
				sb.MultiBlockQuote = MultiBlockQuote.None;
				sb.SetNonDramaticCharacterId(narrator);
			}

			var target = CreateStandard1CorinthiansScript();
			var countOfSourceBlocks = source.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(countOfSourceBlocks, source.GetScriptBlocks().Count, "Sanity check");
			Assert.AreEqual(countOfSourceBlocks, targetBlocksAfterApplyingSplit.Count);
			var blockWithVerse26 = targetBlocksAfterApplyingSplit.Single(b => b.ChapterNumber == 15 && b.LastVerseNum == 26);
			Assert.AreEqual(k1Co15V26Text, ((ScriptText)blockWithVerse26.BlockElements.Last()).Content);
			var iBlockEndingInV26 = targetBlocksAfterApplyingSplit.IndexOf(blockWithVerse26);
			Assert.AreEqual("{27}\u00A0" + k1Co15V27Text.Substring(0, v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].GetText(true));
			Assert.AreEqual("scripture", targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].CharacterId);
			Assert.AreEqual(k1Co15V27Text.Substring(v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2].GetText(true));
			Assert.IsTrue(targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 2].CharacterId == narrator);
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
			var source = CreateStandard1CorinthiansScript();
			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);

			var blockForV26 = source.GetScriptBlocks()[source.GetIndexOfFirstBlockForVerse(15, 26)];
			source.SplitBlock(blockForV26, "26", k1Co15V26Text.Length, true, "scripture", ScrVers.English)
				.UserConfirmed = true;

			var matchup = englishRefText.GetBlocksForVerseMatchedToReferenceText(source,
				source.GetIndexOfFirstBlockForVerse(15, 26), ScrVers.English);
			var countOfSplitsFromApplyingReferenceText = matchup.CountOfBlocksAddedBySplitting;
			MatchUpBlocksAndApplyToSource(matchup);
			Assert.AreEqual(origBlockCount + countOfSplitsFromApplyingReferenceText + 1, source.GetScriptBlocks().Count);

			Assert.IsTrue(origBlockCount < source.GetScriptBlocks().Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(15, 27);
			var v27SplitPos = 44;
			source.SplitBlock(source.GetScriptBlocks()[iBlockToSplit], "27", v27SplitPos, true,
				CharacterVerseData.GetStandardCharacterId(source.BookId, CharacterVerseData.StandardCharacter.Narrator), ScrVers.English);

			var target = CreateStandard1CorinthiansScript();
			var countOfSourceBlocks = source.GetScriptBlocks().Count;

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
			var targetBlocksAfterApplyingSplit = target.GetScriptBlocks();
			Assert.AreEqual(countOfSourceBlocks, targetBlocksAfterApplyingSplit.Count);
			var blockWithVerse26 = targetBlocksAfterApplyingSplit.Single(b => b.ChapterNumber == 15 && b.LastVerseNum == 26);
			Assert.AreEqual(k1Co15V26Text, ((ScriptText)blockWithVerse26.BlockElements.Last()).Content);
			var iBlockEndingInV26 = targetBlocksAfterApplyingSplit.IndexOf(blockWithVerse26);
			Assert.AreEqual("{27}\u00A0" + k1Co15V27Text.Substring(0, v27SplitPos),
				targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].GetText(true));
			Assert.AreEqual("scripture", targetBlocksAfterApplyingSplit[iBlockEndingInV26 + 1].CharacterId);
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

			var source = new BookScript("MAT", blocks);
			var target = source.Clone(false);

			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(source, ScrVers.English);
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
			var narrator = CharacterVerseData.GetStandardCharacterId(source.BookId, CharacterVerseData.StandardCharacter.Narrator);

			var split4 = source.SplitBlock(blockToSplit, "31", kSplitPos4, true, "Jesus", ScrVers.English);
			split4.SetMatchedReferenceBlock("“Most certainly I tell you that the tax collectors and the prostitutes are entering into the Kingdom of God before you.");
			split4.UserConfirmed = true;

			var split3 = source.SplitBlock(blockToSplit, "31", kSplitPos3, true, narrator, ScrVers.English);
			split3.SetMatchedReferenceBlock("Jesus said to them,");
			split3.UserConfirmed = true;

			var split2 = source.SplitBlock(blockToSplit, "31", kSplitPos2, true, "chief priests/elders", ScrVers.English);
			split2.SetMatchedReferenceBlock("“The first.”");
			split2.UserConfirmed = true;

			var split1 = source.SplitBlock(blockToSplit, "31", kSplitPos1, true, narrator, ScrVers.English);
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

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
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
			Assert.AreEqual("{30}\u00A0He came to the second, and said the same thing. He answered, ‘I go, sir,’ but he didn’t go.",
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

			var source = new BookScript("MRK", blocks);
			var target = source.Clone(false);

			var origBlockCount = source.GetScriptBlocks().Count;
			var englishRefText = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
			englishRefText.ApplyTo(source, ScrVers.English);
			var sourceBlocksChunkedOut = source.GetScriptBlocks();
			Assert.IsTrue(origBlockCount < sourceBlocksChunkedOut.Count);

			var iBlockToSplit = source.GetIndexOfFirstBlockForVerse(8, 26);
			Assert.AreEqual(2, iBlockToSplit);
			var blockToSplit = source.GetScriptBlocks()[iBlockToSplit];

			var split = source.SplitBlock(blockToSplit, "26", kSplitPos, true, "Jesus", ScrVers.English);
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

			target.ApplyUserDecisions(source, ScrVers.English, englishRefText);
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

		/// <summary>
		/// Match up all blocks to corresponding blocks in the reference text. Anything left over gets "blindly" matched
		/// to an empty reference block and assigned to narrator.
		/// </summary>
		/// <param name="matchup"></param>
		private static void MatchUpBlocksAndApplyToSource(BlockMatchup matchup)
		{
			matchup.MatchAllBlocks(ScrVers.English);
			var narrator = CharacterVerseData.GetStandardCharacterId(matchup.BookId, CharacterVerseData.StandardCharacter.Narrator);
			foreach (var block in matchup.CorrelatedBlocks.Where(b => b.CharacterIsUnclear() ||
				(b.MultiBlockQuote != MultiBlockQuote.None && b.CharacterIsStandard)))
			{
				block.SetNonDramaticCharacterId(narrator);
				block.MultiBlockQuote = MultiBlockQuote.None;
			}
			matchup.Apply();
		}
		#endregion PG-1168

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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57, true, "Jesus", ScrVers.English);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "36", 57, true, "James/John", ScrVers.English);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "10", 52, true, "Peter (Simon)/James/John", m_testVersification);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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

			var bookScript = new BookScript("MRK", mrkBlocks);
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

			var bookScript = new BookScript("MRK", mrkBlocks);
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

			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
			var newBlock = bookScript.SplitBlock(blockToSplit, "2", 5);
			Assert.AreEqual("Bill", blockToSplit.CharacterId);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, newBlock.CharacterId);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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
			var bookScript = new BookScript("MRK", mrkBlocks);
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

			var bookScript = new BookScript("REV", blocks);
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

			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.CleanUpMultiBlockQuotes(ScrVers.English);
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

			var bookScript = new BookScript("MRK", mrkBlocks);

			bookScript.CleanUpMultiBlockQuotes(ScrVers.English);
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
		public void ReplaceBlocks_FollowingBlockIsNotContinuationOfQuote_NoChangesToFollowingBlock(MultiBlockQuote followingBlockMultiBlockQuoteType,
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
			var bookScript = new BookScript("MRK", mrkBlocks);

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

		[Test]
		public void ReplaceBlocks_LastReplacementBlockIsNoneFollowingBlockIsContinuationOfQuote_ThrowsArgumentException()
		{
			var mrkBlocks = new List<Block>();
			mrkBlocks.Add(NewChapterBlock(1));
			mrkBlocks.Add(NewSingleVersePara(1));
			mrkBlocks.Add(NewSingleVersePara(2));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Start;
			mrkBlocks.Add(NewSingleVersePara(3));
			mrkBlocks.Last().CharacterId = "blind man";
			mrkBlocks.Last().MultiBlockQuote = MultiBlockQuote.Continuation;
			var bookScript = new BookScript("MRK", mrkBlocks);

			var replacementBlocks = new List<Block>();
			replacementBlocks.Add(NewSingleVersePara(1, "This is the new text."));
			replacementBlocks[0].CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			replacementBlocks[0].SetMatchedReferenceBlock("{1} Reference text for v1.");
			replacementBlocks.Add(NewSingleVersePara(2, "This is the new text."));
			replacementBlocks[1].CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator);
			replacementBlocks[1].SetMatchedReferenceBlock("{2} Reference text for v2.");

			var exception = Assert.Throws<ArgumentException>(() => bookScript.ReplaceBlocks(1, 2, replacementBlocks));
			Assert.AreEqual("Last replacement block must have a MultiBlockQuote value of Start or Continuation, since the first " +
				"block following the replacement range is a Continuation block.", exception.Message);
			Assert.AreEqual(4, bookScript.GetScriptBlocks().Count, "No replacements should have been made");
		}

		[TestCase(MultiBlockQuote.Continuation)]
		[TestCase(MultiBlockQuote.Start)]
		public void ReplaceBlocks_FollowingBlocksAreContinuationOfQuote_CharacterIdUpdatedForFollowingBlocks(MultiBlockQuote finalReplacementBlockMultiBlockQuoteType)
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
			var bookScript = new BookScript("MRK", mrkBlocks);

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
				replacementBlocks.Last().CharacterIdOverrideForScript = "Walter";
			replacementBlocks.Last().SetMatchedReferenceBlock("{2} Reference text for v2.");
			replacementBlocks.Add(NewBlock("This is the new text, spoken by the neighbors."));
			replacementBlocks.Last().MultiBlockQuote = finalReplacementBlockMultiBlockQuoteType;
			replacementBlocks.Last().CharacterId = "neighbors";
			replacementBlocks.Last().CharacterIdOverrideForScript = "Walter";
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
		#endregion

		#region Private Helper methods
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
			block.IsParagraphStart = true;
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
			return new BookScript("MRK", GetStandardMarkScriptBlocks(includeExtraVersesInChapter1, includeBookTitle));
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
			mrkBlocks.Last().SetCharacterAndDelivery(new CharacterVerse[0]);
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
				mrkBlocks.Last().SetCharacterAndDelivery(new CharacterVerse[0]);
			}

			mrkBlocks.Add(NewPara("s", "El bautismo de Jesús", "MRK"));
			mrkBlocks.Add(NewSingleVersePara(9, "Por esos días llegó Jesús desde Nazaret de Galilea, y fue bautizado por Juan en el Jordán. ", "MRK")
				.AddVerse(10, "En cuanto Jesús salió del agua, vio que los cielos se abrían y que el Espíritu descendía sobre él como una paloma. ")
				.AddVerse(11, "Y desde los cielos se oyó una voz que decía: "));
			m_curSetupVerse = 11;
			mrkBlocks.Add(NewBlock("«Tú eres mi Hijo amado, en quien me complazco.»"));
			mrkBlocks.Last().SetCharacterAndDelivery(new CharacterVerse[0]);

			if (includeExtraVersesInChapter1)
			{
				mrkBlocks.Add(NewSingleVersePara(14,
					"Después de que Juan fue encarcelado, Jesús fue a Galilea para proclamar el evangelio del reino de Dios.", "MRK")
					.AddVerse(15, "Decía: "));
				m_curSetupVerse = 15;
				mrkBlocks.Add(NewBlock("«El tiempo se ha cumplido, y el reino de Dios se ha acercado. ¡Arrepiéntanse, y crean en el evangelio!»"));
				mrkBlocks.Last().SetCharacterAndDelivery(new CharacterVerse[0]);
			}

			return mrkBlocks;
		}

		private BookScript CreateStandard1CorinthiansScript()
		{
			return new BookScript("1CO", GetStandard1CorinthiansScriptBlocks());
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
			var orig = new BookScript("MRK", blocks);

			orig.MainTitle = "Main Title";
			orig.PageHeader = "Page Header";
			orig.SingleVoice = singleVoice;

			var block1 = new Block("m", 3, 2).AddVerse("2", "Verse 2 text.");
			var block2 = new Block("m", 3, 3).AddVerse("3", "Verse 3 text.");
			orig.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { block1, block2 });
			var origMark1_4Blocks = orig.GetBlocksForVerse(1, 4).ToList(); // Populates orig.m_chapterStartBlockIndices
			var origMark2_1Blocks = orig.GetBlocksForVerse(2, 1).ToList();

			// for the single voice case, we don't want to join blocks, because it re-combines them into paragraphs and prevents
			// block-by-block comparison.
			var clone = orig.Clone(!singleVoice);
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
