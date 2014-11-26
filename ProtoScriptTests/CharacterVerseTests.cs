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
			int characterId = CharacterVerse.GetCharacter("MRK", 1, 4);
			Assert.AreEqual(273, characterId);
		}

		[Test]
		public void GetCharacter_MoreThanOneMatch_GetsUnknownCharacterId()
		{
			int characterId = CharacterVerse.GetCharacter("MRK", 10, 48);
			Assert.AreEqual(Block.kUnknownCharacterId, characterId);
		}

		[Test]
		public void GetCharacter_NoMatch_GetsUnknownCharacterId()
		{
			int characterId = CharacterVerse.GetCharacter("MRK", 1, 1);
			Assert.AreEqual(Block.kUnknownCharacterId, characterId);
		}
	}
}
