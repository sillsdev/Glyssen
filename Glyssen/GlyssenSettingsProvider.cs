using GlyssenEngine.Utilities;

namespace Glyssen
{
	internal static class GlyssenSettingsProvider
	{
		private class ExportSettingsProvider : IDefaultDirectoryProvider
		{
			public string DefaultDirectory
			{
				get => Properties.Settings.Default.DefaultExportDirectory;
				set => Properties.Settings.Default.DefaultExportDirectory = value;
			}
		}

		public static IDefaultDirectoryProvider ExportSettingsprovider => new ExportSettingsProvider();
	}
}
