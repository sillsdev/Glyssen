using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using L10NSharp;
using Glyssen.VoiceActor;

using System.Diagnostics;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private Project m_project { get; set; }

		public VoiceActorInformationDlg(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_dataGrid.RowsAdded += m_dataGrid_RowsAdded;
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

		private void m_dataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			m_btnNext.Enabled = true;
		}

		private void m_btnSave_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation(m_project);
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			m_dataGrid.SaveVoiceActorInformation(m_project);
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
