using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using Palaso.IO;
using Paratext;
using ProtoScript.Bundle;
using ProtoScript.Dialogs;
using ProtoScript.Properties;
using Canon = ProtoScript.Bundle.Canon;

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
		}

		private string ProjectsBaseFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Program.kCompany, Program.kProduct);
			}
		}

		protected void HandleStringsLocalized()
		{
			m_bundleIdFmt = m_lblBundleId.Text;
			m_LanguageIdFmt = m_lblLanguage.Text;
		}

		private void HandleSelectBundle_Click(object sender, EventArgs e)
		{
			using (var dlg = new SelectProjectDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					if (Path.GetExtension(dlg.FileName) == Project.kProjectFileExtension)
						LoadProject(dlg.FileName);
					else
						LoadBundle(dlg.FileName);
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox("");
		}

		private void SandboxForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject))
			{
				using (var dlg = new WelcomeDialog())
				{
					if (dlg.ShowDialog() == DialogResult.OK)
						LoadBundle(Settings.Default.CurrentProject);
				}
			}
			else
			{
				LoadProject(Settings.Default.CurrentProject);
			}
		}

		private void LoadProject(string filePath)
		{
			m_project = Project.Load(filePath);
			UpdateDisplayOfProjectIdInfo();
		}

		private void LoadBundle(string bundlePath)
		{
			var bundle = new Bundle.Bundle(bundlePath);
			// See if we already have a project for this bundle and open it instead.
			var projFilePath = Project.GetProjectFilePath(ProjectsBaseFolder, bundle.Language, bundle.Id);
			if (File.Exists(projFilePath))
			{
				LoadProject(projFilePath);
				return;
			}
			m_project = new Project(bundle.Metadata);
			UpdateDisplayOfProjectIdInfo();
			m_project.PopulateAndParseBooks(bundle);

			Canon canon;
			UsxDocument book;
			if (bundle.TryGetCanon(1, out canon))
			{
				if (canon.TryGetBook("MRK", out book))
				{
					m_project.AddBook("MRK", new QuoteParser("MRK", new UsxParser(book.GetChaptersAndParas()).Parse()).Parse());
				}
			}
		}

		private void UpdateDisplayOfProjectIdInfo()
		{
			m_lblBundleId.Text = string.Format(m_bundleIdFmt, m_project.Id);
			m_lblLanguage.Text = string.Format(m_LanguageIdFmt, m_project.Language);
		}

		private void HandleSave_Click(object sender, EventArgs e)
		{
			m_project.Save(ProjectsBaseFolder);
		}

		private void SandboxForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.Default.Save();
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
					m_project.ExportTabDelimited(dlg.FileName);
				}
			}
		}

		/// <summary>
		/// TODO This very rudamentary code is just for developer testing at this point
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_btnLoadSfm_Click(object sender, EventArgs e)
		{
			string sfmFilePath = null;
			using (var dlg = new OpenFileDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
					sfmFilePath = dlg.FileName;
			}
			string usfmStylesheetPath = Path.Combine(FileLocator.GetDirectoryDistributedWithApplication("sfm"), "usfm.sty");
			var book = new UsxDocument(UsfmToUsx.ConvertToXmlDocument(new ScrStylesheet(usfmStylesheetPath), File.ReadAllText(sfmFilePath)));
			var metadata = new DblMetadata { id = "sfm" + DateTime.Now.Ticks, language = new DblMetadataLanguage { iso = "zzz" } };
			m_project = new Project(metadata);
			m_project.AddBook("MRK", new QuoteParser("MRK", new UsxParser(book.GetChaptersAndParas()).Parse()).Parse());
		}

		private void m_btnSettings_Click(object sender, EventArgs e)
		{
			using (var dlg = new ProjectSettingsDialog(m_project))
				dlg.ShowDialog();
		}

	}
}
