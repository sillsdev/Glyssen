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
			this.m_btnAssignActor = new System.Windows.Forms.Button();
			this.m_btnExport = new System.Windows.Forms.Button();
			this.m_btnSave = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_characterGroupGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.GroupNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.m_voiceActorGrid = new Glyssen.Controls.VoiceActorInformationGrid();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.m_tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnAssignActor
			// 
			this.m_btnAssignActor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnAssignActor.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnAssignActor, "DialogBoxes.VoiceActorAssignmentDlg.AssignActor");
			this.m_btnAssignActor.Location = new System.Drawing.Point(119, 344);
			this.m_btnAssignActor.Name = "m_btnAssignActor";
			this.m_btnAssignActor.Size = new System.Drawing.Size(128, 23);
			this.m_btnAssignActor.TabIndex = 3;
			this.m_btnAssignActor.Text = "Assign Actor to Group";
			this.m_btnAssignActor.UseVisualStyleBackColor = true;
			this.m_btnAssignActor.Click += new System.EventHandler(this.m_btnAssignActor_Click);
			// 
			// m_btnExport
			// 
			this.m_btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnExport, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnExport, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnExport, "DialogBoxes.VoiceActorAssignmentDlg.Export");
			this.m_btnExport.Location = new System.Drawing.Point(172, 373);
			this.m_btnExport.Name = "m_btnExport";
			this.m_btnExport.Size = new System.Drawing.Size(75, 23);
			this.m_btnExport.TabIndex = 5;
			this.m_btnExport.Text = "Export";
			this.m_btnExport.UseVisualStyleBackColor = true;
			this.m_btnExport.Click += new System.EventHandler(this.m_btnExport_Click);
			// 
			// m_btnSave
			// 
			this.m_btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnSave, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnSave, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnSave, "DialogBoxes.VoiceActorAssignmentDlg.Save");
			this.m_btnSave.Location = new System.Drawing.Point(91, 373);
			this.m_btnSave.Name = "m_btnSave";
			this.m_btnSave.Size = new System.Drawing.Size(75, 23);
			this.m_btnSave.TabIndex = 4;
			this.m_btnSave.Text = "Save";
			this.m_btnSave.UseVisualStyleBackColor = true;
			this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "DialogBoxes.VoiceActorAssignmentDlg.AssignActors");
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Assign Actors to Groups";
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
			this.m_characterGroupGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.m_characterGroupGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GroupNumber,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5});
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.Silver;
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
			this.m_characterGroupGrid.Location = new System.Drawing.Point(0, 0);
			this.m_characterGroupGrid.MultiSelect = false;
			this.m_characterGroupGrid.Name = "m_characterGroupGrid";
			this.m_characterGroupGrid.PaintHeaderAcrossFullGridWidth = true;
			this.m_characterGroupGrid.ReadOnly = true;
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
			this.m_characterGroupGrid.Size = new System.Drawing.Size(337, 399);
			this.m_characterGroupGrid.TabIndex = 1;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDoubleClick);
			this.m_characterGroupGrid.SelectionChanged += new System.EventHandler(this.m_eitherGrid_SelectionChanged);
			this.m_characterGroupGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragDrop);
			this.m_characterGroupGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.m_characterGroupGrid_DragOver);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			// 
			// GroupNumber
			// 
			this.GroupNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			this.GroupNumber.DataPropertyName = "GroupNumber";
			this.GroupNumber.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.GroupNumber!Group #";
			this.GroupNumber.Name = "GroupNumber";
			this.GroupNumber.ReadOnly = true;
			this.GroupNumber.Width = 5;
			// 
			// Column2
			// 
			this.Column2.DataPropertyName = "CharactersString";
			this.Column2.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Characters!Characters";
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			// 
			// Column3
			// 
			this.Column3.DataPropertyName = "RequiredAttributesString";
			this.Column3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.RequiredAttributes!Required";
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			// 
			// Column4
			// 
			this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.DataPropertyName = "EstimatedHours";
			dataGridViewCellStyle3.Format = "N2";
			dataGridViewCellStyle3.NullValue = null;
			this.Column4.DefaultCellStyle = dataGridViewCellStyle3;
			this.Column4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.Hours!Hours";
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column4.Width = 348;
			// 
			// Column5
			// 
			this.Column5.DataPropertyName = "VoiceActorAssignedName";
			this.Column5.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.ActorAssigned!Actor Assigned";
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorAssignmentDlg";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(15, 31);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.m_characterGroupGrid);
			this.splitContainer1.Panel1.Margin = new System.Windows.Forms.Padding(3);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.m_tableLayoutPanel);
			this.splitContainer1.Panel2.Controls.Add(this.m_btnSave);
			this.splitContainer1.Panel2.Controls.Add(this.m_btnExport);
			this.splitContainer1.Panel2.Margin = new System.Windows.Forms.Padding(3);
			this.splitContainer1.Size = new System.Drawing.Size(597, 399);
			this.splitContainer1.SplitterDistance = 337;
			this.splitContainer1.SplitterWidth = 10;
			this.splitContainer1.TabIndex = 0;
			this.splitContainer1.TabStop = false;
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanel.ColumnCount = 1;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.Controls.Add(this.m_voiceActorGrid, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnAssignActor, 0, 2);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.m_tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 3;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(250, 370);
			this.m_tableLayoutPanel.TabIndex = 2;
			// 
			// m_voiceActorGrid
			// 
			this.m_voiceActorGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_voiceActorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_voiceActorGrid, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_voiceActorGrid, "DialogBoxes.VoiceActorAssignmentDlg.VoiceActorInformationGrid");
			this.m_voiceActorGrid.Location = new System.Drawing.Point(3, 0);
			this.m_voiceActorGrid.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.m_voiceActorGrid.Name = "m_voiceActorGrid";
			this.m_voiceActorGrid.Size = new System.Drawing.Size(244, 328);
			this.m_voiceActorGrid.TabIndex = 2;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DialogBoxes.VoiceActorAssignmentDlg.WindowTitle");
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "VoiceActorAssignmentDlg";
			this.Text = "Voice Actor Assignment";
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VoiceActorAssignmentDlg_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.VoiceActorInformationGrid m_voiceActorGrid;
		private System.Windows.Forms.Button m_btnAssignActor;
		private System.Windows.Forms.Button m_btnExport;
		private System.Windows.Forms.Button m_btnSave;
		private System.Windows.Forms.Label label1;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterGroupGrid;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private System.Windows.Forms.DataGridViewTextBoxColumn GroupNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column5;

	}
}