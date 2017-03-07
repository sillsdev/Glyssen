using System.Windows.Forms;
using Glyssen.Controls;

namespace Glyssen.ReferenceTextUtility
{
	public partial class OutputForm : Form
	{
		public OutputForm()
		{
			InitializeComponent();
			m_logBox.ShowCopyToClipboardMenuItem = true;
			m_logBox.ShowFontMenuItem = true;
		}

		public void Clear()
		{
			m_logBox.Clear();
		}

		public void DisplayMessage(string message, bool isError)
		{
			this.SafeInvoke(() =>
			{
				if (isError)
					m_logBox.WriteError(message);
				else
					m_logBox.WriteMessage(message);
			});
		}
	}
}
