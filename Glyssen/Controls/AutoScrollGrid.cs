using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace Glyssen.Controls
{
	/// <summary>
	/// AutoScrollGrid will auto-scroll when dragging at the top or bottom.
	/// Partly taken from http://stackoverflow.com/questions/2567809/how-to-autoscroll-a-datagridview-during-drag-and-drop
	/// </summary>
	public class AutoScrollGrid : BetterGrid
	{
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
	}
}