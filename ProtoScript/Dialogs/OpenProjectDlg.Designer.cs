namespace ProtoScript.Dialogs
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
			System.Windows.Forms.Label lblNewProject;
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.lblExistingProject = new System.Windows.Forms.Label();
			this.m_linkTextReleaseBundle = new System.Windows.Forms.LinkLabel();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_linkParatextProject = new System.Windows.Forms.LinkLabel();
			this.m_chkShowInactiveProjects = new System.Windows.Forms.CheckBox();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.m_listExistingProjects = new ProtoScript.Controls.ExistingProjectsList();
			lblNewProject = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblNewProject
			// 
			lblNewProject.AutoSize = true;
			lblNewProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			lblNewProject.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(lblNewProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(lblNewProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(lblNewProject, "OpenProjectDialog.NewProject");
			lblNewProject.Location = new System.Drawing.Point(0, 205);
			lblNewProject.Margin = new System.Windows.Forms.Padding(0, 20, 0, 10);
			lblNewProject.Name = "lblNewProject";
			lblNewProject.Size = new System.Drawing.Size(229, 18);
			lblNewProject.TabIndex = 2;
			lblNewProject.Text = "Create new project based on:";
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.OpenProjectDialog";
			// 
			// lblExistingProject
			// 
			this.lblExistingProject.AutoSize = true;
			this.lblExistingProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			this.lblExistingProject.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.lblExistingProject, "OpenProjectDialog.SelectProject");
			this.lblExistingProject.Location = new System.Drawing.Point(0, 0);
			this.lblExistingProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.lblExistingProject.Name = "lblExistingProject";
			this.lblExistingProject.Size = new System.Drawing.Size(179, 18);
			this.lblExistingProject.TabIndex = 0;
			this.lblExistingProject.Text = "Select existing project:";
			// 
			// m_linkTextReleaseBundle
			// 
			this.m_linkTextReleaseBundle.AutoSize = true;
			this.m_linkTextReleaseBundle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkTextReleaseBundle.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkTextReleaseBundle, "DialogBoxes.OpenProjectDlg.m_linkTextReleaseBundle");
			this.m_linkTextReleaseBundle.Location = new System.Drawing.Point(0, 233);
			this.m_linkTextReleaseBundle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkTextReleaseBundle.Name = "m_linkTextReleaseBundle";
			this.m_linkTextReleaseBundle.Size = new System.Drawing.Size(143, 18);
			this.m_linkTextReleaseBundle.TabIndex = 3;
			this.m_linkTextReleaseBundle.TabStop = true;
			this.m_linkTextReleaseBundle.Text = "Text Release Bundle";
			this.m_linkTextReleaseBundle.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkTextReleaseBundle_LinkClicked);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.BackColor = System.Drawing.Color.Transparent;
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(340, 308);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = false;
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.BackColor = System.Drawing.Color.Transparent;
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(421, 308);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = false;
			// 
			// m_linkParatextProject
			// 
			this.m_linkParatextProject.AutoSize = true;
			this.m_linkParatextProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkParatextProject.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkParatextProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkParatextProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkParatextProject, "DialogBoxes.OpenProjectDlg.m_linkParatextProject");
			this.m_linkParatextProject.Location = new System.Drawing.Point(0, 255);
			this.m_linkParatextProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkParatextProject.Name = "m_linkParatextProject";
			this.m_linkParatextProject.Size = new System.Drawing.Size(111, 18);
			this.m_linkParatextProject.TabIndex = 4;
			this.m_linkParatextProject.TabStop = true;
			this.m_linkParatextProject.Text = "Paratext project";
			// 
			// m_chkShowInactiveProjects
			// 
			this.m_chkShowInactiveProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_chkShowInactiveProjects.AutoSize = true;
			this.m_chkShowInactiveProjects.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkShowInactiveProjects, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkShowInactiveProjects, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkShowInactiveProjects, "DialogBoxes.OpenProjectDialog.OpenProjectDlg.m_chkShowInactiveProjects");
			this.m_chkShowInactiveProjects.Location = new System.Drawing.Point(345, 165);
			this.m_chkShowInactiveProjects.Name = "m_chkShowInactiveProjects";
			this.m_chkShowInactiveProjects.Size = new System.Drawing.Size(133, 17);
			this.m_chkShowInactiveProjects.TabIndex = 6;
			this.m_chkShowInactiveProjects.Text = "Show inactive projects";
			this.m_chkShowInactiveProjects.UseVisualStyleBackColor = true;
			this.m_chkShowInactiveProjects.Visible = false;
			this.m_chkShowInactiveProjects.CheckedChanged += new System.EventHandler(this.HandleShowHiddenProjectsCheckedChanged);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelMain.ColumnCount = 1;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMain.Controls.Add(this.lblExistingProject, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkParatextProject, 0, 5);
			this.m_tableLayoutPanelMain.Controls.Add(lblNewProject, 0, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkTextReleaseBundle, 0, 4);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_listExistingProjects, 0, 1);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_chkShowInactiveProjects, 0, 2);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 6;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(481, 277);
			this.m_tableLayoutPanelMain.TabIndex = 0;
			// 
			// m_listExistingProjects
			// 
			this.m_listExistingProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_listExistingProjects.IncludeHiddenProjects = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_listExistingProjects, "DialogBoxes.OpenProjectDialog.ExistingProjectsList");
			this.m_listExistingProjects.Location = new System.Drawing.Point(3, 25);
			this.m_listExistingProjects.Name = "m_listExistingProjects";
			this.m_listExistingProjects.Size = new System.Drawing.Size(475, 134);
			this.m_listExistingProjects.TabIndex = 5;
			this.m_listExistingProjects.SelectedProjectChanged += new System.EventHandler(this.HandleSelectedProjectChanged);
			this.m_listExistingProjects.ListLoaded += new System.EventHandler(this.HandleExistingProjectsListLoaded);
			this.m_listExistingProjects.DoubleClick += new System.EventHandler(this.HandleExistingProjectsDoubleClick);
			// 
			// OpenProjectDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(511, 347);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "OpenProjectDialog.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(420, 382);
			this.Name = "OpenProjectDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Open Project";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMain.ResumeLayout(false);
			this.m_tableLayoutPanelMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.LinkLabel m_linkParatextProject;
		private System.Windows.Forms.Label lblExistingProject;
		private System.Windows.Forms.LinkLabel m_linkTextReleaseBundle;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private Controls.ExistingProjectsList m_listExistingProjects;
		private System.Windows.Forms.CheckBox m_chkShowInactiveProjects;
	}
}