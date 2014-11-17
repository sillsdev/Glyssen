using System;
using System.Windows.Forms;
using ProtoScript.Properties;

namespace ProtoScript.Dialogs
{
	public partial class WelcomeDialog : Form
	{
		public WelcomeDialog()
		{
			InitializeComponent();
		}

		private void m_selectBundleBtn_Click(object sender, EventArgs e)
		{
			using (var dlg = new SelectBundleDialog())
			{
				DialogResult = dlg.ShowDialog();
				if (dlg.FileName != null)
				{
					Settings.Default.CurrentProject = dlg.FileName;
					Close();
				}
			}
		}
	}
}
