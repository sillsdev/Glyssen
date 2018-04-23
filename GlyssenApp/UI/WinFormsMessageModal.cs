using Glyssen;
using Waxuquerque;

namespace GlyssenApp.UI
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