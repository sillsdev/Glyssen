namespace Glyssen.Dialogs
{
	partial class HtmlMessageDlg
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
			this.m_tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_browser = new Glyssen.Controls.Browser();
			this.m_tableLayoutPanel.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_tableLayoutPanel
			// 
			this.m_tableLayoutPanel.ColumnCount = 1;
			this.m_tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.Controls.Add(this.flowLayoutPanel1, 0, 1);
			this.m_tableLayoutPanel.Controls.Add(this.m_browser, 0, 0);
			this.m_tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.m_tableLayoutPanel.Name = "m_tableLayoutPanel";
			this.m_tableLayoutPanel.RowCount = 2;
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutPanel.Size = new System.Drawing.Size(484, 362);
			this.m_tableLayoutPanel.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.m_btnOk);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 319);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(484, 43);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Location = new System.Drawing.Point(399, 10);
			this.m_btnOk.Margin = new System.Windows.Forms.Padding(10);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 2;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_browser
			// 
			this.m_browser.AutoSize = true;
			this.m_browser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_browser.Location = new System.Drawing.Point(3, 3);
			this.m_browser.Name = "m_browser";
			this.m_browser.Size = new System.Drawing.Size(478, 313);
			this.m_browser.TabIndex = 1;
			this.m_browser.OnMouseClick += new System.EventHandler<Gecko.DomMouseEventArgs>(this.m_browser_OnMouseClick);
			// 
			// HtmlMessageDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 362);
			this.Controls.Add(this.m_tableLayoutPanel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "HtmlMessageDlg";
			this.ShowIcon = false;
			this.Text = "HtmlMessageDlg";
			this.Load += new System.EventHandler(this.HtmlMessageDlg_Load);
			this.m_tableLayoutPanel.ResumeLayout(false);
			this.m_tableLayoutPanel.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel m_tableLayoutPanel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button m_btnOk;
		private Controls.Browser m_browser;

	}
}