using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Glyssen.VoiceActor;
using L10NSharp;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event System.Windows.Forms.DataGridViewRowsAddedEventHandler RowsAdded;
		private int m_currentId;

		public VoiceActorInformationGrid()
		{
			//RowsAdded = new DataGridViewRowsAddedEventHandler();
			InitializeComponent();

			m_currentId = 0;

			m_dataGrid[0, 0].Value = m_currentId++;

			m_dataGrid.RowsAdded += AssignAndIncrementId;
			m_dataGrid.RowsAdded += RaiseRowsAddedEvent;
		}

		private void AssignAndIncrementId(object sender, DataGridViewRowsAddedEventArgs e)
		{
			m_dataGrid[0, m_dataGrid.RowCount - 1].Value = m_currentId++;
		}

		public void SaveVoiceActorInformation(Project project)
		{
			VoiceActorList voiceActorInfo = new VoiceActorList();

			for (int i = 0; i < m_dataGrid.RowCount; i++)
			{
				VoiceActorEntity currentActor = new VoiceActorEntity();

				string[] rowValues = new string[m_dataGrid.ColumnCount];
				for (int j = 0; j < m_dataGrid.ColumnCount; j++)
				{
					object dataMember = m_dataGrid[j, i].Value;
					rowValues[j] = dataMember == null ? null : dataMember.ToString();
				}

				currentActor.Id = rowValues[0];
				currentActor.Name = rowValues[1];
				currentActor.Gender = rowValues[2];
				currentActor.Age = rowValues[3];

				if (!currentActor.isEmpty())
				{
					voiceActorInfo.Actors.Add(currentActor);
				}
			}

			project.SaveVoiceActorInformationData(voiceActorInfo);
		}

		public void RemoveSelectedRows(bool confirmWithUser)
		{
			bool deleteConfirmed = !confirmWithUser;

			if (confirmWithUser)
			{
				string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Message", "Are you sure you want to delete the selected rows?");
				string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");
				deleteConfirmed = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;
			}

			if (deleteConfirmed)
			{
				for (int i = m_dataGrid.SelectedRows.Count - 1; i >= 0; i--)
				{
					m_dataGrid.Rows.Remove(m_dataGrid.SelectedRows[i]);
				}
			}
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == toolStripMenuItem1)
			{
				RemoveSelectedRows(true);
			}
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				RemoveSelectedRows(true);
			}
		}

		private void RaiseRowsAddedEvent(object sender, DataGridViewRowsAddedEventArgs e)
		{
			DataGridViewRowsAddedEventHandler handler = RowsAdded;
			if(handler != null)
			{
				handler(sender, e);
			}
		}
	}
}
