using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		public VoiceActorInformationDlg()
		{
			InitializeComponent();
		}

		private void RemoveSelectedRows(bool confirmWithUser)
		{
			bool deleteConfirmed = !confirmWithUser;

			if (confirmWithUser)
			{
				deleteConfirmed = MessageBox.Show("Are you sure you want to delete these rows?", "Confirm", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;
			}

			if (deleteConfirmed)
			{
				for (int i = m_dataGrid.SelectedRows.Count - 1; i >= 0; i--)
				{
					m_dataGrid.Rows.Remove(m_dataGrid.SelectedRows[i]);
				}

				if(m_dataGrid.RowCount <= 1)
				{
					m_btnNext.Enabled = false;
				}
			}
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

		private void m_dataGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				RemoveSelectedRows(true);
			}
		}

		private void m_dataGridContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == m_deleteRowsToolStripMenuItem)
			{
				RemoveSelectedRows(true);
			}
		}

		private void m_dataGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			m_btnNext.Enabled = true;
		}
	}
}
