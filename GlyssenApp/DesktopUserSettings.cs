using Glyssen;
using GlyssenApp.Properties;

namespace GlyssenApp
{
	internal class DesktopUserSettings : IUserSettings
	{
		public string CurrentProject
		{
			get => Settings.Default.CurrentProject;
			set => Settings.Default.CurrentProject = value;
		}

		public string DefaultExportDirectory
		{
			get => Settings.Default.DefaultExportDirectory;
			set => Settings.Default.DefaultExportDirectory = value;
		}
	}
}