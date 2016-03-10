using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using Rhino.Mocks;
using SIL.Scripture;
using SIL.Xml;
using ScrVers = Paratext.ScrVers;

namespace GlyssenTests.Character
{
	[TestFixture]
	public class CharacterAssignerTests
	{
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
</book>";

			return XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
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

			return XmlSerializationHelper.DeserializeFromString<BookScript>(bookScript);
		}

		[Test]
		public void AssignAll_SetDefaultForMultipleChoiceCharactersFalseOverwriteUserConfirmedFalse_OverwritesOnlyUnconfirmedBlocks()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "King Saul", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Jesus", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, false, false);
			Assert.AreEqual("King Saul", bookScript[1].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void AssignAll_OverwriteUserConfirmedTrue_OverwritesAll()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "King Saul", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, false, true);
			Assert.AreEqual("John the Baptist", bookScript[1].CharacterId);
			Assert.AreEqual("King Saul", bookScript[2].CharacterId);
		}

		[Test]
		public void AssignAll_Overwriting_ControlFileHasMultipleChoiceCharacters_SetsImplicitOrExplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "James/John", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, true, true);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[1].CharacterId);
			Assert.AreEqual("Andrew", bookScript[1].CharacterIdInScript);
			Assert.AreEqual("James/John", bookScript[2].CharacterId);
			Assert.AreEqual("James", bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void AssignAll_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileDoesNotHaveExplicitDefault_SetsImplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterIdInScript);

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Made Up Guy", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters(41, 1, 5, 0, versification: ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Thomas/Andrew/Bartholomew", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, true, false);
			Assert.AreEqual("Made Up Guy", bookScript[1].CharacterId);
			Assert.AreEqual("Made Up Guy", bookScript[1].CharacterIdInScript);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterId);
			Assert.AreEqual("Thomas", bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void AssignAll_NotOverwriting_SetDefaultForMultipleChoiceCharactersTrue_ControlFileHasExplicitDefault_SetsExplicitDefault()
		{
			var bookScript = GetSimpleBookScript();
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterId);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterIdInScript);

			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "Made Up Guy", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters(41, 1, 5, 0, versification: ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "Thomas/Andrew/Bartholomew", null, null, false, QuoteType.Normal, "Andrew") });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, true, false);
			Assert.AreEqual("Thomas/Andrew/Bartholomew", bookScript[2].CharacterId);
			Assert.AreEqual("Andrew", bookScript[2].CharacterIdInScript);
		}

		[Test]
		public void AssignAll_BlockIsStandardCharacter_DoesNotOverwrite()
		{
			var bookScript = GetSimpleBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "John the Baptist", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "King Saul", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, false, true);
			Assert.AreEqual("narrator-MRK", bookScript[0].CharacterId);
		}

		[Test]
		public void AssignAll_ContainsMultiBlockQuoteWithMoreThanOneCharacter_PerformsCleanUpByAssigningBlockToAmbiguousAndUserConfirmedToFalse()
		{
			var bookScript = GetMultiBlockBookScript();
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 4, 0, 4, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 4), "irrelevant", null, null, false) });
			cvInfo.Stub(x => x.GetCharacters("MRK", 1, 5, 0, 5, ScrVers.English)).Return(new[] { new CharacterVerse(new BCVRef(41, 1, 5), "irrelevant", null, null, false) });
			new CharacterAssigner(cvInfo).AssignAll(new[] { bookScript }, ScrVers.English, false, false);
			Assert.AreEqual("Ambiguous", bookScript[0].CharacterId);
			Assert.AreEqual("Ambiguous", bookScript[1].CharacterId);
			Assert.AreEqual("firstCharacter", bookScript[2].CharacterId);
			Assert.AreEqual("firstCharacter", bookScript[3].CharacterId);
			Assert.False(bookScript[0].UserConfirmed);
			Assert.False(bookScript[1].UserConfirmed);
			Assert.True(bookScript[2].UserConfirmed);
			Assert.True(bookScript[3].UserConfirmed);
		}

		[Test]
		public void AssignAll_MultiverseQuoteWithTwoCharactersInFirstVerseAndOneCharacterInSecond_AssignedToCharacterRatherThanAmbiguous()
		{
			var cvInfo = MockRepository.GenerateMock<ICharacterVerseInfo>();
			cvInfo.Stub(x => x.GetCharacters("MAT", 17, 26, 0, 26, ScrVers.English)).Return(new[]
			{
				new CharacterVerse(new BCVRef(40, 17, 26), "Peter (Simon)", null, null, false),
				new CharacterVerse(new BCVRef(40, 17, 26), "Jesus", null, null, false),
			});
			cvInfo.Stub(x => x.GetCharacters("MAT", 17, 27, 0, 27, ScrVers.English)).Return(new[]
			{
				new CharacterVerse(new BCVRef(40, 17, 27), "Jesus", null, null, false)
			});

			var bookScript = new BookScript
			{
				BookId = "MAT",
				Blocks = new List<Block>
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
				}
			};

			Assert.AreEqual("Jesus", bookScript.Blocks[0].CharacterId);
			Assert.AreEqual("Jesus", bookScript.Blocks[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, bookScript.Blocks[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, bookScript.Blocks[1].MultiBlockQuote);

			var characterAssigner = new CharacterAssigner(cvInfo);
			characterAssigner.AssignAll(new[] { bookScript }, ScrVers.English, false);

			Assert.AreEqual("Jesus", bookScript.Blocks[0].CharacterId);
			Assert.AreEqual("Jesus", bookScript.Blocks[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Start, bookScript.Blocks[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, bookScript.Blocks[1].MultiBlockQuote);
		}

		[Test]
		public void AssignAll_FreshlyParsedProject_AssignAllChangesNothing()
		{
			var booksToIncludeInTestProject = Enum.GetValues(typeof(TestProject.TestBook)).Cast<TestProject.TestBook>().ToArray();
			var freshTestProject = TestProject.CreateTestProject(booksToIncludeInTestProject);
			var testProjectToAssign = TestProject.CreateTestProject(booksToIncludeInTestProject);

			var characterAssigner = new CharacterAssigner(ControlCharacterVerseData.Singleton);
			characterAssigner.AssignAll(testProjectToAssign.Books.ToList(), ScrVers.English, false);

			var expected = freshTestProject.Books;
			var actual = testProjectToAssign.Books;
			Assert.AreEqual(expected.Count, actual.Count);

			for (var i = 0; i < expected.Count; i++)
			{
				var expectedBlocks = expected[i].Blocks;
				var actualBlocks = actual[i].Blocks;

				// both books contains the same number of blocks
				Assert.AreEqual(expectedBlocks.Count, actualBlocks.Count);

				for (var j = 0; j < expectedBlocks.Count; j++)
				{
					// both blocks contain the same number of elements
					Assert.AreEqual(expectedBlocks[j].BlockElements.Count, actualBlocks[j].BlockElements.Count);
					Assert.AreEqual(expectedBlocks[j].GetText(true), actualBlocks[j].GetText(true));
				}
			}
		}
	}
}
