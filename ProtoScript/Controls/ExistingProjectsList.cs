using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ProtoScript.Bundle;

namespace ProtoScript.Controls
{
	public partial class ExistingProjectsList : UserControl
	{
		public event EventHandler SelectedProjectChanged;

		private string m_selectedProject;
		private string m_filterIcuLocale;
		private string m_filterBundleId;

		public ExistingProjectsList()
		{
			InitializeComponent();
		}
		
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedProject
		{
			get { return m_selectedProject; }
			set
			{
				m_selectedProject = value;
				if (m_selectedProject == null)
					return;
				for (int iRow  = 0; iRow < m_list.RowCount; iRow++)
				{
					if (m_selectedProject.Equals(m_list.Rows[iRow].Cells[colProjectPath.Index].Value))
					{
						m_list.Rows[iRow].Selected = true;
						break;
					}
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadExistingProjects();
		}

		public void LoadExistingProjects()
		{
			m_list.SelectionChanged -= m_list_SelectionChanged;
			foreach (var recordingProjectFolder in Project.AllRecordingProjectFolders)
			{
				var path = Directory.GetFiles(recordingProjectFolder, "*" + Project.kProjectFileExtension).FirstOrDefault();
				if (path != null)
				{
					Exception exception;
					var metadata = DblMetadata.Load(path, out exception);
					if (exception != null)
						continue;
					if (metadata.HiddenByDefault)
						continue;

					if ((m_filterIcuLocale != null && m_filterIcuLocale != metadata.language.iso) ||
						(m_filterBundleId != null && m_filterBundleId != metadata.id))
						continue;

					int iRow = m_list.AddRow(new object[]
					{
						metadata.language,
						Path.GetFileName(recordingProjectFolder),
						Path.GetFileName(metadata.OriginalPathOfDblFile),
						path
					});

					if (SelectedProject == path)
						m_list.Rows[iRow].Selected = true;
				}
			}
			m_list.SelectionChanged += m_list_SelectionChanged;
		}

		public void SetFilter(string icuLocale, string bundleId)
		{
			m_filterIcuLocale = icuLocale;
			m_filterBundleId = bundleId;
			if (IsHandleCreated)
				LoadExistingProjects();
		}

		private void m_list_SelectionChanged(object sender, EventArgs e)
		{
			if (DesignMode || m_list.SelectedRows.Count < 1 || m_list.SelectedRows[0].Index < 0)
				SelectedProject = null;
			else
				SelectedProject = m_list.SelectedRows[0].Cells[colProjectPath.Index].Value as String;

			if (SelectedProjectChanged != null)
				SelectedProjectChanged(this, new EventArgs());
		}

		private void m_list_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick(new EventArgs());
		}
	}
}
