using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenCharactersTests.Utilities;
using GlyssenEngine;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using GlyssenEngineTests.Script;
using Mono.Unix.Native;
using NUnit.Framework;
using SIL.Extensions;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.WritingSystems;
using static GlyssenCharacters.CharacterSpeakingMode;
using static GlyssenCharacters.CharacterVerseData;
using static GlyssenCharacters.CharacterVerseData.StandardCharacter;
using static GlyssenEngineTests.Quote.QuoteParserTests;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	public class QuoteParserTests
	{
		private static readonly int kMATbookNum = BCVRef.BookToNumber("MAT");
		private static readonly int kMRKbookNum = BCVRef.BookToNumber("MRK");
		private static readonly int kLUKbookNum = BCVRef.BookToNumber("LUK");

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;
		}

		[Test]
		public void Parse_ContainsUserConfirmedBlock_ThrowsInvalidOperationException()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			block.UserConfirmed = true;
			var input = new List<Block> { block };
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input);
			Assert.Throws<InvalidOperationException>(() => parser.Parse());
		}

		#region PG-1419 tests (blocks created by quote milestones)
		// Note: More tests for PG-1419 in QuoteParserTestsWithTestCharacterVerseOct2015
		[TestCase("qt-s")]
		[TestCase("qt1-s")]
		public void Parse_ContainsBlockWithPreConfirmedCharacters_PreConfirmedCharacterAssignmentsUnchanged(string styleTag)
		{
			const string endStyle = "p";
			var blockJ1 = new Block(styleTag, 6, 38) { CharacterId = "Jesus"}
				.AddVerse(38, "How many «loaves» do you have?")
				.AddEndQuoteId();
			var blockN1 = new Block(endStyle, 6, 38).AddText("he asked.");
			var blockD = new Block(styleTag, 6, 38) { CharacterId = "disciples", CharacterIdInScript = "Andrew"}
				.AddText("--Seven, if you count the fish,")
				.AddEndQuoteId();
			var blockN2 = new Block(endStyle, 6, 38).AddText("they replied after Jesus told them,");
			var blockJ2 = new Block(styleTag, 6, 38).AddText("\"Go check!\""); // Note: character not set
			var input = new List<Block> { blockJ1, blockN1, blockD, blockN2, blockJ2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			var narrator = GetStandardCharacterId("MRK", Narrator);
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].HasPreConfirmedCharacter, Is.True);

			Assert.That(output[1].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[1].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[2].CharacterId, Is.EqualTo("disciples"));
			Assert.That(output[2].CharacterIdInScript, Is.EqualTo("Andrew"));
			Assert.That(output[2].HasPreConfirmedCharacter, Is.True);

			Assert.That(output[3].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[3].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[4].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[4].HasPreConfirmedCharacter, Is.False);
		}

		[TestCase(null)]
		[TestCase("Andrew")]
		// These two cases are probably unlikely, but they could occur (perhaps, for example,
		// if someone exported a script from Glyssen that was not disambiguated and then
		// processed that to generate milestones in the USFM data.
		[TestCase(kUnexpectedCharacter)]
		[TestCase(kAmbiguousCharacter)] 
		public void Parse_ContainsBlockWithPreConfirmedCharactersFollowingUnclosedFirstLevelQuote_ExistingQuoteIsClosed(
			string character)
		{
			var block1 = new Block("p", 6, 38)
				.AddVerse(38, "Jesus asked, «How many loaves do you have? Go check!"); // No close quote
			var block2 = new Block("qt-s", 6, 38) { CharacterId = character }
				.AddText("--Seven, if you count the fish,")
				.AddEndQuoteId();
			var block3 = new Block("p", 6, 38).AddText("they replied.");
			var input = new List<Block> { block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			var narrator = GetStandardCharacterId("MRK", Narrator);
			Assert.That(output[0].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[0].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[1].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[1].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[2].CharacterId, Is.EqualTo(character ?? kAmbiguousCharacter));
			Assert.That(output[2].IsPredeterminedFirstLevelQuoteStart, Is.True);

			Assert.That(output[3].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[3].HasPreConfirmedCharacter, Is.False);
		}

		/// <summary>
		/// Unlikely scenario: Parser-detected quote, followed by explicit quote, followed by
		/// special character style (\wj) that implies a speaker.
		/// </summary>
		/// <param name="character"></param>
		[TestCase(null, true)]
		[TestCase("Philip the apostle", true)]
		[TestCase("Philip the apostle", false)]
		// These two cases are probably unlikely, but they could occur (perhaps, for example,
		// if someone exported a script from Glyssen that was not disambiguated and then
		// processed that to generate milestones in the USFM data.
		[TestCase(kUnexpectedCharacter, true)]
		[TestCase(kAmbiguousCharacter, false)]
		public void Parse_SpecialCharacterStyleFollowingExplicitQuoteFollowingParserDetectedQuote_CorrectCharactersAssignedForAllQuotes(
			string character, bool closeQuotes)
		{
			var delivery = character == "Philip the apostle" ? "frustrated" : null;
			var block1 = new Block("p", 6, 37)
				.AddVerse(37, "But Jesus answered, «No, you feed them, Phil!" +
				(closeQuotes ? "»" : " "));
			var block2 = new Block("qt-s", 6, 37)
				{
					IsParagraphStart = closeQuotes,
					CharacterId = character,
					Delivery = delivery
 				}
				.AddText("-- Do you seriously expect us to spend more than half a year’s wages " +
					"to feed them? ");
			var block3 = new Block("wj", 6, 38) { IsParagraphStart = true, CharacterId = "Jesus" }
				.AddVerse(38, "-- How many loaves do you have? Go check!");
			var block4 = new Block("p", 6, 38)
				.AddText("When they found out, they said, «Five, plus two squirmy fish.»");
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			var narrator = GetStandardCharacterId("MRK", Narrator);
			int i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[++i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[i].HasPreConfirmedCharacter, Is.False);
			Assert.That(output[i].GetText(includeVerseNumbers:false),
				Does.StartWith("«No, you feed them, Phil!"));

			Assert.That(output[++i].CharacterId, Is.EqualTo(character ?? kAmbiguousCharacter));
			Assert.That(output[i].Delivery, Is.EqualTo(delivery));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[++i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[i].HasPreConfirmedCharacter, Is.False);
			Assert.That(output[i].GetText(includeVerseNumbers:false),
				Is.EqualTo("«Five, plus two squirmy fish.»"));

			Assert.That(output.Count, Is.EqualTo(i + 1));
		}

		[TestCase(false, false)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(true, true)]
		public void Parse_QuoteWithPreConfirmedCharactersAtEndOfParagraph_PreConfirmedCharacterAssignmentsUnchanged(
			bool includeFollowingParagraph, bool includeStartingQuoteIdAnnotation)
		{
			// Note: In this case, the block that starts the quote will have a QuoteId annotation at the
			// end, but there will be no subsequent block that has the qt-e tag (i.e.,
			// IsPredeterminedFirstLevelQuoteEnd will not be true for either the quote block or the subsequent
			// block).
			const string styleTag = "qt-s";
			var block1 = new Block("p", 6, 38)
				.AddVerse(38, "Jesus asked, «How many loaves do you have? Go check!» The disciples replied, " );
			var block2 = new Block(styleTag, 6, 38) { CharacterId = "Andrew" }
				.AddText("«Five plus a couple of old fish.»")
				.AddEndQuoteId();
			if (includeStartingQuoteIdAnnotation)
				block2.BlockElements.Insert(0, new QuoteId { Id = "987654321", Start = true });
			block2.BlockElements.Add(new QuoteId { Id = includeStartingQuoteIdAnnotation ? "987654321" : null, Start = false });
			var input = new List<Block> { block1, block2 };
			if (includeFollowingParagraph)
				input.Add(new Block("p", 6, 39).AddVerse(39, "Then Jesus told them to have the crowd huddle up on the sod."));
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(includeFollowingParagraph ? 5 : 4));

			var narrator = GetStandardCharacterId("MRK", Narrator);
			Assert.That(output[0].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[0].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[1].CharacterId, Is.EqualTo(kAmbiguousCharacter));;;
			Assert.That(output[1].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[2].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[2].HasPreConfirmedCharacter, Is.False);

			Assert.That(output[3].CharacterId, Is.EqualTo("Andrew"));
			Assert.That(output[3].HasPreConfirmedCharacter, Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			if (includeFollowingParagraph)
			{
				Assert.That(output.Last().CharacterId, Is.EqualTo(narrator));
				Assert.That(output.Last().HasPreConfirmedCharacter, Is.False);
			}
		}

		[Test]
		public void Parse_PredeterminedLevel1QuoteInsideParserDetectedLevel1QuoteWithFollowingExplicitQuote_ParserDetectedAndExplicitQuotesKeptSeparate()
		{
			var narrator = GetStandardCharacterId("JER", Narrator);
			var chapterCharacter = GetStandardCharacterId("JER", BookOrChapter);
			var blockC1 = new Block("c", 1) { CharacterId = chapterCharacter };
			var blockV9 = new Block("p", 1, 9) {IsParagraphStart = true}
				.AddVerse(9, "Then the Lord reached out his hand and touched my mouth and said to " +
					"me, “I have put my words in your mouth. ");
			// The following explicit quote should have started in previous verse.
			var blockV10 = new Block("qt-s", 1, 10) {CharacterId = "God"}
				.AddVerse(10, "See, today I appoint you over nations and kingdoms to uproot and " +
					"tear down, to destroy and overthrow, to build and to plant.”");
			// The preceding explicit quote should have ended in previous verse.
			var blockV11a = new Block("p", 1, 11) {IsParagraphStart = true, CharacterId = "God"}
				.AddVerse(11, "The word of the Lord came to me: “What do you see, Jeremiah?”")
				.AddEndQuoteId();
			var blockV11b = new Block("qt-s", 1, 11) {CharacterId = "Jeremiah"}
				.AddText("“I see the branch of an almond tree,” ")
				.AddEndQuoteId();
			var blockV11c = new Block("p", 1, 11).AddText("I replied.");

			var input = new List<Block>
				{ blockC1, blockV9, blockV10, blockV11a, blockV11b, blockV11c };
			var output = new QuoteParser(ControlCharacterVerseData.Singleton, "JER", input,
					QuoteSystemForStandardAmericanEnglish)
				.Parse().ToList();
			
			Assert.That(output.All(b => !b.IsPredeterminedQuoteInterruption));

			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(9));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{9}\u00A0" +
				"Then the Lord reached out his hand and touched my mouth and said to me, "));

			Assert.That(output[++i].CharacterId, Is.EqualTo("God"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(9));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("“I have put my words in your mouth. "));
			
			Assert.That(output[++i].CharacterId, Is.EqualTo("God"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(10));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{10}\u00A0" +
				"See, today I appoint you over nations and kingdoms to uproot and " +
				"tear down, to destroy and overthrow, to build and to plant.”"));
			
			Assert.That(output[++i].CharacterId, Is.EqualTo("God"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(11));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true), Is.EqualTo("{11}\u00A0" +
				"The word of the Lord came to me: “What do you see, Jeremiah?”"));

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jeremiah"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(11));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true), Is.EqualTo("“I see the branch of an almond tree,” "));

			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(11));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("I replied."));

			Assert.That(output.Count, Is.EqualTo(++i));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_MultiBlockQuoteWithPreConfirmedCharacters_PreConfirmedCharacterAssignmentsUnchanged(
			bool includeQuoteIdAnnotations)
		{
			var chapterCharacter = GetStandardCharacterId("MAT", BookOrChapter);
			var blockC5 = new Block("c", 5) { CharacterId = chapterCharacter };
			var blockV12 = new Block("p", 5, 1) { IsParagraphStart = true }.AddVerse(1, "Then Jesus saw the people and went up to instruct them. ")
				.AddVerse(2, "He began to teach them as follows:");
			var blockV3a = new Block("qt-s", 5, 3) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Start}
				.AddVerse(3, "Blessed are the poor in spirit,");
			if (includeQuoteIdAnnotations)
				blockV3a.BlockElements.Insert(1, new QuoteId { Id = "sermon on the mount", Start = true});
			var blockV3b = new Block("q2", 5, 3) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Continuation}
				.AddText("for theirs is the celestial kingdom.");
			var blockV4a = new Block("q1", 5, 4) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Continuation}
				.AddVerse(4, "Blessed are those who mourn,");
			var blockV4b = new Block("q2", 5, 4) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Continuation}
				.AddText("for they will be given consolation.");
			var blockV11ff = new Block("p", 5, 11) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Continuation}
				.AddVerse(11, "Blessed are you.").AddVerse(12, "Rejoice.");
			var blockC6 = new Block("c", 6) { CharacterId = chapterCharacter };
			var block6V1ff = new Block("p", 6, 1) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Start}
				.AddVerse(1, "Show off your righteousness in front of folks and forfeit your heavenly prize.")
				.AddVerse(2, "Give to the poor discretely. ");
			var block6V3a = new Block("p", 6, 3) { IsParagraphStart = true, CharacterId = "Jesus", MultiBlockQuote = MultiBlockQuote.Continuation}
				.AddVerse(3, "Keep your hands mutually ignorant, ")
				.AddEndQuoteId(includeQuoteIdAnnotations ? "sermon on the mount" : null);
			var block6V3b = new Block("p", 6, 3).AddText(" he preached.");

			var input = new List<Block> { blockC5, blockV12, blockV3a, blockV3b, blockV4a, blockV4b,
				blockV11ff, blockC6, block6V1ff, block6V3a, block6V3b };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count - 2),
				"Poetry blocks not ending with periods should have joined with following poetry blocks.");
			var narrator = GetStandardCharacterId("MAT", Narrator);

			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));
			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].HasPreConfirmedCharacter, Is.True);
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[i].GetText(true), Is.EqualTo(
				"{3}\u00A0Blessed are the poor in spirit, for theirs is the celestial kingdom."));
			if (includeQuoteIdAnnotations)
			{
				var quoteAnnotation = (QuoteId)output[i].BlockElements[1];
				Assert.That(quoteAnnotation.Id, Is.EqualTo("sermon on the mount"));
				Assert.That(quoteAnnotation.Start, Is.True);
				Assert.That(quoteAnnotation.IsNarrator, Is.False);
			}

			Assert.That(output[++i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(
				"{4}\u00A0Blessed are those who mourn, for they will be given consolation."));

			Assert.That(output[++i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i + 2].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo(chapterCharacter));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i + 2].GetText(true, true)));

			Assert.That(output[++i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i + 2].GetText(true, true)));

			Assert.That(output[++i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i + 2].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i + 2].GetText(true, true)));
		}

		[TestCase(true, true, null)]
		[TestCase(true, true, "Jesus")]
		[TestCase(true, false, null)]
		[TestCase(false, false, null)]
		[TestCase(false, false, "Jesus")]
		public void Parse_MultiBlockQuoteWithPreConfirmedCharactersGoesPastExpectedReference_QuoteBrokenAndCharacterChangedToUnknown(
			bool includeStartAnnotation, bool includeEndAnnotation, string character)
		{
			var chapterCharacter = GetStandardCharacterId("MAT", BookOrChapter);
			var blockC7 = new Block("c", 7) { CharacterId = chapterCharacter };
			var blockJ24 = new Block("qt-s", 7, 24) { IsParagraphStart = true, CharacterId = character}
				.AddVerse(24, "«Everyone who obeys will be wise. ")
				.AddVerse(25, "Nothing bad will happen when it rains. ")
				.AddVerse("26-27", "But fools will end up with flattened houses because don't listen.");
			if (includeStartAnnotation)
				blockJ24.BlockElements.Insert(1, new QuoteId { Id = "end of sermon", Start = true });
			// Jesus' quote is left open.
			var block28 = new Block("p", 7, 28) { IsParagraphStart = true }
				.AddVerse(28, "The crowds were in awe of his teaching ")
				.AddVerse(29, "because he spoke with authority.");
			if (includeEndAnnotation)
				block28.AddEndQuoteId("end of sermon");

			var input = new List<Block> { blockC7, blockJ24, block28 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));

			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False,
				"This becomes false when the character is set to unexpected");
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript,
				Is.EqualTo(GetStandardCharacterId("MAT", Narrator)));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));
		}

		[Test]
		public void Parse_NeedsReviewCharacter_CharacterAndCharacterIdInScriptUnchanged()
		{
			var chapterCharacter = GetStandardCharacterId("MAT", BookOrChapter);
			var blockC7 = new Block("c", 7) { CharacterId = chapterCharacter };
			var blockJ24 = new Block("qt-s", 7, 24) { IsParagraphStart = true,
					CharacterId = kNeedsReview,
					CharacterIdInScript = "Fred" }
				.AddVerse(24, "«Everyone who obeys will be wise. ")
				.AddVerse(25, "Nothing bad will happen when it rains. ")
				.AddVerse("26-27", "But fools will end up with flattened houses because don't listen,» ")
				.AddEndQuoteId();
			var blockJ27b = new Block("p", 7, 27)
				.AddText("said Fred.");

			var input = new List<Block> { blockC7, blockJ24, blockJ27b };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));

			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[i].CharacterIdInScript, Is.EqualTo("Fred"));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript,
				Is.EqualTo(GetStandardCharacterId("MAT", Narrator)));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));
		}
		#endregion // PG-1419 tests (blocks created by quote milestones)

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtEnd()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He replied, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(7));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(output[0].CharacterIs("MRK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Isaiah was right when he prophesied about you.»"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].Delivery, Is.EqualTo("rebuking"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(7));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_UnclosedQuoteAtEnd_LastBlockSetToUnknown()
		{
			var block = new Block("p", 2, 10);
			block.BlockElements.Add(new ScriptText("But the angel said to them, «Do not be afraid!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("But the angel said to them, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(10));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Do not be afraid!"));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(10));
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtBeginning()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Go!» "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.False);

			Assert.That(output[1].GetText(false), Is.EqualTo("he said."));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.True);
		}

		[Test]
		public void Parse_OneBlockBecomesThree_TwoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»  «Make me!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!»  "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.False);

			Assert.That(output[2].GetText(false), Is.EqualTo("«Make me!»"));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.False);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("Yecu openyogi ni, "));
			Assert.That(output[0].CharacterIs("MAT", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("“Wutye ki mugati adi?” "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.False);

			Assert.That(output[2].GetText(false), Is.EqualTo("Gugamo ni, "));
			Assert.That(output[2].CharacterIs("MAT", Narrator), Is.True);

			Assert.That(output[3].GetText(false), Is.EqualTo("“Abiro, ki rec mogo matitino manok.” "));
			Assert.That(IsCharacterOfType(output[3].CharacterId, Narrator), Is.False);

			Assert.That(output[4].GetText(false), Is.EqualTo("Ociko lwak ni gubed piny i ŋom, "));
			Assert.That(output[4].CharacterIs("MAT", Narrator), Is.True);
		}

		[Test]
		public void Parse_OneBlockBecomesThree_QuoteInMiddle()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!» "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.False);

			Assert.That(output[2].GetText(false), Is.EqualTo("quietly."));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.True);
		}

		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("See Spot run. "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("See Jane see Spot run."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("See Spot run. "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("See Jane see Spot run."));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.True);
		}
		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!»"));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.False);
		}

		[Test]
		public void Parse_TwoBlocksBecomeThree_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Make me!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!» "));
			Assert.That(output[2].GetText(false), Is.EqualTo("«Make me!»"));
		}
		[Test]
		public void Parse_TwoBlocksBecomeThree_AlreadyBrokenInMiddleOfQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«Go "));
			Assert.That(output[2].GetText(false), Is.EqualTo("west!»"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo(firstLevelContinuer + "Get!»"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(true), Is.EqualTo(
				"{10}\u00A0Jesús e-sapinganga sogdebalid, "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"“Ar bemar neg-gwagwense dogdapile, obunnomaloed."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].IsParagraphStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo(
				"{11}\u00A0" + firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye."));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].IsParagraphStart, Is.True);

			Assert.That(output[3].GetText(true), Is.EqualTo(
				firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].IsParagraphStart, Is.True);

			Assert.That(output[4].GetText(true), Is.EqualTo(
				"{12}\u00A0Jesús-sapingan nadmargu, be-daed be ogwamar. {13}\u00A0Deginbali."));
			Assert.That(IsCharacterOfType(output[4].CharacterId, Narrator), Is.True);
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].IsParagraphStart, Is.True);
		}


		[Test]
		public void Parse_Continuer_NarratorAfter_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));
			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);

			Assert.That(output[2].GetText(false), Is.EqualTo("«Get!» "));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);

			Assert.That(output[3].GetText(false), Is.EqualTo("Thus he ended."));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));
			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);

			Assert.That(output[2].GetText(false), Is.EqualTo("«Get!»"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);

			Assert.That(output[3].GetText(false), Is.EqualTo("«No,» "));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.False);

			Assert.That(output[4].GetText(false), Is.EqualTo("she replied."));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].CharacterIs("LUK", Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));

			Assert.That(output[1].GetText(false), Is.EqualTo("«‹«Go!"));

			Assert.That(output[2].GetText(false), Is.EqualTo("«‹«Get!»›» "));

			Assert.That(output[3].GetText(false), Is.EqualTo("Thus he ended."));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("« ‹Go!"));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo("« ‹Get!› »"));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(false), Is.EqualTo("Thus he ended."));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo(firstLevelContinuer + "‹Get!"));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(false), Is.EqualTo("«No!»"));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[4].GetText(false), Is.EqualTo("Still in quote.›»"));
			Assert.That(output[4].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo(firstLevelContinuer + "‹Get!"));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(false), Is.EqualTo(secondLevelContinuer + "«No!»"));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[4].GetText(false), Is.EqualTo(secondLevelContinuer + "Still in quote.›»"));
			Assert.That(output[4].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[0].CharacterIs("LUK", Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("«Go!"));
			Assert.That(output[1].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo(firstLevelContinuer + " ‹Get!"));
			Assert.That(output[2].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(false), Is.EqualTo("«No!»"));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[4].GetText(false), Is.EqualTo("Still in quote.›»"));
			Assert.That(output[4].CharacterIs("LUK", Narrator), Is.False);
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«‹«Go!"));
			Assert.That(output[2].GetText(false), Is.EqualTo(thirdLevelContinuer + "Get!»›» "));
			Assert.That(output[3].GetText(false), Is.EqualTo("Thus he ended."));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«‹«Go!"));
			Assert.That(output[2].GetText(false), Is.EqualTo(thirdLevelContinuer + "Get!»›»"));
			Assert.That(output[3].GetText(false), Is.EqualTo("«No,» "));
			Assert.That(output[4].GetText(false), Is.EqualTo("she replied."));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«‹«Go!"));
			Assert.That(output[2].GetText(false), Is.EqualTo(thirdLevelContinuer + "Get!»›»"));
			Assert.That(output[3].GetText(false), Is.EqualTo("«No,» "));
			Assert.That(output[4].GetText(false), Is.EqualTo("she replied."));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("«‹She said, "));
			Assert.That(output[2].GetText(false), Is.EqualTo("«Get!» rudely.›»"));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<Go!>> he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("<<Go!>> "));
			Assert.That(output[1].GetText(false), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>> loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<Go!>> "));
			Assert.That(output[2].GetText(false), Is.EqualTo("loudly."));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>>"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<Go!>>"));
		}

		[Test]
		public void Parse_MultipleCharacters_Level1CloseStartsWithLevel2Close_Level1CloseImmediatelyFollowsLevel2Close_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<She said <Go!> and <Get!> >> and then he finished."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<She said <Go!> and <Get!> >> "));
			Assert.That(output[2].GetText(false), Is.EqualTo("and then he finished."));
			Assert.That(output[2].CharacterId, Is.EqualTo("narrator-MRK"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<She said <Go!> and "));
			Assert.That(output[2].GetText(false), Is.EqualTo("<<Continue>> "));
			Assert.That(output[3].GetText(false), Is.EqualTo("Not a quote."));
			Assert.That(output[3].CharacterId, Is.EqualTo(GetStandardCharacterId("MRK", Narrator)));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("A gye 'ushu kong le, "));
			Assert.That(output[0].CharacterId, Is.EqualTo(GetStandardCharacterId("MRK", Narrator)));

			Assert.That(output[1].GetText(false), Is.EqualTo("<<Udebid ugyang a ma de le: <Unim a de Atyagi le: <<Be bel kwu-m abee fe he itang.>> > "));
			Assert.That(output[1].CharacterId, Is.Not.EqualTo(GetStandardCharacterId("MRK", Narrator)));

			Assert.That(output[2].GetText(false), Is.EqualTo("Gbe Udebid or a ma ko Ukristi le Atyam, ki nya sha ná a, ufe ù ha fel igia ima?>> "));
			Assert.That(output[2].CharacterId, Is.Not.EqualTo(GetStandardCharacterId("MRK", Narrator)));

			Assert.That(output[3].GetText(false), Is.EqualTo("Undi ken or lè he."));
			Assert.That(output[3].CharacterId, Is.EqualTo(GetStandardCharacterId("MRK", Narrator)));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("&*Go!^~ he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("&*Go!^~ "));
			Assert.That(output[1].GetText(false), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~ loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("&*Go!^~ "));
			Assert.That(output[2].GetText(false), Is.EqualTo("loudly."));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("&*Go!^~"));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<<Go!>>> he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("<<<Go!>>> "));
			Assert.That(output[1].GetText(false), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>> loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<<Go!>>> "));
			Assert.That(output[2].GetText(false), Is.EqualTo("loudly."));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>>"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("<<<Go!>>>"));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("\"Go!\""));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("\"Go!\" he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("\"Go!\" "));
			Assert.That(output[1].GetText(false), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_StartEndSame_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\" quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(false), Is.EqualTo("\"Go!\" "));
			Assert.That(output[2].GetText(false), Is.EqualTo("quietly."));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("\"She said, 'They said, \"No way.\"'\""));
		}

		[Test]
		public void Parse_VerseAtBeginning()
		{
			var block = new Block("p", 5);
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			Assert.That(input.Count, Is.EqualTo(1));
			Assert.That(input[0].GetText(true), Is.EqualTo("{3}\u00A0He said, «Go!»"));
			Assert.That(input[0].ChapterNumber, Is.EqualTo(5));
			Assert.That(input[0].InitialStartVerseNumber, Is.EqualTo(3));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{3}\u00A0He said, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(5));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("«Go!»"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(5));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(3));
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
			Assert.That(input.Count, Is.EqualTo(1));
			Assert.That(input[0].GetText(true), Is.EqualTo(
				"{3}\u00A0Matthew tried to learn to fish, but Peter was upset. " +
				"{4}\u00A0He said, «Go back to your tax booth!»"));
			Assert.That(input[0].ChapterNumber, Is.EqualTo(5));
			Assert.That(input[0].InitialStartVerseNumber, Is.EqualTo(3));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo(
				"{3}\u00A0Matthew tried to learn to fish, but Peter was upset. " +
				"{4}\u00A0He said, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(5));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("«Go back to your tax booth!»"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(5));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(4));
		}

		[Test]
		public void Parse_VerseBeforeQuote()
		{
			var block = new Block("p", 6, 2);
			block.BlockElements.Add(new ScriptText("He said, "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block };
			Assert.That(input.Count, Is.EqualTo(1));
			Assert.That(input[0].GetText(true), Is.EqualTo("He said, {3}\u00A0«Go!»"));
			Assert.That(input[0].ChapterNumber, Is.EqualTo(6));
			Assert.That(input[0].InitialStartVerseNumber, Is.EqualTo(2));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(6));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(2));

			Assert.That(output[1].GetText(true), Is.EqualTo("{3}\u00A0«Go!»"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(6));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(3));
		}

		[Test]
		public void Parse_VerseAfterQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("he said."));
			var input = new List<Block> { block };
			Assert.That(input.Count, Is.EqualTo(1));
			Assert.That(input[0].GetText(true), Is.EqualTo("«Go!» {3}\u00A0he said."));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("«Go!» "));
			Assert.That(output[1].GetText(true), Is.EqualTo("{3}\u00A0he said."));
		}

		[Test]
		public void Parse_VerseWithinQuote()
		{
			var block = new Block("p", 6, 3);
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block };
			Assert.That(input.Count, Is.EqualTo(1));
			Assert.That(input[0].GetText(true), Is.EqualTo("He said, «Go {4}\u00A0west!»"));
			Assert.That(input[0].ChapterNumber, Is.EqualTo(6));
			Assert.That(input[0].InitialStartVerseNumber, Is.EqualTo(3));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(6));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("«Go {4}\u00A0west!»"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(6));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(3));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{1}\u00A0abc {2}\u00A0def "));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(1));

			Assert.That(output[1].GetText(true), Is.EqualTo("«ghi» "));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(2));

			Assert.That(output[2].GetText(true), Is.EqualTo("{3}\u00A0jkl "));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(3));
		}

		[Test]
		public void Parse_SpaceStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("«Go!»"));
		}
		[Test]
		public void Parse_PunctuationStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go»!! he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("«Go»!! "));
			Assert.That(output[1].GetText(true), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_PunctuationStaysWithPriorBlock_AtBlockEnd()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("«Go»!"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{23}\u00A0Ɔ ya maa neencla le: "));
			Assert.That(output[1].GetText(true), Is.EqualTo("«Nyinyia ‑anɩ saa ɔ ya 'lɛ nʋ bha?» "));
			Assert.That(output[2].GetText(true), Is.EqualTo($"{punctuation}Ma maa ka..."));
		}

		[Test]
		public void Parse_UsingDifferentQuoteMarks()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("“Go!” he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("“Go!” "));
			Assert.That(output[1].GetText(true), Is.EqualTo("he said."));
		}

		[Test]
		public void Parse_Level2_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘Get lost.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("“She said, ‘Get lost.’”"));
		}

		[Test]
		public void Parse_Level3_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way.”’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("“She said, ‘They said, “No way.”’”"));
		}

		[Test]
		public void Parse_Level3_ContinuesInside_BreakOnFirstLevelQuoteOnly()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("“She said, ‘They said, “No way!” rudely.’”"));
		}

		[Test]
		public void Parse_Level3_ContinuesOutside_BreakOnFirstLevelQuoteOnly()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely,’” politely."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("“She said, ‘They said, “No way!” rudely,’” "));
			Assert.That(output[2].GetText(true), Is.EqualTo("politely."));
		}

		/// <summary>
		/// PG-578 (Text of 1 Kings 1:11-15 from The World Bible)
		/// </summary>
		[Test]
		public void Parse_Level3_WordInSecondLevelContainsApostropheWhichIsSecondLevelCloser_ApostropheDoesNotEndSecondLevelQuote()
		{
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "1KI", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{11}\u00A0Then Nathan spoke to Bathsheba the mother of Solomon, saying, "));
			Assert.That(output[1].GetText(true),
				Is.EqualTo("“Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? " +
				"{12}\u00A0Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. " +
				"{13}\u00A0Go in to king David, and tell him, ‘Didn’t you, my lord, king, swear to your servant, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ " +
				"{14}\u00A0Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”"));
		}

		/// <summary>
		/// PG-578
		/// </summary>
		[Test]
		public void Parse_Level3_WordInSecondLevelContainsPluralPossessiveApostropheWhichIsSecondLevelCloser_ApostropheDoesNotEndSecondLevelQuote()
		{
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "1KI", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{11}\u00A0Then Nathan spoke to Bathsheba the mother of Solomon, saying, "));
			Assert.That(output[1].GetText(true),
				Is.EqualTo("“Haven’t you heard that Adonijah the son of Haggith reigns, and David our lord doesn’t know it? " +
				"{12}\u00A0Now therefore come, please let me give you counsel, that you may save your own life, and your son Solomon’s life. " +
				"{13}\u00A0Go in to king David, and tell him, ‘Did you not swear in your kids’ hearing, saying, “Assuredly Solomon your son shall reign after me, and he shall sit on my throne?” Why then does Adonijah reign?’ " +
				"{14}\u00A0Behold, while you are still talking there with the king, I will also come in after you and confirm your words.”"));
		}

		/// <summary>
		/// PG-751
		/// </summary>
		[Test]
		public void Parse_ParagraphContainsFirstLevelQuoteWithNestedSecondLevelQuoteFolloweByAnotherFirstLevelQuote_SecondLevelCloserNotTreatedAsAnApostrophe()
		{
			var block = new Block("p", 13) { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("14"));
			block.BlockElements.Add(new ScriptText("“Oona gwine see ‘De Horrible Bad Ting wa mek God place empty’ da stanop een de place weh e ain oughta dey.” (Leh oona wa da read ondastan wa dis mean.) “Wen dat time come, de people een Judea mus ron way quick ta de hill country.”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{14}\u00A0“Oona gwine see ‘De Horrible Bad Ting wa mek God place empty’ da stanop een de place weh e ain oughta dey.” "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[1].GetText(true), Is.EqualTo("(Leh oona wa da read ondastan wa dis mean.) "));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);

			Assert.That(output[2].GetText(true), Is.EqualTo("“Wen dat time come, de people een Judea mus ron way quick ta de hill country.”"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[1].GetText(true), Is.EqualTo("“She said, ‘They said, “No way!” quite rudely.’”"));
			Assert.That(output[2].GetText(true), Is.EqualTo("He continued, "));
			Assert.That(output[3].GetText(true), Is.EqualTo("“The end.”"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JER", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(true),
				Is.EqualTo("{1}\u00A0In the beginning, this word came from Yahweh, saying, " +
				"{2}\u00A0Yahweh says to me: "));
			Assert.That(output[0].CharacterIs("JER", Narrator), Is.True);

			Assert.That(output[1].GetText(true),
				Is.EqualTo("“Make bonds and bars, and put them on your neck. " +
				"{3}\u00A0Then send them to the kings, by the hand of the messengers who come to Jerusalem. " +
				"{4}\u00A0Give them a command, saying, ‘Yahweh of Armies says, “You shall tell your masters: " +
				"{5}\u00A0‘I have made the earth, by my great power. I give it to whom it seems right to me. " +
				"{6}\u00A0Now I have given all these lands to Nebuchadnezzar, my servant. I have also given the animals to him. " +
				"{7}\u00A0All the nations will serve him. Then many nations will make him their servant."));
			Assert.That(output[1].CharacterId, Is.EqualTo("God"));

			Assert.That(output[2].GetText(true),
				Is.EqualTo("{8}\u00A0“‘“‘I will punish the nation which will not serve Nebuchadnezzar,’ says Yahweh, ‘until I have consumed them. " +
				"{9}\u00A0Don’t listen to your prophets, who speak, saying, “You shall not serve the king of Babylon;” " +
				"{10}\u00A0for they prophesy a lie to you, so that I would drive you out. " +
				"{11}\u00A0But the nation under Babylon will remain in their land,’ says Yahweh; ‘and they will dwell in it.’”’”"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jeremiah"));

			Assert.That(output[3].GetText(true), Is.EqualTo("{12}\u00A0I spoke to Zedekiah all these words, saying, "));
			Assert.That(output[3].CharacterIs("JER", Narrator), Is.True);

			Assert.That(output[4].GetText(true), Is.EqualTo("“Bring your necks under the yoke of Babylon, and live.”"));
			Assert.That(output[4].CharacterId, Is.EqualTo("Jeremiah"));
		}

		[Test]
		public void Parse_TitleIntrosChaptersAndExtraBiblicalMaterial_OnlyVerseTextGetsParsedForQuotes()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var titleBlock = new Block("mt");
			titleBlock.BlockElements.Add(new ScriptText("Gospel of Mark"));
			titleBlock.SetStandardCharacter("MRK", BookOrChapter);
			var introBlock1 = new Block("is");
			introBlock1.BlockElements.Add(new ScriptText("All about Mark"));
			var introBlock2 = new Block("ip");
			introBlock1.SetStandardCharacter("MRK", Intro);
			introBlock2.BlockElements.Add(new ScriptText("Some people say, “Mark is way to short,” but I disagree."));
			introBlock2.SetStandardCharacter("MRK", Intro);
			var chapterBlock = new Block("c");
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			chapterBlock.SetStandardCharacter("MRK", BookOrChapter);
			var sectionHeadBlock = new Block("s");
			sectionHeadBlock.BlockElements.Add(new ScriptText("John tells everyone: “The Kingdom of Heaven is at hand”"));
			sectionHeadBlock.SetStandardCharacter("MRK", ExtraBiblical);
			var paraBlock = new Block("p");
			paraBlock.BlockElements.Add(new Verse("1"));
			paraBlock.BlockElements.Add(new ScriptText("Jesus said, “Is that John?”"));
			var input = new List<Block> { titleBlock, introBlock1, introBlock2, chapterBlock, sectionHeadBlock, paraBlock };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(7));

			Assert.That(output[0].GetText(true), Is.EqualTo("Gospel of Mark"));
			Assert.That(output[0].CharacterIs("MRK", BookOrChapter), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("All about Mark"));
			Assert.That(output[1].CharacterIs("MRK", Intro), Is.True);

			Assert.That(output[2].GetText(true), Is.EqualTo("Some people say, “Mark is way to short,” but I disagree."));
			Assert.That(output[2].CharacterIs("MRK", Intro), Is.True);

			Assert.That(output[3].GetText(true), Is.EqualTo("Chapter 1"));
			Assert.That(output[3].CharacterIs("MRK", BookOrChapter), Is.True);

			Assert.That(output[4].GetText(true), Is.EqualTo("John tells everyone: “The Kingdom of Heaven is at hand”"));
			Assert.That(output[4].CharacterIs("MRK", ExtraBiblical), Is.True);

			Assert.That(output[5].GetText(true), Is.EqualTo("{1}\u00A0Jesus said, "));
			Assert.That(output[5].CharacterIs("MRK", Narrator), Is.True);

			Assert.That(output[6].GetText(true), Is.EqualTo("“Is that John?”"));
			Assert.That(output[6].CharacterId, Is.EqualTo(kUnexpectedCharacter));
		}

		[Test]
		public void Parse_IsParagraphStart()
		{
			var chapterBlock = new Block("c") { IsParagraphStart = true };
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { chapterBlock, block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Chapter 1"));
			Assert.That(output[1].GetText(true), Is.EqualTo("He said, "));
			Assert.That(output[2].GetText(true), Is.EqualTo("«Go»!"));

			Assert.That(output[0].IsParagraphStart, Is.True);
			Assert.That(output[1].IsParagraphStart, Is.True);
			Assert.That(output[2].IsParagraphStart, Is.False);
		}

		[Test]
		public void Parse_IsParagraphStart_BlockStartsWithVerse()
		{
			var block = new Block("q1") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));

			Assert.That(output[0].GetText(true), Is.EqualTo("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			Assert.That(output[0].IsParagraphStart, Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun, ŋa tsanaftá hgani ka Emanuwel,» "));
			Assert.That(output[0].StyleTag, Is.EqualTo("q1"));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			Assert.That(output[1].IsParagraphStart, Is.False);
			Assert.That(output[1].StyleTag, Is.EqualTo("q1"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			int i = 0;
			Assert.That(output[i].GetText(true),
				Is.EqualTo("{14}\u00A0Geb degi, Bab-Dummad sogdebalid:"));
			Assert.That(output[i].CharacterId, Is.EqualTo("narrator-GEN"));

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				openingQuoteMark + "Nibneggi gwallumar nagu, adi, neg-mutikid, neg-ibgined-ebo bachikii guegar."));
			Assert.That(output[i].CharacterId, Is.EqualTo("God"));
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				"Adi, ibagan-nagumaid, yolamar-nagumaid, birgamar-nagumaid magar daklegegar."));
			Assert.That(output[i].CharacterId, Is.EqualTo("God"));

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				"{15}\u00A0Degi, gwallumar-niba-naid napneg-mee saegar." + closingQuoteMark));
			Assert.That(output[i].CharacterId, Is.EqualTo("God"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("Deyob gunonikid."));
			Assert.That(output[i].CharacterId, Is.EqualTo("narrator-GEN"));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-MAT"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Peter (Simon)"));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-MAT"));

			Assert.That(output[1].CharacterId, Is.EqualTo("Peter (Simon)"));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(output[1].InitialEndVerseNumber, Is.EqualTo(4));

			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(output[2].InitialEndVerseNumber, Is.EqualTo(6));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-MAT"));

			Assert.That(output[1].CharacterId, Is.EqualTo("Peter (Simon)"));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(output[1].InitialEndVerseNumber, Is.EqualTo(4));

			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(5));
			Assert.That(output[2].InitialEndVerseNumber, Is.EqualTo(0));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-JHN"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Ambiguous"));
			Assert.That(output[2].CharacterId, Is.EqualTo("narrator-JHN"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Ambiguous"));
			Assert.That(output[4].CharacterId, Is.EqualTo("Ambiguous"));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-ACT"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Peter (Simon)"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Peter (Simon)"));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{23}\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun, ŋa tsanaftá hgani ka Emanuwel,» "));
			Assert.That(output[0].StyleTag, Is.EqualTo("p"));

			Assert.That(output[1].GetText(true), Is.EqualTo("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(6));
			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-GEN"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Last in Chapter"));
			Assert.That(output[2].CharacterId, Is.EqualTo("narrator-GEN"));
			Assert.That(output[4].CharacterId, Is.EqualTo("narrator-GEN"));
			Assert.That(output[5].CharacterId, Is.EqualTo("First in Chapter"));
		}

		[Test]
		public void Parse_SpaceAfterVerse_NoEmptyBlock()
		{
			var block1 = new Block("p", 3, 12) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«pe, kadi ki acel.» "));
			block1.BlockElements.Add(new Verse("13"));
			block1.BlockElements.Add(new ScriptText(" «Guŋamo doggi calo lyel ma twolo,»"));
			var input = new List<Block> { block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("«pe, kadi ki acel.» "));
			Assert.That(output[1].GetText(true), Is.EqualTo("{13}\u00A0«Guŋamo doggi calo lyel ma twolo,»"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));

			Assert.That(output[1].GetText(false), Is.EqualTo("« ‹Go!"));

			Assert.That(output[2].GetText(false), Is.EqualTo(openingPunctuation + "« ‹Get!› »)"));

			Assert.That(output[3].GetText(false), Is.EqualTo("Thus he ended."));
			Assert.That(output[3].CharacterIs("LUK", Narrator), Is.True);
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
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“",
				1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT",
				input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(true), Is.EqualTo("“Na njə́a mənə, wuntə digəlyi dzəgə kə́lə hwi, a njə dzəgə ye zəgwi rə kə za, a mbəlyi dzəgə ka zəgwi tsa Immanuʼel.” "));
			Assert.That(output[1].GetText(true), Is.EqualTo(openingPunctuation + "“Immanuʼel” "));
			Assert.That(output[2].GetText(true), Is.EqualTo("tsa ná, njə́ nee, "));
			Assert.That(output[3].GetText(true), Is.EqualTo("“tá myi Hyalatəmwə,” "));
			Assert.That(output[4].GetText(true), Is.EqualTo("əkwə.)"));
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
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“",
				1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT",
				input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(6));

			Assert.That(output[1].GetText(true), Is.EqualTo("“Look, here is water. Why shouldn't I be baptized?” "));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(output[1].StartsAtVerseStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo("[{37}\u00A0“If you believe with all your heart, you may,” "));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[2].StartsAtVerseStart, Is.True);
			Assert.That(output[2].IsParagraphStart, Is.False);

			Assert.That(output[3].GetText(true), Is.EqualTo("replied Phillip. And he answered and said, "));
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[3].StartsAtVerseStart, Is.False);

			Assert.That(output[4].GetText(true), Is.EqualTo("“I believe that Jesus Christ is the Son of God.”] "));
			Assert.That(output[4].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[4].StartsAtVerseStart, Is.False);
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
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal),
				null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT",
				input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(7));

			Assert.That(output[1].GetText(true), Is.EqualTo("“Look, here is water. Why shouldn't I be baptized?” "));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(output[1].StartsAtVerseStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo("[{37}\u00A0Phillip replied, "));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[2].StartsAtVerseStart, Is.True);
			Assert.That(output[2].IsParagraphStart, Is.False);

			Assert.That(output[3].GetText(true), Is.EqualTo("“If you believe with all your heart, you may.” "));
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[3].StartsAtVerseStart, Is.False);
			Assert.That(output[3].IsParagraphStart, Is.False);

			Assert.That(output[4].GetText(true), Is.EqualTo("The eunuch answered, "));
			Assert.That(output[4].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[4].StartsAtVerseStart, Is.False);

			Assert.That(output[5].GetText(true), Is.EqualTo("“I believe that Jesus Christ is the Son of God.”] "));
			Assert.That(output[5].InitialStartVerseNumber, Is.EqualTo(37));
			Assert.That(output[5].StartsAtVerseStart, Is.False);
		}

		[Test]
		public void Parse_SquareBracketBetweenVerseNumberAndWj_VerseNumberDoesNotGetOrphanedInIllegalBlock()
		{
			var input = new List<Block>
			{
				new Block("p", 23, 14) { IsParagraphStart = true }.AddVerse(14, "["),
				new Block("wj", 23, 14) { CharacterId = "Jesus" }.AddText("अ़चाअ़त्‍मादा।]"),
				new Block("wj", 23, 15) { CharacterId = "Jesus", IsParagraphStart = true }.AddVerse(15, "अ़चाअ़त्‍मादा।")
			};
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“",
					1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT",
				input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].GetText(true).TrimEnd(), Is.EqualTo("{14}\u00A0[अ़चाअ़त्‍मादा।]"));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(14));

			Assert.That(output[1].GetText(true).TrimEnd(), Is.EqualTo("{15}\u00A0अ़चाअ़त्‍मादा।"));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(15));
		}

		[TestCase("(")]
		[TestCase("[")]
		[TestCase("{")]
		[TestCase("⦅")]
		[TestCase("¡")]
		[TestCase("¿")]
		[TestCase("[(")]
		[TestCase("¿¡")]
		public void Parse_QuoteStartsWithLeadingPunctuation_LeadingPunctuationIncludedInQuote(string openingPunctuation)
		{
			// PG-644 (Kaqchikel - cak)
			var block1 = new Block("p", 1, 25) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("Pero ri Jesús xuchꞌolij ri itziel espíritu: " + openingPunctuation + "Man chic cachꞌoꞌ y catiel-el riqꞌuin ri ache!"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Pero ri Jesús xuchꞌolij ri itziel espíritu: "));
			Assert.That(output[1].GetText(true), Is.EqualTo(openingPunctuation + "Man chic cachꞌoꞌ y catiel-el riqꞌuin ri ache!"));
		}

		[Test]
		public void Parse_PeriodFollowingClosingQuoteInLastBlock_PeriodGoesWithQuote()
		{
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("“Na njə́a mənə, wuntə digəlyi dzəgə”."));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));
			Assert.That(output[0].GetText(true), Is.EqualTo("“Na njə́a mənə, wuntə digəlyi dzəgə”."));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].StyleTag, Is.EqualTo("q1"));
			Assert.That(output[0].GetText(true), Is.EqualTo("{23}\u00A0«Nen, nyako mo ma peya oŋeyo laco biyac, binywalo latin ma laco» "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].StyleTag, Is.EqualTo("q1"));
			Assert.That(output[1].GetText(true), Is.EqualTo("«Gibicako nyiŋe Emmanuel»"));
			Assert.That(output[1].CharacterId, Is.EqualTo(kAmbiguousCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_MultiBlockQuoteAcrossSectionHead_ClearMultiBlockBeforeSectionHeaderAndResetAfter()
		{
			var block1 = new Block("p", 5, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«Wun bene wubed "));
			var block2 = new Block("s1", 5, 16) { IsParagraphStart = true, CharacterId = GetStandardCharacterId("MAT", ExtraBiblical) };
			block2.BlockElements.Add(new ScriptText("Lok ma Yecu opwonyo i kom cik"));
			var block3 = new Block("p", 5, 17) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("17"));
			block3.BlockElements.Add(new ScriptText("«Pe wutam ni an. "));
			var block4 = new Block("q1", 5, 17) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Ada awaco botwu ni.»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(IsCharacterOfType(output[1].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
		}

		[Test]
		public void Parse_MultiBlockQuoteAcrossSectionHeadWithoutContinuer_ClearMultiBlockBeforeSectionHeaderAndResetAfter()
		{
			var block1 = new Block("p", 5, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«Wun bene wubed "));
			var block2 = new Block("s1", 5, 16) { IsParagraphStart = true, CharacterId = GetStandardCharacterId("MAT", ExtraBiblical) };
			block2.BlockElements.Add(new ScriptText("Lok ma Yecu opwonyo i kom cik"));
			var block3 = new Block("p", 5, 17) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("17"));
			block3.BlockElements.Add(new ScriptText("Pe wutam ni an. "));
			var block4 = new Block("q1", 5, 17) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Ada awaco botwu ni»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(IsCharacterOfType(output[1].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(false), Is.EqualTo("—timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("{6}\u00A0Jesús le dijo: "));
			Assert.That(output[0].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. " +
				"{7}\u00A0Si me hubierais conocido, también hubierais conocido a mi Padre; " +
				"desde ahora le conocéis y le habéis visto."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].Delivery, Is.Null);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[2].GetText(true), Is.EqualTo("{8}\u00A0Felipe le dijo: "));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(8));

			Assert.That(output[3].GetText(true), Is.EqualTo("Señor, muéstranos al Padre, y nos basta."));
			Assert.That(output[3].CharacterId, Is.EqualTo("Philip"));
			Assert.That(output[3].Delivery, Is.Null);
			Assert.That(output[3].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(8));
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
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(true), Is.EqualTo("{6}\u00A0Jesús le dijo: "));
			Assert.That(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[1].GetText(true), Is.EqualTo(quoteByJesusInV6));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].Delivery));, Is.Empty);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[2].GetText(true, Is.EqualTo("{7}\u00A0Si me hubierais conocido, también hubierais conocido a mi Padre); desde ahora le conocéis y le habéis visto."));
			Assert.That(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[2].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(7));

			Assert.That(output[3].GetText(true), Is.EqualTo("{8}\u00A0Felipe le dijo: "));
			Assert.That(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[3].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(8));

			Assert.That(output[4].GetText(true), Is.EqualTo("Señor, muéstranos al Padre, y nos basta."));
			Assert.That(output[4].CharacterId, Is.EqualTo("Philip"));
			Assert.That(output[4].Delivery, Is.Empty);
			Assert.That(output[4].ChapterNumber, Is.EqualTo(14));
			Assert.That(output[4].InitialStartVerseNumber, Is.EqualTo(8));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);

			Assert.That(output[1].GetText(false), Is.EqualTo("—timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));

			Assert.That(output[0].GetText(false), Is.EqualTo("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi."));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, "));
			Assert.That(output[0].CharacterIsUnclear, Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(1));

			Assert.That(output[1].GetText(false), Is.EqualTo("said the frog."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(1));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:"));
			Assert.That(output[0].CharacterId, Is.EqualTo("Good Priest"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(5));

			Assert.That(output[1].GetText(true), Is.EqualTo("{6}\u00A0Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”"));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[2].GetText(true), Is.EqualTo("Yus timiayi. Tu aarmawaitai, —tusar aimkarmiayi."));
			Assert.That(output[2].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[2].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(6));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Quote:"));
			Assert.That(output[0].CharacterId, Is.EqualTo("Good Priest"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(5));

			Assert.That(output[1].GetText(true), Is.EqualTo("{6}\u00A0Poetry stuff. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Good Priest"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[2].GetText(true), Is.EqualTo("More poetry stuff "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Good Priest"));
			Assert.That(output[2].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(6));

			Assert.That(output[3].GetText(true), Is.EqualTo("—back to narrator."));
			Assert.That(output[3].CharacterIs("MAT", Narrator), Is.True);
			Assert.That(output[3].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(6));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{17}\u00A0Quia joˈ tso Jesús nda̱a̱na:"));
			Assert.That(output[0].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(true), Is.EqualTo("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[2].GetText(true), Is.EqualTo("{18}\u00A0Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê."));
			Assert.That(output[2].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(18));
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—Nintimrataram splintaram." + followingPunctuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—Nintimrataram splintaram”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—Nintimrataram splintaram" + spuriousOpener));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
		}

		[TestCase("—“", true, true)]
		[TestCase("—”", true, false)]
		[TestCase("—", true, true)]
		[TestCase("—", true, false)]
		[TestCase("— ", true, true)]
		[TestCase("—“", false, true)]
		[TestCase("—”", false, true)]
		[TestCase("—", false, true)]
		[TestCase("—", false, false)]
		[TestCase("— ", false, true)]
		[TestCase("— ", false, false)]
		public void Parse_DialogQuoteEndingWithSpuriousQuotationPunctuationFollowingCloserAndNoFollowingText_TrailingPunctuationIncludedInQuoteBlock(
			string trailingPunctuation, bool includeSecondNormalLevel, bool isParaStart)
		{
			var block1 = new Block("p", 6, 48) { IsParagraphStart = isParaStart };
			block1.BlockElements.Add(new ScriptText("Jesus said —Nintimrataram splintaram " + trailingPunctuation));
			var input = new List<Block> { block1 };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			if (includeSecondNormalLevel)
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "‘", 2, QuotationMarkingSystemType.Normal));
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"—Nintimrataram splintaram " + trailingPunctuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"—Nintimrataram splintaram " + trailingPunctuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("Some more text"));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram splintaram.”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” is my favorite word."));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite word. Do you like it?"));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” is my favorite word. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("Do you like it?"));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite word, for thus saith Isaiah "));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[3].GetText(true), Is.EqualTo("“Don't even go there!”"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” is my favorite word, for thus saith Isaiah "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("“Don't even go there!”"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” is my favorite word, for thus saith Isaiah “Don't even go there!”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("—“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite word, for thus saith Isaiah? "));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[3].GetText(true), Is.EqualTo("“Don't even go there!”"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"“Nintimrataram splintaram" + endingPunctuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite word."));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite "));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[3].GetText(true), Is.EqualTo("“word" + endingPunctuation));
			Assert.That(output[3].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo(
				"is my favorite word, for thus saith Isaiah" + punctuationToIntroduceIsaiah));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[3].GetText(true), Is.EqualTo("Don't even go there!"));
			Assert.That(output[3].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” "));
			Assert.That(output[1].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[2].GetText(true), Is.EqualTo("is my favorite word, for thus saith Isaiah,"));
			Assert.That(output[2].CharacterId, Is.EqualTo(CharacterVerseData.UnknownCharacter));

			Assert.That(output[3].GetText(true), Is.EqualTo("Don't even go there!"));
			Assert.That(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” is my favorite word, for thus saith Isaiah."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("Don't even go there!"));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram” is my favorite word, for thus saith Isaiah, Don't even go there!"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("I didn't."));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0—“Nintimrataram. Jesús timiayi.” "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("Wikia tuke pujutan sukartin asan. Comete un pedazo de flan. "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0—“Nintimrataram” is a word about which John said “I never use that word. It's lame, dude!”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0Remember that John said, “He must increase. I must decrease!”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true),
				Is.EqualTo("{49}\u00A0Remember that John said, “He must increase. I must decrease" + endingPunctuation + " "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("Following text spoken by narrator."));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0Remember that John said, “He must increase. I must decrease,” and more dialogue."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			block2.BlockElements.Add(new ScriptText("Remember that John said, “He must increase. I must decrease" +
				endingPunctuation + " Text following nested quote."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"{49}\u00A0Remember that John said, “He must increase. I must decrease" +
				endingPunctuation + " Text following nested quote."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{28}\u00A0Ar ome binsaed: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed.” "));
			Assert.That(output[1].CharacterId, Is.EqualTo("woman, bleeding for twelve years"));
			Assert.That(output[1].Delivery, Is.EqualTo("thinking"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].IsParagraphStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo("{29}\u00A0Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa."));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[2].IsParagraphStart, Is.False);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("{17}\u00A0Ar ome binsaed: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("God"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].IsParagraphStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo("{18}\u00A0“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa.”"));
			Assert.That(output[2].CharacterId, Is.EqualTo("God"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].IsParagraphStart, Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(true), Is.EqualTo("{10}\u00A0Jesús e-sapinganga sogdebalid: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Ar bemar neg-gwagwense dogdapile, obunnomaloed."));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].IsParagraphStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo(
				"{11}\u00A0" + firstLevelContinuer + "Ar ibi-neggwebur nuedgi Bab-Dummad-dulamar-guega abingalessurmarye."));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].IsParagraphStart, Is.True);

			Assert.That(output[3].GetText(true), Is.EqualTo(
				firstLevelContinuer + "Napira an bemarga soged, Bab-Dummad-igar-nabirogoed-ibagi, odurdaklemaloed.”"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].IsParagraphStart, Is.True);

			Assert.That(output[4].GetText(true), Is.EqualTo("{12}\u00A0Jesús-sapingan nadmargu, be-daed be ogwamar. {13}\u00A0Deginbali."));
			Assert.That(IsCharacterOfType(output[4].CharacterId, Narrator), Is.True);
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].IsParagraphStart, Is.True);
		}
		#endregion

		[Ignore("We don't know what the results should be.")]
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("{17}\u00A0Ar ome binsaed: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].IsParagraphStart, Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("“Unnila an e-mordukubi wis-ebusale, an yog-nuguar naoed. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("God"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].IsParagraphStart, Is.False);

			Assert.That(output[2].GetText(true), Is.EqualTo("{18}\u00A0“Ome Jesús-e-morduku-ebusgu, yog-nuguar naded. Geb ome na magasaila itosad, ede nugusa."));
			Assert.That(output[2].CharacterId, Is.EqualTo("God"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].IsParagraphStart, Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Then he said: "));
			Assert.That(output[0].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("This is a poem about a fly"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("who wished he didn't never have to die. Crud!"));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[TestCase("-")]
		[TestCase("\u2014")]
		[TestCase("\u2015")]
		public void Parse_DialogueDashBetweenTwoNumbers_KeptAsSingleBlock(string dialogueDash)
		{
			// Note: in the original data where I encountered this, the marker was a \d.
			// The USX Parser now handles that as a special case (except in Psalms) so that
			// the character is set and it is not even treated as a Scripture style. But
			// for this test, I'm keeping it as a "d" just to preserve the historical origin.
			var block = new Block("d", 40);
			block.BlockElements.Add(new ScriptText($"BAGIAN KEDUA PASAL 40{dialogueDash}55"));
			block.IsParagraphStart = true;
			var input = new List<Block> { block };
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueDash, dialogueDash);
			var outputBlock = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input, quoteSystem).Parse().Single();
			
			Assert.That(outputBlock.GetText(true), Is.EqualTo(block.GetText(true)));
			Assert.That(outputBlock.CharacterIsStandard, Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0" + firstLevelContinuer + "Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true), Is.EqualTo("—Jesús timiayi."));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0" + firstLevelContinuer + " Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true), Is.EqualTo("—Jesús timiayi."));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo("“Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true), Is.EqualTo("—Jesús timiayi."));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0" + levelOneContinuer + "Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true), Is.EqualTo("~Jesús timiayi."));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[1].GetText(true), Is.EqualTo("—Jesús timiayi."));
			Assert.That(output[1].CharacterIs("JHN", Narrator), Is.True);

			Assert.That(output[2].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram, —Jesús timiayi."));
			Assert.That(output[2].CharacterId, Is.EqualTo(kUnexpectedCharacter));
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
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram. Jesús timiayi."));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("Wikia tuke pujutan sukartin asan; "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].IsParagraphStart, Is.False);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram. "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[3].GetText(true), Is.EqualTo("Jesús timiayi."));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("{48}\u00A0Wikia tuke pujutan sukartin asan; "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("Wikia tuke pujutan sukartin asan; "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
		}

		[Test]
		public void Parse_DialogueQuoteIntroducedByColonAtEndOfVerse_QuoteAssignedToCharacter()
		{
			var block1 = new Block("p", 6, 47);
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			block1.AddVerse(48, "Wikia tuke pujutan sukartin asan. ");
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("{48}\u00A0Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("Jesus said: "));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("Wikia tuke pujutan sukartin asan; "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].IsParagraphStart, Is.True);

			Assert.That(output[2].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram. "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[3].GetText(true), Is.EqualTo("Jesús timiayi."));
			Assert.That(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo(
				"{49}\u00A0" + continuer + "Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true), Is.EqualTo(
				"{50}\u00A0" + continuer + "Antsu yurumkan, "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(true), Is.EqualTo("—Jesús timiayi."));
			Assert.That(output[3].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(true), Is.EqualTo("{50}\u00A0“Antsu yurumkan,” "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[3].GetText(true), Is.EqualTo("Jesús timiayi."));
			Assert.That(output[3].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan, This should be a continuation of dialogue. "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[++i].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram, "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[++i].GetText(true), Is.EqualTo("{50}\u00A0“Antsu yurumkan,” "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[++i].GetText(true), Is.EqualTo("Jesús timiayi."));
			Assert.That(output[i].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0" + continuer + "Nintimrataram, "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(true),
				Is.EqualTo("{50}\u00A0" + continuer + "Antsu yurumkan,” Jesús timiayi."));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(true), Is.EqualTo("—Wikia tuke pujutan sukartin asan. "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[1].GetText(true), Is.EqualTo("{49}\u00A0“Nintimrataram,” "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].GetText(true), Is.EqualTo("Jesús timiayi."));
			Assert.That(output[2].CharacterIs("JHN", Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("-Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(false), Is.EqualTo("-timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("--Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(false), Is.EqualTo("--timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(false), Is.EqualTo("–timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("—¿Exota naexana pexuyo, po pexuyo judiomonae itorobiya pia pepa peewatsinchi exanaeinchi poxonae pinyo tsane? Paxan payaputan xua bapon naexana xote tsipei bapon pia opiteito tatsi panaitomatsiya punaenaponan pata nacua werena, po nacuatha ichaxota pocotsiwa xometo weecoina. Papatan xua pata wʉnae jainchiwa tsane barapo pexuyo"));
			Assert.That(output[0].CharacterId, Is.EqualTo("magi"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(2));

			Assert.That(output[1].GetText(false), Is.EqualTo(", jei."));
			Assert.That(output[1].CharacterIs("MAT", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(2));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(2));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("—Jiwi ba jopa bewa."));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].ChapterNumber, Is.EqualTo(9));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(15));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("“Jiwi ba jopa. Mataʉtano bocoto”"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(9));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(16));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(false), Is.EqualTo(", jei Jesús."));
			Assert.That(output[2].CharacterIs("MAT", Narrator), Is.True);
			Assert.That(output[2].ChapterNumber, Is.EqualTo(9));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_VerseBridge_CharacterInEachVerse_ResultIsAmbiguous()
		{
			var block1 = new Block("p", 9, 27, 28);
			block1.BlockElements.Add(new Verse("27-28"));
			block1.BlockElements.Add(new ScriptText("«Quote.»"));
			var input = new List<Block> { block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));
			Assert.That(output[0].CharacterId, Is.EqualTo(kAmbiguousCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 2, 7).Select(cv => cv.Character).Single(), Is.EqualTo("teachers of religious law/Pharisees"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 2, 8).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 16).Select(cv => cv.Character).Single(), Is.EqualTo("ruler, a certain=man, rich young"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 17).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("ruler, a certain=man, rich young"));

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 23).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 24).Select(cv => cv.Character).Single(), Is.EqualTo("blind man"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 8, 25).Any(), Is.False);

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[2].CharacterId, Is.EqualTo(kUnexpectedCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 17).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("ruler, a certain=man, rich young"));

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 8).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 9).Any(), Is.False);

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 18).Select(cv => cv.Character), Does.Contain("ruler, a certain=man, rich young"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMATbookNum, 19, 19).Any(), Is.False);

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 16).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 16).Select(cv => cv.Delivery).Single(), Is.EqualTo("giving orders"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 17).Select(cv => cv.Character).Single(), Is.EqualTo("Jesus"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 16, 17).Select(cv => cv.Delivery).Single(), Is.EqualTo(""));

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.EqualTo("giving orders"));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.ChangeOfDelivery));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[1].Delivery, Is.Null);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();

			// Validate environment
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 28)
				.Select(cv => cv.Character).Single(),
				Is.EqualTo("angel of the LORD, an"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 28)
				.Select(cv => cv.Delivery).Single(),
				Is.EqualTo("to Mary"));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 29)
				.Count(c => c.Character == "angel of the LORD, an" && c.Delivery == ""),
				Is.EqualTo(1));
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 29).Select(cv => cv.Character),
				Does.Contain("Mary (Jesus' mother)"));

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[0].CharacterId, Is.EqualTo("angel of the LORD, an"));

			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[1].CharacterId, Is.EqualTo("angel of the LORD, an"));
		}

		[Test]
		public void Parse_DialogueQuoteOnly_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("–", "–", null, 1, QuotationMarkingSystemType.Narrative), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, "));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));

			Assert.That(output[1].GetText(false), Is.EqualTo("–timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(17));
		}

		[Test]
		public void Parse_DialogueBeginningQuoteOnly_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("–", null, null, 1, QuotationMarkingSystemType.Narrative), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));
			Assert.That(output[0].GetText(false), Is.EqualTo("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));


			var block1 = new Block("p", 1, 17);
			block1.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum."));
			var block2 = new Block("p", 1, 18);
			block2.BlockElements.Add(new ScriptText("timiayi."));
			input = new List<Block> { block1, block2 };
			output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));
			Assert.That(output[0].GetText(false), Is.EqualTo("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum."));
			Assert.That(output[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[0].Delivery, Is.Null);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(output[1].GetText(false), Is.EqualTo("timiayi."));
			Assert.That(output[1].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(output[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(18));
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
			Assert.That(output.Count, Is.EqualTo(1));

			Assert.That(output[0].GetText(false), Is.EqualTo("[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(output[0].GetText(true), Is.EqualTo("[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(16));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(9));
		}

		[Test]
		public void Parse_ParagraphAfterScriptureBlockBeginsWithNonWordFormingCharactersBeforeVerseNumber_IncludeAllCharactersInResultingBlocks()
		{
			var block0 = new Block("p", 16, 8) { IsParagraphStart = true };
			block0.BlockElements.Add(new ScriptText(
				"Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo."));
			var block = new Block("p", 16, 8) { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("[ "));
			block.BlockElements.Add(new Verse("9"));
			block.BlockElements.Add(new ScriptText(
				"Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			var input = new List<Block> { block0, block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));
			Assert.That(output[0].GetText(false), Is.EqualTo(
				"Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi " +
				"matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo."));
			Assert.That(output[0].GetText(true), Is.EqualTo(
				"Ci mon gukatti woko ki i lyel, gucako ŋwec ki myelkom pi lworo ma omakogi " +
				"matek twatwal; lworo ogeŋogi tito lokke ki ŋatti mo."));
			Assert.That(output[1].GetText(false), Is.EqualTo(
				"[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam " +
				"Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(output[1].GetText(true), Is.EqualTo(
				"[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot " +
				"Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
			Assert.That(output[0].ChapterNumber, Is.EqualTo(16));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(8));
			Assert.That(output[1].ChapterNumber, Is.EqualTo(16));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(9));
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
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("Yecu onyutte onen bot Maliam Lamagdala"));

			Assert.That(output[1].GetText(false), Is.EqualTo(
				"[ Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot Maliam " +
				"Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(output[1].GetText(true), Is.EqualTo(
				"[ {9}\u00A0Ka en doŋ ocer odiko con i nino mukwoŋo me cabit, okwoŋo nyutte bot " +
				"Maliam Lamagdala, ma yam en oryemo cen abiro i kome-ni. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);

			Assert.That(output[0].ChapterNumber, Is.EqualTo(16));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(8));

			Assert.That(output[1].ChapterNumber, Is.EqualTo(16));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(9));
		}

		//[Test]
		//public void Unparse_OneBlockBecomesThreeBecomesOne_QuoteInMiddle()
		//{
		//	var originalText = "He said, «Go!» quietly.";
		//	var block = new Block("p", 1, 1);
		//	block.BlockElements.Add(new Verse("1"));
		//	block.BlockElements.Add(new ScriptText(originalText));
		//	var input = new List<Block> { block };
		//		//	IList<Block> output1 = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
		//	Assert.That(output1.Count, Is.EqualTo(3));
		//	Assert.That(output1[0].GetText(false), Is.EqualTo("He said, "));
		//	Assert.That(output1[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator), Is.True);
		//	Assert.That(output1[1].GetText(false), Is.EqualTo("«Go!» "));
		//	Assert.That(CharacterVerseData.IsCharacterOfType(output1[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator), Is.False);
		//	Assert.That(output1[2].GetText(false), Is.EqualTo("quietly."));
		//	Assert.That(output1[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator), Is.True);

		//	// now unparse the book
		//	var book = new BookScript("LUK", output1);
		//	var books = new List<BookScript> {book};
		//	var output2 = QuoteParser.Unparse(books);

		//	// expect 1 book
		//	Assert.That(output2.Count, Is.EqualTo(1));

		//	// expect 1 block
		//	Assert.That(output2.First().Value.Count, Is.EqualTo(1));

		//	// expect 2 elements
		//	block = output2.First().Value.First();
		//	Assert.That(block.BlockElements.Count, Is.EqualTo(2));
		//	Assert.IsInstanceOf(typeof(Verse), block.BlockElements[0]);
		//	Assert.IsInstanceOf(typeof(ScriptText), block.BlockElements[1]);
		//	Assert.That(originalText, Is.EqualTo(((ScriptText)block.BlockElements[1]).Content));
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
		//	Assert.That(preparsed.Count, Is.EqualTo(unparsed.Count));

		//	for (var i = 0; i < preparsed.Count; i++)
		//	{
		//		var preparsedBlocks = preparsed[i].Blocks;
		//		var unparsedBlocks = unparsed[unparsedKeys[i]];

		//		// both books contains the same number of blocks
		//		Assert.That(preparsedBlocks.Count, Is.EqualTo(unparsedBlocks.Count));

		//		for (var j = 0; j < preparsedBlocks.Count; j++)
		//		{
		//			// both blocks contain the same number of elements
		//			Assert.That(preparsedBlocks[j].BlockElements.Count, Is.EqualTo(unparsedBlocks[j].BlockElements.Count));
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

			// originally it was throwing "Object reference not set to an instance of an object."
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input,
				new QuoteSystem(levels));
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

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input,
				new QuoteSystem(levels));
			// originally this was throwing an index-out-of-range exception
			var result = parser.Parse().ToList();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0].CharacterId, Is.EqualTo("Satan"));
			Assert.That(result[0].GetText(true), Is.EqualTo(blockP.GetText(true) + " " + blockQ.GetText(true)));
			Assert.That(result[1].CharacterId, Is.EqualTo(GetStandardCharacterId("MAT", Narrator)));
			Assert.That(result[1].GetText(true), Is.EqualTo(blockM.GetText(true)));
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

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input,
				new QuoteSystem(levels));
			// originally this was throwing an index-out-of-range exception
			var result = parser.Parse().ToList();
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].CharacterId, Is.EqualTo("Satan"));
			Assert.That(result[0].GetText(true), Is.EqualTo(
				blockP.GetText(true) + " " + blockQ.GetText(true) + " " + blockM.GetText(true)));
		}

		[TestCase("p", "q1", "q2")]
		[TestCase("p", "q", "m")]
		[TestCase("p", "pi1", "pi2")]
		[TestCase("q1", "q2", "m")]
		public void Parse_PoetryLinesInDifferentVersesWithNoInterveningSentenceEndingPunctuation_VersesAreNotCombined(string style1, string style2, string style3)
		{
			var blockChapter1 = new Block("c", 1) { BookCode = "MRK", CharacterId = GetStandardCharacterId("MRK", BookOrChapter)};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block(style1, 1, 1).AddVerse(1, "Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.");
			var block2 = new Block(style2, 1, 2).AddVerse(2, "This is a poem, ");
			var block3 = new Block(style3, 1, 2);
			block3.BlockElements.Add(new ScriptText("about something good;"));
			var block4 = new Block(style2, 1, 3).AddVerse(3, "So you can see that");
			var block5 = new Block(style3, 1, 3);
			block5.BlockElements.Add(new ScriptText("it's not about something wood."));

			var input = new List<Block> { blockChapter1, block1, block2, block3, block4, block5 };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
			};

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input,
				new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(4));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("MRK"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[0].InitialStartVerseNumber, Is.EqualTo(0));
			Assert.That(results[0].GetText(true), Is.EqualTo("1"));
			Assert.That(results[1].StyleTag, Is.EqualTo(style1));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(results[1].GetText(true), Is.EqualTo(
				"{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(results[2].StyleTag, Is.EqualTo(style2));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(results[2].GetText(true), Is.EqualTo(
				"{2}\u00A0This is a poem, about something good;"));
			// The TestCharacterVerse file does not know about the Scripture quote in Mark 1:2-3.
			Assert.That(results[1].CharacterId, Is.EqualTo(results[2].CharacterId));
			Assert.That(results[3].StyleTag, Is.EqualTo(style2));
			Assert.That(results[3].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[3].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(results[3].GetText(true), Is.EqualTo(
				"{3}\u00A0So you can see that it's not about something wood."));
			Assert.That(results[1].CharacterId, Is.EqualTo(results[3].CharacterId));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote."));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(3));

			Assert.That(output[1].GetText(false), Is.EqualTo("Back to narrator.  No one in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(4));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{2-3}\u00A0«Quote."));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(output[0].InitialEndVerseNumber, Is.EqualTo(3));
			Assert.That(output[0].LastVerseNum, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("{4}\u00A0Back to narrator.  No one in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(output[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[1].LastVerseNum, Is.EqualTo(4));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{3}\u00A0«Quote. "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(output[0].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[0].LastVerseNum, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("{4}\u00A0No one in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(4));
			Assert.That(output[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[1].LastVerseNum, Is.EqualTo(4));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "REV", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(true), Is.EqualTo("{1-2}\u00A0«Quote. {3}\u00A0Possible continuation of quote. "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(output[0].InitialEndVerseNumber, Is.EqualTo(2));
			Assert.That(output[0].LastVerseNum, Is.EqualTo(3));

			Assert.That(output[1].GetText(true), Is.EqualTo("{4}\u00A0Further possible continuation of quote (but it isn't because there is no continuity of characters).»"));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(4));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote."));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("Back to narrator.  Narrator is explicitly in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("Still probably in a quote. "));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[2].GetText(false), Is.EqualTo("Back to narrator.  No one in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("Different character in the control file here but no quote marks, so set to narrator. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("«Quote by different character.» "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Adam"));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Ambiguous quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("Could still be a quote. "));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[2].GetText(false), Is.EqualTo("Back to narrator. "));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtEndOfBook_SetToUnknown()
		{
			var block1 = new Block("p", 5, 43); // Jesus
			block1.BlockElements.Add(new Verse("43"));
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var input = new List<Block> { block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(1));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtSectionHeaderWithCharacterNotInNextVerse_SetToUnknown()
		{
			var block1 = new Block("p", 5, 43) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("43")); // Jesus
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var blockSect = new Block("s", 5, 43) { IsParagraphStart = true, CharacterId = GetStandardCharacterId("MAT", ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 5, 44) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("44")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var input = new List<Block> { block1, blockSect, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("Section Header"));
			Assert.That(IsCharacterOfType(output[1].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(false), Is.EqualTo("No character in control file for this verse. "));
			Assert.That(IsCharacterOfType(output[2].CharacterId, Narrator), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_FirstLevelStillOpenAtSectionHeaderWithVerseContinuingButQuoteNeverClosed_BreakBlockAndSetScriptureBlocksToUnknown()
		{
			var block1 = new Block("p", 5, 43) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("43")); // Jesus
			block1.BlockElements.Add(new ScriptText("«Quote. But where does it end? "));
			var blockSect = new Block("s", 5, 43) { IsParagraphStart = true, CharacterId = GetStandardCharacterId("MAT", ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 5, 43) { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("Verse and quote continues after section header. "));
			block2.BlockElements.Add(new Verse("44")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var input = new List<Block> { block1, blockSect, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(4));
			Assert.That(output[0].GetText(false), Is.EqualTo("«Quote. But where does it end? "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("Section Header"));
			Assert.That(IsCharacterOfType(output[1].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(false), Is.EqualTo("Verse and quote continues after section header. "));
			Assert.That(output[2].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[3].GetText(false), Is.EqualTo("No character in control file for this verse. "));
			Assert.That(IsCharacterOfType(output[3].CharacterId, Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_FirstLevelStillOpenFromMultiVerseBlockAtSectionHeaderWithCharacterNotInNextVerse_SetToUnknown()
		{
			var block1 = new Block("p", 15, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("16")); //Jesus
			block1.BlockElements.Add(new ScriptText("He said, <<Quote. But where does it end? "));
			block1.BlockElements.Add(new Verse("17")); // Jesus
			block1.BlockElements.Add(new ScriptText("Quote continues after verse break."));
			var blockSect = new Block("s", 15, 17) { IsParagraphStart = true, CharacterId = GetStandardCharacterId("MAT", ExtraBiblical) };
			blockSect.BlockElements.Add(new ScriptText("Section Header"));
			var block2 = new Block("p", 15, 18) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("18")); // no one
			block2.BlockElements.Add(new ScriptText("No character in control file for this verse. "));
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal), null, null);
			var input = new List<Block> { block1, blockSect, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].GetText(false), Is.EqualTo("He said, "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].InitialStartVerseNumber, Is.EqualTo(16));
			Assert.That(output[0].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[0].LastVerseNum, Is.EqualTo(16));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("<<Quote. But where does it end? Quote continues after verse break."));
			Assert.That(output[1].CharacterId, Is.EqualTo(kUnexpectedCharacter));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(16));
			Assert.That(output[1].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[1].LastVerseNum, Is.EqualTo(17));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].GetText(false), Is.EqualTo("Section Header"));
			Assert.That(IsCharacterOfType(output[2].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(17));
			Assert.That(output[2].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[2].LastVerseNum, Is.EqualTo(17));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[3].GetText(false), Is.EqualTo("No character in control file for this verse. "));
			Assert.That(IsCharacterOfType(output[3].CharacterId, Narrator), Is.True);
			Assert.That(output[3].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(output[3].InitialEndVerseNumber, Is.EqualTo(0));
			Assert.That(output[3].LastVerseNum, Is.EqualTo(18));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		/// <summary>
		/// Tests logic to keep us from "running off the rails" when a quote is not closed.
		/// </summary>
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input, quoteSystem).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("—Quote. "));
			Assert.That(output[0].CharacterId, Is.EqualTo(kUnexpectedCharacter));

			Assert.That(output[1].GetText(false), Is.EqualTo("No one in the control file for this verse. "));
			Assert.That(IsCharacterOfType(output[1].CharacterId, Narrator), Is.True);
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

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input,
				new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			Assert.That(((ScriptText)results[0].BlockElements[1]).Content, Is.EqualTo("Cutinllatac Dios quillcachishcapica:"));
			Assert.That(((ScriptText)results[1].BlockElements[0]).Content, Is.EqualTo("«Tucui llactacuna,"));
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(((ScriptText)results[2].BlockElements[0]).Content, Is.EqualTo("¡Dios acllashcacunahuan cushicuichic!» "));
			Assert.That(results[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(((ScriptText)results[3].BlockElements[0]).Content, Is.EqualTo("ninmi. "));
			Assert.That(((ScriptText)results[4].BlockElements[1]).Content, Is.EqualTo("«T»"));
		}

		#region Interruption tests

		// These tests all relate to PG-781
		private void AssertIsInterruption(Block block, string expectedInterruptionText)
		{
			Assert.That(block.GetText(true), Is.EqualTo(expectedInterruptionText));
			Assert.That(block.CharacterId, Is.EqualTo(kAmbiguousCharacter),
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			var narrator = GetStandardCharacterId("ROM", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo(
				"{6}\u00A0But the righteousness that is by faith says: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(
				"«Do not say in your heart, ‹Who will ascend into heaven?›» "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[++i].GetText(true), Is.EqualTo(narratorTextV6));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(
				"{7}\u00A0«or ‹Who will descend into the deep?›» "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[++i].GetText(true), Is.EqualTo(narratorTextV7));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("{15}\u00A0«Then you will see the ‹abomination of desolation› "));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Interruption:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("\u2015let the reader understand\u2015 "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			// Text following interruption in original first block:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[i].GetText(true), Is.EqualTo("spoken of by Daniel the prohet. "));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Following blocks:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{16}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{20}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
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
			sectionHeadBlock.SetStandardCharacter("MRK", ExtraBiblical);
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(10));
			// Blocks before section break:
			int i = 0;
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].CharacterIs("MRK", Narrator), Is.True);
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			// Section break:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].CharacterIs("MRK", ExtraBiblical), Is.True);

			// Text preceeding interruption in original block following section break:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(results[i].GetText(true),
				Is.EqualTo("{14}\u00A0“Sa'ad nu ilanju ... sumi'achan, "));

			// Interruption:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("(maserpu maagwatan nan mangwhasa,) "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			// Text following interruption in original first block:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Following blocks:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{17}\u00A0“"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{21}\u00A0“"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(4));
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("{15}\u00A0«Then you will see the ‹abomination of desolation› spoken of by Daniel the prohet "));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Interruption:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("(let the reader understand). "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			// Following blocks:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[i].GetText(true), Does.StartWith("{16}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{20}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			// Text preceding interruption in original first block:
			int i = 0;
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("{15}\u00A0«When you see the ‹abomination of desolation› that Daniel prophesied about "));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Interruption:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("\u2015let the reader understand\u2015 "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			// Text following interruption in original first block:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[i].GetText(true), Is.EqualTo("{16}\u00A0then let those who are in Judea flee. {17}\u00A0Don't go into your house if you're on the roof."));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Following blocks:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{18}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{20}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			// Text preceeding interruption in original first block:
			int i = 0;
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("{14}\u00A0«And this gospel... {15}\u00A0When you see the ‹abomination of desolation› that Daniel prophesied about "));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Interruption:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[i].GetText(true), Is.EqualTo("\u2015let the reader understand\u2015 "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));

			// Text following interruption in original first block:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(results[i].GetText(true), Is.EqualTo("{16}\u00A0then let those who are in Judea flee. {17}\u00A0Don't go into your house if you're on the roof."));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			// Following blocks:
			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{18}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));

			Assert.That(results[++i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(results[i].GetText(true), Does.StartWith("{20}\u00A0«"));
			Assert.That(results[i].CharacterId, Is.EqualTo("Jesus"));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			int i = 0;
			Assert.That(results[i].GetText(true),
				Is.EqualTo("{6}\u00A0But the righteousness that is by faith says: "));
			Assert.That(GetStandardCharacterId("ROM",Narrator),
				Is.EqualTo(results[i].CharacterId));
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[++i].GetText(true),
				Is.EqualTo("«Do not say in your heart, ‹Who will ascend into heaven?› "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[++i].GetText(true), Is.EqualTo(interruptionTextV6));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(results[++i].GetText(true),
				Is.EqualTo("{7}\u00A0or ‹Who will descend into the deep?› "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo("{42}\u00A0He brought him unto Jesus."));
			Assert.That(output[i].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(42));

			Assert.That(output[++i].GetText(true), Is.EqualTo("Jesus looked upon him, and said: "));
			Assert.That(output[i].CharacterIs("JHN", Narrator), Is.True);
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(42));

			Assert.That(output[++i].GetText(true), Is.EqualTo("Thou art Simon the son of John; thou shalt be called Cephas "));
			Assert.That(output[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(42));

			AssertIsInterruption(output[++i], "(which is by interpretation, Peter).");
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(42));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			var narrator = GetStandardCharacterId("ROM", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo("{6}\u00A0Re' la' chiriij i korik wach k'uxliis re' inchalik ruuk' i kojb'aal iriq'or i Looq' laj Huuj chi je' wilih: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo("«Ma-aq'or pan ak'ux: “Ha'wach narijohtiik pan taxaaj?” "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			AssertIsInterruption(results[++i], "(re're' je' cho yuq'unb'al reh i Kristo reh chi nariqajiik cho); ");
			Assert.That(results[++i].GetText(true), Is.EqualTo("{7}\u00A0oon: “Ha'wach nariqajiik chipaam i richamiil i julkahq?”» "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[++i].GetText(true), Is.EqualTo("(re're' je' cho ruksjiik i Kristo chikixilak taqeh kamnaq). "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(4));
			var narrator = GetStandardCharacterId("JHN", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo(
				"{41}\u00A0Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri " +
				"iriguentijeguite, inejapojaqueriqueate icampojiri: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(dialogueDash + "Nonejajiaqueri Meshiashi "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(results[++i].GetText(true), Is.EqualTo(dialogueDash + "ocantaque: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(2));
			var narrator = GetStandardCharacterId("JHN", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo(
				"{41}\u00A0Ariqueate iriatimpa iroqueti yoaquiti icoaguequitiri " +
				"iriguentijeguite, inejapojaqueriqueate icampojiri: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(
				dialogueDash + "Nonejajiaqueri Meshiashi " + dashUsedInText +
				"ocantaque: “Quirishito.”"));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(2));
			var narrator = GetStandardCharacterId("JHN", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo("{41}\u00A0Ariqueate ... icampojiri: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(dialogueQuotation));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
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
			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(3));
			var narrator = GetStandardCharacterId("JHN", Narrator);
			int i = 0;
			Assert.That(results[i].GetText(true), Is.EqualTo(
				"{41}\u00A0Ariqueate iriatimpa ... inejapojaqueriqueate icampojiri: "));
			Assert.That(results[i].CharacterId, Is.EqualTo(narrator));
			Assert.That(results[++i].GetText(true), Is.EqualTo(
				dialogueDash + "Nonejajiaqueri Meshiashi "));
			Assert.That(results[i].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			AssertIsInterruption(results[++i], interruption);
		}

		#region PG-1079 - I wanted to use MAT 1:23 for these tests like the original error but changing CharacterVerse (to add the Interruption) for that verse broke an existing test
		[Test]
		public void Parse_InterruptionNotInMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14)
				.AddVerse(14, "«The virgin will conceive and give birth to a son, and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].GetText(false), Is.EqualTo("«The virgin will conceive and give birth to a son, and they will call him Immanuel "));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo("(which means God with us).»"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_InterruptionAtEndOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive and give birth to a son, ");
			var block2 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].GetText(false), Is.EqualTo("«The virgin will conceive and give birth to a son, "));
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[1].GetText(false), Is.EqualTo("and they will call him Immanuel "));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[2].GetText(false), Is.EqualTo("(which means God with us).»"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_InterruptionAtEndof3PartMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us).»");
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_InterruptionNearEndOfMultiBlockQuote_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us) ");
			var block4 = new Block("p", 13, 14).AddText("thusly.»");
			var input = new List<Block> { block, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[0].CharacterId, Is.EqualTo(output[4].CharacterId));
		}

		[Test]
		public void Parse_InterruptionInMiddleOfMultiBlockQuoteWithFollowingTextInSamePara_MultiBlockQuoteSetCorrectly()
		{
			var block = new Block("p", 13, 14).AddVerse(14, "«The virgin will conceive ");
			var block2 = new Block("p", 13, 14).AddText("and give birth to a son, ");
			var block3 = new Block("p", 13, 14).AddText("and they will call him Immanuel (which means God with us) thusly ");
			var block5 = new Block("p", 13, 14).AddText("and such»");
			var input = new List<Block> { block, block2, block3, block5 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(6));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[0].CharacterId, Is.EqualTo(output[4].CharacterId));

			Assert.That(output[4].CharacterId, Is.EqualTo(output[5].CharacterId));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(6));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[0].CharacterId, Is.EqualTo(output[4].CharacterId));

			Assert.That(output[4].CharacterId, Is.EqualTo(output[5].CharacterId));
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
			var quoteSystem = new QuoteSystem(new BulkObservableList<QuotationMark>
			{
				new QuotationMark(":", null, null, 1, QuotationMarkingSystemType.Narrative)
			});
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM",
				input, quoteSystem).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(6));

			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[0].CharacterId, Is.EqualTo(GetStandardCharacterId("ROM", Narrator)));
			Assert.That(output[1].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(output[2].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[3].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(output[4].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(output[5].CharacterId, Is.EqualTo(kScriptureCharacter));
		}
		#endregion

		internal static QuoteSystem QuoteSystemForStandardAmericanEnglish
		{
			get
			{
				var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
				quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
				quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
				return quoteSystem;
			}
		}
	}

	[TestFixture]
	public class QuoteParserTestsWithTestCharacterVerseOct2015
	{
		[OneTimeSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
		}

		[Test]
		public void Parse_BlockContainsNarratorQuotation_NoOtherCharactersInControlFile_RemainsAsSingleBlockAssignedToNarrator()
		{
			var block = new Block("p", 3, 2).AddVerse(2, "A crippled guy was carried to the, «Beautiful» gate to beg.").AddVerse(3, "When he saw...");
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));

			Assert.That(output[0].GetText(true), Is.EqualTo(block.GetText(true)));
			Assert.That(output[0].CharacterIs("ACT", Narrator), Is.True);
		}

		[Test]
		public void Parse_BlockContainsNarratorQuotation_OtherCharactersInControlFile_BlockSplitAndQuoteMarkedAsAmbiguous()
		{
			var block = new Block("p", 5, 1).AddVerse(1, "This is the book of Adam's race. When God created man, He made him in his image.")
				.AddVerse(2, "He created them and blessed them and named them «Man» that day.");
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));
			Assert.That(output[1].CharacterIsUnclear, Is.True);
		}

		[Test]
		public void Parse_AlternateCharactersInControlFile_QuoteAssignedToNormalCharacter()
		{
			var bookNumActs = BCVRef.BookToNumber("ACT");
			Assert.That(!ControlCharacterVerseData.Singleton.GetCharacters(bookNumActs, 10, new SingleVerse(15))
				.Any(cv => cv.Character == "God" || cv.Character == "Jesus"),
				"Test setup condition not met: Neither God nor Jesus should be returned as a character when includeAlternatesAndRareQuotes is false.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumActs, 10,
				new SingleVerse(15), includeAlternatesAndRareQuotes: true)
				.Select(cv => cv.Character),
				Is.EquivalentTo(new[] { "Holy Spirit, the", "God", "Jesus" }),
				"Test setup condition not met: God and Jesus should be returned as characters when includeAlternatesAndRareQuotes is true.");

			var input = new List<Block> { new Block("p", 10, 15)
				.AddVerse(15, "Il entend la voix ... fois. Elle lui dit: «Ce que Dieu a ... interdit!»") };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].CharacterIs("ACT", Narrator), Is.True);

			Assert.That(output[1].CharacterId, Is.EqualTo("Holy Spirit, the"));
			Assert.That(output[1].UserConfirmed, Is.False);
		}

		[Test]
		public void Parse_AlternateCharactersInControlFile_VerseTextAssignedToImplicitCharacter()
		{
			var bookNumIsaiah = BCVRef.BookToNumber("ISA");
			var onlyNonAlternateCv = ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 37, new SingleVerse(6)).Single();
			Assert.That(onlyNonAlternateCv.Character, Is.EqualTo("Isaiah"),
				"Test setup condition not met: Isaiah should be the only character returned for ISA 37:6 when includeAlternatesAndRareQuotes is false.");
			Assert.That(onlyNonAlternateCv.QuoteType, Is.EqualTo(QuoteType.Normal),
				"Test setup condition not met: Isaiah should be Normal for ISA 37:6.");
			onlyNonAlternateCv = ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 37, new SingleVerse(7)).Single();
			Assert.That(onlyNonAlternateCv.Character, Is.EqualTo("Isaiah"),
				"Test setup condition not met: Isaiah should be the only character returned for ISA 37:7 when includeAlternatesAndRareQuotes is false.");
			Assert.That(onlyNonAlternateCv.QuoteType, Is.EqualTo(QuoteType.Implicit),
				"Test setup condition not met: Isaiah should be Implicit for ISA 37:7.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 37,
				new []{ new SingleVerse(6), new SingleVerse(7)}, includeAlternatesAndRareQuotes: true)
				.Select(cv => cv.Character),
				Is.EquivalentTo(new [] { "Isaiah", "God" }),
				"Test setup condition not met: Isaiah and God should be returned as characters when includeAlternatesAndRareQuotes is true.");

			var input = new List<Block>
			{
				new Block("p", 37, 6) {IsParagraphStart = true}.AddVerse(6, "Isaiah told them,"),
				new Block("p", 37, 6) {IsParagraphStart = true}.AddText("Tell your master, God says: Don't fear the blasphemous words of the servants of the king of Assyria.")
				.AddVerse(7, "Lo I will freak him out with some news to make him go home and get killed there."),
			};
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].CharacterIs("ISA", Narrator), Is.True);

			Assert.That(output[1].CharacterId, Is.EqualTo("Isaiah"));
			Assert.That(output[1].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(output[1].IsParagraphStart, Is.True);
			Assert.That(output[1].UserConfirmed, Is.False);

			Assert.That(output[2].CharacterId, Is.EqualTo("Isaiah"));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(output[2].IsParagraphStart, Is.False);
			Assert.That(output[2].UserConfirmed, Is.False);
		}

		[TestCase("", "", "", "")]
		[TestCase("«", "»", "", "")]
		[TestCase("", "", "«", "»")]
		[TestCase("«", "»", "‹", "›")]
		public void Parse_QuotedExpressionInImplicitlyAssignedVerse_VerseTextAssignedToImplicitCharacter(string openQuoteMarkForPassageIfAny,
			string closeQuoteMarkForPassageIfAny, string openQuoteMarkForExpressionIfAny, string closeQuoteMarkForExpressionIfAny)
		{
			var bookNumIsaiah = BCVRef.BookToNumber("ISA");
			var entriesInCvForIsa62V4 = ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 62, new SingleVerse(4),
				includeAlternatesAndRareQuotes: true);
			Assert.That(QuoteType.ImplicitWithPotentialSelfQuote, Is.EqualTo(entriesInCvForIsa62V4.Single().QuoteType),
				"Test setup condition not met: ISA 62:4 should have one entry, of type ImplicitWithPotentialSelfQuote.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 62,
					new []{new SingleVerse(3), new SingleVerse(5)}).Single().IsImplicit, Is.True, 
				"Test setup condition not met: God should be the implicit speaker in ISA 62:3-5.");

			var input = new List<Block>
			{
				new Block("p", 62, 3).AddVerse(3, openQuoteMarkForPassageIfAny + "You'll be a crown in God's hand.")
					.AddVerse(4, $"You shall not be termed {openQuoteMarkForExpressionIfAny}Forsaken{closeQuoteMarkForExpressionIfAny}; neither shall " +
						$"your land be termed {openQuoteMarkForExpressionIfAny}Desolate{closeQuoteMarkForExpressionIfAny}. But you'll be called " +
						$"{openQuoteMarkForExpressionIfAny}Hephzibah{closeQuoteMarkForExpressionIfAny}, and your land " +
						$"{openQuoteMarkForExpressionIfAny}Beulah{closeQuoteMarkForExpressionIfAny}; for God delights in you, and your land shall be married.")
					.AddVerse(5, "For as a young man marries a virgin, so your sons shall marry you and God will rejoice over you." + closeQuoteMarkForPassageIfAny)
			};
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));
			Assert.That(output[0].CharacterId, Is.EqualTo("God"));
		}

		/// <summary>
		/// This test covers the case where the reference guide and control file assume that God wil speak for a couple verses
		/// and then the prophet/narrator will take over. But in the project data, the quote is never closed, so God should
		/// continue to speak in the script. The control file can accommodate this by listing God as an Alternate for the
		/// following verses. This test ensures that while we normally ignore Alternates when doing automatic block assignments,
		/// we do properly take them into consideration when a quote is not closed.
		/// </summary>
		[Test]
		public void Parse_LongQuoteStartsAsNormalButContinuesAsAlternateInControlFile_QuoteAssignedToInitialNormalCharacter()
		{
			var bookNumIsaiah = BCVRef.BookToNumber("ISA");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1, new VerseBridge(2, 3)).Select(cv => cv.Character).Distinct().Single(),
				Is.EqualTo("God"),
				"Test setup condition not met: God (not Isaiah) should be the primary/expected character in ISA 1:2-3; " +
				"Isaiah should not be included when includeAlternatesAndRareQuotes is false.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1, new VerseBridge(4, 5)).Select(cv => cv.Character).Distinct().Single(),
				Is.EqualTo("Isaiah"),
				"Test setup condition not met: Isaiah (not God) should be the primary/expected character in ISA 1:4-5; " +
				"God should not be included when includeAlternatesAndRareQuotes is false.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1,
				new VerseBridge(2, 5), includeAlternatesAndRareQuotes: true)
				.Select(cv => cv.Character).Distinct(),
				Is.EquivalentTo(new [] { "Isaiah", "God" }),
				"Test setup condition not met: God and Isaiah should both be returned as characters for ISA 1:2-5 when includeAlternatesAndRareQuotes is true.");

			var input = new List<Block> {
				new Block("q1", 1, 2)
					.AddVerse(2, "Listen, because the Lord has spoken: “I reared children, but they rebelled.")
					.AddVerse(3, "The ox and donkey know who takes care of them, but Israel doesn’t know diddly."),
				new Block("q1", 1, 4)
					.AddVerse(4, "“Oh, you sinful nation, burdened by iniquity! You offspring of evildoers! You corrupt children!"),
				new Block("q2", 1, 4)
					.AddText("“They’ve abandoned the Lord; they’ve despised the Holy One of Israel; they’ve walked away from me."),
				new Block("q2", 1, 5)
					.AddVerse(5, "“Why be struck down? Why continue to rebel? Your head is sick, and your heart is faint.”")
			};
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "\u2014", "\u2014");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count + 1));

			Assert.That(output[0].CharacterIs("ISA", Narrator), Is.True);

			Assert.That(output.Skip(1).All(b => b.CharacterId == "God"), Is.True);
			Assert.That(output.All(b => !b.UserConfirmed), Is.True);
		}

		/// <summary>
		/// This test covers the case where the reference guide and control file assume that Isaiah will speak if there is a quote
		/// but have God as an Alternate. However, the quote continues on into verse that must be God (one normal verse folllowed by
		/// a few implicit verses). This test ensures that while we normally ignore Alternates when doing automatic block assignments,
		/// we do properly take them into consideration when a quote is not closed.
		/// </summary>
		[TestCase(2)]
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		[TestCase(10)]
		public void Parse_LongQuoteStartsAsPotentialAndAlternateButContinuesIntoVersesWhereAltIsNormalThenImplicit_QuoteAssignedToAltNormalImplicitCharacter(int quoteStartVerse)
		{
			var bookNumIsaiah = BCVRef.BookToNumber("ISA");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1,
				new VerseBridge(2, 3)).Select(cv => cv.Character).Distinct().Single(),
				Is.EqualTo("God"),
				"Test setup condition not met: God (not Isaiah) should be the primary/expected character in ISA 1:2-3; " +
				"Isaiah should not be included when includeAlternatesAndRareQuotes is false.");
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1, new VerseBridge(4, 10)).Select(cv => cv.Character).Distinct().Single(),
				Is.EqualTo("Isaiah"),
				"Test setup condition not met: Isaiah (not God) should be the primary/expected character in ISA 1:4-10; " +
				"God should not be included when includeAlternatesAndRareQuotes is false.");
			var characterInIsa1V11 = ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1,
				new SingleVerse(11), includeAlternatesAndRareQuotes: true).Single();
			Assert.That(characterInIsa1V11.Character, Is.EqualTo("God"),
				"Test setup condition not met: God should be the only possible character in ISA 1:11.");
			Assert.That(QuoteType.Normal, Is.EqualTo(characterInIsa1V11.QuoteType),
				"Test setup condition not met: God should be a normal (not implicit) character in ISA 1:11.");
			var characterInIsa1V12 = ControlCharacterVerseData.Singleton.GetCharacters(bookNumIsaiah, 1,
				new VerseBridge(12, 17), includeAlternatesAndRareQuotes: true).Single();
			Assert.That(characterInIsa1V11.Character, Is.EqualTo(characterInIsa1V12.Character),
				"Test setup condition not met: God should be the only possible character in ISA 1:12-17.");
			Assert.That(QuoteType.Normal, Is.EqualTo(characterInIsa1V12.QuoteType),
				"Test setup condition not met: God should be the implicit speaker in in ISA 1:12-17.");

			var input = new List<Block> {
				new Block("q1", 1, 2)
					.AddVerse(2, "Listen, because the Lord has spoken: I reared children, but they rebelled."),
				new Block("q1", 1, 3)
					.AddVerse(3, "The ox and donkey know who takes care of them, but Israel doesn’t know diddly."),
				new Block("q1", 1, 4)
					.AddVerse(4, "Oh, you sinful nation, burdened by iniquity! You offspring of evildoers! You corrupt children!"),
				new Block("q2", 1, 4)
					.AddText("They’ve abandoned the Lord; they’ve despised the Holy One of Israel; they’ve walked away from me."),
				new Block("q2", 1, 5)
					.AddVerse(5, "Why be struck down? Why continue to rebel? Your head is sick, and your heart is faint."),
				new Block("q2", 1, 6)
					.AddVerse(6, "You are a hot mess from top to bottom, like a gaping wound."),
				new Block("q1", 1, 7)
					.AddVerse(7, "Your land has been ravaged."),
				new Block("q2", 1, 8)
					.AddVerse(8, "Jerusalem has been reduced to an abandoned shack."),
				new Block("q2", 1, 9)
					.AddVerse(9, "If God had not spared a tiny remnant, we would have been wiped off the map."),
				new Block("q1", 1, 10)
					.AddVerse(10, "I am talking to you rebels:"),
				new Block("q2", 1, 11)
					.AddVerse(11, "Do you think I need the meat and blood of the animals you sacrifice? They make me sick!"),
				new Block("q2", 1, 12)
					.AddVerse(12, "What makes you think I want you near my house?"),
				new Block("q1", 1, 13)
					.AddVerse(13, "Just forget all this worthless and meaningless worship."),
				new Block("q2", 1, 14)
					.AddVerse(14, "You might think you are doing what you are supposed to do, but I am tired of it all!"),
				new Block("q2", 1, 15)
					.AddVerse(15, "You commit acts of violence with your hands and then fold them in worthless prayers. I am not listening!"),
				new Block("q1", 1, 16)
					.AddVerse(16, "Clean up your act."),
				new Block("q2", 1, 17)
					.AddVerse(17, "If you want to worship me, see if you can figure out how to start loving widows and orphans.”")
			};
			var textOfVerseWhereQuoteStarts = (ScriptText)input.First(b => b.InitialStartVerseNumber == quoteStartVerse).BlockElements[1];
			textOfVerseWhereQuoteStarts.Content = "The LORD says, “" + textOfVerseWhereQuoteStarts.Content;
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input, quoteSystem).Parse().ToList();
			// Note: after skipping any verse that precede the beginning of the verse where the quote starts, we also need to
			// skip the narrator block that introduces the speaking.
			Assert.That(output.SkipWhile(b => b.InitialStartVerseNumber < quoteStartVerse).Skip(1).All(b => b.CharacterId == "God"), Is.True);
		}

		[Test]
		public void Parse_RareQuoteContinuedIntoVerseWithNormalScriptureQuote_ScriptureCharacterAssigned()
		{
			var input = new List<Block>
			{
				new Block("p", 12, 39) {IsParagraphStart = true}.AddVerse(39, "Gima rino bon ingon jo kep Yesus bi Yesaya tri tut,"),
				new Block("p", 12, 39) {IsParagraphStart = true}.AddText("“Allah pai, ")
					.AddVerse(40, "‘Ditak igyai ba. Dikes bingat ba, niti au. Diyai nok ingkwei hanyen Da bidibindei, hanyen bi.’ ”"),
			};

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-JHN"));
			Assert.That(output[1].CharacterId, Is.EqualTo("scripture"));
		}

		[Test]
		public void Parse_VersesInVerseBridgeHaveTwoCharactersWithConflictingAlternateAndNormalQuoteTypes_CharacterSetAsAmbiguous()
		{
			var input = new List<Block>
			{
				new Block("p", 1, 3, 4) {IsParagraphStart = true}.AddVerse("3-4", "“The ox and donkey know their master, but this sinful nation " +
					"of mine is full of iniquity. They are like corrupt children who do not know or understand what family they belong to. They have " +
					"so despised their Lord as to utterly forsake him, and now they have become cut off and are buried under a pile of sin.”"),
			};

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(1));
			Assert.That(output[0].CharacterId, Is.EqualTo(kAmbiguousCharacter));
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[0].CharacterId, Is.EqualTo("narrator-JHN"));
			Assert.That(output[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[3].CharacterId, Is.EqualTo("Jesus"));
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

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "PSA", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(3));

			Assert.That(output[0].StyleTag, Is.EqualTo(q1));
			Assert.That(output[0].GetText(true), Is.EqualTo("{8}\u00A0I will keep thy statutes; This language doesn't use periods in poetry"));

			Assert.That(output[1].GetText(true), Is.EqualTo("Beth"));

			Assert.That(output[2].GetText(true), Is.EqualTo("{9}\u00A0How shall a pipsqueak cleanse his way?"));

			Assert.That(output.All(b => b.CharacterIs("PSA", Narrator)), Is.True);
		}

		// Test for PG-1121
		[Test]
		public void Parse_ColonFollowedByNormalInlineQuoteAndSubsequentVerses_ColonNotTreatedAsStartOfDialogue()
		{
			var block1 = new Block("p", 21, 40) { IsParagraphStart = true }
				.AddVerse(40, "Chay capitán “allinmi” niykuptinmi, Pablo upallachirqa. Upallaruptinkum nirqa:");
			var blockChapter22 = new Block("c");
			blockChapter22.BlockElements.Add(new ScriptText("Chapter 22"));
			blockChapter22.SetStandardCharacter("ACT", BookOrChapter);
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
			blockChapter23.SetStandardCharacter("ACT", BookOrChapter);
			var block6 = new Block("p", 23, 23) { IsParagraphStart = true }.AddVerse(23, "Soldadokunapa capitanninkunata qayaykuspan kamachirqa:");
			// Note: In verse 24, the quote really should end right before the word "nispan", but we would need to implement PG-487 to support that.
			var block7 = new Block("p", 23, 23) { IsParagraphStart = true }.AddText("Iskay pachak alistaychik, chaynapi llaqtaman rinankupaq. ")
				.AddVerse(24, "Alistaychiktaqyá Pablo sillakunanpaq, chaynapi sanollata Felixman chayachinaykichikpaq, nispan.");
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
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input, quoteSystem).Parse().ToList();

			int i = 0;
			var block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{40}\u00A0Chay capitán "));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("“allinmi” "));
			Assert.That(block.CharacterId, Is.EqualTo("commander of Roman troops in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("niykuptinmi, Pablo upallachirqa. Upallaruptinkum nirqa:"));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("Chapter 22"));
			Assert.That(IsCharacterOfType(block.CharacterId, BookOrChapter), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{1}\u00A0“Taytakuna, uyariykuwaychik”, "));
			Assert.That(block.CharacterId, Is.EqualTo("Paul"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("nispa."));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{2}\u00A0Chaymi hebreo upallarurqaku. Pablo nirqa:"));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{3}\u00A0Diosninchiktam tukuy servisqaykichikta hina."));
			Assert.That(block.CharacterId, Is.EqualTo("Paul"));

			// This would be nice, but we aren't that good yet.
			//block = output[i++];
			//Assert.That(block.GetText(true), Is.EqualTo("{6}\u00A0Ichaqa Damasco qonqayllamanta kanchaykamuwarqa. " +
			//	"{7}\u00A0Hinaptinmi pampaman wichiykuspay uyarirqani “¡Saulo, Saulo! ¿Imanasqataq qatikachawanki?” niqta. " +
			//	"{8}\u00A0Chaymi ta-purqani: “¿Pitaq Señor?”, nispay. Hinaptinmi arqa: “Ñoqaqa Nazaret Jesusmi kani”, nispan. " +
			//	"{9}\u00A0Riqmasiykunapas nisqantaqa. " +
			//	"{10}\u00A0Hinaptinmi tapurqani: “¿Señor, ruwasaq?” nispay. Payñataqmi: “Hatarispayki Damasco, chaypimnisunki”, nispa. "));
			//Assert.That(block.CharacterId, Is.EqualTo("Paul"));

			while (output[++i].GetText(true) != "Chapter 23");

			block = output[i++];
			Assert.That(IsCharacterOfType(block.CharacterId, BookOrChapter), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{23}\u00A0Soldadokunapa capitanninkunata qayaykuspan kamachirqa:"));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"Iskay pachak alistaychik, chaynapi llaqtaman rinankupaq. " +
				"{24}\u00A0Alistaychiktaqyá Pablo sillakunanpaq, chaynapi sanollata Felixman chayachinaykichikpaq, nispan."));
			Assert.That(block.CharacterId, Is.EqualTo("commander of Roman troops in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{25}\u00A0Paykunawanmi kayna niq cartata apachirqa:"));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"{26}\u00A0“Ancha reqsisqa prefecto Félix, ñoqa Claudio Lisiasmi saludamuyki. " +
				"{27}\u00A0Pusachimusqay munarqaku. Ichaqa Roma, soldadokunawan rispay salvaramurqani. " +
				"{28}\u00A0Hinaspaymi imamantam judiokunapa cortenman pusarqani. " +
				"{29}\u00A0Chaypim yachamurqani acusasqankuta. Ichaqa kananpaqpas. " +
				"{30}\u00A0Ichaqa wañurachinankupaq qanman pusachimuyki. Chaynallataqmi acusanankupaq”, "));
			Assert.That(block.CharacterId, Is.EqualTo("commander of Roman troops in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("nispa."));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		#region PG-1191: Combine poetry blocks with preceding paragraph only if there's not an expected Scripture quote in the verse
		[Test]
		public void Parse_PoetryNarratorBlockFollowsNormalParagraphInVerseWithoutScriptureQuote_PoetryParagraphsCombinedWithPreceding()
		{
			var input = new List<Block>();
			input.Add(new Block("c", 8) { IsParagraphStart = true, BookCode = "ACT" }.AddText("8"));
			input.Add(new Block("p", 8, 34) { IsParagraphStart = true }.AddVerse(34, "Lë naʼ nayúj lu guich rulabëʼ, rna cni:"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Gulachë́ʼë Lëʼ quië ludöddëʼ"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 34) { IsParagraphStart = true }.AddText("Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));

			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddVerse(35, "Gulucaʼnëʼ Lëʼ caʼz,"));
			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddText("Len cutu gluʼë latj nu cuequi xbey Lëʼ."));
			input.Add(new Block("q", 8, 35) { IsParagraphStart = true }.AddText("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));

			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input, quoteSystem).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[1].CharacterIs("ACT", Narrator), Is.True);
			Assert.That(output[1].IsParagraphStart, Is.True);
			Assert.That(output[1].StyleTag, Is.EqualTo("p"));
			Assert.That(output[1].GetText(true),
				Is.EqualTo("{34}\u00A0Lë naʼ nayúj lu guich rulabëʼ, rna cni: Ca böʼcuʼ zxílaʼdauʼ, Gulachë́ʼë Lëʼ quië ludöddëʼ Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ, " +
				"Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));

			Assert.That(output[2].CharacterIs("ACT", Narrator), Is.True);
			Assert.That(output[2].IsParagraphStart, Is.True);
			Assert.That(output[2].StyleTag, Is.EqualTo("q"));
			Assert.That(output[2].GetText(true), Is.EqualTo("{35}\u00A0Gulucaʼnëʼ Lëʼ caʼz, Len cutu gluʼë latj nu cuequi xbey Lëʼ."));

			Assert.That(output[3].CharacterIs("ACT", Narrator), Is.True);
			Assert.That(output[3].IsParagraphStart, Is.True);
			Assert.That(output[3].StyleTag, Is.EqualTo("q"));
			Assert.That(output[3].GetText(true), Is.EqualTo("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));
		}

		[Test]
		public void Parse_PoetryNarratorBlockFollowsNormalParagraphInVerseWithScriptureQuote_PoetryScriptureParasNotCombinedWithNormalPara()
		{
			var input = new List<Block>();
			input.Add(new Block("c", 8) { IsParagraphStart = true, BookCode = "ACT" }.AddText("8"));
			input.Add(new Block("p", 8, 32) { IsParagraphStart = true }.AddVerse(32, "Lë naʼ nayúj lu guich rulabëʼ, rna cni:"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Gulachë́ʼë Lëʼ quië ludöddëʼ"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ,"));
			input.Add(new Block("q", 8, 32) { IsParagraphStart = true }.AddText("Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));

			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddVerse(33, "Gulucaʼnëʼ Lëʼ caʼz,"));
			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddText("Len cutu gluʼë latj nu cuequi xbey Lëʼ."));
			input.Add(new Block("q", 8, 33) { IsParagraphStart = true }.AddText("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));

			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input, quoteSystem).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[1].CharacterIs("ACT", Narrator), Is.True);
			Assert.That(output[1].IsParagraphStart, Is.True);
			Assert.That(output[1].StyleTag, Is.EqualTo("p"));
			Assert.That(output[1].GetText(true), Is.EqualTo("{32}\u00A0Lë naʼ nayúj lu guich rulabëʼ, rna cni:"));
			Assert.That(output[1].CharacterIs("ACT", Narrator), Is.True);
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[2].IsParagraphStart, Is.True);
			Assert.That(output[2].StyleTag, Is.EqualTo("q"));
			Assert.That(output[2].GetText(true),
				Is.EqualTo("Ca böʼcuʼ zxílaʼdauʼ, Gulachë́ʼë Lëʼ quië ludöddëʼ Lëʼ, Len ca böʼcuʼ zxílaʼdauʼ, " +
				"Cutu rnëbaʼ catiʼ nu rchugu lítsaʼbaʼ, Caʼ benëʼ Lëʼ, cutu bsalj ruʼë gnëʼ."));
			Assert.That(output[2].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[3].IsParagraphStart, Is.True);
			Assert.That(output[3].StyleTag, Is.EqualTo("q"));
			Assert.That(output[3].GetText(true), Is.EqualTo("{33}\u00A0Gulucaʼnëʼ Lëʼ caʼz, Len cutu gluʼë latj nu cuequi xbey Lëʼ."));
			Assert.That(output[3].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[4].IsParagraphStart, Is.True);
			Assert.That(output[4].StyleTag, Is.EqualTo("q"));
			Assert.That(output[4].GetText(true), Is.EqualTo("¿Nuzxa caz gac quixjöʼ zxguiaʼ nabágaʼgac bunách uládz queëʼ?"));
			Assert.That(output[4].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
		}
		#endregion

		#region Tests for PG-40
		[Test]
		public void Parse_NoQuotationMarksInPassageWithImplicitQuotes_ImplicitCharacterInfoSet()
		{
			var block1 = new Block("p", 2, 28) { IsParagraphStart = true }.AddVerse(28, "Simeon received him, blessed God, and said,");
			var block2 = new Block("q1", 2, 29) { IsParagraphStart = true }.AddVerse(29, "Release your servant in peace now, Lord,");
			var block3 = new Block("q2", 2, 29) { IsParagraphStart = true }.AddText("according to your promise;");
			var block4 = new Block("q1", 2, 30) { IsParagraphStart = true }.AddVerse(30, "for I have seen you come to the rescue,");
			var block5 = new Block("q2", 2, 31) { IsParagraphStart = true }.AddVerse(31, "according to your plan, before all peoples;");
			var block6 = new Block("q1", 2, 32) { IsParagraphStart = true }.AddVerse(32, "a brilliance to make the nations see,");
			var block7 = new Block("q2", 2, 32) { IsParagraphStart = true }.AddText("and the shekinah of your people Israel.");
			var block8 = new Block("p", 2, 33) { IsParagraphStart = true }.AddVerse(33, "Joseph and his mom were wowed by the things spoken concerning the boy; ")
				.AddVerse(34, "He blessed them, saying to Mary, “Get ready for some ups and downs in Israel because of this child. People are going to criticize him. ")
				.AddVerse(35, "Additionally, a sword will puncture your soul, so the thoughts of many hearts can be shown.”");
			var block9 = new Block("p", 2, 36) { IsParagraphStart = true }.AddVerse(36, "There was a prophetess named Anna (she was old, having been married 7 years ")
				.AddVerse(37, "and then surviving as a widow for 84 more), who didn’t leave the temple, where she worshipped with fasting and prayer.");
			var input = new List<Block> { block1, block2, block3, block4, block5, block6, block7, block8, block9 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();

			int i = 0;
			var block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{28}\u00A0Simeon received him, blessed God, and said,"));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"{29}\u00A0Release your servant in peace now, Lord, according to your promise;"));
			Assert.That(block.CharacterId, Is.EqualTo("Simeon, devout man in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{30}\u00A0for I have seen you come to the rescue,"));
			Assert.That(block.CharacterId, Is.EqualTo("Simeon, devout man in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{31}\u00A0according to your plan, before all peoples;"));
			Assert.That(block.CharacterId, Is.EqualTo("Simeon, devout man in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{32}\u00A0a brilliance to make the nations see, and the shekinah of your people Israel."));
			Assert.That(block.CharacterId, Is.EqualTo("Simeon, devout man in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"{33}\u00A0Joseph and his mom were wowed by the things spoken concerning the boy; " +
				"{34}\u00A0He blessed them, saying to Mary, "));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"“Get ready for some ups and downs in Israel because of this child. People are going to criticize him. " +
				"{35}\u00A0Additionally, a sword will puncture your soul, so the thoughts of many hearts can be shown.”"));
			Assert.That(block.CharacterId, Is.EqualTo("Simeon, devout man in Jerusalem"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"{36}\u00A0There was a prophetess named Anna (she was old, having been married 7 years " +
				"{37}\u00A0and then surviving as a widow for 84 more), who didn’t leave the temple, where she worshipped with fasting and prayer."));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteBeginsWithPartialVerseNotMarkedAsImplicit_SingleParagraphWithMultipleLeadingAndSingleTrailingNarratorVerse_BlockSplitToApplyImplicitInfo()
		{
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 30) { IsParagraphStart = true }.AddVerse(30, "Moses sprinkled the men with oil. ")
				.AddVerse(31, "And Moses told Aaron and sons, Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse(32, "Burn the leftovers. ")
				.AddVerse(33, "Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ")
				.AddVerse(35, "Staying there night and day will keep you from dying. ")
				.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(30));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(32));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(35));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteBeginsWithPartialVerseNotMarkedAsImplicit_SingleParagraphWithLeadingAndTrailingNarratorVerses_BlockSplitToApplyImplicitInfo()
		{
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 31) { IsParagraphStart = true }.AddVerse(31, "And Moses told Aaron and sons, Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse(32, "Burn the leftovers. ").AddVerse(33, "Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ").AddVerse(35, "Staying there night and day will keep you from dying. ")
				.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(32));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(35));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteBeginsWithPartialVerseNotMarkedAsImplicit_SpeechBrokenOutAsSeparateParagraph_ImplicitCharacterInfoSet()
		{
			var chapter = new Block("c", 8) {CharacterId = GetStandardCharacterId("LEV", BookOrChapter)}.AddText("8");
			var block1 = new Block("p", 8, 31) {IsParagraphStart = true}.AddVerse(31, "And Moses told Aaron and sons,");
			var block2 = new Block("p", 8, 31) {IsParagraphStart = true}.AddText("Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse(32, "Burn the leftovers. ").AddVerse(33, "Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ").AddVerse(35, "Staying there night and day will keep you from dying. ");
			var block3 = new Block("q1", 8, 36) {IsParagraphStart = true}.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> {chapter, block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(((ScriptText)outBlock.BlockElements.First()).Content,
				Is.EqualTo("Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. "));

			outBlock = output[i++];
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(35));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteBeginsWithPartialVerseAndWholeVersesNotMarkedAsImplicit_LeadingVersesParagraph_ImplicitCharacterInfoSet()
		{
			var chapter = new Block("c", 5) {CharacterId = GetStandardCharacterId("AMO", BookOrChapter)}.AddText("8");
			var block1 = new Block("p", 5, 18) {IsParagraphStart = true}.AddVerse(18, "The Lord says: ");
			var block2 = new Block("p", 5, 18) {IsParagraphStart = true}
				.AddText("Woe to you who want the day of the Lord! It is dark, ")
				.AddVerse(19, "as if a man fled from a lion and get killed by a bear or serpent. ")
				.AddVerse(20, "Isn't that right? Dark not light. ")
				.AddVerse(21, "I detest your feats and gatherings. ")
				.AddVerse(22, "Don't even get me started on your burnt offerings! ")
				.AddVerse(23, "You call that noise worship music!? If you want me to listen,")
				.AddVerse(24, "Start pursuing justice and righteousness.");
			var input = new List<Block> {chapter, block1, block2};
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "AMO", input).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(4));

			Assert.That(output[2].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[2].InitialStartVerseNumber, Is.EqualTo(18));
			Assert.That(output[2].LastVerseNum, Is.EqualTo(20));
		}

		[Test]
		public void Parse_ImplicitQuoteHasExplicitQuotationMarksAndExtraHeSaidsInSeparateBlocks_ExplicitQuoteMarkedAsImplicitSpeakerAndHeSaidMarkedAsNeedsReview()
		{
			 
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 31) { IsParagraphStart = true }
				.AddVerse(31, "And Moses told Aaron and sons, «Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse(32, "Burn the leftovers», so he said.");
			var block2 = new Block("p", 8, 33) { IsParagraphStart = true }
				.AddVerse(33, "Moses continued: «Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ")
				.AddVerse(35, "Staying there night and day will keep you from dying,» said he.");
			var block3 = new Block("q1", 8, 36) { IsParagraphStart = true }.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"«Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. " +
				"{32}\u00A0Burn the leftovers», "));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(32));
			Assert.That(outBlock.GetText(true), Is.EqualTo("so he said."));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(33));
			Assert.That(outBlock.GetText(true), Is.EqualTo("{33}\u00A0Moses continued: "));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"«Stay seven days until you are consecrated. " +
				"{34}\u00A0This is how the Lord will make atonement. " +
				"{35}\u00A0Staying there night and day will keep you from dying,» "));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(35));
			Assert.That(outBlock.GetText(true), Is.EqualTo("said he."));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteHasExplicitQuotationMarksAndExtraHeSaidsWeirdAndComplex_ExplicitQuoteMarkedAsImplicitSpeakerAndHeSaidMarkedAsNeedsReview()
		{
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 31) { IsParagraphStart = true }
				.AddVerse(31, "And Moses told Aaron and sons, «Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse("32-33", "Stay seven days next to the burning leftovers until you are consecrated.»");
			var block2 = new Block("p", 8, 32, 33) { IsParagraphStart = true }
				.AddText("Having thus commanded, ")
				.AddVerse(34, "Moses told them how to make atonement, saying: ")
				.AddVerse(35, "«Stay there night and day to keep you from dying.»");
			var block3 = new Block("q1", 8, 36) { IsParagraphStart = true }.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"«Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. " +
				"{32-33}\u00A0Stay seven days next to the burning leftovers until you are consecrated.»"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(32));
			Assert.That(outBlock.InitialEndVerseNumber, Is.EqualTo(33));
			Assert.That(outBlock.GetText(true), Is.EqualTo("Having thus commanded, " +
				"{34}\u00A0Moses told them how to make atonement, saying: "));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(35));
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"{35}\u00A0«Stay there night and day to keep you from dying.»"));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteContainsExpectedSelfQuoteMarkedAsFirstLevel_ImplicitCharacterInfoSet()
		{
			var chapter = new Block("c", 51) { CharacterId = GetStandardCharacterId("ISA", BookOrChapter) }.AddText("8");
			var block1 = new Block("q1", 51, 16) { IsParagraphStart = true }.AddVerse(16, "I have told you what to say, and I will keep you safe in the palm of my hand.");
			var block2 = new Block("q1", 51, 16) { IsParagraphStart = true }.AddText("I spread out the heavens and laid foundations for the earth.");
			var block3 = new Block("q1", 51, 16) { IsParagraphStart = true }.AddText("Now I say, «Jerusalem, your people are mine.»");
			var input = new List<Block> { chapter, block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(4));
			Assert.That(output.Skip(1).All(b => "God" == b.CharacterId), Is.True);
		}

		[Test]
		public void Parse_ImplicitQuoteHasExplicitQuotationMarksAndExtraHeSaidInContinuousBlock_ExplicitQuoteMarkedAsImplicitSpeakerAndHeSaidMarkedAsNeedsReview()
		{
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 31) { IsParagraphStart = true }
				.AddVerse(31, "And Moses told Aaron and sons, «Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. ")
				.AddVerse(32, "Burn the leftovers», so he said. ")
				.AddVerse(33, "Moses continued: «Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ")
				.AddVerse(35, "Staying there night and day will keep you from dying,» said he.");
			var block2 = new Block("q1", 8, 36) { IsParagraphStart = true }.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(31));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"«Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it. " +
				"{32}\u00A0Burn the leftovers», "));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(32));
			Assert.That(outBlock.GetText(true), Is.EqualTo("so he said. {33}\u00A0Moses continued: "));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.GetText(true), Is.EqualTo(
				"«Stay seven days until you are consecrated. " +
				"{34}\u00A0This is how the Lord will make atonement. " +
				"{35}\u00A0Staying there night and day will keep you from dying,» "));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(35));
			Assert.That(outBlock.GetText(true), Is.EqualTo("said he."));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterIs("LEV", Narrator), Is.True);

			Assert.That(output.Count, Is.EqualTo(i));
		}

		[Test]
		public void Parse_ImplicitQuoteBeginsWithPartialVerseNotMarkedAsImplicit_OneVersePerParagraph_ImplicitCharacterInfoSet()
		{
			var chapter = new Block("c", 8) {CharacterId = GetStandardCharacterId("LEV", BookOrChapter)}.AddText("8");
			var block1 = new Block("p", 8, 31) { IsParagraphStart = true }.AddVerse(31, "And Moses told Aaron and sons, Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it.");
			var block2 = new Block("q1", 8, 32) { IsParagraphStart = true }.AddVerse(32, "Burn the leftovers.");
			var block3 = new Block("q2", 8, 33) { IsParagraphStart = true }.AddVerse(33, "Stay seven days until you are consecrated.");
			var block4 = new Block("q1", 8, 34) { IsParagraphStart = true }.AddVerse(34, "This is how the Lord will make atonement.");
			var block5 = new Block("q2", 8, 35) { IsParagraphStart = true }.AddVerse(35, "Staying there night and day will keep you from dying.");
			var block6 = new Block("q1", 8, 36) { IsParagraphStart = true }.AddVerse(36, "So that's what Aaron and the boys did.");
			var input = new List<Block> { chapter, block1, block2, block3, block4, block5, block6 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			Assert.That(output.Count, Is.EqualTo(input.Count));

			int i = 1;
			var block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo(
				"{31}\u00A0And Moses told Aaron and sons, Boil the meat by the tabernacle and eat it with bread, as I commanded: Ya'll eat it."));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{32}\u00A0Burn the leftovers."));
			Assert.That(block.CharacterId, Is.EqualTo("Moses"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{33}\u00A0Stay seven days until you are consecrated."));
			Assert.That(block.CharacterId, Is.EqualTo("Moses"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{34}\u00A0This is how the Lord will make atonement."));
			Assert.That(block.CharacterId, Is.EqualTo("Moses"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{35}\u00A0Staying there night and day will keep you from dying."));
			Assert.That(block.CharacterId, Is.EqualTo("Moses"));

			block = output[i++];
			Assert.That(block.GetText(true), Is.EqualTo("{36}\u00A0So that's what Aaron and the boys did."));
			Assert.That(IsCharacterOfType(block.CharacterId, Narrator), Is.True);
		}

		[Test]
		public void Parse_ImplicitQuoteIsPartOfAVerseBridge_BreakOffBridgePartsAndMarkThemAsNeedsReview()
		{
			var chapter = new Block("c", 8) { CharacterId = GetStandardCharacterId("LEV", BookOrChapter) }.AddText("8");
			var block1 = new Block("p", 8, 31, 32) {IsParagraphStart = true}
				.AddVerse("31-32", "Boil the meat by the tabernacle and eat it with bread, burning the leftovers as commanded, Moses told Aaron and his sons. ")
				.AddVerse(33, "Stay seven days until you are consecrated. ")
				.AddVerse(34, "This is how the Lord will make atonement. ")
				.AddVerse("35-36", "So that's what Aaron and the boys did because Moses said, Staying there night and day will keep you from dying.");
			var input = new List<Block> { chapter, block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LEV", input).Parse().ToList();

			int i = 1;
			var outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(31));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(32));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(33));
			Assert.That(outBlock.LastVerseNum, Is.EqualTo(34));
			Assert.That(outBlock.CharacterId, Is.EqualTo("Moses"));

			outBlock = output[i++];
			Assert.That(outBlock.InitialStartVerseNumber, Is.EqualTo(35));
			Assert.That(outBlock.InitialEndVerseNumber, Is.EqualTo(36));
			Assert.That(outBlock.CharacterId, Is.EqualTo(kNeedsReview));

			Assert.That(output.Count, Is.EqualTo(i));
		}
		#endregion PG-40

		#region Test for PG-1201
		[Test]
		public void Parse_HypotheticalCharacterNotInReferenceText_HypotheticalCharacterTreatedAsNarratorQuote()
		{
			var chapter = new Block("c", 12) { CharacterId = GetStandardCharacterId("1CO", BookOrChapter) }.AddText("12");
			var block1 = new Block("p", 12, 15) { IsParagraphStart = true }
				.AddVerse(15, "If the foot says, “I ain't a hand, so I ain't part of the body,” that wouldn't make it so. ")
				.AddVerse(16, "Likewise, if the ear says, “I ain't no eye, so I ain't part of the body,” that logic doesn't fly. ")
				.AddVerse(17, "If the whole body were one member, where would the other senses and functions get done? ")
				.AddVerse(18, "But in fact God has placed the parts in the body as He saw fit. ")
				.AddVerse(19, "If there were just one big honkin' part, where would the body be? ")
				.AddVerse(20, "As it is, there are many parts, but one body.");
			var block2 = new Block("p", 12, 15) { IsParagraphStart = true }
			.AddVerse(21, "The eye can't tell the hand, “I don’t need you!” And the head can't tell the feet, “I don’t need you!”");
			var input = new List<Block> { chapter, block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			
			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton,
				ReferenceText.GetStandardReferenceText(ReferenceTextType.Russian));
			IList<Block> output = new QuoteParser(cvRepo, "1CO", input, quoteSystem).Parse()
				.ToList();

			Assert.That(output.Count, Is.EqualTo(input.Count));
			Assert.That(output.Skip(1).All(b => b.CharacterIs("1CO", Narrator)), Is.True);
		}

		[Test]
		public void Parse_HypotheticalCharacterInReferenceText_HypotheticalCharacterUsed()
		{
			var chapter = new Block("c", 10) { CharacterId = GetStandardCharacterId("PSA", BookOrChapter) }.AddText("10");
			var block6a = new Block("q1", 10, 6) { IsParagraphStart = true }.AddVerse(6, "He says to himself, “Nothing could ever shake me.”");
			var block6b = new Block("q2", 10, 6) { IsParagraphStart = true }.AddText("He swears, “No one will ever harm me.”");
			var block7a = new Block("q1", 10, 7) { IsParagraphStart = true }.AddVerse(7, "His mouth is filled with lies and threats;");
			var block7b = new Block("q2", 10, 7) {IsParagraphStart = true}.AddText("trouble and evil are under his tongue.");
			var block8a = new Block("q1", 10, 8) {IsParagraphStart = true}.AddVerse(8, "He lies in ambush near the hamlets;");
			var block8b = new Block("q2", 10, 8) { IsParagraphStart = true }.AddText(" he jumps out to kill the innocent.");
			var block8c = new Block("q1", 10, 8) {IsParagraphStart = true}.AddText("His eyes watch for his victims from his hiding place;");
			var block9a = new Block("q2", 10, 9) { IsParagraphStart = true }.AddVerse(9, "like a lion in hiding he crouches in wait."); 
			var block9b = new Block("q1", 10, 9) {IsParagraphStart = true}.AddText("He lies in wait to snare the helpless;");
			var block9c = new Block("q2", 10, 9) { IsParagraphStart = true }.AddText("he catches the weak and drags them off in his mesh bag.");
			var block10a = new Block("q1", 10, 10) {IsParagraphStart = true}.AddVerse(10, "His victims are crushed and collapse;");
			var block10b = new Block("q2", 10, 10) { IsParagraphStart = true }.AddText("they fall under his power.");
			var block11a = new Block("q1", 10, 11) {IsParagraphStart = true}.AddVerse(11, "He tells himself, “God will never take note;");
			var block11b = new Block("q2", 10, 11) { IsParagraphStart = true }.AddText("he hides his face and ignores everything.”");

			var input = new List<Block> { chapter, block6a, block6b, block7a, block7b, block8a, block8b, block8c,
				block9a, block9b, block9c, block10a, block10b, block11a, block11b };

			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);

			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton,
				ReferenceText.GetStandardReferenceText(ReferenceTextType.English));
			IList<Block> output = new QuoteParser(cvRepo, "PSA", input, quoteSystem).Parse()
				.ToList();

			int i = 1;
			var narrator = GetStandardCharacterId("PSA", Narrator);
			Assert.That(output[i].GetText(true), Is.EqualTo("{6}\u00A0He says to himself, "));

			Assert.That(output[i++].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].GetText(true), Is.EqualTo("“Nothing could ever shake me.”"));

			Assert.That(output[i++].CharacterId, Is.EqualTo("man, wicked"));
			Assert.That(output[i].GetText(true), Is.EqualTo("He swears, "));

			Assert.That(output[i++].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].GetText(true), Is.EqualTo("“No one will ever harm me.”"));

			Assert.That(output[i++].CharacterId, Is.EqualTo("man, wicked"));

			Assert.That(output.Skip(i).TakeWhile(b => b.InitialStartVerseNumber < 11 || b.InitialStartVerseNumber == 11 && b.StartsAtVerseStart)
				.All(b => b.CharacterId == narrator), Is.True);

			Assert.That(output.Last().GetText(true), Is.EqualTo("“God will never take note; he hides his face and ignores everything.”"));
			Assert.That(output.Last().CharacterId, Is.EqualTo("man, wicked"));
		}
		#endregion

		#region Tests for PG-1272
		[TestCase(":")]
		[TestCase(null)]
		public void Parse_BlocksHaveCharacterSetBasedOnSpecialUSXCharStyles_PresetCharactersNotChanged(string dialogueQuoteStart)
		{
			var block1 = new Block("p", 4, 1) {IsParagraphStart = true};
			block1.BlockElements.Add(new Verse("1"));
			block1.BlockElements.Add(new ScriptText("Jesus was led by the Spirit into the wild to be tempted by Satan. "));
			block1.BlockElements.Add(new Verse("2"));
			block1.BlockElements.Add(new ScriptText("After a long fast, he was starving. "));
			block1.BlockElements.Add(new Verse("3"));
			block1.BlockElements.Add(new ScriptText("The tempter came and said, “If you are the Son of God, make these stones into bread.”"));
			var block2 = new Block("p", 4, 4) {IsParagraphStart = true};
			block2.BlockElements.Add(new Verse("4"));
			block2.BlockElements.Add(new ScriptText("Jesus replied, "));
			var block3 = new Block("wj", 4, 4) {IsParagraphStart = false, CharacterId = "Jesus"};
			block3.BlockElements.Add(new ScriptText("The Word says: “Man must not live on bread alone, but on what God says.”"));
			var block4 = new Block("p", 4, 5) {IsParagraphStart = true};
			block4.BlockElements.Add(new Verse("5"));
			block4.BlockElements.Add(new ScriptText("The devil led him to the Temple in Zion "));
			block4.BlockElements.Add(new Verse("6"));
			block4.BlockElements.Add(new ScriptText("“If you are the Son of God,” he said, “hurl yourself down.” He tempted him by quoting where the Word says: "));
			var block5 = new Block("qt", 4, 6) {IsParagraphStart = false, CharacterId = "scripture"};
			block5.BlockElements.Add(new ScriptText("He will command his angels to hold you up and not let you smash your foot on a rock."));
			var input = new List<Block> {block1, block2, block3, block4, block5};
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), dialogueQuoteStart, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(10));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo(
				"{1}\u00A0Jesus was led by the Spirit into the wild to be tempted by Satan. " +
				"{2}\u00A0After a long fast, he was starving. " +
				"{3}\u00A0The tempter came and said, "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("“If you are the Son of God, make these stones into bread.”"));
			Assert.That(output[i].CharacterId, Is.EqualTo("Satan (Devil)"));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("{4}\u00A0Jesus replied, "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("The Word says: “Man must not live on bread alone, but on what God says.”"));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("wj"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("{5}\u00A0The devil led him to the Temple in Zion "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("{6}\u00A0“If you are the Son of God,” "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Satan (Devil)"));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("he said, "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("“hurl yourself down.” "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Satan (Devil)"));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("He tempted him by quoting where the Word says: "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo("He will command his angels to hold you up and not let you smash your foot on a rock."));
			Assert.That(output[i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("qt"));
		}

		[Test]
		public void Parse_WordsOfJesusInBlockWhereJesusSpeaksWithSpecificDelivery_DeliveryAssignedBasedOnControlFile()
		{
			var block1 = new Block("p", 11, 25) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("25"));
			block1.BlockElements.Add(new ScriptText("Then Jesus said: "));
			var block2 = new Block("wj", 11, 25) { IsParagraphStart = false, CharacterId = "Jesus" };
			block2.BlockElements.Add(new ScriptText("I praise you, Father, Lord of all, because you keep wise people in the dark and you show yourself to kids "));
			block2.BlockElements.Add(new Verse("26"));
			block2.BlockElements.Add(new ScriptText("because that is what pleases you. "));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo("{25}\u00A0Then Jesus said: "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				"I praise you, Father, Lord of all, because you keep wise people in the dark and you show yourself to kids " +
				"{26}\u00A0because that is what pleases you. "));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].Delivery, Is.EqualTo("praying"));
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("wj"));
		}

		// PG-1272: The expected results for these tests are a bit dubious. In the unlikely case where there is a verse bridge that crosses
		// the line from one delivery to another, as long as there is a delivery that all verses/bridges in the block share in common, that
		// one will be used. It's quite likely that what we'd really want is an undefined delivery, but these test cases are really contrived
		// and I can't think of a real scenario where this is likely to occur. Until a real user complains, it's not likely to be worth any
		// further programming to make it work "as expected" (whatever that turns out to be).
		[TestCase("25", "26-27", ExpectedResult = "praying")]
		[TestCase("21-23", "24-25", ExpectedResult = "rebuking")]
		[TestCase("21-23", "24-26", ExpectedResult = "rebuking")]
		public string Parse_BridgesWhereJesusSpeaksWithConflictingDeliveries_AssignedToJesusWithDeliveryInCommonAcrossAllVerseBridges(
			string firstRef, string secondRef)
		{
			var firstVerseOrBridge = new Verse(firstRef);
			var block1 = new Block("p", 11, firstVerseOrBridge.StartVerse, firstVerseOrBridge.LastVerseOfBridge) { IsParagraphStart = true };
			block1.BlockElements.Add(firstVerseOrBridge);
			block1.BlockElements.Add(new ScriptText("Then Jesus said: “First thing. "));
			block1.BlockElements.Add(new Verse(secondRef));
			block1.BlockElements.Add(new ScriptText("Second thing.”"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo("{" + firstRef + "}\u00A0Then Jesus said: "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				"“First thing. {" + secondRef + "}\u00A0Second thing.”"));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			return output[i].Delivery;
		}

		[Test]
		public void Parse_WordsOfJesusInBlockWhereJesusSpeaksWithDifferentDeliveries_AssignedToJesusWithUndefinedDelivery()
		{
			var block1 = new Block("p", 14, 18) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("18"));
			block1.BlockElements.Add(new ScriptText("Jesus said: "));
			var block2 = new Block("wj", 14, 18) { IsParagraphStart = false, CharacterId = "Jesus" };
			block2.BlockElements.Add(new ScriptText("Bring the food here to me "));
			block2.BlockElements.Add(new Verse("19"));
			block2.BlockElements.Add(new ScriptText("and have everyone sit down on the turf. "));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo(block1.GetText(true)));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].StyleTag, Is.EqualTo("p"));

			Assert.That(output[++i].GetText(true), Is.EqualTo(block2.GetText(true)));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].Delivery, Is.Null);
			Assert.That(output[i].IsParagraphStart, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("wj"));
		}

		// As part of the work for PG-1272, I concluded that if we have a block that covers multiple verses that have the same character
		// but incompatible deliveries, we should just leave the delivery undefined.
		[TestCase("25-26", "27")]
		[TestCase("25-26", "27-28")]
		[TestCase("21-24", "25")]
		[TestCase("21-24", "25-26")]
		public void Parse_BlockWhereJesusSpeaksWithConflictingDeliveries_AssignedToJesusWithNoSpecificDelivery(string firstRef, string secondRef)
		{
			var firstVerseOrBridge = new Verse(firstRef);
			var block1 = new Block("p", 11, firstVerseOrBridge.StartVerse, firstVerseOrBridge.LastVerseOfBridge) { IsParagraphStart = true };
			block1.BlockElements.Add(firstVerseOrBridge);
			block1.BlockElements.Add(new ScriptText("Then Jesus said: “First thing. "));
			block1.BlockElements.Add(new Verse(secondRef));
			block1.BlockElements.Add(new ScriptText("Second thing.”"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(2));

			int i = 0;
			Assert.That(output[i].GetText(true), Is.EqualTo("{" + firstRef + "}\u00A0Then Jesus said: "));
			Assert.That(IsCharacterOfType(output[i].CharacterId, Narrator), Is.True);

			Assert.That(output[++i].GetText(true), Is.EqualTo(
				"“First thing. {" + secondRef + "}\u00A0Second thing.”"));
			Assert.That(output[i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].Delivery, Is.Null);
		}
		#endregion // Tests for PG-1272

		#region Tests for PG-1415
		// The following three tests have the same setup as QuoteParserTests.Parse_PoetryLinesInDifferentVersesWithNoInterveningSentenceEndingPunctuation_VersesAreNotCombined.
		// That fixture uses the TestCharacterVerse file, so the results are different because that
		// version of the file does not contain entries for the Scripture quotes in Mark 1:2-3.
		// In this class, the tests are split into two different versions because text marked with
		// the \m marker must not be included as part of the implicit Scripture quote.
		[TestCase("q1", "q2")]
		[TestCase("pi1", "pi2")]
		public void Parse_PoetryLinesInVersesWhoseOnlyKnownCharacterIsScripture_PoetryBlocksTreatedAsMultiBlockScriptureQuote(string style2, string style3)
		{
			var blockChapter1 = new Block("c", 1) { BookCode = "MRK", CharacterId = GetStandardCharacterId("MRK", BookOrChapter)};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block("p", 1, 1).AddVerse(1, "Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.");
			var block2 = new Block(style2, 1, 2).AddVerse(2, "This is a poem, ");
			var block3 = new Block(style3, 1, 2);
			block3.BlockElements.Add(new ScriptText("about something good;"));
			var block4 = new Block(style2, 1, 3).AddVerse(3, "So you can see that");
			var block5 = new Block(style3, 1, 3);
			block5.BlockElements.Add(new ScriptText("it's not about something wood."));

			var input = new List<Block> { blockChapter1, block1, block2, block3, block4, block5 };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
			};

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton,
				blockChapter1.BookCode, input, new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(4));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("MRK"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[0].InitialStartVerseNumber, Is.EqualTo(0));

			Assert.That(results[1].StyleTag, Is.EqualTo("p"));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(results[1].GetText(true), Is.EqualTo(
				"{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(results[1].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);

			Assert.That(results[2].StyleTag, Is.EqualTo(style2));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(results[2].GetText(true), Is.EqualTo(
				"{2}\u00A0This is a poem, about something good;"));
			Assert.That(results[2].CharacterId, Is.EqualTo("scripture"));
			Assert.That(results[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(results[3].StyleTag, Is.EqualTo(style2));
			Assert.That(results[3].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[3].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(results[3].GetText(true), Is.EqualTo(
				"{3}\u00A0So you can see that it's not about something wood."));
			Assert.That(results[3].CharacterId, Is.EqualTo("scripture"));
			Assert.That(results[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
		}

		[Test]
		public void Parse_PoetryLinesAndContinuationLinesStartInVersesWithoutKnownScripture_PoetryBlocksKeptAsNarrator()
		{
			var blockChapter1 = new Block("c", 1) { BookCode = "MRK", CharacterId = GetStandardCharacterId("MRK", BookOrChapter)};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block("q", 1, 1).AddVerse(1, "Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.");
			var block2 = new Block("q", 1, 2).AddVerse(2, "This is a poem, ");
			var block3 = new Block("m", 1, 2);
			block3.BlockElements.Add(new ScriptText("about something good;"));
			var block4 = new Block("q", 1, 3).AddVerse(3, "So you can see that");
			var block5 = new Block("m", 1, 3);
			block5.BlockElements.Add(new ScriptText("it's not about something wood."));

			var input = new List<Block> { blockChapter1, block1, block2, block3, block4, block5 };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
			};

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton,
				blockChapter1.BookCode, input, new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("MRK"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[0].InitialStartVerseNumber, Is.EqualTo(0));

			Assert.That(results[1].StyleTag, Is.EqualTo("q"));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(results[1].GetText(true), Is.EqualTo(
				"{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(results[1].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			// Since the poetry containing this next block started in a verse where we were not
			// expecting a Scripture quote, we can't 100% safely treat this as a Scripture quote.
			Assert.That(results[2].StyleTag, Is.EqualTo("q"));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(results[2].GetText(true),
				Is.EqualTo("{2}\u00A0This is a poem, about something good;"));
			Assert.That(results[2].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[3].StyleTag, Is.EqualTo("q"));
			Assert.That(results[3].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[3].InitialStartVerseNumber, Is.EqualTo(3));
			// Since the preceding "m" continuation paragraph popped us out of poetry,
			// This new poetry that is entirely in v. 3 CAN be safely treated as a
			// Scripture quote. A little weird, maybe, but probably what we'd want -or
			// at least the best assumption we can make.
			Assert.That(results[3].GetText(true), Is.EqualTo("{3}\u00A0So you can see that"));
			Assert.That(results[3].CharacterId, Is.EqualTo("scripture"));
			Assert.That(results[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[4].StyleTag, Is.EqualTo("m"));
			Assert.That(results[4].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[4].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(results[4].GetText(true), Is.EqualTo("it's not about something wood."));
			Assert.That(results[4].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[TestCase("q1", "q2", "m")]
		// I'm not 100% sure that "mi" should be treated as a continuation, but it's a rare style and is
		// formatted similar to "m", so it's probably safer to treat it in a similar way.
		[TestCase("q", "q", "mi")]
		public void Parse_PoetryLinesAndContinuationLinesInVersesWhoseOnlyKnownCharacterIsScripture_PoetryBlocksTreatedAsScriptureQuotes(
			string style1, string style2, string continuationStyle)
		{
			var blockChapter1 = new Block("c", 1) { BookCode = "MRK", CharacterId = GetStandardCharacterId("MRK", BookOrChapter)};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block("p", 1, 1).AddVerse(1, "Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco.");
			var block2 = new Block(style1, 1, 2).AddVerse(2, "This is a poem, ");
			var block3 = new Block(continuationStyle, 1, 2);
			block3.BlockElements.Add(new ScriptText("about something good;"));
			var block4 = new Block(style2, 1, 3).AddVerse(3, "So you can see that");
			var block5 = new Block(continuationStyle, 1, 3);
			block5.BlockElements.Add(new ScriptText("it's not about something wood."));

			var input = new List<Block> { blockChapter1, block1, block2, block3, block4, block5 };

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal),
			};

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton,
				blockChapter1.BookCode, input, new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(6));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("MRK"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[0].InitialStartVerseNumber, Is.EqualTo(0));

			Assert.That(results[1].StyleTag, Is.EqualTo("p"));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(1));
			Assert.That(results[1].GetText(true), Is.EqualTo("{1}\u00A0Ka nino okato manok, Yecu dok odwogo i Kapernaum, ci pire owinnye ni en tye paco."));
			Assert.That(results[1].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[2].StyleTag, Is.EqualTo(style1));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(results[2].GetText(true), Is.EqualTo("{2}\u00A0This is a poem, "));
			Assert.That(results[2].CharacterId, Is.EqualTo("scripture"));
			Assert.That(results[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[3].StyleTag, Is.EqualTo(continuationStyle));
			Assert.That(results[3].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[3].InitialStartVerseNumber, Is.EqualTo(2));
			Assert.That(results[3].GetText(true), Is.EqualTo("about something good;"));
			Assert.That(results[3].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[4].StyleTag, Is.EqualTo(style2));
			Assert.That(results[4].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[4].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(results[4].GetText(true), Is.EqualTo("{3}\u00A0So you can see that"));
			Assert.That(results[4].CharacterId, Is.EqualTo("scripture"));
			Assert.That(results[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[5].StyleTag, Is.EqualTo(continuationStyle));
			Assert.That(results[5].ChapterNumber, Is.EqualTo(1));
			Assert.That(results[5].InitialStartVerseNumber, Is.EqualTo(3));
			Assert.That(results[5].GetText(true), Is.EqualTo("it's not about something wood."));
			Assert.That(results[5].CharacterIs(blockChapter1.BookCode, Narrator), Is.True);
			Assert.That(results[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_PoetryLinesInVerseWithScriptureAndOtherPossibleSpeakers_PoetryBlocksKeptAsNarrator()
		{
			var blockChapter9 = new Block("c", 9) { BookCode = "MAT", CharacterId = GetStandardCharacterId("MAT", BookOrChapter)};
			blockChapter9.BlockElements.Add(new ScriptText("9"));
			var block1 = new Block("p", 9, 36).AddVerse(36, "When Jesus saw the crowds, he was moved by compassion for them, because they were oppressed and helpless, ");
			var block2 = new Block("qr", 9, 36).AddText("like a scattered flock of sheep with no pastor.");

			var input = new List<Block> { blockChapter9, block1, block2 };

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton, blockChapter9.BookCode, input);
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(3));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("MAT"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(9));

			Assert.That(results[1].StyleTag, Is.EqualTo("p"));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(9));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(results[1].GetText(true), Is.EqualTo("{36}\u00A0When Jesus saw the crowds, he was moved by compassion for them, because they were oppressed and helpless, "));
			Assert.That(results[1].CharacterIs(blockChapter9.BookCode, Narrator), Is.True);
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			// Since this verse has other potential speakers, we can't 100% safely treat this as a Scripture quote.
			Assert.That(results[2].StyleTag, Is.EqualTo("qr"));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(9));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(36));
			Assert.That(results[2].GetText(true), Is.EqualTo("like a scattered flock of sheep with no pastor."));
			Assert.That(results[2].CharacterIs(blockChapter9.BookCode, Narrator), Is.True);
			Assert.That(results[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_PoetryLinesInVerseWithScriptureAndNarratorQuote_PoetryBlocksCombinedAndTreatedAsScriptureQuote()
		{
			var blockChapter4 = new Block("c", 4) { BookCode = "HEB", CharacterId = GetStandardCharacterId("HEB", BookOrChapter)};
			blockChapter4.BlockElements.Add(new ScriptText("4"));
			var block1 = new Block("p", 4, 7).AddVerse(7, "God again set a certain day, calling it “Today.” " +
				"This was when he later spoke through David, as in already quoted:");
			var block2 = new Block("q1", 4, 7).AddText("Today, if you hear his voice, ");
			var block3 = new Block("q2", 4, 7).AddText("do not harden your hearts.");

			var levels = new BulkObservableList<QuotationMark>
			{
				new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal),
			};
			
			var input = new List<Block> { blockChapter4, block1, block2, block3 };

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton,
				blockChapter4.BookCode, input, new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(5));
			Assert.That(results[0].StyleTag, Is.EqualTo("c"));
			Assert.That(results[0].IsChapterAnnouncement, Is.True);
			Assert.That(results[0].BookCode, Is.EqualTo("HEB"));
			Assert.That(results[0].ChapterNumber, Is.EqualTo(4));

			Assert.That(results[1].StyleTag, Is.EqualTo("p"));
			Assert.That(results[1].ChapterNumber, Is.EqualTo(4));
			Assert.That(results[1].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(results[1].GetText(true), Is.EqualTo("{7}\u00A0God again set a certain day, calling it "));
			Assert.That(results[1].CharacterIs(blockChapter4.BookCode, Narrator), Is.True);
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[2].StyleTag, Is.EqualTo("p"));
			Assert.That(results[2].ChapterNumber, Is.EqualTo(4));
			Assert.That(results[2].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(results[2].GetText(true), Is.EqualTo("“Today.” "));
			Assert.That(results[2].CharacterId, Is.EqualTo(kAmbiguousCharacter));
			Assert.That(results[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[3].StyleTag, Is.EqualTo("p"));
			Assert.That(results[3].ChapterNumber, Is.EqualTo(4));
			Assert.That(results[3].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(results[3].GetText(true), Is.EqualTo("This was when he later spoke through David, as in already quoted:"));
			Assert.That(results[3].CharacterIs(blockChapter4.BookCode, Narrator), Is.True);
			Assert.That(results[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(results[4].StyleTag, Is.EqualTo("q1"));
			Assert.That(results[4].ChapterNumber, Is.EqualTo(4));
			Assert.That(results[4].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(results[4].GetText(true), Is.EqualTo("Today, if you hear his voice, do not harden your hearts."));
			Assert.That(results[4].CharacterId, Is.EqualTo(kScriptureCharacter));
			Assert.That(results[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}
		#endregion

		#region PG-1435
		[Test]
		public void Parse_NoQuotationMarksSpecified_DoesNotCrash()
		{
			var blockChapter1 = new Block("c", 1)
			{
				BookCode = "HEB",
				CharacterId = GetStandardCharacterId("HEB", BookOrChapter)
			};
			blockChapter1.BlockElements.Add(new ScriptText("1"));
			var block1 = new Block("p", 1, 1).AddVerse(1, "Welcome to Hebrews.");

			var levels = new BulkObservableList<QuotationMark>
			{
			};

			var input = new List<Block> {blockChapter1, block1};

			var parser = new QuoteParser(ControlCharacterVerseData.Singleton,
				blockChapter1.BookCode, input, new QuoteSystem(levels));
			var results = parser.Parse().ToList();

			Assert.That(results.Count, Is.EqualTo(2));
		}
		#endregion

		#region PG-1419 tests (blocks created by quote milestones)
		// Note: More tests for PG-1419 in QuoteParserTests
		[TestCase(true, false, "men, some")]
		[TestCase(true, false, null)]
		[TestCase(true, true, null)]
		[TestCase(false, false, null)]
		[TestCase(false, false, "men, some")]
		[TestCase(false, true, "men, some")]
		public void Parse_InterruptionInExplicitQuote_QuoteCharacterAndInterruptionPreserved(
			bool includeQuoteIdsForQuotes, bool includeQuoteIdsForInterruption, string characterForMenQuote)
		{
			var narrator = GetStandardCharacterId("2CH", Narrator);
			var chapterCharacter = GetStandardCharacterId("2CH", BookOrChapter);
			var blockC20 = new Block("c", 20) { CharacterId = chapterCharacter };
			var block1 = new Block("p", 20, 1) {IsParagraphStart = true}
				.AddVerse(1, "After this, the Moabites and others came to war against Jehoshaphat. ");
			var block2a = new Block("p", 20, 2) {IsParagraphStart = true}
				.AddVerse(2, "Some people came and told Jehoshaphat, ");
			var blockMen2b = new Block("qt1-s", 20, 2) { CharacterId = characterForMenQuote}
				.AddText("“A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar ");
			if (includeQuoteIdsForQuotes)
				blockMen2b.BlockElements.Insert(0, new QuoteId {Id = "m1", Start = true});
			var blockInt2c = new Block("qt2-s", 20, 2) { CharacterId = narrator }
				.AddText("(that is, En Gedi) ");
			if (includeQuoteIdsForInterruption)
			{
				blockInt2c.BlockElements.Insert(0, new QuoteId {Id = "i1", Start = true, IsNarrator = true});
				blockInt2c.BlockElements.Add(new QuoteId {Id = "i1", Start = false, IsNarrator = true});
			}
			var blockMen2d = new Block("qt1-s", 20, 2) { CharacterId = characterForMenQuote}
				.AddText("is where they are currently camped.” ")
				.AddEndQuoteId(includeQuoteIdsForQuotes ? "m1" : null);
			var block3a = new Block("p", 20, 3)
				.AddVerse(3, "Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: ");
			var blockJehoshaphat3b = new Block("qt1-s", 20, 3) { CharacterId = "Jehoshaphat, king of Judah"}
				.AddText("All Judah must fast. ");
			if (includeQuoteIdsForQuotes)
				blockJehoshaphat3b.BlockElements.Insert(0, new QuoteId {Id = "j1", Start = true});

			blockJehoshaphat3b.AddEndQuoteId(includeQuoteIdsForQuotes ? "j1" : null);

			var input = new List<Block> { blockC20, block1, block2a, blockMen2b, blockInt2c, blockMen2d, block3a, blockJehoshaphat3b };

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "2CH", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));
			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("men, some"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("men, some"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jehoshaphat, king of Judah"));
			Assert.That(output[i].Delivery, Is.Null);
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].StyleTag, Is.EqualTo("qt1-s"));
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_InterruptionInParserDetectedQuote_QuoteCharactersAndInterruptionPreserved(
			bool includeQuoteIdsForInterruption)
		{
			var narrator = GetStandardCharacterId("2CH", Narrator);
			var chapterCharacter = GetStandardCharacterId("2CH", BookOrChapter);
			var blockC20 = new Block("c", 20) { CharacterId = chapterCharacter };
			var block1 = new Block("p", 20, 1) {IsParagraphStart = true}
				.AddVerse(1, "After this, the Moabites and others came to war against Jehoshaphat. ");
			var block2a = new Block("p", 20, 2) {IsParagraphStart = true}
				.AddVerse(2, "Some people came and told Jehoshaphat, “A vast army is coming against you from Edom, from the other side of the Dead Sea. Hazezon Tamar ");
			var blockInt2b = new Block("qt-s", 20, 2) { CharacterId = narrator }
				.AddText("(that is, En Gedi) ");
			if (includeQuoteIdsForInterruption)
			{
				blockInt2b.BlockElements.Insert(0, new QuoteId {Id = "i1", Start = true, IsNarrator = true});
				blockInt2b.BlockElements.Add(new QuoteId {Id = "i1", Start = false, IsNarrator = true});
			}
			var block2c3 = new Block("p", 20, 2)
				.AddText("is where they are currently camped.” ")
				.AddVerse(3, "Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: “All Judah must fast.” ");

			var input = new List<Block> { blockC20, block1, block2a, blockInt2b, block2c3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "2CH", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{2}\u00A0Some people came and told Jehoshaphat, "));

			Assert.That(output[++i].CharacterId, Is.EqualTo("men, some"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("“A vast army is coming " +
				"against you from Edom, from the other side of the Dead Sea. Hazezon Tamar "));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(blockInt2b.GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("men, some"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(
				"is where they are currently camped.” "));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(
				"{3}\u00A0Alarmed, Jehoshaphat resolved to inquire of the Lord, and he proclaimed: "));

			Assert.That(output[++i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[i].CharacterIdOverrideForScript, Is.Null);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("“All Judah must fast.” "));

			Assert.That(output.Count, Is.EqualTo(++i));
		}

		[Test]
		public void Parse_InterruptionInMultiBlockExplicitQuote_QuoteCharacterAndInterruptionPreserved()
		{
			var narrator = GetStandardCharacterId("MAT", Narrator);
			var chapterCharacter = GetStandardCharacterId("MAT", BookOrChapter);
			var blockC24 = new Block("c", 24) { CharacterId = chapterCharacter };
			var blockV4a = new Block("p", 24, 4) { IsParagraphStart = true }
				.AddVerse(4, "Jesus answered: ");
			var blockV4thru8 = new Block("qt-s", 24, 4) { IsParagraphStart = true, CharacterId = "Jesus" }
				.AddText("“Watch out ")
				.AddVerse(5, "People gonna mess wit you. ")
				.AddVerse(6, "You are gonna hear crazy stuff. ")
				.AddVerse(7, "Nations will go to war. ")
				.AddVerse(8, "That is just the start!");
			var blockV9thru14 = new Block("p", 24, 9) { IsParagraphStart = true, CharacterId = "Jesus" }
				.AddVerse("9-11", "“People are going to kill you and hate you ")
				.AddVerse("12-14", "And everyone will hear the gospel");
			var blockV15a = new Block("p", 24, 15) { IsParagraphStart = true, CharacterId = "Jesus" }
				.AddVerse("15", "“So when you see ‘the abomination that causes desolation,’ talked about by Daniel");
			var blockInt15 = new Block("qt2-s", 24, 15) { CharacterId = narrator }
				.AddText("—let the reader understand— ");
			var blockV16thru21 = new Block("qt1-s", 24, 16) { CharacterId = "Jesus" }
				.AddVerse(16, "then let those who are in Judea flee ")
				.AddVerse("17-21", "Do not go back for anything and realize it is going to be tough.")
				.AddEndQuoteId();

			var input = new List<Block> {
				blockC24, blockV4a, blockV4thru8, blockV9thru14, blockV15a, blockInt15,
				blockV16thru21 };

			var output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input,
					QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));
			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo("Jesus"));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(output[i].CharacterId, Is.EqualTo(output[i].CharacterIdInScript));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo(narrator));
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output[++i].CharacterIdInScript, Is.EqualTo("Jesus"));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].IsPredeterminedQuoteInterruption, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo(input[i].GetText(true, true)));

			Assert.That(output.Count, Is.EqualTo(++i));
		}

		[TestCase("God", true, true)]
		[TestCase("God", false)]
		[TestCase("Jeremiah", true)]
		[TestCase("Jeremiah", false, true)]
		[TestCase(kAmbiguousCharacter, true)]
		[TestCase(kAmbiguousCharacter, false)]
		[TestCase(null, false, true)]
		[TestCase(null, false)]
		public void Parse_PredeterminedLevel1QuoteInsideParserDetectedLevel1Quote_FolllowingBlockNeedsReview(
			string character, bool explicitLevel1, bool includeQuoteId = false)
		{
			var narrator = GetStandardCharacterId("JER", Narrator);
			var chapterCharacter = GetStandardCharacterId("JER", BookOrChapter);
			const string quid = "actually_level_2";
			var blockC1 = new Block("c", 1) { CharacterId = chapterCharacter };
			var blockV6 = new Block("p", 1, 6) {IsParagraphStart = true}
				.AddVerse(6, "“Alas, Sovereign Lord,” I said, “I do not know how to speak; I am too young.” ");
			var blockV7a = new Block("p", 1, 7) {IsParagraphStart = true}
				.AddVerse(7, "But the Lord said to me, “Do not say, ");
			var blockV7b = new Block(explicitLevel1 ? "qt1-s" : "qt-s", 1, 7) { CharacterId = character }
				.AddText("‘I am too young.’ ")
				.AddEndQuoteId(includeQuoteId ? quid : null);
			if (includeQuoteId)
				blockV7b.BlockElements.Insert(0, new QuoteId {Id = quid, Start = true});
			var blockV7c = new Block("p", 1, 7)
				.AddText("You must go to everyone I send you to and say whatever I command you. ")
				.AddVerse(8, "Do not be afraid of them, for I am with you and will rescue you,” " +
					"declares the Lord.");
			var blockV9 = new Block("p", 1, 9) {IsParagraphStart = true}
				.AddVerse(9, "Then the Lord reached out his hand and touched my mouth and said to " +
					"me, “I have put my words in your mouth. ")
				.AddVerse(10, "See, today I appoint you over nations and kingdoms to uproot and " +
					"tear down, to destroy and overthrow, to build and to plant.”");

			var input = new List<Block> { blockC1, blockV6, blockV7a, blockV7b, blockV7c, blockV9 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JER", input,
				QuoteSystemForStandardAmericanEnglish).Parse().ToList();
			
			Assert.That(output.All(b => b.MultiBlockQuote == MultiBlockQuote.None),
				"Even if this results in two adjacent blocks with the same speaker, they should not" +
				" be treated as a multi-block quote.");

			Assert.That(output.All(b => !b.IsPredeterminedQuoteInterruption));

			var i = 0;
			Assert.That(output[i].CharacterId, Is.EqualTo(chapterCharacter));

			Assert.That(output[++i].CharacterId, Is.EqualTo("Jeremiah"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{6}\u00A0“Alas, Sovereign Lord,” "));

			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo("I said, "));
			
			Assert.That(output[++i].CharacterId, Is.EqualTo("Jeremiah"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(6));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true),
				Is.EqualTo("“I do not know how to speak; I am too young.” "));

			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{7}\u00A0But the Lord said to me, "));
			
			Assert.That(output[++i].CharacterId, Is.EqualTo("God"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo("“Do not say, "));

			Assert.That(output[++i].CharacterId, Is.EqualTo(character ?? "God"));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.True);
			Assert.That(output[i].GetText(true), Is.EqualTo("‘I am too young.’ "));
			var quoteAnnotation = (QuoteId)output[i].BlockElements.Last();
			Assert.That(quoteAnnotation.Id, Is.EqualTo(includeQuoteId ? quid : null));
			Assert.That(quoteAnnotation.Start, Is.False);
			Assert.That(quoteAnnotation.IsNarrator, Is.False);
			if (includeQuoteId)
			{
				quoteAnnotation = (QuoteId)output[i].BlockElements[0];
				Assert.That(quoteAnnotation.Id, Is.EqualTo(quid));
				Assert.That(quoteAnnotation.Start, Is.True);
				Assert.That(quoteAnnotation.IsNarrator, Is.False);
			}
			
			Assert.That(output[++i].CharacterId, Is.EqualTo(kNeedsReview));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(7));
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].GetText(true, true), Is.EqualTo(
				"You must go to everyone I send you to and say whatever I command you. " +
				"{8}\u00A0Do not be afraid of them, for I am with you and will rescue you,” " +
				"declares the Lord."));
			
			Assert.That(output[++i].CharacterId, Is.EqualTo(narrator));
			Assert.That(output[i].InitialStartVerseNumber, Is.EqualTo(9));
			Assert.That(output[i].IsParagraphStart, Is.True);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteStart, Is.False);
			Assert.That(output[i].IsPredeterminedFirstLevelQuoteEnd, Is.False);
			Assert.That(output[i].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
			Assert.That(output[i].GetText(true, true), Is.EqualTo("{9}\u00A0" +
				"Then the Lord reached out his hand and touched my mouth and said to me, "));

			Assert.That(output[++i].CharacterId, Is.EqualTo("God"));

			Assert.That(output.Count, Is.EqualTo(++i));
		}
		#endregion // PG-1419 tests (blocks created by quote milestones)

		#region Tests related to NVI-S for PTX-22920
		
		/// <summary>
		/// Although the official NVI-S resource has the same character for the start and end
		/// dialogue dash (―), this resource appears never to use it as an ending marker. But
		/// it does use the double right guillemets (») to explicitly mark the end of a multi-
		/// paragraph dialogue quotation. So I'm using that as the end marker and have written
		/// this test accordingly.
		/// </summary>
		private QuoteSystem GetNviQuoteSystem()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal), "\u2015", "»");
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "”", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "’", 3, QuotationMarkingSystemType.Normal));
			quoteSystem.SetReportingClauseDelimiters("\u2014");
			return quoteSystem;
		}

		private QuoteSystem GetUsQuoteSystem() =>
			QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark(
				"“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);

		[Test]
		public void Parse_DialogueQuoteWithMultipleParagraphsHavingEndQuoteAsContinuers_EndedByFirstLevelEnd()
		{
			var block0 = new Block("p", 2, 27) {IsParagraphStart = true};
			block0.BlockElements.Add(new Verse("27"));
			block0.BlockElements.Add(new ScriptText("Daniel respondió: "));
			var block1 = new Block("p", 2, 27) {IsParagraphStart = true};
			block1.BlockElements.Add(new ScriptText("\u2015No hay ningún mago que pueda explicarle el misterio. "));
			block1.BlockElements.Add(new Verse("28-30"));
			block1.BlockElements.Add(new ScriptText("Pero hay much que decir. "));
			var block2 = new Block("p", 2, 31) {IsParagraphStart = true};
			block2.BlockElements.Add(new Verse("31"));
			block2.BlockElements.Add(new ScriptText("»En su sueño Su Majestad veía una estatua enorme. "));
			block2.BlockElements.Add(new Verse("32-38"));
			block2.BlockElements.Add(new ScriptText("Yadda yadda. "));
			var block3 = new Block("p", 2, 39) {IsParagraphStart = true};
			block3.BlockElements.Add(new Verse("39"));
			block3.BlockElements.Add(new ScriptText("»Después de Su Majestad surgirá. "));
			block3.BlockElements.Add(new Verse("40"));
			block3.BlockElements.Add(new ScriptText("Finalmente, vendrá un cuarto reino, sólido como el hierro. "));
			var block4 = new Block("p", 2, 41) {IsParagraphStart = true};
			block4.BlockElements.Add(new Verse("41"));
			block4.BlockElements.Add(new ScriptText("»Su Majestad veía que los pies de la estatua eran de hierro y barro. "));
			block4.BlockElements.Add(new Verse("42-43"));
			block4.BlockElements.Add(new ScriptText("Su Majestad vio dos elementos que no pueden fundirse. El pueblo no será unida. "));
			var block5 = new Block("p", 2, 44) {IsParagraphStart = true};
			block5.BlockElements.Add(new Verse("44"));
			block5.BlockElements.Add(new ScriptText("»En los días Dios establecerá un reino permanente y hará pedazos a estos reinos. "));
			block5.BlockElements.Add(new Verse("45"));
			block5.BlockElements.Add(new ScriptText("Dios le ha mostrado el futuro. La interpretación es digna de confianza». "));
			var block6 = new Block("p", 2, 46) {IsParagraphStart = true};
			block6.BlockElements.Add(new Verse("46"));
			block6.BlockElements.Add(new ScriptText("Al oír esto, el rey ordenó que se le presentara una ofrenda, "));
			block6.BlockElements.Add(new Verse("47"));
			block6.BlockElements.Add(new ScriptText("y le dijo: "));
			var input = new List<Block> { block0, block1, block2, block3, block4, block5, block6 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "DAN",
				input, GetNviQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(7));

			Assert.That(output[0].GetText(false), Is.EqualTo("Daniel respondió: "));
			Assert.That(output[0].CharacterIs("DAN", Narrator), Is.True);

			Assert.That(output[1].GetText(false), Is.EqualTo(
				"―No hay ningún mago que pueda explicarle el misterio. Pero hay much que decir. "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Daniel"));

			Assert.That(output[2].GetText(false), Is.EqualTo(
				"»En su sueño Su Majestad veía una estatua enorme. Yadda yadda. "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Daniel"));

			Assert.That(output[3].GetText(false), Is.EqualTo("»Después de Su Majestad surgirá. " +
				"Finalmente, vendrá un cuarto reino, sólido como el hierro. "));
			Assert.That(output[3].CharacterId, Is.EqualTo("Daniel"));

			Assert.That(output[4].GetText(false), Is.EqualTo(
				"»Su Majestad veía que los pies de la estatua eran de hierro y barro. " +
				"Su Majestad vio dos elementos que no pueden fundirse. El pueblo no será unida. "));
			Assert.That(output[4].CharacterId, Is.EqualTo("Daniel"));

			Assert.That(output[5].GetText(false), Is.EqualTo(
				"»En los días Dios establecerá un reino permanente y hará pedazos a estos reinos. " +
				"Dios le ha mostrado el futuro. La interpretación es digna de confianza». "));
			Assert.That(output[5].CharacterId, Is.EqualTo("Daniel"));

			Assert.That(output[6].GetText(false), Is.EqualTo(
				"Al oír esto, el rey ordenó que se le presentara una ofrenda, y le dijo: "));
			Assert.That(output[6].CharacterIs("DAN", Narrator), Is.True);
		}

		[Test]
		public void Parse_DialogueDashFollowedByParagraphStartingANewDialogueSpeakerWithReportingClauseInterruptions_SpeakerChangeNotedCorrectly()
		{
			var block1 = new Block("p", 5, 3);
			block1.BlockElements.Add(new Verse("3"));
			block1.BlockElements.Add(new ScriptText("El rey le preguntó:"));
			block1.IsParagraphStart = true;
			var block2 = new Block("p", 5, 3);
			block2.BlockElements.Add(new ScriptText("\u2015¿Qué te pasa, Ester? ¿Cuál es tu petición? ¡Aun la mitad del reino te concedería! "));
			var block3 = new Block("p", 5, 4);
			block3.BlockElements.Add(new Verse("4"));
			block3.BlockElements.Add(new ScriptText("\u2015Si le parece bien \u2014respondió Ester\u2014, vengan al banquete que ofrezco. "));
			var block4 = new Block("p", 5, 5);
			block4.BlockElements.Add(new Verse("5"));
			block4.BlockElements.Add(new ScriptText("\u2015Traigan a Amán, para poder cumplir con su deseo \u2014ordenó el rey. "));
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "EST",
				input, GetNviQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(7));
			Assert.That(output.All(b => b.MultiBlockQuote == MultiBlockQuote.None));

			Assert.That(output[0].GetText(false), Is.EqualTo("El rey le preguntó:"));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);

			Assert.That(output[1].GetText(true), Is.EqualTo("\u2015¿Qué te pasa, Ester? ¿Cuál es tu petición? ¡Aun la mitad del reino te concedería! "));
			Assert.That(output[1].CharacterId, Is.EqualTo("Xerxes, king of Persia and Media (Ahasuerus)"));

			Assert.That(output[2].GetText(false), Is.EqualTo("\u2015Si le parece bien "));
			Assert.That(output[2].CharacterId, Is.EqualTo("Esther, queen"));

			Assert.That(output[3].GetText(false), Is.EqualTo("\u2014respondió Ester\u2014, "));
			Assert.That(IsCharacterOfType(output[3].CharacterId, Narrator), Is.True);

			Assert.That(output[4].GetText(true), Is.EqualTo("vengan al banquete que ofrezco. "));
			Assert.That(output[4].CharacterId, Is.EqualTo("Esther, queen"));

			Assert.That(output[5].GetText(false), Is.EqualTo("\u2015Traigan a Amán, para poder cumplir con su deseo "));
			Assert.That(output[5].CharacterId, Is.EqualTo("Xerxes, king of Persia and Media (Ahasuerus)"));

			Assert.That(output[6].GetText(false), Is.EqualTo("\u2014ordenó el rey. "));
			Assert.That(IsCharacterOfType(output[6].CharacterId, Narrator), Is.True);
		}

		[Test]
		public void Parse_DialogueDashFollowedByContinuationParagraphWithReportingClauseandSentencePunctBeforeVerseNumber_ReportingClauseBrokenOut()
		{
			var block1 = new Block("p", 32, 7);
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("El Señor le dijo a Moisés: "));
			block1.IsParagraphStart = true;
			var block2 = new Block("p", 32, 7);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new ScriptText("\u2015Baja. Se ha corrompido Israel. "));
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("No solo han hecho un ídolo, sino que le han " +
				"ofrecido sacrificios y han declarado: “Israel, ¡he aquí tu dios que te liberó!” "));
			var block3 = new Block("p", 32, 9);
			block3.IsParagraphStart = true;
			block3.BlockElements.Add(new Verse("9"));
			block3.BlockElements.Add(new ScriptText("»Este es un pueblo terco —añadió el Señor, " +
				"dirigiéndose a Moisés—. "));
			block3.BlockElements.Add(new Verse("10"));
			block3.BlockElements.Add(new ScriptText("Los voy a destruir en mi ira. " +
				"Pero de ti haré una nación». "));
			var input = new List<Block> { block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "EXO",
				input, GetNviQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(5));

			Assert.That(output[0].GetText(false), Is.EqualTo("El Señor le dijo a Moisés: "));
			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].GetText(false), Is.EqualTo(
				"\u2015Baja. Se ha corrompido Israel. No solo han hecho un ídolo, " +
				"sino que le han ofrecido sacrificios y han declarado: “Israel, ¡he aquí tu " +
				"dios que te liberó!” "));
			Assert.That(output[1].CharacterId, Is.EqualTo("God"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(output[2].GetText(false), Is.EqualTo("»Este es un pueblo terco "));
			Assert.That(output[2].CharacterId, Is.EqualTo("God"));
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[3].GetText(true), Is.EqualTo("—añadió el Señor, dirigiéndose a Moisés—. "));
			Assert.That(IsCharacterOfType(output[3].CharacterId, Narrator), Is.True);
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[4].GetText(false), Is.EqualTo("Los voy a destruir en mi ira. Pero de ti haré una nación». "));
			Assert.That(output[4].CharacterId, Is.EqualTo("God"));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_PiStyleWithoutQuotesCorrespondsToVersesWithExpectedSpeaker_PiBlocksTreatedAsQuote(bool includeFollowingPara)
		{
			var block1 = new Block("p", 1, 27);
			block1.BlockElements.Add(new Verse("27"));
			block1.BlockElements.Add(new ScriptText("God created humans in his image, both men and women. "));
			block1.BlockElements.Add(new Verse("28"));
			block1.BlockElements.Add(new ScriptText("He blessed them, saying: "));
			block1.IsParagraphStart = true;
			var block2 = new Block("pi", 1, 29);
			block2.IsParagraphStart = true;
			block2.BlockElements.Add(new Verse("29"));
			block2.BlockElements.Add(new ScriptText("I have provided food for you to eat. "));
			block2.BlockElements.Add(new Verse("30"));
			// Note that in the CEV, the trailing comment "An so it was" is (incorrectly) included in
			// the \pi block (spoken by God).
			block2.BlockElements.Add(new ScriptText("Eat plants like the animals. And so it was. "));
			var input = new List<Block> { block1, block2 };

			if (includeFollowingPara)
			{
				var block3 = new Block("p", 1, 31);
				block3.IsParagraphStart = true;
				block3.BlockElements.Add(new Verse("31"));
				block3.BlockElements.Add(new ScriptText("God looked at all the very good stuff he did. "));
				input.Add(block3);
			}

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN",
				input, GetUsQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));

			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].CharacterId, Is.EqualTo("God"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output.Skip(2).All(b => IsCharacterOfType(b.CharacterId, Narrator) &&
				b.MultiBlockQuote == MultiBlockQuote.None));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Parse_PiStyleWithoutQuotesCorrespondsToVersesWithExpectedSpeakerAndDelivery_PiBlocksTreatedAsQuote(bool includeFollowingPara)
		{
			var block1 = new Block("p", 24, 11) { IsParagraphStart = true}
			.AddVerse(11, "When he got there, ")
			.AddVerse(12, "he prayed: ");
			var block2 = new Block("pi", 24, 12) { IsParagraphStart = true}
			.AddText("You, are the God my master Abraham worships. ")
			.AddVerse(13, "The young women of the city will soon come. ")
			.AddVerse(14, "help me get a good one. ");
			var input = new List<Block> { block1, block2 };

			if (includeFollowingPara)
			{
				var block3 = new Block("p", 1, 15, 16) { IsParagraphStart = true}
				.AddVerse("15-16", "While he was praying, a beautiful woman came with a jar. ");
				input.Add(block3);
			}

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN",
				input, GetUsQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));

			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].CharacterId, Is.EqualTo("Abraham's chief servant"));
			Assert.That(output[1].Delivery, Is.EqualTo("praying"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output.Skip(2).All(b => IsCharacterOfType(b.CharacterId, Narrator) &&
				b.MultiBlockQuote == MultiBlockQuote.None));
		}

		[Test]
		public void Parse_MultiplePiStyleParasWithSectionHeadInteruption_PiBlocksTreatedAsQuote()
		{
			var chapterCharacter = GetStandardCharacterId("ISA", BookOrChapter);
			var extraBiblical = GetStandardCharacterId("ISA", ExtraBiblical);

			var block1 = new Block("p", 7, 13) { IsParagraphStart = true }
				.AddVerse(13, "Then I said: ");
			var block2 = new Block("pi", 7, 13) { IsParagraphStart = true }
				.AddText("You have tried my patience and God's patience. ")
				.AddVerse(14, "But the Lord will still give you proof. ")
				.AddVerse("15-16", "Even before the boy is old, stuff will happen. ")
				.AddVerse("17", "God will even bring Assyria to attack you. ");
			var block3 = new Block("s1", 7, 17) { IsParagraphStart = true, CharacterId = extraBiblical}
				.AddText("The Threat of an Invasion ");
			var block4 = new Block("pi", 7, 18) { IsParagraphStart = true }
				.AddVerse("18", "When that time comes, God will whistle. ")
				.AddVerse("19", "They will settle everywhere—in valleys and between rocks. ");
			var block5 = new Block("pi", 7, 20) { IsParagraphStart = true }
				.AddVerse("20-25", "The Lord will pay the king of Assyria. And goats will be loose.");
			var blockC8 = new Block("c", 8) { CharacterId = chapterCharacter };
			var blockC81 = new Block("s1", 8, 0) { IsParagraphStart = true, CharacterId = extraBiblical }
				.AddText("A Warning and a Hope ");
			var blockC82 = new Block("p", 8, 1) { IsParagraphStart = true }
				.AddVerse(1, "The Lord said... ");
			var input = new List<Block> { block1, block2, block3, block4, block5, blockC8, blockC81, blockC82 };

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA",
				input, GetUsQuoteSystem()).Parse().ToList();
			Assert.That(output.Count, Is.EqualTo(input.Count));

			Assert.That(IsCharacterOfType(output[0].CharacterId, Narrator), Is.True);
			Assert.That(output[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[1].CharacterId, Is.EqualTo("Isaiah"));
			Assert.That(output[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));

			Assert.That(IsCharacterOfType(output[2].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[2].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(output[3].CharacterId, Is.EqualTo("Isaiah"));
			Assert.That(output[3].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(output[4].CharacterId, Is.EqualTo("Isaiah"));
			Assert.That(output[4].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			Assert.That(IsCharacterOfType(output[5].CharacterId, BookOrChapter), Is.True);
			Assert.That(output[5].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(IsCharacterOfType(output[6].CharacterId, ExtraBiblical), Is.True);
			Assert.That(output[6].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));

			Assert.That(IsCharacterOfType(output[7].CharacterId, Narrator), Is.True);
			Assert.That(output[7].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.None));
		}

		[Test]
		public void Parse_ImplicitWithPotentialSelfQuote_TreatedAsQuote()
		{
			//Assert.Fail("Write a test based on ESV16 to see why ISA 41:9 is getting marked as Needs Review.");
			var chapterCharacter = GetStandardCharacterId("ISA", BookOrChapter);
			var extraBiblical = GetStandardCharacterId("ISA", ExtraBiblical);

			var input = new List<Block>
			{
				new Block("c", 41) { CharacterId = chapterCharacter },
				new Block("q1", 41, 8) { IsParagraphStart = true }
					.AddVerse(8, "But you, Israel, my servant, "),
				new Block("q2", 41, 8) { IsParagraphStart = true }
					.AddText("Jacob, whom I chose, the descendent of my frind Abraham, "),
				new Block("q1", 41, 9) { IsParagraphStart = true }
					.AddVerse(9, "you whom I took from the ends of the earth, "),
				new Block("q1", 41, 9) { IsParagraphStart = true }
					.AddText("telling you, “You are my servant, "),
				new Block("q2", 41, 9) { IsParagraphStart = true }
					.AddText("I have chosen you and not cast you off”; "),
				new Block("q1", 41, 10) { IsParagraphStart = true }
					.AddVerse(10, "fear not, for I am with you. "),
			};
			
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ISA",
				input, GetUsQuoteSystem()).Parse().ToList();

			Assert.That(output.All(b => b.CharacterId != kNeedsReview), Is.True);
		}
		#endregion
	}
}
