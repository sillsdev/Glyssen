using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Bundle;
using Glyssen.Shared.Bundle;
using L10NSharp;
using Paratext.Data;
using Paratext.Data.Checking;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.DblBundle.Usx;
using SIL.Reporting;
using SIL.Scripture;
using SIL.WritingSystems;

namespace Glyssen.Paratext
{
	internal class ParatextScrTextWrapper
	{
		internal const string kLiveParatextProjectType = "live Paratext project";
		public const string kParatextProgramName = "Paratext";
		public const string kMarkersCheckId = "Marker";
		public const string kQuotationCheckId = "Quotation";
		public const string kChapterVerseCheckId = "ChapterVerse";
		private static readonly string[] s_requiredChecks = { kMarkersCheckId, kQuotationCheckId, kChapterVerseCheckId };

		private ScrStylesheetAdapter m_stylesheet;
		private WritingSystemDefinition m_writingSystem;
		private GlyssenDblTextMetadata m_metadata;
		private CheckingStatuses m_checkingStatusData;
		private readonly ParatextProjectBookInfo m_bookInfo = new ParatextProjectBookInfo();

		private ScrText UnderlyingScrText { get; }

		private ScrVers Versification => UnderlyingScrText.Settings.Versification;
		public IEnumerable<int> CanonicalBookNumbersInProject =>
			UnderlyingScrText.JoinedBooksPresentSet.SelectedBookNumbers.Where(Canon.IsCanonical);
		public string LanguageIso3Code => UnderlyingScrText.Language.LanguageId.Iso6393Code;
		public string ProjectFullName => UnderlyingScrText.JoinedFullName;
		public int FailedChecksBookCount => m_bookInfo.FailedChecksBookCount;
		public static string RequiredCheckNames => string.Join(LocalizationManager.GetString("Common.SimpleListSeparator", ", "),
			s_requiredChecks.Select(ParatextProjectBookInfo.LocalizedCheckName));
		private string ProjectId => UnderlyingScrText.Name;
		public bool UserCanEditProject => !UnderlyingScrText.Permissions.HaveRoleNotObserver;
		public bool HasBooksWithoutProblems => m_bookInfo.HasBooksWithoutProblems;

		public ParatextScrTextWrapper(ScrText underlyingText)
		{
			UnderlyingScrText = underlyingText;

			GetBookInfo();
			if (m_bookInfo.SupportedBookCount == 0)
				throw new NoSupportedBooksException(ProjectId, m_bookInfo);
		}

		private void GetBookInfo()
		{
			try
			{
				m_checkingStatusData = CheckingStatuses.Get(UnderlyingScrText);
			}
			catch (Exception e)
			{
				throw new ApplicationException($"Unexpected error retrieving the checking status data for {kParatextProgramName} project: {ProjectId}", e);
			}
			foreach (var bookNum in CanonicalBookNumbersInProject)
			{
				var code = Canon.BookNumberToId(bookNum);
				if (!Canon.IsBookOTNT(bookNum))
				{
					m_bookInfo.Add(bookNum, code, ParatextProjectBookInfo.BookState.ExcludedNonCanonical);
					continue;
				}

				var failedChecks = new List<String>(s_requiredChecks.Length);
				foreach (var check in s_requiredChecks)
				{
					CheckingStatus status;
					try
					{
						status = m_checkingStatusData.GetCheckingStatus(code, check);
					}
					catch (Exception e)
					{
						throw new ApplicationException($"Unexpected error retrieving the {check} check status for {code} in {kParatextProgramName} project: {ProjectId}", e);
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

		public WritingSystemDefinition WritingSystemDefinition
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
						ErrorReport.NotifyUserOfProblem(e, LocalizationManager.GetString("Project.FailedToCreateWritingSystemFromParatextLdml",
							"Failed to create Writing System based on LDML file for {0} project {1}",
							"Param 0: \"Paratext\"; Param 1: Project short name (unique project identifier)"), ProjectId);
					}
				}
				return m_writingSystem;
			}
		}

		public GlyssenDblTextMetadata GlyssenDblTextMetadata
		{
			get
			{
				if (m_metadata == null)
				{
					var languageInfo = UnderlyingScrText.Language;
					m_metadata = new GlyssenDblTextMetadata
					{
						Id = UnderlyingScrText.Settings.DBLId,
						ParatextProjectId = ProjectId,
						Type = kLiveParatextProjectType,
						Revision = 1,
						Versification = Versification.Name,
						Identification = new DblMetadataIdentification
						{
							Name = UnderlyingScrText.JoinedFullName,
							SystemIds = new HashSet<DblMetadataSystemId>(new[]
							{
								new DblMetadataSystemId {Type = "paratext", Id = UnderlyingScrText.Settings.Guid}
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

					if (!String.IsNullOrEmpty(UnderlyingScrText.Settings.TMSId))
						m_metadata.Identification.SystemIds.Add(new DblMetadataSystemId {Type = "tms", Id = UnderlyingScrText.Settings.TMSId});

					var nameInfo = UnderlyingScrText.BookNames;
					foreach (var bookNum in CanonicalBookNumbersInProject)
					{
						m_metadata.AvailableBooks.Add(new Book
						{
							Abbreviation = nameInfo.GetAbbreviation(bookNum),
							Code = BCVRef.NumberToBookCode(bookNum),
							IncludeInScript = !UserCanEditProject || m_bookInfo.GetState(bookNum) == ParatextProjectBookInfo.BookState.NoProblem,
							LongName = nameInfo.GetLongName(bookNum),
							ShortName = nameInfo.GetShortName(bookNum)
						});
					}
				}
				return m_metadata;
			}
		}


		public IEnumerable<UsxDocument> GetUsxDocumentsForIncludedParatextBooks()
		{
			return m_metadata.AvailableBooks.Where(ab => ab.IncludeInScript).Select(ib => Canon.BookIdToNumber(ib.Code))
				.Select(bookNum => new UsxDocument(UsfmToUsx.ConvertToXmlDocument(
				UnderlyingScrText, bookNum, UnderlyingScrText.GetText(bookNum))));
		}
	}
}

