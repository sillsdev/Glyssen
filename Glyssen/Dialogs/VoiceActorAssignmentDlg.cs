using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Properties;
using L10NSharp;
using SIL.IO;
using SIL.ObjectModel;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private readonly Project m_project;
		private bool m_canAssign;

		private enum DragSource
		{
			CharacterGroupGrid,
			VoiceActorGrid
		};
		private DragSource m_dragSource;  

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			WindowState = FormWindowState.Maximized;

			AlignBtnAssignActorToSplitter();

			m_project = project;
			m_canAssign = true;

			m_voiceActorGrid.Initialize(m_project);
			m_voiceActorGrid.ReadOnly = true;

			m_voiceActorGrid.Saved += m_voiceActorGrid_Saved;
			m_voiceActorGrid.CellUpdated += m_voiceActorGrid_CellUpdated;
			m_voiceActorGrid.CellDoubleClicked += m_voiceActorGrid_CellDoubleClicked;
			m_voiceActorGrid.GridMouseMove += m_voiceActorGrid_MouseMove;
			m_voiceActorGrid.UserRemovedRows += m_voiceActorGrid_UserRemovedRows;
			m_voiceActorGrid.SelectionChanged += m_eitherGrid_SelectionChanged;

			var characterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);
			if (characterGroups.Count == 0)
			{
				CharacterGroupTemplate charGroupTemplate;
				using (TempFile tempFile = new TempFile())
				{
					File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
					ICharacterGroupSource charGroupSource = new CharacterGroupTemplateExcelFile(tempFile.Path);
					charGroupTemplate = charGroupSource.GetTemplate(m_project.VoiceActorList.Actors.Count);
				}

				Dictionary<string, CharacterGroup> characterIdToCharacterGroup = new Dictionary<string, CharacterGroup>();

				foreach (CharacterGroup group in charGroupTemplate.CharacterGroups.Values)
				{
					group.CharacterIds.IntersectWith(m_project.IncludedCharacterIds);

					characterGroups.Add(group);

					foreach (string id in group.CharacterIds)
					{
						characterIdToCharacterGroup.Add(id, group);
					}
				}

				var characterDetails = CharacterDetailData.Singleton.GetAll();

				foreach (var detail in characterDetails)
				{
					if (characterIdToCharacterGroup.ContainsKey(detail.Character))
					{
						var group = characterIdToCharacterGroup[detail.Character];

						if (detail.Gender != "")
							group.GenderAttributes.Add(detail.Gender);
						if (detail.Age != "")
							group.AgeAttributes.Add(detail.Age);
					}
				}

				m_project.CharacterGroupList.PopulateEstimatedHours(m_project.IncludedBooks);
			}
			m_project.CharacterGroupList.PopulateRequiredAttributes();

			m_characterGroupGrid.DataSource = characterGroups;
			m_characterGroupGrid.MultiSelect = true;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns["GroupNumber"], ListSortDirection.Ascending);
		}

		private void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();
			m_saveStatus.OnSaved();
		}

		private void AssignSelectedActorToSelectedGroup()
		{
			if (!m_canAssign)
				return;

			CharacterGroup group = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			if (group == null)
				return;
			group.AssignVoiceActor(m_voiceActorGrid.SelectedVoiceActorEntity);

			SaveAssignments();

			m_characterGroupGrid.Refresh();			
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

		private void m_btnSave_Click(object sender, EventArgs e)
		{
			SaveAssignments();
		}

		private void m_voiceActorGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_voiceActorGrid_CellUpdated(object sender, DataGridViewCellEventArgs e)
		{
			var grid = sender as DataGridView;
			if (grid.Columns[e.ColumnIndex].DataPropertyName == "Name")
				m_characterGroupGrid.Refresh();
		}

		private void m_characterGroupGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;

			if (grid.Columns[e.ColumnIndex].DataPropertyName == "VoiceActorAssignedName" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
				AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_CellDoubleClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;

			if (grid.IsCurrentCellInEditMode)
				return;

			if (grid.Columns[e.ColumnIndex].DataPropertyName == "Name" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
				AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_UserRemovedRows(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			m_characterGroupGrid.Refresh();
		}

		private void m_eitherGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_canAssign = m_voiceActorGrid.SelectedRows.Count == 1 && m_characterGroupGrid.SelectedRows.Count == 1;

			m_btnAssignActor.Enabled = m_canAssign;
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteRowsDialog.Message", "Are you sure you want to un-assign the actors from the selected groups?");
				string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteRowsDialog.Title", "Confirm");
				if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{

					for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
					{
						CharacterGroup entry = m_characterGroupGrid.SelectedRows[i].DataBoundItem as CharacterGroup;
						entry.RemoveVoiceActor();
					}

					SaveAssignments();

					m_characterGroupGrid.Refresh();
				}
			}
		}

		private void m_btnExport_Click(object sender, EventArgs e)
		{
			var characterGroups = m_characterGroupGrid.DataSource as SortableBindingList<CharacterGroup>;

			bool assignmentsComplete = characterGroups.All(t => t.IsVoiceActorAssigned);

			string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Message", "Some of the character groups have no voice talent assigned. Are you sure you want to export an incomplete script?\n(Note: You can export the script again as many times as you want.)");
			string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Title", "Export Incomplete Script?");
			if (assignmentsComplete || MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				SaveAssignments();
				new ProjectExport(m_project).Export(this);
			}
		}

		private void m_voiceActorGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_voiceActorGrid.ReadOnly && e.Button == MouseButtons.Left)
			{
				var hitInfo = m_voiceActorGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell)
				{
					var sourceActor = m_voiceActorGrid.SelectedVoiceActorEntity;
					if (sourceActor != null && sourceActor.HasMeaningfulData())
					{
						m_dragSource = DragSource.VoiceActorGrid;
						DoDragDrop(sourceActor, DragDropEffects.Copy);
					}
					else
					{
						DoDragDrop(0, DragDropEffects.None);
					}
				}
			}
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			if (!(e.Data.GetDataPresent(typeof(VoiceActor.VoiceActor)) || e.Data.GetDataPresent(typeof(CharacterGroup))))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			e.Effect = DragDropEffects.Copy;
		}

		private void m_characterGroupGrid_DragDrop(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
			{
				if (m_dragSource == DragSource.VoiceActorGrid)
				{
					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;
					AssignSelectedActorToSelectedGroup();
				}
				else if (m_dragSource == DragSource.CharacterGroupGrid)
				{
					CharacterGroup sourceGroup = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
					CharacterGroup destinationGroup = m_characterGroupGrid.Rows[hitInfo.RowIndex].DataBoundItem as CharacterGroup;

					VoiceActor.VoiceActor sourceActor = sourceGroup.VoiceActorAssigned;
					VoiceActor.VoiceActor destinationActor = destinationGroup.VoiceActorAssigned;

					destinationGroup.AssignVoiceActor(sourceActor);
					if (destinationActor != null)
						sourceGroup.AssignVoiceActor(destinationActor);
					else
						sourceGroup.RemoveVoiceActor();

					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;

					SaveAssignments();
					m_characterGroupGrid.Refresh();				
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

		private void m_characterGroupGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var hitInfo = m_characterGroupGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "VoiceActorAssignedName")
				{
					CharacterGroup sourceGroup = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
					if (sourceGroup.IsVoiceActorAssigned)
					{
						m_dragSource = DragSource.CharacterGroupGrid;
						DoDragDrop(sourceGroup, DragDropEffects.Copy);
					}
					else
					{
						DoDragDrop(0, DragDropEffects.None);
					}
				}
			}
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			AlignBtnAssignActorToSplitter();
		}

		private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
		{
			Refresh();
			m_btnAssignActor.Focus();
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SaveAssignments();
			Close();
		}

		private void m_linkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_voiceActorGrid.ReadOnly = false;
			//Restore EditMode overwritten in m_voiceActorGrid_Leave
			m_voiceActorGrid.EditMode = DataGridViewEditMode.EditOnEnter;
			m_voiceActorGrid.Focus();
		}

		private void m_voiceActorGrid_Leave(object sender, EventArgs e)
		{
			m_voiceActorGrid.ReadOnly = true;
			//EndEdit is insufficient in and of itself
			m_voiceActorGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			m_voiceActorGrid.EndEdit();
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
	}
}
