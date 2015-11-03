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
			this.components = new System.ComponentModel.Container();
			this.colBundleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colModifiedDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			colModifiedDate.DefaultCellStyle.Format = "MM/dd/yyyy";
			this.colInactive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();

			m_list.Columns.AddRange(colBundleName, colModifiedDate);
			m_list.Columns.AddRange(colInactive);
			// 
			// m_list
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_list, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_list, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_list, "ProjectsList");
			// 
			// colBundleName
			// 
			colBundleName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			colBundleName.HeaderText = "_L10N_:DialogBoxes.OpenProjectDlg.ProjectsList.OriginalBundleFilename!Original Bundle Filename";
			colBundleName.MinimumWidth = 50;
			colBundleName.Name = "colBundleName";
			colBundleName.ReadOnly = true;
			colBundleName.Width = 94;
			//
			// colModifiedDate
			// 
			colModifiedDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			colModifiedDate.HeaderText = "_L10N_:DialogBoxes.OpenProjectDlg.ProjectsList.ModifiedDate!Date Modified";
			colModifiedDate.MinimumWidth = 50;
			colModifiedDate.Name = "colModifiedDate";
			colModifiedDate.ReadOnly = true;
			colModifiedDate.Width = 94;
			// 
			// colInactive
			// 
			colInactive.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			colInactive.HeaderText = "_L10N_:DialogBoxes.OpenProjectDlg.ProjectsList.Inactive!Inactive";
			colInactive.MinimumWidth = 50;
			colInactive.Name = "colInactive";
			colInactive.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			colInactive.Width = 73;
			// 
			// locExtender
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			// 
			// ExistingProjectsList
			// 
			this.Name = "ExistingProjectsList";
			this.Size = new System.Drawing.Size(368, 147);
			this.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
		}
		#endregion

		private System.Windows.Forms.DataGridViewTextBoxColumn colBundleName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colModifiedDate;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colInactive;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
