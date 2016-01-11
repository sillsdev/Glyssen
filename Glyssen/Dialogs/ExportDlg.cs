using System;
using System.IO;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class ExportDlg : CustomForm
	{
		private readonly ProjectExporter m_projectExporter;
		private ExportFileType m_selectedFileType = ExportFileType.Excel;
		private string m_actorDirectory;
		private string m_bookDirectory;
		private string m_defaultDirectory;
		private string m_actorDirectoryFmt;
		private string m_bookDirectoryFmt;

		public ExportDlg(ProjectExporter projectExporter)
		{
			m_projectExporter = projectExporter;

			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_defaultDirectory = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrWhiteSpace(m_defaultDirectory))
				m_defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string defaultFileName = m_projectExporter.Project.PublicationName + " " +
				LocalizationManager.GetString("DialogBoxes.ExportDlg.RecordingScriptFileNameDefaultSuffix", "Recording Script") +
				ProjectExporter.GetFileExtension(m_selectedFileType);
			m_lblFileName.Text = Path.Combine(m_defaultDirectory, defaultFileName);
			UpdateDisplay();
		}

		private void HandleStringsLocalized()
		{
			m_lblDescription.Text = string.Format(m_lblDescription.Text, ProductName);
			m_actorDirectoryFmt = m_lblActorDirectory.Text;
			m_bookDirectoryFmt = m_lblBookDirectory.Text;
		}

		private void UpdateDisplay()
		{
			m_lblFileExists.Visible = File.Exists(m_lblFileName.Text);

			UpdateActorDisplay();
			UpdateBookDisplay();
		}

		private void UpdateActorDisplay()
		{
			if (!m_projectExporter.IncludeVoiceActors)
			{
				m_checkIncludeActorBreakdown.Checked = false;
				m_checkIncludeActorBreakdown.Visible = false;
				m_lblActorDirectory.Visible = false;
				m_lblActorDirectoryExists.Visible = false;
			}
			else if (m_checkIncludeActorBreakdown.Checked)
			{
				m_lblActorDirectory.Visible = true;
				m_actorDirectory = Path.Combine(Path.GetDirectoryName(m_lblFileName.Text), Path.GetFileNameWithoutExtension(m_lblFileName.Text) + " Voice Actors");
				m_lblActorDirectory.Text = string.Format(m_actorDirectoryFmt, m_actorDirectory);
				m_lblActorDirectoryExists.Visible = Directory.Exists(m_actorDirectory);
			}
			else
			{
				m_lblActorDirectory.Visible = false;
				m_lblActorDirectoryExists.Visible = false;
			}
		}

		private void UpdateBookDisplay()
		{
			if (m_checkIncludeBookBreakdown.Checked)
			{
				m_lblBookDirectory.Visible = true;
				m_bookDirectory = Path.Combine(Path.GetDirectoryName(m_lblFileName.Text), Path.GetFileNameWithoutExtension(m_lblFileName.Text) + " Books");
				m_lblBookDirectory.Text = string.Format(m_bookDirectoryFmt, m_bookDirectory);
				m_lblBookDirectoryExists.Visible = Directory.Exists(m_bookDirectory);
			}
			else
			{
				m_lblBookDirectory.Visible = false;
				m_lblBookDirectoryExists.Visible = false;
			}
		}

		private void Browse_Click(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ExportDlg.SaveFileDialog.Title", "Choose File Location");
				dlg.OverwritePrompt = false;
				dlg.InitialDirectory = m_defaultDirectory;
				dlg.FileName = Path.GetFileName(m_lblFileName.Text);
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}|{4} ({5})|{5}",
					LocalizationManager.GetString("DialogBoxes.ExportDlg.ExcelFileTypeLabel", "Excel files"), "*" + ProjectExporter.kExcelFileExtension,
					LocalizationManager.GetString("DialogBoxes.ExportDlg.TabDelimitedFileTypeLabel", "Tab-delimited files"), "*" + ProjectExporter.kTabDelimitedFileExtension,
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"), "*.*");
				dlg.DefaultExt = ProjectExporter.kExcelFileExtension;
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					switch (dlg.FilterIndex)
					{
						//1-indexed
						case 2: //.txt
							m_selectedFileType = ExportFileType.TabSeparated;
							break;
						default:
							m_selectedFileType = ExportFileType.Excel;
							break;
					}

					m_defaultDirectory = Path.GetDirectoryName(dlg.FileName);
					m_lblFileName.Text = dlg.FileName;

					string expectedFileExtension = ProjectExporter.GetFileExtension(m_selectedFileType);
					if (!m_lblFileName.Text.EndsWith(expectedFileExtension))
						m_lblFileName.Text += expectedFileExtension;

					UpdateDisplay();
				}
			}
		}

		private void CheckIncludeActorBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			UpdateActorDisplay();
		}

		private void CheckIncludeBookBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			UpdateBookDisplay();
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (m_lblFileExists.Visible || m_lblActorDirectoryExists.Visible || m_lblBookDirectoryExists.Visible)
			{
				string text = LocalizationManager.GetString("DialogBoxes.ExportDlg.ConfirmOverwrite.Text", "Are you sure you want to overwrite the existing files?");
				string caption = LocalizationManager.GetString("DialogBoxes.ExportDlg.ConfirmOverwrite.Caption", "Overwrite?");
				if (MessageBox.Show(this, text, caption, MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					DialogResult = DialogResult.None;
					return;
				}
			}

			Settings.Default.DefaultExportDirectory = m_defaultDirectory;
			string filePath = m_lblFileName.Text;
			try
			{
				m_projectExporter.GenerateFile(filePath, m_selectedFileType);
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				ErrorReport.ReportNonFatalExceptionWithMessage(ex,
					string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExport",
					"Could not export data to {0}", "{0} is a file name."), filePath));
			}
			if (m_checkIncludeActorBreakdown.Checked)
			{
				try
				{
					Directory.CreateDirectory(m_actorDirectory);
					m_projectExporter.GenerateActorFiles(m_actorDirectory, m_selectedFileType);
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.ReportNonFatalExceptionWithMessage(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportActors",
						"Could not export one or more voice actor script files to {0}", "{0} is a directory name."), m_actorDirectory));
				}
			}
			if (m_checkIncludeBookBreakdown.Checked)
			{
				try
				{
					Directory.CreateDirectory(m_bookDirectory);
					m_projectExporter.GenerateBookFiles(m_bookDirectory, m_selectedFileType);
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.ReportNonFatalExceptionWithMessage(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportBooks",
						"Could not export one or more book script files to {0}", "{0} is a directory name."), m_bookDirectory));
				}
			}
		}
	}
}
