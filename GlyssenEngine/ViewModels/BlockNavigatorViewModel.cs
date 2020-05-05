using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using SIL;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;

namespace GlyssenEngine.ViewModels
{
	public class BlockNavigatorViewModel<TFont> : IDisposable
	{
		protected readonly Project m_project;
		public const string kDataCharacter = "data-character";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body {1}>{2}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		public const string kCssClassContext = "context";
		private const string kCssFrame = Block.kCssFrame +
										".highlight{{background-color:highlight;color:highlighttext}}" +
										"." + kCssClassContext + ":hover{{background-color:#E1F0FF}}" +
										".block-spacer{{height:30px}}" +
										".section-header{{text-align:center;font-weight:bold}}" +
										".chapter-label{{font-weight:bold;font-size:150%}}";
		public const string kMainQuoteElementId = "main-quote-text";

		private bool m_showVerseNumbers = true; // May make this configurable later
		private IAdjustableFontInfo<TFont> m_font;
		public Func<ReferenceText, IAdjustableFontInfo<TFont>> GetAdjustableFontInfoForReferenceText { get; }
		private readonly Dictionary<ReferenceText, IAdjustableFontInfo<TFont>> m_referenceTextFonts = new Dictionary<ReferenceText, IAdjustableFontInfo<TFont>>();
		private BlockNavigator m_navigator;
		private readonly IEnumerable<string> m_includedBooks;
		private List<BookBlockIndices> m_relevantBookBlockIndices;
		protected BookBlockIndices m_temporarilyIncludedBookBlockIndices;
		private static readonly BookBlockTupleComparer s_bookBlockComparer = new BookBlockTupleComparer();
		protected readonly IEqualityComparer<CharacterSpeakingMode> m_characterEqualityComparer = new CharacterEqualityComparer();
		private int m_currentRelevantIndex = -1;
		private BlocksToDisplay m_mode;
		private BlockMatchup m_currentRefBlockMatchups;
		private bool m_attemptRefBlockMatchup;

		public event EventHandler UiFontSizeChanged;
		public event EventHandler CurrentBlockChanged;
		public event EventHandler CurrentBlockMatchupChanged;
		public event EventHandler FilterReset;

		protected BookScript CurrentBook => BlockAccessor.CurrentBook;

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode = BlocksToDisplay.AllScripture, IAdjustableFontInfo<TFont> fontInfo = null,
			Func<ReferenceText, IAdjustableFontInfo<TFont>> getAdjustableFontInfoForReferenceText = null)
			: this(project, mode, null, fontInfo, getAdjustableFontInfoForReferenceText)
		{
		}

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode, BookBlockIndices startingIndices, IAdjustableFontInfo<TFont> fontInfo,
			Func<ReferenceText, IAdjustableFontInfo<TFont>> getAdjustableFontInfoForReferenceText)
		{
			GetAdjustableFontInfoForReferenceText = getAdjustableFontInfoForReferenceText;
			m_project = project;
			m_project.QuoteParseCompleted += HandleProjectQuoteParseCompleted;

			m_navigator = new BlockNavigator(m_project.IncludedBooks);

			m_includedBooks = project.IncludedBookIds;
			Versification = project.Versification;

			m_font = fontInfo;
			CacheReferenceTextFonts(project.ReferenceText);

			FontSizeUiAdjustment = project.FontSizeUiAdjustment;

			SetModeInternal(mode);

			if (startingIndices != null && !startingIndices.IsUndefined && startingIndices.BookIndex < m_project.IncludedBooks.Count)
			{
				var startingBlock = GetBlock(startingIndices);
				if (startingBlock != null && !CharacterVerseData.IsCharacterExtraBiblical(startingBlock.CharacterId))
				{
					SetBlock(startingIndices);
					m_currentRelevantIndex = m_relevantBookBlockIndices.IndexOf(startingIndices);
					if (m_currentRelevantIndex < 0)
						m_temporarilyIncludedBookBlockIndices = startingIndices;
				}
			}
		}

		private void HandleProjectQuoteParseCompleted(object sender, EventArgs e)
		{
			m_navigator = new BlockNavigator(m_project.IncludedBooks);
			ResetFilter(null);
		}

		private void CacheReferenceTextFonts(ReferenceText referenceText)
		{
			if (GetAdjustableFontInfoForReferenceText == null)
				return;
			m_referenceTextFonts[referenceText] = GetAdjustableFontInfoForReferenceText(referenceText);

			if (referenceText.HasSecondaryReferenceText)
				CacheReferenceTextFonts(referenceText.SecondaryReferenceText);
		}

		public BlockNavigatorViewModel(IReadOnlyList<BookScript> books, ScrVers versification)
		{
			m_navigator = new BlockNavigator(books);

			m_includedBooks = books.Select(b => b.BookId);
			Versification = versification;

			SetModeInternal(BlocksToDisplay.AllScripture);
		}

		internal BlockNavigator BlockNavigator
		{
			set
			{
				m_navigator = value;
				ResetFilter(null);
			}
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (m_project != null)
			{
				if (m_font != null)
					m_project.FontSizeUiAdjustment = FontSizeUiAdjustment;
				m_project.QuoteParseCompleted -= HandleProjectQuoteParseCompleted;
			}

			if (m_font != null)
			{
				(m_font as IDisposable)?.Dispose();
				m_font = null;
			}

			foreach (var fontProxy in m_referenceTextFonts.Values.OfType<IDisposable>())
				fontProxy.Dispose();
			m_referenceTextFonts.Clear();
		}
		#endregion

		#region Public properties
		public ScrVers Versification { get; }

		public int BlockCountForCurrentBook => BlockAccessor.CurrentBook.GetScriptBlocks().Count + CountOfBlocksAddedByCurrentMatchup;

		private int CountOfBlocksAddedByCurrentMatchup => BlockGroupingStyle == BlockGroupingType.BlockCorrelation ? m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting : 0;

		public int RelevantBlockCount => m_relevantBookBlockIndices.Count;

		protected IEnumerable<BookBlockIndices> IndicesAtOrBeyondLocationInBook(BookBlockIndices current)
		{
			return m_relevantBookBlockIndices.Where(r => r.BookIndex == current.BookIndex &&
				r.BlockIndex >= current.BlockIndex);
		}

		/// <summary>
		/// This shows the current position within the filtered list. In rainbow mode, we count "matchups". In white mode, we count blocks.
		/// </summary>
		public int CurrentDisplayIndex => m_currentRelevantIndex + 1;

		public IBlockAccessor BlockAccessor => m_navigator;

		public string CurrentBookId => BlockAccessor.CurrentBook.BookId;
		public int CurrentBookNumber => BlockAccessor.CurrentBook.BookNumber;
		public bool CurrentBookIsSingleVoice => BlockAccessor.CurrentBook.SingleVoice;

		public Block CurrentBlock => BlockGroupingStyle == BlockGroupingType.BlockCorrelation ? m_currentRefBlockMatchups.CorrelatedAnchorBlock : BlockAccessor.CurrentBlock;
		public Block CurrentEndBlock => BlockAccessor.CurrentEndBlock;
		protected Block CurrentBlockInOriginal => BlockAccessor.CurrentBlock;
		public BlockMatchup CurrentReferenceTextMatchup => m_currentRefBlockMatchups;
		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }
		public string ProjectName => m_project.Name;
		public BlockGroupingType BlockGroupingStyle => m_currentRefBlockMatchups == null ? BlockGroupingType.Quote : BlockGroupingType.BlockCorrelation;
		public bool AttemptRefBlockMatchup => m_attemptRefBlockMatchup;

		public bool CanDisplayReferenceTextForCurrentBlock => m_project.ReferenceText.CanDisplayReferenceTextForBook(CurrentBook) && !CurrentBook.SingleVoice;

		public bool IsCurrentLocationRelevant =>  m_relevantBookBlockIndices.Any(i => i.Contains(BlockAccessor.GetIndices()));

		public IEnumerable<string> IncludedBooks => m_includedBooks;
		public TFont Font => m_font.Font;
		public IFontInfo<TFont> FontInfo => m_font;
		public bool RightToLeftScript => m_font.RightToLeftScript;
		public IFontInfo<TFont> PrimaryReferenceTextFont => m_referenceTextFonts[m_project.ReferenceText];
		public IFontInfo<TFont> EnglishReferenceTextFont
		{
			get
			{
				if (m_project.ReferenceText.HasSecondaryReferenceText)
					return m_referenceTextFonts[m_project.ReferenceText.SecondaryReferenceText];
				return m_referenceTextFonts[m_project.ReferenceText];
			}
		}
		public int FontSizeUiAdjustment
		{
			get => m_font.FontSizeUiAdjustment;
			set
			{
				if (m_font != null)
					m_font.FontSizeUiAdjustment = value;
				foreach (var fontProxy in m_referenceTextFonts.Values)
					fontProxy.FontSizeUiAdjustment = value;

				UiFontSizeChanged?.Invoke(this, new EventArgs());
			}
		}

		public int CurrentBlockIndexInBook
		{
			get => BlockAccessor.GetIndices().BlockIndex + (m_currentRefBlockMatchups?.CorrelatedBlocks.IndexOf(m_currentRefBlockMatchups.CorrelatedAnchorBlock) ?? 0);
			set
			{
				int index = value;
				var bookIndex = BlockAccessor.GetIndices().BookIndex;

				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation)
				{
					if (index >= m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count)
					{
						// Adjust index to account for any temporary additions resulting from splitting blocks in the matchup.
						index -= m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting;
					}
					else if (index >= m_currentRefBlockMatchups.IndexOfStartBlockInBook)
					{
						// A block within the existing matchup has been selected, so we need to translate the index to find the
						// correct block within the sequence of correlated blocks rather than within the book.
						index -= m_currentRefBlockMatchups.IndexOfStartBlockInBook;
						var newAnchorBlock = m_currentRefBlockMatchups.CorrelatedBlocks[index];
						while (newAnchorBlock.CharacterIs(CurrentBookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
						{
							index++; // A section head should NEVER be the last one in the matchup!
							newAnchorBlock = m_currentRefBlockMatchups.CorrelatedBlocks[index];
						}
						// Just reset the anchor (if needed) and get out.
						if (newAnchorBlock != m_currentRefBlockMatchups.CorrelatedAnchorBlock)
						{
							m_currentRefBlockMatchups.ChangeAnchor(newAnchorBlock);
							HandleCurrentBlockChanged();
						}
						return;
					}
				}
				else
					FindStartOfQuote(ref index);

				var location = new BookBlockIndices(bookIndex, index);
				m_currentRelevantIndex = m_relevantBookBlockIndices.IndexOf(location);
				m_temporarilyIncludedBookBlockIndices = m_currentRelevantIndex < 0 ? location : null;
				SetBlock(location);
			}
		}

		public Block FindStartOfQuote(ref int index)
		{
			Block b;
			do
			{
				b = CurrentBook.GetScriptBlocks()[index];
			} while (b.IsContinuationOfPreviousBlockQuote && --index >= 0);
			Debug.Assert(index >= 0);
			return b;
		}

		public void SetMode(BlocksToDisplay mode, bool attemptBlockMatchup = false)
		{
			if (m_mode == mode && m_attemptRefBlockMatchup == attemptBlockMatchup)
				return;

			if (!attemptBlockMatchup && (mode & BlocksToDisplay.NotAlignedToReferenceText) > 0)
				throw new InvalidOperationException("The \"Not aligned to reference text\" filter requires matching up with the reference text.");

			m_attemptRefBlockMatchup = attemptBlockMatchup;
			SetModeInternal(mode);
		}

		public virtual BlocksToDisplay Mode
		{
			get => m_mode;
			protected set => m_mode = value;
		}

		private void SetModeInternal(BlocksToDisplay mode, bool stayOnCurrentBlock = false)
		{
			Mode = mode;
			if ((Mode & BlocksToDisplay.NotAlignedToReferenceText) > 0)
				m_attemptRefBlockMatchup = true;
			m_temporarilyIncludedBookBlockIndices = GetCurrentBlockIndices();
			ResetFilter(BlockAccessor.CurrentBlock, stayOnCurrentBlock);

			Logger.WriteEvent($"Block navigator mode set to {Mode}. Relevant blocks = {RelevantBlockCount}");
		}

		protected void ResetFilter(Block selectedBlock, bool stayOnCurrentBlock = false)
		{
			PopulateRelevantBlocks();

			if (RelevantBlockCount > 0)
			{
				m_currentRelevantIndex = -1;
				if (m_temporarilyIncludedBookBlockIndices != null)
				{
					// Block that was temporarily included in previous filter might now match the new filter
					if (SetAsCurrentLocationIfRelevant(m_temporarilyIncludedBookBlockIndices))
						return;
					if (m_temporarilyIncludedBookBlockIndices.IsMultiBlock)
					{
						// Even though this group of blocks is not "relevant", it may well be that one of the blocks it contains is.
						var indices = new BookBlockIndices(m_temporarilyIncludedBookBlockIndices.BookIndex, m_temporarilyIncludedBookBlockIndices.BlockIndex);
						for (var iBlock = 0; iBlock < m_temporarilyIncludedBookBlockIndices.MultiBlockCount; iBlock++)
						{
							indices.BlockIndex++;
							if (SetAsCurrentLocationIfRelevant(indices))
								return; // This can happen, for example if we're switching out of rainbow mode and going from a previously relevant matchup to a single relevant block within it.
						}
					}
					if (stayOnCurrentBlock)
					{
						SetBlock(m_temporarilyIncludedBookBlockIndices);
						return;
					}
				}
				LoadNextRelevantBlock();
			}
			else if (selectedBlock != null)
			{
				m_temporarilyIncludedBookBlockIndices = BlockAccessor.GetIndicesOfSpecificBlock(selectedBlock);
				m_navigator.SetIndices(m_temporarilyIncludedBookBlockIndices);
			}
			else
			{
				m_temporarilyIncludedBookBlockIndices = BlockAccessor.GetIndices();
			}

			FilterReset?.Invoke(this, new EventArgs());
		}

		private bool SetAsCurrentLocationIfRelevant(BookBlockIndices indices)
		{
			int i;
			for (i = 0; i < m_relevantBookBlockIndices.Count; i++)
			{
				if (m_relevantBookBlockIndices[i] == indices || m_relevantBookBlockIndices[i].Contains(indices))
					break;
			}
			if (i == m_relevantBookBlockIndices.Count)
				return false;
			m_currentRelevantIndex = i;
			m_temporarilyIncludedBookBlockIndices = null;
			SetBlock(m_relevantBookBlockIndices[m_currentRelevantIndex]);
			return true;
		}
		#endregion

		#region Context blocks
		protected IEnumerable<Block> ContextBlocksBackward
		{
			get { return BlockAccessor.GetPreviousNBlocksWithinBook(BackwardContextBlockCount); }
		}

		protected IEnumerable<Block> ContextBlocksForward
		{
			get { return BlockAccessor.GetNextNBlocksWithinBook(ForwardContextBlockCount); }
		}
		#endregion

		#region HTML Browser support
		public string Html
		{
			get
			{
				return BuildHtml(
					BuildContextBlocksHtml(ContextBlocksBackward),
					BuildCurrentBlockHtml(),
					BuildContextBlocksHtml(ContextBlocksForward),
					BuildStyle());
			}
		}

		private string BuildHtml(string previousText, string mainText, string followingText, string style)
		{
			var bldr = new StringBuilder();
			bldr.Append(previousText);
			bldr.Append("<div id=\"");
			bldr.Append(kMainQuoteElementId);
			bldr.Append("\" class=\"highlight\">");
			bldr.Append(mainText);
			bldr.Append("</div>");
			if (!String.IsNullOrEmpty(followingText))
				bldr.Append(kHtmlLineBreak).Append(followingText);
			var bodyAttributes = m_font.RightToLeftScript ? "class=\"right-to-left\"" : "";
			return String.Format(kHtmlFrame, style, bodyAttributes, bldr);
		}

		private string BuildHtml(IEnumerable<Block> blocks)
		{
			var bldr = new StringBuilder();
			foreach (Block block in blocks)
				bldr.Append(BuildHtml(block));
			return bldr.ToString();
		}

		private string BuildHtml(Block block)
		{
			string text = block.GetTextAsHtml(m_showVerseNumbers, m_font.RightToLeftScript);
			var bldr = new StringBuilder();
			bldr.Append("<div");
			if (block.StyleTag.StartsWith("s"))
				bldr.Append(" class=\"block section-header\"");
			else if (block.IsChapterAnnouncement)
				bldr.Append(" class=\"block chapter-label\"");
			else if (block.IsScripture)
				bldr.Append(" class=\"block scripture\"");
			else
				bldr.Append(" class=\"block\"");
			bldr.Append(" data-block-index-in-book=\"").Append(BlockAccessor.GetIndicesOfSpecificBlock(block).BlockIndex).Append("\"");
			bldr.Append(">");
			bldr.Append(text);
			bldr.Append("</div>");
			return bldr.ToString();
		}

		private string BuildCurrentBlockHtml()
		{
			return BuildHtml(GetAllBlocksWhichContinueTheQuoteStartedByBlock(CurrentBlock));
		}

		private string BuildContextBlocksHtml(IEnumerable<Block> blocks)
		{
			var bldr = new StringBuilder();
			foreach (Block block in blocks)
			{
				bldr.Append("<div class=\"").Append(kCssClassContext).Append("\" ").Append(kDataCharacter).Append("=\"").Append(block.CharacterId).Append("\">");
				foreach (Block innerBlock in GetAllBlocksWhichContinueTheQuoteStartedByBlock(block))
					bldr.Append(BuildHtml(innerBlock));
				bldr.Append("</div>");
				if (!block.IsContinuationOfPreviousBlockQuote)
					bldr.Append(kHtmlLineBreak);
			}
			return bldr.ToString();
		}

		private string BuildStyle()
		{
			return String.Format(kCssFrame, m_font.FontFamily, m_font.Size);
		}
		#endregion

		#region Methods for dealing with multi-block groups/quotes
		public IEnumerable<Block> GetAllBlocksWhichContinueTheQuoteStartedByBlock(Block firstBlock)
		{
			return m_navigator.GetAllBlocksWhichContinueTheQuoteStartedByBlock(firstBlock, CountOfBlocksAddedByCurrentMatchup);
		}

		public IEnumerable<int> GetIndicesOfQuoteContinuationBlocks(Block startQuoteBlock)
		{
			// Note this method assumes the startQuoteBlock is in the navigator's current book.
			return m_navigator.GetIndicesOfQuoteContinuationBlocks(startQuoteBlock, CountOfBlocksAddedByCurrentMatchup);
		}

		public int IndexOfFirstBlockInCurrentGroup
		{
			get
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					return BlockAccessor.GetIndicesOfSpecificBlock(CurrentBlock).BlockIndex;
				return m_currentRefBlockMatchups.IndexOfStartBlockInBook;
			}
		}
		public int IndexOfLastBlockInCurrentGroup
		{
			get
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
				{
					if (CurrentBlock.MultiBlockQuote == MultiBlockQuote.Start)
					{
						var continuationIndices = GetIndicesOfQuoteContinuationBlocks(CurrentBlock).LastOrDefault();
						if (continuationIndices != 0)
							return continuationIndices;

						// This is a serious data corruption. If this ever happens, in addition to finding and fixing the cause of it, a
						// ProjectDataMigration step will be required (most likely, calling CleanUpOrphanedMultiBlockQuoteStati).
						ErrorReport.ReportNonFatalMessageWithStackTrace(Localizer.GetString("Project.StartBlockWithoutContinuation",
							"Data problem: Start block has no subsequent continuation blocks: {0}"), CurrentBlock.ToString(true, CurrentBookId));
						return CurrentBlockIndexInBook;
					}
					return IndexOfFirstBlockInCurrentGroup;
				}
				return m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count - 1;
			}
		}
		#endregion

		#region GetBlockReference
		public string GetBlockReferenceString(Block block = null)
		{
			block = block ?? BlockAccessor.CurrentBlock;
			var startRef = new BCVRef(BCVRef.BookToNumber(CurrentBookId), block.ChapterNumber, block.InitialStartVerseNumber);
			var lastVerseInBlock = block.LastVerseNum;
			var endRef = (lastVerseInBlock <= block.InitialStartVerseNumber) ? startRef :
				new BCVRef(startRef.Book, startRef.Chapter, lastVerseInBlock);
			return GetReferenceString(startRef, endRef);
		}

		public string GetReferenceString(BCVRef startRef, BCVRef endRef)
		{
			return BCVRef.MakeReferenceString(startRef, endRef, ":", "-");
		}

		public VerseRef GetBlockVerseRef()
		{
			return BlockAccessor.CurrentBlock.StartRef(CurrentBookNumber, Versification);
		}

		public int GetLastVerseInCurrentQuote()
		{
			return GetLastBlockInCurrentQuote().LastVerseNum;
		}

		public Block GetLastBlockInCurrentQuote()
		{
			if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation && m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting != 0)
				return m_currentRefBlockMatchups.CorrelatedBlocks.Last();

			if (CurrentBlock.MultiBlockQuote == MultiBlockQuote.None)
				return CurrentBlock;

			var lastBlock = BlockAccessor.GetSurroundingBlocksWithinBookWhile(b => b.MultiBlockQuote != MultiBlockQuote.None && b.MultiBlockQuote != MultiBlockQuote.Start, true).LastOrDefault();
			return lastBlock ?? CurrentBlock;
		}
		#endregion

		#region Navigation methods
		public void UpdateNavigatorForIncludedBooks(IEnumerable<string> filteredListOfBookIds = null)
		{
			BlockNavigator = new BlockNavigator(filteredListOfBookIds == null ? m_project.IncludedBooks :
				m_project.IncludedBooks.Where(ib => filteredListOfBookIds.Contains(ib.BookId)).ToList());
		}

		public Block GetNthBlockInCurrentBook(int i)
		{
			if (BlockGroupingStyle != BlockGroupingType.BlockCorrelation)
			{
				return BlockAccessor.CurrentBook.GetScriptBlocks()[i];
			}
			if (m_currentRefBlockMatchups.IndexOfStartBlockInBook > i)
			{
				return BlockAccessor.CurrentBook.GetScriptBlocks()[i];
			}
			if (i < m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count)
			{
				return m_currentRefBlockMatchups.CorrelatedBlocks[i - m_currentRefBlockMatchups.IndexOfStartBlockInBook];
			}
			return BlockAccessor.CurrentBook.GetScriptBlocks()[i - m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting];
		}

		public bool CanNavigateToPreviousRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentLocationRelevant)
				{
					if (m_currentRelevantIndex == 0)
						return false;
					if (BlockGroupingStyle == BlockGroupingType.Quote)
						return true;
					Debug.Assert(m_currentRelevantIndex >= 0);
					return GetIndexOfPreviousRelevantBlockNotInCurrentMatchup() >= 0;
				}

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block before it.
				var firstRelevantBlock = m_relevantBookBlockIndices[0];
				var indicesOfCurrentLocation = m_temporarilyIncludedBookBlockIndices ?? BlockAccessor.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.First());
				return s_bookBlockComparer.Compare(firstRelevantBlock, indicesOfCurrentLocation) < 0;
			}
		}

		private bool m_isRecursiveCall;
		public bool CanNavigateToNextRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
				{
					m_isRecursiveCall = false;
					return false;
				}

				if (IsCurrentLocationRelevant)
				{
					if (m_currentRelevantIndex == RelevantBlockCount - 1)
					{
						m_isRecursiveCall = false;
						return false;
					}

					if (BlockGroupingStyle == BlockGroupingType.Quote)
					{
						m_isRecursiveCall = false;
						return true;
					}

					if (m_currentRelevantIndex < 0)
					{
						if (m_isRecursiveCall)
						{
							throw new IndexOutOfRangeException("If Current Block is Relevant, m_currentRelevantIndex should be the index of that block!");
						}

						// My original hope was that I had fully solved this with a call to SetModeInternal at the end of SplitBlock.
						// However, just after I had completed and tested that change (but before it was deployed),
						// a case of this exception was reported for which there was no evidence of any split having been performed (PG-1078).
						// So, here is yet another hack to try to recover from a state we should never get in...
						SetModeInternal(Mode, true);
						m_isRecursiveCall = true;
						return CanNavigateToNextRelevantBlock;
					}
					m_isRecursiveCall = false;
					return GetIndexOfNextRelevantBlockNotInCurrentMatchup() > m_currentRelevantIndex;
				}

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block after it.
				var indicesOfCurrentLocation = m_currentRefBlockMatchups == null ? m_temporarilyIncludedBookBlockIndices :
					BlockAccessor.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.Last());
				m_isRecursiveCall = false;
				return s_bookBlockComparer.Compare(m_relevantBookBlockIndices.Last(), indicesOfCurrentLocation) > 0;
			}
		}

		public bool TryLoadBlock(VerseRef verseRef)
		{
			if (TrySelectRefInCurrentBlockMatchup(verseRef))
				return true;

			var indices = BlockAccessor.GetIndicesOfFirstBlockAtReference(verseRef, AttemptRefBlockMatchup);
			if (indices == null)
				return false;

			var indexOfRelevantBlock = m_relevantBookBlockIndices.IndexOf(indices);
			if (indexOfRelevantBlock < 0)
			{
				var block = GetBlock(indices);
				if (CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId))
					return false;
				m_temporarilyIncludedBookBlockIndices = indices;
			}
			else
				m_temporarilyIncludedBookBlockIndices = null;

			m_currentRelevantIndex = indexOfRelevantBlock;

			SetBlock(indices);
			TrySelectRefInCurrentBlockMatchup(verseRef);
			return true;
		}

		private bool TrySelectRefInCurrentBlockMatchup(VerseRef verseRef)
		{
			if (m_currentRefBlockMatchups != null && CurrentBook.BookNumber == verseRef.BookNum && CurrentBlock.ChapterNumber == verseRef.ChapterNum &&
				m_currentRefBlockMatchups.CorrelatedBlocks.First().InitialStartVerseNumber <= verseRef.VerseNum &&
				m_currentRefBlockMatchups.CorrelatedBlocks.Last().LastVerseNum >= verseRef.VerseNum)
			{
				// Just need to find the correct block within the current matchup and select it as the anchor block.
				foreach (var block in m_currentRefBlockMatchups.CorrelatedBlocks)
				{
					if (block.LastVerseNum >= verseRef.VerseNum)
					{
						if (m_currentRefBlockMatchups.CorrelatedAnchorBlock != block)
						{
							m_currentRefBlockMatchups.ChangeAnchor(block);
							HandleCurrentBlockChanged();
						}
						return true;
					}
				}
			}

			return false;
		}

		// Internal for testing
		protected internal void LoadNextRelevantBlockInSubsequentBook()
		{
			if (!CanNavigateToNextRelevantBlock)
				return;
			if (BlockAccessor.IsLastBook(CurrentBook))
				return;

			var currentBookIndex = BlockAccessor.GetIndicesOfSpecificBlock(BlockAccessor.CurrentBlock).BookIndex;

			var blockIndex = (IsCurrentLocationRelevant) ? m_currentRelevantIndex + 1 :
				GetIndexOfClosestRelevantBlock(m_relevantBookBlockIndices, m_temporarilyIncludedBookBlockIndices, false, 0, RelevantBlockCount - 1);

			var bookBlockIndices = m_relevantBookBlockIndices[blockIndex];
			while (bookBlockIndices.BookIndex == currentBookIndex)
			{
				if (++blockIndex >= m_relevantBookBlockIndices.Count)
					return;
				bookBlockIndices = m_relevantBookBlockIndices[blockIndex];
			}

			m_currentRelevantIndex = blockIndex;
			SetBlock(bookBlockIndices);
		}

		public void LoadNextRelevantBlock()
		{
			if (IsCurrentLocationRelevant)
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					m_currentRelevantIndex++;
				else
					m_currentRelevantIndex = GetIndexOfNextRelevantBlockNotInCurrentMatchup();
				SetBlock(m_relevantBookBlockIndices[m_currentRelevantIndex]);
			}
			else
				LoadClosestRelevantBlock(false);
		}

		private int GetIndexOfPreviousRelevantBlockNotInCurrentMatchup()
		{
			for (int i = m_currentRelevantIndex - 1; i >= 0; i--)
			{
				if (m_relevantBookBlockIndices[i].BookIndex != m_relevantBookBlockIndices[m_currentRelevantIndex].BookIndex ||
					!m_currentRefBlockMatchups.OriginalBlocks.Contains(CurrentBook.GetScriptBlocks()[m_relevantBookBlockIndices[i].BlockIndex]))
				{
					return i;
				}
			}
			return -1;
		}

		private int GetIndexOfNextRelevantBlockNotInCurrentMatchup()
		{
			for (int i = m_currentRelevantIndex + 1; i < RelevantBlockCount; i++)
			{
				try
				{
					if (m_relevantBookBlockIndices[i].BookIndex != m_relevantBookBlockIndices[m_currentRelevantIndex].BookIndex ||
						!m_currentRefBlockMatchups.OriginalBlocks.Contains(
							CurrentBook.GetScriptBlocks()[m_relevantBookBlockIndices[i].BlockIndex]))
					{
						return i;
					}
				}
				catch (Exception e) when (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException)
				{
					throw new IndexOutOfRangeException($"Index out of range. RelevantBlockCount = {RelevantBlockCount}. " +
						$"m_currentRelevantIndex = {m_currentRelevantIndex}. i = {i}.", e);
				}
			}
			return -1;
		}

		public void LoadPreviousRelevantBlock()
		{
			if (IsCurrentLocationRelevant)
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					m_currentRelevantIndex--;
				else
					m_currentRelevantIndex = GetIndexOfPreviousRelevantBlockNotInCurrentMatchup();
				SetBlock(m_relevantBookBlockIndices[m_currentRelevantIndex]);
			}
			else
				LoadClosestRelevantBlock(true);
		}

		private void LoadClosestRelevantBlock(bool prev)
		{
			if (m_temporarilyIncludedBookBlockIndices == null)
				m_currentRelevantIndex = 0;
			else
			{
				m_currentRelevantIndex = GetIndexOfClosestRelevantBlock(m_relevantBookBlockIndices, m_temporarilyIncludedBookBlockIndices, prev, 0, RelevantBlockCount - 1);
				m_temporarilyIncludedBookBlockIndices = null;
			}

			if (m_currentRelevantIndex < 0)
				m_currentRelevantIndex = 0;

			SetBlock(m_relevantBookBlockIndices[m_currentRelevantIndex]);
		}

		private void SetBlock(BookBlockIndices indices, bool clearBlockMatchup = true)
		{
			if (clearBlockMatchup && m_currentRefBlockMatchups != null &&
				(m_includedBooks.ElementAt(indices.BookIndex) != m_currentRefBlockMatchups.BookId ||
					indices.BlockIndex != m_currentRefBlockMatchups.IndexOfStartBlockInBook ||
					indices.BlockCount != m_currentRefBlockMatchups.OriginalBlockCount))
			{
				ClearBlockMatchup();
			}
			m_navigator.SetIndices(indices);
			if (!IsCurrentLocationRelevant)
				m_temporarilyIncludedBookBlockIndices = indices;
			if (m_currentRefBlockMatchups == null)
			{
				SetBlockMatchupForCurrentLocation();
				if (m_currentRefBlockMatchups != null &&
					!indices.IsMultiBlock &&
					// Pretty sure these next two checks will always be true, but better safe than sorry:
					m_currentRefBlockMatchups.IndexOfStartBlockInBook <= indices.BlockIndex &&
					m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.OriginalBlockCount >= indices.BlockIndex &&
					m_currentRefBlockMatchups.CorrelatedAnchorBlock != m_currentRefBlockMatchups.CorrelatedBlocks[indices.BlockIndex - m_currentRefBlockMatchups.IndexOfStartBlockInBook])
				{
					m_currentRefBlockMatchups.ChangeAnchor(m_currentRefBlockMatchups.CorrelatedBlocks[indices.BlockIndex - m_currentRefBlockMatchups.IndexOfStartBlockInBook]);
				}
			}

			HandleCurrentBlockChanged();
		}

		public void SetBlockMatchupForCurrentLocation()
		{
			if (!AttemptRefBlockMatchup || CurrentBook.SingleVoice)
				return;

			var origValue = m_currentRefBlockMatchups;
			var currentIndices = BlockAccessor.GetIndices();

			Logger.WriteMinorEvent($"Setting block matchup for block {currentIndices.BlockIndex} in " +
				$"{CurrentBook.BookId} {CurrentBlock.ChapterNumber}:{CurrentBlock.InitialStartVerseNumber}");

			m_currentRefBlockMatchups = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook,
				currentIndices.BlockIndex, m_project.ReportingClauses, currentIndices.MultiBlockCount);
			if (m_currentRefBlockMatchups != null)
			{
				m_currentRefBlockMatchups.MatchAllBlocks();
				// We might have gotten here by ad-hoc navigation (clicking or using the Verse Reference control). Since we are in "rainbow mode"
				// the filter holds *groups* of relevant blocks (rather than individual ones), so if the new current matchup corresponds to one
				// of those groups (i.e., it is relevant), we need to set indices based on the group rather than the individual block. Otherwise,
				// we'll lose track of our place in the list (which not only affects the display index but also can lead to crashes, such as PG-924,
				// later when we try to go to the previous or next relevant passage).
				if (IsCurrentLocationRelevant && m_temporarilyIncludedBookBlockIndices != null && !BlockAccessor.GetIndices().IsMultiBlock)
				{
					m_navigator.SetIndices(new BookBlockIndices(BlockAccessor.GetIndices().BookIndex, m_currentRefBlockMatchups.IndexOfStartBlockInBook, (uint)m_currentRefBlockMatchups.OriginalBlockCount));
					m_temporarilyIncludedBookBlockIndices = null;
					m_currentRelevantIndex = m_relevantBookBlockIndices.IndexOf(BlockAccessor.GetIndices());
				}
			}
			if (origValue != m_currentRefBlockMatchups)
				CurrentBlockMatchupChanged?.Invoke(this, new EventArgs());
		}

		protected void ClearBlockMatchup()
		{
			if (m_currentRefBlockMatchups == null)
				return;
			var relevant = IsCurrentLocationRelevant;
			m_currentRefBlockMatchups = null;
			if (!relevant)
				m_temporarilyIncludedBookBlockIndices = GetCurrentBlockIndices();
			CurrentBlockMatchupChanged?.Invoke(this, new EventArgs());
		}

		protected IEnumerable<BookBlockIndices> IndicesOfOriginalRelevantBlocks
		{
			get
			{
				int bookIndex = GetCurrentBlockIndices().BookIndex;

				for (int i = 0; i < CurrentReferenceTextMatchup.OriginalBlockCount; i++)
				{
					var indices = new BookBlockIndices(bookIndex, CurrentReferenceTextMatchup.IndexOfStartBlockInBook + i);
					if (m_relevantBookBlockIndices.Contains(indices))
						yield return indices;
				}
			}
		}

		public virtual void ApplyCurrentReferenceTextMatchup()
		{
			if (BlockGroupingStyle != BlockGroupingType.BlockCorrelation)
				throw new InvalidOperationException("No current reference text block matchup!");
			Debug.Assert(m_currentRefBlockMatchups != null);
			if (!m_currentRefBlockMatchups.HasOutstandingChangesToApply)
				throw new InvalidOperationException("Current reference text block matchup has no outstanding changes!");
			var insertions = m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting;
			var insertionIndex = m_currentRelevantIndex;

			m_currentRefBlockMatchups.Apply();
			if (insertionIndex < 0)
			{
				var indicesOfFirstBlock = BlockAccessor.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.First());
				insertionIndex = GetIndexOfClosestRelevantBlock(m_relevantBookBlockIndices, indicesOfFirstBlock, false, 0, RelevantBlockCount - 1);
				if (insertionIndex < 0)
					insertionIndex = RelevantBlockCount;
			}
			else if (insertionIndex > RelevantBlockCount) // PG-823: We just removed multiple relevant blocks, such that the insertion index is out of range.
				insertionIndex = RelevantBlockCount;

			var origRelevantBlockCount = RelevantBlockCount;

			m_navigator.ExtendCurrentBlockGroup((uint)insertions);

			// Insertions before the anchor block can mess up m_currentBlockIndex, so we need to reset it to point to the newly inserted
			// block that corresponds to the "anchor" block. Since the "OriginalBlocks" is not a cloned copy of the "CorrelatedBlocks",
			// We can safely use the index of the anchor block in CorrelatedBlocks to find the correct block in OriginalBlocks.
			if (!BlockAccessor.GetIndices().IsMultiBlock)
			{
				var originalAnchorBlock = m_currentRefBlockMatchups.OriginalBlocks.ElementAt(
						m_currentRefBlockMatchups.CorrelatedBlocks.IndexOf(m_currentRefBlockMatchups.CorrelatedAnchorBlock));
				SetBlock(BlockAccessor.GetIndicesOfSpecificBlock(originalAnchorBlock), false);
			}
			if (insertions > 0)
			{
				var currentBookIndex = BlockAccessor.GetIndices().BookIndex;
				var startIndex = insertionIndex + RelevantBlockCount - origRelevantBlockCount;
				// PG-1263: Could not figure out what went wrong, so I'm adding this check to try to analyze the
				// individual pieces if this ever happens again.
				if (startIndex < 0)
					throw new IndexOutOfRangeException($"Start index should never be negative. " +
						$"{nameof(insertionIndex)} = {insertionIndex}; " +
						$"{nameof(RelevantBlockCount)} = {RelevantBlockCount}; " +
						$"{nameof(origRelevantBlockCount)} = {origRelevantBlockCount}; ");
				if (m_currentRelevantIndex >= 0 && BlockAccessor.GetIndices().IsMultiBlock)
				{
					// Since this "relevant passage" is a multi-block matchup (as opposed to a single block), rather than incrementing the
					// BlockIndex, we want to extend the count. Otherwise, this will cease to be relevant, and when the user clicks
					// the Previous button, they will no longer get the same blocks selected.
					m_relevantBookBlockIndices[m_currentRelevantIndex].ExtendToIncludeMoreBlocks((uint)insertions);
					startIndex++;
				}
				for (int i = startIndex; i < RelevantBlockCount && m_relevantBookBlockIndices[i].BookIndex == currentBookIndex; i++)
					m_relevantBookBlockIndices[i].BlockIndex += insertions;
			}

			ProcessNewHeSaidRenderings();
		}

		private void ProcessNewHeSaidRenderings()
		{
			if (m_project.AddNewReportingClauses(m_currentRefBlockMatchups.HeSaidBlocks.Select(b => b.GetText(false).Trim()).Distinct()))
			{
				for (var i = 0; i < m_relevantBookBlockIndices.Count;)
				{
					if (i == m_currentRelevantIndex)
					{
						i++;
						continue;
					}

					var location = m_relevantBookBlockIndices[i];
					var matchup = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(m_project.IncludedBooks[location.BookIndex],
						location.BlockIndex, m_project.ReportingClauses, location.MultiBlockCount);
					// We do not want to remove blocks that the user has matched manually (in case they want to navigate back to them),
					// so if all of the (original) blocks are explicitly matched, then leave it alone.
					if (matchup != null && matchup.OriginalBlocks.Any(b => !b.MatchesReferenceText) && !IsRelevant(matchup))
					{
						if (i < m_currentRelevantIndex)
							m_currentRelevantIndex--;
						m_relevantBookBlockIndices.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}
		}

		protected virtual void HandleCurrentBlockChanged()
		{
			CurrentBlockChanged?.Invoke(this, new EventArgs());
			StoreCurrentBlockIndices();
		}

		protected virtual void StoreCurrentBlockIndices()
		{
		}

		protected BookBlockIndices GetCurrentBlockIndices()
		{
			return BlockAccessor.GetIndices();
		}

		internal BookBlockIndices GetBlockIndices(Block block)
		{
			return BlockAccessor.GetIndicesOfSpecificBlock(block);
		}

		private Block GetBlock(BookBlockIndices indices)
		{
			var startingBook = m_project.IncludedBooks[indices.BookIndex];
			var blocks = startingBook.GetScriptBlocks();
			return blocks.Count > indices.BlockIndex ? blocks[indices.BlockIndex] : null;
		}

		public static int GetIndexOfClosestRelevantBlock(List<BookBlockIndices> list, BookBlockIndices key, bool prev,
			int min, int max)
		{
			if (min > max)
			{
				if (prev)
					return (max >= 0 && max < list.Count && s_bookBlockComparer.Compare(key, list[max]) > 0) ? max : -1;

				return (min >= 0 && min < list.Count && s_bookBlockComparer.Compare(key, list[min]) < 0) ? min : -1;
			}
			int mid = (min + max) / 2;

			int comparison = s_bookBlockComparer.Compare(key, list[mid]);

			if (comparison == 0)
				throw new ArgumentException("Block not expected to be in existing list", "key");

			if (comparison < 0)
				return GetIndexOfClosestRelevantBlock(list, key, prev, min, mid - 1);

			return GetIndexOfClosestRelevantBlock(list, key, prev, mid + 1, max);
		}
		#endregion

		#region Filtering methods
		protected virtual void PopulateRelevantBlocks()
		{
			m_navigator.GoToFirstBlock();
			m_relevantBookBlockIndices = new List<BookBlockIndices>();
			Block block = BlockAccessor.CurrentBlock;
			for (; ; )
			{
				if (block.IsScripture)
				{
					var indices = BlockAccessor.GetIndices();

					BlockMatchup matchup;
					if (AttemptRefBlockMatchup &&
						(matchup = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook, indices.BlockIndex, m_project.ReportingClauses)) != null)
					{
						if (indices.ExtendForMatchup(matchup))
							m_navigator.SetIndices(indices); // The call to GetIndices (above) gets a copy, so we need to set the state to reflect the updated Multi-block count

						// TODO (PG-784): If a book is single-voice, no block in it should match this filter.
						//if (!CurrentBookIsSingleVoice)
						if (IsRelevant(matchup))
						{
							AddRelevant(indices);
						}
					}
					else if (IsRelevant(block))
					{
						AddRelevant(indices);
					}
				}

				if (BlockAccessor.IsLastBlock())
					break;
				block = m_navigator.GoToNextBlock();
			}

			m_navigator.GoToFirstBlock();
		}

		private void AddRelevant(BookBlockIndices indices, bool reSort = false)
		{
			m_relevantBookBlockIndices.Add(indices);
			if (reSort)
				m_relevantBookBlockIndices.Sort();
			RelevantBlocksAdded(indices);
		}

		protected virtual void RelevantBlocksAdded(BookBlockIndices addedBlocks)
		{
			// No-op in base class
		}

		private bool IsRelevant(BlockMatchup matchup) =>
			matchup.OriginalBlocks.Any(b => IsRelevant(b)) ||
			(Mode & BlocksToDisplay.NotAlignedToReferenceText) > 0 && !matchup.AllScriptureBlocksMatch;

		private bool IsRelevant(Block block, bool rebuildingFilter = true)
		{
			if ((Mode & BlocksToDisplay.NotAlignedToReferenceText) > 0)
				return block.CharacterIsUnclear;
			if (block.IsContinuationOfPreviousBlockQuote)
				return false;
			if (rebuildingFilter && (Mode & BlocksToDisplay.ExcludeUserConfirmed) > 0 && block.UserConfirmed)
				return false;
			if ((Mode & BlocksToDisplay.NotAssignedAutomatically) > 0)
				return BlockNotAssignedAutomatically(block);

			if ((Mode & BlocksToDisplay.AllExpectedQuotes) > 0)
				return IsBlockForVerseWithExpectedQuote(block);
			if ((Mode & BlocksToDisplay.MissingExpectedQuote) > 0)
			{
				if (CurrentBookIsSingleVoice)
					return false;

				if (block.IsQuote || CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId))
					return false;

				// When rebuilding the filter, we can be sure that any block that starts mid-verse has already had that first partial verse
				// checked, so we can save some time by ony checking any verses that start in the block and then we only have to examine any
				// following blocks (for the same final verse) to see if there is a quote.
				var versesToCheck = rebuildingFilter ? block.BlockElements.OfType<IVerse>() : block.AllVerses;
				var versesWithPotentialMissingQuote =
					versesToCheck.SelectMany(v => v.AllVerseNumbers).Where(verse => ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber,
					block.ChapterNumber, new SingleVerse(verse), Versification).Any(c => c.IsExpected))
					.Select(v => new BCVRef(CurrentBookNumber, block.ChapterNumber, v)).ToList();
				if (!versesWithPotentialMissingQuote.Any())
					return false;

				return BlockHasMissingExpectedQuote(block, rebuildingFilter, versesWithPotentialMissingQuote);
			}
			if ((Mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) > 0)
			{
				if (!block.IsQuote || CurrentBookIsSingleVoice)
					return false;

				var expectedSpeakers = ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber, block.ChapterNumber,
					(Block.InitialVerseNumberBridgeFromBlock)block, Versification).Distinct(m_characterEqualityComparer).Count();

				var actualQuotes = 1; // this is the quote represented by the given block.

				if (actualQuotes > expectedSpeakers)
					return true;

				// Check surrounding blocks to count quote blocks for same verse.
				foreach (var surroundingBlockInVerse in BlockAccessor.GetSurroundingBlocksWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber, false, block))
				{
					if (surroundingBlockInVerse.IsQuoteStart)
					{
						if (++actualQuotes > expectedSpeakers)
							return true;
					}
				}

				return false;
			}
			if ((Mode & BlocksToDisplay.AllScripture) > 0)
				return true;
			if ((Mode & BlocksToDisplay.AllQuotes) > 0)
				return block.IsQuote;
			if ((Mode & BlocksToDisplay.NeedsReview) > 0)
				return block.CharacterIdInScript == CharacterVerseData.kNeedsReview;
			return false;
		}

		private bool IsBlockForVerseWithExpectedQuote(Block block)
		{
			if (!block.IsScripture)
				return false;
			return ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber, block.ChapterNumber,
				(Block.VerseRangeFromBlock)block, Versification).Any(c => c.IsExpected);
		}

		private bool BlockNotAssignedAutomatically(Block block)
		{
			if (CurrentBookIsSingleVoice)
				return false;

			return (block.UserConfirmed || block.CharacterIsUnclear);
		}

		internal bool BlockHasMissingExpectedQuote(Block startBlock, bool searchForwardOnly, IEnumerable<BCVRef> versesWithPotentialMissingQuote)
		{
			return versesWithPotentialMissingQuote.Any(verse => BlockAccessor.GetSurroundingBlocksWithinBookWhile
				(b => PeekBackwardBlocksMatch(b, verse), searchForwardOnly, startBlock).All(b => !b.IsQuote));
		}

		static bool PeekBackwardBlocksMatch(Block block , BCVRef verse)
		{
			if (block.ChapterNumber != verse.Chapter) return false;

			if (block.LastVerseNum == verse.Verse) return true;

			// check for verse surrounded by the block (can happen if there is a verse bridge)
			if ((verse.Verse >= block.InitialStartVerseNumber) && (verse.Verse < block.LastVerseNum))
				return true;

			return false;
		}

		static bool PeekForwardBlocksMatch(Block block, BCVRef verse)
		{
			if (block.ChapterNumber != verse.Chapter) return false;

			if (block.InitialStartVerseNumber == verse.Verse) return true;

			// check for verse surrounded by the block (can happen if there is a verse bridge)
			if ((verse.Verse > block.InitialStartVerseNumber) && (verse.Verse <= block.LastVerseNum))
				return true;

			return false;
		}

		protected void AddToRelevantBlocksIfNeeded(Block newOrModifiedBlock, bool blockIsNew)
		{
			if (m_currentRefBlockMatchups != null)
			{
				if (blockIsNew)
				{
					var indicesOfNewOrModifiedBlock = GetBlockIndices(newOrModifiedBlock);
					var currentIndices = BlockAccessor.GetIndices();
					if (currentIndices.BookIndex == indicesOfNewOrModifiedBlock.BookIndex &&
						currentIndices.BlockIndex <= indicesOfNewOrModifiedBlock.BlockIndex &&
						currentIndices.EffectiveFinalBlockIndex >= indicesOfNewOrModifiedBlock.BlockIndex - 1)
					{
						// We need to increment the count of blocks in both the navigator and the current
						// relevant block (if the current matchup is relevant). These BookBlockIndices objects
						// are (hopefully identical?) copies of each other.
						m_navigator.ExtendCurrentBlockGroup(1);
						if (m_currentRelevantIndex >= 0)
							m_relevantBookBlockIndices[m_currentRelevantIndex].ExtendToIncludeMoreBlocks(1);
						return;
					}
					Debug.Fail("Can this happen now?");
					// else, fall through in order do the simple addition of this block to relevant blocks if appropriate.
				}
				else
					return;
			}
			if (IsRelevant(newOrModifiedBlock, false))
			{
				var indicesOfNewOrModifiedBlock = GetBlockIndices(newOrModifiedBlock);
				AddRelevant(indicesOfNewOrModifiedBlock, true);
				if (m_currentRelevantIndex == -1 && CurrentBlock.Equals(newOrModifiedBlock))
					m_currentRelevantIndex = m_relevantBookBlockIndices.IndexOf(indicesOfNewOrModifiedBlock);
			}
			HandleCurrentBlockChanged();
		}

		/// <summary>
		/// Gets whether the specified block represents Scripture text. (Only Scripture blocks can have their
		/// character/delivery changed. Book titles, chapters, and section heads have characters assigned
		/// programmatically and cannot be changed.)
		/// </summary>
		public bool GetIsBlockScripture(int blockIndex)
		{
			return GetNthBlockInCurrentBook(blockIndex).IsScripture;
		}
		#endregion
	}

	#region BookBlockTupleComparer
	public class BookBlockTupleComparer : IComparer<BookBlockIndices>
	{
		public int Compare(BookBlockIndices x, BookBlockIndices y)
		{
			int item1Comparison = x.BookIndex.CompareTo(y.BookIndex);
			if (item1Comparison == 0)
			{
				return x.BlockIndex.CompareTo(y.BlockIndex);
			}
			return item1Comparison;
		}
	}
	#endregion
}
