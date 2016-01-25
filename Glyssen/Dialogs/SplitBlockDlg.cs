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
		public Block BlockToSplit { get; private set; }
		public string VerseToSplit { get; private set; }
		public int CharacterOffsetToSplit { get; private set; }
		private string m_htmlFilePath;

		public SplitBlockDlg(IWritingSystemDisplayInfo wsInfo, IEnumerable<Block> originalBlocks)
		{
			m_originalBlocks = originalBlocks.ToList();
			m_fontFamily = wsInfo.FontFamily;
			m_rightToLeftScript = wsInfo.RightToLeft;
			m_fontSize = wsInfo.FontSize;
			InitializeComponent();

			m_blocksDisplayBrowser.Disposed += m_blocksDisplayBrowser_Disposed;
		}

		void m_blocksDisplayBrowser_Disposed(object sender, EventArgs e)
		{
			if (m_htmlFilePath != null)
				File.Delete(m_htmlFilePath);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			m_htmlFilePath = Path.ChangeExtension(Path.GetTempFileName(), "htm");
			m_style = string.Format(Block.kCssFrame, m_fontFamily, m_fontSize) + "body {cursor:col-resize;}";

			SetHtml();
		}

		private void SetHtml()
		{
			const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
									"<style>{0}</style></head><body {1}>{2}</body></html>";

			var bldr = new StringBuilder();
			for (int index = 0; index < m_originalBlocks.Count; index++)
			{
				Block block = m_originalBlocks[index];
				bldr.Append(BuildHtml(block, index));
			}

			var bodyAttributes = m_rightToLeftScript ? "class=\"right-to-left\"" : "";
			File.WriteAllText(m_htmlFilePath, String.Format(kHtmlFrame, m_style, bodyAttributes, bldr));
			m_blocksDisplayBrowser.Navigate(m_htmlFilePath);
		}

		private string BuildHtml(Block block, int id)
		{
			var bldr = new StringBuilder();
			bldr.AppendFormat("<div id=\"{0}\" class=\"block\"", id);
			bldr.Append(">");
			if (BlockToSplit == block && VerseToSplit != null)
				bldr.Append(block.GetTextAsHtml(true, m_rightToLeftScript, VerseToSplit, CharacterOffsetToSplit, "<hr/>"));
			else
			{
				if (BlockToSplit == block && VerseToSplit == null)
				{
					Debug.Assert(CharacterOffsetToSplit == 0);
						bldr.Append("<hr/>");
				}
				bldr.Append(block.GetTextAsHtml(true, m_rightToLeftScript));
			}
			bldr.Append("</div>");
			return bldr.ToString();
		}

		private void InsertSplitLocation(object sender, DomMouseEventArgs e)
		{
			if (DetermineSplitLocation(e.Target))
			{
				SetHtml();
				m_btnOk.Enabled = true;
				m_lblInvalidSplitLocation.Visible = false;
			}
			else
				m_lblInvalidSplitLocation.Visible = true;
		}

		private bool DetermineSplitLocation(DomEventTarget target)
		{
			var selection = m_blocksDisplayBrowser.Window.Selection;
			var newOffset = selection.AnchorOffset;

			if (!m_blocksDisplayBrowser.Visible || target == null)
				return false;

			var geckoElement = target.CastToGeckoElement();
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

				VerseToSplit = null; // We're actually splitting between blocks of a multi-block quote
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
					VerseToSplit = null;
				}
				else
				{
					blockIndex = int.Parse(targetElement.Parent.Id);

					if (blockIndex == m_originalBlocks.Count - 1 && newOffset == selection.AnchorNode.NodeValue.Length)
						return false; // Can't split at very end of last verse in last block

					VerseToSplit = targetElement.Id;
					var childNodes = selection.AnchorNode.ParentNode.ChildNodes;

					if (childNodes.Length == 3 && selection.AnchorNode.Equals(childNodes[2]))
						newOffset += childNodes[0].NodeValue.Length;
				}
			}
			else
				return false;

			BlockToSplit = m_originalBlocks[blockIndex];
			CharacterOffsetToSplit = newOffset;
			return true;
		}

		private bool DetermineSplitLocationAtStartOfVerse(GeckoDivElement blockElement, string verseElementInnerHtml)
		{
			BlockToSplit = m_originalBlocks[int.Parse(blockElement.Id)];
			var ichThisVerse = blockElement.InnerHtml.IndexOf(verseElementInnerHtml, StringComparison.Ordinal);
			var ichPrecedingVerse = blockElement.InnerHtml.LastIndexOf("<sup>", ichThisVerse, StringComparison.Ordinal);
			ichPrecedingVerse = blockElement.InnerHtml.LastIndexOf("<sup>", ichPrecedingVerse, StringComparison.Ordinal);
			if (ichPrecedingVerse < 0)
			{
				VerseToSplit = BlockToSplit.InitialVerseNumberOrBridge;
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
				VerseToSplit = match.Result("${verse}");
			}
			CharacterOffsetToSplit = BookScript.kSplitAtEndOfVerse;
			return true;
		}
	}
}
