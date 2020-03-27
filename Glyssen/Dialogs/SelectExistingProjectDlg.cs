using System;
using System.IO;
using System.Windows.Forms;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenFileBasedPersistence;
using SIL.DblBundle;
using static System.String;

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
			SelectedProject = PersistenceImplementation.GetAvailableDefaultProjectFilePath(m_bundle);
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
