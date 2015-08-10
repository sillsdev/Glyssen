using System;
using Glyssen.Controls;

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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			this.m_btnAssignActor = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_characterGroupGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.GroupNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterIds = new Glyssen.Controls.DataGridViewListBoxColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_contextMenuCharacterGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_assignActorToGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_unAssignActorFromGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_splitGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnUpdateGroup = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.m_linkClose = new System.Windows.Forms.LinkLabel();
			this.m_linkEdit = new System.Windows.Forms.LinkLabel();
			this.m_helpIcon = new System.Windows.Forms.PictureBox();
			this.m_contextMenuVoiceActors = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_assignActorToGroupToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_editActorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_deleteActorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_saveStatus = new Glyssen.Controls.SaveStatus();
			this.m_voiceActorGrid = new Glyssen.Controls.VoiceActorInformationGrid();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
			this.m_contextMenuCharacterGroups.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_helpIcon)).BeginInit();
			this.m_contextMenuVoiceActors.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnAssignActor
			// 
			this.m_btnAssignActor.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnAssignActor, "DialogBoxes.VoiceActorAssignmentDlg.AssignActorButton");
			this.m_btnAssignActor.Location = new System.Drawing.Point(318, 134);
			this.m_btnAssignActor.Name = "m_btnAssignActor";
			this.m_btnAssignActor.Size = new System.Drawing.Size(48, 36);
			this.m_btnAssignActor.TabIndex = 2;
			this.m_btnAssignActor.Text = "Assign\r\n<<";
			this.m_btnAssignActor.UseVisualStyleBackColor = true;
			this.m_btnAssignActor.Click += new System.EventHandler(this.m_btnAssignActor_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "DialogBoxes.VoiceActorAssignmentDlg.AssignActors");
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(199, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Assign Voice Actors to Character Groups";
			// 
			// m_characterGroupGrid
			// 
			this.m_characterGroupGrid.AllowDrop = true;
			this.m_characterGroupGrid.AllowUserToAddRows = false;
			this.m_characterGroupGrid.AllowUserToDeleteRows = false;
			this.m_characterGroupGrid.AllowUserToOrderColumns = true;
			this.m_characterGroupGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
			this.m_characterGroupGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
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
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_characterGroupGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.m_characterGroupGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.m_characterGroupGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GroupNumber,
            this.CharacterIds,
            this.Column3,
            this.CharStatus,
            this.Column4,
            this.Column5});
			this.m_characterGroupGrid.ContextMenuStrip = this.m_contextMenuCharacterGroups;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle4.NullValue = null;
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.DefaultCellStyle = dataGridViewCellStyle4;
			this.m_characterGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_characterGroupGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterGroupGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_characterGroupGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterGroupGrid.GridColor = System.Drawing.Color.Black;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_characterGroupGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_characterGroupGrid, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_characterGroupGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_characterGroupGrid, "DialogBoxes.VoiceActorAssignmentDlg.betterGrid1");
			this.m_characterGroupGrid.Location = new System.Drawing.Point(0, 20);
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
			this.m_characterGroupGrid.Size = new System.Drawing.Size(300, 356);
			this.m_characterGroupGrid.TabIndex = 0;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_characterGroupGrid_CellLeave);
			this.m_characterGroupGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDoubleClick);
			this.m_characterGroupGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDown);
			this.m_characterGroupGrid.SelectionChanged += new System.EventHandler(this.m_characterGroupGrid_SelectionChanged);
			this.m_characterGroupGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragDrop);
			this.m_characterGroupGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragOver);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			this.m_characterGroupGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.m_characterGroupGrid_MouseMove);
			// 
			// GroupNumber
			// 
			this.GroupNumber.DataPropertyName = "GroupNumber";
			this.GroupNumber.FillWeight = 18.8621F;
			this.GroupNumber.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.GroupNumber!Group #";
			this.GroupNumber.Name = "GroupNumber";
			// 
			// CharacterIds
			// 
			this.CharacterIds.DataPropertyName = "CharacterIds";
			this.CharacterIds.FillWeight = 150.8968F;
			this.CharacterIds.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
			this.CharacterIds.Name = "CharacterIds";
			this.CharacterIds.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// Column3
			// 
			this.Column3.DataPropertyName = "AttributesDisplay";
			this.Column3.FillWeight = 75.44839F;
			this.Column3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.Column3.Name = "Column3";
			// 
			// CharStatus
			// 
			this.CharStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.CharStatus.DataPropertyName = "StatusDisplay";
			this.CharStatus.FillWeight = 115.4822F;
			this.CharStatus.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Status!Status";
			this.CharStatus.Name = "CharStatus";
			this.CharStatus.Visible = false;
			this.CharStatus.Width = 348;
			// 
			// Column4
			// 
			this.Column4.DataPropertyName = "EstimatedHours";
			dataGridViewCellStyle3.Format = "N2";
			dataGridViewCellStyle3.NullValue = null;
			this.Column4.DefaultCellStyle = dataGridViewCellStyle3;
			this.Column4.FillWeight = 37.7242F;
			this.Column4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.Column4.Name = "Column4";
			// 
			// Column5
			// 
			this.Column5.DataPropertyName = "VoiceActorAssignedName";
			this.Column5.FillWeight = 56.5863F;
			this.Column5.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.ActorAssigned!Actor Assigned";
			this.Column5.Name = "Column5";
			// 
			// m_contextMenuCharacterGroups
			// 
			this.m_contextMenuCharacterGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_assignActorToGroupToolStripMenuItem,
            this.m_unAssignActorFromGroupToolStripMenuItem,
            this.m_splitGroupToolStripMenuItem});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_contextMenuCharacterGroups, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_contextMenuCharacterGroups, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_contextMenuCharacterGroups, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuCharacterGroups.Name = "m_contextMenuCharacterGroups";
			this.m_contextMenuCharacterGroups.Size = new System.Drawing.Size(286, 70);
			this.m_contextMenuCharacterGroups.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuCharacterGroups_Opening);
			// 
			// m_assignActorToGroupToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_assignActorToGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_assignActorToGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_assignActorToGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.AssignSelectedActorToSelectedGro" +
        "up");
			this.m_assignActorToGroupToolStripMenuItem.Name = "m_assignActorToGroupToolStripMenuItem";
			this.m_assignActorToGroupToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
			this.m_assignActorToGroupToolStripMenuItem.Text = "Assign Selected Actor to Selected Group";
			this.m_assignActorToGroupToolStripMenuItem.Click += new System.EventHandler(this.m_assignActorToGroupToolStripMenuItem_Click);
			// 
			// m_unAssignActorFromGroupToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_unAssignActorFromGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_unAssignActorFromGroupToolStripMenuItem, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_unAssignActorFromGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.unAssignActorFromGroupToolStripMenuItem");
			this.m_unAssignActorFromGroupToolStripMenuItem.Name = "m_unAssignActorFromGroupToolStripMenuItem";
			this.m_unAssignActorFromGroupToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
			this.m_unAssignActorFromGroupToolStripMenuItem.Text = "Un-Assign Actor from Selected Group";
			this.m_unAssignActorFromGroupToolStripMenuItem.Click += new System.EventHandler(this.m_unAssignActorFromGroupToolStripMenuItem_Click);
			// 
			// m_splitGroupToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_splitGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_splitGroupToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_splitGroupToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.SplitSelectedGroup");
			this.m_splitGroupToolStripMenuItem.Name = "m_splitGroupToolStripMenuItem";
			this.m_splitGroupToolStripMenuItem.Size = new System.Drawing.Size(285, 22);
			this.m_splitGroupToolStripMenuItem.Text = "Split Selected Group";
			this.m_splitGroupToolStripMenuItem.Click += new System.EventHandler(this.m_splitGroupToolStripMenuItem_Click);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorAssignmentDlg";
			// 
			// m_btnUpdateGroup
			// 
			this.m_btnUpdateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_btnUpdateGroup.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnUpdateGroup, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnUpdateGroup, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnUpdateGroup, "DialogBoxes.VoiceActorAssignmentDlg.UpdateGroup");
			this.m_btnUpdateGroup.Location = new System.Drawing.Point(15, 407);
			this.m_btnUpdateGroup.Name = "m_btnUpdateGroup";
			this.m_btnUpdateGroup.Size = new System.Drawing.Size(185, 23);
			this.m_btnUpdateGroup.TabIndex = 4;
			this.m_btnUpdateGroup.Text = "Update/Edit the Character Group(s)";
			this.m_btnUpdateGroup.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label2, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label2, "DialogBoxes.VoiceActorAssignmentDlg.CharacterGroupsTitle");
			this.label2.Location = new System.Drawing.Point(105, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(90, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Character Groups";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label3, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label3, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label3, "DialogBoxes.VoiceActorAssignmentDlg.VoiceActorsTitle");
			this.label3.Location = new System.Drawing.Point(87, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(67, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Voice Actors";
			// 
			// m_linkClose
			// 
			this.m_linkClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkClose.AutoSize = true;
			this.m_linkClose.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkClose, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkClose, "DialogBoxes.VoiceActorAssignmentDlg.Close");
			this.m_linkClose.Location = new System.Drawing.Point(579, 412);
			this.m_linkClose.Name = "m_linkClose";
			this.m_linkClose.Size = new System.Drawing.Size(33, 13);
			this.m_linkClose.TabIndex = 5;
			this.m_linkClose.TabStop = true;
			this.m_linkClose.Text = "Close";
			this.m_linkClose.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkClose_LinkClicked);
			// 
			// m_linkEdit
			// 
			this.m_linkEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_linkEdit.AutoSize = true;
			this.m_linkEdit.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_linkEdit, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_linkEdit, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_linkEdit, "DialogBoxes.VoiceActorAssignmentDlg.Edit");
			this.m_linkEdit.Location = new System.Drawing.Point(214, 0);
			this.m_linkEdit.Name = "m_linkEdit";
			this.m_linkEdit.Size = new System.Drawing.Size(25, 13);
			this.m_linkEdit.TabIndex = 6;
			this.m_linkEdit.TabStop = true;
			this.m_linkEdit.Text = "Edit";
			this.m_linkEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.m_linkEdit_LinkClicked);
			// 
			// m_helpIcon
			// 
			this.m_helpIcon.Cursor = System.Windows.Forms.Cursors.Hand;
			this.m_helpIcon.Image = global::Glyssen.Properties.Resources.helpSmall;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_helpIcon, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_helpIcon, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_helpIcon, "DialogBoxes.VoiceActorAssignmentDlg.pictureBox1");
			this.m_helpIcon.Location = new System.Drawing.Point(216, 9);
			this.m_helpIcon.Name = "m_helpIcon";
			this.m_helpIcon.Size = new System.Drawing.Size(19, 23);
			this.m_helpIcon.TabIndex = 7;
			this.m_helpIcon.TabStop = false;
			this.m_helpIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.m_helpIcon_MouseClick);
			this.m_helpIcon.MouseLeave += new System.EventHandler(this.m_helpIcon_MouseLeave);
			// 
			// m_contextMenuVoiceActors
			// 
			this.m_contextMenuVoiceActors.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_assignActorToGroupToolStripMenuItem2,
            this.m_editActorToolStripMenuItem,
            this.m_deleteActorToolStripMenuItem});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_contextMenuVoiceActors, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_contextMenuVoiceActors, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_contextMenuVoiceActors, "DialogBoxes.VoiceActorAssignmentDlg.contextMenuStrip1");
			this.m_contextMenuVoiceActors.Name = "m_contextMenuVoiceActors";
			this.m_contextMenuVoiceActors.Size = new System.Drawing.Size(318, 70);
			this.m_contextMenuVoiceActors.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenuVoiceActors_Opening);
			// 
			// m_assignActorToGroupToolStripMenuItem2
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_assignActorToGroupToolStripMenuItem2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_assignActorToGroupToolStripMenuItem2, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_assignActorToGroupToolStripMenuItem2, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.AssignSelectedActorToSelectedGro" +
        "up");
			this.m_assignActorToGroupToolStripMenuItem2.Name = "m_assignActorToGroupToolStripMenuItem2";
			this.m_assignActorToGroupToolStripMenuItem2.Size = new System.Drawing.Size(317, 22);
			this.m_assignActorToGroupToolStripMenuItem2.Text = "Assign Selected Voice Actor to Selected Group";
			this.m_assignActorToGroupToolStripMenuItem2.Click += new System.EventHandler(this.m_assignActorToGroupToolStripMenuItem_Click);
			// 
			// m_editActorToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_editActorToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_editActorToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_editActorToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.EditSelectedActor");
			this.m_editActorToolStripMenuItem.Name = "m_editActorToolStripMenuItem";
			this.m_editActorToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
			this.m_editActorToolStripMenuItem.Text = "Edit Selected Voice Actor";
			this.m_editActorToolStripMenuItem.Click += new System.EventHandler(this.m_editActorToolStripMenuItem_Click);
			// 
			// m_deleteActorToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_deleteActorToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_deleteActorToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_deleteActorToolStripMenuItem, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_deleteActorToolStripMenuItem, "DialogBoxes.VoiceActorAssignmentDlg.deleteSelectedVoiceActorToolStripMenuItem");
			this.m_deleteActorToolStripMenuItem.Name = "m_deleteActorToolStripMenuItem";
			this.m_deleteActorToolStripMenuItem.Size = new System.Drawing.Size(317, 22);
			this.m_deleteActorToolStripMenuItem.Text = "Delete Selected Voice Actor";
			this.m_deleteActorToolStripMenuItem.Click += new System.EventHandler(this.m_deleteActorToolStripMenuItem_Click);
			// 
			// m_saveStatus
			// 
			this.m_saveStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_saveStatus.AutoSize = true;
			this.m_saveStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_saveStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_saveStatus.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_saveStatus, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_saveStatus, "DialogBoxes.VoiceActorAssignmentDlg.SaveStatus");
			this.m_saveStatus.Location = new System.Drawing.Point(373, 412);
			this.m_saveStatus.Name = "m_saveStatus";
			this.m_saveStatus.Size = new System.Drawing.Size(97, 13);
			this.m_saveStatus.TabIndex = 6;
			// 
			// m_voiceActorGrid
			// 
			this.m_voiceActorGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_voiceActorGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_voiceActorGrid.CharacterGroupsWithAssignedActors = null;
			this.m_voiceActorGrid.ContextMenuStrip = this.m_contextMenuVoiceActors;
			this.m_voiceActorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_voiceActorGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_voiceActorGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_voiceActorGrid, "DialogBoxes.VoiceActorAssignmentDlg.VoiceActorInformationGrid");
			this.m_voiceActorGrid.Location = new System.Drawing.Point(0, 20);
			this.m_voiceActorGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_voiceActorGrid.Name = "m_voiceActorGrid";
			this.m_voiceActorGrid.ReadOnly = false;
			this.m_voiceActorGrid.Size = new System.Drawing.Size(242, 356);
			this.m_voiceActorGrid.TabIndex = 1;
			this.m_voiceActorGrid.Leave += new System.EventHandler(this.m_voiceActorGrid_Leave);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_characterGroupGrid, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(300, 376);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.m_voiceActorGrid, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(242, 376);
			this.tableLayoutPanel2.TabIndex = 2;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.m_linkEdit);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(236, 14);
			this.panel1.TabIndex = 2;
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
			this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
			this.splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(3);
			this.splitContainer1.Panel2MinSize = 200;
			this.splitContainer1.Size = new System.Drawing.Size(597, 376);
			this.splitContainer1.SplitterDistance = 300;
			this.splitContainer1.SplitterWidth = 55;
			this.splitContainer1.TabIndex = 0;
			this.splitContainer1.TabStop = false;
			this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
			this.splitContainer1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseUp);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.DataPropertyName = "GroupNumber";
			this.dataGridViewTextBoxColumn1.FillWeight = 25F;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.GroupNumber!Group #";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.Width = 13;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.DataPropertyName = "CharactersString";
			this.dataGridViewTextBoxColumn2.FillWeight = 200F;
			this.dataGridViewTextBoxColumn2.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.Width = 69;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.DataPropertyName = "AttributesDisplay";
			dataGridViewCellStyle5.Format = "N2";
			dataGridViewCellStyle5.NullValue = null;
			this.dataGridViewTextBoxColumn3.DefaultCellStyle = dataGridViewCellStyle5;
			this.dataGridViewTextBoxColumn3.FillWeight = 50F;
			this.dataGridViewTextBoxColumn3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Attributes!Attributes";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.Width = 55;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.DataPropertyName = "EstimatedHours";
			dataGridViewCellStyle6.Format = "N2";
			dataGridViewCellStyle6.NullValue = null;
			this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle6;
			this.dataGridViewTextBoxColumn4.FillWeight = 50F;
			this.dataGridViewTextBoxColumn4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.Width = 47;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.DataPropertyName = "VoiceActorAssignedName";
			this.dataGridViewTextBoxColumn5.FillWeight = 75F;
			this.dataGridViewTextBoxColumn5.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.ActorAssigned!Actor Assigned";
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.Width = 51;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.m_helpIcon);
			this.Controls.Add(this.m_saveStatus);
			this.Controls.Add(this.m_linkClose);
			this.Controls.Add(this.m_btnAssignActor);
			this.Controls.Add(this.m_btnUpdateGroup);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorAssignmentDlg.WindowTitle");
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "VoiceActorAssignmentDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice Actor Assignment";
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VoiceActorAssignmentDlg_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			this.m_contextMenuCharacterGroups.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_helpIcon)).EndInit();
			this.m_contextMenuVoiceActors.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.VoiceActorInformationGrid m_voiceActorGrid;
		private System.Windows.Forms.Button m_btnAssignActor;
		private System.Windows.Forms.Label label1;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterGroupGrid;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Button m_btnUpdateGroup;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.LinkLabel m_linkClose;
		private Controls.SaveStatus m_saveStatus;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.LinkLabel m_linkEdit;
		private System.Windows.Forms.PictureBox m_helpIcon;
		private System.Windows.Forms.ToolTip m_toolTip;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuCharacterGroups;
		private System.Windows.Forms.ToolStripMenuItem m_assignActorToGroupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_unAssignActorFromGroupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_splitGroupToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip m_contextMenuVoiceActors;
		private System.Windows.Forms.ToolStripMenuItem m_assignActorToGroupToolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem m_editActorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem m_deleteActorToolStripMenuItem;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private System.Windows.Forms.DataGridViewTextBoxColumn GroupNumber;
		private DataGridViewListBoxColumn CharacterIds;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharStatus;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column5;

	}
}