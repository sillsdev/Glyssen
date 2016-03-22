using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
		private const int kSeparatorLineHeight = 1;
		private readonly List<Int32> m_columnWidths = new List<int>();
		private List<string> m_columnWidthStringList = new List<string>();
		private List<string> m_columnNames = new List<string>();
		private DataGridViewMultiColumnComboBoxCell m_ownerCell;
		private string m_newText;
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
			IndentSize = 18;
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
			get { return m_columnWidthStringList; }
			set
			{
				if (value == null)
					value = new List<string>();


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
				if (DesignMode)
					return;

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
		/// Gets or sets the number of pixels to indent (ignored if CategoryColumnName is not specified).
		/// </summary>
		[DefaultValue(18)]
		public int IndentSize { get; set; }

		/// <summary>
		/// Gets or sets the names of the columns to display.  The default is <see cref="System.Collections.ObjectModel.Collection&lt;String&gt;"/>.
		/// </summary>
		public List<String> ColumnNames
		{
			get { return m_columnNames; }
			set
			{
				if (value == null)
					value = new List<string>();

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
			get { return base.DrawMode; }
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
			get { return base.DropDownStyle; }
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
		public DataGridViewMultiColumnComboBoxCell OwnerCell
		{
			get { return m_ownerCell; }
			set
			{
				m_ownerCell = value;
				InitializeColumns();
				SetDropDownHeight();
			}
		}

		private void SetDropDownHeight()
		{
			if (Items.Count > 0)
				DropDownHeight = GetRequiredHeightForFirstNItems(MaxDropDownItems);
		}

		private DataGridViewMultiColumnComboBoxColumn OwningColumn
		{
			get { return OwnerCell == null ? null : (DataGridViewMultiColumnComboBoxColumn)OwnerCell.OwningColumn; }
		}
		#endregion

		#region "Event Handlers"

		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
		protected override void OnDataSourceChanged(EventArgs e)
		{
			base.OnDataSourceChanged(e);
			InitializeColumns();
			SetDropDownHeight();
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);
			e.ItemHeight = GetRequiredItemHeight(e.Index);
		}

		private int GetRequiredItemHeight(int index)
		{
			if (OwningColumn == null)
				return ItemHeight;

			object category;
			if (IsItemFirstInCategory(index, out category))
				return ItemHeight * 2 + kSeparatorLineHeight;

			if (OwningColumn.CategoryColumnName != null && FilterItemOnProperty(Items[index], OwningColumn.CategoryColumnName) is DBNull)
				return ItemHeight + kSeparatorLineHeight;

			return ItemHeight;
		}

		private int GetRequiredHeightForFirstNItems(int n)
		{
			if (n <= 0)
				throw new ArgumentOutOfRangeException("n", "The value of n must be greater than 0.");
			if (Items.Count == 0)
				throw new InvalidOperationException("Non-empty Items collection must be set before calling GetRequiredHeightForFirstNItems.");

			n = Math.Min(n, Items.Count);

			int requiredHeight = SystemInformation.BorderSize.Height * 2;
			for (int i = 0; i < n; i++)
				requiredHeight += GetRequiredItemHeight(i);
			return requiredHeight;
		}

		private bool IsItemFirstInCategory(int index, out object category)
		{
			if (OwningColumn != null && OwningColumn.CategoryColumnName != null)
			{
				category = FilterItemOnProperty(Items[index], OwningColumn.CategoryColumnName);
				return (!(category is DBNull) && (index == 0 || FilterItemOnProperty(Items[index - 1], OwningColumn.CategoryColumnName) != category));
			}
			category = null;
			return false;
		}

		private bool IsUncategorizedItemSpecial(int index)
		{
			return OwningColumn != null && OwningColumn.CategoryColumnName != null && FilterItemOnProperty(Items[index], OwningColumn.CategoryColumnName) is DBNull;
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
			Image icon = FilterItemOnProperty(Items[e.Index], "Icon") as Image;
			var lastRight = OwningColumn.CategoryColumnName == null || e.State.HasFlag(DrawItemState.ComboBoxEdit) || icon != null ? 0 : IndentSize;

			var drawUncategorizedItemSpecial = IsUncategorizedItemSpecial(e.Index);

			if (e.State.HasFlag(DrawItemState.ComboBoxEdit))
			{
				if (drawUncategorizedItemSpecial)
					return;
				boundsRect.Height--;
			}

			Color brushForeColor = Color.Black;
			if ((e.State & DrawItemState.Selected) == 0)
			{
				// Item is not selected. Use BackColorOdd & BackColorEven
				var backColor = Convert.ToBoolean(e.Index % 2) ? BackColorOdd : BackColorEven;
				using (var brushBackColor = new SolidBrush(backColor))
					e.Graphics.FillRectangle(brushBackColor, boundsRect);
			}

			Font font;

			object category;
			if (IsItemFirstInCategory(e.Index, out category) && !e.State.HasFlag(DrawItemState.ComboBoxEdit))
			{
				var categoryRect = new Rectangle(boundsRect.X, boundsRect.Y, boundsRect.Width, boundsRect.Height / 2);
				if ((e.State & DrawItemState.Selected) != 0)
				{
					// Item is selected. Repaint background for the category portion of the line.
					using (var brushBackColor = new SolidBrush(Color.White))
						e.Graphics.FillRectangle(brushBackColor, categoryRect);
				}

				var sCategory = Convert.ToString(category);
				font = OwningColumn.FontForCategories ?? Font;
				using (var brush = new SolidBrush(brushForeColor))
				{
					if (e.Index > 0)
					{
						using (var linePen = new Pen(SystemColors.GrayText))
							e.Graphics.DrawLine(linePen, categoryRect.X, categoryRect.Y, categoryRect.Right, categoryRect.Y);
					}
					categoryRect.Y += kSeparatorLineHeight;
					categoryRect.Height -= kSeparatorLineHeight;
					e.Graphics.DrawString(sCategory, font, brush, categoryRect);
				}
				boundsRect.Y = categoryRect.Bottom;
				boundsRect.Height -= categoryRect.Height;
			}

			if ((e.State & DrawItemState.Selected) != 0)
			{
				// Item is selected. Use ForeColor = White
				brushForeColor = Color.White;
			}

			if (e.State.HasFlag(DrawItemState.ComboBoxEdit))
			{
				lastRight += 2;
			}

			font = Font;
			if (drawUncategorizedItemSpecial)
			{
				if (OwningColumn.FontForUncategorizedItems != null)
					font = OwningColumn.FontForUncategorizedItems;
			}
			else if (OwningColumn.CategoryColumnName != null)
			{
				boundsRect.Width -= IndentSize;
			}

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
						if (drawUncategorizedItemSpecial)
						{
							if (e.Index > 0)
								e.Graphics.DrawLine(linePen, boundsRect.X, boundsRect.Y, boundsRect.Right, boundsRect.Y);
							boundsRect.Width -= IndentSize;
							boundsRect.Y += kSeparatorLineHeight;
							boundsRect.Height -= kSeparatorLineHeight;
						}

						if (icon != null)
						{
							int top = boundsRect.Y;
							int height = boundsRect.Height;
							int left = boundsRect.X;
							int width = IndentSize - 2;
							if (icon.Height < height)
							{
								top += ((boundsRect.Height - icon.Height) / 2);
								height = icon.Height;
							}
							if (icon.Width < width)
							{
								left += ((boundsRect.Width - icon.Width) / 2);
								width = icon.Width;
							}
							e.Graphics.DrawImage(icon, left, top, width, height);
							lastRight += IndentSize;
						}

						if (drawUncategorizedItemSpecial)
						{
							string item = Convert.ToString(FilterItemOnProperty(Items[e.Index], "SpecialUse"));
							boundsRect.X = lastRight;
							boundsRect.Width = boundsRect.Width;
							e.Graphics.DrawString(item, font, brush, boundsRect, new StringFormat());
						}
						else
						{
							// Default (L-to-R) is to display the strings in ascending order from zero to the highest column.
							int loopStart = 0;
							Func<int, bool> notDone = i => i < ColumnNames.Count;
							Func<int, int> loopIncrementer = i => i + 1;
							StringFormat format = new StringFormat();

							// If the ComboBox is displaying a RightToLeft language, draw it this way.
							if (RightToLeft.Equals(RightToLeft.Yes))
							{
								// Define a StringFormat object to make the string display RTL.
								format = new StringFormat
								{
									Alignment = StringAlignment.Near,
									FormatFlags = StringFormatFlags.DirectionRightToLeft
								};

								// Draw the strings in reverse order from high column index to zero column index.
								loopStart = ColumnNames.Count - 1;
								notDone = i => i >= 0;
								loopIncrementer = i => i - 1;
							}

							for (var colIndex = loopStart; notDone(colIndex); colIndex = loopIncrementer(colIndex))
							{
								if (m_columnWidths[colIndex] > 0)
								{
									string item = Convert.ToString(FilterItemOnProperty(Items[e.Index], ColumnNames[colIndex]));

									boundsRect.X = lastRight;
									boundsRect.Width = m_columnWidths[colIndex];
									lastRight = boundsRect.Right;
									e.Graphics.DrawString(item, font, brush, boundsRect, format);

									if (lastRight + 7 >= e.Bounds.Right)
										break;

									if (notDone(loopIncrementer(colIndex)) && !drawUncategorizedItemSpecial)
										e.Graphics.DrawLine(linePen, boundsRect.Right - 1, boundsRect.Top, boundsRect.Right - 1, boundsRect.Bottom);
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
			if (TotalWidth <= 0 || Items.Count == 0)
				return;

			if (DropDownHeight < GetRequiredHeightForFirstNItems(Items.Count))
			{
				// The vertical scrollbar is present. Add its width to the total.
				// If you don't then RightToLeft languages will have a few characters obscured.
				DropDownWidth = TotalWidth + SystemInformation.VerticalScrollBarWidth;
			}
			else
				DropDownWidth = TotalWidth;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// TODO: Handle SHIFT-Left and SHIFT-Right
			switch (keyData)
			{
				case Keys.Left:
					if (SelectionStart != 0)
						SelectionStart = SelectionStart - 1;
					SelectionLength = 0;
					return true;
				case Keys.Right:
					if (SelectionStart + SelectionLength != Text.Length)
						SelectionStart = SelectionStart + SelectionLength + 1;
					SelectionLength = 0;
					return true;
				case Keys.Enter:
				case Keys.Tab:
					m_newText = Text;
					if (DroppedDown)
						DroppedDown = false;
					break;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data.</param>
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			int idx;
			string toFind = Text.Substring(0, SelectionStart) + e.KeyChar + Text.Substring(SelectionStart + SelectionLength);
			int newSelectionStart = SelectionStart + 1;

			if (!Char.IsControl(e.KeyChar))
			{
				idx = FindString(toFind);

				if (idx != -1 && !IsUncategorizedItemSpecial(idx))
				{
					// The string was found
					SelectedIndex = idx;
					SelectionStart = toFind.Length;
					SelectionLength = Text.Length - SelectionStart;
				}
				else
				{
					// Replace any selected text with the key pressed
					SelectedIndex = -1;
					Text = toFind;
					SelectionStart = newSelectionStart;
					Debug.WriteLine("SelectedIndex:" + SelectedIndex);
				}
			}

			int resetSelectionStart = SelectionStart;
			int resetSelectionLength = SelectionLength;

			if (!DroppedDown && e.KeyChar != (int)Keys.Escape && e.KeyChar != (int)Keys.Tab && e.KeyChar != (int)Keys.Enter)
			{
				DroppedDown = AutoDropdown;
				SelectionStart = resetSelectionStart;
				SelectionLength = resetSelectionLength;
			}

			if (e.KeyChar == (char)Keys.Back) // A Backspace Key is hit
				return;

			e.Handled = true;
		}

		#endregion

		#region "Methods"

		/// <summary>
		/// Sets what columns to be displayed and calculates the width to use to display them.
		/// </summary>
		private void InitializeColumns()
		{
			if (OwnerCell == null || DataManager == null)
				return;

			if (ColumnNames.Count == 0)
			{
				var propertyDescriptorCollection = DataManager.GetItemProperties();
				for (var colIndex = 0; colIndex < propertyDescriptorCollection.Count; colIndex++)
					if (propertyDescriptorCollection[colIndex].Name != OwningColumn.CategoryColumnName)
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
			if (OwningColumn.CategoryColumnName != null)
				TotalWidth += IndentSize;
		}

		#endregion

		#region http://stackoverflow.com/questions/1245530/unable-to-set-the-dropdownheight-of-combobox
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left; // x position of upper-left corner
			public int Top; // y position of upper-left corner
			public int Right; // x position of lower-right corner
			public int Bottom; // y position of lower-right corner
		}

		public const int SWP_NOZORDER = 0x0004;
		public const int SWP_NOACTIVATE = 0x0010;
		public const int SWP_FRAMECHANGED = 0x0020;
		public const int SWP_NOOWNERZORDER = 0x0200;

		public const int WM_CTLCOLORLISTBOX = 0x0134;
		public const int WM_LBUTTONDOWN = 0x0201;

		private int _hwndDropDown = 0;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_CTLCOLORLISTBOX)
			{
				if (_hwndDropDown == 0)
				{
					_hwndDropDown = m.LParam.ToInt32();

					RECT r;
					GetWindowRect((IntPtr)_hwndDropDown, out r);

					int newHeight = GetRequiredHeightForFirstNItems(MaxDropDownItems);

					SetWindowPos((IntPtr)_hwndDropDown, IntPtr.Zero,
						r.Left,
						r.Top,
						DropDownWidth,
						newHeight,
						SWP_FRAMECHANGED |
						SWP_NOACTIVATE |
						SWP_NOZORDER |
						SWP_NOOWNERZORDER);
				}
			}
			else if (m.Msg == WM_LBUTTONDOWN)
			{
				Debug.WriteLine("Got left mouse down!!!");
				if (DroppedDown && Bounds.Contains(MousePosition))
					return;
			}

			base.WndProc(ref m);
		}

		protected override void OnDropDownClosed(EventArgs e)
		{
			Debug.WriteLine("In OnDropDownClosed");
			_hwndDropDown = 0;
			if (SelectedIndex != -1 && m_newText == null)
				EditingControlFormattedValue = ((DataRowView)SelectedItem)[OwningColumn.DisplayMember];
			m_newText = null;
			base.OnDropDownClosed(e);
		}
		#endregion
	}
}
