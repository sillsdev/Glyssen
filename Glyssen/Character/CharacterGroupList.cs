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

		public bool HasVoiceActorAssigned()
		{
			return CharacterGroups.Any(cg => cg.VoiceActorAssignedId != -1);
		}
	}
}
