using System;
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
		private string m_bundleIdFmt;
		private string m_LanguageIdFmt;

		public SandboxForm()
		{
			InitializeComponent();
			HandleStringsLocalized();

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void SetProject(Project project)
		{
			m_project = project;
			m_linkChangeQuotationSystem.Enabled = m_btnExportToTabSeparated.Enabled = m_project != null;
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
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private DialogResult ShowOpenProjectDialog(bool welcome = false)
		{
			Project.CreateSampleProjectIfNeeded();

			using (var dlg = new OpenProjectDlg(welcome))
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
			m_lblBundleId.Text = string.Format(m_bundleIdFmt, m_project != null ? m_project.Id : String.Empty);
			m_lblLanguage.Text = string.Format(m_LanguageIdFmt, m_project != null ? m_project.Language : String.Empty);
			UpdateDisplayOfQuoteSystemInfo();
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
			using (var dlg = new QuotationMarksDialog(m_project, !reparseOkay))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
					UpdateDisplayOfQuoteSystemInfo();
			}
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			var viewModel = new AssignCharacterViewModel(m_project);
			if (viewModel.RelevantBlockCount == 0)
			{
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.AllAssignmentsMadeAutomatically",
					"Character assigments were made automatically for all quotations in the Scripture text. There is nothing more to do at this time.");
				MessageBox.Show(this, msg, ProductName, MessageBoxButtons.OK);
				return;
			}
			using (var dlg = new AssignCharacterDialog(viewModel))
				dlg.ShowDialog();
		}

		private void m_btnSelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDialog(m_project))
				dlg.ShowDialog();
		}
	}
}
