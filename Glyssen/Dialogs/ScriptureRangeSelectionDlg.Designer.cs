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
			this.BookCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VernacularBookName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IncludeInScript = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.MultiVoice = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.m_checkBoxNewTestament = new System.Windows.Forms.CheckBox();
			this.m_checkBoxOldTestament = new System.Windows.Forms.CheckBox();
			this.m_otBooksGrid = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_ntBooksGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_otBooksGrid)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
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
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(580, 467);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(661, 467);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
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
			this.m_ntBooksGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_ntBooksGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_ntBooksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_ntBooksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BookCode,
            this.VernacularBookName,
            this.IncludeInScript,
            this.MultiVoice});
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
			this.m_ntBooksGrid.VirtualMode = true;
			this.m_ntBooksGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BooksGrid_CellContentClick);
			this.m_ntBooksGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.BooksGrid_CellFormatting);
			this.m_ntBooksGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.BooksGrid_CellPainting);
			this.m_ntBooksGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValueNeeded);
			this.m_ntBooksGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValuePushed);
			this.m_ntBooksGrid.SelectionChanged += new System.EventHandler(this.BooksGrid_SelectionChanged);
			// 
			// BookCode
			// 
			this.BookCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.BookCode.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.Book!Book";
			this.BookCode.Name = "BookCode";
			this.BookCode.ReadOnly = true;
			this.BookCode.Width = 325;
			// 
			// VernacularBookName
			// 
			this.VernacularBookName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.VernacularBookName.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.VernacularName!Vernacular Name";
			this.VernacularBookName.Name = "VernacularBookName";
			this.VernacularBookName.ReadOnly = true;
			// 
			// IncludeInScript
			// 
			this.IncludeInScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.IncludeInScript.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.IncludeInScript!Include in Script";
			this.IncludeInScript.Name = "IncludeInScript";
			this.IncludeInScript.Width = 75;
			// 
			// MultiVoice
			// 
			this.MultiVoice.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.MultiVoice.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.MultipleVoiceActors!Multiple Voice " +
    "Actors";
			this.MultiVoice.Name = "MultiVoice";
			this.MultiVoice.Width = 75;
			// 
			// m_checkBoxNewTestament
			// 
			this.m_checkBoxNewTestament.AutoSize = true;
			this.m_checkBoxNewTestament.Checked = true;
			this.m_checkBoxNewTestament.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_checkBoxNewTestament.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkBoxNewTestament, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkBoxNewTestament, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkBoxNewTestament, "DialogBoxes.ScriptureRangeSelectionDlg.NewTestament");
			this.m_checkBoxNewTestament.Location = new System.Drawing.Point(365, 3);
			this.m_checkBoxNewTestament.Name = "m_checkBoxNewTestament";
			this.m_checkBoxNewTestament.Size = new System.Drawing.Size(101, 17);
			this.m_checkBoxNewTestament.TabIndex = 4;
			this.m_checkBoxNewTestament.Text = "New Testament";
			this.m_checkBoxNewTestament.UseVisualStyleBackColor = true;
			this.m_checkBoxNewTestament.CheckedChanged += new System.EventHandler(this.CheckBoxNewTestament_CheckedChanged);
			// 
			// m_checkBoxOldTestament
			// 
			this.m_checkBoxOldTestament.AutoSize = true;
			this.m_checkBoxOldTestament.Checked = true;
			this.m_checkBoxOldTestament.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_checkBoxOldTestament.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkBoxOldTestament, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkBoxOldTestament, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkBoxOldTestament, "DialogBoxes.ScriptureRangeSelectionDlg.OldTestament");
			this.m_checkBoxOldTestament.Location = new System.Drawing.Point(3, 3);
			this.m_checkBoxOldTestament.Name = "m_checkBoxOldTestament";
			this.m_checkBoxOldTestament.Size = new System.Drawing.Size(95, 17);
			this.m_checkBoxOldTestament.TabIndex = 5;
			this.m_checkBoxOldTestament.Text = "Old Testament";
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
			this.m_otBooksGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_otBooksGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_otBooksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_otBooksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewCheckBoxColumn2});
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
			this.m_otBooksGrid.VirtualMode = true;
			this.m_otBooksGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BooksGrid_CellContentClick);
			this.m_otBooksGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.BooksGrid_CellFormatting);
			this.m_otBooksGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.BooksGrid_CellPainting);
			this.m_otBooksGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValueNeeded);
			this.m_otBooksGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.BooksGrid_CellValuePushed);
			this.m_otBooksGrid.SelectionChanged += new System.EventHandler(this.BooksGrid_SelectionChanged);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.Book!Book";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.Width = 325;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn2.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.VernacularName!Vernacular Name";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// dataGridViewCheckBoxColumn1
			// 
			this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.dataGridViewCheckBoxColumn1.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.IncludeInScript!Include in Script";
			this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
			this.dataGridViewCheckBoxColumn1.Width = 75;
			// 
			// dataGridViewCheckBoxColumn2
			// 
			this.dataGridViewCheckBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.dataGridViewCheckBoxColumn2.HeaderText = "_L10N_:DialogBoxes.ScriptureRangeSelectionDlg.MultipleVoiceActors!Multiple Voice " +
    "Actors";
			this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
			this.dataGridViewCheckBoxColumn2.Width = 75;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.m_otBooksGrid, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_checkBoxOldTestament, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_ntBooksGrid, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_checkBoxNewTestament, 1, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(724, 440);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// ScriptureRangeSelectionDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(748, 502);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ScriptureRangeSelectionDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(596, 383);
			this.Name = "ScriptureRangeSelectionDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Books";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_ntBooksGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_otBooksGrid)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
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
		private System.Windows.Forms.DataGridViewTextBoxColumn BookCode;
		private System.Windows.Forms.DataGridViewTextBoxColumn VernacularBookName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn IncludeInScript;
		private System.Windows.Forms.DataGridViewCheckBoxColumn MultiVoice;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
	}
}