using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.Scripture;
using static System.String;

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

			return Format(kHtmlFrame, m_style, bldr);
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
			var currentSplitGroup = new List<Block>();
			foreach (var block in unappliedSplit)
			{
				// REVIEW: Not sure how, but somehow we've ended up with some data in the wild where
				// unrelated splits in different chapters both have a split id of 0. This makes for
				// wonky references. It's be nice to figure out how this happened and make sure it can't
				// still happen, but for now, let's at least make sure we display the references separately.
				if (currentSplitGroup.Any() && (currentSplitGroup[0].SplitId != block.SplitId ||
					currentSplitGroup[0].ChapterNumber != block.ChapterNumber))
				{
					AddHtmlForGroupOfSplits(bldr, bookId, currentSplitGroup);
					currentSplitGroup.Clear();
				}
				currentSplitGroup.Add(block);
			}
			if (currentSplitGroup.Any())
				AddHtmlForGroupOfSplits(bldr, bookId, currentSplitGroup);

			bldr.Append("</div>");
			//			bldr.Append("<hr>"); // Doesn't get copied to clipboard
			bldr.Append("<div>&nbsp;</div>");
			return bldr.ToString();
		}

		private void AddHtmlForGroupOfSplits(StringBuilder bldr, string bookId, List<Block> currentSplitGroup)
		{
			bldr.Append(CreateReferenceForSplits(BCVRef.BookToNumber(bookId), currentSplitGroup));
			bldr.Append(Join(" //SPLIT// ", currentSplitGroup.Select(BuildBlockHtml)));
		}

		private string CreateReferenceForSplits(int bookNum, IList<Block> groupOfSplits)
		{
			int chapterNumber = groupOfSplits[0].ChapterNumber;
			BCVRef startRef = new BCVRef(bookNum, chapterNumber, groupOfSplits[0].InitialStartVerseNumber);
			BCVRef endRef = new BCVRef(bookNum, chapterNumber, groupOfSplits.Last().LastVerseNum);
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
