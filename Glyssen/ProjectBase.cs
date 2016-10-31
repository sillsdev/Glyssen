using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glyssen.Bundle;
using L10NSharp;
using Paratext;
using SIL.DblBundle;

namespace Glyssen
{
	public abstract class ProjectBase
	{
		public const string kProjectFileExtension = ".glyssen";
		public const string kShareFileExtension = ".glyssenshare";
		protected const string kBookScriptFileExtension = ".xml";
		private static Dictionary<string, ScrVers> s_loadedVersifications = new Dictionary<string, ScrVers>();

		static ProjectBase()
		{
			// Force the built-in English versification to get loaded before any project-specific one might get
			// loaded. In the unlikely (except in tests) case that some other versification were to get loaded
			// with the name "English", that would become the official English versification.
			if (ScrVers.English.Name != "English")
				throw new ApplicationException("English versification could not be loaded from ParatextShared resources. Abandon all hope!");
		}

		public static ScrVers LoadVersification(string vrsPath)
		{
			// Rather than blindy loading a new versification each time we're asked to do so, we want to avoid duplicates
			// (either two versifications loaded from the same location or two versifications with identical mappings).
			// In tests, at least, this provides a slight performance improvement, and it should also help if a bundle
			// uses a "standard" vrs file that matches that of the reference text. It could also save a little bit of memory
			// and it might help guard against some future change in Paratext that makes comparing versifications inefficient
			// (which is what originally prompted this code), so it generally seems better.
			ScrVers vers;
			if (!s_loadedVersifications.TryGetValue(vrsPath, out vers))
			{
				vers = Paratext.Versification.Table.Load(vrsPath,
					LocalizationManager.GetString("Project.DefaultCustomVersificationName",
						"custom", "Used as the versification name when a the versification file does not contain a name."));
				if (vers.Equals(ScrVers.English))
					vers = ScrVers.English;
				else
				{
					var identicalVers = s_loadedVersifications.Values.FirstOrDefault(v => v.Equals(vers));
					if (identicalVers != null)
						vers = identicalVers;
				}
				s_loadedVersifications[vrsPath] = vers;
			}
			return vers;
		}

		protected static string ProjectsBaseFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Program.kCompany, Program.kProduct);
			}
		}

		protected readonly GlyssenDblTextMetadata m_metadata;
		protected readonly List<BookScript> m_books = new List<BookScript>();
		protected ScrVers m_vers;
		protected string m_recordingProjectName;

		protected ProjectBase(GlyssenDblTextMetadata metadata, string recordingProjectName)
		{
			m_metadata = metadata;
			m_recordingProjectName = recordingProjectName;
		}

		public IReadOnlyList<BookScript> Books { get { return m_books; } }

		public ScrVers Versification
		{
			get { return m_vers; }
		}

		public string LanguageName
		{
			get { return m_metadata.Language.Name; }
		}

		public string FontFamily
		{
			get { return m_metadata.FontFamily; }
		}

		public int FontSizeInPoints
		{
			get { return m_metadata.FontSizeInPoints; }
		}

		public int FontSizeUiAdjustment
		{
			get { return m_metadata.FontSizeUiAdjustment; }
			set { m_metadata.FontSizeUiAdjustment = value; }
		}

		public bool RightToLeftScript
		{
			get { return m_metadata.Language.ScriptDirection == "RTL"; }
		}

		protected abstract string ProjectFolder { get; }

		protected Func<string, string> GetBookName { get; set; }

		public string GetFormattedChapterAnnouncement(string bookCode, int chapterNumber)
		{
			if (GetBookName == null)
				return null;
			var bookName = GetBookName(bookCode);
			if (string.IsNullOrWhiteSpace(bookName))
				return null;

			StringBuilder bldr = new StringBuilder();
			bldr.Append(bookName);
			bldr.Append(" ");
			bldr.Append(chapterNumber);
			return bldr.ToString();
		}

		protected string VersificationFilePath
		{
			get { return Path.Combine(ProjectFolder, DblBundleFileUtils.kVersificationFileName); }
		}
	}
}
