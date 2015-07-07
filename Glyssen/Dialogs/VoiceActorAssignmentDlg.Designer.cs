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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoiceActorAssignmentDlg));
			this.m_btnAssignActor = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.m_btnSave = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.m_characterGroupGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.GroupNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_voiceActorGrid = new Glyssen.Controls.VoiceActorInformationGrid();
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).BeginInit();
			this.m_tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnAssignActor
			// 
			this.m_btnAssignActor.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnAssignActor, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnAssignActor, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.button1");
			this.m_btnAssignActor.Location = new System.Drawing.Point(374, 321);
			this.m_btnAssignActor.Name = "m_btnAssignActor";
			this.m_btnAssignActor.Size = new System.Drawing.Size(128, 23);
			this.m_btnAssignActor.TabIndex = 1;
			this.m_btnAssignActor.Text = "Assign Actor to Group";
			this.m_btnAssignActor.UseVisualStyleBackColor = true;
			this.m_btnAssignActor.Click += new System.EventHandler(this.m_btnAssignActor_Click);
			// 
			// button2
			// 
			this.button2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.button2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.button2, null);
			this.l10NSharpExtender1.SetLocalizingId(this.button2, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.button2");
			this.button2.Location = new System.Drawing.Point(519, 372);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Export";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// m_btnSave
			// 
			this.m_btnSave.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnSave, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnSave, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnSave, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.button3");
			this.m_btnSave.Location = new System.Drawing.Point(437, 372);
			this.m_btnSave.Name = "m_btnSave";
			this.m_btnSave.Size = new System.Drawing.Size(75, 23);
			this.m_btnSave.TabIndex = 3;
			this.m_btnSave.Text = "Save";
			this.m_btnSave.UseVisualStyleBackColor = true;
			this.m_btnSave.Click += new System.EventHandler(this.m_btnSave_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.label1");
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(151, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Assign Actors to Groups";
			// 
			// m_characterGroupGrid
			// 
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
			this.m_characterGroupGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
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
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.Color.Silver;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.m_characterGroupGrid.DefaultCellStyle = dataGridViewCellStyle3;
			this.m_characterGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_characterGroupGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterGroupGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_characterGroupGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterGroupGrid.GridColor = System.Drawing.Color.Black;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_characterGroupGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_characterGroupGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_characterGroupGrid, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.betterGrid1");
			this.m_characterGroupGrid.Location = new System.Drawing.Point(3, 3);
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
			this.m_characterGroupGrid.Size = new System.Drawing.Size(355, 312);
			this.m_characterGroupGrid.TabIndex = 5;
			this.m_characterGroupGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.m_characterGroupGrid.WaterMark = "!";
			this.m_characterGroupGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.m_characterGroupGrid_CellMouseDoubleClick);
			this.m_characterGroupGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_characterGroupGrid_KeyDown);
			// 
			// GroupNumber
			// 
			this.GroupNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.GroupNumber.DataPropertyName = "GroupNumber";
			this.GroupNumber.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignment.GroupNumber!Grp #";
			this.GroupNumber.Name = "GroupNumber";
			this.GroupNumber.ReadOnly = true;
			this.GroupNumber.Width = 372;
			// 
			// Column2
			// 
			this.Column2.DataPropertyName = "CharactersString";
			this.Column2.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignment.Characters!Characters";
			this.Column2.Name = "Column2";
			this.Column2.ReadOnly = true;
			// 
			// Column3
			// 
			this.Column3.DataPropertyName = "RequiredAttributes";
			this.Column3.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignment.RequiredAttributes!Required";
			this.Column3.Name = "Column3";
			this.Column3.ReadOnly = true;
			// 
			// Column4
			// 
			this.Column4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.Column4.DataPropertyName = "EstimatedHours";
			this.Column4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignment.Hours!Hrs";
			this.Column4.Name = "Column4";
			this.Column4.ReadOnly = true;
			this.Column4.Width = 316;
			// 
			// Column5
			// 
			this.Column5.DataPropertyName = "VoiceActorAssignedName";
			this.Column5.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignment.ActorAssigned!Actor Assigned";
			this.Column5.Name = "Column5";
			this.Column5.ReadOnly = true;
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_tableLayoutPanel.ColumnCount = 3;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
			this.m_tableLayoutPanel.Controls.Add(this.m_characterGroupGrid, 0, 0);
			this.m_tableLayoutPanel.Controls.Add(this.m_voiceActorGrid, 1, 0);
			this.m_tableLayoutPanel.Controls.Add(this.button2, 2, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnSave, 1, 2);
			this.m_tableLayoutPanel.Controls.Add(this.m_btnAssignActor, 1, 1);
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(15, 31);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 3;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(597, 399);
			this.m_tableLayoutPanel.TabIndex = 6;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "DialogBoxes.VoiceActorAssignment";
			// 
			// m_voiceActorGrid
			// 
			this.m_voiceActorGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.m_voiceActorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_voiceActorGrid, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_voiceActorGrid, "DialogBoxes.VoiceActorAssignment.VoiceActorAssignmentDlg.VoiceActorInformationGri" +
        "d2");
			this.m_voiceActorGrid.Location = new System.Drawing.Point(364, 3);
			this.m_voiceActorGrid.Name = "m_voiceActorGrid";
			this.m_voiceActorGrid.Size = new System.Drawing.Size(148, 312);
			this.m_voiceActorGrid.TabIndex = 0;
			// 
			// VoiceActorAssignmentDlg
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "VoiceActorAssignmentDlg.WindowTitle");
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "VoiceActorAssignmentDlg";
			this.Text = "Voice Actor Assignment";
			((System.ComponentModel.ISupportInitialize)(this.m_characterGroupGrid)).EndInit();
			this.m_tableLayoutPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.VoiceActorInformationGrid m_voiceActorGrid;
		private System.Windows.Forms.Button m_btnAssignActor;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button m_btnSave;
		private System.Windows.Forms.Label label1;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterGroupGrid;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.DataGridViewTextBoxColumn GroupNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column5;

	}
}