using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Script;
using SIL;
using SIL.Scripture;

namespace GlyssenEngine
{
	public abstract class ProjectBase : IProject
	{
		public const string kShareFileExtension = ".glyssenshare";

		public static IProjectPersistenceReader Reader { get; set; }

		public ScrVers LoadVersification(bool useFallback = false)
		{
			var versificationResource = useFallback ? ProjectResource.FallbackVersification : ProjectResource.Versification;
			using (var versificationReader = Reader.Load(this, versificationResource))
			{
				if (versificationReader != null)
				{
					return SIL.Scripture.Versification.Table.Implementation.Load(versificationReader,
						versificationResource.ToString(),
						Localizer.GetString("Project.DefaultCustomVersificationName",
						"custom", "Used as the versification name when the versification file does not contain a name."));
				}
			}
			return null;
		}

		protected readonly GlyssenDblTextMetadataBase m_metadata;
		protected readonly List<BookScript> m_books = new List<BookScript>();
		protected string m_recordingProjectName;

		protected ProjectBase(GlyssenDblTextMetadataBase metadata, string recordingProjectName)
		{
			m_metadata = metadata;
			m_recordingProjectName = recordingProjectName;
		}

		public IReadOnlyList<BookScript> Books => m_books;

		public virtual ScrVers Versification { get; private set;  }

		protected virtual void SetVersification(ScrVers versification)
		{
			Versification = versification;
		}

		public string LanguageName => m_metadata.Language.Name;

		public string LanguageIsoCode => m_metadata.Language.Iso;

		public virtual string ValidLanguageIsoCode => LanguageIsoCode;

		public string MetadataId => m_metadata.Id;

		public abstract string Name { get; }

		public string LanguageLdml => m_metadata.Language.Ldml;

		public string FontFamily => m_metadata.FontFamily;

		public int FontSizeInPoints => m_metadata.FontSizeInPoints;

		public int FontSizeUiAdjustment
		{
			get { return m_metadata.FontSizeUiAdjustment; }
			set { m_metadata.FontSizeUiAdjustment = value; }
		}

		public bool RightToLeftScript => m_metadata.Language.ScriptDirection == "RTL";

		protected Func<string, string> GetBookName { get; set; }

		public string GetFormattedChapterAnnouncement(string bookCode, int chapterNumber)
		{
			var bookName = GetBookName?.Invoke(bookCode);
			if (string.IsNullOrWhiteSpace(bookName))
				return null;

			StringBuilder bldr = new StringBuilder();
			bldr.Append(bookName);
			bldr.Append(" ");
			bldr.Append(chapterNumber);
			return bldr.ToString();
		}

		public BookScript GetBook(string id)
		{
#if DEBUG
			// Debug version imposes sanity check
			return Books.SingleOrDefault(b => b.BookId == id);
#else
// Slightly faster in production
			return Books.FirstOrDefault(b => b.BookId == id);
#endif
		}

		public BookScript GetBook(int bookNum)
		{
#if DEBUG
			// Debug version imposes sanity check
			return Books.SingleOrDefault(b => b.BookNumber == bookNum);
#else
// Slightly faster in production
			return Books.FirstOrDefault(b => b.BookNumber == bookNum);
#endif
		}

	}
}
