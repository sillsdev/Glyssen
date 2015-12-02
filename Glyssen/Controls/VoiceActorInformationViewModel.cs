using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.VoiceActor;
using L10NSharp;
using SIL.Extensions;

namespace Glyssen.Controls
{
	public class VoiceActorInformationViewModel
	{
		private readonly Project m_project;
		private int m_currentId;
		private readonly List<IVoiceActorUndoAction> m_undoActions = new List<IVoiceActorUndoAction>();
		private readonly HashSet<int> m_removedActorIds = new HashSet<int>();
		private readonly List<Tuple<VoiceActor.VoiceActor, CharacterGroup>> m_originalActors = new List<Tuple<VoiceActor.VoiceActor, CharacterGroup>>();

		public IEnumerable<IVoiceActorUndoAction> Changes { get { return m_undoActions; } }

		public EventHandler Saved;

		public VoiceActorInformationViewModel(Project project)
		{
			m_currentId = 0;
			m_project = project;
			if (Actors.Any())
				m_currentId = Actors.Max(a => a.Id) + 1;

			foreach (var actor in Actors)
			{
				var characterGroup = project.CharacterGroupList.GetGroupsAssignedToActor(actor.Id).FirstOrDefault();
				m_originalActors.Add(new Tuple<VoiceActor.VoiceActor, CharacterGroup>(actor.MakeCopy(), characterGroup));
			}
		}

		public List<VoiceActor.VoiceActor> Actors { get { return m_project.VoiceActorList.Actors; } }

		public void SaveVoiceActorInformation()
		{
			m_project.SaveVoiceActorInformationData();

			if (Saved != null)
				Saved(this, EventArgs.Empty);
		}

		public VoiceActor.VoiceActor AddNewActor()
		{
			var actor = new VoiceActor.VoiceActor { Id = m_currentId++ };
			Actors.Add(actor);
			return actor;
		}

		public bool IsActorAssigned(VoiceActor.VoiceActor actor)
		{
			return m_project.CharacterGroupList.HasVoiceActorAssigned(actor.Id);
		}

		public bool DeleteVoiceActors(ISet<VoiceActor.VoiceActor> actors)
		{
			if (!actors.Any())
				return false;

			bool assignedActorRemoved = false;
			foreach (var voiceActor in actors)
			{
				if (IsActorAssigned(voiceActor))
				{
					m_project.CharacterGroupList.RemoveVoiceActor(voiceActor.Id);
					assignedActorRemoved = true;
				}
				Actors.Remove(voiceActor);
			}
			if (assignedActorRemoved)
				m_project.SaveCharacterGroupData();

			m_removedActorIds.AddRange(actors.Select(a => a.Id));
			SaveVoiceActorInformation();

			return true;
		}

		public static DataTable GetGenderDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(ActorGender));
			table.Columns.Add("Name");
			table.Rows.Add(ActorGender.Male, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Gender.Male", "M - Male"));
			table.Rows.Add(ActorGender.Female, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Gender.Female", "F - Female"));
			return table;
		}

		public static DataTable GetAgeDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(ActorAge));
			table.Columns.Add("Name");
			table.Rows.Add(ActorAge.Adult, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Age.Adult", "A - Adult"));
			table.Rows.Add(ActorAge.Elder, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Age.Elder", "E - Elder"));
			table.Rows.Add(ActorAge.YoungAdult, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Age.YoungAdult", "Y - Young Adult"));
			table.Rows.Add(ActorAge.Child, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.Age.Child", "C - Child"));
			return table;
		}

		public static DataTable GetVoiceQualityDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(VoiceQuality));
			table.Columns.Add("Name");
			table.Rows.Add(VoiceQuality.Normal, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Normal", "N - Normal"));
			table.Rows.Add(VoiceQuality.Dramatic, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Dramatic", "D - Dramatic"));
			table.Rows.Add(VoiceQuality.Authoritative, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Authoritative", "A - Authoritative/Firm"));
			table.Rows.Add(VoiceQuality.Weak, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Weak", "W - Weak"));
			table.Rows.Add(VoiceQuality.Suspicious, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Suspicious", "S - Suspicious"));
			table.Rows.Add(VoiceQuality.Clear, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Clear", "C - Clear"));
			return table;
		}

		public static DataTable GetSpecialRoleDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof (SpecialRole));
			table.Columns.Add("Name");
			table.Rows.Add(SpecialRole.None, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.SpecialRole.None", "None"));
			table.Rows.Add(SpecialRole.Cameo, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.SpecialRole.Cameo", "Cameo"));
			table.Rows.Add(SpecialRole.Narrator, LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.SpecialRole.Narrator", "Narrator"));
			return table;
		}

		public void AssessChanges()
		{
			foreach (var id in m_removedActorIds)
			{
				var removedActorInfo = m_originalActors.Find(a => a.Item1.Id == id);
				m_undoActions.Add(new VoiceActorDeletedUndoAction(m_project, removedActorInfo.Item1, removedActorInfo.Item2));
			}
			foreach (var currentActor in m_project.VoiceActorList.Actors)
			{
				var originalActor = m_originalActors.Find(a => a.Item1.Id == currentActor.Id);
				if (originalActor == null)
					m_undoActions.Add(new VoiceActorAddedUndoAction(m_project, currentActor.Id));
				else if (originalActor.Item1.Name != currentActor.Name || !originalActor.Item1.IsInterchangeableWith(currentActor))
					m_undoActions.Add(new VoiceActorEditUndoAction(m_project, originalActor.Item1));
			}
		}

		public bool IsDuplicateActorName(VoiceActor.VoiceActor editedVoiceActor, string newActorName)
		{
			return Actors.Where(a => a != editedVoiceActor).Any(actor => actor.Name == newActorName);
		}

		public int CountOfAssignedActors(HashSet<VoiceActor.VoiceActor> actorsToRemove)
		{
			return actorsToRemove.Count(IsActorAssigned);
		}
	}
}
