using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProtoScript
{
	public class QuoteParser
	{
		private readonly IEnumerable<Block> m_inputBlocks;
		private readonly QuoteParserOptions m_options;

		#region working members
		// These members are used by several methods. Making them class-level prevents passing of references.
		private List<Block> m_outputBlocks;
		private Block m_workingBlock;
		private readonly List<BlockElement> m_nonScriptTextBlockElements = new List<BlockElement>();
		#endregion

		/// <summary>
		/// Create a QuoteParser using the default QuoteParserOptions
		/// </summary>
		/// <param name="blocks"></param>
		public QuoteParser(IEnumerable<Block> blocks)
		{
			m_inputBlocks = blocks;
			m_options = new QuoteParserOptions();
		}

		public QuoteParser(IEnumerable<Block> blocks, QuoteParserOptions options)
		{
			m_inputBlocks = blocks;
			m_options = options;
		}

		/// <summary>
		/// Parse through the given blocks character by character to determine where we need to break based on quotes 
		/// </summary>
		/// <returns>A new enumerable of blocks broken up for quotes</returns>
		public IEnumerable<Block> Parse()
		{
			m_outputBlocks = new List<Block>();
			var sb = new StringBuilder();
			int quoteLevel = 0;
			bool quoteEndPending = false;
			foreach (Block block in m_inputBlocks)
			{
				m_workingBlock = new Block(block.StyleTag, block.ChapterNumber, block.InitialVerseNumber);
				
				foreach (BlockElement element in block.BlockElements)
				{
					var scriptText = element as ScriptText;
					if (scriptText == null)
					{
						if (quoteEndPending)
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag);
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
							FlushStringBuilderAndBlock(sb, block.StyleTag);
							quoteEndPending = false;
						}
						if (quoteLevel == 0 && IsStartOfQuote(ch))
						{
							FlushStringBuilderAndBlock(sb, block.StyleTag);
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
				FlushBlock(block.StyleTag);
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
					{
						int verseNum;
						if (Int32.TryParse(verse.Number, out verseNum))
							m_workingBlock.InitialVerseNumber = verseNum;
						else
							Debug.Fail("TODO: Deal with bogus verse number in data!");
					}
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
		private void FlushStringBuilderAndBlock(StringBuilder sb, string styleTag)
		{
			FlushStringBuilderToBlockElement(sb);
			if (m_workingBlock.BlockElements.Count > 0)
				FlushBlock(styleTag);
		}

		/// <summary>
		/// Add the working block to the new list and create a new working block
		/// </summary>
		/// <param name="styleTag"></param>
		private void FlushBlock(string styleTag)
		{
			m_outputBlocks.Add(m_workingBlock);
			var lastVerse = m_workingBlock.BlockElements.OfType<Verse>().LastOrDefault();
			int verseNum = m_workingBlock.InitialVerseNumber;
			if (lastVerse != null)
			{
				if (!Int32.TryParse(lastVerse.Number, out verseNum))
				{
					Debug.Fail("TODO: Deal with bogus verse number in data!");
				}
			}
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
			return character.Equals(m_options.StartQuoteMarker);
		}

		/// <summary>
		/// The given string represents the end of a quote
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool IsEndOfQuote(string character)
		{
			return character.Equals(m_options.EndQuoteMarker);
		}
	}

	public class QuoteParserOptions
	{
		public string StartQuoteMarker = "«";
		public string EndQuoteMarker = "»";
	}
}
