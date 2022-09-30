using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using GlyssenCharacters.Properties;
using SIL.Extensions;

namespace GlyssenCharacters
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
		private IDictionary<string, ISet<string>> m_characterIdToSameCharactersWithDifferentAge;

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

		private IDictionary<string, ISet<string>> CharacterIdToSameCharactersWithDifferentAge
		{
			get
			{
				if (m_characterIdToSameCharactersWithDifferentAge == null)
				{
					m_characterIdToSameCharactersWithDifferentAge = new Dictionary<string, ISet<string>>();
					foreach (var relatedCharacters in GetAll().Where(rc => rc.RelationshipType == CharacterRelationshipType.SameCharacterWithMultipleAges))
					{
						foreach (var characterId in relatedCharacters.CharacterIds)
						{
							if (m_characterIdToSameCharactersWithDifferentAge.ContainsKey(characterId))
								m_characterIdToSameCharactersWithDifferentAge[characterId].AddRange(relatedCharacters.CharacterIds);
							else
								m_characterIdToSameCharactersWithDifferentAge[characterId] = new HashSet<string>(relatedCharacters.CharacterIds);
						}
					}
				}
				return m_characterIdToSameCharactersWithDifferentAge;
			}
		}

		public bool HasMatchingCharacterIdsOfADifferentAge(string characterId)
		{
			return CharacterIdToSameCharactersWithDifferentAge.ContainsKey(characterId);
		}

		public bool TryGetMatchingCharacterIdsOfADifferentAge(string characterId, out ISet<string> result)
		{
			return CharacterIdToSameCharactersWithDifferentAge.TryGetValue(characterId, out result);
		}

		// This returns an enumeration with any character IDs weeded out that represent the same individual person in a different
		// mode, age, etc. If the set contains two characters that resolve to a single individual, the one returned is arbitrary.
		public IEnumerable<string> UniqueIndividuals(ISet<string> characterSet)
		{
			var setOfCharactersForAlreadyReturnedIndividuals = new HashSet<string>();
			ISet<string> alterEgos;
			foreach (var characterId in characterSet)
			{
				if (setOfCharactersForAlreadyReturnedIndividuals.Contains(characterId))
					continue;
				if (TryGetMatchingCharacterIdsOfADifferentAge(characterId, out alterEgos))
				{
					setOfCharactersForAlreadyReturnedIndividuals.AddRange(alterEgos);
				}
				yield return characterId;
			}
		}
	}
}
