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
			if (disposing)
			{
				if (components != null)
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
			this.m_L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_btnAbout = new System.Windows.Forms.ToolStripButton();
			this.m_uiLanguageMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.m_shareMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.m_exportMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.m_importMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.m_lastExportLocationLink = new System.Windows.Forms.LinkLabel();
			this.m_lnkExit = new System.Windows.Forms.LinkLabel();
			this.m_btnOpenProject = new System.Windows.Forms.Button();
			this.m_imgCheckOpen = new System.Windows.Forms.PictureBox();
			this.m_lblActorsAssigned = new System.Windows.Forms.Label();
			this.m_imgCheckAssignActors = new System.Windows.Forms.PictureBox();
			this.m_lblProjectInfo = new System.Windows.Forms.Label();
			this.m_imgCheckSettings = new System.Windows.Forms.PictureBox();
			this.m_btnAssignVoiceActors = new System.Windows.Forms.Button();
			this.m_lblSettingsInfo = new System.Windows.Forms.Label();
			this.m_imgCheckAssignCharacters = new System.Windows.Forms.PictureBox();
			this.m_btnSettings = new System.Windows.Forms.Button();
			this.m_lblBookSelectionInfo = new System.Windows.Forms.Label();
			this.m_lblPercentAssigned = new System.Windows.Forms.Label();
			this.m_imgCheckBooks = new System.Windows.Forms.PictureBox();
			this.m_btnSelectBooks = new System.Windows.Forms.Button();
			this.m_btnIdentify = new System.Windows.Forms.Button();
			this.m_btnExport = new System.Windows.Forms.Button();
			this.m_lblSelectNextTask = new System.Windows.Forms.Label();
			this.m_btnCastSizePlanning = new System.Windows.Forms.Button();
			this.m_imgCastSizePlanning = new System.Windows.Forms.PictureBox();
			this.m_lblFilesAreHere = new System.Windows.Forms.Label();
			this.m_lblCastSizePlan = new System.Windows.Forms.Label();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckOpen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssignActors)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckSettings)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssignCharacters)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckBooks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCastSizePlanning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.m_tableLayoutPanel.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_L10NSharpExtender
			// 
			this.m_L10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_L10NSharpExtender.PrefixForNewItems = "MainForm";
			// 
			// m_toolStrip
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.glyssenColorPalette.SetForeColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.Default);
			this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_btnAbout,
            this.m_uiLanguageMenu,
            this.m_shareMenu});
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_toolStrip, "MainForm.toolStrip1");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(15, 10, 20, 0);
			this.m_toolStrip.Size = new System.Drawing.Size(572, 32);
			this.m_toolStrip.TabIndex = 8;
			this.m_toolStrip.Text = "toolStrip1";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStrip, false);
			// 
			// m_btnAbout
			// 
			this.m_btnAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_btnAbout.AutoToolTip = false;
			this.m_btnAbout.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_btnAbout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.glyssenColorPalette.SetForeColor(this.m_btnAbout, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_btnAbout.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.m_btnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnAbout, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnAbout, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnAbout, "MainForm.About");
			this.m_btnAbout.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			this.m_btnAbout.Name = "m_btnAbout";
			this.m_btnAbout.Size = new System.Drawing.Size(53, 19);
			this.m_btnAbout.Text = "About...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAbout, true);
			this.m_btnAbout.Click += new System.EventHandler(this.About_Click);
			// 
			// m_uiLanguageMenu
			// 
			this.m_uiLanguageMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_uiLanguageMenu.AutoToolTip = false;
			this.m_uiLanguageMenu.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_uiLanguageMenu, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_uiLanguageMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_uiLanguageMenu.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetForeColor(this.m_uiLanguageMenu, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_uiLanguageMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_uiLanguageMenu, "");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_uiLanguageMenu, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_uiLanguageMenu, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_uiLanguageMenu, "MainForm.toolStripDropDownButton1");
			this.m_uiLanguageMenu.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			this.m_uiLanguageMenu.Name = "m_uiLanguageMenu";
			this.m_uiLanguageMenu.Size = new System.Drawing.Size(58, 19);
			this.m_uiLanguageMenu.Text = "English";
			this.m_uiLanguageMenu.ToolTipText = "User-interface Language";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_uiLanguageMenu, true);
			// 
			// m_shareMenu
			// 
			this.m_shareMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_shareMenu.AutoToolTip = false;
			this.m_shareMenu.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_shareMenu, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_shareMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_shareMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_exportMenu,
            this.m_importMenu});
			this.m_shareMenu.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetForeColor(this.m_shareMenu, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_shareMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_shareMenu, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_shareMenu, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_shareMenu, "MainForm.Share");
			this.m_shareMenu.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			this.m_shareMenu.Name = "m_shareMenu";
			this.m_shareMenu.Size = new System.Drawing.Size(49, 19);
			this.m_shareMenu.Text = "Share";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_shareMenu, false);
			// 
			// m_exportMenu
			// 
			this.glyssenColorPalette.SetBackColor(this.m_exportMenu, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_exportMenu, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_exportMenu.ForeColor = System.Drawing.SystemColors.ControlText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_exportMenu, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_exportMenu, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_exportMenu, "MainForm.Export");
			this.m_exportMenu.Name = "m_exportMenu";
			this.m_exportMenu.Size = new System.Drawing.Size(110, 22);
			this.m_exportMenu.Text = "Export";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_exportMenu, false);
			this.m_exportMenu.Click += new System.EventHandler(this.Export_Click);
			// 
			// m_importMenu
			// 
			this.glyssenColorPalette.SetBackColor(this.m_importMenu, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_importMenu, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_importMenu.ForeColor = System.Drawing.SystemColors.ControlText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_importMenu, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_importMenu, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_importMenu, "MainForm.Import");
			this.m_importMenu.Name = "m_importMenu";
			this.m_importMenu.Size = new System.Drawing.Size(110, 22);
			this.m_importMenu.Text = "Import";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_importMenu, false);
			this.m_importMenu.Click += new System.EventHandler(this.Import_Click);
			// 
			// m_lastExportLocationLink
			// 
			this.m_lastExportLocationLink.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_lastExportLocationLink.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lastExportLocationLink.BackColor = System.Drawing.SystemColors.Control;
			this.m_lastExportLocationLink.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_lastExportLocationLink.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_lastExportLocationLink.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lastExportLocationLink, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lastExportLocationLink, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lastExportLocationLink, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lastExportLocationLink, "MainForm.LastExportLocation");
			this.m_lastExportLocationLink.Location = new System.Drawing.Point(76, 0);
			this.m_lastExportLocationLink.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.m_lastExportLocationLink.MaximumSize = new System.Drawing.Size(250, 0);
			this.m_lastExportLocationLink.Name = "m_lastExportLocationLink";
			this.m_lastExportLocationLink.Size = new System.Drawing.Size(95, 13);
			this.m_lastExportLocationLink.TabIndex = 32;
			this.m_lastExportLocationLink.TabStop = true;
			this.m_lastExportLocationLink.Text = "last export location";
			this.m_lastExportLocationLink.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lastExportLocationLink, true);
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_lastExportLocationLink, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_lastExportLocationLink.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_lastExportLocationLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_lastExportLocationLink_LinkClicked);
			// 
			// m_lnkExit
			// 
			this.m_lnkExit.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_lnkExit.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lnkExit.BackColor = System.Drawing.SystemColors.Control;
			this.m_lnkExit.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_lnkExit.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_lnkExit.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lnkExit, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lnkExit, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lnkExit, "MainForm.Exit");
			this.m_lnkExit.Location = new System.Drawing.Point(312, 0);
			this.m_lnkExit.Name = "m_lnkExit";
			this.m_lnkExit.Size = new System.Drawing.Size(24, 13);
			this.m_lnkExit.TabIndex = 7;
			this.m_lnkExit.TabStop = true;
			this.m_lnkExit.Text = "Exit";
			this.m_lnkExit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lnkExit, true);
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_lnkExit, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_lnkExit.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_lnkExit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.Exit_LinkClicked);
			// 
			// m_btnOpenProject
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnOpenProject, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOpenProject, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOpenProject, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnOpenProject, "Choose a recording project to work on");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnOpenProject, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnOpenProject, "MainForm.OpenProject");
			this.m_btnOpenProject.Location = new System.Drawing.Point(23, 35);
			this.m_btnOpenProject.Name = "m_btnOpenProject";
			this.m_btnOpenProject.Size = new System.Drawing.Size(159, 23);
			this.m_btnOpenProject.TabIndex = 0;
			this.m_btnOpenProject.Text = "({0}) Open Project...";
			this.m_btnOpenProject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOpenProject, false);
			this.m_btnOpenProject.UseVisualStyleBackColor = false;
			this.m_btnOpenProject.Click += new System.EventHandler(this.HandleOpenProject_Click);
			// 
			// m_imgCheckOpen
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCheckOpen, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCheckOpen, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCheckOpen.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckOpen, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckOpen, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckOpen, "MainForm.SufficientlyCompleted");
			this.m_imgCheckOpen.Location = new System.Drawing.Point(188, 35);
			this.m_imgCheckOpen.Name = "m_imgCheckOpen";
			this.m_imgCheckOpen.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckOpen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckOpen.TabIndex = 26;
			this.m_imgCheckOpen.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCheckOpen, false);
			this.m_imgCheckOpen.Visible = false;
			// 
			// m_lblActorsAssigned
			// 
			this.m_lblActorsAssigned.AutoEllipsis = true;
			this.m_lblActorsAssigned.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblActorsAssigned, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblActorsAssigned.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblActorsAssigned, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblActorsAssigned.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblActorsAssigned, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblActorsAssigned, "{0}  is the number of active actors and {1} is an expression indicating the numbe" +
        "r of assigned actors");
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblActorsAssigned, "MainForm.ActorsAssignedPlural");
			this.m_lblActorsAssigned.Location = new System.Drawing.Point(216, 177);
			this.m_lblActorsAssigned.Name = "m_lblActorsAssigned";
			this.m_lblActorsAssigned.Size = new System.Drawing.Size(333, 29);
			this.m_lblActorsAssigned.TabIndex = 30;
			this.m_lblActorsAssigned.Text = "{0} voice actors identified, {1}";
			this.m_lblActorsAssigned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblActorsAssigned, true);
			// 
			// m_imgCheckAssignActors
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCheckAssignActors, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCheckAssignActors, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCheckAssignActors.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckAssignActors, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckAssignActors, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckAssignActors, "MainForm.SufficientlyCompleted");
			this.m_imgCheckAssignActors.Location = new System.Drawing.Point(188, 180);
			this.m_imgCheckAssignActors.Name = "m_imgCheckAssignActors";
			this.m_imgCheckAssignActors.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckAssignActors.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckAssignActors.TabIndex = 31;
			this.m_imgCheckAssignActors.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCheckAssignActors, false);
			this.m_imgCheckAssignActors.Visible = false;
			// 
			// m_lblProjectInfo
			// 
			this.m_lblProjectInfo.AutoEllipsis = true;
			this.m_lblProjectInfo.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblProjectInfo, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblProjectInfo, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblProjectInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblProjectInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblProjectInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblProjectInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblProjectInfo, "MainForm.MainForm.m_lblBundleId");
			this.m_lblProjectInfo.Location = new System.Drawing.Point(216, 32);
			this.m_lblProjectInfo.Name = "m_lblProjectInfo";
			this.m_lblProjectInfo.Size = new System.Drawing.Size(333, 29);
			this.m_lblProjectInfo.TabIndex = 3;
			this.m_lblProjectInfo.Text = "{0}";
			this.m_lblProjectInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblProjectInfo, true);
			// 
			// m_imgCheckSettings
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCheckSettings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCheckSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCheckSettings.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckSettings, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckSettings, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckSettings, "MainForm.SufficientlyCompleted");
			this.m_imgCheckSettings.Location = new System.Drawing.Point(188, 64);
			this.m_imgCheckSettings.Name = "m_imgCheckSettings";
			this.m_imgCheckSettings.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckSettings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckSettings.TabIndex = 27;
			this.m_imgCheckSettings.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCheckSettings, false);
			this.m_imgCheckSettings.Visible = false;
			// 
			// m_btnAssignVoiceActors
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnAssignVoiceActors, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAssignVoiceActors.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnAssignVoiceActors, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnAssignVoiceActors, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnAssignVoiceActors, "Enter Voice Actor information and assign Voice Actors to Character Groups");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnAssignVoiceActors, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnAssignVoiceActors, "MainForm.AssignVoiceActors");
			this.m_btnAssignVoiceActors.Location = new System.Drawing.Point(23, 180);
			this.m_btnAssignVoiceActors.Name = "m_btnAssignVoiceActors";
			this.m_btnAssignVoiceActors.Size = new System.Drawing.Size(159, 23);
			this.m_btnAssignVoiceActors.TabIndex = 5;
			this.m_btnAssignVoiceActors.Text = "({0}) Assign Voice Actors...";
			this.m_btnAssignVoiceActors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAssignVoiceActors, false);
			this.m_btnAssignVoiceActors.UseVisualStyleBackColor = false;
			this.m_btnAssignVoiceActors.Click += new System.EventHandler(this.AssignVoiceActors_Click);
			// 
			// m_lblSettingsInfo
			// 
			this.m_lblSettingsInfo.AutoEllipsis = true;
			this.m_lblSettingsInfo.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSettingsInfo, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblSettingsInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblSettingsInfo, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSettingsInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblSettingsInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblSettingsInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblSettingsInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblSettingsInfo, "MainForm.MainForm.m_lblLanguage");
			this.m_lblSettingsInfo.Location = new System.Drawing.Point(216, 61);
			this.m_lblSettingsInfo.Name = "m_lblSettingsInfo";
			this.m_lblSettingsInfo.Size = new System.Drawing.Size(333, 29);
			this.m_lblSettingsInfo.TabIndex = 5;
			this.m_lblSettingsInfo.Text = "{0}";
			this.m_lblSettingsInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSettingsInfo, true);
			// 
			// m_imgCheckAssignCharacters
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCheckAssignCharacters, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCheckAssignCharacters, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCheckAssignCharacters.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckAssignCharacters, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckAssignCharacters, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckAssignCharacters, "MainForm.SufficientlyCompleted");
			this.m_imgCheckAssignCharacters.Location = new System.Drawing.Point(188, 122);
			this.m_imgCheckAssignCharacters.Name = "m_imgCheckAssignCharacters";
			this.m_imgCheckAssignCharacters.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckAssignCharacters.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckAssignCharacters.TabIndex = 29;
			this.m_imgCheckAssignCharacters.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCheckAssignCharacters, false);
			this.m_imgCheckAssignCharacters.Visible = false;
			// 
			// m_btnSettings
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnSettings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnSettings.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnSettings, "Change the settings used to generate the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnSettings, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnSettings, "MainForm.ProjectSettings");
			this.m_btnSettings.Location = new System.Drawing.Point(23, 64);
			this.m_btnSettings.Name = "m_btnSettings";
			this.m_btnSettings.Size = new System.Drawing.Size(159, 23);
			this.m_btnSettings.TabIndex = 1;
			this.m_btnSettings.Text = "({0}) Project Settings...";
			this.m_btnSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnSettings, false);
			this.m_btnSettings.UseVisualStyleBackColor = false;
			this.m_btnSettings.Click += new System.EventHandler(this.Settings_Click);
			// 
			// m_lblBookSelectionInfo
			// 
			this.m_lblBookSelectionInfo.AutoEllipsis = true;
			this.m_lblBookSelectionInfo.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblBookSelectionInfo, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblBookSelectionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblBookSelectionInfo, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblBookSelectionInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblBookSelectionInfo, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblBookSelectionInfo, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this.m_lblBookSelectionInfo, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblBookSelectionInfo, "MainForm.MainForm.m_lblLanguage");
			this.m_lblBookSelectionInfo.Location = new System.Drawing.Point(216, 90);
			this.m_lblBookSelectionInfo.Name = "m_lblBookSelectionInfo";
			this.m_lblBookSelectionInfo.Size = new System.Drawing.Size(333, 29);
			this.m_lblBookSelectionInfo.TabIndex = 24;
			this.m_lblBookSelectionInfo.Text = "{0}";
			this.m_lblBookSelectionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblBookSelectionInfo, true);
			// 
			// m_lblPercentAssigned
			// 
			this.m_lblPercentAssigned.AutoEllipsis = true;
			this.m_lblPercentAssigned.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblPercentAssigned, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblPercentAssigned.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblPercentAssigned, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblPercentAssigned.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblPercentAssigned, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblPercentAssigned, "{0:N1} is a number with one decimal point");
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblPercentAssigned, "MainForm.PercentComplete");
			this.m_lblPercentAssigned.Location = new System.Drawing.Point(216, 119);
			this.m_lblPercentAssigned.Name = "m_lblPercentAssigned";
			this.m_lblPercentAssigned.Size = new System.Drawing.Size(333, 29);
			this.m_lblPercentAssigned.TabIndex = 17;
			this.m_lblPercentAssigned.Text = "Assignments: {0:N1}% complete; Alignment to reference text: {1:N1}% complete";
			this.m_lblPercentAssigned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblPercentAssigned, true);
			// 
			// m_imgCheckBooks
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCheckBooks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCheckBooks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCheckBooks.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCheckBooks, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCheckBooks, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCheckBooks, "MainForm.SufficientlyCompleted");
			this.m_imgCheckBooks.Location = new System.Drawing.Point(188, 93);
			this.m_imgCheckBooks.Name = "m_imgCheckBooks";
			this.m_imgCheckBooks.Size = new System.Drawing.Size(22, 23);
			this.m_imgCheckBooks.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCheckBooks.TabIndex = 28;
			this.m_imgCheckBooks.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCheckBooks, false);
			this.m_imgCheckBooks.Visible = false;
			// 
			// m_btnSelectBooks
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnSelectBooks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnSelectBooks.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnSelectBooks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnSelectBooks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnSelectBooks, "Choose which books to include in the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnSelectBooks, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnSelectBooks, "MainForm.SelectBooks");
			this.m_btnSelectBooks.Location = new System.Drawing.Point(23, 93);
			this.m_btnSelectBooks.Name = "m_btnSelectBooks";
			this.m_btnSelectBooks.Size = new System.Drawing.Size(159, 23);
			this.m_btnSelectBooks.TabIndex = 2;
			this.m_btnSelectBooks.Text = "({0}) Select Books...";
			this.m_btnSelectBooks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnSelectBooks, false);
			this.m_btnSelectBooks.UseVisualStyleBackColor = false;
			this.m_btnSelectBooks.Click += new System.EventHandler(this.SelectBooks_Click);
			// 
			// m_btnIdentify
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnIdentify, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnIdentify.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnIdentify, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnIdentify, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnIdentify, "Select a Character ID for each block in the recording script");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnIdentify, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnIdentify, "MainForm.IdentifySpeakingParts");
			this.m_btnIdentify.Location = new System.Drawing.Point(23, 122);
			this.m_btnIdentify.Name = "m_btnIdentify";
			this.m_btnIdentify.Size = new System.Drawing.Size(159, 23);
			this.m_btnIdentify.TabIndex = 3;
			this.m_btnIdentify.Text = "({0}) Identify Speaking Parts...";
			this.m_btnIdentify.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnIdentify, false);
			this.m_btnIdentify.UseVisualStyleBackColor = false;
			this.m_btnIdentify.Click += new System.EventHandler(this.Assign_Click);
			// 
			// m_btnExport
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnExport, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnExport.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnExport, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnExport, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnExport, "View the script as a spreadsheet (Ctrl-E to bypass this view and open Export dial" +
        "og)");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnExport, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnExport, "MainForm.ViewScript");
			this.m_btnExport.Location = new System.Drawing.Point(23, 209);
			this.m_btnExport.Name = "m_btnExport";
			this.m_btnExport.Size = new System.Drawing.Size(159, 23);
			this.m_btnExport.TabIndex = 6;
			this.m_btnExport.Text = "({0}) View Recording Script...";
			this.m_btnExport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnExport, false);
			this.m_btnExport.UseVisualStyleBackColor = false;
			this.m_btnExport.Click += new System.EventHandler(this.View_Script_Click);
			// 
			// m_lblSelectNextTask
			// 
			this.m_lblSelectNextTask.AutoSize = true;
			this.m_lblSelectNextTask.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblSelectNextTask, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanel.SetColumnSpan(this.m_lblSelectNextTask, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblSelectNextTask, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSelectNextTask.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblSelectNextTask, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblSelectNextTask, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblSelectNextTask, "MainForm.SelectTask");
			this.m_lblSelectNextTask.Location = new System.Drawing.Point(23, 4);
			this.m_lblSelectNextTask.Margin = new System.Windows.Forms.Padding(3, 0, 3, 15);
			this.m_lblSelectNextTask.Name = "m_lblSelectNextTask";
			this.m_lblSelectNextTask.Size = new System.Drawing.Size(177, 13);
			this.m_lblSelectNextTask.TabIndex = 38;
			this.m_lblSelectNextTask.Text = "Select the next task you want to do:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblSelectNextTask, true);
			// 
			// m_btnCastSizePlanning
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnCastSizePlanning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCastSizePlanning.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCastSizePlanning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCastSizePlanning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_btnCastSizePlanning, " I would like Glyssen to help me know how many actors to recruit");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_btnCastSizePlanning, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_btnCastSizePlanning, "MainForm.CastSizePlanning");
			this.m_btnCastSizePlanning.Location = new System.Drawing.Point(23, 151);
			this.m_btnCastSizePlanning.Name = "m_btnCastSizePlanning";
			this.m_btnCastSizePlanning.Size = new System.Drawing.Size(159, 23);
			this.m_btnCastSizePlanning.TabIndex = 4;
			this.m_btnCastSizePlanning.Text = "({0}) Cast Size Planning...";
			this.m_btnCastSizePlanning.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCastSizePlanning, false);
			this.m_btnCastSizePlanning.UseVisualStyleBackColor = false;
			this.m_btnCastSizePlanning.Click += new System.EventHandler(this.m_btnCastSizePlanning_Click);
			// 
			// m_imgCastSizePlanning
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgCastSizePlanning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgCastSizePlanning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_imgCastSizePlanning.Image = global::Glyssen.Properties.Resources.green_check;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_imgCastSizePlanning, "Sufficiently completed to move on to following tasks");
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_imgCastSizePlanning, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_imgCastSizePlanning, "MainForm.SufficientlyCompleted");
			this.m_imgCastSizePlanning.Location = new System.Drawing.Point(188, 151);
			this.m_imgCastSizePlanning.Name = "m_imgCastSizePlanning";
			this.m_imgCastSizePlanning.Size = new System.Drawing.Size(22, 23);
			this.m_imgCastSizePlanning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_imgCastSizePlanning.TabIndex = 40;
			this.m_imgCastSizePlanning.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgCastSizePlanning, false);
			this.m_imgCastSizePlanning.Visible = false;
			// 
			// m_lblFilesAreHere
			// 
			this.m_lblFilesAreHere.AutoSize = true;
			this.m_lblFilesAreHere.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFilesAreHere, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblFilesAreHere, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFilesAreHere.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblFilesAreHere, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblFilesAreHere, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblFilesAreHere, "MainForm.FilesAreHere");
			this.m_lblFilesAreHere.Location = new System.Drawing.Point(3, 0);
			this.m_lblFilesAreHere.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.m_lblFilesAreHere.Name = "m_lblFilesAreHere";
			this.m_lblFilesAreHere.Size = new System.Drawing.Size(73, 13);
			this.m_lblFilesAreHere.TabIndex = 0;
			this.m_lblFilesAreHere.Text = "Files are here:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFilesAreHere, true);
			this.m_lblFilesAreHere.Visible = false;
			// 
			// m_lblCastSizePlan
			// 
			this.m_lblCastSizePlan.AutoSize = true;
			this.m_lblCastSizePlan.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblCastSizePlan, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblCastSizePlan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_lblCastSizePlan, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblCastSizePlan.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this.m_lblCastSizePlan, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this.m_lblCastSizePlan, null);
			this.m_L10NSharpExtender.SetLocalizingId(this.m_lblCastSizePlan, "MainForm.CastSizePlanPlural");
			this.m_lblCastSizePlan.Location = new System.Drawing.Point(216, 148);
			this.m_lblCastSizePlan.Name = "m_lblCastSizePlan";
			this.m_lblCastSizePlan.Size = new System.Drawing.Size(333, 29);
			this.m_lblCastSizePlan.TabIndex = 42;
			this.m_lblCastSizePlan.Text = "Cast size is {0}, including {1} narrators";
			this.m_lblCastSizePlan.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblCastSizePlan, true);
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.AutoSize = true;
			this.m_tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanel.ColumnCount = 3;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.Controls.Add(this.m_lblSelectNextTask, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnOpenProject, 0, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCheckOpen, 1, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblActorsAssigned, 2, 6);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCheckAssignActors, 1, 6);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblProjectInfo, 2, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCheckSettings, 1, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnAssignVoiceActors, 0, 6);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblSettingsInfo, 2, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCheckAssignCharacters, 1, 4);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnSettings, 0, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblBookSelectionInfo, 2, 3);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblPercentAssigned, 2, 4);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCheckBooks, 1, 3);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnSelectBooks, 0, 3);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnIdentify, 0, 4);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnExport, 0, 7);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnCastSizePlanning, 0, 5);
			this.m_tableLayoutPanel.Controls.Add(this.m_imgCastSizePlanning, 1, 5);
			this.m_tableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 2, 8);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblCastSizePlan, 2, 5);
			this.m_tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(0, 32);
			this.m_tableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.Padding = new System.Windows.Forms.Padding(20, 4, 20, 20);
			this.m_tableLayoutPanel.RowCount = 9;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(572, 310);
			this.m_tableLayoutPanel.TabIndex = 40;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanel, false);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.m_lnkExit, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_lastExportLocationLink, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_lblFilesAreHere, 0, 0);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel2, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(213, 235);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(339, 13);
			this.tableLayoutPanel2.TabIndex = 41;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel2, false);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(572, 342);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.Controls.Add(this.m_toolStrip);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.KeyPreview = true;
			this.m_L10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_L10NSharpExtender.SetLocalizationComment(this, null);
			this.m_L10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_L10NSharpExtender.SetLocalizingId(this, "MainForm.WindowTitle");
			this.Location = new System.Drawing.Point(50, 50);
			this.MinimumSize = new System.Drawing.Size(540, 100);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Glyssen";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_L10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckOpen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssignActors)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckSettings)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckAssignCharacters)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCheckBooks)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgCastSizePlanning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_L10NSharpExtender;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripButton m_btnAbout;
		private System.Windows.Forms.ToolStripDropDownButton m_uiLanguageMenu;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private System.Windows.Forms.Label m_lblSelectNextTask;
		private System.Windows.Forms.LinkLabel m_lastExportLocationLink;
		private System.Windows.Forms.LinkLabel m_lnkExit;
		private System.Windows.Forms.Button m_btnOpenProject;
		private System.Windows.Forms.PictureBox m_imgCheckOpen;
		private System.Windows.Forms.Label m_lblActorsAssigned;
		private System.Windows.Forms.PictureBox m_imgCheckAssignActors;
		private System.Windows.Forms.Label m_lblProjectInfo;
		private System.Windows.Forms.PictureBox m_imgCheckSettings;
		private System.Windows.Forms.Button m_btnAssignVoiceActors;
		private System.Windows.Forms.Label m_lblSettingsInfo;
		private System.Windows.Forms.PictureBox m_imgCheckAssignCharacters;
		private System.Windows.Forms.Button m_btnSettings;
		private System.Windows.Forms.Label m_lblBookSelectionInfo;
		private System.Windows.Forms.Label m_lblPercentAssigned;
		private System.Windows.Forms.PictureBox m_imgCheckBooks;
		private System.Windows.Forms.Button m_btnSelectBooks;
		private System.Windows.Forms.Button m_btnIdentify;
		private System.Windows.Forms.Button m_btnExport;
		private System.Windows.Forms.Button m_btnCastSizePlanning;
		private System.Windows.Forms.PictureBox m_imgCastSizePlanning;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label m_lblFilesAreHere;
		private System.Windows.Forms.Label m_lblCastSizePlan;
		private System.Windows.Forms.ToolStripDropDownButton m_shareMenu;
		private System.Windows.Forms.ToolStripMenuItem m_exportMenu;
		private System.Windows.Forms.ToolStripMenuItem m_importMenu;
	}
}




