using System.Linq;
using NUnit.Framework;
using ProtoScript;
using ProtoScript.Character;

namespace ProtoScriptTests
{
	/// <summary>
	/// Not that these tests purport to test the GetCharacters method, but in fact that is just a simple LINQ statement;
	/// they're really testing the Load method.
	/// </summary>
	[TestFixture]
	class CharacterVerseDataTests
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
		}

		[Test]
		public void GetCharacters_NoMatch_EmptyResults()
		{
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters("MRK", 1, 1).Any());
		}

		[Test]
		public void GetCharacters_One()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters("GEN", 15, 20).ToList();
			Assert.AreEqual(1, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
		}

		[Test]
		public void GetCharacter_VerseBridge_StartVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters("MRK", 15, 38).Single();
			Assert.AreEqual("centurion=centurion/other guards|army officer", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_MiddleVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters("GEN", 15, 20).Single();
			Assert.AreEqual("God", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_EndVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters("MRK", 16, 4).Single();
			Assert.AreEqual("Mary Magdalene", character.Character);
		}

		[Test] public void GetCharacters_MoreThanOneWithNoDuplicates_ReturnsAll()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters("MRK", 6, 24).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias' daughter"));
		}

		[Test]
		public void GetCharacters_MultipleEntriesForSameCharacterWithDifferentDeliveries_ReturnsOneResultPerDelivery()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters("MRK", 15, 44).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(characters[0].Character, characters[1].Character);
			Assert.AreNotEqual(characters[0].Delivery, characters[1].Delivery);
		}

		[Test]
		public void GetCharacters_MultipleCharactersInOneButNotAllVerses_ReturnsSingleCharacter()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters("1SA", 6, 4, 0, 6).Single();
			Assert.AreEqual("Philistine priests and diviners", character.Character);
		}

		[Test]
		public void GetCharacters_MultipleCharactersInMultipleVerses_ReturnsAmbiguous()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters("1SA", 8, 21, 0, 22);
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Samuel"));
		}
	}
}
