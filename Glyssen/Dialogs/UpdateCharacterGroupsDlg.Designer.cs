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
			this.m_radioGenCharGrps = new System.Windows.Forms.RadioButton();
			this.m_radioSplitGroup = new System.Windows.Forms.RadioButton();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.UpdateCharacterGroupsDlg";
			// 
			// m_radioGenCharGrps
			// 
			this.m_radioGenCharGrps.AutoSize = true;
			this.m_radioGenCharGrps.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_radioGenCharGrps, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_radioGenCharGrps, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_radioGenCharGrps, "DialogBoxes.UpdateCharacterGroupsDlg.AutoGenerate");
			this.m_radioGenCharGrps.Location = new System.Drawing.Point(28, 30);
			this.m_radioGenCharGrps.MinimumSize = new System.Drawing.Size(359, 17);
			this.m_radioGenCharGrps.Name = "m_radioGenCharGrps";
			this.m_radioGenCharGrps.Size = new System.Drawing.Size(359, 17);
			this.m_radioGenCharGrps.TabIndex = 0;
			this.m_radioGenCharGrps.TabStop = true;
			this.m_radioGenCharGrps.Text = "Automatically generate character groups for the provided voice actors";
			this.m_radioGenCharGrps.UseVisualStyleBackColor = true;
			// 
			// m_radioSplitGroup
			// 
			this.m_radioSplitGroup.AutoSize = true;
			this.m_radioSplitGroup.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_radioSplitGroup, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_radioSplitGroup, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_radioSplitGroup, "DialogBoxes.UpdateCharacterGroupsDlg.ManualSplit");
			this.m_radioSplitGroup.Location = new System.Drawing.Point(28, 78);
			this.m_radioSplitGroup.Name = "m_radioSplitGroup";
			this.m_radioSplitGroup.Size = new System.Drawing.Size(227, 17);
			this.m_radioSplitGroup.TabIndex = 1;
			this.m_radioSplitGroup.TabStop = true;
			this.m_radioSplitGroup.Text = "Manually split the selected character group";
			this.m_radioSplitGroup.UseVisualStyleBackColor = true;
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(357, 186);
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
			this.m_btnOk.Location = new System.Drawing.Point(276, 186);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 3;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// UpdateCharacterGroupsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(444, 221);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_radioSplitGroup);
			this.Controls.Add(this.m_radioGenCharGrps);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "UpdateCharacterGroupsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpdateCharacterGroupsDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Update Character Groups";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.RadioButton m_radioGenCharGrps;
		private System.Windows.Forms.RadioButton m_radioSplitGroup;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
	}
}