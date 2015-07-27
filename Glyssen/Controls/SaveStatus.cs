using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using Timer = System.Timers.Timer;

namespace Glyssen.Controls
{
	public partial class SaveStatus : UserControl
	{
		private Timer m_timer;
		private readonly string m_savedLabelFmt;

		public SaveStatus()
		{
			InitializeComponent();

			m_savedLabelFmt = m_lbl.Text;
		}

		public Color BackgroundColor 
		{
			get { return BackColor; }
			set
			{
				BackColor = value;
				m_lbl.BackColor = value;
			}
		}

		public void OnSaving(bool requiresFinishEvent = false)
		{
			ShowSaving();

			if (m_timer != null)
				m_timer.Close();

			if (!requiresFinishEvent)
			{
				m_timer = new Timer(1000);
				m_timer.Elapsed += ((sender, args) =>
				{
					((Timer)sender).Enabled = false;
					if (m_lbl.InvokeRequired)
					{
						m_lbl.BeginInvoke((MethodInvoker)(OnSaved));
					}
					else
					{
						OnSaved();
					}
				});
				m_timer.Enabled = true;
			}
		}

		public void OnSaved()
		{
			if (m_timer != null)
				m_timer.Close();

			ShowSaved();
		}

		private void ShowSaving()
		{
			m_lbl.Text = LocalizationManager.GetString("Common.SaveStatus.Saving", "Saving...");
			m_lbl.ForeColor = Color.Yellow;			
		}

		private void ShowSaved()
		{
			m_lbl.Text = m_savedLabelFmt;
			m_lbl.ForeColor = Color.White;			
		}
	}
}
