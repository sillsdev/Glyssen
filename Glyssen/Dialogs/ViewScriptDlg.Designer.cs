namespace Glyssen.Dialogs
{
	partial class ViewScriptDlg
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
				L10NSharp.UI.LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
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
			this.m_L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.m_fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_exportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_exportToHearThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_dataGridView = new System.Windows.Forms.DataGridView();
			this.m_btnClose = new System.Windows.Forms.Button();
			this.m_lblLoading = new System.Windows.Forms.Label();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.m_subTableLayout = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).BeginInit();
			this.menuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.m_tableLayout.SuspendLayout();
			this.m_subTableLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_L10NSharpExtender
			// 
			this.m_L10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_L10NSharpExtender.PrefixForNewItems = "DialogBoxes.ViewScriptDlg";
			// 
			// menuStrip
			// 
			this.glyssenColorPalette.SetBackColor(this.menuStrip, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.menuStrip, Glyssen.Utilities.GlyssenColors.Default);
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_fileMenuItem});
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.menuStrip, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.menuStrip, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.menuStrip, "DialogBoxes.ViewScriptDlg.menuStrip1");
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(571, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip1";
			this.glyssenColorPalette.SetUsePaletteColors(this.menuStrip, false);
			// 
			// m_fileMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_fileMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_exportMenuItem,
            this.m_exportToHearThisToolStripMenuItem});
			this.glyssenColorPalette.SetForeColor(this.m_fileMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_fileMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_fileMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_fileMenuItem, "DialogBoxes.ViewScriptDlg.FileMenuItem");
			this.m_fileMenuItem.Name = "m_fileMenuItem";
			this.m_fileMenuItem.Size = new System.Drawing.Size(37, 20);
			this.m_fileMenuItem.Text = "File";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_fileMenuItem, false);
			// 
			// m_exportMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_exportMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_exportMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_exportMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_exportMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_exportMenuItem, "DialogBoxes.ViewScriptDlg.ExportToSpreadsheet");
			this.m_exportMenuItem.Name = "m_exportMenuItem";
			this.m_exportMenuItem.Size = new System.Drawing.Size(187, 22);
			this.m_exportMenuItem.Text = "Export to spreadsheet";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_exportMenuItem, false);
			this.m_exportMenuItem.Click += new System.EventHandler(this.m_exportMenuItem_Click);
			// 
			// m_exportToHearThisToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_exportToHearThisToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_exportToHearThisToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_exportToHearThisToolStripMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_exportToHearThisToolStripMenuItem, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_exportToHearThisToolStripMenuItem, "DialogBoxes.ViewScriptDlg.ExportToHearThis");
			this.m_exportToHearThisToolStripMenuItem.Name = "m_exportToHearThisToolStripMenuItem";
			this.m_exportToHearThisToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.m_exportToHearThisToolStripMenuItem.Text = "Export to {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_exportToHearThisToolStripMenuItem, false);
			this.m_exportToHearThisToolStripMenuItem.Click += new System.EventHandler(this.m_exportToHearThisToolStripMenuItem_Click);
			// 
			// m_dataGridView
			// 
			this.m_dataGridView.AllowUserToAddRows = false;
			this.m_dataGridView.AllowUserToDeleteRows = false;
			this.glyssenColorPalette.SetBackColor(this.m_dataGridView, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_dataGridView, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_dataGridView, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_dataGridView, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_dataGridView, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_dataGridView, "DialogBoxes.ViewScriptDlg.DataGrid");
			this.m_dataGridView.Location = new System.Drawing.Point(3, 3);
			this.m_dataGridView.Name = "m_dataGridView";
			this.m_dataGridView.ReadOnly = true;
			this.m_dataGridView.Size = new System.Drawing.Size(565, 344);
			this.m_dataGridView.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_dataGridView, false);
			// 
			// m_btnClose
			// 
			this.m_btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnClose, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnClose, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnClose, "Common.Close");
			this.m_btnClose.Location = new System.Drawing.Point(480, 10);
			this.m_btnClose.Margin = new System.Windows.Forms.Padding(10);
			this.m_btnClose.Name = "m_btnClose";
			this.m_btnClose.Size = new System.Drawing.Size(75, 23);
			this.m_btnClose.TabIndex = 6;
			this.m_btnClose.Text = "Close";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnClose, false);
			this.m_btnClose.UseVisualStyleBackColor = true;
			this.m_btnClose.Click += new System.EventHandler(this.m_btnClose_Click);
			// 
			// m_lblLoading
			// 
			this.m_lblLoading.AutoSize = true;
			this.m_lblLoading.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblLoading, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblLoading.Dock = System.Windows.Forms.DockStyle.Left;
			this.m_lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblLoading, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblLoading.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblLoading, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblLoading, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblLoading, "DialogBoxes.ViewScriptDlg.Loading");
			this.m_lblLoading.Location = new System.Drawing.Point(3, 0);
			this.m_lblLoading.Name = "m_lblLoading";
			this.m_lblLoading.Size = new System.Drawing.Size(64, 43);
			this.m_lblLoading.TabIndex = 7;
			this.m_lblLoading.Text = "Loading...";
			this.m_lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblLoading, true);
			// 
			// m_tableLayout
			// 
			this.glyssenColorPalette.SetBackColor(this.m_tableLayout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayout.ColumnCount = 1;
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayout.Controls.Add(this.m_dataGridView, 0, 0);
			this.m_tableLayout.Controls.Add(this.m_subTableLayout, 0, 1);
			this.m_tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayout, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayout.Location = new System.Drawing.Point(0, 24);
			this.m_tableLayout.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayout.Name = "m_tableLayout";
			this.m_tableLayout.RowCount = 2;
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.Size = new System.Drawing.Size(571, 399);
			this.m_tableLayout.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayout, false);
			// 
			// m_subTableLayout
			// 
			this.m_subTableLayout.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_subTableLayout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_subTableLayout.ColumnCount = 2;
			this.m_subTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_subTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_subTableLayout.Controls.Add(this.m_btnClose, 1, 0);
			this.m_subTableLayout.Controls.Add(this.m_lblLoading, 0, 0);
			this.m_subTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_subTableLayout, Glyssen.Utilities.GlyssenColors.Default);
			this.m_subTableLayout.Location = new System.Drawing.Point(3, 353);
			this.m_subTableLayout.Name = "m_subTableLayout";
			this.m_subTableLayout.RowCount = 1;
			this.m_subTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_subTableLayout.Size = new System.Drawing.Size(565, 43);
			this.m_subTableLayout.TabIndex = 2;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_subTableLayout, false);
			// 
			// ViewScriptDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(571, 423);
			this.Controls.Add(this.m_tableLayout);
			this.Controls.Add(this.menuStrip);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_L10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ViewScriptDlg.WindowTitle");
			this.MainMenuStrip = this.menuStrip;
			this.Name = "ViewScriptDlg";
			this.ShowInTaskbar = false;
			this.Text = "View Recording Script - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ViewScriptDlg_Load);
			this.Shown += new System.EventHandler(this.ViewScriptDlg_Shown);
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).EndInit();
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.m_tableLayout.ResumeLayout(false);
			this.m_tableLayout.PerformLayout();
			this.m_subTableLayout.ResumeLayout(false);
			this.m_subTableLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_L10NSharpExtender;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem m_fileMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_exportMenuItem;
		private System.Windows.Forms.TableLayoutPanel m_tableLayout;
		private System.Windows.Forms.DataGridView m_dataGridView;
		private System.Windows.Forms.TableLayoutPanel m_subTableLayout;
		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.Label m_lblLoading;
		private System.Windows.Forms.ToolStripMenuItem m_exportToHearThisToolStripMenuItem;
	}
}