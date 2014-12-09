using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using L10NSharp;

namespace ProtoScript.Dialogs
{
	public partial class AssignCharacterDialog : Form
	{
		private const string kNarrator = "Narrator";
		private const string kNormalDelivery = "{normal}";
		private const string kHtmlFrame = "<html><head><meta charset=\"UTF-8\">" +
		                                  "<style>{0}</style></head><body>{1}</body></html>";
		private const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}.highlight{{background-color:yellow}}";

		private readonly string m_fontFamily;
		private readonly int m_fontSizeInPoints;
		private readonly BlockNavigator m_navigator;
		private List<Tuple<int, int>> m_relevantBlocks;
		private int m_assignedBlocks;
		private int m_displayBlockIndex;
		private IEnumerable<CharacterVerse> m_characters;
		private IEnumerable<string> m_deliveries;
		private bool m_showVerseNumbers = true; // May make this configurable later

		public AssignCharacterDialog()
		{
			InitializeComponent();
		}

		public AssignCharacterDialog(Project project)
		{
			InitializeComponent();

			m_fontFamily = project.FontFamily;
			m_fontSizeInPoints = project.FontSizeInPoints;

			m_navigator = new BlockNavigator(project.Books);

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
			Block previousBlock = m_navigator.PeekPreviousBlock();
			Block nextBlock = m_navigator.PeekNextBlock();
			m_blocksDisplayBrowser.DisplayHtml(
				BuildHtml(
					previousBlock == null ? null : previousBlock.GetText(m_showVerseNumbers),
					m_navigator.CurrentBlock.GetText(m_showVerseNumbers),
					nextBlock == null ? null : nextBlock.GetText(m_showVerseNumbers),
					BuildStyle()));

			UpdateDisplay();
			UpdateNavigationButtons();
		}

		private void UpdateDisplay()
		{
			String book = m_navigator.CurrentBook.BookId;
			int chapter = m_navigator.CurrentBlock.ChapterNumber;
			int verse = m_navigator.CurrentBlock.InitialVerseNumber;
			m_labelReference.Text = string.Format("{0} {1}:{2}", book, chapter, verse);
			string xOfY = LocalizationManager.GetString("AssignCharacterDialog.XofY", "{0} of {1}", "{0} is the current clip number; {1} is the total number of clips.");
			m_labelXofY.Text = string.Format(xOfY, m_displayBlockIndex + 1, m_relevantBlocks.Count);

			m_btnAssign.Enabled = false;

			LoadCharacterListBox(CharacterVerseData.Singleton.GetCharacters(book, chapter, verse));
		}

		private string BuildHtml(string previousText, string mainText, string followingText, string style)
		{
			var bldr = new StringBuilder();
			if (!string.IsNullOrEmpty(previousText))
			{
				bldr.Append(previousText);
				bldr.Append("<br/><br/>");
			}
			bldr.Append("<span class=\"highlight\">");
			bldr.Append(mainText);
			bldr.Append("</span>");
			if (!string.IsNullOrEmpty(followingText))
			{
				bldr.Append("<br/><br/>");
				bldr.Append(followingText);
			}
			return string.Format(kHtmlFrame, style, bldr);
		}

		private string BuildStyle()
		{
			return string.Format(kCssFrame, m_fontFamily, m_fontSizeInPoints);
		}

		private void UpdateNavigationButtons()
		{
			m_btnNext.Enabled = !OnLastRelevantBlock();
			m_btnPrevious.Enabled = !OnFirstRelevantBlock();
		}

		private void UpdateAssignButton()
		{
			m_btnAssign.Enabled = m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;
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

			m_listBoxCharacters.Items.Clear();
			m_listBoxDeliveries.Items.Clear();

			m_listBoxCharacters.Items.Add(kNarrator);
			foreach (CharacterVerse cv in new SortedSet<CharacterVerse>(m_characters, new CharacterComparer()))
				m_listBoxCharacters.Items.Add(cv.Character);

			SetCharacter();
		}

		private void SetCharacter()
		{
			Block currentBlock = m_navigator.CurrentBlock;
			if (currentBlock.CharacterIs(Block.StandardCharacter.Narrator))
				m_listBoxCharacters.SelectedItem = kNarrator;
			if (!currentBlock.CharacterIsUnclear())
			{
				if (!m_listBoxCharacters.Items.Contains(currentBlock.CharacterId))
					m_listBoxCharacters.Items.Add(currentBlock.CharacterId);
				m_listBoxCharacters.SelectedItem = currentBlock.CharacterId;
			}
		}

		private void LoadDeliveryListBox()
		{
			m_listBoxDeliveries.Items.Clear();
			m_deliveries = new List<string>();
			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			m_listBoxDeliveries.Items.Add(kNormalDelivery);
			if (selectedCharacter != kNarrator)
			{
				m_deliveries = m_characters.Where(c => c.Character == selectedCharacter).Select(c => c.Delivery).Where(d => !string.IsNullOrEmpty(d));
				foreach (string delivery in m_deliveries)
					m_listBoxDeliveries.Items.Add(delivery);
			}
			SetDelivery();
		}

		private void SetDelivery()
		{
			Block currentBlock = m_navigator.CurrentBlock;
			string delivery = string.IsNullOrEmpty(currentBlock.Delivery) ? kNormalDelivery : currentBlock.Delivery;
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

		private bool OnFirstRelevantBlock()
		{
			return m_displayBlockIndex == 0;
		}

		private bool OnLastRelevantBlock()
		{
			return m_displayBlockIndex == m_relevantBlocks.Count - 1;
		}

		private void SaveSelections()
		{
			Block currentBlock = m_navigator.CurrentBlock;

			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			if (selectedCharacter == kNarrator)
				currentBlock.SetStandardCharacter(m_navigator.CurrentBook.BookId, Block.StandardCharacter.Narrator);
			else
				currentBlock.CharacterId = selectedCharacter;

			var selectedDelivery = (string)m_listBoxDeliveries.SelectedItem;
			currentBlock.Delivery = selectedDelivery == kNormalDelivery ? null : selectedDelivery;

			if (!currentBlock.UserConfirmed)
			{
				m_progressBar.Increment(1);
				m_assignedBlocks++;
			}

			currentBlock.UserConfirmed = true;
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			LoadNextRelevantBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
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
			if (!OnLastRelevantBlock())
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox();
			UpdateAssignButton();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignButton();
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
	}
}
