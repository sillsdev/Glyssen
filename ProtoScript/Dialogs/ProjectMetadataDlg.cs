using System;
using System.IO;
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
			m_wsFontControl.BindToModel(model.WsModel);
			m_initialSave = initialSave;
			if (!m_initialSave)
			{
				m_chkOverride.Visible = false;
				m_txtIso639_2_Code.Enabled = false;
			}
			UpdateProjectId(null, null);
		}

		public ProjectMetadataViewModel ProjectMetadataViewModel {
			get
			{
				m_model.LanguageName = LanguageName;
				m_model.IsoCode = IsoCode;
				m_model.ProjectName = ProjectName;
				m_model.ProjectId = ProjectId;
				return m_model;
			}
			private set
			{
				m_model = value;
				LanguageName = value.LanguageName;
				IsoCode = value.IsoCode;
				ProjectName = value.ProjectName;
				ProjectId = value.ProjectId;
			}
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

		private string ProjectName
		{
			get { return m_txtProjectName.Text; }
			set { m_txtProjectName.Text = value; }
		}

		private string ProjectId
		{
			get { return m_txtProjectId.Text; }
			set { m_txtProjectId.Text = value; }
		}

		private void SetReadOnly()
		{
			m_txtLanguageName.Enabled = false;
			m_txtIso639_2_Code.Enabled = false;
			m_txtProjectName.Enabled = false;
			m_chkOverride.Visible = false;
			m_wsFontControl.ReadOnly = true;
			m_btnOk.Enabled = false;
		}

		private void UpdateProjectId(object sender, EventArgs e)
		{
			if (!m_initialSave)
				return;
			if (!m_txtProjectId.Enabled)
			{
				StringBuilder bldr = new StringBuilder("sfm");
				
				if (!string.IsNullOrWhiteSpace(m_txtIso639_2_Code.Text))
				{
					bldr.Append("_");
					bldr.Append(m_txtIso639_2_Code.Text);
				}
				if (!string.IsNullOrWhiteSpace(m_txtProjectName.Text))
				{
					bldr.Append("_");
					bldr.Append(m_txtProjectName.Text);
				}
				if (bldr.ToString() == "sfm" || File.Exists(Project.GetProjectFilePath(IsoCode, bldr.ToString())))
				{
					// If the user didn't supply anything helpful to distinguish this project from any other SFM project
					// or we already have a project with this same language and project name, tack on something to make it
					// unique.
					bldr.Append(DateTime.Now.Ticks);
				}
				m_txtProjectId.Text = bldr.ToString();
			}
		}

		private void m_chkOverride_CheckedChanged(object sender, EventArgs e)
		{
			m_txtProjectId.Enabled = m_chkOverride.Checked;
			UpdateProjectId(sender, e);
		}

		private void HandleOkButtonClick(object sender, EventArgs e)
		{
			if (m_initialSave && File.Exists(Project.GetProjectFilePath(IsoCode, m_txtProjectId.Text)))
			{
				var msg =string.Format(LocalizationManager.GetString("DialogBoxes.OpenProjectDlg.OverwriteProjectPrompt",
					"A {0} project with an ID of {1} already exists for this language. Do you want to overwrite it?"),
					ProductName, m_txtProjectId.Text);
				if (MessageBox.Show(this, msg, ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) ==
					DialogResult.Yes)
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}
	}
}
