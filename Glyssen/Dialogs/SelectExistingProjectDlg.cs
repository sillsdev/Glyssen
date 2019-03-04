using System;
using System.IO;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Shared.Bundle;
using SIL.DblBundle;

namespace Glyssen.Dialogs
{
	public partial class SelectExistingProjectDlg : Form
	{
		private readonly Bundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage> m_bundle;

		public SelectExistingProjectDlg(Bundle<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage> bundle)
		{
			m_bundle = bundle;
			InitializeComponent();
			m_listExistingProjects.SetFilter(bundle.LanguageIso, bundle.Id);
		}

		public string SelectedProject { get; private set; }

		private void HandleSelectedProjectChanged(object sender, EventArgs e)
		{
			SelectedProject = m_listExistingProjects.SelectedProject;
			m_btnOk.Enabled = SelectedProject != null;
		}

		private void m_listExistingProjects_DoubleClick(object sender, EventArgs e)
		{
			if (m_btnOk.Enabled)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void m_linkCreateNewProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var defaultRecordingProjectName = Project.GetDefaultProjectFilePath(m_bundle);
			if (!File.Exists(defaultRecordingProjectName))
				SelectedProject = defaultRecordingProjectName;
			else
			{
				string fmt = defaultRecordingProjectName + " ({0})";
				int n = 1;
				do
				{
					SelectedProject = String.Format(fmt, n++);
				} while (File.Exists(SelectedProject));
			}
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
