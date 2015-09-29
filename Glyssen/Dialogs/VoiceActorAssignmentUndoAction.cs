using System.Linq;
using Glyssen.Character;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public class VoiceActorAssignmentUndoAction : IUndoAction
	{
		private readonly Project m_project;
		private readonly int m_newActorId;
		private readonly int m_oldActorId;

		public VoiceActorAssignmentUndoAction(Project project, CharacterGroup group, int newActorId)
		{
			m_project = project;
			m_oldActorId = group.VoiceActorId;
			m_newActorId = newActorId;
			group.AssignVoiceActor(newActorId);
		}

		private string ActorName
		{
			get { return m_project.VoiceActorList.GetVoiceActorById(m_newActorId).Name; }
		}

		public string Description
		{
			get { return string.Format(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.VoiceActorAssignment", "Assign voice actor {0}"), ActorName); }
		}

		public bool Undo()
		{
			var group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_newActorId);
			if (group == null)
				return false;
			group.AssignVoiceActor(m_oldActorId);
			return true;
		}

		public bool Redo()
		{
			var group = m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.VoiceActorId == m_oldActorId);
			if (group == null)
				return false;
			group.AssignVoiceActor(m_newActorId);
			return true;
		}
	}
}
