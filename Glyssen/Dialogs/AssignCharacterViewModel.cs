using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Utilities;
using L10NSharp;
using SIL.Scripture;
using ScrVers = Paratext.ScrVers;

namespace Glyssen.Dialogs
{
	public class AssignCharacterViewModel : BlockNavigatorViewModel
	{
		#region Data members and events
		private readonly ProjectCharacterVerseData m_projectCharacterVerseData;
		private readonly CombinedCharacterVerseData m_combinedCharacterVerseData;
		private readonly CharacterIdComparer m_characterComparer = new CharacterIdComparer();
		private readonly DeliveryComparer m_deliveryComparer = new DeliveryComparer();
		private readonly AliasComparer m_aliasComparer = new AliasComparer();
		private int m_assignedBlocks;
		private HashSet<CharacterVerse> m_currentCharacters;
		private IEnumerable<Character> m_generatedCharacterList;
		private List<Delivery> m_currentDeliveries = new List<Delivery>();

		public event EventHandler AssignedBlocksIncremented;
		public event EventHandler CurrentBookSaved;
		#endregion

		#region Constructors
		public AssignCharacterViewModel(Project project)
			: this(project, project.Status.AssignCharacterMode != 0 ? project.Status.AssignCharacterMode : BlocksToDisplay.NeedAssignments, project.Status.AssignCharacterBlock)
		{
		}

		public AssignCharacterViewModel(Project project, BlocksToDisplay mode, BookBlockIndices startingIndices)
			: base(project, mode, startingIndices)
		{
			m_projectCharacterVerseData = project.ProjectCharacterVerseData;
			m_combinedCharacterVerseData = new CombinedCharacterVerseData(project);
		}
		#endregion

		#region Public properties
		public int AssignedBlockCount { get { return m_assignedBlocks; } }

		public override BlocksToDisplay Mode
		{
			get { return base.Mode; }
			set
			{
				base.Mode = value;
				m_project.Status.AssignCharacterMode = value;
			}
		}

		public bool AreAllAssignmentsComplete
		{
			get { return m_assignedBlocks == m_relevantBlocks.Count; }
		}

		public bool IsCurrentBookSingleVoice
		{
			get { return CurrentBook.SingleVoice; }
		}
		#endregion

		public void SetUiStrings(string narrator, string bookChapterCharacter, string introCharacter,
			string extraCharacter, string normalDelivery)
		{
			Character.SetUiStrings(narrator, bookChapterCharacter, introCharacter, extraCharacter, () => CurrentBookId, GetCurrentRelevantAlias);
			Delivery.SetNormalDelivery(normalDelivery);
		}

		public void SetCurrentBookSingleVoice(bool singleVoice)
		{
			if (CurrentBook.SingleVoice == singleVoice)
				return;
			CurrentBook.SingleVoice = singleVoice;
			if (singleVoice)
			{
				// Order is important
				AssignNarratorForRemainingBlocksInCurrentBook();
				m_project.SaveBook(CurrentBook);
				LoadNextRelevantBlockInSubsequentBook();
			}
			else
				m_project.SaveBook(CurrentBook);
			OnSaveCurrentBook();

			Analytics.Track("SetSingleVoice", new Dictionary<string, string>
			{
				{ "book", CurrentBookId },
				{ "singleVoice", singleVoice.ToString() },
				{ "method", "AssignCharacterViewModel.SetCurrentBookSingleVoice" }
			});
		}

		public void AddCharacterDetailToProject(string characterId, CharacterGender gender, CharacterAge age)
		{
			var detail = new CharacterDetail { CharacterId = characterId, Gender = gender, Age = age, MaxSpeakers = 1 };
			m_project.AddProjectCharacterDetail(detail);
			m_project.SaveProjectCharacterDetailData();
		}

		private void OnSaveCurrentBook()
		{
			if (CurrentBookSaved != null)
				CurrentBookSaved(this, EventArgs.Empty);
		}

		private void OnAssignedBlocksIncremented()
		{
			if (AssignedBlocksIncremented != null)
				AssignedBlocksIncremented(this, new EventArgs());
		}

		#region Overridden methods
		protected override void PopulateRelevantBlocks()
		{
			m_assignedBlocks = 0;
			base.PopulateRelevantBlocks();
		}

		protected override void RelevantBlockAdded(Block block)
		{
			if (block.UserConfirmed)
				m_assignedBlocks++;
		}

		protected override void StoreCurrentBlockIndices()
		{
			m_project.Status.AssignCharacterBlock = GetCurrentBlockIndices();
		}
		#endregion

		#region Methods to get characters and deliveries
		public HashSet<CharacterVerse> GetUniqueCharactersForCurrentReference()
		{
			return new HashSet<CharacterVerse>(m_combinedCharacterVerseData.GetCharacters(CurrentBookId,
				CurrentBlock.ChapterNumber, CurrentBlock.InitialStartVerseNumber, CurrentBlock.InitialEndVerseNumber, versification: Versification));
		}

		public IEnumerable<Character> GetCharactersForCurrentReference(bool expandIfNone = true)
		{
			m_currentCharacters = GetUniqueCharactersForCurrentReference();

			var listToReturn = new List<Character>(new SortedSet<Character>(
				m_currentCharacters.Select(cv => new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias, cv.ProjectSpecific)), m_characterComparer));
			listToReturn.Sort(m_aliasComparer);

			if (listToReturn.All(c => !c.IsNarrator))
				listToReturn.Add(Character.Narrator);

			if (m_currentCharacters.Count == 0 && expandIfNone)
			{
				// This will get any expected characters from other verses in the current block.
				var block = CurrentBlock;
				foreach (var character in m_combinedCharacterVerseData.GetCharacters(CurrentBookId, block.ChapterNumber,
						block.InitialStartVerseNumber, block.LastVerse, versification: Versification))
				{
					m_currentCharacters.Add(character);
				}

				var listToAdd = new SortedSet<Character>(m_currentCharacters.Select(cv =>
					new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias)), m_characterComparer).Where(c => !listToReturn.Contains(c)).ToList();
				listToAdd.Sort(m_aliasComparer);
				listToReturn.AddRange(listToAdd);
			}

			if (m_currentCharacters.Count == 0 && expandIfNone)
			{
				// This will get any potential or actual characters from surrounding material.
				foreach (var block in ContextBlocksBackward.Union(ContextBlocksForward))
				{
					foreach (var character in m_combinedCharacterVerseData.GetCharacters(CurrentBookId, block.ChapterNumber,
						block.InitialStartVerseNumber, block.InitialEndVerseNumber, versification: Versification))
					{
						m_currentCharacters.Add(character);
					}
				}

				var listToAdd = new SortedSet<Character>(m_currentCharacters.Select(cv =>
					new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias)), m_characterComparer).Where(c => !listToReturn.Contains(c)).ToList();
				listToAdd.Sort(m_aliasComparer);
				listToReturn.AddRange(listToAdd);
			}

			return m_generatedCharacterList = listToReturn;
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
				m_currentCharacters.RemoveWhere(c => !c.LocalizedCharacter.Contains(filterText, StringComparison.OrdinalIgnoreCase) &&
					(c.LocalizedAlias == null || !c.LocalizedAlias.Contains(filterText, StringComparison.OrdinalIgnoreCase)));
			}

			var listToReturn = new List<Character>(new SortedSet<Character>(
				m_currentCharacters.Select(cv => new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias,
					!charactersForCurrentRef.Contains(cv) || cv.ProjectSpecific)), m_characterComparer));
			listToReturn.Sort(m_aliasComparer);

			// PG-88: Now add (at the end of list) any items from charactersForCurrentRef (plus the narrator) that are not in the list.
			listToReturn.AddRange(charactersForCurrentRef.Where(cv => listToReturn.All(ec => ec.CharacterId != cv.Character))
				.Select(cv => new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias, cv.ProjectSpecific)));

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
		#endregion

		#region Character/delivery assignment methods
		public bool IsModified(Character newCharacter, Delivery newDelivery)
		{
			Block currentBlock = CurrentBlock;
			if (CharacterVerseData.IsCharacterStandard(currentBlock.CharacterId, false))
				return false; // Can't change these.

			if (newCharacter == null)
				return !(currentBlock.CharacterIsUnclear() || currentBlock.CharacterId == null);
			if (newCharacter.IsNarrator)
			{
				if (!currentBlock.CharacterIs(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
					return true;
			}
			else if (newCharacter.CharacterId != currentBlock.CharacterId)
				return true;

			if (newDelivery == null)
				return true;

			if (newDelivery.IsNormal)
				return (!string.IsNullOrEmpty(currentBlock.Delivery));

			return newDelivery.Text != currentBlock.Delivery;
		}

		private void SetCharacterAndDelivery(Block block, Character selectedCharacter, Delivery selectedDelivery)
		{
			// If the user sets a non-narrator to a block we marked as narrator, we want to track it
			if (!selectedCharacter.IsNarrator && !block.IsQuote)
				Analytics.Track("NarratorToQuote", new Dictionary<string, string>
				{
					{ "book", CurrentBookId },
					{ "chapter", block.ChapterNumber.ToString(CultureInfo.InvariantCulture) },
					{ "initialStartVerse", block.InitialStartVerseNumber.ToString(CultureInfo.InvariantCulture) },
					{ "character", selectedCharacter.CharacterId }
				});

			if (selectedCharacter.ProjectSpecific || selectedDelivery.ProjectSpecific)
				AddRecordToProjectCharacterVerseData(block, selectedCharacter, selectedDelivery);

			if (selectedCharacter.IsNarrator)
				block.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				block.SetCharacterAndCharacterIdInScript(selectedCharacter.CharacterId, BCVRef.BookToNumber(CurrentBookId), m_project.Versification);

			block.Delivery = selectedDelivery.IsNormal ? null : selectedDelivery.Text;

			block.UserConfirmed = true;
		}

		public void SetCharacterAndDelivery(Character selectedCharacter, Delivery selectedDelivery)
		{
			if (!CurrentBlock.UserConfirmed)
			{
				m_assignedBlocks++;
				OnAssignedBlocksIncremented();
			}

			foreach (Block block in GetAllBlocksWithSameQuote(CurrentBlock))
				SetCharacterAndDelivery(block, selectedCharacter, selectedDelivery);

			m_project.SaveBook(CurrentBook);
			OnSaveCurrentBook();
		}

		private void AddRecordToProjectCharacterVerseData(Block block, Character character, Delivery delivery)
		{
			var cv = new CharacterVerse(
				new BCVRef(GetBlockVerseRef(block, ScrVers.English).BBBCCCVVV),
				character.IsNarrator
						? CharacterVerseData.GetStandardCharacterId(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator)
						: character.CharacterId,
				delivery.IsNormal ? null : delivery.Text,
				character.Alias,
				character.ProjectSpecific || delivery.ProjectSpecific);
			m_projectCharacterVerseData.Add(cv);

			m_project.SaveProjectCharacterVerseData();
		}

		private string GetCurrentRelevantAlias(string characterId)
		{
			foreach (Character character in m_generatedCharacterList)
			{
				if (character.CharacterId == characterId)
				{
					if (!string.IsNullOrEmpty(character.LocalizedAlias))
						return character.LocalizedAlias;
					break;
				}
			}
			return null;
		}

		private void AssignNarratorForRemainingBlocksInCurrentBook()
		{
			AssignNarratorForRemainingBlocksInBook(CurrentBook);
		}

		public void AssignNarratorForRemainingBlocksInBook(BookScript book)
		{
			foreach (var block in book.GetScriptBlocks().Where(b => b.CharacterIsUnclear()))
			{
				block.SetStandardCharacter(book.BookId, CharacterVerseData.StandardCharacter.Narrator);
				block.UserConfirmed = true;

				if (block.MultiBlockQuote != MultiBlockQuote.Continuation)
				{
					m_assignedBlocks++;
					OnAssignedBlocksIncremented();
				}
			}
		}
		#endregion

		#region Block editing methods
		public Block SplitBlock(Block blockToSplit, string verseToSplit, int characterOffsetToSplit)
		{
			Block newBlock = CurrentBook.SplitBlock(blockToSplit, verseToSplit, characterOffsetToSplit);
			AddToRelevantBlocksIfNeeded(newBlock);
			return newBlock;
		}
		#endregion

		#region Character class
		public class Character
		{
			private static Character s_narrator;
			private static string s_bookChapterCharacter;
			private static string s_introCharacter;
			private static string s_extraCharacter;

			private readonly string m_characterId;
			private readonly string m_localizedCharacterId;
			private readonly string m_localizedAlias;
			private readonly string m_alias;
			private readonly bool m_projectSpecific;
			private static Func<string> s_funcToGetBookId;
			private static Func<string, string> s_funcToGetRelevantAlias;

			public static Character Narrator { get { return s_narrator; } }

			public string CharacterId { get { return m_characterId; } }
			public string LocalizedCharacterId { get { return m_localizedCharacterId; } }
			public string Alias { get { return m_alias; } }
			public string LocalizedAlias { get { return m_localizedAlias; } }
			public bool ProjectSpecific { get { return m_projectSpecific; } }
			public bool IsNarrator { get { return Equals(s_narrator); } }

			public string LocalizedDisplay { get { return ToLocalizedString(); } }

			public static void SetUiStrings(string narrator, string bookChapterCharacter, string introCharacter,
				string extraCharacter, Func<string> funcToGetBookId, Func<string, string> funcToGetRelevantAlias)
			{
				s_funcToGetBookId = funcToGetBookId;
				s_funcToGetRelevantAlias = funcToGetRelevantAlias;
				s_narrator = new Character(narrator, null, null, null, false);
				s_bookChapterCharacter = bookChapterCharacter;
				s_introCharacter = introCharacter;
				s_extraCharacter = extraCharacter;
			}

			internal Character(string characterId, string localizedCharacterId = null, string alias = null, string localizedAlias = null, bool projectSpecific = true)
			{
				m_characterId = CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator) ?
					s_narrator.CharacterId : characterId;
				m_localizedCharacterId = localizedCharacterId ?? characterId;
				m_alias = String.IsNullOrWhiteSpace(alias) ? null : alias;
				m_localizedAlias = String.IsNullOrWhiteSpace(localizedAlias) ? null : localizedAlias;
				m_projectSpecific = projectSpecific;
			}

			public override string ToString()
			{
				if (IsNarrator)
					return String.Format(CharacterId, s_funcToGetBookId());
				return LocalizedAlias ?? CharacterId;
			}

			public string ToLocalizedString()
			{
				if (IsNarrator)
					return ToString();
				return LocalizedAlias ?? LocalizedCharacterId;
			}

			public static string GetCharacterIdForUi(string characterId)
			{
				switch (CharacterVerseData.GetStandardCharacterType(characterId))
				{
					case CharacterVerseData.StandardCharacter.Narrator: return s_narrator.ToString();
					case CharacterVerseData.StandardCharacter.Intro: return String.Format(s_introCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.ExtraBiblical: return String.Format(s_extraCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.BookOrChapter: return String.Format(s_bookChapterCharacter, s_funcToGetBookId());
					default:
						if (characterId == CharacterVerseData.AmbiguousCharacter || characterId == CharacterVerseData.UnknownCharacter)
							return "";
						string relevantAlias = s_funcToGetRelevantAlias(characterId);
						characterId = LocalizationManager.GetDynamicString(Program.kApplicationId, "CharacterName." + characterId, characterId);
						if (relevantAlias != null)
							return characterId + " [" + relevantAlias + "]";
						return characterId;
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

		#region AliasComparer class
		public class AliasComparer : IComparer<Character>
		{
			int IComparer<Character>.Compare(Character x, Character y)
			{
				string xTextToCompare = string.IsNullOrEmpty(x.Alias) ? x.CharacterId : x.Alias;
				string yTextToCompare = string.IsNullOrEmpty(y.Alias) ? y.CharacterId : y.Alias;
				int result = String.Compare(xTextToCompare, yTextToCompare, StringComparison.InvariantCultureIgnoreCase);
				if (result != 0)
					return result;
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
