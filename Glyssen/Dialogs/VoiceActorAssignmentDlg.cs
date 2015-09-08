using System;
using System.Collections.Generic;
using System.ComponentModel;
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

			m_actorAssignmentViewModel = new VoiceActorAssignmentViewModel(project);
			m_actorAssignmentViewModel.Saved += m_actorAssignmentViewModel_Saved;

			m_wordWrapCellStyle = new DataGridViewCellStyle();
			m_wordWrapCellStyle.WrapMode = DataGridViewTriState.True;

			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			WindowState = FormWindowState.Maximized;

			AlignBtnAssignActorToSplitter();

			var characterGroups = m_actorAssignmentViewModel.CharacterGroups;

			m_characterGroupGrid.DataSource = characterGroups;
			m_characterGroupGrid.MultiSelect = true;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns["EstimatedHours"], ListSortDirection.Descending);

			m_voiceActorGrid.CharacterGroupsWithAssignedActors = characterGroups;
			m_voiceActorGrid.Initialize(project);
			m_voiceActorGrid.CellDoubleClicked += m_voiceActorGrid_CellDoubleClicked;
			m_voiceActorGrid.GridMouseMove += m_voiceActorGrid_MouseMove;
			m_voiceActorGrid.SelectionChanged += m_eitherGrid_SelectionChanged;
		}

		private void AssignSelectedActorToSelectedGroup()
		{
			CharacterGroup group = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			if (group == null)
				return;

			VoiceActor.VoiceActor actor = m_voiceActorGrid.SelectedVoiceActorEntity;
			if (m_actorAssignmentViewModel.IsActorAssigned(actor))
			{
				string text = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorAlreadyAssigned.Text", "The Voice Actor is already assigned to a Character Group. If you make this assignment, the other assignment will be removed.");
				string caption = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ActorAlreadyAssigned.Caption", "Voice Actor Already Assigned");
				if (MessageBox.Show(text, caption, MessageBoxButtons.OKCancel) != DialogResult.OK)
					return;
				m_actorAssignmentViewModel.UnAssignActorFromGroup(actor);
			}

			m_actorAssignmentViewModel.AssignActorToGroup(actor, group);

			m_characterGroupGrid.Refresh();
			m_voiceActorGrid.RefreshSort();
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
				m_voiceActorGrid.RefreshSort();
			}			
		}

		private void AlignBtnAssignActorToSplitter()
		{
			int xDist = splitContainer1.SplitterDistance;
			m_btnAssignActor.Location = new Point(xDist + 19, m_btnAssignActor.Location.Y);
		}

		private void m_btnAssignActor_Click(object sender, EventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_eitherGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_actorAssignmentViewModel.CanAssign = m_voiceActorGrid.SelectedRows.Count == 1 && m_characterGroupGrid.SelectedRows.Count == 1;

			m_btnAssignActor.Enabled = m_actorAssignmentViewModel.CanAssign;
		}

//		private void m_btnExport_Click(object sender, EventArgs e)
//		{
//			var characterGroups = m_characterGroupGrid.DataSource as SortableBindingList<CharacterGroup>;
// 
//			bool assignmentsComplete = characterGroups.All(t => t.IsVoiceActorAssigned);
//
//			string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Message", "Some of the character groups have no voice talent assigned. Are you sure you want to export an incomplete script?\n(Note: You can export the script again as many times as you want.)");
//			string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Title", "Export Incomplete Script?");
//			if (assignmentsComplete || MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
//			{
//				SaveAssignments();
//				new ProjectExport(m_project).Export(this);
//			}
//		}

		private void m_voiceActorGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var hitInfo = m_voiceActorGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell)
				{
					var sourceActor = m_voiceActorGrid.SelectedVoiceActorEntity;
					if (sourceActor != null && sourceActor.IsValid())
					{
						DoDragDrop(sourceActor, DragDropEffects.Copy);
					}
					else
					{
						DoDragDrop(0, DragDropEffects.None);
					}
				}
			}
		}

		private void VoiceActorAssignmentDlg_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F6)
			{
				if (m_characterGroupGrid.ContainsFocus)
					m_voiceActorGrid.Focus();
				else
					m_characterGroupGrid.Focus();
			}
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			AlignBtnAssignActorToSplitter();
		}

		private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
		{
			//A disabled control is un-focusable
			bool btnIsEnabled = m_btnAssignActor.Enabled;
			m_btnAssignActor.Enabled = true;

			//Focus on Assign button to remove awkward focus rectangle around splitter
			m_btnAssignActor.Focus();

			m_btnAssignActor.Enabled = btnIsEnabled;
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Close();
		}

		private void m_btnEditVoiceActors_Click(object sender, EventArgs e)
		{
			using (var actorDlg = new VoiceActorInformationDlg(m_project))
				actorDlg.ShowDialog();
			m_voiceActorGrid.RefreshSort();
			m_characterGroupGrid.Refresh();
		}

		private void m_helpIcon_MouseClick(object sender, MouseEventArgs e)
		{
			var pic = sender as PictureBox;
			m_toolTip.Show("In this window, you can assign voice actors to speak the parts of a group of Biblical characters.\n" +
							"To assign an actor to a group, use one of the assignment methods below:" +
							"\n1) Select a group and an actor and click \"Assign\"" +
							"\n2) Select a group and an actor and double-click the character group or voice actor's name" +
							"\n3) Drag an actor's name into the corresponding \"Actor Assigned\" column of a character group",
				pic, new Point(e.X + 10, e.Y + 5));
		}

		private void m_helpIcon_MouseLeave(object sender, EventArgs e)
		{
			m_toolTip.Hide(this);
		}

		private void m_assignActorToGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void m_splitGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Todo
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;
			bool multipleActorsSelected = m_voiceActorGrid.SelectedRows.Count > 1;

			m_assignActorToGroupToolStripMenuItem.Enabled = !multipleGroupsSelected && !multipleActorsSelected;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignMultipleGroups",
					"Un-Assign Actors from Selected Groups")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignSingleGroup",
					"Un-Assign Actor from Selected Group");

			m_splitGroupToolStripMenuItem.Enabled = !multipleGroupsSelected;
		}

		private void m_contextMenuVoiceActors_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;
			bool multipleActorsSelected = m_voiceActorGrid.SelectedRows.Count > 1;

			m_assignActorToGroupToolStripMenuItem2.Enabled = !multipleActorsSelected && !multipleGroupsSelected;
		}

		private void m_contextMenuCharacters_Opening(object sender, CancelEventArgs e)
		{
			ListBoxEditingControl ctrl = m_characterGroupGrid.EditingControl as ListBoxEditingControl;
			if (ctrl == null)
			{
				e.Cancel = true;
				return;
			}
			m_tmiCreateNewGroup.Text = ctrl.SelectedItems.Count > 1
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
				m_characterGroupGrid.CurrentCell = m_characterGroupGrid.Rows[m_characterGroupGrid.RowCount-1].Cells["CharacterIds"];
				ExpandCurrentCharacterGroupRow();
			}
		}

		private void m_btnUpdateGroup_Click(object sender, EventArgs e)
		{
			using (var updateDlg = new UpdateCharacterGroupsDlg())
			{
				if (updateDlg.ShowDialog() == DialogResult.OK &&
					updateDlg.SelectedOption == UpdateCharacterGroupsDlg.SelectionType.AutoGen)
				{
					m_characterGroupGrid.DataSource = new BindingList<CharacterGroup>();
					m_actorAssignmentViewModel.GenerateGroups();
					m_characterGroupGrid.DataSource = m_actorAssignmentViewModel.CharacterGroups;
					m_characterGroupGrid.Refresh();
				}
			}
		}

		#region Voice Actor Grid
		private void m_voiceActorGrid_CellDoubleClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;
			if (grid == null)
				return;
			if (grid.Columns[e.ColumnIndex].DataPropertyName == "Name" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
				AssignSelectedActorToSelectedGroup();
		}
		#endregion

		#region Character Group Grid
		private void ExpandCurrentCharacterGroupRow()
		{
			var currentRow = m_characterGroupGrid.CurrentCell.OwningRow;

			m_characterGroupGrid.CurrentCell = currentRow.Cells["CharacterIds"];

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

			m_characterGroupGrid.ReadOnly = false;

			if (!m_characterGroupGrid.IsCurrentCellInEditMode)
				m_characterGroupGrid.EditMode = DataGridViewEditMode.EditOnEnter;

			m_characterGroupGrid.MultiSelect = false;

			currentRow.Selected = true;

			m_characterGroupGrid.PerformLayout();
		}

		private void m_actorAssignmentViewModel_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_characterGroupGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;

			if (grid.Columns[e.ColumnIndex].DataPropertyName == "VoiceActorAssignedName" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
				AssignSelectedActorToSelectedGroup();
		}

		private void m_characterGroupGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_eitherGrid_SelectionChanged(sender, e);
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
			if (m_characterGroupGrid.Columns[e.ColumnIndex].DataPropertyName == "CharacterIds")
			{
				m_characterGroupGrid.Rows[e.RowIndex].Height = 22;
				m_characterGroupGrid.Rows[e.RowIndex].DefaultCellStyle = null;
				m_characterGroupGrid.ReadOnly = true;
				m_characterGroupGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
				m_characterGroupGrid.MultiSelect = true;
				m_characterGroupGrid.Rows[e.RowIndex].Selected = true;
			}
		}

		private void m_characterGroupGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && m_characterGroupGrid.Columns[e.ColumnIndex].DataPropertyName == "CharacterIds")
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
				m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "CharacterIds")
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
				m_characterGroupGrid.CurrentCell = dropRow.Cells["CharacterIds"];
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
				if (e.Data.GetDataPresent(typeof(List<string>)) && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "CharacterIds")
				{
					List<string> characterIds = e.Data.GetData(typeof(List<string>)) as List<string>;
					if (characterIds != null)
						HandleCharacterIdDrop(characterIds, hitInfo.RowIndex);
					return;
				}
				if (e.Data.GetDataPresent(typeof(VoiceActor.VoiceActor)))
				{
					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;
					AssignSelectedActorToSelectedGroup();
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
					m_voiceActorGrid.RefreshSort();
				}
			}
		}

		private void m_characterGroupGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var hitInfo = m_characterGroupGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell)
				{
					if (m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "VoiceActorAssignedName")
					{
						CharacterGroup sourceGroup = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
						if (sourceGroup != null && sourceGroup.IsVoiceActorAssigned)
						{
							DoDragDrop(sourceGroup, DragDropEffects.Copy);
						}
						else
						{
							DoDragDrop(0, DragDropEffects.None);
						}
						return;
					}
					if (m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "CharacterIds")
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
							DoDragDrop(new List<string>{characterId}, DragDropEffects.Move);
						}
					}
				}
			}
		}

		private void m_characterGroupGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			ErrorReport.ReportFatalException(e.Exception);
		}
		#endregion
	}
}
