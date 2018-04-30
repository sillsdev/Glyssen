using System;
using System.IO;
using System.Windows.Forms;
using Glyssen;
using GlyssenApp.Properties;
using Glyssen.Shared;
using L10NSharp;
using L10NSharp.UI;
using Waxuquerque;
using Analytics = Waxuquerque.Utilities.Analytics;
using ErrorReport = SIL.Reporting.ErrorReport;
using PathUtilities = SIL.IO.PathUtilities;

namespace GlyssenApp.UI.Dialogs
{
	public class RolesForVoiceActorsSaveAsDialog : IDisposable
	{
		private readonly ProjectExporter m_projectExporter;
		private readonly SaveFileDialog m_saveFileDialog;
		private string m_rolesForVoiceActorsFileNameSuffix;
		private string m_defaultDirectory;

		public RolesForVoiceActorsSaveAsDialog(ProjectExporter projectExporter)
		{
			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_projectExporter = projectExporter;

			m_defaultDirectory = Settings.Default.DefaultExportDirectory;
			if (string.IsNullOrWhiteSpace(m_defaultDirectory))
			{
				m_defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GlyssenInfo.kProduct);
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
				LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"), "*.*");
			m_saveFileDialog.DefaultExt = Constants.kExcelFileExtension;
			m_saveFileDialog.OverwritePrompt = true;
		}

		public string FileName { get; private set; }

		private void HandleStringsLocalized()
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
