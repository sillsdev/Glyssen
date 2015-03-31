using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using L10NSharp;

namespace ProtoScript.Controls
{
	public partial class BlockProgressBar : ProgressBarUnanimated
	{
		public BlockProgressBar()
		{
			InitializeComponent();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;
			double percentComplete = Maximum == 0 ? 100 : (double)Value / Maximum * 100;
			int blocksRemaining = Maximum - Value;
			string text = string.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.BlockProgressFmt",
				"{0:N1}% Complete; {1} Blocks Remaining"), percentComplete, blocksRemaining);
			using (var f = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold))
			{
				SizeF len = g.MeasureString(text, f);
				// Calculate the location of the text (the middle of progress bar)
				var location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2), Convert.ToInt32((Height / 2) - len.Height / 2));
				g.DrawString(text, f, Brushes.Black, location);
			}
		}

		// Avoids flicker of text on bar
		[DllImport("uxtheme.dll")]
		private static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);
		protected override void OnHandleCreated(EventArgs e)
		{
			SetWindowTheme(Handle, "", "");
			base.OnHandleCreated(e);
		}
	}
}
