using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Glyssen.Character;
using Glyssen.Dialogs;
using GlyssenEngine.Casting;
using L10NSharp;
using SIL;
using SIL.Extensions;

namespace Glyssen.Controls
{
	#region SortBy enumeration
	public enum VoiceActorsSortedBy
	{
		OrderEntered,
		Name,
	}
	#endregion

	public class VoiceActorInformationViewModel
	{
		private const int kAscending = 1;
		private const int kDescending = -1;

		private readonly Project m_project;
		private int m_currentId;
		private readonly HashSet<int> m_removedActorIds = new HashSet<int>();
		private readonly List<Tuple<VoiceActor, CharacterGroup>> m_originalActors = new List<Tuple<VoiceActor, CharacterGroup>>();

		public IEnumerable<IVoiceActorUndoAction> Changes
		{
			get
			{
				foreach (var id in m_removedActorIds)
				{
					var removedActorInfo = m_originalActors.Find(a => a.Item1.Id == id);
					if (removedActorInfo != null)
						yield return new VoiceActorDeletedUndoAction(m_project, removedActorInfo.Item1, removedActorInfo.Item2);
				}
				foreach (var currentActor in m_project.VoiceActorList.AllActors)
				{
					Debug.Assert(!string.IsNullOrWhiteSpace(currentActor.Name));
					var originalActor = m_originalActors.Find(a => a.Item1.Id == currentActor.Id);
					if (originalActor == null)
						yield return new VoiceActorAddedUndoAction(m_project, currentActor.Id);
					else if (originalActor.Item1.Name != currentActor.Name || !originalActor.Item1.IsInterchangeableWith(currentActor))
						yield return new VoiceActorEditUndoAction(m_project, originalActor.Item1);
				}
			}
		}

		public EventHandler Saved { get; set; }

		public enum ActorValidationState
		{
			Valid,
			NoName,
		}

		public VoiceActorInformationViewModel(Project project)
		{
			m_currentId = 0;
			m_project = project;
			if (Actors.Any())
				m_currentId = Actors.Max(a => a.Id) + 1;

			InitialActorCount = Actors.Count;

			foreach (var actor in Actors)
			{
				var characterGroup = project.CharacterGroupList.GetGroupsAssignedToActor(actor.Id).FirstOrDefault();
				m_originalActors.Add(new Tuple<VoiceActor, CharacterGroup>(actor.MakeCopy(), characterGroup));
			}
		}

		public Project Project { get { return m_project; } }

		public int InitialActorCount { get; private set; }

		public bool DataHasChanged { get; private set; }

		public bool DataHasChangedInWaysThatMightAffectGroupGeneration
		{
			get { return Changes.Any(c => !c.JustChangedName) || Actors.Any(a => a.IsCameo); }
		}

		public List<VoiceActor> Actors { get { return m_project.VoiceActorList.AllActors; } }

		public IEnumerable<VoiceActor> ActiveActors { get { return m_project.VoiceActorList.ActiveActors; } }

		public void SaveVoiceActorInformation()
		{
			Debug.Assert((Actors.Count == 0) || (Actors.Last().Name != ""));

			m_project.SaveVoiceActorInformationData();
			DataHasChanged = true;

			if (Saved != null)
				Saved(this, EventArgs.Empty);
		}

		public VoiceActor AddNewActor()
		{
			var actor = new VoiceActor { Id = m_currentId++ };
			Actors.Add(actor);
			return actor;
		}

		public bool IsActorAssigned(VoiceActor actor)
		{
			return m_project.CharacterGroupList.HasVoiceActorAssigned(actor.Id);
		}

		public bool DeleteVoiceActors(ISet<VoiceActor> actors)
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

		public void SetInactive(VoiceActor actor, bool inactive)
		{
			if (actor.IsInactive == inactive)
				return;

			if (inactive && IsActorAssigned(actor))
			{
				m_project.CharacterGroupList.RemoveVoiceActor(actor.Id);
				m_project.SaveCharacterGroupData();
			}
			actor.IsInactive = inactive;
			SaveVoiceActorInformation();
		}

		public static DataTable GetGenderDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(ActorGender));
			table.Columns.Add("Name");
			table.Rows.Add(ActorGender.Male, Localizer.GetString("DialogBoxes.VoiceActorInformation.Gender.Male", "M - Male"));
			table.Rows.Add(ActorGender.Female, Localizer.GetString("DialogBoxes.VoiceActorInformation.Gender.Female", "F - Female"));
			return table;
		}

		public static DataTable GetAgeDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(ActorAge));
			table.Columns.Add("Name");
			table.Rows.Add(ActorAge.Adult, Localizer.GetString("DialogBoxes.VoiceActorInformation.Age.Adult", "A - Adult"));
			table.Rows.Add(ActorAge.Elder, Localizer.GetString("DialogBoxes.VoiceActorInformation.Age.Elder", "E - Elder"));
			table.Rows.Add(ActorAge.YoungAdult, Localizer.GetString("DialogBoxes.VoiceActorInformation.Age.YoungAdult", "Y - Young Adult"));
			table.Rows.Add(ActorAge.Child, Localizer.GetString("DialogBoxes.VoiceActorInformation.Age.Child", "C - Child"));
			return table;
		}

		public static DataTable GetVoiceQualityDataTable()
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(VoiceQuality));
			table.Columns.Add("Name");
			table.Rows.Add(VoiceQuality.Normal, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Normal", "N - Normal", "This feature not currently in use in Glyssen"));
			table.Rows.Add(VoiceQuality.Dramatic, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Dramatic", "D - Dramatic", "This feature not currently in use in Glyssen"));
			table.Rows.Add(VoiceQuality.Authoritative, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Authoritative", "A - Authoritative/Firm", "This feature not currently in use in Glyssen"));
			table.Rows.Add(VoiceQuality.Weak, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Weak", "W - Weak", "This feature not currently in use in Glyssen"));
			table.Rows.Add(VoiceQuality.Suspicious, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Suspicious", "S - Suspicious", "This feature not currently in use in Glyssen"));
			table.Rows.Add(VoiceQuality.Clear, Localizer.GetString("DialogBoxes.VoiceActorInformation.VoiceQuality.Clear", "C - Clear", "This feature not currently in use in Glyssen"));
			return table;
		}

		public bool IsDuplicateActorName(VoiceActor editedVoiceActor, string newActorName)
		{
			return Actors.Where(a => a != editedVoiceActor).Any(actor => actor.Name == newActorName);
		}

		public int CountOfAssignedActors(HashSet<VoiceActor> actorsToRemove)
		{
			return actorsToRemove.Count(IsActorAssigned);
		}

		public ActorValidationState ValidateActor(int index)
		{
			if (index < 0 || index >= Actors.Count)
				throw new ArgumentOutOfRangeException("index");
			var actor = Actors[index];
			if (string.IsNullOrWhiteSpace(actor.Name))
				return ActorValidationState.NoName;
			return ActorValidationState.Valid;
		}

		public void Sort(VoiceActorsSortedBy by, bool sortAscending)
		{
			Comparison<VoiceActor> how;
			int direction = sortAscending ? kAscending : kDescending;
			switch (by)
			{
				case VoiceActorsSortedBy.OrderEntered:
					how = (a, b) => a.Id < b.Id ? -1 : 1;
					break;
				case VoiceActorsSortedBy.Name:
					how = (a, b) => String.Compare(a.Name, b.Name, StringComparison.CurrentCulture) * direction;
					break;
				default:
					throw new ArgumentException("Unexpected sorting method", "by");
			}
			Actors.Sort((a, b) =>
			{
				int result = 0;
				if (!a.IsInactive && b.IsInactive)
					result = -1;
				if (a.IsInactive && !b.IsInactive)
					result = 1;
				if (result != 0)
					return result;
				return how.Invoke(a, b);
			});
		}
	}
}
