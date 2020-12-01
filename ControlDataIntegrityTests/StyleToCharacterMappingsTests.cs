using System.Linq;
using GlyssenEngine.Character;
using NUnit.Framework;

namespace ControlDataIntegrityTests
{
	public class StyleToCharacterMappingsTests
	{
		[Test]
		public void DataIntegrity_LoadsCorrectly()
		{
			Assert.IsTrue(StyleToCharacterMappings.AllSfMarkers.Any());
		}

		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var marker in StyleToCharacterMappings.AllSfMarkers)
			{
				if (StyleToCharacterMappings.TryGetCharacterForCharStyle(marker, out var characterId))
				{
					Assert.True(CharacterDetailData.Singleton.GetDictionary().Keys.Contains(characterId),
						$"Character {characterId} in StyleToCharacterMappings.xml was not found in CharacterDetail");
				}
				else
				{
					Assert.IsTrue(StyleToCharacterMappings.TryGetStandardCharacterForParaStyle(
						marker, out _));
				}
			}
		}
	}
}
