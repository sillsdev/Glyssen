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

namespace Glyssen.Dialogs
{
	public class RolesForVoiceActorsSaveAsDialog : IDisposable, ILocalizable
	{
		private readonly ProjectExporter m_projectExporter;
		private readonly SaveFileDialog m_saveFileDialog;
		private string m_rolesForVoiceActorsFileNameSuffix;
		private string m_defaultDirectory;

		public RolesForVoiceActorsSaveAsDialog(ProjectExporter projectExporter)
		{
			HandleStringsLocalized();
			Program.RegisterLocalizable(this);

			m_projectExporter = projectExporter;

			m_defaultDirectory = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrWhiteSpace(m_defaultDirectory))
			{
				m_defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GlyssenInfo.Product);
				if (!Directory.Exists(m_defaultDirectory))
					Directory.CreateDirectory(m_defaultDirectory);
			}
			string defaultFileName = m_projectExporter.Project.PublicationName + " " +
				m_rolesForVoiceActorsFileNameSuffix + Constants.kExcelFileExtension;
			FileName = Path.Combine(m_defaultDirectory, defaultFileName);
			m_saveFileDialog = new SaveFileDialog();
			m_saveFileDialog.Title = LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.SaveFileDialog.Title", "Choose File Location");
			m_saveFileDialog.OverwritePrompt = false;
			m_saveFileDialog.InitialDirectory = m_defaultDirectory;
			m_saveFileDialog.FileName = Path.GetFileName(FileName);
			m_saveFileDialog.Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}",
				LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.ExcelFileTypeLabel", "Excel files"), "*" + Constants.kExcelFileExtension,
				L10N.AllFilesLabel, "*.*");
			m_saveFileDialog.DefaultExt = Constants.kExcelFileExtension;
			m_saveFileDialog.OverwritePrompt = true;
		}

		public string FileName { get; private set; }

		public void HandleStringsLocalized()
		{
			m_rolesForVoiceActorsFileNameSuffix =
				LocalizationManager.GetString("DialogBoxes.RolesForVoiceActorsSaveAsDlg.RolesForVoiceActorsFileNameDefaultSuffix", "Roles for Voice Actors");
		}

		public DialogResult ShowDialog(IWin32Window owner = null)
		{
			var dialogResult = owner != null ? m_saveFileDialog.ShowDialog(owner) : m_saveFileDialog.ShowDialog();

			if (dialogResult != DialogResult.OK)
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
				dialogResult = DialogResult.None;
			}

			string directoryName = Path.GetDirectoryName(FileName);
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
