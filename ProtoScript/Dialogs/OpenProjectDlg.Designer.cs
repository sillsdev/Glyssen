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
			this.m_linkSFFolder = new System.Windows.Forms.LinkLabel();
			this.m_linkSingleSFBook = new System.Windows.Forms.LinkLabel();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelExistingProject = new System.Windows.Forms.TableLayoutPanel();
			this.m_listExistingProjects = new System.Windows.Forms.ListBox();
			lblNewProject = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			this.m_tableLayoutPanelExistingProject.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblNewProject
			// 
			lblNewProject.AutoSize = true;
			lblNewProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			lblNewProject.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(lblNewProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(lblNewProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(lblNewProject, "OpenProjectDialog.label1");
			lblNewProject.Location = new System.Drawing.Point(0, 158);
			lblNewProject.Margin = new System.Windows.Forms.Padding(0, 20, 0, 10);
			lblNewProject.Name = "lblNewProject";
			lblNewProject.Size = new System.Drawing.Size(179, 18);
			lblNewProject.TabIndex = 2;
			lblNewProject.Text = "New Project Based On";
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "OpenProjectDialog";
			// 
			// lblExistingProject
			// 
			this.lblExistingProject.AutoSize = true;
			this.lblExistingProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			this.lblExistingProject.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.lblExistingProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.lblExistingProject, "OpenProjectDialog.label1");
			this.lblExistingProject.Location = new System.Drawing.Point(0, 0);
			this.lblExistingProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.lblExistingProject.Name = "lblExistingProject";
			this.lblExistingProject.Size = new System.Drawing.Size(126, 18);
			this.lblExistingProject.TabIndex = 0;
			this.lblExistingProject.Text = "Existing Project";
			// 
			// m_linkTextReleaseBundle
			// 
			this.m_linkTextReleaseBundle.AutoSize = true;
			this.m_linkTextReleaseBundle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkTextReleaseBundle.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkTextReleaseBundle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkTextReleaseBundle, "OpenProjectDialog.OpenProjectDlg.m_linkTextReleaseBundle");
			this.m_linkTextReleaseBundle.Location = new System.Drawing.Point(0, 186);
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
			this.m_btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(233, 305);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(314, 305);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_linkParatextProject
			// 
			this.m_linkParatextProject.AutoSize = true;
			this.m_linkParatextProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkParatextProject.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkParatextProject, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkParatextProject, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkParatextProject, "OpenProjectDialog.OpenProjectDlg.m_linkParatextProject");
			this.m_linkParatextProject.Location = new System.Drawing.Point(0, 208);
			this.m_linkParatextProject.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkParatextProject.Name = "m_linkParatextProject";
			this.m_linkParatextProject.Size = new System.Drawing.Size(111, 18);
			this.m_linkParatextProject.TabIndex = 4;
			this.m_linkParatextProject.TabStop = true;
			this.m_linkParatextProject.Text = "Paratext project";
			// 
			// m_linkSFFolder
			// 
			this.m_linkSFFolder.AutoSize = true;
			this.m_linkSFFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkSFFolder.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkSFFolder, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkSFFolder, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_linkSFFolder, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkSFFolder, "OpenProjectDialog.OpenProjectDlg.m_linkSFFolder");
			this.m_linkSFFolder.Location = new System.Drawing.Point(0, 230);
			this.m_linkSFFolder.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkSFFolder.Name = "m_linkSFFolder";
			this.m_linkSFFolder.Size = new System.Drawing.Size(212, 18);
			this.m_linkSFFolder.TabIndex = 5;
			this.m_linkSFFolder.TabStop = true;
			this.m_linkSFFolder.Text = "Folder of Standard Format files";
			// 
			// m_linkSingleSFBook
			// 
			this.m_linkSingleSFBook.AutoSize = true;
			this.m_linkSingleSFBook.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_linkSingleSFBook.LinkColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkSingleSFBook, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkSingleSFBook, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_linkSingleSFBook, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkSingleSFBook, "OpenProjectDialog.OpenProjectDlg.m_linkSingleSFBook");
			this.m_linkSingleSFBook.Location = new System.Drawing.Point(0, 252);
			this.m_linkSingleSFBook.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_linkSingleSFBook.Name = "m_linkSingleSFBook";
			this.m_linkSingleSFBook.Size = new System.Drawing.Size(260, 18);
			this.m_linkSingleSFBook.TabIndex = 6;
			this.m_linkSingleSFBook.TabStop = true;
			this.m_linkSingleSFBook.Text = "Single Book from Standard Format file";
			this.m_linkSingleSFBook.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkSingleSFBook_LinkClicked);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.ColumnCount = 1;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkSingleSFBook, 0, 5);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkSFFolder, 0, 4);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkParatextProject, 0, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_tableLayoutPanelExistingProject, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(lblNewProject, 0, 1);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_linkTextReleaseBundle, 0, 2);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 6;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(374, 274);
			this.m_tableLayoutPanelMain.TabIndex = 0;
			// 
			// m_tableLayoutPanelExistingProject
			// 
			this.m_tableLayoutPanelExistingProject.ColumnCount = 1;
			this.m_tableLayoutPanelExistingProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelExistingProject.Controls.Add(this.lblExistingProject, 0, 0);
			this.m_tableLayoutPanelExistingProject.Controls.Add(this.m_listExistingProjects, 0, 1);
			this.m_tableLayoutPanelExistingProject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableLayoutPanelExistingProject.Location = new System.Drawing.Point(3, 3);
			this.m_tableLayoutPanelExistingProject.Name = "m_tableLayoutPanelExistingProject";
			this.m_tableLayoutPanelExistingProject.RowCount = 2;
			this.m_tableLayoutPanelExistingProject.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelExistingProject.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelExistingProject.Size = new System.Drawing.Size(368, 132);
			this.m_tableLayoutPanelExistingProject.TabIndex = 1;
			// 
			// m_listExistingProjects
			// 
			this.m_listExistingProjects.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_listExistingProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_listExistingProjects.FormattingEnabled = true;
			this.m_listExistingProjects.ItemHeight = 18;
			this.m_listExistingProjects.Location = new System.Drawing.Point(0, 25);
			this.m_listExistingProjects.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.m_listExistingProjects.Name = "m_listExistingProjects";
			this.m_listExistingProjects.Size = new System.Drawing.Size(368, 104);
			this.m_listExistingProjects.TabIndex = 1;
			this.m_listExistingProjects.SelectedIndexChanged += new System.EventHandler(this.m_listExistingProjects_SelectedIndexChanged);
			this.m_listExistingProjects.DoubleClick += new System.EventHandler(this.m_listExistingProjects_DoubleClick);
			// 
			// OpenProjectDlg
			// 
			this.AcceptButton = this.m_btnCancel;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(404, 344);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "OpenProjectDialog.OpenProject");
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
			this.m_tableLayoutPanelExistingProject.ResumeLayout(false);
			this.m_tableLayoutPanelExistingProject.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.LinkLabel m_linkSFFolder;
		private System.Windows.Forms.LinkLabel m_linkParatextProject;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelExistingProject;
		private System.Windows.Forms.Label lblExistingProject;
		private System.Windows.Forms.ListBox m_listExistingProjects;
		private System.Windows.Forms.LinkLabel m_linkTextReleaseBundle;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.LinkLabel m_linkSingleSFBook;
	}
}