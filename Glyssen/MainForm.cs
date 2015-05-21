using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Dialogs;
using Glyssen.Properties;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using SIL.DblBundle;
using SIL.IO;
using SIL.Windows.Forms.Miscellaneous;

namespace Glyssen
{
	public partial class MainForm : Form
	{
		private Project m_project;
		private string m_percentAssignedFmt;

		public MainForm()
		{
			InitializeComponent();

			InitializeLocalizableFormats();

			SetupUILanguageMenu();
			m_toolStrip.Renderer = new NoBorderToolStripRenderer();
			m_uiLanguageMenu.ToolTipText = LocalizationManager.GetString("MainForm.UILanguage", "User-interface Language");

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void SetProject(Project project)
		{
			m_project = project;
			if (m_project != null)
			{
				m_project.ProjectStateChanged += (sender, args) => FinishSetProjectIfReady();
				m_project.ProjectStateChanged += (sender, args) => UpdateProjectState();
			}

			ResetUi();

			if (m_project != null && (m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0)
				return; //FinishSetProject will be called by the event handler

			FinishSetProject();
		}

		private void FinishSetProjectIfReady()
		{
			if (m_project != null && (m_project.ProjectState & ProjectState.ReadyForUserInteraction) > 0)
				FinishSetProject();
		}

		private void FinishSetProject()
		{
			UpdateDisplayOfProjectInfo();

			if (m_project != null)
				Analytics.Track("LoadProject", new Dictionary<string, string>
				{
					{ "language", m_project.LanguageIsoCode },
					{ "ID", m_project.Id },
					{ "recordingProjectName", m_project.Name },
				});
		}

		private void InitializeLocalizableFormats()
		{
		}

		protected void HandleStringsLocalized()
		{
			m_percentAssignedFmt = m_lblPercentAssigned.Text;
			UpdateLocalizedText();
		}

		private void UpdateButtons(bool readOnly)
		{
			bool validProject = m_project != null;
			m_btnOpenProject.Enabled = !readOnly;
			m_imgCheckOpen.Visible = validProject;
			m_btnSettings.Enabled = !readOnly && validProject;
			m_imgCheckSettings.Visible = m_btnSettings.Enabled && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed && m_project.IsQuoteSystemReadyForParse;
			m_btnSelectBooks.Enabled = !readOnly && validProject && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed;
			m_imgCheckBooks.Visible = m_btnSelectBooks.Enabled && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed;
			m_btnAssign.Enabled = !readOnly && m_imgCheckSettings.Visible && m_imgCheckBooks.Visible;
			m_imgCheckAssign.Visible = m_btnAssign.Enabled && m_project.ProjectAnalysis.UserPercentAssigned == 100d;
			m_btnExportToTabSeparated.Enabled = !readOnly && m_imgCheckAssign.Visible;
			m_lnkExit.Enabled = !readOnly;
		}

		private void ResetUi()
		{
			m_btnSelectBooks.Enabled = false;
			m_btnSettings.Enabled = false;
			m_btnAssign.Enabled = false;
			m_btnExportToTabSeparated.Enabled = false;
			m_imgCheckOpen.Visible = false;
			m_imgCheckSettings.Visible = false;
			m_imgCheckBooks.Visible = false;
			m_imgCheckAssign.Visible = false;
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private void ShowOpenProjectDialog()
		{
			using (var dlg = new OpenProjectDlg(m_project))
			{
				var result = dlg.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					switch (dlg.Type)
					{
						case OpenProjectDlg.ProjectType.ExistingProject:
							LoadProject(dlg.SelectedProject);
							break;
						case OpenProjectDlg.ProjectType.TextReleaseBundle:
							LoadBundle(dlg.SelectedProject);
							break;
						//case OpenProjectDlg.ProjectType.StandardFormatBook:
							//LoadSfmBook(dlg.SelectedProject);
							//break;
						//case OpenProjectDlg.ProjectType.StandardFormatFolder:
							//LoadSfmFolder(dlg.SelectedProject);
							//break;
						default:
							MessageBox.Show("Sorry - not implemented yet");
							break;
					}
				}
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject) || !File.Exists(Settings.Default.CurrentProject))
				SetProject(null);
			else
				LoadProject(Settings.Default.CurrentProject);
		}

		private void LoadProject(string filePath)
		{
			if (!LoadAndHandleApplicationExceptions(() => SetProject(Project.Load(filePath))))
				SetProject(null);
		}

		private void LoadBundle(string bundlePath)
		{
			GlyssenBundle bundle = null;

			if (!LoadAndHandleApplicationExceptions(() => { bundle = new GlyssenBundle(bundlePath); }))
			{
				SetProject(null);
				return;
			}

			string projFilePath;
			// See if we already have project(s) for this bundle and give the user the option of opening an existing project instead.
			var publicationFolder = Project.GetPublicationFolderPath(bundle);
			if (Directory.Exists(publicationFolder) && 
				Directory.GetDirectories(publicationFolder).Any(f => Directory.GetFiles(f, "*" + Project.kProjectFileExtension).Any()))
			{
				using (var dlg = new SelectExistingProjectDlg(bundle))
				{
					dlg.ShowDialog(this);
					projFilePath = dlg.SelectedProject;
				}
			}
			else
				projFilePath = Project.GetDefaultProjectFilePath(bundle);
			
			if (File.Exists(projFilePath))
			{
				LoadProject(projFilePath);
				bundle.Dispose();
				return;
			}

			var recordingProjectName = Path.GetFileName(Path.GetDirectoryName(projFilePath));
			Versification.Table.HandleVersificationLineError = null;
			try
			{
				SetProject(new Project(bundle, recordingProjectName));
			}
			catch (InvalidVersificationLineException ex)
			{
				var error = ex.Message;
				int i = error.IndexOf("\n", StringComparison.Ordinal);
				if (i > 0)
					error = error.Substring(0, i);
				var msg = string.Format(LocalizationManager.GetString("Project.InvalidVersificationFile",
					"Invalid versification file in text release bundle. Unable to create project.\r\n" +
					"Text release Bundle: {0}\r\n" +
					"Versification file: {1}\r\n" +
					"Error: {2}"),
					bundlePath, DblBundleFileUtils.kVersificationFileName, error);
				MessageBox.Show(msg, Program.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				SetProject(null);
			}

			bundle.Dispose();
		}

		private bool LoadAndHandleApplicationExceptions(Action loadCommand)
		{
			try
			{
				loadCommand();
			}
			catch (ApplicationException ex)
			{
				var bldr = new StringBuilder(ex.Message);
				if (ex.InnerException != null)
				{
					bldr.Append(Environment.NewLine);
					bldr.Append(ex.InnerException);
				}

				MessageBox.Show(bldr.ToString(), ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				UpdateDisplayOfProjectInfo();
				return false;
			}
			return true;
		}

		private void UpdateDisplayOfProjectInfo()
		{
			UpdateProjectState();
			UpdateLocalizedText();
		}

		public void UpdateLocalizedText()
		{
			m_lblProjectInfo.Text = m_project != null ? m_project.ProjectSummary : String.Empty;

			if (m_project != null && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed)
				m_lblSettingsInfo.Text = m_project.IsQuoteSystemReadyForParse ? m_project.SettingsSummary : LocalizationManager.GetString("MainForm.QuoteMarksNeedReview", "Quote marks need to be reviewed");
			else
				m_lblSettingsInfo.Text = String.Empty;

			m_lblBookSelectionInfo.Text = m_project != null && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed ? m_project.BookSelectionSummary : String.Empty;
			
			UpdateDisplayOfPercentAssigned();
		}

		private void UpdateDisplayOfPercentAssigned()
		{
			if (!m_btnAssign.Enabled)
			{
				m_lblPercentAssigned.Text = string.Empty;
				return;
			}

			double percentAssigned = 0;
			if (m_project != null && m_project.ProjectAnalysis != null)
				percentAssigned = m_project.ProjectAnalysis.UserPercentAssigned;
			m_lblPercentAssigned.Text = percentAssigned > 0 ? string.Format(m_percentAssignedFmt, percentAssigned) : string.Empty;
		}

		private void UpdateProjectState()
		{
			if (m_project != null)
				UpdateButtons((m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0);
		}
	
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveCurrentProject();
			Settings.Default.Save();
		}

		private void SaveCurrentProject()
		{
			if (m_project != null)
				m_project.Save();
		}

		private void HandleExportToTabSeparated_Click(object sender, EventArgs e)
		{
			var defaultDir = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrEmpty(defaultDir))
			{
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = LocalizationManager.GetString("DialogBoxes.ExportDlg.Title", "Export Tab-Delimited Data");
				dlg.OverwritePrompt = true;
				dlg.InitialDirectory = defaultDir;
				dlg.FileName = "MRK.txt";
				dlg.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ExportDlg.TabDelimitedFileTypeLabel", "Tab-delimited files"),
					"*.txt",
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
					"*.*");
				dlg.DefaultExt = ".txt";
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					Settings.Default.DefaultExportDirectory = Path.GetDirectoryName(dlg.FileName);
					try
					{
						m_project.ExportTabDelimited(dlg.FileName);
						Analytics.Track("Export");
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			}
		}

		private void LoadSfmBook(string sfmFilePath)
		{
			try
			{
				SetProject(SfmLoader.LoadSfmBookAndMetadata(sfmFilePath));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void LoadSfmFolder(string sfmFolderPath)
		{
			try
			{
				SetProject(SfmLoader.LoadSfmFolderAndMetadata(sfmFolderPath));
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void SetupUILanguageMenu()
		{
			m_uiLanguageMenu.DropDownItems.Clear();
			foreach (var lang in LocalizationManager.GetUILanguages(true))
			{
				var item = m_uiLanguageMenu.DropDownItems.Add(lang.NativeName);
				item.Tag = lang;
				string languageId = ((CultureInfo)item.Tag).IetfLanguageTag;
				item.Click += ((a, b) =>
				{
					Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", languageId }, { "initialStartup", "true" } });

					LocalizationManager.SetUILanguage(languageId, true);
					Settings.Default.UserInterfaceLanguage = languageId;
					item.Select();
					m_uiLanguageMenu.Text = ((CultureInfo)item.Tag).NativeName;
				});
				if (languageId == Settings.Default.UserInterfaceLanguage)
				{
					m_uiLanguageMenu.Text = ((CultureInfo)item.Tag).NativeName;
				}
			}

			m_uiLanguageMenu.DropDownItems.Add(new ToolStripSeparator());
			var menu = m_uiLanguageMenu.DropDownItems.Add(LocalizationManager.GetString("MainForm.MoreMenuItem",
				"More...", "Last item in menu of UI languages"));
			menu.Click += ((a, b) =>
			{
				Program.LocalizationManager.ShowLocalizationDialogBox(false);
				SetupUILanguageMenu();
			});
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			using (var viewModel = new AssignCharacterViewModel(m_project))
			{
				using (var dlg = new AssignCharacterDlg(viewModel))
					dlg.ShowDialog();
			}
			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
		}

		private void m_btnSelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDlg(m_project))
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					m_project.ClearAssignCharacterStatus();
					m_project.Analyze();
					UpdateDisplayOfProjectInfo();
				}
		}

		private void m_btnSettings_Click(object sender, EventArgs e)
		{
			var model = new ProjectMetadataViewModel(m_project);
			using (var dlg = new ProjectSettingsDlg(model))
			{
				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				m_project.UpdateSettings(model);
			}
			UpdateDisplayOfProjectInfo();
		}

		private void m_lnkExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Save is handled in FormClosing event
			Close();
		}

		private void m_btnAbout_Click(object sender, EventArgs e)
		{
			using (var dlg = new SILAboutBox(FileLocator.GetFileDistributedWithApplication("aboutbox.htm")))
			{
				dlg.ShowDialog();
			}
		}

		public class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}
		}
	}
}
