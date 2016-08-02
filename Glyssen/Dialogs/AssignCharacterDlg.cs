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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopAnalytics;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Utilities;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	public partial class AssignCharacterDlg : FormWithPersistedSettings, IMessageFilter
	{
		private readonly AssignCharacterViewModel m_viewModel;
		private string m_xOfYFmt;
		private string m_singleVoiceCheckboxFmt;
		private bool m_promptToCloseWhenAssignmentsAreComplete = true;
		int m_characterListHoveredIndex = -1;
		private readonly ToolTip m_characterListToolTip = new ToolTip();
		private bool m_formLoading;
		private readonly FontProxy m_originalDefaultFontForLists;
		private readonly FontProxy m_originalDefaultFontForCharacterAndDeliveryColumns;
		private Font m_primaryReferenceTextFont;
		private Font m_englishReferenceTextFont;
		private bool m_userMadeChangesToReferenceTextMatchup;

		private void HandleStringsLocalized()
		{
			m_viewModel.SetUiStrings(
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.Narrator", "narrator ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BookChapterCharacter", "book title or chapter ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IntroCharacter", "introduction ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.ExtraCharacter", "section head ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NormalDelivery", "normal"));

			L10N.LocalizeComboList(m_toolStripComboBoxFilter, "DialogBoxes.AssignCharacterDlg.FilterOptions");

			m_xOfYFmt = m_labelXofY.Text;
			m_singleVoiceCheckboxFmt = m_chkSingleVoice.Text;

			Text = string.Format(Text, m_viewModel.ProjectName);
		}

		public AssignCharacterDlg(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();

			m_viewModel = viewModel;
			m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;

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
			m_viewModel.CurrentBlockMatchupChanged += LoadBlockMatchup;

			UpdateProgressBarForMode();

			m_dataGridReferenceText.DataError += HandleDataGridViewDataError;
			colPrimary.HeaderText = m_viewModel.PrimaryReferenceTextName;

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			colCharacter.DisplayMember = m_listBoxCharacters.DisplayMember = "LocalizedDisplay";
			colDelivery.DisplayMember = m_listBoxDeliveries.DisplayMember = "LocalizedDisplay";
			m_originalDefaultFontForLists = new FontProxy(m_listBoxCharacters.Font);
			m_originalDefaultFontForCharacterAndDeliveryColumns = new FontProxy(m_dataGridReferenceText.DefaultCellStyle.Font);
			SetFontsFromViewModel();

			m_viewModel.AssignedBlocksIncremented += m_viewModel_AssignedBlocksIncremented;
			m_viewModel.UiFontSizeChanged += (sender, args) => SetFontsFromViewModel();

			m_blocksViewer.VisibleChanged += BlocksViewerVisibleChanged;
			m_blocksViewer.Disposed += (sender, args) => m_blocksViewer.VisibleChanged -= BlocksViewerVisibleChanged;

			SetFilterControlsFromMode();

			m_viewModel.CurrentBookSaved += UpdateSavedText;
			m_viewModel.FilterReset +=m_viewModel_FilterReset;
		}

		void HandleDataGridViewDataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Analytics.ReportException(e.Exception);
			ErrorReport.ReportFatalException(e.Exception);
			throw e.Exception;
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
			else if ((mode & BlocksToDisplay.AllQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 4;
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 5;
			else
				// ReSharper disable once NotResolvedInText
				throw new InvalidEnumArgumentException("mode", (int)mode, typeof(BlocksToDisplay));

			if ((mode & BlocksToDisplay.ExcludeUserConfirmed) != 0)
				m_toolStripButtonExcludeUserConfirmed.Checked = true;
		}

		private void BlocksViewerVisibleChanged(object sender, EventArgs args)
		{
			LoadBlock(sender, args);
			LoadBlockMatchup(sender, args);
		}


		private void LoadBlock(object sender, EventArgs args)
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

		private void LoadBlockMatchup(object sender, EventArgs args)
		{
			if (m_blocksViewer.Visible)
			{
				if (m_viewModel.CurrentReferenceTextMatchup != null)
					this.SafeInvoke(ShowMatchReferenceTextTabPage);
				this.SafeInvoke(UpdateReferenceTextTabPageDisplay);
				m_userMadeChangesToReferenceTextMatchup = false;
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
			if (m_viewModel.IsCurrentBlockRelevant)
				Debug.Assert(m_viewModel.RelevantBlockCount >= m_viewModel.CurrentBlockDisplayIndex);
			m_labelXofY.Text = string.Format(m_xOfYFmt, m_viewModel.CurrentBlockDisplayIndex, m_viewModel.RelevantBlockCount);
			m_chkSingleVoice.Text = string.Format(m_singleVoiceCheckboxFmt, m_viewModel.CurrentBookId);

			m_viewModel.GetBlockVerseRef().SendScrReference();

			HideCharacterFilter();
			m_btnAssign.Enabled = false;

			LoadCharacterListBox(m_viewModel.GetUniqueCharactersForCurrentReference());
			UpdateShortcutDisplay();

			m_chkSingleVoice.Checked = m_viewModel.IsCurrentBookSingleVoice;

			m_menuBtnSplitBlock.Enabled = !CharacterVerseData.IsCharacterStandard(m_viewModel.CurrentBlock.CharacterId, false);
		}

		private void ShowMatchReferenceTextTabPage()
		{
			if (!m_tabControlCharacterSelection.TabPages.Contains(tabPageMatchReferenceText))
				m_tabControlCharacterSelection.TabPages.Add(tabPageMatchReferenceText);
		}

		private void UpdateReferenceTextTabPageDisplay()
		{
			m_dataGridReferenceText.CellValueChanged -= m_dataGridReferenceText_CellValueChanged;

			m_tabControlCharacterSelection.SelectedTab = m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation ?
				tabPageMatchReferenceText : tabPageSelectCharacter;

			m_dataGridReferenceText.RowCount = 0;
			colCharacter.Items.Clear();
			colDelivery.Items.Clear();

			if (m_viewModel.CurrentReferenceTextMatchup == null)
				m_tabControlCharacterSelection.TabPages.Remove(tabPageMatchReferenceText);
			else
			{
				foreach (AssignCharacterViewModel.Character character in m_viewModel.GetCharactersForCurrentReferenceTextMatchup())
					colCharacter.Items.Add(character);

				foreach (AssignCharacterViewModel.Delivery delivery in m_viewModel.GetDeliveriesForCurrentReferenceTextMatchup())
					colDelivery.Items.Add(delivery);

				ShowMatchReferenceTextTabPage();

				m_dataGridReferenceText.RowCount = m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks.Count;
				colPrimary.Visible = m_viewModel.HasSecondaryReferenceText;
				colCharacter.Visible = colCharacter.Items.Count > 1;
				colDelivery.Visible = colDelivery.Items.Count > 1;
				var primaryColumnIndex = colPrimary.Visible ? colPrimary.Index : colEnglish.Index;
				int i = 0;
				foreach (var correlatedBlock in m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks)
				{
					Debug.Assert(correlatedBlock.MatchesReferenceText);

					var row = m_dataGridReferenceText.Rows[i];
					row.DefaultCellStyle.BackColor = GlyssenColorPalette.ColorScheme.GetMatchColor(i++);
					if (colPrimary.Visible)
						row.Cells[colEnglish.Index].Value = correlatedBlock.ReferenceBlocks.Single().PrimaryReferenceText;
					row.Cells[primaryColumnIndex].Value = correlatedBlock.PrimaryReferenceText;
					if (colCharacter.Visible)
					{
						string characterId = correlatedBlock.CharacterIsUnclear() ? correlatedBlock.ReferenceBlocks.Single().CharacterId :
							correlatedBlock.CharacterId;

						if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
							characterId = ((AssignCharacterViewModel.Character)colCharacter.Items[0]).LocalizedDisplay;
						foreach (var character in colCharacter.Items)
						{
							if (character.ToString() == characterId)
							{
								row.Cells[colCharacter.Index].Value = character;
								break;
							}
						}
					}
					if (colDelivery.Visible)
					{
						var delivery = correlatedBlock.Delivery;
						if (string.IsNullOrEmpty(delivery))
							delivery = correlatedBlock.ReferenceBlocks.Single().Delivery;
						if (string.IsNullOrEmpty(delivery))
							delivery = ((AssignCharacterViewModel.Delivery)colDelivery.Items[0]).LocalizedDisplay;
						row.Cells[colDelivery.Index].Value = delivery;
					}
				}
			}

			UpdateAssignOrApplyButtonState();

			m_dataGridReferenceText.CellValueChanged += m_dataGridReferenceText_CellValueChanged;
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

		private void UpdateAssignOrApplyButtonState()
		{
			Button btn = m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter
				? m_btnAssign
				: m_btnApplyReferenceTextMatches;

			btn.Enabled = IsCharacterAndDeliverySelectionComplete && IsDirty();
			if (btn.Enabled && !btn.Focused)
			{
				var focusedControl = this.FindFocusedControl();
				if (focusedControl is Button || focusedControl is LinkLabel)
					btn.Focus();
			}
		}

		private bool IsCharacterAndDeliverySelectionComplete
		{
			get
			{
				if (m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter)
					return m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex > -1;

				return AreSelectionsCompleteForColumn(colCharacter) && AreSelectionsCompleteForColumn(colDelivery);
			}
		}

		private bool AreSelectionsCompleteForColumn(DataGridViewComboBoxColumn col)
		{
			return !col.Visible || m_dataGridReferenceText.Rows.Cast<DataGridViewRow>().All(row => row.Cells[col.Index].Value != null);
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
			if (m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter)
			{
				return m_viewModel.IsModified((AssignCharacterViewModel.Character) m_listBoxCharacters.SelectedItem,
					(AssignCharacterViewModel.Delivery) m_listBoxDeliveries.SelectedItem);
			}
			return m_viewModel.CurrentReferenceTextMatchup.HasOutstandingChangesToApply;
		}

		private void LoadNextRelevantBlock()
		{
			m_viewModel.LoadNextRelevantBlock();
			m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
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
			UpdateAssignOrApplyButtonState();
		}

		private void SelectCharacter()
		{
			var character = m_viewModel.GetCharacterToSelectForCurrentBlock(CurrentContextCharacters);
			if (character != null)
				m_listBoxCharacters.SelectedItem = character;
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

				if (m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter)
				{
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
				}
				else
				{
					if (m_btnApplyReferenceTextMatches.Enabled && m_userMadeChangesToReferenceTextMatchup)
					{
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedReferenceTextChangesMessage",
							"The alignment of the vernacular script to the reference text has not been applied. Do you want to save the alignment before navigating?");
						if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
							m_viewModel.ApplyCurrentReferenceTextMatchup();
					}
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
			m_listBoxCharacters.Font = m_listBoxDeliveries.Font = m_originalDefaultFontForLists.AdjustFontSize(m_viewModel.FontSizeUiAdjustment);
			m_pnlShortcuts.Height = m_listBoxCharacters.ItemHeight * 5;

			if (m_primaryReferenceTextFont != null)
				m_primaryReferenceTextFont.Dispose();

			colPrimary.DefaultCellStyle.Font = m_viewModel.PrimaryReferenceTextFont;
			colEnglish.DefaultCellStyle.Font = m_viewModel.EnglishReferenceTextFont;
			m_dataGridReferenceText.DefaultCellStyle.Font =
				m_originalDefaultFontForCharacterAndDeliveryColumns.AdjustFontSize(m_viewModel.FontSizeUiAdjustment);
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
			TileFormLocation();
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
			{
				m_viewModel.LoadPreviousRelevantBlock();
				m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
			}
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
			UpdateAssignOrApplyButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignOrApplyButtonState();
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

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			Application.AddMessageFilter(this);
		}

		protected override void OnDeactivate(EventArgs e)
		{
			Application.RemoveMessageFilter(this);
			base.OnDeactivate(e);
		}

		[DllImport("user32.dll")]
		private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// If the block view has focus and is in the browser mode, it will eat the keystrokes, so we need to ensure
		/// they post to this window so we can do the accelartor-key thing.
		/// </summary>
		/// <remarks>This is invoked because we implement IMessagFilter and call Application.AddMessageFilter(this)</remarks>
		public bool PreFilterMessage(ref Message m)
		{
			const int WM_KEYDOWN = 0x100;

			if (m.Msg == WM_KEYDOWN)
			{
				if (m_blocksViewer.ContainsFocus && ((Keys) m.WParam | Keys.Control) == 0)
				{
					m_listBoxCharacters.Focus();
					PostMessage(Handle, (uint) m.Msg, m.WParam, m.LParam);
					return true;
				}
			}

			return false;
		}

		private void AssignCharacterDialog_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (m_txtCharacterFilter.Focused || m_txtDeliveryFilter.Focused || m_scriptureReference.VerseControl.ContainsFocus ||
				m_tabControlCharacterSelection.SelectedTab != tabPageSelectCharacter)
				return;

			int selectedIndexOneBased;
			Int32.TryParse(e.KeyChar.ToString(CultureInfo.InvariantCulture), out selectedIndexOneBased);
			if (selectedIndexOneBased < 1 || selectedIndexOneBased > 5)
			{
				// Might be trying to select character by the first letter (e.g. s for Saul)
				e.Handled = HandleCharacterSelectionKeyPress(e);
			}
			else if (m_pnlShortcuts.Visible)
			{
				if (m_listBoxCharacters.Items.Count >= selectedIndexOneBased)
					m_listBoxCharacters.SelectedIndex = selectedIndexOneBased - 1; //listBox is zero-based
			}
		}

		private bool HandleCharacterSelectionKeyPress(KeyPressEventArgs e)
		{
			if (Char.IsLetter(e.KeyChar))
			{
				var charactersStartingWithSelectedLetter =
					CurrentContextCharacters.Where(c => c.ToString().StartsWith(e.KeyChar.ToString(CultureInfo.InvariantCulture), true, CultureInfo.InvariantCulture));
				if (charactersStartingWithSelectedLetter.Count() == 1)
					m_listBoxCharacters.SelectedItem = charactersStartingWithSelectedLetter.Single();
				else
					m_listBoxCharacters.SelectedItem = null;
				return true;
			}
			return false;
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
				case 4: mode = BlocksToDisplay.AllQuotes; break;
				default: mode = BlocksToDisplay.AllScripture; break;
			}

			if (m_toolStripButtonExcludeUserConfirmed.Checked)
				mode |= BlocksToDisplay.ExcludeUserConfirmed;

			m_viewModel.Mode = mode;

			if (m_viewModel.RelevantBlockCount > 0)
			{
				LoadBlock(sender, e);
				m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
				LoadBlockMatchup(sender, e);
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

				m_viewModel.AttemptRefBlockMatchup = false;
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
				m_viewModel.AttemptRefBlockMatchup = true;
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
			using (var dlg = new SplitBlockDlg(m_viewModel.Font, m_viewModel.GetAllBlocksWhichContinueTheQuoteStartedByBlock(m_viewModel.CurrentBlock),
				m_viewModel.GetUniqueCharactersForCurrentReference(), m_viewModel.CurrentBookId))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_viewModel.SplitBlock(dlg.SplitLocations, dlg.SelectedCharacters);
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

		private void m_btnApplyReferenceTextMatches_Click(object sender, EventArgs e)
		{
			m_viewModel.ApplyCurrentReferenceTextMatchup();
			if (m_viewModel.CanNavigateToNextRelevantBlock)
				LoadNextRelevantBlock();
		}

		private void UpdateUpDownButtonStates(object sender, DataGridViewCellEventArgs e)
		{
			m_btnMoveReferenceTextDown.Enabled = e.RowIndex != m_dataGridReferenceText.RowCount - 1;
			m_btnMoveReferenceTextUp.Enabled = e.RowIndex != 0;
		}

		private void HandleMoveReferenceTextUpOrDown_Click(object sender, EventArgs e)
		{
			var upOrDown = sender == m_btnMoveReferenceTextUp ? -1 : 1;
			var source = m_dataGridReferenceText.Rows[m_dataGridReferenceText.CurrentCellAddress.Y];
			var destIndex = source.Index + upOrDown;
			var dest = m_dataGridReferenceText.Rows[destIndex];
			if (colPrimary.Visible)
				SwapValues(source, dest, colPrimary.Index);
			SwapValues(source, dest, colEnglish.Index);
			if (colCharacter.Visible)
				SwapValues(source, dest, colCharacter.Index);
			if (colDelivery.Visible)
				SwapValues(source, dest, colDelivery.Index);

			int iCol = 0;
			while (!m_dataGridReferenceText.Columns[iCol].Visible)
				iCol++;
			m_dataGridReferenceText.CurrentCell = m_dataGridReferenceText.Rows[destIndex].Cells[iCol];
		}

		private void SwapValues(DataGridViewRow source, DataGridViewRow dest, int columnIndex)
		{
			var temp = source.Cells[columnIndex].Value;
			source.Cells[columnIndex].Value = dest.Cells[columnIndex].Value;
			dest.Cells[columnIndex].Value = temp;
		}

		private void HandleCharacterSelectionTabIndexChanged(object sender, EventArgs e)
		{
			m_viewModel.AttemptRefBlockMatchup = m_tabControlCharacterSelection.SelectedTab == tabPageMatchReferenceText;
			m_blocksViewer.HighlightStyle = m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter ?
				BlockGroupingType.Quote : BlockGroupingType.BlockCorrelation;
			ShowMatchReferenceTextTabPage(); // Put this tab back - this is kind of a hack for now
		}

		private void m_dataGridReferenceText_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if ((colPrimary.Visible && e.ColumnIndex == colPrimary.Index) || (!colPrimary.Visible && e.ColumnIndex == colEnglish.Index))
			{
				var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
				m_viewModel.CurrentReferenceTextMatchup.SetReferenceText(e.RowIndex, newValue, 0);
			}
			else if (e.ColumnIndex == colEnglish.Index)
			{
				var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
				m_viewModel.CurrentReferenceTextMatchup.SetReferenceText(e.RowIndex, newValue, 1);
			}
			else if (colCharacter.Visible && e.ColumnIndex == colCharacter.Index)
			{
				var selectedCharacter = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as AssignCharacterViewModel.Character;
				if (selectedCharacter == null)
				{
					var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
					selectedCharacter = colCharacter.Items.Cast<AssignCharacterViewModel.Character>().FirstOrDefault(c => c.LocalizedDisplay == newValue);
					if (selectedCharacter == null)
						throw new Exception("Selected character not found");
				}
				m_viewModel.SetReferenceTextMatchupCharacter(e.RowIndex, selectedCharacter);
			}
			else
			{
				var selectedDelivery = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as AssignCharacterViewModel.Delivery;
				if (selectedDelivery == null)
				{
					var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
					selectedDelivery = colDelivery.Items.Cast<AssignCharacterViewModel.Delivery>().FirstOrDefault(d => d.LocalizedDisplay == newValue);
					if (selectedDelivery == null)
						throw new Exception("Selected delivery not found!");
				}
				m_viewModel.SetReferenceTextMatchupDelivery(e.RowIndex, selectedDelivery);				
			}
			m_userMadeChangesToReferenceTextMatchup = true;
			UpdateAssignOrApplyButtonState();
		}
		#endregion
	}
}
