using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.ObjectModel;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event EventHandler TableUpdated;
		public event DataGridViewRowEventHandler UserAddedRow;
		public event DataGridViewRowsRemovedEventHandler UserRemovedRows;
		public event DataGridViewCellMouseEventHandler CellDoubleClicked;
		public event MouseEventHandler GridMouseMove;
		private int m_currentId;
		private Project m_project;
		private SortableBindingList<VoiceActor.VoiceActor> m_bindingList;
		private ComboBox m_currentComboBox;

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			m_currentId = 0;

			m_dataGrid.UserAddedRow += HandleUserAddedRow;
			m_dataGrid.CellMouseDoubleClick += HandleDoubleClick;
			m_dataGrid.MouseMove += HandleMouseMove;
		}

		public int RowCount { get { return m_dataGrid.RowCount; } }

		public VoiceActor.VoiceActor SelectedVoiceActorEntity
		{
			get { return m_dataGrid.SelectedRows[0].DataBoundItem as VoiceActor.VoiceActor; }
		}

		public DataGridViewSelectedRowCollection SelectedRows
		{
			get { return m_dataGrid.SelectedRows; }
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

		public DataGridView.HitTestInfo HitTest(int x, int y)
		{
			return m_dataGrid.HitTest(x, y);
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

		private void m_dataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			DataGridViewCell cell = m_dataGrid.CurrentCell;

			if (cell.OwningColumn == m_dataGrid.Columns["ActorGender"] || cell.OwningColumn == m_dataGrid.Columns["ActorAge"])
			{
				m_currentComboBox = e.Control as ComboBox;
				m_currentComboBox.SelectedIndexChanged += box_SelectedIndexChanged;
			}
			else
			{
				m_currentComboBox = null;
			}
		}

		private void box_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox box = sender as ComboBox;

			//Cell value was not saved without this
			m_dataGrid.CurrentCell.Value = box.SelectedItem;

			m_dataGrid.MoveToNextField();
		}

		private void HandleAddingNew(object sender, AddingNewEventArgs e)
		{
			e.NewObject = new VoiceActor.VoiceActor { Id = m_currentId++ };
		}

		private void RemoveSelectedRows(bool confirmWithUser)
		{
			if (m_dataGrid.SelectedRows.Count == 0)
				return;

			bool deleteConfirmed = !confirmWithUser;

			if (confirmWithUser)
			{
				List<VoiceActor.VoiceActor> selectedVoiceActorsWithAssignments = new List<VoiceActor.VoiceActor>();
				foreach (DataGridViewRow row in m_dataGrid.SelectedRows)
				{
					VoiceActor.VoiceActor voiceActor = row.DataBoundItem as VoiceActor.VoiceActor;
					if (voiceActor == null)
						continue;
					if (m_project.CharacterGroupList.HasVoiceActorAssigned(voiceActor.Id))
					{
						selectedVoiceActorsWithAssignments.Add(voiceActor);
					}
				}
				if (selectedVoiceActorsWithAssignments.Any())
				{
					string assignedMsg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.Message", "One or more of the selected actors is assigned to a character group. Deleting the actor will remove the assignment as well. Are you sure you want to delete the selected actors?");
					string assignedTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteAssignedActorsDialog.Title", "Voice Actor(s) Assigned");
					if (MessageBox.Show(assignedMsg, assignedTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						foreach (var voiceActor in selectedVoiceActorsWithAssignments)
							m_project.CharacterGroupList.RemoveVoiceActor(voiceActor.Id);
						m_project.SaveCharacterGroupData();
						deleteConfirmed = true;
					}
					else
					{
						return;
					}
				}
				else
				{
					string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Message", "Are you sure you want to delete the selected rows?");
					string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");
					deleteConfirmed = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
				}
			}

			if (deleteConfirmed)
			{
				int indexOfFirstRowToRemove = m_dataGrid.SelectedRows[0].Index;
				for (int i = m_dataGrid.SelectedRows.Count - 1; i >= 0; i--)
				{
					if (m_dataGrid.SelectedRows[i].Index != m_dataGrid.RowCount - 1)
						m_dataGrid.Rows.Remove(m_dataGrid.SelectedRows[i]);
				}
				SaveVoiceActorInformation();

				DataGridViewRowsRemovedEventHandler handler = UserRemovedRows;
				if (handler != null)
					handler(m_dataGrid, new DataGridViewRowsRemovedEventArgs(indexOfFirstRowToRemove, m_dataGrid.RowCount));

				HandleTableUpdated();
			}
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == toolStripMenuItem1)
			{
				RemoveSelectedRows(true);
			}
		}

		private void HandleTableUpdated()
		{
			EventHandler handler = TableUpdated;
			if (handler != null)
				handler(m_dataGrid, EventArgs.Empty);
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

		private void HandleDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler handler = CellDoubleClicked;
			if (handler != null)
				handler(sender, e);
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			MouseEventHandler handler = GridMouseMove;
			if (handler != null)
				handler(sender, e);
		}

		private void m_dataGrid_CurrentCellChanged(object sender, System.EventArgs e)
		{
			SaveVoiceActorInformation();
		}

		private void m_dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (m_currentComboBox != null)
			{
				m_currentComboBox.SelectedIndexChanged -= box_SelectedIndexChanged;
			}

			HandleTableUpdated();
		}
	}
}
