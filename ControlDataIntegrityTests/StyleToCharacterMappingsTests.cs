using System.Linq;
using GlyssenCharacters;
using NUnit.Framework;

namespace ControlDataIntegrityTests
{
	public class StyleToCharacterMappingsTests
	{
		[Test]
		public void DataIntegrity_LoadsCorrectly()
		{
			Assert.That(StyleToCharacterMappings.AllSfMarkers.Any(), Is.True);
		}

		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var marker in StyleToCharacterMappings.AllSfMarkers)
			{
				if (StyleToCharacterMappings.TryGetCharacterForCharStyle(marker, out var characterId))
				{
					Assert.That(CharacterDetailData.Singleton.GetDictionary().Keys, Does.Contain(characterId),
						$"Character {characterId} in StyleToCharacterMappings.xml was not found in CharacterDetail");
				}
				else
				{
					Assert.That(StyleToCharacterMappings.TryGetStandardCharacterForParaStyle(
						marker, out _), Is.True);
				}
			}
		}
	}
}
