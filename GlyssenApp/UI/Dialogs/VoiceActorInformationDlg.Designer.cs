using GlyssenApp.UI.Controls;
using GlyssenApp.Utilities;
using L10NSharp.UI;

namespace GlyssenApp.UI.Dialogs
{
	partial class VoiceActorInformationDlg
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
				LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
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
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_lblActorsEnteredSoFar = new System.Windows.Forms.Label();
			this.m_lblTally = new System.Windows.Forms.Label();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.m_toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.m_dataGrid = new VoiceActorInformationGrid();
			this.m_saveStatus = new SaveStatus();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_btnCancelClose = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new GlyssenColorPalette();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.toolStrip1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorInformation";
			// 
			// m_lblActorsEnteredSoFar
			// 
			this.m_lblActorsEnteredSoFar.AutoSize = true;
			this.m_lblActorsEnteredSoFar.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblActorsEnteredSoFar, GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblActorsEnteredSoFar, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblActorsEnteredSoFar, GlyssenColors.ForeColor);
			this.m_lblActorsEnteredSoFar.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblActorsEnteredSoFar, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblActorsEnteredSoFar, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblActorsEnteredSoFar, "DialogBoxes.VoiceActorInformation.ActorsEnteredSoFar");
			this.m_lblActorsEnteredSoFar.Location = new System.Drawing.Point(3, 0);
			this.m_lblActorsEnteredSoFar.Name = "m_lblActorsEnteredSoFar";
			this.m_lblActorsEnteredSoFar.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
			this.m_lblActorsEnteredSoFar.Size = new System.Drawing.Size(264, 21);
			this.m_lblActorsEnteredSoFar.TabIndex = 7;
			this.m_lblActorsEnteredSoFar.Text = "It looks like you have already entered {0} voice actors.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblActorsEnteredSoFar, true);
			// 
			// m_lblTally
			// 
			this.m_lblTally.AutoSize = true;
			this.m_lblTally.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblTally, GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblTally, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblTally, GlyssenColors.ForeColor);
			this.m_lblTally.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblTally, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblTally, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblTally, "DialogBoxes.VoiceActorInformation.Tally");
			this.m_lblTally.Location = new System.Drawing.Point(3, 319);
			this.m_lblTally.Name = "m_lblTally";
			this.m_lblTally.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.m_lblTally.Size = new System.Drawing.Size(178, 16);
			this.m_lblTally.TabIndex = 12;
			this.m_lblTally.Text = "Tally: {0} Male, {1} Female, {2} Child";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblTally, true);
			// 
			// toolStrip1
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStrip1, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.toolStrip1, GlyssenColors.Default);
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_toolStripButtonHelp});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.toolStrip1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.toolStrip1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.toolStrip1, "DialogBoxes.VoiceActorInformation.toolStrip1");
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(609, 25);
			this.toolStrip1.TabIndex = 7;
			this.toolStrip1.Text = "toolStrip1";
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStrip1, false);
			// 
			// m_toolStripButtonHelp
			// 
			this.m_toolStripButtonHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonHelp, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonHelp, GlyssenColors.ForeColor);
			this.m_toolStripButtonHelp.Image = global::GlyssenApp.Properties.Resources.helpSmall;
			this.m_toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_toolStripButtonHelp, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_toolStripButtonHelp, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_toolStripButtonHelp, "Common.Help");
			this.m_toolStripButtonHelp.Name = "m_toolStripButtonHelp";
			this.m_toolStripButtonHelp.Size = new System.Drawing.Size(52, 22);
			this.m_toolStripButtonHelp.Text = "Help";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonHelp, false);
			this.m_toolStripButtonHelp.Visible = false;
			// 
			// m_dataGrid
			// 
			this.m_dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGrid.AutoScroll = true;
			this.m_dataGrid.AutoSize = true;
			this.m_dataGrid.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_dataGrid, GlyssenColors.BackColor);
			this.m_dataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.SetColumnSpan(this.m_dataGrid, 3);
			this.m_dataGrid.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_dataGrid, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_dataGrid, "DialogBoxes.VoiceActorInformation.voiceActorInformationGrid21");
			this.m_dataGrid.Location = new System.Drawing.Point(0, 55);
			this.m_dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.ReadOnly = false;
			this.m_dataGrid.Size = new System.Drawing.Size(582, 264);
			this.m_dataGrid.TabIndex = 3;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_dataGrid, true);
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_saveStatus, GlyssenColors.BackColor);
			this.m_saveStatus.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_saveStatus, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorInformation.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(465, 36);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
			this.m_saveStatus.Size = new System.Drawing.Size(117, 13);
			this.m_saveStatus.TabIndex = 4;
			this.m_saveStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_saveStatus, true);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(3, 6);
			this.m_btnOk.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 2;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.m_lblInstructions.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblInstructions, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblInstructions, GlyssenColors.ForeColor);
			this.m_lblInstructions.ForeColor = System.Drawing.SystemColors.WindowText;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.VoiceActorInformation.InstructionsForEnteringActorInformation");
			this.m_lblInstructions.Location = new System.Drawing.Point(3, 21);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
			this.m_lblInstructions.Size = new System.Drawing.Size(448, 34);
			this.m_lblInstructions.TabIndex = 8;
			this.m_lblInstructions.Text = "Please bring the list up to date. Add any additional actors. Mark as \"Inactive\" a" +
    "ny who are no longer available. Ensure attributes are correct, especially Gender" +
    ".";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblInstructions, true);
			// 
			// m_btnCancelClose
			// 
			this.m_btnCancelClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancelClose, GlyssenColors.BackColor);
			this.m_btnCancelClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancelClose, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancelClose, GlyssenColors.ForeColor);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnCancelClose, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnCancelClose, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnCancelClose, "Common.Cancel");
			this.m_btnCancelClose.Location = new System.Drawing.Point(84, 6);
			this.m_btnCancelClose.Name = "m_btnCancelClose";
			this.m_btnCancelClose.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancelClose.TabIndex = 13;
			this.m_btnCancelClose.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancelClose, false);
			this.m_btnCancelClose.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.m_lblInstructions, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_dataGrid, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblActorsEnteredSoFar, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblTally, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_saveStatus, 2, 1);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 31);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(582, 335);
			this.tableLayoutPanel1.TabIndex = 6;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel2, GlyssenColors.BackColor);
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.m_btnCancelClose, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_btnOk, 0, 0);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel2, GlyssenColors.Default);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(435, 372);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(162, 32);
			this.tableLayoutPanel2.TabIndex = 8;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel2, false);
			// 
			// VoiceActorInformationDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancelClose;
			this.ClientSize = new System.Drawing.Size(609, 415);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.tableLayoutPanel1);
			this.glyssenColorPalette.SetForeColor(this, GlyssenColors.Default);
			this.Icon = global::GlyssenApp.Properties.Resources.glyssenIcon;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, "{0} is the project name");
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorInformation.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 425);
			this.Name = "VoiceActorInformationDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor List - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.VoiceActorInformationDlg_Load);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private VoiceActorInformationGrid m_dataGrid;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblActorsEnteredSoFar;
		private System.Windows.Forms.Label m_lblTally;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonHelp;
		private GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.Button m_btnOk;
		private SaveStatus m_saveStatus;
		private System.Windows.Forms.Label m_lblInstructions;
		private System.Windows.Forms.Button m_btnCancelClose;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	}
}