using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Properties;
using Glyssen.Rules;
using Glyssen.Utilities;
using Glyssen.VoiceActor;
using L10NSharp;
using SIL.Extensions;
using SIL.IO;
using SIL.Scripture;
using SIL.Windows.Forms.Progress;

namespace Glyssen.Dialogs
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
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private CharacterByKeyStrokeComparer m_characterByKeyStrokeComparer;
		public delegate void SavedEventHandler(VoiceActorAssignmentViewModel sender, IEnumerable<CharacterGroup> groupsAffected);
		public event SavedEventHandler Saved;
		private readonly UndoStack<ICharacterGroupsUndoAction> m_undoStack = new UndoStack<ICharacterGroupsUndoAction>();

		public VoiceActorAssignmentViewModel(Project project)
		{
			m_project = project;
			CanAssign = true;

			m_keyStrokesByCharacterId = m_project.GetKeyStrokesByCharacterId();

			if (!CharacterGroups.Any())
				GenerateGroupsWithProgress();

			CharacterGroupAttribute<CharacterGender>.GetUiStringForValue = GetUiStringForCharacterGender;
			CharacterGroupAttribute<CharacterAge>.GetUiStringForValue = GetUiStringForCharacterAge;
			m_project.CharacterGroupList.PopulateEstimatedHours(m_keyStrokesByCharacterId);

#if DEBUG
			var p = new Proximity(m_project);
			foreach (var group in CharacterGroups.OrderBy(g => g.GroupNumber))
				Debug.WriteLine(group.GroupNumber + ": " + p.CalculateMinimumProximity(group.CharacterIds));
#endif
		}

		private static string GetUiStringForCharacterGender(CharacterGender characterGender)
		{
			switch (characterGender)
			{
				case CharacterGender.Male: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Male", "Male");
				case CharacterGender.Female: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Female", "Female");
				case CharacterGender.PreferMale: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.PreferMale", "Pref: Male");
				case CharacterGender.PreferFemale: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.PreferFemale", "Pref: Female");
				case CharacterGender.Neuter: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterGender.Neuter", "Neuter");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForCharacterAge(CharacterAge characterAge)
		{
			switch (characterAge)
			{
				case CharacterAge.Child: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Child", "Child");
				case CharacterAge.Elder: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Elder", "Elder");
				case CharacterAge.YoungAdult: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForActorGender(ActorGender actorGender)
		{
			switch (actorGender)
			{
				case ActorGender.Male: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Male", "Male");
				case ActorGender.Female: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorGender.Female", "Female");
				default: return string.Empty;
			}
		}

		private static string GetUiStringForActorAge(ActorAge actorAge)
		{
			switch (actorAge)
			{
				case ActorAge.Adult: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Adult", "Adult");
				case ActorAge.Child: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Child", "Child");
				case ActorAge.Elder: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.Elder", "Elder");
				case ActorAge.YoungAdult: return LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CharacterAge.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		// REVIEW: What is this intended for? It's only ever set false in tests
		public bool CanAssign { get; set; }

		public List<CharacterGroup> CharacterGroups { get { return m_project.CharacterGroupList.CharacterGroups; } }

		public List<String> UndoActions { get { return m_undoStack.UndoDescriptions; } }
		public List<String> RedoActions { get { return m_undoStack.RedoDescriptions; } }

		private CharacterByKeyStrokeComparer ByKeyStrokeComparer
		{
			get
			{
				if (m_characterByKeyStrokeComparer == null)
					m_characterByKeyStrokeComparer = new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId);
				return m_characterByKeyStrokeComparer;
			}
		}

		public void RegenerateGroups(bool attemptToMaintainAssignments)
		{
			// Create a copy. Cameos are handled in the generation code (because we always maintain those assignments).
			var previousGroups = CharacterGroups.Where(g => !m_project.IsCharacterGroupAssignedToCameoActor(g)).ToList();

			GenerateGroupsWithProgress();

			if (attemptToMaintainAssignments && previousGroups.Count > 0)
			{
				// We assume the parts with the most keystrokes are most important to maintain
				var sortedDict = from entry in m_keyStrokesByCharacterId orderby entry.Value descending select entry;
				foreach (var entry in sortedDict)
				{
					string characterId = entry.Key;
					var previousGroupWithCharacter = previousGroups.FirstOrDefault(g => g.CharacterIds.Contains(characterId));
					if (previousGroupWithCharacter != null)
					{
						var newlyGeneratedGroupWithCharacter = CharacterGroups.FirstOrDefault(g => !g.IsVoiceActorAssigned && g.CharacterIds.Contains(characterId));
						if (newlyGeneratedGroupWithCharacter == null)
							continue;
						newlyGeneratedGroupWithCharacter.AssignVoiceActor(previousGroupWithCharacter.VoiceActorId);
						previousGroups.Remove(previousGroupWithCharacter);
						if (previousGroups.Count == 0)
							break;
					}
				}
			}

			m_project.CharacterGroupList.PopulateEstimatedHours(m_keyStrokesByCharacterId);

			Save();
		}

		private void GenerateGroupsWithProgress()
		{
			using (var progressDialog = new ProgressDialog())
			{
				progressDialog.ShowInTaskbar = false;
				progressDialog.Overview = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ProgressDialog.Overview", "Generating optimal character groups based on voice actor attributes.");
				progressDialog.CanCancel = false;
				progressDialog.BarStyle = ProgressBarStyle.Marquee;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += OnGenerateGroupsWorkerDoWork;
				worker.RunWorkerCompleted += (s, e) => { if (e.Error != null) throw e.Error; };
				progressDialog.BackgroundWorker = worker;
				progressDialog.ShowDialog();
			}
		}

		private void OnGenerateGroupsWorkerDoWork(object s, DoWorkEventArgs e)
		{
			var generatedGroups = new CharacterGroupGenerator(m_project, m_keyStrokesByCharacterId).GenerateCharacterGroups();
			CharacterGroups.Clear();
			CharacterGroups.AddRange(generatedGroups);
		}

		// Keep this method around for now in case we decide to support templates in some scenarios
		// ReSharper disable once UnusedMember.Local
		private void CreateInitialGroupsFromTemplate()
		{
			CharacterGroupTemplate charGroupTemplate;
			using (var tempFile = new TempFile())
			{
				File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
				ICharacterGroupSource charGroupSource = new CharacterGroupTemplateExcelFile(m_project, tempFile.Path);
				charGroupTemplate = charGroupSource.GetTemplate(m_project.VoiceActorList.Actors.Count);
			}

			HashSet<string> includedCharacterIds = new HashSet<string>();
			foreach (var book in m_project.IncludedBooks)
				foreach (var block in book.GetScriptBlocks(true))
					if (!block.CharacterIsUnclear())
						includedCharacterIds.Add(block.CharacterId);
			ISet<string> matchedCharacterIds = new HashSet<string>();

			foreach (var group in charGroupTemplate.CharacterGroups.Values)
			{
				group.CharacterIds.IntersectWith(includedCharacterIds);

				if (!group.CharacterIds.Any())
					continue;

				CharacterGroups.Add(group);

				matchedCharacterIds.AddRange(group.CharacterIds);
			}

			// Add an extra group for any characters which weren't in the template
			var unmatchedCharacters = includedCharacterIds.Except(matchedCharacterIds);
			var unmatchedCharacterGroup = new CharacterGroup(m_project, 999, ByKeyStrokeComparer);
			unmatchedCharacterGroup.CharacterIds.AddRange(unmatchedCharacters);
			CharacterGroups.Add(unmatchedCharacterGroup);
		}

		private void Save(ICharacterGroupsUndoAction actionToSave = null)
		{
			m_project.SaveCharacterGroupData();

			if (Saved != null)
			{
				if (actionToSave == null)
					actionToSave = m_undoStack.Peek();
				Saved(this, actionToSave == null ? null : actionToSave.GroupsAffectedByLastOperation);
			}
		}

		public void AssignActorToGroup(int actorId, CharacterGroup group)
		{
			if (CanAssign && group.VoiceActorId != actorId)
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
			return (group.IsVoiceActorAssigned && !m_project.IsCharacterGroupAssignedToCameoActor(group));
		}

		public void UnAssignActorFromGroups(IEnumerable<CharacterGroup> groups)
		{
			m_undoStack.Push(new RemoveVoiceActorAssignmentsUndoAction(m_project, groups));
			Save();
		}

		private CharacterGroup GetSourceGroupForMove(IList<string> characterIds, CharacterGroup destGroup)
		{
			if (characterIds.Count == 0)
				throw new ArgumentException("At least one characterId must be provided", "characterIds");

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

		public bool MoveCharactersToGroup(IList<string> characterIds, CharacterGroup destGroup, bool confirmWithUser = false)
		{
			CharacterGroup sourceGroup = GetSourceGroupForMove(characterIds, destGroup);

			if (sourceGroup == null)
				return false;

			if (destGroup != null && confirmWithUser && destGroup.CharacterIds.Count > 0)
			{
				var proximity = new Proximity(m_project);

				var testGroup = new CharacterIdHashSet(destGroup.CharacterIds);
				var resultsBefore = proximity.CalculateMinimumProximity(testGroup);
				int proximityBefore = resultsBefore.NumberOfBlocks;

				testGroup.AddRange(characterIds);
				var resultsAfter = proximity.CalculateMinimumProximity(testGroup);
				int proximityAfter = resultsAfter.NumberOfBlocks;

				if (proximityBefore > proximityAfter && proximityAfter <= Proximity.kDefaultMinimumProximity)
				{
					var firstReference = new BCVRef(BCVRef.BookToNumber(resultsAfter.FirstBook.BookId), resultsAfter.FirstBlock.ChapterNumber,
						resultsAfter.FirstBlock.InitialStartVerseNumber).ToString();

					var secondReference = new BCVRef(BCVRef.BookToNumber(resultsAfter.SecondBook.BookId), resultsAfter.SecondBlock.ChapterNumber,
						resultsAfter.SecondBlock.InitialStartVerseNumber).ToString();

					var dlgMessageFormat1 = (firstReference == secondReference) ?
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part1",
							"This move will result in a group with a minimum proximity of {0} blocks between [{1}] and [{2}] in {3}.") :
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part1",
							"This move will result in a group with a minimum proximity of {0} blocks between [{1}] in {3} and [{2}] in {4}.");
					dlgMessageFormat1 = string.Format(dlgMessageFormat1,
						resultsAfter.NumberOfBlocks,
						CharacterVerseData.GetCharacterNameForUi(resultsAfter.FirstBlock.CharacterIdInScript),
						CharacterVerseData.GetCharacterNameForUi(resultsAfter.SecondBlock.CharacterIdInScript),
						firstReference, secondReference);
					var dlgMessageFormat2 =
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part2",
							"Do you want to continue with this move?");

					var dlgMessage = string.Format(dlgMessageFormat1 + Environment.NewLine + Environment.NewLine + dlgMessageFormat2);
					var dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Title",
						"Confirm");

					if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) != DialogResult.Yes)
						return false;
				}
			}

			m_undoStack.Push(new MoveCharactersToGroupUndoAction(m_project, sourceGroup, destGroup, characterIds));

			m_project.CharacterGroupList.PopulateEstimatedHours(m_keyStrokesByCharacterId);
			Save();

			return true;
		}

		public bool SplitGroup(List<string> charactersToMove)
		{
			var returnValue = MoveCharactersToGroup(charactersToMove, null);
			if (returnValue)
				((MoveCharactersToGroupUndoAction) m_undoStack.Peek()).IsSplit = true;
			return returnValue;
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

			//TODO put the best matches first
			foreach (var actor in m_project.VoiceActorList.Actors.Where(a => !m_project.CharacterGroupList.HasVoiceActorAssigned(a.Id)).OrderBy(a => a.Name))
				table.Rows.Add(GetDataTableRow(actor, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AvailableVoiceActors", "Available:")));

			table.Rows.Add(
				-1,
				null,
				Resources.RemoveActor,
				LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment", "Remove Voice Actor Assignment"),
				"",
				"",
				"");

			foreach (var actor in m_project.VoiceActorList.Actors.Where(a => m_project.CharacterGroupList.HasVoiceActorAssigned(a.Id)).OrderBy(a => a.Name))
				table.Rows.Add(GetDataTableRow(actor, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Categories.AlreadyAssignedVoiceActors",
					"Assigned to a Character Group:")));

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
				actor.IsCameo ? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Cameo", "Cameo") : ""
			};
		}

		public void Sort(SortedBy by, bool sortAscending)
		{
			Comparison<CharacterGroup> how;
			int direction = sortAscending ? kAscending : kDescending;
			switch (by)
			{
				case SortedBy.Name:
					how = (a, b) => String.Compare(a.Name, b.Name, StringComparison.CurrentCulture) * direction;
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
					throw new ArgumentException("Unexpected sorting method", "by");
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
	}
}
