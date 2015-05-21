using System;
using System.IO;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Properties;
using L10NSharp;
using SIL.DblBundle;

namespace Glyssen.Dialogs
{
	public class SelectProjectDlg : IDisposable
	{
		private readonly OpenFileDialog m_fileDialog;

		public SelectProjectDlg(bool allowProjectFiles = true)
		{
			var defaultDir = Settings.Default.DefaultBundleDirectory;
			if (string.IsNullOrEmpty(defaultDir) || !Directory.Exists(defaultDir))
			{
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			string projectFiles = "";
			if (allowProjectFiles)
				projectFiles = string.Format("{0} ({1})|{1}|",
					string.Format(LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ProjectFilesLabel", "{0} Project Files", "{0} is the product name"), Program.kProduct),
					"*" + Project.kProjectFileExtension);
			m_fileDialog = new OpenFileDialog
			{
				Title = LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.Title", "Open Project"),
				InitialDirectory = defaultDir,
				Filter = string.Format("{0} ({1})|{1}|{2}{3} ({4})|{4}",
					LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ResourceBundleFileTypeLabel", "Text Resource Bundle files"),
					"*" + DblBundleFileUtils.kDblBundleExtension,
					projectFiles,
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
					"*.*"),
				DefaultExt = DblBundleFileUtils.kDblBundleExtension
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

		public static bool GiveUserChanceToFindOriginalBundle(Project project)
		{
			using (var dlg = new SelectProjectDlg(false))
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string invalidMessage = LocalizationManager.GetString("File.InvalidBundleMsg", "The selected file is not a valid text bundle. Would you like to try again?");
					string invalidCaption = LocalizationManager.GetString("File.InvalidBundleMsg", "Invalid Bundle");
					string bundlePath = dlg.FileName;
					if (Path.GetExtension(bundlePath) == DblBundleFileUtils.kDblBundleExtension)
					{
						try
						{
							var bundle = new GlyssenBundle(bundlePath);
							if (bundle.Id != project.Id)
							{
								string message = LocalizationManager.GetString("File.WrongBundleMsg", "The ID of the selected text bundle does not match this project. Would you like to try again?");
								string caption = LocalizationManager.GetString("File.WrongBundle", "Wrong Bundle");
								return ErrorMessageWithRetry(message, caption, project);
							}

							project.OriginalPathOfDblFile = bundlePath;
							return true;
						}
						catch
						{
							return ErrorMessageWithRetry(invalidMessage, invalidCaption, project);
						}
					}
					return ErrorMessageWithRetry(invalidMessage, invalidCaption, project);
				}
			return false;
		}

		private static bool ErrorMessageWithRetry(string message, string caption, Project project)
		{
			if (DialogResult.Yes == MessageBox.Show(message, caption, MessageBoxButtons.YesNo))
				return GiveUserChanceToFindOriginalBundle(project);
			return false;
		}
	}
}
