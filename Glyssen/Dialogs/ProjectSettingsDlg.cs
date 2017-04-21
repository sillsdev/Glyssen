using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Shared.Bundle;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using SIL.IO;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class ProjectSettingsDlg : FormWithPersistedSettings
	{
		private ProjectSettingsViewModel m_model;

		private class ChapterAnnouncementItem
		{
			public string UiString { get; private set; }
			public ChapterAnnouncement ChapterAnnouncement { get; private set; }

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

		public ProjectSettingsDlg(ProjectSettingsViewModel model)
		{
			InitializeComponent();

			for (int i = 0; i < m_cboBookMarker.Items.Count; i++)
			{
				var chapterAnnouncement = (ChapterAnnouncement)i;
				m_cboBookMarker.Items[i] = new ChapterAnnouncementItem(LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId,
					"DialogBoxes.ProjectSettingsDlg.ChapterAnnouncementTab.BookMarkerComboBox.Items." + chapterAnnouncement,
					m_cboBookMarker.Items[i].ToString()), chapterAnnouncement);
			}
			if (model.Project.IncludedBooks.All(book => string.IsNullOrEmpty(book.PageHeader)))
				RemoveItemFromBookMarkerCombo(ChapterAnnouncement.PageHeader);
			if (model.Project.IncludedBooks.All(book => string.IsNullOrEmpty(book.MainTitle)))
				RemoveItemFromBookMarkerCombo(ChapterAnnouncement.MainTitle1);

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
			HandleStringsLocalized();
			LoadProjectDramatizationOptions();
			ProjectSettingsViewModel = model;
			UpdateQuotePageDisplay();
		}

		private void HandleStringsLocalized()
		{
			LoadReferenceTextOptions();
			var fmt = m_linkLblChangeOmittedChapterAnnouncements.Text;
			var linkStartPos = fmt.IndexOf("{0}");
			m_linkLblChangeOmittedChapterAnnouncements.Text = String.Format(fmt, m_tabPageScriptOptions.Text);
			if (linkStartPos >= 0 && m_tabPageScriptOptions.Text.Length > 0)
				m_linkLblChangeOmittedChapterAnnouncements.LinkArea = new LinkArea(linkStartPos, m_tabPageScriptOptions.Text.Length);
			else
				m_linkLblChangeOmittedChapterAnnouncements.LinkArea = default(LinkArea);
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
			var dataSource = new Dictionary<string, ReferenceTextIdentifier>();
			foreach (var refTextId in ReferenceTextIdentifier.AllAvailable)
			{
				string key;
				if (refTextId.Type == ReferenceTextType.Custom)
				{
					var fmt = (refTextId.Missing) ?
						LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.MissingReferenceText", "Missing: {0}") :
						LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.CustomReferenceText", "Custom: {0}");
					key = String.Format(fmt, refTextId.CustomIdentifier);
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

		public string ReferenceTextTabPageName { get { return m_tabPageReferenceTexts.Text; } }

		private ProjectSettingsViewModel ProjectSettingsViewModel
		{
			set
			{
				m_model = value;
                RecordingProjectName = value.RecordingProjectName;
                AudioStockNumber = value.AudioStockNumber;
				m_txtOriginalBundlePath.Text = value.BundlePath;
				LanguageName = value.LanguageName;
				IsoCode = value.IsoCode;
				PublicationName = value.PublicationName;
				PublicationId = value.PublicationId;
				m_txtVersification.Text = value.Versification.Name;
				m_lblQuoteMarkSummary.Text = value.Project.QuoteSystem.ShortSummary;

				m_wsFontControl.BindToModel(m_model.WsModel);

				if (!string.IsNullOrWhiteSpace(m_model.SampleText))
					m_wsFontControl.TestAreaText = m_model.SampleText;

				// PG-433, 07 JAN 2016, PH: Disable some UI if project file is not writable
                var enableControls = m_model.Project.ProjectFileIsWritable;
                m_btnUpdateFromBundle.Enabled =
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

				foreach (KeyValuePair<string, ReferenceTextIdentifier> kvp in m_ReferenceText.Items)
				{
					if (kvp.Value == m_model.Project.ReferenceTextIdentifier)
					{
						m_ReferenceText.SelectedItem = kvp;
						break;
					}
				}

				m_bookIntro.SelectedValue = m_model.Project.DramatizationPreferences.BookIntroductionsDramatization;
				m_sectionHeadings.SelectedValue = m_model.Project.DramatizationPreferences.SectionHeadDramatization;
				m_titleChapters.SelectedValue = m_model.Project.DramatizationPreferences.BookTitleAndChapterDramatization;
			}
		}

        private string RecordingProjectName
        {
            get { return m_txtRecordingProjectName.Text.Trim(); }
            set { m_txtRecordingProjectName.Text = value.Trim(); }
        }

        private string AudioStockNumber
        {
            get { return m_txtAudioStockNumber.Text; }
            set { m_txtAudioStockNumber.Text = value; }
        }

		private string LanguageName
		{
			set { m_txtLanguageName.Text = value ?? string.Empty; }
		}

		private string IsoCode
		{
			get { return (string.IsNullOrWhiteSpace(m_txtIso639_2_Code.Text)) ? "zzz" : m_txtIso639_2_Code.Text; }
			set { m_txtIso639_2_Code.Text = value; }
		}

		private string PublicationName
		{
			set { m_txtPublicationName.Text = value; }
		}

		private string PublicationId
		{
			get { return m_txtPublicationId.Text; }
			set { m_txtPublicationId.Text = value; }
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
					quoteMarkReviewText = string.Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.QuoteMarksReviewed", "Quote mark settings were reviewed on {0}.", "{0} is a date"), m_model.Project.QuoteSystemDate.ToString("yyyy-MM-dd"));
					break;
				case QuoteSystemStatus.UserSet:
					quoteMarkReviewText = string.Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.QuoteMarksUpdated", "Quote mark settings were updated on {0}.", "{0} is a date"), m_model.Project.QuoteSystemDate.ToString("yyyy-MM-dd"));
					break;
			}
			m_lblQuoteMarkReview.Text = quoteMarkReviewText;
		}

		private void UpdateAnnouncementsPageDisplay()
		{
			var showChangeOmittedChapterAnnouncementsLabel = m_titleChapters.SelectedValue is ExtraBiblicalMaterialSpeakerOption &&
				(ExtraBiblicalMaterialSpeakerOption) m_titleChapters.SelectedValue == ExtraBiblicalMaterialSpeakerOption.Omitted;

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
			if (m_model.RecordingProjectName != RecordingProjectName && File.Exists(Project.GetProjectFilePath(IsoCode, PublicationId, RecordingProjectName)))
			{
				var msg =string.Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OverwriteProjectPrompt",
					"A {0} project with an ID of {1} and a Recording Project Name of {2} already exists for this language. Do you want to overwrite it?"),
					ProductName, m_txtPublicationId.Text, RecordingProjectName);
				if (MessageBox.Show(this, msg, ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
				{
					DialogResult = DialogResult.None;
					return;
				}
				DirectoryUtilities.DeleteDirectoryRobust(Project.GetProjectFolderPath(IsoCode, PublicationId, RecordingProjectName));
			}

            m_model.RecordingProjectName = RecordingProjectName;
            m_model.AudioStockNumber = AudioStockNumber;
			m_model.ChapterAnnouncementStyle = ChapterAnnouncementStyle;
			m_model.SkipChapterAnnouncementForFirstChapter = !m_chkChapterOneAnnouncements.Checked;
			m_model.Project.ReferenceTextIdentifier = ((KeyValuePair<string, ReferenceTextIdentifier>)m_ReferenceText.SelectedItem).Value;

			m_model.Project.DramatizationPreferences.BookIntroductionsDramatization = (ExtraBiblicalMaterialSpeakerOption)m_bookIntro.SelectedValue;
			m_model.Project.DramatizationPreferences.SectionHeadDramatization = (ExtraBiblicalMaterialSpeakerOption)m_sectionHeadings.SelectedValue;
			m_model.Project.DramatizationPreferences.BookTitleAndChapterDramatization = (ExtraBiblicalMaterialSpeakerOption)m_titleChapters.SelectedValue;

			m_model.Project.ProjectSettingsStatus = ProjectSettingsStatus.Reviewed;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void HandleCancelButtonClick(object sender, EventArgs e)
		{
			// This ensures that any temporary override to this (which ultimately sets a static
			// member on Block) is reset if the user presses Cancel.
			m_model.RevertChapterAnnouncementStyle();
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private ChapterAnnouncement ChapterAnnouncementStyle
		{
			get
			{
				return m_rdoChapterLabel.Checked ? ChapterAnnouncement.ChapterLabel :
					((ChapterAnnouncementItem)m_cboBookMarker.SelectedItem).ChapterAnnouncement;
			}
		}

		private void m_btnQuoteMarkSettings_Click(object sender, EventArgs e)
		{
			bool reparseOkay = false;
			if (m_model.Project.IsSampleProject)
			{
				string msg = LocalizationManager.GetString("Project.CannotChangeSampleMsg", "The Quote Mark Settings cannot be modified for the Sample project.");
				string title = LocalizationManager.GetString("Project.CannotChangeSample", "Cannot Change Sample Project");
				MessageBox.Show(msg, title);
			}
			else
			{
				if (!m_model.Project.IsReparseOkay())
				{
					string msg = string.Format(LocalizationManager.GetString("Project.UnableToLocateTextBundleMsg",
						"The original text bundle for the project is no longer in its original location ({0}). " +
						"The Quote Mark Settings cannot be modified without access to the original text bundle."), m_model.Project.OriginalBundlePath) +
						Environment.NewLine + Environment.NewLine +
						LocalizationManager.GetString("Project.LocateBundleYourself", "Would you like to locate the text bundle yourself?");
					string title = LocalizationManager.GetString("Project.UnableToLocateTextBundle", "Unable to Locate Text Bundle");
					if (DialogResult.Yes == MessageBox.Show(msg, title, MessageBoxButtons.YesNo))
						reparseOkay = SelectProjectDlg.GiveUserChanceToFindOriginalBundle(m_model.Project);
				}
				else
					reparseOkay = true;
			}

			using (var viewModel = new BlockNavigatorViewModel(m_model.Project, BlocksToDisplay.AllExpectedQuotes, m_model))
				using (var dlg = new QuotationMarksDlg(m_model.Project, viewModel, !reparseOkay))
				{
					MainForm.LogDialogDisplay(dlg);
					if (dlg.ShowDialog(this) == DialogResult.OK)
						UpdateQuotePageDisplay();
				}
		}

		private void m_btnUpdateFromBundle_Click(object sender, EventArgs e)
		{
			using (var dlg = new SelectProjectDlg(false, m_model.BundlePath))
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					var selectedBundlePath = dlg.FileName;
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
				var oldLanguage = string.IsNullOrEmpty(m_model.LanguageName) ? m_model.IsoCode : string.Format("{0} ({1})", m_model.LanguageName, m_model.IsoCode);
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
					new Font(m_model.WsModel.CurrentDefaultFontName,
						(float)Math.Min(m_lblBookTitleHeading.Font.SizeInPoints * 1.1, m_model.WsModel.CurrentDefaultFontSize));
			}
		}

		private void m_txtRecordingProjectName_TextChanged(object sender, EventArgs e)
		{
			m_btnOk.Enabled = !String.IsNullOrWhiteSpace(m_txtRecordingProjectName.Text);
		}

		private void HandleSelectedReferenceTextChanged(object sender, EventArgs e)
		{
			m_linkRefTextAttribution.Text = String.Empty;
			m_linkRefTextAttribution.Links.Clear();
			if (m_ReferenceText.SelectedItem is KeyValuePair<string, ReferenceTextIdentifier>)
			{
				var metadata = ((KeyValuePair<string, ReferenceTextIdentifier>) m_ReferenceText.SelectedItem).Value.Metadata;
				if (metadata == null)
					return;
				var copyright = metadata.Copyright;
				if (copyright == null || copyright.Statement == null)
					return;

				var copyrightInternalNodes = copyright.Statement.InternalNodes;
				if (copyrightInternalNodes != null)
				{
					m_linkRefTextAttribution.Text = string.Join(Environment.NewLine, copyrightInternalNodes.Select(n => n.InnerText));
				}
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
		}

		private void HandleWebSiteLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string tgt = e.Link.LinkData as string;

			if (!string.IsNullOrEmpty(tgt))
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
	}
}
