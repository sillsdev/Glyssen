using System.Collections.Generic;

namespace Waxuquerque.Character
{
	public class CharacterGroupTemplate
	{
		private readonly Project m_project;
		public Dictionary<int, CharacterGroup> CharacterGroups { get; private set; }

		public CharacterGroupTemplate(Project project)
		{
			m_project = project;
			CharacterGroups = new Dictionary<int, CharacterGroup>();
		}

		public void AddCharacterToGroup(string characterId, int groupNumber)
		{
			CharacterGroup group;
			if (!CharacterGroups.TryGetValue(groupNumber, out group))
			{
				group = new CharacterGroup(m_project);
				CharacterGroups.Add(groupNumber, group);
			}
			group.CharacterIds.Add(characterId);
		}
	}
}
