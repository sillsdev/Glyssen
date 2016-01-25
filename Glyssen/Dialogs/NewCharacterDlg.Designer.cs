namespace Glyssen.Dialogs
{
	partial class NewCharacterDlg
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
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_cmbGender = new System.Windows.Forms.ComboBox();
			this.m_cmbAge = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.NewCharacterDlg";
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(21, 3);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(102, 3);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblInstructions, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.SetColumnSpan(this.m_lblInstructions, 2);
			this.glyssenColorPalette.SetForeColor(this.m_lblInstructions, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblInstructions.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.NewCharacterDlg.Instructions");
			this.m_lblInstructions.Location = new System.Drawing.Point(3, 0);
			this.m_lblInstructions.MaximumSize = new System.Drawing.Size(403, 70);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(397, 35);
			this.m_lblInstructions.TabIndex = 17;
			this.m_lblInstructions.Text = "You are creating a new character named {0}. Please provide attributes for this ch" +
    "aracter.";
			// 
			// m_cmbGender
			// 
			this.glyssenColorPalette.SetBackColor(this.m_cmbGender, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_cmbGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbGender.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.glyssenColorPalette.SetForeColor(this.m_cmbGender, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_cmbGender.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cmbGender, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cmbGender, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cmbGender, "DialogBoxes.NewCharacterDlg.comboBox1");
			this.m_cmbGender.Location = new System.Drawing.Point(51, 38);
			this.m_cmbGender.Name = "m_cmbGender";
			this.m_cmbGender.Size = new System.Drawing.Size(171, 21);
			this.m_cmbGender.TabIndex = 18;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_cmbGender, false);
			// 
			// m_cmbAge
			// 
			this.glyssenColorPalette.SetBackColor(this.m_cmbAge, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_cmbAge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbAge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.glyssenColorPalette.SetForeColor(this.m_cmbAge, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_cmbAge.FormattingEnabled = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_cmbAge, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_cmbAge, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_cmbAge, "DialogBoxes.NewCharacterDlg.comboBox1");
			this.m_cmbAge.Location = new System.Drawing.Point(51, 65);
			this.m_cmbAge.Name = "m_cmbAge";
			this.m_cmbAge.Size = new System.Drawing.Size(171, 21);
			this.m_cmbAge.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_cmbAge, false);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.label1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label1, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label1, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label1, "DialogBoxes.NewCharacterDlg.Gender");
			this.label1.Location = new System.Drawing.Point(3, 42);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(42, 13);
			this.label1.TabIndex = 20;
			this.label1.Text = "Gender";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.label2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.label2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label2.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.NewCharacterDlg.Age");
			this.label2.Location = new System.Drawing.Point(3, 69);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 13);
			this.label2.TabIndex = 21;
			this.label2.Text = "Age";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblInstructions, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_cmbAge, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_cmbGender, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.tableLayoutPanel1, Glyssen.Utilities.GlyssenColors.Default);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 15);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(403, 169);
			this.tableLayoutPanel1.TabIndex = 20;
			this.glyssenColorPalette.SetUsePaletteColors(this.tableLayoutPanel1, false);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.panel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.panel1.Controls.Add(this.m_btnCancel);
			this.panel1.Controls.Add(this.m_btnOk);
			this.glyssenColorPalette.SetForeColor(this.panel1, Glyssen.Utilities.GlyssenColors.Default);
			this.panel1.Location = new System.Drawing.Point(220, 137);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(180, 29);
			this.panel1.TabIndex = 21;
			this.glyssenColorPalette.SetUsePaletteColors(this.panel1, false);
			// 
			// NewCharacterDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(433, 199);
			this.Controls.Add(this.tableLayoutPanel1);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.SplitCharacterGroupDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(280, 212);
			this.Name = "NewCharacterDlg";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add New Character";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblInstructions;
		private System.Windows.Forms.ComboBox m_cmbGender;
		private System.Windows.Forms.ComboBox m_cmbAge;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}