using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine.Export;
using GlyssenEngine.Utilities;
using L10NSharp;
using L10NSharp.XLiffUtils;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class ExportDlg : Form
	{
		private readonly ProjectExporter m_viewModel;
		private string m_actorDirectoryFmt;
		private string m_bookDirectoryFmt;
		private string m_clipDirectoryFmt;
		private string m_clipListFileFmt;
		private string m_openFileForMeText;

		public ExportDlg(ProjectExporter viewModel)
		{
			m_viewModel = viewModel;

			InitializeComponent();

			if (!m_viewModel.IncludeVoiceActors)
				HideControlsThatRequireVoiceActors();

			HandleStringsLocalized();
			LocalizeItemDlg<XLiffDocument>.StringsLocalized += HandleStringsLocalized;

			m_lblFileName.Text = m_viewModel.FullFileName;

			UpdateDisplay();
		}

		private void HideControlsThatRequireVoiceActors()
		{
			m_checkIncludeActorBreakdown.Checked = false;
			m_checkIncludeActorBreakdown.Visible = false;
			m_lblActorDirectory.Visible = false;
			m_lblActorDirectoryExists.Visible = false;

			m_checkIncludeClipListFile.Checked = false;
			m_checkIncludeClipListFile.Visible = false;
			m_lblClipListFilename.Visible = false;
			m_lblClipListFileExists.Visible = false;

			m_checkCreateClips.Checked = false;
			m_checkCreateClips.Visible = false;
			m_lblClipDirectory.Visible = false;
		}

		private void ExportDlg_Load(object sender, EventArgs e)
		{
			if (Owner is MainForm)
				MainForm.SetChildFormLocation(this);
			else
				CenterToParent();
		}

		private void HandleStringsLocalized()
		{
			m_lblDescription.Text = string.Format(m_lblDescription.Text, ProductName);
			m_actorDirectoryFmt = m_lblActorDirectory.Text;
			m_bookDirectoryFmt = m_lblBookDirectory.Text;
			m_clipDirectoryFmt = m_lblClipDirectory.Text;
			m_clipListFileFmt = m_lblClipListFilename.Text;
			m_openFileForMeText = m_checkOpenForMe.Text;

			Text = string.Format(Text, m_viewModel.Project.Name);
		}

		private void UpdateDisplay()
		{
			m_lblFileName.Text = m_viewModel.FullFileName;
			m_lblFileExists.Visible = File.Exists(m_lblFileName.Text);

			UpdateActorDisplay();
			UpdateBookDisplay();
			UpdateClipListDisplay();
			UpdateOpenForMeDisplay();
			UpdateCreateClipsDisplay();
		}

		private void UpdateActorDisplay()
		{
			if (!m_viewModel.IncludeVoiceActors)
				return;
			if (m_checkIncludeActorBreakdown.Checked)
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
				m_lblBookDirectoryExists.Visible = Directory.Exists(m_viewModel.BookDirectory);
			}
			else
			{
				m_lblBookDirectory.Visible = false;
				m_lblBookDirectoryExists.Visible = false;
			}
		}

		private void UpdateCreateClipsDisplay()
		{
			if (!m_viewModel.IncludeVoiceActors)
				return;

			if (m_checkCreateClips.Checked)
			{
				m_lblClipDirectory.Visible = true;
				m_lblClipDirectory.Text = string.Format(m_clipDirectoryFmt, m_viewModel.ClipDirectory);
			}
			else
			{
				m_lblClipDirectory.Visible = false;
			}
		}

		private void UpdateClipListDisplay()
		{
			if (!m_viewModel.IncludeVoiceActors)
				return;
			if (m_checkIncludeClipListFile.Checked)
			{
				var folder = Path.GetDirectoryName(m_lblFileName.Text) ?? string.Empty;
				var filename = Path.GetFileNameWithoutExtension(m_lblFileName.Text) ?? m_viewModel.Project.PublicationName;

				var clipListFilenameSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ClipListFileNameSuffix",
					"Clip List");

				if (filename.Contains(m_viewModel.RecordingScriptFileNameSuffix))
					filename = filename.Replace(m_viewModel.RecordingScriptFileNameSuffix, clipListFilenameSuffix);
				else
					filename += " " + clipListFilenameSuffix;

				filename += Constants.kExcelFileExtension;

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
				dlg.InitialDirectory = m_viewModel.CurrentBaseFolder;
				dlg.FileName = Path.GetFileName(m_lblFileName.Text);
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}|{4} ({5})|{5}",
					LocalizationManager.GetString("DialogBoxes.ExportDlg.ExcelFileTypeLabel", "Excel files"), "*" + Constants.kExcelFileExtension,
					LocalizationManager.GetString("DialogBoxes.ExportDlg.TabDelimitedFileTypeLabel", "Tab-delimited files"), "*" + ProjectExporter.kTabDelimitedFileExtension,
					L10N.AllFilesLabel, "*.*");
				dlg.DefaultExt = Constants.kExcelFileExtension;
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

					var newName = dlg.FileName;

					string expectedFileExtension = ProjectExporter.GetFileExtension(m_viewModel.SelectedFileType);
					if (!newName.EndsWith(expectedFileExtension))
						newName += expectedFileExtension;

					m_viewModel.FullFileName = newName;

					UpdateDisplay();
				}
			}
		}

		private void CheckIncludeActorBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			m_viewModel.IncludeActorBreakdown = m_checkIncludeActorBreakdown.Checked;
			UpdateActorDisplay();
			UpdateOpenForMeDisplay();
		}

		private void CheckIncludeBookBreakdown_CheckedChanged(object sender, EventArgs e)
		{
			m_viewModel.IncludeBookBreakdown = m_checkIncludeBookBreakdown.Checked;
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
			IReadOnlyDictionary<string, List<string>> lockedFiles;
			do
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					Analytics.Track("Export", new Dictionary<string, string>
					{
						{ "exportType", m_viewModel.SelectedFileType.ToString() },
						{ "includeVoiceActors", m_viewModel.IncludeVoiceActors.ToString() },
						{ "includeDelivery", m_viewModel.IncludeDelivery.ToString() }
					});

					lockedFiles = m_viewModel.ExportNow(m_checkOpenForMe.Checked);
					if (!lockedFiles.Any())
					{
						DialogResult = DialogResult.OK;
						Close();
						return;
					}
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}

				var bldr = new StringBuilder(String.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExport",
					"Export failed to write one or more files. Try closing any applications that have these files open, and then click {0}. Details:",
					"Param is \"Retry\" button label."),
					MessageBoxStrings.RetryButton));
				foreach (var key in lockedFiles.Keys)
				{
					bldr.Append("\r\n\r\n");
					bldr.AppendFormat(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportProblemExplanationLabel",
						"Error: {0}"), key);
					if (lockedFiles[key].Count > 1 || !key.Contains(lockedFiles[key][0]))
					{
						bldr.Append("\r\n");
						bldr.Append(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportProblemFilesLabel",
							"Files affected:"));
						foreach (var file in lockedFiles[key])
						{
							bldr.Append("\r\n\t");
							bldr.Append(file);
						}
					}
				}

				if (MessageBox.Show(bldr.ToString(), Text, MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
				{
					Enabled = true;
					m_viewModel.OpenExportFileOrLocation();
					return;
				}
			} while (true);
		}

		private void CheckCreateClips_CheckedChanged(object sender, EventArgs e)
		{
			m_viewModel.IncludeCreateClips = m_checkCreateClips.Checked;
			UpdateCreateClipsDisplay();
		}
	}
}
