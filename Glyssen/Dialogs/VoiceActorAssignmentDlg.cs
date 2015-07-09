using System;
using System.Collections.Generic;
using System.ComponentModel;
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
			m_voiceActorGrid.UserRemovedRows += m_voiceActorGrid_UserRemovedRows;

			//REVIEW: We should be able to do this in designer and/or modify the layout
			//Expand some controls to fill out table panel layout
			Control groupTable = m_tableLayoutPanel.GetControlFromPosition(0, 0);
			m_tableLayoutPanel.SetRowSpan(groupTable, 3);
			Control actorTable = m_tableLayoutPanel.GetControlFromPosition(1, 0);
			m_tableLayoutPanel.SetColumnSpan(actorTable, 2);
			Control middleButton = m_tableLayoutPanel.GetControlFromPosition(1, 1);
			m_tableLayoutPanel.SetColumnSpan(middleButton, 2);

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
	}
}
