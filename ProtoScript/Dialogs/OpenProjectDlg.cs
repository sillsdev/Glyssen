using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using Microsoft.Win32;
using ProtoScript.Bundle;
using ProtoScript.Properties;

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

		private readonly List<string> m_existingProjectPaths = new List<string>();
		private readonly Project m_currentProject;
		public OpenProjectDlg(Project currentProject)
		{
			m_currentProject = currentProject;
			InitializeComponent();
		}

		public ProjectType Type { get; private set; }

		public string SelectedProject { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadExistingProjects();
		}

		private void LoadExistingProjects()
		{
			m_listExistingProjects.SelectedIndex = -1;
			m_listExistingProjects.Items.Clear();
			m_existingProjectPaths.Clear();
			DblMetadata itemToSelect = null;
			foreach (var projectFolder in Directory.GetDirectories(Project.ProjectsBaseFolder))
			{
				foreach (var versionFolder in Directory.GetDirectories(projectFolder))
				{
					// ENHANCE: Deal with the possibility of multiple versions of a project.
					// Do we want to show all versions or just the most recent one?
					var path = Directory.GetFiles(versionFolder, "*" + Project.kProjectFileExtension).FirstOrDefault();
					if (path != null)
					{
						Exception exception;
						var metadata = DblMetadata.Load(path, out exception);
						if (exception != null)
							continue;
						if (metadata.HiddenByDefault)
							continue;

						m_listExistingProjects.Items.Add(metadata);
						m_existingProjectPaths.Add(path);

						if (m_currentProject != null && m_currentProject.Id == metadata.id)
							itemToSelect = metadata;
					}
				}
			}
			if (itemToSelect != null)
				m_listExistingProjects.SelectedItem = itemToSelect;
		}

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
				dlg.RootFolder = Environment.SpecialFolder.MyComputer;
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

		private void m_listExistingProjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_listExistingProjects.SelectedIndex >= 0;
			m_linkRemoveProject.Enabled = m_listExistingProjects.SelectedIndex >= 0;
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			Type = ProjectType.ExistingProject;
			SelectedProject = m_existingProjectPaths[m_listExistingProjects.SelectedIndex];
		}

		private void m_listExistingProjects_DoubleClick(object sender, EventArgs e)
		{
			Type = ProjectType.ExistingProject;
			SelectedProject = m_existingProjectPaths[m_listExistingProjects.SelectedIndex];
			DialogResult = DialogResult.OK;
			Close();
		}

		private void m_linkRemoveProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (m_listExistingProjects.SelectedIndex < 0)
				return;
			if (m_currentProject != null && m_currentProject.Id == ((DblMetadata)m_listExistingProjects.SelectedItem).id)
			{
				string title = LocalizationManager.GetString("Project.CannotRemoveCaption", "Cannot Remove from List");
				string msg = LocalizationManager.GetString("Project.CannotRemove", "Cannot remove the selected project because it is currently open");
				MessageBox.Show(msg, title);
				return;
			}
			Project.SetHiddenFlag(m_existingProjectPaths[m_listExistingProjects.SelectedIndex], true);
			LoadExistingProjects();
		}
	}
}
