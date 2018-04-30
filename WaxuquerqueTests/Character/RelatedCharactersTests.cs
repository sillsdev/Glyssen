using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SIL.Xml;
using Waxuquerque.Character;
using WaxuquerqueTests.Properties;

namespace WaxuquerqueTests.Character
{
	class RelatedCharactersTests
	{

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
		}

		[Test]
		public void Serialize()
		{
			HashSet<RelatedCharacters> set = new HashSet<RelatedCharacters>();
			set.Add(new RelatedCharacters
			{
				CharacterIds = new List<string> { "David", "David (old)" },
				RelationshipType = CharacterRelationshipType.SameCharacterWithMultipleAges
			});
			var serializedString = XmlSerializationHelper.SerializeToString(set);
			Assert.AreNotEqual(-1, serializedString.IndexOf("<RelatedCharacters RelationshipType=\"SameCharacterWithMultipleAges\">"));
			Assert.AreNotEqual(-1, serializedString.IndexOf("<CharacterId>David</CharacterId>"));
			Assert.AreNotEqual(-1, serializedString.IndexOf("<CharacterId>David (old)</CharacterId>"));
		}

		[Test]
		public void GetAll()
		{
			var allRelatedCharactersSet = RelatedCharactersData.Singleton.GetAll();
			Assert.Greater(allRelatedCharactersSet.Count, 0);

			var davidRelatedCharacters = allRelatedCharactersSet.First(rc => rc.CharacterIds.Contains("David"));
			Assert.True(davidRelatedCharacters.CharacterIds.Contains("David (old)"));
			Assert.AreEqual(CharacterRelationshipType.SameCharacterWithMultipleAges, davidRelatedCharacters.RelationshipType);
		}

		[Test]
		public void GetCharacterIdsForType()
		{
			var characterIds = RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges);
			Assert.AreEqual(14, characterIds.Count);
			Assert.True(characterIds.Contains("David"));
			Assert.True(characterIds.Contains("David (old)"));
		}

		[Test]
		public void GetCharacterIdToRelatedCharactersDictionary()
		{
			var dictionary = RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary();
			Assert.AreEqual(14, dictionary.Count);
			Assert.True(dictionary["David (old)"].Single().CharacterIds.Contains("David"));
		}
	}
}
