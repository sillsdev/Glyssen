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
			this.label1 = new System.Windows.Forms.Label();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_dataGrid = new Glyssen.Controls.VoiceActorInformationGrid();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_linkClose = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "DialogBoxes.VoiceActorInformation.EnterVoiceActors");
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter Voice Actors";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnNext.Enabled = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnNext, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnNext, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(413, 281);
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
			this.m_dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_dataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_dataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_dataGrid, "DialogBoxes.VoiceActorInformation.voiceActorInformationGrid21");
			this.m_dataGrid.Location = new System.Drawing.Point(12, 29);
			this.m_dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.ReadOnly = false;
			this.m_dataGrid.Size = new System.Drawing.Size(476, 249);
			this.m_dataGrid.TabIndex = 3;
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorInformation.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(12, 312);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(224, 15);
			this.m_saveStatus.TabIndex = 4;
			// 
			// m_linkClose
			// 
			this.m_linkClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkClose.AutoSize = true;
			this.m_linkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkClose, "DialogBoxes.VoiceActorInformation.linkLabel1");
			this.m_linkClose.Location = new System.Drawing.Point(455, 312);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(33, 13);
			this.m_linkClose.TabIndex = 5;
			this.m_linkClose.TabStop = true;
			this.m_linkClose.Text = "Close";
			this.m_linkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkClose_LinkClicked);
			// 
			// VoiceActorInformationDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(500, 334);
			this.Controls.Add(this.m_linkClose);
			this.Controls.Add(this.m_saveStatus);
			this.Controls.Add(this.m_dataGrid);
			this.Controls.Add(this.m_btnNext);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorInformation.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "VoiceActorInformationDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor Information";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button m_btnNext;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private Controls.VoiceActorInformationGrid m_dataGrid;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.LinkLabel m_linkClose;
	}
}