using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Properties;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace Glyssen.Utilities
{ 
	public class FormWithPersistedSettings: Form
	{
		private string m_formSettingsName;
		private bool m_finishedLoading;

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
			if (string.IsNullOrEmpty(m_formSettingsName)) return;
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
		/// the form. The name of the entry should be the name of the form followed by "FormSettings" and the
		/// type should be "SIL.Windows.Forms.PortableSettingsProvider.FormSettings"
		/// </summary>
		private void RestoreFormSettings()
		{
			// attempt to restore form size and position
			m_formSettingsName = SettingExists(Name + "FormSettings");
			if (string.IsNullOrEmpty(m_formSettingsName) && Name.EndsWith("Dlg"))
			{
				m_formSettingsName = SettingExists(Name.Substring(0, Name.Length - 3) + "DialogFormSettings");
			}
			if (string.IsNullOrEmpty(m_formSettingsName)) return;

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
				var settingName = GetGridSettingsName(grid);

				if (!string.IsNullOrEmpty(settingName))
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
				var settingName = GetGridSettingsName(grid);

				if (!string.IsNullOrEmpty(settingName))
				{
					Settings.Default[settingName] = GridSettings.Create(grid);
				}
			}

			foreach (Control child in control.Controls)
			{
				SaveGridSettings(child);
			}
		}

		private static string SettingExists(string settingName)
		{
			var setting = Settings.Default.Properties.Cast<SettingsProperty>().FirstOrDefault(p => string.Equals(p.Name, settingName, StringComparison.CurrentCultureIgnoreCase));
			return setting == null ? null : setting.Name;
		}

		private static string GetGridSettingsName(DataGridView grid)
		{
			var settingName = SettingExists(grid.Name + "GridSettings");
			if (!string.IsNullOrEmpty(settingName))
			{
				return settingName;
			}

			// try to find the settings by removing "m_" from the grid name
			if (grid.Name.StartsWith("m_"))
			{
				settingName = SettingExists(grid.Name.Substring(2) + "GridSettings");
				if (!string.IsNullOrEmpty(settingName))
				{
					return settingName;
				}
			}

			// if you are here, the settings were not found
			return null;
		}
	}
}
