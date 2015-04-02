namespace ProtoScript.Dialogs
{
	partial class SelectExistingProjectDlg
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
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_listExistingProjects = new ProtoScript.Controls.ExistingProjectsList();
			this.m_linkCreateNewProject = new System.Windows.Forms.LinkLabel();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.SelectExistingProjectDialog";
			// 
			// lblExistingProject
			// 
			this.lblExistingProject.AutoSize = true;
			this.lblExistingProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblExistingProject.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.lblExistingProject, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.lblExistingProject, "DialogBoxes.SelectExistingProjectDialog.SelectExistingProjectDlg.lblExistingProje" +
        "ct");
			this.lblExistingProject.Location = new System.Drawing.Point(0, 0);
			this.lblExistingProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.lblExistingProject.Name = "lblExistingProject";
			this.lblExistingProject.Size = new System.Drawing.Size(462, 36);
			this.lblExistingProject.TabIndex = 0;
			this.lblExistingProject.Text = "The selected bundle is already associated with the following projects. To open an" +
    " existing project, select it and click OK.";
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
			this.m_btnOk.Location = new System.Drawing.Point(412, 220);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = false;
			// 
			// m_listExistingProjects
			// 
			this.m_listExistingProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_listExistingProjects.IncludeHiddenProjects = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_listExistingProjects, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_listExistingProjects, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_listExistingProjects, "DialogBoxes.SelectExistingProjectDialog.SelectExistingProjectDlg.ExistingProjects" +
        "List");
			this.m_listExistingProjects.Location = new System.Drawing.Point(3, 43);
			this.m_listExistingProjects.Name = "m_listExistingProjects";
			this.m_listExistingProjects.Size = new System.Drawing.Size(466, 122);
			this.m_listExistingProjects.TabIndex = 5;
			this.m_listExistingProjects.SelectedProjectChanged += new System.EventHandler(this.HandleSelectedProjectChanged);
			this.m_listExistingProjects.DoubleClick += new System.EventHandler(this.m_listExistingProjects_DoubleClick);
			// 
			// m_linkCreateNewProject
			// 
			this.m_linkCreateNewProject.AutoSize = true;
			this.m_linkCreateNewProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkCreateNewProject.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkCreateNewProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkCreateNewProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkCreateNewProject, "DialogBoxes.SelectExistingProjectDialog.linkLabel1");
			this.m_linkCreateNewProject.Location = new System.Drawing.Point(3, 168);
			this.m_linkCreateNewProject.Name = "m_linkCreateNewProject";
			this.m_linkCreateNewProject.Size = new System.Drawing.Size(306, 18);
			this.m_linkCreateNewProject.TabIndex = 6;
			this.m_linkCreateNewProject.TabStop = true;
			this.m_linkCreateNewProject.Text = "Create a new recording project for this bundle";
			this.m_linkCreateNewProject.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkCreateNewProject_LinkClicked);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelMain.ColumnCount = 1;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.Controls.Add(this.lblExistingProject, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_listExistingProjects, 0, 1);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkCreateNewProject, 0, 2);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 3;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(472, 188);
			this.m_tableLayoutPanelMain.TabIndex = 0;
			// 
			// SelectExistingProjectDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(502, 258);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this, "SelectExistingProjectDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(518, 296);
			this.Name = "SelectExistingProjectDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Existing Project";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMain.ResumeLayout(false);
			this.m_tableLayoutPanelMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.Label lblExistingProject;
		private System.Windows.Forms.Button m_btnOk;
		private Controls.ExistingProjectsList m_listExistingProjects;
		private System.Windows.Forms.LinkLabel m_linkCreateNewProject;
	}
}