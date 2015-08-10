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
		public EventHandler Saved;

		public VoiceActorAssignmentViewModel(Project project)
		{
			m_project = project;
			CanAssign = true;

			if (m_project.CharacterGroupList.CharacterGroups.Any())
				CharacterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);
			else
				CreateInitialGroupsFromTemplate();

			GenerateAttributes();
			m_project.CharacterGroupList.PopulateAttributesDisplay();
			m_project.CharacterGroupList.PopulateEstimatedHours(m_project.IncludedBooks);

#if DEBUG
			var p = new Proximity(m_project);
			foreach (var group in CharacterGroups.OrderBy(g => g.GroupNumber))
				Debug.WriteLine(group.GroupNumber + ": " + p.CalculateMinimumProximity(group.CharacterIds));
#endif
		}

		public bool CanAssign { get; set; }

		public SortableBindingList<CharacterGroup> CharacterGroups { get; set; }

		private void CreateInitialGroupsFromTemplate()
		{
			CharacterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);

			CharacterGroupTemplate charGroupTemplate;
			using (var tempFile = new TempFile())
			{
				File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
				ICharacterGroupSource charGroupSource = new CharacterGroupTemplateExcelFile(tempFile.Path);
				charGroupTemplate = charGroupSource.GetTemplate(m_project.VoiceActorList.Actors.Count);
			}

			ISet<string> matchedCharacterIds = new HashSet<string>();

			foreach (var group in charGroupTemplate.CharacterGroups.Values)
			{
				group.CharacterIds.IntersectWith(m_project.IncludedCharacterIds);

				if (!group.CharacterIds.Any())
					continue;

				CharacterGroups.Add(group);

				matchedCharacterIds.AddRange(group.CharacterIds);
			}

			// Add an extra group for any characters which weren't in the template
			var unmatchedCharacters = m_project.IncludedCharacterIds.Except(matchedCharacterIds);
			var unmatchedCharacterGroup = new CharacterGroup
			{
				GroupNumber = 999,
				CharacterIds = new CharacterIdHashSet(unmatchedCharacters)
			};
			CharacterGroups.Add(unmatchedCharacterGroup);
		}

		private void ProcessCharacterIds(CharacterGroup group, Dictionary<string, CharacterGroup> characterIdToCharacterGroup,
			ISet<CharacterDetail> standardCharacterDetails)
		{
			foreach (var characterId in group.CharacterIds)
			{
				if (!characterIdToCharacterGroup.ContainsKey(characterId))
					characterIdToCharacterGroup.Add(characterId, group);
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					standardCharacterDetails.Add(new CharacterDetail
					{
						Character = characterId,
						Gender = "Either",
						Age = "Middle Adult",
						Status = true
					});
				else if (CharacterVerseData.IsCharacterStandard(characterId, false))
					standardCharacterDetails.Add(new CharacterDetail {Character = characterId, Gender = "Either", Age = "Middle Adult"});
			}
		}

		public void AddNewGroup()
		{
			var newGroupNumber = 1;

			while (CharacterGroups.Any(t => t.GroupNumber == newGroupNumber))
				newGroupNumber++;

			CharacterGroups.Add(new CharacterGroup(newGroupNumber));			
		}

		private void GenerateAttributes()
		{
			var characterIdToCharacterGroup = new Dictionary<string, CharacterGroup>();
			ISet<CharacterDetail> standardCharacterDetails = new HashSet<CharacterDetail>();

			foreach (var group in CharacterGroups)
			{
				ProcessCharacterIds(group, characterIdToCharacterGroup, standardCharacterDetails);
				group.AgeAttributes.Clear();
				group.GenderAttributes.Clear();
				group.Status = false;
			}

			foreach (var detail in CharacterDetailData.Singleton.GetAll().Union(standardCharacterDetails))
			{
				if (characterIdToCharacterGroup.ContainsKey(detail.Character))
				{
					var group = characterIdToCharacterGroup[detail.Character];

					if (!string.IsNullOrWhiteSpace(detail.Gender))
						group.GenderAttributes.Add(detail.Gender);
					if (!string.IsNullOrWhiteSpace(detail.Age))
						group.AgeAttributes.Add(detail.Age);
					if (detail.Status)
						group.Status = true;
				}
			}

			m_project.CharacterGroupList.PopulateAttributesDisplay();
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

				HashSet<string> testGroup = new CharacterIdHashSet(destGroup.CharacterIds);
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
						"[" + CharacterVerseData.GetCharacterNameForUi(resultsAfter.FirstBlock.CharacterId) + "]",
						"[" + CharacterVerseData.GetCharacterNameForUi(resultsAfter.SecondBlock.CharacterId) + "]",
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
			GenerateAttributes();
			m_project.CharacterGroupList.PopulateEstimatedHours(m_project.IncludedBooks);
			SaveAssignments();

			return true;
		}

		public void RemoveUnusedGroups()
		{
			CharacterGroups.RemoveAll(t => t.CharacterIds.Count == 0);
		}
	}
}
