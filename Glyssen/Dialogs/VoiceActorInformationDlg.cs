using System;
using System.Windows.Forms;
using Glyssen.Bundle;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private readonly Project m_project;

		public VoiceActorInformationDlg(Project project, bool showNext = true)
		{
			InitializeComponent();

			m_project = project;

			m_dataGrid.Initialize(m_project, false);

			m_dataGrid.Saved += m_dataGrid_Saved;
			m_dataGrid.UserAddedRow += m_dataGrid_UserAddedRow;
			m_dataGrid.UserRemovedRows += m_dataGrid_UserRemovedRows;

			m_btnNext.Enabled = m_dataGrid.RowCount > 1; // If 1, only one empty row

			m_btnNext.Visible = showNext;
			m_linkClose.Visible = showNext;
			m_btnOk.Visible = !showNext;
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
			m_project.VoiceActorStatus = VoiceActorStatus.Provided;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_project.VoiceActorStatus = VoiceActorStatus.UnProvided;
			DialogResult = DialogResult.Cancel;
			Close();			
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
