using Waxuquerque.Utilities;

namespace Glyssen.UI.Dialogs
{
	public class WinFormsMessageModal : IMessageModal
	{
		public void Show(string text)
		{
			System.Windows.Forms.MessageBox.Show(text);
		}

		public void Show(string text, string caption)
		{
			System.Windows.Forms.MessageBox.Show(text, caption);
		}
	}
}