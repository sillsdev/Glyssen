using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using Microsoft.Win32;
using ProtoScript.Controls;
using ProtoScript.Properties;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace ProtoScript.Dialogs
{
	public partial class OpenProjectDlg : Form
	{
		public enum ProjectType
		{
			ExistingProject,
			TextReleaseBundle,
			ParatextProject,
			StandardFormatFolder,
			StandardFormatBook,
		}

		private readonly Project m_currentProject;

		public OpenProjectDlg(Project currentProject)
		{
			m_currentProject = currentProject;
			InitializeComponent();

			if (Settings.Default.OpenProjectDlgFormSettings == null)
				Settings.Default.OpenProjectDlgFormSettings = FormSettings.Create(this);

			if (m_currentProject != null)
			{
				m_listExistingProjects.SelectedProject = m_currentProject.ProjectFilePath;
				SelectedProject = m_currentProject.ProjectFilePath;
				m_listExistingProjects.AddReadOnlyProject(m_currentProject);
			}
			else
				m_btnOk.Enabled = false;
		}

		[DefaultValue(ProjectType.ExistingProject)]
		public ProjectType Type { get; private set; }

		public string SelectedProject { get; private set; }

		private void m_linkTextReleaseBundle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new SelectProjectDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					SelectedProject = dlg.FileName;
					if (Path.GetExtension(SelectedProject) == Project.kProjectFileExtension)
						Type = ProjectType.ExistingProject;
					else
						Type = ProjectType.TextReleaseBundle;
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		private string DefaultSfmDirectory
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultSfmDirectory))
					return Settings.Default.DefaultSfmDirectory;
				return ParatextProjectsFolder;
			}
		}

		public static string ParatextProjectsFolder
		{
			get
			{
				const string ParatextRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\ScrChecks\1.0\Settings_Directory";
				var path = Registry.GetValue(ParatextRegistryKey, "", null);
				if (path != null)
				{
					if (Directory.Exists(path.ToString()))
						return path.ToString();
				}
				else
				{
					foreach (var drive in Environment.GetLogicalDrives())
					{
						string possibleLocation = Path.Combine(drive, "My Paratext Projects");
						if (Directory.Exists(possibleLocation))
							return possibleLocation;
					}
				}
				return null;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			Settings.Default.OpenProjectDlgFormSettings.InitializeForm(this);
			base.OnLoad(e);
			m_listExistingProjects.GridSettings = Settings.Default.OpenProjectDlgGridSettings;

			if (m_listExistingProjects.SelectedProject != null)
				m_btnOk.Enabled = true;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Settings.Default.OpenProjectDlgGridSettings = m_listExistingProjects.GridSettings;
			base.OnClosing(e);
		}

		private void m_linkSingleSFBook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new OpenFileDialog())
			{
				dlg.InitialDirectory = DefaultSfmDirectory;

				if (dlg.ShowDialog() == DialogResult.OK)
				{
					SelectedProject = dlg.FileName;
					var folder = Path.GetDirectoryName(SelectedProject);
					if (folder != null)
					{
						folder = Path.GetDirectoryName(folder);
						if (folder != null)
							Settings.Default.DefaultSfmDirectory = folder;
					}
					Type = ProjectType.StandardFormatBook;
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		private void m_linkSFFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new FolderBrowserDialog())
			{
				dlg.Reset();
				dlg.RootFolder = Environment.SpecialFolder.Desktop;
				dlg.Description = LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.SelectFolderOfSfFiles", "Select the folder containing the project's Standard Format files.");
				dlg.SelectedPath = DefaultSfmDirectory;
				dlg.ShowNewFolderButton = false;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					SelectedProject = dlg.SelectedPath;
					Type = ProjectType.StandardFormatFolder;
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		private void HandleSelectedProjectChanged(object sender, EventArgs e)
		{
			SelectedProject = m_listExistingProjects.SelectedProject;
			m_btnOk.Enabled = SelectedProject != null;
		}

		private void HandleExistingProjectsDoubleClick(object sender, EventArgs e)
		{
			if (m_btnOk.Enabled)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void HandleShowHiddenProjectsCheckedChanged(object sender, EventArgs e)
		{
			m_listExistingProjects.IncludeHiddenProjects = m_chkShowInactiveProjects.Checked;
		}

		private void HandleExistingProjectsListLoaded(object sender, EventArgs e)
		{
			m_chkShowInactiveProjects.Visible = m_listExistingProjects.HiddenProjectsExist;
		}
	}
}
