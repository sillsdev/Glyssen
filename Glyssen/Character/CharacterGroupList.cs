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
		public static CharacterGroupList LoadCharacterGroupListFromFile(string filename)
		{
			return XmlSerializationHelper.DeserializeFromFile<CharacterGroupList>(filename);
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has any voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool AnyVoiceActorAssigned()
		{
			return CharacterGroups.Any(cg => cg.VoiceActorAssignedId != -1);
		}

		/// <summary>
		/// <returns>true if any CharacterGroup has the specified voice actor assigned, false otherwise</returns>
		/// </summary>
		public bool HasVoiceActorAssigned(int voiceActorId)
		{
			return CharacterGroups.Any(cg => cg.VoiceActorAssignedId == voiceActorId);
		}

		public int CountVoiceActorsAssigned()
		{
			return CharacterGroups.Count(cg => cg.VoiceActorAssignedId != -1);
		}

		public void RemoveVoiceActor(int voiceActorId)
		{
			foreach (var group in CharacterGroups)
				if (group.VoiceActorAssignedId == voiceActorId)
					group.RemoveVoiceActor();
		}

		public void PopulateEstimatedHours(IEnumerable<BookScript> books)
		{
			Dictionary<string, int> keyStrokesByCharacterId = new Dictionary<string, int>();
			foreach (var book in books)
			{
				foreach (var block in book.GetScriptBlocks(true))
				{
					if (!keyStrokesByCharacterId.ContainsKey(block.CharacterId))
						keyStrokesByCharacterId.Add(block.CharacterId, 0);
					keyStrokesByCharacterId[block.CharacterId] += block.GetText(false).Length;
				}
			}

			foreach (var group in CharacterGroups)
			{
				int keyStrokes = 0;
				foreach (var characterId in group.CharacterIds)
				{
					if (keyStrokesByCharacterId.ContainsKey(characterId))
						keyStrokes += keyStrokesByCharacterId[characterId];
				}
				group.EstimatedHours = keyStrokes / (double)Program.kKeyStrokesPerHour;
			}
		}
	}
}
