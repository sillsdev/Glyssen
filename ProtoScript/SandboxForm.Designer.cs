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
			this.button2 = new System.Windows.Forms.Button();
			this.m_lblBundleId = new System.Windows.Forms.Label();
			this.btnSave = new System.Windows.Forms.Button();
			this.m_lblLanguage = new System.Windows.Forms.Label();
			this.m_btnExportToTabSeparated = new System.Windows.Forms.Button();
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
			// button2
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.button2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.button2, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.button2, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.button2, "SandboxForm.button2");
			this.button2.Location = new System.Drawing.Point(32, 206);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "L10NSharp";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
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
			// btnSave
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this.btnSave, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.btnSave, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.btnSave, L10NSharp.LocalizationPriority.High);
			this.l10NSharpExtender1.SetLocalizingId(this.btnSave, "SandboxForm.SandboxForm.btnSave");
			this.btnSave.Location = new System.Drawing.Point(32, 68);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(84, 23);
			this.btnSave.TabIndex = 4;
			this.btnSave.Text = "Save Project";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.HandleSave_Click);
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
			this.l10NSharpExtender1.SetLocalizableToolTip(this.m_btnExportToTabSeparated, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.m_btnExportToTabSeparated, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.m_btnExportToTabSeparated, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.m_btnExportToTabSeparated, "SandboxForm.SandboxForm.m_btnExportToTabSeparated");
			this.m_btnExportToTabSeparated.Location = new System.Drawing.Point(32, 98);
			this.m_btnExportToTabSeparated.Name = "m_btnExportToTabSeparated";
			this.m_btnExportToTabSeparated.Size = new System.Drawing.Size(84, 23);
			this.m_btnExportToTabSeparated.TabIndex = 6;
			this.m_btnExportToTabSeparated.Text = "Export...";
			this.m_btnExportToTabSeparated.UseVisualStyleBackColor = true;
			this.m_btnExportToTabSeparated.Click += new System.EventHandler(this.HandleExportToTabSeparated_Click);
			// 
			// SandboxForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(559, 262);
			this.Controls.Add(this.m_btnExportToTabSeparated);
			this.Controls.Add(this.m_lblLanguage);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.m_lblBundleId);
			this.Controls.Add(this.button2);
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
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label m_lblBundleId;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Label m_lblLanguage;
		private System.Windows.Forms.Button m_btnExportToTabSeparated;
	}
}

