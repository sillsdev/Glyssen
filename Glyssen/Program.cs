﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using L10NSharp;
using L10NSharp.UI;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;

namespace Glyssen
{
	static class Program
	{
		public const string kCompany = "FCBH-SIL";
		public const string kProduct = "Glyssen";
		public const string kApplicationId = "Glyssen";

		public const double kKeyStrokesPerHour = 6000;
		public const double kCameoCharacterEstimatedHoursLimit = 0.2;

		private const string kOldProductName = "Protoscript Generator";

		// From https://stackoverflow.com/questions/73515/how-to-tell-if-net-code-is-being-run-by-visual-studio-designer
		/// <summary>
		/// <para>If false, we can assume that something like the Designer or Designer Serializer is trying to run the code.</para>
		/// <para>Basically, this is an alternative to Control.DesignMode that works better.</para>
 		/// </summary>
		public static bool IsRunning { get; set; }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			IsRunning = true;

			if (GetRunningGlyssenProcessCount() > 1)
			{
				ErrorReport.NotifyUserOfProblem("There is another copy of Glyssen already running. This instance of Glyssen will now shut down.");
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//bring in settings from any previous version
			if (Settings.Default.NeedUpgrade)
			{
				//see http://stackoverflow.com/questions/3498561/net-applicationsettingsbase-should-i-call-upgrade-every-time-i-load
				Settings.Default.Upgrade();
				Settings.Default.Reload();
				Settings.Default.NeedUpgrade = false;
				Settings.Default.Save();
			}

			SetUpErrorHandling();

#if DEBUG
			using (new Analytics("jBh7Qg4jw2nRFE8j8EY1FDipzin3RFIP", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }))
#else
			string feedbackSetting = Environment.GetEnvironmentVariable("FEEDBACK");

			//default is to allow tracking
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting.ToLower() == "yes" || feedbackSetting.ToLower() == "true";

			using (new Analytics("WEyYj2BOnZAP9kplKmo2BDPvfyofbMZy", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }, allowTracking))
#endif
			{
				Logger.Init();

				var oldPgBaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					kCompany, kOldProductName);
				if (Directory.Exists(oldPgBaseFolder) && !Directory.Exists(BaseDataFolder))
					Directory.Move(oldPgBaseFolder, BaseDataFolder);

				if (!Directory.Exists(BaseDataFolder))
				{
					// create the directory
					Directory.CreateDirectory(BaseDataFolder);
				}

				// PG-433, 07 JAN 2016, PH: Set the permissions so everyone can read and write to this directory
				DirectoryUtilities.SetFullControl(BaseDataFolder, false);

				DataMigrator.UpgradeToCurrentDataFormatVersion();

				SampleProject.CreateSampleProjectIfNeeded();

				SetUpLocalization();

				// The following not only gets the location of the settings file;
				// it also detects corruption and deletes it if needed so we don't crash.
				string userConfigSettingsPath = GetUserConfigFilePath();

				if ((Control.ModifierKeys & Keys.Shift) > 0 && !string.IsNullOrEmpty(userConfigSettingsPath))
					HandleDeleteUserSettings(userConfigSettingsPath);

				Sldr.Initialize();

				try
				{
					Application.Run(new MainForm());
				}
				finally
				{
					Sldr.Cleanup();
				}
			}
		}

		public static string BaseDataFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					kCompany, kProduct);
			}
		}

		public static string GetUserConfigFilePath()
		{
			try
			{
				return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
			}
			catch (ConfigurationErrorsException e)
			{
				File.Delete(e.Filename);
				return e.Filename;
			}
		}

		private static void HandleDeleteUserSettings(string userConfigSettingsPath)
		{
				var confirmationString = LocalizationManager.GetString("Program.ConfirmDeleteUserSettingsFile",
					"Do you want to delete your user settings? (This will clear your most-recently-used project, publishing settings, UI language settings, etc.  It will not affect your project data.)");

				if (DialogResult.Yes == MessageBox.Show(confirmationString, kProduct, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
					File.Delete(userConfigSettingsPath);
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.SetErrorReporter(new WinFormsErrorReporter());
			ErrorReport.EmailAddress = IssuesEmailAddress;
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init(new WinFormsExceptionHandler());
			ExceptionHandler.AddDelegate(ReportError);
		}

		private static void ReportError(object sender, CancelExceptionHandlingEventArgs e)
		{
			Analytics.ReportException(e.Exception);
		}

		public static LocalizationManager LocalizationManager { get; private set; }

		private static void SetUpLocalization()
		{
			string installedStringFileFolder = FileLocator.GetDirectoryDistributedWithApplication("localization");
			string targetTmxFilePath = Path.Combine(kCompany, kProduct);
			string desiredUiLangId = Settings.Default.UserInterfaceLanguage;

			LocalizationManager = LocalizationManager.Create(desiredUiLangId, kApplicationId, Application.ProductName, Application.ProductVersion,
				installedStringFileFolder, targetTmxFilePath, Resources.glyssenIcon, IssuesEmailAddress, "Glyssen");

			if (string.IsNullOrEmpty(desiredUiLangId))
				if (LocalizationManager.GetUILanguages(true).Count() > 1)
					using (var dlg = new LanguageChoosingSimpleDialog(Resources.glyssenIcon))
						if (DialogResult.OK == dlg.ShowDialog())
						{
							Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", dlg.SelectedLanguage }, { "initialStartup", "true" } });

							LocalizationManager.SetUILanguage(dlg.SelectedLanguage, true);
							Settings.Default.UserInterfaceLanguage = dlg.SelectedLanguage;
						}

			var uiLanguage = LocalizationManager.UILanguageId;
			LocalizationManager.Create(uiLanguage, "Palaso", "Palaso", Application.ProductVersion,
				installedStringFileFolder, targetTmxFilePath, Resources.glyssenIcon, IssuesEmailAddress,
				"SIL.Windows.Forms.WritingSystems", "SIL.DblBundle", "SIL.Windows.Forms.DblBundle", "SIL.Windows.Forms.Miscellaneous");
		}

		/// <summary>
		/// The email address people should write to with issues
		/// </summary>
		public static string IssuesEmailAddress
		{
			get { return "glyssen-support_lsdev@sil.org"; }
		}

		/// <summary>
		/// Getting the count of running Glyssen instances.
		/// </summary>
		/// <returns>The number of running Glyssen instances</returns>
		public static int GetRunningGlyssenProcessCount()
		{
			return Process.GetProcesses().Count(p => p.ProcessName.ToLowerInvariant().Contains("glyssen"));
		}
	}
}
