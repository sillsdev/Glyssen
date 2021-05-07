using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Glyssen.Shared;
using Glyssen.Shared.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;
using static System.Char;

namespace GlyssenEngine
{
	public class ReferenceText : ProjectBase, IReferenceLanguageInfo, IReferenceTextProject
	{
		internal const string kOpenFirstLevelQuote = "«";
		protected readonly ReferenceTextType m_referenceTextType;
		private readonly HashSet<string> m_modifiedBooks = new HashSet<string>();

		private static readonly Dictionary<IReferenceTextProxy, ReferenceText> s_instantiatedReferenceTexts;

		static ReferenceText()
		{
			s_instantiatedReferenceTexts = new Dictionary<IReferenceTextProxy, ReferenceText>();
			if (Project.Writer != null) // Can be null in dev tools
			{
				Project.Writer.OnProjectDeleted += delegate(object sender, IProject project)
				{
					if (project is IReferenceTextProject refText && refText.Type == ReferenceTextType.Custom)
					{
						s_instantiatedReferenceTexts.RemoveAll(r => r.Key.Type == ReferenceTextType.Custom && r.Key.Name == project.Name);
					}
				};
			}
		}

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
					referenceText = new ReferenceText(id.Metadata, id.Type);
					referenceText.LoadExistingBooks();
					s_instantiatedReferenceTexts[id] = referenceText;
				}
			}

			return referenceText;
		}

		private string GetEnglishVersion(FcbhTestament testament)
		{
			var type = Constants.GetFCBHTestamentVersionSystemId(testament);
			return m_metadata?.Identification?.SystemIds?.FirstOrDefault(sysId => sysId.Type == type)?.Id;
		}

		public string EnglishOTVersion => GetEnglishVersion(FcbhTestament.OT);
		public string EnglishNTVersion => GetEnglishVersion(FcbhTestament.NT);

		public ReferenceTextType Type => m_referenceTextType;
		
		public override string Name => Type == ReferenceTextType.Custom ? LanguageName ?? m_metadata.Name : Type.ToString();

		private BookScript TryLoadBook(string bookCode)
		{
			return BookScript.Deserialize(Reader.LoadBook(this, bookCode), Versification);
		}

		private void ReloadModifiedBooks()
		{
			lock (m_modifiedBooks)
			{
				if (!m_modifiedBooks.Any())
					return;

				for (int i = 0; i < m_books.Count; i++)
				{
					var bookId = m_books[i].BookId;
					if (m_modifiedBooks.Contains(bookId))
					{
						var bookScript = TryLoadBook(bookId);
						Debug.Assert(bookScript != null);
						m_books[i] = bookScript;
					}
				}

				m_modifiedBooks.Clear();
			}
		}

		protected ReferenceText(GlyssenDblTextMetadataBase metadata, ReferenceTextType referenceTextType)
			: base(metadata, referenceTextType.ToString())
		{
			m_referenceTextType = referenceTextType;

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
			var versification = LoadVersification(GlyssenVersificationTable.InvalidVersificationLineExceptionHandling.Throw);
			if (versification != null)
			{
				SetVersification(versification);
			}
			else
			{
				Logger.WriteMinorEvent("Custom versification for proprietary reference text used by " +
					"this project was not found or could not be loaded. Using standard English versification.");
				SetVersification(ScrVers.English);
			}
		}

		public bool HasSecondaryReferenceText => m_referenceTextType != ReferenceTextType.English;

		public ReferenceText SecondaryReferenceText => GetStandardReferenceText(ReferenceTextType.English);

		public IReferenceLanguageInfo BackingReferenceLanguage => SecondaryReferenceText;

		// ENHANCE: When we support custom reference texts in languages that do not use a simple space character, this will
		// need to be overridable in m_metadata.Language.
		public string WordSeparator => " ";

		public string HeSaidText => m_metadata.Language.HeSaidText ?? "he said.";
		private static Regex s_regexEnglishReportingClause = new Regex(@"(?<clause>((?<pronoun>(He)|(She)|(They)) |((The )?(\w+ ){1,2}))said\b([^\.\w]*\w+){0,2})[,:]\s*$");

		private bool BlockIsOmissibleReportingClause(Block refBlock, out string modifiedOmittedHeSaidText)
		{
			modifiedOmittedHeSaidText = null;
			if (HasSecondaryReferenceText)
				refBlock = refBlock.ReferenceBlocks.Single();
			var onlyScriptText = refBlock.BlockElements.OfType<ScriptText>().OnlyOrDefault();
			if (onlyScriptText == null)
				return false;

			var match = s_regexEnglishReportingClause.Match(onlyScriptText.Content);
			if (!match.Success)
				return false;
			modifiedOmittedHeSaidText = match.Groups["clause"] + ".";
			if (match.Groups["pronoun"].Success)
			{
				var sb = new StringBuilder(modifiedOmittedHeSaidText);
				sb[0] = ToLowerInvariant(sb[0]);
				modifiedOmittedHeSaidText = sb.ToString();
			}

			return true;
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
					ApplyTo(clone, project.ReportingClauses);
					yield return clone;
				}
			}
		}

		public bool HasContentForBook(string bookId)
		{
			return Books.Any(b => b.BookId == bookId);
		}

		internal void ApplyTo(BookScript vernacularBook, IReadOnlyCollection<string> reportingClauses = null)
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

				MatchVernBlocksToReferenceTextBlocks(vernacularBook.GetScriptBlocks(),
					vernacularBook.BookId, vernacularBook.Versification, vernacularBook.SingleVoice,
					reportingClauses: reportingClauses);
			}
		}

		public bool CanDisplayReferenceTextForBook(BookScript vernacularBook)
		{
			return Books.Any(b => b.BookId == vernacularBook.BookId);
		}

		public bool IsOkayToSplitBeforeBlock(BookScript vernBook, Block block,
			List<VerseSplitLocation> verseSplitLocationsBasedOnRef)
		{
			VerseRef startVerse = block.StartRef(vernBook);
			startVerse.ChangeVersification(Versification); // faster to do this once up-front.
			return verseSplitLocationsBasedOnRef.Any(s => s.Before.CompareTo(startVerse) == 0);
		}

		/// <summary>
		/// Given a particular block (identified by index) in a book, returns a "matchup" consisting of all the blocks for all
		/// the verses in that block, plus any adjoining verses that do not correspond to a clean break location. Note that the
		/// matchup can include both preceding and following blocks. By design, if the index of any block in the matchup is passed
		/// to this method, it will always return a matchup covering the same set of blocks (unless a predetermined block count is
		/// supplied). Matchup objects are not cached for this purpose, however, so it should not be expected to return the
		/// identical object.
		/// </summary>
		/// <param name="vernacularBook">The book</param>
		/// <param name="iBlock">Index of the "anchor" block for the matchup</param>
		/// <param name="reportingClauses">vernacular strings that can be automatically matched to "he said"</param>
		/// <param name="predeterminedBlockCount">Used to get a fast result if the caller knows the exact extent of the desired
		/// matchup (typically based on a previous call to this method).</param>
		/// <param name="allowSplitting">Flag indicating whether existing reference text blocks can be split (at verse breaks) to
		/// achieve better correspondence to the vernacular blocks.</param>
		public BlockMatchup GetBlocksForVerseMatchedToReferenceText(BookScript vernacularBook, int iBlock,
			IReadOnlyCollection<string> reportingClauses = null, uint predeterminedBlockCount = 0, bool allowSplitting = true)
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

			bool IsCleanBreakLocation(Block block) =>
				IsOkayToSplitBeforeBlock(vernacularBook, block, verseSplitLocationsBasedOnRef);

			var matchup = new BlockMatchup(vernacularBook, iBlock, splitBlocks,
				IsCleanBreakLocation, this, predeterminedBlockCount);

			if (!matchup.AllScriptureBlocksMatch)
			{
				if (allowSplitting)
				{
					lock(m_modifiedBooks)
					{
						MatchVernBlocksToReferenceTextBlocks(matchup.CorrelatedBlocks, vernacularBook.BookId,
							vernacularBook.Versification, reportingClauses: reportingClauses);
					}
				}
				else
					MatchVernBlocksToReferenceTextBlocks(matchup.CorrelatedBlocks, vernacularBook.BookId,
						vernacularBook.Versification, allowSplitting: false);
			}
			return matchup;
		}

		/// <summary>
		/// By examining the blocks (and their currently assigned character IDs) for each verse represented by the given list of
		/// vernacular blocks, the corresponding reference blocks are matched up to achieve the best
		/// possible correspondence. Whenever this results in a reliable match to a single block, the correlation is considered
		/// a "match." Otherwise, the blocks are stored as a sequence (0 or more) of mismatching blocks.
		/// </summary>
		/// <param name="vernBlockList">the list of vernacular (i.e., "target language") blocks (either a whole book's worth -
		/// and never more than one book's worth, or just the correlated blocks for a single matchup). Note that these objects
		/// will usually be modified by this method. If that is not desirable, a cloned list should be passed.</param>
		/// <param name="bookId">The standard SIL three-letter book code</param>
		/// <param name="vernacularVersification">If this is different from the versification used by the reference text (which for
		/// standard reference texts is always English), this will be used to ensure that the reference blocks for the corresponding
		/// verses are used.</param>
		/// <param name="forceMatch">Flag indicating that any block which would otherwise be considered a mismatch should be
		/// considered a match to the corresponding reference text blocks. If there is more than one corresponding block, these
		/// will be joined into a single block for this purpose. (A block can never be regarded as a match to more than one
		/// reference block.)</param>
		/// <param name="allowSplitting">Flag indicating whether existing reference text blocks can be split (at verse breaks) to
		/// achieve better correspondence to the vernacular blocks. IMPORTANT: If this is true, the CALLER is responsible for
		/// obtaining the lock on m_modifiedBooks.</param>
		private void MatchVernBlocksToReferenceTextBlocks(IReadOnlyList<Block> vernBlockList, string bookId, ScrVers vernacularVersification,
			bool forceMatch = false, bool allowSplitting = true, IReadOnlyCollection<string> reportingClauses = null)
		{
			Debug.Assert(!allowSplitting || Monitor.IsEntered(m_modifiedBooks),
				"If reference text blocks are allowed to be split, caller is responsible for obtaining a lock on m_modifiedBooks.");
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
							if (!currentRefBlock.IsChapterAnnouncement)
							{
								// Note: A chapter announcement should always be followed by a
								// Scripture block, so using First should never fail. However in
								// PG-1434, we came across a case where bad data could lead to a
								// crash. We probably want to improve the USX parser to prevent
								// this, but for now, to be safe, if there's no more Scripture,
								// we're done.
								var nextScriptureBlock = vernBlockList.Skip(iVernBlock + 1).FirstOrDefault(b => b.IsScripture);
								if (nextScriptureBlock == null)
									break;
								if (currentRefBlock.EndRef(bookNum, Versification) >= nextScriptureBlock.StartRef(bookNum, vernacularVersification))
									iRefBlock--;
							}

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
						if ((refInitStartVerse > vernInitStartVerse && refInitStartVerse > currentVernBlock.EndRef(bookNum, vernacularVersification)) ||
							currentVernBlock.MatchesReferenceText)
						{
							iRefBlock--;
							continue;
						}
						break;
				}

				while (CharacterVerseData.IsCharacterExtraBiblical(currentRefBlock.CharacterId) || vernInitStartVerse > currentRefBlock.EndRef(bookNum, Versification))
				{
					iRefBlock++;
					if (iRefBlock == refBlockList.Count)
						return; // couldn't find a ref block to use at all.
					currentRefBlock = refBlockList[iRefBlock];
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

					// Since there's only one vernacular block for this verse (or verse bridge),
					// just combine all ref blocks into one and call it a match unless:
					// 1) the start verses don't match (in which case we're probably dealing with
					// a mapping that involved a verse split).
					// 2) The end refs don't match and the ref block has a character speaking in
					// that is unexpected for the end ref (in which case the reference text
					// probably has a mistake and should be looked at).
					var correspondingReferenceBlocks = refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerseChunk).ToList();
					var lastRefBlock = correspondingReferenceBlocks.Last();
					var endRef = lastRefBlock.EndRef(bookNum, Versification);
					if (correspondingReferenceBlocks.First().StartRef(bookNum, Versification).CompareTo(vernInitStartVerse) == 0 &&
						(endRef.CompareTo(lastVernVerseFound) == 0 ||
						!lastRefBlock.IsQuote ||
						ControlCharacterVerseData.Singleton.GetCharacters(bookNum, endRef.ChapterNum, endRef.VerseNum).Any(cv => cv.Character ==  lastRefBlock.CharacterId)))
					{
						currentVernBlock.SetMatchedReferenceBlock(bookNum, vernacularVersification, this,
							correspondingReferenceBlocks);
					}
					else
					{
						currentVernBlock.SetUnmatchedReferenceBlocks(correspondingReferenceBlocks);
					}

					continue;
				}

				var omittedHeSaids = new List<Tuple<int, string>>(2);
				string verseFromOmittedHeSaidBlockToPrepend = null;
				var lookForReportingClauses = reportingClauses != null && reportingClauses.Any();

				for (int i = 0; i < numberOfVernBlocksInVerseChunk && i < numberOfRefBlocksInVerseChunk; i++)
				{
					var vernBlockInVerseChunk = vernBlockList[indexOfVernVerseStart + i];
					var currRefBlockIndex = indexOfRefVerseStart + i + omittedHeSaids.Count;
					var handled = false;
					if (i + omittedHeSaids.Count < numberOfRefBlocksInVerseChunk)
					{
						var refBlockInVerseChunk = refBlockList[currRefBlockIndex];
						if (BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification) &&
							(!vernBlockInVerseChunk.CharacterIsUnclear || omittedHeSaids.Count <= i))
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
										// If allowSplitting is true, the caller was responsible for obtaining the lock on m_modifiedBooks,
										// so we don't need to incur that overhead again here.
										CombineRefBlocksToCreateMatch(remainingRefBlocks.ToList(), vernBlockInVerseChunk, allowSplitting && m_modifiedBooks.Contains(bookId));
									}
									else
										vernBlockInVerseChunk.SetUnmatchedReferenceBlocks(remainingRefBlocks);
								}

								break;
							}

							if (lookForReportingClauses && omittedHeSaids.Any() &&
								vernBlockInVerseChunk.CharacterIs(bookId, CharacterVerseData.StandardCharacter.Narrator) &&
								BlockIsOmissibleReportingClause(refBlockInVerseChunk, out var nextModifiedOmittedHeSaidText) &&
								TryMatchToReportingClause(bookNum, vernBlockList, indexOfVernVerseStart + i, reportingClauses, omittedHeSaids, vernacularVersification))
							{
								omittedHeSaids.Add(new Tuple<int, string>(currRefBlockIndex, nextModifiedOmittedHeSaidText));
							}
							else
								vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);

							handled = true;
						}
						else if (numberOfVernBlocksInVerseChunk == 1 && numberOfRefBlocksInVerseChunk == 1 &&
							BlocksEndWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
						{
							Debug.Assert(i == 0);
							if (allowSplitting && TryMatchBySplittingRefBlock(vernBlockInVerseChunk, refBook, indexOfRefVerseStart, vernacularVersification))
								iRefBlock++;
							else
							{
								if (forceMatch)
									vernBlockList[iVernBlock].SetMatchedReferenceBlock(refBlockInVerseChunk);
								else
									vernBlockList[iVernBlock].SetUnmatchedReferenceBlocks(new[] {refBlockInVerseChunk});
							}

							handled = true;
						}
						else if (lookForReportingClauses &&
							vernBlockInVerseChunk.IsQuote &&
							refBlockInVerseChunk.CharacterIs(bookId, CharacterVerseData.StandardCharacter.Narrator) &&
							BlockIsOmissibleReportingClause(refBlockInVerseChunk, out var nextModifiedOmittedHeSaidText))
						{
							// This is a special edge case because the above test for "BlocksMatch" fails when the vern block
							// is "Needs Review", but if all other conditions for a reporting clause match are satisfied, and
							// we have a pending omitted he said, we want to match to it.
							if (!omittedHeSaids.Any() ||
								vernBlockInVerseChunk.CharacterId != CharacterVerseData.kNeedsReview ||
								!BlocksStartWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification) ||
								!BlocksEndWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification) ||
								!TryMatchToReportingClause(bookNum, vernBlockList, indexOfVernVerseStart + i, reportingClauses, omittedHeSaids, vernacularVersification))
							{
								if (refBlockInVerseChunk.StartsAtVerseStart)
									verseFromOmittedHeSaidBlockToPrepend = refBlockInVerseChunk.BlockElements.OfType<Verse>().Single().Number;
								i--;
							}

							omittedHeSaids.Add(new Tuple<int, string>(currRefBlockIndex, nextModifiedOmittedHeSaidText));
							handled = true;
						}
						// Consider case where the vernacular uses a verse bridge for verses that have
						// a quote, but would otherwise align neatly with the reference text. (This
						// conditional is hard to read and not terribly efficient, but it will short-
						// circuit quickly in most cases.)
						// Note: I originally wrote this logic to use BlocksHaveCompatibleCharacters,
						// but I was concerned about the rare case where the reference text had two
						// different speakers, but the vernacular only had one (the other being
						// rendered as indirect speech). This could cause a "match" where the lines
						// from the two different speakers are combined.
						else if (numberOfVernBlocksInVerseChunk == 2 &&
							vernBlockInVerseChunk.IsSimpleBridge &&
							BlocksStartWithSameVerse(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification) &&
							vernBlockInVerseChunk.CharacterId == refBlockInVerseChunk.CharacterId &&
							BlocksEndWithSameVerse(bookNum, vernBlockList[indexOfVernVerseStart + 1],
								refBlockList[indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1], vernacularVersification) &&
							vernBlockList[indexOfVernVerseStart + 1].CharacterId != vernBlockInVerseChunk.CharacterId && // One of the blocks needs to be a different character
							vernBlockList[indexOfVernVerseStart + 1].CharacterId ==
							refBlockList[indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1].CharacterId &&
							refBlockList.Skip(indexOfRefVerseStart + 1).Take(numberOfRefBlocksInVerseChunk - 2).All(b =>
								vernBlockInVerseChunk.CharacterId == b.CharacterId ||
								vernBlockList[indexOfVernVerseStart + 1].CharacterId == b.CharacterId))
						{
							// Note that this logic introduces the faint possibility of changing the order of
							// reference text blocks to achieve a match. At the time of this writing, there are
							// no tests for this case. It's probably very unlikely, but if it were ever to occur,
							// it would likely be desirable.
							vernBlockInVerseChunk.SetMatchedReferenceBlock(bookNum, vernacularVersification, this,
								refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerseChunk)
									.Where(b => vernBlockInVerseChunk.CharacterId == b.CharacterId).ToList());
							var otherVernBlockInVerseChunk = vernBlockList[indexOfVernVerseStart + 1];
							otherVernBlockInVerseChunk.SetMatchedReferenceBlock(bookNum, vernacularVersification, this,
								refBlockList.Skip(indexOfRefVerseStart).Take(numberOfRefBlocksInVerseChunk)
									.Where(b => otherVernBlockInVerseChunk.CharacterId == b.CharacterId).ToList());
							i++;
							iRefBlock = indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - 1;
							handled = true;
						}
					}
					if (!handled)
					{
						if (lookForReportingClauses && omittedHeSaids.Any() && i == 0)
						{
							// We made no progress in the top-down pass. But since we found a
							// possible he-said with no matching vernacular, this verse may have reporting
							// clauses that have not yet been identified. Let's try to match without
							// considering he-saids. We won't get a perfect, well-ordered match, but at
							// least we won't have reference text lines go AWOL.
							omittedHeSaids.Clear();
							i--; // it's going to get re-incremented in the for loop.
							lookForReportingClauses = false;
							continue;
						}
						iVernBlock = indexOfVernVerseStart + i;
						if (vernBlockList[iVernBlock].CharacterIs(bookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
							iVernBlock++;

						var j = 0;
						var iLastVernBlockMatchedFromBottomUp = -1;
						var numberOfUnexpectedReportingClausesMatched = 0;
						if (numberOfVernBlocksInVerseChunk - i + omittedHeSaids.Count >= 2)
						{
							// Look from the bottom up
							for (; j < numberOfVernBlocksInVerseChunk && j + i < numberOfRefBlocksInVerseChunk + omittedHeSaids.Count + numberOfUnexpectedReportingClausesMatched; j++)
							{
								var iCurrVernBottomUp = indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - j - 1;
								vernBlockInVerseChunk = vernBlockList[iCurrVernBottomUp];
								if (vernBlockInVerseChunk.MatchesReferenceText)
									break;

								var currBottomUpRefBlockIndex = indexOfRefVerseStart + numberOfRefBlocksInVerseChunk - j
									- 1 + numberOfUnexpectedReportingClausesMatched;
								var refBlockInVerseChunk = refBlockList[currBottomUpRefBlockIndex];
								if ((iCurrVernBottomUp >= i || omittedHeSaids.Count == 0) && // PG-1408: Don't match two different vern blocks to the same ref block
									BlocksMatch(bookNum, vernBlockInVerseChunk, refBlockInVerseChunk, vernacularVersification))
								{
									vernBlockInVerseChunk.SetMatchedReferenceBlock(refBlockInVerseChunk);
									iLastVernBlockMatchedFromBottomUp = iCurrVernBottomUp;
								}
								else if (reportingClauses != null &&
									TryMatchToReportingClause(bookNum, vernBlockList, iCurrVernBottomUp, reportingClauses, omittedHeSaids, vernacularVersification))
								{
									numberOfUnexpectedReportingClausesMatched++;
									iLastVernBlockMatchedFromBottomUp = iCurrVernBottomUp;
								}
								else
								{
									if (omittedHeSaids.Any())
									{
										var iLastOmittedHeSaid = omittedHeSaids.Count - 1;

										if (omittedHeSaids[iLastOmittedHeSaid].Item1 == currBottomUpRefBlockIndex && iCurrVernBottomUp == 0)
										{
											// Special case. We found our way all the way to the top, so we can just use the
											// existing ref block, rather than remaking it from the modified one.
											vernBlockInVerseChunk.SetUnmatchedReferenceBlocks(
												new[] {refBlockList[currBottomUpRefBlockIndex]});
											verseFromOmittedHeSaidBlockToPrepend = null; // Just to be safe
										}
										else
										{
											// Since the modified "he said" text in omittedHeSaids is always the English
											// one, the order in which they get hooked up depends on whether or not this
											// (the primary reference text) is English or not.
											Block englishRefBlock;
											if (HasSecondaryReferenceText)
											{
												var clonedRefBlock = refBlockList[omittedHeSaids[iLastOmittedHeSaid].Item1]
													.Clone(Block.ReferenceBlockCloningBehavior.SetToNewEmptyList);
												clonedRefBlock.RemoveVerseNumbers(vernBlockList.Skip(indexOfVernVerseStart)
													.Take(i).SelectMany(vb => vb.BlockElements).OfType<Verse>());
												vernBlockInVerseChunk.SetUnmatchedReferenceBlocks(
													new[] {clonedRefBlock});
												englishRefBlock = clonedRefBlock.SetMatchedReferenceBlock(omittedHeSaids[iLastOmittedHeSaid].Item2);
											}
											else
											{
												englishRefBlock = vernBlockInVerseChunk.SetUnmatchedNarratorReferenceBlock(
													omittedHeSaids[iLastOmittedHeSaid].Item2, bookId);
											}
									
											if (verseFromOmittedHeSaidBlockToPrepend != null && vernBlockInVerseChunk.StartsAtVerseStart)
											{
												englishRefBlock.BlockElements.Insert(0, new Verse(verseFromOmittedHeSaidBlockToPrepend));
												verseFromOmittedHeSaidBlockToPrepend = null;
											}
										}
										omittedHeSaids.RemoveAt(iLastOmittedHeSaid);
									}
									else
									{
										j -= numberOfUnexpectedReportingClausesMatched;
										break;
									}
								}
							}
						}
						var numberOfUnmatchedRefBlocks = numberOfRefBlocksInVerseChunk - i - j /* + numberOfUnexpectedReportingClausesMatched */;
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
								if (j + i == numberOfVernBlocksInVerseChunk - 1)
								{
									TryMatchToReportingClause(bookNum, vernBlockList, iVernBlock, reportingClauses,
										omittedHeSaids, vernacularVersification);
								}
								// else do nothing (PG-1085)
							}
							else if (forceMatch)
							{
								// If allowSplitting is true, the caller was responsible for obtaining the lock on m_modifiedBooks,
								// so we don't need to incur that overhead again here.
								CombineRefBlocksToCreateMatch(remainingRefBlocksList, vernBlockList[iVernBlock], allowSplitting && m_modifiedBooks.Contains(bookId));
							}
							else
							{
								if (remainingRefBlocksList.Count == 1)
								{
									if (vernBlockList[iVernBlock].MatchesReferenceText &&
										vernBlockList[iVernBlock].ReferenceBlocks.Single().CharacterId != remainingRefBlocksList[0].CharacterId)
									{
										// See if the immediately following or preceding block is a better match
										var otherIndicesToTry = (i + indexOfVernVerseStart <= iVernBlock) ?
											new[] {iVernBlock - 1, iVernBlock + 1} :
											new[] {iVernBlock + 1, iVernBlock - 1};
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
												{
													vernBlockList[iPreOrPost].InsertUnmatchedReferenceBlocks(0, remainingRefBlocksList);
													if (vernBlockList[iVernBlock].StartsAtVerseStart &&
														!vernBlockList[iVernBlock].ReferenceBlocks.Any(b => b.ContainsVerseNumber))
													{
														// See if we can shift a verse number from one of the following reference blocks
														foreach (var subsequentRefBlock in vernBlockList[iPreOrPost].ReferenceBlocks)
														{
															if (subsequentRefBlock.StartsAtVerseStart)
															{
																var refVerse = subsequentRefBlock.BlockElements.First(e => e is Verse);
																var refBlockToStartWithVerse = vernBlockList[iVernBlock].ReferenceBlocks.First();
																refBlockToStartWithVerse.BlockElements.Insert(0, refVerse);
																subsequentRefBlock.BlockElements.Remove(refVerse);
																if (HasSecondaryReferenceText && refBlockToStartWithVerse.MatchesReferenceText)
																{
																	if (subsequentRefBlock.MatchesReferenceText)
																	{
																		var subsequentEnglishRefBlock = subsequentRefBlock.ReferenceBlocks[0];
																		refVerse = subsequentEnglishRefBlock.BlockElements.First(e => e is Verse);
																		refBlockToStartWithVerse = refBlockToStartWithVerse.ReferenceBlocks[0];
																		refBlockToStartWithVerse.BlockElements.Insert(0, refVerse);
																		subsequentEnglishRefBlock.BlockElements.Remove(refVerse);
																	}
																}
																m_modifiedBooks.Add(bookId);
																break;
															}

															if (subsequentRefBlock.ContainsVerseNumber)
																break;
														}
													}
												}

												remainingRefBlocksList = null;
												break;
											}
										}
									}
									else
									{
										var betterMatch = vernBlockList.Skip(iVernBlock).Take(numberOfVernBlocksInVerseChunk - j)
											.OnlyOrDefault(b => !b.MatchesReferenceText &&
												BlocksMatch(bookNum, b, remainingRefBlocksList[0], vernacularVersification));
										if (betterMatch != null)
										{
											betterMatch.SetMatchedReferenceBlock(remainingRefBlocksList[0]);
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
											TryMatchBySplittingRefBlock(vernBlockList.Skip(iVernBlock)
												.Take(numberOfVernBlocksInVerseChunk)
												.TakeWhile(b => !b.ReferenceBlocks.Any())
												.ToList(),
												refBook, indexOfRefVerseStart + i, vernacularVersification))
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

					if (verseFromOmittedHeSaidBlockToPrepend != null && vernBlockInVerseChunk.ReferenceBlocks.Any())
					{
						var refBlock = vernBlockInVerseChunk.ReferenceBlocks.First();
						if (!refBlock.StartsAtVerseStart)
						{
							refBlock.BlockElements.Insert(0, new Verse(verseFromOmittedHeSaidBlockToPrepend));
							if (HasSecondaryReferenceText && refBlock.MatchesReferenceText)
							{
								var englishRefBlock = refBlock.ReferenceBlocks[0];
								if (!englishRefBlock.StartsAtVerseStart)
									englishRefBlock.BlockElements.Insert(0, new Verse(verseFromOmittedHeSaidBlockToPrepend));
							}

							verseFromOmittedHeSaidBlockToPrepend = null;
						}
					}
				}
				var indexOfLastVernVerseInVerseChunk = indexOfVernVerseStart + numberOfVernBlocksInVerseChunk - 1;
				if (vernBlockList[indexOfLastVernVerseInVerseChunk].ReferenceBlocks.Any())
					iVernBlock = indexOfLastVernVerseInVerseChunk;
				else if (numberOfVernBlocksInVerseChunk == numberOfRefBlocksInVerseChunk + 1)
				{
					if (vernBlockList[indexOfLastVernVerseInVerseChunk].TryMatchToReportingClause(reportingClauses, this, bookNum, vernacularVersification))
						iVernBlock = indexOfLastVernVerseInVerseChunk;
				}
			}
		}

		private bool TryMatchToReportingClause(int bookNum, IReadOnlyList<Block> vernBlockList, int iVernBlock,
			IReadOnlyCollection<string> reportingClauses, List<Tuple<int, string>> omittedHeSaids, ScrVers vernacularVersification)
		{
			if (reportingClauses == null || !vernBlockList[iVernBlock].TryMatchToReportingClause(reportingClauses, this, bookNum, vernacularVersification))
				return false;

			var omittedEnglishHeSaidText = omittedHeSaids.LastOrDefault()?.Item2;
			if (omittedEnglishHeSaidText != null)
			{
				var englishRefBlock = vernBlockList[iVernBlock].ReferenceBlocks.Single();
				if (HasSecondaryReferenceText)
				{
					englishRefBlock = englishRefBlock.ReferenceBlocks.Single();
				}
				englishRefBlock.BlockElements.OfType<ScriptText>().Single().Content = omittedEnglishHeSaidText;
				omittedHeSaids.RemoveAt(omittedHeSaids.Count - 1);
			}

			// At least to date, reference texts do not have verse bridges. If the vernacular were to have a verse bridge
			// we at least want to find and remove the corresponding verse number at the start of the reference text of
			// the subsequent block. This could still leave a spurious duplicate verse number later in that reference block
			// but this should be quite rare.
			if (vernBlockList[iVernBlock].ReferenceBlocks.Single().BlockElements.First() is Verse v &&
				vernBlockList.Count > iVernBlock + 1 &&
				(vernBlockList[iVernBlock + 1].ReferenceBlocks.OnlyOrDefault()?.BlockElements.FirstOrDefault() as Verse)?.StartVerse == v.StartVerse)
			{
				var clone = vernBlockList[iVernBlock + 1].ReferenceBlocks.Single().Clone(Block.ReferenceBlockCloningBehavior.CloneListAndAllReferenceBlocks);
				clone.BlockElements.RemoveAt(0);
				vernBlockList[iVernBlock + 1].SetMatchedReferenceBlock(clone);
				if (HasSecondaryReferenceText)
				{
					// At least to date, all reference texts use the same (English) versification and more-or-less have verse numbers
					// align. This might not be perfect (in that it could occasionally allow a duplicate verse number to appear in the
					// reference text where the numbers do not exactly align), but we can safely assume that if there is a verse
					// number in the secondary reference text, it is the corresponding one and can be removed.
					if (vernBlockList[iVernBlock + 1].ReferenceBlocks[0].ReferenceBlocks[0].BlockElements.FirstOrDefault() is Verse)
					{
						// No need to create a clone here. The above cloning should have taken care of that already.
						vernBlockList[iVernBlock + 1].ReferenceBlocks[0].ReferenceBlocks[0].BlockElements.RemoveAt(0);
					}
				}
			}
			return true;
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
		/// In cases where a ref block contains multiple verses but the vernacular is missing the
		/// verse(s) with which it starts, we don't want to lose the part of the reference text
		/// that does correspond to the omitted verses, so it may be necessary to split the
		/// reference text block so the vernacular block can be matched just to the portion that
		/// makes sense. Because this changes the reference block, we need to note that the
		/// reference text book is "dirty", so this method should only be called in the context
		/// of a method that has locked m_modifiedBooks; otherwise, the local call here to lock
		/// it could result in deadlock.
		/// </summary>
		/// <returns>Whether or not the ref block was able to be split up at a verse break so as to
		/// get a block that could be matched to the vernacular block.</returns>
		private bool TryMatchBySplittingRefBlock(Block vernBlock, PortionScript refBook,
			int iRefBlock, ScrVers vernacularVersification)
		{
			lock (m_modifiedBooks) // See comment above
			{
				if (vernBlock.BlockElements[0] is Verse)
				{
					var vernStartRef = vernBlock.StartRef(refBook.BookNumber, vernacularVersification);
					vernStartRef.ChangeVersification(refBook.Versification);
					var refBlockList = refBook.GetScriptBlocks();
					Block refBlock = refBlockList[iRefBlock];
					var vernStartVerse = vernStartRef.VerseNum;
					if (vernStartRef.ChapterNum == refBlock.ChapterNumber &&
						refBlock.BlockElements.Skip(1).OfType<Verse>().Any(v => v.StartVerse == vernStartVerse) &&
						refBook.TrySplitBlockAtEndOfVerse(refBlock, vernStartVerse - 1))
					{
						m_modifiedBooks.Add(refBook.BookId);
						var newBlock = refBlockList[iRefBlock + 1];
						Debug.Assert(newBlock.StartsAtVerseStart && newBlock.InitialStartVerseNumber == vernStartVerse);
						vernBlock.SetMatchedReferenceBlock(newBlock);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// In cases where a ref block contains multiple verses but the vernacular is missing the
		/// verse(s) with which it starts, we don't want to lose the part of the reference text
		/// that does correspond to the omitted verses, so it may be necessary to split the
		/// reference text block so the vernacular blocks can be matched just to the portion that
		/// makes sense. Because this changes the reference block, we need to note that the
		/// reference text book is "dirty", so this method should only be called in the context
		/// of a method that has locked m_modifiedBooks; otherwise, the local call here to lock
		/// it could result in deadlock.
		/// </summary>
		/// <returns>Whether or not the ref block was able to be split up at verse breaks so as to
		/// get blocks that could be matched to the vernacular blocks.</returns>
		private bool TryMatchBySplittingRefBlock(List<Block> vernBlocks, PortionScript refBook,
			int iRefBlock, ScrVers vernacularVersification)
		{
			if (!vernBlocks.Any())
				return false;
			var firstVernBlock = vernBlocks.First();
			if (vernBlocks.Count == 1)
			{
				return TryMatchBySplittingRefBlock(firstVernBlock, refBook, iRefBlock, vernacularVersification);
			}
			lock (m_modifiedBooks) // See comment above
			{
				var refBlockList = refBook.GetScriptBlocks();
				Block refBlock = refBlockList[iRefBlock];

				if (!firstVernBlock.StartsAtVerseStart && !refBlock.StartsAtVerseStart)
				{
					var vernStartRef = firstVernBlock.StartRef(refBook.BookNumber, vernacularVersification);
					vernStartRef.ChangeVersification(refBook.Versification);

					var vernEndRef = vernBlocks.Last().EndRef(refBook.BookNumber, vernacularVersification);
					vernEndRef.ChangeVersification(refBook.Versification);

					if (vernStartRef.VerseNum == refBlock.InitialStartVerseNumber &&
						vernEndRef.VerseNum == refBlock.LastVerseNum)
					{
						int vernSplitVerse = 0;
						var didSplit = false;
						// For each vern block that starts with a verse num, break the ref block at that
						// same verse num (after converting versification)
						for (var i = 0; i < vernBlocks.Count; i++)
						{
							var vernBlock = vernBlocks[i];
							if (!vernBlock.StartsAtVerseStart)
							{
								vernEndRef = vernBlock.EndRef(refBook.BookNumber, vernacularVersification);
								vernEndRef.ChangeVersification(refBook.Versification);
								vernSplitVerse = vernEndRef.VerseNum;
								continue;
							}

							Debug.Assert(vernSplitVerse > 0);

							vernStartRef = vernBlock.StartRef(refBook.BookNumber, vernacularVersification);
							vernStartRef.ChangeVersification(refBook.Versification);
							var vernStartVerse = vernStartRef.VerseNum;
							
							if (vernStartRef.ChapterNum == refBlock.ChapterNumber &&
								refBlock.BlockElements.Skip(1).OfType<Verse>().Any(v => v.StartVerse == vernStartVerse) &&
								refBook.TrySplitBlockAtEndOfVerse(refBlock, vernSplitVerse))
							{
								m_modifiedBooks.Add(refBook.BookId);
								var newBlock = refBlockList[iRefBlock + 1];
								Debug.Assert(newBlock.StartsAtVerseStart && newBlock.InitialStartVerseNumber == vernStartVerse);
								vernBlocks[i - 1].SetMatchedReferenceBlock(refBlockList[iRefBlock]);
								vernBlock.SetMatchedReferenceBlock(newBlock);
								didSplit = true;
							}
							else
							{
								return false;
							}
						}

						return didSplit;
					}
				}
			}
			return false;
		}

		private bool BlocksMatch(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification) =>
			BlocksStartWithSameVerse(bookNum, vernBlock, refBlock, vernacularVersification) &&
			BlocksHaveCompatibleCharacters(vernBlock, refBlock) &&
			BlocksEndWithSameVerse(bookNum, vernBlock, refBlock, vernacularVersification);

		private bool BlocksStartWithSameVerse(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification) =>
			vernBlock.StartRef(bookNum, vernacularVersification).CompareTo(refBlock.StartRef(bookNum, Versification)) == 0;

		// ENHANCE: In passages where there is a narrator override, narrator should be considered a "match" with the override character
		private bool BlocksHaveCompatibleCharacters(Block vernBlock, Block refBlock) =>
			vernBlock.CharacterId == refBlock.CharacterId || (vernBlock.CharacterIsUnclear && !refBlock.CharacterIsStandard);

		private bool BlocksEndWithSameVerse(int bookNum, Block vernBlock, Block refBlock, ScrVers vernacularVersification) =>
			vernBlock.EndRef(bookNum, vernacularVersification).CompareTo(refBlock.EndRef(bookNum, Versification)) == 0;

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
					splitLocations.Add(new VerseSplitLocation(bookNum, prevBlock, block, script.Versification));
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

			// Since blocks cannot cross chapter breaks, if a split location is at the start of a chapter, typically
			// both texts will already have a break there. However, if there is a versification difference, the chapter
			// break location in one may not be the chapter break location in the other. But since verse 0 does not
			// usually map to anything, changing the versification does not push it back across the chapter divide.
			// In that case, we FIRST need to back up one verse, THEN change the versification so that if the text
			// being broken has these two verses combined in a single block, we'll detect this as a valid break location.
			// NOTE: We used to do all the comparisons without changing the versification and only changed the
			// versification when we were actually going to make the split, but there didn't seem to be be any reason for
			// this since the comparisons require the same logic that is used to actually make the versification change.
			// So changing it up front should generally prove more efficient.
			// At first glance, it might seem this logic could cause problems in Psalms, where there are places that map
			// verse 0 to verse 1, but since these never correspond to places where two Psalms are combined into one
			// (which obviously wouldn't make sense to do if there were a Hebrew subtitle present), it works out just
			// fine. If we go back to the previous verse (the last verse in the previous Psalm) and then convert to the
			// other versification, the conversion does not alter the reference. One of the more interesting places is
			// at the start of Psalm 11 (in English & Original == Psalm 10 in Russian). Here most versifications put the
			// Hebrew subtitle together with the text of what is verse 1 in English. But since mappings can't deal with
			// partial verses, there is no mapping for this chapter in English. If any versification were to combine
			// this Psalm with the previous one and a block were to have the last verse of that Psalm and the first verse
			// of this Psalm, it would make for a more interesting case.
			VerseRef GetAdjustedVerseToSplitAfter(int i)
			{
				VerseRef loc = verseSplitLocations[i];
				if (loc.Versification != blocksToSplit.Versification)
				{
					if (loc.VerseNum == 0)
						loc.PreviousVerse();
					loc.ChangeVersification(blocksToSplit.Versification);
				}

				return loc;
			}

			bool splitsMade = false;
			var iSplit = 0;
			VerseRef verseToSplitAfter = GetAdjustedVerseToSplitAfter(iSplit);
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
					verseToSplitAfter = GetAdjustedVerseToSplitAfter(++iSplit);
				}

				var lastVerse = block.EndRef(bookNum, blocksToSplit.Versification);
				if (lastVerse < verseToSplitAfter)
					continue;

				if (initEndVerse.CompareTo(lastVerse) != 0 && lastVerse >= verseSplitLocations[iSplit].Before)
				{
					bool invalidSplitLocation = false;
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
						verseToSplitAfter = GetAdjustedVerseToSplitAfter(++iSplit);
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
	}
}
