using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Glyssen.Dialogs;
using Glyssen.Export;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenCharacters;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Export;
using GlyssenEngine.Paratext;
using GlyssenEngine.Rules;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;
using GlyssenFileBasedPersistence;
using L10NSharp;
using SIL.DblBundle;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms;
using SIL.Windows.Forms.Miscellaneous;
using Ionic.Zip;
using NetSparkle;
using Paratext.Data;
using SIL.Scripture;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.ReleaseNotes;
using SIL.Windows.Forms.WritingSystems;
using static System.String;
using Analytics = DesktopAnalytics.Analytics;
using Resources = Glyssen.Properties.Resources;
using AssignCharacterViewModel = GlyssenEngine.ViewModels.AssignCharacterViewModel<System.Drawing.Font>;

namespace Glyssen
{
	public partial class MainForm : FormWithPersistedSettings, ILocalizable
	{
		private const string kShareFileExtension = ".glyssenshare";

		private static Sparkle UpdateChecker { get; set; }
		private readonly PersistenceImplementation m_persistenceImpl;
		private Project m_project;
		private ParatextScrTextWrapper m_paratextScrTextWrapperForRecentlyCreatedProject;
		private CastSizePlanningViewModel m_projectCastSizePlanningViewModel;
		private string m_percentAssignedFmt;
		private string m_actorsAssignedFmt;
		private string m_castSizeFmt;
		private readonly List<Tuple<Button, string>> m_buttonFormats = new List<Tuple<Button, string>>();
		private bool? m_isOkayToClearExistingRefBlocksThatCannotBeMigrated;
		private ReferenceText m_temporaryRefTextOverrideForExporting;

		public MainForm(PersistenceImplementation persistenceImpl, IReadOnlyList<string> args)
		{
			InitializeComponent();

			m_persistenceImpl = persistenceImpl;
			Project.FontRepository = new WinFormsFontRepositoryAdapter();

			Project.UpgradingProjectToNewParserVersion += UpgradingProjectToNewParserVersion;
			Project.GetBadLdmlRecoveryAction += GetBadLdmlFileRecoveryAction;
			
			Logger.WriteEvent($"Initial UI language: {Settings.Default.UserInterfaceLanguage}");

			m_toolStrip.Renderer = new NoBorderToolStripRenderer();

			SetupUiLanguageMenu();
			HandleStringsLocalized();
			Program.RegisterLocalizable(this);

			m_lastExportLocationLink.Text = Empty;

			// Did the user start Glyssen by double-clicking a share file?
			if (args.Count == 1 && args[0].ToLowerInvariant().EndsWith(kShareFileExtension))
			{
				ImportShare(args[0]);
			}
		}

		private BadLdmlRecoveryAction GetBadLdmlFileRecoveryAction(Project sender, string error, bool attemptToUseBackup)
		{
			var msg1 = Format(LocalizationManager.GetString("Project.LdmlFileLoadError",
					"The writing system definition file for project {0} could not be read:\n{1}\nError: {2}",
					"Param 0: project name; Param 1: LDML filename; Param 2: XML Error message"),
				sender.Name, m_persistenceImpl.GetLdmlFilePath(sender), error);
			var msg2 = Format(attemptToUseBackup
				? LocalizationManager.GetString("Project.UseBackupLdmlFile",
					"To use the automatically created backup (which might be out-of-date), click {0}.",
					"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
					"created backup file exists. Param is \"Retry\" button label.")
				: LocalizationManager.GetString("Project.AdvancedUserLdmlRepairInstructions",
					"If you can replace it with a valid backup or know how to repair it yourself, do so and then click {0}.",
					"Appears between \"Project.LdmlFileLoadError\" and \"Project.IgnoreToRepairLdmlFile\" when an automatically " +
					"created backup file does not exist. Param is \"Retry\" button label."), MessageBoxStrings.RetryButton);
			var msg3 = Format(LocalizationManager.GetString("Project.IgnoreToRepairLdmlFile",
					"Otherwise, click {0} and {1} will repair the file for you. Some information might not be recoverable, " +
					"so check the quote system and font settings carefully.",
					"Param 0: \"Ignore\" button label; " +
					"Param 1: Product name (e.g., \"Glyssen\")"),
				MessageBoxStrings.IgnoreButton, GlyssenInfo.Product);
			var msg = msg1 + "\n\n" + msg2 + msg3;
			Logger.WriteEvent(msg);

			switch (MessageBox.Show(msg, GlyssenInfo.Product, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
			{
				default: return BadLdmlRecoveryAction.Repair;
				case DialogResult.Retry: return BadLdmlRecoveryAction.Retry;
				case DialogResult.Abort: return BadLdmlRecoveryAction.Abort;
			}
		}

		private void UpgradingProjectToNewParserVersion(object sender, EventArgs e)
		{
			var existingProject = (Project)sender;
			var details = new Dictionary<string, string>
			{
				{"language", existingProject.LanguageIsoCode},
				{"ID", existingProject.Id},
				{"recordingProjectName", existingProject.Name},
				{"oldParserVersion", existingProject.ParserVersionWhenLastParsed.ToString(CultureInfo.InvariantCulture)},
				{"newParserVersion", Project.kParserVersion.ToString(CultureInfo.InvariantCulture)}
			};
			if (existingProject.IsLiveParatextProject)
				details.Add("ParatextProjectName", existingProject.ParatextProjectName);
			Analytics.Track("UpgradeParatextProject", details);
		}

		public static void SetChildFormLocation(Form childForm)
		{
			MainForm parentForm = childForm.Owner as MainForm;
			Debug.Assert(parentForm != null);

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (parentForm == null || childForm.WindowState == FormWindowState.Maximized)
				return;

			childForm.Location = new Point(parentForm.Location.X + kChildFormLocationX, parentForm.Location.Y + kChildFormLocationY);
		}

		private void SetProject(Project project)
		{
			if (m_project != null)
			{
				Logger.WriteEvent($"Closing project {m_project.Name}");

				m_project.ProjectStateChanged -= FinishSetProjectIfReady;
				m_project.CharacterGroupCollectionChanged -= UpdateDisplayOfCastSizePlan;
				m_project.CharacterGroupCollectionChanged -= ClearCastSizePlanningViewModel;
				m_project.CharacterStatisticsCleared -= ClearCastSizePlanningViewModel;
				m_project.AnalysisCompleted -= ProjectQuoteParseAnalysisCompleted;
			}

			m_projectCastSizePlanningViewModel = null;
			m_project = project;

			if (m_project != null)
			{
				Logger.WriteEvent($"Opening project {m_project.Name}");

				m_btnOpenProject.Enabled = false;
				m_project.ProjectStateChanged += FinishSetProjectIfReady;
				m_project.CharacterGroupCollectionChanged += UpdateDisplayOfCastSizePlan;
				m_project.CharacterGroupCollectionChanged += ClearCastSizePlanningViewModel;
				m_project.CharacterStatisticsCleared += ClearCastSizePlanningViewModel;
				m_project.AnalysisCompleted += ProjectQuoteParseAnalysisCompleted;
			}

			ResetUi();

			if (m_project != null && (m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0)
				return; //FinishSetProject will be called by the event handler

			FinishSetProject();
		}

		private void FinishSetProjectIfReady(object sender, EventArgs e)
		{
			if (m_project != null && (m_project.ProjectState & ProjectState.ReadyForUserInteraction) > 0)
				FinishSetProject();
			else
				UpdateProjectState();
		}

		private void FinishSetProject()
		{
			UpdateDisplayOfProjectInfo();

			if (m_project != null)
			{
				if (m_project.HasUnappliedSplits())
				{
					using (var dlg = new UnappliedSplitsDlg(m_project.Name, new FontProxy(m_project.FontFamily, m_project.FontSizeInPoints, m_project.RightToLeftScript),
						new UnappliedSplitsViewModel(m_project.IncludedBooks, m_project.RightToLeftScript)))
					{
						LogDialogDisplay(dlg);
						dlg.ShowDialog(this);
					}
				}

				Settings.Default.CurrentProject = m_persistenceImpl.GetProjectFilePath(m_project);
				Logger.WriteEvent($"CurrentProject set to {Settings.Default.CurrentProject}");
				Settings.Default.Save();

				Analytics.Track("LoadProject", new Dictionary<string, string>
				{
					{ "language", m_project.LanguageIsoCode },
					{ "ID", m_project.Id },
					{ "recordingProjectName", m_project.Name },
				});
			}
		}

		private void ClearCastSizePlanningViewModel(object sender, EventArgs e)
		{
			m_projectCastSizePlanningViewModel = null;
		}

		private void ProjectQuoteParseAnalysisCompleted(object sender, EventArgs e)
		{
			var project = (Project)sender;
			var analysis = project.ProjectAnalysis;
			Analytics.Track("ProjectAnalysis", new Dictionary<string, string>
			{
				{"language", project.LanguageIsoCode},
				{"ID", project.Id},
				{"recordingProjectName", Name},
				{"TotalBlocks", analysis.TotalBlocks.ToString(CultureInfo.InvariantCulture)},
				{"UserPercentAssigned", analysis.UserPercentAssigned.ToString(CultureInfo.InvariantCulture)},
				{"TotalPercentAssigned", analysis.TotalPercentAssigned.ToString(CultureInfo.InvariantCulture)},
				{"PercentUnknown", analysis.PercentUnknown.ToString(CultureInfo.InvariantCulture)}
			});
		}

		public void HandleStringsLocalized()
		{
			m_percentAssignedFmt = m_lblPercentAssigned.Text;
			m_actorsAssignedFmt = m_lblActorsAssigned.Text;
			m_castSizeFmt = m_lblCastSizePlan.Text;
			RememberButtonFormats();
			UpdateLocalizedText();
			m_project?.ProjectCharacterVerseData.HandleStringsLocalized();
			ControlCharacterVerseData.Singleton.HandleStringsLocalized();

			m_uiLanguageMenu.ToolTipText = LocalizationManager.GetString("MainForm.UILanguage", "User-interface Language");
		}

		private void RememberButtonFormats()
		{
			m_buttonFormats.Clear();
			var pos = m_tableLayoutPanel.GetCellPosition(m_btnOpenProject);
			for (var rowIndex = pos.Row; rowIndex < m_tableLayoutPanel.RowStyles.Count; rowIndex++)
			{
				if (m_tableLayoutPanel.GetControlFromPosition(pos.Column, rowIndex) is Button btn)
					m_buttonFormats.Add(new Tuple<Button, string>(btn, btn.Text));
			}
		}

		private void UpdateButtons(bool readyForUserInteraction)
		{
			m_btnOpenProject.Enabled = readyForUserInteraction;
			m_imgCheckOpen.Visible = true;
			m_btnSettings.Enabled = readyForUserInteraction && m_project.ProjectIsWritable;
			m_imgCheckSettings.Visible = m_btnSettings.Enabled && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.IsQuoteSystemReadyForParse;
			m_btnSelectBooks.Enabled = readyForUserInteraction && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.ProjectIsWritable;
			m_imgCheckBooks.Visible = m_btnSelectBooks.Enabled && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed && m_project.IncludedBooks.Any();
			m_btnIdentify.Enabled = readyForUserInteraction && m_imgCheckSettings.Visible && m_imgCheckBooks.Visible;
			m_imgCheckAssignCharacters.Visible = m_btnIdentify.Enabled && (int)(m_project.ProjectAnalysis.UserPercentAssigned) == 100;
			if (m_project.ReferenceText == null)
			{
				m_imgCheckAssignCharacters.Visible = true;
				m_imgCheckAssignCharacters.Image = Resources.Alert;
			}
			else if (m_btnIdentify.Enabled)
				m_imgCheckAssignCharacters.Image = m_project.ProjectAnalysis.AlignmentPercent == 100 ? Resources.green_check : Resources.yellow_check;
			m_btnExport.Enabled = readyForUserInteraction && m_btnIdentify.Enabled;

			m_btnAssignVoiceActors.Visible = Environment.GetEnvironmentVariable("Glyssen_ProtoscriptOnly", EnvironmentVariableTarget.User) == null;
			m_btnCastSizePlanning.Visible = m_btnAssignVoiceActors.Visible;

			m_btnCastSizePlanning.Enabled = m_btnCastSizePlanning.Visible && readyForUserInteraction && m_imgCheckAssignCharacters.Visible;

			// TODO: Determine when the Cast Size Planning task is done enough to move on to the next task.
			// Tom added the final portion of the logic to allow us to continue to access the Roles for Voice Actors dialog at least
			// until Phil finishes his Cast Size Planning work. Obviously, we need the part about having groups. The question is whether
			// we should also allow them to access Roles for Voice Actors if they entered actors but never clicked Generate Groups in the
			// Cast Size Planning dialog box... See more in the big red bullet point that I added to
			// https://docs.google.com/document/d/1cwPEYGnLJKK4TkP2MQy3t49k5r9rexCTWDb-jlCSDUo/edit?usp=sharing
			m_imgCastSizePlanning.Visible = m_btnCastSizePlanning.Visible && m_btnCastSizePlanning.Enabled &&
				(m_project.CharacterGroupList.CharacterGroups.Any() || m_project.VoiceActorList.ActiveActors.Any());

			m_btnAssignVoiceActors.Enabled = m_btnAssignVoiceActors.Visible && readyForUserInteraction && m_imgCastSizePlanning.Visible;
			m_imgCheckAssignActors.Visible = m_btnAssignVoiceActors.Visible && m_btnAssignVoiceActors.Enabled && m_project.IsVoiceActorScriptReady;
			m_lnkExit.Enabled = readyForUserInteraction;

			m_shareMenu.Enabled = readyForUserInteraction;
		}

		private void ResetUi()
		{
			m_btnSelectBooks.Enabled = false;
			m_btnSettings.Enabled = false;
			m_btnIdentify.Enabled = false;
			m_btnExport.Enabled = false;
			m_btnAssignVoiceActors.Enabled = false;
			m_btnCastSizePlanning.Enabled = false;
			m_imgCheckOpen.Visible = false;
			m_imgCheckSettings.Visible = false;
			m_imgCheckBooks.Visible = false;
			m_imgCheckAssignCharacters.Visible = false;
			m_imgCheckAssignActors.Visible = false;
			m_imgCastSizePlanning.Visible = false;

			m_lblProjectInfo.Text = Empty;
			m_lblSettingsInfo.Text = Empty;
			m_lblBookSelectionInfo.Text = Empty;
			m_lblPercentAssigned.Text = Empty;
			m_lblCastSizePlan.Text = Empty;
			m_lblActorsAssigned.Text = Empty;
			m_lastExportLocationLink.Text = Empty;

			m_lblFilesAreHere.Visible = false;

			m_shareMenu.Enabled = false;
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private DialogResult ShowModalDialogWithWaitCursor(Form dlg)
		{
			LogDialogDisplay(dlg);

			var origCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			var result = dlg.ShowDialog(this);
			Cursor = origCursor;
			return result;
		}

		internal static void LogDialogDisplay(Form dlg, object modelDetails = null)
		{
			Logger.WriteEvent($"Displaying dialog box: {dlg.Text}" + (modelDetails == null ? "" :
				$" for {modelDetails}"));
		}

		private void ShowOpenProjectDialog()
		{
			using (var dlg = new OpenProjectDlg(ProjectRepository.GetProjectFilePath(m_project)))
			{
				var result = ShowModalDialogWithWaitCursor(dlg);
				if (result != DialogResult.OK)
					return;

				try
				{
					switch (dlg.Type)
					{
						case OpenProjectDlg.ProjectType.ExistingProject:
							InitializeProgress();
							LoadProject(dlg.SelectedProject);
							break;
						case OpenProjectDlg.ProjectType.TextReleaseBundle:
							InitializeProgress();
							LoadBundle(dlg.SelectedProject);
							break;
						case OpenProjectDlg.ProjectType.ParatextProject:
							InitializeProgress();
							LoadParatextProject(dlg.SelectedProject, dlg.ParatextProjectId);
							break;
						default:
							MessageBox.Show(@"Sorry - not implemented yet");
							break;
					}
				}
				finally
				{
					CloseProgress();
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsNullOrEmpty(Settings.Default.CurrentProject) || !File.Exists(Settings.Default.CurrentProject))
				SetProject(null);
			else
			{
				try
				{
					LoadProject(Settings.Default.CurrentProject);
				}
				catch (Exception ex)
				{
					Analytics.Track("Project Load Failure", new Dictionary<string, string>
					{
						{WinFormsErrorAnalytics.kExceptionMsgKey, ex.Message},
						{"CurrentProjectPath", Settings.Default.CurrentProject},
					});
					try
					{
						// Attempt to clear this setting so that Glyssen won't be permanently hamstrung.
						Settings.Default.CurrentProject = null;
						Settings.Default.Save();
					}
					catch (Exception exceptionSavingSettings)
					{
						Logger.WriteError("Failed to clear CurrentProject setting. If problem persists, " +
							"Glyssen will not be able to restart successfully.", exceptionSavingSettings);
					}
					throw;
				}
			}
			UpdateChecker = new Sparkle(@"http://build.palaso.org/guestAuth/repository/download/Glyssen_GlyssenMasterPublish/.lastSuccessful/appcast.xml",
				Icon);
			UpdateChecker.DoLaunchAfterUpdate = false; // The installer already takes care of launching.
			// We don't want to do this until the main window is loaded because a) it's very easy for the user to overlook, and b)
			// more importantly, when the toast notifier closes, it can sometimes clobber an error message being displayed for the user.
			UpdateChecker.CheckOnFirstApplicationIdle();
		}

		private void InitializeProgress()
		{
			m_btnOpenProject.Enabled = false;
			ResetUi();
			m_tableLayoutPanel.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;
		}

		private void CloseProgress()
		{
			Cursor.Current = Cursors.Default;
			m_tableLayoutPanel.Enabled = true;
			UpdateDisplayOfProjectInfo();
		}

		private void LoadProject(string filePath, Action additionalActionAfterSettingProject = null)
		{
			Logger.WriteEvent($"Attempting to load project from {filePath}");
			bool loadedSuccessfully = LoadAndHandleApplicationExceptions(() =>
			{
				SetProject(Project.Load(InstantiateProjectFromMetadata(filePath), HandleMissingBundleNeededForProjectUpgrade, new WinformsParatextProjectLoadingAssistant(ParserUpgradeMessage, false)));
				additionalActionAfterSettingProject?.Invoke();
			});

			if (!loadedSuccessfully)
				SetProject(null);

			m_lastExportLocationLink.Text = m_project?.LastExportLocation;
			m_lblFilesAreHere.Visible = !IsNullOrEmpty(m_lastExportLocationLink.Text);
		}

		private Project InstantiateProjectFromMetadata(string projectFilePath)
		{
			// PG-433, 04 JAN 2015, PH: Let the user know if the project file is not writable
			var isWritable = !FileHelper.IsLocked(projectFilePath);
			if (!isWritable)
			{
				MessageModal.Show(LocalizationManager.GetString("Project.NotWritableMsg",
					"The project file is not writable. No changes will be saved."));
			}

			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(projectFilePath, out var exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				var message = Format(LocalizationManager.GetString("File.ProjectMetadataInvalid", "Project could not be loaded: {0}"), projectFilePath);
				ErrorReport.NotifyUserOfProblem(exception, message);
				return null;
			}

			Project project;
			try
			{
				project = new Project(metadata, ProjectRepository.GetRecordingProjectNameFromProjectFilePath(projectFilePath), true);
			}
			catch (ProjectNotFoundException e)
			{
				throw new ApplicationException(Format(LocalizationManager.GetString("Project.ParatextProjectNotFound",
						"Unable to access the {0} project {1}, which is needed to load the {2} project {3}.\r\n\r\nTechnical details:",
						"Param 0: \"Paratext\" (product name); " +
						"Param 1: Paratext project short name (unique project identifier); " +
						"Param 2: \"Glyssen\" (product name); " +
						"Param 3: Glyssen recording project name"),
					ParatextScrTextWrapper.kParatextProgramName,
					metadata.ParatextProjectId,
					GlyssenInfo.Product,
					metadata.Name), e);
			}

			project.ProjectIsWritable = isWritable;
			return project;
		}

		public static string ParserUpgradeMessage => LocalizationManager.GetString("Project.ParserVersionUpgraded", "The splitting engine has been upgraded.") + " ";

		private bool HandleMissingBundleNeededForProjectUpgrade(Project existingProject)
		{
			string msg = ParserUpgradeMessage + " " + Format(LocalizationManager.GetString("Project.ParserUpgradeBundleMissingMsg",
				"To make use of the new engine, the original text release bundle must be available, but it is not in the original location ({0})."),
				existingProject.OriginalBundlePath) +
				Program.LocateBundleYourselfQuestion;
			string caption = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
			if (DialogResult.Yes == MessageBox.Show(msg, caption, MessageBoxButtons.YesNo))
				return SelectBundleForProjectDlg.GiveUserChanceToFindOriginalBundle(existingProject);
			return false;
		}

		private void LoadBundle(string bundlePath)
		{
			Logger.WriteEvent($"Loading bundle {bundlePath}");

			GlyssenBundle bundle = null;

			if (!LoadAndHandleApplicationExceptions(() => { bundle = new GlyssenBundle(bundlePath); }))
			{
				SetProject(null);
				return;
			}

			string projFilePath;
			// See if we already have project(s) for this bundle and give the user the option of opening an existing project instead.
			var publicationFolder = ProjectRepository.GetPublicationFolderPath(bundle);
			if (Directory.Exists(publicationFolder) &&
				Directory.GetDirectories(publicationFolder).Any(f => Directory.GetFiles(f, "*" + ProjectRepository.kProjectFileExtension)
					.Any(filename => GlyssenDblTextMetadata.GetRevisionOrChangesetId(filename) == bundle.Metadata.RevisionOrChangesetId)))
			{
				using (var dlg = new SelectExistingProjectDlg(bundle))
				{
					LogDialogDisplay(dlg);
					dlg.ShowDialog(this);
					if (dlg.SelectedProject == null)
					{
						// User clicked the red X. Let's just pretend this whole nightmare never happened.
						return;
					}
					projFilePath = dlg.SelectedProject;
					if (File.Exists(projFilePath))
					{
						LoadProject(projFilePath);
						bundle.Dispose();
						return;
					}
				}
			}
			else
			{
				projFilePath = PersistenceImplementation.GetAvailableDefaultProjectFilePath(bundle, bundle.Metadata.Revision);
			}

			var recordingProjectName = ProjectRepository.GetRecordingProjectNameFromProjectFilePath(projFilePath);

			try
			{
				SetProject(new Project(bundle, recordingProjectName));
			}
			catch (InvalidVersificationLineException ex)
			{
				var error = ex.Message;
				int i = error.IndexOf("\n", StringComparison.Ordinal);
				if (i > 0)
					error = error.Substring(0, i);
				var msg = Format(LocalizationManager.GetString("Project.InvalidVersificationFile",
					"Invalid versification file in text release bundle. Unable to create project.\r\n" +
					"Text release Bundle: {0}\r\n" +
					"Versification file: {1}\r\n" +
					"Error: {2}"),
					bundlePath, DblBundleFileUtils.kVersificationFileName, error);
				Logger.WriteError(msg, ex);
				MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				SetProject(null);
			}

			bundle.Dispose();
		}

		private void LoadParatextProject(string paratextProjName, string paratextProjectId)
		{
			Logger.WriteEvent($"Loading {ParatextScrTextWrapper.kParatextProgramName} project {paratextProjName} (id:{paratextProjectId})");

			ParatextScrTextWrapper paratextProject = null;

			if (!LoadAndHandleApplicationExceptions(() =>
			{
				var scrText = ScrTextCollection.FindById(HexId.FromStrSafe(paratextProjectId), paratextProjName);
				paratextProject = new ParatextScrTextWrapper(scrText);
			}))
			{
				SetProject(null);
				return;
			}

			var optionalObserverInfo = paratextProject.UserCanEditProject ? Empty :
				Format(LocalizationManager.GetString("Project.NonEditingRole", "(You do not seem to have editing privileges for this {0} project.)",
					"Param: \"Paratext\" (product name)"), ParatextScrTextWrapper.kParatextProgramName) +
				Environment.NewLine;

			if (!paratextProject.HasQuotationRulesSet)
			{
				var msg = Format(LocalizationManager.GetString("Project.ParatextQuotationRulesNotDefined",
						"You are attempting to create a {0} project for {1} project {2}, which does not have its Quotation " +
						"Rules defined." +
						"\r\n{3}" +
						"The Quotation Rules are not only needed to run the {4} check, {0} also uses them to look for " +
						"speaking parts in the Scripture data. If you are not able to get those rules defined in {1}, you will " +
						"need to set the Quote Mark Settings manually in this {0} project.",
						"Param 0: \"Glyssen\" (product name); " +
						"Param 1: \"Paratext\" (product name); " +
						"Param 2: Paratext project short name (unique project identifier); " +
						"Param 3: Optional line indicating that user is an observer on the Paratext project; " +
						"Param 4: Name of the Paratext \"Quotations\" check"),
					GlyssenInfo.Product,
					ParatextScrTextWrapper.kParatextProgramName,
					paratextProjName,
					optionalObserverInfo,
					ParatextProjectBookInfo.LocalizedCheckName(ParatextScrTextWrapper.kQuotationCheckId));
				var result = MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
				if (result == DialogResult.Cancel)
				{
					Logger.WriteEvent($"User cancelled project creation because {ParatextScrTextWrapper.kParatextProgramName} " +
						"Quotation Rules were not defined.");
					SetProject(null);
					return;
				}
				Logger.WriteEvent($"User proceeding with project creation although {ParatextScrTextWrapper.kParatextProgramName} " +
					"Quotation Rules were not defined.");
				paratextProject.IgnoreQuotationsProblems();
			}

			// If the Paratext project contained no supported books at all, we already threw an appropriate exception above.
			// Now we check for the case where the project has supported books but, because of failing checks, none of them
			// was included by default.
			if (!paratextProject.HasBooksWithoutProblems)
			{
				var msg = Format(LocalizationManager.GetString("Project.NoBooksPassedChecks",
					"{0} is not reporting a current successful status for any book in the {1} project for the basic checks " +
					"that {2} usually requires to pass:" +
					"\r\n   {3}\r\n{4}\r\n" +
					"Do you want to proceed with creating a new {2} project for it anyway?",
					"Param 0: \"Paratext\" (product name); " +
					"Param 1: Paratext project short name (unique project identifier); " +
					"Param 2: \"Glyssen\" (product name); " +
					"Param 3: List of Paratext check names; " +
					"Param 4: Optional line indicating that user is an observer on the Paratext project"),
					ParatextScrTextWrapper.kParatextProgramName,
					paratextProjName,
					GlyssenInfo.Product,
					paratextProject.RequiredCheckNames,
					optionalObserverInfo);
				var result = MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (result == DialogResult.No)
				{
					Logger.WriteEvent("User cancelled project creation because no books passed recommended checks.");
					SetProject(null);
					return;
				}
				Logger.WriteEvent("User proceeding with project creation although no books passed recommended checks.");
			}

			var projFilePath = PersistenceImplementation.GetDefaultProjectFilePath(paratextProject);
			if (File.Exists(projFilePath))
				throw new Exception($"User should not have been able to select a Paratext project to create a new Glyssen project when that project already exists: {projFilePath}");

			var glyssenMetadata = ApplicationMetadata.Load(out Exception error);
			if (glyssenMetadata?.InactiveUnstartedParatextProjects != null)
			{
				var list = new List<string>(glyssenMetadata.InactiveUnstartedParatextProjects);
				if (list.Contains(paratextProjName))
				{
					list.Remove(paratextProjName);
					glyssenMetadata.InactiveUnstartedParatextProjects = list.Any() ? list.ToArray() : null;
					glyssenMetadata.Save();
				}
			}

			SetProject(new Project(paratextProject));
			m_paratextScrTextWrapperForRecentlyCreatedProject = paratextProject;
		}

		private bool LoadAndHandleApplicationExceptions(Action loadCommand)
		{
			try
			{
				loadCommand();
			}
			catch (ApplicationException ex)
			{
				Logger.WriteError(ex);
				var bldr = new StringBuilder(ex.Message);
				if (ex.InnerException != null)
				{
					bldr.Append(Environment.NewLine);
					bldr.Append(ex.InnerException);
				}

				MessageBox.Show(this, bldr.ToString(), ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				UpdateDisplayOfProjectInfo();
				return false;
			}
			return true;
		}

		private void UpdateDisplayOfProjectInfo()
		{
			UpdateProjectState();
			UpdateLocalizedText();
		}

		private void UpdateLocalizedText()
		{
			m_lblProjectInfo.Text = m_project != null ? m_project.ProjectSummary : Empty;

			if (m_project != null && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed)
				m_lblSettingsInfo.Text = m_project.IsQuoteSystemReadyForParse ? m_project.SettingsSummary : LocalizationManager.GetString("MainForm.QuoteMarksNeedReview", "Quote marks need to be reviewed");
			else
				m_lblSettingsInfo.Text = Empty;

			m_lblBookSelectionInfo.Text = m_project != null && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed ? m_project.BookSelectionSummary : Empty;

			UpdateDisplayOfPercentOfCharactersAssigned();
			UpdateDisplayOfCastSizePlan(null, null);
			UpdateDisplayOfActorsAssigned();

			// insert button numbers
			var buttonNumber = 1;
			foreach (var buttonFmtInfo in m_buttonFormats.Where(b => !b.Item1.IsDisposed))
			{
				var btn = buttonFmtInfo.Item1;
				btn.Text = Format(buttonFmtInfo.Item2, buttonNumber++);
			}
		}

		private void UpdateDisplayOfPercentOfCharactersAssigned()
		{
			if (!m_btnIdentify.Enabled || m_project == null)
			{
				m_lblPercentAssigned.Text = Empty;
				return;
			}

			double percentAssigned = 0;
			double percentAligned = 0;
			if (m_project.ProjectAnalysis != null)
			{
				percentAssigned = m_project.ProjectAnalysis.UserPercentAssigned;
				percentAligned = m_project.ProjectAnalysis.AlignmentPercent;
			}
			if (m_project.ReferenceText == null)
				m_lblPercentAssigned.Text = LocalizationManager.GetString("MainForm.ReferenceTextUnavailable", "Reference text unavailable");
			else
			{
				m_lblPercentAssigned.Text = percentAssigned > 0 ? Format(m_percentAssignedFmt, MathUtilities.FormattedPercent(percentAssigned, 1, 2),
					MathUtilities.FormattedPercent(percentAligned, 1, 2)) : Empty;
			}
		}

		private void UpdateDisplayOfCastSizePlan(object sender, EventArgs e)
		{
			if (!m_btnCastSizePlanning.Visible || !m_btnCastSizePlanning.Enabled || m_project == null ||
				!m_project.CharacterGroupList.CharacterGroups.Any())
			{
				m_lblCastSizePlan.Text = Empty;
				return;
			}

			var castSize = m_project.CharacterGroupList.CharacterGroups.Count;
			var narratorCount = m_project.CharacterGroupList.CharacterGroups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator);
			string format = (narratorCount > 1) ? m_castSizeFmt :
				LocalizationManager.GetString("MainForm.CastSizePlanSingleNarrator", "Cast size is {0}, including 1 narrator",
				"{0} is an expression indicating the total cast size");

			var newValue = Format(format, castSize, narratorCount);
			if (newValue != m_lblCastSizePlan.Text)
			{
				Logger.WriteEvent($"New Cast size plan info: {newValue}");
				m_lblCastSizePlan.Text = Format(format, castSize, narratorCount);
			}
		}

		private void UpdateDisplayOfActorsAssigned()
		{
			if (!m_btnAssignVoiceActors.Visible || !m_btnAssignVoiceActors.Enabled || m_project == null)
			{
				m_lblActorsAssigned.Text = Empty;
				return;
			}

			int actors = m_project.VoiceActorList.ActiveActors.Count();
			if (actors == 0)
			{
				m_lblActorsAssigned.Text = Empty;
				return;
			}
			int assigned = m_project.CharacterGroupList.CountVoiceActorsAssigned();
			string format = (actors > 1) ? Format(m_actorsAssignedFmt, actors, "{0}") :
				LocalizationManager.GetString("MainForm.ActorsAssignedSingle", "1 voice actor identified, {0}",
				"{0} is an expression indicating the number of assigned actors");

			string assignedParameter;
			switch (assigned)
			{
				case 0:
					assignedParameter = LocalizationManager.GetString("MainForm.NoneAssigned", "0 assigned",
						"This string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle");
					break;
				case 1:
					assignedParameter = LocalizationManager.GetString("MainForm.OneAssigned", "1 assigned",
						"This string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle");
					break;
				default:
					assignedParameter = Format(LocalizationManager.GetString("MainForm.MoreThanOneAssigned", "{0} assigned",
						"{0} is the number of actors assigned. The resulting string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle"),
						assigned);
					break;
			}

			var newValue = Format(format, assignedParameter);
			if (newValue != m_lblActorsAssigned.Text)
			{
				Logger.WriteEvent($"New Voice Actor info: {newValue}");
				m_lblActorsAssigned.Text = newValue;
			}
		}

		private void UpdateProjectState()
		{
			if (m_project == null || Settings.Default.CurrentProject != m_persistenceImpl.GetProjectFilePath(m_project))
			{
				// Temporarily clear this setting. If something goes horribly wrong loading/migrating the project,
				// we don't want to get the user into a situation where Glyssen is permanently hamstrung because it
				// always attempts to open the same (corrupt) project.
				Settings.Default.CurrentProject = null;
				Settings.Default.Save();
			}
			if (m_project == null)
				m_btnOpenProject.Enabled = true;
			else
				UpdateButtons((m_project.ProjectState & ProjectState.ReadyForUserInteraction) > 0);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveCurrentProject();
			Settings.Default.Save();
		}

		private void SaveCurrentProject(bool clearCharacterStats = false)
		{
			if (m_project != null)
			{
				if (clearCharacterStats)
					m_project.ClearCharacterStatistics();
				m_project.Save();
			}
		}

		private void ShowLastLocation()
		{
			var lastLocation = m_project.LastExportLocation;
			m_lastExportLocationLink.Text = lastLocation;
			m_lblFilesAreHere.Visible = !IsNullOrEmpty(lastLocation);
		}

		private void View_Script_Click(object sender, EventArgs e)
		{
			ShowProjectScriptPresenterDlg(exporter => new ViewScriptDlg(exporter));
		}

		private void EnsureGroupsAreInSynchWithCharactersInUse()
		{
			if (!m_project.CharacterGroupList.CharacterGroups.Any())
				return;

			var adjuster = new CharacterGroupsAdjuster(m_project);
			if (adjuster.GroupsAreNotInSynchWithData)
			{
				using (var progressDialog = new GenerateGroupsProgressDialog(m_project, OnGenerateGroupsWorkerDoWork, false, true))
				{
					var generator = new CharacterGroupGenerator(m_project, ProjectCastSizePlanningViewModel.SelectedCastSize, progressDialog.BackgroundWorker);
					progressDialog.ProgressState.Arguments = generator;

					if (progressDialog.ShowDialog() == DialogResult.OK && generator.GeneratedGroups != null)
					{
						var assignedBefore = m_project.CharacterGroupList.CountVoiceActorsAssigned();
						generator.ApplyGeneratedGroupsToProject();

						if (m_project.CharacterGroupList.CountVoiceActorsAssigned() < assignedBefore)
						{
							var msg = LocalizationManager.GetString("MainForm.FewerAssignedActorsAfterGeneration",
								"An actor assignment had to be removed. Please review the Voice Actor assignments, and adjust where necessary.");
							MessageBox.Show(this, msg, Text, MessageBoxButtons.OK);
						}
					}
					else
					{
						adjuster.MakeMinimalAdjustments();
					}

					m_project.Save(true);
				}
			}
			// This is for the migration of old projects.
			// ENHANCE; Theoretically, we could, before we update the controlfileversion number, set a flag
			// letting us know if this needs to run or not. It would be for any number < 96.
			// This method would be moved into Project (which maybe it should be anyway).
			// But this must be called only AFTER EnsureGroupsAreInSynchWithCharactersInUse has been run.
			if (m_project.CharacterGroupList.CharacterGroups.Any(g => g.GroupIdLabel == CharacterGroup.Label.None))
				CharacterGroupList.AssignGroupIds(m_project.CharacterGroupList.CharacterGroups);
		}

		private void OnGenerateGroupsWorkerDoWork(object s, DoWorkEventArgs e)
		{
			var generator = (CharacterGroupGenerator)((ProgressState)e.Argument).Arguments;
			generator.GenerateCharacterGroups();
		}

		private bool IsOkToExport(ProjectExporter exporter)
		{
			var export = true;
			string dlgMessage = null;
			bool scriptIncomplete = false;
			if (exporter.IncludeVoiceActors)
			{
				if (!m_project.IsVoiceActorAssignmentsComplete)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.NotAssignedMessage",
						"One or more character groups have no voice actor assigned. Are you sure you want to export an incomplete script?");
					scriptIncomplete = true;
				}
				else if (!m_project.EveryAssignedGroupHasACharacter)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.EmptyGroupMessage",
						"One or more character groups have no characters in them. Are you sure you want to export a script?");
				}
			}
			else if (m_project.ProjectAnalysis.UserPercentAssigned < 100d)
			{
				dlgMessage = Format(LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.CharacterAssignmentIncompleteMessage",
						"Character assignment is {0} complete. Are you sure you want to export a script?", "Parameter is a percentage."),
					MathUtilities.FormattedPercent(m_project.ProjectAnalysis.UserPercentAssigned, 1, 3));
				scriptIncomplete = true;
			}
			else if (m_project.ProjectAnalysis.NeedsReviewBlocks > 0)
			{
				dlgMessage = BlocksNeedReviewMessage + " " + UseNeedsReviewFilterHint + Environment.NewLine +
					LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.BlocksNeedReviewMessage",
					"Any block marked for review will not be assigned to a voice actor in the script until an actual biblical " +
					"character is specified. Are you sure you want to export the script now?");
			}

			if (dlgMessage != null)
			{
				string dlgTitle = scriptIncomplete ?
					LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?"):
					LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.Title", "Export Script?");

				dlgMessage += Environment.NewLine +
					LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.MessageNote",
						"(Note: You can export the script again as many times as you want.)");
				export = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
			}

			return export;
		}

		private void SetupUiLanguageMenu()
		{
			bool LanguageSelected(string languageId)
			{
				Analytics.Track("SetUiLanguage",
					new Dictionary<string, string>
					{
						{ "uiLanguage", languageId },
						{ "previous", Settings.Default.UserInterfaceLanguage },
						{"reapplyLocalizations", "true"}
					});
				Logger.WriteEvent("UI language changed from " +
					$"{Settings.Default.UserInterfaceLanguage} to {languageId}");
				Settings.Default.UserInterfaceLanguage = languageId;
				Program.UpdateUiLanguageForUser(languageId);
				return true;
			}

			bool MoreSelected()
			{
				Analytics.Track("Opened localization dialog box");
				return true;
			}

			m_uiLanguageMenu.InitializeWithAvailableUILocales(LanguageSelected,
				Program.PrimaryLocalizationManager, Program.LocIncompleteViewModel,
				MoreSelected);
		}

		private void Assign_Click(object sender, EventArgs e)
		{
			if (m_project.ReferenceTextProxy.Missing)
			{
				if (!ResolveNullReferenceText())
					return;
			}

			if (ModifierKeys == Keys.Shift && MessageBox.Show("Are you sure you want to automatically disambiguate (for demo purposes)?", ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
				m_project.DoDemoDisambiguation();

			var origCursor = Cursor;
			Cursor = Cursors.WaitCursor;

			var fontProxy = new AdjustableFontProxy(m_project.FontFamily, m_project.FontSizeInPoints, m_project.RightToLeftScript);
			using (var viewModel = new AssignCharacterViewModel(m_project, fontProxy, AdjustableFontProxy.GetFontProxyForReferenceText))
			{
				viewModel.ProjectCharacterVerseDataAdded += HandleProjectCharacterAdded;
				using (var dlg = new AssignCharacterDlg(viewModel))
				{
					LogDialogDisplay(dlg);
					dlg.ShowDialog(this);
				}
			}

			Cursor = origCursor;

			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
			SaveCurrentProject(true);
		}

		private void HandleProjectCharacterAdded(AssignCharacterViewModel sender, string reference, string characterId, string delivery)
		{
			Analytics.Track("AddCharacter", new Dictionary<string, string>
			{
				{"verseReference", reference},
				{"characterId", characterId},
				{"delivery", delivery}
			});
		}

		private void SelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDlg(m_project, m_paratextScrTextWrapperForRecentlyCreatedProject))
			{
				if (ShowModalDialogWithWaitCursor(dlg) == DialogResult.OK)
				{
					m_project.ClearAssignCharacterStatus();
					if ((m_project.ProjectState & ProjectState.FullyInitialized) > 0)
					{
						m_project.Analyze();
						UpdateDisplayOfProjectInfo();
						SaveCurrentProject(true);
					}
					else
					{
						m_project.ProjectStateChanged += ProjectStateChangedAfterSelectingBooks;
					}
				}
			}
			m_paratextScrTextWrapperForRecentlyCreatedProject = null;
		}

		private void ProjectStateChangedAfterSelectingBooks(object sender, Project.ProjectStateChangedEventArgs e)
		{
			if (m_project != sender /* This probably can't happen. */ ||
			    (m_project.ProjectState & ProjectState.FullyInitialized) == 0)
				return;

			if (!m_project.IncludedBooks.Any())
				MessageBox.Show(this, LocalizationManager.GetString("Project.NoBooksIncluded", "No content found in any included books."), GlyssenInfo.Product);

			m_project.ProjectStateChanged -= ProjectStateChangedAfterSelectingBooks;
			
			SaveCurrentProject(true);
		}

		private void Settings_Click(object sender, EventArgs e)
		{
			var origCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			var model = new ProjectSettingsViewModel(m_project);
			var wsModel = ProjectWritingSystemSetupModel;
			using (var dlg = new ProjectSettingsDlg(model, wsModel))
			{
				LogDialogDisplay(dlg);
				m_isOkayToClearExistingRefBlocksThatCannotBeMigrated = null;
				m_project.IsOkayToClearExistingRefBlocksWhenChangingReferenceText = AskUserWhetherToClearExistingRefBlocksThatCannotBeMigrated;
				var result = dlg.ShowDialog(this);
				Cursor = origCursor;
				if (result != DialogResult.OK)
					return;

				m_project.UpdateSettings(model, wsModel.CurrentDefaultFontName, (int)wsModel.CurrentDefaultFontSize,
					wsModel.CurrentRightToLeftScript);
				SaveCurrentProject();
				m_project.IsOkayToClearExistingRefBlocksWhenChangingReferenceText = null;

				if (dlg.UpdatedBundle != null)
				{
					Analytics.Track("UpdateProjectFromBundleData", new Dictionary<string, string>
					{
						{"language", m_project.LanguageIsoCode},
						{"projectID", m_project.Id},
						{"recordingProjectName", m_project.Name},
						{"bundlePathChanged", (m_project.OriginalBundlePath != model.BundlePath).ToString()}
					});
					var project = m_project.UpdateProjectFromBundleData(dlg.UpdatedBundle);
					project.OriginalBundlePath = model.BundlePath;
					SetProject(project);
				}
				else if (dlg.UpdatedParatextProject != null)
				{
					Analytics.Track("UpdateProjectFromParatextData", new Dictionary<string, string>
					{
						{"language", m_project.LanguageIsoCode},
						{"paratextProjectName", m_project.ParatextProjectName},
						{"projectID", m_project.Id},
						{"recordingProjectName", m_project.Name},
					});
					var project = m_project.UpdateProjectFromParatextData(dlg.UpdatedParatextProject);
					SetProject(project);
				}
			}
			UpdateDisplayOfProjectInfo();
		}

		private bool AskUserWhetherToClearExistingRefBlocksThatCannotBeMigrated()
		{
			if (m_isOkayToClearExistingRefBlocksThatCannotBeMigrated == null)
			{
				m_isOkayToClearExistingRefBlocksThatCannotBeMigrated =
					MessageBox.Show(this, Format(LocalizationManager.GetString("Project.OkayToClearExistingRefBlocksThatCannotBeMigrated",
					"This project has been changed to use the {0} reference text, but some blocks were already matched to " +
					"the previous reference text. Some of those matches cannot be migrated, which means that the reference " +
					"text data for those blocks is not in the correct language. " +
					"To avoid probable confusion, would you like to allow {1} to clear the matches that cannot be migrated properly?",
					"Param 0: name of language of new reference text; " +
					"Param 1: \"Glyssen\" (product name)"), m_project.UiReferenceTextName, GlyssenInfo.Product),
					ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
			}
			return (bool)m_isOkayToClearExistingRefBlocksThatCannotBeMigrated;
		}

		private void Exit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Save is handled in FormClosing event
			Close();
		}

		private void About_Click(object sender, EventArgs e)
		{
			using (var dlg = new SILAboutBox(FileLocationUtilities.GetFileDistributedWithApplication("aboutbox.htm")))
			{
				dlg.CheckForUpdatesClicked += HandleAboutDialogCheckForUpdatesClick;
				dlg.ReleaseNotesClicked += HandleAboutDialogReleaseNotesClicked;
				dlg.ShowDialog(this);
			}
		}

		private void HandleAboutDialogCheckForUpdatesClick(object sender, EventArgs e)
		{
			var updateStatus = UpdateChecker.CheckForUpdatesAtUserRequest();
			if (updateStatus == Sparkle.UpdateStatus.UpdateNotAvailable)
				((SILAboutBox)sender).NotifyNoUpdatesAvailable();
		}

		private void HandleAboutDialogReleaseNotesClicked(object sender, EventArgs e)
		{
			var path = FileLocationUtilities.GetFileDistributedWithApplication("ReleaseNotes.md");
			using (var dlg = new ShowReleaseNotesDialog(((Form)sender).Icon, path))
				dlg.ShowDialog();
		}

		private string BlocksNeedReviewMessage =>
			m_project.ProjectAnalysis.NeedsReviewBlocks > 1 ?
			Format(LocalizationManager.GetString("MainForm.BlocksNeedReviewMessage",
			"There are {0} blocks in this project that need review before finalizing the script."), m_project.ProjectAnalysis.NeedsReviewBlocks) :
			LocalizationManager.GetString("MainForm.OneBlockNeedReviewMessage",
				"There is one block in this project that needs review before finalizing the script.");

		private string UseNeedsReviewFilterHint =>
			LocalizationManager.GetString("MainForm.UseNeedsReviewFilterHint",
			"(You can use the \"Needs review\" filter in Identify Speaking Parts to see which blocks still need attention.)");

		private void AssignVoiceActors_Click(object sender, EventArgs e)
		{
			if (m_project.ProjectAnalysis.NeedsReviewBlocks > 0)
			{
				var msg = LocalizationManager.GetString("Project.BlocksNeedReviewBeforeAssigning",
					"You can work on voice actor assignments now, but any block marked for review will not be assigned to a " +
					"character group until an actual biblical character is specified. Therefore, character groups and actor " +
					"assignments to those groups will be tentative until you complete this work in Identify Speaking Parts. ");
				if (MessageBox.Show(this, BlocksNeedReviewMessage + Environment.NewLine + msg + Environment.NewLine +
					UseNeedsReviewFilterHint, ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
				{
					return;
				}
			}
			// TODO: Eventually, this should be called when the user requests that all overrides be reverted to the defaults.
			//m_project.UseDefaultForUnresolvedMultipleChoiceCharacters();

			bool regenerateGroups = sender == m_btnCastSizePlanning;
			if (!regenerateGroups)
				EnsureGroupsAreInSynchWithCharactersInUse();

			if (!m_project.CharacterGroupList.CharacterGroups.Any())
				GenerateGroupsProgressDialog.GenerateGroupsWithProgress(m_project, false, true, false, ProjectCastSizePlanningViewModel.SelectedCastSize);
			else if (regenerateGroups)
				GenerateGroupsProgressDialog.GenerateGroupsWithProgress(m_project, true, false, false, ProjectCastSizePlanningViewModel.SelectedCastSize);

			bool launchCastSizePlanning;
			using (var dlg = new VoiceActorAssignmentDlg(new VoiceActorAssignmentViewModel(m_project)))
			{
				ShowModalDialogWithWaitCursor(dlg);
				launchCastSizePlanning = dlg.LaunchCastSizePlanningUponExit;
			}
			m_project.EnsureCastSizeOptionValid();
			SaveCurrentProject();
			UpdateDisplayOfProjectInfo();
			if (launchCastSizePlanning)
				m_btnCastSizePlanning_Click(m_btnCastSizePlanning, new EventArgs());
		}

		private class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}
		}

		private void m_lastExportLocationLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// REVIEW: Should we store the actual path to the main exported file and call SelectFileInExplorer() instead?
			PathUtilities.OpenDirectoryInExplorer(m_lastExportLocationLink.Text);
		}

		private CastSizePlanningViewModel ProjectCastSizePlanningViewModel
		{
			get
			{
				if (m_projectCastSizePlanningViewModel == null)
					m_projectCastSizePlanningViewModel = new CastSizePlanningViewModel(m_project);
				return m_projectCastSizePlanningViewModel;
			}
		}

		private WritingSystemSetupModel ProjectWritingSystemSetupModel =>
			new WritingSystemSetupModel(m_project.WritingSystem)
			{
				CurrentDefaultFontName = m_project.FontFamily,
				CurrentDefaultFontSize = m_project.FontSizeInPoints,
				CurrentRightToLeftScript = m_project.RightToLeftScript
			};

		private void m_btnCastSizePlanning_Click(object sender, EventArgs e)
		{
			bool launchAssignVoiceActors = false;
			using (var dlg = new CastSizePlanningDlg(ProjectCastSizePlanningViewModel))
			{
				SaveCurrentProject();
				if (ShowModalDialogWithWaitCursor(dlg) == DialogResult.OK)
				{
					UpdateDisplayOfProjectInfo();
					launchAssignVoiceActors = true;
				}
			}
			if (launchAssignVoiceActors)
				AssignVoiceActors_Click(m_btnCastSizePlanning, EventArgs.Empty);
		}

		private void ShowProjectScriptPresenterDlg(Func<ProjectExporter, Form> getProjectScriptPresenterDlg)
		{
			EnsureGroupsAreInSynchWithCharactersInUse();

			if (m_project.ReferenceText == null)
			{
				if (m_temporaryRefTextOverrideForExporting != null)
					m_project.ReferenceText = m_temporaryRefTextOverrideForExporting;
				else if (!ResolveNullReferenceText(Format(LocalizationManager.GetString("Project.TemporarilyUseEnglishReferenceText",
					"To continue and temporarily use the English reference text, click {0}.", "Param is \"Ignore\" button label (in the current Windows locale)"),
					MessageBoxStrings.IgnoreButton)))
				{
					return;
				}
			}

			try
			{
				var exporter = new ProjectExporter(m_project, GlyssenSettingsProvider.ExportSettingsProvider, new ExcelColorizer());

				if (!IsOkToExport(exporter))
					return;

				using (var dlg = getProjectScriptPresenterDlg(exporter))
				{
					ShowModalDialogWithWaitCursor(dlg);
					ShowLastLocation();
				}
			}
			finally
			{
				// Reset the temporarily overriden ReferenceText property so it won't accidentally get used for the ISP dialog.
				if (m_project.ReferenceTextProxy.Missing)
					m_project.ReferenceText = null;
			}
		}

		private bool ResolveNullReferenceText(string ignoreOptionText = null)
		{
			if (m_project.ReferenceText != null)
				return true;

			var model = new ProjectSettingsViewModel(m_project);
			string projectSettingsDlgTitle, referenceTextTabName;
			using (var dlg = new ProjectSettingsDlg(model, ProjectWritingSystemSetupModel))
			{
				projectSettingsDlgTitle = dlg.Text;
				referenceTextTabName = dlg.ReferenceTextTabPageName;
			}
			var customReferenceTextFolder = m_persistenceImpl.GetProjectFolderPath(m_project.ReferenceTextProxy);
			if (customReferenceTextFolder.Any(c => c == ' '))
				customReferenceTextFolder = customReferenceTextFolder.Replace(" ", "\u00A0");
			customReferenceTextFolder = "file://" + customReferenceTextFolder;

			while (m_project.ReferenceText == null)
			{
				var msgFmt = LocalizationManager.GetString("Project.UnavailableReferenceText",
					"This project uses a custom reference text ({0}) that is not available on this computer.\nIf you have access " +
					"to the required reference text files, please put them in" +
					"\n    {1}\n" +
					"and then click {2} to use the {0} reference text.\n\n" +
					"Otherwise, to permanently change the reference text used by this project, open the {3}\n" +
					"dialog box and select the desired reference text on the {4} tab page.",
					"Param 0: Name of reference text (typically a language name); " +
					"Param 1: Folder path where custom reference text; " +
					"Param 2: The name of the \"Retry\" button label (in the current Windows locale); " +
					"Param 3: Name of the Project Settings dialog box; " +
					"Param 4: Label of the Reference Text tab");
				var msg = Format(msgFmt, m_project.UiReferenceTextName, customReferenceTextFolder, MessageBoxStrings.RetryButton,
					projectSettingsDlgTitle, referenceTextTabName);
				if (ignoreOptionText != null)
					msg += "\n\n" + ignoreOptionText;
				Logger.WriteEvent(msg);
				switch (FlexibleMessageBox.Show(msg, GlyssenInfo.Product,
					ignoreOptionText == null ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning,
					(sender, e) => { SafeCreateAndOpenFolder(e.LinkText); }))
				{
					case DialogResult.Cancel:
					case DialogResult.Abort: return false;
					case DialogResult.Ignore:
						m_project.ReferenceText = m_temporaryRefTextOverrideForExporting = ReferenceText.GetStandardReferenceText(ReferenceTextType.English);
						break;
				}
			}
			return true;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.E)
			{
				if (m_btnExport.Enabled)
				{
					ShowProjectScriptPresenterDlg(exportViewModel => new ExportDlg(exportViewModel));
					e.Handled = true;
					return;
				}
			}
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Zip the project directory into a zip file with the same name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Export_Click(object sender, EventArgs e)
		{
			try
			{
				m_tableLayoutPanel.Enabled = false;
				Cursor.Current = Cursors.WaitCursor;
				ExportShare();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				m_tableLayoutPanel.Enabled = true;
			}
		}

		private void ExportShare()
		{
			var sourceDir = Path.GetDirectoryName(m_persistenceImpl.GetProjectFilePath(m_project));
			Debug.Assert(sourceDir != null);
			var nameInZip = sourceDir.Substring(ProjectRepository.ProjectsBaseFolder.Length);

			var shareFolder = ProjectRepository.DefaultShareFolder;
			Directory.CreateDirectory(shareFolder);

			string fallbackVersificationFilePath = null;

			if (!m_persistenceImpl.ResourceExists(m_project, ProjectResource.Versification))
			{
				fallbackVersificationFilePath = m_persistenceImpl.GetFallbackVersificationFilePath(m_project);
				try
				{
					m_project.Versification.Save(fallbackVersificationFilePath);
				}
				catch (Exception e)
				{
					Logger.WriteError($"Failed to save fallback versification file to {fallbackVersificationFilePath}", e);
				}
			}

			try
			{
				var saveAsName = Path.Combine(shareFolder, m_project.LanguageIsoCode + "_" + m_project.Name) + kShareFileExtension;
				using (var zip = new ZipFile())
				{
					zip.AddDirectory(sourceDir, nameInZip);
					zip.Save(saveAsName);
				}

				if (m_project.ReferenceTextProxy.Type == ReferenceTextType.Custom)
				{
					var msg = LocalizationManager.GetString("MainForm.ExportedProjectUsesCustomReferenceText",
						"This project uses a custom reference text ({0}). For best results, if you share this project, the custom reference text " +
						"should be installed on the other computer before importing.");
					MessageBox.Show(this, Format(msg, m_project.ReferenceTextProxy.CustomIdentifier),
						ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}

				PathUtilities.SelectFileInExplorer(saveAsName);
				Logger.WriteEvent($"Project exported to {saveAsName}");
			}
			finally
			{
				if (fallbackVersificationFilePath != null)
					RobustFile.Delete(fallbackVersificationFilePath);
			}
		}

		private void Import_Click(object sender, EventArgs e)
		{
			string importFile = null;

			// show the user an Open File dialog
			using (var ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = ProjectRepository.DefaultShareFolder;
				ofd.Filter = Format("{0} ({1})|{1}|{2} ({3})|{3}",
					LocalizationManager.GetString("DialogBoxes.ImportDlg.GlyssenSharesFileTypeLabel",
						"Glyssen shares", "Label used in Import file dialog for \"*.glyssenshare\" files"),
					"*" + kShareFileExtension,
					L10N.AllFilesLabel, "*.*");
				ofd.RestoreDirectory = true;

				if (ofd.ShowDialog() == DialogResult.OK)
					importFile = ofd.FileName;
			}

			// if nothing was selected, return now
			if (IsNullOrEmpty(importFile))
				return;

			try
			{
				InitializeProgress();
				ImportShare(importFile);
			}
			finally
			{
				CloseProgress();
			}
		}

		private void ImportShare(string importFile)
		{
			Logger.WriteEvent($"Importing {importFile}.");

			// open the zip file
			using (var zip = new ZipFile(importFile))
			{
				var path = zip.Entries.FirstOrDefault(ze => ze.IsDirectory);

				if (path == null)
					return;

				var targetDir = Path.Combine(ProjectRepository.ProjectsBaseFolder, path.FileName);

				var projectFileName = path.FileName.Split('/').First() + ProjectRepository.kProjectFileExtension;
				var projectFilePath = Path.Combine(targetDir, projectFileName);

				// warn the user if data will be overwritten
				if (RobustFile.Exists(projectFilePath))
				{
					var msg = LocalizationManager.GetString("MainForm.ImportWarning",
							"Warning: You are about to import a project that already exists. If you continue, the existing project files will be replaced by the files being imported, which " +
							"might result in loss of data. Do you want to continue and overwrite the existing files?");
					Logger.WriteEvent(msg + " " + projectFilePath);

					if (MessageBox.Show(msg, GlyssenInfo.Product, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
						return;
				}

				// close the current project
				SetProject(null);

				zip.ExtractAll(ProjectRepository.ProjectsBaseFolder, ExtractExistingFileAction.OverwriteSilently);

				// open the imported project
				if (RobustFile.Exists(projectFilePath))
				{
					void AdditionalActionAfterSettingProject()
					{
						SaveCurrentProject();
						if (m_project.ReferenceTextProxy.Missing)
						{
							var msg = LocalizationManager.GetString("MainForm.ImportedProjectUsesMissingReferenceText",
								"The imported project uses a custom reference text ({0}) that is not available on this computer.\nFor best results, " +
								"close {1} and install the reference text here:" +
								"\n    {2}\n" +
								"\nThen restart {1} to continue working with this project.",
								"Param 0: name of missing reference text; Param 1: \"Glyssen\"; Param 2: Path to Local Reference Texts folder");
							FlexibleMessageBox.Show(this, Format(msg, m_project.ReferenceTextProxy.CustomIdentifier, ProductName,
								"file://" + m_persistenceImpl.GetProjectFolderPath(m_project.ReferenceTextProxy)),
								ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning, (sender, e) => { SafeCreateAndOpenFolder(e.LinkText); });
						}
					}

					LoadProject(projectFilePath, AdditionalActionAfterSettingProject);
				}
			}
		}

		private static void SafeCreateAndOpenFolder(string folderPath)
		{
			try
			{
				Directory.CreateDirectory(folderPath);
				Process.Start(folderPath);
			}
			catch (Exception)
			{
				// Ignore;
			}
		}
	}
}
