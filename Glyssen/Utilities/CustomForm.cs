using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Properties;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace Glyssen.Utilities
{ 
	public class CustomForm: Form
	{
		private string m_formSettingsName;
		private bool m_finishedLoading;

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			SetColors();
		}

		private void SetColors()
		{
			BackColor = CustomColor.BackColor;
			ForeColor = CustomColor.ForeColor;
			SetLinkColors(this);
		}

		private static void SetLinkColors(Control control)
		{
			var lnkLabel = control as LinkLabel;
			if (lnkLabel != null)
			{
				lnkLabel.ActiveLinkColor = CustomColor.LinkColor;
				lnkLabel.LinkColor = CustomColor.LinkColor;
				lnkLabel.VisitedLinkColor = CustomColor.LinkColor;
			}

			foreach (Control child in control.Controls)
			{
				SetLinkColors(child);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			RestoreFormSettings();
			RestoreGridSettings(this);
			m_finishedLoading = true;
			base.OnLoad(e);
		}

		/// <summary>
		/// Save the current form size and position, if there is an entry in Settings.Settings for the form.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResizeEnd(e);
			if (!m_finishedLoading) return;
			if (WindowState != FormWindowState.Normal) return;
			if (!SettingExists(m_formSettingsName)) return;
			Settings.Default.Save();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			SaveGridSettings(this);
			Settings.Default.Save();
			base.OnClosing(e);
		}

		/// <summary>
		/// To enable remembering and restoring form size and position, add an entry in Settings.Settings for
		/// the form.  The name of the entry should be the name of the form followed by "FormSettings" and the
		/// type should be "SIL.Windows.Forms.PortableSettingsProvider.FormSettings"
		/// </summary>
		private void RestoreFormSettings()
		{
			// attempt to restore form size and position
			m_formSettingsName = Name + "FormSettings";
			if (SettingExists(m_formSettingsName))
			{
				if (Settings.Default[m_formSettingsName] == null)
				{
					Settings.Default[m_formSettingsName] = FormSettings.Create(this);
					Settings.Default.Save();
				}
				else
				{
					((FormSettings)Settings.Default[m_formSettingsName]).InitializeForm(this);					
				}
			} 
		}

		/// <summary>
		/// To enable remembering and restoring grid properties, add an entry in Settings.Settings for the grid.
		/// The name of the entry should be the name of the grid followed by "GridSettings" and the type should
		/// be "SIL.Windows.Forms.PortableSettingsProvider.GridSettings"
		/// </summary>
		/// <param name="control"></param>
		private static void RestoreGridSettings(Control control)
		{
			var grid = control as DataGridView;
			if (grid != null)
			{
				var settingName = grid.Name + "GridSettings";

				if (SettingExists(settingName))
				{
					if (Settings.Default[settingName] == null)
					{
						Settings.Default[settingName] = GridSettings.Create(grid);
						Settings.Default.Save();
					}
					else
					{
						((GridSettings)Settings.Default[settingName]).InitializeGrid(grid);
					}
				} 
			}

			foreach (Control child in control.Controls)
			{
				RestoreGridSettings(child);
			}
		}

		private static void SaveGridSettings(Control control)
		{
			var grid = control as DataGridView;
			if (grid != null)
			{
				var settingName = grid.Name + "GridSettings";

				if (SettingExists(settingName))
				{
					Settings.Default[settingName] = GridSettings.Create(grid);
				}
			}

			foreach (Control child in control.Controls)
			{
				SaveGridSettings(child);
			}
		}

		private static bool SettingExists(string settingName)
		{
			return Settings.Default.Properties.Cast<SettingsProperty>().Any(prop => prop.Name == settingName);
		}
	}
}
