using System;
using System.Windows.Forms;
using Glyssen;
using Glyssen.Shared;
using L10NSharp;
using L10NSharp.UI;
using Waxuquerque;

namespace GlyssenApp.UI.Dialogs
{
	public partial class ExportToRecordingToolDlg : Form
	{
		private readonly ProjectExporter m_viewModel;

		public ExportToRecordingToolDlg(ProjectExporter viewModel)
		{
			m_viewModel = viewModel;

			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void ExportToRecordingToolDlg_Load(object sender, EventArgs e)
		{
			if (Owner is MainForm)
				MainForm.SetChildFormLocation(this);
			else
				CenterToParent();
		}

		private void HandleStringsLocalized()
		{
			m_lblDescription.Text = string.Format(m_lblDescription.Text, ProductName, Constants.kGlyssenScriptFileExtension, "HearThis");
			m_lblWarning.Text = string.Format(m_lblWarning.Text, "HearThis");

			Text = string.Format(Text, m_viewModel.Project.Name);
		}

		private void Browse_Click(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ViewScriptDlg.ExportToHearThis.SaveFileDialog.Title", "Choose File Location");
				dlg.FileName = m_viewModel.Project.Name + Constants.kGlyssenScriptFileExtension;
				dlg.Filter = string.Format("{0} ({1})|{1}", "Glyssenscript files", "*" + Constants.kGlyssenScriptFileExtension);
				dlg.DefaultExt = Constants.kGlyssenScriptFileExtension;
				dlg.InitialDirectory = m_viewModel.CurrentBaseFolder;

				if (dlg.ShowDialog(this) == DialogResult.OK)
					m_fileNameTextBox.Text = dlg.FileName;
			}
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				ScriptExporter.MakeGlyssenScriptFile(m_viewModel.Project, m_viewModel.GetExportBlocks(), m_fileNameTextBox.Text);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private void FileNameTextBox_TextChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = !string.IsNullOrWhiteSpace(m_fileNameTextBox.Text);
		}
	}
}
