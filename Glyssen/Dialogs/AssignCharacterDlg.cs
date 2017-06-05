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
using SIL.Extensions;
using SIL.Reporting;
using SIL.Scripture;
using SIL.Windows.Forms.Extensions;
using static System.String;

namespace Glyssen.Dialogs
{
	public partial class AssignCharacterDlg : FormWithPersistedSettings, IMessageFilter
	{
		private readonly AssignCharacterViewModel m_viewModel;
		private string m_xOfYFmt;
		private string m_singleVoiceCheckboxFmt;
		private bool m_promptToCloseWhenTaskIsComplete;
		int m_characterListHoveredIndex = -1;
		private readonly ToolTip m_characterListToolTip = new ToolTip();
		private bool m_formLoading;
		private readonly FontProxy m_originalDefaultFontForLists;
		private readonly FontProxy m_originalDefaultFontForCharacterAndDeliveryColumns;
		private Font m_primaryReferenceTextFont;
		private Font m_englishReferenceTextFont;
		private bool m_userMadeChangesToReferenceTextMatchup;
		private string m_defaultBlocksViewerText;
		private readonly int m_indexOfFirstFilterItemRemoved;
		private readonly object[] m_filterItemsForRainbowModeOnly;
		private bool m_addingCharacterDelivery;
		private bool m_askedUserAboutAssigningOnDoubleClick;

		private void HandleStringsLocalized()
		{
			m_viewModel.SetUiStrings(
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.Narrator", "narrator ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BookChapterCharacter", "book title or chapter ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IntroCharacter", "introduction ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.ExtraCharacter", "section head ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NormalDelivery", "normal"));

			if (m_toolStripComboBoxFilter.Items.Count == m_indexOfFirstFilterItemRemoved)
			{
				Debug.Assert(m_filterItemsForRainbowModeOnly != null);
				m_toolStripComboBoxFilter.Items.AddRange(m_filterItemsForRainbowModeOnly);
			}
			L10N.LocalizeComboList(m_toolStripComboBoxFilter, "DialogBoxes.AssignCharacterDlg.FilterOptions");
			UpdateFilterItems();

			m_defaultBlocksViewerText = m_blocksViewer.Text;
			m_xOfYFmt = m_labelXofY.Text;
			m_singleVoiceCheckboxFmt = m_chkSingleVoice.Text;

			Text = Format(Text, m_viewModel.ProjectName);

			m_CharacterOrDeliveryContextMenuItemMoveUp.Text = m_RefTextContextMenuItemMoveUp.Text;
			m_CharacterOrDeliveryContextMenuItemMoveUp.ToolTipText = m_RefTextContextMenuItemMoveUp.ToolTipText;
			m_CharacterOrDeliveryContextMenuItemMoveDown.Text = m_RefTextContextMenuItemMoveDown.Text;
			m_CharacterOrDeliveryContextMenuItemMoveDown.ToolTipText = m_RefTextContextMenuItemMoveDown.ToolTipText;
		}

		public AssignCharacterDlg(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();

			const int numberOfFilterItemsForRainbowModeOnly = 1;
			m_indexOfFirstFilterItemRemoved = m_toolStripComboBoxFilter.Items.Count - numberOfFilterItemsForRainbowModeOnly;
			m_filterItemsForRainbowModeOnly = new object[numberOfFilterItemsForRainbowModeOnly];
			for (int i = 0; i < numberOfFilterItemsForRainbowModeOnly; i++)
				m_filterItemsForRainbowModeOnly[i] = m_toolStripComboBoxFilter.Items[m_indexOfFirstFilterItemRemoved];

			m_viewModel = viewModel;

			m_scriptureReference.VerseControl.GetLocalizedBookName = L10N.GetLocalizedBookNameFunc(m_scriptureReference.VerseControl.GetLocalizedBookName);

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			if (m_viewModel.CanDisplayReferenceTextForCurrentBlock)
			{
				// We want CheckChanged event to fire, so just setting Checked to true is not enough.
				m_toolStripButtonMatchReferenceText.CheckState = (Settings.Default.AssignCharactersMatchReferenceText || m_viewModel.DoingAlignmentTask) ?
					CheckState.Checked : CheckState.Unchecked;
				HandleCharacterSelectionTabIndexChanged(m_tabControlCharacterSelection, new EventArgs());
			}
			else
			{
				if (m_viewModel.Mode == BlocksToDisplay.NotAlignedToReferenceText)
					m_viewModel.Mode = BlocksToDisplay.NotYetAssigned;
				Debug.Assert(!m_toolStripButtonMatchReferenceText.Checked);
				m_toolStripButtonMatchReferenceText.Enabled = false;
			}

			m_tabControlCharacterSelection.ItemSize = new Size(0, 1);
			m_tabControlCharacterSelection.Location = new Point(m_tabControlCharacterSelection.Location.X, -1);

			m_txtCharacterFilter.CorrectHeight();
			m_txtDeliveryFilter.CorrectHeight();
			m_blocksViewer.ViewType = ScriptBlocksViewType.Grid;

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
			m_viewModel.CorrelatedBlockCharacterAssignmentChanged += HandleCorrelatedBlockCharacterAssignmentChanged;

			UpdateProgressBarForMode();

			m_dataGridReferenceText.DataError += HandleDataGridViewDataError;
			colPrimary.HeaderText = m_viewModel.PrimaryReferenceTextName;

			colCharacter.DisplayMember = m_listBoxCharacters.DisplayMember = "LocalizedDisplay";
			colDelivery.DisplayMember = m_listBoxDeliveries.DisplayMember = "LocalizedDisplay";
			m_originalDefaultFontForLists = new FontProxy(m_listBoxCharacters.Font);
			m_originalDefaultFontForCharacterAndDeliveryColumns = new FontProxy(m_dataGridReferenceText.DefaultCellStyle.Font);
			SetFontsFromViewModel(this, null);

			m_viewModel.AssignedBlocksIncremented += m_viewModel_AssignedBlocksIncremented;
			m_viewModel.UiFontSizeChanged += SetFontsFromViewModel;

			m_blocksViewer.VisibleChanged += BlocksViewerVisibleChanged;

			SetFilterControlsFromMode();

			m_viewModel.CurrentBookSaved += UpdateSavedText;
			m_viewModel.FilterReset += HandleFilterReset;

			BlocksViewerOnMinimumWidthChanged(m_blocksViewer, new EventArgs());
			m_blocksViewer.MinimumWidthChanged += BlocksViewerOnMinimumWidthChanged;
		}

		private void UpdateFilterItems()
		{
			if (m_toolStripButtonMatchReferenceText.Checked)
			{
				if (m_toolStripComboBoxFilter.Items.Count == m_indexOfFirstFilterItemRemoved)
					m_toolStripComboBoxFilter.Items.AddRange(m_filterItemsForRainbowModeOnly);
			}
			else if (m_toolStripComboBoxFilter.Items.Count > m_indexOfFirstFilterItemRemoved)
			{
				Debug.Assert(m_filterItemsForRainbowModeOnly != null);
				if (m_toolStripComboBoxFilter.SelectedIndex >= m_indexOfFirstFilterItemRemoved)
				{
					m_toolStripComboBoxFilter.SelectedIndex = 0;
					HandleFilterChanged(m_toolStripComboBoxFilter, new EventArgs());
				}
				while (m_toolStripComboBoxFilter.Items.Count != m_indexOfFirstFilterItemRemoved)
					m_toolStripComboBoxFilter.Items.RemoveAt(m_indexOfFirstFilterItemRemoved);
			}
		}

		private void BlocksViewerOnMinimumWidthChanged(object sender, EventArgs eventArgs)
		{
			m_splitContainer.Panel1MinSize = Math.Max(m_splitContainer.Panel1MinSize, m_blocksViewer.MinimumSize.Width + m_splitContainer.Panel1.Padding.Horizontal);
		}

		void HandleDataGridViewDataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Analytics.ReportException(e.Exception);
			ErrorReport.ReportFatalException(e.Exception);
			throw e.Exception;
		}

		void HandleFilterReset(object sender, EventArgs e)
		{
			UpdateNavigationButtonState();
		}

		void m_viewModel_AssignedBlocksIncremented(AssignCharacterViewModel sender, int increment)
		{
			this.SafeInvoke(() =>
			{
				if (m_progressBar.Visible)
				{
					m_progressBar.Maximum = m_viewModel.RelevantBlockCount;
					m_progressBar.Increment(increment);
				}
			}, GetType().FullName + ".m_viewModel_AssignedBlocksIncremented");
		}

		private void UpdateProgressBarForMode()
		{
			if (m_viewModel.InTaskMode)
			{
				m_progressBar.Visible = true;
				m_progressBar.Maximum = m_viewModel.RelevantBlockCount;
				m_progressBar.Value = m_viewModel.CompletedBlockCount;
				m_progressBar.UnitName = m_viewModel.DoingAlignmentTask ?
					LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.PassageProgressUnitName", "Passages") : null;
				m_progressBar.Invalidate();
				if (m_viewModel.IsCurrentTaskComplete)
				{
					if (m_promptToCloseWhenTaskIsComplete)
					{
						// At some point while using this dialog, the user had one of the two "task" filters selected
						// and had not yet completed the task, so now that they are switching back to that filter, we
						// need to let them know they ARE done with that task now.
						ShowCompletionMessage();
					}
				}
				else
					m_promptToCloseWhenTaskIsComplete = true;
			}
			else
			{
				m_progressBar.Visible = false;
			}
		}

		private void ShowCompletionMessage()
		{
			string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.TaskCompleteTitle", "Task Complete");
			string msg = m_viewModel.DoingAssignmentTask ?
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.AssignmentsComplete",
					"All character assignments have been made. ") :
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.AlignmentsComplete",
					"Alignment of blocks to the Reference Text is complete. ");

			if (!char.IsWhiteSpace(msg.Last()))
				msg += " ";

			msg += LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CloseDialogMessage", "Would you like to return to the main window?");
			if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Close();
				return;
			}
			m_promptToCloseWhenTaskIsComplete = false;
		}

		private void SetFilterControlsFromMode()
		{
			var mode = m_viewModel.Mode;
			Logger.WriteEvent("Initial filter in Identify Speaking Parts dialog: " + mode);

			if (mode == BlocksToDisplay.NotYetAssigned)
				m_toolStripComboBoxFilter.SelectedIndex = 0;
			else if (mode == BlocksToDisplay.NotAssignedAutomatically)
				m_toolStripComboBoxFilter.SelectedIndex = 1;
			else if ((mode & BlocksToDisplay.MissingExpectedQuote) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 2;
			else if ((mode & BlocksToDisplay.MoreQuotesThanExpectedSpeakers) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 3;
			else if ((mode & BlocksToDisplay.AllExpectedQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 4;
			else if ((mode & BlocksToDisplay.AllQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 5;
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 6;
			else if ((mode & BlocksToDisplay.NotAlignedToReferenceText) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 7;
			else
				// ReSharper disable once NotResolvedInText
				throw new InvalidEnumArgumentException("mode", (int)mode, typeof(BlocksToDisplay));
		}

		private void BlocksViewerVisibleChanged(object sender, EventArgs args)
		{
			LoadBlock(sender, args);
			LoadBlockMatchup(sender, args);
		}

		private void LoadBlock(object sender, EventArgs args)
		{
			if (m_viewModel.CanDisplayReferenceTextForCurrentBlock)
			{
				m_toolStripButtonMatchReferenceText.Enabled = true;
			}
			else
			{
				if (m_toolStripButtonMatchReferenceText.Checked)
				{
					// We want CheckChanged event to fire, so just setting Checked to true is not enough.
					m_toolStripButtonSelectCharacter.CheckState = CheckState.Checked;
					Debug.Assert(!m_toolStripButtonMatchReferenceText.Checked);
				}
				m_toolStripButtonMatchReferenceText.Enabled = false;
			}

			if (m_blocksViewer.Visible)
			{
				this.SafeInvoke(() =>
				{
					SetReferenceTextGridRowToAnchorRow();
					UpdateDisplay();
					UpdateNavigationButtonState();
				}, GetType().FullName + ".LoadBlock");
			}
		}

		private void LoadBlockMatchup(object sender, EventArgs args)
		{
			if (m_blocksViewer.Visible)
			{
				this.SafeInvoke(UpdateReferenceTextTabPageDisplay, GetType().FullName + ".LoadBlockMatchup");
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
			UpdateNavigationIndexLabel();
			m_chkSingleVoice.Text = Format(m_singleVoiceCheckboxFmt, m_viewModel.CurrentBookId);

			m_viewModel.GetBlockVerseRef().SendScrReference();

			HideCharacterFilter();
			m_btnAssign.Enabled = false;

			LoadCharacterListBox(m_viewModel.GetUniqueCharactersForCurrentReference());
			UpdateShortcutDisplay();

			m_chkSingleVoice.Checked = m_viewModel.IsCurrentBookSingleVoice;

			m_menuBtnSplitBlock.Enabled = !CharacterVerseData.IsCharacterExtraBiblical(m_viewModel.CurrentBlock.CharacterId);
		}

		private void UpdateNavigationIndexLabel()
		{
			if (m_labelXofY.Visible)
			{
				Debug.Assert(m_viewModel.RelevantBlockCount >= m_viewModel.CurrentBlockDisplayIndex);
				m_labelXofY.Text = Format(m_xOfYFmt, m_viewModel.CurrentBlockDisplayIndex, m_viewModel.RelevantBlockCount);
			}
		}

		private void UpdateInsertHeSaidButtonState()
		{
			m_menuInsertIntoAllEmptyCells.Enabled = m_menuInsertIntoSelectedRowOnly.Enabled =
				GetColumnsIntoWhichHeSaidCanBeInserted(m_dataGridReferenceText.CurrentRow).Any();
			if (!m_menuInsertIntoAllEmptyCells.Enabled)
			{
				foreach (DataGridViewRow row in m_dataGridReferenceText.Rows)
				{
					if (GetColumnsIntoWhichHeSaidCanBeInserted(row).Any())
					{
						m_menuInsertIntoAllEmptyCells.Enabled = true;
						break;
					}
				}
			}
		}

		private IEnumerable<int> GetColumnsIntoWhichHeSaidCanBeInserted(DataGridViewRow row)
		{
			var matchup = m_viewModel.CurrentReferenceTextMatchup;
			if (row != null && matchup != null && (matchup.CorrelatedBlocks[row.Index].
					CharacterIs(m_viewModel.CurrentBookId, CharacterVerseData.StandardCharacter.Narrator) ||
				matchup.CorrelatedBlocks[row.Index].CharacterId == CharacterVerseData.kUnknownCharacter))
			{
				if (Block.IsEmptyVerseReferenceText(row.Cells[colEnglish.Index].Value as string))
					yield return colEnglish.Index;
				if (colPrimary.Visible && Block.IsEmptyVerseReferenceText(row.Cells[colPrimary.Index].Value as string))
					yield return colPrimary.Index;
			}
		}

		private void UpdateReferenceTextTabPageDisplay()
		{
			UpdateNavigationIndexLabel();
			UpdateNavigationButtonState();

			m_dataGridReferenceText.EditMode = DataGridViewEditMode.EditProgrammatically;

			m_dataGridReferenceText.CellValueChanged -= m_dataGridReferenceText_CellValueChanged;

			m_dataGridReferenceText.RowCount = 0;
			colCharacter.Items.Clear();
			colDelivery.Items.Clear();

			if (m_viewModel.CurrentReferenceTextMatchup != null)
			{
				foreach (AssignCharacterViewModel.Character character in m_viewModel.GetCharactersForCurrentReferenceTextMatchup())
					colCharacter.Items.Add(character);

				colCharacter.ReadOnly = colCharacter.Items.Count == 1 &&
					!m_viewModel.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear());

				foreach (AssignCharacterViewModel.Delivery delivery in m_viewModel.GetDeliveriesForCurrentReferenceTextMatchup())
					colDelivery.Items.Add(delivery);

				m_dataGridReferenceText.RowCount = m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks.Count;
				colPrimary.Visible = m_viewModel.HasSecondaryReferenceText;
				// BryanW says it will be easier to train people if this column is always visible, even when there is nothing to do.
				//colCharacter.Visible = colCharacter.Items.Count > 1 || m_viewModel.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear());
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
					SetCharacterCellValue(row, correlatedBlock);
					if (colDelivery.Visible)
						SetDeliveryCellValue(row, correlatedBlock);
				}
				m_dataGridReferenceText.EditMode = DataGridViewEditMode.EditOnEnter;
				var cellToMakeCurrent = m_dataGridReferenceText.FirstDisplayedCell;
				if (cellToMakeCurrent.ReadOnly)
				{
					int c = cellToMakeCurrent.ColumnIndex + 1;
					while (c < m_dataGridReferenceText.ColumnCount &&
					(m_dataGridReferenceText.Rows[cellToMakeCurrent.RowIndex].Cells[c].ReadOnly ||
						!m_dataGridReferenceText.Rows[cellToMakeCurrent.RowIndex].Cells[c].Visible))
						c++;
					if (c < m_dataGridReferenceText.ColumnCount)
						cellToMakeCurrent = m_dataGridReferenceText.Rows[cellToMakeCurrent.RowIndex].Cells[c];
				}
				m_dataGridReferenceText.CurrentCell = cellToMakeCurrent;
				m_dataGridReferenceText.BeginEdit(true);
			}

			UpdateInsertHeSaidButtonState();
			UpdateAssignOrApplyAndResetButtonState();

			m_dataGridReferenceText.CellValueChanged += m_dataGridReferenceText_CellValueChanged;
		}

		private void SetDeliveryCellValue(DataGridViewRow row, Block correlatedBlock)
		{
			var delivery = correlatedBlock.Delivery;
			if (IsNullOrEmpty(delivery))
				delivery = correlatedBlock.ReferenceBlocks.Single().Delivery;
			if (IsNullOrEmpty(delivery))
				delivery = ((AssignCharacterViewModel.Delivery)colDelivery.Items[0]).LocalizedDisplay;
			row.Cells[colDelivery.Index].Value = delivery;
			row.Cells[colDelivery.Index].ReadOnly =
				(row.Cells[colCharacter.Index].Value as AssignCharacterViewModel.Character) == AssignCharacterViewModel.Character.Narrator;
		}

		private void SetCharacterCellValue(DataGridViewRow row, Block correlatedBlock)
		{
			string characterId = correlatedBlock.CharacterIsUnclear() ? correlatedBlock.ReferenceBlocks.Single().CharacterId :
				correlatedBlock.CharacterId;

			if (CharacterVerseData.IsCharacterStandard(characterId))
			{
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					row.Cells[colCharacter.Index].Value = (AssignCharacterViewModel.Character)colCharacter.Items[0];
				else
					row.Cells[colCharacter.Index].ReadOnly = true;
			}
			else
			{
				foreach (AssignCharacterViewModel.Character character in colCharacter.Items)
				{
					if (character.CharacterId == characterId)
					{
						row.Cells[colCharacter.Index].Value = character;
						break;
					}
				}
			}
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

		private void UpdateAssignOrApplyAndResetButtonState()
		{
			Button btn = m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter
				? m_btnAssign
				: m_btnApplyReferenceTextMatches;

			btn.Enabled = IsCharacterAndDeliverySelectionComplete && IsDirty;
			m_btnReset.Enabled = IsDirty;
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
			return !col.Visible ||
				m_dataGridReferenceText.Rows.Cast<DataGridViewRow>().All(row => row.Cells[col.Index].Value != null
					|| row.Cells[col.Index].ReadOnly);
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

		private bool IsDirty
		{
			get
			{
				if (m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter)
				{
					return m_viewModel.IsModified((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem,
						(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
				}
				return m_viewModel.CurrentReferenceTextMatchup != null && m_viewModel.CurrentReferenceTextMatchup.HasOutstandingChangesToApply;
			}
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
			UpdateAssignOrApplyAndResetButtonState();
		}

		private void SelectCharacter()
		{
			var character = m_viewModel.GetCharacterToSelectForCurrentBlock(CurrentContextCharacters);
			if (character != null)
				m_listBoxCharacters.SelectedItem = character;
		}

		private IEnumerable<AssignCharacterViewModel.Character> CurrentContextCharacters =>
			m_listBoxCharacters.Items.Cast<AssignCharacterViewModel.Character>();

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
			string currentDelivery = IsNullOrEmpty(currentBlock.Delivery) ? AssignCharacterViewModel.Delivery.Normal.Text : currentBlock.Delivery;

			if (m_listBoxDeliveries.Items.Count == 1)
				m_listBoxDeliveries.SelectedIndex = 0;
			else
			{
				if (currentBlock.CharacterId == ((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem).CharacterId)
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
				(AssignCharacterViewModel.Delivery)m_listBoxDeliveries.SelectedItem);
		}

		private bool IsOkayToLeaveBlock()
		{
			bool result = true;

			if (m_dataGridReferenceText.IsCurrentCellInEditMode)
				m_dataGridReferenceText.EndEdit(DataGridViewDataErrorContexts.LeaveControl);

			if (IsDirty)
			{
				if (m_tabControlCharacterSelection.SelectedTab == tabPageSelectCharacter)
				{
					if (m_btnAssign.Enabled)
					{
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedChangesMessage",
							"The Character and Delivery selections have not been submitted. Do you want to save your changes before navigating?");
						if (MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
							SaveSelections();
					}
					else if (m_listBoxCharacters.SelectedIndex < 0)
					{
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NoSelectionMessage",
							"You have not selected a Character and Delivery. Would you like to leave without changing the assignment?");
						result = MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
							DialogResult.Yes;
					}
					else
					{
						Debug.Assert(m_listBoxCharacters.SelectedIndex > -1 && m_listBoxDeliveries.SelectedIndex < 0);
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NoDeliveryMessage",
							"You have selected a Character but no Delivery. Would you like to discard your selection and leave without changing the assignment?");
						result = MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) ==
							DialogResult.Yes;
					}
				}
				else if (m_userMadeChangesToReferenceTextMatchup)
				{
					if (m_btnApplyReferenceTextMatches.Enabled)
					{
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedReferenceTextChangesMessage",
							"The alignment of the reference text to the vernacular script has not been applied. Do you want to save the alignment before navigating?");
						if (MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
							result = CheckRefTextValuesAndApplyMatchup();
					}
					else
					{
						// Technically, we shouyld have a separate message for the case where the Delivery column is showing, but in practice
						// there is no way for the user to set the value for a cell in the Delivery column to null, so even though our code
						// checks for this, it can't really happen.
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IncompleteCharacterAssignments",
							"You have not finished specifying the character information for every block. Would you like to discard the changes you have made?");
						result = MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo,
							MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
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
			if (IsNullOrWhiteSpace(character))
				return;

			var existingItem = CurrentContextCharacters.FirstOrDefault(c => c.ToString() == character);
			if (existingItem != null)
			{
				m_listBoxCharacters.SelectedItem = existingItem;
				return;
			}

			using (var dlg = new NewCharacterDlg(character))
			{
				MainForm.LogDialogDisplay(dlg);
				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				m_viewModel.StoreCharacterDetail(character, dlg.Gender, dlg.Age);
			}

			var newItem = new AssignCharacterViewModel.Character(character);
			m_listBoxCharacters.Items.Add(newItem);
			m_listBoxCharacters.SelectedItem = newItem;
		}

		private void AddNewDelivery(string delivery)
		{
			if (IsNullOrWhiteSpace(delivery))
				return;
			m_listBoxDeliveries.SelectedItem = m_listBoxDeliveries.Items.Cast<AssignCharacterViewModel.Delivery>()
				.FirstOrDefault(d => d.Text == delivery);
			if (m_listBoxDeliveries.SelectedItem != null)
				return;
			var newItem = new AssignCharacterViewModel.Delivery(delivery);
			m_listBoxDeliveries.Items.Add(newItem);
			m_listBoxDeliveries.SelectedItem = newItem;
		}

		private void SetFontsFromViewModel(object sender, EventArgs args)
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

		#region Form overrides & key-press handling
		/// ------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			m_formLoading = true;

			base.OnLoad(e);
			m_blocksViewer.BlocksGridSettings = Settings.Default.AssignCharactersBlockContextGrid;
			if (Settings.Default.AssignCharactersSliderLocation > 0)
				m_splitContainer.SplitterDistance = Settings.Default.AssignCharactersSliderLocation;

			m_pnlShortcuts.Height = m_listBoxCharacters.ItemHeight * 5;

			TileFormLocation();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			m_formLoading = false;
			if (m_viewModel.RelevantBlockCount == 0)
				m_blocksViewer.ShowNothingMatchesFilterMessage();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Settings.Default.AssignCharactersBlockContextGrid = m_blocksViewer.BlocksGridSettings;
			base.OnClosing(e);
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
				if (m_blocksViewer.ContainsFocus && ((Keys)m.WParam | Keys.Control) == 0)
				{
					m_listBoxCharacters.Focus();
					PostMessage(Handle, (uint)m.Msg, m.WParam, m.LParam);
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
		#endregion

		#region Event handlers & helpers
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
			}
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			SaveSelections();
			MoveOn();
		}

		private void MoveOn()
		{
			if (m_viewModel.IsCurrentTaskComplete && m_promptToCloseWhenTaskIsComplete)
				ShowCompletionMessage();

			if (m_viewModel.CanNavigateToNextRelevantBlock)
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedCharacter = (AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem;

			LoadDeliveryListBox(m_viewModel.GetDeliveriesForCharacter(selectedCharacter));
			HideDeliveryFilter();
			if (selectedCharacter != null && selectedCharacter.IsNarrator)
				m_llMoreDel.Enabled = false;
			UpdateAssignOrApplyAndResetButtonState();
		}

		private void m_listBoxDeliveries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateAssignOrApplyAndResetButtonState();
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

		private void HandleCorrelatedBlockCharacterAssignmentChanged(AssignCharacterViewModel sender, int index)
		{
			Debug.Assert(index < m_dataGridReferenceText.RowCount);
			// REVIEW: Might need to disable CellValueChanged handler
			SetCharacterCellValue(m_dataGridReferenceText.Rows[index], m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks[index]);
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

			Cursor = Cursors.WaitCursor;

			try
			{
				BlocksToDisplay mode;

				switch (m_toolStripComboBoxFilter.SelectedIndex)
				{
					case 0:
						mode = BlocksToDisplay.NotYetAssigned;
						break;
					case 1:
						mode = BlocksToDisplay.NotAssignedAutomatically;
						break;
					case 2:
						mode = BlocksToDisplay.MissingExpectedQuote;
						break;
					case 3:
						mode = BlocksToDisplay.MoreQuotesThanExpectedSpeakers;
						break;
					case 4:
						mode = BlocksToDisplay.AllExpectedQuotes;
						break;
					case 5:
						mode = BlocksToDisplay.AllQuotes;
						break;
					case 7:
						mode = BlocksToDisplay.NotAlignedToReferenceText;
						break;
					default:
						mode = BlocksToDisplay.AllScripture;
						break;
				}

				Logger.WriteEvent("Changed filter in Identify Speaking Parts dialog: " + mode);

				m_viewModel.Mode = mode;

				if (m_viewModel.RelevantBlockCount > 0)
				{
					LoadBlock(sender, e);
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
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private void HandleMatchReferenceTextCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonMatchReferenceText.Checked == m_toolStripButtonSelectCharacter.Checked)
			{
				m_toolStripButtonSelectCharacter.Checked = !m_toolStripButtonMatchReferenceText.Checked;

				Debug.Assert(!m_toolStripButtonSelectCharacter.Checked);

				m_tabControlCharacterSelection.SelectedTab = tabPageMatchReferenceText;
				Settings.Default.AssignCharactersMatchReferenceText = true;
			}
		}

		private void HandleSelectCharacterCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonMatchReferenceText.Checked == m_toolStripButtonSelectCharacter.Checked)
			{
				IsOkayToLeaveBlock(); // returns true whether reply is Yes or No
				m_toolStripButtonMatchReferenceText.Checked = !m_toolStripButtonSelectCharacter.Checked;

				Debug.Assert(!m_toolStripButtonMatchReferenceText.Checked);

				m_tabControlCharacterSelection.SelectedTab = tabPageSelectCharacter;
				Settings.Default.AssignCharactersMatchReferenceText = false;
			}
		}

		private void HandleTaskToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
			{
				button.Checked = true;

				Analytics.Track("SwitchTask", new Dictionary<string, string> {{"dialog", Name}, {"task", button.ToString()}});
				Logger.WriteEvent($"Changed Task to {button} in Identify Speaking Parts dialog.");
			}
		}

		private string UnsavedChangesMessageBoxTitle => LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedChanges", "Unsaved Changes");

		private void HandleSplitBlocksClick(object sender, EventArgs e)
		{
			Block blockToSplit;
			if (m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation)
			{
				if (IsDirty && m_btnApplyReferenceTextMatches.Enabled && m_userMadeChangesToReferenceTextMatchup)
				{
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedReferenceTextChangesBeforeSplitting",
						"The alignment of the reference text to the vernacular script has not been applied. Do you want to save the alignment before splitting this block?");
					if (MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
						if (!CheckRefTextValuesAndApplyMatchup())
							return;
				}

				var matchup = m_viewModel.CurrentReferenceTextMatchup;
				blockToSplit = matchup.GetCorrespondingOriginalBlock(matchup.CorrelatedBlocks[m_dataGridReferenceText.CurrentCellAddress.Y]);
			}
			else
				blockToSplit = m_viewModel.CurrentBlock;
			using (var dlg = new SplitBlockDlg(m_viewModel.Font, m_viewModel.GetAllBlocksWhichContinueTheQuoteStartedByBlock(blockToSplit),
				m_viewModel.GetUniqueCharactersForCurrentReference(), m_viewModel.CurrentBookId))
			{
				MainForm.LogDialogDisplay(dlg);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					Logger.WriteMinorEvent("Split block in {0} into {1} parts.", m_scriptureReference.VerseControl.VerseRef.ToString(),
						dlg.SplitLocations.Count + 1);
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
			if (!m_chkSingleVoice.Checked && m_viewModel.InTaskMode && !m_viewModel.IsCurrentTaskComplete)
				m_promptToCloseWhenTaskIsComplete = true;
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
					if (!IsNullOrEmpty(hoveredCharacter.LocalizedAlias))
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
			if (CheckRefTextValuesAndApplyMatchup())
				MoveOn();
		}

		private bool CheckRefTextValuesAndApplyMatchup()
		{
			var problems = m_viewModel.CurrentReferenceTextMatchup.GetInvalidReferenceBlocksAtAnyLevel().ToList();
			if (problems.Any())
			{
				string msg;
				var firstProblem = problems.First();
				var refTextColumnIndex = firstProblem.Item2 == 1 ? colPrimary.Index : colEnglish.Index;
				var language = m_dataGridReferenceText.Columns[refTextColumnIndex].HeaderText;
				if (problems.Count == 1)
				{
					msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.SingleReferenceTextEndsWithVerse",
						"One of the {0} reference texts entered ends with a verse number.");
				}
				else if (problems.All(p => p.Item2 == firstProblem.Item2))
				{
					msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.MultipleCellsOfSameReferenceTextsEndWithVerse",
						"Some of the {0} reference texts entered end with a verse number.");
				}
				else
				{
					msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.MultipleCellsOfDifferentReferenceTextsEndWithVerse",
						"Some of the reference texts entered end with a verse number.");
				}

				msg = String.Format(msg, language) + " " + LocalizationManager.GetString(
					"DialogBoxes.AssignCharacterDlg.AllowReferenceTextsEndingWithVerse",
					"Would you like to correct this before applying your changes?");
				if (MessageBox.Show(this, msg, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					var col = firstProblem.Item2 == 1 && colPrimary.Visible ? colPrimary : colEnglish;
					m_dataGridReferenceText.CurrentCell = m_dataGridReferenceText.Rows[firstProblem.Item1].Cells[col.Index];
					return false;
				}
			}
			m_viewModel.ApplyCurrentReferenceTextMatchup();
			return true;
		}

		private void UpdateRowSpecificButtonStates(object sender, DataGridViewCellEventArgs e)
		{
			m_btnMoveReferenceTextDown.Enabled = e.RowIndex != m_dataGridReferenceText.RowCount - 1;
			m_btnMoveReferenceTextUp.Enabled = e.RowIndex != 0;
			m_menuInsertIntoSelectedRowOnly.Enabled = GetColumnsIntoWhichHeSaidCanBeInserted(m_dataGridReferenceText.Rows[e.RowIndex]).Any();
		}

		private void HandleMoveReferenceTextUpOrDown_Click(object sender, EventArgs e)
		{
			bool down = (sender == m_btnMoveReferenceTextDown || (sender as ToolStripButton)?.Name == m_RefTextContextMenuItemMoveDown.Name);
			var currentRowIndex = m_dataGridReferenceText.CurrentCellAddress.Y;
			var rowA = m_dataGridReferenceText.Rows[down ? currentRowIndex : currentRowIndex - 1];
			var rowB = m_dataGridReferenceText.Rows[rowA.Index + 1];
			if (colPrimary.Visible)
				SwapRefText(rowA, rowB, colPrimary.Index);
			SwapRefText(rowA, rowB, colEnglish.Index);
			if (m_viewModel.CurrentReferenceTextMatchup.CanChangeCharacterAndDeliveryInfo(rowA.Index, rowB.Index))
			{
				if (!colCharacter.ReadOnly)
					SwapValues(rowA, rowB, colCharacter.Index);
				if (colDelivery.Visible)
					SwapValues(rowA, rowB, colDelivery.Index);
			}

			int iCol = 0;
			while (!m_dataGridReferenceText.Columns[iCol].Visible)
				iCol++;
			m_dataGridReferenceText.CurrentCell = m_dataGridReferenceText.Rows[currentRowIndex + (down ? 1 : -1)].Cells[iCol];
		}

		private void SwapValues(DataGridViewRow rowA, DataGridViewRow rowB, int columnIndex)
		{
			var temp = rowA.Cells[columnIndex].Value;
			rowA.Cells[columnIndex].Value = rowB.Cells[columnIndex].Value;
			rowB.Cells[columnIndex].Value = temp;
		}

		private void SwapRefText(DataGridViewRow rowA, DataGridViewRow rowB, int columnIndex)
		{
			string newRowAValue, newRowBValue;
			Block.GetSwappedReferenceText((string)rowA.Cells[columnIndex].Value, (string)rowB.Cells[columnIndex].Value,
				out newRowAValue, out newRowBValue);
			rowA.Cells[columnIndex].Value = newRowAValue;
			rowB.Cells[columnIndex].Value = newRowBValue;
		}

		private void HandleCharacterSelectionTabIndexChanged(object sender, EventArgs e)
		{
			if (m_tabControlCharacterSelection.SelectedTab == tabPageMatchReferenceText)
			{
				m_viewModel.AttemptRefBlockMatchup = true;
				m_blocksViewer.Text =
					LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BlocksViewerInstructionsForMatchReferenceText",
						"Match reference text for each colored row.");
				m_saveStatus.Visible = false;
				m_blocksViewer.ContentBorderStyle = m_dataGridReferenceText.BorderStyle;
			}
			else
			{
				m_viewModel.AttemptRefBlockMatchup = false;
				m_blocksViewer.Text = m_defaultBlocksViewerText;
				m_saveStatus.Visible = true;
				m_blocksViewer.ContentBorderStyle = BorderStyle.None;
			}
			UpdateFilterItems();
		}

		private void m_dataGridReferenceText_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if ((e.ColumnIndex == colCharacter.Index) && m_dataGridReferenceText.IsCurrentCellDirty)
			{
				var matchup = m_viewModel.CurrentReferenceTextMatchup;
				if (matchup == null)
					return; // This can happen when transitioning from one block matchup to another.
				var correlatedBlock = matchup.CorrelatedBlocks[e.RowIndex];
				if (correlatedBlock.IsContinuationOfPreviousBlockQuote)
				{
					var index = e.RowIndex - 1;
					while (index >= 0 && matchup.CorrelatedBlocks[index].IsContinuationOfPreviousBlockQuote)
						index--;
					int verseWhereQuoteStarts;
					if (index >= 0)
						verseWhereQuoteStarts = matchup.CorrelatedBlocks[index].LastVerseNum;
					else
					{
						index = matchup.IndexOfStartBlockInBook;
						verseWhereQuoteStarts = m_viewModel.FindStartOfQuote(ref index).LastVerseNum;
					}
					var msgFmt = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CannotChangeMidQuoteBlock", "The character cannot be changed for this block because it is in the middle of a quote. " +
						"To change this, select the block that begins this quote (in verse {0}). If this quote block needs to be split up so that different characters can be assigned, " +
						"use the Split command.");
					MessageBox.Show(this, Format(msgFmt, verseWhereQuoteStarts), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					m_dataGridReferenceText.CancelEdit();
				}
			}
		}

		private void m_dataGridReferenceText_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == colDelivery.Index)
			{
				Debug.Assert(colDelivery.Visible);
				var selectedDelivery = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as AssignCharacterViewModel.Delivery;
				if (selectedDelivery == null)
				{
					var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
					selectedDelivery =
						colDelivery.Items.Cast<AssignCharacterViewModel.Delivery>().FirstOrDefault(d => d.LocalizedDisplay == newValue);
					if (selectedDelivery == null)
					{
						var block = m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks[e.RowIndex];
						throw new Exception($"Selected delivery '{newValue}' not found! " +
							$"({m_viewModel.CurrentBookId} {block.ChapterNumber}:{block.InitialStartVerseNumber})");
					}
				}
				m_viewModel.SetReferenceTextMatchupDelivery(e.RowIndex, selectedDelivery);
			}
			else
			{
				var matchup = m_viewModel.CurrentReferenceTextMatchup;
				var block = matchup.CorrelatedBlocks[e.RowIndex];

				if ((colPrimary.Visible && e.ColumnIndex == colPrimary.Index) ||
					(!colPrimary.Visible && e.ColumnIndex == colEnglish.Index))
				{
					var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
					matchup.SetReferenceText(e.RowIndex, newValue, 0);
				}
				else if (e.ColumnIndex == colEnglish.Index)
				{
					var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
					matchup.SetReferenceText(e.RowIndex, newValue, 1);
				}
				else
				{
					Debug.Assert(e.ColumnIndex == colCharacter.Index);
					Debug.Assert(!colCharacter.ReadOnly);
					var selectedCharacter = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex]
						.Value as AssignCharacterViewModel.Character;
					if (selectedCharacter == null)
					{
						var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
						selectedCharacter = colCharacter.Items.Cast<AssignCharacterViewModel.Character>()
							.First(c => c.LocalizedDisplay == newValue);
					}

					Logger.WriteMinorEvent($"Setting character to {selectedCharacter.CharacterId} for " +
						$"block {block.ChapterNumber}:{block.InitialStartVerseNumber} {block.GetText(true)}");

					if (selectedCharacter == AssignCharacterViewModel.Character.Narrator && colDelivery.Visible)
					{
						// Narrators are never allowed to have a delivery other than normal.
						// Unfortunately, by the time we call IsBlockAssignedToUnknownCharacterDeliveryPair below,
						// the line that sets the character in the reference text matchup will have already reset
						// the delivery. This leaves the UI out of synch with the data in the block, so we need
						// to fix that first.
						var deliveryCell = m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index];
						if (deliveryCell.Value as string != AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay)
						{
							Logger.WriteMinorEvent("Character is Narrator. Forcing delivery to normal.");
							deliveryCell.Value = AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay;
						}
					}
					m_viewModel.SetReferenceTextMatchupCharacter(e.RowIndex, selectedCharacter);

					if (!m_addingCharacterDelivery && m_viewModel.IsBlockAssignedToUnknownCharacterDeliveryPair(block))
					{
						// The first one should always be "normal" - we want a more specific one, if any.
						var existingValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index].Value;
						var delivery = m_viewModel.GetDeliveriesForCharacter(selectedCharacter).LastOrDefault();
						if (existingValue != null || (delivery != null && delivery != AssignCharacterViewModel.Delivery.Normal))
						{
							string deliveryAsString = delivery == null
								? AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay
								: delivery.LocalizedDisplay;
							if (existingValue as string != deliveryAsString)
							{
								Logger.WriteMinorEvent($"Unknown Character-delivery pair. Forcing delivery to {deliveryAsString}.");
								m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index].Value = deliveryAsString;
							}
						}
					}

					if (colDelivery.Visible)
					{
						m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index].ReadOnly =
							selectedCharacter == AssignCharacterViewModel.Character.Narrator;
					}
				}
				UpdateInsertHeSaidButtonState();
			}
			m_userMadeChangesToReferenceTextMatchup = true;
			UpdateAssignOrApplyAndResetButtonState();
		}

		private void m_dataGridReferenceText_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (!DesignMode && e.ColumnIndex == colCharacter.Index &&
				e.RowIndex >= 0 &&
				m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly &&
				m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
			{
				var correlatedBlock = m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks[e.RowIndex];
				string characterId = correlatedBlock.CharacterId;
				e.PaintBackground(e.ClipBounds, (e.State & DataGridViewElementStates.Selected) > 0);

				var cellBounds = e.CellBounds;
				var adjust = (new DataGridViewComboBoxEditingControl()).Margin.Top;
				cellBounds.Height -= adjust;
				cellBounds.Y += adjust;
				TextRenderer.DrawText(e.Graphics, AssignCharacterViewModel.Character.GetCharacterIdForUi(characterId),
					e.CellStyle.Font, cellBounds, e.CellStyle.ForeColor,
					TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.GlyphOverhangPadding);
				e.Handled = true;
			}
		}

		private void m_dataGridReferenceText_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != colEnglish.Index && e.ColumnIndex != colPrimary.Index)
				return;

			if (m_dataGridReferenceText.CurrentCellAddress.Y < 0 || (!Focused && (m_dataGridReferenceText.EditingControl == null || !m_dataGridReferenceText.EditingControl.Focused)))
			{
				if (m_dataGridReferenceText.CurrentRow != null)
				{
					const int kExtraHeightToallowForBordersAndMargin = 3;
					var minHeight = m_dataGridReferenceText.RowTemplate.Height * 3;
					if (String.IsNullOrEmpty(m_dataGridReferenceText.CurrentCell.Value as string))
					{
						var clipboardText = Clipboard.GetText();
						if (clipboardText.Length > 0)
						{
							using (Graphics g = CreateGraphics())
							{
								TextFormatFlags flags = ComputeTextFormatFlagsForCellStyleAlignment(m_viewModel.Font.RightToLeftScript);
								var heightNeeded = DataGridViewCell.MeasureTextHeight(g,
										clipboardText, m_dataGridReferenceText.CurrentCell.InheritedStyle.Font,
										m_dataGridReferenceText.Columns[e.ColumnIndex].Width, flags) +
									kExtraHeightToallowForBordersAndMargin;
								minHeight = Math.Max(minHeight, heightNeeded);
							}
						}
					}
					if (m_dataGridReferenceText.CurrentRow.Height < minHeight)
						m_dataGridReferenceText.CurrentRow.MinimumHeight = minHeight;
				}
			}
		}

		private static TextFormatFlags ComputeTextFormatFlagsForCellStyleAlignment(bool rightToLeft)
		{
			TextFormatFlags tff = TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
			if (rightToLeft)
				tff |= TextFormatFlags.Right | TextFormatFlags.RightToLeft;
			else
				tff |= TextFormatFlags.Left;
			return tff;
		}

		private void m_dataGridReferenceText_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != colEnglish.Index && e.ColumnIndex != colPrimary.Index)
				return;
			if (e.RowIndex >= 0 && e.RowIndex < m_dataGridReferenceText.RowCount)
				m_dataGridReferenceText.Rows[e.RowIndex].MinimumHeight = m_dataGridReferenceText.RowTemplate.MinimumHeight;
		}

		private void HandleMouseEnterButtonThatAffectsEntireGridRow(object sender, EventArgs e)
		{
			if (m_dataGridReferenceText.CurrentCellAddress.Y < 0)
				return;
			m_dataGridReferenceText.EditMode = DataGridViewEditMode.EditProgrammatically;
			if (m_dataGridReferenceText.IsCurrentCellInEditMode)
				m_dataGridReferenceText.EndEdit(DataGridViewDataErrorContexts.LeaveControl);
			m_dataGridReferenceText.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			m_dataGridReferenceText.Rows[m_dataGridReferenceText.CurrentCellAddress.Y].Selected = true;
		}

		private void HandleMouseLeaveInsertHeSaidButton(object sender, EventArgs e)
		{
			if (m_btnInsertHeSaid.DropDown.Visible)
			{
				if (m_dataGridReferenceText.MultiSelect)
					m_dataGridReferenceText.ClearSelection();
			}
			else
				HandleMouseLeaveButtonThatAffectsEntireGridRow(sender, e);
		}

		private void HandleMouseLeaveButtonThatAffectsEntireGridRow(object sender, EventArgs e)
		{
			if (m_btnInsertHeSaid.DropDown.Visible)
				return;
			m_dataGridReferenceText.MultiSelect = false;
			m_dataGridReferenceText.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			m_dataGridReferenceText.EditMode = DataGridViewEditMode.EditOnEnter;
		}

		private void HandleInsertHeSaidCheckChanged(object sender, EventArgs e)
		{
			if (sender == m_menuInsertIntoAllEmptyCells)
				m_menuInsertIntoSelectedRowOnly.Checked = !m_menuInsertIntoAllEmptyCells.Checked;
			else
				m_menuInsertIntoAllEmptyCells.Checked = !m_menuInsertIntoSelectedRowOnly.Checked;
		}

		private void HandleInsertHeSaidClicked(object sender, EventArgs e)
		{
			int row = -1;
			if (m_menuInsertIntoSelectedRowOnly.Checked)
			{
				if (!m_menuInsertIntoSelectedRowOnly.Enabled)
					return; // ENHANCE: Give user feedback? Beep? message?
				row = m_dataGridReferenceText.CurrentCellAddress.Y;
			}
			m_viewModel.CurrentReferenceTextMatchup.InsertHeSaidText(row, HandleHeSaidInserted);
			if (m_dataGridReferenceText.IsCurrentCellInEditMode)
				m_dataGridReferenceText.EndEdit(DataGridViewDataErrorContexts.CurrentCellChange);
			UpdateAssignOrApplyAndResetButtonState();
		}

		private void HandleInsertContextMenuHeSaidClicked(object sender, EventArgs e)
		{
			m_viewModel.CurrentReferenceTextMatchup.InsertHeSaidText(m_dataGridReferenceText.CurrentCellAddress.Y, HandleHeSaidInserted);
			UpdateAssignOrApplyAndResetButtonState();
		}

		private void HandleHeSaidInserted(int iRow, int level, string text)
		{
			m_dataGridReferenceText.CellValueChanged -= m_dataGridReferenceText_CellValueChanged;
			m_userMadeChangesToReferenceTextMatchup = true;
			var column = level == 0 && colPrimary.Visible ? colPrimary : colEnglish;
			m_dataGridReferenceText.Rows[iRow].Cells[column.Index].Value = text;
			if (!colCharacter.ReadOnly)
				m_dataGridReferenceText.Rows[iRow].Cells[colCharacter.Index].Value = (AssignCharacterViewModel.Character)colCharacter.Items[0];
			m_dataGridReferenceText.CellValueChanged += m_dataGridReferenceText_CellValueChanged;
		}

		private void HandleResetMatchupClick(object sender, EventArgs e)
		{
			m_viewModel.SetBlockMatchupForCurrentVerse();
		}

		private void HandleMouseEnterInsertHeSaidButton(object sender, EventArgs e)
		{
			m_dataGridReferenceText.EditMode = DataGridViewEditMode.EditProgrammatically;
			if (m_dataGridReferenceText.IsCurrentCellInEditMode)
				m_dataGridReferenceText.EndEdit(DataGridViewDataErrorContexts.LeaveControl);

			bool selectedRowOnly;
			if (sender == m_btnInsertHeSaid)
				selectedRowOnly = m_menuInsertIntoSelectedRowOnly.Checked;
			else
				selectedRowOnly = sender == m_menuInsertIntoSelectedRowOnly;

			if (selectedRowOnly)
			{
				//if (m_dataGridReferenceText.MultiSelect)
				//	m_dataGridReferenceText.ClearSelection();
				if (!m_menuInsertIntoSelectedRowOnly.Enabled)
					return;
			}

			m_dataGridReferenceText.MultiSelect = true;
			m_dataGridReferenceText.SelectionMode = DataGridViewSelectionMode.CellSelect;

			if (selectedRowOnly)
			{
				foreach (var iCol in GetColumnsIntoWhichHeSaidCanBeInserted(m_dataGridReferenceText.CurrentRow))
					m_dataGridReferenceText.CurrentRow.Cells[iCol].Selected = true;
			}
			else
			{
				foreach (DataGridViewRow row in m_dataGridReferenceText.Rows)
				{
					foreach (var iCol in GetColumnsIntoWhichHeSaidCanBeInserted(row))
						row.Cells[iCol].Selected = true;
				}
			}
		}

		private void SetReferenceTextGridRowToAnchorRow()
		{
			if (m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation && m_dataGridReferenceText.CurrentCell != null)
			{
				var matchup = m_viewModel.CurrentReferenceTextMatchup;
				var iRow = matchup.CorrelatedBlocks.IndexOf(matchup.CorrelatedAnchorBlock);
				if (m_dataGridReferenceText.CurrentCellAddress.Y != iRow)
				{
					if (m_dataGridReferenceText.IsCurrentCellInEditMode)
						m_dataGridReferenceText.EndEdit(DataGridViewDataErrorContexts.CurrentCellChange);
					m_dataGridReferenceText.CurrentCell =
						m_dataGridReferenceText.Rows[iRow].Cells[m_dataGridReferenceText.CurrentCellAddress.X];
					if (!m_dataGridReferenceText.IsCurrentCellInEditMode)
						m_dataGridReferenceText.BeginEdit(true);
				}
			}
		}

		private void m_ContextMenuItemSplitText_Click(object sender, EventArgs e)
		{
			if (!m_dataGridReferenceText.IsCurrentCellInEditMode)
				m_dataGridReferenceText.BeginEdit(false);
			var editingCtrl = (DataGridViewTextBoxEditingControl)m_dataGridReferenceText.EditingControl;
			editingCtrl.Click += HandleClickToSplitText;
			editingCtrl.HandleDestroyed -= HandleClickToSplitText;
		}

		private void HandleClickToSplitText(object sender, EventArgs eventArgs)
		{
			var editingCtrl = (DataGridViewTextBoxEditingControl)sender;
			if (editingCtrl.SelectionStart <= 0 || editingCtrl.SelectionStart >= editingCtrl.TextLength || editingCtrl.SelectionLength > 0)
			{
				var msgFmt = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.InvalidSplitTextAction",
					"To split the reference text, click the location where it is to be split. Do not attempt to make a text selection " +
					"or click at the very start or end of the text. You will need to select the {0} command again to enable splitting now.");
				MessageBox.Show(this, String.Format(msgFmt, m_ContextMenuItemSplitText.Text), ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				editingCtrl.Click -= HandleClickToSplitText;
				return;
			}
			var currCell = m_dataGridReferenceText.CurrentCell;
			var textBeforeInsertionPoint = editingCtrl.Text.Substring(0, editingCtrl.SelectionStart);
			var textAfterInsertionPoint = editingCtrl.Text.Substring(editingCtrl.SelectionStart);
			var destCell = GetSplitTextDestination();
			if (destCell.RowIndex < currCell.RowIndex)
			{
				destCell.Value = textBeforeInsertionPoint;
				currCell.Value = textAfterInsertionPoint;
			}
			else
			{
				currCell.Value = textBeforeInsertionPoint;
				destCell.Value = textAfterInsertionPoint;
			}
			m_dataGridReferenceText.CurrentCell = destCell;
			if (GetSplitTextDestination() == null)
				editingCtrl.Click -= HandleClickToSplitText;
		}

		private DataGridViewCell GetSplitTextDestination()
		{
			var rowIndex = m_dataGridReferenceText.CurrentCellAddress.Y;
			var colIndex = m_dataGridReferenceText.CurrentCellAddress.X;
			if (rowIndex < m_dataGridReferenceText.RowCount - 1)
			{
				var cellBelow = m_dataGridReferenceText.Rows[rowIndex + 1].Cells[colIndex];
				if (String.IsNullOrEmpty(cellBelow.Value as String))
					return cellBelow;
			}
			if (rowIndex > 0)
			{
				var cellAbove = m_dataGridReferenceText.Rows[rowIndex - 1].Cells[colIndex];
				if (String.IsNullOrEmpty(cellAbove.Value as String))
					return cellAbove;
			}
			return null;
		}

		private void m_contextMenuRefTextCell_Opening(object sender, CancelEventArgs e)
		{
			m_ContextMenuItemSplitText.Enabled = GetSplitTextDestination() != null;
			m_ContextMenuItemInsertHeSaid.Enabled = GetColumnsIntoWhichHeSaidCanBeInserted(m_dataGridReferenceText.CurrentRow).Any();
			m_RefTextContextMenuItemMoveUp.Enabled = m_dataGridReferenceText.CurrentCellAddress.Y != 0;
			m_RefTextContextMenuItemMoveDown.Enabled = m_dataGridReferenceText.CurrentCellAddress.Y != m_dataGridReferenceText.RowCount - 1;
		}

		private void m_contextMenuCharacterOrDeliveryCell_Opening(object sender, CancelEventArgs e)
		{
			m_CharacterOrDeliveryContextMenuItemMoveUp.Enabled = m_dataGridReferenceText.CurrentCellAddress.Y != 0;
			m_CharacterOrDeliveryContextMenuItemMoveDown.Enabled = m_dataGridReferenceText.CurrentCellAddress.Y != m_dataGridReferenceText.RowCount - 1;
		}

		private void m_dataGridReferenceText_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				m_dataGridReferenceText.CurrentCell = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex];
		}

		private void ContextMenuItemAddCharacterOrDelivery_Click(object sender, EventArgs e)
		{
			using (var dlg = new AddCharacterDlg(m_viewModel))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					m_addingCharacterDelivery = true;

					var block = m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks[m_dataGridReferenceText.CurrentCellAddress.Y];
					m_viewModel.AddPendingProjectCharacterVerseData(block, dlg.SelectedCharacter, dlg.SelectedDelivery);
					AddNewDeliveryIfNeeded(dlg.SelectedDelivery);

					AssignCharacterViewModel.Character newCharacter = dlg.SelectedCharacter;
					foreach (AssignCharacterViewModel.Character character in colCharacter.Items)
					{
						if (character == newCharacter)
						{
							newCharacter = null;
							break;
						}
					}
					if (newCharacter != null)
					{
						colCharacter.Items.Add(newCharacter);
					}

					var currentCharacterCell = m_dataGridReferenceText.CurrentRow.Cells[colCharacter.Index];
					if (currentCharacterCell == m_dataGridReferenceText.CurrentCell && m_dataGridReferenceText.IsCurrentCellInEditMode)
					{
						m_dataGridReferenceText.CurrentCell = m_dataGridReferenceText.CurrentRow.Cells[colEnglish.Index];
						if (currentCharacterCell.Value as AssignCharacterViewModel.Character != dlg.SelectedCharacter)
							currentCharacterCell.Value = dlg.SelectedCharacter;
						m_dataGridReferenceText.CurrentCell = currentCharacterCell;
					}
					if (currentCharacterCell.Value as AssignCharacterViewModel.Character != dlg.SelectedCharacter)
						currentCharacterCell.Value = dlg.SelectedCharacter;

					m_addingCharacterDelivery = false;
				}
			}
		}

		private void AddNewDeliveryIfNeeded(AssignCharacterViewModel.Delivery selectedDelivery)
		{
			AssignCharacterViewModel.Delivery newDelivery = selectedDelivery;
			if (colDelivery.Visible)
			{
				foreach (AssignCharacterViewModel.Delivery delivery in colDelivery.Items)
				{
					if (delivery == newDelivery)
					{
						newDelivery = null;
						break;
					}
				}
			}
			else
			{
				if (newDelivery == AssignCharacterViewModel.Delivery.Normal)
					return;

				colDelivery.Items.Clear();
				colDelivery.Visible = true;
				colDelivery.Items.Add(AssignCharacterViewModel.Delivery.Normal);
			}

			if (newDelivery != null)
			{
				colDelivery.Items.Add(newDelivery);
			}

			var currentDeliveryCell = m_dataGridReferenceText.CurrentRow.Cells[colDelivery.Index];
			if (currentDeliveryCell == m_dataGridReferenceText.CurrentCell && m_dataGridReferenceText.IsCurrentCellInEditMode)
			{
				var dropDown = (DataGridViewComboBoxEditingControl)m_dataGridReferenceText.EditingControl;
				dropDown.SelectedItem = selectedDelivery;
			}
			else
			{
				if (currentDeliveryCell.Value as AssignCharacterViewModel.Delivery != selectedDelivery)
					currentDeliveryCell.Value = selectedDelivery;
			}
		}

		private void m_listBoxCharacters_DoubleClick(object sender, EventArgs e)
		{
			if (m_btnAssign.Enabled && m_listBoxDeliveries.Items.Count == 1)
			{
				if (!Settings.Default.AssignCharactersDoubleClickShouldAssign)
				{
					if (m_askedUserAboutAssigningOnDoubleClick)
						return;
					var msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UseDoubleClickAsShortcut",
						"You just double-clicked. By default, Glyssen slows you down so you will take time to carefully review each selection. Do " +
						"you want Glyssen to let you go faster by interpreting a double click as your confirmation of the selection, so you won't have to " +
						"click the {0} button?");
					if (MessageBox.Show(this, Format(msg, m_btnAssign.Text.Replace("&", Empty)), ProductName, MessageBoxButtons.YesNo) == DialogResult.No)
					{
						m_askedUserAboutAssigningOnDoubleClick = true;
						return;
					}
					Settings.Default.AssignCharactersDoubleClickShouldAssign = true;
				}
				m_btnAssign.PerformClick();
			}
			else if (m_listBoxCharacters.SelectedIndex >= 0)
			{
				m_listBoxDeliveries.Focus();
			}
		}
		#endregion

		private void m_listBoxDeliveries_DoubleClick(object sender, EventArgs e)
		{
			if (m_btnAssign.Enabled && Settings.Default.AssignCharactersDoubleClickShouldAssign)
				m_btnAssign.PerformClick();
		}
	}
}
