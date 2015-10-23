using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using L10NSharp;
using SIL.Extensions;

namespace Glyssen.Dialogs
{
	public class MoveCharactersToGroupUndoAction : CharacterGroupsUndoAction
	{
		private readonly Project m_project;
		private readonly string m_destGroupName;
		private readonly IList<string> m_characterIdsMoved;
		private readonly IList<string> m_remainingCharacterIdsInSource;
		private int m_destGroupActor;
		private int m_sourceGroupActor;

		public MoveCharactersToGroupUndoAction(Project project, CharacterGroup sourceGroup, CharacterGroup destGroup, IList<string> characterIds)
		{
			m_project = project;
			m_characterIdsMoved = characterIds;
			m_sourceGroupActor = sourceGroup.VoiceActorId;

			if (destGroup != null)
				m_destGroupName = destGroup.Name;

			if (Do(sourceGroup, ref destGroup))
				m_remainingCharacterIdsInSource = new List<string>(sourceGroup.CharacterIds);

			m_destGroupActor = destGroup.VoiceActorId;
		}

		private bool Do(CharacterGroup sourceGroup, ref CharacterGroup destGroup)
		{
			if (destGroup == null)
				destGroup = CreateNewGroup();

			AddGroupAffected(destGroup);
			bool sourceSurvived;
			sourceGroup.CharacterIds.ExceptWith(m_characterIdsMoved);
			if (sourceGroup.CharacterIds.Count == 0 && !sourceGroup.IsVoiceActorAssigned)
			{
				m_project.CharacterGroupList.CharacterGroups.Remove(sourceGroup);
				sourceSurvived = false;
			}
			else
			{
				AddGroupAffected(sourceGroup);
				sourceSurvived = true;
			}

			destGroup.CharacterIds.AddRange(m_characterIdsMoved);
			return sourceSurvived;
		}

		public override string Description
		{
			get
			{
				if (IsSplit)
					return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.SplitGroup", "Split group");
				if (string.IsNullOrEmpty(m_destGroupName))
					return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.MoveCharactersToNewGroup", "Create new group");

				return string.Format(
					LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.MoveCharactersToGroup", "Move characters to {0} group"),
					m_destGroupName); }
		}

		public bool IsSplit { get; set; }

		private CharacterGroup CreateNewGroup()
		{
			var newGroupNumber = 1;

			while (m_project.CharacterGroupList.CharacterGroups.Any(t => t.GroupNumber == newGroupNumber))
				newGroupNumber++;

			var newGroup = new CharacterGroup(m_project);
			m_project.CharacterGroupList.CharacterGroups.Add(newGroup);

			return newGroup;
		}

		private CharacterGroup FindGroup(int actor, IEnumerable<string> characterIds)
		{
			try
			{
				return m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g =>
					g.VoiceActorId == actor && g.CharacterIds.IsSupersetOf(characterIds));
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}

		protected override bool PerformUndo()
		{
			CharacterGroup sourceGroup;
			if (m_remainingCharacterIdsInSource == null)
			{
				Debug.Assert(m_sourceGroupActor == -1, "If source actor was set, the source group should not have been removed and m_remainingCharacterIdsInSource should have been set to an empty list");
				sourceGroup = CreateNewGroup();
			}
			else
			{
				sourceGroup = FindGroup(m_sourceGroupActor, m_remainingCharacterIdsInSource);
				if (sourceGroup == null)
					return false;
			}
			CharacterGroup destGroup = FindGroup(m_destGroupActor, m_characterIdsMoved);
			if (destGroup == null)
				return false;

			sourceGroup.CharacterIds.AddRange(m_characterIdsMoved);
			AddGroupAffected(sourceGroup);

			if (destGroup.CharacterIds.SetEquals(m_characterIdsMoved))
				m_project.CharacterGroupList.CharacterGroups.Remove(destGroup);
			else
			{
				destGroup.CharacterIds.ExceptWith(m_characterIdsMoved);
				AddGroupAffected(destGroup);
			}

			return true;
		}

		protected override bool PerformRedo()
		{
			IEnumerable<string> charactersInSource = (m_remainingCharacterIdsInSource == null) ?
				m_characterIdsMoved :
				m_characterIdsMoved.Union(m_remainingCharacterIdsInSource);
			var sourceGroup = FindGroup(m_sourceGroupActor, charactersInSource);
			CharacterGroup destGroup;
			try
			{
				destGroup = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g =>
					g.VoiceActorId == m_destGroupActor && g.Name == m_destGroupName);
			}
			catch (InvalidOperationException)
			{
				destGroup = null;
			}

			Do(sourceGroup, ref destGroup);
			return true;
		}
	}
}
