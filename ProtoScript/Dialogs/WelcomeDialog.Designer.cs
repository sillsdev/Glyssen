namespace ProtoScript.Dialogs
{
	partial class WelcomeDialog
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
			this.label1 = new System.Windows.Forms.Label();
			this.m_selectBundleBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(47, 64);
			this.label1.MaximumSize = new System.Drawing.Size(200, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(194, 52);
			this.label1.TabIndex = 0;
			this.label1.Text = "Welcome to the Protoscript Generator.  To generate proscripts, you will need a Te" +
    "xt Release Bundle from Paratext or the Digital Bible Library.";
			// 
			// m_selectBundleBtn
			// 
			this.m_selectBundleBtn.Location = new System.Drawing.Point(177, 210);
			this.m_selectBundleBtn.Name = "m_selectBundleBtn";
			this.m_selectBundleBtn.Size = new System.Drawing.Size(85, 23);
			this.m_selectBundleBtn.TabIndex = 1;
			this.m_selectBundleBtn.Text = "Select Bundle";
			this.m_selectBundleBtn.UseVisualStyleBackColor = true;
			this.m_selectBundleBtn.Click += new System.EventHandler(this.m_selectBundleBtn_Click);
			// 
			// WelcomeDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.m_selectBundleBtn);
			this.Controls.Add(this.label1);
			this.Name = "WelcomeDialog";
			this.Text = "WelcomeDialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button m_selectBundleBtn;
	}
}