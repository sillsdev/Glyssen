using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Quote;
using Glyssen.Shared;
using NUnit.Framework;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.WritingSystems;

namespace GlyssenTests.Quote
{
	[TestFixture]
	public class QuoteParserTests
	{
		private static readonly int kMATbookNum = BCVRef.BookToNumber("MAT");
		private static readonly int kMRKbookNum = BCVRef.BookToNumber("MRK");
		private static readonly int kLUKbookNum = BCVRef.BookToNumber("LUK");

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
		}

		[Test]
		public void Parse_ContainsUserConfirmedBlock_ThrowsInvalidOperationException()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			block.UserConfirmed = true;
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input);
			Assert.Throws<InvalidOperationException>(() => parser.Parse());
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtEnd()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
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
		public void Parse_OneBlockBecomesTwo_UnclosedQuoteAtEnd_LastBlockSetToUnknown()
		{
			var block = new Block("p", 2, 10);
			block.BlockElements.Add(new ScriptText("But the angel said to them, «Do not be afraid!"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("But the angel said to them, ", output[0].GetText(false));
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(10, output[0].InitialStartVerseNumber);
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Do not be afraid!", output[1].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(10, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtBeginning()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go ", output[1].GetText(false));
			Assert.AreEqual("west!»", output[2].GetText(false));
		}

		[TestCase("«")]
		[TestCase("»")]
		[TestCase("%")]
		public void Parse_Continuer_QuoteContinues(string firstLevelContinuer)
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "Get!»"));
			var input = new List<Block> { block, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.AreEqual(firstLevelContinuer + "Get!»", output[2].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
		}

		[TestCase("“")]
		[TestCase("”")]
		[TestCase("&")]
		public void Parse_TwoFirstLevelContinuationParagraphs_AllContinuationParasKeptTogether(string firstLevelContinuer)
		{
			var block1 = new Block("p", 6, 10);
			block1.IsParagraphStart = true;
			block1.BlockElements.Add(new Verse("10"));
			block1.BlockElements.Add(new ScriptText("Jesús e-sapinganga sogdebalid, “Ar bemar neg-gwagwense dogdapile, obunnomaloed."));
			var block2 = new Block("p", 6, 11);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("11"));
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye."));
			var block3 = new Block("p", 6, 11);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new ScriptText(firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”"));
			var block4 = new Block("p", 6, 12);
			block4.IsParagraphStart = true;
			block4.BlockElements.Add(new Verse("12"));
			block4.BlockElements.Add(new ScriptText("Jesús-sapingan nadmargu, be-daed be ogwamar. "));
			block4.BlockElements.Add(new Verse("13"));
			block4.BlockElements.Add(new ScriptText("Deginbali."));
			var input = new List<Block> { block1, block2, block3, block4 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("{10}\u00A0Jesús e-sapinganga sogdebalid, ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].IsParagraphStart);

			Assert.AreEqual("“Ar bemar neg-gwagwense dogdapile, obunnomaloed.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].IsParagraphStart);

			Assert.AreEqual("{11}\u00A0" + firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye.", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsTrue(output[2].IsParagraphStart);

			Assert.AreEqual(firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”", output[3].GetText(true));
			Assert.AreEqual("Jesus", output[3].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);
			Assert.IsTrue(output[3].IsParagraphStart);

			Assert.AreEqual("{12}\u00A0Jesús-sapingan nadmargu, be-daed be ogwamar. {13}\u00A0Deginbali.", output[4].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[4].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[4].MultiBlockQuote);
			Assert.IsTrue(output[4].IsParagraphStart);
		}


		[Test]
		public void Parse_Continuer_NarratorAfter_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!» Thus he ended."));
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("«Get!» ", output[2].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_QuoteAfter_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("«Get!»", output[2].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.IsFalse(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("she replied.", output[4].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[4].MultiBlockQuote);
			Assert.IsTrue(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_NarratorAfter_ThirdLevel()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹«Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹«Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_HasSpace_NarratorAfter()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "« ‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "« ‹ «", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, « ‹Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("« ‹Get!› »"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("Thus he ended."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("« ‹Go!", output[1].GetText(false));
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);

			Assert.AreEqual("« ‹Get!› »", output[2].GetText(false));
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);

			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		/// <summary>
		/// Not sure why this test was written this way. It possibly started out as one thing and gradually morphed.
		/// We changed the name to reflect the bad data. The continuer/opener before No! is wrong, as is the lack of
		/// a continuer on the last line. Original name suggested that it was expected to end the quote and have a
		/// narrator line following it, but the assertion did not correspond to that name.
		/// </summary>
		[TestCase("«", "«‹", "«‹«")]
		[TestCase("»", "»›", "»›»")]
		public void Parse_Continuer_SecondLevelStartsWithFirstLevelContinuerFollowedByBadData_QuoteStaysOpen(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "‹Get!"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No!»"));
			var block4 = new Block("p") { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Still in quote.›»"));
			var input = new List<Block> { block, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);

			Assert.AreEqual(firstLevelContinuer + "‹Get!", output[2].GetText(false));
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);

			Assert.AreEqual("«No!»", output[3].GetText(false));
			Assert.IsFalse(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);

			Assert.AreEqual("Still in quote.›»", output[4].GetText(false));
			Assert.IsFalse(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[4].MultiBlockQuote);
		}

		[TestCase("«", "«‹", "«‹«")]
		[TestCase("»", "»›", "»›»")]
		public void Parse_Continuer_SecondLevelStartsWithFirstLevelContinuer_NarratorAfter(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "‹Get!"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText(secondLevelContinuer + "«No!»"));
			var block4 = new Block("p") { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText(secondLevelContinuer + "Still in quote.›»"));
			var input = new List<Block> { block, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);

			Assert.AreEqual(firstLevelContinuer + "‹Get!", output[2].GetText(false));
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);

			Assert.AreEqual(secondLevelContinuer + "«No!»", output[3].GetText(false));
			Assert.IsFalse(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);

			Assert.AreEqual(secondLevelContinuer + "Still in quote.›»", output[4].GetText(false));
			Assert.IsFalse(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[4].MultiBlockQuote);
		}

		/// <summary>
		/// Not sure why this test was written this way. It possibly started out as one thing and gradually morphed.
		/// We changed the name to reflect the bad data. The continuer/opener before No! is wrong, as is the lack of
		/// a continuer on the last line. Original name suggested that it was expected to end the quote and have a
		/// narrator line following it, but the assertion did not correspond to that name.
		/// </summary>
		[TestCase("«", "« ‹", "« ‹ «")]
		[TestCase("»", "» ›", "» › »")]
		public void Parse_Continuer_SecondLevelStartsWithFirstLevelContinuerWithSpacesFollowedByBadData_QuoteStaysOpen(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + " ‹Get!"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No!»"));
			var block4 = new Block("p") { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Still in quote.›»"));
			var input = new List<Block> { block, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.IsFalse(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);

			Assert.AreEqual(firstLevelContinuer + " ‹Get!", output[2].GetText(false));
			Assert.IsFalse(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);

			Assert.AreEqual("«No!»", output[3].GetText(false));
			Assert.IsFalse(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);

			Assert.AreEqual("Still in quote.›»", output[4].GetText(false));
			Assert.IsFalse(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[4].MultiBlockQuote);
		}

		[TestCase("«", "‹", "«")]
		[TestCase("»", "›", "»")]
		public void Parse_Continuer_NarratorAfter_ThirdLevel_ContinuerIsOnlyInnermost(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(thirdLevelContinuer + "Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual(thirdLevelContinuer + "Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[TestCase("«", "«‹", "«‹«")]
		[TestCase("»", "»›", "»›»")]
		public void Parse_Continuer_QuoteAfter_ThirdLevel(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(thirdLevelContinuer + "Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual(thirdLevelContinuer + "Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[TestCase("«", "‹", "«")]
		[TestCase("»", "›", "»")]
		public void Parse_Continuer_QuoteAfter_ThirdLevel_ContinuerIsOnlyInnermost(string firstLevelContinuer, string secondLevelContinuer, string thirdLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", thirdLevelContinuer, 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(thirdLevelContinuer + "Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual(thirdLevelContinuer + "Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[TestCase("«", "‹")]
		[TestCase("»", "›")]
		public void Parse_Level3_BlockStartsWithNewThirdLevelQuote(string firstLevelContinuer, string secondLevelContinuer)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", secondLevelContinuer, 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹She said, "));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«Get!» rudely.›»"));
			var input = new List<Block> { block, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹She said, ", output[1].GetText(false));
			Assert.AreEqual("«Get!» rudely.›»", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<Go!>> he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("<<Go!>> ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>> loudly."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<Go!>> ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>>"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<Go!>>", output[1].GetText(false));
		}

		[Test]
		public void Parse_MultipleCharacters_Level1CloseStartsWithLevel2Close_Level1CloseImmediatelyFollowsLevel2Close_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<She said <Go!> and <Get!> >> and then he finished."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<She said <Go!> and <Get!> >> ", output[1].GetText(false));
			Assert.AreEqual("and then he finished.", output[2].GetText(false));
			Assert.AreEqual("narrator-MRK", output[2].CharacterId);
		}

		[Test]
		public void Parse_MultipleCharacters_Level1ContinuerStartsWithLevel2Open_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<She said <Go!> and "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("<<Continue>> "));
			var block3 = new Block("p");
			block3.BlockElements.Add(new ScriptText("Not a quote."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<She said <Go!> and ", output[1].GetText(false));
			Assert.AreEqual("<<Continue>> ", output[2].GetText(false));
			Assert.AreEqual("Not a quote.", output[3].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[3].CharacterId);
		}

		[Test]
		public void Parse_MultipleCharacters_3Levels_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<<<", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<<", ">>", "<<<<<", 3, QuotationMarkingSystemType.Normal));
			var block1 = new Block("p");
			block1.BlockElements.Add(new ScriptText("A gye 'ushu kong le, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("<<Udebid ugyang a ma de le: "));
			var block3 = new Block("q1");
			block3.BlockElements.Add(new ScriptText("<Unim a de Atyagi le: <<Be bel kwu-m "));
			var block6 = new Block("q2");
			block6.BlockElements.Add(new ScriptText("abee fe he itang.>> > "));
			var block7 = new Block("m");
			block7.BlockElements.Add(new ScriptText("Gbe Udebid or a ma ko Ukristi le Atyam, ki nya sha ná a, ufe ù ha fel igia ima?>> "));
			var block8 = new Block("p");
			block8.BlockElements.Add(new ScriptText("Undi ken or lè he."));
			var input = new List<Block> { block1, block2, block3, block6, block7, block8 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("A gye 'ushu kong le, ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[0].CharacterId);
			Assert.AreEqual("<<Udebid ugyang a ma de le: <Unim a de Atyagi le: <<Be bel kwu-m abee fe he itang.>> > ", output[1].GetText(false));
			Assert.AreNotEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[1].CharacterId);
			Assert.AreEqual("Gbe Udebid or a ma ko Ukristi le Atyam, ki nya sha ná a, ufe ù ha fel igia ima?>> ", output[2].GetText(false));
			Assert.AreNotEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[2].CharacterId);
			Assert.AreEqual("Undi ken or lè he.", output[3].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[3].CharacterId);
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("&*Go!^~ he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("&*Go!^~ ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~ loudly."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("&*Go!^~ ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("&*Go!^~", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<<Go!>>> he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("<<<Go!>>> ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>> loudly."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<<Go!>>> ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>>"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<<Go!>>>", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\""));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\"", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("\"Go!\" he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("\"Go!\" ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\" quietly."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\" ", output[1].GetText(false));
			Assert.AreEqual("quietly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_ThreeLevels()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("'", "'", "'", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("\"", "\"", "\"", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"She said, 'They said, \"No way.\"'\""));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
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
			Assert.AreEqual("{3}\u00A0He said, «Go!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{3}\u00A0He said, ", output[0].GetText(true));
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
			Assert.AreEqual("{3}\u00A0Matthew tried to learn to fish, but Peter was upset. {4}\u00A0He said, «Go back to your tax booth!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{3}\u00A0Matthew tried to learn to fish, but Peter was upset. {4}\u00A0He said, ", output[0].GetText(true));
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
			Assert.AreEqual("He said, {3}\u00A0«Go!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(2, input[0].InitialStartVerseNumber);

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("{3}\u00A0«Go!»", output[1].GetText(true));
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
			Assert.AreEqual("«Go!» {3}\u00A0he said.", input[0].GetText(true));

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));

			Assert.AreEqual("«Go!» ", output[0].GetText(true));
			Assert.AreEqual("{3}\u00A0he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseWithinQuote()
		{
			var block = new Block("p", 6, 3);
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, «Go {4}\u00A0west!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go west!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go {4}\u00A0west!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseFollowsQuoteEndMarkAndSpace_InitialStartVerseNumberCorrect()
		{
			var block = new Block("p", 1, 1) { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("abc "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("def «ghi» "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("jkl "));
			var input = new List<Block> { block };

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("abc def ", output[0].GetText(false));
			Assert.AreEqual("«ghi» ", output[1].GetText(false));
			Assert.AreEqual("jkl ", output[2].GetText(false));
			Assert.AreEqual("{1}\u00A0abc {2}\u00A0def ", output[0].GetText(true));
			Assert.AreEqual(1, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«ghi» ", output[1].GetText(true));
			Assert.AreEqual(2, output[1].InitialStartVerseNumber);
			Assert.AreEqual("{3}\u00A0jkl ", output[2].GetText(true));
			Assert.AreEqual(3, output[2].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_SpaceStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go»!", output[1].GetText(true));
		}

		/// <summary>
		/// PG-789
		/// </summary>
		[TestCase("-")]
		[TestCase("!")]
		[TestCase("'")]
		public void Parse_TrailingPunctuationFollowingSpace_TrailingPunctuationGoesWithNextBlock(string punctuation)
		{
			var block = new Block("p", 27);
			// The en-dash is supposed to be a tone-mark that applies to the word "Ma".
			block.AddVerse(23, $"Ɔ ya maa neencla le: «Nyinyia ‑anɩ saa ɔ ya 'lɛ nʋ bha?» {punctuation}Ma maa ka...");
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("{23}\u00A0Ɔ ya maa neencla le: ", output[0].GetText(true));
			Assert.AreEqual("«Nyinyia ‑anɩ saa ɔ ya 'lɛ nʋ bha?» ", output[1].GetText(true));
			Assert.AreEqual($"{punctuation}Ma maa ka...", output[2].GetText(true));
		}

		[Test]
		public void Parse_UsingDifferentQuoteMarks()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("“Go!” he said."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("“Go!” ", output[0].GetText(true));
			Assert.AreEqual("he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level2_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘Get lost.’”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘Get lost.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way.”’”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way.”’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_ContinuesInside_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely.’”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_ContinuesOutside_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely,’” politely."));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely,’” ", output[1].GetText(true));
			Assert.AreEqual("politely.", output[2].GetText(true));
		}

		/// <summary>
		/// PG-578 (Text of 1 Kings 1:11-15 from The World Bible)
		/// </summary>
		[Test]
		public void Parse_Level3_WordInSecondLevelContainsApostropheWhichIsSecondLevelCloser_ApostropheDoesNotEndSecondLevelQuote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("11"));
			block.BlockElements.Add(new ScriptText("Then Nathan spoke to Bathsheba the mother of Solomon, saying, “Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? "));
			block.BlockElements.Add(new Verse("12"));
			block.BlockElements.Add(new ScriptText("Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. "));
			block.BlockElements.Add(new Verse("13"));
			block.BlockElements.Add(new ScriptText("Go in to king David, and tell him, ‘Didn’t you, my lord, king, swear to your servant, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ "));
			block.BlockElements.Add(new Verse("14"));
			block.BlockElements.Add(new ScriptText("Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "1KI", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{11}\u00A0Then Nathan spoke to Bathsheba the mother of Solomon, saying, ", output[0].GetText(true));
			Assert.AreEqual("“Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? " +
							"{12}\u00A0Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. " +
							"{13}\u00A0Go in to king David, and tell him, ‘Didn’t you, my lord, king, swear to your servant, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ " +
							"{14}\u00A0Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”", output[1].GetText(true));
		}

		/// <summary>
		/// PG-578
		/// </summary>
		[Test]
		public void Parse_Level3_WordInSecondLevelContainsPluralPossessiveApostropheWhichIsSecondLevelCloser_ApostropheDoesNotEndSecondLevelQuote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("11"));
			block.BlockElements.Add(new ScriptText("Then Nathan spoke to Bathsheba the mother of Solomon, saying, “Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? "));
			block.BlockElements.Add(new Verse("12"));
			block.BlockElements.Add(new ScriptText("Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. "));
			block.BlockElements.Add(new Verse("13"));
			block.BlockElements.Add(new ScriptText("Go in to king David, and tell him, ‘Did you not swear in your kids’ hearing, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ "));
			block.BlockElements.Add(new Verse("14"));
			block.BlockElements.Add(new ScriptText("Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "1KI", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{11}\u00A0Then Nathan spoke to Bathsheba the mother of Solomon, saying, ", output[0].GetText(true));
			Assert.AreEqual("“Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? " +
							"{12}\u00A0Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. " +
							"{13}\u00A0Go in to king David, and tell him, ‘Did you not swear in your kids’ hearing, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ " +
							"{14}\u00A0Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”", output[1].GetText(true));
		}


		/// <summary>
		/// PG-751
		/// </summary>
		[Test]
		public void Parse_ParagraphContainsFirstLevelQuoteWithNestedSecondLevelQuoteFolloweByAnotherFirstLevelQuote_SecondLevelCloserNotTreatedAsAnApostrophe()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p", 13) { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("14"));
			block.BlockElements.Add(new ScriptText("“Oona gwine see ‘De Horrible Bad Ting wa mek God place empty’ da stanop een de place weh e ain oughta dey.” (Leh oona wa da read ondastan wa dis mean.) “Wen dat time come, de people een Judea mus ron way quick ta de hill country.”"));
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("{14}\u00A0“Oona gwine see ‘De Horrible Bad Ting wa mek God place empty’ da stanop een de place weh e ain oughta dey.” ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual("(Leh oona wa da read ondastan wa dis mean.) ", output[1].GetText(true));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Wen dat time come, de people een Judea mus ron way quick ta de hill country.”", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
		}

		[Test]
		public void Parse_Level3_Level1QuoteFollows_BrokenCorrectly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” quite rudely.’”"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("He continued, “The end.”"));
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” quite rudely.’”", output[1].GetText(true));
			Assert.AreEqual("He continued, ", output[2].GetText(true));
			Assert.AreEqual("“The end.”", output[3].GetText(true));
		}

		/// <summary>
		/// PG-602 (Text based on Jeremiah 27:1-12 from The World Bible)
		/// </summary>
		[Ignore("It is a known limitation of the quote parser/settings that it can't handle quotes nested 5 deep.")]
		[Test]
		public void Parse_Level4ContinuerWithLevel5QuoteNestedInsideParagraph_BrokenCorrectly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“‘“", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p", 27, 1) { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("In the beginning, this word came from Yahweh, saying, "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Yahweh says to me: “Make bonds and bars, and put them on your neck. "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("Then send them to the kings, by the hand of the messengers who come to Jerusalem. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Give them a command, saying, ‘Yahweh of Armies says, “You shall tell your masters: "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("‘I have made the earth, by my great power. I give it to whom it seems right to me. "));
			block.BlockElements.Add(new Verse("6"));
			block.BlockElements.Add(new ScriptText("Now I have given all these lands to Nebuchadnezzar, my servant. I have also given the animals to him. "));
			block.BlockElements.Add(new Verse("7"));
			block.BlockElements.Add(new ScriptText("All the nations will serve him. Then many nations will make him their servant."));
			var block2 = new Block("p", 27, 8) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("“‘“‘I will punish the nation which will not serve Nebuchadnezzar,’ says Yahweh, ‘until I have consumed them. "));
			block2.BlockElements.Add(new Verse("9"));
			block2.BlockElements.Add(new ScriptText("Don’t listen to your prophets, who speak, saying, “You shall not serve the king of Babylon;” ")); // Level 5!!!
			block2.BlockElements.Add(new Verse("10"));
			block2.BlockElements.Add(new ScriptText("for they prophesy a lie to you, so that I would drive you out. "));
			block2.BlockElements.Add(new Verse("11"));
			block2.BlockElements.Add(new ScriptText("But the nation under Babylon will remain in their land,’ says Yahweh; ‘and they will dwell in it.’”’”"));
			var block3 = new Block("p", 27, 12) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("12"));
			block3.BlockElements.Add(new ScriptText("I spoke to Zedekiah all these words, saying, “Bring your necks under the yoke of Babylon, and live.”"));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JER", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("{1}\u00A0In the beginning, this word came from Yahweh, saying, " +
							"{2}\u00A0Yahweh says to me: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JER", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Make bonds and bars, and put them on your neck. " +
							"{3}\u00A0Then send them to the kings, by the hand of the messengers who come to Jerusalem. " +
							"{4}\u00A0Give them a command, saying, ‘Yahweh of Armies says, “You shall tell your masters: " +
							"{5}\u00A0‘I have made the earth, by my great power. I give it to whom it seems right to me. " +
							"{6}\u00A0Now I have given all these lands to Nebuchadnezzar, my servant. I have also given the animals to him. " +
							"{7}\u00A0All the nations will serve him. Then many nations will make him their servant.", output[1].GetText(true));
			Assert.AreEqual("God", output[1].CharacterId);
			Assert.AreEqual("{8}\u00A0“‘“‘I will punish the nation which will not serve Nebuchadnezzar,’ says Yahweh, ‘until I have consumed them. " +
							"{9}\u00A0Don’t listen to your prophets, who speak, saying, “You shall not serve the king of Babylon;” " +
							"{10}\u00A0for they prophesy a lie to you, so that I would drive you out. " +
							"{11}\u00A0But the nation under Babylon will remain in their land,’ says Yahweh; ‘and they will dwell in it.’”’”", output[2].GetText(true));
			Assert.AreEqual("Jeremiah", output[2].CharacterId);
			Assert.AreEqual("{12}\u00A0I spoke to Zedekiah all these words, saying, ", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JER", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Bring your necks under the yoke of Babylon, and live.”", output[4].GetText(true));
			Assert.AreEqual("Jeremiah", output[4].CharacterId);
		}

		[Test]
		public void Parse_TitleIntrosChaptersAndExtraBiblicalMaterial_OnlyVerseTextGetsParsedForQuotes()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
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
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
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
			Assert.AreEqual("{1}\u00A0Jesus said, ", output[5].GetText(true));
			Assert.IsTrue(output[5].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Is that John?”", output[6].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[6].CharacterId);
		}

		[Test]
		public void Parse_IsParagraphStart()
		{
			var chapterBlock = new Block("c") { IsParagraphStart = true };
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { chapterBlock, block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
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
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
		}

		[Test]
		public void Parse_VerseAndQuoteSpansPoetryParagraphs_PoetryParagraphsCombinedAndIsParagraphStartSetCorrectly()
		{
			var block = new Block("q1") { IsParagraphStart = true, ChapterNumber = 1 };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block("q1") { IsParagraphStart = true, ChapterNumber = 1, InitialStartVerseNumber = 23};
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun, ŋa tsanaftá hgani ka Emanuwel,» ", output[0].GetText(true));
			Assert.AreEqual("q1", output[0].StyleTag);
			Assert.IsTrue(output[0].IsParagraphStart);
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[1].GetText(true));
			Assert.IsFalse(output[1].IsParagraphStart);
			Assert.AreEqual("q1", output[1].StyleTag);
		}

		/// <summary>
		/// PG-603: Quote parser messes up if multi-block quote is introduced with a colon
		/// </summary>
		[TestCase("«", "»")]
		[TestCase("“", "”")]
		[TestCase("”", "”")]
		[TestCase("“", "“")]
		public void Parse_QuoteIntroducedWithColonSpansMultiplePoetryParagraphs_EntireQuoteFound(string openingQuoteMark, string closingQuoteMark)
		{
			// Based on Kuna San Blas (Gen 1:14-15)
			// ReSharper disable once RedundantArgumentDefaultValue
			var quoteSystem = new QuoteSystem(new QuotationMark(openingQuoteMark, closingQuoteMark, openingQuoteMark, 1, QuotationMarkingSystemType.Normal), ":", null);

			var block1 = new Block("p", 1, 14) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("14"));
			block1.BlockElements.Add(new ScriptText("Geb degi, Bab-Dummad sogdebalid:" + openingQuoteMark + "Nibneggi gwallumar nagu,"));
//			var block2 = new Block("q1", 1, 14) { IsParagraphStart = true };
//			block2.BlockElements.Add(new ScriptText(openingQuoteMark + "Nibneggi gwallumar nagu,"));
			var block3 = new Block("q2", 1, 14) { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("adi, neg-mutikid, neg-ibgined-ebo bachikii guegar."));
			var block4 = new Block("q1", 1, 14) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Adi, ibagan-nagumaid, yolamar-nagumaid, birgamar-nagumaid magar daklegegar."));
			var block5 = new Block("q2", 1, 15) { IsParagraphStart = true };
			block5.BlockElements.Add(new Verse("15"));
			block5.BlockElements.Add(new ScriptText("Degi, gwallumar-niba-naid napneg-mee saegar." + closingQuoteMark));
			var block6 = new Block("q2", 1, 15) { IsParagraphStart = true };
			block6.BlockElements.Add(new ScriptText("Deyob gunonikid."));
			var input = new List<Block> { block1, /*block2,*/ block3, block4, block5, block6 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			int i = 0;
			Assert.AreEqual("{14}\u00A0Geb degi, Bab-Dummad sogdebalid:", output[i].GetText(true));
			Assert.AreEqual("narrator-GEN", output[i].CharacterId);
			Assert.AreEqual(openingQuoteMark + "Nibneggi gwallumar nagu, adi, neg-mutikid, neg-ibgined-ebo bachikii guegar.", output[++i].GetText(true));
			Assert.AreEqual("God", output[i].CharacterId);
			Assert.AreEqual("p", output[i].StyleTag);
			Assert.AreEqual("Adi, ibagan-nagumaid, yolamar-nagumaid, birgamar-nagumaid magar daklegegar.", output[++i].GetText(true));
			Assert.AreEqual("God", output[i].CharacterId);
			Assert.AreEqual("{15}\u00A0Degi, gwallumar-niba-naid napneg-mee saegar." + closingQuoteMark, output[++i].GetText(true));
			Assert.AreEqual("God", output[i].CharacterId);
			Assert.AreEqual("Deyob gunonikid.", output[++i].GetText(true));
			Assert.AreEqual("narrator-GEN", output[i].CharacterId);
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

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("narrator-MAT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
		}

		[Test]
		public void Parse_BackToNarratorImmediatelyAfterVerseChangeWithVerseBridge_InitialStartAndEndVersesAreCorrect()
		{
			var block1 = new Block("p", 17, 2);
			block1.BlockElements.Add(new Verse("2"));
			block1.BlockElements.Add(new ScriptText("Then Peter said, "));
			block1.BlockElements.Add(new Verse("3-4"));
			block1.BlockElements.Add(new ScriptText("«What verse is this?»"));
			block1.BlockElements.Add(new Verse("5-6"));
			block1.BlockElements.Add(new ScriptText("Then the narrator said something."));
			var input = new List<Block> { block1 };

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("narrator-MAT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);
			Assert.AreEqual(4, output[1].InitialEndVerseNumber);
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(5, output[2].InitialStartVerseNumber);
			Assert.AreEqual(6, output[2].InitialEndVerseNumber);
		}

		[Test]
		public void Parse_Parse_BackToNarratorImmediatelyAfterVerseChangeWithoutVerseBridge_InitialStartAndEndVersesAreCorrect()
		{
			var block1 = new Block("p", 17, 2);
			block1.BlockElements.Add(new Verse("2"));
			block1.BlockElements.Add(new ScriptText("Then Peter said, "));
			block1.BlockElements.Add(new Verse("3-4"));
			block1.BlockElements.Add(new ScriptText("«What verse is this?»"));
			block1.BlockElements.Add(new Verse("5"));
			block1.BlockElements.Add(new ScriptText("Then the narrator said something."));
			var input = new List<Block> { block1 };

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("narrator-MAT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);
			Assert.AreEqual(4, output[1].InitialEndVerseNumber);
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(5, output[2].InitialStartVerseNumber);
			Assert.AreEqual(0, output[2].InitialEndVerseNumber);
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

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
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

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("narrator-ACT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[2].CharacterId);
		}

		[TestCase("q1")]
		[TestCase("pi")]
		[TestCase("pi2")]
		public void Parse_QuoteSpansPoetryParagraphs(string poetryStyleTag)
		{
			var block1 = new Block("p", 1, 23);
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block(poetryStyleTag, 1, 23);
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block1, block2 };

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun, ŋa tsanaftá hgani ka Emanuwel,» ", output[0].GetText(true));
			Assert.AreEqual("p", output[0].StyleTag);
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[1].GetText(true));
		}

		[Test]
		public void Parse_AcrossChapter_FindsCorrectCharacters()
		{
			var block1 = new Block("p", 1, 31) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("31"));
			block1.BlockElements.Add(new ScriptText("Some text and «a quote» and more text."));
			// ReSharper disable once RedundantArgumentDefaultValue
			var blockC = new Block("c", 2, 0) { IsParagraphStart = true };
			blockC.BlockElements.Add(new ScriptText("2"));
			var block2 = new Block("p", 2, 1) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("1"));
			block2.BlockElements.Add(new ScriptText("Text in the next chapter and «another quote»"));
			var input = new List<Block> { block1, blockC, block2 };

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual("narrator-GEN", output[0].CharacterId);
			Assert.AreEqual("Last in Chapter", output[1].CharacterId);
			Assert.AreEqual("narrator-GEN", output[2].CharacterId);
			Assert.AreEqual("narrator-GEN", output[4].CharacterId);
			Assert.AreEqual("First in Chapter", output[5].CharacterId);
		}

		[Test]
		public void Parse_SpaceAfterVerse_NoEmptyBlock()
		{
			var block1 = new Block("p", 3, 12) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«pe, kadi ki acel.» "));
			block1.BlockElements.Add(new Verse("13"));
			block1.BlockElements.Add(new ScriptText(" «Guŋamo doggi calo lyel ma twolo,»"));
			var input = new List<Block> { block1 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«pe, kadi ki acel.» ", output[0].GetText(false));
			Assert.AreEqual("«Guŋamo doggi calo lyel ma twolo,»", output[1].GetText(false));
			Assert.AreEqual("«pe, kadi ki acel.» ", output[0].GetText(true));
			Assert.AreEqual("{13}\u00A0«Guŋamo doggi calo lyel ma twolo,»", output[1].GetText(true));
		}

		[TestCase("(")]
		[TestCase("[")]
		[TestCase("{")]
		[TestCase("⦅")]
		[TestCase("¡")]
		[TestCase("¿")]
		[TestCase("[(")]
		[TestCase("¿¡")]
		public void Parse_OpeningPunctuationBeforeLevel2Continuer_NarratorAfter(string openingPunctuation)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "« ‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "« ‹ «", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, « ‹Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText(openingPunctuation + "« ‹Get!› »)"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("Thus he ended."));
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("« ‹Go!", output[1].GetText(false));
			Assert.AreEqual(openingPunctuation + "« ‹Get!› »)", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[TestCase("(")]
		[TestCase("[")]
		[TestCase("{")]
		[TestCase("⦅")]
		[TestCase("¡")]
		[TestCase("¿")]
		[TestCase("[(")]
		[TestCase("¿¡")]
		public void Parse_OpeningPunctuationAfterQuote_OpeningPunctuationGoesWithFollowingBlock(string openingPunctuation)
		{
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("“Na njə́a mənə, wuntə digəlyi dzəgə kə́lə hwi, a njə dzəgə ye zəgwi rə kə za, a mbəlyi dzəgə ka zəgwi tsa Immanuʼel.” " + openingPunctuation + "“Immanuʼel” tsa ná, njə́ nee, “tá myi Hyalatəmwə,” əkwə.)"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("“Na njə́a mənə, wuntə digəlyi dzəgə kə́lə hwi, a njə dzəgə ye zəgwi rə kə za, a mbəlyi dzəgə ka zəgwi tsa Immanuʼel.” ", output[0].GetText(true));
			Assert.AreEqual(openingPunctuation + "“Immanuʼel” ", output[1].GetText(true));
			Assert.AreEqual("tsa ná, njə́ nee, ", output[2].GetText(true));
			Assert.AreEqual("“tá myi Hyalatəmwə,” ", output[3].GetText(true));
			Assert.AreEqual("əkwə.)", output[4].GetText(true));
		}

		[Test]
		public void Parse_OpeningSquareBracketBeforeVerseThatStartsQuote_OpeningPunctuationGoesWithFollowingBlock()
		{
			var input = new List<Block>
			{
				new Block("p", 8, 36) { IsParagraphStart = true }.AddVerse(36, "As they traveled along the road, they came to some " +
				"water and the eunuch said, “Look, here is water. Why shouldn't I be baptized?” [").AddVerse(37, "“If you believe with all " +
				"your heart, you may,” replied Phillip. And he answered and said, “I believe that Jesus Christ is the Son of God.”] ").AddVerse(
				38, "He ordered the carriage to stop, and they went down into the water, and Philip baptized him.")
			};
			QuoteParser.SetQuoteSystem(QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal),
				null, null));
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual("“Look, here is water. Why shouldn't I be baptized?” ", output[1].GetText(true));
			Assert.AreEqual(36, output[1].InitialStartVerseNumber);
			Assert.IsFalse(output[1].StartsAtVerseStart);
			Assert.AreEqual("[{37}\u00A0“If you believe with all your heart, you may,” ", output[2].GetText(true));
			Assert.AreEqual(37, output[2].InitialStartVerseNumber);
			Assert.IsTrue(output[2].StartsAtVerseStart);
			Assert.IsFalse(output[2].IsParagraphStart);
			Assert.AreEqual("replied Phillip. And he answered and said, ", output[3].GetText(true));
			Assert.AreEqual(37, output[3].InitialStartVerseNumber);
			Assert.IsFalse(output[3].StartsAtVerseStart);
			Assert.AreEqual("“I believe that Jesus Christ is the Son of God.”] ", output[4].GetText(true));
			Assert.AreEqual(37, output[4].InitialStartVerseNumber);
			Assert.IsFalse(output[4].StartsAtVerseStart);
		}

		[Test]
		public void Parse_OpeningSquareBracketWhenPrecedingVerseEndsQuote_OpeningPunctuationGoesWithFollowingBlock()
		{
			var input = new List<Block>
			{
				new Block("p", 8, 36) { IsParagraphStart = true }.AddVerse(36, "As they traveled along the road, they came to some " +
				"water and the eunuch said, “Look, here is water. Why shouldn't I be baptized?” [").AddVerse(37, "Phillip replied, “If you believe with all " +
				"your heart, you may.” The eunuch answered, “I believe that Jesus Christ is the Son of God.”] ").AddVerse(
				38, "He ordered the carriage to stop, and they went down into the water, and Philip baptized him.")
			};
			QuoteParser.SetQuoteSystem(QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal),
				null, null));
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(7, output.Count);
			Assert.AreEqual("“Look, here is water. Why shouldn't I be baptized?” ", output[1].GetText(true));
			Assert.AreEqual(36, output[1].InitialStartVerseNumber);
			Assert.IsFalse(output[1].StartsAtVerseStart);
			Assert.AreEqual("[{37}\u00A0Phillip replied, ", output[2].GetText(true));
			Assert.AreEqual(37, output[2].InitialStartVerseNumber);
			Assert.IsTrue(output[2].StartsAtVerseStart);
			Assert.IsFalse(output[2].IsParagraphStart);
			Assert.AreEqual("“If you believe with all your heart, you may.” ", output[3].GetText(true));
			Assert.AreEqual(37, output[3].InitialStartVerseNumber);
			Assert.IsFalse(output[3].StartsAtVerseStart);
			Assert.IsFalse(output[3].IsParagraphStart);
			Assert.AreEqual("The eunuch answered, ", output[4].GetText(true));
			Assert.AreEqual(37, output[4].InitialStartVerseNumber);
			Assert.IsFalse(output[4].StartsAtVerseStart);
			Assert.AreEqual("“I believe that Jesus Christ is the Son of God.”] ", output[5].GetText(true));
			Assert.AreEqual(37, output[5].InitialStartVerseNumber);
			Assert.IsFalse(output[5].StartsAtVerseStart);
		}

		[TestCase("(")]
		[TestCase("[")]
		[TestCase("{")]
		[TestCase("⦅")]
		[TestCase("¡")]
		[TestCase("¿")]
		[TestCase("[(")]
		[TestCase("¿¡")]
		public void Parse_QuoteStartsWithLeadingPunctation_LeadingPunctationIncludedInQuote(string openingPunctuation)
		{
			// PG-644 (Kaqchikel - cak)
			var block1 = new Block("p", 1, 25) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("Pero ri Jesús xuchꞌolij ri itziel espíritu: " + openingPunctuation + "Man chic cachꞌoꞌ y catiel-el riqꞌuin ri ache!"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("Pero ri Jesús xuchꞌolij ri itziel espíritu: ", output[0].GetText(true));
			Assert.AreEqual(openingPunctuation + "Man chic cachꞌoꞌ y catiel-el riqꞌuin ri ache!", output[1].GetText(true));
		}

		[Test]
		public void Parse_PeriodFollowingClosingQuoteInLastBlock_PeriodGoesWithQuote()
		{
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("“Na njə́a mənə, wuntə digəlyi dzəgə”."));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("“Na njə́a mənə, wuntə digəlyi dzəgə”.", output[0].GetText(true));
		}

		[Test]
		public void Parse_AdjacentQuotesInPoetryParagraphs_SeparateQuotesDoNotGetCombined()
		{
			// This is totally hacked data and is not a very likely scenario
			var block1 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Nen, nyako mo ma peya oŋeyo "));
			var block2 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("laco biyac, "));
			var block3 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("binywalo latin ma laco» "));
			var block4 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("«Gibicako nyiŋe Emmanuel»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual("q1", output[0].StyleTag);
			Assert.AreEqual("{23}\u00A0«Nen, nyako mo ma peya oŋeyo laco biyac, binywalo latin ma laco» ", output[0].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual("q1", output[1].StyleTag);
			Assert.AreEqual("«Gibicako nyiŋe Emmanuel»", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_BlocksDoNotGetCombined()
		{
			// This is totally hacked data and is not a very likely scenario
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Nen, nyako mo ma peya oŋeyo "));
			var block2 = new Block("p", 1, 23) { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("laco biyac, "));
			var block3 = new Block("p", 1, 23) { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("binywalo latin ma laco» "));
			var block4 = new Block("pi", 1, 23) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("«Gibicako nyiŋe Emmanuel»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_MultiBlockQuoteAcrossSectionHead_ClearMultiBlockBeforeSectionHeaderAndResetAfter()
		{
			var block1 = new Block("p", 5, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«Wun bene wubed "));
			var block2 = new Block("s1", 5, 16) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			block2.BlockElements.Add(new ScriptText("Lok ma Yecu opwonyo i kom cik"));
			var block3 = new Block("p", 5, 17) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("17"));
			block3.BlockElements.Add(new ScriptText("«Pe wutam ni an. "));
			var block4 = new Block("q1", 5, 17) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Ada awaco botwu ni.»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("Jesus", output[3].CharacterId);
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_MultiBlockQuoteAcrossSectionHeadWithoutContinuer_ClearMultiBlockBeforeSectionHeaderAndResetAfter()
		{
			var block1 = new Block("p", 5, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«Wun bene wubed "));
			var block2 = new Block("s1", 5, 16) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			block2.BlockElements.Add(new ScriptText("Lok ma Yecu opwonyo i kom cik"));
			var block3 = new Block("p", 5, 17) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("17"));
			block3.BlockElements.Add(new ScriptText("Pe wutam ni an. "));
			var block4 = new Block("q1", 5, 17) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Ada awaco botwu ni»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("Jesus", output[3].CharacterId);
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteAtStartAndNearEnd_OneBlockBecomesTwo(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
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
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("Felipe le dijo: Señor, muéstranos al Padre, y nos basta."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("{6}\u00A0Jesús le dijo: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);

			Assert.AreEqual("Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. {7}\u00A0Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(null, output[1].Delivery);
			Assert.AreEqual(14, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("{8}\u00A0Felipe le dijo: ", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[2].ChapterNumber);
			Assert.AreEqual(8, output[2].InitialStartVerseNumber);

			Assert.AreEqual("Señor, muéstranos al Padre, y nos basta.", output[3].GetText(true));
			Assert.AreEqual("Philip", output[3].CharacterId);
			Assert.AreEqual(null, output[3].Delivery);
			Assert.AreEqual(14, output[3].ChapterNumber);
			Assert.AreEqual(8, output[3].InitialStartVerseNumber);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[TestCase(".")]
		[TestCase("?")]
		[TestCase("!")]
		[TestCase("\uFF61")] // HALFWIDTH IDEOGRAPHIC FULL STOP (I wanted to test at least one of the many non-Roman sentence-ending characters)
		public void Parse_DialogueQuoteUsingColonEndingWithSentenceEndingPunctuation_OneBlockBecomesThree(string endingPunctuation)
		{
			string quoteByJesusInV6 = String.Format("Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí{0} ", endingPunctuation);
			var block1 = new Block("p", 14, 6);
			block1.BlockElements.Add(new Verse("6"));
			block1.BlockElements.Add(new ScriptText("Jesús le dijo: " + quoteByJesusInV6));
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto."));
			var block2 = new Block("p", 14, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("Felipe le dijo: Señor, muéstranos al Padre, y nos basta."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("{6}\u00A0Jesús le dijo: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);

			Assert.AreEqual(quoteByJesusInV6, output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(string.Empty, output[1].Delivery);
			Assert.AreEqual(14, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("{7}\u00A0Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[2].ChapterNumber);
			Assert.AreEqual(7, output[2].InitialStartVerseNumber);

			Assert.AreEqual("{8}\u00A0Felipe le dijo: ", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[3].ChapterNumber);
			Assert.AreEqual(8, output[3].InitialStartVerseNumber);

			Assert.AreEqual("Señor, muéstranos al Padre, y nos basta.", output[4].GetText(true));
			Assert.AreEqual("Philip", output[4].CharacterId);
			Assert.AreEqual(string.Empty, output[4].Delivery);
			Assert.AreEqual(14, output[4].ChapterNumber);
			Assert.AreEqual(8, output[4].InitialStartVerseNumber);
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		/// <summary>
		/// VERSION 1
		/// This test assumes we can safely ignore the nested regular quotes in the dialogue quote,
		/// but we have decided that it is fairly uncommon to do this and may be an actual error in the data.
		/// The following test (currently ignored) reflects a different approach. We need to decide which
		/// approach is correct.
		/// </summary>
		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteContainingRegularQuote_InnerRegularQuoteIgnored(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);

			Assert.AreEqual("—timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		/// <summary>
		/// VERSION 2
		/// Rather than assume we can safely ignore the nested regular quotes in the dialogue quote,
		/// we have decided that it is fairly uncommon to do this and may be an actual error in the data,
		/// so it is better to mark the entire thing as unknown and force the user to look at it and split it
		/// up manualy if needed.
		/// </summary>
		[Ignore("Need to decide how to deal with an opening quotation mark in an dialogue quote")]
		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteContainingRegularQuote_EntireParagraphKeptAsSingleUnknownBlock(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);

			Assert.AreEqual("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi.", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuotesInsideFirstLevelRegularQuotesNotIndicatingChangeOfSpeaker_FirstLevelQuoteBecomesSeparateBlock(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, said the frog."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
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

		[Ignore("Need to decide how to deal with an opening quotation mark in an dialogue quote")]
		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithNoSentenceEndingPunctuationFollowedByCloseAfterPoetry_QuoteRemainsOpenUntilClosed(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 2, 5);
			block1.BlockElements.Add(new ScriptText("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:"));
			var block2 = new Block("q2", 2, 6);
			block2.BlockElements.Add(new Verse("6"));
			block2.BlockElements.Add(new ScriptText("Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”"));
			var block3 = new Block("m", 2, 6);
			block3.BlockElements.Add(new ScriptText("Yus timiayi. Tu aarmawaitai, —tusar aimkarmiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:", output[0].GetText(true));
			Assert.AreEqual("Good Priest", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialStartVerseNumber);

			Assert.AreEqual("{6}\u00A0Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("Yus timiayi. Tu aarmawaitai, —tusar aimkarmiayi.", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[2].CharacterId);
			Assert.AreEqual(2, output[2].ChapterNumber);
			Assert.AreEqual(6, output[2].InitialStartVerseNumber);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithNoSentenceEndingPunctuationFollowedByCloseAfterPoetry_SentenceEndingWithinPoetry_QuoteRemainsOpenUntilClosed(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 2, 5);
			block1.BlockElements.Add(new ScriptText("—Quote:"));
			var block2 = new Block("q2", 2, 6);
			block2.BlockElements.Add(new Verse("6"));
			block2.BlockElements.Add(new ScriptText("Poetry stuff. "));
			var block3 = new Block("m", 2, 6);
			block3.BlockElements.Add(new ScriptText("More poetry stuff —back to narrator."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Quote:", output[0].GetText(true));
			Assert.AreEqual("Good Priest", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialStartVerseNumber);

			Assert.AreEqual("{6}\u00A0Poetry stuff. ", output[1].GetText(true));
			Assert.AreEqual("Good Priest", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("More poetry stuff ", output[2].GetText(true));
			Assert.AreEqual("Good Priest", output[2].CharacterId);
			Assert.AreEqual(2, output[2].ChapterNumber);
			Assert.AreEqual(6, output[2].InitialStartVerseNumber);

			Assert.AreEqual("—back to narrator.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(2, output[3].ChapterNumber);
			Assert.AreEqual(6, output[3].InitialStartVerseNumber);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithNoExplicitEnd_QuoteClosedByEndOfParagraph(bool includeSecondNormalLevel)
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
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("{17}\u00A0Quia joˈ tso Jesús nda̱a̱na:", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);

			Assert.AreEqual("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);

			Assert.AreEqual("{18}\u00A0Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[2].ChapterNumber);
			Assert.AreEqual(18, output[2].InitialStartVerseNumber);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[TestCase(" ")]
		[TestCase(")")]
		[TestCase(") ")]
		[TestCase(" )")]
		[TestCase("”")]
		public void Parse_DialogueQuoteWithExplicitEnd_FollowingNonOpeningPunctuationIncludedInQuote(string followingPunctuation)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram." + followingPunctuation));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—Nintimrataram splintaram." + followingPunctuation, output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogQuoteEndingWithSpuriousClosingQuotationMark_TrailingPunctuationKeptWithQuote(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram”"));
			var input = new List<Block> { block1 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—Nintimrataram splintaram”", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Ignore("Need to decide how to deal with an opening quotation mark in an dialogue quote")]
		[TestCase("“", true)]
		[TestCase("‘", true)]
		[TestCase("“", false)]
		[TestCase("‘", false)]
		public void Parse_DialogQuoteEndingWithSpuriousOpeningQuotationMark_TrailingPunctuationKeptWithQuoteAndMarkedAsUnknown(string spuriousOpener, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram" + spuriousOpener));
			var input = new List<Block> { block1 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—Nintimrataram splintaram" + spuriousOpener, output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
		}

		[TestCase("—“", true)]
		[TestCase("—”", true)]
		[TestCase("—", true)]
		[TestCase("— ", true)]
		[TestCase("—“", false)]
		[TestCase("—”", false)]
		[TestCase("—", false)]
		[TestCase("— ", false)]
		public void Parse_DialogQuoteEndingWithSpuriousQuotationPunctuationFollowingCloserAndNoFollowingText_TrailingPunctuationIncludedInQuoteBlock(string trailingPunctuation, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram " + trailingPunctuation));
			var input = new List<Block> { block1 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—Nintimrataram splintaram " + trailingPunctuation, output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[TestCase("—”", true)]
		[TestCase("—", true)]
		[TestCase("— ", true)]
		[TestCase("—”", false)]
		[TestCase("—", false)]
		[TestCase("— ", false)]
		public void Parse_DialogQuoteEndingWithSpuriousQuotationPunctuationFollowingCloserAndNoFollowingTextInParagraph_TrailingPunctuationIncludedInQuoteBlock(string trailingPunctuation, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram " + trailingPunctuation));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new ScriptText("Some more text"));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—Nintimrataram splintaram " + trailingPunctuation, output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Some more text", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		#region Tests for PG-417
#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[TestCase(QuoteUtils.kSentenceEndingPunctuation, true)]
		[TestCase(QuoteUtils.kSentenceEndingPunctuation, false)]
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[TestCase(null, true)]
		[TestCase(null, false)]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuoteWithExplicitEnd_QuoteIdentifiedCorrectly(string dialogQuoteEnd, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —“Nintimrataram splintaram.”"));
			var input = new List<Block> { block1, };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", dialogQuoteEnd);
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram splintaram.”", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		// Version #1 (see below)
		[Ignore("Need to decide how to deal with an opening quotation mark in an dialogue quote")]
		[Test]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuote_DialogueBlockGoesToEndOfParaButMarkedAsUnknown()
		{
			// TODO: Need to decide whether to use this version of the test or one of the following versions.

			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —“Nintimrataram” is my favorite word. Do you like it?"));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” is my favorite word.", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);
		}

		[Ignore("This is alternate version #2 of the above test, depending on what we decide is the desired behavior.")]
		[Test]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuote_TreatAsRegularFirstLevelQuoteWithDashIncluded()
		{
			// TODO: Need to decide whether to use this version of the test, version #1 (above), or version #3 (below).

			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —“Nintimrataram” is my favorite word. Do you like it?"));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("is my favorite word. Do you like it?", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Ignore("This is alternate version #3 of the above tests, depending on what we decide is the desired behavior.")]
		[Test]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuote_IgnoreFirstLevelQuoteAndEndAtSentenceEndingPunctuation()
		{
			// TODO: Need to decide whether to use this version of the test or one of the above versions.

			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said —“Nintimrataram” is my favorite word. Do you like it?"));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” is my favorite word. ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Do you like it?", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Ignore("See above 3 tests. This will be similar.")]
		[Test]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuoteWithAdditionalNormalParagraph_AllBlocksMarkedUnknownUntilParaEnd()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram” is my favorite word, for thus saith Isaiah "));
			var block3 = new Block("p", 6, 48);
			block3.BlockElements.Add(new ScriptText("“Don't even go there!”"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite word, for thus saith Isaiah ", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);

			Assert.AreEqual("“Don't even go there!”", output[3].GetText(true));
			Assert.AreEqual("Jesus", output[3].CharacterId);
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[Test]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuoteWithAdditionalNormalParagraph_DialogueQuoteContinuesUntilParagraphEndsWithSentenceEndingPunctuation()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram” is my favorite word, for thus saith Isaiah "));
			var block3 = new Block("p", 6, 48);
			block3.BlockElements.Add(new ScriptText("“Don't even go there!”"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” is my favorite word, for thus saith Isaiah ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("“Don't even go there!”", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		//[TestCase(QuoteUtils.kSentenceEndingPunctuation)]
		[TestCase(null)]
		public void Parse_DialogueDashFollowedImmediatelyByOpeningFirstLevelQuoteWithAdditionalPoetryParagraph_AllBlocksAfterStartOfQuoteMarkedUnknown(string dialogQuoteEnd)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram” is my favorite word, for thus saith Isaiah "));
			var block3 = new Block("m", 6, 48);
			block3.BlockElements.Add(new ScriptText("“Don't even go there!”"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", dialogQuoteEnd);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” is my favorite word, for thus saith Isaiah “Don't even go there!”", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[Test]
		public void Parse_DialogueDashFollowedByOpeningFirstLevelQuoteSentenceEndingPunctuationAndAnAdditionalPoetryParagraph_AllBlocksMarkedUnknownUntilExplicitEnd()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram” is my favorite word, for thus saith Isaiah? "));
			var block3 = new Block("m", 6, 48);
			block3.BlockElements.Add(new ScriptText("“Don't even go there!”"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("—“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite word, for thus saith Isaiah? ", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);

			Assert.AreEqual("“Don't even go there!”", output[3].GetText(true));
			Assert.AreEqual("Jesus", output[3].CharacterId);
		}

		[TestCase(".”", QuoteUtils.kSentenceEndingPunctuation)]
		[TestCase("”.", QuoteUtils.kSentenceEndingPunctuation)]
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[TestCase(".”", null)]
		[TestCase("”.", null)]
		public void Parse_DialogueColonFollowedImmediatelyByOpeningFirstLevelQuoteWithExplicitEnd_QuoteIdentifiedCorrectly(string endingPunctuation, string dialogQuoteEnd)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: “Nintimrataram splintaram" + endingPunctuation));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", dialogQuoteEnd);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram splintaram" + endingPunctuation, output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[Test]
		public void Parse_DialogueColonFollowedImmediatelyByOpeningFirstLevelQuote_AllBlocksMarkedUnknownUntilExplicitEnd()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: “Nintimrataram” is my favorite word."));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite word.", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);
		}

		[TestCase(".”")]
		[TestCase("”.")]
		public void Parse_DialogueColonFollowedImmediatelyByOpeningFirstLevelQuote_AllBlocksMarkedUnknownUntilExplicitEnd(string endingPunctuation)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: “Nintimrataram” is my favorite “word" + endingPunctuation));
			var input = new List<Block> { block1, };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite ", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);

			Assert.AreEqual("“word" + endingPunctuation, output[3].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[3].CharacterId);
		}

		[TestCase("p", ":")]
		[TestCase("m", ":")]
		[TestCase("m", ",")]
		public void Parse_DialogueColonFollowedInNextParaByOpeningFirstLevelQuote_AllBlocksMarkedUnknownUntilExplicitEnd(string paraTagForIsaiah, string punctuationToIntroduceIsaiah)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("“Nintimrataram” is my favorite word, for thus saith Isaiah" + punctuationToIntroduceIsaiah));
			var block3 = new Block(paraTagForIsaiah, 6, 48);
			block3.BlockElements.Add(new ScriptText("Don't even go there!"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite word, for thus saith Isaiah" + punctuationToIntroduceIsaiah, output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);

			Assert.AreEqual("Don't even go there!", output[3].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[3].CharacterId);
		}

		[TestCase("“")]
		[TestCase("”")]
		public void Parse_DialogueColonFollowedInNextParaByOpeningFirstLevelQuote_AllBlocksMarkedUnknownUntilParagraphEnd(string firstLevelContinuer)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("“Nintimrataram” is my favorite word, for thus saith Isaiah,"));
			var block3 = new Block("p", 6, 48);
			block3.BlockElements.Add(new ScriptText("Don't even go there!"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” ", output[1].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("is my favorite word, for thus saith Isaiah,", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[2].CharacterId);

			Assert.AreEqual("Don't even go there!", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[TestCase("“")]
		[TestCase("”")]
		public void Parse_DialogueColonFollowedByParagraphThatEndsASentence_EntireFollowingParagraphTreatedAsQuote(string firstLevelContinuer)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("“Nintimrataram” is my favorite word, for thus saith Isaiah."));
			var block3 = new Block("p", 6, 48);
			block3.BlockElements.Add(new ScriptText("Don't even go there!"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” is my favorite word, for thus saith Isaiah.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Don't even go there!", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[TestCase("“")]
		[TestCase("”")]
		public void Parse_DialogueColonFollowedByParagraphThatDoesNotEndASentence_FollowingParagraphsTreatedAsQuoteUntilSentenceEnd(string firstLevelContinuer)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new ScriptText("“Nintimrataram” is my favorite word, for thus saith Isaiah,"));
			var block3 = new Block("m", 6, 48);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new ScriptText("Don't even go there!"));
			var block4 = new Block("p", 6, 48);
			block4.IsParagraphStart = true;
			block4.BlockElements.Add(new ScriptText("I didn't."));
			var input = new List<Block> { block1, block2, block3, block4 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("“Nintimrataram” is my favorite word, for thus saith Isaiah, Don't even go there!", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("I didn't.", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[Test]
		public void Parse_DialogueQuoteContainingInitialNestedQuoteWithSentenceEndingPunctuation_SentenceEndingPunctuationInNestedQuotesIgnored()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram. Jesús timiayi.” Wikia tuke pujutan sukartin asan. Comete un pedazo de flan. "));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{49}\u00A0—“Nintimrataram. Jesús timiayi.” ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Wikia tuke pujutan sukartin asan. Comete un pedazo de flan. ", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_DialogueQuoteContainingMultipleNestedQuotesWithSentenceEndingPunctuation_SentenceEndingPunctuationInNestedQuotesIgnored()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("—“Nintimrataram” is a word about which John said “I never use that word. It's lame, dude!”"));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{49}\u00A0—“Nintimrataram” is a word about which John said “I never use that word. It's lame, dude!”", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_DialogueQuoteContainingSingleNestedQuoteWithSentenceEndingPunctuation_SentenceEndingPunctuationInNestedQuoteIgnored()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("Remember that John said, “He must increase. I must decrease!”"));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0Remember that John said, “He must increase. I must decrease!”", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}

		[TestCase("!”")]
		[TestCase("! ”")]
		[TestCase("”!")]
		public void Parse_DialogueQuoteContainingToEndWithSentenceEndingPunctuationSingleNestedQuoteWithSentenceEndingPunctuationAndSomeFollowingText_DialogueQuoteEndsWithFinalSentenceEndingPunctuationInNestedQuote(string endingPunctuation)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("Remember that John said, “He must increase. I must decrease" + endingPunctuation + " Following text spoken by narrator."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{49}\u00A0Remember that John said, “He must increase. I must decrease" + endingPunctuation + " ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("Following text spoken by narrator.", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}

		[Test]
		public void Parse_DialogueQuoteContainingToEndWithSentenceEndingPunctuationSingleNestedQuoteWithInternalButNotFinalSentenceEndingPunctuationAndSomeFollowingText_DialogueQuoteEndsIgnoresPunctuationInNestedQuote()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("Remember that John said, “He must increase. I must decrease,” and more dialogue."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0Remember that John said, “He must increase. I must decrease,” and more dialogue.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[TestCase("!”")]
		[TestCase("”!")]
		public void Parse_DialogueQuoteToEndAtEndOfParagraphContainingSingleNestedQuoteWithSentenceEndingPunctuationAndSomeFollowingText_DialogueQuoteEndsWithFinalSentenceEndingPunctuationInNestedQuote(string endingPunctuation)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 49);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("Remember that John said, “He must increase. I must decrease" + endingPunctuation + " Text following nested quote."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0Remember that John said, “He must increase. I must decrease" + endingPunctuation + " Text following nested quote.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}

		[Test]
		public void Parse_MidVerseColonFollowedByExplicitQuoteAndSpeakerFollowedByVerseWithout_QuoteBrokenOutAsSeparateBlockWithSpeakerAssigned()
		{
			// Kuna San Blas
			var block1 = new Block("p", 5, 28);
			block1.IsParagraphStart = true;
			block1.BlockElements.Add(new Verse("28"));
			block1.BlockElements.Add(new ScriptText("Ar ome binsaed: “Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed.” "));
			block1.BlockElements.Add(new Verse("29"));
			block1.BlockElements.Add(new ScriptText("Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa."));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("{28}\u00A0Ar ome binsaed: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].IsParagraphStart);

			Assert.AreEqual("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed.” ", output[1].GetText(true));
			Assert.AreEqual("woman, bleeding for twelve years", output[1].CharacterId);
			Assert.AreEqual("thinking", output[1].Delivery);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].IsParagraphStart);

			Assert.AreEqual("{29}\u00A0Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa.", output[2].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
			Assert.IsFalse(output[2].IsParagraphStart);
		}

		[Test]
		public void Parse_MidVerseColonFollowedByExplicitQuoteAndSpeakerFollowedByParagraphWithContinuer_MultiBlockQuoteHasSpeakerAssigned()
		{
			var block1 = new Block("p", 3, 17);
			block1.IsParagraphStart = true;
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("Ar ome binsaed: “Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. "));
			var block2 = new Block("p", 3, 18);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("18"));
			block2.BlockElements.Add(new ScriptText("“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa.”"));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("{17}\u00A0Ar ome binsaed: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].IsParagraphStart);

			Assert.AreEqual("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. ", output[1].GetText(true));
			Assert.AreEqual("God", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].IsParagraphStart);

			Assert.AreEqual("{18}\u00A0“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa.”", output[2].GetText(true));
			Assert.AreEqual("God", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsTrue(output[2].IsParagraphStart);
		}

		[TestCase("“", ".")]
		[TestCase("”", ".")]
		[TestCase("$", ".")]
		[TestCase("“", null)]
		[TestCase("”", null)]
		[TestCase("$", null)]
		public void Parse_ColonFollowedByNormalInlineQuoteWithTwoContinuationParagraphs_AllContinuationParasKeptTogether(string firstLevelContinuer, string dialogueQuoteCloser)
		{
			var block1 = new Block("p", 6, 10);
			block1.IsParagraphStart = true;
			block1.BlockElements.Add(new Verse("10"));
			block1.BlockElements.Add(new ScriptText("Jesús e-sapinganga sogdebalid: “Ar bemar neg-gwagwense dogdapile, obunnomaloed."));
			var block2 = new Block("p", 6, 11);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("11"));
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye."));
			var block3 = new Block("p", 6, 11);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new ScriptText(firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”"));
			var block4 = new Block("p", 6, 12);
			block4.IsParagraphStart = true;
			block4.BlockElements.Add(new Verse("12"));
			block4.BlockElements.Add(new ScriptText("Jesús-sapingan nadmargu, be-daed be ogwamar. "));
			block4.BlockElements.Add(new Verse("13"));
			block4.BlockElements.Add(new ScriptText("Deginbali."));
			var input = new List<Block> { block1, block2, block3, block4 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), ":", dialogueQuoteCloser);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);

			Assert.AreEqual("{10}\u00A0Jesús e-sapinganga sogdebalid: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].IsParagraphStart);

			Assert.AreEqual("“Ar bemar neg-gwagwense dogdapile, obunnomaloed.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].IsParagraphStart);

			Assert.AreEqual("{11}\u00A0" + firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye.", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsTrue(output[2].IsParagraphStart);

			Assert.AreEqual(firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”", output[3].GetText(true));
			Assert.AreEqual("Jesus", output[3].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);
			Assert.IsTrue(output[3].IsParagraphStart);

			Assert.AreEqual("{12}\u00A0Jesús-sapingan nadmargu, be-daed be ogwamar. {13}\u00A0Deginbali.", output[4].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[4].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[4].MultiBlockQuote);
			Assert.IsTrue(output[4].IsParagraphStart);
		}
		#endregion

		[Ignore]
		[Test]
		public void Parse_PotentialContinuationParagraphMissingCloser_NotSureWhatToDo()
		{
			var block1 = new Block("p", 3, 17);
			block1.IsParagraphStart = true;
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("Ar ome binsaed: “Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. "));
			var block2 = new Block("p", 3, 18);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("18"));
			block2.BlockElements.Add(new ScriptText("“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa."));
			var block3 = new Block("p", 3, 19);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new Verse("19"));
			block3.BlockElements.Add(new ScriptText("Previous paragraph didn't have a closer!"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("{17}\u00A0Ar ome binsaed: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.IsTrue(output[0].IsParagraphStart);

			Assert.AreEqual("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. ", output[1].GetText(true));
			Assert.AreEqual("God", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.IsFalse(output[1].IsParagraphStart);

			Assert.AreEqual("{18}\u00A0“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa.", output[2].GetText(true));
			Assert.AreEqual("God", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.IsTrue(output[2].IsParagraphStart);
		}

		[TestCase("“", true)]
		[TestCase("”", true)]
		[TestCase("%", true)]
		[TestCase("“", false)]
		[TestCase("”", false)]
		[TestCase("%", false)]
		public void Parse_DialogueQuoteInPoetryWithNoExplicitEndFollowedByMorePoetry_SecondPoetryParagraphShouldNotBePartOfQuote(string firstLevelContinuer, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Then he said: "));
			var block2 = new Block("q2", 6, 48);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new ScriptText("This is a poem about a fly"));
			var block3 = new Block("q2", 6, 48);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new ScriptText("who wished he didn't never have to die. Crud!"));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), ":", ".");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Then he said: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("This is a poem about a fly", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("who wished he didn't never have to die. Crud!", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[TestCase("“", true)]
		[TestCase("”", true)]
		[TestCase("%", true)]
		[TestCase("% ^ &", true)]
		[TestCase("% ", true)]
		[TestCase("“", false)]
		[TestCase("”", false)]
		[TestCase("%", false)]
		[TestCase("% ^ &", false)]
		[TestCase("% ", false)]
		public void Parse_DialogueQuoteWithPotentialContinuerFollowingVerseNumber_EndedByQuotationDash(string firstLevelContinuer, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + "Nintimrataram, —Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0" + firstLevelContinuer + "Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("—Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[TestCase("“", true)]
		[TestCase("”", true)]
		[TestCase("“ ", true)]
		[TestCase("” ", true)]
		[TestCase("%", true)]
		[TestCase("% ", true)]
		[TestCase("“", false)]
		[TestCase("”", false)]
		[TestCase("“ ", false)]
		[TestCase("” ", false)]
		[TestCase("%", false)]
		[TestCase("% ", false)]
		public void Parse_DialogueQuoteWithPotentialContinuerFollowingVerseNumberWithExtraSpace_EndedByQuotationDash(string firstLevelContinuer, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText(firstLevelContinuer + " Nintimrataram, —Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", firstLevelContinuer, 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0" + firstLevelContinuer + " Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("—Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithPotentialContinuerNoVerseNumber_EndedByQuotationDash(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("“Nintimrataram, —Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("“Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("—Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[TestCase("“", true)]
		[TestCase("”", true)]
		[TestCase("“", false)]
		[TestCase("”", false)]
		public void Parse_DialogueQuoteWithPotentialContinuer_EndedByUserSpecifiedPunctuation(string levelOneContinuer, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText(levelOneContinuer + "Nintimrataram, ~Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", levelOneContinuer, 1, QuotationMarkingSystemType.Normal), "—", "~");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0" + levelOneContinuer + "Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("~Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteFollowedByBogusOpeningFirstLevelQuote_BogusBlockMarkedAsUnknown(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. —Jesús timiayi."));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram, —Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);

			Assert.AreEqual("—Jesús timiayi.", output[1].GetText(true));
			Assert.IsTrue(output[1].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{49}\u00A0“Nintimrataram, —Jesús timiayi.", output[2].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[2].CharacterId);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[Test]
		public void Parse_DialogueQuoteFollowedByOpeningFirstLevelQuote_DqEndedExplicitlyBySentenceEndingPunctuation()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("Wikia tuke pujutan sukartin asan. "));
			var block3 = new Block("p", 6, 49);
			block3.BlockElements.Add(new Verse("49"));
			block3.BlockElements.Add(new ScriptText("“Nintimrataram. Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("Wikia tuke pujutan sukartin asan. ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0“Nintimrataram. Jesús timiayi.", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[Test]
		public void Parse_InlineDialogueQuoteFollowedByOpeningFirstLevelQuote_EndedBySentenceEndingPunctuation()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: Wikia tuke pujutan sukartin asan; "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram. Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("Wikia tuke pujutan sukartin asan; ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.IsFalse(output[1].IsParagraphStart);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0“Nintimrataram. ", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);

			Assert.AreEqual("Jesús timiayi.", output[3].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[Test]
		public void Parse_DialogueQuoteIntroducedByColonAtEndOfVerseAndParagraph_QuoteAssignedToCharacter()
		{
			var block1 = new Block("p", 6, 47);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.AddVerse(48, "Wikia tuke pujutan sukartin asan; ");
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{48}\u00A0Wikia tuke pujutan sukartin asan; ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_DialogueQuoteIntroducedByColonInMiddleOfVerse_QuoteAssignedToCharacter()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.BlockElements.Add(new ScriptText("Wikia tuke pujutan sukartin asan; "));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("Wikia tuke pujutan sukartin asan; ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_DialogueQuoteIntroducedByColonAtEndOfVerse_QuoteAssignedToCharacter()
		{
			var block1 = new Block("p", 6, 47);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			block1.AddVerse(48, "Wikia tuke pujutan sukartin asan. ");
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("{48}\u00A0Wikia tuke pujutan sukartin asan. ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
		[Test]
		public void Parse_DialogueQuoteFollowedByOpeningFirstLevelQuote_EndedBySentenceEndingPunctuation()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("p", 6, 48);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new ScriptText("Wikia tuke pujutan sukartin asan; "));
			var block3 = new Block("p", 6, 49);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new Verse("49"));
			block3.BlockElements.Add(new ScriptText("“Nintimrataram. Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", QuoteUtils.kSentenceEndingPunctuation);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("Jesus said: ", output[0].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			Assert.AreEqual("Wikia tuke pujutan sukartin asan; ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.IsTrue(output[1].IsParagraphStart);

			Assert.AreEqual("{49}\u00A0“Nintimrataram. ", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);

			Assert.AreEqual("Jesús timiayi.", output[3].GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

		[TestCase("“", true)]
		[TestCase("”", true)]
		[TestCase("%", true)]
		[TestCase("“", false)]
		[TestCase("”", false)]
		[TestCase("%", false)]
		public void Parse_DialogueQuoteWithPotentialContinuerOverMultipleParagraphs_EndedByQuotationDash(string continuer, bool includeSecondNormalLevel)
		{
			// iso: acu
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText(continuer + "Nintimrataram, "));
			var block3 = new Block("p", 6, 50);
			block3.BlockElements.Add(new Verse("50"));
			block3.BlockElements.Add(new ScriptText(continuer + "Antsu yurumkan, —Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", continuer, 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0" + continuer + "Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("{50}\u00A0" + continuer + "Antsu yurumkan, ", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);

			Assert.AreEqual("—Jesús timiayi.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithPotentialAmbiguousContinuerOrOpenerOverMultipleParagraphs_NoBlocksMarkedAsContinuation(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram, "));
			var block3 = new Block("p", 6, 50);
			block3.BlockElements.Add(new Verse("50"));
			block3.BlockElements.Add(new ScriptText("“Antsu yurumkan,” Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0“Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("{50}\u00A0“Antsu yurumkan,” ", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);

			Assert.AreEqual("Jesús timiayi.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithPotentialAmbiguousContinuerOrOpenerOverMultipleParagraphsIncludingFollowOnPoetry_NoBlocksMarkedAsContinuation(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan, "));
			var block2 = new Block("q", 6, 48);
			block2.BlockElements.Add(new ScriptText("This should be a continuation of dialogue. "));
			var block3 = new Block("p", 6, 49);
			block3.BlockElements.Add(new Verse("49"));
			block3.BlockElements.Add(new ScriptText("“Nintimrataram, "));
			var block4 = new Block("p", 6, 50);
			block4.BlockElements.Add(new Verse("50"));
			block4.BlockElements.Add(new ScriptText("“Antsu yurumkan,” Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3, block4 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			int i = 0;
			Assert.AreEqual("—Wikia tuke pujutan sukartin asan, This should be a continuation of dialogue. ", output[i].GetText(true));
			Assert.AreEqual("Jesus", output[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[i].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0“Nintimrataram, ", output[++i].GetText(true));
			Assert.AreEqual("Jesus", output[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[i].MultiBlockQuote);

			Assert.AreEqual("{50}\u00A0“Antsu yurumkan,” ", output[++i].GetText(true));
			Assert.AreEqual("Jesus", output[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[i].MultiBlockQuote);

			Assert.AreEqual("Jesús timiayi.", output[++i].GetText(true));
			Assert.IsTrue(output[i].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[i].MultiBlockQuote);
		}

		[TestCase("”", true)]
		[TestCase("%", true)]
		[TestCase("”", false)]
		[TestCase("%", false)]
		public void Parse_DialogueQuoteWithPotentialContinuerOverMultipleParagraphsWithErrantFirstLevelCloser_EndedByEndOfParagraph(string continuer, bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText(continuer + "Nintimrataram, "));
			var block3 = new Block("p", 6, 50);
			block3.BlockElements.Add(new Verse("50"));
			block3.BlockElements.Add(new ScriptText(continuer + "Antsu yurumkan,” Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", continuer, 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);

			Assert.AreEqual("{49}\u00A0" + continuer + "Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);

			Assert.AreEqual("{50}\u00A0" + continuer + "Antsu yurumkan,” Jesús timiayi.", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteWithPotentialContinuer_EndedByFirstLevelEnd(bool includeSecondNormalLevel)
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram,” Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);

			Assert.AreEqual("{49}\u00A0“Nintimrataram,” ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteUsesHyphen_ParsesCorrectly(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("-Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, -timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "-", "-");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("-Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("-timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteUsesTwoHyphens_ParsesCorrectly(bool includeSecondNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("--Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, --timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "--", "--");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("--Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("--timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public void Parse_DialogueQuoteUsesEndash_ParsesCorrectly(bool includeFirstNormalLevel, bool includeSecondNormalLevel, bool includeThirdNormalLevel)
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("–", "–", null, 1, QuotationMarkingSystemType.Narrative));
			if (includeFirstNormalLevel)
			{
				quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
				if (includeSecondNormalLevel)
				{
					quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
					if (includeThirdNormalLevel)
						quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 3, QuotationMarkingSystemType.Normal));
				}
			}
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("–timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public void Parse_DialogueQuoteUsesCommaAndTextForEnding_ParsesCorrectly(bool includeFirstNormalLevel, bool includeSecondNormalLevel, bool includeThirdNormalLevel)
		{
			// Cuiba (cui) PG-589
			var block = new Block("p", 2, 2);
			block.BlockElements.Add(new ScriptText("—¿Exota naexana pexuyo, po pexuyo judiomonae itorobiya pia pepa peewatsinchi exanaeinchi poxonae pinyo tsane? Paxan payaputan xua bapon naexana xote tsipei bapon pia opiteito tatsi panaitomatsiya punaenaponan pata nacua werena, po nacuatha ichaxota pocotsiwa xometo weecoina. Papatan xua pata wʉnae jainchiwa tsane barapo pexuyo, jei."));
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("—", ", jei", null, 1, QuotationMarkingSystemType.Narrative));
			if (includeFirstNormalLevel)
			{
				quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
				if (includeSecondNormalLevel)
				{
					quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
					if (includeThirdNormalLevel)
						quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 3, QuotationMarkingSystemType.Normal));
				}
			}
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—¿Exota naexana pexuyo, po pexuyo judiomonae itorobiya pia pepa peewatsinchi exanaeinchi poxonae pinyo tsane? Paxan payaputan xua bapon naexana xote tsipei bapon pia opiteito tatsi panaitomatsiya punaenaponan pata nacua werena, po nacuatha ichaxota pocotsiwa xometo weecoina. Papatan xua pata wʉnae jainchiwa tsane barapo pexuyo", output[0].GetText(false));
			Assert.AreEqual("magi", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual(", jei.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(2, output[1].InitialStartVerseNumber);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_DialogueQuoteUsesCommaAndTextForEndingAndIsFollowedByNormalQuoteThatHappensToBeFollowedByDialogueEndingString_DialogueAndNormalQuoteAreNotCombined(bool includeSecondNormalLevel)
		{
			// Cuiba (cui) PG-589
			var block1 = new Block("p", 9, 15) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("—Jiwi ba jopa bewa."));
			var block2 = new Block("p", 9, 16) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("16"));
			block2.BlockElements.Add(new ScriptText("“Jiwi ba jopa. "));
			block2.BlockElements.Add(new Verse("17"));
			block2.BlockElements.Add(new ScriptText("Mataʉtano bocoto”, jei Jesús."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", ", jei");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Jiwi ba jopa bewa.", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(9, output[0].ChapterNumber);
			Assert.AreEqual(15, output[0].InitialStartVerseNumber);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("“Jiwi ba jopa. Mataʉtano bocoto”", output[1].GetText(false));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(9, output[1].ChapterNumber);
			Assert.AreEqual(16, output[1].InitialStartVerseNumber);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual(", jei Jesús.", output[2].GetText(false));
			Assert.IsTrue(output[2].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(9, output[2].ChapterNumber);
			Assert.AreEqual(17, output[2].InitialStartVerseNumber);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[Test]
		public void Parse_VerseBridge_CharacterInEachVerse_ResultIsAmbiguous()
		{
			var block1 = new Block("p", 9, 27, 28);
			block1.BlockElements.Add(new Verse("27-28"));
			block1.BlockElements.Add(new ScriptText("«Quote.»"));
			var input = new List<Block> { block1 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[0].CharacterId);
		}

		[Test]
		public void Parse_SeeminglyMultiBlockQuoteDoesntLineUpWithControlFileButRatherIsTwoCharacters_SetToFirstToUnknownAndBothToMultiBlockNone()
		{
			var block1 = new Block("p", 2, 7);
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 2, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker.»"));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("teachers of religious law/Pharisees", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 2, 7).Select(cv => cv.Character).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 2, 8).Select(cv => cv.Character).Single());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_SeeminglyMultiBlockQuoteDoesntLineUpWithControlFileButRatherIsTwoCharactersAndAmbiguous_SetToFirstToUnknownAndToMultiBlockNone()
		{
			var block1 = new Block("p", 19, 16);
			block1.BlockElements.Add(new Verse("16"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 17);
			block2.BlockElements.Add(new Verse("17"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker."));
			var block3 = new Block("p", 19, 18);
			block3.BlockElements.Add(new Verse("18"));
			block3.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker.»"));
			var input = new List<Block> { block1, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("ruler, a certain=man, rich young", ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 16).Select(cv => cv.Character).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 17).Select(cv => cv.Character).Single());
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[2].CharacterId);
		}

		[Test]
		public void Parse_SeeminglyMultiBlockQuoteDoesntLineUpWithControlFileButRatherIsTwoCharactersAndUnknown_SetAllToUnknownAndMultiBlockNone()
		{
			var block1 = new Block("p", 8, 23);
			block1.BlockElements.Add(new Verse("23")); // Jesus
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 2, 24);
			block2.BlockElements.Add(new Verse("24")); // blind man
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker."));
			var block3 = new Block("p", 2, 25);
			block3.BlockElements.Add(new Verse("25")); // no one
			block3.BlockElements.Add(new ScriptText("«Continuation of quote by an unknown speaker."));
			var input = new List<Block> { block1, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 23).Select(cv => cv.Character).Single());
			Assert.AreEqual("blind man", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 24).Select(cv => cv.Character).Single());
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 25).Any());

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[2].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_AmbiguousAndCharacter_SetToCharacter()
		{
			var block1 = new Block("p", 19, 17);
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 18);
			block2.BlockElements.Add(new Verse("18"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker.»"));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 17).Select(cv => cv.Character).Single());
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_SeeminglyMultiBlockQuoteDoesntLineUpWithControlFileButIsRatherCharacterAndUnknown_SetFirstToUnknownAndBothToMultiBlockNone()
		{
			var block1 = new Block("p", 19, 8);
			block1.BlockElements.Add(new Verse("8"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 9);
			block2.BlockElements.Add(new Verse("9"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by unknown speaker."));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 8).Select(cv => cv.Character).Single());
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 9).Any());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
		}

		[Test]
		public void Parse_SeeminglyMultiBlockQuoteDoesntLineUpWithControlFileButIsRatherAmbiguousAndUnknown_SetBothToUnknownAndMultiBlockNone()
		{
			var block1 = new Block("p", 19, 18);
			block1.BlockElements.Add(new Verse("18"));
			block1.BlockElements.Add(new ScriptText("«Ambiguous quote starts."));
			var block2 = new Block("p", 19, 19);
			block2.BlockElements.Add(new Verse("19"));
			block2.BlockElements.Add(new ScriptText("«Quote seems to continue by unknown speaker."));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 19).Any());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_DifferentDeliveries_SetChangeOfDelivery()
		{
			var block1 = new Block("p", 16, 16);
			block1.BlockElements.Add(new Verse("16"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 16, 17);
			block2.BlockElements.Add(new Verse("17"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by same speaker and different delivery.»"));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 16).Select(cv => cv.Character).Single());
			Assert.AreEqual("giving orders", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 16).Select(cv => cv.Delivery).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 17).Select(cv => cv.Character).Single());
			Assert.AreEqual("", ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 17).Select(cv => cv.Delivery).Single());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual("giving orders", output[0].Delivery);
			Assert.AreEqual(MultiBlockQuote.ChangeOfDelivery, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(null, output[1].Delivery);
		}

		[Test]
		public void Parse_MultiBlockQuote_DifferentDeliveries_AmbiguousAndCharacter_SetToCharacter()
		{
			var block1 = new Block("p", 1, 28);
			block1.BlockElements.Add(new Verse("28"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 1, 29);
			block2.BlockElements.Add(new Verse("29"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker.»"));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("angel of the LORD, an", ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 28).Select(cv => cv.Character).Single());
			Assert.AreEqual("to Mary", ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 28).Select(cv => cv.Delivery).Single());
			Assert.AreEqual(1, ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 29).Count(c => c.Character == "angel of the LORD, an" && c.Delivery == ""));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 29).Select(cv => cv.Character).Contains("Mary (Jesus' mother)"));

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("angel of the LORD, an", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("angel of the LORD, an", output[1].CharacterId);
		}

		[Test]
		public void Parse_DialogueQuoteOnly_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("–", "–", null, 1, QuotationMarkingSystemType.Narrative), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("–timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueBeginningQuoteOnly_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("–", null, null, 1, QuotationMarkingSystemType.Narrative), null, null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi.", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);


			var block1 = new Block("p", 1, 17);
			block1.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum."));
			var block2 = new Block("p", 1, 18);
			block2.BlockElements.Add(new ScriptText("timiayi."));
			input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum.", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(null, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(18, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_InitialParagraphBeginsWithNonWordFormingCharactersBeforeVerseNumber_IncludeAllCharactersInResultingBlocks()
		{
			var block = new Block("p", 16, 8) { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("[ "));
			block.BlockElements.Add(new Verse("9"));
			block.BlockElements.Add(new ScriptText("Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[0].GetText(false));
			Assert.AreEqual("[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[0].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(16, output[0].ChapterNumber);
			Assert.AreEqual(9, output[0].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_ParagraphAfterScriptureBlockBeginsWithNonWordFormingCharactersBeforeVerseNumber_IncludeAllCharactersInResultingBlocks()
		{
			var block0 = new Block("p", 16, 8) { IsParagraphStart = true };
			block0.BlockElements.Add(new ScriptText("Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo."));
			var block = new Block("p", 16, 8) { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("[ "));
			block.BlockElements.Add(new Verse("9"));
			block.BlockElements.Add(new ScriptText("Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			var input = new List<Block> { block0, block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo.", output[0].GetText(false));
			Assert.AreEqual("Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo.", output[0].GetText(true));
			Assert.AreEqual("[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[1].GetText(false));
			Assert.AreEqual("[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[1].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(16, output[0].ChapterNumber);
			Assert.AreEqual(8, output[0].InitialStartVerseNumber);
			Assert.AreEqual(16, output[1].ChapterNumber);
			Assert.AreEqual(9, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_ParagraphAfterSectionHeaderBeginsWithNonWordFormingCharactersBeforeVerseNumber_IncludeAllCharactersInResultingBlocks()
		{
			var shBlock = new Block("s1", 16, 8) { IsParagraphStart = true };
			shBlock.BlockElements.Add(new ScriptText("Yecu onyutte onen bot Maliam Lamagdala"));
			var block = new Block("p", 16, 8) { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("[ "));
			block.BlockElements.Add(new Verse("9"));
			block.BlockElements.Add(new ScriptText("Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			var input = new List<Block> { shBlock, block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("Yecu onyutte onen bot Maliam Lamagdala", output[0].GetText(false));
			Assert.AreEqual("Yecu onyutte onen bot Maliam Lamagdala", output[0].GetText(true));
			Assert.AreEqual("[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[1].GetText(false));
			Assert.AreEqual("[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. ", output[1].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(16, output[0].ChapterNumber);
			Assert.AreEqual(8, output[0].InitialStartVerseNumber);
			Assert.AreEqual(16, output[1].ChapterNumber);
			Assert.AreEqual(9, output[1].InitialStartVerseNumber);
		}

		//[Test]
		//public void Unparse_OneBlockBecomesThreeBecomesOne_QuoteInMiddle()
		//{
		//	var originalText = "He said, «Go!» quietly.";
		//	var block = new Block("p", 1, 1);
		//	block.BlockElements.Add(new Verse("1"));
		//	block.BlockElements.Add(new ScriptText(originalText));
		//	var input = new List<Block> { block };
		//	QuoteParser.SetQuoteSystem(QuoteSystem.Default);
		//	IList<Block> output1 = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
		//	Assert.AreEqual(3, output1.Count);
		//	Assert.AreEqual("He said, ", output1[0].GetText(false));
		//	Assert.IsTrue(output1[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		//	Assert.AreEqual("«Go!» ", output1[1].GetText(false));
		//	Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output1[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		//	Assert.AreEqual("quietly.", output1[2].GetText(false));
		//	Assert.IsTrue(output1[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));

		//	// now unparse the book
		//	var book = new BookScript("LUK", output1);
		//	var books = new List<BookScript> {book};
		//	var output2 = QuoteParser.Unparse(books);

		//	// expect 1 book
		//	Assert.AreEqual(1, output2.Count);

		//	// expect 1 block
		//	Assert.AreEqual(1, output2.First().Value.Count);

		//	// expect 2 elements
		//	block = output2.First().Value.First();
		//	Assert.AreEqual(2, block.BlockElements.Count);
		//	Assert.IsInstanceOf(typeof(Verse), block.BlockElements[0]);
		//	Assert.IsInstanceOf(typeof(ScriptText), block.BlockElements[1]);
		//	Assert.AreEqual(originalText, ((ScriptText)block.BlockElements[1]).Content);
		//}

		//[Test]
		//public void Unparse_UnparseProject_Works()
		//{
		//	var booksToInclude = Enum.GetValues(typeof (TestProject.TestBook)).Cast<TestProject.TestBook>().ToList();

		//	var preparsed = TestProject.BooksBeforeQuoteParse(booksToInclude.ToArray());
		//	var project = TestProject.CreateTestProject(booksToInclude.ToArray());
		//	var unparsed = QuoteParser.Unparse(project.Books);
		//	var unparsedKeys = unparsed.Keys.ToList();

		//	// sort the lists so the books are in the same order
		//	preparsed.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));
		//	unparsedKeys.Sort((a, b) => BCVRef.BookToNumber(a.BookId).CompareTo(BCVRef.BookToNumber(b.BookId)));

		//	// both sets contain the same number of books
		//	Assert.AreEqual(preparsed.Count, unparsed.Count);

		//	for (var i = 0; i < preparsed.Count; i++)
		//	{
		//		var preparsedBlocks = preparsed[i].Blocks;
		//		var unparsedBlocks = unparsed[unparsedKeys[i]];

		//		// both books contains the same number of blocks
		//		Assert.AreEqual(preparsedBlocks.Count, unparsedBlocks.Count);

		//		for (var j = 0; j < preparsedBlocks.Count; j++)
		//		{
		//			// both blocks contain the same number of elements
		//			Assert.AreEqual(preparsedBlocks[j].BlockElements.Count, unparsedBlocks[j].BlockElements.Count);
		//		}
		//	}
		//}

		[Test]
		public void Parse_OnlyQuoteMarkerIsColon_DoesNotThrow()
		{
			// this is a test for PG-636

			// set up some text that uses a colon as a dialog marker
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied: Isaiah was right when he prophesied about you."));
			block.UserConfirmed = false;
			var input = new List<Block> { block };

			// set up a quote system that only has a colon
			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark(":", "", null, 1, QuotationMarkingSystemType.Narrative)
			};
			QuoteParser.SetQuoteSystem(new QuoteSystem(levels));

			// originally it was throwing "Object reference not set to an instance of an object."
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input);
			Assert.DoesNotThrow(() => parser.Parse());
		}

		[TestCase("—")]
		[TestCase("-")]
		public void Parse_NoPairedQuotationMarks_ImplicitContinuationInPoetryFollowedByExplicitCloser_DoesNotThrow(string quoteDashMark)
		{
			// this is a test for PG-778. The problem data is from Mat 4:6:
			// <para style="p">
			//   <verse number="6" style="v"/>—Papa Diosen baque hihquish, naman paquehue. Jahuen huisha janin ta Papan yohihiqui:</para>
			// <para style="q">Jahuen yonotibaan ta mia coirantihiqui. Jaton mequemanbi ta mia bihinihnicantihiqui, macan qui min joxcorohyamanon</para>
			// <para style="m">—hahquin yoshiman.</para>

			// set up some text that uses a colon as a dialog marker
			var blockP = new Block("p", 4, 6) { IsParagraphStart = true };
			blockP.AddVerse(6, quoteDashMark + "Papa Diosen baque hihquish, naman paquehue. Jahuen huisha janin ta Papan yohihiqui:");
			var blockQ = new Block("q", 4, 6) { IsParagraphStart = true };
			blockQ.BlockElements.Add(new ScriptText("Jahuen yonotibaan ta mia coirantihiqui. Jaton mequemanbi ta mia bihinihnicantihiqui, macan qui min joxcorohyamanon"));
			var blockM = new Block("m", 4, 6) { IsParagraphStart = true };
			blockM.BlockElements.Add(new ScriptText(quoteDashMark + "hahquin yoshiman."));
			var input = new List<Block> { blockP, blockQ, blockM };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark(quoteDashMark, quoteDashMark, null, 1, QuotationMarkingSystemType.Narrative)
			};
			QuoteParser.SetQuoteSystem(new QuoteSystem(levels));

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			// originally this was throwing an index-out-of-range exception
			var result = parser.Parse().ToList();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("Satan", result[0].CharacterId);
			Assert.AreEqual(blockP.GetText(true) + " " + blockQ.GetText(true), result[0].GetText(true));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.Narrator),
				result[1].CharacterId);
			Assert.AreEqual(blockM.GetText(true), result[1].GetText(true));
		}

		[TestCase("—")]
		[TestCase("-")]
		public void Parse_NoPairedQuotationMarks_ImplicitContinuationInPoetryWithNoExplicitCloser_DoesNotThrow(string quoteDashMark)
		{
			// this is a test for PG-778. The problem data is from Mat 4:6:
			// <para style="p">
			//   <verse number="6" style="v"/>—Papa Diosen baque hihquish, naman paquehue. Jahuen huisha janin ta Papan yohihiqui:</para>
			// <para style="q">Jahuen yonotibaan ta mia coirantihiqui. Jaton mequemanbi ta mia bihinihnicantihiqui, macan qui min joxcorohyamanon</para>
			// <para style="m">—hahquin yoshiman.</para>

			// set up some text that uses a colon as a dialog marker
			var blockP = new Block("p", 4, 6) { IsParagraphStart = true };
			blockP.AddVerse(6, quoteDashMark + "Papa Diosen baque hihquish, naman paquehue. Jahuen huisha janin ta Papan yohihiqui:");
			var blockQ = new Block("q", 4, 6) { IsParagraphStart = true };
			blockQ.BlockElements.Add(new ScriptText("Jahuen yonotibaan ta mia coirantihiqui. Jaton mequemanbi ta mia bihinihnicantihiqui, macan qui min joxcorohyamanon"));
			var blockM = new Block("m", 4, 6) { IsParagraphStart = true };
			blockM.BlockElements.Add(new ScriptText(quoteDashMark + "hahquin yoshiman."));
			var input = new List<Block> { blockP, blockQ, blockM };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark(quoteDashMark, "", null, 1, QuotationMarkingSystemType.Narrative)
			};
			QuoteParser.SetQuoteSystem(new QuoteSystem(levels));

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			// originally this was throwing an index-out-of-range exception
			var result = parser.Parse().ToList();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("Satan", result[0].CharacterId);
			Assert.AreEqual(blockP.GetText(true) + " " + blockQ.GetText(true) + " " + blockM.GetText(true), result[0].GetText(true));
		}

		[TestCase("p", "q1", "q2")]
		[TestCase("p", "q", "m")]
		[TestCase("p", "pi1", "pi2")]
		[TestCase("q1", "q2", "m")]
		public void Parse_PoetryLinesInDifferentVersesWithNoInterveningSentenceEndingPunctuation_VersesAreNotCombined(string style1, string style2, string style3)
		{
			var blockChapter1 = new Block("c", 1) { BookCode = "MRK", CharacterId = CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter)};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block(style1, 1, 1).AddVerse(1, "Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.");
			var block2 = new Block(style2, 1, 2).AddVerse(2, "This is a poem, ");
			var block3 = new Block(style3, 1, 2);
			block3.BlockElements.Add(new ScriptText("about something good;"));
			var block4 = new Block(style2, 1, 3).AddVerse(3, "So you can see that");
			var block5 = new Block(style3, 1, 3);
			block5.BlockElements.Add(new ScriptText("it's not about something wood."));

			var input = new List<Block> { blockChapter1, block1, block2, block3, block4, block5 };

			// set up a quote system that matches PG-690
			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
			};
			QuoteParser.SetQuoteSystem(new QuoteSystem(levels));

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(4, results.Count);
			Assert.AreEqual("c", results[0].StyleTag);
			Assert.IsTrue(results[0].IsChapterAnnouncement);
			Assert.AreEqual("MRK", results[0].BookCode);
			Assert.AreEqual(1, results[0].ChapterNumber);
			Assert.AreEqual(0, results[0].InitialStartVerseNumber);
			Assert.AreEqual("1", results[0].GetText(true));
			Assert.AreEqual(style1, results[1].StyleTag);
			Assert.AreEqual(1, results[1].ChapterNumber);
			Assert.AreEqual(1, results[1].InitialStartVerseNumber);
			Assert.AreEqual("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.", results[1].GetText(true));
			Assert.AreEqual(style2, results[2].StyleTag);
			Assert.AreEqual(1, results[2].ChapterNumber);
			Assert.AreEqual(2, results[2].InitialStartVerseNumber);
			Assert.AreEqual("{2}\u00A0This is a poem, about something good;", results[2].GetText(true));
			Assert.AreEqual(style2, results[3].StyleTag);
			Assert.AreEqual(1, results[3].ChapterNumber);
			Assert.AreEqual(3, results[3].InitialStartVerseNumber);
		}

		#region Recovery from bad data
		[Test]
		public void Parse_FirstLevelCloseMissingFollowedByNoControlFileEntry_CloseQuoteWhenNoCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 1, 3); // God
			block1.BlockElements.Add(new Verse("3"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 1, 4); // no one
			block2.BlockElements.Add(new Verse("4"));
			block2.BlockElements.Add(new ScriptText("Back to narrator.  No one in the control file for this verse. "));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Quote.", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);

			Assert.AreEqual("Back to narrator.  No one in the control file for this verse. ", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
		}
		[Test]
		public void Parse_FirstLevelCloseMissingInBlockWithVerseBridgeFollowedByNoControlFileEntry_CloseQuoteWhenNoCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 1, 2, 3); // God
			block1.BlockElements.Add(new Verse("2-3"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 1, 4); // no one
			block2.BlockElements.Add(new Verse("4"));
			block2.BlockElements.Add(new ScriptText("Back to narrator.  No one in the control file for this verse. "));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{2-3}\u00A0«Quote.", output[0].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual(3, output[0].InitialEndVerseNumber);
			Assert.AreEqual(3, output[0].LastVerseNum);

			Assert.AreEqual("{4}\u00A0Back to narrator.  No one in the control file for this verse. ", output[1].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
			Assert.AreEqual(0, output[1].InitialEndVerseNumber);
			Assert.AreEqual(4, output[1].LastVerseNum);
		}

		[Test]
		public void Parse_QuoteContinuesIntoVerseWithNoOneInControlFile_BreakBlockAndSetFirstPartUnknown()
		{
			var block1 = new Block("p", 1, 3);
			block1.BlockElements.Add(new Verse("3")); // God
			block1.BlockElements.Add(new ScriptText("«Quote. "));
			block1.BlockElements.Add(new Verse("4")); // no one
			block1.BlockElements.Add(new ScriptText("No one in the control file for this verse. "));
			var input = new List<Block> { block1 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{3}\u00A0«Quote. ", output[0].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual(0, output[0].InitialEndVerseNumber);
			Assert.AreEqual(3, output[0].LastVerseNum);

			Assert.AreEqual("{4}\u00A0No one in the control file for this verse. ", output[1].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
			Assert.AreEqual(0, output[1].InitialEndVerseNumber);
			Assert.AreEqual(4, output[1].LastVerseNum);
		}

		[Test]
		public void Parse_QuoteSeemsToContinueAfterAllOriginalCharactersHaveBeenEliminated_BreakBlockAndSetFirstPartUnknown()
		{
			var block1 = new Block("p", 23, 1, 2);
			block1.BlockElements.Add(new Verse("1-2")); // FakeGuy1 and FakeGuy2
			block1.BlockElements.Add(new ScriptText("«Quote. "));
			block1.BlockElements.Add(new Verse("3")); // FakeGuy1
			block1.BlockElements.Add(new ScriptText("Possible continuation of quote. "));
			block1.BlockElements.Add(new Verse("4")); // FakeGuy2
			block1.BlockElements.Add(new ScriptText("Further possible continuation of quote (but it isn't because there is no continuity of characters).»"));
			var input = new List<Block> { block1 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "REV", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("{1-2}\u00A0«Quote. {3}\u00A0Possible continuation of quote. ", output[0].GetText(true));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(1, output[0].InitialStartVerseNumber);
			Assert.AreEqual(2, output[0].InitialEndVerseNumber);
			Assert.AreEqual(3, output[0].LastVerseNum);

			Assert.AreEqual("{4}\u00A0Further possible continuation of quote (but it isn't because there is no continuity of characters).»", output[1].GetText(true));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_FirstLevelCloseMissingFollowedByExplicitNarratorInControlFile_CloseQuoteWhenNoCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 1, 9); // God
			block1.BlockElements.Add(new Verse("9"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 1, 10); // narrator-GEN
			block2.BlockElements.Add(new Verse("10"));
			block2.BlockElements.Add(new ScriptText("Back to narrator.  Narrator is explicitly in the control file for this verse. "));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Quote.", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("Back to narrator.  Narrator is explicitly in the control file for this verse. ", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_FirstLevelCloseMissingAfterMultipleBlocks_CloseQuoteWhenNoCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 3, 4); // serpent
			block1.BlockElements.Add(new Verse("4"));
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var block2 = new Block("p", 3, 5); // serpent
			block2.BlockElements.Add(new Verse("5"));
			block2.BlockElements.Add(new ScriptText("Still probably in a quote. "));
			var block3 = new Block("p", 3, 6); // no one
			block3.BlockElements.Add(new Verse("6"));
			block3.BlockElements.Add(new ScriptText("Back to narrator.  No one in the control file for this verse. "));
			var input = new List<Block> { block1, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("Still probably in a quote. ", output[1].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("Back to narrator.  No one in the control file for this verse. ", output[2].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_FirstLevelCloseMissingInBlockWithOnePossibleCharacter_CloseQuoteWhenDifferentCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 3, 12); // Adam
			block1.BlockElements.Add(new Verse("12"));
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var block2 = new Block("p", 3, 13); // Eve
			block2.BlockElements.Add(new Verse("13"));
			block2.BlockElements.Add(new ScriptText("Different character in the control file here but no quote marks, so set to narrator. "));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("Different character in the control file here but no quote marks, so set to narrator. ", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_FirstLevelCloseMissingInBlockWithOnePossibleCharacter_CloseQuoteWhenGetToVerseWithoutThatCharacterInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 3, 11); // God
			block1.BlockElements.Add(new Verse("11"));
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var block2 = new Block("p", 3, 12); // Adam
			block2.BlockElements.Add(new Verse("12"));
			block2.BlockElements.Add(new ScriptText("«Quote by different character.» "));
			var input = new List<Block> { block1, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("«Quote by different character.» ", output[1].GetText(false));
			Assert.AreEqual("Adam", output[1].CharacterId);
		}

		[Test]
		public void Parse_FirstLevelCloseMissingInAmbiguousQuote_CloseQuoteWhenGetToVerseWithoutAnyOfThoseCharactersInControlFileAndSetCharacterForRelevantBlocksToUnknown()
		{
			var block1 = new Block("p", 5, 40); // crowd and Jesus
			block1.BlockElements.Add(new Verse("40"));
			block1.BlockElements.Add(new ScriptText("«Ambiguous quote. But where does it end? "));
			var block2 = new Block("p", 5, 41); // Jesus and narrator-MRK
			block2.BlockElements.Add(new Verse("41"));
			block2.BlockElements.Add(new ScriptText("Could still be a quote. "));
			var block3 = new Block("p", 5, 42); // No one
			block3.BlockElements.Add(new Verse("42"));
			block3.BlockElements.Add(new ScriptText("Back to narrator. "));
			var input = new List<Block> { block1, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("«Ambiguous quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("Could still be a quote. ", output[1].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);

			Assert.AreEqual("Back to narrator. ", output[2].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtEndOfBook_SetToUnknown()
		{
			var block1 = new Block("p", 5, 43); // Jesus
			block1.BlockElements.Add(new Verse("43"));
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var input = new List<Block> { block1 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtSectionHeaderWithCharacterNotInNextVerse_SetToUnknown()
		{
			var block1 = new Block("p", 5, 43) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("43")); // Jesus
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var blockSect = new Block("s", 5, 43) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 5, 44) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("44")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var input = new List<Block> { block1, blockSect, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("Section Header", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("No character in control file for this verse. ", output[2].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtSectionHeaderWithVerseContinuingButQuoteNeverClosed_BreakBlockAndSetScriptureBlocksToUnknown()
		{
			var block1 = new Block("p", 5, 43) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("43")); // Jesus
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var blockSect = new Block("s", 5, 43) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 5, 43) { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("Verse and quote continues after section header. "));
			block2.BlockElements.Add(new Verse("44")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var input = new List<Block> { block1, blockSect, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("«Quote. But where does it end? ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("Section Header", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("Verse and quote continues after section header. ", output[2].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[2].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);

			Assert.AreEqual("No character in control file for this verse. ", output[3].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_FirstLevelStillOpenFromMultiVerseBlockAtSectionHeaderWithCharacterNotInNextVerse_SetToUnknown()
		{
			var block1 = new Block("p", 15, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("16")); //Jesus
			block1.BlockElements.Add(new ScriptText("He said, <<Quote. But where does it end? "));
			block1.BlockElements.Add(new Verse("17")); // Jesus
			block1.BlockElements.Add(new ScriptText("Quote continues after verse break."));
			var blockSect = new Block("s", 15, 17) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 15, 18) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("18")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal), null, null);
			var input = new List<Block> { block1, blockSect, block2 };
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(16, output[0].InitialStartVerseNumber);
			Assert.AreEqual(0, output[0].InitialEndVerseNumber);
			Assert.AreEqual(16, output[0].LastVerseNum);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);

			Assert.AreEqual("<<Quote. But where does it end? Quote continues after verse break.", output[1].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[1].CharacterId);
			Assert.AreEqual(16, output[1].InitialStartVerseNumber);
			Assert.AreEqual(0, output[1].InitialEndVerseNumber);
			Assert.AreEqual(17, output[1].LastVerseNum);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);

			Assert.AreEqual("Section Header", output[2].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual(17, output[2].InitialStartVerseNumber);
			Assert.AreEqual(0, output[2].InitialEndVerseNumber);
			Assert.AreEqual(17, output[2].LastVerseNum);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);

			Assert.AreEqual("No character in control file for this verse. ", output[3].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(18, output[3].InitialStartVerseNumber);
			Assert.AreEqual(0, output[3].InitialEndVerseNumber);
			Assert.AreEqual(18, output[3].LastVerseNum);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_DialogueQuoteCloseMissingFollowedByNoControlFileEntry_CloseQuoteWhenNoCharacterInControlFileAndSetDialogueQuoteToUnknown()
		{
			var block1 = new Block("p", 1, 3); // God
			block1.BlockElements.Add(new Verse("3"));
			block1.BlockElements.Add(new ScriptText("—Quote. "));
			block1.BlockElements.Add(new Verse("4"));
			block1.BlockElements.Add(new ScriptText("No one in the control file for this verse. "));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), "—", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Quote. ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.kUnknownCharacter, output[0].CharacterId);

			Assert.AreEqual("No one in the control file for this verse. ", output[1].GetText(false));
			Assert.True(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}
		#endregion

		[Test]
		public void Parse_ContinuerSameAsCloserAndBlockStartsWithSpecialOpeningPunctuation_FollowedByRegularQuote_ParsedCorrectly()
		{
			// PG-690, PG-695 (Quichua Cañar - qxr)
			// Added to for PG-705

			// set up some text that uses special opening punctuation
			var block1 = new Block("p", 15, 10) {UserConfirmed = false};
			block1.BlockElements.Add(new Verse("10"));
			block1.BlockElements.Add(new ScriptText("Cutinllatac Dios quillcachishcapica:"));
			var block2 = new Block("p", 15, 10) { UserConfirmed = false };
			block2.BlockElements.Add(new ScriptText("«Tucui llactacuna,"));
			var block3 = new Block("p", 15, 10) { UserConfirmed = false };
			block3.BlockElements.Add(new ScriptText("¡Dios acllashcacunahuan cushicuichic!» ninmi. "));
			block3.BlockElements.Add(new Verse("11"));
			block3.BlockElements.Add(new ScriptText("«T»"));

			var input = new List<Block> { block1, block2, block3 };

			// set up a quote system that matches PG-690
			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
				new QuotationMark("«", "»", "«", 2, QuotationMarkingSystemType.Normal),
				new QuotationMark("«", "»", "»", 3, QuotationMarkingSystemType.Normal)
			};
			QuoteParser.SetQuoteSystem(new QuoteSystem(levels));

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			Assert.AreEqual("Cutinllatac Dios quillcachishcapica:", ((ScriptText)results[0].BlockElements[1]).Content);
			Assert.AreEqual("«Tucui llactacuna,", ((ScriptText)results[1].BlockElements[0]).Content);
			Assert.AreEqual(MultiBlockQuote.Start, results[1].MultiBlockQuote);
			Assert.AreEqual("¡Dios acllashcacunahuan cushicuichic!» ", ((ScriptText)results[2].BlockElements[0]).Content);
			Assert.AreEqual(MultiBlockQuote.Continuation, results[2].MultiBlockQuote);
			Assert.AreEqual("ninmi. ", ((ScriptText)results[3].BlockElements[0]).Content);
			Assert.AreEqual("«T»", ((ScriptText)results[4].BlockElements[1]).Content);
		}

		#region Interruption tests

		// These tests all relate to PG-781
		private void AssertIsInterruption(Block block, string expectedInterruptionText)
		{
			Assert.AreEqual(expectedInterruptionText, block.GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, block.CharacterId,
				"Interruption should be marked as ambiguous to force user to look at it.");
		}

		[TestCase("", "")]
		[TestCase("(", ")")]
		[TestCase("[", "]")]
		[TestCase("-", "-")]
		[TestCase("\u2014", "\u2014")]
		[TestCase("\u2015", "\u2015")]
		public void Parse_VerseWithInterruptionHasNoInterruptionInsideQuote_QuoteAndNarratorTextAssignedAutomatically(string leadingPunct, string trailingPunct)
		{
			string narratorTextV6 = leadingPunct + "that is, to bring Christ down" + trailingPunct;
			string narratorTextV7 = leadingPunct + "that is, to bring Christ up from the dead" + trailingPunct + ".";
			var block1 = new Block("p", 10, 6).AddVerse(6, "But the righteousness that is by faith says: «Do not say in your heart, ‹Who will ascend into heaven?›» " + narratorTextV6)
				.AddVerse(7, "«or ‹Who will descend into the deep?›» " + narratorTextV7);
			var input = new List<Block> { block1 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{6}\u00A0But the righteousness that is by faith says: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual("«Do not say in your heart, ‹Who will ascend into heaven?›» ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			Assert.AreEqual(narratorTextV6, results[++i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual("{7}\u00A0«or ‹Who will descend into the deep?›» ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			Assert.AreEqual(narratorTextV7, results[++i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
		}

		[Test]
		public void Parse_VerseWithInterruptionInBlockByItselfWithQuoteContinuedAfterwardAndInFollowingBlocks_MultiBlockQuoteSettingsAreCorrect()
		{
			var block1 = new Block("p", 24, 15).AddVerse(15, "«Then you will see the ‹abomination of desolation› \u2015let the reader understand\u2015 spoken of by Daniel the prohet. ");
			var block2 = new Block("p", 24, 16).AddVerse(16, "«At that point, the people of Judea need to flee.")
				.AddVerse(17, "Don't go into your house if you're on the roof.")
				.AddVerse(18, "Don't go back to get your coat. ")
				.AddVerse(19, "It will stink if you are pregnant! ");
			var block3 = new Block("p", 24, 20).AddVerse(20, "«Pray that it does not happen in winter. ")
				.AddVerse(21, "It's going to be downright ugly!»");
			var input = new List<Block> { block1, block2, block3 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("{15}\u00A0«Then you will see the ‹abomination of desolation› ", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Interruption:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("\u2015let the reader understand\u2015 ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);

			// Text following interruption in original first block:
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.AreEqual("spoken of by Daniel the prohet. ", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Following blocks:
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{16}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{20}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);
		}

		// PG-1160
		[Test]
		public void Parse_VerseWithInterruptionInBlockByItselfWithQuoteStartedInPrecedingSectionAndContinuedAfterwardAndInFollowingBlocks_MultiBlockQuoteSettingsAreCorrect()
		{
			var block1 = new Block("p", 13, 5).AddVerse(5, "Kun anan Hesus an chicha, “Ammanju ... cha'aju. ")
				.AddVerse(6, "Tan angsan ... manggwana, ‘Sa'on nan Kristu’ ad angsan nan allilagwoncha. ")
				.AddVerse(7, "Ad sa'ad ... luhung. ")
				.AddVerse(8, "Tan ... mangkumut.");
			var block2 = new Block("p", 13, 9).AddVerse(9, "“Manannad aju ... an sa'on. ")
				.AddVerse(10, "Gwon sa'ad ... ayutayuta. ")
				.AddVerse(11, "Ad sa'ad nu ... Ispilitun Apudyus.");
			var block3 = new Block("p", 13, 12).AddVerse(12, "“Ad san ... impa'toycha chicha. ")
				.AddVerse(13, "Ad as cha'aju ... pammatina mataku.");
			var sectionHeadBlock = new Block("s");
			sectionHeadBlock.BlockElements.Add(new ScriptText("Nan Humu'nakan Nan Amod A'oogjat"));
			sectionHeadBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var block4 = new Block("p", 13, 14).AddVerse(14, "“Sa'ad nu ilanju ... sumi'achan, (maserpu maagwatan nan mangwhasa,) sa'ad chachay ingkaw ad Judea ... san whibilig. ")
				.AddVerse(15, "Ad sa'ad nan ... as ijagwidna. ")
				.AddVerse(16, "Ad sa'ad nan ... nan silupna.");
			var block5 = new Block("p", 13, 17).AddVerse(17, "“Ad achagchaku chanan mahuki ja chanan mantatakiwhi san sachi gway chimpu. ")
				.AddVerse(18, "Sija nan ... amod nan tagling. ")
				.AddVerse(19, "Tan sa'ad ... achi kun puyus ma'gwa nan amasna asin. ")
				.AddVerse(20, "Sa'ad nu achin Apudyus ... nan whilang nan erkaw san sachiyay chimpu.");
			var block6 = new Block("p", 13, 21).AddVerse(21, "“Ad sa'ad ..., ‘Ilanju ad, annaja nan Kristu!’ gwinnu ‘Anchiya nan Kristu!’ achiju tuttugwaon. ")
				.AddVerse(22, "Tan lumosgwa chanan ... gway pinilina allilagwoncha chicha. ")
				.AddVerse(23, "Gwon ammanju ... nan annachaja ma'gwa.”");
			var input = new List<Block> { block1, block2, block3, sectionHeadBlock, block4, block5, block6 };

			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“‘", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(10, results.Count);
			// Blocks before section break:
			int i = 0;
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.IsTrue(results[i].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);

			// Section break:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));

			// Text preceeding interruption in original block following section break:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("Jesus", results[i].CharacterId);
			Assert.AreEqual("{14}\u00A0“Sa'ad nu ilanju ... sumi'achan, ", results[i].GetText(true));

			// Interruption:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("(maserpu maagwatan nan mangwhasa,) ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);

			// Text following interruption in original first block:
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Following blocks:
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{17}\u00A0“"));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{21}\u00A0“"));
			Assert.AreEqual("Jesus", results[i].CharacterId);
		}

		[Test]
		public void Parse_VerseWithInterruptionInBlockByItselfWithQuoteContinuedInFollowingBlocks_MultiBlockQuoteSettingsAreCorrect()
		{
			var block1 = new Block("p", 24, 15).AddVerse(15, "«Then you will see the ‹abomination of desolation› spoken of by Daniel the prohet (let the reader understand). ");
			var block2 = new Block("p", 24, 16).AddVerse(16, "«At that point, the people of Judea need to flee.")
				.AddVerse(17, "Don't go into your house if you're on the roof.")
				.AddVerse(18, "Don't go back to get your coat. ")
				.AddVerse(19, "It will stink if you are pregnant! ");
			var block3 = new Block("p", 24, 20).AddVerse(20, "«Pray that it does not happen in winter. ")
				.AddVerse(21, "It's going to be downright ugly!»");
			var input = new List<Block> { block1, block2, block3 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(4, results.Count);
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("{15}\u00A0«Then you will see the ‹abomination of desolation› spoken of by Daniel the prohet ", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Interruption:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("(let the reader understand). ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);

			// Following blocks:
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{16}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{20}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);
		}

		[Test]
		public void Parse_InterruptionInFirstVerseOfBlockWithQuoteContinuedInFollowingVersesAndFollowingBlocks_MultiBlockQuoteSettingsAreCorrect()
		{
			var block1 = new Block("p", 24, 15).AddVerse(15, "«When you see the ‹abomination of desolation› that Daniel prophesied about \u2015let the reader understand\u2015 ")
				.AddVerse(16, "then let those who are in Judea flee. ").AddVerse(17, "Don't go into your house if you're on the roof.");
			var block2 = new Block("p", 24, 18).AddVerse(18, "«Don't go back to get your coat. ")
				.AddVerse(19, "It will stink if you are pregnant!");
			var block3 = new Block("p", 24, 20).AddVerse(20, "«Pray that it does not happen in winter. ")
				.AddVerse(21, "It's going to be downright ugly!»");
			var input = new List<Block> { block1, block2, block3 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("{15}\u00A0«When you see the ‹abomination of desolation› that Daniel prophesied about ", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Interruption:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("\u2015let the reader understand\u2015 ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);

			// Text following interruption in original first block:
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.AreEqual("{16}\u00A0then let those who are in Judea flee. {17}\u00A0Don't go into your house if you're on the roof.", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Following blocks:
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{18}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{20}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);
		}

		[Test]
		public void Parse_InterruptionInSubsequentVerseOfBlockWithQuoteContinuedInFollowingVersesAndFollowingBlocks_MultiBlockQuoteSettingsAreCorrect()
		{
			var block1 = new Block("p", 24, 14).AddVerse(14, "«And this gospel... ").AddVerse(15, "When you see the ‹abomination of desolation› that Daniel prophesied about \u2015let the reader understand\u2015 ")
				.AddVerse(16, "then let those who are in Judea flee. ").AddVerse(17, "Don't go into your house if you're on the roof.");
			var block2 = new Block("p", 24, 18).AddVerse(18, "«Don't go back to get your coat. ")
				.AddVerse(19, "It will stink if you are pregnant!");
			var block3 = new Block("p", 24, 20).AddVerse(20, "«Pray that it does not happen in winter. ")
				.AddVerse(21, "It's going to be downright ugly!»");
			var input = new List<Block> { block1, block2, block3 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("{14}\u00A0«And this gospel... {15}\u00A0When you see the ‹abomination of desolation› that Daniel prophesied about ", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Interruption:
			Assert.AreEqual(MultiBlockQuote.None, results[++i].MultiBlockQuote);
			Assert.AreEqual("\u2015let the reader understand\u2015 ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);

			// Text following interruption in original first block:
			Assert.AreEqual(MultiBlockQuote.Start, results[++i].MultiBlockQuote);
			Assert.AreEqual("{16}\u00A0then let those who are in Judea flee. {17}\u00A0Don't go into your house if you're on the roof.", results[i].GetText(true));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			// Following blocks:
			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{18}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);

			Assert.AreEqual(MultiBlockQuote.Continuation, results[++i].MultiBlockQuote);
			Assert.IsTrue(results[i].GetText(true).StartsWith("{20}\u00A0«"));
			Assert.AreEqual("Jesus", results[i].CharacterId);
		}

		[TestCase("(", ")")]
		[TestCase("[", "]")]
		[TestCase("-", "-")]
		[TestCase("\u2014", "\u2014")]
		[TestCase("\u2015", "\u2015")]
		public void Parse_VerseWithInterruptionHasInterruptionInsideQuote_QuoteAndNarratorTextAssignedAutomatically(string leadingPunct, string trailingPunct)
		{
			string interruptionTextV6 = leadingPunct + "that is, to bring Christ down" + trailingPunct + " ";
			string interruptionTextV7 = leadingPunct + "that is, to bring Christ up from the dead" + trailingPunct + ".» ";
			var block1 = new Block("p", 10, 6).AddVerse(6, "But the righteousness that is by faith says: «Do not say in your heart, ‹Who will ascend into heaven?› " + interruptionTextV6)
				.AddVerse(7, "or ‹Who will descend into the deep?› " + interruptionTextV7);
			var input = new List<Block> { block1 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			int i = 0;
			Assert.AreEqual("{6}\u00A0But the righteousness that is by faith says: ", results[i].GetText(true));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator), results[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("«Do not say in your heart, ‹Who will ascend into heaven?› ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual(interruptionTextV6, results[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			Assert.AreEqual("{7}\u00A0or ‹Who will descend into the deep?› ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			Assert.AreEqual(MultiBlockQuote.None, results[i].MultiBlockQuote);
			AssertIsInterruption(results[++i], interruptionTextV7);
		}

		[Test]
		public void Parse_InterruptionInDialogueQuoteWithNoExplicitEnd_NarratorCouldBeQuotationOrInterruption_DirectSpeechPortionIsAmbiguous()
		{
			var block1 = new Block("p", 1, 42).AddVerse("42", "He brought him unto Jesus.");
			var block2 = new Block("p", 1, 42);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new ScriptText("Jesus looked upon him, and said: Thou art Simon the son of John; thou shalt be called Cephas (which is by interpretation, Peter)."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			int i = 0;
			Assert.AreEqual("{42}\u00A0He brought him unto Jesus.", output[i].GetText(true));
			Assert.IsTrue(output[i].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(42, output[i].InitialStartVerseNumber);

			Assert.AreEqual("Jesus looked upon him, and said: ", output[++i].GetText(true));
			Assert.IsTrue(output[i].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(42, output[i].InitialStartVerseNumber);

			Assert.AreEqual("Thou art Simon the son of John; thou shalt be called Cephas ", output[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[i].CharacterId);
			Assert.AreEqual(42, output[i].InitialStartVerseNumber);

			AssertIsInterruption(output[++i], "(which is by interpretation, Peter).");
			Assert.AreEqual(42, output[i].InitialStartVerseNumber);
		}

		/// <summary>
		/// PG-982: Fix logic to prevent crash caused by incorrectly using length of previous interruption to get
		/// substring of last block element, whose length may be too short. Data is from the Poqomchi project.
		/// </summary>
		[Test]
		public void Parse_VerseWithInterruptionInsideQuoteFollowedByShortVerse_InterruptionsParsedCorrectly()
		{
			var block1 = new Block("p", 10, 6).AddVerse(6, "Re' la' chiriij i korik wach k'uxliis re' inchalik ruuk' i kojb'aal iriq'or i Looq' laj Huuj chi je' wilih: «Ma-aq'or pan ak'ux: “Ha'wach narijohtiik pan taxaaj?” (re're' je' cho yuq'unb'al reh i Kristo reh chi nariqajiik cho); ")
				.AddVerse(7, "oon: “Ha'wach nariqajiik chipaam i richamiil i julkahq?”» (re're' je' cho ruksjiik i Kristo chikixilak taqeh kamnaq). ");
			var input = new List<Block> { block1 };

			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "«“", 2, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(5, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{6}\u00A0Re' la' chiriij i korik wach k'uxliis re' inchalik ruuk' i kojb'aal iriq'or i Looq' laj Huuj chi je' wilih: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual("«Ma-aq'or pan ak'ux: “Ha'wach narijohtiik pan taxaaj?” ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			AssertIsInterruption(results[++i], "(re're' je' cho yuq'unb'al reh i Kristo reh chi nariqajiik cho); ");
			Assert.AreEqual("{7}\u00A0oon: “Ha'wach nariqajiik chipaam i richamiil i julkahq?”» ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, results[i].CharacterId);
			Assert.AreEqual("(re're' je' cho ruksjiik i Kristo chikixilak taqeh kamnaq). ", results[++i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
		}

		/// <summary>
		/// PG-1017: Don't interpret dashes as possible interruptions if they are being used as quotation marks
		/// </summary>
		[TestCase("\u2014")]
		[TestCase("\u2015")]
		public void Parse_QuoteSystemUsesOpenAndCloseDialogueDash_ClosingDialogueDashNotInterpretedAsInterruption(string dialogueDash)
		{
			var block1 = new Block("p", 1, 41).AddVerse(41, "Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri iriguentijeguite, inejapojaqueriqueate icampojiri: " + dialogueDash + "Nonejajiaqueri Meshiashi " + dialogueDash + "ocantaque: “Quirishito.”");
			var input = new List<Block> {block1};

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueDash, dialogueDash);
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(4, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{41}\u00A0Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri iriguentijeguite, inejapojaqueriqueate icampojiri: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual(dialogueDash + "Nonejajiaqueri Meshiashi ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);
			Assert.AreEqual(dialogueDash + "ocantaque: ", results[++i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			AssertIsInterruption(results[++i], "“Quirishito.”");
		}

		/// <summary>
		/// PG-1017: Don't interpret any kind of dashes as possible interruptions if dashes are being used as quotation marks
		/// </summary>
		[TestCase("\u2014", "\u2014")]
		[TestCase("\u2014", "\u2015")]
		[TestCase("\u2015", "\u2014")]
		[TestCase("\u2015", "\u2015")]
		public void Parse_QuoteSystemUsesOpenDialogueDash_SingleFollowingDashNotInterpretedAsInterruption(string dialogueDash, string dashUsedInText)
		{
			var block1 = new Block("p", 1, 41).AddVerse(41, "Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri iriguentijeguite, inejapojaqueriqueate icampojiri: " + dialogueDash + "Nonejajiaqueri Meshiashi " + dashUsedInText + "ocantaque: “Quirishito.”");
			var input = new List<Block> { block1 };

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueDash, null);
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(2, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{41}\u00A0Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri iriguentijeguite, inejapojaqueriqueate icampojiri: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual(dialogueDash + "Nonejajiaqueri Meshiashi " + dashUsedInText + "ocantaque: “Quirishito.”", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);
		}

		/// <summary>
		/// PG-1017: Don't interpret any kind of dashes as possible interruptions if dashes are being used as quotation marks
		/// </summary>
		[TestCase("\u2014", "\u2014")]
		[TestCase("\u2014", "\u2015")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", "\u2014")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", "\u2015")]
		[TestCase("\u2014", " \u2014")]
		[TestCase("\u2014", " \u2015")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", " \u2014")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", " \u2015")]
		[TestCase("\u2014", "\u2014 ")]
		[TestCase("\u2014", "\u2015 ")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", "\u2014 ")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", "\u2015 ")]
		[TestCase("\u2014", " \u2014 ")]
		[TestCase("\u2014", " \u2015 ")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", " \u2014 ")] // Technically, we could allow this to match as an interuption but because these two characters "look" the same, it's probably best not to, right?
		[TestCase("\u2015", " \u2015 ")]
		public void Parse_QuoteSystemUsesOpenDialogueDash_PairedFollowingDashesNotInterpretedAsInterruption(string dialogueDash, string followingDashesUsedInText)
		{
			var dialogueQuotation = dialogueDash + "Nonejajiaqueri Meshiashi" + followingDashesUsedInText + "Quirishito" + followingDashesUsedInText + "ocantaque.";
			var block1 = new Block("p", 1, 41).AddVerse(41, "Ariqueate ... icampojiri: " + dialogueQuotation);
			var input = new List<Block> { block1 };

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueDash, null);
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(2, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{41}\u00A0Ariqueate ... icampojiri: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual(dialogueQuotation, results[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);
		}

		/// <summary>
		/// PG-1017: If dashes are being used as quotation marks, still look for other characters that might signal an interruption.
		/// </summary>
		[TestCase("(", ")", "\u2014")]
		[TestCase("(", ")", "\u2015")]
		[TestCase("[", "]", "\u2014")]
		[TestCase("[", "]", "\u2015")]
		[TestCase("-", "-", "\u2014")]
		[TestCase("-", "-", "\u2015")]
		public void Parse_QuoteSystemUsesDialogueDash_OtherInterruptionCharactersInterpretedAsInterruptions(string leadingPunct, string trailingPunct, string dialogueDash)
		{
			var interruption = leadingPunct + "ocantaque: “Quirishito.”" + trailingPunct;
			var block1 = new Block("p", 1, 41).AddVerse(41, "Ariqueate iriatimpa ... inejapojaqueriqueate icampojiri: " + dialogueDash + "Nonejajiaqueri Meshiashi " + interruption);
			var input = new List<Block> { block1 };

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueDash, null);
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input);
			var results = parser.Parse().ToList();

			Assert.AreEqual(3, results.Count);
			var narrator = CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator);
			int i = 0;
			Assert.AreEqual("{41}\u00A0Ariqueate iriatimpa ... inejapojaqueriqueate icampojiri: ", results[i].GetText(true));
			Assert.AreEqual(narrator, results[i].CharacterId);
			Assert.AreEqual(dialogueDash + "Nonejajiaqueri Meshiashi ", results[++i].GetText(true));
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, results[i].CharacterId);
			AssertIsInterruption(results[++i], interruption);
		}

		#region PG-1079 - I wanted to use MAT 1:23 for these tests like the original error but changing CharacterVerse (to add the Interruption) for that verse broke an existing test
		[Test]
		public void Parse_InterruptionNotInMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14)
				.AddVerse(14, "«The virgin will conceive and give birth to a son, and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«The virgin will conceive and give birth to a son, and they will call him Immanuel ", output[0].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual("(which means God with us).»", output[1].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
		}

		[Test]
		public void Parse_InterruptionAtEndOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive and give birth to a son, ");
			var block2 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block, block2 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("«The virgin will conceive and give birth to a son, ", output[0].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("and they will call him Immanuel ", output[1].GetText(false));
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("(which means God with us).»", output[2].GetText(false));
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
		}

		[Test]
		public void Parse_InterruptionAtEndof3PartMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block, block2, block3 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(4, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_InterruptionNearEndOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us) ");
			var block4 = new Block("p", 13, 14).AddText("thusly.»");
			var input = new List<Block> { block, block2, block3, block4 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(5, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[4].MultiBlockQuote);
			Assert.AreEqual(output[0].CharacterId, output[4].CharacterId);
		}

		[Test]
		public void Parse_InterruptionInMiddleOfMultiBlockQuoteWithFollowingTextInSamePara_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us) thusly ");
			var block5 = new Block("p", 13, 14).AddText("and such»");
			var input = new List<Block> { block, block2, block3, block5 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(6, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, output[4].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[5].MultiBlockQuote);
			Assert.AreEqual(output[0].CharacterId, output[4].CharacterId);
			Assert.AreEqual(output[4].CharacterId, output[5].CharacterId);
		}

		[Test]
		public void Parse_InterruptionInMiddleOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us) ");
			var block4 = new Block("p", 13, 14).AddText("thusly ");
			var block5 = new Block("p", 13, 14).AddText("and such»");
			var input = new List<Block> { block, block2, block3, block4, block5 };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.AreEqual(6, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, output[4].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[5].MultiBlockQuote);
			Assert.AreEqual(output[0].CharacterId, output[4].CharacterId);
			Assert.AreEqual(output[4].CharacterId, output[5].CharacterId);
		}
		#endregion

		//PG-1088
		[Test]
		public void Parse_MultipleInterruptionsInMiddleOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			// Rom 10:5-8
			var block = new Block("p", 10, 5).AddVerse(5, "Augwan ... endagai: Gareg ... naro.")
			.AddVerse(6, "Doba ... ginero. (Anai ... ucugwohob.) ")
			.AddVerse(7, "Dara ... touwero. (Anai ... touohob.) ")
			.AddVerse(8, "Doba ... endagai: Allah ... terimda.");
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(new QuoteSystem(new BulkObservableList<QuotationMark>
			{
				new QuotationMark(":", null, null, 1, QuotationMarkingSystemType.Narrative)
			}));
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();

			Assert.AreEqual(6, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[4].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[5].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("ROM", CharacterVerseData.StandardCharacter.Narrator), output[0].CharacterId);
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, output[1].CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[2].CharacterId);
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, output[3].CharacterId);
			Assert.AreEqual(CharacterVerseData.kAmbiguousCharacter, output[4].CharacterId);
			Assert.AreEqual(CharacterVerse.kScriptureCharacter, output[5].CharacterId);
		}
		#endregion
	}

	[TestFixture]
	public class QuoteParserTestsWithTestCharacterVerseOct2015
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
		}

		[Test]
		public void Parse_BlockContainsNarratorQuotation_NoOtherCharactersInControlFile_RemainsAsSingleBlockAssignedToNarrator()
		{
			var block = new Block("p", 3, 2).AddVerse(2, "A crippled guy was carried to the, «Beautiful» gate to beg.").AddVerse(3, "When he saw...");
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual(block.GetText(true), output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_BlockContainsNarratorQuotation_OtherCharactersInControlFile_BlockSplitAndQuoteMarkedAsAmbiguous()
		{
			var block = new Block("p", 5, 1).AddVerse(1, "This is the book of Adam's race. When God created man, He made him in his image.")
				.AddVerse(2, "He created them and blessed them and named them «Man» that day.");
			var input = new List<Block> { block };
			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.IsTrue(output[1].CharacterIsUnclear());
		}

		[Test]
		public void Parse_TwoAdjacentQuotesBySameCharacter_NotCombined()
		{
			var input = new List<Block>
			{
				new Block("p", 9, 3) {IsParagraphStart = true}.AddVerse(3, "Jesus replied:"),
				new Block("p", 9, 3) {IsParagraphStart = true}.AddText("\u2014Neither this man nor his parents sinned\u2014"),
				new Block("q", 9, 3) {IsParagraphStart = true, MultiBlockQuote = MultiBlockQuote.Start}.AddText("“but this happened so that the works of God might be displayed in him."),
				new Block("q", 9, 4) {IsParagraphStart = true, MultiBlockQuote = MultiBlockQuote.Continuation}.AddVerse(4, "“As long as it is day, we must do the works of him who sent me. Night is coming, when no one can work. ")
					.AddVerse(5, "While I am in the world, I am the light of the world.”")
			};

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "\u2014", "\u2014");
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("narrator-JHN", output[0].CharacterId);
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual("Jesus", output[2].CharacterId);
			Assert.AreEqual("Jesus", output[3].CharacterId);
		}

		// Test for PG-1203
		[TestCase("q", "q")]
		[TestCase("q", "qr")]
		[TestCase("q", "qc")]
		[TestCase("qm1", "qm2")]
		[TestCase("q1", "q2")]
		[TestCase("q", "qm")]
		[TestCase("qc", "qr")]
		public void Parse_AcrosticHeading_NotCombined(string q1, string q2)
		{
			var input = new List<Block>
			{
				new Block(q1, 119, 8) {IsParagraphStart = true}.AddVerse(8, "I will keep thy statutes;"),
				new Block(q2, 119, 8) {IsParagraphStart = true}.AddText("This language doesn't use periods in poetry"),
				new Block("qa", 119, 8) {IsParagraphStart = true}.AddText("Beth"),
				new Block(q1, 119, 9) {IsParagraphStart = true}.AddVerse(9, "How shall a pipsqueak cleanse his way?"),
			};

			QuoteParser.SetQuoteSystem(QuoteSystem.Default);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "PSA", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(q1, output[0].StyleTag);
			Assert.AreEqual("{8}\u00A0I will keep thy statutes; This language doesn't use periods in poetry", output[0].GetText(true));
			Assert.AreEqual("Beth", output[1].GetText(true));
			Assert.AreEqual("{9}\u00A0How shall a pipsqueak cleanse his way?", output[2].GetText(true));
			Assert.IsTrue(output.All(b => b.CharacterIs("PSA", CharacterVerseData.StandardCharacter.Narrator)));
		}

		// Test for PG-1121
		[Test]
		public void Parse_ColonFollowedByNormalInlineQuoteAndSubsequentVerses_ColonNotTreatedAsStartOfDialogue()
		{
			var block1 = new Block("p", 21, 40) { IsParagraphStart = true }
				.AddVerse(40, "Chay capitán “allinmi” niykuptinmi, Pablo upallachirqa. Upallaruptinkum nirqa:");
			var blockChapter22 = new Block("c");
			blockChapter22.BlockElements.Add(new ScriptText("Chapter 22"));
			blockChapter22.SetStandardCharacter("ACT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var block2 = new Block("p", 22, 1) { IsParagraphStart = true }.AddVerse(1, "“Taytakuna, uyariykuwaychik”, nispa.");
			var block3 = new Block("p", 22, 2) { IsParagraphStart = true }.AddVerse(2, "Chaymi hebreo upallarurqaku. Pablo nirqa:");
			var block4 = new Block("p", 22, 3) { IsParagraphStart = true }.AddVerse(3, "Diosninchiktam tukuy servisqaykichikta hina.");
			var block5 = new Block("p", 22, 6) { IsParagraphStart = true }.AddVerse(6, "Ichaqa Damasco qonqayllamanta kanchaykamuwarqa. ")
				.AddVerse(7, "Hinaptinmi pampaman wichiykuspay uyarirqani “¡Saulo, Saulo! ¿Imanasqataq qatikachawanki?” niqta. ")
				.AddVerse(8, "Chaymi ta-purqani: “¿Pitaq Señor?”, nispay. Hinaptinmi arqa: “Ñoqaqa Nazaret Jesusmi kani”, nispan. ")
				.AddVerse(9, "Riqmasiykunapas nisqantaqa. ")
				.AddVerse(10, "Hinaptinmi tapurqani: “¿Señor, ruwasaq?” nispay. Payñataqmi: “Hatarispayki Damasco, chaypimnisunki”, nispa. ");
			var blockChapter23 = new Block("c");
			blockChapter23.BlockElements.Add(new ScriptText("Chapter 23"));
			blockChapter23.SetStandardCharacter("ACT", CharacterVerseData.StandardCharacter.BookOrChapter);
			var block6 = new Block("p", 23, 23) { IsParagraphStart = true }.AddVerse(23, "Soldadokunapa capitanninkunata qayaykuspan kamachirqa:");
			// Note: In verse 24, the quote really should end right before the word "nispan", but we would need to implement PG-487 to support that.
			var block7 = new Block("p", 23, 23) { IsParagraphStart = true }.AddText("Iskay pachak alistaychik, chaynapi llaqtaman rinankupaq. ")
				.AddVerse(24, "Alistaychiktaqyá  Pablo sillakunanpaq, chaynapi sanollata Felixman chayachinaykichikpaq, nispan.");
			var block8 = new Block("p", 23, 25) { IsParagraphStart = true }.AddVerse(25, "Paykunawanmi kayna niq cartata apachirqa:");
			var block9 = new Block("p", 23, 26) { IsParagraphStart = true }
				.AddVerse(26, "“Ancha reqsisqa prefecto Félix, ñoqa Claudio Lisiasmi saludamuyki. ")
				.AddVerse(27, "Pusachimusqay munarqaku. Ichaqa Roma, soldadokunawan rispay salvaramurqani. ")
				.AddVerse(28, "Hinaspaymi imamantam judiokunapa cortenman pusarqani. ")
				.AddVerse(29, "Chaypim yachamurqani acusasqankuta. Ichaqa kananpaqpas. ")
				.AddVerse(30, "Ichaqa wañurachinankupaq qanman pusachimuyki. Chaynallataqmi acusanankupaq”, nispa.");
			var input = new List<Block> { block1, blockChapter22, block2, block3, block4, block5, blockChapter23, block6, block7, block8, block9 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			QuoteParser.SetQuoteSystem(quoteSystem);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();

			int i = 0;
			var block = output[i++];
			Assert.AreEqual("{40}\u00A0Chay capitán ", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("“allinmi” ", block.GetText(true));
			Assert.AreEqual("commander of Roman troops in Jerusalem", block.CharacterId);

			block = output[i++];
			Assert.AreEqual("niykuptinmi, Pablo upallachirqa. Upallaruptinkum nirqa:", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("Chapter 22", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.BookOrChapter));

			block = output[i++];
			Assert.AreEqual("{1}\u00A0“Taytakuna, uyariykuwaychik”, ", block.GetText(true));
			Assert.AreEqual("Paul", block.CharacterId);

			block = output[i++];
			Assert.AreEqual("nispa.", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("{2}\u00A0Chaymi hebreo upallarurqaku. Pablo nirqa:", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("{3}\u00A0Diosninchiktam tukuy servisqaykichikta hina.", block.GetText(true));
			Assert.AreEqual("Paul", block.CharacterId);

			// This would be nice, but we aren't that good yet.
			//block = output[i++];
			//Assert.AreEqual("{6}\u00A0Ichaqa Damasco qonqayllamanta kanchaykamuwarqa. " +
			//	"{7}\u00A0Hinaptinmi pampaman wichiykuspay uyarirqani “¡Saulo, Saulo! ¿Imanasqataq qatikachawanki?” niqta. " +
			//	"{8}\u00A0Chaymi ta-purqani: “¿Pitaq Señor?”, nispay. Hinaptinmi arqa: “Ñoqaqa Nazaret Jesusmi kani”, nispan. " +
			//	"{9}\u00A0Riqmasiykunapas nisqantaqa. " +
			//	"{10}\u00A0Hinaptinmi tapurqani: “¿Señor, ruwasaq?” nispay. Payñataqmi: “Hatarispayki Damasco, chaypimnisunki”, nispa. ", block.GetText(true));
			//Assert.AreEqual("Paul", block.CharacterId);

			while (output[++i].GetText(true) != "Chapter 23");

			block = output[i++];
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.BookOrChapter));

			block = output[i++];
			Assert.AreEqual("{23}\u00A0Soldadokunapa capitanninkunata qayaykuspan kamachirqa:", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("Iskay pachak alistaychik, chaynapi llaqtaman rinankupaq. " +
				"{24}\u00A0Alistaychiktaqyá  Pablo sillakunanpaq, chaynapi sanollata Felixman chayachinaykichikpaq, nispan.", block.GetText(true));
			Assert.AreEqual("commander of Roman troops in Jerusalem", block.CharacterId);

			block = output[i++];
			Assert.AreEqual("{25}\u00A0Paykunawanmi kayna niq cartata apachirqa:", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));

			block = output[i++];
			Assert.AreEqual("{26}\u00A0“Ancha reqsisqa prefecto Félix, ñoqa Claudio Lisiasmi saludamuyki. " +
				"{27}\u00A0Pusachimusqay munarqaku. Ichaqa Roma, soldadokunawan rispay salvaramurqani. " +
				"{28}\u00A0Hinaspaymi imamantam judiokunapa cortenman pusarqani. " +
				"{29}\u00A0Chaypim yachamurqani acusasqankuta. Ichaqa kananpaqpas. " +
				"{30}\u00A0Ichaqa wañurachinankupaq qanman pusachimuyki. Chaynallataqmi acusanankupaq”, ", block.GetText(true));
			Assert.AreEqual("commander of Roman troops in Jerusalem", block.CharacterId);

			block = output[i++];
			Assert.AreEqual("nispa.", block.GetText(true));
			Assert.IsTrue(CharacterVerseData.IsCharacterOfType(block.CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		#region PG-1191: Combine poetry blocks with preceding paragraph only if there's not an expected Scripture quote in the verse
		[Test]
		public void Parse_PoetryNarratorBlockFollowsNormalParagraphInVerseWithoutScriptureQuote_PoetryParagraphsCombinedWithPreceding()
		{
			var input = new List<Block>();
			input.Add(new Block("c", 8, 0) { IsParagraphStart = true, BookCode = "ACT" }.AddText("8"));
			input.Add(new Block("p", 8, 34) { IsParagraphStart = true }.AddVerse(34, "Lë naʼ nayúj lu guich rulabëʼ, rna cni:"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Gulachë́ʼë Lëʼ quië ludöddëʼ"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));

			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddVerse(35, "Gulucaʼnëʼ Lëʼ caʼz,"));
			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddText("Len cutu gluʼë latj nu cuequi xbey Lëʼ."));
			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddText("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));

			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();

			Assert.AreEqual(4, output.Count);

			Assert.IsTrue(output[1].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.AreEqual("p", output[1].StyleTag);
			Assert.AreEqual("{34}\u00A0Lë naʼ nayúj lu guich rulabëʼ, rna cni: Ca böʼcuʼ zxílaʼdauʼ, Gulachë́ʼë Lëʼ quië ludöddëʼ Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ, " +
				"Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ.", output[1].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[2].IsParagraphStart);
			Assert.AreEqual("q", output[2].StyleTag);
			Assert.AreEqual("{35}\u00A0Gulucaʼnëʼ Lëʼ caʼz, Len cutu gluʼë latj nu cuequi xbey Lëʼ.", output[2].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[3].IsParagraphStart);
			Assert.AreEqual("q", output[3].StyleTag);
			Assert.AreEqual("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?", output[3].GetText(true));
		}

		[Test]
		public void Parse_PoetryNarratorBlockFollowsNormalParagraphInVerseWithScriptureQuote_PoetryParasNotCombinedWithNormalPara()
		{
			var input = new List<Block>();
			input.Add(new Block("c", 8, 0) { IsParagraphStart = true, BookCode = "ACT" }.AddText("8"));
			input.Add(new Block("p", 8, 32) { IsParagraphStart = true }.AddVerse(32, "Lë naʼ nayúj lu guich rulabëʼ, rna cni:"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Gulachë́ʼë Lëʼ quië ludöddëʼ"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));

			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddVerse(33, "Gulucaʼnëʼ Lëʼ caʼz,"));
			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddText("Len cutu gluʼë latj nu cuequi xbey Lëʼ."));
			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddText("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));

			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", null);
			QuoteParser.SetQuoteSystem(quoteSystem);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();

			Assert.AreEqual(5, output.Count);

			Assert.IsTrue(output[1].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.AreEqual("p", output[1].StyleTag);
			Assert.AreEqual("{32}\u00A0Lë naʼ nayúj lu guich rulabëʼ, rna cni:", output[1].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[2].IsParagraphStart);
			Assert.AreEqual("q", output[2].StyleTag);
			Assert.AreEqual("Ca böʼcuʼ zxílaʼdauʼ, Gulachë́ʼë Lëʼ quië ludöddëʼ Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ, " +
				"Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ.", output[2].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[3].IsParagraphStart);
			Assert.AreEqual("q", output[3].StyleTag);
			Assert.AreEqual("{33}\u00A0Gulucaʼnëʼ Lëʼ caʼz, Len cutu gluʼë latj nu cuequi xbey Lëʼ.", output[3].GetText(true));
			Assert.IsTrue(output[4].CharacterIs("ACT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsTrue(output[4].IsParagraphStart);
			Assert.AreEqual("q", output[4].StyleTag);
			Assert.AreEqual("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?", output[4].GetText(true));
		}
		#endregion
	}
}
