// Attributions for icons used in this dialog box:
//
// Exclude User Confirmed Icon:
// Artist: Double-J Design (Available for custom work)
// Iconset: Origami Colored Pencil Icons (160 icons)
// License: CC Attribution 4.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using ProtoScript.Character;
using ProtoScript.Controls;
using SIL.ScriptureControls;
using SIL.ScriptureUtils;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace ProtoScript.Dialogs
{
	public partial class AssignCharacterDialog : Form
	{
		private readonly AssignCharacterViewModel m_viewModel;
		private string m_xOfYFmt;
		private bool m_promptToCloseWhenAssignmentsAreComplete = true;

		private void HandleStringsLocalized()
		{
			m_viewModel.SetUiStrings(
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.Narrator", "narrator ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.BookChapterCharacter", "book title or chapter ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.IntroCharacter", "introduction ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.ExtraCharacter", "section head ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.NormalDelivery", "normal"));

			m_xOfYFmt = m_labelXofY.Text;
		}

		public AssignCharacterDialog(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();

			m_viewModel = viewModel;

			if (Properties.Settings.Default.AssignCharacterDialogFormSettings == null)
				Properties.Settings.Default.AssignCharacterDialogFormSettings = FormSettings.Create(this);

			m_txtCharacterFilter.CorrectHeight();
			m_txtDeliveryFilter.CorrectHeight();
			if (Properties.Settings.Default.AssignCharactersShowGridView)
				m_toolStripButtonGridView.Checked = true;

			var books = new BookSet();
			foreach (var bookId in m_viewModel.IncludedBooks)
				books.Add(bookId);
			m_scriptureReference.VerseControl.BooksPresentSet = books;
			m_scriptureReference.VerseControl.ShowEmptyBooks = false;

			Disposed += AssignCharacterDialog_Disposed;

			m_scriptureReference.VerseControl.AllowVerseSegments = false;
			m_scriptureReference.VerseControl.Versification = m_viewModel.Versification;
			m_scriptureReference.VerseControl.VerseRefChanged += m_scriptureReference_VerseRefChanged;
			m_scriptureReference.VerseControl.Disposed += (sender, args) =>
				m_scriptureReference.VerseControl.VerseRefChanged -= m_scriptureReference_VerseRefChanged;

			m_blocksViewer.Initialize(m_viewModel,
				block => AssignCharacterViewModel.Character.GetCharacterIdForUi(block.CharacterId, CurrentContextCharacters),
				block => block.Delivery);
			m_viewModel.CurrentBlockChanged += LoadBlock;

			UpdateProgressBarForMode();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_viewModel.AssignedBlocksIncremented += m_viewModel_AssignedBlocksIncremented;

			m_blocksViewer.VisibleChanged += LoadBlock;
			m_blocksViewer.Disposed += (sender, args) => m_blocksViewer.VisibleChanged -= LoadBlock;

			SetFilterControlsFromMode();
		}

		void m_viewModel_AssignedBlocksIncremented(object sender, EventArgs e)
		{
			this.SafeInvoke(() =>
			{
				if (m_progressBar.Visible)
					m_progressBar.Increment(1);
			});
		}

		private void UpdateProgressBarForMode()
		{
			if ((m_viewModel.Mode & BlocksToDisplay.NeedAssignments) == BlocksToDisplay.NeedAssignments)
			{
				m_progressBar.Visible = true;
				m_progressBar.Maximum = m_viewModel.RelevantBlockCount;
				m_progressBar.Value = m_viewModel.AssignedBlockCount;
			}
			else
			{
				m_progressBar.Visible = false;				
			}
		}

		private void SetFilterControlsFromMode()
		{
			var mode = m_viewModel.Mode;
			if ((mode & BlocksToDisplay.NeedAssignments) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 0;
			else if ((mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 1;
			else if ((mode & BlocksToDisplay.AllExpectedQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 2;
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 3;
			else
				throw new InvalidEnumArgumentException("mode", (int)mode, typeof(BlocksToDisplay));

			if ((mode & BlocksToDisplay.ExcludeUserConfirmed) != 0)
				m_toolStripButtonExcludeUserConfirmed.Checked = true;
		}

		public void LoadBlock(object sender, EventArgs args)
		{
			if (m_blocksViewer.Visible)
			{
				this.SafeInvoke(() =>
				{
					UpdateDisplay();
					UpdateNavigationButtonState();
				});
			}
		}

		private void UpdateDisplay()
		{
			var blockRef = m_viewModel.GetBlockReference(m_viewModel.CurrentBlock);
			var versesInBlock = m_viewModel.CurrentBlock.LastVerse - blockRef.Verse;
			var displayedRefMinusBlockStartRef = m_scriptureReference.VerseControl.VerseRef.BBBCCCVVV - blockRef.BBCCCVVV;
			if (displayedRefMinusBlockStartRef < 0 || displayedRefMinusBlockStartRef > versesInBlock)
				m_scriptureReference.VerseControl.VerseRef = new VerseRef(m_viewModel.GetBlockReference(m_viewModel.CurrentBlock), Paratext.ScrVers.English);
			m_labelXofY.Visible = m_viewModel.IsCurrentBlockRelevant;
			Debug.Assert(m_viewModel.RelevantBlockCount >= m_viewModel.CurrentBlockDisplayIndex);
			m_labelXofY.Text = string.Format(m_xOfYFmt, m_viewModel.CurrentBlockDisplayIndex, m_viewModel.RelevantBlockCount);

			SendScrReference(m_viewModel.GetBlockReference(m_viewModel.CurrentBlock));

			HideCharacterFilter();
			m_btnAssign.Enabled = false;

			LoadCharacterListBox(m_viewModel.GetCharactersForCurrentReference());
			UpdateShortcutDisplay();
		}

		private void UpdateShortcutDisplay()
		{
			m_pnlShortcuts.Visible = m_listBoxCharacters.Items.Count <= 5;
			m_lblShortcut1.Visible = m_listBoxCharacters.Items.Count > 0;
			m_lblShortcut2.Visible = m_listBoxCharacters.Items.Count > 1;
			m_lblShortcut3.Visible = m_listBoxCharacters.Items.Count > 2;
			m_lblShortcut4.Visible = m_listBoxCharacters.Items.Count > 3;
			m_lblShortcut5.Visible = m_listBoxCharacters.Items.Count > 4;
		}

		private void UpdateNavigationButtonState()
		{
			m_btnNext.Enabled = m_viewModel.CanNavigateToNextRelevantBlock;
			m_btnPrevious.Enabled = m_viewModel.CanNavigateToPreviousRelevantBlock;
		}

		private void UpdateAssignButtonState()
		{
			bool characterAndDeliverySelected = m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;
			m_btnAssign.Enabled = characterAndDeliverySelected && IsDirty();
			if (m_btnAssign.Enabled)
			{
				if (m_btnNext.Focused)
					m_btnAssign.Focus();
				if (m_btnPrevious.Focused)
					m_btnAssign.Focus();
			}
		}

		private void ShowCharacterFilter()
		{
			m_pnlCharacterFilter.Show();
			m_btnAddCharacter.Show();
			m_pnlShortcuts.Hide();
			m_llMoreChar.Enabled = false;
		}

		private void HideCharacterFilter()
		{
			m_txtCharacterFilter.Clear();
			m_pnlCharacterFilter.Hide();
			m_btnAddCharacter.Hide();
			m_llMoreChar.Enabled = true;
		}

		private void ShowDeliveryFilter()
		{
			m_pnlDeliveryFilter.Show();
			m_btnAddDelivery.Show();
			m_llMoreDel.Enabled = false;
		}

		private void HideDeliveryFilter()
		{
			m_txtDeliveryFilter.Clear();
			m_pnlDeliveryFilter.Hide();
			m_btnAddDelivery.Hide();
			m_llMoreDel.Enabled = true;
		}

		private bool IsDirty()
		{
			return m_viewModel.IsModified((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem,
				(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
		}

		private void LoadNextRelevantBlock()
		{
			m_viewModel.LoadNextRelevantBlock();
		}

		private void LoadCharacterListBox(IEnumerable<AssignCharacterViewModel.Character> characters)
		{
			m_listBoxCharacters.BeginUpdate();

			m_listBoxCharacters.Items.Clear();
			m_listBoxDeliveries.Items.Clear();
			HideDeliveryFilter();

			foreach (var character in characters)
				m_listBoxCharacters.Items.Add(character);

			SelectCharacter();

			m_listBoxCharacters.EndUpdate();
		}

		private void SelectCharacter()
		{
			Block currentBlock = m_viewModel.CurrentBlock;
			if (currentBlock.CharacterIs(m_viewModel.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator))
				m_listBoxCharacters.SelectedItem = AssignCharacterViewModel.Character.Narrator;
			else if (!currentBlock.CharacterIsUnclear())
			{
				foreach (var item in CurrentContextCharacters)
				{
					if (item.CharacterId == currentBlock.CharacterId)
					{
						m_listBoxCharacters.SelectedItem = item;
						return;
					}
				}
			}
		}

		private IEnumerable<AssignCharacterViewModel.Character> CurrentContextCharacters
		{
			get { return m_listBoxCharacters.Items.Cast<AssignCharacterViewModel.Character>(); }
		}

		private void LoadDeliveryListBox(IEnumerable<AssignCharacterViewModel.Delivery> deliveries, AssignCharacterViewModel.Delivery selectedItem = null)
		{
			m_listBoxDeliveries.BeginUpdate();
			m_listBoxDeliveries.Items.Clear();

			foreach (var delivery in deliveries)
				m_listBoxDeliveries.Items.Add(delivery);

			SelectDelivery(selectedItem);
			m_listBoxDeliveries.EndUpdate();
		}

		private void SelectDelivery(AssignCharacterViewModel.Delivery previouslySelectedDelivery)
		{
			if (m_listBoxCharacters.Items.Count == 0 || m_listBoxDeliveries.Items.Count == 0 || m_listBoxCharacters.SelectedItem == null)
				return;
			Block currentBlock = m_viewModel.CurrentBlock;
			string currentDelivery = string.IsNullOrEmpty(currentBlock.Delivery) ? AssignCharacterViewModel.Delivery.Normal.Text : currentBlock.Delivery;

			if (m_listBoxDeliveries.Items.Count == 1)
				m_listBoxDeliveries.SelectedIndex = 0;
			else
			{
				if (currentBlock.CharacterId == ((AssignCharacterViewModel.Character) m_listBoxCharacters.SelectedItem).CharacterId)
				{
					foreach (var delivery in m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>())
					{
						if (delivery.Text == currentDelivery)
						{
							m_listBoxDeliveries.SelectedItem = delivery;
							return;
						}
					}
				}
				else if (m_listBoxDeliveries.Items.Count == 2)
					m_listBoxDeliveries.SelectedIndex = 1; // The first one will always be "Normal", so choose the other one.
			}

			if (m_listBoxDeliveries.SelectedItem == null && previouslySelectedDelivery != null)
			{
				if (m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>().Any(delivery => delivery == previouslySelectedDelivery))
				{
					m_listBoxDeliveries.SelectedItem = previouslySelectedDelivery;
				}
			}
		}

		private void SaveSelections()
		{
			m_viewModel.SetCharacterAndDelivery((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem,
				(AssignCharacterViewModel.Delivery) m_listBoxDeliveries.SelectedItem);
		}

		private bool IsOkayToLeaveBlock()
		{
			bool result = true;

			if (IsDirty())
			{
				if (m_btnAssign.Enabled)
				{
					string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChanges", "Unsaved Changes");
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChangesMessage",
						"The Character and Delivery selections have not been submitted. Do you want to save your changes before navigating?");
					if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
						SaveSelections();
				}
				else
				{
					Debug.Assert(m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex < 0);
					string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChanges", "Unsaved Changes");
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChangesMessage",
						"You have selected a Character but no Delivery. Would you like to discard your selection and leave without changing the assignment?");
					result = MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
						DialogResult.Yes;
				}

				Focus();
			}
			return result;
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
			LoadCharacterListBox(m_viewModel.GetUniqueCharacters());
		}

		private void AddNewCharacter(string character)
		{
			if (string.IsNullOrWhiteSpace(character))
				return;

			var existingItem = CurrentContextCharacters
				.FirstOrDefault(c => c.ToString() == character);
			if (existingItem != null)
			{
				m_listBoxCharacters.SelectedItem = existingItem;
				return;
			}

			var newItem = new AssignCharacterViewModel.Character(character);
			m_listBoxCharacters.Items.Add(newItem);
			m_listBoxCharacters.SelectedItem = newItem; 
		}

		private void AddNewDelivery(string delivery)
		{
			if (string.IsNullOrWhiteSpace(delivery))
				return;
			m_listBoxDeliveries.SelectedItem = m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>()
				.FirstOrDefault(d => d.Text == delivery);
			if (m_listBoxDeliveries.SelectedItem != null)
				return;
			var newItem = new AssignCharacterViewModel.Delivery(delivery);
			m_listBoxDeliveries.Items.Add(newItem);
			m_listBoxDeliveries.SelectedItem = newItem;
		}

		#region Form events
		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			Properties.Settings.Default.AssignCharacterDialogFormSettings.InitializeForm(this);
			base.OnLoad(e);
			m_blocksViewer.BlocksGridSettings = Properties.Settings.Default.AssignCharactersBlockContextGrid;
		}

		void AssignCharacterDialog_Disposed(object sender, EventArgs e)
		{
			m_viewModel.CurrentBlockChanged -= LoadBlock;
			LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
			m_viewModel.AssignedBlocksIncremented -= m_viewModel_AssignedBlocksIncremented;

			Disposed -= AssignCharacterDialog_Disposed;
		}
		
		protected override void OnClosing(CancelEventArgs e)
		{
			Properties.Settings.Default.AssignCharactersBlockContextGrid = m_blocksViewer.BlocksGridSettings;
			base.OnClosing(e);
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			if (IsOkayToLeaveBlock())
				LoadNextRelevantBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
			if (IsOkayToLeaveBlock())
				m_viewModel.LoadPreviousRelevantBlock();
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			SaveSelections();
			if (m_viewModel.AreAllAssignmentsComplete && m_promptToCloseWhenAssignmentsAreComplete)
			{
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.AssignmentsComplete", "Assignments Complete");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.CloseDialogMessage", "All assignments have been made. Would you like to return to the main window?");
				if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					Close();
					return;
				}
				m_promptToCloseWhenAssignmentsAreComplete = false;
			}
			if (m_viewModel.CanNavigateToNextRelevantBlock)
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedCharacter = (AssignCharacterViewModel.Character) m_listBoxCharacters.SelectedItem;

			LoadDeliveryListBox(m_viewModel.GetDeliveriesForCharacter(selectedCharacter));
			HideDeliveryFilter();
			if (selectedCharacter != null && selectedCharacter.IsNarrator)
				m_llMoreDel.Enabled = false;
			UpdateAssignButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignButtonState();
		}

		private void m_llMoreChar_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowCharacterFilter();
			ShowCharactersInBook();
			m_txtCharacterFilter.Focus();
		}

		private void m_llMoreDel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowDeliveryFilter();
			LoadDeliveryListBox(m_viewModel.GetUniqueDeliveries(), (AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
			m_txtDeliveryFilter.Focus();
		}

		private void AssignCharacterDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = !IsOkayToLeaveBlock();
		}

		private void m_txtCharacterFilter_TextChanged(object sender, EventArgs e)
		{
			LoadCharacterListBox(m_viewModel.GetUniqueCharacters(m_txtCharacterFilter.Text));
		}

		private void m_txtDeliveryFilter_TextChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox(m_viewModel.GetUniqueDeliveries(m_txtDeliveryFilter.Text),
				(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
		}

		private void m_icnCharacterFilter_Click(object sender, EventArgs e)
		{
			m_txtCharacterFilter.Focus();
		}

		private void m_icnDeliveryFilter_Click(object sender, EventArgs e)
		{
			m_txtDeliveryFilter.Focus();
		}

		private void m_btnAddCharacter_Click(object sender, EventArgs e)
		{
			AddNewCharacter(m_txtCharacterFilter.Text);
		}

		private void m_btnAddDelivery_Click(object sender, EventArgs e)
		{
			AddNewDelivery(m_txtDeliveryFilter.Text);
		}

		private void AssignCharacterDialog_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!m_pnlShortcuts.Visible || m_txtCharacterFilter.Focused || m_txtDeliveryFilter.Focused || m_scriptureReference.Focused )
				return;

			int selectedIndexOneBased;
			Int32.TryParse(e.KeyChar.ToString(CultureInfo.InvariantCulture), out selectedIndexOneBased);
			if (selectedIndexOneBased < 1 || selectedIndexOneBased > 5)
				return;

			if (m_listBoxCharacters.Items.Count >= selectedIndexOneBased)
				m_listBoxCharacters.SelectedIndex = selectedIndexOneBased - 1; //listBox is zero-based
		}

		private void AssignCharacterDialog_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && m_splitContainer.ContainsFocus)
			{
				if (m_btnAssign.Enabled)
					m_btnAssign.PerformClick();
				else if (m_btnNext.Enabled)
					m_btnNext.PerformClick();
			}
		}

		private void HandleFilterChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				return;

			BlocksToDisplay mode;

			switch (m_toolStripComboBoxFilter.SelectedIndex)
			{
				case 0: mode = BlocksToDisplay.NeedAssignments; break;
				case 1: mode = BlocksToDisplay.MoreQuotesThanExpectedSpeakers; break;
				case 2: mode = BlocksToDisplay.AllExpectedQuotes; break;
				default: mode = BlocksToDisplay.AllScripture; break;
			}

			if (m_toolStripButtonExcludeUserConfirmed.Checked)
				mode |= BlocksToDisplay.ExcludeUserConfirmed;

			m_viewModel.Mode = mode;

			if (m_viewModel.RelevantBlockCount > 0)
			{
				LoadBlock(sender, e);
			}
			else
			{
				m_blocksViewer.Clear();
				m_labelXofY.Visible = false;
				m_listBoxCharacters.Items.Clear();
				m_listBoxDeliveries.Items.Clear();

				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.NoMatches", "Nothing matches your current filter.");
				MessageBox.Show(this, msg, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			UpdateProgressBarForMode();
		}

		private void HandleHtmlViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonGridView.Checked = !m_toolStripButtonHtmlView.Checked;

				Debug.Assert(!m_toolStripButtonGridView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Html;
				Properties.Settings.Default.AssignCharactersShowGridView = false;
			}
		}

		private void HandleDataGridViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonHtmlView.Checked = !m_toolStripButtonGridView.Checked;

				Debug.Assert(!m_toolStripButtonHtmlView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Grid;
				Properties.Settings.Default.AssignCharactersShowGridView = true;
			}
		}

		private void HandleViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
				button.Checked = true;
		}

		private void IncreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.IncreaseFont();
		}

		private void DecreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.DecreaseFont();
		}

		private void m_scriptureReference_VerseRefChanged(object sender, PropertyChangedEventArgs e)
		{
			m_viewModel.TryLoadBlock(m_scriptureReference.VerseControl.VerseRef);
		}
		#endregion
	}
}
