using System;
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

			var currVerse = blockToSplit.InitialVerseNumberOrBridge;

			Block newBlock = null;
			int indexOfFirstElementToRemove = -1;

			for (int i = 0; i < blockToSplit.BlockElements.Count; i++)
			{
				var blockElement = blockToSplit.BlockElements[i];

				if (newBlock != null)
				{
					newBlock.BlockElements.Add(blockElement);
					continue;
				}

				Verse verse = blockElement as Verse;
				if (verse != null)
					currVerse = verse.Number;
				else if (verseToSplit == currVerse)
				{
					ScriptText text = blockElement as ScriptText;

					string content;
					if (text == null)
					{
						if (blockToSplit.BlockElements.Count > i + 1 && blockToSplit.BlockElements[i + 1] is Verse)
						{
							content = string.Empty;
							characterOffsetToSplit = 0;
							indexOfFirstElementToRemove = i + 1;
						}
						else
							continue;
					}
					else
					{
						content = text.Content;

						if (blockToSplit.BlockElements.Count > i + 1)
						{
							if (!(blockToSplit.BlockElements[i + 1] is Verse) && (characterOffsetToSplit == kSplitAtEndOfVerse || characterOffsetToSplit > content.Length))
							{
								// Some kind of annotation. We can skip this. If we're splitting at
								continue;
							}
							indexOfFirstElementToRemove = i + 1;
						}

						if (characterOffsetToSplit == kSplitAtEndOfVerse)
							characterOffsetToSplit = content.Length;

						if (characterOffsetToSplit <= 0 || characterOffsetToSplit > content.Length)
						{
							throw new ArgumentOutOfRangeException("characterOffsetToSplit", characterOffsetToSplit,
								@"Value must be greater than 0 and less than or equal to the length (" + content.Length +
								@") of the text of verse " + currVerse + @".");
						}
						if (characterOffsetToSplit == content.Length && indexOfFirstElementToRemove < 0)
						{
							SplitBeforeBlock(iBlock + 1, splitId);
							return m_blocks[iBlock + 1];
						}
					}

					int initialStartVerse, initialEndVerse;
					if (characterOffsetToSplit == content.Length)
					{
						var firstVerseAfterSplit = ((Verse)blockToSplit.BlockElements[indexOfFirstElementToRemove]);
						initialStartVerse = firstVerseAfterSplit.StartVerse;
						initialEndVerse = firstVerseAfterSplit.EndVerse;
					}
					else
					{
						var verseNumParts = verseToSplit.Split(new[] { '-' }, 2, StringSplitOptions.None);
						initialStartVerse = int.Parse(verseNumParts[0]);
						initialEndVerse = verseNumParts.Length == 2 ? int.Parse(verseNumParts[1]) : 0;
					}
					newBlock = new Block(blockToSplit.StyleTag, blockToSplit.ChapterNumber,
						initialStartVerse, initialEndVerse);
					if (userSplit)
					{
						if (string.IsNullOrEmpty(characterId))
							newBlock.CharacterId = CharacterVerseData.kUnknownCharacter;
						else
						{
							if (versification == null)
								throw new ArgumentNullException("versification");
							newBlock.SetCharacterAndCharacterIdInScript(characterId, BCVRef.BookToNumber(Id), versification);
							newBlock.UserConfirmed = true;
						}
					}
					else
					{
						newBlock.CharacterId = blockToSplit.CharacterId;
						newBlock.CharacterIdOverrideForScript = blockToSplit.CharacterIdOverrideForScript;
						newBlock.Delivery = blockToSplit.Delivery;
						newBlock.UserConfirmed = blockToSplit.UserConfirmed;
					}
					if (characterOffsetToSplit < content.Length)
						newBlock.BlockElements.Add(new ScriptText(content.Substring(characterOffsetToSplit)));
					if (text != null)
						text.Content = content.Substring(0, characterOffsetToSplit);
					m_blocks.Insert(iBlock + 1, newBlock);
					OnBlocksInserted(iBlock + 1);

				}
			}

			if (newBlock == null)
				throw new ArgumentException(String.Format("Verse {0} not found in given block: {1}", verseToSplit, blockToSplit.GetText(true)), "verseToSplit");

			if (indexOfFirstElementToRemove >= 0)
			{
				while (indexOfFirstElementToRemove < blockToSplit.BlockElements.Count)
					blockToSplit.BlockElements.RemoveAt(indexOfFirstElementToRemove);
			}

			if (userSplit)
			{
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

		protected virtual void OnBlocksInserted(int insertionIndex)
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
				SplitBlock(vernBlock, verseString, kSplitAtEndOfVerse, false);
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
