using System;
using Glyssen.Dialogs;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	public class SelectBundleDialogTests
	{
		[Test, Ignore("By hand only")]
		[STAThread]
		public void ShowDialog()
		{
			using (var dlg = new SelectProjectDialog())
			{
				dlg.ShowDialog();
			}
		}
	}
}
