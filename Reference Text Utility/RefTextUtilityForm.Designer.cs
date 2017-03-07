namespace Glyssen.ReferenceTextUtility
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
			this.m_lblProject = new System.Windows.Forms.Label();
			this.m_btnChooseProject = new System.Windows.Forms.Button();
			this.m_rdoSourceExistingProject = new System.Windows.Forms.RadioButton();
			this.m_lblSpreadsheetFilePath = new System.Windows.Forms.Label();
			this.m_btnSelectSpreadsheetFile = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.m_dataGridRefTexts = new System.Windows.Forms.DataGridView();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblLoading = new System.Windows.Forms.Label();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colAction = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.colDestination = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHeSaidText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colIsoCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridRefTexts)).BeginInit();
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
			this.groupBox2.Controls.Add(this.m_dataGridRefTexts);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox2.Location = new System.Drawing.Point(12, 118);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(703, 164);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Reference Text";
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
			this.m_dataGridRefTexts.Size = new System.Drawing.Size(690, 136);
			this.m_dataGridRefTexts.TabIndex = 0;
			this.m_dataGridRefTexts.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGridRefTexts_CellValueChanged);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnOk.Enabled = false;
			this.m_btnOk.Location = new System.Drawing.Point(285, 297);
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
			this.m_btnCancel.Location = new System.Drawing.Point(367, 297);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "Close";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_lblLoading
			// 
			this.m_lblLoading.AutoSize = true;
			this.m_lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblLoading.ForeColor = System.Drawing.Color.Blue;
			this.m_lblLoading.Location = new System.Drawing.Point(271, 120);
			this.m_lblLoading.Name = "m_lblLoading";
			this.m_lblLoading.Size = new System.Drawing.Size(71, 15);
			this.m_lblLoading.TabIndex = 6;
			this.m_lblLoading.Text = "Loading...";
			this.m_lblLoading.Visible = false;
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
			// RefTextUtilityForm
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(727, 333);
			this.Controls.Add(this.m_lblLoading);
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
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridRefTexts)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

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
	}
}

