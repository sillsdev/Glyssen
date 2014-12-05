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

			m_browser = new XWebBrowser(XWebBrowser.BrowserType.GeckoFx)
			{
				Parent = this, 
				Dock = DockStyle.Fill, 
				AllowWebBrowserDrop = false, 
				IsWebBrowserContextMenuEnabled = false, 
				WebBrowserShortcutsEnabled = false
			};
			Controls.Add(m_browser);
		}

		public void Navigate(string url)
		{
			m_browser.Navigate(url);
		}

		public void DisplayHtml(string html)
		{
			((GeckoWebBrowser)m_browser.NativeBrowser).LoadHtml(html);
		}
	}
}
