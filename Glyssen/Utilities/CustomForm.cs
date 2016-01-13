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
			BackColor = CustomColor.BackColor;
			ForeColor = CustomColor.ForeColor;
			SetLinkColors(this);
		}

		private static void SetLinkColors(Control control)
		{
			var lnkLabel = control as LinkLabel;
			if (lnkLabel != null)
			{
				lnkLabel.ActiveLinkColor = CustomColor.LinkColor;
				lnkLabel.LinkColor = CustomColor.LinkColor;
				lnkLabel.VisitedLinkColor = CustomColor.LinkColor;
			}

			foreach (Control child in control.Controls)
			{
				SetLinkColors(child);
			}
		}
	}
}
