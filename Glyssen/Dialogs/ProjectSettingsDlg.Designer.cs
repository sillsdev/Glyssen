namespace Glyssen.Dialogs
{
	partial class ProjectSettingsDlg
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
				if (UpdatedBundle != null)
				{
					UpdatedBundle.Dispose();
					UpdatedBundle = null;
				}
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblLanguageName = new System.Windows.Forms.Label();
			this.m_lblIso639_2_Code = new System.Windows.Forms.Label();
			this.m_lblPublicationName = new System.Windows.Forms.Label();
			this.m_lblPublicationId = new System.Windows.Forms.Label();
			this.m_btnOk = new System.Windows.Forms.Button();
			this.m_wsFontControl = new SIL.Windows.Forms.WritingSystems.WSFontControl();
			this.m_lblRecordingProjectName = new System.Windows.Forms.Label();
			this.m_txtRecordingProjectName = new System.Windows.Forms.TextBox();
			this.m_lblWritingSystem = new System.Windows.Forms.Label();
			this.m_lblWsSeparator = new System.Windows.Forms.Label();
			this.m_txtLanguageName = new System.Windows.Forms.Label();
			this.m_txtIso639_2_Code = new System.Windows.Forms.Label();
			this.m_txtPublicationName = new System.Windows.Forms.Label();
			this.m_txtPublicationId = new System.Windows.Forms.Label();
			this.m_lblOriginalBundlePath = new System.Windows.Forms.Label();
			this.m_txtOriginalBundlePath = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.m_lblQuoteMarks = new System.Windows.Forms.Label();
			this.m_lblQuoteMarkReview = new System.Windows.Forms.Label();
			this.m_lblQuoteMarkSummary = new System.Windows.Forms.Label();
			this.m_btnQuoteMarkSettings = new System.Windows.Forms.Button();
			this.m_txtVersification = new System.Windows.Forms.Label();
			this.m_lblVersification = new System.Windows.Forms.Label();
			this.m_lblSummary = new System.Windows.Forms.Label();
			this.m_btnUpdateFromBundle = new System.Windows.Forms.Button();
			this.m_tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
			this.m_panelWritingSystemLabelAndSeparator = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).BeginInit();
			this.m_tableLayoutMain.SuspendLayout();
			this.m_panelWritingSystemLabelAndSeparator.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_l10NSharpExtender
			// 
			this.m_l10NSharpExtender.LocalizationManagerId = "Glyssen";
			this.m_l10NSharpExtender.PrefixForNewItems = "DialogBoxes.ProjectSettingsDlg";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.glyssenColorPalette.SetForeColor(this.m_btnCancel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnCancel, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnCancel, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnCancel, "Common.Cancel");
			this.m_btnCancel.Location = new System.Drawing.Point(437, 555);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 2;
			this.m_btnCancel.Text = "Cancel";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnCancel, false);
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_lblLanguageName
			// 
			this.m_lblLanguageName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblLanguageName.AutoSize = true;
			this.m_lblLanguageName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblLanguageName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblLanguageName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblLanguageName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblLanguageName, "DialogBoxes.ProjectSettingsDlg.LanguageName");
			this.m_lblLanguageName.Location = new System.Drawing.Point(3, 85);
			this.m_lblLanguageName.Name = "m_lblLanguageName";
			this.m_lblLanguageName.Size = new System.Drawing.Size(89, 13);
			this.m_lblLanguageName.TabIndex = 0;
			this.m_lblLanguageName.Text = "Language Name:";
			// 
			// m_lblIso639_2_Code
			// 
			this.m_lblIso639_2_Code.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblIso639_2_Code.AutoSize = true;
			this.m_lblIso639_2_Code.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblIso639_2_Code, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblIso639_2_Code, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblIso639_2_Code.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblIso639_2_Code, "DialogBoxes.ProjectSettingsDlg.EthnologueCode");
			this.m_lblIso639_2_Code.Location = new System.Drawing.Point(3, 98);
			this.m_lblIso639_2_Code.Name = "m_lblIso639_2_Code";
			this.m_lblIso639_2_Code.Size = new System.Drawing.Size(149, 13);
			this.m_lblIso639_2_Code.TabIndex = 2;
			this.m_lblIso639_2_Code.Text = "Ethnologue (ISO 639-2) Code:";
			// 
			// m_lblPublicationName
			// 
			this.m_lblPublicationName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblPublicationName.AutoSize = true;
			this.m_lblPublicationName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblPublicationName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblPublicationName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblPublicationName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblPublicationName, "DialogBoxes.ProjectSettingsDlg.PublicationName");
			this.m_lblPublicationName.Location = new System.Drawing.Point(3, 126);
			this.m_lblPublicationName.Name = "m_lblPublicationName";
			this.m_lblPublicationName.Size = new System.Drawing.Size(93, 13);
			this.m_lblPublicationName.TabIndex = 4;
			this.m_lblPublicationName.Text = "Publication Name:";
			// 
			// m_lblPublicationId
			// 
			this.m_lblPublicationId.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblPublicationId.AutoSize = true;
			this.m_lblPublicationId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblPublicationId, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblPublicationId, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblPublicationId.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblPublicationId, "DialogBoxes.ProjectSettingsDlg.PublicationId");
			this.m_lblPublicationId.Location = new System.Drawing.Point(3, 139);
			this.m_lblPublicationId.Name = "m_lblPublicationId";
			this.m_lblPublicationId.Size = new System.Drawing.Size(74, 13);
			this.m_lblPublicationId.TabIndex = 6;
			this.m_lblPublicationId.Text = "Publication Id:";
			// 
			// m_btnOk
			// 
			this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnOk, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnOk, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_btnOk, L10NSharp.LocalizationPriority.High);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnOk, "Common.OK");
			this.m_btnOk.Location = new System.Drawing.Point(356, 555);
			this.m_btnOk.Name = "m_btnOk";
			this.m_btnOk.Size = new System.Drawing.Size(75, 23);
			this.m_btnOk.TabIndex = 1;
			this.m_btnOk.Text = "OK";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnOk, false);
			this.m_btnOk.UseVisualStyleBackColor = true;
			this.m_btnOk.Click += new System.EventHandler(this.HandleOkButtonClick);
			// 
			// m_wsFontControl
			// 
			this.m_wsFontControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_wsFontControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_wsFontControl, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_wsFontControl, 4);
			this.m_wsFontControl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
			this.glyssenColorPalette.SetForeColor(this.m_wsFontControl, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_wsFontControl.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_wsFontControl, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_wsFontControl, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_wsFontControl, "DialogBoxes.ProjectSettingsDlg.WSFontControl");
			this.m_wsFontControl.Location = new System.Drawing.Point(3, 325);
			this.m_wsFontControl.Name = "m_wsFontControl";
			this.m_wsFontControl.Size = new System.Drawing.Size(491, 196);
			this.m_wsFontControl.TabIndex = 7;
			this.m_wsFontControl.TestAreaText = "";
			// 
			// m_lblRecordingProjectName
			// 
			this.m_lblRecordingProjectName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_lblRecordingProjectName.AutoSize = true;
			this.m_lblRecordingProjectName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblRecordingProjectName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblRecordingProjectName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblRecordingProjectName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblRecordingProjectName, "DialogBoxes.ProjectSettingsDlg.RecordingProjectName");
			this.m_lblRecordingProjectName.Location = new System.Drawing.Point(3, 6);
			this.m_lblRecordingProjectName.Name = "m_lblRecordingProjectName";
			this.m_lblRecordingProjectName.Size = new System.Drawing.Size(126, 13);
			this.m_lblRecordingProjectName.TabIndex = 12;
			this.m_lblRecordingProjectName.Text = "Recording Project Name:";
			// 
			// m_txtRecordingProjectName
			// 
			this.m_txtRecordingProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_txtRecordingProjectName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtRecordingProjectName, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtRecordingProjectName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtRecordingProjectName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtRecordingProjectName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtRecordingProjectName, "DialogBoxes.ProjectSettingsDlg.m_txtProjectName");
			this.m_txtRecordingProjectName.Location = new System.Drawing.Point(158, 3);
			this.m_txtRecordingProjectName.Name = "m_txtRecordingProjectName";
			this.m_txtRecordingProjectName.Size = new System.Drawing.Size(336, 20);
			this.m_txtRecordingProjectName.TabIndex = 13;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_txtRecordingProjectName, false);
			// 
			// m_lblWritingSystem
			// 
			this.m_lblWritingSystem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblWritingSystem.AutoSize = true;
			this.m_lblWritingSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblWritingSystem, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblWritingSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblWritingSystem, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblWritingSystem.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWritingSystem, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWritingSystem, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWritingSystem, "DialogBoxes.ProjectSettingsDlg.Writing System");
			this.m_lblWritingSystem.Location = new System.Drawing.Point(0, 22);
			this.m_lblWritingSystem.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.m_lblWritingSystem.Name = "m_lblWritingSystem";
			this.m_lblWritingSystem.Size = new System.Drawing.Size(102, 15);
			this.m_lblWritingSystem.TabIndex = 15;
			this.m_lblWritingSystem.Text = "Writing System";
			this.m_lblWritingSystem.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// m_lblWsSeparator
			// 
			this.m_lblWsSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblWsSeparator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblWsSeparator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblWsSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.glyssenColorPalette.SetForeColor(this.m_lblWsSeparator, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblWsSeparator.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblWsSeparator, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblWsSeparator, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblWsSeparator, "DialogBoxes.ProjectSettingsDlg.label1");
			this.m_lblWsSeparator.Location = new System.Drawing.Point(8, 32);
			this.m_lblWsSeparator.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
			this.m_lblWsSeparator.Name = "m_lblWsSeparator";
			this.m_lblWsSeparator.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.m_lblWsSeparator.Size = new System.Drawing.Size(475, 2);
			this.m_lblWsSeparator.TabIndex = 16;
			// 
			// m_txtLanguageName
			// 
			this.m_txtLanguageName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtLanguageName.AutoSize = true;
			this.m_txtLanguageName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtLanguageName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtLanguageName, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtLanguageName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtLanguageName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtLanguageName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtLanguageName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtLanguageName, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtLanguageName");
			this.m_txtLanguageName.Location = new System.Drawing.Point(158, 85);
			this.m_txtLanguageName.Name = "m_txtLanguageName";
			this.m_txtLanguageName.Size = new System.Drawing.Size(336, 13);
			this.m_txtLanguageName.TabIndex = 23;
			this.m_txtLanguageName.Text = "#";
			// 
			// m_txtIso639_2_Code
			// 
			this.m_txtIso639_2_Code.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtIso639_2_Code.AutoSize = true;
			this.m_txtIso639_2_Code.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtIso639_2_Code, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtIso639_2_Code, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtIso639_2_Code, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtIso639_2_Code.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtIso639_2_Code, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtIso639_2_Code, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtIso639_2_Code, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtIso639_2_Code");
			this.m_txtIso639_2_Code.Location = new System.Drawing.Point(158, 98);
			this.m_txtIso639_2_Code.Name = "m_txtIso639_2_Code";
			this.m_txtIso639_2_Code.Size = new System.Drawing.Size(336, 13);
			this.m_txtIso639_2_Code.TabIndex = 24;
			this.m_txtIso639_2_Code.Text = "#";
			// 
			// m_txtPublicationName
			// 
			this.m_txtPublicationName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtPublicationName.AutoSize = true;
			this.m_txtPublicationName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtPublicationName, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtPublicationName, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtPublicationName, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtPublicationName.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtPublicationName, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtPublicationName, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtPublicationName, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtPublicationName");
			this.m_txtPublicationName.Location = new System.Drawing.Point(158, 126);
			this.m_txtPublicationName.Name = "m_txtPublicationName";
			this.m_txtPublicationName.Size = new System.Drawing.Size(336, 13);
			this.m_txtPublicationName.TabIndex = 25;
			this.m_txtPublicationName.Text = "#";
			// 
			// m_txtPublicationId
			// 
			this.m_txtPublicationId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtPublicationId.AutoSize = true;
			this.m_txtPublicationId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtPublicationId, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtPublicationId, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtPublicationId, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtPublicationId.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtPublicationId, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtPublicationId, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtPublicationId, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtPublicationId");
			this.m_txtPublicationId.Location = new System.Drawing.Point(158, 139);
			this.m_txtPublicationId.Name = "m_txtPublicationId";
			this.m_txtPublicationId.Size = new System.Drawing.Size(336, 13);
			this.m_txtPublicationId.TabIndex = 26;
			this.m_txtPublicationId.Text = "#";
			// 
			// m_lblOriginalBundlePath
			// 
			this.m_lblOriginalBundlePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblOriginalBundlePath.AutoSize = true;
			this.m_lblOriginalBundlePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblOriginalBundlePath, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblOriginalBundlePath, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblOriginalBundlePath.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblOriginalBundlePath, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblOriginalBundlePath, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_lblOriginalBundlePath");
			this.m_lblOriginalBundlePath.Location = new System.Drawing.Point(3, 41);
			this.m_lblOriginalBundlePath.Name = "m_lblOriginalBundlePath";
			this.m_lblOriginalBundlePath.Size = new System.Drawing.Size(106, 29);
			this.m_lblOriginalBundlePath.TabIndex = 28;
			this.m_lblOriginalBundlePath.Text = "Original Bundle Path:";
			this.m_lblOriginalBundlePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_txtOriginalBundlePath
			// 
			this.m_txtOriginalBundlePath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtOriginalBundlePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtOriginalBundlePath, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtOriginalBundlePath, 2);
			this.glyssenColorPalette.SetForeColor(this.m_txtOriginalBundlePath, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtOriginalBundlePath.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtOriginalBundlePath, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtOriginalBundlePath, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtOriginalBundlePath, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtOriginalBundlePath");
			this.m_txtOriginalBundlePath.Location = new System.Drawing.Point(158, 41);
			this.m_txtOriginalBundlePath.Name = "m_txtOriginalBundlePath";
			this.m_txtOriginalBundlePath.Size = new System.Drawing.Size(248, 29);
			this.m_txtOriginalBundlePath.TabIndex = 29;
			this.m_txtOriginalBundlePath.Text = "#";
			this.m_txtOriginalBundlePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.label2, Glyssen.Utilities.GlyssenColors.BackColor);
			this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.glyssenColorPalette.SetForeColor(this.label2, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.label2.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.label2, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.label2, "DialogBoxes.ProjectSettingsDlg.label1");
			this.label2.Location = new System.Drawing.Point(8, 32);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.label2.Size = new System.Drawing.Size(475, 2);
			this.label2.TabIndex = 16;
			// 
			// m_lblQuoteMarks
			// 
			this.m_lblQuoteMarks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblQuoteMarks.AutoSize = true;
			this.m_lblQuoteMarks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarks, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblQuoteMarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarks, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblQuoteMarks.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarks, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarks, "DialogBoxes.ProjectSettingsDlg.QuoteMarks");
			this.m_lblQuoteMarks.Location = new System.Drawing.Point(0, 22);
			this.m_lblQuoteMarks.Name = "m_lblQuoteMarks";
			this.m_lblQuoteMarks.Size = new System.Drawing.Size(88, 15);
			this.m_lblQuoteMarks.TabIndex = 15;
			this.m_lblQuoteMarks.Text = "Quote Marks";
			this.m_lblQuoteMarks.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// m_lblQuoteMarkReview
			// 
			this.m_lblQuoteMarkReview.AutoSize = true;
			this.m_lblQuoteMarkReview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarkReview, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_lblQuoteMarkReview, 4);
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarkReview, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblQuoteMarkReview.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarkReview, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarkReview, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblQuoteMarkReview, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarkReview, "DialogBoxes.ProjectSettingsDlg.label1");
			this.m_lblQuoteMarkReview.Location = new System.Drawing.Point(3, 230);
			this.m_lblQuoteMarkReview.Name = "m_lblQuoteMarkReview";
			this.m_lblQuoteMarkReview.Size = new System.Drawing.Size(67, 13);
			this.m_lblQuoteMarkReview.TabIndex = 20;
			this.m_lblQuoteMarkReview.Text = "Review Text";
			// 
			// m_lblQuoteMarkSummary
			// 
			this.m_lblQuoteMarkSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_lblQuoteMarkSummary.AutoSize = true;
			this.m_lblQuoteMarkSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblQuoteMarkSummary, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_lblQuoteMarkSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
			this.glyssenColorPalette.SetForeColor(this.m_lblQuoteMarkSummary, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblQuoteMarkSummary.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblQuoteMarkSummary, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblQuoteMarkSummary, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblQuoteMarkSummary, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblQuoteMarkSummary, "DialogBoxes.ProjectSettingsDlg.label1");
			this.m_lblQuoteMarkSummary.Location = new System.Drawing.Point(158, 243);
			this.m_lblQuoteMarkSummary.Name = "m_lblQuoteMarkSummary";
			this.m_lblQuoteMarkSummary.Size = new System.Drawing.Size(132, 29);
			this.m_lblQuoteMarkSummary.TabIndex = 22;
			this.m_lblQuoteMarkSummary.Text = "Summary Text";
			// 
			// m_btnQuoteMarkSettings
			// 
			this.m_btnQuoteMarkSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnQuoteMarkSettings.AutoSize = true;
			this.m_btnQuoteMarkSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_btnQuoteMarkSettings, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_btnQuoteMarkSettings, 2);
			this.glyssenColorPalette.SetForeColor(this.m_btnQuoteMarkSettings, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnQuoteMarkSettings, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnQuoteMarkSettings, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnQuoteMarkSettings, "DialogBoxes.ProjectSettingsDlg.ReviewChangeQuoteMarkSettings");
			this.m_btnQuoteMarkSettings.Location = new System.Drawing.Point(371, 246);
			this.m_btnQuoteMarkSettings.Name = "m_btnQuoteMarkSettings";
			this.m_btnQuoteMarkSettings.Size = new System.Drawing.Size(123, 23);
			this.m_btnQuoteMarkSettings.TabIndex = 21;
			this.m_btnQuoteMarkSettings.Text = "Quote Mark Settings...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnQuoteMarkSettings, false);
			this.m_btnQuoteMarkSettings.UseVisualStyleBackColor = true;
			this.m_btnQuoteMarkSettings.Click += new System.EventHandler(this.m_btnQuoteMarkSettings_Click);
			// 
			// m_txtVersification
			// 
			this.m_txtVersification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_txtVersification.AutoSize = true;
			this.m_txtVersification.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_txtVersification, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_txtVersification, 3);
			this.glyssenColorPalette.SetForeColor(this.m_txtVersification, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_txtVersification.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_txtVersification, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_txtVersification, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_txtVersification, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_txtVersification, "DialogBoxes.ProjectSettingsDlg.ProjectSettingsDlg.m_txtVersification");
			this.m_txtVersification.Location = new System.Drawing.Point(158, 167);
			this.m_txtVersification.Name = "m_txtVersification";
			this.m_txtVersification.Size = new System.Drawing.Size(336, 13);
			this.m_txtVersification.TabIndex = 27;
			this.m_txtVersification.Text = "#";
			// 
			// m_lblVersification
			// 
			this.m_lblVersification.AutoSize = true;
			this.m_lblVersification.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblVersification, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblVersification, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblVersification.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblVersification, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblVersification, null);
			this.m_l10NSharpExtender.SetLocalizationPriority(this.m_lblVersification, L10NSharp.LocalizationPriority.NotLocalizable);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblVersification, "DialogBoxes.ProjectSettingsDlg.Versification");
			this.m_lblVersification.Location = new System.Drawing.Point(3, 167);
			this.m_lblVersification.Name = "m_lblVersification";
			this.m_lblVersification.Size = new System.Drawing.Size(67, 13);
			this.m_lblVersification.TabIndex = 17;
			this.m_lblVersification.Text = "Versification:";
			// 
			// m_lblSummary
			// 
			this.m_lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.m_lblSummary.AutoSize = true;
			this.m_lblSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(73)))), ((int)(((byte)(108)))));
			this.glyssenColorPalette.SetBackColor(this.m_lblSummary, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_lblSummary, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_lblSummary.ForeColor = System.Drawing.Color.White;
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_lblSummary, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_lblSummary, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_lblSummary, "DialogBoxes.ProjectSettingsDlg.Summary");
			this.m_lblSummary.Location = new System.Drawing.Point(3, 243);
			this.m_lblSummary.Name = "m_lblSummary";
			this.m_lblSummary.Size = new System.Drawing.Size(53, 29);
			this.m_lblSummary.TabIndex = 23;
			this.m_lblSummary.Text = "Summary:";
			this.m_lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_btnUpdateFromBundle
			// 
			this.m_btnUpdateFromBundle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.m_btnUpdateFromBundle.AutoSize = true;
			this.glyssenColorPalette.SetBackColor(this.m_btnUpdateFromBundle, Glyssen.Utilities.GlyssenColors.BackColor);
			this.glyssenColorPalette.SetForeColor(this.m_btnUpdateFromBundle, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this.m_btnUpdateFromBundle, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this.m_btnUpdateFromBundle, null);
			this.m_l10NSharpExtender.SetLocalizingId(this.m_btnUpdateFromBundle, "DialogBoxes.ProjectSettingsDlg.Update");
			this.m_btnUpdateFromBundle.Location = new System.Drawing.Point(419, 44);
			this.m_btnUpdateFromBundle.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.m_btnUpdateFromBundle.Name = "m_btnUpdateFromBundle";
			this.m_btnUpdateFromBundle.Size = new System.Drawing.Size(75, 23);
			this.m_btnUpdateFromBundle.TabIndex = 30;
			this.m_btnUpdateFromBundle.Text = "Update...";
			this.glyssenColorPalette.SetUsePaletteColors(this.m_btnUpdateFromBundle, false);
			this.m_btnUpdateFromBundle.UseVisualStyleBackColor = true;
			this.m_btnUpdateFromBundle.Click += new System.EventHandler(this.m_btnUpdateFromBundle_Click);
			// 
			// m_tableLayoutMain
			// 
			this.m_tableLayoutMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.glyssenColorPalette.SetBackColor(this.m_tableLayoutMain, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.ColumnCount = 4;
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayoutMain.Controls.Add(this.m_txtRecordingProjectName, 1, 0);
			this.m_tableLayoutMain.Controls.Add(this.m_lblQuoteMarkSummary, 1, 14);
			this.m_tableLayoutMain.Controls.Add(this.m_lblQuoteMarkReview, 0, 13);
			this.m_tableLayoutMain.Controls.Add(this.m_lblSummary, 0, 14);
			this.m_tableLayoutMain.Controls.Add(this.m_lblRecordingProjectName, 0, 0);
			this.m_tableLayoutMain.Controls.Add(this.m_lblLanguageName, 0, 4);
			this.m_tableLayoutMain.Controls.Add(this.m_lblPublicationId, 0, 8);
			this.m_tableLayoutMain.Controls.Add(this.m_lblIso639_2_Code, 0, 5);
			this.m_tableLayoutMain.Controls.Add(this.m_lblPublicationName, 0, 7);
			this.m_tableLayoutMain.Controls.Add(this.m_wsFontControl, 0, 16);
			this.m_tableLayoutMain.Controls.Add(this.m_panelWritingSystemLabelAndSeparator, 0, 15);
			this.m_tableLayoutMain.Controls.Add(this.panel1, 0, 12);
			this.m_tableLayoutMain.Controls.Add(this.m_txtLanguageName, 1, 4);
			this.m_tableLayoutMain.Controls.Add(this.m_txtIso639_2_Code, 1, 5);
			this.m_tableLayoutMain.Controls.Add(this.m_txtPublicationName, 1, 7);
			this.m_tableLayoutMain.Controls.Add(this.m_txtPublicationId, 1, 8);
			this.m_tableLayoutMain.Controls.Add(this.m_lblOriginalBundlePath, 0, 2);
			this.m_tableLayoutMain.Controls.Add(this.m_txtOriginalBundlePath, 1, 2);
			this.m_tableLayoutMain.Controls.Add(this.m_lblVersification, 0, 11);
			this.m_tableLayoutMain.Controls.Add(this.m_txtVersification, 1, 11);
			this.m_tableLayoutMain.Controls.Add(this.m_btnUpdateFromBundle, 3, 2);
			this.m_tableLayoutMain.Controls.Add(this.m_btnQuoteMarkSettings, 2, 14);
			this.m_tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glyssenColorPalette.SetForeColor(this.m_tableLayoutMain, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayoutMain.Location = new System.Drawing.Point(15, 15);
			this.m_tableLayoutMain.Name = "m_tableLayoutMain";
			this.m_tableLayoutMain.RowCount = 17;
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.m_tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayoutMain.Size = new System.Drawing.Size(497, 524);
			this.m_tableLayoutMain.TabIndex = 0;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_tableLayoutMain, false);
			// 
			// m_panelWritingSystemLabelAndSeparator
			// 
			this.m_panelWritingSystemLabelAndSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.m_panelWritingSystemLabelAndSeparator, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.m_panelWritingSystemLabelAndSeparator, 4);
			this.m_panelWritingSystemLabelAndSeparator.Controls.Add(this.m_lblWritingSystem);
			this.m_panelWritingSystemLabelAndSeparator.Controls.Add(this.m_lblWsSeparator);
			this.glyssenColorPalette.SetForeColor(this.m_panelWritingSystemLabelAndSeparator, Glyssen.Utilities.GlyssenColors.Default);
			this.m_panelWritingSystemLabelAndSeparator.Location = new System.Drawing.Point(3, 275);
			this.m_panelWritingSystemLabelAndSeparator.Name = "m_panelWritingSystemLabelAndSeparator";
			this.m_panelWritingSystemLabelAndSeparator.Size = new System.Drawing.Size(491, 44);
			this.m_panelWritingSystemLabelAndSeparator.TabIndex = 15;
			this.glyssenColorPalette.SetUsePaletteColors(this.m_panelWritingSystemLabelAndSeparator, false);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.glyssenColorPalette.SetBackColor(this.panel1, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayoutMain.SetColumnSpan(this.panel1, 4);
			this.panel1.Controls.Add(this.m_lblQuoteMarks);
			this.panel1.Controls.Add(this.label2);
			this.glyssenColorPalette.SetForeColor(this.panel1, Glyssen.Utilities.GlyssenColors.Default);
			this.panel1.Location = new System.Drawing.Point(3, 183);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(491, 44);
			this.panel1.TabIndex = 19;
			this.glyssenColorPalette.SetUsePaletteColors(this.panel1, false);
			// 
			// ProjectSettingsDlg
			// 
			this.AcceptButton = this.m_btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(527, 594);
			this.Controls.Add(this.m_btnOk);
			this.Controls.Add(this.m_tableLayoutMain);
			this.Controls.Add(this.m_btnCancel);
			this.glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.m_l10NSharpExtender.SetLocalizableToolTip(this, null);
			this.m_l10NSharpExtender.SetLocalizationComment(this, null);
			this.m_l10NSharpExtender.SetLocalizingId(this, "DialogBoxes.ProjectSettingsDlg.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(543, 632);
			this.Name = "ProjectSettingsDlg";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 55);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Project Settings";
			((System.ComponentModel.ISupportInitialize)(this.m_l10NSharpExtender)).EndInit();
			this.m_tableLayoutMain.ResumeLayout(false);
			this.m_tableLayoutMain.PerformLayout();
			this.m_panelWritingSystemLabelAndSeparator.ResumeLayout(false);
			this.m_panelWritingSystemLabelAndSeparator.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.L10NSharpExtender m_l10NSharpExtender;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblLanguageName;
		private System.Windows.Forms.Label m_lblIso639_2_Code;
		private System.Windows.Forms.Label m_lblPublicationName;
		private System.Windows.Forms.Label m_lblPublicationId;
		private System.Windows.Forms.TableLayoutPanel m_tableLayoutMain;
		private SIL.Windows.Forms.WritingSystems.WSFontControl m_wsFontControl;
		private System.Windows.Forms.Button m_btnOk;
		private System.Windows.Forms.Label m_lblRecordingProjectName;
		private System.Windows.Forms.TextBox m_txtRecordingProjectName;
		private System.Windows.Forms.Panel m_panelWritingSystemLabelAndSeparator;
		private System.Windows.Forms.Label m_lblWritingSystem;
		private System.Windows.Forms.Label m_lblWsSeparator;
		private System.Windows.Forms.Label m_txtLanguageName;
		private System.Windows.Forms.Label m_txtIso639_2_Code;
		private System.Windows.Forms.Label m_txtPublicationName;
		private System.Windows.Forms.Label m_txtPublicationId;
		private System.Windows.Forms.Label m_lblOriginalBundlePath;
		private System.Windows.Forms.Label m_txtOriginalBundlePath;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label m_lblQuoteMarks;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label m_lblVersification;
		private System.Windows.Forms.Label m_txtVersification;
		private System.Windows.Forms.Button m_btnQuoteMarkSettings;
		private System.Windows.Forms.Label m_lblQuoteMarkSummary;
		private System.Windows.Forms.Label m_lblQuoteMarkReview;
		private System.Windows.Forms.Label m_lblSummary;
		private System.Windows.Forms.Button m_btnUpdateFromBundle;
		private Utilities.GlyssenColorPalette glyssenColorPalette;
	}
}