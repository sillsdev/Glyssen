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

			if (buttons == Buttons.AbortRetryIgnore)
				messageBoxButtons = MessageBoxButtons.AbortRetryIgnore;
			else if (buttons == Buttons.OK)
				messageBoxButtons = MessageBoxButtons.OK;
			else if (buttons == Buttons.OKCancel)
				messageBoxButtons = MessageBoxButtons.OKCancel;
			else if (buttons == Buttons.RetryCancel)
				messageBoxButtons = MessageBoxButtons.RetryCancel;
			else if (buttons == Buttons.YesNo)
				messageBoxButtons = MessageBoxButtons.YesNo;
			else // if (buttons == Buttons.YesNoCancel)
				messageBoxButtons = MessageBoxButtons.YesNoCancel;

			if (icon == Icon.Asterisk)
				messageBoxIcon = MessageBoxIcon.Asterisk;
			else if (icon == Icon.Error)
				messageBoxIcon = MessageBoxIcon.Error;
			else if (icon == Icon.Exclamation)
				messageBoxIcon = MessageBoxIcon.Exclamation;
			else if (icon == Icon.Hand)
				messageBoxIcon = MessageBoxIcon.Hand;
			else if (icon == Icon.Information)
				messageBoxIcon = MessageBoxIcon.Information;
			else if (icon == Icon.None)
				messageBoxIcon = MessageBoxIcon.None;
			else if (icon == Icon.Question)
				messageBoxIcon = MessageBoxIcon.Question;
			else if (icon == Icon.Stop)
				messageBoxIcon = MessageBoxIcon.Stop;
			else // if (icon == Icon.Warning)
				messageBoxIcon = MessageBoxIcon.Warning;

			if (defaultButton == DefaultButton.Button1)
				messageBoxDefaultButton = MessageBoxDefaultButton.Button1;
			else if (defaultButton == DefaultButton.Button2)
				messageBoxDefaultButton = MessageBoxDefaultButton.Button2;
			else // if (defaultButton == DefaultButton.Button3)
				messageBoxDefaultButton = MessageBoxDefaultButton.Button3;

			DialogResult result = MessageBox.Show(text, caption, messageBoxButtons, messageBoxIcon, messageBoxDefaultButton);

			if (result == DialogResult.Abort)
				return MessageResult.Abort;
			if (result == DialogResult.Cancel)
				return MessageResult.Cancel;
			if (result == DialogResult.Ignore)
				return MessageResult.Ignore;
			if (result == DialogResult.No)
				return MessageResult.No;
			if (result == DialogResult.None)
				return MessageResult.None;
			if (result == DialogResult.OK)
				return MessageResult.OK;
			if (result == DialogResult.Retry)
				return MessageResult.Retry;
			// if (result == DialogResult.Yes)
			return MessageResult.Yes;
		}
	}
}
