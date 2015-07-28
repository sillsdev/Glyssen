using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using SIL.ObjectModel;

namespace Glyssen.Controls
{
	public partial class VoiceActorInformationGrid : UserControl
	{
		public event EventHandler Saved;
		public event DataGridViewCellEventHandler CellUpdated;
		public event DataGridViewRowEventHandler UserAddedRow;
		public event DataGridViewRowsRemovedEventHandler UserRemovedRows;
		public event DataGridViewCellMouseEventHandler CellDoubleClicked;
		public event MouseEventHandler GridMouseMove;
		public event EventHandler SelectionChanged;
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

		public bool ReadOnly
		{
			get { return m_dataGrid.ReadOnly; }
			set { m_dataGrid.ReadOnly = value; }
		}

		public void Initialize(Project project)
		{
			m_project = project;
			LoadVoiceActorInformation();
		}

		public void SaveVoiceActorInformation()
		{
			m_project.SaveVoiceActorInformationData();

			if (Saved != null)
			{
				Saved(m_dataGrid, EventArgs.Empty);
			}
		}

		public DataGridView.HitTestInfo HitTest(int x, int y)
		{
			return m_dataGrid.HitTest(x, y);
		}

		public DataGridViewEditMode EditMode
		{
			get { return m_dataGrid.EditMode; }
			set { m_dataGrid.EditMode = value; }
		}

		public bool EndEdit()
		{
			return m_dataGrid.EndEdit();
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

				m_currentComboBox.KeyPress -= box_KeyPress;
				m_currentComboBox.KeyPress += box_KeyPress;
			}
			else
			{
				m_currentComboBox = null;
			}
		}

		private void box_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsLetter(e.KeyChar))
			{
				m_dataGrid.MoveToNextField();
			}
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
					string assignedMsg;
					string assignedTitle;
					if (selectedVoiceActorsWithAssignments.Count > 1)
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
					string dlgMessage;
					string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.DeleteRowsDialog.Title", "Confirm");
					
					if (SelectedRows.Count > 1)
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
			}
		}

		public Color BackgroundColor
		{
			get { return m_dataGrid.BackgroundColor; }
			set { m_dataGrid.BackgroundColor = value; }
		}

		private void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (e.ClickedItem == m_contextMenu_itemDeleteActors)
			{
				RemoveSelectedRows(true);
			}
		}

		private void HandleCellUpdated(DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler handler = CellUpdated;
			if (handler != null)
				handler(m_dataGrid, e);
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
			//Open combobox (or no effect on regular text box input)
			SendKeys.Send("{F4}");
		}

		private void m_dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			HandleCellUpdated(e);
			SaveVoiceActorInformation();
		}

		private void m_dataGrid_SelectionChanged(object sender, EventArgs e)
		{
			EventHandler handler = SelectionChanged;
			if (handler != null)
				handler(sender, e);
		}

		private void m_contextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (SelectedRows.Count > 1)
			{
				m_contextMenu_itemDeleteActors.Text = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ContextMenu.DeleteActors", "Delete Actors");
			}
			else
			{
				m_contextMenu_itemDeleteActors.Text = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.ContextMenu.DeleteActor", "Delete Actor");
			}
		}
	}
}
