using GlyssenApp.Utilities;

namespace GlyssenApp.UI.Dialogs
{
	partial class SplitCharacterGroupDlg
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
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_lblAddOneOrMore = new System.Windows.Forms.Label();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnRemove = new System.Windows.Forms.Button();
			this.m_btnAdd = new System.Windows.Forms.Button();
			this.m_lblExistingGroup = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.m_tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.m_listboxExisting = new System.Windows.Forms.ListBox();
			this.m_listboxNew = new System.Windows.Forms.ListBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.glyssenColorPalette = new GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.SplitCharacterGroupDlg";
			// 
			// m_lblAddOneOrMore
			// 
			this.m_lblAddOneOrMore.AutoSize = true;
			this.m_lblAddOneOrMore.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblAddOneOrMore, GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.SetColumnSpan(this.m_lblAddOneOrMore, 3);
			this.glyssenColorPalette.SetForeColor(this.m_lblAddOneOrMore, GlyssenColors.ForeColor);
			this.m_lblAddOneOrMore.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblAddOneOrMore, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblAddOneOrMore, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblAddOneOrMore, "DialogBoxes.SelectExistingProjectDlg.AddOneOrMore");
			this.m_lblAddOneOrMore.Location = new System.Drawing.Point(0, 0);
			this.m_lblAddOneOrMore.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblAddOneOrMore.Name = "m_lblAddOneOrMore";
			this.m_lblAddOneOrMore.Size = new System.Drawing.Size(212, 13);
			this.m_lblAddOneOrMore.TabIndex = 0;
			this.m_lblAddOneOrMore.Text = "Add one or more characters to a new group";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblAddOneOrMore, true);
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(331, 386);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.m_btnOk_Click);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnCancel, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(412, 386);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnRemove
			// 
			this.m_btnRemove.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_btnRemove.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnRemove, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnRemove, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnRemove, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnRemove, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnRemove, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnRemove, "DialogBoxes.SplitCharacterGroupDlg.Remove");
			this.m_btnRemove.Location = new System.Drawing.Point(3, 150);
			this.m_btnRemove.Name = "m_btnRemove";
			this.m_btnRemove.Size = new System.Drawing.Size(72, 23);
			this.m_btnRemove.TabIndex = 3;
			this.m_btnRemove.Text = "<< Remove";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnRemove, false);
			this.m_btnRemove.UseVisualStyleBackColor = true;
			this.m_btnRemove.Click += new System.EventHandler(this.m_btnRemove_Click);
			// 
			// m_btnAdd
			// 
			this.m_btnAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.m_btnAdd.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnAdd, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_btnAdd, GlyssenColors.ForeColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnAdd, GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnAdd, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnAdd, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnAdd, "DialogBoxes.SplitCharacterGroupDlg.Add");
			this.m_btnAdd.Location = new System.Drawing.Point(3, 121);
			this.m_btnAdd.Name = "m_btnAdd";
			this.m_btnAdd.Size = new System.Drawing.Size(72, 23);
			this.m_btnAdd.TabIndex = 4;
			this.m_btnAdd.Text = "Add >>";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnAdd, false);
			this.m_btnAdd.UseVisualStyleBackColor = true;
			this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
			// 
			// m_lblExistingGroup
			// 
			this.m_lblExistingGroup.AutoSize = true;
			this.m_lblExistingGroup.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.m_lblExistingGroup, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblExistingGroup, GlyssenColors.ForeColor);
			this.m_lblExistingGroup.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblExistingGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblExistingGroup, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblExistingGroup, "DialogBoxes.SelectExistingProjectDlg.ExistingGroup");
			this.m_lblExistingGroup.Location = new System.Drawing.Point(0, 37);
			this.m_lblExistingGroup.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.m_lblExistingGroup.Name = "m_lblExistingGroup";
			this.m_lblExistingGroup.Size = new System.Drawing.Size(75, 13);
			this.m_lblExistingGroup.TabIndex = 6;
			this.m_lblExistingGroup.Text = "Existing Group";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_lblExistingGroup, true);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.glyssenColorPalette.SetBackColor(this.label2, GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label2, GlyssenColors.ForeColor);
			this.label2.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.SelectExistingProjectDlg.NewGroup");
			this.label2.Location = new System.Drawing.Point(278, 37);
			this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "New Group";
			this.glyssenColorPalette.SetUsePaletteColors(this.label2, true);
			// 
			// m_tableLayoutPanelMain
			// 
			this.m_tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutPanelMain, GlyssenColors.BackColor);
			this.m_tableLayoutPanelMain.ColumnCount = 3;
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.m_tableLayoutPanelMain.Controls.Add(this.m_listboxExisting, 0, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_listboxNew, 2, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.tableLayoutPanel1, 1, 3);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblAddOneOrMore, 0, 0);
			this.m_tableLayoutPanelMain.Controls.Add(this.m_lblExistingGroup, 0, 2);
			this.m_tableLayoutPanelMain.Controls.Add(this.label2, 2, 2);
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutPanelMain, GlyssenColors.Default);
			this.m_tableLayoutPanelMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			this.m_tableLayoutPanelMain.RowCount = 4;
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanelMain.Size = new System.Drawing.Size(472, 354);
			this.m_tableLayoutPanelMain.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutPanelMain, false);
			// 
			// m_listboxExisting
			// 
			this.glyssenColorPalette.SetBackColor(this.m_listboxExisting, GlyssenColors.BackColor);
			this.m_listboxExisting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_listboxExisting, GlyssenColors.ForeColor);
			this.m_listboxExisting.FormattingEnabled = true;
			this.m_listboxExisting.Location = new System.Drawing.Point(3, 57);
			this.m_listboxExisting.Name = "m_listboxExisting";
			this.m_listboxExisting.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.m_listboxExisting.Size = new System.Drawing.Size(188, 294);
			this.m_listboxExisting.Sorted = true;
			this.m_listboxExisting.TabIndex = 1;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_listboxExisting, false);
			this.m_listboxExisting.SelectedIndexChanged += new System.EventHandler(this.Listboxes_SelectedIndexChanged);
			// 
			// m_listboxNew
			// 
			this.glyssenColorPalette.SetBackColor(this.m_listboxNew, GlyssenColors.BackColor);
			this.m_listboxNew.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_listboxNew, GlyssenColors.ForeColor);
			this.m_listboxNew.FormattingEnabled = true;
			this.m_listboxNew.Location = new System.Drawing.Point(281, 57);
			this.m_listboxNew.Name = "m_listboxNew";
			this.m_listboxNew.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.m_listboxNew.Size = new System.Drawing.Size(188, 294);
			this.m_listboxNew.Sorted = true;
			this.m_listboxNew.TabIndex = 2;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_listboxNew, false);
			this.m_listboxNew.SelectedIndexChanged += new System.EventHandler(this.Listboxes_SelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_btnAdd, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.m_btnRemove, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(197, 57);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(78, 294);
			this.tableLayoutPanel1.TabIndex = 5;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// SplitCharacterGroupDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(502, 424);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutPanelMain);
			this.glyssenColorPalette.SetForeColor(this, GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.SplitCharacterGroupDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(518, 462);
			this.Name = "SplitCharacterGroupDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Split Character Group";
			this.glyssenColorPalette.SetUsePaletteColors(this, true);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutPanelMain.ResumeLayout(false);
			this.m_tableLayoutPanelMain.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanelMain;
		private System.Windows.Forms.Label m_lblAddOneOrMore;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.ListBox m_listboxExisting;
		private System.Windows.Forms.ListBox m_listboxNew;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button m_btnAdd;
		private System.Windows.Forms.Button m_btnRemove;
		private System.Windows.Forms.Label m_lblExistingGroup;
		private System.Windows.Forms.Label label2;
		private GlyssenColorPalette glyssenColorPalette;
	}
}