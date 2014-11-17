using System;
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
		public SandboxForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (var dlg = new SelectBundleDialog())
			{
				dlg.ShowDialog();
				Settings.Default.CurrentProject = dlg.FileName;
				LoadBundle(dlg.FileName);
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox("");
		}

		private void SandboxForm_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Settings.Default.CurrentProject))
				using (var dlg = new WelcomeDialog())
					dlg.ShowDialog();
			LoadBundle(Settings.Default.CurrentProject);
		}

		private void LoadBundle(string bundlePath)
		{
			label1.Text = bundlePath;
			var bundle = new Bundle.Bundle(bundlePath);
			m_bundleId.Text = bundle.Id;
			Canon canon;
			UsxDocument book;
			if (bundle.TryGetCanon(1, out canon))
				if (canon.TryGetBook("MRK", out book))
				{
					MessageBox.Show(book.GetBook().OuterXml);
					var sb = new StringBuilder();
					foreach (var block in new UsxParaParser(book.GetParas()).Parse())
						sb.Append(block.GetAsXml(false));
					Console.WriteLine(sb.ToString());
				}
		}
	}
}
