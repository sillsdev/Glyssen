using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Properties;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace Glyssen.UI.Dialogs
{
	public class FormWithPersistedSettings: Form
	{
		protected const int kChildFormLocationX = 202;
		protected const int kChildFormLocationY = 95;

		private string m_formSettingsName;
		private bool m_finishedLoading;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RestoreFormSettings();
			RestoreGridSettings(this);
			m_finishedLoading = true;

			var workingArea = Screen.GetWorkingArea(this);
			if (!workingArea.Contains(Bounds) && WindowState != FormWindowState.Maximized)
			{
				if (Bounds.Left < workingArea.Left || Bounds.Top < workingArea.Top)
					Location = new Point(Math.Max(Bounds.Left, workingArea.Left), Math.Max(Bounds.Top, workingArea.Top));

				if (Width > workingArea.Width)
					Width = workingArea.Width;
				if (Height > workingArea.Height)
					Height = workingArea.Height;
			}
		}

		/// <summary>
		/// Save the current form size and position, if there is an entry in Settings.Settings for the form.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResizeEnd(e);
			if (!m_finishedLoading || WindowState != FormWindowState.Normal || string.IsNullOrEmpty(m_formSettingsName))
				return;
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
			if (string.IsNullOrEmpty(m_formSettingsName))
				return;

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
						var gridSettings = (GridSettings)Settings.Default[settingName];
						if (gridSettings.Columns.Length == grid.Columns.Count)
							gridSettings.InitializeGrid(grid);
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
			return setting?.Name;
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

		protected void TileFormLocation()
		{
			if ((Owner == null) || (WindowState == FormWindowState.Maximized))
				return;
			var availableScreenRect = Screen.FromControl(this).WorkingArea;
			var newX = Owner.Location.X + kChildFormLocationX;
			var newY = Owner.Location.Y + kChildFormLocationY;
			if (newX + Width > availableScreenRect.Right)
				newX -= newX + Width - availableScreenRect.Right;
			if (newY + Height > availableScreenRect.Bottom)
				newY -= newY + Height - availableScreenRect.Bottom;
			Location = new Point(newX, newY);
		}
	}
}
