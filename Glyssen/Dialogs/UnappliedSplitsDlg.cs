using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Utilities;
using L10NSharp.UI;
using SIL.Scripture;

namespace Glyssen.Dialogs
{
	public partial class UnappliedSplitsDlg : CustomForm
	{
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								"<style>{0}</style></head><body>{1}</body></html>";

		private string m_style;
		private readonly string m_projectName;
		private readonly IEnumerable<BookScript> m_books;
		private readonly string m_fontFamily;
		private readonly int m_fontSize;
		private readonly bool m_rightToLeftScript;
		private string m_htmlFilePath;

		public UnappliedSplitsDlg(string projectName, IWritingSystemDisplayInfo wsInfo, IEnumerable<BookScript> books)
		{
			m_projectName = projectName;
			m_books = books;
			m_fontFamily = wsInfo.FontFamily;
			m_rightToLeftScript = wsInfo.RightToLeft;
			m_fontSize = wsInfo.FontSize;
			InitializeComponent();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

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
			m_style = string.Format(Block.kCssFrame, m_fontFamily, m_fontSize);

			SetHtml();
		}

		private void SetHtml()
		{
			var bldr = new StringBuilder();
			foreach (var book in m_books.Where(b => b.UnappliedSplits.Any()))
				bldr.Append(BuildBookHtml(book));

			File.WriteAllText(m_htmlFilePath, string.Format(kHtmlFrame, m_style, bldr));
			m_browser.Navigate(m_htmlFilePath);
		}

		private string BuildBookHtml(BookScript book)
		{
			var bldr = new StringBuilder();
			bldr.AppendFormat("<div id=\"{0}\" class=\"book\">", book.BookId);
			foreach (var unappliedSplit in book.UnappliedSplits)
				bldr.Append(BuildUnappliedSplitHtml(book.BookId, unappliedSplit.ToList()));
			bldr.Append("</div>");
			return bldr.ToString();
		}

		private string BuildUnappliedSplitHtml(string bookId, IList<Block> unappliedSplit)
		{
			var bldr = new StringBuilder();
			bldr.Append("<div class=\"splits\">");
			bldr.Append(CreateReferenceForUnappliedSplit(BCVRef.BookToNumber(bookId), unappliedSplit));
			foreach (var block in unappliedSplit)
			{
				bldr.Append(BuildBlockHtml(block));
				if (unappliedSplit.IndexOf(block) != unappliedSplit.Count-1)
					bldr.Append(" //SPLIT// ");
			}
			bldr.Append("</div>");
//			bldr.Append("<hr>"); // Doesn't get copied to clipboard
			bldr.Append("<div>&nbsp;</div>");
			return bldr.ToString();
		}

		private string CreateReferenceForUnappliedSplit(int bookNum, IList<Block> unappliedSplit)
		{
			if (unappliedSplit.Count == 0)
				throw new ArgumentException("unappliedSplit must contain at least one block.", "unappliedSplit");

			int chapterNumber = unappliedSplit[0].ChapterNumber;
			BCVRef startRef = new BCVRef(bookNum, chapterNumber, unappliedSplit[0].InitialStartVerseNumber);
			BCVRef endRef = new BCVRef(bookNum, chapterNumber, unappliedSplit[unappliedSplit.Count-1].LastVerse);
			return BCVRef.MakeReferenceString(startRef, endRef, ":", "-");
		}

		private string BuildBlockHtml(Block block)
		{
			var bldr = new StringBuilder();
			var blockDivAttributes = "class=\"block" + (m_rightToLeftScript ? " right-to-left" : "") + "\"";
			bldr.AppendFormat("<div {0}>", blockDivAttributes);

			bldr.AppendFormat("<strong>{0}: </strong>", block.CharacterId);
			bldr.Append(block.GetTextAsHtml(true, m_rightToLeftScript));
			bldr.Append("</div>");
			return bldr.ToString();
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
				foreach (var book in m_books)
					book.ClearUnappliedSplits();
		}

		private void CheckFinished_CheckedChanged(object sender, EventArgs e)
		{
			m_btnClose.Enabled = m_checkFinished.Checked;
			ControlBox = m_checkFinished.Checked;
		}
	}
}
