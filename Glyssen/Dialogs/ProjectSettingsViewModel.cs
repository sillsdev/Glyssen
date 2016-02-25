using System;
using System.Linq;
using Glyssen.Bundle;
using OfficeOpenXml.FormulaParsing.Utilities;
using SIL.Scripture;
using SIL.Windows.Forms.WritingSystems;
using ScrVers = Paratext.ScrVers;

namespace Glyssen.Dialogs
{
	public class ProjectSettingsViewModel
	{
		private readonly string m_exampleMultiChapterBookId;
		private readonly string m_exampleMultiChapterBookTitle;
		private readonly string m_exampleSingleChapterBookId;
		private readonly string m_exampleSingleChapterBookTitle;
		private readonly int m_exampleChapterNumber = 2;
		private readonly string m_exampleChapterLabel;
		private readonly string m_exampleFirstChapterLabel;
		private readonly string m_exampleSingleChapterLabel;
		private ChapterAnnouncement m_chapterAnnouncementStyle;

		public ProjectSettingsViewModel(WritingSystemSetupModel wsModel)
		{
			WsModel = wsModel;
		}

		public ProjectSettingsViewModel(Project project)
		{
			Project = project;

			WsModel = new WritingSystemSetupModel(project.WritingSystem)
			{
				CurrentDefaultFontName = project.FontFamily,
				CurrentDefaultFontSize = project.FontSizeInPoints,
				CurrentRightToLeftScript = project.RightToLeftScript
			};

			RecordingProjectName = project.Name;
		    AudioStockNumber = project.AudioStockNumber;
			BundlePath = project.OriginalBundlePath;
			LanguageName = project.LanguageName;
			IsoCode = project.LanguageIsoCode;
			PublicationId = project.Id;
			PublicationName = project.PublicationName;
			Versification = project.Versification;
			m_chapterAnnouncementStyle = project.ChapterAnnouncementStyle;
			SkipChapterAnnouncementForFirstChapter = project.SkipChapterAnnouncementForFirstChapter;
			SkipChapterAnnouncementForSingleChapterBooks = SkipChapterAnnouncementForFirstChapter || project.SkipChapterAnnouncementForSingleChapterBooks;

			var block = project.IncludedBooks.SelectMany(book => book.GetScriptBlocks().Where(b => b.BlockElements.OfType<Verse>().Any()))
				.FirstOrDefault();
			if (block != null)
				SampleText = block.GetText(false);

			var multiChapterBooks = project.IncludedBooks.Where(book => Versification.LastChapter(BCVRef.BookToNumber(book.BookId)) > 1);
			foreach (var book in multiChapterBooks)
			{
				var chapterBlocks = book.GetScriptBlocks().Where(b => b.IsChapterAnnouncement).Take(2).ToList();
				if (chapterBlocks.Any())
				{
					m_exampleFirstChapterLabel = chapterBlocks.First().BlockElements.OfType<ScriptText>().First().Content;

					if (chapterBlocks.Count > 1)
					{
						m_exampleChapterLabel = chapterBlocks[1].BlockElements.OfType<ScriptText>().First().Content;
						m_exampleChapterNumber = chapterBlocks[1].ChapterNumber;
						m_exampleMultiChapterBookId = book.BookId;
						var title = book.GetScriptBlocks().FirstOrDefault(b => b.StyleTag == "mt");
						if (title != null)
							m_exampleMultiChapterBookTitle = title.GetText(false);
					}
					break;
				}
			}
			var singleChapterBook = project.IncludedBooks.FirstOrDefault(book => Versification.LastChapter(BCVRef.BookToNumber(book.BookId)) == 1);
			if (singleChapterBook != null)
			{
				var chapterBlock = singleChapterBook.GetScriptBlocks().FirstOrDefault(b => b.IsChapterAnnouncement);
				if (chapterBlock != null)
					m_exampleSingleChapterLabel = chapterBlock.BlockElements.OfType<ScriptText>().First().Content;
				m_exampleSingleChapterBookId = singleChapterBook.BookId;
				var title = singleChapterBook.GetScriptBlocks().FirstOrDefault(b => b.StyleTag == "mt");
				if (title != null)
					m_exampleSingleChapterBookTitle = title.GetText(false);
			}
		}

		public Project Project { get; private set; }
		public WritingSystemSetupModel WsModel { get; private set; }
		public string LanguageName { get; private set; }
		public string IsoCode { get; private set; }
		public string PublicationName { get; private set; }
        public string RecordingProjectName { get; set; }
        public string AudioStockNumber { get; set; }
		public string BundlePath { get; set; }
		public string PublicationId { get; private set; }
		public string SampleText { get; private set; }
		public ScrVers Versification { get; private set; }
		public bool SkipChapterAnnouncementForFirstChapter { get; set; }
		public bool SkipChapterAnnouncementForSingleChapterBooks { get; set; }

		/// <summary>
		/// For implementation reasons, the setter alters static members on Block and Project. Therefore,
		/// is reset if the user presses Cancel. Therefore, client MUST call
		/// <seealso cref="RevertChapterAnnouncementStyle"/> if ChapterAnnouncementStyle has been changed
		/// but that change is not to be persisted.
		/// </summary>
		public ChapterAnnouncement ChapterAnnouncementStyle
		{
			get { return m_chapterAnnouncementStyle; }
			set
			{
				m_chapterAnnouncementStyle = value;
				Project.SetBlockGetChapterAnnouncement(value);
			}
		}

		public bool ChapterAnnouncementIsStrictlyNumeric
		{
			get
			{
				var example = ExampleSubsequentChapterAnnouncement;
				return example != null && example.All(Char.IsNumber);
			}
		}

		public string ExampleSubsequentChapterAnnouncement
		{
			get
			{
				if (ChapterAnnouncementStyle == ChapterAnnouncement.ChapterLabel)
					return m_exampleChapterLabel;
				return Project.GetFormattedChapterAnnouncement(m_exampleMultiChapterBookId, m_exampleChapterNumber) ?? m_exampleChapterLabel;
			}
		}

		public string ExampleFirstChapterAnnouncement
		{
			get
			{
				if (SkipChapterAnnouncementForFirstChapter)
					return String.Empty;
				if (ChapterAnnouncementStyle == ChapterAnnouncement.ChapterLabel)
					return m_exampleFirstChapterLabel;
				return Project.GetFormattedChapterAnnouncement(m_exampleMultiChapterBookId, 1) ?? m_exampleFirstChapterLabel;
			}
		}

		public string ExampleSingleChapterAnnouncement
		{
			get
			{
				if (SkipChapterAnnouncementForFirstChapter || SkipChapterAnnouncementForSingleChapterBooks)
					return String.Empty;
				if (ChapterAnnouncementStyle == ChapterAnnouncement.ChapterLabel)
					return m_exampleSingleChapterLabel;
				return Project.GetFormattedChapterAnnouncement(m_exampleSingleChapterBookId, 1) ?? m_exampleSingleChapterLabel;
			}
		}

		public string ExampleTitleForMultipleChapterBook
		{
			get { return m_exampleMultiChapterBookTitle; }
		}

		public string ExampleTitleForSingleChapterBook
		{
			get { return m_exampleSingleChapterBookTitle; }
		}

		/// <summary>
		/// This method MUST be called if the client has changed
		/// <seealso cref="ChapterAnnouncementStyle"/> but does not wish to persist that change.
		/// </summary>
		public void RevertChapterAnnouncementStyle()
		{
			if (m_chapterAnnouncementStyle != Project.ChapterAnnouncementStyle)
				ChapterAnnouncementStyle = Project.ChapterAnnouncementStyle;
		}
	}
}
