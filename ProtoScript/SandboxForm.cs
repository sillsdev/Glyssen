using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;
using Palaso.Xml;
using ProtoScript.Bundle;
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
				var bundle = new Bundle.Bundle(dlg.FileName);
				m_bundleId.Text = bundle.Id;
				Canon canon;
				UsxDocument book;
				if (bundle.Canons.TryGetValue(1, out canon))
					if (canon.Books.TryGetValue("MRK", out book))
					{
						MessageBox.Show(book.GetBook().OuterXml);
						var sb = new StringBuilder();
						foreach (var block in new UsxParaParser(book.GetParas()).Parse())
							sb.Append(block.GetAsXml(false));
						Console.WriteLine(sb.ToString());
					}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			LocalizationManager.ShowLocalizationDialogBox("");
		}
	}
}
