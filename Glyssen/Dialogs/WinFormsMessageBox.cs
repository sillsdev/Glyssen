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
				case Buttons.OKCancel:
					messageBoxButtons = MessageBoxButtons.OKCancel;
					break;
				case Buttons.RetryCancel:
					messageBoxButtons = MessageBoxButtons.RetryCancel;
					break;
				case Buttons.YesNo:
					messageBoxButtons = MessageBoxButtons.YesNo;
					break;
				default: // Buttons.YesNoCancel
					messageBoxButtons = MessageBoxButtons.YesNoCancel;
					break;
			}

			switch (icon)
			{
				case Icon.Asterisk:
					messageBoxIcon = MessageBoxIcon.Asterisk;
					break;
				case Icon.Error:
					messageBoxIcon = MessageBoxIcon.Error;
					break;
				case Icon.Exclamation:
					messageBoxIcon = MessageBoxIcon.Exclamation;
					break;
				case Icon.Hand:
					messageBoxIcon = MessageBoxIcon.Hand;
					break;
				case Icon.Information:
					messageBoxIcon = MessageBoxIcon.Information;
					break;
				case Icon.None:
					messageBoxIcon = MessageBoxIcon.None;
					break;
				case Icon.Question:
					messageBoxIcon = MessageBoxIcon.Question;
					break;
				case Icon.Stop:
					messageBoxIcon = MessageBoxIcon.Stop;
					break;
				default: // Icon.Warning
					messageBoxIcon = MessageBoxIcon.Warning;
					break;
			}

			switch (defaultButton)
			{
				case DefaultButton.Button1:
					messageBoxDefaultButton = MessageBoxDefaultButton.Button1;
					break;
				case DefaultButton.Button2:
					messageBoxDefaultButton = MessageBoxDefaultButton.Button2;
					break;
				default: // DefaultButton.Button3
					messageBoxDefaultButton = MessageBoxDefaultButton.Button3;
					break;
			}

			DialogResult result = MessageBox.Show(text, caption, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton);

			switch (result)
			{
				case DialogResult.Abort:
					return MessageResult.Abort;
				case DialogResult.Cancel:
					return MessageResult.Cancel;
				case DialogResult.Ignore:
					return MessageResult.Ignore;
				case DialogResult.No:
					return MessageResult.No;
				case DialogResult.None:
					return MessageResult.None;
				case DialogResult.OK:
					return MessageResult.OK;
				case DialogResult.Retry:
					return MessageResult.Retry;
				default: // DialogResult.Yes
					return MessageResult.Yes;
			}
		}
	}
}
