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
using System.Linq;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Gecko.Events;
using L10NSharp;
using L10NSharp.UI;
using Palaso.UI.WindowsForms.PortableSettingsProvider;
using Paratext;
using ProtoScript.Character;
using ProtoScript.Controls;
using SIL.ScriptureControls;
using SIL.ScriptureUtils;

namespace ProtoScript.Dialogs
{
	public partial class AssignCharacterDialog : Form
	{
		private readonly AssignCharacterViewModel m_viewModel;
		private const string kCssClassContext = "context";
		private const int kContextBlocksBackward = 10;
		private const int kContextBlocksForward = 10;

		private ToolTip m_toolTip;
		private string m_xOfYFmt;
		private bool m_updatingContext = true;

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
			
			if (Properties.Settings.Default.AssignCharacterDialogFormSettings == null)
				Properties.Settings.Default.AssignCharacterDialogFormSettings = FormSettings.Create(this);

			m_txtCharacterFilter.CorrectHeight();
			m_txtDeliveryFilter.CorrectHeight();
			if (Properties.Settings.Default.AssignCharactersShowGridView)
				m_toolStripButtonGridView.Checked = true;

			var books = new BookSet();
			foreach (var bookId in viewModel.IncludedBooks)
				books.Add(bookId);
			m_scriptureReference.VerseControl.BooksPresentSet = books;
			m_scriptureReference.VerseControl.ShowEmptyBooks = false;

			m_scriptureReference.VerseControl.AllowVerseSegments = false;
			// TODO (PG-117): Set versification according to project
			m_scriptureReference.VerseControl.Versification = viewModel.Versification;
			m_scriptureReference.VerseControl.VerseRefChanged += m_scriptureReference_VerseRefChanged;

			m_viewModel = viewModel;
			m_viewModel.BackwardContextBlockCount = kContextBlocksBackward;
			m_viewModel.ForwardContextBlockCount = kContextBlocksForward;

			colText.DefaultCellStyle.Font = m_viewModel.Font;
			var origFont = m_dataGridViewBlocks.DefaultCellStyle.Font;
			m_dataGridViewBlocks.DefaultCellStyle.Font = new Font(origFont.FontFamily,
				origFont.SizeInPoints + m_viewModel.FontSizeUiAdjustment, origFont.Style);
			if (m_viewModel.RightToLeft)
				m_dataGridViewBlocks.CellPainting += HandleDataGridViewBlocksCellPainting;

			UpdateProgressBarForMode();

			HandleStringsLocalized();

			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
			m_viewModel.AssignedBlocksIncremented += (sender, args) => { if (m_progressBar.Visible) m_progressBar.Increment(1); };

			SetFilterControlsFromMode();

			m_blocksDisplayBrowser.VisibleChanged += (sender, args) => LoadBlock();
			m_dataGridViewBlocks.VisibleChanged += (sender, args) => BeginInvoke(new Action(() =>
			{
				LoadBlock();
				if (m_dataGridViewBlocks.Visible)
					m_dataGridViewBlocks.AutoResizeColumns();
			}));
			m_dataGridViewBlocks.RowHeightChanged += AdjustScrollingInResponseToRowSizeChange;
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
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 2;
			else
				throw new InvalidEnumArgumentException("mode", (int)mode, typeof(BlocksToDisplay));

			if ((mode & BlocksToDisplay.ExcludeUserConfirmed) != 0)
				m_toolStripButtonExcludeUserConfirmed.Checked = true;
		}

		public void LoadBlock()
		{
			UpdateContextBlocksDisplay();
			UpdateDisplay();
			UpdateNavigationButtonState();
		}

		private void UpdateContextBlocksDisplay()
		{
			if (m_blocksDisplayBrowser.Visible)
				m_blocksDisplayBrowser.DisplayHtml(m_viewModel.Html);
			else
			{
				m_updatingContext = true;
				SuspendLayout();
				// Need to clear the selction here and again below here because some of the property setters on
				// DataGridView have the side-effect of creating a selection. And since we might be changing the row
				// count, we can't afford to have HandleDataGridViewBlocksCellValueNeeded getting called with an
				// index that is out of range for the new book.
				m_dataGridViewBlocks.ClearSelection();
				m_dataGridViewBlocks.MultiSelect = m_viewModel.CurrentBlock.MultiBlockQuote != MultiBlockQuote.None;
				colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				m_dataGridViewBlocks.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				m_dataGridViewBlocks.RowCount = m_viewModel.BlockCountForCurrentBook;
				m_dataGridViewBlocks.ClearSelection(); // see note, above.
				var firstRow = m_viewModel.CurrentBlockIndexInBook;
				var lastRow = firstRow;
				m_dataGridViewBlocks.Rows[firstRow].Selected = true;
				if (m_viewModel.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Start)
				{
					foreach (var i in m_viewModel.GetIndicesOfQuoteContinuationBlocks(m_viewModel.CurrentBlock))
					{
						m_dataGridViewBlocks.Rows[i].Selected = true;
						lastRow = i;
					}
				}
				colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
				m_dataGridViewBlocks.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
				ScrollDesiredRowsIntoView(firstRow, lastRow);

				ResumeLayout();
				m_updatingContext = false;
			}
		}

		private void ScrollDesiredRowsIntoView(int firstRow, int lastRow)
		{
			int precedingContextRows = 4;
			int followingContextRows = Math.Min(2, m_dataGridViewBlocks.RowCount - lastRow - 1);
			var lastRowLocation = m_dataGridViewBlocks.GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
			while ((lastRowLocation.Height == 0 || (firstRow != lastRow &&
				lastRowLocation.Y + lastRowLocation.Height > m_dataGridViewBlocks.ClientRectangle.Height)) && precedingContextRows >= 0)
			{
				var firstRowOfContextToMakeVisible = Math.Max(0, firstRow - precedingContextRows--);
				m_dataGridViewBlocks.FirstDisplayedScrollingRowIndex = firstRowOfContextToMakeVisible;

				if (followingContextRows > 0)
					followingContextRows--;
				lastRowLocation = m_dataGridViewBlocks.GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
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
			LoadBlock();
		}

		private void LoadPreviousRelevantBlock()
		{
			m_viewModel.LoadPreviousRelevantBlock();
			LoadBlock();
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

		private bool UserConfirmSaveChangesIfNecessary()
		{
			if (m_listBoxCharacters.SelectedIndex > -1 && IsDirty())
			{
				string title = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChanges", "Unsaved Changes");
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.UnsavedChangesMessage", "The Character and Delivery selections for this clip have not been submitted. Do you want to save your changes before navigating?");
				return MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes;
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

			if (Properties.Settings.Default.AssignCharactersBlockContextGrid != null)
				Properties.Settings.Default.AssignCharactersBlockContextGrid.InitializeGrid(m_dataGridViewBlocks);

			base.OnLoad(e);

			if (Properties.Settings.Default.AssignCharactersBlockContextGrid != null)
				Properties.Settings.Default.AssignCharactersBlockContextGrid.InitializeGrid(m_dataGridViewBlocks);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			m_dataGridViewBlocks.ColumnWidthChanged += HandleDataGridViewBlocksColumnWidthChanged;
		}

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
				if (MessageBox.Show(this, msg, title, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					Close();
					return;
				}
			}
			if (!m_viewModel.CanNavigateToNextRelevantBlock)
				LoadNextRelevantBlock();
		}

		private void m_listBoxCharacters_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadDeliveryListBox(m_viewModel.GetDeliveriesForCharacter((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem));
			HideDeliveryFilter();
			if (((AssignCharacterViewModel.Character)m_listBoxCharacters.SelectedItem).IsNarrator)
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
			if (UserConfirmSaveChangesIfNecessary())
				SaveSelections();
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

			switch (e.KeyChar)
			{
				case '1':
					m_listBoxCharacters.SelectedIndex = 0;
					break;
				case '2':
					m_listBoxCharacters.SelectedIndex = 1;
					break;
				case '3':
					m_listBoxCharacters.SelectedIndex = 2;
					break;
				case '4':
					m_listBoxCharacters.SelectedIndex = 3;
					break;
				case '5':
					m_listBoxCharacters.SelectedIndex = 4;
					break;
			}
		}

		private void AssignCharacterDialog_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && m_btnAssign.Enabled)
				m_btnAssign.PerformClick();
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
				LoadBlock();
			}
			else
			{
				if (m_blocksDisplayBrowser.Visible)
					m_blocksDisplayBrowser.DisplayHtml(String.Empty);
				else
				{
					m_updatingContext = true;
					SuspendLayout();
					m_dataGridViewBlocks.ClearSelection();
					m_dataGridViewBlocks.RowCount = 0;
					colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
					ResumeLayout();
					m_updatingContext = false;
				}
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

				SuspendLayout();
				m_blocksDisplayBrowser.Visible = true;
				m_dataGridViewBlocks.Visible = false;
				m_dataGridViewBlocks.Dock = DockStyle.None;
				m_blocksDisplayBrowser.Dock = DockStyle.Fill;
				ResumeLayout();
				Properties.Settings.Default.AssignCharactersShowGridView = false;
			}
		}

		private void HandleDataGridViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonHtmlView.Checked = !m_toolStripButtonGridView.Checked;

				Debug.Assert(!m_toolStripButtonHtmlView.Checked);

				SuspendLayout();
				m_dataGridViewBlocks.Visible = true;
				m_blocksDisplayBrowser.Visible = false;
				m_blocksDisplayBrowser.Dock = DockStyle.None;
				m_dataGridViewBlocks.Dock = DockStyle.Fill;
				ResumeLayout();
				Properties.Settings.Default.AssignCharactersShowGridView = true;
			}
		}

		private void HandleViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
				button.Checked = true;
		}

		private void HandleDataGridViewBlocksCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			var block = m_viewModel.GetNthBlockInCurrentBook(e.RowIndex);
			if (e.ColumnIndex == colReference.Index)
				e.Value = m_viewModel.GetBlockReferenceString(block);
			else if (e.ColumnIndex == colCharacter.Index)
				e.Value = AssignCharacterViewModel.Character.GetCharacterIdForUi(block.CharacterId, CurrentContextCharacters);
			else if (e.ColumnIndex == colDelivery.Index)
				e.Value = block.Delivery;
			else if (e.ColumnIndex == colText.Index)
				e.Value = block.GetText(true);
		}

		void HandleDataGridViewBlocksColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			if (m_dataGridViewBlocks.ContainsFocus)
				Properties.Settings.Default.AssignCharactersBlockContextGrid = GridSettings.Create(m_dataGridViewBlocks);
		}

		/// <summary>
		/// This method only gets subscribed to for right-to-left scripts. If we need this in the future for other painting enhancements,
		/// the logic that controls this will need to be moved into this method (so it can be subscribed to unconditionally).
		/// </summary>
		void HandleDataGridViewBlocksCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.ColumnIndex == colText.Index && e.RowIndex >= 0)
			{
				e.PaintBackground(e.CellBounds, true);
				TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString(),
				e.CellStyle.Font, e.CellBounds, e.CellStyle.ForeColor,
				 TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.RightToLeft | TextFormatFlags.Right);
				e.Handled = true;
			}
		}

		private void HandleDataGridViewBlocksSelectionChanged(object sender, EventArgs e)
		{
			if (m_updatingContext)
				return;

			if (m_viewModel.GetIsBlockScripture(m_dataGridViewBlocks.SelectedRows[0].Index))
				m_viewModel.CurrentBlockIndexInBook = m_dataGridViewBlocks.SelectedRows[0].Index;

			LoadBlock();
		}

		private void AdjustScrollingInResponseToRowSizeChange(object sender, DataGridViewRowEventArgs args)
		{
			if (m_dataGridViewBlocks.SelectedRows.Count > 0)
			{
				var firstRow = m_dataGridViewBlocks.SelectedRows[m_dataGridViewBlocks.SelectedRows.Count - 1].Index;
				var lastRow = m_dataGridViewBlocks.SelectedRows[0].Index;
				if (args.Row.Index > firstRow - 5 && args.Row.Index < lastRow + 2)
					BeginInvoke(new Action(() => ScrollDesiredRowsIntoView(firstRow, lastRow)));
			}
		}

		private void IncreaseFont(object sender, EventArgs e)
		{
			m_viewModel.FontSizeUiAdjustment++;
			colText.DefaultCellStyle.Font = m_viewModel.Font;
			var origFont = m_dataGridViewBlocks.DefaultCellStyle.Font;
			m_dataGridViewBlocks.DefaultCellStyle.Font = new Font(origFont.FontFamily, origFont.SizeInPoints + 1, origFont.Style);
			UpdateContextBlocksDisplay();
		}

		private void DecreaseFont(object sender, EventArgs e)
		{
			m_viewModel.FontSizeUiAdjustment--;
			colText.DefaultCellStyle.Font = m_viewModel.Font;
			var origFont = m_dataGridViewBlocks.DefaultCellStyle.Font;
			m_dataGridViewBlocks.DefaultCellStyle.Font = new Font(origFont.FontFamily, origFont.SizeInPoints - 1, origFont.Style);
			UpdateContextBlocksDisplay();
		}

		private void m_scriptureReference_VerseRefChanged(object sender, PropertyChangedEventArgs e)
		{
			if (m_viewModel.TryLoadBlock(m_scriptureReference.VerseControl.VerseRef))
				LoadBlock();
		}
		#endregion

		#region Browser events
		private void OnMouseOver(object sender, DomMouseEventArgs e)
		{
			if (e.Target == null)
				return;
			if (m_blocksDisplayBrowser.Visible)
			{
				var geckoElement = e.Target.CastToGeckoElement();
				var divElement = geckoElement as GeckoDivElement;
				if (divElement == null)
					return;

				if (divElement.Parent.ClassName == kCssClassContext)
				{
					m_toolTip = new ToolTip {IsBalloon = true};
					// 22 is the magic number which happens to make these display in the correct place
					int x = m_blocksDisplayBrowser.Location.X + m_blocksDisplayBrowser.Size.Width - 22;
					int y = m_blocksDisplayBrowser.Location.Y + e.ClientY - m_blocksDisplayBrowser.Margin.Top;
					m_toolTip.Show(divElement.Parent.GetAttribute(AssignCharacterViewModel.kDataCharacter), this, x, y);
				}
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

			if (divElement.Parent.ClassName == kCssClassContext)
			{
				m_toolTip.Hide(this);
			}
		}

		private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			m_blocksDisplayBrowser.ScrollElementIntoView(BlockNavigatorViewModel.kMainQuoteElementId, -225);
		}
		#endregion
	}
}
