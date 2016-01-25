namespace Glyssen.Dialogs
{
	partial class NarrationOptionsDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NarrationOptionsDlg));
			this.m_lblExplanatory = new System.Windows.Forms.Label();
			this.m_lblDesiredMaleNarratorNum = new System.Windows.Forms.Label();
			this.m_selectionsTableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.m_numMaleNarrator = new System.Windows.Forms.NumericUpDown();
			this.m_lblDesiredFemaleNarratorNum = new System.Windows.Forms.Label();
			this.m_numFemaleNarrator = new System.Windows.Forms.NumericUpDown();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_selectionsTableLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaleNarrator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFemaleNarrator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblExplanatory
			// 
			this.m_lblExplanatory.AutoSize = true;
			this.m_lblExplanatory.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblExplanatory, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_selectionsTableLayout.SetColumnSpan(this.m_lblExplanatory, 2);
			this.m_lblExplanatory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblExplanatory, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblExplanatory.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExplanatory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExplanatory, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExplanatory, "DialogBoxes.NarrationOptionsDlg.Explanatory");
			this.m_lblExplanatory.Location = new System.Drawing.Point(1, 0);
			this.m_lblExplanatory.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.m_lblExplanatory.MaximumSize = new System.Drawing.Size(394, 0);
			this.m_lblExplanatory.Name = "m_lblExplanatory";
			this.m_lblExplanatory.Padding = new System.Windows.Forms.Padding(0, 8, 8, 17);
			this.m_lblExplanatory.Size = new System.Drawing.Size(394, 142);
			this.m_lblExplanatory.TabIndex = 0;
			this.m_lblExplanatory.Text = resources.GetString("m_lblExplanatory.Text");
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExplanatory, true);
			// 
			// m_lblDesiredMaleNarratorNum
			// 
			this.m_lblDesiredMaleNarratorNum.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDesiredMaleNarratorNum.AutoSize = true;
			this.m_lblDesiredMaleNarratorNum.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDesiredMaleNarratorNum, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblDesiredMaleNarratorNum, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDesiredMaleNarratorNum.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDesiredMaleNarratorNum, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDesiredMaleNarratorNum, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDesiredMaleNarratorNum, "DialogBoxes.NarrationOptionsDlg.DesiredNarratorNum");
			this.m_lblDesiredMaleNarratorNum.Location = new System.Drawing.Point(0, 148);
			this.m_lblDesiredMaleNarratorNum.Margin = new System.Windows.Forms.Padding(0);
			this.m_lblDesiredMaleNarratorNum.Name = "m_lblDesiredMaleNarratorNum";
			this.m_lblDesiredMaleNarratorNum.Size = new System.Drawing.Size(165, 13);
			this.m_lblDesiredMaleNarratorNum.TabIndex = 2;
			this.m_lblDesiredMaleNarratorNum.Text = "Desired number of male narrators:";
			this.m_lblDesiredMaleNarratorNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDesiredMaleNarratorNum, true);
			// 
			// m_selectionsTableLayout
			// 
			this.glyssenColorPalette.SetBackColor(this.m_selectionsTableLayout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_selectionsTableLayout.ColumnCount = 2;
			this.m_selectionsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_selectionsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_selectionsTableLayout.Controls.Add(this.m_lblDesiredMaleNarratorNum, 0, 1);
			this.m_selectionsTableLayout.Controls.Add(this.m_lblExplanatory, 0, 0);
			this.m_selectionsTableLayout.Controls.Add(this.m_numMaleNarrator, 1, 1);
			this.m_selectionsTableLayout.Controls.Add(this.m_lblDesiredFemaleNarratorNum, 0, 2);
			this.m_selectionsTableLayout.Controls.Add(this.m_numFemaleNarrator, 1, 2);
			this.glyssenColorPalette.SetForeColor(this.m_selectionsTableLayout, Glyssen.Utilities.GlyssenColors.Default);
			this.m_selectionsTableLayout.Location = new System.Drawing.Point(8, 10);
			this.m_selectionsTableLayout.Margin = new System.Windows.Forms.Padding(1);
			this.m_selectionsTableLayout.Name = "m_selectionsTableLayout";
			this.m_selectionsTableLayout.RowCount = 4;
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_selectionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_selectionsTableLayout.Size = new System.Drawing.Size(400, 270);
			this.m_selectionsTableLayout.TabIndex = 7;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_selectionsTableLayout, false);
			// 
			// m_numMaleNarrator
			// 
			this.m_numMaleNarrator.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_numMaleNarrator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_numMaleNarrator, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_numMaleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_numMaleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_numMaleNarrator, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_numMaleNarrator, "NarrationOptionsDlg.m_numNarratorNum");
			this.m_numMaleNarrator.Location = new System.Drawing.Point(177, 145);
			this.m_numMaleNarrator.Maximum = new decimal(new int[] {
            66,
            0,
            0,
            0});
			this.m_numMaleNarrator.Name = "m_numMaleNarrator";
			this.m_numMaleNarrator.Size = new System.Drawing.Size(50, 20);
			this.m_numMaleNarrator.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_numMaleNarrator, false);
			this.m_numMaleNarrator.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// m_lblDesiredFemaleNarratorNum
			// 
			this.m_lblDesiredFemaleNarratorNum.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblDesiredFemaleNarratorNum.AutoSize = true;
			this.m_lblDesiredFemaleNarratorNum.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDesiredFemaleNarratorNum, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblDesiredFemaleNarratorNum, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDesiredFemaleNarratorNum.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDesiredFemaleNarratorNum, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDesiredFemaleNarratorNum, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDesiredFemaleNarratorNum, "DialogBoxes.NarrationOptionsDlg.DesiredFemaleNarratorNum");
			this.m_lblDesiredFemaleNarratorNum.Location = new System.Drawing.Point(0, 174);
			this.m_lblDesiredFemaleNarratorNum.Margin = new System.Windows.Forms.Padding(0);
			this.m_lblDesiredFemaleNarratorNum.Name = "m_lblDesiredFemaleNarratorNum";
			this.m_lblDesiredFemaleNarratorNum.Size = new System.Drawing.Size(174, 13);
			this.m_lblDesiredFemaleNarratorNum.TabIndex = 5;
			this.m_lblDesiredFemaleNarratorNum.Text = "Desired number of female narrators:";
			this.m_lblDesiredFemaleNarratorNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDesiredFemaleNarratorNum, true);
			// 
			// m_numFemaleNarrator
			// 
			this.m_numFemaleNarrator.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_numFemaleNarrator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_numFemaleNarrator, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_numFemaleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_numFemaleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_numFemaleNarrator, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_numFemaleNarrator, "NarrationOptionsDlg.m_numNarratorNum");
			this.m_numFemaleNarrator.Location = new System.Drawing.Point(177, 171);
			this.m_numFemaleNarrator.Maximum = new decimal(new int[] {
            66,
            0,
            0,
            0});
			this.m_numFemaleNarrator.Name = "m_numFemaleNarrator";
			this.m_numFemaleNarrator.Size = new System.Drawing.Size(50, 20);
			this.m_numFemaleNarrator.TabIndex = 6;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_numFemaleNarrator, false);
			this.m_numFemaleNarrator.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
			this.m_btnCancel.Location = new System.Drawing.Point(327, 346);
			this.m_btnCancel.Margin = new System.Windows.Forms.Padding(1);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
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
			this.m_btnOk.Location = new System.Drawing.Point(245, 346);
			this.m_btnOk.Margin = new System.Windows.Forms.Padding(1);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 4;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Clicked);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// NarrationOptionsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(412, 379);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_selectionsTableLayout);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.NarrationOptionsDlg.WindowTitle");
			this.Margin = new System.Windows.Forms.Padding(1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(428, 417);
			this.Name = "NarrationOptionsDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Narration Preferences";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.m_selectionsTableLayout.ResumeLayout(false);
			this.m_selectionsTableLayout.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numMaleNarrator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numFemaleNarrator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label m_lblExplanatory;
		private System.Windows.Forms.TableLayoutPanel m_selectionsTableLayout;
		private System.Windows.Forms.Label m_lblDesiredMaleNarratorNum;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.NumericUpDown m_numMaleNarrator;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Label m_lblDesiredFemaleNarratorNum;
		private System.Windows.Forms.NumericUpDown m_numFemaleNarrator;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}