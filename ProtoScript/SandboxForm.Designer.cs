namespace ProtoScript
{
	partial class SandboxForm
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
			this.m_btnSelectBundle = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.m_btnLocalize = new System.Windows.Forms.Button();
			this.m_lblBundleId = new System.Windows.Forms.Label();
			this.m_lblLanguage = new System.Windows.Forms.Label();
			this.m_btnExportToTabSeparated = new System.Windows.Forms.Button();
			this.m_btnLoadSfm = new System.Windows.Forms.Button();
			this.m_btnSettings = new System.Windows.Forms.Button();
			this.m_btnAssign = new System.Windows.Forms.Button();
			this.m_btnSelectBooks = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnSelectBundle
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnSelectBundle, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnSelectBundle, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnSelectBundle, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnSelectBundle, "SandboxForm.SandboxForm.m_btnSelectBundle");
			this.m_btnSelectBundle.Location = new System.Drawing.Point(32, 38);
			this.m_btnSelectBundle.Name = "m_btnSelectBundle";
			this.m_btnSelectBundle.Size = new System.Drawing.Size(84, 23);
			this.m_btnSelectBundle.TabIndex = 0;
			this.m_btnSelectBundle.Text = "Open Project";
			this.m_btnSelectBundle.UseVisualStyleBackColor = true;
			this.m_btnSelectBundle.Click += new System.EventHandler(this.HandleSelectBundle_Click);
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "ProtoscriptGenerator";
			this.l10NSharpExtender1.PrefixForNewItems = "SandboxForm";
			// 
			// m_btnLocalize
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnLocalize, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnLocalize, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnLocalize, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnLocalize, "SandboxForm.button2");
			this.m_btnLocalize.Location = new System.Drawing.Point(32, 221);
			this.m_btnLocalize.Name = "m_btnLocalize";
			this.m_btnLocalize.Size = new System.Drawing.Size(84, 23);
			this.m_btnLocalize.TabIndex = 2;
			this.m_btnLocalize.Text = "L10NSharp";
			this.m_btnLocalize.UseVisualStyleBackColor = true;
			this.m_btnLocalize.Click += new System.EventHandler(this.m_btnLocalize_Click);
			// 
			// m_lblBundleId
			// 
			this.m_lblBundleId.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblBundleId, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblBundleId, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_lblBundleId, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblBundleId, "SandboxForm.SandboxForm.m_lblBundleId");
			this.m_lblBundleId.Location = new System.Drawing.Point(137, 43);
			this.m_lblBundleId.Name = "m_lblBundleId";
			this.m_lblBundleId.Size = new System.Drawing.Size(74, 13);
			this.m_lblBundleId.TabIndex = 3;
			this.m_lblBundleId.Text = "Bundle ID: {0}";
			// 
			// m_lblLanguage
			// 
			this.m_lblLanguage.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_lblLanguage, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_lblLanguage, null);
			this.l10NSharpExtender1.SetLocalizingId(this.m_lblLanguage, "SandboxForm.SandboxForm.m_lblLanguage");
			this.m_lblLanguage.Location = new System.Drawing.Point(341, 43);
			this.m_lblLanguage.Name = "m_lblLanguage";
			this.m_lblLanguage.Size = new System.Drawing.Size(165, 13);
			this.m_lblLanguage.TabIndex = 5;
			this.m_lblLanguage.Text = "Language code (ISO 639-02): {0}";
			// 
			// m_btnExportToTabSeparated
			// 
			this.m_btnExportToTabSeparated.Enabled = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnExportToTabSeparated, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnExportToTabSeparated, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnExportToTabSeparated, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnExportToTabSeparated, "SandboxForm.SandboxForm.m_btnExportToTabSeparated");
			this.m_btnExportToTabSeparated.Location = new System.Drawing.Point(32, 125);
			this.m_btnExportToTabSeparated.Name = "m_btnExportToTabSeparated";
			this.m_btnExportToTabSeparated.Size = new System.Drawing.Size(84, 23);
			this.m_btnExportToTabSeparated.TabIndex = 6;
			this.m_btnExportToTabSeparated.Text = "Export...";
			this.m_btnExportToTabSeparated.UseVisualStyleBackColor = true;
			this.m_btnExportToTabSeparated.Click += new System.EventHandler(this.HandleExportToTabSeparated_Click);
			// 
			// m_btnLoadSfm
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnLoadSfm, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnLoadSfm, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnLoadSfm, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnLoadSfm, "SandboxForm.SandboxForm.btnSave");
			this.m_btnLoadSfm.Location = new System.Drawing.Point(32, 163);
			this.m_btnLoadSfm.Name = "m_btnLoadSfm";
			this.m_btnLoadSfm.Size = new System.Drawing.Size(84, 23);
			this.m_btnLoadSfm.TabIndex = 6;
			this.m_btnLoadSfm.Text = "Load SFM";
			this.m_btnLoadSfm.UseVisualStyleBackColor = true;
			this.m_btnLoadSfm.Visible = false;
			this.m_btnLoadSfm.Click += new System.EventHandler(this.m_btnLoadSfm_Click);
			// 
			// m_btnSettings
			// 
			this.m_btnSettings.Enabled = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnSettings, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnSettings, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnSettings, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnSettings, "SandboxForm.button2");
			this.m_btnSettings.Location = new System.Drawing.Point(32, 192);
			this.m_btnSettings.Name = "m_btnSettings";
			this.m_btnSettings.Size = new System.Drawing.Size(84, 23);
			this.m_btnSettings.TabIndex = 7;
			this.m_btnSettings.Text = "Settings";
			this.m_btnSettings.UseVisualStyleBackColor = true;
			this.m_btnSettings.Click += new System.EventHandler(this.m_btnSettings_Click);
			// 
			// m_btnAssign
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnAssign, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnAssign, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnAssign, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnAssign, "SandboxForm.Assign");
			this.m_btnAssign.Location = new System.Drawing.Point(32, 96);
			this.m_btnAssign.Name = "m_btnAssign";
			this.m_btnAssign.Size = new System.Drawing.Size(84, 23);
			this.m_btnAssign.TabIndex = 8;
			this.m_btnAssign.Text = "Assign";
			this.m_btnAssign.UseVisualStyleBackColor = true;
			this.m_btnAssign.Click += new System.EventHandler(this.m_btnAssign_Click);
			// 
			// m_btnSelectBooks
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnSelectBooks, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnSelectBooks, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnSelectBooks, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnSelectBooks, "SandboxForm.SelectBooks");
			this.m_btnSelectBooks.Location = new System.Drawing.Point(32, 67);
			this.m_btnSelectBooks.Name = "m_btnSelectBooks";
			this.m_btnSelectBooks.Size = new System.Drawing.Size(84, 23);
			this.m_btnSelectBooks.TabIndex = 9;
			this.m_btnSelectBooks.Text = "Select Book(s)";
			this.m_btnSelectBooks.UseVisualStyleBackColor = true;
			this.m_btnSelectBooks.Click += new System.EventHandler(this.m_btnSelectBooks_Click);
			// 
			// SandboxForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(559, 262);
			this.Controls.Add(this.m_btnSelectBooks);
			this.Controls.Add(this.m_btnAssign);
			this.Controls.Add(this.m_btnSettings);
			this.Controls.Add(this.m_btnExportToTabSeparated);
			this.Controls.Add(this.m_btnLoadSfm);
			this.Controls.Add(this.m_lblLanguage);
			this.Controls.Add(this.m_lblBundleId);
			this.Controls.Add(this.m_btnLocalize);
			this.Controls.Add(this.m_btnSelectBundle);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SandboxForm.WindowTitle");
			this.Name = "SandboxForm";
			this.Text = "Protoscript Generator";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SandboxForm_FormClosing);
			this.Load += new System.EventHandler(this.SandboxForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnSelectBundle;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.Button m_btnLocalize;
		private System.Windows.Forms.Label m_lblBundleId;
		private System.Windows.Forms.Label m_lblLanguage;
		private System.Windows.Forms.Button m_btnExportToTabSeparated;
		private System.Windows.Forms.Button m_btnLoadSfm;
		private System.Windows.Forms.Button m_btnSettings;
		private System.Windows.Forms.Button m_btnAssign;
		private System.Windows.Forms.Button m_btnSelectBooks;
	}
}

