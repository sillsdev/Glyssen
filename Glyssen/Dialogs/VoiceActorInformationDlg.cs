using System;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Controls;
using Glyssen.Utilities;
using Glyssen.VoiceActor;
using L10NSharp;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : CustomForm
	{
		private readonly VoiceActorInformationViewModel m_viewModel;
		private readonly bool m_initialEntry;

		private string m_tallyFmt;
		private string m_projectSummaryFmt;
		private string m_recordingTimeFmt;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool showNext = true)
		{
			InitializeComponent();

			m_dataGrid.Saved += DataGrid_Saved;
			m_dataGrid.RowCountChanged += DataGrid_RowCountChanged;

			m_viewModel = viewModel;
			m_initialEntry = showNext;
			m_dataGrid.Initialize(m_viewModel);

			m_btnNext.Visible = showNext;
			m_linkClose.Visible = showNext;
			m_btnOk.Visible = !showNext;

			m_linkNarrationPreferences.Links.Clear();
			m_linkNarrationPreferences.Links.Add(60, 21); //TODO internationalize

			m_linkMoreInfo.Links.Clear();
			m_linkMoreInfo.Links.Add(77, 9); //TODO internationalize

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		public bool CloseParent { get; private set; }

		private void HandleStringsLocalized()
		{
			m_tallyFmt = m_lblTally.Text;
			m_projectSummaryFmt = m_lblProjectSummary.Text;
			m_recordingTimeFmt = m_lblRecordingTime.Text;

			Project project = m_viewModel.Project;
			m_lblProjectSummary.Text = string.Format(m_projectSummaryFmt, project.IncludedBooks.Count, project.GetKeyStrokesByCharacterId().Count);
			m_lblRecordingTime.Text = string.Format(m_recordingTimeFmt, project.GetEstimatedRecordingTime());
			UpdateTally();
		}

		private void UpdateTally()
		{
			var actors = m_viewModel.Project.VoiceActorList.Actors;
			int numMale = actors.Count(a => a.Gender == ActorGender.Male);
			int numFemale = actors.Count(a => a.Gender == ActorGender.Female);
			int numChildren = actors.Count(a => a.Age == ActorAge.Child);
			m_lblTally.Text = string.Format(m_tallyFmt, numMale, numFemale, numChildren);
		}

		private void DataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();

			UpdateTally();
		}

		private void DataGrid_RowCountChanged(object sender, EventArgs e)
		{
			m_btnNext.Enabled = m_dataGrid.RowCount > 1;
			m_btnOk.Enabled = m_dataGrid.RowCount > 1;
		}

		private void BtnNext_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void LinkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void BtnOk_Click(object sender, EventArgs e)
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

		private void LinkNarrationPreferences_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new NarrationOptionsDlg(m_viewModel.Project))
				dlg.ShowDialog();
		}

		private void LinkMoreInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string line1 = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.MoreInfo.Line1",
				"Enter the cast of actual voice actors available for recording the script. " +
				"Glyssen will optimize the character role assignments to match the actual cast, " +
				"even if fewer or more than the \"recommended\" number of actors.");
			string line2 = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.MoreInfo.Line2",
				"If you have not yet identified all actors by name, you can enter the ones you know and come back later to enter the others.");
			string line3 = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.MoreInfo.Line3",
				"You may also enter pseudonyms now as placeholders for additional actors you expect to identify.");
			string line4 = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.MoreInfo.Line4",
				"See the Guide for the Recording Project Coordinator for ideas on how Glyssen can help with recruiting actors.");
			string msg = line1 + Environment.NewLine +
				line2 + Environment.NewLine +
				line3 + Environment.NewLine +
				line4;
			MessageBox.Show(msg);
		}
	}
}
