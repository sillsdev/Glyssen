using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using SIL;

namespace Glyssen.Dialogs
{
	public class RemoveVoiceActorAssignmentsUndoAction : CharacterGroupsUndoAction
	{
		private const string kEmptyGroup = "~Group with no character IDs~";
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_characterRestorationInfo = new Dictionary<string, int>();

		public RemoveVoiceActorAssignmentsUndoAction(Project project, CharacterGroup group)
		{
			m_project = project;
			if (group.CharacterIds.Count == 0)
			{
				m_characterRestorationInfo[kEmptyGroup] = group.VoiceActorId;
				m_project.CharacterGroupList.CharacterGroups.Remove(group);
			}
			else
			{
				AddGroupAffected(group);
				m_characterRestorationInfo.Add(group.GroupId, group.VoiceActorId);
				group.RemoveVoiceActor();
			}
		}

		public RemoveVoiceActorAssignmentsUndoAction(Project project, IEnumerable<CharacterGroup> groups) : base(groups)
		{
			m_project = project;
			foreach (var characterGroup in groups.Where(g => g.IsVoiceActorAssigned && !g.AssignedToCameoActor))
			{
				m_characterRestorationInfo.Add(characterGroup.GroupId, characterGroup.VoiceActorId);
				characterGroup.RemoveVoiceActor();
			}
		}

		public override string Description
		{
			get
			{
				if (m_characterRestorationInfo.Count > 1)
					return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignments", "Remove voice actor assignment for multiple groups");

				var groupId = m_characterRestorationInfo.Keys.Single();

				if (groupId == kEmptyGroup)
					return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForGroupWithNoCharacters", "Remove voice actor assignment");

				CharacterGroup charGroup = m_project.GetGroupById(groupId);
				if (charGroup.CharacterIds.Count > 1)
					return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForGroup", "Remove voice actor assignment for {0} group"), charGroup.GroupIdForUiDisplay);
				return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.RemoveVoiceActorAssignmentForCharacter", "Remove voice actor assignment for {0}"), charGroup.CharacterIds.Single());
			}
		}

		private IEnumerable<KeyValuePair<string, int>> ExistingGroupsToRestore
		{
			get { return m_characterRestorationInfo.Where(kvp => kvp.Key != kEmptyGroup); }
		}

		protected override bool PerformUndo()
		{
			try
			{
				var assignmentsToRestore = ExistingGroupsToRestore.ToDictionary(kvp => m_project.GetGroupById(kvp.Key), kvp => kvp.Value);
				if (assignmentsToRestore.Count != ExistingGroupsToRestore.Count())
					return false;

				int actorForDeletedGroup;
				if (m_characterRestorationInfo.TryGetValue(kEmptyGroup, out actorForDeletedGroup))
				{
					var emptyGroup = new CharacterGroup(m_project);
					m_project.CharacterGroupList.CharacterGroups.Add(emptyGroup);
					emptyGroup.AssignVoiceActor(actorForDeletedGroup);
					AddGroupAffected(emptyGroup);
				}

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
			var groupsToUnassign = ExistingGroupsToRestore.Select(kvp => m_project.GetGroupById(kvp.Key)).Where(g => g != null).ToList();
			if (groupsToUnassign.Count != ExistingGroupsToRestore.Count())
				return false;

			int actorForDeletedGroup;
			if (m_characterRestorationInfo.TryGetValue(kEmptyGroup, out actorForDeletedGroup))
			{
				var emptyGroupsToRemove = m_project.CharacterGroupList.GetGroupsAssignedToActor(actorForDeletedGroup)
						.Where(g => !g.CharacterIds.Any()).ToList();

				if (!emptyGroupsToRemove.Any())
					return false;
				foreach (var emptyGroup in emptyGroupsToRemove)
				{
					m_project.CharacterGroupList.CharacterGroups.Remove(emptyGroup);
					AddGroupAffected(emptyGroup);
				}
			}

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
