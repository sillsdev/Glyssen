using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GlyssenApp.UI.Controls
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
				m_control.Data = Value;
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
		private object m_dataOriginal;
		private int[] m_selectionSave = new int[0];
		private bool m_restoringSelection;

		public ListBoxEditingControl()
		{
			BorderStyle = BorderStyle.None;
			SelectionMode = SelectionMode.MultiExtended;

			//Necessary in order to enforce ItemHeight
			//See http://stackoverflow.com/questions/15298701/how-to-add-padding-between-items-in-a-listbox
			DrawMode = DrawMode.OwnerDrawFixed;

			IntegralHeight = false;
			ItemHeight = 21;

			MouseMove += HandleMouseMove;
		}

		public object Data
		{
			get { return m_dataOriginal; }
			set
			{
				m_dataOriginal = value;

				//A custom type may be used with a "ToList" method
				var type = value.GetType();
				var method = type.GetMethod("ToList");

				if (method != null)
				{
					DataSource = method.Invoke(value, null);
				}
				else
				{
					var items = value as IEnumerable<string>;
					DataSource = items.ToList();
				}
			}
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
			//.NET appears to just pass in the default cell style for the whole datagridview
			BackColor = Color.White;
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
				//The +1 lines the text up closer to its default (for TextBox)
				e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X + 1, e.Bounds.Y + 1);
			}
		}

		public void OnMouseMove()
		{
			OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, -1, -1, 0));
		}

		private void HandleMouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				List<string> characterIds = SelectedItems.Cast<string>().ToList();
				DoDragDrop(characterIds, DragDropEffects.Move);
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			SaveSelection();
		}

		private void SaveSelection()
		{
			if (!m_restoringSelection && SelectionMode == SelectionMode.MultiExtended)
			{
				SelectedIndexCollection sel = SelectedIndices;
				if (m_selectionSave.Length != sel.Count)
				{
					m_selectionSave = new int[sel.Count];
				}
				SelectedIndices.CopyTo(m_selectionSave, 0);
			}
		}

		private void RestoreSelection(int clickedItemIndex)
		{
			if (SelectionMode == SelectionMode.MultiExtended &&
				Control.ModifierKeys == Keys.None &&
				Array.IndexOf(m_selectionSave, clickedItemIndex) >= 0)
			{

				m_restoringSelection = true;
				foreach (int i in m_selectionSave)
				{
					SetSelected(i, true);
				}
				SetSelected(clickedItemIndex, true);
				m_restoringSelection = false;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			int clickedItemIndex = IndexFromPoint(e.Location);
			if (clickedItemIndex >= 0 && MouseButtons == MouseButtons.Left &&
				(GetSelected(clickedItemIndex) || Control.ModifierKeys == Keys.Shift))
			{
				RestoreSelection(clickedItemIndex);
			}
		}
	}

	#endregion
}
