using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public class RemoveVoiceActorAssignmentsUndoAction : CharacterGroupsUndoAction
	{
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_characterRestorationInfo = new Dictionary<string, int>();

		public RemoveVoiceActorAssignmentsUndoAction(Project project, CharacterGroup group) : base(group)
		{
			m_project = project;
			m_characterRestorationInfo.Add(group.Name, group.VoiceActorId);
			group.RemoveVoiceActor();
		}

		public RemoveVoiceActorAssignmentsUndoAction(Project project, IEnumerable<CharacterGroup> groups) : base(groups)
		{
			m_project = project;
			foreach (var characterGroup in groups.Where(g => g.IsVoiceActorAssigned && !m_project.IsCharacterGroupAssignedToCameoActor(g)))
			{
				m_characterRestorationInfo.Add(characterGroup.Name, characterGroup.VoiceActorId);
				characterGroup.RemoveVoiceActor();
			}
		}

		public override string Description
		{
			get
			{
				if (m_characterRestorationInfo.Count > 1)
					return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignments", "Remove voice actor assignment for multiple groups");
				CharacterGroup charGroup = m_project.GetGroupByName(m_characterRestorationInfo.Keys.Single());
				if (charGroup.CharacterIds.Count > 1)
					return string.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForGroup", "Remove voice actor assignment for {0} group"), charGroup.Name);
				return string.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForCharacter", "Remove voice actor assignment for {0}"), charGroup.CharacterIds.Single());
			}
		}

		protected override bool PerformUndo()
		{
			try
			{
				var assignmentsToRestore = m_characterRestorationInfo.ToDictionary(kvp => m_project.GetGroupByName(kvp.Key), kvp => kvp.Value);
				if (assignmentsToRestore.Count != m_characterRestorationInfo.Count)
					return false;
				foreach (var kvp in assignmentsToRestore.Where(kvp => kvp.Key.VoiceActorId != kvp.Value))
				{
					kvp.Key.AssignVoiceActor(kvp.Value);
					AddGroupAffected(kvp.Key);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		protected override bool PerformRedo()
		{
			var groupsToUnassign = m_characterRestorationInfo.Select(kvp => m_project.GetGroupByName(kvp.Key)).Where(g => g != null).ToList();
			if (m_characterRestorationInfo.Count != groupsToUnassign.Count)
				return false;

			foreach (var group in groupsToUnassign)
			{
				if (group.IsVoiceActorAssigned)
				{
					group.RemoveVoiceActor();
					AddGroupAffected(group);
				}
			}

			return true;
		}
	}
}
