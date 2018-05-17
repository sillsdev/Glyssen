using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Glyssen.Properties;
using Glyssen.Shared;
using Waxuquerque;

namespace Glyssen.UI.Dialogs
{
	public partial class OpenProjectDlg : FormWithPersistedSettings
	{
		private bool m_gridSettingsChanged;

		public enum ProjectType
		{
			ExistingProject,
			TextReleaseBundle,
			ParatextProject,
		}

		public OpenProjectDlg(Project currentProject)
		{
			InitializeComponent();

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

		private void OpenProjectDlg_Load(object sender, EventArgs e)
		{
			TileFormLocation();
		}

		private void m_linkTextReleaseBundle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (var dlg = new SelectProjectDlg())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					SelectedProject = dlg.FileName;
					if (Path.GetExtension(SelectedProject) == Constants.kProjectFileExtension)
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
			base.OnLoad(e);
			if (Settings.Default.OpenProjectDlgGridSettings != null &&
				//Don't use settings based on old number of columns
				Settings.Default.OpenProjectDlgGridSettings.Columns.Length == m_listExistingProjects.GridSettings.Columns.Length)
			{
				m_listExistingProjects.GridSettings = Settings.Default.OpenProjectDlgGridSettings;
			}

			if (m_listExistingProjects.SelectedProject != null)
			{
				m_listExistingProjects.ScrollToSelected();
				m_btnOk.Enabled = true;
			}

			m_gridSettingsChanged = false;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (m_gridSettingsChanged)
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

		private void HandleProjectListSorted(object sender, EventArgs e)
		{
			m_gridSettingsChanged = true;
		}

		private void HandleColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			if (e.Column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
				return;

			m_gridSettingsChanged = true;
		}

		private void m_listExistingProjects_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
		{
			m_gridSettingsChanged = true;
		}
	}
}
