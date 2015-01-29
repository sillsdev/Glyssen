using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using L10NSharp;
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
		private readonly DeliveryComparer m_deliveryComparer = new DeliveryComparer();
		private int m_displayBlockIndex = -1;

		private int m_assignedBlocks;

		private HashSet<CharacterVerse> m_currentCharacters;
		private List<Delivery> m_currentDeliveries;

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

		public void SetNarratorAndNormalDelivery(string narrator, string normalDelivery)
		{
			Character.SetNarrator(narrator, () => CurrentBookId);
			Delivery.SetNormalDelivery(normalDelivery);
		}

		private IEnumerable<Block> ContextBlocksBackward
		{
			get { return m_navigator.PeekBackwardWithinBook(BackwardContextBlockCount); }
		}

		private IEnumerable<Block> ContextBlocksForward
		{
			get { return m_navigator.PeekForwardWithinBook(ForwardContextBlockCount); }
		}

		public string Html
		{
			get
			{
				return BuildHtml(
					BuildHtml(ContextBlocksBackward),
					HttpUtility.HtmlEncode(m_navigator.CurrentBlock.GetText(m_showVerseNumbers)),
					BuildHtml(ContextBlocksForward),
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

		public HashSet<CharacterVerse> GetUniqueCharactersForCurrentReference()
		{
			return new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetCharacters(CurrentBookId,
				CurrentBlock.ChapterNumber, CurrentBlock.InitialStartVerseNumber, CurrentBlock.InitialEndVerseNumber));
		}

		public IEnumerable<Character> GetCharactersForCurrentReference(bool expandIfNone = true)
		{
			m_currentCharacters = GetUniqueCharactersForCurrentReference();

			var listToReturn = new List<Character>(new SortedSet<Character>(
				m_currentCharacters.Select(cv => new Character(cv.Character, cv.Alias, cv.ProjectSpecific)), m_characterComparer));

			if (listToReturn.All(c => !c.IsNarrator))
				listToReturn.Add(Character.Narrator);

			if (m_currentCharacters.Count == 0 && expandIfNone)
			{
				// This will get any potential or actual characters from surrounding material.
				foreach (var block in ContextBlocksBackward.Union(ContextBlocksForward))
				{
					foreach (var character in m_combinedCharacterVerseData.GetCharacters(CurrentBookId, block.ChapterNumber,
						block.InitialStartVerseNumber, block.InitialEndVerseNumber))
					{
						m_currentCharacters.Add(character);
					}
				}

				listToReturn.AddRange(new SortedSet<Character>(m_currentCharacters.Select(cv =>
					new Character(cv.Character, cv.Alias)), m_characterComparer));
			}

			return listToReturn;
		}

		public IEnumerable<Character> GetUniqueCharacters(string filterText = null)
		{
			var charactersForCurrentRef = GetUniqueCharactersForCurrentReference();

			if (string.IsNullOrWhiteSpace(filterText))
			{
				m_currentCharacters = new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries(CurrentBookId));
			}
			else
			{
				m_currentCharacters = new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetUniqueCharacterAndDeliveries());

				filterText = filterText.Trim();
				m_currentCharacters.RemoveWhere(c => !c.Character.Contains(filterText, StringComparison.OrdinalIgnoreCase) &&
					!c.Alias.Contains(filterText, StringComparison.OrdinalIgnoreCase));
			}

			var listToReturn = new List<Character>(new SortedSet<Character>(m_currentCharacters.Select(cv => new Character(cv.Character, cv.Alias,
				!charactersForCurrentRef.Contains(cv) || cv.ProjectSpecific)), m_characterComparer));

			// PG-88: Now add (at the end of list) any items from charactersForCurrentRef (plus the narrator) that are not in the list.
			listToReturn.AddRange(charactersForCurrentRef.Where(cv => listToReturn.All(ec => ec.CharacterId != cv.Character))
				.Select(cv => new Character(cv.Character, cv.Alias, cv.ProjectSpecific)));

			if (listToReturn.All(c => !c.IsNarrator))
				listToReturn.Add(Character.Narrator);
			return listToReturn;
		}
		
		public IEnumerable<Delivery> GetDeliveriesForCharacter(Character selectedCharacter)
		{
			m_currentDeliveries = new List<Delivery>();
			if (selectedCharacter == null)
				return m_currentDeliveries;
			m_currentDeliveries.Add(Delivery.Normal);
			if (!selectedCharacter.IsNarrator)
			{
				foreach (var cv in m_currentCharacters.Where(c => c.Character == selectedCharacter.CharacterId &&
					!string.IsNullOrEmpty(c.Delivery) &&
					m_currentDeliveries.All(d => d.Text != c.Delivery)))
				{
					m_currentDeliveries.Add(new Delivery(cv.Delivery, cv.ProjectSpecific));
				}
			}
			return m_currentDeliveries;
		}

		public IEnumerable<Delivery> GetUniqueDeliveries(string filterText = null)
		{
			List<Delivery> deliveries;
			if (string.IsNullOrWhiteSpace(filterText))
				deliveries = m_combinedCharacterVerseData.GetUniqueDeliveries()
					.Select(d => m_currentDeliveries.FirstOrDefault(cd => cd.Text == d) ?? new Delivery(d)).ToList();
			else
				deliveries = m_combinedCharacterVerseData.GetUniqueDeliveries()
				.Where(ud => ud.Contains(filterText.Trim(), StringComparison.OrdinalIgnoreCase)).Select(d =>
					m_currentDeliveries.FirstOrDefault(cd => cd.Text == d) ?? new Delivery(d)).ToList();

			deliveries.Sort(m_deliveryComparer);

			deliveries.AddRange(m_currentDeliveries.Where(d => !deliveries.Contains(d)));

			return deliveries;
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
			return (block.UserConfirmed || block.CharacterIsUnclear()) && block.MultiBlockQuote != MultiBlockQuote.Continuation;
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

			if (selectedCharacter.ProjectSpecific || selectedDelivery.ProjectSpecific)
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
				character.ProjectSpecific || delivery.ProjectSpecific);
			m_projectCharacterVerseData.AddCharacterVerse(cv);
		}

		#region Character class
		public class Character
		{
			private static Character s_narrator;

			private readonly string m_characterId;
			private readonly string m_alias;
			private readonly bool m_projectSpecific;
			private static Func<string> s_funcToGetBookId;

			public static Character Narrator { get { return s_narrator; } }

			public string CharacterId { get { return m_characterId; } }
			public string Alias { get { return m_alias; } }
			public bool ProjectSpecific { get { return m_projectSpecific; } }
			public bool IsNarrator { get { return Equals(s_narrator); } }

			public static void SetNarrator(string narrator, Func<string> funcToGetBookId)
			{
				s_funcToGetBookId = funcToGetBookId;
				s_narrator = new Character(narrator, null, false);
			}

			internal Character(string characterId, string alias = null, bool projectSpecific = true)
			{
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					m_characterId = s_narrator.CharacterId;
				else
					m_characterId= characterId;	
				m_alias = String.IsNullOrWhiteSpace(alias) ? null : alias;
				m_projectSpecific = projectSpecific;
			}

			public override string ToString()
			{
				if (IsNarrator)
					return String.Format(CharacterId, s_funcToGetBookId());
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

		#region CharacterIdComparer class
		public class CharacterIdComparer : IComparer<Character>
		{
			int IComparer<Character>.Compare(Character x, Character y)
			{
				return String.Compare(x.CharacterId, y.CharacterId, StringComparison.InvariantCultureIgnoreCase);
			}
		}
		#endregion

		#region Delivery class
		public class Delivery
		{
			private static Delivery s_normalDelivery;

			private readonly string m_text;
			private readonly bool m_projectSpecific;

			public string Text { get { return m_text; } }
			public bool ProjectSpecific { get { return m_projectSpecific; } }
			public static Delivery Normal { get { return s_normalDelivery; } }
			public bool IsNormal { get { return Equals(s_normalDelivery); } }

			public static void SetNormalDelivery(string normalDelivery)
			{
				s_normalDelivery = new Delivery(normalDelivery, false);
			}

			internal Delivery(string text, bool projectSpecific = true)
			{
				m_text = text;
				m_projectSpecific = projectSpecific;
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

		#region DeliveryComparer class
		public class DeliveryComparer : IComparer<Delivery>
		{
			int IComparer<Delivery>.Compare(Delivery x, Delivery y)
			{
				return String.Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
			}
		}
		#endregion
	}
}
