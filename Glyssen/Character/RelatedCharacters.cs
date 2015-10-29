using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.Extensions;

namespace Glyssen.Character
{
	public enum CharacterRelationshipType
	{
		General,
		SameCharacterWithMultipleAges
	}

	[XmlType]
	public class RelatedCharacters
	{
		[XmlAttribute]
		public CharacterRelationshipType RelationshipType { get; set; }

		[XmlElement("CharacterId")]
		public List<string> CharacterIds { get; set; }
	}

	public class RelatedCharactersData
	{
		private static RelatedCharactersData s_singleton;
		private static string s_source;
		private ISet<RelatedCharacters> m_all;
		private IDictionary<string, ISet<RelatedCharacters>> m_characterIdToRelatedCharactersDictionary;

		internal static string Source
		{
			get { return s_source; }
			set
			{
				s_source = value;
				s_singleton = null;
			}
		}

		private RelatedCharactersData()
		{
			// Tests can set this before accessing the Singleton.
			if (Source == null)
				Source = Resources.RelatedCharacters;
		}

		public static RelatedCharactersData Singleton
		{
			get { return s_singleton ?? (s_singleton = new RelatedCharactersData()); }
		}

		public ISet<RelatedCharacters> GetAll()
		{
			if (m_all == null)
			{
				using (var reader = new StringReader(Source))
				{
					XmlSerializer deserializer = new XmlSerializer(typeof(HashSet<RelatedCharacters>), new XmlRootAttribute("RelatedCharacterSets"));
					m_all = (HashSet<RelatedCharacters>)deserializer.Deserialize(reader);
				}
			}
			return m_all;
		}

		public ISet<string> GetCharacterIdsForType(CharacterRelationshipType relationshipType)
		{
			ISet<string> characterIdsForType = new HashSet<string>();
			foreach (var relatedCharacters in GetAll().Where(rc => rc.RelationshipType == relationshipType))
				characterIdsForType.AddRange(relatedCharacters.CharacterIds);
			return characterIdsForType;
		}

		public IDictionary<string, ISet<RelatedCharacters>> GetCharacterIdToRelatedCharactersDictionary()
		{
			if (m_characterIdToRelatedCharactersDictionary == null)
			{
				m_characterIdToRelatedCharactersDictionary = new Dictionary<string, ISet<RelatedCharacters>>();
				foreach (var relatedCharacters in GetAll())
				{
					foreach (var characterId in relatedCharacters.CharacterIds)
					{
						if (m_characterIdToRelatedCharactersDictionary.ContainsKey(characterId))
							m_characterIdToRelatedCharactersDictionary[characterId].Add(relatedCharacters);
						else
							m_characterIdToRelatedCharactersDictionary[characterId] = new HashSet<RelatedCharacters> { relatedCharacters };
					}
				}
			}
			return m_characterIdToRelatedCharactersDictionary;
		}

		public ISet<string> GetMatchingCharacterIds(string characterId, CharacterRelationshipType relationshipType)
		{
			ISet<string> result = new HashSet<string>();
			var dictionary = GetCharacterIdToRelatedCharactersDictionary();
			if (!dictionary.ContainsKey(characterId))
				return result;

			IEnumerable<RelatedCharacters> relatedCharactersSet = dictionary[characterId].Where(rc => rc.RelationshipType == relationshipType);
			foreach (var relatedCharacters in relatedCharactersSet)
				result.AddRange(relatedCharacters.CharacterIds);

			return result;
		}
	}
}
