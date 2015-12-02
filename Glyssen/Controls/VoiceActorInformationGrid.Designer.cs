﻿namespace Glyssen.Controls
{
	partial class VoiceActorInformationGrid
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
				if (ParentForm != null)
					ParentForm.Closing -= ParentForm_Closing;
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_contextMenu_itemDeleteActors = new System.Windows.Forms.ToolStripMenuItem();
			this.m_deleteRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_dataGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.dataGridViewComboBoxColumn2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.dataGridViewComboBoxColumn3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.dataGridViewComboBoxColumn4 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ActorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ActorGender = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ActorAge = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ActorQuality = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ActorStatus = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.SpecialRole = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.DeleteButtonCol = new System.Windows.Forms.DataGridViewImageColumn();
			this.m_contextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// m_contextMenu
			// 
			this.m_contextMenu.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.m_contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_contextMenu_itemDeleteActors});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_contextMenu, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_contextMenu, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_contextMenu, "DialogBoxes.VoiceActorInformation.DataGridContextMenu");
			this.m_contextMenu.Name = "contextMenuStrip1";
			this.m_contextMenu.ShowImageMargin = false;
			this.m_contextMenu.Size = new System.Drawing.Size(250, 50);
			this.m_contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.m_contextMenu_Opening);
			this.m_contextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
			// 
			// m_contextMenu_itemDeleteActors
			// 
			this.m_contextMenu_itemDeleteActors.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_contextMenu_itemDeleteActors, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_contextMenu_itemDeleteActors, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_contextMenu_itemDeleteActors, "DialogBoxes.VoiceActorInformation.ContextMenu.DeleteActors");
			this.m_contextMenu_itemDeleteActors.Name = "m_contextMenu_itemDeleteActors";
			this.m_contextMenu_itemDeleteActors.Size = new System.Drawing.Size(249, 46);
			this.m_contextMenu_itemDeleteActors.Text = "Delete Actors";
			// 
			// m_deleteRowsToolStripMenuItem
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_deleteRowsToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_deleteRowsToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_deleteRowsToolStripMenuItem, "DialogBoxes.VoiceActorInformation.m_deleteRowsToolStripMenuItem");
			this.m_deleteRowsToolStripMenuItem.Name = "m_deleteRowsToolStripMenuItem";
			this.m_deleteRowsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorInformation";
			// 
			// m_dataGrid
			// 
			this.m_dataGrid.AllowUserToAddRows = false;
			this.m_dataGrid.AllowUserToDeleteRows = false;
			this.m_dataGrid.AllowUserToOrderColumns = true;
			this.m_dataGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
			this.m_dataGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.m_dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.m_dataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.m_dataGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.m_dataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.m_dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ActorName,
            this.ActorGender,
            this.ActorAge,
            this.ActorQuality,
            this.ActorStatus,
            this.SpecialRole,
            this.DeleteButtonCol});
			this.m_dataGrid.ContextMenuStrip = this.m_contextMenu;
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_dataGrid.DefaultCellStyle = dataGridViewCellStyle7;
			this.m_dataGrid.DrawTextBoxEditControlBorder = false;
			this.m_dataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.m_dataGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_dataGrid.FullRowFocusRectangleColor = System.Drawing.Color.Empty;
			this.m_dataGrid.GridColor = System.Drawing.Color.Black;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_dataGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_dataGrid, "VoiceActorInformationDlg.m_dataGrid");
			this.m_dataGrid.Location = new System.Drawing.Point(0, 0);
			this.m_dataGrid.Margin = new System.Windows.Forms.Padding(0);
			this.m_dataGrid.MultiSelect = false;
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_dataGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_dataGrid.RowHeadersVisible = false;
			this.m_dataGrid.RowHeadersWidth = 22;
			this.m_dataGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_dataGrid.SelectedCellBackColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedCellForeColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedRowBackColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectedRowForeColor = System.Drawing.Color.Empty;
			this.m_dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.m_dataGrid.ShowWaterMarkWhenDirty = false;
			this.m_dataGrid.Size = new System.Drawing.Size(384, 244);
			this.m_dataGrid.TabIndex = 4;
			this.m_dataGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Empty;
			this.m_dataGrid.VirtualMode = true;
			this.m_dataGrid.WaterMark = "!";
			this.m_dataGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGrid_CellContentClick);
			this.m_dataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGrid_CellEndEdit);
			this.m_dataGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGrid_CellMouseEnterOrLeave);
			this.m_dataGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGrid_CellMouseEnterOrLeave);
			this.m_dataGrid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.m_dataGrid_CellValidating);
			this.m_dataGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_dataGrid_CellValueNeeded);
			this.m_dataGrid.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_dataGrid_CellValuePushed);
			this.m_dataGrid.CurrentCellChanged += new System.EventHandler(this.m_dataGrid_CurrentCellChanged);
			this.m_dataGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.m_dataGrid_EditingControlShowing);
			this.m_dataGrid.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.m_dataGrid_NewRowNeeded);
			this.m_dataGrid.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.m_dataGrid_RowsAdded);
			this.m_dataGrid.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.m_dataGrid_RowsRemoved);
			this.m_dataGrid.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.m_dataGrid_RowValidating);
			this.m_dataGrid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.HandleUserDeletingRow);
			this.m_dataGrid.Leave += new System.EventHandler(this.m_dataGrid_Leave);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			dataGridViewCellStyle8.NullValue = "[new actor]";
			this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle8;
			this.dataGridViewTextBoxColumn1.Frozen = true;
			this.dataGridViewTextBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Name!Name";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ToolTipText = "Enter Voice Actor first and last name";
			// 
			// dataGridViewComboBoxColumn1
			// 
			this.dataGridViewComboBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn1.DataPropertyName = "Gender";
			this.dataGridViewComboBoxColumn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dataGridViewComboBoxColumn1.Frozen = true;
			this.dataGridViewComboBoxColumn1.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Gender!Gender";
			this.dataGridViewComboBoxColumn1.MinimumWidth = 90;
			this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
			this.dataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewComboBoxColumn2
			// 
			this.dataGridViewComboBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn2.DataPropertyName = "Age";
			dataGridViewCellStyle9.BackColor = System.Drawing.Color.White;
			this.dataGridViewComboBoxColumn2.DefaultCellStyle = dataGridViewCellStyle9;
			this.dataGridViewComboBoxColumn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dataGridViewComboBoxColumn2.Frozen = true;
			this.dataGridViewComboBoxColumn2.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.VoiceAge!Voice Age";
			this.dataGridViewComboBoxColumn2.MinimumWidth = 90;
			this.dataGridViewComboBoxColumn2.Name = "dataGridViewComboBoxColumn2";
			this.dataGridViewComboBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewComboBoxColumn3
			// 
			this.dataGridViewComboBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn3.DataPropertyName = "VoiceQuality";
			this.dataGridViewComboBoxColumn3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dataGridViewComboBoxColumn3.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.VoiceQuality!Voice Quality";
			this.dataGridViewComboBoxColumn3.MinimumWidth = 90;
			this.dataGridViewComboBoxColumn3.Name = "dataGridViewComboBoxColumn3";
			this.dataGridViewComboBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.dataGridViewComboBoxColumn3.Visible = false;
			// 
			// dataGridViewComboBoxColumn4
			// 
			this.dataGridViewComboBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn4.DataPropertyName = "SpecialRole";
			this.dataGridViewComboBoxColumn4.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.SpecialRole!Special Role";
			this.dataGridViewComboBoxColumn4.Name = "dataGridViewComboBoxColumn4";
			this.dataGridViewComboBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewComboBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// ActorName
			// 
			this.ActorName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ActorName.DataPropertyName = "Name";
			dataGridViewCellStyle3.NullValue = "[new actor]";
			this.ActorName.DefaultCellStyle = dataGridViewCellStyle3;
			this.ActorName.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Name!Name";
			this.ActorName.Name = "ActorName";
			this.ActorName.ToolTipText = "Enter Voice Actor first and last name";
			// 
			// ActorGender
			// 
			this.ActorGender.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ActorGender.DataPropertyName = "Gender";
			this.ActorGender.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ActorGender.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Gender!Gender";
			this.ActorGender.MinimumWidth = 90;
			this.ActorGender.Name = "ActorGender";
			this.ActorGender.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorGender.Width = 852;
			// 
			// ActorAge
			// 
			this.ActorAge.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ActorAge.DataPropertyName = "Age";
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
			this.ActorAge.DefaultCellStyle = dataGridViewCellStyle4;
			this.ActorAge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ActorAge.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.VoiceAge!Voice Age";
			this.ActorAge.MinimumWidth = 90;
			this.ActorAge.Name = "ActorAge";
			this.ActorAge.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorAge.Width = 914;
			// 
			// ActorQuality
			// 
			this.ActorQuality.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ActorQuality.DataPropertyName = "VoiceQuality";
			this.ActorQuality.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ActorQuality.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.VoiceQuality!Voice Quality";
			this.ActorQuality.MinimumWidth = 90;
			this.ActorQuality.Name = "ActorQuality";
			this.ActorQuality.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorQuality.Visible = false;
			this.ActorQuality.Width = 996;
			// 
			// ActorStatus
			// 
			this.ActorStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ActorStatus.DataPropertyName = "Status";
			this.ActorStatus.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Status!Status";
			this.ActorStatus.Name = "ActorStatus";
			this.ActorStatus.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.ActorStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorStatus.Visible = false;
			this.ActorStatus.Width = 816;
			// 
			// SpecialRole
			// 
			this.SpecialRole.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.SpecialRole.DataPropertyName = "SpecialRole";
			dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
			this.SpecialRole.DefaultCellStyle = dataGridViewCellStyle5;
			this.SpecialRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SpecialRole.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.SpecialRole!Special Role";
			this.SpecialRole.Items.AddRange(new object[] {
            "None",
            "Cameo",
            "Narrator"});
			this.SpecialRole.Name = "SpecialRole";
			this.SpecialRole.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.SpecialRole.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.SpecialRole.Width = 966;
			// 
			// DeleteButtonCol
			// 
			this.DeleteButtonCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.DeleteButtonCol.DefaultCellStyle = dataGridViewCellStyle6;
			this.DeleteButtonCol.HeaderText = "";
			this.DeleteButtonCol.Image = global::Glyssen.Properties.Resources.RemoveGridRowNormal;
			this.DeleteButtonCol.MinimumWidth = 20;
			this.DeleteButtonCol.Name = "DeleteButtonCol";
			this.DeleteButtonCol.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.DeleteButtonCol.ToolTipText = "Delete this voice actor";
			this.DeleteButtonCol.Width = 20;
			// 
			// VoiceActorInformationGrid
			// 
			this.Controls.Add(this.m_dataGrid);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorInformation.VoiceActorInformationGrid2.VoiceActorInformatio" +
        "nGrid2");
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "VoiceActorInformationGrid";
			this.Size = new System.Drawing.Size(384, 244);
			this.m_contextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_dataGrid;
		private System.Windows.Forms.ContextMenuStrip m_contextMenu;
		private System.Windows.Forms.ToolStripMenuItem m_contextMenu_itemDeleteActors;
		private System.Windows.Forms.ToolStripMenuItem m_deleteRowsToolStripMenuItem;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn2;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn3;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn4;
		private System.Windows.Forms.DataGridViewTextBoxColumn ActorName;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActorGender;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActorAge;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActorQuality;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ActorStatus;
		private System.Windows.Forms.DataGridViewComboBoxColumn SpecialRole;
		private System.Windows.Forms.DataGridViewImageColumn DeleteButtonCol;
	}
}
