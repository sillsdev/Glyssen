using System.Collections.Generic;
using NUnit.Framework;
using Waxuquerque;
using Waxuquerque.Character;
using Waxuquerque.Rules;

namespace WaxuquerqueTests.Rules
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

			Assert.AreEqual(52000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(48000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(12000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("HOS"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(1000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JUD"), includedBooks, keyStrokesByBook).KeyStrokeCount);
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

			Assert.AreEqual(50000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("GEN"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(66000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ISA"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(52000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JER"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(48000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("EZK"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(52000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(52000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("ACT"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(42000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("JHN"), includedBooks, keyStrokesByBook).KeyStrokeCount);
			Assert.AreEqual(42000, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("REV"), includedBooks, keyStrokesByBook).KeyStrokeCount);
		}

		[Test]
		public void Constructor_AuthorHasNoIncludedBooks_KeyStrokeCountReturnsZero()
		{
			var keyStrokesByBook = new Dictionary<string, int>();
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator)] = 24000; // 52000 combined
			keyStrokesByBook[CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)] = 28000;
			var includedBooks = new List<string> { "GEN", "ISA", "JER", "JHN", "REV" };

			Assert.AreEqual(0, new AuthorStats(BiblicalAuthors.GetAuthorOfBook("LUK"), includedBooks, keyStrokesByBook).KeyStrokeCount);
		}
	}
}
