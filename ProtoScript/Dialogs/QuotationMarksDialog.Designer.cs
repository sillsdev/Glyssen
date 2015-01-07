namespace ProtoScript.Dialogs
{
	partial class QuotationMarksDialog
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
			this.m_lblQuotationMarks = new System.Windows.Forms.Label();
			this.m_lblHorizontalSeparator = new System.Windows.Forms.Label();
			this.m_chkDialogueQuotations = new System.Windows.Forms.CheckBox();
			this.m_cboQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_cboEndQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_lblEndDialogueQuote = new System.Windows.Forms.Label();
			this.m_chkAlternateSpeakersInFirstLevelQuotes = new System.Windows.Forms.CheckBox();
			this.m_pnlDialogeQuotes = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_pnlDialogeQuotes.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(406, 202);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 0;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(320, 202);
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
			this.m_l10NSharpExtender.PrefixForNewItems = "QuotationMarksDialog";
			// 
			// m_comboQuoteMarks
			// 
			this.m_comboQuoteMarks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_comboQuoteMarks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.m_comboQuoteMarks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_comboQuoteMarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_comboQuoteMarks.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_comboQuoteMarks, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_comboQuoteMarks, "QuotationMarksDialog.QuotationMarksDialog.m_comboQuoteMarks");
			this.m_comboQuoteMarks.Location = new System.Drawing.Point(161, 12);
			this.m_comboQuoteMarks.MaxDropDownItems = 15;
			this.m_comboQuoteMarks.Name = "m_comboQuoteMarks";
			this.m_comboQuoteMarks.Size = new System.Drawing.Size(320, 30);
			this.m_comboQuoteMarks.TabIndex = 1;
			this.m_comboQuoteMarks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandlecomboQuoteMarksDrawItem);
			// 
			// m_lblQuotationMarks
			// 
			this.m_lblQuotationMarks.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblQuotationMarks, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuotationMarks, "QuotationMarksDialog.QuotationMarksDialog.m_lblQuotationMarks");
			this.m_lblQuotationMarks.Location = new System.Drawing.Point(20, 23);
			this.m_lblQuotationMarks.Name = "m_lblQuotationMarks";
			this.m_lblQuotationMarks.Size = new System.Drawing.Size(132, 13);
			this.m_lblQuotationMarks.TabIndex = 6;
			this.m_lblQuotationMarks.Text = "First-level quotation marks:";
			// 
			// m_lblHorizontalSeparator
			// 
			this.m_lblHorizontalSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblHorizontalSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblHorizontalSeparator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblHorizontalSeparator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblHorizontalSeparator, "ProjectSettingsDialog.label1");
			this.m_lblHorizontalSeparator.Location = new System.Drawing.Point(20, 75);
			this.m_lblHorizontalSeparator.Name = "m_lblHorizontalSeparator";
			this.m_lblHorizontalSeparator.Size = new System.Drawing.Size(461, 2);
			this.m_lblHorizontalSeparator.TabIndex = 8;
			// 
			// m_chkDialogueQuotations
			// 
			this.m_chkDialogueQuotations.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_chkDialogueQuotations, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkDialogueQuotations, "QuotationMarksDialog.QuotationMarksDialog.m_chkDialogueQuotations");
			this.m_chkDialogueQuotations.Location = new System.Drawing.Point(3, 10);
			this.m_chkDialogueQuotations.Name = "m_chkDialogueQuotations";
			this.m_chkDialogueQuotations.Size = new System.Drawing.Size(264, 17);
			this.m_chkDialogueQuotations.TabIndex = 9;
			this.m_chkDialogueQuotations.Text = "This project marks dialogue with quotation dashes.";
			this.m_chkDialogueQuotations.UseVisualStyleBackColor = true;
			this.m_chkDialogueQuotations.CheckedChanged += new System.EventHandler(this.m_chkDialogueQuotations_CheckedChanged);
			// 
			// m_cboQuotationDash
			// 
			this.m_cboQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboQuotationDash.Enabled = false;
			this.m_cboQuotationDash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cboQuotationDash.FormattingEnabled = true;
			this.m_cboQuotationDash.Items.AddRange(new object[] {
            "Quotation dash (U+2015)",
            "Em-dash (U+2014)"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_cboQuotationDash, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboQuotationDash, "QuotationMarksDialog.QuotationMarksDialog.m_cboQuotationDash");
			this.m_cboQuotationDash.Location = new System.Drawing.Point(271, 8);
			this.m_cboQuotationDash.Margin = new System.Windows.Forms.Padding(0);
			this.m_cboQuotationDash.Name = "m_cboQuotationDash";
			this.m_cboQuotationDash.Size = new System.Drawing.Size(190, 21);
			this.m_cboQuotationDash.TabIndex = 10;
			this.m_cboQuotationDash.TextChanged += new System.EventHandler(this.m_cboQuotationDash_TextChanged);
			// 
			// m_cboEndQuotationDash
			// 
			this.m_cboEndQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboEndQuotationDash.Enabled = false;
			this.m_cboEndQuotationDash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cboEndQuotationDash.FormattingEnabled = true;
			this.m_cboEndQuotationDash.Items.AddRange(new object[] {
            "End of paragraph (only)",
            "#dash#",
            "Any punctuation mark"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboEndQuotationDash, "QuotationMarksDialog.QuotationMarksDialog.m_cboEndQuotationDash");
			this.m_cboEndQuotationDash.Location = new System.Drawing.Point(271, 32);
			this.m_cboEndQuotationDash.Margin = new System.Windows.Forms.Padding(0);
			this.m_cboEndQuotationDash.Name = "m_cboEndQuotationDash";
			this.m_cboEndQuotationDash.Size = new System.Drawing.Size(190, 21);
			this.m_cboEndQuotationDash.TabIndex = 11;
			// 
			// m_lblEndDialogueQuote
			// 
			this.m_lblEndDialogueQuote.AutoSize = true;
			this.m_lblEndDialogueQuote.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblEndDialogueQuote, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblEndDialogueQuote, "QuotationMarksDialog.QuotationMarksDialog.m_lblEndDialogueQuote");
			this.m_lblEndDialogueQuote.Location = new System.Drawing.Point(129, 35);
			this.m_lblEndDialogueQuote.Name = "m_lblEndDialogueQuote";
			this.m_lblEndDialogueQuote.Size = new System.Drawing.Size(134, 13);
			this.m_lblEndDialogueQuote.TabIndex = 12;
			this.m_lblEndDialogueQuote.Text = "Dialogue quotation ending:";
			// 
			// m_chkAlternateSpeakersInFirstLevelQuotes
			// 
			this.m_chkAlternateSpeakersInFirstLevelQuotes.AutoSize = true;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_chkAlternateSpeakersInFirstLevelQuotes, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkAlternateSpeakersInFirstLevelQuotes, "QuotationMarksDialog.QuotationMarksDialog.m_chkAlternateSpeakersInFirstLevelQuote" +
        "s");
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Location = new System.Drawing.Point(0, 96);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Name = "m_chkAlternateSpeakersInFirstLevelQuotes";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Size = new System.Drawing.Size(418, 17);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.TabIndex = 13;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Tag = "\"Making this invisible until we can find a project that actually uses this. Not y" +
    "et implemented in QuoteParser.\"";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Text = "Quotation dashes also indicate change of speaker within first-level quotation mar" +
    "ks.";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.UseVisualStyleBackColor = true;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Visible = false;
			// 
			// m_pnlDialogeQuotes
			// 
			this.m_pnlDialogeQuotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkAlternateSpeakersInFirstLevelQuotes);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkDialogueQuotations);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboEndQuotationDash);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_lblEndDialogueQuote);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboQuotationDash);
			this.m_pnlDialogeQuotes.Location = new System.Drawing.Point(20, 80);
			this.m_pnlDialogeQuotes.Name = "m_pnlDialogeQuotes";
			this.m_pnlDialogeQuotes.Size = new System.Drawing.Size(461, 116);
			this.m_pnlDialogeQuotes.TabIndex = 13;
			// 
			// QuotationMarksDialog
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(499, 237);
			this.Controls.Add(this.m_pnlDialogeQuotes);
			this.Controls.Add(this.m_lblHorizontalSeparator);
			this.Controls.Add(this.m_lblQuotationMarks);
			this.Controls.Add(this.m_comboQuoteMarks);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "ProjectSettingsDialog.ProjectSettings");
			this.MinimumSize = new System.Drawing.Size(515, 275);
			this.Name = "QuotationMarksDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Quotation Marks";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_pnlDialogeQuotes.ResumeLayout(false);
			this.m_pnlDialogeQuotes.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.ComboBox m_comboQuoteMarks;
		private System.Windows.Forms.Label m_lblQuotationMarks;
		private System.Windows.Forms.Label m_lblHorizontalSeparator;
		private System.Windows.Forms.CheckBox m_chkDialogueQuotations;
		private System.Windows.Forms.ComboBox m_cboQuotationDash;
		private System.Windows.Forms.ComboBox m_cboEndQuotationDash;
		private System.Windows.Forms.Label m_lblEndDialogueQuote;
		private System.Windows.Forms.Panel m_pnlDialogeQuotes;
		private System.Windows.Forms.CheckBox m_chkAlternateSpeakersInFirstLevelQuotes;
	}
}