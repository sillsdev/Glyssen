using System;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using Palaso.UI.WindowsForms.HtmlBrowser;

namespace ProtoScript.Controls
{
	public partial class Browser : UserControl
	{
		private readonly GeckoWebBrowser m_geckoBrowser;

		public Browser()
		{
			InitializeComponent();

			var browser = new XWebBrowser(XWebBrowser.BrowserType.GeckoFx)
			{
				Parent = this, 
				Dock = DockStyle.Fill, 
				AllowWebBrowserDrop = false, 
				IsWebBrowserContextMenuEnabled = false, 
				WebBrowserShortcutsEnabled = false
			};
			m_geckoBrowser = (GeckoWebBrowser)browser.NativeBrowser;
			Controls.Add(browser);
		}

		public void Navigate(string url)
		{
			m_geckoBrowser.Navigate(url);
		}

		public void DisplayHtml(string html)
		{
			m_geckoBrowser.LoadHtml(html);
		}

		public void ScrollElementIntoView(string elementId, int adjustment = 0)
		{
			var div = new GeckoDivElement(m_geckoBrowser.Document.GetElementById(elementId).DomObject);
			div.ScrollIntoView(true);
			div.Parent.ScrollTop += adjustment;
		}

		public void AddDocumentCompletedEventHandler(EventHandler<GeckoDocumentCompletedEventArgs> eventHandler)
		{
			m_geckoBrowser.DocumentCompleted += eventHandler;
		}

		public void RemoveDocumentCompletedEventHandler(EventHandler<GeckoDocumentCompletedEventArgs> eventHandler)
		{
			m_geckoBrowser.DocumentCompleted -= eventHandler;
		}
	}
}
