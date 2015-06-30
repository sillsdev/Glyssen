using System.Security.AccessControl;
using Gecko;

namespace Glyssen.Controls
{
	partial class ExistingProjectsList
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			colBundleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			colInactive = new System.Windows.Forms.DataGridViewTextBoxColumn();

			this.SuspendLayout();

			m_list.Columns.AddRange(colBundleName, colInactive);

			colBundleName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			colBundleName.HeaderText = "Original Bundle Filename";
			colBundleName.MinimumWidth = 50;
			colBundleName.Name = "colBundleName";
			colBundleName.ReadOnly = true;
			colBundleName.Width = 94;

			colInactive.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			colInactive.HeaderText = "Inactive";
			colInactive.MinimumWidth = 50;
			colInactive.Name = "colInactive";
			colInactive.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			colInactive.Width = 73;

			// 
			// ExistingProjectsList
			// 
			this.Name = "ExistingProjectsList";
			this.Size = new System.Drawing.Size(368, 147);
			this.ResumeLayout(false);
		}
		#endregion

		private System.Windows.Forms.DataGridViewTextBoxColumn colBundleName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colInactive;

	}
}
