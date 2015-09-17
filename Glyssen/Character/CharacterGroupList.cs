using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SIL.Xml;

namespace Glyssen.Character
{
	[XmlRoot("CharacterGroupList")]
	public class CharacterGroupList
	{
		public CharacterGroupList()
		{
			CharacterGroups = new List<CharacterGroup>();
		}

		[XmlElement("CharacterGroup")]
		public List<CharacterGroup> CharacterGroups { get; set; }

		public void SaveToFile(string filename)
		{
			XmlSerializationHelper.SerializeToFile(filename, this);
		}
		public static CharacterGroupList LoadCharacterGroupListFromFile(string filename, Dictionary<string, int> keyStrokesByCharacterId)
		{
			var comparer = new CharacterByKeyStrokeComparer(keyStrokesByCharacterId);
			var list = XmlSerializationHelper.DeserializeFromFile<CharacterGroupList>(filename);
			foreach (var characterGroup in list.CharacterGroups)
				characterGroup.CharacterIds.ToStringComparer = comparer;
			return list;
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has any voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool AnyVoiceActorAssigned()
		{
			return CharacterGroups.Any(cg => cg.VoiceActorId != -1);
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has the specified voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool HasVoiceActorAssigned(int voiceActorId)
		{
			return CharacterGroups.Any(cg => cg.VoiceActorId == voiceActorId);
		}

		public int CountVoiceActorsAssigned()
		{
			return CharacterGroups.Count(cg => cg.VoiceActorId != -1);
		}

		public CharacterGroup GroupContainingCharacterId(string characterId)
		{
			return CharacterGroups.FirstOrDefault(g => g.CharacterIds.Contains(characterId));
		}

		public void RemoveVoiceActor(int voiceActorId)
		{
			foreach (var group in CharacterGroups)
				if (group.VoiceActorId == voiceActorId)
					group.RemoveVoiceActor();
		}

		public void PopulateEstimatedHours(Dictionary<string, int> keyStrokesByCharacterId)
		{
			foreach (var group in CharacterGroups)
			{
				int keyStrokes = 0;
				foreach (var characterId in group.CharacterIds)
				{
					int keystrokesForCharacter;
					if (keyStrokesByCharacterId.TryGetValue(characterId, out keystrokesForCharacter))
						keyStrokes += keystrokesForCharacter;
				}
				group.EstimatedHours = keyStrokes / (double)Program.kKeyStrokesPerHour;
			}
		}
	}
}
