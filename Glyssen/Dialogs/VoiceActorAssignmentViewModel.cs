using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Properties;
using Glyssen.Rules;
using L10NSharp;
using SIL.Extensions;
using SIL.IO;
using SIL.ObjectModel;
using SIL.Windows.Forms.Progress;

namespace Glyssen.Dialogs
{
	public class VoiceActorAssignmentViewModel
	{
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		public EventHandler Saved;

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
				case CharacterAge.Child: return LocalizationManager.GetString("Age.Child", "Child");
				case CharacterAge.Elder: return LocalizationManager.GetString("Age.Elder", "Elder");
				case CharacterAge.YoungAdult: return LocalizationManager.GetString("Age.YoungAdult", "Young Adult");
				default: return string.Empty;
			}
		}

		public bool CanAssign { get; set; }

		public SortableBindingList<CharacterGroup> CharacterGroups { get; set; }

		public void RegenerateGroups()
		{
			GenerateGroupsWithProgress();
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
			var unmatchedCharacterGroup = new CharacterGroup(999, new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId));
			unmatchedCharacterGroup.CharacterIds.AddRange(unmatchedCharacters);
			CharacterGroups.Add(unmatchedCharacterGroup);
		}

		public CharacterGroup AddNewGroup()
		{
			var newGroupNumber = 1;

			while (CharacterGroups.Any(t => t.GroupNumber == newGroupNumber))
				newGroupNumber++;

			CharacterGroup newGroup = new CharacterGroup(newGroupNumber, new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId));
			CharacterGroups.Add(newGroup);

			return newGroup;
		}

		public void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();

			if (Saved != null)
				Saved(this, EventArgs.Empty);
		}

		public bool IsActorAssigned(VoiceActor.VoiceActor voiceActor)
		{
			return m_project.CharacterGroupList.HasVoiceActorAssigned(voiceActor.Id);
		}

		public void AssignActorToGroup(VoiceActor.VoiceActor actor, CharacterGroup group)
		{
			if (CanAssign)
			{
				group.AssignVoiceActor(actor);
				SaveAssignments();
			}
		}

		public void UnAssignActorFromGroup(CharacterGroup group)
		{
			group.RemoveVoiceActor();
			SaveAssignments();
		}

		public void UnAssignActorFromGroup(VoiceActor.VoiceActor actor)
		{
			m_project.CharacterGroupList.RemoveVoiceActor(actor.Id);
			SaveAssignments();
		}

		public void MoveActorFromGroupToGroup(CharacterGroup sourceGroup, CharacterGroup destGroup, bool swap = false)
		{
			VoiceActor.VoiceActor sourceActor = sourceGroup.VoiceActorAssigned;
			VoiceActor.VoiceActor destinationActor = destGroup.VoiceActorAssigned;

			destGroup.AssignVoiceActor(sourceActor);
			if (swap && destinationActor != null)
				sourceGroup.AssignVoiceActor(destinationActor);
			else
				sourceGroup.RemoveVoiceActor();

			SaveAssignments();
		}

		public bool MoveCharactersToGroup(IList<string> characterIds, CharacterGroup destGroup, bool confirmWithUser = true)
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

			if (confirmWithUser)
			{
				var proximity = new Proximity(m_project);

				var testGroup = new CharacterIdHashSet(destGroup.CharacterIds);
				var resultsBefore = proximity.CalculateMinimumProximity(testGroup);
				int proximityBefore = resultsBefore.NumberOfBlocks;

				testGroup.AddRange(characterIds);
				var resultsAfter = proximity.CalculateMinimumProximity(testGroup);
				int proximityAfter = resultsAfter.NumberOfBlocks;

				if ((proximityBefore == -1 || proximityBefore > proximityAfter) &&
					proximityAfter >= 0 &&
					proximityAfter <= Proximity.kDefaultMinimumProximity)
				{

					var dlgMessageFormat1 =
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part1",
							"You are about to move {0} from group #{1} into group #{2}." +
							" As a result, group #{2} will have a minimum proximity of {3} blocks between {4} and {5} in the verses {6} and {7}.");
					var dlgMessageFormat2 =
						LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.MoveCharacterDialog.Message.Part2",
							"Do you want to continue moving this character?");

					var dlgMessage = string.Format(dlgMessageFormat1 + Environment.NewLine + Environment.NewLine + dlgMessageFormat2,
//						"[" + CharacterVerseData.GetCharacterNameForUi(characterId) + "]",
"multiple characters",
						sourceGroup.GroupNumber, destGroup.GroupNumber,
						resultsAfter.NumberOfBlocks,
						"[" + CharacterVerseData.GetCharacterNameForUi(resultsAfter.FirstBlock.CharacterIdInScript) + "]",
						"[" + CharacterVerseData.GetCharacterNameForUi(resultsAfter.SecondBlock.CharacterIdInScript) + "]",
						resultsAfter.FirstBook.BookId + " " + resultsAfter.FirstBlock.ChapterNumber + ":" +
						resultsAfter.FirstBlock.InitialStartVerseNumber,
						resultsAfter.SecondBook.BookId + " " + resultsAfter.SecondBlock.ChapterNumber + ":" +
						resultsAfter.SecondBlock.InitialStartVerseNumber);
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

		public bool SplitGroup(CharacterGroup group, List<string> charactersToMove)
		{
			var newGroup = new CharacterGroup(0, new CharacterByKeyStrokeComparer(m_keyStrokesByCharacterId));
			m_project.CharacterGroupList.CharacterGroups.Add(newGroup);
			return MoveCharactersToGroup(charactersToMove, newGroup, false);
		}

		public void RemoveUnusedGroups()
		{
			CharacterGroups.RemoveAll(t => t.CharacterIds.Count == 0 && !t.IsVoiceActorAssigned);
		}
	}
}
