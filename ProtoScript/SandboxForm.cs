using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Bundle;
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

			Canon canon;
			UsxDocument book;
			if (bundle.TryGetCanon(1, out canon))
			{
				if (canon.TryGetBook("MRK", out book))
				{
					m_project.AddBook("MRK", new UsxParser(book.GetParas()).Parse());
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
	}
}
