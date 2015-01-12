using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	public class QuoteParserTests
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtEnd()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He replied, ", output[0].GetText(false));
			Assert.AreEqual(7, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Isaiah was right when he prophesied about you.»", output[1].GetText(false));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual("rebuking", output[1].Delivery);
			Assert.AreEqual(7, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_UnclosedQuoteAtEnd()
		{
			var block = new Block("p", 2, 10);
			block.BlockElements.Add(new ScriptText("But the angel said to them, «Do not be afraid!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("But the angel said to them, ", output[0].GetText(false));
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(10, output[0].InitialStartVerseNumber);
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Do not be afraid!", output[1].GetText(false));
			Assert.AreEqual("angel of the LORD, an", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(10, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtBeginning()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("he said.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_OneBlockBecomesThree_TwoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»  «Make me!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!»  ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_QuoteEndsRightBeforeNewVerseStart_QuoteCharacterIdentified()
		{
			var block = new Block("p", 15, 34);
			block.BlockElements.Add(new Verse("34"));
			block.BlockElements.Add(new ScriptText("Yecu openyogi ni, “Wutye ki mugati adi?” Gugamo ni, “Abiro, ki rec mogo matitino manok.” "));
			block.BlockElements.Add(new Verse("35"));
			block.BlockElements.Add(new ScriptText("Ociko lwak ni gubed piny i ŋom, "));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", null, null, false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("Yecu openyogi ni, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Wutye ki mugati adi?” ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("Gugamo ni, ", output[2].GetText(false));
			Assert.IsTrue(output[2].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Abiro, ki rec mogo matitino manok.” ", output[3].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("Ociko lwak ni gubed piny i ŋom, ", output[4].GetText(false));
			Assert.IsTrue(output[4].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_OneBlockBecomesThree_QuoteInMiddle()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("quietly.", output[2].GetText(false));
			Assert.IsTrue(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("See Spot run. "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("See Jane see Spot run."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("See Spot run. ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("See Jane see Spot run.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}
		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_TwoBlocksBecomeThree_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Make me!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("»Get!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_NarratorAfterContinuingQuoteMarker_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[Test]
		public void Parse_QuoteAfterContinuingQuoteMarker_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_NarratorAfterContinuingQuoteMarker_ThirdLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹«Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹«Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[Test]
		public void Parse_NarratorAfterContinuingQuoteMarker_ThirdLevel_ContinuerIsOnlyInnermost()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[Test]
		public void Parse_QuoteAfterContinuingQuoteMarker_ThirdLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹«Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹«Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_QuoteAfterContinuingQuoteMarker_ThirdLevel_ContinuerIsOnlyInnermost()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtEnd()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("\"Go!\" ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteInMiddle()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\" quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\" ", output[1].GetText(false));
			Assert.AreEqual("quietly.", output[2].GetText(false));
		}

		[Test, Ignore("TODO")]
		public void Parse_StartEndSame_ThreeLevels()
		{
			var options = new QuoteSystem { StartQuoteMarker = "\"", EndQuoteMarker = "\"" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"She said, 'They said, \"No way.\"'\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
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
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);
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
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]Matthew tried to learn to fish, but Peter was upset. [4]He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go back to your tax booth!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
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
			Assert.AreEqual(2, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);

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

			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			Assert.AreEqual(2, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go west!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(2, output[1].InitialStartVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go [3]west!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_SpaceStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
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
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way.”’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_BreakOnFirstLevelQuoteOnly_HasThreeLevelsAndContinuesInside()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_BreakOnFirstLevelQuoteOnly_HasThreeLevelsAndContinuesOutside()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely,’” politely."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely,’” ", output[1].GetText(true));
			Assert.AreEqual("politely.", output[2].GetText(true));
		}

		[Test]
		public void Parse_QuoteFollowsThirdLevelQuote_BrokenCorrectly()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” quite rudely.’”"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("He continued, “The end.”"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” quite rudely.’”", output[1].GetText(true));
			Assert.AreEqual("He continued, ", output[2].GetText(true));
			Assert.AreEqual("“The end.”", output[3].GetText(true));
		}

		[Test]
		public void Parse_TitleIntrosChaptersAndExtraBiblicalMaterial_OnlyVerseTextGetsParsedForQuotes()
		{
			var options = new QuoteSystem { StartQuoteMarker = "“", EndQuoteMarker = "”" };
			var titleBlock = new Block("mt");
			titleBlock.BlockElements.Add(new ScriptText("Gospel of Mark"));
			titleBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			var introBlock1 = new Block("is");
			introBlock1.BlockElements.Add(new ScriptText("All about Mark"));
			var introBlock2 = new Block("ip");
			introBlock1.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Intro);
			introBlock2.BlockElements.Add(new ScriptText("Some people say, “Mark is way to short,” but I disagree."));
			introBlock2.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Intro);
			var chapterBlock = new Block("c");
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			chapterBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadBlock = new Block("s");
			sectionHeadBlock.BlockElements.Add(new ScriptText("John tells everyone: “The Kingdom of Heaven is at hand”"));
			sectionHeadBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var paraBlock = new Block("p");
			paraBlock.BlockElements.Add(new Verse("1"));
			paraBlock.BlockElements.Add(new ScriptText("Jesus said, “Is that John?”"));
			var input = new List<Block> { titleBlock, introBlock1, introBlock2, chapterBlock, sectionHeadBlock, paraBlock };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, options).Parse().ToList();
			Assert.AreEqual(7, output.Count);
			Assert.AreEqual("Gospel of Mark", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual("All about Mark", output[1].GetText(true));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual("Some people say, “Mark is way to short,” but I disagree.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual("Chapter 1", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual("John tells everyone: “The Kingdom of Heaven is at hand”", output[4].GetText(true));
			Assert.IsTrue(output[4].CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("[1]Jesus said, ", output[5].GetText(true));
			Assert.IsTrue(output[5].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Is that John?”", output[6].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[6].CharacterId);
		}

		[Test]
		public void Parse_IsParagraphStart()
		{
			var chapterBlock = new Block("c") { IsParagraphStart = true };
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { chapterBlock, block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("Chapter 1", output[0].GetText(true));
			Assert.AreEqual("He said, ", output[1].GetText(true));
			Assert.AreEqual("«Go»!", output[2].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.IsFalse(output[2].IsParagraphStart);
		}

		[Test]
		public void Parse_IsParagraphStart_BlockStartsWithVerse()
		{
			var block = new Block("q1") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("[23]«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
		}

		[Test]
		public void Parse_IsParagraphStart_VerseAndQuoteSpansParagraphs()
		{
			var block = new Block("q1") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block("q1") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("[23]«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.AreEqual("ŋa tsanaftá hgani ka Emanuwel,» ", output[1].GetText(true));
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[2].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.IsFalse(output[2].IsParagraphStart);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_NarratorAndOther()
		{
			var block1 = new Block("p", 17, 3, 4);
			block1.BlockElements.Add(new Verse("3-4"));
			block1.BlockElements.Add(new ScriptText("Then Peter said, "));
			var block2 = new Block("q1", 17, 3, 4);
			block2.BlockElements.Add(new ScriptText("«What verse is this?»"));
			var input = new List<Block> { block1, block2 };

			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("narrator-MAT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_DifferentCharacters_MarksBothAsAmbiguous()
		{
			var block1 = new Block("p", 6, 7, 9);
			block1.BlockElements.Add(new Verse("7-9"));
			block1.BlockElements.Add(new ScriptText("Philip said, «Surely you can't be serious.»"));
			block1.BlockElements.Add(new ScriptText("Andrew said, «I am serious.»"));
			var block2 = new Block("q1", 6, 7, 9);
			block2.BlockElements.Add(new ScriptText("«And don't call me Shirley.»"));
			var input = new List<Block> { block1, block2 };

			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("narrator-JHN", output[0].CharacterId);
			Assert.AreEqual("Ambiguous", output[1].CharacterId);
			Assert.AreEqual("narrator-JHN", output[2].CharacterId);
			Assert.AreEqual("Ambiguous", output[3].CharacterId);
			Assert.AreEqual("Ambiguous", output[4].CharacterId);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_SameCharacter_MarksBothAsCorrectCharacter()
		{
			var block1 = new Block("p", 1, 19, 20);
			block1.BlockElements.Add(new Verse("19-20"));
			block1.BlockElements.Add(new ScriptText("Peter said, «They don't call him the son of thunder for nothing.»"));
			var block2 = new Block("q1", 1, 19, 20);
			block2.BlockElements.Add(new ScriptText("«Oh, and his brother, too.»"));
			var input = new List<Block> { block1, block2 };

			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("narrator-ACT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[2].CharacterId);
		}

		[Test]
		public void Parse_QuoteSpansParagraphs()
		{
			var block1 = new Block("p", 1, 23);
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block("q1", 1, 23);
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block1, block2 };

			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("[23]«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.AreEqual("ŋa tsanaftá hgani ka Emanuwel,» ", output[1].GetText(true));
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[2].GetText(true));
		}

		[Test]
		public void Parse_AcrossChapter_FindsCorrectCharacters()
		{
			var block1 = new Block("p", 1, 31) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("31"));
			block1.BlockElements.Add(new ScriptText("Some text and «a quote» and more text."));
			var blockC = new Block("c", 2, 0) { IsParagraphStart = true };
			blockC.BlockElements.Add(new ScriptText("2"));
			var block2 = new Block("p", 2, 1) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("1"));
			block2.BlockElements.Add(new ScriptText("Text in the next chapter and «another quote»"));
			var input = new List<Block> { block1, blockC, block2 };

			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual("narrator-GEN", output[0].CharacterId);
			Assert.AreEqual("Last in Chapter", output[1].CharacterId);
			Assert.AreEqual("narrator-GEN", output[2].CharacterId);
			Assert.AreEqual("narrator-GEN", output[4].CharacterId);
			Assert.AreEqual("First in Chapter", output[5].CharacterId);
		}

		public void Parse_DialogueQuoteAtStartAndNearEnd_OneBlockBecomesTwo()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", "—", false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("—timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteUsingColonWithNoExplicitEnd_OneBlockBecomesTwo()
		{
			var block1 = new Block("p", 14, 6);
			block1.BlockElements.Add(new Verse("6"));
			block1.BlockElements.Add(new ScriptText("Jesús le dijo: Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. "));
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto."));
			var block2 = new Block("p", 14, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("Felipe le dijo: Señor, muéstranos al Padre, y nos basta."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", ":", null, false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("[6]Jesús le dijo: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);
			Assert.AreEqual("Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. [7]Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(string.Empty, output[1].Delivery);
			Assert.AreEqual(14, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("[8]Felipe le dijo: ", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[2].ChapterNumber);
			Assert.AreEqual(8, output[2].InitialStartVerseNumber);
			Assert.AreEqual("Señor, muéstranos al Padre, y nos basta.", output[3].GetText(true));
			Assert.AreEqual("Philip", output[3].CharacterId);
			Assert.AreEqual(string.Empty, output[3].Delivery);
			Assert.AreEqual(14, output[3].ChapterNumber);
			Assert.AreEqual(8, output[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteAtStartEndedByAnyPunctuation_OneBlockBecomesTwo()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", QuoteSystem.AnyPunctuation, false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram. ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteContainingRegularQuote_InnerRegularQuoteIgnored()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", "—", false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("—timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuotesInsideFirstLevelRegularQuotesNotIndicatingChangeOfSpeaker_FirstLevelQuoteBecomesSeparateBlock()
		{
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, said the frog."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", "—", false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIsUnclear());
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(1, output[0].InitialStartVerseNumber);
			Assert.AreEqual("said the frog.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(1, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithNoSentenceEndingPunctuationFollowedByCloseAfterPoetry_QuoteRemainsOpenUntilClosed()
		{
			var block1 = new Block("p", 2, 5);
			block1.BlockElements.Add(new ScriptText("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:"));
			var block2 = new Block("q2", 2, 6);
			block2.BlockElements.Add(new Verse("6"));
			block2.BlockElements.Add(new ScriptText("Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”"));
			var block3 = new Block("m", 2, 6);
			block3.BlockElements.Add(new ScriptText("Yus timiayi. Tu aarmawaitai, —tusar aimkarmiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", "—", false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:", output[0].GetText(true));
			Assert.AreEqual("Good Priest", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialStartVerseNumber);

			Assert.AreEqual("[6]Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”", output[1].GetText(true));
			Assert.AreEqual("Good Priest", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("Yus timiayi. Tu aarmawaitai, ", output[2].GetText(true));
			Assert.AreEqual("Good Priest", output[2].CharacterId);
			Assert.AreEqual(2, output[2].ChapterNumber);
			Assert.AreEqual(6, output[2].InitialStartVerseNumber);

			Assert.AreEqual("—tusar aimkarmiayi.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(2, output[3].ChapterNumber);
			Assert.AreEqual(6, output[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithNoExplicitEnd_QuoteClosedByEndOfParagraph()
		{
			var block1 = new Block("p", 1, 17);
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("Quia joˈ tso Jesús nda̱a̱na:"));
			var block2 = new Block("p", 1, 17);
			block2.BlockElements.Add(new ScriptText("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa."));
			var block3 = new Block("m", 1, 18);
			block3.BlockElements.Add(new Verse("18"));
			block3.BlockElements.Add(new ScriptText("Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem("“", "”", "—", "—", false);
			IList<Block> output = new QuoteParser(CharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("[17]Quia joˈ tso Jesús nda̱a̱na:", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);

			Assert.AreEqual("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);

			Assert.AreEqual("[18]Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[2].ChapterNumber);
			Assert.AreEqual(18, output[2].InitialStartVerseNumber);
		}
	}
}
