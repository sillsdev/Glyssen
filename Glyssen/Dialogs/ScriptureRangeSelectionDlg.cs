using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DesktopAnalytics;
using Glyssen.Controls;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Paratext;
using GlyssenEngine.Script;
using L10NSharp;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using SIL.DblBundle.Text;
using SIL.Reporting;
using SIL.Scripture;
using static System.String;

namespace Glyssen.Dialogs
{
	public partial class ScriptureRangeSelectionDlg : Form
	{
		private readonly Project m_project;
		private ParatextScrTextWrapper m_paratextScrTextWrapper;
		private readonly List<Book> m_availableOtBooks;
		private readonly List<Book> m_availableNtBooks;
		private readonly int m_bookCodeColumnIndex = 0;
		private readonly int m_vernacularColumnIndex = 1;
		private readonly int m_includeInScriptColumnIndex = 2;
		private readonly int m_multiVoiceColumnIndex = 3;
		private Dictionary<string, bool> m_includeInScript;
		private Dictionary<string, bool> m_multiVoice;
		private List<string> m_storedOtSelections;
		private List<string> m_storedNtSelections;

		private DialogResult m_userDecisionAboutUpdatedBookContent = DialogResult.None;

		internal ScriptureRangeSelectionDlg(Project project, ParatextScrTextWrapper paratextScrTextWrapper)
		{
			InitializeComponent();

			m_project = project;
			m_paratextScrTextWrapper = paratextScrTextWrapper;

			m_availableOtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) <= 39).ToList();
			m_availableNtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) > 39).ToList();

			Initialize();

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;
		}

		private void HandleStringsLocalized()
		{
			Debug.Assert(LocalizationManager.UILanguageId != "en" || Text == "Select Books - {0}",
				"Dev alert: the localized string and ID of this dialog's window title MUST be kept in sync with the version in Project.FoundUnacceptableChangesInAvailableBooks!");

			Text = Format(Text, m_project.Name);
		}

		private void ScriptureRangeSelectionDlg_Load(object sender, EventArgs e)
		{
			MainForm.SetChildFormLocation(this);
		}

		private void Initialize()
		{
			m_includeInScript = new Dictionary<string, bool>(m_project.AvailableBooks.Count);
			m_multiVoice = new Dictionary<string, bool>(m_project.AvailableBooks.Count);

			// First time in. For bundle-based projects, default to all included.
			// For Paratext projects, we don't do this because we've predetermined which books
			// are included based on checking status.
			if (m_project.IsBundleBasedProject && m_project.IncludedBooks.Count == 0)
				foreach (var availableBook in m_project.AvailableBooks)
					availableBook.IncludeInScript = true;

			m_otBooksGrid.RowCount = m_availableOtBooks.Count;
			m_ntBooksGrid.RowCount = m_availableNtBooks.Count;

			if (m_otBooksGrid.RowCount == 0)
				HideOtGrid();
			if (m_ntBooksGrid.RowCount == 0)
				HideNtGrid();

			InitializeGridData(m_otBooksGrid, m_availableOtBooks);
			InitializeGridData(m_ntBooksGrid, m_availableNtBooks);

			if (m_checkBoxOldTestament.Visible && !m_includeInScript.Any(p => p.Value && BCVRef.BookToNumber(p.Key) <= 39))
				m_checkBoxOldTestament.Checked = false;
			if (m_checkBoxNewTestament.Visible && !m_includeInScript.Any(p => p.Value && BCVRef.BookToNumber(p.Key) > 39))
				m_checkBoxNewTestament.Checked = false;

			SetupDropdownHeaderCells();
		}

		/// <summary>
		/// Attempts to set m_paratextScrTextWrapper (if not set) and optionally updates the information about available books
		/// and their current checking status. This can fail if the Paratext project cannot be found, so callers should check
		/// the return value rather than blindly assuming it succeeded. The focus of this dialog is not on getting updated
		/// content from Paratext, so we don't want to bother the user if that isn't possible. If it is possible, then we ask
		/// them about including the updated content (when appropriate) when the OK button is clicked. This method will generally
		/// not initiate interaction with the user, but there is at least one edge case where it might (when the Paratext
		/// project is found but has a different metadata ID).
		/// </summary>
		/// <param name="refreshBooksIfExisting">Flag indicating that GetUpdatedBookInfo should be called to update the checking
		/// status of books in Paratext when m_paratextScrTextWrapper is already set</param>
		/// <returns><c>true</c> if m_paratextScrTextWrapper is set; <c>false</c> if m_paratextScrTextWrapper is null</returns>
		private bool GetParatextScrTextWrapperIfNeeded(bool refreshBooksIfExisting = false)
		{
			Debug.Assert(m_project.IsLiveParatextProject);
			if (m_paratextScrTextWrapper == null)
			{
				m_paratextScrTextWrapper = m_project.GetLiveParatextDataIfCompatible(
					new WinformsParatextProjectLoadingAssistant(null, false)
						{ AutoConfirmUpdateThatWouldExcludeExistingBooks = true });
				if (m_paratextScrTextWrapper == null)
					return false;
			}
			else if (refreshBooksIfExisting)
				m_paratextScrTextWrapper.GetUpdatedBookInfo();

			return true;
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (m_paratextScrTextWrapper != null && m_project.AvailableBooks.Any(b => !b.IncludeInScript))
			{
				// Since the wrapper is already set, we know this is a newly-created project.
				Debug.Assert(m_project.IsLiveParatextProject);
				var failedChecksBookCount = m_paratextScrTextWrapper.FailedChecksBooks.Count();
				if (failedChecksBookCount > 0)
				{
					var msg = Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ExcludedParatextBookExplanation",
						"{0} did not automatically include {1} of the books in project {2} because {3} is not reporting a " +
						"current successful status for all the basic checks that {0} usually requires to pass. " +
						"If you intend to prepare the recording script for any of the excluded books at this time, " +
						"we strongly recommend that you get the following checks to pass in {2} before including them:",
						"Param 0: \"Glyssen\" (product name); " +
						"Param 1: (integer) number of Scripture books that did not pass basic checks; " +
						"Param 2: Project short name (unique project identifier); " +
						"Param 3: \"Paratext\" (product name)"),
						GlyssenInfo.Product,
						failedChecksBookCount,
						m_project.ParatextProjectName,
						ParatextScrTextWrapper.kParatextProgramName) +
						Environment.NewLine +
						m_paratextScrTextWrapper.RequiredCheckNames;
					MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private void SetupDropdownHeaderCells()
		{
			// set up dropdown column headers
			var columnOptions = new SortedList<int, string>
			{
				{0, LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.SelectAll", "Select All", "\"All\" refers to Scripture books")},
				{1, LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ClearAll", "Clear All", "\"All\" refers to Scripture books")}
			};

			m_ntBooksGrid.Columns[2].HeaderCell = CreateDropdownHeaderCell(m_ntBooksGrid.Columns[2].HeaderCell, columnOptions);
			m_ntBooksGrid.Columns[3].HeaderCell = CreateDropdownHeaderCell(m_ntBooksGrid.Columns[3].HeaderCell, columnOptions);
			m_otBooksGrid.Columns[2].HeaderCell = CreateDropdownHeaderCell(m_otBooksGrid.Columns[2].HeaderCell, columnOptions);
			m_otBooksGrid.Columns[3].HeaderCell = CreateDropdownHeaderCell(m_otBooksGrid.Columns[3].HeaderCell, columnOptions);
		}

		private DataGridViewDropdownColumnHeaderCell CreateDropdownHeaderCell(
			DataGridViewColumnHeaderCell oldHeaderCell, SortedList<int, string> columnOptions)
		{
			var cell = new DataGridViewDropdownColumnHeaderCell(oldHeaderCell, columnOptions);
			cell.MenuItemClicked += cell_MenuItemClicked;
			return cell;
		}

		private void cell_MenuItemClicked(object sender, CustomMenuItemClickedEventArgs e)
		{
			var grid = e.Column.DataGridView;
			// e.EventKey: 0 = Select All, 1 = Clear All
			var shouldCheck = e.EventKey == 0;
			var rowsToChange = grid.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow && row.Visible &&
				(bool)row.Cells[e.Column.Index].Value != shouldCheck).ToList();

			if (shouldCheck && m_project.IsLiveParatextProject)
			{
				if (!GetParatextScrTextWrapperIfNeeded(true))
					return;

				var booksToChange = new HashSet<string>(rowsToChange.Select(r => (string)r.Cells[m_colNTBookCode.Index].Value));
				var booksWithFailingChecks = m_paratextScrTextWrapper.FailedChecksBooks.Where(b => booksToChange.Contains(b)).ToList();
				if (booksWithFailingChecks.Any())
				{
					var msg = Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ExcludedParatextBookExplanation",
						"{0} is not reporting a current successful status for the following books for all the basic checks that {1} usually requires to pass:" +
						 "\r\n{2}\r\n\r\n" +
						"Although you can ignore this warning and proceed to include these books, {1} might fail to process the data " +
						"properly, which could give the appearance of data loss or corruption and could even cause {1} to stop responding. " +
						"Therefore, you should get these checks to pass in {0} project {3} before including them:\r\n" +
						"   {4}\r\n\r\n" +
						"Do you want {1} to select all the {5} books to include them in the {6} project script anyway?",
						"Param 0: \"Paratext\" (product name); " +
						"Param 1: \"Glyssen\" (product name); " +
						"Param 2: list of 3-letter codes of Scripture books that did not pass basic checks; " +
						"Param 3: Paratext project short name (unique project identifier); " +
						"Param 4: List of names of Paratext checks; " +
						"Param 5: \"Old Testament\" or \"New Testament\" (as localized); " +
						"Param 6: Glyssen recording project name"),
						ParatextScrTextWrapper.kParatextProgramName,
						GlyssenInfo.Product,
						Join(LocalizationManager.GetString("Common.SimpleListSeparator", ", "), booksWithFailingChecks),
						m_project.ParatextProjectName,
						m_paratextScrTextWrapper.RequiredCheckNames,
						grid == m_otBooksGrid ? BookSetUtils.OldTestamentLocalizedString : BookSetUtils.NewTestamentLocalizedString,
						m_project.Name);
					if (DialogResult.No == MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.YesNo,
						MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
					{
						return;
					}
				}
			}
			foreach (var row in rowsToChange)
			{
				row.Cells[e.Column.Index].Value = shouldCheck;
				if (e.Column.Index == m_includeInScriptColumnIndex)
					SetCorrespondingMultivoiceCell(row.Index, e.Column.Index, grid);
			}
		}

		private void InitializeGridData(DataGridView grid, List<Book> books)
		{
			foreach (var availableBook in books)
			{
				m_includeInScript[availableBook.Code] = availableBook.IncludeInScript;
				if (!availableBook.IncludeInScript)
					grid[m_multiVoiceColumnIndex, books.IndexOf(availableBook)].ReadOnly = true;

				var bookScript = m_project.Books.SingleOrDefault(b => b.BookId == availableBook.Code);
				m_multiVoice[availableBook.Code] = !bookScript?.SingleVoice ?? true;
			}
		}

		private void HideOtGrid()
		{
			m_checkBoxOldTestament.Visible = false;
			m_otBooksGrid.Visible = false;
			m_checkBoxNewTestament.Visible = false;
			tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Absolute;
			tableLayoutPanel1.ColumnStyles[0].Width = 0;
			Width = (int)(Width * .6d);
		}

		private void HideNtGrid()
		{
			m_checkBoxNewTestament.Visible = false;
			m_ntBooksGrid.Visible = false;
			m_checkBoxOldTestament.Visible = false;
			tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Absolute;
			tableLayoutPanel1.ColumnStyles[1].Width = 0;
			Width = (int)(Width * .6d);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			var bookNumsToAddFromParatext = new HashSet<int>(); 
			foreach (var book in m_project.AvailableBooks)
			{
				bool includingThisBook = m_includeInScript[book.Code];

				if (!includingThisBook)
				{
					book.IncludeInScript = false;
					continue;
				}

				if (!m_project.IsLiveParatextProject)
					book.IncludeInScript = true;

				var existingBookScript = m_project.IncludedBooks.SingleOrDefault(b => b.BookId == book.Code);

				if (existingBookScript != null)
				{
					bool prevSingleVoiceValue = existingBookScript.SingleVoice;
					existingBookScript.SingleVoice = !m_multiVoice[existingBookScript.BookId];
					if (prevSingleVoiceValue != existingBookScript.SingleVoice)
					{
						Analytics.Track("SetSingleVoice", new Dictionary<string, string>
						{
							{"book", existingBookScript.BookId},
							{"singleVoice", existingBookScript.SingleVoice.ToString()},
							{"method", "ScriptureRangeSelectionDlg.m_btnOk_Click"}
						});
					}
					continue;
				}

				Debug.Assert(m_project.IsLiveParatextProject);
				book.IncludeInScript = true;

				BookScript bookScriptFromExistingFile = m_project.LoadExistingBookIfPossible(book.Code);
				if (bookScriptFromExistingFile == null || UserWantsUpdatedContent(bookScriptFromExistingFile))
					bookNumsToAddFromParatext.Add(Canon.BookIdToNumber(book.Code));
				else
				{
					bookScriptFromExistingFile.SingleVoice = !m_multiVoice[book.Code];
					m_project.IncludeExistingBook(bookScriptFromExistingFile);
					Analytics.Track("IncludeExistingBook", new Dictionary<string, string>
					{
						{ "book", bookScriptFromExistingFile.BookId },
						{ "singleVoice", bookScriptFromExistingFile.SingleVoice.ToString() }
					});
				}
			}

			if (bookNumsToAddFromParatext.Any())
			{
				if (!GetParatextScrTextWrapperIfNeeded())
					return;
				m_project.IncludeBooksFromParatext(m_paratextScrTextWrapper, bookNumsToAddFromParatext,
					bookScript =>
					{
						bookScript.SingleVoice = !m_multiVoice[bookScript.BookId];
						Analytics.Track("SetSingleVoice", new Dictionary<string, string>
						{
							{ "book", bookScript.BookId },
							{ "singleVoice", bookScript.SingleVoice.ToString() },
							{ "method", "postParseAction anonymous delegate from ScriptureRangeSelectionDlg.m_btnOk_Click" }
						});
					});
			}

			m_project.BookSelectionStatus = BookSelectionStatus.Reviewed;

			Analytics.Track("SelectBooks", new Dictionary<string, string> { { "bookSummary", m_project.BookSelectionSummary } });
		}

		private bool UserWantsUpdatedContent(BookScript bookScriptFromExistingFile)
		{
			// If there is a newer version ask user if they want to get the updated version.
			if (!GetParatextScrTextWrapperIfNeeded(true) || m_paratextScrTextWrapper.GetBookChecksum(bookScriptFromExistingFile.BookNumber) == bookScriptFromExistingFile.ParatextChecksum)
				return false;
			// If the updated version does NOT pass tests but the existing version does (i.e., user didn't override the checking status),
			// we'll just stick with the version we have. If they want to update it manually later, they can.
			if (m_paratextScrTextWrapper.DoesBookPassChecks(bookScriptFromExistingFile.BookNumber) && !bookScriptFromExistingFile.CheckStatusOverridden)
				return false;

			var result = m_userDecisionAboutUpdatedBookContent;

			if (result == DialogResult.None)
			{
				var msg = Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.GetUpdatedContent.Msg",
					"{0} has existing data for {1}. However, {2} reports that there is updated content in project {3}. " +
					"Do you want {0} to use the latest content?",
					"Param 0: \"Glyssen\" (product name); " +
					"Param 1: 3-letter Scripture book code; " +
					"Param 2: \"Paratext\" (product name); " +
					"Param 3: Paratext project short name (unique project identifier)"),
					GlyssenInfo.Product,
					bookScriptFromExistingFile.BookId,
					ParatextScrTextWrapper.kParatextProgramName,
					m_project.ParatextProjectName);
				var applyToAllText = LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.GetUpdatedContent.ApplyToAll",
					"Apply to all newly included books");
				using (var dlg = new YesNoApplyToAllDlg(msg, applyToAllText))
				{
					result = dlg.ShowDialog(this);
					if (dlg.ApplyToAll)
						m_userDecisionAboutUpdatedBookContent = result;
				}
			}
			return result == DialogResult.Yes;
		}

		private void BooksGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			var books = sender.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
			var book = books[e.RowIndex];
			if (e.ColumnIndex == m_bookCodeColumnIndex)
				e.Value = book.Code;
			else if (e.ColumnIndex == m_vernacularColumnIndex)
				e.Value = book.ShortName;
			else if (e.ColumnIndex == m_includeInScriptColumnIndex)
				e.Value = m_includeInScript[book.Code];
			else
				e.Value = m_multiVoice[book.Code];
		}

		private void BooksGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (!e.Cancel && e.ColumnIndex == m_includeInScriptColumnIndex && (bool)e.FormattedValue)
			{
				var grid = (DataGridView)sender;

				if (grid.IsCurrentCellDirty && !IsValidToIncludeBook(grid, e.RowIndex))
				{
					e.Cancel = true;
					grid.RefreshEdit();
					grid.CancelEdit();
				}
			}
		}

		/// <summary>
		/// This method should be used any time the user takes an action that would result in marking a book
		/// for inclusion in the project. (It does not need to be called when a book is being removed.)
		/// Currently, this can be either when they click a check-box for an individual book or when they use one of
		/// the "select all" menu items. There are several quick-return paths, but in the "interesting" case,
		/// the purpose of this method is to check whether the book in question is currently in a state (as reported
		/// by Paratext) where the required basic checks all pass. If not, the user needs to confirm that they indeed
		/// wish to include the book and thus override this requirement.
		/// </summary>
		/// <param name="grid">Either the OT grid or the NT grid</param>
		/// <param name="rowIndex">The row index in the grid also corresponds to an entry in the available OT or
		/// NT books list.</param>
		/// <returns></returns>
		private bool IsValidToIncludeBook(DataGridView grid, int rowIndex)
		{
			if (!m_project.IsLiveParatextProject)
				return true;
			var books = grid.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
			var book = books[rowIndex];
			if (book.IncludeInScript)
				return true; // Always valid to exclude
			var bookCode = book.Code;
			if (m_project.DoesBookScriptFileExist(bookCode))
				return true; // Might try to get an updated version later but this one is valid.

			if (!GetParatextScrTextWrapperIfNeeded())
				return false;
			var bookNum = Canon.BookIdToNumber(bookCode);
			if (!m_paratextScrTextWrapper.CanonicalBookNumbersInProject.Contains(bookNum))
			{
				ReportParatextBookNoLongerAvailable(bookCode);
				grid.CurrentRow.DefaultCellStyle.ForeColor = GlyssenColorPalette.ColorScheme.Warning;
				return false;
			}
			else if (grid.CurrentRow.DefaultCellStyle.ForeColor == GlyssenColorPalette.ColorScheme.Warning)
				grid.CurrentRow.DefaultCellStyle.ForeColor = grid.DefaultCellStyle.ForeColor;

			if (m_paratextScrTextWrapper.DoesBookPassChecks(bookNum, true))
				return true;

			var failureMessage = Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.FailedChecksForBook",
				"{0} is not reporting a current successful status for {1} in project {2} for the following basic checks " +
				"that {3} usually requires to pass:\r\n{4}",
				"Param 0: \")Paratext\" (product name); " +
				"Param 1: 3-letter Scripture book code; " +
				"Param 2: Paratext project short name (unique project identifier); " +
				"Param 3: \"Glyssen\" (product name); " +
				"Param 4: List of failing Paratext check names"),
				ParatextScrTextWrapper.kParatextProgramName,
				bookCode,
				m_project.ParatextProjectName,
				GlyssenInfo.Product,
				Join(", ", m_paratextScrTextWrapper.GetCheckFailuresForBook(bookCode)));

			var msg = failureMessage + Environment.NewLine + Environment.NewLine +
				Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ConfirmInclusionOfParatextBookThatDoesNotPassChecks",
				"Depending on the specific errors, {0} might fail to process the data for this book properly, which could " +
				"give the appearance of data loss or corruption and could even cause {0} to stop responding. " +
				"Do you want to include this book in the {1} project anyway?",
				"Param 0: \"Glyssen\" (product name); " +
				"Param 1: Glyssen recording project name"),
				GlyssenInfo.Product,
				m_project.Name);

			if (DialogResult.No == MessageBox.Show(this, msg, GlyssenInfo.Product, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
				return false;
			Logger.WriteEvent($"Including book {bookCode} even though " + failureMessage);
			return true;
		}

		private void ReportParatextBookNoLongerAvailable(string bookCode)
		{
			var msg = Format(LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ParatextBookNoLongerAvailableMsg",
				"Sorry. {0} is no longer available from {1} project {2}.",
				"Param 0: 3-letter ID of Scripture book; " +
				"Param 1: \"Paratext\" (product name); " +
				"Param 2: Paratext project short name (unique project identifier)"),
				bookCode,
				ParatextScrTextWrapper.kParatextProgramName,
				m_project.ParatextProjectName);
			var caption = LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ParatextBookNoLongerAvailableCaption",
				"Unable to Include Book");
			MessageBox.Show(this, msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		private void BooksGrid_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			var books = sender.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
			var book = books[e.RowIndex];
			if (e.ColumnIndex == m_includeInScriptColumnIndex)
			{
				m_includeInScript[book.Code] = (bool)e.Value;
			}
			else if (e.ColumnIndex == m_multiVoiceColumnIndex)
				m_multiVoice[book.Code] = (bool)e.Value;
		}

		private void BooksGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1 || e.ColumnIndex != m_includeInScriptColumnIndex)
				return;

			var grid = (DataGridView)sender;
			if (!(bool)grid[e.ColumnIndex, e.RowIndex].EditedFormattedValue || IsValidToIncludeBook(grid, e.RowIndex))
			{
				grid.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);
				SetCorrespondingMultivoiceCell(e.RowIndex, e.ColumnIndex, grid);
			}
			else
			{
				Debug.Assert(grid.CurrentCell != null && grid.CurrentCell.ColumnIndex == e.ColumnIndex &&
					grid.CurrentCell.RowIndex == e.RowIndex);

				if (grid.IsCurrentCellInEditMode)
				{
					grid.RefreshEdit();
					grid.CancelEdit();
					Debug.Assert((bool)grid.CurrentCell.Value == false);
				}
				else
				{
					grid.CurrentCell.Value = false;
				}
			}
		}

		private void SetCorrespondingMultivoiceCell(int rowIndex, int columnIndex, DataGridView grid)
		{
			var correspondingMultiVoiceCell = grid[m_multiVoiceColumnIndex, rowIndex];
			if ((bool)grid[columnIndex, rowIndex].EditedFormattedValue)
			{
				correspondingMultiVoiceCell.ReadOnly = false;
				m_btnOk.Enabled = true;
			}
			else
			{
				correspondingMultiVoiceCell.ReadOnly = true;
				m_btnOk.Enabled = m_otBooksGrid.Rows.Cast<DataGridViewRow>().Any(row => row.Index != rowIndex && (bool)((DataGridViewCheckBoxCell)row.Cells[m_includeInScriptColumnIndex]).Value) ||
				                  m_ntBooksGrid.Rows.Cast<DataGridViewRow>().Any(row => row.Index != rowIndex && (bool)((DataGridViewCheckBoxCell)row.Cells[m_includeInScriptColumnIndex]).Value);
			}
			grid.EndEdit();
			grid.InvalidateCell(correspondingMultiVoiceCell);
		}

		private void BooksGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			var grid = (DataGridView)sender;
			if (e.RowIndex < 0 || e.ColumnIndex != m_multiVoiceColumnIndex || (!grid[e.ColumnIndex, e.RowIndex].ReadOnly && grid.Enabled))
				return;

			var books = grid.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
			var bookId = books[e.RowIndex].Code;
			if (grid.Enabled && m_includeInScript[bookId])
				return;

			e.PaintBackground(e.CellBounds, false);

			Point drawInPoint = new Point(e.CellBounds.X + e.CellBounds.Width / 2 - 7, e.CellBounds.Y + e.CellBounds.Height / 2 - 7);
			CheckBoxRenderer.DrawCheckBox(e.Graphics, drawInPoint, m_multiVoice[bookId] ? CheckBoxState.CheckedDisabled : CheckBoxState.UncheckedDisabled);

			e.Handled = true;
		}

		private void BooksGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			var grid = (DataGridView)sender;
			if (grid.Enabled)
				return;
			e.CellStyle.BackColor = Color.LightGray;
		}

		private void BooksGrid_SelectionChanged(object sender, EventArgs e)
		{
			((DataGridView)sender).ClearSelection();
		}

		private void CheckBoxOldTestament_CheckedChanged(object sender, EventArgs e)
		{
			if (m_checkBoxOldTestament.Checked)
			{
				foreach (DataGridViewRow row in m_otBooksGrid.Rows)
				{
					if (m_storedOtSelections.Contains(m_availableOtBooks[row.Index].Code))
						m_otBooksGrid[m_includeInScriptColumnIndex, row.Index].Value = true;
				}
			}
			else
			{
				m_storedOtSelections = m_includeInScript.Where(p => BCVRef.BookToNumber(p.Key) <= 39 && p.Value).Select(p => p.Key).ToList();
				foreach (DataGridViewRow row in m_otBooksGrid.Rows)
					m_otBooksGrid[m_includeInScriptColumnIndex, row.Index].Value = false;
			}

			m_otBooksGrid.Enabled = m_checkBoxOldTestament.Checked;

			UpdateState();
		}

		private void CheckBoxNewTestament_CheckedChanged(object sender, EventArgs e)
		{
			if (m_checkBoxNewTestament.Checked)
			{
				foreach (DataGridViewRow row in m_ntBooksGrid.Rows)
				{
					if (m_storedNtSelections.Contains(m_availableNtBooks[row.Index].Code))
						m_ntBooksGrid[m_includeInScriptColumnIndex, row.Index].Value = true;
				}
			}
			else
			{
				m_storedNtSelections = m_includeInScript.Where(p => BCVRef.BookToNumber(p.Key) > 39 && p.Value).Select(p => p.Key).ToList();
				foreach (DataGridViewRow row in m_ntBooksGrid.Rows)
					m_ntBooksGrid[m_includeInScriptColumnIndex, row.Index].Value = false;
			}

			m_ntBooksGrid.Enabled = m_checkBoxNewTestament.Checked;

			UpdateState();
		}

		private void UpdateState()
		{
			m_btnOk.Enabled = m_includeInScript.Any(p => p.Value);
		}
	}
}
