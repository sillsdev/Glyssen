using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Bundle;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace ProtoScript.Controls
{
	public partial class ExistingProjectsList : UserControl
	{
		public event EventHandler SelectedProjectChanged;
		public event EventHandler ListLoaded;

		private string m_selectedProject;
		private string m_filterIcuLocale;
		private string m_filterBundleId;
		private bool m_includeHiddenProjects;
		private bool m_hiddenProjectsExist;
		private List<string> m_readOnlyProjects = new List<string>();

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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public void AddReadOnlyProject(Project project)
		{
			m_readOnlyProjects.Add(project.ProjectFilePath);
		}

		public bool IncludeHiddenProjects
		{
			get { return m_includeHiddenProjects; }
			set
			{
				m_includeHiddenProjects = value;
				if (IsHandleCreated)
					LoadExistingProjects();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HiddenProjectsExist
		{
			get { return m_hiddenProjectsExist; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridSettings GridSettings
		{
			get { return GridSettings.Create(m_list); }
			set
			{
				if (value != null)
					value.InitializeGrid(m_list);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadExistingProjects();
		}

		public void LoadExistingProjects()
		{
			m_list.SelectionChanged -= HandleSelectionChanged;
			m_list.CellValueChanged -= HandleCellValueChanged;
			m_list.CellValidating -= HandleCellValidating;

			m_list.Rows.Clear();
			m_hiddenProjectsExist = false;
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
					{
						m_hiddenProjectsExist = true;
						if (!IncludeHiddenProjects)
							continue;
					}

					if ((m_filterIcuLocale != null && m_filterIcuLocale != metadata.language.iso) ||
						(m_filterBundleId != null && m_filterBundleId != metadata.id))
						continue;

					int iRow = m_list.Rows.Add(new object[]
					{
						metadata.language,
						Path.GetFileName(recordingProjectFolder),
						Path.GetFileName(metadata.OriginalPathOfDblFile),
						metadata.HiddenByDefault,
						path
					});

					if (SelectedProject == path)
						m_list.Rows[iRow].Selected = true;
				}
			}

			m_list.Sort(m_list.SortedColumn ?? colLanguage,
				m_list.SortOrder == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
			m_list.SelectionChanged += HandleSelectionChanged;
			m_list.CellValueChanged += HandleCellValueChanged;
			m_list.CellValidating += HandleCellValidating;

			if (ListLoaded != null)
				ListLoaded(this, new EventArgs());
		}

		public void SetFilter(string icuLocale, string bundleId)
		{
			m_filterIcuLocale = icuLocale;
			m_filterBundleId = bundleId;
			if (IsHandleCreated)
				LoadExistingProjects();
		}

		private void HandleSelectionChanged(object sender, EventArgs e)
		{
			if (DesignMode || m_list.SelectedRows.Count < 1 || m_list.SelectedRows[0].Index < 0)
				SelectedProject = null;
			else
				SelectedProject = m_list.SelectedRows[0].Cells[colProjectPath.Index].Value as String;

			if (SelectedProjectChanged != null)
				SelectedProjectChanged(this, new EventArgs());
		}

		private void HandleDoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick(new EventArgs());
		}

		private void HandleCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != colInactive.Index)
				throw new InvalidOperationException("Unexpected change in read-only column!");

			var row = m_list.Rows[e.RowIndex];
			var inactive = (bool)row.Cells[e.ColumnIndex].Value;

			Project.SetHiddenFlag(SelectedProject, inactive);
		}

		private void HandleCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex != colInactive.Index)
				return;

			if ((bool)e.FormattedValue && m_readOnlyProjects.Contains(m_list.Rows[e.RowIndex].Cells[colProjectPath.Index].Value))
			{
				string title = LocalizationManager.GetString("Project.CannotRemoveCaption", "Cannot Remove from List");
				string msg = LocalizationManager.GetString("Project.CannotRemove", "Cannot remove the selected project because it is currently open");
				MessageBox.Show(msg, title);
				m_list.RefreshEdit();
				e.Cancel = true;
			}
		}
	}
}
