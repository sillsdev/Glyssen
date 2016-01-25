using System.Drawing;

namespace Glyssen.Utilities
{
	public interface IGlyssenColorScheme
	{
		Color BackColor { get; }
		Color ForeColor { get; }
		Color LinkColor { get; }
		Color ActiveLinkColor { get; }
		Color DisabledLinkColor { get; }
		Color VisitedLinkColor { get; }
		Color Warning { get; }
		Color Highlight1 { get; }
		Color Highlight2 { get; }
		Color Highlight3 { get; }
	}

	public class DefaultColorScheme : IGlyssenColorScheme
	{
		public Color BackColor { get { return SystemColors.Control; } }
		public Color ForeColor { get { return SystemColors.WindowText; } }
		public Color LinkColor { get { return SystemColors.HotTrack; } }
		public Color ActiveLinkColor { get { return SystemColors.HotTrack; } }
		public Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public Color VisitedLinkColor { get { return SystemColors.HotTrack; } }
		public Color Warning { get { return Color.Red; } }
		public Color Highlight1 { get { return Color.DarkBlue; } }
		public Color Highlight2 { get { return Color.MediumBlue; } }
		public Color Highlight3 { get { return Color.Blue; } }
	}

	public class TraditionalBlueColorScheme : IGlyssenColorScheme
	{
		public Color BackColor { get { return Color.FromArgb(0, 73, 108); } }
		public Color ForeColor { get { return Color.White; } }
		public Color LinkColor { get { return Color.FromArgb(51, 153, 255); } }
		public Color ActiveLinkColor { get { return LinkColor; } }
		public Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public Color VisitedLinkColor { get { return LinkColor; } }
		public Color Warning { get { return Color.Red; } }
		public Color Highlight1 { get { return Color.Yellow; } }
		public Color Highlight2 { get { return Color.LawnGreen; } }
		public Color Highlight3 { get { return Color.Orange; } }
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
	}
}
