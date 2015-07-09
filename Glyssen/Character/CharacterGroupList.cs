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
	}
}
