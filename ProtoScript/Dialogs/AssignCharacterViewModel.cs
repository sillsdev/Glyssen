using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ProtoScript.Character;
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
		private readonly CharacterIdComparer m_characterComparer = new CharacterIdComparer();
		private int m_displayBlockIndex = -1;

		private int m_assignedBlocks;
		private IEnumerable<Block> m_contextBlocksBackward;
		private IEnumerable<Block> m_contextBlocksForward;

		private HashSet<CharacterVerse> m_currentCharacters; 

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
		public CharacterIdComparer CharacterComparer { get { return m_characterComparer; } }
		public int BackwardContextBlockCount { get; set; }
		public int ForwardContextBlockCount { get; set; }

		public string Html
		{
			get
			{
				return BuildHtml(
					BuildHtml(m_contextBlocksBackward = m_navigator.PeekBackwardWithinBook(BackwardContextBlockCount)),
					HttpUtility.HtmlEncode(m_navigator.CurrentBlock.GetText(m_showVerseNumbers)),
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

		public IEnumerable<Character> Characters
		{
			get
			{
				return new SortedSet<Character>(m_currentCharacters.Select(cv => new Character(cv.Character, cv.Alias, cv.UserCreated)),
					m_characterComparer);
			}
		}

		public IEnumerable<Character> GetCharactersForCurrentReference(bool expandIfNone)
		{
			m_currentCharacters = new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetCharacters(CurrentBookId,
				CurrentBlock.ChapterNumber, CurrentBlock.InitialStartVerseNumber, CurrentBlock.InitialEndVerseNumber));

			var listToReturn = new List<Character>(new SortedSet<Character>(
				m_currentCharacters.Select(cv => new Character(cv.Character, cv.Alias, cv.UserCreated)), m_characterComparer));

			if (listToReturn.All(c => !c.IsNarrator))
				listToReturn.Add(Character.Narrator);

			if (m_currentCharacters.Count == 0 && expandIfNone)
			{
				// This will get any potential or actual characters from surrounding material.
				foreach (var block in m_contextBlocksBackward.Union(m_contextBlocksForward))
				{
					foreach (var character in m_combinedCharacterVerseData.GetCharacters(CurrentBookId, block.ChapterNumber,
						block.InitialStartVerseNumber, block.InitialEndVerseNumber))
					{
						m_currentCharacters.Add(character);
					}
				}

				listToReturn.AddRange(new SortedSet<Character>(m_currentCharacters.Select(cv =>
					new Character(cv.Character, cv.Alias, cv.UserCreated)), m_characterComparer));
			}

			return listToReturn;
		}

		public IEnumerable<Character> GetUniqueCharacters(string filterText = null)
		{
			m_currentCharacters = new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries());
			if (!string.IsNullOrWhiteSpace(filterText))
			{
				filterText = filterText.Trim();
				m_currentCharacters.RemoveWhere(c => !c.Character.Contains(filterText, StringComparison.OrdinalIgnoreCase) &&
					!c.Alias.Contains(filterText, StringComparison.OrdinalIgnoreCase));
			}
			return Characters;
		}

		public IEnumerable<Character> GetUniqueCharactersForCurrentBook()
		{
			m_currentCharacters = new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries(CurrentBookId));
			return Characters;
		}
		
		public IEnumerable<Delivery> GetDeliveriesForCharacter(Character selectedCharacter)
		{
			var deliveries = new List<Delivery>();
			deliveries.Add(Delivery.Normal);
			if (!selectedCharacter.IsNarrator)
			{
				foreach (var delivery in m_currentCharacters.Where(c => c.Character == selectedCharacter.CharacterId)
					.Where(c => !string.IsNullOrEmpty(c.Delivery))
					.Select(cv => cv.Delivery))
				{
					if (deliveries.All(d => d.Text != delivery))
						deliveries.Add(new Delivery(delivery));
				}
			}
			return deliveries;
		}

		public IEnumerable<Delivery> GetUniqueDeliveries(string filterText = null)
		{
			if (string.IsNullOrWhiteSpace(filterText))
				return m_combinedCharacterVerseData.GetUniqueDeliveries().Select(d => new Delivery(d));
				
			return m_combinedCharacterVerseData.GetUniqueDeliveries()
				.Where(d => d.Contains(filterText.Trim(), StringComparison.OrdinalIgnoreCase)).Select(d => new Delivery(d));
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
			string text = SuperscriptVerseNumbers(HttpUtility.HtmlEncode(block.GetText(m_showVerseNumbers)));
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

		public void SetCharacterAndDelivery(Character selectedCharacter, Delivery selectedDelivery)
		{
			Block currentBlock = CurrentBlock;

			if (currentBlock.CharacterId == CharacterVerseData.UnknownCharacter || selectedCharacter.UserCreated)
				currentBlock.UserAdded = true;

			if (currentBlock.UserAdded || selectedDelivery.UserCreated)
				AddRecordToProjectCharacterVerseData(currentBlock, selectedCharacter, selectedDelivery);

			if (selectedCharacter.IsNarrator)
				currentBlock.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				currentBlock.CharacterId = selectedCharacter.CharacterId;

			currentBlock.Delivery = selectedDelivery.IsNormal ? null : selectedDelivery.Text;

			if (!currentBlock.UserConfirmed)
			{
				m_assignedBlocks++;
				if (AssignedBlocksIncremented != null)
					AssignedBlocksIncremented(this, new EventArgs());
			}
			currentBlock.UserConfirmed = true;
		}

		private BCVRef GetBlockReference(Block block)
		{
			return new BCVRef(BCVRef.BookToNumber(CurrentBookId), block.ChapterNumber, block.InitialStartVerseNumber);
		}

		private void AddRecordToProjectCharacterVerseData(Block block, Character character, Delivery delivery)
		{
			var cv = new CharacterVerse(
				GetBlockReference(block),
				character.IsNarrator
						? CharacterVerseData.GetStandardCharacterId(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator)
						: character.CharacterId,
				delivery.IsNormal ? null : delivery.Text,
				character.Alias,
				character.UserCreated || delivery.UserCreated);
			m_projectCharacterVerseData.AddCharacterVerse(cv);
		}

		#region Character class
		public class Character
		{
			private static Character s_narrator;

			private readonly string m_characterId;
			private readonly string m_alias;
			private readonly bool m_userCreated;

			public static Character Narrator { get { return s_narrator; } }

			public string CharacterId { get { return m_characterId; } }
			public string Alias { get { return m_alias; } }
			public bool UserCreated { get { return m_userCreated; } }
			public bool IsNarrator { get { return Equals(s_narrator); } }

			public static void SetNarrator(string narrator)
			{
				s_narrator = new Character(narrator);
			}

			internal Character(string characterId, string alias = null, bool userCreated = false)
			{
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					m_characterId = s_narrator.CharacterId;
				else
					m_characterId= characterId;	
				m_alias = String.IsNullOrWhiteSpace(alias) ? null : alias;
				m_userCreated = userCreated;
			}

			public override string ToString()
			{
				return Alias ?? CharacterId;
			}

			#region Equality members
			protected bool Equals(Character other)
			{
				return string.Equals(CharacterId, other.CharacterId);
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
				return (m_characterId != null ? m_characterId.GetHashCode() : 0);
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

		public class CharacterIdComparer : IComparer<Character>
		{
			int IComparer<Character>.Compare(Character x, Character y)
			{
				return String.Compare(x.CharacterId, y.CharacterId, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		#region Delivery class
		public class Delivery
		{
			private static Delivery s_normalDelivery;

			private readonly string m_text;
			private readonly bool m_userCreated;

			public string Text { get { return m_text; } }
			public bool UserCreated { get { return m_userCreated; } }
			public static Delivery Normal { get { return s_normalDelivery; } }
			public bool IsNormal { get { return Equals(s_normalDelivery); } }

			public static void SetNormalDelivery(string normalDelivery)
			{
				s_normalDelivery = new Delivery(normalDelivery);
			}

			internal Delivery(string text, bool userCreated = false)
			{
				m_text = text;
				m_userCreated = userCreated;
			}

			public override string ToString()
			{
				return Text;
			}

			#region Equality members
			protected bool Equals(Delivery other)
			{
				return String.Equals(Text, other.Text);
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
