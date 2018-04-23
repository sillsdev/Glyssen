using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using SIL;
using SIL.Extensions;
using Waxuquerque.Character;
using Waxuquerque.Properties;
using Waxuquerque.Rules;
using Waxuquerque.Utilities;
using Waxuquerque.ViewModel.Undo;
using Waxuquerque.VoiceActor;

namespace Waxuquerque.ViewModel
{
	#region SortBy enumeration
	public enum SortedBy
	{
		Name,
		Attributes,
		EstimatedTime,
		Actor,
	}
	#endregion

	public class VoiceActorAssignmentViewModel
	{
		private const int kAscending = 1;
		private const int kDescending = -1;

		private readonly Project m_project;

		public delegate void SavedEventHandler(VoiceActorAssignmentViewModel sender, IEnumerable<CharacterGroup> groupsAffected, bool requiresActorListRefresh);
		public event SavedEventHandler Saved;
		private readonly UndoStack<ICharacterGroupsUndoAction> m_undoStack = new UndoStack<ICharacterGroupsUndoAction>();
		private readonly Dictionary<string, HashSet<string>> m_findTextToMatchingCharacterIds = new Dictionary<string, HashSet<string>>();

		private Proximity ProjectProximity { get; }

		public VoiceActorAssignmentViewModel(Project project)
		{
			m_project = project;
			ProjectProximity = new Proximity(m_project, false);

			CharacterGroupAttribute<CharacterGender>.GetUiStringForValue = GetUiStringForCharacterGender;
			CharacterGroupAttribute<CharacterAge>.GetUiStringForValue = GetUiStringForCharacterAge;

			LogAndOutputToDebugConsole("Group".PadRight(7) + ": " + MinimumProximity.ReportHeader + Environment.NewLine +
				"-".PadRight(100, '-'));
			foreach (var group in CharacterGroups.OrderBy(g => g.GroupIdForUiDisplay))
				LogAndOutputToDebugConsole(group.GroupIdForUiDisplay.PadRight(7) + ": " + ProjectProximity.CalculateMinimumProximity(group.CharacterIds));
		}

		private static void LogAndOutputToDebugConsole(string message)
		{
			Debug.WriteLine(message);
			Logger.WriteEvent(message);
		}

		private static string GetUiStringForCharacterGender(CharacterGender characterGender)
		{
			switch (characterGender)
			{
				case CharacterGender.Male: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Male", "Male");
				case CharacterGender.Female: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Female", "Female");
				case CharacterGender.PreferMale: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.PreferMale", "Pref: Male");
				case CharacterGender.PreferFemale: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.PreferFemale", "Pref: Female");
// Probably don't want to show this anyway, and definitely not for narrators				case CharacterGender.Neuter: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Neuter", "Neuter");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForCharacterAge(CharacterAge characterAge)
		{
			switch (characterAge)
			{
				case CharacterAge.Child: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Child", "Child");
				case CharacterAge.Elder: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Elder", "Elder");
				case CharacterAge.YoungAdult: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForActorGender(ActorGender actorGender)
		{
			switch (actorGender)
			{
				case ActorGender.Male: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Male", "Male");
				case ActorGender.Female: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Female", "Female");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForActorAge(ActorAge actorAge)
		{
			switch (actorAge)
			{
				case ActorAge.Adult: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Adult", "Adult");
				case ActorAge.Child: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Child", "Child");
				case ActorAge.Elder: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Elder", "Elder");
				case ActorAge.YoungAdult: return Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		public string GetUiStringForCharacterGender(string localizedCharacterId)
		{
			CharacterDetail detail;
			var characterId = CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId];

			return m_project.AllCharacterDetailDictionary.TryGetValue(characterId, out detail) ?
				GetUiStringForCharacterGender(detail.Gender) : string.Empty;
		}

		public string GetUiStringForCharacterAge(string localizedCharacterId)
		{
			CharacterDetail detail;
			var characterId = CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId];

			return m_project.AllCharacterDetailDictionary.TryGetValue(characterId, out detail) ?
				GetUiStringForCharacterAge(detail.Age) : string.Empty;
		}

		public double GetEstimatedHoursForCharacter(string localizedCharacterId)
		{
			var characterId = CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId];
			return m_project.KeyStrokesByCharacterId[characterId] / Project.kKeyStrokesPerHour;
		}

		public IList<CharacterGroup> CharacterGroups { get { return m_project.CharacterGroupList.CharacterGroups; } }

		public List<String> UndoActions { get { return m_undoStack.UndoDescriptions; } }
		public List<String> RedoActions { get { return m_undoStack.RedoDescriptions; } }

		public Project Project
		{
			get { return m_project; }
		}

		public void RegenerateGroups(Action generate)
		{
			// TODO (PG-437): Create Undo action to store state before calling generate.
			generate();

			Save();
		}

		private void Save(ICharacterGroupsUndoAction actionToSave = null)
		{
			m_project.SaveCharacterGroupData();

			if (Saved != null)
			{
				if (actionToSave == null)
					actionToSave = m_undoStack.Peek();
				Saved(this, actionToSave == null ? null : actionToSave.GroupsAffectedByLastOperation, actionToSave is VoiceActorEditingUndoAction);
			}
		}

		public void AssignActorToGroup(int actorId, CharacterGroup group)
		{
			if (group.VoiceActorId != actorId)
			{
				RemoveVoiceActorAssignmentsUndoAction undoActionForRemovingPreviousAssignments = null;
				if (actorId == CharacterGroup.kNoActorAssigned)
				{
					m_undoStack.Push(new RemoveVoiceActorAssignmentsUndoAction(m_project, group));
				}
				else
				{
					var groups = m_project.CharacterGroupList.GetGroupsAssignedToActor(actorId).ToList();
					if (groups.Any())
					{
						undoActionForRemovingPreviousAssignments = new RemoveVoiceActorAssignmentsUndoAction(m_project, groups);
					}

					// Note: Creating the undo action actually does the assignment.
					var undoActionForNewAssignment = new VoiceActorAssignmentUndoAction(m_project, group, actorId);
					if (undoActionForRemovingPreviousAssignments == null)
						m_undoStack.Push(undoActionForNewAssignment);
					else
					{
						m_undoStack.Push(new CharacterGroupUndoActionSequence(undoActionForRemovingPreviousAssignments, undoActionForNewAssignment));
					}
				}
				Save();
			}
		}

		public bool CanRemoveAssignment(CharacterGroup group)
		{
			return (group.IsVoiceActorAssigned && !group.AssignedToCameoActor);
		}

		public void UnAssignActorFromGroups(IEnumerable<CharacterGroup> groups)
		{
			m_undoStack.Push(new RemoveVoiceActorAssignmentsUndoAction(m_project, groups));
			Save();
		}

		private CharacterGroup GetSourceGroupForMove(IList<string> characterIds, CharacterGroup destGroup)
		{
			if (characterIds.Count == 0)
				throw new ArgumentException(@"At least one characterId must be provided", "characterIds");

			// Currently, we assume all characterIds are coming from the same source group
			CharacterGroup sourceGroup = CharacterGroups.FirstOrDefault(t => t.CharacterIds.Contains(characterIds[0]));

			// REVIEW: The second part of this condition used to be done in the following "if" statement, but I can't
			// think of any reason why moving a character to the group it's already in should result in any unused groups.
			if (sourceGroup == null || sourceGroup == destGroup)
				return null;

			if (destGroup == null && !sourceGroup.IsVoiceActorAssigned && sourceGroup.CharacterIds.SetEquals(characterIds))
			{
				return null; // Moving all characetrs from an unassigned group to a new group would accomplish nothing.
			}
			return sourceGroup;
		}

		public bool CanMoveCharactersToGroup(IList<string> characterIds, CharacterGroup destGroup)
		{
			return GetSourceGroupForMove(characterIds, destGroup) != null;
		}

		public bool MoveCharactersToGroup(IList<string> characterIds, CharacterGroup destGroup,
			Func<MinimumProximity, bool> userConfirmationAfterProximityWarning = null)
		{
			CharacterGroup sourceGroup = GetSourceGroupForMove(characterIds, destGroup);

			if (sourceGroup == null)
				return false;

			if (destGroup != null && userConfirmationAfterProximityWarning != null && destGroup.CharacterIds.Count > 0)
			{
				var testGroup = new CharacterIdHashSet(destGroup.CharacterIds);
				var resultsBefore = ProjectProximity.CalculateMinimumProximity(testGroup);

				testGroup.AddRange(characterIds);
				var resultsAfter = ProjectProximity.CalculateMinimumProximity(testGroup);

				if (resultsBefore.IsBetterThan(resultsAfter) && !resultsAfter.IsAcceptable())
				{
					if (userConfirmationAfterProximityWarning(resultsAfter))
					{
						Logger.WriteEvent("User cancelled move of characters.");
						return false;
					}
					Logger.WriteEvent($">>>Moving {characterIds.Count} characters to group {destGroup.GroupId}:\r\n   {String.Join("\r\n   ", characterIds)}");
				}
				else
				{
					LogAndOutputToDebugConsole($"Moving {characterIds.Count} character(s) to group {destGroup.GroupId} changed the " +
						$"proximity from {resultsBefore.NumberOfBlocks} to {resultsAfter.NumberOfBlocks}.");
				}
			}

			m_undoStack.Push(new MoveCharactersToGroupUndoAction(m_project, sourceGroup, destGroup, characterIds));

			Save();

			return true;
		}

		public CharacterGroup SplitGroup(List<string> charactersToMove)
		{
			var returnValue = MoveCharactersToGroup(charactersToMove, null);
			if (returnValue)
			{
				((MoveCharactersToGroupUndoAction) m_undoStack.Peek()).IsSplit = true;
				return m_project.CharacterGroupList.CharacterGroups.Single(g => g.CharacterIds.Contains(charactersToMove[0]));
			}
			return null;
		}

		public DataTable GetMultiColumnActorDataTable(CharacterGroup group)
		{
			var table = new DataTable();
			table.Columns.Add("ID", typeof(int));
			table.Columns.Add("Category");
			table.Columns.Add("Icon", typeof(Image));
			table.Columns.Add("Name");
			table.Columns.Add("Gender");
			table.Columns.Add("Age");
			table.Columns.Add("Cameo");
			table.Columns.Add("SpecialUse");

			bool includeCameos = !(group != null && !group.AssignedToCameoActor);

			//TODO put the best matches first
			foreach (var actor in m_project.VoiceActorList.ActiveActors.Where(a => (!m_project.CharacterGroupList.HasVoiceActorAssigned(a.Id) && (includeCameos || !a.IsCameo))).OrderBy(a => a.Name))
			{
				table.Rows.Add(GetDataTableRow(actor, Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AvailableVoiceActors", "Available:")));
			}

			table.Rows.Add(
				-1,
				null,
				Resources.RemoveActor,
				"",
				//Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment", "Remove Voice Actor Assignment"),
				"",
				"",
				"",
				Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment", "Remove Voice Actor Assignment"));

			foreach (var actor in m_project.VoiceActorList.ActiveActors.Where(a => (m_project.CharacterGroupList.HasVoiceActorAssigned(a.Id) && (includeCameos || !a.IsCameo))).OrderBy(a => a.Name))
			{
				table.Rows.Add(GetDataTableRow(actor, Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AlreadyAssignedVoiceActors",
					"Assigned to a Character Group:")));
			}

			return table;
		}

		private object[] GetDataTableRow(VoiceActor.VoiceActor actor, string category)
		{
			return new object[]
			{
				actor.Id,
				category,
				null,
				actor.Name,
				GetUiStringForActorGender(actor.Gender),
				GetUiStringForActorAge(actor.Age),
				actor.IsCameo ? Localizer.GetString("DialogBoxes.VoiceActorAssignmentDlg.Cameo", "Cameo") : "",
				null
			};
		}

		public void Sort(SortedBy by, bool sortAscending)
		{
			Comparison<CharacterGroup> how;
			int direction = sortAscending ? kAscending : kDescending;
			switch (by)
			{
				case SortedBy.Name:
					how = (a, b) => CompareGroupNames(a, b) * direction;
					break;
				case SortedBy.Attributes:
					how = (a, b) => String.Compare(a.AttributesDisplay, b.AttributesDisplay, StringComparison.CurrentCulture) * direction;
					break;
				case SortedBy.Actor:
					how = (a, b) =>
					{
						var actorA = m_project.VoiceActorList.GetVoiceActorById(a.VoiceActorId);
						var nameA = actorA == null ? String.Empty : actorA.Name;
						var actorB = m_project.VoiceActorList.GetVoiceActorById(b.VoiceActorId);
						var nameB = actorB == null ? String.Empty : actorB.Name;
						return String.Compare(nameA, nameB, StringComparison.CurrentCulture) * direction;
					};
					break;
				case SortedBy.EstimatedTime:
					how = (a, b) => a.EstimatedHours.CompareTo(b.EstimatedHours) * direction;
					break;
				default:
					throw new ArgumentException(@"Unexpected sorting method", "by");
			}
			CharacterGroups.Sort(how);
		}

		public bool Undo()
		{
			var action = m_undoStack.Peek();
			if (action != null && m_undoStack.Undo())
			{
				Save(action);
				return true;
			}
			return false;
		}

		public bool Redo()
		{
			if (m_undoStack.CanRedo && m_undoStack.Redo())
			{
				Save();
				return true;
			}
			return false;
		}

		public void NoteActorChanges(IEnumerable<IVoiceActorUndoAction> changes)
		{
			if (changes.Any())
			{
				m_undoStack.Push(new VoiceActorEditingUndoAction(m_project, changes));
				Save();
			}
		}

		public Tuple<int, int> FindNextMatchingCharacter(string textToFind, int startingGroupIndex, int startingCharacterIndex)
		{
			if (textToFind == null)
				throw new ArgumentNullException("textToFind");
			textToFind = textToFind.Trim();
			if (textToFind.Length < 2)
				throw new ArgumentException(@"Value must contain at least two non-whitespace characters.", "textToFind");
			if (startingGroupIndex < 0 || startingGroupIndex >= CharacterGroups.Count)
				startingGroupIndex = 0;
			if (startingCharacterIndex < 0 || startingCharacterIndex >= CharacterGroups[startingGroupIndex].CharacterIds.Count)
				startingCharacterIndex = 0;

			var matchingCharacterIds = GetMatchingCharacterIds(textToFind);

			bool wrapped = false;
			for (int iGroup = startingGroupIndex; iGroup <= (wrapped ? startingGroupIndex : CharacterGroups.Count - 1); iGroup++)
			{
				// TODO: Instead of alphabetical list, we need the list as currently sorted in UI.
				var characterIdsInGroup = CharacterGroups[iGroup].CharacterIds.ToList();

				for (int iCharacter = (wrapped || iGroup > startingGroupIndex ? 0 : startingCharacterIndex + 1);
					iCharacter <= (wrapped && iGroup ==startingGroupIndex ? startingCharacterIndex : CharacterGroups[iGroup].CharacterIds.Count - 1);
					iCharacter++)
				{
					if (matchingCharacterIds.Contains(characterIdsInGroup[iCharacter], StringComparison.Ordinal))
						return new Tuple<int, int>(iGroup, iCharacter);
				}

				if (!wrapped && iGroup == CharacterGroups.Count - 1)
				{
					wrapped = true;
					iGroup = -1;
				}
			}
			return new Tuple<int, int>(-1, -1);
		}

		private HashSet<string> GetMatchingCharacterIds(string textToFind)
		{
			HashSet<string> matchingCharacterIds;
			if (!m_findTextToMatchingCharacterIds.TryGetValue(textToFind, out matchingCharacterIds))
			{
				if (CharacterDetailData.Singleton.GetAll().Count() !=
					CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary.Count)
				{
					// First time getting UI versions of character IDs (typically when running tests), so we need to force
					// population of CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary.
					foreach (var characterId in CharacterGroups.SelectMany(characterGroup => characterGroup.CharacterIds))
						CharacterVerseData.GetCharacterNameForUi(characterId);
				}
				matchingCharacterIds = new HashSet<string>();
				m_findTextToMatchingCharacterIds.Add(textToFind, matchingCharacterIds);
				foreach (var kvp in CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary)
				{
					if (kvp.Value.IndexOf(textToFind, StringComparison.OrdinalIgnoreCase) >= 0)
						matchingCharacterIds.Add(kvp.Key);
				}
			}
			return matchingCharacterIds;
		}

		public void CreateNewActorAndAssignToGroup(string voiceActorName, CharacterGroup group)
		{
			var actor = new VoiceActor.VoiceActor { Id = 99, Name = voiceActorName };
			m_project.VoiceActorList.AllActors.Add(actor);
			AssignActorToGroup(actor.Id, group);
		}

		public VoiceActor.VoiceActor AddNewActorToGroup(string actorName, CharacterGroup group)
		{
			Debug.Assert(!group.AssignedToCameoActor);

			var actorViewModel = new VoiceActorInformationViewModel(m_project);

			if (actorViewModel.IsDuplicateActorName(null, actorName) || actorName == "Remove Voice Actor Assignment")
				throw new InvalidOperationException("Attempting to add existing actor!");

			var newActor = actorViewModel.AddNewActor();
			newActor.Name = actorName;
			switch (group.GroupIdLabel)
			{
				case CharacterGroup.Label.Female:
					newActor.Gender = ActorGender.Female;
					break;
				case CharacterGroup.Label.Child:
					newActor.Age = ActorAge.Child;
					break;
			}
			AssignActorToGroup(newActor.Id, group);
			actorViewModel.SaveVoiceActorInformation();
			return newActor;
		}

		private static int CompareGroupNames(CharacterGroup x, CharacterGroup y)
		{
			var s1 = x.GroupIdForUiDisplay ?? string.Empty;
			var s2 = y.GroupIdForUiDisplay ?? string.Empty;

			// check for identical strings
			if (s1 == s2)
				return 0;

			// check for an empty string
			if (string.IsNullOrEmpty(s1))
				return -1;

			if (string.IsNullOrEmpty(s2))
				return 1;

			var len1 = s1.Length;
			var len2 = s2.Length;
			var currentIdx = 0;
			var d1 = '.';
			var d2 = '.';

			while ((currentIdx < len1) && (currentIdx < len2))
			{
				// compare once character from each string
				var result = string.Compare(s1, currentIdx, s2, currentIdx, 1);

				// if both characters are not the same, check for digits
				if (result != 0)
				{
					d1 = s1[currentIdx];
					d2 = s2[currentIdx];
					if (!char.IsDigit(d1) || !char.IsDigit(d2)) return result;

					// both characters are digits, compare the full numbers
					var val1 = int.Parse(Regex.Match(s1, @"\d+").Value);
					var val2 = int.Parse(Regex.Match(s2, @"\d+").Value);
					return (val1 < val2) ? -1 : 1;
				}

				currentIdx++;
			}

			// if you are here, one string starts with the other
			// the shorter string comes before the longer one
			if (!char.IsDigit(d1) || !char.IsDigit(d2)) return (len1 < len2) ? -1 : 1;

			// both ended with digits, so compare the full numbers
			var num1 = int.Parse(Regex.Match(s1, @"\d+").Value);
			var num2 = int.Parse(Regex.Match(s2, @"\d+").Value);
			return (num1 < num2) ? -1 : 1;
		}
	}
}
