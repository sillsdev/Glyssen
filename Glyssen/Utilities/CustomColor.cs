using System.Drawing;

namespace Glyssen.Utilities
{
	public interface IGlyssenColorScheme
	{
		Color BackColor { get; }
		Color MouseDownBackColor { get; }
		Color MouseOverBackColor { get; }
		Color ForeColor { get; }
		Color LinkColor { get; }
		Color ActiveLinkColor { get; }
		Color DisabledLinkColor { get; }
		Color VisitedLinkColor { get; }
		Color Warning { get; }
		Color Highlight1 { get; }
		Color Highlight2 { get; }
		Color Highlight3 { get; }

		Color GetMatchColor(int i);
	}

	public class DefaultColorScheme : IGlyssenColorScheme
	{
		public Color BackColor { get { return SystemColors.Control; } }
		public Color MouseDownBackColor { get { return MouseOverBackColor; } }
		public Color MouseOverBackColor { get { return SystemColors.ControlDark; } }
		public Color ForeColor { get { return SystemColors.WindowText; } }
		public Color LinkColor { get { return SystemColors.HotTrack; } }
		public Color ActiveLinkColor { get { return SystemColors.HotTrack; } }
		public Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public Color VisitedLinkColor { get { return SystemColors.HotTrack; } }
		public Color Warning { get { return Color.Red; } }
		public Color Highlight1 { get { return Color.DarkBlue; } }
		public Color Highlight2 { get { return Color.MediumBlue; } }
		public Color Highlight3 { get { return Color.Blue; } }

		public Color GetMatchColor(int i)
		{
			switch (i % 7)
			{
				case 0: return Color.LightPink;
				case 1:	return Color.PaleTurquoise;
				case 2: return Color.LightSalmon;
				case 3: return Color.PaleGreen;
				case 4: return Color.LightYellow;
				case 5: return Color.DarkKhaki;
				default: return Color.GreenYellow;
			}
		}
	}

	public class TraditionalBlueColorScheme : IGlyssenColorScheme
	{
		public Color BackColor { get { return Color.FromArgb(0, 73, 108); } }
		public Color MouseDownBackColor { get { return MouseOverBackColor; } }
		public Color MouseOverBackColor { get { return Color.FromArgb(0, 93, 128); } }
		public Color ForeColor { get { return Color.White; } }
		public Color LinkColor { get { return Color.FromArgb(51, 153, 255); } }
		public Color ActiveLinkColor { get { return LinkColor; } }
		public Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public Color VisitedLinkColor { get { return LinkColor; } }
		public Color Warning { get { return Color.Red; } }
		public Color Highlight1 { get { return Color.Yellow; } }
		public Color Highlight2 { get { return Color.LawnGreen; } }
		public Color Highlight3 { get { return Color.Orange; } }

		public Color GetMatchColor(int i)
		{
			switch (i % 7)
			{
				case 0: return Color.LavenderBlush;
				case 1: return Color.PaleTurquoise;
				case 2: return Color.LightSalmon;
				case 3: return Color.PaleGreen;
				case 4: return Color.LightYellow;
				case 5: return Color.Bisque;
				default: return Color.Azure;
			}
		}
	}

	public enum GlyssenColors
	{
		Default,
		ForeColor,
		BackColor,
		LinkColor,
		ActiveLinkColor,
		DisabledLinkColor,
		VisitedLinkColor,
		MouseDownBackColor,
		MouseOverBackColor,
		Warning,
	}
}
