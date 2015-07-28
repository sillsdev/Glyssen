using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Glyssen.Properties;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace Glyssen.Dialogs
{
	public partial class OpenProjectDlg : Form
	{
		public enum ProjectType
		{
			ExistingProject,
			TextReleaseBundle,
			ParatextProject,
		}

		public OpenProjectDlg(Project currentProject)
		{
			InitializeComponent();

			if (Settings.Default.OpenProjectDlgFormSettings == null)
				Settings.Default.OpenProjectDlgFormSettings = FormSettings.Create(this);

			if (currentProject != null)
			{
				m_listExistingProjects.SelectedProject = currentProject.ProjectFilePath;
				SelectedProject = currentProject.ProjectFilePath;
				m_listExistingProjects.AddReadOnlyProject(currentProject);
			}
			else
				m_btnOk.Enabled = false;
		}

		[DefaultValue(ProjectType.ExistingProject)]
		public ProjectType Type { get; private set; }

		public string SelectedProject { get; private set; }

		private void m_linkTextReleaseBundle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new SelectProjectDlg())
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
