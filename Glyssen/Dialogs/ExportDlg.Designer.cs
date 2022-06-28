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
			if (disposing)
				components?.Dispose();
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
			this.m_btnChange = new System.Windows.Forms.Button();
			this.m_lblFileName = new System.Windows.Forms.Label();
			this.m_lblFileExists = new System.Windows.Forms.Label();
			this.m_lblFilenameLabel = new System.Windows.Forms.Label();
			this.m_lblActorDirectoryExists = new System.Windows.Forms.Label();
			this.m_lblActorDirectory = new System.Windows.Forms.Label();
			this.m_lblBookDirectory = new System.Windows.Forms.Label();
			this.m_lblBookDirectoryExists = new System.Windows.Forms.Label();
			this.m_checkIncludeClipListFile = new System.Windows.Forms.CheckBox();
			this.m_lblClipListFileExists = new System.Windows.Forms.Label();
			this.m_lblClipListFilename = new System.Windows.Forms.Label();
			this.m_lblDescription2 = new System.Windows.Forms.Label();
			this.m_checkOpenForMe = new System.Windows.Forms.CheckBox();
			this.m_checkCreateClips = new System.Windows.Forms.CheckBox();
			this.m_lblClipDirectory = new System.Windows.Forms.Label();
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
			this.m_btnCancel.Location = new System.Drawing.Point(405, 402);
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
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(324, 402);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 0;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// m_checkIncludeBookBreakdown
			// 
			this.m_checkIncludeBookBreakdown.AutoSize = true;
			this.m_checkIncludeBookBreakdown.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkIncludeBookBreakdown, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkIncludeBookBreakdown, 3);
			this.m_checkIncludeBookBreakdown.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkIncludeBookBreakdown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkIncludeBookBreakdown.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkIncludeBookBreakdown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkIncludeBookBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkIncludeBookBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkIncludeBookBreakdown, "DialogBoxes.ExportDlg.ScriptPerBook");
			this.m_checkIncludeBookBreakdown.Location = new System.Drawing.Point(3, 182);
			this.m_checkIncludeBookBreakdown.Name = "m_checkIncludeBookBreakdown";
			this.m_checkIncludeBookBreakdown.Size = new System.Drawing.Size(213, 17);
			this.m_checkIncludeBookBreakdown.TabIndex = 8;
			this.m_checkIncludeBookBreakdown.Text = "Also create one script file for each book";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkIncludeBookBreakdown, true);
			this.m_checkIncludeBookBreakdown.UseVisualStyleBackColor = true;
			this.m_checkIncludeBookBreakdown.CheckedChanged += new System.EventHandler(this.CheckIncludeBookBreakdown_CheckedChanged);
			// 
			// m_lblDescription
			// 
			this.m_lblDescription.AutoSize = true;
			this.m_lblDescription.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblDescription, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblDescription, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDescription.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblDescription, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription, "DialogBoxes.ExportDlg.Description");
			this.m_lblDescription.Location = new System.Drawing.Point(3, 0);
			this.m_lblDescription.Name = "m_lblDescription";
			this.m_lblDescription.Size = new System.Drawing.Size(250, 13);
			this.m_lblDescription.TabIndex = 0;
			this.m_lblDescription.Text = "{0} will create recording scripts as spreadsheet files.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDescription, true);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_checkIncludeActorBreakdown
			// 
			this.m_checkIncludeActorBreakdown.AutoSize = true;
			this.m_checkIncludeActorBreakdown.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkIncludeActorBreakdown, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkIncludeActorBreakdown, 3);
			this.m_checkIncludeActorBreakdown.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkIncludeActorBreakdown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkIncludeActorBreakdown.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkIncludeActorBreakdown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkIncludeActorBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkIncludeActorBreakdown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkIncludeActorBreakdown, "DialogBoxes.ExportDlg.ScriptPerActor");
			this.m_checkIncludeActorBreakdown.Location = new System.Drawing.Point(3, 121);
			this.m_checkIncludeActorBreakdown.Name = "m_checkIncludeActorBreakdown";
			this.m_checkIncludeActorBreakdown.Size = new System.Drawing.Size(242, 17);
			this.m_checkIncludeActorBreakdown.TabIndex = 5;
			this.m_checkIncludeActorBreakdown.Text = "Also create one script file for each voice actor";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkIncludeActorBreakdown, true);
			this.m_checkIncludeActorBreakdown.UseVisualStyleBackColor = true;
			this.m_checkIncludeActorBreakdown.CheckedChanged += new System.EventHandler(this.CheckIncludeActorBreakdown_CheckedChanged);
			// 
			// m_btnChange
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnChange, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnChange, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnChange, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnChange, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnChange, "DialogBoxes.ExportDlg.Change");
			this.m_btnChange.Location = new System.Drawing.Point(390, 63);
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileName, "DialogBoxes.ExportDlg.m_lblFileName");
			this.m_lblFileName.Location = new System.Drawing.Point(28, 63);
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
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblFileExists, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblFileExists, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblFileExists.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFileExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFileExists, "DialogBoxes.ExportDlg.FileExists");
			this.m_lblFileExists.Location = new System.Drawing.Point(28, 92);
			this.m_lblFileExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFileExists.Name = "m_lblFileExists";
			this.m_lblFileExists.Size = new System.Drawing.Size(183, 13);
			this.m_lblFileExists.TabIndex = 4;
			this.m_lblFileExists.Text = "This file exists and will be overwritten.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFileExists, true);
			// 
			// m_lblFilenameLabel
			// 
			this.m_lblFilenameLabel.AutoSize = true;
			this.m_lblFilenameLabel.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFilenameLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblFilenameLabel, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblFilenameLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFilenameLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFilenameLabel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFilenameLabel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFilenameLabel, "DialogBoxes.ExportDlg.FileName");
			this.m_lblFilenameLabel.Location = new System.Drawing.Point(3, 39);
			this.m_lblFilenameLabel.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblFilenameLabel.Name = "m_lblFilenameLabel";
			this.m_lblFilenameLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_lblFilenameLabel.Size = new System.Drawing.Size(216, 18);
			this.m_lblFilenameLabel.TabIndex = 1;
			this.m_lblFilenameLabel.Text = "The master recording script will be saved as:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFilenameLabel, true);
			// 
			// m_lblActorDirectoryExists
			// 
			this.m_lblActorDirectoryExists.AutoSize = true;
			this.m_lblActorDirectoryExists.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblActorDirectoryExists, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblActorDirectoryExists, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblActorDirectoryExists, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblActorDirectoryExists.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblActorDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblActorDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblActorDirectoryExists, "DialogBoxes.ExportDlg.ActorDirectoryExists");
			this.m_lblActorDirectoryExists.Location = new System.Drawing.Point(28, 163);
			this.m_lblActorDirectoryExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblActorDirectoryExists.Name = "m_lblActorDirectoryExists";
			this.m_lblActorDirectoryExists.Size = new System.Drawing.Size(256, 13);
			this.m_lblActorDirectoryExists.TabIndex = 7;
			this.m_lblActorDirectoryExists.Text = "This folder exists. Voice actor files will be overwritten.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblActorDirectoryExists, true);
			// 
			// m_lblActorDirectory
			// 
			this.m_lblActorDirectory.AutoSize = true;
			this.m_lblActorDirectory.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblActorDirectory, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblActorDirectory, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblActorDirectory, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblActorDirectory.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblActorDirectory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblActorDirectory, "{0} is a directory");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblActorDirectory, "DialogBoxes.ExportDlg.FilesWillBeCreated");
			this.m_lblActorDirectory.Location = new System.Drawing.Point(28, 144);
			this.m_lblActorDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblActorDirectory.Name = "m_lblActorDirectory";
			this.m_lblActorDirectory.Size = new System.Drawing.Size(127, 13);
			this.m_lblActorDirectory.TabIndex = 6;
			this.m_lblActorDirectory.Text = "Files will be created in {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblActorDirectory, true);
			// 
			// m_lblBookDirectory
			// 
			this.m_lblBookDirectory.AutoSize = true;
			this.m_lblBookDirectory.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblBookDirectory, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblBookDirectory, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblBookDirectory, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblBookDirectory.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookDirectory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookDirectory, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookDirectory, "DialogBoxes.ExportDlg.FilesWillBeCreated");
			this.m_lblBookDirectory.Location = new System.Drawing.Point(28, 205);
			this.m_lblBookDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblBookDirectory.Name = "m_lblBookDirectory";
			this.m_lblBookDirectory.Size = new System.Drawing.Size(127, 13);
			this.m_lblBookDirectory.TabIndex = 9;
			this.m_lblBookDirectory.Text = "Files will be created in {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblBookDirectory, true);
			// 
			// m_lblBookDirectoryExists
			// 
			this.m_lblBookDirectoryExists.AutoSize = true;
			this.m_lblBookDirectoryExists.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblBookDirectoryExists, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblBookDirectoryExists, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblBookDirectoryExists, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblBookDirectoryExists.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBookDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBookDirectoryExists, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBookDirectoryExists, "DialogBoxes.ExportDlg.ActorDirectoryExists");
			this.m_lblBookDirectoryExists.Location = new System.Drawing.Point(28, 224);
			this.m_lblBookDirectoryExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblBookDirectoryExists.Name = "m_lblBookDirectoryExists";
			this.m_lblBookDirectoryExists.Size = new System.Drawing.Size(227, 13);
			this.m_lblBookDirectoryExists.TabIndex = 10;
			this.m_lblBookDirectoryExists.Text = "This folder exists. Book files will be overwritten.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblBookDirectoryExists, true);
			// 
			// m_checkIncludeClipListFile
			// 
			this.m_checkIncludeClipListFile.AutoSize = true;
			this.m_checkIncludeClipListFile.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkIncludeClipListFile, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkIncludeClipListFile, 3);
			this.m_checkIncludeClipListFile.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkIncludeClipListFile, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkIncludeClipListFile.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkIncludeClipListFile, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkIncludeClipListFile, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkIncludeClipListFile, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkIncludeClipListFile, "DialogBoxes.ExportDlg.IncludeClipListFile");
			this.m_checkIncludeClipListFile.Location = new System.Drawing.Point(3, 243);
			this.m_checkIncludeClipListFile.Name = "m_checkIncludeClipListFile";
			this.m_checkIncludeClipListFile.Size = new System.Drawing.Size(138, 17);
			this.m_checkIncludeClipListFile.TabIndex = 11;
			this.m_checkIncludeClipListFile.Text = "Also create a clip list file";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkIncludeClipListFile, true);
			this.m_checkIncludeClipListFile.UseVisualStyleBackColor = true;
			this.m_checkIncludeClipListFile.Visible = false;
			this.m_checkIncludeClipListFile.CheckedChanged += new System.EventHandler(this.CheckIncludeClipListFile_CheckedChanged);
			// 
			// m_lblClipListFileExists
			// 
			this.m_lblClipListFileExists.AutoSize = true;
			this.m_lblClipListFileExists.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblClipListFileExists, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblClipListFileExists, Glyssen.Utilities.GlyssenColors.Warning);
			this.m_lblClipListFileExists.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblClipListFileExists, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblClipListFileExists, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblClipListFileExists, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblClipListFileExists, "DialogBoxes.ExportDlg.ClipListFileExists");
			this.m_lblClipListFileExists.Location = new System.Drawing.Point(28, 285);
			this.m_lblClipListFileExists.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblClipListFileExists.Name = "m_lblClipListFileExists";
			this.m_lblClipListFileExists.Size = new System.Drawing.Size(183, 13);
			this.m_lblClipListFileExists.TabIndex = 13;
			this.m_lblClipListFileExists.Text = "This file exists and will be overwritten.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblClipListFileExists, true);
			this.m_lblClipListFileExists.Visible = false;
			// 
			// m_lblClipListFilename
			// 
			this.m_lblClipListFilename.AutoSize = true;
			this.m_lblClipListFilename.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblClipListFilename, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblClipListFilename, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblClipListFilename, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblClipListFilename.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblClipListFilename, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblClipListFilename, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblClipListFilename, "DialogBoxes.ExportDlg.ClipListFilename");
			this.m_lblClipListFilename.Location = new System.Drawing.Point(28, 266);
			this.m_lblClipListFilename.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblClipListFilename.Name = "m_lblClipListFilename";
			this.m_lblClipListFilename.Size = new System.Drawing.Size(72, 13);
			this.m_lblClipListFilename.TabIndex = 12;
			this.m_lblClipListFilename.Text = "File name: {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblClipListFilename, true);
			this.m_lblClipListFilename.Visible = false;
			// 
			// m_lblDescription2
			// 
			this.m_lblDescription2.AutoSize = true;
			this.m_lblDescription2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDescription2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblDescription2, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblDescription2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDescription2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDescription2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDescription2, "\"them\" refers to recording scripts saved as spreadsheet files.");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDescription2, "DialogBoxes.ExportDlg.Description2");
			this.m_lblDescription2.Location = new System.Drawing.Point(3, 13);
			this.m_lblDescription2.Name = "m_lblDescription2";
			this.m_lblDescription2.Size = new System.Drawing.Size(322, 13);
			this.m_lblDescription2.TabIndex = 14;
			this.m_lblDescription2.Text = "Use Excel, LibreOffice, or a similar application to view or print them.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDescription2, true);
			// 
			// m_checkOpenForMe
			// 
			this.m_checkOpenForMe.AutoSize = true;
			this.m_checkOpenForMe.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_checkOpenForMe.Checked = true;
			this.m_checkOpenForMe.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkOpenForMe, 3);
			this.m_checkOpenForMe.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkOpenForMe.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkOpenForMe, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkOpenForMe, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkOpenForMe, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkOpenForMe, "DialogBoxes.ExportDlg.OpenForMe");
			this.m_checkOpenForMe.Location = new System.Drawing.Point(3, 356);
			this.m_checkOpenForMe.Name = "m_checkOpenForMe";
			this.m_checkOpenForMe.Size = new System.Drawing.Size(162, 17);
			this.m_checkOpenForMe.TabIndex = 15;
			this.m_checkOpenForMe.Text = "Open the exported file for me";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkOpenForMe, true);
			this.m_checkOpenForMe.UseVisualStyleBackColor = true;
			// 
			// m_checkCreateClips
			// 
			this.m_checkCreateClips.AutoSize = true;
			this.m_checkCreateClips.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_checkCreateClips, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_checkCreateClips, 3);
			this.m_checkCreateClips.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_checkCreateClips, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_checkCreateClips.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_checkCreateClips, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_checkCreateClips, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_checkCreateClips, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_checkCreateClips, "DialogBoxes.ExportDlg.CreateClipFiles");
			this.m_checkCreateClips.Location = new System.Drawing.Point(3, 304);
			this.m_checkCreateClips.Name = "m_checkCreateClips";
			this.m_checkCreateClips.Size = new System.Drawing.Size(148, 17);
			this.m_checkCreateClips.TabIndex = 12;
			this.m_checkCreateClips.Text = "Also create blank clip files";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_checkCreateClips, true);
			this.m_checkCreateClips.UseVisualStyleBackColor = true;
			this.m_checkCreateClips.CheckedChanged += new System.EventHandler(this.CheckCreateClips_CheckedChanged);
			// 
			// m_lblClipDirectory
			// 
			this.m_lblClipDirectory.AutoSize = true;
			this.m_lblClipDirectory.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblClipDirectory, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.SetColumnSpan(this.m_lblClipDirectory, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblClipDirectory, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblClipDirectory.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblClipDirectory, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblClipDirectory, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblClipDirectory, "DialogBoxes.ExportDlg.FilesWillBeCreated");
			this.m_lblClipDirectory.Location = new System.Drawing.Point(28, 327);
			this.m_lblClipDirectory.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblClipDirectory.Name = "m_lblClipDirectory";
			this.m_lblClipDirectory.Size = new System.Drawing.Size(127, 13);
			this.m_lblClipDirectory.TabIndex = 10;
			this.m_lblClipDirectory.Text = "Files will be created in {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblClipDirectory, true);
			this.m_lblClipDirectory.Visible = false;
			// 
			// m_tableLayoutPanelMan
			// 
			this.m_tableLayoutPanelMan.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMan, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMan.ColumnCount = 3;
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblClipDirectory, 1, 17);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkCreateClips, 0, 16);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_btnChange, 2, 4);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkIncludeActorBreakdown, 0, 7);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkIncludeBookBreakdown, 0, 10);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFileExists, 1, 5);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblActorDirectoryExists, 1, 9);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblActorDirectory, 1, 8);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblBookDirectory, 1, 11);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblBookDirectoryExists, 1, 12);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkIncludeClipListFile, 0, 13);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblClipListFileExists, 1, 15);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblClipListFilename, 1, 14);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblDescription, 0, 0);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblDescription2, 0, 1);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_checkOpenForMe, 0, 19);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFilenameLabel, 0, 3);
			this.m_tableLayoutPanelMan.Controls.Add(this.m_lblFileName, 1, 4);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMan, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMan.Location = new System.Drawing.Point(12, 12);
			this.m_tableLayoutPanelMan.Name = "m_tableLayoutPanelMan";
			this.m_tableLayoutPanelMan.RowCount = 20;
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanelMan.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMan.Size = new System.Drawing.Size(468, 384);
			this.m_tableLayoutPanelMan.TabIndex = 8;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMan, false);
			// 
			// ExportDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(492, 437);
			this.Controls.Add(this.m_tableLayoutPanelMan);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ExportDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export Recording Script - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ExportDlg_Load);
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
		private System.Windows.Forms.CheckBox m_checkIncludeBookBreakdown;
		private System.Windows.Forms.Label m_lblDescription;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMan;
		private System.Windows.Forms.CheckBox m_checkIncludeActorBreakdown;
		private System.Windows.Forms.Button m_btnChange;
		private System.Windows.Forms.Label m_lblFileName;
		private System.Windows.Forms.Label m_lblFileExists;
		private System.Windows.Forms.Label m_lblFilenameLabel;
		private System.Windows.Forms.Label m_lblActorDirectory;
		private System.Windows.Forms.Label m_lblActorDirectoryExists;
		private System.Windows.Forms.Label m_lblBookDirectory;
		private System.Windows.Forms.Label m_lblBookDirectoryExists;
		private System.Windows.Forms.CheckBox m_checkIncludeClipListFile;
		private System.Windows.Forms.Label m_lblClipListFileExists;
		private System.Windows.Forms.Label m_lblClipListFilename;
		private GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.Label m_lblDescription2;
		private System.Windows.Forms.CheckBox m_checkOpenForMe;
		private System.Windows.Forms.Label m_lblClipDirectory;
		private System.Windows.Forms.CheckBox m_checkCreateClips;
	}
}
