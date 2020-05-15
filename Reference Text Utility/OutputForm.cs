using System.Windows.Forms;
using SIL.Windows.Forms.Extensions;

namespace Glyssen.ReferenceTextUtility
{
	public partial class OutputForm : Form
	{
		private int m_totalMessages = 0;
		private bool m_messageLimitReached = false;
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
			if (m_messageLimitReached)
				return;
			this.SafeInvoke(() =>
			{
				m_totalMessages++;
				if (m_totalMessages < 10000)
				{
					if (isError)
						m_logBox.WriteError(message);
					else
						m_logBox.WriteMessage(message);
				}
				else if (!m_messageLimitReached)
				{
					m_logBox.WriteError("Maximum number of messages reached!");
					m_messageLimitReached = true;
				}

			}, GetType().FullName + ".DisplayMessage");
		}
	}
}
