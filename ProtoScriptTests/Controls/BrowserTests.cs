using System;
using System.Windows.Forms;
using NUnit.Framework;
using ProtoScript.Controls;

namespace ProtoScriptTests.Controls
{
	class BrowserTests
	{
		[Test, Ignore("By hand only")]
		[STAThread]
		public void Navigate()
		{
			using (var form = new Form())
			{
				using (var browser = new Browser())
				{
					form.Controls.Add(browser);
					browser.Navigate("www.google.com");
					form.ShowDialog();
				}
			}
		}
	}
}
