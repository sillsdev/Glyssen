using System;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Controls;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private Project m_project { get; set; }

		public VoiceActorInformationDlg(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_dataGrid.Initialize(m_project);

			m_dataGrid.Saved += m_dataGrid_Saved;
			m_dataGrid.UserAddedRow += m_dataGrid_UserAddedRow;
			m_dataGrid.UserRemovedRows += m_dataGrid_UserRemovedRows;

			m_btnNext.Enabled = m_dataGrid.RowCount > 1; // If 1, only one empty row
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

		private void m_btnSave_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation();
			m_project.VoiceActorStatus = VoiceActorStatus.UnProvided;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation();
			m_project.VoiceActorStatus = VoiceActorStatus.Provided;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
