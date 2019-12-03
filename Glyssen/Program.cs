using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext.Data;
using Paratext.Data.Users;
using PtxUtils;
using SIL;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.i18n;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;

namespace Glyssen
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

		public static IEnumerable<ErrorMessageInfo> CompatibleParatextProjectLoadErrors => ScrTextCollection.ErrorMessages.Where(e => e.ProjecType != ProjectType.Resource && !e.ProjecType.IsNoteType());
		private static List<Exception> _pendingExceptionsToReportToAnalytics = new List<Exception>();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
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

			UserInfo userInfo = new UserInfo { UILanguageCode = Settings.Default.UserInterfaceLanguage };
			bool sldrIsInitialized = false;

			Logger.Init();
			Trace.Listeners.Add(new LogFileTraceListener());

			Alert.Implementation = new AlertImpl(); // Do this before calling Initialize, just in case Initialize tries to display an alert.
			if (ParatextInfo.IsParatextInstalled)
			{
				string userName = null;

				try
				{
					ParatextData.Initialize();
					sldrIsInitialized = true;
					userName = RegistrationInfo.UserName;
					userInfo.Email = RegistrationInfo.EmailAddress;
					foreach (var errMsgInfo in CompatibleParatextProjectLoadErrors.Where(e => e.Reason == UnsupportedReason.Unspecified))
					{
						_pendingExceptionsToReportToAnalytics.Add(errMsgInfo.Exception);
					}
				}
				catch (Exception fatalEx) when (fatalEx is FileLoadException || fatalEx is TypeInitializationException)
				{
					ErrorReport.ReportFatalException(fatalEx);
				}
				catch (Exception ex)
				{
					_pendingExceptionsToReportToAnalytics.Add(ex);
				}

				if (userName != null)
				{
					var split = userName.LastIndexOf(" ", StringComparison.Ordinal);
					if (split > 0)
					{
						userInfo.FirstName = userName.Substring(0, split);
						userInfo.LastName = userName.Substring(split + 1);
					}
					else
					{
						userInfo.LastName = userName;

					}
				}
			}
			// ENHANCE (PG-63): Implement something like this if we decide to give the user the option of manually
			// specifying the location of Paratext data files if the program isn’t actually installed.
			//else
			//{
			//	RegistrationInfo.Implementation = new GlyssenAnonymousRegistrationInfo();

			//	if (!String.IsNullOrWhiteSpace(Settings.Default.UserSpecifiedParatext8ProjectsDir) &&
			//		Directory.Exists(Settings.Default.UserSpecifiedParatext8ProjectsDir))
			//	{
			//		try
			//		{
			//			ParatextData.Initialize(Settings.Default.UserSpecifiedParatext8ProjectsDir);
			//			sldrIsInitialized = true;
			//		}
			//		catch (Exception ex)
			//		{
			//			_pendingExceptionsToReportToAnalytics.Add(ex);
			//			Settings.Default.UserSpecifiedParatext8ProjectsDir = null;
			//		}
			//	}
			//}

#if DEBUG
			using (new Analytics("jBh7Qg4jw2nRFE8j8EY1FDipzin3RFIP", userInfo))
#else
			//default is to allow tracking if this isn't set
			string feedbackSetting = Environment.GetEnvironmentVariable("FEEDBACK")?.ToLower();
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting == "yes" || feedbackSetting == "true";

			using (new Analytics("WEyYj2BOnZAP9kplKmo2BDPvfyofbMZy", userInfo, allowTracking))
#endif
			{
				foreach (var exception in _pendingExceptionsToReportToAnalytics)
					Analytics.ReportException(exception);
				_pendingExceptionsToReportToAnalytics.Clear();

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

				SetUpLocalization();

				DataMigrator.UpgradeToCurrentDataFormatVersion();

				SampleProject.CreateSampleProjectIfNeeded();

				// The following not only gets the location of the settings file;
				// it also detects corruption and deletes it if needed so we don't crash.
				string userConfigSettingsPath = GetUserConfigFilePath();

				if ((Control.ModifierKeys & Keys.Shift) > 0 && !string.IsNullOrEmpty(userConfigSettingsPath))
					HandleDeleteUserSettings(userConfigSettingsPath);

				// This might also be needed if Glyssen and ParatextData use different versions of SIL.WritingSystems.dll
				if (!sldrIsInitialized)
					Sldr.Initialize();

				try
				{
					Application.Run(new MainForm(args));
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
				var confirmationString = Localizer.GetString("Program.ConfirmDeleteUserSettingsFile",
					"Do you want to delete your user settings? (This will clear your most-recently-used project, publishing settings, UI language settings, etc.  It will not affect your project data.)");

				if (DialogResult.Yes == MessageBox.Show(confirmationString, GlyssenInfo.kProduct, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
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

		public static ILocalizationManager PrimaryLocalizationManager { get; private set; }

		private static void SetUpLocalization()
		{
			Localizer.Default = new L10NSharpLocalizer();
			string installedStringFileFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication("localization");
			string targetTmxFilePath = Path.Combine(GlyssenInfo.kCompany, GlyssenInfo.kProduct);
			string desiredUiLangId = Settings.Default.UserInterfaceLanguage;

			PrimaryLocalizationManager = LocalizationManager.Create(TranslationMemory.Tmx, desiredUiLangId, GlyssenInfo.kApplicationId, Application.ProductName, Application.ProductVersion,
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

			var uiLanguage = Localizer.UILanguageId;
			LocalizationManager.Create(TranslationMemory.Tmx, uiLanguage, "Palaso", "Palaso", Application.ProductVersion,
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
