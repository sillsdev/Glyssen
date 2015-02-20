namespace ProtoScript.Dialogs
{
	partial class ProjectMetadataDlg
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblLanguageName = new System.Windows.Forms.Label();
			this.m_lblIso639_2_Code = new System.Windows.Forms.Label();
			this.m_lblProjectName = new System.Windows.Forms.Label();
			this.m_lblProjectId = new System.Windows.Forms.Label();
			this.m_txtLanguageName = new System.Windows.Forms.TextBox();
			this.m_txtIso639_2_Code = new System.Windows.Forms.TextBox();
			this.m_txtProjectName = new System.Windows.Forms.TextBox();
			this.m_txtProjectId = new System.Windows.Forms.TextBox();
			this.m_chkOverride = new System.Windows.Forms.CheckBox();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_wsFontControl = new SIL.Windows.Forms.WritingSystems.WSFontControl();
			this.m_tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutProjectId = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutMain.SuspendLayout();
			this.m_tableLayoutProjectId.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.ProjectMetadataDlg";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(437, 342);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_lblLanguageName
			// 
			this.m_lblLanguageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLanguageName.AutoSize = true;
			this.m_lblLanguageName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
			this.m_lblLanguageName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblLanguageName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLanguageName, "DialogBoxes.ProjectMetadataDlg.m_lblLanguageName");
			this.m_lblLanguageName.Location = new System.Drawing.Point(3, 4);
			this.m_lblLanguageName.Name = "m_lblLanguageName";
			this.m_lblLanguageName.Size = new System.Drawing.Size(120, 18);
			this.m_lblLanguageName.TabIndex = 0;
			this.m_lblLanguageName.Text = "Language Name:";
			// 
			// m_lblIso639_2_Code
			// 
			this.m_lblIso639_2_Code.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblIso639_2_Code.AutoSize = true;
			this.m_lblIso639_2_Code.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
			this.m_lblIso639_2_Code.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblIso639_2_Code, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblIso639_2_Code, "DialogBoxes.ProjectMetadataDlg.m_lblIso639_2_Code");
			this.m_lblIso639_2_Code.Location = new System.Drawing.Point(3, 30);
			this.m_lblIso639_2_Code.Name = "m_lblIso639_2_Code";
			this.m_lblIso639_2_Code.Size = new System.Drawing.Size(207, 18);
			this.m_lblIso639_2_Code.TabIndex = 2;
			this.m_lblIso639_2_Code.Text = "Ethnologue (ISO 639-2) Code:";
			// 
			// m_lblProjectName
			// 
			this.m_lblProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblProjectName.AutoSize = true;
			this.m_lblProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
			this.m_lblProjectName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblProjectName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblProjectName, "DialogBoxes.ProjectMetadataDlg.m_lblProjectName");
			this.m_lblProjectName.Location = new System.Drawing.Point(3, 66);
			this.m_lblProjectName.Name = "m_lblProjectName";
			this.m_lblProjectName.Size = new System.Drawing.Size(103, 18);
			this.m_lblProjectName.TabIndex = 4;
			this.m_lblProjectName.Text = "Project Name:";
			// 
			// m_lblProjectId
			// 
			this.m_lblProjectId.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblProjectId.AutoSize = true;
			this.m_lblProjectId.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
			this.m_lblProjectId.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblProjectId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblProjectId, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblProjectId, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblProjectId, "DialogBoxes.ProjectMetadataDlg.m_lblProjectId");
			this.m_lblProjectId.Location = new System.Drawing.Point(3, 93);
			this.m_lblProjectId.Name = "m_lblProjectId";
			this.m_lblProjectId.Size = new System.Drawing.Size(74, 18);
			this.m_lblProjectId.TabIndex = 6;
			this.m_lblProjectId.Text = "Project Id:";
			// 
			// m_txtLanguageName
			// 
			this.m_txtLanguageName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtLanguageName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtLanguageName, "DialogBoxes.ProjectMetadataDlg.m_txtLanguageName");
			this.m_txtLanguageName.Location = new System.Drawing.Point(216, 3);
			this.m_txtLanguageName.Name = "m_txtLanguageName";
			this.m_txtLanguageName.Size = new System.Drawing.Size(278, 20);
			this.m_txtLanguageName.TabIndex = 1;
			// 
			// m_txtIso639_2_Code
			// 
			this.m_txtIso639_2_Code.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtIso639_2_Code, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtIso639_2_Code, "DialogBoxes.ProjectMetadataDlg.m_txtIso639_2_Code");
			this.m_txtIso639_2_Code.Location = new System.Drawing.Point(216, 29);
			this.m_txtIso639_2_Code.MaxLength = 3;
			this.m_txtIso639_2_Code.Name = "m_txtIso639_2_Code";
			this.m_txtIso639_2_Code.Size = new System.Drawing.Size(278, 20);
			this.m_txtIso639_2_Code.TabIndex = 3;
			this.m_txtIso639_2_Code.WordWrap = false;
			this.m_txtIso639_2_Code.TextChanged += new System.EventHandler(this.UpdateProjectId);
			// 
			// m_txtProjectName
			// 
			this.m_txtProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtProjectName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtProjectName, "DialogBoxes.ProjectMetadataDlg.m_txtProjectName");
			this.m_txtProjectName.Location = new System.Drawing.Point(216, 65);
			this.m_txtProjectName.Name = "m_txtProjectName";
			this.m_txtProjectName.Size = new System.Drawing.Size(278, 20);
			this.m_txtProjectName.TabIndex = 5;
			this.m_txtProjectName.TextChanged += new System.EventHandler(this.UpdateProjectId);
			// 
			// m_txtProjectId
			// 
			this.m_txtProjectId.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_txtProjectId.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtProjectId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtProjectId, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtProjectId, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtProjectId, "DialogBoxes.ProjectMetadataDlg.m_txtProjectId");
			this.m_txtProjectId.Location = new System.Drawing.Point(3, 3);
			this.m_txtProjectId.Name = "m_txtProjectId";
			this.m_txtProjectId.Size = new System.Drawing.Size(189, 20);
			this.m_txtProjectId.TabIndex = 0;
			// 
			// m_chkOverride
			// 
			this.m_chkOverride.AutoSize = true;
			this.m_chkOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
			this.m_chkOverride.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkOverride, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkOverride, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkOverride, "DialogBoxes.ProjectMetadataDlg.checkBox1");
			this.m_chkOverride.Location = new System.Drawing.Point(198, 3);
			this.m_chkOverride.Name = "m_chkOverride";
			this.m_chkOverride.Size = new System.Drawing.Size(83, 22);
			this.m_chkOverride.TabIndex = 1;
			this.m_chkOverride.Text = "Override";
			this.m_chkOverride.UseVisualStyleBackColor = true;
			this.m_chkOverride.CheckedChanged += new System.EventHandler(this.m_chkOverride_CheckedChanged);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(356, 342);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.HandleOkButtonClick);
			// 
			// m_wsFontControl
			// 
			this.m_tableLayoutMain.SetColumnSpan(this.m_wsFontControl, 2);
			this.m_wsFontControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_wsFontControl.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_wsFontControl, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_wsFontControl, "DialogBoxes.ProjectMetadataDlg.WSFontControl");
			this.m_wsFontControl.Location = new System.Drawing.Point(4, 130);
			this.m_wsFontControl.Margin = new System.Windows.Forms.Padding(4);
			this.m_wsFontControl.Name = "m_wsFontControl";
			this.m_wsFontControl.Size = new System.Drawing.Size(489, 177);
			this.m_wsFontControl.TabIndex = 7;
			// 
			// m_tableLayoutMain
			// 
			this.m_tableLayoutMain.ColumnCount = 2;
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutMain.Controls.Add(this.m_tableLayoutProjectId, 1, 4);
			this.m_tableLayoutMain.Controls.Add(this.m_txtProjectName, 1, 3);
			this.m_tableLayoutMain.Controls.Add(this.m_txtIso639_2_Code, 1, 1);
			this.m_tableLayoutMain.Controls.Add(this.m_lblLanguageName, 0, 0);
			this.m_tableLayoutMain.Controls.Add(this.m_lblProjectId, 0, 4);
			this.m_tableLayoutMain.Controls.Add(this.m_txtLanguageName, 1, 0);
			this.m_tableLayoutMain.Controls.Add(this.m_lblIso639_2_Code, 0, 1);
			this.m_tableLayoutMain.Controls.Add(this.m_lblProjectName, 0, 3);
			this.m_tableLayoutMain.Controls.Add(this.m_wsFontControl, 0, 6);
			this.m_tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableLayoutMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutMain.Name = "m_tableLayoutMain";
			this.m_tableLayoutMain.RowCount = 7;
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutMain.Size = new System.Drawing.Size(497, 311);
			this.m_tableLayoutMain.TabIndex = 0;
			// 
			// m_tableLayoutProjectId
			// 
			this.m_tableLayoutProjectId.AutoSize = true;
			this.m_tableLayoutProjectId.ColumnCount = 2;
			this.m_tableLayoutProjectId.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutProjectId.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutProjectId.Controls.Add(this.m_txtProjectId, 0, 0);
			this.m_tableLayoutProjectId.Controls.Add(this.m_chkOverride, 1, 0);
			this.m_tableLayoutProjectId.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableLayoutProjectId.Location = new System.Drawing.Point(213, 88);
			this.m_tableLayoutProjectId.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutProjectId.Name = "m_tableLayoutProjectId";
			this.m_tableLayoutProjectId.RowCount = 1;
			this.m_tableLayoutProjectId.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutProjectId.Size = new System.Drawing.Size(284, 28);
			this.m_tableLayoutProjectId.TabIndex = 11;
			// 
			// ProjectMetadataDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(527, 381);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutMain);
			this.Controls.Add(this.m_btnCancel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ProjectMetadataDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(543, 410);
			this.Name = "ProjectMetadataDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Project Metadata";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutMain.ResumeLayout(false);
			this.m_tableLayoutMain.PerformLayout();
			this.m_tableLayoutProjectId.ResumeLayout(false);
			this.m_tableLayoutProjectId.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblLanguageName;
		private System.Windows.Forms.Label m_lblIso639_2_Code;
		private System.Windows.Forms.Label m_lblProjectName;
		private System.Windows.Forms.Label m_lblProjectId;
		private System.Windows.Forms.TextBox m_txtLanguageName;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutMain;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutProjectId;
		private System.Windows.Forms.TextBox m_txtProjectId;
		private System.Windows.Forms.CheckBox m_chkOverride;
		private System.Windows.Forms.TextBox m_txtProjectName;
		private System.Windows.Forms.TextBox m_txtIso639_2_Code;
		private SIL.Windows.Forms.WritingSystems.WSFontControl m_wsFontControl;
		private System.Windows.Forms.Button m_btnOk;
	}
}