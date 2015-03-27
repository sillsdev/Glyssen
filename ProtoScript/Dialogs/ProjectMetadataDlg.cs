using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;

namespace ProtoScript.Dialogs
{
	public partial class ProjectMetadataDlg : Form
	{
		private ProjectMetadataViewModel m_model;
		private readonly bool m_initialSave;
		public ProjectMetadataDlg(ProjectMetadataViewModel model, bool initialSave = false, bool readOnly = false)
		{
			InitializeComponent();
			if (readOnly)
				SetReadOnly();
			ProjectMetadataViewModel = model;
			m_initialSave = initialSave;
			if (!m_initialSave)
			{
				m_chkOverride.Visible = false;
				m_txtIso639_2_Code.Enabled = false;
			}
			UpdateProjectId(null, null);

			bool vrsSelected = false;
			if (!string.IsNullOrEmpty(model.VersificationFilePath) && File.Exists(model.VersificationFilePath))
			{
				m_cboVersification.Items.Add(
					Paratext.Versification.Table.VersificationTables().Single(v => v.PathName == model.VersificationFilePath));
				m_cboVersification.SelectedIndex = 0;
				vrsSelected = true;
			}

			foreach (var vrs in Paratext.Versification.Table.VersificationTables())
			{
				if (!vrs.PathName.StartsWith(Project.ProjectsBaseFolder))
				{
					var i = m_cboVersification.Items.Add(vrs.Name);
					if (!vrsSelected && vrs.Name == model.VersificationName)
					{
						m_cboVersification.SelectedIndex = i;
						vrsSelected = true;
					}
				}
			}

			if (!readOnly)
				m_cboVersification.Items.Add(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OtherVersification", "Other..."));
		}

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
			m_btnOk.Enabled = false;
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

		private void m_chkOverride_CheckedChanged(object sender, EventArgs e)
		{
			m_txtPublicationId.Enabled = m_chkOverride.Checked;
			UpdateProjectId(sender, e);
		}

		private void HandleOkButtonClick(object sender, EventArgs e)
		{
			if (m_initialSave && File.Exists(Project.GetProjectFilePath(IsoCode, PublicationId, RecordingProjectName)))
			{
				var msg =string.Format(LocalizationManager.GetString("DialogBoxes.ProjectSettingsDlg.OverwriteProjectPrompt",
					"A {0} project with an ID of {1} already exists for this language. Do you want to overwrite it?"),
					ProductName, m_txtPublicationId.Text);
				if (MessageBox.Show(this, msg, ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) !=
					DialogResult.Yes)
				{
					return;
				}
			}

			m_model.RecordingProjectName = RecordingProjectName;
			m_model.LanguageName = LanguageName;
			m_model.IsoCode = IsoCode;
			m_model.PublicationName = PublicationName;
			m_model.PublicationId = PublicationId;
			
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
