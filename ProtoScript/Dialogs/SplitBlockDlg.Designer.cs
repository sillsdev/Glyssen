namespace ProtoScript.Dialogs
{
	partial class SplitBlockDlg
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
			this.m_blocksDisplayBrowser = new ProtoScript.Controls.Browser();
			this.m_lblInstructions = new System.Windows.Forms.Label();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_lblInvalidSplitLocation = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.SplitBlockDlg";
			// 
			// m_blocksDisplayBrowser
			// 
			this.m_blocksDisplayBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_blocksDisplayBrowser.AutoSize = true;
			this.m_blocksDisplayBrowser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_blocksDisplayBrowser, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_blocksDisplayBrowser, "DialogBoxes.SplitBlockDlg.Browser");
			this.m_blocksDisplayBrowser.Location = new System.Drawing.Point(12, 39);
			this.m_blocksDisplayBrowser.Name = "m_blocksDisplayBrowser";
			this.m_blocksDisplayBrowser.Size = new System.Drawing.Size(414, 169);
			this.m_blocksDisplayBrowser.TabIndex = 1;
			this.m_blocksDisplayBrowser.OnMouseClick += new System.EventHandler<Gecko.DomMouseEventArgs>(this.InsertSplitLocation);
			// 
			// m_lblInstructions
			// 
			this.m_lblInstructions.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInstructions, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInstructions, "DialogBoxes.SplitBlockDlg.label1");
			this.m_lblInstructions.Location = new System.Drawing.Point(12, 13);
			this.m_lblInstructions.Name = "m_lblInstructions";
			this.m_lblInstructions.Size = new System.Drawing.Size(249, 13);
			this.m_lblInstructions.TabIndex = 2;
			this.m_lblInstructions.Text = "Click the location where you want to split the block.";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(351, 227);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 3;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOk.Enabled = false;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(270, 227);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 4;
			this.m_btnOk.Text = "OK";
			this.m_btnOk.UseVisualStyleBackColor = true;
			// 
			// m_lblInvalidSplitLocation
			// 
			this.m_lblInvalidSplitLocation.AutoSize = true;
			this.m_lblInvalidSplitLocation.ForeColor = System.Drawing.Color.Red;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblInvalidSplitLocation, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblInvalidSplitLocation, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblInvalidSplitLocation, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblInvalidSplitLocation, "DialogBoxes.SplitBlockDlg.SplitBlockDlg.m_lblInvalidSplitLocation");
			this.m_lblInvalidSplitLocation.Location = new System.Drawing.Point(13, 215);
			this.m_lblInvalidSplitLocation.Name = "m_lblInvalidSplitLocation";
			this.m_lblInvalidSplitLocation.Size = new System.Drawing.Size(165, 13);
			this.m_lblInvalidSplitLocation.TabIndex = 5;
			this.m_lblInvalidSplitLocation.Text = "This is not a valid location to split.";
			this.m_lblInvalidSplitLocation.Visible = false;
			// 
			// SplitBlockDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(438, 262);
			this.Controls.Add(this.m_lblInvalidSplitLocation);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_lblInstructions);
			this.Controls.Add(this.m_blocksDisplayBrowser);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "SplitBlockDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(454, 300);
			this.Name = "SplitBlockDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Split Block";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private Controls.Browser m_blocksDisplayBrowser;
		private System.Windows.Forms.Label m_lblInstructions;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Label m_lblInvalidSplitLocation;
	}
}