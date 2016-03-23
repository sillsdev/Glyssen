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
			set { Settings.Default.DefaultExportDirectory = value; }
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
				{
					if (includeActorBreakdown || includeBookBreakdown)
						PathUtilities.OpenDirectoryInExplorer(DefaultDirectory);
					else
						PathUtilities.OpenFileInApplication(fullFileName);
				}
			}
			catch
			{
				// Oh well.
			}

			return true;
		}
	}
}
