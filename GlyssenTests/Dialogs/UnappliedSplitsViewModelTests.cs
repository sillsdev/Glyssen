using System.Collections.Generic;
using System.Text;
using System.Xml;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Shared;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenTests.Dialogs
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

			var mark = new BookScript("MRK", blocks);

			var blockToSplit = mark.GetFirstBlockForVerse(8, 26);
			var newBlock = Split(mark, blockToSplit, "26", 47);

			mark.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplit, newBlock });

			var model = new UnappliedSplitsViewModel(new [] {mark.Clone(false)}, false);
			var xmlDoc = new XmlDocument();
			var html = model.GetHtml();
			xmlDoc.LoadXml(html.Replace("&nbsp;", "&#160;"));
			var body = xmlDoc.ChildNodes[0].SelectSingleNode("body");
			var bookDiv = body.FirstChild;
			Assert.AreEqual("div", bookDiv.Name);
			Assert.AreEqual("MRK", bookDiv.Attributes.GetNamedItem("id").Value);
			var splitsDiv = bookDiv.ChildNodes[0];
			Assert.AreEqual("splits", splitsDiv.Attributes.GetNamedItem("class").Value);
			Assert.AreEqual("MRK 8:25-26", splitsDiv.ChildNodes[0].Value);
			var preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplit, false);
			Assert.AreEqual(" //SPLIT// ", splitsDiv.ChildNodes[2].Value);
			var postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlock, false);
			Assert.AreEqual("\u00A0", bookDiv.ChildNodes[1].InnerText);
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

			var mark = new BookScript("MRK", blocks);

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

			var acts = new BookScript("ACT", blocks);

			var blockToSplitActs = acts.GetFirstBlockForVerse(10, 5);
			var newBlockActs = Split(acts, blockToSplitActs, "7", 21);

			acts.UnappliedBlockSplits_DoNotUse.Add(new List<Block> { blockToSplitActs, newBlockActs });

			var model = new UnappliedSplitsViewModel(new[] { mark.Clone(false), acts.Clone(false) }, false);
			var xmlDoc = new XmlDocument();
			var html = model.GetHtml();
			xmlDoc.LoadXml(html.Replace("&nbsp;", "&#160;"));
			var body = xmlDoc.ChildNodes[0].SelectSingleNode("body");
			var bookDiv = body.ChildNodes[0];
			Assert.AreEqual("div", bookDiv.Name);
			Assert.AreEqual("MRK", bookDiv.Attributes.GetNamedItem("id").Value);
			var splitsDiv = bookDiv.ChildNodes[0];
			Assert.AreEqual("splits", splitsDiv.Attributes.GetNamedItem("class").Value);
			Assert.AreEqual("MRK 8:25-26", splitsDiv.ChildNodes[0].Value);
			var preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplitMark, false);
			Assert.AreEqual(" //SPLIT// ", splitsDiv.ChildNodes[2].Value);
			var postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlockMark, false);
			Assert.AreEqual("\u00A0", bookDiv.ChildNodes[1].InnerText);

			bookDiv = body.ChildNodes[1];
			Assert.AreEqual("div", bookDiv.Name);
			Assert.AreEqual("ACT", bookDiv.Attributes.GetNamedItem("id").Value);
			splitsDiv = bookDiv.ChildNodes[0];
			Assert.AreEqual("splits", splitsDiv.Attributes.GetNamedItem("class").Value);
			Assert.AreEqual("ACT 10:5-7", splitsDiv.ChildNodes[0].Value);
			preSplitBlockDiv = splitsDiv.ChildNodes[1];
			VerifyBlockHtml(preSplitBlockDiv, blockToSplitActs, false);
			Assert.AreEqual(" //SPLIT// ", splitsDiv.ChildNodes[2].Value);
			postSplitBlockDiv = splitsDiv.ChildNodes[3];
			VerifyBlockHtml(postSplitBlockDiv, newBlockActs, false);
			Assert.AreEqual("\u00A0", bookDiv.ChildNodes[1].InnerText);
		}

		private Block Split(BookScript book, Block blockToSplit, string verse, int pos)
		{
			var newBlock = book.SplitBlock(blockToSplit, verse, pos, true, "Jesus", ScrVers.English);
			newBlock.Delivery = "giving orders";
			newBlock.UserConfirmed = true;
			blockToSplit.UserConfirmed = true;
			Assert.AreEqual(newBlock.SplitId, blockToSplit.SplitId);
			return newBlock;
		}

		private string Narrator(string bookId) => CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator);

		private void VerifyBlockHtml(XmlNode blockHtml, Block block, bool rightToLeft)
		{
			Assert.AreEqual("strong", blockHtml.ChildNodes[0].Name);
			Assert.AreEqual($"{block.CharacterId}: ", blockHtml.ChildNodes[0].FirstChild.Value);
			var bldr = new StringBuilder();
			for (int i = 1; i < blockHtml.ChildNodes.Count; i++)
				bldr.Append(blockHtml.ChildNodes[i].OuterXml);
			Assert.AreEqual(block.GetTextAsHtml(true, rightToLeft).Replace("&#160;", "\u00A0"),
				bldr.ToString());
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
