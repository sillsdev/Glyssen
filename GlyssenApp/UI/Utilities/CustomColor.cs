using System.Collections.Generic;
using System.Drawing;

namespace GlyssenApp.UI.Utilities
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
		public virtual Color BackColor { get { return SystemColors.Control; } }
		public virtual Color MouseDownBackColor { get { return MouseOverBackColor; } }
		public virtual Color MouseOverBackColor { get { return SystemColors.ControlDark; } }
		public virtual Color ForeColor { get { return SystemColors.WindowText; } }
		public virtual Color LinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color ActiveLinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public virtual Color VisitedLinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color Warning { get { return Color.Red; } }
		public virtual Color Highlight1 { get { return Color.DarkBlue; } }
		public virtual Color Highlight2 { get { return Color.MediumBlue; } }
		public virtual Color Highlight3 { get { return Color.Blue; } }

		public static readonly List<Color> MaxContrastColorList = new List<Color>
		{
			Color.FromArgb(255, 236, 066), //Soft Vivid Yellow
			Color.FromArgb(210, 111, 113), //Dusty Plum
			Color.FromArgb(247, 160, 102), //Peachy Orange
			Color.FromArgb(156, 152, 228), //Light Blue/Lavender
			Color.FromArgb(151, 206, 142), //Soft green
			Color.FromArgb(169, 150, 160), //Medium Off-Gray
			Color.FromArgb(243, 092, 180), //Pepto Bismol
			Color.FromArgb(206, 162, 098), //Grayish Yellow
			Color.FromArgb(208, 234, 030), //Soft green
			Color.MediumTurquoise,
			Color.FromArgb(245, 192, 237), //Light Pink
			Color.LimeGreen,
			Color.OrangeRed
		};

		public virtual Color GetMatchColor(int i)
		{
			return MaxContrastColorList[i % MaxContrastColorList.Count];
		}
	}

	public class TraditionalBlueColorScheme : DefaultColorScheme
	{
		public override Color BackColor { get { return Color.FromArgb(0, 73, 108); } }
		public override Color MouseOverBackColor { get { return Color.FromArgb(0, 93, 128); } }
		public override Color ForeColor { get { return Color.White; } }
		public override Color LinkColor { get { return Color.FromArgb(51, 153, 255); } }
		public override Color ActiveLinkColor { get { return LinkColor; } }
		public override Color VisitedLinkColor { get { return LinkColor; } }
		public override Color Highlight1 { get { return Color.Yellow; } }
		public override Color Highlight2 { get { return Color.LawnGreen; } }
		public override Color Highlight3 { get { return Color.Orange; } }
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