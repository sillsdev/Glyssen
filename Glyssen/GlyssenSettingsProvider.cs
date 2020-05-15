using GlyssenEngine.Export;

namespace Glyssen
{
	internal static class GlyssenSettingsProvider
	{
		private class GlyssenExportSettingsProvider : IDefaultDirectoryProvider
		{
			public string DefaultDirectory
			{
				get => Properties.Settings.Default.DefaultExportDirectory;
				set => Properties.Settings.Default.DefaultExportDirectory = value;
			}
		}

		public static IDefaultDirectoryProvider ExportSettingsProvider => new GlyssenExportSettingsProvider();
	}
}
