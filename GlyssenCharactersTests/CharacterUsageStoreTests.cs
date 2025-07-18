﻿using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared;
using GlyssenCharacters;
using GlyssenCharactersTests.Properties;
using NUnit.Framework;
using SIL.Scripture;
using static System.Int32;

namespace GlyssenCharactersTests
{
	[TestFixture]
	class CharacterUsageStoreTests
	{
		[Test]
		public void GetStandardCharacterName_KnownCharacterWithSingleDelivery_ReturnsCharacterNameAndDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Jesus"));
			Assert.That(delivery, Is.EqualTo("questioning"));
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_KnownCharacterInVerseBridgeStartingInVerseBefore_ReturnsCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("36-38")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Jesus"));
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithNoDelivery_ReturnsEnglishCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Andrés", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Andrew"));
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		/// <summary>
		/// This test case is for a highly improbable scenario, but just in case there were
		/// ever two localized names (in the same or different languages) that happened to
		/// be translations of two different character IDs used in the same verse, we want
		/// it to be treated as ambiguous.
		/// </summary>
		[Test]
		public void GetStandardCharacterName_LocalizedCharacterNameCorrespondsToMultipleCharacters_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Unrealistic scenario",
				BCVRef.BookToNumber("MRK"), 6, new[] {new Verse("38")},
				out var delivery, out var defaultCharacter), Is.Null);
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithSingleDelivery_ReturnsEnglishCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jésus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Jesus"));
			Assert.That(delivery, Is.EqualTo("questioning"));
			Assert.That(defaultCharacter, Is.Null);
		}

		[TestCase("ZEC", 1, "10-12", "Zacarías (en una visión)", ExpectedResult = "Zechariah the prophet, son of Berechiah")]
		public string GetStandardCharacterName_LocalizedAliasWithNoDelivery_ReturnsEnglishCharacterName(
			string bookCode, int chapterNum, string verse, string alias)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);

			var result = store.GetKnownCharacterName(alias, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] { new Verse(verse) }, out var delivery, out var defaultCharacter);

			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);

			return result;
		}

		[Test]
		public void GetStandardCharacterName_LocalizedGroupCharacterWithDefault_ReturnsEnglishCharacterNameAndDefault()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Barnabus/Paulus", BCVRef.BookToNumber("ACT"),
				14, new[] {new Verse("15-16"), new Verse("17")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Barnabas/Paul"));
			Assert.That(delivery, Is.EqualTo("preaching"));
			Assert.That(defaultCharacter, Is.EqualTo("Paul"));
		}

		[Test]
		public void GetStandardCharacterName_KnownCharacterWithMultipleDeliveries_ReturnsCharacterNameAndNullDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jesus", BCVRef.BookToNumber("MAT"),
				14, new[] {new Verse("19")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Jesus"));
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithMultipleDeliveries_ReturnsCharacterNameAndNullDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jesucristo", BCVRef.BookToNumber("MAT"),
				14, new[] {new Verse("19")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Jesus"));
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_SpecificCharacterInList_ReturnsFullCharacterListWithSpecifiedCharacterAsDefault()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("teachers of religious law", BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Pharisees/teachers of religious law"));
			Assert.That(delivery, Is.EqualTo("critical"));
			Assert.That(defaultCharacter, Is.EqualTo("teachers of religious law"));
		}

		[TestCase("Pharisees and teachers of religious law")]
		[TestCase("Pharisees/teachers of religious law ")]
		[TestCase(" Pharisees/teachers of religious law")]
		[TestCase("pharisees/teachers of religious law")]
		[TestCase("Pharisees / teachers of religious law")]
		[TestCase("Pharisees/teachers of religious law (bad)")]
		[TestCase("Pharisees/teachers of religious-law")]
		[TestCase("pharisees and Teachers of religious law")]
		[TestCase("Pharisees teachers of religious law")]
		[TestCase("teachers of religious law/Pharisees")]
		[TestCase("pharisees")]
		[TestCase("fariseos")]
		[TestCase("फरीसियों")]
		public void GetStandardCharacterName_CloseMatchToCharacter_ReturnsStandardCharacterName(string close)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(close, BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Pharisees/teachers of religious law"));
			Assert.That(delivery, Is.EqualTo("critical"));
			Assert.That(defaultCharacter, Is.EqualTo("Pharisees"));
		}

		[TestCase("teachers of religious (Jewish OT) law")]
		[TestCase("teachers ofreligiouslaw")]
		[TestCase("धार्मिक कानून शिक्षक")]
		public void GetStandardCharacterName_CloseMatchToNonDefaultCharacter_ReturnsStandardCharacterNameAndMatchingDefault(string close)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(close, BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("Pharisees/teachers of religious law"));
			Assert.That(delivery, Is.EqualTo("critical"));
			Assert.That(defaultCharacter, Is.EqualTo("teachers of religious law"));
		}

		[TestCase("Enoc", ExpectedResult = "Enoch")]
		[TestCase("Enóch", ExpectedResult = "Enoch")]
		[TestCase("enoc", ExpectedResult = "Enoch")]
		[TestCase("Enocho", ExpectedResult = "Enoch")]
		[TestCase("Enoc (quoted)", ExpectedResult = "Enoch")]
		[TestCase("Enock (quotation)", ExpectedResult = "Enoch")]
		[TestCase("voice of man calling (preparing the way for Christ)", "ISA", 40, "3",
			ExpectedResult = "voice of one calling (preparing way for Christ)")]
		[TestCase("singer in Judea", "ISA", 26, "1-6", "singing",
			ExpectedResult = "singers in Judah")]
		public string GetStandardCharacterName_CloseMatchToOnlyCharacter_ReturnsStandardCharacterName(
			string close, string bookCode = "JUD", int chapterNum = 1, string verse = "14-15",
			string expectedDelivery = null)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			var result = store.GetKnownCharacterName(close, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out var delivery, out _);
			Assert.That(delivery, Is.EqualTo(expectedDelivery));
			return result;
		}

		[TestCase("MRK", 6, "15", "people, st others")]
		[TestCase("MRK", 6, "15", "peoples, others")]
		[TestCase("MRK", 6, "15", "people, some others")]
		[TestCase("MRK", 6, "15", "people, others")]
		[TestCase("LUK", 22, "70", "यीशु शिक्षक")] // "Jesus the teacher"
		public void GetStandardCharacterName_CloseMatchToMultipleCharactersWithNoClearWinner_ReturnsNull(
			string bookCode, int chapterNum, string verse, string close)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(close, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _), Is.Null);
		}

		[TestCase("ISA", 41, "6", "islander", null, ExpectedResult = "islanders")]
		[TestCase("ISA", 28, "9", "hearer", "mocking", ExpectedResult = "hearers")]
		public string GetStandardCharacterName_CloseMatchInVerseWithMultipleCharactersWithClearWinner_ReturnsNull(
			string bookCode, int chapterNum, string verse, string close, string expectedDelivery)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			var result = store.GetKnownCharacterName(close, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out var delivery, out _);
			Assert.That(delivery, Is.EqualTo(expectedDelivery));
			return result;
		}

		[TestCase("REV", 1, "17", "someone li", ExpectedResult = "Jesus")]
		[TestCase("REV", 1, "17", "son of man", ExpectedResult = "Jesus")]
		[TestCase("REV", 1, "17", "Son of man", ExpectedResult = "Jesus")]
		public string GetStandardCharacterName_SingleEntryHasMatchingSubstring_ReturnsStandardCharacterName(
			string bookCode, int chapterNum, string verse, string charStringInData)
		{
			var bookNum = BCVRef.BookToNumber(bookCode);
			// Verify that the C-V data is as follows:
			// REV	1	17	Jesus		someone like a son of man	Normal
			Assert.That(ControlCharacterVerseData.Singleton.GetCharacters(bookNum, chapterNum,
				Parse(verse)).Single().Alias, Is.EqualTo("someone like a son of man"));

			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			return store.GetKnownCharacterName(charStringInData, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _);
		}

		[TestCase("ISA", 45, "24", "tongue", ExpectedResult = "every tongue")]
		[TestCase("ISA", 45, "24", "Yahweh", ExpectedResult = "God")]
		public string GetStandardCharacterName_SingleMatchingSubstring_ReturnsStandardCharacterNameCorrespondingToSubstringMatch(
			string bookCode, int chapterNum, string verse, string charStringInData)
		{
			var bookNum = BCVRef.BookToNumber(bookCode);
			// Verify that the C-V data is as follows:
			// ISA	45	24	God		God (Yahweh)	Normal		
			// ISA	45	24	every tongue			Hypothetical
			var entries = ControlCharacterVerseData.Singleton.GetCharacters(bookNum, chapterNum,
				Parse(verse)).ToList();
			Assert.That(entries.Count, Is.GreaterThan(1));
			Assert.That(entries.Count(c => c.Character.Contains(charStringInData) ||
				c.Alias.Contains(charStringInData)), Is.EqualTo(1));

			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			return store.GetKnownCharacterName(charStringInData, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _);
		}

		[TestCase("REV", 1, "17", "of")]
		[TestCase("REV", 1, "17", "ome")]
		[TestCase("REV", 1, "17", "Son")]
		[TestCase("REV", 1, "17", "son")]
		public void GetStandardCharacterName_ShortOrPartialWordMatchingSubstring_ReturnsNull(
			string bookCode, int chapterNum, string verse, string alias)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(alias, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _), Is.Null);
		}

		[TestCase("ISA", 63, "4", "garments")]
		[TestCase("ZEC", 1, "10", "angel")]
		public void GetStandardCharacterName_SubstringMatchesMultipleEntries_ReturnsNull(
			string bookCode, int chapterNum, string verse, string alias)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(alias, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _), Is.Null);
		}

		[TestCase("REV", 1, "17", "someone like a son of man", ExpectedResult = "Jesus")]
		[TestCase("ISA", 41, "6", "everyone (islanders or foreigners)", ExpectedResult = "islanders")]
		[TestCase("ISA", 41, "6", "everyone", ExpectedResult = "islanders")]
		[TestCase("ISA", 41, "6", "islanders", ExpectedResult = "islanders")]
		[TestCase("ISA", 41, "6", "foreigners", ExpectedResult = "islanders")]
		public string GetStandardCharacterName_KnownCharacterWithMatchingAlias_ReturnsStandardCharacterName(
			string bookCode, int chapterNum, string verse, string alias)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			return store.GetKnownCharacterName(alias, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out _, out _);
		}

		[Test]
		public void GetStandardCharacterName_CharacterIsGroupConsistingOfMultipleKnownCharacters_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Andrew/Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter), Is.Null);
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test]
		public void GetStandardCharacterName_CharacterIsInMoreThanOneKnownGroup_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Asher", BCVRef.BookToNumber("GEN"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter), Is.Null);
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[TestCase("narrator-MAT")]
		[TestCase("narrator (MAT)")]
		[TestCase("narrador (MAT)")]
		public void GetStandardCharacterName_NarratorScareQuote_ReturnsStandardCharacterName(string narratorChar)
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(narratorChar, BCVRef.BookToNumber("MAT"),
				2, new[] {new Verse("1")}, out var delivery, out var defaultCharacter),
				Is.EqualTo("narrator-MAT"));
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[TestCase("JUD", 1, "14-15", "Anacho")]
		[TestCase("MAT", 2, "1", "buggaboo snerfwiddle")]
		public void GetStandardCharacterName_UnknownCharacter_ReturnsNull(string bookCode,
			int chapterNum, string verse, string character)
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName(character, BCVRef.BookToNumber(bookCode),
				chapterNum, new[] {new Verse(verse)}, out var delivery, out var defaultCharacter), Is.Null);
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		[Test] public void GetStandardCharacterName_KnownCharacterNotExpectedInVerse_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.That(store.GetKnownCharacterName("Jesus", BCVRef.BookToNumber("MAT"),
				1, new[] {new Verse("1")}, out var delivery, out var defaultCharacter), Is.Null);
			Assert.That(delivery, Is.Null);
			Assert.That(defaultCharacter, Is.Null);
		}

		// Note: In production, we have tried to clean up this kind of C-V data so this can't
		// happen because it seldom makes sense. This test uses an older test version of the
		// CV file to test this condition, in case there are places where it is still possible.
		[Test] public void GetStandardCharacterName_TwoVersesWithSameCharacterGroupButDifferentDefaults_UsesDefaultFromFirstVerse()
		{
			try
			{
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
				var store = new CharacterUsageStore(ScrVers.English,
					ControlCharacterVerseData.Singleton, GetLocalizedVariants);
				Assert.That(store.GetKnownCharacterName("Peter (Simon)/John", BCVRef.BookToNumber("ACT"),
					4, new[] {new Verse("19"), new Verse("20")}, out var delivery, out var defaultCharacter),
					Is.EqualTo("Peter (Simon)/John"));
				Assert.That(delivery, Is.Null);
				Assert.That(defaultCharacter, Is.EqualTo("Peter (Simon)"));
			}
			finally
			{
				ControlCharacterVerseData.TabDelimitedCharacterVerseData = null;
			}
		}

		private IEnumerable<string> GetLocalizedVariants(string englishCharId)
		{
			switch (englishCharId)
			{
				case "Andrew":
					yield return "Andrés";
					yield return "Andy";
					yield return "Unrealistic scenario";
					break;

				case "Jesus":
					yield return "Jesucristo";
					yield return "Jésus";
					yield return "यीशु";
					yield return "Unrealistic scenario";
					break;

				case "narrator-MAT":
					yield return "narrator (MAT)";
					yield return "narrador (MAT)";
					break;

				case "Barnabas/Paul":
					yield return "Bernabé/Pablo";
					yield return "Barnabus/Paulus";
					break;

				case "Zechariah (in vision)":
					yield return "Zacarías (en una visión)";
					break;

				case "Pharisees":
					yield return "fariseos";
					yield return "फरीसियों";
					break;

				case "teachers of religious law":
					yield return "धार्मिक कानून के शिक्षक";
					break;
			}
		}
	}
}
