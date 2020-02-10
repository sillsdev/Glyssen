﻿using System.Windows.Forms;

namespace Glyssen.Controls
{
	public static class ControlExtensions
	{
		public static Control FindFocusedControl(this Control control)
		{
			var container = control as IContainerControl;
			while (container != null)
			{
				control = container.ActiveControl;
				container = control as IContainerControl;
			}
			return control;
		}
	}
}
