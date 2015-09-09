namespace Glyssen.Dialogs
{
	partial class TooManyUnexpectedQuotesFoundDlg
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
			this.m_lblPossibleProblemsWithFirstLevelQuotes = new System.Windows.Forms.Label();
			this.m_rdoUseSettings = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblOnlyNPercentOfExpectedQuotesFound = new System.Windows.Forms.Label();
			this.m_rdoReview = new System.Windows.Forms.RadioButton();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblPossibleProblemsWithFirstLevelQuotes
			// 
			this.m_lblPossibleProblemsWithFirstLevelQuotes.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblPossibleProblemsWithFirstLevelQuotes, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblPossibleProblemsWithFirstLevelQuotes, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_lblPossibleProblemsWithFirstLevelQuotes, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblPossibleProblemsWithFirstLevelQuotes, "DialogBoxes.QuotationMarksDlg.TooManyUnexpectedQuotesFoundDlg.PossibleProblemsWithFirstLevelQuotes");
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Location = new System.Drawing.Point(3, 17);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Name = "m_lblPossibleProblemsWithFirstLevelQuotes";
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Size = new System.Drawing.Size(413, 30);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.TabIndex = 1;
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Text = "Since more than {0:F0}% of the quotes found were unexpected, it probably means th" +
    "e quotation mark settings are incorrect.";
			// 
			// m_rdoUseSettings
			// 
			this.m_rdoUseSettings.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_rdoUseSettings, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_rdoUseSettings, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_rdoUseSettings, "DialogBoxes.QuotationMarksDlg.TooManyUnexpectedQuotesFoundDlg.UseTheseSettings");
			this.m_rdoUseSettings.Location = new System.Drawing.Point(3, 83);
			this.m_rdoUseSettings.Name = "m_rdoUseSettings";
			this.m_rdoUseSettings.Size = new System.Drawing.Size(188, 17);
			this.m_rdoUseSettings.TabIndex = 3;
			this.m_rdoUseSettings.TabStop = true;
			this.m_rdoUseSettings.Text = "Use these quotation mark settings.";
			this.m_rdoUseSettings.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblOnlyNPercentOfExpectedQuotesFound, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoUseSettings, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoReview, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblPossibleProblemsWithFirstLevelQuotes, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_btnOk, 0, 4);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 13);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(440, 162);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// m_lblOnlyNPercentOfExpectedQuotesFound
			// 
			this.m_lblOnlyNPercentOfExpectedQuotesFound.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblOnlyNPercentOfExpectedQuotesFound, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblOnlyNPercentOfExpectedQuotesFound, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_lblOnlyNPercentOfExpectedQuotesFound, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblOnlyNPercentOfExpectedQuotesFound, "DialogBoxes.QuotationMarksDlg.TooManyUnexpectedQuotesFoundDlg.OnlyNPercentOfExpectedQuotesFound");
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Location = new System.Drawing.Point(3, 0);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Name = "m_lblOnlyNPercentOfExpectedQuotesFound";
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Size = new System.Drawing.Size(209, 17);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.TabIndex = 0;
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Text = "{0:F1}% of quotes found were unexpected.";
			// 
			// m_rdoReview
			// 
			this.m_rdoReview.AutoSize = true;
			this.m_rdoReview.Checked = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_rdoReview, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_rdoReview, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_rdoReview, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_rdoReview, "DialogBoxes.QuotationMarksDlg.TooManyUnexpectedQuotesFoundDlg.LetMeReview");
			this.m_rdoReview.Location = new System.Drawing.Point(3, 50);
			this.m_rdoReview.Name = "m_rdoReview";
			this.m_rdoReview.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.m_rdoReview.Size = new System.Drawing.Size(224, 27);
			this.m_rdoReview.TabIndex = 2;
			this.m_rdoReview.TabStop = true;
			this.m_rdoReview.Text = "Let me review the quotes that were found.";
			this.m_rdoReview.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnOk.AutoSize = true;
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(182, 136);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 5;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.QuotationMarksDlg";
			// 
			// TooManyUnexpectedQuotesFoundDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 187);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this, "TooManyUnexpectedQuotesFoundDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TooManyUnexpectedQuotesFoundDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Too Many Unexpected Quotes Found";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label m_lblPossibleProblemsWithFirstLevelQuotes;
		private System.Windows.Forms.RadioButton m_rdoUseSettings;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.Label m_lblOnlyNPercentOfExpectedQuotesFound;
		private System.Windows.Forms.RadioButton m_rdoReview;
	}
}