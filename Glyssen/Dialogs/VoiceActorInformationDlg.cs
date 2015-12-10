using System;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Controls;
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
		}

		public bool CloseParent { get; private set; }

		private void m_dataGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
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
	}
}
