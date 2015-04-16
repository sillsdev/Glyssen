using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Properties;

namespace ProtoScript.Dialogs
{
	public class SelectProjectDialog : IDisposable
	{
		private const string kResourceBundleExtension = ".zip";
		private readonly OpenFileDialog m_fileDialog;

		public SelectProjectDialog()
		{
			var defaultDir = Settings.Default.DefaultBundleDirectory;
			if (string.IsNullOrEmpty(defaultDir))
			{
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			m_fileDialog = new OpenFileDialog
			{
				Title = LocalizationManager.GetString("SelectBundleDialog.Title", "Open Project"),
				InitialDirectory = defaultDir,
				Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}|{4} ({5})|{5}",
					LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ResourceBundleFileTypeLabel", "Text Resource Bundle files"),
					"*" + kResourceBundleExtension,
					string.Format(LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ProjectFilesLabel", "{0} Project Files", "{0} is the product name"), Program.kProduct),
					"*" + Project.kProjectFileExtension,
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
					"*.*"),
				DefaultExt = kResourceBundleExtension
			};
		}

		public DialogResult ShowDialog()
		{
			var result = m_fileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileName = m_fileDialog.FileName;
				var dir = Path.GetDirectoryName(FileName);
				if (!string.IsNullOrEmpty(dir))
					Settings.Default.DefaultBundleDirectory = dir;
			}
			return result;
		}

		public string FileName { get; private set; }

		public void Dispose()
		{
			m_fileDialog.Dispose();
		}
	}
}
