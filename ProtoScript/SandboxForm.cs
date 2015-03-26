using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using ProtoScript.Dialogs;
using ProtoScript.Properties;

namespace ProtoScript
{
	public partial class SandboxForm : Form
	{
		private Project m_project;
		private string m_projectInfoFmt;
		private string m_settingsInfoFmt;
		private string m_bookSelectionInfoFmt;
		private string m_projectLoadedFmt;
		private string m_percentAssigned;

		public SandboxForm()
		{
			InitializeComponent();
			HandleStringsLocalized();

			SetupUILanguageMenu();
			m_toolStrip.Renderer = new NoBorderToolStripRenderer();
			m_uiLanguageMenu.ToolTipText = LocalizationManager.GetString("MainForm.UILanguage", "User-interface Language");

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void SetProject(Project project)
		{
			m_project = project;
			if (m_project != null)
			{
				m_project.ProjectStateChanged += (sender, args) => FinishSetProjectIfReady();
				m_project.ProjectStateChanged += (sender, args) => UpdateProjectState();
				m_project.ProgressChanged += (sender, args) => UpdateDisplayOfProjectLoaded(args.ProgressPercentage);
			}

			bool validProject = m_project != null;
			m_linkChangeQuotationSystem.Enabled = validProject;
			m_btnSelectBooks.Enabled = validProject;
			m_btnSettings.Enabled = validProject;
			m_btnAssign.Enabled = validProject;
			m_btnExportToTabSeparated.Enabled = validProject;

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
			{
				if (m_project.ConfirmedQuoteSystem == null)
				{
					var msg = string.Format(LocalizationManager.GetString("Project.NeedQuoteSystem",
						"{0} was unable to identify the quotation marks used in this project. " +
						"The quotation system is needed to automatically identify quotations. " +
						"Would you like to display the {1} dialog box now in order to select the correct quotation system for this project?"),
						ProductName, LocalizationManager.GetString("ProjectSettingsDialog.ProjectSettings", "Project Settings"));
					if (MessageBox.Show(msg, ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
						HandleChangeQuotationMarks_Click(null, null);
				}
			}
		}

		protected void HandleStringsLocalized()
		{
			m_projectInfoFmt = m_lblProjectInfo.Text;
			m_settingsInfoFmt = m_lblSettingsInfo.Text;
			m_bookSelectionInfoFmt = m_lblBookSelectionInfo.Text;
			m_projectLoadedFmt = m_lblProjectLoaded.Text;
			m_percentAssigned = m_lblPercentAssigned.Text;
		}

		private void UpdateButtons(bool readOnly)
		{
			m_btnOpenProject.Enabled = !readOnly;
			m_imgCheckOpen.Visible = m_project != null;
			m_btnSettings.Enabled = !readOnly && m_project != null;
			m_btnSelectBooks.Enabled = !readOnly && m_project != null;
			m_btnAssign.Enabled = !readOnly && m_project != null;
			m_btnExportToTabSeparated.Enabled = !readOnly && m_project != null;
			m_lnkExit.Enabled = !readOnly;
			m_linkChangeQuotationSystem.Enabled = !readOnly && m_project != null;
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private DialogResult ShowOpenProjectDialog(bool welcome = false)
		{
			Project.CreateSampleProjectIfNeeded();

			using (var dlg = new OpenProjectDlg(m_project, welcome))
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
						case OpenProjectDlg.ProjectType.StandardFormatBook:
							LoadSfmBook(dlg.SelectedProject);
							break;
						case OpenProjectDlg.ProjectType.StandardFormatFolder:
							LoadSfmFolder(dlg.SelectedProject);
							break;
						default:
							MessageBox.Show("Sorry - not implemented yet");
							break;
					}
				}
				return result;
			}
		}

		private void SandboxForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject) || !File.Exists(Settings.Default.CurrentProject))
			{
				if (ShowOpenProjectDialog(true) != DialogResult.OK)
					Close();
			}
			else
			{
				LoadProject(Settings.Default.CurrentProject);
			}
		}

		private void LoadProject(string filePath)
		{
			if (!LoadAndHandleApplicationExceptions(() => SetProject(Project.Load(filePath))))
				SetProject(null);
		}

		private void LoadBundle(string bundlePath)
		{
			Bundle.Bundle bundle = null;

			if (!LoadAndHandleApplicationExceptions(() => { bundle = new Bundle.Bundle(bundlePath); }))
			{
				SetProject(null);
				return;
			}

			// See if we already have a project for this bundle and open it instead.
			var projFilePath = Project.GetProjectFilePath(bundle.Language, bundle.Id);
			if (File.Exists(projFilePath))
			{
				LoadProject(projFilePath);
				return;
			}
			SetProject(new Project(bundle));

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
			m_lblProjectInfo.Text = string.Format(m_projectInfoFmt, m_project != null ? m_project.ProjectSummary : String.Empty);
			m_lblSettingsInfo.Text = string.Format(m_settingsInfoFmt, m_project != null ? m_project.SettingsSummary : String.Empty);
			m_lblBookSelectionInfo.Text = string.Format(m_bookSelectionInfoFmt, m_project != null ? m_project.BookSelectionSummary : String.Empty);
			UpdateDisplayOfProjectLoaded(m_project != null ? m_project.PercentInitialized : 0);
			UpdateDisplayOfPercentAssigned();
			UpdateDisplayOfQuoteSystemInfo();
		}

		private void UpdateDisplayOfProjectLoaded(int percent)
		{
			double assignedAutomatically = 0;
			if (m_project != null && m_project.ProjectAnalysis != null)
				assignedAutomatically = m_project.ProjectAnalysis.TotalPercentAssigned;
			m_lblProjectLoaded.Text = string.Format(m_projectLoadedFmt, percent.ToString(CultureInfo.InvariantCulture), assignedAutomatically);
		}

		private void UpdateDisplayOfPercentAssigned()
		{
			double percentAssigned = 0;
			if (m_project != null && m_project.ProjectAnalysis != null)
				percentAssigned = m_project.ProjectAnalysis.UserPercentAssigned;
			m_lblPercentAssigned.Text = string.Format(m_percentAssigned, percentAssigned);
			m_imgCheckAssign.Visible = percentAssigned == 100d;
		}

		private void UpdateProjectState()
		{
			if (m_project != null)
				UpdateButtons((m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0);
		}

		private void UpdateDisplayOfQuoteSystemInfo()
		{
			m_lblSelectedQuotationMarks.Text = m_project != null ? m_project.QuoteSystem.ToString() : String.Empty;
		}
	
		private void SandboxForm_FormClosing(object sender, FormClosingEventArgs e)
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
				item.Click += ((a, b) =>
				{
					LocalizationManager.SetUILanguage(((CultureInfo)item.Tag).IetfLanguageTag, true);
					Settings.Default.UserInterfaceLanguage = ((CultureInfo)item.Tag).IetfLanguageTag;
					item.Select();
					m_uiLanguageMenu.Text = ((CultureInfo)item.Tag).NativeName;
				});
				if (((CultureInfo)item.Tag).IetfLanguageTag == Settings.Default.UserInterfaceLanguage)
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

		private void HandleChangeQuotationMarks_Click(object sender, EventArgs e)
		{
			bool reparseOkay = true;
			if (!m_project.IsReparseOkay())
			{
				string msg = LocalizationManager.GetString("Project.UnableToModifyQuoteSystemMessage", 
					"The original source of the project is no longer in its original location or has been significantly modified. " +
					"The quote system cannot be modified since that would require a reparse of the original text.");
				string title = LocalizationManager.GetString("Project.UnableToModifyQuoteSystem", "Unable to Modify Quote System");
				MessageBox.Show(msg, title);
				reparseOkay = false;
			}

			using (var viewModel = new BlockNavigatorViewModel(m_project, BlocksToDisplay.AllExpectedQuotes))
			{
				using (var dlg = new QuotationMarksDialog(m_project, viewModel, !reparseOkay))
				{
					if (dlg.ShowDialog(this) == DialogResult.OK)
						UpdateDisplayOfQuoteSystemInfo();
				}
			}
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			using (var viewModel = new AssignCharacterViewModel(m_project))
			{
				using (var dlg = new AssignCharacterDialog(viewModel))
					dlg.ShowDialog();
			}
			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
		}

		private void m_btnSelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDialog(m_project))
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					m_project.ClearProjectStatus();
					m_project.Analyze();
					UpdateDisplayOfProjectInfo();
				}
		}

		private void m_btnSettings_Click(object sender, EventArgs e)
		{
			bool readOnly = File.Exists(m_project.OriginalPathOfDblFile);
			using (var dlg = new ProjectMetadataDlg(m_project.ProjectMetadataViewModel, false, readOnly))
			{
				if (dlg.ShowDialog() == DialogResult.Cancel)
					return;

				m_project.ProjectMetadataViewModel = dlg.ProjectMetadataViewModel;
			}
		}

		private void m_lnkExit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Save is handled in FormClosing event
			Close();
		}

		public class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}
		}
	}
}
