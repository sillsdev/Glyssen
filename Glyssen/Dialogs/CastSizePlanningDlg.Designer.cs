using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	partial class CastSizePlanningDlg
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
				LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

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
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_linkAbout = new System.Windows.Forms.LinkLabel();
			this.m_lblStartingOver = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.m_lblProjectSummary = new System.Windows.Forms.Label();
			this.m_lblRecordingTime = new System.Windows.Forms.Label();
			this.m_linkMoreInfo = new System.Windows.Forms.LinkLabel();
			this.m_rbSingleNarrator = new System.Windows.Forms.RadioButton();
			this.m_rbAuthorNarrator = new System.Windows.Forms.RadioButton();
			this.m_rbCustomNarrator = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.m_maleNarrators = new System.Windows.Forms.NumericUpDown();
			this.m_femaleNarrators = new System.Windows.Forms.NumericUpDown();
			this.m_lblWorkDistributed = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.m_linkVoiceActorList = new System.Windows.Forms.LinkLabel();
			this.label6 = new System.Windows.Forms.Label();
			this.m_castSizePlanningOptions = new Glyssen.Controls.CastSizePlanningOptions();
			this.m_lblWhenYouClick = new System.Windows.Forms.Label();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnGenerate = new System.Windows.Forms.Button();
			this.m_lblNarratorWarning = new System.Windows.Forms.Label();
			this.m_imgNarratorWarning = new System.Windows.Forms.PictureBox();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_tableLayoutStartingOver = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.m_flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_layoutNarrators = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_layoutMaleFemale = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.m_flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_tblNarratorWarning = new System.Windows.Forms.TableLayoutPanel();
			this.m_flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_maleNarrators)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_femaleNarrators)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgNarratorWarning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.m_tableLayoutStartingOver.SuspendLayout();
			this.m_tableLayoutPanel.SuspendLayout();
			this.m_flowLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.m_layoutNarrators.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.m_layoutMaleFemale.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.m_flowLayoutPanel3.SuspendLayout();
			this.m_flowLayoutPanel5.SuspendLayout();
			this.m_tblNarratorWarning.SuspendLayout();
			this.m_flowLayoutPanel4.SuspendLayout();
			this.m_flowLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_linkAbout
			// 
			this.m_linkAbout.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkAbout.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkAbout.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkAbout.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_linkAbout.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkAbout.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetLinkColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkAbout.LinkVisited = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkAbout, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkAbout, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkAbout, "DialogBoxes.CastSizePlanningDlg.About");
			this.m_linkAbout.Location = new System.Drawing.Point(457, 6);
			this.m_linkAbout.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_linkAbout.Name = "m_linkAbout";
			this.m_linkAbout.Size = new System.Drawing.Size(98, 13);
			this.m_linkAbout.TabIndex = 6;
			this.m_linkAbout.TabStop = true;
			this.m_linkAbout.Text = "What is this about?";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkAbout, true);
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkAbout, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkAbout.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkAbout_LinkClicked);
			// 
			// m_lblStartingOver
			// 
			this.m_lblStartingOver.AutoSize = true;
			this.m_lblStartingOver.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblStartingOver, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblStartingOver.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_lblStartingOver, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblStartingOver.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblStartingOver, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblStartingOver, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblStartingOver, "DialogBoxes.CastSizePlanningDlg.StartingOver");
			this.m_lblStartingOver.Location = new System.Drawing.Point(28, 6);
			this.m_lblStartingOver.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_lblStartingOver.Name = "m_lblStartingOver";
			this.m_lblStartingOver.Size = new System.Drawing.Size(527, 26);
			this.m_lblStartingOver.TabIndex = 7;
			this.m_lblStartingOver.Text = "Changes to these numbers will adjust the current size of the cast.\r\nThe existing " +
    "list of \"Roles for Voice Actors\" will be replaced by a newly generated list.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblStartingOver, true);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.label1, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.label1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.label1, "DialogBoxes.label1");
			this.label1.Location = new System.Drawing.Point(1, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 0, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(24, 36);
			this.label1.TabIndex = 8;
			this.label1.Text = "!";
			this.glyssenColorPalette.SetUsePaletteColors(this.label1, true);
			// 
			// m_lblProjectSummary
			// 
			this.m_lblProjectSummary.AutoSize = true;
			this.m_lblProjectSummary.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblProjectSummary, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblProjectSummary, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblProjectSummary.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblProjectSummary, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblProjectSummary, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblProjectSummary, "DialogBoxes.CastSizePlanningDlg.ProjectSummary.Plural");
			this.m_lblProjectSummary.Location = new System.Drawing.Point(3, 3);
			this.m_lblProjectSummary.Margin = new System.Windows.Forms.Padding(3);
			this.m_lblProjectSummary.Name = "m_lblProjectSummary";
			this.m_lblProjectSummary.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.m_lblProjectSummary.Size = new System.Drawing.Size(282, 16);
			this.m_lblProjectSummary.TabIndex = 8;
			this.m_lblProjectSummary.Text = "This project has {0} books with {1} distinct character roles.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblProjectSummary, true);
			// 
			// m_lblRecordingTime
			// 
			this.m_lblRecordingTime.AutoSize = true;
			this.m_lblRecordingTime.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblRecordingTime, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblRecordingTime, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblRecordingTime.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblRecordingTime, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblRecordingTime, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblRecordingTime, "DialogBoxes.CastSizePlanningDlg.RecordingTime");
			this.m_lblRecordingTime.Location = new System.Drawing.Point(3, 22);
			this.m_lblRecordingTime.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_lblRecordingTime.Name = "m_lblRecordingTime";
			this.m_lblRecordingTime.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.m_lblRecordingTime.Size = new System.Drawing.Size(188, 16);
			this.m_lblRecordingTime.TabIndex = 9;
			this.m_lblRecordingTime.Text = "Estimated recording time: {0:N2} hours";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblRecordingTime, true);
			// 
			// m_linkMoreInfo
			// 
			this.m_linkMoreInfo.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkMoreInfo.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkMoreInfo.BackColor = System.Drawing.SystemColors.Control;
			this.m_linkMoreInfo.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkMoreInfo.Dock = System.Windows.Forms.DockStyle.Right;
			this.m_linkMoreInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkMoreInfo.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetLinkColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkMoreInfo.LinkVisited = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkMoreInfo, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkMoreInfo, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkMoreInfo, "DialogBoxes.CastSizePlanningDlg.MoreInfo");
			this.m_linkMoreInfo.Location = new System.Drawing.Point(285, 6);
			this.m_linkMoreInfo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_linkMoreInfo.Name = "m_linkMoreInfo";
			this.m_linkMoreInfo.Size = new System.Drawing.Size(50, 13);
			this.m_linkMoreInfo.TabIndex = 21;
			this.m_linkMoreInfo.TabStop = true;
			this.m_linkMoreInfo.Text = "more info";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkMoreInfo, true);
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkMoreInfo, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkMoreInfo.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_linkMoreInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkMoreInfo_LinkClicked);
			// 
			// m_rbSingleNarrator
			// 
			this.m_rbSingleNarrator.AutoSize = true;
			this.m_rbSingleNarrator.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rbSingleNarrator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_rbSingleNarrator.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbSingleNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_rbSingleNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rbSingleNarrator.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbSingleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbSingleNarrator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbSingleNarrator, "DialogBoxes.CastSizePlanningDlg.SingleNarrator");
			this.m_rbSingleNarrator.Location = new System.Drawing.Point(3, 0);
			this.m_rbSingleNarrator.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_rbSingleNarrator.Name = "m_rbSingleNarrator";
			this.m_rbSingleNarrator.Size = new System.Drawing.Size(95, 17);
			this.m_rbSingleNarrator.TabIndex = 0;
			this.m_rbSingleNarrator.Text = "Single Narrator";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbSingleNarrator, true);
			this.m_rbSingleNarrator.UseVisualStyleBackColor = true;
			this.m_rbSingleNarrator.CheckedChanged += new System.EventHandler(this.NarratorOptionChanged);
			// 
			// m_rbAuthorNarrator
			// 
			this.m_rbAuthorNarrator.AutoSize = true;
			this.m_rbAuthorNarrator.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rbAuthorNarrator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_rbAuthorNarrator.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbAuthorNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_rbAuthorNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rbAuthorNarrator.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbAuthorNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbAuthorNarrator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbAuthorNarrator, "DialogBoxes.CastSizePlanningDlg.AuthorNarrator");
			this.m_rbAuthorNarrator.Location = new System.Drawing.Point(3, 20);
			this.m_rbAuthorNarrator.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_rbAuthorNarrator.Name = "m_rbAuthorNarrator";
			this.m_rbAuthorNarrator.Size = new System.Drawing.Size(116, 17);
			this.m_rbAuthorNarrator.TabIndex = 1;
			this.m_rbAuthorNarrator.Text = "Narration by Author";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbAuthorNarrator, true);
			this.m_rbAuthorNarrator.UseVisualStyleBackColor = true;
			this.m_rbAuthorNarrator.CheckedChanged += new System.EventHandler(this.NarratorOptionChanged);
			// 
			// m_rbCustomNarrator
			// 
			this.m_rbCustomNarrator.AutoSize = true;
			this.m_rbCustomNarrator.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_rbCustomNarrator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_rbCustomNarrator.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_rbCustomNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_rbCustomNarrator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_rbCustomNarrator.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_rbCustomNarrator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_rbCustomNarrator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_rbCustomNarrator, "DialogBoxes.CastSizePlanningDlg.CustomNarrators");
			this.m_rbCustomNarrator.Location = new System.Drawing.Point(3, 40);
			this.m_rbCustomNarrator.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_rbCustomNarrator.Name = "m_rbCustomNarrator";
			this.m_rbCustomNarrator.Size = new System.Drawing.Size(60, 17);
			this.m_rbCustomNarrator.TabIndex = 2;
			this.m_rbCustomNarrator.Text = "Custom";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_rbCustomNarrator, true);
			this.m_rbCustomNarrator.UseVisualStyleBackColor = true;
			this.m_rbCustomNarrator.CheckedChanged += new System.EventHandler(this.NarratorOptionChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.label2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.CastSizePlanningDlg.Male");
			this.label2.Location = new System.Drawing.Point(3, 6);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(74, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Male";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.label2, true);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label3, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label3.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.label3, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label3, "DialogBoxes.CastSizePlanningDlg.Female");
			this.label3.Location = new System.Drawing.Point(83, 6);
			this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(74, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Female";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.label3, true);
			// 
			// m_maleNarrators
			// 
			this.glyssenColorPalette.SetBackColor(this.m_maleNarrators, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_maleNarrators, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_maleNarrators, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_maleNarrators, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_maleNarrators, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_maleNarrators, "DialogBoxes.numericUpDown1");
			this.m_maleNarrators.Location = new System.Drawing.Point(12, 25);
			this.m_maleNarrators.Margin = new System.Windows.Forms.Padding(12, 3, 12, 3);
			this.m_maleNarrators.Name = "m_maleNarrators";
			this.m_maleNarrators.Size = new System.Drawing.Size(56, 20);
			this.m_maleNarrators.TabIndex = 2;
			this.m_maleNarrators.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_maleNarrators, false);
			this.m_maleNarrators.ValueChanged += new System.EventHandler(this.NarratorsValueChanged);
			// 
			// m_femaleNarrators
			// 
			this.glyssenColorPalette.SetBackColor(this.m_femaleNarrators, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_femaleNarrators, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_femaleNarrators, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_femaleNarrators, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_femaleNarrators, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_femaleNarrators, "DialogBoxes.numericUpDown1");
			this.m_femaleNarrators.Location = new System.Drawing.Point(92, 25);
			this.m_femaleNarrators.Margin = new System.Windows.Forms.Padding(12, 3, 12, 3);
			this.m_femaleNarrators.Name = "m_femaleNarrators";
			this.m_femaleNarrators.Size = new System.Drawing.Size(56, 20);
			this.m_femaleNarrators.TabIndex = 3;
			this.m_femaleNarrators.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_femaleNarrators, false);
			this.m_femaleNarrators.ValueChanged += new System.EventHandler(this.NarratorsValueChanged);
			// 
			// m_lblWorkDistributed
			// 
			this.m_lblWorkDistributed.AutoSize = true;
			this.m_lblWorkDistributed.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblWorkDistributed, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblWorkDistributed, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblWorkDistributed.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWorkDistributed, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWorkDistributed, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWorkDistributed, "DialogBoxes.CastSizePlanningDlg.WorkDistributed");
			this.m_lblWorkDistributed.Location = new System.Drawing.Point(3, 6);
			this.m_lblWorkDistributed.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_lblWorkDistributed.Name = "m_lblWorkDistributed";
			this.m_lblWorkDistributed.Size = new System.Drawing.Size(276, 13);
			this.m_lblWorkDistributed.TabIndex = 20;
			this.m_lblWorkDistributed.Text = "How should narration work be distributed within the cast?";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblWorkDistributed, true);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label4, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label4, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label4.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label4, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label4, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label4, "DialogBoxes.CastSizePlanningDlg.SizeOfCast");
			this.label4.Location = new System.Drawing.Point(6, 229);
			this.label4.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(250, 13);
			this.label4.TabIndex = 23;
			this.label4.Text = "What size of voice actor cast should be planed for?";
			this.glyssenColorPalette.SetUsePaletteColors(this.label4, true);
			// 
			// label5
			// 
			this.label5.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label5, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label5.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.label5, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label5.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label5, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label5, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label5, "DialogBoxes.CastSizePlanningDlg.NoteTop");
			this.label5.Location = new System.Drawing.Point(3, 6);
			this.label5.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(128, 39);
			this.label5.TabIndex = 21;
			this.label5.Text = "Note- If you have already recruited actors, you may enter them in";
			this.glyssenColorPalette.SetUsePaletteColors(this.label5, true);
			// 
			// m_linkVoiceActorList
			// 
			this.m_linkVoiceActorList.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.glyssenColorPalette.SetBackColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkVoiceActorList.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkVoiceActorList.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_linkVoiceActorList.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_linkVoiceActorList.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkVoiceActorList.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkVoiceActorList.LinkVisited = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkVoiceActorList, "DialogBoxes.CastSizePlanningDlg.VoiceActorList");
			this.m_linkVoiceActorList.Location = new System.Drawing.Point(3, 48);
			this.m_linkVoiceActorList.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_linkVoiceActorList.Name = "m_linkVoiceActorList";
			this.m_linkVoiceActorList.Size = new System.Drawing.Size(128, 13);
			this.m_linkVoiceActorList.TabIndex = 22;
			this.m_linkVoiceActorList.TabStop = true;
			this.m_linkVoiceActorList.Text = "Voice Actor List";
			this.m_linkVoiceActorList.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkVoiceActorList, true);
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkVoiceActorList.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_linkVoiceActorList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkVoiceActorList_LinkClicked);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label6, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label6.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.label6, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label6.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label6, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label6, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label6, "DialogBoxes.CastSizePlanningDlg.NoteBottom");
			this.label6.Location = new System.Drawing.Point(3, 64);
			this.label6.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(128, 26);
			this.label6.TabIndex = 23;
			this.label6.Text = "and choose \'Match Voice Actor List\' at left.";
			this.glyssenColorPalette.SetUsePaletteColors(this.label6, true);
			// 
			// m_castSizePlanningOptions
			// 
			this.m_castSizePlanningOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_castSizePlanningOptions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_castSizePlanningOptions.BackColor = System.Drawing.SystemColors.Control;
			this.m_castSizePlanningOptions.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_castSizePlanningOptions, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_castSizePlanningOptions.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_castSizePlanningOptions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_castSizePlanningOptions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_castSizePlanningOptions, "DialogBoxes.CastSizePlanningOptions");
			this.m_castSizePlanningOptions.Location = new System.Drawing.Point(0, 0);
			this.m_castSizePlanningOptions.Margin = new System.Windows.Forms.Padding(0);
			this.m_castSizePlanningOptions.Name = "m_castSizePlanningOptions";
			this.m_castSizePlanningOptions.SelectedCastSizeRow = Glyssen.Dialogs.CastSizeRow.Recommended;
			this.m_castSizePlanningOptions.Size = new System.Drawing.Size(418, 151);
			this.m_castSizePlanningOptions.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_castSizePlanningOptions, true);
			this.m_castSizePlanningOptions.CastSizeOptionChanged += new System.EventHandler<Glyssen.Dialogs.CastSizeOptionChangedEventArgs>(this.m_castSizePlanningOptions_CastSizeOptionChanged);
			this.m_castSizePlanningOptions.CastSizeCustomValueChanged += new System.EventHandler<Glyssen.Dialogs.CastSizeValueChangedEventArgs>(this.m_castSizePlanningOptions_CastSizeCustomValueChanged);
			// 
			// m_lblWhenYouClick
			// 
			this.m_lblWhenYouClick.AutoSize = true;
			this.m_lblWhenYouClick.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblWhenYouClick, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblWhenYouClick.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_lblWhenYouClick, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblWhenYouClick.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWhenYouClick, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWhenYouClick, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWhenYouClick, "DialogBoxes.CastSizePlanningDlg.WhenYouClick");
			this.m_lblWhenYouClick.Location = new System.Drawing.Point(3, 6);
			this.m_lblWhenYouClick.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_lblWhenYouClick.Name = "m_lblWhenYouClick";
			this.m_lblWhenYouClick.Size = new System.Drawing.Size(519, 26);
			this.m_lblWhenYouClick.TabIndex = 25;
			this.m_lblWhenYouClick.Text = "When you click \'Generate Groups\', Glyssen will form groups of character roles for" +
    " the number of voice actors selected above.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblWhenYouClick, true);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(480, 3);
			this.m_btnCancel.MinimumSize = new System.Drawing.Size(75, 23);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnGenerate
			// 
			this.m_btnGenerate.AutoSize = true;
			this.m_btnGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_btnGenerate, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnGenerate.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnGenerate, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnGenerate, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnGenerate, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnGenerate, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnGenerate, "DialogBoxes.CastSizePlanningDlg.GenerateGroups");
			this.m_btnGenerate.Location = new System.Drawing.Point(364, 3);
			this.m_btnGenerate.MinimumSize = new System.Drawing.Size(75, 23);
			this.m_btnGenerate.Name = "m_btnGenerate";
			this.m_btnGenerate.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.m_btnGenerate.Size = new System.Drawing.Size(110, 23);
			this.m_btnGenerate.TabIndex = 3;
			this.m_btnGenerate.Text = "Generate Groups";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnGenerate, false);
			this.m_btnGenerate.UseVisualStyleBackColor = true;
			this.m_btnGenerate.Click += new System.EventHandler(this.m_btnGenerate_Click);
			// 
			// m_lblNarratorWarning
			// 
			this.m_lblNarratorWarning.AutoSize = true;
			this.m_lblNarratorWarning.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblNarratorWarning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblNarratorWarning.Dock = System.Windows.Forms.DockStyle.Right;
			this.glyssenColorPalette.SetForeColor(this.m_lblNarratorWarning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblNarratorWarning.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblNarratorWarning, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblNarratorWarning, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblNarratorWarning, "DialogBoxes.CastSizePlanningDlg.NarratorWarning");
			this.m_lblNarratorWarning.Location = new System.Drawing.Point(30, 6);
			this.m_lblNarratorWarning.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
			this.m_lblNarratorWarning.Name = "m_lblNarratorWarning";
			this.m_lblNarratorWarning.Size = new System.Drawing.Size(270, 13);
			this.m_lblNarratorWarning.TabIndex = 0;
			this.m_lblNarratorWarning.Text = "Cast size numbers must be larger than narrator numbers.";
			this.m_lblNarratorWarning.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblNarratorWarning, true);
			// 
			// m_imgNarratorWarning
			// 
			this.glyssenColorPalette.SetBackColor(this.m_imgNarratorWarning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_imgNarratorWarning, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_imgNarratorWarning, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_imgNarratorWarning, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_imgNarratorWarning, "DialogBoxes.pictureBox1");
			this.m_imgNarratorWarning.Location = new System.Drawing.Point(0, 0);
			this.m_imgNarratorWarning.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.m_imgNarratorWarning.Name = "m_imgNarratorWarning";
			this.m_imgNarratorWarning.Size = new System.Drawing.Size(24, 24);
			this.m_imgNarratorWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.m_imgNarratorWarning.TabIndex = 1;
			this.m_imgNarratorWarning.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_imgNarratorWarning, false);
			// 
			// m_tableLayoutStartingOver
			// 
			this.m_tableLayoutStartingOver.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutStartingOver, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutStartingOver.ColumnCount = 2;
			this.m_tableLayoutStartingOver.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutStartingOver.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutStartingOver.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutStartingOver.Controls.Add(this.m_lblStartingOver, 1, 0);
			this.m_tableLayoutStartingOver.Controls.Add(this.label1, 0, 0);
			this.m_tableLayoutStartingOver.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutStartingOver, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutStartingOver.Location = new System.Drawing.Point(3, 78);
			this.m_tableLayoutStartingOver.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_tableLayoutStartingOver.Name = "m_tableLayoutStartingOver";
			this.m_tableLayoutStartingOver.RowCount = 1;
			this.m_tableLayoutStartingOver.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutStartingOver.Size = new System.Drawing.Size(558, 39);
			this.m_tableLayoutStartingOver.TabIndex = 18;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutStartingOver, false);
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.AutoSize = true;
			this.m_tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanel.ColumnCount = 1;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.Controls.Add(this.m_flowLayoutPanel1, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_tableLayoutStartingOver, 0, 2);
			this.m_tableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 0, 3);
			this.m_tableLayoutPanel.Controls.Add(this.m_layoutNarrators, 0, 4);
			this.m_tableLayoutPanel.Controls.Add(this.label4, 0, 5);
			this.m_tableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 0, 6);
			this.m_tableLayoutPanel.Controls.Add(this.m_flowLayoutPanel5, 0, 8);
			this.m_tableLayoutPanel.Controls.Add(this.m_flowLayoutPanel4, 0, 7);
			this.m_tableLayoutPanel.Controls.Add(this.m_flowLayoutPanel2, 0, 1);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(10, 10);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 9;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(564, 482);
			this.m_tableLayoutPanel.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanel, false);
			this.m_tableLayoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.m_tableLayoutPanel_Paint);
			// 
			// m_flowLayoutPanel1
			// 
			this.m_flowLayoutPanel1.AutoSize = true;
			this.m_flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.m_flowLayoutPanel1.Controls.Add(this.m_lblProjectSummary);
			this.m_flowLayoutPanel1.Controls.Add(this.m_lblRecordingTime);
			this.m_flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.m_flowLayoutPanel1.Name = "m_flowLayoutPanel1";
			this.m_flowLayoutPanel1.Size = new System.Drawing.Size(288, 41);
			this.m_flowLayoutPanel1.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanel1, true);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.m_linkMoreInfo, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblWorkDistributed, 0, 0);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 123);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(338, 25);
			this.tableLayoutPanel1.TabIndex = 21;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// m_layoutNarrators
			// 
			this.m_layoutNarrators.AutoSize = true;
			this.m_layoutNarrators.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_layoutNarrators, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_layoutNarrators.ColumnCount = 2;
			this.m_layoutNarrators.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_layoutNarrators.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_layoutNarrators.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.m_layoutNarrators.Controls.Add(this.m_layoutMaleFemale, 1, 0);
			this.glyssenColorPalette.SetForeColor(this.m_layoutNarrators, Glyssen.Utilities.GlyssenColors.Default);
			this.m_layoutNarrators.Location = new System.Drawing.Point(26, 151);
			this.m_layoutNarrators.Margin = new System.Windows.Forms.Padding(26, 3, 3, 6);
			this.m_layoutNarrators.Name = "m_layoutNarrators";
			this.m_layoutNarrators.RowCount = 1;
			this.m_layoutNarrators.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_layoutNarrators.Size = new System.Drawing.Size(311, 66);
			this.m_layoutNarrators.TabIndex = 22;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_layoutNarrators, false);
			this.m_layoutNarrators.Paint += new System.Windows.Forms.PaintEventHandler(this.m_layoutNarrators_Paint);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.flowLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.flowLayoutPanel1.Controls.Add(this.m_rbSingleNarrator);
			this.flowLayoutPanel1.Controls.Add(this.m_rbAuthorNarrator);
			this.flowLayoutPanel1.Controls.Add(this.m_rbCustomNarrator);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.glyssenColorPalette.SetForeColor(this.flowLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(122, 60);
			this.flowLayoutPanel1.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.flowLayoutPanel1, false);
			// 
			// m_layoutMaleFemale
			// 
			this.m_layoutMaleFemale.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_layoutMaleFemale, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_layoutMaleFemale.ColumnCount = 2;
			this.m_layoutMaleFemale.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_layoutMaleFemale.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_layoutMaleFemale.Controls.Add(this.m_femaleNarrators, 1, 1);
			this.m_layoutMaleFemale.Controls.Add(this.label3, 1, 0);
			this.m_layoutMaleFemale.Controls.Add(this.label2, 0, 0);
			this.m_layoutMaleFemale.Controls.Add(this.m_maleNarrators, 0, 1);
			this.glyssenColorPalette.SetForeColor(this.m_layoutMaleFemale, Glyssen.Utilities.GlyssenColors.Default);
			this.m_layoutMaleFemale.Location = new System.Drawing.Point(148, 3);
			this.m_layoutMaleFemale.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.m_layoutMaleFemale.Name = "m_layoutMaleFemale";
			this.m_layoutMaleFemale.RowCount = 2;
			this.m_layoutMaleFemale.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_layoutMaleFemale.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_layoutMaleFemale.Size = new System.Drawing.Size(160, 48);
			this.m_layoutMaleFemale.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_layoutMaleFemale, false);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel2.Controls.Add(this.m_flowLayoutPanel3, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.m_castSizePlanningOptions, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel2, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 248);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(558, 152);
			this.tableLayoutPanel2.TabIndex = 24;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel2, false);
			// 
			// m_flowLayoutPanel3
			// 
			this.m_flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanel3, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanel3.Controls.Add(this.label5);
			this.m_flowLayoutPanel3.Controls.Add(this.m_linkVoiceActorList);
			this.m_flowLayoutPanel3.Controls.Add(this.label6);
			this.m_flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanel3, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanel3.Location = new System.Drawing.Point(421, 3);
			this.m_flowLayoutPanel3.Name = "m_flowLayoutPanel3";
			this.m_flowLayoutPanel3.Size = new System.Drawing.Size(134, 146);
			this.m_flowLayoutPanel3.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanel3, false);
			// 
			// m_flowLayoutPanel5
			// 
			this.m_flowLayoutPanel5.AutoSize = true;
			this.m_flowLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanel5, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanel5.Controls.Add(this.m_btnCancel);
			this.m_flowLayoutPanel5.Controls.Add(this.m_btnGenerate);
			this.m_flowLayoutPanel5.Controls.Add(this.m_tblNarratorWarning);
			this.m_flowLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.m_flowLayoutPanel5.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanel5, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanel5.Location = new System.Drawing.Point(3, 450);
			this.m_flowLayoutPanel5.Name = "m_flowLayoutPanel5";
			this.m_flowLayoutPanel5.Size = new System.Drawing.Size(558, 29);
			this.m_flowLayoutPanel5.TabIndex = 26;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanel5, false);
			this.m_flowLayoutPanel5.WrapContents = false;
			// 
			// m_tblNarratorWarning
			// 
			this.m_tblNarratorWarning.AutoSize = true;
			this.m_tblNarratorWarning.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tblNarratorWarning, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tblNarratorWarning.ColumnCount = 2;
			this.m_tblNarratorWarning.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tblNarratorWarning.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tblNarratorWarning.Controls.Add(this.m_lblNarratorWarning, 1, 0);
			this.m_tblNarratorWarning.Controls.Add(this.m_imgNarratorWarning, 0, 0);
			this.glyssenColorPalette.SetForeColor(this.m_tblNarratorWarning, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tblNarratorWarning.Location = new System.Drawing.Point(58, 2);
			this.m_tblNarratorWarning.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.m_tblNarratorWarning.Name = "m_tblNarratorWarning";
			this.m_tblNarratorWarning.RowCount = 1;
			this.m_tblNarratorWarning.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tblNarratorWarning.Size = new System.Drawing.Size(303, 25);
			this.m_tblNarratorWarning.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tblNarratorWarning, false);
			this.m_tblNarratorWarning.Visible = false;
			// 
			// m_flowLayoutPanel4
			// 
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanel4, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanel4.Controls.Add(this.m_lblWhenYouClick);
			this.m_flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanel4, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanel4.Location = new System.Drawing.Point(3, 406);
			this.m_flowLayoutPanel4.Name = "m_flowLayoutPanel4";
			this.m_flowLayoutPanel4.Size = new System.Drawing.Size(558, 38);
			this.m_flowLayoutPanel4.TabIndex = 27;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanel4, false);
			// 
			// m_flowLayoutPanel2
			// 
			this.m_flowLayoutPanel2.AutoSize = true;
			this.m_flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanel2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanel2.Controls.Add(this.m_linkAbout);
			this.m_flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanel2, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanel2.Location = new System.Drawing.Point(3, 50);
			this.m_flowLayoutPanel2.Name = "m_flowLayoutPanel2";
			this.m_flowLayoutPanel2.Size = new System.Drawing.Size(558, 25);
			this.m_flowLayoutPanel2.TabIndex = 28;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanel2, false);
			// 
			// CastSizePlanningDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(584, 607);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.CastSizePlanningDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "CastSizePlanningDlg";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.Text = "Cast Size Planning";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.CastSizePlanningDlg_Load);
			this.Shown += new System.EventHandler(this.CastSizePlanningDlg_Shown);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_maleNarrators)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_femaleNarrators)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_imgNarratorWarning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.m_tableLayoutStartingOver.ResumeLayout(false);
			this.m_tableLayoutStartingOver.PerformLayout();
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.m_flowLayoutPanel1.ResumeLayout(false);
			this.m_flowLayoutPanel1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.m_layoutNarrators.ResumeLayout(false);
			this.m_layoutNarrators.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.m_layoutMaleFemale.ResumeLayout(false);
			this.m_layoutMaleFemale.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.m_flowLayoutPanel3.ResumeLayout(false);
			this.m_flowLayoutPanel3.PerformLayout();
			this.m_flowLayoutPanel5.ResumeLayout(false);
			this.m_flowLayoutPanel5.PerformLayout();
			this.m_tblNarratorWarning.ResumeLayout(false);
			this.m_tblNarratorWarning.PerformLayout();
			this.m_flowLayoutPanel4.ResumeLayout(false);
			this.m_flowLayoutPanel4.PerformLayout();
			this.m_flowLayoutPanel2.ResumeLayout(false);
			this.m_flowLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutStartingOver;
		private System.Windows.Forms.LinkLabel m_linkAbout;
		private System.Windows.Forms.Label m_lblStartingOver;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanel1;
		private System.Windows.Forms.Label m_lblProjectSummary;
		private System.Windows.Forms.Label m_lblRecordingTime;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.LinkLabel m_linkMoreInfo;
		private System.Windows.Forms.TableLayoutPanel m_layoutNarrators;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton m_rbSingleNarrator;
		private System.Windows.Forms.RadioButton m_rbAuthorNarrator;
		private System.Windows.Forms.RadioButton m_rbCustomNarrator;
		private System.Windows.Forms.TableLayoutPanel m_layoutMaleFemale;
		private System.Windows.Forms.NumericUpDown m_femaleNarrators;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown m_maleNarrators;
		private System.Windows.Forms.Label m_lblWorkDistributed;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanel3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.LinkLabel m_linkVoiceActorList;
		private System.Windows.Forms.Label label6;
		private Controls.CastSizePlanningOptions m_castSizePlanningOptions;
		private System.Windows.Forms.Label m_lblWhenYouClick;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanel5;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnGenerate;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanel4;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel m_tblNarratorWarning;
		private System.Windows.Forms.Label m_lblNarratorWarning;
		private System.Windows.Forms.PictureBox m_imgNarratorWarning;
	}
}