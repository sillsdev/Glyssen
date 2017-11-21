using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace GlyssenApp.UI
{
	/// <summary>
	/// Displays a DataGridViewMultiColumnComboBoxEditingControl in a <see cref="T:System.Windows.Forms.DataGridView"/> control.
	/// http://www.intertech.com/Blog/winforms-multi-column-combo-box-in-a-datagridview/
	/// </summary>
	public class DataGridViewMultiColumnComboBoxCell : DataGridViewComboBoxCell
	{
		#region "Member Variables"

		private List<string> m_columnNames = new List<string>();
		private List<string> m_columnWidths = new List<string>();
		private Color m_evenRowsBackColor = SystemColors.Control;
		private Color m_oddRowsBackColor = SystemColors.Control;

		// Constants
		private const String EvenRowsBackColorErrorMsg = "The EvenRowsBackColor property cannot be null.";
		private const String OddRowsBackColorErrorMsg = "The OddRowsBackColor property cannot be null.";

		// Type of this cell's editing control
		private static readonly Type DefaultEditType = typeof(DataGridViewMultiColumnComboBoxEditingControl);


		#endregion

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			var column = (DataGridViewMultiColumnComboBoxColumn)OwningColumn;
			var dataTable = column.DataSource as DataTable;
			if (column.CategoryColumnName != null && dataTable != null)
			{
				var valueMember = column.ValueMember;
				if (!String.IsNullOrEmpty(valueMember) && dataTable.Rows.Count > 0)
				{
					for (int i = 0; i < dataTable.Rows.Count; i++)
					{
						var row = dataTable.Rows[i];
						if (row[valueMember].Equals(value) && row[column.CategoryColumnName] is DBNull)
						{
							base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, String.Empty, errorText, cellStyle, advancedBorderStyle, paintParts);
							return;
						}
					}
				}
			}
			Image image = column.GetSpecialImageToDraw(rowIndex);
			if (image == null)
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			else
			{
				paintParts ^= DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.Focus;
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, String.Empty, errorText, cellStyle, advancedBorderStyle, paintParts);

				cellBounds.Inflate(-1, -1); // This is a hack - it will only work if borders are 1 pixel.
				Color foreColor = ((elementState & DataGridViewElementStates.Selected) > 0) ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
				using (var foregroundBrush = new SolidBrush(foreColor))
					graphics.DrawString(formattedValue.ToString(), cellStyle.Font, foregroundBrush, cellBounds);

				var height = image.Height;
				var width = image.Width;
				int top = cellBounds.Y + 2;
				var lockImageRect = new Rectangle(cellBounds.Right - width - 2, top, width, height);
				graphics.DrawImage(image, lockImageRect);
			}
		}

		#region "Properties"

		/// <summary>
		/// Define the type of the cell's editing control
		/// </summary>
		/// <returns>A Type of <see cref="DataGridViewMultiColumnComboBoxEditingControl"/>.</returns>
		public override Type EditType
		{
			get { return DefaultEditType; }
		}

		/// <summary>
		/// The ColumnNames property replicates the one from the DataGridViewMultiColumnComboBoxEditingControl control
		/// </summary>
		/// <exception cref="T:System.ArgumentNullException">When property is null.</exception>
		public List<String> ColumnNames
		{
			get { return m_columnNames; }
			set { m_columnNames = value ?? new List<string>(); }
		}

		/// <summary>
		/// The ColumnWidths property replicates the one from the DataGridViewMultiColumnComboBoxEditingControl control
		/// </summary>
		/// <exception cref="T:System.ArgumentNullException">When property is null.</exception>
		public List<String> ColumnWidths
		{
			get { return m_columnWidths; }
			set { m_columnWidths = value ?? new List<string>(); }
		}

		/// <summary>
		/// Gets or sets the background color for the even rows portion of the DataGridViewMultiColumnComboBoxEditingControl control.  The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor"/> property.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the background color of the even rows portion of the DataGridViewMultiColumnComboBoxEditingControl.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">When property is null.</exception>
		public Color EvenRowsBackColor
		{
			get { return m_evenRowsBackColor; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(EvenRowsBackColorErrorMsg);
				}

				m_evenRowsBackColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the background color for the odd rows portion of the DataGridViewMultiColumnComboBoxEditingControl control.  The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor"/> property.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the background color of the odd rows portion of the DataGridViewMultiColumnComboBoxEditingControl.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">When property is null.</exception>
		public Color OddRowsBackColor
		{
			get { return m_oddRowsBackColor; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(OddRowsBackColorErrorMsg);
				}

				m_oddRowsBackColor = value;
			}
		}

		#endregion

		#region "Methods"
		/// <summary>
		/// Creates an exact copy of this cell, copies all the custom properties.
		/// </summary>
		///
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the cloned <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxCell"/>.
		/// </returns>
		public override object Clone()
		{
			var clone = (DataGridViewMultiColumnComboBoxCell)base.Clone();

			// Make sure to copy added properties.
			clone.ColumnNames = ColumnNames;
			clone.ColumnWidths = ColumnWidths;
			clone.EvenRowsBackColor = EvenRowsBackColor;
			clone.OddRowsBackColor = OddRowsBackColor;

			return clone;
		}

		/// <summary>
		/// Custom implementation of the InitializeEditingControl function. This function is called by the DataGridView control
		/// at the beginning of an editing session. It makes sure that the properties of the DataGridViewMultiColumnComboBoxEditingControl editing control are
		/// set according to the cell properties.
		/// </summary>
		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

			var editingControl = DataGridView.EditingControl as DataGridViewMultiColumnComboBoxEditingControl;
			// Just return if editing control is null.
			if (editingControl == null)
				return;

			// Set custom properties of Multi Column Combo Box.
			editingControl.ColumnNames = ColumnNames;
			editingControl.ColumnWidths = ColumnWidths;
			editingControl.BackColorEven = EvenRowsBackColor;
			editingControl.BackColorOdd = OddRowsBackColor;
			editingControl.MaxDropDownItems = MaxDropDownItems;
			editingControl.OwnerCell = this;

			if (Value != null)
				editingControl.SelectedValue = Value;

			editingControl.AutoComplete = AutoComplete;
			if (!AutoComplete)
				return;

			editingControl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			editingControl.AutoCompleteSource = AutoCompleteSource.ListItems;
		}

		/// <summary>
		/// Returns a standard textual representation of the cell.
		/// </summary>
		public override string ToString()
		{
			return string.Format("DataGridViewMultiColumnComboBoxCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex.ToString(CultureInfo.CurrentCulture), RowIndex.ToString(CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Utility function that sets a new value for the ColumnNames property of the cell. This function is used by
		/// the cell and column ColumnNames property. The column uses this method instead of the ColumnNames
		/// property for performance reasons. This way the column can invalidate the entire column at once instead of
		/// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
		/// this cell may be shared among multiple rows.
		/// </summary>
		internal void SetColumnNames(int rowIndex, List<string> value)
		{
			Debug.Assert(value != null);
			m_columnNames = value;
			if (OwnsEditingMultiColumnComboBox(rowIndex))
			{
				EditingMultiColumnComboBox.ColumnNames = value;
			}
		}

		/// <summary>
		/// Utility function that sets a new value for the ColumnWidths property of the cell. This function is used by
		/// the cell and column ColumnWidths property. The column uses this method instead of the ColumnWidths
		/// property for performance reasons. This way the column can invalidate the entire column at once instead of
		/// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
		/// this cell may be shared among multiple rows.
		/// </summary>
		internal void SetColumnWidths(int rowIndex, List<string> value)
		{
			Debug.Assert(value != null);
			m_columnWidths = value;
			if (OwnsEditingMultiColumnComboBox(rowIndex))
			{
				EditingMultiColumnComboBox.ColumnWidths = value;
			}
		}

		/// <summary>
		/// Utility function that sets a new value for the EvenRowsBackColor property of the cell. This function is used by
		/// the cell and column EvenRowsBackColor property. The column uses this method instead of the EvenRowsBackColor
		/// property for performance reasons. This way the column can invalidate the entire column at once instead of
		/// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
		/// this cell may be shared among multiple rows.
		/// </summary>
		internal void SetEvenRowsBackColor(int rowIndex, Color value)
		{
			Debug.Assert(value != null);
			m_evenRowsBackColor = value;
			if (OwnsEditingMultiColumnComboBox(rowIndex))
			{
				EditingMultiColumnComboBox.BackColorEven = value;
			}
		}

		/// <summary>
		/// Utility function that sets a new value for the OddRowsBackColor property of the cell. This function is used by
		/// the cell and column OddRowsBackColor property. The column uses this method instead of the OddRowsBackColor
		/// property for performance reasons. This way the column can invalidate the entire column at once instead of
		/// invalidating each cell of the column individually. A row index needs to be provided as a parameter because
		/// this cell may be shared among multiple rows.
		/// </summary>
		internal void SetOddRowsBackColor(int rowIndex, Color value)
		{
			Debug.Assert(value != null);
			m_oddRowsBackColor = value;
			if (OwnsEditingMultiColumnComboBox(rowIndex))
			{
				EditingMultiColumnComboBox.BackColorOdd = value;
			}
		}

		/// <summary>
		/// Determines whether this cell, at the given row index, shows the grid's editing control or not.
		/// The row index needs to be provided as a parameter because this cell may be shared among multiple rows.
		/// </summary>
		private bool OwnsEditingMultiColumnComboBox(int rowIndex)
		{
			if (rowIndex == -1 || DataGridView == null)
			{
				return false;
			}
			var editingControl = DataGridView.EditingControl as DataGridViewMultiColumnComboBoxEditingControl;
			return editingControl != null && rowIndex == ((IDataGridViewEditingControl)editingControl).EditingControlRowIndex;
		}

		/// <summary>
		/// Returns the current DataGridView EditingControl as a DataGridViewMultiColumnComboBoxEditingControl control
		/// </summary>
		private DataGridViewMultiColumnComboBoxEditingControl EditingMultiColumnComboBox
		{
			get { return DataGridView.EditingControl as DataGridViewMultiColumnComboBoxEditingControl; }
		}

		#endregion

	}
}
