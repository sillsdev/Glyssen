using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Glyssen.Controls
{
	//Auto-sizing partly taken from http://stackoverflow.com/questions/2122985/removing-the-empty-gray-space-in-datagrid-in-c-sharp
	//Auto-scrolling partly taken from http://stackoverflow.com/questions/2567809/how-to-autoscroll-a-datagridview-during-drag-and-drop

	/// <summary>
	/// Inherited from BetterGrid (DataGridView), this grid is intended for two purposes:
	/// <para>1) The height of an AutoGrid is only as big as it has rows, but its height will max out at the point where its bottom alignment matches its bottom alignment as set in the Designer.</para>
	/// <para>The primary use case of this feature is to allow controls to float underneath a DataGridView's last row.</para>
	/// <para>To use this feature, put the AutoGrid into a TableLayoutPanel in a row with Autosizing. Size the AutoGrid inside to whatever maximum size it may grow to.</para>
	/// <para>If the AutoGrid is nested within a UserControl, anchor it in the UserControl and specify the ParentLayersAffected (i.e. the number of layers between the AutoGrid and the TableLayoutPanel).</para>
	/// <para>2) AutoGrid will auto-scroll when dragging at the top or bottom.</para>
	/// </summary>
	public class AutoGrid : DataGridViewOverrideEnter
	{
		public AutoGrid()
		{
			m_independentParentInitSized = false;
			m_lastAffectedInitSized = false;
			m_initSized = false;

			ParentLayersAffected = 0;
		}

		#region AutoSize

		private bool m_independentParentInitSized;
		private bool m_lastAffectedInitSized;
		private bool m_initSized;
		private int m_initHeight;
		private int m_parentExtraHeight;
		private Control m_parentWithIndependentSize;
		private Control m_lastAffectedParent;
		private bool m_setGridHeightActive;
		private int m_minBottomMargin;

		private int MinBottomMargin
		{
			get
			{
				return m_minBottomMargin;
			}
			set
			{
				m_setGridHeightActive = true;
				m_minBottomMargin = value;
			}
		}

		private int MaxHeight
		{
			get
			{
				if (m_parentWithIndependentSize == null)
					return -1;
				return m_parentWithIndependentSize.Height - MinBottomMargin - m_lastAffectedParent.Top;
			}
		}

		/// <summary>
		/// Number of ancestors to resize with grid
		/// </summary>
		public int ParentLayersAffected { get; set; }

		protected override void OnHandleCreated(EventArgs e)
		{
			OnParentChanged(e);
			BeginInvoke(new MethodInvoker(SetGridHeight));
			base.OnHandleCreated(e);
		}

		protected override void OnClientSizeChanged(EventArgs e)
		{
			if (!m_initSized)
			{
				m_initHeight = Height;
				m_initSized = true;
			}
			base.OnClientSizeChanged(e);
		}

		protected override void OnParentChanged(EventArgs e)
		{
			if (m_parentWithIndependentSize != null)
				m_parentWithIndependentSize.ClientSizeChanged -= HandleIndependentParentResize;
			m_independentParentInitSized = false;

			m_parentWithIndependentSize = Parent;
			int layer = 1;
			while (m_parentWithIndependentSize != null && layer <= ParentLayersAffected)
			{
				m_parentWithIndependentSize = m_parentWithIndependentSize.Parent;
				layer++;
			}

			if (m_parentWithIndependentSize == null)
			{
				if (m_lastAffectedParent != null)
					m_lastAffectedParent.ClientSizeChanged -= HandleLastAffectedResize;
				m_lastAffectedInitSized = false;

				m_lastAffectedParent = this;
				layer = 0;
				while (m_lastAffectedParent.Parent != null && layer < ParentLayersAffected)
				{
					m_lastAffectedParent = m_lastAffectedParent.Parent;
					layer++;
				}

				if (m_lastAffectedParent != this)
				{
					m_lastAffectedParent.ParentChanged += HandleParentChanged;
					m_lastAffectedParent.ClientSizeChanged += HandleLastAffectedResize;
				}
			}
			else
			{
				m_initHeight = Height;
				m_parentWithIndependentSize.ClientSizeChanged += HandleIndependentParentResize;
			}
			base.OnParentChanged(e);
		}

		private void HandleParentChanged(object sender, EventArgs e)
		{
			OnParentChanged(e);
		}

		private void HandleLastAffectedResize(object sender, EventArgs e)
		{
			if (!m_lastAffectedInitSized)
			{
				m_parentExtraHeight = m_lastAffectedParent.Height - m_initHeight;
				m_lastAffectedInitSized = true;
			}
		}

		private void HandleIndependentParentResize(object sender, EventArgs e)
		{
			Debug.Assert(sender == m_parentWithIndependentSize);
			if (!m_independentParentInitSized)
			{
				MinBottomMargin = m_parentWithIndependentSize.Height - m_lastAffectedParent.Bottom;
				m_independentParentInitSized = true;
			}

			SetGridHeight();
		}

		#region Standard Events Trigger SetGridHeight
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
		#endregion

		private void SetGridHeight()
		{
			if (!m_setGridHeightActive || !Program.IsRunning)
				return;

			m_initSized = true;
			m_independentParentInitSized = true;
			m_lastAffectedInitSized = true;

			//Prevents flicker
			if (m_parentWithIndependentSize != null)
				m_parentWithIndependentSize.SuspendLayout();

			int height = ColumnHeadersHeight + 2;
			if (HorizontalScrollBar.Visible) 
				height += SystemInformation.HorizontalScrollBarHeight;
			for (int row = 0; row < RowCount; ++row)
			{
				height = Math.Min(MaxHeight, height + Rows[row].Height);
				if (height >= MaxHeight) 
					break;
			}

			Control cControl = this;
			int iLayer = 0;
			while (iLayer < ParentLayersAffected && cControl.Parent != null)
			{
				cControl = cControl.Parent;
				iLayer++;
			}

			cControl.ClientSize = new Size(cControl.ClientSize.Width, height + m_parentExtraHeight);

			if (m_parentWithIndependentSize != null)
				m_parentWithIndependentSize.ResumeLayout();
		}

		#endregion

		#region AutoScroll

		public delegate void RowDroppedEventHangler(object source, DataGridViewRow sourceRow, DataGridViewRow destinationRow);
		private bool m_scrolling;
		private System.Threading.Timer m_scrollTimer;

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

		#endregion
	}
}