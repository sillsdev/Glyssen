using GlyssenApp.Utilities;

namespace GlyssenApp.UI.Dialogs
{
	partial class PercentageOfExpectedQuotesFoundTooLowDlg
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
			this.m_lblOnlyNPercentOfExpectedQuotesFound = new System.Windows.Forms.Label();
			this.m_lblPossibleProblemsWithFirstLevelQuotes = new System.Windows.Forms.Label();
			this.m_rdoUseSettings = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblDirectSpeechNotMarked = new System.Windows.Forms.Label();
			this.m_rdoReview = new System.Windows.Forms.RadioButton();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash = new System.Windows.Forms.Label();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.glyssenColorPalette1 = new GlyssenColorPalette();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblOnlyNPercentOfExpectedQuotesFound
			// 
			this.m_lblOnlyNPercentOfExpectedQuotesFound.AutoSize = true;
			this.m_lblOnlyNPercentOfExpectedQuotesFound.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_lblOnlyNPercentOfExpectedQuotesFound, GlyssenColors.BackColor);
			this.glyssenColorPalette1.SetForeColor(this.m_lblOnlyNPercentOfExpectedQuotesFound, GlyssenColors.ForeColor);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblOnlyNPercentOfExpectedQuotesFound, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblOnlyNPercentOfExpectedQuotesFound, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblOnlyNPercentOfExpectedQuotesFound, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.OnlyNPerce" +
        "ntFound");
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Location = new System.Drawing.Point(3, 0);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Name = "m_lblOnlyNPercentOfExpectedQuotesFound";
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Size = new System.Drawing.Size(351, 17);
			this.m_lblOnlyNPercentOfExpectedQuotesFound.TabIndex = 0;
			this.m_lblOnlyNPercentOfExpectedQuotesFound.Text = "Only {0:F1}% of verses with expected quotes were found to have quotes.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_lblOnlyNPercentOfExpectedQuotesFound, true);
			// 
			// m_lblPossibleProblemsWithFirstLevelQuotes
			// 
			this.m_lblPossibleProblemsWithFirstLevelQuotes.AutoSize = true;
			this.m_lblPossibleProblemsWithFirstLevelQuotes.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_lblPossibleProblemsWithFirstLevelQuotes, GlyssenColors.BackColor);
			this.glyssenColorPalette1.SetForeColor(this.m_lblPossibleProblemsWithFirstLevelQuotes, GlyssenColors.ForeColor);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblPossibleProblemsWithFirstLevelQuotes, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblPossibleProblemsWithFirstLevelQuotes, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblPossibleProblemsWithFirstLevelQuotes, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.Level1Inco" +
        "rrect");
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Location = new System.Drawing.Point(3, 17);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Name = "m_lblPossibleProblemsWithFirstLevelQuotes";
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Size = new System.Drawing.Size(396, 30);
			this.m_lblPossibleProblemsWithFirstLevelQuotes.TabIndex = 1;
			this.m_lblPossibleProblemsWithFirstLevelQuotes.Text = "Since less than {0:F0}% of the expected quotes were found, it probably means the " +
    "quotation mark settings are incorrect.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_lblPossibleProblemsWithFirstLevelQuotes, true);
			// 
			// m_rdoUseSettings
			// 
			this.m_rdoUseSettings.AutoSize = true;
			this.m_rdoUseSettings.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_rdoUseSettings, GlyssenColors.BackColor);
			this.m_rdoUseSettings.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette1.SetFlatAppearanceBorderColor(this.m_rdoUseSettings, GlyssenColors.ForeColor);
			this.m_rdoUseSettings.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette1.SetForeColor(this.m_rdoUseSettings, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_rdoUseSettings, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_rdoUseSettings, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_rdoUseSettings, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.UseTheseSe" +
        "ttings");
			this.m_rdoUseSettings.Location = new System.Drawing.Point(3, 156);
			this.m_rdoUseSettings.Name = "m_rdoUseSettings";
			this.m_rdoUseSettings.Size = new System.Drawing.Size(188, 17);
			this.m_rdoUseSettings.TabIndex = 3;
			this.m_rdoUseSettings.TabStop = true;
			this.m_rdoUseSettings.Text = "Use these quotation mark settings.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_rdoUseSettings, true);
			this.m_rdoUseSettings.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette1.SetBackColor(this.tableLayoutPanel1, GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblOnlyNPercentOfExpectedQuotesFound, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoUseSettings, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_lblDirectSpeechNotMarked, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_rdoReview, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_lblPossibleProblemsWithFirstLevelQuotes, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_btnOk, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette1.SetForeColor(this.tableLayoutPanel1, GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(444, 215);
			this.tableLayoutPanel1.TabIndex = 5;
			this.glyssenColorPalette1.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// m_lblDirectSpeechNotMarked
			// 
			this.m_lblDirectSpeechNotMarked.AutoSize = true;
			this.m_lblDirectSpeechNotMarked.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_lblDirectSpeechNotMarked, GlyssenColors.BackColor);
			this.glyssenColorPalette1.SetForeColor(this.m_lblDirectSpeechNotMarked, GlyssenColors.ForeColor);
			this.m_lblDirectSpeechNotMarked.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblDirectSpeechNotMarked, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblDirectSpeechNotMarked, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblDirectSpeechNotMarked, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.DirectSpee" +
        "chNotMarked");
			this.m_lblDirectSpeechNotMarked.Location = new System.Drawing.Point(3, 90);
			this.m_lblDirectSpeechNotMarked.Name = "m_lblDirectSpeechNotMarked";
			this.m_lblDirectSpeechNotMarked.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblDirectSpeechNotMarked.Size = new System.Drawing.Size(434, 30);
			this.m_lblDirectSpeechNotMarked.TabIndex = 4;
			this.m_lblDirectSpeechNotMarked.Text = "This can also happen if many instances of direct speech in the text are not indic" +
    "ated using quotation marks.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_lblDirectSpeechNotMarked, true);
			// 
			// m_rdoReview
			// 
			this.m_rdoReview.AutoSize = true;
			this.m_rdoReview.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_rdoReview, GlyssenColors.BackColor);
			this.m_rdoReview.Checked = true;
			this.m_rdoReview.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette1.SetFlatAppearanceBorderColor(this.m_rdoReview, GlyssenColors.ForeColor);
			this.m_rdoReview.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette1.SetForeColor(this.m_rdoReview, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_rdoReview, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_rdoReview, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_rdoReview, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.LetMeRevie" +
        "w");
			this.m_rdoReview.Location = new System.Drawing.Point(3, 123);
			this.m_rdoReview.Name = "m_rdoReview";
			this.m_rdoReview.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.m_rdoReview.Size = new System.Drawing.Size(352, 27);
			this.m_rdoReview.TabIndex = 2;
			this.m_rdoReview.TabStop = true;
			this.m_rdoReview.Text = "Let me review the verses where quotes were expected but not found.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_rdoReview, true);
			this.m_rdoReview.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnOk.AutoSize = true;
			this.glyssenColorPalette1.SetBackColor(this.m_btnOk, GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.glyssenColorPalette1.SetFlatAppearanceBorderColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.glyssenColorPalette1.SetForeColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(184, 189);
			this.m_btnOk.Margin = new System.Windows.Forms.Padding(3, 11, 3, 3);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 5;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash
			// 
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.AutoSize = true;
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette1.SetBackColor(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, GlyssenColors.BackColor);
			this.glyssenColorPalette1.SetForeColor(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, GlyssenColors.ForeColor);
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, "DialogBoxes.QuotationMarksDlg.PercentageOfExpectedQuotesFoundTooLowDlg.Level1Prob" +
        "ablyCorrect");
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Location = new System.Drawing.Point(3, 47);
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Name = "m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash";
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Size = new System.Drawing.Size(416, 43);
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.TabIndex = 6;
			this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash.Text = "The quotation mark settings for Level 1 are probably correct. However, since less" +
    " than {0:F0}% of the expected quotes were found, some of the other quotation mar" +
    "k settings might be incorrect.";
			this.glyssenColorPalette1.SetUsePaletteColors(this.m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash, true);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.QuotationMarksDlg";
			// 
			// PercentageOfExpectedQuotesFoundTooLowDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette1.SetBackColor(this, GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(464, 239);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.glyssenColorPalette1.SetForeColor(this, GlyssenColors.Default);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this, "PercentageOfExpectedQuotesFoundTooLowDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PercentageOfExpectedQuotesFoundTooLowDlg";
			this.Padding = new System.Windows.Forms.Padding(10, 12, 10, 12);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Percentage of Expected Quotes Found Too Low";
			this.glyssenColorPalette1.SetUsePaletteColors(this, true);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblOnlyNPercentOfExpectedQuotesFound;
		private System.Windows.Forms.Label m_lblPossibleProblemsWithFirstLevelQuotes;
		private System.Windows.Forms.RadioButton m_rdoUseSettings;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.RadioButton m_rdoReview;
		private System.Windows.Forms.Label m_lblDirectSpeechNotMarked;
		private System.Windows.Forms.Label m_lblPossibleProblemsWithLowerLevelQuotesOrDialogueDash;
		private GlyssenColorPalette glyssenColorPalette1;
	}
}