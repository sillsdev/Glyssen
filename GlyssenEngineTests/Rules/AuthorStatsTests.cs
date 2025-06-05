using System.Collections.Generic;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Rules;
using NUnit.Framework;

namespace GlyssenEngineTests.Rules
{
	[TestFixture]
	class AuthorStatsTests
	{
		[Test]
		public void Constructor_BooksWithDifferentAuthors_NoStatsCombined()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("HOS", CharacterVerseData.StandardCharacter.Narrator)] = 12000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)] = 1000;
			var includedBooks = new List<string> { "JER", "EZK", "HOS", "JUD" };

			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(52000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(48000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("HOS"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(12000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JUD"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(1000));
		}

		[Test]
		public void Constructor_BooksWithSameAuthors_StatsCombinedForBooksWithSameAuthors()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("GEN", CharacterVerseData.StandardCharacter.Narrator)] = 50000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ISA", CharacterVerseData.StandardCharacter.Narrator)] = 66000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JER", CharacterVerseData.StandardCharacter.Narrator)] = 52000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("EZK", CharacterVerseData.StandardCharacter.Narrator)] = 48000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // 52000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("JHN", CharacterVerseData.StandardCharacter.Narrator)] = 20000; // 42000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("REV", CharacterVerseData.StandardCharacter.Narrator)] = 22000;
			var includedBooks = new List<string> { "GEN", "ISA", "JER", "EZK", "LUK", "ACT", "JHN", "REV" };

			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("GEN"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(50000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ISA"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(66000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(52000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(48000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(52000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ACT"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(52000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JHN"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(42000));
			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("REV"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(42000));
		}

		[Test]
		public void Constructor_AuthorHasNoIncludedBooks_KeyStrokeCountReturnsZero()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // 52000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			var includedBooks = new List<string> { "GEN", "ISA", "JER", "JHN", "REV" };

			Assert.That(new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), includedBooks, keyStrokesByBook).KeyStrokeCount, Is.EqualTo(0));
		}
	}
}
