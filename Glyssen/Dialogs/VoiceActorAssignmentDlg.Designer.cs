using System;
using System.Windows.Forms;
using Glyssen.Controls;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace Glyssen.Dialogs
{
	partial class VoiceActorAssignmentDlg
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
				L10NSharp.UI.LocalizeItemDlg.StringsLocalized -= HandleStringsLocalized;
				if (m_hyperlinkFont != null)
					m_hyperlinkFont.Dispose();
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			this.m_contextMenuCharacters = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_menuItemCreateNewGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuItemAssignToCameoActor = new System.Windows.Forms.ToolStripMenuItem();
			this.m_cameoActorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuItemMoveToAnotherGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_contextMenuCharacterGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_unAssignActorFromGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_AddCharacterToGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_undoButton = new System.Windows.Forms.ToolStripButton();
			this.m_redoButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_editActorsButton = new System.Windows.Forms.ToolStripButton();
			this.m_splitSelectedGroupButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripLabelFindCharacter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripTextBoxFindCharacter = new System.Windows.Forms.ToolStripTextBox();
			this.m_toolStripButtonFindNextMatchingCharacter = new System.Windows.Forms.ToolStripButton();
			this.m_characterDetailsGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.CharacterDetailsIdCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsGenderCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsAgeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_lblMovePendingInfo = new System.Windows.Forms.Label();
			this.m_btnCancelMove = new System.Windows.Forms.Button();
			this.m_btnMove = new System.Windows.Forms.Button();
			this.m_lblMoveInstr = new System.Windows.Forms.Label();
			this.m_linkLabelShowHideDetails = new System.Windows.Forms.LinkLabel();
			this.m_characterGroupGrid = new System.Windows.Forms.DataGridView();
			this.GroupIdCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterIdsCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.AttributesCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.EstimatedHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VoiceActorCol = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_lblFewerOrMore = new System.Windows.Forms.Label();
			this.m_linkVoiceActorList = new System.Windows.Forms.LinkLabel();
			this.m_lblInstructions2 = new System.Windows.Forms.Label();
			this.m_linkClose = new System.Windows.Forms.LinkLabel();
			this.m_linkPrint = new System.Windows.Forms.LinkLabel();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelLowerRight = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelMove = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelVoiceActorList = new System.Windows.Forms.TableLayoutPanel();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewMultiColumnComboBoxColumn1 = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_contextMenuCharacters.SuspendLayout();
			this.m_contextMenuCharacterGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
			this.m_menuStrip.SuspendLayout();
			this.m_tableLayoutPanel.SuspendLayout();
			this.m_tableLayoutPanelLowerRight.SuspendLayout();
			this.m_tableLayoutPanelMove.SuspendLayout();
			this.m_tableLayoutPanelVoiceActorList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_contextMenuCharacters
			// 
			this.glyssenColorPalette.SetBackColor(this.m_contextMenuCharacters, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_contextMenuCharacters, Glyssen.Utilities.GlyssenColors.Default);
			this.m_contextMenuCharacters.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuItemCreateNewGroup,
            this.m_menuItemAssignToCameoActor,
            this.m_menuItemMoveToAnotherGroup});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacters, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacters, "DialogBoxes.VoiceActorAssignmentDlg.m_contextMenuCharacters.m_contextMenuCharacte" +
        "rs");
			this.m_contextMenuCharacters.Name = "m_contextMenuCharacters";
			this.m_contextMenuCharacters.Size = new System.Drawing.Size(322, 70);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_contextMenuCharacters, false);
			this.m_contextMenuCharacters.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacters_Opening);
			// 
			// m_menuItemCreateNewGroup
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuItemCreateNewGroup, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_menuItemCreateNewGroup, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_menuItemCreateNewGroup.Image = global::Glyssen.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuItemCreateNewGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemCreateNewGroup, "DialogBoxes.VoiceActorAssignmentDlg.m_menuItemCreateNewGroup");
			this.m_menuItemCreateNewGroup.Name = "m_menuItemCreateNewGroup";
			this.m_menuItemCreateNewGroup.Size = new System.Drawing.Size(321, 22);
			this.m_menuItemCreateNewGroup.Text = "Create a new group with the selected character";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuItemCreateNewGroup, false);
			this.m_menuItemCreateNewGroup.Click += new System.EventHandler(this.m_menuItemCreateNewGroup_Click);
			// 
			// m_menuItemAssignToCameoActor
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuItemAssignToCameoActor, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_menuItemAssignToCameoActor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_cameoActorToolStripMenuItem});
			this.glyssenColorPalette.SetForeColor(this.m_menuItemAssignToCameoActor, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_menuItemAssignToCameoActor.Image = global::Glyssen.Properties.Resources.CameoStar;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemAssignToCameoActor, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemAssignToCameoActor, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemAssignToCameoActor, "DialogBoxes.VoiceActorAssignmentDlg.AssignToCameoActor");
			this.m_menuItemAssignToCameoActor.Name = "m_menuItemAssignToCameoActor";
			this.m_menuItemAssignToCameoActor.Size = new System.Drawing.Size(321, 22);
			this.m_menuItemAssignToCameoActor.Text = "Assign selected character to cameo actor";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuItemAssignToCameoActor, false);
			// 
			// m_cameoActorToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_cameoActorToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_cameoActorToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cameoActorToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cameoActorToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cameoActorToolStripMenuItem, "DialogBoxes.cameoActorToolStripMenuItem");
			this.m_cameoActorToolStripMenuItem.Name = "m_cameoActorToolStripMenuItem";
			this.m_cameoActorToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.m_cameoActorToolStripMenuItem.Text = "CameoActor";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_cameoActorToolStripMenuItem, false);
			this.m_cameoActorToolStripMenuItem.Click += new System.EventHandler(this.HandleAssignToCameoActorClick);
			// 
			// m_menuItemMoveToAnotherGroup
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuItemMoveToAnotherGroup, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_menuItemMoveToAnotherGroup, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_menuItemMoveToAnotherGroup.Image = global::Glyssen.Properties.Resources.MoveArrow;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemMoveToAnotherGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemMoveToAnotherGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuItemMoveToAnotherGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemMoveToAnotherGroup, "DialogBoxes.VoiceActorAssignmentDlg.MoveSelectedCharacterToAnotherGroup");
			this.m_menuItemMoveToAnotherGroup.Name = "m_menuItemMoveToAnotherGroup";
			this.m_menuItemMoveToAnotherGroup.Size = new System.Drawing.Size(321, 22);
			this.m_menuItemMoveToAnotherGroup.Text = "Move selected character to another group...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuItemMoveToAnotherGroup, false);
			this.m_menuItemMoveToAnotherGroup.Click += new System.EventHandler(this.m_menuItemMoveToAnotherGroup_Click);
			// 
			// m_contextMenuCharacterGroups
			// 
			this.glyssenColorPalette.SetBackColor(this.m_contextMenuCharacterGroups, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_contextMenuCharacterGroups, Glyssen.Utilities.GlyssenColors.Default);
			this.m_contextMenuCharacterGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_unAssignActorFromGroupToolStripMenuItem,
            this.m_splitGroupToolStripMenuItem,
            this.m_AddCharacterToGroupToolStripMenuItem});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacterGroups, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacterGroups, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuCharacterGroups.Name = "m_contextMenuCharacterGroups";
			this.m_contextMenuCharacterGroups.Size = new System.Drawing.Size(248, 70);
			this.glyssenColorPalette.SetUsePaletteColors(this.m_contextMenuCharacterGroups, false);
			this.m_contextMenuCharacterGroups.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacterGroups_Opening);
			// 
			// m_unAssignActorFromGroupToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_unAssignActorFromGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_unAssignActorFromGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_unAssignActorFromGroupToolStripMenuItem.Image = global::Glyssen.Properties.Resources.RemoveActor;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_unAssignActorFromGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment");
			this.m_unAssignActorFromGroupToolStripMenuItem.Name = "m_unAssignActorFromGroupToolStripMenuItem";
			this.m_unAssignActorFromGroupToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.m_unAssignActorFromGroupToolStripMenuItem.Text = "Remove Voice Actor Assignment";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_unAssignActorFromGroupToolStripMenuItem, false);
			this.m_unAssignActorFromGroupToolStripMenuItem.Click += new System.EventHandler(this.m_unAssignActorFromGroupToolStripMenuItem_Click);
			// 
			// m_splitGroupToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_splitGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_splitGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_splitGroupToolStripMenuItem.Image = global::Glyssen.Properties.Resources.splitGroup;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.SplitSelectedGroup");
			this.m_splitGroupToolStripMenuItem.Name = "m_splitGroupToolStripMenuItem";
			this.m_splitGroupToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.m_splitGroupToolStripMenuItem.Text = "Split Group...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_splitGroupToolStripMenuItem, false);
			this.m_splitGroupToolStripMenuItem.Click += new System.EventHandler(this.HandleSplitSelectedGroupClick);
			// 
			// m_AddCharacterToGroupToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.m_AddCharacterToGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_AddCharacterToGroupToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_AddCharacterToGroupToolStripMenuItem.Image = global::Glyssen.Properties.Resources.AddCharacter;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_AddCharacterToGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_AddCharacterToGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_AddCharacterToGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.AddCharacterToGroupToolStripMenuItem");
			this.m_AddCharacterToGroupToolStripMenuItem.Name = "m_AddCharacterToGroupToolStripMenuItem";
			this.m_AddCharacterToGroupToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.m_AddCharacterToGroupToolStripMenuItem.Text = "Add character to this group...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_AddCharacterToGroupToolStripMenuItem, false);
			this.m_AddCharacterToGroupToolStripMenuItem.Click += new System.EventHandler(this.HandleAddCharacterToGroupClick);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_toolStrip
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStrip, Glyssen.Utilities.GlyssenColors.Default);
			this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_undoButton,
            this.m_redoButton,
            this.toolStripSeparator1,
            this.m_editActorsButton,
            this.m_splitSelectedGroupButton,
            this.toolStripSeparator2,
            this.m_toolStripLabelFindCharacter,
            this.m_toolStripTextBoxFindCharacter,
            this.m_toolStripButtonFindNextMatchingCharacter});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.VoiceActorAssignmentDlg.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 24);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.m_toolStrip.Size = new System.Drawing.Size(859, 25);
			this.m_toolStrip.TabIndex = 7;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStrip, false);
			// 
			// m_undoButton
			// 
			this.glyssenColorPalette.SetBackColor(this.m_undoButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_undoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_undoButton.Enabled = false;
			this.glyssenColorPalette.SetForeColor(this.m_undoButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_undoButton.Image = global::Glyssen.Properties.Resources.undo;
			this.m_undoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_undoButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_undoButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_undoButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.UndoButton");
			this.m_undoButton.Name = "m_undoButton";
			this.m_undoButton.Size = new System.Drawing.Size(23, 22);
			this.m_undoButton.Text = "Undo";
			this.m_undoButton.ToolTipText = "Undo {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_undoButton, false);
			this.m_undoButton.Click += new System.EventHandler(this.HandleUndoButtonClick);
			this.m_undoButton.MouseEnter += new System.EventHandler(this.SetUndoOrRedoButtonToolTip);
			// 
			// m_redoButton
			// 
			this.glyssenColorPalette.SetBackColor(this.m_redoButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_redoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_redoButton.Enabled = false;
			this.glyssenColorPalette.SetForeColor(this.m_redoButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_redoButton.Image = global::Glyssen.Properties.Resources.redo;
			this.m_redoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_redoButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_redoButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_redoButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.RedoButton");
			this.m_redoButton.Name = "m_redoButton";
			this.m_redoButton.Size = new System.Drawing.Size(23, 22);
			this.m_redoButton.Text = "Redo";
			this.m_redoButton.ToolTipText = "Redo {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_redoButton, false);
			this.m_redoButton.Click += new System.EventHandler(this.HandleRedoButtonClick);
			this.m_redoButton.MouseEnter += new System.EventHandler(this.SetUndoOrRedoButtonToolTip);
			// 
			// toolStripSeparator1
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.toolStripSeparator1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator1, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator1, false);
			// 
			// m_editActorsButton
			// 
			this.m_editActorsButton.AutoToolTip = false;
			this.glyssenColorPalette.SetBackColor(this.m_editActorsButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_editActorsButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_editActorsButton.Image = global::Glyssen.Properties.Resources.people;
			this.m_editActorsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_editActorsButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_editActorsButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_editActorsButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.VoiceActorList");
			this.m_editActorsButton.Name = "m_editActorsButton";
			this.m_editActorsButton.Size = new System.Drawing.Size(118, 22);
			this.m_editActorsButton.Text = "Voice Actor List...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_editActorsButton, false);
			this.m_editActorsButton.Click += new System.EventHandler(this.HandleEditVoiceActorsClick);
			// 
			// m_splitSelectedGroupButton
			// 
			this.m_splitSelectedGroupButton.AutoToolTip = false;
			this.glyssenColorPalette.SetBackColor(this.m_splitSelectedGroupButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_splitSelectedGroupButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_splitSelectedGroupButton.Image = global::Glyssen.Properties.Resources.splitGroup;
			this.m_splitSelectedGroupButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitSelectedGroupButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitSelectedGroupButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitSelectedGroupButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.SplitSelectedGroup");
			this.m_splitSelectedGroupButton.Name = "m_splitSelectedGroupButton";
			this.m_splitSelectedGroupButton.Size = new System.Drawing.Size(142, 22);
			this.m_splitSelectedGroupButton.Text = "Split Selected Group...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_splitSelectedGroupButton, false);
			this.m_splitSelectedGroupButton.Click += new System.EventHandler(this.HandleSplitSelectedGroupClick);
			// 
			// toolStripSeparator2
			// 
			this.glyssenColorPalette.SetBackColor(this.toolStripSeparator2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.toolStripSeparator2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.glyssenColorPalette.SetForeColor(this.toolStripSeparator2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			this.glyssenColorPalette.SetUsePaletteColors(this.toolStripSeparator2, false);
			// 
			// m_toolStripLabelFindCharacter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripLabelFindCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStripLabelFindCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripLabelFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripLabelFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripLabelFindCharacter, "DialogBoxes.VoiceActorAssignmentDlg.FindCharacter");
			this.m_toolStripLabelFindCharacter.Name = "m_toolStripLabelFindCharacter";
			this.m_toolStripLabelFindCharacter.Size = new System.Drawing.Size(87, 22);
			this.m_toolStripLabelFindCharacter.Text = "Find Character:";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripLabelFindCharacter, false);
			// 
			// m_toolStripTextBoxFindCharacter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripTextBoxFindCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_toolStripTextBoxFindCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripTextBoxFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripTextBoxFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripTextBoxFindCharacter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripTextBoxFindCharacter, "DialogBoxes.VoiceActorAssignmentDlg.m_toolStripTextBoxFindCharacter");
			this.m_toolStripTextBoxFindCharacter.Name = "m_toolStripTextBoxFindCharacter";
			this.m_toolStripTextBoxFindCharacter.Size = new System.Drawing.Size(120, 25);
			this.m_toolStripTextBoxFindCharacter.ToolTipText = "Begin typing a character ID to find the group that contains it";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripTextBoxFindCharacter, false);
			this.m_toolStripTextBoxFindCharacter.TextChanged += new System.EventHandler(this.m_toolStripTextBoxFindCharacter_TextChanged);
			// 
			// m_toolStripButtonFindNextMatchingCharacter
			// 
			this.glyssenColorPalette.SetBackColor(this.m_toolStripButtonFindNextMatchingCharacter, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_toolStripButtonFindNextMatchingCharacter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonFindNextMatchingCharacter.Enabled = false;
			this.glyssenColorPalette.SetForeColor(this.m_toolStripButtonFindNextMatchingCharacter, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_toolStripButtonFindNextMatchingCharacter.Image = global::Glyssen.Properties.Resources.search_glyph;
			this.m_toolStripButtonFindNextMatchingCharacter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonFindNextMatchingCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonFindNextMatchingCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonFindNextMatchingCharacter, "DialogBoxes.VoiceActorAssignmentDlg.FindNextMatch");
			this.m_toolStripButtonFindNextMatchingCharacter.Name = "m_toolStripButtonFindNextMatchingCharacter";
			this.m_toolStripButtonFindNextMatchingCharacter.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonFindNextMatchingCharacter.Text = "Find next match";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripButtonFindNextMatchingCharacter, false);
			this.m_toolStripButtonFindNextMatchingCharacter.Click += new System.EventHandler(this.m_toolStripButtonFindNextMatchingCharacter_Click);
			// 
			// m_characterDetailsGrid
			// 
			this.m_characterDetailsGrid.AllowUserToAddRows = false;
			this.m_characterDetailsGrid.AllowUserToDeleteRows = false;
			this.m_characterDetailsGrid.AllowUserToOrderColumns = true;
			this.m_characterDetailsGrid.AllowUserToResizeRows = false;
			this.m_characterDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_characterDetailsGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.glyssenColorPalette.SetBackColor(this.m_characterDetailsGrid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_characterDetailsGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_characterDetailsGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_characterDetailsGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_characterDetailsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.m_characterDetailsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_characterDetailsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CharacterDetailsIdCol,
            this.CharacterDetailsGenderCol,
            this.CharacterDetailsAgeCol,
            this.CharacterDetailsHoursCol});
			this.m_characterDetailsGrid.ContextMenuStrip = this.m_contextMenuCharacters;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterDetailsGrid.DefaultCellStyle = dataGridViewCellStyle3;
			this.m_characterDetailsGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterDetailsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_characterDetailsGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.glyssenColorPalette.SetForeColor(this.m_characterDetailsGrid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_characterDetailsGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterDetailsGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterDetailsGrid, "DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails");
			this.m_characterDetailsGrid.Location = new System.Drawing.Point(0, 263);
			this.m_characterDetailsGrid.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.m_characterDetailsGrid.MultiSelect = false;
			this.m_characterDetailsGrid.Name = "m_characterDetailsGrid";
			this.m_characterDetailsGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_characterDetailsGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_characterDetailsGrid.RowHeadersVisible = false;
			this.m_characterDetailsGrid.RowHeadersWidth = 22;
			this.m_characterDetailsGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_characterDetailsGrid.SelectedCellBackColor = System.Drawing.Color.Empty;
			this.m_characterDetailsGrid.SelectedCellForeColor = System.Drawing.Color.Empty;
			this.m_characterDetailsGrid.SelectedRowBackColor = System.Drawing.Color.Empty;
			this.m_characterDetailsGrid.SelectedRowForeColor = System.Drawing.Color.Empty;
			this.m_characterDetailsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_characterDetailsGrid.ShowWaterMarkWhenDirty = false;
			this.m_characterDetailsGrid.Size = new System.Drawing.Size(583, 123);
			this.m_characterDetailsGrid.TabIndex = 8;
			this.m_characterDetailsGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_characterDetailsGrid, false);
			this.m_characterDetailsGrid.VirtualMode = true;
			this.m_characterDetailsGrid.WaterMark = "!";
			this.m_characterDetailsGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleGridCellMouseDown);
			this.m_characterDetailsGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_characterDetailsGrid_CellValueNeeded);
			// 
			// CharacterDetailsIdCol
			// 
			this.CharacterDetailsIdCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.CharacterDetailsIdCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.CharacterId!Character" +
    " ID";
			this.CharacterDetailsIdCol.MinimumWidth = 50;
			this.CharacterDetailsIdCol.Name = "CharacterDetailsIdCol";
			this.CharacterDetailsIdCol.ReadOnly = true;
			// 
			// CharacterDetailsGenderCol
			// 
			this.CharacterDetailsGenderCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.CharacterDetailsGenderCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Gender!Gender";
			this.CharacterDetailsGenderCol.Name = "CharacterDetailsGenderCol";
			this.CharacterDetailsGenderCol.ReadOnly = true;
			this.CharacterDetailsGenderCol.Width = 449;
			// 
			// CharacterDetailsAgeCol
			// 
			this.CharacterDetailsAgeCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.CharacterDetailsAgeCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Age!Age";
			this.CharacterDetailsAgeCol.Name = "CharacterDetailsAgeCol";
			this.CharacterDetailsAgeCol.ReadOnly = true;
			this.CharacterDetailsAgeCol.Width = 415;
			// 
			// CharacterDetailsHoursCol
			// 
			this.CharacterDetailsHoursCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle2.Format = "N2";
			this.CharacterDetailsHoursCol.DefaultCellStyle = dataGridViewCellStyle2;
			this.CharacterDetailsHoursCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Hours!Hours";
			this.CharacterDetailsHoursCol.Name = "CharacterDetailsHoursCol";
			this.CharacterDetailsHoursCol.ReadOnly = true;
			this.CharacterDetailsHoursCol.Width = 437;
			// 
			// m_lblMovePendingInfo
			// 
			this.m_lblMovePendingInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblMovePendingInfo.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lblMovePendingInfo, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMove.SetColumnSpan(this.m_lblMovePendingInfo, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblMovePendingInfo, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblMovePendingInfo, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblMovePendingInfo, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblMovePendingInfo, "DialogBoxes.VoiceActorAssignmentDlg.MovePendingLabel");
			this.m_lblMovePendingInfo.Location = new System.Drawing.Point(8, 8);
			this.m_lblMovePendingInfo.Margin = new System.Windows.Forms.Padding(8, 8, 8, 0);
			this.m_lblMovePendingInfo.Name = "m_lblMovePendingInfo";
			this.m_lblMovePendingInfo.Size = new System.Drawing.Size(222, 13);
			this.m_lblMovePendingInfo.TabIndex = 0;
			this.m_lblMovePendingInfo.Text = "Move pending for {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblMovePendingInfo, false);
			// 
			// m_btnCancelMove
			// 
			this.m_btnCancelMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancelMove, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancelMove, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancelMove, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancelMove, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancelMove, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancelMove, "Common.Cancel");
			this.m_btnCancelMove.Location = new System.Drawing.Point(36, 87);
			this.m_btnCancelMove.Margin = new System.Windows.Forms.Padding(8);
			this.m_btnCancelMove.Name = "m_btnCancelMove";
			this.m_btnCancelMove.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancelMove.TabIndex = 1;
			this.m_btnCancelMove.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancelMove, false);
			this.m_btnCancelMove.UseVisualStyleBackColor = true;
			this.m_btnCancelMove.Click += new System.EventHandler(this.m_btnCancelMove_Click);
			// 
			// m_btnMove
			// 
			this.m_btnMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.glyssenColorPalette.SetBackColor(this.m_btnMove, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnMove.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnMove, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnMove, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnMove, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnMove, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnMove, "DialogBoxes.VoiceActorAssignmentDlg.MoveButton");
			this.m_btnMove.Location = new System.Drawing.Point(127, 87);
			this.m_btnMove.Margin = new System.Windows.Forms.Padding(8);
			this.m_btnMove.Name = "m_btnMove";
			this.m_btnMove.Size = new System.Drawing.Size(75, 23);
			this.m_btnMove.TabIndex = 2;
			this.m_btnMove.Text = "&Move";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnMove, false);
			this.m_btnMove.UseVisualStyleBackColor = true;
			this.m_btnMove.Click += new System.EventHandler(this.m_btnMove_Click);
			// 
			// m_lblMoveInstr
			// 
			this.m_lblMoveInstr.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_lblMoveInstr, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMove.SetColumnSpan(this.m_lblMoveInstr, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblMoveInstr, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblMoveInstr, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblMoveInstr, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblMoveInstr, "DialogBoxes.MoveCharacterInstructions");
			this.m_lblMoveInstr.Location = new System.Drawing.Point(8, 23);
			this.m_lblMoveInstr.Margin = new System.Windows.Forms.Padding(8, 2, 8, 8);
			this.m_lblMoveInstr.Name = "m_lblMoveInstr";
			this.m_lblMoveInstr.Size = new System.Drawing.Size(212, 26);
			this.m_lblMoveInstr.TabIndex = 3;
			this.m_lblMoveInstr.Text = "Select the desired destination group above and then click Move.";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblMoveInstr, false);
			// 
			// m_linkLabelShowHideDetails
			// 
			this.m_linkLabelShowHideDetails.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkLabelShowHideDetails.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_linkLabelShowHideDetails.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkLabelShowHideDetails.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkLabelShowHideDetails.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_linkLabelShowHideDetails.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkLabelShowHideDetails.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkLabelShowHideDetails, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkLabelShowHideDetails, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkLabelShowHideDetails, "DialogBoxes.VoiceActorAssignmentDlg.ShowCharacterDetailsLink");
			this.m_linkLabelShowHideDetails.Location = new System.Drawing.Point(3, 238);
			this.m_linkLabelShowHideDetails.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
			this.m_linkLabelShowHideDetails.Name = "m_linkLabelShowHideDetails";
			this.m_linkLabelShowHideDetails.Size = new System.Drawing.Size(129, 13);
			this.m_linkLabelShowHideDetails.TabIndex = 11;
			this.m_linkLabelShowHideDetails.TabStop = true;
			this.m_linkLabelShowHideDetails.Text = "Show details for {0} group";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkLabelShowHideDetails, true);
			this.m_linkLabelShowHideDetails.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkLabelShowHideDetails, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkLabelShowHideDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleShowOrHideCharacterDetails_Click);
			// 
			// m_characterGroupGrid
			// 
			this.m_characterGroupGrid.AllowDrop = true;
			this.m_characterGroupGrid.AllowUserToAddRows = false;
			this.m_characterGroupGrid.AllowUserToDeleteRows = false;
			this.m_characterGroupGrid.AllowUserToOrderColumns = true;
			this.m_characterGroupGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.m_characterGroupGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
			this.m_characterGroupGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_characterGroupGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.m_characterGroupGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.glyssenColorPalette.SetBackColor(this.m_characterGroupGrid, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_characterGroupGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_characterGroupGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_characterGroupGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
			this.m_characterGroupGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GroupIdCol,
            this.CharacterIdsCol,
            this.AttributesCol,
            this.CharStatusCol,
            this.EstimatedHoursCol,
            this.VoiceActorCol});
			this.m_tableLayoutPanel.SetColumnSpan(this.m_characterGroupGrid, 2);
			this.m_characterGroupGrid.ContextMenuStrip = this.m_contextMenuCharacterGroups;
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle7.NullValue = null;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.DefaultCellStyle = dataGridViewCellStyle7;
			this.m_characterGroupGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.glyssenColorPalette.SetForeColor(this.m_characterGroupGrid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_characterGroupGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_characterGroupGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterGroupGrid, "DialogBoxes.VoiceActorAssignmentDlg.betterGrid1");
			this.m_characterGroupGrid.Location = new System.Drawing.Point(0, 33);
			this.m_characterGroupGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_characterGroupGrid.MultiSelect = false;
			this.m_characterGroupGrid.Name = "m_characterGroupGrid";
			this.m_characterGroupGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_characterGroupGrid.RowHeadersVisible = false;
			this.m_characterGroupGrid.RowHeadersWidth = 22;
			this.m_characterGroupGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_characterGroupGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_characterGroupGrid.Size = new System.Drawing.Size(833, 192);
			this.m_characterGroupGrid.TabIndex = 6;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_characterGroupGrid, false);
			this.m_characterGroupGrid.VirtualMode = true;
			this.m_characterGroupGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellContentClick);
			this.m_characterGroupGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellEnter);
			this.m_characterGroupGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.m_characterGroupGrid_CellFormatting);
			this.m_characterGroupGrid.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellLeave);
			this.m_characterGroupGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleGridCellMouseDown);
			this.m_characterGroupGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_characterGroupGrid_CellValueNeeded);
			this.m_characterGroupGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_characterGroupGrid_CellValuePushed);
			this.m_characterGroupGrid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleGridColumnHeaderMouseClick);
			this.m_characterGroupGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.m_characterGroupGrid_DataError);
			this.m_characterGroupGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.m_characterGroupGrid_EditingControlShowing);
			this.m_characterGroupGrid.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.m_characterGroupGrid_RowPrePaint);
			this.m_characterGroupGrid.SelectionChanged += new System.EventHandler(this.m_characterGroupGrid_SelectionChanged);
			this.m_characterGroupGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragDrop);
			this.m_characterGroupGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragOver);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			// 
			// GroupIdCol
			// 
			this.GroupIdCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.GroupIdCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Group ID";
			this.GroupIdCol.Name = "GroupIdCol";
			this.GroupIdCol.ReadOnly = true;
			this.GroupIdCol.Width = 387;
			// 
			// CharacterIdsCol
			// 
			this.CharacterIdsCol.FillWeight = 150.8968F;
			this.CharacterIdsCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters In Group";
			this.CharacterIdsCol.Name = "CharacterIdsCol";
			this.CharacterIdsCol.ReadOnly = true;
			this.CharacterIdsCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.CharacterIdsCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// AttributesCol
			// 
			this.AttributesCol.FillWeight = 75.44839F;
			this.AttributesCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.AttributesCol.Name = "AttributesCol";
			this.AttributesCol.ReadOnly = true;
			// 
			// CharStatusCol
			// 
			this.CharStatusCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.CharStatusCol.FillWeight = 115.4822F;
			this.CharStatusCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Status!Status";
			this.CharStatusCol.Name = "CharStatusCol";
			this.CharStatusCol.ReadOnly = true;
			this.CharStatusCol.Visible = false;
			// 
			// EstimatedHoursCol
			// 
			this.EstimatedHoursCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle6.Format = "N2";
			dataGridViewCellStyle6.NullValue = null;
			this.EstimatedHoursCol.DefaultCellStyle = dataGridViewCellStyle6;
			this.EstimatedHoursCol.FillWeight = 37.7242F;
			this.EstimatedHoursCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.EstimatedHoursCol.Name = "EstimatedHoursCol";
			this.EstimatedHoursCol.ReadOnly = true;
			this.EstimatedHoursCol.Width = 348;
			// 
			// VoiceActorCol
			// 
			this.VoiceActorCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.VoiceActorCol.CategoryColumnName = "Category";
			this.VoiceActorCol.ColumnNames.Add("Name");
			this.VoiceActorCol.ColumnNames.Add("Gender");
			this.VoiceActorCol.ColumnNames.Add("Age");
			this.VoiceActorCol.ColumnNames.Add("Cameo");
			this.VoiceActorCol.EvenRowsBackColor = System.Drawing.Color.White;
			this.VoiceActorCol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.VoiceActorCol.FontForCategories = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VoiceActorCol.FontForUncategorizedItems = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.VoiceActorCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.VoiceActorAssigned!Voice Actor Assigne" +
    "d";
			this.VoiceActorCol.MaxDropDownItems = 20;
			this.VoiceActorCol.MinimumWidth = 50;
			this.VoiceActorCol.OddRowsBackColor = System.Drawing.Color.White;
			this.VoiceActorCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.VoiceActorCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.VoiceActorCol.Width = 502;
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_saveStatus, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_saveStatus.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_saveStatus, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorAssignmentDlg.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(733, 17);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 6;
			this.m_saveStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_saveStatus, true);
			// 
			// m_menuStrip
			// 
			this.glyssenColorPalette.SetBackColor(this.m_menuStrip, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_menuStrip, Glyssen.Utilities.GlyssenColors.Default);
			this.m_menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuStrip, "DialogBoxes.menuStrip1");
			this.m_menuStrip.Location = new System.Drawing.Point(0, 0);
			this.m_menuStrip.Name = "m_menuStrip";
			this.m_menuStrip.Size = new System.Drawing.Size(859, 24);
			this.m_menuStrip.TabIndex = 10;
			this.m_menuStrip.Text = "menuStrip1";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_menuStrip, false);
			// 
			// fileToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.fileToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.printToolStripMenuItem});
			this.glyssenColorPalette.SetForeColor(this.fileToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.fileToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.fileToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.fileToolStripMenuItem, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.fileToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.Menu.File");
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			this.glyssenColorPalette.SetUsePaletteColors(this.fileToolStripMenuItem, false);
			// 
			// saveAsToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.saveAsToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.saveAsToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.saveAsToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.saveAsToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.saveAsToolStripMenuItem, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.saveAsToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.Menu.SaveAs");
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			this.saveAsToolStripMenuItem.Text = "Save As...";
			this.glyssenColorPalette.SetUsePaletteColors(this.saveAsToolStripMenuItem, false);
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// printToolStripMenuItem
			// 
			this.glyssenColorPalette.SetBackColor(this.printToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.printToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.printToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.printToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.printToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.Menu.Print");
			this.printToolStripMenuItem.Name = "printToolStripMenuItem";
			this.printToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			this.printToolStripMenuItem.Text = "Print...";
			this.glyssenColorPalette.SetUsePaletteColors(this.printToolStripMenuItem, false);
			this.printToolStripMenuItem.Click += new System.EventHandler(this.HandlePrintClick);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.glyssenColorPalette.SetBackColor(this.helpToolStripMenuItem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.helpToolStripMenuItem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.helpToolStripMenuItem.Image = global::Glyssen.Properties.Resources.helpSmall;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.helpToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.helpToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.helpToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.Menu.Help");
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
			this.helpToolStripMenuItem.Text = "Help";
			this.glyssenColorPalette.SetUsePaletteColors(this.helpToolStripMenuItem, false);
			// 
			// m_lblFewerOrMore
			// 
			this.m_lblFewerOrMore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblFewerOrMore.AutoSize = true;
			this.m_lblFewerOrMore.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblFewerOrMore, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblFewerOrMore, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblFewerOrMore.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblFewerOrMore, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblFewerOrMore, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblFewerOrMore, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblFewerOrMore, "DialogBoxes.VoiceActorAssignmentDlg.FewerOrMore");
			this.m_lblFewerOrMore.Location = new System.Drawing.Point(3, 0);
			this.m_lblFewerOrMore.Name = "m_lblFewerOrMore";
			this.m_lblFewerOrMore.Size = new System.Drawing.Size(238, 13);
			this.m_lblFewerOrMore.TabIndex = 0;
			this.m_lblFewerOrMore.Text = "Have fewer or more voice actors? Click to edit";
			this.m_lblFewerOrMore.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblFewerOrMore, true);
			// 
			// m_linkVoiceActorList
			// 
			this.m_linkVoiceActorList.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkVoiceActorList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkVoiceActorList.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkVoiceActorList.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkVoiceActorList.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_linkVoiceActorList.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkVoiceActorList.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkVoiceActorList, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkVoiceActorList, "DialogBoxes.VoiceActorAssignmentDlg.VoiceActorList");
			this.m_linkVoiceActorList.Location = new System.Drawing.Point(3, 13);
			this.m_linkVoiceActorList.Name = "m_linkVoiceActorList";
			this.m_linkVoiceActorList.Size = new System.Drawing.Size(238, 13);
			this.m_linkVoiceActorList.TabIndex = 1;
			this.m_linkVoiceActorList.TabStop = true;
			this.m_linkVoiceActorList.Text = "Voice Actor List";
			this.m_linkVoiceActorList.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkVoiceActorList, true);
			this.m_linkVoiceActorList.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkVoiceActorList, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkVoiceActorList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleEditVoiceActorsClick);
			// 
			// m_lblInstructions2
			// 
			this.m_lblInstructions2.AutoSize = true;
			this.m_lblInstructions2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblInstructions2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblInstructions2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblInstructions2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInstructions2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInstructions2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInstructions2, "DialogBoxes.VoiceActorAssignmentDlg.Instructions.Line2");
			this.m_lblInstructions2.Location = new System.Drawing.Point(3, 17);
			this.m_lblInstructions2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_lblInstructions2.Name = "m_lblInstructions2";
			this.m_lblInstructions2.Size = new System.Drawing.Size(374, 13);
			this.m_lblInstructions2.TabIndex = 13;
			this.m_lblInstructions2.Text = "-or- In the right-hand column, enter / assign the voice actor to speak the roles." +
    "";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblInstructions2, true);
			// 
			// m_linkClose
			// 
			this.m_linkClose.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkClose.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkClose.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkClose.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.glyssenColorPalette.SetForeColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkClose.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetLinkColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkClose.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkClose, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkClose, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_linkClose, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkClose, "DialogBoxes.VoiceActorAssignmentDlg.Close");
			this.m_linkClose.Location = new System.Drawing.Point(815, 449);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(33, 13);
			this.m_linkClose.TabIndex = 11;
			this.m_linkClose.TabStop = true;
			this.m_linkClose.Text = "Close";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkClose, true);
			this.m_linkClose.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkClose, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkClose_LinkClicked);
			// 
			// m_linkPrint
			// 
			this.m_linkPrint.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetActiveLinkColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_linkPrint.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_linkPrint.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetDisabledLinkColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_linkPrint.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_linkPrint.ForeColor = System.Drawing.SystemColors.WindowText;
			this.glyssenColorPalette.SetForeColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_linkPrint.LinkArea = new System.Windows.Forms.LinkArea(0, 3);
			this.glyssenColorPalette.SetLinkColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_linkPrint.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkPrint, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkPrint, "{0} is the clickable text.  Its localizingId is DialogBoxes.VoiceActorAssignmentD" +
        "lg.Instructions.Line1.LinkText");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkPrint, "DialogBoxes.VoiceActorAssignmentDlg.Instructions.Line1.NonLink");
			this.m_linkPrint.Location = new System.Drawing.Point(3, 0);
			this.m_linkPrint.Name = "m_linkPrint";
			this.m_linkPrint.Size = new System.Drawing.Size(231, 17);
			this.m_linkPrint.TabIndex = 14;
			this.m_linkPrint.TabStop = true;
			this.m_linkPrint.Text = "{0} this list for use as you recruit voice actors.";
			this.m_linkPrint.UseCompatibleTextRendering = true;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_linkPrint, true);
			this.m_linkPrint.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.glyssenColorPalette.SetVisitedLinkColor(this.m_linkPrint, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_linkPrint.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandlePrintClick);
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanel.ColumnCount = 2;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
			this.m_tableLayoutPanel.Controls.Add(this.m_characterDetailsGrid, 0, 4);
			this.m_tableLayoutPanel.Controls.Add(this.m_linkLabelShowHideDetails, 0, 3);
			this.m_tableLayoutPanel.Controls.Add(this.m_characterGroupGrid, 0, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_lblInstructions2, 0, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_linkPrint, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_saveStatus, 1, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_tableLayoutPanelLowerRight, 1, 3);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanel, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(15, 52);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 5;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(833, 389);
			this.m_tableLayoutPanel.TabIndex = 9;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanel, false);
			// 
			// m_tableLayoutPanelLowerRight
			// 
			this.m_tableLayoutPanelLowerRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelLowerRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelLowerRight, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelLowerRight.ColumnCount = 1;
			this.m_tableLayoutPanelLowerRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelLowerRight.Controls.Add(this.m_tableLayoutPanelMove, 0, 1);
			this.m_tableLayoutPanelLowerRight.Controls.Add(this.m_tableLayoutPanelVoiceActorList, 0, 0);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelLowerRight, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelLowerRight.Location = new System.Drawing.Point(586, 228);
			this.m_tableLayoutPanelLowerRight.Name = "m_tableLayoutPanelLowerRight";
			this.m_tableLayoutPanelLowerRight.RowCount = 2;
			this.m_tableLayoutPanel.SetRowSpan(this.m_tableLayoutPanelLowerRight, 2);
			this.m_tableLayoutPanelLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelLowerRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelLowerRight.Size = new System.Drawing.Size(244, 158);
			this.m_tableLayoutPanelLowerRight.TabIndex = 15;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelLowerRight, false);
			// 
			// m_tableLayoutPanelMove
			// 
			this.m_tableLayoutPanelMove.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMove, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelMove.ColumnCount = 2;
			this.m_tableLayoutPanelMove.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelMove.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelMove.Controls.Add(this.m_lblMovePendingInfo, 0, 0);
			this.m_tableLayoutPanelMove.Controls.Add(this.m_btnMove, 1, 2);
			this.m_tableLayoutPanelMove.Controls.Add(this.m_lblMoveInstr, 0, 1);
			this.m_tableLayoutPanelMove.Controls.Add(this.m_btnCancelMove, 0, 2);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMove, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelMove.Location = new System.Drawing.Point(3, 37);
			this.m_tableLayoutPanelMove.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.m_tableLayoutPanelMove.Name = "m_tableLayoutPanelMove";
			this.m_tableLayoutPanelMove.RowCount = 3;
			this.m_tableLayoutPanelMove.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMove.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMove.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMove.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMove.Size = new System.Drawing.Size(238, 118);
			this.m_tableLayoutPanelMove.TabIndex = 4;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMove, false);
			this.m_tableLayoutPanelMove.Visible = false;
			// 
			// m_tableLayoutPanelVoiceActorList
			// 
			this.m_tableLayoutPanelVoiceActorList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelVoiceActorList.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelVoiceActorList, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutPanelVoiceActorList.ColumnCount = 1;
			this.m_tableLayoutPanelVoiceActorList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelVoiceActorList.Controls.Add(this.m_lblFewerOrMore, 0, 0);
			this.m_tableLayoutPanelVoiceActorList.Controls.Add(this.m_linkVoiceActorList, 0, 1);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelVoiceActorList, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutPanelVoiceActorList.Location = new System.Drawing.Point(0, 3);
			this.m_tableLayoutPanelVoiceActorList.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.m_tableLayoutPanelVoiceActorList.Name = "m_tableLayoutPanelVoiceActorList";
			this.m_tableLayoutPanelVoiceActorList.RowCount = 2;
			this.m_tableLayoutPanelVoiceActorList.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelVoiceActorList.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelVoiceActorList.Size = new System.Drawing.Size(244, 26);
			this.m_tableLayoutPanelVoiceActorList.TabIndex = 5;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelVoiceActorList, false);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.CharacterId!Character" +
    " ID";
			this.dataGridViewTextBoxColumn1.MinimumWidth = 50;
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn2.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Gender!Gender";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Age!Age";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle8.Format = "N2";
			this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle8;
			this.dataGridViewTextBoxColumn4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Hours!Hours";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.FillWeight = 150.8968F;
			this.dataGridViewTextBoxColumn5.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters In Group";
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.ReadOnly = true;
			this.dataGridViewTextBoxColumn5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.dataGridViewTextBoxColumn5.Width = 5;
			// 
			// dataGridViewTextBoxColumn6
			// 
			this.dataGridViewTextBoxColumn6.FillWeight = 75.44839F;
			this.dataGridViewTextBoxColumn6.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			this.dataGridViewTextBoxColumn6.ReadOnly = true;
			this.dataGridViewTextBoxColumn6.Width = 5;
			// 
			// dataGridViewTextBoxColumn7
			// 
			this.dataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn7.FillWeight = 115.4822F;
			this.dataGridViewTextBoxColumn7.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Status!Status";
			this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
			this.dataGridViewTextBoxColumn7.ReadOnly = true;
			this.dataGridViewTextBoxColumn7.Visible = false;
			// 
			// dataGridViewTextBoxColumn8
			// 
			this.dataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle9.Format = "N2";
			dataGridViewCellStyle9.NullValue = null;
			this.dataGridViewTextBoxColumn8.DefaultCellStyle = dataGridViewCellStyle9;
			this.dataGridViewTextBoxColumn8.FillWeight = 37.7242F;
			this.dataGridViewTextBoxColumn8.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
			this.dataGridViewTextBoxColumn8.ReadOnly = true;
			// 
			// dataGridViewMultiColumnComboBoxColumn1
			// 
			this.dataGridViewMultiColumnComboBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewMultiColumnComboBoxColumn1.CategoryColumnName = "Category";
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Name");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Gender");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Age");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Cameo");
			this.dataGridViewMultiColumnComboBoxColumn1.EvenRowsBackColor = System.Drawing.Color.White;
			this.dataGridViewMultiColumnComboBoxColumn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dataGridViewMultiColumnComboBoxColumn1.FontForCategories = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dataGridViewMultiColumnComboBoxColumn1.FontForUncategorizedItems = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dataGridViewMultiColumnComboBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.VoiceActorAssigned!Voice Actor Assigne" +
    "d";
			this.dataGridViewMultiColumnComboBoxColumn1.MaxDropDownItems = 20;
			this.dataGridViewMultiColumnComboBoxColumn1.MinimumWidth = 50;
			this.dataGridViewMultiColumnComboBoxColumn1.OddRowsBackColor = System.Drawing.Color.White;
			this.dataGridViewMultiColumnComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewMultiColumnComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.dataGridViewMultiColumnComboBoxColumn1.Width = 100;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(859, 477);
			this.Controls.Add(this.m_linkClose);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_menuStrip);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, "{0} is the project name");
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.VoiceActorAssignmentDlg.WindowTitle");
			this.MainMenuStrip = this.m_menuStrip;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(875, 480);
			this.Name = "VoiceActorAssignmentDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Roles for Voice Actors - {0}";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceActorAssignmentDlg_FormClosing);
			this.Load += new System.EventHandler(this.VoiceActorAssignmentDlg_Load);
			this.m_contextMenuCharacters.ResumeLayout(false);
			this.m_contextMenuCharacterGroups.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			this.m_menuStrip.ResumeLayout(false);
			this.m_menuStrip.PerformLayout();
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.m_tableLayoutPanelLowerRight.ResumeLayout(false);
			this.m_tableLayoutPanelLowerRight.PerformLayout();
			this.m_tableLayoutPanelMove.ResumeLayout(false);
			this.m_tableLayoutPanelMove.PerformLayout();
			this.m_tableLayoutPanelVoiceActorList.ResumeLayout(false);
			this.m_tableLayoutPanelVoiceActorList.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacterGroups;
		private System.Windows.Forms.ToolStripMenuItem m_unAssignActorFromGroupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_splitGroupToolStripMenuItem;
		private System.Windows.Forms.DataGridView m_characterGroupGrid;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacters;
		private System.Windows.Forms.ToolStripMenuItem m_menuItemCreateNewGroup;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripButton m_editActorsButton;
		private System.Windows.Forms.ToolStripButton m_splitSelectedGroupButton;
		private System.Windows.Forms.ToolStripButton m_undoButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton m_redoButton;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterDetailsGrid;
		private System.Windows.Forms.ToolStripMenuItem m_menuItemMoveToAnotherGroup;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFindCharacter;
		private System.Windows.Forms.ToolStripTextBox m_toolStripTextBoxFindCharacter;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonFindNextMatchingCharacter;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsIdCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsGenderCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsAgeCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsHoursCol;
		private System.Windows.Forms.Button m_btnMove;
		private System.Windows.Forms.Button m_btnCancelMove;
		private System.Windows.Forms.Label m_lblMovePendingInfo;
		private System.Windows.Forms.Label m_lblMoveInstr;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMove;
		private System.Windows.Forms.ToolStripMenuItem m_menuItemAssignToCameoActor;
		private System.Windows.Forms.ToolStripMenuItem m_cameoActorToolStripMenuItem;
		private System.Windows.Forms.LinkLabel m_linkLabelShowHideDetails;
		private ToolStripMenuItem m_AddCharacterToGroupToolStripMenuItem;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
		private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
		private DataGridViewMultiColumnComboBoxColumn dataGridViewMultiColumnComboBoxColumn1;
		private MenuStrip m_menuStrip;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem saveAsToolStripMenuItem;
		private ToolStripMenuItem helpToolStripMenuItem;
		private TableLayoutPanel m_tableLayoutPanelVoiceActorList;
		private Label m_lblFewerOrMore;
		private LinkLabel m_linkVoiceActorList;
		private ToolStripMenuItem printToolStripMenuItem;
		private Label m_lblInstructions2;
		private LinkLabel m_linkClose;
		private LinkLabel m_linkPrint;
		private TableLayoutPanel m_tableLayoutPanelLowerRight;
		private DataGridViewTextBoxColumn GroupIdCol;
		private DataGridViewTextBoxColumn CharacterIdsCol;
		private DataGridViewTextBoxColumn AttributesCol;
		private DataGridViewTextBoxColumn CharStatusCol;
		private DataGridViewTextBoxColumn EstimatedHoursCol;
		private DataGridViewMultiColumnComboBoxColumn VoiceActorCol;

	}
}