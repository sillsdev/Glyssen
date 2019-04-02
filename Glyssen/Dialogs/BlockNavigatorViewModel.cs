using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Utilities;
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;

namespace Glyssen.Dialogs
{
	public class BlockNavigatorViewModel : IDisposable
	{
		protected readonly Project m_project;
		internal const string kDataCharacter = "data-character";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body {1}>{2}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		internal const string kCssClassContext = "context";
		private const string kCssFrame = Block.kCssFrame +
										".highlight{{background-color:highlight;color:highlighttext}}" +
										"." + kCssClassContext + ":hover{{background-color:#E1F0FF}}" +
										".block-spacer{{height:30px}}" +
										".section-header{{text-align:center;font-weight:bold}}" +
										".chapter-label{{font-weight:bold;font-size:150%}}";
		internal const string kMainQuoteElementId = "main-quote-text";

		private bool m_showVerseNumbers = true; // May make this configurable later
		private FontProxy m_font;
		private readonly Dictionary<ReferenceText, FontProxy> m_referenceTextFonts = new Dictionary<ReferenceText, FontProxy>();
		private BlockNavigator m_navigator;
		private readonly IEnumerable<string> m_includedBooks;
		protected List<BookBlockIndices> m_relevantBookBlockIndices;
		protected BookBlockIndices m_temporarilyIncludedBookBlockIndices;
		private static readonly BookBlockTupleComparer s_bookBlockComparer = new BookBlockTupleComparer();
		private int m_currentRelevantIndex = -1;
		private BlocksToDisplay m_mode;
		private BlockMatchup m_currentRefBlockMatchups;
		private bool m_attemptRefBlockMatchup;

		public event EventHandler UiFontSizeChanged;
		public event EventHandler CurrentBlockChanged;
		public event EventHandler CurrentBlockMatchupChanged;
		public event EventHandler FilterReset;

		protected BookScript CurrentBook => BlockAccessor.CurrentBook;

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode = BlocksToDisplay.AllScripture, ProjectSettingsViewModel settingsViewModel = null)
			: this(project, mode, null, settingsViewModel)
		{
		}

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode, BookBlockIndices startingIndices, ProjectSettingsViewModel settingsViewModel = null)
		{
			m_project = project;
			m_project.QuoteParseCompleted += HandleProjectQuoteParseCompleted;

			m_navigator = new BlockNavigator(m_project.IncludedBooks);

			m_includedBooks = project.IncludedBookIds;
			Versification = project.Versification;

			if (settingsViewModel != null)
			{
				m_font = new FontProxy(settingsViewModel.WsModel.CurrentDefaultFontName,
					(int)settingsViewModel.WsModel.CurrentDefaultFontSize, settingsViewModel.WsModel.CurrentRightToLeftScript);
			}
			else
			{
				m_font = new FontProxy(project.FontFamily, project.FontSizeInPoints, project.RightToLeftScript);
			}
			CacheReferenceTextFonts(project.ReferenceText);

			FontSizeUiAdjustment = project.FontSizeUiAdjustment;

			Mode = mode;

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
			m_referenceTextFonts[referenceText] = new FontProxy(referenceText.FontFamily,
				referenceText.FontSizeInPoints, referenceText.RightToLeftScript);

			if (referenceText.HasSecondaryReferenceText)
				CacheReferenceTextFonts(referenceText.SecondaryReferenceText);
		}

		public BlockNavigatorViewModel(IReadOnlyList<BookScript> books, ScrVers versification)
		{
			m_navigator = new BlockNavigator(books);

			m_includedBooks = books.Select(b => b.BookId);
			Versification = versification;

			Mode = BlocksToDisplay.AllScripture;
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
				m_project.FontSizeUiAdjustment = FontSizeUiAdjustment;
				m_project.QuoteParseCompleted -= HandleProjectQuoteParseCompleted;
			}

			if (m_font != null)
			{
				m_font.Dispose();
				m_font = null;
			}

			foreach (var fontProxy in m_referenceTextFonts.Values)
				fontProxy.Dispose();
			m_referenceTextFonts.Clear();
		}
		#endregion

		#region Public properties
		public ScrVers Versification { get; private set; }
		public int BlockCountForCurrentBook
		{
			get
			{
				var actualCount = BlockAccessor.CurrentBook.GetScriptBlocks().Count;
				var adjustment = BlockGroupingStyle == BlockGroupingType.BlockCorrelation ? m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting : 0;
				return actualCount + adjustment;
			}
		}
		public int RelevantBlockCount { get { return m_relevantBookBlockIndices.Count; } }
		public int CurrentBlockDisplayIndex
		{
			get
			{

				// If we're in block matchup mode and the current matchup group covers the last relevant block, then make display index
				// show as if we're on that very last block so it won't be confusing to the user why they can't click the Next button.
				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation &&
					m_currentRelevantIndex >= 0 &&
					m_currentRelevantIndex < RelevantBlockCount - 1 &&
					m_relevantBookBlockIndices[m_currentRelevantIndex].BookIndex == m_relevantBookBlockIndices.Last().BookIndex &&
					m_relevantBookBlockIndices.Skip(m_currentRelevantIndex + 1).All(i => m_currentRefBlockMatchups.OriginalBlocks.Contains(CurrentBook.GetScriptBlocks()[i.BlockIndex])))
				{
					return RelevantBlockCount;
				}
				return m_currentRelevantIndex + 1;
			}
		}

		public IBlockAccessor BlockAccessor => m_navigator;

		public string CurrentBookId => BlockAccessor.CurrentBook.BookId;
		public int CurrentBookNumber => BlockAccessor.CurrentBook.BookNumber;
		public bool CurrentBookIsSingleVoice => BlockAccessor.CurrentBook.SingleVoice;

		public Block CurrentBlock
		{
			get
			{
				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation && m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting != 0)
					return m_currentRefBlockMatchups.CorrelatedAnchorBlock;
				return BlockAccessor.CurrentBlock;
			}
		}
		public Block CurrentEndBlock => BlockAccessor.CurrentEndBlock;
		protected Block CurrentBlockInOriginal => BlockAccessor.CurrentBlock;
		public BlockMatchup CurrentReferenceTextMatchup => m_currentRefBlockMatchups;
		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }
		public string ProjectName => m_project.Name;
		public BlockGroupingType BlockGroupingStyle => m_currentRefBlockMatchups == null ? BlockGroupingType.Quote : BlockGroupingType.BlockCorrelation;
		public bool AttemptRefBlockMatchup
		{
			get { return m_attemptRefBlockMatchup; }
			set
			{
				if (m_attemptRefBlockMatchup == value)
					return;
				m_attemptRefBlockMatchup = value;
				if (value)
					SetBlockMatchupForCurrentVerse();
				else
					ClearBlockMatchup();
			}
		}

		public bool CanDisplayReferenceTextForCurrentBlock => m_project.ReferenceText.CanDisplayReferenceTextForBook(CurrentBook) && !CurrentBook.SingleVoice;

		public bool IsCurrentBlockRelevant
		{
			get
			{
				BookBlockIndices indices = BlockAccessor.GetIndices();
				return m_relevantBookBlockIndices.Any(i => i.Contains(indices));
			}
		}

		public IEnumerable<string> IncludedBooks { get { return m_includedBooks; } }
		public FontProxy Font { get { return m_font; } }
		public FontProxy PrimaryReferenceTextFont => m_referenceTextFonts[m_project.ReferenceText];
		public FontProxy EnglishReferenceTextFont
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
			get { return m_font.FontSizeUiAdjustment; }
			set
			{
				m_font.AdjustFontSize(value, true);
				foreach (var fontProxy in m_referenceTextFonts.Values)
					fontProxy.AdjustFontSize(value, true);

				if (UiFontSizeChanged != null)
					UiFontSizeChanged(this, new EventArgs());
			}
		}

		public int CurrentBlockIndexInBook
		{
			get => BlockAccessor.GetIndices().BlockIndex;
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
						if (newAnchorBlock != m_currentRefBlockMatchups.CorrelatedAnchorBlock)
						{
							// Just reset the anchor and get out.
							m_currentRefBlockMatchups.ChangeAnchor(newAnchorBlock);
							var relevantBlockIndex = m_relevantBookBlockIndices.IndexOf(new BookBlockIndices(bookIndex, value));
							if (relevantBlockIndex >= 0)
								m_currentRelevantIndex = relevantBlockIndex;
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

		public virtual BlocksToDisplay Mode
		{
			get => m_mode;
			set
			{
				if (m_mode == value)
					return;

				SetModeInternal(value);
			}
		}

		protected void SetModeInternal(BlocksToDisplay mode, bool stayOnCurrentBlock = false)
		{
			m_mode = mode;
			m_temporarilyIncludedBookBlockIndices = GetCurrentBlockIndices();
			ResetFilter(BlockAccessor.CurrentBlock, stayOnCurrentBlock);
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
						var indices = new BookBlockIndices(m_temporarilyIncludedBookBlockIndices);
						indices.MultiBlockCount = 0;
						for (int iBlock = 0; iBlock < m_temporarilyIncludedBookBlockIndices.MultiBlockCount; iBlock++)
						{
							indices.BlockIndex = m_temporarilyIncludedBookBlockIndices.BlockIndex + iBlock;
							if (SetAsCurrentLocationIfRelevant(indices))
								return;
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
			switch (firstBlock.MultiBlockQuote)
			{
				case MultiBlockQuote.Start:
					yield return firstBlock;
					foreach (var i in GetIndicesOfQuoteContinuationBlocks(firstBlock))
						yield return BlockAccessor.CurrentBook[i];
					break;
				case MultiBlockQuote.Continuation:
					// These should all be brought in through a Start block, so don't do anything with them here
					break;
				default:
					// Not part of a multi-block quote. Just return the base-line block
					yield return firstBlock;
					break;
			}
		}

		public IEnumerable<int> GetIndicesOfQuoteContinuationBlocks(Block startQuoteBlock)
		{
			// Note this method assumes the startQuoteBlock is in the navigator's current book.
			Debug.Assert(startQuoteBlock.MultiBlockQuote == MultiBlockQuote.Start);

			for (int j = BlockAccessor.GetIndicesOfSpecificBlock(startQuoteBlock).BlockIndex + 1; j < BlockCountForCurrentBook; j++)
			{
				Block block = BlockAccessor.CurrentBook[j];
				if (block == null || !block.IsContinuationOfPreviousBlockQuote)
					break;
				yield return j;
			}
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
						return GetIndicesOfQuoteContinuationBlocks(CurrentBlock).Last();
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

		public VerseRef GetBlockVerseRef(Block block = null, ScrVers targetVersification = null)
		{
			block = block ?? BlockAccessor.CurrentBlock;
			var verseRef =  block.StartRef(BCVRef.BookToNumber(CurrentBookId), Versification);
			if (targetVersification != null)
				verseRef.ChangeVersification(targetVersification);
			return verseRef;
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

			var lastBlock = BlockAccessor.GetNextBlocksWithinBookWhile(b => b.MultiBlockQuote != MultiBlockQuote.None && b.MultiBlockQuote != MultiBlockQuote.Start).LastOrDefault();
			return lastBlock ?? CurrentBlock;
		}
		#endregion

		#region Navigation methods
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

				if (IsCurrentBlockRelevant)
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

				if (IsCurrentBlockRelevant)
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
			var indices = BlockAccessor.GetIndicesOfFirstBlockAtReference(verseRef, AttemptRefBlockMatchup);
			if (indices == null)
				return false;

			m_currentRelevantIndex = m_relevantBookBlockIndices.IndexOf(indices);
			if (m_currentRelevantIndex < 0)
			{
				var block = GetBlock(indices);
				if (CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId))
					return false;
				m_temporarilyIncludedBookBlockIndices = indices;
			}
			else
				m_temporarilyIncludedBookBlockIndices = null;
			SetBlock(indices);
			return true;
		}

		// Internal for testing
		protected internal void LoadNextRelevantBlockInSubsequentBook()
		{
			if (!CanNavigateToNextRelevantBlock)
				return;
			if (BlockAccessor.IsLastBook(CurrentBook))
				return;

			var currentBookIndex = BlockAccessor.GetIndicesOfSpecificBlock(BlockAccessor.CurrentBlock).BookIndex;

			var blockIndex = (IsCurrentBlockRelevant) ? m_currentRelevantIndex + 1 :
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
			if (IsCurrentBlockRelevant)
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
				catch(IndexOutOfRangeException e)
				{
					throw new IndexOutOfRangeException($"Index out of range. RelevantBlockCount = {RelevantBlockCount}. " +
						$"m_currentRelevantIndex = {m_currentRelevantIndex}. i = {i}.", e);
				}
			}
			return -1;
		}

		public void LoadPreviousRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
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
			if (clearBlockMatchup)
				ClearBlockMatchup();
			m_navigator.SetIndices(indices);
			if (!IsCurrentBlockRelevant)
				m_temporarilyIncludedBookBlockIndices = indices;
			if (m_currentRefBlockMatchups == null)
				SetBlockMatchupForCurrentVerse();
			HandleCurrentBlockChanged();
		}

		public void SetBlockMatchupForCurrentVerse()
		{
			if (!AttemptRefBlockMatchup || CurrentBook.SingleVoice)
				return;

			var origValue = m_currentRefBlockMatchups;

			Logger.WriteMinorEvent($"Setting block matchup for block {CurrentBlockIndexInBook} in " +
				$"{CurrentBook.BookId} {CurrentBlock.ChapterNumber}:{CurrentBlock.InitialStartVerseNumber}");

			m_currentRefBlockMatchups = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook,
				CurrentBlockIndexInBook, BlockAccessor.GetIndices().MultiBlockCount);
			if (m_currentRefBlockMatchups != null)
			{
				m_currentRefBlockMatchups.MatchAllBlocks(m_project.Versification);
				// We might have gotten here by ad-hoc navigation (clicking or using the Verse Reference control). If we're using a filter
				// that holds *groups* of relevant blocks (rather than individual ones) and the new current block is in one of those groups
				// (i.e., it is relevant), we need to set indices based on the group rather than the individual block. Otherwise, we'll lose
				// track of our place in the list (which not only affects the display index but also can lead to crashes, such as PG-924)
				// later when we try to go to the previous or next relevant passage).
				if (IsCurrentBlockRelevant && m_temporarilyIncludedBookBlockIndices != null && !BlockAccessor.GetIndices().IsMultiBlock && m_relevantBookBlockIndices.Any(i => i.IsMultiBlock))
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
			var relevant = IsCurrentBlockRelevant;
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
			bool relevantBlockRemoved = false;
			if (insertions > 0 && !BlockAccessor.GetIndices().IsMultiBlock)
			{
				foreach (var indices in IndicesOfOriginalRelevantBlocks)
					relevantBlockRemoved |= m_relevantBookBlockIndices.Remove(indices);
			}

			m_currentRefBlockMatchups.Apply(m_project.Versification);
			if (insertionIndex < 0)
			{
				var indicesOfFirstBlock = BlockAccessor.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.First());
				insertionIndex = GetIndexOfClosestRelevantBlock(m_relevantBookBlockIndices, indicesOfFirstBlock, false, 0, m_relevantBookBlockIndices.Count - 1);
				if (insertionIndex == -1)
					insertionIndex = m_relevantBookBlockIndices.Count;
			}
			else if (insertionIndex > m_relevantBookBlockIndices.Count) // PG-823: We just removed multiple relevant blocks, such that the insertion index is out of range.
				insertionIndex = m_relevantBookBlockIndices.Count;

			var origRelevantBlockCount = RelevantBlockCount;

			if (relevantBlockRemoved)
			{
				m_relevantBookBlockIndices.InsertRange(insertionIndex,
					m_currentRefBlockMatchups.OriginalBlocks.Where(b => IsRelevant(b, true)).Select(b => BlockAccessor.GetIndicesOfSpecificBlock(b)));
				if (m_temporarilyIncludedBookBlockIndices != null)
				{
					var indexOfCurrentBlock = m_relevantBookBlockIndices.IndexOf(m_temporarilyIncludedBookBlockIndices);
					if (indexOfCurrentBlock >= 0)
					{
						m_temporarilyIncludedBookBlockIndices = null;
						m_currentRelevantIndex = indexOfCurrentBlock;
					}
				}
			}
			else if (BlockAccessor.GetIndices().IsMultiBlock)
			{
				m_navigator.ExtendCurrentBlockGroup((uint)insertions);
			}

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
				if (m_currentRelevantIndex >= 0 && BlockAccessor.GetIndices().IsMultiBlock)
				{
					// Since this "relevant passage" is a multi-block matchup (as opposed to a single block), rather than incrementing the
					// BlockIndex, we want to extend the count. Otherwise, this will cease to be relevant, and when the user clicks
					// the Previous button, they will no longer get the same blocks selected.
					m_relevantBookBlockIndices[m_currentRelevantIndex].MultiBlockCount += (uint) insertions;
					startIndex++;
				}
				for (int i = startIndex; i < RelevantBlockCount && m_relevantBookBlockIndices[i].BookIndex == currentBookIndex; i++)
					m_relevantBookBlockIndices[i].BlockIndex += insertions;
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
			BlockMatchup lastMatchup = null;

			m_navigator.GoToFirstBlock();
			m_relevantBookBlockIndices = new List<BookBlockIndices>();
			Block block = BlockAccessor.CurrentBlock;
			for (; ; )
			{
				if (IsRelevant(block, ref lastMatchup))
				{
					var indices = BlockAccessor.GetIndices();
					if (lastMatchup == null)
						RelevantBlockAdded(block);
					else
						indices.MultiBlockCount = (uint)lastMatchup.OriginalBlockCount;
					m_relevantBookBlockIndices.Add(indices);
				}
				if (BlockAccessor.IsLastBlock())
					break;
				block = m_navigator.GoToNextBlock();
			}

			m_navigator.GoToFirstBlock();
		}

		protected virtual void RelevantBlockAdded(Block block)
		{
			// No-op in base class
		}

		private bool IsRelevant(Block block, bool ignoreExcludeUserConfirmed = false)
		{
			BlockMatchup lastMatchup = null;
			return IsRelevant(block, ref lastMatchup, ignoreExcludeUserConfirmed);
		}

		private bool IsRelevant(Block block, ref BlockMatchup lastMatchup, bool ignoreExcludeUserConfirmed = false)
		{
			if ((Mode & BlocksToDisplay.NotAlignedToReferenceText) > 0)
			{
				// TODO (PG-784): If a book is single-voice, no block in it should match this filter.
				//if (CurrentBookIsSingleVoice)
				//	return false;

				if (!block.IsScripture)
					return false;

				// Note that the logic here is absolutely dependent on block being in CurrentBook!!!

				if (!m_project.ReferenceText.CanDisplayReferenceTextForBook(CurrentBook))
					return false;

				if (lastMatchup != null && lastMatchup.OriginalBlocks.Contains(block))
					return false;

				lastMatchup = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook,
					BlockAccessor.GetIndicesOfSpecificBlock(block).BlockIndex);

				return lastMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear()) ||
					(lastMatchup.OriginalBlocks.Count() > 1 && !lastMatchup.CorrelatedBlocks.All(b => b.MatchesReferenceText));
			}
			if (block.IsContinuationOfPreviousBlockQuote)
				return false;
			if (!ignoreExcludeUserConfirmed && (Mode & BlocksToDisplay.ExcludeUserConfirmed) > 0 && block.UserConfirmed)
				return false;
			if ((Mode & BlocksToDisplay.NotAssignedAutomatically) > 0)
				return BlockNotAssignedAutomatically(block);

			if ((Mode & BlocksToDisplay.AllExpectedQuotes) > 0)
				return IsBlockInVerseWithExpectedQuote(block);
			if ((Mode & BlocksToDisplay.MissingExpectedQuote) > 0)
			{
				if (CurrentBookIsSingleVoice)
					return false;

				if (block.IsQuote || CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId))
					return false;

				IEnumerable<BCVRef> versesWithPotentialMissingQuote =
					ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber, block.ChapterNumber, block.InitialStartVerseNumber,
					block.LastVerseNum, versification: Versification).Where(c => c.IsExpected).Select(c => c.BcvRef);

				var withPotentialMissingQuote = versesWithPotentialMissingQuote as IList<BCVRef> ?? versesWithPotentialMissingQuote.ToList();
				if (!withPotentialMissingQuote.Any())
					return false;

				// REVIEW: This method peeks forward/backward from the *CURRENT* block, which might not be the block passed in to this method.
				return CurrentBlockHasMissingExpectedQuote(withPotentialMissingQuote);
			}
			if ((Mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) > 0)
			{
				if (!block.IsQuote || CurrentBookIsSingleVoice)
					return false;

				var expectedSpeakers = ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber, block.ChapterNumber, block.InitialStartVerseNumber,
					block.InitialEndVerseNumber, versification: Versification).Distinct(new CharacterEqualityComparer()).Count();

				var actualquotes = 1; // this is the quote represented by the given block.

				if (actualquotes > expectedSpeakers)
					return true;

				// REVIEW: This method peeks forward/backward from the *CURRENT* block, which might not be the block passed in to this method.
				// Check surrounding blocks to count quote blocks for same verse.
				actualquotes += BlockAccessor.GetPreviousBlocksWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber).Count(b => b.IsQuoteStart);

				if (actualquotes > expectedSpeakers)
					return true;

				actualquotes += BlockAccessor.GetNextBlocksWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber).Count(b => b.IsQuoteStart);

				return (actualquotes > expectedSpeakers);
			}
			if ((Mode & BlocksToDisplay.AllScripture) > 0)
				return block.IsScripture;
			if ((Mode & BlocksToDisplay.AllQuotes) > 0)
				return block.IsQuote;
			return false;
		}

		private bool IsBlockInVerseWithExpectedQuote(Block block)
		{
			if (!block.IsScripture)
				return false;
			return ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookNumber, block.ChapterNumber,
				block.InitialStartVerseNumber,
				block.LastVerseNum, versification: Versification).Any(c => c.IsExpected);
		}

		private bool BlockNotAssignedAutomatically(Block block)
		{
			if (CurrentBookIsSingleVoice)
				return false;

			return (block.UserConfirmed || block.CharacterIsUnclear());
		}

		internal bool CurrentBlockHasMissingExpectedQuote(IEnumerable<BCVRef> versesWithPotentialMissingQuote)
		{
			foreach (var verse in versesWithPotentialMissingQuote)
			{
				if (BlockAccessor.GetPreviousBlocksWithinBookWhile(b => PeekBackwardBlocksMatch(b, verse)).All(b => !b.IsQuote) &&
					BlockAccessor.GetNextBlocksWithinBookWhile(b => PeekForwardBlocksMatch(b, verse)).All(b => !b.IsQuote))
					return true;
			}
			return false;
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
							m_relevantBookBlockIndices[m_currentRelevantIndex].MultiBlockCount++;
						return;
					}
					// else - This can happen when using a filter (e.g., All Scripture) that does not make use
					// of MultiBlockCount to track with the block matchups. In that case, we want to fall through
					// in order do the simple addition of this block to relevant blocks if appropriate.
				}
				else
					return;
			}
			if (IsRelevant(newOrModifiedBlock, true))
			{
				var indicesOfNewOrModifiedBlock = GetBlockIndices(newOrModifiedBlock);
				m_relevantBookBlockIndices.Add(indicesOfNewOrModifiedBlock);
				m_relevantBookBlockIndices.Sort();
				RelevantBlockAdded(newOrModifiedBlock);
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
