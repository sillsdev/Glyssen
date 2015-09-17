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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_contextMenuCharacters = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_tmiCreateNewGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_contextMenuCharacterGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_unAssignActorFromGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnUpdateGroup = new System.Windows.Forms.Button();
			this.m_linkClose = new System.Windows.Forms.LinkLabel();
			this.m_btnEditVoiceActors = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.m_characterGroupGrid = new Glyssen.Controls.AutoGrid();
			this.CharacterIdsCol = new Glyssen.Controls.DataGridViewListBoxColumn();
			this.AttributesCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.EstimatedHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.VoiceActorCol = new Glyssen.Controls.DataGridViewMultiColumnComboBoxColumn();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_contextMenuCharacters.SuspendLayout();
			this.m_contextMenuCharacterGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.SuspendLayout();
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
			this.m_lblInstructions.Location = new System.Drawing.Point(12, 9);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(199, 13);
			this.m_lblInstructions.TabIndex = 0;
			this.m_lblInstructions.Text = "Assign Voice Actors to Character Groups";
			// 
			// m_contextMenuCharacters
			// 
			this.m_contextMenuCharacters.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tmiCreateNewGroup});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacters, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_contextMenuCharacters, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacters, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuCharacters.Name = "m_contextMenuCharacters";
			this.m_contextMenuCharacters.Size = new System.Drawing.Size(328, 26);
			this.m_contextMenuCharacters.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacters_Opening);
			// 
			// m_tmiCreateNewGroup
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_tmiCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_tmiCreateNewGroup, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_tmiCreateNewGroup, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_tmiCreateNewGroup, "DialogBoxes.VoiceActorAssignmentDlg.createANewGroupWithTheseCharactersToolStripMe" +
        "nuItem");
			this.m_tmiCreateNewGroup.Name = "m_tmiCreateNewGroup";
			this.m_tmiCreateNewGroup.Size = new System.Drawing.Size(327, 22);
			this.m_tmiCreateNewGroup.Text = "Create a New Group with the Selected Character";
			this.m_tmiCreateNewGroup.Click += new System.EventHandler(this.m_tmiCreateNewGroup_Click);
			// 
			// m_contextMenuCharacterGroups
			// 
			this.m_contextMenuCharacterGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_unAssignActorFromGroupToolStripMenuItem,
            this.m_splitGroupToolStripMenuItem});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_contextMenuCharacterGroups, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_contextMenuCharacterGroups, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuCharacterGroups.Name = "m_contextMenuCharacterGroups";
			this.m_contextMenuCharacterGroups.Size = new System.Drawing.Size(274, 48);
			this.m_contextMenuCharacterGroups.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacterGroups_Opening);
			// 
			// m_unAssignActorFromGroupToolStripMenuItem
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_unAssignActorFromGroupToolStripMenuItem, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_unAssignActorFromGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.unAssignActorFromGroupToolStripMenuItem");
			this.m_unAssignActorFromGroupToolStripMenuItem.Name = "m_unAssignActorFromGroupToolStripMenuItem";
			this.m_unAssignActorFromGroupToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
			this.m_unAssignActorFromGroupToolStripMenuItem.Text = "Un-Assign Actor from Selected Group";
			this.m_unAssignActorFromGroupToolStripMenuItem.Click += new System.EventHandler(this.m_unAssignActorFromGroupToolStripMenuItem_Click);
			// 
			// m_splitGroupToolStripMenuItem
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_splitGroupToolStripMenuItem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_splitGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.SplitSelectedGroup");
			this.m_splitGroupToolStripMenuItem.Name = "m_splitGroupToolStripMenuItem";
			this.m_splitGroupToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
			this.m_splitGroupToolStripMenuItem.Text = "Split Selected Group";
			this.m_splitGroupToolStripMenuItem.Click += new System.EventHandler(this.m_splitGroupToolStripMenuItem_Click);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.VoiceActorAssignmentDlg";
			// 
			// m_btnUpdateGroup
			// 
			this.m_btnUpdateGroup.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnUpdateGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnUpdateGroup, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnUpdateGroup, "DialogBoxes.VoiceActorAssignmentDlg.UpdateGroup");
			this.m_btnUpdateGroup.Location = new System.Drawing.Point(0, 3);
			this.m_btnUpdateGroup.Name = "m_btnUpdateGroup";
			this.m_btnUpdateGroup.Size = new System.Drawing.Size(185, 23);
			this.m_btnUpdateGroup.TabIndex = 4;
			this.m_btnUpdateGroup.Text = "Update/Edit the Character Group(s)";
			this.m_btnUpdateGroup.UseVisualStyleBackColor = true;
			this.m_btnUpdateGroup.Click += new System.EventHandler(this.m_btnUpdateGroup_Click);
			// 
			// m_linkClose
			// 
			this.m_linkClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkClose.AutoSize = true;
			this.m_linkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_linkClose, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_linkClose, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_linkClose, "DialogBoxes.VoiceActorAssignmentDlg.Close");
			this.m_linkClose.Location = new System.Drawing.Point(799, 8);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(33, 13);
			this.m_linkClose.TabIndex = 5;
			this.m_linkClose.TabStop = true;
			this.m_linkClose.Text = "Close";
			this.m_linkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkClose_LinkClicked);
			// 
			// m_btnEditVoiceActors
			// 
			this.m_btnEditVoiceActors.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnEditVoiceActors, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnEditVoiceActors, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnEditVoiceActors, "DialogBoxes.VoiceActorAssignmentDlg.EditVoiceActors");
			this.m_btnEditVoiceActors.Location = new System.Drawing.Point(191, 3);
			this.m_btnEditVoiceActors.Name = "m_btnEditVoiceActors";
			this.m_btnEditVoiceActors.Size = new System.Drawing.Size(98, 23);
			this.m_btnEditVoiceActors.TabIndex = 7;
			this.m_btnEditVoiceActors.Text = "Edit Voice Actors";
			this.m_btnEditVoiceActors.UseVisualStyleBackColor = true;
			this.m_btnEditVoiceActors.Click += new System.EventHandler(this.m_btnEditVoiceActors_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_characterGroupGrid, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(832, 405);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.m_linkClose);
			this.panel3.Controls.Add(this.m_btnEditVoiceActors);
			this.panel3.Controls.Add(this.m_saveStatus);
			this.panel3.Controls.Add(this.m_btnUpdateGroup);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(0, 352);
			this.panel3.Margin = new System.Windows.Forms.Padding(0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(832, 53);
			this.panel3.TabIndex = 7;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(15, 25);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
			this.splitContainer1.Panel1.Margin = new System.Windows.Forms.Padding(3);
			this.splitContainer1.Panel1MinSize = 300;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(3);
			this.splitContainer1.Panel2Collapsed = true;
			this.splitContainer1.Panel2MinSize = 0;
			this.splitContainer1.Size = new System.Drawing.Size(832, 405);
			this.splitContainer1.SplitterDistance = 418;
			this.splitContainer1.SplitterWidth = 55;
			this.splitContainer1.TabIndex = 0;
			this.splitContainer1.TabStop = false;
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
			this.m_characterGroupGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
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
			this.m_characterGroupGrid.Location = new System.Drawing.Point(0, 0);
			this.m_characterGroupGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_characterGroupGrid.Name = "m_characterGroupGrid";
			this.m_characterGroupGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_characterGroupGrid.ParentLayersAffected = 0;
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
			this.m_characterGroupGrid.Size = new System.Drawing.Size(832, 352);
			this.m_characterGroupGrid.TabIndex = 6;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellEnter);
			this.m_characterGroupGrid.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellLeave);
			this.m_characterGroupGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDown);
			this.m_characterGroupGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.m_characterGroupGrid_CellValidating);
			this.m_characterGroupGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.m_characterGroupGrid_DataError);
			this.m_characterGroupGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.m_characterGroupGrid_EditingControlShowing);
			this.m_characterGroupGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragDrop);
			this.m_characterGroupGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragOver);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			this.m_characterGroupGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.m_characterGroupGrid_MouseMove);
			// 
			// CharacterIdsCol
			// 
			this.CharacterIdsCol.ContextMenuStrip = this.m_contextMenuCharacters;
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
			this.VoiceActorCol.ColumnNames.Add("Name");
			this.VoiceActorCol.ColumnNames.Add("Gender");
			this.VoiceActorCol.ColumnNames.Add("Age");
			this.VoiceActorCol.ColumnNames.Add("Cameo");
			this.VoiceActorCol.ColumnWidths.Add("150");
			this.VoiceActorCol.EvenRowsBackColor = System.Drawing.SystemColors.Control;
			this.VoiceActorCol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.VoiceActorCol.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.VoiceActorAssigned!Voice Actor Assigne" +
    "d";
			this.VoiceActorCol.MaxDropDownItems = 20;
			this.VoiceActorCol.MinimumWidth = 50;
			this.VoiceActorCol.OddRowsBackColor = System.Drawing.SystemColors.Control;
			this.VoiceActorCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.VoiceActorCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.VoiceActorCol.Width = 502;
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_saveStatus, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorAssignmentDlg.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(308, 8);
			this.m_saveStatus.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 6;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(859, 442);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.m_lblInstructions);
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
			this.m_contextMenuCharacters.ResumeLayout(false);
			this.m_contextMenuCharacterGroups.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblInstructions;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Button m_btnUpdateGroup;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.LinkLabel m_linkClose;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacterGroups;
		private System.Windows.Forms.ToolStripMenuItem m_unAssignActorFromGroupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_splitGroupToolStripMenuItem;
		private AutoGrid m_characterGroupGrid;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacters;
		private System.Windows.Forms.ToolStripMenuItem m_tmiCreateNewGroup;
		private System.Windows.Forms.Button m_btnEditVoiceActors;
		private DataGridViewListBoxColumn CharacterIdsCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn AttributesCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharStatusCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn EstimatedHoursCol;
		private DataGridViewMultiColumnComboBoxColumn VoiceActorCol;

	}
}