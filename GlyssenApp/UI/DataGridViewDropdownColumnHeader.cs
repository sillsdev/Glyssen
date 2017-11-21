// Based on this article:  https://msdn.microsoft.com/en-us/library/aa480727.aspx
// Accessed: 08 FEB 2016

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Timer = System.Threading.Timer;

namespace GlyssenApp.UI
{
	/// <summary>
	/// Provides a drop-down filter list in a DataGridViewColumnHeaderCell.
	/// </summary>
	class DataGridViewDropdownColumnHeaderCell : DataGridViewColumnHeaderCell
	{
		public event EventHandler<CustomMenuItemClickedEventArgs> MenuItemClicked;

		/// <summary>The current width of the drop-down button. This field is used to adjust the cell padding.</summary>
		private int m_currentDropDownButtonPaddingOffset;

		private ContextMenuStrip m_dropdownMenu;

		// the menu only needs to be sized once
		private bool m_menuHasBeenSized;

		private bool m_intervalElapsed = true;
		private bool m_dropDownListBoxShowing;

		/// <summary>The bounds of the drop-down button, or Rectangle.Empty if the button bounds need to be recalculated.</summary>
		private Rectangle m_dropDownButtonBoundsValue = Rectangle.Empty;

		/// <summary></summary>
		/// <param name="oldHeaderCell">The DataGridViewColumnHeaderCell to copy property values from.</param>
		/// <param name="items"></param>
		public DataGridViewDropdownColumnHeaderCell(DataGridViewColumnHeaderCell oldHeaderCell, SortedList<int, string> items)
		{
			CopyCellProperties(oldHeaderCell);

			// add the items to a ContextMenuStrip
			SetupDropdownMenu(items);
        }

		private void CopyCellProperties(DataGridViewColumnHeaderCell oldHeaderCell)
		{
			ContextMenuStrip = oldHeaderCell.ContextMenuStrip;
			ErrorText = oldHeaderCell.ErrorText;
			Tag = oldHeaderCell.Tag;
			ToolTipText = oldHeaderCell.ToolTipText;
			Value = oldHeaderCell.Value;
			ValueType = oldHeaderCell.ValueType;

			// Use HasStyle to avoid creating a new style object
			// when the Style property has not previously been set. 
			if (oldHeaderCell.HasStyle)
			{
				Style = oldHeaderCell.Style;
			}

			// Copy this type's properties if the old cell is an auto-filter cell. 
			// This enables the Clone method to reuse this constructor. 
			var cell = oldHeaderCell as DataGridViewDropdownColumnHeaderCell;
			if (cell != null)
			{
				m_currentDropDownButtonPaddingOffset = cell.m_currentDropDownButtonPaddingOffset;
			}
		}

		private void SetupDropdownMenu(SortedList<int, string> items)
		{
			m_dropdownMenu = new ContextMenuStrip();
			foreach (var item in items)
			{
				var toolStripBtn = new ToolStripButton(item.Value)
				{
					Tag = item,
					AutoSize = false,
					DisplayStyle = ToolStripItemDisplayStyle.Text,
				};
				toolStripBtn.Click += toolStripBtn_Click;
				m_dropdownMenu.Items.Add(toolStripBtn);
			}
			m_dropdownMenu.VisibleChanged += m_dropdownMenu_VisibleChanged;
		}

		private void toolStripBtn_Click(object sender, EventArgs e)
		{
			var btn = (ToolStripButton)sender;
			var kvp = (KeyValuePair<int, string>)(btn.Tag);

			// return the column and event key in the event args
			if (MenuItemClicked != null)
				MenuItemClicked(sender, new CustomMenuItemClickedEventArgs(OwningColumn, kvp.Key));
		}

		private void m_dropdownMenu_VisibleChanged(object sender, EventArgs e)
		{
			if (m_dropdownMenu.Visible) return;

			// force the button to repaint
			m_dropDownListBoxShowing = false;
			InvalidateDropDownButtonBounds();
			DataGridView.InvalidateCell(this);

			// start the timer to prevent the menu from reappearing immediately
			m_intervalElapsed = false;
			var t = new Timer(WaitFinished);
			t.Change(200, Timeout.Infinite);
		}

		private void WaitFinished(object sender)
		{
			m_intervalElapsed = true;

			var t = (Timer) sender;
			t.Dispose();
		}

        /// <summary>
        /// This happens when the grid is resized
        /// </summary>
        protected override void OnDataGridViewChanged()
        {
            // Continue only if there is a DataGridView. 
            if (DataGridView == null)
            {
                return;
            }

            // Add handlers to DataGridView events. 
            HandleDataGridViewEvents();

            // Initialize the drop-down button bounds so that any initial
            // column autosizing will accommodate the button width. 
            SetDropDownButtonBounds();

            // Call the OnDataGridViewChanged method on the base class to 
            // raise the DataGridViewChanged event.
            base.OnDataGridViewChanged();
        }

        private void HandleDataGridViewEvents()
        {
            DataGridView.ColumnWidthChanged += DataGridView_ColumnWidthChanged;
            DataGridView.ColumnHeadersHeightChanged += DataGridView_ColumnHeadersHeightChanged;
        }

        private void DataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
			InvalidateDropDownButtonBounds();
        }

        private void DataGridView_ColumnHeadersHeightChanged(object sender, EventArgs e)
        {
			InvalidateDropDownButtonBounds();
        }

        /// <summary>
        /// Paints the column header cell, including the drop-down button. 
        /// </summary>
        /// <param name="graphics">The Graphics used to paint the DataGridViewCell.</param>
        /// <param name="clipBounds">A Rectangle that represents the area of the DataGridView that needs to be repainted.</param>
        /// <param name="cellBounds">A Rectangle that contains the bounds of the DataGridViewCell that is being painted.</param>
        /// <param name="rowIndex">The row index of the cell that is being painted.</param>
        /// <param name="cellState">A bitwise combination of DataGridViewElementStates values that specifies the state of the cell.</param>
        /// <param name="value">The data of the DataGridViewCell that is being painted.</param>
        /// <param name="formattedValue">The formatted data of the DataGridViewCell that is being painted.</param>
        /// <param name="errorText">An error message that is associated with the cell.</param>
        /// <param name="cellStyle">A DataGridViewCellStyle that contains formatting and style information about the cell.</param>
        /// <param name="advancedBorderStyle">A DataGridViewAdvancedBorderStyle that contains border styles for the cell that is being painted.</param>
        /// <param name="paintParts">A bitwise combination of the DataGridViewPaintParts values that specifies which parts of the cell need to be painted.</param>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, 
			DataGridViewElementStates cellState, object value, object formattedValue, string errorText, 
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
	        CheckMenuItemWidths(graphics);

            // Use the base method to paint the default appearance. 
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, 
                errorText, cellStyle, advancedBorderStyle, paintParts);

            // Continue only if ContentBackground is part of the paint request. 
            if ((paintParts & DataGridViewPaintParts.ContentBackground) == 0)
                return;

            // Retrieve the current button bounds. 
            var buttonBounds = DropDownButtonBounds;

            // Continue only if the buttonBounds is big enough to draw.
            if (buttonBounds.Width < 1 || buttonBounds.Height < 1) return;

            // Paint the button manually or using visual styles if visual styles 
            // are enabled, using the correct state depending on whether the 
            // filter list is showing and whether there is a filter in effect 
            // for the current column. 
            if (Application.RenderWithVisualStyles)
            {
                var state = ComboBoxState.Normal;

                if (m_dropDownListBoxShowing)
                    state = ComboBoxState.Pressed;

                ComboBoxRenderer.DrawDropDownButton(
                    graphics, buttonBounds, state);
            }
            else
            {
                // Determine the pressed state in order to paint the button 
                // correctly and to offset the down arrow. 
                var pressedOffset = 0;
                var state = PushButtonState.Normal;
                if (m_dropDownListBoxShowing)
                {
                    state = PushButtonState.Pressed;
                    pressedOffset = 1;
                }
                ButtonRenderer.DrawButton(graphics, buttonBounds, state);

                graphics.FillPolygon(SystemBrushes.ControlText, new[] {
                    new Point(
                        buttonBounds.Width / 2 + 
                            buttonBounds.Left - 1 + pressedOffset, 
                        buttonBounds.Height * 3 / 4 + 
                            buttonBounds.Top - 1 + pressedOffset),
                    new Point(
                        buttonBounds.Width / 4 + 
                            buttonBounds.Left + pressedOffset,
                        buttonBounds.Height / 2 + 
                            buttonBounds.Top - 1 + pressedOffset),
                    new Point(
                        buttonBounds.Width * 3 / 4 + 
                            buttonBounds.Left - 1 + pressedOffset,
                        buttonBounds.Height / 2 + 
                            buttonBounds.Top - 1 + pressedOffset)
                });
            }

        }

		/// <summary>
		/// We do this because allowing the menu to autosize causes the size to be reported incorrectly the first time
		/// </summary>
		/// <param name="graphics"></param>
		protected void CheckMenuItemWidths(Graphics graphics)
		{
			if (m_menuHasBeenSized) return;

			var maxWidth = 0.0;
			foreach (ToolStripButton btn in m_dropdownMenu.Items)
			{
				var textSize = graphics.MeasureString(btn.Text, btn.Font);
				if (textSize.Width > maxWidth) maxWidth = textSize.Width;
			}
			foreach (ToolStripButton btn in m_dropdownMenu.Items)
			{
				btn.Width = (int)maxWidth;
			}
			m_menuHasBeenSized = true;
		}

        /// <summary>
        /// Handles mouse clicks to the header cell, displaying the 
        /// drop-down list or sorting the owning column as appropriate. 
        /// </summary>
        /// <param name="e">A DataGridViewCellMouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            Debug.Assert(DataGridView != null, "DataGridView is null");

			// this allows the menu to close when the user clicks on the button again
	        if (!m_intervalElapsed) return;

            // retrieve the current size and location of the header cell, excluding any portion that is scrolled off screen. 
            var cellBounds = DataGridView.GetCellDisplayRectangle(e.ColumnIndex, -1, false);

            // Continue only if the mouse coordinates are not within the column resize zone. 
            if (((DataGridView.RightToLeft == RightToLeft.No && cellBounds.Width - e.X < 6) || e.X < 6))
            {
                return;
            }

            // unless RightToLeft is enabled, store the width of the portion that is scrolled off screen. 
            var scrollingOffset = 0;
            if (DataGridView.RightToLeft == RightToLeft.No && DataGridView.FirstDisplayedScrollingColumnIndex == ColumnIndex)
            {
                scrollingOffset = DataGridView.FirstDisplayedScrollingColumnHiddenWidth;
            }

            // Show the drop-down list if the mouse click occurred within the drop-down button bounds.
			// Otherwise, if sorting is enabled and the click occurred outside the drop-down button bounds, sort by the owning column. 
            // The mouse coordinates are relative to the cell bounds.
            if (DropDownButtonBounds.Contains(e.X + cellBounds.Left - scrollingOffset, e.Y + cellBounds.Top))
            {
                // If the current cell is in edit mode, commit the edit. 
                if (DataGridView.IsCurrentCellInEditMode)
                {
                    // Commit and end the cell edit.  
                    DataGridView.EndEdit();

                    // Commit any change to the underlying data source. 
                    var source = DataGridView.DataSource as BindingSource;
                    if (source != null)
                    {
                        source.EndEdit();
                    }
                }

	            var showAt = cellBounds.Location;
	            showAt.Y += cellBounds.Height;
	            showAt.X -= m_dropdownMenu.Width - cellBounds.Width;
	            m_dropdownMenu.Show(DataGridView.PointToScreen(showAt));

				// Set the size and location of dropDownListBox, then display it. 
				m_dropDownListBoxShowing = true;

				// Invalidate the cell so that the drop-down button will repaint in the pressed state. 
				DataGridView.InvalidateCell(this);
            }

            base.OnMouseDown(e);
        }

        #region button bounds: DropDownButtonBounds, InvalidateDropDownButtonBounds, SetDropDownButtonBounds, AdjustPadding
		
        /// <summary>
        /// The bounds of the drop-down button, or Rectangle.Empty if filtering
        /// is disabled. Recalculates the button bounds if filtering is enabled
        /// and the bounds are empty.
        /// </summary>
        protected Rectangle DropDownButtonBounds
        {
            get
            {
                if (m_dropDownButtonBoundsValue == Rectangle.Empty)
                {
                    SetDropDownButtonBounds();
                }
                return m_dropDownButtonBoundsValue;
            }
        }

        /// <summary>
        /// Sets dropDownButtonBoundsValue to Rectangle.Empty if it isn't already empty. 
        /// This indicates that the button bounds should be recalculated. 
        /// </summary>
        private void InvalidateDropDownButtonBounds()
        {
            if (!m_dropDownButtonBoundsValue.IsEmpty)
            {
                m_dropDownButtonBoundsValue = Rectangle.Empty;
            }
        }

        /// <summary>
        /// Sets the position and size of dropDownButtonBoundsValue based on the current 
        /// cell bounds and the preferred cell height for a single line of header text. 
        /// </summary>
        private void SetDropDownButtonBounds()
        {
            // Retrieve the cell display rectangle, which is used to 
            // set the position of the drop-down button. 
            var cellBounds = 
                DataGridView.GetCellDisplayRectangle(
                ColumnIndex, -1, false);

            // Initialize a variable to store the button edge length,
            // setting its initial value based on the font height. 
            var buttonEdgeLength = InheritedStyle.Font.Height + 5;

            // Calculate the height of the cell borders and padding.
            var borderRect = BorderWidths(
                DataGridView.AdjustColumnHeaderBorderStyle(
                DataGridView.AdvancedColumnHeadersBorderStyle,
                new DataGridViewAdvancedBorderStyle(), false, false));

            var borderAndPaddingHeight = 2 +
                borderRect.Top + borderRect.Height +
                InheritedStyle.Padding.Vertical;

            var visualStylesEnabled =
                Application.RenderWithVisualStyles &&
                DataGridView.EnableHeadersVisualStyles;

            if (visualStylesEnabled) 
            {
                borderAndPaddingHeight += 3;
            }

            // Constrain the button edge length to the height of the 
            // column headers minus the border and padding height. 
            if (buttonEdgeLength >
                DataGridView.ColumnHeadersHeight -
                borderAndPaddingHeight)
            {
                buttonEdgeLength =
                    DataGridView.ColumnHeadersHeight -
                    borderAndPaddingHeight;
            }

            // Constrain the button edge length to the
            // width of the cell minus three.
            if (buttonEdgeLength > cellBounds.Width - 3)
            {
                buttonEdgeLength = cellBounds.Width - 3;
            }

            // Calculate the location of the drop-down button, with adjustments
            // based on whether visual styles are enabled. 
            var topOffset = visualStylesEnabled ? 4 : 1;
            var top = cellBounds.Bottom - buttonEdgeLength - topOffset;
            var leftOffset = visualStylesEnabled ? 3 : 1;
            int left;
            if (DataGridView.RightToLeft == RightToLeft.No)
            {
                left = cellBounds.Right - buttonEdgeLength - leftOffset;
            }
            else
            {
                left = cellBounds.Left + leftOffset;
            }

            // Set the dropDownButtonBoundsValue value using the calculated 
            // values, and adjust the cell padding accordingly.  
            m_dropDownButtonBoundsValue = new Rectangle(left, top, 
                buttonEdgeLength, buttonEdgeLength);
            AdjustPadding(buttonEdgeLength + leftOffset);
        }

        /// <summary>
        /// Adjusts the cell padding to widen the header by the drop-down button width.
        /// </summary>
        /// <param name="newDropDownButtonPaddingOffset">The new drop-down button width.</param>
        private void AdjustPadding(int newDropDownButtonPaddingOffset)
        {
            // Determine the difference between the new and current 
            // padding adjustment.
            var widthChange = newDropDownButtonPaddingOffset - 
                m_currentDropDownButtonPaddingOffset;

            // If the padding needs to change, store the new value and 
            // make the change.
            if (widthChange != 0)
            {
                // Store the offset for the drop-down button separately from 
                // the padding in case the client needs additional padding.
                m_currentDropDownButtonPaddingOffset = 
                    newDropDownButtonPaddingOffset;
                
                // Create a new Padding using the adjustment amount, then add it
                // to the cell's existing Style.Padding property value. 
                var dropDownPadding = new Padding(0, 0, widthChange, 0);
                Style.Padding = Padding.Add(
                    InheritedStyle.Padding, dropDownPadding);
            }
        }

        #endregion button bounds

		protected override void Dispose(bool disposing)
		{
			if (DataGridView != null)
			{
				DataGridView.ColumnWidthChanged -= DataGridView_ColumnWidthChanged;
				DataGridView.ColumnHeadersHeightChanged -= DataGridView_ColumnHeadersHeightChanged;	
			}

			if (m_dropdownMenu != null)
			{
				m_dropdownMenu.VisibleChanged -= m_dropdownMenu_VisibleChanged;
			}

			if (m_dropdownMenu != null)
			{
				foreach (ToolStripButton btn in m_dropdownMenu.Items)
				{
					btn.Click -= toolStripBtn_Click;
				}
			}

			base.Dispose(disposing);
		}
	}

	class CustomMenuItemClickedEventArgs : EventArgs
	{
		public int EventKey;
		public DataGridViewColumn Column;

		public CustomMenuItemClickedEventArgs(DataGridViewColumn column, int eventKey)
		{
			Column = column;
			EventKey = eventKey;
		}
	}
}
