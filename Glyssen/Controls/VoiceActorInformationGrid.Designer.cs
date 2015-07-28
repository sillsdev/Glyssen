namespace Glyssen.Controls
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			this.m_contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_contextMenu_itemDeleteActors = new System.Windows.Forms.ToolStripMenuItem();
			this.m_deleteRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_dataGrid = new Glyssen.Controls.DataGridViewOverrideEnter();
			this.ActorId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ActorName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ActorGender = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ActorAge = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.m_contextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// m_contextMenu
			// 
			this.m_contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_contextMenu_itemDeleteActors});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_contextMenu, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_contextMenu, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_contextMenu, "DialogBoxes.VoiceActorInformation.DataGridContextMenu");
			this.m_contextMenu.Name = "contextMenuStrip1";
			this.m_contextMenu.ShowImageMargin = false;
			this.m_contextMenu.Size = new System.Drawing.Size(120, 26);
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
			this.m_contextMenu_itemDeleteActors.Size = new System.Drawing.Size(119, 22);
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
			this.m_dataGrid.AllowUserToDeleteRows = false;
			this.m_dataGrid.AllowUserToOrderColumns = true;
			this.m_dataGrid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
			this.m_dataGrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
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
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.m_dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.m_dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ActorId,
            this.ActorName,
            this.ActorGender,
            this.ActorAge});
			this.m_dataGrid.ContextMenuStrip = this.m_contextMenu;
			this.m_dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
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
			this.m_dataGrid.Name = "m_dataGrid";
			this.m_dataGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_dataGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.m_dataGrid.RowHeadersVisible = false;
			this.m_dataGrid.RowHeadersWidth = 22;
			dataGridViewCellStyle5.BackColor = System.Drawing.Color.Silver;
			dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Black;
			this.m_dataGrid.RowsDefaultCellStyle = dataGridViewCellStyle5;
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
			this.m_dataGrid.WaterMark = "!";
			this.m_dataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.m_dataGrid_CellEndEdit);
			this.m_dataGrid.CurrentCellChanged += new System.EventHandler(this.m_dataGrid_CurrentCellChanged);
			this.m_dataGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.m_dataGrid_EditingControlShowing);
			this.m_dataGrid.SelectionChanged += new System.EventHandler(this.m_dataGrid_SelectionChanged);
			this.m_dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleKeyDown);
			// 
			// ActorId
			// 
			this.ActorId.DataPropertyName = "Id";
			this.ActorId.HeaderText = "ID";
			this.ActorId.Name = "ActorId";
			this.ActorId.Visible = false;
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
			this.ActorGender.Items.AddRange(new object[] {
            "F - Female",
            "M - Male"});
			this.ActorGender.MinimumWidth = 90;
			this.ActorGender.Name = "ActorGender";
			this.ActorGender.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorGender.Width = 342;
			// 
			// ActorAge
			// 
			this.ActorAge.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ActorAge.DataPropertyName = "Age";
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
			this.ActorAge.DefaultCellStyle = dataGridViewCellStyle4;
			this.ActorAge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ActorAge.HeaderText = "_L10N_:DialogBoxes.VoiceActorInformation.Age!Age";
			this.ActorAge.Items.AddRange(new object[] {
            "O - Old",
            "Y - Young",
            "C - Child"});
			this.ActorAge.MinimumWidth = 90;
			this.ActorAge.Name = "ActorAge";
			this.ActorAge.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ActorAge.Width = 308;
			// 
			// VoiceActorInformationGrid
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
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

		private Glyssen.Controls.DataGridViewOverrideEnter m_dataGrid;
		private System.Windows.Forms.ContextMenuStrip m_contextMenu;
		private System.Windows.Forms.ToolStripMenuItem m_contextMenu_itemDeleteActors;
		private System.Windows.Forms.ToolStripMenuItem m_deleteRowsToolStripMenuItem;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.DataGridViewTextBoxColumn ActorId;
		private System.Windows.Forms.DataGridViewTextBoxColumn ActorName;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActorGender;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActorAge;
	}
}
