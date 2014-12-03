using System.Diagnostics;
using System.Windows.Forms;
using Gecko;
using Palaso.UI.WindowsForms.HtmlBrowser;

namespace ProtoScript.Controls
{
	public partial class Browser : UserControl
	{
		private XWebBrowser m_browser;

		public Browser()
		{
			InitializeComponent();

			m_browser = new XWebBrowser(XWebBrowser.BrowserType.GeckoFx);
			if (!(m_browser.NativeBrowser is GeckoWebBrowser))
				Debug.Fail("Failed to use GeckoWebBrowser");
			m_browser.Parent = this;
			m_browser.Dock = DockStyle.Fill;
			m_browser.AllowWebBrowserDrop = false;
			m_browser.IsWebBrowserContextMenuEnabled = false;
			m_browser.WebBrowserShortcutsEnabled = false;
			Controls.Add(m_browser);
		}

		public void Navigate(string url)
		{
			m_browser.Navigate(url);
		}
	}
}
