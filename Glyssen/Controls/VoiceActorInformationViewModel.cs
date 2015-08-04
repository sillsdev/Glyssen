using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.VoiceActor;
using L10NSharp;
using SIL.Extensions;

namespace Glyssen.Controls
{
	public class VoiceActorInformationViewModel
	{
		private Project m_project;
		private int m_currentId;

		public EventHandler Saved;

		public VoiceActorInformationViewModel()
		{
			m_currentId = 0;
		}

		public VoiceActorSortableBindingList BindingList { get; set; }
		public IEnumerable<CharacterGroup> CharacterGroupsWithAssignedActors { get; set; }

		public void SaveVoiceActorInformation()
		{
			m_project.SaveVoiceActorInformationData();

			if (Saved != null)
			{
				Saved(BindingList, EventArgs.Empty);
			}
		}

		public void Initialize(Project project)
		{
			m_project = project;
			var actors = m_project.VoiceActorList.Actors;
			if (actors.Any())
				m_currentId = actors.Max(a => a.Id) + 1;
			BindingList = new VoiceActorSortableBindingList(actors);
			BindingList.CharacterGroups = CharacterGroupsWithAssignedActors;
			BindingList.AddingNew += HandleAddingNew;
		}

		private void HandleAddingNew(object sender, AddingNewEventArgs e)
		{
			e.NewObject = new VoiceActor.VoiceActor { Id = m_currentId++ };
		}

		public bool DeleteVoiceActors(IEnumerable<VoiceActor.VoiceActor> actors, bool confirmWithUser)
		{
			if (!actors.Any())
				return false;

			bool deleteConfirmed = !confirmWithUser;

			if (confirmWithUser)
			{
				int actorsAssignedCount = actors.Count(t => m_project.CharacterGroupList.HasVoiceActorAssigned(t.Id));

				if (actorsAssignedCount > 0)
				{
					string assignedMsg;
					string assignedTitle;
					if (actorsAssignedCount > 1)
					{
						assignedMsg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessagePlural", "One or more of the selected actors is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actors?");
						assignedTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitlePlural", "Voice Actors Assigned");
					}
					else
					{
						assignedMsg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessageSingular", "The selected actor is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actor?");
						assignedTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitleSingular", "Voice Actor Assigned");
					}

					if (MessageBox.Show(assignedMsg, assignedTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						foreach (var voiceActor in actors)
							m_project.CharacterGroupList.RemoveVoiceActor(voiceActor.Id);
						m_project.SaveCharacterGroupData();
						deleteConfirmed = true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					string dlgMessage;
					string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");

					if (actors.Count() > 1)
					{
						dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessagePlural", "Are you sure you want to delete the selected actors?");
					}
					else
					{
						dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessageSingular", "Are you sure you want to delete the selected actor?");
					}
					deleteConfirmed = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
				}
			}

			if (deleteConfirmed)
			{
				BindingList.RemoveAll(actors.Contains);
				SaveVoiceActorInformation();
			}
			return deleteConfirmed;
		}
	}
}
