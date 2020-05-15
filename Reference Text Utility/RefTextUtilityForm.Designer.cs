﻿namespace Glyssen.ReferenceTextUtility
{
	partial class RefTextUtilityForm
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RefTextUtilityForm));
			this.m_rdoSourceExcel = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.m_lblLoading = new System.Windows.Forms.Label();
			this.m_lblProject = new System.Windows.Forms.Label();
			this.m_btnChooseProject = new System.Windows.Forms.Button();
			this.m_rdoSourceExistingProject = new System.Windows.Forms.RadioButton();
			this.m_lblSpreadsheetFilePath = new System.Windows.Forms.Label();
			this.m_btnSelectSpreadsheetFile = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.m_btnSkipAll = new System.Windows.Forms.Button();
			this.m_dataGridRefTexts = new System.Windows.Forms.DataGridView();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colAction = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.colDestination = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHeSaidText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colIsoCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_grpComparisonSensitivity = new System.Windows.Forms.GroupBox();
			this.m_chkCurlyVsStraight = new System.Windows.Forms.CheckBox();
			this.m_chkSymbols = new System.Windows.Forms.CheckBox();
			this.m_chkPunctuation = new System.Windows.Forms.CheckBox();
			this.m_chkQuoteMarkDifferences = new System.Windows.Forms.CheckBox();
			this.m_chkWhitespace = new System.Windows.Forms.CheckBox();
			this.m_lblIgnore = new System.Windows.Forms.Label();
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridRefTexts)).BeginInit();
			this.m_grpComparisonSensitivity.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_rdoSourceExcel
			// 
			this.m_rdoSourceExcel.AutoSize = true;
			this.m_rdoSourceExcel.Checked = true;
			this.m_rdoSourceExcel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_rdoSourceExcel.Location = new System.Drawing.Point(6, 21);
			this.m_rdoSourceExcel.Name = "m_rdoSourceExcel";
			this.m_rdoSourceExcel.Size = new System.Drawing.Size(114, 17);
			this.m_rdoSourceExcel.TabIndex = 1;
			this.m_rdoSourceExcel.TabStop = true;
			this.m_rdoSourceExcel.Text = "Excel Spreadsheet";
			this.m_rdoSourceExcel.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.m_lblLoading);
			this.groupBox1.Controls.Add(this.m_lblProject);
			this.groupBox1.Controls.Add(this.m_btnChooseProject);
			this.groupBox1.Controls.Add(this.m_rdoSourceExistingProject);
			this.groupBox1.Controls.Add(this.m_lblSpreadsheetFilePath);
			this.groupBox1.Controls.Add(this.m_btnSelectSpreadsheetFile);
			this.groupBox1.Controls.Add(this.m_rdoSourceExcel);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(703, 86);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Source";
			// 
			// m_lblLoading
			// 
			this.m_lblLoading.AutoSize = true;
			this.m_lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblLoading.ForeColor = System.Drawing.Color.Blue;
			this.m_lblLoading.Location = new System.Drawing.Point(626, 21);
			this.m_lblLoading.Name = "m_lblLoading";
			this.m_lblLoading.Size = new System.Drawing.Size(71, 15);
			this.m_lblLoading.TabIndex = 6;
			this.m_lblLoading.Text = "Loading...";
			this.m_lblLoading.Visible = false;
			// 
			// m_lblProject
			// 
			this.m_lblProject.AutoSize = true;
			this.m_lblProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblProject.Location = new System.Drawing.Point(207, 57);
			this.m_lblProject.Name = "m_lblProject";
			this.m_lblProject.Size = new System.Drawing.Size(156, 13);
			this.m_lblProject.TabIndex = 6;
			this.m_lblProject.Text = "This option not yet implemented";
			// 
			// m_btnChooseProject
			// 
			this.m_btnChooseProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnChooseProject.Location = new System.Drawing.Point(126, 52);
			this.m_btnChooseProject.Name = "m_btnChooseProject";
			this.m_btnChooseProject.Size = new System.Drawing.Size(75, 23);
			this.m_btnChooseProject.TabIndex = 5;
			this.m_btnChooseProject.Text = "Select...";
			this.m_btnChooseProject.UseVisualStyleBackColor = true;
			// 
			// m_rdoSourceExistingProject
			// 
			this.m_rdoSourceExistingProject.AutoSize = true;
			this.m_rdoSourceExistingProject.Enabled = false;
			this.m_rdoSourceExistingProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_rdoSourceExistingProject.Location = new System.Drawing.Point(6, 55);
			this.m_rdoSourceExistingProject.Name = "m_rdoSourceExistingProject";
			this.m_rdoSourceExistingProject.Size = new System.Drawing.Size(98, 17);
			this.m_rdoSourceExistingProject.TabIndex = 4;
			this.m_rdoSourceExistingProject.Text = "Glyssen Project";
			this.m_rdoSourceExistingProject.UseVisualStyleBackColor = true;
			// 
			// m_lblSpreadsheetFilePath
			// 
			this.m_lblSpreadsheetFilePath.AutoSize = true;
			this.m_lblSpreadsheetFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblSpreadsheetFilePath.Location = new System.Drawing.Point(207, 23);
			this.m_lblSpreadsheetFilePath.Name = "m_lblSpreadsheetFilePath";
			this.m_lblSpreadsheetFilePath.Size = new System.Drawing.Size(14, 13);
			this.m_lblSpreadsheetFilePath.TabIndex = 3;
			this.m_lblSpreadsheetFilePath.Text = "#";
			this.m_lblSpreadsheetFilePath.TextChanged += new System.EventHandler(this.m_lblSpreadsheetFilePath_TextChanged);
			// 
			// m_btnSelectSpreadsheetFile
			// 
			this.m_btnSelectSpreadsheetFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_btnSelectSpreadsheetFile.Location = new System.Drawing.Point(126, 18);
			this.m_btnSelectSpreadsheetFile.Name = "m_btnSelectSpreadsheetFile";
			this.m_btnSelectSpreadsheetFile.Size = new System.Drawing.Size(75, 23);
			this.m_btnSelectSpreadsheetFile.TabIndex = 2;
			this.m_btnSelectSpreadsheetFile.Text = "Select...";
			this.m_btnSelectSpreadsheetFile.UseVisualStyleBackColor = true;
			this.m_btnSelectSpreadsheetFile.Click += new System.EventHandler(this.m_btnSelectSpreadsheetFile_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish);
			this.groupBox2.Controls.Add(this.m_btnSkipAll);
			this.groupBox2.Controls.Add(this.m_dataGridRefTexts);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox2.Location = new System.Drawing.Point(12, 166);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(703, 236);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Reference Text";
			// 
			// m_btnSkipAll
			// 
			this.m_btnSkipAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_btnSkipAll.AutoSize = true;
			this.m_btnSkipAll.Enabled = false;
			this.m_btnSkipAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_btnSkipAll.Location = new System.Drawing.Point(9, 207);
			this.m_btnSkipAll.Name = "m_btnSkipAll";
			this.m_btnSkipAll.Size = new System.Drawing.Size(132, 23);
			this.m_btnSkipAll.TabIndex = 1;
			this.m_btnSkipAll.Text = "Set all languages to skip";
			this.m_btnSkipAll.UseVisualStyleBackColor = true;
			this.m_btnSkipAll.Click += new System.EventHandler(this.m_btnSkipAll_Click);
			// 
			// m_dataGridRefTexts
			// 
			this.m_dataGridRefTexts.AllowUserToAddRows = false;
			this.m_dataGridRefTexts.AllowUserToDeleteRows = false;
			this.m_dataGridRefTexts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGridRefTexts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.m_dataGridRefTexts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colAction,
            this.colDestination,
            this.colHeSaidText,
            this.colIsoCode});
			this.m_dataGridRefTexts.Location = new System.Drawing.Point(7, 22);
			this.m_dataGridRefTexts.Name = "m_dataGridRefTexts";
			this.m_dataGridRefTexts.RowHeadersVisible = false;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_dataGridRefTexts.RowsDefaultCellStyle = dataGridViewCellStyle1;
			this.m_dataGridRefTexts.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_dataGridRefTexts.Size = new System.Drawing.Size(690, 178);
			this.m_dataGridRefTexts.TabIndex = 0;
			this.m_dataGridRefTexts.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGridRefTexts_CellValueChanged);
			// 
			// colName
			// 
			this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colName.FillWeight = 70F;
			this.colName.HeaderText = "Name";
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			this.colName.Width = 74;
			// 
			// colAction
			// 
			this.colAction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colAction.HeaderText = "Action";
			this.colAction.Items.AddRange(new object[] {
            "Create in Temp Folder",
            "Create/Overwrite",
            "Compare to Current",
            "Skip"});
			this.colAction.Name = "colAction";
			this.colAction.Width = 57;
			// 
			// colDestination
			// 
			this.colDestination.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colDestination.FillWeight = 130F;
			this.colDestination.HeaderText = "Destination Path";
			this.colDestination.MaxInputLength = 256;
			this.colDestination.Name = "colDestination";
			this.colDestination.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// colHeSaidText
			// 
			this.colHeSaidText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colHeSaidText.HeaderText = "“He said” Text";
			this.colHeSaidText.Name = "colHeSaidText";
			this.colHeSaidText.Width = 131;
			// 
			// colIsoCode
			// 
			this.colIsoCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colIsoCode.HeaderText = "ISO Code";
			this.colIsoCode.Name = "colIsoCode";
			this.colIsoCode.Width = 99;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnOk.Enabled = false;
			this.m_btnOk.Location = new System.Drawing.Point(285, 417);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 4;
			this.m_btnOk.Text = "Process...";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnProcess_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(367, 417);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "Close";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_grpComparisonSensitivity
			// 
			this.m_grpComparisonSensitivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_grpComparisonSensitivity.Controls.Add(this.m_chkCurlyVsStraight);
			this.m_grpComparisonSensitivity.Controls.Add(this.m_chkSymbols);
			this.m_grpComparisonSensitivity.Controls.Add(this.m_chkPunctuation);
			this.m_grpComparisonSensitivity.Controls.Add(this.m_chkQuoteMarkDifferences);
			this.m_grpComparisonSensitivity.Controls.Add(this.m_chkWhitespace);
			this.m_grpComparisonSensitivity.Controls.Add(this.m_lblIgnore);
			this.m_grpComparisonSensitivity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
			this.m_grpComparisonSensitivity.Location = new System.Drawing.Point(12, 105);
			this.m_grpComparisonSensitivity.Name = "m_grpComparisonSensitivity";
			this.m_grpComparisonSensitivity.Size = new System.Drawing.Size(703, 55);
			this.m_grpComparisonSensitivity.TabIndex = 6;
			this.m_grpComparisonSensitivity.TabStop = false;
			this.m_grpComparisonSensitivity.Text = "Comparison Sensitivity";
			// 
			// m_chkCurlyVsStraight
			// 
			this.m_chkCurlyVsStraight.AutoSize = true;
			this.m_chkCurlyVsStraight.Checked = true;
			this.m_chkCurlyVsStraight.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_chkCurlyVsStraight.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkCurlyVsStraight.Location = new System.Drawing.Point(200, 26);
			this.m_chkCurlyVsStraight.Name = "m_chkCurlyVsStraight";
			this.m_chkCurlyVsStraight.Size = new System.Drawing.Size(142, 17);
			this.m_chkCurlyVsStraight.TabIndex = 5;
			this.m_chkCurlyVsStraight.Text = "Curly vs. Straight Quotes";
			this.m_chkCurlyVsStraight.UseVisualStyleBackColor = true;
			// 
			// m_chkSymbols
			// 
			this.m_chkSymbols.AutoSize = true;
			this.m_chkSymbols.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkSymbols.Location = new System.Drawing.Point(624, 26);
			this.m_chkSymbols.Name = "m_chkSymbols";
			this.m_chkSymbols.Size = new System.Drawing.Size(65, 17);
			this.m_chkSymbols.TabIndex = 4;
			this.m_chkSymbols.Text = "Symbols";
			this.m_chkSymbols.UseVisualStyleBackColor = true;
			// 
			// m_chkPunctuation
			// 
			this.m_chkPunctuation.AutoSize = true;
			this.m_chkPunctuation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkPunctuation.Location = new System.Drawing.Point(517, 26);
			this.m_chkPunctuation.Name = "m_chkPunctuation";
			this.m_chkPunctuation.Size = new System.Drawing.Size(96, 17);
			this.m_chkPunctuation.TabIndex = 3;
			this.m_chkPunctuation.Text = "All punctuation";
			this.m_chkPunctuation.UseVisualStyleBackColor = true;
			this.m_chkPunctuation.CheckedChanged += new System.EventHandler(this.m_chkPunctuation_CheckedChanged);
			// 
			// m_chkQuoteMarkDifferences
			// 
			this.m_chkQuoteMarkDifferences.AutoSize = true;
			this.m_chkQuoteMarkDifferences.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkQuoteMarkDifferences.Location = new System.Drawing.Point(353, 26);
			this.m_chkQuoteMarkDifferences.Name = "m_chkQuoteMarkDifferences";
			this.m_chkQuoteMarkDifferences.Size = new System.Drawing.Size(153, 17);
			this.m_chkQuoteMarkDifferences.TabIndex = 2;
			this.m_chkQuoteMarkDifferences.Text = "Quotation mark differences";
			this.m_chkQuoteMarkDifferences.UseVisualStyleBackColor = true;
			this.m_chkQuoteMarkDifferences.CheckedChanged += new System.EventHandler(this.m_chkQuoteMarkDifferences_CheckedChanged);
			// 
			// m_chkWhitespace
			// 
			this.m_chkWhitespace.AutoSize = true;
			this.m_chkWhitespace.Checked = true;
			this.m_chkWhitespace.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_chkWhitespace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkWhitespace.Location = new System.Drawing.Point(51, 26);
			this.m_chkWhitespace.Name = "m_chkWhitespace";
			this.m_chkWhitespace.Size = new System.Drawing.Size(138, 17);
			this.m_chkWhitespace.TabIndex = 1;
			this.m_chkWhitespace.Text = "Whitespace differences";
			this.m_chkWhitespace.UseVisualStyleBackColor = true;
			// 
			// m_lblIgnore
			// 
			this.m_lblIgnore.AutoSize = true;
			this.m_lblIgnore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_lblIgnore.Location = new System.Drawing.Point(6, 27);
			this.m_lblIgnore.Name = "m_lblIgnore";
			this.m_lblIgnore.Size = new System.Drawing.Size(40, 13);
			this.m_lblIgnore.TabIndex = 0;
			this.m_lblIgnore.Text = "Ignore:";
			// 
			// m_chkAttemptToRegularizeQuotationMarksToMatchEnglish
			// 
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.AutoSize = true;
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Checked = true;
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Location = new System.Drawing.Point(200, 211);
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Name = "m_chkAttemptToRegularizeQuotationMarksToMatchEnglish";
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Size = new System.Drawing.Size(254, 17);
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.TabIndex = 2;
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.Text = "Attempt to add quotation marks to match English";
			this.m_chkAttemptToRegularizeQuotationMarksToMatchEnglish.UseVisualStyleBackColor = true;
			// 
			// RefTextUtilityForm
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(727, 453);
			this.Controls.Add(this.m_grpComparisonSensitivity);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RefTextUtilityForm";
			this.Text = "Proprietary Reference Text Creation Utility";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridRefTexts)).EndInit();
			this.m_grpComparisonSensitivity.ResumeLayout(false);
			this.m_grpComparisonSensitivity.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton m_rdoSourceExcel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label m_lblProject;
		private System.Windows.Forms.Button m_btnChooseProject;
		private System.Windows.Forms.RadioButton m_rdoSourceExistingProject;
		private System.Windows.Forms.Label m_lblSpreadsheetFilePath;
		private System.Windows.Forms.Button m_btnSelectSpreadsheetFile;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.DataGridView m_dataGridRefTexts;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblLoading;
		private System.Windows.Forms.DataGridViewTextBoxColumn colName;
		private System.Windows.Forms.DataGridViewComboBoxColumn colAction;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDestination;
		private System.Windows.Forms.DataGridViewTextBoxColumn colHeSaidText;
		private System.Windows.Forms.DataGridViewTextBoxColumn colIsoCode;
		private System.Windows.Forms.GroupBox m_grpComparisonSensitivity;
		private System.Windows.Forms.Label m_lblIgnore;
		private System.Windows.Forms.CheckBox m_chkWhitespace;
		private System.Windows.Forms.CheckBox m_chkPunctuation;
		private System.Windows.Forms.CheckBox m_chkQuoteMarkDifferences;
		private System.Windows.Forms.CheckBox m_chkSymbols;
		private System.Windows.Forms.Button m_btnSkipAll;
		private System.Windows.Forms.CheckBox m_chkCurlyVsStraight;
		private System.Windows.Forms.CheckBox m_chkAttemptToRegularizeQuotationMarksToMatchEnglish;
	}
}

