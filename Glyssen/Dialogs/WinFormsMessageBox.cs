﻿using System;
using System.Windows.Forms;
using GlyssenEngine.Utilities;

namespace Glyssen.Dialogs
{
	public class WinFormsMessageBox : IMessageModal
	{
		public MessageResult Show(string text, string caption, Buttons buttons, Icon icon, DefaultButton defaultButton)
		{
			MessageBoxButtons messageBoxButtons;
			MessageBoxIcon messageBoxIcon;
			MessageBoxDefaultButton messageBoxDefaultButton;

			switch (buttons)
			{
				case Buttons.AbortRetryIgnore:
					messageBoxButtons = MessageBoxButtons.AbortRetryIgnore;
					break;
				case Buttons.OK:
					messageBoxButtons = MessageBoxButtons.OK;
					break;
				default:
					throw new NotImplementedException("Unexpected MessageModal button!");
			}

			switch (icon)
			{
				case Icon.None:
					messageBoxIcon = MessageBoxIcon.None;
					break;
				case Icon.Warning:
					messageBoxIcon = MessageBoxIcon.Warning;
					break;
				default:
					throw new NotImplementedException("Unexpected MessageModal icon!");
			}

			switch (defaultButton)
			{
				case DefaultButton.Button1:
					messageBoxDefaultButton = MessageBoxDefaultButton.Button1;
					break;
				case DefaultButton.Button2:
					messageBoxDefaultButton = MessageBoxDefaultButton.Button2;
					break;
				case DefaultButton.Button3:
					messageBoxDefaultButton = MessageBoxDefaultButton.Button3;
					break;
				default:
					throw new NotImplementedException("Unexpected MessageModal default button!");
			}

			DialogResult result = MessageBox.Show(text, caption, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton);

			switch (result)
			{
				case DialogResult.Abort:
					return MessageResult.Abort;
				case DialogResult.Ignore:
					return MessageResult.Ignore;
				case DialogResult.OK:
					return MessageResult.OK;
				case DialogResult.Retry:
					return MessageResult.Retry;
				case DialogResult.None:
					return MessageResult.None;
				default:
					throw new NotImplementedException("Unexpected dialog result!");
			}
		}
	}
}
