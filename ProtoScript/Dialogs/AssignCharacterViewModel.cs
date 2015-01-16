using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SIL.ScriptureUtils;

namespace ProtoScript.Dialogs
{
	public class AssignCharacterViewModel
	{
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body {1}>{2}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		private const string kCssClassContext = "context";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
										".highlight{{background-color:yellow}}" +
										"." + kCssClassContext + ":hover{{background-color:#FFFFA0}}" +
										".block-spacer{{height:30px}}" +
										".right-to-left{{direction:rtl}}";

		private bool m_showVerseNumbers = true; // May make this configurable later
		private readonly string m_fontFamily;
		private readonly int m_fontSizeInPoints;
		private readonly bool m_rightToLeftScript;
		private readonly ProjectCharacterVerseData m_projectCharacterVerseData;
		private readonly CombinedCharacterVerseData m_combinedCharacterVerseData;
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
			m_rightToLeftScript = project.RightToLeftScript;
			m_projectCharacterVerseData = project.ProjectCharacterVerseData;
			m_combinedCharacterVerseData = new CombinedCharacterVerseData(project);

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

		public IEnumerable<CharacterVerse> GetCharacters(string bookCode, int chapter, int verse)
		{
			return m_combinedCharacterVerseData.GetCharacters(bookCode, chapter, verse);
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries()
		{
			return m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries();
		}

		public IEnumerable<CharacterVerse> GetUniqueCharacterAndDeliveries(string bookCode)
		{
			return m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries(bookCode);
		}

		public IEnumerable<string> GetUniqueDeliveries()
		{
			return m_combinedCharacterVerseData.GetUniqueDeliveries();
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
			string text = SuperscriptVerseNumbers(block.GetText(m_showVerseNumbers));
			var bldr = new StringBuilder();
			bldr.Append("<div class='").Append(kCssClassContext).Append("' data-character='").Append(block.CharacterId).Append("'>")
				.Append(text)
				.Append("</div>")
				.Append(kHtmlLineBreak);
			return bldr.ToString();
		}

		private string SuperscriptVerseNumbers(string text)
		{
			return text.Replace("[", "<sup>").Replace("]", "</sup>");
		}

		private string BuildStyle()
		{
			return String.Format(kCssFrame, m_fontFamily, m_fontSizeInPoints);
		}

		internal void SetCharacterAndDelivery(Character selectedCharacter, Delivery selectedDelivery)
		{
			Block currentBlock = CurrentBlock;

			if (currentBlock.CharacterId == CharacterVerseData.UnknownCharacter || selectedCharacter.UserCreated)
				currentBlock.UserAdded = true;

			if (currentBlock.UserAdded || selectedDelivery.UserCreated)
				AddRecordToProjectCharacterVerseData(currentBlock, selectedCharacter, selectedDelivery);

			if (selectedCharacter.Text == Narrator)
				currentBlock.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				currentBlock.CharacterId = selectedCharacter.Text;

			currentBlock.Delivery = selectedDelivery.Text == NormalDelivery ? null : selectedDelivery.Text;

			if (!currentBlock.UserConfirmed)
			{
				m_assignedBlocks++;
				if (AssignedBlocksIncremented != null)
					AssignedBlocksIncremented(this, new EventArgs());
			}
			currentBlock.UserConfirmed = true;
		}

		private void AddRecordToProjectCharacterVerseData(Block block, Character character, Delivery delivery)
		{
			var cv = new CharacterVerse
			{
				BcvRef = new BCVRef(BCVRef.BookToNumber(CurrentBookId), block.ChapterNumber, block.InitialStartVerseNumber),
				Character = character.Text == Narrator ? CharacterVerseData.GetStandardCharacterId(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator) : character.Text,
				Delivery = delivery.Text == NormalDelivery ? null : delivery.Text,
				UserCreated = character.UserCreated || delivery.UserCreated
			};
			m_projectCharacterVerseData.AddCharacterVerse(cv);
		}

		#region Character class
		internal class Character
		{
			public string Text;
			public bool UserCreated;

			public Character(string text)
			{
				Text = text;
			}

			public Character(string text, bool userCreated)
			{
				Text = text;
				UserCreated = userCreated;
			}

			public override string ToString()
			{
				return Text;
			}

			#region Equality members
			protected bool Equals(Character other)
			{
				return string.Equals(Text, other.Text);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != this.GetType())
					return false;
				return Equals((Character)obj);
			}

			public override int GetHashCode()
			{
				return (Text != null ? Text.GetHashCode() : 0);
			}

			public static bool operator ==(Character left, Character right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(Character left, Character right)
			{
				return !Equals(left, right);
			}
			#endregion
		}
		#endregion

		#region Delivery class
		internal class Delivery
		{
			public string Text;
			public bool UserCreated;

			public Delivery(string text)
			{
				Text = text;
			}

			public Delivery(string text, bool userCreated)
			{
				Text = text;
				UserCreated = userCreated;
			}

			public override string ToString()
			{
				return Text;
			}

			#region Equality members
			protected bool Equals(Delivery other)
			{
				return String.Equals(Text, (string)other.Text);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != this.GetType())
					return false;
				return Equals((Delivery)obj);
			}

			public override int GetHashCode()
			{
				return (Text != null ? Text.GetHashCode() : 0);
			}

			public static bool operator ==(Delivery left, Delivery right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(Delivery left, Delivery right)
			{
				return !Equals(left, right);
			}
			#endregion
		}
		#endregion
	}
}
