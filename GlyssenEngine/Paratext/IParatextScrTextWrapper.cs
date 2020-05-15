using System.Collections.Generic;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.WritingSystems;

namespace GlyssenEngine.Paratext
{
	public interface IParatextScrTextWrapper
	{
		IEnumerable<QuotationMark> QuotationMarks { get; }
		bool HasQuotationRulesSet { get; }
		IStylesheet Stylesheet { get; }
		WritingSystemDefinition WritingSystem { get; }
		IReadOnlyList<Book> AvailableBooks { get; }
		IEnumerable<UsxDocument> UsxDocumentsForIncludedBooks { get; }
		string GetBookChecksum(int bookNum);
		bool DoesBookPassChecks(int bookNumber, bool refreshInfoIfNeeded = false);
		void IncludeBooks(IEnumerable<string> booksToInclude);
	}
}
