using GlyssenCharacters;
using NUnit.Framework;

namespace ControlDataIntegrityTests
{
	class RelatedCharacterDataTests
	{
		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var characterId in RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary().Keys)
			{
				Assert.That(CharacterDetailData.Singleton.GetDictionary().Keys, Does.Contain(characterId),
					$"The following character ID was not found in CharacterDetail: {characterId}");
			}
		}
	}
}
