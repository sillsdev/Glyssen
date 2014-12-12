using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Bundle;

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
		public OpenProjectDlg(bool welcome = false)
		{
			InitializeComponent();
			if (welcome)
			{
				ShowInTaskbar = true;
				Text = LocalizationManager.GetString("DialogBoxes.WelcomeDlg.Title", "Welcome to Protoscript Generator");
			}
		}

		protected override bool ShowFocusCues
		{
			get { return m_tableLayoutPanelExistingProject.Visible && base.ShowFocusCues; }
		}

		public ProjectType Type { get; private set; }

		public string SelectedProject { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
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

						m_listExistingProjects.Items.Add(metadata);
						m_existingProjectPaths.Add(path);
					}
				}
			}
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

		private void m_linkSingleSFBook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new OpenFileDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					SelectedProject = dlg.FileName;
					Type = ProjectType.StandardFormatBook;
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		private void m_listExistingProjects_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = m_listExistingProjects.SelectedIndex >= 0;
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
	}
}
