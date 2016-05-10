using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Character;
using L10NSharp;
using Paratext;
using SIL.Reporting;
using SIL.IO;
using SIL.Scripture;
using SIL.Xml;
using ScrVers = Paratext.ScrVers;

namespace Glyssen
{
	public enum ReferenceTextType
	{
		English,
		//French,
		Custom
	}

	public class ReferenceText : ProjectBase
	{
		public const string kDistFilesReferenceTextDirectoryName = "reference_texts";

		private static ReferenceText s_english;
		public static ReferenceText English
		{
			get
			{
				if (s_english == null)
				{
					s_english = GetStandardReferenceText(ReferenceTextType.English);
					s_english.m_vers = ScrVers.English;
				}
				return s_english;
			}
		}

		private static ReferenceText GetStandardReferenceText(ReferenceTextType referenceTextType)
		{
			string languageName = LocalizationManager.GetDynamicString("Glyssen", "ReferenceText." + referenceTextType, referenceTextType.ToString());

			string referenceProjectFilePath = GetReferenceTextProjectFileLocation(referenceTextType);
			Exception exception;
			var metadata = GlyssenDblTextMetadata.Load<GlyssenDblTextMetadata>(referenceProjectFilePath, out exception);
			if (exception != null)
			{
				Analytics.ReportException(exception);
				ErrorReport.ReportNonFatalExceptionWithMessage(exception,
					LocalizationManager.GetString("ReferenceText.CouldNotLoad", "The {0} reference text could not be loaded: {1}"),
					languageName,
					referenceProjectFilePath);
				return null;
			}

			var referenceText = new ReferenceText(metadata, referenceTextType);

			var projectDir = Path.GetDirectoryName(referenceProjectFilePath);
			Debug.Assert(projectDir != null);
			string[] files = Directory.GetFiles(projectDir, "???" + kBookScriptFileExtension);
			for (int i = 1; i <= BCVRef.LastBook; i++)
			{
				string bookCode = BCVRef.NumberToBookCode(i);
				string possibleFileName = Path.Combine(projectDir, bookCode + kBookScriptFileExtension);
				if (files.Contains(possibleFileName))
					referenceText.m_books.Add(XmlSerializationHelper.DeserializeFromFile<BookScript>(possibleFileName));
			}
			return referenceText;
		}

		private static string GetReferenceTextProjectFileLocation(ReferenceTextType referenceTextType)
		{
			string projectFileName;
			switch (referenceTextType)
			{
				default:
					projectFileName = "eng.glyssen";
					break;
			}
			return FileLocator.GetFileDistributedWithApplication(kDistFilesReferenceTextDirectoryName, referenceTextType.ToString(), projectFileName);
		}


		private readonly ReferenceTextType m_referenceTextType;

		public ReferenceText(GlyssenDblTextMetadata metadata, ReferenceTextType referenceTextType)
			: base(metadata, referenceTextType.ToString())
		{
			m_referenceTextType = referenceTextType;

			GetBookName = bookId =>
			{
				var book = Books.FirstOrDefault(b => b.BookId == bookId);
				return book == null ? null : book.PageHeader;
			};
		}

		/// <summary>
		/// This gets the included books from the project. As needed, blocks are broken up and matched to
		/// correspond to this reference text (in which case, the books and blocks returned are copies, so
		/// that the project itself is not modified).
		/// </summary>
		public IEnumerable<BookScript> GetBooksWithBlocksConnectedToReferenceText(Project project)
		{
			foreach (var book in project.IncludedBooks)
			{
				var referenceBook = Books.SingleOrDefault(b => b.BookId == book.BookId);
				// REVIEW: Should we allow a reference text to be hooked up that does not have all the
				// books in the vernacular? For now, Jon at FCBH says yes.
				if (referenceBook == null)
					yield return book;
				else
				{
					var clone = book.Clone(true);
					ApplyTo(clone, referenceBook.GetScriptBlocks(), GetFormattedChapterAnnouncement, project.Versification, Versification);
					yield return clone;
				}
			}
		}

		public static void ApplyTo(BookScript vernacularBook, IEnumerable<Block> referenceTextBlocks,
			Func<string, int, string> formatReferenceTextChapterAnnouncement,
			ScrVers vernacularVersification, ScrVers referenceVersification)
		{
			var refBlockList = referenceTextBlocks.ToList();

			SplitVernBlocksToMatchReferenceText(vernacularBook, refBlockList, vernacularVersification, referenceVersification);

			MatchVernBlocksToReferenceTextBlocks(vernacularBook, refBlockList, formatReferenceTextChapterAnnouncement, vernacularVersification, referenceVersification);
		}

		private static void MatchVernBlocksToReferenceTextBlocks(BookScript vernacularBook, List<Block> refBlockList,
			Func<string, int, string> formatReferenceTextChapterAnnouncement,
			ScrVers vernacularVersification, ScrVers referenceVersification)
		{
			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var vernBlockList = vernacularBook.GetScriptBlocks();

			for (int iVernBlock = 0, iRefBlock = 0; iVernBlock < vernBlockList.Count && iRefBlock < refBlockList.Count; iVernBlock++, iRefBlock++)
			{
				var currentVernBlock = vernBlockList[iVernBlock];
				var currentRefBlock = refBlockList[iRefBlock];
				var vernInitStartVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialStartVerseNumber, vernacularVersification);
				var refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, referenceVersification);

				var type = CharacterVerseData.GetStandardCharacterType(currentVernBlock.CharacterId);
				switch (type)
				{
					case CharacterVerseData.StandardCharacter.BookOrChapter:
						if (currentVernBlock.IsChapterAnnouncement)
						{
							var refChapterBlock = new Block(currentVernBlock.StyleTag, currentVernBlock.ChapterNumber);
							refChapterBlock.BlockElements.Add(new ScriptText(formatReferenceTextChapterAnnouncement(vernacularBook.BookId, currentVernBlock.ChapterNumber)));
							currentVernBlock.SetMatchedReferenceBlock(refChapterBlock);
							if (currentRefBlock.IsChapterAnnouncement)
								continue;
						}
						goto case CharacterVerseData.StandardCharacter.ExtraBiblical;
					case CharacterVerseData.StandardCharacter.ExtraBiblical:
						if (type == CharacterVerseData.GetStandardCharacterType(currentRefBlock.CharacterId))
						{
							currentVernBlock.SetMatchedReferenceBlock(currentRefBlock);
							continue;
						}
						goto case CharacterVerseData.StandardCharacter.Intro;
					case CharacterVerseData.StandardCharacter.Intro:
						// This will be re-incremented in the for loop, so it effectively allows
						// the vern index to advance while keeping the ref index the same.
						iRefBlock--;
						continue;
					default:
						if (refInitStartVerse > vernInitStartVerse)
						{
							iRefBlock--;
							continue;
						}
						break;
				}

				while (CharacterVerseData.IsCharacterStandard(currentRefBlock.CharacterId, false) || vernInitStartVerse > refInitStartVerse)
				{
					iRefBlock++;
					currentRefBlock = refBlockList[iRefBlock];
					refInitStartVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, vernacularVersification);
				}

				var indexOfVernVerseStart = iVernBlock;
				var indexOfRefVerseStart = iRefBlock;
				var vernInitEndVerse = (currentVernBlock.InitialEndVerseNumber == 0) ? vernInitStartVerse :
					new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialEndVerseNumber, vernacularVersification);
				var refInitEndVerse = (currentRefBlock.InitialEndVerseNumber == 0) ? refInitStartVerse :
					new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialEndVerseNumber, referenceVersification);

				FindAllScriptureBlocksThroughVerse(vernBlockList, vernInitEndVerse, ref iVernBlock, bookNum, vernacularVersification);
				FindAllScriptureBlocksThroughVerse(refBlockList, vernInitEndVerse, ref iRefBlock, bookNum, referenceVersification);

				int numberOfVernBlocksInVerse = iVernBlock - indexOfVernVerseStart + 1;
				int numberOfRefBlocksInVerse = iRefBlock - indexOfRefVerseStart + 1;

				if (vernInitStartVerse.CompareTo(refInitStartVerse) == 0 && vernInitEndVerse.CompareTo(refInitEndVerse) == 0 &&
					((numberOfVernBlocksInVerse == 1 && numberOfRefBlocksInVerse == 1) ||
					(numberOfVernBlocksInVerse == numberOfRefBlocksInVerse &&
					vernBlockList.Skip(indexOfVernVerseStart).Take(numberOfVernBlocksInVerse).Select(b => b.CharacterId).SequenceEqual(refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerse).Select(b => b.CharacterId)))))
				{
					for (int i = 0; i < numberOfVernBlocksInVerse; i++)
						vernBlockList[indexOfVernVerseStart + i].SetMatchedReferenceBlock(refBlockList[indexOfRefVerseStart + i]);
				}
				else
				{
					currentVernBlock.MatchesReferenceText = false;
					currentVernBlock.ReferenceBlocks = new List<Block>(refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerse));
				}
			}
		}

		private static void FindAllScriptureBlocksThroughVerse(IReadOnlyList<Block> blockList, VerseRef endVerse, ref int i, int bookNum, ScrVers versification)
		{
			for (; ; )
			{
				var nextScriptureBlock = blockList.Skip(i + 1).FirstOrDefault(b => !CharacterVerseData.IsCharacterStandard(b.CharacterId, false));
				if (nextScriptureBlock == null)
					return;
				var nextVerseRef = new VerseRef(bookNum, nextScriptureBlock.ChapterNumber, nextScriptureBlock.InitialStartVerseNumber, versification);
				if (nextVerseRef > endVerse)
					return;
				i++;
			}
		}

		private static bool NextScriptureBlockStartsWithAVerse(IReadOnlyList<Block> blockList, int i)
		{
			var nextScriptureBlock = blockList.Skip(i + 1).FirstOrDefault(b => !CharacterVerseData.IsCharacterStandard(b.CharacterId, false));
			return nextScriptureBlock != null && nextScriptureBlock.BlockElements.First() is Verse;
		}

		private static void SplitVernBlocksToMatchReferenceText(BookScript vernacularBook, List<Block> refBlockList,
			ScrVers vernacularVersification, ScrVers referenceVersification)
		{
			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var vernBlockList = vernacularBook.GetScriptBlocks();

			for (int iRefBlock = 0, iVernBlock = 0; iRefBlock < refBlockList.Count && iVernBlock < vernBlockList.Count; iRefBlock++, iVernBlock++)
			{
				var currentRefBlock = refBlockList[iRefBlock];
				var currentVernBlock = vernBlockList[iVernBlock];
				var vernInitStartVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialStartVerseNumber, vernacularVersification);
				var refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, referenceVersification);

				while (CharacterVerseData.IsCharacterStandard(currentVernBlock.CharacterId, false) || refInitStartVerse > vernInitStartVerse)
				{
					if (iVernBlock == vernBlockList.Count - 1)
						return;
					currentVernBlock = vernBlockList[++iVernBlock];
					vernInitStartVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialStartVerseNumber, vernacularVersification);
				}
				while (CharacterVerseData.IsCharacterStandard(currentRefBlock.CharacterId, false) || vernInitStartVerse > refInitStartVerse)
				{
					if (iRefBlock == refBlockList.Count - 1)
						return;
					currentRefBlock = refBlockList[++iRefBlock];
					refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, referenceVersification);
				}

				var nextRefScriptureBlockStartsWithAVerse = NextScriptureBlockStartsWithAVerse(refBlockList, iRefBlock);
				if (nextRefScriptureBlockStartsWithAVerse)
				{
					var vernLastVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.LastVerse, vernacularVersification);
					var refLastVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.LastVerse, referenceVersification);
					if (refLastVerse < vernLastVerse)
					{
						vernacularVersification.ChangeVersification(refLastVerse);
						var verseToSplit = refLastVerse.Verse;
						if ((vernInitStartVerse.CompareTo(refLastVerse) == 0 && currentVernBlock.InitialEndVerseNumber == 0) ||
							currentVernBlock.BlockElements.OfType<Verse>().Any(v => v.Number == verseToSplit))
						{
							vernacularBook.SplitBlock(currentVernBlock, verseToSplit, BookScript.kSplitAtEndOfVerse, false);
						}
					}
					else if (!NextScriptureBlockStartsWithAVerse(vernBlockList, iVernBlock))
						iRefBlock--;
				}
			}
		}

		protected override string ProjectFolder
		{
			get { return FileLocator.GetDirectoryDistributedWithApplication(kDistFilesReferenceTextDirectoryName, m_referenceTextType.ToString()); }
		}
	}
}
