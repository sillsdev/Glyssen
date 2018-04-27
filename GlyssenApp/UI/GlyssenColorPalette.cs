using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GlyssenApp.Utilities;
using SIL.Windows.Forms.AppColorPalette;

namespace GlyssenApp.UI
{
	public class GlyssenColorPalette : AppColorPaletteExtender<GlyssenColors>
	{
		private static IGlyssenColorScheme s_colorScheme = new DefaultColorScheme();

		public static IGlyssenColorScheme ColorScheme
		{
			get { return s_colorScheme; }
			set { s_colorScheme = value; }
		}

		protected override bool UsePaletteColorsForComponent(IComponent component)
		{
			return base.UsePaletteColorsForComponent(component) &&
				(component is Form || component is Label || component is UserControl || component is RadioButton || component is CheckBox || component is TabPage);
		}

		public override Color GetColor(GlyssenColors glyssenColor)
		{
			switch (glyssenColor)
			{
				case GlyssenColors.ForeColor: return ColorScheme.ForeColor;
				case GlyssenColors.BackColor: return ColorScheme.BackColor;
				case GlyssenColors.LinkColor: return ColorScheme.LinkColor;
				case GlyssenColors.ActiveLinkColor: return ColorScheme.ActiveLinkColor;
				case GlyssenColors.DisabledLinkColor: return ColorScheme.DisabledLinkColor;
				case GlyssenColors.VisitedLinkColor: return ColorScheme.VisitedLinkColor;
				case GlyssenColors.MouseDownBackColor: return ColorScheme.MouseDownBackColor;
				case GlyssenColors.MouseOverBackColor: return ColorScheme.MouseOverBackColor;
				case GlyssenColors.Warning: return ColorScheme.Warning;
				case GlyssenColors.Default: return Color.Aqua; // This should never happen!
				default: throw new ArgumentException("Unexpected Glyssen color " + glyssenColor);
			}
		}

		protected override GlyssenColors GetDefaultPaletteColor(ColorProperties colorProperty)
		{
			switch (colorProperty)
			{
				case ColorProperties.ForeColor:
				case ColorProperties.BorderColor:
					return GlyssenColors.ForeColor;
				case ColorProperties.BackColor: return GlyssenColors.BackColor;
				case ColorProperties.MouseDownBackColor: return GlyssenColors.MouseDownBackColor;
				case ColorProperties.MouseOverBackColor: return GlyssenColors.MouseOverBackColor;
				case ColorProperties.LinkColor: return GlyssenColors.LinkColor;
				case ColorProperties.ActiveLinkColor: return GlyssenColors.ActiveLinkColor;
				case ColorProperties.DisabledLinkColor: return GlyssenColors.DisabledLinkColor;
				case ColorProperties.VisitedLinkColor: return GlyssenColors.VisitedLinkColor;

				default: return base.GetDefaultPaletteColor(colorProperty);
			}
		}
	}
}
