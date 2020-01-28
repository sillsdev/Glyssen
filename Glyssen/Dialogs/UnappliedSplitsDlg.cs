using L10NSharp.TMXUtils;
using L10NSharp.UI;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GlyssenEngine;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;

namespace Glyssen.Dialogs
{
	public partial class UnappliedSplitsDlg : Form
	{
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								"<style>{0}</style></head><body>{1}</body></html>";

		private string m_style;
		private readonly string m_projectName;
		private readonly UnappliedSplitsViewModel m_model;
		private readonly IFontInfo<Font> m_font;
		private string m_htmlFilePath;

		public UnappliedSplitsDlg(string projectName, IFontInfo<Font> fontProxy, UnappliedSplitsViewModel model)
		{
			m_projectName = projectName;
			m_font = fontProxy;
			m_model = model;
			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;

			m_browser.Disposed += Browser_Disposed;
		}

		private void HandleStringsLocalized()
		{
			m_lblInstructions.Text = string.Format(m_lblInstructions.Text, m_projectName);
		}

		void Browser_Disposed(object sender, EventArgs e)
		{
			if (m_htmlFilePath != null)
				File.Delete(m_htmlFilePath);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			m_htmlFilePath = Path.ChangeExtension(Path.GetTempFileName(), "htm");
			m_style = string.Format(Block.kCssFrame, m_font.FontFamily, m_font.Size);

			SetHtml();
		}

		private void SetHtml()
		{
			File.WriteAllText(m_htmlFilePath, string.Format(kHtmlFrame, m_style, m_model.GetHtml()));
			m_browser.Navigate(m_htmlFilePath);
		}

		private void BtnCopyToClipboard_Click(object sender, EventArgs e)
		{
			m_browser.SelectAll();
			m_browser.CopySelection();
			m_browser.SelectNone();
		}

		private void BtnClose_Click(object sender, EventArgs e)
		{
			if (!m_checkFinished.Checked)
				return;

			if (m_checkDeleteData.Checked)
				m_model.ClearData();
		}

		private void CheckFinished_CheckedChanged(object sender, EventArgs e)
		{
			m_btnClose.Enabled = m_checkFinished.Checked;
			ControlBox = m_checkFinished.Checked;
		}
	}
}
