using System.Linq;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class CharacterVerseTests
	{
		[Test]
		public void GetCharacters_NoMatch_EmptyResults()
		{
			Assert.IsFalse(CharacterVerse.GetCharacters("MRK", 1, 1).Any());
		}

		[Test]
		public void GetCharacters_One()
		{
			var characters = CharacterVerse.GetCharacters("GEN", 15, 20).ToList();
			Assert.AreEqual(1, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
		}

		[Test]
		public void GetCharacter_VerseBridge_StartVerse()
		{
			var character = CharacterVerse.GetCharacters("MRK", 15, 38).Single();
			Assert.AreEqual("centurion=centurion/other guards|army officer", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_MiddleVerse()
		{
			var character = CharacterVerse.GetCharacters("GEN", 15, 20).Single();
			Assert.AreEqual("God", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_EndVerse()
		{
			var character = CharacterVerse.GetCharacters("MRK", 16, 4).Single();
			Assert.AreEqual("Mary Magdalene", character.Character);
		}

		[Test] public void GetCharacters_MoreThanOneWithNoDuplicates_ReturnsAll()
		{
			var characters = CharacterVerse.GetCharacters("MRK", 6, 24).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias' daughter"));
		}

		[Test]
		public void GetCharacters_MoreThanOneWithSomeDuplicates_ReturnsUnique()
		{
			var characters = CharacterVerse.GetCharacters("MRK", 6, 37).ToList();
			Assert.AreEqual(3, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == ""));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == "questioning"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Philip"));
		}

		[Test]
		public void GetCharacters_MultipleEntriesForSameCharacterWithDifferentDeliveries_ReturnsOneResultPerDelivery()
		{
			var characters = CharacterVerse.GetCharacters("MRK", 15, 44).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(characters[0].Character, characters[1].Character);
			Assert.AreNotEqual(characters[0].Delivery, characters[1].Delivery);
		}

		[Test]
		public void GetCharacters_MultipleEntriesForSameCharacter_ReturnsSingleCharacter()
		{
			var character = CharacterVerse.GetCharacters("MRK", 16, 3).Single();
			Assert.AreEqual("Mary Magdalene", character.Character);
		}
	}
}
