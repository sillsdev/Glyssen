using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Paratext;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;
using GlyssenFileBasedPersistence;
using L10NSharp;
using L10NSharp.XLiffUtils;
using L10NSharp.UI;
using SIL.Extensions;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.WritingSystems;
using static System.String;
using Analytics = DesktopAnalytics.Analytics;
using BlockNavigatorViewModel = GlyssenEngine.ViewModels.BlockNavigatorViewModel<System.Drawing.Font>;

namespace Glyssen.Dialogs
{
	public partial class ProjectSettingsDlg : FormWithPersistedSettings
	{
		private ProjectSettingsViewModel m_model;
		private WritingSystemSetupModel m_wsViewModel;

		private class ChapterAnnouncementItem
		{
			private string UiString { get; }
			public ChapterAnnouncement ChapterAnnouncement { get; }

			public ChapterAnnouncementItem(string uiString, ChapterAnnouncement chapterAnnouncement)
			{
				UiString = uiString;
				ChapterAnnouncement = chapterAnnouncement;
			}

			public override string ToString()
			{
				return UiString;
			}
		}

		public ProjectSettingsDlg(ProjectSettingsViewModel model, WritingSystemSetupModel wsViewModel)
		{
			InitializeComponent();

			m_txtRecordingProjectName.MaxLength = model.Project.MaxProjectNameLength;

			for (int i = 0; i < m_cboBookMarker.Items.Count; i++)
			{
				var chapterAnnouncement = (ChapterAnnouncement)i;
				m_cboBookMarker.Items[i] = new ChapterAnnouncementItem(LocalizationManager.GetDynamicString(GlyssenInfo.ApplicationId,
					"DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookMarkerComboBox.Items." + chapterAnnouncement,
					m_cboBookMarker.Items[i].ToString()), chapterAnnouncement);
			}

			IReadOnlyList<BookScript> books = model.Project.IncludedBooks.Any() ? model.Project.IncludedBooks : model.Project.Books;
			if (books.All(book => IsNullOrEmpty(book.PageHeader)))
				RemoveItemFromBookMarkerCombo(ChapterAnnouncement.PageHeader);
			if (books.All(book => IsNullOrEmpty(book.MainTitle)))
				RemoveItemFromBookMarkerCombo(ChapterAnnouncement.MainTitle1);

			LocalizeItemDlg<XLiffDocument>.StringsLocalized += HandleStringsLocalized;
			SetViewModel(model, wsViewModel);
			HandleStringsLocalized();
		}

		private void HandleStringsLocalized()
		{
			LoadReferenceTextOptions();
			LoadProjectDramatizationOptions();
			UpdateQuotePageDisplay();

			var fmt = m_linkLblChangeOmittedChapterAnnouncements.Text;
			var linkStartPos = fmt.IndexOf("{0}", StringComparison.OrdinalIgnoreCase);
			m_linkLblChangeOmittedChapterAnnouncements.Text = Format(fmt, m_tabPageScriptOptions.Text);
			if (linkStartPos >= 0 && m_tabPageScriptOptions.Text.Length > 0)
				m_linkLblChangeOmittedChapterAnnouncements.LinkArea = new LinkArea(linkStartPos, m_tabPageScriptOptions.Text.Length);
			else
				m_linkLblChangeOmittedChapterAnnouncements.LinkArea = default;
			if (m_model.IsLiveParatextProject)
				m_lblOriginalSource.Text = Format(LocalizationManager.GetString(
					"DialogBoxes.ProjectSettingsDlg.SourceLabelForParatextProject", "{0} project:", "\"Paratext\" (product name)"),
					ParatextScrTextWrapper.kParatextProgramName);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			var project = m_model.Project;
			if (IsHandleCreated && m_model.IsLiveParatextProject &&
				(project.QuoteSystemStatus & QuoteSystemStatus.ParseReady) != 0)
			{
				var paratextProj = project.GetParatextScrTextWrapper();
				if (paratextProj != null &&
					!project.WritingSystem.QuotationMarks.SequenceEqual(project.GetQuotationMarksWithFullySpecifiedContinuers(paratextProj.QuotationMarks)))
				{
					string msg = Format(LocalizationManager.GetString("Project.ParatextQuoteSystemChanged",
							"The quotation mark settings in {0} project {1} no longer match the settings in this {2} project. To update " +
							"this project to match the {0} project settings (and get the latest versions of the text), click {3}.",
							"Param 0: \"Paratext\" (product name); " +
							"Param 1: Paratext project short name (unique project identifier); " +
							"Param 2: \"Glyssen\" (product name); " +
							"Param 3: localized name of the \"Update\" button"),
						ParatextScrTextWrapper.kParatextProgramName,
						m_model.ParatextProjectName,
						GlyssenInfo.Product,
						LocalizedUpdateButtonName);
					MessageBox.Show(this, msg, Text, MessageBoxButtons.OK);
				}
			}
		}

		private void LoadProjectDramatizationOptions()
		{
			var optionList = new List<KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>>
			{
				new KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>(ExtraBiblicalMaterialSpeakerOption.Narrator,
					LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ExtraBiblicalSpeakerOption.Narrator", "Narrator")),
				new KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>(ExtraBiblicalMaterialSpeakerOption.MaleActor,
					LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ExtraBiblicalSpeakerOption.MaleActor", "Male Actor")),
				new KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>(ExtraBiblicalMaterialSpeakerOption.FemaleActor,
					LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ExtraBiblicalSpeakerOption.FemaleActor", "Female Actor")),
				new KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>(ExtraBiblicalMaterialSpeakerOption.ActorOfEitherGender,
					LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ExtraBiblicalSpeakerOption.EitherGender", "Either Gender")),
				new KeyValuePair<ExtraBiblicalMaterialSpeakerOption, string>(ExtraBiblicalMaterialSpeakerOption.Omitted,
					LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ExtraBiblicalSpeakerOption.Omitted", "Omitted"))
			};

			m_bookIntro.DataSource = new BindingSource(optionList, null);
			m_bookIntro.DisplayMember = "Value";
			m_bookIntro.ValueMember = "Key";

			m_sectionHeadings.DataSource = new BindingSource(optionList, null);
			m_sectionHeadings.DisplayMember = "Value";
			m_sectionHeadings.ValueMember = "Key";

			m_titleChapters.DataSource = new BindingSource(optionList, null);
			m_titleChapters.DisplayMember = "Value";
			m_titleChapters.ValueMember = "Key";
		}

		private void LoadReferenceTextOptions()
		{
			// Saving the selection (if any) here is required to deal with the (rare) case where
			// the user does on-the-fly localization. When there is an existing selection that
			// differs from the one that was set when we came into this dialog and we reset the
			// data source, the selection gets lost. So we need to remember which one was selected
			// and restore it below.
			var currentSelection = SelectedReferenceText;
			var dataSource = new Dictionary<string, ReferenceTextProxy>();
			foreach (var refTextId in ReferenceTextProxy.AllAvailable)
			{
				string key;
				if (refTextId.Type == ReferenceTextType.Custom)
				{
					var fmt = (refTextId.Missing) ?
						LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.MissingReferenceText", "Missing: {0}") :
						LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.CustomReferenceText", "Custom: {0}");
					key = Format(fmt, refTextId.CustomIdentifier);
				}
				else
				{
					key = refTextId.Type.ToString();
				}

				dataSource.Add(key, refTextId);
			}
			m_ReferenceText.DataSource = new BindingSource(dataSource, null);
			m_ReferenceText.ValueMember = "Value";
			m_ReferenceText.DisplayMember = "Key";

			SelectCurrentReferenceText(currentSelection ?? m_model.Project.ReferenceTextProxy);
		}

		private void ProjectSettingsDlg_Load(object sender, EventArgs e)
		{
			TileFormLocation();
		}

		private void RemoveItemFromBookMarkerCombo(ChapterAnnouncement chapterAnnouncement)
		{
			var i = GetIndexOfItemFromBookMarkerCombo(chapterAnnouncement);
			if (i != -1)
				m_cboBookMarker.Items.RemoveAt(i);
		}

		private int GetIndexOfItemFromBookMarkerCombo(ChapterAnnouncement chapterAnnouncement)
		{
			for (int i = 0; i < m_cboBookMarker.Items.Count; i++)
			{
				if (((ChapterAnnouncementItem)m_cboBookMarker.Items[i]).ChapterAnnouncement == chapterAnnouncement)
					return i;
			}
			return -1;
		}

		public GlyssenBundle UpdatedBundle { get; private set; }
		internal ParatextScrTextWrapper UpdatedParatextProject { get; private set; }

		public string ReferenceTextTabPageName => m_tabPageReferenceTexts.Text;

		private void SetViewModel(ProjectSettingsViewModel projectSettingsViewModel, WritingSystemSetupModel wsViewModel)
		{
			m_model = projectSettingsViewModel;
			m_wsViewModel = wsViewModel;
			RecordingProjectName = m_model.RecordingProjectName;
			AudioStockNumber = m_model.AudioStockNumber;
			m_txtOriginalSource.Text = m_model.IsLiveParatextProject ? m_model.ParatextProjectName : m_model.BundlePath;
			LanguageName = m_model.LanguageName;
			IsoCode = m_model.IsoCode;
			PublicationName = m_model.PublicationName;
			PublicationId = m_model.PublicationId;
			m_txtVersification.Text = m_model.Versification.Name;
			m_lblQuoteMarkSummary.Text = m_model.Project.QuoteSystem.ShortSummary;

			m_wsFontControl.BindToModel(wsViewModel);

			if (!IsNullOrWhiteSpace(m_model.SampleText))
				m_wsFontControl.TestAreaText = m_model.SampleText;

			// PG-433, 07 JAN 2016, PH: Disable some UI if project file is not writable
			var enableControls = m_model.Project.ProjectIsWritable;
			m_btnUpdateFromSource.Enabled =
				m_txtRecordingProjectName.Enabled =
					m_txtAudioStockNumber.Enabled = enableControls && !m_model.Project.IsSampleProject;
			m_btnQuoteMarkSettings.Enabled = enableControls;
			m_btnOk.Enabled = enableControls;
			m_wsFontControl.Enabled = enableControls;
			var i = GetIndexOfItemFromBookMarkerCombo(m_model.ChapterAnnouncementStyle);
			if (m_model.ChapterAnnouncementStyle == ChapterAnnouncement.ChapterLabel || i < 0)
			{
				m_rdoChapterLabel.Checked = true;
				m_cboBookMarker.SelectedIndex = 0;
			}
			else
				m_cboBookMarker.SelectedIndex = i;

			m_chkChapterOneAnnouncements.Checked = !m_model.SkipChapterAnnouncementForFirstChapter;
			m_chkAnnounceChaptersForSingleChapterBooks.Checked = !m_model.SkipChapterAnnouncementForSingleChapterBooks;

			SelectCurrentReferenceText(m_model.Project.ReferenceTextProxy);

			m_bookIntro.SelectedValue = m_model.Project.DramatizationPreferences.BookIntroductionsDramatization;
			m_sectionHeadings.SelectedValue = m_model.Project.DramatizationPreferences.SectionHeadDramatization;
			m_titleChapters.SelectedValue = m_model.Project.DramatizationPreferences.BookTitleAndChapterDramatization;
		}

		private void SelectCurrentReferenceText(ReferenceTextProxy currentReferenceText)
		{
			foreach (KeyValuePair<string, ReferenceTextProxy> kvp in m_ReferenceText.Items)
			{
				if (kvp.Value == currentReferenceText)
				{
					m_ReferenceText.SelectedItem = kvp;
					break;
				}
			}
		}

		private string RecordingProjectName
        {
            get => m_txtRecordingProjectName.Text.Trim();
			set => m_txtRecordingProjectName.Text = value.Trim();
		}

        private string AudioStockNumber
        {
            get => m_txtAudioStockNumber.Text.Trim();
			set => m_txtAudioStockNumber.Text = value;
		}

		private string LanguageName
		{
			set => m_txtLanguageName.Text = value ?? Empty;
		}

		private string IsoCode
		{
			get => (IsNullOrWhiteSpace(m_txtIso639_2_Code.Text)) ? "zzz" : m_txtIso639_2_Code.Text;
			set => m_txtIso639_2_Code.Text = value;
		}

		private string PublicationName
		{
			set => m_txtPublicationName.Text = value;
		}

		private string PublicationId
		{
			get => m_txtPublicationId.Text;
			set => m_txtPublicationId.Text = value;
		}

		private void UpdateQuotePageDisplay()
		{
			m_lblQuoteMarkSummary.Text = m_model.Project.QuoteSystem.ShortSummary;

			m_lblQuoteMarkReview.ForeColor = GlyssenColorPalette.ColorScheme.ForeColor;

			string quoteMarkReviewText = "";
			switch (m_model.Project.QuoteSystemStatus)
			{
				case QuoteSystemStatus.Obtained:
					quoteMarkReviewText = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ReviewQuoteMarks", "You may review the quote mark settings.");
					break;
				case QuoteSystemStatus.Guessed:
					quoteMarkReviewText = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.CarefullyReviewQuoteMarks", "Carefully review the quote mark settings.");
					m_lblQuoteMarkReview.ForeColor = GlyssenColorPalette.ColorScheme.Warning;
					break;
				case QuoteSystemStatus.Reviewed:
					quoteMarkReviewText = Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.QuoteMarksReviewed", "Quote mark settings were reviewed on {0}.", "{0} is a date"), m_model.Project.QuoteSystemDate.ToString("yyyy-MM-dd"));
					break;
				case QuoteSystemStatus.UserSet:
					quoteMarkReviewText = Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.QuoteMarksUpdated", "Quote mark settings were updated on {0}.", "{0} is a date"), m_model.Project.QuoteSystemDate.ToString("yyyy-MM-dd"));
					break;
			}
			m_lblQuoteMarkReview.Text = quoteMarkReviewText;
		}

		private void UpdateAnnouncementsPageDisplay()
		{
			var showChangeOmittedChapterAnnouncementsLabel =
				m_titleChapters.SelectedValue is ExtraBiblicalMaterialSpeakerOption extraBiblicalOption &&
				extraBiblicalOption == ExtraBiblicalMaterialSpeakerOption.Omitted;

			if (showChangeOmittedChapterAnnouncementsLabel)
			{
				m_linkLblChangeOmittedChapterAnnouncements.Visible = true;

				m_lblExampleSubsequentChapterAnnouncement.Text = "";
				m_lblExampleFirstChapterAnnouncement.Text = "";
				m_lblExampleSingleChapterAnnouncement.Text = "";
				m_lblExampleTitleForMultipleChapterBook.Text = "";
				m_lblExampleTitleForSingleChapterBook.Text = "";
			}
			else
			{
				m_linkLblChangeOmittedChapterAnnouncements.Visible = false;

				if (m_model != null)
				{
					m_lblExampleSubsequentChapterAnnouncement.Text = m_model.ExampleSubsequentChapterAnnouncement;
					m_lblExampleFirstChapterAnnouncement.Text = m_model.ExampleFirstChapterAnnouncement;
					m_lblExampleSingleChapterAnnouncement.Text = m_model.ExampleSingleChapterAnnouncement;
					m_lblExampleTitleForMultipleChapterBook.Text = m_model.ExampleTitleForMultipleChapterBook;
					m_lblExampleTitleForSingleChapterBook.Text = m_model.ExampleTitleForSingleChapterBook;
					bool displayWarning = m_model.ChapterAnnouncementIsStrictlyNumeric;
					m_lblChapterAnnouncementWarning.Visible = displayWarning;
					m_lblExampleSubsequentChapterAnnouncement.ForeColor = displayWarning
						? GlyssenColorPalette.ColorScheme.Warning
						: GlyssenColorPalette.ColorScheme.ForeColor;
				}
			}
		}

		private void HandleOkButtonClick(object sender, EventArgs e)
		{
			if (m_model.RecordingProjectName != RecordingProjectName && ProjectBase.Reader.ProjectExistsHaving(IsoCode, PublicationId, RecordingProjectName))
			{
				var msg =Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OverwriteProjectPrompt",
					"A {0} project with an ID of {1} and a Recording Project Name of {2} already exists for this language. Do you want to overwrite it?"),
					ProductName, m_txtPublicationId.Text, RecordingProjectName);
				if (MessageBox.Show(this, msg, ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
				{
					DialogResult = DialogResult.None;
					return;
				}

				var existingProjectPath = ProjectRepository.GetProjectFolderPath(IsoCode, PublicationId, RecordingProjectName);
				if (!RobustIO.DeleteDirectoryAndContents(existingProjectPath))
				{
					var failedMsg = Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OverwriteProjectFailed",
							"{0} was unable to delete all of the files in {1}. You can try to clean this up manually and then re-attempt saving these changes."),
						ProductName, existingProjectPath);
					MessageBox.Show(this, failedMsg, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
			}

            m_model.RecordingProjectName = RecordingProjectName;
            m_model.AudioStockNumber = AudioStockNumber;
			m_model.ChapterAnnouncementStyle = ChapterAnnouncementStyle;
			m_model.SkipChapterAnnouncementForFirstChapter = !m_chkChapterOneAnnouncements.Checked;
			m_model.Project.ReferenceTextProxy = SelectedReferenceText;

			m_model.Project.DramatizationPreferences.BookIntroductionsDramatization = (ExtraBiblicalMaterialSpeakerOption)m_bookIntro.SelectedValue;
			m_model.Project.DramatizationPreferences.SectionHeadDramatization = (ExtraBiblicalMaterialSpeakerOption)m_sectionHeadings.SelectedValue;
			m_model.Project.DramatizationPreferences.BookTitleAndChapterDramatization = (ExtraBiblicalMaterialSpeakerOption)m_titleChapters.SelectedValue;

			m_model.Project.ProjectSettingsStatus = ProjectSettingsStatus.Reviewed;
			DialogResult = DialogResult.OK;
			Close();
		}

		private ReferenceTextProxy SelectedReferenceText => ((KeyValuePair<string, ReferenceTextProxy>?)m_ReferenceText.SelectedItem)?.Value;

		private void HandleCancelButtonClick(object sender, EventArgs e)
		{
			// This ensures that any temporary override to this (which ultimately sets a static
			// member on Block) is reset if the user presses Cancel.
			m_model.RevertChapterAnnouncementStyle();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private ChapterAnnouncement ChapterAnnouncementStyle =>
			m_rdoChapterLabel.Checked ? ChapterAnnouncement.ChapterLabel :
				((ChapterAnnouncementItem)m_cboBookMarker.SelectedItem).ChapterAnnouncement;

		public string LocalizedGeneralTabName => m_tabPageGeneral.Text;
		public string LocalizedUpdateButtonName => m_btnUpdateFromSource.Text.Replace("...", Empty);

		private void m_btnQuoteMarkSettings_Click(object sender, EventArgs e)
		{
			var reparseOkay = false;
			if (m_model.IsLiveParatextProject)
			{
				reparseOkay = m_model.Project.QuoteSystemStatus != QuoteSystemStatus.Obtained;
			}
			else if (!m_model.Project.IsSampleProject)
			{
				if (m_model.Project.IsOkayToChangeQuoteSystem)
					reparseOkay = true;
				else
				{
					string msg = Format(LocalizationManager.GetString("Project.UnableToLocateTextBundleMsg",
							"The original text release bundle for the project is no longer in its original location ({0}). " +
							"The Quote Mark Settings cannot be modified without access to it."), m_model.Project.OriginalBundlePath) +
						Program.LocateBundleYourselfQuestion;
					string title = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle", "Message caption");
					if (DialogResult.Yes == MessageBox.Show(msg, title, MessageBoxButtons.YesNo))
						reparseOkay = SelectBundleForProjectDlg.GiveUserChanceToFindOriginalBundle(m_model.Project);
				}
			}

			BlockNavigatorViewModel viewModel = null;
			try
			{
				var fontProxy = new AdjustableFontProxy(m_wsViewModel.CurrentDefaultFontName,
					(int)m_wsViewModel.CurrentDefaultFontSize, m_wsViewModel.CurrentRightToLeftScript);
				if (m_model.Project.IncludedBooks.Any())
					viewModel = new BlockNavigatorViewModel(m_model.Project, BlocksToDisplay.AllExpectedQuotes, fontProxy, AdjustableFontProxy.GetFontProxyForReferenceText);
				using (var dlg = new QuotationMarksDlg(m_model.Project, viewModel, !reparseOkay, this))
				{
					MainForm.LogDialogDisplay(dlg);
					if (dlg.ShowDialog(this) == DialogResult.OK)
						UpdateQuotePageDisplay();
				}
			}
			finally
			{
				viewModel?.Dispose();
			}
		}

		private void m_btnUpdate_Click(object sender, EventArgs e)
		{
			if (m_model.IsLiveParatextProject)
			{
				UpdatedParatextProject = m_model.GetUpdatedParatextData(new WinformsParatextProjectLoadingAssistant(null, true));
				if (UpdatedParatextProject != null)
				{
					Logger.WriteEvent($"Updating project {m_lblRecordingProjectName} from Paratext data {m_model.ParatextProjectName}");
					HandleOkButtonClick(sender, e);
				}
				else
				{
					Analytics.Track("CancelledUpdateProjectFromParatextData", new Dictionary<string, string>
					{
						{"projectLanguage", m_model.IsoCode},
						{"paratextProjectName", m_model.ParatextProjectName},
						{"projectID", m_model.PublicationId},
						{"recordingProjectName", m_model.RecordingProjectName}
					});
				}
			}
			else
			{
				if (SelectBundleForProjectDlg.TryGetBundleName(m_model.RecordingProjectName, m_model.BundlePath, out string selectedBundlePath))
				{
					var bundle = new GlyssenBundle(selectedBundlePath);
					if (ConfirmProjectUpdateFromBundle(bundle))
					{
						Logger.WriteEvent($"Updating project {m_lblRecordingProjectName} from bundle {selectedBundlePath}");
						m_model.BundlePath = selectedBundlePath;
						UpdatedBundle = bundle;
						HandleOkButtonClick(sender, e);
					}
					else
					{
						Analytics.Track("CancelledUpdateProjectFromBundleData", new Dictionary<string, string>
						{
							{"bundleLanguage", bundle.LanguageIso},
							{"projectLanguage", m_model.IsoCode},
							{"bundleID", bundle.Id},
							{"projectID", m_model.PublicationId},
							{"recordingProjectName", m_model.RecordingProjectName},
							{"bundlePathChanged", (m_model.BundlePath != selectedBundlePath).ToString()}
						});
						bundle.Dispose();
					}
				}
			}
		}

		private bool ConfirmProjectUpdateFromBundle(GlyssenBundle bundle)
		{
			StringBuilder msg = new StringBuilder();
			var oldValueFormat = LocalizationManager.GetString("Project.UpdateFromBundle.OldValue",
				"Old value: {0}");
			var newValueFormat = LocalizationManager.GetString("Project.UpdateFromBundle.NewValue",
				"New value: {0}");

			if (bundle.LanguageIso != m_model.IsoCode)
			{
				msg.Append(Environment.NewLine);
				msg.Append(LocalizationManager.GetString("Project.UpdateFromBundle.Language", "Language"));
				msg.Append(Environment.NewLine);
				msg.Append("    ");
				var oldLanguage = IsNullOrEmpty(m_model.LanguageName) ? m_model.IsoCode :
					$"{m_model.LanguageName} ({m_model.IsoCode})";
				msg.AppendFormat(oldValueFormat, oldLanguage);
				msg.Append(Environment.NewLine);
				msg.Append("    ");
				msg.AppendFormat(newValueFormat, bundle.LanguageAsString);
				msg.Append(Environment.NewLine);
			}

			if (bundle.Id != m_model.PublicationId)
			{
				msg.Append(Environment.NewLine);
				msg.Append(LocalizationManager.GetString("Project.UpdateFromBundle.Id", "ID"));
				msg.Append(Environment.NewLine);
				msg.Append("    ");
				msg.AppendFormat(oldValueFormat, m_model.PublicationId);
				msg.Append(Environment.NewLine);
				msg.Append("    ");
				msg.AppendFormat(newValueFormat, bundle.Id);
				msg.Append(Environment.NewLine);
			}

			if (msg.Length > 0)
			{
				msg.Insert(0, Environment.NewLine);
				msg.Insert(0, LocalizationManager.GetString("Project.UpdateFromBundle.UpdatedBundleMismatch",
					"The metadata of the selected text release bundle does not match the current project. If you continue with this update, the following metadata will be changed:"));

				// This isn't necessarily "required" or "expected", but it seems useful to report
				if (bundle.Name != m_model.PublicationName)
				{
					msg.Append(Environment.NewLine);
					msg.Append(LocalizationManager.GetString("Project.UpdateFromBundle.PublicationName", "Publication name"));
					msg.Append(Environment.NewLine);
					msg.Append("    ");
					msg.AppendFormat(oldValueFormat, m_model.PublicationName);
					msg.Append(Environment.NewLine);
					msg.Append("    ");
					msg.AppendFormat(newValueFormat, bundle.Name);
					msg.Append(Environment.NewLine);
				}
				msg.Append(Environment.NewLine);
				msg.Append(LocalizationManager.GetString("Project.UpdateFromBundle.AllowUpdate",
					"Do you want to continue with this update?"));
				return MessageBox.Show(this, msg.ToString(), LocalizationManager.GetString("Project.UpdateFromBundle.ConfirmUpdate", "Confirm Update"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
					DialogResult.Yes;
			}
			return true;
		}

		private void HandleChapterAnnouncementStyleChange(object sender, EventArgs e)
		{
			if (m_rdoBookNamePlusChapterNumber.Checked)
			{
				m_chkChapterOneAnnouncements.Checked = false;
				m_chkAnnounceChaptersForSingleChapterBooks.Checked = false;
			}
			HandleChapterAnnouncementChange(sender, e);
		}

		private void HandleChapterAnnouncementChange(object sender, EventArgs e)
		{
			m_model.ChapterAnnouncementStyle = ChapterAnnouncementStyle;
			UpdateAnnouncementsPageDisplay();
		}

		private void HandleAnnounceFirstChapterCheckedChanged(object sender, EventArgs e)
		{
			m_model.SkipChapterAnnouncementForFirstChapter = !m_chkChapterOneAnnouncements.Checked;
			m_chkAnnounceChaptersForSingleChapterBooks.Enabled = m_chkChapterOneAnnouncements.Checked;
			UpdateAnnouncementsPageDisplay();
		}

		private void HandleAnnounceSingleChapterCheckedChanged(object sender, EventArgs e)
		{
			m_model.SkipChapterAnnouncementForSingleChapterBooks = !m_chkAnnounceChaptersForSingleChapterBooks.Checked;
			UpdateAnnouncementsPageDisplay();
		}

		private void HandleSelectedTabPageChanged(object sender, EventArgs e)
		{
			if (m_tabControl.SelectedTab == m_tabPageTitleAndChapterAnnouncmentOptions)
			{
				m_lblExampleFirstChapterAnnouncement.Font =
				m_lblExampleSingleChapterAnnouncement.Font =
				m_lblExampleSubsequentChapterAnnouncement.Font =
				m_lblExampleTitleForMultipleChapterBook.Font =
				m_lblExampleTitleForSingleChapterBook.Font =
					new Font(m_wsViewModel.CurrentDefaultFontName,
						(float)Math.Min(m_lblBookTitleHeading.Font.SizeInPoints * 1.1, m_wsViewModel.CurrentDefaultFontSize));
			}
		}

		private void m_txtRecordingProjectName_TextChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = !IsNullOrWhiteSpace(m_txtRecordingProjectName.Text);
		}

		private void HandleSelectedReferenceTextChanged(object sender, EventArgs e)
		{
			m_linkRefTextAttribution.Text = Empty;
			m_linkRefTextAttribution.Links.Clear();
			if (m_ReferenceText.SelectedItem is KeyValuePair<string, ReferenceTextProxy> kvp)
			{
				var refTextProxy = kvp.Value;
				var metadata = refTextProxy.Metadata;
				SetReferenceTextCopyrightInfo(metadata);
				var referenceText = ReferenceText.GetReferenceText(refTextProxy);
				m_labelWarningReferenceTextDoesNotCoverAllBooks.Visible =
					!m_model.Project.AvailableBooks.Select(b => b.Code).All(bookId =>
						referenceText.HasContentForBook(bookId));

				m_labelOTVersion.Text = referenceText.EnglishOTVersion;
				m_labelNTVersion.Text = referenceText.EnglishNTVersion;
			}
		}

		private void SetReferenceTextCopyrightInfo(GlyssenDblTextMetadataBase metadata)
		{
			var copyrightInternalNodes = metadata?.Copyright?.Statement?.InternalNodes;
			if (copyrightInternalNodes == null)
				return;

			m_linkRefTextAttribution.Text = Join(Environment.NewLine, copyrightInternalNodes.Select(n => n.InnerText));
			const string kHttpPrefix = "http://";
			var linkStart = m_linkRefTextAttribution.Text.IndexOf(kHttpPrefix, StringComparison.Ordinal);
			if (linkStart >= 0)
			{
				var linkExtent = m_linkRefTextAttribution.Text.LastIndexOf("/", StringComparison.Ordinal) - linkStart;
				if (linkExtent > 0)
				{
					//m_linkRefTextAttribution.LinkArea = new LinkArea(linkStart, linkExtent);
					m_linkRefTextAttribution.Links.Add(linkStart, linkExtent,
						m_linkRefTextAttribution.Text.Substring(linkStart, linkExtent));
				}
			}
		}

		private void HandleWebSiteLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string tgt = e.Link.LinkData as string;

			if (!IsNullOrEmpty(tgt))
				System.Diagnostics.Process.Start(tgt);
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_tabControl.SelectedTab = m_tabPageScriptOptions;
			m_titleChapters.Focus();
		}

		private void m_titleChapters_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateAnnouncementsPageDisplay();
		}

		private void m_txtRecordingProjectName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var proposedName = RecordingProjectName;

			if (proposedName.Length == 0)
			{
				var captionFmt = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.RecordingProjectNameRequiredCaption",
					"{0} Required",
					"Parameter is the \"Recording Project Name\" label used in the Project Settings dialog box");

				var msgFmt = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.RecordingProjectNameRequired",
					"The {0} is required because it will be used as part of the file path to store the project.",
					"Parameter is the \"Recording Project Name\" label used in the Project Settings dialog box");

				DisplayRecordingProjectNameValidationError(msgFmt, captionFmt, e);
				e.Cancel = true;
				return;
			}

			var details = new StringBuilder();
			if (FileSystemUtils.StartsOrEndsWithDisallowedCharacters(proposedName))
			{
				details.Append(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.RemoveIllegalStartOrEndCharacters",
					"Do not start or end the name with a period.",
					"The \"name\" here refers to the Recording Project Name entered in the Project Settings dialog box."));
			}
			var illegalCharacters = FileSystemUtils.GetIllegalFilenameCharacters(proposedName);
			if (illegalCharacters.Any())
			{
				if (details.Length > 0)
					details.Insert(0, "\u2022 ").Append(Environment.NewLine).Append("\u2022 "); // Make it into a bullet list
				details.Append(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.RemoveIllegalCharacters",
					"Remove illegal characters from the name:",
					"The \"name\" here refers to the Recording Project Name entered in the Project Settings dialog box. " +
					"This will be followed by a list of one or more illegal characters that were encountered."));
				if (illegalCharacters.Count == 1)
					details.Append(" ").Append(illegalCharacters[0]);
				else
				{
					foreach (var illegalCharacter in illegalCharacters)
						details.Append(Environment.NewLine).Append("    ").Append(illegalCharacter);
				}
			}

			if (details.Length == 0 && FileSystemUtils.IsReservedFilename(proposedName))
			{
				details.Append(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.DoNotUseReservedFilename",
					"Use a different name. The name {0} is a reserved filename in the Windows operating system.",
					"The parameter is the Recording Project Name entered in the Project Settings dialog box."));
			}

			if (details.Length > 0)
			{
				details.Insert(0, Environment.NewLine);
				var captionFmt = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.InvalidRecordingProjectNameCaption",
					"Invalid {0}",
					"Parameter is the \"Recording Project Name\" label used in the Project Settings dialog box");

				var msgFmt = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.InvalidRecordingProjectName",
					"The {0} will be used as part of the file path to store the project. To make it a legal name, do the following:",
					"Parameter is the \"Recording Project Name\" label used in the Project Settings dialog box");

				DisplayRecordingProjectNameValidationError(msgFmt + details, captionFmt, e);
			}
		}

		private void DisplayRecordingProjectNameValidationError(string msgFmt, string captionFmt, System.ComponentModel.CancelEventArgs e)
		{
			var recordingProjectNameLabel = m_lblRecordingProjectName.Text;
			for (int i = recordingProjectNameLabel.Length - 1; i > 0; i--)
			{
				if (char.IsPunctuation(recordingProjectNameLabel[i]))
					recordingProjectNameLabel = recordingProjectNameLabel.Remove(i, 1);
				else
					break;
			}
			MessageBox.Show(this, Format(msgFmt, recordingProjectNameLabel),
				Format(captionFmt, recordingProjectNameLabel),
				MessageBoxButtons.OK, MessageBoxIcon.Error);

			e.Cancel = true;
		}

		private void m_txtAudioStockNumber_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// I'm using U+2024 (One Dot Leader) as my replacement character because, while it is a legal
			// filename character, in practice it results in misleading names and should never be used. It's
			// almost certainly never going to be typed in by a (non-malicious) user as an audio stock number.
			const char kReplacement = '\u2024';
			var sanitized = AudioStockNumber.SanitizeFilename(kReplacement);

			string errorMsg = null;
			if (sanitized.Length != AudioStockNumber.Length)
			{
				// Note: It's theoretically possible to get here because of other weird leading or trailing
				// control characters, but this is highly unlikely.
				// (See SIL.Extensions.StringExtensions.IsInvalidFilenameLeadingOrTrailingSpaceChar)
				m_txtAudioStockNumber.Select(0, AudioStockNumber.Length);
				errorMsg = LocalizationManager.GetString("Project.StockNumberIllegalTrailingCharacters",
					"The {0} must not end with a dot (.).",
					"Param is the (localized) \"Audio Stock Number\" label (with punctuation trimmed).");
			}
			else
			{
				var firstIllegalCharacter = sanitized.IndexOf(kReplacement);
				if (firstIllegalCharacter >= 0)
				{
					var lastIllegalCharacter = AudioStockNumber.LastIndexOf(kReplacement);
					m_txtAudioStockNumber.Select(firstIllegalCharacter, lastIllegalCharacter);
					errorMsg = LocalizationManager.GetString("Project.StockNumberIllegalCharacters",
						"The {0} must not contain illegal directory name characters.",
						"Param is the (localized) \"Audio Stock Number\" label (with punctuation trimmed).");
				}
			}

			if (errorMsg != null)
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				m_txtAudioStockNumber.Select(0, AudioStockNumber.Length);
				errorProvider1.SetError(m_txtAudioStockNumber, Format(errorMsg,
					Regex.Replace(m_lblAudioStockNumber.Text, @"[^\w\s]", "")));
			}
		}

		private void m_txtAudioStockNumber_Validated(object sender, EventArgs e)
		{
			// If all conditions have been met, clear the ErrorProvider of errors.
			errorProvider1.SetError(m_txtAudioStockNumber, "");
		}
	}
}
