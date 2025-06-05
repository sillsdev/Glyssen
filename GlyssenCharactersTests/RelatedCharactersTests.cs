using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenCharacters;
using GlyssenCharactersTests.Properties;
using NUnit.Framework;
using SIL.Xml;

namespace GlyssenCharactersTests
{
	class RelatedCharactersTests
	{

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			RelatedCharactersData.Source = Resources.TestRelatedCharacters;
		}

		[Test]
		public void Serialize()
		{
			HashSet<RelatedCharacters> set = new HashSet<RelatedCharacters>
			{
				new RelatedCharacters
				{
					CharacterIds = new List<string> { "David", "David (old)" },
					RelationshipType = CharacterRelationshipType.SameCharacterWithMultipleAges
				}
			};
			var serializedString = XmlSerializationHelper.SerializeToString(set);
			Assert.That(serializedString.IndexOf("<RelatedCharacters RelationshipType=\"SameCharacterWithMultipleAges\">",
				StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
			Assert.That(serializedString.IndexOf("<CharacterId>David</CharacterId>",
				StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
			Assert.That(serializedString.IndexOf("<CharacterId>David (old)</CharacterId>",
				StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
		}

		[Test]
		public void GetAll()
		{
			var allRelatedCharactersSet = RelatedCharactersData.Singleton.GetAll();
			Assert.That(allRelatedCharactersSet.Count, Is.GreaterThan(0));

			var davidRelatedCharacters = allRelatedCharactersSet.First(rc => rc.CharacterIds.Contains("David"));
			Assert.That(davidRelatedCharacters.CharacterIds, Does.Contain("David (old)"));
			Assert.That(CharacterRelationshipType.SameCharacterWithMultipleAges, Is.EqualTo(davidRelatedCharacters.RelationshipType));
		}

		[Test]
		public void GetCharacterIdsForType()
		{
			var characterIds = RelatedCharactersData.Singleton.GetCharacterIdsForType(CharacterRelationshipType.SameCharacterWithMultipleAges);
			Assert.That(characterIds.Count, Is.EqualTo(14));
			Assert.That(characterIds, Does.Contain("David"));
			Assert.That(characterIds, Does.Contain("David (old)"));
		}

		[Test]
		public void GetCharacterIdToRelatedCharactersDictionary()
		{
			var dictionary = RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary();
			Assert.That(dictionary.Count, Is.EqualTo(14));
			Assert.That(dictionary["David (old)"].Single().CharacterIds, Does.Contain("David"));
		}
	}
}
