using System;
using NUnit.Framework;
using ProtoScript.Dialogs;

namespace ProtoScriptTests.Dialogs
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
