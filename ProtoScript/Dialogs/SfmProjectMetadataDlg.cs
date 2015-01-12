using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using L10NSharp;
using Palaso.UI.WindowsForms.WritingSystems;

namespace ProtoScript.Dialogs
{
	public partial class SfmProjectMetadataDlg : Form
	{
		private readonly WritingSystemSetupModel m_model;

		public SfmProjectMetadataDlg(WritingSystemSetupModel model)
		{
			m_model = model;
			InitializeComponent();
			m_wsFontControl.BindToModel(model);
			UpdateProjectId(null, null);
		}

		public string LanguageName
		{
			get { return m_txtLanguageName.Text; }
			set { m_txtLanguageName.Text = value ?? string.Empty; }
		}

		public string IsoCode
		{
			get { return (string.IsNullOrWhiteSpace(m_txtIso639_2_Code.Text)) ? "zzz" : m_txtIso639_2_Code.Text; }
		}

		public string ProjectName
		{
			get { return m_txtProjectName.Text; }
		}

		public string ProjectId
		{
			get { return m_txtProjectId.Text; }
		}

		private void UpdateProjectId(object sender, EventArgs e)
		{
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
			if (File.Exists(Project.GetProjectFilePath(IsoCode, m_txtProjectId.Text)))
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
