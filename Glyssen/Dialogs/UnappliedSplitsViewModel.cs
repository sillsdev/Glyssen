using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Scripture;

namespace Glyssen.Dialogs
{
	public class UnappliedSplitsViewModel
	{
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\"/>" +
								"<style>{0}</style></head><body>{1}</body></html>";

		private string m_style;
		private readonly IEnumerable<BookScript> m_books;
		private readonly bool m_rightToLeft;

		public UnappliedSplitsViewModel(IEnumerable<BookScript> books, bool rightToLeft)
		{
			m_books = books;
			m_rightToLeft = rightToLeft;
		}
		
		public string GetHtml()
		{
			var bldr = new StringBuilder();
			foreach (var book in m_books.Where(b => b.UnappliedSplits.Any()))
				bldr.Append(BuildBookHtml(book));

			return string.Format(kHtmlFrame, m_style, bldr);
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
			BCVRef endRef = new BCVRef(bookNum, chapterNumber, unappliedSplit[unappliedSplit.Count-1].LastVerseNum);
			return BCVRef.MakeReferenceString(startRef, endRef, ":", "-");
		}

		private string BuildBlockHtml(Block block)
		{
			var bldr = new StringBuilder();
			var blockDivAttributes = "class=\"block" + (m_rightToLeft ? " right-to-left" : "") + "\"";
			bldr.AppendFormat("<div {0}>", blockDivAttributes);

			bldr.AppendFormat("<strong>{0}: </strong>", block.CharacterId);
			bldr.Append(block.GetTextAsHtml(true, m_rightToLeft));
			bldr.Append("</div>");
			return bldr.ToString();
		}

		public void ClearData()
		{
			foreach (var book in m_books)
				book.ClearUnappliedSplits();
		}
	}
}
