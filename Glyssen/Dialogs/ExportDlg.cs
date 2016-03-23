using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using SIL.Windows.Forms.Miscellaneous;

namespace Glyssen.Dialogs
{
	public partial class ExportDlg : Form
	{
		private readonly ExportViewModel m_viewModel;
		private string m_actorDirectoryFmt;
		private string m_bookDirectoryFmt;
		private string m_clipListFileFmt;
		private string m_openFileForMeText;

		public ExportDlg(ExportViewModel viewModel)
		{
			m_viewModel = viewModel;

			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_lblFileName.Text = m_viewModel.FullFileName;

			UpdateDisplay();
		}

		private void ExportDlg_Load(object sender, EventArgs e)
		{
			MainForm.SetChildFormLocation(this);
		}

		private void HandleStringsLocalized()
		{
			m_lblDescription.Text = string.Format(m_lblDescription.Text, ProductName);
			m_actorDirectoryFmt = m_lblActorDirectory.Text;
			m_bookDirectoryFmt = m_lblBookDirectory.Text;
			m_clipListFileFmt = m_lblClipListFilename.Text;
			m_openFileForMeText = m_checkOpenForMe.Text;

			Text = string.Format(Text, m_viewModel.Exporter.Project.Name);
		}

		private void UpdateDisplay()
		{
			m_lblFileExists.Visible = File.Exists(m_lblFileName.Text);

			UpdateActorDisplay();
			UpdateBookDisplay();
			UpdateClipListDisplay();
			UpdateOpenForMeDisplay();
		}

		private void UpdateActorDisplay()
		{
			if (!m_viewModel.Exporter.IncludeVoiceActors)
			{
				m_checkIncludeActorBreakdown.Checked = false;
				m_checkIncludeActorBreakdown.Visible = false;
				m_lblActorDirectory.Visible = false;
				m_lblActorDirectoryExists.Visible = false;
			}
			else if (m_checkIncludeActorBreakdown.Checked)
			{
				m_lblActorDirectory.Visible = true;
				m_lblActorDirectory.Text = string.Format(m_actorDirectoryFmt, m_viewModel.ActorDirectory);
				m_lblActorDirectoryExists.Visible = Directory.Exists(m_viewModel.ActorDirectory);
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
				m_lblBookDirectory.Text = string.Format(m_bookDirectoryFmt, m_viewModel.BookDirectory);
				m_lblBookDirectoryExists.Visible = Directory.Exists(m_lblBookDirectory.Text);
			}
			else
			{
				m_lblBookDirectory.Visible = false;
				m_lblBookDirectoryExists.Visible = false;
			}
		}

		private void UpdateClipListDisplay()
		{
			if (!m_viewModel.Exporter.IncludeVoiceActors)
			{
				m_checkIncludeClipListFile.Checked = false;
				m_checkIncludeClipListFile.Visible = false;
				m_lblClipListFilename.Visible = false;
				m_lblClipListFileExists.Visible = false;
			}
			else if (m_checkIncludeClipListFile.Checked)
			{
				var folder = Path.GetDirectoryName(m_lblFileName.Text) ?? string.Empty;
				var filename = Path.GetFileNameWithoutExtension(m_lblFileName.Text) ?? m_viewModel.Exporter.Project.PublicationName;

				var clipListFilenameSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ClipListFileNameSuffix",
					"Clip List");

				if (filename.Contains(m_viewModel.RecordingScriptFileNameSuffix))
					filename = filename.Replace(m_viewModel.RecordingScriptFileNameSuffix, clipListFilenameSuffix);
				else
					filename += " " + clipListFilenameSuffix;

				filename += ProjectExporter.kExcelFileExtension;

				m_lblClipListFilename.Text = string.Format(m_clipListFileFmt, Path.Combine(folder, filename));

				m_lblClipListFilename.Visible = true;
				m_lblClipListFileExists.Visible = File.Exists(m_checkIncludeClipListFile.Text);
			}
			else
			{
				m_lblClipListFilename.Visible = false;
				m_lblClipListFileExists.Visible = false;				
			}
		}

		private void UpdateOpenForMeDisplay()
		{
			if (m_checkIncludeActorBreakdown.Checked || m_checkIncludeBookBreakdown.Checked || m_checkIncludeClipListFile.Checked)
				m_checkOpenForMe.Text = LocalizationManager.GetString("DialogBoxes.ExportDlg.OpenFolderForMe", "Open the folder for me");
			else
				m_checkOpenForMe.Text = m_openFileForMeText;
		}

		private void Browse_Click(object sender, EventArgs e)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ExportDlg.SaveFileDialog.Title", "Choose File Location");
				dlg.OverwritePrompt = false;
				dlg.InitialDirectory = m_viewModel.DefaultDirectory;
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
							m_viewModel.SelectedFileType = ExportFileType.TabSeparated;
							break;
						default:
							m_viewModel.SelectedFileType = ExportFileType.Excel;
							break;
					}

					m_lblFileName.Text = dlg.FileName;

					string expectedFileExtension = ProjectExporter.GetFileExtension(m_viewModel.SelectedFileType);
					if (!m_lblFileName.Text.EndsWith(expectedFileExtension))
						m_lblFileName.Text += expectedFileExtension;

					UpdateDisplay();
				}
			}
		}

		private void CheckIncludeActorBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			UpdateActorDisplay();
			UpdateOpenForMeDisplay();
		}

		private void CheckIncludeBookBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			UpdateBookDisplay();
			UpdateOpenForMeDisplay();
		}

		private void CheckIncludeClipListFile_CheckedChanged(object sender, EventArgs e)
		{
			UpdateClipListDisplay();
			UpdateOpenForMeDisplay();
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			Enabled = false;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (m_viewModel.ExportNow(m_lblFileName.Text,
					m_checkIncludeActorBreakdown.Checked,
					m_checkIncludeBookBreakdown.Checked,
					m_checkOpenForMe.Checked))
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}
	}
}
