using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlyssenEngine.Utilities;

namespace Glyssen.Dialogs
{
	public class WinFormsMessageModal : IMessageModal
	{
		public void ShowMessage(string message)
		{
			MessageBox.Show(message);
		}

		public void ShowWarning(string message)
		{
			MessageBox.Show(message, @"Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public void ShowError(string message)
		{
			MessageBox.Show(message, @"Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public bool AskQuestion(string question)
		{
			if (DialogResult.Yes == MessageBox.Show(question, @"Question:", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				return true;
			else
				return false;
		}

		public bool AskRetry(string question)
		{
			if (DialogResult.Retry == MessageBox.Show(question, @"Try Again?", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question))
				return true;
			else
				return false;
		}

		public int AbortRetryIgnore(string question)
		{
			DialogResult result = MessageBox.Show(question, "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

			if (result == DialogResult.Abort)
				return 0;
			else if (result == DialogResult.Retry)
				return 1;
			else //	if (result == DialogResult.Ignore)
				return 2;
		}
	}
}
