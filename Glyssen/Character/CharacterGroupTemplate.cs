using System.Collections.Generic;

namespace Glyssen.Character
{
	public class CharacterGroupTemplate
	{
		public Dictionary<int, CharacterGroup> CharacterGroups { get; private set; }

		public CharacterGroupTemplate()
		{
			CharacterGroups = new Dictionary<int, CharacterGroup>();
		}

		public void AddCharacterToGroup(string characterId, int groupNumber)
		{
			CharacterGroup group;
			if (!CharacterGroups.TryGetValue(groupNumber, out group))
			{
				group = new CharacterGroup(groupNumber);
				CharacterGroups.Add(groupNumber, group);
			}
			group.CharacterIds.Add(characterId);
		}
	}
}
