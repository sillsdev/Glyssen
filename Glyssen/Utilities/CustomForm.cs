using System;
using System.Windows.Forms;

namespace Glyssen.Utilities
{ 
	public class CustomForm: Form
	{
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			SetColors();
		}

		private void SetColors()
		{
			BackColor = GlyssenColorPalette.ColorScheme.BackColor;
			//ForeColor = GlyssenColorPalette.ColorScheme.ForeColor;
			//SetLinkColors(this);
		}

		//private static void SetLinkColors(Control control)
		//{
		//	var lnkLabel = control as LinkLabel;
		//	if (lnkLabel != null)
		//	{
		//		lnkLabel.ActiveLinkColor = GlyssenColorPalette.ColorScheme.LinkColor;
		//		lnkLabel.LinkColor = GlyssenColorPalette.ColorScheme.LinkColor;
		//		lnkLabel.VisitedLinkColor = GlyssenColorPalette.ColorScheme.LinkColor;
		//	}

		//	foreach (Control child in control.Controls)
		//	{
		//		SetLinkColors(child);
		//	}
		//}
	}
}
