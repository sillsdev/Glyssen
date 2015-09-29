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
using Glyssen.VoiceActor;
using L10NSharp;
using SIL.Extensions;
using SIL.IO;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.Windows.Forms.Progress;

namespace Glyssen.Dialogs
{
	public class VoiceActorAssignmentViewModel
	{
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private CharacterByKeyStrokeComparer m_characterByKeyStrokeComparer;
		public EventHandler Saved;
		private UndoStack m_undoStack = new UndoStack();

		public VoiceActorAssignmentViewModel(Project project)
		{
			m_project = project;
			CanAssign = true;

			m_keyStrokesByCharacterId = m_project.GetKeyStrokesByCharacterId();

			CharacterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);
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

		public bool CanAssign { get; set; }

		public SortableBindingList<CharacterGroup> CharacterGroups { get; set; }

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
		private void CreateInitialGroupsFromTemplate()
		{
			CharacterGroupTemplate charGroupTemplate;
			using (var tempFile = new TempFile())
			{
				File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
				ICharacterGroupSource charGroupSource = new CharacterGroupTemplateExcelFile(tempFile.Path);
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
			var unmatchedCharacterGroup = new CharacterGroup(999, ByKeyStrokeComparer);
			unmatchedCharacterGroup.CharacterIds.AddRange(unmatchedCharacters);
			CharacterGroups.Add(unmatchedCharacterGroup);
		}

		public CharacterGroup AddNewGroup(IEnumerable<string> characterIds, out bool charactersMoved)
		{
			if (characterIds == null)
				throw new ArgumentNullException("characterIds");

			var characterList = characterIds.ToList();
			if (!characterList.Any())
				throw new ArgumentException("Group cannot be created with no characters.", "characterIds");

			var newGroupNumber = 1;

			while (CharacterGroups.Any(t => t.GroupNumber == newGroupNumber))
				newGroupNumber++;

			CharacterGroup newGroup = new CharacterGroup(newGroupNumber, ByKeyStrokeComparer);
			CharacterGroups.Add(newGroup);

			charactersMoved = MoveCharactersToGroup(characterList, newGroup);

			return newGroup;
		}

		public void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();

			if (Saved != null)
				Saved(this, EventArgs.Empty);
		}

		public bool IsActorAssigned(int voiceActorId)
		{
			return voiceActorId > -1 && m_project.CharacterGroupList.HasVoiceActorAssigned(voiceActorId);
		}

		public void AssignActorToGroup(VoiceActor.VoiceActor actor, CharacterGroup group)
		{
			if (CanAssign)
			{
				// Note: Creating the undo action actually does the assignment.
				m_undoStack.Push(new VoiceActorAssignmentUndoAction(m_project, group, actor.Id));
				SaveAssignments();
			}
		}

		public void UnAssignActorFromGroups(IEnumerable<CharacterGroup> groups)
		{
			m_undoStack.Push(new RemoveVoiceActorAssignmentsUndoAction(m_project, groups));
			SaveAssignments();
		}

		public void UnAssignActorFromGroup(int voiceActorId)
		{
			var groups = m_project.CharacterGroupList.GetGroupsAssignedToActor(voiceActorId).ToList();
			if (groups.Any())
			{
				m_undoStack.Push(new RemoveVoiceActorAssignmentsUndoAction(m_project, groups));
				SaveAssignments();
			}
		}

		public void MoveActorFromGroupToGroup(CharacterGroup sourceGroup, CharacterGroup destGroup, bool swap = false)
		{
			int sourceActor = sourceGroup.VoiceActorId;
			int destinationActor = destGroup.VoiceActorId;

			destGroup.AssignVoiceActor(sourceActor);
			if (swap && destGroup.IsVoiceActorAssigned)
				sourceGroup.AssignVoiceActor(destinationActor);
			else
				sourceGroup.RemoveVoiceActor();

			SaveAssignments();
		}

		public bool MoveCharactersToGroup(IList<string> characterIds, CharacterGroup destGroup, bool confirmWithUser = false)
		{
			if (characterIds.Count == 0)
				throw new ArgumentException("At least one characterId must be provided", "characterIds");

			// Currently, we assume all characterIds are coming from the same source group
			CharacterGroup sourceGroup = CharacterGroups.FirstOrDefault(t => t.CharacterIds.Contains(characterIds[0]));

			if (sourceGroup == null)
				return false;

			if (sourceGroup == destGroup || (sourceGroup.CharacterIds.Count <= 1 && destGroup.CharacterIds.Count == 0))
			{
				RemoveUnusedGroups();
				return false;
			}

			if (confirmWithUser && destGroup.CharacterIds.Count > 0)
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
					{
						RemoveUnusedGroups();
						return false;
					}
				}
			}

			sourceGroup.CharacterIds.ExceptWith(characterIds);
			destGroup.CharacterIds.AddRange(characterIds);

			RemoveUnusedGroups();
			m_project.CharacterGroupList.PopulateEstimatedHours(m_keyStrokesByCharacterId);
			SaveAssignments();

			return true;
		}

		public bool SplitGroup(List<string> charactersToMove)
		{
			var newGroup = new CharacterGroup(0, ByKeyStrokeComparer);
			m_project.CharacterGroupList.CharacterGroups.Add(newGroup);
			var returnValue = MoveCharactersToGroup(charactersToMove, newGroup);
			SaveAssignments();
			return returnValue;
		}

		public void RemoveUnusedGroups()
		{
			CharacterGroups.RemoveAll(t => t.CharacterIds.Count == 0 && !t.IsVoiceActorAssigned);
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
	}
}
