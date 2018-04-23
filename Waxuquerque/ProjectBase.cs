using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using SIL;
using SIL.DblBundle;
using SIL.Scripture;

namespace Waxuquerque
{
	public abstract class ProjectBase
	{
		public const string kShareFileExtension = ".glyssenshare";

		public static ScrVers LoadVersification(string vrsPath)
		{
			return SIL.Scripture.Versification.Table.Implementation.Load(vrsPath, Localizer.GetString("Project.DefaultCustomVersificationName",
				"custom", "Used as the versification name when the versification file does not contain a name."));
		}

		protected static string ProjectsBaseFolder => GlyssenInfo.BaseDataFolder;

		protected readonly GlyssenDblTextMetadataBase m_metadata;
		protected readonly List<BookScript> m_books = new List<BookScript>();
		protected ScrVers m_vers;
		protected string m_recordingProjectName;

		protected ProjectBase(GlyssenDblTextMetadataBase metadata, string recordingProjectName)
		{
			m_metadata = metadata;
			m_recordingProjectName = recordingProjectName;
		}

		public IReadOnlyList<BookScript> Books => m_books;

		public ScrVers Versification => m_vers;

		public string LanguageName => m_metadata.Language.Name;

		public string LanguageIsoCode => m_metadata.Language.Iso;

		public string LanguageLdml => m_metadata.Language.Ldml;

		public string FontFamily => m_metadata.FontFamily;

		public int FontSizeInPoints => m_metadata.FontSizeInPoints;

		public int FontSizeUiAdjustment
		{
			get { return m_metadata.FontSizeUiAdjustment; }
			set { m_metadata.FontSizeUiAdjustment = value; }
		}

		public bool RightToLeftScript => m_metadata.Language.ScriptDirection == "RTL";

		protected abstract string ProjectFolder { get; }

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

		protected string VersificationFilePath => Path.Combine(ProjectFolder, DblBundleFileUtils.kVersificationFileName);
	}
}
