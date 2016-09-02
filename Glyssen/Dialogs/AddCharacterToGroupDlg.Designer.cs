namespace Glyssen.Dialogs
{
	partial class AddCharacterToGroupDlg
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
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_toolStrip = new System.Windows.Forms.ToolStrip();
			this.m_toolStripLabelFindCharacter = new System.Windows.Forms.ToolStripLabel();
			this.m_toolStripTextBoxFindCharacter = new System.Windows.Forms.ToolStripTextBox();
			this.m_characterDetailsGrid = new SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid();
			this.CharacterDetailsIdCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsGenderCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsAgeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CharacterDetailsHoursCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
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
            this.m_toolStripLabelFindCharacter,
            this.m_toolStripTextBoxFindCharacter});
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_toolStrip, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_toolStrip, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStrip, "DialogBoxes.AddCharacterToGroupDlg.m_toolStrip");
			this.m_toolStrip.Location = new System.Drawing.Point(0, 0);
			this.m_toolStrip.Name = "m_toolStrip";
			this.m_toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.m_toolStrip.Size = new System.Drawing.Size(655, 25);
			this.m_toolStrip.TabIndex = 29;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStrip, false);
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
			this.m_l10NSharpExtender.SetLocalizingId(this.m_toolStripTextBoxFindCharacter, "DialogBoxes.AddCharacterToGroupDlg.m_toolStripTextBoxFindCharacter");
			this.m_toolStripTextBoxFindCharacter.Name = "m_toolStripTextBoxFindCharacter";
			this.m_toolStripTextBoxFindCharacter.Size = new System.Drawing.Size(120, 25);
			this.m_toolStripTextBoxFindCharacter.ToolTipText = "Begin typing a character ID to find the group that contains it";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_toolStripTextBoxFindCharacter, false);
			this.m_toolStripTextBoxFindCharacter.TextChanged += new System.EventHandler(this.m_toolStripTextBoxFindCharacter_TextChanged);
			// 
			// m_characterDetailsGrid
			// 
			this.m_characterDetailsGrid.AllowUserToAddRows = false;
			this.m_characterDetailsGrid.AllowUserToDeleteRows = false;
			this.m_characterDetailsGrid.AllowUserToOrderColumns = true;
			this.m_characterDetailsGrid.AllowUserToResizeRows = false;
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
			this.m_characterDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_characterDetailsGrid.DrawTextBoxEditControlBorder = false;
			this.m_characterDetailsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.m_characterDetailsGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.glyssenColorPalette.SetForeColor(this.m_characterDetailsGrid, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_characterDetailsGrid.FullRowFocusRectangleColor = System.Drawing.SystemColors.ControlDark;
			this.m_characterDetailsGrid.GridColor = System.Drawing.Color.Black;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_characterDetailsGrid, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_characterDetailsGrid, "DialogBoxes.AddCharacterToGroupDlg.m_characterDetailsGrid");
			this.m_characterDetailsGrid.Location = new System.Drawing.Point(12, 15);
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
			this.m_characterDetailsGrid.Size = new System.Drawing.Size(631, 320);
			this.m_characterDetailsGrid.TabIndex = 17;
			this.m_characterDetailsGrid.TextBoxEditControlBorderColor = System.Drawing.Color.Silver;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_characterDetailsGrid, false);
			this.m_characterDetailsGrid.VirtualMode = true;
			this.m_characterDetailsGrid.WaterMark = "!";
			this.m_characterDetailsGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.m_characterDetailsGrid_CellValueNeeded);
			this.m_characterDetailsGrid.SelectionChanged += new System.EventHandler(this.m_characterDetailsGrid_SelectionChanged);
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
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(556, 3);
			this.m_btnCancel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 28;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(475, 3);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 27;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
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
			dataGridViewCellStyle3.Format = "N2";
			this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridViewTextBoxColumn4.HeaderText = "_L10N_:DialogBoxes.VoiceActorAssignmentDlg.CharacterDetails.Hours!Hours";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// tableLayoutPanel1
			// 
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_characterDetailsGrid, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(12);
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(655, 379);
			this.tableLayoutPanel1.TabIndex = 31;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.flowLayoutPanel2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.flowLayoutPanel2.Controls.Add(this.m_btnCancel);
			this.flowLayoutPanel2.Controls.Add(this.m_btnOk);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.glyssenColorPalette.SetForeColor(this.flowLayoutPanel2, Glyssen.Utilities.GlyssenColors.Default);
			this.flowLayoutPanel2.Location = new System.Drawing.Point(12, 338);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(631, 29);
			this.flowLayoutPanel2.TabIndex = 32;
			this.glyssenColorPalette.SetUsePaletteColors(this.flowLayoutPanel2, false);
			// 
			// AddCharacterToGroupDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(655, 404);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_toolStrip);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.AddCharacterToGroupDlg.AddCharacterToGroup");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddCharacterToGroupDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Add Character to Group";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_toolStrip.ResumeLayout(false);
			this.m_toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_characterDetailsGrid)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private System.Windows.Forms.ToolStrip m_toolStrip;
		private System.Windows.Forms.ToolStripLabel m_toolStripLabelFindCharacter;
		private System.Windows.Forms.ToolStripTextBox m_toolStripTextBoxFindCharacter;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private SIL.Windows.Forms.Widgets.BetterGrid.BetterGrid m_characterDetailsGrid;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsIdCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsGenderCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsAgeCol;
		private System.Windows.Forms.DataGridViewTextBoxColumn CharacterDetailsHoursCol;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}