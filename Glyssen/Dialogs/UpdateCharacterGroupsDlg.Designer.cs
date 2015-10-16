namespace Glyssen.Dialogs
{
	partial class UpdateCharacterGroupsDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateCharacterGroupsDlg));
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_radioMaintainOnlyCameo = new System.Windows.Forms.RadioButton();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_lblHeading = new System.Windows.Forms.Label();
			this.m_radioMaintainActors = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes";
			// 
			// m_radioMaintainOnlyCameo
			// 
			this.m_radioMaintainOnlyCameo.AutoSize = true;
			this.m_radioMaintainOnlyCameo.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_radioMaintainOnlyCameo, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_radioMaintainOnlyCameo, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_radioMaintainOnlyCameo, "DialogBoxes.UpdateCharacterGroupsDlg.MaintainOnlyCameo");
			this.m_radioMaintainOnlyCameo.Location = new System.Drawing.Point(3, 69);
			this.m_radioMaintainOnlyCameo.MaximumSize = new System.Drawing.Size(300, 50);
			this.m_radioMaintainOnlyCameo.MinimumSize = new System.Drawing.Size(359, 17);
			this.m_radioMaintainOnlyCameo.Name = "m_radioMaintainOnlyCameo";
			this.m_radioMaintainOnlyCameo.Size = new System.Drawing.Size(359, 17);
			this.m_radioMaintainOnlyCameo.TabIndex = 0;
			this.m_radioMaintainOnlyCameo.Text = "Do not maintain voice actor assignments (except cameos)";
			this.m_radioMaintainOnlyCameo.UseVisualStyleBackColor = true;
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(335, 170);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(254, 170);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 3;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_lblHeading
			// 
			this.m_lblHeading.AutoSize = true;
			this.m_lblHeading.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblHeading, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblHeading, "{0} is the application name");
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblHeading, "DialogBoxes.UpdateCharacterGroupsDlg.Heading");
			this.m_lblHeading.Location = new System.Drawing.Point(3, 0);
			this.m_lblHeading.Name = "m_lblHeading";
			this.m_lblHeading.Size = new System.Drawing.Size(379, 26);
			this.m_lblHeading.TabIndex = 5;
			this.m_lblHeading.Text = "{0} can optimize the number and composition of character groups to match the acto" +
    "rs you have entered.";
			// 
			// m_radioMaintainActors
			// 
			this.m_radioMaintainActors.AutoSize = true;
			this.m_radioMaintainActors.Checked = true;
			this.m_radioMaintainActors.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_radioMaintainActors, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_radioMaintainActors, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_radioMaintainActors, "DialogBoxes.UpdateCharacterGroupsDlg.MaintainActors");
			this.m_radioMaintainActors.Location = new System.Drawing.Point(3, 49);
			this.m_radioMaintainActors.MaximumSize = new System.Drawing.Size(300, 50);
			this.m_radioMaintainActors.MinimumSize = new System.Drawing.Size(359, 17);
			this.m_radioMaintainActors.Name = "m_radioMaintainActors";
			this.m_radioMaintainActors.Size = new System.Drawing.Size(359, 17);
			this.m_radioMaintainActors.TabIndex = 6;
			this.m_radioMaintainActors.TabStop = true;
			this.m_radioMaintainActors.Text = "Attempt to maintain all voice actor assignments";
			this.m_radioMaintainActors.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.m_radioMaintainActors, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.m_lblHeading, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.m_radioMaintainOnlyCameo, 0, 3);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(398, 152);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// UpdateCharacterGroupsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(422, 205);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.UpdateCharacterGroupsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpdateCharacterGroupsDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Update Character Groups";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.RadioButton m_radioMaintainOnlyCameo;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Label m_lblHeading;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.RadioButton m_radioMaintainActors;
	}
}