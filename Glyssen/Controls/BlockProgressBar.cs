using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Glyssen.Utilities;
using L10NSharp;

namespace Glyssen.Controls
{
	public partial class BlockProgressBar : ProgressBarUnanimated
	{
		private string m_unitName;

		public BlockProgressBar()
		{
			InitializeComponent();
			UnitName = null; // Force initialization to default value
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;
			double percentComplete = Maximum == 0 ? 100 : (double)Value / Maximum * 100;
			int blocksRemaining = Maximum - Value;
			string text = string.Format(LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.AssignmentProgressFmt",
				"{0} Complete; {1} {2} Remaining",
				"Param 0: percentage; " +
				"Param 1: integer (number of blocks or passages remaining); " +
				"Param 2: name of unit of work, either \"blocks\" or \"passages\""),
				MathUtilities.FormattedPercent(percentComplete, 1, 1), blocksRemaining, UnitName);

			SizeF len = g.MeasureString(text, Font);
			// Calculate the location of the text (the middle of progress bar)
			var location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2), Convert.ToInt32((Height / 2) - len.Height / 2));
			g.DrawString(text, Font, Brushes.Black, location);
		}

		public string UnitName
		{
			get => m_unitName;
			set => m_unitName = value ?? LocalizationManager.GetString("DialogBoxes.AssignCharacterDlg.BlockProgressUnitName", "Blocks",
				"Parameter #2 in DialogBoxes.AssignCharacterDlg.AssignmentProgressFmt");
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
