using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using DesktopAnalytics;
using GlyssenApp.Properties;
using GlyssenApp.UI;
using Glyssen.Shared;
using GlyssenApp.UI.Dialogs;
using GlyssenApp.Utilities;
using L10NSharp;
using L10NSharp.UI;
using SIL;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.FileSystem;
using SIL.Windows.Forms.i18n;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;
using Waxuquerque;
using Waxuquerque.Character;
using Waxuquerque.Utilities;
using Analytics = Waxuquerque.Utilities.Analytics;
using ErrorReport = SIL.Reporting.ErrorReport;
using Logger = SIL.Reporting.Logger;
using PathUtilities = Waxuquerque.Utilities.PathUtilities;

namespace GlyssenApp
{
	static class Program
	{
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
		static void Main(string[] args)
		{
			IsRunning = true;

			MessageModal.Default = new WinFormsMessageModal();
			PathUtilities.Default = new DesktopPathUtilities();
			UserSettings.Default = new DesktopUserSettings();
			Analytics.Default = new SegmentAnalytics();

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
			using (new DesktopAnalytics.Analytics("jBh7Qg4jw2nRFE8j8EY1FDipzin3RFIP", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }))
#else
			string feedbackSetting = Environment.GetEnvironmentVariable("FEEDBACK");

			//default is to allow tracking
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting.ToLower() == "yes" || feedbackSetting.ToLower() == "true";

			using (new DesktopAnalytics.Analytics("WEyYj2BOnZAP9kplKmo2BDPvfyofbMZy", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }, allowTracking))
#endif
			{
				Logger.Init();
				Waxuquerque.Utilities.Logger.Default = new DesktopLogger();

				Project.HelpUserRecoverFromBadLdmlFile = HelpUserRecoverFromBadLdmlFile;

				var oldPgBaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					GlyssenInfo.kCompany, kOldProductName);
				var baseDataFolder = GlyssenInfo.BaseDataFolder;
				if (Directory.Exists(oldPgBaseFolder) && !Directory.Exists(baseDataFolder))
					Directory.Move(oldPgBaseFolder, baseDataFolder);

				if (!Directory.Exists(baseDataFolder))
				{
					// create the directory
					Directory.CreateDirectory(baseDataFolder);
				}

				// PG-433, 07 JAN 2016, PH: Set the permissions so everyone can read and write to this directory
				DirectoryUtilities.SetFullControl(baseDataFolder, false);

				DataMigrator.UpgradeToCurrentDataFormatVersion((label, path) => ConfirmRecycleDialog.ConfirmThenRecycle(label, path), out var warning);

				if (!string.IsNullOrWhiteSpace(warning))
					MessageBox.Show(warning, GlyssenInfo.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Warning);

				SampleProject.CreateSampleProjectIfNeeded();

				SetUpLocalization();
				LocalizeItemDlg.StringsLocalized += ControlCharacterVerseData.Singleton.HandleStringsLocalized;

				// The following not only gets the location of the settings file;
				// it also detects corruption and deletes it if needed so we don't crash.
				string userConfigSettingsPath = GetUserConfigFilePath();

				if ((Control.ModifierKeys & Keys.Shift) > 0 && !string.IsNullOrEmpty(userConfigSettingsPath))
					HandleDeleteUserSettings(userConfigSettingsPath);

				Sldr.Initialize();

				try
				{
					Application.Run(new MainForm(args, LocalizationManager));
				}
				finally
				{
					Sldr.Cleanup();
				}
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

				if (DialogResult.Yes == MessageBox.Show(confirmationString, GlyssenInfo.kProduct, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
					File.Delete(userConfigSettingsPath);
		}

		private static void SetUpErrorHandling()
		{
			ErrorReport.SetErrorReporter(new WinFormsErrorReporter());
			ErrorReport.EmailAddress = IssuesEmailAddress;
			ErrorReport.AddStandardProperties();
			Waxuquerque.Utilities.ErrorReport.Default = new DesktopErrorReport();

			ExceptionHandler.Init(new WinFormsExceptionHandler());
			ExceptionHandler.AddDelegate(ReportError);
		}

		private static void ReportError(object sender, CancelExceptionHandlingEventArgs e)
		{
			DesktopAnalytics.Analytics.ReportException(e.Exception);
		}

		private static LocalizationManager LocalizationManager { get; set; }

		private static void SetUpLocalization()
		{
			string installedStringFileFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication("localization");
			string targetTmxFilePath = Path.Combine(GlyssenInfo.kCompany, GlyssenInfo.kProduct);
			string desiredUiLangId = Settings.Default.UserInterfaceLanguage;

			LocalizationManager = LocalizationManager.Create(desiredUiLangId, GlyssenInfo.kApplicationId, Application.ProductName, Application.ProductVersion,
				installedStringFileFolder, targetTmxFilePath, Resources.glyssenIcon, IssuesEmailAddress, "Glyssen");

			if (string.IsNullOrEmpty(desiredUiLangId))
				if (LocalizationManager.GetUILanguages(true).Count() > 1)
					using (var dlg = new LanguageChoosingSimpleDialog(Resources.glyssenIcon))
						if (DialogResult.OK == dlg.ShowDialog())
						{
							DesktopAnalytics.Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", dlg.SelectedLanguage }, { "initialStartup", "true" } });

							LocalizationManager.SetUILanguage(dlg.SelectedLanguage, true);
							Settings.Default.UserInterfaceLanguage = dlg.SelectedLanguage;
						}

			var uiLanguage = LocalizationManager.UILanguageId;
			LocalizationManager.Create(uiLanguage, "Palaso", "Palaso", Application.ProductVersion,
				installedStringFileFolder, targetTmxFilePath, Resources.glyssenIcon, IssuesEmailAddress,
				"SIL.Windows.Forms.WritingSystems", "SIL.DblBundle", "SIL.Windows.Forms.DblBundle", "SIL.Windows.Forms.Miscellaneous");

			Localizer.Default = new L10NSharpLocalizer();
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

		public static HelpUserRecoverResult HelpUserRecoverFromBadLdmlFile(bool attemptToUseBackup, string projectName, string ldmlFilePath, XmlException e)
		{
			var msg1 = string.Format(LocalizationManager.GetString("Project.LdmlFileLoadError",
					"The writing system definition file for project {0} could not be read:\n{1}\nError: {2}",
					"Param 0: project name; Param 1: LDML filename; Param 2: XML Error message"),
				projectName, ldmlFilePath, e.Message);
			var msg2 = attemptToUseBackup
				? LocalizationManager.GetString("Project.UseBackupLdmlFile",
					"To use the automatically created backup (which might be out-of-date), click Retry.",
					"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
					"created backup file exists.")
				: LocalizationManager.GetString("Project.AdvancedUserLdmlRepairInstructions",
					"If you can replace it with a valid backup or know how to repair it yourself, do so and then click Retry.",
					"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
					"created backup file does not exist.");
			var msg3 = string.Format(LocalizationManager.GetString("Project.IgnoreToRepairLdmlFile",
				"Otherwise, click Ignore and {0} will repair the file for you. Some information might not be recoverable, " +
				"so check the quote system and font settings carefully.", "Param 0: \"Glyssen\""), GlyssenInfo.kProduct);
			var msg = msg1 + "\n\n" + msg2 + msg3;
			Logger.WriteError(msg, e);

			switch (MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
			{
				default: return HelpUserRecoverResult.Default;
				case DialogResult.Retry: return HelpUserRecoverResult.Retry;
				case DialogResult.Abort: return HelpUserRecoverResult.Abort;
			}
		}
	}
}
