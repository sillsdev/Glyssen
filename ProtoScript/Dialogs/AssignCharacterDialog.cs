using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using L10NSharp;
using L10NSharp.UI;
using SIL.ScriptureControls;
using SIL.ScriptureUtils;

namespace ProtoScript.Dialogs
{
	public partial class AssignCharacterDialog : Form
	{
		private string m_narrator;
		private string m_normalDelivery;
		private const string kMainQuoteElementId = "main-quote-text";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
										  "<style>{0}</style></head><body>{1}</body></html>";
		private const string kHtmlLineBreak = "<div class='block-spacer'></div>";
		private const string kCssClassContext = "context";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
		                                 ".highlight{{background-color:yellow}}" +
		                                 "." + kCssClassContext + ":hover{{background-color:#FFFFA0}}" +
		                                 ".block-spacer{{height:30px}}";

		private readonly string m_fontFamily;
		private readonly int m_fontSizeInPoints;
		private readonly BlockNavigator m_navigator;
		private List<Tuple<int, int>> m_relevantBlocks;
		private int m_assignedBlocks;
		private int m_displayBlockIndex;
		private IEnumerable<CharacterVerse> m_characters;
		private IEnumerable<string> m_deliveries;
		private bool m_showVerseNumbers = true; // May make this configurable later
		private ToolTip m_toolTip;

		public AssignCharacterDialog()
		{
			InitializeComponent();
			HandleStringsLocalized();

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
		}

		private void HandleStringsLocalized()
		{
			m_narrator = LocalizationManager.GetString("AssignCharacterDialog.Narrator", "Narrator");
			m_normalDelivery = LocalizationManager.GetString("AssignCharacterDialog.NormalDelivery", "normal");
		}

		public AssignCharacterDialog(Project project) : this()
		{
			m_blocksDisplayBrowser.OnMouseOver += OnMouseOver;
			m_blocksDisplayBrowser.OnMouseOut += OnMouseOut;
			m_blocksDisplayBrowser.OnDocumentCompleted += OnDocumentCompleted;

			m_fontFamily = project.FontFamily;
			m_fontSizeInPoints = project.FontSizeInPoints;

			m_navigator = new BlockNavigator(project.IncludedBooks);

			PopulateRelevantBlocks();
			m_progressBar.Maximum = m_relevantBlocks.Count;
			m_progressBar.Increment(m_assignedBlocks);

			if (IsRelevant(m_navigator.CurrentBlock))
			{
				m_displayBlockIndex = 0;
				LoadBlock();
			}
			else
			{
				m_displayBlockIndex = -1;
				LoadNextRelevantBlock();
			}
		}

		private void PopulateRelevantBlocks()
		{
			m_navigator.NavigateToFirstBlock();
			m_relevantBlocks = new List<Tuple<int, int>>();
			Block block;
			do
			{
				block = m_navigator.CurrentBlock;
				if (IsRelevant(block))
				{
					m_relevantBlocks.Add(m_navigator.GetIndices());
					if (block.UserConfirmed)
						m_assignedBlocks++;
				}
				m_navigator.NextBlock();
			} while (!m_navigator.IsLastBlock(block));

			m_navigator.NavigateToFirstBlock();
		}

		private bool IsRelevant(Block block)
		{
			return block.UserConfirmed || block.CharacterIsUnclear();
		}

		public void LoadBlock()
		{
			m_blocksDisplayBrowser.DisplayHtml(
				BuildHtml(
					BuildHtml(m_navigator.PeekBackwardWithinBook(10)),
					m_navigator.CurrentBlock.GetText(m_showVerseNumbers),
					BuildHtml(m_navigator.PeekForwardWithinBook(10)),
					BuildStyle()));

			UpdateDisplay();
			UpdateNavigationButtonState();
		}

		private void UpdateDisplay()
		{
			String book = m_navigator.CurrentBook.BookId;
			int chapter = m_navigator.CurrentBlock.ChapterNumber;
			int verse = m_navigator.CurrentBlock.InitialStartVerseNumber;
			var currRef = new BCVRef(BCVRef.BookToNumber(book), chapter, verse);
			m_labelReference.Text = BCVRef.MakeReferenceString(currRef, currRef, ":", "-");
			string xOfY = LocalizationManager.GetString("AssignCharacterDialog.XofY", "{0} of {1}", "{0} is the current clip number; {1} is the total number of clips.");
			m_labelXofY.Text = string.Format(xOfY, m_displayBlockIndex + 1, m_relevantBlocks.Count);

			SendScrReference(currRef);

			m_btnAssign.Enabled = false;

			LoadCharacterListBox(CharacterVerseData.Singleton.GetCharacters(book, chapter, verse));
		}

		private string BuildHtml(string previousText, string mainText, string followingText, string style)
		{
			var bldr = new StringBuilder();
			bldr.Append(previousText);
			bldr.Append("<div id=\"main-quote-text\" class=\"highlight\">");
			bldr.Append(mainText);
			bldr.Append("</div>");
			if (!string.IsNullOrEmpty(followingText))
				bldr.Append(kHtmlLineBreak).Append(followingText);
			return string.Format(kHtmlFrame, style, bldr);
		}

		private string BuildHtml(IEnumerable<Block> blocks)
		{
			var bldr = new StringBuilder();
			foreach (Block block in blocks)
				bldr.Append(BuildHtml(block));
			return bldr.ToString();
		}

		private string BuildHtml(Block block)
		{
			var bldr = new StringBuilder();
			bldr.Append("<div class='").Append(kCssClassContext).Append("' data-character='").Append(block.CharacterId).Append("'>")
				.Append(block.GetText(m_showVerseNumbers)).Append("</div>").Append(kHtmlLineBreak);
			return bldr.ToString();
		}

		private string BuildStyle()
		{
			return string.Format(kCssFrame, m_fontFamily, m_fontSizeInPoints);
		}

		private void UpdateNavigationButtonState()
		{
			m_btnNext.Enabled = !IsLastRelevantBlock();
			m_btnPrevious.Enabled = !IsFirstRelevantBlock();
		}

		private void UpdateAssignButtonState()
		{
			bool characterAndDeliverySelected = m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;
			m_btnAssign.Enabled = characterAndDeliverySelected && IsDirty();
		}

		private bool IsDirty()
		{
			BookScript currentBook = m_navigator.CurrentBook;
			Block currentBlock = m_navigator.CurrentBlock;
			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			if (selectedCharacter == m_narrator)
			{
				if (!currentBlock.CharacterIs(currentBook.BookId, CharacterVerseData.StandardCharacter.Narrator))
					return true;
			}
			else if (selectedCharacter != currentBlock.CharacterId)
				return true;
			var selectedDelivery = (string)m_listBoxDeliveries.SelectedItem;
			if (selectedDelivery == m_normalDelivery)
			{
				if (currentBlock.Delivery != null)
					return true;
			}
			else if (selectedDelivery != currentBlock.Delivery)
				return true;
			return false;
		}

		private void LoadNextBlock()
		{
			m_navigator.NextBlock();
			LoadBlock();
		}

		private void LoadNextRelevantBlock()
		{
			m_navigator.SetIndices(m_relevantBlocks[++m_displayBlockIndex]);
			LoadBlock();
		}

		private void LoadPreviousBlock()
		{
			m_navigator.PreviousBlock();
			LoadBlock();
		}

		private void LoadPreviousRelevantBlock()
		{
			m_navigator.SetIndices(m_relevantBlocks[--m_displayBlockIndex]);
			LoadBlock();
		}

		private void LoadCharacterListBox(IEnumerable<CharacterVerse> characters)
		{
			m_characters = characters;

			m_listBoxCharacters.BeginUpdate();

			m_listBoxCharacters.Items.Clear();
			m_listBoxDeliveries.Items.Clear();

			m_listBoxCharacters.Items.Add(m_narrator);
			foreach (CharacterVerse cv in new SortedSet<CharacterVerse>(m_characters, new CharacterComparer())
				.Where(c => !CharacterVerseData.IsCharacterOfType(c.Character, CharacterVerseData.StandardCharacter.Narrator)))
				m_listBoxCharacters.Items.Add(cv.Character);

			SelectCharacter();

			m_listBoxCharacters.EndUpdate();
		}

		private void SelectCharacter()
		{
			Block currentBlock = m_navigator.CurrentBlock;
			if (currentBlock.CharacterIs(m_navigator.CurrentBook.BookId, CharacterVerseData.StandardCharacter.Narrator))
				m_listBoxCharacters.SelectedItem = m_narrator;
			else if (!currentBlock.CharacterIsUnclear())
			{
				if (!m_listBoxCharacters.Items.Contains(currentBlock.CharacterId))
					m_listBoxCharacters.Items.Add(currentBlock.CharacterId);
				m_listBoxCharacters.SelectedItem = currentBlock.CharacterId;
			}
		}

		private void LoadDeliveryListBox()
		{
			m_listBoxDeliveries.BeginUpdate();
			m_listBoxDeliveries.Items.Clear();
			m_deliveries = new List<string>();
			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			m_listBoxDeliveries.Items.Add(m_normalDelivery);
			if (selectedCharacter != m_narrator)
			{
				m_deliveries = m_characters.Where(c => c.Character == selectedCharacter).Select(c => c.Delivery).Where(d => !string.IsNullOrEmpty(d));
				foreach (string delivery in m_deliveries)
					m_listBoxDeliveries.Items.Add(delivery);
			}
			SelectDelivery();
			m_listBoxDeliveries.EndUpdate();
		}

		private void SelectDelivery()
		{
			Block currentBlock = m_navigator.CurrentBlock;
			string delivery = string.IsNullOrEmpty(currentBlock.Delivery) ? m_normalDelivery : currentBlock.Delivery;
			if (!m_listBoxDeliveries.Items.Contains(delivery))
				m_listBoxDeliveries.Items.Add(delivery);

			if (m_listBoxDeliveries.Items.Count == 1)
				m_listBoxDeliveries.SelectedIndex = 0;
			else
			{
				if (currentBlock.CharacterId == (string)m_listBoxCharacters.SelectedItem)
				{
					m_listBoxDeliveries.SelectedItem = delivery;
				}
				else if (m_deliveries.Count() == 1)
				{
					m_listBoxDeliveries.SelectedIndex = 1;
				}
			}
		}

		private bool IsFirstRelevantBlock()
		{
			return m_displayBlockIndex == 0;
		}

		private bool IsLastRelevantBlock()
		{
			return m_displayBlockIndex == m_relevantBlocks.Count - 1;
		}

		private void SaveSelections()
		{
			Block currentBlock = m_navigator.CurrentBlock;

			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			if (selectedCharacter == m_narrator)
				currentBlock.SetStandardCharacter(m_navigator.CurrentBook.BookId, CharacterVerseData.StandardCharacter.Narrator);
			else
				currentBlock.CharacterId = selectedCharacter;

			var selectedDelivery = (string)m_listBoxDeliveries.SelectedItem;
			currentBlock.Delivery = selectedDelivery == m_normalDelivery ? null : selectedDelivery;

			if (!currentBlock.UserConfirmed)
			{
				m_progressBar.Increment(1);
				m_assignedBlocks++;
			}

			currentBlock.UserConfirmed = true;
		}

		private bool UserConfirmSaveChangesIfNecessary()
		{
			if (m_listBoxCharacters.SelectedIndex > -1 && IsDirty())
			{
				string title = LocalizationManager.GetString("AssignCharacterDialog.UnsavedChanges", "Unsaved Changes");
				string msg = LocalizationManager.GetString("AssignCharacterDialog.UnsavedChangesMessage", "The Character and Delivery selections for this clip have not been submitted. Do you want to save your changes before navigating?");
				return MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes;
			}
			return false;
		}

		/// <summary>
		/// Sends a "Santa Fe" focus message which can be used by other applications (such as Paratext)
		/// to navigate to the same Scripture reference
		/// </summary>
		/// <param name="currRef"></param>
		private void SendScrReference(BCVRef currRef)
		{
			if (currRef != null && currRef.Valid)
				SantaFeFocusMessageHandler.SendFocusMessage(currRef.ToString());
		}

		#region Form events
		private void m_btnNext_Click(object sender, EventArgs e)
		{
			if (UserConfirmSaveChangesIfNecessary())
				SaveSelections();
			LoadNextRelevantBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
			if (UserConfirmSaveChangesIfNecessary())
				SaveSelections();
			LoadPreviousRelevantBlock();
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			SaveSelections();
			if (m_assignedBlocks == m_relevantBlocks.Count)
			{
				string title = LocalizationManager.GetString("AssignCharacterDialog.AssignmentsComplete", "Assignments Complete");
				string msg = LocalizationManager.GetString("AssignCharacterDialog.CloseDialogMessage", "All assignments have been made.  Would you like to return to the main window?");
				if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					Close();
					return;
				}
			}
			if (!IsLastRelevantBlock())
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox();
			UpdateAssignButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignButtonState();
		}

		private void m_linkLabelChapter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			String book = m_navigator.CurrentBook.BookId;
			int chapter = m_navigator.CurrentBlock.ChapterNumber;
			LoadCharacterListBox(CharacterVerseData.Singleton.GetUniqueCharacters(book, chapter));
		}

		private void m_linkLabelBook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			String book = m_navigator.CurrentBook.BookId;
			LoadCharacterListBox(CharacterVerseData.Singleton.GetUniqueCharacters(book));
		}

		private void m_linkLabelAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LoadCharacterListBox(CharacterVerseData.Singleton.GetUniqueCharacters());
		}

		private void AssignCharacterDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (UserConfirmSaveChangesIfNecessary())
				SaveSelections();
		}
		#endregion

		#region Browser events
		private void OnMouseOver(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null)
				return;
			var geckoElement = e.Target.CastToGeckoElement();
			var divElement = geckoElement as GeckoDivElement;
			if (divElement == null)
				return;

			if (divElement.ClassName == kCssClassContext)
			{
				m_toolTip = new ToolTip { IsBalloon = true };
				int x = m_blocksDisplayBrowser.Location.X + m_blocksDisplayBrowser.Size.Width - 33;
				int y = m_blocksDisplayBrowser.Location.Y + e.ClientY - 15;
				m_toolTip.Show(divElement.GetAttribute("data-character"), this, x, y);
			}
		}

		private void OnMouseOut(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null)
				return;
			var geckoElement = e.Target.CastToGeckoElement();
			var divElement = geckoElement as GeckoDivElement;
			if (divElement == null)
				return;

			if (divElement.ClassName == kCssClassContext)
			{
				m_toolTip.Hide(this);
			}
		}

		private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			m_blocksDisplayBrowser.ScrollElementIntoView(kMainQuoteElementId, -225);
		}
		#endregion
	}
}
