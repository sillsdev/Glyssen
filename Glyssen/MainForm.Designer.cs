namespace Glyssen
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.m_btnOpenProject = new System.Windows.Forms.Button();
			this.m_L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_lblProjectInfo = new System.Windows.Forms.Label();
			this.m_lblSettingsInfo = new System.Windows.Forms.Label();
			this.m_btnExportToTabSeparated = new System.Windows.Forms.Button();
			this.m_btnAssign = new System.Windows.Forms.Button();
			this.m_btnSelectBooks = new System.Windows.Forms.Button();
			this.m_btnSettings = new System.Windows.Forms.Button();
			this.m_lblPercentAssigned = new System.Windows.Forms.Label();
			this.m_lblSelectNextTask = new System.Windows.Forms.Label();
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_btnAbout = new System.Windows.Forms.ToolStripButton();
			this.m_uiLanguageMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.m_lnkExit = new System.Windows.Forms.LinkLabel();
			this.m_lblBookSelectionInfo = new System.Windows.Forms.Label();
			this.m_imgCheckOpen = new System.Windows.Forms.PictureBox();
			this.m_imgCheckSettings = new System.Windows.Forms.PictureBox();
			this.m_imgCheckBooks = new System.Windows.Forms.PictureBox();
			this.m_imgCheckAssign = new System.Windows.Forms.PictureBox();
			this.m_btnAssignVoiceActors = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckOpen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckSettings)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckBooks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssign)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnOpenProject
			// 
			this.m_btnOpenProject.BackColor = System.Drawing.Color.Transparent;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnOpenProject, "Choose a recording project to work on");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnOpenProject, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnOpenProject, "MainForm.OpenProject");
			this.m_btnOpenProject.Location = new System.Drawing.Point(29, 59);
			this.m_btnOpenProject.Name = "m_btnOpenProject";
			this.m_btnOpenProject.Size = new System.Drawing.Size(134, 23);
			this.m_btnOpenProject.TabIndex = 0;
			this.m_btnOpenProject.Text = "(1) Open Project...";
			this.m_btnOpenProject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnOpenProject.UseVisualStyleBackColor = false;
			this.m_btnOpenProject.Click += new System.EventHandler(this.HandleOpenProject_Click);
			// 
			// m_L10NSharpExtender
			// 
			this.m_L10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_L10NSharpExtender.PrefixForNewItems = "MainForm";
			// 
			// m_lblProjectInfo
			// 
			this.m_lblProjectInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblProjectInfo.AutoEllipsis = true;
			this.m_lblProjectInfo.ForeColor = System.Drawing.Color.White;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblProjectInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblProjectInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblProjectInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblProjectInfo, "MainForm.MainForm.m_lblBundleId");
			this.m_lblProjectInfo.Location = new System.Drawing.Point(197, 64);
			this.m_lblProjectInfo.Name = "m_lblProjectInfo";
			this.m_lblProjectInfo.Size = new System.Drawing.Size(312, 13);
			this.m_lblProjectInfo.TabIndex = 3;
			this.m_lblProjectInfo.Text = "{0}";
			// 
			// m_lblSettingsInfo
			// 
			this.m_lblSettingsInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblSettingsInfo.AutoEllipsis = true;
			this.m_lblSettingsInfo.ForeColor = System.Drawing.Color.White;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblSettingsInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblSettingsInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblSettingsInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblSettingsInfo, "MainForm.MainForm.m_lblLanguage");
			this.m_lblSettingsInfo.Location = new System.Drawing.Point(197, 93);
			this.m_lblSettingsInfo.Name = "m_lblSettingsInfo";
			this.m_lblSettingsInfo.Size = new System.Drawing.Size(312, 13);
			this.m_lblSettingsInfo.TabIndex = 5;
			this.m_lblSettingsInfo.Text = "{0}";
			// 
			// m_btnExportToTabSeparated
			// 
			this.m_btnExportToTabSeparated.BackColor = System.Drawing.Color.Transparent;
			this.m_btnExportToTabSeparated.Enabled = false;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnExportToTabSeparated, "Export to a tab-separated values file");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnExportToTabSeparated, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnExportToTabSeparated, "MainForm.Export");
			this.m_btnExportToTabSeparated.Location = new System.Drawing.Point(29, 194);
			this.m_btnExportToTabSeparated.Name = "m_btnExportToTabSeparated";
			this.m_btnExportToTabSeparated.Size = new System.Drawing.Size(134, 23);
			this.m_btnExportToTabSeparated.TabIndex = 4;
			this.m_btnExportToTabSeparated.Text = "(5) Export...";
			this.m_btnExportToTabSeparated.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnExportToTabSeparated.UseVisualStyleBackColor = false;
			this.m_btnExportToTabSeparated.Click += new System.EventHandler(this.HandleExportToTabSeparated_Click);
			// 
			// m_btnAssign
			// 
			this.m_btnAssign.BackColor = System.Drawing.Color.Transparent;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnAssign, "Select a Character ID for each block in the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnAssign, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnAssign, "MainForm.AssignCharacters");
			this.m_btnAssign.Location = new System.Drawing.Point(29, 146);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(134, 23);
			this.m_btnAssign.TabIndex = 3;
			this.m_btnAssign.Text = "(4) Assign Characters...";
			this.m_btnAssign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnAssign.UseVisualStyleBackColor = false;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_btnSelectBooks
			// 
			this.m_btnSelectBooks.BackColor = System.Drawing.Color.Transparent;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnSelectBooks, "Choose which books to include in the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnSelectBooks, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnSelectBooks, "MainForm.SelectBooks");
			this.m_btnSelectBooks.Location = new System.Drawing.Point(29, 117);
			this.m_btnSelectBooks.Name = "m_btnSelectBooks";
			this.m_btnSelectBooks.Size = new System.Drawing.Size(134, 23);
			this.m_btnSelectBooks.TabIndex = 2;
			this.m_btnSelectBooks.Text = "(3) Select Books...";
			this.m_btnSelectBooks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnSelectBooks.UseVisualStyleBackColor = false;
			this.m_btnSelectBooks.Click += new System.EventHandler(this.m_btnSelectBooks_Click);
			// 
			// m_btnSettings
			// 
			this.m_btnSettings.BackColor = System.Drawing.Color.Transparent;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnSettings, "Change the settings used to generate the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnSettings, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnSettings, "MainForm.ProjectSettings");
			this.m_btnSettings.Location = new System.Drawing.Point(29, 88);
			this.m_btnSettings.Name = "m_btnSettings";
			this.m_btnSettings.Size = new System.Drawing.Size(134, 23);
			this.m_btnSettings.TabIndex = 1;
			this.m_btnSettings.Text = "(2) Project Settings...";
			this.m_btnSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnSettings.UseVisualStyleBackColor = false;
			this.m_btnSettings.Click += new System.EventHandler(this.m_btnSettings_Click);
			// 
			// m_lblPercentAssigned
			// 
			this.m_lblPercentAssigned.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblPercentAssigned.AutoEllipsis = true;
			this.m_lblPercentAssigned.ForeColor = System.Drawing.Color.White;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblPercentAssigned, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblPercentAssigned, "{0:N1} is a number with one decimal point");
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblPercentAssigned, "MainForm.PercentComplete");
			this.m_lblPercentAssigned.Location = new System.Drawing.Point(197, 151);
			this.m_lblPercentAssigned.Name = "m_lblPercentAssigned";
			this.m_lblPercentAssigned.Size = new System.Drawing.Size(312, 13);
			this.m_lblPercentAssigned.TabIndex = 17;
			this.m_lblPercentAssigned.Text = "{0:N1}% complete";
			// 
			// m_lblSelectNextTask
			// 
			this.m_lblSelectNextTask.AutoSize = true;
			this.m_lblSelectNextTask.ForeColor = System.Drawing.Color.White;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblSelectNextTask, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblSelectNextTask, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblSelectNextTask, "MainForm.SelectTask");
			this.m_lblSelectNextTask.Location = new System.Drawing.Point(29, 34);
			this.m_lblSelectNextTask.Name = "m_lblSelectNextTask";
			this.m_lblSelectNextTask.Size = new System.Drawing.Size(177, 13);
			this.m_lblSelectNextTask.TabIndex = 19;
			this.m_lblSelectNextTask.Text = "Select the next task you want to do:";
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_btnAbout,
            this.m_uiLanguageMenu});
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_toolStrip, "MainForm.toolStrip1");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(15, 10, 20, 0);
			this.m_toolStrip.Size = new System.Drawing.Size(518, 32);
			this.m_toolStrip.TabIndex = 6;
			this.m_toolStrip.Text = "toolStrip1";
			// 
			// m_btnAbout
			// 
			this.m_btnAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_btnAbout.AutoToolTip = false;
			this.m_btnAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_btnAbout.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_btnAbout.Image = ((System.Drawing.Image)(resources.GetObject("m_btnAbout.Image")));
			this.m_btnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnAbout, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnAbout, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnAbout, "MainForm.About");
			this.m_btnAbout.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			this.m_btnAbout.Name = "m_btnAbout";
			this.m_btnAbout.Size = new System.Drawing.Size(53, 19);
			this.m_btnAbout.Text = "About...";
			this.m_btnAbout.Click += new System.EventHandler(this.m_btnAbout_Click);
			// 
			// m_uiLanguageMenu
			// 
			this.m_uiLanguageMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_uiLanguageMenu.AutoToolTip = false;
			this.m_uiLanguageMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_uiLanguageMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_uiLanguageMenu.Image = ((System.Drawing.Image)(resources.GetObject("m_uiLanguageMenu.Image")));
			this.m_uiLanguageMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_uiLanguageMenu, "");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_uiLanguageMenu, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_uiLanguageMenu, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_uiLanguageMenu, "MainForm.toolStripDropDownButton1");
			this.m_uiLanguageMenu.Name = "m_uiLanguageMenu";
			this.m_uiLanguageMenu.Size = new System.Drawing.Size(58, 19);
			this.m_uiLanguageMenu.Text = "English";
			this.m_uiLanguageMenu.ToolTipText = "User-interface Language";
			// 
			// m_lnkExit
			// 
			this.m_lnkExit.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_lnkExit.AutoSize = true;
			this.m_lnkExit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lnkExit, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lnkExit, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lnkExit, "MainForm.Exit");
			this.m_lnkExit.Location = new System.Drawing.Point(32, 248);
			this.m_lnkExit.Name = "m_lnkExit";
			this.m_lnkExit.Size = new System.Drawing.Size(24, 13);
			this.m_lnkExit.TabIndex = 5;
			this.m_lnkExit.TabStop = true;
			this.m_lnkExit.Text = "Exit";
			this.m_lnkExit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_lnkExit_LinkClicked);
			// 
			// m_lblBookSelectionInfo
			// 
			this.m_lblBookSelectionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblBookSelectionInfo.AutoEllipsis = true;
			this.m_lblBookSelectionInfo.ForeColor = System.Drawing.Color.White;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblBookSelectionInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblBookSelectionInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblBookSelectionInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblBookSelectionInfo, "MainForm.MainForm.m_lblLanguage");
			this.m_lblBookSelectionInfo.Location = new System.Drawing.Point(197, 122);
			this.m_lblBookSelectionInfo.Name = "m_lblBookSelectionInfo";
			this.m_lblBookSelectionInfo.Size = new System.Drawing.Size(312, 13);
			this.m_lblBookSelectionInfo.TabIndex = 24;
			this.m_lblBookSelectionInfo.Text = "{0}";
			// 
			// m_imgCheckOpen
			// 
			this.m_imgCheckOpen.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckOpen, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckOpen, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckOpen, "MainForm.SufficientlyCompleted");
			this.m_imgCheckOpen.Location = new System.Drawing.Point(166, 62);
			this.m_imgCheckOpen.Name = "m_imgCheckOpen";
			this.m_imgCheckOpen.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckOpen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckOpen.TabIndex = 26;
			this.m_imgCheckOpen.TabStop = false;
			this.m_imgCheckOpen.Visible = false;
			// 
			// m_imgCheckSettings
			// 
			this.m_imgCheckSettings.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckSettings, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckSettings, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckSettings, "MainForm.SufficientlyCompleted");
			this.m_imgCheckSettings.Location = new System.Drawing.Point(166, 91);
			this.m_imgCheckSettings.Name = "m_imgCheckSettings";
			this.m_imgCheckSettings.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckSettings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckSettings.TabIndex = 27;
			this.m_imgCheckSettings.TabStop = false;
			this.m_imgCheckSettings.Visible = false;
			// 
			// m_imgCheckBooks
			// 
			this.m_imgCheckBooks.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckBooks, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckBooks, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckBooks, "MainForm.SufficientlyCompleted");
			this.m_imgCheckBooks.Location = new System.Drawing.Point(166, 120);
			this.m_imgCheckBooks.Name = "m_imgCheckBooks";
			this.m_imgCheckBooks.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckBooks.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckBooks.TabIndex = 28;
			this.m_imgCheckBooks.TabStop = false;
			this.m_imgCheckBooks.Visible = false;
			// 
			// m_imgCheckAssign
			// 
			this.m_imgCheckAssign.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckAssign, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckAssign, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckAssign, "MainForm.SufficientlyCompleted");
			this.m_imgCheckAssign.Location = new System.Drawing.Point(166, 149);
			this.m_imgCheckAssign.Name = "m_imgCheckAssign";
			this.m_imgCheckAssign.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckAssign.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckAssign.TabIndex = 29;
			this.m_imgCheckAssign.TabStop = false;
			this.m_imgCheckAssign.Visible = false;
			// 
			// m_btnAssignVoiceActors
			// 
			this.m_btnAssignVoiceActors.BackColor = System.Drawing.Color.Transparent;
			this.m_btnAssignVoiceActors.Enabled = false;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnAssignVoiceActors, "Enter Voice Actor information and assign to Character Group.");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnAssignVoiceActors, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnAssignVoiceActors, "MainForm.AssignVoiceActors");
			this.m_btnAssignVoiceActors.Location = new System.Drawing.Point(29, 223);
			this.m_btnAssignVoiceActors.Name = "m_btnAssignVoiceActors";
			this.m_btnAssignVoiceActors.Size = new System.Drawing.Size(134, 23);
			this.m_btnAssignVoiceActors.TabIndex = 5;
			this.m_btnAssignVoiceActors.Text = "(6) Assign Voice Actors";
			this.m_btnAssignVoiceActors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.m_btnAssignVoiceActors.UseVisualStyleBackColor = false;
			this.m_btnAssignVoiceActors.Click += new System.EventHandler(this.m_btnAssignVoiceActors_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(518, 293);
			this.Controls.Add(this.m_btnAssignVoiceActors);
			this.Controls.Add(this.m_imgCheckAssign);
			this.Controls.Add(this.m_imgCheckBooks);
			this.Controls.Add(this.m_imgCheckSettings);
			this.Controls.Add(this.m_imgCheckOpen);
			this.Controls.Add(this.m_lblBookSelectionInfo);
			this.Controls.Add(this.m_lnkExit);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_lblSelectNextTask);
			this.Controls.Add(this.m_lblPercentAssigned);
			this.Controls.Add(this.m_btnSettings);
			this.Controls.Add(this.m_btnSelectBooks);
			this.Controls.Add(this.m_btnAssign);
			this.Controls.Add(this.m_btnExportToTabSeparated);
			this.Controls.Add(this.m_lblSettingsInfo);
			this.Controls.Add(this.m_lblProjectInfo);
			this.Controls.Add(this.m_btnOpenProject);
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this, "MainForm.WindowTitle");
			this.MinimumSize = new System.Drawing.Size(534, 331);
			this.Name = "MainForm";
			this.Text = "Glyssen";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckOpen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckSettings)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckBooks)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssign)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOpenProject;
		private L10NSharp.UI.L10NSharpExtender m_L10NSharpExtender;
		private System.Windows.Forms.Label m_lblProjectInfo;
		private System.Windows.Forms.Label m_lblSettingsInfo;
		private System.Windows.Forms.Button m_btnExportToTabSeparated;
		private System.Windows.Forms.Button m_btnAssign;
		private System.Windows.Forms.Button m_btnSelectBooks;
		private System.Windows.Forms.Button m_btnSettings;
		private System.Windows.Forms.Label m_lblPercentAssigned;
		private System.Windows.Forms.Label m_lblSelectNextTask;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripButton m_btnAbout;
		private System.Windows.Forms.ToolStripDropDownButton m_uiLanguageMenu;
		private System.Windows.Forms.LinkLabel m_lnkExit;
		private System.Windows.Forms.Label m_lblBookSelectionInfo;
		private System.Windows.Forms.PictureBox m_imgCheckOpen;
		private System.Windows.Forms.PictureBox m_imgCheckSettings;
		private System.Windows.Forms.PictureBox m_imgCheckBooks;
		private System.Windows.Forms.PictureBox m_imgCheckAssign;
		private System.Windows.Forms.Button m_btnAssignVoiceActors;
	}
}




