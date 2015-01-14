using System;
using System.Collections.Generic;
using System.Drawing;
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
		private readonly AssignCharacterViewModel m_viewModel;
		private string m_narrator;
		private string m_normalDelivery;
		private const string kMainQuoteElementId = "main-quote-text";
		private const string kCssClassContext = "context";
		private const int kContextBlocksBackward = 10;
		private const int kContextBlocksForward = 10;

		private IEnumerable<CharacterVerse> m_characters;
		private IEnumerable<string> m_deliveries;
		private ToolTip m_toolTip;
		private IEnumerable<CharacterVerse> m_charactersInBook;
		private readonly Size m_listBoxCharactersOriginalSize;
		private readonly Point m_listBoxCharactersOriginalLocation;
		private readonly Size m_listBoxDeliveriesOriginalSize;
		private readonly Point m_listBoxDeliveriesOriginalLocation;

		private void HandleStringsLocalized()
		{
			m_narrator = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.Narrator", "Narrator");
			m_normalDelivery = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.NormalDelivery", "normal");
			m_viewModel.Narrator = m_narrator;
			m_viewModel.NormalDelivery = m_normalDelivery;
		}

		public AssignCharacterDialog(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();
			m_listBoxCharactersOriginalSize = m_listBoxCharacters.Size;
			m_listBoxCharactersOriginalLocation = m_listBoxCharacters.Location;
			m_listBoxDeliveriesOriginalSize = m_listBoxDeliveries.Size;
			m_listBoxDeliveriesOriginalLocation = m_listBoxDeliveries.Location;

			m_viewModel = viewModel;
			m_viewModel.BackwardContextBlockCount = kContextBlocksBackward;
			m_viewModel.ForwardContextBlockCount = kContextBlocksForward;
			m_blocksDisplayBrowser.OnMouseOver += OnMouseOver;
			m_blocksDisplayBrowser.OnMouseOut += OnMouseOut;
			m_blocksDisplayBrowser.OnDocumentCompleted += OnDocumentCompleted;

			m_progressBar.Maximum = viewModel.RelevantBlockCount;
			m_progressBar.Increment(viewModel.AssignedBlockCount);

			HandleStringsLocalized();

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
			m_viewModel.AssignedBlocksIncremented += (sender, args) => m_progressBar.Increment(1);

			LoadBlock();
		}

		public void LoadBlock()
		{
			m_blocksDisplayBrowser.DisplayHtml(m_viewModel.Html);

			UpdateDisplay();
			UpdateNavigationButtonState();
		}

		private void UpdateDisplay()
		{
			String book = m_viewModel.CurrentBookId;
			int chapter = m_viewModel.CurrentBlock.ChapterNumber;
			int verse = m_viewModel.CurrentBlock.InitialStartVerseNumber;
			var currRef = new BCVRef(BCVRef.BookToNumber(book), chapter, verse);
			m_labelReference.Text = BCVRef.MakeReferenceString(currRef, currRef, ":", "-");
			string xOfY = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.XofY", "{0} of {1}", "{0} is the current clip number; {1} is the total number of clips.");
			m_labelXofY.Text = string.Format(xOfY, m_viewModel.CurrentBlockDisplayIndex, m_viewModel.RelevantBlockCount);

			SendScrReference(currRef);

			HideCharacterFilter();
			m_btnAssign.Enabled = false;

			LoadCharacterListBox(CharacterVerseData.Singleton.GetCharacters(book, chapter, verse));
		}


		private void UpdateNavigationButtonState()
		{
			m_btnNext.Enabled = !m_viewModel.IsLastRelevantBlock;
			m_btnPrevious.Enabled = !m_viewModel.IsFirstRelevantBlock;
		}

		private void UpdateAssignButtonState()
		{
			bool characterAndDeliverySelected = m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;
			m_btnAssign.Enabled = characterAndDeliverySelected && IsDirty();
		}

		private void ShowCharacterFilter()
		{
			m_pnlCharacterFilter.Show();
			var verticalAdjustment = m_pnlCharacterFilter.Size.Height + 5;
			m_listBoxCharacters.Location = new Point(m_listBoxCharacters.Location.X, m_listBoxCharacters.Location.Y + verticalAdjustment);
			m_listBoxCharacters.Size = new Size(m_listBoxCharacters.Size.Width, m_listBoxCharacters.Size.Height - verticalAdjustment);
			m_llMoreChar.Enabled = false;
		}

		private void HideCharacterFilter()
		{
			m_txtCharacterFilter.Clear();
			m_pnlCharacterFilter.Hide();
			m_listBoxCharacters.Location = m_listBoxCharactersOriginalLocation;
			m_listBoxCharacters.Size = m_listBoxCharactersOriginalSize;
			m_llMoreChar.Enabled = true;
		}

		private void ShowDeliveryFilter()
		{
			m_pnlDeliveryFilter.Show();
			var verticalAdjustment = m_pnlDeliveryFilter.Size.Height + 5;
			m_listBoxDeliveries.Location = new Point(m_listBoxDeliveries.Location.X, m_listBoxDeliveries.Location.Y + verticalAdjustment);
			m_listBoxDeliveries.Size = new Size(m_listBoxDeliveries.Size.Width, m_listBoxDeliveries.Size.Height - verticalAdjustment);
			m_llMoreDel.Enabled = false;
		}

		private void HideDeliveryFilter()
		{
			m_txtDeliveryFilter.Clear();
			m_pnlDeliveryFilter.Hide();
			m_listBoxDeliveries.Location = m_listBoxDeliveriesOriginalLocation;
			m_listBoxDeliveries.Size = m_listBoxDeliveriesOriginalSize;
			m_llMoreDel.Enabled = true;
		}

		private bool IsDirty()
		{
			Block currentBlock = m_viewModel.CurrentBlock;
			var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
			if (selectedCharacter == m_narrator)
			{
				if (!currentBlock.CharacterIs(m_viewModel.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
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

		private void LoadNextRelevantBlock()
		{
			m_viewModel.LoadNextRelevantBlock();
			LoadBlock();
		}

		private void LoadPreviousRelevantBlock()
		{
			m_viewModel.LoadPreviousRelevantBlock();
			LoadBlock();
		}

		private void LoadCharacterListBox(IEnumerable<CharacterVerse> characters, bool expandListIfNone = true)
		{
			m_characters = characters;

			m_listBoxCharacters.BeginUpdate();

			m_listBoxCharacters.Items.Clear();
			m_listBoxDeliveries.Items.Clear();
			HideDeliveryFilter();

			m_listBoxCharacters.Items.Add(m_narrator);
			AddItemsToCharacterListBox(m_characters);

			if (expandListIfNone && m_listBoxCharacters.Items.Count < 2)
				ExpandCharactersInList();

			SelectCharacter();

			m_listBoxCharacters.EndUpdate();
		}

		private void ExpandCharactersInList()
		{
			m_characters = m_viewModel.ContextBlocks.Where(b => !b.CharacterIsStandard && !b.CharacterIsUnclear())
				.Select(b => new CharacterVerse { Character = b.CharacterId, Delivery = b.Delivery });
			AddItemsToCharacterListBox(m_characters);
		}

		private void AddItemsToCharacterListBox(IEnumerable<CharacterVerse> characters)
		{
			foreach (CharacterVerse cv in new SortedSet<CharacterVerse>(characters, new CharacterComparer())
				.Where(c => !CharacterVerseData.IsCharacterOfType(c.Character, CharacterVerseData.StandardCharacter.Narrator)))
				m_listBoxCharacters.Items.Add(cv.Character);
		}

		private void SelectCharacter()
		{
			Block currentBlock = m_viewModel.CurrentBlock;
			if (currentBlock.CharacterIs(m_viewModel.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
				m_listBoxCharacters.SelectedItem = m_narrator;
			else if (!currentBlock.CharacterIsUnclear())
			{
				if (!m_listBoxCharacters.Items.Contains(currentBlock.CharacterId))
					m_listBoxCharacters.Items.Add(currentBlock.CharacterId);
				m_listBoxCharacters.SelectedItem = currentBlock.CharacterId;
			}
		}

		private void LoadDeliveryListBox(IEnumerable<string> deliveries = null)
		{
			m_listBoxDeliveries.BeginUpdate();
			m_listBoxDeliveries.Items.Clear();

			if (deliveries != null)
			{
				m_deliveries = deliveries;
				m_listBoxDeliveries.Items.Add(m_normalDelivery);
				foreach (string delivery in m_deliveries)
					m_listBoxDeliveries.Items.Add(delivery);
			}
			else
			{
				m_deliveries = new List<string>();
				var selectedCharacter = (string)m_listBoxCharacters.SelectedItem;
				m_listBoxDeliveries.Items.Add(m_normalDelivery);
				if (selectedCharacter != m_narrator)
				{
					m_deliveries = m_characters.Where(c => c.Character == selectedCharacter).Select(c => c.Delivery).Where(d => !string.IsNullOrEmpty(d));
					foreach (string delivery in m_deliveries)
						m_listBoxDeliveries.Items.Add(delivery);
				}
			}
			SelectDelivery();
			m_listBoxDeliveries.EndUpdate();
		}

		private void SelectDelivery()
		{
			Block currentBlock = m_viewModel.CurrentBlock;
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

		private void SaveSelections()
		{
			m_viewModel.SetCharacterAndDelivery((string) m_listBoxCharacters.SelectedItem,
				(string) m_listBoxDeliveries.SelectedItem);
		}

		private bool UserConfirmSaveChangesIfNecessary()
		{
			if (m_listBoxCharacters.SelectedIndex > -1 && IsDirty())
			{
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChanges", "Unsaved Changes");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChangesMessage", "The Character and Delivery selections for this clip have not been submitted. Do you want to save your changes before navigating?");
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

		private void ShowCharactersInBook()
		{
			String book = m_viewModel.CurrentBookId;
			m_charactersInBook = CharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries(book);
			LoadCharacterListBox(m_charactersInBook);
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
			if (m_viewModel.AreAllAssignmentsComplete)
			{
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.AssignmentsComplete", "Assignments Complete");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.CloseDialogMessage", "All assignments have been made. Would you like to return to the main window?");
				if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					Close();
					return;
				}
			}
			if (!m_viewModel.IsLastRelevantBlock)
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox();
			HideDeliveryFilter();
			if ((string)m_listBoxCharacters.SelectedItem == m_narrator)
				m_llMoreDel.Enabled = false;
			UpdateAssignButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignButtonState();
		}

		private void m_linkLabelMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowCharacterFilter();
			ShowCharactersInBook();
		}

		private void m_llMoreDel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowDeliveryFilter();
			LoadDeliveryListBox(CharacterVerseData.Singleton.GetUniqueDeliveries());
		}

		private void AssignCharacterDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (UserConfirmSaveChangesIfNecessary())
				SaveSelections();
		}

		private void m_txtBoxCharacterFilter_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(m_txtCharacterFilter.Text))
				ShowCharactersInBook();
			else
				LoadCharacterListBox(CharacterVerseData.Singleton.GetUniqueCharacterAndDeliveries().Where(cv => cv.Character.Contains(m_txtCharacterFilter.Text.Trim(), StringComparison.OrdinalIgnoreCase)), false);
		}

		private void m_txtBoxDeliveryFilter_TextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(m_txtDeliveryFilter.Text))
				LoadDeliveryListBox(CharacterVerseData.Singleton.GetUniqueDeliveries());
			else
				LoadDeliveryListBox(CharacterVerseData.Singleton.GetUniqueDeliveries().Where(d => d.Contains(m_txtDeliveryFilter.Text.Trim(), StringComparison.OrdinalIgnoreCase)));
		}

		private void m_icnCharacterFilter_Click(object sender, EventArgs e)
		{
			m_txtCharacterFilter.Focus();
		}

		private void m_icnDeliveryFilter_Click(object sender, EventArgs e)
		{
			m_txtDeliveryFilter.Focus();
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
