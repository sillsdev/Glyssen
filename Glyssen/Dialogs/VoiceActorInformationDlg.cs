using System;
using System.Windows.Forms;
using Glyssen.Controls;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorInformationDlg : Form
	{
		private readonly VoiceActorInformationViewModel m_viewModel;

		public VoiceActorInformationDlg(VoiceActorInformationViewModel viewModel, bool showNext = true)
		{
			InitializeComponent();

			m_dataGrid.Saved += m_dataGrid_Saved;
			m_dataGrid.RowCountChanged += m_dataGrid_RowCountChanged;

			m_viewModel = viewModel;
			m_dataGrid.Initialize(m_viewModel);

			m_btnNext.Visible = showNext;
			m_linkClose.Visible = showNext;
			m_btnOk.Visible = !showNext;
		}

		private void m_dataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_dataGrid_RowCountChanged(object sender, EventArgs e)
		{
			m_btnNext.Enabled = m_dataGrid.RowCount > 1;
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

		private void m_linkConfigureOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_viewModel.LaunchNarrationOptionsDlg();
		}
	}
}
