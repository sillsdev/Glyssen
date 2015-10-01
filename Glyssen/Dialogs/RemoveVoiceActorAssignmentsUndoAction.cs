using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using Glyssen.Utilities;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public class RemoveVoiceActorAssignmentsUndoAction : IUndoAction
	{
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_characterRestorationInfo = new Dictionary<string, int>();

		//private string GetGroupName(CharacterGroup group)
		//{
		//	return group.Name ?? String.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.GroupForActor", ""), m_project.VoiceActorList.GetVoiceActorById(group.VoiceActorId).Name);
		//}

		public RemoveVoiceActorAssignmentsUndoAction(Project project, CharacterGroup group)
		{
			m_project = project;
			m_characterRestorationInfo.Add(group.Name, group.VoiceActorId);
			group.RemoveVoiceActor();
		}

		public RemoveVoiceActorAssignmentsUndoAction(Project project, IEnumerable<CharacterGroup> groups)
		{
			m_project = project;
			foreach (var characterGroup in groups.Where(g => g.IsVoiceActorAssigned))
			{
				m_characterRestorationInfo.Add(characterGroup.Name, characterGroup.VoiceActorId);
				characterGroup.RemoveVoiceActor();
			}
		}

		private CharacterGroup GetGroupByName(string name)
		{
			var grp = m_project.CharacterGroupList.GetGroupByName(name);
			return grp ?? m_project.CharacterGroupList.GroupContainingCharacterId(name);
		}

		public string Description
		{
			get
			{
				if (m_characterRestorationInfo.Count > 1)
					return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignments", "Remove voice actor assignment for multiple groups");
				CharacterGroup charGroup = GetGroupByName(m_characterRestorationInfo.Keys.Single());
				if (charGroup.CharacterIds.Count > 1)
					return string.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForGroup", "Remove voice actor assignment for {0} group"), charGroup.Name);
				return string.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForCharacter", "Remove voice actor assignment for {0}"), charGroup.CharacterIds.Single());
			}
		}

		public bool Undo()
		{
			try
			{
				var assignmentsToRestore = m_characterRestorationInfo.ToDictionary(kvp => GetGroupByName(kvp.Key), kvp => kvp.Value);
				if (assignmentsToRestore.Count != m_characterRestorationInfo.Count)
					return false;
				foreach (var kvp in assignmentsToRestore)
					kvp.Key.AssignVoiceActor(kvp.Value);
			}
			catch (Exception)
			{
				return false;
			}
				//new Dictionary<CharacterGroup, int>(m_characterRestorationInfo.Count);
			//foreach (var kvp in m_characterRestorationInfo)
			//{
			//	CharacterGroup charGroup = GetGroupByName(kvp.Key);
			//	if (charGroup == null)
			//		return false;
			//	charGroup.AssignVoiceActor(kvp.Value);
			//}
			//var group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_newActor);
			//if (group == null)
			//	return false;
			//group.AssignVoiceActor(m_oldActor);
			return true;
		}

		public bool Redo()
		{
			//var group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_oldActor);
			//if (group == null)
			//	return false;
			//group.AssignVoiceActor(m_newActor);
			return true;
		}
	}
}
