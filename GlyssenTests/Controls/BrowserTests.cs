using System.Threading;
using System.Windows.Forms;
using Glyssen.Controls;
using NUnit.Framework;

namespace GlyssenTests.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	class BrowserTests
	{
		[Test]
		[Explicit] // By hand
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

		[Test]
		[Explicit] // By hand
		public void DisplayHtml()
		{
			using (var form = new Form())
			{
				using (var browser = new Browser())
				{
					form.Controls.Add(browser);
					browser.DisplayHtml("normal <em>italics</em> <strong>bold</strong>");
					form.ShowDialog();
				}
			}
		}
	}
}
