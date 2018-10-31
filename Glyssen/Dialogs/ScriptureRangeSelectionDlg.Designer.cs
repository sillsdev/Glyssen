using System.Windows.Forms;

namespace Glyssen.Dialogs
{
	partial class ScriptureRangeSelectionDlg
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
			L10NSharp.UI.LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
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
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_ntBooksGrid = new System.Windows.Forms.DataGridView();
			this.m_checkBoxNewTestament = new System.Windows.Forms.CheckBox();
			this.m_checkBoxOldTestament = new System.Windows.Forms.CheckBox();
			this.m_otBooksGrid = new System.Windows.Forms.DataGridView();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_colNTBookCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colVernacularNTBookName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colIncludeNTBookInScript = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.m_colNTMultiVoice = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.m_colOTBookCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colVernacularOTBookName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_colIncludeOTBookInScript = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.m_colOTMultiVoice = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_ntBooksGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_otBooksGrid)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.ScriptureRangeSelectionDlg";
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(580, 467);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(661, 467);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_ntBooksGrid
			// 
			this.m_ntBooksGrid.AllowUserToAddRows = false;
			this.m_ntBooksGrid.AllowUserToDeleteRows = false;
			this.m_ntBooksGrid.AllowUserToResizeRows = false;
			this.m_ntBooksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_ntBooksGrid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_ntBooksGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_ntBooksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_ntBooksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_colNTBookCode,
            this.m_colVernacularNTBookName,
            this.m_colIncludeNTBookInScript,
            this.m_colNTMultiVoice});
			this.glyssenColorPalette.SetForeColor(this.m_ntBooksGrid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_ntBooksGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_ntBooksGrid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_ntBooksGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_ntBooksGrid, "DialogBoxes.ScriptureRangeSelectionDlg.dataGridView1");
			this.m_ntBooksGrid.Location = new System.Drawing.Point(365, 26);
			this.m_ntBooksGrid.MultiSelect = false;
			this.m_ntBooksGrid.Name = "m_ntBooksGrid";
			this.m_ntBooksGrid.RowHeadersVisible = false;
			this.m_ntBooksGrid.Size = new System.Drawing.Size(356, 411);
			this.m_ntBooksGrid.TabIndex = 3;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_ntBooksGrid, false);
			this.m_ntBooksGrid.VirtualMode = true;
			this.m_ntBooksGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BooksGrid_CellContentClick);
			this.m_ntBooksGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.BooksGrid_CellFormatting);
			this.m_ntBooksGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.BooksGrid_CellPainting);
			this.m_ntBooksGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.BooksGrid_CellValidating);
			this.m_ntBooksGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValueNeeded);
			this.m_ntBooksGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValuePushed);
			this.m_ntBooksGrid.SelectionChanged += new System.EventHandler(this.BooksGrid_SelectionChanged);
			// 
			// m_checkBoxNewTestament
			// 
			this.m_checkBoxNewTestament.AutoSize = true;
			this.m_checkBoxNewTestament.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkBoxNewTestament, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_checkBoxNewTestament.Checked = true;
			this.m_checkBoxNewTestament.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_checkBoxNewTestament.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkBoxNewTestament, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkBoxNewTestament.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkBoxNewTestament, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkBoxNewTestament, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkBoxNewTestament, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkBoxNewTestament, "DialogBoxes.ScriptureRangeSelectionDlg.NewTestament");
			this.m_checkBoxNewTestament.Location = new System.Drawing.Point(365, 3);
			this.m_checkBoxNewTestament.Name = "m_checkBoxNewTestament";
			this.m_checkBoxNewTestament.Size = new System.Drawing.Size(101, 17);
			this.m_checkBoxNewTestament.TabIndex = 4;
			this.m_checkBoxNewTestament.Text = "New Testament";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkBoxNewTestament, true);
			this.m_checkBoxNewTestament.UseVisualStyleBackColor = true;
			this.m_checkBoxNewTestament.CheckedChanged += new System.EventHandler(this.CheckBoxNewTestament_CheckedChanged);
			// 
			// m_checkBoxOldTestament
			// 
			this.m_checkBoxOldTestament.AutoSize = true;
			this.m_checkBoxOldTestament.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkBoxOldTestament, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_checkBoxOldTestament.Checked = true;
			this.m_checkBoxOldTestament.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_checkBoxOldTestament.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkBoxOldTestament, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkBoxOldTestament.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkBoxOldTestament, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkBoxOldTestament, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkBoxOldTestament, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkBoxOldTestament, "DialogBoxes.ScriptureRangeSelectionDlg.OldTestament");
			this.m_checkBoxOldTestament.Location = new System.Drawing.Point(3, 3);
			this.m_checkBoxOldTestament.Name = "m_checkBoxOldTestament";
			this.m_checkBoxOldTestament.Size = new System.Drawing.Size(95, 17);
			this.m_checkBoxOldTestament.TabIndex = 5;
			this.m_checkBoxOldTestament.Text = "Old Testament";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkBoxOldTestament, true);
			this.m_checkBoxOldTestament.UseVisualStyleBackColor = true;
			this.m_checkBoxOldTestament.CheckedChanged += new System.EventHandler(this.CheckBoxOldTestament_CheckedChanged);
			// 
			// m_otBooksGrid
			// 
			this.m_otBooksGrid.AllowUserToAddRows = false;
			this.m_otBooksGrid.AllowUserToDeleteRows = false;
			this.m_otBooksGrid.AllowUserToResizeRows = false;
			this.m_otBooksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_otBooksGrid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_otBooksGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_otBooksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_otBooksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_colOTBookCode,
            this.m_colVernacularOTBookName,
            this.m_colIncludeOTBookInScript,
            this.m_colOTMultiVoice});
			this.glyssenColorPalette.SetForeColor(this.m_otBooksGrid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_otBooksGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_otBooksGrid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_otBooksGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_otBooksGrid, "DialogBoxes.ScriptureRangeSelectionDlg.dataGridView1");
			this.m_otBooksGrid.Location = new System.Drawing.Point(3, 26);
			this.m_otBooksGrid.MultiSelect = false;
			this.m_otBooksGrid.Name = "m_otBooksGrid";
			this.m_otBooksGrid.RowHeadersVisible = false;
			this.m_otBooksGrid.Size = new System.Drawing.Size(356, 411);
			this.m_otBooksGrid.TabIndex = 6;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_otBooksGrid, false);
			this.m_otBooksGrid.VirtualMode = true;
			this.m_otBooksGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BooksGrid_CellContentClick);
			this.m_otBooksGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.BooksGrid_CellFormatting);
			this.m_otBooksGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.BooksGrid_CellPainting);
			this.m_ntBooksGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.BooksGrid_CellValidating);
			this.m_otBooksGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValueNeeded);
			this.m_otBooksGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValuePushed);
			this.m_otBooksGrid.SelectionChanged += new System.EventHandler(this.BooksGrid_SelectionChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.m_otBooksGrid, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_checkBoxOldTestament, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_ntBooksGrid, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_checkBoxNewTestament, 1, 0);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(724, 440);
			this.tableLayoutPanel1.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// m_colNTBookCode
			// 
			this.m_colNTBookCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.m_colNTBookCode.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.Book!Book";
			this.m_colNTBookCode.Name = "m_colNTBookCode";
			this.m_colNTBookCode.ReadOnly = true;
			this.m_colNTBookCode.Width = 325;
			// 
			// m_colVernacularNTBookName
			// 
			this.m_colVernacularNTBookName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.m_colVernacularNTBookName.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.VernacularName!Vernacular Name";
			this.m_colVernacularNTBookName.Name = "m_colVernacularNTBookName";
			this.m_colVernacularNTBookName.ReadOnly = true;
			// 
			// m_colIncludeNTBookInScript
			// 
			this.m_colIncludeNTBookInScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.m_colIncludeNTBookInScript.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.IncludeInScript!Include in Script";
			this.m_colIncludeNTBookInScript.Name = "m_colIncludeNTBookInScript";
			this.m_colIncludeNTBookInScript.Width = 75;
			// 
			// m_colNTMultiVoice
			// 
			this.m_colNTMultiVoice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.m_colNTMultiVoice.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.MultipleVoiceActors!Multiple Voice " +
    "Actors";
			this.m_colNTMultiVoice.Name = "m_colNTMultiVoice";
			this.m_colNTMultiVoice.Width = 75;
			// 
			// m_colOTBookCode
			// 
			this.m_colOTBookCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.m_colOTBookCode.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.Book!Book";
			this.m_colOTBookCode.Name = "m_colOTBookCode";
			this.m_colOTBookCode.ReadOnly = true;
			this.m_colOTBookCode.Width = 325;
			// 
			// m_colVernacularOTBookName
			// 
			this.m_colVernacularOTBookName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.m_colVernacularOTBookName.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.VernacularName!Vernacular Name";
			this.m_colVernacularOTBookName.Name = "m_colVernacularOTBookName";
			this.m_colVernacularOTBookName.ReadOnly = true;
			// 
			// m_colIncludeOTBookInScript
			// 
			this.m_colIncludeOTBookInScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.m_colIncludeOTBookInScript.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.IncludeInScript!Include in Script";
			this.m_colIncludeOTBookInScript.Name = "m_colIncludeOTBookInScript";
			this.m_colIncludeOTBookInScript.Width = 75;
			// 
			// m_colOTMultiVoice
			// 
			this.m_colOTMultiVoice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.m_colOTMultiVoice.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.MultipleVoiceActors!Multiple Voice " +
    "Actors";
			this.m_colOTMultiVoice.Name = "m_colOTMultiVoice";
			this.m_colOTMultiVoice.Width = 75;
			// 
			// ScriptureRangeSelectionDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(748, 502);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ScriptureRangeSelectionDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(596, 383);
			this.Name = "ScriptureRangeSelectionDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Books - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ScriptureRangeSelectionDlg_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_ntBooksGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_otBooksGrid)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.DataGridView m_ntBooksGrid;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.DataGridView m_otBooksGrid;
		private System.Windows.Forms.CheckBox m_checkBoxOldTestament;
		private System.Windows.Forms.CheckBox m_checkBoxNewTestament;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colNTBookCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colVernacularNTBookName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn m_colIncludeNTBookInScript;
		private System.Windows.Forms.DataGridViewCheckBoxColumn m_colNTMultiVoice;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colOTBookCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn m_colVernacularOTBookName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn m_colIncludeOTBookInScript;
		private System.Windows.Forms.DataGridViewCheckBoxColumn m_colOTMultiVoice;
	}
}
