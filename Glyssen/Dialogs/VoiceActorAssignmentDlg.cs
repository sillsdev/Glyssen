using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Rules;
using L10NSharp;
using L10NSharp.UI;
using SIL.Progress;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private readonly VoiceActorAssignmentViewModel m_actorAssignmentViewModel;
		private readonly DataGridViewCellStyle m_wordWrapCellStyle;
		private readonly Project m_project;
		private readonly Dictionary<string, int> m_keyStrokesByCharacterId;
		private bool m_currentCellExpanded = false;
		private DataGridViewColumn m_sortedColumn;
		private bool m_sortedAscending;
		private int m_indexOfRowNotToInvalidate = -1;
		private bool m_undoingOrRedoing;

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			m_project = project;
			m_keyStrokesByCharacterId = m_project.GetKeyStrokesByCharacterId();

			m_menuItemCreateNewGroup.Tag = "CreateNewGroup";

			if (!m_project.CharacterGroupList.CharacterGroups.Any())
				GenerateGroupsWithProgress(false);

			m_actorAssignmentViewModel = new VoiceActorAssignmentViewModel(project, m_keyStrokesByCharacterId);
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
			SortByColumn(EstimatedHoursCol, false);
			SetRowCount();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void GenerateGroupsWithProgress(bool attemptToPreserveActorAssignments)
		{
			using (var progressDialog = new GenerateGroupsProgressDialog(m_project, OnGenerateGroupsWorkerDoWork))
			{
				progressDialog.ProgressState.Arguments = attemptToPreserveActorAssignments;
				progressDialog.ShowDialog();
			}
		}

		private void OnGenerateGroupsWorkerDoWork(object s, DoWorkEventArgs e)
		{
			var attemptToPreserveActorAssignments = (bool)((ProgressState) e.Argument).Arguments;
			new CharacterGroupGenerator(m_project, m_keyStrokesByCharacterId, attemptToPreserveActorAssignments).UpdateProjectCharacterGroups();
		}

		/// <summary>
		/// Sets the row count if necessary; otherwise (optionally) refreshes the grid.
		/// </summary>
		/// <param name="refreshOnlyIfNeeded">Indicates whether a full refresh is desired in cases where the row count has not changed</param>
		/// <returns>A value indicating whether the grid was refreshed (always true if the row count changed)</returns>
		private bool SetRowCount(bool refreshOnlyIfNeeded = false)
		{
			bool changingRowCount = m_actorAssignmentViewModel.CharacterGroups.Count != m_characterGroupGrid.RowCount;
			if (changingRowCount)
			{
				var columnSizeModesToRestore = m_characterGroupGrid.Columns.OfType<DataGridViewColumn>().Where(c => c.AutoSizeMode != DataGridViewAutoSizeColumnMode.None).ToDictionary(c => c, c => c.AutoSizeMode);
				var autoSizeRowsModeToRestore = m_characterGroupGrid.AutoSizeRowsMode;
				foreach (var column in columnSizeModesToRestore.Keys)
					column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				m_characterGroupGrid.RowCount = m_actorAssignmentViewModel.CharacterGroups.Count;
				// Need to clear the selection here again because some of the property setters on
				// DataGridView have the side-effect of creating a selection. We want to avoid having
				// HandleDataGridViewBlocksCellValueNeeded get called with an index that is out of
				// range for the new book.
				m_characterGroupGrid.ClearSelection();
				m_characterGroupGrid.RowCount = m_actorAssignmentViewModel.CharacterGroups.Count;
				foreach (var kvp in columnSizeModesToRestore)
					kvp.Key.AutoSizeMode = kvp.Value;
				m_characterGroupGrid.AutoSizeRowsMode = autoSizeRowsModeToRestore;
			}
			else if (!refreshOnlyIfNeeded)
				m_characterGroupGrid.Refresh();
			else
				return false;
			return true;
		}

		private void HandleStringsLocalized()
		{
			m_undoButton.Tag = new Tuple<string, Keys>(m_undoButton.ToolTipText, Keys.Z);
			m_redoButton.Tag = new Tuple<string, Keys>(m_redoButton.ToolTipText, Keys.Y);

			if (!m_undoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_undoButton, null);
			if (!m_redoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_redoButton, null);
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
				return m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.SelectedRows[0].Index];
			}
		}

		private void UnAssignActorsFromSelectedGroups()
		{
			m_actorAssignmentViewModel.UnAssignActorFromGroups(SelectedGroupsThatCanBeUnassigned);
		}

		private void HandleEditVoiceActorsClick(object sender, EventArgs e)
		{
			var actorInfoViewModel = new VoiceActorInformationViewModel(m_project);

			using (var actorDlg = new VoiceActorInformationDlg(actorInfoViewModel, false))
			{
				actorDlg.ShowDialog();
				if (actorInfoViewModel.Changes.Any())
				{
					m_actorAssignmentViewModel.NoteActorChanges(actorInfoViewModel.Changes);
					if (actorInfoViewModel.Changes.Any(c => !c.JustChangedName) &&
						MessageBox.Show(this, String.Format(LocalizationManager.GetString(
							"DialogBoxes.VoiceActorAssignmentDlg.UpdateCharacterGroupsPrompt",
							"{0} can optimize the number and composition of character groups to match the voice actors you have entered. " +
							"Would you like {0} to update the groups now?"), Program.kProduct), Text, MessageBoxButtons.YesNo) ==
						DialogResult.Yes)
					{
						HandleUpdateGroupsClick(m_updateGroupsButton, e);
						//m_characterGroupGrid.InvalidateColumn(VoiceActorCol.Index);
					}

					VoiceActorCol.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(null);

					if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) &&
						m_characterGroupGrid.CurrentRow != null)
					{
						SetVoiceActorCellDataSource();
					}
				}
			}
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void HandleSplitSelectedGroupClick(object sender, EventArgs e)
		{
			using (var splitGroupDlg = new SplitCharacterGroupDlg(FirstSelectedCharacterGroup, m_actorAssignmentViewModel))
				splitGroupDlg.ShowDialog(this);
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignments",
					"Remove Voice Actor Assignments")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment",
					"Remove Voice Actor Assignment");

			m_unAssignActorFromGroupToolStripMenuItem.Enabled = SelectedGroupsThatCanBeUnassigned.Any();
			m_splitGroupToolStripMenuItem.Enabled = !multipleGroupsSelected && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
		}

		private IEnumerable<CharacterGroup> SelectedGroupsThatCanBeUnassigned
		{
			get
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					var group = m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.SelectedRows[i].Index];
					if (m_actorAssignmentViewModel.CanRemoveAssignment(group))
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

			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToList(), null))
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
					bool attemptToPreserveActorAssignments = updateDlg.SelectedOption == UpdateCharacterGroupsDlg.SelectionType.AutoGenAndMaintain;
					m_actorAssignmentViewModel.RegenerateGroups(() => { GenerateGroupsWithProgress(attemptToPreserveActorAssignments); });
					SortByColumn(m_sortedColumn, m_sortedAscending);
				}
			}
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

		private void HandleModelSaved(VoiceActorAssignmentViewModel sender, IEnumerable<CharacterGroup> changedGroups)
		{
			int columnIndex = m_characterGroupGrid.CurrentCellAddress.X;

			m_saveStatus.OnSaved();
			if (!SetRowCount(changedGroups != null))
				InvalidateRowsForGroups(changedGroups);
			m_indexOfRowNotToInvalidate = -1;
			m_characterGroupGrid.IsDirty = false;
			m_undoButton.Enabled = m_actorAssignmentViewModel.UndoActions.Any();
			if (!m_undoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_undoButton, null);
			m_redoButton.Enabled = m_actorAssignmentViewModel.RedoActions.Any();
			if (!m_redoButton.Enabled)
				SetUndoOrRedoButtonToolTip(m_redoButton, null);

			var groupToSelect = changedGroups == null ? null : changedGroups.FirstOrDefault();
			if (groupToSelect != null)
			{
				var rowIndex = m_actorAssignmentViewModel.CharacterGroups.IndexOf(groupToSelect);
				if (rowIndex >= 0)
				{
					m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[rowIndex].Cells[columnIndex];
					if (columnIndex == CharacterIdsCol.Index)
						ExpandCurrentCharacterGroupRow();
				}
			}
		}

		private void InvalidateRowsForGroups(IEnumerable<CharacterGroup> changedGroups)
		{
			foreach (var group in changedGroups)
			{
				var i = m_actorAssignmentViewModel.CharacterGroups.IndexOf(@group);
				if (i >= 0 && i != m_indexOfRowNotToInvalidate)
					m_characterGroupGrid.InvalidateRow(i);
			}
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

				if (MessageBox.Show(this, dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
				if (m_actorAssignmentViewModel.CanMoveCharactersToGroup((List<string>) e.Data.GetData(typeof (List<string>)), null))
				{
					//Follow the status of the drag-n-drop to remove new row on completion
					if (m_characterGroupGrid.EditingControl != null)
					{
						m_characterGroupGrid.EditingControl.QueryContinueDrag -= m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
						if (!m_characterGroupGrid.Rows[hitInfo.RowIndex].IsNewRow)
							m_characterGroupGrid.EditingControl.QueryContinueDrag += m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
					}

					m_characterGroupGrid.AllowUserToAddRows = true;
				}

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
			IList<string> characterIds = new List<string>(localizedCharacterIds.Count);
			foreach (var localizedCharacterId in localizedCharacterIds)
				characterIds.Add(CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[localizedCharacterId]);

			CharacterGroup dropGroup = (m_characterGroupGrid.Rows[rowIndex].IsNewRow) ? null :
				m_actorAssignmentViewModel.CharacterGroups[rowIndex];

			// Do this before creating the new group because it causes RowCount to go back down to reflect the number of non-empty rows.
			m_characterGroupGrid.AllowUserToAddRows = false;

			m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds, dropGroup, true);
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
					var group = m_actorAssignmentViewModel.CharacterGroups[hitInfo.RowIndex];
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
				if (!m_undoingOrRedoing)
					SendKeys.Send("{F4}");
			}
		}

		private void m_characterGroupGrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			if (e.RowIndex >= m_actorAssignmentViewModel.CharacterGroups.Count)
				return;
			var group = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex];
			var isCameo = group != null && group.AssignedToCameoActor;
			// Need to check before setting to avoid making it impossible to open the drop-down list.
			if (m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly != isCameo)
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ReadOnly = isCameo;
			const string strCameoTooltip = "This actor is assigned to perform a cameo role. You can " +
				"change the characters in this group, but you cannot change the actor assignment.";
			if (isCameo)
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ToolTipText =
					LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.CameoTooltip", strCameoTooltip);
			else
				m_characterGroupGrid.Rows[e.RowIndex].Cells[VoiceActorCol.Name].ToolTipText = "";

		}

		private void m_characterGroupGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Analytics.ReportException(e.Exception);
			ErrorReport.ReportFatalException(e.Exception);
			throw e.Exception;
		}

		private void m_characterGroupGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_splitSelectedGroupButton.Enabled = m_characterGroupGrid.SelectedRows.Count == 1 && FirstSelectedCharacterGroup.CharacterIds.Count > 1;
		}

		private void m_characterGroupGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.ColumnIndex == CharacterIdsCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].CharacterIds;
			else if (e.ColumnIndex == AttributesCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].AttributesDisplay;
			else if (e.ColumnIndex == CharStatusCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].StatusDisplay;
			else if (e.ColumnIndex == EstimatedHoursCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].EstimatedHours;
			else if (e.ColumnIndex == VoiceActorCol.Index)
				e.Value = m_actorAssignmentViewModel.CharacterGroups[e.RowIndex].VoiceActorId;
		}

		private void m_characterGroupGrid_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			if (e.ColumnIndex == VoiceActorCol.Index)
			{
				m_indexOfRowNotToInvalidate = e.RowIndex;
				m_actorAssignmentViewModel.AssignActorToGroup((int) e.Value, m_actorAssignmentViewModel.CharacterGroups[e.RowIndex]);
			}
		}

		private void HandleGridColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;
			var clickedColumn = m_characterGroupGrid.Columns[e.ColumnIndex];
			if (clickedColumn.SortMode == DataGridViewColumnSortMode.NotSortable)
				return;

			// We generally want to sort ascending unless it already was ascending, but for estimated
			// hours, we want descending to be the default.
			bool sortAscending = (clickedColumn == EstimatedHoursCol) ? (clickedColumn.HeaderCell.SortGlyphDirection == SortOrder.Descending) :
				(clickedColumn.HeaderCell.SortGlyphDirection != SortOrder.Ascending);
			if (sortAscending)
			{
				foreach (var column in m_characterGroupGrid.Columns.OfType<DataGridViewColumn>())
				{
					if (column != clickedColumn)
						column.HeaderCell.SortGlyphDirection = SortOrder.None;
				}
			}

			SortByColumn(clickedColumn, sortAscending);
		}

		private void SetVoiceActorCellDataSource()
		{
			Debug.Assert(m_characterGroupGrid.CurrentRow != null);
			var group = m_actorAssignmentViewModel.CharacterGroups[m_characterGroupGrid.CurrentCellAddress.Y];
			var cell = (DataGridViewComboBoxCell)m_characterGroupGrid.CurrentCell;
			cell.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(group);
		}

		private void SortByColumn(DataGridViewColumn column, bool sortAscending)
		{
			column.HeaderCell.SortGlyphDirection = sortAscending ? SortOrder.Ascending : SortOrder.Descending;

			if (column == AttributesCol)
				m_actorAssignmentViewModel.Sort(SortedBy.Attributes, sortAscending);
			else if (column == EstimatedHoursCol)
				m_actorAssignmentViewModel.Sort(SortedBy.EstimatedTime, sortAscending);
			else if (column == VoiceActorCol)
				m_actorAssignmentViewModel.Sort(SortedBy.Actor, sortAscending);
			else
				m_actorAssignmentViewModel.Sort(SortedBy.Name, sortAscending);

			m_sortedColumn = column;
			m_sortedAscending = sortAscending;
			m_characterGroupGrid.Refresh();
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
			if (m_characterGroupGrid == null || m_characterGroupGrid.CurrentCell == null || m_characterGroupGrid.RowCount != m_project.CharacterGroupList.CharacterGroups.Count)
				return;

			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) &&
				m_characterGroupGrid.CurrentRow != null)
			{
				SetVoiceActorCellDataSource();
			}
			else if (m_currentCellExpanded && m_characterGroupGrid.CurrentCell.EditType == typeof(ListBoxEditingControl))
				ExpandCurrentCharacterGroupRow();
		}

		private void HandleUndoButtonClick(object sender, EventArgs e)
		{
			m_undoingOrRedoing = true;
			try
			{
				if (!m_actorAssignmentViewModel.Undo())
					MessageBox.Show(this, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.UndoFailed", "Undo Failed!"));
			}
			finally
			{
				m_undoingOrRedoing = false;
			}
		}

		private void HandleRedoButtonClick(object sender, EventArgs e)
		{
			m_undoingOrRedoing = true;
			try
			{
				if (!m_actorAssignmentViewModel.Redo())
					MessageBox.Show(this, LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.RedoFailed", "Redo Failed!"));
			}
			finally
			{
				m_undoingOrRedoing = false;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Control)
			{
				foreach (var item in m_toolStrip.Items.OfType<ToolStripItem>().Where(i => i.Tag is Tuple<string, Keys>))
				{
					if (e.KeyCode == ((Tuple<string, Keys>)item.Tag).Item2)
					{
						item.PerformClick();
						return;
					}
				}
			}
			base.OnKeyDown(e);
		}

		private void SetUndoOrRedoButtonToolTip(object sender, EventArgs e)
		{
			var btn = (ToolStripButton)sender;
			var tag = (Tuple<string, Keys>)btn.Tag;
			string ctrlKeyTip = string.Format("(Ctrl-{0})", tag.Item2);
			var description = (btn == m_undoButton ? m_actorAssignmentViewModel.UndoActions : m_actorAssignmentViewModel.RedoActions).FirstOrDefault();
			if (description == null)
				description = string.Empty;
			else if (!description.EndsWith(" "))
				description += " ";
			btn.ToolTipText = String.Format(tag.Item1, description + ctrlKeyTip);
		}

		private void m_characterGroupGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			var dropDown = e.Control as DataGridViewComboBoxEditingControl;
			if (dropDown == null)
				return;
			dropDown.DropDownClosed += DropDownOnDropDownClosed;
		}

		/// <summary>
		/// When the drop-down closes, we want to force the change to be committed and get out of edit mode. This gives a better user experience,
		/// especially when reassigning an actor to a different group (otherwise, the actor looks like he's assigned to both groups).
		/// </summary>
		private void DropDownOnDropDownClosed(object sender, EventArgs eventArgs)
		{
			var dropDown = sender as DataGridViewComboBoxEditingControl;
			if (dropDown == null)
				return;

			// If there was no actual change, the view model correctly ignores it.
			m_characterGroupGrid.NotifyCurrentCellDirty(true);
			m_characterGroupGrid.EndEdit(DataGridViewDataErrorContexts.Commit);

			dropDown.DropDownClosed -= DropDownOnDropDownClosed;
		}
	}
}
