using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace Glyssen.Controls
{
	//Auto-sizing partly taken from http://stackoverflow.com/questions/2122985/removing-the-empty-gray-space-in-datagrid-in-c-sharp
	//Auto-scrolling partly taken from http://stackoverflow.com/questions/2567809/how-to-autoscroll-a-datagridview-during-drag-and-drop

	/// <summary>
	/// Inherited from BetterGrid (DataGridView), this grid is intended for two purposes:
	/// <para>1) The height of an AutoGrid is only as big as it has rows, but its height will max out at the point where its bottom alignment matches its bottom alignment as set in the Designer.</para>
	/// <para>The primary use case of this feature is to allow controls to float underneath a DataGridView's last row.</para>
	/// <para>To use this feature, put the AutoGrid into a TableLayoutPanel in a row with Autosizing. Size the AutoGrid inside to whatever maximum size it may grow to.</para>
	/// <para>2) AutoGrid will auto-scroll when dragging at the top or bottom.</para>
	/// </summary>
	public class AutoGrid : DataGridViewOverrideEnter
	{
		public delegate void RowDroppedEventHangler(object source, DataGridViewRow sourceRow, DataGridViewRow destinationRow);

		private bool m_parentInitSized;
		private Control m_parentResizeListen;
		private bool m_scrolling;
		private System.Threading.Timer m_scrollTimer;

		public AutoGrid()
		{
			m_parentInitSized = false;

			ParentLayersAffected = 0;
		}

		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public int MinBottomMargin { get; private set; }

		private int MaxHeight
		{
			get
			{
				if (ParentWithIndependentSize == null)
					return 100;
				return ParentWithIndependentSize.Height - MinBottomMargin - LastAffected.Top;
			}
		}

		/// <summary>
		/// Number of ancestors to resize with grid
		/// </summary>
		public int ParentLayersAffected { get; set; }

		private Control ParentWithIndependentSize
		{
			get
			{
				Control cParent = base.Parent;
				int layer = 1;
				while (cParent != null && layer <= ParentLayersAffected)
				{
					cParent = cParent.Parent;
					layer++;
				}
				return cParent;
			}
		}

		private Control LastAffected
		{
			get
			{
				Control cParent = this;
				int layer = 0;
				while (cParent.Parent != null && layer < ParentLayersAffected)
				{
					cParent = cParent.Parent;
					layer++;
				}
				return cParent;
			}	
		}

		private Control ParentResizeListen
		{
			get
			{
				return m_parentResizeListen;
			}
			set
			{
				if (m_parentResizeListen != null)
					m_parentResizeListen.ClientSizeChanged -= HandleParentResize;

				m_parentInitSized = false;
				m_parentResizeListen = value;
				if (m_parentResizeListen != null)
					m_parentResizeListen.ClientSizeChanged += HandleParentResize;
			}
		}

		protected void OnParentChanging()
		{

		}

		protected override void OnParentChanged(EventArgs e)
		{
			ParentResizeListen = null;
			if (ParentWithIndependentSize == null)
			{
				if (LastAffected != this)
					LastAffected.ParentChanged += HandleParentChanged;
			}
			else
			{
				MinBottomMargin = ParentWithIndependentSize.Height - LastAffected.Bottom;

				ParentResizeListen = ParentWithIndependentSize;
			}
			base.OnParentChanged(e);
		}

		private void HandleParentChanged(object sender, EventArgs e)
		{
			var someControl = sender as Control;

			ParentResizeListen = null;

			OnParentChanged(e);
		}

		private void HandleParentResize(object sender, EventArgs e)
		{
			if (!m_parentInitSized)
			{
				MinBottomMargin = ParentWithIndependentSize.Height - LastAffected.Bottom;

				m_parentInitSized = true;
			}

			SetGridHeight();
		}

		protected override void OnRowHeightChanged(DataGridViewRowEventArgs e)
		{
			SetGridHeight();
			base.OnRowHeightChanged(e);
		}

		protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
		{
			SetGridHeight();
			base.OnRowsAdded(e);
		}

		protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
		{
			SetGridHeight();
			base.OnRowsRemoved(e);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			BeginInvoke(new MethodInvoker(SetGridHeight));
			base.OnHandleCreated(e);
		}

		private void SetGridHeight()
		{
			//Prevents flicker
			Control cParent = Parent;
			while (cParent != null && cParent != ParentWithIndependentSize)
			{
				//cParent.SuspendLayout();
				cParent = cParent.Parent;
			}
			if (ParentWithIndependentSize != null)
				ParentWithIndependentSize.SuspendLayout();

			if (DesignMode) 
				return;
			int height = ColumnHeadersHeight + 2;
			if (HorizontalScrollBar.Visible) 
				height += SystemInformation.HorizontalScrollBarHeight;
			for (int row = 0; row < RowCount; ++row)
			{
				height = Math.Min(MaxHeight, height + Rows[row].Height);
				if (height >= MaxHeight) 
					break;
			}

			var newSize = new Size(ClientSize.Width, height);

			int dy = height - ClientSize.Height;
			ClientSize = newSize;

			//Refresh();

			PerformLayout();
//			Size = newSize;

			cParent = Parent;
			int layer = 1;
			while (cParent != null && layer <= ParentLayersAffected)
			{
//				cParent.ClientSize = new Size(cParent.ClientSize.Width, cParent.ClientSize.Height + dy);
				cParent.ClientSize = newSize;
				//cParent.Refresh();
				cParent.PerformLayout();
//				cParent.Size = newSize;
				cParent = cParent.Parent;
				layer++;
			}

			//if (height < MaxHeight && RowCount > 1) 
			//	FirstDisplayedScrollingRowIndex = 0;

			if (ClientSize.Height < 100)
			{
				Debug.WriteLine("too short");
			}
			if (Parent != null && Parent.ClientSize.Height < 100)
			{
				Debug.WriteLine("too short");
			}

			cParent = Parent;
			while (cParent != null && cParent != ParentWithIndependentSize)
			{
				//cParent.ResumeLayout();
				cParent = cParent.Parent;
			}
			if (ParentWithIndependentSize != null)
			{
				//ParentWithIndependentSize.PerformLayout();
				ParentWithIndependentSize.ResumeLayout();
			}

			if (ClientSize.Height < 100)
			{
				Debug.WriteLine("too short");
			}
			if (Parent != null && Parent.ClientSize.Height < 100)
			{
				Debug.WriteLine("too short");
			}
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			Point p = PointToClient(new Point(drgevent.X, drgevent.Y));

			if (m_scrollTimer == null && ShouldScrollDown(p))
			{
				m_scrollTimer = new System.Threading.Timer(TimerScroll, 1, 0, 250);
			}
			if (m_scrollTimer == null && ShouldScrollUp(p))
			{
				m_scrollTimer = new System.Threading.Timer(TimerScroll, -1, 0, 250);
			}
			if (!(ShouldScrollUp(p) || ShouldScrollDown(p)))
			{
				StopAutoScrolling();
			}

			base.OnDragOver(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			StopAutoScrolling();
			base.OnDragDrop(drgevent);
		}

		private bool ShouldScrollUp(Point location)
		{
			return location.Y > ColumnHeadersHeight
					&& location.Y < ColumnHeadersHeight + 15
					&& location.X >= 0
					&& location.X <= Bounds.Width;
		}

		private bool ShouldScrollDown(Point location)
		{
			return location.Y > Bounds.Height - 15
					&& location.Y < Bounds.Height
					&& location.X >= 0
					&& location.X <= Bounds.Width;
		}

		private void StopAutoScrolling()
		{
			if (m_scrollTimer != null)
			{
				m_scrollTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				m_scrollTimer = null;
			}
		}

		private void TimerScroll(object state)
		{
			SetScrollBar((int) state);
		}

		private void SetScrollBar(int direction)
		{
			if (m_scrolling)
			{
				return;
			}
			if (InvokeRequired)
			{
				Invoke(new Action<int>(SetScrollBar), direction);
			}
			else
			{
				m_scrolling = true;

				if (0 < direction)
				{
					if (FirstDisplayedScrollingRowIndex < Rows.Count - 1)
					{
						FirstDisplayedScrollingRowIndex++;
					}
				}
				else
				{
					if (FirstDisplayedScrollingRowIndex > 0)
					{
						FirstDisplayedScrollingRowIndex--;
					}
				}

				m_scrolling = false;
			}
		}
	}
}