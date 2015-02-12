using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using Paratext;
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
		ExcludeUserConfirmed = 64,
		NeedAssignments = Unexpected | Ambiguous,
		HotSpots = MissingExpectedQuote | MoreQuotesThanExpectedSpeakers | KnownTroubleSpots,
	}

	public class AssignCharacterViewModel : IDisposable
	{
		internal const string kDataCharacter = "data-character";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
								  "<style>{0}</style></head><body {1}>{2}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		private const string kCssClassContext = "context";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
										".highlight{{background-color:yellow}}" +
										"." + kCssClassContext + ":hover{{background-color:#FFFFA0}}" +
										".block-spacer{{height:30px}}" +
										".right-to-left{{direction:rtl}}" +
										".section-header{{text-align:center;font-weight:bold}}";



		private bool m_showVerseNumbers = true; // May make this configurable later
		private Font m_font;
		private readonly string m_fontFamily;
		private readonly int m_baseFontSizeInPoints;
		private int m_fontSizeUiAdjustment;
		private readonly bool m_rightToLeftScript;
		private readonly ProjectCharacterVerseData m_projectCharacterVerseData;
		private readonly CombinedCharacterVerseData m_combinedCharacterVerseData;
		private readonly BlockNavigator m_navigator;
		private readonly IEnumerable<string> m_includedBooks; 
		private List<Tuple<int, int>> m_relevantBlocks;
		private Tuple<int, int> m_temporarilyIncludedBlock;
		private readonly CharacterIdComparer m_characterComparer = new CharacterIdComparer();
		private readonly DeliveryComparer m_deliveryComparer = new DeliveryComparer();
		private static readonly BookBlockTupleComparer s_bookBlockComparer = new BookBlockTupleComparer();
		private int m_displayBlockIndex = -1;

		private int m_assignedBlocks;

		private HashSet<CharacterVerse> m_currentCharacters;
		private List<Delivery> m_currentDeliveries = new List<Delivery>();
		private BlocksToDisplay m_mode;

		public event EventHandler AssignedBlocksIncremented;

		public AssignCharacterViewModel(Project project, BlocksToDisplay mode = BlocksToDisplay.NeedAssignments)
		{
			m_navigator = new BlockNavigator(project.IncludedBooks);
			m_includedBooks = project.IncludedBooks.Select(b => b.BookId);
			m_fontFamily = project.FontFamily;
			m_baseFontSizeInPoints = project.FontSizeInPoints;
			FontSizeUiAdjustment = project.FontSizeUiAdjustment;
			m_rightToLeftScript = project.RightToLeftScript;
			m_projectCharacterVerseData = project.ProjectCharacterVerseData;
			m_combinedCharacterVerseData = new CombinedCharacterVerseData(project);
			Versification = project.Versification;

			Mode = mode;
		}

		public ScrVers Versification { get; private set; }
		public int BlockCountForCurrentBook { get { return m_navigator.CurrentBook.GetScriptBlocks().Count; } }
		public int RelevantBlockCount { get { return m_relevantBlocks.Count; } }
		public int AssignedBlockCount { get { return m_assignedBlocks; } }
		public int CurrentBlockDisplayIndex { get { return m_displayBlockIndex + 1; } }
		public string CurrentBookId { get { return m_navigator.CurrentBook.BookId; } }
		public Block CurrentBlock { get { return m_navigator.CurrentBlock; } }
		public CharacterIdComparer CharacterComparer { get { return m_characterComparer; } }
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
 				do
 				{
	 				location = new Tuple<int, int>(m_navigator.GetIndices().Item1, index);
					m_navigator.SetIndices(location);			
 				} while (CurrentBlock.MultiBlockQuote == MultiBlockQuote.Continuation && --index >= 0);
				Debug.Assert(index >= 0);
 				m_displayBlockIndex = m_relevantBlocks.IndexOf(location);
				m_temporarilyIncludedBlock = m_displayBlockIndex < 0 ? location : null;
			}
		}

		public void SetUiStrings(string narrator, string bookChapterCharacter, string introCharacter,
			string extraCharacter, string normalDelivery)
		{
			Character.SetUIStrings(narrator, bookChapterCharacter, introCharacter, extraCharacter, () => CurrentBookId);
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
					BuildContextBlocksHtml(ContextBlocksBackward),
					BuildCurrentBlockHtml(),
					BuildContextBlocksHtml(ContextBlocksForward),
					BuildStyle());
			}
		}

		public Block GetNthBlockInCurrentBook(int i)
		{
			return m_navigator.CurrentBook.GetScriptBlocks()[i];
		}

		public string GetBlockReferenceString(Block block = null)
		{
			block = block ?? m_navigator.CurrentBlock;
			var startRef = GetBlockReference(block);
			var endRef = (block.InitialEndVerseNumber <= block.InitialStartVerseNumber) ? startRef :
				new BCVRef(startRef.Book, startRef.Chapter, block.InitialEndVerseNumber);
			return BCVRef.MakeReferenceString(startRef, endRef, ":", "-");
		}

		public bool CanNavigateToPreviousRelevantBlock
		{
			get
			{
				if (RelevantBlockCount == 0)
					return false;

				if (IsCurrentBlockRelevant)
					return m_displayBlockIndex != 0; 

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
					return m_displayBlockIndex != RelevantBlockCount - 1;

				// Current block was navigated to ad-hoc and doesn't match the filter. See if there is a relevant block after it.
				var lastRelevantBlock = m_relevantBlocks.Last();
				return s_bookBlockComparer.Compare(lastRelevantBlock, m_temporarilyIncludedBlock) > 0;
			}
		}

		public bool AreAllAssignmentsComplete
		{
			get { return m_assignedBlocks == m_relevantBlocks.Count; }
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
					m_displayBlockIndex = 0;
				else if (RelevantBlockCount > 0)
				{
					m_displayBlockIndex = -1;
					LoadNextRelevantBlock();
				}
			}
		}

		public bool TryLoadBlock(VerseRef verseRef)
		{
			var indices = m_navigator.GetIndicesOfFirstBlockAtReference(new BCVRef(verseRef.BBBCCCVVV));
			if (indices == null)
				return false;
			m_displayBlockIndex = m_relevantBlocks.IndexOf(indices);
			m_temporarilyIncludedBlock = m_displayBlockIndex < 0 ? indices : null;
			m_navigator.SetIndices(indices);
			return true;
		}

		public void LoadNextRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
				m_navigator.SetIndices(m_relevantBlocks[++m_displayBlockIndex]);
			else
				LoadClosestRelevantBlock(false);
		}

		public void LoadPreviousRelevantBlock()
		{
			if (IsCurrentBlockRelevant)
				m_navigator.SetIndices(m_relevantBlocks[--m_displayBlockIndex]);
			else
				LoadClosestRelevantBlock(true);
		}

		private void LoadClosestRelevantBlock(bool prev)
		{
			m_displayBlockIndex = GetIndexOfClosestRelevantBlock(m_relevantBlocks, m_temporarilyIncludedBlock, prev, 0, RelevantBlockCount - 1);
			m_temporarilyIncludedBlock = null;
			m_navigator.SetIndices(m_relevantBlocks[m_displayBlockIndex]);
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
					new Character(cv.Character, cv.Alias)), m_characterComparer).Where(c => !listToReturn.Contains(c)));
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

		private void PopulateRelevantBlocks()
		{
			m_assignedBlocks = 0;
			m_navigator.NavigateToFirstBlock();
			m_relevantBlocks = new List<Tuple<int, int>>();
			Block block = m_navigator.CurrentBlock;
			for (;;)
			{
				if (IsRelevant(block))
				{
					m_relevantBlocks.Add(m_navigator.GetIndices());
					if (block.UserConfirmed)
						m_assignedBlocks++;
				}
				if (m_navigator.IsLastBlock())
					break;
				block = m_navigator.NextBlock();
			}

			m_navigator.NavigateToFirstBlock();
		}

		private bool IsRelevant(Block block)
		{
			if (block.MultiBlockQuote == MultiBlockQuote.Continuation)
				return false;
			if ((Mode & BlocksToDisplay.ExcludeUserConfirmed) > 0 && block.UserConfirmed)
				return false;
			if ((Mode & BlocksToDisplay.NeedAssignments) > 0)
				return (block.UserConfirmed || block.CharacterIsUnclear());
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
			bldr.Append("<div");
			if (block.StyleTag.StartsWith("s"))
				bldr.Append(" class=\"section-header\"");
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

		private IEnumerable<Block> GetAllBlocksWithSameQuote(Block baseLineBlock)
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

		private string SuperscriptVerseNumbers(string text)
		{
			return text.Replace("[", "<sup>").Replace("]", "</sup>");
		}

		private string BuildStyle()
		{
			return String.Format(kCssFrame, m_fontFamily, m_baseFontSizeInPoints + m_fontSizeUiAdjustment);
		}

		public bool IsModified(Character newCharacter, Delivery newDelivery)
		{
			Block currentBlock = CurrentBlock;
			if (newCharacter.IsNarrator)
			{
				if (!currentBlock.CharacterIs(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
					return true;
			}
			else if (newCharacter.CharacterId != currentBlock.CharacterId)
				return true;

			if (newDelivery.IsNormal)
				return (!string.IsNullOrEmpty(currentBlock.Delivery));

			return newDelivery.Text != currentBlock.Delivery;
		}

		private void SetCharacterAndDelivery(Block block, Character selectedCharacter, Delivery selectedDelivery)
		{
			if (selectedCharacter.ProjectSpecific || selectedDelivery.ProjectSpecific)
				AddRecordToProjectCharacterVerseData(block, selectedCharacter, selectedDelivery);

			if (selectedCharacter.IsNarrator)
				block.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				block.CharacterId = selectedCharacter.CharacterId;

			block.Delivery = selectedDelivery.IsNormal ? null : selectedDelivery.Text;

			block.UserConfirmed = true;
		}

		public void SetCharacterAndDelivery(Character selectedCharacter, Delivery selectedDelivery)
		{
			if (!CurrentBlock.UserConfirmed)
			{
				m_assignedBlocks++;
				if (AssignedBlocksIncremented != null)
					AssignedBlocksIncremented(this, new EventArgs());
			}

			foreach (Block block in GetAllBlocksWithSameQuote(CurrentBlock))
				SetCharacterAndDelivery(block, selectedCharacter, selectedDelivery);
		}

		public BCVRef GetBlockReference(Block block)
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
			private static string s_bookChapterCharacter;
			private static string s_introCharacter;
			private static string s_extraCharacter;

			private readonly string m_characterId;
			private readonly string m_alias;
			private readonly bool m_projectSpecific;
			private static Func<string> s_funcToGetBookId;

			public static Character Narrator { get { return s_narrator; } }

			public string CharacterId { get { return m_characterId; } }
			public string Alias { get { return m_alias; } }
			public bool ProjectSpecific { get { return m_projectSpecific; } }
			public bool IsNarrator { get { return Equals(s_narrator); } }

			public static void SetUIStrings(string narrator, string bookChapterCharacter, string introCharacter,
				string extraCharacter, Func<string> funcToGetBookId)
			{
				s_funcToGetBookId = funcToGetBookId;
				s_narrator = new Character(narrator, null, false);
				s_bookChapterCharacter = bookChapterCharacter;
				s_introCharacter = introCharacter;
				s_extraCharacter = extraCharacter;
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

			public static string GetCharacterIdForUi(string characterId, IEnumerable<Character> charactersInContext)
			{
				// TODO: PG-112
				switch (CharacterVerseData.GetStandardCharacterType(characterId))
				{
					case CharacterVerseData.StandardCharacter.Narrator: return s_narrator.ToString();
					case CharacterVerseData.StandardCharacter.Intro: return String.Format(s_introCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.ExtraBiblical: return String.Format(s_extraCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.BookOrChapter: return String.Format(s_bookChapterCharacter, s_funcToGetBookId());
					default: return characterId;
				}
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

		#region
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

		#region IDisposable Members

		public void Dispose()
		{
			if (m_font != null)
			{
				m_font.Dispose();
				m_font = null;
			}
		}

		#endregion
	}
}
