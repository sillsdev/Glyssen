namespace Glyssen.Dialogs
{
	partial class OpenProjectDlg
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
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.lblExistingProject = new System.Windows.Forms.Label();
			this.m_linkTextReleaseBundle = new System.Windows.Forms.LinkLabel();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_chkShowInactiveProjects = new System.Windows.Forms.CheckBox();
			this.m_listExistingProjects = new Glyssen.Controls.ExistingProjectsList();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.OpenProjectDlg";
			// 
			// lblExistingProject
			// 
			this.lblExistingProject.AutoSize = true;
			this.lblExistingProject.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.lblExistingProject, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.lblExistingProject, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.lblExistingProject.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.lblExistingProject, "DialogBoxes.OpenProjectDlg.SelectProject");
			this.lblExistingProject.Location = new System.Drawing.Point(0, 0);
			this.lblExistingProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.lblExistingProject.Name = "lblExistingProject";
			this.lblExistingProject.Size = new System.Drawing.Size(113, 13);
			this.lblExistingProject.TabIndex = 0;
			this.lblExistingProject.Text = "Select existing project:";
			this.glyssenColorPalette.SetUsePaletteColors(this.lblExistingProject, true);
			// 
			// m_linkTextReleaseBundle
			// 
			this.m_linkTextReleaseBundle.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkTextReleaseBundle.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkTextReleaseBundle.BackColor = System.Drawing.SystemColors.Control;
			this.m_linkTextReleaseBundle.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.glyssenColorPalette.SetForeColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkTextReleaseBundle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetLinkColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkTextReleaseBundle.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkTextReleaseBundle, "DialogBoxes.OpenProjectDlg.CreateNewProject");
			this.m_linkTextReleaseBundle.Location = new System.Drawing.Point(0, 260);
			this.m_linkTextReleaseBundle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkTextReleaseBundle.Name = "m_linkTextReleaseBundle";
			this.m_linkTextReleaseBundle.Size = new System.Drawing.Size(211, 13);
			this.m_linkTextReleaseBundle.TabIndex = 3;
			this.m_linkTextReleaseBundle.TabStop = true;
			this.m_linkTextReleaseBundle.Text = "Create new project from Text Release Bundle";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkTextReleaseBundle, true);
			this.m_linkTextReleaseBundle.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkTextReleaseBundle, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkTextReleaseBundle.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkTextReleaseBundle_LinkClicked);
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
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(340, 308);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
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
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(421, 308);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_chkShowInactiveProjects
			// 
			this.m_chkShowInactiveProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_chkShowInactiveProjects.AutoSize = true;
			this.m_chkShowInactiveProjects.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_chkShowInactiveProjects, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_chkShowInactiveProjects.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_chkShowInactiveProjects, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_chkShowInactiveProjects.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_chkShowInactiveProjects, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkShowInactiveProjects, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkShowInactiveProjects, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkShowInactiveProjects, "DialogBoxes.OpenProjectDlg.ShowInactiveProjects");
			this.m_chkShowInactiveProjects.Location = new System.Drawing.Point(345, 240);
			this.m_chkShowInactiveProjects.Name = "m_chkShowInactiveProjects";
			this.m_chkShowInactiveProjects.Size = new System.Drawing.Size(133, 17);
			this.m_chkShowInactiveProjects.TabIndex = 6;
			this.m_chkShowInactiveProjects.Text = "Show inactive projects";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_chkShowInactiveProjects, true);
			this.m_chkShowInactiveProjects.UseVisualStyleBackColor = true;
			this.m_chkShowInactiveProjects.Visible = false;
			this.m_chkShowInactiveProjects.CheckedChanged += new System.EventHandler(this.HandleShowHiddenProjectsCheckedChanged);
			// 
			// m_listExistingProjects
			// 
			this.m_listExistingProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_listExistingProjects, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_listExistingProjects, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_listExistingProjects.IncludeHiddenProjects = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_listExistingProjects, "DialogBoxes.OpenProjectDlg.ExistingProjectsList");
			this.m_listExistingProjects.Location = new System.Drawing.Point(8, 24);
			this.m_listExistingProjects.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
			this.m_listExistingProjects.Name = "m_listExistingProjects";
			this.m_listExistingProjects.Size = new System.Drawing.Size(465, 206);
			this.m_listExistingProjects.TabIndex = 5;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_listExistingProjects, false);
			this.m_listExistingProjects.SelectedProjectChanged += new System.EventHandler(this.HandleSelectedProjectChanged);
			this.m_listExistingProjects.ListLoaded += new System.EventHandler(this.HandleExistingProjectsListLoaded);
			this.m_listExistingProjects.ColumnWidthChanged += new System.EventHandler<System.Windows.Forms.DataGridViewColumnEventArgs>(this.HandleColumnWidthChanged);
			this.m_listExistingProjects.ColumnDisplayIndexChanged += new System.EventHandler<System.Windows.Forms.DataGridViewColumnEventArgs>(this.m_listExistingProjects_ColumnDisplayIndexChanged);
			this.m_listExistingProjects.ProjectListSorted += new System.EventHandler(this.HandleProjectListSorted);
			this.m_listExistingProjects.DoubleClick += new System.EventHandler(this.HandleExistingProjectsDoubleClick);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMain, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.ColumnCount = 1;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMain.Controls.Add(this.lblExistingProject, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkTextReleaseBundle, 0, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_listExistingProjects, 0, 1);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_chkShowInactiveProjects, 0, 2);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMain, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 4;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(481, 277);
			this.m_tableLayoutPanelMain.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMain, false);
			// 
			// OpenProjectDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(511, 347);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.OpenProjectDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(410, 353);
			this.Name = "OpenProjectDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Open Project";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.OpenProjectDlg_Load);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMain.ResumeLayout(false);
			this.m_tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.Label lblExistingProject;
		private System.Windows.Forms.LinkLabel m_linkTextReleaseBundle;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private Controls.ExistingProjectsList m_listExistingProjects;
		private System.Windows.Forms.CheckBox m_chkShowInactiveProjects;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}
