using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Glyssen.Controls
{
	//Modified from example at https://msdn.microsoft.com/en-us/library/7tas5c80(v=vs.110).aspx

	#region DataGridViewListBoxColumn Definition

	public class DataGridViewListBoxColumn : DataGridViewColumn
	{
		public DataGridViewListBoxColumn() : base(new DataGridViewListBoxCell())
		{
		}

		public override DataGridViewCell CellTemplate
		{
			get
			{
				return base.CellTemplate;
			}
			set
			{
				if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewListBoxCell)))
				{
					throw new InvalidCastException("Must be of type DataGridViewListBoxCell");
				}
				base.CellTemplate = value;
			}
		}
	}

	#endregion

	#region DataGridViewListBoxCell Definition

	public class DataGridViewListBoxCell : DataGridViewTextBoxCell
	{
		private ListBoxEditingControl m_control;

		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

			ListBoxEditingControl ctl = DataGridView.EditingControl as ListBoxEditingControl;
			m_control = ctl;

			if (Value != null)
			{
				//A custom type may be used with a "ToList" method
				var type = Value.GetType();
				var method = type.GetMethod("ToList");

				if (method != null)
				{
					m_control.DataSource = method.Invoke(Value, null);
				}
				else
				{
					var items = Value as IEnumerable<string>;
					m_control.DataSource = items.ToList();
				}
			}
		}

		public override Type EditType
		{
			get
			{
				return typeof(ListBoxEditingControl);
			}
		}
	}

	#endregion

	#region ListBoxEditingControl Definition

	class ListBoxEditingControl : ListBox, IDataGridViewEditingControl
	{
		public ListBoxEditingControl()
		{
			BorderStyle = BorderStyle.None;

			//Necessary in order to enforce ItemHeight
			//See http://stackoverflow.com/questions/15298701/how-to-add-padding-between-items-in-a-listbox
			DrawMode = DrawMode.OwnerDrawFixed;

			IntegralHeight = false;
			ItemHeight = 21;
		}

		#region IDataGridViewEditingControl Implementations

		public object EditingControlFormattedValue { get; set; }

		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			ForeColor = dataGridViewCellStyle.ForeColor;
			BackColor = dataGridViewCellStyle.BackColor;
		}

		public int EditingControlRowIndex { get; set; }

		public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
		{
			switch (key & Keys.KeyCode)
			{
				case Keys.Up:
				case Keys.Down:
					return true;
				default:
					return !dataGridViewWantsInputKey;
			}
		}

		public void PrepareEditingControlForEdit(bool selectAll)
		{
		}

		public bool RepositionEditingControlOnValueChange
		{
			get
			{
				return true;
			}
		}

		public DataGridView EditingControlDataGridView { get; set; }

		public bool EditingControlValueChanged { get; set; }

		public Cursor EditingPanelCursor
		{
			get
			{
				return Cursor;
			}
		}

		#endregion

		//A similar override was in the microsoft's example code, but this currently raises a type error 
		//because the DataSource is a List while the higher-level source may actually be a different type (e.g. HashSet)
		//protected override void OnSelectedValueChanged(EventArgs eventargs)
		//{
		//	EditingControlValueChanged = true;
		//	EditingControlDataGridView.NotifyCurrentCellDirty(true);
		//	base.OnSelectedValueChanged(eventargs);
		//}

		//Necessary if using DrawMode = DrawMode.OwnerDrawFixed
		//Taken from http://stackoverflow.com/questions/15298701/how-to-add-padding-between-items-in-a-listbox
		//There is currently an issue with an extra row showing, and scrolling down by default
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.State == DrawItemState.Focus)
				e.DrawFocusRectangle();
			var index = e.Index;
			if (index < 0 || index >= Items.Count) return;
			var item = Items[index];
			string text = (item == null) ? "(null)" : item.ToString();
			using (var brush = new SolidBrush(e.ForeColor))
			{
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				e.Graphics.DrawString(text, e.Font, brush, e.Bounds);
			}
		}
	}

	#endregion
}
