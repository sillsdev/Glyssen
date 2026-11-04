using System;
using System.IO;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine.Export;
using L10NSharp;
using SIL.IO;
using SIL.Reporting;
using static System.Environment.SpecialFolder;
using static System.IO.Path;
using static System.Windows.Forms.DialogResult;
using static Glyssen.Shared.Constants;

namespace Glyssen.Dialogs
{
	public class RolesForVoiceActorsSaveAsDialog : IDisposable
	{
		private readonly ProjectExporter m_projectExporter;
		private readonly SaveFileDialog m_saveFileDialog;
		private string m_defaultDirectory;

		public RolesForVoiceActorsSaveAsDialog(ProjectExporter projectExporter)
		{
			m_projectExporter = projectExporter;

			m_defaultDirectory = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrWhiteSpace(m_defaultDirectory))
			{
				m_defaultDirectory = Combine(Environment.GetFolderPath(MyDocuments), GlyssenInfo.Product);
				if (!Directory.Exists(m_defaultDirectory))
					Directory.CreateDirectory(m_defaultDirectory);
			}
			
			var rolesForVoiceActorsFileNameSuffix = LocalizationManager.GetString(
				"DialogBoxes.RolesForVoiceActorsSaveAsDlg.RolesForVoiceActorsFileNameDefaultSuffix",
				"Roles for Voice Actors");

			var defaultFileName = m_projectExporter.Project.PublicationName + " " +
				rolesForVoiceActorsFileNameSuffix + kExcelFileExtension;
			FileName = Combine(m_defaultDirectory, defaultFileName);
			m_saveFileDialog = new SaveFileDialog();
			m_saveFileDialog.Title = LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.SaveFileDialog.Title", "Choose File Location");
			m_saveFileDialog.OverwritePrompt = false;
			m_saveFileDialog.InitialDirectory = m_defaultDirectory;
			m_saveFileDialog.FileName = GetFileName(FileName);
			m_saveFileDialog.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
				LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.ExcelFileTypeLabel", "Excel files"), "*" + kExcelFileExtension,
				L10N.AllFilesLabel, "*.*");
			m_saveFileDialog.DefaultExt = kExcelFileExtension;
			m_saveFileDialog.OverwritePrompt = true;
		}

		public string FileName { get; private set; }

		public DialogResult ShowDialog(IWin32Window owner = null)
		{
			var dialogResult = owner != null ? m_saveFileDialog.ShowDialog(owner) : m_saveFileDialog.ShowDialog();

			if (dialogResult != OK)
				return dialogResult;

			FileName = m_saveFileDialog.FileName;

			try
			{
				m_projectExporter.ExportRolesForVoiceActors(FileName);
			}
			catch (Exception ex)
			{
				Analytics.ReportException(ex);
				ErrorReport.NotifyUserOfProblem(ex,
					string.Format(LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.CouldNotExport",
						"Could not save Roles for Voice Actors data to {0}", "{0} is a file name."), FileName));
				dialogResult = None;
			}

			string directoryName = GetDirectoryName(FileName);
			if (directoryName != null)
			{
				m_defaultDirectory = directoryName;
				Settings.Default.DefaultExportDirectory = m_defaultDirectory;
			}
			try
			{
				PathUtilities.OpenFileInApplication(FileName);
			}
			catch
			{
				// Oh well, we tried.
			}
			return dialogResult;
		}

		public void Dispose()
		{
			m_saveFileDialog.Dispose();
		}
	}
}
