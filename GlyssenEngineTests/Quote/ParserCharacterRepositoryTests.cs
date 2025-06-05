using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenEngine.Quote;
using NUnit.Framework;
using Resources = GlyssenCharactersTests.Properties.Resources;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	public class ParserCharacterRepositoryTests
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
		}

		[TestCase(19, 10, "6", "man, wicked")]
		[TestCase(40, 7, "8", "Jesus")]
		public void ParserCharacterRepository_ReferenceTextIsNull_ReturnsSingleCharacterForVerse(int book, int chapter, string verse, string expectedCharacter)
		{
			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton, null);

			var characters = cvRepo.GetCharacters(book, chapter, new Verse(verse));

			Assert.That(characters.Single().ToString(), Is.EqualTo(expectedCharacter));
		}

		[Test]
		public void ParserCharacterRepository_ReferenceTextIsNull_ReturnsMultipleCharactersForVerse()
		{
			var cvRepo = new ParserCharacterRepository(ControlCharacterVerseData.Singleton, null);

			var characters = cvRepo.GetCharacters(44, 8, new Verse("37"));

			Assert.That(characters.Count == 2, Is.True);
			Assert.That(characters.Any(i => i.Character == "Ethiopian officer of Queen Candace"));
			Assert.That(characters.Any(i => i.Character == "Philip the evangelist"));
		}
	}
}
