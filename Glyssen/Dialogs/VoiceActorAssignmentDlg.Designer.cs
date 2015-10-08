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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_contextMenuCharacters = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_menuItemCreateNewGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_contextMenuCharacterGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_unAssignActorFromGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.m_updateGroupsButton = new System.Windows.Forms.ToolStripButton();
			this.m_splitSelectedGroupButton = new System.Windows.Forms.ToolStripButton();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_characterGroupGrid = new Glyssen.Controls.AutoScrollGrid();
			this.CharacterIdsCol = new Glyssen.Controls.DataGridViewListBoxColumn();
			this.AttributesCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.EstimatedHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VoiceActorCol = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.dataGridViewListBoxColumn1 = new Glyssen.Controls.DataGridViewListBoxColumn();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewMultiColumnComboBoxColumn1 = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.m_contextMenuCharacters.SuspendLayout();
			this.m_contextMenuCharacterGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
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
            this.m_menuItemCreateNewGroup});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacters, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacters, "DialogBoxes.VoiceActorAssignmentDlg.m_contextMenuCharacters.m_contextMenuCharacte" +
        "rs");
			this.m_contextMenuCharacters.Name = "m_contextMenuCharacters";
			this.m_contextMenuCharacters.Size = new System.Drawing.Size(328, 48);
			this.m_contextMenuCharacters.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacters_Opening);
			// 
			// m_menuItemCreateNewGroup
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_menuItemCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_menuItemCreateNewGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_menuItemCreateNewGroup, "DialogBoxes.VoiceActorAssignmentDlg.m_menuItemCreateNewGroup");
			this.m_menuItemCreateNewGroup.Name = "m_menuItemCreateNewGroup";
			this.m_menuItemCreateNewGroup.Size = new System.Drawing.Size(327, 22);
			this.m_menuItemCreateNewGroup.Text = "Create a New Group with the Selected Character";
			this.m_menuItemCreateNewGroup.Click += new System.EventHandler(this.m_menuItemCreateNewGroup_Click);
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
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.VoiceActorAssignmentDlg";
			// 
			// m_toolStrip
			// 
			this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.m_updateGroupsButton,
            this.m_splitSelectedGroupButton});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.VoiceActorAssignmentDlg.toolStrip1");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.m_toolStrip.Size = new System.Drawing.Size(859, 25);
			this.m_toolStrip.TabIndex = 7;
			this.m_toolStrip.Text = "toolStrip1";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.AutoToolTip = false;
			this.toolStripButton1.Image = global::Glyssen.Properties.Resources.people;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.toolStripButton1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.toolStripButton1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.toolStripButton1, "DialogBoxes.VoiceActorAssignmentDlg.ToolStrip.EditVoiceActors");
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(125, 22);
			this.toolStripButton1.Text = "Edit Voice Actors...";
			this.toolStripButton1.Click += new System.EventHandler(this.HandleEditVoiceActorsClick);
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
			this.m_characterGroupGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_characterGroupGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_characterGroupGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.m_characterGroupGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CharacterIdsCol,
            this.AttributesCol,
            this.CharStatusCol,
            this.EstimatedHoursCol,
            this.VoiceActorCol});
			this.m_characterGroupGrid.ContextMenuStrip = this.m_contextMenuCharacterGroups;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle4.NullValue = null;
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.DefaultCellStyle = dataGridViewCellStyle4;
			this.m_characterGroupGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterGroupGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_characterGroupGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterGroupGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterGroupGrid, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_characterGroupGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterGroupGrid, "DialogBoxes.VoiceActorAssignmentDlg.betterGrid1");
			this.m_characterGroupGrid.Location = new System.Drawing.Point(15, 50);
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
			this.m_characterGroupGrid.Size = new System.Drawing.Size(829, 378);
			this.m_characterGroupGrid.TabIndex = 6;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellEnter);
			this.m_characterGroupGrid.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellLeave);
			this.m_characterGroupGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDown);
			this.m_characterGroupGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.m_characterGroupGrid_CellValidating);
			this.m_characterGroupGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.m_characterGroupGrid_DataError);
			this.m_characterGroupGrid.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.m_characterGroupGrid_RowPrePaint);
			this.m_characterGroupGrid.SelectionChanged += new System.EventHandler(this.m_characterGroupGrid_SelectionChanged);
			this.m_characterGroupGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragDrop);
			this.m_characterGroupGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragOver);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			this.m_characterGroupGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.m_characterGroupGrid_MouseMove);
			// 
			// CharacterIdsCol
			// 
			this.CharacterIdsCol.DataPropertyName = "CharacterIds";
			this.CharacterIdsCol.FillWeight = 150.8968F;
			this.CharacterIdsCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
			this.CharacterIdsCol.Name = "CharacterIdsCol";
			this.CharacterIdsCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// AttributesCol
			// 
			this.AttributesCol.DataPropertyName = "AttributesDisplay";
			this.AttributesCol.FillWeight = 75.44839F;
			this.AttributesCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.AttributesCol.Name = "AttributesCol";
			this.AttributesCol.ReadOnly = true;
			// 
			// CharStatusCol
			// 
			this.CharStatusCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.CharStatusCol.DataPropertyName = "StatusDisplay";
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
			this.EstimatedHoursCol.DataPropertyName = "EstimatedHours";
			dataGridViewCellStyle3.Format = "N2";
			dataGridViewCellStyle3.NullValue = null;
			this.EstimatedHoursCol.DefaultCellStyle = dataGridViewCellStyle3;
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
			this.VoiceActorCol.DataPropertyName = "VoiceActorId";
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
			// dataGridViewListBoxColumn1
			// 
			this.dataGridViewListBoxColumn1.ContextMenuStrip = this.m_contextMenuCharacters;
			this.dataGridViewListBoxColumn1.DataPropertyName = "CharacterIds";
			this.dataGridViewListBoxColumn1.FillWeight = 150.8968F;
			this.dataGridViewListBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
			this.dataGridViewListBoxColumn1.Name = "dataGridViewListBoxColumn1";
			this.dataGridViewListBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewListBoxColumn1.Width = 5;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.DataPropertyName = "AttributesDisplay";
			this.dataGridViewTextBoxColumn1.FillWeight = 75.44839F;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.Width = 5;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn2.DataPropertyName = "StatusDisplay";
			this.dataGridViewTextBoxColumn2.FillWeight = 115.4822F;
			this.dataGridViewTextBoxColumn2.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Status!Status";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			this.dataGridViewTextBoxColumn2.Visible = false;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.dataGridViewTextBoxColumn3.DataPropertyName = "EstimatedHours";
			dataGridViewCellStyle5.Format = "N2";
			dataGridViewCellStyle5.NullValue = null;
			this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle5;
			this.dataGridViewTextBoxColumn3.FillWeight = 37.7242F;
			this.dataGridViewTextBoxColumn3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// dataGridViewMultiColumnComboBoxColumn1
			// 
			this.dataGridViewMultiColumnComboBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Name");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Gender");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Age");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnNames.Add("Cameo");
			this.dataGridViewMultiColumnComboBoxColumn1.ColumnWidths.Add("150");
			this.dataGridViewMultiColumnComboBoxColumn1.DataPropertyName = "VoiceActorId";
			this.dataGridViewMultiColumnComboBoxColumn1.EvenRowsBackColor = System.Drawing.SystemColors.Control;
			this.dataGridViewMultiColumnComboBoxColumn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dataGridViewMultiColumnComboBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.VoiceActorAssigned!Voice Actor Assigne" +
    "d";
			this.dataGridViewMultiColumnComboBoxColumn1.MaxDropDownItems = 20;
			this.dataGridViewMultiColumnComboBoxColumn1.MinimumWidth = 50;
			this.dataGridViewMultiColumnComboBoxColumn1.OddRowsBackColor = System.Drawing.SystemColors.Control;
			this.dataGridViewMultiColumnComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewMultiColumnComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.dataGridViewMultiColumnComboBoxColumn1.Width = 100;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(859, 477);
			this.Controls.Add(this.m_characterGroupGrid);
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
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
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
		private DataGridViewListBoxColumn dataGridViewListBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private DataGridViewMultiColumnComboBoxColumn dataGridViewMultiColumnComboBoxColumn1;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripButton m_splitSelectedGroupButton;
		private System.Windows.Forms.ToolStripButton m_updateGroupsButton;
		private DataGridViewListBoxColumn CharacterIdsCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn AttributesCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharStatusCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn EstimatedHoursCol;
		private DataGridViewMultiColumnComboBoxColumn VoiceActorCol;

	}
}