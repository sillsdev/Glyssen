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
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!»", output[1].GetText(false));
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
			Assert.AreEqual("he said.", output[1].GetText(false));
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
			Assert.AreEqual("«Go!»  ", output[1].GetText(false));
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
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
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.AreEqual("quietly.", output[2].GetText(false));
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
			Assert.AreEqual("See Jane see Spot run.", output[1].GetText(false));
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
			Assert.AreEqual("«Go!»", output[1].GetText(false));
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
			var options = new QuoteParserOptions { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
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
			var options = new QuoteParserOptions { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
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
			var options = new QuoteParserOptions { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
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
			var block = new Block("p");
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("[3]He said, «Go!»", input[0].GetText(true));

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseBeforeQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, [3]«Go!»", input[0].GetText(true));

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!»", output[1].GetText(false));

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("[3]«Go!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseWithinQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, «Go [3]west!»", input[0].GetText(true));

			IList<Block> output = new QuoteParser(input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go west!»", output[1].GetText(false));

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
			var options = new QuoteParserOptions { StartQuoteMarker = "“", EndQuoteMarker = "”" };
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
			var options = new QuoteParserOptions { StartQuoteMarker = "“", EndQuoteMarker = "”" };
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
			var options = new QuoteParserOptions { StartQuoteMarker = "“", EndQuoteMarker = "”" };
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
