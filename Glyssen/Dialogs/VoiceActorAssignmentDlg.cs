using Glyssen.Character;
using Glyssen.Properties;
using Glyssen.VoiceActor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SIL.IO;
using SIL.ObjectModel;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private Project m_project;
		private ICharacterGroupSource m_charGroupSource;
		private CharacterGroupTemplate m_charGroupTemplate;
		private SortableBindingList<CharacterGroup> m_charActorPairs;

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_project = project;

			m_voiceActorGrid.Initialize(m_project);
			m_voiceActorGrid.CellDoubleClicked += m_voiceActorGrid_CellDoubleClicked;

			var characterGroups = m_project.CharacterGroupList.CharacterGroups;

			m_charActorPairs = new SortableBindingList<CharacterGroup>(characterGroups);

			//Expand some controls to fill out table panel layout
			Control groupTable = m_tableLayoutPanel.GetControlFromPosition(0, 0);
			m_tableLayoutPanel.SetRowSpan(groupTable, 3);
			Control actorTable = m_tableLayoutPanel.GetControlFromPosition(1, 0);
			m_tableLayoutPanel.SetColumnSpan(actorTable, 2);
			Control middleButton = m_tableLayoutPanel.GetControlFromPosition(1, 1);
			m_tableLayoutPanel.SetColumnSpan(middleButton, 2);


			if (m_charActorPairs.Count == 0)
			{
				using (TempFile tempFile = new TempFile())
				{
					File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
					m_charGroupSource = new CharacterGroupTemplateExcelFile(tempFile.Path);
					m_charGroupTemplate = m_charGroupSource.GetTemplate(4);
				}

				foreach (KeyValuePair<int, CharacterGroup> pair in m_charGroupTemplate.CharacterGroups)
				{
					CharacterGroup group = pair.Value;

					m_charActorPairs.Add(group);
				}
			}

			m_characterGroupGrid.DataSource = m_charActorPairs;
			m_characterGroupGrid.MultiSelect = true;
		}

		private void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();
		}

		private void AssignSelectedActorToSelectedGroup()
		{
			VoiceActor.VoiceActor assignee = m_voiceActorGrid.SelectedVoiceActorEntity;

			CharacterGroup entry = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			entry.AssignVoiceActor(assignee);

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

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					CharacterGroup entry = m_characterGroupGrid.SelectedRows[i].DataBoundItem as CharacterGroup;
					entry.RemoveVoiceActor();					
				}
				m_characterGroupGrid.Refresh();
			}
		}

		private void m_btnExport_Click(object sender, EventArgs e)
		{
			SaveAssignments();
			m_project.ExportTabDelimited(this, "MRK.txt", true);
		}
	}
}
