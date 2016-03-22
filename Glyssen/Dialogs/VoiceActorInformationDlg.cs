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
	public partial class VoiceActorInformationDlg : FormWithPersistedSettings
	{
		private readonly VoiceActorInformationViewModel m_viewModel;
		private readonly bool m_changeOkToGenerateGroups;

		private string m_tallyFmt;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool initialEntry, bool changeOkToGenerateGroups)
		{
			InitializeComponent();

			m_dataGrid.Saved += DataGrid_Saved;
			m_dataGrid.RowCountChanged += DataGrid_RowCountChanged;

			m_viewModel = viewModel;
			m_changeOkToGenerateGroups = changeOkToGenerateGroups;
			m_dataGrid.Initialize(m_viewModel, !initialEntry);

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		//public bool CloseParent { get; private set; }

		private void VoiceActorInformationDlg_Load(object sender, EventArgs e)
		{
			if (Owner is MainForm)
				MainForm.SetChildFormLocation(this);
			else
				CenterToParent();
		}

		private void HandleStringsLocalized()
		{
			m_tallyFmt = m_lblTally.Text;

			m_lblActorsEnteredSoFar.Text = string.Format(m_lblActorsEnteredSoFar.Text, m_viewModel.InitialActorCount);
			if (m_viewModel.DataHasChanged)
				m_btnCancelClose.Text = LocalizationManager.GetString("Common.Close", "Close");
			UpdateTally();

			Text = string.Format(Text, m_viewModel.Project.Name);
		}

		private void UpdateTally()
		{
			var actors = m_viewModel.Project.VoiceActorList.ActiveActors;
			int numMale = actors.Count(a => a.Gender == ActorGender.Male);
			int numFemale = actors.Count(a => a.Gender == ActorGender.Female);
			int numChildren = actors.Count(a => a.Age == ActorAge.Child);
			m_lblTally.Text = string.Format(m_tallyFmt, numMale, numFemale, numChildren);
		}

		private void DataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();

			if (m_viewModel.DataHasChanged)
			{
				m_btnCancelClose.Text = LocalizationManager.GetString("Common.Close", "Close");
			}

			m_btnOk.Text = m_viewModel.DataHasChangedInWaysThatMightAffectGroupGeneration && m_changeOkToGenerateGroups ?
				LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.GenerateGroupsButton", "Generate Groups") :
				LocalizationManager.GetString("Common.OK", "OK");

			// ENHANCE: This is likely too much overhead and not worth it, but maybe we should kick off group generation here and
			// when it completes, we check to see if the groups are identical or not. If not, we display the Generate Groups button
			// instead of OK. Otherwise, the onus is on the view model to correctly determine whether  the groups are going to
			// change based on the data changes it knows about.

			UpdateTally();
		}

		private void DataGrid_RowCountChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_dataGrid.RowCount > 1;
		}

		private void HandleCancelOrCloseButtonClicked(object sender, EventArgs e)
		{
			Close(DialogResult.Cancel);
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			Close(DialogResult.OK);
		}

		private void Close(DialogResult dialogResult)
		{
			DialogResult = dialogResult;
			Close();
		}

		// Don't think this is needed anymore.
		//private void VoiceActorInformationDlg_FormClosing(object sender, FormClosingEventArgs e)
		//{
		//	if (!m_initialEntry && !m_viewModel.Actors.Any())
		//	{
		//		string msg = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.CloseWithoutActors.Message", "If this dialog is closed without actors, character groups will be removed.");
		//		string caption = LocalizationManager.GetString("DialogBoxes.VoiceActorInformation.CloseWithoutActors.Caption", "No Actors");
		//		if (MessageBox.Show(msg, caption, MessageBoxButtons.OKCancel) == DialogResult.OK)
		//		{
// This method has been moved to VoiceActorAssignmentViewModel (where it eventually might not be needed either):
		//			m_viewModel.ResetActorAndCharacterGroupState();
		//			CloseParent = true;
		//		}
		//		else
		//		{
		//			e.Cancel = true;
		//		}
		//	}
		//}
	}
}
