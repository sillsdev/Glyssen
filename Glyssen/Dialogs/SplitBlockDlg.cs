using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Glyssen.Utilities;
using GlyssenEngine.Utilities;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class SplitBlockDlg : FormWithPersistedSettings
	{
		private readonly SplitBlockViewModel m_model;
		private string m_htmlFilePath;

		// For purposes of splitting, anyway, we treat leading punctuation as if is permanently attached to the
		// beginning of the verse just as much as the verse number is. So the user may not split within
		// (or just after) the leading punctuation. This means the split index is always without regard to
		// the presence or absence of leading punctuation.
		// This was perhaps more of an ease-of-implementation decision rather than a decision made on its merits.
		private readonly Regex m_verseNumberRegex = new Regex(@"(" + Regex.Escape(SplitBlockViewModel.kLeadingPunctuationHtmlStart) + ".*?" + Regex.Escape(SplitBlockViewModel.kLeadingPunctuationHtmlEnd) + ")?<sup>.*?</sup>");

		public SplitBlockDlg(SplitBlockViewModel model)
		{
			m_model = model;

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

			SetHtml();
		}

		private void SetHtml()
		{
			File.WriteAllText(m_htmlFilePath, m_model.Html);
			m_blocksDisplayBrowser.Navigate(m_htmlFilePath);
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
					m_model.RemoveSplitLocation(splitId);
					SetHtml();
					m_btnOk.Enabled = m_model.SplitLocations.Any();
					m_lblInvalidSplitLocation.Visible = false;
				}
				else if (DetermineSplitLocation(geckoElement))
				{
					try
					{
						SetHtml();
						m_btnOk.Enabled = true;
						m_lblInvalidSplitLocation.Visible = false;
					}
					catch (Exception exception)
					{
						m_lblInvalidSplitLocation.Visible = true;
						Logger.WriteError(exception);
						m_model.RemoveLastSplitLocation();
					}

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
			if (geckoElement is GeckoHtmlElement geckoHtmlElement && geckoHtmlElement.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase) && geckoHtmlElement.ClassName.StartsWith("split-line"))
			{
				string splitIdStr = geckoHtmlElement.ClassName.Equals("split-line") ? geckoHtmlElement.Id : geckoHtmlElement.Parent.Id;
				string splitIdNumber = splitIdStr.Substring(SplitBlockViewModel.kSplitElementIdPrefix.Length);
				splitId = Int32.Parse(splitIdNumber);
				return true;
			}
			return false;
		}

		private bool DetermineSplitLocation(GeckoElement geckoElement)
		{
			var selection = m_blocksDisplayBrowser.Window.Selection;
			var newOffset = selection.AnchorOffset;

			if (geckoElement is GeckoHtmlElement targetElement)
			{
				// was a verse marker clicked?
				if (!targetElement.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase))
				{
					targetElement = targetElement.Parent;
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

				// if something else (not a "splittext" div), you cannot split here
				if (targetElement.ClassName != "splittext")
					return false;
			}
			else return false;

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
				// ENHANCE: Prevent splitting such that the new segment is nothing but punctuation, with the exception
				// of opening "brace" (parenthesis, square bracket, etc.) punctuation. (See test code in BlockTests for PG-1311.)
				else if (segmentText.Substring(newOffset).IsWhitespace())
				{
					newOffset = segmentText.Length;
				}
			}

			if (newOffset == 0)
			{
				var newTargetElement = targetElement.PreviousSibling as GeckoHtmlElement;

				// Can't split at start of first block.
				if (newTargetElement == null || newTargetElement.TagName != "div")
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

			return m_model.AddSplitIfNotDuplicate(blockIndex, verseToSplit, actualOffset);
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
		private int GetSplitIndexInVerse(int subOffset, int blockIndex, string verseToSplit, GeckoHtmlElement selectedDivElement, bool selectLastIndex=false)
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

					var textWithoutVerseNumbers = m_verseNumberRegex.Replace(previousElement.InnerHtml, "");
					var unencodedText = System.Web.HttpUtility.HtmlDecode(textWithoutVerseNumbers);
					sb.Append(unencodedText);
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
		private bool SplitIsAtEndOfLastBlock(int subOffset, int blockIndex, GeckoHtmlElement selectedDivElement)
		{
			// not if this is not the last block
			if (blockIndex < m_model.OriginalBlockCount - 1)
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

		public IReadOnlyList<BlockSplitData> SplitLocations => m_model.SplitLocations;

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
