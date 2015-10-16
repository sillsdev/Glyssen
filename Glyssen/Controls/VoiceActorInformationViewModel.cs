using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.VoiceActor;
using L10NSharp;
using SIL.Extensions;
using SIL.ObjectModel;

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

		public delegate void DeletingActorsHandler(VoiceActorInformationViewModel sender, DeletingActorsEventArgs e);

		public event DeletingActorsHandler DeletingActors;

		public VoiceActorInformationViewModel(Project project)
		{
			m_currentId = 0;
			m_project = project;
			var actors = m_project.VoiceActorList.Actors;
			if (actors.Any())
				m_currentId = actors.Max(a => a.Id) + 1;
			BindingList = new SortableBindingList<VoiceActor.VoiceActor>(actors);
			BindingList.AddingNew += HandleAddingNew;

			foreach (var actor in project.VoiceActorList.Actors)
			{
				var characterGroup = project.CharacterGroupList.GetGroupsAssignedToActor(actor.Id).FirstOrDefault();
				m_originalActors.Add(new Tuple<VoiceActor.VoiceActor, CharacterGroup>(actor.MakeCopy(), characterGroup));
			}
		}

		public SortableBindingList<VoiceActor.VoiceActor> BindingList { get; set; }

		public int ActorCount { get { return m_project.VoiceActorList.Actors.Count; } }

		public void SaveVoiceActorInformation()
		{
			m_project.SaveVoiceActorInformationData();

			if (Saved != null)
			{
				Saved(BindingList, EventArgs.Empty);
			}
		}

		private void HandleAddingNew(object sender, AddingNewEventArgs e)
		{
			e.NewObject = new VoiceActor.VoiceActor { Id = m_currentId++ };
		}

		public bool DeleteVoiceActors(ISet<VoiceActor.VoiceActor> actors)
		{
			if (!actors.Any())
				return false;

			int actorsAssignedCount = actors.Count(t => m_project.CharacterGroupList.HasVoiceActorAssigned(t.Id));

			if (DeletingActors != null)
			{
				var e = new DeletingActorsEventArgs(actors.Count, actorsAssignedCount);
				DeletingActors(this, e);
				if (e.Cancel)
					return false;
			}

			if (actorsAssignedCount > 0)
			{
				foreach (var voiceActor in actors)
					m_project.CharacterGroupList.RemoveVoiceActor(voiceActor.Id);
				m_project.SaveCharacterGroupData();
			}

			m_removedActorIds.AddRange(actors.Select(a => a.Id));
			BindingList.RemoveAll(actors.Contains);
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
	}

	public class DeletingActorsEventArgs : CancelEventArgs
	{
		public int CountOfActorsToDelete { get; private set; }
		public int CountOfAssignedActorsToDelete { get; private set; }

		public DeletingActorsEventArgs(int countOfActorsToDelete, int countOfAssignedActorsToDelete)
		{
			CountOfActorsToDelete = countOfActorsToDelete;
			CountOfAssignedActorsToDelete = countOfAssignedActorsToDelete;
		}
	}
}
