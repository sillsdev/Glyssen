using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Controls;
using L10NSharp;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private readonly VoiceActorAssignmentViewModel m_actorAssignmentViewModel;
		private readonly DataGridViewCellStyle m_wordWrapCellStyle;
		private readonly Project m_project;
		private bool m_currentCellExpanded = false;

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			m_project = project;

			m_menuItemCreateNewGroup.Tag = "CreateNewGroup";

			m_actorAssignmentViewModel = new VoiceActorAssignmentViewModel(project);
			m_actorAssignmentViewModel.Saved += HandleModelSaved;

			m_wordWrapCellStyle = new DataGridViewCellStyle();
			m_wordWrapCellStyle.WrapMode = DataGridViewTriState.True;

			VoiceActorCol.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(null);
			VoiceActorCol.ValueMember = "ID";
			VoiceActorCol.DisplayMember = "Name";
			VoiceActorCol.GetSpecialDropDownImageToDraw += VoiceActorCol_GetSpecialDropDownImageToDraw;

			// Sadly, we have to do this here because setting it in the Designer doesn't work since BetterGrid overrides
			// the default value in its constructor.
			m_characterGroupGrid.MultiSelect = true;
			m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns[EstimatedHoursCol.Name], ListSortDirection.Descending);

			m_characterGroupGrid.CellValueChanged += (sender, args) => { Save(); };
		}

		private Image VoiceActorCol_GetSpecialDropDownImageToDraw(DataGridViewMultiColumnComboBoxColumn sender, int rowIndex)
		{
			return m_characterGroupGrid.Rows[rowIndex].Cells[sender.Index].ReadOnly ? Properties.Resources.bluelock : null;
		}

		public CharacterGroup FirstSelectedCharacterGroup
		{
			get
			{
				if (m_characterGroupGrid.SelectedRows.Count == 0)
					return null;
				return m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			}
		}

		private void UnAssignActorsFromSelectedGroups()
		{
			foreach (var group in SelectedAssignedNonCameoGroups)
				m_actorAssignmentViewModel.UnAssignActorFromGroup(group);

			m_characterGroupGrid.Refresh();
		}

		private void HandleEditVoiceActorsClick(object sender, EventArgs e)
		{
			using (var actorDlg = new VoiceActorInformationDlg(m_project, false))
				actorDlg.ShowDialog();
			m_characterGroupGrid.Refresh();
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void HandleSplitSelectedGroupClick(object sender, EventArgs e)
		{
			using (var splitGroupDlg = new SplitCharacterGroupDlg(FirstSelectedCharacterGroup, m_actorAssignmentViewModel))
				if (splitGroupDlg.ShowDialog(this) == DialogResult.OK)
				{
					Save();

					// Refresh is not adding the new row for whatever reason
					m_characterGroupGrid.DataSource = new BindingList<CharacterGroup>();
					m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
				}
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignments",
					"Remove Voice Actor Assignments")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment",
					"Remove Voice Actor Assignment");

			m_unAssignActorFromGroupToolStripMenuItem.Enabled = SelectedAssignedNonCameoGroups.Any();
			m_splitGroupToolStripMenuItem.Enabled = !multipleGroupsSelected && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
		}

		private IEnumerable<CharacterGroup> SelectedAssignedNonCameoGroups
		{
			get 
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					var group = m_characterGroupGrid.SelectedRows[i].DataBoundItem as CharacterGroup;
					if (group.IsVoiceActorAssigned && !m_project.IsCharacterGroupAssignedToCameoActor(group))
						yield return group;
				}
			}
		}

		private void m_contextMenuCharacters_Opening(object sender, CancelEventArgs e)
		{
			ListBoxEditingControl ctrl = m_characterGroupGrid.EditingControl as ListBoxEditingControl;
			if (ctrl == null)
			{
				e.Cancel = true;
				return;
			}

			// Can't use m_contextMenuCharacters or m_tmiCreateNewGroup here because for some reason the ones displaying are copies of those
			ContextMenuStrip cms = sender as ContextMenuStrip;
			if (cms == null)
				return;
			ToolStripMenuItem item = cms.Items.Cast<ToolStripMenuItem>().FirstOrDefault(i => i.Tag.ToString() == "CreateNewGroup");
			if (item == null)
				return;

			item.Text = ctrl.SelectedItems.Count > 1
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.CreateNewGroupWithCharacters",
					"Create a New Group with the Selected Characters")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.CreateNewGroupWithCharacter",
					"Create a New Group with the Selected Character");
		}

		private void m_menuItemCreateNewGroup_Click(object sender, EventArgs e)
		{
			ListBoxEditingControl ctrl = m_characterGroupGrid.EditingControl as ListBoxEditingControl;
			if (ctrl == null)
				return;
			IEnumerable<string> localizedCharacterIds = ctrl.SelectedItems.Cast<string>();
			var characterIds = localizedCharacterIds.Select(lc => CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[lc]);
			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToList(), m_actorAssignmentViewModel.AddNewGroup(), false))
			{
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[m_characterGroupGrid.RowCount-1].Cells[CharacterIdsCol.Name];
				ExpandCurrentCharacterGroupRow();
			}
		}

		private void HandleUpdateGroupsClick(object sender, EventArgs e)
		{
			using (var updateDlg = new UpdateCharacterGroupsDlg())
			{
				if (updateDlg.ShowDialog() == DialogResult.OK &&
					(updateDlg.SelectedOption == UpdateCharacterGroupsDlg.SelectionType.AutoGenAndMaintain ||
					updateDlg.SelectedOption == UpdateCharacterGroupsDlg.SelectionType.AutoGen))
				{
					m_characterGroupGrid.DataSource = new BindingList<CharacterGroup>();
					m_actorAssignmentViewModel.RegenerateGroups(updateDlg.SelectedOption == UpdateCharacterGroupsDlg.SelectionType.AutoGenAndMaintain);
					m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
					Save();
					RefreshCharacterGroupSort();
				}
			}
		}

		private void Save()
		{
			m_actorAssignmentViewModel.SaveAssignments();
			m_characterGroupGrid.IsDirty = false;
		}

		private void VoiceActorAssignmentDlg_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EndEdit();
		}

		#region Character Group Grid
		private void ExpandCurrentCharacterGroupRow()
		{
			var currentRow = m_characterGroupGrid.CurrentCell.OwningRow;

			m_characterGroupGrid.CurrentCell = currentRow.Cells[CharacterIdsCol.Name];

			var data = m_characterGroupGrid.CurrentCell.Value as ISet<string>;
			int estimatedRowHeight = data.Count * 21;
			int maxRowHeight = 200;

			//Without the +1, an extra row is drawn, and the list starts scrolled down one item
			currentRow.Height = Math.Max(21, Math.Min(estimatedRowHeight, maxRowHeight)) + 1;
			currentRow.DefaultCellStyle = m_wordWrapCellStyle;

			//Scroll table if expanded row will be hidden
			int dRows = currentRow.Index - m_characterGroupGrid.FirstDisplayedScrollingRowIndex;
			if (dRows * 22 + maxRowHeight >= m_characterGroupGrid.Height - m_characterGroupGrid.ColumnHeadersHeight)
			{
				m_characterGroupGrid.FirstDisplayedScrollingRowIndex = currentRow.Index;
			}

			m_characterGroupGrid.MultiSelect = false;

			currentRow.Selected = true;

			m_characterGroupGrid.Columns[CharacterIdsCol.Name].ContextMenuStrip = m_contextMenuCharacters;

			m_characterGroupGrid.PerformLayout();

			m_currentCellExpanded = true;

			if (!m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.BeginEdit(false);
		}

		public void RefreshCharacterGroupSort()
		{
			m_characterGroupGrid.Sort(m_characterGroupGrid.SortedColumn, m_characterGroupGrid.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
		}

		private void HandleModelSaved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

				string dlgMessage = multipleGroupsSelected
					? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.MessagePlural",
						"Are you sure you want to remove the voice actor assignments from the selected character groups?")
					: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.MessageSingular",
						"Are you sure you want to remove the voice actor assignment from the selected character group?");
				string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ConfirmUnassignDialog.Title", "Confirm");

				if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
					UnAssignActorsFromSelectedGroups();
			}
		}

		private void m_characterGroupGrid_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (m_characterGroupGrid.Columns[e.ColumnIndex].Name == CharacterIdsCol.Name)
			{
				m_characterGroupGrid.Rows[e.RowIndex].Height = 22;
				m_characterGroupGrid.Rows[e.RowIndex].DefaultCellStyle = null;
				m_characterGroupGrid.MultiSelect = true;
				m_characterGroupGrid.Rows[e.RowIndex].Selected = true;
				m_characterGroupGrid.Columns[CharacterIdsCol.Name].ContextMenuStrip = null;
				m_currentCellExpanded = false;
			}
		}

		private void m_characterGroupGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex < 0)
				return;
			if (e.Button == MouseButtons.Left && e.ColumnIndex >= 0 && m_characterGroupGrid.Columns[e.ColumnIndex].Name == CharacterIdsCol.Name)
			{
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid[e.ColumnIndex, e.RowIndex];

				ExpandCurrentCharacterGroupRow();
			}
			else if (e.Button == MouseButtons.Right && !m_characterGroupGrid.SelectedRows.Contains(m_characterGroupGrid.Rows[e.RowIndex]))
			{
				var column = e.ColumnIndex;
				if (column == m_characterGroupGrid.Columns[VoiceActorCol.Name].Index)
					column = m_characterGroupGrid.Columns[AttributesCol.Name].Index;
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid[column, e.RowIndex];
			}
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell && e.Data.GetDataPresent(typeof(List<string>)) &&
			    m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
			{
				//Follow the status of the drag-n-drop to remove new row on completion
				if (m_characterGroupGrid.EditingControl != null)
				{
					m_characterGroupGrid.EditingControl.QueryContinueDrag -= m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
					if (!m_characterGroupGrid.Rows[hitInfo.RowIndex].IsNewRow)
						m_characterGroupGrid.EditingControl.QueryContinueDrag += m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
				}

				m_characterGroupGrid.AllowUserToAddRows = true;

				e.Effect = DragDropEffects.Move;
				return;
			}
			e.Effect = DragDropEffects.None;
		}

		private void m_characterGroupGrid_DragEnd_DisallowUserToAddRows(object sender, QueryContinueDragEventArgs e)
		{
			if (e.Action == DragAction.Cancel || e.Action == DragAction.Drop)
				m_characterGroupGrid.AllowUserToAddRows = false;
		}

		private void HandleCharacterIdDrop(List<string> localizedCharacterIds, int rowIndex)
		{
			if (m_characterGroupGrid.Rows[rowIndex].IsNewRow)
				m_actorAssignmentViewModel.AddNewGroup();

			IList<string> characterIds = new List<string>(localizedCharacterIds.Count);
			foreach (var localizedCharacterId in localizedCharacterIds)
				characterIds.Add(CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId]);
			DataGridViewRow dropRow = m_characterGroupGrid.Rows[rowIndex];
			CharacterGroup dropGroup = dropRow.DataBoundItem as CharacterGroup;

			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds, dropGroup))
			{
				m_characterGroupGrid.CurrentCell = dropRow.Cells[CharacterIdsCol.Name];
				ExpandCurrentCharacterGroupRow();
			}

			m_characterGroupGrid.AllowUserToAddRows = false;
		}

		private void m_characterGroupGrid_DragDrop(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
			{
				if (e.Data.GetDataPresent(typeof(List<string>)) && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
				{
					List<string> characterIds = e.Data.GetData(typeof(List<string>)) as List<string>;
					if (characterIds != null)
						HandleCharacterIdDrop(characterIds, hitInfo.RowIndex);
					return;
				}
				if (e.Data.GetDataPresent(typeof(CharacterGroup)))
				{
					var sourceGroup = e.Data.GetData(typeof(CharacterGroup)) as CharacterGroup;
					var destinationGroup = m_characterGroupGrid.Rows[hitInfo.RowIndex].DataBoundItem as CharacterGroup;

					m_actorAssignmentViewModel.MoveActorFromGroupToGroup(sourceGroup, destinationGroup);

					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;

					m_characterGroupGrid.Refresh();
				}
			}
		}

		private void m_characterGroupGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var hitInfo = m_characterGroupGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == CharacterIdsCol.Name)
				{
					//Although the DataGridViewListBox usually handles the drag and drop, it only does so when the cell has focus.
					//In this case, the user starts dragging the first character ID even before selecting the row
					var group = m_characterGroupGrid.Rows[hitInfo.RowIndex].DataBoundItem as CharacterGroup;
					if (group == null)
						return;
					string characterId = group.CharacterIds.ToList().FirstOrDefault();
					if (group.CharacterIds.Count == 1 && characterId != null)
					{
						//Without refreshing, the rows selected displays weirdly
						Refresh();
						DoDragDrop(new List<string> { characterId }, DragDropEffects.Move);
					}
				}
			}
		}

		private void m_characterGroupGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl))
			{
				if (m_characterGroupGrid.CurrentRow != null)
					SetVoiceActorCellDataSource();
				SendKeys.Send("{F4}");
			}
		}

		private void m_characterGroupGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (m_characterGroupGrid.CurrentCell.IsInEditMode && m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) && m_characterGroupGrid.CurrentRow != null)
			{
				var priorValue = m_characterGroupGrid.CurrentCell.Value;
				int voiceActorId = (int)((DataRowView)((DataGridViewMultiColumnComboBoxEditingControl)m_characterGroupGrid.EditingControl).SelectedItem)["ID"];

				if (priorValue.Equals(voiceActorId))
					return;

				if (m_actorAssignmentViewModel.IsActorAssigned(voiceActorId))
				{
					string text = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorAlreadyAssigned.Text", "The Voice Actor is already assigned to a Character Group. If you make this assignment, the other assignment will be removed.");
					string caption = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorAlreadyAssigned.Caption", "Voice Actor Already Assigned");
					if (MessageBox.Show(text, caption, MessageBoxButtons.OKCancel) != DialogResult.OK)
					{
						e.Cancel = true;
						return;
					}
					m_actorAssignmentViewModel.UnAssignActorFromGroup(voiceActorId);
				}

				m_characterGroupGrid.Refresh();
			}
		}

		private void m_characterGroupGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			var group = m_characterGroupGrid.Rows[e.RowIndex].DataBoundItem as CharacterGroup;
			var isCameo = group != null && m_project.IsCharacterGroupAssignedToCameoActor(group);
			// Need to check before setting to avoid making it impossible to open the drop-down list.
			if (m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly != isCameo)
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly = isCameo;
		}

		private void m_characterGroupGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			ErrorReport.ReportFatalException(e.Exception);
		}

		private void m_characterGroupGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_splitSelectedGroupButton.Enabled = m_characterGroupGrid.SelectedRows.Count == 1 && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
		}
		#endregion

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EndEdit();
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			if (m_characterGroupGrid == null || m_characterGroupGrid.CurrentCell == null)
				return;

			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) &&
			    m_characterGroupGrid.CurrentRow != null)
			{
				SetVoiceActorCellDataSource();
			}
			else if (m_currentCellExpanded && m_characterGroupGrid.CurrentCell.EditType == typeof(ListBoxEditingControl))
				ExpandCurrentCharacterGroupRow();
		}

		private void SetVoiceActorCellDataSource()
		{
			Debug.Assert(m_characterGroupGrid.CurrentRow != null);
			var group = (CharacterGroup)m_characterGroupGrid.CurrentRow.DataBoundItem;
			var cell = (DataGridViewComboBoxCell)m_characterGroupGrid.CurrentCell;
			cell.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(group);
		}
	}
}
