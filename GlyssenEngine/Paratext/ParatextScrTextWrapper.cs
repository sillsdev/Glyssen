using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Bundle;
using Paratext.Data;
using Paratext.Data.Checking;
using Paratext.Data.ProjectSettingsAccess;
using SIL;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.Reporting;
using SIL.Scripture;
using SIL.WritingSystems;
using static System.String;
// IBundle is probably an unfortunate name. IProjectSourceMetadata would have been a better name.
using IProjectSourceMetadata = SIL.DblBundle.IBundle;

namespace GlyssenEngine.Paratext
{
	public class ParatextScrTextWrapper : IParatextScrTextWrapper, IProjectSourceMetadata
	{
		internal const string kLiveParatextProjectType = "live Paratext project";
		public const string kParatextProgramName = "Paratext";
		public const string kMarkersCheckId = "Marker";
		public const string kQuotationCheckId = "Quotation";
		public const string kChapterVerseCheckId = "ChapterVerse";

		private readonly List<string> m_requiredChecks = new List<string>(new [] {kMarkersCheckId, kQuotationCheckId, kChapterVerseCheckId});
		private ScrStylesheetAdapter m_stylesheet;
		private WritingSystemDefinition m_writingSystem;
		private GlyssenDblTextMetadata m_metadata;
		private readonly ParatextProjectBookInfo m_bookInfo = new ParatextProjectBookInfo();

		protected ScrText UnderlyingScrText { get; }

		private ScrVers Versification => UnderlyingScrText.Settings.Versification;
		public IEnumerable<int> CanonicalBookNumbersInProject {
			get
			{
				UnderlyingScrText.SetBooksPresent(); // This ensures list is up-to-date wrt the file system.
				return UnderlyingScrText.BooksPresentSet.SelectedBookNumbers.Where(Canon.IsCanonical);
			}
		}

		public string LanguageIso3Code => UnderlyingScrText.Language.LanguageId.Iso6393Code;
		public string ProjectFullName => UnderlyingScrText.FullName;
		// REVIEW (PG-63): In all cases where FailedChecksBooks is accessed, analyze whether UserCanEditProject should be
		// taken into account. Maybe FailedChecksBooks should always return an empty list when !UserCanEditProject.
		public IEnumerable<string> FailedChecksBooks => m_bookInfo.FailedChecksBooks;
		public string RequiredCheckNames => Join(Localizer.GetString("Common.SimpleListSeparator", ", "),
			m_requiredChecks.Select(ParatextProjectBookInfo.LocalizedCheckName));
		private string ProjectName => UnderlyingScrText.Name;
		// Note: Technically this is not absolutely guaranteed to be unique, but Paratext assumes
		// that in the rare case where there are two projects with the same short name, they will
		// have different full names. Using the ID (GUID) would guarantee uniqueness but it would
		// not be very helpful to a normal user.
		private string UniqueProjectNameForUi => ScrTextCollection.Find(ProjectName) == null ? $"{ProjectName} ({ProjectFullName})" : ProjectName;
		public bool UserCanEditProject => UnderlyingScrText.Permissions.AmAdministratorOrTeamMember;
		public bool HasBooksWithoutProblems => m_bookInfo.HasBooksWithoutProblems;
		public virtual IEnumerable<QuotationMark> QuotationMarks
		{
			get
			{
				var qmInfo = new QuotationMarkInfo(UnderlyingScrText.Settings, false);
				if (qmInfo.Quotes.IsSet)
				{
					yield return new QuotationMark(qmInfo.Quotes.Begin, qmInfo.Quotes.End, qmInfo.Continuer.Continuer, 1, QuotationMarkingSystemType.Normal);
					if (qmInfo.InnerQuotes.IsSet)
					{
						yield return new QuotationMark(qmInfo.InnerQuotes.Begin, qmInfo.InnerQuotes.End, qmInfo.InnerContinuer.Continuer, 2, QuotationMarkingSystemType.Normal);
						if (qmInfo.InnerInnerQuotes.IsSet)
							yield return new QuotationMark(qmInfo.InnerInnerQuotes.Begin, qmInfo.InnerInnerQuotes.End, qmInfo.InnerInnerContinuer.Continuer, 3, QuotationMarkingSystemType.Normal);
					}
				}
				// REVIEW: It is possible in Paratext to define all three levels for the "alternate" quote system and not
				// have them be "narrative" (i.e., dialog) quotes at all. It is also possible to have alternates set at level 2 and/or 3
				// without having them set at the higher levels.
				if (qmInfo.LevelHasAlternate(1))
				{
					yield return new QuotationMark(qmInfo.Quotes.AltBegin, qmInfo.Quotes.AltEnd, qmInfo.Continuer.AltContinuer, 1, QuotationMarkingSystemType.Narrative);
				}
			}
		}

		public virtual string ReportingClauseStartDelimiter => null;
		public virtual string ReportingClauseEndDelimiter => null;

		public bool HasQuotationRulesSet => QuotationMarks?.Any() ?? false;

		public ParatextScrTextWrapper(ScrText underlyingText, bool ignoreQuotationProblemsIfRulesAreNotSet = false)
		{
			UnderlyingScrText = underlyingText;

			if (ignoreQuotationProblemsIfRulesAreNotSet && !HasQuotationRulesSet)
				IgnoreQuotationsProblems();
			else
				GetUpdatedBookInfo();
			if (m_bookInfo.SupportedBookCount == 0)
				throw new NoSupportedBooksException(UniqueProjectNameForUi, m_bookInfo);
		}

		public void GetUpdatedBookInfo()
		{
			CheckingStatuses checkingStatusData;

			try
			{
				checkingStatusData = CheckingStatuses.Get(UnderlyingScrText);
				checkingStatusData.CancelChanges(); // This forces it to reload from disk.
			}
			catch (Exception e)
			{
				throw new ApplicationException($"Unexpected error retrieving the checking status data for {kParatextProgramName} project: {UniqueProjectNameForUi}", e);
			}
			foreach (var bookNum in CanonicalBookNumbersInProject)
			{
				var code = Canon.BookNumberToId(bookNum);
				if (!Canon.IsBookOTNT(bookNum))
				{
					m_bookInfo.Add(bookNum, code, ParatextProjectBookInfo.BookState.ExcludedNonCanonical);
					continue;
				}

				var failedChecks = new List<String>(m_requiredChecks.Count);
				foreach (var check in m_requiredChecks)
				{
					CheckingStatus status;
					try
					{
						status = checkingStatusData.GetCheckingStatus(code, check);
					}
					catch (Exception e)
					{
						throw new ApplicationException($"Unexpected error retrieving the {check} check status for {code} in {kParatextProgramName} project: {UniqueProjectNameForUi}", e);
					}
					if (status == null || !status.Successful)
						failedChecks.Add(check);
				}
				if (failedChecks.Any())
					m_bookInfo.Add(bookNum, code, ParatextProjectBookInfo.BookState.FailedCheck, failedChecks);
				else
					m_bookInfo.Add(bookNum, code, ParatextProjectBookInfo.BookState.NoProblem);
			}
		}

		public IStylesheet Stylesheet => m_stylesheet ??
			(m_stylesheet = new ScrStylesheetAdapter(UnderlyingScrText.DefaultStylesheet));

		public WritingSystemDefinition WritingSystem
		{
			get
			{
				if (m_writingSystem == null)
				{
					try
					{
						var ldmlAdaptor = new LdmlDataMapper(new WritingSystemFactory());
						m_writingSystem = new WritingSystemDefinition();
						ldmlAdaptor.Read(UnderlyingScrText.LdmlPath, m_writingSystem);
					}
					catch (Exception e)
					{
						ErrorReport.NotifyUserOfProblem(e, Localizer.GetString("Project.FailedToCreateWritingSystemFromParatextLdml",
							"Failed to create Writing System based on LDML file for {0} project {1}",
							"Param 0: \"Paratext\"; " +
							"Param 1: Unique Paratext project identifier"),
							kParatextProgramName,
							UniqueProjectNameForUi);
					}
				}
				return m_writingSystem;
			}
		}

		public IReadOnlyList<Book> AvailableBooks => GlyssenDblTextMetadata.AvailableBooks;

		internal GlyssenDblTextMetadata GlyssenDblTextMetadata
		{
			get
			{
				if (m_metadata == null)
				{
					var languageInfo = UnderlyingScrText.Language;
					m_metadata = new GlyssenDblTextMetadata
					{
						Id = UnderlyingScrText.Settings.DBLId.Id,
						ParatextProjectId = ProjectName,
						Type = kLiveParatextProjectType,
						Revision = 1,
						Versification = Versification.Name,
						Identification = new DblMetadataIdentification
						{
							Name = UnderlyingScrText.FullName,
							SystemIds = new HashSet<DblMetadataSystemId>(new[]
							{
								new DblMetadataSystemId {Type = GlyssenDblTextMetadata.kParatextSystemId, Id = UnderlyingScrText.Guid.Id}
							})
						},

						Language = new GlyssenDblMetadataLanguage
						{
							Name = languageInfo.LanguageId.Iso639Name ?? languageInfo.LanguageTag.Name ?? UnderlyingScrText.DisplayLanguageName,
							FontFamily = languageInfo.FontName,
							FontSizeInPoints = languageInfo.FontSize,
							Iso = languageInfo.LanguageId.Iso6393Code,
							Ldml = languageInfo.LanguageTag.Code,
							ScriptDirection = languageInfo.RightToLeft ? "RTL" : "LTR"
						},
						AvailableBooks = new List<Book>()
					};

					if (!IsNullOrEmpty(UnderlyingScrText.Settings.Copyright))
						m_metadata.Copyright = new DblMetadataCopyright(UnderlyingScrText.Settings.Copyright);

					if (!IsNullOrEmpty(UnderlyingScrText.Settings.TMSId))
						m_metadata.Identification.SystemIds.Add(new DblMetadataSystemId {Type = "tms", Id = UnderlyingScrText.Settings.TMSId});

					var nameInfo = UnderlyingScrText.BookNames;
					foreach (var bookNum in CanonicalBookNumbersInProject)
					{
						m_metadata.AvailableBooks.Add(GetBook(nameInfo, bookNum, 
							m_bookInfo.GetState(bookNum) == ParatextProjectBookInfo.BookState.NoProblem));
					}
				}
				return m_metadata;
			}
		}

		internal static Book GetBook(BookNames nameInfo, int bookNum, bool includeInScript)
		{
			return new Book
			{
				Abbreviation = nameInfo.GetAbbreviation(bookNum),
				Code = BCVRef.NumberToBookCode(bookNum),
				IncludeInScript = includeInScript,
				LongName = nameInfo.GetLongName(bookNum),
				ShortName = nameInfo.GetShortName(bookNum)
			};
		}

		private UsxDocument GetUsxDocumentForBook(int bookNum)
		{
			return new UsxDocument(UsfmToUsx.ConvertToXmlDocument(UnderlyingScrText, bookNum, UnderlyingScrText.GetText(bookNum)));
		}

		public IEnumerable<UsxDocument> UsxDocumentsForIncludedBooks => GetUsxDocumentsForIncludedParatextBooks();

		internal ParatextUsxBookList GetUsxDocumentsForIncludedParatextBooks(ISet<int> subset = null)
		{
			// Getting the checksum and the checking status at the same time and returning them together ensures that they are really
			// in sync, rather than relying on the caller to get them at the same time. 
			var list = new ParatextUsxBookList();
			foreach (var bookNum in GlyssenDblTextMetadata.AvailableBooks.Where(ab => ab.IncludeInScript).Select(ib => Canon.BookIdToNumber(ib.Code)))
			{
				if (subset != null && !subset.Contains(bookNum))
					continue;
				list.Add(bookNum, GetUsxDocumentForBook(bookNum), UnderlyingScrText.GetBookCheckSum(bookNum),
					m_bookInfo.GetState(bookNum) == ParatextProjectBookInfo.BookState.NoProblem);
			}
			return list;
		}

		public string GetBookChecksum(int bookNum)
		{
			return UnderlyingScrText.GetBookCheckSum(bookNum);
		}

		public bool DoesBookPassChecks(int bookNumber, bool refreshInfoIfNeeded = false)
		{
			if (m_bookInfo.GetState(bookNumber) == ParatextProjectBookInfo.BookState.NoProblem)
				return true; // We assume this is still up-to-date
			if (!refreshInfoIfNeeded)
				return false;
			GetUpdatedBookInfo();
			return DoesBookPassChecks(bookNumber, false);
		}

		public IEnumerable<string> GetCheckFailuresForBook(string bookId)
		{
			return m_bookInfo.GetFailedChecks(Canon.BookIdToNumber(bookId));
		}

		public void IgnoreQuotationsProblems()
		{
			m_requiredChecks.Remove(kQuotationCheckId);
			GetUpdatedBookInfo();
			m_metadata = null;
		}

		public void IncludeBooks(IEnumerable<string> booksToInclude)
		{
			var set = new HashSet<string>(booksToInclude);
			foreach (var bookMetadata in GlyssenDblTextMetadata.AvailableBooks.Where(b => !b.IncludeInScript && set.Contains(b.Code)))
				bookMetadata.IncludeInScript = true;
		}

		public void IncludeOverriddenBooksFromProject(Project project)
		{
			IncludeBooks(project.IncludedBooks.Select(b => b.BookId));
		}

		public string Id => GlyssenDblTextMetadata.Id;
		public string LanguageIso => LanguageIso3Code;
		public string Name => ProjectFullName;
	}
}

