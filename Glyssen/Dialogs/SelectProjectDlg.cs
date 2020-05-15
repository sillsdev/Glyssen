using Glyssen.Properties;
using Glyssen.Shared;
using GlyssenFileBasedPersistence;
using L10NSharp;
using SIL.Windows.Forms.DblBundle;

namespace Glyssen.Dialogs
{
	public class SelectProjectDlg : SelectProjectDlgBase
	{
		protected override string DefaultBundleDirectory
		{
			get => Settings.Default.DefaultBundleDirectory;
			set => Settings.Default.DefaultBundleDirectory = value;
		}

		protected override string ProjectFileExtension => ProjectRepository.kProjectFileExtension;

		protected override string Title => LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.Title", "Open Project");

		protected override string ProductName => GlyssenInfo.Product;
	}
}
