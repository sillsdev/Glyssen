using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using ProtoScript.Dialogs;
using ProtoScript.Properties;
using ProtoScript.Utilities;

namespace ProtoScript
{
	public partial class SandboxForm : Form
	{
		private Project m_project;
		private AssignCharacterViewModel m_assignCharacterViewModel;
		private string m_bundleIdFmt;
		private string m_LanguageIdFmt;
		private string m_projectLoadedFmt;
		private string m_percentAssigned;

		public SandboxForm()
		{
			InitializeComponent();
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
			if (m_assignCharacterViewModel != null)
				m_assignCharacterViewModel.Dispose();

			if (m_project != null)
				m_assignCharacterViewModel = new AssignCharacterViewModel(m_project);

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
			m_bundleIdFmt = m_lblBundleId.Text;
			m_LanguageIdFmt = m_lblLanguage.Text;
			m_projectLoadedFmt = m_lblProjectLoaded.Text;
			m_percentAssigned = m_lblPercentAssigned.Text;
		}

		private void SetReadOnly(bool readOnly)
		{
			m_btnOpenProject.Enabled = !readOnly;
			m_btnSettings.Enabled = !readOnly && m_project != null;
			m_btnSelectBooks.Enabled = !readOnly && m_project != null;
			m_btnAssign.Enabled = !readOnly && m_project != null;
			m_btnExportToTabSeparated.Enabled = !readOnly && m_project != null;
			m_btnLocalize.Enabled = !readOnly;
			m_linkChangeQuotationSystem.Enabled = !readOnly && m_project != null;
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private DialogResult ShowOpenProjectDialog()
		{
			Project.CreateSampleProjectIfNeeded();

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

		private void m_btnLocalize_Click(object sender, EventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox("");
		}

		private void SandboxForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject) || !File.Exists(Settings.Default.CurrentProject))
			{
				if (ShowOpenProjectDialog() != DialogResult.OK)
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
				StringBuilder bldr = new StringBuilder(ex.Message);
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
			m_lblBundleId.Text = string.Format(m_bundleIdFmt, m_project != null ? m_project.Name : String.Empty);
			m_lblLanguage.Text = string.Format(m_LanguageIdFmt, m_project != null ? m_project.LanguageIsoCode : String.Empty);
			UpdateDisplayOfProjectLoaded(m_project != null ? m_project.PercentInitialized : 0);
			UpdateDisplayOfPercentAssigned();
			UpdateDisplayOfQuoteSystemInfo();
		}

		private void UpdateDisplayOfProjectLoaded(int percent)
		{
			m_lblProjectLoaded.Text = string.Format(m_projectLoadedFmt, percent.ToString(CultureInfo.InvariantCulture));
			m_lblProjectLoaded.ForeColor = percent < 100 ? Color.Red : Color.LawnGreen;
		}

		private void UpdateDisplayOfPercentAssigned()
		{
			int percentAssigned = 0;
			if (m_assignCharacterViewModel != null)
				percentAssigned = MathUtilities.Percent(m_assignCharacterViewModel.AssignedBlockCount, m_assignCharacterViewModel.RelevantBlockCount);
			m_lblPercentAssigned.Text = string.Format(m_percentAssigned, percentAssigned);
		}

		private void UpdateProjectState()
		{
			if (m_project != null)
				SetReadOnly((m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0);
		}

		private void UpdateDisplayOfQuoteSystemInfo()
		{
			m_lblSelectedQuotationMarks.Text = m_project != null ? m_project.QuoteSystem.ToString() : String.Empty;
		}
	
		private void SandboxForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveCurrentProject();
			Settings.Default.Save();

			if (m_assignCharacterViewModel != null)
				m_assignCharacterViewModel.Dispose();
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
			if (m_assignCharacterViewModel.RelevantBlockCount == 0)
			{
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.AllAssignmentsMadeAutomatically",
					"Character assigments were made automatically for all quotations in the Scripture text. There is nothing more to do at this time.");
				MessageBox.Show(this, msg, ProductName, MessageBoxButtons.OK);
				return;
			}
			using (var dlg = new AssignCharacterDialog(m_assignCharacterViewModel))
				dlg.ShowDialog();
			UpdateDisplayOfPercentAssigned();
		}

		private void m_btnSelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDialog(m_project))
				dlg.ShowDialog();
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
	}
}
