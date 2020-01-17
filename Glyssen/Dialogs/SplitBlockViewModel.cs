using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Glyssen.Properties;
using Glyssen.Shared;
using GlyssenEngine.Character;
using GlyssenEngine.Utilities;
using static System.String;

namespace Glyssen.Dialogs
{
	public class SplitBlockViewModel
	{
		public const string kLeadingPunctuationHtmlStart = "<span class=\"leading-punctuation\">";
		public const string kLeadingPunctuationHtmlEnd = "</span>";
		public const string kSplitElementIdPrefix = "split";
		private const string kSplitLineFrame = "<div id=\"" + kSplitElementIdPrefix + "{0}\" class=\"split-line\"><div class=\"split-line-top\"></div></div>";

		private readonly IFontInfo m_font;
		private readonly List<Block> m_originalBlocks;
		private readonly string m_style;
		private readonly string m_css = Resources.BlockSplitCss;
		private string m_characterSelectFmt;
		private readonly List<BlockSplitData> m_splitLocations = new List<BlockSplitData>();
		private int m_blockSplitIdCounter = 1;  // zero is reserved for assigning a character id to the first segment
		/// <summary>Random string which will (hopefully) never appear in real text</summary>
		private const string kAwooga = "^~^";

		public SplitBlockViewModel(AssignCharacterViewModel assignCharacterViewModel, Block blockToSplit) :
			this(assignCharacterViewModel.Font, assignCharacterViewModel.GetAllBlocksWhichContinueTheQuoteStartedByBlock(blockToSplit),
				assignCharacterViewModel.GetUniqueCharactersForCurrentReference(), assignCharacterViewModel.CurrentBookId)
		{
		}

		public SplitBlockViewModel(IFontInfo fontProxy, IEnumerable<Block> originalBlocks,
			IEnumerable<ICharacter> charactersForCurrentReference, string currentBookId)
		{
			m_font = fontProxy;
			m_originalBlocks = originalBlocks.ToList();

			foreach (var block in m_originalBlocks)
			{
				if (block.BookCode == null)
					block.BookCode = currentBookId;
			}

			m_style = Format(Block.kCssFrame, m_font.FontFamily, m_font.Size) + m_css;
			SetHtmlSelectCodeForDropdown(currentBookId, charactersForCurrentReference);
		}

		public int OriginalBlockCount => m_originalBlocks.Count;
		public IReadOnlyList<BlockSplitData> SplitLocations => m_splitLocations;

		public string Html
		{
			get
			{
				const string htmlFrame = "<html><head><meta charset=\"UTF-8\">" +
					"<style>{0}</style></head><body {1}>{2}</body></html>";

				var bldr = new StringBuilder();
				for (int index = 0; index < m_originalBlocks.Count; index++)
				{
					Block block = m_originalBlocks[index];
					bldr.Append(BuildHtml(block, index));
				}

				var bodyAttributes = m_font.RightToLeftScript ? "class=\"right-to-left\"" : "";
				return  Format(htmlFrame, m_style, bodyAttributes, bldr);
			}
		}

		private void SetHtmlSelectCodeForDropdown(string bookId, IEnumerable<ICharacter> characters)
		{
			const string optionTemplate = "<option value=\"{0}\">{1}</option>";
			var sb = new StringBuilder("<select class=\"select-character\" data-splitid=\"{0}\"><option value=\"\"></option>");

			if (characters != null)
			{
				foreach (var character in characters)
				{
					if (character.IsNarrator)
					{
						sb.AppendFormat(optionTemplate,
							CharacterVerseData.GetStandardCharacterId(bookId, CharacterVerseData.StandardCharacter.Narrator),
							character.LocalizedDisplay);
					}
					else
					{
						var stdCharacterType = CharacterVerseData.GetStandardCharacterType(character.CharacterId);
						if (stdCharacterType == CharacterVerseData.StandardCharacter.NonStandard)
						{
							sb.AppendFormat(optionTemplate, character.CharacterId, character.LocalizedDisplay);
						}
						else
						{
							sb.AppendFormat(optionTemplate,
								CharacterVerseData.GetStandardCharacterId(bookId, stdCharacterType),
								character.LocalizedDisplay);
						}
					}
				}
			}

			sb.Append("</select>");
			m_characterSelectFmt = sb.ToString();
		}

		private string BuildHtml(Block block, int id)
		{
			var bldr = new StringBuilder();

			if (id == 0 && SplitLocations.Count != 0)
				bldr.Append(Format(m_characterSelectFmt, 0));
			List<BlockSplitData> splitLocationsForThisBlock = SplitLocations.Where(s => s.BlockToSplit == block).ToList();
			if (splitLocationsForThisBlock.Count > 0)
			{
				bool processedFirstBlock = false;
				if (splitLocationsForThisBlock[0].VerseToSplit == null)
				{
					Debug.Assert(splitLocationsForThisBlock[0].CharacterOffsetToSplit == 0);
					bldr.Append(BuildSplitLineHtml(splitLocationsForThisBlock[0].Id));
					bldr.Append(Format(m_characterSelectFmt, splitLocationsForThisBlock[0].Id));
					processedFirstBlock = true;
				}
				bldr.Append(GetSplitTextAsHtml(block, id, m_font.RightToLeftScript, splitLocationsForThisBlock.Skip(processedFirstBlock ? 1 : 0).ToList(), Format(m_characterSelectFmt, id)));
			}
			else
			{
				bldr.Append(GetSplitTextAsHtml(block, id, m_font.RightToLeftScript, null, Format(m_characterSelectFmt, id)));
			}

			return bldr.ToString();
		}

		internal string GetSplitTextAsHtml(Block block, int blockId, bool rightToLeftScript, IReadOnlyCollection<BlockSplitData> blockSplits, string characterSelectCode = null)
		{
			var bldr = new StringBuilder();
			var currVerse = block.InitialVerseNumberOrBridge;
			var verseNumberHtml = Empty;
			string leadingPunctuationHtml = null;
			const string splitTextTemplate = "<div class=\"splittext\" data-blockid=\"{2}\" data-verse=\"{3}\">{4}{0}{1}</div>";
			const string leadingPunctuationTemplate = kLeadingPunctuationHtmlStart + "{0}" + kLeadingPunctuationHtmlEnd;

			// Look for special case where verse has leading punctuation before the verse number such as
			// ({1} This verse is surrounded by parentheses)
			// This can only happen at the beginning of a block.
			// If we have it, we basically want to do the split as if it wasn't there at all. i.e. Split the main part of the verse only, and
			// do not include the leading punctuation as part of the offset.
			var leadingPunctuation = block.GetLeadingPunctuation();
			if (leadingPunctuation != null)
				leadingPunctuationHtml = Format(leadingPunctuationTemplate, leadingPunctuation);

			foreach (var blockElement in block.BlockElements.Skip(leadingPunctuationHtml != null ? 1 : 0))
			{
				// add verse marker
				if (blockElement is Verse verse)
				{
					verseNumberHtml = block.BuildVerseNumber(verse.Number, rightToLeftScript);
					currVerse = verse.Number;
					continue;
				}

				// add verse text
				var text = blockElement as ScriptText;
				if (text == null) continue;

				var encodedContent = Format(splitTextTemplate, verseNumberHtml, HttpUtility.HtmlEncode(text.Content), blockId, currVerse, leadingPunctuationHtml);

				if (blockSplits != null && blockSplits.Any())
				{
					var preEncodedContent = text.Content;

					var allContentToInsert = new List<string>();
					foreach (var groupOfSplits in blockSplits.GroupBy(s => new { s.BlockToSplit, s.VerseToSplit }))
					{

						var sortedGroupOfSplits = groupOfSplits.OrderByDescending(s => s, BlockSplitData.BlockSplitDataOffsetComparer);
						foreach (var blockSplit in sortedGroupOfSplits)
						{
							var offsetToInsertExtra = blockSplit.CharacterOffsetToSplit;
							if (blockSplit.VerseToSplit == currVerse)
							{
								if (offsetToInsertExtra == PortionScript.kSplitAtEndOfVerse)
									offsetToInsertExtra = preEncodedContent.Length;

								if (offsetToInsertExtra < 0 || offsetToInsertExtra > preEncodedContent.Length)
								{
									throw new IndexOutOfRangeException("Value of offsetToInsertExtra must be greater than or equal to 0 and less " +
																	   $"than or equal to the length ({preEncodedContent.Length}) of the content of verse {currVerse}");
								}

								allContentToInsert.Insert(0, BuildSplitLineHtml(blockSplit.Id) + characterSelectCode);
								preEncodedContent = preEncodedContent.Insert(offsetToInsertExtra, kAwooga);
							}
						}
					}

					if (preEncodedContent != text.Content)
					{
						encodedContent = HttpUtility.HtmlEncode(preEncodedContent);

						// wrap each text segment in a splittext div
						var segments = encodedContent.Split(new[] { kAwooga }, StringSplitOptions.None);
						var newSegments = new List<string>();
						foreach (var segment in segments)
						{
							newSegments.Add(Format(splitTextTemplate, verseNumberHtml, segment, blockId, currVerse, leadingPunctuationHtml));
							verseNumberHtml = Empty;
							leadingPunctuationHtml = null;
						}

						encodedContent = Join(kAwooga, newSegments);

						foreach (var contentToInsert in allContentToInsert)
							encodedContent = encodedContent.ReplaceFirst(kAwooga, contentToInsert);
					}
				}

				bldr.Append(encodedContent);

				// reset verse number element
				verseNumberHtml = Empty;

				leadingPunctuationHtml = null;
			}

			return bldr.ToString();
		}

		internal static string BuildSplitLineHtml(int id)
		{
			return Format(kSplitLineFrame, id);
		}

		public void RemoveLastSplitLocation()
		{
			m_splitLocations.Remove(m_splitLocations.Last());
		}

		public void RemoveSplitLocation(int splitId)
		{
			m_splitLocations.Remove(m_splitLocations.Single(s => s.Id == splitId));
		}

		public bool AddSplitIfNotDuplicate(int blockIndex, string verseToSplit, int actualOffset)
		{
			// check for duplicate splits
			if (IsDuplicateSplit(blockIndex, verseToSplit, actualOffset))
				return false;

			var blockSplitData = new BlockSplitData(m_blockSplitIdCounter++, m_originalBlocks[blockIndex], verseToSplit, actualOffset);
			m_splitLocations.Add(blockSplitData);
			return true;
		}

		private bool IsDuplicateSplit(int blockIndex, string verseNumberStr, int characterOffest)
		{
			// if this is the first split, it isn't a duplicate
			if (m_splitLocations.Count == 0)
				return false;

			// check for 2 splits in same location
			if (m_splitLocations.Any(splitLocation => (splitLocation.BlockToSplit == m_originalBlocks[blockIndex])
				&& (splitLocation.VerseToSplit == verseNumberStr)
				&& (splitLocation.CharacterOffsetToSplit == characterOffest)))
			{
				return true;
			}

			return false;
		}
	}
}
