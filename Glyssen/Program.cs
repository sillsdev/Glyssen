using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Utilities;
using GlyssenFileBasedPersistence;
using L10NSharp;
using L10NSharp.UI;
using Paratext.Data;
using Paratext.Data.Users;
using PtxUtils;
using PtxUtils.Progress;
using SIL.Extensions;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.i18n;
using SIL.Windows.Forms.Reporting;
using SIL.WritingSystems;
using static System.Char;
using static System.String;
using Resources = Glyssen.Properties.Resources;

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

			GlyssenInfo.Product = Application.ProductName;
			MessageModal.Default = new WinFormsMessageBox();
			GlyssenEngine.ErrorHandling.NonFatalErrorHandler.Default = new WinFormsErrorAnalytics();

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

			GlyssenVersificationTable.Initialize();
			ProgressUtils.Implementation = new ProgressUtilsImpl();
			Alert.Implementation = new AlertImpl(); // Do this before calling Initialize, just in case Initialize tries to display an alert.
			if (ParatextInfo.IsParatextInstalled)
			{
				string userName = null;

				try
				{
					ParatextData.Initialize();
					sldrIsInitialized = true;
					userName = RegistrationInfo.UserName;
					Logger.WriteEvent($"Paratext user name: {userName}");
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
			using (new DesktopAnalytics.Analytics("jBh7Qg4jw2nRFE8j8EY1FDipzin3RFIP", userInfo))
#else
			//default is to allow tracking if this isn't set
			string feedbackSetting = Environment.GetEnvironmentVariable("FEEDBACK")?.ToLower();
			var allowTracking = string.IsNullOrEmpty(feedbackSetting) || feedbackSetting == "yes" || feedbackSetting == "true";

			using (new DesktopAnalytics.Analytics("WEyYj2BOnZAP9kplKmo2BDPvfyofbMZy", userInfo, allowTracking))
#endif
			{
				foreach (var exception in _pendingExceptionsToReportToAnalytics)
					Analytics.ReportException(exception);
				_pendingExceptionsToReportToAnalytics.Clear();

				var oldPgBaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					GlyssenInfo.Company, kOldProductName);
				var baseDataFolder = ProjectRepository.ProjectsBaseFolder;
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

				bool HandleMissingBundleNeededForUpgrade(Project existingProject)
				{
					string msg = Format(LocalizationManager.GetString("Project.DataFormatMigrationBundleMissingMsg",
						"To upgrade the {0} project to the current {1} data format, the original text release bundle must be available, " +
						"but it is not in the original location ({2}).",
						"Param 0: Glyssen recording project name;" +
						"Param 1: \"Glyssen\" (product name); " +
						"Param 2: Path to the original location of the text release bundle"),
						existingProject.Name,
						GlyssenInfo.Product,
						existingProject.OriginalBundlePath) +
						LocateBundleYourselfQuestion;
					string caption = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
					if (DialogResult.Yes == MessageBox.Show(msg, caption, MessageBoxButtons.YesNo))
						return SelectBundleForProjectDlg.GiveUserChanceToFindOriginalBundle(existingProject);
					return false;
				}

				void HandleProjectPathChanged(string previousPath, string newPath)
				{
					if (Settings.Default.CurrentProject == previousPath)
						Settings.Default.CurrentProject = newPath;
				}

				bool ConfirmSafeAudioAudioReplacements(IReadOnlyList<Tuple<string, string>> safeReplacements)
				{
					string fmt;
					if (safeReplacements.Count == 1)
					{
						fmt = LocalizationManager.GetString("DataMigration.ConfirmReplacementOfAudioAudio",
							"Doing this will replace the existing project by the same name, which was originally created by {0}. " +
							"Since none of the blocks in the project to be overwritten have any user decisions recorded, this seems " +
							"to be safe, but since {0} failed to make a backup, you need to confirm this. If you choose not to confirm " +
							"this action, you can either clean up the problem project yourself or verify that is is safe and then restart " +
							"{0}. You will be asked about this each time you start the program as long as this problem remains unresolved.\r\n" +
							"Confirm overwriting?",
							"Param: \"Glyssen\" (product name); " +
							"This follows the \"AudioAudioProblemPreambleSingle\".");
					}
					else
					{
						fmt = LocalizationManager.GetString("DataMigration.ConfirmReplacementsOfAudioAudio",
							"Doing this will replace the existing projects by the same name, which were originally created by {0}. " +
							"Since none of the blocks in the projects to be overwritten have any user decisions recorded, this seems " +
							"to be safe, but since {0} failed to make a backup, you need to confirm this. If you choose not to confirm " +
							"this action, you can either clean up the problem projects yourself or verify that is is safe and then restart " +
							"{0}. You will be asked about this each time you start the program as long as these problems remains unresolved.\r\n" +
							"Confirm overwriting?",
							"Param: \"Glyssen\" (product name); " +
							"This follows the \"AudioAudioProblemPreambleMultiple\".");
					}

					var msg = DataMigrator.GetAudioAudioProblemPreamble(safeReplacements.Count) +
						Join(Environment.NewLine, safeReplacements.Select(r => r.Item1)) + Environment.NewLine + Environment.NewLine +
						Format(fmt, GlyssenInfo.Product);
					return DialogResult.Yes == MessageBox.Show(msg, GlyssenInfo.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				}

				Project.DefaultRecordingProjectNameSuffix = " " + LocalizationManager.GetString("Project.RecordingProjectDefaultSuffix", "Audio",
					"This must not contain any illegal file path characters!").Trim(FileSystemUtils.TrimCharacters);
				var persistenceImpl = new PersistenceImplementation();
				ProjectBase.Reader = ReferenceTextProxy.Reader = persistenceImpl;
				Project.Writer = persistenceImpl;
				var upgradeInfo = DataMigrator.UpgradeToCurrentDataFormatVersion(HandleMissingBundleNeededForUpgrade,
					HandleProjectPathChanged, ConfirmSafeAudioAudioReplacements);
				if (upgradeInfo != null)
				{
					Analytics.Track("DataVersionUpgrade", new Dictionary<string, string>
					{
						{ "old", upgradeInfo.Item1.ToString(CultureInfo.InvariantCulture) },
						{ "new", upgradeInfo.Item2.ToString(CultureInfo.InvariantCulture) }
					});
				}

				SampleProject.CreateSampleProjectIfNeeded();

				// The following not only gets the location of the settings file;
				// it also detects corruption and deletes it if needed so we don't crash.
				string userConfigSettingsPath = GetUserConfigFilePath();

				if ((Control.ModifierKeys & Keys.Shift) > 0 && !IsNullOrEmpty(userConfigSettingsPath))
					HandleDeleteUserSettings(userConfigSettingsPath);

				// This might also be needed if Glyssen and ParatextData use different versions of SIL.WritingSystems.dll
				if (!sldrIsInitialized)
					Sldr.Initialize();

				GeckoUtilities.InitializeGecko();

				try
				{
					Application.Run(new MainForm(persistenceImpl, args));
				}
				finally
				{
					Sldr.Cleanup();
				}
			}
		}

		public static string LocateBundleYourselfQuestion => Environment.NewLine + Environment.NewLine +
			LocalizationManager.GetString("Project.LocateBundleYourself", "Would you like to locate the text release bundle yourself?");

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

				if (DialogResult.Yes == MessageBox.Show(confirmationString, GlyssenInfo.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
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
			SIL.Localizer.Default = new L10NSharpLocalizer();
			string installedStringFileFolder = FileLocationUtilities.GetDirectoryDistributedWithApplication("localization");
			string relativeSettingPathForLocalizationFolder = Path.Combine(GlyssenInfo.Company, GlyssenInfo.Product);
			string desiredUiLangId = Settings.Default.UserInterfaceLanguage;
			var assembly = Assembly.GetEntryAssembly();
			var versionField = assembly?.GetType("GitVersionInformation")?.GetField("MajorMinorPatch");
			var version = versionField?.GetValue(null) as string ??
				Application.ProductVersion.Substring(0, Application.ProductVersion.IndexOf(c => !IsDigit(c) && c != '.'));

			// ENHANCE: Create a separate LM for GlyssenEngine, so we can generate a nuget package
			// with the localized strings (similar to what we do for libpalaso and chorus).
			PrimaryLocalizationManager = LocalizationManager.Create(TranslationMemory.XLiff, desiredUiLangId, GlyssenInfo.ApplicationId, Application.ProductName, version,
				installedStringFileFolder, relativeSettingPathForLocalizationFolder, Resources.glyssenIcon, IssuesEmailAddress,
				typeof(SIL.Localizer)
					.GetMethods(BindingFlags.Static | BindingFlags.Public)
					.Where(m => m.Name == "GetString"), "Glyssen");

			if (IsNullOrEmpty(desiredUiLangId))
				if (LocalizationManager.GetUILanguages(true).Count() > 1)
					using (var dlg = new LanguageChoosingSimpleDialog(Resources.glyssenIcon))
						if (DialogResult.OK == dlg.ShowDialog())
						{
							Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", dlg.SelectedLanguage }, { "initialStartup", "true" } });

							LocalizationManager.SetUILanguage(dlg.SelectedLanguage, true);
							Settings.Default.UserInterfaceLanguage = dlg.SelectedLanguage;
						}

			var uiLanguage = LocalizationManager.UILanguageId;
			LocalizationManager.Create(TranslationMemory.XLiff, uiLanguage, "Palaso", "Palaso", version,
				installedStringFileFolder, relativeSettingPathForLocalizationFolder, Resources.glyssenIcon, IssuesEmailAddress,
				typeof(SIL.Localizer)
					.GetMethods(BindingFlags.Static | BindingFlags.Public)
					.Where(m => m.Name == "GetString"),
				"SIL.Windows.Forms.*", "SIL.DblBundle");
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
			return Process.GetProcesses().Select(p => p.ProcessName.ToLowerInvariant()).Count(n => n.Contains("glyssen") && !n.Contains("installer"));
		}
	}
}
