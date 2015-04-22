using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using ProtoScript.Properties;

namespace ProtoScript.Dialogs
{
	public partial class SplitBlockDlg : Form
	{
		private readonly List<Block> m_originalBlocks;
		private readonly string m_fontFamily;
		private readonly int m_fontSize;
		private readonly bool m_rightToLeftScript;
		public Block BlockToSplit { get; private set; }
		public string VerseToSplit { get; private set; }
		public int CharacterOffsetToSplit { get; private set; }
		private string m_htmlFilePath;
		private readonly string m_breakIconFilePath;
		private readonly string m_imageElement;

		public SplitBlockDlg(IWritingSystemDisplayInfo wsInfo, IEnumerable<Block> originalBlocks)
		{
			m_originalBlocks = originalBlocks.ToList();
			m_fontFamily = wsInfo.FontFamily;
			m_rightToLeftScript = wsInfo.RightToLeft;
			m_fontSize = wsInfo.FontSize;
			InitializeComponent();

			m_breakIconFilePath = Path.GetTempFileName();
			Resources.SplitLocation.Save(m_breakIconFilePath, ImageFormat.Png);
			m_imageElement = String.Format(@"<img src=""file://{0}""/>", m_breakIconFilePath);

			m_blocksDisplayBrowser.Disposed += m_blocksDisplayBrowser_Disposed;
		}

		void m_blocksDisplayBrowser_Disposed(object sender, EventArgs e)
		{
			if (m_breakIconFilePath != null)
				File.Delete(m_breakIconFilePath);
			if (m_htmlFilePath != null)
				File.Delete(m_htmlFilePath);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			m_htmlFilePath = Path.GetTempFileName();
			SetHtml();
			//using (var stream = new MemoryStream(Resources.split_point))
			//	m_blocksDisplayBrowser.Cursor = new Cursor(stream);
		}

		private void SetHtml()
		{
			var bldr = new StringBuilder();
			for (int index = 0; index < m_originalBlocks.Count; index++)
			{
				Block block = m_originalBlocks[index];
				bldr.Append(BuildHtml(block, index));
			}

			File.WriteAllText(m_htmlFilePath, bldr.ToString());
			m_blocksDisplayBrowser.Navigate(m_htmlFilePath);
		}

		private string BuildHtml(Block block, int id)
		{
			const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
									"<style>{0}</style></head><body {1}>{2}</body></html>";

			var bldr = new StringBuilder();
			bldr.AppendFormat("<div id=\"{0}\" class=\"block\"", id);
			bldr.Append(">");
			if (BlockToSplit == block && VerseToSplit != null)
				bldr.Append(block.GetTextAsHtml(true, VerseToSplit, CharacterOffsetToSplit, m_imageElement));
			else
			{
				if (BlockToSplit == block && VerseToSplit == null)
				{
					Debug.Assert(CharacterOffsetToSplit == 0);
						bldr.Append("<hr/>");
				}
				bldr.Append(block.GetTextAsHtml(true));
			}
			bldr.Append("</div>");
			var style = string.Format(Block.kCssFrame, m_fontFamily, m_fontSize);
			var bodyAttributes = m_rightToLeftScript ? "class=\"right-to-left\"" : "";
			return String.Format(kHtmlFrame, style, bodyAttributes, bldr);
		}

		private void InsertSplitLocation(object sender, DomMouseEventArgs e)
		{
			var selection = m_blocksDisplayBrowser.Window.Selection;
			var newOffset =  selection.AnchorOffset;

			if (!m_blocksDisplayBrowser.Visible || e.Target == null)
				return;

			var targetElement = e.Target.CastToGeckoElement() as GeckoDivElement;
			if (targetElement == null)
				return;

			int blockIndex;

			if (targetElement.ClassName == "block")
			{
				blockIndex = int.Parse(targetElement.Id);
				if (newOffset > 0)
				{
					// For simplicity, make it so a split at the end of a block is really a split at the start of the following block.
					if (++blockIndex == m_originalBlocks.Count)
						return; // Can't split at very end of last verse in last block
					newOffset = 0;
				}
				else if (blockIndex == 0)
					return; // Can't split at start of first block.

				VerseToSplit = null; // We're actually splitting between blocks of a multi-block quote
			}
			else if (targetElement.ClassName == "scripttext")
			{
				if (newOffset == 0)
				{
					var indexInBlock = targetElement.ParentElement.ChildNodes.IndexOf(targetElement);
					if (indexInBlock > 0)
						return;
					VerseToSplit = null;
					blockIndex = int.Parse(targetElement.Parent.Id);
				}
				else
				{
					blockIndex = int.Parse(targetElement.Parent.Id);

					if (blockIndex == m_originalBlocks.Count - 1 && newOffset == selection.AnchorNode.NodeValue.Length)
						return; // Can't split at very end of last verse in last block

					VerseToSplit = targetElement.Id;
					var childNodes = selection.AnchorNode.ParentNode.ChildNodes;

					if (childNodes.Length == 3 && selection.AnchorNode.Equals(childNodes[2]))
						newOffset += childNodes[0].NodeValue.Length;
				}
			}
			else
				return;

			BlockToSplit = m_originalBlocks[blockIndex];

			CharacterOffsetToSplit = newOffset;
			
			SetHtml();

			m_btnOk.Enabled = true;
		}
	}
}
