namespace Glyssen.Dialogs
{
	partial class VoiceActorInformationDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorInformationDlg));
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_dataGrid = new Glyssen.Controls.VoiceActorInformationGrid();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_linkClose = new System.Windows.Forms.LinkLabel();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_panelContainingActions = new System.Windows.Forms.Panel();
			this.m_tblLayoutForProjectInfo = new System.Windows.Forms.TableLayoutPanel();
			this.m_lblNarratorData = new System.Windows.Forms.Label();
			this.m_lblMaleCharacterData = new System.Windows.Forms.Label();
			this.m_lblFemaleCharacterData = new System.Windows.Forms.Label();
			this.m_lblChildCharacterData = new System.Windows.Forms.Label();
			this.m_linkNarrationOptions = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.m_panelContainingActions.SuspendLayout();
			this.m_tblLayoutForProjectInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.m_lblInstructions.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.VoiceActorInformation.EnterVoiceActors");
			this.m_lblInstructions.Location = new System.Drawing.Point(12, 9);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(95, 13);
			this.m_lblInstructions.TabIndex = 0;
			this.m_lblInstructions.Text = "Enter Voice Actors";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnNext.Enabled = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnNext, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnNext, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(504, 3);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 2;
			this.m_btnNext.Text = "Next";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorInformation";
			// 
			// m_dataGrid
			// 
			this.m_dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_dataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_dataGrid, "DialogBoxes.VoiceActorInformation.voiceActorInformationGrid21");
			this.m_dataGrid.Location = new System.Drawing.Point(0, 178);
			this.m_dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.ReadOnly = false;
			this.m_dataGrid.Size = new System.Drawing.Size(582, 325);
			this.m_dataGrid.TabIndex = 3;
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorInformation.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(485, 160);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 4;
			this.m_saveStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// m_linkClose
			// 
			this.m_linkClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkClose.AutoSize = true;
			this.m_linkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkClose, "DialogBoxes.VoiceActorInformation.Close");
			this.m_linkClose.Location = new System.Drawing.Point(546, 37);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(33, 13);
			this.m_linkClose.TabIndex = 5;
			this.m_linkClose.TabStop = true;
			this.m_linkClose.Text = "Close";
			this.m_linkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkClose_LinkClicked);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnOk, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(504, 24);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 2;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_saveStatus, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_dataGrid, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_panelContainingActions, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_tblLayoutForProjectInfo, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 25);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(582, 553);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// m_panelContainingActions
			// 
			this.m_panelContainingActions.Controls.Add(this.m_linkClose);
			this.m_panelContainingActions.Controls.Add(this.m_btnNext);
			this.m_panelContainingActions.Controls.Add(this.m_btnOk);
			this.m_panelContainingActions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panelContainingActions.Location = new System.Drawing.Point(0, 503);
			this.m_panelContainingActions.Margin = new System.Windows.Forms.Padding(0);
			this.m_panelContainingActions.Name = "m_panelContainingActions";
			this.m_panelContainingActions.Size = new System.Drawing.Size(582, 50);
			this.m_panelContainingActions.TabIndex = 4;
			// 
			// m_tblLayoutForProjectInfo
			// 
			this.m_tblLayoutForProjectInfo.ColumnCount = 1;
			this.m_tblLayoutForProjectInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tblLayoutForProjectInfo.Controls.Add(this.m_lblNarratorData, 0, 0);
			this.m_tblLayoutForProjectInfo.Controls.Add(this.m_lblMaleCharacterData, 0, 2);
			this.m_tblLayoutForProjectInfo.Controls.Add(this.m_lblFemaleCharacterData, 0, 3);
			this.m_tblLayoutForProjectInfo.Controls.Add(this.m_lblChildCharacterData, 0, 4);
			this.m_tblLayoutForProjectInfo.Controls.Add(this.m_linkNarrationOptions, 0, 1);
			this.m_tblLayoutForProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tblLayoutForProjectInfo.Location = new System.Drawing.Point(3, 3);
			this.m_tblLayoutForProjectInfo.Name = "m_tblLayoutForProjectInfo";
			this.m_tblLayoutForProjectInfo.RowCount = 5;
			this.m_tblLayoutForProjectInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.m_tblLayoutForProjectInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tblLayoutForProjectInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.m_tblLayoutForProjectInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.m_tblLayoutForProjectInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tblLayoutForProjectInfo.Size = new System.Drawing.Size(576, 154);
			this.m_tblLayoutForProjectInfo.TabIndex = 5;
			// 
			// m_lblNarratorData
			// 
			this.m_lblNarratorData.AutoSize = true;
			this.m_lblNarratorData.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblNarratorData, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblNarratorData, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblNarratorData, "DialogBoxes.VoiceActorInformation.label1");
			this.m_lblNarratorData.Location = new System.Drawing.Point(3, 0);
			this.m_lblNarratorData.Name = "m_lblNarratorData";
			this.m_lblNarratorData.Size = new System.Drawing.Size(56, 13);
			this.m_lblNarratorData.TabIndex = 0;
			this.m_lblNarratorData.Text = "Narrators: ";
			// 
			// m_lblMaleCharacterData
			// 
			this.m_lblMaleCharacterData.AutoSize = true;
			this.m_lblMaleCharacterData.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblMaleCharacterData, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblMaleCharacterData, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblMaleCharacterData, "DialogBoxes.VoiceActorInformation.label2");
			this.m_lblMaleCharacterData.Location = new System.Drawing.Point(3, 40);
			this.m_lblMaleCharacterData.Name = "m_lblMaleCharacterData";
			this.m_lblMaleCharacterData.Size = new System.Drawing.Size(90, 13);
			this.m_lblMaleCharacterData.TabIndex = 1;
			this.m_lblMaleCharacterData.Text = "Male Characters: ";
			// 
			// m_lblFemaleCharacterData
			// 
			this.m_lblFemaleCharacterData.AutoSize = true;
			this.m_lblFemaleCharacterData.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblFemaleCharacterData, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblFemaleCharacterData, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblFemaleCharacterData, "DialogBoxes.VoiceActorInformation.label3");
			this.m_lblFemaleCharacterData.Location = new System.Drawing.Point(3, 60);
			this.m_lblFemaleCharacterData.Name = "m_lblFemaleCharacterData";
			this.m_lblFemaleCharacterData.Size = new System.Drawing.Size(101, 13);
			this.m_lblFemaleCharacterData.TabIndex = 2;
			this.m_lblFemaleCharacterData.Text = "Female Characters: ";
			// 
			// m_lblChildCharacterData
			// 
			this.m_lblChildCharacterData.AutoSize = true;
			this.m_lblChildCharacterData.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblChildCharacterData, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblChildCharacterData, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblChildCharacterData, "DialogBoxes.VoiceActorInformation.label4");
			this.m_lblChildCharacterData.Location = new System.Drawing.Point(3, 80);
			this.m_lblChildCharacterData.Name = "m_lblChildCharacterData";
			this.m_lblChildCharacterData.Size = new System.Drawing.Size(90, 13);
			this.m_lblChildCharacterData.TabIndex = 3;
			this.m_lblChildCharacterData.Text = "Child Characters: ";
			// 
			// m_linkNarrationOptions
			// 
			this.m_linkNarrationOptions.AutoSize = true;
			this.m_linkNarrationOptions.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkNarrationOptions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkNarrationOptions, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkNarrationOptions, "DialogBoxes.VoiceActorInformation.linkLabel1");
			this.m_linkNarrationOptions.Location = new System.Drawing.Point(3, 20);
			this.m_linkNarrationOptions.Name = "m_linkNarrationOptions";
			this.m_linkNarrationOptions.Size = new System.Drawing.Size(112, 13);
			this.m_linkNarrationOptions.TabIndex = 4;
			this.m_linkNarrationOptions.TabStop = true;
			this.m_linkNarrationOptions.Text = "Set Narrator Options...";
			this.m_linkNarrationOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkNarrationOptions_LinkClicked);
			// 
			// VoiceActorInformationDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(609, 590);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_lblInstructions);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorInformation.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(450, 300);
			this.Name = "VoiceActorInformationDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor Information";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.m_panelContainingActions.ResumeLayout(false);
			this.m_panelContainingActions.PerformLayout();
			this.m_tblLayoutForProjectInfo.ResumeLayout(false);
			this.m_tblLayoutForProjectInfo.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblInstructions;
		private System.Windows.Forms.Button m_btnNext;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private Controls.VoiceActorInformationGrid m_dataGrid;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.LinkLabel m_linkClose;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel m_panelContainingActions;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.TableLayoutPanel m_tblLayoutForProjectInfo;
		private System.Windows.Forms.Label m_lblNarratorData;
		private System.Windows.Forms.Label m_lblMaleCharacterData;
		private System.Windows.Forms.Label m_lblFemaleCharacterData;
		private System.Windows.Forms.Label m_lblChildCharacterData;
		private System.Windows.Forms.LinkLabel m_linkNarrationOptions;
	}
}