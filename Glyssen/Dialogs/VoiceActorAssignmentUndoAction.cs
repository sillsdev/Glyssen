using System;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using SIL;

namespace Glyssen.Dialogs
{
	public class VoiceActorAssignmentUndoAction : CharacterGroupsUndoAction
	{
		private readonly Project m_project;
		private readonly int m_newActorId;
		private readonly int m_oldActorId;
		private readonly string m_groupId;
		private readonly string m_origActorName;

		public VoiceActorAssignmentUndoAction(Project project, CharacterGroup group, int newActorId) : base(group)
		{
			m_project = project;
			m_oldActorId = group.VoiceActorId;
			m_newActorId = newActorId;
			m_groupId = group.GroupId;
			m_origActorName = m_project.VoiceActorList.GetVoiceActorById(m_newActorId).Name;
			group.AssignVoiceActor(newActorId);
		}

		private string ActorName
		{
			get
			{
				var actor = m_project.VoiceActorList.GetVoiceActorById(m_newActorId);
				return actor?.Name ?? m_origActorName; }
		}

		public override string Description
		{
			get { return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.VoiceActorAssignment", "Assign voice actor {0}"), ActorName); }
		}

		protected override bool PerformUndo()
		{
			CharacterGroup group = null;
			try
			{
				group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_newActorId);
			}
			catch (InvalidOperationException)
			{
			}
			if (group == null)
				group = m_project.CharacterGroupList.GetGroupById(m_groupId);
			if (group == null)
				return false;
			group.AssignVoiceActor(m_oldActorId);
			AddGroupAffected(group);
			return true;
		}

		protected override bool PerformRedo()
		{
			CharacterGroup group = null;
			try
			{
				group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_oldActorId);
			}
			catch (InvalidOperationException)
			{
			}
			if (group == null)
				group = m_project.CharacterGroupList.GetGroupById(m_groupId);
			if (group == null)
				return false;
			group.AssignVoiceActor(m_newActorId);
			AddGroupAffected(group);
			return true;
		}
	}
}
