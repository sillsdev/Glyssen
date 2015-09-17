using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_tmiCreateNewGroup.Tag = "CreateNewGroup";

			m_actorAssignmentViewModel = new VoiceActorAssignmentViewModel(project);
			m_actorAssignmentViewModel.Saved += m_actorAssignmentViewModel_Saved;

			m_wordWrapCellStyle = new DataGridViewCellStyle();
			m_wordWrapCellStyle.WrapMode = DataGridViewTriState.True;

			VoiceActorCol.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(null);
			VoiceActorCol.ValueMember = "ID";
			VoiceActorCol.DisplayMember = "Name";

			m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns["EstimatedHoursCol"], ListSortDirection.Descending);
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
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			string dlgMessage = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.MessagePlural",
					"Are you sure you want to un-assign the actors from the selected groups?")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.MessageSingular",
					"Are you sure you want to un-assign the actor from the selected group?");
			string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.Title", "Confirm");
			if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					var group = m_characterGroupGrid.SelectedRows[i].DataBoundItem as CharacterGroup;
					m_actorAssignmentViewModel.UnAssignActorFromGroup(group);
				}

				m_characterGroupGrid.Refresh();
			}			
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Close();
		}

		private void m_btnEditVoiceActors_Click(object sender, EventArgs e)
		{
			using (var actorDlg = new VoiceActorInformationDlg(m_project, false))
				actorDlg.ShowDialog();
			m_characterGroupGrid.Refresh();
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void m_splitGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var splitGroupDlg = new SplitCharacterGroupDlg(FirstSelectedCharacterGroup, m_actorAssignmentViewModel))
				if (splitGroupDlg.ShowDialog(this) == DialogResult.OK)
				{
					m_saveStatus.OnSaved();

					// Refresh is not adding the new row for whatever reason
					m_characterGroupGrid.DataSource = new BindingList<CharacterGroup>();
					m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
				}
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignMultipleGroups",
					"Un-Assign Actors from Selected Groups")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignSingleGroup",
					"Un-Assign Actor from Selected Group");

			m_splitGroupToolStripMenuItem.Enabled = !multipleGroupsSelected;
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

		private void m_tmiCreateNewGroup_Click(object sender, EventArgs e)
		{
			ListBoxEditingControl ctrl = m_characterGroupGrid.EditingControl as ListBoxEditingControl;
			if (ctrl == null)
				return;
			IEnumerable<string> localizedCharacterIds = ctrl.SelectedItems.Cast<string>();
			var characterIds = localizedCharacterIds.Select(lc => CharacterVerseData.SingletonLocalizedCharacterIdToCharacterIdDictionary[lc]);
			if (m_actorAssignmentViewModel.MoveCharactersToGroup(characterIds.ToList(), m_actorAssignmentViewModel.AddNewGroup(), false))
			{
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[m_characterGroupGrid.RowCount-1].Cells["CharacterIdsCol"];
				ExpandCurrentCharacterGroupRow();
			}
		}

		private void m_btnUpdateGroup_Click(object sender, EventArgs e)
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
					m_actorAssignmentViewModel.SaveAssignments();
					RefreshCharacterGroupSort();
				}
			}
		}

		#region Character Group Grid
		private void ExpandCurrentCharacterGroupRow()
		{
			var currentRow = m_characterGroupGrid.CurrentCell.OwningRow;

			m_characterGroupGrid.CurrentCell = currentRow.Cells["CharacterIdsCol"];

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

			m_characterGroupGrid.PerformLayout();
		}

		public void RefreshCharacterGroupSort()
		{
			m_characterGroupGrid.Sort(m_characterGroupGrid.SortedColumn, m_characterGroupGrid.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
		}

		private void m_actorAssignmentViewModel_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				UnAssignActorsFromSelectedGroups();
			}
		}

		private void m_characterGroupGrid_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (m_characterGroupGrid.Columns[e.ColumnIndex].Name == "CharacterIdsCol")
			{
				m_characterGroupGrid.Rows[e.RowIndex].Height = 22;
				m_characterGroupGrid.Rows[e.RowIndex].DefaultCellStyle = null;
				m_characterGroupGrid.MultiSelect = true;
				m_characterGroupGrid.Rows[e.RowIndex].Selected = true;
			}
		}

		private void m_characterGroupGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && m_characterGroupGrid.Columns[e.ColumnIndex].Name == "CharacterIdsCol")
			{
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid[e.ColumnIndex, e.RowIndex];

				ExpandCurrentCharacterGroupRow();
			}
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell && e.Data.GetDataPresent(typeof(List<string>)) &&
				m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == "CharacterIdsCol")
			{
				//Follow the status of the drag-n-drop to remove new row on completion
				m_characterGroupGrid.EditingControl.QueryContinueDrag -= m_characterGroupGrid_DragEnd_DisallowUserToAddRows;
				if (!m_characterGroupGrid.Rows[hitInfo.RowIndex].IsNewRow)
					m_characterGroupGrid.EditingControl.QueryContinueDrag += m_characterGroupGrid_DragEnd_DisallowUserToAddRows;

				m_characterGroupGrid.AllowUserToAddRows = true;

				e.Effect = DragDropEffects.Move;
				return;
			}
			if (!(e.Data.GetDataPresent(typeof(VoiceActor.VoiceActor)) || e.Data.GetDataPresent(typeof(CharacterGroup))))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			e.Effect = DragDropEffects.Copy;
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
				m_characterGroupGrid.CurrentCell = dropRow.Cells["CharacterIdsCol"];
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
				if (e.Data.GetDataPresent(typeof(List<string>)) && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == "CharacterIdsCol")
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
				if (hitInfo.Type == DataGridViewHitTestType.Cell && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].Name == "CharacterIdsCol")
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
				SendKeys.Send("{F4}");
		}

		private void m_characterGroupGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) && m_characterGroupGrid.CurrentRow != null)
			{
				var group = (CharacterGroup)m_characterGroupGrid.CurrentRow.DataBoundItem;
				var cell = (DataGridViewComboBoxCell)m_characterGroupGrid.CurrentCell;
				cell.DataSource = m_actorAssignmentViewModel.GetMultiColumnActorDataTable(group);
			}
		}

		private void m_characterGroupGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (m_characterGroupGrid.CurrentCell.EditType == typeof(DataGridViewMultiColumnComboBoxEditingControl) && m_characterGroupGrid.CurrentRow != null)
			{
				int voiceActorId = (int)((DataRowView)((DataGridViewMultiColumnComboBoxEditingControl)m_characterGroupGrid.EditingControl).SelectedItem)["ID"];
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

		private void m_characterGroupGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
//			if (m_characterGroupGrid.Columns[e.ColumnIndex] is DataGridViewMultiColumnComboBoxColumn)
//			{
//				if (m_characterGroupGrid.CurrentRow == null)
//					return;
//				var group = (CharacterGroup)m_characterGroupGrid.CurrentRow.DataBoundItem;
//				if (!group.IsVoiceActorAssigned)
////				if (e.Value.Equals(LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.Unassigned", "Unassigned")))
//				{
//					e. Value = "";
//					e.FormattingApplied = true;
//				}
//			}
		}

		private void m_characterGroupGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			ErrorReport.ReportFatalException(e.Exception);
		}
		#endregion

		private void VoiceActorAssignmentDlg_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EndEdit();
		}
	}
}
