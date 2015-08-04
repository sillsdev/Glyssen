using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.VoiceActor;
using L10NSharp;

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
		private VoiceActorSortableBindingList m_bindingList;
		private readonly Font m_italicsFont;

		public VoiceActorInformationGrid()
		{
			InitializeComponent();

			//Ensures that rows stay the height we set in the designer (specifically to match the character groups grid)
			m_dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			m_currentId = 0;

			m_dataGrid.UserAddedRow += HandleUserAddedRow;
			m_dataGrid.CellMouseDoubleClick += HandleDoubleClick;
			m_dataGrid.MouseMove += HandleMouseMove;
			m_dataGrid.CellFormatting += HandleCellFormatting;

			Font originalGridFont = m_dataGrid.Font;
			m_italicsFont = new Font(originalGridFont.FontFamily, originalGridFont.Size, originalGridFont.Style | FontStyle.Italic);
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

		public override ContextMenuStrip ContextMenuStrip
		{
			get { return m_dataGrid.ContextMenuStrip; }
			set { m_dataGrid.ContextMenuStrip = value; }
		}

		public IEnumerable<CharacterGroup> CharacterGroupsWithAssignedActors { get; set; }

		public void Initialize(Project project, bool sort = true)
		{
			m_project = project;
			LoadVoiceActorInformation(sort);
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

		private void LoadVoiceActorInformation(bool sort = true)
		{
			var actors = m_project.VoiceActorList.Actors;
			if (actors.Any())
				m_currentId = actors.Max(a => a.Id) + 1;
			m_bindingList = new VoiceActorSortableBindingList(actors);
			m_bindingList.CharacterGroups = CharacterGroupsWithAssignedActors;
			m_dataGrid.DataSource = m_bindingList;
			m_bindingList.AddingNew += HandleAddingNew;

			if (sort)
				m_dataGrid.Sort(m_dataGrid.Columns["ActorName"], ListSortDirection.Ascending);
		}

		private void m_dataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			var currentComboBox = e.Control as ComboBox;
			if (currentComboBox != null)
			{
				currentComboBox.KeyPress -= Combobox_KeyPress;
				currentComboBox.KeyPress += Combobox_KeyPress;

				if (currentComboBox.SelectedIndex == -1)
					currentComboBox.SelectedIndex = 0;
			}
		}

		private void Combobox_KeyPress(object sender, KeyPressEventArgs e)
		{
			Debug.Assert(sender is ComboBox);
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

		public void RefreshSort()
		{
			m_dataGrid.Sort(m_dataGrid.SortedColumn, m_dataGrid.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
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

		private void HandleCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			VoiceActor.VoiceActor actor = m_dataGrid.Rows[e.RowIndex].DataBoundItem as VoiceActor.VoiceActor;
			if (actor == null || CharacterGroupsWithAssignedActors == null || !CharacterGroupsWithAssignedActors.Any())
			{
				e.FormattingApplied = false;
				return;
			}
			if (CharacterGroupsWithAssignedActors.Any(cg => cg.VoiceActorAssigned == actor))
			{
				e.CellStyle.Font = m_italicsFont;
				e.CellStyle.ForeColor = Color.DimGray;
			}
			else
			{
				e.FormattingApplied = false;
			}
		}

		private void m_dataGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (m_dataGrid.CurrentCell == null)
				return;
			if (m_dataGrid.CurrentCell.OwningColumn.DataPropertyName == "Name")
			{
				m_dataGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			}
			else
			{
				m_dataGrid.EditMode = DataGridViewEditMode.EditOnEnter;
				//Open combobox (or no effect on regular text box input)
				SendKeys.Send("{F4}");
			}
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
