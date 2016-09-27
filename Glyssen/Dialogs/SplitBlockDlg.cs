﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Glyssen.Properties;
using Glyssen.Utilities;

namespace Glyssen.Dialogs
{
	public partial class SplitBlockDlg : FormWithPersistedSettings
	{
		private string m_style;
		private readonly List<Block> m_originalBlocks;
		private readonly string m_fontFamily;
		private readonly int m_fontSize;
		private readonly bool m_rightToLeftScript;
		private readonly List<BlockSplitData> m_splitLocations = new List<BlockSplitData>();
		private string m_htmlFilePath;
		private int m_blockSplitIdCounter = 1;  // zero is reserved for assigning a character id to the first segment
		private readonly IEnumerable<AssignCharacterViewModel.Character> m_characters;
		private readonly string m_css = Resources.BlockSplitCss;
		private readonly Regex m_verseNumberRegex = new Regex(@"<sup>.*</sup>");

		public SplitBlockDlg(IWritingSystemDisplayInfo wsInfo, IEnumerable<Block> originalBlocks)
		{
			m_originalBlocks = originalBlocks.ToList();
			m_fontFamily = wsInfo.FontFamily;
			m_rightToLeftScript = wsInfo.RightToLeft;
			m_fontSize = wsInfo.FontSize;

			// if splitting blocks, get list of potential characters
			var viewModel = wsInfo as AssignCharacterViewModel;
			if (viewModel != null)
			{
				m_characters = viewModel.GetUniqueCharactersForCurrentReference();
				foreach (var block in m_originalBlocks)
				{
					if (block.BookCode == null)
					{
						block.BookCode = viewModel.CurrentBookId;
					}
				}
			}

			InitializeComponent();

			m_blocksDisplayBrowser.Disposed += BlocksDisplayBrowser_Disposed;
		}

		void BlocksDisplayBrowser_Disposed(object sender, EventArgs e)
		{
			if (m_htmlFilePath != null)
				File.Delete(m_htmlFilePath);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			m_htmlFilePath = Path.ChangeExtension(Path.GetTempFileName(), "htm");
			m_style = string.Format(Block.kCssFrame, m_fontFamily, m_fontSize) + m_css;

			SetHtml();
		}

		private void SetHtml()
		{
			const string htmlFrame = "<html><head><meta charset=\"UTF-8\">" +
									"<style>{0}</style></head><body {1}>{2}</body></html>";

			var bldr = new StringBuilder();
			for (int index = 0; index < m_originalBlocks.Count; index++)
			{
				Block block = m_originalBlocks[index];
				bldr.Append(BuildHtml(block, index));
			}

			var bodyAttributes = m_rightToLeftScript ? "class=\"right-to-left\"" : "";
			File.WriteAllText(m_htmlFilePath, String.Format(htmlFrame, m_style, bodyAttributes, bldr));
			m_blocksDisplayBrowser.Navigate(m_htmlFilePath);
		}

		private string BuildHtml(Block block, int id)
		{
			var bldr = new StringBuilder();

			if ((id == 0) && (SplitLocations.Count != 0))
				bldr.Append(block.CharacterSelect(0, m_characters));
			List<BlockSplitData> splitLocationsForThisBlock = SplitLocations.Where(s => s.BlockToSplit == block).ToList();
			if (splitLocationsForThisBlock.Count > 0)
			{
				bool processedFirstBlock = false;
				if (splitLocationsForThisBlock[0].VerseToSplit == null)
				{
					Debug.Assert(splitLocationsForThisBlock[0].CharacterOffsetToSplit == 0);
					bldr.Append(Block.BuildSplitLineHtml(splitLocationsForThisBlock[0].Id));
					bldr.Append(block.CharacterSelect(splitLocationsForThisBlock[0].Id));
					processedFirstBlock = true;
				}
				bldr.Append(block.GetSplitTextAsHtml(id, m_rightToLeftScript, splitLocationsForThisBlock.Skip(processedFirstBlock ? 1 : 0).ToList(), true));
			}
			else
			{
				bldr.Append(block.GetSplitTextAsHtml(id, m_rightToLeftScript, null, true));
			}

			return bldr.ToString();
		}

		private void HandleClick(object sender, DomMouseEventArgs e)
		{
			GeckoElement geckoElement;
			if (m_blocksDisplayBrowser.Visible && GeckoUtilities.ParseDomEventTargetAsGeckoElement(e.Target, out geckoElement))
			{
				int splitId;
				if (IsElementSelect(geckoElement))
				{
					m_lblInvalidSplitLocation.Visible = false;
				}
				else if (IsElementSplitLine(geckoElement, out splitId))
				{
					m_splitLocations.Remove(m_splitLocations.Single(s => s.Id == splitId));
					SetHtml();
					m_btnOk.Enabled = m_splitLocations.Any();
					m_lblInvalidSplitLocation.Visible = false;
				}
				else if (DetermineSplitLocation(geckoElement))
				{
					SetHtml();
					m_btnOk.Enabled = true;
					m_lblInvalidSplitLocation.Visible = false;
				}
				else
					m_lblInvalidSplitLocation.Visible = true;
			}
			else
				m_lblInvalidSplitLocation.Visible = true;
		}

		private static bool IsElementSelect(GeckoElement geckoElement)
		{
			var geckoSelectElement = geckoElement as GeckoSelectElement;
			if (geckoSelectElement != null)
				return true;

			var geckoOptionElement = geckoElement as GeckoOptionElement;
			return geckoOptionElement != null;
		}

		private static bool IsElementSplitLine(GeckoElement geckoElement, out int splitId)
		{
			splitId = -1;
			var geckoDivElement = geckoElement as GeckoDivElement;
			if (geckoDivElement != null && geckoDivElement.ClassName.StartsWith("split-line"))
			{
				string splitIdStr = geckoDivElement.ClassName.Equals("split-line") ? geckoDivElement.Id : geckoDivElement.Parent.Id;
				string splitIdNumber = splitIdStr.Substring(Block.kSplitElementIdPrefix.Length);
				splitId = Int32.Parse(splitIdNumber);
				return true;
			}
			return false;
		}

		private bool DetermineSplitLocation(GeckoElement geckoElement)
		{
			var selection = m_blocksDisplayBrowser.Window.Selection;
			var newOffset = selection.AnchorOffset;

			var targetElement = geckoElement as GeckoDivElement;

			// was a verse marker clicked?
			if (targetElement == null)
			{
				var geckoHtmlElement = geckoElement as GeckoHtmlElement;
				if (geckoHtmlElement != null)
				{
					targetElement = geckoHtmlElement.Parent as GeckoDivElement;
					if (targetElement == null)
						return false;

					if (geckoElement.TagName == "SUP")
					{
						if (newOffset != 0)
							return false;
					}
					else
						newOffset = 0;
				}
				else
					return false;
			}

			// if something else (not a "splittext" div), you cannot split here
			if (targetElement.ClassName != "splittext")
				return false;

			var verseToSplit = targetElement.GetAttribute("data-verse");
			var blockIndex = int.Parse(targetElement.GetAttribute("data-blockid"));
			var splitAtEnd = false;

			// check for potential new empty segment
			if (newOffset != 0)
			{
				var segmentText = m_verseNumberRegex.Replace(targetElement.InnerHtml, "");
				if (segmentText.Substring(0, newOffset).IsWhitespace())
				{
					newOffset = 0;
				}
				else if (segmentText.Substring(newOffset).IsWhitespace())
				{
					newOffset = segmentText.Length;
				}
			}
			
			if (newOffset == 0)
			{
				var newTargetElement = targetElement.PreviousSibling as GeckoDivElement;

				// Can't split at start of first block.
				if (newTargetElement == null)
					return false; 

				// if the previous sibling is a split, this is a duplicate
				if (newTargetElement.ClassName == "split-line")
					return false;

				// the split is at the beginning of a segment, move it to the end of the previous segment
				targetElement = newTargetElement;
				blockIndex = int.Parse(targetElement.GetAttribute("data-blockid"));
				splitAtEnd = true;
				verseToSplit = targetElement.GetAttribute("data-verse");
			}
			else
			{
				if (SplitIsAtEndOfLastBlock(newOffset, blockIndex, targetElement))
					return false;
			}

			// calculate the offset from the beginning of the block
			var actualOffset = GetSplitIndexInVerse(newOffset, blockIndex, verseToSplit, targetElement, splitAtEnd);

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

		// ReSharper disable once SuggestBaseTypeForParameter
		/// <summary>
		/// Given the split location in the selected segment, calculate the split location in the original block
		/// </summary>
		/// <param name="subOffset">The index that the user selected</param>
		/// <param name="blockIndex">The index of the block containing the selected segment</param>
		/// <param name="verseToSplit">The verse number to look for, if null, search the whole block</param>
		/// <param name="selectedDivElement">The selected segment</param>
		/// <param name="selectLastIndex">If true, ignore the subOffset and return the last position in the block</param>
		/// <returns></returns>
		private int GetSplitIndexInVerse(int subOffset, int blockIndex, string verseToSplit, GeckoDivElement selectedDivElement, bool selectLastIndex=false)
		{
			// get the text from the previous splittext elements in this block
			var sb = new StringBuilder();
			var previousElement = selectedDivElement.PreviousSibling as GeckoHtmlElement;
			while (previousElement != null)
			{
				if (previousElement.ClassName == "splittext")
				{
					// stop looking if we've moved to a different block
					if (int.Parse(previousElement.GetAttribute("data-blockid")) != blockIndex)
						break;

					// stop looking if we've moved to a different verse
					if (previousElement.GetAttribute("data-verse") != verseToSplit)
						break;

					sb.Append(m_verseNumberRegex.Replace(previousElement.InnerHtml, ""));
				}
				previousElement = previousElement.PreviousSibling as GeckoHtmlElement;
			}

			// get the part of the current element text that is included
			var lastSegement = m_verseNumberRegex.Replace(selectedDivElement.InnerHtml, "");

			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (selectLastIndex)
				sb.Append(lastSegement);
			else
				sb.Append(lastSegement.Substring(0, subOffset));

			return sb.Length;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		/// <summary>
		/// Is the selected offset at the end of the last block
		/// </summary>
		/// <param name="subOffset">The index that the user selected</param>
		/// <param name="blockIndex">The index of the block containing the selected segment</param>
		/// <param name="selectedDivElement">The selected segment</param>
		/// <returns></returns>
		private bool SplitIsAtEndOfLastBlock(int subOffset, int blockIndex, GeckoDivElement selectedDivElement)
		{
			// not if this is not the last block
			if (blockIndex < m_originalBlocks.Count - 1)
				return false;

			// not if offset is not at end of segment
			var segmentText = m_verseNumberRegex.Replace(selectedDivElement.InnerHtml, "");
			if (segmentText.Length > subOffset)
				return false;

			// this is the last block, and offset is at the end of a segment
			var nextElement = selectedDivElement.NextSibling as GeckoHtmlElement;
			while (nextElement != null)
			{
				// if the next element is a splittext div, the current offset is not at the end of the last block
				if (nextElement.ClassName == "splittext")
					return false;

				nextElement = nextElement.NextSibling as GeckoHtmlElement;
			}

			return true;
		}

		public IReadOnlyList<BlockSplitData> SplitLocations
		{
			get { return m_splitLocations; }
		}

		private void SplitBlockDlg_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK) return;

			// get a list of the character assignment dropdowns if DialogResult == OK
			SelectedCharacters = new List<KeyValuePair<int, string>>();
			var elements = m_blocksDisplayBrowser.Window.Document.GetElementsByTagName("select");

			foreach (var selectElement in elements.Where(element => element.ClassName.Contains("select-character")))
			{
				var element = (GeckoSelectElement)selectElement;
				var splitId = int.Parse(element.GetAttribute("data-splitid"));
				var characterId = element.Value;

				SelectedCharacters.Add(new KeyValuePair<int, string>(splitId, characterId));
			}
		}

		public List<KeyValuePair<int, string>> SelectedCharacters { get; private set; }
	}
}
