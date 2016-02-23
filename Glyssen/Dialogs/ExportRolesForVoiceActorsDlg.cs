using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using L10NSharp;
using L10NSharp.UI;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class ExportRolesForVoiceActorsDlg : Form
	{
		private readonly ProjectExporter m_projectExporter;
		private string m_defaultDirectory;
		private string m_rolesForVoiceActorsFileNameSuffix;

		public ExportRolesForVoiceActorsDlg(ProjectExporter projectExporter)
		{
			m_projectExporter = projectExporter;

			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_defaultDirectory = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrWhiteSpace(m_defaultDirectory))
			{
				m_defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ProductName);
				if (!Directory.Exists(m_defaultDirectory))
					Directory.CreateDirectory(m_defaultDirectory);
			}
			string defaultFileName = m_projectExporter.Project.PublicationName + " " +
				m_rolesForVoiceActorsFileNameSuffix + ProjectExporter.kExcelFileExtension;
			m_lblFileName.Text = Path.Combine(m_defaultDirectory, defaultFileName);

			UpdateDisplay();
		}

		private void HandleStringsLocalized()
		{
			m_lblDescription.Text = string.Format(m_lblDescription.Text, ProductName);
			m_rolesForVoiceActorsFileNameSuffix =
				LocalizationManager.GetString("DialogBoxes.ExportRolesForVoiceActorsDlg.RolesForVoiceActorsFileNameDefaultSuffix", "Roles for Voice Actors");
		}

		private void UpdateDisplay()
		{
			m_lblFileExists.Visible = File.Exists(m_lblFileName.Text);
		}

		private void Browse_Click(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ExportRolesForVoiceActorsDlg.SaveFileDialog.Title", "Choose File Location");
				dlg.OverwritePrompt = false;
				dlg.InitialDirectory = m_defaultDirectory;
				dlg.FileName = Path.GetFileName(m_lblFileName.Text);
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ExportRolesForVoiceActorsDlg.ExcelFileTypeLabel", "Excel files"), "*" + ProjectExporter.kExcelFileExtension,
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"), "*.*");
				dlg.DefaultExt = ProjectExporter.kExcelFileExtension;
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_defaultDirectory = Path.GetDirectoryName(dlg.FileName);
					m_lblFileName.Text = dlg.FileName;

					string expectedFileExtension = ProjectExporter.kExcelFileExtension;
					if (!m_lblFileName.Text.EndsWith(expectedFileExtension))
						m_lblFileName.Text += expectedFileExtension;

					UpdateDisplay();
				}
			}
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			Settings.Default.DefaultExportDirectory = m_defaultDirectory;
			string filePath = m_lblFileName.Text;
			try
			{
				m_projectExporter.ExportRolesForVoiceActors(filePath);

				if (m_defaultDirectory != null && m_checkOpenForMe.Checked)
					Process.Start(m_defaultDirectory);
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				ErrorReport.ReportNonFatalExceptionWithMessage(ex,
					string.Format(LocalizationManager.GetString("DialogBoxes.ExportRolesForVoiceActorsDlg.CouldNotExport",
					"Could not export data to {0}", "{0} is a file name."), filePath));
				DialogResult = DialogResult.None;
			}
		}
	}
}
