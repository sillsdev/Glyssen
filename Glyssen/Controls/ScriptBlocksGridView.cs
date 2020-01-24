using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Utilities;
using GlyssenEngine.Utilities;
using GlyssenEngine.ViewModels;
using Pango;
using SIL.Reporting;
using SIL.Scripture;
using Analytics = DesktopAnalytics.Analytics;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace Glyssen.Controls
{
	public class ScriptBlocksGridView : DataGridView
	{
		private bool m_updatingContext = true;
		private DataGridViewTextBoxColumn m_colReference;
		private DataGridViewTextBoxColumn m_colText;
		private BlockNavigatorViewModel<Font> m_viewModel;
		private FontProxy m_originalDefaultFont;

		private bool m_userIsResizingColumns;
		private bool m_userResizedRefColumn;
		private int m_minimumWidthFromDesigner;
		private string m_bookIdUsedToSizeRefColumn;
		private bool m_betweenMouseDownAndMouseUp;

		public event EventHandler MinimumWidthChanged;

		#region overrides
		protected override void OnRowHeightChanged(DataGridViewRowEventArgs e)
		{
			base.OnRowHeightChanged(e);
			this.BeginInvoke((MethodInvoker)(() =>
			{
				if (!m_updatingContext && SelectedRows.Count > 0)
				{
					var firstRow = SelectedRows[SelectedRows.Count - 1].Index;
					// When first initializing or moving to a different row (in a different book?), we
					// can briefly get into a state where the selected row is out of range. I'd like to
					// understand this better to prevent it higher up by clearing the SelectedRows
					// collection, but this first check seems like a safe/robust way to prevent a crash.
					if (firstRow >= RowCount || e.Row.Index <= firstRow - 5)
						return;
					var lastRow = SelectedRows[0].Index;
					if (e.Row.Index < lastRow + 2)
						ScrollDesiredRowsIntoView(firstRow, lastRow);
				}
			}));
		}

		protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex >= 0 && Rows[e.RowIndex].Selected)
			{
				ResetSelectionBackColors();
				m_viewModel.CurrentBlockIndexInBook = e.RowIndex;
				return;
			}
			base.OnCellMouseDown(e);
			m_betweenMouseDownAndMouseUp = true;
		}

		protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
		{
			base.OnCellMouseUp(e);
			m_betweenMouseDownAndMouseUp = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			// NOTE: The use of m_betweenMouseDownAndMouseUp is needed because it is commonly the case that clicking (MouseDown) a
			// row to select it will cause that row (and maybe other adjected rows) to be selected and the scrolling will adjust to
			// position them at the top of the view (for better "rainbow" alignment). Since the scrolling happens while the mouse is
			// still down, the normal behavior of the data grid view, which is in multiple-row select mode, is to think it needs to
			// adjust anew the selected row(s). But this causes subsequent row(s) to get selected instead, and that is bad.
			if (!m_betweenMouseDownAndMouseUp)
				base.OnMouseMove(e);
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			if (m_updatingContext)
				return;

			// NOTE: The use of m_betweenMouseDownAndMouseUp is needed because it is commonly the case that clicking (MouseDown) a
			// row to select it will cause that row (and maybe other adjacent rows) to be selected and the scrolling will adjust to
			// position them at the top of the view (for better "rainbow" alignment). Since the scrolling happens while the mouse is
			// still down, the normal behavior of the data grid view, which is in multiple-row select mode, is to think it needs to
			// adjust anew the selected row(s). But this causes subsequent row(s) to get selected instead, and that is bad.
			if (SelectedRows.Count > 0 && m_viewModel.GetIsBlockScripture(SelectedRows[0].Index) && !m_betweenMouseDownAndMouseUp)
				m_viewModel.CurrentBlockIndexInBook = SelectedRows[0].Index;

			base.OnSelectionChanged(e);
		}

		protected override void OnCellValueNeeded(DataGridViewCellValueEventArgs e)
		{
			if (e.RowIndex < 0 || e.RowIndex >= m_viewModel.BlockCountForCurrentBook)
			{
				// This should never happen, but because of the side-effects of various DGV properites and methods,
				// it seems to be incredibly difficult to ensure that things are done in an order that won't on
				// occassion cause it to request the value for a cell which no longer exists.
				e.Value = string.Empty;
			}
			else
			{
				var block = m_viewModel.GetNthBlockInCurrentBook(e.RowIndex);
				if (e.ColumnIndex == m_colReference.Index)
					e.Value = m_viewModel.GetBlockReferenceString(block);
				else if (e.ColumnIndex == m_colText.Index)
					e.Value = block.GetText(true);
				else
				{
					if (m_viewModel.CurrentReferenceTextMatchup != null)
					{
						var correspondingOrigBlock = m_viewModel.CurrentReferenceTextMatchup.GetCorrespondingOriginalBlock(block);
						if (correspondingOrigBlock != null)
						{
							if (Columns[e.ColumnIndex].Name == "colCharacter")
								e.Value = correspondingOrigBlock.CharacterIsUnclear ? "" : correspondingOrigBlock.CharacterId;
							else
								e.Value = correspondingOrigBlock.Delivery;
							return;
						}
					}
					base.OnCellValueNeeded(e);
				}
			}
		}

		protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
		{
			if (!e.Handled && m_viewModel != null && m_viewModel.RightToLeftScript && e.ColumnIndex == m_colText.Index && e.RowIndex >= 0)
			{
				e.PaintBackground(e.CellBounds, true);
				TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString(),
					e.CellStyle.Font, e.CellBounds, e.CellStyle.ForeColor,
					TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.RightToLeft | TextFormatFlags.Right);
				e.Handled = true;
			}
			base.OnCellPainting(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				e.SuppressKeyPress = true;
			base.OnKeyDown(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && m_viewModel != null)
			{
				m_viewModel.CurrentBlockChanged -= CurrentBlockChanged;
				m_viewModel.UiFontSizeChanged -= HandleUiFontSizeChanged;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region public methods
		public void Initialize(BlockNavigatorViewModel<Font> viewModel)
		{
			m_colReference = (DataGridViewTextBoxColumn)Columns[0];
			m_colText = (DataGridViewTextBoxColumn)Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
			Debug.Assert(m_colReference != null);
			Debug.Assert(m_colText != null);
			m_viewModel = viewModel;

			m_originalDefaultFont = new FontProxy(DefaultCellStyle.Font);
			SetFontsFromViewModel();

			m_minimumWidthFromDesigner = MinimumSize.Width;
			SizeRefColumnForCurrentBook();

			m_viewModel.CurrentBlockChanged += CurrentBlockChanged;
			m_viewModel.UiFontSizeChanged += HandleUiFontSizeChanged;
		}

		private void HandleUiFontSizeChanged(object sender, EventArgs eventArgs)
		{
			SetFontsFromViewModel();
			if (!m_userResizedRefColumn)
				SizeRefColumnForCurrentBook();
		}

		private void CurrentBlockChanged(object sender, EventArgs eventArgs)
		{
			if (m_bookIdUsedToSizeRefColumn != m_viewModel.CurrentBookId && !m_userResizedRefColumn)
				SizeRefColumnForCurrentBook();
		}

		public void UpdateContext()
		{
			int firstRow, lastRow;
			m_updatingContext = true;
			Cursor restoreCursor = Cursor.Current;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				SuspendLayout();

				ResetSelectionBackColors();

				ClearSelection();
				bool changingRowCount = RowCount != m_viewModel.BlockCountForCurrentBook;
				firstRow = m_viewModel.IndexOfFirstBlockInCurrentGroup;
				lastRow = m_viewModel.IndexOfLastBlockInCurrentGroup;
				bool multiSelect = firstRow != lastRow;
				if (changingRowCount || MultiSelect != multiSelect)
				{
					MultiSelect = multiSelect;
					if (changingRowCount)
					{
						AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
						RowCount = m_viewModel.BlockCountForCurrentBook;
					}
					// Need to clear the selection here again because some of the property setters on
					// DataGridView have the side-effect of creating a selection. We want to avoid having
					// HandleDataGridViewBlocksCellValueNeeded get called with an index that is out of
					// range for the new book.
					ClearSelection();
				}

				for (var i = firstRow; i <= lastRow; i++)
				{
					Rows[i].Selected = true;
					if (m_viewModel.BlockGroupingStyle == BlockGroupingType.BlockCorrelation)
					{
						Rows[i].DefaultCellStyle.SelectionBackColor = GlyssenColorPalette.ColorScheme.GetMatchColor(i - firstRow);
						Rows[i].DefaultCellStyle.SelectionForeColor = Color.Black;

						var characterId = (string)Rows[i].Cells["colCharacter"].Value;
						Rows[i].Cells["colText"].Style.SelectionForeColor = GlyssenColorPalette.ColorScheme.GetForeColorByCharacterId(characterId);
					}
				}

				if (changingRowCount)
					AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			}
			finally
			{
				ResumeLayout();
				Cursor.Current = restoreCursor;
			}

			m_updatingContext = false;

			ScrollDesiredRowsIntoView(firstRow, lastRow);
		}

		//public void Clear()
		//{
		//	m_updatingContext = true;
		//	SuspendLayout();
		//	ClearSelection();
		//	RowCount = 0;
		//	ResizeFirstColumn();
		//	ResumeLayout();
		//	m_updatingContext = false;
		//}
		#endregion

		#region Methods to control automated and user column sizing
		protected override void OnResize(EventArgs e)
		{
			m_userIsResizingColumns = false;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				base.OnResize(e);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
		{
			base.OnCellMouseEnter(e);
			if (Visible && e.RowIndex == -1)
			{
				// We don't want to set this to false when the mouse leaves the header row because that can happen before
				// all the column width changes get processed. So it will remain true until some other event (there are
				// several) sets it back to false.
				m_userIsResizingColumns = true;
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			m_userIsResizingColumns = false;
			base.OnLostFocus(e);
		}

		protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
		{
			base.OnColumnWidthChanged(e);
			if (m_userIsResizingColumns && e.Column.Index == m_colReference.Index)
			{
				var overage = Columns.Cast<DataGridViewColumn>().Sum(col => col.Width) - (ClientRectangle.Width - VerticalScrollBar.Width);
				if (overage > 0)
				{
					bool restore = m_userIsResizingColumns;
					m_userIsResizingColumns = false;
					e.Column.Width -= overage;
					m_userIsResizingColumns = restore;
				}

				if (!m_userResizedRefColumn)
				{
					m_userResizedRefColumn = true;
					int minWidth = Width - ClientRectangle.Width + VerticalScrollBar.Width +
									Columns.Cast<DataGridViewColumn>().Sum(col => col.MinimumWidth + col.DividerWidth);
					if (minWidth > m_minimumWidthFromDesigner && minWidth != MinimumSize.Width)
					{
						MinimumSize = new Size(minWidth, MinimumSize.Height);
						MinimumWidthChanged?.Invoke(this, new EventArgs());
					}
				}
			}
		}

		private void SizeRefColumnForCurrentBook()
		{
			m_userIsResizingColumns = false;
			m_colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

			int bookNum = BCVRef.BookToNumber(m_viewModel.CurrentBookId);
			var lastChapter = m_viewModel.Versification.GetLastChapter(bookNum);
			var maxVerse = 0;
			for (int i = 1; i <= lastChapter; i++)
				maxVerse = Math.Max(maxVerse, m_viewModel.Versification.GetLastVerse(bookNum, i));

			var startRef = new BCVRef(bookNum, lastChapter, maxVerse - 1);
			var endRef = new BCVRef(bookNum, lastChapter, maxVerse);
			var refString = m_viewModel.GetReferenceString(startRef, endRef);

			DataGridViewCellStyle cellStyle = m_colReference.DefaultCellStyle;
			using (Graphics g = CreateGraphics())
			{
				Debug.Assert(CellBorderStyle == DataGridViewCellBorderStyle.Single);
				const int borderWidth = 1;
				TextFormatFlags flags = ComputeTextFormatFlagsForCellStyleAlignment(m_viewModel.RightToLeftScript);
				m_colReference.Width = DataGridViewCell.MeasureTextWidth(g, refString,
					cellStyle.Font ?? DefaultCellStyle.Font, Int32.MaxValue, flags) +
					cellStyle.Padding.Horizontal + borderWidth;
			}

			CalculateMinimumWidth();

			m_bookIdUsedToSizeRefColumn = m_viewModel.CurrentBookId;
		}

		private static TextFormatFlags ComputeTextFormatFlagsForCellStyleAlignment(bool rightToLeft)
        {
            TextFormatFlags tff = TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping;
            if (rightToLeft)
                tff |= TextFormatFlags.Right |TextFormatFlags.RightToLeft;
            else
                tff |= TextFormatFlags.Left;
            return tff;
        }


		private void CalculateMinimumWidth()
		{
			int minWidth = Width - ClientRectangle.Width + VerticalScrollBar.Width;
			foreach (DataGridViewColumn col in Columns.Cast<DataGridViewColumn>())
			{
				if (col == m_colReference)
				{
					minWidth += col.Width;
					col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				}
				else
					minWidth += col.MinimumWidth;

				minWidth += col.DividerWidth;
			}
			if (minWidth > m_minimumWidthFromDesigner)
			{
				MinimumSize = new Size(minWidth, MinimumSize.Height);
				if (MinimumWidthChanged != null)
					MinimumWidthChanged(this, new EventArgs());
			}
		}
		#endregion

		#region private methods
		private void ResetSelectionBackColors()
		{
			if (m_viewModel.BlockGroupingStyle != BlockGroupingType.BlockCorrelation)
			{
				foreach (DataGridViewRow row in SelectedRows)
				{
					row.DefaultCellStyle.SelectionBackColor = DefaultCellStyle.SelectionBackColor;
					row.DefaultCellStyle.SelectionForeColor = DefaultCellStyle.SelectionForeColor;
					row.Cells["colText"].Style.SelectionForeColor = DefaultCellStyle.SelectionForeColor;
				}
			}
		}

		private void SetFontsFromViewModel()
		{
			m_colText.DefaultCellStyle.Font = m_viewModel.Font;
			m_originalDefaultFont.FontSizeUiAdjustment = m_viewModel.FontSizeUiAdjustment;
			DefaultCellStyle.Font = m_originalDefaultFont;
		}

		private void ScrollDesiredRowsIntoView(int firstRow, int lastRow)
		{
			if (m_viewModel.CurrentReferenceTextMatchup != null)
			{
				// I think that the fix for PG-884 (to call this method asynchronously from OnRowHeightChanged)
				// is the "real" fix for PG-810. So most likely, this try-catch is no longer needed. But since
				// I was never able to reproduce PG-810, I'm leaving it in here for now.
				try
				{
					FirstDisplayedScrollingRowIndex = firstRow;
				}
				catch (Exception exception)
				{
					Analytics.ReportException(exception, new Dictionary<string, string>
					{
						{"firstRow", firstRow.ToString()},
						{"lastRow", lastRow.ToString()},
						{"RowCount", RowCount.ToString()},
						{"existing FirstDisplayedScrollingRowIndex", FirstDisplayedScrollingRowIndex.ToString()},
						{"m_viewModel.CurrentBookId", m_viewModel.CurrentBookId},
					});
					ErrorReport.ReportNonFatalExceptionWithMessage(exception,
						"Although this is not a fatal error, the Glyssen developers are trying to find the cause of this problem (PG-810) so it can be fixed." +
						" Please report this if possible." + Environment.NewLine +
						"firstRow = " + firstRow + Environment.NewLine +
						"lastRow = " + lastRow + Environment.NewLine +
						"RowCount = " + RowCount + Environment.NewLine +
						"existing FirstDisplayedScrollingRowIndex = " + FirstDisplayedScrollingRowIndex + Environment.NewLine +
						"m_viewModel.CurrentBookId = " + m_viewModel.CurrentBookId + Environment.NewLine +
						"IndexOfStartBlockInBook = " + m_viewModel.CurrentReferenceTextMatchup.IndexOfStartBlockInBook + Environment.NewLine +
						"CorrelatedBlocks.Count = " + m_viewModel.CurrentReferenceTextMatchup.CorrelatedBlocks.Count);
				}
			}
			else
			{
				int precedingContextRows = 4;
				int followingContextRows = Math.Min(2, RowCount - lastRow - 1);
				var lastRowLocation = GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
				while (FirstDisplayedCell.RowIndex > firstRow || (lastRowLocation.Height == 0 || (firstRow != lastRow &&
					lastRowLocation.Y + lastRowLocation.Height > ClientRectangle.Height) ||
					GetCellDisplayRectangle(0, firstRow, true).Height < GetCellDisplayRectangle(0, firstRow, false).Height) &&
					precedingContextRows >= 0)
				{
					var firstRowOfContextToMakeVisible = Math.Max(0, firstRow - precedingContextRows--);
					FirstDisplayedScrollingRowIndex = firstRowOfContextToMakeVisible;

					if (followingContextRows > 0)
						followingContextRows--;
					lastRowLocation = GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
				}
			}
		}
		#endregion
	}
}
