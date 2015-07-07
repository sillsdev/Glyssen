using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.ObjectModel;
using System.Collections.Generic;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event DataGridViewRowsRemovedEventHandler RowsRemoved;
		public event DataGridViewRowEventHandler UserAddedRow;
		public event DataGridViewCellMouseEventHandler CellDoubleClicked;
		private int m_currentId;
		private Project m_project;
		private SortableBindingList<VoiceActor.VoiceActor> m_bindingList;

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			m_currentId = 0;

			m_dataGrid.UserAddedRow += HandleUserAddedRow;
			m_dataGrid.RowsRemoved += HandleRowsRemoved;
			m_dataGrid.CellMouseDoubleClick += HandleDoubleClick;
		}

		public void Initialize(Project project)
		{
			m_project = project;
			LoadVoiceActorInformation();
		}

		public void SaveVoiceActorInformation()
		{
			m_project.SaveVoiceActorInformationData();
		}

		private void LoadVoiceActorInformation()
		{
			var actors = m_project.VoiceActorList.Actors;
			if (actors.Any())
				m_currentId = actors.Max(a => a.Id) + 1;
			m_bindingList = new SortableBindingList<VoiceActor.VoiceActor>(actors);
			m_dataGrid.DataSource = m_bindingList;
			m_bindingList.AddingNew += HandleAddingNew;
		}

		public VoiceActor.VoiceActor SelectedVoiceActorEntity
		{
			get { return m_dataGrid.SelectedRows[0].DataBoundItem as VoiceActor.VoiceActor; }
		}

		private void HandleAddingNew(object sender, AddingNewEventArgs e)
		{
			e.NewObject = new VoiceActor.VoiceActor { Id = m_currentId++ };
		}

		private void RemoveSelectedRows(bool confirmWithUser)
		{
			bool deleteConfirmed = !confirmWithUser;

			if (confirmWithUser)
			{
				string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Message", "Are you sure you want to delete the selected rows?");
				string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");
				deleteConfirmed = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
			}

			if (deleteConfirmed)
			{
				for (int i = m_dataGrid.SelectedRows.Count - 1; i >= 0; i--)
				{
					if (m_dataGrid.SelectedRows[i].Index != m_dataGrid.RowCount - 1)
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

		private void HandleUserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler handler = UserAddedRow;
			if (handler != null)
				handler(sender, e);
		}

		private void HandleRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			DataGridViewRowsRemovedEventHandler handler = RowsRemoved;
			if (handler != null)
				handler(sender, e);
		}

		private void HandleDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler handler = CellDoubleClicked;
			if (CellDoubleClicked != null)
				handler(sender, e);
		}
	}
}
