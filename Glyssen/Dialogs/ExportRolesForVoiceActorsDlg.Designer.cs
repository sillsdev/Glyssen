namespace Glyssen.Dialogs
{
	partial class ExportRolesForVoiceActorsDlg
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
			L10NSharp.UI.LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
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
			this.m_lblDescription = new System.Windows.Forms.Label();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnChange = new System.Windows.Forms.Button();
			this.m_lblFileName = new System.Windows.Forms.Label();
			this.m_lblFileExists = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.m_checkOpenForMe = new System.Windows.Forms.CheckBox();
			this.m_lblDescription2 = new System.Windows.Forms.Label();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
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
			this.m_btnCancel.Location = new System.Drawing.Point(405, 186);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.Ok");
			this.m_btnOk.Location = new System.Drawing.Point(324, 186);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 0;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// m_lblDescription
			// 
			this.m_lblDescription.AutoSize = true;
			this.m_lblDescription.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.SetColumnSpan(this.m_lblDescription, 4);
			this.glyssenColorPalette.SetForeColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDescription.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblDescription, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription, "DialogBoxes.ExportRolesForVoiceActorsDlg.Description");
			this.m_lblDescription.Location = new System.Drawing.Point(3, 0);
			this.m_lblDescription.Name = "m_lblDescription";
			this.m_lblDescription.Size = new System.Drawing.Size(247, 13);
			this.m_lblDescription.TabIndex = 0;
			this.m_lblDescription.Text = "{0} will create Roles for Voice Actors as an xlsx file.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDescription, true);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_btnChange
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnChange, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnChange, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnChange, "DialogBoxes.ExportRolesForVoiceActorsDlg.Change");
			this.m_btnChange.Location = new System.Drawing.Point(390, 39);
			this.m_btnChange.MaximumSize = new System.Drawing.Size(75, 23);
			this.m_btnChange.MinimumSize = new System.Drawing.Size(75, 23);
			this.m_btnChange.Name = "m_btnChange";
			this.m_btnChange.Size = new System.Drawing.Size(75, 23);
			this.m_btnChange.TabIndex = 3;
			this.m_btnChange.Text = "Change...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnChange, false);
			this.m_btnChange.UseVisualStyleBackColor = true;
			this.m_btnChange.Click += new System.EventHandler(this.Browse_Click);
			// 
			// m_lblFileName
			// 
			this.m_lblFileName.AutoEllipsis = true;
			this.m_lblFileName.AutoSize = true;
			this.m_lblFileName.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFileName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblFileName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFileName.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFileName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFileName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblFileName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileName, "DialogBoxes.ExportRolesForVoiceActorsDlg.label1");
			this.m_lblFileName.Location = new System.Drawing.Point(64, 39);
			this.m_lblFileName.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFileName.Name = "m_lblFileName";
			this.m_lblFileName.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_lblFileName.Size = new System.Drawing.Size(54, 18);
			this.m_lblFileName.TabIndex = 2;
			this.m_lblFileName.Text = "{filename}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFileName, true);
			// 
			// m_lblFileExists
			// 
			this.m_lblFileExists.AutoSize = true;
			this.m_lblFileExists.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFileExists, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.SetColumnSpan(this.m_lblFileExists, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblFileExists, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblFileExists.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileExists, "DialogBoxes.ExportRolesForVoiceActorsDlg.FileExists");
			this.m_lblFileExists.Location = new System.Drawing.Point(28, 68);
			this.m_lblFileExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFileExists.Name = "m_lblFileExists";
			this.m_lblFileExists.Size = new System.Drawing.Size(183, 13);
			this.m_lblFileExists.TabIndex = 4;
			this.m_lblFileExists.Text = "This file exists and will be overwritten.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFileExists, true);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label3, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.SetColumnSpan(this.label3, 2);
			this.glyssenColorPalette.SetForeColor(this.label3, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label3, "DialogBoxes.ExportRolesForVoiceActorsDlg.FileName");
			this.label3.Location = new System.Drawing.Point(3, 39);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.label3.Size = new System.Drawing.Size(55, 18);
			this.label3.TabIndex = 1;
			this.label3.Text = "File name:";
			this.glyssenColorPalette.SetUsePaletteColors(this.label3, true);
			// 
			// m_checkOpenForMe
			// 
			this.m_checkOpenForMe.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_checkOpenForMe.BackColor = System.Drawing.SystemColors.Control;
			this.m_checkOpenForMe.Checked = true;
			this.m_checkOpenForMe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tableLayoutPanelMain.SetColumnSpan(this.m_checkOpenForMe, 4);
			this.m_checkOpenForMe.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkOpenForMe.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkOpenForMe, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkOpenForMe, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkOpenForMe, "DialogBoxes.ExportRolesForVoiceActorsDlg.OpenForMe");
			this.m_checkOpenForMe.Location = new System.Drawing.Point(3, 97);
			this.m_checkOpenForMe.Name = "m_checkOpenForMe";
			this.m_checkOpenForMe.Size = new System.Drawing.Size(145, 17);
			this.m_checkOpenForMe.TabIndex = 5;
			this.m_checkOpenForMe.Text = "Open the directory for me";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkOpenForMe, true);
			this.m_checkOpenForMe.UseVisualStyleBackColor = true;
			// 
			// m_lblDescription2
			// 
			this.m_lblDescription2.AutoSize = true;
			this.m_lblDescription2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDescription2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.SetColumnSpan(this.m_lblDescription2, 4);
			this.glyssenColorPalette.SetForeColor(this.m_lblDescription2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDescription2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription2, "DialogBoxes.ExportRolesForVoiceActorsDlg.Description2");
			this.m_lblDescription2.Location = new System.Drawing.Point(3, 13);
			this.m_lblDescription2.Name = "m_lblDescription2";
			this.m_lblDescription2.Size = new System.Drawing.Size(292, 13);
			this.m_lblDescription2.TabIndex = 9;
			this.m_lblDescription2.Text = "Use Excel, LibreOffice, or a similar application to print the list.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDescription2, true);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMain, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.ColumnCount = 4;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblDescription2, 0, 1);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblFileName, 2, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_btnChange, 3, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.label3, 0, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblFileExists, 1, 4);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblDescription, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_checkOpenForMe, 0, 6);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMain, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(12, 12);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 7;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(468, 168);
			this.m_tableLayoutPanelMain.TabIndex = 8;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMain, false);
			// 
			// ExportRolesForVoiceActorsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(492, 221);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ExportRolesForVoiceActorsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportRolesForVoiceActorsDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export Roles for Voice Actors";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMain.ResumeLayout(false);
			this.m_tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Label m_lblDescription;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.Button m_btnChange;
		private System.Windows.Forms.Label m_lblFileName;
		private System.Windows.Forms.Label m_lblFileExists;
		private System.Windows.Forms.Label label3;
		private Glyssen.Utilities.GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.CheckBox m_checkOpenForMe;
		private System.Windows.Forms.Label m_lblDescription2;
	}
}