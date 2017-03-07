namespace Glyssen.ReferenceTextUtility
{
	partial class OutputForm
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
			this.m_logBox = new SIL.Windows.Forms.Progress.LogBox();
			this.SuspendLayout();
			// 
			// m_logBox
			// 
			this.m_logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this.m_logBox.CancelRequested = false;
			this.m_logBox.ErrorEncountered = false;
			this.m_logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.m_logBox.GetDiagnosticsMethod = null;
			this.m_logBox.Location = new System.Drawing.Point(12, 12);
			this.m_logBox.Name = "m_logBox";
			this.m_logBox.ProgressIndicator = null;
			this.m_logBox.ShowCopyToClipboardMenuItem = false;
			this.m_logBox.ShowDetailsMenuItem = false;
			this.m_logBox.ShowDiagnosticsMenuItem = false;
			this.m_logBox.ShowFontMenuItem = false;
			this.m_logBox.ShowMenu = true;
			this.m_logBox.Size = new System.Drawing.Size(639, 442);
			this.m_logBox.TabIndex = 0;
			// 
			// OutputForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(663, 466);
			this.Controls.Add(this.m_logBox);
			this.Name = "OutputForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OutputForm";
			this.ResumeLayout(false);

		}

		#endregion

		private SIL.Windows.Forms.Progress.LogBox m_logBox;
	}
}