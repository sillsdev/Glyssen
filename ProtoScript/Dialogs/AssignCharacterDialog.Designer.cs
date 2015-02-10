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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
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
			this.m_lblShortcut1 = new System.Windows.Forms.Label();
			this.m_lblShortcut2 = new System.Windows.Forms.Label();
			this.m_lblShortcut3 = new System.Windows.Forms.Label();
			this.m_lblShortcut4 = new System.Windows.Forms.Label();
			this.m_lblShortcut5 = new System.Windows.Forms.Label();
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_toolStripButtonHtmlView = new System.Windows.Forms.ToolStripButton();
			this.m_toolStripButtonGridView = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripComboBoxFilter = new System.Windows.Forms.ToolStripComboBox();
			this.m_toolStripButtonExcludeUserConfirmed = new System.Windows.Forms.ToolStripButton();
			this.m_dataGridViewBlocks = new System.Windows.Forms.DataGridView();
			this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCharacter = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDelivery = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			this.m_progressBar = new ProtoScript.Controls.ProgressBarUnanimated();
			this.m_listBoxCharacters = new System.Windows.Forms.ListBox();
			this.m_listBoxDeliveries = new System.Windows.Forms.ListBox();
			this.m_pnlCharacterFilter = new System.Windows.Forms.Panel();
			this.tableLayoutPanelCharacter = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlDeliveryFilter = new System.Windows.Forms.Panel();
			this.tableLayoutPanelDelivery = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlShortcuts = new System.Windows.Forms.Panel();
			this.m_pnlCharacterAndDeliverySelection = new System.Windows.Forms.TableLayoutPanel();
			this.m_splitContainer = new System.Windows.Forms.SplitContainer();
			this.m_tableBlocks = new System.Windows.Forms.TableLayoutPanel();
			this.m_panelContext = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).BeginInit();
			this.m_pnlCharacterFilter.SuspendLayout();
			this.tableLayoutPanelCharacter.SuspendLayout();
			this.m_pnlDeliveryFilter.SuspendLayout();
			this.tableLayoutPanelDelivery.SuspendLayout();
			this.m_pnlShortcuts.SuspendLayout();
			this.m_pnlCharacterAndDeliverySelection.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).BeginInit();
			this.m_splitContainer.Panel1.SuspendLayout();
			this.m_splitContainer.Panel2.SuspendLayout();
			this.m_splitContainer.SuspendLayout();
			this.m_tableBlocks.SuspendLayout();
			this.m_panelContext.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "AssignCharacterDialog";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnNext.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(224, 482);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 4;
			this.m_btnNext.Text = "Next";
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnPrevious
			// 
			this.m_btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnPrevious.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnPrevious, "Common.Previous");
			this.m_btnPrevious.Location = new System.Drawing.Point(83, 482);
			this.m_btnPrevious.Name = "m_btnPrevious";
			this.m_btnPrevious.Size = new System.Drawing.Size(75, 23);
			this.m_btnPrevious.TabIndex = 3;
			this.m_btnPrevious.Text = "Previous";
			this.m_btnPrevious.UseVisualStyleBackColor = true;
			this.m_btnPrevious.Click += new System.EventHandler(this.m_btnPrevious_Click);
			// 
			// m_btnAssign
			// 
			this.m_btnAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnAssign.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAssign, "AssignCharacterDialog.AssignCharacter");
			this.m_btnAssign.Location = new System.Drawing.Point(204, 442);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(95, 23);
			this.m_btnAssign.TabIndex = 2;
			this.m_btnAssign.Text = "Assign Character";
			this.m_btnAssign.UseVisualStyleBackColor = true;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_labelReference
			// 
			this.m_labelReference.AutoSize = true;
			this.m_labelReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_labelReference.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_labelReference, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelReference, "AssignCharacterDialog.label1");
			this.m_labelReference.Location = new System.Drawing.Point(222, 0);
			this.m_labelReference.Name = "m_labelReference";
			this.m_labelReference.Size = new System.Drawing.Size(76, 18);
			this.m_labelReference.TabIndex = 5;
			this.m_labelReference.Text = "Reference";
			this.m_labelReference.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// m_labelWhoSpeaks
			// 
			this.m_labelWhoSpeaks.AutoSize = true;
			this.m_labelWhoSpeaks.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_labelWhoSpeaks.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelWhoSpeaks, "AssignCharacterDialog.WhoSpeaks");
			this.m_labelWhoSpeaks.Location = new System.Drawing.Point(3, 0);
			this.m_labelWhoSpeaks.Name = "m_labelWhoSpeaks";
			this.m_labelWhoSpeaks.Size = new System.Drawing.Size(156, 18);
			this.m_labelWhoSpeaks.TabIndex = 10;
			this.m_labelWhoSpeaks.Text = "Who speaks this part?";
			// 
			// m_labelXofY
			// 
			this.m_labelXofY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_labelXofY.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelXofY, "{0} is the current block number; {1} is the total number of blocks.");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelXofY, "AssignCharacterDialog.XofY");
			this.m_labelXofY.Location = new System.Drawing.Point(159, 484);
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
			this.m_llMoreChar.Location = new System.Drawing.Point(21, 198);
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
			this.m_txtCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_txtCharacterFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtCharacterFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtCharacterFilter, "AssignCharacterDialog.AssignCharacterDialog.m_txtCharacterFilter");
			this.m_txtCharacterFilter.Location = new System.Drawing.Point(0, 0);
			this.m_txtCharacterFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtCharacterFilter.Name = "m_txtCharacterFilter";
			this.m_txtCharacterFilter.Size = new System.Drawing.Size(232, 17);
			this.m_txtCharacterFilter.TabIndex = 14;
			this.m_txtCharacterFilter.TextChanged += new System.EventHandler(this.m_txtCharacterFilter_TextChanged);
			// 
			// m_lblCharacter
			// 
			this.m_lblCharacter.AutoSize = true;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_lblCharacter, 2);
			this.m_lblCharacter.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblCharacter.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblCharacter, "AssignCharacterDialog.Character");
			this.m_lblCharacter.Location = new System.Drawing.Point(21, 0);
			this.m_lblCharacter.Name = "m_lblCharacter";
			this.m_lblCharacter.Size = new System.Drawing.Size(73, 18);
			this.m_lblCharacter.TabIndex = 16;
			this.m_lblCharacter.Text = "Character";
			// 
			// m_lblDelivery
			// 
			this.m_lblDelivery.AutoSize = true;
			this.m_lblDelivery.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblDelivery.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDelivery, "AssignCharacterDialog.Delivery");
			this.m_lblDelivery.Location = new System.Drawing.Point(21, 211);
			this.m_lblDelivery.Name = "m_lblDelivery";
			this.m_lblDelivery.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.m_lblDelivery.Size = new System.Drawing.Size(60, 28);
			this.m_lblDelivery.TabIndex = 17;
			this.m_lblDelivery.Text = "Delivery";
			this.m_lblDelivery.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// m_llMoreDel
			// 
			this.m_llMoreDel.AutoSize = true;
			this.m_llMoreDel.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_llMoreDel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llMoreDel, "AssignCharacterDialog.MoreDeliveries");
			this.m_llMoreDel.Location = new System.Drawing.Point(21, 362);
			this.m_llMoreDel.Name = "m_llMoreDel";
			this.m_llMoreDel.Size = new System.Drawing.Size(80, 13);
			this.m_llMoreDel.TabIndex = 18;
			this.m_llMoreDel.TabStop = true;
			this.m_llMoreDel.Text = "More Deliveries";
			this.m_llMoreDel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llMoreDel_LinkClicked);
			// 
			// m_txtDeliveryFilter
			// 
			this.m_txtDeliveryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_txtDeliveryFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtDeliveryFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtDeliveryFilter, "AssignCharacterDialog.AssignCharacterDialog.m_txtDeliveryFilter");
			this.m_txtDeliveryFilter.Location = new System.Drawing.Point(0, 0);
			this.m_txtDeliveryFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtDeliveryFilter.Name = "m_txtDeliveryFilter";
			this.m_txtDeliveryFilter.Size = new System.Drawing.Size(232, 17);
			this.m_txtDeliveryFilter.TabIndex = 19;
			this.m_txtDeliveryFilter.TextChanged += new System.EventHandler(this.m_txtDeliveryFilter_TextChanged);
			// 
			// m_icnCharacterFilter
			// 
			this.m_icnCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_icnCharacterFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.m_icnCharacterFilter.Image = global::ProtoScript.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnCharacterFilter, "AssignCharacterDialog.pictureBox1");
			this.m_icnCharacterFilter.Location = new System.Drawing.Point(232, 0);
			this.m_icnCharacterFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_icnCharacterFilter.Name = "m_icnCharacterFilter";
			this.m_icnCharacterFilter.Size = new System.Drawing.Size(18, 17);
			this.m_icnCharacterFilter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.m_icnCharacterFilter.TabIndex = 22;
			this.m_icnCharacterFilter.TabStop = false;
			this.m_icnCharacterFilter.Click += new System.EventHandler(this.m_icnCharacterFilter_Click);
			// 
			// m_icnDeliveryFilter
			// 
			this.m_icnDeliveryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_icnDeliveryFilter.Image = global::ProtoScript.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnDeliveryFilter, "AssignCharacterDialog.pictureBox1");
			this.m_icnDeliveryFilter.Location = new System.Drawing.Point(232, 0);
			this.m_icnDeliveryFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_icnDeliveryFilter.Name = "m_icnDeliveryFilter";
			this.m_icnDeliveryFilter.Size = new System.Drawing.Size(18, 17);
			this.m_icnDeliveryFilter.TabIndex = 22;
			this.m_icnDeliveryFilter.TabStop = false;
			this.m_icnDeliveryFilter.Click += new System.EventHandler(this.m_icnDeliveryFilter_Click);
			// 
			// m_btnAddCharacter
			// 
			this.m_btnAddCharacter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_btnAddCharacter.BackgroundImage = global::ProtoScript.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_btnAddCharacter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.m_btnAddCharacter.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_btnAddCharacter.FlatAppearance.BorderSize = 2;
			this.m_btnAddCharacter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.m_btnAddCharacter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAddCharacter, "Add New Character");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAddCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAddCharacter, "AssignCharacterDialog.AddCharacter");
			this.m_btnAddCharacter.Location = new System.Drawing.Point(278, 21);
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
			this.m_btnAddDelivery.Location = new System.Drawing.Point(278, 239);
			this.m_btnAddDelivery.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnAddDelivery.Name = "m_btnAddDelivery";
			this.m_btnAddDelivery.Size = new System.Drawing.Size(20, 20);
			this.m_btnAddDelivery.TabIndex = 25;
			this.m_btnAddDelivery.UseVisualStyleBackColor = false;
			this.m_btnAddDelivery.Click += new System.EventHandler(this.m_btnAddDelivery_Click);
			// 
			// m_lblShortcut1
			// 
			this.m_lblShortcut1.AutoSize = true;
			this.m_lblShortcut1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblShortcut1.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut1, "AssignCharacterDialog.label1");
			this.m_lblShortcut1.Location = new System.Drawing.Point(3, 3);
			this.m_lblShortcut1.Name = "m_lblShortcut1";
			this.m_lblShortcut1.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut1.TabIndex = 26;
			this.m_lblShortcut1.Text = "1";
			// 
			// m_lblShortcut2
			// 
			this.m_lblShortcut2.AutoSize = true;
			this.m_lblShortcut2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblShortcut2.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut2, "AssignCharacterDialog.label1");
			this.m_lblShortcut2.Location = new System.Drawing.Point(3, 22);
			this.m_lblShortcut2.Name = "m_lblShortcut2";
			this.m_lblShortcut2.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut2.TabIndex = 27;
			this.m_lblShortcut2.Text = "2";
			// 
			// m_lblShortcut3
			// 
			this.m_lblShortcut3.AutoSize = true;
			this.m_lblShortcut3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblShortcut3.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut3, "AssignCharacterDialog.label1");
			this.m_lblShortcut3.Location = new System.Drawing.Point(3, 41);
			this.m_lblShortcut3.Name = "m_lblShortcut3";
			this.m_lblShortcut3.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut3.TabIndex = 28;
			this.m_lblShortcut3.Text = "3";
			// 
			// m_lblShortcut4
			// 
			this.m_lblShortcut4.AutoSize = true;
			this.m_lblShortcut4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblShortcut4.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut4, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut4, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut4, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut4, "AssignCharacterDialog.label1");
			this.m_lblShortcut4.Location = new System.Drawing.Point(3, 60);
			this.m_lblShortcut4.Name = "m_lblShortcut4";
			this.m_lblShortcut4.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut4.TabIndex = 29;
			this.m_lblShortcut4.Text = "4";
			// 
			// m_lblShortcut5
			// 
			this.m_lblShortcut5.AutoSize = true;
			this.m_lblShortcut5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lblShortcut5.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut5, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut5, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut5, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut5, "AssignCharacterDialog.label1");
			this.m_lblShortcut5.Location = new System.Drawing.Point(3, 79);
			this.m_lblShortcut5.Name = "m_lblShortcut5";
			this.m_lblShortcut5.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut5.TabIndex = 30;
			this.m_lblShortcut5.Text = "5";
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_toolStripButtonHtmlView,
            this.m_toolStripButtonGridView,
            this.toolStripSeparator1,
            this.m_toolStripLabelFilter,
            this.m_toolStripComboBoxFilter,
            this.m_toolStripButtonExcludeUserConfirmed});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "AssignCharacterDialog.AssignCharacterDialog.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Size = new System.Drawing.Size(635, 25);
			this.m_toolStrip.TabIndex = 31;
			// 
			// m_toolStripButtonHtmlView
			// 
			this.m_toolStripButtonHtmlView.Checked = true;
			this.m_toolStripButtonHtmlView.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_toolStripButtonHtmlView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonHtmlView.Image = global::ProtoScript.Properties.Resources.html_view;
			this.m_toolStripButtonHtmlView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonHtmlView, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonHtmlView, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonHtmlView, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonHtmlView, "AssignCharacterDialog.AssignCharacterDialog.m_toolStripButtonHtmlView");
			this.m_toolStripButtonHtmlView.Name = "m_toolStripButtonHtmlView";
			this.m_toolStripButtonHtmlView.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonHtmlView.Text = "Formatted view";
			this.m_toolStripButtonHtmlView.ToolTipText = "Left pane shows the highlighted block and surrounding context formatted as Script" +
    "ure";
			this.m_toolStripButtonHtmlView.CheckedChanged += new System.EventHandler(this.HandleHtmlViewCheckChanged);
			this.m_toolStripButtonHtmlView.Click += new System.EventHandler(this.HandleViewTypeToolStripButtonClick);
			// 
			// m_toolStripButtonGridView
			// 
			this.m_toolStripButtonGridView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonGridView.Image = global::ProtoScript.Properties.Resources.grid_icon;
			this.m_toolStripButtonGridView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonGridView, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonGridView, "AssignCharacterDialog.AssignCharacterDialog.m_toolStripButtonGridView");
			this.m_toolStripButtonGridView.Name = "m_toolStripButtonGridView";
			this.m_toolStripButtonGridView.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonGridView.Text = "Grid view";
			this.m_toolStripButtonGridView.ToolTipText = "Left pane shows the highlighted block and surrounding context in a grid";
			this.m_toolStripButtonGridView.CheckedChanged += new System.EventHandler(this.HandleDataGridViewCheckChanged);
			this.m_toolStripButtonGridView.Click += new System.EventHandler(this.HandleViewTypeToolStripButtonClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// m_toolStripLabelFilter
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripLabelFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripLabelFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripLabelFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripLabelFilter, "AssignCharacterDialog.AssignCharacterDialog.m_toolStripLabelFilter");
			this.m_toolStripLabelFilter.Name = "m_toolStripLabelFilter";
			this.m_toolStripLabelFilter.Size = new System.Drawing.Size(36, 22);
			this.m_toolStripLabelFilter.Text = "Filter:";
			// 
			// m_toolStripComboBoxFilter
			// 
			this.m_toolStripComboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_toolStripComboBoxFilter.Items.AddRange(new object[] {
            "Quotes not assigned automatically",
            "More quotes than expected in verse",
            "All Scripture"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripComboBoxFilter, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripComboBoxFilter, "AssignCharacterDialog.AssignCharacterDialog.m_toolStripComboBoxFilter");
			this.m_toolStripComboBoxFilter.Name = "m_toolStripComboBoxFilter";
			this.m_toolStripComboBoxFilter.Size = new System.Drawing.Size(225, 25);
			this.m_toolStripComboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.HandleFilterChanged);
			// 
			// m_toolStripButtonExcludeUserConfirmed
			// 
			this.m_toolStripButtonExcludeUserConfirmed.CheckOnClick = true;
			this.m_toolStripButtonExcludeUserConfirmed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonExcludeUserConfirmed.Image = global::ProtoScript.Properties.Resources.yellow_ok_icon;
			this.m_toolStripButtonExcludeUserConfirmed.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonExcludeUserConfirmed, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonExcludeUserConfirmed, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonExcludeUserConfirmed, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonExcludeUserConfirmed, "AssignCharacterDialog.AssignCharacterDialog.m_toolStripButtonExcludeUserConfirmed" +
        "");
			this.m_toolStripButtonExcludeUserConfirmed.Name = "m_toolStripButtonExcludeUserConfirmed";
			this.m_toolStripButtonExcludeUserConfirmed.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonExcludeUserConfirmed.Text = "Exclude confirmed";
			this.m_toolStripButtonExcludeUserConfirmed.ToolTipText = "Exclude blocks that are already user-confirmed";
			this.m_toolStripButtonExcludeUserConfirmed.CheckedChanged += new System.EventHandler(this.HandleFilterChanged);
			// 
			// m_dataGridViewBlocks
			// 
			this.m_dataGridViewBlocks.AllowUserToAddRows = false;
			this.m_dataGridViewBlocks.AllowUserToDeleteRows = false;
			this.m_dataGridViewBlocks.AllowUserToOrderColumns = true;
			this.m_dataGridViewBlocks.AllowUserToResizeRows = false;
			this.m_dataGridViewBlocks.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
			this.m_dataGridViewBlocks.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_dataGridViewBlocks.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_dataGridViewBlocks.CausesValidation = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGridViewBlocks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.m_dataGridViewBlocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGridViewBlocks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReference,
            this.colCharacter,
            this.colDelivery,
            this.colText});
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGridViewBlocks.DefaultCellStyle = dataGridViewCellStyle3;
			this.m_dataGridViewBlocks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_dataGridViewBlocks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_dataGridViewBlocks, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_dataGridViewBlocks, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_dataGridViewBlocks, "AssignCharacterDialog.AssignCharacterDialog.m_dataGridViewBlocks");
			this.m_dataGridViewBlocks.Location = new System.Drawing.Point(0, 319);
			this.m_dataGridViewBlocks.MultiSelect = false;
			this.m_dataGridViewBlocks.Name = "m_dataGridViewBlocks";
			this.m_dataGridViewBlocks.ReadOnly = true;
			this.m_dataGridViewBlocks.RowHeadersVisible = false;
			this.m_dataGridViewBlocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGridViewBlocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGridViewBlocks.ShowCellErrors = false;
			this.m_dataGridViewBlocks.ShowEditingIcon = false;
			this.m_dataGridViewBlocks.ShowRowErrors = false;
			this.m_dataGridViewBlocks.Size = new System.Drawing.Size(292, 149);
			this.m_dataGridViewBlocks.TabIndex = 30;
			this.m_dataGridViewBlocks.VirtualMode = true;
			this.m_dataGridViewBlocks.Visible = false;
			this.m_dataGridViewBlocks.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.HandleDataGridViewBlocksCellValueNeeded);
			this.m_dataGridViewBlocks.SelectionChanged += new System.EventHandler(this.HandleDataGridViewBlocksSelectionChanged);
			// 
			// colReference
			// 
			this.colReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.colReference.DefaultCellStyle = dataGridViewCellStyle2;
			this.colReference.HeaderText = "Reference";
			this.colReference.MaxInputLength = 11;
			this.colReference.MinimumWidth = 30;
			this.colReference.Name = "colReference";
			this.colReference.ReadOnly = true;
			this.colReference.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colReference.Width = 82;
			// 
			// colCharacter
			// 
			this.colCharacter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colCharacter.FillWeight = 200F;
			this.colCharacter.HeaderText = "Character";
			this.colCharacter.MinimumWidth = 60;
			this.colCharacter.Name = "colCharacter";
			this.colCharacter.ReadOnly = true;
			this.colCharacter.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colDelivery
			// 
			this.colDelivery.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colDelivery.HeaderText = "Delivery";
			this.colDelivery.MinimumWidth = 60;
			this.colDelivery.Name = "colDelivery";
			this.colDelivery.ReadOnly = true;
			this.colDelivery.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colText
			// 
			this.colText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colText.FillWeight = 200F;
			this.colText.HeaderText = "Text";
			this.colText.MinimumWidth = 60;
			this.colText.Name = "colText";
			this.colText.ReadOnly = true;
			this.colText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// m_blocksDisplayBrowser
			// 
			this.m_blocksDisplayBrowser.AutoSize = true;
			this.m_blocksDisplayBrowser.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_blocksDisplayBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_blocksDisplayBrowser, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksDisplayBrowser, "ProjectSettingsDialog.Browser");
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(0, 0);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(295, 471);
			this.m_blocksDisplayBrowser.TabIndex = 2;
			this.m_blocksDisplayBrowser.OnMouseOver += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseOver);
			this.m_blocksDisplayBrowser.OnMouseOut += new System.EventHandler<Gecko.DomMouseEventArgs>(this.OnMouseOut);
			this.m_blocksDisplayBrowser.OnDocumentCompleted += new System.EventHandler<Gecko.Events.GeckoDocumentCompletedEventArgs>(this.OnDocumentCompleted);
			// 
			// m_progressBar
			// 
			this.m_progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_progressBar, "AssignCharacterDialog.AssignCharacterDialog.m_progressBar");
			this.m_progressBar.Location = new System.Drawing.Point(0, 544);
			this.m_progressBar.Name = "m_progressBar";
			this.m_progressBar.Size = new System.Drawing.Size(635, 17);
			this.m_progressBar.TabIndex = 12;
			// 
			// m_listBoxCharacters
			// 
			this.m_listBoxCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_listBoxCharacters, 2);
			this.m_listBoxCharacters.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_listBoxCharacters.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_listBoxCharacters.FormattingEnabled = true;
			this.m_listBoxCharacters.IntegralHeight = false;
			this.m_listBoxCharacters.ItemHeight = 18;
			this.m_listBoxCharacters.Location = new System.Drawing.Point(21, 48);
			this.m_listBoxCharacters.Name = "m_listBoxCharacters";
			this.m_listBoxCharacters.Size = new System.Drawing.Size(274, 147);
			this.m_listBoxCharacters.TabIndex = 0;
			this.m_listBoxCharacters.SelectedIndexChanged += new System.EventHandler(this.m_listBoxCharacters_SelectedIndexChanged);
			// 
			// m_listBoxDeliveries
			// 
			this.m_listBoxDeliveries.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_listBoxDeliveries, 2);
			this.m_listBoxDeliveries.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_listBoxDeliveries.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_listBoxDeliveries.FormattingEnabled = true;
			this.m_listBoxDeliveries.IntegralHeight = false;
			this.m_listBoxDeliveries.ItemHeight = 18;
			this.m_listBoxDeliveries.Location = new System.Drawing.Point(21, 269);
			this.m_listBoxDeliveries.Name = "m_listBoxDeliveries";
			this.m_listBoxDeliveries.Size = new System.Drawing.Size(274, 90);
			this.m_listBoxDeliveries.TabIndex = 1;
			this.m_listBoxDeliveries.SelectedIndexChanged += new System.EventHandler(this.m_listBoxDeliveries_SelectedIndexChanged);
			// 
			// m_pnlCharacterFilter
			// 
			this.m_pnlCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlCharacterFilter.AutoSize = true;
			this.m_pnlCharacterFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_pnlCharacterFilter.BackColor = System.Drawing.Color.White;
			this.m_pnlCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlCharacterFilter.Controls.Add(this.tableLayoutPanelCharacter);
			this.m_pnlCharacterFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.m_pnlCharacterFilter.Location = new System.Drawing.Point(21, 21);
			this.m_pnlCharacterFilter.Name = "m_pnlCharacterFilter";
			this.m_pnlCharacterFilter.Size = new System.Drawing.Size(254, 21);
			this.m_pnlCharacterFilter.TabIndex = 21;
			// 
			// tableLayoutPanelCharacter
			// 
			this.tableLayoutPanelCharacter.AutoSize = true;
			this.tableLayoutPanelCharacter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelCharacter.ColumnCount = 2;
			this.tableLayoutPanelCharacter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCharacter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelCharacter.Controls.Add(this.m_icnCharacterFilter, 1, 0);
			this.tableLayoutPanelCharacter.Controls.Add(this.m_txtCharacterFilter, 0, 0);
			this.tableLayoutPanelCharacter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelCharacter.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelCharacter.Name = "tableLayoutPanelCharacter";
			this.tableLayoutPanelCharacter.RowCount = 1;
			this.tableLayoutPanelCharacter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCharacter.Size = new System.Drawing.Size(250, 17);
			this.tableLayoutPanelCharacter.TabIndex = 0;
			// 
			// m_pnlDeliveryFilter
			// 
			this.m_pnlDeliveryFilter.AutoSize = true;
			this.m_pnlDeliveryFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_pnlDeliveryFilter.BackColor = System.Drawing.Color.White;
			this.m_pnlDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlDeliveryFilter.Controls.Add(this.tableLayoutPanelDelivery);
			this.m_pnlDeliveryFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.m_pnlDeliveryFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_pnlDeliveryFilter.Location = new System.Drawing.Point(21, 242);
			this.m_pnlDeliveryFilter.Name = "m_pnlDeliveryFilter";
			this.m_pnlDeliveryFilter.Size = new System.Drawing.Size(254, 21);
			this.m_pnlDeliveryFilter.TabIndex = 23;
			// 
			// tableLayoutPanelDelivery
			// 
			this.tableLayoutPanelDelivery.AutoSize = true;
			this.tableLayoutPanelDelivery.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelDelivery.ColumnCount = 2;
			this.tableLayoutPanelDelivery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelDelivery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelDelivery.Controls.Add(this.m_icnDeliveryFilter, 1, 0);
			this.tableLayoutPanelDelivery.Controls.Add(this.m_txtDeliveryFilter, 0, 0);
			this.tableLayoutPanelDelivery.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelDelivery.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelDelivery.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelDelivery.Name = "tableLayoutPanelDelivery";
			this.tableLayoutPanelDelivery.RowCount = 1;
			this.tableLayoutPanelDelivery.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelDelivery.Size = new System.Drawing.Size(250, 17);
			this.tableLayoutPanelDelivery.TabIndex = 0;
			// 
			// m_pnlShortcuts
			// 
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut5);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut4);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut3);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut2);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut1);
			this.m_pnlShortcuts.Location = new System.Drawing.Point(3, 48);
			this.m_pnlShortcuts.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_pnlShortcuts.Name = "m_pnlShortcuts";
			this.m_pnlShortcuts.Size = new System.Drawing.Size(15, 147);
			this.m_pnlShortcuts.TabIndex = 28;
			// 
			// m_pnlCharacterAndDeliverySelection
			// 
			this.m_pnlCharacterAndDeliverySelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlCharacterAndDeliverySelection.ColumnCount = 3;
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_lblCharacter, 1, 0);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_llMoreDel, 1, 7);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlDeliveryFilter, 1, 5);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_btnAddDelivery, 2, 5);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlShortcuts, 0, 2);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_btnAddCharacter, 2, 1);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_listBoxDeliveries, 1, 6);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlCharacterFilter, 1, 1);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_lblDelivery, 1, 4);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_listBoxCharacters, 1, 2);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_llMoreChar, 1, 3);
			this.m_pnlCharacterAndDeliverySelection.Location = new System.Drawing.Point(3, 36);
			this.m_pnlCharacterAndDeliverySelection.Name = "m_pnlCharacterAndDeliverySelection";
			this.m_pnlCharacterAndDeliverySelection.RowCount = 8;
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.Size = new System.Drawing.Size(298, 375);
			this.m_pnlCharacterAndDeliverySelection.TabIndex = 29;
			// 
			// m_splitContainer
			// 
			this.m_splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_splitContainer.Location = new System.Drawing.Point(0, 25);
			this.m_splitContainer.Name = "m_splitContainer";
			// 
			// m_splitContainer.Panel1
			// 
			this.m_splitContainer.Panel1.Controls.Add(this.m_tableBlocks);
			this.m_splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(12, 12, 0, 12);
			this.m_splitContainer.Panel1MinSize = 250;
			// 
			// m_splitContainer.Panel2
			// 
			this.m_splitContainer.Panel2.Controls.Add(this.m_pnlCharacterAndDeliverySelection);
			this.m_splitContainer.Panel2.Controls.Add(this.m_btnNext);
			this.m_splitContainer.Panel2.Controls.Add(this.m_btnPrevious);
			this.m_splitContainer.Panel2.Controls.Add(this.m_labelXofY);
			this.m_splitContainer.Panel2.Controls.Add(this.m_btnAssign);
			this.m_splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(0, 12, 12, 12);
			this.m_splitContainer.Panel2MinSize = 250;
			this.m_splitContainer.Size = new System.Drawing.Size(635, 519);
			this.m_splitContainer.SplitterDistance = 313;
			this.m_splitContainer.TabIndex = 30;
			// 
			// m_tableBlocks
			// 
			this.m_tableBlocks.AccessibleDescription = "";
			this.m_tableBlocks.ColumnCount = 2;
			this.m_tableBlocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableBlocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableBlocks.Controls.Add(this.m_panelContext, 0, 1);
			this.m_tableBlocks.Controls.Add(this.m_labelReference, 1, 0);
			this.m_tableBlocks.Controls.Add(this.m_labelWhoSpeaks, 0, 0);
			this.m_tableBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableBlocks.Location = new System.Drawing.Point(12, 12);
			this.m_tableBlocks.Name = "m_tableBlocks";
			this.m_tableBlocks.RowCount = 2;
			this.m_tableBlocks.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableBlocks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableBlocks.Size = new System.Drawing.Size(301, 495);
			this.m_tableBlocks.TabIndex = 11;
			// 
			// m_panelContext
			// 
			this.m_tableBlocks.SetColumnSpan(this.m_panelContext, 2);
			this.m_panelContext.Controls.Add(this.m_dataGridViewBlocks);
			this.m_panelContext.Controls.Add(this.m_blocksDisplayBrowser);
			this.m_panelContext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panelContext.Location = new System.Drawing.Point(3, 21);
			this.m_panelContext.Name = "m_panelContext";
			this.m_panelContext.Size = new System.Drawing.Size(295, 471);
			this.m_panelContext.TabIndex = 30;
			// 
			// AssignCharacterDialog
			// 
			this.AcceptButton = this.m_btnAssign;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(635, 561);
			this.Controls.Add(this.m_splitContainer);
			this.Controls.Add(this.m_progressBar);
			this.Controls.Add(this.m_toolStrip);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "AssignCharacterDialog.AssignCharacter");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(651, 599);
			this.Name = "AssignCharacterDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assign Characters";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssignCharacterDialog_FormClosing);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssignCharacterDialog_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AssignCharacterDialog_KeyPress);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).EndInit();
			this.m_pnlCharacterFilter.ResumeLayout(false);
			this.m_pnlCharacterFilter.PerformLayout();
			this.tableLayoutPanelCharacter.ResumeLayout(false);
			this.tableLayoutPanelCharacter.PerformLayout();
			this.m_pnlDeliveryFilter.ResumeLayout(false);
			this.m_pnlDeliveryFilter.PerformLayout();
			this.tableLayoutPanelDelivery.ResumeLayout(false);
			this.tableLayoutPanelDelivery.PerformLayout();
			this.m_pnlShortcuts.ResumeLayout(false);
			this.m_pnlShortcuts.PerformLayout();
			this.m_pnlCharacterAndDeliverySelection.ResumeLayout(false);
			this.m_pnlCharacterAndDeliverySelection.PerformLayout();
			this.m_splitContainer.Panel1.ResumeLayout(false);
			this.m_splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).EndInit();
			this.m_splitContainer.ResumeLayout(false);
			this.m_tableBlocks.ResumeLayout(false);
			this.m_tableBlocks.PerformLayout();
			this.m_panelContext.ResumeLayout(false);
			this.m_panelContext.PerformLayout();
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
		private System.Windows.Forms.Label m_lblShortcut1;
		private System.Windows.Forms.Label m_lblShortcut2;
		private System.Windows.Forms.Panel m_pnlShortcuts;
		private System.Windows.Forms.Label m_lblShortcut3;
		private System.Windows.Forms.Label m_lblShortcut5;
		private System.Windows.Forms.Label m_lblShortcut4;
		private System.Windows.Forms.TableLayoutPanel m_pnlCharacterAndDeliverySelection;
		private System.Windows.Forms.SplitContainer m_splitContainer;
		private System.Windows.Forms.TableLayoutPanel m_tableBlocks;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDelivery;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCharacter;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFilter;
		private System.Windows.Forms.ToolStripComboBox m_toolStripComboBoxFilter;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonExcludeUserConfirmed;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonGridView;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonHtmlView;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.DataGridView m_dataGridViewBlocks;
		private System.Windows.Forms.Panel m_panelContext;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCharacter;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDelivery;
		private System.Windows.Forms.DataGridViewTextBoxColumn colText;
	}
}