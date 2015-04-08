using System.Security.AccessControl;
using Gecko;

namespace ProtoScript.Controls
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_list = new System.Windows.Forms.DataGridView();
			this.colLanguage = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colRecordingProjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colBundleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colInactive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colProjectPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.m_list)).BeginInit();
			this.SuspendLayout();
			// 
			// m_list
			// 
			this.m_list.AllowUserToAddRows = false;
			this.m_list.AllowUserToDeleteRows = false;
			this.m_list.AllowUserToOrderColumns = true;
			this.m_list.AllowUserToResizeRows = false;
			this.m_list.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.m_list.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.m_list.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_list.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_list.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.m_list.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_list.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.m_list.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLanguage,
            this.colRecordingProjectName,
            this.colBundleName,
            this.colInactive,
            this.colProjectPath});
			this.m_list.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_list.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_list.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
			this.m_list.Location = new System.Drawing.Point(0, 0);
			this.m_list.MultiSelect = false;
			this.m_list.Name = "m_list";
			this.m_list.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_list.RowHeadersVisible = false;
			this.m_list.RowHeadersWidth = 22;
			this.m_list.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_list.ShowEditingIcon = false;
			this.m_list.Size = new System.Drawing.Size(368, 147);
			this.m_list.TabIndex = 0;
			this.m_list.DoubleClick += new System.EventHandler(this.HandleDoubleClick);
			// 
			// colLanguage
			// 
			this.colLanguage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colLanguage.HeaderText = "Language";
			this.colLanguage.Name = "colLanguage";
			this.colLanguage.ReadOnly = true;
			this.colLanguage.Width = 84;
			// 
			// colRecordingProjectName
			// 
			this.colRecordingProjectName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colRecordingProjectName.HeaderText = "Recording Project";
			this.colRecordingProjectName.Name = "colRecordingProjectName";
			this.colRecordingProjectName.ReadOnly = true;
			this.colRecordingProjectName.Width = 115;
			// 
			// colBundleName
			// 
			this.colBundleName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colBundleName.HeaderText = "Original Bundle Filename";
			this.colBundleName.Name = "colBundleName";
			this.colBundleName.ReadOnly = true;
			// 
			// colInactive
			// 
			this.colInactive.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colInactive.HeaderText = "Inactive";
			this.colInactive.Name = "colInactive";
			this.colInactive.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.colInactive.Width = 73;
			// 
			// colProjectPath
			// 
			this.colProjectPath.HeaderText = "ProjectPath";
			this.colProjectPath.Name = "colProjectPath";
			this.colProjectPath.ReadOnly = true;
			this.colProjectPath.Visible = false;
			this.colProjectPath.Width = 93;
			// 
			// ExistingProjectsList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_list);
			this.DoubleBuffered = true;
			this.Name = "ExistingProjectsList";
			this.Size = new System.Drawing.Size(368, 147);
			((System.ComponentModel.ISupportInitialize)(this.m_list)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView m_list;
		private System.Windows.Forms.DataGridViewTextBoxColumn colLanguage;
		private System.Windows.Forms.DataGridViewTextBoxColumn colRecordingProjectName;
		private System.Windows.Forms.DataGridViewTextBoxColumn colBundleName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn colInactive;
		private System.Windows.Forms.DataGridViewTextBoxColumn colProjectPath;
	}
}
