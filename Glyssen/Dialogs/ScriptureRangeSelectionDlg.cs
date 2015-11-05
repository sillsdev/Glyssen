﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DesktopAnalytics;
using Glyssen.Bundle;
using SIL.DblBundle.Text;
using SIL.Scripture;

namespace Glyssen.Dialogs
{
	public partial class ScriptureRangeSelectionDlg : Form
	{
		private readonly Project m_project;
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

		public ScriptureRangeSelectionDlg()
		{
			InitializeComponent();
		}

		public ScriptureRangeSelectionDlg(Project project) : this()
		{
			m_project = project;

			m_availableOtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) <= 39).ToList();
			m_availableNtBooks = m_project.AvailableBooks.Where(b => BCVRef.BookToNumber(b.Code) > 39).ToList();

			Initialize();
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
			foreach (var book in m_project.AvailableBooks)
			{
				book.IncludeInScript = m_includeInScript[book.Code];

				if (!book.IncludeInScript)
					continue;

				var bookScript = m_project.IncludedBooks.Single(b => b.BookId == book.Code);
				if (bookScript.SingleVoice != m_multiVoice[book.Code])
					continue;

				bookScript.SingleVoice = !m_multiVoice[book.Code];
				Analytics.Track("SetSingleVoice", new Dictionary<string, string>
				{
					{ "book", book.Code },
					{ "singleVoice", bookScript.SingleVoice.ToString() },
					{ "method", "ScriptureRangeSelectionDlg.m_btnOk_Click" }
				});

				if (!bookScript.SingleVoice)
					continue;

				using (var model = new AssignCharacterViewModel(m_project))
					model.AssignNarratorForRemainingBlocksInBook(bookScript);
			}
			m_project.BookSelectionStatus = BookSelectionStatus.Reviewed;

			Analytics.Track("SelectBooks", new Dictionary<string, string> { { "bookSummary", m_project.BookSelectionSummary } });
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

		private void BooksGrid_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
		{
			var books = sender.Equals(m_otBooksGrid) ? m_availableOtBooks : m_availableNtBooks;
			var book = books[e.RowIndex];
			if (e.ColumnIndex == m_includeInScriptColumnIndex)
				m_includeInScript[book.Code] = (bool)e.Value;
			else if (e.ColumnIndex == m_multiVoiceColumnIndex)
				m_multiVoice[book.Code] = (bool)e.Value;
		}

		private void BooksGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1)
				return;

			if (e.ColumnIndex == m_includeInScriptColumnIndex)
			{
				var grid = (DataGridView)sender;
				var correspondingMultiVoiceCell = grid[m_multiVoiceColumnIndex, e.RowIndex];
				if ((bool)grid[e.ColumnIndex, e.RowIndex].EditedFormattedValue)
				{
					correspondingMultiVoiceCell.ReadOnly = false;
					m_btnOk.Enabled = true;
				}
				else
				{
					correspondingMultiVoiceCell.ReadOnly = true;
					m_btnOk.Enabled = m_otBooksGrid.Rows.Cast<DataGridViewRow>().Any(row => row.Index != e.RowIndex && (bool)((DataGridViewCheckBoxCell)row.Cells[m_includeInScriptColumnIndex]).Value) ||
						m_ntBooksGrid.Rows.Cast<DataGridViewRow>().Any(row => row.Index != e.RowIndex && (bool)((DataGridViewCheckBoxCell)row.Cells[m_includeInScriptColumnIndex]).Value);
				}
				grid.EndEdit();
				grid.InvalidateCell(correspondingMultiVoiceCell);
			}
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
