using GlyssenApp.Properties;
using Waxuquerque;

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