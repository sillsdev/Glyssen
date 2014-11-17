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
			this.components = new System.ComponentModel.Container();
			this.m_welcomeLabel = new System.Windows.Forms.Label();
			this.m_selectBundleBtn = new System.Windows.Forms.Button();
			this.m_l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// m_welcomeLabel
			// 
			this.m_welcomeLabel.AutoSize = true;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_welcomeLabel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_welcomeLabel, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_welcomeLabel, "WelcomeDialog.WelcomeText");
			this.m_welcomeLabel.Location = new System.Drawing.Point(47, 64);
			this.m_welcomeLabel.MaximumSize = new System.Drawing.Size(300, 0);
			this.m_welcomeLabel.Name = "m_welcomeLabel";
			this.m_welcomeLabel.Size = new System.Drawing.Size(265, 39);
			this.m_welcomeLabel.TabIndex = 0;
			this.m_welcomeLabel.Text = "Welcome to the Protoscript Generator.  To generate protoscripts, you will need a " +
    "Text Release Bundle from Paratext or the Digital Bible Library.";
			// 
			// m_selectBundleBtn
			// 
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_selectBundleBtn, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_selectBundleBtn, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_selectBundleBtn, "WelcomeDialog.SelectBundle");
			this.m_selectBundleBtn.Location = new System.Drawing.Point(303, 199);
			this.m_selectBundleBtn.Name = "m_selectBundleBtn";
			this.m_selectBundleBtn.Size = new System.Drawing.Size(85, 23);
			this.m_selectBundleBtn.TabIndex = 1;
			this.m_selectBundleBtn.Text = "Select Bundle";
			this.m_selectBundleBtn.UseVisualStyleBackColor = true;
			this.m_selectBundleBtn.Click += new System.EventHandler(this.m_selectBundleBtn_Click);
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "ProtoscriptGenerator";
			this.m_l10NSharpExtender.PrefixForNewItems = "WelcomeDialog";
			// 
			// WelcomeDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 262);
			this.Controls.Add(this.m_selectBundleBtn);
			this.Controls.Add(this.m_welcomeLabel);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "WelcomeDialog.WelcomeTitle");
			this.Name = "WelcomeDialog";
			this.Text = "Welcome to Protoscript Generator";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_welcomeLabel;
		private System.Windows.Forms.Button m_selectBundleBtn;
		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
	}
}