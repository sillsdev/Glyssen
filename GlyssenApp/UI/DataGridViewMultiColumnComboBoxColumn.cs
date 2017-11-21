using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace GlyssenApp.UI
{
	/// <summary>
	/// Represents a column of <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxColumn"/> objects.
	/// http://www.intertech.com/Blog/winforms-multi-column-combo-box-in-a-datagridview/
	/// </summary>
	/// <filterpriority>2</filterpriority>
	[ToolboxBitmap(typeof(DataGridViewComboBoxColumn), "DataGridViewComboBoxColumn.bmp")]
	[Designer("Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class DataGridViewMultiColumnComboBoxColumn : DataGridViewComboBoxColumn
	{
		#region "Constructor"

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxColumn"/> class to the default state.
		/// </summary>
		public DataGridViewMultiColumnComboBoxColumn()
		{
			CellTemplate = new DataGridViewMultiColumnComboBoxCell();
		}

		#endregion

		#region "Properties"

		/// <summary>
		/// Gets or sets the template used to create cells.
		/// </summary>
		///
		/// <returns>
		/// A <see cref="T:System.Windows.Forms.DataGridViewCell"/> that all other cells in the column are modeled after. The default value is a new <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxCell"/>.
		/// </returns>
		/// <exception cref="T:System.InvalidCastException">When setting this property to a value that is not of type <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxCell"/>. </exception><filterpriority>1</filterpriority>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override sealed DataGridViewCell CellTemplate
		{
			get { return base.CellTemplate; }
			set
			{
				// Ensure that the cell used for the template is a DataGridViewMultiColumnComboBoxCell.
				if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewMultiColumnComboBoxCell)))
				{
					throw new InvalidCastException("Must be a DataGridViewMultiColumnComboBoxCell");
				}
				base.CellTemplate = value;
			}
		}

		[Category("Design")]
		[DefaultValue("")]
		[Browsable(true)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Bindable(true)]
		[ParenthesizePropertyName(true)]
		[Description("Indicates the name used in code to identify the object.")]
		public new string Name
		{
			get { return base.Name; }
			set { base.Name = value; }
		}

		/// <summary>
		/// Replicates the ColumnNames property of the DataGridViewMultiColumnComboBoxCell cell type.
		/// </summary>
		[Category("Data")]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Which columns to show. Leave blank to show all. put entries in [] to rename Column Headers.")]
		public List<String> ColumnNames
		{
			get
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				return MultiColumnComboBoxCellTemplate.ColumnNames;
			}
			set
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				// Update the template cell so that subsequent cloned cells use the new value.
				MultiColumnComboBoxCellTemplate.ColumnNames = value;
				if (DataGridView == null)
					return;

				// Update all the existing DataGridViewMultiColumnComboBoxCell cells in the column accordingly.
				var dataGridViewRows = DataGridView.Rows;
				var rowCount = dataGridViewRows.Count;
				for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
				{
					// Be careful not to unshare rows unnecessarily.
					// This could have severe performance repercussions.
					var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
					var dataGridViewCell = dataGridViewRow.Cells[Index] as DataGridViewMultiColumnComboBoxCell;
					if (dataGridViewCell != null)
					{
						// Call the internal SetColumnNames method instead of the property to avoid invalidation
						// of each cell. The whole column is invalidated later in a single operation for better performance.
						dataGridViewCell.SetColumnNames(rowIndex, value);
					}
				}
				DataGridView.InvalidateColumn(Index);
				// TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
			}
		}

		/// <summary>
		/// Replicates the ColumnWidths property of the DataGridViewMultiColumnComboBoxCell cell type.
		/// </summary>
		[Category("Data")]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Width of each column. Leave blank to use the defualt. Put entries in [] to rename Column Headers.")]
		public List<string> ColumnWidths
		{
			get
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				return MultiColumnComboBoxCellTemplate.ColumnWidths;
			}
			set
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}

				// Update the template cell so that subsequent cloned cells use the new value.
				MultiColumnComboBoxCellTemplate.ColumnWidths = value;
				if (DataGridView == null)
					return;

				// Update all the existing DataGridViewMultiColumnComboBoxCell cells in the column accordingly.
				var dataGridViewRows = DataGridView.Rows;
				var rowCount = dataGridViewRows.Count;
				for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
				{
					// Be careful not to unshare rows unnecessarily.
					// This could have severe performance repercussions.
					var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
					var dataGridViewCell = dataGridViewRow.Cells[Index] as DataGridViewMultiColumnComboBoxCell;
					if (dataGridViewCell != null)
					{
						// Call the internal SetColumnWidths method instead of the property to avoid invalidation
						// of each cell. The whole column is invalidated later in a single operation for better performance.
						dataGridViewCell.SetColumnWidths(rowIndex, value);
					}
				}
				DataGridView.InvalidateColumn(Index);
				// TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
			}
		}

		/// <summary>
		/// Replicates the EvenRowsBackColor property of the DataGridViewMultiColumnComboBoxCell cell type.
		/// </summary>
		///
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the background color of the even rows of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor"/> property.
		/// </returns>
		[Category("Appearance")]
		[DefaultValue(typeof(SystemColors), "System.Drawing.SystemColors.Control")]
		[Description("The background color for the even rows.")]
		public Color EvenRowsBackColor
		{
			get
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				return MultiColumnComboBoxCellTemplate.EvenRowsBackColor;
			}
			set
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				// Update the template cell so that subsequent cloned cells use the new value.
				MultiColumnComboBoxCellTemplate.EvenRowsBackColor = value;
				if (DataGridView == null)
					return;

				// Update all the existing DataGridViewMultiColumnComboBoxCell cells in the column accordingly.
				var dataGridViewRows = DataGridView.Rows;
				var rowCount = dataGridViewRows.Count;
				for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
				{
					// Be careful not to unshare rows unnecessarily.
					// This could have severe performance repercussions.
					var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
					var dataGridViewCell = dataGridViewRow.Cells[Index] as DataGridViewMultiColumnComboBoxCell;
					if (dataGridViewCell != null)
					{
						// Call the internal SetEvenRowsBackColor method instead of the property to avoid invalidation
						// of each cell. The whole column is invalidated later in a single operation for better performance.
						dataGridViewCell.SetEvenRowsBackColor(rowIndex, value);
					}
				}
				DataGridView.InvalidateColumn(Index);
				// TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
			}
		}

		/// <summary>
		/// Replicates the OddRowsBackColor property of the DataGridViewMultiColumnComboBoxCell cell type.
		/// </summary>
		///
		/// <returns>
		/// A <see cref="T:System.Drawing.Color"/> that represents the background color of the odd rows of the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultBackColor"/> property.
		/// </returns>
		[Category("Appearance")]
		[DefaultValue(typeof(SystemColors), "System.Drawing.SystemColors.Control")]
		[Description("The background color for the odd rows.")]
		public Color OddRowsBackColor
		{
			get
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				return MultiColumnComboBoxCellTemplate.OddRowsBackColor;
			}
			set
			{
				if (MultiColumnComboBoxCellTemplate == null)
				{
					throw new InvalidOperationException("Operation cannot be completed because this DataGridViewColumn does not have a CellTemplate.");
				}
				// Update the template cell so that subsequent cloned cells use the new value.
				MultiColumnComboBoxCellTemplate.OddRowsBackColor = value;
				if (DataGridView == null)
					return;

				// Update all the existing DataGridViewMultiColumnComboBoxCell cells in the column accordingly.
				var dataGridViewRows = DataGridView.Rows;
				var rowCount = dataGridViewRows.Count;
				for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
				{
					// Be careful not to unshare rows unnecessarily.
					// This could have severe performance repercussions.
					var dataGridViewRow = dataGridViewRows.SharedRow(rowIndex);
					var dataGridViewCell = dataGridViewRow.Cells[Index] as DataGridViewMultiColumnComboBoxCell;
					if (dataGridViewCell != null)
					{
						// Call the internal SetOddRowsBackColor method instead of the property to avoid invalidation
						// of each cell. The whole column is invalidated later in a single operation for better performance.
						dataGridViewCell.SetOddRowsBackColor(rowIndex, value);
					}
				}
				DataGridView.InvalidateColumn(Index);
				// TODO: Call the grid's autosizing methods to autosize the column, rows, column headers / row headers as needed.
			}
		}

		/// <summary>
		/// Small utility function that returns the template cell as a DataGridViewMultiColumnComboBoxCell
		/// </summary>
		private DataGridViewMultiColumnComboBoxCell MultiColumnComboBoxCellTemplate
		{
			get { return (DataGridViewMultiColumnComboBoxCell)CellTemplate; }
		}

		[Category("Data")]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Value indicating the column name in the data source (which must be a Data Table) which specifies the category name to be used for grouping items of the same category.")]
		public string CategoryColumnName { get; set; }

		[Category("Appearance")]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Font used to draw any uncategorized items in the combo box (ignored if CategoryColumnName is not set).")]
		public Font FontForUncategorizedItems { get; set; }

		[Category("Appearance")]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Font used to draw category labels in the combo box (ignored if CategoryColumnName is not set).")]
		public Font FontForCategories { get; set; }

		#endregion

		#region Events
		public delegate Image GetImageForCellEventHandler(DataGridViewMultiColumnComboBoxColumn sender, int rowIndex);

		public event GetImageForCellEventHandler GetSpecialDropDownImageToDraw;
		#endregion

		#region "Methods"

		/// <summary>
		/// Creates an exact copy of this column.
		/// </summary>
		///
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the cloned <see cref="T:GlyssenApp.UI.DataGridViewMultiColumnComboBoxColumn"/>.
		/// </returns>
		public override object Clone()
		{
			var clone = (DataGridViewMultiColumnComboBoxColumn)base.Clone();
			if (clone == null)
				return null;

			clone.ColumnNames = ColumnNames;
			clone.ColumnWidths = ColumnWidths;
			clone.EvenRowsBackColor = EvenRowsBackColor;
			clone.OddRowsBackColor = OddRowsBackColor;
			clone.MaxDropDownItems = MaxDropDownItems;
			clone.CategoryColumnName = CategoryColumnName;
			clone.FontForUncategorizedItems = FontForUncategorizedItems;
			clone.FontForCategories = FontForCategories;
			clone.GetSpecialDropDownImageToDraw = GetSpecialDropDownImageToDraw;
			return clone;
		}

		/// <returns>
		/// A <see cref="T:System.String"/> that describes the column.
		/// </returns>
		public override string ToString()
		{
			var sb = new StringBuilder(100);
			sb.Append("DataGridViewMultiColumnComboBoxColumn { Name=");
			sb.Append(Name);
			sb.Append(", Index=");
			sb.Append(Index.ToString(CultureInfo.CurrentCulture));
			sb.Append(" }");
			return sb.ToString();
		}

		public Image GetSpecialImageToDraw(int rowIndex)
		{
			return GetSpecialDropDownImageToDraw != null ? GetSpecialDropDownImageToDraw(this, rowIndex) : null;
		}
		#endregion
	}
}
