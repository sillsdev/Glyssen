using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using L10NSharp;
using L10NSharp.UI;
using ProtoScript.Properties;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.Reporting;

namespace ProtoScript
{
	static class Program
	{
		public const string kCompany = "FCBH-SIL";
		public const string kProduct = "Protoscript Generator";
		public const string kApplicationId = "ProtoscriptGenerator";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//TODO set real keys - this key is just pointing to a test account on mixpanel - andrew-polk/protoscript-test-dev
#if DEBUG
			using (new Analytics("BhtwjdH3oj1n8nMjd53pPRireKxB3BQl", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }, true))
#else
			string feedbackSetting = Environment.GetEnvironmentVariable("FEEDBACK");

			//default is to allow tracking
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting.ToLower() == "yes" || feedbackSetting.ToLower() == "true";

			using (new Analytics("BhtwjdH3oj1n8nMjd53pPRireKxB3BQl", new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage }, allowTracking))
#endif
			{
				SetUpErrorHandling();

				Project.CreateSampleProjectIfNeeded();

				SetUpLocalization();

				// The following not only gets the location of the settings file;
				// it also detects corruption and deletes it if needed so we don't crash. 
				string userConfigSettingsPath = GetUserConfigFilePath();

				if ((Control.ModifierKeys & Keys.Shift) > 0 && !string.IsNullOrEmpty(userConfigSettingsPath))
					HandleDeleteUserSettings(userConfigSettingsPath);

				Application.Run(new MainForm());
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
				installedStringFileFolder, targetTmxFilePath, Resources.PgIcon, IssuesEmailAddress, "ProtoScript");

			if (string.IsNullOrEmpty(desiredUiLangId))
				if (LocalizationManager.GetUILanguages(true).Count() > 1)
					using (var dlg = new LanguageChoosingSimpleDialog(Resources.PgIcon))
						if (DialogResult.OK == dlg.ShowDialog())
						{
							Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", dlg.SelectedLanguage }, { "initialStartup", "true" } });
			
							LocalizationManager.SetUILanguage(dlg.SelectedLanguage, true);
							Settings.Default.UserInterfaceLanguage = dlg.SelectedLanguage;
						}

			// For now, do not set up localization for Palaso
			// TODO, should we?
		}

		/// <summary>
		/// The email address people should write to with issues
		/// </summary>
		public static string IssuesEmailAddress
		{
			// TODO (PG-26) get an email address generated which matches the application's real name
			get { return "protoscript_generator@sil.org"; }
		}
	}
}
