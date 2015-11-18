using System;
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_contextMenuCharacters = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_menuItemCreateNewGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuItemMoveToAnotherGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupCharacterDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_contextMenuCharacterGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_unAssignActorFromGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_undoButton = new System.Windows.Forms.ToolStripButton();
			this.m_redoButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_editActorsButton = new System.Windows.Forms.ToolStripButton();
			this.m_updateGroupsButton = new System.Windows.Forms.ToolStripButton();
			this.m_splitSelectedGroupButton = new System.Windows.Forms.ToolStripButton();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnShowHideDetails = new System.Windows.Forms.Button();
			this.m_characterDetailsGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.CharacterDetailsIdCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsGenderCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsAgeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_lblNoCharactersInGroup = new System.Windows.Forms.Label();
			this.m_characterGroupGrid = new Glyssen.Controls.AutoScrollGrid();
			this.CharacterIdsCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.AttributesCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.EstimatedHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VoiceActorCol = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.m_tableLayoutPanelCharacterDetails = new System.Windows.Forms.TableLayoutPanel();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_toolStripTextBoxFindCharacter = new System.Windows.Forms.ToolStripTextBox();
			this.m_toolStripLabelFindCharacter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripButtonFindNextMatchingCharacter = new System.Windows.Forms.ToolStripButton();
			this.m_contextMenuCharacters.SuspendLayout();
			this.m_contextMenuCharacterGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
			this.m_tableLayoutPanel.SuspendLayout();
			this.m_tableLayoutPanelCharacterDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.m_lblInstructions.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.VoiceActorAssignmentDlg.AssignActors");
			this.m_lblInstructions.Location = new System.Drawing.Point(12, 34);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(199, 13);
			this.m_lblInstructions.TabIndex = 0;
			this.m_lblInstructions.Text = "Assign Voice Actors to Character Groups";
			// 
			// m_contextMenuCharacters
			// 
			this.m_contextMenuCharacters.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuItemCreateNewGroup,
            this.m_menuItemMoveToAnotherGroup,
            this.m_splitGroupCharacterDetailsToolStripMenuItem});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacters, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacters, "DialogBoxes.VoiceActorAssignmentDlg.m_contextMenuCharacters.m_contextMenuCharacte" +
        "rs");
			this.m_contextMenuCharacters.Name = "m_contextMenuCharacters";
			this.m_contextMenuCharacters.Size = new System.Drawing.Size(322, 70);
			this.m_contextMenuCharacters.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacters_Opening);
			// 
			// m_menuItemCreateNewGroup
			// 
			this.m_menuItemCreateNewGroup.Image = global::Glyssen.Properties.Resources._112_Plus_Green_16x16_72;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuItemCreateNewGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemCreateNewGroup, "DialogBoxes.VoiceActorAssignmentDlg.m_menuItemCreateNewGroup");
			this.m_menuItemCreateNewGroup.Name = "m_menuItemCreateNewGroup";
			this.m_menuItemCreateNewGroup.Size = new System.Drawing.Size(321, 22);
			this.m_menuItemCreateNewGroup.Text = "Create a new group with the selected character";
			this.m_menuItemCreateNewGroup.Click += new System.EventHandler(this.m_menuItemCreateNewGroup_Click);
			// 
			// m_menuItemMoveToAnotherGroup
			// 
			this.m_menuItemMoveToAnotherGroup.Image = global::Glyssen.Properties.Resources.MoveArrow;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemMoveToAnotherGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemMoveToAnotherGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuItemMoveToAnotherGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemMoveToAnotherGroup, "DialogBoxes.VoiceActorAssignmentDlg.MoveSelectedCharacterToAnotherGroup");
			this.m_menuItemMoveToAnotherGroup.Name = "m_menuItemMoveToAnotherGroup";
			this.m_menuItemMoveToAnotherGroup.Size = new System.Drawing.Size(321, 22);
			this.m_menuItemMoveToAnotherGroup.Text = "Move selected character to another group...";
			this.m_menuItemMoveToAnotherGroup.Click += new System.EventHandler(this.m_menuItemMoveToAnotherGroup_Click);
			// 
			// m_splitGroupCharacterDetailsToolStripMenuItem
			// 
			this.m_splitGroupCharacterDetailsToolStripMenuItem.Image = global::Glyssen.Properties.Resources.splitGroup;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitGroupCharacterDetailsToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitGroupCharacterDetailsToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitGroupCharacterDetailsToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.SplitSelectedGroup");
			this.m_splitGroupCharacterDetailsToolStripMenuItem.Name = "m_splitGroupCharacterDetailsToolStripMenuItem";
			this.m_splitGroupCharacterDetailsToolStripMenuItem.Size = new System.Drawing.Size(321, 22);
			this.m_splitGroupCharacterDetailsToolStripMenuItem.Text = "Split group...";
			this.m_splitGroupCharacterDetailsToolStripMenuItem.Click += new System.EventHandler(this.HandleSplitSelectedGroupClick);
			// 
			// m_contextMenuCharacterGroups
			// 
			this.m_contextMenuCharacterGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_unAssignActorFromGroupToolStripMenuItem,
            this.m_splitGroupToolStripMenuItem});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacterGroups, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacterGroups, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuCharacterGroups.Name = "m_contextMenuCharacterGroups";
			this.m_contextMenuCharacterGroups.Size = new System.Drawing.Size(248, 48);
			this.m_contextMenuCharacterGroups.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacterGroups_Opening);
			// 
			// m_unAssignActorFromGroupToolStripMenuItem
			// 
			this.m_unAssignActorFromGroupToolStripMenuItem.Image = global::Glyssen.Properties.Resources.RemoveActor;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_unAssignActorFromGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.RemoveVoiceActorAssignment");
			this.m_unAssignActorFromGroupToolStripMenuItem.Name = "m_unAssignActorFromGroupToolStripMenuItem";
			this.m_unAssignActorFromGroupToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.m_unAssignActorFromGroupToolStripMenuItem.Text = "Remove Voice Actor Assignment";
			this.m_unAssignActorFromGroupToolStripMenuItem.Click += new System.EventHandler(this.m_unAssignActorFromGroupToolStripMenuItem_Click);
			// 
			// m_splitGroupToolStripMenuItem
			// 
			this.m_splitGroupToolStripMenuItem.Image = global::Glyssen.Properties.Resources.splitGroup;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.SplitSelectedGroup");
			this.m_splitGroupToolStripMenuItem.Name = "m_splitGroupToolStripMenuItem";
			this.m_splitGroupToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
			this.m_splitGroupToolStripMenuItem.Text = "Split Group...";
			this.m_splitGroupToolStripMenuItem.Click += new System.EventHandler(this.HandleSplitSelectedGroupClick);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_undoButton,
            this.m_redoButton,
            this.toolStripSeparator1,
            this.m_editActorsButton,
            this.m_updateGroupsButton,
            this.m_splitSelectedGroupButton,
            this.toolStripSeparator2,
            this.m_toolStripLabelFindCharacter,
            this.m_toolStripTextBoxFindCharacter,
            this.m_toolStripButtonFindNextMatchingCharacter});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.VoiceActorAssignmentDlg.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.m_toolStrip.Size = new System.Drawing.Size(859, 25);
			this.m_toolStrip.TabIndex = 7;
			// 
			// m_undoButton
			// 
			this.m_undoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_undoButton.Enabled = false;
			this.m_undoButton.Image = global::Glyssen.Properties.Resources.undo;
			this.m_undoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_undoButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_undoButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_undoButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.UndoButton");
			this.m_undoButton.Name = "m_undoButton";
			this.m_undoButton.Size = new System.Drawing.Size(23, 22);
			this.m_undoButton.Text = "Undo";
			this.m_undoButton.ToolTipText = "Undo {0}";
			this.m_undoButton.Click += new System.EventHandler(this.HandleUndoButtonClick);
			this.m_undoButton.MouseEnter += new System.EventHandler(this.SetUndoOrRedoButtonToolTip);
			// 
			// m_redoButton
			// 
			this.m_redoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_redoButton.Enabled = false;
			this.m_redoButton.Image = global::Glyssen.Properties.Resources.redo;
			this.m_redoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_redoButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_redoButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_redoButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.RedoButton");
			this.m_redoButton.Name = "m_redoButton";
			this.m_redoButton.Size = new System.Drawing.Size(23, 22);
			this.m_redoButton.Text = "Redo";
			this.m_redoButton.ToolTipText = "Redo {0}";
			this.m_redoButton.Click += new System.EventHandler(this.HandleRedoButtonClick);
			this.m_redoButton.MouseEnter += new System.EventHandler(this.SetUndoOrRedoButtonToolTip);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// m_editActorsButton
			// 
			this.m_editActorsButton.AutoToolTip = false;
			this.m_editActorsButton.Image = global::Glyssen.Properties.Resources.people;
			this.m_editActorsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_editActorsButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_editActorsButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_editActorsButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.EditVoiceActors");
			this.m_editActorsButton.Name = "m_editActorsButton";
			this.m_editActorsButton.Size = new System.Drawing.Size(125, 22);
			this.m_editActorsButton.Text = "Edit Voice Actors...";
			this.m_editActorsButton.Click += new System.EventHandler(this.HandleEditVoiceActorsClick);
			// 
			// m_updateGroupsButton
			// 
			this.m_updateGroupsButton.AutoToolTip = false;
			this.m_updateGroupsButton.Image = global::Glyssen.Properties.Resources.UpdateGroups;
			this.m_updateGroupsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_updateGroupsButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_updateGroupsButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_updateGroupsButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.UpdateCharacterGroups");
			this.m_updateGroupsButton.Name = "m_updateGroupsButton";
			this.m_updateGroupsButton.Size = new System.Drawing.Size(169, 22);
			this.m_updateGroupsButton.Text = "Update Character Groups...";
			this.m_updateGroupsButton.Click += new System.EventHandler(this.HandleUpdateGroupsClick);
			// 
			// m_splitSelectedGroupButton
			// 
			this.m_splitSelectedGroupButton.AutoToolTip = false;
			this.m_splitSelectedGroupButton.Image = global::Glyssen.Properties.Resources.splitGroup;
			this.m_splitSelectedGroupButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitSelectedGroupButton, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitSelectedGroupButton, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitSelectedGroupButton, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.SplitSelectedGroup");
			this.m_splitSelectedGroupButton.Name = "m_splitSelectedGroupButton";
			this.m_splitSelectedGroupButton.Size = new System.Drawing.Size(142, 22);
			this.m_splitSelectedGroupButton.Text = "Split Selected Group...";
			this.m_splitSelectedGroupButton.Click += new System.EventHandler(this.HandleSplitSelectedGroupClick);
			// 
			// m_btnOK
			// 
			this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOK.AutoSize = true;
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOK, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOK, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOK, "Common.OK");
			this.m_btnOK.Location = new System.Drawing.Point(769, 442);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(78, 23);
			this.m_btnOK.TabIndex = 8;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			// 
			// m_btnShowHideDetails
			// 
			this.m_btnShowHideDetails.AutoSize = true;
			this.m_btnShowHideDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnShowHideDetails, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnShowHideDetails, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnShowHideDetails, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnShowHideDetails, "DialogBoxes.VoiceActorAssignmentDlg.HideCharacterDetailsButton");
			this.m_btnShowHideDetails.Location = new System.Drawing.Point(0, 242);
			this.m_btnShowHideDetails.Margin = new System.Windows.Forms.Padding(0, 10, 3, 6);
			this.m_btnShowHideDetails.Name = "m_btnShowHideDetails";
			this.m_btnShowHideDetails.Size = new System.Drawing.Size(123, 23);
			this.m_btnShowHideDetails.TabIndex = 7;
			this.m_btnShowHideDetails.Text = "Hide Character Details";
			this.m_btnShowHideDetails.UseVisualStyleBackColor = true;
			this.m_btnShowHideDetails.Click += new System.EventHandler(this.m_btnShowHideDetails_Click);
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
			this.m_characterDetailsGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_characterDetailsGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_characterDetailsGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_characterDetailsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
			this.m_characterDetailsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_characterDetailsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CharacterDetailsIdCol,
            this.CharacterDetailsGenderCol,
            this.CharacterDetailsAgeCol,
            this.CharacterDetailsHoursCol});
			this.m_characterDetailsGrid.ContextMenuStrip = this.m_contextMenuCharacters;
			this.m_characterDetailsGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterDetailsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_characterDetailsGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_characterDetailsGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterDetailsGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterDetailsGrid, "DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails");
			this.m_characterDetailsGrid.Location = new System.Drawing.Point(0, 3);
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
			this.m_characterDetailsGrid.Size = new System.Drawing.Size(555, 52);
			this.m_characterDetailsGrid.TabIndex = 8;
			this.m_characterDetailsGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterDetailsGrid.VirtualMode = true;
			this.m_characterDetailsGrid.WaterMark = "!";
			this.m_characterDetailsGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleGridCellMouseDown);
			this.m_characterDetailsGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_characterDetailsGrid_CellValueNeeded);
			// 
			// CharacterDetailsIdCol
			// 
			this.CharacterDetailsIdCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.CharacterDetailsIdCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.CharacterId!Character" +
    "Id";
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
			dataGridViewCellStyle9.Format = "N2";
			this.CharacterDetailsHoursCol.DefaultCellStyle = dataGridViewCellStyle9;
			this.CharacterDetailsHoursCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Hours!Hours";
			this.CharacterDetailsHoursCol.Name = "CharacterDetailsHoursCol";
			this.CharacterDetailsHoursCol.ReadOnly = true;
			this.CharacterDetailsHoursCol.Width = 437;
			// 
			// m_lblNoCharactersInGroup
			// 
			this.m_lblNoCharactersInGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblNoCharactersInGroup.AutoSize = true;
			this.m_lblNoCharactersInGroup.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblNoCharactersInGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblNoCharactersInGroup, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblNoCharactersInGroup, "DialogBoxes.label1");
			this.m_lblNoCharactersInGroup.Location = new System.Drawing.Point(3, 58);
			this.m_lblNoCharactersInGroup.Name = "m_lblNoCharactersInGroup";
			this.m_lblNoCharactersInGroup.Size = new System.Drawing.Size(549, 42);
			this.m_lblNoCharactersInGroup.TabIndex = 9;
			this.m_lblNoCharactersInGroup.Text = resources.GetString("m_lblNoCharactersInGroup.Text");
			this.m_lblNoCharactersInGroup.UseCompatibleTextRendering = true;
			// 
			// m_characterGroupGrid
			// 
			this.m_characterGroupGrid.AllowDrop = true;
			this.m_characterGroupGrid.AllowUserToAddRows = false;
			this.m_characterGroupGrid.AllowUserToDeleteRows = false;
			this.m_characterGroupGrid.AllowUserToOrderColumns = true;
			this.m_characterGroupGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.m_characterGroupGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.m_characterGroupGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_characterGroupGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.m_characterGroupGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.m_characterGroupGrid.BackgroundColor = System.Drawing.SystemColors.Window;
			this.m_characterGroupGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_characterGroupGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle10.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
			this.m_characterGroupGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CharacterIdsCol,
            this.AttributesCol,
            this.CharStatusCol,
            this.EstimatedHoursCol,
            this.VoiceActorCol});
			this.m_tableLayoutPanel.SetColumnSpan(this.m_characterGroupGrid, 2);
			this.m_characterGroupGrid.ContextMenuStrip = this.m_contextMenuCharacterGroups;
			dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle12.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle12.NullValue = null;
			dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.DefaultCellStyle = dataGridViewCellStyle12;
			this.m_characterGroupGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterGroupGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_characterGroupGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterGroupGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_characterGroupGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterGroupGrid, "DialogBoxes.VoiceActorAssignmentDlg.betterGrid1");
			this.m_characterGroupGrid.Location = new System.Drawing.Point(0, 0);
			this.m_characterGroupGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_characterGroupGrid.MultiSelect = false;
			this.m_characterGroupGrid.Name = "m_characterGroupGrid";
			this.m_characterGroupGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_characterGroupGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_characterGroupGrid.RowHeadersVisible = false;
			this.m_characterGroupGrid.RowHeadersWidth = 22;
			this.m_characterGroupGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_characterGroupGrid.SelectedCellBackColor = System.Drawing.Color.Empty;
			this.m_characterGroupGrid.SelectedCellForeColor = System.Drawing.Color.Empty;
			this.m_characterGroupGrid.SelectedRowBackColor = System.Drawing.Color.Empty;
			this.m_characterGroupGrid.SelectedRowForeColor = System.Drawing.Color.Empty;
			this.m_characterGroupGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_characterGroupGrid.ShowWaterMarkWhenDirty = false;
			this.m_characterGroupGrid.Size = new System.Drawing.Size(833, 232);
			this.m_characterGroupGrid.TabIndex = 6;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.VirtualMode = true;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellEnter);
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
			this.m_characterGroupGrid.MouseUp += new System.Windows.Forms.MouseEventHandler(this.m_characterGroupGrid_MouseUp);
			// 
			// CharacterIdsCol
			// 
			this.CharacterIdsCol.FillWeight = 150.8968F;
			this.CharacterIdsCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
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
			this.CharStatusCol.Width = 348;
			// 
			// EstimatedHoursCol
			// 
			this.EstimatedHoursCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle11.Format = "N2";
			dataGridViewCellStyle11.NullValue = null;
			this.EstimatedHoursCol.DefaultCellStyle = dataGridViewCellStyle11;
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
			this.m_saveStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorAssignmentDlg.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(751, 34);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 6;
			this.m_saveStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanel.ColumnCount = 2;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.m_tableLayoutPanel.Controls.Add(this.m_tableLayoutPanelCharacterDetails, 0, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_characterGroupGrid, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnShowHideDetails, 0, 1);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(15, 55);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 3;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(833, 371);
			this.m_tableLayoutPanel.TabIndex = 9;
			this.m_tableLayoutPanel.Resize += new System.EventHandler(this.m_tableLayoutPanel_Resize);
			// 
			// m_tableLayoutPanelCharacterDetails
			// 
			this.m_tableLayoutPanelCharacterDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanelCharacterDetails.AutoSize = true;
			this.m_tableLayoutPanelCharacterDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_tableLayoutPanelCharacterDetails.ColumnCount = 1;
			this.m_tableLayoutPanelCharacterDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelCharacterDetails.Controls.Add(this.m_lblNoCharactersInGroup, 0, 1);
			this.m_tableLayoutPanelCharacterDetails.Controls.Add(this.m_characterDetailsGrid, 0, 0);
			this.m_tableLayoutPanelCharacterDetails.Location = new System.Drawing.Point(0, 271);
			this.m_tableLayoutPanelCharacterDetails.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelCharacterDetails.Name = "m_tableLayoutPanelCharacterDetails";
			this.m_tableLayoutPanelCharacterDetails.RowCount = 2;
			this.m_tableLayoutPanelCharacterDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelCharacterDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelCharacterDetails.Size = new System.Drawing.Size(555, 100);
			this.m_tableLayoutPanelCharacterDetails.TabIndex = 10;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.CharacterId!Character" +
    "Id";
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
			dataGridViewCellStyle3.Format = "N2";
			this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridViewTextBoxColumn4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Hours!Hours";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// m_toolStripTextBoxFindCharacter
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripTextBoxFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripTextBoxFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripTextBoxFindCharacter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripTextBoxFindCharacter, "DialogBoxes.VoiceActorAssignmentDlg.m_toolStripTextBoxFindCharacter");
			this.m_toolStripTextBoxFindCharacter.Name = "m_toolStripTextBoxFindCharacter";
			this.m_toolStripTextBoxFindCharacter.Size = new System.Drawing.Size(120, 25);
			this.m_toolStripTextBoxFindCharacter.ToolTipText = "Begin typing a character ID to find the group that contains it";
			this.m_toolStripTextBoxFindCharacter.TextChanged += new System.EventHandler(this.m_toolStripTextBoxFindCharacter_TextChanged);
			// 
			// m_toolStripLabelFindCharacter
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripLabelFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripLabelFindCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStripLabelFindCharacter, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripLabelFindCharacter, "DialogBoxes.VoiceActorAssignmentDlg.m_toolStripLabelFindCharacter");
			this.m_toolStripLabelFindCharacter.Name = "m_toolStripLabelFindCharacter";
			this.m_toolStripLabelFindCharacter.Size = new System.Drawing.Size(87, 22);
			this.m_toolStripLabelFindCharacter.Text = "Find Character:";
			// 
			// m_toolStripButtonFindNextMatchingCharacter
			// 
			this.m_toolStripButtonFindNextMatchingCharacter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_toolStripButtonFindNextMatchingCharacter.Enabled = false;
			this.m_toolStripButtonFindNextMatchingCharacter.Image = global::Glyssen.Properties.Resources.search_glyph;
			this.m_toolStripButtonFindNextMatchingCharacter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStripButtonFindNextMatchingCharacter, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStripButtonFindNextMatchingCharacter, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripButtonFindNextMatchingCharacter, "DialogBoxes.VoiceActorAssignmentDlg.toolStripButton1");
			this.m_toolStripButtonFindNextMatchingCharacter.Name = "m_toolStripButtonFindNextMatchingCharacter";
			this.m_toolStripButtonFindNextMatchingCharacter.Size = new System.Drawing.Size(23, 22);
			this.m_toolStripButtonFindNextMatchingCharacter.Text = "Find next match";
			this.m_toolStripButtonFindNextMatchingCharacter.Click += new System.EventHandler(this.m_toolStripButtonFindNextMatchingCharacter_Click);
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(859, 477);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_toolStrip);
			this.Controls.Add(this.m_lblInstructions);
			this.Controls.Add(this.m_saveStatus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.VoiceActorAssignmentDlg.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(875, 480);
			this.Name = "VoiceActorAssignmentDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor Assignment";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceActorAssignmentDlg_FormClosing);
			this.m_contextMenuCharacters.ResumeLayout(false);
			this.m_contextMenuCharacterGroups.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.m_tableLayoutPanelCharacterDetails.ResumeLayout(false);
			this.m_tableLayoutPanelCharacterDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblInstructions;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacterGroups;
		private System.Windows.Forms.ToolStripMenuItem m_unAssignActorFromGroupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_splitGroupToolStripMenuItem;
		private Glyssen.Controls.AutoScrollGrid m_characterGroupGrid;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacters;
		private System.Windows.Forms.ToolStripMenuItem m_menuItemCreateNewGroup;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.ToolStripButton m_editActorsButton;
		private System.Windows.Forms.ToolStripButton m_splitSelectedGroupButton;
		private System.Windows.Forms.ToolStripButton m_updateGroupsButton;
		private System.Windows.Forms.ToolStripButton m_undoButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton m_redoButton;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private System.Windows.Forms.Button m_btnShowHideDetails;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterDetailsGrid;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsIdCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsGenderCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsAgeCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsHoursCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterIdsCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn AttributesCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharStatusCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn EstimatedHoursCol;
		private DataGridViewMultiColumnComboBoxColumn VoiceActorCol;
		private System.Windows.Forms.ToolStripMenuItem m_splitGroupCharacterDetailsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_menuItemMoveToAnotherGroup;
		private System.Windows.Forms.Label m_lblNoCharactersInGroup;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelCharacterDetails;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFindCharacter;
		private System.Windows.Forms.ToolStripTextBox m_toolStripTextBoxFindCharacter;
		private System.Windows.Forms.ToolStripButton m_toolStripButtonFindNextMatchingCharacter;

	}
}