using GlyssenEngine.Character;
using NUnit.Framework;

namespace GlyssenEngineTests.Character
{
	/// <summary>
	/// Note that these tests purport to test the GetCharacters method, but in fact that is just a simple LINQ statement;
	/// they're really testing the Load method.
	/// </summary>
	[TestFixture]
	class CharacterVerseDataTests
	{
		[TestCase("BC-GEN")]
		[TestCase("extra-MAT")]
		[TestCase("intro-REV")]
		public void IsCharacterExtraBiblical_ExtraBiblicalCharacter_ReturnsTrue(string characterId)
		{
			Assert.IsTrue(CharacterVerseData.IsCharacterExtraBiblical(characterId));
		}

		[TestCase("BCGEN")]
		// ENHANCE: Character ID should only be considered extra-biblical if prefix is followed by book ID. [TestCase("extra-weird guy")]
		[TestCase("-extra-")]
		[TestCase("introduction of the new king")]
		[TestCase("Joseph")]
		public void IsCharacterExtraBiblical_NormalCharacter_ReturnsFalse(string characterId)
		{
			Assert.IsFalse(CharacterVerseData.IsCharacterExtraBiblical(characterId));
		}

		[TestCase("narrator-MAL")]
		[TestCase("BC-GEN")]
		[TestCase("extra-MAT")]
		[TestCase("intro-REV")]
		public void IsCharacterStandard_StandardCharacter_ReturnsTrue(string characterId)
		{
			Assert.IsTrue(CharacterVerseData.IsCharacterStandard(characterId));
		}

		[TestCase("Jesus")]
		[TestCase("narrator's friend")]
		[TestCase("extraordinary dude in MAT")]
		[TestCase("someone speaking in intro-REV")]
		public void IsCharacterStandard_NormalCharacter_ReturnsTrue(string characterId)
		{
			Assert.IsFalse(CharacterVerseData.IsCharacterStandard(characterId));
		}
	}
}
