using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Paratext;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext.Data;
using SIL.DblBundle;
using SIL.Windows.Forms.DblBundle;

namespace Glyssen.Controls
{
	public partial class ExistingProjectsList : ProjectsListBase<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		private string m_fmtParatextProjectSource;

		public ExistingProjectsList()
		{
			InitializeComponent();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
			HandleStringsLocalized();
		}

		public void AddReadOnlyProject(Project project)
		{
			AddReadOnlyProject(project.ProjectFilePath);
		}

		private void HandleStringsLocalized()
		{
			m_fmtParatextProjectSource = LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.NoRecordingProject",
				"Paratext project: {0}");
		}

		protected override DataGridViewColumn InactiveColumn => colInactive;

		protected override DataGridViewColumn FillColumn => colBundleName;

		protected override IEnumerable<string> AllProjectFolders => Project.AllRecordingProjectFolders;

		protected override string ProjectFileExtension => Constants.kProjectFileExtension;

		public Func<IEnumerable<ScrText>> GetParatextProjects { private get; set; }

		protected override IEnumerable<Tuple<string, IProjectInfo>> Projects
		{
			get
			{
				var existingProjects = new List<string>();
				foreach (var project in base.Projects)
				{
					existingProjects.Add(project.Item2.Id);
					yield return project;
				}

				if (GetParatextProjects != null)
				{
					foreach (var scrText in GetParatextProjects())
					{
						if (!existingProjects.Contains(scrText.Name))
							yield return new Tuple<string, IProjectInfo>(scrText.Name, new ParatextProjectProxy(scrText));
					}
				}


			}
		}

		protected override string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			var recordingProjName = project.Item1.GetContainingFolderName();
			if (!String.IsNullOrEmpty(recordingProjName))
				return recordingProjName;
			return LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.NoRecordingProject",
				"(recording project not started)");
		}

		protected override IEnumerable<object> GetAdditionalRowData(IProjectInfo project)
		{
			var metadata = project as GlyssenDblTextMetadata;
			if (metadata == null)
			{
				yield return String.Format(m_fmtParatextProjectSource, project.Name);
				yield return null;
				yield return false; // REVIEW: Should we regard not-yet-started Paratext projects as "inactive"?
			}
			else
			{
				if (!String.IsNullOrEmpty(metadata.OriginalReleaseBundlePath))
					yield return Path.GetFileName(metadata.OriginalReleaseBundlePath);
				else if (metadata.Id != SampleProject.kSample)
					yield return String.Format(m_fmtParatextProjectSource, metadata.ParatextProjectId);
				else
					yield return null;
				if (metadata.LastModified.Year < 1900)
					yield return null;
				else
					yield return metadata.LastModified;
				yield return metadata.Inactive;
			}
		}

		protected override bool IsInactive(IProjectInfo project)
		{
			return (project as GlyssenDblTextMetadata)?.Inactive ?? false;
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
