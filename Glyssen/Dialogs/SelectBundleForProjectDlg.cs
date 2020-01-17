using System;
using System.IO;
using System.Windows.Forms;
using Glyssen.Properties;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using SIL;
using SIL.DblBundle;
using static System.IO.Path;
using static System.String;

namespace Glyssen.Dialogs
{
	public class SelectBundleForProjectDlg : IDisposable
	{
		private OpenFileDialog m_fileDialog;
		private readonly string m_projectNameAssociatedWithBundle;
		private readonly string m_defaultFile;

		private SelectBundleForProjectDlg(string projectNameAssociatedWithBundle, string defaultFile)
		{
			m_defaultFile = defaultFile;
			m_projectNameAssociatedWithBundle = projectNameAssociatedWithBundle;
		}

		private string DefaultBundleDirectory
		{
			get => Settings.Default.DefaultBundleDirectory;
			set => Settings.Default.DefaultBundleDirectory = value;
		}

		private string Title => Format(Localizer.GetString("DialogBoxes.SelectProjectDlg.LocateTextReleaseBundleTitle", "Locate Text Release Bundle for project: {0}",
			"Parameter is the project name for which the user is to try to locate the original or updated Text Release Bundle."),
			m_projectNameAssociatedWithBundle);

		public DialogResult ShowDialog()
		{
			if (m_fileDialog == null)
				InitializeFileOpenDialog();
			var result = m_fileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileName = m_fileDialog.FileName;
				var dir = GetDirectoryName(FileName);
				if (!IsNullOrEmpty(dir))
					DefaultBundleDirectory = dir;
			}
			return result;
		}

		private void InitializeFileOpenDialog()
		{
			string defaultDir = DefaultBundleDirectory;
			if (m_defaultFile != null)
			{
				try
				{
					FileName = GetFileName(m_defaultFile);
					defaultDir = GetDirectoryName(m_defaultFile);
					if (IsNullOrEmpty(defaultDir) || !Directory.Exists(defaultDir))
						defaultDir = DefaultBundleDirectory;
				}
				catch
				{
				}
			}
			if (IsNullOrEmpty(defaultDir) || !Directory.Exists(defaultDir))
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			m_fileDialog = new OpenFileDialog
			{
				Title = Title,
				InitialDirectory = defaultDir,
				FileName = FileName,
				Filter = Format("{0} ({1})|{1}|{2} ({3})|{3}",
					Localizer.GetString("DialogBoxes.SelectProjectDlg.ResourceBundleFileTypeLabel", "Text Resource Bundle files"),
					"*" + DblBundleFileUtils.kDblBundleExtension,
					Localizer.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
					"*.*"),
				DefaultExt = DblBundleFileUtils.kDblBundleExtension
			};
		}

		public string FileName { get; private set; }

		public void Dispose()
		{
			m_fileDialog.Dispose();
		}

		public static bool TryGetBundleName(string projectName, string defaultFile, out string filename)
		{
			filename = null;
			using (var dlg = new SelectBundleForProjectDlg(projectName, defaultFile))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					filename = dlg.FileName;
					return true;
				}
			}
			return false;
		}

		public static bool GiveUserChanceToFindOriginalBundle(Project project)
		{
			if (TryGetBundleName(project.Name, project.OriginalBundlePath, out string bundlePath))
			{
				string invalidMessage = Localizer.GetString("File.InvalidBundleMsg", "The selected file is not a valid text release bundle. Would you like to try again?");
				string invalidCaption = Localizer.GetString("File.InvalidBundleMsg", "Invalid Bundle");
				if (GetExtension(bundlePath) == DblBundleFileUtils.kDblBundleExtension)
				{
					try
					{
						var bundle = new GlyssenBundle(bundlePath);
						if (bundle.Id != project.Id)
						{
							string message = Localizer.GetString("File.WrongBundleMsg", "The ID of the selected text release bundle does not match this project. Would you like to try again?");
							string caption = Localizer.GetString("File.WrongBundle", "Wrong Bundle");
							return ErrorMessageWithRetry(message, caption, project);
						}

						project.OriginalBundlePath = bundlePath;
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
			return MessageBox.Show(message, caption, MessageBoxButtons.YesNo) == DialogResult.Yes &&
				GiveUserChanceToFindOriginalBundle(project);
		}
	}
}
