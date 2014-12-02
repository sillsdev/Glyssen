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
		}

		[Test]
		public void GetCharacter_MoreThanOneMatch_GetsUnknownCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 10, 48);
			Assert.AreEqual(Block.UnknownCharacter, characterId);
		}

		[Test]
		public void GetCharacter_NoMatch_GetsUnknownCharacterId()
		{
			var characterId = CharacterVerse.GetCharacter("MRK", 1, 1);
			Assert.AreEqual(Block.UnknownCharacter, characterId);
		}
	}
}
