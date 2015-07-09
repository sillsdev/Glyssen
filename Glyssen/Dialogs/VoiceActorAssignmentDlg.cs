using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Properties;
using SIL.IO;
using SIL.ObjectModel;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private readonly Project m_project;

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_voiceActorGrid.Initialize(m_project);
			m_voiceActorGrid.CellDoubleClicked += m_voiceActorGrid_CellDoubleClicked;
			m_voiceActorGrid.GridMouseMove += m_voiceActorGrid_MouseMove;
			m_voiceActorGrid.UserRemovedRows += m_voiceActorGrid_UserRemovedRows;

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

				foreach (KeyValuePair<int, CharacterGroup> pair in charGroupTemplate.CharacterGroups)
					characterGroups.Add(pair.Value);
			}

			m_characterGroupGrid.DataSource = characterGroups;
			m_characterGroupGrid.MultiSelect = true;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns["GroupNumber"], ListSortDirection.Ascending);
		}

		private void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();
		}

		private void AssignSelectedActorToSelectedGroup()
		{
			CharacterGroup group = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			if (group == null)
				return;
			group.AssignVoiceActor(m_voiceActorGrid.SelectedVoiceActorEntity);

			SaveAssignments();

			m_characterGroupGrid.Refresh();			
		}

		private void m_btnAssignActor_Click(object sender, EventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_btnSave_Click(object sender, EventArgs e)
		{
			SaveAssignments();
		}

		private void m_characterGroupGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_CellDoubleClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_UserRemovedRows(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			m_characterGroupGrid.Refresh();
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
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

		private void m_btnExport_Click(object sender, EventArgs e)
		{
			SaveAssignments();
			new ProjectExport(m_project).Export(this);
		}

		private void m_voiceActorGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (m_voiceActorGrid.SelectedVoiceActorEntity != null && m_voiceActorGrid.SelectedRows.Count == 1)
				{
					DoDragDrop(m_voiceActorGrid.SelectedVoiceActorEntity, DragDropEffects.Copy);
				}
			}
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(VoiceActor.VoiceActor)))
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
				m_characterGroupGrid.ClearSelection();
				m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;
				AssignSelectedActorToSelectedGroup();
			}
		}
	}
}
