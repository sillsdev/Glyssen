using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Glyssen.Controls
{
    /// <summary>
    /// Represents the hosted Multi-Column Combo Box control in a <see cref="T:Glyssen.Controls.DataGridViewMultiColumnComboBoxCell"/>.
	/// http://www.intertech.com/Blog/winforms-multi-column-combo-box-in-a-datagridview/
    /// </summary>
    public class DataGridViewMultiColumnComboBoxEditingControl : DataGridViewComboBoxEditingControl
    {
        #region "Member Variables"

        private readonly List<Int32> m_columnWidths = new List<int>();
        private List<string> m_columnWidthStringList = new List<string>();
        private List<string> m_columnNames = new List<string>();

        #endregion

        #region "Constructor"

        /// <summary>
		/// Initializes a new instance of the <see cref="T:Glyssen.Controls.DataGridViewMultiColumnComboBoxEditingControl"/> class.
        /// </summary>
        public DataGridViewMultiColumnComboBoxEditingControl()
        {
            // Initialize all properties.
            AutoComplete = true;
            AutoDropdown = true;
            BackColorEven = Color.White;
            BackColorOdd = Color.White;
            ColumnWidths = new List<string>();
            ColumnWidthDefault = 75;
            TotalWidth = 0;
            ColumnNames = new List<string>();
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownStyle = ComboBoxStyle.DropDown;
            OwnerCell = null;

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            ContextMenu = new ContextMenu();
            EditingControlValueChanged = false;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        #endregion

        #region "Properties"
        /// <summary>
        /// Gets or sets the auto complete property.  The default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool AutoComplete { get; set; }
        /// <summary>
        /// Gets or sets the auto drop down property.  The default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool AutoDropdown { get; set; }
        /// <summary>
        /// Gets or sets the background color for the even rows portion.  The default is White.
        /// </summary>
        [DefaultValue(typeof(Color), "White")]
        public Color BackColorEven { get; set; }
        /// <summary>
        /// Gets or sets the background color for the odd rows portion.  The default is White.
        /// </summary>
        [DefaultValue(typeof(Color), "White")]
        public Color BackColorOdd { get; set; }

        /// <summary>
        /// Gets or sets the width of the columns to display.
        /// The default is <see cref="System.Collections.ObjectModel.Collection&lt;Int32&gt;"/>.
        /// </summary>
        public List<String> ColumnWidths
        {
            get
            {
                return m_columnWidthStringList;
            }
            set
            {
                if (value == null) value = new List<string>();


                var invalidValue = "";
                var invalidIndex = -1;
                var idx = 0;

                // iterate through the strings and check that they're all integers
                // or blanks
                foreach (var s in value)
                {
                    // If it has length, test if it's an integer
                    if (!String.IsNullOrWhiteSpace(s))
                    {
                        // It's not an integer. Flag the offending value.
                        int intValue;
                        if (!int.TryParse(s, out intValue))
                        {
                            invalidIndex = idx;
                            invalidValue = s;
                        }
                        else // The value was okay. Increment the item index.
                        {
                            idx++;
                        }
                    }
                    else // The value is a space. Use the default width.
                    {
                        idx++;
                    }
                }

                // If an invalid value was found, raise an exception.
                if (invalidIndex > -1)
                {
                    var errMsg = "Invalid column width '" + invalidValue + "' located at column " + invalidIndex;
                    throw new ArgumentOutOfRangeException(errMsg);
                }

                m_columnWidthStringList = value;

                // Only set the values of the collections at runtime.
                // Setting them at design time doesn't accomplish 
                // anything and causes errors since the collections 
                // don't exist at design time.
                if (DesignMode) return;

                m_columnWidths.Clear();
                foreach (var s in value)
                {
                    // Initialize a column width to an integer
                    m_columnWidths.Add(Convert.ToBoolean(s.Trim().Length)
                        ? Convert.ToInt32(s)
                        : ColumnWidthDefault);
                }

                // If the column is bound to data, set the column widths
                // for any columns that aren't explicitly set by the 
                // string value entered by the programmer
                if (DataManager != null)
                {
                    InitializeColumns();
                }
            }
        }
        /// <summary>
        /// Gets or sets the default column width.  The default is 75.
        /// </summary>
        [DefaultValue(75)]
        public int ColumnWidthDefault { get; set; }
        /// <summary>
        /// Gets or sets the total width of the drop down. The default is 0.
        /// </summary>
        [DefaultValue(0)]
        public int TotalWidth { get; private set; }

        /// <summary>
        /// Gets or sets the names of the columns to display.  The default is <see cref="System.Collections.ObjectModel.Collection&lt;String&gt;"/>.
        /// </summary>
        public List<String> ColumnNames
        {
            get
            {
                return m_columnNames;
            }
            set
            {
                if (value == null) value = new List<string>();

                if (value.Any(String.IsNullOrWhiteSpace))
                    throw new NotSupportedException("Column name cannot be blank.");

                var columnNames = value.ToList();

                if (!DesignMode)
                {
                    m_columnNames.Clear();
                }

                m_columnNames = columnNames.Select(cn => cn.Trim()).ToList();
            }
        }
        /// <summary>
        /// Gets a value indicating your code will handle drawing of elements in the list.
        /// </summary>
        /// 
        /// <returns>
        /// The default is <see cref="F:System.Windows.Forms.DrawMode.OwnerDrawVariable"/>.
        /// </returns>
        [DefaultValue(DrawMode.OwnerDrawVariable)]
        public new DrawMode DrawMode
        {
            get
            {
                return base.DrawMode;
            }
            private set
            {
                if (value != DrawMode.OwnerDrawVariable)
                {
                    throw new NotSupportedException("Needs to be DrawMode.OwnerDrawVariable");
                }
                base.DrawMode = value;
            }
        }
        /// <summary>
        /// Gets a value specifying the style of the combo box to be DropDown.
        /// </summary>
        /// 
        /// <returns>
        /// The only value is DropDown.
        /// </returns>
        [DefaultValue(ComboBoxStyle.DropDown)]
        public new ComboBoxStyle DropDownStyle
        {
            get
            {
                return base.DropDownStyle;
            }
            private set
            {
                if (value != ComboBoxStyle.DropDown)
                {
                    throw new NotSupportedException("ComboBoxStyle.DropDown is the only supported style");
                }
                base.DropDownStyle = value;
            }
        }
        /// <summary>
        /// Gets or sets a value specifying the owner of this control.
        /// </summary>
        public DataGridViewMultiColumnComboBoxCell OwnerCell { get; set; }

        #endregion

        #region "Event Handlers"

        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
            InitializeColumns();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DrawItem"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data. </param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || DesignMode)
	        {
		        base.OnDrawItem(e);
                return;
	        }

            e.DrawBackground();

            var boundsRect = e.Bounds;
            var lastRight = 0;

			var column = OwnerCell.OwningColumn as DataGridViewMultiColumnComboBoxColumn;
	        var drawItemSpecial = (column != null && column.HideFirstValueWhenSelected && e.Index == 0);

	        if (e.State.HasFlag(DrawItemState.ComboBoxEdit))
	        {
		        if (drawItemSpecial)
			        return;
		        boundsRect.Height--;
	        }

	        Color brushForeColor;
            if ((e.State & DrawItemState.Selected) == 0)
            {
                // Item is not selected. Use BackColorOdd & BackColorEven
                var backColor = Convert.ToBoolean(e.Index % 2) ? BackColorOdd : BackColorEven;
                using (var brushBackColor = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brushBackColor, boundsRect);
                }
                brushForeColor = Color.Black;
            }
            else
            {
                // Item is selected. Use ForeColor = White
                brushForeColor = Color.White;
            }

	        if (e.State.HasFlag(DrawItemState.ComboBoxEdit))
	        {
		        lastRight += 2;
	        }

	        Font font = Font;
	        if (column != null && column.FontForFirstItem != null && e.Index == 0)
		        font = column.FontForFirstItem;

            using (var linePen = new Pen(SystemColors.GrayText))
            {
                using (var brush = new SolidBrush(brushForeColor))
                {
                    if (ColumnNames.Count == 0)
                    {
                        e.Graphics.DrawString(Convert.ToString(Items[e.Index]), font, brush, boundsRect);
                    }
                    else
                    {
                        // If the ComboBox is displaying a RightToLeft language, draw it this way.
                        if (RightToLeft.Equals(RightToLeft.Yes))
                        {
                            // Define a StringFormat object to make the string display RTL.
                            var rtl = new StringFormat
                            {
                                Alignment = StringAlignment.Near,
                                FormatFlags = StringFormatFlags.DirectionRightToLeft
                            };

                            // Draw the strings in reverse order from high column index to zero column index.
                            for (var colIndex = ColumnNames.Count - 1; colIndex >= 0; colIndex--)
                            {
                                if (!Convert.ToBoolean(ColumnWidths[colIndex])) continue;

                                var item = Convert.ToString(FilterItemOnProperty(Items[e.Index], ColumnNames[colIndex]));

                                boundsRect.X = lastRight;
                                boundsRect.Width = m_columnWidths[colIndex];
                                lastRight = boundsRect.Right;

                                // Draw the string with the RTL object.
                                e.Graphics.DrawString(item, font, brush, boundsRect, rtl);

                                if (colIndex > 0 && !drawItemSpecial)
                                {
                                    e.Graphics.DrawLine(linePen, boundsRect.Right, boundsRect.Top, boundsRect.Right, boundsRect.Bottom);
                                }
                            }
                        }
                        // If the ComboBox is displaying a LeftToRight language, draw it this way.
                        else
                        {
                            // Display the strings in ascending order from zero to the highest column.
							for (var colIndex = 0; colIndex < ColumnNames.Count; colIndex++)
                            {
                                if (Convert.ToBoolean(m_columnWidths[colIndex]))
                                {
                                    var item = Convert.ToString(FilterItemOnProperty(Items[e.Index], ColumnNames[colIndex]));

                                    boundsRect.X = lastRight;
                                    boundsRect.Width = m_columnWidths[colIndex];
                                    lastRight = boundsRect.Right;
                                    e.Graphics.DrawString(item, font, brush, boundsRect);

                                    if (colIndex < ColumnNames.Count - 1 && !drawItemSpecial)
                                    {
                                        e.Graphics.DrawLine(linePen, boundsRect.Right - 1, boundsRect.Top, boundsRect.Right - 1, boundsRect.Bottom);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDown"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnDropDown(EventArgs e)
        {
            if (TotalWidth <= 0) return;

            if (Items.Count > MaxDropDownItems)
            {
                // The vertical scrollbar is present. Add its width to the total.
                // If you don't then RightToLeft languages will have a few characters obscured.
                DropDownWidth = TotalWidth + SystemInformation.VerticalScrollBarWidth;
            }
            else
            {
                DropDownWidth = TotalWidth;
            }
        }
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int idx;
            string toFind;

            DroppedDown = AutoDropdown;
            if (!Char.IsControl(e.KeyChar))
            {
                if (AutoComplete)
                {
                    toFind = Text.Substring(0, SelectionStart) + e.KeyChar;
                    idx = FindStringExact(toFind);

                    if (idx == -1)
                    {
                        // An exact match for the whole string was not found
                        // Find a substring instead.
                        idx = FindString(toFind);
                    }
                    else
                    {
                        // An exact match was found. Close the dropdown.
                        DroppedDown = false;
                    }

                    if (idx != -1) // The substring was found.
                    {
                        SelectedIndex = idx;
                        SelectionStart = toFind.Length;
                        SelectionLength = Text.Length - SelectionStart;
                    }
                    else // The last keystroke did not create a valid substring.
                    {
                        // If the substring is not found, cancel the keypress
                        e.KeyChar = (char)0;
                    }
                }
                else // AutoComplete = false. Treat it like a DropDownList by finding the
                // KeyChar that was struck starting from the current index
                {
                    idx = FindString(e.KeyChar.ToString(CultureInfo.InvariantCulture), SelectedIndex);

                    if (idx != -1)
                    {
                        SelectedIndex = idx;
                    }
                }
            }

            // Do no allow the user to backspace over characters. Treat it like
            // a left arrow instead. The user must not be allowed to change the 
            // value in the ComboBox. 
            if ((e.KeyChar == (char)(Keys.Back)) &&  // A Backspace Key is hit
                (AutoComplete) &&                   // AutoComplete = true
                (Convert.ToBoolean(SelectionStart))) // And the SelectionStart is positive
            {
                // Find a substring that is one character less the the current selection.
                // This mimicks moving back one space with an arrow key. This substring should
                // always exist since we don't allow invalid selections to be typed. If you're
                // on the 3rd character of a valid code, then the first two characters have to 
                // be valid. Moving back to them and finding the 1st occurrence should never fail.
                toFind = Text.Substring(0, SelectionStart - 1);
                idx = FindString(toFind);

                if (idx != -1)
                {
                    SelectedIndex = idx;
                    SelectionStart = toFind.Length;
                    SelectionLength = Text.Length - SelectionStart;
                }
            }

            // e.Handled is always true. We handle every keystroke programatically.
            e.Handled = true;
        }

        #endregion

        #region "Methods"

	    /// <summary>
	    /// Sets what columns to be displayed and calculates the width to use to display them.
	    /// </summary>
	    private void InitializeColumns()
	    {
		    if (ColumnNames.Count == 0)
		    {
			    var propertyDescriptorCollection = DataManager.GetItemProperties();
			    for (var colIndex = 0; colIndex < propertyDescriptorCollection.Count; colIndex++)
				    ColumnNames.Add(propertyDescriptorCollection[colIndex].Name);
		    }

		    TotalWidth = 0;
		    using (var graphics = CreateGraphics())
		    {
			    for (var colIndex = 0; colIndex < ColumnNames.Count; colIndex++)
			    {
				    // If no column widths are explicitly set, calculate the width required to show the longest item.
				    if (ColumnWidths.Count == 0)
				    {
					    int maxWidth = 0;
					    foreach (var item in Items)
					    {
						    var value = Convert.ToString(FilterItemOnProperty(item, ColumnNames[colIndex]));
						    var size = graphics.MeasureString(value, Font);
						    if (size.Width > maxWidth)
							    maxWidth = (int)Math.Ceiling(size.Width);
					    }
					    m_columnWidths.Add(maxWidth);
				    }
				    // If the index is greater than the collection of explicitly set column widths, use the default.
				    else if (colIndex >= ColumnWidths.Count)
				    {
					    m_columnWidths.Add(ColumnWidthDefault);
				    }
				    TotalWidth += m_columnWidths[colIndex];
			    }
		    }
	    }

	    #endregion
    }
}
