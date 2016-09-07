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

	public class ReferenceText : ProjectBase
	{
		protected readonly ReferenceTextType m_referenceTextType;
		private string m_projectFolder;
		private readonly HashSet<string> m_modifiedBooks = new HashSet<string>();

		private static readonly Dictionary<ReferenceTextIdentifier, ReferenceText> s_instantiatedReferenceTexts = new Dictionary<ReferenceTextIdentifier, ReferenceText>();

		public static ReferenceText GetStandardReferenceText(ReferenceTextType referenceTextType)
		{
			return GetReferenceText(ReferenceTextIdentifier.GetOrCreate(referenceTextType));
		}

		public static ReferenceText GetReferenceText(ReferenceTextIdentifier id)
		{
			ReferenceText referenceText;
			if (s_instantiatedReferenceTexts.TryGetValue(id, out referenceText))
				referenceText.ReloadModifiedBooks();
			else
			{
				referenceText = new ReferenceText(id.Metadata, id.Type, id.GetProjectFolder());
				referenceText.LoadBooks();
				switch (id.Type)
				{
					case ReferenceTextType.English:
					//case ReferenceTextType.Azeri:
					//case ReferenceTextType.French:
					//case ReferenceTextType.Indonesian:
					//case ReferenceTextType.Portuguese:
					case ReferenceTextType.Russian:
						//case ReferenceTextType.Spanish:
						//case ReferenceTextType.TokPisin:
						referenceText.m_vers = ScrVers.English;
						break;
				}
				s_instantiatedReferenceTexts[id] = referenceText;
			}
			return referenceText;
		}

		///// <summary>
		///// This version is just for unit tests. No entry is made in the static list of instantiated reference texts,
		///// and no books are loaded.
		///// </summary>
		///// <param name="metadata"></param>
		//internal static ReferenceText CreateTestReferenceText(GlyssenDblTextMetadata metadata)
		//{
		//	return new ReferenceText(metadata, ReferenceTextType.Custom);
		//}

		private BookScript TryLoadBook(string[] files, string bookCode)
		{
			var fileName = files.FirstOrDefault(f => Path.GetFileName(f) == bookCode + kBookScriptFileExtension);
			return fileName != null ? XmlSerializationHelper.DeserializeFromFile<BookScript>(fileName) : null;
		}

		private string[] BookScriptFiles { get { return Directory.GetFiles(ProjectFolder, "???" + kBookScriptFileExtension); } }

		private void LoadBooks()
		{
			var files = BookScriptFiles;

			for (int i = 1; i <= BCVRef.LastBook; i++)
			{
				string bookCode = BCVRef.NumberToBookCode(i);
				var bookScript = TryLoadBook(files, bookCode);
				if (bookScript != null)
					m_books.Add(bookScript);
			}
		}

		private void ReloadModifiedBooks()
		{
			if (!m_modifiedBooks.Any())
				return;

			var files = BookScriptFiles;
			for (int i = 0; i < m_books.Count; i++)
			{
				var bookId = m_books[i].BookId;
				if (m_modifiedBooks.Contains(bookId))
				{
					var bookScript = TryLoadBook(files, bookId);
					Debug.Assert(bookScript != null);
					m_books[i] = bookScript;
				}
			}
			m_modifiedBooks.Clear();
		}

		protected ReferenceText(GlyssenDblTextMetadata metadata, ReferenceTextType referenceTextType, string projectFolder)
			: base(metadata, referenceTextType.ToString())
		{
			m_referenceTextType = referenceTextType;
			m_projectFolder = projectFolder;


			GetBookName = bookId =>
			{
				var book = Books.FirstOrDefault(b => b.BookId == bookId);
				return book == null ? null : book.PageHeader;
			};

			if (m_referenceTextType == ReferenceTextType.Custom)
				SetVersification();
		}

		protected virtual void SetVersification()
		{
			Debug.Assert(m_referenceTextType == ReferenceTextType.Custom);
			m_vers = LoadVersification(VersificationFilePath);
		}

		public bool HasSecondaryReferenceText
		{
			get { return m_referenceTextType != ReferenceTextType.English; }
		}

		public string SecondaryReferenceTextLanguageName
		{
			get { return HasSecondaryReferenceText ? "English" : null; }
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
					ApplyTo(clone, project.Versification);
					yield return clone;
				}
			}
		}

		public void ApplyTo(BookScript vernacularBook, ScrVers vernacularVersification)
		{
			ReloadModifiedBooks();

			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var referenceBook = Books.Single(b => b.BookId == vernacularBook.BookId);

			var verseSplitLocationsBasedOnRef = GetVerseSplitLocations(referenceBook, bookNum);
			var verseSplitLocationsBasedOnVern = GetVerseSplitLocations(vernacularBook, bookNum);
			MakesSplits(vernacularBook, bookNum, vernacularVersification, verseSplitLocationsBasedOnRef, "vernacular", LanguageName);

			if (MakesSplits(referenceBook, bookNum, Versification, verseSplitLocationsBasedOnVern, LanguageName, "vernacular"))
				m_modifiedBooks.Add(referenceBook.BookId);

			MatchVernBlocksToReferenceTextBlocks(vernacularBook, vernacularVersification);
		}

		private void MatchVernBlocksToReferenceTextBlocks(BookScript vernacularBook, ScrVers vernacularVersification)
		{
			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var vernBlockList = vernacularBook.GetScriptBlocks();
			var refBook = Books.Single(b => b.BookId == vernacularBook.BookId);
			var refBlockList = refBook.GetScriptBlocks();

			for (int iVernBlock = 0, iRefBlock = 0; iVernBlock < vernBlockList.Count && iRefBlock < refBlockList.Count; iVernBlock++, iRefBlock++)
			{
				var currentVernBlock = vernBlockList[iVernBlock];
				var currentRefBlock = refBlockList[iRefBlock];
				var vernInitStartVerse = new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialStartVerseNumber, vernacularVersification);
				var refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, Versification);

				var type = CharacterVerseData.GetStandardCharacterType(currentVernBlock.CharacterId);
				switch (type)
				{
					case CharacterVerseData.StandardCharacter.BookOrChapter:
						if (currentVernBlock.IsChapterAnnouncement)
						{
							var refChapterBlock = new Block(currentVernBlock.StyleTag, currentVernBlock.ChapterNumber);
							refChapterBlock.BlockElements.Add(new ScriptText(GetFormattedChapterAnnouncement(vernacularBook.BookId, currentVernBlock.ChapterNumber)));
							if (currentRefBlock.IsChapterAnnouncement && currentRefBlock.MatchesReferenceText)
								refChapterBlock.SetMatchedReferenceBlock(currentRefBlock.ReferenceBlocks.Single().Clone());
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
					refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, vernacularVersification);
				}

				var indexOfVernVerseStart = iVernBlock;
				var indexOfRefVerseStart = iRefBlock;
				var vernInitEndVerse = (currentVernBlock.InitialEndVerseNumber == 0) ? vernInitStartVerse :
					new VerseRef(bookNum, currentVernBlock.ChapterNumber, currentVernBlock.InitialEndVerseNumber, vernacularVersification);
				var lastVernVerseFound = FindAllScriptureBlocksThroughVerse(vernBlockList, vernInitEndVerse, ref iVernBlock, bookNum, vernacularVersification, true);
				FindAllScriptureBlocksThroughVerse(refBlockList, lastVernVerseFound, ref iRefBlock, bookNum, Versification, false);

				int numberOfVernBlocksInVerseChunk = iVernBlock - indexOfVernVerseStart + 1;
				int numberOfRefBlocksInVerseChunk = iRefBlock - indexOfRefVerseStart + 1;

				for (int i = 0; i < numberOfVernBlocksInVerseChunk && i < numberOfRefBlocksInVerseChunk; i++)
				{
					var vernBlockInVerseChunk = vernBlockList[indexOfVernVerseStart + i];
					var refBlockInVerseChunk = refBlockList[indexOfRefVerseStart + i];
					if (BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification) ||
						(numberOfVernBlocksInVerseChunk == 1 && numberOfRefBlocksInVerseChunk == 1 &&
						BlocksEndWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification)))
					{
						if (i == numberOfVernBlocksInVerseChunk - 1 && i < numberOfRefBlocksInVerseChunk - 1)
						{
							vernBlockInVerseChunk.MatchesReferenceText = false;
							vernBlockInVerseChunk.ReferenceBlocks =
								new List<Block>(refBlockList.Skip(indexOfRefVerseStart + i).Take(numberOfRefBlocksInVerseChunk - i));
							break;
						}
						vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockList[indexOfRefVerseStart + i]);
					}
					else
					{
						iVernBlock = indexOfVernVerseStart + i;

						int j = 0;
						if (numberOfVernBlocksInVerseChunk - i >= 2)
						{
							// Look from the bottom up
							for (; j + 1 < numberOfVernBlocksInVerseChunk && j + i < numberOfRefBlocksInVerseChunk; j++)
							{
								vernBlockInVerseChunk = vernBlockList[indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - j - 1];
								refBlockInVerseChunk = refBlockList[indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - j - 1];
								if (BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
									vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);
								else
									break;
							}
						}
						vernBlockList[iVernBlock].MatchesReferenceText = false;
						vernBlockList[iVernBlock].ReferenceBlocks =
							new List<Block>(refBlockList.Skip(indexOfRefVerseStart + i).Take(numberOfRefBlocksInVerseChunk - i - j));
						iRefBlock = indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1;

						break;
					}
				}
			}
		}

		private bool BlocksMatch(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var vernInitStartVerse = new VerseRef(bookNum, vernBlock.ChapterNumber, vernBlock.InitialStartVerseNumber, vernacularVersification);
			var refInitStartVerse = new VerseRef(bookNum, refBlock.ChapterNumber, refBlock.InitialStartVerseNumber, Versification);
			return vernInitStartVerse.CompareTo(refInitStartVerse) == 0 &&
				vernBlock.CharacterId == refBlock.CharacterId &&
				BlocksEndWithSameVerse(bookNum, vernBlock, refBlock, vernacularVersification);
		}

		private bool BlocksEndWithSameVerse(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var lastVernVerse = new VerseRef(bookNum, vernBlock.ChapterNumber, vernBlock.LastVerse, vernacularVersification);
			var lastRefVerse = new VerseRef(bookNum, refBlock.ChapterNumber, refBlock.LastVerse, Versification);
			return lastVernVerse.CompareTo(lastRefVerse) == 0;
		}

		private static VerseRef FindAllScriptureBlocksThroughVerse(IReadOnlyList<Block> blockList, VerseRef endVerse, ref int i, int bookNum, ScrVers versification, bool forceEndAtVerseBreak)
		{
			if (forceEndAtVerseBreak)
				endVerse = new VerseRef(bookNum, blockList[i].ChapterNumber, blockList[i].LastVerse, versification);
			for (; ; )
			{
				var nextScriptureBlock = blockList.Skip(i + 1).FirstOrDefault(b => !CharacterVerseData.IsCharacterStandard(b.CharacterId, false));
				if (nextScriptureBlock == null)
					break;
				var nextVerseRef = new VerseRef(bookNum, nextScriptureBlock.ChapterNumber, nextScriptureBlock.InitialStartVerseNumber, versification);
				if (nextVerseRef > endVerse)
					break;
				i++;
				if (forceEndAtVerseBreak)
					endVerse = new VerseRef(bookNum, nextScriptureBlock.ChapterNumber, nextScriptureBlock.LastVerse, versification);
			}
			return forceEndAtVerseBreak ? endVerse :
				new VerseRef(bookNum, blockList[i].ChapterNumber, blockList[i].LastVerse, versification);
		}

		private class VerseSplitLocation
		{
			private readonly VerseRef m_after;
			private readonly VerseRef m_before;
			public VerseRef After { get { return m_after; } }
			public VerseRef Before { get { return m_before; } }

			public VerseSplitLocation(int bookNum, Block prevBlock, Block splitStartBlock, ScrVers versification)
			{
				m_after = new VerseRef(bookNum, prevBlock.ChapterNumber, prevBlock.LastVerse, versification);
				m_before = new VerseRef(bookNum, splitStartBlock.ChapterNumber, splitStartBlock.InitialStartVerseNumber, versification);
			}

			public static implicit operator VerseRef(VerseSplitLocation location)
			{
				return location.After;
			}
		}
		private List<VerseSplitLocation> GetVerseSplitLocations(BookScript referenceBook, int bookNum)
		{
			var splitLocations = new List<VerseSplitLocation>();
			Block prevBlock = null;
			foreach (var refBlock in referenceBook.GetScriptBlocks())
			{
				if (prevBlock != null && refBlock.BlockElements.First() is Verse)
					splitLocations.Add(new VerseSplitLocation(bookNum, prevBlock, refBlock, Versification));
				prevBlock = refBlock;
			}
			return splitLocations;
		}

		/// <summary>
		/// Split blocks in the given book to match verse split locations
		/// </summary>
		/// <returns>A value indicating whether any splits were made</returns>
		private static bool MakesSplits(BookScript bookToSplit, int bookNum, ScrVers versification,
			List<VerseSplitLocation> verseSplitLocations, string descriptionOfProjectBeingSplit,
			string descriptionOfProjectUsedToDetermineSplitLocations)
		{
			if (!verseSplitLocations.Any())
				return false;
			bool splitsMade = false;
			var iSplit = 0;
			VerseRef verseToSplitAfter = verseSplitLocations[iSplit];
			for (int index = 0; index < bookToSplit.Blocks.Count; index++)
			{
				var block = bookToSplit.Blocks[index];
				var initStartVerse = new VerseRef(bookNum, block.ChapterNumber, block.InitialStartVerseNumber,
					versification);
				VerseRef initEndVerse;
				if (block.InitialEndVerseNumber != 0)
					initEndVerse = new VerseRef(bookNum, block.ChapterNumber, block.InitialEndVerseNumber,
						versification);
				else
					initEndVerse = initStartVerse;

				while (initEndVerse > verseToSplitAfter)
				{
					if (iSplit == verseSplitLocations.Count - 1)
						return splitsMade;
					verseToSplitAfter = verseSplitLocations[++iSplit];
				}

				var lastVerse = new VerseRef(bookNum, block.ChapterNumber, block.LastVerse, versification);
				if (lastVerse < verseToSplitAfter)
					continue;

				if (initEndVerse.CompareTo(lastVerse) != 0 && lastVerse >= verseSplitLocations[iSplit].Before)
				{
					versification.ChangeVersification(verseToSplitAfter);
					if (bookToSplit.TrySplitBlockAtEndOfVerse(block, verseToSplitAfter.VerseNum))
						splitsMade = true;
					else
					{
#if DEBUG
						if (!BlockContainsVerseEndInMiddleOfVerseBridge(block, verseToSplitAfter.VerseNum))
						{
							ErrorReport.NotifyUserOfProblem(
								"Attempt to split {0} block to match breaks in the {1} text failed. Book: {2}; Chapter: {3}; Verse: {4}; Block: {5}",
								descriptionOfProjectBeingSplit, descriptionOfProjectUsedToDetermineSplitLocations,
								bookToSplit.BookId, block.ChapterNumber, verseToSplitAfter.VerseNum, block.GetText(true));
						}
#endif
						if (iSplit == verseSplitLocations.Count - 1)
							break;
						verseToSplitAfter = verseSplitLocations[++iSplit];
						index--;
					}
				}
			}
			return splitsMade;
		}

		private static bool BlockContainsVerseEndInMiddleOfVerseBridge(Block block, int verse)
		{
			return block.BlockElements.OfType<Verse>().Any(ve => ve.StartVerse <= verse && ve.EndVerse > verse);
		}

		protected override string ProjectFolder { get { return m_projectFolder; } }
	}
}
