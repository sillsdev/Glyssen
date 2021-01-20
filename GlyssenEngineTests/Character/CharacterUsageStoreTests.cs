using System.Collections.Generic;
using Glyssen.Shared;
using GlyssenEngine.Character;
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
		public void GetStandardCharacterName_LocalizedCharacterWithSingleDelivery_ReturnsEnglishCharacterName()
		{
			var store = new CharacterUsageStore(ScrVers.English, 
				ControlCharacterVerseData.Singleton, GetLocalizedVariants);
			Assert.AreEqual("Andrew", store.GetStandardCharacterName("Andrés", BCVRef.BookToNumber("MRK"),
				6, new[] {new Verse("38")}, out var delivery, out var defaultCharacter));
			Assert.IsNull(delivery);
			Assert.IsNull(defaultCharacter);
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

		private IEnumerable<string> GetLocalizedVariants(string englishCharId)
		{
			switch (englishCharId)
			{
				case "Andrew":
					yield return "Andrés";
					yield return "Andy";
					break;

				case "narrator-MAT":
					yield return "narrator (MAT)";
					yield return "narrador (MAT)";
					break;
			}
		}
	}
}
