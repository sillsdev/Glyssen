using System.Collections.Generic;
using Glyssen.Shared.Bundle;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.WritingSystems;

namespace GlyssenEngine.Paratext
{
	public interface IParatextScrTextWrapper
	{
		IEnumerable<int> CanonicalBookNumbersInProject { get; }
		string LanguageIso3Code { get; }
		string ProjectFullName { get; }
		IEnumerable<string> FailedChecksBooks { get; }
		string RequiredCheckNames { get; }
		bool UserCanEditProject { get; }
		bool HasBooksWithoutProblems { get; }
		IEnumerable<QuotationMark> QuotationMarks { get; }
		bool HasQuotationRulesSet { get; }
		IStylesheet Stylesheet { get; }
		WritingSystemDefinition WritingSystem { get; }
		IReadOnlyList<Book> AvailableBooks { get; }
		void GetUpdatedBookInfo();
		IEnumerable<UsxDocument> UsxDocumentsForIncludedBooks { get; }
		string GetBookChecksum(int bookNum);
		bool DoesBookPassChecks(int bookNumber, bool refreshInfoIfNeeded = false);
		IEnumerable<string> GetCheckFailuresForBook(string bookId);
		void IgnoreQuotationsProblems();
		void IncludeBooks(IEnumerable<string> booksToInclude);
		void IncludeOverriddenBooksFromProject(Project project);
	}
}
