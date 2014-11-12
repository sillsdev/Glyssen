using System;
using System.Windows.Forms;
using ProtoScript.Dialogs;

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
				label1.Text = dlg.FileName;
			}
		}
	}
}
