using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Utilities;
using Paratext;
using SIL.Extensions;
using SIL.Scripture;
using ScrVers = Paratext.ScrVers;

namespace Glyssen.Dialogs
{
	[Flags]
	public enum BlocksToDisplay
	{
		Unexpected = 1,
		Ambiguous = 2,
		MissingExpectedQuote = 4,
		MoreQuotesThanExpectedSpeakers = 8,
		KnownTroubleSpots = 16,
		AllScripture = 32, // If this bit is set, ignore everything else (except Exclude user-confirmed)- show all editable (i.e., Scripture) blocks
		AllExpectedQuotes = 64,
		ExcludeUserConfirmed = 128,
		AllQuotes = 256,
		NotAlignedToReferenceText = 512,
		NeedAssignments = Unexpected | Ambiguous,
		HotSpots = MissingExpectedQuote | MoreQuotesThanExpectedSpeakers | KnownTroubleSpots,
	}

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
		protected List<BookBlockIndices> m_relevantBlocks;
		protected BookBlockIndices m_temporarilyIncludedBlock;
		private static readonly BookBlockTupleComparer s_bookBlockComparer = new BookBlockTupleComparer();
		private int m_currentBlockIndex = -1;
		private BlocksToDisplay m_mode;
		private BlockMatchup m_currentRefBlockMatchups;
		private bool m_attemptRefBlockMatchup;

		public event EventHandler UiFontSizeChanged;
		public event EventHandler CurrentBlockChanged;
		public event EventHandler CurrentBlockMatchupChanged;
		public event EventHandler FilterReset;

		protected BookScript CurrentBook { get { return m_navigator.CurrentBook; } }

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode = BlocksToDisplay.AllScripture, ProjectSettingsViewModel settingsViewModel = null)
			: this(project, mode, null, settingsViewModel)
		{
		}

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode, BookBlockIndices startingIndices, ProjectSettingsViewModel settingsViewModel = null)
		{
			m_project = project;
			m_project.QuoteParseCompleted += HandleProjectQuoteParseCompleted;

			m_navigator = new BlockNavigator(m_project.IncludedBooks);

			m_includedBooks = project.IncludedBooks.Select(b => b.BookId);
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

			if (startingIndices != null && !startingIndices.IsUndefined)
			{
				SetBlock(startingIndices);
				m_currentBlockIndex = m_relevantBlocks.IndexOf(startingIndices);
				if (m_currentBlockIndex < 0)
					m_temporarilyIncludedBlock = startingIndices;
			}
		}

		void HandleProjectQuoteParseCompleted(object sender, EventArgs e)
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
				var actualCount = m_navigator.CurrentBook.GetScriptBlocks().Count;
				var adjustment = BlockGroupingStyle == BlockGroupingType.BlockCorrelation ? m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting : 0;
				return actualCount + adjustment;
			}
		}
		public int RelevantBlockCount { get { return m_relevantBlocks.Count; } }
		public int CurrentBlockDisplayIndex
		{
			get
			{

				// If we're in block matchup mode and the current matchup group covers the last relevant block, then make display index
				// show as if we're on that very last block so it won't be confusing to the user why they can't click the Next button.
				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation &&
					m_currentBlockIndex >= 0 &&
					m_currentBlockIndex < RelevantBlockCount - 1 &&
					m_relevantBlocks[m_currentBlockIndex].BookIndex == m_relevantBlocks.Last().BookIndex &&
					m_relevantBlocks.Skip(m_currentBlockIndex + 1).All(i => m_currentRefBlockMatchups.OriginalBlocks.Contains(CurrentBook.GetScriptBlocks(false)[i.BlockIndex])))
				{
					return RelevantBlockCount;
				}
				return m_currentBlockIndex + 1;
			}
		}
		public string CurrentBookId { get { return m_navigator.CurrentBook.BookId; } }
		public bool CurrentBookIsSingleVoice { get { return m_navigator.CurrentBook.SingleVoice; } }
		public Block CurrentBlock
		{
			get
			{
				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation && m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting != 0)
					return m_currentRefBlockMatchups.CorrelatedAnchorBlock;
				return m_navigator.CurrentBlock;
			}
		}
		protected Block CurrentBlockInOriginal { get { return m_navigator.CurrentBlock; } }
		public BlockMatchup CurrentReferenceTextMatchup { get { return m_currentRefBlockMatchups; } }
		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }
		public string ProjectName { get { return m_project.Name; } }
		public BlockGroupingType BlockGroupingStyle { get; set; }
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

		public bool IsCurrentBlockRelevant
		{
			get
			{
				BookBlockIndices indices = m_navigator.GetIndices();
				return m_relevantBlocks.Contains(indices);
			}
		}

		public IEnumerable<string> IncludedBooks { get { return m_includedBooks; } }
		public FontProxy Font { get { return m_font; } }
		public FontProxy PrimaryReferenceTextFont { get { return m_referenceTextFonts[m_project.ReferenceText]; } }
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
			get
			{
				return m_navigator.GetIndices().BlockIndex;
			}
			set
			{
				int index = value;
				var bookIndex = m_navigator.GetIndices().BookIndex;

				if (BlockGroupingStyle == BlockGroupingType.BlockCorrelation && index >= m_currentRefBlockMatchups.IndexOfStartBlockInBook)
				{
					if (index >= m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count)
					{
						// Adjust index to account for any temporary additions resulting from splitting blocks in the matchup.
						index -= m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting;
					}
					else
					{
						// A block within the existing matchup has been selected, so we need to translate the index to find the
						// correct block within the sequence of correlated blocks rather than within the book.
						index -= m_currentRefBlockMatchups.IndexOfStartBlockInBook;
						var newAnchorBlock = m_currentRefBlockMatchups.CorrelatedBlocks[index];
						if (newAnchorBlock != m_currentRefBlockMatchups.CorrelatedAnchorBlock)
						{
							// Just reset the anchor and get out.
							m_currentRefBlockMatchups.ChangeAnchor(newAnchorBlock);
							//var correspondingOrigBlock = m_currentRefBlockMatchups.GetCorrespondingOriginalBlock(newAnchorBlock);
							//if (newAnchorBlock != null)
							//{
							var relevantBlockIndex = m_relevantBlocks.IndexOf(new BookBlockIndices(bookIndex, value));
							if (relevantBlockIndex >= 0)
								m_currentBlockIndex = relevantBlockIndex;
							//}
							HandleCurrentBlockChanged();
						}
						return;
					}
				}

				Block b;
				do
				{
					b = m_navigator.CurrentBook.GetScriptBlocks()[index];
				} while ((b.MultiBlockQuote == MultiBlockQuote.Continuation || b.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery) && --index >= 0);
				Debug.Assert(index >= 0);
				var location = new BookBlockIndices(bookIndex, index);
				m_currentBlockIndex = m_relevantBlocks.IndexOf(location);
				m_temporarilyIncludedBlock = m_currentBlockIndex < 0 ? location : null;
				SetBlock(location);
			}
		}

		public virtual BlocksToDisplay Mode
		{
			get { return m_mode; }
			set
			{
				if (m_mode == value)
					return;

				m_mode = value;
				m_temporarilyIncludedBlock = GetCurrentBlockIndices();
				ResetFilter(m_navigator.CurrentBlock);
			}
		}

		protected void ResetFilter(Block selectedBlock)
		{
			PopulateRelevantBlocks();

			if (RelevantBlockCount > 0)
			{
				m_currentBlockIndex = -1;
				if (m_temporarilyIncludedBlock != null)
				{
					// Block that was temporarily included in previous filter might now match the new filter
					var i = m_relevantBlocks.IndexOf(m_temporarilyIncludedBlock);
					if (i >= 0)
					{
						m_currentBlockIndex = i;
						m_temporarilyIncludedBlock = null;
						SetBlock(m_relevantBlocks[m_currentBlockIndex]);
						return;
					}
				}
				LoadNextRelevantBlock();
			}
			else if (selectedBlock != null)
			{
				m_temporarilyIncludedBlock = m_navigator.GetIndicesOfSpecificBlock(selectedBlock);
				m_navigator.SetIndices(m_temporarilyIncludedBlock);
			}
			else
			{
				m_temporarilyIncludedBlock = m_navigator.GetIndices();
			}

			if (FilterReset != null)
				FilterReset(this, new EventArgs());
		}
		#endregion

		#region Context blocks
		protected IEnumerable<Block> ContextBlocksBackward
		{
			get { return m_navigator.PeekBackwardWithinBook(BackwardContextBlockCount); }
		}

		protected IEnumerable<Block> ContextBlocksForward
		{
			get { return m_navigator.PeekForwardWithinBook(ForwardContextBlockCount); }
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
			bldr.Append(" data-block-index-in-book=\"").Append(m_navigator.GetIndicesOfSpecificBlock(block).BlockIndex).Append("\"");
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
				if (block.MultiBlockQuote != MultiBlockQuote.Continuation && block.MultiBlockQuote != MultiBlockQuote.ChangeOfDelivery)
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
						yield return m_navigator.CurrentBook[i];
					break;
				case MultiBlockQuote.Continuation:
				case MultiBlockQuote.ChangeOfDelivery:
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

			for (int j = m_navigator.GetIndicesOfSpecificBlock(startQuoteBlock).BlockIndex + 1; j < BlockCountForCurrentBook; j++)
			{
				Block block = m_navigator.CurrentBook[j];
				if (block == null || (block.MultiBlockQuote != MultiBlockQuote.Continuation && block.MultiBlockQuote != MultiBlockQuote.ChangeOfDelivery))
					break;
				yield return j;
			}
		}

		public int IndexOfFirstBlockInCurrentGroup
		{
			get
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					return m_navigator.GetIndicesOfSpecificBlock(CurrentBlock).BlockIndex;
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
						GetIndicesOfQuoteContinuationBlocks(CurrentBlock).Last();
					return IndexOfFirstBlockInCurrentGroup;
				}
				return m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count - 1;
			}
		}
		#endregion

		#region GetBlockReference
		public string GetBlockReferenceString(Block block = null)
		{
			block = block ?? m_navigator.CurrentBlock;
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
			block = block ?? m_navigator.CurrentBlock;
			var verseRef =  new VerseRef(BCVRef.BookToNumber(CurrentBookId), block.ChapterNumber, block.InitialStartVerseNumber, Versification);
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

			var lastBlock = m_navigator.PeekForwardWithinBookWhile(b => b.MultiBlockQuote != MultiBlockQuote.None && b.MultiBlockQuote != MultiBlockQuote.Start).LastOrDefault();
			return lastBlock ?? CurrentBlock;
		}
		#endregion

		#region Navigation methods
		public Block GetNthBlockInCurrentBook(int i)
		{
			if (BlockGroupingStyle != BlockGroupingType.BlockCorrelation)
			{
				return m_navigator.CurrentBook.GetScriptBlocks()[i];
			}
			if (m_currentRefBlockMatchups.IndexOfStartBlockInBook > i)
			{
				return m_navigator.CurrentBook.GetScriptBlocks()[i];
			}
			if (i < m_currentRefBlockMatchups.IndexOfStartBlockInBook + m_currentRefBlockMatchups.CorrelatedBlocks.Count)
			{
				return m_currentRefBlockMatchups.CorrelatedBlocks[i - m_currentRefBlockMatchups.IndexOfStartBlockInBook];
			}
			return m_navigator.CurrentBook.GetScriptBlocks()[i - m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting];
		}

		public bool CanNavigateToPreviousRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentBlockRelevant)
				{
					if (m_currentBlockIndex == 0)
						return false;
					if (BlockGroupingStyle == BlockGroupingType.Quote)
						return true;
					return GetIndexOfPreviousRelevantBlockNotInCurrentMatchup() >= 0;
				}

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block before it.
				var firstRelevantBlock = m_relevantBlocks[0];
				var indicesOfCurrentLocation = m_temporarilyIncludedBlock ?? m_navigator.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.First());
				return s_bookBlockComparer.Compare(firstRelevantBlock, indicesOfCurrentLocation) < 0;
			}
		}

		public bool CanNavigateToNextRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentBlockRelevant)
				{
					if (m_currentBlockIndex == RelevantBlockCount - 1)
						return false;
					if (BlockGroupingStyle == BlockGroupingType.Quote)
						return true;
					return GetIndexOfNextRelevantBlockNotInCurrentMatchup() > m_currentBlockIndex;
				}

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block after it.
				var indicesOfCurrentLocation = m_currentRefBlockMatchups == null ? m_temporarilyIncludedBlock :
					m_navigator.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.Last());
				return s_bookBlockComparer.Compare(m_relevantBlocks.Last(), indicesOfCurrentLocation) > 0;
			}
		}

		public bool TryLoadBlock(VerseRef verseRef)
		{
			var indices = m_navigator.GetIndicesOfFirstBlockAtReference(verseRef);
			if (indices == null)
				return false;
			m_currentBlockIndex = m_relevantBlocks.IndexOf(indices);
			m_temporarilyIncludedBlock = m_currentBlockIndex < 0 ? indices : null;
			SetBlock(indices);
			return true;
		}

		// Internal for testing
		protected internal void LoadNextRelevantBlockInSubsequentBook()
		{
			if (!CanNavigateToNextRelevantBlock)
				return;
			if (m_navigator.IsLastBook(CurrentBook))
				return;

			var currentBookIndex = m_navigator.GetIndicesOfSpecificBlock(m_navigator.CurrentBlock).BookIndex;

			var blockIndex = (IsCurrentBlockRelevant) ? m_currentBlockIndex + 1 :
				GetIndexOfClosestRelevantBlock(m_relevantBlocks, m_temporarilyIncludedBlock, false, 0, RelevantBlockCount - 1);

			var bookBlockIndices = m_relevantBlocks[blockIndex];
			while (bookBlockIndices.BookIndex == currentBookIndex)
			{
				if (++blockIndex >= m_relevantBlocks.Count)
					return;
				bookBlockIndices = m_relevantBlocks[blockIndex];
			}

			m_currentBlockIndex = blockIndex;
			SetBlock(bookBlockIndices);
		}

		public void LoadNextRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					m_currentBlockIndex++;
				else
					m_currentBlockIndex = GetIndexOfNextRelevantBlockNotInCurrentMatchup();
				SetBlock(m_relevantBlocks[m_currentBlockIndex]);
			}
			else
				LoadClosestRelevantBlock(false);
		}

		private int GetIndexOfPreviousRelevantBlockNotInCurrentMatchup()
		{
			for (int i = m_currentBlockIndex - 1; i >= 0; i--)
			{
				if (m_relevantBlocks[i].BookIndex != m_relevantBlocks[m_currentBlockIndex].BookIndex ||
					!m_currentRefBlockMatchups.OriginalBlocks.Contains(CurrentBook.GetScriptBlocks(false)[m_relevantBlocks[i].BlockIndex]))
				{
					return i;
				}
			}
			return -1;
		}

		private int GetIndexOfNextRelevantBlockNotInCurrentMatchup()
		{
			for (int i = m_currentBlockIndex + 1; i < RelevantBlockCount; i++)
			{
				if (m_relevantBlocks[i].BookIndex != m_relevantBlocks[m_currentBlockIndex].BookIndex ||
					!m_currentRefBlockMatchups.OriginalBlocks.Contains(
						CurrentBook.GetScriptBlocks(false)[m_relevantBlocks[i].BlockIndex]))
				{
					return i;
				}
			}
			return -1;
		}

		public void LoadPreviousRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
			{
				if (BlockGroupingStyle == BlockGroupingType.Quote)
					m_currentBlockIndex--;
				else
					m_currentBlockIndex = GetIndexOfPreviousRelevantBlockNotInCurrentMatchup();
				SetBlock(m_relevantBlocks[m_currentBlockIndex]);
			}
			else
				LoadClosestRelevantBlock(true);
		}

		private void LoadClosestRelevantBlock(bool prev)
		{
			if (m_temporarilyIncludedBlock == null)
				m_currentBlockIndex = 0;
			else
			{
				m_currentBlockIndex = GetIndexOfClosestRelevantBlock(m_relevantBlocks, m_temporarilyIncludedBlock, prev, 0, RelevantBlockCount - 1);
				m_temporarilyIncludedBlock = null;
			}

			if (m_currentBlockIndex < 0)
				m_currentBlockIndex = 0;

			SetBlock(m_relevantBlocks[m_currentBlockIndex]);
		}

		private void SetBlock(BookBlockIndices indices, bool clearBlockMatchup = true)
		{
			if (clearBlockMatchup)
				ClearBlockMatchup();
			m_navigator.SetIndices(indices);
			HandleCurrentBlockChanged();
		}

		public void SetBlockMatchupForCurrentVerse()
		{
			if (!AttemptRefBlockMatchup || CurrentBook.SingleVoice)
				return;

			var origValue = m_currentRefBlockMatchups;

			m_currentRefBlockMatchups = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook,
				CurrentBlockIndexInBook, m_project.Versification);
			if (m_currentRefBlockMatchups != null)
			{
				m_currentRefBlockMatchups.MatchAllBlocks(m_project.Versification);
				// REVIEW: We might want to keep track of which style the user prefers.
				BlockGroupingStyle = BlockGroupingType.BlockCorrelation;
			}
			if (CurrentBlockMatchupChanged != null && origValue != m_currentRefBlockMatchups)
				CurrentBlockMatchupChanged(this, new EventArgs());
		}

		public void ClearBlockMatchup()
		{
			if (m_currentRefBlockMatchups == null)
				return;
			BlockGroupingStyle = BlockGroupingType.Quote;
			var relevant = IsCurrentBlockRelevant;
			m_currentRefBlockMatchups = null;
			if (!relevant)
				m_temporarilyIncludedBlock = GetCurrentBlockIndices();
			if (CurrentBlockMatchupChanged != null)
				CurrentBlockMatchupChanged(this, new EventArgs());
		}

		public virtual void ApplyCurrentReferenceTextMatchup()
		{
			if (BlockGroupingStyle != BlockGroupingType.BlockCorrelation)
				throw new InvalidOperationException("No current reference text block matchup!");
			Debug.Assert(m_currentRefBlockMatchups != null);
			if (!m_currentRefBlockMatchups.HasOutstandingChangesToApply)
				throw new InvalidOperationException("Current reference text block matchup has no outstanding changes!");
			var insertions = m_currentRefBlockMatchups.CountOfBlocksAddedBySplitting;
			var insertionIndex = m_currentBlockIndex;
			foreach (var block in m_currentRefBlockMatchups.OriginalBlocks)
				m_relevantBlocks.Remove(m_navigator.GetIndicesOfSpecificBlock(block));
			m_currentRefBlockMatchups.Apply(m_project.Versification);
			if (insertionIndex < 0)
			{
				var indicesOfFirstBlock = m_navigator.GetIndicesOfSpecificBlock(m_currentRefBlockMatchups.OriginalBlocks.First());
				insertionIndex = GetIndexOfClosestRelevantBlock(m_relevantBlocks, indicesOfFirstBlock, false, 0, m_relevantBlocks.Count - 1);
				if (insertionIndex == -1)
					insertionIndex = m_relevantBlocks.Count;
			}
			var origRelevantBlockCount = RelevantBlockCount;
			m_relevantBlocks.InsertRange(insertionIndex,
				m_currentRefBlockMatchups.OriginalBlocks.Where(b => IsRelevant(b, true)).Select(b => m_navigator.GetIndicesOfSpecificBlock(b)));
			// Insertions before the anchor block can mess up m_currentBlockIndex, so we need to reset it to point to the newly inserted
			// block that corresponds to the "anchor" block. Since the "OriginalBlocks" is not a cloned copy of the "CorrelatedBlocks",
			// We can safely use the index of the anchor block in CorrelatedBlocks to find the correct block in OriginalBlocks.
			var originalAnchorBlock = m_currentRefBlockMatchups.OriginalBlocks.ElementAt(m_currentRefBlockMatchups.CorrelatedBlocks.IndexOf(m_currentRefBlockMatchups.CorrelatedAnchorBlock));
			SetBlock(m_navigator.GetIndicesOfSpecificBlock(originalAnchorBlock), false);
			var currentBookIndex = m_navigator.GetIndices().BookIndex;
			for (int i = insertionIndex + RelevantBlockCount - origRelevantBlockCount; i < RelevantBlockCount && m_relevantBlocks[i].BookIndex == currentBookIndex; i++)
				m_relevantBlocks[i].BlockIndex += insertions;
		}

		protected virtual void HandleCurrentBlockChanged()
		{
			if (CurrentBlockChanged != null)
				CurrentBlockChanged(this, new EventArgs());
			StoreCurrentBlockIndices();
		}

		protected virtual void StoreCurrentBlockIndices()
		{
		}

		protected BookBlockIndices GetCurrentBlockIndices()
		{
			return m_navigator.GetIndices();
		}

		internal BookBlockIndices GetBlockIndices(Block block)
		{
			return m_navigator.GetIndicesOfSpecificBlock(block);
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
			m_navigator.NavigateToFirstBlock();
			m_relevantBlocks = new List<BookBlockIndices>();
			Block block = m_navigator.CurrentBlock;
			for (; ; )
			{
				if (IsRelevant(block))
				{
					m_relevantBlocks.Add(m_navigator.GetIndices());
					RelevantBlockAdded(block);
				}
				if (m_navigator.IsLastBlock())
					break;
				block = m_navigator.NextBlock();
			}

			m_navigator.NavigateToFirstBlock();
		}

		protected virtual void RelevantBlockAdded(Block block)
		{
			// No-op in base class
		}

		private static BlockMatchup s_lastMatchup = null;

		private bool IsRelevant(Block block, bool ignoreExcludeUserConfirmed = false)
		{
			if (block.MultiBlockQuote == MultiBlockQuote.Continuation || block.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery)
				return false;
			if (!ignoreExcludeUserConfirmed && (Mode & BlocksToDisplay.ExcludeUserConfirmed) > 0 && block.UserConfirmed)
				return false;
			if ((Mode & BlocksToDisplay.NeedAssignments) > 0)
				return BlockNeedsAssignment(block);
			if ((Mode & BlocksToDisplay.NotAlignedToReferenceText) > 0)
			{
				if (!block.IsScripture)
					return false;

				if (s_lastMatchup == null || !s_lastMatchup.OriginalBlocks.Contains(block))
				{
					s_lastMatchup = m_project.ReferenceText.GetBlocksForVerseMatchedToReferenceText(CurrentBook,
						m_navigator.GetIndicesOfSpecificBlock(block).BlockIndex, m_project.Versification);
				}
				return s_lastMatchup.OriginalBlocks.Count() > 1 && !s_lastMatchup.CorrelatedBlocks.All(b => b.MatchesReferenceText);
			}
			if ((Mode & BlocksToDisplay.AllExpectedQuotes) > 0)
				return IsBlockInVerseWithExpectedQuote(block);
			if ((Mode & BlocksToDisplay.MissingExpectedQuote) > 0)
			{
				if (CurrentBookIsSingleVoice)
					return false;

				if (block.IsQuote || CharacterVerseData.IsCharacterExtraBiblical(block.CharacterId))
					return false;

				IEnumerable<BCVRef> versesWithPotentialMissingQuote =
					ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookId, block.ChapterNumber, block.InitialStartVerseNumber,
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

				var expectedSpeakers = ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookId, block.ChapterNumber, block.InitialStartVerseNumber,
					block.InitialEndVerseNumber, versification: Versification).Distinct(new CharacterEqualityComparer()).Count();

				var actualquotes = 1; // this is the quote represented by the given block.

				if (actualquotes > expectedSpeakers)
					return true;

				// REVIEW: This method peeks forward/backward from the *CURRENT* block, which might not be the block passed in to this method. 
				// Check surrounding blocks to count quote blocks for same verse.
				actualquotes += m_navigator.PeekBackwardWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber)
					.Count(b => b.IsQuote && (b.MultiBlockQuote == MultiBlockQuote.Start || b.MultiBlockQuote == MultiBlockQuote.None));

				if (actualquotes > expectedSpeakers)
					return true;

				actualquotes += m_navigator.PeekForwardWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber)
					.Count(b => b.IsQuote && (b.MultiBlockQuote == MultiBlockQuote.Start || b.MultiBlockQuote == MultiBlockQuote.None));

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
			return ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookId, block.ChapterNumber,
				block.InitialStartVerseNumber,
				block.LastVerseNum, versification: Versification).Any(c => c.IsExpected);
		}

		private bool BlockNeedsAssignment(Block block)
		{
			if (CurrentBookIsSingleVoice)
				return false;

			return (block.UserConfirmed || block.CharacterIsUnclear());
		}

		internal bool CurrentBlockHasMissingExpectedQuote(IEnumerable<BCVRef> versesWithPotentialMissingQuote)
		{
			foreach (var verse in versesWithPotentialMissingQuote)
			{
				if (m_navigator.PeekBackwardWithinBookWhile(b => PeekBackwardBlocksMatch(b, verse)).All(b => !b.IsQuote) &&
					m_navigator.PeekForwardWithinBookWhile(b => PeekForwardBlocksMatch(b, verse)).All(b => !b.IsQuote))
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

		protected void AddToRelevantBlocksIfNeeded(Block newOrModifiedBlock)
		{
			if (IsRelevant(newOrModifiedBlock, true))
			{
				var indicesOfNewOrModifiedBlock = GetBlockIndices(newOrModifiedBlock);
				m_relevantBlocks.Add(indicesOfNewOrModifiedBlock);
				m_relevantBlocks.Sort();
				RelevantBlockAdded(newOrModifiedBlock);
				if (m_currentBlockIndex == -1 && CurrentBlock.Equals(newOrModifiedBlock))
					m_currentBlockIndex = m_relevantBlocks.IndexOf(indicesOfNewOrModifiedBlock);
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
