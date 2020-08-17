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

			var characters = cvRepo.GetCharacters(45, 2, new Verse("17"));

			Assert.True(characters.Count == 2);
			Assert.That(characters.Any(i => i.Character == "narrator-ROM"));
			Assert.That(characters.Any(i => i.Character == "you (hypothetical)"));
		}
	}
}
