using System;
using System.Collections.Generic;
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

namespace Glyssen.Dialogs
{
	class VoiceActorAssignmentViewModel
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
				CharacterGroups.AddRange(new CharacterGroupGenerator(project, m_keyStrokesByCharacterId).GenerateCharacterGroups());

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
			var unmatchedCharacterGroup = new CharacterGroup
			{
				GroupNumber = 999,
				CharacterIds = new CharacterIdHashSet(unmatchedCharacters)
			};
			CharacterGroups.Add(unmatchedCharacterGroup);
		}

		private void ProcessCharacterIds(CharacterGroup group, Dictionary<string, CharacterGroup> characterIdToCharacterGroup)
		{
			foreach (var characterId in group.CharacterIds)
				if (!characterIdToCharacterGroup.ContainsKey(characterId))
					characterIdToCharacterGroup.Add(characterId, group);
		}

		public void AddNewGroup()
		{
			var newGroupNumber = 1;

			while (CharacterGroups.Any(t => t.GroupNumber == newGroupNumber))
				newGroupNumber++;

			CharacterGroups.Add(new CharacterGroup(newGroupNumber));			
		}

		public void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();

			if (Saved != null)
				Saved(this, EventArgs.Empty);
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

		public bool MoveCharacterToGroup(string characterId, CharacterGroup destGroup, bool confirmWithUser = true)
		{
			CharacterGroup sourceGroup = CharacterGroups.FirstOrDefault(t => t.CharacterIds.Contains(characterId));

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

				testGroup.Add(characterId);
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
						"[" + CharacterVerseData.GetCharacterNameForUi(characterId) + "]",
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

			sourceGroup.CharacterIds.Remove(characterId);
			destGroup.CharacterIds.Add(characterId);

			RemoveUnusedGroups();
			m_project.CharacterGroupList.PopulateEstimatedHours(m_keyStrokesByCharacterId);
			SaveAssignments();

			return true;
		}

		public void RemoveUnusedGroups()
		{
			CharacterGroups.RemoveAll(t => t.CharacterIds.Count == 0);
		}
	}
}
