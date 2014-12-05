using System.Linq;
using NUnit.Framework;
using ProtoScript;

namespace ProtoScriptTests
{
	[TestFixture]
	class CharacterVerseTests
	{
		[Test]
		public void GetCharacter_OneMatch_GetsCorrectCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 1, 4);
			Assert.AreEqual("John the Baptist", characterId);
			characterId = CharacterVerse.GetCharacter("MRK", 1, 20);
			Assert.AreEqual("Jesus", characterId);
		}

		[Test]
		public void GetCharacter_MoreThanOneMatch_DifferentCharacters_GetsAmbiguousCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 10, 48);
			Assert.AreEqual(Block.AmbiguousCharacter, characterId);
		}

		[Test]
		public void GetCharacter_MoreThanOneMatch_SameCharacter_GetsCorrectCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 15, 44);
			Assert.AreEqual("Pilate", characterId);
			characterId = CharacterVerse.GetCharacter("MRK", 16, 3);
			Assert.AreEqual("Mary Magdalene", characterId);
		}

		[Test]
		public void GetCharacter_NoMatch_GetsUnknownCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 1, 1);
			Assert.AreEqual(Block.UnknownCharacter, characterId);
		}

		[Test]
		public void GetCharacter_VerseBridge_StartVerse()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 15, 38);
			Assert.AreEqual("centurion=centurion/other guards|army officer", characterId);
		}

		[Test]
		public void GetCharacter_VerseBridge_MiddleVerse()
		{
			var characterId = CharacterVerse.GetCharacter("GEN", 15, 20);
			Assert.AreEqual("God", characterId);
		}

		[Test]
		public void GetCharacter_VerseBridge_EndVerse()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 16, 4);
			Assert.AreEqual("Mary Magdalene", characterId);
		}

		[Test]
		public void GetCharacters_One()
		{
			var characters = CharacterVerse.GetCharacters("GEN", 15, 20);
			Assert.AreEqual(1, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
		}

		[Test]
		public void GetCharacters_MoreThanOne()
		{
			var characters = CharacterVerse.GetCharacters("MRK", 6, 24);
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias' daughter"));
		}

		[Test]
		public void GetCharacters_MoreThanOne_Duplicate_ReturnsUnique()
		{
			var characters = CharacterVerse.GetCharacters("MRK", 6, 37);
			Assert.AreEqual(3, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == ""));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Jesus" && c.Delivery == "questioning"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Philip"));
		}
	}
}
