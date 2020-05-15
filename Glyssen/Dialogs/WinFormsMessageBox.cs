using System.Windows.Forms;
using Glyssen.Shared;
using GlyssenEngine.Utilities;

namespace Glyssen.Dialogs
{
	public class WinFormsMessageBox : IMessageModal
	{
		public string Caption => GlyssenInfo.Product;

		public void Show(string text, bool warningIcon)
		{
			MessageBox.Show(text, Caption, MessageBoxButtons.OK, warningIcon ? MessageBoxIcon.Warning : MessageBoxIcon.None);
		}
	}
}
