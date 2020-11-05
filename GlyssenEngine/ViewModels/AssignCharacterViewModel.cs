using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Script;
using GlyssenEngine.Utilities;
using SIL.Scripture;
using CollectionExtensions = SIL.Extensions.CollectionExtensions;
using SIL;
using SIL.Reporting;
using static System.String;
using SIL.Extensions;

namespace GlyssenEngine.ViewModels
{
	public class AssignCharacterViewModel<TFont> : BlockNavigatorViewModel<TFont>
	{
		#region Data members and events
		private readonly CombinedCharacterVerseData m_combinedCharacterVerseData;
		private readonly CharacterIdComparer m_characterComparer = new CharacterIdComparer();
		private readonly DeliveryComparer m_deliveryComparer = new DeliveryComparer();
		private readonly AliasComparer m_aliasComparer = new AliasComparer();
		private readonly Dictionary<string, CharacterDetail> m_pendingCharacterDetails = new Dictionary<string, CharacterDetail>();
		private readonly HashSet<ICharacterDeliveryInfo> m_pendingCharacterDeliveryAdditions = new HashSet<ICharacterDeliveryInfo>();
		private ISet<ICharacterDeliveryInfo> m_currentCharacters;
		private IEnumerable<Character> m_generatedCharacterList;
		private List<Delivery> m_currentDeliveries = new List<Delivery>();

		public delegate void AssignedBlockIncrementEventHandler(AssignCharacterViewModel<TFont> sender, int increment);
		public event AssignedBlockIncrementEventHandler AssignedBlocksIncremented;
		public event EventHandler CurrentBookSaved;
		public delegate void CorrelatedBlockChangedHandler(AssignCharacterViewModel<TFont> sender, int index);
		public event CorrelatedBlockChangedHandler CorrelatedBlockCharacterAssignmentChanged;
		public delegate void SettingBlockCharacterHandler(AssignCharacterViewModel<TFont> sender, Block block, ICharacter character);
		public event SettingBlockCharacterHandler SetBlockCharacter;
		public delegate void ProjectCharacterVerseDataAddedHandler(AssignCharacterViewModel<TFont> sender, string reference, string characterId, string delivery);
		public event ProjectCharacterVerseDataAddedHandler ProjectCharacterVerseDataAdded;

		#endregion

		#region Constructors
		public AssignCharacterViewModel(Project project, IAdjustableFontInfo<TFont> fontInfo = null,
			Func<ReferenceText, IAdjustableFontInfo<TFont>> getAdjustableFontInfoForReferenceText = null)
			: this(project, project.Status.AssignCharacterMode != 0 ? project.Status.AssignCharacterMode : BlocksToDisplay.NotYetAssigned,
				project.Status.AssignCharacterBlock, fontInfo, getAdjustableFontInfoForReferenceText)
		{
		}

		public AssignCharacterViewModel(Project project, BlocksToDisplay mode, BookBlockIndices startingIndices, IAdjustableFontInfo<TFont> fontInfo = null,
			Func<ReferenceText, IAdjustableFontInfo<TFont>> getAdjustableFontInfoForReferenceText = null)
			: base(project, mode, startingIndices, fontInfo, getAdjustableFontInfoForReferenceText)
		{
			Logger.WriteEvent($"CompletedBlockCount = {CompletedBlockCount}");

			m_combinedCharacterVerseData = new CombinedCharacterVerseData(project);

			CurrentBlockMatchupChanged += OnCurrentBlockMatchupChanged;
		}

		private void OnCurrentBlockMatchupChanged(object sender, EventArgs args)
		{
			m_pendingCharacterDeliveryAdditions.Clear();
		}

		#endregion

		#region Public properties
		public int CompletedBlockCount { get; private set; }

		public override BlocksToDisplay Mode
		{
			get => base.Mode;
			protected set
			{
				base.Mode = value;
				m_project.Status.AssignCharacterMode = value;
			}
		}

		public bool DoingAssignmentTask => (Mode & BlocksToDisplay.NotAssignedAutomatically) > 0;

		public bool DoingAlignmentTask => Mode == BlocksToDisplay.NotAlignedToReferenceText;

		public bool InTaskMode => DoingAssignmentTask || DoingAlignmentTask;

		public bool IsCurrentTaskComplete => InTaskMode && CompletedBlockCount == RelevantBlockCount;

		public bool IsCurrentBookSingleVoice => CurrentBook.SingleVoice;

		public bool HasSecondaryReferenceText => m_project.ReferenceText.HasSecondaryReferenceText;

		public string PrimaryReferenceTextName => m_project.ReferenceText.LanguageName;
		#endregion

		public void SetUiStrings(string narrator, string bookChapterCharacter, string introCharacter,
			string extraCharacter, string normalDelivery)
		{
			Character.SetUiStrings(narrator, bookChapterCharacter, introCharacter, extraCharacter, () => CurrentBookId, GetCurrentRelevantAlias);
			Delivery.SetNormalDelivery(normalDelivery);
		}

		/// <summary>
		/// Updates the current book to have the specified value, saves the project ti reflect the change,
		/// and resets the state of the model (filter, etc.) accordingly.
		/// </summary>
		/// <param name="singleVoice"></param>
		/// <returns><c>true</c> if the value was changed</returns>
		public bool SetCurrentBookSingleVoice(bool singleVoice)
		{
			if (CurrentBook.SingleVoice == singleVoice)
				return false;

			CurrentBook.SingleVoice = singleVoice;
			m_project.SaveBook(CurrentBook);

			if (singleVoice)
			{
				m_temporarilyIncludedBookBlockIndices = GetCurrentBlockIndices();
				ClearBlockMatchup();
			}

			ResetFilter(CurrentBlock);

			OnSaveCurrentBook();
			return true;
		}

		public void StoreCharacterDetail(string characterId, CharacterGender gender, CharacterAge age)
		{
			if (m_project.AllCharacterDetailDictionary.ContainsKey(characterId))
			{
				throw new ArgumentException("Project already contains a character with ID " + characterId);
			}
			var detail = new CharacterDetail { CharacterId = characterId, Gender = gender, Age = age, MaxSpeakers = 1 };
			m_pendingCharacterDetails[characterId] = detail;
		}

		public void AddPendingProjectCharacterVerseData(Block block, Character character, Delivery delivery)
		{
			AddPendingProjectCharacterVerseData(character.CharacterId, delivery);
		}

		private void AddPendingProjectCharacterVerseDataIfNeeded(Block block, string characterId)
		{
			if (!GetUniqueCharacterVerseObjectsForBlock(block).Any(c => c.Character == characterId && c.Delivery == null))
				AddPendingProjectCharacterVerseData(characterId);
		}

		private void AddPendingProjectCharacterVerseData(string characterId, Delivery delivery = null)
		{
			Debug.Assert(!IsNullOrEmpty(characterId));
			m_pendingCharacterDeliveryAdditions.Add(new CharacterSpeakingMode(
				characterId,
				delivery == null ? Delivery.Normal.Text : delivery.Text,
				null,
				true));
			if (CurrentReferenceTextMatchup != null)
				PopulateCurrentCharactersForCurrentReferenceTextMatchup(); // This forces the model's internal list to refresh to just the relevant ones
		}

		private void OnSaveCurrentBook()
		{
			CurrentBookSaved?.Invoke(this, EventArgs.Empty);
		}

		private void OnAssignedBlocksIncremented(int increment)
		{
			AssignedBlocksIncremented?.Invoke(this, increment);
		}

		#region Overridden methods
		private bool m_inHandleCurrentBlockChanged = false;
		protected override void HandleCurrentBlockChanged()
		{
			if (m_inHandleCurrentBlockChanged)
				return;
			m_inHandleCurrentBlockChanged = true;
			if (CharacterVerseData.IsCharacterExtraBiblical(CurrentBlock.CharacterId))
				throw new InvalidOperationException("Cannot attempt to match an extra-biblical block to a reference text.");
			if (AttemptRefBlockMatchup)
			{
				if (CurrentReferenceTextMatchup == null || !CurrentReferenceTextMatchup.IncludesBlock(CurrentBlock))
				{
					SetBlockMatchupForCurrentLocation();
				}
				else
				{
					CurrentReferenceTextMatchup.ChangeAnchor(CurrentBlock);
				}
			}
			base.HandleCurrentBlockChanged();
			m_inHandleCurrentBlockChanged = false;
		}

		protected override void PopulateRelevantBlocks()
		{
			CompletedBlockCount = 0;
			base.PopulateRelevantBlocks();
		}

		protected override void RelevantBlocksAdded(BookBlockIndices blocksAdded)
		{
			Debug.Assert(blocksAdded.BookIndex == BlockAccessor.GetIndices().BookIndex);
			var completed = IsCurrentBookSingleVoice;
			if (!completed)
			{
				for (var i = blocksAdded.BlockIndex; i <= blocksAdded.EffectiveFinalBlockIndex; i++)
				{
					var block = CurrentBook.GetScriptBlocks()[i];
					if (block.UserConfirmed)
						completed = true;
					else if (block.CharacterIsUnclear)
					{
						// This is not common, but it's possible for one or more blocks in matchup
						// to be user-confirmed, while others are not. Probably because the user
						// did some work in non-rainbow mode.
						return;
					}
				}
			}
			if (completed)
			{
				CompletedBlockCount++;
				Debug.Assert(CompletedBlockCount <= RelevantBlockCount);
			}
		}

		protected override void StoreCurrentBlockIndices()
		{
			m_project.Status.AssignCharacterBlock = GetCurrentBlockIndices();
		}
		#endregion

		#region Methods to get characters and deliveries
		private HashSet<CharacterSpeakingMode> GetUniqueCharacterVerseObjectsForCurrentReference()
		{
			return GetUniqueCharacterVerseObjectsForBlock(CurrentBlock);
		}

		private HashSet<CharacterSpeakingMode> GetUniqueCharacterVerseObjectsForBlock(Block block)
		{
			return new HashSet<CharacterSpeakingMode>(m_combinedCharacterVerseData.GetCharacters(CurrentBookNumber,
				block.ChapterNumber, (Block.VerseRangeFromBlock)block, Versification, true, true));
		}

		public IEnumerable<Character> GetCharactersForCurrentReferenceTextMatchup()
		{
			PopulateCurrentCharactersForCurrentReferenceTextMatchup();
			return GetUniqueCharacters(false);
		}

		private void PopulateCurrentCharactersForCurrentReferenceTextMatchup()
		{
			m_currentCharacters = new HashSet<ICharacterDeliveryInfo>();
			foreach (var block in CurrentReferenceTextMatchup.CorrelatedBlocks)
				m_currentCharacters.UnionWith(GetUniqueCharacterVerseObjectsForBlock(block));
			m_currentCharacters.UnionWith(m_pendingCharacterDeliveryAdditions);
		}

		public IEnumerable<Character> GetUniqueCharactersForCurrentReference(bool expandIfNone = true)
		{
			m_currentCharacters = new HashSet<ICharacterDeliveryInfo>(GetUniqueCharacterVerseObjectsForCurrentReference());
			return GetUniqueCharacters(expandIfNone);
		}

		public IEnumerable<Character> GetUniqueCharactersForBlock(Block block, bool expandIfNone = true)
		{
			m_currentCharacters = new HashSet<ICharacterDeliveryInfo>(GetUniqueCharacterVerseObjectsForBlock(block));
			return GetUniqueCharacters(expandIfNone);
		}

		private IEnumerable<Character> GetUniqueCharacters(bool expandIfNone = true)
		{
			var listToReturn = new List<Character>(new SortedSet<Character>(
				m_currentCharacters.Select(cv => new Character(cv.Character, cv.LocalizedCharacter, cv.Alias, cv.LocalizedAlias, cv.ProjectSpecific)), m_characterComparer));
			listToReturn.Sort(m_aliasComparer);

			if (listToReturn.All(c => !c.IsNarrator))
				listToReturn.Insert(0, Character.Narrator);

			if (m_currentCharacters.Count == 0 && expandIfNone)
			{
				// This will get any expected characters from other verses in the current block.
				var block = CurrentBlock;
				CollectionExtensions.AddRange(m_currentCharacters, m_combinedCharacterVerseData.GetCharacters(CurrentBookNumber, block.ChapterNumber,
					new[] { (Block.VerseRangeFromBlock)block }, Versification, true));

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
					m_currentCharacters.UnionWith(m_combinedCharacterVerseData.GetCharacters(CurrentBookNumber, block.ChapterNumber,
						(Block.VerseRangeFromBlock)block, Versification, true));
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
			var charactersForCurrentRef = GetUniqueCharacterVerseObjectsForCurrentReference();

			if (IsNullOrWhiteSpace(filterText))
			{
				m_currentCharacters = new HashSet<ICharacterDeliveryInfo>(m_combinedCharacterVerseData.GetUniqueCharacterDeliveryInfo(CurrentBookId));
			}
			else
			{
				filterText = filterText.Trim();
				// First add all the matches that do NOT have an alias because we only want to add the aliased versions if there is no non-aliased version.
				var uniqueEntries = m_combinedCharacterVerseData.GetUniqueCharacterDeliveryAliasInfo();
				m_currentCharacters = new HashSet<ICharacterDeliveryInfo>(uniqueEntries
					.Where(c => c.LocalizedAlias == null && c.LocalizedCharacter.Contains(filterText, StringComparison.OrdinalIgnoreCase)),
					new CharacterDeliveryEqualityComparer());
				m_currentCharacters.UnionWith(uniqueEntries.Where(c => c.LocalizedAlias != null &&
					(c.LocalizedCharacter.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
					c.LocalizedAlias.Contains(filterText, StringComparison.OrdinalIgnoreCase))));
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
					!IsNullOrEmpty(c.Delivery) &&
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
			if (IsNullOrWhiteSpace(filterText))
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

		public IEnumerable<Delivery> GetDeliveriesForCurrentReferenceTextMatchup()
		{
			m_currentDeliveries = new List<Delivery>();

			foreach (var block in CurrentReferenceTextMatchup.CorrelatedBlocks)
			{
				foreach (var delivery in GetUniqueCharacterVerseObjectsForBlock(block).Where(cv => !IsNullOrEmpty(cv.Delivery))
					.Select(cv => new Delivery(cv.Delivery, cv.ProjectSpecific)))
				{
					if (!m_currentDeliveries.Any(d => d.Text == delivery.Text))
						m_currentDeliveries.Add(delivery);
				}

				var deliveryFromBlock = GetEffectiveDelivery(block);
				if (!IsNullOrEmpty(deliveryFromBlock) && !m_currentDeliveries.Any(d => d.Text == deliveryFromBlock))
					m_currentDeliveries.Add(new Delivery(deliveryFromBlock));
			}

			m_currentDeliveries.Sort(m_deliveryComparer);

			m_currentDeliveries.Insert(0, Delivery.Normal);

			return m_currentDeliveries;
		}
		
		public string GetEffectiveDelivery(Block block)
		{
			var delivery = block.Delivery;
			if (IsNullOrEmpty(delivery) && block.MatchesReferenceText)
			{
				var refBlock = block.ReferenceBlocks.Single();
				delivery = refBlock.Delivery ??
					(m_project.ReferenceText.HasSecondaryReferenceText ? refBlock.ReferenceBlocks.SingleOrDefault()?.Delivery : null);
			}

			return delivery;
		}

		public IEnumerable<Delivery> GetDeliveriesForCharacterInCurrentReferenceTextMatchup(Character character)
		{
			return GetDeliveriesForCharacter(character).Intersect(GetDeliveriesForCurrentReferenceTextMatchup());
		}
		#endregion

		#region Character/delivery assignment methods
		public bool IsModified(Character newCharacter, Delivery newDelivery)
		{
			Block currentBlock = CurrentBlockInOriginal;
			if (CharacterVerseData.IsCharacterExtraBiblical(currentBlock.CharacterId))
				return false; // Can't change these.

			if (newCharacter == null)
				return !(currentBlock.CharacterIsUnclear || currentBlock.CharacterId == null);
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
				return (!IsNullOrEmpty(currentBlock.Delivery));

			return newDelivery.Text != currentBlock.Delivery;
		}

		private void SetCharacterAndDelivery(Block block, Character selectedCharacter, Delivery selectedDelivery)
		{
			if (CharacterVerseData.IsCharacterUnclear(selectedCharacter.CharacterId))
				throw new ArgumentException("Character cannot be confirmed as ambiguous or unknown.", nameof(selectedCharacter));

			SetCharacter(block, selectedCharacter);
			block.Delivery = selectedDelivery.IsNormal ? null : selectedDelivery.Text;

			AddRecordsToProjectControlFilesIfNeeded(block);
		}

		private void SetCharacter(Block block, Character selectedCharacter)
		{
			if (selectedCharacter == null)
			{
				block.CharacterId = CharacterVerseData.kAmbiguousCharacter;
				block.CharacterIdInScript = null;
			}
			else if (selectedCharacter.IsNarrator)
				block.SetStandardCharacter(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				block.SetCharacterIdAndCharacterIdInScript(selectedCharacter.CharacterId, BCVRef.BookToNumber(CurrentBookId),
					m_project.Versification);
			block.UserConfirmed = !block.CharacterIsUnclear;
			SetBlockCharacter?.Invoke(this, block, selectedCharacter);
		}

		public void SetCharacterAndDelivery(Character selectedCharacter, Delivery selectedDelivery)
		{
			Debug.Assert(CurrentReferenceTextMatchup == null, "This method should never be called when in rainbow mode.");

			if (!CurrentBlockInOriginal.UserConfirmed)
			{
				CompletedBlockCount++;
				OnAssignedBlocksIncremented(1);
			}

			foreach (Block block in GetAllBlocksWhichContinueTheQuoteStartedByBlock(CurrentBlockInOriginal))
				SetCharacterAndDelivery(block, selectedCharacter, selectedDelivery);

			m_project.SaveBook(CurrentBook);
			OnSaveCurrentBook();
		}

		public override void ApplyCurrentReferenceTextMatchup()
		{
			int numberOfBlocksCompleted = 0;
			if (DoingAssignmentTask)
			{
				numberOfBlocksCompleted = CurrentReferenceTextMatchup.OriginalBlocks.Count(b => !b.UserConfirmed && b.CharacterIsUnclear);
			}
			else if (DoingAlignmentTask)
			{
				numberOfBlocksCompleted = 1;
			}
			base.ApplyCurrentReferenceTextMatchup();

			//PG-1106: If the last block in the current matchup is part of a quote that might be continued in following blocks (outside
			// the current matchup), we'll need to check the following block(s) and add records to ProjectCharacterVerse for as many
			// following verses as needed.

			// PG-805: The block matchup UI does not prevent pairing a delivery with a character to which it does not correspond and
			// also allows addition of new character/delivery pairs, so we need to check to see whether this has happened and, if
			// so, add an appropriate entry to the project CV data.
			int iLastBlockInMatchup = CurrentReferenceTextMatchup.IndexOfStartBlockInBook + CurrentReferenceTextMatchup.OriginalBlockCount;
			var blocks = CurrentBook.GetScriptBlocks();
			for (int i = CurrentReferenceTextMatchup.IndexOfStartBlockInBook; i < iLastBlockInMatchup || (i < blocks.Count && blocks[i].IsContinuationOfPreviousBlockQuote); i++)
			{
				var block = blocks[i];
				if (block.IsScripture)
					AddRecordsToProjectControlFilesIfNeeded(block);
			}

			m_pendingCharacterDeliveryAdditions.Clear();

			m_project.SaveBook(CurrentBook);
			OnSaveCurrentBook();
			if (numberOfBlocksCompleted > 0)
			{
				CompletedBlockCount += numberOfBlocksCompleted;
				OnAssignedBlocksIncremented(numberOfBlocksCompleted);
			}
		}

		public bool IsBlockAssignedToUnknownCharacterDeliveryPair(Block block)
		{
			if (block.CharacterIsStandard)
			{
				Debug.Assert(block.Delivery == null);
				return false;
			}
			return !GetUniqueCharacterVerseObjectsForBlock(block).Any(cv => cv.Character == block.CharacterId &&
				((cv.Delivery ?? "") == (block.Delivery ?? "")));
		}

		public void SetReferenceTextMatchupCharacter(int blockIndex, Character selectedCharacter)
		{
			var block = CurrentReferenceTextMatchup.CorrelatedBlocks[blockIndex];
			SetCharacter(block, selectedCharacter);
			var isNarrator = block.CharacterIsStandard; // Can't be one of the other standard types, but if it were, we'd still want to break the chain.
			if (block.MultiBlockQuote == MultiBlockQuote.Start)
			{
				foreach (var contBlock in CurrentReferenceTextMatchup.CorrelatedBlocks.Skip(++blockIndex).TakeWhile(b => b.IsContinuationOfPreviousBlockQuote))
				{
					SetCharacter(contBlock, selectedCharacter);
					if (isNarrator)
						contBlock.MultiBlockQuote = MultiBlockQuote.None;
					CorrelatedBlockCharacterAssignmentChanged?.Invoke(this, blockIndex++);
				}
				if (isNarrator)
					block.MultiBlockQuote = MultiBlockQuote.None;
			}
		}

		public void SetReferenceTextMatchupDelivery(int blockIndex, Delivery selectedDelivery)
		{
			CurrentReferenceTextMatchup.CorrelatedBlocks[blockIndex].Delivery = selectedDelivery.IsNormal ? null : selectedDelivery.Text;
			// REVIEW: We need to think about whether the delivery should automatically flow through the continuation blocks
			// withing the matchup, particularly if they are blocks which were previously all part of the same block and were
			// merely split off by the reference text.
		}

		private void AddRecordsToProjectControlFilesIfNeeded(Block block)
		{
			if (block.CharacterIsStandard)
				return;
			if (m_pendingCharacterDetails.TryGetValue(block.CharacterId, out var detail))
			{
				m_project.AddProjectCharacterDetail(detail);
				m_project.SaveProjectCharacterDetailData();
				m_pendingCharacterDetails.Remove(detail.CharacterId);
			}

			if (m_project.ProjectCharacterVerseData.AddEntriesFor(CurrentBookNumber, block))
				ProjectCharacterVerseDataAdded?.Invoke(this, block.GetReferenceString(BCVRef.NumberToBookCode(CurrentBookNumber)), block.CharacterId, block.Delivery);
			m_project.SaveProjectCharacterVerseData();
		}

		private string GetCurrentRelevantAlias(string characterId)
		{
			if (m_generatedCharacterList == null)
				return null;
			foreach (Character character in m_generatedCharacterList)
			{
				if (character.CharacterId == characterId)
				{
					if (!IsNullOrEmpty(character.LocalizedAlias))
						return character.LocalizedAlias;
					break;
				}
			}
			return null;
		}
		#endregion

		#region Block editing methods
		public void SplitBlock(IEnumerable<BlockSplitData> blockSplits, List<KeyValuePair<int, string>> characters)
		{
			try
			{
				CurrentBlockMatchupChanged -= OnCurrentBlockMatchupChanged;

				// set the character for the first block
				Block currentBlock = CurrentBlock;
				var firstCharacterId = characters.First(c => c.Key == 0).Value;
				if (currentBlock.CharacterId != firstCharacterId)
				{
					if (IsNullOrEmpty(firstCharacterId))
						currentBlock.CharacterId = CharacterVerseData.kUnexpectedCharacter;
					else
					{
						Debug.Assert(currentBlock.CharacterIdOverrideForScript == null && firstCharacterId.SplitCharacterId().Length == 1,
							"This is a case that needs to be fixed for PG-1143");
						currentBlock.CharacterId = firstCharacterId;
						AddPendingProjectCharacterVerseDataIfNeeded(currentBlock, firstCharacterId);
					}
					currentBlock.Delivery = null;
				}

				foreach (var groupOfSplits in blockSplits.GroupBy(s => new {s.BlockToSplit}))
				{
					foreach (var blockSplitData in groupOfSplits.OrderByDescending(s => s,
						BlockSplitData.BlockSplitDataVerseAndOffsetComparer))
					{
						// get the character selected for this split
						var characterId = characters.First(c => c.Key == blockSplitData.Id).Value;

						var originalNextBlock = BlockAccessor.GetNthNextBlockWithinBook(1, blockSplitData.BlockToSplit);
						var chipOffTheOldBlock = CurrentBook.SplitBlock(blockSplitData.BlockToSplit, blockSplitData.VerseToSplit,
							blockSplitData.CharacterOffsetToSplit, true, characterId);
						if (!IsNullOrEmpty(characterId))
							AddPendingProjectCharacterVerseDataIfNeeded(chipOffTheOldBlock, characterId);

						var isNewBlock = originalNextBlock != chipOffTheOldBlock;
						if (isNewBlock)
						{
							var newBlockIndices = GetBlockIndices(chipOffTheOldBlock);
							foreach (var bookBlockIndices in IndicesAtOrBeyondLocationInBook(newBlockIndices))
								bookBlockIndices.BlockIndex++;
						}
						else
						{
							// We "split" between existing blocks in a multi-block quote,
							// so we don't need to do the same kind of cleanup above.
						}
						AddToRelevantBlocksIfNeeded(chipOffTheOldBlock, isNewBlock);
					}
				}

				if (AttemptRefBlockMatchup)
				{
					// A split will always require the current matchup to be re-constructed.
					SetBlockMatchupForCurrentLocation();
				}


				//// This is basically a hack. All kinds of problems were occurring after splits causing our indices to get off.
				//// See https://jira.sil.org/browse/PG-1075. This ensures our state is valid every time.
				//SetModeInternal(Mode, true);
			}
			finally
			{
				CurrentBlockMatchupChanged += OnCurrentBlockMatchupChanged;
			}
		}
		#endregion

		#region Character class
		public class Character : ICharacter
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
			public bool IsStandard => new List<String>
			{
				s_narrator.CharacterId,
				s_bookChapterCharacter,
				s_introCharacter,
				s_extraCharacter
			}.Contains(CharacterId);

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

			public Character(string characterId, string localizedCharacterId = null, string alias = null, string localizedAlias = null, bool projectSpecific = true)
			{
				m_characterId = CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator) ?
					s_narrator.CharacterId : characterId;
				m_localizedCharacterId = localizedCharacterId ?? characterId;
				m_alias = IsNullOrWhiteSpace(alias) ? null : alias;
				m_localizedAlias = IsNullOrWhiteSpace(localizedAlias) ? null : localizedAlias;
				m_projectSpecific = projectSpecific;
			}

			public override string ToString()
			{
				if (IsNarrator)
					return Format(CharacterId, s_funcToGetBookId());
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
					case CharacterVerseData.StandardCharacter.Intro: return Format(s_introCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.ExtraBiblical: return Format(s_extraCharacter, s_funcToGetBookId());
					case CharacterVerseData.StandardCharacter.BookOrChapter: return Format(s_bookChapterCharacter, s_funcToGetBookId());
					default:
						if (characterId == CharacterVerseData.kAmbiguousCharacter || characterId == CharacterVerseData.kUnexpectedCharacter)
							return "";
						string relevantAlias = s_funcToGetRelevantAlias(characterId);
						characterId = Localizer.GetDynamicString(GlyssenInfo.ApplicationId, "CharacterName." + characterId, characterId);
						if (relevantAlias != null)
							return characterId + " [" + relevantAlias + "]";
						return characterId;
				}
			}

			#region Equality members
			protected bool Equals(Character other)
			{
				return String.Equals(CharacterId, other.CharacterId);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != GetType())
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

		public abstract class CharacterComparer
		{
			protected int CompareSpecialCases(Character x, Character y, string xTextToCompare, string yTextToCompare)
			{
				// if the CharacterIds are not the same, check for a special case
				if ((x.CharacterId) != (y.CharacterId))
				{
					// narrator should be first item
					if (x.IsNarrator) return -1;
					if (y.IsNarrator) return 1;

					// Jesus should be second item
					if (x.CharacterId == "Jesus") return -1;
					if (y.CharacterId == "Jesus") return 1;
				}

				// this is not a special case
				return Compare(xTextToCompare, yTextToCompare, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public class CharacterIdComparer : CharacterComparer, IComparer<Character>
		{
			int IComparer<Character>.Compare(Character x, Character y)
			{
				return CompareSpecialCases(x, y, x.CharacterId, y.CharacterId);
			}
		}
		#endregion

		#region AliasComparer class
		public class AliasComparer : CharacterComparer, IComparer<Character>
		{
			int IComparer<Character>.Compare(Character x, Character y)
			{
				var xTextToCompare = IsNullOrEmpty(x.Alias) ? x.CharacterId : x.Alias;
				var yTextToCompare = IsNullOrEmpty(y.Alias) ? y.CharacterId : y.Alias;

				var result = CompareSpecialCases(x, y, xTextToCompare, yTextToCompare);
				return result != 0 ? result : Compare(x.CharacterId, y.CharacterId, StringComparison.InvariantCultureIgnoreCase);
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
			public string LocalizedDisplay { get { return ToLocalizedString(); } }

			private string ToLocalizedString()
			{
				// TODO: Enable localization of deliveries
				return Text;
			}
			public bool ProjectSpecific { get { return m_projectSpecific; } }
			public static Delivery Normal { get { return s_normalDelivery; } }
			public bool IsNormal { get { return Equals(s_normalDelivery); } }

			public static void SetNormalDelivery(string normalDelivery)
			{
				s_normalDelivery = new Delivery(normalDelivery, false);
			}

			public Delivery(string text, bool projectSpecific = true)
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
				if (obj.GetType() != GetType())
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
				return Compare(x.Text, y.Text, StringComparison.InvariantCultureIgnoreCase);
			}
		}
		#endregion

		public Character GetCharacterToSelectForCurrentBlock(IEnumerable<Character> currentCharacters)
		{
			if (CurrentBlock.CharacterIs(CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
				return Character.Narrator;
			if (CurrentBlock.CharacterIsUnclear)
			{
				if (!CurrentBlock.ContainsVerseNumber)
				{
					var charactersForCurrentVerse = GetUniqueCharacterVerseObjectsForCurrentReference();
					// ENHANCE: Some "Quotations" in the control file may represent text that is typically rendered as
					// indirect speech (and should therefore be marked as Indirect|Quotation). We really don't want to
					// include these, but in practice it probably won't matter much.
					charactersForCurrentVerse.RemoveWhere(c => !c.IsExpected && c.QuoteType != QuoteType.Quotation);
					if (charactersForCurrentVerse.Distinct(m_characterEqualityComparer).Count() != 2)
						return null;
					var userConfirmedBlock = CurrentBook.GetBlocksForVerse(CurrentBlock.ChapterNumber, CurrentBlock.InitialStartVerseNumber).OnlyOrDefault(b => b.UserConfirmed);
					if (userConfirmedBlock == null)
						return null;

					charactersForCurrentVerse.RemoveWhere(c => c.Character == userConfirmedBlock.CharacterId);

					if (charactersForCurrentVerse.Count != 1)
						return null;

					return currentCharacters.FirstOrDefault(item => item.LocalizedCharacterId == charactersForCurrentVerse.Single().LocalizedCharacter);
				}
			}
			return currentCharacters.FirstOrDefault(item => item.CharacterId == CurrentBlock.CharacterId);
		}

		public void GetRowIndicesForMovingReferenceText(bool down, int currentRowIndex, out int iPreceding, out int iFollowing)
		{
			if (currentRowIndex < 0 || currentRowIndex >= CurrentReferenceTextMatchup.CorrelatedBlocks.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(currentRowIndex),
					"Index must correspond to a correlated block in the current reference text matchup.");
			}

			if (down)
			{
				if (currentRowIndex == CurrentReferenceTextMatchup.CorrelatedBlocks.Count -1 )
				{
					throw new ArgumentException(nameof(currentRowIndex),
						"When searching downward, index must not correspond to the last correlated block in the current reference text matchup.");
				}
				iPreceding = currentRowIndex;
				iFollowing = iPreceding + 1;
				if (!TryFindScriptureRowAtOrBelow(ref iFollowing))
					throw new InvalidOperationException($"Current Matchup is invalid: {CurrentReferenceTextMatchup}");
			}
			else
			{
				if (currentRowIndex == 0)
				{
					throw new ArgumentException(nameof(currentRowIndex),
						"When searching upward, index must not correspond to the first correlated block in the current reference text matchup.");
				}

				iFollowing = currentRowIndex;
				iPreceding = iFollowing - 1;
				if (!TryFindScriptureRowAtOrAbove(ref iPreceding))
					throw new InvalidOperationException($"Current Matchup is invalid: {CurrentReferenceTextMatchup}");
			}
		}

		public bool TryFindScriptureRowAtOrBelow(ref int i)
		{
			var count = CurrentReferenceTextMatchup.CorrelatedBlocks.Count;
			// Since the only non-Scripture block that can ever exist in a matchup is a
			// section head, all we really need to do is:
			// CurrentReferenceTextMatchup.CorrelatedBlocks[i].CharacterIs(CurrentBookId, CharacterVerseData.StandardCharacter.ExtraBiblical))
			// But since we don't expect this to be performance-critical, we'll use the
			// ultra-safe, shorter, and more readable IsScripture.
			while (i < count && !CurrentReferenceTextMatchup.CorrelatedBlocks[i].IsScripture)
				i++;

			return i < CurrentReferenceTextMatchup.CorrelatedBlocks.Count;
		}

		public bool TryFindScriptureRowAtOrAbove(ref int i)
		{
			// See comment in TryFindScriptureRowAtOrBelow
			while (i >= 0 && !CurrentReferenceTextMatchup.CorrelatedBlocks[i].IsScripture)
				i--;

			return i >= 0;
		}
	}
}
