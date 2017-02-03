﻿using System;
using System.Collections.Generic;
using System.IO;
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

		public static ScrVers LoadVersification(string vrsPath)
		{
			return Paratext.Versification.Table.Load(vrsPath, LocalizationManager.GetString("Project.DefaultCustomVersificationName",
				"custom", "Used as the versification name when the versification file does not contain a name."));
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
