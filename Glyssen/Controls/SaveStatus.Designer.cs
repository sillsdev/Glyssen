namespace Glyssen.Controls
{
	partial class SaveStatus
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_lbl = new System.Windows.Forms.Label();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lbl
			// 
			this.m_lbl.AutoSize = true;
			this.m_lbl.BackColor = System.Drawing.SystemColors.Control;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lbl, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lbl, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lbl, "Common.SaveStatus.Saved");
			this.m_lbl.Location = new System.Drawing.Point(-3, 0);
			this.m_lbl.Name = "m_lbl";
			this.m_lbl.Size = new System.Drawing.Size(97, 13);
			this.m_lbl.TabIndex = 0;
			this.m_lbl.Text = "All Changes Saved";
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Glyssen";
			this.l10NSharpExtender1.PrefixForNewItems = "Common.SaveStatus";
			// 
			// SaveStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.Controls.Add(this.m_lbl);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "Common.SaveStatus.SaveStatus.SaveStatus");
			this.Name = "SaveStatus";
			this.Size = new System.Drawing.Size(99, 15);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lbl;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
