using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoScript.Dialogs
{
	public class AssignCharacterViewModel
	{
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body>{1}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		private const string kCssClassContext = "context";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
										 ".highlight{{background-color:yellow}}" +
										 "." + kCssClassContext + ":hover{{background-color:#FFFFA0}}" +
										 ".block-spacer{{height:30px}}";

		private bool m_showVerseNumbers = true; // May make this configurable later
		private readonly string m_fontFamily;
		private readonly int m_fontSizeInPoints;
		private readonly BlockNavigator m_navigator;
		private List<Tuple<int, int>> m_relevantBlocks;
		private int m_displayBlockIndex = -1;

		private int m_assignedBlocks;
		private IEnumerable<Block> m_contextBlocksBackward;
		private IEnumerable<Block> m_contextBlocksForward;

		public event EventHandler AssignedBlocksIncremented;

		public AssignCharacterViewModel(Project project)
		{
			m_navigator = new BlockNavigator(project.IncludedBooks);
			m_fontFamily = project.FontFamily;
			m_fontSizeInPoints = project.FontSizeInPoints;

			PopulateRelevantBlocks();

			if (IsRelevant(m_navigator.CurrentBlock))
				m_displayBlockIndex = 0;
			else if (RelevantBlockCount > 0)
				LoadNextRelevantBlock();
		}

		public int RelevantBlockCount { get { return m_relevantBlocks.Count; } }
		public int AssignedBlockCount { get { return m_assignedBlocks; } }
		public int CurrentBlockDisplayIndex { get { return m_displayBlockIndex + 1; } }
		public string CurrentBookId { get { return m_navigator.CurrentBook.BookId; } }
		public Block CurrentBlock { get { return m_navigator.CurrentBlock; } }
		public IEnumerable<Block> ContextBlocks
		{
			get { return m_contextBlocksBackward.Union(m_contextBlocksForward); }
		}

		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }
		public string Narrator { get; set; }
		public string NormalDelivery { get; set; }

		public string Html
		{
			get
			{
				return BuildHtml(
					BuildHtml(m_contextBlocksBackward = m_navigator.PeekBackwardWithinBook(BackwardContextBlockCount)),
					m_navigator.CurrentBlock.GetText(m_showVerseNumbers),
					BuildHtml(m_contextBlocksForward = m_navigator.PeekForwardWithinBook(ForwardContextBlockCount)),
					BuildStyle());
			}
		}

		public bool IsFirstRelevantBlock
		{
			get { return m_displayBlockIndex == 0; }
		}

		public bool IsLastRelevantBlock
		{
			get { return m_displayBlockIndex == RelevantBlockCount - 1; }
		}

		public bool AreAllAssignmentsComplete
		{
			get { return m_assignedBlocks == m_relevantBlocks.Count; }
		}

		public void LoadNextRelevantBlock()
		{
			m_navigator.SetIndices(m_relevantBlocks[++m_displayBlockIndex]);
		}

		public void LoadPreviousRelevantBlock()
		{
			m_navigator.SetIndices(m_relevantBlocks[--m_displayBlockIndex]);
		}

		private void PopulateRelevantBlocks()
		{
			m_navigator.NavigateToFirstBlock();
			m_relevantBlocks = new List<Tuple<int, int>>();
			Block block;
			do
			{
				block = m_navigator.CurrentBlock;
				if (IsRelevant(block))
				{
					m_relevantBlocks.Add(m_navigator.GetIndices());
					if (block.UserConfirmed)
						m_assignedBlocks++;
				}
				m_navigator.NextBlock();
			} while (!m_navigator.IsLastBlock(block));

			m_navigator.NavigateToFirstBlock();
		}

		private bool IsRelevant(Block block)
		{
			return block.UserConfirmed || block.CharacterIsUnclear();
		}

		private string BuildHtml(string previousText, string mainText, string followingText, string style)
		{
			var bldr = new StringBuilder();
			bldr.Append(previousText);
			bldr.Append("<div id=\"main-quote-text\" class=\"highlight\">");
			bldr.Append(mainText);
			bldr.Append("</div>");
			if (!string.IsNullOrEmpty(followingText))
				bldr.Append(kHtmlLineBreak).Append(followingText);
			return string.Format(kHtmlFrame, style, bldr);
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
			var bldr = new StringBuilder();
			bldr.Append("<div class='").Append(kCssClassContext).Append("' data-character='").Append(block.CharacterId).Append("'>")
				.Append(block.GetText(m_showVerseNumbers)).Append("</div>").Append(kHtmlLineBreak);
			return bldr.ToString();
		}

		private string BuildStyle()
		{
			return string.Format(kCssFrame, m_fontFamily, m_fontSizeInPoints);
		}

		public void SetCharacterAndDelivery(string selectedCharacter, string selectedDelivery)
		{
			Block currentBlock = CurrentBlock;

			if (selectedCharacter == Narrator)
				currentBlock.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				currentBlock.CharacterId = selectedCharacter;

			currentBlock.Delivery = selectedDelivery == NormalDelivery ? null : selectedDelivery;

			if (!currentBlock.UserConfirmed)
				m_assignedBlocks++;

			currentBlock.UserConfirmed = true;
		}
	}
}
