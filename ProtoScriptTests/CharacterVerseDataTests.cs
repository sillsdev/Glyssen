using System.Linq;
using NUnit.Framework;
using ProtoScript;

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
			CharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
		}

		[Test]
		public void GetCharacters_NoMatch_EmptyResults()
		{
			Assert.IsFalse(CharacterVerseData.Singleton.GetCharacters("MRK", 1, 1).Any());
		}

		[Test]
		public void GetCharacters_One()
		{
			var characters = CharacterVerseData.Singleton.GetCharacters("GEN", 15, 20).ToList();
			Assert.AreEqual(1, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
		}

		[Test]
		public void GetCharacter_VerseBridge_StartVerse()
		{
			var character = CharacterVerseData.Singleton.GetCharacters("MRK", 15, 38).Single();
			Assert.AreEqual("centurion=centurion/other guards|army officer", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_MiddleVerse()
		{
			var character = CharacterVerseData.Singleton.GetCharacters("GEN", 15, 20).Single();
			Assert.AreEqual("God", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_EndVerse()
		{
			var character = CharacterVerseData.Singleton.GetCharacters("MRK", 16, 4).Single();
			Assert.AreEqual("Mary Magdalene", character.Character);
		}

		[Test] public void GetCharacters_MoreThanOneWithNoDuplicates_ReturnsAll()
		{
			var characters = CharacterVerseData.Singleton.GetCharacters("MRK", 6, 24).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias' daughter"));
		}

		[Test]
		public void GetCharacters_MoreThanOneWithSomeDuplicates_ReturnsUnique()
		{
			var characters = CharacterVerseData.Singleton.GetCharacters("MRK", 6, 37).ToList();
			Assert.AreEqual(3, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == ""));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == "questioning"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Philip"));
		}

		[Test]
		public void GetCharacters_MultipleEntriesForSameCharacterWithDifferentDeliveries_ReturnsOneResultPerDelivery()
		{
			var characters = CharacterVerseData.Singleton.GetCharacters("MRK", 15, 44).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(characters[0].Character, characters[1].Character);
			Assert.AreNotEqual(characters[0].Delivery, characters[1].Delivery);
		}

		[Test]
		public void GetCharacters_MultipleEntriesForSameCharacter_ReturnsSingleCharacter()
		{
			var character = CharacterVerseData.Singleton.GetCharacters("MRK", 16, 3).Single();
			Assert.AreEqual("Mary Magdalene", character.Character);
		}
	}
}
