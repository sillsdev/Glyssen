namespace ProtoScript.Dialogs
{
	partial class AssignCharacterDialog
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
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_btnPrevious = new System.Windows.Forms.Button();
			this.m_btnAssign = new System.Windows.Forms.Button();
			this.m_labelReference = new System.Windows.Forms.Label();
			this.m_linkLabelChapter = new System.Windows.Forms.LinkLabel();
			this.m_linkLabelBook = new System.Windows.Forms.LinkLabel();
			this.m_linkLabelAll = new System.Windows.Forms.LinkLabel();
			this.m_labelWhoSpeaks = new System.Windows.Forms.Label();
			this.m_labelXofY = new System.Windows.Forms.Label();
			this.m_listBoxCharacters = new System.Windows.Forms.ListBox();
			this.m_listBoxDeliveries = new System.Windows.Forms.ListBox();
			this.m_progressBar = new ProtoScript.Controls.ProgressBarUnanimated();
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "AssignCharacterDialog";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnNext, "Common.Skip");
			this.m_btnNext.Location = new System.Drawing.Point(543, 503);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 0;
			this.m_btnNext.Text = "Skip";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnPrevious
			// 
			this.m_btnPrevious.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnPrevious, "Common.Previous");
			this.m_btnPrevious.Location = new System.Drawing.Point(402, 503);
			this.m_btnPrevious.Name = "m_btnPrevious";
			this.m_btnPrevious.Size = new System.Drawing.Size(75, 23);
			this.m_btnPrevious.TabIndex = 1;
			this.m_btnPrevious.Text = "Previous";
			this.m_btnPrevious.UseVisualStyleBackColor = true;
			this.m_btnPrevious.Click += new System.EventHandler(this.m_btnPrevious_Click);
			// 
			// m_btnAssign
			// 
			this.m_btnAssign.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAssign, "AssignCharacterDialog.AssignCharacter");
			this.m_btnAssign.Location = new System.Drawing.Point(523, 417);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(95, 23);
			this.m_btnAssign.TabIndex = 3;
			this.m_btnAssign.Text = "Assign Character";
			this.m_btnAssign.UseVisualStyleBackColor = true;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_labelReference
			// 
			this.m_labelReference.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_labelReference, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelReference, "AssignCharacterDialog.label1");
			this.m_labelReference.Location = new System.Drawing.Point(344, 36);
			this.m_labelReference.Name = "m_labelReference";
			this.m_labelReference.Size = new System.Drawing.Size(57, 13);
			this.m_labelReference.TabIndex = 5;
			this.m_labelReference.Text = "Reference";
			// 
			// m_linkLabelChapter
			// 
			this.m_linkLabelChapter.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkLabelChapter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkLabelChapter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkLabelChapter, "ProjectSettingsDialog.ShowChapter");
			this.m_linkLabelChapter.Location = new System.Drawing.Point(426, 227);
			this.m_linkLabelChapter.Name = "m_linkLabelChapter";
			this.m_linkLabelChapter.Size = new System.Drawing.Size(150, 13);
			this.m_linkLabelChapter.TabIndex = 6;
			this.m_linkLabelChapter.TabStop = true;
			this.m_linkLabelChapter.Text = "Show all characters in chapter";
			this.m_linkLabelChapter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkLabelChapter_LinkClicked);
			// 
			// m_linkLabelBook
			// 
			this.m_linkLabelBook.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkLabelBook, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkLabelBook, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkLabelBook, "ProjectSettingsDialog.ShowBook");
			this.m_linkLabelBook.Location = new System.Drawing.Point(426, 244);
			this.m_linkLabelBook.Name = "m_linkLabelBook";
			this.m_linkLabelBook.Size = new System.Drawing.Size(138, 13);
			this.m_linkLabelBook.TabIndex = 7;
			this.m_linkLabelBook.TabStop = true;
			this.m_linkLabelBook.Text = "Show all characters in book";
			this.m_linkLabelBook.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkLabelBook_LinkClicked);
			// 
			// m_linkLabelAll
			// 
			this.m_linkLabelAll.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkLabelAll, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkLabelAll, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkLabelAll, "ProjectSettingsDialog.ShowBook");
			this.m_linkLabelAll.Location = new System.Drawing.Point(426, 261);
			this.m_linkLabelAll.Name = "m_linkLabelAll";
			this.m_linkLabelAll.Size = new System.Drawing.Size(100, 13);
			this.m_linkLabelAll.TabIndex = 9;
			this.m_linkLabelAll.TabStop = true;
			this.m_linkLabelAll.Text = "Show all characters";
			this.m_linkLabelAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkLabelAll_LinkClicked);
			// 
			// m_labelWhoSpeaks
			// 
			this.m_labelWhoSpeaks.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelWhoSpeaks, "AssignCharacterDialog.WhoSpeaks");
			this.m_labelWhoSpeaks.Location = new System.Drawing.Point(13, 36);
			this.m_labelWhoSpeaks.Name = "m_labelWhoSpeaks";
			this.m_labelWhoSpeaks.Size = new System.Drawing.Size(113, 13);
			this.m_labelWhoSpeaks.TabIndex = 10;
			this.m_labelWhoSpeaks.Text = "Who speaks this part?";
			// 
			// m_labelXofY
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelXofY, "AssignCharacterDialog.XofY");
			this.m_labelXofY.Location = new System.Drawing.Point(478, 505);
			this.m_labelXofY.Name = "m_labelXofY";
			this.m_labelXofY.Size = new System.Drawing.Size(64, 18);
			this.m_labelXofY.TabIndex = 11;
			this.m_labelXofY.Text = "{0} of {1}";
			this.m_labelXofY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_listBoxCharacters
			// 
			this.m_listBoxCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_listBoxCharacters.FormattingEnabled = true;
			this.m_listBoxCharacters.Location = new System.Drawing.Point(413, 60);
			this.m_listBoxCharacters.Name = "m_listBoxCharacters";
			this.m_listBoxCharacters.Size = new System.Drawing.Size(205, 156);
			this.m_listBoxCharacters.TabIndex = 4;
			this.m_listBoxCharacters.SelectedIndexChanged += new System.EventHandler(this.m_listBoxCharacters_SelectedIndexChanged);
			// 
			// m_listBoxDeliveries
			// 
			this.m_listBoxDeliveries.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_listBoxDeliveries.FormattingEnabled = true;
			this.m_listBoxDeliveries.Location = new System.Drawing.Point(413, 293);
			this.m_listBoxDeliveries.Name = "m_listBoxDeliveries";
			this.m_listBoxDeliveries.Size = new System.Drawing.Size(205, 104);
			this.m_listBoxDeliveries.TabIndex = 8;
			this.m_listBoxDeliveries.SelectedIndexChanged += new System.EventHandler(this.m_listBoxDeliveries_SelectedIndexChanged);
			// 
			// m_progressBar
			// 
			this.m_progressBar.Location = new System.Drawing.Point(12, 534);
			this.m_progressBar.Name = "m_progressBar";
			this.m_progressBar.Size = new System.Drawing.Size(606, 17);
			this.m_progressBar.TabIndex = 12;
			// 
			// m_blocksDisplayBrowser
			// 
			this.m_blocksDisplayBrowser.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_blocksDisplayBrowser, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksDisplayBrowser, "ProjectSettingsDialog.Browser");
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(12, 60);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(383, 466);
			this.m_blocksDisplayBrowser.TabIndex = 2;
			// 
			// AssignCharacterDialog
			// 
			this.AcceptButton = this.m_btnAssign;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(630, 561);
			this.Controls.Add(this.m_progressBar);
			this.Controls.Add(this.m_labelXofY);
			this.Controls.Add(this.m_labelWhoSpeaks);
			this.Controls.Add(this.m_linkLabelAll);
			this.Controls.Add(this.m_listBoxDeliveries);
			this.Controls.Add(this.m_linkLabelBook);
			this.Controls.Add(this.m_linkLabelChapter);
			this.Controls.Add(this.m_labelReference);
			this.Controls.Add(this.m_listBoxCharacters);
			this.Controls.Add(this.m_btnAssign);
			this.Controls.Add(this.m_blocksDisplayBrowser);
			this.Controls.Add(this.m_btnPrevious);
			this.Controls.Add(this.m_btnNext);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "AssignCharacterDialog.AssignCharacter");
			this.Name = "AssignCharacterDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assign Character";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Button m_btnPrevious;
		private Controls.Browser m_blocksDisplayBrowser;
		private System.Windows.Forms.Button m_btnAssign;
		private System.Windows.Forms.ListBox m_listBoxCharacters;
		private System.Windows.Forms.Label m_labelReference;
		private System.Windows.Forms.LinkLabel m_linkLabelChapter;
		private System.Windows.Forms.LinkLabel m_linkLabelBook;
		private System.Windows.Forms.ListBox m_listBoxDeliveries;
		private System.Windows.Forms.LinkLabel m_linkLabelAll;
		private System.Windows.Forms.Label m_labelWhoSpeaks;
		private System.Windows.Forms.Label m_labelXofY;
		private ProtoScript.Controls.ProgressBarUnanimated m_progressBar;
	}
}