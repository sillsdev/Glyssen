using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paratext;
using SIL.ScriptureUtils;

namespace ProtoScript
{
	public class QuoteParser
	{
		private readonly ICharacterVerseInfo m_cvInfo;
		private readonly string m_bookId;
		private readonly int m_bookNum;
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteSystem m_quoteSystem;

		#region working members
		// These members are used by several methods. Making them class-level prevents passing of references.
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		#endregion

		/// <summary>
		/// Create a QuoteParser using the default QuoteSystem
		/// </summary>
		/// <param name="bookId"></param>
		/// <param name="blocks"></param>
		public QuoteParser(ICharacterVerseInfo cvInfo, string bookId, IEnumerable<Block> blocks) : this(cvInfo, bookId, blocks, QuoteSystem.Default)
		{
		}

		public QuoteParser(ICharacterVerseInfo cvInfo, string bookId, IEnumerable<Block> blocks, QuoteSystem quoteSystem)
		{
			m_cvInfo = cvInfo;
			m_bookId = bookId;
			m_bookNum = BCVRef.BookToNumber(bookId);
			m_inputBlocks = blocks;
			m_quoteSystem = quoteSystem;
		}

		/// <summary>
		/// Parse through the given blocks character by character to determine where we need to break based on quotes 
		/// </summary>
		/// <returns>A new enumerable of blocks broken up for quotes</returns>
		public IEnumerable<Block> Parse()
		{
			if (m_quoteSystem == null)
				return m_inputBlocks;
			m_outputBlocks = new List<Block>();
			var sb = new StringBuilder();
			int quoteLevel = 0;
			bool quoteEndPending = false;
			foreach (Block block in m_inputBlocks)
			{
				if (block.CharacterId != Block.NotSet)
				{
					m_outputBlocks.Add(block);
					continue;
				}

				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialVerseNumber);
				
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						if (quoteEndPending)
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag, quoteLevel > 0);
							quoteEndPending = false;
						}

						// Add the element to our working list in case we need to move it to the next block (see MoveTrailingElementsIfNecessary)
						if (m_workingBlock.BlockElements.Any())
							m_nonScriptTextBlockElements.Add(element);

						m_workingBlock.BlockElements.Add(element);
						continue;
					}
					sb.Clear();
					foreach (char c in scriptText.Content)
					{
						string ch = Char.ToString(c);
						if (quoteEndPending)
						{
							if (!IsStartOfQuote(ch) && IsAddOnCharacter(c))
							{
								sb.Append(c);
								continue;
							}
							FlushStringBuilderAndBlock(sb, block.StyleTag, true);
							quoteEndPending = false;
						}
						if (quoteLevel == 0 && IsStartOfQuote(ch))
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag, quoteLevel > 0);
							sb.Append(c);
							quoteLevel++;
						}
						else if (IsEndOfQuote(ch))
						{
							sb.Append(c);
							quoteEndPending = true;
							quoteLevel--;
						}
						else
							sb.Append(c);
					}
					FlushStringBuilderToBlockElement(sb);
				}
				FlushBlock(block.StyleTag, quoteEndPending || quoteLevel > 0);
			}
			return m_outputBlocks;
		}

		/// <summary>
		/// Flush the current string builder to a block element
		/// </summary>
		/// <param name="sb"></param>
		private void FlushStringBuilderToBlockElement(StringBuilder sb)
		{
			if (sb.Length > 0)
			{
				MoveTrailingElementsIfNecessary();
				m_workingBlock.BlockElements.Add(new ScriptText(sb.ToString()));
				sb.Clear();
			}
		}

		/// <summary>
		/// Block elements which are not scriptText must not be the last elements in their block.
		/// Move them from the end of one block to the beginning of the next.
		/// </summary>
		private void MoveTrailingElementsIfNecessary()
		{
			if (m_outputBlocks.Any())
			{
				int numRemoved = m_outputBlocks.Last().BlockElements.RemoveAll(m_nonScriptTextBlockElements.Contains);
				if (numRemoved > 0)
				{
					var verse = m_nonScriptTextBlockElements.First() as Verse;
					if (verse != null)
						m_workingBlock.InitialVerseNumber = ScrReference.VerseToIntStart(verse.Number);
					m_workingBlock.BlockElements.InsertRange(0, m_nonScriptTextBlockElements);
				}
			}
			m_nonScriptTextBlockElements.Clear();
		}

		/// <summary>
		/// Flush the current string builder to a block element,
		/// and flush the current block elements to a block
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="styleTag"></param>
		/// <param name="inQuote"></param>
		private void FlushStringBuilderAndBlock(StringBuilder sb, string styleTag, bool inQuote)
		{
			FlushStringBuilderToBlockElement(sb);
			if (m_workingBlock.BlockElements.Count > 0)
			{
				FlushBlock(styleTag, inQuote);
			}
		}

		/// <summary>
		/// Add the working block to the new list and create a new working block
		/// </summary>
		/// <param name="styleTag"></param>
		/// <param name="inQuote"></param>
		private void FlushBlock(string styleTag, bool inQuote)
		{
			if (inQuote)
				m_workingBlock.SetCharacterAndDelivery(m_cvInfo.GetCharacters(m_bookId, m_workingBlock.ChapterNumber, m_workingBlock.InitialVerseNumber));
			else
				m_workingBlock.SetStandardCharacter(m_bookId, Block.StandardCharacter.Narrator);

			m_outputBlocks.Add(m_workingBlock);
			var lastVerse = m_workingBlock.BlockElements.OfType<Verse>().LastOrDefault();
			int verseNum = m_workingBlock.InitialVerseNumber;
			if (lastVerse != null)
				verseNum = ScrReference.VerseToIntStart(lastVerse.Number);
			m_workingBlock = new Block(styleTag, m_workingBlock.ChapterNumber, verseNum);
		}

		/// <summary>
		/// The given character should be attached to whatever text precedes it.
		/// For example, given this text:
		///		«Go»! he said.
		/// The first block should contain the trailing punctuation and whitespace:
		///		«Go»! 
		///		he said.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsAddOnCharacter(char character)
		{
			return Char.IsWhiteSpace(character) || Char.IsPunctuation(character);
		}

		/// <summary>
		/// The given string represents the beginning of a quote
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsStartOfQuote(string character)
		{
			return character.Equals(m_quoteSystem.StartQuoteMarker);
		}

		/// <summary>
		/// The given string represents the end of a quote
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsEndOfQuote(string character)
		{
			return character.Equals(m_quoteSystem.EndQuoteMarker);
		}
	}
	}
