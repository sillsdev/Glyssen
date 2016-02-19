using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gecko;
using Gecko.DOM;
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
		public IReadOnlyList<BlockSplitData> SplitLocations { get { return m_splitLocations; } }
		private string m_htmlFilePath;
		private int m_blockSplitIdCounter;

		private const string Css = "body {cursor:col-resize;} .split-line {cursor:url(DeleteCursor.png),not-allowed;height:21px;width:100%;} .split-line-top {height:10px;border-bottom: 1px solid gray;}";

		public SplitBlockDlg(IWritingSystemDisplayInfo wsInfo, IEnumerable<Block> originalBlocks)
		{
			m_originalBlocks = originalBlocks.ToList();
			m_fontFamily = wsInfo.FontFamily;
			m_rightToLeftScript = wsInfo.RightToLeft;
			m_fontSize = wsInfo.FontSize;
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
			m_style = string.Format(Block.kCssFrame, m_fontFamily, m_fontSize) + Css;

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
			bldr.AppendFormat("<div id=\"{0}\" class=\"block\">", id);
			List<BlockSplitData> splitLocationsForThisBlock = SplitLocations.Where(s => s.BlockToSplit == block).ToList();
			if (splitLocationsForThisBlock.Count > 0)
			{
				bool processedFirstBlock = false;
				if (splitLocationsForThisBlock[0].VerseToSplit == null)
				{
					Debug.Assert(splitLocationsForThisBlock[0].CharacterOffsetToSplit == 0);
					bldr.Append(Block.BuildSplitLineHtml(splitLocationsForThisBlock[0].Id));
					processedFirstBlock = true;
				}
				bldr.Append(block.GetTextAsHtml(true, m_rightToLeftScript, splitLocationsForThisBlock.Skip(processedFirstBlock ? 1 : 0)));
			}
			else
			{
				bldr.Append(block.GetTextAsHtml(true, m_rightToLeftScript));
			}
			bldr.Append("</div>");
			return bldr.ToString();
		}

		private void HandleClick(object sender, DomMouseEventArgs e)
		{
			GeckoElement geckoElement;
			if (m_blocksDisplayBrowser.Visible && GeckoUtilities.ParseDomEventTargetAsGeckoElement(e.Target, out geckoElement))
			{
				int splitId;
				if (IsElementSplitLine(geckoElement, out splitId))
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

		private bool IsElementSplitLine(GeckoElement geckoElement, out int splitId)
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
						if (!targetElement.InnerHtml.StartsWith(geckoHtmlElement.OuterHtml))
							return DetermineSplitLocationAtStartOfVerse(targetElement, geckoHtmlElement.InnerHtml);
					}
					else
						newOffset = 0;
				}
				else
					return false;
			}

			int blockIndex;

			string verseToSplit;
			if (targetElement.ClassName == "block")
			{
				blockIndex = int.Parse(targetElement.Id);
				if (newOffset > 0)
				{
					// For simplicity, make it so a split at the end of a block is really a split at the start of the following block.
					if (++blockIndex == m_originalBlocks.Count)
						return false; // Can't split at very end of last verse in last block
					newOffset = 0;
				}
				else if (blockIndex == 0)
					return false; // Can't split at start of first block.

				verseToSplit = null; // We're actually splitting between blocks of a multi-block quote
			}
			else if (targetElement.ClassName == "scripttext")
			{
				if (newOffset == 0)
				{
					var indexInBlock = targetElement.ParentElement.ChildNodes.IndexOf(targetElement);
					if (indexInBlock > 0)
						return false;
					blockIndex = int.Parse(targetElement.Parent.Id);
					if (blockIndex == 0)
						return false; // Can't split at start of first block.
					verseToSplit = null;
				}
				else
				{
					blockIndex = int.Parse(targetElement.Parent.Id);

					if (blockIndex == m_originalBlocks.Count - 1 && newOffset == selection.AnchorNode.NodeValue.Length)
						return false; // Can't split at very end of last verse in last block

					verseToSplit = targetElement.Id;
					var childNodes = selection.AnchorNode.ParentNode.ChildNodes;

					if (childNodes.Length > 1)
					{
						foreach (var childNode in childNodes)
						{
							if (childNode.Equals(selection.AnchorNode))
								break;
							if (childNode.NodeType == NodeType.Text)
								newOffset += childNode.NodeValue.Length;
						}
					}
				}
			}
			else
				return false;

			BlockSplitData blockSplitData = new BlockSplitData(m_blockSplitIdCounter++, m_originalBlocks[blockIndex], verseToSplit, newOffset);
			m_splitLocations.Add(blockSplitData);
			return true;
		}

		private bool DetermineSplitLocationAtStartOfVerse(GeckoDivElement blockElement, string verseElementInnerHtml)
		{
			BlockSplitData blockSplitData = new BlockSplitData(m_blockSplitIdCounter++);
			blockSplitData.BlockToSplit = m_originalBlocks[int.Parse(blockElement.Id)];
			var ichThisVerse = blockElement.InnerHtml.IndexOf(verseElementInnerHtml, StringComparison.Ordinal);
			var ichPrecedingVerse = blockElement.InnerHtml.LastIndexOf("<sup>", ichThisVerse, StringComparison.Ordinal);
			ichPrecedingVerse = blockElement.InnerHtml.LastIndexOf("<sup>", ichPrecedingVerse, StringComparison.Ordinal);
			if (ichPrecedingVerse < 0)
			{
				blockSplitData.VerseToSplit = blockSplitData.BlockToSplit.InitialVerseNumberOrBridge;
			}
			else
			{
				var regexVerse = new Regex(@"\<sup\>(&rlm;)?(?<verse>[0-9-]+)*((&#160;)|(&nbsp;))");
				var match = regexVerse.Match(blockElement.InnerHtml, ichPrecedingVerse);
				if (!match.Success)
				{
					Debug.Fail("HTML data for verse number not formed as expected");
					// ReSharper disable once HeuristicUnreachableCode
					return false;
				}
				blockSplitData.VerseToSplit = match.Result("${verse}");
			}
			blockSplitData.CharacterOffsetToSplit = BookScript.kSplitAtEndOfVerse;
			m_splitLocations.Add(blockSplitData);
			return true;
		}
	}
}
