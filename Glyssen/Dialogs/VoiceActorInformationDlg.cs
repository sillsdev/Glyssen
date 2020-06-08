using System;
using System.Linq;
using Glyssen.Utilities;
using GlyssenEngine.Casting;
using GlyssenEngine.ViewModels;
using L10NSharp;
using L10NSharp.XLiffUtils;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : FormWithPersistedSettings
	{
		private readonly VoiceActorInformationViewModel m_viewModel;
		private readonly bool m_changeOkToGenerateGroups;

		private string m_tallyFmt;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool initialEntry, bool changeOkToGenerateGroups, bool enableOkButtonEvenIfNoChanges = false)
		{
			InitializeComponent();

			m_dataGrid.Saved += DataGrid_Saved;

			m_viewModel = viewModel;
			m_changeOkToGenerateGroups = changeOkToGenerateGroups;
			m_dataGrid.Initialize(m_viewModel, !initialEntry);
			if (enableOkButtonEvenIfNoChanges)
				m_btnOk.Enabled = m_viewModel.ActiveActors.Any();

			HandleStringsLocalized();
			LocalizeItemDlg<XLiffDocument>.StringsLocalized += HandleStringsLocalized;
		}

		private void VoiceActorInformationDlg_Load(object sender, EventArgs e)
		{
			// TODO: re-enable this button once help has been implemented
			m_toolStripButtonHelp.Visible = false;

			TileFormLocation();
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
			// instead of OK. Otherwise, the onus is on the view model to correctly determine whether the groups are going to
			// change based on the data changes it knows about.

			UpdateTally();

			m_btnOk.Enabled = m_viewModel.ActiveActors.Any();
		}
	}
}
