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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext;

namespace Glyssen.Dialogs
{
	public partial class AssignCharacterDlg : FormWithPersistedSettings
	{
		private readonly AssignCharacterViewModel m_viewModel;
		private string m_xOfYFmt;
		private string m_singleVoiceCheckboxFmt;
		private bool m_promptToCloseWhenAssignmentsAreComplete = true;
		int m_characterListHoveredIndex = -1;
		private readonly ToolTip m_characterListToolTip = new ToolTip();
		private bool m_formLoading;
		private readonly Font m_originalDefaultFontForLists;

		private void HandleStringsLocalized()
		{
			m_viewModel.SetUiStrings(
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.Narrator", "narrator ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BookChapterCharacter", "book title or chapter ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IntroCharacter", "introduction ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.ExtraCharacter", "section head ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NormalDelivery", "normal"));

			m_xOfYFmt = m_labelXofY.Text;
			m_singleVoiceCheckboxFmt = m_lblSingleVoice.Text;

			Text = string.Format(Text, m_viewModel.ProjectName);
		}

		public AssignCharacterDlg(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();

			m_viewModel = viewModel;

			m_txtCharacterFilter.CorrectHeight();
			m_txtDeliveryFilter.CorrectHeight();
			if (Settings.Default.AssignCharactersShowGridView)
				m_toolStripButtonGridView.Checked = true;

			var books = new BookSet();
			foreach (var bookId in m_viewModel.IncludedBooks)
				books.Add(bookId);
			m_scriptureReference.VerseControl.BooksPresentSet = books;
			m_scriptureReference.VerseControl.ShowEmptyBooks = false;

			m_scriptureReference.VerseControl.AllowVerseSegments = false;
			m_scriptureReference.VerseControl.Versification = m_viewModel.Versification;
			m_scriptureReference.VerseControl.VerseRefChanged += m_scriptureReference_VerseRefChanged;
			m_scriptureReference.VerseControl.Disposed += (sender, args) =>
				m_scriptureReference.VerseControl.VerseRefChanged -= m_scriptureReference_VerseRefChanged;

			m_blocksViewer.Initialize(m_viewModel,
				AssignCharacterViewModel.Character.GetCharacterIdForUi,
				block => block.Delivery);
			m_viewModel.CurrentBlockChanged += LoadBlock;

			UpdateProgressBarForMode();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_listBoxCharacters.DisplayMember = "LocalizedDisplay";
			m_originalDefaultFontForLists = m_listBoxCharacters.Font;
			SetFontsFromViewModel();

			m_viewModel.AssignedBlocksIncremented += m_viewModel_AssignedBlocksIncremented;
			m_viewModel.UiFontSizeChanged += (sender, args) => SetFontsFromViewModel();

			m_blocksViewer.VisibleChanged += LoadBlock;
			m_blocksViewer.Disposed += (sender, args) => m_blocksViewer.VisibleChanged -= LoadBlock;

			SetFilterControlsFromMode();

			m_viewModel.CurrentBookSaved += UpdateSavedText;
			m_viewModel.FilterReset +=m_viewModel_FilterReset;
		}

		void m_viewModel_FilterReset(object sender, EventArgs e)
		{
			UpdateNavigationButtonState();
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
			else if ((mode & BlocksToDisplay.MissingExpectedQuote) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 1;
			else if ((mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 2;
			else if ((mode & BlocksToDisplay.AllExpectedQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 3;
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 4;
			else
				// ReSharper disable once NotResolvedInText
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
			var blockRef = m_viewModel.GetBlockVerseRef();
			int versesInSelection = m_viewModel.GetLastVerseInCurrentQuote() - blockRef.VerseNum;
			var displayedRefMinusBlockStartRef = m_scriptureReference.VerseControl.VerseRef.BBBCCCVVV - blockRef.BBBCCCVVV;
			if (displayedRefMinusBlockStartRef < 0 || displayedRefMinusBlockStartRef > versesInSelection)
				m_scriptureReference.VerseControl.VerseRef = m_viewModel.GetBlockVerseRef();
			m_labelXofY.Visible = m_viewModel.IsCurrentBlockRelevant;
			Debug.Assert(m_viewModel.RelevantBlockCount >= m_viewModel.CurrentBlockDisplayIndex);
			m_labelXofY.Text = string.Format(m_xOfYFmt, m_viewModel.CurrentBlockDisplayIndex, m_viewModel.RelevantBlockCount);
			m_lblSingleVoice.Text = string.Format(m_singleVoiceCheckboxFmt, m_viewModel.CurrentBookId);

			m_viewModel.GetBlockVerseRef().SendScrReference();

			HideCharacterFilter();
			m_btnAssign.Enabled = false;

			LoadCharacterListBox(m_viewModel.GetCharactersForCurrentReference());
			UpdateShortcutDisplay();

			m_chkSingleVoice.Checked = m_viewModel.IsCurrentBookSingleVoice;

			m_menuBtnSplitBlock.Enabled = !CharacterVerseData.IsCharacterStandard(m_viewModel.CurrentBlock.CharacterId, false);
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
			if (m_btnAssign.Enabled && !m_btnAssign.Focused)
			{
				var focusedControl = this.FindFocusedControl();
				if (focusedControl is Button || focusedControl is LinkLabel)
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
			UpdateAssignButtonState();
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
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedChanges", "Unsaved Changes");
				if (m_btnAssign.Enabled)
				{
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedChangesMessage",
						"The Character and Delivery selections have not been submitted. Do you want to save your changes before navigating?");
					if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
						SaveSelections();
				}
				else if (m_listBoxCharacters.SelectedIndex < 0)
				{
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NoSelectionMessage",
						"You have not selected a Character and Delivery. Would you like to leave without changing the assignment?");
					result = MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
						DialogResult.Yes;
				}
				else
				{
					Debug.Assert(m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex < 0);
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NoDeliveryMessage",
						"You have selected a Character but no Delivery. Would you like to discard your selection and leave without changing the assignment?");
					result = MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
						DialogResult.Yes;
				}

				Focus();
			}
			return result;
		}

		private void ShowCharactersInBook()
		{
			LoadCharacterListBox(m_viewModel.GetUniqueCharacters());
		}

		private void AddNewCharacter(string character)
		{
			if (string.IsNullOrWhiteSpace(character))
				return;

			var existingItem = CurrentContextCharacters.FirstOrDefault(c => c.ToString() == character);
			if (existingItem != null)
			{
				m_listBoxCharacters.SelectedItem = existingItem;
				return;
			}

			using (var dlg = new NewCharacterDlg(character))
			{
				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				m_viewModel.AddCharacterDetailToProject(character, dlg.Gender, dlg.Age);
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

		private void SetFontsFromViewModel()
		{
			float newFontSize = Math.Max(m_originalDefaultFontForLists.SizeInPoints + m_viewModel.FontSizeUiAdjustment, BlockNavigatorViewModel.kMinFontSize);
			Font newFont = new Font(m_originalDefaultFontForLists.FontFamily, newFontSize, m_originalDefaultFontForLists.Style);
			m_listBoxCharacters.Font = newFont;
			m_listBoxDeliveries.Font = newFont;
			m_pnlShortcuts.Height = m_listBoxCharacters.ItemHeight * 5;
		}

		private void UpdateSavedText(object obj, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		#region Form events
		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			m_formLoading = true;

			base.OnLoad(e);
			m_blocksViewer.BlocksGridSettings = Settings.Default.AssignCharactersBlockContextGrid;
			if (Settings.Default.AssignCharactersSliderLocation > 0)
				m_splitContainer.SplitterDistance = Settings.Default.AssignCharactersSliderLocation;

			m_pnlShortcuts.Height = m_listBoxCharacters.ItemHeight * 5;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (m_viewModel.RelevantBlockCount == 0)
				m_blocksViewer.ShowNothingMatchesFilterMessage();
		}

		private void AssignCharacterDlg_Load(object sender, EventArgs e)
		{
			MainForm.SetChildFormLocation(this);
		}

		private void AssignCharacterDialog_Shown(object sender, EventArgs e)
		{
			m_formLoading = false;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Settings.Default.AssignCharactersBlockContextGrid = m_blocksViewer.BlocksGridSettings;
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
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.AssignmentsComplete", "Assignments Complete");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CloseDialogMessage", "All assignments have been made. Would you like to return to the main window?");
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
			if (m_txtCharacterFilter.Focused || m_txtDeliveryFilter.Focused || m_scriptureReference.VerseControl.ContainsFocus)
				return;

			int selectedIndexOneBased;
			Int32.TryParse(e.KeyChar.ToString(CultureInfo.InvariantCulture), out selectedIndexOneBased);
			if (selectedIndexOneBased < 1 || selectedIndexOneBased > 5)
			{
				// Might be trying to select character by the first letter (e.g. s for Saul)
				HandleCharacterSelectionKeyPress(e);
				e.Handled = true;
			}
			else if (m_pnlShortcuts.Visible)
			{
				if (m_listBoxCharacters.Items.Count >= selectedIndexOneBased)
					m_listBoxCharacters.SelectedIndex = selectedIndexOneBased - 1; //listBox is zero-based
			}
		}

		private void HandleCharacterSelectionKeyPress(KeyPressEventArgs e)
		{
			if (Char.IsLetter(e.KeyChar))
			{
				var charactersStartingWithSelectedLetter =
					CurrentContextCharacters.Where(c => c.ToString().StartsWith(e.KeyChar.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture));
				if (charactersStartingWithSelectedLetter.Count() == 1)
					m_listBoxCharacters.SelectedItem = charactersStartingWithSelectedLetter.Single();
				else
					m_listBoxCharacters.SelectedItem = null;
			}
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
				case 1: mode = BlocksToDisplay.MissingExpectedQuote; break;
				case 2: mode = BlocksToDisplay.MoreQuotesThanExpectedSpeakers; break;
				case 3: mode = BlocksToDisplay.AllExpectedQuotes; break;
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
				m_labelXofY.Visible = false;
				UpdateNavigationButtonState();
				m_blocksViewer.ShowNothingMatchesFilterMessage();
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
				Settings.Default.AssignCharactersShowGridView = false;
			}
		}

		private void HandleDataGridViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonHtmlView.Checked = !m_toolStripButtonGridView.Checked;

				Debug.Assert(!m_toolStripButtonHtmlView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Grid;
				Settings.Default.AssignCharactersShowGridView = true;
			}
		}

		private void HandleViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
			{
				button.Checked = true;

				Analytics.Track("SwitchView", new Dictionary<string, string> { { "dialog", Name }, { "view", button.ToString() } });
			}
		}

		private void HandleSplitBlocksClick(object sender, EventArgs e)
		{
			using (var dlg = new SplitBlockDlg(m_viewModel, m_viewModel.GetAllBlocksWhichContinueTheQuoteStartedByBlock(m_viewModel.CurrentBlock)))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_viewModel.SplitBlock(dlg.SplitLocations);
				}
			}
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

		private void m_chkSingleVoice_CheckedChanged(object sender, EventArgs e)
		{
			m_viewModel.SetCurrentBookSingleVoice(m_chkSingleVoice.Checked);
			UpdateProgressBarForMode();
			UpdateNavigationButtonState();

			// Enable or disable some controls
			var notChecked = !m_chkSingleVoice.Checked;
			m_lblCharacter.Enabled = notChecked;
			m_listBoxCharacters.Enabled = notChecked;
			m_llMoreChar.Enabled = notChecked;
			m_txtCharacterFilter.Enabled = notChecked;
			m_btnAddCharacter.Enabled = notChecked;
			m_icnCharacterFilter.Enabled = notChecked;

			m_lblDelivery.Enabled = notChecked;
			m_listBoxDeliveries.Enabled = notChecked;
			m_llMoreDel.Enabled = notChecked;
			m_txtDeliveryFilter.Enabled = notChecked;
			m_btnAddDelivery.Enabled = notChecked;
			m_icnDeliveryFilter.Enabled = notChecked;

			m_btnAssign.Enabled = notChecked;
			m_lblShortcut1.Enabled = notChecked;
			m_lblShortcut2.Enabled = notChecked;
			m_lblShortcut3.Enabled = notChecked;
			m_lblShortcut4.Enabled = notChecked;
			m_lblShortcut5.Enabled = notChecked;
		}

		private void m_listBoxCharacters_MouseMove(object sender, MouseEventArgs e)
		{
			int newHoveredIndex = m_listBoxCharacters.IndexFromPoint(e.Location);

			if (m_characterListHoveredIndex != newHoveredIndex)
			{
				m_characterListHoveredIndex = newHoveredIndex;
				if (m_characterListHoveredIndex > -1)
				{
					m_characterListToolTip.Active = false;
					var hoveredCharacter = ((AssignCharacterViewModel.Character)m_listBoxCharacters.Items[m_characterListHoveredIndex]);
					if (!string.IsNullOrEmpty(hoveredCharacter.LocalizedAlias))
					{
						m_characterListToolTip.SetToolTip(m_listBoxCharacters, hoveredCharacter.LocalizedCharacterId);
						m_characterListToolTip.Active = true;
					}
				}
			}
		}

		private void m_splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!m_formLoading)
				Settings.Default.AssignCharactersSliderLocation = e.SplitX;
		}

		private void m_listBoxCharacters_KeyPress(object sender, KeyPressEventArgs e)
		{
			HandleCharacterSelectionKeyPress(e);
			e.Handled = true;
		}

		private void m_llClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Close();
		}

		private void m_lblSingleVoice_Click(object sender, EventArgs e)
		{
			m_chkSingleVoice.Checked = !m_chkSingleVoice.Checked;
		}

		#endregion
	}
}
