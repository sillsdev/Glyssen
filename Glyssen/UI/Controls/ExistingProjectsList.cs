using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using SIL.DblBundle;
using SIL.Windows.Forms.DblBundle;
using Waxuquerque;
using Waxuquerque.Bundle;

namespace Glyssen.UI.Controls
{
	public partial class ExistingProjectsList : ProjectsListBase<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		public ExistingProjectsList()
		{
			InitializeComponent();
		}

		public void AddReadOnlyProject(Project project)
		{
			AddReadOnlyProject(project.ProjectFilePath);
		}

		protected override DataGridViewColumn InactiveColumn { get { return colInactive; } }

		protected override DataGridViewColumn FillColumn { get { return colBundleName; } }

		protected override IEnumerable<string> AllProjectFolders
		{
			get { return Project.AllRecordingProjectFolders; }
		}

		protected override string ProjectFileExtension
		{
			get { return Constants.kProjectFileExtension; }
		}

		protected override string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			return Path.GetFileName(Path.GetDirectoryName(project.Item1));
		}

		protected override IEnumerable<object> GetAdditionalRowData(IProjectInfo project)
		{
			var metadata = (GlyssenDblTextMetadata) project;
			yield return Path.GetFileName(metadata.OriginalPathBundlePath);
			if (metadata.LastModified.Year < 1900)
				yield return null;
			else
				yield return metadata.LastModified;
			yield return metadata.Inactive;
		}

		protected override bool IsInactive(IProjectInfo project)
		{
			return ((GlyssenDblTextMetadata)project).Inactive;
		}

		protected override void SetHiddenFlag(bool inactive)
		{
			Project.SetHiddenFlag(SelectedProject, inactive);
		}

		public void ScrollToSelected()
		{
			if (SelectedProject != null)
				m_list.FirstDisplayedScrollingRowIndex = m_list.SelectedCells[0].RowIndex;
		}
	}
}
