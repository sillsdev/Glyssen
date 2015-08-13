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
		private Control m_independentParentResizeListen;
		private Control m_lastAffectedResizeListen;
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
				if (ParentWithIndependentSize == null)
					return -1;
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
				Control cControl = this;
				int layer = 0;
				while (cControl.Parent != null && layer < ParentLayersAffected)
				{
					cControl = cControl.Parent;
					layer++;
				}
				return cControl;
			}	
		}

		private Control IndependentParentResizeListen
		{
			get
			{
				return m_independentParentResizeListen;
			}
			set
			{
				if (m_independentParentResizeListen != null)
					m_independentParentResizeListen.ClientSizeChanged -= HandleIndependentParentResize;

				m_independentParentInitSized = false;
				m_independentParentResizeListen = value;
				if (m_independentParentResizeListen != null)
					m_independentParentResizeListen.ClientSizeChanged += HandleIndependentParentResize;
			}
		}

		private Control LastAffectedResizeListen
		{
			get
			{
				return m_lastAffectedResizeListen;
			}
			set
			{
				if (m_lastAffectedResizeListen != null)
					m_lastAffectedResizeListen.ClientSizeChanged -= HandleLastAffectedResize;

				m_lastAffectedInitSized = false;
				m_lastAffectedResizeListen = value;
				if (m_lastAffectedResizeListen != null)
					m_lastAffectedResizeListen.ClientSizeChanged += HandleLastAffectedResize;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
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
			IndependentParentResizeListen = null;
			if (ParentWithIndependentSize == null)
			{
				if (LastAffected != this)
				{
					LastAffected.ParentChanged += HandleParentChanged;
					LastAffectedResizeListen = LastAffected;
				}
			}
			else
			{
				m_initHeight = Height;
				IndependentParentResizeListen = ParentWithIndependentSize;
			}
			base.OnParentChanged(e);
		}

		private void HandleParentChanged(object sender, EventArgs e)
		{
			IndependentParentResizeListen = null;
			OnParentChanged(e);
		}

		private void HandleLastAffectedResize(object sender, EventArgs e)
		{
			if (!m_lastAffectedInitSized)
			{
				m_parentExtraHeight = LastAffected.Height - m_initHeight;
				m_lastAffectedInitSized = true;
			}
		}

		private void HandleIndependentParentResize(object sender, EventArgs e)
		{
			if (!m_independentParentInitSized)
			{
				MinBottomMargin = ParentWithIndependentSize.Height - LastAffected.Bottom;
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
			if (ParentWithIndependentSize != null)
				ParentWithIndependentSize.SuspendLayout();

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

			if (ParentWithIndependentSize != null)
				ParentWithIndependentSize.ResumeLayout();
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