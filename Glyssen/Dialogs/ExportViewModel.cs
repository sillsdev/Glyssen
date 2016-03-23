using System;
using System.Data;
using System.IO;
using DesktopAnalytics;
using Glyssen.Properties;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public class ExportViewModel
	{
		private Project Project { get { return m_project; } }

		private ProjectExporter m_exporter;
		private string m_customFileName;
		private readonly Project m_project;

		public ExportViewModel(Project project)
		{
			m_project = project;
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
					defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Program.kProduct);
					if (!Directory.Exists(defaultDirectory))
						Directory.CreateDirectory(defaultDirectory);
				}

				return defaultDirectory;
			}
		}

		internal string CurrentBaseFolder
		{
			get { return Path.GetDirectoryName(FullFileName); }
		}

		internal string FullFileName
		{
			get
			{
				if (!string.IsNullOrEmpty(m_customFileName))
					return m_customFileName;

				var defaultFileName = Project.Name + " " +
					RecordingScriptFileNameSuffix + ProjectExporter.GetFileExtension(SelectedFileType);

				return Path.Combine(DefaultDirectory, defaultFileName.Trim());
			}
			set
			{
				m_customFileName = value;
			}
		}

		private string FileNameWithoutExtension
		{
			get { return Path.GetFileNameWithoutExtension(FullFileName); }
		}

		internal string ActorDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.ActorDirectoryNameSuffix", "Voice Actors");
				return Path.Combine(CurrentBaseFolder, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal string BookDirectory
		{
			get
			{
				var dirSuffix = LocalizationManager.GetString("DialogBoxes.ExportDlg.BookDirectoryNameSuffix", "Books");
				return Path.Combine(CurrentBaseFolder, FileNameWithoutExtension + " " + dirSuffix);
			}
		}

		internal ProjectExporter Exporter
		{
			get { return m_exporter ?? (m_exporter = new ProjectExporter(Project)); }
		}

		internal bool ExportNow(bool includeActorBreakdown, bool includeBookBreakdown, bool openForMe)
		{
			var exportedAtLeastOneFile = false;
			
			try
			{
				Exporter.GenerateFile(FullFileName, SelectedFileType);
				exportedAtLeastOneFile = true;

				// remember the location (at least for this project and possible as new default)
				Project.Status.LastExportLocation = CurrentBaseFolder;
				if (!string.IsNullOrEmpty(m_customFileName))
				{
					// if the directory is not the stored default directory, make the new directory the default
					if (!DirectoryUtilities.AreDirectoriesEquivalent(Project.Status.LastExportLocation, DefaultDirectory))
						Settings.Default.DefaultExportDirectory = Project.Status.LastExportLocation;
				}

			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				ErrorReport.ReportNonFatalExceptionWithMessage(ex,
					string.Format(LocalizationManager.GetString("DialogBoxes.ExportDlg.CouldNotExport",
					"Could not export data to {0}", "{0} is a file name."), FullFileName));
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
				if (exportedAtLeastOneFile && Project.Status.LastExportLocation != null && openForMe)
				{
					if (includeActorBreakdown || includeBookBreakdown)
						PathUtilities.OpenDirectoryInExplorer(Project.Status.LastExportLocation);
					else
						PathUtilities.OpenFileInApplication(FullFileName);
				}
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

		internal DataTable GeneratePreviewTable()
		{
			string[] lines;
			var dt = new DataTable();

			using (var tempFile = TempFile.WithExtension(ProjectExporter.kTabDelimitedFileExtension))
			{
				Exporter.GenerateFile(tempFile.Path, ExportFileType.TabSeparated);
				lines = File.ReadAllLines(tempFile.Path);
			}

			if (lines.Length > 0)
			{
				// add columns
				var line = lines[0].Split('\t');
				foreach (var val in line)
				{
					dt.Columns.Add(val);
				}

				// add rows
				for (var i = 1; i < lines.Length; i++)
				{
					// ReSharper disable once CoVariantArrayConversion
					dt.Rows.Add(lines[i].Split('\t'));
				}
			}

			return dt;
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
