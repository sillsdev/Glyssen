using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Glyssen.Character;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using SIL.Scripture;
using SIL.Xml;

namespace Glyssen
{

	public interface IReferenceLanguageInfo
	{
		bool HasSecondaryReferenceText { get; }
		IReferenceLanguageInfo BackingReferenceLanguage { get; }
		string HeSaidText { get; }
		string WordSeparator { get; }
	}

	public class ReferenceText : ProjectBase, IReferenceLanguageInfo
	{
		protected readonly ReferenceTextType m_referenceTextType;
		private string m_projectFolder;
		private readonly HashSet<string> m_modifiedBooks = new HashSet<string>();

		private static readonly Dictionary<IReferenceTextProxy, ReferenceText> s_instantiatedReferenceTexts = new Dictionary<IReferenceTextProxy, ReferenceText>();

		public static ReferenceText GetStandardReferenceText(ReferenceTextType referenceTextType)
		{
			return GetReferenceText(ReferenceTextProxy.GetOrCreate(referenceTextType));
		}

		public static ReferenceText GetReferenceText(IReferenceTextProxy id)
		{
			ReferenceText referenceText;
			if (s_instantiatedReferenceTexts.TryGetValue(id, out referenceText))
				referenceText.ReloadModifiedBooks();
			else
			{
				referenceText = new ReferenceText(id.Metadata, id.Type, id.ProjectFolder);
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

		public ReferenceTextType Type => m_referenceTextType;

		private BookScript TryLoadBook(string[] files, string bookCode)
		{
			var fileName = files.FirstOrDefault(f => Path.GetFileName(f) == bookCode + Constants.kBookScriptFileExtension);
			return fileName != null ? XmlSerializationHelper.DeserializeFromFile<BookScript>(fileName) : null;
		}

		private string[] BookScriptFiles { get { return Directory.GetFiles(ProjectFolder, "???" + Constants.kBookScriptFileExtension); } }

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

		protected ReferenceText(GlyssenDblTextMetadataBase metadata, ReferenceTextType referenceTextType, string projectFolder)
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
			if (File.Exists(VersificationFilePath))
			{
				m_vers = LoadVersification(VersificationFilePath);
			}
			else
			{
				Logger.WriteMinorEvent($"Custom versification file for proprietary reference text used by this project not found: {VersificationFilePath} - Using standard English versisfication.");
				m_vers = ScrVers.English;
			}
		}

		public bool HasSecondaryReferenceText
		{
			get { return m_referenceTextType != ReferenceTextType.English; }
		}

		public string SecondaryReferenceTextLanguageName
		{
			get { return HasSecondaryReferenceText ? "English" : null; }
		}

		public ReferenceText SecondaryReferenceText
		{
			get { return GetStandardReferenceText(ReferenceTextType.English); }
		}

		public IReferenceLanguageInfo BackingReferenceLanguage
		{
			get { return SecondaryReferenceText; }
		}

		public string WordSeparator
		{
			// ENHANCE: When we support custom reference texts in languages that do not use a simple space character, this will
			// need to be overridable in m_metadata.Language.
			get { return " "; }
		}

		public string HeSaidText
		{
			get { return m_metadata.Language.HeSaidText ?? "he said."; }
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

		public bool HasContentForBook(string bookId)
		{
			return Books.Any(b => b.BookId == bookId);
		}

		internal void ApplyTo(BookScript vernacularBook, ScrVers vernacularVersification)
		{
			ReloadModifiedBooks();

			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var referenceBook = Books.Single(b => b.BookId == vernacularBook.BookId);

			var verseSplitLocationsBasedOnRef = GetVerseSplitLocations(referenceBook, bookNum);
			var verseSplitLocationsBasedOnVern = GetVerseSplitLocations(vernacularBook, bookNum);
			MakesSplits(vernacularBook, bookNum, vernacularVersification, verseSplitLocationsBasedOnRef, "vernacular", LanguageName, true);

			if (MakesSplits(referenceBook, bookNum, Versification, verseSplitLocationsBasedOnVern, LanguageName, "vernacular"))
				m_modifiedBooks.Add(referenceBook.BookId);

			MatchVernBlocksToReferenceTextBlocks(vernacularBook.GetScriptBlocks(), vernacularBook.BookId, vernacularVersification, vernacularBook.SingleVoice);
		}

		public bool CanDisplayReferenceTextForBook(BookScript vernacularBook)
		{
			return Books.Any(b => b.BookId == vernacularBook.BookId);
		}

		public bool IsOkayToSplitAtVerse(VerseRef nextVerse, ScrVers vernacularVersification, List<VerseSplitLocation> verseSplitLocationsBasedOnRef)
		{
			nextVerse.Versification = vernacularVersification;
			return verseSplitLocationsBasedOnRef.Any(s => s.Before.CompareTo(nextVerse) == 0);
		}

		public BlockMatchup GetBlocksForVerseMatchedToReferenceText(BookScript vernacularBook, int iBlock, ScrVers vernacularVersification, uint predeterminedBlockCount = 0)
		{
			if (iBlock < 0 || iBlock >= vernacularBook.GetScriptBlocks().Count)
				throw new ArgumentOutOfRangeException("iBlock");

			if (!CanDisplayReferenceTextForBook(vernacularBook))
				return null;

			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var verseSplitLocationsBasedOnRef = GetVerseSplitLocations(vernacularBook.BookId);

			var matchup = new BlockMatchup(vernacularBook, iBlock, portion =>
			{
				MakesSplits(portion, bookNum, vernacularVersification, verseSplitLocationsBasedOnRef, "vernacular", LanguageName);
			},
			nextVerse => IsOkayToSplitAtVerse(nextVerse, vernacularVersification, verseSplitLocationsBasedOnRef),
			this, predeterminedBlockCount);

			if (!matchup.AllScriptureBlocksMatch)
			{
				MatchVernBlocksToReferenceTextBlocks(matchup.CorrelatedBlocks, vernacularBook.BookId, vernacularVersification);
			}
			return matchup;
		}

		private void MatchVernBlocksToReferenceTextBlocks(IReadOnlyList<Block> vernBlockList, string bookId, ScrVers vernacularVersification, bool forceMatch = false)
		{
			int bookNum = BCVRef.BookToNumber(bookId);
			var refBook = Books.Single(b => b.BookId == bookId);
			var refBlockList = refBook.GetScriptBlocks();
			int iRefBlock = 0;
			int iRefBlockMemory = -1;
			// The following is a minor performance enhancement to help when just hooking up one verse's worth of blocks.
			// In it's current form, it doesn't work for tests that have partial reference texts that don't start at verse 1.
			// Also, it doesn't take versification mappings into consideration.
			//if (vernBlockList[0].ChapterNumber > 1)
			//{
			//	iRefBlock = refBook.GetIndexOfFirstBlockForVerse(vernBlockList[0].ChapterNumber, );
			//	if (iRefBlock < 0)
			//	{
			//		// Apparently there is no reference text for the verse(s) we're interested in.
			//		return;
			//	}
			//}

			for (int iVernBlock = 0; iVernBlock < vernBlockList.Count; iVernBlock++, iRefBlock++)
			{
				if (iRefBlock >= refBlockList.Count && iRefBlockMemory < 0)
				{
					// We still have more vernacular verses to consider. Most likely, this is an
					// additional alternate ending (in Mark). It's not very efficient, but we'll just
					// start back at the beginning of the reference text block collection.
					iRefBlock = 0;
				}

				var currentVernBlock = vernBlockList[iVernBlock];
				// TODO: This handles the only case I know of (and for which there is a test) where a versification pulls in verses
				// from the end of the book to an earlier spot, namely Romans 14:24-26 <- Romans 16:25-27. If we ever have this same
				// kind of behavior that is pulling from somewhere other than the *end* of the book, the logic to reset iRefBlock
				// based on iRefBlockMemory will need to be moved/added elsewhere.
				if (iRefBlockMemory >= 0 && iRefBlock >= refBlockList.Count)
				{
					iRefBlock = iRefBlockMemory;
					iRefBlockMemory = -1;
				}
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
							refChapterBlock.BlockElements.Add(new ScriptText(GetFormattedChapterAnnouncement(bookId, currentVernBlock.ChapterNumber)));

							if (HasSecondaryReferenceText)
							{
								if (currentRefBlock.IsChapterAnnouncement && currentRefBlock.MatchesReferenceText)
								{
									refChapterBlock.SetMatchedReferenceBlock(currentRefBlock.ReferenceBlocks.Single().Clone());
								}
								else if (!currentRefBlock.IsChapterAnnouncement)
								{
									// Find the reference text's chapter announcement to get the secondary reference text chapter announcement
									var i = iRefBlock + 1;
									while (i < refBlockList.Count)
									{
										var workingRefBlock = refBlockList[i];
										var workingRefInitStartVerse = new VerseRef(bookNum, workingRefBlock.ChapterNumber, workingRefBlock.InitialStartVerseNumber, Versification);

										var compareResult = workingRefInitStartVerse.CompareTo(vernInitStartVerse);

										if (compareResult > 0) // break out early; we passed the verse reference, so there is no chapter label
											break;

										if (compareResult == 0 && workingRefBlock.IsChapterAnnouncement && workingRefBlock.MatchesReferenceText)
										{
											refChapterBlock.SetMatchedReferenceBlock(workingRefBlock.ReferenceBlocks.Single().Clone());
											break;
										}

										i++;
									}
								}
							}

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
						if (refInitStartVerse.CompareTo(vernInitStartVerse) > 0 || currentVernBlock.MatchesReferenceText)
						{
							iRefBlock--;
							continue;
						}
						break;
				}

				while (CharacterVerseData.IsCharacterExtraBiblical(currentRefBlock.CharacterId) || vernInitStartVerse > refInitStartVerse)
				{
					iRefBlock++;
					if (iRefBlock == refBlockList.Count)
						return; // couldn't find a ref block to use at all.
					currentRefBlock = refBlockList[iRefBlock];
					refInitStartVerse = new VerseRef(bookNum, currentRefBlock.ChapterNumber, currentRefBlock.InitialStartVerseNumber, vernacularVersification);
				}

				var indexOfVernVerseStart = iVernBlock;
				var indexOfRefVerseStart = iRefBlock;
				BlockMatchup.AdvanceToCleanVerseBreak(vernBlockList, i => true, ref iVernBlock);
				var lastVernVerseFound = new VerseRef(bookNum, vernBlockList[iVernBlock].ChapterNumber, vernBlockList[iVernBlock].LastVerseNum,
					vernacularVersification);
				FindAllScriptureBlocksThroughVerse(refBlockList, lastVernVerseFound, ref iRefBlock, Versification);

				int numberOfVernBlocksInVerseChunk = iVernBlock - indexOfVernVerseStart + 1;
				int numberOfRefBlocksInVerseChunk = iRefBlock - indexOfRefVerseStart + 1;

				for (int i = indexOfVernVerseStart; i <= iVernBlock; i++)
				{
					if (vernBlockList[i].CharacterIs(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
						numberOfVernBlocksInVerseChunk--;
				}

				if (numberOfVernBlocksInVerseChunk == 1 && numberOfRefBlocksInVerseChunk > 1)
				{
					var lastRefBlockInVerseChunk = refBlockList[iRefBlock];
					var refVerse = new VerseRef(bookNum, lastRefBlockInVerseChunk.ChapterNumber, lastRefBlockInVerseChunk.InitialStartVerseNumber, Versification);
					if (lastVernVerseFound.CompareTo(refVerse) == 0 && lastVernVerseFound.BBBCCCVVV < refVerse.BBBCCCVVV)
					{
						// A versification difference has us pulling a verse from later in the text, so we need to get out and look beyond our current
						// index in the ref text, but remember this spot so we can come back to it.
						iRefBlockMemory = indexOfRefVerseStart;
						iRefBlock--;
						iVernBlock--;
						continue;
					}

					// Since there's only one vernacular block for this verse (or verse bridge), just combine all
					// ref blocks into one and call it a match.
					vernBlockList[indexOfVernVerseStart].SetMatchedReferenceBlock(bookNum, vernacularVersification, this,
						refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerseChunk));
					continue;
				}

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
						if (vernBlockList[iVernBlock].CharacterIs(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
							iVernBlock++;

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
						var numberOfUnmatchedRefBlocks = numberOfRefBlocksInVerseChunk - i - j;
						var remainingRefBlocks = refBlockList.Skip(indexOfRefVerseStart + i).Take(numberOfUnmatchedRefBlocks);
						if (numberOfVernBlocksInVerseChunk == 1 && numberOfUnmatchedRefBlocks > 1)
						{
							// Since there's only one vernacular block for this verse (or verse bridge), just combine all
							// ref blocks into one and call it a match.
							vernBlockInVerseChunk.SetMatchedReferenceBlock(bookNum, vernacularVersification, this, remainingRefBlocks);
						}
						else
						{
							var remainingRefBlocksList = remainingRefBlocks.ToList();
							if (!remainingRefBlocksList.Any())
							{
								// do nothing (PG-1085)
							}
							else if (forceMatch)
							{
								var refBlock = remainingRefBlocksList[0];
								for (int rb = 1; rb < remainingRefBlocksList.Count; rb++)
									refBlock.CombineWith(remainingRefBlocksList[rb]);
								vernBlockList[iVernBlock].SetMatchedReferenceBlock(refBlock);
							}
							else
							{
								vernBlockList[iVernBlock].MatchesReferenceText = false;
								vernBlockList[iVernBlock].ReferenceBlocks = remainingRefBlocksList;
							}
							iRefBlock = indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1;
						}
						break;
					}
				}
				var indexOfLastVernVerseInVerseChunk = indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - 1;
				if (vernBlockList[indexOfLastVernVerseInVerseChunk].MatchesReferenceText)
					iVernBlock = indexOfLastVernVerseInVerseChunk;
			}
		}

		private bool BlocksMatch(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var vernInitStartVerse = new VerseRef(bookNum, vernBlock.ChapterNumber, vernBlock.InitialStartVerseNumber, vernacularVersification);
			var refInitStartVerse = new VerseRef(bookNum, refBlock.ChapterNumber, refBlock.InitialStartVerseNumber, Versification);
			return vernInitStartVerse.CompareTo(refInitStartVerse) == 0 &&
				(vernBlock.CharacterId == refBlock.CharacterId || vernBlock.CharacterIsUnclear() ) &&
				BlocksEndWithSameVerse(bookNum, vernBlock, refBlock, vernacularVersification);
		}

		private bool BlocksEndWithSameVerse(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var lastVernVerse = new VerseRef(bookNum, vernBlock.ChapterNumber, vernBlock.LastVerseNum, vernacularVersification);
			var lastRefVerse = new VerseRef(bookNum, refBlock.ChapterNumber, refBlock.LastVerseNum, Versification);
			return lastVernVerse.CompareTo(lastRefVerse) == 0;
		}

		private static void FindAllScriptureBlocksThroughVerse(IReadOnlyList<Block> blockList, VerseRef endVerse, ref int i, ScrVers versification)
		{
			for (; ; )
			{
				var nextScriptureBlock = blockList.Skip(i + 1).FirstOrDefault(b => !CharacterVerseData.IsCharacterExtraBiblical(b.CharacterId));
				if (nextScriptureBlock == null)
					break;
				var nextVerseRef = new VerseRef(endVerse.BookNum, nextScriptureBlock.ChapterNumber, nextScriptureBlock.InitialStartVerseNumber, versification);
				if (nextVerseRef > endVerse)
					break;
				i++;
			}
		}

		public class VerseSplitLocation
		{
			private readonly VerseRef m_after;
			private readonly VerseRef m_before;
			public VerseRef After { get { return m_after; } }
			public VerseRef Before { get { return m_before; } }

			public VerseSplitLocation(int bookNum, Block prevBlock, Block splitStartBlock, ScrVers versification)
			{
				m_after = new VerseRef(bookNum, prevBlock.ChapterNumber, prevBlock.LastVerseNum, versification);
				m_before = new VerseRef(bookNum, splitStartBlock.ChapterNumber, splitStartBlock.InitialStartVerseNumber, versification);
			}

			public static implicit operator VerseRef(VerseSplitLocation location)
			{
				return location.After;
			}
		}

		public List<VerseSplitLocation> GetVerseSplitLocations(string bookId)
		{
			var referenceBook = Books.Single(b => b.BookId == bookId);
			return GetVerseSplitLocations(referenceBook, BCVRef.BookToNumber(bookId));
		}

		private List<VerseSplitLocation> GetVerseSplitLocations(PortionScript script, int bookNum)
		{
			var splitLocations = new List<VerseSplitLocation>();
			Block prevBlock = null;
			foreach (var block in script.GetScriptBlocks())
			{
				if (prevBlock != null && block.StartsAtVerseStart)
					splitLocations.Add(new VerseSplitLocation(bookNum, prevBlock, block, Versification));
				prevBlock = block;
			}
			return splitLocations;
		}

		/// <summary>
		/// Split blocks in the given book to match verse split locations
		/// </summary>
		/// <returns>A value indicating whether any splits were made</returns>
		private static bool MakesSplits(PortionScript blocksToSplit, int bookNum, ScrVers versification,
			List<VerseSplitLocation> verseSplitLocations, string descriptionOfProjectBeingSplit,
			string descriptionOfProjectUsedToDetermineSplitLocations, bool preventSplittingBlocksAlreadyMatchedToRefText = false)
		{
			if (!verseSplitLocations.Any())
				return false;
			bool splitsMade = false;
			var iSplit = 0;
			VerseRef verseToSplitAfter = verseSplitLocations[iSplit];
			var blocks = blocksToSplit.GetScriptBlocks();
			for (int index = 0; index < blocks.Count; index++)
			{
				var block = blocks[index];
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

				var lastVerse = new VerseRef(bookNum, block.ChapterNumber, block.LastVerseNum, versification);
				if (lastVerse < verseToSplitAfter)
					continue;

				if (initEndVerse.CompareTo(lastVerse) != 0 && lastVerse >= verseSplitLocations[iSplit].Before)
				{
					bool invalidSplitLocation = false;
					versification.ChangeVersification(verseToSplitAfter);
					if (preventSplittingBlocksAlreadyMatchedToRefText && block.MatchesReferenceText)
						invalidSplitLocation = blocksToSplit.GetVerseStringToUseForSplittingBlock(block, verseToSplitAfter.VerseNum) == null;
					else if (blocksToSplit.TrySplitBlockAtEndOfVerse(block, verseToSplitAfter.VerseNum))
						splitsMade = true;
					else
						invalidSplitLocation = true;

					if (invalidSplitLocation)
					{
#if DEBUG
						if (!BlockContainsVerseEndInMiddleOfVerseBridge(block, verseToSplitAfter.VerseNum))
						{
							ErrorReport.NotifyUserOfProblem(
								"Attempt to split {0} block to match breaks in the {1} text failed. Book: {2}; Chapter: {3}; Verse: {4}; Block: {5}",
								descriptionOfProjectBeingSplit, descriptionOfProjectUsedToDetermineSplitLocations,
								blocksToSplit.Id, block.ChapterNumber, verseToSplitAfter.VerseNum, block.GetText(true));
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
