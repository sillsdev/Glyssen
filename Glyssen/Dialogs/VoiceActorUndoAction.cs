using System;
using System.Collections.Generic;
using System.Linq;
using GlyssenEngine;
using GlyssenEngine.Character;
using GlyssenEngine.Utilities;
using GlyssenEngine.VoiceActor;
using SIL;

namespace Glyssen.Dialogs
{
	public interface IVoiceActorUndoAction : IUndoAction
	{
		string ActorAffected { get; }
		bool JustChangedName { get; }
	}

	public class VoiceActorEditUndoAction : IVoiceActorUndoAction
	{
		private readonly Project m_project;
		private readonly VoiceActor m_affectedActorInformation;
		private readonly VoiceActor m_previousActorInformation;

		public string ActorAffected
		{
			get { return m_affectedActorInformation == null ? null : m_affectedActorInformation.Name; }
		}

		public string PreviousNameOfActor
		{
			get { return m_previousActorInformation.Name; }
		}

		public bool JustChangedName
		{
			get
			{
				if (m_affectedActorInformation.Name == m_previousActorInformation.Name)
					return false;
				return m_affectedActorInformation.IsInterchangeableWith(m_previousActorInformation);
			}
		}

		public VoiceActorEditUndoAction(Project project, VoiceActor previousActorInformation)
		{
			if (previousActorInformation == null)
				throw new ArgumentNullException("previousActorInformation");

			m_project = project;
			var currentActor = m_project.VoiceActorList.GetVoiceActorById(previousActorInformation.Id);
			if (currentActor == null)
				throw new ArgumentException("The edited actor is not in the project.", "previousActorInformation");
			m_affectedActorInformation = currentActor.MakeCopy();
			m_previousActorInformation = previousActorInformation;
		}

		public string Description
		{
			get
			{
				if (JustChangedName)
				{
					return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.VoiceActorNameChange",
						"Change name of voice actor from {0} to {1}"), PreviousNameOfActor, ActorAffected);
				}
				return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.EditSingleVoiceActor",
					"Edit voice actor {0}"), ActorAffected);
			}
		}

		public bool Undo()
		{
			var affectedActor = m_project.VoiceActorList.GetVoiceActorById(m_affectedActorInformation.Id);

			if (affectedActor == null || m_project.VoiceActorList.AllActors.Any(a => !a.Equals(affectedActor) && a.Name == m_previousActorInformation.Name))
				return false;

			var index = m_project.VoiceActorList.AllActors.IndexOf(affectedActor);
			m_project.VoiceActorList.AllActors.RemoveAt(index);
			m_project.VoiceActorList.AllActors.Insert(index, m_previousActorInformation.MakeCopy());

			return true;
		}

		public bool Redo()
		{
			var affectedActor = m_project.VoiceActorList.GetVoiceActorById(m_previousActorInformation.Id);
			if (affectedActor == null || m_project.VoiceActorList.AllActors.Any(a => !a.Equals(affectedActor) && a.Name == m_affectedActorInformation.Name))
				return false;

			var index = m_project.VoiceActorList.AllActors.IndexOf(affectedActor);
			m_project.VoiceActorList.AllActors.RemoveAt(index);
			m_project.VoiceActorList.AllActors.Insert(index, m_affectedActorInformation.MakeCopy());

			return true;
		}
	}

	public class VoiceActorDeletedUndoAction : IVoiceActorUndoAction
	{
		private readonly Project m_project;
		private readonly VoiceActor m_deletedActor;
		private readonly IList<string> m_characterIdsOfAssignedGroup;

		public string ActorAffected
		{
			get { return null; }
		}

		public string DeletedActorName
		{
			get { return m_deletedActor.Name; }
		}

		public bool JustChangedName
		{
			get { return false; }
		}

		public VoiceActorDeletedUndoAction(Project project, VoiceActor deletedActor, CharacterGroup assignedGroup = null)
		{
			if (deletedActor == null)
				throw new ArgumentNullException("deletedActor");

			m_project = project;
			m_deletedActor = deletedActor;
			if (assignedGroup != null)
				m_characterIdsOfAssignedGroup = new List<string>(assignedGroup.CharacterIds);
		}

		public string Description
		{
			get
			{
				return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.DeleteVoiceActor",
					"Delete voice actor {0}"), DeletedActorName);
			}
		}

		private CharacterGroup GroupAssignedToActor
		{
			get { return m_project.CharacterGroupList.CharacterGroups.SingleOrDefault(g => g.CharacterIds.IsSupersetOf(m_characterIdsOfAssignedGroup)); }
		}

		public bool Undo()
		{
			if (m_project.VoiceActorList.AllActors.Any(a => a.Name == m_deletedActor.Name))
				return false;
			CharacterGroup assignedCharacterGroup = null;
			if (m_characterIdsOfAssignedGroup != null)
			{
				assignedCharacterGroup = GroupAssignedToActor;
				if (assignedCharacterGroup == null)
					return false;
			}
			var replacementActor = m_deletedActor.MakeCopy();
			while (m_project.VoiceActorList.AllActors.Any(a => a.Id == replacementActor.Id))
				replacementActor.Id++;
			m_project.VoiceActorList.AllActors.Add(replacementActor);
			if (assignedCharacterGroup != null)
				assignedCharacterGroup.AssignVoiceActor(replacementActor.Id);
			return true;
		}

		public bool Redo()
		{
			var actorToDelete = m_project.VoiceActorList.GetVoiceActorById(m_deletedActor.Id);
			if (actorToDelete == null)
				return true;

			if (m_characterIdsOfAssignedGroup != null)
			{
				var assignedCharacterGroup = GroupAssignedToActor;
				if (m_project.CharacterGroupList.GetGroupsAssignedToActor(actorToDelete.Id).Any(g => g != assignedCharacterGroup))
					return false;
				if (assignedCharacterGroup != null && assignedCharacterGroup.VoiceActorId == actorToDelete.Id)
					assignedCharacterGroup.RemoveVoiceActor();
			}
			m_project.VoiceActorList.AllActors.Remove(actorToDelete);
			return true;
		}
	}

	public class VoiceActorAddedUndoAction : IVoiceActorUndoAction
	{
		private readonly Project m_project;
		private readonly VoiceActor m_addedActorInformation;

		public string ActorAffected
		{
			get
			{
				return m_addedActorInformation.Name;
			}
		}

		public bool JustChangedName
		{
			get { return false; }
		}

		public VoiceActorAddedUndoAction(Project project, int affectedActor)
		{
			m_addedActorInformation = project.VoiceActorList.GetVoiceActorById(affectedActor);
			if (m_addedActorInformation == null)
			{
				throw new ArgumentException("Affected actor must be in the project", "affectedActor");
			}

			m_project = project;
			m_addedActorInformation = m_addedActorInformation.MakeCopy();
		}

		public string Description
		{
			get
			{
				return string.Format(Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.AddVoiceActor",
					"Add voice actor {0}"), ActorAffected);
			}
		}

		public bool Undo()
		{
			var affectedActor = m_project.VoiceActorList.GetVoiceActorById(m_addedActorInformation.Id);

			if (affectedActor == null || m_project.CharacterGroupList.GetGroupsAssignedToActor(affectedActor.Id).Any())
				return false;

			m_project.VoiceActorList.AllActors.Remove(affectedActor);
			return true;
		}

		public bool Redo()
		{
			if (m_project.VoiceActorList.AllActors.Any(a => a.Name == m_addedActorInformation.Name))
				return false;
			var replacementActor = m_addedActorInformation.MakeCopy();
			while (m_project.VoiceActorList.AllActors.Any(a => a.Id == replacementActor.Id))
				replacementActor.Id++;
			m_project.VoiceActorList.AllActors.Add(replacementActor);
			return true;
		}
	}

	public class VoiceActorEditingUndoAction : UndoActionSequence<IVoiceActorUndoAction>, ICharacterGroupsUndoAction
	{
		private readonly Project m_project;
		private Func<IVoiceActorUndoAction, string> GetAffectedActor { get; set; }

		public VoiceActorEditingUndoAction(Project project, IEnumerable<IVoiceActorUndoAction> actions) : base(actions)
		{
			m_project = project;
			GetAffectedActor = action => action.ActorAffected;
		}

		public override string Description
		{
			get
			{
				if (Actions.Count > 1)
					return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Undo.EditMultipleVoiceActors", "Edit voice actors");

				return base.Description;
			}
		}

		public IEnumerable<CharacterGroup> GroupsAffectedByLastOperation
		{
			get
			{
				return Actions.SelectMany(action => m_project.CharacterGroupList.CharacterGroups.Where(g =>
				{
					var actor = m_project.VoiceActorList.GetVoiceActorById(g.VoiceActorId);
					return (actor != null && actor.Name == GetAffectedActor(action));
				}));
			}
		}
	}
}
