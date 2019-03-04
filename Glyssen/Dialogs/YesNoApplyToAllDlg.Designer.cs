namespace Glyssen.Dialogs
{
	partial class YesNoApplyToAllDlg
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
			System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
			this.m_lblMessage = new System.Windows.Forms.Label();
			this.m_chkApplyToAll = new System.Windows.Forms.CheckBox();
			this.m_btnYes = new System.Windows.Forms.Button();
			this.m_btnNo = new System.Windows.Forms.Button();
			flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			flowLayoutPanel1.AutoSize = true;
			flowLayoutPanel1.Controls.Add(this.m_lblMessage);
			flowLayoutPanel1.Controls.Add(this.m_chkApplyToAll);
			flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new System.Drawing.Size(392, 69);
			flowLayoutPanel1.TabIndex = 6;
			// 
			// m_lblMessage
			// 
			this.m_lblMessage.AutoSize = true;
			flowLayoutPanel1.SetFlowBreak(this.m_lblMessage, true);
			this.m_lblMessage.Location = new System.Drawing.Point(3, 0);
			this.m_lblMessage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this.m_lblMessage.Name = "m_lblMessage";
			this.m_lblMessage.Size = new System.Drawing.Size(14, 13);
			this.m_lblMessage.TabIndex = 1;
			this.m_lblMessage.Text = "#";
			// 
			// m_chkApplyToAll
			// 
			this.m_chkApplyToAll.AutoSize = true;
			flowLayoutPanel1.SetFlowBreak(this.m_chkApplyToAll, true);
			this.m_chkApplyToAll.Location = new System.Drawing.Point(3, 28);
			this.m_chkApplyToAll.Name = "m_chkApplyToAll";
			this.m_chkApplyToAll.Size = new System.Drawing.Size(77, 17);
			this.m_chkApplyToAll.TabIndex = 3;
			this.m_chkApplyToAll.Text = "&Apply to all";
			this.m_chkApplyToAll.UseVisualStyleBackColor = true;
			// 
			// m_btnYes
			// 
			this.m_btnYes.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.m_btnYes.Location = new System.Drawing.Point(130, 97);
			this.m_btnYes.Name = "m_btnYes";
			this.m_btnYes.Size = new System.Drawing.Size(75, 23);
			this.m_btnYes.TabIndex = 4;
			this.m_btnYes.Text = "&Yes";
			this.m_btnYes.UseVisualStyleBackColor = true;
			// 
			// m_btnNo
			// 
			this.m_btnNo.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.m_btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
			this.m_btnNo.Location = new System.Drawing.Point(211, 97);
			this.m_btnNo.Name = "m_btnNo";
			this.m_btnNo.Size = new System.Drawing.Size(75, 23);
			this.m_btnNo.TabIndex = 5;
			this.m_btnNo.Text = "&No";
			this.m_btnNo.UseVisualStyleBackColor = true;
			// 
			// YesNoApplyToAllDlg
			// 
			this.AcceptButton = this.m_btnYes;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.m_btnNo;
			this.ClientSize = new System.Drawing.Size(416, 131);
			this.Controls.Add(flowLayoutPanel1);
			this.Controls.Add(this.m_btnNo);
			this.Controls.Add(this.m_btnYes);
			this.MaximumSize = new System.Drawing.Size(6000, 170);
			this.MinimumSize = new System.Drawing.Size(16, 170);
			this.Name = "YesNoApplyToAllDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			flowLayoutPanel1.ResumeLayout(false);
			flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblMessage;
		private System.Windows.Forms.CheckBox m_chkApplyToAll;
		private System.Windows.Forms.Button m_btnYes;
		private System.Windows.Forms.Button m_btnNo;
	}
}