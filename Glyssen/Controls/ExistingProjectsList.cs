using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Shared.Bundle;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Paratext;
using GlyssenFileBasedPersistence;
using L10NSharp;
using L10NSharp.UI;
using L10NSharp.XLiffUtils;
using Paratext.Data;
using SIL.DblBundle;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Windows.Forms.DblBundle;
using static System.String;

namespace Glyssen.Controls
{
	public partial class ExistingProjectsList : ProjectsListBase<GlyssenDblTextMetadata, GlyssenDblMetadataLanguage>
	{
		private string m_fmtParatextProjectSource;
		private readonly Dictionary<string, bool> m_unstartedParatextProjectStates = new Dictionary<string, bool>();
		private readonly Dictionary<string, string> m_paratextProjectIds = new Dictionary<string, string>();
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
			LocalizeItemDlg<XLiffDocument>.StringsLocalized += HandleStringsLocalized;
			HandleStringsLocalized();
		}

		private void HandleStringsLocalized()
		{
			m_fmtParatextProjectSource = LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.ParatextProjectLabel",
				"{0} project: {1}", "Param 0: \"Paratext\" (product name); Param 1: Paratext project short name (unique project identifier)");
		}

		protected override DataGridViewColumn InactiveColumn => colInactive;

		protected override DataGridViewColumn FillColumn => colBundleName;

		protected override IEnumerable<string> AllProjectFolders => ProjectRepository.AllRecordingProjectFolders;

		protected override string ProjectFileExtension => ProjectRepository.kProjectFileExtension;

		public Func<IEnumerable<ScrText>> GetParatextProjects { private get; set; }

		public string GetIdentifierForParatextProject =>
			m_paratextProjectIds.TryGetValue(SelectedProject, out var id) ? id : null;

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
						{
							var proxy = new ParatextProjectProxy(scrText);
							m_paratextProjectIds[proxy.Name] = scrText.Guid;
							yield return new Tuple<string, IProjectInfo>(proxy.Name, proxy);
						}
					}
				}
			}
		}

		protected override string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			var recordingProjName = ProjectRepository.GetRecordingProjectNameFromProjectFilePath(project.Item1);
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
				{
					// Note: Once we have a actual glyssen project started, we no longer worry about trying to
					// show the unambiguous Paratext project name in the rare case of two projects that have the
					// same (short) name. Presumably the language and/or the recording project name will be
					// different, and that will be sufficient for the user to distinguish between them. We
					// could test each one to see if the project exists but does not have a unique short name
					// in order to display the unambiguous name here, but it doesn't seem worth the trouble.
					yield return Format(m_fmtParatextProjectSource, ParatextScrTextWrapper.kParatextProgramName,
						metadata.ParatextProjectId);
				}
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
			// Note: there is a really tiny chance that someone may have made a Paratext project
			// inactive and then later added a second project with the same name on the machine. At
			// that point, the unique name that will be used for the project will change to include
			// the full name in parentheses, so it will not longer be found in the list of inactive
			// projects. In that case, the user would have to re-inactivate it and this list would
			// forever be left containing the old (short) name as well. This is so unlikely it's
			// barely even worth mentioning.
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
				try
				{
					// The file stream for the metadata needs to be closed BEFORE we call SetHiddenFlag because that
					// will need to open the stream with a write lock to save the modified metadata.
					GlyssenDblTextMetadata metadata;
					using (var stream = new FileStream(SelectedProject, FileMode.Open))
					{
						using (var reader = new StreamReader(stream))
						{
							metadata = GlyssenDblTextMetadata.Load(reader, SelectedProject);
						}
					}

					Project.SetHiddenFlag(metadata, ProjectRepository.GetProjectName(SelectedProject), inactive);
				}
				catch (Exception exception)
				{
					Analytics.ReportException(exception);
					ErrorReport.ReportNonFatalExceptionWithMessage(exception,
						Format(LocalizationManager.GetString("File.ProjectCouldNotBeModified", "Project could not be modified: {0}"), SelectedProject));
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
