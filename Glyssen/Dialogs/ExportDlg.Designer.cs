using Glyssen.Utilities;

namespace Glyssen.Dialogs
{
	partial class ExportDlg
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
			this.m_checkIncludeBookBreakdown = new System.Windows.Forms.CheckBox();
			this.m_lblDescription = new System.Windows.Forms.Label();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_checkIncludeActorBreakdown = new System.Windows.Forms.CheckBox();
			this.m_btnBrowse = new System.Windows.Forms.Button();
			this.m_lblFileName = new System.Windows.Forms.Label();
			this.m_lblFileExists = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.m_lblActorDirectoryExists = new System.Windows.Forms.Label();
			this.m_lblActorDirectory = new System.Windows.Forms.Label();
			this.m_lblBookDirectory = new System.Windows.Forms.Label();
			this.m_lblBookDirectoryExists = new System.Windows.Forms.Label();
			this.m_tableLayoutPanelMan = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMan.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(405, 271);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 4;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.Ok");
			this.m_btnOk.Location = new System.Drawing.Point(324, 271);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 3;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// m_checkIncludeBookBreakdown
			// 
			this.m_checkIncludeBookBreakdown.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkIncludeBookBreakdown, 4);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkIncludeBookBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkIncludeBookBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkIncludeBookBreakdown, "DialogBoxes.ExportDlg.ScriptPerBook");
			this.m_checkIncludeBookBreakdown.Location = new System.Drawing.Point(3, 145);
			this.m_checkIncludeBookBreakdown.Name = "m_checkIncludeBookBreakdown";
			this.m_checkIncludeBookBreakdown.Size = new System.Drawing.Size(213, 17);
			this.m_checkIncludeBookBreakdown.TabIndex = 6;
			this.m_checkIncludeBookBreakdown.Text = "Also create one script file for each book";
			this.m_checkIncludeBookBreakdown.UseVisualStyleBackColor = true;
			this.m_checkIncludeBookBreakdown.CheckedChanged += new System.EventHandler(this.CheckIncludeBookBreakdown_CheckedChanged);
			// 
			// m_lblDescription
			// 
			this.m_lblDescription.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblDescription, 4);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblDescription, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription, "DialogBoxes.ExportDlg.Description");
			this.m_lblDescription.Location = new System.Drawing.Point(3, 0);
			this.m_lblDescription.Name = "m_lblDescription";
			this.m_lblDescription.Size = new System.Drawing.Size(261, 13);
			this.m_lblDescription.TabIndex = 7;
			this.m_lblDescription.Text = "{0} will export your script to the location of your choice";
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_checkIncludeActorBreakdown
			// 
			this.m_checkIncludeActorBreakdown.AutoSize = true;
			this.m_checkIncludeActorBreakdown.Checked = true;
			this.m_checkIncludeActorBreakdown.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkIncludeActorBreakdown, 4);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkIncludeActorBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkIncludeActorBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkIncludeActorBreakdown, "DialogBoxes.ExportDlg.ScriptPerActor");
			this.m_checkIncludeActorBreakdown.Location = new System.Drawing.Point(3, 84);
			this.m_checkIncludeActorBreakdown.Name = "m_checkIncludeActorBreakdown";
			this.m_checkIncludeActorBreakdown.Size = new System.Drawing.Size(242, 17);
			this.m_checkIncludeActorBreakdown.TabIndex = 8;
			this.m_checkIncludeActorBreakdown.Text = "Also create one script file for each voice actor";
			this.m_checkIncludeActorBreakdown.UseVisualStyleBackColor = true;
			this.m_checkIncludeActorBreakdown.CheckedChanged += new System.EventHandler(this.CheckIncludeActorBreakdown_CheckedChanged);
			// 
			// m_btnBrowse
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnBrowse, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnBrowse, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnBrowse, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnBrowse, "DialogBoxes.ExportDlg.Browse");
			this.m_btnBrowse.Location = new System.Drawing.Point(390, 26);
			this.m_btnBrowse.MaximumSize = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.MinimumSize = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.Name = "m_btnBrowse";
			this.m_btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.m_btnBrowse.TabIndex = 9;
			this.m_btnBrowse.Text = "Browse...";
			this.m_btnBrowse.UseVisualStyleBackColor = true;
			this.m_btnBrowse.Click += new System.EventHandler(this.Browse_Click);
			// 
			// m_lblFileName
			// 
			this.m_lblFileName.AutoEllipsis = true;
			this.m_lblFileName.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFileName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFileName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblFileName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileName, "DialogBoxes.ExportDlg.label1");
			this.m_lblFileName.Location = new System.Drawing.Point(64, 26);
			this.m_lblFileName.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFileName.Name = "m_lblFileName";
			this.m_lblFileName.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_lblFileName.Size = new System.Drawing.Size(54, 18);
			this.m_lblFileName.TabIndex = 11;
			this.m_lblFileName.Text = "{filename}";
			// 
			// m_lblFileExists
			// 
			this.m_lblFileExists.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblFileExists, 3);
			this.m_lblFileExists.ForeColor = CustomColor.Warning;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileExists, "DialogBoxes.ExportDlg.FileExists");
			this.m_lblFileExists.Location = new System.Drawing.Point(28, 55);
			this.m_lblFileExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFileExists.Name = "m_lblFileExists";
			this.m_lblFileExists.Size = new System.Drawing.Size(183, 13);
			this.m_lblFileExists.TabIndex = 12;
			this.m_lblFileExists.Text = "This file exists and will be overwritten.";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.label3, 2);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label3, "DialogBoxes.ExportDlg.FileName");
			this.label3.Location = new System.Drawing.Point(3, 26);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.label3.Size = new System.Drawing.Size(55, 18);
			this.label3.TabIndex = 15;
			this.label3.Text = "File name:";
			// 
			// m_lblActorDirectoryExists
			// 
			this.m_lblActorDirectoryExists.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblActorDirectoryExists, 3);
			this.m_lblActorDirectoryExists.ForeColor = CustomColor.Warning;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblActorDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblActorDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblActorDirectoryExists, "DialogBoxes.ExportDlg.ActorDirectoryExists");
			this.m_lblActorDirectoryExists.Location = new System.Drawing.Point(28, 126);
			this.m_lblActorDirectoryExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblActorDirectoryExists.Name = "m_lblActorDirectoryExists";
			this.m_lblActorDirectoryExists.Size = new System.Drawing.Size(278, 13);
			this.m_lblActorDirectoryExists.TabIndex = 16;
			this.m_lblActorDirectoryExists.Text = "This directory exists.  Voice actors files will be overwritten.";
			// 
			// m_lblActorDirectory
			// 
			this.m_lblActorDirectory.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblActorDirectory, 3);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblActorDirectory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblActorDirectory, "{0} is a directory");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblActorDirectory, "DialogBoxes.ExportDlg.FilesWillBeCreated");
			this.m_lblActorDirectory.Location = new System.Drawing.Point(28, 107);
			this.m_lblActorDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblActorDirectory.Name = "m_lblActorDirectory";
			this.m_lblActorDirectory.Size = new System.Drawing.Size(127, 13);
			this.m_lblActorDirectory.TabIndex = 12;
			this.m_lblActorDirectory.Text = "Files will be created in {0}";
			// 
			// m_lblBookDirectory
			// 
			this.m_lblBookDirectory.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblBookDirectory, 3);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookDirectory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookDirectory, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookDirectory, "DialogBoxes.ExportDlg.FilesWillBeCreated");
			this.m_lblBookDirectory.Location = new System.Drawing.Point(28, 168);
			this.m_lblBookDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblBookDirectory.Name = "m_lblBookDirectory";
			this.m_lblBookDirectory.Size = new System.Drawing.Size(127, 13);
			this.m_lblBookDirectory.TabIndex = 17;
			this.m_lblBookDirectory.Text = "Files will be created in {0}";
			// 
			// m_lblBookDirectoryExists
			// 
			this.m_lblBookDirectoryExists.AutoSize = true;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblBookDirectoryExists, 3);
			this.m_lblBookDirectoryExists.ForeColor = CustomColor.Warning;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookDirectoryExists, "DialogBoxes.ExportDlg.ActorDirectoryExists");
			this.m_lblBookDirectoryExists.Location = new System.Drawing.Point(28, 187);
			this.m_lblBookDirectoryExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblBookDirectoryExists.Name = "m_lblBookDirectoryExists";
			this.m_lblBookDirectoryExists.Size = new System.Drawing.Size(244, 13);
			this.m_lblBookDirectoryExists.TabIndex = 18;
			this.m_lblBookDirectoryExists.Text = "This directory exists.  Book files will be overwritten.";
			// 
			// m_tableLayoutPanelMan
			// 
			this.m_tableLayoutPanelMan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelMan.ColumnCount = 4;
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFileName, 2, 2);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_btnBrowse, 3, 2);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkIncludeActorBreakdown, 0, 5);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkIncludeBookBreakdown, 0, 8);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblDescription, 0, 0);
			this.m_tableLayoutPanelMan.Controls.Add(this.label3, 0, 2);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFileExists, 1, 3);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblActorDirectoryExists, 1, 7);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblActorDirectory, 1, 6);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblBookDirectory, 1, 9);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblBookDirectoryExists, 1, 10);
			this.m_tableLayoutPanelMan.Location = new System.Drawing.Point(12, 12);
			this.m_tableLayoutPanelMan.Name = "m_tableLayoutPanelMan";
			this.m_tableLayoutPanelMan.RowCount = 11;
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.Size = new System.Drawing.Size(468, 253);
			this.m_tableLayoutPanelMan.TabIndex = 8;
			// 
			// ExportDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(492, 306);
			this.Controls.Add(this.m_tableLayoutPanelMan);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ExportDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMan.ResumeLayout(false);
			this.m_tableLayoutPanelMan.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.CheckBox m_checkIncludeBookBreakdown;
		private System.Windows.Forms.Label m_lblDescription;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMan;
		private System.Windows.Forms.CheckBox m_checkIncludeActorBreakdown;
		private System.Windows.Forms.Button m_btnBrowse;
		private System.Windows.Forms.Label m_lblFileName;
		private System.Windows.Forms.Label m_lblFileExists;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label m_lblActorDirectory;
		private System.Windows.Forms.Label m_lblActorDirectoryExists;
		private System.Windows.Forms.Label m_lblBookDirectory;
		private System.Windows.Forms.Label m_lblBookDirectoryExists;
	}
}