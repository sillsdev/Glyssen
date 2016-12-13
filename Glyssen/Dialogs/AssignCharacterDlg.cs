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
using SIL.Extensions;
using SIL.Reporting;
using static System.String;

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
		private readonly string m_defaultBlocksViewerText;
		private int m_IndexOfFirstFilterItemRemoved;
		private object[] m_filterItemsForRainbowModeOnly;

		private void HandleStringsLocalized()
		{
			m_viewModel.SetUiStrings(
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.Narrator", "narrator ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BookChapterCharacter", "book title or chapter ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.IntroCharacter", "introduction ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.ExtraCharacter", "section head ({0})"),
				LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.NormalDelivery", "normal"));

			if (m_toolStripComboBoxFilter.Items.Count == m_IndexOfFirstFilterItemRemoved)
			{
				Debug.Assert(m_filterItemsForRainbowModeOnly != null);
				m_toolStripComboBoxFilter.Items.AddRange(m_filterItemsForRainbowModeOnly);
			}
			L10N.LocalizeComboList(m_toolStripComboBoxFilter, "DialogBoxes.AssignCharacterDlg.FilterOptions");
			UpdateFilterItems();

			m_xOfYFmt = m_labelXofY.Text;
			m_singleVoiceCheckboxFmt = m_chkSingleVoice.Text;

			Text = Format(Text, m_viewModel.ProjectName);
		}

		public AssignCharacterDlg(AssignCharacterViewModel viewModel)
		{
			InitializeComponent();

			const int numberOfFilterItemsForRainbowModeOnly = 1;
			m_IndexOfFirstFilterItemRemoved = m_toolStripComboBoxFilter.Items.Count - numberOfFilterItemsForRainbowModeOnly;
			m_filterItemsForRainbowModeOnly = new object[numberOfFilterItemsForRainbowModeOnly];
			for (int i = 0; i < numberOfFilterItemsForRainbowModeOnly; i++)
				m_filterItemsForRainbowModeOnly[i] = m_toolStripComboBoxFilter.Items[m_IndexOfFirstFilterItemRemoved];

			m_viewModel = viewModel;
			if (m_viewModel.CanDisplayReferenceTextForCurrentBlock)
			{
				// We want CheckChanged event to fire, so just setting Checked to true is not enough.
				m_toolStripButtonMatchReferenceText.CheckState = (Settings.Default.AssignCharactersMatchReferenceText ||
																(m_viewModel.Mode & BlocksToDisplay.NotAlignedToReferenceText) != 0)
					? CheckState.Checked
					: CheckState.Unchecked;
				HandleCharacterSelectionTabIndexChanged(m_tabControlCharacterSelection, new EventArgs());
			}
			else
			{
				Debug.Assert(!m_toolStripButtonMatchReferenceText.Checked);
				m_toolStripButtonMatchReferenceText.Enabled = false;
			}

			m_tabControlCharacterSelection.ItemSize = new Size(0, 1);
			m_tabControlCharacterSelection.Location = new Point(m_tabControlCharacterSelection.Location.X, -1);

			m_txtCharacterFilter.CorrectHeight();
			m_txtDeliveryFilter.CorrectHeight();
			if (Settings.Default.AssignCharactersShowGridView || m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation)
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
			m_defaultBlocksViewerText = m_blocksViewer.Text;
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
			SetFontsFromViewModel(this, null);

			m_viewModel.AssignedBlocksIncremented += m_viewModel_AssignedBlocksIncremented;
			m_viewModel.UiFontSizeChanged += SetFontsFromViewModel;

			m_blocksViewer.VisibleChanged += BlocksViewerVisibleChanged;

			SetFilterControlsFromMode();

			m_viewModel.CurrentBookSaved += UpdateSavedText;
			m_viewModel.FilterReset +=HandleFilterReset;

			BlocksViewerOnMinimumWidthChanged(m_blocksViewer, new EventArgs());
			m_blocksViewer.MinimumWidthChanged += BlocksViewerOnMinimumWidthChanged;
		}

		private void UpdateFilterItems()
		{
			if (m_toolStripButtonMatchReferenceText.Checked)
			{
				if (m_toolStripComboBoxFilter.Items.Count == m_IndexOfFirstFilterItemRemoved)
					m_toolStripComboBoxFilter.Items.AddRange(m_filterItemsForRainbowModeOnly);
			}
			else if (m_toolStripComboBoxFilter.Items.Count > m_IndexOfFirstFilterItemRemoved)
			{
				Debug.Assert(m_filterItemsForRainbowModeOnly != null);
				if (m_toolStripComboBoxFilter.SelectedIndex >= m_IndexOfFirstFilterItemRemoved)
				{
					m_toolStripComboBoxFilter.SelectedIndex = 0;
					HandleFilterChanged(m_toolStripComboBoxFilter, new EventArgs());
				}
				while (m_toolStripComboBoxFilter.Items.Count != m_IndexOfFirstFilterItemRemoved)
					m_toolStripComboBoxFilter.Items.RemoveAt(m_IndexOfFirstFilterItemRemoved);
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

		void m_viewModel_AssignedBlocksIncremented(AssignCharacterViewModel sender, int increment, int newMaximum)
		{
			this.SafeInvoke(() =>
			{
				if (m_progressBar.Visible)
				{
					m_progressBar.Maximum = newMaximum;
					m_progressBar.Increment(increment);
				}
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
			Logger.WriteEvent("Initial filter in Identify Speaking Parts dialog: " + mode);

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
			else if ((mode & BlocksToDisplay.NotAlignedToReferenceText) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 6;
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
				});
			}
		}

		private void LoadBlockMatchup(object sender, EventArgs args)
		{
			if (m_blocksViewer.Visible)
			{
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
					string characterId = correlatedBlock.CharacterIsUnclear() ? correlatedBlock.ReferenceBlocks.Single().CharacterId :
						correlatedBlock.CharacterId;

					if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
						row.Cells[colCharacter.Index].Value = (AssignCharacterViewModel.Character) colCharacter.Items[0];
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
						if (row.Cells[colCharacter.Index].Value == null)
							Debug.WriteLine("Problem");
					}
					if (colDelivery.Visible)
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
				}
				colCharacter.ReadOnly = colCharacter.Items.Count == 1 && !m_viewModel.CurrentReferenceTextMatchup.OriginalBlocks.Any(b => b.CharacterIsUnclear());
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

			btn.Enabled = IsCharacterAndDeliverySelectionComplete && IsDirty();
			m_btnReset.Enabled = IsDirty();
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
			return m_viewModel.CurrentReferenceTextMatchup != null && m_viewModel.CurrentReferenceTextMatchup.HasOutstandingChangesToApply;
		}

		private void LoadNextRelevantBlock()
		{
			m_viewModel.LoadNextRelevantBlock();
			//m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
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
			string currentDelivery = IsNullOrEmpty(currentBlock.Delivery) ? AssignCharacterViewModel.Delivery.Normal.Text : currentBlock.Delivery;

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
				else
				{
					if (m_btnApplyReferenceTextMatches.Enabled && m_userMadeChangesToReferenceTextMatchup)
					{
						string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedReferenceTextChangesMessage",
							"The alignment of the reference text to the vernacular script has not been applied. Do you want to save the alignment before navigating?");
						if (MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
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

				m_viewModel.AddCharacterDetailToProject(character, dlg.Gender, dlg.Age);
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
				//m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
			}
		}

		private void m_btnAssign_Click(object sender, EventArgs e)
		{
			SaveSelections();
			MoveOn();
		}

		private void MoveOn()
		{
			if (m_viewModel.AreAllAssignmentsComplete && m_promptToCloseWhenAssignmentsAreComplete)
			{
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.AssignmentsComplete",
					"Assignments Complete");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CloseDialogMessage",
					"All assignments have been made. Would you like to return to the main window?");
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

			Cursor = Cursors.WaitCursor;

			try
			{
				BlocksToDisplay mode;

				switch (m_toolStripComboBoxFilter.SelectedIndex)
				{
					case 0: mode = BlocksToDisplay.NeedAssignments; break;
					case 1: mode = BlocksToDisplay.MissingExpectedQuote; break;
					case 2: mode = BlocksToDisplay.MoreQuotesThanExpectedSpeakers; break;
					case 3: mode = BlocksToDisplay.AllExpectedQuotes; break;
					case 4: mode = BlocksToDisplay.AllQuotes; break;
					case 6: mode = BlocksToDisplay.NotAlignedToReferenceText; break;
					default: mode = BlocksToDisplay.AllScripture; break;
				}

				if (m_toolStripButtonExcludeUserConfirmed.Checked)
					mode |= BlocksToDisplay.ExcludeUserConfirmed;

				Logger.WriteEvent("Changed filter in Identify Speaking Parts dialog: " + mode);

				m_viewModel.Mode = mode;

				if (m_viewModel.RelevantBlockCount > 0)
				{
					LoadBlock(sender, e);
					//m_viewModel.AttemptRefBlockMatchup = m_toolStripButtonGridView.Checked;
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

				m_toolStripButtonGridView.Checked = true;
				m_toolStripButtonHtmlView.Enabled = false;
				m_tabControlCharacterSelection.SelectedTab = tabPageMatchReferenceText;
				Settings.Default.AssignCharactersMatchReferenceText = true;
			}
		}

		private void HandleSelectCharacterCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonMatchReferenceText.Checked == m_toolStripButtonSelectCharacter.Checked)
			{
				m_toolStripButtonMatchReferenceText.Checked = !m_toolStripButtonSelectCharacter.Checked;

				Debug.Assert(!m_toolStripButtonMatchReferenceText.Checked);

				m_toolStripButtonHtmlView.Enabled = true;
				m_tabControlCharacterSelection.SelectedTab = tabPageSelectCharacter;
				Settings.Default.AssignCharactersMatchReferenceText = false;
			}
		}

		private void HandleHtmlViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonGridView.Checked = !m_toolStripButtonHtmlView.Checked;

				Debug.Assert(!m_toolStripButtonGridView.Checked);
				Debug.Assert(!m_viewModel.AttemptRefBlockMatchup);

				//m_viewModel.AttemptRefBlockMatchup = false;
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
				//m_viewModel.AttemptRefBlockMatchup = true;
			}
		}

		private void HandleTaskOrViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			var type = button.Tag as string;
			if (!button.Checked)
			{
				button.Checked = true;

				if (type != null)
				{
					Analytics.Track("Switch" + type, new Dictionary<string, string> {{"dialog", Name}, {type.ToLowerInvariant(), button.ToString()}});
					Logger.WriteEvent("Changed " + type + " to " + button + " in Identify Speaking Parts dialog.");
				}
			}
		}

		private string UnsavedChangesMessageBoxTitle => LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedChanges", "Unsaved Changes");

		private void HandleSplitBlocksClick(object sender, EventArgs e)
		{

			Block blockToSplit;
			if (m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation)
			{
				if (IsDirty() && m_btnApplyReferenceTextMatches.Enabled && m_userMadeChangesToReferenceTextMatchup)
				{
					string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.UnsavedReferenceTextChangesBeforeSplitting",
						"The alignment of the reference text to the vernacular script has not been applied. Do you want to save the alignment before splitting this block?");
					if (MessageBox.Show(this, msg, UnsavedChangesMessageBoxTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
						m_viewModel.ApplyCurrentReferenceTextMatchup();
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
			m_viewModel.ApplyCurrentReferenceTextMatchup();
			MoveOn();
		}

		private void UpdateRowSpecificButtonStates(object sender, DataGridViewCellEventArgs e)
		{
			m_btnMoveReferenceTextDown.Enabled = e.RowIndex != m_dataGridReferenceText.RowCount - 1;
			m_btnMoveReferenceTextUp.Enabled = e.RowIndex != 0;
			m_menuInsertIntoSelectedRowOnly.Enabled = GetColumnsIntoWhichHeSaidCanBeInserted(m_dataGridReferenceText.Rows[e.RowIndex]).Any();
		}

		private void HandleMoveReferenceTextUpOrDown_Click(object sender, EventArgs e)
		{
			bool down = sender == m_btnMoveReferenceTextDown;
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
			Block.GetSwappedReferenceText((string) rowA.Cells[columnIndex].Value, (string) rowB.Cells[columnIndex].Value,
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
						"Match reference text to these blocks");
			}
			else
			{
				m_viewModel.AttemptRefBlockMatchup = false;
				m_blocksViewer.Text = m_defaultBlocksViewerText;
			}
			UpdateFilterItems();
		}

		private void m_dataGridReferenceText_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.ColumnIndex == colCharacter.Index || e.ColumnIndex == colDelivery.Index)
			{
				var matchup = m_viewModel.CurrentReferenceTextMatchup;
				var correlatedBlock = matchup.CorrelatedBlocks[e.RowIndex];
				if (correlatedBlock.MultiBlockQuote == MultiBlockQuote.Continuation || correlatedBlock.MultiBlockQuote == MultiBlockQuote.ChangeOfDelivery)
				{
					var index = matchup.IndexOfStartBlockInBook;
					var verseWhereQuoteStarts = m_viewModel.FindStartOfQuote(ref index).LastVerseNum;

					var msgFmt = LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.CannotChangeMidQuoteBlock", "The {0} cannot be changed for this block because it is in the middle of a quote. " +
						"To change this, select the block that begins this quote (in verse {1}). If this quote block needs to be split up so that different characters or deliveries can be assigned, " +
						"use the Split command.");
					var columnName = m_dataGridReferenceText.Columns[e.ColumnIndex].HeaderText;
					MessageBox.Show(this, Format(msgFmt, columnName, verseWhereQuoteStarts), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
						throw new Exception("Selected delivery not found!");
				}
				m_viewModel.SetReferenceTextMatchupDelivery(e.RowIndex, selectedDelivery);
			}
			else
			{
				var matchup = m_viewModel.CurrentReferenceTextMatchup;

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
					var selectedCharacter =
						m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as AssignCharacterViewModel.Character;
					if (selectedCharacter == null)
					{
						var newValue = m_dataGridReferenceText.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;
						selectedCharacter =
							colCharacter.Items.Cast<AssignCharacterViewModel.Character>().FirstOrDefault(c => c.LocalizedDisplay == newValue);
					}

					if (selectedCharacter == AssignCharacterViewModel.Character.Narrator && colDelivery.Visible)
					{
						// Narrators are never allowed to have a delivery other than normal.
						// Unfortunately, by the time we call IsBlockAssignedToUnknownCharacterDeliveryPair below,
						// the line that sets the character in the reference text matchup will have already reset
						// the delivery. This leaves the UI out of synch with the data in the block, so we need
						// to fix that first.
						var deliveryCell = m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index];
						if (deliveryCell.Value as string != AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay)
							deliveryCell.Value = AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay;
					}
					m_viewModel.SetReferenceTextMatchupCharacter(e.RowIndex, selectedCharacter);

					var block = matchup.CorrelatedBlocks[e.RowIndex];
					if (m_viewModel.IsBlockAssignedToUnknownCharacterDeliveryPair(block))
					{
						// The first one should always be "normal" - we want a more specific one, if any.
						var delivery = m_viewModel.GetDeliveriesForCharacter(selectedCharacter).LastOrDefault();
						string deliveryAsString = delivery == null
							? AssignCharacterViewModel.Delivery.Normal.LocalizedDisplay
							: delivery.LocalizedDisplay;
						m_dataGridReferenceText.Rows[e.RowIndex].Cells[colDelivery.Index].Value = deliveryAsString;
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

		private void HandleHeSaidInserted(int iRow, int level, string text)
		{
			m_dataGridReferenceText.CellValueChanged -= m_dataGridReferenceText_CellValueChanged;
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

		private void HandleBlocksViewerSelectionChanged(object sender, EventArgs e)
		{
			this.SafeInvoke(SetReferenceTextGridRowToAnchorRow);
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
		#endregion
	}
}
