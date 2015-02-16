namespace ProtoScript.Dialogs
{
	partial class QuotationMarksDialog
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_comboQuoteMarks = new System.Windows.Forms.ComboBox();
			this.m_lblQuotationMarks = new System.Windows.Forms.Label();
			this.m_lblHorizontalSeparator = new System.Windows.Forms.Label();
			this.m_chkDialogueQuotations = new System.Windows.Forms.CheckBox();
			this.m_cboQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_cboEndQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_lblEndDialogueQuote = new System.Windows.Forms.Label();
			this.m_chkAlternateSpeakersInFirstLevelQuotes = new System.Windows.Forms.CheckBox();
			this.m_dataGridViewBlocks = new System.Windows.Forms.DataGridView();
			this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			this.m_labelProjectData = new System.Windows.Forms.Label();
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_toolStripButtonHtmlView = new System.Windows.Forms.ToolStripButton();
			this.m_toolStripButtonGridView = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripButtonLargerFont = new System.Windows.Forms.ToolStripButton();
			this.m_toolStripButtonSmallerFont = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripComboBoxFilter = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_scriptureReference = new Paratext.ToolStripVerseControl();
			this.m_pnlDialogeQuotes = new System.Windows.Forms.Panel();
			this.m_splitContainer = new System.Windows.Forms.SplitContainer();
			this.m_tableBlocks = new System.Windows.Forms.TableLayoutPanel();
			this.m_panelContext = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			this.m_pnlDialogeQuotes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).BeginInit();
			this.m_splitContainer.Panel1.SuspendLayout();
			this.m_splitContainer.Panel2.SuspendLayout();
			this.m_splitContainer.SuspendLayout();
			this.m_tableBlocks.SuspendLayout();
			this.m_panelContext.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(682, 398);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 0;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(596, 398);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.QuotationMarksDialog";
			// 
			// m_comboQuoteMarks
			// 
			this.m_comboQuoteMarks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_comboQuoteMarks.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.m_comboQuoteMarks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_comboQuoteMarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_comboQuoteMarks.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_comboQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_comboQuoteMarks, "DialogBoxes.QuotationMarksDialog.m_comboQuoteMarks");
			this.m_comboQuoteMarks.Location = new System.Drawing.Point(147, 2);
			this.m_comboQuoteMarks.MaxDropDownItems = 15;
			this.m_comboQuoteMarks.Name = "m_comboQuoteMarks";
			this.m_comboQuoteMarks.Size = new System.Drawing.Size(324, 30);
			this.m_comboQuoteMarks.TabIndex = 1;
			this.m_comboQuoteMarks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandlecomboQuoteMarksDrawItem);
			// 
			// m_lblQuotationMarks
			// 
			this.m_lblQuotationMarks.AutoSize = true;
			this.m_lblQuotationMarks.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuotationMarks, "DialogBoxes.QuotationMarksDialog.m_lblQuotationMarks");
			this.m_lblQuotationMarks.Location = new System.Drawing.Point(9, 13);
			this.m_lblQuotationMarks.Name = "m_lblQuotationMarks";
			this.m_lblQuotationMarks.Size = new System.Drawing.Size(132, 13);
			this.m_lblQuotationMarks.TabIndex = 6;
			this.m_lblQuotationMarks.Text = "First-level quotation marks:";
			// 
			// m_lblHorizontalSeparator
			// 
			this.m_lblHorizontalSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblHorizontalSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblHorizontalSeparator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblHorizontalSeparator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblHorizontalSeparator, "ProjectSettingsDialog.label1");
			this.m_lblHorizontalSeparator.Location = new System.Drawing.Point(3, 65);
			this.m_lblHorizontalSeparator.Name = "m_lblHorizontalSeparator";
			this.m_lblHorizontalSeparator.Size = new System.Drawing.Size(471, 2);
			this.m_lblHorizontalSeparator.TabIndex = 8;
			// 
			// m_chkDialogueQuotations
			// 
			this.m_chkDialogueQuotations.AutoSize = true;
			this.m_chkDialogueQuotations.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkDialogueQuotations, "DialogBoxes.QuotationMarksDialog.m_chkDialogueQuotations");
			this.m_chkDialogueQuotations.Location = new System.Drawing.Point(18, 12);
			this.m_chkDialogueQuotations.Name = "m_chkDialogueQuotations";
			this.m_chkDialogueQuotations.Size = new System.Drawing.Size(264, 17);
			this.m_chkDialogueQuotations.TabIndex = 9;
			this.m_chkDialogueQuotations.Text = "This project marks dialogue with quotation dashes.";
			this.m_chkDialogueQuotations.UseVisualStyleBackColor = true;
			this.m_chkDialogueQuotations.CheckedChanged += new System.EventHandler(this.m_chkDialogueQuotations_CheckedChanged);
			// 
			// m_cboQuotationDash
			// 
			this.m_cboQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboQuotationDash.Enabled = false;
			this.m_cboQuotationDash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cboQuotationDash.FormattingEnabled = true;
			this.m_cboQuotationDash.Items.AddRange(new object[] {
            "Quotation dash (U+2015)",
            "Em-dash (U+2014)"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboQuotationDash, "DialogBoxes.QuotationMarksDialog.m_cboQuotationDash");
			this.m_cboQuotationDash.Location = new System.Drawing.Point(286, 10);
			this.m_cboQuotationDash.Margin = new System.Windows.Forms.Padding(0);
			this.m_cboQuotationDash.Name = "m_cboQuotationDash";
			this.m_cboQuotationDash.Size = new System.Drawing.Size(185, 21);
			this.m_cboQuotationDash.TabIndex = 10;
			this.m_cboQuotationDash.TextChanged += new System.EventHandler(this.m_cboQuotationDash_TextChanged);
			// 
			// m_cboEndQuotationDash
			// 
			this.m_cboEndQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboEndQuotationDash.Enabled = false;
			this.m_cboEndQuotationDash.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cboEndQuotationDash.FormattingEnabled = true;
			this.m_cboEndQuotationDash.Items.AddRange(new object[] {
            "End of paragraph (only)",
            "#dash#",
            "Any punctuation mark"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboEndQuotationDash, "DialogBoxes.QuotationMarksDialog.m_cboEndQuotationDash");
			this.m_cboEndQuotationDash.Location = new System.Drawing.Point(286, 34);
			this.m_cboEndQuotationDash.Margin = new System.Windows.Forms.Padding(0);
			this.m_cboEndQuotationDash.Name = "m_cboEndQuotationDash";
			this.m_cboEndQuotationDash.Size = new System.Drawing.Size(185, 21);
			this.m_cboEndQuotationDash.TabIndex = 11;
			// 
			// m_lblEndDialogueQuote
			// 
			this.m_lblEndDialogueQuote.AutoSize = true;
			this.m_lblEndDialogueQuote.Enabled = false;
			this.m_lblEndDialogueQuote.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblEndDialogueQuote, "DialogBoxes.QuotationMarksDialog.m_lblEndDialogueQuote");
			this.m_lblEndDialogueQuote.Location = new System.Drawing.Point(138, 32);
			this.m_lblEndDialogueQuote.Name = "m_lblEndDialogueQuote";
			this.m_lblEndDialogueQuote.Size = new System.Drawing.Size(134, 13);
			this.m_lblEndDialogueQuote.TabIndex = 12;
			this.m_lblEndDialogueQuote.Text = "Dialogue quotation ending:";
			// 
			// m_chkAlternateSpeakersInFirstLevelQuotes
			// 
			this.m_chkAlternateSpeakersInFirstLevelQuotes.AutoSize = true;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Enabled = false;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkAlternateSpeakersInFirstLevelQuotes, "DialogBoxes.QuotationMarksDialog.m_chkAlternateSpeakersInFirstLevelQuotes");
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Location = new System.Drawing.Point(15, 98);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Name = "m_chkAlternateSpeakersInFirstLevelQuotes";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Size = new System.Drawing.Size(418, 17);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.TabIndex = 13;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Tag = "\"Making this invisible until we can find a project that actually uses this. Not y" +
    "et implemented in QuoteParser.\"";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Text = "Quotation dashes also indicate change of speaker within first-level quotation mar" +
    "ks.";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.UseVisualStyleBackColor = true;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Visible = false;
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_dataGridViewBlocks, "DialogBoxes.QuotationMarksDialog.m_dataGridViewBlocks");
			this.m_dataGridViewBlocks.Location = new System.Drawing.Point(-2, 221);
			this.m_dataGridViewBlocks.MultiSelect = false;
			this.m_dataGridViewBlocks.Name = "m_dataGridViewBlocks";
			this.m_dataGridViewBlocks.ReadOnly = true;
			this.m_dataGridViewBlocks.RowHeadersVisible = false;
			this.m_dataGridViewBlocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGridViewBlocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGridViewBlocks.ShowCellErrors = false;
			this.m_dataGridViewBlocks.ShowEditingIcon = false;
			this.m_dataGridViewBlocks.ShowRowErrors = false;
			this.m_dataGridViewBlocks.Size = new System.Drawing.Size(254, 97);
			this.m_dataGridViewBlocks.TabIndex = 30;
			this.m_dataGridViewBlocks.VirtualMode = true;
			this.m_dataGridViewBlocks.Visible = false;
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
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(252, 318);
			this.m_blocksDisplayBrowser.TabIndex = 2;
			this.m_blocksDisplayBrowser.OnDocumentCompleted += new System.EventHandler<Gecko.Events.GeckoDocumentCompletedEventArgs>(this.OnDocumentCompleted);
			// 
			// m_labelProjectData
			// 
			this.m_labelProjectData.AutoSize = true;
			this.m_labelProjectData.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_labelProjectData.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelProjectData, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelProjectData, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_labelProjectData, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelProjectData, "DialogBoxes.QuotationMarksDialog.m_labelProjectData");
			this.m_labelProjectData.Location = new System.Drawing.Point(3, 0);
			this.m_labelProjectData.Name = "m_labelProjectData";
			this.m_labelProjectData.Size = new System.Drawing.Size(94, 18);
			this.m_labelProjectData.TabIndex = 10;
			this.m_labelProjectData.Text = "Project Data:";
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_toolStripButtonHtmlView,
            this.m_toolStripButtonGridView,
            this.toolStripSeparator2,
            this.m_toolStripButtonLargerFont,
            this.m_toolStripButtonSmallerFont,
            this.toolStripSeparator1,
            this.m_toolStripLabelFilter,
            this.m_toolStripComboBoxFilter,
            this.toolStripSeparator3,
            this.m_scriptureReference});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.QuotationMarksDialog.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Size = new System.Drawing.Size(775, 25);
			this.m_toolStrip.TabIndex = 32;
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonHtmlView, "DialogBoxes.BlockNavigationControls.m_toolStripButtonHtmlView");
			this.m_toolStripButtonHtmlView.Name = "m_toolStripButtonHtmlView";
			this.m_toolStripButtonHtmlView.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonHtmlView.Text = "Formatted view";
			this.m_toolStripButtonHtmlView.ToolTipText = "Left pane shows the highlighted block and surrounding context formatted as Script" +
    "ure";
			// 
			// m_toolStripButtonGridView
			// 
			this.m_toolStripButtonGridView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonGridView.Image = global::ProtoScript.Properties.Resources.grid_icon;
			this.m_toolStripButtonGridView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonGridView, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonGridView, "DialogBoxes.BlockNavigationControls.m_toolStripButtonGridView");
			this.m_toolStripButtonGridView.Name = "m_toolStripButtonGridView";
			this.m_toolStripButtonGridView.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonGridView.Text = "Grid view";
			this.m_toolStripButtonGridView.ToolTipText = "Left pane shows the highlighted block and surrounding context in a grid";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// m_toolStripButtonLargerFont
			// 
			this.m_toolStripButtonLargerFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonLargerFont.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_toolStripButtonLargerFont.Image = global::ProtoScript.Properties.Resources.IncreaseSize;
			this.m_toolStripButtonLargerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonLargerFont, "Increase size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonLargerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonLargerFont, "DialogBoxes.BlockNavigationControls.m_toolStripButtonLargerFont");
			this.m_toolStripButtonLargerFont.Name = "m_toolStripButtonLargerFont";
			this.m_toolStripButtonLargerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonLargerFont.Text = "Increase size of text";
			// 
			// m_toolStripButtonSmallerFont
			// 
			this.m_toolStripButtonSmallerFont.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.m_toolStripButtonSmallerFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonSmallerFont.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_toolStripButtonSmallerFont.Image = global::ProtoScript.Properties.Resources.DecreaseSize;
			this.m_toolStripButtonSmallerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonSmallerFont, "Decrease size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonSmallerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonSmallerFont, "DialogBoxes.BlockNavigationControls.m_toolStripButtonSmallerFont");
			this.m_toolStripButtonSmallerFont.Name = "m_toolStripButtonSmallerFont";
			this.m_toolStripButtonSmallerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonSmallerFont.Text = "Decrease size of text";
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripLabelFilter, "DialogBoxes.BlockNavigationControls.m_toolStripLabelFilter");
			this.m_toolStripLabelFilter.Name = "m_toolStripLabelFilter";
			this.m_toolStripLabelFilter.Size = new System.Drawing.Size(36, 22);
			this.m_toolStripLabelFilter.Text = "Filter:";
			// 
			// m_toolStripComboBoxFilter
			// 
			this.m_toolStripComboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_toolStripComboBoxFilter.Items.AddRange(new object[] {
            "Verses with expected quotes",
            "All Scripture"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripComboBoxFilter, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripComboBoxFilter, "DialogBoxes.QuotationMarksDialog.m_toolStripComboBoxFilter");
			this.m_toolStripComboBoxFilter.Name = "m_toolStripComboBoxFilter";
			this.m_toolStripComboBoxFilter.Size = new System.Drawing.Size(225, 25);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// m_scriptureReference
			// 
			this.m_scriptureReference.BackColor = System.Drawing.Color.Transparent;
			this.m_scriptureReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_scriptureReference, "");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_scriptureReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_scriptureReference, L10NSharp.LocalizationPriority.Low);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_scriptureReference, "DialogBoxes.BlockNavigationControls.VerseControl");
			this.m_scriptureReference.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.m_scriptureReference.Name = "m_scriptureReference";
			this.m_scriptureReference.Size = new System.Drawing.Size(191, 23);
			// 
			// m_pnlDialogeQuotes
			// 
			this.m_pnlDialogeQuotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkAlternateSpeakersInFirstLevelQuotes);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkDialogueQuotations);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboEndQuotationDash);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_lblEndDialogueQuote);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboQuotationDash);
			this.m_pnlDialogeQuotes.Location = new System.Drawing.Point(0, 70);
			this.m_pnlDialogeQuotes.Name = "m_pnlDialogeQuotes";
			this.m_pnlDialogeQuotes.Size = new System.Drawing.Size(471, 269);
			this.m_pnlDialogeQuotes.TabIndex = 13;
			// 
			// m_splitContainer
			// 
			this.m_splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_splitContainer.Location = new System.Drawing.Point(20, 38);
			this.m_splitContainer.Name = "m_splitContainer";
			// 
			// m_splitContainer.Panel1
			// 
			this.m_splitContainer.Panel1.Controls.Add(this.m_tableBlocks);
			// 
			// m_splitContainer.Panel2
			// 
			this.m_splitContainer.Panel2.Controls.Add(this.m_lblQuotationMarks);
			this.m_splitContainer.Panel2.Controls.Add(this.m_pnlDialogeQuotes);
			this.m_splitContainer.Panel2.Controls.Add(this.m_comboQuoteMarks);
			this.m_splitContainer.Panel2.Controls.Add(this.m_lblHorizontalSeparator);
			this.m_splitContainer.Size = new System.Drawing.Size(737, 344);
			this.m_splitContainer.SplitterDistance = 260;
			this.m_splitContainer.TabIndex = 14;
			// 
			// m_tableBlocks
			// 
			this.m_tableBlocks.AccessibleDescription = "";
			this.m_tableBlocks.ColumnCount = 1;
			this.m_tableBlocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableBlocks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableBlocks.Controls.Add(this.m_panelContext, 0, 1);
			this.m_tableBlocks.Controls.Add(this.m_labelProjectData, 0, 0);
			this.m_tableBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableBlocks.Location = new System.Drawing.Point(0, 0);
			this.m_tableBlocks.Name = "m_tableBlocks";
			this.m_tableBlocks.RowCount = 2;
			this.m_tableBlocks.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableBlocks.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableBlocks.Size = new System.Drawing.Size(258, 342);
			this.m_tableBlocks.TabIndex = 12;
			// 
			// m_panelContext
			// 
			this.m_panelContext.Controls.Add(this.m_dataGridViewBlocks);
			this.m_panelContext.Controls.Add(this.m_blocksDisplayBrowser);
			this.m_panelContext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_panelContext.Location = new System.Drawing.Point(3, 21);
			this.m_panelContext.Name = "m_panelContext";
			this.m_panelContext.Size = new System.Drawing.Size(252, 318);
			this.m_panelContext.TabIndex = 30;
			// 
			// QuotationMarksDialog
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(775, 433);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_splitContainer);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "ProjectSettingsDialog.ProjectSettings");
			this.MinimumSize = new System.Drawing.Size(515, 275);
			this.Name = "QuotationMarksDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Quotation Marks";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridViewBlocks)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			this.m_pnlDialogeQuotes.ResumeLayout(false);
			this.m_pnlDialogeQuotes.PerformLayout();
			this.m_splitContainer.Panel1.ResumeLayout(false);
			this.m_splitContainer.Panel2.ResumeLayout(false);
			this.m_splitContainer.Panel2.PerformLayout();
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

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.ComboBox m_comboQuoteMarks;
		private System.Windows.Forms.Label m_lblQuotationMarks;
		private System.Windows.Forms.Label m_lblHorizontalSeparator;
		private System.Windows.Forms.CheckBox m_chkDialogueQuotations;
		private System.Windows.Forms.ComboBox m_cboQuotationDash;
		private System.Windows.Forms.ComboBox m_cboEndQuotationDash;
		private System.Windows.Forms.Label m_lblEndDialogueQuote;
		private System.Windows.Forms.Panel m_pnlDialogeQuotes;
		private System.Windows.Forms.CheckBox m_chkAlternateSpeakersInFirstLevelQuotes;
		private System.Windows.Forms.SplitContainer m_splitContainer;
		private System.Windows.Forms.TableLayoutPanel m_tableBlocks;
		private System.Windows.Forms.Panel m_panelContext;
		private System.Windows.Forms.DataGridView m_dataGridViewBlocks;
		private Controls.Browser m_blocksDisplayBrowser;
		private System.Windows.Forms.Label m_labelProjectData;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonHtmlView;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonGridView;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonLargerFont;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonSmallerFont;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFilter;
		private System.Windows.Forms.ToolStripComboBox m_toolStripComboBoxFilter;
		private Paratext.ToolStripVerseControl m_scriptureReference;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
		private System.Windows.Forms.DataGridViewTextBoxColumn colText;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
	}
}