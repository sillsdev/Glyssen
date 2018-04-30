using GlyssenApp.Properties;
using Waxuquerque;
using Waxuquerque.Utilities;

namespace GlyssenApp.Utilities
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