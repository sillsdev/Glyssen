using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Glyssen.Bundle;
using L10NSharp;
using SIL.IO;

namespace Glyssen.Dialogs
{
	public partial class ProjectSettingsDlg : Form
	{
		private ProjectMetadataViewModel m_model;
		private readonly bool m_initialSave;

		public ProjectSettingsDlg(ProjectMetadataViewModel model, bool initialSave = false)
		{
			InitializeComponent();
			SetReadOnly();
			ProjectMetadataViewModel = model;
			m_initialSave = initialSave;
			if (!m_initialSave)
			{
				m_chkOverride.Visible = false;
				m_txtIso639_2_Code.Enabled = false;
			}
			UpdateProjectId(null, null);
			UpdateDisplay();

			//bool vrsSelected = false;
			//if (!string.IsNullOrEmpty(model.VersificationFilePath) && File.Exists(model.VersificationFilePath))
			//{
			//	m_cboVersification.Items.Add(
			//		Versification.Table.VersificationTables().Single(v => v.PathName == model.VersificationFilePath));
			//	m_cboVersification.SelectedIndex = 0;
			//	vrsSelected = true;
			//}

			//foreach (var vrs in Paratext.Versification.Table.VersificationTables())
			//{
			//	if (!vrs.PathName.StartsWith(Project.ProjectsBaseFolder))
			//	{
			//		var i = m_cboVersification.Items.Add(vrs.Name);
			//		if (!vrsSelected && vrs.Name == model.VersificationName)
			//		{
			//			m_cboVersification.SelectedIndex = i;
			//			vrsSelected = true;
			//		}
			//	}
			//}
			//if (!readOnly)
			//	m_cboVersification.Items.Add(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OtherVersification",
			//		"Other..."));

			//LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		//private void HandleStringsLocalized()
		//{
		//	if (m_cboVersification.Items[m_cboVersification.Items.Count - 1] is string)
		//	{
		//		m_cboVersification.Items[m_cboVersification.Items.Count - 1] =
		//			m_cboVersification.Items.Add(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OtherVersification",
		//				"Other..."));
		//	}
		//}

		private ProjectMetadataViewModel ProjectMetadataViewModel
		{
			set
			{
				m_model = value;
				RecordingProjectName = value.RecordingProjectName;
				LanguageName = value.LanguageName;
				IsoCode = value.IsoCode;
				PublicationName = value.PublicationName;
				PublicationId = value.PublicationId;
				m_txtVersification.Text = value.Versification.Name;
				m_lblQuoteMarkSummary.Text = value.Project.QuoteSystem.ToString();

				m_wsFontControl.BindToModel(m_model.WsModel);

				if (!string.IsNullOrWhiteSpace(m_model.SampleText))
					m_wsFontControl.TestAreaText = m_model.SampleText;
			}
		}

		private string RecordingProjectName
		{
			get { return m_txtRecordingProjectName.Text; }
			set { m_txtRecordingProjectName.Text = value; }
		}

		private string LanguageName
		{
			get { return m_txtLanguageName.Text; }
			set { m_txtLanguageName.Text = value ?? string.Empty; }
		}

		private string IsoCode
		{
			get { return (string.IsNullOrWhiteSpace(m_txtIso639_2_Code.Text)) ? "zzz" : m_txtIso639_2_Code.Text; }
			set { m_txtIso639_2_Code.Text = value; }
		}

		private string PublicationName
		{
			get { return m_txtPublicationName.Text; }
			set { m_txtPublicationName.Text = value; }
		}

		private string PublicationId
		{
			get { return m_txtPublicationId.Text; }
			set { m_txtPublicationId.Text = value; }
		}

		private void SetReadOnly()
		{
			m_txtLanguageName.Enabled = false;
			m_txtIso639_2_Code.Enabled = false;
			m_txtPublicationName.Enabled = false;
			m_chkOverride.Visible = false;
//			m_wsFontControl.ReadOnly = true;
//			m_btnOk.Enabled = false;
			m_txtVersification.Enabled = false;
		}

		private void UpdateProjectId(object sender, EventArgs e)
		{
			if (!m_initialSave)
				return;
			if (!m_txtPublicationId.Enabled)
			{
				StringBuilder bldr = new StringBuilder("sfm");
				
				if (!string.IsNullOrWhiteSpace(m_txtIso639_2_Code.Text))
				{
					bldr.Append("_");
					bldr.Append(m_txtIso639_2_Code.Text);
				}
				if (!string.IsNullOrWhiteSpace(m_txtPublicationName.Text))
				{
					bldr.Append("_");
					bldr.Append(m_txtPublicationName.Text);
				}
				var publicationId = bldr.ToString();
				while (publicationId == "sfm" || File.Exists(Project.GetProjectFilePath(IsoCode, publicationId, RecordingProjectName)))
				{
					// If the user didn't supply anything helpful to distinguish this project from any other SFM project
					// or we already have a project with this same language and project name, tack on something to make it
					// unique.
					publicationId = bldr.ToString() + DateTime.Now.Ticks;
				}
				m_txtPublicationId.Text = publicationId;
			}
		}

		private void UpdateDisplay()
		{
			m_lblQuoteMarkSummary.Text = m_model.Project.QuoteSystem.ToString();

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

		private void m_chkOverride_CheckedChanged(object sender, EventArgs e)
		{
			m_txtPublicationId.Enabled = m_chkOverride.Checked;
			UpdateProjectId(sender, e);
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
			//m_model.LanguageName = LanguageName;
			//m_model.IsoCode = IsoCode;
			//m_model.PublicationName = PublicationName;
			//m_model.PublicationId = PublicationId;
			
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
						"The Quote Mark Settings cannot be modified without access to the original text bundle."), m_model.Project.OriginalPathOfDblFile) +
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

		//private void m_cboVersification_SelectedIndexChanged(object sender, EventArgs e)
		//{
			//var lastItemIndex = m_cboVersification.Items.Count - 1;
			//if (m_cboVersification.Items[lastItemIndex] is string && m_cboVersification.SelectedIndex == lastItemIndex)
			//{
			//	using (var dlg = new OpenFileDialog())
			//	{
			//		var defaultDir = OpenProjectDlg.ParatextProjectsFolder;
			//		if (defaultDir != null)
			//			dlg.InitialDirectory = defaultDir;

			//		dlg.Title = LocalizationManager.GetString("SelectVersificationDialog.Title", "Open Project"),
			//	InitialDirectory = defaultDir,
			//	Filter = string.Format("{0} ({1})|{1}|{2} ({3})|{3}|{4} ({5})|{5}",
			//		LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ResourceBundleFileTypeLabel", "Text Resource Bundle files"),
			//		"*" + kResourceBundleExtension,
			//		LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ProjectFilesLabel", "Glyssen Project Files"),
			//		"*" + Project.kProjectFileExtension,
			//		LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
			//		"*.*"),
			//	DefaultExt = kResourceBundleExtension

			//		if (dlg.ShowDialog() == DialogResult.OK)
			//		{
			//			SelectedProject = dlg.FileName;
			//			var folder = Path.GetDirectoryName(SelectedProject);
			//			if (folder != null)
			//			{
			//				folder = Path.GetDirectoryName(folder);
			//				if (folder != null)
			//					Settings.Default.DefaultSfmDirectory = folder;
			//			}
			//			Type = ProjectType.StandardFormatBook;
			//			DialogResult = DialogResult.OK;
			//			Close();
			//		}
			//	}
			//}
		//}
	}
}
