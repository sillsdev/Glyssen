using System.Collections.Generic;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngineTests.Properties;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenEngineTests.Character
{
	[TestFixture]
	class CharacterUsageStoreTests
	{
		[Test]
		public void GetStandardCharacterName_KnownCharacterWithSingleDelivery_ReturnsCharacterNameAndDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Jesus", store.GetStandardCharacterName("Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")},  out var delivery, out var defaultCharacter));
			Assert.AreEqual("questioning", delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_KnownCharacterInVerseBridgeStartingInVerseBefore_ReturnsCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Jesus", store.GetStandardCharacterName("Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("36-38")},  out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithNoDelivery_ReturnsEnglishCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Andrew", store.GetStandardCharacterName("Andrés", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
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
			Assert.IsNull(store.GetStandardCharacterName("Unrealistic scenario",
				BCVRef.BookToNumber("MRK"), 6, new[] {new Verse("38")},
				out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithSingleDelivery_ReturnsEnglishCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Jesus", store.GetStandardCharacterName("Jésus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter));
			Assert.AreEqual("questioning", delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedGroupCharacterWithDefault_ReturnsEnglishCharacterNameAndDefault()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Barnabas/Paul", store.GetStandardCharacterName("Barnabus/Paulus", BCVRef.BookToNumber("ACT"),
				14, new[] {new Verse("15-16"), new Verse("17")}, out var delivery, out var defaultCharacter));
			Assert.AreEqual("preaching", delivery);
			Assert.AreEqual("Paul", defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_KnownCharacterWithMultipleDeliveries_ReturnsCharacterNameAndNullDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Jesus", store.GetStandardCharacterName("Jesus", BCVRef.BookToNumber("MAT"),
				14, new[] {new Verse("19")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_LocalizedCharacterWithMultipleDeliveries_ReturnsCharacterNameAndNullDelivery()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Jesus", store.GetStandardCharacterName("Jesucristo", BCVRef.BookToNumber("MAT"),
				14, new[] {new Verse("19")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_SpecificCharacterInList_ReturnsFullCharacterListWithSpecifiedCharacterAsDefault()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Pharisees/teachers of religious law", store.GetStandardCharacterName("teachers of religious law", BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter));
			Assert.AreEqual("critical", delivery);
			Assert.AreEqual("teachers of religious law", defaultCharacter);
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
		public void GetStandardCharacterName_CloseMatchToCharacter_ReturnsStandardCharacterName(string close)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Pharisees/teachers of religious law", store.GetStandardCharacterName(close, BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter));
			Assert.AreEqual("critical", delivery);
			Assert.AreEqual("Pharisees", defaultCharacter);
		}

		[TestCase("teachers of religious (Jewish OT) law")] [TestCase("teachers ofreligiouslaw")]
		public void GetStandardCharacterName_CloseMatchToNonDefaultCharacter_ReturnsStandardCharacterNameAndMatchingDefault(string close)
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Pharisees/teachers of religious law", store.GetStandardCharacterName(close, BCVRef.BookToNumber("MRK"),
				7, new[] {new Verse("5")}, out var delivery, out var defaultCharacter));
			Assert.AreEqual("critical", delivery);
			Assert.AreEqual("teachers of religious law", defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_CharacterIsGroupConsistingOfMultipleKnownCharacters_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.IsNull(store.GetStandardCharacterName("Andrew/Jesus", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_CharacterIsInMoreThanOneKnownGroup_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.IsNull(store.GetStandardCharacterName("Asher", BCVRef.BookToNumber("GEN"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[TestCase("narrator-MAT")]
		[TestCase("narrator (MAT)")]
		[TestCase("narrador (MAT)")]
		public void GetStandardCharacterName_NarratorScareQuote_ReturnsStandardCharacterName(string narratorChar)
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("narrator-MAT", store.GetStandardCharacterName(narratorChar, BCVRef.BookToNumber("MAT"),
				2, new[] {new Verse("1")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test]
		public void GetStandardCharacterName_UnknownCharacter_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.IsNull(store.GetStandardCharacterName("buggaboo snerfwiddle", BCVRef.BookToNumber("MAT"),
				2, new[] {new Verse("1")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
		}

		[Test] public void GetStandardCharacterName_KnownCharacterNotExpectedInVerse_ReturnsNull()
		{
			var store = new CharacterUsageStore(ScrVers.English,
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.IsNull(store.GetStandardCharacterName("Jesus", BCVRef.BookToNumber("MAT"),
				1, new[] {new Verse("1")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
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
				Assert.AreEqual("Peter (Simon)/John", store.GetStandardCharacterName("Peter (Simon)/John", BCVRef.BookToNumber("ACT"),
					4, new[] {new Verse("19"), new Verse("20")}, out var delivery, out var defaultCharacter));
				Assert.IsNull(delivery);
				Assert.AreEqual("Peter (Simon)", defaultCharacter);
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
			}
		}
	}
}
