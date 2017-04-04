using System;
using Glyssen.Controls;
using Glyssen.Utilities;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	partial class AssignCharacterDlg
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
				m_dataGridReferenceText.DataError -= HandleDataGridViewDataError;

				m_viewModel.CurrentBlockChanged -= LoadBlock;
				LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
				m_viewModel.AssignedBlocksIncremented -= m_viewModel_AssignedBlocksIncremented;
				m_viewModel.CurrentBlockMatchupChanged -= LoadBlockMatchup;
				m_viewModel.UiFontSizeChanged -= SetFontsFromViewModel;
				m_viewModel.CurrentBookSaved -= UpdateSavedText;
				m_viewModel.FilterReset -= HandleFilterReset;

				m_blocksViewer.MinimumWidthChanged -= BlocksViewerOnMinimumWidthChanged;
				m_blocksViewer.VisibleChanged -= BlocksViewerVisibleChanged;

				if (m_primaryReferenceTextFont != null)
				{
					m_primaryReferenceTextFont.Dispose();
					m_primaryReferenceTextFont = null;
				}
				if (m_englishReferenceTextFont != null)
				{
					m_englishReferenceTextFont.Dispose();
					m_englishReferenceTextFont = null;
				}

				if (components != null)
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnNext = new System.Windows.Forms.Button();
			this.m_btnPrevious = new System.Windows.Forms.Button();
			this.m_btnAssign = new System.Windows.Forms.Button();
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
			this.m_toolStripButtonSelectCharacter = new System.Windows.Forms.ToolStripButton();
			this.m_toolStripButtonMatchReferenceText = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripButtonLargerFont = new System.Windows.Forms.ToolStripButton();
			this.m_toolStripButtonSmallerFont = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripComboBoxFilter = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_scriptureReference = new Paratext.ToolStripVerseControl();
			this.m_menuBtnSplitBlock = new System.Windows.Forms.ToolStripButton();
			this.m_chkSingleVoice = new System.Windows.Forms.CheckBox();
			this.m_llClose = new System.Windows.Forms.LinkLabel();
			this.m_blocksViewer = new Glyssen.Controls.ScriptBlocksViewer();
			this.m_progressBar = new Glyssen.Controls.BlockProgressBar();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.tabPageSelectCharacter = new System.Windows.Forms.TabPage();
			this.m_pnlCharacterAndDeliverySelection = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlDeliveryFilter = new System.Windows.Forms.Panel();
			this.tableLayoutPanelDelivery = new System.Windows.Forms.TableLayoutPanel();
			this.m_pnlShortcuts = new System.Windows.Forms.TableLayoutPanel();
			this.m_listBoxDeliveries = new System.Windows.Forms.ListBox();
			this.m_pnlCharacterFilter = new System.Windows.Forms.Panel();
			this.tableLayoutPanelCharacter = new System.Windows.Forms.TableLayoutPanel();
			this.m_listBoxCharacters = new System.Windows.Forms.ListBox();
			this.tabPageMatchReferenceText = new System.Windows.Forms.TabPage();
			this.m_dataGridReferenceText = new System.Windows.Forms.DataGridView();
			this.colCharacter = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.m_contextMenuCharacterOrDeliveryCell = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ContextMenuItemAddCharacterOrDelivery = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.m_CharacterOrDeliveryContextMenuItemMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_CharacterOrDeliveryContextMenuItemMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.colPrimary = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_contextMenuRefTextCell = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ContextMenuItemSplitText = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ContextMenuItemInsertHeSaid = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.m_RefTextContextMenuItemMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_RefTextContextMenuItemMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.colEnglish = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colDelivery = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.m_tableLayoutPanelMatchReferenceTextButtons = new System.Windows.Forms.TableLayoutPanel();
			this.m_btnApplyReferenceTextMatches = new System.Windows.Forms.Button();
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnMoveReferenceTextUp = new System.Windows.Forms.Button();
			this.m_btnMoveReferenceTextDown = new System.Windows.Forms.Button();
			this.m_toolStripMatchReferenceText = new System.Windows.Forms.ToolStrip();
			this.m_btnInsertHeSaid = new System.Windows.Forms.ToolStripSplitButton();
			this.m_menuInsertIntoSelectedRowOnly = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuInsertIntoAllEmptyCells = new System.Windows.Forms.ToolStripMenuItem();
			this.m_btnReset = new System.Windows.Forms.Button();
			this.m_lblReferenceText = new System.Windows.Forms.Label();
			this.m_splitContainer = new System.Windows.Forms.SplitContainer();
			this.m_tabControlCharacterSelection = new System.Windows.Forms.TabControl();
			this.tableLayoutPanelNavigationControls = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			this.tabPageSelectCharacter.SuspendLayout();
			this.m_pnlCharacterAndDeliverySelection.SuspendLayout();
			this.m_pnlDeliveryFilter.SuspendLayout();
			this.tableLayoutPanelDelivery.SuspendLayout();
			this.m_pnlShortcuts.SuspendLayout();
			this.m_pnlCharacterFilter.SuspendLayout();
			this.tableLayoutPanelCharacter.SuspendLayout();
			this.tabPageMatchReferenceText.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridReferenceText)).BeginInit();
			this.m_contextMenuCharacterOrDeliveryCell.SuspendLayout();
			this.m_contextMenuRefTextCell.SuspendLayout();
			this.m_tableLayoutPanelMatchReferenceTextButtons.SuspendLayout();
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.SuspendLayout();
			this.m_toolStripMatchReferenceText.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).BeginInit();
			this.m_splitContainer.Panel1.SuspendLayout();
			this.m_splitContainer.Panel2.SuspendLayout();
			this.m_splitContainer.SuspendLayout();
			this.m_tabControlCharacterSelection.SuspendLayout();
			this.tableLayoutPanelNavigationControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.AssignCharacterDlg";
			// 
			// m_btnNext
			// 
			this.m_btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnNext, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnNext.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnNext, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnNext, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnNext, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnNext, "Common.Next");
			this.m_btnNext.Location = new System.Drawing.Point(208, 4);
			this.m_btnNext.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_btnNext.Name = "m_btnNext";
			this.m_btnNext.Size = new System.Drawing.Size(75, 23);
			this.m_btnNext.TabIndex = 4;
			this.m_btnNext.Text = "Next";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnNext, false);
			this.m_btnNext.UseVisualStyleBackColor = true;
			this.m_btnNext.Click += new System.EventHandler(this.m_btnNext_Click);
			// 
			// m_btnPrevious
			// 
			this.m_btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnPrevious, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnPrevious.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnPrevious, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnPrevious, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnPrevious, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnPrevious, "Common.Previous");
			this.m_btnPrevious.Location = new System.Drawing.Point(31, 4);
			this.m_btnPrevious.Name = "m_btnPrevious";
			this.m_btnPrevious.Size = new System.Drawing.Size(75, 23);
			this.m_btnPrevious.TabIndex = 3;
			this.m_btnPrevious.Text = "Previous";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnPrevious, false);
			this.m_btnPrevious.UseVisualStyleBackColor = true;
			this.m_btnPrevious.Click += new System.EventHandler(this.m_btnPrevious_Click);
			// 
			// m_btnAssign
			// 
			this.m_btnAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnAssign, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_btnAssign, 2);
			this.m_btnAssign.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnAssign, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnAssign, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAssign, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAssign, "DialogBoxes.AssignCharacterDlg.AssignCharacter");
			this.m_btnAssign.Location = new System.Drawing.Point(315, 364);
			this.m_btnAssign.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(95, 23);
			this.m_btnAssign.TabIndex = 2;
			this.m_btnAssign.Text = "Assign Character";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAssign, false);
			this.m_btnAssign.UseVisualStyleBackColor = true;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_labelWhoSpeaks
			// 
			this.m_labelWhoSpeaks.AutoSize = true;
			this.m_labelWhoSpeaks.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelWhoSpeaks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelWhoSpeaks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelWhoSpeaks.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelWhoSpeaks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelWhoSpeaks, "DialogBoxes.AssignCharacterDlg.WhoSpeaks");
			this.m_labelWhoSpeaks.Location = new System.Drawing.Point(3, 0);
			this.m_labelWhoSpeaks.Name = "m_labelWhoSpeaks";
			this.m_labelWhoSpeaks.Size = new System.Drawing.Size(156, 18);
			this.m_labelWhoSpeaks.TabIndex = 10;
			this.m_labelWhoSpeaks.Text = "Who speaks this part?";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelWhoSpeaks, true);
			// 
			// m_labelXofY
			// 
			this.m_labelXofY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_labelXofY.AutoSize = true;
			this.m_labelXofY.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_labelXofY, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_labelXofY, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_labelXofY.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_labelXofY, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_labelXofY, "{0} is the current block number; {1} is the total number of blocks.");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_labelXofY, "DialogBoxes.AssignCharacterDlg.XofY");
			this.m_labelXofY.Location = new System.Drawing.Point(112, 2);
			this.m_labelXofY.MinimumSize = new System.Drawing.Size(90, 28);
			this.m_labelXofY.Name = "m_labelXofY";
			this.m_labelXofY.Size = new System.Drawing.Size(90, 28);
			this.m_labelXofY.TabIndex = 11;
			this.m_labelXofY.Text = "{0} of {1}";
			this.m_labelXofY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_labelXofY, true);
			// 
			// m_llMoreChar
			// 
			this.m_llMoreChar.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_llMoreChar.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_llMoreChar.BackColor = System.Drawing.SystemColors.Control;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_llMoreChar, 2);
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_llMoreChar.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_llMoreChar.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_llMoreChar.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llMoreChar, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llMoreChar, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llMoreChar, "DialogBoxes.AssignCharacterDlg.MoreCharacters");
			this.m_llMoreChar.Location = new System.Drawing.Point(17, 189);
			this.m_llMoreChar.Name = "m_llMoreChar";
			this.m_llMoreChar.Size = new System.Drawing.Size(85, 13);
			this.m_llMoreChar.TabIndex = 13;
			this.m_llMoreChar.TabStop = true;
			this.m_llMoreChar.Text = "More Characters";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_llMoreChar, true);
			this.m_llMoreChar.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_llMoreChar, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_llMoreChar.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llMoreChar_LinkClicked);
			// 
			// m_txtCharacterFilter
			// 
			this.m_txtCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_txtCharacterFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_txtCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.glyssenColorPalette.SetForeColor(this.m_txtCharacterFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtCharacterFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtCharacterFilter, "DialogBoxes.AssignCharacterDlg.m_txtCharacterFilter");
			this.m_txtCharacterFilter.Location = new System.Drawing.Point(0, 2);
			this.m_txtCharacterFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtCharacterFilter.Name = "m_txtCharacterFilter";
			this.m_txtCharacterFilter.Size = new System.Drawing.Size(348, 13);
			this.m_txtCharacterFilter.TabIndex = 14;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtCharacterFilter, false);
			this.m_txtCharacterFilter.TextChanged += new System.EventHandler(this.m_txtCharacterFilter_TextChanged);
			// 
			// m_lblCharacter
			// 
			this.m_lblCharacter.AutoSize = true;
			this.m_lblCharacter.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_lblCharacter, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblCharacter.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblCharacter, "DialogBoxes.AssignCharacterDlg.Character");
			this.m_lblCharacter.Location = new System.Drawing.Point(17, 0);
			this.m_lblCharacter.Name = "m_lblCharacter";
			this.m_lblCharacter.Padding = new System.Windows.Forms.Padding(0, 13, 0, 0);
			this.m_lblCharacter.Size = new System.Drawing.Size(53, 26);
			this.m_lblCharacter.TabIndex = 16;
			this.m_lblCharacter.Text = "Character";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblCharacter, true);
			// 
			// m_lblDelivery
			// 
			this.m_lblDelivery.AutoSize = true;
			this.m_lblDelivery.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblDelivery, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_lblDelivery, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblDelivery, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblDelivery.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblDelivery, "DialogBoxes.AssignCharacterDlg.Delivery");
			this.m_lblDelivery.Location = new System.Drawing.Point(17, 202);
			this.m_lblDelivery.Name = "m_lblDelivery";
			this.m_lblDelivery.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.m_lblDelivery.Size = new System.Drawing.Size(45, 23);
			this.m_lblDelivery.TabIndex = 17;
			this.m_lblDelivery.Text = "Delivery";
			this.m_lblDelivery.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblDelivery, true);
			// 
			// m_llMoreDel
			// 
			this.m_llMoreDel.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_llMoreDel.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_llMoreDel.BackColor = System.Drawing.SystemColors.Control;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_llMoreDel, 2);
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_llMoreDel.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_llMoreDel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_llMoreDel.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llMoreDel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llMoreDel, "DialogBoxes.AssignCharacterDlg.MoreDeliveries");
			this.m_llMoreDel.Location = new System.Drawing.Point(17, 348);
			this.m_llMoreDel.Name = "m_llMoreDel";
			this.m_llMoreDel.Size = new System.Drawing.Size(80, 13);
			this.m_llMoreDel.TabIndex = 18;
			this.m_llMoreDel.TabStop = true;
			this.m_llMoreDel.Text = "More Deliveries";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_llMoreDel, true);
			this.m_llMoreDel.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_llMoreDel, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_llMoreDel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llMoreDel_LinkClicked);
			// 
			// m_txtDeliveryFilter
			// 
			this.m_txtDeliveryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_txtDeliveryFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_txtDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.glyssenColorPalette.SetForeColor(this.m_txtDeliveryFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtDeliveryFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtDeliveryFilter, "DialogBoxes.AssignCharacterDlg.m_txtDeliveryFilter");
			this.m_txtDeliveryFilter.Location = new System.Drawing.Point(0, 2);
			this.m_txtDeliveryFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_txtDeliveryFilter.Name = "m_txtDeliveryFilter";
			this.m_txtDeliveryFilter.Size = new System.Drawing.Size(348, 13);
			this.m_txtDeliveryFilter.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtDeliveryFilter, false);
			this.m_txtDeliveryFilter.TextChanged += new System.EventHandler(this.m_txtDeliveryFilter_TextChanged);
			// 
			// m_icnCharacterFilter
			// 
			this.m_icnCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_icnCharacterFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_icnCharacterFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.m_icnCharacterFilter.ForeColor = System.Drawing.Color.White;
			this.glyssenColorPalette.SetForeColor(this.m_icnCharacterFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_icnCharacterFilter.Image = global::Glyssen.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnCharacterFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnCharacterFilter, "DialogBoxes.AssignCharacterDlg.pictureBox1");
			this.m_icnCharacterFilter.Location = new System.Drawing.Point(348, 0);
			this.m_icnCharacterFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_icnCharacterFilter.Name = "m_icnCharacterFilter";
			this.m_icnCharacterFilter.Size = new System.Drawing.Size(18, 17);
			this.m_icnCharacterFilter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.m_icnCharacterFilter.TabIndex = 22;
			this.m_icnCharacterFilter.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_icnCharacterFilter, false);
			this.m_icnCharacterFilter.Click += new System.EventHandler(this.m_icnCharacterFilter_Click);
			// 
			// m_icnDeliveryFilter
			// 
			this.m_icnDeliveryFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_icnDeliveryFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_icnDeliveryFilter.ForeColor = System.Drawing.Color.White;
			this.glyssenColorPalette.SetForeColor(this.m_icnDeliveryFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_icnDeliveryFilter.Image = global::Glyssen.Properties.Resources.search_glyph;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_icnDeliveryFilter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_icnDeliveryFilter, "DialogBoxes.AssignCharacterDlg.pictureBox1");
			this.m_icnDeliveryFilter.Location = new System.Drawing.Point(348, 0);
			this.m_icnDeliveryFilter.Margin = new System.Windows.Forms.Padding(0);
			this.m_icnDeliveryFilter.Name = "m_icnDeliveryFilter";
			this.m_icnDeliveryFilter.Size = new System.Drawing.Size(18, 17);
			this.m_icnDeliveryFilter.TabIndex = 22;
			this.m_icnDeliveryFilter.TabStop = false;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_icnDeliveryFilter, false);
			this.m_icnDeliveryFilter.Click += new System.EventHandler(this.m_icnDeliveryFilter_Click);
			// 
			// m_btnAddCharacter
			// 
			this.m_btnAddCharacter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_btnAddCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAddCharacter.BackColor = System.Drawing.SystemColors.Control;
			this.m_btnAddCharacter.BackgroundImage = global::Glyssen.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_btnAddCharacter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.m_btnAddCharacter.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
			this.m_btnAddCharacter.FlatAppearance.BorderSize = 2;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnAddCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAddCharacter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.m_btnAddCharacter.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_btnAddCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAddCharacter, "Add New Character");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAddCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAddCharacter, "DialogBoxes.AssignCharacterDlg.AddCharacter");
			this.m_btnAddCharacter.Location = new System.Drawing.Point(390, 29);
			this.m_btnAddCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnAddCharacter.Name = "m_btnAddCharacter";
			this.m_btnAddCharacter.Size = new System.Drawing.Size(20, 20);
			this.m_btnAddCharacter.TabIndex = 24;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAddCharacter, true);
			this.m_btnAddCharacter.UseVisualStyleBackColor = false;
			this.m_btnAddCharacter.Click += new System.EventHandler(this.m_btnAddCharacter_Click);
			// 
			// m_btnAddDelivery
			// 
			this.m_btnAddDelivery.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.glyssenColorPalette.SetBackColor(this.m_btnAddDelivery, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAddDelivery.BackColor = System.Drawing.SystemColors.Control;
			this.m_btnAddDelivery.BackgroundImage = global::Glyssen.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_btnAddDelivery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.m_btnAddDelivery.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
			this.m_btnAddDelivery.FlatAppearance.BorderSize = 2;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnAddDelivery, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnAddDelivery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.m_btnAddDelivery.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_btnAddDelivery, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAddDelivery, "Add New Delivery");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAddDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAddDelivery, "DialogBoxes.AssignCharacterDlg.AddDelivery");
			this.m_btnAddDelivery.Location = new System.Drawing.Point(390, 228);
			this.m_btnAddDelivery.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnAddDelivery.Name = "m_btnAddDelivery";
			this.m_btnAddDelivery.Size = new System.Drawing.Size(20, 20);
			this.m_btnAddDelivery.TabIndex = 25;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAddDelivery, true);
			this.m_btnAddDelivery.UseVisualStyleBackColor = false;
			this.m_btnAddDelivery.Click += new System.EventHandler(this.m_btnAddDelivery_Click);
			// 
			// m_lblShortcut1
			// 
			this.m_lblShortcut1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblShortcut1.AutoSize = true;
			this.m_lblShortcut1.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblShortcut1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblShortcut1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblShortcut1, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblShortcut1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut1, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut1, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblShortcut1.Location = new System.Drawing.Point(0, 2);
			this.m_lblShortcut1.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.m_lblShortcut1.Name = "m_lblShortcut1";
			this.m_lblShortcut1.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut1.TabIndex = 26;
			this.m_lblShortcut1.Text = "1";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblShortcut1, true);
			// 
			// m_lblShortcut2
			// 
			this.m_lblShortcut2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblShortcut2.AutoSize = true;
			this.m_lblShortcut2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblShortcut2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblShortcut2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblShortcut2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblShortcut2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut2, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut2, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut2, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblShortcut2.Location = new System.Drawing.Point(0, 19);
			this.m_lblShortcut2.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.m_lblShortcut2.Name = "m_lblShortcut2";
			this.m_lblShortcut2.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut2.TabIndex = 27;
			this.m_lblShortcut2.Text = "2";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblShortcut2, true);
			// 
			// m_lblShortcut3
			// 
			this.m_lblShortcut3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblShortcut3.AutoSize = true;
			this.m_lblShortcut3.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblShortcut3, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblShortcut3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblShortcut3, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblShortcut3.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut3, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut3, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut3, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut3, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblShortcut3.Location = new System.Drawing.Point(0, 36);
			this.m_lblShortcut3.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.m_lblShortcut3.Name = "m_lblShortcut3";
			this.m_lblShortcut3.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut3.TabIndex = 28;
			this.m_lblShortcut3.Text = "3";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblShortcut3, true);
			// 
			// m_lblShortcut4
			// 
			this.m_lblShortcut4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblShortcut4.AutoSize = true;
			this.m_lblShortcut4.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblShortcut4, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblShortcut4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblShortcut4, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblShortcut4.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut4, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut4, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut4, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut4, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblShortcut4.Location = new System.Drawing.Point(0, 53);
			this.m_lblShortcut4.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.m_lblShortcut4.Name = "m_lblShortcut4";
			this.m_lblShortcut4.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut4.TabIndex = 29;
			this.m_lblShortcut4.Text = "4";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblShortcut4, true);
			// 
			// m_lblShortcut5
			// 
			this.m_lblShortcut5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblShortcut5.AutoSize = true;
			this.m_lblShortcut5.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblShortcut5, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblShortcut5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblShortcut5, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblShortcut5.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblShortcut5, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblShortcut5, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblShortcut5, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblShortcut5, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblShortcut5.Location = new System.Drawing.Point(0, 71);
			this.m_lblShortcut5.Margin = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.m_lblShortcut5.Name = "m_lblShortcut5";
			this.m_lblShortcut5.Size = new System.Drawing.Size(10, 12);
			this.m_lblShortcut5.TabIndex = 30;
			this.m_lblShortcut5.Text = "5";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblShortcut5, true);
			// 
			// m_toolStrip
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.Default);
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_toolStripButtonSelectCharacter,
            this.m_toolStripButtonMatchReferenceText,
            this.toolStripSeparator4,
            this.m_toolStripButtonLargerFont,
            this.m_toolStripButtonSmallerFont,
            this.toolStripSeparator1,
            this.m_toolStripLabelFilter,
            this.m_toolStripComboBoxFilter,
            this.toolStripSeparator3,
            this.m_scriptureReference,
            this.m_menuBtnSplitBlock});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.AssignCharacterDlg.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Size = new System.Drawing.Size(875, 25);
			this.m_toolStrip.TabIndex = 31;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStrip, false);
			// 
			// m_toolStripButtonSelectCharacter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonSelectCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripButtonSelectCharacter.Checked = true;
			this.m_toolStripButtonSelectCharacter.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_toolStripButtonSelectCharacter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonSelectCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripButtonSelectCharacter.Image = global::Glyssen.Properties.Resources.WhoSpeaks;
			this.m_toolStripButtonSelectCharacter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonSelectCharacter, "Select Character (Who speaks this part?)");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonSelectCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonSelectCharacter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonSelectCharacter, "DialogBoxes.AssignCharacterDlg.m_toolStripButtonSelectCharacter");
			this.m_toolStripButtonSelectCharacter.Name = "m_toolStripButtonSelectCharacter";
			this.m_toolStripButtonSelectCharacter.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonSelectCharacter.Text = "Select Character";
			this.m_toolStripButtonSelectCharacter.ToolTipText = "Select Character (Who speaks this part?)";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonSelectCharacter, false);
			this.m_toolStripButtonSelectCharacter.CheckedChanged += new System.EventHandler(this.HandleSelectCharacterCheckChanged);
			this.m_toolStripButtonSelectCharacter.Click += new System.EventHandler(this.HandleTaskToolStripButtonClick);
			// 
			// m_toolStripButtonMatchReferenceText
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonMatchReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripButtonMatchReferenceText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonMatchReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripButtonMatchReferenceText.Image = global::Glyssen.Properties.Resources.rainbow;
			this.m_toolStripButtonMatchReferenceText.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripButtonMatchReferenceText, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonMatchReferenceText, "DialogBoxes.AssignCharacterDlg.m_toolStripButtonMatchReferenceText");
			this.m_toolStripButtonMatchReferenceText.Name = "m_toolStripButtonMatchReferenceText";
			this.m_toolStripButtonMatchReferenceText.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonMatchReferenceText.Text = "Match Reference Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonMatchReferenceText, false);
			this.m_toolStripButtonMatchReferenceText.CheckedChanged += new System.EventHandler(this.HandleMatchReferenceTextCheckChanged);
			this.m_toolStripButtonMatchReferenceText.Click += new System.EventHandler(this.HandleTaskToolStripButtonClick);
			// 
			// toolStripSeparator4
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator4, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator4, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator4, false);
			// 
			// m_toolStripButtonLargerFont
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonLargerFont, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripButtonLargerFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonLargerFont.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonLargerFont, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripButtonLargerFont.Image = global::Glyssen.Properties.Resources.IncreaseSize;
			this.m_toolStripButtonLargerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonLargerFont, "Increase size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonLargerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonLargerFont, "DialogBoxes.BlockNavigationControls.IncreaseTextSize");
			this.m_toolStripButtonLargerFont.Name = "m_toolStripButtonLargerFont";
			this.m_toolStripButtonLargerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonLargerFont.Text = "Increase size of text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonLargerFont, false);
			this.m_toolStripButtonLargerFont.Click += new System.EventHandler(this.IncreaseFont);
			// 
			// m_toolStripButtonSmallerFont
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonSmallerFont, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripButtonSmallerFont.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.m_toolStripButtonSmallerFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonSmallerFont.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonSmallerFont, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripButtonSmallerFont.Image = global::Glyssen.Properties.Resources.DecreaseSize;
			this.m_toolStripButtonSmallerFont.ImageTransparentColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonSmallerFont, "Decrease size of text");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonSmallerFont, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonSmallerFont, "DialogBoxes.BlockNavigationControls.DecreaseTextSize");
			this.m_toolStripButtonSmallerFont.Name = "m_toolStripButtonSmallerFont";
			this.m_toolStripButtonSmallerFont.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonSmallerFont.Text = "Decrease size of text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonSmallerFont, false);
			this.m_toolStripButtonSmallerFont.Click += new System.EventHandler(this.DecreaseFont);
			// 
			// toolStripSeparator1
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.toolStripSeparator1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator1, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator1, false);
			// 
			// m_toolStripLabelFilter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripLabelFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStripLabelFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripLabelFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripLabelFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripLabelFilter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripLabelFilter, "DialogBoxes.BlockNavigationControls.m_toolStripLabelFilter");
			this.m_toolStripLabelFilter.Name = "m_toolStripLabelFilter";
			this.m_toolStripLabelFilter.Size = new System.Drawing.Size(36, 22);
			this.m_toolStripLabelFilter.Text = "Filter:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripLabelFilter, false);
			// 
			// m_toolStripComboBoxFilter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripComboBoxFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripComboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.glyssenColorPalette.SetForeColor(this.m_toolStripComboBoxFilter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripComboBoxFilter.Items.AddRange(new object[] {
            "Unassigned quotes",
            "All quotes not assigned automatically",
            "Verses with missing expected quotes",
            "More quotes than expected in verse",
            "Verses with expected quotes",
            "All quotes",
            "All Scripture",
            "Verses not aligned with reference text"});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripComboBoxFilter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripComboBoxFilter, L10NSharp.LocalizationPriority.MediumHigh);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripComboBoxFilter, "DialogBoxes.BlockNavigationControls.Filter");
			this.m_toolStripComboBoxFilter.Name = "m_toolStripComboBoxFilter";
			this.m_toolStripComboBoxFilter.Size = new System.Drawing.Size(225, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripComboBoxFilter, false);
			this.m_toolStripComboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.HandleFilterChanged);
			// 
			// toolStripSeparator3
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator3, Glyssen.Utilities.GlyssenColors.BackColor);
			this.toolStripSeparator3.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator3, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator3, false);
			// 
			// m_scriptureReference
			// 
			this.glyssenColorPalette.SetBackColor(this.m_scriptureReference, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_scriptureReference.BackColor = System.Drawing.SystemColors.Control;
			this.m_scriptureReference.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_scriptureReference, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_scriptureReference, "");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_scriptureReference, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_scriptureReference, L10NSharp.LocalizationPriority.Low);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_scriptureReference, "DialogBoxes.BlockNavigationControls.VerseControl");
			this.m_scriptureReference.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.m_scriptureReference.Name = "m_scriptureReference";
			this.m_scriptureReference.Size = new System.Drawing.Size(191, 23);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_scriptureReference, false);
			// 
			// m_menuBtnSplitBlock
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuBtnSplitBlock, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_menuBtnSplitBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.glyssenColorPalette.SetForeColor(this.m_menuBtnSplitBlock, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_menuBtnSplitBlock.Image = global::Glyssen.Properties.Resources.SplitBlock;
			this.m_menuBtnSplitBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuBtnSplitBlock, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuBtnSplitBlock, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuBtnSplitBlock, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuBtnSplitBlock, "DialogBoxes.AssignCharacterDlg.m_menuBtnSplitBlock");
			this.m_menuBtnSplitBlock.Name = "m_menuBtnSplitBlock";
			this.m_menuBtnSplitBlock.Size = new System.Drawing.Size(23, 22);
			this.m_menuBtnSplitBlock.Text = "Split";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuBtnSplitBlock, false);
			this.m_menuBtnSplitBlock.Click += new System.EventHandler(this.HandleSplitBlocksClick);
			// 
			// m_chkSingleVoice
			// 
			this.m_chkSingleVoice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_chkSingleVoice.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_chkSingleVoice, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_chkSingleVoice.BackColor = System.Drawing.SystemColors.Control;
			this.m_chkSingleVoice.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_chkSingleVoice, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_chkSingleVoice.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_chkSingleVoice, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_chkSingleVoice, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_chkSingleVoice, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_chkSingleVoice, "DialogBoxes.AssignCharacterDlg.m_chkSingleVoice");
			this.m_chkSingleVoice.Location = new System.Drawing.Point(8, 438);
			this.m_chkSingleVoice.Name = "m_chkSingleVoice";
			this.m_chkSingleVoice.Size = new System.Drawing.Size(248, 17);
			this.m_chkSingleVoice.TabIndex = 31;
			this.m_chkSingleVoice.Text = "This book ({0}) will be narrated, not dramatized.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_chkSingleVoice, true);
			this.m_chkSingleVoice.UseVisualStyleBackColor = false;
			this.m_chkSingleVoice.CheckedChanged += new System.EventHandler(this.m_chkSingleVoice_CheckedChanged);
			// 
			// m_llClose
			// 
			this.m_llClose.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_llClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_llClose.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_llClose.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_llClose.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetForeColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_llClose.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetLinkColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_llClose.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_llClose, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_llClose, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_llClose, "Common.Close");
			this.m_llClose.Location = new System.Drawing.Point(388, 504);
			this.m_llClose.Name = "m_llClose";
			this.m_llClose.Size = new System.Drawing.Size(33, 13);
			this.m_llClose.TabIndex = 31;
			this.m_llClose.TabStop = true;
			this.m_llClose.Text = "Close";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_llClose, true);
			this.m_llClose.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_llClose, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_llClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_llClose_LinkClicked);
			// 
			// m_blocksViewer
			// 
			this.m_blocksViewer.AccessibleDescription = "";
			this.glyssenColorPalette.SetBackColor(this.m_blocksViewer, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_blocksViewer.BackColor = System.Drawing.SystemColors.Control;
			this.m_blocksViewer.ContentBorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_blocksViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_blocksViewer, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_blocksViewer.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksViewer, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksViewer, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksViewer, "DialogBoxes.AssignCharacterDlg.ScriptBlocksViewer");
			this.m_blocksViewer.Location = new System.Drawing.Point(12, 12);
			this.m_blocksViewer.MinimumSize = new System.Drawing.Size(360, 9);
			this.m_blocksViewer.Name = "m_blocksViewer";
			this.m_blocksViewer.Size = new System.Drawing.Size(418, 505);
			this.m_blocksViewer.TabIndex = 11;
			this.m_blocksViewer.Text = "Who speaks this part?";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_blocksViewer, true);
			this.m_blocksViewer.ViewType = Glyssen.Controls.ScriptBlocksViewType.Html;
			// 
			// m_progressBar
			// 
			this.m_progressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_progressBar, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.glyssenColorPalette.SetForeColor(this.m_progressBar, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_progressBar.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_progressBar, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_progressBar, "DialogBoxes.AssignCharacterDlg.m_progressBar");
			this.m_progressBar.Location = new System.Drawing.Point(0, 554);
			this.m_progressBar.Name = "m_progressBar";
			this.m_progressBar.Size = new System.Drawing.Size(875, 17);
			this.m_progressBar.TabIndex = 12;
			this.m_progressBar.UnitName = "Blocks";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_progressBar, false);
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_saveStatus, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_saveStatus.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_saveStatus, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_saveStatus, "DialogBoxes.AssignCharacterDlg.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(24, 504);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 33;
			this.m_saveStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_saveStatus, true);
			// 
			// tabPageSelectCharacter
			// 
			this.glyssenColorPalette.SetBackColor(this.tabPageSelectCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tabPageSelectCharacter.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageSelectCharacter.Controls.Add(this.m_pnlCharacterAndDeliverySelection);
			this.glyssenColorPalette.SetForeColor(this.tabPageSelectCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.tabPageSelectCharacter.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.tabPageSelectCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.tabPageSelectCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.tabPageSelectCharacter, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.tabPageSelectCharacter");
			this.tabPageSelectCharacter.Location = new System.Drawing.Point(4, 22);
			this.tabPageSelectCharacter.Name = "tabPageSelectCharacter";
			this.tabPageSelectCharacter.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSelectCharacter.Size = new System.Drawing.Size(416, 396);
			this.tabPageSelectCharacter.TabIndex = 0;
			this.tabPageSelectCharacter.Text = "Select Character";
			this.glyssenColorPalette.SetUsePaletteColors(this.tabPageSelectCharacter, true);
			// 
			// m_pnlCharacterAndDeliverySelection
			// 
			this.m_pnlCharacterAndDeliverySelection.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_pnlCharacterAndDeliverySelection, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlCharacterAndDeliverySelection.ColumnCount = 3;
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlCharacterAndDeliverySelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_lblCharacter, 1, 0);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_llMoreDel, 1, 7);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlDeliveryFilter, 1, 5);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_btnAssign, 1, 8);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_btnAddDelivery, 2, 5);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlShortcuts, 0, 2);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_btnAddCharacter, 2, 1);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_listBoxDeliveries, 1, 6);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_pnlCharacterFilter, 1, 1);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_lblDelivery, 1, 4);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_listBoxCharacters, 1, 2);
			this.m_pnlCharacterAndDeliverySelection.Controls.Add(this.m_llMoreChar, 1, 3);
			this.m_pnlCharacterAndDeliverySelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_pnlCharacterAndDeliverySelection, Glyssen.Utilities.GlyssenColors.Default);
			this.m_pnlCharacterAndDeliverySelection.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_pnlCharacterAndDeliverySelection.Location = new System.Drawing.Point(3, 3);
			this.m_pnlCharacterAndDeliverySelection.Name = "m_pnlCharacterAndDeliverySelection";
			this.m_pnlCharacterAndDeliverySelection.RowCount = 9;
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_pnlCharacterAndDeliverySelection.Size = new System.Drawing.Size(410, 390);
			this.m_pnlCharacterAndDeliverySelection.TabIndex = 29;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_pnlCharacterAndDeliverySelection, true);
			// 
			// m_pnlDeliveryFilter
			// 
			this.m_pnlDeliveryFilter.AutoSize = true;
			this.m_pnlDeliveryFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_pnlDeliveryFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlDeliveryFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlDeliveryFilter.Controls.Add(this.tableLayoutPanelDelivery);
			this.m_pnlDeliveryFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_pnlDeliveryFilter, Glyssen.Utilities.GlyssenColors.Default);
			this.m_pnlDeliveryFilter.Location = new System.Drawing.Point(17, 228);
			this.m_pnlDeliveryFilter.Name = "m_pnlDeliveryFilter";
			this.m_pnlDeliveryFilter.Size = new System.Drawing.Size(370, 21);
			this.m_pnlDeliveryFilter.TabIndex = 23;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_pnlDeliveryFilter, false);
			// 
			// tableLayoutPanelDelivery
			// 
			this.tableLayoutPanelDelivery.AutoSize = true;
			this.tableLayoutPanelDelivery.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanelDelivery, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanelDelivery.ColumnCount = 2;
			this.tableLayoutPanelDelivery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelDelivery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelDelivery.Controls.Add(this.m_icnDeliveryFilter, 1, 0);
			this.tableLayoutPanelDelivery.Controls.Add(this.m_txtDeliveryFilter, 0, 0);
			this.tableLayoutPanelDelivery.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanelDelivery, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanelDelivery.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelDelivery.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelDelivery.Name = "tableLayoutPanelDelivery";
			this.tableLayoutPanelDelivery.RowCount = 1;
			this.tableLayoutPanelDelivery.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelDelivery.Size = new System.Drawing.Size(366, 17);
			this.tableLayoutPanelDelivery.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanelDelivery, false);
			// 
			// m_pnlShortcuts
			// 
			this.m_pnlShortcuts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_pnlShortcuts, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlShortcuts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut5, 0, 4);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut1, 0, 0);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut2, 0, 1);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut4, 0, 3);
			this.m_pnlShortcuts.Controls.Add(this.m_lblShortcut3, 0, 2);
			this.glyssenColorPalette.SetForeColor(this.m_pnlShortcuts, Glyssen.Utilities.GlyssenColors.Default);
			this.m_pnlShortcuts.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_pnlShortcuts.Location = new System.Drawing.Point(3, 56);
			this.m_pnlShortcuts.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_pnlShortcuts.Name = "m_pnlShortcuts";
			this.m_pnlShortcuts.RowCount = 5;
			this.m_pnlShortcuts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlShortcuts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlShortcuts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlShortcuts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlShortcuts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.m_pnlShortcuts.Size = new System.Drawing.Size(11, 86);
			this.m_pnlShortcuts.TabIndex = 28;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_pnlShortcuts, false);
			// 
			// m_listBoxDeliveries
			// 
			this.glyssenColorPalette.SetBackColor(this.m_listBoxDeliveries, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_listBoxDeliveries.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_listBoxDeliveries, 2);
			this.m_listBoxDeliveries.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_listBoxDeliveries, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_listBoxDeliveries.FormattingEnabled = true;
			this.m_listBoxDeliveries.IntegralHeight = false;
			this.m_listBoxDeliveries.Location = new System.Drawing.Point(17, 255);
			this.m_listBoxDeliveries.Name = "m_listBoxDeliveries";
			this.m_listBoxDeliveries.Size = new System.Drawing.Size(390, 90);
			this.m_listBoxDeliveries.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_listBoxDeliveries, false);
			this.m_listBoxDeliveries.SelectedIndexChanged += new System.EventHandler(this.m_listBoxDeliveries_SelectedIndexChanged);
			// 
			// m_pnlCharacterFilter
			// 
			this.m_pnlCharacterFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_pnlCharacterFilter.AutoSize = true;
			this.m_pnlCharacterFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_pnlCharacterFilter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_pnlCharacterFilter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pnlCharacterFilter.Controls.Add(this.tableLayoutPanelCharacter);
			this.glyssenColorPalette.SetForeColor(this.m_pnlCharacterFilter, Glyssen.Utilities.GlyssenColors.Default);
			this.m_pnlCharacterFilter.Location = new System.Drawing.Point(17, 29);
			this.m_pnlCharacterFilter.Name = "m_pnlCharacterFilter";
			this.m_pnlCharacterFilter.Size = new System.Drawing.Size(370, 21);
			this.m_pnlCharacterFilter.TabIndex = 21;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_pnlCharacterFilter, false);
			// 
			// tableLayoutPanelCharacter
			// 
			this.tableLayoutPanelCharacter.AutoSize = true;
			this.tableLayoutPanelCharacter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanelCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanelCharacter.ColumnCount = 2;
			this.tableLayoutPanelCharacter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCharacter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelCharacter.Controls.Add(this.m_icnCharacterFilter, 1, 0);
			this.tableLayoutPanelCharacter.Controls.Add(this.m_txtCharacterFilter, 0, 0);
			this.tableLayoutPanelCharacter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanelCharacter, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanelCharacter.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelCharacter.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelCharacter.Name = "tableLayoutPanelCharacter";
			this.tableLayoutPanelCharacter.RowCount = 1;
			this.tableLayoutPanelCharacter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCharacter.Size = new System.Drawing.Size(366, 17);
			this.tableLayoutPanelCharacter.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanelCharacter, false);
			// 
			// m_listBoxCharacters
			// 
			this.glyssenColorPalette.SetBackColor(this.m_listBoxCharacters, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_listBoxCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_pnlCharacterAndDeliverySelection.SetColumnSpan(this.m_listBoxCharacters, 2);
			this.m_listBoxCharacters.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_listBoxCharacters, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_listBoxCharacters.FormattingEnabled = true;
			this.m_listBoxCharacters.IntegralHeight = false;
			this.m_listBoxCharacters.Location = new System.Drawing.Point(17, 56);
			this.m_listBoxCharacters.Name = "m_listBoxCharacters";
			this.m_listBoxCharacters.Size = new System.Drawing.Size(390, 130);
			this.m_listBoxCharacters.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_listBoxCharacters, false);
			this.m_listBoxCharacters.SelectedIndexChanged += new System.EventHandler(this.m_listBoxCharacters_SelectedIndexChanged);
			this.m_listBoxCharacters.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.m_listBoxCharacters_KeyPress);
			this.m_listBoxCharacters.MouseMove += new System.Windows.Forms.MouseEventHandler(this.m_listBoxCharacters_MouseMove);
			// 
			// tabPageMatchReferenceText
			// 
			this.glyssenColorPalette.SetBackColor(this.tabPageMatchReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tabPageMatchReferenceText.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageMatchReferenceText.Controls.Add(this.m_dataGridReferenceText);
			this.tabPageMatchReferenceText.Controls.Add(this.m_tableLayoutPanelMatchReferenceTextButtons);
			this.tabPageMatchReferenceText.Controls.Add(this.m_lblReferenceText);
			this.glyssenColorPalette.SetForeColor(this.tabPageMatchReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.tabPageMatchReferenceText.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.tabPageMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.tabPageMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.tabPageMatchReferenceText, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.tabPageMatchReferenceText");
			this.tabPageMatchReferenceText.Location = new System.Drawing.Point(4, 22);
			this.tabPageMatchReferenceText.Name = "tabPageMatchReferenceText";
			this.tabPageMatchReferenceText.Padding = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.tabPageMatchReferenceText.Size = new System.Drawing.Size(416, 396);
			this.tabPageMatchReferenceText.TabIndex = 1;
			this.tabPageMatchReferenceText.Text = "Match Reference Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.tabPageMatchReferenceText, true);
			this.tabPageMatchReferenceText.UseVisualStyleBackColor = true;
			// 
			// m_dataGridReferenceText
			// 
			this.m_dataGridReferenceText.AllowUserToAddRows = false;
			this.m_dataGridReferenceText.AllowUserToDeleteRows = false;
			this.m_dataGridReferenceText.AllowUserToResizeRows = false;
			this.m_dataGridReferenceText.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.m_dataGridReferenceText.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
			this.glyssenColorPalette.SetBackColor(this.m_dataGridReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_dataGridReferenceText.BackgroundColor = System.Drawing.SystemColors.ControlLight;
			this.m_dataGridReferenceText.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.m_dataGridReferenceText.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCharacter,
            this.colPrimary,
            this.colEnglish,
            this.colDelivery});
			this.m_dataGridReferenceText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_dataGridReferenceText.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.glyssenColorPalette.SetForeColor(this.m_dataGridReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_dataGridReferenceText.GridColor = System.Drawing.SystemColors.ControlLight;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_dataGridReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_dataGridReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_dataGridReferenceText, "DialogBoxes.AssignCharacterDlg.DataGridReferenceText");
			this.m_dataGridReferenceText.Location = new System.Drawing.Point(3, 32);
			this.m_dataGridReferenceText.MultiSelect = false;
			this.m_dataGridReferenceText.Name = "m_dataGridReferenceText";
			this.m_dataGridReferenceText.RowHeadersWidth = 26;
			this.m_dataGridReferenceText.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.m_dataGridReferenceText.Size = new System.Drawing.Size(410, 295);
			this.m_dataGridReferenceText.TabIndex = 35;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_dataGridReferenceText, true);
			this.m_dataGridReferenceText.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGridReferenceText_CellEnter);
			this.m_dataGridReferenceText.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGridReferenceText_CellLeave);
			this.m_dataGridReferenceText.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_dataGridReferenceText_CellMouseDown);
			this.m_dataGridReferenceText.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.m_dataGridReferenceText_CellPainting);
			this.m_dataGridReferenceText.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.m_dataGridReferenceText_CellValidating);
			this.m_dataGridReferenceText.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.UpdateRowSpecificButtonStates);
			// 
			// colCharacter
			// 
			this.colCharacter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colCharacter.ContextMenuStrip = this.m_contextMenuCharacterOrDeliveryCell;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.Transparent;
			this.colCharacter.DefaultCellStyle = dataGridViewCellStyle1;
			this.colCharacter.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
			this.colCharacter.DropDownWidth = 45;
			this.colCharacter.FillWeight = 60.32995F;
			this.colCharacter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.colCharacter.HeaderText = "Character";
			this.colCharacter.MinimumWidth = 56;
			this.colCharacter.Name = "colCharacter";
			// 
			// m_contextMenuCharacterOrDeliveryCell
			// 
			this.glyssenColorPalette.SetBackColor(this.m_contextMenuCharacterOrDeliveryCell, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_contextMenuCharacterOrDeliveryCell, Glyssen.Utilities.GlyssenColors.Default);
			this.m_contextMenuCharacterOrDeliveryCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ContextMenuItemAddCharacterOrDelivery,
            this.toolStripSeparator7,
            this.m_CharacterOrDeliveryContextMenuItemMoveUp,
            this.m_CharacterOrDeliveryContextMenuItemMoveDown});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacterOrDeliveryCell, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacterOrDeliveryCell, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacterOrDeliveryCell, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacterOrDeliveryCell, "DialogBoxes.AssignCharacterDlg.contextMenuStrip1");
			this.m_contextMenuCharacterOrDeliveryCell.Name = "m_contextMenuCharacterOrDeliveryCell";
			this.m_contextMenuCharacterOrDeliveryCell.Size = new System.Drawing.Size(219, 76);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_contextMenuCharacterOrDeliveryCell, false);
			this.m_contextMenuCharacterOrDeliveryCell.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacterOrDeliveryCell_Opening);
			// 
			// m_ContextMenuItemAddCharacterOrDelivery
			// 
			this.glyssenColorPalette.SetBackColor(this.m_ContextMenuItemAddCharacterOrDelivery, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_ContextMenuItemAddCharacterOrDelivery, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_ContextMenuItemAddCharacterOrDelivery, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_ContextMenuItemAddCharacterOrDelivery, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_ContextMenuItemAddCharacterOrDelivery, "DialogBoxes.AssignCharacterDlg.AddCharacterOrDeliveryContextMenuItem");
			this.m_ContextMenuItemAddCharacterOrDelivery.Name = "m_ContextMenuItemAddCharacterOrDelivery";
			this.m_ContextMenuItemAddCharacterOrDelivery.Size = new System.Drawing.Size(218, 22);
			this.m_ContextMenuItemAddCharacterOrDelivery.Text = "Add Character or Delivery...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_ContextMenuItemAddCharacterOrDelivery, false);
			this.m_ContextMenuItemAddCharacterOrDelivery.Click += new System.EventHandler(this.ContextMenuItemAddCharacterOrDelivery_Click);
			// 
			// toolStripSeparator7
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator7, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator7, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(215, 6);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator7, false);
			// 
			// m_CharacterOrDeliveryContextMenuItemMoveUp
			// 
			this.glyssenColorPalette.SetBackColor(this.m_CharacterOrDeliveryContextMenuItemMoveUp, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_CharacterOrDeliveryContextMenuItemMoveUp, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_CharacterOrDeliveryContextMenuItemMoveUp, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_CharacterOrDeliveryContextMenuItemMoveUp, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_CharacterOrDeliveryContextMenuItemMoveUp, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_CharacterOrDeliveryContextMenuItemMoveUp, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.moveLineUpToolStripMenuItem");
			this.m_CharacterOrDeliveryContextMenuItemMoveUp.Name = "m_CharacterOrDeliveryContextMenuItemMoveUp";
			this.m_CharacterOrDeliveryContextMenuItemMoveUp.Size = new System.Drawing.Size(218, 22);
			this.m_CharacterOrDeliveryContextMenuItemMoveUp.Text = "Move Line Up";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_CharacterOrDeliveryContextMenuItemMoveUp, false);
			this.m_CharacterOrDeliveryContextMenuItemMoveUp.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			// 
			// m_CharacterOrDeliveryContextMenuItemMoveDown
			// 
			this.glyssenColorPalette.SetBackColor(this.m_CharacterOrDeliveryContextMenuItemMoveDown, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_CharacterOrDeliveryContextMenuItemMoveDown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_CharacterOrDeliveryContextMenuItemMoveDown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_CharacterOrDeliveryContextMenuItemMoveDown, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_CharacterOrDeliveryContextMenuItemMoveDown, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_CharacterOrDeliveryContextMenuItemMoveDown, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.moveLineDownToolStripMenuItem");
			this.m_CharacterOrDeliveryContextMenuItemMoveDown.Name = "m_CharacterOrDeliveryContextMenuItemMoveDown";
			this.m_CharacterOrDeliveryContextMenuItemMoveDown.Size = new System.Drawing.Size(218, 22);
			this.m_CharacterOrDeliveryContextMenuItemMoveDown.Text = "Move Line Down";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_CharacterOrDeliveryContextMenuItemMoveDown, false);
			this.m_CharacterOrDeliveryContextMenuItemMoveDown.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			// 
			// colPrimary
			// 
			this.colPrimary.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colPrimary.ContextMenuStrip = this.m_contextMenuRefTextCell;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.colPrimary.DefaultCellStyle = dataGridViewCellStyle2;
			this.colPrimary.FillWeight = 75.31017F;
			this.colPrimary.HeaderText = "{0}";
			this.colPrimary.Name = "colPrimary";
			this.colPrimary.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// m_contextMenuRefTextCell
			// 
			this.glyssenColorPalette.SetBackColor(this.m_contextMenuRefTextCell, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_contextMenuRefTextCell, Glyssen.Utilities.GlyssenColors.Default);
			this.m_contextMenuRefTextCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ContextMenuItemSplitText,
            this.m_ContextMenuItemInsertHeSaid,
            this.toolStripSeparator6,
            this.m_RefTextContextMenuItemMoveUp,
            this.m_RefTextContextMenuItemMoveDown});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuRefTextCell, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuRefTextCell, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuRefTextCell, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuRefTextCell, "DialogBoxes.AssignCharacterDlg.contextMenuStrip1");
			this.m_contextMenuRefTextCell.Name = "m_contextMenuRefTextCell";
			this.m_contextMenuRefTextCell.Size = new System.Drawing.Size(164, 98);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_contextMenuRefTextCell, false);
			this.m_contextMenuRefTextCell.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuRefTextCell_Opening);
			// 
			// m_ContextMenuItemSplitText
			// 
			this.glyssenColorPalette.SetBackColor(this.m_ContextMenuItemSplitText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_ContextMenuItemSplitText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_ContextMenuItemSplitText, "After selecting this command, click in the text where you want to split the text." +
        "");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_ContextMenuItemSplitText, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_ContextMenuItemSplitText, "DialogBoxes.AssignCharacterDlg.SplitTextContextMenuItem");
			this.m_ContextMenuItemSplitText.Name = "m_ContextMenuItemSplitText";
			this.m_ContextMenuItemSplitText.Size = new System.Drawing.Size(163, 22);
			this.m_ContextMenuItemSplitText.Text = "Split Text";
			this.m_ContextMenuItemSplitText.ToolTipText = "After selecting this command, click in the text where you want to split the text." +
    "";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_ContextMenuItemSplitText, false);
			this.m_ContextMenuItemSplitText.Click += new System.EventHandler(this.m_ContextMenuItemSplitText_Click);
			// 
			// m_ContextMenuItemInsertHeSaid
			// 
			this.glyssenColorPalette.SetBackColor(this.m_ContextMenuItemInsertHeSaid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_ContextMenuItemInsertHeSaid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_ContextMenuItemInsertHeSaid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_ContextMenuItemInsertHeSaid, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_ContextMenuItemInsertHeSaid, "DialogBoxes.AssignCharacterDlg.InsertHeSaidContextMenuItem");
			this.m_ContextMenuItemInsertHeSaid.Name = "m_ContextMenuItemInsertHeSaid";
			this.m_ContextMenuItemInsertHeSaid.Size = new System.Drawing.Size(163, 22);
			this.m_ContextMenuItemInsertHeSaid.Text = "Insert \"He Said\"";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_ContextMenuItemInsertHeSaid, false);
			this.m_ContextMenuItemInsertHeSaid.Click += new System.EventHandler(this.HandleInsertContextMenuHeSaidClicked);
			// 
			// toolStripSeparator6
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator6, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator6, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(160, 6);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator6, false);
			// 
			// m_RefTextContextMenuItemMoveUp
			// 
			this.glyssenColorPalette.SetBackColor(this.m_RefTextContextMenuItemMoveUp, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_RefTextContextMenuItemMoveUp, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_RefTextContextMenuItemMoveUp, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_RefTextContextMenuItemMoveUp, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_RefTextContextMenuItemMoveUp, "DialogBoxes.AssignCharacterDlg.MoveLineUpContextMenuItem");
			this.m_RefTextContextMenuItemMoveUp.Name = "m_RefTextContextMenuItemMoveUp";
			this.m_RefTextContextMenuItemMoveUp.Size = new System.Drawing.Size(163, 22);
			this.m_RefTextContextMenuItemMoveUp.Text = "Move Line Up";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_RefTextContextMenuItemMoveUp, false);
			this.m_RefTextContextMenuItemMoveUp.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			// 
			// m_RefTextContextMenuItemMoveDown
			// 
			this.glyssenColorPalette.SetBackColor(this.m_RefTextContextMenuItemMoveDown, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_RefTextContextMenuItemMoveDown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_RefTextContextMenuItemMoveDown, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_RefTextContextMenuItemMoveDown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_RefTextContextMenuItemMoveDown, "DialogBoxes.AssignCharacterDlg.MoveLineDownContextMenuItem");
			this.m_RefTextContextMenuItemMoveDown.Name = "m_RefTextContextMenuItemMoveDown";
			this.m_RefTextContextMenuItemMoveDown.Size = new System.Drawing.Size(163, 22);
			this.m_RefTextContextMenuItemMoveDown.Text = "Move Line Down";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_RefTextContextMenuItemMoveDown, false);
			this.m_RefTextContextMenuItemMoveDown.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			// 
			// colEnglish
			// 
			this.colEnglish.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colEnglish.ContextMenuStrip = this.m_contextMenuRefTextCell;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.colEnglish.DefaultCellStyle = dataGridViewCellStyle3;
			this.colEnglish.FillWeight = 75.31017F;
			this.colEnglish.HeaderText = "English";
			this.colEnglish.Name = "colEnglish";
			this.colEnglish.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// colDelivery
			// 
			this.colDelivery.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colDelivery.ContextMenuStrip = this.m_contextMenuCharacterOrDeliveryCell;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.Transparent;
			this.colDelivery.DefaultCellStyle = dataGridViewCellStyle4;
			this.colDelivery.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
			this.colDelivery.DropDownWidth = 45;
			this.colDelivery.FillWeight = 39.04972F;
			this.colDelivery.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.colDelivery.HeaderText = "Delivery";
			this.colDelivery.MinimumWidth = 35;
			this.colDelivery.Name = "colDelivery";
			// 
			// m_tableLayoutPanelMatchReferenceTextButtons
			// 
			this.m_tableLayoutPanelMatchReferenceTextButtons.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMatchReferenceTextButtons, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMatchReferenceTextButtons.ColumnCount = 3;
			this.m_tableLayoutPanelMatchReferenceTextButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMatchReferenceTextButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMatchReferenceTextButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMatchReferenceTextButtons.Controls.Add(this.m_btnApplyReferenceTextMatches, 2, 0);
			this.m_tableLayoutPanelMatchReferenceTextButtons.Controls.Add(this.m_flowLayoutPanelMatchReferenceTextLeftButtons, 0, 0);
			this.m_tableLayoutPanelMatchReferenceTextButtons.Controls.Add(this.m_btnReset, 1, 0);
			this.m_tableLayoutPanelMatchReferenceTextButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMatchReferenceTextButtons, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMatchReferenceTextButtons.Location = new System.Drawing.Point(3, 327);
			this.m_tableLayoutPanelMatchReferenceTextButtons.Name = "m_tableLayoutPanelMatchReferenceTextButtons";
			this.m_tableLayoutPanelMatchReferenceTextButtons.RowCount = 1;
			this.m_tableLayoutPanelMatchReferenceTextButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMatchReferenceTextButtons.Size = new System.Drawing.Size(410, 66);
			this.m_tableLayoutPanelMatchReferenceTextButtons.TabIndex = 41;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMatchReferenceTextButtons, false);
			// 
			// m_btnApplyReferenceTextMatches
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnApplyReferenceTextMatches, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnApplyReferenceTextMatches, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnApplyReferenceTextMatches, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnApplyReferenceTextMatches, "Save the alignment of the reference text to the vernacular script.");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnApplyReferenceTextMatches, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnApplyReferenceTextMatches, "DialogBoxes.AssignCharacterDlg.ApplyReferenceTextMatchesButton");
			this.m_btnApplyReferenceTextMatches.Location = new System.Drawing.Point(335, 3);
			this.m_btnApplyReferenceTextMatches.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_btnApplyReferenceTextMatches.Name = "m_btnApplyReferenceTextMatches";
			this.m_btnApplyReferenceTextMatches.Size = new System.Drawing.Size(75, 23);
			this.m_btnApplyReferenceTextMatches.TabIndex = 36;
			this.m_btnApplyReferenceTextMatches.Text = "Apply";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnApplyReferenceTextMatches, false);
			this.m_btnApplyReferenceTextMatches.UseVisualStyleBackColor = true;
			this.m_btnApplyReferenceTextMatches.Click += new System.EventHandler(this.m_btnApplyReferenceTextMatches_Click);
			// 
			// m_flowLayoutPanelMatchReferenceTextLeftButtons
			// 
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_flowLayoutPanelMatchReferenceTextLeftButtons, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Controls.Add(this.m_btnMoveReferenceTextUp);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Controls.Add(this.m_btnMoveReferenceTextDown);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Controls.Add(this.m_toolStripMatchReferenceText);
			this.glyssenColorPalette.SetForeColor(this.m_flowLayoutPanelMatchReferenceTextLeftButtons, Glyssen.Utilities.GlyssenColors.Default);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Location = new System.Drawing.Point(3, 3);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Name = "m_flowLayoutPanelMatchReferenceTextLeftButtons";
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.Size = new System.Drawing.Size(170, 60);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.TabIndex = 40;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_flowLayoutPanelMatchReferenceTextLeftButtons, false);
			// 
			// m_btnMoveReferenceTextUp
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnMoveReferenceTextUp, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnMoveReferenceTextUp, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnMoveReferenceTextUp, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnMoveReferenceTextUp, "Align the selected block in the reference text with the previous block in the ver" +
        "nacular script.");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnMoveReferenceTextUp, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnMoveReferenceTextUp, "DialogBoxes.AssignCharacterDlg.MoveReferenceTextUp");
			this.m_btnMoveReferenceTextUp.Location = new System.Drawing.Point(11, 3);
			this.m_btnMoveReferenceTextUp.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
			this.m_btnMoveReferenceTextUp.Name = "m_btnMoveReferenceTextUp";
			this.m_btnMoveReferenceTextUp.Size = new System.Drawing.Size(75, 23);
			this.m_btnMoveReferenceTextUp.TabIndex = 37;
			this.m_btnMoveReferenceTextUp.Text = "Move Up";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnMoveReferenceTextUp, false);
			this.m_btnMoveReferenceTextUp.UseVisualStyleBackColor = true;
			this.m_btnMoveReferenceTextUp.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			this.m_btnMoveReferenceTextUp.MouseEnter += new System.EventHandler(this.HandleMouseEnterButtonThatAffectsEntireGridRow);
			this.m_btnMoveReferenceTextUp.MouseLeave += new System.EventHandler(this.HandleMouseLeaveButtonThatAffectsEntireGridRow);
			// 
			// m_btnMoveReferenceTextDown
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnMoveReferenceTextDown, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnMoveReferenceTextDown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnMoveReferenceTextDown, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnMoveReferenceTextDown, "Align the selected block in the reference text with the following block in the ve" +
        "rnacular script.");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnMoveReferenceTextDown, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnMoveReferenceTextDown, "DialogBoxes.AssignCharacterDlg.MoveReferenceTextDown");
			this.m_btnMoveReferenceTextDown.Location = new System.Drawing.Point(92, 3);
			this.m_btnMoveReferenceTextDown.Name = "m_btnMoveReferenceTextDown";
			this.m_btnMoveReferenceTextDown.Size = new System.Drawing.Size(75, 23);
			this.m_btnMoveReferenceTextDown.TabIndex = 38;
			this.m_btnMoveReferenceTextDown.Text = "Move Down";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnMoveReferenceTextDown, false);
			this.m_btnMoveReferenceTextDown.UseVisualStyleBackColor = true;
			this.m_btnMoveReferenceTextDown.Click += new System.EventHandler(this.HandleMoveReferenceTextUpOrDown_Click);
			this.m_btnMoveReferenceTextDown.MouseEnter += new System.EventHandler(this.HandleMouseEnterButtonThatAffectsEntireGridRow);
			this.m_btnMoveReferenceTextDown.MouseLeave += new System.EventHandler(this.HandleMouseLeaveButtonThatAffectsEntireGridRow);
			// 
			// m_toolStripMatchReferenceText
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripMatchReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripMatchReferenceText.CanOverflow = false;
			this.m_toolStripMatchReferenceText.Dock = System.Windows.Forms.DockStyle.None;
			this.glyssenColorPalette.SetForeColor(this.m_toolStripMatchReferenceText, Glyssen.Utilities.GlyssenColors.Default);
			this.m_toolStripMatchReferenceText.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStripMatchReferenceText.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_btnInsertHeSaid});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripMatchReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripMatchReferenceText, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripMatchReferenceText, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.m_toolStripMatchReferenceText");
			this.m_toolStripMatchReferenceText.Location = new System.Drawing.Point(11, 32);
			this.m_toolStripMatchReferenceText.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
			this.m_toolStripMatchReferenceText.Name = "m_toolStripMatchReferenceText";
			this.m_toolStripMatchReferenceText.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.m_toolStripMatchReferenceText.Size = new System.Drawing.Size(107, 25);
			this.m_toolStripMatchReferenceText.TabIndex = 39;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripMatchReferenceText, false);
			// 
			// m_btnInsertHeSaid
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnInsertHeSaid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnInsertHeSaid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_btnInsertHeSaid.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuInsertIntoSelectedRowOnly,
            this.m_menuInsertIntoAllEmptyCells});
			this.glyssenColorPalette.SetForeColor(this.m_btnInsertHeSaid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_btnInsertHeSaid.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnInsertHeSaid, "Insert \"he said\" into any blank reference text cells in the selected row.");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnInsertHeSaid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnInsertHeSaid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnInsertHeSaid, "DialogBoxes.AssignCharacterDlg.AssignCharacterDlg.m_btnInsertHeSaid");
			this.m_btnInsertHeSaid.Margin = new System.Windows.Forms.Padding(0);
			this.m_btnInsertHeSaid.Name = "m_btnInsertHeSaid";
			this.m_btnInsertHeSaid.Size = new System.Drawing.Size(104, 25);
			this.m_btnInsertHeSaid.Text = "Insert \"He said\"";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnInsertHeSaid, false);
			this.m_btnInsertHeSaid.ButtonClick += new System.EventHandler(this.HandleInsertHeSaidClicked);
			this.m_btnInsertHeSaid.MouseEnter += new System.EventHandler(this.HandleMouseEnterInsertHeSaidButton);
			this.m_btnInsertHeSaid.MouseLeave += new System.EventHandler(this.HandleMouseLeaveInsertHeSaidButton);
			// 
			// m_menuInsertIntoSelectedRowOnly
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuInsertIntoSelectedRowOnly, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_menuInsertIntoSelectedRowOnly.Checked = true;
			this.m_menuInsertIntoSelectedRowOnly.CheckOnClick = true;
			this.m_menuInsertIntoSelectedRowOnly.CheckState = System.Windows.Forms.CheckState.Checked;
			this.glyssenColorPalette.SetForeColor(this.m_menuInsertIntoSelectedRowOnly, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuInsertIntoSelectedRowOnly, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuInsertIntoSelectedRowOnly, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuInsertIntoSelectedRowOnly, "DialogBoxes.AssignCharacterDlg.toolStripMenuItem1");
			this.m_menuInsertIntoSelectedRowOnly.Name = "m_menuInsertIntoSelectedRowOnly";
			this.m_menuInsertIntoSelectedRowOnly.Size = new System.Drawing.Size(172, 22);
			this.m_menuInsertIntoSelectedRowOnly.Text = "Selected Row Only";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuInsertIntoSelectedRowOnly, false);
			this.m_menuInsertIntoSelectedRowOnly.CheckedChanged += new System.EventHandler(this.HandleInsertHeSaidCheckChanged);
			this.m_menuInsertIntoSelectedRowOnly.Click += new System.EventHandler(this.HandleInsertHeSaidClicked);
			this.m_menuInsertIntoSelectedRowOnly.MouseEnter += new System.EventHandler(this.HandleMouseEnterInsertHeSaidButton);
			// 
			// m_menuInsertIntoAllEmptyCells
			// 
			this.m_menuInsertIntoAllEmptyCells.AutoToolTip = true;
			this.glyssenColorPalette.SetBackColor(this.m_menuInsertIntoAllEmptyCells, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_menuInsertIntoAllEmptyCells.CheckOnClick = true;
			this.glyssenColorPalette.SetForeColor(this.m_menuInsertIntoAllEmptyCells, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuInsertIntoAllEmptyCells, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuInsertIntoAllEmptyCells, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuInsertIntoAllEmptyCells, "DialogBoxes.AssignCharacterDlg.toolStripMenuItem1");
			this.m_menuInsertIntoAllEmptyCells.Name = "m_menuInsertIntoAllEmptyCells";
			this.m_menuInsertIntoAllEmptyCells.Size = new System.Drawing.Size(172, 22);
			this.m_menuInsertIntoAllEmptyCells.Text = "All Empty Cells";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuInsertIntoAllEmptyCells, false);
			this.m_menuInsertIntoAllEmptyCells.CheckedChanged += new System.EventHandler(this.HandleInsertHeSaidCheckChanged);
			this.m_menuInsertIntoAllEmptyCells.Click += new System.EventHandler(this.HandleInsertHeSaidClicked);
			this.m_menuInsertIntoAllEmptyCells.MouseEnter += new System.EventHandler(this.HandleMouseEnterInsertHeSaidButton);
			this.m_menuInsertIntoAllEmptyCells.MouseLeave += new System.EventHandler(this.HandleMouseLeaveInsertHeSaidButton);
			// 
			// m_btnReset
			// 
			this.glyssenColorPalette.SetBackColor(this.m_btnReset, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnReset, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnReset, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnReset, "Revert all unsaved edits");
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnReset, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnReset, "DialogBoxes.AssignCharacterDlg.button1");
			this.m_btnReset.Location = new System.Drawing.Point(254, 3);
			this.m_btnReset.Name = "m_btnReset";
			this.m_btnReset.Size = new System.Drawing.Size(75, 23);
			this.m_btnReset.TabIndex = 41;
			this.m_btnReset.Text = "Reset";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnReset, false);
			this.m_btnReset.UseVisualStyleBackColor = true;
			this.m_btnReset.Click += new System.EventHandler(this.HandleResetMatchupClick);
			// 
			// m_lblReferenceText
			// 
			this.m_lblReferenceText.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblReferenceText, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblReferenceText.Dock = System.Windows.Forms.DockStyle.Top;
			this.glyssenColorPalette.SetForeColor(this.m_lblReferenceText, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblReferenceText.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblReferenceText, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblReferenceText, "DialogBoxes.AssignCharacterDlg.label1");
			this.m_lblReferenceText.Location = new System.Drawing.Point(3, 8);
			this.m_lblReferenceText.Name = "m_lblReferenceText";
			this.m_lblReferenceText.Size = new System.Drawing.Size(410, 24);
			this.m_lblReferenceText.TabIndex = 42;
			this.m_lblReferenceText.Text = "Reference Text";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblReferenceText, true);
			// 
			// m_splitContainer
			// 
			this.glyssenColorPalette.SetBackColor(this.m_splitContainer, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_splitContainer, Glyssen.Utilities.GlyssenColors.Default);
			this.m_splitContainer.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_splitContainer.Location = new System.Drawing.Point(0, 25);
			this.m_splitContainer.Name = "m_splitContainer";
			// 
			// m_splitContainer.Panel1
			// 
			this.glyssenColorPalette.SetBackColor(this.m_splitContainer.Panel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_splitContainer.Panel1.Controls.Add(this.m_blocksViewer);
			this.glyssenColorPalette.SetForeColor(this.m_splitContainer.Panel1, Glyssen.Utilities.GlyssenColors.Default);
			this.m_splitContainer.Panel1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(12, 12, 0, 12);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_splitContainer.Panel1, false);
			this.m_splitContainer.Panel1MinSize = 250;
			// 
			// m_splitContainer.Panel2
			// 
			this.glyssenColorPalette.SetBackColor(this.m_splitContainer.Panel2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_splitContainer.Panel2.Controls.Add(this.m_tabControlCharacterSelection);
			this.m_splitContainer.Panel2.Controls.Add(this.m_chkSingleVoice);
			this.m_splitContainer.Panel2.Controls.Add(this.m_saveStatus);
			this.m_splitContainer.Panel2.Controls.Add(this.m_llClose);
			this.m_splitContainer.Panel2.Controls.Add(this.tableLayoutPanelNavigationControls);
			this.glyssenColorPalette.SetForeColor(this.m_splitContainer.Panel2, Glyssen.Utilities.GlyssenColors.Default);
			this.m_splitContainer.Panel2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(0, 13, 12, 12);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_splitContainer.Panel2, false);
			this.m_splitContainer.Panel2MinSize = 307;
			this.m_splitContainer.Size = new System.Drawing.Size(875, 529);
			this.m_splitContainer.SplitterDistance = 430;
			this.m_splitContainer.TabIndex = 30;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_splitContainer, false);
			this.m_splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.m_splitContainer_SplitterMoved);
			// 
			// m_tabControlCharacterSelection
			// 
			this.m_tabControlCharacterSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tabControlCharacterSelection.Appearance = System.Windows.Forms.TabAppearance.Buttons;
			this.glyssenColorPalette.SetBackColor(this.m_tabControlCharacterSelection, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tabControlCharacterSelection.Controls.Add(this.tabPageSelectCharacter);
			this.m_tabControlCharacterSelection.Controls.Add(this.tabPageMatchReferenceText);
			this.m_tabControlCharacterSelection.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_tabControlCharacterSelection, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_tabControlCharacterSelection.ItemSize = new System.Drawing.Size(91, 18);
			this.m_tabControlCharacterSelection.Location = new System.Drawing.Point(5, 10);
			this.m_tabControlCharacterSelection.Name = "m_tabControlCharacterSelection";
			this.m_tabControlCharacterSelection.Padding = new System.Drawing.Point(6, 4);
			this.m_tabControlCharacterSelection.SelectedIndex = 0;
			this.m_tabControlCharacterSelection.Size = new System.Drawing.Size(424, 422);
			this.m_tabControlCharacterSelection.TabIndex = 35;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tabControlCharacterSelection, true);
			this.m_tabControlCharacterSelection.SelectedIndexChanged += new System.EventHandler(this.HandleCharacterSelectionTabIndexChanged);
			// 
			// tableLayoutPanelNavigationControls
			// 
			this.tableLayoutPanelNavigationControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelNavigationControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanelNavigationControls, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanelNavigationControls.ColumnCount = 3;
			this.tableLayoutPanelNavigationControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelNavigationControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelNavigationControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelNavigationControls.Controls.Add(this.m_btnPrevious, 0, 0);
			this.tableLayoutPanelNavigationControls.Controls.Add(this.m_labelXofY, 1, 0);
			this.tableLayoutPanelNavigationControls.Controls.Add(this.m_btnNext, 2, 0);
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanelNavigationControls, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanelNavigationControls.Location = new System.Drawing.Point(138, 469);
			this.tableLayoutPanelNavigationControls.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelNavigationControls.Name = "tableLayoutPanelNavigationControls";
			this.tableLayoutPanelNavigationControls.RowCount = 1;
			this.tableLayoutPanelNavigationControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelNavigationControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelNavigationControls.Size = new System.Drawing.Size(283, 30);
			this.tableLayoutPanelNavigationControls.TabIndex = 30;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanelNavigationControls, false);
			// 
			// AssignCharacterDlg
			// 
			this.AcceptButton = this.m_btnAssign;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(875, 571);
			this.Controls.Add(this.m_splitContainer);
			this.Controls.Add(this.m_progressBar);
			this.Controls.Add(this.m_toolStrip);
			this.DoubleBuffered = true;
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.Icon = global::Glyssen.Properties.Resources.glyssenIcon;
			this.KeyPreview = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.AssignCharacterDlg.IdentifySpeakingParts");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 599);
			this.Name = "AssignCharacterDlg";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Identify Speaking Parts - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AssignCharacterDialog_FormClosing);
			this.Load += new System.EventHandler(this.AssignCharacterDlg_Load);
			this.Shown += new System.EventHandler(this.AssignCharacterDialog_Shown);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssignCharacterDialog_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AssignCharacterDialog_KeyPress);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnCharacterFilter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_icnDeliveryFilter)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			this.tabPageSelectCharacter.ResumeLayout(false);
			this.m_pnlCharacterAndDeliverySelection.ResumeLayout(false);
			this.m_pnlCharacterAndDeliverySelection.PerformLayout();
			this.m_pnlDeliveryFilter.ResumeLayout(false);
			this.m_pnlDeliveryFilter.PerformLayout();
			this.tableLayoutPanelDelivery.ResumeLayout(false);
			this.tableLayoutPanelDelivery.PerformLayout();
			this.m_pnlShortcuts.ResumeLayout(false);
			this.m_pnlShortcuts.PerformLayout();
			this.m_pnlCharacterFilter.ResumeLayout(false);
			this.m_pnlCharacterFilter.PerformLayout();
			this.tableLayoutPanelCharacter.ResumeLayout(false);
			this.tableLayoutPanelCharacter.PerformLayout();
			this.tabPageMatchReferenceText.ResumeLayout(false);
			this.tabPageMatchReferenceText.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGridReferenceText)).EndInit();
			this.m_contextMenuCharacterOrDeliveryCell.ResumeLayout(false);
			this.m_contextMenuRefTextCell.ResumeLayout(false);
			this.m_tableLayoutPanelMatchReferenceTextButtons.ResumeLayout(false);
			this.m_tableLayoutPanelMatchReferenceTextButtons.PerformLayout();
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.ResumeLayout(false);
			this.m_flowLayoutPanelMatchReferenceTextLeftButtons.PerformLayout();
			this.m_toolStripMatchReferenceText.ResumeLayout(false);
			this.m_toolStripMatchReferenceText.PerformLayout();
			this.m_splitContainer.Panel1.ResumeLayout(false);
			this.m_splitContainer.Panel2.ResumeLayout(false);
			this.m_splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_splitContainer)).EndInit();
			this.m_splitContainer.ResumeLayout(false);
			this.m_tabControlCharacterSelection.ResumeLayout(false);
			this.tableLayoutPanelNavigationControls.ResumeLayout(false);
			this.tableLayoutPanelNavigationControls.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnNext;
		private System.Windows.Forms.Button m_btnPrevious;
		private System.Windows.Forms.Button m_btnAssign;
		private System.Windows.Forms.ListBox m_listBoxCharacters;
		private System.Windows.Forms.ListBox m_listBoxDeliveries;
		private System.Windows.Forms.Label m_labelWhoSpeaks;
		private System.Windows.Forms.Label m_labelXofY;
		private Glyssen.Controls.BlockProgressBar m_progressBar;
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
		private System.Windows.Forms.TableLayoutPanel m_pnlShortcuts;
		private System.Windows.Forms.Label m_lblShortcut3;
		private System.Windows.Forms.Label m_lblShortcut5;
		private System.Windows.Forms.Label m_lblShortcut4;
		private System.Windows.Forms.TableLayoutPanel m_pnlCharacterAndDeliverySelection;
		private System.Windows.Forms.SplitContainer m_splitContainer;
		private Glyssen.Controls.ScriptBlocksViewer m_blocksViewer;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDelivery;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCharacter;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFilter;
		private System.Windows.Forms.ToolStripComboBox m_toolStripComboBoxFilter;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonSmallerFont;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonLargerFont;
		private Paratext.ToolStripVerseControl m_scriptureReference;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelNavigationControls;
		private System.Windows.Forms.CheckBox m_chkSingleVoice;
		private System.Windows.Forms.ToolStripButton m_menuBtnSplitBlock;
		private System.Windows.Forms.LinkLabel m_llClose;
		private SaveStatus m_saveStatus;
		private GlyssenColorPalette glyssenColorPalette;
		private System.Windows.Forms.TabControl m_tabControlCharacterSelection;
		private System.Windows.Forms.TabPage tabPageSelectCharacter;
		private System.Windows.Forms.TabPage tabPageMatchReferenceText;
		private System.Windows.Forms.DataGridView m_dataGridReferenceText;
		private System.Windows.Forms.Button m_btnApplyReferenceTextMatches;
		private System.Windows.Forms.Button m_btnMoveReferenceTextDown;
		private System.Windows.Forms.Button m_btnMoveReferenceTextUp;
		private System.Windows.Forms.ToolStrip m_toolStripMatchReferenceText;
		private System.Windows.Forms.ToolStripSplitButton m_btnInsertHeSaid;
		private System.Windows.Forms.ToolStripMenuItem m_menuInsertIntoSelectedRowOnly;
		private System.Windows.Forms.ToolStripMenuItem m_menuInsertIntoAllEmptyCells;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMatchReferenceTextButtons;
		private System.Windows.Forms.FlowLayoutPanel m_flowLayoutPanelMatchReferenceTextLeftButtons;
		private System.Windows.Forms.Button m_btnReset;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonSelectCharacter;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonMatchReferenceText;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.Label m_lblReferenceText;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuRefTextCell;
		private System.Windows.Forms.ToolStripMenuItem m_ContextMenuItemSplitText;
		private System.Windows.Forms.ToolStripMenuItem m_RefTextContextMenuItemMoveUp;
		private System.Windows.Forms.ToolStripMenuItem m_RefTextContextMenuItemMoveDown;
		private System.Windows.Forms.ToolStripMenuItem m_ContextMenuItemInsertHeSaid;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacterOrDeliveryCell;
		private System.Windows.Forms.ToolStripMenuItem m_ContextMenuItemAddCharacterOrDelivery;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.DataGridViewComboBoxColumn colCharacter;
		private System.Windows.Forms.DataGridViewTextBoxColumn colPrimary;
		private System.Windows.Forms.DataGridViewTextBoxColumn colEnglish;
		private System.Windows.Forms.DataGridViewComboBoxColumn colDelivery;
		private System.Windows.Forms.ToolStripMenuItem m_CharacterOrDeliveryContextMenuItemMoveUp;
		private System.Windows.Forms.ToolStripMenuItem m_CharacterOrDeliveryContextMenuItemMoveDown;
	}
}