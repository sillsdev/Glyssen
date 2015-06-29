namespace Glyssen.Dialogs
{
	partial class VoiceActorInformationDlg
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorInformationDlg));
			this.label1 = new System.Windows.Forms.Label();
			this.m_btnSave = new System.Windows.Forms.Button();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_dataGridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_deleteRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_dataGrid = new Glyssen.Controls.DataGridViewOverrideEnter();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.m_dataGridContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(125, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter Voice Actors";
			// 
			// m_btnSave
			// 
			this.m_btnSave.Location = new System.Drawing.Point(209, 293);
			this.m_btnSave.Name = "m_btnSave";
			this.m_btnSave.Size = new System.Drawing.Size(75, 23);
			this.m_btnSave.TabIndex = 1;
			this.m_btnSave.Text = "Save";
			this.m_btnSave.UseVisualStyleBackColor = true;
			// 
			// m_btnNext
			// 
			this.m_btnNext.Enabled = false;
			this.m_btnNext.Location = new System.Drawing.Point(336, 293);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 2;
			this.m_btnNext.Text = "Next";
			this.m_btnNext.UseVisualStyleBackColor = true;
			// 
			// m_dataGridContextMenu
			// 
			this.m_dataGridContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_deleteRowsToolStripMenuItem});
			this.m_dataGridContextMenu.Name = "m_dataGridContextMenu";
			this.m_dataGridContextMenu.ShowImageMargin = false;
			this.m_dataGridContextMenu.Size = new System.Drawing.Size(122, 26);
			this.m_dataGridContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.m_dataGridContextMenu_ItemClicked);
			// 
			// m_deleteRowsToolStripMenuItem
			// 
			this.m_deleteRowsToolStripMenuItem.Name = "m_deleteRowsToolStripMenuItem";
			this.m_deleteRowsToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.m_deleteRowsToolStripMenuItem.Text = "Delete Row(s)";
			// 
			// m_dataGrid
			// 
			this.m_dataGrid.AllowUserToDeleteRows = false;
			this.m_dataGrid.AllowUserToOrderColumns = true;
			this.m_dataGrid.AllowUserToResizeColumns = false;
			this.m_dataGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
			this.m_dataGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.m_dataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.m_dataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_dataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.m_dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3});
			this.m_dataGrid.ContextMenuStrip = this.m_dataGridContextMenu;
			this.m_dataGrid.DrawTextBoxEditControlBorder = false;
			this.m_dataGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_dataGrid.FullRowFocusRectangleColor = System.Drawing.Color.Empty;
			this.m_dataGrid.GridColor = System.Drawing.Color.Black;
			this.m_dataGrid.Location = new System.Drawing.Point(42, 41);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_dataGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_dataGrid.RowHeadersVisible = false;
			this.m_dataGrid.RowHeadersWidth = 22;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.Silver;
			dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
			this.m_dataGrid.RowsDefaultCellStyle = dataGridViewCellStyle4;
			this.m_dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGrid.SelectedCellBackColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedCellForeColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedRowBackColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedRowForeColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGrid.ShowWaterMarkWhenDirty = false;
			this.m_dataGrid.Size = new System.Drawing.Size(401, 246);
			this.m_dataGrid.TabIndex = 4;
			this.m_dataGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Empty;
			this.m_dataGrid.WaterMark = "!";
			this.m_dataGrid.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.m_dataGrid_RowsAdded);
			this.m_dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_dataGrid_KeyDown);
			// 
			// Column1
			// 
			this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column1.HeaderText = "Name";
			this.Column1.Name = "Column1";
			this.Column1.ToolTipText = "Enter Voice Actor first and last name";
			// 
			// Column2
			// 
			this.Column2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Column2.HeaderText = "Gender";
			this.Column2.Items.AddRange(new object[] {
            "F - Female",
            "M - Male"});
			this.Column2.Name = "Column2";
			this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// Column3
			// 
			dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
			this.Column3.DefaultCellStyle = dataGridViewCellStyle3;
			this.Column3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Column3.HeaderText = "Age";
			this.Column3.Items.AddRange(new object[] {
            "O - Old",
            "Y - Young",
            "C - Child"});
			this.Column3.Name = "Column3";
			this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// VoiceActorInformationDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(500, 334);
			this.Controls.Add(this.m_dataGrid);
			this.Controls.Add(this.m_btnNext);
			this.Controls.Add(this.m_btnSave);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "VoiceActorInformationDlg";
			this.Text = "Voice Actor Information";
			this.m_dataGridContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_dataGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button m_btnSave;
		private System.Windows.Forms.Button m_btnNext;
		private Controls.DataGridViewOverrideEnter m_dataGrid;
		private System.Windows.Forms.ContextMenuStrip m_dataGridContextMenu;
		private System.Windows.Forms.ToolStripMenuItem m_deleteRowsToolStripMenuItem;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		private System.Windows.Forms.DataGridViewComboBoxColumn Column2;
		private System.Windows.Forms.DataGridViewComboBoxColumn Column3;
	}
}