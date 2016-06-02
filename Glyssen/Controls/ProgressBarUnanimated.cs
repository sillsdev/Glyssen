using System.Drawing;
using System.Windows.Forms;

namespace Glyssen.Controls
{
	public class ProgressBarUnanimated : ProgressBar
	{
		private readonly Color m_barForeColor = Color.ForestGreen;

		public ProgressBarUnanimated()
		{
			SetStyle(ControlStyles.UserPaint, true);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// None... Helps control the flicker.
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			const int inset = 2; // A single inset value to control teh sizing of the inner rect.

			using (Image offscreenImage = new Bitmap(Width, Height))
			{
				using (Graphics offscreen = Graphics.FromImage(offscreenImage))
				{
					var rect = new Rectangle(0, 0, Width, Height);

					if (ProgressBarRenderer.IsSupported)
						ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);

					rect.Inflate(new Size(-inset, -inset)); // Deflate inner rect.
					rect.Width = (int)(rect.Width * ((double)Value / Maximum));
					if (rect.Width == 0) rect.Width = 1; // Can't draw rec with width of 0.

					var brush = new SolidBrush(m_barForeColor);
					offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);

					e.Graphics.DrawImage(offscreenImage, 0, 0);
					offscreenImage.Dispose();
				}
			}
		}
	}
}
