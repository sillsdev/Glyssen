using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Quote;
using NUnit.Framework;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	public class ParserCharacterRepositoryTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerseOct2015;
		}

		[TestCase(19, 10, "6", "man, wicked")]
		[TestCase(40, 7, "8", "Jesus")]
		public void ParserCharacterRepository_ReferenceTextIsNull_ReturnsSingleCharacterForVerse(int book, int chapter, string verse, string expectedCharacter)
		{
			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton, null);

			var characters = cvRepo.GetCharacters(book, chapter, new Verse(verse));

			Assert.AreEqual(expectedCharacter, characters.Single().ToString());
		}

		[Test]
		public void ParserCharacterRepository_ReferenceTextIsNull_ReturnsMultipleCharactersForVerse()
		{
			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton, null);

			var characters = cvRepo.GetCharacters(44, 8, new Verse("37"));

			Assert.True(characters.Count == 2);
			Assert.That(characters.Any(i => i.Character == "Ethiopian officer of Queen Candace"));
			Assert.That(characters.Any(i => i.Character == "Philip the evangelist"));
		}
	}
}
