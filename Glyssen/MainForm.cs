using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Paratext;
using GlyssenEngine.Rules;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;
using L10NSharp;
using L10NSharp.UI;
using SIL;
using SIL.DblBundle;
using SIL.IO;
using SIL.Progress;
using SIL.Reporting;
using SIL.Windows.Forms;
using SIL.Windows.Forms.Miscellaneous;
using Ionic.Zip;
using L10NSharp.TMXUtils;
using NetSparkle;
using Paratext.Data;
using SIL.Scripture;
using SIL.Windows.Forms.ReleaseNotes;
using SIL.Windows.Forms.WritingSystems;
using static System.String;
using Analytics = DesktopAnalytics.Analytics;
using Resources = Glyssen.Properties.Resources;
using AssignCharacterViewModel = GlyssenEngine.ViewModels.AssignCharacterViewModel<System.Drawing.Font>;

namespace Glyssen
{
	public partial class MainForm : FormWithPersistedSettings
	{
		public static Sparkle UpdateChecker { get; private set; }
		private Project m_project;
		private ParatextScrTextWrapper m_paratextScrTextWrapperForRecentlyCreatedProject;
		private CastSizePlanningViewModel m_projectCastSizePlanningViewModel;
		private string m_percentAssignedFmt;
		private string m_actorsAssignedFmt;
		private string m_castSizeFmt;
		private readonly List<Tuple<Button, string>> m_buttonFormats = new List<Tuple<Button, string>>();
		private bool? m_isOkayToClearExistingRefBlocksThatCannotBeMigrated;
		private ReferenceText m_temporaryRefTextOverrideForExporting;

		public MainForm(IReadOnlyList<string> args)
		{
			InitializeComponent();

			Project.s_fontRepository = new WinFormsFontRepositoryAdapter();

			SetupUiLanguageMenu();
			Logger.WriteEvent($"Initial UI language: {Settings.Default.UserInterfaceLanguage}");

			m_toolStrip.Renderer = new NoBorderToolStripRenderer();
			m_uiLanguageMenu.ToolTipText = Localizer.GetString("MainForm.UILanguage", "User-interface Language");

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized; // Don't need to unsubscribe since this object will be around as long as the program is running.

			m_lastExportLocationLink.Text = Empty;

			// Did the user start Glyssen by double-clicking a share file?
			if (args.Count == 1 && args[0].ToLowerInvariant().EndsWith(ProjectBase.kShareFileExtension))
			{
				ImportShare(args[0]);
			}
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
			}

			m_projectCastSizePlanningViewModel = null;
			m_project = project;

			if (m_project != null)
			{
				Logger.WriteEvent($"Opening project {m_project.Name}");

				m_project.ProjectStateChanged += FinishSetProjectIfReady;
				m_project.CharacterGroupCollectionChanged += UpdateDisplayOfCastSizePlan;
				m_project.CharacterGroupCollectionChanged += ClearCastSizePlanningViewModel;
				m_project.CharacterStatisticsCleared += ClearCastSizePlanningViewModel;
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
					using (var viewModel = new AssignCharacterViewModel(m_project))
					using (var dlg = new UnappliedSplitsDlg(m_project.Name, viewModel.FontInfo,
						new UnappliedSplitsViewModel(m_project.IncludedBooks, m_project.RightToLeftScript)))
					{
						LogDialogDisplay(dlg);
						dlg.ShowDialog(this);
					}

				Settings.Default.CurrentProject = m_project.ProjectFilePath;
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

		private void HandleStringsLocalized()
		{
			m_percentAssignedFmt = m_lblPercentAssigned.Text;
			m_actorsAssignedFmt = m_lblActorsAssigned.Text;
			m_castSizeFmt = m_lblCastSizePlan.Text;
			RememberButtonFormats();
			UpdateLocalizedText();
			if (m_project != null)
				m_project.ProjectCharacterVerseData.HandleStringsLocalized();
			ControlCharacterVerseData.Singleton.HandleStringsLocalized();
		}

		private void RememberButtonFormats()
		{
			m_buttonFormats.Clear();
			var pos = m_tableLayoutPanel.GetCellPosition(m_btnOpenProject);
			for (var rowIndex = pos.Row; rowIndex < m_tableLayoutPanel.RowStyles.Count; rowIndex++)
			{
				var btn = m_tableLayoutPanel.GetControlFromPosition(pos.Column, rowIndex) as Button;
				if (btn != null)
					m_buttonFormats.Add(new Tuple<Button, string>(btn, btn.Text));
			}
		}

		private void UpdateButtons(bool readOnly)
		{
			m_btnOpenProject.Enabled = !readOnly;
			m_imgCheckOpen.Visible = true;
			m_btnSettings.Enabled = !readOnly && m_project.ProjectFileIsWritable;
			m_imgCheckSettings.Visible = m_btnSettings.Enabled && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.IsQuoteSystemReadyForParse;
			m_btnSelectBooks.Enabled = !readOnly && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.ProjectFileIsWritable;
			m_imgCheckBooks.Visible = m_btnSelectBooks.Enabled && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed && m_project.IncludedBooks.Any();
			m_btnIdentify.Enabled = !readOnly && m_imgCheckSettings.Visible && m_imgCheckBooks.Visible;
			m_imgCheckAssignCharacters.Visible = m_btnIdentify.Enabled && (int)(m_project.ProjectAnalysis.UserPercentAssigned) == 100;
			if (m_project.ReferenceText == null)
			{
				m_imgCheckAssignCharacters.Visible = true;
				m_imgCheckAssignCharacters.Image = Resources.Alert;
			}
			else if (m_btnIdentify.Enabled)
				m_imgCheckAssignCharacters.Image = m_project.ProjectAnalysis.AlignmentPercent == 100 ? Resources.green_check : Resources.yellow_check;
			m_btnExport.Enabled = !readOnly && m_btnIdentify.Enabled;

			m_btnAssignVoiceActors.Visible = Environment.GetEnvironmentVariable("Glyssen_ProtoscriptOnly", EnvironmentVariableTarget.User) == null;
			m_btnCastSizePlanning.Visible = m_btnAssignVoiceActors.Visible;

			m_btnCastSizePlanning.Enabled = m_btnCastSizePlanning.Visible && !readOnly && m_imgCheckAssignCharacters.Visible;

			// TODO: Determine when the Cast Size Planning task is done enough to move on to the next task.
			// Tom added the final portion of the logic to allow us to continue to access the Roles for Voice Actors dialog at least
			// until Phil finishes his Cast Size Planning work. Obviously, we need the part about having groups. The question is whether
			// we should also allow them to access Roles for Voice Actors if they entered actors but never clicked Generate Groups in the
			// Cast Size Planning dialog box... See more in the big red bullet point that I added to
			// https://docs.google.com/document/d/1cwPEYGnLJKK4TkP2MQy3t49k5r9rexCTWDb-jlCSDUo/edit?usp=sharing
			m_imgCastSizePlanning.Visible = m_btnCastSizePlanning.Visible && m_btnCastSizePlanning.Enabled &&
				(m_project.CharacterGroupList.CharacterGroups.Any() || m_project.VoiceActorList.ActiveActors.Any());

			m_btnAssignVoiceActors.Enabled = m_btnAssignVoiceActors.Visible && !readOnly && m_imgCastSizePlanning.Visible;
			m_imgCheckAssignActors.Visible = m_btnAssignVoiceActors.Visible && m_btnAssignVoiceActors.Enabled && m_project.IsVoiceActorScriptReady;
			m_lnkExit.Enabled = !readOnly;

			m_exportMenu.Enabled = true;
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

			m_exportMenu.Enabled = false;
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

		internal static void LogDialogDisplay(Form dlg)
		{
			Logger.WriteEvent($"Displaying dialog box: {dlg.Text}");
		}

		private void ShowOpenProjectDialog()
		{
			using (var dlg = new OpenProjectDlg(m_project))
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
							LoadParatextProject(dlg.SelectedProject);
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
						{"exceptionMessage", ex.Message},
						{"CurrentProjectPath", Settings.Default.CurrentProject},
					});
					throw;
				}
			}
			UpdateChecker = new Sparkle(@"http://build.palaso.org/guestAuth/repository/download/Glyssen_GlyssenMasterPublish/.lastSuccessful/appcast.xml",
				Icon);
			// We don't want to do this until the main window is loaded because a) it's very easy for the user to overlook, and b)
			// more importantly, when the toast notifier closes, it can sometimes clobber an error message being displayed for the user.
			UpdateChecker.CheckOnFirstApplicationIdle();
		}

		private void InitializeProgress()
		{
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
			bool loadedSuccessfully = LoadAndHandleApplicationExceptions(() =>
			{
				SetProject(Project.Load(filePath, HandleMissingBundleNeededForProjectUpgrade));
				additionalActionAfterSettingProject?.Invoke();
			});

			if (!loadedSuccessfully)
				SetProject(null);

			m_lastExportLocationLink.Text = m_project?.LastExportLocation;
			m_lblFilesAreHere.Visible = !IsNullOrEmpty(m_lastExportLocationLink.Text);
		}

		private bool HandleMissingBundleNeededForProjectUpgrade(Project existingProject)
		{
			string msg = Project.ParserUpgradeMessage + " " + Format(Localizer.GetString("Project.ParserUpgradeBundleMissingMsg",
				"To make use of the new engine, the original text release bundle must be available, but it is not in the original location ({0})."),
				existingProject.OriginalBundlePath) +
				Environment.NewLine + Environment.NewLine +
				Localizer.GetString("Project.LocateBundleYourself", "Would you like to locate the text release bundle yourself?");
			string caption = Localizer.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
			if (MessageResult.Yes == MessageModal.Show(msg, caption, Buttons.YesNo))
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
			var publicationFolder = Project.GetPublicationFolderPath(bundle);
			if (Directory.Exists(publicationFolder) &&
				Directory.GetDirectories(publicationFolder).Any(f => Directory.GetFiles(f, "*" + Constants.kProjectFileExtension)
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
				}
			}
			else
				projFilePath = Project.GetDefaultProjectFilePath(bundle);

			var recordingProjectName = projFilePath.GetContainingFolderName();
			if (File.Exists(projFilePath))
			{
				if (GlyssenDblTextMetadata.GetRevisionOrChangesetId(projFilePath) == bundle.Metadata.RevisionOrChangesetId)
				{
					LoadProject(projFilePath);
					bundle.Dispose();
					return;
				}
				// If we get here, then the Select Existing Project dialog was not displayed, but there is
				// already a project with the same path (i.e., for a different revision). So we need to
				// generate a unique revision-specific project path.
				// TODO (PG-222): Before blindly creating a new project, we probably need to prompt the
				// user to see if they want to upgrade an existing project instead. If there are multiple
				// candidate projects, we'll need to present a list.
				var baserecordingProjectName = recordingProjectName;
				recordingProjectName = $"{baserecordingProjectName} (Rev {bundle.Metadata.Revision})";
				var path = Project.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
				for (int i = 1; File.Exists(path); i++)
				{
					recordingProjectName = $"{baserecordingProjectName} (Rev {bundle.Metadata.Revision}.{i})";
					path = Project.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
				}
			}

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
				var msg = Format(Localizer.GetString("Project.InvalidVersificationFile",
					"Invalid versification file in text release bundle. Unable to create project.\r\n" +
					"Text release Bundle: {0}\r\n" +
					"Versification file: {1}\r\n" +
					"Error: {2}"),
					bundlePath, DblBundleFileUtils.kVersificationFileName, error);
				Logger.WriteError(msg, ex);
				MessageBox.Show(this, msg, GlyssenInfo.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				SetProject(null);
			}

			bundle.Dispose();
		}

		private void LoadParatextProject(string paratextProjId)
		{
			Logger.WriteEvent($"Loading {ParatextScrTextWrapper.kParatextProgramName} project {paratextProjId}");

			ParatextScrTextWrapper paratextProject = null;

			if (!LoadAndHandleApplicationExceptions(() => { paratextProject = new ParatextScrTextWrapper(ScrTextCollection.Find(paratextProjId)); }))
			{
				SetProject(null);
				return;
			}

			var optionalObserverInfo = paratextProject.UserCanEditProject ? Empty :
				Format(Localizer.GetString("Project.NonEditingRole", "(You do not seem to have editing privileges for this {0} project.)",
					"Param: \"Paratext\" (product name)"), ParatextScrTextWrapper.kParatextProgramName) +
				Environment.NewLine;

			if (!paratextProject.HasQuotationRulesSet)
			{
				var msg = Format(Localizer.GetString("Project.ParatextQuotationRulesNotDefined",
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
					GlyssenInfo.kProduct,
					ParatextScrTextWrapper.kParatextProgramName,
					paratextProjId,
					optionalObserverInfo,
					ParatextProjectBookInfo.LocalizedCheckName(ParatextScrTextWrapper.kQuotationCheckId));
				var result = MessageBox.Show(this, msg, GlyssenInfo.kProduct, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
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
				var msg = Format(Localizer.GetString("Project.NoBooksPassedChecks",
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
					paratextProjId,
					GlyssenInfo.kProduct,
					paratextProject.RequiredCheckNames,
					optionalObserverInfo);
				var result = MessageBox.Show(this, msg, GlyssenInfo.kProduct, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (result == DialogResult.No)
				{
					Logger.WriteEvent("User cancelled project creation because no books passed recommended checks.");
					SetProject(null);
					return;
				}
				Logger.WriteEvent("User proceeding with project creation although no books passed recommended checks.");
			}

			var projFilePath = Project.GetDefaultProjectFilePath(paratextProject);
			if (File.Exists(projFilePath))
				throw new Exception($"User should not have been able to select a Paratext project to create a new Glyssen project when that project already exists: {projFilePath}");

			var glyssenMetadata = ApplicationMetadata.Load(out Exception error);
			if (glyssenMetadata?.InactiveUnstartedParatextProjects != null)
			{
				var list = new List<string>(glyssenMetadata.InactiveUnstartedParatextProjects);
				if (list.Contains(paratextProjId))
				{
					list.Remove(paratextProjId);
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
				m_lblSettingsInfo.Text = m_project.IsQuoteSystemReadyForParse ? m_project.SettingsSummary : Localizer.GetString("MainForm.QuoteMarksNeedReview", "Quote marks need to be reviewed");
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
				m_lblPercentAssigned.Text = Localizer.GetString("MainForm.ReferenceTextUnavailable", "Reference text unavailable");
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
				Localizer.GetString("MainForm.CastSizePlanSingleNarrator", "Cast size is {0}, including 1 narrator",
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
				Localizer.GetString("MainForm.ActorsAssignedSingle", "1 voice actor identified, {0}",
				"{0} is an expression indicating the number of assigned actors");

			string assignedParameter;
			switch (assigned)
			{
				case 0:
					assignedParameter = Localizer.GetString("MainForm.NoneAssigned", "0 assigned",
						"This string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle");
					break;
				case 1:
					assignedParameter = Localizer.GetString("MainForm.OneAssigned", "1 assigned",
						"This string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle");
					break;
				default:
					assignedParameter = Format(Localizer.GetString("MainForm.MoreThanOneAssigned", "{0} assigned",
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
			// Temporarily clear this setting. If something goes horribly wrong loading/migrating the project,
			// we don't want to get the user into a situation where Glyssen is permanently hamstrung because it
			// always attempts to open the same (corrupt) project.
			Settings.Default.CurrentProject = null;
			Settings.Default.Save();
			if (m_project != null)
				UpdateButtons((m_project.ProjectState & ProjectState.ReadyForUserInteraction) == 0);
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
							var msg = Localizer.GetString("MainForm.FewerAssignedActorsAfterGeneration",
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
					dlgMessage = Localizer.GetString("DialogBoxes.ExportIncompleteScript.NotAssignedMessage",
						"One or more character groups have no voice actor assigned. Are you sure you want to export an incomplete script?");
					scriptIncomplete = true;
				}
				else if (!m_project.EveryAssignedGroupHasACharacter)
				{
					dlgMessage = Localizer.GetString("DialogBoxes.ExportIncompleteScript.EmptyGroupMessage",
						"One or more character groups have no characters in them. Are you sure you want to export a script?");
				}
			}
			else if (m_project.ProjectAnalysis.UserPercentAssigned < 100d)
			{
				dlgMessage = Format(Localizer.GetString("DialogBoxes.ExportIncompleteScript.CharacterAssignmentIncompleteMessage",
						"Character assignment is {0} complete. Are you sure you want to export a script?", "Parameter is a percentage."),
					MathUtilities.FormattedPercent(m_project.ProjectAnalysis.UserPercentAssigned, 1, 3));
				scriptIncomplete = true;
			}
			else if (m_project.ProjectAnalysis.NeedsReviewBlocks > 0)
			{
				dlgMessage = BlocksNeedReviewMessage + " " + UseNeedsReviewFilterHint + Environment.NewLine +
					Localizer.GetString("DialogBoxes.ExportIncompleteScript.BlocksNeedReviewMessage",
					"Any block marked for review will not be assigned to a voice actor in the script until an actual biblical " +
					"character is specified. Are you sure you want to export the script now?");
			}

			if (dlgMessage != null)
			{
				string dlgTitle = scriptIncomplete ?
					Localizer.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?"):
					Localizer.GetString("DialogBoxes.ExportIncompleteScript.Title", "Export Script?");

				dlgMessage += Environment.NewLine +
					Localizer.GetString("DialogBoxes.ExportIncompleteScript.MessageNote",
						"(Note: You can export the script again as many times as you want.)");
				export = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
			}

			return export;
		}

		private void SetupUiLanguageMenu()
		{
			m_uiLanguageMenu.DropDownItems.Clear();
			foreach (var lang in LocalizationManager.GetUILanguages(true))
			{
				var item = m_uiLanguageMenu.DropDownItems.Add(lang.NativeName);
				item.Tag = lang;
				string languageId = ((L10NCultureInfo)item.Tag).IetfLanguageTag;
				item.Click += ((a, b) =>
				{
					Analytics.Track("SetUiLanguage", new Dictionary<string, string> {{"uiLanguage", languageId}, {"reapplyLocalizations", "true"}});
					Logger.WriteEvent($"UI language changed to {languageId}.");

					LocalizationManager.SetUILanguage(languageId, true);
					Settings.Default.UserInterfaceLanguage = languageId;
					item.Select();
					m_uiLanguageMenu.Text = ((L10NCultureInfo)item.Tag).NativeName;
				});
				if (languageId == Settings.Default.UserInterfaceLanguage)
				{
					m_uiLanguageMenu.Text = ((L10NCultureInfo)item.Tag).NativeName;
				}
			}

			m_uiLanguageMenu.DropDownItems.Add(new ToolStripSeparator());
			var menu = m_uiLanguageMenu.DropDownItems.Add(Localizer.GetString("MainForm.MoreMenuItem",
				"More...", "Last item in menu of UI languages"));
			menu.Click += ((a, b) =>
			{
				Program.PrimaryLocalizationManager.ShowLocalizationDialogBox(false);
				SetupUiLanguageMenu();
			});
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

			using (var viewModel = new AssignCharacterViewModel(m_project))
			using (var dlg = new AssignCharacterDlg(viewModel))
			{
				LogDialogDisplay(dlg);
				dlg.ShowDialog(this);
			}
			Cursor = origCursor;

			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
			SaveCurrentProject(true);
		}

		private void SelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDlg(m_project, m_paratextScrTextWrapperForRecentlyCreatedProject))
			{
				if (ShowModalDialogWithWaitCursor(dlg) == DialogResult.OK)
				{
					m_project.ClearAssignCharacterStatus();
					m_project.Analyze();
					UpdateDisplayOfProjectInfo();
					SaveCurrentProject(true);
				}
			}
			m_paratextScrTextWrapperForRecentlyCreatedProject = null;
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
						{"paratextPojectName", m_project.ParatextProjectName},
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
					MessageBox.Show(this, Format(Localizer.GetString("Project.OkayToClearExistingRefBlocksThatCannotBeMigrated",
					"This project has been changed to use the {0} reference text, but some blocks were already matched to " +
					"the previous reference text. Some of those matches cannot be migrated, which means that the reference " +
					"text data for those blocks is not in the correct language. " +
					"To avoid probable confusion, would you like to allow {1} to clear the matches that cannot be migrated properly?",
					"Param 0: name of language of new reference text; " +
					"Param 1: \"Glyssen\" (product name)"), m_project.UiReferenceTextName, GlyssenInfo.kProduct),
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
			(m_project.ProjectAnalysis.NeedsReviewBlocks > 1 ?
			Format(Localizer.GetString("MainForm.BlocksNeedReviewMessage",
			"There are {0} blocks in this project that need review before finalizing the script.")) :
			Localizer.GetString("MainForm.OneBlockNeedReviewMessage",
				"There is one block in this project that needs review before finalizing the script."));

		private string UseNeedsReviewFilterHint =>
			Localizer.GetString("MainForm.UseNeedsReviewFilterHint",
			"(You can use the \"Needs review\" filter in Identify Speaking Parts to see which blocks still need attention.)")
			;

		private void AssignVoiceActors_Click(object sender, EventArgs e)
		{
			if (m_project.ProjectAnalysis.NeedsReviewBlocks > 0)
			{
				var msg = Localizer.GetString("Project.BlocksNeedReviewBeforeAssigning",
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
				else if (!ResolveNullReferenceText(Format(Localizer.GetString("Project.TemporarilyUseEnglishReferenceText",
					"To continue and temporarily use the English reference text, click {0}.", "Param is \"Ignore\" button label  (in the current Windows locale)"),
					MessageBoxStrings.IgnoreButton)))
				{
					return;
				}
			}

			try
			{
				var exporter = new ProjectExporter(m_project, GlyssenSettingsProvider.ExportSettingsprovider);

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
			var customReferenceTextFolder = m_project.ReferenceTextProxy.ProjectFolder;
			if (customReferenceTextFolder.Any(c => c == ' '))
				customReferenceTextFolder = customReferenceTextFolder.Replace(" ", "\u00A0");
			customReferenceTextFolder = "file://" + customReferenceTextFolder;

			while (m_project.ReferenceText == null)
			{
				string msg;
				var msgFmt = Localizer.GetString("Project.UnavailableReferenceText",
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
				msg = Format(msgFmt, m_project.UiReferenceTextName, customReferenceTextFolder, MessageBoxStrings.RetryButton,
					projectSettingsDlgTitle, referenceTextTabName);
				if (ignoreOptionText != null)
					msg += "\n\n" + ignoreOptionText;
				Logger.WriteEvent(msg);
				switch (FlexibleMessageBox.Show(msg, GlyssenInfo.kProduct,
					ignoreOptionText == null ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning,
					(sender, e) => { FileSystemUtils.SafeCreateAndOpenFolder(e.LinkText); }))
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
				m_project.PrepareForExport();

				var sourceDir = Path.GetDirectoryName(m_project.ProjectFilePath);

				Debug.Assert(sourceDir != null);
				Debug.Assert(sourceDir.StartsWith(GlyssenInfo.BaseDataFolder));

				var nameInZip = sourceDir.Substring(GlyssenInfo.BaseDataFolder.Length);

				var share = Path.Combine(GlyssenInfo.BaseDataFolder, "share");
				Directory.CreateDirectory(share);

				var saveAsName = Path.Combine(share, m_project.LanguageIsoCode + "_" + m_project.Name) + ProjectBase.kShareFileExtension;

				using (var zip = new ZipFile())
				{
					zip.AddDirectory(sourceDir, nameInZip);
					zip.Save(saveAsName);
				}

				if (m_project.ReferenceTextProxy.Type == ReferenceTextType.Custom)
				{
					var msg = Localizer.GetString("MainForm.ExportedProjectUsesCustomReferenceText",
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
				m_project.ExportCompleted();
				Cursor.Current = Cursors.Default;
				m_tableLayoutPanel.Enabled = true;
			}

		}

		private void Import_Click(object sender, EventArgs e)
		{

			string importFile = null;

			// show the user an Open File dialog
			using (var ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = Path.Combine(GlyssenInfo.BaseDataFolder, "share");
				ofd.Filter = Format("Glyssen shares (*{0})|*{0}|All files (*.*)|*.*", ProjectBase.kShareFileExtension);
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
				if (zip.Entries.Count <= 0) return;

				var path = zip.Entries.FirstOrDefault(ze => ze.IsDirectory);
				var targetDir = Path.Combine(GlyssenInfo.BaseDataFolder, path.FileName);

				var projectFileName = path.FileName.Split('/').First() + Constants.kProjectFileExtension;
				var projectFilePath = Path.Combine(targetDir, projectFileName);

				// warn the user if data will be overwritten
				if (RobustFile.Exists(projectFilePath))
				{
					var msg = Localizer.GetString("MainForm.ImportWarning",
							"Warning: You are about to import a project that already exists. If you continue, the existing project files will be replaced by the files being imported, which " +
							"might result in loss of data. Do you want to continue and overwrite the existing files?");
					Logger.WriteEvent(msg + " " + projectFilePath);

					if (MessageBox.Show(msg, GlyssenInfo.kProduct, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
						return;
				}

				// close the current project
				SetProject(null);

				zip.ExtractAll(GlyssenInfo.BaseDataFolder, ExtractExistingFileAction.OverwriteSilently);

				// open the imported project
				if (RobustFile.Exists(projectFilePath))
					LoadProject(projectFilePath, () =>
					{
						if (m_project.ReferenceTextProxy.Missing)
						{
							var msg = Localizer.GetString("MainForm.ImportedProjectUsesMissingReferenceText",
								"The imported project uses a custom reference text ({0}) that is not available on this computer.\nFor best results, " +
								"close {1} and install the reference text here:" +
								"\n    {2}\n" +
								"\nThen restart {1} to continue working with this project.",
								"Param 0: name of missing reference text; Param 1: \"Glyssen\"; Param 2: Path to Local Reference Texts folder");
							FlexibleMessageBox.Show(this, Format(msg, m_project.ReferenceTextProxy.CustomIdentifier, ProductName,
								"file://" + m_project.ReferenceTextProxy.ProjectFolder),
								ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning, (sender, e) => { FileSystemUtils.SafeCreateAndOpenFolder(e.LinkText); });
						}
					});
			}
		}
	}
}
