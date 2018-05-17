using System;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using Timer = System.Timers.Timer;

namespace Glyssen.UI.Controls
{
	public partial class SaveStatus : UserControl
	{
		private Timer m_timer;
		private readonly string m_savedLabelFmt;
		private readonly string m_justSavedLabelFmt;
		private readonly string m_savingLabelFmt;
		private readonly string m_savingLongWaitLabelFmt;

		public const int kMinimumSavingTime = 1000;
		public const int kJustSavedTime = 2000;

		private bool m_success;
		private bool m_timerUp;

		public SaveStatus()
		{
			InitializeComponent();

			m_savedLabelFmt = m_lbl.Text;
			m_justSavedLabelFmt = LocalizationManager.GetString("Common.SaveStatus.JustSaved", "Saved on {0:d} at {0:t}");
			m_savingLabelFmt = LocalizationManager.GetString("Common.SaveStatus.Saving", "Saving...");
			m_savingLongWaitLabelFmt = LocalizationManager.GetString("Common.SaveStatus.SavingLongWait", "Saving... (taking longer than expected)");
		}

		public override Color BackColor
		{
			get { return base.BackColor; }
			set
			{
				base.BackColor = value;
				m_lbl.BackColor = value;
			}
		}

		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set
			{
				base.ForeColor = value;
				m_lbl.ForeColor = value;
			}
		}

		public ContentAlignment TextAlign
		{
			get { return m_lbl.TextAlign; }
			set { m_lbl.TextAlign = value; }
		}

		public string SavedLabel { get { return m_savedLabelFmt; } }
		public string SavingLabel { get { return m_savingLabelFmt; } }
		public string SavingLongWaitLabel { get { return m_savingLongWaitLabelFmt; } }

		public string StatusText { get { return m_lbl.Text; } }

		/// <summary>
		/// <para>Changes status to saving status for kMinimumSavingTime milliseconds.</para>
		/// <para>If OnSaved is not called within kMinimumSavingTime milliseconds, status is changed to the saving-long-wait status.</para>
		/// </summary>
		/// <param name="requiresOnSaved">If false, status will automatically revert to saved status after kMinimumSavingTime milliseconds without further input.
		/// (In other words, setting this parameter to false is only intended deceptively to bring a sense of security to the user.)</param>
		public void OnSaving(bool requiresOnSaved = true)
		{
			m_success = false;
			m_timerUp = false;

			ShowSaving();

			if (m_timer != null)
				m_timer.Close();

			m_timer = new Timer(kMinimumSavingTime);
			m_timer.Elapsed += ((sender, args) =>
			{
				((Timer)sender).Enabled = false;
				if (m_lbl.InvokeRequired)
				{
					m_lbl.BeginInvoke((MethodInvoker)(() =>
					{
						HandleTimerUp(requiresOnSaved);
					}));
				}
				else
				{
					HandleTimerUp(requiresOnSaved);
				}
			});
			m_timer.Enabled = true;
		}

		public void OnSaved()
		{
			m_success = true;

			if (m_timer == null)
				m_timerUp = true;
			else
				m_timer.Close();

			if (m_timerUp)
			{
				ShowSaved();
			}
		}

		private void HandleTimerUp(bool requiresOnSaved)
		{
			m_timerUp = true;
			if (!requiresOnSaved || m_success)
			{
				ShowSaved();
			}
			else
			{
				ShowSavingLongWait();
			}
		}

		private void ShowSaving()
		{
			m_lbl.Text = m_savingLabelFmt;
			ForeColor = GlyssenColorPalette.ColorScheme.Highlight1;
		}

		private void ShowSaved()
		{
			m_lbl.Text = string.Format(m_justSavedLabelFmt, DateTime.Now);
			ForeColor = GlyssenColorPalette.ColorScheme.Highlight2;

			if (m_timer != null)
				m_timer.Close();

			m_timer = new Timer(kJustSavedTime);
			m_timer.Elapsed += ((sender, args) =>
			{
				((Timer)sender).Enabled = false;
				if (m_lbl.InvokeRequired)
				{
					m_lbl.BeginInvoke((MethodInvoker)(() =>
					{
						m_lbl.Text = m_savedLabelFmt;
						ForeColor = GlyssenColorPalette.ColorScheme.ForeColor;
					}));
				}
				else
				{
					m_lbl.Text = m_savedLabelFmt;
					ForeColor = GlyssenColorPalette.ColorScheme.ForeColor;
				}
			});
			m_timer.Enabled = true;

		}

		private void ShowSavingLongWait()
		{
			m_lbl.Text = m_savingLongWaitLabelFmt;
			ForeColor = GlyssenColorPalette.ColorScheme.Highlight3;
		}
	}
}
