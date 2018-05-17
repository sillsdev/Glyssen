using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using SIL.Windows.Forms.HtmlBrowser;

namespace Glyssen.UI.Controls
{
	public partial class Browser : UserControl
	{
		private readonly GeckoWebBrowser m_geckoBrowser;

		public new event EventHandler<DomMouseEventArgs> OnMouseMove;
		public event EventHandler<DomMouseEventArgs> OnMouseOver;
		public new event EventHandler<DomMouseEventArgs> OnMouseClick;
		public event EventHandler<DomMouseEventArgs> OnMouseOut;
		public event EventHandler<GeckoDocumentCompletedEventArgs> OnDocumentCompleted;

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
			if (!ReallyDesignMode)
			{
				m_geckoBrowser = (GeckoWebBrowser)browser.NativeBrowser;
				m_geckoBrowser.DomMouseMove += HandleDomMouseMove;
				m_geckoBrowser.DomMouseOver += HandleDomMouseOver;
				m_geckoBrowser.DomClick += HandleDomMouseClick;
				m_geckoBrowser.DomMouseOut += HandleDomMouseOut;
				m_geckoBrowser.DocumentCompleted += HandleDocumentCompleted;
			}
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
			var element = m_geckoBrowser.Document.GetElementById(elementId);
			if (element == null)
				return;
			var div = new GeckoDivElement(element.DomObject);
			div.ScrollIntoView(true);
			div.Parent.ScrollTop += adjustment;
		}

		private bool ReallyDesignMode
		{
			get
			{
				return (DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		public GeckoWindow Window
		{
			get { return m_geckoBrowser.Window; }
		}

		public void SelectAll()
		{
			m_geckoBrowser.SelectAll();
		}

		public void SelectNone()
		{
			m_geckoBrowser.SelectNone();
		}

		public bool CanCopySelection
		{
			get { return m_geckoBrowser.CanCopySelection; }
		}

		public bool CopySelection()
		{
			return m_geckoBrowser.CopySelection();
		}

		#region browser events
		private void HandleDomMouseMove(object sender, DomMouseEventArgs e)
		{
			EventHandler<DomMouseEventArgs> handler = OnMouseMove;
			if (handler != null)
				handler(this, e);
		}

		private void HandleDomMouseOver(object sender, DomMouseEventArgs e)
		{
			EventHandler<DomMouseEventArgs> handler = OnMouseOver;
			if (handler != null)
				handler(this, e);
		}

		private void HandleDomMouseClick(object sender, DomMouseEventArgs e)
		{
			EventHandler<DomMouseEventArgs> handler = OnMouseClick;
			if (handler != null)
				handler(this, e);

			e.Handled = true;  // don't let the browser navigate itself
		}

		private void HandleDomMouseOut(object sender, DomMouseEventArgs e)
		{
			EventHandler<DomMouseEventArgs> handler = OnMouseOut;
			if (handler != null)
				handler(this, e);
		}

		private void HandleDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			EventHandler<GeckoDocumentCompletedEventArgs> handler = OnDocumentCompleted;
			if (handler != null)
				handler(this, e);
		}
		#endregion
	}
}
