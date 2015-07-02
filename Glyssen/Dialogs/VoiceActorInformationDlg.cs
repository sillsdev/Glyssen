using System;
using System.Windows.Forms;

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
			m_dataGrid.RowsRemoved += m_dataGrid_RowsRemoved;
			m_dataGrid.UserAddedRow += m_dataGrid_UserAddedRow;
		}

		//Todo: Selecting combo box item should move to next field
		//private void m_dataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		//{
		//	DataGridViewCell cell = m_dataGrid.CurrentCell;

		//	ComboBox box = e.Control as ComboBox;

		//	if (cell.ColumnIndex > 0)
		//	{
		//		//Todo: Selecting combo box item should move to next field
		//		box.SelectedIndexChanged -= box_SelectedIndexChanged;
		//		box.SelectedIndexChanged += box_SelectedIndexChanged;
		//	}
		//}

		//private void box_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	m_dataGrid.MoveToNextField();
		//}

		private void m_dataGrid_UserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			m_btnNext.Enabled = true;
		}

		private void m_dataGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			if (e.RowCount == 1)
				m_btnNext.Enabled = false;
		}

		private void m_btnSave_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation();
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
