using System;
using System.Windows.Forms;
using Glyssen.Controls;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private readonly VoiceActorInformationViewModel m_viewModel;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool showNext = true)
		{
			InitializeComponent();

			m_viewModel = viewModel;
			m_viewModel.DeletingActors += ConfirmActorDeletion;

			m_dataGrid.Initialize(m_viewModel, false);

			m_dataGrid.Saved += m_dataGrid_Saved;
			m_dataGrid.UserAddedRow += m_dataGrid_UserAddedRow;
			m_dataGrid.UserRemovedRows += m_dataGrid_UserRemovedRows;

			m_btnNext.Enabled = m_dataGrid.RowCount > 1; // If 1, only one empty row

			m_btnNext.Visible = showNext;
			m_linkClose.Visible = showNext;
			m_btnOk.Visible = !showNext;
		}

		private void ConfirmActorDeletion(VoiceActorInformationViewModel sender, DeletingActorsEventArgs e)
		{
			if (e.Cancel)
				return;

			string msg;
			string title;

			if (e.CountOfAssignedActorsToDelete > 0)
			{
				if (e.CountOfActorsToDelete > 1)
				{
					msg =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessagePlural",
							"One or more of the selected actors is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actors?");
					title =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitlePlural",
							"Voice Actors Assigned");
				}
				else
				{
					msg =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.MessageSingular",
							"The selected actor is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actor?");
					title =
						LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.TitleSingular",
							"Voice Actor Assigned");
				}
			}
			else
			{
				title = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");

				if (e.CountOfActorsToDelete > 1)
				{
					msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessagePlural",
						"Are you sure you want to delete the selected actors?");
				}
				else
				{
					msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.MessageSingular",
						"Are you sure you want to delete the selected actor?");
				}
			}
			e.Cancel = MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.No;
		}

		private void m_dataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_dataGrid_UserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			m_btnNext.Enabled = true;
		}

		private void m_dataGrid_UserRemovedRows(object sender, DataGridViewRowsRemovedEventArgs e)
		{		
			if (e.RowCount == 1) // If 1, only one empty row
				m_btnNext.Enabled = false;
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();			
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			m_viewModel.AssessChanges();
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
