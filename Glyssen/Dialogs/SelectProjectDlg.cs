using System.IO;
using System.Windows.Forms;
using Glyssen.Bundle;
using Glyssen.Properties;
using Glyssen.Shared;
using L10NSharp;
using SIL.DblBundle;
using SIL.Windows.Forms.DblBundle;

namespace Glyssen.Dialogs
{
	public class SelectProjectDlg : SelectProjectDlgBase
	{
		public SelectProjectDlg(bool allowProjectFiles = true, string defaultFile = null) : base(allowProjectFiles, defaultFile)
		{
		}

		protected override string DefaultBundleDirectory
		{
			get { return Settings.Default.DefaultBundleDirectory; }
			set { Settings.Default.DefaultBundleDirectory = value; }
		}

		protected override string ProjectFileExtension
		{
			get { return Constants.kProjectFileExtension; }
		}

		protected override string Title
		{
			get { return LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.Title", "Open Project"); }
		}

		protected override string ProductName
		{
			get { return GlyssenInfo.kProduct; }
		}

		public static bool GiveUserChanceToFindOriginalBundle(Project project)
		{
			using (var dlg = new SelectProjectDlg(false))
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string invalidMessage = LocalizationManager.GetString("File.InvalidBundleMsg", "The selected file is not a valid text release bundle. Would you like to try again?");
					string invalidCaption = LocalizationManager.GetString("File.InvalidBundleMsg", "Invalid Bundle");
					string bundlePath = dlg.FileName;
					if (Path.GetExtension(bundlePath) == DblBundleFileUtils.kDblBundleExtension)
					{
						try
						{
							var bundle = new GlyssenBundle(bundlePath);
							if (bundle.Id != project.Id)
							{
								string message = LocalizationManager.GetString("File.WrongBundleMsg", "The ID of the selected text release bundle does not match this project. Would you like to try again?");
								string caption = LocalizationManager.GetString("File.WrongBundle", "Wrong Bundle");
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
			if (DialogResult.Yes == MessageBox.Show(message, caption, MessageBoxButtons.YesNo))
				return GiveUserChanceToFindOriginalBundle(project);
			return false;
		}
	}
}
