using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Bundle;
using ProtoScript.Dialogs;
using ProtoScript.Properties;

namespace ProtoScript
{
	public partial class SandboxForm : Form
	{
		private Project m_project;

		public SandboxForm()
		{
			InitializeComponent();
		}

		private void HandleSelectBundle_Click(object sender, EventArgs e)
		{
			using (var dlg = new SelectBundleDialog())
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					LoadBundle(dlg.FileName);
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox("");
		}

		private void SandboxForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject))
			{
				using (var dlg = new WelcomeDialog())
				{
					if (dlg.ShowDialog() == DialogResult.OK)
						LoadBundle(Settings.Default.CurrentProject);
				}
			}
			else
			{
				MessageBox.Show("TODO: write code to reload project from XML");
			}
		}

		private void LoadBundle(string bundlePath)
		{
			var bundle = new Bundle.Bundle(bundlePath);
			m_lblFile.Text = string.Format(m_lblFile.Text, bundlePath);
			m_lblBundleId.Text = string.Format(m_lblBundleId.Text, bundle.Id);
			m_lblLanguage.Text = string.Format(m_lblLanguage.Text, bundle.Language);

			m_project = new Project(bundle.Metadata);
			Canon canon;
			UsxDocument book;
			if (bundle.TryGetCanon(1, out canon))
			{
				if (canon.TryGetBook("MRK", out book))
				{
					m_project.AddBook("MRK", new UsxParser(book.GetParas()).Parse());
				}
			}
		}

		private void HandleSave_Click(object sender, EventArgs e)
		{
			m_project.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				Program.kCompany, Program.kProduct));
		}

		private void SandboxForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.Default.Save();
		}
	}
}
