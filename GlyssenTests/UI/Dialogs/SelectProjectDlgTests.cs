using System;
using Glyssen.UI.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.UI.Dialogs
{
	[TestFixture]
	public class SelectProjectDlgTests
	{
		[Test, Ignore("By hand only")]
		[STAThread]
		public void ShowDialog()
		{
			using (var dlg = new SelectProjectDlg())
			{
				dlg.ShowDialog();
			}
		}
	}
}
