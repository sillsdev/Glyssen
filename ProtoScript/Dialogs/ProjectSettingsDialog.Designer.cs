namespace ProtoScript.Dialogs
{
	partial class ProjectSettingsDialog
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_comboQuoteMarks = new System.Windows.Forms.ComboBox();
			this.comboBox3 = new System.Windows.Forms.ComboBox();
			this.comboBox4 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(328, 191);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 0;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
			// 
			// m_btnOk
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(242, 191);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "ProjectSettingsDialog";
			// 
			// m_comboQuoteMarks
			// 
			this.m_comboQuoteMarks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_comboQuoteMarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_comboQuoteMarks.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_comboQuoteMarks, "ProjectSettingsDialog.comboBox1");
			this.m_comboQuoteMarks.Location = new System.Drawing.Point(23, 39);
			this.m_comboQuoteMarks.MaxDropDownItems = 15;
			this.m_comboQuoteMarks.Name = "m_comboQuoteMarks";
			this.m_comboQuoteMarks.Size = new System.Drawing.Size(81, 32);
			this.m_comboQuoteMarks.TabIndex = 1;
			// 
			// comboBox3
			// 
			this.comboBox3.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.comboBox3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.comboBox3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.comboBox3, "ProjectSettingsDialog.comboBox1");
			this.comboBox3.Location = new System.Drawing.Point(190, 39);
			this.comboBox3.Name = "comboBox3";
			this.comboBox3.Size = new System.Drawing.Size(121, 21);
			this.comboBox3.TabIndex = 3;
			// 
			// comboBox4
			// 
			this.comboBox4.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.comboBox4, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.comboBox4, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.comboBox4, "ProjectSettingsDialog.comboBox1");
			this.comboBox4.Location = new System.Drawing.Point(317, 39);
			this.comboBox4.Name = "comboBox4";
			this.comboBox4.Size = new System.Drawing.Size(86, 21);
			this.comboBox4.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label1, "ProjectSettingsDialog.QuoteMarks");
			this.label1.Location = new System.Drawing.Point(23, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Quote Marks:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label3, "ProjectSettingsDialog.DefineRange");
			this.label3.Location = new System.Drawing.Point(187, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Define Range:";
			// 
			// ProjectSettingsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 237);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBox4);
			this.Controls.Add(this.comboBox3);
			this.Controls.Add(this.m_comboQuoteMarks);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "ProjectSettingsDialog.ProjectSettings");
			this.Name = "ProjectSettingsDialog";
			this.Text = "Project Settings";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.ComboBox m_comboQuoteMarks;
		private System.Windows.Forms.ComboBox comboBox3;
		private System.Windows.Forms.ComboBox comboBox4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
	}
}