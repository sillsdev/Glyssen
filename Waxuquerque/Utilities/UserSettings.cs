using System;

namespace Waxuquerque.Utilities
{
	public interface IUserSettings
	{
		string CurrentProject { get; set; }
		string DefaultExportDirectory { get; set; }
	}

	public class UserSettings
	{
		private static IUserSettings s_instance;

		public static IUserSettings Default
		{
			get
			{
				if (s_instance == null)
					throw new InvalidOperationException("Not Initialized. Set UserSettings.Default first.");
				return s_instance;
			}
			set => s_instance = value;
		}

		public static string CurrentProject
		{
			get => Default.CurrentProject;
			set => Default.CurrentProject = value;
		}

		public static string DefaultExportDirectory
		{
			get => Default.DefaultExportDirectory;
			set => Default.DefaultExportDirectory = value;
		}
	}
}
