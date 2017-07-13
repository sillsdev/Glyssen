using Glyssen.Utilities;

namespace Glyssen.Dialogs
{
	partial class ExportToRecordingToolDlg
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
			this.m_lblDescription = new System.Windows.Forms.Label();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnBrowse = new System.Windows.Forms.Button();
			this.m_lblFilenameLabel = new System.Windows.Forms.Label();
			this.m_fileNameTextBox = new System.Windows.Forms.TextBox();
			this.m_lblImportant = new System.Windows.Forms.Label();
			this.m_lblWarning = new System.Windows.Forms.Label();
			this.m_tableLayoutPanelMan = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMan.SuspendLayout();
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
			this.m_btnCancel.Location = new System.Drawing.Point(426, 192);
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
			this.m_btnOk.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(345, 192);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 0;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.Ok_Click);
			// 
			// m_lblDescription
			// 
			this.m_lblDescription.AutoSize = true;
			this.m_lblDescription.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblDescription, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDescription.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription, "{0} is \"Glyssen\", {1} is a file extension, {2} is \"HearThis\"");
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblDescription, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription, "DialogBoxes.ExportDlg.Description");
			this.m_lblDescription.Location = new System.Drawing.Point(3, 0);
			this.m_lblDescription.Name = "m_lblDescription";
			this.m_lblDescription.Size = new System.Drawing.Size(306, 13);
			this.m_lblDescription.TabIndex = 0;
			this.m_lblDescription.Text = "{0} will create a {1} file which can be used by {2} 2.0 or greater.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDescription, true);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_btnBrowse
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnBrowse, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnBrowse, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnBrowse, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnBrowse, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnBrowse, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnBrowse, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnBrowse, "DialogBoxes.ExportToRecordingDlg.Browse");
			this.m_btnBrowse.Location = new System.Drawing.Point(413, 99);
			this.m_btnBrowse.MaximumSize = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.MinimumSize = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.Name = "m_btnBrowse";
			this.m_btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.TabIndex = 3;
			this.m_btnBrowse.Text = "Browse...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnBrowse, false);
			this.m_btnBrowse.UseVisualStyleBackColor = true;
			this.m_btnBrowse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// m_lblFilenameLabel
			// 
			this.m_lblFilenameLabel.AutoSize = true;
			this.m_lblFilenameLabel.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFilenameLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblFilenameLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFilenameLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFilenameLabel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFilenameLabel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFilenameLabel, "DialogBoxes.ExportToRecordingToolDlg.SelectFileLocation");
			this.m_lblFilenameLabel.Location = new System.Drawing.Point(3, 75);
			this.m_lblFilenameLabel.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFilenameLabel.Name = "m_lblFilenameLabel";
			this.m_lblFilenameLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_lblFilenameLabel.Size = new System.Drawing.Size(170, 18);
			this.m_lblFilenameLabel.TabIndex = 1;
			this.m_lblFilenameLabel.Text = "Select the location to save the file:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFilenameLabel, true);
			// 
			// m_fileNameTextBox
			// 
			this.m_fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_fileNameTextBox, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_fileNameTextBox, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_fileNameTextBox, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_fileNameTextBox, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_fileNameTextBox, "DialogBoxes.textBox1");
			this.m_fileNameTextBox.Location = new System.Drawing.Point(3, 99);
			this.m_fileNameTextBox.Multiline = true;
			this.m_fileNameTextBox.Name = "m_fileNameTextBox";
			this.m_fileNameTextBox.ReadOnly = true;
			this.m_fileNameTextBox.Size = new System.Drawing.Size(404, 23);
			this.m_fileNameTextBox.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_fileNameTextBox, false);
			this.m_fileNameTextBox.TextChanged += new System.EventHandler(this.FileNameTextBox_TextChanged);
			// 
			// m_lblImportant
			// 
			this.m_lblImportant.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lblImportant, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblImportant, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblImportant.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblImportant, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblImportant, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblImportant, "DialogBoxes.ExportToRecordingToolDlg.Important");
			this.m_lblImportant.Location = new System.Drawing.Point(3, 23);
			this.m_lblImportant.Name = "m_lblImportant";
			this.m_lblImportant.Size = new System.Drawing.Size(54, 13);
			this.m_lblImportant.TabIndex = 5;
			this.m_lblImportant.Text = "Important:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblImportant, false);
			// 
			// m_lblWarning
			// 
			this.m_lblWarning.AutoSize = true;
			this.m_lblWarning.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblWarning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblWarning, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblWarning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblWarning.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWarning, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWarning, "{0} is \"HearThis\"");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWarning, "DialogBoxes.ExportToRecordingToolDlg.ReadyToExportNotice");
			this.m_lblWarning.Location = new System.Drawing.Point(3, 36);
			this.m_lblWarning.Name = "m_lblWarning";
			this.m_lblWarning.Size = new System.Drawing.Size(481, 26);
			this.m_lblWarning.TabIndex = 6;
			this.m_lblWarning.Text = "{0} is not currently able to handle changes to the recording script once recordin" +
    "g has begun. For this reason, it is important that you do not export to {0} unti" +
    "l you are ready to begin recording.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblWarning, true);
			// 
			// m_tableLayoutPanelMan
			// 
			this.m_tableLayoutPanelMan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMan, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.ColumnCount = 2;
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMan.Controls.Add(this.m_btnBrowse, 1, 6);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblDescription, 0, 0);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFilenameLabel, 0, 5);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_fileNameTextBox, 0, 6);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblImportant, 0, 2);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblWarning, 0, 3);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMan, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMan.Location = new System.Drawing.Point(12, 12);
			this.m_tableLayoutPanelMan.Name = "m_tableLayoutPanelMan";
			this.m_tableLayoutPanelMan.RowCount = 8;
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMan.Size = new System.Drawing.Size(491, 174);
			this.m_tableLayoutPanelMan.TabIndex = 8;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMan, false);
			// 
			// ExportToRecordingToolDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(513, 227);
			this.Controls.Add(this.m_tableLayoutPanelMan);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ExportToRecordingToolDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportToRecordingToolDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export Recording Script to HearThis - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ExportToRecordingToolDlg_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMan.ResumeLayout(false);
			this.m_tableLayoutPanelMan.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Label m_lblDescription;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMan;
		private System.Windows.Forms.Button m_btnBrowse;
		private System.Windows.Forms.Label m_lblFilenameLabel;
		private GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.TextBox m_fileNameTextBox;
		private System.Windows.Forms.Label m_lblImportant;
		private System.Windows.Forms.Label m_lblWarning;
	}
}