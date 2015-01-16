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
			this.m_labelWhoSpeaks = new System.Windows.Forms.Label();
			this.m_labelXofY = new System.Windows.Forms.Label();
			this.m_llMoreChar = new System.Windows.Forms.LinkLabel();
			this.m_txtCharacterFilter = new System.Windows.Forms.TextBox();
			this.m_lblCharacter = new System.Windows.Forms.Label();
			this.m_lblDelivery = new System.Windows.Forms.Label();
			this.m_llMoreDel = new System.Windows.Forms.LinkLabel();
			this.m_txtDeliveryFilter = new System.Windows.Forms.TextBox();
			this.m_icnCharacterFilter = new System.Windows.Forms.PictureBox();
			this.m_icnDeliveryFilter = new System.Windows.Forms.PictureBox();
			this.m_btnAddCharacter = new System.Windows.Forms.Button();
			this.m_btnAddDelivery = new System.Windows.Forms.Button();
			this.m_listBoxCharacters = new System.Windows.Forms.ListBox();
			this.m_listBoxDeliveries = new System.Windows.Forms.ListBox();
			this.m_pnlCharacterFilter = new System.Windows.Forms.Panel();
			this.m_pnlDeliveryFilter = new System.Windows.Forms.Panel();
			this.m_progressBar = new ProtoScript.Controls.ProgressBarUnanimated();
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).BeginInit();
			this.m_pnlCharacterFilter.SuspendLayout();
			this.m_pnlDeliveryFilter.SuspendLayout();
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(543, 495);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 4;
			this.m_btnNext.Text = "Next";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnPrevious
			// 
			this.m_btnPrevious.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnPrevious, "Common.Previous");
			this.m_btnPrevious.Location = new System.Drawing.Point(402, 495);
			this.m_btnPrevious.Name = "m_btnPrevious";
			this.m_btnPrevious.Size = new System.Drawing.Size(75, 23);
			this.m_btnPrevious.TabIndex = 3;
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
			this.m_btnAssign.Location = new System.Drawing.Point(523, 455);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(95, 23);
			this.m_btnAssign.TabIndex = 2;
			this.m_btnAssign.Text = "Assign Character";
			this.m_btnAssign.UseVisualStyleBackColor = true;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_labelReference
			// 
			this.m_labelReference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_labelReference.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_labelReference, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelReference, "AssignCharacterDialog.label1");
			this.m_labelReference.Location = new System.Drawing.Point(251, 19);
			this.m_labelReference.Name = "m_labelReference";
			this.m_labelReference.Size = new System.Drawing.Size(144, 13);
			this.m_labelReference.TabIndex = 5;
			this.m_labelReference.Text = "Reference";
			this.m_labelReference.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// m_labelWhoSpeaks
			// 
			this.m_labelWhoSpeaks.AutoSize = true;
			this.m_labelWhoSpeaks.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelWhoSpeaks, "AssignCharacterDialog.WhoSpeaks");
			this.m_labelWhoSpeaks.Location = new System.Drawing.Point(13, 19);
			this.m_labelWhoSpeaks.Name = "m_labelWhoSpeaks";
			this.m_labelWhoSpeaks.Size = new System.Drawing.Size(113, 13);
			this.m_labelWhoSpeaks.TabIndex = 10;
			this.m_labelWhoSpeaks.Text = "Who speaks this part?";
			// 
			// m_labelXofY
			// 
			this.m_labelXofY.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelXofY, "AssignCharacterDialog.XofY");
			this.m_labelXofY.Location = new System.Drawing.Point(478, 497);
			this.m_labelXofY.Name = "m_labelXofY";
			this.m_labelXofY.Size = new System.Drawing.Size(64, 18);
			this.m_labelXofY.TabIndex = 11;
			this.m_labelXofY.Text = "{0} of {1}";
			this.m_labelXofY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// m_llMoreChar
			// 
			this.m_llMoreChar.AutoSize = true;
			this.m_llMoreChar.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llMoreChar, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llMoreChar, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llMoreChar, "AssignCharacterDialog.MoreCharacters");
			this.m_llMoreChar.Location = new System.Drawing.Point(410, 257);
			this.m_llMoreChar.Name = "m_llMoreChar";
			this.m_llMoreChar.Size = new System.Drawing.Size(85, 13);
			this.m_llMoreChar.TabIndex = 13;
			this.m_llMoreChar.TabStop = true;
			this.m_llMoreChar.Text = "More Characters";
			this.m_llMoreChar.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_llMoreChar.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llMoreChar_LinkClicked);
			// 
			// m_txtCharacterFilter
			// 
			this.m_txtCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtCharacterFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtCharacterFilter, "AssignCharacterDialog.textBox1");
			this.m_txtCharacterFilter.Location = new System.Drawing.Point(1, 1);
			this.m_txtCharacterFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtCharacterFilter.Name = "m_txtCharacterFilter";
			this.m_txtCharacterFilter.Size = new System.Drawing.Size(160, 13);
			this.m_txtCharacterFilter.TabIndex = 14;
			this.m_txtCharacterFilter.TextChanged += new System.EventHandler(this.m_txtCharacterFilter_TextChanged);
			// 
			// m_lblCharacter
			// 
			this.m_lblCharacter.AutoSize = true;
			this.m_lblCharacter.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblCharacter, "AssignCharacterDialog.Character");
			this.m_lblCharacter.Location = new System.Drawing.Point(410, 43);
			this.m_lblCharacter.Name = "m_lblCharacter";
			this.m_lblCharacter.Size = new System.Drawing.Size(53, 13);
			this.m_lblCharacter.TabIndex = 16;
			this.m_lblCharacter.Text = "Character";
			// 
			// m_lblDelivery
			// 
			this.m_lblDelivery.AutoSize = true;
			this.m_lblDelivery.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDelivery, "AssignCharacterDialog.Delivery");
			this.m_lblDelivery.Location = new System.Drawing.Point(410, 298);
			this.m_lblDelivery.Name = "m_lblDelivery";
			this.m_lblDelivery.Size = new System.Drawing.Size(45, 13);
			this.m_lblDelivery.TabIndex = 17;
			this.m_lblDelivery.Text = "Delivery";
			// 
			// m_llMoreDel
			// 
			this.m_llMoreDel.AutoSize = true;
			this.m_llMoreDel.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_llMoreDel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llMoreDel, "AssignCharacterDialog.MoreDeliveries");
			this.m_llMoreDel.Location = new System.Drawing.Point(410, 434);
			this.m_llMoreDel.Name = "m_llMoreDel";
			this.m_llMoreDel.Size = new System.Drawing.Size(80, 13);
			this.m_llMoreDel.TabIndex = 18;
			this.m_llMoreDel.TabStop = true;
			this.m_llMoreDel.Text = "More Deliveries";
			this.m_llMoreDel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llMoreDel_LinkClicked);
			// 
			// m_txtDeliveryFilter
			// 
			this.m_txtDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtDeliveryFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtDeliveryFilter, "AssignCharacterDialog.textBox1");
			this.m_txtDeliveryFilter.Location = new System.Drawing.Point(1, 1);
			this.m_txtDeliveryFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtDeliveryFilter.Name = "m_txtDeliveryFilter";
			this.m_txtDeliveryFilter.Size = new System.Drawing.Size(160, 13);
			this.m_txtDeliveryFilter.TabIndex = 19;
			this.m_txtDeliveryFilter.TextChanged += new System.EventHandler(this.m_txtDeliveryFilter_TextChanged);
			// 
			// m_icnCharacterFilter
			// 
			this.m_icnCharacterFilter.Image = global::ProtoScript.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnCharacterFilter, "AssignCharacterDialog.pictureBox1");
			this.m_icnCharacterFilter.Location = new System.Drawing.Point(161, -1);
			this.m_icnCharacterFilter.Name = "m_icnCharacterFilter";
			this.m_icnCharacterFilter.Size = new System.Drawing.Size(18, 20);
			this.m_icnCharacterFilter.TabIndex = 22;
			this.m_icnCharacterFilter.TabStop = false;
			this.m_icnCharacterFilter.Click += new System.EventHandler(this.m_icnCharacterFilter_Click);
			// 
			// m_icnDeliveryFilter
			// 
			this.m_icnDeliveryFilter.Image = global::ProtoScript.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnDeliveryFilter, "AssignCharacterDialog.pictureBox1");
			this.m_icnDeliveryFilter.Location = new System.Drawing.Point(161, -1);
			this.m_icnDeliveryFilter.Name = "m_icnDeliveryFilter";
			this.m_icnDeliveryFilter.Size = new System.Drawing.Size(18, 20);
			this.m_icnDeliveryFilter.TabIndex = 22;
			this.m_icnDeliveryFilter.TabStop = false;
			this.m_icnDeliveryFilter.Click += new System.EventHandler(this.m_icnDeliveryFilter_Click);
			// 
			// m_btnAddCharacter
			// 
			this.m_btnAddCharacter.BackgroundImage = global::ProtoScript.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_btnAddCharacter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.m_btnAddCharacter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_btnAddCharacter.FlatAppearance.BorderSize = 2;
			this.m_btnAddCharacter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.m_btnAddCharacter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAddCharacter, "Add New Character");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAddCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAddCharacter, "AssignCharacterDialog.AddCharacter");
			this.m_btnAddCharacter.Location = new System.Drawing.Point(598, 59);
			this.m_btnAddCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnAddCharacter.Name = "m_btnAddCharacter";
			this.m_btnAddCharacter.Size = new System.Drawing.Size(20, 20);
			this.m_btnAddCharacter.TabIndex = 24;
			this.m_btnAddCharacter.UseVisualStyleBackColor = false;
			this.m_btnAddCharacter.Click += new System.EventHandler(this.m_btnAddCharacter_Click);
			// 
			// m_btnAddDelivery
			// 
			this.m_btnAddDelivery.BackgroundImage = global::ProtoScript.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_btnAddDelivery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.m_btnAddDelivery.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_btnAddDelivery.FlatAppearance.BorderSize = 2;
			this.m_btnAddDelivery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.m_btnAddDelivery.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAddDelivery, "Add New Delivery");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAddDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAddDelivery, "AssignCharacterDialog.AddDelivery");
			this.m_btnAddDelivery.Location = new System.Drawing.Point(598, 314);
			this.m_btnAddDelivery.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnAddDelivery.Name = "m_btnAddDelivery";
			this.m_btnAddDelivery.Size = new System.Drawing.Size(20, 20);
			this.m_btnAddDelivery.TabIndex = 25;
			this.m_btnAddDelivery.UseVisualStyleBackColor = false;
			this.m_btnAddDelivery.Click += new System.EventHandler(this.m_btnAddDelivery_Click);
			// 
			// m_listBoxCharacters
			// 
			this.m_listBoxCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_listBoxCharacters.FormattingEnabled = true;
			this.m_listBoxCharacters.Location = new System.Drawing.Point(413, 59);
			this.m_listBoxCharacters.Name = "m_listBoxCharacters";
			this.m_listBoxCharacters.Size = new System.Drawing.Size(205, 195);
			this.m_listBoxCharacters.TabIndex = 0;
			this.m_listBoxCharacters.SelectedIndexChanged += new System.EventHandler(this.m_listBoxCharacters_SelectedIndexChanged);
			// 
			// m_listBoxDeliveries
			// 
			this.m_listBoxDeliveries.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_listBoxDeliveries.FormattingEnabled = true;
			this.m_listBoxDeliveries.Location = new System.Drawing.Point(413, 314);
			this.m_listBoxDeliveries.Name = "m_listBoxDeliveries";
			this.m_listBoxDeliveries.Size = new System.Drawing.Size(205, 117);
			this.m_listBoxDeliveries.TabIndex = 1;
			this.m_listBoxDeliveries.SelectedIndexChanged += new System.EventHandler(this.m_listBoxDeliveries_SelectedIndexChanged);
			// 
			// m_pnlCharacterFilter
			// 
			this.m_pnlCharacterFilter.BackColor = System.Drawing.Color.White;
			this.m_pnlCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlCharacterFilter.Controls.Add(this.m_icnCharacterFilter);
			this.m_pnlCharacterFilter.Controls.Add(this.m_txtCharacterFilter);
			this.m_pnlCharacterFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.m_pnlCharacterFilter.Location = new System.Drawing.Point(413, 59);
			this.m_pnlCharacterFilter.Name = "m_pnlCharacterFilter";
			this.m_pnlCharacterFilter.Size = new System.Drawing.Size(181, 20);
			this.m_pnlCharacterFilter.TabIndex = 21;
			// 
			// m_pnlDeliveryFilter
			// 
			this.m_pnlDeliveryFilter.BackColor = System.Drawing.Color.White;
			this.m_pnlDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlDeliveryFilter.Controls.Add(this.m_icnDeliveryFilter);
			this.m_pnlDeliveryFilter.Controls.Add(this.m_txtDeliveryFilter);
			this.m_pnlDeliveryFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.m_pnlDeliveryFilter.Location = new System.Drawing.Point(413, 314);
			this.m_pnlDeliveryFilter.Name = "m_pnlDeliveryFilter";
			this.m_pnlDeliveryFilter.Size = new System.Drawing.Size(181, 20);
			this.m_pnlDeliveryFilter.TabIndex = 23;
			// 
			// m_progressBar
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_progressBar, "AssignCharacterDialog.AssignCharacterDialog.m_progressBar");
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
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(12, 43);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(383, 475);
			this.m_blocksDisplayBrowser.TabIndex = 2;
			// 
			// AssignCharacterDialog
			// 
			this.AcceptButton = this.m_btnAssign;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(630, 561);
			this.Controls.Add(this.m_btnAddDelivery);
			this.Controls.Add(this.m_btnAddCharacter);
			this.Controls.Add(this.m_pnlDeliveryFilter);
			this.Controls.Add(this.m_pnlCharacterFilter);
			this.Controls.Add(this.m_llMoreDel);
			this.Controls.Add(this.m_lblDelivery);
			this.Controls.Add(this.m_lblCharacter);
			this.Controls.Add(this.m_llMoreChar);
			this.Controls.Add(this.m_progressBar);
			this.Controls.Add(this.m_labelXofY);
			this.Controls.Add(this.m_labelWhoSpeaks);
			this.Controls.Add(this.m_listBoxDeliveries);
			this.Controls.Add(this.m_labelReference);
			this.Controls.Add(this.m_listBoxCharacters);
			this.Controls.Add(this.m_btnAssign);
			this.Controls.Add(this.m_blocksDisplayBrowser);
			this.Controls.Add(this.m_btnPrevious);
			this.Controls.Add(this.m_btnNext);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "AssignCharacterDialog.AssignCharacter");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AssignCharacterDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assign Character";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssignCharacterDialog_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).EndInit();
			this.m_pnlCharacterFilter.ResumeLayout(false);
			this.m_pnlCharacterFilter.PerformLayout();
			this.m_pnlDeliveryFilter.ResumeLayout(false);
			this.m_pnlDeliveryFilter.PerformLayout();
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
		private System.Windows.Forms.ListBox m_listBoxDeliveries;
		private System.Windows.Forms.Label m_labelWhoSpeaks;
		private System.Windows.Forms.Label m_labelXofY;
		private ProtoScript.Controls.ProgressBarUnanimated m_progressBar;
		private System.Windows.Forms.LinkLabel m_llMoreChar;
		private System.Windows.Forms.TextBox m_txtCharacterFilter;
		private System.Windows.Forms.Label m_lblCharacter;
		private System.Windows.Forms.Label m_lblDelivery;
		private System.Windows.Forms.LinkLabel m_llMoreDel;
		private System.Windows.Forms.TextBox m_txtDeliveryFilter;
		private System.Windows.Forms.Panel m_pnlCharacterFilter;
		private System.Windows.Forms.PictureBox m_icnCharacterFilter;
		private System.Windows.Forms.Panel m_pnlDeliveryFilter;
		private System.Windows.Forms.PictureBox m_icnDeliveryFilter;
		private System.Windows.Forms.Button m_btnAddCharacter;
		private System.Windows.Forms.Button m_btnAddDelivery;
	}
}