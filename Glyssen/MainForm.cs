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
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Character;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Rules;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using SIL.DblBundle;
using SIL.IO;
using SIL.Progress;
using SIL.Windows.Forms.Miscellaneous;
using Ionic.Zip;

namespace Glyssen
{
	public partial class MainForm : FormWithPersistedSettings
	{
		private Project m_project;
		private CastSizePlanningViewModel m_projectCastSizePlanningViewModel;
		private string m_percentAssignedFmt;
		private string m_actorsAssignedFmt;
		private string m_castSizeFmt;
		private readonly List<Tuple<Button, string>> m_buttonFormats = new List<Tuple<Button, string>>();

		public MainForm(IReadOnlyList<string> args)
		{
			InitializeComponent();

			SetupUiLanguageMenu();
			m_toolStrip.Renderer = new NoBorderToolStripRenderer();
			m_uiLanguageMenu.ToolTipText = LocalizationManager.GetString("MainForm.UILanguage", "User-interface Language");

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized; // Don't need to unsubscribe since this object will be around as long as the program is running.

			m_lastExportLocationLink.Text = string.Empty;

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
				m_project.ProjectStateChanged -= FinishSetProjectIfReady;
				m_project.CharacterGroupCollectionChanged -= UpdateDisplayOfCastSizePlan;
				m_project.CharacterGroupCollectionChanged -= ClearCastSizePlanningViewModel;
				m_project.CharacterStatisticsCleared -= ClearCastSizePlanningViewModel;
			}

			m_projectCastSizePlanningViewModel = null;
			m_project = project;

			if (m_project != null)
			{
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
						using (var dlg = new UnappliedSplitsDlg(m_project.Name, viewModel, m_project.IncludedBooks))
							dlg.ShowDialog(this);

				Settings.Default.CurrentProject = m_project.ProjectFilePath;
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
			bool validProject = m_project != null;
			m_btnOpenProject.Enabled = !readOnly;
			m_imgCheckOpen.Visible = validProject;
			m_btnSettings.Enabled = !readOnly && validProject && m_project.ProjectFileIsWritable;
			m_imgCheckSettings.Visible = m_btnSettings.Enabled && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.IsQuoteSystemReadyForParse;
			m_btnSelectBooks.Enabled = !readOnly && validProject && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.ProjectFileIsWritable;
			m_imgCheckBooks.Visible = m_btnSelectBooks.Enabled && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed;
			m_btnIdentify.Enabled = !readOnly && m_imgCheckSettings.Visible && m_imgCheckBooks.Visible;
			m_imgCheckAssignCharacters.Visible = m_btnIdentify.Enabled && (int)(m_project.ProjectAnalysis.UserPercentAssigned) == 100;
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

			m_exportMenu.Enabled = validProject;
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

			m_lblProjectInfo.Text = string.Empty;
			m_lblSettingsInfo.Text = string.Empty;
			m_lblBookSelectionInfo.Text = string.Empty;
			m_lblPercentAssigned.Text = string.Empty;
			m_lblCastSizePlan.Text = string.Empty;
			m_lblActorsAssigned.Text = string.Empty;
			m_lastExportLocationLink.Text = string.Empty;

			m_lblFilesAreHere.Visible = false;

			m_exportMenu.Enabled = false;
		}

		private void HandleOpenProject_Click(object sender, EventArgs e)
		{
			SaveCurrentProject();
			ShowOpenProjectDialog();
		}

		private void ShowOpenProjectDialog()
		{
			using (var dlg = new OpenProjectDlg(m_project))
			{
				var result = dlg.ShowDialog(this);
				if (result != DialogResult.OK) return;

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

		private void MainForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject) || !File.Exists(Settings.Default.CurrentProject))
				SetProject(null);
			else
				LoadProject(Settings.Default.CurrentProject);		
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

		private void LoadProject(string filePath)
		{
			if (!LoadAndHandleApplicationExceptions(() => SetProject(Project.Load(filePath))))
				SetProject(null);

			m_lastExportLocationLink.Text = m_project != null ? m_project.Status.LastExportLocation : string.Empty;
			m_lblFilesAreHere.Visible = m_lastExportLocationLink.Text != string.Empty;
		}

		private void LoadBundle(string bundlePath)
		{
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
				Directory.GetDirectories(publicationFolder).Any(f => Directory.GetFiles(f, "*" + ProjectBase.kProjectFileExtension)
					.Any(filename => GlyssenDblTextMetadata.GetRevisionOrChangesetId(filename) == bundle.Metadata.RevisionOrChangesetId)))
			{
				using (var dlg = new SelectExistingProjectDlg(bundle))
				{
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

			var recordingProjectName = Path.GetFileName(Path.GetDirectoryName(projFilePath));
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
				recordingProjectName = string.Format("{0} (Rev {1})", baserecordingProjectName, bundle.Metadata.Revision);
				var path = Project.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
				for (int i = 1; File.Exists(path); i++)
				{
					recordingProjectName = string.Format("{0} (Rev {1}.{2})", baserecordingProjectName, bundle.Metadata.Revision, i);
					path = Project.GetProjectFilePath(bundle.LanguageIso, bundle.Id, recordingProjectName);
				}
			}

			Versification.Table.HandleVersificationLineError = null;
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
				var msg = string.Format(LocalizationManager.GetString("Project.InvalidVersificationFile",
					"Invalid versification file in text release bundle. Unable to create project.\r\n" +
					"Text release Bundle: {0}\r\n" +
					"Versification file: {1}\r\n" +
					"Error: {2}"),
					bundlePath, DblBundleFileUtils.kVersificationFileName, error);
				MessageBox.Show(this, msg, Program.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				SetProject(null);
			}

			bundle.Dispose();
		}

		private bool LoadAndHandleApplicationExceptions(Action loadCommand)
		{
			try
			{
				loadCommand();
			}
			catch (ApplicationException ex)
			{
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
			m_lblProjectInfo.Text = m_project != null ? m_project.ProjectSummary : String.Empty;

			if (m_project != null && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed)
				m_lblSettingsInfo.Text = m_project.IsQuoteSystemReadyForParse ? m_project.SettingsSummary : LocalizationManager.GetString("MainForm.QuoteMarksNeedReview", "Quote marks need to be reviewed");
			else
				m_lblSettingsInfo.Text = String.Empty;

			m_lblBookSelectionInfo.Text = m_project != null && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed ? m_project.BookSelectionSummary : String.Empty;

			UpdateDisplayOfPercentOfCharactersAssigned();
			UpdateDisplayOfCastSizePlan(null, null);
			UpdateDisplayOfActorsAssigned();

			// insert button numbers
			var buttonNumber = 1;
			foreach (var buttonFmtInfo in m_buttonFormats.Where(b => !b.Item1.IsDisposed))
			{
				var btn = buttonFmtInfo.Item1;
				btn.Text = string.Format(buttonFmtInfo.Item2, buttonNumber++);
			}
		}

		private void UpdateDisplayOfPercentOfCharactersAssigned()
		{
			if (!m_btnIdentify.Enabled)
			{
				m_lblPercentAssigned.Text = string.Empty;
				return;
			}

			double percentAssigned = 0;
			if (m_project != null && m_project.ProjectAnalysis != null)
				percentAssigned = m_project.ProjectAnalysis.UserPercentAssigned;
			m_lblPercentAssigned.Text = percentAssigned > 0 ? string.Format(m_percentAssignedFmt, percentAssigned) : string.Empty;
		}

		private void UpdateDisplayOfCastSizePlan(object sender, EventArgs e)
		{
			if (!m_btnCastSizePlanning.Visible || !m_btnCastSizePlanning.Enabled || m_project == null ||
				!m_project.CharacterGroupList.CharacterGroups.Any())
			{
				m_lblCastSizePlan.Text = string.Empty;
				return;
			}

			var castSize = m_project.CharacterGroupList.CharacterGroups.Count;
			var narratorCount = m_project.CharacterGroupList.CharacterGroups.Count(g => g.GroupIdLabel == CharacterGroup.Label.Narrator);
			string format = (narratorCount > 1) ? m_castSizeFmt :
				LocalizationManager.GetString("MainForm.CastSizePlanSingleNarrator", "Cast size is {0}, including 1 narrator",
				"{0} is an expression indicating the total cast size");

			m_lblCastSizePlan.Text = String.Format(format, castSize, narratorCount);
		}

		private void UpdateDisplayOfActorsAssigned()
		{
			if (!m_btnAssignVoiceActors.Visible || !m_btnAssignVoiceActors.Enabled || m_project == null)
			{
				m_lblActorsAssigned.Text = string.Empty;
				return;
			}

			int actors = m_project.VoiceActorList.ActiveActors.Count();
			if (actors == 0)
			{
				m_lblActorsAssigned.Text = string.Empty;
				return;
			}
			int assigned = m_project.CharacterGroupList.CountVoiceActorsAssigned();
			string format = (actors > 1) ? string.Format(m_actorsAssignedFmt, actors, "{0}") :
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
					assignedParameter = LocalizationManager.GetString("MainForm.NoneAssigned", "1 assigned",
						"This string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle");
					break;
				default:
					assignedParameter = String.Format(LocalizationManager.GetString("MainForm.NoneAssigned", "{0} assigned",
						"{0} is the number of actors assigned. The resulting string is filled in as a parameter in MainForm.ActorsAssignedPlural or MainForm.ActorsAssignedSingle"),
						assigned);
					break;
			}

			m_lblActorsAssigned.Text = string.Format(format, assignedParameter);
		}

		private void UpdateProjectState()
		{
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
			var lastLocation = m_project.Status.LastExportLocation;
			m_lastExportLocationLink.Text = lastLocation;
			m_lblFilesAreHere.Visible = !string.IsNullOrEmpty(lastLocation);
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
			string dlgTitle = null;
			if (exporter.IncludeVoiceActors)
			{
				if (!m_project.IsVoiceActorAssignmentsComplete)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.NotAssignedMessage",
						"One or more character groups have no voice actor assigned. Are you sure you want to export an incomplete script?");
					dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?");
				}
				else if (!m_project.EveryAssignedGroupHasACharacter)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.EmptyGroupMessage",
						"One or more character groups have no characters in them. Are you sure you want to export a script?");
					dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.Title", "Export Script?");
				}
			}
			else if (m_project.ProjectAnalysis.UserPercentAssigned < 100d)
			{
				dlgMessage = string.Format(LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.CharacterAssignmentIncompleteMessage",
					"Character assignment is {0:N1}% complete. Are you sure you want to export a script?"), m_project.ProjectAnalysis.UserPercentAssigned);
				dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.TitleIncomplete", "Export Incomplete Script?");
			}

			if (dlgMessage != null)
			{
				dlgMessage += Environment.NewLine +
							  LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.MessageNote",
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
				string languageId = ((CultureInfo)item.Tag).IetfLanguageTag;
				item.Click += ((a, b) =>
				{
					Analytics.Track("SetUiLanguage", new Dictionary<string, string> { { "uiLanguage", languageId }, { "initialStartup", "true" } });

					LocalizationManager.SetUILanguage(languageId, true);
					Settings.Default.UserInterfaceLanguage = languageId;
					item.Select();
					m_uiLanguageMenu.Text = ((CultureInfo)item.Tag).NativeName;
				});
				if (languageId == Settings.Default.UserInterfaceLanguage)
				{
					m_uiLanguageMenu.Text = ((CultureInfo)item.Tag).NativeName;
				}
			}

			m_uiLanguageMenu.DropDownItems.Add(new ToolStripSeparator());
			var menu = m_uiLanguageMenu.DropDownItems.Add(LocalizationManager.GetString("MainForm.MoreMenuItem",
				"More...", "Last item in menu of UI languages"));
			menu.Click += ((a, b) =>
			{
				Program.LocalizationManager.ShowLocalizationDialogBox(false);
				SetupUiLanguageMenu();
			});
		}

		private void Assign_Click(object sender, EventArgs e)
		{
			using (var viewModel = new AssignCharacterViewModel(m_project))
				using (var dlg = new AssignCharacterDlg(viewModel))
					dlg.ShowDialog(this);
			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
			SaveCurrentProject(true);
		}

		private void SelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDlg(m_project))
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_project.ClearAssignCharacterStatus();
					m_project.Analyze();
					UpdateDisplayOfProjectInfo();
					SaveCurrentProject(true);
				}
		}

		private void Settings_Click(object sender, EventArgs e)
		{
			var model = new ProjectSettingsViewModel(m_project);
			using (var dlg = new ProjectSettingsDlg(model))
			{
				if (dlg.ShowDialog(this) != DialogResult.OK)
					return;

				m_project.UpdateSettings(model);
				SaveCurrentProject();

				if (dlg.UpdatedBundle != null)
				{
					Analytics.Track("UpdateProjectFromBundleData", new Dictionary<string, string>
					{
						{"language", m_project.LanguageIsoCode},
						{"ID", m_project.Id},
						{"recordingProjectName", m_project.Name},
						{"bundlePathChanged", (m_project.OriginalBundlePath != model.BundlePath).ToString()}
					});
					var project = m_project.UpdateProjectFromBundleData(dlg.UpdatedBundle);
					project.OriginalBundlePath = model.BundlePath;
					SetProject(project);
				}
			}
			UpdateDisplayOfProjectInfo();
		}

		private void Exit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Save is handled in FormClosing event
			Close();
		}

		private void About_Click(object sender, EventArgs e)
		{
			using (var dlg = new SILAboutBox(FileLocator.GetFileDistributedWithApplication("aboutbox.htm")))
			{
				dlg.ShowDialog(this);
			}
		}

		private void AssignVoiceActors_Click(object sender, EventArgs e)
		{
			// TODO: Eventually, this should be called when the user requests that all overrides be reverted to the defaults.
			//m_project.UseDefaultForUnresolvedMultipleChoiceCharacters();

			bool regenerateGroups = sender == m_btnCastSizePlanning;
			if (!regenerateGroups)
				EnsureGroupsAreInSynchWithCharactersInUse();

			if (!m_project.CharacterGroupList.CharacterGroups.Any())
				CharacterGroupGenerator.GenerateGroupsWithProgress(m_project, false, true, false, ProjectCastSizePlanningViewModel.SelectedCastSize);
			else if (regenerateGroups)
				CharacterGroupGenerator.GenerateGroupsWithProgress(m_project, true, false, false, ProjectCastSizePlanningViewModel.SelectedCastSize);

			bool launchCastSizePlanning;
			using (var dlg = new VoiceActorAssignmentDlg(new VoiceActorAssignmentViewModel(m_project)))
			{
				dlg.ShowDialog(this);
				launchCastSizePlanning = dlg.LaunchCastSizePlanningUponExit;
			}
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

		private void m_btnCastSizePlanning_Click(object sender, EventArgs e)
		{
			bool launchAssignVoiceActors = false;
			using (var dlg = new CastSizePlanningDlg(ProjectCastSizePlanningViewModel))
			{
				SaveCurrentProject();
				if (dlg.ShowDialog(this) == DialogResult.OK)
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

			var exporter = new ProjectExporter(m_project);

			if (!IsOkToExport(exporter))
				return;

			using (var dlg = getProjectScriptPresenterDlg(exporter))
			{
				dlg.ShowDialog(this);
				ShowLastLocation();
			}
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

				var sourceDir = Path.GetDirectoryName(m_project.ProjectFilePath);

				// ReSharper disable once PossibleNullReferenceException
				var nameInZip = sourceDir.Substring(Program.BaseDataFolder.Length);

				// make sure the share directory exists
				var share = Path.Combine(Program.BaseDataFolder, "share");
				Directory.CreateDirectory(share);

				var parts = nameInZip.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
				var saveAsName = Path.Combine(share, parts[0] + "_" + parts[parts.Length - 1] + ProjectBase.kShareFileExtension);

				using (var zip = new ZipFile())
				{
					zip.AddDirectory(sourceDir, nameInZip);
					zip.Save(saveAsName);
				}

				PathUtilities.SelectFileInExplorer(saveAsName);
			}
			finally
			{
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
				ofd.InitialDirectory = Path.Combine(Program.BaseDataFolder, "share");
				ofd.Filter = string.Format("Glyssen shares (*{0})|*{0}|All files (*.*)|*.*", ProjectBase.kShareFileExtension);
				ofd.RestoreDirectory = true;

				if (ofd.ShowDialog() == DialogResult.OK)
					importFile = ofd.FileName;
			}

			// if nothing was selected, return now
			if (string.IsNullOrEmpty(importFile))
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
			// open the zip file
			using (var zip = new ZipFile(importFile))
			{
				if (zip.Entries.Count <= 0) return;

				var path = zip.Entries.FirstOrDefault(ze => ze.IsDirectory);
				var targetDir = Path.Combine(Program.BaseDataFolder, path.FileName);

				// warn the user if data will be overwritten
				if (Directory.Exists(targetDir))
				{
					var msg = LocalizationManager.GetString("MainForm.ImportWarning",
						"WARNING: If you continue, your existing files will be replaced by the files you are importing, possibly resulting in loss of data. Do you want to continue and overwrite the existing files?");
					if (MessageBox.Show(msg, Program.kProduct, MessageBoxButtons.OKCancel) != DialogResult.OK)
						return;
				}

				// close the current project
				SetProject(null);

				zip.ExtractAll(Program.BaseDataFolder, ExtractExistingFileAction.OverwriteSilently);

				// open the imported project
				var projectFile = Directory.EnumerateFiles(targetDir, string.Format("*{0}", ProjectBase.kProjectFileExtension)).FirstOrDefault();
				if (File.Exists(projectFile))
					LoadProject(projectFile);
			}
		}
	}
}
