namespace Glyssen
{
	partial class NarrationOptionsDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NarrationOptionsDlg));
			this.lblExplain = new System.Windows.Forms.Label();
			this.txtNarratorNum = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblExplain
			// 
			this.lblExplain.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblExplain, 2);
			this.lblExplain.ForeColor = System.Drawing.Color.White;
			this.lblExplain.Location = new System.Drawing.Point(3, 0);
			this.lblExplain.MaximumSize = new System.Drawing.Size(1050, 0);
			this.lblExplain.Name = "lblExplain";
			this.lblExplain.Padding = new System.Windows.Forms.Padding(0, 20, 20, 40);
			this.lblExplain.Size = new System.Drawing.Size(1050, 348);
			this.lblExplain.TabIndex = 0;
			this.lblExplain.Text = resources.GetString("lblExplain.Text");
			// 
			// txtNarratorNum
			// 
			this.txtNarratorNum.Location = new System.Drawing.Point(426, 351);
			this.txtNarratorNum.MaxLength = 2;
			this.txtNarratorNum.Name = "txtNarratorNum";
			this.txtNarratorNum.Size = new System.Drawing.Size(69, 38);
			this.txtNarratorNum.TabIndex = 0;
			this.txtNarratorNum.Text = "1";
			this.txtNarratorNum.WordWrap = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(10, 348);
			this.label1.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 0, 20, 40);
			this.label1.Size = new System.Drawing.Size(395, 72);
			this.label1.TabIndex = 2;
			this.label1.Text = "Desired number of narrators:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(3, 420);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
			this.label2.Size = new System.Drawing.Size(412, 52);
			this.label2.TabIndex = 3;
			this.label2.Text = "Narrator roles can be filled by:";
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.ForeColor = System.Drawing.Color.White;
			this.radioButton1.Location = new System.Drawing.Point(3, 517);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(346, 36);
			this.radioButton1.TabIndex = 2;
			this.radioButton1.Text = "Male Voice Actors Only";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.ForeColor = System.Drawing.Color.White;
			this.radioButton2.Location = new System.Drawing.Point(3, 559);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(379, 36);
			this.radioButton2.TabIndex = 3;
			this.radioButton2.Text = "Female Voice Actors Only";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Checked = true;
			this.radioButton3.ForeColor = System.Drawing.Color.White;
			this.radioButton3.Location = new System.Drawing.Point(3, 475);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(417, 36);
			this.radioButton3.TabIndex = 1;
			this.radioButton3.TabStop = true;
			this.radioButton3.Text = "Either Male or Female Actors";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.radioButton2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.radioButton3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.radioButton1, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtNarratorNum, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblExplain, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(21, 23);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1066, 645);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.button2, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.button1, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(654, 750);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(420, 105);
			this.tableLayoutPanel2.TabIndex = 8;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(3, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(168, 77);
			this.button1.TabIndex = 4;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(213, 3);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(186, 77);
			this.button2.TabIndex = 5;
			this.button2.Text = "Cancel";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// NarrationOptionsDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.ClientSize = new System.Drawing.Size(1099, 903);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NarrationOptionsDlg";
			this.Text = "Narration Preferences";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblExplain;
		private System.Windows.Forms.TextBox txtNarratorNum;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
	}
}