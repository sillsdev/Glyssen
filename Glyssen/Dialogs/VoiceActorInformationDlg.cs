using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Controls;
using Glyssen.VoiceActor;
using L10NSharp;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private readonly VoiceActorInformationViewModel m_viewModel;
		private readonly bool m_initialEntry;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool showNext = true)
		{
			InitializeComponent();

			m_dataGrid.Saved += m_dataGrid_Saved;
			m_dataGrid.RowCountChanged += m_dataGrid_RowCountChanged;

			m_viewModel = viewModel;
			m_initialEntry = showNext;
			m_dataGrid.Initialize(m_viewModel);

			m_btnNext.Visible = showNext;
			m_linkClose.Visible = showNext;
			m_btnOk.Visible = !showNext;

			m_linkNarrationPreferences.Links.Clear();
			m_linkNarrationPreferences.Links.Add(60, 21);

			m_linkMoreInfo.Links.Clear();
			m_linkMoreInfo.Links.Add(77, 9);

			Project project = viewModel.Project;
			label1.Text = string.Format("This project has {0} books with {1} distinct character roles.",
				project.IncludedBooks.Count, project.GetKeyStrokesByCharacterId().Count);
			label2.Text = string.Format("Estimated recording time: {0:N2} hours", project.CharacterGroupList.CharacterGroups.Sum(g => g.EstimatedHours));

			UpdateTally();
		}

		public bool CloseParent { get; private set; }

		private void m_dataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();

			UpdateTally();
		}

		private void UpdateTally()
		{
			var actors = m_viewModel.Project.VoiceActorList.Actors;
			int numMale = actors.Count(a => a.Gender == ActorGender.Male);
			int numFemale = actors.Count(a => a.Gender == ActorGender.Female);
			int numChildren = actors.Count(a => a.Age == ActorAge.Child);
			m_lblTally.Text = string.Format("Tally: {0} Male, {1} Female, {2} Child", numMale, numFemale, numChildren);
		}

		private void m_dataGrid_RowCountChanged(object sender, EventArgs e)
		{
			m_btnNext.Enabled = m_dataGrid.RowCount > 1;
			m_btnOk.Enabled = m_dataGrid.RowCount > 1;
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			m_viewModel.AssessChanges();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void VoiceActorInformationDlg_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!m_initialEntry && !m_viewModel.Actors.Any())
			{
				string msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.CloseWithoutActors.Message", "If this dialog is closed without actors, character groups will be removed.");
				string caption = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.CloseWithoutActors.Caption", "No Actors");
				if (MessageBox.Show(msg, caption, MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					m_viewModel.ResetActorAndCharacterGroupState();
					CloseParent = true;
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new NarrationOptionsDlg(m_viewModel.Project))
				dlg.ShowDialog();
		}

		private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string msg = "Enter the cast of actual voice actors available for recording the script. Glyssen will optimize the character role assignments to match the actual cast, even if fewer or more than the \"recommended\" number of actors. " + Environment.NewLine +
				"If you have not yet identified all actors by name, you can enter the ones you know and come back later to enter the others." + Environment.NewLine +
				"You may also enter pseudonyms now as placeholders for additional actors you expect to identify." + Environment.NewLine +
				"See the Guide for the Recording Project Coordinator for ideas on how Glyssen can help with recruiting actors.";
			MessageBox.Show(msg);
		}
	}
}
