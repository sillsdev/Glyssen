using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Bundle;
using L10NSharp;
using SIL.IO;

namespace Glyssen.Dialogs
{
	public partial class ProjectSettingsDlg : Form
	{
		private ProjectSettingsViewModel m_model;

		public ProjectSettingsDlg(ProjectSettingsViewModel model)
		{
			InitializeComponent();
			ProjectSettingsViewModel = model;
			UpdateDisplay();
		}

		public GlyssenBundle UpdatedBundle { get; private set; }

		private ProjectSettingsViewModel ProjectSettingsViewModel
		{
			set
			{
				m_model = value;
				RecordingProjectName = value.RecordingProjectName;
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
				var enableControls = (m_model.Project == null) || m_model.Project.ProjectFileIsWritable;
				m_btnUpdateFromBundle.Enabled = enableControls;
				m_btnQuoteMarkSettings.Enabled = enableControls;
				m_btnOk.Enabled = enableControls;
				m_wsFontControl.Enabled = enableControls;
				m_txtRecordingProjectName.Enabled = enableControls;
			}
		}

		private string RecordingProjectName
		{
			get { return m_txtRecordingProjectName.Text; }
			set { m_txtRecordingProjectName.Text = value; }
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

		private void UpdateDisplay()
		{
			m_lblQuoteMarkSummary.Text = m_model.Project.QuoteSystem.ShortSummary;

			m_lblQuoteMarkReview.ForeColor = Color.White;

			string quoteMarkReviewText = "";
			switch (m_model.Project.QuoteSystemStatus)
			{
				case QuoteSystemStatus.Obtained:
					quoteMarkReviewText = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.ReviewQuoteMarks", "You may review the quote mark settings.");
					break;
				case QuoteSystemStatus.Guessed:
					quoteMarkReviewText = LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.CarefullyReviewQuoteMarks", "Carefully review the quote mark settings.");
					m_lblQuoteMarkReview.ForeColor = Color.Yellow;
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

			m_model.Project.ProjectSettingsStatus = ProjectSettingsStatus.Reviewed;
			DialogResult = DialogResult.OK;
			Close();
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
					if (dlg.ShowDialog(this) == DialogResult.OK)
						UpdateDisplay();
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
						m_model.BundlePath = dlg.FileName;
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

	}
}
