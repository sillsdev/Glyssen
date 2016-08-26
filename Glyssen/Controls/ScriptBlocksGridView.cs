using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Dialogs;
using Glyssen.Utilities;

namespace Glyssen.Controls
{
	public class ScriptBlocksGridView : DataGridView
	{
		private bool m_updatingContext = true;
		private DataGridViewTextBoxColumn m_colReference;
		private DataGridViewTextBoxColumn m_colText;
		private BlockNavigatorViewModel m_viewModel;
		private FontProxy m_originalDefaultFont;

		private Dictionary<DataGridViewColumn, DataGridViewAutoSizeColumnMode> m_originalColumnSizeModes;
		private bool m_userIsResizingColumns;
		private int m_widthOfAutoSizeColumns;

		public event EventHandler MinimumWidthChanged;

		#region overrides
		protected override void OnRowHeightChanged(DataGridViewRowEventArgs e)
		{
			base.OnRowHeightChanged(e);
			if (!m_updatingContext && SelectedRows.Count > 0)
			{
				var firstRow = SelectedRows[SelectedRows.Count - 1].Index;
				var lastRow = SelectedRows[0].Index;
				if (e.Row.Index > firstRow - 5 && e.Row.Index < lastRow + 2)
					this.SafeInvoke(() => ScrollDesiredRowsIntoView(firstRow, lastRow), true);
			}
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
			if (m_updatingContext && (e.RowIndex < 0 || e.RowIndex >= m_viewModel.BlockCountForCurrentBook))
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
								e.Value = correspondingOrigBlock.CharacterIsUnclear() ? "" : correspondingOrigBlock.CharacterId;
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
			if (!e.Handled && m_viewModel != null && m_viewModel.Font.RightToLeftScript && e.ColumnIndex == m_colText.Index && e.RowIndex >= 0)
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

		#endregion

		#region public methods
		public void Initialize(BlockNavigatorViewModel viewModel)
		{
			m_colReference = (DataGridViewTextBoxColumn)Columns[0];
			m_colText = (DataGridViewTextBoxColumn)Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
			Debug.Assert(m_colReference != null);
			Debug.Assert(m_colText != null);
			m_viewModel = viewModel;

			m_originalDefaultFont = new FontProxy(DefaultCellStyle.Font);
			SetFontsFromViewModel();

			m_viewModel.UiFontSizeChanged += (sender, args) => SetFontsFromViewModel();
		}

		public void UpdateContext()
		{
			m_updatingContext = true;
			SuspendLayout();
	
			ResetSelectionBackColors();

			ClearSelection();
			bool changingRowCount = RowCount != m_viewModel.BlockCountForCurrentBook;
			var firstRow = m_viewModel.IndexOfFirstBlockInCurrentGroup;
			var lastRow = m_viewModel.IndexOfLastBlockInCurrentGroup;
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
				}
			}

			if (changingRowCount)
				AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			
			ResumeLayout();

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
		protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
		{
			base.OnColumnWidthChanged(e);
			if (!Visible || !m_userIsResizingColumns)
				return;

			if (e.Column.AutoSizeMode == DataGridViewAutoSizeColumnMode.None &&
				Columns.Cast<DataGridViewColumn>().Sum(col => col.Width) > ClientRectangle.Width)
			{
				bool restore = m_userIsResizingColumns;
				m_userIsResizingColumns = false;
				e.Column.Width = ClientRectangle.Width -
					Columns.Cast<DataGridViewColumn>().Where(col => col != e.Column).Sum(col => col.Width);
				m_userIsResizingColumns = restore;
			}

			int newWidthOfAutoSizeColumns = Columns.Cast<DataGridViewColumn>().Where(ColumnAutoSizeIsDeterminedByContent).Sum(col => col.Width);
			if (m_widthOfAutoSizeColumns != newWidthOfAutoSizeColumns)
			{
				MinimumSize = new Size(MinimumSize.Width - m_widthOfAutoSizeColumns + newWidthOfAutoSizeColumns, MinimumSize.Height);
				m_widthOfAutoSizeColumns = newWidthOfAutoSizeColumns;
				if (MinimumWidthChanged != null)
					MinimumWidthChanged(this, new EventArgs());
			}
		}

		private bool ColumnAutoSizeIsDeterminedByContent(DataGridViewColumn col)
		{
			switch (col.AutoSizeMode)
			{
				case DataGridViewAutoSizeColumnMode.ColumnHeader:
				case DataGridViewAutoSizeColumnMode.AllCells:
				case DataGridViewAutoSizeColumnMode.AllCellsExceptHeader:
				case DataGridViewAutoSizeColumnMode.DisplayedCells:
				case DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader:
					return true;
			}
			return false;
		}

		protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
		{
			base.OnCellMouseEnter(e);
			m_userIsResizingColumns = true;
			if (e.ColumnIndex == -1)
			{
				foreach (var col in m_originalColumnSizeModes.Keys)
					col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			}
		}

		protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
		{
			base.OnCellMouseLeave(e);
			m_userIsResizingColumns = false;
			if (e.ColumnIndex == -1)
			{
				foreach (var colSizeMode in m_originalColumnSizeModes)
					colSizeMode.Key.AutoSizeMode = colSizeMode.Value;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!Visible || m_originalColumnSizeModes != null)
				return;

			m_originalColumnSizeModes = new Dictionary<DataGridViewColumn, DataGridViewAutoSizeColumnMode>();

			int minWidth = Width - ClientRectangle.Width + VerticalScrollBar.Width;
			m_widthOfAutoSizeColumns = 0;
			foreach (DataGridViewColumn col in Columns.Cast<DataGridViewColumn>())
			{
				if (ColumnAutoSizeIsDeterminedByContent(col))
					m_widthOfAutoSizeColumns += col.Width;
				else
					minWidth += col.MinimumWidth;

				minWidth += col.DividerWidth;
				if (col != m_colText && col.AutoSizeMode != DataGridViewAutoSizeColumnMode.None)
					m_originalColumnSizeModes[col] = col.AutoSizeMode;
			}
			minWidth += m_widthOfAutoSizeColumns;
			if (minWidth > MinimumSize.Width)
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
				}
			}
		}

		private void SetFontsFromViewModel()
		{
			m_colText.DefaultCellStyle.Font = m_viewModel.Font;
			DefaultCellStyle.Font = m_originalDefaultFont.AdjustFontSize(m_viewModel.FontSizeUiAdjustment);
		}

		private void ScrollDesiredRowsIntoView(int firstRow, int lastRow)
		{
			if (m_viewModel.CurrentReferenceTextMatchup != null)
			{
				FirstDisplayedScrollingRowIndex = firstRow;
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
