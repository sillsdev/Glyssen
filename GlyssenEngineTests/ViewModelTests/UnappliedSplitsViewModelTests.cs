﻿using System.Collections.Generic;
using System.Text;
using System.Xml;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine.Script;
using GlyssenEngine.ViewModels;
using GlyssenEngineTests.Script;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	class UnappliedSplitsViewModelTests
	{
		[Test]
		public void GetHtml_SingleSplitInSingleBook_HtmlIsCorrect()
		{
			var blocks = new List<Block> {
				NewChapterBlock(8, "MRK"),
				new Block("p", 8, 25) { CharacterId = Narrator("MRK")}
					.AddVerse(25, "Torang do saluhutna diida. ")
					// Offset:               1         2         3         4         5         6
					// Offset:     0123456789012345678901234567890123456789012345678901234567890123456789
					// Split:                                                     |
					.AddVerse(26, "Laos disuru ma ibana muli tu jabuna, didok ma: Unang bongoti huta i! ")
			};

			var mark = new BookScript("MRK", blocks, ScrVers.English);

			var blockToSplit = mark.GetFirstBlockForVerse(8, 26);
			var newBlock = Split(mark, blockToSplit, "26", 47);

			mark.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplit, newBlock });

			var model = new UnappliedSplitsViewModel(new [] {mark.Clone()}, false);
			var xmlDoc = new XmlDocument();
			var html = model.GetHtml();
			xmlDoc.LoadXml(html.Replace("&nbsp;", "&#160;"));
			var body = xmlDoc.ChildNodes[0].SelectSingleNode("body");
			var bookDiv = body.FirstChild;
			Assert.That(bookDiv.Name, Is.EqualTo("div"));
			Assert.That(bookDiv.Attributes.GetNamedItem("id").Value, Is.EqualTo("MRK"));
			var splitsDiv = bookDiv.ChildNodes[0];
			Assert.That(splitsDiv.Attributes.GetNamedItem("class").Value, Is.EqualTo("splits"));
			Assert.That(splitsDiv.ChildNodes[0].Value, Is.EqualTo("MRK 8:25-26"));
			var preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplit, false);
			Assert.That(splitsDiv.ChildNodes[2].Value, Is.EqualTo(" //SPLIT// "));
			var postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlock, false);
			Assert.That(bookDiv.ChildNodes[1].InnerText, Is.EqualTo("\u00A0"));
		}

		[Test]
		public void GetHtml_SingleSplitInEachOfTwoBooks_HtmlIsCorrect()
		{
			var blocks = new List<Block> {
				NewChapterBlock(8, "MRK"),
				new Block("p", 8, 25) { CharacterId = Narrator("MRK")}
					.AddVerse(25, "Torang do saluhutna diida. ")
					// Offset:               1         2         3         4         5         6
					// Offset:     0123456789012345678901234567890123456789012345678901234567890123456789
					// Split:                                                     |
					.AddVerse(26, "Laos disuru ma ibana muli tu jabuna, didok ma: Unang bongoti huta i! ")
			};

			var mark = new BookScript("MRK", blocks, ScrVers.English);

			var blockToSplitMark = mark.GetFirstBlockForVerse(8, 26);
			var newBlockMark = Split(mark, blockToSplitMark, "26", 47);

			mark.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplitMark, newBlockMark });

			blocks = new List<Block> {
				NewChapterBlock(10, "ACT"),
				new Block("p", 10, 5) { CharacterId = Narrator("ACT")}
					.AddVerse(5, "Blugga woo. ")
					.AddVerse(6, "Pooka new! ")
					// Offset:               1         2         3
					// Offset:     0123456789012345678901234567890123456789
					// Split:                           |
					.AddVerse(7, "Flabba Gabba punkifing, snerfdo blugtew! ")
			};

			var acts = new BookScript("ACT", blocks, ScrVers.English);

			var blockToSplitActs = acts.GetFirstBlockForVerse(10, 5);
			var newBlockActs = Split(acts, blockToSplitActs, "7", 21);

			acts.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplitActs, newBlockActs });

			var model = new UnappliedSplitsViewModel(new[] { mark.Clone(), acts.Clone() }, false);
			var xmlDoc = new XmlDocument();
			var html = model.GetHtml();
			xmlDoc.LoadXml(html.Replace("&nbsp;", "&#160;"));
			var body = xmlDoc.ChildNodes[0].SelectSingleNode("body");
			var bookDiv = body.ChildNodes[0];
			Assert.That(bookDiv.Name, Is.EqualTo("div"));
			Assert.That(bookDiv.Attributes.GetNamedItem("id").Value, Is.EqualTo("MRK"));
			var splitsDiv = bookDiv.ChildNodes[0];
			Assert.That(splitsDiv.Attributes.GetNamedItem("class").Value, Is.EqualTo("splits"));
			Assert.That(splitsDiv.ChildNodes[0].Value, Is.EqualTo("MRK 8:25-26"));
			var preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplitMark, false);
			Assert.That(splitsDiv.ChildNodes[2].Value, Is.EqualTo(" //SPLIT// "));
			var postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlockMark, false);
			Assert.That(bookDiv.ChildNodes[1].InnerText, Is.EqualTo("\u00A0"));

			bookDiv = body.ChildNodes[1];
			Assert.That(bookDiv.Name, Is.EqualTo("div"));
			Assert.That(bookDiv.Attributes.GetNamedItem("id").Value, Is.EqualTo("ACT"));
			splitsDiv = bookDiv.ChildNodes[0];
			Assert.That(splitsDiv.Attributes.GetNamedItem("class").Value, Is.EqualTo("splits"));
			Assert.That(splitsDiv.ChildNodes[0].Value, Is.EqualTo("ACT 10:5-7"));
			preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplitActs, false);
			Assert.That(splitsDiv.ChildNodes[2].Value, Is.EqualTo(" //SPLIT// "));
			postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlockActs, false);
			Assert.That(bookDiv.ChildNodes[1].InnerText, Is.EqualTo("\u00A0"));
		}

		[Test]
		public void GetHtml_SplitsInMultipleChaptersOfSingleBook_EachGroupOfSplitsHasReference()
		{
			var blocks = new List<Block> {
				NewChapterBlock(8, "MRK"),
				new Block("p", 8, 25) { CharacterId = Narrator("MRK")}
					.AddVerse(25, "Torang do saluhutna diida. ")
					// Offset:               1         2         3         4         5         6
					// Offset:     0123456789012345678901234567890123456789012345678901234567890123456789
					// Split:                                                     |
					.AddVerse(26, "Laos disuru ma ibana muli tu jabuna, didok ma: Unang bongoti huta i! "),
				NewChapterBlock(10, "MRK"),
				new Block("p", 10, 5) { CharacterId = Narrator("MRK")}
					.AddVerse(5, "Borang to kaluhutna qiida. ")
					// Offset:               1         2         3         4         5         6
					// Offset:     0123456789012345678901234567890123456789012345678901234567890123456789
					// Split:                           |                         |
					.AddVerse(6, "Kaos tisuru sa obana: nuli du zabuna, widok na: Enang pongoti luta o! ")
			};

			var mark = new BookScript("MRK", blocks, ScrVers.English);

			var blockToSplit1 = mark.GetFirstBlockForVerse(8, 26);
			var newBlock1 = Split(mark, blockToSplit1, "26", 47);

			var blockToSplit2 = mark.GetFirstBlockForVerse(10, 6);
			var newBlock2 = Split(mark, blockToSplit2, "6", 47);
			var newBlock3 = Split(mark, newBlock2, "6", 21);

			mark.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplit1, newBlock1, blockToSplit2,
				newBlock2, newBlock3});

			var model = new UnappliedSplitsViewModel(new[] { mark.Clone() }, false);
			var xmlDoc = new XmlDocument();
			var html = model.GetHtml();
			xmlDoc.LoadXml(html.Replace("&nbsp;", "&#160;"));
			var body = xmlDoc.ChildNodes[0].SelectSingleNode("body");
			var bookDiv = body.FirstChild;
			Assert.That(bookDiv.Name, Is.EqualTo("div"));
			Assert.That(bookDiv.Attributes.GetNamedItem("id").Value, Is.EqualTo("MRK"));
			var splitsDiv = bookDiv.ChildNodes[0];
			Assert.That(splitsDiv.Attributes.GetNamedItem("class").Value, Is.EqualTo("splits"));
			Assert.That(splitsDiv.ChildNodes[0].Value, Is.EqualTo("MRK 8:25-26"));
			var preSplitBlock1Div = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlock1Div, blockToSplit1, false);
			Assert.That(splitsDiv.ChildNodes[2].Value, Is.EqualTo(" //SPLIT// "));
			var postSplitBlock1Div = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlock1Div, newBlock1, false);
			Assert.That(splitsDiv.ChildNodes[4].Value, Is.EqualTo("MRK 10:5-6"));
			var preSplitBlock2Div = splitsDiv.ChildNodes[5];
			VerifyBlockHtml(preSplitBlock2Div, blockToSplit2, false);
			Assert.That(splitsDiv.ChildNodes[6].Value, Is.EqualTo(" //SPLIT// "));
			var postSplitBlock2Div = splitsDiv.ChildNodes[7];
			VerifyBlockHtml(postSplitBlock2Div, newBlock2, false);
			Assert.That(splitsDiv.ChildNodes[8].Value, Is.EqualTo(" //SPLIT// "));
			var lastSplitBlockDiv = splitsDiv.ChildNodes[9];
			VerifyBlockHtml(lastSplitBlockDiv, newBlock3, false);
			Assert.That(bookDiv.ChildNodes[1].InnerText, Is.EqualTo("\u00A0"));
		}

		private Block Split(BookScript book, Block blockToSplit, string verse, int pos)
		{
			var newBlock = book.SplitBlock(blockToSplit, verse, pos, true, "Jesus");
			newBlock.Delivery = "giving orders";
			newBlock.UserConfirmed = true;
			blockToSplit.UserConfirmed = true;
			Assert.That(newBlock.SplitId, Is.EqualTo(blockToSplit.SplitId));
			return newBlock;
		}

		private string Narrator(string bookId) => CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

		private void VerifyBlockHtml(XmlNode blockHtml, Block block, bool rightToLeft)
		{
			Assert.That(blockHtml.ChildNodes[0].Name, Is.EqualTo("strong"));
			Assert.That(blockHtml.ChildNodes[0].FirstChild.Value,
				Is.EqualTo($"{block.CharacterId}: "));
			var bldr = new StringBuilder();
			for (int i = 1; i < blockHtml.ChildNodes.Count; i++)
				bldr.Append(blockHtml.ChildNodes[i].OuterXml);
			Assert.That(bldr.ToString(), Is.EqualTo(
				block.GetTextAsHtml(true, rightToLeft).Replace("&#160;", "\u00A0")));
		}

		private Block NewChapterBlock(int chapterNum, string bookCode)
		{
			var block = new Block("c", chapterNum) {IsParagraphStart = true};
			block.BlockElements.Add(new ScriptText(chapterNum.ToString()));
			block.SetStandardCharacter(bookCode, CharacterVerseData.StandardCharacter.BookOrChapter);
			return block;
		}

	}
}
