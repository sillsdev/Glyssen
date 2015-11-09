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
			this.m_linkConfigureOptions = new System.Windows.Forms.LinkLabel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_panelContainingActions = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.m_panelContainingActions.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.m_lblInstructions.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblInstructions, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.VoiceActorInformation.EnterVoiceActors");
			this.m_lblInstructions.Location = new System.Drawing.Point(32, 21);
			this.m_lblInstructions.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(249, 32);
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
			this.m_btnNext.Location = new System.Drawing.Point(1344, 7);
			this.m_btnNext.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(200, 55);
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
			this.m_dataGrid.Location = new System.Drawing.Point(0, 0);
			this.m_dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.ReadOnly = false;
			this.m_dataGrid.Size = new System.Drawing.Size(1552, 775);
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
			this.m_saveStatus.Location = new System.Drawing.Point(1346, 21);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(257, 32);
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
			this.m_linkClose.Location = new System.Drawing.Point(1456, 91);
			this.m_linkClose.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(88, 32);
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
			this.m_btnOk.Location = new System.Drawing.Point(1344, 60);
			this.m_btnOk.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(200, 55);
			this.m_btnOk.TabIndex = 2;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_linkConfigureOptions
			// 
			this.m_linkConfigureOptions.AutoSize = true;
			this.m_linkConfigureOptions.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkConfigureOptions, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkConfigureOptions, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkConfigureOptions, "DialogBoxes.VoiceActorInformation.linkLabel1");
			this.m_linkConfigureOptions.Location = new System.Drawing.Point(649, 21);
			this.m_linkConfigureOptions.Name = "m_linkConfigureOptions";
			this.m_linkConfigureOptions.Size = new System.Drawing.Size(269, 32);
			this.m_linkConfigureOptions.TabIndex = 7;
			this.m_linkConfigureOptions.TabStop = true;
			this.m_linkConfigureOptions.Text = "Configure Options...";
			this.m_linkConfigureOptions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkConfigureOptions_LinkClicked);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_dataGrid, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_panelContainingActions, 0, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(40, 60);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1552, 897);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// m_panelContainingActions
			// 
			this.m_panelContainingActions.Controls.Add(this.m_linkClose);
			this.m_panelContainingActions.Controls.Add(this.m_btnNext);
			this.m_panelContainingActions.Controls.Add(this.m_btnOk);
			this.m_panelContainingActions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panelContainingActions.Location = new System.Drawing.Point(0, 775);
			this.m_panelContainingActions.Margin = new System.Windows.Forms.Padding(0);
			this.m_panelContainingActions.Name = "m_panelContainingActions";
			this.m_panelContainingActions.Size = new System.Drawing.Size(1552, 122);
			this.m_panelContainingActions.TabIndex = 4;
			// 
			// VoiceActorInformationDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(1624, 985);
			this.Controls.Add(this.m_linkConfigureOptions);
			this.Controls.Add(this.m_saveStatus);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_lblInstructions);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorInformation.WindowTitle");
			this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(1147, 594);
			this.Name = "VoiceActorInformationDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor Information";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.m_panelContainingActions.ResumeLayout(false);
			this.m_panelContainingActions.PerformLayout();
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
		private System.Windows.Forms.LinkLabel m_linkConfigureOptions;
	}
}