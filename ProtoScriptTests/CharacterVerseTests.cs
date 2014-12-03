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
	}
}
