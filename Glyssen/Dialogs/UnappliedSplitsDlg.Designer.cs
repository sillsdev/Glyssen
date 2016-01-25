namespace Glyssen.Dialogs
{
	partial class UnappliedSplitsDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnappliedSplitsDlg));
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_btnCopyToClipboard = new System.Windows.Forms.Button();
			this.m_btnClose = new System.Windows.Forms.Button();
			this.m_browser = new Glyssen.Controls.Browser();
			this.m_checkFinished = new System.Windows.Forms.CheckBox();
			this.m_checkDeleteData = new System.Windows.Forms.CheckBox();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.UnappliedSplitsDlg";
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lblInstructions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblInstructions, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.UnappliedSplitsDlg.Instructions");
			this.m_lblInstructions.Location = new System.Drawing.Point(12, 13);
			this.m_lblInstructions.MaximumSize = new System.Drawing.Size(643, 0);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(643, 39);
			this.m_lblInstructions.TabIndex = 2;
			this.m_lblInstructions.Text = resources.GetString("m_lblInstructions.Text");
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblInstructions, true);
			// 
			// m_btnCopyToClipboard
			// 
			this.m_btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCopyToClipboard, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCopyToClipboard, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCopyToClipboard, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCopyToClipboard, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCopyToClipboard, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCopyToClipboard, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCopyToClipboard, "DialogBoxes.UnappliedSplitsDlg.CopyToClipboard");
			this.m_btnCopyToClipboard.Location = new System.Drawing.Point(461, 417);
			this.m_btnCopyToClipboard.Name = "m_btnCopyToClipboard";
			this.m_btnCopyToClipboard.Size = new System.Drawing.Size(113, 23);
			this.m_btnCopyToClipboard.TabIndex = 3;
			this.m_btnCopyToClipboard.Text = "Copy To Clipboard";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCopyToClipboard, false);
			this.m_btnCopyToClipboard.UseVisualStyleBackColor = true;
			this.m_btnCopyToClipboard.Click += new System.EventHandler(this.BtnCopyToClipboard_Click);
			// 
			// m_btnClose
			// 
			this.m_btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnClose.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnClose, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnClose, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnClose, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnClose, "DialogBoxes.UnappliedSplitsDlg.UnappliedSplitsDlg.Close");
			this.m_btnClose.Location = new System.Drawing.Point(580, 417);
			this.m_btnClose.Name = "m_btnClose";
			this.m_btnClose.Size = new System.Drawing.Size(75, 23);
			this.m_btnClose.TabIndex = 4;
			this.m_btnClose.Text = "Close";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnClose, false);
			this.m_btnClose.UseVisualStyleBackColor = true;
			this.m_btnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// m_browser
			// 
			this.m_browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_browser.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_browser, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_browser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.glyssenColorPalette.SetForeColor(this.m_browser, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_browser, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_browser, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_browser, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_browser, "DialogBoxes.UnappliedSplitsDlg.Browser");
			this.m_browser.Location = new System.Drawing.Point(12, 61);
			this.m_browser.Name = "m_browser";
			this.m_browser.Size = new System.Drawing.Size(643, 298);
			this.m_browser.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_browser, true);
			// 
			// m_checkFinished
			// 
			this.m_checkFinished.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_checkFinished.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_checkFinished, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkFinished, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_checkFinished, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkFinished, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkFinished, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkFinished, "DialogBoxes.UnappliedSplitsDlg.SavedAcknowlegement");
			this.m_checkFinished.Location = new System.Drawing.Point(15, 365);
			this.m_checkFinished.Name = "m_checkFinished";
			this.m_checkFinished.Size = new System.Drawing.Size(321, 17);
			this.m_checkFinished.TabIndex = 5;
			this.m_checkFinished.Text = "I have saved this information and am ready to close this dialog.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkFinished, true);
			this.m_checkFinished.UseVisualStyleBackColor = true;
			this.m_checkFinished.CheckedChanged += new System.EventHandler(this.CheckFinished_CheckedChanged);
			// 
			// m_checkDeleteData
			// 
			this.m_checkDeleteData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_checkDeleteData.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_checkDeleteData, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkDeleteData, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_checkDeleteData, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkDeleteData, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkDeleteData, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkDeleteData, "DialogBoxes.UnappliedSplitsDlg.SavedAcknowlegement");
			this.m_checkDeleteData.Location = new System.Drawing.Point(15, 388);
			this.m_checkDeleteData.Name = "m_checkDeleteData";
			this.m_checkDeleteData.Size = new System.Drawing.Size(482, 17);
			this.m_checkDeleteData.TabIndex = 6;
			this.m_checkDeleteData.Text = "Do not show this again for these block splits.  (You will not be able to view thi" +
    "s information again.)";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkDeleteData, true);
			this.m_checkDeleteData.UseVisualStyleBackColor = true;
			// 
			// UnappliedSplitsDlg
			// 
			this.AcceptButton = this.m_btnClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(667, 452);
			this.ControlBox = false;
			this.Controls.Add(this.m_checkDeleteData);
			this.Controls.Add(this.m_checkFinished);
			this.Controls.Add(this.m_btnClose);
			this.Controls.Add(this.m_btnCopyToClipboard);
			this.Controls.Add(this.m_lblInstructions);
			this.Controls.Add(this.m_browser);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.UnappliedSplitsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(683, 458);
			this.Name = "UnappliedSplitsDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Unapplied Block Splits";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private Controls.Browser m_browser;
		private System.Windows.Forms.Label m_lblInstructions;
		private System.Windows.Forms.Button m_btnCopyToClipboard;
		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.CheckBox m_checkFinished;
		private System.Windows.Forms.CheckBox m_checkDeleteData;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}