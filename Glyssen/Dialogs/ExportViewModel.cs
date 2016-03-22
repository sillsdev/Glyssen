using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using Glyssen.Rules;
using L10NSharp;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public class ExportViewModel
	{
		public event EventHandler<EventArgs> ScriptExported;

		internal Project Project { get; set; }

		private ProjectExporter m_exporter;
		private readonly string m_productName;
		private string m_customFileName;

		public ExportViewModel(Project project, string productName)
		{
			Project = project;
			m_productName = productName;
			SelectedFileType = ExportFileType.Excel;
		}

		internal ExportFileType SelectedFileType { get; set; }

		internal string RecordingScriptFileNameSuffix
		{
			get
			{
				return LocalizationManager.GetString("DialogBoxes.ExportDlg.RecordingScriptFileNameDefaultSuffix", "Recording Script");
			}
		}

		internal string DefaultDirectory
		{
			get
			{
				var defaultDirectory = Settings.Default.DefaultExportDirectory;
				if (string.IsNullOrWhiteSpace(defaultDirectory))
				{
					defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), m_productName);
					if (!Directory.Exists(defaultDirectory))
						Directory.CreateDirectory(defaultDirectory);
				}

				return defaultDirectory;
			}
			set { Settings.Default.DefaultExportDirectory = value; }
		}

		internal string FullFileName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_customFileName))
					return m_customFileName;

				var defaultFileName = Project.PublicationName + " " +
					RecordingScriptFileNameSuffix + ProjectExporter.GetFileExtension(SelectedFileType);

				return Path.Combine(DefaultDirectory, defaultFileName.Trim());
			}
			set
			{
				m_customFileName = value;

				// if the directory is not the stored default directory, make the new directory the default
				var dirName = Path.GetDirectoryName(m_customFileName);
				if (!DirectoryUtilities.AreDirectoriesEquivalent(dirName, DefaultDirectory))
					DefaultDirectory = dirName;
			}
		}

		internal string FileNameWithoutExtension
		{
			get { return Path.GetFileNameWithoutExtension(FullFileName); }
		}

		internal string ActorDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ActorDirectoryNameSuffix", "Voice Actors");
				return Path.Combine(DefaultDirectory, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal string BookDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.BookDirectoryNameSuffix", "Books");
				return Path.Combine(Path.GetDirectoryName(FullFileName), FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal ProjectExporter Exporter
		{
			get { return m_exporter ?? (m_exporter = new ProjectExporter(Project)); }
		}

		internal bool IsOkToExport()
		{
			EnsureGroupsAreInSynchWithCharactersInUse();

			var export = true;
			string dlgMessage = null;
			string dlgTitle = null;
			if (Exporter.IncludeVoiceActors)
			{
				if (!Project.IsVoiceActorAssignmentsComplete)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.NotAssignedMessage",
						"One or more character groups have no voice actor assigned. Are you sure you want to export an incomplete script?");
					dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?");
				}
				else if (!Project.EveryAssignedGroupHasACharacter)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.EmptyGroupMessage",
						"One or more character groups have no characters in them. Are you sure you want to export a script?");
					dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.Title", "Export Script?");
				}
			}
			else if (Project.ProjectAnalysis.UserPercentAssigned < 100d)
			{
				dlgMessage = string.Format(LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.CharacterAssignmentIncompleteMessage",
					"Character assignment is {0:N1}% complete. Are you sure you want to export a script?"), Project.ProjectAnalysis.UserPercentAssigned);
				dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?");
			}

			if (dlgMessage != null)
			{
				dlgMessage += Environment.NewLine +
							  LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.MessageNote",
								  "(Note: You can export the script again as many times as you want.)");
				export = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
			}

			return export;
		}

		internal bool ExportNow(string fullFileName, bool includeActorBreakdown, bool includeBookBreakdown, bool openForMe)
		{
			var exportedAtLeastOneFile = false;
			FullFileName = fullFileName;
			
			try
			{
				Exporter.GenerateFile(fullFileName, SelectedFileType);
				exportedAtLeastOneFile = true;

				// remember the location
				Project.Status.LastExportLocation = Path.GetDirectoryName(fullFileName);
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				ErrorReport.ReportNonFatalExceptionWithMessage(ex,
					string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExport",
					"Could not export data to {0}", "{0} is a file name."), fullFileName));
			}
			if (includeActorBreakdown)
			{
				try
				{
					Directory.CreateDirectory(ActorDirectory);
					Exporter.GenerateActorFiles(ActorDirectory, SelectedFileType);
					exportedAtLeastOneFile = true;
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.ReportNonFatalExceptionWithMessage(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportActors",
						"Could not export one or more voice actor script files to {0}", "{0} is a directory name."), ActorDirectory));
				}
			}
			if (includeBookBreakdown)
			{
				try
				{
					Directory.CreateDirectory(BookDirectory);
					Exporter.GenerateBookFiles(BookDirectory, SelectedFileType);
					exportedAtLeastOneFile = true;
				}
				catch (Exception ex)
				{
					Analytics.ReportException(ex);
					ErrorReport.ReportNonFatalExceptionWithMessage(ex,
						string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExportBooks",
						"Could not export one or more book script files to {0}", "{0} is a directory name."), BookDirectory));
				}
			}

			try
			{
				if (exportedAtLeastOneFile && DefaultDirectory != null && openForMe)
					PathUtilities.OpenDirectoryInExplorer(DefaultDirectory);
			}
			catch
			{
				// Oh well.
			}

			// notify anyone listening that something was exported
			if (ScriptExported != null)
				ScriptExported(this, new EventArgs());

			return true;
		}

		internal void EnsureGroupsAreInSynchWithCharactersInUse()
		{
			if (!Project.CharacterGroupList.CharacterGroups.Any())
				return;
			var adjuster = new CharacterGroupsAdjuster(Project);
			if (adjuster.GroupsAreNotInSynchWithData)
			{
				using (var progressDialog = new GenerateGroupsProgressDialog(Project, OnGenerateGroupsWorkerDoWork, false, true))
				{
					var generator = new CharacterGroupGenerator(Project, Project.GetKeyStrokesByCharacterId(), progressDialog.BackgroundWorker);
					progressDialog.ProgressState.Arguments = generator;

					if (progressDialog.ShowDialog() == DialogResult.OK && generator.GeneratedGroups != null)
						generator.ApplyGeneratedGroupsToProject();
					else
						adjuster.MakeMinimalAdjustments();

					Project.Save();
				}
			}
		}

		private void OnGenerateGroupsWorkerDoWork(object s, DoWorkEventArgs e)
		{
			var generator = (CharacterGroupGenerator)((ProgressState)e.Argument).Arguments;
			generator.GenerateCharacterGroups();
		}
	}
}
