using System.Diagnostics;
using Gecko;
using Glyssen.Properties;
using Glyssen.Utilities;
using SIL.IO;
using SIL.Windows.Forms.Extensions;

namespace Glyssen.Dialogs
{
	public partial class HtmlMessageDlg : FormWithPersistedSettings
	{

		public HtmlMessageDlg(string html)
		{
			InitializeComponent();

			SetHtml(html);
		}
			
		public void SetHtml(string html)
		{
			// check if just a fragment was passed
			if (!html.Contains("<body"))
			{
				var doc = Resources.HtmlMsg;
				html = doc.Replace("{0}", html);
			}

			m_browser.DisplayHtml(html);
		}


		private void m_browser_OnMouseClick(object sender, DomMouseEventArgs e)
		{
			if (this.DesignModeAtAll())
				return;

			if (e.Target == null)
				return;

			var element = e.Target.CastToGeckoElement();

			// handle if the user clicked on a hyperlink
			if (element.TagName == "A")
			{
				var url = element.GetAttribute("href");
				if (url.StartsWith("file://"))
				{
					var path = url.Replace("file://", "");

					var classAttr = element.GetAttribute("class");
					if (classAttr != null && classAttr.Contains("showFileLocation"))
					{
						PathUtilities.SelectFileInExplorer(path);
					}
					else
					{
						Process.Start(path);
					}
				}
				else
				{
					Process.Start(url);
				}
			}
		}
	}
}
