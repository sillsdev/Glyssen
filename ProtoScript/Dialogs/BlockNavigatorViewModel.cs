using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using Paratext;
using Paratext.Users;
using ProtoScript.Character;
using SIL.ScriptureUtils;
using ScrVers = Paratext.ScrVers;


namespace ProtoScript.Dialogs
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
		NeedAssignments = Unexpected | Ambiguous,
		HotSpots = MissingExpectedQuote | MoreQuotesThanExpectedSpeakers | KnownTroubleSpots,
	}

	public class BlockNavigatorViewModel : IDisposable
	{
		private readonly Project m_project;
		internal const string kDataCharacter = "data-character";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body {1}>{2}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		internal const string kCssClassContext = "context";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
										".highlight{{background-color:yellow}}" +
										"." + kCssClassContext + ":hover{{background-color:#FFFFA0}}" +
										".block-spacer{{height:30px}}" +
										".right-to-left{{direction:rtl}}" +
										".section-header{{text-align:center;font-weight:bold}}" +
										".chapter-label{{font-weight:bold;font-size:150%}}";
		internal const string kMainQuoteElementId = "main-quote-text";

		private bool m_showVerseNumbers = true; // May make this configurable later
		private Font m_font;
		private readonly string m_fontFamily;
		private readonly int m_baseFontSizeInPoints;
		private int m_fontSizeUiAdjustment;
		private readonly bool m_rightToLeftScript;
		private readonly BlockNavigator m_navigator;
		private readonly IEnumerable<string> m_includedBooks;
		protected List<Tuple<int, int>> m_relevantBlocks;
		private Tuple<int, int> m_temporarilyIncludedBlock;
		private static readonly BookBlockTupleComparer s_bookBlockComparer = new BookBlockTupleComparer();
		private int m_currentBlockIndex = -1;
		private BlocksToDisplay m_mode;

		public event EventHandler UiFontSizeChanged;
		public event EventHandler CurrentBlockChanged;

		public BlockNavigatorViewModel(Project project, BlocksToDisplay mode = BlocksToDisplay.AllScripture)
		{
			m_project = project;
			m_navigator = new BlockNavigator(project.IncludedBooks);
			m_includedBooks = project.IncludedBooks.Select(b => b.BookId);
			m_fontFamily = project.FontFamily;
			m_baseFontSizeInPoints = project.FontSizeInPoints;
			FontSizeUiAdjustment = project.FontSizeUiAdjustment;
			m_rightToLeftScript = project.RightToLeftScript;
			Versification = project.Versification;

			Mode = mode;
		}

		#region IDisposable Members
		public void Dispose()
		{
			m_project.FontSizeUiAdjustment = m_fontSizeUiAdjustment;

			if (m_font != null)
			{
				m_font.Dispose();
				m_font = null;
			}
		}
		#endregion

		#region Public properties
		public ScrVers Versification { get; private set; }
		public int BlockCountForCurrentBook { get { return m_navigator.CurrentBook.GetScriptBlocks().Count; } }
		public int RelevantBlockCount { get { return m_relevantBlocks.Count; } }
		public int CurrentBlockDisplayIndex { get { return m_currentBlockIndex + 1; } }
		public string CurrentBookId { get { return m_navigator.CurrentBook.BookId; } }
		public Block CurrentBlock { get { return m_navigator.CurrentBlock; } }
		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }
		public bool IsCurrentBlockRelevant { get { return m_temporarilyIncludedBlock == null; } }
		public IEnumerable<string> IncludedBooks { get { return m_includedBooks; } }
		public bool RightToLeft { get { return m_rightToLeftScript; } }
		public Font Font { get { return m_font; } }
		public int FontSizeUiAdjustment
		{
			get { return m_fontSizeUiAdjustment; }
			set
			{
				if (m_font != null)
					m_font.Dispose();
				m_fontSizeUiAdjustment = value;
				m_font = new Font(m_fontFamily, m_baseFontSizeInPoints + m_fontSizeUiAdjustment);

				if (UiFontSizeChanged != null)
					UiFontSizeChanged(this, new EventArgs());
			}
		}

		public int CurrentBlockIndexInBook
		{
			get
			{
				return m_navigator.GetIndices().Item2;
			}
			set
			{
				int index = value;
				Tuple<int, int> location;
				var bookIndex = m_navigator.GetIndices().Item1;
				do
				{
					location = new Tuple<int, int>(bookIndex, index);
					m_navigator.SetIndices(location);
				} while (CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation && --index >= 0);
				Debug.Assert(index >= 0);
				m_currentBlockIndex = m_relevantBlocks.IndexOf(location);
				m_temporarilyIncludedBlock = m_currentBlockIndex < 0 ? location : null;
				if (CurrentBlockChanged != null)
					CurrentBlockChanged(this, new EventArgs());
			}
		}

		public BlocksToDisplay Mode
		{
			get { return m_mode; }
			set
			{
				if (m_mode == value)
					return;

				m_mode = value;

				PopulateRelevantBlocks();

				if (IsRelevant(m_navigator.CurrentBlock))
					m_currentBlockIndex = 0;
				else if (RelevantBlockCount > 0)
				{
					m_currentBlockIndex = -1;
					LoadNextRelevantBlock();
				}
			}
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
			bldr.Append(SuperscriptVerseNumbers(mainText));
			bldr.Append("</div>");
			if (!String.IsNullOrEmpty(followingText))
				bldr.Append(kHtmlLineBreak).Append(followingText);
			var bodyAttributes = m_rightToLeftScript ? "class=\"right-to-left\"" : "";
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
			string text = SuperscriptVerseNumbers(HttpUtility.HtmlEncode(block.GetText(m_showVerseNumbers)));
			var bldr = new StringBuilder();
			bldr.Append("<div");
			if (block.StyleTag.StartsWith("s"))
				bldr.Append(" class=\"section-header\"");
			else if (block.StyleTag.StartsWith("c"))
				bldr.Append(" class=\"chapter-label\"");
			bldr.Append(">");
			bldr.Append(text);
			bldr.Append("</div>");
			return bldr.ToString();
		}

		private string BuildCurrentBlockHtml()
		{
			return BuildHtml(GetAllBlocksWithSameQuote(m_navigator.CurrentBlock));
		}

		private string BuildContextBlocksHtml(IEnumerable<Block> blocks)
		{
			var bldr = new StringBuilder();
			foreach (Block block in blocks)
			{
				bldr.Append("<div class='").Append(kCssClassContext).Append("' ").Append(kDataCharacter).Append("='").Append(block.CharacterId).Append("'>");
				foreach (Block innerBlock in GetAllBlocksWithSameQuote(block))
					bldr.Append(BuildHtml(innerBlock));
				bldr.Append("</div>");
				if (block.MultiBlockQuote != MultiBlockQuote.Continuation)
					bldr.Append(kHtmlLineBreak);
			}
			return bldr.ToString();
		}

		private string SuperscriptVerseNumbers(string text)
		{
			return text.Replace("[", "<sup>").Replace("]", "</sup>");
		}

		private string BuildStyle()
		{
			return String.Format(kCssFrame, m_fontFamily, m_baseFontSizeInPoints + m_fontSizeUiAdjustment);
		}
		#endregion

		#region Methods for dealing with multi-block quotes
		protected IEnumerable<Block> GetAllBlocksWithSameQuote(Block baseLineBlock)
		{
			switch (baseLineBlock.MultiBlockQuote)
			{
				case MultiBlockQuote.Start:
					yield return baseLineBlock;
					foreach (var i in GetIndicesOfQuoteContinuationBlocks(baseLineBlock))
						yield return m_navigator.CurrentBook[i];
					break;
				case MultiBlockQuote.Continuation:
					// These should all be brought in through a Start block, so don't do anything with them here
					break;
				default:
					// Not part of a multi-block quote. Just return the base-line block
					yield return baseLineBlock;
					break;
			}
		}

		public IEnumerable<int> GetIndicesOfQuoteContinuationBlocks(Block startQuoteBlock)
		{
			// Note this method assumes the startQuoteBlock is in the navigator's current book.
			Debug.Assert(startQuoteBlock.MultiBlockQuote == MultiBlockQuote.Start);

			for (int j = m_navigator.GetIndicesOfSpecificBlock(startQuoteBlock).Item2 + 1; ; j++)
			{
				Block block = m_navigator.CurrentBook[j];
				if (block == null || block.MultiBlockQuote != MultiBlockQuote.Continuation)
					break;
				yield return j;
			}
		}
		#endregion

		#region GetBlockReference
		public string GetBlockReferenceString(Block block = null)
		{
			block = block ?? m_navigator.CurrentBlock;
			var startRef = GetBlockReference(block);
			var lastVerseInBlock = block.LastVerse;
			var endRef = (lastVerseInBlock <= block.InitialStartVerseNumber) ? startRef :
				new BCVRef(startRef.Book, startRef.Chapter, lastVerseInBlock);
			return BCVRef.MakeReferenceString(startRef, endRef, ":", "-");
		}

		public BCVRef GetBlockReference(Block block)
		{
			return new BCVRef(BCVRef.BookToNumber(CurrentBookId), block.ChapterNumber, block.InitialStartVerseNumber);
		}
		#endregion

		#region Navigation methods
		public Block GetNthBlockInCurrentBook(int i)
		{
			return m_navigator.CurrentBook.GetScriptBlocks()[i];
		}

		public bool CanNavigateToPreviousRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentBlockRelevant)
					return m_currentBlockIndex != 0;

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block before it.
				var firstRelevantBlock = m_relevantBlocks[0];
				return s_bookBlockComparer.Compare(firstRelevantBlock, m_temporarilyIncludedBlock) < 0;
			}
		}

		public bool CanNavigateToNextRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentBlockRelevant)
					return m_currentBlockIndex != RelevantBlockCount - 1;

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block after it.
				var lastRelevantBlock = m_relevantBlocks.Last();
				return s_bookBlockComparer.Compare(lastRelevantBlock, m_temporarilyIncludedBlock) > 0;
			}
		}

		public bool TryLoadBlock(VerseRef verseRef)
		{
			var indices = m_navigator.GetIndicesOfFirstBlockAtReference(new BCVRef(verseRef.BBBCCCVVV));
			if (indices == null)
				return false;
			m_currentBlockIndex = m_relevantBlocks.IndexOf(indices);
			m_temporarilyIncludedBlock = m_currentBlockIndex < 0 ? indices : null;
			SetBlock(indices);
			return true;
		}

		public void LoadNextRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
				SetBlock(m_relevantBlocks[++m_currentBlockIndex]);
			else
				LoadClosestRelevantBlock(false);
		}

		public void LoadPreviousRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
				SetBlock(m_relevantBlocks[--m_currentBlockIndex]);
			else
				LoadClosestRelevantBlock(true);
		}

		private void LoadClosestRelevantBlock(bool prev)
		{
			m_currentBlockIndex = GetIndexOfClosestRelevantBlock(m_relevantBlocks, m_temporarilyIncludedBlock, prev, 0, RelevantBlockCount - 1);
			m_temporarilyIncludedBlock = null;
			SetBlock(m_relevantBlocks[m_currentBlockIndex]);
		}

		private void SetBlock(Tuple<int, int> indices)
		{
			m_navigator.SetIndices(indices);
			if (CurrentBlockChanged != null)
				CurrentBlockChanged(this, new EventArgs());
		}

		public static int GetIndexOfClosestRelevantBlock(List<Tuple<int, int>> list, Tuple<int, int> key, bool prev,
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
			m_relevantBlocks = new List<Tuple<int, int>>();
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

		protected  virtual void RelevantBlockAdded(Block block)
		{
			// No-op in base class
		}

		private bool IsRelevant(Block block)
		{
			if (block.MultiBlockQuote == MultiBlockQuote.Continuation)
				return false;
			if ((Mode & BlocksToDisplay.ExcludeUserConfirmed) > 0 && block.UserConfirmed)
				return false;
			if ((Mode & BlocksToDisplay.NeedAssignments) > 0)
				return (block.UserConfirmed || block.CharacterIsUnclear());
			if ((Mode & BlocksToDisplay.AllExpectedQuotes) > 0)
			{
				if (!GetIsBlockScripture(block))
					return false;
				return ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookId, block.ChapterNumber, block.InitialStartVerseNumber,
					block.LastVerse).Any(c => c.IsExpected);
			}
			if ((Mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) > 0)
			{
				if (!block.IsQuote)
					return false;

				var expectedSpeakers = ControlCharacterVerseData.Singleton.GetCharacters(CurrentBookId, block.ChapterNumber, block.InitialStartVerseNumber,
					block.InitialEndVerseNumber).Distinct(new CvCharacterIdComparer()).Count();

				var actualquotes = 1; // this is the quote represented by the given block.

				if (actualquotes > expectedSpeakers)
					return true;

				// Check surrounding blocks to count quote blocks for same verse.
				actualquotes += m_navigator.PeekBackwardWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber)
					.Count(b => b.IsQuote);

				if (actualquotes > expectedSpeakers)
					return true;

				actualquotes += m_navigator.PeekForwardWithinBookWhile(b => b.ChapterNumber == block.ChapterNumber &&
					b.InitialStartVerseNumber == block.InitialStartVerseNumber)
					.Count(b => b.IsQuote);

				return (actualquotes > expectedSpeakers);
			}
			if ((Mode & BlocksToDisplay.AllScripture) > 0)
				return GetIsBlockScripture(block);
			return false;
		}

		/// <summary>
		/// Gets whether the specified block represents Scripture text. (Only Scripture blocks can have their
		/// character/delivery changed. Book titles, chapters, and section heads have characters assigned
		/// programmatically and cannot be changed.)
		/// </summary>
		public bool GetIsBlockScripture(Block block)
		{
			return !CharacterVerseData.IsCharacterStandard(block.CharacterId, false);
		}

		/// <summary>
		/// Gets whether the specified block represents Scripture text. (Only Scripture blocks can have their
		/// character/delivery changed. Book titles, chapters, and section heads have characters assigned
		/// programmatically and cannot be changed.)
		/// </summary>
		public bool GetIsBlockScripture(int blockIndex)
		{
			return GetIsBlockScripture(GetNthBlockInCurrentBook(blockIndex));
		}
		#endregion
	}

	#region BookBlockTupleComparer
	public class BookBlockTupleComparer : IComparer<Tuple<int, int>>
	{
		public int Compare(Tuple<int, int> x, Tuple<int, int> y)
		{
			int item1Comparison = x.Item1.CompareTo(y.Item1);
			if (item1Comparison == 0)
			{
				return x.Item2.CompareTo(y.Item2);
			}
			return item1Comparison;
		}
	}
	#endregion
}
