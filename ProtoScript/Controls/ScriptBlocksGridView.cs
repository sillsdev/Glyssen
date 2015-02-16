using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProtoScript.Dialogs;

namespace ProtoScript.Controls
{
	public class ScriptBlocksGridView : DataGridView
	{
		private bool m_updatingContext = true;
		private DataGridViewTextBoxColumn m_colReference;
		private DataGridViewTextBoxColumn m_colText;
		private BlockNavigatorViewModel m_viewModel;
		private Font m_originalDefaultFont;

		#region overrides
		protected override void OnRowHeightChanged(DataGridViewRowEventArgs e)
		{
			base.OnRowHeightChanged(e);
			if (SelectedRows.Count > 0)
			{
				var firstRow = SelectedRows[SelectedRows.Count - 1].Index;
				var lastRow = SelectedRows[0].Index;
				if (e.Row.Index > firstRow - 5 && e.Row.Index < lastRow + 2)
					BeginInvoke(new Action(() => ScrollDesiredRowsIntoView(firstRow, lastRow)));
			}
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			if (m_updatingContext)
				return;

			if (SelectedRows.Count > 0 && m_viewModel.GetIsBlockScripture(SelectedRows[0].Index))
				m_viewModel.CurrentBlockIndexInBook = SelectedRows[0].Index;

			base.OnSelectionChanged(e);
		}

		protected override void OnCellValueNeeded(DataGridViewCellValueEventArgs e)
		{
			var block = m_viewModel.GetNthBlockInCurrentBook(e.RowIndex);
			if (e.ColumnIndex == m_colReference.Index)
				e.Value = m_viewModel.GetBlockReferenceString(block);
			else if (e.ColumnIndex == m_colText.Index)
				e.Value = block.GetText(true);
			else
				base.OnCellValueNeeded(e);
		}

		protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
		{
			if (!e.Handled && m_viewModel != null && m_viewModel.RightToLeft && e.ColumnIndex == m_colText.Index && e.RowIndex >= 0)
			{
				e.PaintBackground(e.CellBounds, true);
				TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString(),
				e.CellStyle.Font, e.CellBounds, e.CellStyle.ForeColor,
				 TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.RightToLeft | TextFormatFlags.Right);
				e.Handled = true;
			}
			base.OnCellPainting(e);
		}
		#endregion

		#region public methods
		public void Initialize(BlockNavigatorViewModel viewModel)
		{
			m_colReference = (DataGridViewTextBoxColumn)Columns[0];
			m_colText = (DataGridViewTextBoxColumn)Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
			Debug.Assert(m_colReference != null);
			Debug.Assert(m_colText != null);
			m_viewModel = viewModel;

			m_originalDefaultFont = DefaultCellStyle.Font;
			SetFontsFromViewModel();

			m_viewModel.UiFontSizeChanged += (sender, args) => SetFontsFromViewModel();
		}

		public void UpdateContext()
		{
			m_updatingContext = true;
			SuspendLayout();
			// Need to clear the selction here and again below here because some of the property setters on
			// DataGridView have the side-effect of creating a selection. And since we might be changing the row
			// count, we can't afford to have HandleDataGridViewBlocksCellValueNeeded getting called with an
			// index that is out of range for the new book.
			ClearSelection();
			MultiSelect = m_viewModel.CurrentBlock.MultiBlockQuote != MultiBlockQuote.None;
			m_colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			RowCount = m_viewModel.BlockCountForCurrentBook;
			ClearSelection(); // see note, above.
			var firstRow = m_viewModel.CurrentBlockIndexInBook;
			var lastRow = firstRow;
			Rows[firstRow].Selected = true;
			if (m_viewModel.CurrentBlock.MultiBlockQuote == MultiBlockQuote.Start)
			{
				foreach (var i in m_viewModel.GetIndicesOfQuoteContinuationBlocks(m_viewModel.CurrentBlock))
				{
					Rows[i].Selected = true;
					lastRow = i;
				}
			}
			m_colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
			ScrollDesiredRowsIntoView(firstRow, lastRow);

			ResumeLayout();
			m_updatingContext = false;
		}

		public void Clear()
		{
			m_updatingContext = true;
			SuspendLayout();
			ClearSelection();
			RowCount = 0;
			m_colReference.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
			ResumeLayout();
			m_updatingContext = false;
		}
		#endregion

		#region private methods
		private void SetFontsFromViewModel()
		{
			m_colText.DefaultCellStyle.Font = m_viewModel.Font;
			DefaultCellStyle.Font = new Font(m_originalDefaultFont.FontFamily,
				m_originalDefaultFont.SizeInPoints + m_viewModel.FontSizeUiAdjustment, m_originalDefaultFont.Style);
		}

		private void ScrollDesiredRowsIntoView(int firstRow, int lastRow)
		{
			int precedingContextRows = 4;
			int followingContextRows = Math.Min(2, RowCount - lastRow - 1);
			var lastRowLocation = GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
			while ((lastRowLocation.Height == 0 || (firstRow != lastRow &&
				lastRowLocation.Y + lastRowLocation.Height > ClientRectangle.Height)) && precedingContextRows >= 0)
			{
				var firstRowOfContextToMakeVisible = Math.Max(0, firstRow - precedingContextRows--);
				FirstDisplayedScrollingRowIndex = firstRowOfContextToMakeVisible;

				if (followingContextRows > 0)
					followingContextRows--;
				lastRowLocation = GetCellDisplayRectangle(0, lastRow + followingContextRows, false);
			}
		}
		#endregion
	}
}
