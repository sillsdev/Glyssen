using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using SIL.Reporting;
using SIL.Scripture;

namespace GlyssenEngine
{
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
			lock (s_instantiatedReferenceTexts)
			{
				if (s_instantiatedReferenceTexts.TryGetValue(id, out referenceText))
					referenceText.ReloadModifiedBooks();
				else
				{
					referenceText = new ReferenceText(id.Metadata, id.Type, id.ProjectFolder);
					referenceText.LoadBooks();
					s_instantiatedReferenceTexts[id] = referenceText;
				}
			}

			return referenceText;
		}

		public ReferenceTextType Type => m_referenceTextType;

		private BookScript TryLoadBook(string[] files, string bookCode)
		{
			var fileName = files.FirstOrDefault(f => Path.GetFileName(f) == bookCode + Constants.kBookScriptFileExtension);
			return fileName != null ? BookScript.Deserialize(fileName, Versification) : null;
		}

		private string[] BookScriptFiles => Directory.GetFiles(ProjectFolder, "???" + Constants.kBookScriptFileExtension);

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
			lock (m_modifiedBooks)
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
		}

		protected ReferenceText(GlyssenDblTextMetadataBase metadata, ReferenceTextType referenceTextType, string projectFolder)
			: base(metadata, referenceTextType.ToString())
		{
			m_referenceTextType = referenceTextType;
			m_projectFolder = projectFolder;

			GetBookName = bookId => GetBook(bookId)?.PageHeader;

			switch (Type)
			{
				case ReferenceTextType.English:
				case ReferenceTextType.Russian:
					SetVersification(ScrVers.English);
					break;
				default:
					SetVersification();
					break;
			}
		}

		protected virtual void SetVersification()
		{
			Debug.Assert(m_referenceTextType == ReferenceTextType.Custom);
			if (File.Exists(VersificationFilePath))
			{
				SetVersification(LoadVersification(VersificationFilePath));
			}
			else
			{
				Logger.WriteMinorEvent($"Custom versification file for proprietary reference text used by this project not found: {VersificationFilePath} - Using standard English versification.");
				SetVersification(ScrVers.English);
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
		/// <param name="project">The project</param>
		/// <param name="applyNarratorOverrides">A value indicating whether to apply the narrator
		/// overrides. This will take a bit more processing and can be safely forgone if caller just
		/// wants to collect statistical information about assignments/alignments. But for any use in
		/// "phase 2" where the effective character is needed, this should be true.</param>
		public IEnumerable<BookScript> GetBooksWithBlocksConnectedToReferenceText(Project project, bool applyNarratorOverrides = true)
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
					var clone = book.GetCloneWithJoinedBlocks(applyNarratorOverrides);
					ApplyTo(clone);
					yield return clone;
				}
			}
		}

		public bool HasContentForBook(string bookId)
		{
			return Books.Any(b => b.BookId == bookId);
		}

		internal void ApplyTo(BookScript vernacularBook)
		{
			lock (m_modifiedBooks)
			{
				ReloadModifiedBooks();

				int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
				var referenceBook = Books.Single(b => b.BookId == vernacularBook.BookId);

				var verseSplitLocationsBasedOnRef = GetVerseSplitLocations(referenceBook, bookNum);
				var verseSplitLocationsBasedOnVern = GetVerseSplitLocations(vernacularBook, bookNum);
				MakesSplits(vernacularBook, bookNum, verseSplitLocationsBasedOnRef, "vernacular", LanguageName, true);

				if (MakesSplits(referenceBook, bookNum, verseSplitLocationsBasedOnVern, LanguageName, "vernacular"))
					m_modifiedBooks.Add(referenceBook.BookId);

				MatchVernBlocksToReferenceTextBlocks(vernacularBook.GetScriptBlocks(), vernacularBook.BookId, vernacularBook.Versification, vernacularBook.SingleVoice);
			}
		}

		public bool CanDisplayReferenceTextForBook(BookScript vernacularBook)
		{
			return Books.Any(b => b.BookId == vernacularBook.BookId);
		}

		public bool IsOkayToSplitBeforeBlock(BookScript vernBook, Block block, List<VerseSplitLocation> verseSplitLocationsBasedOnRef)
		{
			VerseRef startVerse = block.StartRef(vernBook);
			startVerse.ChangeVersification(Versification); // faster to do this once up-front.
			return verseSplitLocationsBasedOnRef.Any(s => s.Before.CompareTo(startVerse) == 0);
		}

		public BlockMatchup GetBlocksForVerseMatchedToReferenceText(BookScript vernacularBook, int iBlock,
			uint predeterminedBlockCount = 0, bool allowSplitting = true)
		{
			if (iBlock < 0 || iBlock >= vernacularBook.GetScriptBlocks().Count)
				throw new ArgumentOutOfRangeException("iBlock");

			if (!CanDisplayReferenceTextForBook(vernacularBook))
				return null;

			int bookNum = BCVRef.BookToNumber(vernacularBook.BookId);
			var verseSplitLocationsBasedOnRef = GetVerseSplitLocations(vernacularBook.BookId);

			Action<PortionScript> splitBlocks = allowSplitting ? portion =>
			{
				MakesSplits(portion, bookNum, verseSplitLocationsBasedOnRef, "vernacular", LanguageName);
			} : (Action < PortionScript >)null;

			var matchup = new BlockMatchup(vernacularBook, iBlock, splitBlocks,
				block => IsOkayToSplitBeforeBlock(vernacularBook, block, verseSplitLocationsBasedOnRef),
				this, predeterminedBlockCount);

			if (!matchup.AllScriptureBlocksMatch)
			{
				if (allowSplitting)
				{
					lock(m_modifiedBooks)
					{
						MatchVernBlocksToReferenceTextBlocks(matchup.CorrelatedBlocks, vernacularBook.BookId, vernacularBook.Versification);
					}
				}
				else
					MatchVernBlocksToReferenceTextBlocks(matchup.CorrelatedBlocks, vernacularBook.BookId, vernacularBook.Versification,
						allowSplitting: false);
			}
			return matchup;
		}

		private void MatchVernBlocksToReferenceTextBlocks(IReadOnlyList<Block> vernBlockList, string bookId, ScrVers vernacularVersification,
			bool forceMatch = false, bool allowSplitting = true)
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
				var currentVernBlock = vernBlockList[iVernBlock];

				if (iRefBlock >= refBlockList.Count)
				{
					if (iRefBlockMemory < 0)
					{
						Debug.Assert(iVernBlock > 0);
						if (currentVernBlock.LastVerseNum == vernBlockList[iVernBlock - 1].LastVerseNum)
						{
							// Either this is a section head, in which case we don't care until we get to the
							// next actual Scripture block; or the vernacular happens to have more blocks in the
							// last verse at the end of the book, in which case they will just go unmatched.
							continue;
						}

						// We still have more vernacular verses to consider. Most likely, this is an
						// additional alternate ending (in Mark).
						if (bookId != "MRK")
							Logger.WriteMinorEvent("Reference text matching went off end of ref block list for " +
								$"vern block {currentVernBlock}");
						var verse = currentVernBlock.StartRef(bookNum, vernacularVersification);
						verse.ChangeVersification(Versification);
						iRefBlock = refBook.GetIndexOfFirstBlockForVerse(verse.ChapterNum, verse.VerseNum);
						if (iRefBlock < 0)
							break;
					}
					else
					{
						// This handles the only case I know of (and for which there is a test) where a versification pulls in verses
						// from the end of the book to an earlier spot, namely Romans 14:24-26 <- Romans 16:25-27. If we ever have this same
						// kind of behavior that is pulling from somewhere other than the *end* of the book, the logic to reset iRefBlock
						// based on iRefBlockMemory will need to be moved/added elsewhere.
						iRefBlock = iRefBlockMemory;
						iRefBlockMemory = -1;
					}
				}


				var currentRefBlock = refBlockList[iRefBlock];
				var vernInitStartVerse = currentVernBlock.StartRef(bookNum, vernacularVersification);
				var refInitStartVerse = currentRefBlock.StartRef(bookNum, Versification);

				var type = CharacterVerseData.GetStandardCharacterType(currentVernBlock.CharacterId);
				switch (type)
				{
					case CharacterVerseData.StandardCharacter.BookOrChapter:
						if (currentVernBlock.IsChapterAnnouncement)
						{
							var refChapterBlock = new Block(currentVernBlock.StyleTag, currentVernBlock.ChapterNumber) { CharacterId = currentVernBlock.CharacterId };
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
										var workingRefInitStartVerse = workingRefBlock.StartRef(bookNum, Versification);

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
							if (!currentRefBlock.IsChapterAnnouncement && currentRefBlock.ChapterNumber == currentVernBlock.ChapterNumber)
								iRefBlock--;
							continue;
						}
						goto case CharacterVerseData.StandardCharacter.ExtraBiblical; // Book title
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

				var refLastVerse = currentRefBlock.EndRef(bookNum, Versification);

				while (CharacterVerseData.IsCharacterExtraBiblical(currentRefBlock.CharacterId) || vernInitStartVerse > refLastVerse)
				{
					iRefBlock++;
					if (iRefBlock == refBlockList.Count)
						return; // couldn't find a ref block to use at all.
					currentRefBlock = refBlockList[iRefBlock];
					refLastVerse = currentRefBlock.EndRef(bookNum, Versification);
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
					var refVerse = lastRefBlockInVerseChunk.StartRef(bookNum, Versification);
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
						refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerseChunk).ToList());
					continue;
				}

				for (int i = 0; i < numberOfVernBlocksInVerseChunk && i < numberOfRefBlocksInVerseChunk; i++)
				{
					var vernBlockInVerseChunk = vernBlockList[indexOfVernVerseStart + i];
					var refBlockInVerseChunk = refBlockList[indexOfRefVerseStart + i];
					if (BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
					{
						if (i == numberOfVernBlocksInVerseChunk - 1 && i < numberOfRefBlocksInVerseChunk - 1)
						{
							// This is the last vernacular block in the list, but we have more than one ref block to account for.

							if (numberOfRefBlocksInVerseChunk - i == 2 && // Exactly 2 reference blocks remaining to be assigned
								i > 0 && // There is a preceding vernacular block
								// The following line could be uncommented to constrain this only to the original intended condition
								// if we find cases where we are coming in here but shouldn't:
								//vernBlockInVerseChunk.CharacterIs(bookId, CharacterVerseData.StandardCharacter.Narrator) &&
								// Can only safely combine reference blocks if they are for the same character:
								vernBlockList[indexOfVernVerseStart + i - 1].ReferenceBlocks.All(r => r.CharacterId == refBlockList[indexOfRefVerseStart + i + 1].CharacterId) &&
								BlocksMatch(bookNum, vernBlockList[indexOfVernVerseStart + i - 1], // Preceding vern block's character & end ref are compatible
								refBlockList[indexOfRefVerseStart + i + 1], vernacularVersification)) // with following ref block
							{
								// This code was specifically written for PG-794, the case where the vernacular has the narrator announcing
								// the speech afterwards instead of beforehand (as is typically the case in the reference text). In that
								// case we want to assign the "he said" reference text to the current vernacular block and attach the
								// following reference text block to the preceding vernacular block.
								// Because we are not explicitly checking to see if this block is a narrator block, this condition can
								// also be matched in other rare cases (for a somewhat contrived example where the reference
								// text has a trailing "he said" but the vernacular does not, see unit test
								// ApplyTo_MultipleSpeakersInVerse_SpeakersBeginCorrespondingThenDoNotCorrespond_ReferenceTextCopiedIntoBestMatchedVerseBlocks.)
								// Even in such cases, it seems likely that we would want to attach the following reference text
								// block to the preceding vernacular block if it is a better match.
								var precedingVernBlock = vernBlockList[indexOfVernVerseStart + i - 1];
								precedingVernBlock.AppendUnmatchedReferenceBlock(refBlockList[indexOfRefVerseStart + i + 1]);
								vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);
							}
							else
							{
								var remainingRefBlocks = refBlockList.Skip(indexOfRefVerseStart + i).Take(numberOfRefBlocksInVerseChunk - i);
								if (forceMatch)
								{
									// Caller responsible for obtaining lock on m_modifiedBooks
									CombineRefBlocksToCreateMatch(remainingRefBlocks.ToList(), vernBlockInVerseChunk, m_modifiedBooks.Contains(bookId));
								}
								else
									vernBlockInVerseChunk.SetUnmatchedReferenceBlocks(remainingRefBlocks);
							}
							break;
						}
						vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);
					}
					else if (numberOfVernBlocksInVerseChunk == 1 && numberOfRefBlocksInVerseChunk == 1 &&
						BlocksEndWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
					{
						Debug.Assert(i == 0);
						if (allowSplitting && TryMatchBySplittingRefBlock(vernBlockInVerseChunk, refBook, indexOfRefVerseStart))
							iRefBlock++;
						else
						{
							if (forceMatch)
								vernBlockList[iVernBlock].SetMatchedReferenceBlock(refBlockInVerseChunk);
							else
								vernBlockList[iVernBlock].SetUnmatchedReferenceBlocks(new[] {refBlockInVerseChunk});
						}
					}
					else
					{
						iVernBlock = indexOfVernVerseStart + i;
						if (vernBlockList[iVernBlock].CharacterIs(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
							iVernBlock++;

						int j = 0;
						var iLastVernBlockMatchedFromBottomUp = -1;
						if (numberOfVernBlocksInVerseChunk - i >= 2)
						{
							// Look from the bottom up
							for (; j < numberOfVernBlocksInVerseChunk && j + i < numberOfRefBlocksInVerseChunk; j++)
							{
								var iCurrVernBottomUp = indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - j - 1;
								vernBlockInVerseChunk = vernBlockList[iCurrVernBottomUp];
								if (vernBlockInVerseChunk.MatchesReferenceText)
									break;
								refBlockInVerseChunk = refBlockList[indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - j - 1];
								if (BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
								{
									vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);
									iLastVernBlockMatchedFromBottomUp = iCurrVernBottomUp;
								}
								else
									break;
							}
						}
						var numberOfUnmatchedRefBlocks = numberOfRefBlocksInVerseChunk - i - j;
						var remainingRefBlocks = refBlockList.Skip(indexOfRefVerseStart + i).Take(numberOfUnmatchedRefBlocks).ToList();
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
								// Caller responsible for obtaining lock on m_modifiedBooks
								CombineRefBlocksToCreateMatch(remainingRefBlocksList, vernBlockList[iVernBlock], m_modifiedBooks.Contains(bookId));
							}
							else
							{
								if (remainingRefBlocksList.Count == 1 && vernBlockList[iVernBlock].MatchesReferenceText &&
									vernBlockList[iVernBlock].ReferenceBlocks.Single().CharacterId != remainingRefBlocksList[0].CharacterId)
								{
									// See if the immediately following or preceding block is a better match
									var otherIndicesToTry = (i <= iVernBlock) ?
										new [] {iVernBlock - 1, iVernBlock + 1} :
										new [] {iVernBlock + 1, iVernBlock - 1};
									foreach (var iPreOrPost in otherIndicesToTry.Where(o => vernBlockList.Count > o && o >= 0))
									{
										if (vernBlockList[iPreOrPost].ReferenceBlocks.FirstOrDefault()?.CharacterId == remainingRefBlocksList[0].CharacterId ||
											vernBlockList[iPreOrPost].CharacterId == vernBlockList[iVernBlock].CharacterId)
										{
											if (!vernBlockList[iPreOrPost].ReferenceBlocks.Any())
												vernBlockList[iPreOrPost].SetUnmatchedReferenceBlocks(remainingRefBlocksList);
											else if (iPreOrPost < iVernBlock) // Pre
												vernBlockList[iPreOrPost].AppendUnmatchedReferenceBlocks(remainingRefBlocksList);
											else // Post
												vernBlockList[iPreOrPost].InsertUnmatchedReferenceBlocks(0, remainingRefBlocksList);
											remainingRefBlocksList = null;
											break;
										}
									}
								}

								if (remainingRefBlocksList != null)
								{
									if (vernBlockList[iVernBlock].ReferenceBlocks.Any())
									{
										// After matching things up as best we could from the top down and the bottom up, we
										// ran out of suitable "holes" in the vernacular. Since our "target" vernacular block
										// already has a reference block, we either need to prepend or append any other
										// unmatched ref blocks. We don't want to change the order of the ref blocks, so we
										// have to be careful. The variable i represents where we got to in our (top-down)
										// matching, so generally if we didn't get all the way down to our target block, we
										// insert the remaining ref blocks before and if we got past it, then we append to
										// the end. But if we attached the reference block to our target block in the bottom-
										// up matching, then the remaining blocks are actually *before* the existing matched
										// block, so we need to insert. Really, iLastVernBlockMatchedFromBottomUp could just
										// be a Boolean flag: fMatchedTargetVernBlockDuringBottomUpMatching.
										if (i < iVernBlock || i == iLastVernBlockMatchedFromBottomUp)
											vernBlockList[iVernBlock].InsertUnmatchedReferenceBlocks(0, remainingRefBlocksList);
										else
											vernBlockList[iVernBlock].AppendUnmatchedReferenceBlocks(remainingRefBlocksList);
									}
									else
									{
										// One more weird edge case to check to see if we can manage to get a good match: Maybe
										// the remaining ref block contains (the start of) the verse that the remaining vern
										// block starts with. (In this case, the ref block didn't get split because the
										// preceding vern verse was totally missing, but we can split it now and make it match.)
										if (allowSplitting && remainingRefBlocksList.Count == 1 &&
											TryMatchBySplittingRefBlock(vernBlockList[iVernBlock], refBook, indexOfRefVerseStart + i))
										{
											numberOfRefBlocksInVerseChunk++;
										}
										else
											vernBlockList[iVernBlock].SetUnmatchedReferenceBlocks(remainingRefBlocksList);
									}
								}
							}
							iRefBlock = indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1;
						}
						break;
					}
				}
				var indexOfLastVernVerseInVerseChunk = indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - 1;
				if (vernBlockList[indexOfLastVernVerseInVerseChunk].ReferenceBlocks.Any())
					iVernBlock = indexOfLastVernVerseInVerseChunk;
			}
		}

		private static void CombineRefBlocksToCreateMatch(List<Block> remainingRefBlocksList, Block vernBlock, bool clone)
		{
			var refBlock = remainingRefBlocksList[0];
			if (clone)
				refBlock.Clone(Block.ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks);
			for (int rb = 1; rb < remainingRefBlocksList.Count; rb++)
				refBlock.CombineWith(remainingRefBlocksList[rb]);
			vernBlock.SetMatchedReferenceBlock(refBlock);
		}

		/// <summary>
		/// In cases where a ref block contains multiple verses but the vernacular is missing the verse(s) with which it starts, we don't
		/// want to lose the part of the reference text that does correspond to the omitted verses, so it may be necessary to split the
		/// reference text block so the vernacular block can be matched just to the portion that makes sense. Because this changes the
		/// reference block, we need to note that the reference text book is "dirty", so this method should only be called in the context
		/// of a method that has locked m_modifiedBooks; otherwise, the local call here to lock it could result in deadlock.
		/// </summary>
		/// <returns></returns>
		private bool TryMatchBySplittingRefBlock(Block vernBlock, PortionScript refBook, int iRefBlock)
		{
			lock (m_modifiedBooks)
			{
				var refBlockList = refBook.GetScriptBlocks();
				Block refBlock = refBlockList[iRefBlock];
				if (vernBlock.BlockElements[0] is Verse vernBlockInitStartVerse &&
					refBlock.BlockElements.Skip(1).OfType<Verse>().Any(v => v.StartVerse == vernBlockInitStartVerse.StartVerse) &&
					refBook.TrySplitBlockAtEndOfVerse(refBlock, vernBlockInitStartVerse.StartVerse - 1))
				{
					m_modifiedBooks.Add(refBook.BookId);
					var newBlock = refBlockList[iRefBlock + 1];
					Debug.Assert(newBlock.StartsAtVerseStart && newBlock.InitialStartVerseNumber == vernBlockInitStartVerse.StartVerse);
					vernBlock.SetMatchedReferenceBlock(newBlock);
					return true;
				}
			}
			return false;
		}

		private bool BlocksMatch(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var vernInitStartVerse = vernBlock.StartRef(bookNum, vernacularVersification);
			var refInitStartVerse = refBlock.StartRef(bookNum, Versification);
			return vernInitStartVerse.CompareTo(refInitStartVerse) == 0 &&
				// ENHANCE: In passages where there is a narrator override, narrator should be considered a "match" with the override character
				(vernBlock.CharacterId == refBlock.CharacterId || (vernBlock.CharacterIsUnclear && !refBlock.CharacterIsStandard)) &&
				BlocksEndWithSameVerse(bookNum, vernBlock, refBlock, vernacularVersification);
		}

		private bool BlocksEndWithSameVerse(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification)
		{
			var lastVernVerse = vernBlock.EndRef(bookNum, vernacularVersification);
			var lastRefVerse = refBlock.EndRef(bookNum, Versification);
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
				m_after = prevBlock.EndRef(bookNum, versification);
				m_before = splitStartBlock.StartRef(bookNum, versification);
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
		private static bool MakesSplits(PortionScript blocksToSplit, int bookNum,
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
					blocksToSplit.Versification);
				VerseRef initEndVerse;
				if (block.InitialEndVerseNumber != 0)
					initEndVerse = new VerseRef(bookNum, block.ChapterNumber, block.InitialEndVerseNumber,
						blocksToSplit.Versification);
				else
					initEndVerse = initStartVerse;

				while (initEndVerse > verseToSplitAfter)
				{
					if (iSplit == verseSplitLocations.Count - 1)
						return splitsMade;
					verseToSplitAfter = verseSplitLocations[++iSplit];
				}

				var lastVerse = block.EndRef(bookNum, blocksToSplit.Versification);
				if (lastVerse < verseToSplitAfter)
					continue;

				if (initEndVerse.CompareTo(lastVerse) != 0 && lastVerse >= verseSplitLocations[iSplit].Before)
				{
					bool invalidSplitLocation = false;
					verseToSplitAfter.ChangeVersification(blocksToSplit.Versification);
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
								blocksToSplit.BookId, block.ChapterNumber, verseToSplitAfter.VerseNum, block.GetText(true));
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
			return block.BlockElements.OfType<Verse>().Any(ve => ve.StartVerse <= verse && ve.EndVerse >= verse);
		}

		protected override string ProjectFolder { get { return m_projectFolder; } }
	}
}
