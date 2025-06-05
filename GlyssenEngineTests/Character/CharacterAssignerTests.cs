using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using GlyssenEngine.Script;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.Scripture;
using SIL.WritingSystems;
using SIL.Xml;

namespace GlyssenEngineTests.Character
{
	[TestFixture]
	public class CharacterAssignerTests
	{
		private static readonly int kMATbookNum = BCVRef.BookToNumber("MAT");
		private static readonly int kMRKbookNum = BCVRef.BookToNumber("MRK");
		private IQuoteInterruptionFinder m_interruptionFinder;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			m_interruptionFinder = new QuoteSystem(new QuotationMark("—", "—", null, 1, QuotationMarkingSystemType.Narrative));
		}

		private BookScript GetSimpleBookScript()
		{
			const string bookScript = @"
<book id=""MRK"">
	<block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""narrator-MRK"" userConfirmed=""false"">
		<verse num=""4"" />
		<text>Mantsa tama, ka zlagaptá Yuhwana, mnda maga Batem ma mtak, kaʼa mantsa: </text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""Made Up Guy"" userConfirmed=""false"">
		<text>«Mbəɗanafwa mbəɗa ta nzakwa ghuni, ka magaghunafta lu ta batem, ka plighunista Lazglafta ta dmakuha ghuni,» </text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""5"" characterId=""Thomas/Andrew/Bartholomew"" userConfirmed=""true"">
		<text>«Gobbledy-gook» </text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""6"" characterId=""Soup"" userConfirmed=""true"">
		<text>«Blah blah blah» </text>
	</block>
</book>";

			var newBook = XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
			newBook.Initialize(ScrVers.English);
			return newBook;
		}

		private BookScript GetMultiBlockBookScript()
		{
			const string bookScript = @"
<book id=""MRK"">
	<block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""firstCharacter"" userConfirmed=""true"" multiBlockQuote=""Start"">
		<verse num=""4"" />
		<text>1 </text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""4"" characterId=""secondCharacter"" userConfirmed=""true"" multiBlockQuote=""Continuation"">
		<text>2</text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""5"" characterId=""firstCharacter"" userConfirmed=""true"" multiBlockQuote=""Start"">
		<verse num=""5"" />
		<text>3 </text>
	</block>
	<block style=""p"" chapter=""1"" initialStartVerse=""5"" characterId=""firstCharacter"" userConfirmed=""true"" multiBlockQuote=""Continuation"">
		<text>4</text>
	</block>
</book>";

			var newBook = XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
			newBook.Initialize(ScrVers.English);
			return newBook;
		}

		[TestCase()]
		[TestCase(true)]
		public void AssignAll_SetDefaultForMultipleChoiceCharactersFalseOverwriteUserConfirmedFalse_OverwritesOnlyUnconfirmedBlocks(
			bool preConfirmed = false)
		{
			var bookScript = GetSimpleBookScript();
			if (preConfirmed)
				foreach (var block in bookScript.Blocks.Where(b => b.UserConfirmed))
				{
					block.StyleTag = "qt-s";
					block.UserConfirmed = false;
				}

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "King Saul");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, "Jesus");
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, false);
			Assert.That(bookScript[1].CharacterId, Is.EqualTo("King Saul"));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("Thomas/Andrew/Bartholomew"));
		}

		[Test]
		public void AssignAll_OverwriteUserConfirmedTrue_OverwritesAll()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "John the Baptist");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, "King Saul");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 6, ScrVers.English, new CharacterSpeakingMode[0]);
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, true);
			Assert.That(bookScript[1].CharacterId, Is.EqualTo("John the Baptist"));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("King Saul"));
		}

		[Test]
		public void AssignAll_Overwriting_ControlFileHasMultipleChoiceCharacters_SetsImplicitOrExplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, new[] { new CharacterSpeakingMode("Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") });
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, "James/John");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 6, ScrVers.English, new CharacterSpeakingMode[0]);
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, true, true);
			Assert.That(bookScript[1].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[1].CharacterIdInScript, Is.EqualTo("Andrew"));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("James/John"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("James"));
		}

		[Test]
		public void AssignAll_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileDoesNotHaveExplicitDefault_SetsImplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("Thomas/Andrew/Bartholomew"));

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "Made Up Guy");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English,
				new[] { new CharacterSpeakingMode("Thomas/Andrew/Bartholomew", null, null, false) }, true);
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, true, false);
			Assert.That(bookScript[1].CharacterId, Is.EqualTo("Made Up Guy"));
			Assert.That(bookScript[1].CharacterIdInScript, Is.EqualTo("Made Up Guy"));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("Thomas"));
		}

		[Test]
		public void AssignAll_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileHasExplicitDefault_SetsExplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("Thomas/Andrew/Bartholomew"));

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "Made Up Guy");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English,
				new[] { new CharacterSpeakingMode("Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") }, true);
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, true, false);
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Thomas/Andrew/Bartholomew"));
			Assert.That(bookScript[2].CharacterIdInScript, Is.EqualTo("Andrew"));
		}

		[Test]
		public void AssignAll_BlockIsStandardCharacter_DoesNotOverwrite()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "John the Baptist");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, "King Saul");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 6, ScrVers.English, new CharacterSpeakingMode[0]);
			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, true);
			Assert.That(bookScript[0].CharacterId, Is.EqualTo("narrator-MRK"));
		}

		[Test]
		public void AssignAll_ContainsSimpleQuoteWithNoCharactersAndSimpleQuoteWithMoreThanOneCharacter_PerformsCleanUpByAssigningBlocksToUnknownAndAmbiguousAndUserConfirmedToFalse()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, new CharacterSpeakingMode[0]);
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, new[]
			{
				new CharacterSpeakingMode("John the Baptist", null, null, false),
				new CharacterSpeakingMode("King Saul", null, null, false)
			});
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 6, ScrVers.English, new CharacterSpeakingMode[0]);

			Assert.That(bookScript[2].UserConfirmed, Is.True);
			Assert.That(bookScript[3].UserConfirmed, Is.True);

			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, true);
			Assert.That(bookScript[2].CharacterId, Is.EqualTo(CharacterVerseData.kAmbiguousCharacter));
			Assert.That(bookScript[2].UserConfirmed, Is.False);
			Assert.That(bookScript[3].CharacterId, Is.EqualTo(CharacterVerseData.kUnexpectedCharacter));
			Assert.That(bookScript[3].UserConfirmed, Is.False);
		}

		[Test]
		public void AssignAll_ContainsMultiBlockQuoteWithMoreThanOneCharacter_PerformsCleanUpByAssigningBlockToAmbiguousAndUserConfirmedToFalse()
		{
			var bookScript = GetMultiBlockBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 4, ScrVers.English, "irrelevant");
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 1, 5, ScrVers.English, "irrelevant");

			Assert.That(bookScript[0].UserConfirmed, Is.True);
			Assert.That(bookScript[1].UserConfirmed, Is.True);

			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, false);
			Assert.That(bookScript[0].CharacterId, Is.EqualTo(CharacterVerseData.kAmbiguousCharacter));
			Assert.That(bookScript[1].CharacterId, Is.EqualTo(CharacterVerseData.kAmbiguousCharacter));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("firstCharacter"));
			Assert.That(bookScript[3].CharacterId, Is.EqualTo("firstCharacter"));
			Assert.That(bookScript[0].UserConfirmed, Is.False);
			Assert.That(bookScript[1].UserConfirmed, Is.False);
			Assert.That(bookScript[2].UserConfirmed, Is.True);
			Assert.That(bookScript[3].UserConfirmed, Is.True);
		}

		/// <summary>
		/// PG-781
		/// </summary>
		[TestCase(ScrVersType.English)]
		[TestCase(ScrVersType.Original)]
		public void AssignAll_AssignedQuoteBlocksAndAmbiguousInterruptionBlock_NotModified(ScrVersType vers)
		{
			var versification = new ScrVers(vers);
			var bookScript = XmlSerializationHelper.DeserializeFromString<BookScript>(@"
				<book id=""MRK"">
					<block style=""p"" chapter=""13"" initialStartVerse=""14"" characterId=""Jesus"" userConfirmed=""false"" multiBlockQuote=""Start"">
						<verse num=""14"" />
						<text>Oona gwine see ‘De Horrible Bad Ting wa mek God place empty’ da stanop een de place weh e ain oughta dey. </text>
					</block>
					<block style=""p"" chapter=""13"" initialStartVerse=""14"" characterId=""Ambiguous"" userConfirmed=""false"">
						<text>(Leh oona wa da read ondastan wa dis mean.) </text>
					</block>
					<block style=""p"" chapter=""13"" initialStartVerse=""14"" characterId=""Jesus"" userConfirmed=""false"" multiBlockQuote=""Start"">
						<text>Wen dat time come, de people een Judea mus ron way quick ta de hill country. </text>
					</block>
				</book>");
			bookScript.Initialize(versification);
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMRKbookNum, 13, 14, versification, new[]
			{
				new CharacterSpeakingMode("Jesus", null, null, false, QuoteType.Normal),
				new CharacterSpeakingMode("narrator-MRK", null, null, false, QuoteType.Interruption)
			});

			Assert.That(bookScript.GetScriptBlocks().Any(b => b.UserConfirmed), Is.False);

			new CharacterAssigner(cvInfo, m_interruptionFinder).AssignAll(new[] { bookScript }, false, false);
			Assert.That(bookScript[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript[1].CharacterId, Is.EqualTo(CharacterVerseData.kAmbiguousCharacter));
			Assert.That(bookScript[2].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript.GetScriptBlocks().Any(b => b.UserConfirmed), Is.False);
		}

		[TestCase(ScrVersType.English)]
		[TestCase(ScrVersType.RussianOrthodox)]
		public void AssignAll_MultiverseQuoteWithTwoCharactersInFirstVerseAndOneCharacterInSecond_AssignedToCharacterRatherThanAmbiguous(ScrVersType vers)
		{
			var versification = new ScrVers(vers);
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			StubGetCharactersForSingleVerse(cvInfo, kMATbookNum, 17, 26, versification, new[]
			{
				new CharacterSpeakingMode("Peter (Simon)", null, null, false),
				new CharacterSpeakingMode("Jesus", null, null, false),
			});
			StubGetCharactersForSingleVerse(cvInfo, kMATbookNum, 17, 27, versification, new[]
			{
				new CharacterSpeakingMode("Jesus", null, null, false)
			});

			var bookScript = new BookScript("MAT",
				new List<Block>
				{
					new Block
					{
						ChapterNumber = 17,
						InitialStartVerseNumber = 26,
						BlockElements = new List<BlockElement>
						{
							new ScriptText("This quote starts in verse 26 ")
						},
						CharacterId = "Jesus",
						MultiBlockQuote = MultiBlockQuote.Start
					},
					new Block
					{
						ChapterNumber = 17,
						InitialStartVerseNumber = 27,
						BlockElements = new List<BlockElement>
						{
							new Verse("27"),
							new ScriptText("and continues in verse 27")
						},
						CharacterId = "Jesus",
						MultiBlockQuote = MultiBlockQuote.Continuation
					}
				},
				versification
			);

			Assert.That(bookScript.Blocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript.Blocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript.Blocks[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(bookScript.Blocks[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));

			var characterAssigner = new CharacterAssigner(cvInfo, m_interruptionFinder);
			characterAssigner.AssignAll(new[] { bookScript }, false);

			Assert.That(bookScript.Blocks[0].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript.Blocks[1].CharacterId, Is.EqualTo("Jesus"));
			Assert.That(bookScript.Blocks[0].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Start));
			Assert.That(bookScript.Blocks[1].MultiBlockQuote, Is.EqualTo(MultiBlockQuote.Continuation));
		}

		[Test]
		public void AssignAll_FreshlyParsedProject_AssignAllChangesNothing()
		{
			var booksToIncludeInTestProject = Enum.GetValues(typeof(TestProject.TestBook)).Cast<TestProject.TestBook>().ToArray();
			var freshTestProject = TestProject.CreateTestProject(booksToIncludeInTestProject);
			var testProjectToAssign = TestProject.CreateTestProject(booksToIncludeInTestProject);

			var characterAssigner = new CharacterAssigner(ControlCharacterVerseData.Singleton, m_interruptionFinder);
			characterAssigner.AssignAll(testProjectToAssign.Books.ToList(), false);

			var expected = freshTestProject.Books;
			var actual = testProjectToAssign.Books;
			Assert.That(actual.Count, Is.EqualTo(expected.Count));

			for (var i = 0; i < expected.Count; i++)
			{
				var expectedBlocks = expected[i].Blocks;
				var actualBlocks = actual[i].Blocks;

				Assert.That(actualBlocks.Count, Is.EqualTo(expectedBlocks.Count),
					"Both books should contain the same number of blocks");

				for (var j = 0; j < expectedBlocks.Count; j++)
				{
					Assert.That(actualBlocks[j].BlockElements.Count,
						Is.EqualTo(expectedBlocks[j].BlockElements.Count),
						"Both blocks should contain the same number of elements");
					Assert.That(actualBlocks[j].GetText(true),
						Is.EqualTo(expectedBlocks[j].GetText(true)));
				}
			}
		}

		private void StubGetCharactersForSingleVerse(ICharacterVerseInfo cvInfo, int bookNum, int chapter, int verse, ScrVers versification,
			string singleCharacterToReturn)
		{
			StubGetCharactersForSingleVerse(cvInfo, bookNum, chapter, verse, versification,
				new[] { new CharacterSpeakingMode(singleCharacterToReturn, null, null, false) });
		}

		private void StubGetCharactersForSingleVerse(ICharacterVerseInfo cvInfo, int bookNum, int chapter, int verse, ScrVers versification,
			CharacterSpeakingMode[] result, bool includeAlternatesAndRareQuotes = false)
		{
			cvInfo.Stub(x => x.GetCharacters(Arg.Is(bookNum), Arg.Is(chapter),
				Arg<IReadOnlyCollection<IVerse>>.Matches(a => a.Single().StartVerse == verse && a.Single().EndVerse == verse), Arg.Is(versification),
				Arg.Is(includeAlternatesAndRareQuotes), Arg.Is(false))).Return(new HashSet<CharacterSpeakingMode>(result));
		}
	}
}
