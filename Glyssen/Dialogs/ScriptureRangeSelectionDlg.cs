using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DesktopAnalytics;
using Glyssen.Bundle;
using Glyssen.Controls;
using Glyssen.Paratext;
using Glyssen.Shared;
using L10NSharp;
using L10NSharp.UI;
using SIL.DblBundle.Text;
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

		internal ScriptureRangeSelectionDlg(Project project, ParatextScrTextWrapper paratextScrTextWrapper)
		{
			InitializeComponent();

			m_project = project;
			m_paratextScrTextWrapper = paratextScrTextWrapper;

			m_availableOtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) <= 39).ToList();
			m_availableNtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) > 39).ToList();

			Initialize();

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;
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

			// First time in. Default to all included.
			if (m_project.IncludedBooks.Count == 0)
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

			if (!m_includeInScript.Any(p => p.Value && BCVRef.BookToNumber(p.Key) <= 39))
				m_checkBoxOldTestament.Checked = false;
			if (!m_includeInScript.Any(p => p.Value && BCVRef.BookToNumber(p.Key) > 39))
				m_checkBoxNewTestament.Checked = false;

			SetupDropdownHeaderCells();
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
						"{0} did not automatically {1} of the books in project {2} because {3} is not reporting a " +
						"current successful status for the basic checks that Glyssen normally requires to pass." +
						"If you intend to prepare the recording script for any of the excluded books at this time, " +
						"we strongly recommend that you get the following checks to pass in {2} before including them:",
						"Param 0: \"Glyssen\" (product name); " +
						"Param 1: (integer) number of Scripture books that did not pass basic checks; " +
						"Param 2: Project short name (unique project identifier); " +
						"Param 3: \"Paratext\" (product name)"),
						GlyssenInfo.kProduct,
						failedChecksBookCount,
						m_project.ParatextProjectName,
						ParatextScrTextWrapper.kParatextProgramName) +
						Environment.NewLine +
						ParatextScrTextWrapper.RequiredCheckNames;
					MessageBox.Show(this, msg, GlyssenInfo.kProduct, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private void SetupDropdownHeaderCells()
		{
			// set up dropdown column headers
			var columnOptions = new SortedList<int, string>
			{
				{0, LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.SelectAll", "Select All")},
				{1, LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.SelectAll", "Clear All")}
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
			// e.EventKey: 0 = Select All, 1 = Clear All
			var shouldCheck = e.EventKey == 0;
			foreach (var row in e.Column.DataGridView.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow))
			{
				row.Cells[e.Column.Index].Value = shouldCheck;
				CheckCorrespondingCell(row.Index, e.Column.Index, e.Column.DataGridView);
			}
		}

		private void InitializeGridData(DataGridView grid, List<Book> books)
		{
			foreach (var availableBook in books)
			{
				m_includeInScript[availableBook.Code] = availableBook.IncludeInScript;
				if (!availableBook.IncludeInScript)
					grid[m_multiVoiceColumnIndex, books.IndexOf(availableBook)].ReadOnly = true;

				var bookScript = m_project.Books.Single(b => b.BookId == availableBook.Code);
				m_multiVoice[availableBook.Code] = !bookScript.SingleVoice;
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

		private void BtnOk_Click(object sender, EventArgs e)
		{
			List<BookScript> booksToAskUserAbout = new List<BookScript>();
			foreach (var book in m_project.AvailableBooks)
			{
				book.IncludeInScript = m_includeInScript[book.Code];

				if (!book.IncludeInScript)
					continue;

				var bookScript = m_project.IncludedBooks.Single(b => b.BookId == book.Code);
				if (bookScript.SingleVoice != m_multiVoice[book.Code])
					continue;

				SingleVoiceReason singleVoiceReason;
				if (m_multiVoice[book.Code] && BookMetadata.DefaultToSingleVoice(book.Code, out singleVoiceReason) && singleVoiceReason == SingleVoiceReason.TooComplexToAssignAccurately)
				{
					booksToAskUserAbout.Add(bookScript);
					continue;
				}

				bookScript.SingleVoice = !m_multiVoice[book.Code];
				Analytics.Track("SetSingleVoice", new Dictionary<string, string>
				{
					{ "book", book.Code },
					{ "singleVoice", bookScript.SingleVoice.ToString() },
					{ "method", "ScriptureRangeSelectionDlg.m_btnOk_Click" }
				});
			}

			if (booksToAskUserAbout.Any() && !ConfirmSetToMultiVoice(booksToAskUserAbout))
			{
				DialogResult = DialogResult.None;
				return;
			}

			m_project.BookSelectionStatus = BookSelectionStatus.Reviewed;

			Analytics.Track("SelectBooks", new Dictionary<string, string> { { "bookSummary", m_project.BookSelectionSummary } });
		}

		private bool ConfirmSetToMultiVoice(List<BookScript> booksToAskUserAbout)
		{
			if (booksToAskUserAbout.Any())
			{
				StringBuilder sb = new StringBuilder();
				foreach (var book in booksToAskUserAbout)
					sb.Append(book.BookId + Environment.NewLine);

				string msg = LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ConfirmDifficultMultiVoice.MessagePart1", "You have selected to record the following books with multiple voice actors. Glyssen can help you do this, but you may not get the expected results without some manual intervention due to the complexity of these books.") +
					Environment.NewLine + Environment.NewLine +
					sb + Environment.NewLine +
					LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ConfirmDifficultMultiVoice.MessagePart2", "Are you sure? If you select no, the books above will each be recorded by a single voice actor.");
				string caption = LocalizationManager.GetString("DialogBoxes.ScriptureRangeSelectionDlg.ConfirmDifficultMultiVoice.Caption", "Are you sure?");
				DialogResult result = MessageBox.Show(msg, caption, MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Cancel)
					return false;
				if (result == DialogResult.Yes)
				{
					foreach (var book in booksToAskUserAbout)
					{
						book.SingleVoice = false;
						Analytics.Track("SetSingleVoice", new Dictionary<string, string>
						{
							{ "book", book.BookId },
							{ "singleVoice", "false" },
							{ "method", "ScriptureRangeSelectionDlg.ConfirmSetToMultiVoice" }
						});
					}
				}
			}

			return true;
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
			if (e.ColumnIndex == m_includeInScriptColumnIndex && m_project.IsLiveParatextProject)
			{
				var books = sender.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
				var book = books[e.RowIndex];
				if (m_project.BookDataFileExists(book.Code))
				{
					// TODO: We need to check the check status, warn the user if not passing, and get the data from Paratext.
					// Even if the data is there, we might still want to ask them if they want to get updated data from Paratext.
				}
				else
				{
					if (m_paratextScrTextWrapper == null)
					{
						m_paratextScrTextWrapper = m_project.GetLiveParatextDataIfCompatible(true, "", false);
					}
					else
					{
						// TODO: Refresh checking status
					}
					// TODO: See if checks pass for this book. If not, as user whether to override
				}
			}
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
			if (e.RowIndex == -1)
				return;

			CheckCorrespondingCell(e.RowIndex, e.ColumnIndex, (DataGridView)sender);
		}

		private void CheckCorrespondingCell(int rowIndex, int columnIndex, DataGridView grid)
		{
			if (columnIndex != m_includeInScriptColumnIndex) return;

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
