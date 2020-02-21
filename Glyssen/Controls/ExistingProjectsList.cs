using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.ErrorHandling;
using GlyssenEngine.Paratext;
using GlyssenEngine.Utilities;
using GlyssenFileBasedPersistence;
using L10NSharp;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using Paratext.Data;
using SIL;
using SIL.DblBundle;
using SIL.Extensions;
using SIL.Windows.Forms.DblBundle;
using static System.String;

namespace Glyssen.Controls
{
	public partial class ExistingProjectsList : ProjectsListBase<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		private string m_fmtParatextProjectSource;
		private readonly Dictionary<string, bool> m_unstartedParatextProjectStates = new Dictionary<string, bool>();
		private ApplicationMetadata m_glyssenMetadata = null;

		private ApplicationMetadata GlyssenMetadata
		{
			get
			{
				if (m_glyssenMetadata == null)
				{
					m_glyssenMetadata = ApplicationMetadata.Load(out Exception error);
					if (error != null)
						throw error;
				}
				return m_glyssenMetadata;
			}
		}

		public ExistingProjectsList()
		{
			InitializeComponent();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;
			HandleStringsLocalized();
		}

		public void AddReadOnlyProject(Project project)
		{
			AddReadOnlyProject(project.ProjectFilePath);
		}

		private void HandleStringsLocalized()
		{
			m_fmtParatextProjectSource = LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.ParatextProjectLabel",
				"{0} project: {1}", "Param 0: \"Paratext\" (product name); Param 1: Paratext project short name (unique project identifier)");
		}

		protected override DataGridViewColumn InactiveColumn => colInactive;

		protected override DataGridViewColumn FillColumn => colBundleName;

		protected override IEnumerable<string> AllProjectFolders => ProjectRepository.AllRecordingProjectFolders;

		protected override string ProjectFileExtension => PersistenceImplementation.kProjectFileExtension;

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
						if (!existingProjects.Contains(scrText.Settings.DBLId))
							yield return new Tuple<string, IProjectInfo>(scrText.Name, new ParatextProjectProxy(scrText));
					}
				}
			}
		}

		protected override string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			var recordingProjName = project.Item1.GetContainingFolderName();
			if (!IsNullOrEmpty(recordingProjName))
				return recordingProjName;
			if (!m_unstartedParatextProjectStates.ContainsKey(project.Item1))
				m_unstartedParatextProjectStates[project.Item1] = IsInactiveParatextProject(project.Item1); 
			return LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.NoRecordingProject",
				"(recording project not started)");
		}

		protected override IEnumerable<object> GetAdditionalRowData(IProjectInfo project)
		{
			var metadata = project as GlyssenDblTextMetadata;
			if (metadata == null)
			{
				yield return Format(m_fmtParatextProjectSource, ParatextScrTextWrapper.kParatextProgramName, project.Name);
				yield return null;
				yield return m_unstartedParatextProjectStates[project.Name];
			}
			else
			{
				if (!IsNullOrEmpty(metadata.OriginalReleaseBundlePath))
					yield return Path.GetFileName(metadata.OriginalReleaseBundlePath);
				else if (metadata.Id != SampleProject.kSample)
					yield return Format(m_fmtParatextProjectSource, ParatextScrTextWrapper.kParatextProgramName, metadata.ParatextProjectId);
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
			return (project as GlyssenDblTextMetadata)?.Inactive ?? IsInactiveParatextProject(project.Name);
		}

		private bool IsInactiveParatextProject(string paratextProjectName)
		{
			return GlyssenMetadata.InactiveUnstartedParatextProjects != null &&
				GlyssenMetadata.InactiveUnstartedParatextProjects.Contains(paratextProjectName, StringComparison.Ordinal);
		}

		protected override void SetHiddenFlag(bool inactive)
		{
			if (m_unstartedParatextProjectStates.ContainsKey(SelectedProject))
			{
				m_unstartedParatextProjectStates[SelectedProject] = inactive;
				GlyssenMetadata.InactiveUnstartedParatextProjects = m_unstartedParatextProjectStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();
				GlyssenMetadata.Save();
			}
			else
			{
				using (var reader = new StreamReader(new FileStream(SelectedProject, FileMode.Open)))
				{
					try
					{
						GlyssenDblTextMetadata.SetHiddenFlag(reader, SelectedProject, inactive);
					}
					catch (Exception exception)
					{
						NonFatalErrorHandler.ReportAndHandleException(exception,
							Format(Localizer.GetString("File.ProjectCouldNotBeModified", "Project could not be modified: {0}"), SelectedProject));
					}
				}
			}
		}

		public void ScrollToSelected()
		{
			if (SelectedProject != null)
				m_list.FirstDisplayedScrollingRowIndex = m_list.SelectedCells[0].RowIndex;
		}
	}
}
