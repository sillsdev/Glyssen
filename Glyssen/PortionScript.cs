﻿using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using SIL.Scripture;
using ScrVers = Paratext.ScrVers;

namespace Glyssen
{
	public class PortionScript
	{
		public const int kSplitAtEndOfVerse = -999;
		protected string m_id;
		protected List<Block> m_blocks;

		public PortionScript(string id, IEnumerable<Block> blocks)
		{
			m_id = id;
			if (blocks != null)
				m_blocks = blocks.ToList();
		}

		public string Id { get { return m_id; } }
		public virtual IReadOnlyList<Block> GetScriptBlocks()
		{
			return m_blocks;
		}

		public Block SplitBlock(Block blockToSplit, string verseToSplit, int characterOffsetToSplit, bool userSplit = true,
			string characterId = null, ScrVers versification = null)
		{
			var iBlock = m_blocks.IndexOf(blockToSplit);

			if (iBlock < 0)
				throw new ArgumentException(@"Block not found in the list for " + Id, "blockToSplit");

			int splitId;
			if (blockToSplit.SplitId != Block.kNotSplit)
				splitId = blockToSplit.SplitId;
			else
				splitId = m_blocks.Max(b => b.SplitId) + 1;

			if (verseToSplit == null && characterOffsetToSplit == 0)
			{
				SplitBeforeBlock(iBlock, splitId);
				return blockToSplit;
			}

			Block newBlock = blockToSplit.SplitBlock(verseToSplit, characterOffsetToSplit);
			if (newBlock == null)
			{
				SplitBeforeBlock(iBlock + 1, splitId);
				return m_blocks[iBlock + 1];
			}

			m_blocks.Insert(iBlock + 1, newBlock);
			OnBlocksInserted(iBlock + 1);

			if (userSplit)
			{
				newBlock.Delivery = null;
				if (string.IsNullOrEmpty(characterId))
				{
					newBlock.CharacterId = CharacterVerseData.kUnknownCharacter;
					newBlock.CharacterIdOverrideForScript = null;
					newBlock.UserConfirmed = false;
				}
				else
				{
					if (versification == null)
						throw new ArgumentNullException("versification");
					var bookNum = BCVRef.BookToNumber(Id);
					if (bookNum < 0)
						throw new InvalidOperationException("Attempting a user-originated split of a block which is not part of a known Scripture book. Possible characters cannot be determined.");
					newBlock.SetCharacterAndCharacterIdInScript(characterId, BCVRef.BookToNumber(Id), versification);
					newBlock.UserConfirmed = true;
				}

				if (blockToSplit.MultiBlockQuote == MultiBlockQuote.Start)
				{
					blockToSplit.MultiBlockQuote = MultiBlockQuote.None;
					newBlock.MultiBlockQuote = MultiBlockQuote.Start;
				}
				else if ((blockToSplit.MultiBlockQuote == MultiBlockQuote.Continuation || blockToSplit.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery) &&
					iBlock < m_blocks.Count - 2 &&
					(m_blocks[iBlock + 2].MultiBlockQuote == MultiBlockQuote.Continuation || m_blocks[iBlock + 2].MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery))
				{
					newBlock.MultiBlockQuote = MultiBlockQuote.Start;
				}

				blockToSplit.SplitId = newBlock.SplitId = splitId;
			}
			else if (blockToSplit.MultiBlockQuote != MultiBlockQuote.None)
				newBlock.MultiBlockQuote = MultiBlockQuote.Continuation;
			//TODO handle splitId already exists but userSplit == false

			return newBlock;
		}

		protected virtual void OnBlocksInserted(int insertionIndex, int countOfInsertedBlocks = 1)
		{
			// No op
		}

		public bool TrySplitBlockAtEndOfVerse(Block vernBlock, int verseNum)
		{
			var firstVerseElement = vernBlock.BlockElements.OfType<Verse>().FirstOrDefault();
			if (firstVerseElement == null)
				return false;
			var blockBeginsWithVerse = vernBlock.BlockElements.First() is Verse;
			var verseString = verseNum.ToString();

			if (vernBlock.InitialEndVerseNumber == verseNum)
			{
				verseString = vernBlock.InitialVerseNumberOrBridge;
				if (firstVerseElement.Number != verseString && blockBeginsWithVerse)
				{
					var secondPartOfVerse = vernBlock.BlockElements.Skip(2).OfType<Verse>().FirstOrDefault();
					if (secondPartOfVerse == null)
						return false;
					verseString = secondPartOfVerse.Number;
				}
			}
			else if (blockBeginsWithVerse ||
				!(vernBlock.InitialEndVerseNumber == 0 && vernBlock.InitialStartVerseNumber == verseNum))
			{
				foreach (var verse in vernBlock.BlockElements.OfType<Verse>())
				{
					if (verse.Number == verseString)
						break;
					if (verse.EndVerse == verseNum)
					{
						verseString = verse.Number;
						break;
					}
					if (verse.StartVerse >= verseNum)
						return false;
				}
			}
			try
			{
				var newBlock = SplitBlock(vernBlock, verseString, kSplitAtEndOfVerse, false);
				if (vernBlock.MatchesReferenceText)
				{
					// REVIEW: Should this be First or Single, or do we need to possibly handle the case of a sequence?
					// For now, at least, matching implies there is exactly one reference block.
					var refBlock = vernBlock.ReferenceBlocks.Single();
					try
					{
						newBlock.SetMatchedReferenceBlock(refBlock.SplitBlock(verseString, kSplitAtEndOfVerse));
					}
					catch (ArgumentException)
					{
						while (refBlock != null)
						{
							var lastVerseOfRefBlock = refBlock.LastVerse;
							var newRefBlock = new Block(newBlock.StyleTag, newBlock.ChapterNumber, lastVerseOfRefBlock.StartVerse, lastVerseOfRefBlock.EndVerse);
							newRefBlock.BlockElements.Add(new ScriptText(""));
							newBlock.SetMatchedReferenceBlock(newRefBlock);
							newBlock = newRefBlock;
							refBlock = refBlock.ReferenceBlocks.FirstOrDefault();
						}
					}
				}
			}
			catch (ArgumentException)
			{
				return false;
			}
			return true;
		}

		private void SplitBeforeBlock(int indexOfBlockToSplit, int splitId)
		{
			if (indexOfBlockToSplit == 0 || m_blocks[indexOfBlockToSplit].MultiBlockQuote == MultiBlockQuote.None || m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote == MultiBlockQuote.None)
				throw new ArgumentException("Split allowed only between blocks that are part of a multi-block quote");

			if (m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote == MultiBlockQuote.Start)
				m_blocks[indexOfBlockToSplit - 1].MultiBlockQuote = MultiBlockQuote.None;

			if (indexOfBlockToSplit < m_blocks.Count - 1 && m_blocks[indexOfBlockToSplit + 1].MultiBlockQuote == MultiBlockQuote.Continuation)
				m_blocks[indexOfBlockToSplit].MultiBlockQuote = MultiBlockQuote.Start;
			else
				m_blocks[indexOfBlockToSplit].MultiBlockQuote = MultiBlockQuote.None;

			m_blocks[indexOfBlockToSplit - 1].SplitId = m_blocks[indexOfBlockToSplit].SplitId = splitId;
		}
	}
}
