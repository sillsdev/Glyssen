using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Controls;
using Glyssen.Dialogs;
using Glyssen.Properties;
using Glyssen.Rules;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using SIL.DblBundle;
using SIL.IO;
using SIL.Windows.Forms.Miscellaneous;

namespace Glyssen
{
	public partial class MainForm : CustomForm
	{
		private Project m_project;
		private string m_percentAssignedFmt;
		private string m_actorsAssignedFmt;
		private string m_exportButtonFmt;

		public MainForm()
		{
			InitializeComponent();

			SetupUiLanguageMenu();
			m_toolStrip.Renderer = new NoBorderToolStripRenderer();
			m_uiLanguageMenu.ToolTipText = LocalizationManager.GetString("MainForm.UILanguage", "User-interface Language");

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized; // Don't need to unsubscribe since this object will be around as long as the program is running.
		}

		private void SetProject(Project project)
		{
			if (m_project != null)
				m_project.ProjectStateChanged -= FinishSetProjectIfReady;

			m_project = project;

			if (m_project != null)
				m_project.ProjectStateChanged += FinishSetProjectIfReady;

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

		protected void HandleStringsLocalized()
		{
			m_percentAssignedFmt = m_lblPercentAssigned.Text;
			m_actorsAssignedFmt = m_lblActorsAssigned.Text;
			m_exportButtonFmt = m_btnExport.Text;
			UpdateLocalizedText();
			if (m_project != null)
				m_project.ProjectCharacterVerseData.HandleStringsLocalized();
		}

		private void UpdateButtons(bool readOnly)
		{
			bool validProject = m_project != null;
			m_btnOpenProject.Enabled = !readOnly;
			m_imgCheckOpen.Visible = validProject;
			m_btnSettings.Enabled = !readOnly && validProject;
			m_imgCheckSettings.Visible = m_btnSettings.Enabled && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed &&
				m_project.IsQuoteSystemReadyForParse;
			m_btnSelectBooks.Enabled = !readOnly && validProject && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed;
			m_imgCheckBooks.Visible = m_btnSelectBooks.Enabled && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed;
			m_btnAssign.Enabled = !readOnly && m_imgCheckSettings.Visible && m_imgCheckBooks.Visible;
			m_imgCheckAssignCharacters.Visible = m_btnAssign.Enabled && m_project.ProjectAnalysis.UserPercentAssigned == 100d;
			m_btnExport.Enabled = !readOnly && m_imgCheckAssignCharacters.Visible;
			m_btnAssignVoiceActors.Visible = Environment.GetEnvironmentVariable("Glyssen_ProtoscriptOnly", EnvironmentVariableTarget.User) == null;
			m_btnAssignVoiceActors.Enabled = m_btnExport.Enabled;
			m_imgCheckAssignActors.Visible = m_btnAssignVoiceActors.Enabled && m_project.IsVoiceActorScriptReady;
			m_lnkExit.Enabled = !readOnly;
		}

		private void ResetUi()
		{
			m_btnSelectBooks.Enabled = false;
			m_btnSettings.Enabled = false;
			m_btnAssign.Enabled = false;
			m_btnExport.Enabled = false;
			m_btnAssignVoiceActors.Enabled = false;
			m_imgCheckOpen.Visible = false;
			m_imgCheckSettings.Visible = false;
			m_imgCheckBooks.Visible = false;
			m_imgCheckAssignCharacters.Visible = false;
			m_imgCheckAssignActors.Visible = false;
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
				if (result == DialogResult.OK)
				{
					switch (dlg.Type)
					{
						case OpenProjectDlg.ProjectType.ExistingProject:
							LoadProject(dlg.SelectedProject);
							break;
						case OpenProjectDlg.ProjectType.TextReleaseBundle:
							LoadBundle(dlg.SelectedProject);
							break;
						default:
							MessageBox.Show("Sorry - not implemented yet");
							break;
					}
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

		private void LoadProject(string filePath)
		{
			if (!LoadAndHandleApplicationExceptions(() => SetProject(Project.Load(filePath))))
				SetProject(null);
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
				Directory.GetDirectories(publicationFolder).Any(f => Directory.GetFiles(f, "*" + Project.kProjectFileExtension)
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

		public void UpdateLocalizedText()
		{
			m_lblProjectInfo.Text = m_project != null ? m_project.ProjectSummary : String.Empty;

			if (m_project != null && m_project.ProjectSettingsStatus == ProjectSettingsStatus.Reviewed)
				m_lblSettingsInfo.Text = m_project.IsQuoteSystemReadyForParse ? m_project.SettingsSummary : LocalizationManager.GetString("MainForm.QuoteMarksNeedReview", "Quote marks need to be reviewed");
			else
				m_lblSettingsInfo.Text = String.Empty;

			m_lblBookSelectionInfo.Text = m_project != null && m_project.BookSelectionStatus == BookSelectionStatus.Reviewed ? m_project.BookSelectionSummary : String.Empty;

			UpdateDisplayOfActorsAssigned();

			m_btnExport.Text = string.Format(m_exportButtonFmt, m_btnAssignVoiceActors.Visible ? "6" : "5");

			UpdateDisplayOfPercentAssigned();
		}

		private void UpdateDisplayOfActorsAssigned()
		{
			if (!m_btnAssignVoiceActors.Visible || !m_btnAssignVoiceActors.Enabled || m_project == null)
			{
				m_lblActorsAssigned.Text = string.Empty;
				return;
			}

			int actors = m_project.VoiceActorList.Actors.Count;
			int assigned = m_project.CharacterGroupList.CountVoiceActorsAssigned();
			m_lblActorsAssigned.Text = string.Format(m_actorsAssignedFmt, actors, assigned);
		}

		private void UpdateDisplayOfPercentAssigned()
		{
			if (!m_btnAssign.Enabled)
			{
				m_lblPercentAssigned.Text = string.Empty;
				return;
			}

			double percentAssigned = 0;
			if (m_project != null && m_project.ProjectAnalysis != null)
				percentAssigned = m_project.ProjectAnalysis.UserPercentAssigned;
			m_lblPercentAssigned.Text = percentAssigned > 0 ? string.Format(m_percentAssignedFmt, percentAssigned) : string.Empty;
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

		private void SaveCurrentProject()
		{
			if (m_project != null)
				m_project.Save();
		}

		private void Export_Click(object sender, EventArgs e)
		{
			EnsureGroupsAreInSynchWithCharactersInUse();

			var exporter = new ProjectExporter(m_project);

			bool export = true;
			if (exporter.IncludeVoiceActors)
			{
				string dlgMessage = null;
				string dlgTitle = null;
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
				else if (m_project.HasUnusedActor)
				{
					dlgMessage = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.UnusedActorMessage",
						"One or more voice actors have not been assigned to a character group. Are you sure you want to export a script?");
					dlgTitle = LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.Title", "Export Script?");
				}
				if (dlgMessage != null)
				{
					dlgMessage += Environment.NewLine +
						LocalizationManager.GetString("DialogBoxes.ExportIncompleteScript.MessageNote",
						"(Note: You can export the script again as many times as you want.)");
					export = MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes;
				}
			}
			if (export)
				using (var dlg = new ExportDlg(exporter))
					dlg.ShowDialog(this);
		}

		private void EnsureGroupsAreInSynchWithCharactersInUse()
		{
			if (!m_project.CharacterGroupList.CharacterGroups.Any())
				return;
			var adjuster = new CharacterGroupsAdjuster(m_project);
			if (adjuster.GroupsAreNotInSynchWithData)
			{
				string dlgMessage = String.Format(LocalizationManager.GetString("DialogBoxes.GroupsAreNotInSynchWithData.Message",
					"There have been changes to this project. The character groups no longer match the characters. {0} must update the groups in one of two ways:" +
					Environment.NewLine + Environment.NewLine +
					"{0} can start fresh, optimizing the number and composition of character groups based on the characters in the script and the voice actors in this project. " +
					"This is usually the recommended option." +
					"However, if the previously generated groups were manually changed most of these changes will probably be lost." +
					Environment.NewLine + Environment.NewLine +
					"Alternatively, {0} can just make the minimal changes to remove unused characters and put any new characters in a single group. This will probably require " +
					"substantial manual customization to avoid problems. If this proves too difficult, the groups can be re-optimized later by clicking {1} in the {2} dialog box." +
					Environment.NewLine + Environment.NewLine +
					"Would you like {0} to do the recommended action and create new character groups now?"),
					ProductName,
					LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.UpdateCharacterGroups",
						"Update Character Groups...").TrimEnd('.'),
					LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.WindowTitle", "Voice Actor Assignment"));

				if (MessageBox.Show(dlgMessage, ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					using (var progressDialog = new GenerateGroupsProgressDialog(m_project, (s, e) => adjuster.FullyRegenerateGroups(), false))
						progressDialog.ShowDialog();
				}
				else
					adjuster.MakeMinimalAdjustments();
			}
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
					dlg.ShowDialog();
			m_project.Analyze();
			UpdateDisplayOfProjectInfo();
			SaveCurrentProject();
		}

		private void SelectBooks_Click(object sender, EventArgs e)
		{
			using (var dlg = new ScriptureRangeSelectionDlg(m_project))
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					m_project.ClearAssignCharacterStatus();
					m_project.Analyze();
					UpdateDisplayOfProjectInfo();
					SaveCurrentProject();
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
					LoadProject(project.ProjectFilePath);
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
				dlg.ShowDialog();
			}
		}

		private void AssignVoiceActors_Click(object sender, EventArgs e)
		{
			// TODO: Eventually, this should be called when the user requests that all overrides be reverted to the defaults.
			//m_project.UseDefaultForUnresolvedMultipleChoiceCharacters();

			if (m_project.VoiceActorStatus == VoiceActorStatus.UnProvided)
			{
				var actorInfoViewModel = new VoiceActorInformationViewModel(m_project);

				using (var dlg = new VoiceActorInformationDlg(actorInfoViewModel))
					if (dlg.ShowDialog() == DialogResult.OK)
						m_project.VoiceActorStatus = VoiceActorStatus.Provided;
				SaveCurrentProject();
			}
			else
			{
				EnsureGroupsAreInSynchWithCharactersInUse();
			}

			if (m_project.VoiceActorStatus == VoiceActorStatus.Provided)
			{
				using (var dlg = new VoiceActorAssignmentDlg(m_project))
					dlg.ShowDialog();
				SaveCurrentProject();
			}
			UpdateDisplayOfProjectInfo();
		}

		public class NoBorderToolStripRenderer : ToolStripProfessionalRenderer
		{
			protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
			{
			}
		}
	}
}
