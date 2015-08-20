using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	partial class QuotationMarksDlg
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				m_project.AnalysisCompleted -= HandleAnalysisCompleted;
				LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_comboQuoteMarks = new System.Windows.Forms.ComboBox();
			this.m_lblQuotationMarks = new System.Windows.Forms.Label();
			this.m_lblHorizontalSeparator2 = new System.Windows.Forms.Label();
			this.m_chkDialogueQuotations = new System.Windows.Forms.CheckBox();
			this.m_cboQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_cboEndQuotationDash = new System.Windows.Forms.ComboBox();
			this.m_lblEndDialogueQuote = new System.Windows.Forms.Label();
			this.m_chkAlternateSpeakersInFirstLevelQuotes = new System.Windows.Forms.CheckBox();
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
			this.m_btnPrevious = new System.Windows.Forms.Button();
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_labelXofY = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.m_lblStartDialogueQuote = new System.Windows.Forms.Label();
			this.m_lblLevel1 = new System.Windows.Forms.Label();
			this.m_lblLevel2 = new System.Windows.Forms.Label();
			this.m_lblLevel3 = new System.Windows.Forms.Label();
			this.m_lblBegin = new System.Windows.Forms.Label();
			this.m_lblContinue = new System.Windows.Forms.Label();
			this.m_lblEnd = new System.Windows.Forms.Label();
			this.m_cbLevel1Begin = new System.Windows.Forms.ComboBox();
			this.m_cbLevel1Continue = new System.Windows.Forms.ComboBox();
			this.m_cbLevel1End = new System.Windows.Forms.ComboBox();
			this.m_cbLevel2Begin = new System.Windows.Forms.ComboBox();
			this.m_cbLevel2Continue = new System.Windows.Forms.ComboBox();
			this.m_cbLevel2End = new System.Windows.Forms.ComboBox();
			this.m_cbLevel3Begin = new System.Windows.Forms.ComboBox();
			this.m_cbLevel3End = new System.Windows.Forms.ComboBox();
			this.m_cbLevel3Continue = new System.Windows.Forms.ComboBox();
			this.m_lblPrompt = new System.Windows.Forms.Label();
			this.m_lblHorizontalSeparator1 = new System.Windows.Forms.Label();
			this.m_blocksViewer = new Glyssen.Controls.ScriptBlocksViewer();
			this.m_pnlDialogeQuotes = new System.Windows.Forms.Panel();
			this.m_splitContainer = new System.Windows.Forms.SplitContainer();
			this.m_tableLayoutPanelDataBrowser = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlLevels = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			this.m_pnlDialogeQuotes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).BeginInit();
			this.m_splitContainer.Panel1.SuspendLayout();
			this.m_splitContainer.Panel2.SuspendLayout();
			this.m_splitContainer.SuspendLayout();
			this.m_tableLayoutPanelDataBrowser.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.m_pnlLevels.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.BackColor = System.Drawing.Color.Transparent;
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(717, 481);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 0;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = false;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.BackColor = System.Drawing.Color.Transparent;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(631, 481);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = false;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.QuotationMarksDlg";
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_comboQuoteMarks, "DialogBoxes.QuotationMarksDlg.m_comboQuoteMarks");
			this.m_comboQuoteMarks.Location = new System.Drawing.Point(264, 480);
			this.m_comboQuoteMarks.MaxDropDownItems = 15;
			this.m_comboQuoteMarks.Name = "m_comboQuoteMarks";
			this.m_comboQuoteMarks.Size = new System.Drawing.Size(284, 30);
			this.m_comboQuoteMarks.TabIndex = 1;
			this.m_comboQuoteMarks.Visible = false;
			this.m_comboQuoteMarks.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandlecomboQuoteMarksDrawItem);
			// 
			// m_lblQuotationMarks
			// 
			this.m_lblQuotationMarks.AutoSize = true;
			this.m_lblQuotationMarks.BackColor = System.Drawing.Color.Transparent;
			this.m_lblQuotationMarks.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuotationMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuotationMarks, "DialogBoxes.QuotationMarksDlg.FirstLevelQuotationMarks");
			this.m_lblQuotationMarks.Location = new System.Drawing.Point(72, 491);
			this.m_lblQuotationMarks.Name = "m_lblQuotationMarks";
			this.m_lblQuotationMarks.Size = new System.Drawing.Size(132, 13);
			this.m_lblQuotationMarks.TabIndex = 6;
			this.m_lblQuotationMarks.Text = "First-level quotation marks:";
			this.m_lblQuotationMarks.Visible = false;
			// 
			// m_lblHorizontalSeparator2
			// 
			this.m_lblHorizontalSeparator2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblHorizontalSeparator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblHorizontalSeparator2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblHorizontalSeparator2, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblHorizontalSeparator2, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblHorizontalSeparator2, "ProjectSettingsDlg.m_lblHorizontalSeparator2");
			this.m_lblHorizontalSeparator2.Location = new System.Drawing.Point(3, 206);
			this.m_lblHorizontalSeparator2.Name = "m_lblHorizontalSeparator2";
			this.m_lblHorizontalSeparator2.Size = new System.Drawing.Size(473, 2);
			this.m_lblHorizontalSeparator2.TabIndex = 8;
			// 
			// m_chkDialogueQuotations
			// 
			this.m_chkDialogueQuotations.AutoSize = true;
			this.m_chkDialogueQuotations.BackColor = System.Drawing.Color.Transparent;
			this.m_chkDialogueQuotations.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkDialogueQuotations, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkDialogueQuotations, "DialogBoxes.QuotationMarksDlg.ProjectMarksDialogue");
			this.m_chkDialogueQuotations.Location = new System.Drawing.Point(6, 11);
			this.m_chkDialogueQuotations.Name = "m_chkDialogueQuotations";
			this.m_chkDialogueQuotations.Size = new System.Drawing.Size(264, 17);
			this.m_chkDialogueQuotations.TabIndex = 11;
			this.m_chkDialogueQuotations.Text = "This project marks dialogue with quotation dashes.";
			this.m_chkDialogueQuotations.UseVisualStyleBackColor = false;
			this.m_chkDialogueQuotations.CheckedChanged += new System.EventHandler(this.m_chkDialogueQuotations_CheckedChanged);
			// 
			// m_cboQuotationDash
			// 
			this.m_cboQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboQuotationDash.Enabled = false;
			this.m_cboQuotationDash.FormattingEnabled = true;
			this.m_cboQuotationDash.Items.AddRange(new object[] {
            "Quotation dash (U+2015)",
            "Em-dash (U+2014)"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboQuotationDash, "DialogBoxes.QuotationMarksDlg.m_cboQuotationDash");
			this.m_cboQuotationDash.Location = new System.Drawing.Point(232, 40);
			this.m_cboQuotationDash.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.m_cboQuotationDash.Name = "m_cboQuotationDash";
			this.m_cboQuotationDash.Size = new System.Drawing.Size(235, 21);
			this.m_cboQuotationDash.TabIndex = 12;
			this.m_cboQuotationDash.TextChanged += new System.EventHandler(this.m_cboQuotationDash_TextChanged);
			// 
			// m_cboEndQuotationDash
			// 
			this.m_cboEndQuotationDash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cboEndQuotationDash.Enabled = false;
			this.m_cboEndQuotationDash.FormattingEnabled = true;
			this.m_cboEndQuotationDash.Items.AddRange(new object[] {
            "End of paragraph (only)",
            "#dash#",
            "Any punctuation mark"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cboEndQuotationDash, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cboEndQuotationDash, "DialogBoxes.QuotationMarksDlg.m_cboEndQuotationDash");
			this.m_cboEndQuotationDash.Location = new System.Drawing.Point(232, 72);
			this.m_cboEndQuotationDash.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.m_cboEndQuotationDash.Name = "m_cboEndQuotationDash";
			this.m_cboEndQuotationDash.Size = new System.Drawing.Size(235, 21);
			this.m_cboEndQuotationDash.TabIndex = 13;
			// 
			// m_lblEndDialogueQuote
			// 
			this.m_lblEndDialogueQuote.AutoSize = true;
			this.m_lblEndDialogueQuote.BackColor = System.Drawing.Color.Transparent;
			this.m_lblEndDialogueQuote.Enabled = false;
			this.m_lblEndDialogueQuote.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblEndDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblEndDialogueQuote, "DialogBoxes.QuotationMarksDlg.EndDialogueQuote");
			this.m_lblEndDialogueQuote.Location = new System.Drawing.Point(47, 75);
			this.m_lblEndDialogueQuote.Name = "m_lblEndDialogueQuote";
			this.m_lblEndDialogueQuote.Size = new System.Drawing.Size(134, 13);
			this.m_lblEndDialogueQuote.TabIndex = 12;
			this.m_lblEndDialogueQuote.Text = "Dialogue quotation ending:";
			// 
			// m_chkAlternateSpeakersInFirstLevelQuotes
			// 
			this.m_chkAlternateSpeakersInFirstLevelQuotes.AutoSize = true;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.BackColor = System.Drawing.Color.Transparent;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Enabled = false;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkAlternateSpeakersInFirstLevelQuotes, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkAlternateSpeakersInFirstLevelQuotes, "DialogBoxes.QuotationMarksDlg.m_chkAlternateSpeakersInFirstLevelQuotes");
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Location = new System.Drawing.Point(15, 136);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Name = "m_chkAlternateSpeakersInFirstLevelQuotes";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Size = new System.Drawing.Size(15, 14);
			this.m_chkAlternateSpeakersInFirstLevelQuotes.TabIndex = 13;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Tag = "\"Making this invisible until we can find a project that actually uses this. Not y" +
    "et implemented in QuoteParser.\"";
			this.m_chkAlternateSpeakersInFirstLevelQuotes.UseVisualStyleBackColor = false;
			this.m_chkAlternateSpeakersInFirstLevelQuotes.Visible = false;
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.QuotationMarksDlg.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Size = new System.Drawing.Size(810, 25);
			this.m_toolStrip.TabIndex = 32;
			// 
			// m_toolStripButtonHtmlView
			// 
			this.m_toolStripButtonHtmlView.Checked = true;
			this.m_toolStripButtonHtmlView.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_toolStripButtonHtmlView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonHtmlView.Image = global::Glyssen.Properties.Resources.html_view;
			this.m_toolStripButtonHtmlView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonHtmlView, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonHtmlView, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonHtmlView, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonHtmlView, "DialogBoxes.BlockNavigationControls.FormattedView");
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
			this.m_toolStripButtonGridView.Image = global::Glyssen.Properties.Resources.grid_icon;
			this.m_toolStripButtonGridView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonGridView, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonGridView, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonGridView, "DialogBoxes.BlockNavigationControls.GridView");
			this.m_toolStripButtonGridView.Name = "m_toolStripButtonGridView";
			this.m_toolStripButtonGridView.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonGridView.Text = "Grid view";
			this.m_toolStripButtonGridView.ToolTipText = "Left pane shows the highlighted block and surrounding context in a grid";
			this.m_toolStripButtonGridView.CheckedChanged += new System.EventHandler(this.HandleDataGridViewCheckChanged);
			this.m_toolStripButtonGridView.Click += new System.EventHandler(this.HandleViewTypeToolStripButtonClick);
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
			this.m_toolStripButtonLargerFont.Image = global::Glyssen.Properties.Resources.IncreaseSize;
			this.m_toolStripButtonLargerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonLargerFont, "Increase size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonLargerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonLargerFont, "DialogBoxes.BlockNavigationControls.IncreaseTextSize");
			this.m_toolStripButtonLargerFont.Name = "m_toolStripButtonLargerFont";
			this.m_toolStripButtonLargerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonLargerFont.Text = "Increase size of text";
			this.m_toolStripButtonLargerFont.Click += new System.EventHandler(this.IncreaseFont);
			// 
			// m_toolStripButtonSmallerFont
			// 
			this.m_toolStripButtonSmallerFont.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.m_toolStripButtonSmallerFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonSmallerFont.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_toolStripButtonSmallerFont.Image = global::Glyssen.Properties.Resources.DecreaseSize;
			this.m_toolStripButtonSmallerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonSmallerFont, "Decrease size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonSmallerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonSmallerFont, "DialogBoxes.BlockNavigationControls.DecreaseTextSize");
			this.m_toolStripButtonSmallerFont.Name = "m_toolStripButtonSmallerFont";
			this.m_toolStripButtonSmallerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonSmallerFont.Text = "Decrease size of text";
			this.m_toolStripButtonSmallerFont.Click += new System.EventHandler(this.DecreaseFont);
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
            "Verses with missing expected quotes",
            "All quotes",
            "All Scripture"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripComboBoxFilter, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripComboBoxFilter, "DialogBoxes.BlockNavigationControls.Filter");
			this.m_toolStripComboBoxFilter.Name = "m_toolStripComboBoxFilter";
			this.m_toolStripComboBoxFilter.Size = new System.Drawing.Size(225, 25);
			this.m_toolStripComboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.HandleFilterChanged);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// m_scriptureReference
			// 
			this.m_scriptureReference.BackColor = System.Drawing.Color.Transparent;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_scriptureReference, "");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_scriptureReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_scriptureReference, L10NSharp.LocalizationPriority.Low);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_scriptureReference, "DialogBoxes.BlockNavigationControls.VerseControl");
			this.m_scriptureReference.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.m_scriptureReference.Name = "m_scriptureReference";
			this.m_scriptureReference.Size = new System.Drawing.Size(191, 23);
			// 
			// m_btnPrevious
			// 
			this.m_btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnPrevious.BackColor = System.Drawing.Color.Transparent;
			this.m_btnPrevious.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnPrevious, "Common.Previous");
			this.m_btnPrevious.Location = new System.Drawing.Point(19, 393);
			this.m_btnPrevious.Name = "m_btnPrevious";
			this.m_btnPrevious.Size = new System.Drawing.Size(75, 23);
			this.m_btnPrevious.TabIndex = 14;
			this.m_btnPrevious.Text = "Previous";
			this.m_btnPrevious.UseVisualStyleBackColor = false;
			this.m_btnPrevious.Click += new System.EventHandler(this.m_btnPrevious_Click);
			// 
			// m_btnNext
			// 
			this.m_btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_btnNext.BackColor = System.Drawing.Color.Transparent;
			this.m_btnNext.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(170, 393);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 15;
			this.m_btnNext.Text = "Next";
			this.m_btnNext.UseVisualStyleBackColor = false;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_labelXofY
			// 
			this.m_labelXofY.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_labelXofY.BackColor = System.Drawing.Color.Transparent;
			this.m_labelXofY.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelXofY, "{0} is the current block number; {1} is the total number of blocks.");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelXofY, "DialogBoxes.AssignCharacterDlg.XofY");
			this.m_labelXofY.Location = new System.Drawing.Point(100, 396);
			this.m_labelXofY.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.m_labelXofY.Name = "m_labelXofY";
			this.m_labelXofY.Size = new System.Drawing.Size(64, 18);
			this.m_labelXofY.TabIndex = 12;
			this.m_labelXofY.Text = "{0} of {1}";
			this.m_labelXofY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label1, "DialogBoxes.QuotationMarksDlg.SwitchFirstLevel");
			this.label1.Location = new System.Drawing.Point(36, 133);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(417, 49);
			this.label1.TabIndex = 14;
			this.label1.Tag = "\"Making this invisible until we can find a project that actually uses this. Not y" +
    "et implemented in QuoteParser.\"";
			this.label1.Text = "Quotation dashes also indicate change of speaker within first-level quotation mar" +
    "ks.";
			this.label1.Visible = false;
			// 
			// m_lblStartDialogueQuote
			// 
			this.m_lblStartDialogueQuote.AutoSize = true;
			this.m_lblStartDialogueQuote.BackColor = System.Drawing.Color.Transparent;
			this.m_lblStartDialogueQuote.Enabled = false;
			this.m_lblStartDialogueQuote.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblStartDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblStartDialogueQuote, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblStartDialogueQuote, "DialogBoxes.QuotationMarksDlg.DialogueQuotationDash");
			this.m_lblStartDialogueQuote.Location = new System.Drawing.Point(47, 43);
			this.m_lblStartDialogueQuote.Name = "m_lblStartDialogueQuote";
			this.m_lblStartDialogueQuote.Size = new System.Drawing.Size(125, 13);
			this.m_lblStartDialogueQuote.TabIndex = 15;
			this.m_lblStartDialogueQuote.Text = "Dialogue quotation dash:";
			// 
			// m_lblLevel1
			// 
			this.m_lblLevel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLevel1.AutoSize = true;
			this.m_lblLevel1.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLevel1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLevel1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLevel1, "DialogBoxes.QuotationMarksDlg.Level1");
			this.m_lblLevel1.Location = new System.Drawing.Point(3, 41);
			this.m_lblLevel1.Name = "m_lblLevel1";
			this.m_lblLevel1.Size = new System.Drawing.Size(45, 13);
			this.m_lblLevel1.TabIndex = 17;
			this.m_lblLevel1.Text = "Level 1:";
			// 
			// m_lblLevel2
			// 
			this.m_lblLevel2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLevel2.AutoSize = true;
			this.m_lblLevel2.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLevel2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLevel2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLevel2, "DialogBoxes.QuotationMarksDlg.Level2");
			this.m_lblLevel2.Location = new System.Drawing.Point(3, 87);
			this.m_lblLevel2.Name = "m_lblLevel2";
			this.m_lblLevel2.Size = new System.Drawing.Size(45, 13);
			this.m_lblLevel2.TabIndex = 18;
			this.m_lblLevel2.Text = "Level 2:";
			// 
			// m_lblLevel3
			// 
			this.m_lblLevel3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLevel3.AutoSize = true;
			this.m_lblLevel3.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLevel3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLevel3, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLevel3, "DialogBoxes.QuotationMarksDlg.Level3");
			this.m_lblLevel3.Location = new System.Drawing.Point(3, 134);
			this.m_lblLevel3.Name = "m_lblLevel3";
			this.m_lblLevel3.Size = new System.Drawing.Size(45, 13);
			this.m_lblLevel3.TabIndex = 22;
			this.m_lblLevel3.Text = "Level 3:";
			// 
			// m_lblBegin
			// 
			this.m_lblBegin.AutoSize = true;
			this.m_lblBegin.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblBegin, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblBegin, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblBegin, "DialogBoxes.QuotationMarksDlg.Begin");
			this.m_lblBegin.Location = new System.Drawing.Point(103, 0);
			this.m_lblBegin.Name = "m_lblBegin";
			this.m_lblBegin.Size = new System.Drawing.Size(34, 13);
			this.m_lblBegin.TabIndex = 32;
			this.m_lblBegin.Text = "Begin";
			// 
			// m_lblContinue
			// 
			this.m_lblContinue.AutoSize = true;
			this.m_lblContinue.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblContinue, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblContinue, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblContinue, "DialogBoxes.QuotationMarksDlg.Continue");
			this.m_lblContinue.Location = new System.Drawing.Point(227, 0);
			this.m_lblContinue.Name = "m_lblContinue";
			this.m_lblContinue.Size = new System.Drawing.Size(49, 13);
			this.m_lblContinue.TabIndex = 33;
			this.m_lblContinue.Text = "Continue";
			// 
			// m_lblEnd
			// 
			this.m_lblEnd.AutoSize = true;
			this.m_lblEnd.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblEnd, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblEnd, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblEnd, "DialogBoxes.QuotationMarksDlg.End");
			this.m_lblEnd.Location = new System.Drawing.Point(351, 0);
			this.m_lblEnd.Name = "m_lblEnd";
			this.m_lblEnd.Size = new System.Drawing.Size(26, 13);
			this.m_lblEnd.TabIndex = 34;
			this.m_lblEnd.Text = "End";
			// 
			// m_cbLevel1Begin
			// 
			this.m_cbLevel1Begin.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel1Begin.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel1Begin, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel1Begin, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel1Begin, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel1Begin.Location = new System.Drawing.Point(103, 28);
			this.m_cbLevel1Begin.Name = "m_cbLevel1Begin";
			this.m_cbLevel1Begin.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel1Begin.TabIndex = 2;
			// 
			// m_cbLevel1Continue
			// 
			this.m_cbLevel1Continue.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel1Continue.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel1Continue, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel1Continue, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel1Continue, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox2");
			this.m_cbLevel1Continue.Location = new System.Drawing.Point(227, 28);
			this.m_cbLevel1Continue.Name = "m_cbLevel1Continue";
			this.m_cbLevel1Continue.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel1Continue.TabIndex = 3;
			// 
			// m_cbLevel1End
			// 
			this.m_cbLevel1End.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel1End.FormattingEnabled = true;
			this.m_cbLevel1End.ItemHeight = 29;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel1End, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel1End, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel1End, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox3");
			this.m_cbLevel1End.Location = new System.Drawing.Point(351, 28);
			this.m_cbLevel1End.Name = "m_cbLevel1End";
			this.m_cbLevel1End.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel1End.TabIndex = 4;
			// 
			// m_cbLevel2Begin
			// 
			this.m_cbLevel2Begin.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel2Begin.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel2Begin, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel2Begin, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel2Begin, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel2Begin.Location = new System.Drawing.Point(103, 74);
			this.m_cbLevel2Begin.Name = "m_cbLevel2Begin";
			this.m_cbLevel2Begin.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel2Begin.TabIndex = 5;
			// 
			// m_cbLevel2Continue
			// 
			this.m_cbLevel2Continue.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel2Continue.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel2Continue, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel2Continue, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel2Continue, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel2Continue.Location = new System.Drawing.Point(227, 74);
			this.m_cbLevel2Continue.Name = "m_cbLevel2Continue";
			this.m_cbLevel2Continue.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel2Continue.TabIndex = 6;
			// 
			// m_cbLevel2End
			// 
			this.m_cbLevel2End.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel2End.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel2End, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel2End, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel2End, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel2End.Location = new System.Drawing.Point(351, 74);
			this.m_cbLevel2End.Name = "m_cbLevel2End";
			this.m_cbLevel2End.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel2End.TabIndex = 7;
			// 
			// m_cbLevel3Begin
			// 
			this.m_cbLevel3Begin.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel3Begin.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel3Begin, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel3Begin, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel3Begin, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel3Begin.Location = new System.Drawing.Point(103, 120);
			this.m_cbLevel3Begin.Name = "m_cbLevel3Begin";
			this.m_cbLevel3Begin.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel3Begin.TabIndex = 8;
			// 
			// m_cbLevel3End
			// 
			this.m_cbLevel3End.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel3End.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel3End, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel3End, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel3End, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel3End.Location = new System.Drawing.Point(351, 120);
			this.m_cbLevel3End.Name = "m_cbLevel3End";
			this.m_cbLevel3End.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel3End.TabIndex = 10;
			// 
			// m_cbLevel3Continue
			// 
			this.m_cbLevel3Continue.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_cbLevel3Continue.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cbLevel3Continue, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cbLevel3Continue, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cbLevel3Continue, "DialogBoxes.QuotationMarksDlg.m_QuotationMarkComboBox1");
			this.m_cbLevel3Continue.Location = new System.Drawing.Point(227, 120);
			this.m_cbLevel3Continue.Name = "m_cbLevel3Continue";
			this.m_cbLevel3Continue.Size = new System.Drawing.Size(94, 37);
			this.m_cbLevel3Continue.TabIndex = 9;
			// 
			// m_lblPrompt
			// 
			this.m_lblPrompt.AutoSize = true;
			this.m_lblPrompt.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblPrompt, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblPrompt, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblPrompt, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblPrompt, "DialogBoxes.QuotationMarksDlg.label2");
			this.m_lblPrompt.Location = new System.Drawing.Point(3, 0);
			this.m_lblPrompt.Margin = new System.Windows.Forms.Padding(3, 0, 3, 15);
			this.m_lblPrompt.Name = "m_lblPrompt";
			this.m_lblPrompt.Size = new System.Drawing.Size(40, 13);
			this.m_lblPrompt.TabIndex = 19;
			this.m_lblPrompt.Text = "Prompt";
			// 
			// m_lblHorizontalSeparator1
			// 
			this.m_lblHorizontalSeparator1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblHorizontalSeparator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblHorizontalSeparator1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblHorizontalSeparator1, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblHorizontalSeparator1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblHorizontalSeparator1, "ProjectSettingsDlg.m_lblHorizontalSeparator1");
			this.m_lblHorizontalSeparator1.Location = new System.Drawing.Point(3, 28);
			this.m_lblHorizontalSeparator1.Name = "m_lblHorizontalSeparator1";
			this.m_lblHorizontalSeparator1.Size = new System.Drawing.Size(473, 2);
			this.m_lblHorizontalSeparator1.TabIndex = 20;
			// 
			// m_blocksViewer
			// 
			this.m_blocksViewer.BackColor = System.Drawing.Color.Transparent;
			this.m_tableLayoutPanelDataBrowser.SetColumnSpan(this.m_blocksViewer, 3);
			this.m_blocksViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksViewer, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksViewer, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksViewer, "DialogBoxes.QuotationMarksDlg.ScriptBlocksViewer");
			this.m_blocksViewer.Location = new System.Drawing.Point(3, 3);
			this.m_blocksViewer.Name = "m_blocksViewer";
			this.m_blocksViewer.Size = new System.Drawing.Size(258, 384);
			this.m_blocksViewer.TabIndex = 0;
			this.m_blocksViewer.Text = "Project Data:";
			this.m_blocksViewer.ViewType = Glyssen.Controls.ScriptBlocksViewType.Html;
			// 
			// m_pnlDialogeQuotes
			// 
			this.m_pnlDialogeQuotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlDialogeQuotes.Controls.Add(this.m_lblStartDialogueQuote);
			this.m_pnlDialogeQuotes.Controls.Add(this.label1);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkAlternateSpeakersInFirstLevelQuotes);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_chkDialogueQuotations);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboEndQuotationDash);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_lblEndDialogueQuote);
			this.m_pnlDialogeQuotes.Controls.Add(this.m_cboQuotationDash);
			this.m_pnlDialogeQuotes.Location = new System.Drawing.Point(3, 211);
			this.m_pnlDialogeQuotes.Name = "m_pnlDialogeQuotes";
			this.m_pnlDialogeQuotes.Size = new System.Drawing.Size(473, 197);
			this.m_pnlDialogeQuotes.TabIndex = 13;
			// 
			// m_splitContainer
			// 
			this.m_splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_splitContainer.Location = new System.Drawing.Point(20, 38);
			this.m_splitContainer.Name = "m_splitContainer";
			// 
			// m_splitContainer.Panel1
			// 
			this.m_splitContainer.Panel1.Controls.Add(this.m_tableLayoutPanelDataBrowser);
			this.m_splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(3);
			// 
			// m_splitContainer.Panel2
			// 
			this.m_splitContainer.Panel2.Controls.Add(this.tableLayoutPanel1);
			this.m_splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.m_splitContainer.Size = new System.Drawing.Size(772, 427);
			this.m_splitContainer.SplitterDistance = 272;
			this.m_splitContainer.TabIndex = 14;
			// 
			// m_tableLayoutPanelDataBrowser
			// 
			this.m_tableLayoutPanelDataBrowser.ColumnCount = 3;
			this.m_tableLayoutPanelDataBrowser.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelDataBrowser.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelDataBrowser.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelDataBrowser.Controls.Add(this.m_labelXofY, 1, 1);
			this.m_tableLayoutPanelDataBrowser.Controls.Add(this.m_btnPrevious, 0, 1);
			this.m_tableLayoutPanelDataBrowser.Controls.Add(this.m_blocksViewer, 0, 0);
			this.m_tableLayoutPanelDataBrowser.Controls.Add(this.m_btnNext, 2, 1);
			this.m_tableLayoutPanelDataBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableLayoutPanelDataBrowser.Location = new System.Drawing.Point(3, 3);
			this.m_tableLayoutPanelDataBrowser.Name = "m_tableLayoutPanelDataBrowser";
			this.m_tableLayoutPanelDataBrowser.RowCount = 2;
			this.m_tableLayoutPanelDataBrowser.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelDataBrowser.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelDataBrowser.Size = new System.Drawing.Size(264, 419);
			this.m_tableLayoutPanelDataBrowser.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_lblHorizontalSeparator1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_lblPrompt, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_lblHorizontalSeparator2, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.m_pnlDialogeQuotes, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.m_pnlLevels, 0, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 6);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(479, 411);
			this.tableLayoutPanel1.TabIndex = 20;
			// 
			// m_pnlLevels
			// 
			this.m_pnlLevels.ColumnCount = 4;
			this.m_pnlLevels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.m_pnlLevels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.Controls.Add(this.m_lblBegin, 1, 0);
			this.m_pnlLevels.Controls.Add(this.m_lblLevel2, 0, 2);
			this.m_pnlLevels.Controls.Add(this.m_lblLevel1, 0, 1);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel1Begin, 1, 1);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel1Continue, 2, 1);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel1End, 3, 1);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel2Begin, 1, 2);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel2Continue, 2, 2);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel2End, 3, 2);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel3Begin, 1, 3);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel3End, 3, 3);
			this.m_pnlLevels.Controls.Add(this.m_cbLevel3Continue, 1, 3);
			this.m_pnlLevels.Controls.Add(this.m_lblLevel3, 0, 3);
			this.m_pnlLevels.Controls.Add(this.m_lblContinue, 2, 0);
			this.m_pnlLevels.Controls.Add(this.m_lblEnd, 3, 0);
			this.m_pnlLevels.Location = new System.Drawing.Point(3, 33);
			this.m_pnlLevels.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.m_pnlLevels.Name = "m_pnlLevels";
			this.m_pnlLevels.RowCount = 4;
			this.m_pnlLevels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.m_pnlLevels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.m_pnlLevels.Size = new System.Drawing.Size(473, 165);
			this.m_pnlLevels.TabIndex = 18;
			// 
			// QuotationMarksDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(810, 516);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_lblQuotationMarks);
			this.Controls.Add(this.m_splitContainer);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_comboQuoteMarks);
			this.Controls.Add(this.m_btnCancel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.QuotationMarksDlg.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(826, 471);
			this.Name = "QuotationMarksDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Quote Mark Settings";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			this.m_pnlDialogeQuotes.ResumeLayout(false);
			this.m_pnlDialogeQuotes.PerformLayout();
			this.m_splitContainer.Panel1.ResumeLayout(false);
			this.m_splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).EndInit();
			this.m_splitContainer.ResumeLayout(false);
			this.m_tableLayoutPanelDataBrowser.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.m_pnlLevels.ResumeLayout(false);
			this.m_pnlLevels.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.ComboBox m_comboQuoteMarks;
		private System.Windows.Forms.Label m_lblQuotationMarks;
		private System.Windows.Forms.Label m_lblHorizontalSeparator2;
		private System.Windows.Forms.CheckBox m_chkDialogueQuotations;
		private System.Windows.Forms.ComboBox m_cboQuotationDash;
		private System.Windows.Forms.ComboBox m_cboEndQuotationDash;
		private System.Windows.Forms.Label m_lblEndDialogueQuote;
		private System.Windows.Forms.Panel m_pnlDialogeQuotes;
		private System.Windows.Forms.CheckBox m_chkAlternateSpeakersInFirstLevelQuotes;
		private System.Windows.Forms.SplitContainer m_splitContainer;
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
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private Controls.ScriptBlocksViewer m_blocksViewer;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelDataBrowser;
		private System.Windows.Forms.Button m_btnPrevious;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Label m_labelXofY;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label m_lblStartDialogueQuote;
		private System.Windows.Forms.Label m_lblLevel1;
		private System.Windows.Forms.TableLayoutPanel m_pnlLevels;
		private System.Windows.Forms.Label m_lblLevel3;
		private System.Windows.Forms.Label m_lblLevel2;
		private System.Windows.Forms.ComboBox m_cbLevel1Begin;
		private System.Windows.Forms.ComboBox m_cbLevel1Continue;
		private System.Windows.Forms.ComboBox m_cbLevel1End;
		private System.Windows.Forms.ComboBox m_cbLevel2Begin;
		private System.Windows.Forms.ComboBox m_cbLevel2Continue;
		private System.Windows.Forms.ComboBox m_cbLevel2End;
		private System.Windows.Forms.ComboBox m_cbLevel3Continue;
		private System.Windows.Forms.ComboBox m_cbLevel3Begin;
		private System.Windows.Forms.ComboBox m_cbLevel3End;
		private System.Windows.Forms.Label m_lblBegin;
		private System.Windows.Forms.Label m_lblContinue;
		private System.Windows.Forms.Label m_lblEnd;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label m_lblHorizontalSeparator1;
		private System.Windows.Forms.Label m_lblPrompt;
	}
}