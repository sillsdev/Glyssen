using System;
using GlyssenApp.UI.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
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
