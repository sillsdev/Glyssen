using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	public class QuoteParserTests
	{
		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtEnd()
		{
			var block = new Block("p", 2, 5);
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialVerseNumber);
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.IsFalse(output[1].IsNarrator);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(5, output[1].InitialVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_UnclosedQuoteAtEnd()
		{
			var block = new Block("p", 2, 5);
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialVerseNumber);
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.IsFalse(output[1].IsNarrator);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(5, output[1].InitialVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtBeginning()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.IsFalse(output[0].IsNarrator);
			Assert.AreEqual("he said.", output[1].GetText(false));
			Assert.IsTrue(output[1].IsNarrator);
		}
		[Test]
		public void Parse_OneBlockBecomesThree_TwoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»  «Make me!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("«Go!»  ", output[1].GetText(false));
			Assert.IsFalse(output[1].IsNarrator);
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
			Assert.IsFalse(output[2].IsNarrator);
		}
		[Test]
		public void Parse_OneBlockBecomesThree_QuoteInMiddle()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.IsFalse(output[1].IsNarrator);
			Assert.AreEqual("quietly.", output[2].GetText(false));
			Assert.IsTrue(output[2].IsNarrator);
		}

		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("See Spot run. "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("See Jane see Spot run."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("See Spot run. ", output[0].GetText(false));
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("See Jane see Spot run.", output[1].GetText(false));
			Assert.IsTrue(output[1].IsNarrator);
		}
		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].IsNarrator);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.IsFalse(output[1].IsNarrator);
		}

		[Test]
		public void Parse_TwoBlocksBecomeThree_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Make me!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
		}
		[Test]
		public void Parse_TwoBlocksBecomeThree_AlreadyBrokenInMiddleOfQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go ", output[1].GetText(false));
			Assert.AreEqual("west!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_ContinuingQuote_UsingStartingMarker()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Get!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»", output[2].GetText(false));
		}

		[Test, Ignore("TODO")]
		public void Parse_ContinuingQuote_UsingEndingMarker()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("»Get!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("»Get!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtEnd()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\"", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtBeginning()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("\"Go!\" he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("\"Go!\" ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test, Ignore("TODO")]
		public void Parse_StartEndSame_ThreeLevels()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"She said, 'They said, \"No way.\"'\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("\"She said, 'They said, \"No way.\"'\"", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseAtBeginning()
		{
			var block = new Block("p", 5);
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("[3]He said, «Go!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialVerseNumber);

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialVerseNumber);
		}

		[Test]
		public void Parse_MultipleVersesBeforeQuote()
		{
			var block = new Block("p", 5);
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("Matthew tried to learn to fish, but Peter was upset. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("He said, «Go back to your tax booth!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("[3]Matthew tried to learn to fish, but Peter was upset. [4]He said, «Go back to your tax booth!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialVerseNumber);

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]Matthew tried to learn to fish, but Peter was upset. [4]He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialVerseNumber);
			Assert.AreEqual("«Go back to your tax booth!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(4, output[1].InitialVerseNumber);
		}

		[Test]
		public void Parse_VerseBeforeQuote()
		{
			var block = new Block("p", 6, 2);
			block.BlockElements.Add(new ScriptText("He said, "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, [3]«Go!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(2, input[0].InitialVerseNumber);

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("[3]«Go!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseAfterQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("he said."));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("«Go!» [3]he said.", input[0].GetText(true));

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));

			Assert.AreEqual("«Go!» ", output[0].GetText(true));
			Assert.AreEqual("[3]he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseWithinQuote()
		{
			var block = new Block("p", 6, 2);
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, «Go [3]west!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(2, input[0].InitialVerseNumber);

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialVerseNumber);
			Assert.AreEqual("«Go west!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(2, output[1].InitialVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go [3]west!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_SpaceStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go!»", output[1].GetText(true));
		}
		[Test]
		public void Parse_PunctuationStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go»!! he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go»!! ", output[0].GetText(true));
			Assert.AreEqual("he said.", output[1].GetText(true));
		}
		[Test]
		public void Parse_PunctuationStaysWithPriorBlock_AtBlockEnd()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go»!", output[1].GetText(true));
		}

		[Test]
		public void Parse_UsingDifferentQuoteMarks()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("“Go!” he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("“Go!” ", output[0].GetText(true));
			Assert.AreEqual("he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_BreakOnFirstLevelQuoteOnly_HasTwoLevels()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘Get lost.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘Get lost.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_BreakOnFirstLevelQuoteOnly_HasThreeLevels()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way.”’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way.”’”", output[1].GetText(true));
		}
	}
}
